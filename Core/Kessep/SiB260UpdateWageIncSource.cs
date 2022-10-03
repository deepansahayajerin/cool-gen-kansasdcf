// Program: SI_B260_UPDATE_WAGE_INC_SOURCE, ID: 371790513, model: 746.
// Short name: SWEI260B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_B260_UPDATE_WAGE_INC_SOURCE.
/// </para>
/// <para>
/// This Procedure is for Updating Wage and Income source information on SI 
/// tables using an External file input from DHR
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB260UpdateWageIncSource: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B260_UPDATE_WAGE_INC_SOURCE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB260UpdateWageIncSource(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB260UpdateWageIncSource.
  /// </summary>
  public SiB260UpdateWageIncSource(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date	  Developer	Description
    // 06/??/96  Rao Mulpuri	Initial Development
    // 08/12/96  G. Lofton	Add unemployment ind to cse person
    // 10/08/96  G. Lofton	Added process to create online error
    // 			for multi employers for a Kansas id.
    // 05/02/97  Sid		Acceptance Test Fixes.
    // 08/20/97  JeHoward      Changed from generic to specific exit
    //                         states.
    // 1/5/98	  Siraj Konkader   Removed persistent views.
    // ------------------------------------------------------------
    // ****************************************************************
    // 11/12/1998  C. Ott   Added logic to refresh the UNEMPLOYMENT_IND 
    // attribute on CSE_PERSON, the first step in the batch run will reset the
    // indicator to "N" for all Persons.  The procedure will set it to "Y" when
    // "UI" record types are processed.
    // ***************************************************************
    // ****************************************************************
    // 01/01/01  Ed Lyman -  WR # 280  Remove logic for quarterly wage
    // reporting.  This will be handled using FCR information.  Enhance
    // Unemployment reporting by sending alerts only when an AP goes on
    // or goes off of unemployment.
    // ****************************************************************
    // ***********************************************************************
    // Restart considerations.  Should be able to rerun job from any point in
    // the run.  Control totals will not be preserved.
    // ***********************************************************************
    // ***********************************************************************
    // The UNEMPLOYMENT_IND attribute is on CSE_PERSON.
    // The first step in the batch run will reset the indicator to "A" for all
    // Persons who have the indicator currently set to "Y".
    // The second step will set indicator to "Y" when UI record types are
    // processed. An alert will be sent if the person has just started receiving
    // unemployment (indicator not equal to "A").
    // The third step will read all persons who have ceased getting unemployment
    // (indicators that still equal "A").  An alert will be sent and the
    // indicator will be reset to "N".
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseSiB260Housekeeping();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test;
      }

      local.Hardcode.EventId = 10;
      local.Hardcode.UserId = "INCS";
      local.Hardcode.ReferenceDate = local.ProgramProcessingInfo.ProcessDate;
      local.Hardcode.BusinessObjectCd = "ICS";

      // ***********************************************************************
      // Step One.
      // ***********************************************************************
      foreach(var item in ReadCsePerson3())
      {
        try
        {
          UpdateCsePerson1();
          ++local.NumberPreviouslyOnUi.Count;
          ++local.NumberUpdatesForCommit.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error while updating CSE Person with A.  Person not unique.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto Test;
            case ErrorCode.PermittedValueViolation:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error while updating CSE Person with A.  Permitted value error.";
                
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto Test;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (local.NumberUpdatesForCommit.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.NumberUpdatesForCommit.Count = 0;
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "ERROR ENCOUNTERED DURING DATABASE COMMIT.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto Test;
          }
        }
      }

      local.NumberUpdatesForCommit.Count = 0;
      UseExtToDoACommit();

      // ***********************************************************************
      // Step Two.
      // ***********************************************************************
      do
      {
        // *****************************************************
        // *This loop processes until end of input file reached.
        // *****************************************************
        do
        {
          // ******************************************************
          // This loop processes until it is time to commit.
          // ******************************************************
          local.PassArea.FileInstruction = "READ";
          UseSiEabReceiveWageIncSource();

          switch(TrimEnd(local.PassArea.TextReturnCode))
          {
            case "":
              // *****  Found a record.  Continue processing.
              ++local.RecordsRead.Count;

              if (AsChar(local.SiWageIncomeSourceRec.CseIndicator) != 'C')
              {
                ++local.RecordsSkipped.Count;

                continue;
              }

              break;
            case "EF":
              // *****  End of file.
              goto AfterCycle;
            default:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "ERROR READING WAGE - INCOME SOURCE FILE.";
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto Test;
          }

          // ******************************************************
          // BW = Basic Wages
          // UI = Unemployment Income
          // ******************************************************
          switch(TrimEnd(local.SiWageIncomeSourceRec.RecordTypeIndicator))
          {
            case "BW":
              ++local.RecordsSkipped.Count;

              continue;
            case "NH":
              ++local.RecordsSkipped.Count;

              continue;
            case "UI":
              ++local.UnemplIncomeRead.Count;

              // ******************************************************************
              // The Input file is sorted by Person Number and may have more 
              // than
              // one record for the same person.  The following line stores the
              // number of the previously read record so that the processing can
              // determine whether this is a subsequent record for the same 
              // person.
              // ******************************************************************
              local.Previous.Number = local.CsePerson.Number;

              if (!Equal(local.SiWageIncomeSourceRec.PersonNumber,
                local.Previous.Number))
              {
                local.Previous.Number =
                  local.SiWageIncomeSourceRec.PersonNumber;

                if (ReadCsePerson1())
                {
                  local.CsePerson.Assign(entities.CsePerson);

                  // ******************************************************************
                  // Unemployment Income is usually reported weekly.
                  // We are not interested in how much the person receives,
                  // only the fact that they are receiving unemployment.
                  // ******************************************************************
                  if (AsChar(local.CsePerson.UnemploymentInd) != 'A')
                  {
                    ++local.NumberAddedToUi.Count;

                    // ******************************************************************
                    // Alert worker that person just started getting ui.
                    // ******************************************************************
                    MoveInfrastructure3(local.Hardcode, local.Infrastructure);
                    local.Infrastructure.Detail =
                      "Has begun receiving unemployment.";
                    local.Infrastructure.ReasonCode = "DHR_INCRUI";
                    local.Infrastructure.SituationNumber = 0;
                    UseOeCabRaiseEvent();

                    if (!IsExitState("ACO_NN0000_ALL_OK"))
                    {
                      local.EabFileHandling.Action = "WRITE";
                      local.EabReportSend.RptDetail =
                        "ERROR ENCOUNTERED DURING RAISE EVENT FOR UPDATE OF CSE PERSON = " +
                        local.CsePerson.Number;
                      UseCabErrorReport();
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      goto Test;
                    }
                  }
                  else
                  {
                    ++local.NumberContinuedOnUi.Count;
                  }

                  try
                  {
                    UpdateCsePerson3();
                    ++local.PersonsNowOnUi.Count;
                    ++local.NumberUpdatesForCommit.Count;
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "NOT UNIQUE DATABASE ERROR IN CSE PERSON FOR " + entities
                          .CsePerson.Number;
                        UseCabErrorReport();
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        goto Test;
                      case ErrorCode.PermittedValueViolation:
                        local.EabFileHandling.Action = "WRITE";
                        local.EabReportSend.RptDetail =
                          "PERMITTED VALUE DATABASE ERROR IN CSE PERSON FOR " +
                          entities.CsePerson.Number;
                        UseCabErrorReport();
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        goto Test;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }
                }
                else
                {
                  ++local.PersonsNotFound.Count;

                  continue;
                }
              }
              else
              {
                continue;
              }

              break;
            default:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Invalid Record Type Indicator = '" + local
                .SiWageIncomeSourceRec.RecordTypeIndicator + "' for CSE Person = " +
                local.CsePerson.Number;
              UseCabErrorReport();

              if (Equal(local.EabFileHandling.Status, "OK"))
              {
              }
              else
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                goto AfterCycle;
              }

              break;
          }
        }
        while(local.NumberUpdatesForCommit.Count < local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault());

        local.NumberUpdatesForCommit.Count = 0;
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "ERROR ENCOUNTERED DURING DATABASE COMMIT.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto Test;
        }
      }
      while(!Equal(local.PassArea.TextReturnCode, "EF"));

AfterCycle:

      // ***********************************************************************
      // Step Three.
      // ***********************************************************************
      foreach(var item in ReadCsePerson2())
      {
        try
        {
          UpdateCsePerson2();
          local.CsePerson.Assign(entities.CsePerson);
          ++local.NumberDiscontinued.Count;
          ++local.NumberUpdatesForCommit.Count;

          // ******************************************************************
          // Alert worker that person is no longer getting ui.
          // ******************************************************************
          MoveInfrastructure3(local.Hardcode, local.Infrastructure);
          local.Infrastructure.Detail = "No longer receiving unemployment.";
          local.Infrastructure.ReasonCode = "DHR_INCRUI";
          local.Infrastructure.SituationNumber = 0;
          UseOeCabRaiseEvent();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "ERROR ENCOUNTERED DURING RAISE EVENT FOR UPDATE OF CSE PERSON = " +
              local.CsePerson.Number;
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto Test;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "NOT UNIQUE DATABASE ERROR IN CSE PERSON FOR " + entities
                .CsePerson.Number;
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto Test;
            case ErrorCode.PermittedValueViolation:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "PERMITTED VALUE DATABASE ERROR IN CSE PERSON FOR " + entities
                .CsePerson.Number;
              UseCabErrorReport();
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto Test;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (local.NumberUpdatesForCommit.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.NumberUpdatesForCommit.Count = 0;
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "ERROR ENCOUNTERED DURING DATABASE COMMIT.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto Test;
          }
        }
      }
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseExtToDoACommit();
      UseSiB260Close();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
      }
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseSiB260Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseOeCabRaiseEvent()
  {
    var useImport = new OeCabRaiseEvent.Import();
    var useExport = new OeCabRaiseEvent.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeCabRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiB260Close()
  {
    var useImport = new SiB260Close.Import();
    var useExport = new SiB260Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.RecordsSkipped.Count = local.RecordsSkipped.Count;
    useImport.UnemployIncomeRead.Count = local.UnemplIncomeRead.Count;
    useImport.PersonsPreviouslyOnUi.Count = local.NumberPreviouslyOnUi.Count;
    useImport.PersonsDiscontinued.Count = local.NumberDiscontinued.Count;
    useImport.PersonsContinued.Count = local.NumberContinuedOnUi.Count;
    useImport.PersonsAddedToUi.Count = local.NumberAddedToUi.Count;
    useImport.PersonsNowOnUi.Count = local.PersonsNowOnUi.Count;

    Call(SiB260Close.Execute, useImport, useExport);
  }

  private void UseSiB260Housekeeping()
  {
    var useImport = new SiB260Housekeeping.Import();
    var useExport = new SiB260Housekeeping.Export();

    Call(SiB260Housekeeping.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseSiEabReceiveWageIncSource()
  {
    var useImport = new SiEabReceiveWageIncSource.Import();
    var useExport = new SiEabReceiveWageIncSource.Export();

    useImport.External.Assign(local.PassArea);
    useExport.SiWageIncomeSourceRec.Assign(local.SiWageIncomeSourceRec);
    useExport.External.Assign(local.PassArea);

    Call(SiEabReceiveWageIncSource.Execute, useImport, useExport);

    local.SiWageIncomeSourceRec.Assign(useExport.SiWageIncomeSourceRec);
    MoveExternal(useExport.External, local.PassArea);
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", local.SiWageIncomeSourceRec.PersonNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson2",
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      null,
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.CsePerson.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private void UpdateCsePerson1()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var unemploymentInd = "A";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson1",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.UnemploymentInd = unemploymentInd;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson2()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var unemploymentInd = "N";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson2",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.UnemploymentInd = unemploymentInd;
    entities.CsePerson.Populated = true;
  }

  private void UpdateCsePerson3()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var unemploymentInd = "Y";

    entities.CsePerson.Populated = false;
    Update("UpdateCsePerson3",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "unemploymentInd", unemploymentInd);
        db.SetString(command, "numb", entities.CsePerson.Number);
      });

    entities.CsePerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePerson.LastUpdatedBy = lastUpdatedBy;
    entities.CsePerson.UnemploymentInd = unemploymentInd;
    entities.CsePerson.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkipped.
    /// </summary>
    [JsonPropertyName("recordsSkipped")]
    public Common RecordsSkipped
    {
      get => recordsSkipped ??= new();
      set => recordsSkipped = value;
    }

    /// <summary>
    /// A value of UnemplIncomeRead.
    /// </summary>
    [JsonPropertyName("unemplIncomeRead")]
    public Common UnemplIncomeRead
    {
      get => unemplIncomeRead ??= new();
      set => unemplIncomeRead = value;
    }

    /// <summary>
    /// A value of NumberPreviouslyOnUi.
    /// </summary>
    [JsonPropertyName("numberPreviouslyOnUi")]
    public Common NumberPreviouslyOnUi
    {
      get => numberPreviouslyOnUi ??= new();
      set => numberPreviouslyOnUi = value;
    }

    /// <summary>
    /// A value of NumberDiscontinued.
    /// </summary>
    [JsonPropertyName("numberDiscontinued")]
    public Common NumberDiscontinued
    {
      get => numberDiscontinued ??= new();
      set => numberDiscontinued = value;
    }

    /// <summary>
    /// A value of NumberContinuedOnUi.
    /// </summary>
    [JsonPropertyName("numberContinuedOnUi")]
    public Common NumberContinuedOnUi
    {
      get => numberContinuedOnUi ??= new();
      set => numberContinuedOnUi = value;
    }

    /// <summary>
    /// A value of NumberAddedToUi.
    /// </summary>
    [JsonPropertyName("numberAddedToUi")]
    public Common NumberAddedToUi
    {
      get => numberAddedToUi ??= new();
      set => numberAddedToUi = value;
    }

    /// <summary>
    /// A value of PersonsNowOnUi.
    /// </summary>
    [JsonPropertyName("personsNowOnUi")]
    public Common PersonsNowOnUi
    {
      get => personsNowOnUi ??= new();
      set => personsNowOnUi = value;
    }

    /// <summary>
    /// A value of NumberUpdatesForCommit.
    /// </summary>
    [JsonPropertyName("numberUpdatesForCommit")]
    public Common NumberUpdatesForCommit
    {
      get => numberUpdatesForCommit ??= new();
      set => numberUpdatesForCommit = value;
    }

    /// <summary>
    /// A value of PersonsNotFound.
    /// </summary>
    [JsonPropertyName("personsNotFound")]
    public Common PersonsNotFound
    {
      get => personsNotFound ??= new();
      set => personsNotFound = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of Hardcode.
    /// </summary>
    [JsonPropertyName("hardcode")]
    public Infrastructure Hardcode
    {
      get => hardcode ??= new();
      set => hardcode = value;
    }

    private Common recordsRead;
    private Common recordsSkipped;
    private Common unemplIncomeRead;
    private Common numberPreviouslyOnUi;
    private Common numberDiscontinued;
    private Common numberContinuedOnUi;
    private Common numberAddedToUi;
    private Common personsNowOnUi;
    private Common numberUpdatesForCommit;
    private Common personsNotFound;
    private CsePerson previous;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Infrastructure infrastructure;
    private CsePerson csePerson;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea textWorkArea;
    private Infrastructure hardcode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }
#endregion
}

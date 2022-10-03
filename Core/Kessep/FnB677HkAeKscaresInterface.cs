// Program: FN_B677_HK_AE_KSCARES_INTERFACE, ID: 371258987, model: 746.
// Short name: SWEF677B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B677_HK_AE_KSCARES_INTERFACE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB677HkAeKscaresInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B677_HK_AE_KSCARES_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB677HkAeKscaresInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB677HkAeKscaresInterface.
  /// </summary>
  public FnB677HkAeKscaresInterface(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 09/09/05  GVandy	WR00256682	Initial Development.
    // 11/29/05  GVandy	PR00260353	Add effective and end dates to Hurricane 
    // Katrina displaced_person records.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program provides an interface to AE and KSCares for the purpose 
    // of identifying clients impacted by Hurricane Katrina.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Read.Action = "READ";
    local.Write.Action = "WRITE";
    local.Close.Action = "CLOSE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB677BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Set the interface indicator to 'X' for all clients previously 
    // identified as HK via the interface.
    // --  The indicator will be reset back to 'Y' as the interface files from 
    // AE and KS Cares are processed.
    // --  Any records remaining with an 'X' after the interface files are 
    // processed are those that are no longer HK in the other systems.
    // --  The records still set to 'X' will be end dated at the end of this 
    // program.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadDisplacedPerson3())
    {
      try
      {
        UpdateDisplacedPerson2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISPLACED_PERSON_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DISPLACED_PERSON_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++local.Commit.Count;

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "(00) Error resetting Interface Indicator for Displaced Person " + entities
          .DisplacedPerson.Number + "..." + local.ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      if (local.Commit.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Checkpoint.
        // -------------------------------------------------------------------------------------------------------------------------
        local.Commit.Count = 0;
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "(03) Error in External Commit Routine.  Return Code = " + NumberToString
            (local.External.NumericReturnCode, 14, 2);
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Commit changes.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Commit.Count = 0;
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabReportSend.RptDetail =
        "(04) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    for(local.File.Count = 1; local.File.Count <= 2; ++local.File.Count)
    {
      switch(local.File.Count)
      {
        case 1:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  AE Hurricane Katrina Interface File.
          // -------------------------------------------------------------------------------------------------------------------------
          local.TextWorkArea.Text8 = "AE";
          local.EabReportSend.RptDetail = "Processing AE HK Interface File";

          break;
        case 2:
          // -------------------------------------------------------------------------------------------------------------------------
          // --  KSCares Hurricane Katrina Interface File.
          // -------------------------------------------------------------------------------------------------------------------------
          local.TextWorkArea.Text8 = "KSCares";
          local.EabReportSend.RptDetail =
            "Processing KS Cares HK Interface File";

          break;
        default:
          break;
      }

      for(local.Counter.Count = 1; local.Counter.Count <= 2; ++
        local.Counter.Count)
      {
        if (local.Counter.Count == 1)
        {
        }
        else
        {
          local.EabReportSend.RptDetail = "";
        }

        UseCabControlReport1();

        if (!Equal(local.Write.Status, "OK"))
        {
          // -- Write to the error report.
          local.Write.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(04) Error Writing Control Report...  Returned Status = " + local
            .Write.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.ReadCount.Count = 0;
      local.Commit.Count = 0;
      local.NumOfCseClients.Count = 0;
      local.NumOfDpUpdated.Count = 0;
      local.NumOfDpCreated.Count = 0;

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Process Interface File.
      // -------------------------------------------------------------------------------------------------------------------------
      do
      {
        // -------------------------------------------------------------------------------------------------------------------------
        // --  Get a record from the interface file.
        // -------------------------------------------------------------------------------------------------------------------------
        switch(local.File.Count)
        {
          case 1:
            // -------------------------------------------------------------------------------------------------------------------------
            // --  AE Hurricane Katrina Interface File.
            // -------------------------------------------------------------------------------------------------------------------------
            UseFnB677ReadAeInterface1();

            break;
          case 2:
            // -------------------------------------------------------------------------------------------------------------------------
            // --  KSCares Hurricane Katrina Interface File.
            // -------------------------------------------------------------------------------------------------------------------------
            UseFnB677ReadKscaresInterface1();

            break;
          default:
            break;
        }

        switch(TrimEnd(local.Read.Status))
        {
          case "EF":
            continue;
          case "OK":
            // -- Continue.
            break;
          default:
            // --  write to error file...
            local.EabReportSend.RptDetail = "(01) Error reading " + TrimEnd
              (local.TextWorkArea.Text8) + " file...  Returned Status = " + local
              .Read.Status;
            UseCabErrorReport1();
            ExitState = "ERROR_READING_FILE_AB";

            return;
        }

        ++local.ReadCount.Count;

        if (ReadCsePerson())
        {
          ++local.NumOfCseClients.Count;

          if (ReadDisplacedPerson1())
          {
            local.EabReportSend.RptDetail = "Person " + local
              .DisplacedPerson.Number + " - Previously identified as HK";

            if (AsChar(entities.DisplacedPerson.DisplacedInd) != 'Y' || AsChar
              (entities.DisplacedPerson.DisplacedInterfaceInd) != 'Y')
            {
              try
              {
                UpdateDisplacedPerson1();
                ++local.NumOfDpUpdated.Count;
                ++local.Commit.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "DISPLACED_PERSON_NU";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "DISPLACED_PERSON_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else
          {
            local.EabReportSend.RptDetail = "Person " + local
              .DisplacedPerson.Number + " - New HK individual";
            local.MaxDate.Date = new DateTime(2099, 12, 31);

            try
            {
              CreateDisplacedPerson();
              ++local.NumOfDpCreated.Count;
              ++local.Commit.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  if (ReadDisplacedPerson2())
                  {
                    local.EabReportSend.RptDetail = "Person " + local
                      .DisplacedPerson.Number + " - New HK individual";

                    try
                    {
                      UpdateDisplacedPerson4();
                      ++local.NumOfDpCreated.Count;
                      ++local.Commit.Count;
                    }
                    catch(Exception e1)
                    {
                      switch(GetErrorCode(e1))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "DISPLACED_PERSON_NU";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "DISPLACED_PERSON_PV";

                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  else
                  {
                    ExitState = "DISPLACED_PERSON_NF";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "DISPLACED_PERSON_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }
        else
        {
          // -- Person is not a CSE client.  Skip the person.
          local.EabReportSend.RptDetail = "Person " + local
            .DisplacedPerson.Number + " - Not a CSE Client";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "(02) Error processing " + TrimEnd
            (local.TextWorkArea.Text8) + " Displaced Person " + local
            .DisplacedPerson.Number + "..." + local.ExitStateWorkArea.Message;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        UseCabControlReport1();

        if (!Equal(local.Write.Status, "OK"))
        {
          // -- Write to the error report.
          local.Write.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(04) Error Writing Control Report...  Returned Status = " + local
            .Write.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (local.Commit.Count > local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Checkpoint.
          // -------------------------------------------------------------------------------------------------------------------------
          local.Commit.Count = 0;
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "(03) Error in External Commit Routine.  Return Code = " + NumberToString
              (local.External.NumericReturnCode, 14, 2);
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }
      while(Equal(local.Read.Status, "OK"));

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Write Totals to the Control Report.
      // -------------------------------------------------------------------------------------------------------------------------
      for(local.Counter.Count = 1; local.Counter.Count <= 9; ++
        local.Counter.Count)
      {
        switch(local.Counter.Count)
        {
          case 3:
            local.EabReportSend.RptDetail =
              TrimEnd(local.TextWorkArea.Text8) + " Interface Statistics";

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "  -- Total Number of Interface Records    " + NumberToString
              (local.ReadCount.Count, 10, 6);

            break;
          case 5:
            local.EabReportSend.RptDetail =
              "  -- Total Number of CSE Clients          " + NumberToString
              (local.NumOfCseClients.Count, 10, 6);

            break;
          case 6:
            local.EabReportSend.RptDetail =
              "  -- Number of Displaced Persons Created  " + NumberToString
              (local.NumOfDpCreated.Count, 10, 6);

            break;
          case 7:
            local.EabReportSend.RptDetail =
              "  -- Number of Displaced Persons Updated  " + NumberToString
              (local.NumOfDpUpdated.Count, 10, 6);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        UseCabControlReport1();

        if (!Equal(local.Write.Status, "OK"))
        {
          // -- Write to the error report.
          local.Write.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(04) Error Writing Control Report...  Returned Status = " + local
            .Write.Status;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // --  Commit changes.
      // -------------------------------------------------------------------------------------------------------------------------
      local.Commit.Count = 0;
      UseExtToDoACommit();

      if (local.External.NumericReturnCode != 0)
      {
        local.EabReportSend.RptDetail =
          "(04) Error in External Commit Routine.  Return Code = " + NumberToString
          (local.External.NumericReturnCode, 14, 2);
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Delete Displaced_Person for all clients no longer HK in EES systems.
    // -------------------------------------------------------------------------------------------------------------------------
    foreach(var item in ReadDisplacedPerson4())
    {
      local.EabReportSend.RptDetail = "Person " + entities
        .DisplacedPerson.Number + " - No longer designated HK";

      try
      {
        UpdateDisplacedPerson3();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISPLACED_PERSON_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DISPLACED_PERSON_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "(12) Error processing " + TrimEnd
          (local.TextWorkArea.Text8) + " Displaced Person " + local
          .DisplacedPerson.Number + "..." + local.ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.Write.Status, "OK"))
      {
        // -- Write to the error report.
        local.Write.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(04) Error Writing Control Report...  Returned Status = " + local
          .Write.Status;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Commit changes.
    // -------------------------------------------------------------------------------------------------------------------------
    local.Commit.Count = 0;
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabReportSend.RptDetail =
        "(04) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the AE File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB677ReadAeInterface2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing AE File...  Returned Status = " + local.Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the KSCares File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB677ReadKscaresInterface2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing KSCares File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.Close.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB677BatchInitialization()
  {
    var useImport = new FnB677BatchInitialization.Import();
    var useExport = new FnB677BatchInitialization.Export();

    Call(FnB677BatchInitialization.Execute, useImport, useExport);

    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseFnB677ReadAeInterface1()
  {
    var useImport = new FnB677ReadAeInterface.Import();
    var useExport = new FnB677ReadAeInterface.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.DisplacedPerson.Number = local.DisplacedPerson.Number;
    useExport.EabFileHandling.Status = local.Read.Status;

    Call(FnB677ReadAeInterface.Execute, useImport, useExport);

    local.DisplacedPerson.Number = useExport.DisplacedPerson.Number;
    local.Read.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB677ReadAeInterface2()
  {
    var useImport = new FnB677ReadAeInterface.Import();
    var useExport = new FnB677ReadAeInterface.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.EabFileHandling.Status = local.Close.Status;

    Call(FnB677ReadAeInterface.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB677ReadKscaresInterface1()
  {
    var useImport = new FnB677ReadKscaresInterface.Import();
    var useExport = new FnB677ReadKscaresInterface.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.DisplacedPerson.Number = local.DisplacedPerson.Number;
    useExport.EabFileHandling.Status = local.Read.Status;

    Call(FnB677ReadKscaresInterface.Execute, useImport, useExport);

    local.DisplacedPerson.Number = useExport.DisplacedPerson.Number;
    local.Read.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB677ReadKscaresInterface2()
  {
    var useImport = new FnB677ReadKscaresInterface.Import();
    var useExport = new FnB677ReadKscaresInterface.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.EabFileHandling.Status = local.Close.Status;

    Call(FnB677ReadKscaresInterface.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void CreateDisplacedPerson()
  {
    var number = local.DisplacedPerson.Number;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
    var endDate = local.MaxDate.Date;
    var displacedInd = "Y";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.DisplacedPerson.Populated = false;
    Update("CreateDisplacedPerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", number);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "displacedInd", displacedInd);
        db.SetNullableString(command, "displacedIntInd", displacedInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
      });

    entities.DisplacedPerson.Number = number;
    entities.DisplacedPerson.EffectiveDate = effectiveDate;
    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.DisplacedInd = displacedInd;
    entities.DisplacedPerson.DisplacedInterfaceInd = displacedInd;
    entities.DisplacedPerson.CreatedBy = createdBy;
    entities.DisplacedPerson.CreatedTimestamp = createdTimestamp;
    entities.DisplacedPerson.LastUpdatedTimestamp = null;
    entities.DisplacedPerson.LastUpdatedBy = "";
    entities.DisplacedPerson.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.DisplacedPerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadDisplacedPerson1()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;
      });
  }

  private bool ReadDisplacedPerson2()
  {
    entities.DisplacedPerson.Populated = false;

    return Read("ReadDisplacedPerson2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDisplacedPerson3()
  {
    entities.DisplacedPerson.Populated = false;

    return ReadEach("ReadDisplacedPerson3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisplacedPerson4()
  {
    entities.DisplacedPerson.Populated = false;

    return ReadEach("ReadDisplacedPerson4",
      null,
      (db, reader) =>
      {
        entities.DisplacedPerson.Number = db.GetString(reader, 0);
        entities.DisplacedPerson.EffectiveDate = db.GetDate(reader, 1);
        entities.DisplacedPerson.EndDate = db.GetNullableDate(reader, 2);
        entities.DisplacedPerson.DisplacedInd = db.GetNullableString(reader, 3);
        entities.DisplacedPerson.DisplacedInterfaceInd =
          db.GetNullableString(reader, 4);
        entities.DisplacedPerson.CreatedBy = db.GetString(reader, 5);
        entities.DisplacedPerson.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.DisplacedPerson.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.DisplacedPerson.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisplacedPerson.Populated = true;

        return true;
      });
  }

  private void UpdateDisplacedPerson1()
  {
    var displacedInd = "Y";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "displacedInd", displacedInd);
        db.SetNullableString(command, "displacedIntInd", displacedInd);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.DisplacedInd = displacedInd;
    entities.DisplacedPerson.DisplacedInterfaceInd = displacedInd;
    entities.DisplacedPerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DisplacedPerson.LastUpdatedBy = lastUpdatedBy;
    entities.DisplacedPerson.Populated = true;
  }

  private void UpdateDisplacedPerson2()
  {
    var displacedInterfaceInd = "X";

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "displacedIntInd", displacedInterfaceInd);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.DisplacedInterfaceInd = displacedInterfaceInd;
    entities.DisplacedPerson.Populated = true;
  }

  private void UpdateDisplacedPerson3()
  {
    var endDate = local.ProgramProcessingInfo.ProcessDate;
    var displacedInterfaceInd = "Y";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson3",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "displacedIntInd", displacedInterfaceInd);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.DisplacedInterfaceInd = displacedInterfaceInd;
    entities.DisplacedPerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DisplacedPerson.LastUpdatedBy = lastUpdatedBy;
    entities.DisplacedPerson.Populated = true;
  }

  private void UpdateDisplacedPerson4()
  {
    var endDate = local.MaxDate.Date;
    var displacedInd = "Y";
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.DisplacedPerson.Populated = false;
    Update("UpdateDisplacedPerson4",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "displacedInd", displacedInd);
        db.SetNullableString(command, "displacedIntInd", displacedInd);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "cspNumber", entities.DisplacedPerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisplacedPerson.EffectiveDate.GetValueOrDefault());
      });

    entities.DisplacedPerson.EndDate = endDate;
    entities.DisplacedPerson.DisplacedInd = displacedInd;
    entities.DisplacedPerson.DisplacedInterfaceInd = displacedInd;
    entities.DisplacedPerson.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.DisplacedPerson.LastUpdatedBy = lastUpdatedBy;
    entities.DisplacedPerson.Populated = true;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of File.
    /// </summary>
    [JsonPropertyName("file")]
    public Common File
    {
      get => file ??= new();
      set => file = value;
    }

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EabFileHandling Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
    }

    /// <summary>
    /// A value of NumOfDpCreated.
    /// </summary>
    [JsonPropertyName("numOfDpCreated")]
    public Common NumOfDpCreated
    {
      get => numOfDpCreated ??= new();
      set => numOfDpCreated = value;
    }

    /// <summary>
    /// A value of NumOfDpUpdated.
    /// </summary>
    [JsonPropertyName("numOfDpUpdated")]
    public Common NumOfDpUpdated
    {
      get => numOfDpUpdated ??= new();
      set => numOfDpUpdated = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of NumOfCseClients.
    /// </summary>
    [JsonPropertyName("numOfCseClients")]
    public Common NumOfCseClients
    {
      get => numOfCseClients ??= new();
      set => numOfCseClients = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
    }

    private DateWorkArea maxDate;
    private TextWorkArea textWorkArea;
    private Common file;
    private DisplacedPerson displacedPerson;
    private EabFileHandling read;
    private EabFileHandling write;
    private EabFileHandling close;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private Common readCount;
    private Common numOfDpCreated;
    private Common numOfDpUpdated;
    private Common commit;
    private Common numOfCseClients;
    private External external;
    private Common counter;
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

    /// <summary>
    /// A value of DisplacedPerson.
    /// </summary>
    [JsonPropertyName("displacedPerson")]
    public DisplacedPerson DisplacedPerson
    {
      get => displacedPerson ??= new();
      set => displacedPerson = value;
    }

    private CsePerson csePerson;
    private DisplacedPerson displacedPerson;
  }
#endregion
}

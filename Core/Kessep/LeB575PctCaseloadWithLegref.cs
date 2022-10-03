// Program: LE_B575_PCT_CASELOAD_WITH_LEGREF, ID: 371410651, model: 746.
// Short name: SWEL575B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B575_PCT_CASELOAD_WITH_LEGREF.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB575PctCaseloadWithLegref: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B575_PCT_CASELOAD_WITH_LEGREF program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB575PctCaseloadWithLegref(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB575PctCaseloadWithLegref.
  /// </summary>
  public LeB575PctCaseloadWithLegref(IContext context, Import import,
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
    // ------------------------------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		        Description
    // 04-20-2009  SWDPLSS - Linda Smith       Initial Development
    // 08/26/2011  GVandy                      CQ29124  Add reason_code = 'RC' 
    // when reading
    //                                         
    // for regional office.
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** Get the run parameters for this program.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
      // : The reporting end of the month is the last day of the prior 
      // processing month.
      local.ReportingEom.Date =
        AddDays(local.ProgramProcessingInfo.ProcessDate, -
        Day(local.ProgramProcessingInfo.ProcessDate));
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        // Extract restart info here, if needed
        local.RestartOffice.SystemGeneratedId =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 1, 4));
        local.RestartSp.SystemGeneratedId =
          (int)StringToNumber(Substring(
            local.ProgramCheckpointRestart.RestartInfo, 250, 6, 5));
      }
    }
    else
    {
      // : Abort exitstate already set.
      return;
    }

    // *****************************************************************
    // * Setup of batch error handling
    // 
    // *
    // *****************************************************************
    // *****************************************************************
    // * Open the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = local.ReportingEom.Date;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * End of Batch error handling setup                             *
    // *****************************************************************
    // : Open Control Report file
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Open the output file
    local.EabFileHandling.Action = "OPEN";
    UseLeEabWriteB575FileExtract3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the Extract File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *********** Main Processing Logic Starts Here **********
    foreach(var item in ReadOffice())
    {
      local.Co.SystemGeneratedId = entities.Office.SystemGeneratedId;

      // 08/26/11  GVandy CQ29124  Add reason_code = 'RC' when reading for 
      // regional office.
      if (ReadCseOrganization())
      {
        local.CoRegion.Code = entities.Region.Code;

        if (Lt(entities.Office.DiscontinueDate, local.ReportingEom.Date))
        {
          local.CoRegion.Name = "";
          local.Co.Name = "";
        }
        else
        {
          local.CoRegion.Name = entities.Region.Name;
          local.Co.Name = entities.Office.Name;
        }
      }
      else
      {
        local.CoRegion.Code = "";
        local.CoRegion.Name = "";
        local.Co.Name = "";
      }

      foreach(var item1 in ReadServiceProvider())
      {
        local.ActiveCoCaseRefCnt.Count = 0;
        local.ActiveCoCaseloadCnt.Count = 0;
        local.InactiveCoCaseRefCnt.Count = 0;
        local.InactiveCoCaseloadCnt.Count = 0;
        local.FileSpInfo.Assign(entities.ServiceProvider);

        foreach(var item2 in ReadCaseOfficeServiceProvider())
        {
          if (Lt(entities.OfficeServiceProvider.DiscontinueDate,
            local.ReportingEom.Date))
          {
            local.ActiveServiceProvider.Flag = "N";
          }
          else
          {
            local.ActiveServiceProvider.Flag = "Y";
          }

          if (ReadLegalReferral())
          {
            if (AsChar(local.ActiveServiceProvider.Flag) == 'Y')
            {
              ++local.ActiveCoCaseRefCnt.Count;
            }
            else
            {
              ++local.InactiveCoCaseRefCnt.Count;
            }

            ++local.TotalLegalRefReadCnt.Count;
          }

          if (AsChar(local.ActiveServiceProvider.Flag) == 'Y')
          {
            ++local.ActiveCoCaseloadCnt.Count;
          }
          else
          {
            ++local.InactiveCoCaseloadCnt.Count;
          }

          ++local.TotalCasesReadCnt.Count;
        }

        if (local.ActiveCoCaseloadCnt.Count > 0 || local
          .InactiveCoCaseloadCnt.Count > 0)
        {
          if (local.ActiveCoCaseloadCnt.Count > 0)
          {
            // :  Write to sequential file
            local.EabFileHandling.Action = "WRITE";
            MoveCseOrganization(local.CoRegion, local.File);
            MoveOffice(local.Co, local.FileOfficeInfo);
            UseLeEabWriteB575FileExtract1();
          }

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while writing to the Extract File.";
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }

          if (local.InactiveCoCaseloadCnt.Count > 0)
          {
            // :  Write to sequential file
            local.EabFileHandling.Action = "WRITE";
            MoveCseOrganization(local.CoRegion, local.File);
            local.File.Name = "";
            MoveOffice(local.Co, local.FileOfficeInfo);
            local.FileOfficeInfo.Name = "";
            UseLeEabWriteB575FileExtract2();
          }

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered while writing to the Extract File.";
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }
        }

        // *** Commit if it's time
        if (local.ProcessCountToCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          local.ProgramCheckpointRestart.ProgramName = global.UserId;
          local.ProgramCheckpointRestart.RestartInfo =
            NumberToString(entities.Office.SystemGeneratedId, 12, 4) + " " + NumberToString
            (entities.ServiceProvider.SystemGeneratedId, 11, 5);
          local.ProgramCheckpointRestart.RestartInd = "Y";
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.NeededToWrite.RptDetail =
              "Error in update checkpoint restart.  Exitstate msg is: " + local
              .ExitStateWorkArea.Message;
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }

          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error in External to do a commit for: " + local
              .NeededToWrite.RptDetail;
            UseCabErrorReport3();
            ExitState = "ACO_AE0000_BATCH_ABEND";

            return;
          }

          local.ProcessCountToCommit.Count = 0;
        }

        ++local.ProcessCountToCommit.Count;
      }
    }

    // *********** Main Processing Logic Ends Here **********
    // END OF PROCESSING
    // : Successful end of job, so update checkpoint restart.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail =
        "Successful End of job, but error in update checkpoint restart.  Exitstate msg is: " +
        local.ExitStateWorkArea.Message;
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // **** START OF REPORT DSN CLOSE PROCESS ****
    // : Close the output file
    local.EabFileHandling.Action = "CLOSE";
    UseLeEabWriteB575FileExtract3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Error encountered closing the Extract File.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // **** END OF REPORT DSN CLOSE PROCESS ****
    // : Write Control Totals
    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Total Cases Read          :";
    local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
      NumberToString(local.TotalCasesReadCnt.Count, 7, 9);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.NeededToWrite.RptDetail = "Total Legal Referrals Read:";
    local.NeededToWrite.RptDetail = TrimEnd(local.NeededToWrite.RptDetail) + " " +
      NumberToString(local.TotalLegalRefReadCnt.Count, 7, 9);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // : Close Control Report file
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.NeededToWrite.RptDetail =
        "Close of Control Report file was unsuccessful.";
      UseCabErrorReport3();
      ExitState = "ACO_AE0000_BATCH_ABEND";

      return;
    }

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeEabWriteB575FileExtract1()
  {
    var useImport = new LeEabWriteB575FileExtract.Import();
    var useExport = new LeEabWriteB575FileExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveCseOrganization(local.File, useImport.RegionInfo);
    MoveOffice(local.FileOfficeInfo, useImport.OfficeInfo);
    useImport.SpInfo.Assign(local.FileSpInfo);
    useImport.CoCaseloadCnt.Count = local.ActiveCoCaseloadCnt.Count;
    useImport.CoReferredCnt.Count = local.ActiveCoCaseRefCnt.Count;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteB575FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabWriteB575FileExtract2()
  {
    var useImport = new LeEabWriteB575FileExtract.Import();
    var useExport = new LeEabWriteB575FileExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveCseOrganization(local.File, useImport.RegionInfo);
    MoveOffice(local.FileOfficeInfo, useImport.OfficeInfo);
    useImport.SpInfo.Assign(local.FileSpInfo);
    useImport.CoCaseloadCnt.Count = local.InactiveCoCaseloadCnt.Count;
    useImport.CoReferredCnt.Count = local.InactiveCoCaseRefCnt.Count;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteB575FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeEabWriteB575FileExtract3()
  {
    var useImport = new LeEabWriteB575FileExtract.Import();
    var useExport = new LeEabWriteB575FileExtract.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeEabWriteB575FileExtract.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadCaseOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          local.ReportingEom.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 3);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 4);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCseOrganization()
  {
    System.Diagnostics.Debug.Assert(entities.Office.Populated);
    entities.Region.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.
          SetString(command, "cogParentType", entities.Office.CogTypeCode ?? "");
          
        db.SetString(command, "cogParentCode", entities.Office.CogCode ?? "");
      },
      (db, reader) =>
      {
        entities.Region.Code = db.GetString(reader, 0);
        entities.Region.Type1 = db.GetString(reader, 1);
        entities.Region.Name = db.GetString(reader, 2);
        entities.Region.Populated = true;
      });
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "statusDate", local.ReportingEom.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 4);
        entities.LegalReferral.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Office.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", local.RestartOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.CogTypeCode = db.GetNullableString(reader, 2);
        entities.Office.CogCode = db.GetNullableString(reader, 3);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return ReadEach("ReadServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(
          command, "servicePrvderId", local.RestartSp.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.LastName = db.GetString(reader, 1);
        entities.ServiceProvider.FirstName = db.GetString(reader, 2);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 3);
        entities.ServiceProvider.Populated = true;

        return true;
      });
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
    /// A value of ReportingEom.
    /// </summary>
    [JsonPropertyName("reportingEom")]
    public DateWorkArea ReportingEom
    {
      get => reportingEom ??= new();
      set => reportingEom = value;
    }

    /// <summary>
    /// A value of CoRegion.
    /// </summary>
    [JsonPropertyName("coRegion")]
    public CseOrganization CoRegion
    {
      get => coRegion ??= new();
      set => coRegion = value;
    }

    /// <summary>
    /// A value of Co.
    /// </summary>
    [JsonPropertyName("co")]
    public Office Co
    {
      get => co ??= new();
      set => co = value;
    }

    /// <summary>
    /// A value of File.
    /// </summary>
    [JsonPropertyName("file")]
    public CseOrganization File
    {
      get => file ??= new();
      set => file = value;
    }

    /// <summary>
    /// A value of FileOfficeInfo.
    /// </summary>
    [JsonPropertyName("fileOfficeInfo")]
    public Office FileOfficeInfo
    {
      get => fileOfficeInfo ??= new();
      set => fileOfficeInfo = value;
    }

    /// <summary>
    /// A value of FileSpInfo.
    /// </summary>
    [JsonPropertyName("fileSpInfo")]
    public ServiceProvider FileSpInfo
    {
      get => fileSpInfo ??= new();
      set => fileSpInfo = value;
    }

    /// <summary>
    /// A value of ActiveCoCaseloadCnt.
    /// </summary>
    [JsonPropertyName("activeCoCaseloadCnt")]
    public Common ActiveCoCaseloadCnt
    {
      get => activeCoCaseloadCnt ??= new();
      set => activeCoCaseloadCnt = value;
    }

    /// <summary>
    /// A value of InactiveCoCaseloadCnt.
    /// </summary>
    [JsonPropertyName("inactiveCoCaseloadCnt")]
    public Common InactiveCoCaseloadCnt
    {
      get => inactiveCoCaseloadCnt ??= new();
      set => inactiveCoCaseloadCnt = value;
    }

    /// <summary>
    /// A value of ActiveCoCaseRefCnt.
    /// </summary>
    [JsonPropertyName("activeCoCaseRefCnt")]
    public Common ActiveCoCaseRefCnt
    {
      get => activeCoCaseRefCnt ??= new();
      set => activeCoCaseRefCnt = value;
    }

    /// <summary>
    /// A value of InactiveCoCaseRefCnt.
    /// </summary>
    [JsonPropertyName("inactiveCoCaseRefCnt")]
    public Common InactiveCoCaseRefCnt
    {
      get => inactiveCoCaseRefCnt ??= new();
      set => inactiveCoCaseRefCnt = value;
    }

    /// <summary>
    /// A value of ActiveServiceProvider.
    /// </summary>
    [JsonPropertyName("activeServiceProvider")]
    public Common ActiveServiceProvider
    {
      get => activeServiceProvider ??= new();
      set => activeServiceProvider = value;
    }

    /// <summary>
    /// A value of TotalCasesReadCnt.
    /// </summary>
    [JsonPropertyName("totalCasesReadCnt")]
    public Common TotalCasesReadCnt
    {
      get => totalCasesReadCnt ??= new();
      set => totalCasesReadCnt = value;
    }

    /// <summary>
    /// A value of TotalLegalRefReadCnt.
    /// </summary>
    [JsonPropertyName("totalLegalRefReadCnt")]
    public Common TotalLegalRefReadCnt
    {
      get => totalLegalRefReadCnt ??= new();
      set => totalLegalRefReadCnt = value;
    }

    /// <summary>
    /// A value of RestartSp.
    /// </summary>
    [JsonPropertyName("restartSp")]
    public ServiceProvider RestartSp
    {
      get => restartSp ??= new();
      set => restartSp = value;
    }

    /// <summary>
    /// A value of RestartOffice.
    /// </summary>
    [JsonPropertyName("restartOffice")]
    public Office RestartOffice
    {
      get => restartOffice ??= new();
      set => restartOffice = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea reportingEom;
    private CseOrganization coRegion;
    private Office co;
    private CseOrganization file;
    private Office fileOfficeInfo;
    private ServiceProvider fileSpInfo;
    private Common activeCoCaseloadCnt;
    private Common inactiveCoCaseloadCnt;
    private Common activeCoCaseRefCnt;
    private Common inactiveCoCaseRefCnt;
    private Common activeServiceProvider;
    private Common totalCasesReadCnt;
    private Common totalLegalRefReadCnt;
    private ServiceProvider restartSp;
    private Office restartOffice;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private Common processCountToCommit;
    private External passArea;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Region.
    /// </summary>
    [JsonPropertyName("region")]
    public CseOrganization Region
    {
      get => region ??= new();
      set => region = value;
    }

    /// <summary>
    /// A value of CoOfficer.
    /// </summary>
    [JsonPropertyName("coOfficer")]
    public CseOrganization CoOfficer
    {
      get => coOfficer ??= new();
      set => coOfficer = value;
    }

    /// <summary>
    /// A value of CseOrganizationRelationship.
    /// </summary>
    [JsonPropertyName("cseOrganizationRelationship")]
    public CseOrganizationRelationship CseOrganizationRelationship
    {
      get => cseOrganizationRelationship ??= new();
      set => cseOrganizationRelationship = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    private CseOrganization region;
    private CseOrganization coOfficer;
    private CseOrganizationRelationship cseOrganizationRelationship;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
    private LegalReferral legalReferral;
  }
#endregion
}

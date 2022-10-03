// Program: FN_B735_DASHBOARD_AUDIT_PURGE, ID: 1902439122, model: 746.
// Short name: SWEF735B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B735_DASHBOARD_AUDIT_PURGE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB735DashboardAuditPurge: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B735_DASHBOARD_AUDIT_PURGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB735DashboardAuditPurge(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB735DashboardAuditPurge.
  /// </summary>
  public FnB735DashboardAuditPurge(IContext context, Import import,
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
    // 07-28-14  LSS			Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (Equal(local.ProgramProcessingInfo.ProcessDate, local.Null1.Date))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Read the Checkpoint Restart Record.
    // -------------------------------------------------------------------------------------------------------------------------
    // Checkpoint - commit frequency is determined by update frequency
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // -- Set Process Date to minus 5 Months for purgable date then convert to 
    // timestamp
    // -------------------------------------------------------------------------------------------------------------------------
    local.Process.Date = AddMonths(local.ProgramProcessingInfo.ProcessDate, -5);

    // Convert Process Date to a DateTimestamp
    local.Process.Time = StringToTime("00.00.00.000000").GetValueOrDefault();
    UseFnBuildTimestampFrmDateTime();
    local.Compare.Timestamp = local.Process.Timestamp;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Delete audit data records older than 5 months from current process 
    // date
    // -------------------------------------------------------------------------------------------------------------------------
    local.TotalDeleted.Count = 0;
    local.RecordProcessingCount.Count = 0;

    foreach(var item in ReadDashboardAuditData4())
    {
      DeleteDashboardAuditData();
      ++local.TotalDeleted.Count;

      // Commit if necessary
      ++local.RecordProcessingCount.Count;

      if (local.RecordProcessingCount.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "(01) Error taking checkpoint";
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.RecordProcessingCount.Count = 0;
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Total Records Older Than 5 Months Purged:     " + NumberToString
            (local.TotalDeleted.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Report Month     Run Number     Number of Records Purged";

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.RecordProcessingCount.Count = 0;

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Delete audit data records for months where run number is previous to 
    // the last full run
    // 
    // (a full run would always include dashboard priority 1-1(D) and priority 5
    // -24)
    // 
    // but not within the same month as the process date
    // -------------------------------------------------------------------------------------------------------------------------
    local.PreviousReportMonth.ReportMonth = -1;
    local.Previous.ReportMonth =
      Year(AddMonths(local.ProgramProcessingInfo.ProcessDate, -1)) * 100 + Month
      (AddMonths(local.ProgramProcessingInfo.ProcessDate, -1));

    foreach(var item in ReadDashboardAuditData5())
    {
      if (entities.ReportMonth.ReportMonth == local
        .PreviousReportMonth.ReportMonth)
      {
        continue;
      }
      else
      {
        local.PreviousReportMonth.ReportMonth =
          entities.ReportMonth.ReportMonth;
      }

      local.FinalMonthlyRun.Flag = "N";
      local.PreviousRunNumber.ReportMonth = 0;
      local.PreviousRunNumber.RunNumber = 0;

      foreach(var item1 in ReadDashboardAuditData6())
      {
        if (entities.RunNumber.ReportMonth == local
          .PreviousRunNumber.ReportMonth && entities.RunNumber.RunNumber == local
          .PreviousRunNumber.RunNumber)
        {
          continue;
        }
        else
        {
          MoveDashboardAuditData(entities.RunNumber, local.PreviousRunNumber);
        }

        local.RunNumberDeleted.Count = 0;

        if (AsChar(local.FinalMonthlyRun.Flag) == 'N')
        {
          if (ReadDashboardAuditData1())
          {
            if (ReadDashboardAuditData2())
            {
              local.FinalMonthlyRun.Flag = "Y";
            }
          }
        }
        else
        {
          foreach(var item2 in ReadDashboardAuditData3())
          {
            DeleteDashboardAuditData();
            ++local.RunNumberDeleted.Count;
            ++local.TotalDeleted.Count;

            // Commit if necessary
            ++local.RecordProcessingCount.Count;

            if (local.RecordProcessingCount.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              local.ProgramCheckpointRestart.RestartInd = "Y";
              UseUpdateCheckpointRstAndCommit();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // -- Write to the error report.
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail = "(02) Error taking checkpoint";
                UseCabErrorReport2();

                // -- Set Abort exit state and escape...
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              local.RecordProcessingCount.Count = 0;
            }
          }
        }

        local.EabReportSend.RptDetail = "   " + NumberToString
          (entities.RunNumber.ReportMonth, 10, 6) + "            " + NumberToString
          (entities.RunNumber.RunNumber, 13, 3) + "               " + NumberToString
          (local.RunNumberDeleted.Count, 8, 8) + "     ";
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "(02) Error Writing Control Report...  Returned Status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(03) Error Writing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "Grand Total Records Deleted:     " + NumberToString
      (local.TotalDeleted.Count, 8, 10);
    local.EabFileHandling.Action = "WRITE";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "(04) Error Writing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "(03) Error taking checkpoint";
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDashboardAuditData(DashboardAuditData source,
    DashboardAuditData target)
  {
    target.ReportMonth = source.ReportMonth;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseFnBuildTimestampFrmDateTime()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea(local.Process, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    local.Process.Assign(useExport.DateWorkArea);
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

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private void DeleteDashboardAuditData()
  {
    Update("DeleteDashboardAuditData",
      (db, command) =>
      {
        db.SetInt32(
          command, "reportMonth", entities.DashboardAuditData.ReportMonth);
        db.SetString(
          command, "dashboardPriority",
          entities.DashboardAuditData.DashboardPriority);
        db.
          SetInt32(command, "runNumber", entities.DashboardAuditData.RunNumber);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.DashboardAuditData.CreatedTimestamp.GetValueOrDefault());
      });
  }

  private bool ReadDashboardAuditData1()
  {
    entities.DashboardAuditData.Populated = false;

    return Read("ReadDashboardAuditData1",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", entities.RunNumber.ReportMonth);
        db.SetInt32(command, "runNumber", entities.RunNumber.RunNumber);
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.Populated = true;
      });
  }

  private bool ReadDashboardAuditData2()
  {
    entities.DashboardAuditData.Populated = false;

    return Read("ReadDashboardAuditData2",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", entities.RunNumber.ReportMonth);
        db.SetInt32(command, "runNumber", entities.RunNumber.RunNumber);
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDashboardAuditData3()
  {
    entities.DashboardAuditData.Populated = false;

    return ReadEach("ReadDashboardAuditData3",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", entities.RunNumber.ReportMonth);
        db.SetInt32(command, "runNumber", entities.RunNumber.RunNumber);
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardAuditData4()
  {
    entities.DashboardAuditData.Populated = false;

    return ReadEach("ReadDashboardAuditData4",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Compare.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DashboardAuditData.ReportMonth = db.GetInt32(reader, 0);
        entities.DashboardAuditData.DashboardPriority = db.GetString(reader, 1);
        entities.DashboardAuditData.RunNumber = db.GetInt32(reader, 2);
        entities.DashboardAuditData.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DashboardAuditData.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardAuditData5()
  {
    entities.ReportMonth.Populated = false;

    return ReadEach("ReadDashboardAuditData5",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", local.Previous.ReportMonth);
      },
      (db, reader) =>
      {
        entities.ReportMonth.ReportMonth = db.GetInt32(reader, 0);
        entities.ReportMonth.DashboardPriority = db.GetString(reader, 1);
        entities.ReportMonth.RunNumber = db.GetInt32(reader, 2);
        entities.ReportMonth.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.ReportMonth.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDashboardAuditData6()
  {
    entities.RunNumber.Populated = false;

    return ReadEach("ReadDashboardAuditData6",
      (db, command) =>
      {
        db.SetInt32(command, "reportMonth", entities.ReportMonth.ReportMonth);
      },
      (db, reader) =>
      {
        entities.RunNumber.ReportMonth = db.GetInt32(reader, 0);
        entities.RunNumber.DashboardPriority = db.GetString(reader, 1);
        entities.RunNumber.RunNumber = db.GetInt32(reader, 2);
        entities.RunNumber.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.RunNumber.Populated = true;

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
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DashboardAuditData Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of PreviousRunNumber.
    /// </summary>
    [JsonPropertyName("previousRunNumber")]
    public DashboardAuditData PreviousRunNumber
    {
      get => previousRunNumber ??= new();
      set => previousRunNumber = value;
    }

    /// <summary>
    /// A value of PreviousReportMonth.
    /// </summary>
    [JsonPropertyName("previousReportMonth")]
    public DashboardAuditData PreviousReportMonth
    {
      get => previousReportMonth ??= new();
      set => previousReportMonth = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of RunNumberDeleted.
    /// </summary>
    [JsonPropertyName("runNumberDeleted")]
    public Common RunNumberDeleted
    {
      get => runNumberDeleted ??= new();
      set => runNumberDeleted = value;
    }

    /// <summary>
    /// A value of FinalMonthlyRun.
    /// </summary>
    [JsonPropertyName("finalMonthlyRun")]
    public Common FinalMonthlyRun
    {
      get => finalMonthlyRun ??= new();
      set => finalMonthlyRun = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DateWorkArea Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of RecordProcessingCount.
    /// </summary>
    [JsonPropertyName("recordProcessingCount")]
    public Common RecordProcessingCount
    {
      get => recordProcessingCount ??= new();
      set => recordProcessingCount = value;
    }

    /// <summary>
    /// A value of TotalDeleted.
    /// </summary>
    [JsonPropertyName("totalDeleted")]
    public Common TotalDeleted
    {
      get => totalDeleted ??= new();
      set => totalDeleted = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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

    private DashboardAuditData previous;
    private DashboardAuditData previousRunNumber;
    private DashboardAuditData previousReportMonth;
    private Common common;
    private DateWorkArea null1;
    private Common runNumberDeleted;
    private Common finalMonthlyRun;
    private DateWorkArea compare;
    private DateWorkArea process;
    private Common recordProcessingCount;
    private Common totalDeleted;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private External external;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of RunNumber.
    /// </summary>
    [JsonPropertyName("runNumber")]
    public DashboardAuditData RunNumber
    {
      get => runNumber ??= new();
      set => runNumber = value;
    }

    /// <summary>
    /// A value of ReportMonth.
    /// </summary>
    [JsonPropertyName("reportMonth")]
    public DashboardAuditData ReportMonth
    {
      get => reportMonth ??= new();
      set => reportMonth = value;
    }

    /// <summary>
    /// A value of DashboardAuditData.
    /// </summary>
    [JsonPropertyName("dashboardAuditData")]
    public DashboardAuditData DashboardAuditData
    {
      get => dashboardAuditData ??= new();
      set => dashboardAuditData = value;
    }

    private DashboardAuditData runNumber;
    private DashboardAuditData reportMonth;
    private DashboardAuditData dashboardAuditData;
  }
#endregion
}

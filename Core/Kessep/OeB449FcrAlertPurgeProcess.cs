// Program: OE_B449_FCR_ALERT_PURGE_PROCESS, ID: 1625402141, model: 746.
// Short name: SWEI449B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B449_FCR_ALERT_PURGE_PROCESS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB449FcrAlertPurgeProcess: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B449_FCR_ALERT_PURGE_PROCESS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB449FcrAlertPurgeProcess(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB449FcrAlertPurgeProcess.
  /// </summary>
  public OeB449FcrAlertPurgeProcess(IContext context, Import import,
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
    // ***************************************************************************************
    // *   Date     Developer  PR#/WR#     
    // Description
    // 
    // *
    // *-------------------------------------------------------------------------------------*
    // * 12/22/2020 Raj        CQ13044
    // Initial Development
    // 
    // *
    // *
    // 
    // Process will select the
    // infrastructure recrods    *
    // *
    // 
    // with reason codes FCRNEWSSNNDNHNH,
    // FCRNEWSSNNDNHQW*
    // *
    // 
    // FCRNEWSSNNDNHQW, FCRSVESPRISONAP,
    // FCRSVESPRISONAR,*
    // *
    // 
    // FCRSVESPRISONCH which are one year
    // old as of end  *
    // *
    // 
    // of previous month(calculated based
    // on run date).  *
    // *
    // 
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB449Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ReportPadding.Text2 = "";
    local.ReportDetailMax.Count = 99;
    local.EabFileHandling.Action = "WRITE";
    local.PurgeCount.Count = 0;
    local.PurgableRecordsCount.Count = 0;
    local.RecordProcessingCount.Count = 0;

    // ********************************************************************************************************
    // *** Read infrastructure records with reason codes FCRNEWSSNNDNHNH, 
    // FCRNEWSSNNDNHQW, FCRNEWSSNNDNHQW, ***
    // *** FCRSVESPRISONAP, FCRSVESPRISONAR, FCRSVESPRISONCH which are older 
    // than the purge date provided   ***
    // *** through Program Processing Information table.
    // 
    // ***
    // ********************************************************************************************************
    foreach(var item in ReadInfrastructure())
    {
      // ********************************************************************************************************
      // *** Set Report Values
      // 
      // ***
      // ********************************************************************************************************
      local.RptInfId.Text18 =
        NumberToString(entities.ExistingInfrastructure.
          SystemGeneratedIdentifier, 18);
      local.RptInfId.Text18 =
        NumberToString(entities.ExistingInfrastructure.
          SystemGeneratedIdentifier, 1, 15);
      local.ReportReasonCd.Text17 = entities.ExistingInfrastructure.ReasonCode;
      local.RptCaseNumber.Text12 =
        entities.ExistingInfrastructure.CaseNumber ?? Spaces(12);
      local.RptPersonNumber.Text12 =
        entities.ExistingInfrastructure.CsePersonNumber ?? Spaces(12);

      if (DateToInt(entities.ExistingInfrastructure.ReferenceDate) > 0)
      {
        local.DayCharValue.Text2 =
          NumberToString(Day(entities.ExistingInfrastructure.ReferenceDate), 14,
          2);
        local.MonthCharValue.Text2 =
          NumberToString(Month(entities.ExistingInfrastructure.ReferenceDate),
          14, 2);
        local.YearCharValue.Text4 =
          NumberToString(Year(entities.ExistingInfrastructure.ReferenceDate),
          12, 4);
        local.RptRefDate.Text12 = local.MonthCharValue.Text2 + "/" + local
          .DayCharValue.Text2 + "/" + local.YearCharValue.Text4;
      }
      else
      {
        local.RptRefDate.Text12 = "";
      }

      // ********************************************************************************************************
      // *** Increment the count of purgable records found
      // 
      // ***
      // ********************************************************************************************************
      ++local.TotalPurgableRecords.Count;

      // ********************************************************************************************************
      // *** Determine dependent records for the selected infrastructure records
      // and delete the same before   ***
      // *** Deleting the parent infrastructure record.
      // 
      // ***
      // ********************************************************************************************************
      // ********************************************************************************************************
      // *** Check any Monitored Activity Rows exists for the selected 
      // infrastructure record, if so, print &  ***
      // *** do not deleted the infrastructure record.
      // 
      // ***
      // ********************************************************************************************************
      foreach(var item1 in ReadMonitoredActivity())
      {
        local.RptRemarks.Text30 = "Monitor Act exists Not Purged";
        local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
          .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
          .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
          .RptRemarks.Text30;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalRecordsSkipped.Count;

        goto ReadEach;
      }

      // ********************************************************************************************************
      // *** Check any Outoging Document record associated with selected 
      // infrastructure record, if so, print  ***
      // *** the information and do not 
      // deleted the infrastructure record.
      // 
      // ***
      // ********************************************************************************************************
      foreach(var item1 in ReadOutgoingDocument())
      {
        local.RptRemarks.Text30 = "Outgoing Doc exists Not Purged";
        local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
          .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
          .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
          .RptRemarks.Text30;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalRecordsSkipped.Count;

        goto ReadEach;
      }

      // ********************************************************************************************************
      // *** Do not delete record if job is running in
      // debug mode
      // 
      // ***
      // ********************************************************************************************************
      if (AsChar(local.Debug.Flag) == 'Y')
      {
        local.RptRemarks.Text30 = "Record To Be Deleted";
        local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
          .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
          .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
          .RptRemarks.Text30;
        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.TotalRecordsPurged.Count;

        continue;
      }
      else
      {
        // ********************************************************************************************************
        // *** Now the selected Infrastructure record needs to be purged, first 
        // associated child records        ***
        // *** Office Service Provider Alerts & Narrative Details records are 
        // purged first befor purging the    ***
        // *** selected infrastructure records and 
        // same will be printed.
        // 
        // ***
        // ********************************************************************************************************
        foreach(var item1 in ReadOfficeServiceProviderAlert())
        {
          DeleteOfficeServiceProviderAlert();
          ++local.TotalAlertsPurged.Count;
        }

        foreach(var item1 in ReadNarrativeDetail())
        {
          DeleteNarrativeDetail();
          ++local.TotalNarrPurged.Count;
        }

        DeleteInfrastructure();
        local.RptRemarks.Text30 = "Record Purged Successfully";
        local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
          .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
          .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
          .RptRemarks.Text30;
        ++local.TotalRecordsPurged.Count;
      }

      // ********************************************************************************************************
      // *** Build Report Record and write the same 
      // with comments
      // 
      // ***
      // ********************************************************************************************************
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ********************************************************************************************************
      // *** Perform the Database Commit based on the commit record count 
      // parameter                           ***
      // ********************************************************************************************************
      ++local.RecordProcessingCount.Count;

      if (local.RecordProcessingCount.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }

        local.RecordProcessingCount.Count = 0;
      }

ReadEach:
      ;
    }

    // ********************************************************************************************************
    // *** Perform the Database Commit based on the commit record count 
    // parameter                           ***
    // ********************************************************************************************************
    // Do a final commit so that if the program abends during report closing the
    // program doesn't need to be re-run.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // ********************************************************************************************************
    // *** Write totals and close reports
    // 
    // ***
    // ********************************************************************************************************
    UseSiB449WriteControlsAndClose();
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB449Housekeeping()
  {
    var useImport = new SiB449Housekeeping.Import();
    var useExport = new SiB449Housekeeping.Export();

    Call(SiB449Housekeeping.Execute, useImport, useExport);

    local.PurgeDate.Date = useExport.PurgeDate.Date;
    local.Debug.Flag = useExport.Debug.Flag;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseSiB449WriteControlsAndClose()
  {
    var useImport = new SiB449WriteControlsAndClose.Import();
    var useExport = new SiB449WriteControlsAndClose.Export();

    useImport.TotalPurgableRecords.Count = local.TotalPurgableRecords.Count;
    useImport.TotalRecordsPurged.Count = local.TotalRecordsPurged.Count;
    useImport.TotalAlertsPurged.Count = local.TotalAlertsPurged.Count;
    useImport.TotalNarrPurged.Count = local.TotalNarrPurged.Count;
    useImport.TotalRecordsSkipped.Count = local.TotalRecordsSkipped.Count;

    Call(SiB449WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void DeleteInfrastructure()
  {
    Update("DeleteInfrastructure#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      });
  }

  private void DeleteNarrativeDetail()
  {
    Update("DeleteNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingNarrativeDetail.InfrastructureId);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingNarrativeDetail.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "lineNumber", entities.ExistingNarrativeDetail.LineNumber);
      });
  }

  private void DeleteOfficeServiceProviderAlert()
  {
    Update("DeleteOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ExistingOfficeServiceProviderAlert.
            SystemGeneratedIdentifier);
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.ExistingInfrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "referenceDate", local.PurgeDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingInfrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingInfrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.ExistingInfrastructure.ReasonCode = db.GetString(reader, 2);
        entities.ExistingInfrastructure.CaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInfrastructure.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingInfrastructure.ReferenceDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingInfrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonitoredActivity()
  {
    entities.ExistingMonitoredActivity.Populated = false;

    return ReadEach("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infSysGenId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingMonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingMonitoredActivity.Name = db.GetString(reader, 1);
        entities.ExistingMonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 2);
        entities.ExistingMonitoredActivity.InfSysGenId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingMonitoredActivity.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 2);
        entities.ExistingNarrativeDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProviderAlert()
  {
    entities.ExistingOfficeServiceProviderAlert.Populated = false;

    return ReadEach("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 1);
        entities.ExistingOfficeServiceProviderAlert.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocument()
  {
    entities.ExistingOutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId",
          entities.ExistingInfrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingOutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.ExistingOutgoingDocument.InfId = db.GetInt32(reader, 1);
        entities.ExistingOutgoingDocument.Populated = true;

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
    /// A value of TotalRecordsSkipped.
    /// </summary>
    [JsonPropertyName("totalRecordsSkipped")]
    public Common TotalRecordsSkipped
    {
      get => totalRecordsSkipped ??= new();
      set => totalRecordsSkipped = value;
    }

    /// <summary>
    /// A value of RptInfId.
    /// </summary>
    [JsonPropertyName("rptInfId")]
    public WorkArea RptInfId
    {
      get => rptInfId ??= new();
      set => rptInfId = value;
    }

    /// <summary>
    /// A value of ReportReasonCd.
    /// </summary>
    [JsonPropertyName("reportReasonCd")]
    public WorkArea ReportReasonCd
    {
      get => reportReasonCd ??= new();
      set => reportReasonCd = value;
    }

    /// <summary>
    /// A value of RptRefDate.
    /// </summary>
    [JsonPropertyName("rptRefDate")]
    public WorkArea RptRefDate
    {
      get => rptRefDate ??= new();
      set => rptRefDate = value;
    }

    /// <summary>
    /// A value of RptRemarks.
    /// </summary>
    [JsonPropertyName("rptRemarks")]
    public TextWorkArea RptRemarks
    {
      get => rptRemarks ??= new();
      set => rptRemarks = value;
    }

    /// <summary>
    /// A value of TotalRecordsPurged.
    /// </summary>
    [JsonPropertyName("totalRecordsPurged")]
    public Common TotalRecordsPurged
    {
      get => totalRecordsPurged ??= new();
      set => totalRecordsPurged = value;
    }

    /// <summary>
    /// A value of TotalAlertsPurged.
    /// </summary>
    [JsonPropertyName("totalAlertsPurged")]
    public Common TotalAlertsPurged
    {
      get => totalAlertsPurged ??= new();
      set => totalAlertsPurged = value;
    }

    /// <summary>
    /// A value of TotalNarrPurged.
    /// </summary>
    [JsonPropertyName("totalNarrPurged")]
    public Common TotalNarrPurged
    {
      get => totalNarrPurged ??= new();
      set => totalNarrPurged = value;
    }

    /// <summary>
    /// A value of TotalPurgableRecords.
    /// </summary>
    [JsonPropertyName("totalPurgableRecords")]
    public Common TotalPurgableRecords
    {
      get => totalPurgableRecords ??= new();
      set => totalPurgableRecords = value;
    }

    /// <summary>
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of PurgeDate.
    /// </summary>
    [JsonPropertyName("purgeDate")]
    public DateWorkArea PurgeDate
    {
      get => purgeDate ??= new();
      set => purgeDate = value;
    }

    /// <summary>
    /// A value of PurgableRecordsCount.
    /// </summary>
    [JsonPropertyName("purgableRecordsCount")]
    public Common PurgableRecordsCount
    {
      get => purgableRecordsCount ??= new();
      set => purgableRecordsCount = value;
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
    /// A value of RecordProcessingCount.
    /// </summary>
    [JsonPropertyName("recordProcessingCount")]
    public Common RecordProcessingCount
    {
      get => recordProcessingCount ??= new();
      set => recordProcessingCount = value;
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

    /// <summary>
    /// A value of MonthCharValue.
    /// </summary>
    [JsonPropertyName("monthCharValue")]
    public TextWorkArea MonthCharValue
    {
      get => monthCharValue ??= new();
      set => monthCharValue = value;
    }

    /// <summary>
    /// A value of DayCharValue.
    /// </summary>
    [JsonPropertyName("dayCharValue")]
    public TextWorkArea DayCharValue
    {
      get => dayCharValue ??= new();
      set => dayCharValue = value;
    }

    /// <summary>
    /// A value of YearCharValue.
    /// </summary>
    [JsonPropertyName("yearCharValue")]
    public TextWorkArea YearCharValue
    {
      get => yearCharValue ??= new();
      set => yearCharValue = value;
    }

    /// <summary>
    /// A value of ReportPadding.
    /// </summary>
    [JsonPropertyName("reportPadding")]
    public WorkArea ReportPadding
    {
      get => reportPadding ??= new();
      set => reportPadding = value;
    }

    /// <summary>
    /// A value of ReportDetailMax.
    /// </summary>
    [JsonPropertyName("reportDetailMax")]
    public Common ReportDetailMax
    {
      get => reportDetailMax ??= new();
      set => reportDetailMax = value;
    }

    /// <summary>
    /// A value of PurgeCount.
    /// </summary>
    [JsonPropertyName("purgeCount")]
    public Common PurgeCount
    {
      get => purgeCount ??= new();
      set => purgeCount = value;
    }

    /// <summary>
    /// A value of RptPersonNumber.
    /// </summary>
    [JsonPropertyName("rptPersonNumber")]
    public WorkArea RptPersonNumber
    {
      get => rptPersonNumber ??= new();
      set => rptPersonNumber = value;
    }

    /// <summary>
    /// A value of RptCaseNumber.
    /// </summary>
    [JsonPropertyName("rptCaseNumber")]
    public WorkArea RptCaseNumber
    {
      get => rptCaseNumber ??= new();
      set => rptCaseNumber = value;
    }

    private Common totalRecordsSkipped;
    private WorkArea rptInfId;
    private WorkArea reportReasonCd;
    private WorkArea rptRefDate;
    private TextWorkArea rptRemarks;
    private Common totalRecordsPurged;
    private Common totalAlertsPurged;
    private Common totalNarrPurged;
    private Common totalPurgableRecords;
    private Common debug;
    private DateWorkArea purgeDate;
    private Common purgableRecordsCount;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common recordProcessingCount;
    private External external;
    private ProgramCheckpointRestart programCheckpointRestart;
    private TextWorkArea monthCharValue;
    private TextWorkArea dayCharValue;
    private TextWorkArea yearCharValue;
    private WorkArea reportPadding;
    private Common reportDetailMax;
    private Common purgeCount;
    private WorkArea rptPersonNumber;
    private WorkArea rptCaseNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProviderAlert")]
    public OfficeServiceProviderAlert ExistingOfficeServiceProviderAlert
    {
      get => existingOfficeServiceProviderAlert ??= new();
      set => existingOfficeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    /// <summary>
    /// A value of ExistingMonitoredActivity.
    /// </summary>
    [JsonPropertyName("existingMonitoredActivity")]
    public MonitoredActivity ExistingMonitoredActivity
    {
      get => existingMonitoredActivity ??= new();
      set => existingMonitoredActivity = value;
    }

    /// <summary>
    /// A value of ExistingOutgoingDocument.
    /// </summary>
    [JsonPropertyName("existingOutgoingDocument")]
    public OutgoingDocument ExistingOutgoingDocument
    {
      get => existingOutgoingDocument ??= new();
      set => existingOutgoingDocument = value;
    }

    private Infrastructure existingInfrastructure;
    private OfficeServiceProviderAlert existingOfficeServiceProviderAlert;
    private NarrativeDetail existingNarrativeDetail;
    private MonitoredActivity existingMonitoredActivity;
    private OutgoingDocument existingOutgoingDocument;
  }
#endregion
}

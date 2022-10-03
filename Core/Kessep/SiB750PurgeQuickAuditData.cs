// Program: SI_B750_PURGE_QUICK_AUDIT_DATA, ID: 374543872, model: 746.
// Short name: SWEI750B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B750_PURGE_QUICK_AUDIT_DATA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB750PurgeQuickAuditData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B750_PURGE_QUICK_AUDIT_DATA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB750PurgeQuickAuditData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB750PurgeQuickAuditData.
  /// </summary>
  public SiB750PurgeQuickAuditData(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 08/24/2009	J Huss		CQ# 211		Initial development
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB750Housekeeping();

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

    // Convert purge date to timestamp.  The timestamp function requires a 
    // format of YYYY-MM-DD.
    local.TempDate.Text8 = NumberToString(DateToInt(local.Purge.Date), 8);
    local.Year.Text4 = Substring(local.TempDate.Text8, 1, 4);
    local.Month.Text2 = Substring(local.TempDate.Text8, 5, 2);
    local.Day.Text2 = Substring(local.TempDate.Text8, 7, 2);
    local.PurgeTimestamp.IefTimestamp = Timestamp(local.Year.Text4 + "-" + local
      .Month.Text2 + "-" + local.Day.Text2);

    // Read for records older than the purge date.
    // Sort by requesting state to provide for organization in the output 
    // report.
    foreach(var item in ReadQuickAudit())
    {
      local.RequestState.Text40 = entities.QuickAudit.RequestingCaseState;
      local.RequestUserId.Text50 = entities.QuickAudit.SystemUserId;
      local.RequestCaseId.Text15 = entities.QuickAudit.RequestingCaseOtherId;
      local.RequestTimestamp.IefTimestamp =
        entities.QuickAudit.RequestTimestamp;
      local.RequestTimestamp.TextTimestamp = "";

      // Convert IEF timestamp to text timestamp
      UseLeCabConvertTimestamp();

      // Write report detail in format of:
      // <REQUESTING STATE>  <REQUESTING USER ID>
      //      <REQUESTED CASE ID>  <REQUEST TIMESTAMP>
      local.Subscript.Count = 1;

      for(var limit = local.ReportDetailMax.Count; local.Subscript.Count <= limit
        ; ++local.Subscript.Count)
      {
        switch(local.Subscript.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = local.RequestState.Text40 + local
              .ReportPadding.Text2 + local.RequestUserId.Text50;

            break;
          case 2:
            local.EabReportSend.RptDetail = "     " + local
              .RequestCaseId.Text15 + local.ReportPadding.Text2 + local
              .RequestTimestamp.TextTimestamp;

            break;
          default:
            goto AfterCycle;
        }

        UseCabControlReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

AfterCycle:

      // Increment the count of purgable records found
      ++local.PurgableRecordsCount.Count;

      // Do not delete record if job is running in debug
      if (AsChar(local.Debug.Flag) == 'Y')
      {
        continue;
      }

      DeleteQuickAudit();

      // Increment the count of purged records
      ++local.PurgeCount.Count;

      // Commit if necessary
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
    }

    // Do a final commit so that if the program abends during report closing the
    // program doesn't need to be re-run.
    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

      return;
    }

    // Write totals and close reports
    UseSiB750WriteControlsAndClose();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.RequestTimestamp,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.RequestTimestamp);
  }

  private void UseSiB750Housekeeping()
  {
    var useImport = new SiB750Housekeeping.Import();
    var useExport = new SiB750Housekeeping.Export();

    Call(SiB750Housekeeping.Execute, useImport, useExport);

    local.Debug.Flag = useExport.Debug.Flag;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    local.Purge.Date = useExport.PurgeDate.Date;
  }

  private void UseSiB750WriteControlsAndClose()
  {
    var useImport = new SiB750WriteControlsAndClose.Import();
    var useExport = new SiB750WriteControlsAndClose.Export();

    useImport.TotalPurgableRecords.Count = local.PurgableRecordsCount.Count;
    useImport.TotalRecordsPurged.Count = local.PurgeCount.Count;

    Call(SiB750WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void DeleteQuickAudit()
  {
    Update("DeleteQuickAudit",
      (db, command) =>
      {
        db.SetString(command, "systemUserId", entities.QuickAudit.SystemUserId);
        db.SetDateTime(
          command, "requestTimestamp",
          entities.QuickAudit.RequestTimestamp.GetValueOrDefault());
      });
  }

  private IEnumerable<bool> ReadQuickAudit()
  {
    entities.QuickAudit.Populated = false;

    return ReadEach("ReadQuickAudit",
      (db, command) =>
      {
        db.SetDateTime(
          command, "requestTimestamp",
          local.PurgeTimestamp.IefTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.QuickAudit.SystemUserId = db.GetString(reader, 0);
        entities.QuickAudit.RequestTimestamp = db.GetDateTime(reader, 1);
        entities.QuickAudit.RequestorId = db.GetString(reader, 2);
        entities.QuickAudit.RequestingCaseId = db.GetString(reader, 3);
        entities.QuickAudit.RequestingCaseOtherId = db.GetString(reader, 4);
        entities.QuickAudit.SystemServerId = db.GetString(reader, 5);
        entities.QuickAudit.SystemResponseCode = db.GetString(reader, 6);
        entities.QuickAudit.DataResponseCode = db.GetString(reader, 7);
        entities.QuickAudit.StartDate = db.GetNullableDate(reader, 8);
        entities.QuickAudit.EndDate = db.GetNullableDate(reader, 9);
        entities.QuickAudit.DataRequestType = db.GetString(reader, 10);
        entities.QuickAudit.ProviderCaseState = db.GetString(reader, 11);
        entities.QuickAudit.ProviderCaseOtherId =
          db.GetNullableString(reader, 12);
        entities.QuickAudit.RequestingCaseState = db.GetString(reader, 13);
        entities.QuickAudit.StateGeneratedId = db.GetString(reader, 14);
        entities.QuickAudit.SystemResponseMessage = db.GetString(reader, 15);
        entities.QuickAudit.DataResponseMessage = db.GetString(reader, 16);
        entities.QuickAudit.Populated = true;

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
    /// A value of Day.
    /// </summary>
    [JsonPropertyName("day")]
    public WorkArea Day
    {
      get => day ??= new();
      set => day = value;
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
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public WorkArea Month
    {
      get => month ??= new();
      set => month = value;
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
    /// A value of Purge.
    /// </summary>
    [JsonPropertyName("purge")]
    public DateWorkArea Purge
    {
      get => purge ??= new();
      set => purge = value;
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
    /// A value of PurgeTimestamp.
    /// </summary>
    [JsonPropertyName("purgeTimestamp")]
    public BatchTimestampWorkArea PurgeTimestamp
    {
      get => purgeTimestamp ??= new();
      set => purgeTimestamp = value;
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
    /// A value of ReportDetailMax.
    /// </summary>
    [JsonPropertyName("reportDetailMax")]
    public Common ReportDetailMax
    {
      get => reportDetailMax ??= new();
      set => reportDetailMax = value;
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
    /// A value of RequestCaseId.
    /// </summary>
    [JsonPropertyName("requestCaseId")]
    public WorkArea RequestCaseId
    {
      get => requestCaseId ??= new();
      set => requestCaseId = value;
    }

    /// <summary>
    /// A value of RequestState.
    /// </summary>
    [JsonPropertyName("requestState")]
    public WorkArea RequestState
    {
      get => requestState ??= new();
      set => requestState = value;
    }

    /// <summary>
    /// A value of RequestTimestamp.
    /// </summary>
    [JsonPropertyName("requestTimestamp")]
    public BatchTimestampWorkArea RequestTimestamp
    {
      get => requestTimestamp ??= new();
      set => requestTimestamp = value;
    }

    /// <summary>
    /// A value of RequestUserId.
    /// </summary>
    [JsonPropertyName("requestUserId")]
    public WorkArea RequestUserId
    {
      get => requestUserId ??= new();
      set => requestUserId = value;
    }

    /// <summary>
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of TempDate.
    /// </summary>
    [JsonPropertyName("tempDate")]
    public WorkArea TempDate
    {
      get => tempDate ??= new();
      set => tempDate = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public WorkArea Year
    {
      get => year ??= new();
      set => year = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External external;
    private ProgramCheckpointRestart programCheckpointRestart;
    private WorkArea day;
    private Common debug;
    private WorkArea month;
    private Common purgableRecordsCount;
    private DateWorkArea purge;
    private Common purgeCount;
    private BatchTimestampWorkArea purgeTimestamp;
    private Common recordProcessingCount;
    private Common reportDetailMax;
    private WorkArea reportPadding;
    private WorkArea requestCaseId;
    private WorkArea requestState;
    private BatchTimestampWorkArea requestTimestamp;
    private WorkArea requestUserId;
    private Common subscript;
    private WorkArea tempDate;
    private WorkArea year;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }
#endregion
}

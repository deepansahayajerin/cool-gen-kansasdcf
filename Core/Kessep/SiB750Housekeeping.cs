// Program: SI_B750_HOUSEKEEPING, ID: 374543936, model: 746.
// Short name: SWE03126
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B750_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SiB750Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B750_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB750Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB750Housekeeping.
  /// </summary>
  public SiB750Housekeeping(IContext context, Import import, Export export):
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
    // 08/24/2008	J Huss		CQ# 211		Initial development
    // ----------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEIB750";
    local.Numbers.Text10 = "0123456789";
    local.ReportMax.Count = 99;

    // Retrieve PPI info
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Retrieve checkpoint restart info - commit frequency is determined by 
    // update frequency
    export.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;

    // Open Control Report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // Open Error Report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // If PPI date is null, set it to current date.
    if (!Lt(local.Null1.Date, local.ProgramProcessingInfo.ProcessDate))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // RI parameter is the Retention Interval in months.
    local.Position.Count =
      Find(local.ProgramProcessingInfo.ParameterList, "RI=");

    if (local.Position.Count > 0)
    {
      local.RetentionInterval.Text3 =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.Position.Count + 3, 3);

      // If Retention Interval value is not numeric then write error message and
      // escape.
      if (Verify(local.RetentionInterval.Text3, local.Numbers.Text10) != 0)
      {
        local.EabReportSend.RptDetail =
          "Retention interval is non-numeric.  Value from PPI record is: " + local
          .RetentionInterval.Text3;
        UseSiB750WriteControlsAndClose();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      // Default to 36 months.
      local.RetentionInterval.Text3 = "036";
    }

    // Set purge date to processing date minus retention interval, in months
    export.PurgeDate.Date =
      AddMonths(local.ProgramProcessingInfo.ProcessDate, (int)(-
      StringToNumber(local.RetentionInterval.Text3)));

    // Only set Debug mode if Debug value is specifically set to Y, otherwise 
    // default to N.
    export.Debug.Flag = "N";
    local.Position.Count =
      Find(local.ProgramProcessingInfo.ParameterList, "DEBUG=");

    if (local.Position.Count > 0)
    {
      local.Debug.Flag =
        Substring(local.ProgramProcessingInfo.ParameterList,
        local.Position.Count + 6, 1);

      if (AsChar(local.Debug.Flag) == 'Y')
      {
        export.Debug.Flag = "Y";
      }
    }

    // Write program parameters to Control Report
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    for(var limit = local.ReportMax.Count; local.Subscript.Count <= limit; ++
      local.Subscript.Count)
    {
      switch(local.Subscript.Count)
      {
        case 1:
          // Write Debug parameter value to Control Report
          if (AsChar(export.Debug.Flag) == 'Y')
          {
            local.DebugText.Text40 = "YES (RECORDS WILL NOT BE DELETED)";
          }
          else
          {
            local.DebugText.Text40 = "NO (RECORDS WILL BE DELETED)";
          }

          local.EabReportSend.RptDetail = "DEBUG:               " + local
            .DebugText.Text40;

          break;
        case 2:
          // Write Process Date to Control Report
          local.TempDate.Text8 =
            NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
            
          local.EabReportSend.RptDetail = "PROCESS DATE:        " + local
            .TempDate.Text8;

          break;
        case 3:
          // Write Retention Interval to Control Report
          local.EabReportSend.RptDetail = "RETENTION INTERVAL:  " + local
            .RetentionInterval.Text3 + " MONTHS";

          break;
        case 4:
          // Write Purge Date parameter value to Control Report
          local.TempDate.Text8 =
            NumberToString(DateToInt(export.PurgeDate.Date), 8);
          local.EabReportSend.RptDetail = "PURGE DATE:          " + local
            .TempDate.Text8;

          break;
        case 5:
          // Write blank line to Control Report
          local.EabReportSend.RptDetail = "";

          break;
        case 6:
          // Write indicator to Control Report
          local.EabReportSend.RptDetail = "REPORT FORMAT:";

          break;
        case 7:
          // Write first line of report format to Control Report
          local.EabReportSend.RptDetail =
            "<REQUESTING STATE>                        <REQUESTING USER ID>";

          break;
        case 8:
          // Write second line of report format to Control Report
          local.EabReportSend.RptDetail =
            "     <REQ. CASE ID>   <REQUEST TIMESTAMP>";

          break;
        case 9:
          // Write blank line to Control Report
          local.EabReportSend.RptDetail = "";

          break;
        default:
          return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiB750WriteControlsAndClose()
  {
    var useImport = new SiB750WriteControlsAndClose.Import();
    var useExport = new SiB750WriteControlsAndClose.Export();

    useImport.ErrorMessage.RptDetail = local.EabReportSend.RptDetail;

    Call(SiB750WriteControlsAndClose.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
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
    /// A value of PurgeDate.
    /// </summary>
    [JsonPropertyName("purgeDate")]
    public DateWorkArea PurgeDate
    {
      get => purgeDate ??= new();
      set => purgeDate = value;
    }

    private Common debug;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea purgeDate;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of DebugText.
    /// </summary>
    [JsonPropertyName("debugText")]
    public WorkArea DebugText
    {
      get => debugText ??= new();
      set => debugText = value;
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
    /// A value of Numbers.
    /// </summary>
    [JsonPropertyName("numbers")]
    public WorkArea Numbers
    {
      get => numbers ??= new();
      set => numbers = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of ReportMax.
    /// </summary>
    [JsonPropertyName("reportMax")]
    public Common ReportMax
    {
      get => reportMax ??= new();
      set => reportMax = value;
    }

    /// <summary>
    /// A value of RetentionInterval.
    /// </summary>
    [JsonPropertyName("retentionInterval")]
    public WorkArea RetentionInterval
    {
      get => retentionInterval ??= new();
      set => retentionInterval = value;
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

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ProgramProcessingInfo programProcessingInfo;
    private Common debug;
    private WorkArea debugText;
    private DateWorkArea null1;
    private WorkArea numbers;
    private Common position;
    private Common reportMax;
    private WorkArea retentionInterval;
    private Common subscript;
    private WorkArea tempDate;
  }
#endregion
}

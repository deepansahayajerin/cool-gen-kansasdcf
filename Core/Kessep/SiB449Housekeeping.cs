// Program: SI_B449_HOUSEKEEPING, ID: 1625402242, model: 746.
// Short name: SWE00396
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_B449_HOUSEKEEPING.
/// </para>
/// <para>
/// This action block used by Batch Pstep OE_B449_FCR_ALERT_PURGE_PROCESS, sets 
/// default value, opens the required files, read program processing information
/// etc. for the calling Procedure Step.
/// </para>
/// </summary>
[Serializable]
public partial class SiB449Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B449_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB449Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB449Housekeeping.
  /// </summary>
  public SiB449Housekeeping(IContext context, Import import, Export export):
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
    // **                           M A I N T E N A N C E   L O G
    // **
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
    // *
    // *
    // 
    // *
    // ***************************************************************************************
    local.ProgramProcessingInfo.Name = "SWEIB449";
    local.Numbers.Text10 = "0123456789";
    local.ReportMax.Count = 99;

    // ***************************************************************************************
    // *** Retrive Program Processing Inforatmon(PPI) for the given Load Module
    // ***
    // ***************************************************************************************
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***************************************************************************************
    // *** Retrieve checkpoint restart info - commit frequency is determined by 
    // update     ***
    // *** frequency.
    // 
    // ***
    // ***************************************************************************************
    export.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***************************************************************************************
    // *** Open Control Report File
    // 
    // ***
    // ***************************************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ***************************************************************************************
    // *** Open Error Report File
    // 
    // ***
    // ***************************************************************************************
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ***************************************************************************************
    // *** When PPI Date is Null, set the value to current date
    // ***
    // ***************************************************************************************
    if (!Lt(local.Null1.Date, local.ProgramProcessingInfo.ProcessDate))
    {
      local.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // ***************************************************************************************
    // *** Set purge date to processing date minus retention interval, i.e. One 
    // Year       ***
    // *** Calculate Last day of previous month and subtract one year from the 
    // the         ***
    // *** Calculated Date.
    // 
    // ***
    // ***************************************************************************************
    export.PurgeDate.Date =
      IntToDate(Year(local.ProgramProcessingInfo.ProcessDate) * 10000 + Month
      (local.ProgramProcessingInfo.ProcessDate) * 100 + 1);
    export.PurgeDate.Date = AddDays(export.PurgeDate.Date, -1);
    export.PurgeDate.Date = AddYears(export.PurgeDate.Date, -1);

    // ***************************************************************************************
    // *** Only set Debug mode if Debug value is specifically set to Y, 
    // otherwise to N.    ***
    // ***************************************************************************************
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

    // ***************************************************************************************
    // ***                  Write program parameters to Control Report
    // ***
    // ***************************************************************************************
    local.EabFileHandling.Action = "WRITE";
    local.Subscript.Count = 1;

    for(var limit = local.ReportMax.Count; local.Subscript.Count <= limit; ++
      local.Subscript.Count)
    {
      switch(local.Subscript.Count)
      {
        case 1:
          // ***************************************************************************************
          // ***              Write Debug parameter value to Control Report
          // ***
          // ***************************************************************************************
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
          // ***************************************************************************************
          // ***                      Write Process Date to Control Report
          // ***
          // ***************************************************************************************
          local.TempDate.Text8 =
            NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
            
          local.EabReportSend.RptDetail = "PROCESS DATE:        " + local
            .TempDate.Text8;

          break;
        case 3:
          // ***************************************************************************************
          // ***            Write Purge Date parameter value to Control Report
          // ***
          // ***************************************************************************************
          local.TempDate.Text8 =
            NumberToString(DateToInt(export.PurgeDate.Date), 8);
          local.EabReportSend.RptDetail = "PURGE DATE:          " + local
            .TempDate.Text8;

          break;
        case 4:
          // ***************************************************************************************
          // ***                           Blank Line to Control Report
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail = "";

          break;
        case 5:
          // ***************************************************************************************
          // ***                           Blank Line to Control Report
          // ***
          // ***************************************************************************************
          local.EabReportSend.RptDetail = "";

          break;
        case 6:
          // Write indicator to Control Report
          local.EabReportSend.RptDetail = "REPORT FORMAT:";

          break;
        case 7:
          local.RptCaseNumber.Text12 = "Case Number";
          local.RptPersonNumber.Text12 = "Person No";
          local.RptInfId.Text18 = "Infrastructure Id";
          local.ReportReasonCd.Text17 = "Reason Code";
          local.RptRefDate.Text12 = "Ref. Date";
          local.RptRemarks.Text30 = "Process/Exception Information";
          local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
            .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
            .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
            .RptRemarks.Text30;

          break;
        case 8:
          // Write second line of report format to Control Report
          local.RptCaseNumber.Text12 = "----------";
          local.RptPersonNumber.Text12 = "----------";
          local.RptInfId.Text18 = "---------------";
          local.ReportReasonCd.Text17 = "-----------";
          local.RptRefDate.Text12 = "----------";
          local.RptRemarks.Text30 = "------------------------------";
          local.EabReportSend.RptDetail = local.RptCaseNumber.Text12 + local
            .RptPersonNumber.Text12 + local.RptInfId.Text18 + local
            .ReportReasonCd.Text17 + local.RptRefDate.Text12 + local
            .RptRemarks.Text30;

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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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
    /// A value of PurgeDate.
    /// </summary>
    [JsonPropertyName("purgeDate")]
    public DateWorkArea PurgeDate
    {
      get => purgeDate ??= new();
      set => purgeDate = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea purgeDate;
    private Common debug;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Numbers.
    /// </summary>
    [JsonPropertyName("numbers")]
    public WorkArea Numbers
    {
      get => numbers ??= new();
      set => numbers = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of TempDate.
    /// </summary>
    [JsonPropertyName("tempDate")]
    public WorkArea TempDate
    {
      get => tempDate ??= new();
      set => tempDate = value;
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
    /// A value of RptRemarks.
    /// </summary>
    [JsonPropertyName("rptRemarks")]
    public TextWorkArea RptRemarks
    {
      get => rptRemarks ??= new();
      set => rptRemarks = value;
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
    /// A value of ReportReasonCd.
    /// </summary>
    [JsonPropertyName("reportReasonCd")]
    public WorkArea ReportReasonCd
    {
      get => reportReasonCd ??= new();
      set => reportReasonCd = value;
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
    /// A value of RptCaseNumber.
    /// </summary>
    [JsonPropertyName("rptCaseNumber")]
    public WorkArea RptCaseNumber
    {
      get => rptCaseNumber ??= new();
      set => rptCaseNumber = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private WorkArea numbers;
    private Common reportMax;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea null1;
    private WorkArea retentionInterval;
    private Common position;
    private Common debug;
    private WorkArea debugText;
    private WorkArea tempDate;
    private Common subscript;
    private TextWorkArea rptRemarks;
    private WorkArea rptRefDate;
    private WorkArea reportReasonCd;
    private WorkArea rptInfId;
    private WorkArea rptCaseNumber;
    private WorkArea rptPersonNumber;
  }
#endregion
}

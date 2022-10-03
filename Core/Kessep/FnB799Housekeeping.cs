// Program: FN_B799_HOUSEKEEPING, ID: 1625408002, model: 746.
// Short name: SWE00849
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B799_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB799Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B799_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB799Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB799Housekeeping.
  /// </summary>
  public FnB799Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************
    // *Get the run parameters for this program.     *
    // ***********************************************
    local.ProgramProcessingInfo.Name = "SWEFB799";
    local.Current.NumericalYear = Now().Date.Year;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!IsEmpty(export.ProgramProcessingInfo.ParameterList))
    {
      local.BeginDateWorkAttributes.TextMonth =
        Substring(export.ProgramProcessingInfo.ParameterList, 1, 2);
      local.BeginDateWorkAttributes.TextDay =
        Substring(export.ProgramProcessingInfo.ParameterList, 3, 2);
      local.BeginDateWorkAttributes.TextYear =
        Substring(export.ProgramProcessingInfo.ParameterList, 5, 4);
      local.EndDateWorkAttributes.TextMonth =
        Substring(export.ProgramProcessingInfo.ParameterList, 10, 2);
      local.EndDateWorkAttributes.TextDay =
        Substring(export.ProgramProcessingInfo.ParameterList, 12, 2);
      local.EndDateWorkAttributes.TextYear =
        Substring(export.ProgramProcessingInfo.ParameterList, 14, 4);

      if (!IsEmpty(local.BeginDateWorkAttributes.TextMonth) && !
        IsEmpty(local.BeginDateWorkAttributes.TextDay) && !
        IsEmpty(local.BeginDateWorkAttributes.TextYear) && !
        IsEmpty(local.EndDateWorkAttributes.TextMonth) && !
        IsEmpty(local.EndDateWorkAttributes.TextDay) && !
        IsEmpty(local.EndDateWorkAttributes.TextYear))
      {
        local.BeginDateWorkAttributes.NumericalDay =
          (int)StringToNumber(local.BeginDateWorkAttributes.TextDay);
        local.BeginDateWorkAttributes.NumericalMonth =
          (int)StringToNumber(local.BeginDateWorkAttributes.TextMonth);
        local.BeginDateWorkAttributes.NumericalYear =
          (int)StringToNumber(local.BeginDateWorkAttributes.TextYear);
        local.EndDateWorkAttributes.NumericalDay =
          (int)StringToNumber(local.EndDateWorkAttributes.TextDay);
        local.EndDateWorkAttributes.NumericalMonth =
          (int)StringToNumber(local.EndDateWorkAttributes.TextMonth);
        local.EndDateWorkAttributes.NumericalYear =
          (int)StringToNumber(local.EndDateWorkAttributes.TextYear);

        if (local.BeginDateWorkAttributes.NumericalMonth >= 1 && local
          .BeginDateWorkAttributes.NumericalMonth <= 12)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        if (local.BeginDateWorkAttributes.NumericalDay >= 1 && local
          .BeginDateWorkAttributes.NumericalDay <= 31)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        if (local.BeginDateWorkAttributes.NumericalYear >= 1950 && local
          .BeginDateWorkAttributes.NumericalYear <= local
          .Current.NumericalYear)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        if (local.EndDateWorkAttributes.NumericalMonth >= 1 && local
          .EndDateWorkAttributes.NumericalMonth <= 12)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        if (local.EndDateWorkAttributes.NumericalDay >= 1 && local
          .EndDateWorkAttributes.NumericalDay <= 31)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        if (local.EndDateWorkAttributes.NumericalYear >= 1950 && local
          .EndDateWorkAttributes.NumericalYear <= local.Current.NumericalYear)
        {
        }
        else
        {
          ExitState = "ACO_NI0000_INVALID_DATE";

          return;
        }

        local.BeginDate.Text10 = local.BeginDateWorkAttributes.TextMonth + "/"
          + local.BeginDateWorkAttributes.TextDay + "/" + local
          .BeginDateWorkAttributes.TextYear;
        local.EndDate.Text10 = local.EndDateWorkAttributes.TextMonth + "/" + local
          .EndDateWorkAttributes.TextDay + "/" + local
          .EndDateWorkAttributes.TextYear;
        export.BeginDate.Date = StringToDate(local.BeginDate.Text10);
        export.EndDate.Date = StringToDate(local.EndDate.Text10);

        if (!Lt(export.BeginDate.Date, export.EndDate.Date))
        {
          ExitState = "ACO_NE0000_END_LESS_THAN_START";

          return;
        }

        // this is ok, dates passed in
      }
      else
      {
        // no begin or end date passed in
        ExitState = "MUST_HAVE_BEGIN_AND_END_DATE";

        return;
      }
    }
    else
    {
      // no begin date and end date passed in
      ExitState = "MUST_HAVE_BEGIN_AND_END_DATE";

      return;
    }

    switch(Month(export.BeginDate.Date))
    {
      case 1:
        local.MonthBegin.Text12 = "JANUARY";

        break;
      case 2:
        local.MonthBegin.Text12 = "FEBUARY";

        break;
      case 3:
        local.MonthBegin.Text12 = "MARCH";

        break;
      case 4:
        local.MonthBegin.Text12 = "APRIL";

        break;
      case 5:
        local.MonthBegin.Text12 = "MAY";

        break;
      case 6:
        local.MonthBegin.Text12 = "JUNE";

        break;
      case 7:
        local.MonthBegin.Text12 = "JULY";

        break;
      case 8:
        local.MonthBegin.Text12 = "AUGUST";

        break;
      case 9:
        local.MonthBegin.Text12 = "SEPTEMBER";

        break;
      case 10:
        local.MonthBegin.Text12 = "OCTOBER";

        break;
      case 11:
        local.MonthBegin.Text12 = "NOVEMBER";

        break;
      case 12:
        local.MonthBegin.Text12 = "DECEMBER";

        break;
      default:
        break;
    }

    switch(Month(export.EndDate.Date))
    {
      case 1:
        local.MonthEnd.Text12 = "JANUARY";

        break;
      case 2:
        local.MonthEnd.Text12 = "FEBUARY";

        break;
      case 3:
        local.MonthEnd.Text12 = "MARCH";

        break;
      case 4:
        local.MonthEnd.Text12 = "APRIL";

        break;
      case 5:
        local.MonthEnd.Text12 = "MAY";

        break;
      case 6:
        local.MonthEnd.Text12 = "JUNE";

        break;
      case 7:
        local.MonthEnd.Text12 = "JULY";

        break;
      case 8:
        local.MonthEnd.Text12 = "AUGUST";

        break;
      case 9:
        local.MonthEnd.Text12 = "SEPTEMBER";

        break;
      case 10:
        local.MonthEnd.Text12 = "OCTOBER";

        break;
      case 11:
        local.MonthEnd.Text12 = "NOVEMBER";

        break;
      case 12:
        local.MonthEnd.Text12 = "DECEMBER";

        break;
      default:
        break;
    }

    local.BeginWorkArea.Text21 = TrimEnd(local.MonthBegin.Text12) + " " + local
      .BeginDateWorkAttributes.TextDay + ", " + local
      .BeginDateWorkAttributes.TextYear;
    local.EndWorkArea.Text21 = TrimEnd(local.MonthEnd.Text12) + " " + local
      .EndDateWorkAttributes.TextDay + ", " + local
      .EndDateWorkAttributes.TextYear;

    // ************************************************
    // * Call external to OPEN the detail report cab.       *
    // ************************************************
    local.Open.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.Open.BlankLineAfterHeading = "Y";
    local.Open.ProgramName = "SWEFB799";
    local.Open.NumberOfColHeadings = 0;
    local.EabFileHandling.Action = "OPEN";
    UseEabExternalReportWriter1();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered opening 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabReportSend.RptDetail =
      "       ;       ;                             KANSAS DEPARTMENT FOR CHILDREN AND FAMILIES";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "         ;         ;               CHILD SUPPORT SERVICES";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "        ;       ;              Joint Filer Affidavits Received";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "            ;         ;                           " + TrimEnd
      (local.BeginWorkArea.Text21) + " - " + local.EndWorkArea.Text21;
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "    NCP#        ;  Year         ; Cash Receipt Detail #     ;Amount   ;JFA Received        ; CRU Processed    ;";
      
    local.EabFileHandling.Action = "WRITE";
    UseEabExternalReportWriter2();

    if (!Equal(local.PassArea.TextReturnCode, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error encountered writing 1st report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private void UseCabControlReport()
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExternalReportWriter1()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveExternal(local.PassArea, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseEabExternalReportWriter2()
  {
    var useImport = new EabExternalReportWriter.Import();
    var useExport = new EabExternalReportWriter.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.EabReportSend.RptDetail;
    MoveExternal(local.PassArea, useExport.External);

    Call(EabExternalReportWriter.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.PassArea);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of BeginDate.
    /// </summary>
    [JsonPropertyName("beginDate")]
    public DateWorkArea BeginDate
    {
      get => beginDate ??= new();
      set => beginDate = value;
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

    private DateWorkArea endDate;
    private DateWorkArea beginDate;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkAttributes Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of MonthEnd.
    /// </summary>
    [JsonPropertyName("monthEnd")]
    public WorkArea MonthEnd
    {
      get => monthEnd ??= new();
      set => monthEnd = value;
    }

    /// <summary>
    /// A value of EndWorkArea.
    /// </summary>
    [JsonPropertyName("endWorkArea")]
    public WorkArea EndWorkArea
    {
      get => endWorkArea ??= new();
      set => endWorkArea = value;
    }

    /// <summary>
    /// A value of BeginWorkArea.
    /// </summary>
    [JsonPropertyName("beginWorkArea")]
    public WorkArea BeginWorkArea
    {
      get => beginWorkArea ??= new();
      set => beginWorkArea = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public TextWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of BeginDate.
    /// </summary>
    [JsonPropertyName("beginDate")]
    public TextWorkArea BeginDate
    {
      get => beginDate ??= new();
      set => beginDate = value;
    }

    /// <summary>
    /// A value of EndDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("endDateWorkAttributes")]
    public DateWorkAttributes EndDateWorkAttributes
    {
      get => endDateWorkAttributes ??= new();
      set => endDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of BeginDateWorkAttributes.
    /// </summary>
    [JsonPropertyName("beginDateWorkAttributes")]
    public DateWorkAttributes BeginDateWorkAttributes
    {
      get => beginDateWorkAttributes ??= new();
      set => beginDateWorkAttributes = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public External Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public WorkArea Date
    {
      get => date ??= new();
      set => date = value;
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

    /// <summary>
    /// A value of MonthBegin.
    /// </summary>
    [JsonPropertyName("monthBegin")]
    public WorkArea MonthBegin
    {
      get => monthBegin ??= new();
      set => monthBegin = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    public WorkArea Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
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

    private DateWorkAttributes current;
    private WorkArea monthEnd;
    private WorkArea endWorkArea;
    private WorkArea beginWorkArea;
    private TextWorkArea endDate;
    private TextWorkArea beginDate;
    private DateWorkAttributes endDateWorkAttributes;
    private DateWorkAttributes beginDateWorkAttributes;
    private External report;
    private WorkArea date;
    private WorkArea year;
    private WorkArea monthBegin;
    private WorkArea name;
    private EabReportSend open;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
  }
#endregion
}

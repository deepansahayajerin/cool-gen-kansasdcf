// Program: OE_B495_HOUSEKEEPING, ID: 372872049, model: 746.
// Short name: SWE02474
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B495_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class OeB495Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B495_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB495Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB495Housekeeping.
  /// </summary>
  public OeB495Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEEB495";

    if (ReadProgramProcessingInfo())
    {
      export.Process.Date = entities.ProgramProcessingInfo.ProcessDate;
      local.PpiFound.Flag = "Y";
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "ZD_PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    // ************************************************************************
    // * PPI should supply period indicator M=monthly, anything
    // * else defaults to weekly.  For monthly processing, the
    // * process date is used to determine the previous month.
    // * For weekly processing, the process date is the period
    // * end date and the begin date is calculated.
    // ************************************************************************
    if (Equal(entities.ProgramProcessingInfo.ParameterList, "M"))
    {
      // **
      // If PPI Parm List contains an indicator M for monthly, and
      // let's say PPI Process_Date = 07/23/1999, then the period
      // end date will be 06/30/1999.  Only activities within the
      // range 06/01/99 to 06/30/99 will be listed.
      // **
      local.ProcessMonth.Count =
        Month(entities.ProgramProcessingInfo.ProcessDate);
      local.ProcessYear.Count =
        Year(entities.ProgramProcessingInfo.ProcessDate);
      export.BeginPeriod.Date =
        IntToDate((int)((long)local.ProcessYear.Count * 10000 + (
          long)local.ProcessMonth.Count * 100 + 1));
      export.BeginPeriod.Date = AddMonths(export.BeginPeriod.Date, -1);
      export.BeginPeriod.Timestamp =
        Timestamp(NumberToString(Year(export.BeginPeriod.Date), 12, 4) + "-" + NumberToString
        (Month(export.BeginPeriod.Date), 14, 2) + "-" + NumberToString
        (Day(export.BeginPeriod.Date), 14, 2) + "-00.00.00.000001");
      export.PeriodBegin.Text10 =
        NumberToString(Month(export.BeginPeriod.Date), 14, 2) + "/" + NumberToString
        (Day(export.BeginPeriod.Date), 14, 2) + "/" + NumberToString
        (Year(export.BeginPeriod.Date), 12, 4);

      // **********************************************************
      // CALCULATE PERIOD END DATE
      // **********************************************************
      export.EndPeriod.Date = AddMonths(export.BeginPeriod.Date, 1);
      export.EndPeriod.Date = AddDays(export.EndPeriod.Date, -1);
      export.EndPeriod.Timestamp =
        Timestamp(NumberToString(Year(export.EndPeriod.Date), 12, 4) + "-" + NumberToString
        (Month(export.EndPeriod.Date), 14, 2) + "-" + NumberToString
        (Day(export.EndPeriod.Date), 14, 2) + "-23.59.59.999999");
      export.PeriodEnd.Text10 =
        NumberToString(Month(export.EndPeriod.Date), 14, 2) + "/" + NumberToString
        (Day(export.EndPeriod.Date), 14, 2) + "/" + NumberToString
        (Year(export.EndPeriod.Date), 12, 4);

      // **********************************************************
      // CALCULATE REPORTING PERIOD MONTH YEAR
      // **********************************************************
      local.ReportingYear.Count = Year(export.EndPeriod.Date);
      local.ReportingMonth.Count = Month(export.EndPeriod.Date);

      switch(local.ReportingMonth.Count)
      {
        case 1:
          export.ReportingMonthYear.Text30 = "January " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 2:
          export.ReportingMonthYear.Text30 = "February " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 3:
          export.ReportingMonthYear.Text30 = "March " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 4:
          export.ReportingMonthYear.Text30 = "April " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 5:
          export.ReportingMonthYear.Text30 = "May " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 6:
          export.ReportingMonthYear.Text30 = "June " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 7:
          export.ReportingMonthYear.Text30 = "July " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 8:
          export.ReportingMonthYear.Text30 = "August " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 9:
          export.ReportingMonthYear.Text30 = "September " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 10:
          export.ReportingMonthYear.Text30 = "October " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 11:
          export.ReportingMonthYear.Text30 = "November " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        case 12:
          export.ReportingMonthYear.Text30 = "December " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
        default:
          export.ReportingMonthYear.Text30 = "Unknown " + NumberToString
            (local.ReportingYear.Count, 12, 4);

          break;
      }
    }
    else
    {
      // **
      // The default is weekly.  Let's say PPI Process_Date = 07/23/1999,
      // then period end date will be 07/23/1999.  Only activities
      // from 07/17/99 to 07/23/99 will be listed.
      // **
      export.EndPeriod.Date = entities.ProgramProcessingInfo.ProcessDate;
      export.PeriodEnd.Text10 =
        NumberToString(Month(export.EndPeriod.Date), 14, 2) + "/" + NumberToString
        (Day(export.EndPeriod.Date), 14, 2) + "/" + NumberToString
        (Year(export.EndPeriod.Date), 12, 4);
      export.BeginPeriod.Date = AddDays(export.EndPeriod.Date, -6);
      export.PeriodBegin.Text10 =
        NumberToString(Month(export.BeginPeriod.Date), 14, 2) + "/" + NumberToString
        (Day(export.BeginPeriod.Date), 14, 2) + "/" + NumberToString
        (Year(export.BeginPeriod.Date), 12, 4);
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.Process.Date;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT BUSINESS REPORT 01
    // **********************************************************
    local.NeededToOpen.ProgramName = local.ProgramProcessingInfo.Name;
    local.NeededToOpen.ProcessDate = export.Process.Date;

    if (Equal(entities.ProgramProcessingInfo.ParameterList, "M"))
    {
      local.NeededToOpen.RptHeading3 =
        "HEALTH INSURANCE AVAILABILITY ACTIVITY" + " " + export
        .ReportingMonthYear.Text30;
    }
    else
    {
      local.NeededToOpen.RptHeading3 =
        "HEALTH INSURANCE AVAILABILITY ACTIVITY  " + export
        .PeriodBegin.Text10 + " thru " + export.PeriodEnd.Text10;
    }

    local.NeededToOpen.BlankLineAfterHeading = "Y";
    UseCabBusinessReport01();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT FILE EAB
    // **********************************************************
    UseEabWriteHinsAvailabilityChgs();
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToOpen.Assign(local.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabWriteHinsAvailabilityChgs()
  {
    var useImport = new EabWriteHinsAvailabilityChgs.Import();
    var useExport = new EabWriteHinsAvailabilityChgs.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteHinsAvailabilityChgs.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of BeginPeriod.
    /// </summary>
    [JsonPropertyName("beginPeriod")]
    public DateWorkArea BeginPeriod
    {
      get => beginPeriod ??= new();
      set => beginPeriod = value;
    }

    /// <summary>
    /// A value of PeriodBegin.
    /// </summary>
    [JsonPropertyName("periodBegin")]
    public TextWorkArea PeriodBegin
    {
      get => periodBegin ??= new();
      set => periodBegin = value;
    }

    /// <summary>
    /// A value of EndPeriod.
    /// </summary>
    [JsonPropertyName("endPeriod")]
    public DateWorkArea EndPeriod
    {
      get => endPeriod ??= new();
      set => endPeriod = value;
    }

    /// <summary>
    /// A value of PeriodEnd.
    /// </summary>
    [JsonPropertyName("periodEnd")]
    public TextWorkArea PeriodEnd
    {
      get => periodEnd ??= new();
      set => periodEnd = value;
    }

    /// <summary>
    /// A value of ReportingMonthYear.
    /// </summary>
    [JsonPropertyName("reportingMonthYear")]
    public TextWorkArea ReportingMonthYear
    {
      get => reportingMonthYear ??= new();
      set => reportingMonthYear = value;
    }

    private DateWorkArea process;
    private DateWorkArea beginPeriod;
    private TextWorkArea periodBegin;
    private DateWorkArea endPeriod;
    private TextWorkArea periodEnd;
    private TextWorkArea reportingMonthYear;
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
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
    }

    /// <summary>
    /// A value of ProcessMonth.
    /// </summary>
    [JsonPropertyName("processMonth")]
    public Common ProcessMonth
    {
      get => processMonth ??= new();
      set => processMonth = value;
    }

    /// <summary>
    /// A value of ProcessYear.
    /// </summary>
    [JsonPropertyName("processYear")]
    public Common ProcessYear
    {
      get => processYear ??= new();
      set => processYear = value;
    }

    /// <summary>
    /// A value of ReportingYear.
    /// </summary>
    [JsonPropertyName("reportingYear")]
    public Common ReportingYear
    {
      get => reportingYear ??= new();
      set => reportingYear = value;
    }

    /// <summary>
    /// A value of ReportingMonth.
    /// </summary>
    [JsonPropertyName("reportingMonth")]
    public Common ReportingMonth
    {
      get => reportingMonth ??= new();
      set => reportingMonth = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private Common ppiFound;
    private Common processMonth;
    private Common processYear;
    private Common reportingYear;
    private Common reportingMonth;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private EabReportSend neededToOpen;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

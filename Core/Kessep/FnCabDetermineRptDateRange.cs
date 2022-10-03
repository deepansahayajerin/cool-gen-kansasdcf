// Program: FN_CAB_DETERMINE_RPT_DATE_RANGE, ID: 373021392, model: 746.
// Short name: SWE01001
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_DETERMINE_RPT_DATE_RANGE.
/// </para>
/// <para>
/// : This cab sets the export from and to dates.  If a report period is passed 
/// in, that will be used in determining the exported dates.  IF no report
/// period is passed in, the cab passes back timestamp values for the imported
/// dates.
/// If no from date is passed it, it will be defaulted to current date.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabDetermineRptDateRange: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_DETERMINE_RPT_DATE_RANGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabDetermineRptDateRange(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabDetermineRptDateRange.
  /// </summary>
  public FnCabDetermineRptDateRange(IContext context, Import import,
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
    // : This cab sets the export from and to dates.  If a report period is 
    // passed in, that will be used in determining the exported dates.  IF no
    // report period is passed in, the cab passes back timestamp values for the
    // imported dates.
    // If no from date is passed it, it will be defaulted to current date.
    export.From.Date = import.From.Date;
    export.To.Date = import.To.Date;

    if (Equal(import.From.Date, local.Null1.Date))
    {
      local.DateWorkArea.Date = Now().Date;
      export.From.Date = Now().Date;
    }
    else
    {
      local.DateWorkArea.Date = import.From.Date;
    }

    if (!IsEmpty(import.ReportPeriod.Text10))
    {
      local.DateWorkArea.Year = Year(local.DateWorkArea.Date);
      local.DateWorkArea.Month = Month(local.DateWorkArea.Date);

      switch(TrimEnd(import.ReportPeriod.Text10))
      {
        case "QUARTER":
          if (local.DateWorkArea.Month >= 1 && local.DateWorkArea.Month <= 3)
          {
            export.From.Date = IntToDate((int)((local.DateWorkArea.Year - 1) * (
              long)10000 + 1001));
          }
          else if (local.DateWorkArea.Month >= 4 && local
            .DateWorkArea.Month <= 6)
          {
            export.From.Date = IntToDate(local.DateWorkArea.Year * 10000 + 101);
          }
          else if (local.DateWorkArea.Month >= 7 && local
            .DateWorkArea.Month <= 9)
          {
            export.From.Date = IntToDate(local.DateWorkArea.Year * 10000 + 401);
          }
          else
          {
            export.From.Date = IntToDate(local.DateWorkArea.Year * 10000 + 701);
          }

          export.To.Date = AddDays(AddMonths(export.From.Date, 3), -1);

          break;
        case "MONTH":
          export.From.Date = AddMonths(IntToDate(local.DateWorkArea.Year * 10000
            + local.DateWorkArea.Month * 100 + 1), -1);
          export.To.Date = AddDays(AddMonths(export.From.Date, 1), -1);

          break;
        case "YEAR":
          export.From.Date = IntToDate(Year(AddYears(export.From.Date, -1)) * 10000
            + 101);
          export.To.Date = IntToDate(Year(export.From.Date) * 10000 + 1231);

          break;
        case "YEARTODATE":
          export.From.Date = IntToDate(local.DateWorkArea.Year * 10000 + 101);
          export.To.Date = Now().Date;

          break;
        default:
          break;
      }
    }

    // :   SET TIMESTAMP VIEWS.
    local.TextMm.Text2 = NumberToString(Month(export.From.Date), 14, 2);
    local.TextDd.Text2 = NumberToString(Day(export.From.Date), 14, 2);
    local.TextYyyy.Text4 = NumberToString(Year(export.From.Date), 4);
    export.From.Timestamp = Timestamp(local.TextYyyy.Text4 + "-" + local
      .TextMm.Text2 + "-" + local.TextDd.Text2 + "-00.00.00.000001");

    if (Equal(export.To.Date, local.Null1.Date))
    {
      return;
    }

    local.TextMm.Text2 = NumberToString(Month(export.To.Date), 14, 2);
    local.TextDd.Text2 = NumberToString(Day(export.To.Date), 14, 2);
    local.TextYyyy.Text4 = NumberToString(Year(export.To.Date), 4);
    export.To.Timestamp = Timestamp(local.TextYyyy.Text4 + "-" + local
      .TextMm.Text2 + "-" + local.TextDd.Text2 + "-23.59.59.999999");
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
    /// <summary>
    /// A value of ReportPeriod.
    /// </summary>
    [JsonPropertyName("reportPeriod")]
    public WorkArea ReportPeriod
    {
      get => reportPeriod ??= new();
      set => reportPeriod = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    private WorkArea reportPeriod;
    private DateWorkArea from;
    private DateWorkArea to;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    private DateWorkArea from;
    private DateWorkArea to;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ParmFrom.
    /// </summary>
    [JsonPropertyName("parmFrom")]
    public DateWorkArea ParmFrom
    {
      get => parmFrom ??= new();
      set => parmFrom = value;
    }

    /// <summary>
    /// A value of ReportPeriod.
    /// </summary>
    [JsonPropertyName("reportPeriod")]
    public WorkArea ReportPeriod
    {
      get => reportPeriod ??= new();
      set => reportPeriod = value;
    }

    /// <summary>
    /// A value of TextMm.
    /// </summary>
    [JsonPropertyName("textMm")]
    public TextWorkArea TextMm
    {
      get => textMm ??= new();
      set => textMm = value;
    }

    /// <summary>
    /// A value of TextDd.
    /// </summary>
    [JsonPropertyName("textDd")]
    public TextWorkArea TextDd
    {
      get => textDd ??= new();
      set => textDd = value;
    }

    /// <summary>
    /// A value of TextYyyy.
    /// </summary>
    [JsonPropertyName("textYyyy")]
    public TextWorkArea TextYyyy
    {
      get => textYyyy ??= new();
      set => textYyyy = value;
    }

    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private DateWorkArea parmFrom;
    private WorkArea reportPeriod;
    private TextWorkArea textMm;
    private TextWorkArea textDd;
    private TextWorkArea textYyyy;
  }
#endregion
}

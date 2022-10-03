// Program: OE_GET_MONTH_START_AND_END_DATE, ID: 372632549, model: 746.
// Short name: SWE00060
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GET_MONTH_START_AND_END_DATE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block sets the start and end dates for a given processing
/// month and year.
/// </para>
/// </summary>
[Serializable]
public partial class OeGetMonthStartAndEndDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GET_MONTH_START_AND_END_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGetMonthStartAndEndDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGetMonthStartAndEndDate.
  /// </summary>
  public OeGetMonthStartAndEndDate(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block returns the start and end dates of a given year and 
    // month.
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	05/16/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	05/16/95			Initial coding
    // *********************************************
    // 	
    export.InvalidMonth.Flag = "N";
    local.Year.Count = import.DateWorkArea.YearMonth / 100;
    local.Month.Count = (int)(import.DateWorkArea.YearMonth - (
      long)local.Year.Count * 100);

    if (local.Month.Count < 1 || local.Month.Count > 12)
    {
      export.InvalidMonth.Flag = "Y";

      return;
    }

    export.MonthStartDate.Date = IntToDate(import.DateWorkArea.YearMonth * 100
      + 1);
    export.MonthEndDate.Date =
      AddDays(AddMonths(export.MonthStartDate.Date, 1), -1);
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InvalidMonth.
    /// </summary>
    [JsonPropertyName("invalidMonth")]
    public Common InvalidMonth
    {
      get => invalidMonth ??= new();
      set => invalidMonth = value;
    }

    /// <summary>
    /// A value of MonthEndDate.
    /// </summary>
    [JsonPropertyName("monthEndDate")]
    public DateWorkArea MonthEndDate
    {
      get => monthEndDate ??= new();
      set => monthEndDate = value;
    }

    /// <summary>
    /// A value of MonthStartDate.
    /// </summary>
    [JsonPropertyName("monthStartDate")]
    public DateWorkArea MonthStartDate
    {
      get => monthStartDate ??= new();
      set => monthStartDate = value;
    }

    private Common invalidMonth;
    private DateWorkArea monthEndDate;
    private DateWorkArea monthStartDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public Common Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of Year.
    /// </summary>
    [JsonPropertyName("year")]
    public Common Year
    {
      get => year ??= new();
      set => year = value;
    }

    private Common month;
    private Common year;
  }
#endregion
}

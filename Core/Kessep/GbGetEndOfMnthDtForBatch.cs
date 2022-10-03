// Program: GB_GET_END_OF_MNTH_DT_FOR_BATCH, ID: 372697184, model: 746.
// Short name: SWE00697
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GB_GET_END_OF_MNTH_DT_FOR_BATCH.
/// </para>
/// <para>
/// This action diagram will determine the end of month date needed specifically
/// for batch processing.
/// </para>
/// </summary>
[Serializable]
public partial class GbGetEndOfMnthDtForBatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_GET_END_OF_MNTH_DT_FOR_BATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbGetEndOfMnthDtForBatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbGetEndOfMnthDtForBatch.
  /// </summary>
  public GbGetEndOfMnthDtForBatch(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.CurrentYear.Year = Year(import.Current.Date);
    local.Text.TextDate = NumberToString(DateToInt(import.Current.Date), 8);
    local.Common.TotalInteger =
      StringToNumber(Substring(
        local.Text.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) + "01");
    local.FirstDayOfMonth.Date = IntToDate((int)local.Common.TotalInteger);
    export.FirstDayOfMonth.Date = local.FirstDayOfMonth.Date;
    local.EndOfMonth.Date = AddDays(local.FirstDayOfMonth.Date, -1);

    if (Month(import.Current.Date) == 1 || Month(import.Current.Date) == 3 || Month
      (import.Current.Date) == 5 || Month(import.Current.Date) == 7 || Month
      (import.Current.Date) == 8 || Month(import.Current.Date) == 10 || Month
      (import.Current.Date) == 12)
    {
      if (Day(import.Current.Date) == 31)
      {
        export.EndOfMonth.Date = import.Current.Date;
      }
      else
      {
        export.EndOfMonth.Date = local.EndOfMonth.Date;
      }
    }
    else if (Month(import.Current.Date) == 4 || Month(import.Current.Date) == 6
      || Month(import.Current.Date) == 9 || Month(import.Current.Date) == 11)
    {
      if (Day(import.Current.Date) == 30)
      {
        export.EndOfMonth.Date = import.Current.Date;
      }
      else
      {
        export.EndOfMonth.Date = local.EndOfMonth.Date;
      }
    }
    else if (Month(import.Current.Date) == 2)
    {
      if (local.CurrentYear.Year == 2000 || local.CurrentYear.Year == 2004 || local
        .CurrentYear.Year == 2008 || local.CurrentYear.Year == 2012 || local
        .CurrentYear.Year == 2016 || local.CurrentYear.Year == 2020 || local
        .CurrentYear.Year == 2024 || local.CurrentYear.Year == 2028 || local
        .CurrentYear.Year == 2032 || local.CurrentYear.Year == 2036 || local
        .CurrentYear.Year == 2040 || local.CurrentYear.Year == 2044 || local
        .CurrentYear.Year == 2048 || local.CurrentYear.Year == 2052 || local
        .CurrentYear.Year == 2056 || local.CurrentYear.Year == 2060)
      {
        if (Day(import.Current.Date) == 29)
        {
          export.EndOfMonth.Date = import.Current.Date;
        }
        else
        {
          export.EndOfMonth.Date = local.EndOfMonth.Date;
        }
      }
      else if (Day(import.Current.Date) == 28)
      {
        export.EndOfMonth.Date = import.Current.Date;
      }
      else
      {
        export.EndOfMonth.Date = local.EndOfMonth.Date;
      }
    }
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
    }

    /// <summary>
    /// A value of EndOfMonth.
    /// </summary>
    [JsonPropertyName("endOfMonth")]
    public DateWorkArea EndOfMonth
    {
      get => endOfMonth ??= new();
      set => endOfMonth = value;
    }

    private DateWorkArea firstDayOfMonth;
    private DateWorkArea endOfMonth;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentYear.
    /// </summary>
    [JsonPropertyName("currentYear")]
    public DateWorkArea CurrentYear
    {
      get => currentYear ??= new();
      set => currentYear = value;
    }

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
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
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public DateWorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of EndOfMonth.
    /// </summary>
    [JsonPropertyName("endOfMonth")]
    public DateWorkArea EndOfMonth
    {
      get => endOfMonth ??= new();
      set => endOfMonth = value;
    }

    private DateWorkArea currentYear;
    private DateWorkArea firstDayOfMonth;
    private Common common;
    private DateWorkArea text;
    private DateWorkArea endOfMonth;
  }
#endregion
}

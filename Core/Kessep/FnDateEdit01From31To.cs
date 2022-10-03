// Program: FN_DATE_EDIT_01FROM_31TO, ID: 371738801, model: 746.
// Short name: SWE00389
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DATE_EDIT_01FROM_31TO.
/// </para>
/// <para>
/// RESP: FINANCE
/// This AB will verify the FROM and TO search dates. Input is expected to be 
/// from a screen with edit pattern of MMYYYY.  Therefore DD will always be 01.
/// If the dates are blank, defaults will be set as follows:
///   FROM DATE - Set to beginning of month before current month
///   TO DATE   - Set to end of current month.
/// If TO DATE is entered, set DD to end of month entered.
/// TO DATE in this AB can be greater than current date.
/// </para>
/// </summary>
[Serializable]
public partial class FnDateEdit01From31To: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DATE_EDIT_01FROM_31TO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDateEdit01From31To(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDateEdit01From31To.
  /// </summary>
  public FnDateEdit01From31To(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.SearchFrom.Date = import.SearchFrom.Date;
    export.SearchTo.Date = import.SearchTo.Date;

    // If from date is blank, set it to beginning of month previous to current 
    // month
    if (Equal(export.SearchFrom.Date, local.Null1.Date))
    {
      local.Date.TextDate =
        NumberToString(DateToInt(Now().Date.AddMonths(-1)), 8, 8);
      local.Date.TextDate =
        Substring(local.Date.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
        "01";
      export.SearchFrom.Date =
        IntToDate((int)StringToNumber(local.Date.TextDate));
    }

    // If to date is blank, set it to end of current month
    if (Equal(export.SearchTo.Date, local.Null1.Date))
    {
      local.Date.TextDate = NumberToString(DateToInt(Now().Date), 8, 8);
      local.Date.TextDate =
        Substring(local.Date.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
        "01";
      export.SearchTo.Date =
        IntToDate((int)StringToNumber(local.Date.TextDate));
      export.SearchTo.Date = AddDays(AddMonths(export.SearchTo.Date, 1), -1);
    }
    else
    {
      // Set DD to end of month entered
      if (Day(import.SearchTo.Date) == 1)
      {
        export.SearchTo.Date = AddDays(AddMonths(import.SearchTo.Date, 1), -1);
      }
    }

    if (Lt(export.SearchTo.Date, export.SearchFrom.Date))
    {
      ExitState = "FROM_DATE_GREATER_THAN_TO_DATE";
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
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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

    private DateWorkArea date;
    private DateWorkArea null1;
  }
#endregion
}

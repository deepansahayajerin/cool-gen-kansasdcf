// Program: CAB_GET_YEAR_MONTH_FROM_DATE, ID: 371737567, model: 746.
// Short name: SWE00064
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_GET_YEAR_MONTH_FROM_DATE.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will return the year and month (ccyymm) number from a date
/// passed to it.
/// </para>
/// </summary>
[Serializable]
public partial class CabGetYearMonthFromDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GET_YEAR_MONTH_FROM_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGetYearMonthFromDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGetYearMonthFromDate.
  /// </summary>
  public CabGetYearMonthFromDate(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(import.DateWorkArea.Date, local.Null1.Date))
    {
      local.Work.Date = Now().Date;
    }
    else
    {
      local.Work.Date = import.DateWorkArea.Date;
    }

    export.DateWorkArea.YearMonth = Year(local.Work.Date) * 100 + Month
      (local.Work.Date);
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public DateWorkArea Work
    {
      get => work ??= new();
      set => work = value;
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

    private DateWorkArea work;
    private DateWorkArea null1;
  }
#endregion
}

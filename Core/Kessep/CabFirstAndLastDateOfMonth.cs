// Program: CAB_FIRST_AND_LAST_DATE_OF_MONTH, ID: 371753807, model: 746.
// Short name: SWE00048
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_FIRST_AND_LAST_DATE_OF_MONTH.
/// </para>
/// <para>
/// This CAB returns the first and the last date of the month for a given date
/// </para>
/// </summary>
[Serializable]
public partial class CabFirstAndLastDateOfMonth: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FIRST_AND_LAST_DATE_OF_MONTH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFirstAndLastDateOfMonth(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFirstAndLastDateOfMonth.
  /// </summary>
  public CabFirstAndLastDateOfMonth(IContext context, Import import,
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
    // **** Getting the first date of the month ****
    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(import.DateWorkArea.Date), 8);
    local.Common.TotalInteger =
      StringToNumber(Substring(
        local.DateWorkArea.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
      "01");
    export.First.Date = IntToDate((int)local.Common.TotalInteger);

    // **** Getting the last date of the month ****
    export.Last.Date = AddDays(AddMonths(export.First.Date, 1), -1);
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
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public DateWorkArea First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public DateWorkArea Last
    {
      get => last ??= new();
      set => last = value;
    }

    private DateWorkArea first;
    private DateWorkArea last;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private Common common;
    private DateWorkArea dateWorkArea;
  }
#endregion
}

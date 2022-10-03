// Program: CAB_DETERMINE_REPORTING_DATES, ID: 372956508, model: 746.
// Short name: SWE00014
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_DETERMINE_REPORTING_DATES.
/// </para>
/// <para>
/// This CAB uses the import date and set up the reporting date to be from the 
/// first of the previous month to the last day of the previous month.
/// </para>
/// </summary>
[Serializable]
public partial class CabDetermineReportingDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_DETERMINE_REPORTING_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabDetermineReportingDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabDetermineReportingDates.
  /// </summary>
  public CabDetermineReportingDates(IContext context, Import import,
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
    local.Bom.Date = AddMonths(import.DateWorkArea.Date, -1);
    local.Current.Year = Year(local.Bom.Date);
    local.Current.Month = Month(local.Bom.Date);
    local.Current.Day = 1;
    local.DateNbr.Count = local.Current.Year * 10000 + local.Current.Month * 100
      + local.Current.Day;
    local.Bom.Date = IntToDate(local.DateNbr.Count);
    local.BatchTimestampWorkArea.TextTimestamp =
      NumberToString(local.Current.Year, 12, 4) + "-" + NumberToString
      (local.Current.Month, 14, 2) + "-";
    local.BatchTimestampWorkArea.TextTimestamp =
      TrimEnd(local.BatchTimestampWorkArea.TextTimestamp) + NumberToString
      (local.Current.Day, 14, 2) + "-00.00.00.000000";
    local.Bom.Timestamp = Timestamp(local.BatchTimestampWorkArea.TextTimestamp);
    local.Eom.Date = AddDays(AddMonths(local.Bom.Date, 1), -1);
    local.Current.Year = Year(local.Eom.Date);
    local.Current.Month = Month(local.Eom.Date);
    local.Current.Day = Day(local.Eom.Date);
    local.BatchTimestampWorkArea.TextTimestamp =
      NumberToString(local.Current.Year, 12, 4) + "-" + NumberToString
      (local.Current.Month, 14, 2) + "-";
    local.BatchTimestampWorkArea.TextTimestamp =
      TrimEnd(local.BatchTimestampWorkArea.TextTimestamp) + NumberToString
      (local.Current.Day, 14, 2) + "-23.59.59.999999";
    local.Eom.Timestamp = Timestamp(local.BatchTimestampWorkArea.TextTimestamp);
    MoveDateWorkArea(local.Bom, export.Bom);
    MoveDateWorkArea(local.Eom, export.Eom);
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    private DateWorkArea eom;
    private DateWorkArea bom;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of DateNbr.
    /// </summary>
    [JsonPropertyName("dateNbr")]
    public Common DateNbr
    {
      get => dateNbr ??= new();
      set => dateNbr = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    private DateWorkArea bom;
    private DateWorkArea current;
    private Common dateNbr;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private DateWorkArea eom;
  }
#endregion
}

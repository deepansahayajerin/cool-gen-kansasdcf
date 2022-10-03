// Program: FN_BUILD_REPORT_TIMEFRAME, ID: 945117374, model: 746.
// Short name: SWE03084
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BUILD_REPORT_TIMEFRAME.
/// </summary>
[Serializable]
public partial class FnBuildReportTimeframe: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BUILD_REPORT_TIMEFRAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBuildReportTimeframe(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBuildReportTimeframe.
  /// </summary>
  public FnBuildReportTimeframe(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Start.Date = import.Start.Date;
    export.End.Date = import.End.Date;

    // -----------------------------------------------------------------------------
    // -- Build starting date/timestamp info
    // -----------------------------------------------------------------------------
    export.Start.Time = new TimeSpan(7, 0, 0);
    UseFnBuildTimestampFrmDateTime1();
    export.Start.Day = Day(export.Start.Date);
    export.Start.Month = Month(export.Start.Date);
    export.Start.Year = Year(export.Start.Date);
    export.Start.YearMonth = export.Start.Year * 100 + export.Start.Month;

    // -- Text date format is MMDDYYYY
    export.Start.TextDate = NumberToString(export.Start.Month, 14, 2) + NumberToString
      (export.Start.Day, 14, 2) + NumberToString(export.Start.Year, 12, 4);

    // -----------------------------------------------------------------------------
    // -- Build ending date/timestamp info
    // -----------------------------------------------------------------------------
    export.End.Time = new TimeSpan(7, 0, 0);
    UseFnBuildTimestampFrmDateTime2();

    // -- Set timestamp to 23.59.59.999999
    export.End.Timestamp =
      AddMicroseconds(AddDays(export.End.Timestamp, 1), -1);
    export.End.Time = TimeOfDay(export.End.Timestamp).GetValueOrDefault();
    export.End.Day = Day(export.End.Date);
    export.End.Month = Month(export.End.Date);
    export.End.Year = Year(export.End.Date);
    export.End.YearMonth = export.End.Year * 100 + export.End.Month;

    // -- Text date format is MMDDYYYY
    export.End.TextDate = NumberToString(export.End.Month, 14, 2) + NumberToString
      (export.End.Day, 14, 2) + NumberToString(export.End.Year, 12, 4);
  }

  private static void MoveDateWorkArea1(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveDateWorkArea2(DateWorkArea source, DateWorkArea target)
    
  {
    target.Date = source.Date;
    target.Time = source.Time;
    target.Timestamp = source.Timestamp;
  }

  private void UseFnBuildTimestampFrmDateTime1()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(export.Start, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, export.Start);
  }

  private void UseFnBuildTimestampFrmDateTime2()
  {
    var useImport = new FnBuildTimestampFrmDateTime.Import();
    var useExport = new FnBuildTimestampFrmDateTime.Export();

    MoveDateWorkArea1(export.End, useImport.DateWorkArea);

    Call(FnBuildTimestampFrmDateTime.Execute, useImport, useExport);

    MoveDateWorkArea2(useExport.DateWorkArea, export.End);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    private DateWorkArea start;
    private DateWorkArea end;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    private DateWorkArea start;
    private DateWorkArea end;
  }
#endregion
}

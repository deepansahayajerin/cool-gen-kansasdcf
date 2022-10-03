// Program: FN_BUILD_TIMESTAMP_FRM_DATE_TIME, ID: 371094737, model: 746.
// Short name: SWE02911
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BUILD_TIMESTAMP_FRM_DATE_TIME.
/// </summary>
[Serializable]
public partial class FnBuildTimestampFrmDateTime: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BUILD_TIMESTAMP_FRM_DATE_TIME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBuildTimestampFrmDateTime(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBuildTimestampFrmDateTime.
  /// </summary>
  public FnBuildTimestampFrmDateTime(IContext context, Import import,
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
    // -----------------------------------------------------------
    // Import Date is mandatory. Time is optional. CAB will return
    // timestamp view.
    // -----------------------------------------------------------
    MoveDateWorkArea(import.DateWorkArea, export.DateWorkArea);

    // -----------------------------------------------------------
    // Desired format for timestamp function YYYY-MM-DD-HH.MI.SS
    // -----------------------------------------------------------
    local.DateWorkAttributes.TextYear =
      NumberToString(DateToInt(import.DateWorkArea.Date), 8, 4);
    local.DateWorkAttributes.TextMonth =
      NumberToString(DateToInt(import.DateWorkArea.Date), 12, 2);
    local.DateWorkAttributes.TextDay =
      NumberToString(DateToInt(import.DateWorkArea.Date), 14, 2);
    local.DateWorkAttributes.TextDate10Char =
      local.DateWorkAttributes.TextYear + "-" + local
      .DateWorkAttributes.TextMonth + "-" + local.DateWorkAttributes.TextDay;
    local.Hour.Text2 =
      NumberToString(TimeToInt(import.DateWorkArea.Time), 10, 2);
    local.Min.Text2 =
      NumberToString(TimeToInt(import.DateWorkArea.Time), 12, 2);
    local.Sec.Text2 =
      NumberToString(TimeToInt(import.DateWorkArea.Time), 14, 2);
    local.Time.Text8 = local.Hour.Text2 + "." + local.Min.Text2 + "." + local
      .Sec.Text2;
    export.DateWorkArea.Timestamp =
      Timestamp(local.DateWorkAttributes.TextDate10Char + "-" + local.Time.Text8);
      
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
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
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Sec.
    /// </summary>
    [JsonPropertyName("sec")]
    public TextWorkArea Sec
    {
      get => sec ??= new();
      set => sec = value;
    }

    /// <summary>
    /// A value of Min.
    /// </summary>
    [JsonPropertyName("min")]
    public TextWorkArea Min
    {
      get => min ??= new();
      set => min = value;
    }

    /// <summary>
    /// A value of Hour.
    /// </summary>
    [JsonPropertyName("hour")]
    public TextWorkArea Hour
    {
      get => hour ??= new();
      set => hour = value;
    }

    /// <summary>
    /// A value of DateWorkAttributes.
    /// </summary>
    [JsonPropertyName("dateWorkAttributes")]
    public DateWorkAttributes DateWorkAttributes
    {
      get => dateWorkAttributes ??= new();
      set => dateWorkAttributes = value;
    }

    private TextWorkArea time;
    private TextWorkArea sec;
    private TextWorkArea min;
    private TextWorkArea hour;
    private DateWorkAttributes dateWorkAttributes;
  }
#endregion
}

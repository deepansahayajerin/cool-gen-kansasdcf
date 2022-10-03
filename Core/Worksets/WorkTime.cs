// The source file: WORK_TIME, ID: 371794186, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// </summary>
[Serializable]
public partial class WorkTime: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkTime()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkTime(WorkTime that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkTime Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WorkTime that)
  {
    base.Assign(that);
    wtime = that.wtime;
    timeWithAmPm = that.timeWithAmPm;
    hh24 = that.hh24;
    mi = that.mi;
    ss = that.ss;
  }

  /// <summary>
  /// The value of the WTIME attribute.
  /// </summary>
  [JsonPropertyName("wtime")]
  [Member(Index = 1, Type = MemberType.Time)]
  public TimeSpan Wtime
  {
    get => wtime;
    set => wtime = value;
  }

  /// <summary>Length of the TIME_WITH_AM_PM attribute.</summary>
  public const int TimeWithAmPm_MaxLength = 5;

  /// <summary>
  /// The value of the TIME_WITH_AM_PM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = TimeWithAmPm_MaxLength)]
  public string TimeWithAmPm
  {
    get => timeWithAmPm ?? "";
    set => timeWithAmPm = TrimEnd(Substring(value, 1, TimeWithAmPm_MaxLength));
  }

  /// <summary>
  /// The json value of the TimeWithAmPm attribute.</summary>
  [JsonPropertyName("timeWithAmPm")]
  [Computed]
  public string TimeWithAmPm_Json
  {
    get => NullIf(TimeWithAmPm, "");
    set => TimeWithAmPm = value;
  }

  /// <summary>
  /// The value of the HH24 attribute.
  /// </summary>
  [JsonPropertyName("hh24")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int Hh24
  {
    get => hh24;
    set => hh24 = value;
  }

  /// <summary>
  /// The value of the MI attribute.
  /// </summary>
  [JsonPropertyName("mi")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 2)]
  public int Mi
  {
    get => mi;
    set => mi = value;
  }

  /// <summary>
  /// The value of the SS attribute.
  /// </summary>
  [JsonPropertyName("ss")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int Ss
  {
    get => ss;
    set => ss = value;
  }

  private TimeSpan wtime;
  private string timeWithAmPm;
  private int hh24;
  private int mi;
  private int ss;
}

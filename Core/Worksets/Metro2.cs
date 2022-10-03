// The source file: METRO2, ID: 1902630907, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Metro2: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Metro2()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Metro2(Metro2 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Metro2 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Metro2 that)
  {
    base.Assign(that);
    baseRecordCount = that.baseRecordCount;
    blockCount = that.blockCount;
    statusDaCount = that.statusDaCount;
    status11Count = that.status11Count;
    status13Count = that.status13Count;
    status64Count = that.status64Count;
    status71Count = that.status71Count;
    status93Count = that.status93Count;
    ssnAllSegmentsCount = that.ssnAllSegmentsCount;
    ssnBaseSegmentCount = that.ssnBaseSegmentCount;
    dobAllSegmentsCount = that.dobAllSegmentsCount;
    dobBaseSegmentCount = that.dobBaseSegmentCount;
  }

  /// <summary>
  /// The value of the BASE_RECORD_COUNT attribute.
  /// </summary>
  [JsonPropertyName("baseRecordCount")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int BaseRecordCount
  {
    get => baseRecordCount;
    set => baseRecordCount = value;
  }

  /// <summary>
  /// The value of the BLOCK_COUNT attribute.
  /// </summary>
  [JsonPropertyName("blockCount")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int BlockCount
  {
    get => blockCount;
    set => blockCount = value;
  }

  /// <summary>
  /// The value of the STATUS_DA_COUNT attribute.
  /// </summary>
  [JsonPropertyName("statusDaCount")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int StatusDaCount
  {
    get => statusDaCount;
    set => statusDaCount = value;
  }

  /// <summary>
  /// The value of the STATUS_11_COUNT attribute.
  /// </summary>
  [JsonPropertyName("status11Count")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int Status11Count
  {
    get => status11Count;
    set => status11Count = value;
  }

  /// <summary>
  /// The value of the STATUS_13_COUNT attribute.
  /// </summary>
  [JsonPropertyName("status13Count")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int Status13Count
  {
    get => status13Count;
    set => status13Count = value;
  }

  /// <summary>
  /// The value of the STATUS_64_COUNT attribute.
  /// </summary>
  [JsonPropertyName("status64Count")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int Status64Count
  {
    get => status64Count;
    set => status64Count = value;
  }

  /// <summary>
  /// The value of the STATUS_71_COUNT attribute.
  /// </summary>
  [JsonPropertyName("status71Count")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 9)]
  public int Status71Count
  {
    get => status71Count;
    set => status71Count = value;
  }

  /// <summary>
  /// The value of the STATUS_93_COUNT attribute.
  /// </summary>
  [JsonPropertyName("status93Count")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int Status93Count
  {
    get => status93Count;
    set => status93Count = value;
  }

  /// <summary>
  /// The value of the SSN_ALL_SEGMENTS_COUNT attribute.
  /// </summary>
  [JsonPropertyName("ssnAllSegmentsCount")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int SsnAllSegmentsCount
  {
    get => ssnAllSegmentsCount;
    set => ssnAllSegmentsCount = value;
  }

  /// <summary>
  /// The value of the SSN_BASE_SEGMENT_COUNT attribute.
  /// </summary>
  [JsonPropertyName("ssnBaseSegmentCount")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int SsnBaseSegmentCount
  {
    get => ssnBaseSegmentCount;
    set => ssnBaseSegmentCount = value;
  }

  /// <summary>
  /// The value of the DOB_ALL_SEGMENTS_COUNT attribute.
  /// </summary>
  [JsonPropertyName("dobAllSegmentsCount")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int DobAllSegmentsCount
  {
    get => dobAllSegmentsCount;
    set => dobAllSegmentsCount = value;
  }

  /// <summary>
  /// The value of the DOB_BASE_SEGMENT_COUNT attribute.
  /// </summary>
  [JsonPropertyName("dobBaseSegmentCount")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 9)]
  public int DobBaseSegmentCount
  {
    get => dobBaseSegmentCount;
    set => dobBaseSegmentCount = value;
  }

  private int baseRecordCount;
  private int blockCount;
  private int statusDaCount;
  private int status11Count;
  private int status13Count;
  private int status64Count;
  private int status71Count;
  private int status93Count;
  private int ssnAllSegmentsCount;
  private int ssnBaseSegmentCount;
  private int dobAllSegmentsCount;
  private int dobBaseSegmentCount;
}

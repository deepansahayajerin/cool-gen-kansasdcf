// The source file: EXTERNAL_FPLS_HEADER, ID: 372367166, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp:OBLGEST
/// This IEF Work Set describes the Heading Information needed to precede detail
/// records for external transmission to the Federal Parent Locator Service.
/// </summary>
[Serializable]
public partial class ExternalFplsHeader: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFplsHeader()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFplsHeader(ExternalFplsHeader that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFplsHeader Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFplsHeader that)
  {
    base.Assign(that);
    spaces1 = that.spaces1;
    stateCode = that.stateCode;
    stationNumber = that.stationNumber;
    spaces2 = that.spaces2;
    fplsHeaderConstant = that.fplsHeaderConstant;
    totalResponses = that.totalResponses;
    fplsSortCode = that.fplsSortCode;
    mmDateGenerated = that.mmDateGenerated;
    ddDateGenerated = that.ddDateGenerated;
    yyDateGenerated = that.yyDateGenerated;
    totalResponsesState = that.totalResponsesState;
    spaces3 = that.spaces3;
    spaces4 = that.spaces4;
  }

  /// <summary>Length of the SPACES_1 attribute.</summary>
  public const int Spaces1_MaxLength = 16;

  /// <summary>
  /// The value of the SPACES_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Spaces1_MaxLength)]
  public string Spaces1
  {
    get => spaces1 ?? "";
    set => spaces1 = TrimEnd(Substring(value, 1, Spaces1_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces1 attribute.</summary>
  [JsonPropertyName("spaces1")]
  [Computed]
  public string Spaces1_Json
  {
    get => NullIf(Spaces1, "");
    set => Spaces1 = value;
  }

  /// <summary>Length of the STATE_CODE attribute.</summary>
  public const int StateCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// Returned by IRS
  /// State supplied by 1099. Could be blank.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = StateCode_MaxLength)]
  public string StateCode
  {
    get => stateCode ?? "";
    set => stateCode = TrimEnd(Substring(value, 1, StateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StateCode attribute.</summary>
  [JsonPropertyName("stateCode")]
  [Computed]
  public string StateCode_Json
  {
    get => NullIf(StateCode, "");
    set => StateCode = value;
  }

  /// <summary>Length of the STATION_NUMBER attribute.</summary>
  public const int StationNumber_MaxLength = 2;

  /// <summary>
  /// The value of the STATION_NUMBER attribute.
  /// Station No of the sending unit. Assigned by FPLS. Required for known and 
  /// unknown cases.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = StationNumber_MaxLength)]
  public string StationNumber
  {
    get => stationNumber ?? "";
    set => stationNumber =
      TrimEnd(Substring(value, 1, StationNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the StationNumber attribute.</summary>
  [JsonPropertyName("stationNumber")]
  [Computed]
  public string StationNumber_Json
  {
    get => NullIf(StationNumber, "");
    set => StationNumber = value;
  }

  /// <summary>Length of the SPACES_2 attribute.</summary>
  public const int Spaces2_MaxLength = 13;

  /// <summary>
  /// The value of the SPACES_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Spaces2_MaxLength)]
  public string Spaces2
  {
    get => spaces2 ?? "";
    set => spaces2 = TrimEnd(Substring(value, 1, Spaces2_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces2 attribute.</summary>
  [JsonPropertyName("spaces2")]
  [Computed]
  public string Spaces2_Json
  {
    get => NullIf(Spaces2, "");
    set => Spaces2 = value;
  }

  /// <summary>Length of the FPLS_HEADER_CONSTANT attribute.</summary>
  public const int FplsHeaderConstant_MaxLength = 13;

  /// <summary>
  /// The value of the FPLS_HEADER_CONSTANT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = FplsHeaderConstant_MaxLength)]
  public string FplsHeaderConstant
  {
    get => fplsHeaderConstant ?? "";
    set => fplsHeaderConstant =
      TrimEnd(Substring(value, 1, FplsHeaderConstant_MaxLength));
  }

  /// <summary>
  /// The json value of the FplsHeaderConstant attribute.</summary>
  [JsonPropertyName("fplsHeaderConstant")]
  [Computed]
  public string FplsHeaderConstant_Json
  {
    get => NullIf(FplsHeaderConstant, "");
    set => FplsHeaderConstant = value;
  }

  /// <summary>
  /// The value of the TOTAL_RESPONSES attribute.
  /// </summary>
  [JsonPropertyName("totalResponses")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int TotalResponses
  {
    get => totalResponses;
    set => totalResponses = value;
  }

  /// <summary>Length of the FPLS_SORT_CODE attribute.</summary>
  public const int FplsSortCode_MaxLength = 1;

  /// <summary>
  /// The value of the FPLS_SORT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FplsSortCode_MaxLength)]
  public string FplsSortCode
  {
    get => fplsSortCode ?? "";
    set => fplsSortCode = TrimEnd(Substring(value, 1, FplsSortCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FplsSortCode attribute.</summary>
  [JsonPropertyName("fplsSortCode")]
  [Computed]
  public string FplsSortCode_Json
  {
    get => NullIf(FplsSortCode, "");
    set => FplsSortCode = value;
  }

  /// <summary>
  /// The value of the MM_DATE_GENERATED attribute.
  /// </summary>
  [JsonPropertyName("mmDateGenerated")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 2)]
  public int MmDateGenerated
  {
    get => mmDateGenerated;
    set => mmDateGenerated = value;
  }

  /// <summary>
  /// The value of the DD_DATE_GENERATED attribute.
  /// </summary>
  [JsonPropertyName("ddDateGenerated")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 2)]
  public int DdDateGenerated
  {
    get => ddDateGenerated;
    set => ddDateGenerated = value;
  }

  /// <summary>
  /// The value of the YY_DATE_GENERATED attribute.
  /// </summary>
  [JsonPropertyName("yyDateGenerated")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 2)]
  public int YyDateGenerated
  {
    get => yyDateGenerated;
    set => yyDateGenerated = value;
  }

  /// <summary>
  /// The value of the TOTAL_RESPONSES_STATE attribute.
  /// </summary>
  [JsonPropertyName("totalResponsesState")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int TotalResponsesState
  {
    get => totalResponsesState;
    set => totalResponsesState = value;
  }

  /// <summary>Length of the SPACES_3 attribute.</summary>
  public const int Spaces3_MaxLength = 200;

  /// <summary>
  /// The value of the SPACES_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Spaces3_MaxLength)]
  public string Spaces3
  {
    get => spaces3 ?? "";
    set => spaces3 = TrimEnd(Substring(value, 1, Spaces3_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces3 attribute.</summary>
  [JsonPropertyName("spaces3")]
  [Computed]
  public string Spaces3_Json
  {
    get => NullIf(Spaces3, "");
    set => Spaces3 = value;
  }

  /// <summary>Length of the SPACES_4 attribute.</summary>
  public const int Spaces4_MaxLength = 191;

  /// <summary>
  /// The value of the SPACES_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Spaces4_MaxLength)]
  public string Spaces4
  {
    get => spaces4 ?? "";
    set => spaces4 = TrimEnd(Substring(value, 1, Spaces4_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces4 attribute.</summary>
  [JsonPropertyName("spaces4")]
  [Computed]
  public string Spaces4_Json
  {
    get => NullIf(Spaces4, "");
    set => Spaces4 = value;
  }

  private string spaces1;
  private string stateCode;
  private string stationNumber;
  private string spaces2;
  private string fplsHeaderConstant;
  private int totalResponses;
  private string fplsSortCode;
  private int mmDateGenerated;
  private int ddDateGenerated;
  private int yyDateGenerated;
  private int totalResponsesState;
  private string spaces3;
  private string spaces4;
}

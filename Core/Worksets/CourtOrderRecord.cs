// The source file: COURT_ORDER_RECORD, ID: 374396723, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CourtOrderRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CourtOrderRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CourtOrderRecord(CourtOrderRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CourtOrderRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CourtOrderRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    courtOrderNumber = that.courtOrderNumber;
    courtOrderType = that.courtOrderType;
    ffpFlag = that.ffpFlag;
    startDate = that.startDate;
    endDate = that.endDate;
    countyId = that.countyId;
    cityIndicator = that.cityIndicator;
    modificationDate = that.modificationDate;
    filler = that.filler;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 12;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CourtOrderNumber_MaxLength)
    ]
  public string CourtOrderNumber
  {
    get => courtOrderNumber ?? "";
    set => courtOrderNumber =
      TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtOrderNumber attribute.</summary>
  [JsonPropertyName("courtOrderNumber")]
  [Computed]
  public string CourtOrderNumber_Json
  {
    get => NullIf(CourtOrderNumber, "");
    set => CourtOrderNumber = value;
  }

  /// <summary>Length of the COURT_ORDER_TYPE attribute.</summary>
  public const int CourtOrderType_MaxLength = 4;

  /// <summary>
  /// The value of the COURT_ORDER_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CourtOrderType_MaxLength)]
  public string CourtOrderType
  {
    get => courtOrderType ?? "";
    set => courtOrderType =
      TrimEnd(Substring(value, 1, CourtOrderType_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtOrderType attribute.</summary>
  [JsonPropertyName("courtOrderType")]
  [Computed]
  public string CourtOrderType_Json
  {
    get => NullIf(CourtOrderType, "");
    set => CourtOrderType = value;
  }

  /// <summary>Length of the FFP_FLAG attribute.</summary>
  public const int FfpFlag_MaxLength = 1;

  /// <summary>
  /// The value of the FFP_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FfpFlag_MaxLength)]
  public string FfpFlag
  {
    get => ffpFlag ?? "";
    set => ffpFlag = TrimEnd(Substring(value, 1, FfpFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the FfpFlag attribute.</summary>
  [JsonPropertyName("ffpFlag")]
  [Computed]
  public string FfpFlag_Json
  {
    get => NullIf(FfpFlag, "");
    set => FfpFlag = value;
  }

  /// <summary>Length of the START_DATE attribute.</summary>
  public const int StartDate_MaxLength = 8;

  /// <summary>
  /// The value of the START_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = StartDate_MaxLength)]
  public string StartDate
  {
    get => startDate ?? "";
    set => startDate = TrimEnd(Substring(value, 1, StartDate_MaxLength));
  }

  /// <summary>
  /// The json value of the StartDate attribute.</summary>
  [JsonPropertyName("startDate")]
  [Computed]
  public string StartDate_Json
  {
    get => NullIf(StartDate, "");
    set => StartDate = value;
  }

  /// <summary>Length of the END_DATE attribute.</summary>
  public const int EndDate_MaxLength = 8;

  /// <summary>
  /// The value of the END_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = EndDate_MaxLength)]
  public string EndDate
  {
    get => endDate ?? "";
    set => endDate = TrimEnd(Substring(value, 1, EndDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EndDate attribute.</summary>
  [JsonPropertyName("endDate")]
  [Computed]
  public string EndDate_Json
  {
    get => NullIf(EndDate, "");
    set => EndDate = value;
  }

  /// <summary>Length of the COUNTY_ID attribute.</summary>
  public const int CountyId_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTY_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CountyId_MaxLength)]
  public string CountyId
  {
    get => countyId ?? "";
    set => countyId = TrimEnd(Substring(value, 1, CountyId_MaxLength));
  }

  /// <summary>
  /// The json value of the CountyId attribute.</summary>
  [JsonPropertyName("countyId")]
  [Computed]
  public string CountyId_Json
  {
    get => NullIf(CountyId, "");
    set => CountyId = value;
  }

  /// <summary>Length of the CITY_INDICATOR attribute.</summary>
  public const int CityIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CITY_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CityIndicator_MaxLength)]
  public string CityIndicator
  {
    get => cityIndicator ?? "";
    set => cityIndicator =
      TrimEnd(Substring(value, 1, CityIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CityIndicator attribute.</summary>
  [JsonPropertyName("cityIndicator")]
  [Computed]
  public string CityIndicator_Json
  {
    get => NullIf(CityIndicator, "");
    set => CityIndicator = value;
  }

  /// <summary>Length of the MODIFICATION_DATE attribute.</summary>
  public const int ModificationDate_MaxLength = 8;

  /// <summary>
  /// The value of the MODIFICATION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ModificationDate_MaxLength)
    ]
  public string ModificationDate
  {
    get => modificationDate ?? "";
    set => modificationDate =
      TrimEnd(Substring(value, 1, ModificationDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ModificationDate attribute.</summary>
  [JsonPropertyName("modificationDate")]
  [Computed]
  public string ModificationDate_Json
  {
    get => NullIf(ModificationDate, "");
    set => ModificationDate = value;
  }

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 151;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string courtOrderNumber;
  private string courtOrderType;
  private string ffpFlag;
  private string startDate;
  private string endDate;
  private string countyId;
  private string cityIndicator;
  private string modificationDate;
  private string filler;
}

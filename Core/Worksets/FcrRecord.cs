// The source file: FCR_RECORD, ID: 371167822, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FcrRecord: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrRecord(FcrRecord that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the BUNDLE_FPLS_LOCATE_RESULTS attribute.</summary>
  public const int BundleFplsLocateResults_MaxLength = 1;

  /// <summary>
  /// The value of the BUNDLE_FPLS_LOCATE_RESULTS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = BundleFplsLocateResults_MaxLength)]
  public string BundleFplsLocateResults
  {
    get => Get<string>("bundleFplsLocateResults") ?? "";
    set => Set(
      "bundleFplsLocateResults", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BundleFplsLocateResults_MaxLength)));
  }

  /// <summary>
  /// The json value of the BundleFplsLocateResults attribute.</summary>
  [JsonPropertyName("bundleFplsLocateResults")]
  [Computed]
  public string BundleFplsLocateResults_Json
  {
    get => NullIf(BundleFplsLocateResults, "");
    set => BundleFplsLocateResults = value;
  }

  /// <summary>Length of the RECORD_COUNT attribute.</summary>
  public const int RecordCount_MaxLength = 8;

  /// <summary>
  /// The value of the RECORD_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = RecordCount_MaxLength)]
  public string RecordCount
  {
    get => Get<string>("recordCount") ?? "";
    set => Set(
      "recordCount", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, RecordCount_MaxLength)));
  }

  /// <summary>
  /// The json value of the RecordCount attribute.</summary>
  [JsonPropertyName("recordCount")]
  [Computed]
  public string RecordCount_Json
  {
    get => NullIf(RecordCount, "");
    set => RecordCount = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_8 attribute.</summary>
  public const int LocateSource8_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LocateSource8_MaxLength)]
  public string LocateSource8
  {
    get => Get<string>("locateSource8") ?? "";
    set => Set(
      "locateSource8", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource8_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource8 attribute.</summary>
  [JsonPropertyName("locateSource8")]
  [Computed]
  public string LocateSource8_Json
  {
    get => NullIf(LocateSource8, "");
    set => LocateSource8 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_7 attribute.</summary>
  public const int LocateSource7_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_7 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LocateSource7_MaxLength)]
  public string LocateSource7
  {
    get => Get<string>("locateSource7") ?? "";
    set => Set(
      "locateSource7", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource7_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource7 attribute.</summary>
  [JsonPropertyName("locateSource7")]
  [Computed]
  public string LocateSource7_Json
  {
    get => NullIf(LocateSource7, "");
    set => LocateSource7 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_6 attribute.</summary>
  public const int LocateSource6_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_6 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LocateSource6_MaxLength)]
  public string LocateSource6
  {
    get => Get<string>("locateSource6") ?? "";
    set => Set(
      "locateSource6", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource6_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource6 attribute.</summary>
  [JsonPropertyName("locateSource6")]
  [Computed]
  public string LocateSource6_Json
  {
    get => NullIf(LocateSource6, "");
    set => LocateSource6 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_5 attribute.</summary>
  public const int LocateSource5_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = LocateSource5_MaxLength)]
  public string LocateSource5
  {
    get => Get<string>("locateSource5") ?? "";
    set => Set(
      "locateSource5", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource5_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource5 attribute.</summary>
  [JsonPropertyName("locateSource5")]
  [Computed]
  public string LocateSource5_Json
  {
    get => NullIf(LocateSource5, "");
    set => LocateSource5 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_4 attribute.</summary>
  public const int LocateSource4_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = LocateSource4_MaxLength)]
  public string LocateSource4
  {
    get => Get<string>("locateSource4") ?? "";
    set => Set(
      "locateSource4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource4_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource4 attribute.</summary>
  [JsonPropertyName("locateSource4")]
  [Computed]
  public string LocateSource4_Json
  {
    get => NullIf(LocateSource4, "");
    set => LocateSource4 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_3 attribute.</summary>
  public const int LocateSource3_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = LocateSource3_MaxLength)]
  public string LocateSource3
  {
    get => Get<string>("locateSource3") ?? "";
    set => Set(
      "locateSource3", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource3_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource3 attribute.</summary>
  [JsonPropertyName("locateSource3")]
  [Computed]
  public string LocateSource3_Json
  {
    get => NullIf(LocateSource3, "");
    set => LocateSource3 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_2 attribute.</summary>
  public const int LocateSource2_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = LocateSource2_MaxLength)]
  public string LocateSource2
  {
    get => Get<string>("locateSource2") ?? "";
    set => Set(
      "locateSource2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource2_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource2 attribute.</summary>
  [JsonPropertyName("locateSource2")]
  [Computed]
  public string LocateSource2_Json
  {
    get => NullIf(LocateSource2, "");
    set => LocateSource2 = value;
  }

  /// <summary>Length of the LOCATE_SOURCE_1 attribute.</summary>
  public const int LocateSource1_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = LocateSource1_MaxLength)]
  public string LocateSource1
  {
    get => Get<string>("locateSource1") ?? "";
    set => Set(
      "locateSource1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSource1_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSource1 attribute.</summary>
  [JsonPropertyName("locateSource1")]
  [Computed]
  public string LocateSource1_Json
  {
    get => NullIf(LocateSource1, "");
    set => LocateSource1 = value;
  }

  /// <summary>Length of the IRS_1099 attribute.</summary>
  public const int Irs1099_MaxLength = 1;

  /// <summary>
  /// The value of the IRS_1099 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Irs1099_MaxLength)]
  public string Irs1099
  {
    get => Get<string>("irs1099") ?? "";
    set => Set(
      "irs1099", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Irs1099_MaxLength)));
  }

  /// <summary>
  /// The json value of the Irs1099 attribute.</summary>
  [JsonPropertyName("irs1099")]
  [Computed]
  public string Irs1099_Json
  {
    get => NullIf(Irs1099, "");
    set => Irs1099 = value;
  }

  /// <summary>Length of the NEW_MEMBER_ID attribute.</summary>
  public const int NewMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the NEW_MEMBER_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = NewMemberId_MaxLength)]
  public string NewMemberId
  {
    get => Get<string>("newMemberId") ?? "";
    set => Set(
      "newMemberId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, NewMemberId_MaxLength)));
  }

  /// <summary>
  /// The json value of the NewMemberId attribute.</summary>
  [JsonPropertyName("newMemberId")]
  [Computed]
  public string NewMemberId_Json
  {
    get => NullIf(NewMemberId, "");
    set => NewMemberId = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_4 attribute.</summary>
  public const int AdditionalLastName4_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = AdditionalLastName4_MaxLength)]
  public string AdditionalLastName4
  {
    get => Get<string>("additionalLastName4") ?? "";
    set => Set(
      "additionalLastName4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalLastName4_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalLastName4 attribute.</summary>
  [JsonPropertyName("additionalLastName4")]
  [Computed]
  public string AdditionalLastName4_Json
  {
    get => NullIf(AdditionalLastName4, "");
    set => AdditionalLastName4 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_4 attribute.</summary>
  public const int AdditionalMiddleName4_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = AdditionalMiddleName4_MaxLength)]
  public string AdditionalMiddleName4
  {
    get => Get<string>("additionalMiddleName4") ?? "";
    set => Set(
      "additionalMiddleName4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalMiddleName4_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName4 attribute.</summary>
  [JsonPropertyName("additionalMiddleName4")]
  [Computed]
  public string AdditionalMiddleName4_Json
  {
    get => NullIf(AdditionalMiddleName4, "");
    set => AdditionalMiddleName4 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_4 attribute.</summary>
  public const int AdditionalFirstName4_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = AdditionalFirstName4_MaxLength)]
  public string AdditionalFirstName4
  {
    get => Get<string>("additionalFirstName4") ?? "";
    set => Set(
      "additionalFirstName4", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalFirstName4_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName4 attribute.</summary>
  [JsonPropertyName("additionalFirstName4")]
  [Computed]
  public string AdditionalFirstName4_Json
  {
    get => NullIf(AdditionalFirstName4, "");
    set => AdditionalFirstName4 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_3 attribute.</summary>
  public const int AdditionalLastName3_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = AdditionalLastName3_MaxLength)]
  public string AdditionalLastName3
  {
    get => Get<string>("additionalLastName3") ?? "";
    set => Set(
      "additionalLastName3", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalLastName3_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalLastName3 attribute.</summary>
  [JsonPropertyName("additionalLastName3")]
  [Computed]
  public string AdditionalLastName3_Json
  {
    get => NullIf(AdditionalLastName3, "");
    set => AdditionalLastName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_3 attribute.</summary>
  public const int AdditionalMiddleName3_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = AdditionalMiddleName3_MaxLength)]
  public string AdditionalMiddleName3
  {
    get => Get<string>("additionalMiddleName3") ?? "";
    set => Set(
      "additionalMiddleName3", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalMiddleName3_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName3 attribute.</summary>
  [JsonPropertyName("additionalMiddleName3")]
  [Computed]
  public string AdditionalMiddleName3_Json
  {
    get => NullIf(AdditionalMiddleName3, "");
    set => AdditionalMiddleName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_3 attribute.</summary>
  public const int AdditionalFirstName3_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = AdditionalFirstName3_MaxLength)]
  public string AdditionalFirstName3
  {
    get => Get<string>("additionalFirstName3") ?? "";
    set => Set(
      "additionalFirstName3", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalFirstName3_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName3 attribute.</summary>
  [JsonPropertyName("additionalFirstName3")]
  [Computed]
  public string AdditionalFirstName3_Json
  {
    get => NullIf(AdditionalFirstName3, "");
    set => AdditionalFirstName3 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_2 attribute.</summary>
  public const int AdditionalLastName2_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = AdditionalLastName2_MaxLength)]
  public string AdditionalLastName2
  {
    get => Get<string>("additionalLastName2") ?? "";
    set => Set(
      "additionalLastName2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalLastName2_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalLastName2 attribute.</summary>
  [JsonPropertyName("additionalLastName2")]
  [Computed]
  public string AdditionalLastName2_Json
  {
    get => NullIf(AdditionalLastName2, "");
    set => AdditionalLastName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDLE_NAME_2 attribute.</summary>
  public const int AdditionalMidleName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDLE_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = AdditionalMidleName2_MaxLength)]
  public string AdditionalMidleName2
  {
    get => Get<string>("additionalMidleName2") ?? "";
    set => Set(
      "additionalMidleName2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalMidleName2_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalMidleName2 attribute.</summary>
  [JsonPropertyName("additionalMidleName2")]
  [Computed]
  public string AdditionalMidleName2_Json
  {
    get => NullIf(AdditionalMidleName2, "");
    set => AdditionalMidleName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_2 attribute.</summary>
  public const int AdditionalFirstName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = AdditionalFirstName2_MaxLength)]
  public string AdditionalFirstName2
  {
    get => Get<string>("additionalFirstName2") ?? "";
    set => Set(
      "additionalFirstName2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalFirstName2_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName2 attribute.</summary>
  [JsonPropertyName("additionalFirstName2")]
  [Computed]
  public string AdditionalFirstName2_Json
  {
    get => NullIf(AdditionalFirstName2, "");
    set => AdditionalFirstName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_1 attribute.</summary>
  public const int AdditionalLastName1_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = AdditionalLastName1_MaxLength)]
  public string AdditionalLastName1
  {
    get => Get<string>("additionalLastName1") ?? "";
    set => Set(
      "additionalLastName1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalLastName1_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalLastName1 attribute.</summary>
  [JsonPropertyName("additionalLastName1")]
  [Computed]
  public string AdditionalLastName1_Json
  {
    get => NullIf(AdditionalLastName1, "");
    set => AdditionalLastName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_1 attribute.</summary>
  public const int AdditionalMiddleName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = AdditionalMiddleName1_MaxLength)]
  public string AdditionalMiddleName1
  {
    get => Get<string>("additionalMiddleName1") ?? "";
    set => Set(
      "additionalMiddleName1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalMiddleName1_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName1 attribute.</summary>
  [JsonPropertyName("additionalMiddleName1")]
  [Computed]
  public string AdditionalMiddleName1_Json
  {
    get => NullIf(AdditionalMiddleName1, "");
    set => AdditionalMiddleName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_1 attribute.</summary>
  public const int AdditionalFirstName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = AdditionalFirstName1_MaxLength)]
  public string AdditionalFirstName1
  {
    get => Get<string>("additionalFirstName1") ?? "";
    set => Set(
      "additionalFirstName1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalFirstName1_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName1 attribute.</summary>
  [JsonPropertyName("additionalFirstName1")]
  [Computed]
  public string AdditionalFirstName1_Json
  {
    get => NullIf(AdditionalFirstName1, "");
    set => AdditionalFirstName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN2 attribute.</summary>
  public const int AdditionalSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the ADDITIONAL_SSN2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = AdditionalSsn2_MaxLength)]
    
  public string AdditionalSsn2
  {
    get => Get<string>("additionalSsn2") ?? "";
    set => Set(
      "additionalSsn2", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalSsn2_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalSsn2 attribute.</summary>
  [JsonPropertyName("additionalSsn2")]
  [Computed]
  public string AdditionalSsn2_Json
  {
    get => NullIf(AdditionalSsn2, "");
    set => AdditionalSsn2 = value;
  }

  /// <summary>Length of the ADDITIONAL_SSN1 attribute.</summary>
  public const int AdditionalSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the ADDITIONAL_SSN1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = AdditionalSsn1_MaxLength)]
    
  public string AdditionalSsn1
  {
    get => Get<string>("additionalSsn1") ?? "";
    set => Set(
      "additionalSsn1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdditionalSsn1_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdditionalSsn1 attribute.</summary>
  [JsonPropertyName("additionalSsn1")]
  [Computed]
  public string AdditionalSsn1_Json
  {
    get => NullIf(AdditionalSsn1, "");
    set => AdditionalSsn1 = value;
  }

  /// <summary>Length of the IRS_U_SSN attribute.</summary>
  public const int IrsUSsn_MaxLength = 9;

  /// <summary>
  /// The value of the IRS_U_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = IrsUSsn_MaxLength)]
  public string IrsUSsn
  {
    get => Get<string>("irsUSsn") ?? "";
    set => Set(
      "irsUSsn", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IrsUSsn_MaxLength)));
  }

  /// <summary>
  /// The json value of the IrsUSsn attribute.</summary>
  [JsonPropertyName("irsUSsn")]
  [Computed]
  public string IrsUSsn_Json
  {
    get => NullIf(IrsUSsn, "");
    set => IrsUSsn = value;
  }

  /// <summary>Length of the MOTHER_MAIDEN_NAME attribute.</summary>
  public const int MotherMaidenName_MaxLength = 16;

  /// <summary>
  /// The value of the MOTHER_MAIDEN_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = MotherMaidenName_MaxLength)]
  public string MotherMaidenName
  {
    get => Get<string>("motherMaidenName") ?? "";
    set => Set(
      "motherMaidenName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MotherMaidenName_MaxLength)));
  }

  /// <summary>
  /// The json value of the MotherMaidenName attribute.</summary>
  [JsonPropertyName("motherMaidenName")]
  [Computed]
  public string MotherMaidenName_Json
  {
    get => NullIf(MotherMaidenName, "");
    set => MotherMaidenName = value;
  }

  /// <summary>Length of the MOTHER_MIDDLE_INITIAL attribute.</summary>
  public const int MotherMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = MotherMiddleInitial_MaxLength)]
  public string MotherMiddleInitial
  {
    get => Get<string>("motherMiddleInitial") ?? "";
    set => Set(
      "motherMiddleInitial", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MotherMiddleInitial_MaxLength)));
  }

  /// <summary>
  /// The json value of the MotherMiddleInitial attribute.</summary>
  [JsonPropertyName("motherMiddleInitial")]
  [Computed]
  public string MotherMiddleInitial_Json
  {
    get => NullIf(MotherMiddleInitial, "");
    set => MotherMiddleInitial = value;
  }

  /// <summary>Length of the MOTHER_FIRST_NAME attribute.</summary>
  public const int MotherFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the MOTHER_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = MotherFirstName_MaxLength)
    ]
  public string MotherFirstName
  {
    get => Get<string>("motherFirstName") ?? "";
    set => Set(
      "motherFirstName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MotherFirstName_MaxLength)));
  }

  /// <summary>
  /// The json value of the MotherFirstName attribute.</summary>
  [JsonPropertyName("motherFirstName")]
  [Computed]
  public string MotherFirstName_Json
  {
    get => NullIf(MotherFirstName, "");
    set => MotherFirstName = value;
  }

  /// <summary>Length of the FATHER_LAST_NAME attribute.</summary>
  public const int FatherLastName_MaxLength = 16;

  /// <summary>
  /// The value of the FATHER_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = FatherLastName_MaxLength)]
    
  public string FatherLastName
  {
    get => Get<string>("fatherLastName") ?? "";
    set => Set(
      "fatherLastName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FatherLastName_MaxLength)));
  }

  /// <summary>
  /// The json value of the FatherLastName attribute.</summary>
  [JsonPropertyName("fatherLastName")]
  [Computed]
  public string FatherLastName_Json
  {
    get => NullIf(FatherLastName, "");
    set => FatherLastName = value;
  }

  /// <summary>Length of the FATHER_MIDDLE_INITIAL attribute.</summary>
  public const int FatherMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = FatherMiddleInitial_MaxLength)]
  public string FatherMiddleInitial
  {
    get => Get<string>("fatherMiddleInitial") ?? "";
    set => Set(
      "fatherMiddleInitial", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FatherMiddleInitial_MaxLength)));
  }

  /// <summary>
  /// The json value of the FatherMiddleInitial attribute.</summary>
  [JsonPropertyName("fatherMiddleInitial")]
  [Computed]
  public string FatherMiddleInitial_Json
  {
    get => NullIf(FatherMiddleInitial, "");
    set => FatherMiddleInitial = value;
  }

  /// <summary>Length of the FATHERS_FIRST_NAME attribute.</summary>
  public const int FathersFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FATHERS_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = FathersFirstName_MaxLength)]
  public string FathersFirstName
  {
    get => Get<string>("fathersFirstName") ?? "";
    set => Set(
      "fathersFirstName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FathersFirstName_MaxLength)));
  }

  /// <summary>
  /// The json value of the FathersFirstName attribute.</summary>
  [JsonPropertyName("fathersFirstName")]
  [Computed]
  public string FathersFirstName_Json
  {
    get => NullIf(FathersFirstName, "");
    set => FathersFirstName = value;
  }

  /// <summary>Length of the STATE_OF_BIRTH attribute.</summary>
  public const int StateOfBirth_MaxLength = 4;

  /// <summary>
  /// The value of the STATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = StateOfBirth_MaxLength)]
  public string StateOfBirth
  {
    get => Get<string>("stateOfBirth") ?? "";
    set => Set(
      "stateOfBirth", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, StateOfBirth_MaxLength)));
  }

  /// <summary>
  /// The json value of the StateOfBirth attribute.</summary>
  [JsonPropertyName("stateOfBirth")]
  [Computed]
  public string StateOfBirth_Json
  {
    get => NullIf(StateOfBirth, "");
    set => StateOfBirth = value;
  }

  /// <summary>Length of the CITY_OF_BIRTH attribute.</summary>
  public const int CityOfBirth_MaxLength = 16;

  /// <summary>
  /// The value of the CITY_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = CityOfBirth_MaxLength)]
  public string CityOfBirth
  {
    get => Get<string>("cityOfBirth") ?? "";
    set => Set(
      "cityOfBirth", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CityOfBirth_MaxLength)));
  }

  /// <summary>
  /// The json value of the CityOfBirth attribute.</summary>
  [JsonPropertyName("cityOfBirth")]
  [Computed]
  public string CityOfBirth_Json
  {
    get => NullIf(CityOfBirth, "");
    set => CityOfBirth = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => Get<string>("lastName") ?? "";
    set => Set(
      "lastName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LastName_MaxLength)));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = MiddleName_MaxLength)]
  public string MiddleName
  {
    get => Get<string>("middleName") ?? "";
    set => Set(
      "middleName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MiddleName_MaxLength)));
  }

  /// <summary>
  /// The json value of the MiddleName attribute.</summary>
  [JsonPropertyName("middleName")]
  [Computed]
  public string MiddleName_Json
  {
    get => NullIf(MiddleName, "");
    set => MiddleName = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => Get<string>("firstName") ?? "";
    set => Set(
      "firstName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FirstName_MaxLength)));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the PREVIOUS_SSN attribute.</summary>
  public const int PreviousSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PREVIOUS_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length = PreviousSsn_MaxLength)]
  public string PreviousSsn
  {
    get => Get<string>("previousSsn") ?? "";
    set => Set(
      "previousSsn", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PreviousSsn_MaxLength)));
  }

  /// <summary>
  /// The json value of the PreviousSsn attribute.</summary>
  [JsonPropertyName("previousSsn")]
  [Computed]
  public string PreviousSsn_Json
  {
    get => NullIf(PreviousSsn, "");
    set => PreviousSsn = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => Get<string>("ssn") ?? "";
    set => Set(
      "ssn", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Ssn_MaxLength)));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the DATE_OF_BIRTH attribute.</summary>
  public const int DateOfBirth_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = DateOfBirth_MaxLength)]
  public string DateOfBirth
  {
    get => Get<string>("dateOfBirth") ?? "";
    set => Set(
      "dateOfBirth", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, DateOfBirth_MaxLength)));
  }

  /// <summary>
  /// The json value of the DateOfBirth attribute.</summary>
  [JsonPropertyName("dateOfBirth")]
  [Computed]
  public string DateOfBirth_Json
  {
    get => NullIf(DateOfBirth, "");
    set => DateOfBirth = value;
  }

  /// <summary>Length of the SEX_CODE attribute.</summary>
  public const int SexCode_MaxLength = 1;

  /// <summary>
  /// The value of the SEX_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = SexCode_MaxLength)]
  public string SexCode
  {
    get => Get<string>("sexCode") ?? "";
    set => Set(
      "sexCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, SexCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the SexCode attribute.</summary>
  [JsonPropertyName("sexCode")]
  [Computed]
  public string SexCode_Json
  {
    get => NullIf(SexCode, "");
    set => SexCode = value;
  }

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int MemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = MemberId_MaxLength)]
  public string MemberId
  {
    get => Get<string>("memberId") ?? "";
    set => Set(
      "memberId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MemberId_MaxLength)));
  }

  /// <summary>
  /// The json value of the MemberId attribute.</summary>
  [JsonPropertyName("memberId")]
  [Computed]
  public string MemberId_Json
  {
    get => NullIf(MemberId, "");
    set => MemberId = value;
  }

  /// <summary>Length of the FAMILY_VIOLENCE attribute.</summary>
  public const int FamilyViolence_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length = FamilyViolence_MaxLength)]
    
  public string FamilyViolence
  {
    get => Get<string>("familyViolence") ?? "";
    set => Set(
      "familyViolence", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FamilyViolence_MaxLength)));
  }

  /// <summary>
  /// The json value of the FamilyViolence attribute.</summary>
  [JsonPropertyName("familyViolence")]
  [Computed]
  public string FamilyViolence_Json
  {
    get => NullIf(FamilyViolence, "");
    set => FamilyViolence = value;
  }

  /// <summary>Length of the PARTICIPANT_TYPE attribute.</summary>
  public const int ParticipantType_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPANT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = ParticipantType_MaxLength)
    ]
  public string ParticipantType
  {
    get => Get<string>("participantType") ?? "";
    set => Set(
      "participantType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ParticipantType_MaxLength)));
  }

  /// <summary>
  /// The json value of the ParticipantType attribute.</summary>
  [JsonPropertyName("participantType")]
  [Computed]
  public string ParticipantType_Json
  {
    get => NullIf(ParticipantType, "");
    set => ParticipantType = value;
  }

  /// <summary>Length of the LOCATE_REQUEST_TYPE attribute.</summary>
  public const int LocateRequestType_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_REQUEST_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = LocateRequestType_MaxLength)]
  public string LocateRequestType
  {
    get => Get<string>("locateRequestType") ?? "";
    set => Set(
      "locateRequestType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateRequestType_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateRequestType attribute.</summary>
  [JsonPropertyName("locateRequestType")]
  [Computed]
  public string LocateRequestType_Json
  {
    get => NullIf(LocateRequestType, "");
    set => LocateRequestType = value;
  }

  /// <summary>Length of the PREVIOUS_CASE_ID attribute.</summary>
  public const int PreviousCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the PREVIOUS_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = PreviousCaseId_MaxLength)]
    
  public string PreviousCaseId
  {
    get => Get<string>("previousCaseId") ?? "";
    set => Set(
      "previousCaseId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, PreviousCaseId_MaxLength)));
  }

  /// <summary>
  /// The json value of the PreviousCaseId attribute.</summary>
  [JsonPropertyName("previousCaseId")]
  [Computed]
  public string PreviousCaseId_Json
  {
    get => NullIf(PreviousCaseId, "");
    set => PreviousCaseId = value;
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 15;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = UserField_MaxLength)]
  public string UserField
  {
    get => Get<string>("userField") ?? "";
    set => Set(
      "userField", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, UserField_MaxLength)));
  }

  /// <summary>
  /// The json value of the UserField attribute.</summary>
  [JsonPropertyName("userField")]
  [Computed]
  public string UserField_Json
  {
    get => NullIf(UserField, "");
    set => UserField = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
    
  public string FipsCountyCode
  {
    get => Get<string>("fipsCountyCode") ?? "";
    set => Set(
      "fipsCountyCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FipsCountyCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>Length of the ORDER_INDICATOR attribute.</summary>
  public const int OrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length = OrderIndicator_MaxLength)]
    
  public string OrderIndicator
  {
    get => Get<string>("orderIndicator") ?? "";
    set => Set(
      "orderIndicator", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, OrderIndicator_MaxLength)));
  }

  /// <summary>
  /// The json value of the OrderIndicator attribute.</summary>
  [JsonPropertyName("orderIndicator")]
  [Computed]
  public string OrderIndicator_Json
  {
    get => NullIf(OrderIndicator, "");
    set => OrderIndicator = value;
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length = CaseType_MaxLength)]
  public string CaseType
  {
    get => Get<string>("caseType") ?? "";
    set => Set(
      "caseType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseType_MaxLength)));
  }

  /// <summary>
  /// The json value of the CaseType attribute.</summary>
  [JsonPropertyName("caseType")]
  [Computed]
  public string CaseType_Json
  {
    get => NullIf(CaseType, "");
    set => CaseType = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => Get<string>("caseId") ?? "";
    set => Set(
      "caseId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseId_MaxLength)));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
    
  public string ActionTypeCode
  {
    get => Get<string>("actionTypeCode") ?? "";
    set => Set(
      "actionTypeCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ActionTypeCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the ActionTypeCode attribute.</summary>
  [JsonPropertyName("actionTypeCode")]
  [Computed]
  public string ActionTypeCode_Json
  {
    get => NullIf(ActionTypeCode, "");
    set => ActionTypeCode = value;
  }

  /// <summary>Length of the DATE_STAMP attribute.</summary>
  public const int DateStamp_MaxLength = 8;

  /// <summary>
  /// The value of the DATE_STAMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length = DateStamp_MaxLength)]
  public string DateStamp
  {
    get => Get<string>("dateStamp") ?? "";
    set => Set(
      "dateStamp", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, DateStamp_MaxLength)));
  }

  /// <summary>
  /// The json value of the DateStamp attribute.</summary>
  [JsonPropertyName("dateStamp")]
  [Computed]
  public string DateStamp_Json
  {
    get => NullIf(DateStamp, "");
    set => DateStamp = value;
  }

  /// <summary>Length of the BATCH_NUMBER attribute.</summary>
  public const int BatchNumber_MaxLength = 6;

  /// <summary>
  /// The value of the BATCH_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = BatchNumber_MaxLength)]
  public string BatchNumber
  {
    get => Get<string>("batchNumber") ?? "";
    set => Set(
      "batchNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, BatchNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the BatchNumber attribute.</summary>
  [JsonPropertyName("batchNumber")]
  [Computed]
  public string BatchNumber_Json
  {
    get => NullIf(BatchNumber, "");
    set => BatchNumber = value;
  }

  /// <summary>Length of the VERSION_CONTROL_NUMBER attribute.</summary>
  public const int VersionControlNumber_MaxLength = 5;

  /// <summary>
  /// The value of the VERSION_CONTROL_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = VersionControlNumber_MaxLength)]
  public string VersionControlNumber
  {
    get => Get<string>("versionControlNumber") ?? "";
    set => Set(
      "versionControlNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, VersionControlNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the VersionControlNumber attribute.</summary>
  [JsonPropertyName("versionControlNumber")]
  [Computed]
  public string VersionControlNumber_Json
  {
    get => NullIf(VersionControlNumber, "");
    set => VersionControlNumber = value;
  }

  /// <summary>Length of the STATE_CODE attribute.</summary>
  public const int StateCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length = StateCode_MaxLength)]
  public string StateCode
  {
    get => Get<string>("stateCode") ?? "";
    set => Set(
      "stateCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, StateCode_MaxLength)));
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

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = RecordIdentifier_MaxLength)]
  public string RecordIdentifier
  {
    get => Get<string>("recordIdentifier") ?? "";
    set => Set(
      "recordIdentifier", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, RecordIdentifier_MaxLength)));
  }

  /// <summary>
  /// The json value of the RecordIdentifier attribute.</summary>
  [JsonPropertyName("recordIdentifier")]
  [Computed]
  public string RecordIdentifier_Json
  {
    get => NullIf(RecordIdentifier, "");
    set => RecordIdentifier = value;
  }
}

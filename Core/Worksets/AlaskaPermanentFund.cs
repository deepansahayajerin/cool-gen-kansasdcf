// The source file: ALASKA_PERMANENT_FUND, ID: 1902536962, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class AlaskaPermanentFund: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AlaskaPermanentFund()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AlaskaPermanentFund(AlaskaPermanentFund that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AlaskaPermanentFund Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AlaskaPermanentFund that)
  {
    base.Assign(that);
    ksFipsStateCode = that.ksFipsStateCode;
    ksFipsCountyCode = that.ksFipsCountyCode;
    ksNcpSsn = that.ksNcpSsn;
    ksNcpPersonNumber = that.ksNcpPersonNumber;
    ksNcpLastName = that.ksNcpLastName;
    ksNcpFirstName = that.ksNcpFirstName;
    ksNcpDateOfBirth = that.ksNcpDateOfBirth;
    akNcpLastName = that.akNcpLastName;
    akNcpFirstName = that.akNcpFirstName;
    akNcpMiddleInitial = that.akNcpMiddleInitial;
    akAddressLine1 = that.akAddressLine1;
    akAddressLine2 = that.akAddressLine2;
    akAddressCity = that.akAddressCity;
    akAddressState = that.akAddressState;
    akAddressZip = that.akAddressZip;
    akDateOfBirth = that.akDateOfBirth;
    akBirthState = that.akBirthState;
    akSex = that.akSex;
    tabDelimitedRecord = that.tabDelimitedRecord;
    incomingResponseRecord = that.incomingResponseRecord;
  }

  /// <summary>Length of the KS_FIPS_STATE_CODE attribute.</summary>
  public const int KsFipsStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the KS_FIPS_STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = KsFipsStateCode_MaxLength)]
    
  public string KsFipsStateCode
  {
    get => ksFipsStateCode ?? "";
    set => ksFipsStateCode =
      TrimEnd(Substring(value, 1, KsFipsStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the KsFipsStateCode attribute.</summary>
  [JsonPropertyName("ksFipsStateCode")]
  [Computed]
  public string KsFipsStateCode_Json
  {
    get => NullIf(KsFipsStateCode, "");
    set => KsFipsStateCode = value;
  }

  /// <summary>Length of the KS_FIPS_COUNTY_CODE attribute.</summary>
  public const int KsFipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the KS_FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = KsFipsCountyCode_MaxLength)
    ]
  public string KsFipsCountyCode
  {
    get => ksFipsCountyCode ?? "";
    set => ksFipsCountyCode =
      TrimEnd(Substring(value, 1, KsFipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the KsFipsCountyCode attribute.</summary>
  [JsonPropertyName("ksFipsCountyCode")]
  [Computed]
  public string KsFipsCountyCode_Json
  {
    get => NullIf(KsFipsCountyCode, "");
    set => KsFipsCountyCode = value;
  }

  /// <summary>Length of the KS_NCP_SSN attribute.</summary>
  public const int KsNcpSsn_MaxLength = 9;

  /// <summary>
  /// The value of the KS_NCP_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = KsNcpSsn_MaxLength)]
  public string KsNcpSsn
  {
    get => ksNcpSsn ?? "";
    set => ksNcpSsn = TrimEnd(Substring(value, 1, KsNcpSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the KsNcpSsn attribute.</summary>
  [JsonPropertyName("ksNcpSsn")]
  [Computed]
  public string KsNcpSsn_Json
  {
    get => NullIf(KsNcpSsn, "");
    set => KsNcpSsn = value;
  }

  /// <summary>Length of the KS_NCP_PERSON_NUMBER attribute.</summary>
  public const int KsNcpPersonNumber_MaxLength = 15;

  /// <summary>
  /// The value of the KS_NCP_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = KsNcpPersonNumber_MaxLength)]
  public string KsNcpPersonNumber
  {
    get => ksNcpPersonNumber ?? "";
    set => ksNcpPersonNumber =
      TrimEnd(Substring(value, 1, KsNcpPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the KsNcpPersonNumber attribute.</summary>
  [JsonPropertyName("ksNcpPersonNumber")]
  [Computed]
  public string KsNcpPersonNumber_Json
  {
    get => NullIf(KsNcpPersonNumber, "");
    set => KsNcpPersonNumber = value;
  }

  /// <summary>Length of the KS_NCP_LAST_NAME attribute.</summary>
  public const int KsNcpLastName_MaxLength = 20;

  /// <summary>
  /// The value of the KS_NCP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = KsNcpLastName_MaxLength)]
  public string KsNcpLastName
  {
    get => ksNcpLastName ?? "";
    set => ksNcpLastName =
      TrimEnd(Substring(value, 1, KsNcpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the KsNcpLastName attribute.</summary>
  [JsonPropertyName("ksNcpLastName")]
  [Computed]
  public string KsNcpLastName_Json
  {
    get => NullIf(KsNcpLastName, "");
    set => KsNcpLastName = value;
  }

  /// <summary>Length of the KS_NCP_FIRST_NAME attribute.</summary>
  public const int KsNcpFirstName_MaxLength = 15;

  /// <summary>
  /// The value of the KS_NCP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = KsNcpFirstName_MaxLength)]
  public string KsNcpFirstName
  {
    get => ksNcpFirstName ?? "";
    set => ksNcpFirstName =
      TrimEnd(Substring(value, 1, KsNcpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the KsNcpFirstName attribute.</summary>
  [JsonPropertyName("ksNcpFirstName")]
  [Computed]
  public string KsNcpFirstName_Json
  {
    get => NullIf(KsNcpFirstName, "");
    set => KsNcpFirstName = value;
  }

  /// <summary>Length of the KS_NCP_DATE_OF_BIRTH attribute.</summary>
  public const int KsNcpDateOfBirth_MaxLength = 8;

  /// <summary>
  /// The value of the KS_NCP_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = KsNcpDateOfBirth_MaxLength)
    ]
  public string KsNcpDateOfBirth
  {
    get => ksNcpDateOfBirth ?? "";
    set => ksNcpDateOfBirth =
      TrimEnd(Substring(value, 1, KsNcpDateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the KsNcpDateOfBirth attribute.</summary>
  [JsonPropertyName("ksNcpDateOfBirth")]
  [Computed]
  public string KsNcpDateOfBirth_Json
  {
    get => NullIf(KsNcpDateOfBirth, "");
    set => KsNcpDateOfBirth = value;
  }

  /// <summary>Length of the AK_NCP_LAST_NAME attribute.</summary>
  public const int AkNcpLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AK_NCP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = AkNcpLastName_MaxLength)]
  public string AkNcpLastName
  {
    get => akNcpLastName ?? "";
    set => akNcpLastName =
      TrimEnd(Substring(value, 1, AkNcpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the AkNcpLastName attribute.</summary>
  [JsonPropertyName("akNcpLastName")]
  [Computed]
  public string AkNcpLastName_Json
  {
    get => NullIf(AkNcpLastName, "");
    set => AkNcpLastName = value;
  }

  /// <summary>Length of the AK_NCP_FIRST_NAME attribute.</summary>
  public const int AkNcpFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the AK_NCP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AkNcpFirstName_MaxLength)]
  public string AkNcpFirstName
  {
    get => akNcpFirstName ?? "";
    set => akNcpFirstName =
      TrimEnd(Substring(value, 1, AkNcpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the AkNcpFirstName attribute.</summary>
  [JsonPropertyName("akNcpFirstName")]
  [Computed]
  public string AkNcpFirstName_Json
  {
    get => NullIf(AkNcpFirstName, "");
    set => AkNcpFirstName = value;
  }

  /// <summary>Length of the AK_NCP_MIDDLE_INITIAL attribute.</summary>
  public const int AkNcpMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the AK_NCP_MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AkNcpMiddleInitial_MaxLength)]
  public string AkNcpMiddleInitial
  {
    get => akNcpMiddleInitial ?? "";
    set => akNcpMiddleInitial =
      TrimEnd(Substring(value, 1, AkNcpMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the AkNcpMiddleInitial attribute.</summary>
  [JsonPropertyName("akNcpMiddleInitial")]
  [Computed]
  public string AkNcpMiddleInitial_Json
  {
    get => NullIf(AkNcpMiddleInitial, "");
    set => AkNcpMiddleInitial = value;
  }

  /// <summary>Length of the AK_ADDRESS_LINE_1 attribute.</summary>
  public const int AkAddressLine1_MaxLength = 40;

  /// <summary>
  /// The value of the AK_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = AkAddressLine1_MaxLength)]
    
  public string AkAddressLine1
  {
    get => akAddressLine1 ?? "";
    set => akAddressLine1 =
      TrimEnd(Substring(value, 1, AkAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the AkAddressLine1 attribute.</summary>
  [JsonPropertyName("akAddressLine1")]
  [Computed]
  public string AkAddressLine1_Json
  {
    get => NullIf(AkAddressLine1, "");
    set => AkAddressLine1 = value;
  }

  /// <summary>Length of the AK_ADDRESS_LINE_2 attribute.</summary>
  public const int AkAddressLine2_MaxLength = 40;

  /// <summary>
  /// The value of the AK_ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = AkAddressLine2_MaxLength)]
    
  public string AkAddressLine2
  {
    get => akAddressLine2 ?? "";
    set => akAddressLine2 =
      TrimEnd(Substring(value, 1, AkAddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the AkAddressLine2 attribute.</summary>
  [JsonPropertyName("akAddressLine2")]
  [Computed]
  public string AkAddressLine2_Json
  {
    get => NullIf(AkAddressLine2, "");
    set => AkAddressLine2 = value;
  }

  /// <summary>Length of the AK_ADDRESS_CITY attribute.</summary>
  public const int AkAddressCity_MaxLength = 30;

  /// <summary>
  /// The value of the AK_ADDRESS_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = AkAddressCity_MaxLength)]
  public string AkAddressCity
  {
    get => akAddressCity ?? "";
    set => akAddressCity =
      TrimEnd(Substring(value, 1, AkAddressCity_MaxLength));
  }

  /// <summary>
  /// The json value of the AkAddressCity attribute.</summary>
  [JsonPropertyName("akAddressCity")]
  [Computed]
  public string AkAddressCity_Json
  {
    get => NullIf(AkAddressCity, "");
    set => AkAddressCity = value;
  }

  /// <summary>Length of the AK_ADDRESS_STATE attribute.</summary>
  public const int AkAddressState_MaxLength = 2;

  /// <summary>
  /// The value of the AK_ADDRESS_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = AkAddressState_MaxLength)]
    
  public string AkAddressState
  {
    get => akAddressState ?? "";
    set => akAddressState =
      TrimEnd(Substring(value, 1, AkAddressState_MaxLength));
  }

  /// <summary>
  /// The json value of the AkAddressState attribute.</summary>
  [JsonPropertyName("akAddressState")]
  [Computed]
  public string AkAddressState_Json
  {
    get => NullIf(AkAddressState, "");
    set => AkAddressState = value;
  }

  /// <summary>Length of the AK_ADDRESS_ZIP attribute.</summary>
  public const int AkAddressZip_MaxLength = 9;

  /// <summary>
  /// The value of the AK_ADDRESS_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = AkAddressZip_MaxLength)]
  public string AkAddressZip
  {
    get => akAddressZip ?? "";
    set => akAddressZip = TrimEnd(Substring(value, 1, AkAddressZip_MaxLength));
  }

  /// <summary>
  /// The json value of the AkAddressZip attribute.</summary>
  [JsonPropertyName("akAddressZip")]
  [Computed]
  public string AkAddressZip_Json
  {
    get => NullIf(AkAddressZip, "");
    set => AkAddressZip = value;
  }

  /// <summary>Length of the AK_DATE_OF_BIRTH attribute.</summary>
  public const int AkDateOfBirth_MaxLength = 8;

  /// <summary>
  /// The value of the AK_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = AkDateOfBirth_MaxLength)]
  public string AkDateOfBirth
  {
    get => akDateOfBirth ?? "";
    set => akDateOfBirth =
      TrimEnd(Substring(value, 1, AkDateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the AkDateOfBirth attribute.</summary>
  [JsonPropertyName("akDateOfBirth")]
  [Computed]
  public string AkDateOfBirth_Json
  {
    get => NullIf(AkDateOfBirth, "");
    set => AkDateOfBirth = value;
  }

  /// <summary>Length of the AK_BIRTH_STATE attribute.</summary>
  public const int AkBirthState_MaxLength = 2;

  /// <summary>
  /// The value of the AK_BIRTH_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = AkBirthState_MaxLength)]
  public string AkBirthState
  {
    get => akBirthState ?? "";
    set => akBirthState = TrimEnd(Substring(value, 1, AkBirthState_MaxLength));
  }

  /// <summary>
  /// The json value of the AkBirthState attribute.</summary>
  [JsonPropertyName("akBirthState")]
  [Computed]
  public string AkBirthState_Json
  {
    get => NullIf(AkBirthState, "");
    set => AkBirthState = value;
  }

  /// <summary>Length of the AK_SEX attribute.</summary>
  public const int AkSex_MaxLength = 1;

  /// <summary>
  /// The value of the AK_SEX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = AkSex_MaxLength)]
  public string AkSex
  {
    get => akSex ?? "";
    set => akSex = TrimEnd(Substring(value, 1, AkSex_MaxLength));
  }

  /// <summary>
  /// The json value of the AkSex attribute.</summary>
  [JsonPropertyName("akSex")]
  [Computed]
  public string AkSex_Json
  {
    get => NullIf(AkSex, "");
    set => AkSex = value;
  }

  /// <summary>Length of the TAB_DELIMITED_RECORD attribute.</summary>
  public const int TabDelimitedRecord_MaxLength = 323;

  /// <summary>
  /// The value of the TAB_DELIMITED_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Varchar, Length
    = TabDelimitedRecord_MaxLength)]
  public string TabDelimitedRecord
  {
    get => tabDelimitedRecord ?? "";
    set => tabDelimitedRecord =
      Substring(value, 1, TabDelimitedRecord_MaxLength);
  }

  /// <summary>
  /// The json value of the TabDelimitedRecord attribute.</summary>
  [JsonPropertyName("tabDelimitedRecord")]
  [Computed]
  public string TabDelimitedRecord_Json
  {
    get => NullIf(TabDelimitedRecord, "");
    set => TabDelimitedRecord = value;
  }

  /// <summary>Length of the INCOMING_RESPONSE_RECORD attribute.</summary>
  public const int IncomingResponseRecord_MaxLength = 245;

  /// <summary>
  /// The value of the INCOMING_RESPONSE_RECORD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = IncomingResponseRecord_MaxLength)]
  public string IncomingResponseRecord
  {
    get => incomingResponseRecord ?? "";
    set => incomingResponseRecord =
      TrimEnd(Substring(value, 1, IncomingResponseRecord_MaxLength));
  }

  /// <summary>
  /// The json value of the IncomingResponseRecord attribute.</summary>
  [JsonPropertyName("incomingResponseRecord")]
  [Computed]
  public string IncomingResponseRecord_Json
  {
    get => NullIf(IncomingResponseRecord, "");
    set => IncomingResponseRecord = value;
  }

  private string ksFipsStateCode;
  private string ksFipsCountyCode;
  private string ksNcpSsn;
  private string ksNcpPersonNumber;
  private string ksNcpLastName;
  private string ksNcpFirstName;
  private string ksNcpDateOfBirth;
  private string akNcpLastName;
  private string akNcpFirstName;
  private string akNcpMiddleInitial;
  private string akAddressLine1;
  private string akAddressLine2;
  private string akAddressCity;
  private string akAddressState;
  private string akAddressZip;
  private string akDateOfBirth;
  private string akBirthState;
  private string akSex;
  private string tabDelimitedRecord;
  private string incomingResponseRecord;
}

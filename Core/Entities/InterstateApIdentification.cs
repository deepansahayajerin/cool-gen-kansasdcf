// The source file: INTERSTATE_AP_IDENTIFICATION, ID: 371435860, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Information about an AP's physical characteristics received or sent on an 
/// interstate case and transmitted through CSENet.
/// </summary>
[Serializable]
public partial class InterstateApIdentification: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateApIdentification()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateApIdentification(InterstateApIdentification that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateApIdentification Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateApIdentification that)
  {
    base.Assign(that);
    nameLast = that.nameLast;
    nameFirst = that.nameFirst;
    middleName = that.middleName;
    nameSuffix = that.nameSuffix;
    ssn = that.ssn;
    dateOfBirth = that.dateOfBirth;
    race = that.race;
    sex = that.sex;
    placeOfBirth = that.placeOfBirth;
    heightFt = that.heightFt;
    heightIn = that.heightIn;
    weight = that.weight;
    hairColor = that.hairColor;
    eyeColor = that.eyeColor;
    otherIdInfo = that.otherIdInfo;
    aliasSsn1 = that.aliasSsn1;
    aliasSsn2 = that.aliasSsn2;
    possiblyDangerous = that.possiblyDangerous;
    maidenName = that.maidenName;
    mothersMaidenOrFathersName = that.mothersMaidenOrFathersName;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTransSerNum = that.ccaTransSerNum;
  }

  /// <summary>Length of the NAME_LAST attribute.</summary>
  public const int NameLast_MaxLength = 17;

  /// <summary>
  /// The value of the NAME_LAST attribute.
  /// AP last name
  /// </summary>
  [JsonPropertyName("nameLast")]
  [Member(Index = 1, Type = MemberType.Char, Length = NameLast_MaxLength, Optional
    = true)]
  public string NameLast
  {
    get => nameLast;
    set => nameLast = value != null
      ? TrimEnd(Substring(value, 1, NameLast_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_FIRST attribute.</summary>
  public const int NameFirst_MaxLength = 12;

  /// <summary>
  /// The value of the NAME_FIRST attribute.
  /// AP's first name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = NameFirst_MaxLength)]
  public string NameFirst
  {
    get => nameFirst ?? "";
    set => nameFirst = TrimEnd(Substring(value, 1, NameFirst_MaxLength));
  }

  /// <summary>
  /// The json value of the NameFirst attribute.</summary>
  [JsonPropertyName("nameFirst")]
  [Computed]
  public string NameFirst_Json
  {
    get => NullIf(NameFirst, "");
    set => NameFirst = value;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 12;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// AP middle name
  /// </summary>
  [JsonPropertyName("middleName")]
  [Member(Index = 3, Type = MemberType.Char, Length = MiddleName_MaxLength, Optional
    = true)]
  public string MiddleName
  {
    get => middleName;
    set => middleName = value != null
      ? TrimEnd(Substring(value, 1, MiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_SUFFIX attribute.</summary>
  public const int NameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the NAME_SUFFIX attribute.
  /// AP's name suffix.  Could be 'Jr', 'Sr', 'III', etc.
  /// </summary>
  [JsonPropertyName("nameSuffix")]
  [Member(Index = 4, Type = MemberType.Char, Length = NameSuffix_MaxLength, Optional
    = true)]
  public string NameSuffix
  {
    get => nameSuffix;
    set => nameSuffix = value != null
      ? TrimEnd(Substring(value, 1, NameSuffix_MaxLength)) : null;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// AP's social security number
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 5, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => ssn;
    set => ssn = value != null ? TrimEnd(Substring(value, 1, Ssn_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// AP's date of birth
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the RACE attribute.</summary>
  public const int Race_MaxLength = 2;

  /// <summary>
  /// The value of the RACE attribute.
  /// AP's race
  /// WH - White
  /// BL - Black
  /// AI - American Indian, Eskimo, or Aleutian
  /// AP - Asian or Pacific Islander
  /// HI - Hispanic
  /// OT - Other
  /// </summary>
  [JsonPropertyName("race")]
  [Member(Index = 7, Type = MemberType.Char, Length = Race_MaxLength, Optional
    = true)]
  public string Race
  {
    get => race;
    set => race = value != null
      ? TrimEnd(Substring(value, 1, Race_MaxLength)) : null;
  }

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// AP's sex
  /// M - Male
  /// F - Female
  /// O - Other
  /// </summary>
  [JsonPropertyName("sex")]
  [Member(Index = 8, Type = MemberType.Char, Length = Sex_MaxLength, Optional
    = true)]
  public string Sex
  {
    get => sex;
    set => sex = value != null ? TrimEnd(Substring(value, 1, Sex_MaxLength)) : null
      ;
  }

  /// <summary>Length of the PLACE_OF_BIRTH attribute.</summary>
  public const int PlaceOfBirth_MaxLength = 15;

  /// <summary>
  /// The value of the PLACE_OF_BIRTH attribute.
  /// AP's city of birth
  /// </summary>
  [JsonPropertyName("placeOfBirth")]
  [Member(Index = 9, Type = MemberType.Char, Length = PlaceOfBirth_MaxLength, Optional
    = true)]
  public string PlaceOfBirth
  {
    get => placeOfBirth;
    set => placeOfBirth = value != null
      ? TrimEnd(Substring(value, 1, PlaceOfBirth_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the HEIGHT_FT attribute.
  /// AP's height in full feet
  /// </summary>
  [JsonPropertyName("heightFt")]
  [Member(Index = 10, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? HeightFt
  {
    get => heightFt;
    set => heightFt = value;
  }

  /// <summary>
  /// The value of the HEIGHT_IN attribute.
  /// AP height in inches above full feet.  For example, if AP height is 5 feet 
  /// 10 inches, this field will contain '10'.
  /// </summary>
  [JsonPropertyName("heightIn")]
  [Member(Index = 11, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? HeightIn
  {
    get => heightIn;
    set => heightIn = value;
  }

  /// <summary>
  /// The value of the WEIGHT attribute.
  /// AP weight in pounds
  /// </summary>
  [JsonPropertyName("weight")]
  [Member(Index = 12, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? Weight
  {
    get => weight;
    set => weight = value;
  }

  /// <summary>Length of the HAIR_COLOR attribute.</summary>
  public const int HairColor_MaxLength = 2;

  /// <summary>
  /// The value of the HAIR_COLOR attribute.
  /// AP's hair color
  /// BD - Bald
  /// BL - Blond
  /// BK - Black
  /// BN - Brown
  /// RD - Red
  /// GY - Gray
  /// OT - Other
  /// </summary>
  [JsonPropertyName("hairColor")]
  [Member(Index = 13, Type = MemberType.Char, Length = HairColor_MaxLength, Optional
    = true)]
  public string HairColor
  {
    get => hairColor;
    set => hairColor = value != null
      ? TrimEnd(Substring(value, 1, HairColor_MaxLength)) : null;
  }

  /// <summary>Length of the EYE_COLOR attribute.</summary>
  public const int EyeColor_MaxLength = 2;

  /// <summary>
  /// The value of the EYE_COLOR attribute.
  /// AP's eye color
  /// BU - Blue
  /// BN - Brown
  /// DK - Dark
  /// GN - Green
  /// GY - Gray
  /// HZ - Hazel
  /// Ot - Other
  /// </summary>
  [JsonPropertyName("eyeColor")]
  [Member(Index = 14, Type = MemberType.Char, Length = EyeColor_MaxLength, Optional
    = true)]
  public string EyeColor
  {
    get => eyeColor;
    set => eyeColor = value != null
      ? TrimEnd(Substring(value, 1, EyeColor_MaxLength)) : null;
  }

  /// <summary>Length of the OTHER_ID_INFO attribute.</summary>
  public const int OtherIdInfo_MaxLength = 20;

  /// <summary>
  /// The value of the OTHER_ID_INFO attribute.
  /// AP's distinguishing marks such as scars, tatoos, limps, etc.
  /// </summary>
  [JsonPropertyName("otherIdInfo")]
  [Member(Index = 15, Type = MemberType.Char, Length = OtherIdInfo_MaxLength, Optional
    = true)]
  public string OtherIdInfo
  {
    get => otherIdInfo;
    set => otherIdInfo = value != null
      ? TrimEnd(Substring(value, 1, OtherIdInfo_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_SSN_1 attribute.</summary>
  public const int AliasSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the ALIAS_SSN_1 attribute.
  /// AP's #1 alias SSN
  /// </summary>
  [JsonPropertyName("aliasSsn1")]
  [Member(Index = 16, Type = MemberType.Char, Length = AliasSsn1_MaxLength, Optional
    = true)]
  public string AliasSsn1
  {
    get => aliasSsn1;
    set => aliasSsn1 = value != null
      ? TrimEnd(Substring(value, 1, AliasSsn1_MaxLength)) : null;
  }

  /// <summary>Length of the ALIAS_SSN_2 attribute.</summary>
  public const int AliasSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the ALIAS_SSN_2 attribute.
  /// AP's #2 alias SSN
  /// </summary>
  [JsonPropertyName("aliasSsn2")]
  [Member(Index = 17, Type = MemberType.Char, Length = AliasSsn2_MaxLength, Optional
    = true)]
  public string AliasSsn2
  {
    get => aliasSsn2;
    set => aliasSsn2 = value != null
      ? TrimEnd(Substring(value, 1, AliasSsn2_MaxLength)) : null;
  }

  /// <summary>Length of the POSSIBLY_DANGEROUS attribute.</summary>
  public const int PossiblyDangerous_MaxLength = 1;

  /// <summary>
  /// The value of the POSSIBLY_DANGEROUS attribute.
  /// An indication of whether or not this person is known to be dangerous.
  /// </summary>
  [JsonPropertyName("possiblyDangerous")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = PossiblyDangerous_MaxLength, Optional = true)]
  public string PossiblyDangerous
  {
    get => possiblyDangerous;
    set => possiblyDangerous = value != null
      ? TrimEnd(Substring(value, 1, PossiblyDangerous_MaxLength)) : null;
  }

  /// <summary>Length of the MAIDEN_NAME attribute.</summary>
  public const int MaidenName_MaxLength = 17;

  /// <summary>
  /// The value of the MAIDEN_NAME attribute.
  /// The maiden name of the person.
  /// </summary>
  [JsonPropertyName("maidenName")]
  [Member(Index = 19, Type = MemberType.Char, Length = MaidenName_MaxLength, Optional
    = true)]
  public string MaidenName
  {
    get => maidenName;
    set => maidenName = value != null
      ? TrimEnd(Substring(value, 1, MaidenName_MaxLength)) : null;
  }

  /// <summary>Length of the MOTHERS_MAIDEN_OR_FATHERS_NAME attribute.</summary>
  public const int MothersMaidenOrFathersName_MaxLength = 17;

  /// <summary>
  /// The value of the MOTHERS_MAIDEN_OR_FATHERS_NAME attribute.
  /// This persons mothers maiden name or fathers last name.
  /// </summary>
  [JsonPropertyName("mothersMaidenOrFathersName")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = MothersMaidenOrFathersName_MaxLength, Optional = true)]
  public string MothersMaidenOrFathersName
  {
    get => mothersMaidenOrFathersName;
    set => mothersMaidenOrFathersName = value != null
      ? TrimEnd(Substring(value, 1, MothersMaidenOrFathersName_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 21, Type = MemberType.Date)]
  public DateTime? CcaTransactionDt
  {
    get => ccaTransactionDt;
    set => ccaTransactionDt = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("ccaTransSerNum")]
  [DefaultValue(0L)]
  [Member(Index = 22, Type = MemberType.Number, Length = 12)]
  public long CcaTransSerNum
  {
    get => ccaTransSerNum;
    set => ccaTransSerNum = value;
  }

  private string nameLast;
  private string nameFirst;
  private string middleName;
  private string nameSuffix;
  private string ssn;
  private DateTime? dateOfBirth;
  private string race;
  private string sex;
  private string placeOfBirth;
  private int? heightFt;
  private int? heightIn;
  private int? weight;
  private string hairColor;
  private string eyeColor;
  private string otherIdInfo;
  private string aliasSsn1;
  private string aliasSsn2;
  private string possiblyDangerous;
  private string maidenName;
  private string mothersMaidenOrFathersName;
  private DateTime? ccaTransactionDt;
  private long ccaTransSerNum;
}

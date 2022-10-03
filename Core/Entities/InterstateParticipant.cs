// The source file: INTERSTATE_PARTICIPANT, ID: 371436425, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Information about people involved on an interstate case that is received or 
/// sent and transmitted through CSENet.
/// </summary>
[Serializable]
public partial class InterstateParticipant: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateParticipant()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateParticipant(InterstateParticipant that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateParticipant Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateParticipant that)
  {
    base.Assign(that);
    systemGeneratedSequenceNum = that.systemGeneratedSequenceNum;
    nameLast = that.nameLast;
    nameFirst = that.nameFirst;
    nameMiddle = that.nameMiddle;
    nameSuffix = that.nameSuffix;
    dateOfBirth = that.dateOfBirth;
    ssn = that.ssn;
    sex = that.sex;
    race = that.race;
    relationship = that.relationship;
    status = that.status;
    dependentRelationCp = that.dependentRelationCp;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    employerAddressLine1 = that.employerAddressLine1;
    employerAddressLine2 = that.employerAddressLine2;
    employerCity = that.employerCity;
    employerState = that.employerState;
    employerZipCode5 = that.employerZipCode5;
    employerZipCode4 = that.employerZipCode4;
    employerName = that.employerName;
    employerEin = that.employerEin;
    addressVerifiedDate = that.addressVerifiedDate;
    employerVerifiedDate = that.employerVerifiedDate;
    workPhone = that.workPhone;
    workAreaCode = that.workAreaCode;
    placeOfBirth = that.placeOfBirth;
    childStateOfResidence = that.childStateOfResidence;
    childPaternityStatus = that.childPaternityStatus;
    employerConfirmedInd = that.employerConfirmedInd;
    addressConfirmedInd = that.addressConfirmedInd;
    ccaTransactionDt = that.ccaTransactionDt;
    ccaTransSerNum = that.ccaTransSerNum;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_SEQUENCE_NUM attribute.
  /// </summary>
  [JsonPropertyName("systemGeneratedSequenceNum")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 1)]
  public int SystemGeneratedSequenceNum
  {
    get => systemGeneratedSequenceNum;
    set => systemGeneratedSequenceNum = value;
  }

  /// <summary>Length of the NAME_LAST attribute.</summary>
  public const int NameLast_MaxLength = 17;

  /// <summary>
  /// The value of the NAME_LAST attribute.
  /// Last name
  /// </summary>
  [JsonPropertyName("nameLast")]
  [Member(Index = 2, Type = MemberType.Char, Length = NameLast_MaxLength, Optional
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
  /// First name
  /// </summary>
  [JsonPropertyName("nameFirst")]
  [Member(Index = 3, Type = MemberType.Char, Length = NameFirst_MaxLength, Optional
    = true)]
  public string NameFirst
  {
    get => nameFirst;
    set => nameFirst = value != null
      ? TrimEnd(Substring(value, 1, NameFirst_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_MIDDLE attribute.</summary>
  public const int NameMiddle_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_MIDDLE attribute.
  /// Middle initial
  /// </summary>
  [JsonPropertyName("nameMiddle")]
  [Member(Index = 4, Type = MemberType.Char, Length = NameMiddle_MaxLength, Optional
    = true)]
  public string NameMiddle
  {
    get => nameMiddle;
    set => nameMiddle = value != null
      ? TrimEnd(Substring(value, 1, NameMiddle_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_SUFFIX attribute.</summary>
  public const int NameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the NAME_SUFFIX attribute.
  /// Suffix of Participant name.  Could be 'Jr.', 'III', 'MD', etc.
  /// </summary>
  [JsonPropertyName("nameSuffix")]
  [Member(Index = 5, Type = MemberType.Char, Length = NameSuffix_MaxLength, Optional
    = true)]
  public string NameSuffix
  {
    get => nameSuffix;
    set => nameSuffix = value != null
      ? TrimEnd(Substring(value, 1, NameSuffix_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// Date of birth
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// Social Security Number
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 7, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => ssn;
    set => ssn = value != null ? TrimEnd(Substring(value, 1, Ssn_MaxLength)) : null
      ;
  }

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// Gender or sex
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

  /// <summary>Length of the RACE attribute.</summary>
  public const int Race_MaxLength = 2;

  /// <summary>
  /// The value of the RACE attribute.
  /// Race:
  /// WH - White
  /// BL - Black
  /// AI - American Indian, Eskimo, Aleutian
  /// AP - Asian or Pacific Islander
  /// HI - Hispanic
  /// OT - Other
  /// </summary>
  [JsonPropertyName("race")]
  [Member(Index = 9, Type = MemberType.Char, Length = Race_MaxLength, Optional
    = true)]
  public string Race
  {
    get => race;
    set => race = value != null
      ? TrimEnd(Substring(value, 1, Race_MaxLength)) : null;
  }

  /// <summary>Length of the RELATIONSHIP attribute.</summary>
  public const int Relationship_MaxLength = 2;

  /// <summary>
  /// The value of the RELATIONSHIP attribute.
  /// The Role of this Participant to the case:
  /// A - Absent Parent - AP
  /// C - Custodial Parent - AR
  /// D - Dependent - CH
  /// P - Putative - AP
  /// S - Second Adult - AP
  /// </summary>
  [JsonPropertyName("relationship")]
  [Member(Index = 10, Type = MemberType.Char, Length = Relationship_MaxLength, Optional
    = true)]
  public string Relationship
  {
    get => relationship;
    set => relationship = value != null
      ? TrimEnd(Substring(value, 1, Relationship_MaxLength)) : null;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// Indicates whether this Participant is an active member in this case.
  /// O - Open (Active)
  /// C - Closed (Inactive)
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 11, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => status;
    set => status = value != null
      ? TrimEnd(Substring(value, 1, Status_MaxLength)) : null;
  }

  /// <summary>Length of the DEPENDENT_RELATION_CP attribute.</summary>
  public const int DependentRelationCp_MaxLength = 2;

  /// <summary>
  /// The value of the DEPENDENT_RELATION_CP attribute.
  /// This field is only used if the Participant is a Dependent (child).  It 
  /// indicates the relationship of the dependent to the Custodial Parent in the
  /// Case.
  /// CH - Adopted, Natural, Ward Child
  /// FC - Foster child
  /// GC - Grandchild
  /// NN - Niece/Nephew
  /// NR - No relation
  /// OR - Other relationship
  /// SB - Sibling
  /// CO - Cousin
  /// </summary>
  [JsonPropertyName("dependentRelationCp")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = DependentRelationCp_MaxLength, Optional = true)]
  public string DependentRelationCp
  {
    get => dependentRelationCp;
    set => dependentRelationCp = value != null
      ? TrimEnd(Substring(value, 1, DependentRelationCp_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// The first line of a postal address.
  /// </summary>
  [JsonPropertyName("addressLine1")]
  [Member(Index = 13, Type = MemberType.Char, Length = AddressLine1_MaxLength, Optional
    = true)]
  public string AddressLine1
  {
    get => addressLine1;
    set => addressLine1 = value != null
      ? TrimEnd(Substring(value, 1, AddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// The second line of a postal address.
  /// </summary>
  [JsonPropertyName("addressLine2")]
  [Member(Index = 14, Type = MemberType.Char, Length = AddressLine2_MaxLength, Optional
    = true)]
  public string AddressLine2
  {
    get => addressLine2;
    set => addressLine2 = value != null
      ? TrimEnd(Substring(value, 1, AddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// The city that the participant is located in.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 15, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The state the participant is located in.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 16, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE_5 attribute.
  /// This is the basis five-character ZIP which is part of an address.
  /// </summary>
  [JsonPropertyName("zipCode5")]
  [Member(Index = 17, Type = MemberType.Char, Length = ZipCode5_MaxLength, Optional
    = true)]
  public string ZipCode5
  {
    get => zipCode5;
    set => zipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE_4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE_4 attribute.
  /// The second part of the zip code, containing four digits.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 18, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_1 attribute.</summary>
  public const int EmployerAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_1 attribute.
  /// The first line of a postal address.
  /// </summary>
  [JsonPropertyName("employerAddressLine1")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = EmployerAddressLine1_MaxLength, Optional = true)]
  public string EmployerAddressLine1
  {
    get => employerAddressLine1;
    set => employerAddressLine1 = value != null
      ? TrimEnd(Substring(value, 1, EmployerAddressLine1_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_2 attribute.</summary>
  public const int EmployerAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_2 attribute.
  /// The second line of a postal address.
  /// </summary>
  [JsonPropertyName("employerAddressLine2")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = EmployerAddressLine2_MaxLength, Optional = true)]
  public string EmployerAddressLine2
  {
    get => employerAddressLine2;
    set => employerAddressLine2 = value != null
      ? TrimEnd(Substring(value, 1, EmployerAddressLine2_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_CITY attribute.</summary>
  public const int EmployerCity_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYER_CITY attribute.
  /// The city that the participant is located in.
  /// </summary>
  [JsonPropertyName("employerCity")]
  [Member(Index = 21, Type = MemberType.Char, Length = EmployerCity_MaxLength, Optional
    = true)]
  public string EmployerCity
  {
    get => employerCity;
    set => employerCity = value != null
      ? TrimEnd(Substring(value, 1, EmployerCity_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_STATE attribute.</summary>
  public const int EmployerState_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER_STATE attribute.
  /// The state the participant is located in.
  /// </summary>
  [JsonPropertyName("employerState")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = EmployerState_MaxLength, Optional = true)]
  public string EmployerState
  {
    get => employerState;
    set => employerState = value != null
      ? TrimEnd(Substring(value, 1, EmployerState_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ZIP_CODE_5 attribute.</summary>
  public const int EmployerZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the EMPLOYER_ZIP_CODE_5 attribute.
  /// The first part of the zip code, containing five digits.
  /// </summary>
  [JsonPropertyName("employerZipCode5")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = EmployerZipCode5_MaxLength, Optional = true)]
  public string EmployerZipCode5
  {
    get => employerZipCode5;
    set => employerZipCode5 = value != null
      ? TrimEnd(Substring(value, 1, EmployerZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_ZIP_CODE_4 attribute.</summary>
  public const int EmployerZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the EMPLOYER_ZIP_CODE_4 attribute.
  /// The second part of the zip code, containing four digits.
  /// </summary>
  [JsonPropertyName("employerZipCode4")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = EmployerZipCode4_MaxLength, Optional = true)]
  public string EmployerZipCode4
  {
    get => employerZipCode4;
    set => employerZipCode4 = value != null
      ? TrimEnd(Substring(value, 1, EmployerZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 33;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// The name of the employer.
  /// </summary>
  [JsonPropertyName("employerName")]
  [Member(Index = 25, Type = MemberType.Char, Length = EmployerName_MaxLength, Optional
    = true)]
  public string EmployerName
  {
    get => employerName;
    set => employerName = value != null
      ? TrimEnd(Substring(value, 1, EmployerName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMPLOYER_EIN attribute.
  /// Employer identification number.
  /// </summary>
  [JsonPropertyName("employerEin")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? EmployerEin
  {
    get => employerEin;
    set => employerEin = value;
  }

  /// <summary>
  /// The value of the ADDRESS_VERIFIED_DATE attribute.
  /// The date that the participants address was verified.
  /// </summary>
  [JsonPropertyName("addressVerifiedDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? AddressVerifiedDate
  {
    get => addressVerifiedDate;
    set => addressVerifiedDate = value;
  }

  /// <summary>
  /// The value of the EMPLOYER_VERIFIED_DATE attribute.
  /// The date it was determined that the participant worked for this employer.
  /// </summary>
  [JsonPropertyName("employerVerifiedDate")]
  [Member(Index = 28, Type = MemberType.Date, Optional = true)]
  public DateTime? EmployerVerifiedDate
  {
    get => employerVerifiedDate;
    set => employerVerifiedDate = value;
  }

  /// <summary>Length of the WORK_PHONE attribute.</summary>
  public const int WorkPhone_MaxLength = 7;

  /// <summary>
  /// The value of the WORK_PHONE attribute.
  /// The telephone number where a person can be reached at work.
  /// </summary>
  [JsonPropertyName("workPhone")]
  [Member(Index = 29, Type = MemberType.Char, Length = WorkPhone_MaxLength, Optional
    = true)]
  public string WorkPhone
  {
    get => workPhone;
    set => workPhone = value != null
      ? TrimEnd(Substring(value, 1, WorkPhone_MaxLength)) : null;
  }

  /// <summary>Length of the WORK_AREA_CODE attribute.</summary>
  public const int WorkAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the WORK_AREA_CODE attribute.
  /// The area code for the telephone number where a person can be reached at 
  /// work.
  /// </summary>
  [JsonPropertyName("workAreaCode")]
  [Member(Index = 30, Type = MemberType.Char, Length = WorkAreaCode_MaxLength, Optional
    = true)]
  public string WorkAreaCode
  {
    get => workAreaCode;
    set => workAreaCode = value != null
      ? TrimEnd(Substring(value, 1, WorkAreaCode_MaxLength)) : null;
  }

  /// <summary>Length of the PLACE_OF_BIRTH attribute.</summary>
  public const int PlaceOfBirth_MaxLength = 25;

  /// <summary>
  /// The value of the PLACE_OF_BIRTH attribute.
  /// The birth place of this person.
  /// </summary>
  [JsonPropertyName("placeOfBirth")]
  [Member(Index = 31, Type = MemberType.Char, Length = PlaceOfBirth_MaxLength, Optional
    = true)]
  public string PlaceOfBirth
  {
    get => placeOfBirth;
    set => placeOfBirth = value != null
      ? TrimEnd(Substring(value, 1, PlaceOfBirth_MaxLength)) : null;
  }

  /// <summary>Length of the CHILD_STATE_OF_RESIDENCE attribute.</summary>
  public const int ChildStateOfResidence_MaxLength = 2;

  /// <summary>
  /// The value of the CHILD_STATE_OF_RESIDENCE attribute.
  /// The state in which this child resides.
  /// </summary>
  [JsonPropertyName("childStateOfResidence")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = ChildStateOfResidence_MaxLength, Optional = true)]
  public string ChildStateOfResidence
  {
    get => childStateOfResidence;
    set => childStateOfResidence = value != null
      ? TrimEnd(Substring(value, 1, ChildStateOfResidence_MaxLength)) : null;
  }

  /// <summary>Length of the CHILD_PATERNITY_STATUS attribute.</summary>
  public const int ChildPaternityStatus_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_PATERNITY_STATUS attribute.
  /// An indication of whether or not paternity has been determined.
  /// </summary>
  [JsonPropertyName("childPaternityStatus")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = ChildPaternityStatus_MaxLength, Optional = true)]
  public string ChildPaternityStatus
  {
    get => childPaternityStatus;
    set => childPaternityStatus = value != null
      ? TrimEnd(Substring(value, 1, ChildPaternityStatus_MaxLength)) : null;
  }

  /// <summary>Length of the EMPLOYER_CONFIRMED_IND attribute.</summary>
  public const int EmployerConfirmedInd_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER_CONFIRMED_IND attribute.
  /// Employment verified indicator (Yes/No).
  /// </summary>
  [JsonPropertyName("employerConfirmedInd")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = EmployerConfirmedInd_MaxLength, Optional = true)]
  public string EmployerConfirmedInd
  {
    get => employerConfirmedInd;
    set => employerConfirmedInd = value != null
      ? TrimEnd(Substring(value, 1, EmployerConfirmedInd_MaxLength)) : null;
  }

  /// <summary>Length of the ADDRESS_CONFIRMED_IND attribute.</summary>
  public const int AddressConfirmedInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_CONFIRMED_IND attribute.
  /// Address verified indicator (Yes/No).
  /// </summary>
  [JsonPropertyName("addressConfirmedInd")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = AddressConfirmedInd_MaxLength, Optional = true)]
  public string AddressConfirmedInd
  {
    get => addressConfirmedInd;
    set => addressConfirmedInd = value != null
      ? TrimEnd(Substring(value, 1, AddressConfirmedInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRANSACTION_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("ccaTransactionDt")]
  [Member(Index = 36, Type = MemberType.Date)]
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
  [Member(Index = 37, Type = MemberType.Number, Length = 12)]
  public long CcaTransSerNum
  {
    get => ccaTransSerNum;
    set => ccaTransSerNum = value;
  }

  private int systemGeneratedSequenceNum;
  private string nameLast;
  private string nameFirst;
  private string nameMiddle;
  private string nameSuffix;
  private DateTime? dateOfBirth;
  private string ssn;
  private string sex;
  private string race;
  private string relationship;
  private string status;
  private string dependentRelationCp;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string employerAddressLine1;
  private string employerAddressLine2;
  private string employerCity;
  private string employerState;
  private string employerZipCode5;
  private string employerZipCode4;
  private string employerName;
  private int? employerEin;
  private DateTime? addressVerifiedDate;
  private DateTime? employerVerifiedDate;
  private string workPhone;
  private string workAreaCode;
  private string placeOfBirth;
  private string childStateOfResidence;
  private string childPaternityStatus;
  private string employerConfirmedInd;
  private string addressConfirmedInd;
  private DateTime? ccaTransactionDt;
  private long ccaTransSerNum;
}

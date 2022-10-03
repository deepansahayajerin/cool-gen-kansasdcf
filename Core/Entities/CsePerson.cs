// The source file: CSE_PERSON, ID: 371421557, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Scope: A person related to CSE REQUESTs for service.
/// Qualifications: Not an employee or agent of CSE.
/// Excludes: Collection officers, CSE attorneys, District Attorneys, 
/// Contractors, Vendors.
/// Example: Child, Absent Parent, Applicant Receipent, Mother, Father.
/// </summary>
[Serializable]
public partial class CsePerson: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePerson()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePerson(CsePerson that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePerson Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This type defines whether the CSE Person is a Client or an organization.
  /// C - Client	
  /// O - Organization
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("C")]
  [Value("O")]
  public string Type1
  {
    get => Get<string>("type1") ?? "";
    set => Set(
      "type1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Type1_MaxLength)));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => Get<string>("number") ?? "";
    set => Set(
      "number", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Number_MaxLength)));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>Length of the KSCARES_NUMBER attribute.</summary>
  public const int KscaresNumber_MaxLength = 10;

  /// <summary>
  /// The value of the KSCARES_NUMBER attribute.
  /// This attribute identifies participation in KSCARES.
  /// </summary>
  [JsonPropertyName("kscaresNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length = KscaresNumber_MaxLength, Optional
    = true)]
  public string KscaresNumber
  {
    get => Get<string>("kscaresNumber");
    set => Set(
      "kscaresNumber", TrimEnd(Substring(value, 1, KscaresNumber_MaxLength)));
  }

  /// <summary>Length of the OCCUPATION attribute.</summary>
  public const int Occupation_MaxLength = 30;

  /// <summary>
  /// The value of the OCCUPATION attribute.
  /// The type of employment or skill that the CSE Person is associated with.
  /// </summary>
  [JsonPropertyName("occupation")]
  [Member(Index = 8, Type = MemberType.Char, Length = Occupation_MaxLength, Optional
    = true)]
  public string Occupation
  {
    get => Get<string>("occupation");
    set =>
      Set("occupation", TrimEnd(Substring(value, 1, Occupation_MaxLength)));
  }

  /// <summary>Length of the AE_CASE_NUMBER attribute.</summary>
  public const int AeCaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the AE_CASE_NUMBER attribute.
  /// The Case number for Automated Eligibility
  /// </summary>
  [JsonPropertyName("aeCaseNumber")]
  [Member(Index = 9, Type = MemberType.Char, Length = AeCaseNumber_MaxLength, Optional
    = true)]
  public string AeCaseNumber
  {
    get => Get<string>("aeCaseNumber");
    set => Set(
      "aeCaseNumber", TrimEnd(Substring(value, 1, AeCaseNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_OF_DEATH attribute.
  /// The date the CSE Person died
  /// </summary>
  [JsonPropertyName("dateOfDeath")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfDeath
  {
    get => Get<DateTime?>("dateOfDeath");
    set => Set("dateOfDeath", value);
  }

  /// <summary>Length of the ILLEGAL_ALIEN_INDICATOR attribute.</summary>
  public const int IllegalAlienIndicator_MaxLength = 2;

  /// <summary>
  /// The value of the ILLEGAL_ALIEN_INDICATOR attribute.
  /// Indicates that the person is an illegal alien.
  /// </summary>
  [JsonPropertyName("illegalAlienIndicator")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = IllegalAlienIndicator_MaxLength, Optional = true)]
  public string IllegalAlienIndicator
  {
    get => Get<string>("illegalAlienIndicator");
    set => Set(
      "illegalAlienIndicator", TrimEnd(Substring(value, 1,
      IllegalAlienIndicator_MaxLength)));
  }

  /// <summary>Length of the CURRENT_SPOUSE_MI attribute.</summary>
  public const int CurrentSpouseMi_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_MI attribute.
  /// The middle initial of the CSE Person's current spouse
  /// </summary>
  [JsonPropertyName("currentSpouseMi")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CurrentSpouseMi_MaxLength, Optional = true)]
  public string CurrentSpouseMi
  {
    get => Get<string>("currentSpouseMi");
    set => Set(
      "currentSpouseMi",
      TrimEnd(Substring(value, 1, CurrentSpouseMi_MaxLength)));
  }

  /// <summary>Length of the CURRENT_SPOUSE_FIRST_NAME attribute.</summary>
  public const int CurrentSpouseFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_FIRST_NAME attribute.
  /// The first name of the CSE Person's current spouse
  /// </summary>
  [JsonPropertyName("currentSpouseFirstName")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = CurrentSpouseFirstName_MaxLength, Optional = true)]
  public string CurrentSpouseFirstName
  {
    get => Get<string>("currentSpouseFirstName");
    set => Set(
      "currentSpouseFirstName", TrimEnd(Substring(value, 1,
      CurrentSpouseFirstName_MaxLength)));
  }

  /// <summary>Length of the BIRTH_PLACE_STATE attribute.</summary>
  public const int BirthPlaceState_MaxLength = 2;

  /// <summary>
  /// The value of the BIRTH_PLACE_STATE attribute.
  /// The state a CSE Person was born in
  /// </summary>
  [JsonPropertyName("birthPlaceState")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = BirthPlaceState_MaxLength, Optional = true)]
  public string BirthPlaceState
  {
    get => Get<string>("birthPlaceState");
    set => Set(
      "birthPlaceState",
      TrimEnd(Substring(value, 1, BirthPlaceState_MaxLength)));
  }

  /// <summary>
  /// The value of the EMERGENCY_PHONE attribute.
  /// The phone number to call in an emergency.
  /// Source-Application
  /// </summary>
  [JsonPropertyName("emergencyPhone")]
  [Member(Index = 15, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? EmergencyPhone
  {
    get => Get<int?>("emergencyPhone");
    set => Set("emergencyPhone", value);
  }

  /// <summary>Length of the NAME_MIDDLE attribute.</summary>
  public const int NameMiddle_MaxLength = 12;

  /// <summary>
  /// The value of the NAME_MIDDLE attribute.
  /// The middle name of the CSE Person
  /// </summary>
  [JsonPropertyName("nameMiddle")]
  [Member(Index = 16, Type = MemberType.Char, Length = NameMiddle_MaxLength, Optional
    = true)]
  public string NameMiddle
  {
    get => Get<string>("nameMiddle");
    set =>
      Set("nameMiddle", TrimEnd(Substring(value, 1, NameMiddle_MaxLength)));
  }

  /// <summary>Length of the NAME_MAIDEN attribute.</summary>
  public const int NameMaiden_MaxLength = 17;

  /// <summary>
  /// The value of the NAME_MAIDEN attribute.
  /// The maiden name of the CSE Person
  /// </summary>
  [JsonPropertyName("nameMaiden")]
  [Member(Index = 17, Type = MemberType.Char, Length = NameMaiden_MaxLength, Optional
    = true)]
  public string NameMaiden
  {
    get => Get<string>("nameMaiden");
    set =>
      Set("nameMaiden", TrimEnd(Substring(value, 1, NameMaiden_MaxLength)));
  }

  /// <summary>
  /// The value of the HOME_PHONE attribute.
  /// The home phone number of the CSE Person
  /// </summary>
  [JsonPropertyName("homePhone")]
  [Member(Index = 18, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? HomePhone
  {
    get => Get<int?>("homePhone");
    set => Set("homePhone", value);
  }

  /// <summary>
  /// The value of the OTHER_NUMBER attribute.
  /// An alternative method of contacting a CSE Person.  Possibly the only way!
  /// </summary>
  [JsonPropertyName("otherNumber")]
  [Member(Index = 19, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? OtherNumber
  {
    get => Get<int?>("otherNumber");
    set => Set("otherNumber", value);
  }

  /// <summary>Length of the BIRTH_PLACE_CITY attribute.</summary>
  public const int BirthPlaceCity_MaxLength = 15;

  /// <summary>
  /// The value of the BIRTH_PLACE_CITY attribute.
  /// The City in which the CSE Person was born in
  /// </summary>
  [JsonPropertyName("birthPlaceCity")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = BirthPlaceCity_MaxLength, Optional = true)]
  public string BirthPlaceCity
  {
    get => Get<string>("birthPlaceCity");
    set => Set(
      "birthPlaceCity", TrimEnd(Substring(value, 1, BirthPlaceCity_MaxLength)));
      
  }

  /// <summary>
  /// The value of the WEIGHT attribute.
  /// The weight of the CSE Person in pounds
  /// </summary>
  [JsonPropertyName("weight")]
  [Member(Index = 21, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? Weight
  {
    get => Get<int?>("weight");
    set => Set("weight", value);
  }

  /// <summary>Length of the OTHER_ID_INFO attribute.</summary>
  public const int OtherIdInfo_MaxLength = 80;

  /// <summary>
  /// The value of the OTHER_ID_INFO attribute.
  /// A description of scars, marks, or tatoos the would be distinguishing or 
  /// identifying.
  /// </summary>
  [JsonPropertyName("otherIdInfo")]
  [Member(Index = 22, Type = MemberType.Varchar, Length
    = OtherIdInfo_MaxLength, Optional = true)]
  public string OtherIdInfo
  {
    get => Get<string>("otherIdInfo");
    set => Set("otherIdInfo", Substring(value, 1, OtherIdInfo_MaxLength));
  }

  /// <summary>Length of the CURRENT_MARITAL_STATUS attribute.</summary>
  public const int CurrentMaritalStatus_MaxLength = 2;

  /// <summary>
  /// The value of the CURRENT_MARITAL_STATUS attribute.
  /// Indicates the current marital status of the CSE Person
  /// MA	Married
  /// SI	Single
  /// DV	Divorced
  /// SE	Separated
  /// CL	Common Law
  /// LS            Legally Separated
  /// NM
  /// Never Married
  /// 
  /// WI            Widowed
  /// </summary>
  [JsonPropertyName("currentMaritalStatus")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = CurrentMaritalStatus_MaxLength, Optional = true)]
  public string CurrentMaritalStatus
  {
    get => Get<string>("currentMaritalStatus");
    set => Set(
      "currentMaritalStatus", TrimEnd(Substring(value, 1,
      CurrentMaritalStatus_MaxLength)));
  }

  /// <summary>Length of the CURRENT_SPOUSE_LAST_NAME attribute.</summary>
  public const int CurrentSpouseLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CURRENT_SPOUSE_LAST_NAME attribute.
  /// The last name of a CSE Person's current spouse
  /// </summary>
  [JsonPropertyName("currentSpouseLastName")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = CurrentSpouseLastName_MaxLength, Optional = true)]
  public string CurrentSpouseLastName
  {
    get => Get<string>("currentSpouseLastName");
    set => Set(
      "currentSpouseLastName", TrimEnd(Substring(value, 1,
      CurrentSpouseLastName_MaxLength)));
  }

  /// <summary>Length of the RACE attribute.</summary>
  public const int Race_MaxLength = 2;

  /// <summary>
  /// The value of the RACE attribute.
  /// The race of the CSE Person
  /// </summary>
  [JsonPropertyName("race")]
  [Member(Index = 25, Type = MemberType.Char, Length = Race_MaxLength, Optional
    = true)]
  public string Race
  {
    get => Get<string>("race");
    set => Set("race", TrimEnd(Substring(value, 1, Race_MaxLength)));
  }

  /// <summary>
  /// The value of the HEIGHT_FT attribute.
  /// The height of the CSE Person in feet, in conjunction with inches
  /// </summary>
  [JsonPropertyName("heightFt")]
  [Member(Index = 26, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? HeightFt
  {
    get => Get<int?>("heightFt");
    set => Set("heightFt", value);
  }

  /// <summary>
  /// The value of the HEIGHT_IN attribute.
  /// The height of the CSE Person in inches in conjunction with the height in 
  /// feet
  /// </summary>
  [JsonPropertyName("heightIn")]
  [Member(Index = 27, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? HeightIn
  {
    get => Get<int?>("heightIn");
    set => Set("heightIn", value);
  }

  /// <summary>Length of the HAIR_COLOR attribute.</summary>
  public const int HairColor_MaxLength = 2;

  /// <summary>
  /// The value of the HAIR_COLOR attribute.
  /// The color of the CSE Person's hair
  /// </summary>
  [JsonPropertyName("hairColor")]
  [Member(Index = 28, Type = MemberType.Char, Length = HairColor_MaxLength, Optional
    = true)]
  public string HairColor
  {
    get => Get<string>("hairColor");
    set => Set("hairColor", TrimEnd(Substring(value, 1, HairColor_MaxLength)));
  }

  /// <summary>Length of the EYE_COLOR attribute.</summary>
  public const int EyeColor_MaxLength = 2;

  /// <summary>
  /// The value of the EYE_COLOR attribute.
  /// The color of the CSE Person's eyes
  /// </summary>
  [JsonPropertyName("eyeColor")]
  [Member(Index = 29, Type = MemberType.Char, Length = EyeColor_MaxLength, Optional
    = true)]
  public string EyeColor
  {
    get => Get<string>("eyeColor");
    set => Set("eyeColor", TrimEnd(Substring(value, 1, EyeColor_MaxLength)));
  }

  /// <summary>
  /// The value of the HOME_PHONE_AREA_CODE attribute.
  /// The area code of the home phone.
  /// </summary>
  [JsonPropertyName("homePhoneAreaCode")]
  [Member(Index = 30, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? HomePhoneAreaCode
  {
    get => Get<int?>("homePhoneAreaCode");
    set => Set("homePhoneAreaCode", value);
  }

  /// <summary>
  /// The value of the EMERGENCY AREA CODE attribute.
  /// The area code of the emergency phone.
  /// </summary>
  [JsonPropertyName("emergencyAreaCode")]
  [Member(Index = 31, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? EmergencyAreaCode
  {
    get => Get<int?>("emergencyAreaCode");
    set => Set("emergencyAreaCode", value);
  }

  /// <summary>
  /// The value of the OTHER AREA CODE attribute.
  /// The area code of the other phone number.
  /// </summary>
  [JsonPropertyName("otherAreaCode")]
  [Member(Index = 32, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtherAreaCode
  {
    get => Get<int?>("otherAreaCode");
    set => Set("otherAreaCode", value);
  }

  /// <summary>Length of the OTHER_PHONE_TYPE attribute.</summary>
  public const int OtherPhoneType_MaxLength = 1;

  /// <summary>
  /// The value of the OTHER_PHONE_TYPE attribute.
  /// A type of phone number in order to contact the AP other than home or work.
  /// </summary>
  [JsonPropertyName("otherPhoneType")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = OtherPhoneType_MaxLength, Optional = true)]
  public string OtherPhoneType
  {
    get => Get<string>("otherPhoneType");
    set => Set(
      "otherPhoneType", TrimEnd(Substring(value, 1, OtherPhoneType_MaxLength)));
      
  }

  /// <summary>
  /// The value of the WORK_PHONE_AREA_CODE attribute.
  /// The area code of the phone number used to contact a person at work.
  /// </summary>
  [JsonPropertyName("workPhoneAreaCode")]
  [Member(Index = 34, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? WorkPhoneAreaCode
  {
    get => Get<int?>("workPhoneAreaCode");
    set => Set("workPhoneAreaCode", value);
  }

  /// <summary>
  /// The value of the WORK_PHONE attribute.
  /// The telephone number where a person can be reached at work.
  /// </summary>
  [JsonPropertyName("workPhone")]
  [Member(Index = 35, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? WorkPhone
  {
    get => Get<int?>("workPhone");
    set => Set("workPhone", value);
  }

  /// <summary>Length of the WORK_PHONE_EXT attribute.</summary>
  public const int WorkPhoneExt_MaxLength = 4;

  /// <summary>
  /// The value of the WORK_PHONE_EXT attribute.
  /// The extension number associated to a person's work telephone number.
  /// </summary>
  [JsonPropertyName("workPhoneExt")]
  [Member(Index = 36, Type = MemberType.Char, Length = WorkPhoneExt_MaxLength, Optional
    = true)]
  public string WorkPhoneExt
  {
    get => Get<string>("workPhoneExt");
    set => Set(
      "workPhoneExt", TrimEnd(Substring(value, 1, WorkPhoneExt_MaxLength)));
  }

  /// <summary>Length of the UNEMPLOYMENT_IND attribute.</summary>
  public const int UnemploymentInd_MaxLength = 1;

  /// <summary>
  /// The value of the UNEMPLOYMENT_IND attribute.
  /// This indicates whether the income source is for unemployment benefits.
  /// </summary>
  [JsonPropertyName("unemploymentInd")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = UnemploymentInd_MaxLength, Optional = true)]
  public string UnemploymentInd
  {
    get => Get<string>("unemploymentInd");
    set => Set(
      "unemploymentInd",
      TrimEnd(Substring(value, 1, UnemploymentInd_MaxLength)));
  }

  /// <summary>Length of the FEDERAL_IND attribute.</summary>
  public const int FederalInd_MaxLength = 1;

  /// <summary>
  /// The value of the FEDERAL_IND attribute.
  /// THIS INDICATES THAT THIS IS A FEDERAL BENEFIT.
  /// </summary>
  [JsonPropertyName("federalInd")]
  [Member(Index = 38, Type = MemberType.Char, Length = FederalInd_MaxLength, Optional
    = true)]
  public string FederalInd
  {
    get => Get<string>("federalInd");
    set =>
      Set("federalInd", TrimEnd(Substring(value, 1, FederalInd_MaxLength)));
  }

  /// <summary>Length of the FAMILY_VIOLENCE_INDICATOR attribute.</summary>
  public const int FamilyViolenceIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE_INDICATOR attribute.
  /// The purpose of the family violence indicator(FVI) is to protect the 
  /// whereabouts of persons at risk of physical or emotional harm, regardless
  /// of the identity of the alleged perpetrator. Address or phone number (any
  /// kind of contact) information that would allow a person to locate persons
  /// at risk must be protected from disclosure. The proposed values for family
  /// violence indicator are as follows. This will be stored in a code value
  /// table and may be modified at the direction of CSE.
  /// 
  /// P=Protective Order issued
  /// C=Child abuse
  /// suspected
  /// D=Domestic violence suspected
  /// blank=No family violence
  /// indicated
  /// </summary>
  [JsonPropertyName("familyViolenceIndicator")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = FamilyViolenceIndicator_MaxLength, Optional = true)]
  public string FamilyViolenceIndicator
  {
    get => Get<string>("familyViolenceIndicator");
    set => Set(
      "familyViolenceIndicator", TrimEnd(Substring(value, 1,
      FamilyViolenceIndicator_MaxLength)));
  }

  /// <summary>Length of the BORN_OUT_OF_WEDLOCK attribute.</summary>
  public const int BornOutOfWedlock_MaxLength = 1;

  /// <summary>
  /// The value of the BORN_OUT_OF_WEDLOCK attribute.
  /// An indicator which describes the current knowledge about whether this 
  /// person was born out of wedlock. It is used for persons who are assigned
  /// Child roles on CSE Cases.                  This indicator can have any of
  /// the following 3 values:               U=Unknown, It is not known if this
  /// person was born out of wedlock or not.
  /// 
  /// N=No, Person (Child) was not born out of wedlock.                 Y=Yes,
  /// Person(Child) was born out of wedlock.                       This
  /// indicator is to be defaulted to 'U' at the time it is created unless one
  /// of the other 2 values is known to be true.
  /// </summary>
  [JsonPropertyName("bornOutOfWedlock")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = BornOutOfWedlock_MaxLength, Optional = true)]
  public string BornOutOfWedlock
  {
    get => Get<string>("bornOutOfWedlock");
    set => Set(
      "bornOutOfWedlock",
      TrimEnd(Substring(value, 1, BornOutOfWedlock_MaxLength)));
  }

  /// <summary>Length of the CSE_TO_ESTBL_PATERNITY attribute.</summary>
  public const int CseToEstblPaternity_MaxLength = 1;

  /// <summary>
  /// The value of the CSE_TO_ESTBL_PATERNITY attribute.
  /// An indicator which describes the current knowledge about whether CSE has 
  /// to (or had to) Establish Paternity (Paternity was at Issue) when this
  /// person (Child) became involved in a CSE Case. It is used for persons who
  /// are assigned Child roles on CSE Cases. This indicator is used to capture
  /// this data for Federal Reporting Purposes.
  /// This indicator can have any of the
  /// following 3 values:             U=Unknown, It is not known if CSE to
  /// establish Paternity for this person or not.
  /// 
  /// N=No, CSE does (did) not have to Establish Paternity for this Person (
  /// Child).
  /// Y=Yes,
  /// CSE does (did) have to Establish Paternity for this Person (Child).
  /// 
  /// This indicator is to be defaulted to 'U' at the time it is created unless
  /// one of the other 2 values is known to be true.
  /// </summary>
  [JsonPropertyName("cseToEstblPaternity")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = CseToEstblPaternity_MaxLength, Optional = true)]
  public string CseToEstblPaternity
  {
    get => Get<string>("cseToEstblPaternity");
    set => Set(
      "cseToEstblPaternity", TrimEnd(Substring(value, 1,
      CseToEstblPaternity_MaxLength)));
  }

  /// <summary>Length of the PATERNITY_ESTABLISHED_INDICATOR attribute.
  /// </summary>
  public const int PaternityEstablishedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_ESTABLISHED_INDICATOR attribute.
  /// An indicator which describes the current knowledge about whether Paternity
  /// has been Established for this person (Child). It is used for persons who
  /// are assigned Child roles on CSE Cases. This indicator can have either of
  /// the following 2 values: N=No,Paternity has not been Established for this
  /// Person (Child).  Y=Yes, Paternity has been Established for this Person (
  /// Child).     This indicator is to be defaulted to 'N' at the time it is
  /// created unless it is known that Paternity has been Established at that
  /// time. For the Paternity Established Indicator, we want to store the date
  /// the Paternity Established(PE) was changed to Y. When Paternity is
  /// Established we want an event on HIST to show that Paternity is
  /// Established. If ever the Paternity Established(PE) indicator is changed
  /// from a Y back to N, then we want an event on HIST to show that Paternity
  /// needs to be established again. If PE is changed back to N, we also need to
  /// reset the Paternity Established Date to low-value date. Since it is no
  /// longer established, it should not be included in any counts as established
  /// during the fiscal year.
  /// </summary>
  [JsonPropertyName("paternityEstablishedIndicator")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = PaternityEstablishedIndicator_MaxLength, Optional = true)]
  public string PaternityEstablishedIndicator
  {
    get => Get<string>("paternityEstablishedIndicator");
    set => Set(
      "paternityEstablishedIndicator", TrimEnd(Substring(value, 1,
      PaternityEstablishedIndicator_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_PATERN_ESTAB attribute.
  /// This attribute is used to capture the Date that the Paternity Established 
  /// Indicator was set to the value &quot;Y&quot;. It will have a default value
  /// of Null Date(00010101) at creation time. Once, the Paternity Established
  /// Indicator is set to &quot;Y&quot;, and it is later changed back to &quot;N
  /// &quot;, then this attribute must be reset to the default value (00010101).
  /// This attribute will have its value set based on the current computer
  /// date. I.E. It is not set directly based on a date value input by the user.
  /// </summary>
  [JsonPropertyName("datePaternEstab")]
  [Member(Index = 43, Type = MemberType.Date)]
  public DateTime? DatePaternEstab
  {
    get => Get<DateTime?>("datePaternEstab");
    set => Set("datePaternEstab", value);
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_LAST_NAME attribute.</summary>
  public const int BirthCertFathersLastName_MaxLength = 17;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_LAST_NAME attribute.
  /// Last name of father on birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersLastName")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = BirthCertFathersLastName_MaxLength, Optional = true)]
  public string BirthCertFathersLastName
  {
    get => Get<string>("birthCertFathersLastName");
    set => Set(
      "birthCertFathersLastName", TrimEnd(Substring(value, 1,
      BirthCertFathersLastName_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_FIRST_NAME attribute.</summary>
  public const int BirthCertFathersFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_FIRST_NAME attribute.
  /// First name of father on birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersFirstName")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = BirthCertFathersFirstName_MaxLength, Optional = true)]
  public string BirthCertFathersFirstName
  {
    get => Get<string>("birthCertFathersFirstName");
    set => Set(
      "birthCertFathersFirstName", TrimEnd(Substring(value, 1,
      BirthCertFathersFirstName_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERT_FATHERS_MI attribute.</summary>
  public const int BirthCertFathersMi_MaxLength = 1;

  /// <summary>
  /// The value of the BIRTH_CERT_FATHERS_MI attribute.
  /// Fathers middle initial on the birth certificate.
  /// </summary>
  [JsonPropertyName("birthCertFathersMi")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = BirthCertFathersMi_MaxLength, Optional = true)]
  public string BirthCertFathersMi
  {
    get => Get<string>("birthCertFathersMi");
    set => Set(
      "birthCertFathersMi", TrimEnd(Substring(value, 1,
      BirthCertFathersMi_MaxLength)));
  }

  /// <summary>Length of the BIRTH_CERTIFICATE_SIGNATURE attribute.</summary>
  public const int BirthCertificateSignature_MaxLength = 1;

  /// <summary>
  /// The value of the BIRTH_CERTIFICATE_SIGNATURE attribute.
  /// Indicates whether or not the father's signature is on the birth 
  /// certificate
  /// 
  /// Y=Yes, the father's signature is on the birth certificate.
  /// N=No, the father's signature is not on the birth certificate.      Blank
  /// </summary>
  [JsonPropertyName("birthCertificateSignature")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = BirthCertificateSignature_MaxLength, Optional = true)]
  public string BirthCertificateSignature
  {
    get => Get<string>("birthCertificateSignature");
    set => Set(
      "birthCertificateSignature", TrimEnd(Substring(value, 1,
      BirthCertificateSignature_MaxLength)));
  }

  /// <summary>
  /// The value of the FV_LETTER_SENT_DATE attribute.
  /// RESP:  SI
  /// 
  /// The purpose of the FV_LETTER_SENT_DATE is to provide a place to store the
  /// date when a letter is sent notifying the recipient that the Family
  /// Violence Indicator is going to be removed (i.e. Blanked out) for that
  /// person on the CSE system.  When a letter is sent notifying the recipient
  /// that the Family Violence Indicator is going to be removed, program logic
  /// will populate this attribute with the current date.  The attribute will
  /// retain this date value until such time as either the printing and sending
  /// of the notification letter is cancelled, or the Family Violence Indicator
  /// is removed (i.e. Blanked out).  When either of these actions occur,
  /// program logic will automatically reset the value of this attribute back to
  /// the default value (i.e. 0001-01-01).  Once the value of this date is
  /// greater than 0001-01-01, then program logic will also enforce the
  /// following rules:                                                       1.
  /// The Family Violence Indicator will only be allowed to be turned off (
  /// Blanked out) if the date that the user is attempting to blank it out is at
  /// least 1 day greater than the value of the date in this attribute, AND if
  /// the date that the user is attempting to blank it out is no more than 30
  /// days greater than the value of the date in this attribute.
  /// 2.  The user will not be allowed to reporint
  /// the notification letter until the date they are attempting to do it is
  /// more than 30 days greater than the date value in this attribute.
  /// </summary>
  [JsonPropertyName("fvLetterSentDate")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? FvLetterSentDate
  {
    get => Get<DateTime?>("fvLetterSentDate");
    set => Set("fvLetterSentDate", value);
  }

  /// <summary>Length of the BIRTHPLACE_COUNTRY attribute.</summary>
  public const int BirthplaceCountry_MaxLength = 2;

  /// <summary>
  /// The value of the BIRTHPLACE_COUNTRY attribute.
  /// The foreign country where the cse_person was born in.
  /// </summary>
  [JsonPropertyName("birthplaceCountry")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = BirthplaceCountry_MaxLength, Optional = true)]
  public string BirthplaceCountry
  {
    get => Get<string>("birthplaceCountry");
    set => Set(
      "birthplaceCountry", TrimEnd(Substring(value, 1,
      BirthplaceCountry_MaxLength)));
  }

  /// <summary>Length of the HOSPITAL_PAT_EST_IND attribute.</summary>
  public const int HospitalPatEstInd_MaxLength = 1;

  /// <summary>
  /// The value of the HOSPITAL_PAT_EST_IND attribute.
  /// Indicates if paternity was established in a Kansas hospital.  Values are '
  /// Y', 'N', and space.
  /// </summary>
  [JsonPropertyName("hospitalPatEstInd")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = HospitalPatEstInd_MaxLength, Optional = true)]
  public string HospitalPatEstInd
  {
    get => Get<string>("hospitalPatEstInd");
    set => Set(
      "hospitalPatEstInd", TrimEnd(Substring(value, 1,
      HospitalPatEstInd_MaxLength)));
  }

  /// <summary>Length of the PRIOR_TAF_IND attribute.</summary>
  public const int PriorTafInd_MaxLength = 1;

  /// <summary>
  /// The value of the PRIOR_TAF_IND attribute.
  /// Identifies if the person was a TAF recipient prior to their Kansas IV-D 
  /// involvement; a determining factor in categorizing disbursement amounts for
  /// the pruoses of the Deficit Reduction Act fee.  Values are: Y, &lt;space>.
  /// </summary>
  [JsonPropertyName("priorTafInd")]
  [Member(Index = 51, Type = MemberType.Char, Length = PriorTafInd_MaxLength, Optional
    = true)]
  public string PriorTafInd
  {
    get => Get<string>("priorTafInd");
    set => Set(
      "priorTafInd", TrimEnd(Substring(value, 1, PriorTafInd_MaxLength)));
  }

  /// <summary>
  /// The value of the FVI_SET_DATE attribute.
  /// Identifies the date in which the FAMILY_VIOLENCE_INDICATOR attribute was 
  /// last created or updated.
  /// </summary>
  [JsonPropertyName("fviSetDate")]
  [Member(Index = 52, Type = MemberType.Date, Optional = true)]
  public DateTime? FviSetDate
  {
    get => Get<DateTime?>("fviSetDate");
    set => Set("fviSetDate", value);
  }

  /// <summary>Length of the FVI_UPDATED_BY attribute.</summary>
  public const int FviUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the FVI_UPDATED_BY attribute.
  /// Identifies the person that last updated the FAMILY_VIOLENCE_INDICATOR 
  /// attribute.
  /// </summary>
  [JsonPropertyName("fviUpdatedBy")]
  [Member(Index = 53, Type = MemberType.Char, Length = FviUpdatedBy_MaxLength, Optional
    = true)]
  public string FviUpdatedBy
  {
    get => Get<string>("fviUpdatedBy");
    set => Set(
      "fviUpdatedBy", TrimEnd(Substring(value, 1, FviUpdatedBy_MaxLength)));
  }

  /// <summary>Length of the TEXT_MESSAGE_INDICATOR attribute.</summary>
  public const int TextMessageIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the TEXT_MESSAGE_INDICATOR attribute.
  /// This indicator will show if the person is willing to be contacted by a 
  /// text message.
  /// </summary>
  [JsonPropertyName("textMessageIndicator")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = TextMessageIndicator_MaxLength, Optional = true)]
  public string TextMessageIndicator
  {
    get => Get<string>("textMessageIndicator");
    set => Set(
      "textMessageIndicator", TrimEnd(Substring(value, 1,
      TextMessageIndicator_MaxLength)));
  }

  /// <summary>Length of the PATERNITY_LOCK_IND attribute.</summary>
  public const int PaternityLockInd_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_LOCK_IND attribute.
  /// Indicator of whether the paternity info for a child is currently locked on
  /// CPAT.  Values Y,N, blank.
  /// </summary>
  [JsonPropertyName("paternityLockInd")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = PaternityLockInd_MaxLength, Optional = true)]
  public string PaternityLockInd
  {
    get => Get<string>("paternityLockInd");
    set => Set(
      "paternityLockInd",
      TrimEnd(Substring(value, 1, PaternityLockInd_MaxLength)));
  }

  /// <summary>
  /// The value of the PATERNITY_LOCK_UPDATE_DATE attribute.
  /// Date on which the paternity info for a child was most recently locked or 
  /// unlocked on CPAT.
  /// </summary>
  [JsonPropertyName("paternityLockUpdateDate")]
  [Member(Index = 56, Type = MemberType.Date, Optional = true)]
  public DateTime? PaternityLockUpdateDate
  {
    get => Get<DateTime?>("paternityLockUpdateDate");
    set => Set("paternityLockUpdateDate", value);
  }

  /// <summary>Length of the PATERNITY_LOCK_UPDATED_BY attribute.</summary>
  public const int PaternityLockUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the PATERNITY_LOCK_UPDATED_BY attribute.
  /// User id that most recently locked or unlocked the childs paternity info on
  /// CPAT.
  /// </summary>
  [JsonPropertyName("paternityLockUpdatedBy")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = PaternityLockUpdatedBy_MaxLength, Optional = true)]
  public string PaternityLockUpdatedBy
  {
    get => Get<string>("paternityLockUpdatedBy");
    set => Set(
      "paternityLockUpdatedBy", TrimEnd(Substring(value, 1,
      PaternityLockUpdatedBy_MaxLength)));
  }

  /// <summary>Length of the TRIBAL_CODE attribute.</summary>
  public const int TribalCode_MaxLength = 10;

  /// <summary>
  /// The value of the TRIBAL_CODE attribute.
  /// A place to put the tribal code
  /// </summary>
  [JsonPropertyName("tribalCode")]
  [Member(Index = 58, Type = MemberType.Char, Length = TribalCode_MaxLength, Optional
    = true)]
  public string TribalCode
  {
    get => Get<string>("tribalCode");
    set =>
      Set("tribalCode", TrimEnd(Substring(value, 1, TribalCode_MaxLength)));
  }

  /// <summary>Length of the THREAT_ON_STAFF attribute.</summary>
  public const int ThreatOnStaff_MaxLength = 1;

  /// <summary>
  /// The value of the THREAT_ON_STAFF attribute.
  /// Was a threat to Staff been make? Y or spaces or N
  /// </summary>
  [JsonPropertyName("threatOnStaff")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = ThreatOnStaff_MaxLength, Optional = true)]
  public string ThreatOnStaff
  {
    get => Get<string>("threatOnStaff");
    set => Set(
      "threatOnStaff", TrimEnd(Substring(value, 1, ThreatOnStaff_MaxLength)));
  }

  /// <summary>Length of the CUSTOMER_SERVICE_CODE attribute.</summary>
  public const int CustomerServiceCode_MaxLength = 1;

  /// <summary>
  /// The value of the CUSTOMER_SERVICE_CODE attribute.
  /// Code indicating if the client is willing to be contacted by text or email 
  /// for customer service inquires
  /// </summary>
  [JsonPropertyName("customerServiceCode")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = CustomerServiceCode_MaxLength, Optional = true)]
  public string CustomerServiceCode
  {
    get => Get<string>("customerServiceCode");
    set => Set(
      "customerServiceCode", TrimEnd(Substring(value, 1,
      CustomerServiceCode_MaxLength)));
  }

  /// <summary>Length of the TAX_ID_SUFFIX attribute.</summary>
  public const int TaxIdSuffix_MaxLength = 2;

  /// <summary>
  /// The value of the TAX_ID_SUFFIX attribute.
  /// This attribute specifies the 2 character suffix for the Tax ID.
  /// </summary>
  [JsonPropertyName("taxIdSuffix")]
  [Member(Index = 61, Type = MemberType.Char, Length = TaxIdSuffix_MaxLength, Optional
    = true)]
  public string TaxIdSuffix
  {
    get => Get<string>("taxIdSuffix");
    set => Set(
      "taxIdSuffix", TrimEnd(Substring(value, 1, TaxIdSuffix_MaxLength)));
  }

  /// <summary>Length of the TAX_ID attribute.</summary>
  public const int TaxId_MaxLength = 9;

  /// <summary>
  /// The value of the TAX_ID attribute.
  /// The tax id associated with the organisation.
  /// </summary>
  [JsonPropertyName("taxId")]
  [Member(Index = 62, Type = MemberType.Char, Length = TaxId_MaxLength, Optional
    = true)]
  public string TaxId
  {
    get => Get<string>("taxId");
    set => Set("taxId", TrimEnd(Substring(value, 1, TaxId_MaxLength)));
  }

  /// <summary>Length of the ORGANIZATION_NAME attribute.</summary>
  public const int OrganizationName_MaxLength = 33;

  /// <summary>
  /// The value of the ORGANIZATION_NAME attribute.
  /// The name of the Organisation.
  /// </summary>
  [JsonPropertyName("organizationName")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = OrganizationName_MaxLength, Optional = true)]
  public string OrganizationName
  {
    get => Get<string>("organizationName");
    set => Set(
      "organizationName",
      TrimEnd(Substring(value, 1, OrganizationName_MaxLength)));
  }
}

// The source file: INCARCERATION, ID: 371435343, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Information necessary to track the AP if he/she has been convicted of a 
/// criminal offense.  The child support obligation continues to accrue when the
/// AP is in prison or jail unless ordered halted by the court.
/// FED REQ: B-1.a.6
/// </summary>
[Serializable]
public partial class Incarceration: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Incarceration()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Incarceration(Incarceration that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Incarceration Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Incarceration that)
  {
    base.Assign(that);
    phoneAreaCode = that.phoneAreaCode;
    phoneExt = that.phoneExt;
    identifier = that.identifier;
    verifiedUserId = that.verifiedUserId;
    verifiedDate = that.verifiedDate;
    inmateNumber = that.inmateNumber;
    paroleEligibilityDate = that.paroleEligibilityDate;
    workReleaseInd = that.workReleaseInd;
    conditionsForRelease = that.conditionsForRelease;
    paroleOfficersTitle = that.paroleOfficersTitle;
    phone = that.phone;
    endDate = that.endDate;
    startDate = that.startDate;
    type1 = that.type1;
    institutionName = that.institutionName;
    paroleOfficerLastName = that.paroleOfficerLastName;
    paroleOfficerFirstName = that.paroleOfficerFirstName;
    paroleOfficerMiddleInitial = that.paroleOfficerMiddleInitial;
    endDateModInd = that.endDateModInd;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    incarcerated = that.incarcerated;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number used to contact the parole 
  /// officer of the CSE Person.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the PHONE_EXT attribute.</summary>
  public const int PhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXT attribute.
  /// The 5 digit extension for the phone number of the incarceration 
  /// institution.
  /// </summary>
  [JsonPropertyName("phoneExt")]
  [Member(Index = 2, Type = MemberType.Char, Length = PhoneExt_MaxLength, Optional
    = true)]
  public string PhoneExt
  {
    get => phoneExt;
    set => phoneExt = value != null
      ? TrimEnd(Substring(value, 1, PhoneExt_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute which, together with relation to CSE_PERSON, uniquely 
  /// identifies one instance of INCARCERATION record.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// Key used to identify the CSE worker who verified or entered the 
  /// incarceration information.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// Date the incarceration information for a CSE Person was confirmed by the 
  /// institution.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the INMATE_NUMBER attribute.</summary>
  public const int InmateNumber_MaxLength = 10;

  /// <summary>
  /// The value of the INMATE_NUMBER attribute.
  /// Identifying number assigned to the CSE Person when they enter 
  /// incarceration.
  /// State numbers are always numeric.
  /// Federal numbers can be numeric or alphanumeric.
  /// </summary>
  [JsonPropertyName("inmateNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length = InmateNumber_MaxLength, Optional
    = true)]
  public string InmateNumber
  {
    get => inmateNumber;
    set => inmateNumber = value != null
      ? TrimEnd(Substring(value, 1, InmateNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PAROLE_ELIGIBILITY_DATE attribute.
  /// The expected date when the CSE Person will have served enough time on his 
  /// sentence to the released into society with supervision.
  /// </summary>
  [JsonPropertyName("paroleEligibilityDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? ParoleEligibilityDate
  {
    get => paroleEligibilityDate;
    set => paroleEligibilityDate = value;
  }

  /// <summary>Length of the WORK_RELEASE_IND attribute.</summary>
  public const int WorkReleaseInd_MaxLength = 1;

  /// <summary>
  /// The value of the WORK_RELEASE_IND attribute.
  /// A yes/no flag indicating that the CSE Person is allowed to leave the 
  /// institution of incarceration to perform work.
  /// </summary>
  [JsonPropertyName("workReleaseInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = WorkReleaseInd_MaxLength, Optional = true)]
  public string WorkReleaseInd
  {
    get => workReleaseInd;
    set => workReleaseInd = value != null
      ? TrimEnd(Substring(value, 1, WorkReleaseInd_MaxLength)) : null;
  }

  /// <summary>Length of the CONDITIONS_FOR_RELEASE attribute.</summary>
  public const int ConditionsForRelease_MaxLength = 70;

  /// <summary>
  /// The value of the CONDITIONS_FOR_RELEASE attribute.
  /// The criteria which must be met by a CSE Person to be released on parole.  
  /// For example: paying for damages, paying child support, finding housing and
  /// work, staying out of trouble.
  /// </summary>
  [JsonPropertyName("conditionsForRelease")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ConditionsForRelease_MaxLength, Optional = true)]
  public string ConditionsForRelease
  {
    get => conditionsForRelease;
    set => conditionsForRelease = value != null
      ? TrimEnd(Substring(value, 1, ConditionsForRelease_MaxLength)) : null;
  }

  /// <summary>Length of the PAROLE_OFFICERS_TITLE attribute.</summary>
  public const int ParoleOfficersTitle_MaxLength = 10;

  /// <summary>
  /// The value of the PAROLE_OFFICERS_TITLE attribute.
  /// The title identifying the contact who is responsible for monitoring the 
  /// actions of the CSE Person when he is paroled.
  /// This is not an attribute that CSE would anticipate needing.  It could be 
  /// deleted.
  /// </summary>
  [JsonPropertyName("paroleOfficersTitle")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ParoleOfficersTitle_MaxLength, Optional = true)]
  public string ParoleOfficersTitle
  {
    get => paroleOfficersTitle;
    set => paroleOfficersTitle = value != null
      ? TrimEnd(Substring(value, 1, ParoleOfficersTitle_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE attribute.
  /// The phone number used to contact the parole officer of the CSE Person.  
  /// The phone number is specified as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("phone")]
  [Member(Index = 11, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Phone
  {
    get => phone;
    set => phone = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date a CSE Person expects to be released from a penal institution.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// The date a CSE Person entered a penal institution.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Futher description of offense.  Whether person is in jail, prison, on 
  /// probation/parole, or in work release program.
  /// The code values and descriptions are kept in CODE_VALUE table.
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 14, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the INSTITUTION_NAME attribute.</summary>
  public const int InstitutionName_MaxLength = 33;

  /// <summary>
  /// The value of the INSTITUTION_NAME attribute.
  /// The name of the institution where a CSE Person is serving time for being 
  /// convicted of a crime.
  /// </summary>
  [JsonPropertyName("institutionName")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = InstitutionName_MaxLength, Optional = true)]
  public string InstitutionName
  {
    get => institutionName;
    set => institutionName = value != null
      ? TrimEnd(Substring(value, 1, InstitutionName_MaxLength)) : null;
  }

  /// <summary>Length of the PAROLE_OFFICER_LAST_NAME attribute.</summary>
  public const int ParoleOfficerLastName_MaxLength = 17;

  /// <summary>
  /// The value of the PAROLE_OFFICER_LAST_NAME attribute.
  /// The surname of the contact who is supervising the CSE Person as a 
  /// condition of being convicted of a crime.
  /// Parole:  The CSE Person must meet with a representative of the court as a 
  /// condition of release from prison.
  /// Probation:  The CSE Person must meet with a court officer who 
  /// investigates, reports on and supervises the CSE Person as a convicted
  /// offender who did not serve any or full time in prison.
  /// </summary>
  [JsonPropertyName("paroleOfficerLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ParoleOfficerLastName_MaxLength, Optional = true)]
  public string ParoleOfficerLastName
  {
    get => paroleOfficerLastName;
    set => paroleOfficerLastName = value != null
      ? TrimEnd(Substring(value, 1, ParoleOfficerLastName_MaxLength)) : null;
  }

  /// <summary>Length of the PAROLE_OFFICER_FIRST_NAME attribute.</summary>
  public const int ParoleOfficerFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the PAROLE_OFFICER_FIRST_NAME attribute.
  /// The given name of the contact who is supervising the CSE Person as a 
  /// condition of being convicted of a crime.
  /// </summary>
  [JsonPropertyName("paroleOfficerFirstName")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ParoleOfficerFirstName_MaxLength, Optional = true)]
  public string ParoleOfficerFirstName
  {
    get => paroleOfficerFirstName;
    set => paroleOfficerFirstName = value != null
      ? TrimEnd(Substring(value, 1, ParoleOfficerFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the PAROLE_OFFICER_MIDDLE_INITIAL attribute.</summary>
  public const int ParoleOfficerMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the PAROLE_OFFICER_MIDDLE_INITIAL attribute.
  /// The initial of the contact who is supervising the CSE Person as a 
  /// condition of being convicted of a crime.
  /// </summary>
  [JsonPropertyName("paroleOfficerMiddleInitial")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ParoleOfficerMiddleInitial_MaxLength, Optional = true)]
  public string ParoleOfficerMiddleInitial
  {
    get => paroleOfficerMiddleInitial;
    set => paroleOfficerMiddleInitial = value != null
      ? TrimEnd(Substring(value, 1, ParoleOfficerMiddleInitial_MaxLength)) : null
      ;
  }

  /// <summary>Length of the END_DATE_MOD_IND attribute.</summary>
  public const int EndDateModInd_MaxLength = 1;

  /// <summary>
  /// The value of the END_DATE_MOD_IND attribute.
  /// To facilitate batch processing of this entity, this indicator is set to a 
  /// value of Y when an occurrence is created and the END_DATE attribute is not
  /// (otherwise value is N), or, when an occurrence of this entity is updated
  /// and the new END_DATE value is not equal to the old END_DATE value and the
  /// END_DATE is not null (otherwise value is N). Managed exclusively by the
  /// application and never displayed to the end user.
  /// </summary>
  [JsonPropertyName("endDateModInd")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = EndDateModInd_MaxLength, Optional = true)]
  public string EndDateModInd
  {
    get => endDateModInd;
    set => endDateModInd = value != null
      ? TrimEnd(Substring(value, 1, EndDateModInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
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

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 23, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the INCARCERATED attribute.</summary>
  public const int Incarcerated_MaxLength = 1;

  /// <summary>
  /// The value of the INCARCERATED attribute.
  /// This field will be used as an indicator to check if the person is 
  /// incarcerated. The valid values are 'Y', 'N' or space. It will be used in
  /// conjunction with 'TYPE' attribute to validate if the person is in Prison,
  /// Jail, Parole, or Probation.
  /// </summary>
  [JsonPropertyName("incarcerated")]
  [Member(Index = 24, Type = MemberType.Char, Length = Incarcerated_MaxLength, Optional
    = true)]
  public string Incarcerated
  {
    get => incarcerated;
    set => incarcerated = value != null
      ? TrimEnd(Substring(value, 1, Incarcerated_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  private int? phoneAreaCode;
  private string phoneExt;
  private int identifier;
  private string verifiedUserId;
  private DateTime? verifiedDate;
  private string inmateNumber;
  private DateTime? paroleEligibilityDate;
  private string workReleaseInd;
  private string conditionsForRelease;
  private string paroleOfficersTitle;
  private int? phone;
  private DateTime? endDate;
  private DateTime? startDate;
  private string type1;
  private string institutionName;
  private string paroleOfficerLastName;
  private string paroleOfficerFirstName;
  private string paroleOfficerMiddleInitial;
  private string endDateModInd;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string incarcerated;
  private string cspNumber;
}

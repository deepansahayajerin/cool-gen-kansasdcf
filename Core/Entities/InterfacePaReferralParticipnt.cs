// The source file: INTERFACE_PA_REFERRAL_PARTICIPNT, ID: 371435799, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVIN
/// </summary>
[Serializable]
public partial class InterfacePaReferralParticipnt: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfacePaReferralParticipnt()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfacePaReferralParticipnt(InterfacePaReferralParticipnt that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfacePaReferralParticipnt Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfacePaReferralParticipnt that)
  {
    base.Assign(that);
    interfaceIdentifier = that.interfaceIdentifier;
    identifier = that.identifier;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    absenceCode = that.absenceCode;
    relationship = that.relationship;
    sex = that.sex;
    dob = that.dob;
    lastName = that.lastName;
    firstName = that.firstName;
    mi = that.mi;
    ssn = that.ssn;
    personNumber = that.personNumber;
    goodCauseStatus = that.goodCauseStatus;
    insurInd = that.insurInd;
    patEstInd = that.patEstInd;
    beneInd = that.beneInd;
    role = that.role;
  }

  /// <summary>Length of the INTERFACE_IDENTIFIER attribute.</summary>
  public const int InterfaceIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the INTERFACE_IDENTIFIER attribute.
  /// Unique identifier for this referral.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = InterfaceIdentifier_MaxLength)]
  public string InterfaceIdentifier
  {
    get => interfaceIdentifier ?? "";
    set => interfaceIdentifier =
      TrimEnd(Substring(value, 1, InterfaceIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the InterfaceIdentifier attribute.</summary>
  [JsonPropertyName("interfaceIdentifier")]
  [Computed]
  public string InterfaceIdentifier_Json
  {
    get => NullIf(InterfaceIdentifier, "");
    set => InterfaceIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute is used to uniquely identify an occurance of a PA REFERRAL 
  /// PARTICIPANT.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the ABSENCE_CODE attribute.</summary>
  public const int AbsenceCode_MaxLength = 2;

  /// <summary>
  /// The value of the ABSENCE_CODE attribute.
  /// The reason for the parent's absence from the home.
  /// AB - Abandoned
  /// DC - Deceased
  /// NM - Never married
  /// DV - Divorced
  /// IC - Incarcerated
  /// IS - Institutionalized
  /// SP - Separated
  /// </summary>
  [JsonPropertyName("absenceCode")]
  [Member(Index = 7, Type = MemberType.Char, Length = AbsenceCode_MaxLength, Optional
    = true)]
  public string AbsenceCode
  {
    get => absenceCode;
    set => absenceCode = value != null
      ? TrimEnd(Substring(value, 1, AbsenceCode_MaxLength)) : null;
  }

  /// <summary>Length of the RELATIONSHIP attribute.</summary>
  public const int Relationship_MaxLength = 2;

  /// <summary>
  /// The value of the RELATIONSHIP attribute.
  /// Indicates potential case role on future CSE Case
  /// CH - child
  /// FC - foster child
  /// SB - sibling
  /// NN - Niece/Nephew
  /// GC - grandchild
  /// CO - cousin
  /// NR - no relationship
  /// OR - other relationship
  /// </summary>
  [JsonPropertyName("relationship")]
  [Member(Index = 8, Type = MemberType.Char, Length = Relationship_MaxLength, Optional
    = true)]
  public string Relationship
  {
    get => relationship;
    set => relationship = value != null
      ? TrimEnd(Substring(value, 1, Relationship_MaxLength)) : null;
  }

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// The sex of the participant. Can be blank.
  /// M - Male
  /// F - Female
  /// </summary>
  [JsonPropertyName("sex")]
  [Member(Index = 9, Type = MemberType.Char, Length = Sex_MaxLength, Optional
    = true)]
  public string Sex
  {
    get => sex;
    set => sex = value != null ? TrimEnd(Substring(value, 1, Sex_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the DOB attribute.
  /// Date of birth
  /// </summary>
  [JsonPropertyName("dob")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? Dob
  {
    get => dob;
    set => dob = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// Last name
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 11, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// First name
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 12, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the MI attribute.</summary>
  public const int Mi_MaxLength = 1;

  /// <summary>
  /// The value of the MI attribute.
  /// Middle initial
  /// </summary>
  [JsonPropertyName("mi")]
  [Member(Index = 13, Type = MemberType.Char, Length = Mi_MaxLength, Optional
    = true)]
  public string Mi
  {
    get => mi;
    set => mi = value != null ? TrimEnd(Substring(value, 1, Mi_MaxLength)) : null
      ;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// Social Security Number of the participant.
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 14, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => ssn;
    set => ssn = value != null ? TrimEnd(Substring(value, 1, Ssn_MaxLength)) : null
      ;
  }

  /// <summary>Length of the PERSON_NUMBER attribute.</summary>
  public const int PersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PERSON_NUMBER attribute.
  /// Person number in CSE
  /// </summary>
  [JsonPropertyName("personNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length = PersonNumber_MaxLength, Optional
    = true)]
  public string PersonNumber
  {
    get => personNumber;
    set => personNumber = value != null
      ? TrimEnd(Substring(value, 1, PersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the GOOD_CAUSE_STATUS attribute.</summary>
  public const int GoodCauseStatus_MaxLength = 1;

  /// <summary>
  /// The value of the GOOD_CAUSE_STATUS attribute.
  /// A good cause case with KS Cares. May be left blank.
  /// P - Pending
  /// D - Denied
  /// C - Confirmed
  /// </summary>
  [JsonPropertyName("goodCauseStatus")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = GoodCauseStatus_MaxLength, Optional = true)]
  public string GoodCauseStatus
  {
    get => goodCauseStatus;
    set => goodCauseStatus = value != null
      ? TrimEnd(Substring(value, 1, GoodCauseStatus_MaxLength)) : null;
  }

  /// <summary>Length of the INSUR_IND attribute.</summary>
  public const int InsurInd_MaxLength = 1;

  /// <summary>
  /// The value of the INSUR_IND attribute.
  /// Indicator to show if medical insurance ordered:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("insurInd")]
  [Member(Index = 17, Type = MemberType.Char, Length = InsurInd_MaxLength, Optional
    = true)]
  public string InsurInd
  {
    get => insurInd;
    set => insurInd = value != null
      ? TrimEnd(Substring(value, 1, InsurInd_MaxLength)) : null;
  }

  /// <summary>Length of the PAT_EST_IND attribute.</summary>
  public const int PatEstInd_MaxLength = 1;

  /// <summary>
  /// The value of the PAT_EST_IND attribute.
  /// Indicator to show if paternity established for this child:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("patEstInd")]
  [Member(Index = 18, Type = MemberType.Char, Length = PatEstInd_MaxLength, Optional
    = true)]
  public string PatEstInd
  {
    get => patEstInd;
    set => patEstInd = value != null
      ? TrimEnd(Substring(value, 1, PatEstInd_MaxLength)) : null;
  }

  /// <summary>Length of the BENE_IND attribute.</summary>
  public const int BeneInd_MaxLength = 1;

  /// <summary>
  /// The value of the BENE_IND attribute.
  /// Indicator to show if receiving benefits
  /// Y - Yes	
  /// N - No
  /// </summary>
  [JsonPropertyName("beneInd")]
  [Member(Index = 19, Type = MemberType.Char, Length = BeneInd_MaxLength, Optional
    = true)]
  public string BeneInd
  {
    get => beneInd;
    set => beneInd = value != null
      ? TrimEnd(Substring(value, 1, BeneInd_MaxLength)) : null;
  }

  /// <summary>Length of the ROLE attribute.</summary>
  public const int Role_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE attribute.
  /// indicated potential case role on future cse case.
  /// AR - AE's PI
  /// AP - Absent parent
  /// CH - Child
  /// CP - Custodial parent	
  /// </summary>
  [JsonPropertyName("role")]
  [Member(Index = 20, Type = MemberType.Char, Length = Role_MaxLength, Optional
    = true)]
  public string Role
  {
    get => role;
    set => role = value != null
      ? TrimEnd(Substring(value, 1, Role_MaxLength)) : null;
  }

  private string interfaceIdentifier;
  private int identifier;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string absenceCode;
  private string relationship;
  private string sex;
  private DateTime? dob;
  private string lastName;
  private string firstName;
  private string mi;
  private string ssn;
  private string personNumber;
  private string goodCauseStatus;
  private string insurInd;
  private string patEstInd;
  private string beneInd;
  private string role;
}

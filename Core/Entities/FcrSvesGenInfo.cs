// The source file: FCR_SVES_GEN_INFO, ID: 945065547, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This Entity Type stores SVES response information that is common to all of 
/// the SVES claim types - Title II Pending (E04), Title II (E05), Title XVI (
/// E06), and Prison (E07), and the Not Found (E10).
/// </summary>
[Serializable]
public partial class FcrSvesGenInfo: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesGenInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesGenInfo(FcrSvesGenInfo that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesGenInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the MEMBER_ID attribute.</summary>
  public const int MemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MEMBER_ID attribute.
  /// This field contains the Member ID that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = MemberId_MaxLength)]
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

  /// <summary>Length of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// </summary>
  public const int LocateSourceResponseAgencyCo_MaxLength = 3;

  /// <summary>
  /// The value of the LOCATE_SOURCE_RESPONSE_AGENCY_CO attribute.
  /// This field contains the code that identifies the Locate Source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = LocateSourceResponseAgencyCo_MaxLength)]
  public string LocateSourceResponseAgencyCo
  {
    get => Get<string>("locateSourceResponseAgencyCo") ?? "";
    set => Set(
      "locateSourceResponseAgencyCo", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LocateSourceResponseAgencyCo_MaxLength)));
  }

  /// <summary>
  /// The json value of the LocateSourceResponseAgencyCo attribute.</summary>
  [JsonPropertyName("locateSourceResponseAgencyCo")]
  [Computed]
  public string LocateSourceResponseAgencyCo_Json
  {
    get => NullIf(LocateSourceResponseAgencyCo, "");
    set => LocateSourceResponseAgencyCo = value;
  }

  /// <summary>Length of the SVES_MATCH_TYPE attribute.</summary>
  public const int SvesMatchType_MaxLength = 1;

  /// <summary>
  /// The value of the SVES_MATCH_TYPE attribute.
  /// This field must contain one of the following values to indicate the action
  /// that initiated the generation of this record:
  ///  L  SVES Locate Request Record
  ///  P  SVES Proactive Match Request Record
  /// </summary>
  [JsonPropertyName("svesMatchType")]
  [Member(Index = 3, Type = MemberType.Char, Length = SvesMatchType_MaxLength, Optional
    = true)]
  public string SvesMatchType
  {
    get => Get<string>("svesMatchType");
    set => Set(
      "svesMatchType", TrimEnd(Substring(value, 1, SvesMatchType_MaxLength)));
  }

  /// <summary>Length of the TRANSMITTER_STATE_TERRITORY_CODE attribute.
  /// </summary>
  public const int TransmitterStateTerritoryCode_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSMITTER_STATE_TERRITORY_CODE attribute.
  /// This field contains the two-digit numeric FIPS Code of the state or 
  /// territory that transmitted the Locate Request to the FCR System.
  /// </summary>
  [JsonPropertyName("transmitterStateTerritoryCode")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = TransmitterStateTerritoryCode_MaxLength, Optional = true)]
  public string TransmitterStateTerritoryCode
  {
    get => Get<string>("transmitterStateTerritoryCode");
    set => Set(
      "transmitterStateTerritoryCode", TrimEnd(Substring(value, 1,
      TransmitterStateTerritoryCode_MaxLength)));
  }

  /// <summary>Length of the RETURNED_FIRST_NAME attribute.</summary>
  public const int ReturnedFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the RETURNED_FIRST_NAME attribute.
  /// This field contains the first name of the person as returned by SVES.
  /// </summary>
  [JsonPropertyName("returnedFirstName")]
  [Member(Index = 5, Type = MemberType.Varchar, Length
    = ReturnedFirstName_MaxLength, Optional = true)]
  public string ReturnedFirstName
  {
    get => Get<string>("returnedFirstName");
    set => Set(
      "returnedFirstName", Substring(value, 1, ReturnedFirstName_MaxLength));
  }

  /// <summary>Length of the RETURNED_MIDDLE_NAME attribute.</summary>
  public const int ReturnedMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the RETURNED_MIDDLE_NAME attribute.
  /// This field contains the middle initial of the person as returned by SVES.
  /// </summary>
  [JsonPropertyName("returnedMiddleName")]
  [Member(Index = 6, Type = MemberType.Varchar, Length
    = ReturnedMiddleName_MaxLength, Optional = true)]
  public string ReturnedMiddleName
  {
    get => Get<string>("returnedMiddleName");
    set => Set(
      "returnedMiddleName", Substring(value, 1, ReturnedMiddleName_MaxLength));
  }

  /// <summary>Length of the RETURNED_LAST_NAME attribute.</summary>
  public const int ReturnedLastName_MaxLength = 30;

  /// <summary>
  /// The value of the RETURNED_LAST_NAME attribute.
  /// This field contains the last name of the person as returned by SVES.
  /// </summary>
  [JsonPropertyName("returnedLastName")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = ReturnedLastName_MaxLength, Optional = true)]
  public string ReturnedLastName
  {
    get => Get<string>("returnedLastName");
    set => Set(
      "returnedLastName", Substring(value, 1, ReturnedLastName_MaxLength));
  }

  /// <summary>Length of the SEX_CODE attribute.</summary>
  public const int SexCode_MaxLength = 1;

  /// <summary>
  /// The value of the SEX_CODE attribute.
  /// This field contains one of the following values:
  /// F   Female
  /// M  Male
  /// U  - Unknown
  /// </summary>
  [JsonPropertyName("sexCode")]
  [Member(Index = 8, Type = MemberType.Char, Length = SexCode_MaxLength, Optional
    = true)]
  public string SexCode
  {
    get => Get<string>("sexCode");
    set => Set("sexCode", TrimEnd(Substring(value, 1, SexCode_MaxLength)));
  }

  /// <summary>
  /// The value of the RETURNED_DATE_OF_BIRTH attribute.
  /// This field contains the date of birth of the person that was returned from
  /// SVES.
  /// The date is in CCYYMMDD format.
  /// If this field is found without a properly formatted valid date, it will 
  /// then contain spaces.
  /// </summary>
  [JsonPropertyName("returnedDateOfBirth")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnedDateOfBirth
  {
    get => Get<DateTime?>("returnedDateOfBirth");
    set => Set("returnedDateOfBirth", value);
  }

  /// <summary>
  /// The value of the RETURNED_DATE_OF_DEATH attribute.
  /// This field contains the date of death of the person that was returned from
  /// SVES.
  /// The date is in CCYYMMDD format.
  /// If this field is found without a properly formatted valid date, it will 
  /// then contain spaces.
  /// If SSAs records contained 00 in the day portion of the date, the FCR 
  /// System returns 01 in the day portion of the Title II Date of Death
  /// field.
  /// </summary>
  [JsonPropertyName("returnedDateOfDeath")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnedDateOfDeath
  {
    get => Get<DateTime?>("returnedDateOfDeath");
    set => Set("returnedDateOfDeath", value);
  }

  /// <summary>Length of the SUBMITTED_FIRST_NAME attribute.</summary>
  public const int SubmittedFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the SUBMITTED_FIRST_NAME attribute.
  /// This field contains the first name that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonPropertyName("submittedFirstName")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = SubmittedFirstName_MaxLength, Optional = true)]
  public string SubmittedFirstName
  {
    get => Get<string>("submittedFirstName");
    set => Set(
      "submittedFirstName", Substring(value, 1, SubmittedFirstName_MaxLength));
  }

  /// <summary>Length of the SUBMITTED_MIDDLE_NAME attribute.</summary>
  public const int SubmittedMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the SUBMITTED_MIDDLE_NAME attribute.
  /// This field contains the middle initial that was provided by the submitter 
  /// of the Locate Request.
  /// </summary>
  [JsonPropertyName("submittedMiddleName")]
  [Member(Index = 12, Type = MemberType.Varchar, Length
    = SubmittedMiddleName_MaxLength, Optional = true)]
  public string SubmittedMiddleName
  {
    get => Get<string>("submittedMiddleName");
    set => Set(
      "submittedMiddleName",
      Substring(value, 1, SubmittedMiddleName_MaxLength));
  }

  /// <summary>Length of the SUBMITTED_LAST_NAME attribute.</summary>
  public const int SubmittedLastName_MaxLength = 30;

  /// <summary>
  /// The value of the SUBMITTED_LAST_NAME attribute.
  /// This field contains the last name that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonPropertyName("submittedLastName")]
  [Member(Index = 13, Type = MemberType.Varchar, Length
    = SubmittedLastName_MaxLength, Optional = true)]
  public string SubmittedLastName
  {
    get => Get<string>("submittedLastName");
    set => Set(
      "submittedLastName", Substring(value, 1, SubmittedLastName_MaxLength));
  }

  /// <summary>
  /// The value of the SUBMITTED_DATE_OF_BIRTH attribute.
  /// This field contains the information that was provided by the submitter of 
  /// the Locate Request.
  /// If the date of birth that was submitted is different from the date of 
  /// birth on SSAs records, this field contains SSAs recorded date of birth
  /// for the person.
  /// </summary>
  [JsonPropertyName("submittedDateOfBirth")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? SubmittedDateOfBirth
  {
    get => Get<DateTime?>("submittedDateOfBirth");
    set => Set("submittedDateOfBirth", value);
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This field contains the Social Security Number that was provided by the 
  /// submitter of the Locate Request.
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 15, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => Get<string>("ssn");
    set => Set("ssn", TrimEnd(Substring(value, 1, Ssn_MaxLength)));
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 15;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// This field contains the User Field that was provided by the submitter of 
  /// the Locate Request.
  /// </summary>
  [JsonPropertyName("userField")]
  [Member(Index = 16, Type = MemberType.Varchar, Length = UserField_MaxLength, Optional
    = true)]
  public string UserField
  {
    get => Get<string>("userField");
    set => Set("userField", Substring(value, 1, UserField_MaxLength));
  }

  /// <summary>Length of the LOCATE_CLOSED_INDICATOR attribute.</summary>
  public const int LocateClosedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the LOCATE_CLOSED_INDICATOR attribute.
  /// If this is the last Locate Response returned for the Locate Request, this 
  /// field contains a C.
  ///  If it is not the last Locate Response for this person for this requestor,
  /// this field contains a space.
  /// </summary>
  [JsonPropertyName("locateClosedIndicator")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = LocateClosedIndicator_MaxLength, Optional = true)]
  public string LocateClosedIndicator
  {
    get => Get<string>("locateClosedIndicator");
    set => Set(
      "locateClosedIndicator", TrimEnd(Substring(value, 1,
      LocateClosedIndicator_MaxLength)));
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// This field contains the FIPS County Code that was provided by the 
  /// submitter of the Locate Request.
  /// </summary>
  [JsonPropertyName("fipsCountyCode")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = FipsCountyCode_MaxLength, Optional = true)]
  public string FipsCountyCode
  {
    get => Get<string>("fipsCountyCode");
    set => Set(
      "fipsCountyCode", TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength)));
      
  }

  /// <summary>Length of the LOCATE_REQUEST_TYPE attribute.</summary>
  public const int LocateRequestType_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_REQUEST_TYPE attribute.
  /// This field contains the Locate Request Type that was provided by the 
  /// submitter of the Locate Request.
  /// </summary>
  [JsonPropertyName("locateRequestType")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = LocateRequestType_MaxLength, Optional = true)]
  public string LocateRequestType
  {
    get => Get<string>("locateRequestType");
    set => Set(
      "locateRequestType", TrimEnd(Substring(value, 1,
      LocateRequestType_MaxLength)));
  }

  /// <summary>Length of the LOCATE_RESPONSE_CODE attribute.</summary>
  public const int LocateResponseCode_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_RESPONSE_CODE attribute.
  /// This field contains a value to clarify the response that was returned from
  /// SVES:
  /// 09  SVES Database is inaccessible for the person.
  /// Spaces  Locate information returned to State.
  /// </summary>
  [JsonPropertyName("locateResponseCode")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = LocateResponseCode_MaxLength, Optional = true)]
  public string LocateResponseCode
  {
    get => Get<string>("locateResponseCode");
    set => Set(
      "locateResponseCode", TrimEnd(Substring(value, 1,
      LocateResponseCode_MaxLength)));
  }

  /// <summary>Length of the MULTIPLE_SSN_INDICATOR attribute.</summary>
  public const int MultipleSsnIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MULTIPLE_SSN_INDICATOR attribute.
  /// This field contains a value to indicate if the SSN that was used in the 
  /// SVES match is a multiple SSN:
  /// M  Additional/multiple SSN
  /// X  Multiple SSN from Corrected SSN
  /// Spaces  Original SSN
  ///  If this field is an M or X, the SSN that was used in the match is in 
  /// the Multiple SSN field.
  /// </summary>
  [JsonPropertyName("multipleSsnIndicator")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = MultipleSsnIndicator_MaxLength, Optional = true)]
  public string MultipleSsnIndicator
  {
    get => Get<string>("multipleSsnIndicator");
    set => Set(
      "multipleSsnIndicator", TrimEnd(Substring(value, 1,
      MultipleSsnIndicator_MaxLength)));
  }

  /// <summary>Length of the MULTIPLE_SSN attribute.</summary>
  public const int MultipleSsn_MaxLength = 9;

  /// <summary>
  /// The value of the MULTIPLE_SSN attribute.
  /// If the SSA SSN verification routines identified one or more multiple valid
  /// SSNs for the person, an additional valid SSN is in this field.
  ///  If the Multiple SSN Indicator is an M or X, this field contains the 
  /// Multiple SSN that was  used in the match. The SSN in this field is
  /// different from the SSN in the SSN field.
  /// </summary>
  [JsonPropertyName("multipleSsn")]
  [Member(Index = 22, Type = MemberType.Char, Length = MultipleSsn_MaxLength, Optional
    = true)]
  public string MultipleSsn
  {
    get => Get<string>("multipleSsn");
    set => Set(
      "multipleSsn", TrimEnd(Substring(value, 1, MultipleSsn_MaxLength)));
  }

  /// <summary>Length of the PARTICIPANT_TYPE attribute.</summary>
  public const int ParticipantType_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPANT_TYPE attribute.
  /// This field contains a value to define the persons Participant Type on the
  /// SVES Proactive Match Request:
  /// CP	 Custodial Party
  /// NP	 Non-custodial Parent
  /// PF	 Putative Father
  /// This field contains spaces on a SVES Locate Request.
  /// </summary>
  [JsonPropertyName("participantType")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ParticipantType_MaxLength, Optional = true)]
  public string ParticipantType
  {
    get => Get<string>("participantType");
    set => Set(
      "participantType",
      TrimEnd(Substring(value, 1, ParticipantType_MaxLength)));
  }

  /// <summary>Length of the FAMILY_VIOLENCE_STATE_1 attribute.</summary>
  public const int FamilyViolenceState1_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE_STATE_1 attribute.
  /// This field contains the first State that entered FV in the child support 
  /// case
  /// </summary>
  [JsonPropertyName("familyViolenceState1")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = FamilyViolenceState1_MaxLength, Optional = true)]
  public string FamilyViolenceState1
  {
    get => Get<string>("familyViolenceState1");
    set => Set(
      "familyViolenceState1", TrimEnd(Substring(value, 1,
      FamilyViolenceState1_MaxLength)));
  }

  /// <summary>Length of the FAMILY_VIOLENCE_STATE_2 attribute.</summary>
  public const int FamilyViolenceState2_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE_STATE_2 attribute.
  /// This field contains the second State that entered FV in the child support 
  /// case
  /// </summary>
  [JsonPropertyName("familyViolenceState2")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = FamilyViolenceState2_MaxLength, Optional = true)]
  public string FamilyViolenceState2
  {
    get => Get<string>("familyViolenceState2");
    set => Set(
      "familyViolenceState2", TrimEnd(Substring(value, 1,
      FamilyViolenceState2_MaxLength)));
  }

  /// <summary>Length of the FAMILY_VIOLENCE_STATE_3 attribute.</summary>
  public const int FamilyViolenceState3_MaxLength = 2;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE_STATE_3 attribute.
  /// This field contains the third State that entered FV in the child support 
  /// case
  /// </summary>
  [JsonPropertyName("familyViolenceState3")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = FamilyViolenceState3_MaxLength, Optional = true)]
  public string FamilyViolenceState3
  {
    get => Get<string>("familyViolenceState3");
    set => Set(
      "familyViolenceState3", TrimEnd(Substring(value, 1,
      FamilyViolenceState3_MaxLength)));
  }

  /// <summary>Length of the SORT_STATE_CODE attribute.</summary>
  public const int SortStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the SORT_STATE_CODE attribute.
  /// This field contains the two-digit numeric FIPS State code of the State 
  /// that will receive the response.
  /// </summary>
  [JsonPropertyName("sortStateCode")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = SortStateCode_MaxLength, Optional = true)]
  public string SortStateCode
  {
    get => Get<string>("sortStateCode");
    set => Set(
      "sortStateCode", TrimEnd(Substring(value, 1, SortStateCode_MaxLength)));
  }

  /// <summary>
  /// The value of the REQUEST_DATE attribute.
  /// Contains the date of the latest FPLS request.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 28, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestDate
  {
    get => Get<DateTime?>("requestDate");
    set => Set("requestDate", value);
  }

  /// <summary>
  /// The value of the RESPONSE_RECEIVED_DATE attribute.
  /// </summary>
  [JsonPropertyName("responseReceivedDate")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
  public DateTime? ResponseReceivedDate
  {
    get => Get<DateTime?>("responseReceivedDate");
    set => Set("responseReceivedDate", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 31, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 33, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }
}

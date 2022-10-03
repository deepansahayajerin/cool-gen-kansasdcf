// The source file: FPLS_LOCATE_REQUEST, ID: 371434492, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains the details of the locate request sent to FPLS system.
/// </summary>
[Serializable]
public partial class FplsLocateRequest: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FplsLocateRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FplsLocateRequest(FplsLocateRequest that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FplsLocateRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute, which together with the relationship with CSE_PERSON, 
  /// identifies uniquely an FPLS transaction.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the TRANSACTION_STATUS attribute.</summary>
  public const int TransactionStatus_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSACTION_STATUS attribute.
  /// Describes the status of the FPLS transaction.
  /// The valid values are maintained in CODE_VALUE entity for the CODE_NAME 
  /// FPLS_TRANS_TYPE.
  /// C - Request created
  /// S - Request sent
  /// R - One or more responses received.
  /// E - FPLS_Locate_Request Expired.
  /// etc.
  /// </summary>
  [JsonPropertyName("transactionStatus")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = TransactionStatus_MaxLength, Optional = true)]
  public string TransactionStatus
  {
    get => Get<string>("transactionStatus");
    set => Set(
      "transactionStatus", TrimEnd(Substring(value, 1,
      TransactionStatus_MaxLength)));
  }

  /// <summary>
  /// The value of the ZDEL_REQ_CREAT_DT attribute.
  /// Date the request was created.
  /// </summary>
  [JsonPropertyName("zdelReqCreatDt")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ZdelReqCreatDt
  {
    get => Get<DateTime?>("zdelReqCreatDt");
    set => Set("zdelReqCreatDt", value);
  }

  /// <summary>Length of the ZDEL_CREAT_USER_ID attribute.</summary>
  public const int ZdelCreatUserId_MaxLength = 8;

  /// <summary>
  /// The value of the ZDEL_CREAT_USER_ID attribute.
  /// ID of the user created the request.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ZdelCreatUserId_MaxLength)]
    
  public string ZdelCreatUserId
  {
    get => Get<string>("zdelCreatUserId") ?? "";
    set => Set(
      "zdelCreatUserId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ZdelCreatUserId_MaxLength)));
  }

  /// <summary>
  /// The json value of the ZdelCreatUserId attribute.</summary>
  [JsonPropertyName("zdelCreatUserId")]
  [Computed]
  public string ZdelCreatUserId_Json
  {
    get => NullIf(ZdelCreatUserId, "");
    set => ZdelCreatUserId = value;
  }

  /// <summary>
  /// The value of the REQUEST_SENT_DATE attribute.
  /// This attribute defines the Date on which the request was actually sent to 
  /// FPLS.
  /// This attribute is updated by the non-ief batch procedure which creates the
  /// FPLS tape from FPLS_LOCATE_REQUEST entity.
  /// </summary>
  [JsonPropertyName("requestSentDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestSentDate
  {
    get => Get<DateTime?>("requestSentDate");
    set => Set("requestSentDate", value);
  }

  /// <summary>Length of the STATE_ABBR attribute.</summary>
  public const int StateAbbr_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_ABBR attribute.
  /// The 2-char alphabetic State Code of the sending state. Required for known 
  /// and unknown cases. Refer to FPLS I-O specification.
  /// </summary>
  [JsonPropertyName("stateAbbr")]
  [Member(Index = 6, Type = MemberType.Char, Length = StateAbbr_MaxLength, Optional
    = true)]
  public string StateAbbr
  {
    get => Get<string>("stateAbbr");
    set => Set("stateAbbr", TrimEnd(Substring(value, 1, StateAbbr_MaxLength)));
  }

  /// <summary>Length of the STATION_NUMBER attribute.</summary>
  public const int StationNumber_MaxLength = 2;

  /// <summary>
  /// The value of the STATION_NUMBER attribute.
  /// Station No of the sending unit. Assigned by FPLS. Required for known and 
  /// unknown cases.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonPropertyName("stationNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length = StationNumber_MaxLength, Optional
    = true)]
  public string StationNumber
  {
    get => Get<string>("stationNumber");
    set => Set(
      "stationNumber", TrimEnd(Substring(value, 1, StationNumber_MaxLength)));
  }

  /// <summary>Length of the TRANSACTION_TYPE attribute.</summary>
  public const int TransactionType_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSACTION_TYPE attribute.
  /// Describes the transaction type.
  /// A - Add a new record (default)
  /// M - Modify an existing record.
  /// D - Delete an existing record.
  /// Only &quot;A&quot; is used. Types &quot;M&quot; and &quot;D&quot; are not 
  /// used by the state.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonPropertyName("transactionType")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = TransactionType_MaxLength, Optional = true)]
  public string TransactionType
  {
    get => Get<string>("transactionType");
    set => Set(
      "transactionType",
      TrimEnd(Substring(value, 1, TransactionType_MaxLength)));
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// Gives the Socia Security No of the Absent Parent.
  /// The first three digits must be in one of the following ranges : 001-649, 
  /// 700-728.
  /// If there is no SSN, initialise to space.
  /// Required for known cases.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonPropertyName("ssn")]
  [Member(Index = 9, Type = MemberType.Char, Length = Ssn_MaxLength, Optional
    = true)]
  public string Ssn
  {
    get => Get<string>("ssn");
    set => Set("ssn", TrimEnd(Substring(value, 1, Ssn_MaxLength)));
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// Contains Case number.
  /// If SSN not available, Case ID must be present. No special characters 
  /// except hyphen allowed.
  /// Required for unknown cases.
  /// </summary>
  [JsonPropertyName("caseId")]
  [Member(Index = 10, Type = MemberType.Char, Length = CaseId_MaxLength, Optional
    = true)]
  public string CaseId
  {
    get => Get<string>("caseId");
    set => Set("caseId", TrimEnd(Substring(value, 1, CaseId_MaxLength)));
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// Expected to contain 3-digit county code. It is optional field. Must be 
  /// blank or a valid numeric value.
  /// </summary>
  [JsonPropertyName("localCode")]
  [Member(Index = 11, Type = MemberType.Char, Length = LocalCode_MaxLength, Optional
    = true)]
  public string LocalCode
  {
    get => Get<string>("localCode");
    set => Set("localCode", TrimEnd(Substring(value, 1, LocalCode_MaxLength)));
  }

  /// <summary>Length of the USERS_FIELD attribute.</summary>
  public const int UsersField_MaxLength = 7;

  /// <summary>
  /// The value of the USERS_FIELD attribute.
  /// User's field. Currently Kansas moves 2-digit region code + 1 char Team 
  /// code to this attribute.
  /// </summary>
  [JsonPropertyName("usersField")]
  [Member(Index = 12, Type = MemberType.Char, Length = UsersField_MaxLength, Optional
    = true)]
  public string UsersField
  {
    get => Get<string>("usersField");
    set =>
      Set("usersField", TrimEnd(Substring(value, 1, UsersField_MaxLength)));
  }

  /// <summary>Length of the TYPE_OF_CASE attribute.</summary>
  public const int TypeOfCase_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE_OF_CASE attribute.
  /// Described type of case.
  /// Required for known and unknown cases.
  /// A - AFDC/Foster Care
  /// N - Non AFDC full service
  /// L - Non AFDC Locate only
  /// P - Parental kidnapping
  /// </summary>
  [JsonPropertyName("typeOfCase")]
  [Member(Index = 13, Type = MemberType.Char, Length = TypeOfCase_MaxLength, Optional
    = true)]
  public string TypeOfCase
  {
    get => Get<string>("typeOfCase");
    set =>
      Set("typeOfCase", TrimEnd(Substring(value, 1, TypeOfCase_MaxLength)));
  }

  /// <summary>Length of the AP_FIRST_NAME attribute.</summary>
  public const int ApFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_FIRST_NAME attribute.
  /// First Name of the Absent Parent. No special characters allowed.
  /// Required for known and unknown cases.
  /// </summary>
  [JsonPropertyName("apFirstName")]
  [Member(Index = 14, Type = MemberType.Char, Length = ApFirstName_MaxLength, Optional
    = true)]
  public string ApFirstName
  {
    get => Get<string>("apFirstName");
    set => Set(
      "apFirstName", TrimEnd(Substring(value, 1, ApFirstName_MaxLength)));
  }

  /// <summary>Length of the AP_MIDDLE_NAME attribute.</summary>
  public const int ApMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_MIDDLE_NAME attribute.
  /// Middle Name of the Absent Parent.
  /// Optional field. No special characters allowed.
  /// </summary>
  [JsonPropertyName("apMiddleName")]
  [Member(Index = 15, Type = MemberType.Char, Length = ApMiddleName_MaxLength, Optional
    = true)]
  public string ApMiddleName
  {
    get => Get<string>("apMiddleName");
    set => Set(
      "apMiddleName", TrimEnd(Substring(value, 1, ApMiddleName_MaxLength)));
  }

  /// <summary>Length of the AP_FIRST_LAST_NAME attribute.</summary>
  public const int ApFirstLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_FIRST_LAST_NAME attribute.
  /// Last name of the absent parent.
  /// No special characters except hyphen. Required for known and  unknown 
  /// cases.
  /// </summary>
  [JsonPropertyName("apFirstLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ApFirstLastName_MaxLength, Optional = true)]
  public string ApFirstLastName
  {
    get => Get<string>("apFirstLastName");
    set => Set(
      "apFirstLastName",
      TrimEnd(Substring(value, 1, ApFirstLastName_MaxLength)));
  }

  /// <summary>Length of the AP_SECOND_LAST_NAME attribute.</summary>
  public const int ApSecondLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_SECOND_LAST_NAME attribute.
  /// Alias - 1 of the absent parent
  /// No special characters allowed except hyphen. Optional field.
  /// </summary>
  [JsonPropertyName("apSecondLastName")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ApSecondLastName_MaxLength, Optional = true)]
  public string ApSecondLastName
  {
    get => Get<string>("apSecondLastName");
    set => Set(
      "apSecondLastName",
      TrimEnd(Substring(value, 1, ApSecondLastName_MaxLength)));
  }

  /// <summary>Length of the AP_THIRD_LAST_NAME attribute.</summary>
  public const int ApThirdLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_THIRD_LAST_NAME attribute.
  /// Alias - 2 of the absent parent.
  /// No special character allowed except hyphen. Optional field.
  /// </summary>
  [JsonPropertyName("apThirdLastName")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ApThirdLastName_MaxLength, Optional = true)]
  public string ApThirdLastName
  {
    get => Get<string>("apThirdLastName");
    set => Set(
      "apThirdLastName",
      TrimEnd(Substring(value, 1, ApThirdLastName_MaxLength)));
  }

  /// <summary>
  /// The value of the AP_DATE_OF_BIRTH attribute.
  /// Date of birth of the absent parent.
  /// Required for unknown cases. Default is zeros.
  /// Age must be between 14 and 99 years.
  /// Output flat file will be moved with MMDDYY (6 digit) format.
  /// </summary>
  [JsonPropertyName("apDateOfBirth")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? ApDateOfBirth
  {
    get => Get<DateTime?>("apDateOfBirth");
    set => Set("apDateOfBirth", value);
  }

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// Sex of the absent parent
  /// Mandatory input.
  /// M - Male
  /// F - Female.
  /// FPLS assumes a default of &quot;M&quot;.
  /// </summary>
  [JsonPropertyName("sex")]
  [Member(Index = 20, Type = MemberType.Char, Length = Sex_MaxLength, Optional
    = true)]
  public string Sex
  {
    get => Get<string>("sex");
    set => Set("sex", TrimEnd(Substring(value, 1, Sex_MaxLength)));
  }

  /// <summary>Length of the COLLECT_ALL_RESPONSES_TOGETHER attribute.</summary>
  public const int CollectAllResponsesTogether_MaxLength = 1;

  /// <summary>
  /// The value of the COLLECT_ALL_RESPONSES_TOGETHER attribute.
  /// Valid values are:
  ///   Y - FPLS will collect all KNOWN responses for this case and return them 
  /// together when all responses are returned by federal agencies, except SESA
  /// responses.
  ///   N - FPLS will send the responses as and when they received from the 
  /// federal agencies.
  /// Optional input. Default is &quot;N&quot;
  /// </summary>
  [JsonPropertyName("collectAllResponsesTogether")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = CollectAllResponsesTogether_MaxLength, Optional = true)]
  public string CollectAllResponsesTogether
  {
    get => Get<string>("collectAllResponsesTogether");
    set => Set(
      "collectAllResponsesTogether", TrimEnd(Substring(value, 1,
      CollectAllResponsesTogether_MaxLength)));
  }

  /// <summary>Length of the TRANSACTION_ERROR attribute.</summary>
  public const int TransactionError_MaxLength = 10;

  /// <summary>
  /// The value of the TRANSACTION_ERROR attribute.
  /// Contains 2-digit error codes concatenated. These error codes are returned 
  /// by FPLS system.
  /// Refer to FPLS Transaction error codes described in FPLS manual.
  /// </summary>
  [JsonPropertyName("transactionError")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = TransactionError_MaxLength, Optional = true)]
  public string TransactionError
  {
    get => Get<string>("transactionError");
    set => Set(
      "transactionError",
      TrimEnd(Substring(value, 1, TransactionError_MaxLength)));
  }

  /// <summary>Length of the AP_CITY_OF_BIRTH attribute.</summary>
  public const int ApCityOfBirth_MaxLength = 16;

  /// <summary>
  /// The value of the AP_CITY_OF_BIRTH attribute.
  /// City of birth of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apCityOfBirth")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ApCityOfBirth_MaxLength, Optional = true)]
  public string ApCityOfBirth
  {
    get => Get<string>("apCityOfBirth");
    set => Set(
      "apCityOfBirth", TrimEnd(Substring(value, 1, ApCityOfBirth_MaxLength)));
  }

  /// <summary>Length of the AP_STATE_OR_COUNTRY_OF_BIRTH attribute.</summary>
  public const int ApStateOrCountryOfBirth_MaxLength = 3;

  /// <summary>
  /// The value of the AP_STATE_OR_COUNTRY_OF_BIRTH attribute.
  /// State or Country of birth of the absent parent. Refer to FPLS state/ 
  /// country codes list.
  /// The valid values are maintained in CODE_VALUE entity for the CODE_NAME 
  /// FPLS_STATE_COUNTRY.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apStateOrCountryOfBirth")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ApStateOrCountryOfBirth_MaxLength, Optional = true)]
  public string ApStateOrCountryOfBirth
  {
    get => Get<string>("apStateOrCountryOfBirth");
    set => Set(
      "apStateOrCountryOfBirth", TrimEnd(Substring(value, 1,
      ApStateOrCountryOfBirth_MaxLength)));
  }

  /// <summary>Length of the APS_FATHERS_FIRST_NAME attribute.</summary>
  public const int ApsFathersFirstName_MaxLength = 13;

  /// <summary>
  /// The value of the APS_FATHERS_FIRST_NAME attribute.
  /// Father's First Name of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsFathersFirstName")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = ApsFathersFirstName_MaxLength, Optional = true)]
  public string ApsFathersFirstName
  {
    get => Get<string>("apsFathersFirstName");
    set => Set(
      "apsFathersFirstName", TrimEnd(Substring(value, 1,
      ApsFathersFirstName_MaxLength)));
  }

  /// <summary>Length of the APS_FATHERS_MI attribute.</summary>
  public const int ApsFathersMi_MaxLength = 1;

  /// <summary>
  /// The value of the APS_FATHERS_MI attribute.
  /// Father's Middle Initial of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsFathersMi")]
  [Member(Index = 26, Type = MemberType.Char, Length = ApsFathersMi_MaxLength, Optional
    = true)]
  public string ApsFathersMi
  {
    get => Get<string>("apsFathersMi");
    set => Set(
      "apsFathersMi", TrimEnd(Substring(value, 1, ApsFathersMi_MaxLength)));
  }

  /// <summary>Length of the APS_FATHERS_LAST_NAME attribute.</summary>
  public const int ApsFathersLastName_MaxLength = 16;

  /// <summary>
  /// The value of the APS_FATHERS_LAST_NAME attribute.
  /// Father's Last Name of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsFathersLastName")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = ApsFathersLastName_MaxLength, Optional = true)]
  public string ApsFathersLastName
  {
    get => Get<string>("apsFathersLastName");
    set => Set(
      "apsFathersLastName", TrimEnd(Substring(value, 1,
      ApsFathersLastName_MaxLength)));
  }

  /// <summary>Length of the APS_MOTHERS_FIRST_NAME attribute.</summary>
  public const int ApsMothersFirstName_MaxLength = 13;

  /// <summary>
  /// The value of the APS_MOTHERS_FIRST_NAME attribute.
  /// Mother's First Name of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsMothersFirstName")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = ApsMothersFirstName_MaxLength, Optional = true)]
  public string ApsMothersFirstName
  {
    get => Get<string>("apsMothersFirstName");
    set => Set(
      "apsMothersFirstName", TrimEnd(Substring(value, 1,
      ApsMothersFirstName_MaxLength)));
  }

  /// <summary>Length of the APS_MOTHERS_MI attribute.</summary>
  public const int ApsMothersMi_MaxLength = 1;

  /// <summary>
  /// The value of the APS_MOTHERS_MI attribute.
  /// Mother's Middle Initial of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsMothersMi")]
  [Member(Index = 29, Type = MemberType.Char, Length = ApsMothersMi_MaxLength, Optional
    = true)]
  public string ApsMothersMi
  {
    get => Get<string>("apsMothersMi");
    set => Set(
      "apsMothersMi", TrimEnd(Substring(value, 1, ApsMothersMi_MaxLength)));
  }

  /// <summary>Length of the APS_MOTHERS_MAIDEN_NAME attribute.</summary>
  public const int ApsMothersMaidenName_MaxLength = 16;

  /// <summary>
  /// The value of the APS_MOTHERS_MAIDEN_NAME attribute.
  /// Mother's Maiden Name of the absent parent. No special chars allowed.
  /// Atleast three of the following fields are required for unknown cases going
  /// to SSA. However for unknown cases going to IRS, only the custodial parent
  /// SSN field is required.
  /// </summary>
  [JsonPropertyName("apsMothersMaidenName")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = ApsMothersMaidenName_MaxLength, Optional = true)]
  public string ApsMothersMaidenName
  {
    get => Get<string>("apsMothersMaidenName");
    set => Set(
      "apsMothersMaidenName", TrimEnd(Substring(value, 1,
      ApsMothersMaidenName_MaxLength)));
  }

  /// <summary>Length of the CP_SSN attribute.</summary>
  public const int CpSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CP_SSN attribute.
  /// Custodial Parent's SSN.
  /// First 3-digits must be one within the following ranges: 001-649 or 700-
  /// 728. This field is required for unknown case to go to IRS for an SSN
  /// search.
  /// </summary>
  [JsonPropertyName("cpSsn")]
  [Member(Index = 31, Type = MemberType.Char, Length = CpSsn_MaxLength, Optional
    = true)]
  public string CpSsn
  {
    get => Get<string>("cpSsn");
    set => Set("cpSsn", TrimEnd(Substring(value, 1, CpSsn_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 33, Type = MemberType.Timestamp)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy") ?? "";
    set => Set(
      "lastUpdatedBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, LastUpdatedBy_MaxLength)));
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
  [Member(Index = 35, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the SEND_REQUEST_TO attribute.</summary>
  public const int SendRequestTo_MaxLength = 45;

  /// <summary>
  /// The value of the SEND_REQUEST_TO attribute.
  /// Contains one or more 3-char agency codes concatenated to identify the 
  /// agencies to which the case is to be sent to.
  /// Required for known and unknown cases.
  /// Valid agency codes are maintained in CODE_VALUE entity for CODE_NAME 
  /// FPLS_AGENCY_CODE.
  /// A01 - Send known cases to DOD
  /// B## - Send known case to SESA state where ##
  ///       is the numeric FIPS state
  /// C01 - Send known case to IRS
  /// C02 - Send Unknown case to IRS for SSN Id
  /// D01 - Send known case to NPRC
  /// E01 - Send known case to SSA for employer and
  ///       benefit information
  /// E02 - Send unknown case to SSA for SSN Id
  /// F01 - Send known case to VA
  /// G01 - Send known case to SSS
  /// </summary>
  [JsonPropertyName("sendRequestTo")]
  [Member(Index = 36, Type = MemberType.Varchar, Length
    = SendRequestTo_MaxLength, Optional = true)]
  public string SendRequestTo
  {
    get => Get<string>("sendRequestTo");
    set => Set("sendRequestTo", Substring(value, 1, SendRequestTo_MaxLength));
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
  [Member(Index = 37, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => Get<string>("cspNumber") ?? "";
    set => Set(
      "cspNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspNumber_MaxLength)));
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
}

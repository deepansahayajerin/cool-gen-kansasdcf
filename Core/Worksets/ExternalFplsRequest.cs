// The source file: EXTERNAL_FPLS_REQUEST, ID: 372362118, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp OBLMGMT:		
/// This work set defines the EXTERNAL format for creating a tape to send to the
/// Federal Person Locator Service (FPLS) Request.
/// Please Note that the actual format calls for CASE_ID in Position 41-55 - We 
/// are using those positions to represent AP Person Number and FPLS_REQUEST
/// Identifier
/// </summary>
[Serializable]
public partial class ExternalFplsRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalFplsRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalFplsRequest(ExternalFplsRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalFplsRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalFplsRequest that)
  {
    base.Assign(that);
    spaces1 = that.spaces1;
    stateAbbr = that.stateAbbr;
    stationNumber = that.stationNumber;
    spaces2 = that.spaces2;
    transactionType = that.transactionType;
    ssn = that.ssn;
    apCsePersonNumber = that.apCsePersonNumber;
    fplsLocateRequestIdentifier = that.fplsLocateRequestIdentifier;
    localCode = that.localCode;
    usersField = that.usersField;
    typeOfCase = that.typeOfCase;
    apFirstName = that.apFirstName;
    apMiddleName = that.apMiddleName;
    apFirstLastName = that.apFirstLastName;
    apSecondLastName = that.apSecondLastName;
    apThirdLastName = that.apThirdLastName;
    apDateOfBirth = that.apDateOfBirth;
    apDateOfBirthMonth = that.apDateOfBirthMonth;
    apDateOfBirthDay = that.apDateOfBirthDay;
    apDateOfBirthYear = that.apDateOfBirthYear;
    sex = that.sex;
    collectAllResponsesTogether = that.collectAllResponsesTogether;
    spaces3 = that.spaces3;
    sendRequestTo = that.sendRequestTo;
    transactionError = that.transactionError;
    spaces4 = that.spaces4;
    apCityOfBirth = that.apCityOfBirth;
    apStateOrCountryOfBirth = that.apStateOrCountryOfBirth;
    apsFathersFirstName = that.apsFathersFirstName;
    apsFathersMi = that.apsFathersMi;
    apsFathersLastName = that.apsFathersLastName;
    apsMothersFirstName = that.apsMothersFirstName;
    apsMothersMi = that.apsMothersMi;
    apsMothersMaidenName = that.apsMothersMaidenName;
    cpSsn = that.cpSsn;
  }

  /// <summary>Length of the SPACES_1 attribute.</summary>
  public const int Spaces1_MaxLength = 16;

  /// <summary>
  /// The value of the SPACES_1 attribute.
  /// Reserved by the FPLS
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Spaces1_MaxLength)]
  public string Spaces1
  {
    get => spaces1 ?? "";
    set => spaces1 = TrimEnd(Substring(value, 1, Spaces1_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces1 attribute.</summary>
  [JsonPropertyName("spaces1")]
  [Computed]
  public string Spaces1_Json
  {
    get => NullIf(Spaces1, "");
    set => Spaces1 = value;
  }

  /// <summary>Length of the STATE_ABBR attribute.</summary>
  public const int StateAbbr_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_ABBR attribute.
  /// The 2-char alphabetic State Code of the sending state. Required for known 
  /// and unknown cases. Refer to FPLS I-O specification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = StateAbbr_MaxLength)]
  public string StateAbbr
  {
    get => stateAbbr ?? "";
    set => stateAbbr = TrimEnd(Substring(value, 1, StateAbbr_MaxLength));
  }

  /// <summary>
  /// The json value of the StateAbbr attribute.</summary>
  [JsonPropertyName("stateAbbr")]
  [Computed]
  public string StateAbbr_Json
  {
    get => NullIf(StateAbbr, "");
    set => StateAbbr = value;
  }

  /// <summary>Length of the STATION_NUMBER attribute.</summary>
  public const int StationNumber_MaxLength = 2;

  /// <summary>
  /// The value of the STATION_NUMBER attribute.
  /// Station No of the sending unit. Assigned by FPLS. Required for known and 
  /// unknown cases.
  /// Refer to FPLS I-O specification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = StationNumber_MaxLength)]
  public string StationNumber
  {
    get => stationNumber ?? "";
    set => stationNumber =
      TrimEnd(Substring(value, 1, StationNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the StationNumber attribute.</summary>
  [JsonPropertyName("stationNumber")]
  [Computed]
  public string StationNumber_Json
  {
    get => NullIf(StationNumber, "");
    set => StationNumber = value;
  }

  /// <summary>Length of the SPACES_2 attribute.</summary>
  public const int Spaces2_MaxLength = 10;

  /// <summary>
  /// The value of the SPACES_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Spaces2_MaxLength)]
  public string Spaces2
  {
    get => spaces2 ?? "";
    set => spaces2 = TrimEnd(Substring(value, 1, Spaces2_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces2 attribute.</summary>
  [JsonPropertyName("spaces2")]
  [Computed]
  public string Spaces2_Json
  {
    get => NullIf(Spaces2, "");
    set => Spaces2 = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TransactionType_MaxLength)]
    
  public string TransactionType
  {
    get => transactionType ?? "";
    set => transactionType =
      TrimEnd(Substring(value, 1, TransactionType_MaxLength));
  }

  /// <summary>
  /// The json value of the TransactionType attribute.</summary>
  [JsonPropertyName("transactionType")]
  [Computed]
  public string TransactionType_Json
  {
    get => NullIf(TransactionType, "");
    set => TransactionType = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
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

  /// <summary>Length of the AP_CSE_PERSON_NUMBER attribute.</summary>
  public const int ApCsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the AP_CSE_PERSON_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ApCsePersonNumber_MaxLength)]
  public string ApCsePersonNumber
  {
    get => apCsePersonNumber ?? "";
    set => apCsePersonNumber =
      TrimEnd(Substring(value, 1, ApCsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ApCsePersonNumber attribute.</summary>
  [JsonPropertyName("apCsePersonNumber")]
  [Computed]
  public string ApCsePersonNumber_Json
  {
    get => NullIf(ApCsePersonNumber, "");
    set => ApCsePersonNumber = value;
  }

  /// <summary>
  /// The value of the FPLS_LOCATE_REQUEST_IDENTIFIER attribute.
  /// The attribute, which together with the relationship with CSE_PERSON, 
  /// identifies uniquely an FPLS transaction.
  /// </summary>
  [JsonPropertyName("fplsLocateRequestIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 5)]
  public int FplsLocateRequestIdentifier
  {
    get => fplsLocateRequestIdentifier;
    set => fplsLocateRequestIdentifier = value;
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// Expected to contain 3-digit county code. It is optional field. Must be 
  /// blank or a valid numeric value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = LocalCode_MaxLength)]
  public string LocalCode
  {
    get => localCode ?? "";
    set => localCode = TrimEnd(Substring(value, 1, LocalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LocalCode attribute.</summary>
  [JsonPropertyName("localCode")]
  [Computed]
  public string LocalCode_Json
  {
    get => NullIf(LocalCode, "");
    set => LocalCode = value;
  }

  /// <summary>Length of the USERS_FIELD attribute.</summary>
  public const int UsersField_MaxLength = 7;

  /// <summary>
  /// The value of the USERS_FIELD attribute.
  /// User's field. Currently Kansas moves 2-digit region code + 1 char Team 
  /// code to this attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = UsersField_MaxLength)]
  public string UsersField
  {
    get => usersField ?? "";
    set => usersField = TrimEnd(Substring(value, 1, UsersField_MaxLength));
  }

  /// <summary>
  /// The json value of the UsersField attribute.</summary>
  [JsonPropertyName("usersField")]
  [Computed]
  public string UsersField_Json
  {
    get => NullIf(UsersField, "");
    set => UsersField = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = TypeOfCase_MaxLength)]
  public string TypeOfCase
  {
    get => typeOfCase ?? "";
    set => typeOfCase = TrimEnd(Substring(value, 1, TypeOfCase_MaxLength));
  }

  /// <summary>
  /// The json value of the TypeOfCase attribute.</summary>
  [JsonPropertyName("typeOfCase")]
  [Computed]
  public string TypeOfCase_Json
  {
    get => NullIf(TypeOfCase, "");
    set => TypeOfCase = value;
  }

  /// <summary>Length of the AP_FIRST_NAME attribute.</summary>
  public const int ApFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_FIRST_NAME attribute.
  /// First Name of the Absent Parent. No special characters allowed.
  /// Required for known and unknown cases.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ApFirstName_MaxLength)]
  public string ApFirstName
  {
    get => apFirstName ?? "";
    set => apFirstName = TrimEnd(Substring(value, 1, ApFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApFirstName attribute.</summary>
  [JsonPropertyName("apFirstName")]
  [Computed]
  public string ApFirstName_Json
  {
    get => NullIf(ApFirstName, "");
    set => ApFirstName = value;
  }

  /// <summary>Length of the AP_MIDDLE_NAME attribute.</summary>
  public const int ApMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the AP_MIDDLE_NAME attribute.
  /// Middle Name of the Absent Parent.
  /// Optional field. No special characters allowed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = ApMiddleName_MaxLength)]
  public string ApMiddleName
  {
    get => apMiddleName ?? "";
    set => apMiddleName = TrimEnd(Substring(value, 1, ApMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApMiddleName attribute.</summary>
  [JsonPropertyName("apMiddleName")]
  [Computed]
  public string ApMiddleName_Json
  {
    get => NullIf(ApMiddleName, "");
    set => ApMiddleName = value;
  }

  /// <summary>Length of the AP_FIRST_LAST_NAME attribute.</summary>
  public const int ApFirstLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_FIRST_LAST_NAME attribute.
  /// Last name of the absent parent.
  /// No special characters except hyphen. Required for known and  unknown 
  /// cases.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = ApFirstLastName_MaxLength)
    ]
  public string ApFirstLastName
  {
    get => apFirstLastName ?? "";
    set => apFirstLastName =
      TrimEnd(Substring(value, 1, ApFirstLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApFirstLastName attribute.</summary>
  [JsonPropertyName("apFirstLastName")]
  [Computed]
  public string ApFirstLastName_Json
  {
    get => NullIf(ApFirstLastName, "");
    set => ApFirstLastName = value;
  }

  /// <summary>Length of the AP_SECOND_LAST_NAME attribute.</summary>
  public const int ApSecondLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_SECOND_LAST_NAME attribute.
  /// Alias - 1 of the absent parent
  /// No special characters allowed except hyphen. Optional field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ApSecondLastName_MaxLength)]
  public string ApSecondLastName
  {
    get => apSecondLastName ?? "";
    set => apSecondLastName =
      TrimEnd(Substring(value, 1, ApSecondLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApSecondLastName attribute.</summary>
  [JsonPropertyName("apSecondLastName")]
  [Computed]
  public string ApSecondLastName_Json
  {
    get => NullIf(ApSecondLastName, "");
    set => ApSecondLastName = value;
  }

  /// <summary>Length of the AP_THIRD_LAST_NAME attribute.</summary>
  public const int ApThirdLastName_MaxLength = 20;

  /// <summary>
  /// The value of the AP_THIRD_LAST_NAME attribute.
  /// Alias - 2 of the absent parent.
  /// No special character allowed except hyphen. Optional field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = ApThirdLastName_MaxLength)
    ]
  public string ApThirdLastName
  {
    get => apThirdLastName ?? "";
    set => apThirdLastName =
      TrimEnd(Substring(value, 1, ApThirdLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApThirdLastName attribute.</summary>
  [JsonPropertyName("apThirdLastName")]
  [Computed]
  public string ApThirdLastName_Json
  {
    get => NullIf(ApThirdLastName, "");
    set => ApThirdLastName = value;
  }

  /// <summary>
  /// The value of the AP_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonPropertyName("apDateOfBirth")]
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? ApDateOfBirth
  {
    get => apDateOfBirth;
    set => apDateOfBirth = value;
  }

  /// <summary>
  /// The value of the AP_DATE_OF_BIRTH_MONTH attribute.
  /// Date of birth of the absent parent.
  /// Required for unknown cases. Default is zeros.
  /// Age must be between 14 and 99 years.
  /// Output flat file will be moved with MMDDYY (6 digit) format.
  /// </summary>
  [JsonPropertyName("apDateOfBirthMonth")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 2)]
  public int ApDateOfBirthMonth
  {
    get => apDateOfBirthMonth;
    set => apDateOfBirthMonth = value;
  }

  /// <summary>
  /// The value of the AP_DATE_OF_BIRTH_DAY attribute.
  /// </summary>
  [JsonPropertyName("apDateOfBirthDay")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 2)]
  public int ApDateOfBirthDay
  {
    get => apDateOfBirthDay;
    set => apDateOfBirthDay = value;
  }

  /// <summary>
  /// The value of the AP_DATE_OF_BIRTH_YEAR attribute.
  /// </summary>
  [JsonPropertyName("apDateOfBirthYear")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 2)]
  public int ApDateOfBirthYear
  {
    get => apDateOfBirthYear;
    set => apDateOfBirthYear = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = Sex_MaxLength)]
  public string Sex
  {
    get => sex ?? "";
    set => sex = TrimEnd(Substring(value, 1, Sex_MaxLength));
  }

  /// <summary>
  /// The json value of the Sex attribute.</summary>
  [JsonPropertyName("sex")]
  [Computed]
  public string Sex_Json
  {
    get => NullIf(Sex, "");
    set => Sex = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = CollectAllResponsesTogether_MaxLength)]
  public string CollectAllResponsesTogether
  {
    get => collectAllResponsesTogether ?? "";
    set => collectAllResponsesTogether =
      TrimEnd(Substring(value, 1, CollectAllResponsesTogether_MaxLength));
  }

  /// <summary>
  /// The json value of the CollectAllResponsesTogether attribute.</summary>
  [JsonPropertyName("collectAllResponsesTogether")]
  [Computed]
  public string CollectAllResponsesTogether_Json
  {
    get => NullIf(CollectAllResponsesTogether, "");
    set => CollectAllResponsesTogether = value;
  }

  /// <summary>Length of the SPACES_3 attribute.</summary>
  public const int Spaces3_MaxLength = 12;

  /// <summary>
  /// The value of the SPACES_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = Spaces3_MaxLength)]
  public string Spaces3
  {
    get => spaces3 ?? "";
    set => spaces3 = TrimEnd(Substring(value, 1, Spaces3_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces3 attribute.</summary>
  [JsonPropertyName("spaces3")]
  [Computed]
  public string Spaces3_Json
  {
    get => NullIf(Spaces3, "");
    set => Spaces3 = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = SendRequestTo_MaxLength)]
  public string SendRequestTo
  {
    get => sendRequestTo ?? "";
    set => sendRequestTo =
      TrimEnd(Substring(value, 1, SendRequestTo_MaxLength));
  }

  /// <summary>
  /// The json value of the SendRequestTo attribute.</summary>
  [JsonPropertyName("sendRequestTo")]
  [Computed]
  public string SendRequestTo_Json
  {
    get => NullIf(SendRequestTo, "");
    set => SendRequestTo = value;
  }

  /// <summary>Length of the TRANSACTION_ERROR attribute.</summary>
  public const int TransactionError_MaxLength = 10;

  /// <summary>
  /// The value of the TRANSACTION_ERROR attribute.
  /// Contains 2-digit error codes concatenated. These error codes are returned 
  /// by FPLS system.
  /// Refer to FPLS Transaction error codes described in FPLS manual.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = TransactionError_MaxLength)]
  public string TransactionError
  {
    get => transactionError ?? "";
    set => transactionError =
      TrimEnd(Substring(value, 1, TransactionError_MaxLength));
  }

  /// <summary>
  /// The json value of the TransactionError attribute.</summary>
  [JsonPropertyName("transactionError")]
  [Computed]
  public string TransactionError_Json
  {
    get => NullIf(TransactionError, "");
    set => TransactionError = value;
  }

  /// <summary>Length of the SPACES_4 attribute.</summary>
  public const int Spaces4_MaxLength = 14;

  /// <summary>
  /// The value of the SPACES_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = Spaces4_MaxLength)]
  public string Spaces4
  {
    get => spaces4 ?? "";
    set => spaces4 = TrimEnd(Substring(value, 1, Spaces4_MaxLength));
  }

  /// <summary>
  /// The json value of the Spaces4 attribute.</summary>
  [JsonPropertyName("spaces4")]
  [Computed]
  public string Spaces4_Json
  {
    get => NullIf(Spaces4, "");
    set => Spaces4 = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = ApCityOfBirth_MaxLength)]
  public string ApCityOfBirth
  {
    get => apCityOfBirth ?? "";
    set => apCityOfBirth =
      TrimEnd(Substring(value, 1, ApCityOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the ApCityOfBirth attribute.</summary>
  [JsonPropertyName("apCityOfBirth")]
  [Computed]
  public string ApCityOfBirth_Json
  {
    get => NullIf(ApCityOfBirth, "");
    set => ApCityOfBirth = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = ApStateOrCountryOfBirth_MaxLength)]
  public string ApStateOrCountryOfBirth
  {
    get => apStateOrCountryOfBirth ?? "";
    set => apStateOrCountryOfBirth =
      TrimEnd(Substring(value, 1, ApStateOrCountryOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the ApStateOrCountryOfBirth attribute.</summary>
  [JsonPropertyName("apStateOrCountryOfBirth")]
  [Computed]
  public string ApStateOrCountryOfBirth_Json
  {
    get => NullIf(ApStateOrCountryOfBirth, "");
    set => ApStateOrCountryOfBirth = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = ApsFathersFirstName_MaxLength)]
  public string ApsFathersFirstName
  {
    get => apsFathersFirstName ?? "";
    set => apsFathersFirstName =
      TrimEnd(Substring(value, 1, ApsFathersFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsFathersFirstName attribute.</summary>
  [JsonPropertyName("apsFathersFirstName")]
  [Computed]
  public string ApsFathersFirstName_Json
  {
    get => NullIf(ApsFathersFirstName, "");
    set => ApsFathersFirstName = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = ApsFathersMi_MaxLength)]
  public string ApsFathersMi
  {
    get => apsFathersMi ?? "";
    set => apsFathersMi = TrimEnd(Substring(value, 1, ApsFathersMi_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsFathersMi attribute.</summary>
  [JsonPropertyName("apsFathersMi")]
  [Computed]
  public string ApsFathersMi_Json
  {
    get => NullIf(ApsFathersMi, "");
    set => ApsFathersMi = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = ApsFathersLastName_MaxLength)]
  public string ApsFathersLastName
  {
    get => apsFathersLastName ?? "";
    set => apsFathersLastName =
      TrimEnd(Substring(value, 1, ApsFathersLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsFathersLastName attribute.</summary>
  [JsonPropertyName("apsFathersLastName")]
  [Computed]
  public string ApsFathersLastName_Json
  {
    get => NullIf(ApsFathersLastName, "");
    set => ApsFathersLastName = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = ApsMothersFirstName_MaxLength)]
  public string ApsMothersFirstName
  {
    get => apsMothersFirstName ?? "";
    set => apsMothersFirstName =
      TrimEnd(Substring(value, 1, ApsMothersFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsMothersFirstName attribute.</summary>
  [JsonPropertyName("apsMothersFirstName")]
  [Computed]
  public string ApsMothersFirstName_Json
  {
    get => NullIf(ApsMothersFirstName, "");
    set => ApsMothersFirstName = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = ApsMothersMi_MaxLength)]
  public string ApsMothersMi
  {
    get => apsMothersMi ?? "";
    set => apsMothersMi = TrimEnd(Substring(value, 1, ApsMothersMi_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsMothersMi attribute.</summary>
  [JsonPropertyName("apsMothersMi")]
  [Computed]
  public string ApsMothersMi_Json
  {
    get => NullIf(ApsMothersMi, "");
    set => ApsMothersMi = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = ApsMothersMaidenName_MaxLength)]
  public string ApsMothersMaidenName
  {
    get => apsMothersMaidenName ?? "";
    set => apsMothersMaidenName =
      TrimEnd(Substring(value, 1, ApsMothersMaidenName_MaxLength));
  }

  /// <summary>
  /// The json value of the ApsMothersMaidenName attribute.</summary>
  [JsonPropertyName("apsMothersMaidenName")]
  [Computed]
  public string ApsMothersMaidenName_Json
  {
    get => NullIf(ApsMothersMaidenName, "");
    set => ApsMothersMaidenName = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = CpSsn_MaxLength)]
  public string CpSsn
  {
    get => cpSsn ?? "";
    set => cpSsn = TrimEnd(Substring(value, 1, CpSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CpSsn attribute.</summary>
  [JsonPropertyName("cpSsn")]
  [Computed]
  public string CpSsn_Json
  {
    get => NullIf(CpSsn, "");
    set => CpSsn = value;
  }

  private string spaces1;
  private string stateAbbr;
  private string stationNumber;
  private string spaces2;
  private string transactionType;
  private string ssn;
  private string apCsePersonNumber;
  private int fplsLocateRequestIdentifier;
  private string localCode;
  private string usersField;
  private string typeOfCase;
  private string apFirstName;
  private string apMiddleName;
  private string apFirstLastName;
  private string apSecondLastName;
  private string apThirdLastName;
  private DateTime? apDateOfBirth;
  private int apDateOfBirthMonth;
  private int apDateOfBirthDay;
  private int apDateOfBirthYear;
  private string sex;
  private string collectAllResponsesTogether;
  private string spaces3;
  private string sendRequestTo;
  private string transactionError;
  private string spaces4;
  private string apCityOfBirth;
  private string apStateOrCountryOfBirth;
  private string apsFathersFirstName;
  private string apsFathersMi;
  private string apsFathersLastName;
  private string apsMothersFirstName;
  private string apsMothersMi;
  private string apsMothersMaidenName;
  private string cpSsn;
}

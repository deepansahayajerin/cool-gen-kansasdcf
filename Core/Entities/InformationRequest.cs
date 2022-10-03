// The source file: INFORMATION_REQUEST, ID: 371422046, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// An Information Request is created when CSE receives a telephone, written, or
/// in-person request for assistance. This information request may result in
/// the generation of an Non-ADC application. **Note** Wehn an application is
/// received it will be reviewed for completeness. Complete applications should
/// result in the opening of a Non-ADC case. Information request that are wrong
/// numbers, not social service related, or which refer to a known existing
/// request or case, will not be logged and will not be considered a CSE
/// Information Request.
/// </summary>
[Serializable]
public partial class InformationRequest: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InformationRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InformationRequest(InformationRequest that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InformationRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 2, Type = MemberType.Timestamp)]
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
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
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
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the REASON_INCOMPLETE attribute.</summary>
  public const int ReasonIncomplete_MaxLength = 60;

  /// <summary>
  /// The value of the REASON_INCOMPLETE attribute.
  /// The attributes identifies why the application is incomplete.
  /// </summary>
  [JsonPropertyName("reasonIncomplete")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ReasonIncomplete_MaxLength, Optional = true)]
  public string ReasonIncomplete
  {
    get => Get<string>("reasonIncomplete");
    set => Set(
      "reasonIncomplete",
      TrimEnd(Substring(value, 1, ReasonIncomplete_MaxLength)));
  }

  /// <summary>Length of the SERVICE_CODE attribute.</summary>
  public const int ServiceCode_MaxLength = 2;

  /// <summary>
  /// The value of the SERVICE_CODE attribute.
  /// An indication of the type of service that the AR has requested that CSE 
  /// provide on her behalf.
  /// EX:    LO - Locate Only
  ///        WI - With Medical
  ///        WO - Without Medical
  /// </summary>
  [JsonPropertyName("serviceCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = ServiceCode_MaxLength, Optional
    = true)]
  public string ServiceCode
  {
    get => Get<string>("serviceCode");
    set => Set(
      "serviceCode", TrimEnd(Substring(value, 1, ServiceCode_MaxLength)));
  }

  /// <summary>Length of the NONPARENT_QUESTIONNAIRE_SENT attribute.</summary>
  public const int NonparentQuestionnaireSent_MaxLength = 1;

  /// <summary>
  /// The value of the NONPARENT_QUESTIONNAIRE_SENT attribute.
  /// This indicates that a nonparent Questionnaire was sent with the Non-ADC 
  /// application.
  /// </summary>
  [JsonPropertyName("nonparentQuestionnaireSent")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = NonparentQuestionnaireSent_MaxLength, Optional = true)]
  public string NonparentQuestionnaireSent
  {
    get => Get<string>("nonparentQuestionnaireSent");
    set => Set(
      "nonparentQuestionnaireSent", TrimEnd(Substring(value, 1,
      NonparentQuestionnaireSent_MaxLength)));
  }

  /// <summary>Length of the PARENT_QUESTIONNAIRE_SENT attribute.</summary>
  public const int ParentQuestionnaireSent_MaxLength = 1;

  /// <summary>
  /// The value of the PARENT_QUESTIONNAIRE_SENT attribute.
  /// This indicates that a parent questionnaire was sent with the Non-ADC 
  /// questionnaire.
  /// </summary>
  [JsonPropertyName("parentQuestionnaireSent")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = ParentQuestionnaireSent_MaxLength, Optional = true)]
  public string ParentQuestionnaireSent
  {
    get => Get<string>("parentQuestionnaireSent");
    set => Set(
      "parentQuestionnaireSent", TrimEnd(Substring(value, 1,
      ParentQuestionnaireSent_MaxLength)));
  }

  /// <summary>Length of the PATERNITY_QUESTIONNAIRE_SENT attribute.</summary>
  public const int PaternityQuestionnaireSent_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_QUESTIONNAIRE_SENT attribute.
  /// This indicates that a paternity questionnaire was sent with the Non-ADC 
  /// application packet.		
  /// </summary>
  [JsonPropertyName("paternityQuestionnaireSent")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = PaternityQuestionnaireSent_MaxLength, Optional = true)]
  public string PaternityQuestionnaireSent
  {
    get => Get<string>("paternityQuestionnaireSent");
    set => Set(
      "paternityQuestionnaireSent", TrimEnd(Substring(value, 1,
      PaternityQuestionnaireSent_MaxLength)));
  }

  /// <summary>Length of the APPLICATION_SENT_INDICATOR attribute.</summary>
  public const int ApplicationSentIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the APPLICATION_SENT_INDICATOR attribute.
  /// This flag indicates whether or not a Non-ADC
  /// application should be generated.
  /// Values:
  /// Y = Yes
  /// N = No  (Default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ApplicationSentIndicator_MaxLength)]
  public string ApplicationSentIndicator
  {
    get => Get<string>("applicationSentIndicator") ?? "";
    set => Set(
      "applicationSentIndicator", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ApplicationSentIndicator_MaxLength)));
  }

  /// <summary>
  /// The json value of the ApplicationSentIndicator attribute.</summary>
  [JsonPropertyName("applicationSentIndicator")]
  [Computed]
  public string ApplicationSentIndicator_Json
  {
    get => NullIf(ApplicationSentIndicator, "");
    set => ApplicationSentIndicator = value;
  }

  /// <summary>Length of the QUESTIONNAIRE_TYPE_INDICATOR attribute.</summary>
  public const int QuestionnaireTypeIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the QUESTIONNAIRE_TYPE_INDICATOR attribute.
  /// This is an indicator which will specify which type of questionnaire will 
  /// be sent to the requestor.		
  /// Types:  Paternity, Parent, Non-Parent
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = QuestionnaireTypeIndicator_MaxLength)]
  public string QuestionnaireTypeIndicator
  {
    get => Get<string>("questionnaireTypeIndicator") ?? "";
    set => Set(
      "questionnaireTypeIndicator", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, QuestionnaireTypeIndicator_MaxLength)));
  }

  /// <summary>
  /// The json value of the QuestionnaireTypeIndicator attribute.</summary>
  [JsonPropertyName("questionnaireTypeIndicator")]
  [Computed]
  public string QuestionnaireTypeIndicator_Json
  {
    get => NullIf(QuestionnaireTypeIndicator, "");
    set => QuestionnaireTypeIndicator = value;
  }

  /// <summary>
  /// The value of the DATE_RECEIVED_BY_CSE_COMPLETE attribute.
  /// This is the date that CSE received a complete application.
  /// </summary>
  [JsonPropertyName("dateReceivedByCseComplete")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? DateReceivedByCseComplete
  {
    get => Get<DateTime?>("dateReceivedByCseComplete");
    set => Set("dateReceivedByCseComplete", value);
  }

  /// <summary>
  /// The value of the DATE_RECEIVED_BY_CSE_INCOMPLETE attribute.
  /// This is the date the application was received by CSE.  The application was
  /// incomplete.
  /// </summary>
  [JsonPropertyName("dateReceivedByCseIncomplete")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? DateReceivedByCseIncomplete
  {
    get => Get<DateTime?>("dateReceivedByCseIncomplete");
    set => Set("dateReceivedByCseIncomplete", value);
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A system-generated number used soley to identify inquiries.  Has no innate
  /// significance.
  /// This is also the application number, and is either written on the 
  /// application, or printed on the application in the case of a computer-
  /// generated application.
  /// </summary>
  [JsonPropertyName("number")]
  [DefaultValue(0L)]
  [Member(Index = 14, Type = MemberType.Number, Length = 10)]
  public long Number
  {
    get => Get<long?>("number") ?? 0L;
    set => Set("number", value == 0L ? null : value as long?);
  }

  /// <summary>
  /// The value of the DATE_APPLICATION_REQUESTED attribute.
  /// The date a request for an application was received by CSE.  Mandated by 
  /// federal regulations.
  /// </summary>
  [JsonPropertyName("dateApplicationRequested")]
  [Member(Index = 15, Type = MemberType.Date)]
  public DateTime? DateApplicationRequested
  {
    get => Get<DateTime?>("dateApplicationRequested");
    set => Set("dateApplicationRequested", value);
  }

  /// <summary>Length of the CALLER_LAST_NAME attribute.</summary>
  public const int CallerLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CALLER_LAST_NAME attribute.
  /// This is the last name of the person who called and initiated the 
  /// information request.  Not necessarily the same as the applicant.
  /// </summary>
  [JsonPropertyName("callerLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = CallerLastName_MaxLength, Optional = true)]
  public string CallerLastName
  {
    get => Get<string>("callerLastName");
    set => Set(
      "callerLastName", TrimEnd(Substring(value, 1, CallerLastName_MaxLength)));
      
  }

  /// <summary>Length of the CALLER_FIRST_NAME attribute.</summary>
  public const int CallerFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CALLER_FIRST_NAME attribute.
  /// This is the first name of the person who challed and initiated the 
  /// infomration request.  Not necessarily the same as the applicant.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = CallerFirstName_MaxLength)
    ]
  public string CallerFirstName
  {
    get => Get<string>("callerFirstName") ?? "";
    set => Set(
      "callerFirstName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CallerFirstName_MaxLength)));
  }

  /// <summary>
  /// The json value of the CallerFirstName attribute.</summary>
  [JsonPropertyName("callerFirstName")]
  [Computed]
  public string CallerFirstName_Json
  {
    get => NullIf(CallerFirstName, "");
    set => CallerFirstName = value;
  }

  /// <summary>Length of the CALLER_MIDDLE_INITIAL attribute.</summary>
  public const int CallerMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the CALLER_MIDDLE_INITIAL attribute.
  /// This is the middle initial of the person who challed and initiated the 
  /// infomration request.  Not necessarily the same as the applicant.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = CallerMiddleInitial_MaxLength)]
  public string CallerMiddleInitial
  {
    get => Get<string>("callerMiddleInitial") ?? "";
    set => Set(
      "callerMiddleInitial", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CallerMiddleInitial_MaxLength)));
  }

  /// <summary>
  /// The json value of the CallerMiddleInitial attribute.</summary>
  [JsonPropertyName("callerMiddleInitial")]
  [Computed]
  public string CallerMiddleInitial_Json
  {
    get => NullIf(CallerMiddleInitial, "");
    set => CallerMiddleInitial = value;
  }

  /// <summary>Length of the INQUIRER_NAME_SUFFIX attribute.</summary>
  public const int InquirerNameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the INQUIRER_NAME_SUFFIX attribute.
  /// </summary>
  [JsonPropertyName("inquirerNameSuffix")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = InquirerNameSuffix_MaxLength, Optional = true)]
  public string InquirerNameSuffix
  {
    get => Get<string>("inquirerNameSuffix");
    set => Set(
      "inquirerNameSuffix", TrimEnd(Substring(value, 1,
      InquirerNameSuffix_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_LAST_NAME attribute.</summary>
  public const int ApplicantLastName_MaxLength = 17;

  /// <summary>
  /// The value of the APPLICANT_LAST_NAME attribute.
  /// Name of the person to whom the application was sent.  When an inquiry is 
  /// received, some amount of pre-screening is done to determine if there is
  /// potentially a CSE service that can be provided.  The application will be
  /// sent only if there is such a service to be provided, and only to a
  /// potential recipient of such CSE service.  Therefore, the applicant will
  /// ALWAYS be a person with standing in a potential CSE case.
  /// </summary>
  [JsonPropertyName("applicantLastName")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ApplicantLastName_MaxLength, Optional = true)]
  public string ApplicantLastName
  {
    get => Get<string>("applicantLastName");
    set => Set(
      "applicantLastName", TrimEnd(Substring(value, 1,
      ApplicantLastName_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_FIRST_NAME attribute.</summary>
  public const int ApplicantFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the APPLICANT_FIRST_NAME attribute.
  /// The first name of the person requesting a Non-ADC application.
  /// </summary>
  [JsonPropertyName("applicantFirstName")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ApplicantFirstName_MaxLength, Optional = true)]
  public string ApplicantFirstName
  {
    get => Get<string>("applicantFirstName");
    set => Set(
      "applicantFirstName", TrimEnd(Substring(value, 1,
      ApplicantFirstName_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_MIDDLE_INITIAL attribute.</summary>
  public const int ApplicantMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the APPLICANT_MIDDLE_INITIAL attribute.
  /// The middle initial of the person requesting a Non-ADC application
  /// </summary>
  [JsonPropertyName("applicantMiddleInitial")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ApplicantMiddleInitial_MaxLength, Optional = true)]
  public string ApplicantMiddleInitial
  {
    get => Get<string>("applicantMiddleInitial");
    set => Set(
      "applicantMiddleInitial", TrimEnd(Substring(value, 1,
      ApplicantMiddleInitial_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_NAME_SUFFIX attribute.</summary>
  public const int ApplicantNameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the APPLICANT_NAME_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ApplicantNameSuffix_MaxLength)]
  public string ApplicantNameSuffix
  {
    get => Get<string>("applicantNameSuffix") ?? "";
    set => Set(
      "applicantNameSuffix", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ApplicantNameSuffix_MaxLength)));
  }

  /// <summary>
  /// The json value of the ApplicantNameSuffix attribute.</summary>
  [JsonPropertyName("applicantNameSuffix")]
  [Computed]
  public string ApplicantNameSuffix_Json
  {
    get => NullIf(ApplicantNameSuffix, "");
    set => ApplicantNameSuffix = value;
  }

  /// <summary>Length of the APPLICANT_STREET_1 attribute.</summary>
  public const int ApplicantStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the APPLICANT_STREET_1 attribute.
  /// Address where the application is being sent.
  /// </summary>
  [JsonPropertyName("applicantStreet1")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ApplicantStreet1_MaxLength, Optional = true)]
  public string ApplicantStreet1
  {
    get => Get<string>("applicantStreet1");
    set => Set(
      "applicantStreet1",
      TrimEnd(Substring(value, 1, ApplicantStreet1_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_STREET_2 attribute.</summary>
  public const int ApplicantStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the APPLICANT_STREET_2 attribute.
  /// Address where the application is being sent.
  /// </summary>
  [JsonPropertyName("applicantStreet2")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = ApplicantStreet2_MaxLength, Optional = true)]
  public string ApplicantStreet2
  {
    get => Get<string>("applicantStreet2");
    set => Set(
      "applicantStreet2",
      TrimEnd(Substring(value, 1, ApplicantStreet2_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_CITY attribute.</summary>
  public const int ApplicantCity_MaxLength = 15;

  /// <summary>
  /// The value of the APPLICANT_CITY attribute.
  /// City where the application is being sent.
  /// </summary>
  [JsonPropertyName("applicantCity")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = ApplicantCity_MaxLength, Optional = true)]
  public string ApplicantCity
  {
    get => Get<string>("applicantCity");
    set => Set(
      "applicantCity", TrimEnd(Substring(value, 1, ApplicantCity_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_STATE attribute.</summary>
  public const int ApplicantState_MaxLength = 2;

  /// <summary>
  /// The value of the APPLICANT_STATE attribute.
  /// 2-letter state code where the application is being sent.
  /// </summary>
  [JsonPropertyName("applicantState")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = ApplicantState_MaxLength, Optional = true)]
  public string ApplicantState
  {
    get => Get<string>("applicantState");
    set => Set(
      "applicantState", TrimEnd(Substring(value, 1, ApplicantState_MaxLength)));
      
  }

  /// <summary>Length of the APPLICANT_ZIP5 attribute.</summary>
  public const int ApplicantZip5_MaxLength = 5;

  /// <summary>
  /// The value of the APPLICANT_ZIP5 attribute.
  /// Postal Code of the address the application was sent to.
  /// Includes: US zip codes, foreign postal code.
  /// </summary>
  [JsonPropertyName("applicantZip5")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = ApplicantZip5_MaxLength, Optional = true)]
  public string ApplicantZip5
  {
    get => Get<string>("applicantZip5");
    set => Set(
      "applicantZip5", TrimEnd(Substring(value, 1, ApplicantZip5_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_ZIP4 attribute.</summary>
  public const int ApplicantZip4_MaxLength = 4;

  /// <summary>
  /// The value of the APPLICANT_ZIP4 attribute.
  /// The four digit addressing standard US postal Code used with APPLICANT_ZIP5
  /// to further identify the region in which the applicant is located.
  /// </summary>
  [JsonPropertyName("applicantZip4")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = ApplicantZip4_MaxLength, Optional = true)]
  public string ApplicantZip4
  {
    get => Get<string>("applicantZip4");
    set => Set(
      "applicantZip4", TrimEnd(Substring(value, 1, ApplicantZip4_MaxLength)));
  }

  /// <summary>Length of the APPLICANT_ZIP3 attribute.</summary>
  public const int ApplicantZip3_MaxLength = 3;

  /// <summary>
  /// The value of the APPLICANT_ZIP3 attribute.
  /// This is the last three digits of the zip code of the address where the 
  /// application was sent.
  /// </summary>
  [JsonPropertyName("applicantZip3")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = ApplicantZip3_MaxLength, Optional = true)]
  public string ApplicantZip3
  {
    get => Get<string>("applicantZip3");
    set => Set(
      "applicantZip3", TrimEnd(Substring(value, 1, ApplicantZip3_MaxLength)));
  }

  /// <summary>
  /// The value of the APPLICANT_PHONE attribute.
  /// phone number of the applicant.
  /// </summary>
  [JsonPropertyName("applicantPhone")]
  [Member(Index = 31, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ApplicantPhone
  {
    get => Get<int?>("applicantPhone");
    set => Set("applicantPhone", value);
  }

  /// <summary>
  /// The value of the DATE_APPLICATION_SENT attribute.
  /// Date the application was sent out by CSE.  This date is required by 
  /// federal mandate.  It has been decided that program information ALWAYS
  /// accompanies the application, this date also fulfills the requirement that
  /// the date the program information is provided to the Applicant be kept.
  /// </summary>
  [JsonPropertyName("dateApplicationSent")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? DateApplicationSent
  {
    get => Get<DateTime?>("dateApplicationSent");
    set => Set("dateApplicationSent", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Type of inquiry received.
  /// Values:
  /// C = CSE
  /// L = Legal Services
  /// S = Social Services
  /// O = Other
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = Type1_MaxLength)]
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

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// A free form area for worker comments.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 34, Type = MemberType.Char, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", TrimEnd(Substring(value, 1, Note_MaxLength)));
  }

  /// <summary>Length of the REASON_DENIED attribute.</summary>
  public const int ReasonDenied_MaxLength = 60;

  /// <summary>
  /// The value of the REASON_DENIED attribute.
  /// The reason that the Non-ADC application was denied and no CSE case was 
  /// opened.
  /// </summary>
  [JsonPropertyName("reasonDenied")]
  [Member(Index = 35, Type = MemberType.Char, Length = ReasonDenied_MaxLength, Optional
    = true)]
  public string ReasonDenied
  {
    get => Get<string>("reasonDenied");
    set => Set(
      "reasonDenied", TrimEnd(Substring(value, 1, ReasonDenied_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_DENIED attribute.
  /// The date the Non-ADC application was denied and no CSE case was opened.
  /// </summary>
  [JsonPropertyName("dateDenied")]
  [Member(Index = 36, Type = MemberType.Date, Optional = true)]
  public DateTime? DateDenied
  {
    get => Get<DateTime?>("dateDenied");
    set => Set("dateDenied", value);
  }

  /// <summary>
  /// The value of the APPLICANT_AREA_CODE attribute.
  /// THE APPLICANT PHONE NUMBER AREA CODE.
  /// </summary>
  [JsonPropertyName("applicantAreaCode")]
  [Member(Index = 37, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ApplicantAreaCode
  {
    get => Get<int?>("applicantAreaCode");
    set => Set("applicantAreaCode", value);
  }

  /// <summary>Length of the APPLICATION_PROCESSED_IND attribute.</summary>
  public const int ApplicationProcessedInd_MaxLength = 1;

  /// <summary>
  /// The value of the APPLICATION_PROCESSED_IND attribute.
  /// APPLICATION_PROCESSED_IND is to provide a place to store a value 
  /// indicating as to wherther or not the Application being dealt with on an
  /// Information Request has been processed or not. The possible values and
  /// their meaning are as follows:
  /// 
  /// Y implies the Application has been processed.
  /// Blank or N (or anything else) implies the Application has not been
  /// processed.
  /// </summary>
  [JsonPropertyName("applicationProcessedInd")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = ApplicationProcessedInd_MaxLength, Optional = true)]
  public string ApplicationProcessedInd
  {
    get => Get<string>("applicationProcessedInd");
    set => Set(
      "applicationProcessedInd", TrimEnd(Substring(value, 1,
      ApplicationProcessedInd_MaxLength)));
  }

  /// <summary>Length of the NAME_SEARCH_COMPLETE attribute.</summary>
  public const int NameSearchComplete_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_SEARCH_COMPLETE attribute.
  /// </summary>
  [JsonPropertyName("nameSearchComplete")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = NameSearchComplete_MaxLength, Optional = true)]
  public string NameSearchComplete
  {
    get => Get<string>("nameSearchComplete");
    set => Set(
      "nameSearchComplete", TrimEnd(Substring(value, 1,
      NameSearchComplete_MaxLength)));
  }

  /// <summary>Length of the REOPEN_REASON_TYPE attribute.</summary>
  public const int ReopenReasonType_MaxLength = 1;

  /// <summary>
  /// The value of the REOPEN_REASON_TYPE attribute.
  /// The reason a case was reopened.
  /// </summary>
  [JsonPropertyName("reopenReasonType")]
  [Member(Index = 40, Type = MemberType.Varchar, Length
    = ReopenReasonType_MaxLength, Optional = true)]
  public string ReopenReasonType
  {
    get => Get<string>("reopenReasonType");
    set => Set(
      "reopenReasonType", Substring(value, 1, ReopenReasonType_MaxLength));
  }

  /// <summary>Length of the MISCELLANEOUS_REASON attribute.</summary>
  public const int MiscellaneousReason_MaxLength = 20;

  /// <summary>
  /// The value of the MISCELLANEOUS_REASON attribute.
  /// Miscellaneous reason the case was reopened.
  /// </summary>
  [JsonPropertyName("miscellaneousReason")]
  [Member(Index = 41, Type = MemberType.Varchar, Length
    = MiscellaneousReason_MaxLength, Optional = true)]
  public string MiscellaneousReason
  {
    get => Get<string>("miscellaneousReason");
    set => Set(
      "miscellaneousReason",
      Substring(value, 1, MiscellaneousReason_MaxLength));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int FkCktCasenumb_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = FkCktCasenumb_MaxLength)]
  public string FkCktCasenumb
  {
    get => Get<string>("fkCktCasenumb") ?? "";
    set => Set(
      "fkCktCasenumb", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, FkCktCasenumb_MaxLength)));
  }

  /// <summary>
  /// The json value of the FkCktCasenumb attribute.</summary>
  [JsonPropertyName("fkCktCasenumb")]
  [Computed]
  public string FkCktCasenumb_Json
  {
    get => NullIf(FkCktCasenumb, "");
    set => FkCktCasenumb = value;
  }
}

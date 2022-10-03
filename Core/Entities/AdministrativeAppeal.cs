// The source file: ADMINISTRATIVE_APPEAL, ID: 371430402, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// A formal request for a hearing to resolve a dispute related to Child Support
/// Enforcement administrative actions or decisions.
/// </summary>
[Serializable]
public partial class AdministrativeAppeal: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdministrativeAppeal()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdministrativeAppeal(AdministrativeAppeal that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdministrativeAppeal Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify an Administrative 
  /// Appeal.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 15;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number that is assigned by the Hearing Unit which identifies an 
  /// Administrative Appeal.
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 2, Type = MemberType.Char, Length = Number_MaxLength, Optional
    = true)]
  public string Number
  {
    get => Get<string>("number");
    set => Set("number", TrimEnd(Substring(value, 1, Number_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of Administrative Appeal such as Fair Hearing, Federal Debt 
  /// Setoff, and State Debt Setoff.	
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The value of the REQUEST_DATE attribute.
  /// The date the appeal form is completed.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? RequestDate
  {
    get => Get<DateTime?>("requestDate");
    set => Set("requestDate", value);
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// The date the Request for Fair Hearing is received by the Fair Hearing 
  /// Board.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? ReceivedDate
  {
    get => Get<DateTime?>("receivedDate");
    set => Set("receivedDate", value);
  }

  /// <summary>Length of the REASON attribute.</summary>
  public const int Reason_MaxLength = 240;

  /// <summary>
  /// The value of the REASON attribute.
  /// The reasons and information why the enforcement action decision is not 
  /// satisfactory.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Varchar, Length = Reason_MaxLength)]
  public string Reason
  {
    get => Get<string>("reason") ?? "";
    set => Set(
      "reason", IsEmpty(value) ? null : Substring(value, 1, Reason_MaxLength));
  }

  /// <summary>
  /// The json value of the Reason attribute.</summary>
  [JsonPropertyName("reason")]
  [Computed]
  public string Reason_Json
  {
    get => NullIf(Reason, "");
    set => Reason = value;
  }

  /// <summary>Length of the RESPONDENT attribute.</summary>
  public const int Respondent_MaxLength = 33;

  /// <summary>
  /// The value of the RESPONDENT attribute.
  /// The name of the person or agency whose enforcement action decision is 
  /// being appealed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Respondent_MaxLength)]
  public string Respondent
  {
    get => Get<string>("respondent") ?? "";
    set => Set(
      "respondent", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Respondent_MaxLength)));
  }

  /// <summary>
  /// The json value of the Respondent attribute.</summary>
  [JsonPropertyName("respondent")]
  [Computed]
  public string Respondent_Json
  {
    get => NullIf(Respondent, "");
    set => Respondent = value;
  }

  /// <summary>Length of the APPELLANT_LAST_NAME attribute.</summary>
  public const int AppellantLastName_MaxLength = 17;

  /// <summary>
  /// The value of the APPELLANT_LAST_NAME attribute.
  /// The last name of the dissatisfied person, not a CSE person, filing the 
  /// Request for a Fair Hearing.
  /// </summary>
  [JsonPropertyName("appellantLastName")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = AppellantLastName_MaxLength, Optional = true)]
  public string AppellantLastName
  {
    get => Get<string>("appellantLastName");
    set => Set(
      "appellantLastName", TrimEnd(Substring(value, 1,
      AppellantLastName_MaxLength)));
  }

  /// <summary>Length of the APPELLANT_FIRST_NAME attribute.</summary>
  public const int AppellantFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the APPELLANT_FIRST_NAME attribute.
  /// The first name of the dissatisfied person, not a CSE person, filing the 
  /// Request for a Fair Hearing.
  /// </summary>
  [JsonPropertyName("appellantFirstName")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = AppellantFirstName_MaxLength, Optional = true)]
  public string AppellantFirstName
  {
    get => Get<string>("appellantFirstName");
    set => Set(
      "appellantFirstName", TrimEnd(Substring(value, 1,
      AppellantFirstName_MaxLength)));
  }

  /// <summary>Length of the APPELLANT_MIDDLE_INITIAL attribute.</summary>
  public const int AppellantMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the APPELLANT_MIDDLE_INITIAL attribute.
  /// The middle initial of the dissatisfied person, not a CSE person, filing 
  /// the Request for a Fair Hearing.
  /// </summary>
  [JsonPropertyName("appellantMiddleInitial")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AppellantMiddleInitial_MaxLength, Optional = true)]
  public string AppellantMiddleInitial
  {
    get => Get<string>("appellantMiddleInitial");
    set => Set(
      "appellantMiddleInitial", TrimEnd(Substring(value, 1,
      AppellantMiddleInitial_MaxLength)));
  }

  /// <summary>Length of the APPELLANT_SUFFIX attribute.</summary>
  public const int AppellantSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the APPELLANT_SUFFIX attribute.
  /// The suffix of the dissatisfied person, not a CSE person, filing the 
  /// Request for a Fair Hearing.
  /// </summary>
  [JsonPropertyName("appellantSuffix")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AppellantSuffix_MaxLength, Optional = true)]
  public string AppellantSuffix
  {
    get => Get<string>("appellantSuffix");
    set => Set(
      "appellantSuffix",
      TrimEnd(Substring(value, 1, AppellantSuffix_MaxLength)));
  }

  /// <summary>Length of the APPELLANT_RELATIONSHIP attribute.</summary>
  public const int AppellantRelationship_MaxLength = 20;

  /// <summary>
  /// The value of the APPELLANT_RELATIONSHIP attribute.
  /// The relationship of the appellant to the recipient, applicant, or 
  /// provider.
  /// </summary>
  [JsonPropertyName("appellantRelationship")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = AppellantRelationship_MaxLength, Optional = true)]
  public string AppellantRelationship
  {
    get => Get<string>("appellantRelationship");
    set => Set(
      "appellantRelationship", TrimEnd(Substring(value, 1,
      AppellantRelationship_MaxLength)));
  }

  /// <summary>Length of the OUTCOME attribute.</summary>
  public const int Outcome_MaxLength = 240;

  /// <summary>
  /// The value of the OUTCOME attribute.
  /// This is the ruling, which indicates specific directions to a person or 
  /// agency.
  /// This outcome resolves an administrative appeal without having a hearing.
  /// </summary>
  [JsonPropertyName("outcome")]
  [Member(Index = 13, Type = MemberType.Varchar, Length = Outcome_MaxLength, Optional
    = true)]
  public string Outcome
  {
    get => Get<string>("outcome");
    set => Set("outcome", Substring(value, 1, Outcome_MaxLength));
  }

  /// <summary>Length of the REVIEW_OUTCOME attribute.</summary>
  public const int ReviewOutcome_MaxLength = 240;

  /// <summary>
  /// The value of the REVIEW_OUTCOME attribute.
  /// This is the outcome when an Administrative Appeal had been requested for 
  /// further review.
  /// </summary>
  [JsonPropertyName("reviewOutcome")]
  [Member(Index = 14, Type = MemberType.Varchar, Length
    = ReviewOutcome_MaxLength, Optional = true)]
  public string ReviewOutcome
  {
    get => Get<string>("reviewOutcome");
    set => Set("reviewOutcome", Substring(value, 1, ReviewOutcome_MaxLength));
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// The date the Administrative Request is forwarded to the Administrative 
  /// Tribunal.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? Date
  {
    get => Get<DateTime?>("date");
    set => Set("date", value);
  }

  /// <summary>
  /// The value of the ADMIN_ORDER_DATE attribute.
  /// The date the Administrative Order has been entered.
  /// </summary>
  [JsonPropertyName("adminOrderDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? AdminOrderDate
  {
    get => Get<DateTime?>("adminOrderDate");
    set => Set("adminOrderDate", value);
  }

  /// <summary>
  /// The value of the WITHDRAW_DATE attribute.
  /// The date that the administrative appeal is withdrawn.
  /// </summary>
  [JsonPropertyName("withdrawDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? WithdrawDate
  {
    get => Get<DateTime?>("withdrawDate");
    set => Set("withdrawDate", value);
  }

  /// <summary>Length of the WITHDRAW_REASON attribute.</summary>
  public const int WithdrawReason_MaxLength = 240;

  /// <summary>
  /// The value of the WITHDRAW_REASON attribute.
  /// The reason the appellant withdraws the administrative appeal.
  /// </summary>
  [JsonPropertyName("withdrawReason")]
  [Member(Index = 18, Type = MemberType.Varchar, Length
    = WithdrawReason_MaxLength, Optional = true)]
  public string WithdrawReason
  {
    get => Get<string>("withdrawReason");
    set => Set("withdrawReason", Substring(value, 1, WithdrawReason_MaxLength));
  }

  /// <summary>Length of the REQUEST_FURTHER_REVIEW attribute.</summary>
  public const int RequestFurtherReview_MaxLength = 240;

  /// <summary>
  /// The value of the REQUEST_FURTHER_REVIEW attribute.
  /// The reason for a request to have the appeal reviewed.
  /// </summary>
  [JsonPropertyName("requestFurtherReview")]
  [Member(Index = 19, Type = MemberType.Varchar, Length
    = RequestFurtherReview_MaxLength, Optional = true)]
  public string RequestFurtherReview
  {
    get => Get<string>("requestFurtherReview");
    set => Set(
      "requestFurtherReview",
      Substring(value, 1, RequestFurtherReview_MaxLength));
  }

  /// <summary>
  /// The value of the REQUEST_FURTHER_REVIEW_DATE attribute.
  /// The date of the request to have the appeal reviewed.
  /// </summary>
  [JsonPropertyName("requestFurtherReviewDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestFurtherReviewDate
  {
    get => Get<DateTime?>("requestFurtherReviewDate");
    set => Set("requestFurtherReviewDate", value);
  }

  /// <summary>Length of the JUDICIAL_REVIEW_IND attribute.</summary>
  public const int JudicialReviewInd_MaxLength = 1;

  /// <summary>
  /// The value of the JUDICIAL_REVIEW_IND attribute.
  /// It specifies whether or not it is referred to Judicial Review.
  /// &quot;Y&quot; indicates that it has been referred to judicial review.  
  /// &quot;N&quot; indicates that it is not.
  /// </summary>
  [JsonPropertyName("judicialReviewInd")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = JudicialReviewInd_MaxLength, Optional = true)]
  public string JudicialReviewInd
  {
    get => Get<string>("judicialReviewInd");
    set => Set(
      "judicialReviewInd", TrimEnd(Substring(value, 1,
      JudicialReviewInd_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 23, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => Get<DateTime?>("createdTstamp");
    set => Set("createdTstamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 25, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => Get<DateTime?>("lastUpdatedTstamp");
    set => Set("lastUpdatedTstamp", value);
  }

  /// <summary>Length of the ADMIN_REVIEW_STATE attribute.</summary>
  public const int AdminReviewState_MaxLength = 2;

  /// <summary>
  /// The value of the ADMIN_REVIEW_STATE attribute.
  /// The state that the administrative appeal will be reviewed in.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = AdminReviewState_MaxLength)]
  public string AdminReviewState
  {
    get => Get<string>("adminReviewState") ?? "";
    set => Set(
      "adminReviewState", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AdminReviewState_MaxLength)));
  }

  /// <summary>
  /// The json value of the AdminReviewState attribute.</summary>
  [JsonPropertyName("adminReviewState")]
  [Computed]
  public string AdminReviewState_Json
  {
    get => NullIf(AdminReviewState, "");
    set => AdminReviewState = value;
  }

  /// <summary>Length of the TANF_CODE attribute.</summary>
  public const int AacTanfCode_MaxLength = 1;

  /// <summary>
  /// The value of the TANF_CODE attribute.
  /// Code used to identify TANF or non-TANF.                                 T 
  /// - TANF
  /// 
  /// N - Non-TANF
  /// 
  /// Space - Not Seperated by TANF (Default)
  /// </summary>
  [JsonPropertyName("aacTanfCode")]
  [Member(Index = 27, Type = MemberType.Char, Length = AacTanfCode_MaxLength, Optional
    = true)]
  public string AacTanfCode
  {
    get => Get<string>("aacTanfCode");
    set => Set(
      "aacTanfCode", TrimEnd(Substring(value, 1, AacTanfCode_MaxLength)));
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("aacRTakenDate")]
  [Member(Index = 28, Type = MemberType.Date, Optional = true)]
  public DateTime? AacRTakenDate
  {
    get => Get<DateTime?>("aacRTakenDate");
    set => Set("aacRTakenDate", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaRType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaRType")]
  [Member(Index = 29, Type = MemberType.Char, Length = CpaRType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaRType
  {
    get => Get<string>("cpaRType");
    set => Set("cpaRType", TrimEnd(Substring(value, 1, CpaRType_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AacRType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of certified enforcement action taken.
  /// They can be: FDSO
  ///              SDSO
  ///              CRED
  ///              RECA
  ///              IRS
  ///              KSMW
  /// 
  /// KDWP
  /// </summary>
  [JsonPropertyName("aacRType")]
  [Member(Index = 30, Type = MemberType.Char, Length = AacRType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("KDWP")]
  [Value("KDMV")]
  [Value("FDSO")]
  [Value("IRSC")]
  [Value("COAG")]
  [Value("CRED")]
  [Value("SDSO")]
  [Value("KSMW")]
  public string AacRType
  {
    get => Get<string>("aacRType");
    set => Set("aacRType", TrimEnd(Substring(value, 1, AacRType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspRNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspRNumber")]
  [Member(Index = 31, Type = MemberType.Char, Length = CspRNumber_MaxLength, Optional
    = true)]
  public string CspRNumber
  {
    get => Get<string>("cspRNumber");
    set =>
      Set("cspRNumber", TrimEnd(Substring(value, 1, CspRNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 32, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => Get<int?>("otyId");
    set => Set("otyId", value);
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// The date that an enforcement action is taken against an Obligor for a 
  /// particular Obligation.
  /// </summary>
  [JsonPropertyName("oaaTakenDate")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? OaaTakenDate
  {
    get => Get<DateTime?>("oaaTakenDate");
    set => Set("oaaTakenDate", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AatType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This identifies the type of enforcement that can be taken.
  /// Examples are:
  ///      SDSO
  ///      FDSO
  /// </summary>
  [JsonPropertyName("aatType")]
  [Member(Index = 34, Type = MemberType.Char, Length = AatType_MaxLength, Optional
    = true)]
  public string AatType
  {
    get => Get<string>("aatType");
    set => Set("aatType", TrimEnd(Substring(value, 1, AatType_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaType")]
  [Member(Index = 35, Type = MemberType.Char, Length = CpaType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType");
    set => Set("cpaType", TrimEnd(Substring(value, 1, CpaType_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 36, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => Get<string>("cspNumber");
    set => Set("cspNumber", TrimEnd(Substring(value, 1, CspNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [Member(Index = 37, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgGeneratedId
  {
    get => Get<int?>("obgGeneratedId");
    set => Set("obgGeneratedId", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspQNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspQNumber")]
  [Member(Index = 38, Type = MemberType.Char, Length = CspQNumber_MaxLength, Optional
    = true)]
  public string CspQNumber
  {
    get => Get<string>("cspQNumber");
    set =>
      Set("cspQNumber", TrimEnd(Substring(value, 1, CspQNumber_MaxLength)));
  }
}

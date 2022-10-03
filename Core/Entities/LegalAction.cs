// The source file: LEGAL_ACTION, ID: 371422212, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// A legal action to be filed with or issued by a court or administrative 
/// authority regarding support obligations pertaining to issues of paternity,
/// custody, visitation, property settlements, parental rights, parental
/// obligations, in areas such as child support, spousal support, and medical
/// responsibilities. It also includes judgements in favor of CSE.
/// Examples: Orders, Petitions, Motions, Answers, Notices, Transmittals, Income
/// Withholding Order, Garnishment, Paternity Order, Support Order
/// </summary>
[Serializable]
public partial class LegalAction: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalAction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalAction(LegalAction that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalAction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the FOREIGN_FIPS_STATE attribute.
  /// The first two characters of the FIPS code which identify the state.
  /// </summary>
  [JsonPropertyName("foreignFipsState")]
  [Member(Index = 1, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? ForeignFipsState
  {
    get => Get<int?>("foreignFipsState");
    set => Set("foreignFipsState", value);
  }

  /// <summary>
  /// The value of the FOREIGN_FIPS_COUNTY attribute.
  /// This is 3-5 position of the FIPS code identifying the county.
  /// </summary>
  [JsonPropertyName("foreignFipsCounty")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ForeignFipsCounty
  {
    get => Get<int?>("foreignFipsCounty");
    set => Set("foreignFipsCounty", value);
  }

  /// <summary>
  /// The value of the FOREIGN_FIPS_LOCATION attribute.
  /// The last two positions of the FIPS code which identify a location within 
  /// the county.
  /// </summary>
  [JsonPropertyName("foreignFipsLocation")]
  [Member(Index = 3, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? ForeignFipsLocation
  {
    get => Get<int?>("foreignFipsLocation");
    set => Set("foreignFipsLocation", value);
  }

  /// <summary>Length of the FOREIGN_ORDER_NUMBER attribute.</summary>
  public const int ForeignOrderNumber_MaxLength = 17;

  /// <summary>
  /// The value of the FOREIGN_ORDER_NUMBER attribute.
  /// An identifying number assigned by the Interstate Tribunal.  This is the 
  /// equivelent to Court Case Number which is the Kansas Court Order Number.
  /// </summary>
  [JsonPropertyName("foreignOrderNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ForeignOrderNumber_MaxLength, Optional = true)]
  public string ForeignOrderNumber
  {
    get => Get<string>("foreignOrderNumber");
    set => Set(
      "foreignOrderNumber", TrimEnd(Substring(value, 1,
      ForeignOrderNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the LAST_MODIFICATION_REVIEW_DATE attribute.
  /// The date that the Court Order is reviewed to consider modification.  A 
  /// Court Order must be reviewed at once every three years.
  /// </summary>
  [JsonPropertyName("lastModificationReviewDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? LastModificationReviewDate
  {
    get => Get<DateTime?>("lastModificationReviewDate");
    set => Set("lastModificationReviewDate", value);
  }

  /// <summary>Length of the ATTORNEY_APPROVAL attribute.</summary>
  public const int AttorneyApproval_MaxLength = 3;

  /// <summary>
  /// The value of the ATTORNEY_APPROVAL attribute.
  /// The initials of the attorney approving the legal action document.
  /// </summary>
  [JsonPropertyName("attorneyApproval")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = AttorneyApproval_MaxLength, Optional = true)]
  public string AttorneyApproval
  {
    get => Get<string>("attorneyApproval");
    set => Set(
      "attorneyApproval",
      TrimEnd(Substring(value, 1, AttorneyApproval_MaxLength)));
  }

  /// <summary>
  /// The value of the APPROVAL_SENT_DATE attribute.
  /// The date that the legal action document is sent to the Petitioner for 
  /// their approval and signature.
  /// </summary>
  [JsonPropertyName("approvalSentDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ApprovalSentDate
  {
    get => Get<DateTime?>("approvalSentDate");
    set => Set("approvalSentDate", value);
  }

  /// <summary>Length of the PETITIONER_APPROVAL attribute.</summary>
  public const int PetitionerApproval_MaxLength = 3;

  /// <summary>
  /// The value of the PETITIONER_APPROVAL attribute.
  /// The initials of the Petitioner accepting the legal action document.
  /// </summary>
  [JsonPropertyName("petitionerApproval")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = PetitionerApproval_MaxLength, Optional = true)]
  public string PetitionerApproval
  {
    get => Get<string>("petitionerApproval");
    set => Set(
      "petitionerApproval", TrimEnd(Substring(value, 1,
      PetitionerApproval_MaxLength)));
  }

  /// <summary>
  /// The value of the APPROVAL_RECEIVED_DATE attribute.
  /// The date that the legal action document is returned from the Petitioner 
  /// with their signature approving the legal action.
  /// </summary>
  [JsonPropertyName("approvalReceivedDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? ApprovalReceivedDate
  {
    get => Get<DateTime?>("approvalReceivedDate");
    set => Set("approvalReceivedDate", value);
  }

  /// <summary>Length of the CLASSIFICATION attribute.</summary>
  public const int Classification_MaxLength = 1;

  /// <summary>
  /// The value of the CLASSIFICATION attribute.
  /// The type of legal document.
  /// P-Petition
  /// M-Motion
  /// O-Order
  /// A-Answer
  /// N-Notice
  /// T-Transmittal
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Classification_MaxLength)]
    
  public string Classification
  {
    get => Get<string>("classification") ?? "";
    set => Set(
      "classification", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Classification_MaxLength)));
  }

  /// <summary>
  /// The json value of the Classification attribute.</summary>
  [JsonPropertyName("classification")]
  [Computed]
  public string Classification_Json
  {
    get => NullIf(Classification, "");
    set => Classification = value;
  }

  /// <summary>Length of the ACTION_TAKEN attribute.</summary>
  public const int ActionTaken_MaxLength = 30;

  /// <summary>
  /// The value of the ACTION_TAKEN attribute.
  /// The specific type of classification action taken.
  /// Actions include:
  /// Attachment
  /// Support
  /// Paternity
  /// Interstate order
  /// Income Withholding
  /// Garnishment
  /// Order to Appear and Show Cause
  /// Notice of Intent
  /// Bond
  /// Lien
  /// Pay In Order
  /// Criminal Non-Support
  /// URESA
  /// Contempt
  /// Dormant Reviver
  /// Aid and Execution
  /// Judgement only
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = ActionTaken_MaxLength)]
  public string ActionTaken
  {
    get => Get<string>("actionTaken") ?? "";
    set => Set(
      "actionTaken", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ActionTaken_MaxLength)));
  }

  /// <summary>
  /// The json value of the ActionTaken attribute.</summary>
  [JsonPropertyName("actionTaken")]
  [Computed]
  public string ActionTaken_Json
  {
    get => NullIf(ActionTaken, "");
    set => ActionTaken = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of order taken.  This will be a Temporary Order or Permanent 
  /// Order.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The value of the FILED_DATE attribute.
  /// The specific date that the court or administrative document was file 
  /// stamped and accepted.
  /// </summary>
  [JsonPropertyName("filedDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? FiledDate
  {
    get => Get<DateTime?>("filedDate");
    set => Set("filedDate", value);
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date that the Legal Action is no longer in effect.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => Get<DateTime?>("endDate");
    set => Set("endDate", value);
  }

  /// <summary>
  /// The value of the FOREIGN_ORDER_REGISTRATION_DATE attribute.
  /// Specific date that the foreign order was registered with a Kansas 
  /// tribunal.
  /// </summary>
  [JsonPropertyName("foreignOrderRegistrationDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? ForeignOrderRegistrationDate
  {
    get => Get<DateTime?>("foreignOrderRegistrationDate");
    set => Set("foreignOrderRegistrationDate", value);
  }

  /// <summary>
  /// The value of the URESA_SENT_DATE attribute.
  /// Specific date that the Uniform Reciprocal Enforcement Act (URESA) 
  /// documents were transmitted.
  /// </summary>
  [JsonPropertyName("uresaSentDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? UresaSentDate
  {
    get => Get<DateTime?>("uresaSentDate");
    set => Set("uresaSentDate", value);
  }

  /// <summary>
  /// The value of the URESA_ACKNOWLEDGED_DATE attribute.
  /// Specific date that the Uniform Reciprocal Enforcement Support Act (URESA) 
  /// was acknowledged as being received.
  /// </summary>
  [JsonPropertyName("uresaAcknowledgedDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? UresaAcknowledgedDate
  {
    get => Get<DateTime?>("uresaAcknowledgedDate");
    set => Set("uresaAcknowledgedDate", value);
  }

  /// <summary>
  /// The value of the UIFSA_SENT_DATE attribute.
  /// Specific date the the Uniform Interstate Family Support Act (UIFSA) 
  /// documents were sent to the other state.
  /// </summary>
  [JsonPropertyName("uifsaSentDate")]
  [Member(Index = 19, Type = MemberType.Date, Optional = true)]
  public DateTime? UifsaSentDate
  {
    get => Get<DateTime?>("uifsaSentDate");
    set => Set("uifsaSentDate", value);
  }

  /// <summary>
  /// The value of the UIFSA_ACKNOWLEDGED_DATE attribute.
  /// Specific date the Uniform Interstate Family Support Act (UIFSA) was 
  /// acknowledged as being received.
  /// </summary>
  [JsonPropertyName("uifsaAcknowledgedDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? UifsaAcknowledgedDate
  {
    get => Get<DateTime?>("uifsaAcknowledgedDate");
    set => Set("uifsaAcknowledgedDate", value);
  }

  /// <summary>Length of the INITIATING_STATE attribute.</summary>
  public const int InitiatingState_MaxLength = 2;

  /// <summary>
  /// The value of the INITIATING_STATE attribute.
  /// The state in which a URESA/UIFSA proceeding is commenced and where the 
  /// obligee is located.
  /// </summary>
  [JsonPropertyName("initiatingState")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = InitiatingState_MaxLength, Optional = true)]
  public string InitiatingState
  {
    get => Get<string>("initiatingState");
    set => Set(
      "initiatingState",
      TrimEnd(Substring(value, 1, InitiatingState_MaxLength)));
  }

  /// <summary>Length of the INITIATING_COUNTY attribute.</summary>
  public const int InitiatingCounty_MaxLength = 15;

  /// <summary>
  /// The value of the INITIATING_COUNTY attribute.
  /// The county where the legal action is filed.
  /// </summary>
  [JsonPropertyName("initiatingCounty")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = InitiatingCounty_MaxLength, Optional = true)]
  public string InitiatingCounty
  {
    get => Get<string>("initiatingCounty");
    set => Set(
      "initiatingCounty",
      TrimEnd(Substring(value, 1, InitiatingCounty_MaxLength)));
  }

  /// <summary>Length of the RESPONDING_STATE attribute.</summary>
  public const int RespondingState_MaxLength = 2;

  /// <summary>
  /// The value of the RESPONDING_STATE attribute.
  /// The  state receiving and acting on an interstate child support case.
  /// </summary>
  [JsonPropertyName("respondingState")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = RespondingState_MaxLength, Optional = true)]
  public string RespondingState
  {
    get => Get<string>("respondingState");
    set => Set(
      "respondingState",
      TrimEnd(Substring(value, 1, RespondingState_MaxLength)));
  }

  /// <summary>Length of the RESPONDING_COUNTY attribute.</summary>
  public const int RespondingCounty_MaxLength = 15;

  /// <summary>
  /// The value of the RESPONDING_COUNTY attribute.
  /// The county receiving and acting on an interstate child support case.
  /// </summary>
  [JsonPropertyName("respondingCounty")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = RespondingCounty_MaxLength, Optional = true)]
  public string RespondingCounty
  {
    get => Get<string>("respondingCounty");
    set => Set(
      "respondingCounty",
      TrimEnd(Substring(value, 1, RespondingCounty_MaxLength)));
  }

  /// <summary>Length of the ORDER_AUTHORITY attribute.</summary>
  public const int OrderAuthority_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_AUTHORITY attribute.
  /// The body from which an action originated.  The action will be an 
  /// Administrative Order or a Judicial Order.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = OrderAuthority_MaxLength)]
    
  public string OrderAuthority
  {
    get => Get<string>("orderAuthority") ?? "";
    set => Set(
      "orderAuthority", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, OrderAuthority_MaxLength)));
  }

  /// <summary>
  /// The json value of the OrderAuthority attribute.</summary>
  [JsonPropertyName("orderAuthority")]
  [Computed]
  public string OrderAuthority_Json
  {
    get => NullIf(OrderAuthority, "");
    set => OrderAuthority = value;
  }

  /// <summary>Length of the COURT_CASE_NUMBER attribute.</summary>
  public const int CourtCaseNumber_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_CASE_NUMBER attribute.
  /// Identifying number assigned by the tribunal.
  /// </summary>
  [JsonPropertyName("courtCaseNumber")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = CourtCaseNumber_MaxLength, Optional = true)]
  public string CourtCaseNumber
  {
    get => Get<string>("courtCaseNumber");
    set => Set(
      "courtCaseNumber",
      TrimEnd(Substring(value, 1, CourtCaseNumber_MaxLength)));
  }

  /// <summary>Length of the STANDARD_NUMBER attribute.</summary>
  public const int StandardNumber_MaxLength = 20;

  /// <summary>
  /// The value of the STANDARD_NUMBER attribute.
  /// This is the converted Court Case number which is used by the courts to 
  /// make support payments. The existing SRS court order number consists of the
  /// court case number with a two digit county designator. The court order
  /// number, a SRS attribute, is created by the court when the payment extract
  /// is created. KAESCES adds the &quot;*&quot; in the second digit of the type
  /// if it is blank. The following is a description of the court case number
  /// in which  YR=Year, TT=Type of Court Case, and #=Number.
  /// Example:
  /// CMASS, Douglas, &amp; Sedgwick Co.:
  /// YRTT ##### (ex. 85D 12345)
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number and a 6th digit (0) is added to the front
  /// of the number (0#####) thus making a twelve digit number.
  /// 	Standardized Number - SG85D*012345
  /// Wyandotte Co. Court:
  /// YRTT ##### (ex. 85D 12345) Screen display
  /// YRYRTT ##### (ex. 1985D 12345) Database storage.
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number, the first two digits of the year are
  /// stripped off, and a 6th digit (0) is added to the front of the number (0
  /// #####) thus making a twelve digit number.
  /// 	Standardized Number - WY85D*012345
  /// Shawnee &amp; Johnson Co. Court:
  /// YRTT ##### (ex. 85D 123456)
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number. Johnson County does suppress the 2nd digit
  /// of type on the database if it is blank; however, it is a two digit field.
  /// Also Johnson county asdds an &quot;a&quot; or &quot;b&quot; at the end of
  /// the number for Court Trustee cases if it is a multiple payor situation.
  /// 	Standardized Number - SN85D*123456
  /// 	Multi-payor	    - JO85D*12345A
  /// 			    - JO85D*12345B
  /// </summary>
  [JsonPropertyName("standardNumber")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = StandardNumber_MaxLength, Optional = true)]
  public string StandardNumber
  {
    get => Get<string>("standardNumber");
    set => Set(
      "standardNumber", TrimEnd(Substring(value, 1, StandardNumber_MaxLength)));
      
  }

  /// <summary>Length of the LONG_ARM_STATUTE_INDICATOR attribute.</summary>
  public const int LongArmStatuteIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the LONG_ARM_STATUTE_INDICATOR attribute.
  /// This indicates whether or not the Legal Action enforces the Long Arm 
  /// Statute.  The Long Arm Statute gives Kansas the power to enforce their
  /// orders against a person that lives in another state.
  /// </summary>
  [JsonPropertyName("longArmStatuteIndicator")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = LongArmStatuteIndicator_MaxLength, Optional = true)]
  public string LongArmStatuteIndicator
  {
    get => Get<string>("longArmStatuteIndicator");
    set => Set(
      "longArmStatuteIndicator", TrimEnd(Substring(value, 1,
      LongArmStatuteIndicator_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_LOCATION attribute.</summary>
  public const int PaymentLocation_MaxLength = 6;

  /// <summary>
  /// The value of the PAYMENT_LOCATION attribute.
  /// Scope: A specification of how the payment should be made.
  /// Valid codes are:
  /// 	Court
  /// 	CSE
  /// 	Direct (to AR)
  /// </summary>
  [JsonPropertyName("paymentLocation")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = PaymentLocation_MaxLength, Optional = true)]
  public string PaymentLocation
  {
    get => Get<string>("paymentLocation");
    set => Set(
      "paymentLocation",
      TrimEnd(Substring(value, 1, PaymentLocation_MaxLength)));
  }

  /// <summary>Length of the ESTABLISHMENT_CODE attribute.</summary>
  public const int EstablishmentCode_MaxLength = 2;

  /// <summary>
  /// The value of the ESTABLISHMENT_CODE attribute.
  /// This identifies who or which agency established the court order.  This is 
  /// only required when a Classification of 'O' (for Order) is entered.
  /// Valid codes are:
  /// 	CS - Child Support
  /// 	OT - Other
  /// </summary>
  [JsonPropertyName("establishmentCode")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = EstablishmentCode_MaxLength, Optional = true)]
  public string EstablishmentCode
  {
    get => Get<string>("establishmentCode");
    set => Set(
      "establishmentCode", TrimEnd(Substring(value, 1,
      EstablishmentCode_MaxLength)));
  }

  /// <summary>Length of the DISMISSED_WITHOUT_PREJUDICE_IND attribute.
  /// </summary>
  public const int DismissedWithoutPrejudiceInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISMISSED_WITHOUT_PREJUDICE_IND attribute.
  /// This indicates that the legal action was dismissed without prejudice.
  /// Values are Y or N.
  /// Y means the legal action was dismissed without prejudice.
  /// </summary>
  [JsonPropertyName("dismissedWithoutPrejudiceInd")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = DismissedWithoutPrejudiceInd_MaxLength, Optional = true)]
  public string DismissedWithoutPrejudiceInd
  {
    get => Get<string>("dismissedWithoutPrejudiceInd");
    set => Set(
      "dismissedWithoutPrejudiceInd", TrimEnd(Substring(value, 1,
      DismissedWithoutPrejudiceInd_MaxLength)));
  }

  /// <summary>Length of the DISMISSAL_CODE attribute.</summary>
  public const int DismissalCode_MaxLength = 2;

  /// <summary>
  /// The value of the DISMISSAL_CODE attribute.
  /// The reason code for dismissal.
  /// Examples:
  /// 	LS - Lack of Service
  /// 	LP - Lack of Prosecution
  /// 	NP - No Paternity
  /// 	OT - Other
  /// </summary>
  [JsonPropertyName("dismissalCode")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = DismissalCode_MaxLength, Optional = true)]
  public string DismissalCode
  {
    get => Get<string>("dismissalCode");
    set => Set(
      "dismissalCode", TrimEnd(Substring(value, 1, DismissalCode_MaxLength)));
  }

  /// <summary>
  /// The value of the REFILE_DATE attribute.
  /// The date when appropriate to reseek an order.
  /// </summary>
  [JsonPropertyName("refileDate")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? RefileDate
  {
    get => Get<DateTime?>("refileDate");
    set => Set("refileDate", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrence of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 35, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => Get<DateTime?>("createdTstamp");
    set => Set("createdTstamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrence of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 37, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => Get<DateTime?>("lastUpdatedTstamp");
    set => Set("lastUpdatedTstamp", value);
  }

  /// <summary>Length of the NON_CSE_PETITIONER attribute.</summary>
  public const int NonCsePetitioner_MaxLength = 33;

  /// <summary>
  /// The value of the NON_CSE_PETITIONER attribute.
  /// This attribute specifies the name of the petitioner if the petitioner is 
  /// not CSE.
  /// </summary>
  [JsonPropertyName("nonCsePetitioner")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = NonCsePetitioner_MaxLength, Optional = true)]
  public string NonCsePetitioner
  {
    get => Get<string>("nonCsePetitioner");
    set => Set(
      "nonCsePetitioner",
      TrimEnd(Substring(value, 1, NonCsePetitioner_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_CP_REQ_IWO_BEGIN attribute.
  /// This field specifies the date from which the custodial parent requests 
  /// withholding begin.  This is a mandatory Federal Requirement E-2.b.3 which
  /// states that for cases not subject to immediate withholding, the system
  /// must automatically initiate withholding on the earliest of:
  /// 	1. The date on which the non-custodial parent fails to make payments in 
  /// an amount equal to the support payable for one ;month;
  /// 	2. The date on which the non-custodial parent requests witholding begin;
  /// 	3. The date on which the custodial parent requests withholding begin if 
  /// the state approves the request;
  /// This date will be entered on the LEGAL_ACTION which specifies the support 
  /// order
  /// </summary>
  [JsonPropertyName("dateCpReqIwoBegin")]
  [Member(Index = 39, Type = MemberType.Date, Optional = true)]
  public DateTime? DateCpReqIwoBegin
  {
    get => Get<DateTime?>("dateCpReqIwoBegin");
    set => Set("dateCpReqIwoBegin", value);
  }

  /// <summary>
  /// The value of the DATE_NON_CP_REQ_IWO_BEGIN attribute.
  /// This field specifies the date from which the non-custodial parent requests
  /// withholding begin.  This is a mandatory Federal Requirement E-2.b.3 which
  /// states that for cases not subject to immediate withholding, the system
  /// must automatically initiate withholding on the earliest of:
  /// 	1. The date on which the non-custodial parent fails to make payments in 
  /// an amount equal to the support payable for one ;month;
  /// 	2. The date on which the non-custodial parent requests witholding begin;
  /// 	3. The date on which the custodial parent requests withholding begin if 
  /// the state approves the request;
  /// This date will be entered on the LEGAL_ACTION which specifies the support 
  /// order
  /// </summary>
  [JsonPropertyName("dateNonCpReqIwoBegin")]
  [Member(Index = 40, Type = MemberType.Date, Optional = true)]
  public DateTime? DateNonCpReqIwoBegin
  {
    get => Get<DateTime?>("dateNonCpReqIwoBegin");
    set => Set("dateNonCpReqIwoBegin", value);
  }

  /// <summary>Length of the CT_ORD_ALT_BILLING_ADDR_IND attribute.</summary>
  public const int CtOrdAltBillingAddrInd_MaxLength = 1;

  /// <summary>
  /// The value of the CT_ORD_ALT_BILLING_ADDR_IND attribute.
  /// This attribute indicates whether the alternate billing address associated 
  /// with the Legal Action was specified in the Court Order or not.
  /// 	Y - The alternate billing address was 			specified by the court order.
  /// 	N - The alternate billing address was 			not specified by the court 			
  /// order.
  /// </summary>
  [JsonPropertyName("ctOrdAltBillingAddrInd")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = CtOrdAltBillingAddrInd_MaxLength, Optional = true)]
  public string CtOrdAltBillingAddrInd
  {
    get => Get<string>("ctOrdAltBillingAddrInd");
    set => Set(
      "ctOrdAltBillingAddrInd", TrimEnd(Substring(value, 1,
      CtOrdAltBillingAddrInd_MaxLength)));
  }

  /// <summary>Length of the INITIATING_COUNTRY attribute.</summary>
  public const int InitiatingCountry_MaxLength = 2;

  /// <summary>
  /// The value of the INITIATING_COUNTRY attribute.
  /// The country in which a URESA/UIFSA proceeding is commenced and where the 
  /// obligee is located. The valid values are maintained in the CODE VALUE
  /// table for the code name COUNTRY CODE.
  /// </summary>
  [JsonPropertyName("initiatingCountry")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = InitiatingCountry_MaxLength, Optional = true)]
  public string InitiatingCountry
  {
    get => Get<string>("initiatingCountry");
    set => Set(
      "initiatingCountry", TrimEnd(Substring(value, 1,
      InitiatingCountry_MaxLength)));
  }

  /// <summary>Length of the RESPONDING_COUNTRY attribute.</summary>
  public const int RespondingCountry_MaxLength = 2;

  /// <summary>
  /// The value of the RESPONDING_COUNTRY attribute.
  /// The country receiving and acting on an interstate child support case. The 
  /// valid values are maintained in the CODE VALUE table for the code name
  /// COUNTRY CODE.
  /// </summary>
  [JsonPropertyName("respondingCountry")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = RespondingCountry_MaxLength, Optional = true)]
  public string RespondingCountry
  {
    get => Get<string>("respondingCountry");
    set => Set(
      "respondingCountry", TrimEnd(Substring(value, 1,
      RespondingCountry_MaxLength)));
  }

  /// <summary>Length of the KPC_STANDARD_NO attribute.</summary>
  public const int KpcStandardNo_MaxLength = 20;

  /// <summary>
  /// The value of the KPC_STANDARD_NO attribute.
  /// This field will be used to store the legal action standard no KPC picks up
  /// when the batch SWEF691B ran.
  /// </summary>
  [JsonPropertyName("kpcStandardNo")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = KpcStandardNo_MaxLength, Optional = true)]
  public string KpcStandardNo
  {
    get => Get<string>("kpcStandardNo");
    set => Set(
      "kpcStandardNo", TrimEnd(Substring(value, 1, KpcStandardNo_MaxLength)));
  }

  /// <summary>Length of the KPC_FLAG attribute.</summary>
  public const int KpcFlag_MaxLength = 1;

  /// <summary>
  /// The value of the KPC_FLAG attribute.
  /// This flag is used to check if the legal action is picked up by the batch 
  /// program 'Fn_B691_KPC_Court_Order_Extract' (SWEF691B). The Flag will be set
  /// to 'Y' if picked by the batch else set to 'N'.
  /// </summary>
  [JsonPropertyName("kpcFlag")]
  [Member(Index = 45, Type = MemberType.Char, Length = KpcFlag_MaxLength, Optional
    = true)]
  public string KpcFlag
  {
    get => Get<string>("kpcFlag");
    set => Set("kpcFlag", TrimEnd(Substring(value, 1, KpcFlag_MaxLength)));
  }

  /// <summary>
  /// The value of the KPC_DATE attribute.
  /// This field will store the Date when the legal action is picked up by the 
  /// batch program 'Fn_B691_KPC_Court_Order_Extract' (SWEF691B). The date will
  /// be set to the date when Legal Action is picked by the batch program.
  /// </summary>
  [JsonPropertyName("kpcDate")]
  [Member(Index = 46, Type = MemberType.Date, Optional = true)]
  public DateTime? KpcDate
  {
    get => Get<DateTime?>("kpcDate");
    set => Set("kpcDate", value);
  }

  /// <summary>Length of the KPC_STD_NO_CHG_FLAG attribute.</summary>
  public const int KpcStdNoChgFlag_MaxLength = 1;

  /// <summary>
  /// The value of the KPC_STD_NO_CHG_FLAG attribute.
  /// This flag is used to check if kpc_std_no has changed. This is created for 
  /// future use. Initially this will be set to N.
  /// </summary>
  [JsonPropertyName("kpcStdNoChgFlag")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = KpcStdNoChgFlag_MaxLength, Optional = true)]
  public string KpcStdNoChgFlag
  {
    get => Get<string>("kpcStdNoChgFlag");
    set => Set(
      "kpcStdNoChgFlag",
      TrimEnd(Substring(value, 1, KpcStdNoChgFlag_MaxLength)));
  }

  /// <summary>
  /// The value of the KEY_CHANGE_DATE attribute.
  /// The date that the legal action's tribunal, court order number, or standard
  /// number were last changed on LACC.
  /// </summary>
  [JsonPropertyName("keyChangeDate")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? KeyChangeDate
  {
    get => Get<DateTime?>("keyChangeDate");
    set => Set("keyChangeDate", value);
  }

  /// <summary>
  /// The value of the FILED_DT_ENTRED_ON attribute.
  /// This is the date on which filed_date was entered. Required for reporting.
  /// </summary>
  [JsonPropertyName("filedDtEntredOn")]
  [Member(Index = 49, Type = MemberType.Date, Optional = true)]
  public DateTime? FiledDtEntredOn
  {
    get => Get<DateTime?>("filedDtEntredOn");
    set => Set("filedDtEntredOn", value);
  }

  /// <summary>Length of the SYSTEM_GEN_IND attribute.</summary>
  public const int SystemGenInd_MaxLength = 1;

  /// <summary>
  /// The value of the SYSTEM_GEN_IND attribute.
  /// Indicates whether a legal action was created automatically by the stystem 
  /// in response to some other action(e.g.auto IWO). 'Y' indicates the legal
  /// action was system generated and blank indicates that the legal action was
  /// manually created by a user.
  /// </summary>
  [JsonPropertyName("systemGenInd")]
  [Member(Index = 50, Type = MemberType.Char, Length = SystemGenInd_MaxLength, Optional
    = true)]
  public string SystemGenInd
  {
    get => Get<string>("systemGenInd");
    set => Set(
      "systemGenInd", TrimEnd(Substring(value, 1, SystemGenInd_MaxLength)));
  }

  /// <summary>
  /// The value of the KPC_TRIBUNAL_ID attribute.
  /// Contains the tribunal id associated to the legal action the last time the 
  /// court order information was reported to the KPC . This information will be
  /// needed for reporting any future key changes to the KPC.
  /// </summary>
  [JsonPropertyName("kpcTribunalId")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcTribunalId
  {
    get => Get<int?>("kpcTribunalId");
    set => Set("kpcTribunalId", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 52, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => Get<string>("cspNumber");
    set => Set("cspNumber", TrimEnd(Substring(value, 1, CspNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute uniquely identifies a tribunal record. It is automatically 
  /// generated by the system starting from 1.
  /// </summary>
  [JsonPropertyName("trbId")]
  [Member(Index = 53, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TrbId
  {
    get => Get<int?>("trbId");
    set => Set("trbId", value);
  }
}

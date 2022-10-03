// The source file: CASH_RECEIPT_DETAIL, ID: 371431516, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This is the receipt information for each item received in a Cash Receipt.
/// </summary>
[Serializable]
public partial class CashReceiptDetail: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptDetail(CashReceiptDetail that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the COLLECTION_AMT_FULLY_APPLIED_IND attribute.
  /// </summary>
  public const int CollectionAmtFullyAppliedInd_MaxLength = 1;

  /// <summary>
  /// The value of the COLLECTION_AMT_FULLY_APPLIED_IND attribute.
  /// A value of 'Y' indicates that the entire collection amount of this cash 
  /// receipt detail has been either distributed or refunded and that no amount
  /// is sitting in suspense.          Permitted values of Y and space.
  /// </summary>
  [JsonPropertyName("collectionAmtFullyAppliedInd")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = CollectionAmtFullyAppliedInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("Y")]
  [Value("")]
  public string CollectionAmtFullyAppliedInd
  {
    get => Get<string>("collectionAmtFullyAppliedInd");
    set => Set(
      "collectionAmtFullyAppliedInd", TrimEnd(Substring(value, 1,
      CollectionAmtFullyAppliedInd_MaxLength)));
  }

  /// <summary>Length of the INTERFACE_TRANS_ID attribute.</summary>
  public const int InterfaceTransId_MaxLength = 14;

  /// <summary>
  /// The value of the INTERFACE_TRANS_ID attribute.
  /// For interfaced collection sources each detail record will have a 
  /// transaction id that is assigned by the transmitting source.
  /// </summary>
  [JsonPropertyName("interfaceTransId")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = InterfaceTransId_MaxLength, Optional = true)]
  public string InterfaceTransId
  {
    get => Get<string>("interfaceTransId");
    set => Set(
      "interfaceTransId",
      TrimEnd(Substring(value, 1, InterfaceTransId_MaxLength)));
  }

  /// <summary>Length of the ADJUSTMENT_IND attribute.</summary>
  public const int AdjustmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADJUSTMENT_IND attribute.
  /// An indicator which if marked Y says that this cash receipt detail is an 
  /// adjustment type to a another cash receipt detail.  Eventually this
  /// adjustment type should be related via a cash receipt adjustment relation
  /// to the original cash receipt detail.
  /// </summary>
  [JsonPropertyName("adjustmentInd")]
  [Member(Index = 3, Type = MemberType.Char, Length = AdjustmentInd_MaxLength, Optional
    = true)]
  public string AdjustmentInd
  {
    get => Get<string>("adjustmentInd");
    set => Set(
      "adjustmentInd", TrimEnd(Substring(value, 1, AdjustmentInd_MaxLength)));
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("sequentialIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 4)]
  public int SequentialIdentifier
  {
    get => Get<int?>("sequentialIdentifier") ?? 0;
    set => Set("sequentialIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// The unique identifier assigned to the court order by the court.
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => Get<string>("courtOrderNumber");
    set => Set(
      "courtOrderNumber",
      TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)));
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the case 
  /// number.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => Get<string>("caseNumber");
    set =>
      Set("caseNumber", TrimEnd(Substring(value, 1, CaseNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the OFFSET_TAXID attribute.
  /// The business taxid (EIN) or SSN for the organization or person receiving 
  /// the refund.
  /// </summary>
  [JsonPropertyName("offsetTaxid")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OffsetTaxid
  {
    get => Get<int?>("offsetTaxid");
    set => Set("offsetTaxid", value);
  }

  /// <summary>
  /// The value of the RECEIVED_AMOUNT attribute.
  /// The amount of money that was receipted in U.S. dollars.
  /// </summary>
  [JsonPropertyName("receivedAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ReceivedAmount
  {
    get => Get<decimal?>("receivedAmount") ?? 0M;
    set => Set(
      "receivedAmount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// The amount paid by the obligor. The obligor  will receive distribution 
  /// credit for this amount.  CSE will receive credit for collecting this
  /// amount.
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal CollectionAmount
  {
    get => Get<decimal?>("collectionAmount") ?? 0M;
    set => Set(
      "collectionAmount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// The date that the money is payed on behalf of the obligor.  If the money 
  /// is paid thru the court it is the date that the court receives the money.
  /// If the money is paid thru income withholding it is the date that the
  /// employer retains the money.
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? CollectionDate
  {
    get => Get<DateTime?>("collectionDate");
    set => Set("collectionDate", value);
  }

  /// <summary>Length of the MULTI_PAYOR attribute.</summary>
  public const int MultiPayor_MaxLength = 1;

  /// <summary>
  /// The value of the MULTI_PAYOR attribute.
  /// An indicator to show that a cash receipt detail was from the mother or the
  /// father.  This tells us who the collection was from when the court order
  /// covers mulitple obligators.
  /// </summary>
  [JsonPropertyName("multiPayor")]
  [Member(Index = 11, Type = MemberType.Char, Length = MultiPayor_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("M")]
  [Value("F")]
  [Value("")]
  [ImplicitValue("F")]
  public string MultiPayor
  {
    get => Get<string>("multiPayor");
    set =>
      Set("multiPayor", TrimEnd(Substring(value, 1, MultiPayor_MaxLength)));
  }

  /// <summary>
  /// The value of the OFFSET_TAX_YEAR attribute.
  /// The tax year of the tax return that was offset (garnished) by the IRS to 
  /// pay CSE debt.
  /// </summary>
  [JsonPropertyName("offsetTaxYear")]
  [Member(Index = 12, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffsetTaxYear
  {
    get => Get<int?>("offsetTaxYear");
    set => Set("offsetTaxYear", value);
  }

  /// <summary>Length of the JOINT_RETURN_IND attribute.</summary>
  public const int JointReturnInd_MaxLength = 1;

  /// <summary>
  /// The value of the JOINT_RETURN_IND attribute.
  /// This indicator sets the status as either a joint return or not.
  /// </summary>
  [JsonPropertyName("jointReturnInd")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = JointReturnInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [Value("")]
  [ImplicitValue("N")]
  public string JointReturnInd
  {
    get => Get<string>("jointReturnInd");
    set => Set(
      "jointReturnInd", TrimEnd(Substring(value, 1, JointReturnInd_MaxLength)));
      
  }

  /// <summary>Length of the JOINT_RETURN_NAME attribute.</summary>
  public const int JointReturnName_MaxLength = 60;

  /// <summary>
  /// The value of the JOINT_RETURN_NAME attribute.
  /// The name of both parties on a joint return.
  /// </summary>
  [JsonPropertyName("jointReturnName")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = JointReturnName_MaxLength, Optional = true)]
  public string JointReturnName
  {
    get => Get<string>("jointReturnName");
    set => Set(
      "jointReturnName",
      TrimEnd(Substring(value, 1, JointReturnName_MaxLength)));
  }

  /// <summary>Length of the DEFAULTED_COLLECTION_DATE_IND attribute.</summary>
  public const int DefaultedCollectionDateInd_MaxLength = 1;

  /// <summary>
  /// The value of the DEFAULTED_COLLECTION_DATE_IND attribute.
  /// Is used to indicate that the collection date was defaulted to the current 
  /// date as opposed to being entered by the user.
  /// </summary>
  [JsonPropertyName("defaultedCollectionDateInd")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = DefaultedCollectionDateInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("Y")]
  public string DefaultedCollectionDateInd
  {
    get => Get<string>("defaultedCollectionDateInd");
    set => Set(
      "defaultedCollectionDateInd", TrimEnd(Substring(value, 1,
      DefaultedCollectionDateInd_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_PERSON_NUMBER attribute.</summary>
  public const int ObligorPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_PERSON_NUMBER attribute.
  /// The unique number assigned to the obligor.
  /// 	
  /// </summary>
  [JsonPropertyName("obligorPersonNumber")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ObligorPersonNumber_MaxLength, Optional = true)]
  public string ObligorPersonNumber
  {
    get => Get<string>("obligorPersonNumber");
    set => Set(
      "obligorPersonNumber", TrimEnd(Substring(value, 1,
      ObligorPersonNumber_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int ObligorSocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the OBLIGOR_SOCIAL_SECURITY_NUMBER attribute.
  /// The unique number assigned to the obligor by the Social Security 
  /// Administration.
  /// </summary>
  [JsonPropertyName("obligorSocialSecurityNumber")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ObligorSocialSecurityNumber_MaxLength, Optional = true)]
  public string ObligorSocialSecurityNumber
  {
    get => Get<string>("obligorSocialSecurityNumber");
    set => Set(
      "obligorSocialSecurityNumber", TrimEnd(Substring(value, 1,
      ObligorSocialSecurityNumber_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_FIRST_NAME attribute.</summary>
  public const int ObligorFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the OBLIGOR_FIRST_NAME attribute.
  /// The first name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorFirstName")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = ObligorFirstName_MaxLength, Optional = true)]
  public string ObligorFirstName
  {
    get => Get<string>("obligorFirstName");
    set => Set(
      "obligorFirstName",
      TrimEnd(Substring(value, 1, ObligorFirstName_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_LAST_NAME attribute.</summary>
  public const int ObligorLastName_MaxLength = 30;

  /// <summary>
  /// The value of the OBLIGOR_LAST_NAME attribute.
  /// The last name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorLastName")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = ObligorLastName_MaxLength, Optional = true)]
  public string ObligorLastName
  {
    get => Get<string>("obligorLastName");
    set => Set(
      "obligorLastName",
      TrimEnd(Substring(value, 1, ObligorLastName_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_MIDDLE_NAME attribute.</summary>
  public const int ObligorMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the OBLIGOR_MIDDLE_NAME attribute.
  /// The middle name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorMiddleName")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ObligorMiddleName_MaxLength, Optional = true)]
  public string ObligorMiddleName
  {
    get => Get<string>("obligorMiddleName");
    set => Set(
      "obligorMiddleName", TrimEnd(Substring(value, 1,
      ObligorMiddleName_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_PHONE_NUMBER attribute.</summary>
  public const int ObligorPhoneNumber_MaxLength = 12;

  /// <summary>
  /// The value of the OBLIGOR_PHONE_NUMBER attribute.
  /// A 10 digit phone number.
  /// </summary>
  [JsonPropertyName("obligorPhoneNumber")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ObligorPhoneNumber_MaxLength, Optional = true)]
  public string ObligorPhoneNumber
  {
    get => Get<string>("obligorPhoneNumber");
    set => Set(
      "obligorPhoneNumber", TrimEnd(Substring(value, 1,
      ObligorPhoneNumber_MaxLength)));
  }

  /// <summary>Length of the PAYEE_FIRST_NAME attribute.</summary>
  public const int PayeeFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYEE_FIRST_NAME attribute.
  /// The first name of the person who is to receive the money.
  /// </summary>
  [JsonPropertyName("payeeFirstName")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = PayeeFirstName_MaxLength, Optional = true)]
  public string PayeeFirstName
  {
    get => Get<string>("payeeFirstName");
    set => Set(
      "payeeFirstName", TrimEnd(Substring(value, 1, PayeeFirstName_MaxLength)));
      
  }

  /// <summary>Length of the PAYEE_MIDDLE_NAME attribute.</summary>
  public const int PayeeMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYEE_MIDDLE_NAME attribute.
  /// The middle name of the person who is to receive the money.
  /// </summary>
  [JsonPropertyName("payeeMiddleName")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = PayeeMiddleName_MaxLength, Optional = true)]
  public string PayeeMiddleName
  {
    get => Get<string>("payeeMiddleName");
    set => Set(
      "payeeMiddleName",
      TrimEnd(Substring(value, 1, PayeeMiddleName_MaxLength)));
  }

  /// <summary>Length of the PAYEE_LAST_NAME attribute.</summary>
  public const int PayeeLastName_MaxLength = 30;

  /// <summary>
  /// The value of the PAYEE_LAST_NAME attribute.
  /// The last name of the who is to receive the money.
  /// </summary>
  [JsonPropertyName("payeeLastName")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = PayeeLastName_MaxLength, Optional = true)]
  public string PayeeLastName
  {
    get => Get<string>("payeeLastName");
    set => Set(
      "payeeLastName", TrimEnd(Substring(value, 1, PayeeLastName_MaxLength)));
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_FIRST_NAME attribute.</summary>
  public const int Attribute1SupportedPersonFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_FIRST_NAME attribute.
  /// The first name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonFirstName")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = Attribute1SupportedPersonFirstName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonFirstName
  {
    get => Get<string>("attribute1SupportedPersonFirstName");
    set => Set(
      "attribute1SupportedPersonFirstName", TrimEnd(Substring(value, 1,
      Attribute1SupportedPersonFirstName_MaxLength)));
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_MIDDLE_NAME attribute.</summary>
  public const int Attribute1SupportedPersonMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_MIDDLE_NAME attribute.
  /// The middle name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonMiddleName")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = Attribute1SupportedPersonMiddleName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonMiddleName
  {
    get => Get<string>("attribute1SupportedPersonMiddleName");
    set => Set(
      "attribute1SupportedPersonMiddleName", TrimEnd(Substring(value, 1,
      Attribute1SupportedPersonMiddleName_MaxLength)));
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_LAST_NAME attribute.</summary>
  public const int Attribute1SupportedPersonLastName_MaxLength = 30;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_LAST_NAME attribute.
  /// The last name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonLastName")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = Attribute1SupportedPersonLastName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonLastName
  {
    get => Get<string>("attribute1SupportedPersonLastName");
    set => Set(
      "attribute1SupportedPersonLastName", TrimEnd(Substring(value, 1,
      Attribute1SupportedPersonLastName_MaxLength)));
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_FIRST_NAME attribute.</summary>
  public const int Attribute2SupportedPersonFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_FIRST_NAME attribute.
  /// The first name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonFirstName")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = Attribute2SupportedPersonFirstName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonFirstName
  {
    get => Get<string>("attribute2SupportedPersonFirstName");
    set => Set(
      "attribute2SupportedPersonFirstName", TrimEnd(Substring(value, 1,
      Attribute2SupportedPersonFirstName_MaxLength)));
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_LAST_NAME attribute.</summary>
  public const int Attribute2SupportedPersonLastName_MaxLength = 20;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_LAST_NAME attribute.
  /// The middle name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonLastName")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = Attribute2SupportedPersonLastName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonLastName
  {
    get => Get<string>("attribute2SupportedPersonLastName");
    set => Set(
      "attribute2SupportedPersonLastName", TrimEnd(Substring(value, 1,
      Attribute2SupportedPersonLastName_MaxLength)));
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_MIDDLE_NAME attribute.</summary>
  public const int Attribute2SupportedPersonMiddleName_MaxLength = 30;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_MIDDLE_NAME attribute.
  /// The last name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonMiddleName")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = Attribute2SupportedPersonMiddleName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonMiddleName
  {
    get => Get<string>("attribute2SupportedPersonMiddleName");
    set => Set(
      "attribute2SupportedPersonMiddleName", TrimEnd(Substring(value, 1,
      Attribute2SupportedPersonMiddleName_MaxLength)));
  }

  /// <summary>Length of the REFERENCE attribute.</summary>
  public const int Reference_MaxLength = 240;

  /// <summary>
  /// The value of the REFERENCE attribute.
  /// A number from the check that may be used to help identify the cash receipt
  /// detail item.
  /// </summary>
  [JsonPropertyName("reference")]
  [Member(Index = 31, Type = MemberType.Varchar, Length = Reference_MaxLength, Optional
    = true)]
  public string Reference
  {
    get => Get<string>("reference");
    set => Set("reference", Substring(value, 1, Reference_MaxLength));
  }

  /// <summary>Length of the NOTES attribute.</summary>
  public const int Notes_MaxLength = 240;

  /// <summary>
  /// The value of the NOTES attribute.
  /// An area for entered any additional information.
  /// </summary>
  [JsonPropertyName("notes")]
  [Member(Index = 32, Type = MemberType.Varchar, Length = Notes_MaxLength, Optional
    = true)]
  public string Notes
  {
    get => Get<string>("notes");
    set => Set("notes", Substring(value, 1, Notes_MaxLength));
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
  [Member(Index = 33, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 34, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => Get<DateTime?>("createdTmst");
    set => Set("createdTmst", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 36, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => Get<DateTime?>("lastUpdatedTmst");
    set => Set("lastUpdatedTmst", value);
  }

  /// <summary>
  /// The value of the REFUNDED_AMOUNT attribute.
  /// The amount that has been refunded and will not usually be equal to the 
  /// collected amount.	
  /// </summary>
  [JsonPropertyName("refundedAmount")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? RefundedAmount
  {
    get => Get<decimal?>("refundedAmount");
    set => Set("refundedAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the DISTRIBUTED_AMOUNT attribute.
  /// The amount that has been distributed and may not equal the collected 
  /// amount.
  /// </summary>
  [JsonPropertyName("distributedAmount")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? DistributedAmount
  {
    get => Get<decimal?>("distributedAmount");
    set => Set("distributedAmount", Truncate(value, 2));
  }

  /// <summary>Length of the SUPP_PERSON_NO_FOR_VOL attribute.</summary>
  public const int SuppPersonNoForVol_MaxLength = 10;

  /// <summary>
  /// The value of the SUPP_PERSON_NO_FOR_VOL attribute.
  /// This attribute specifies the supported cse person number for whom the 
  /// voluntary payment is made.
  /// </summary>
  [JsonPropertyName("suppPersonNoForVol")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = SuppPersonNoForVol_MaxLength, Optional = true)]
  public string SuppPersonNoForVol
  {
    get => Get<string>("suppPersonNoForVol");
    set => Set(
      "suppPersonNoForVol", TrimEnd(Substring(value, 1,
      SuppPersonNoForVol_MaxLength)));
  }

  /// <summary>Length of the OVERRIDE_MANUAL_DIST_IND attribute.</summary>
  public const int OverrideManualDistInd_MaxLength = 1;

  /// <summary>
  /// The value of the OVERRIDE_MANUAL_DIST_IND attribute.
  /// RESP: KESSEP
  /// 
  /// The purpose of this indicator is to allow Automatic Distribution to
  /// override the Manual Distribution Instructions when determining whether
  /// or not a Cash Receipt Detail should be automatically distributed.  If
  /// the value is a space (no override), then Automatic Distribution will
  /// determine whether or not to distribute any of the Cash Receipt Detail
  /// based on the existence of any active Manual Distribution Instructions.
  /// If the value is a &quot;Y&quot; (override), the Automatic Distribution
  /// will distribute the Cash Receipt Detail without regard to the existence
  /// of any active Manual Distribution Instructions.
  /// </summary>
  [JsonPropertyName("overrideManualDistInd")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = OverrideManualDistInd_MaxLength, Optional = true)]
  public string OverrideManualDistInd
  {
    get => Get<string>("overrideManualDistInd");
    set => Set(
      "overrideManualDistInd", TrimEnd(Substring(value, 1,
      OverrideManualDistInd_MaxLength)));
  }

  /// <summary>Length of the INJURED_SPOUSE_IND attribute.</summary>
  public const int InjuredSpouseInd_MaxLength = 1;

  /// <summary>
  /// The value of the INJURED_SPOUSE_IND attribute.
  /// The indicator received from the feds that a spouse has been injured 
  /// through the federal offset.
  /// </summary>
  [JsonPropertyName("injuredSpouseInd")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = InjuredSpouseInd_MaxLength, Optional = true)]
  public string InjuredSpouseInd
  {
    get => Get<string>("injuredSpouseInd");
    set => Set(
      "injuredSpouseInd",
      TrimEnd(Substring(value, 1, InjuredSpouseInd_MaxLength)));
  }

  /// <summary>
  /// The value of the JFA_RECEIVED_DATE attribute.
  /// The date the JFA was received
  /// </summary>
  [JsonPropertyName("jfaReceivedDate")]
  [Member(Index = 42, Type = MemberType.Date, Optional = true)]
  public DateTime? JfaReceivedDate
  {
    get => Get<DateTime?>("jfaReceivedDate");
    set => Set("jfaReceivedDate", value);
  }

  /// <summary>
  /// The value of the CRU_PROCESSED_DATE attribute.
  /// The date the CRU was processed
  /// </summary>
  [JsonPropertyName("cruProcessedDate")]
  [Member(Index = 43, Type = MemberType.Date, Optional = true)]
  public DateTime? CruProcessedDate
  {
    get => Get<DateTime?>("cruProcessedDate");
    set => Set("cruProcessedDate", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 44, Type = MemberType.Number, Length = 3)]
  public int CrtIdentifier
  {
    get => Get<int?>("crtIdentifier") ?? 0;
    set => Set("crtIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 45, Type = MemberType.Number, Length = 3)]
  public int CstIdentifier
  {
    get => Get<int?>("cstIdentifier") ?? 0;
    set => Set("cstIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 46, Type = MemberType.Number, Length = 9)]
  public int CrvIdentifier
  {
    get => Get<int?>("crvIdentifier") ?? 0;
    set => Set("crvIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cltIdentifier")]
  [Member(Index = 47, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CltIdentifier
  {
    get => Get<int?>("cltIdentifier");
    set => Set("cltIdentifier", value);
  }
}

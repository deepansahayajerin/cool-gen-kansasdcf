// The source file: CASH_RECEIPT_DETAIL_HISTORY, ID: 371431781, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity type logs all changes made to the cash receipt detail entity 
/// type.
/// </summary>
[Serializable]
public partial class CashReceiptDetailHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptDetailHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptDetailHistory(CashReceiptDetailHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptDetailHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptDetailHistory that)
  {
    base.Assign(that);
    collectionAmtFullyAppliedInd = that.collectionAmtFullyAppliedInd;
    interfaceTransId = that.interfaceTransId;
    offsetTaxid = that.offsetTaxid;
    jointReturnInd = that.jointReturnInd;
    jointReturnName = that.jointReturnName;
    refundedAmount = that.refundedAmount;
    distributedAmount = that.distributedAmount;
    adjustmentInd = that.adjustmentInd;
    sequentialIdentifier = that.sequentialIdentifier;
    attribute2SupportedPersonFirstName =
      that.attribute2SupportedPersonFirstName;
    attribute2SupportedPersonLastName = that.attribute2SupportedPersonLastName;
    attribute2SupportedPersonMiddleName =
      that.attribute2SupportedPersonMiddleName;
    collectionTypeIdentifier = that.collectionTypeIdentifier;
    cashReceiptEventNumber = that.cashReceiptEventNumber;
    cashReceiptNumber = that.cashReceiptNumber;
    collectionDate = that.collectionDate;
    obligorPersonNumber = that.obligorPersonNumber;
    courtOrderNumber = that.courtOrderNumber;
    caseNumber = that.caseNumber;
    obligorFirstName = that.obligorFirstName;
    obligorLastName = that.obligorLastName;
    obligorMiddleName = that.obligorMiddleName;
    obligorPhoneNumber = that.obligorPhoneNumber;
    obligorSocialSecurityNumber = that.obligorSocialSecurityNumber;
    offsetTaxYear = that.offsetTaxYear;
    defaultedCollectionDateInd = that.defaultedCollectionDateInd;
    multiPayor = that.multiPayor;
    receivedAmount = that.receivedAmount;
    collectionAmount = that.collectionAmount;
    payeeFirstName = that.payeeFirstName;
    payeeMiddleName = that.payeeMiddleName;
    payeeLastName = that.payeeLastName;
    attribute1SupportedPersonFirstName =
      that.attribute1SupportedPersonFirstName;
    attribute1SupportedPersonMiddleName =
      that.attribute1SupportedPersonMiddleName;
    attribute1SupportedPersonLastName = that.attribute1SupportedPersonLastName;
    reference = that.reference;
    notes = that.notes;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    cashReceiptType = that.cashReceiptType;
    cashReceiptSourceType = that.cashReceiptSourceType;
  }

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
  [Value("")]
  [Value("Y")]
  public string CollectionAmtFullyAppliedInd
  {
    get => collectionAmtFullyAppliedInd;
    set => collectionAmtFullyAppliedInd = value != null
      ? TrimEnd(Substring(value, 1, CollectionAmtFullyAppliedInd_MaxLength)) : null
      ;
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
    get => interfaceTransId;
    set => interfaceTransId = value != null
      ? TrimEnd(Substring(value, 1, InterfaceTransId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the OFFSET_TAXID attribute.
  /// The business taxid (EIN) or SSN for the organization or person receiving 
  /// the refund.
  /// </summary>
  [JsonPropertyName("offsetTaxid")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OffsetTaxid
  {
    get => offsetTaxid;
    set => offsetTaxid = value;
  }

  /// <summary>Length of the JOINT_RETURN_IND attribute.</summary>
  public const int JointReturnInd_MaxLength = 1;

  /// <summary>
  /// The value of the JOINT_RETURN_IND attribute.
  /// This indicator sets the status as either a joint return or not.
  /// </summary>
  [JsonPropertyName("jointReturnInd")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = JointReturnInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("Y")]
  [Value("")]
  [Value("N")]
  [ImplicitValue("N")]
  public string JointReturnInd
  {
    get => jointReturnInd;
    set => jointReturnInd = value != null
      ? TrimEnd(Substring(value, 1, JointReturnInd_MaxLength)) : null;
  }

  /// <summary>Length of the JOINT_RETURN_NAME attribute.</summary>
  public const int JointReturnName_MaxLength = 60;

  /// <summary>
  /// The value of the JOINT_RETURN_NAME attribute.
  /// The name of both parties on a joint return.
  /// </summary>
  [JsonPropertyName("jointReturnName")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = JointReturnName_MaxLength, Optional = true)]
  public string JointReturnName
  {
    get => jointReturnName;
    set => jointReturnName = value != null
      ? TrimEnd(Substring(value, 1, JointReturnName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the REFUNDED_AMOUNT attribute.
  /// The amount that has been refunded and will not usually be equal to the 
  /// collected amount.	
  /// </summary>
  [JsonPropertyName("refundedAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? RefundedAmount
  {
    get => refundedAmount;
    set => refundedAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the DISTRIBUTED_AMOUNT attribute.
  /// The amount that has been distributed and may not equal the collected 
  /// amount.
  /// </summary>
  [JsonPropertyName("distributedAmount")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? DistributedAmount
  {
    get => distributedAmount;
    set => distributedAmount = value != null ? Truncate(value, 2) : null;
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
  [Member(Index = 8, Type = MemberType.Char, Length = AdjustmentInd_MaxLength, Optional
    = true)]
  public string AdjustmentInd
  {
    get => adjustmentInd;
    set => adjustmentInd = value != null
      ? TrimEnd(Substring(value, 1, AdjustmentInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("sequentialIdentifier")]
  [Member(Index = 9, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? SequentialIdentifier
  {
    get => sequentialIdentifier;
    set => sequentialIdentifier = value;
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_FIRST_NAME attribute.</summary>
  public const int Attribute2SupportedPersonFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_FIRST_NAME attribute.
  /// The first name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonFirstName")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = Attribute2SupportedPersonFirstName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonFirstName
  {
    get => attribute2SupportedPersonFirstName;
    set => attribute2SupportedPersonFirstName = value != null
      ? TrimEnd(
        Substring(value, 1, Attribute2SupportedPersonFirstName_MaxLength)) : null
      ;
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_LAST_NAME attribute.</summary>
  public const int Attribute2SupportedPersonLastName_MaxLength = 20;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_LAST_NAME attribute.
  /// The middle name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonLastName")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = Attribute2SupportedPersonLastName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonLastName
  {
    get => attribute2SupportedPersonLastName;
    set => attribute2SupportedPersonLastName = value != null
      ? TrimEnd(Substring(value, 1, Attribute2SupportedPersonLastName_MaxLength))
      : null;
  }

  /// <summary>Length of the 2SUPPORTED_PERSON_MIDDLE_NAME attribute.</summary>
  public const int Attribute2SupportedPersonMiddleName_MaxLength = 30;

  /// <summary>
  /// The value of the 2SUPPORTED_PERSON_MIDDLE_NAME attribute.
  /// The last name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute2SupportedPersonMiddleName")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = Attribute2SupportedPersonMiddleName_MaxLength, Optional = true)]
  public string Attribute2SupportedPersonMiddleName
  {
    get => attribute2SupportedPersonMiddleName;
    set => attribute2SupportedPersonMiddleName = value != null
      ? TrimEnd(Substring(
        value, 1, Attribute2SupportedPersonMiddleName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COLLECTION_TYPE_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("collectionTypeIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
  public int CollectionTypeIdentifier
  {
    get => collectionTypeIdentifier;
    set => collectionTypeIdentifier = value;
  }

  /// <summary>
  /// The value of the CASH_RECEIPT_EVENT_NUMBER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("cashReceiptEventNumber")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 9)]
  public int CashReceiptEventNumber
  {
    get => cashReceiptEventNumber;
    set => cashReceiptEventNumber = value;
  }

  /// <summary>
  /// The value of the CASH_RECEIPT_NUMBER attribute.
  /// A unique sequential number that distinguishes a check receipt from all 
  /// other check receipts.
  /// </summary>
  [JsonPropertyName("cashReceiptNumber")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 9)]
  public int CashReceiptNumber
  {
    get => cashReceiptNumber;
    set => cashReceiptNumber = value;
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// The date that the money is payed on behalf of the obligor.  If the money 
  /// is paid thru the court it is the date that the court receives the money.
  /// If the money is paid thru income withholding it is the date that the
  /// employer retains the money.
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionDate
  {
    get => collectionDate;
    set => collectionDate = value;
  }

  /// <summary>Length of the OBLIGOR_PERSON_NUMBER attribute.</summary>
  public const int ObligorPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_PERSON_NUMBER attribute.
  /// Represent the Obligor's person number.
  /// </summary>
  [JsonPropertyName("obligorPersonNumber")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ObligorPersonNumber_MaxLength, Optional = true)]
  public string ObligorPersonNumber
  {
    get => obligorPersonNumber;
    set => obligorPersonNumber = value != null
      ? TrimEnd(Substring(value, 1, ObligorPersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// The unique identifier assigned to the court order by the court.
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => courtOrderNumber;
    set => courtOrderNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the case 
  /// number.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 19, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_FIRST_NAME attribute.</summary>
  public const int ObligorFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the OBLIGOR_FIRST_NAME attribute.
  /// The first name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorFirstName")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = ObligorFirstName_MaxLength, Optional = true)]
  public string ObligorFirstName
  {
    get => obligorFirstName;
    set => obligorFirstName = value != null
      ? TrimEnd(Substring(value, 1, ObligorFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_LAST_NAME attribute.</summary>
  public const int ObligorLastName_MaxLength = 30;

  /// <summary>
  /// The value of the OBLIGOR_LAST_NAME attribute.
  /// The last name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorLastName")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = ObligorLastName_MaxLength, Optional = true)]
  public string ObligorLastName
  {
    get => obligorLastName;
    set => obligorLastName = value != null
      ? TrimEnd(Substring(value, 1, ObligorLastName_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_MIDDLE_NAME attribute.</summary>
  public const int ObligorMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the OBLIGOR_MIDDLE_NAME attribute.
  /// The middle name of the obligor.
  /// </summary>
  [JsonPropertyName("obligorMiddleName")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ObligorMiddleName_MaxLength, Optional = true)]
  public string ObligorMiddleName
  {
    get => obligorMiddleName;
    set => obligorMiddleName = value != null
      ? TrimEnd(Substring(value, 1, ObligorMiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_PHONE_NUMBER attribute.</summary>
  public const int ObligorPhoneNumber_MaxLength = 12;

  /// <summary>
  /// The value of the OBLIGOR_PHONE_NUMBER attribute.
  /// A 10 digit phone number.
  /// </summary>
  [JsonPropertyName("obligorPhoneNumber")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ObligorPhoneNumber_MaxLength, Optional = true)]
  public string ObligorPhoneNumber
  {
    get => obligorPhoneNumber;
    set => obligorPhoneNumber = value != null
      ? TrimEnd(Substring(value, 1, ObligorPhoneNumber_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int ObligorSocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the OBLIGOR_SOCIAL_SECURITY_NUMBER attribute.
  /// The unique number assigned to the obligor by the Social Security 
  /// Administration.
  /// </summary>
  [JsonPropertyName("obligorSocialSecurityNumber")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ObligorSocialSecurityNumber_MaxLength, Optional = true)]
  public string ObligorSocialSecurityNumber
  {
    get => obligorSocialSecurityNumber;
    set => obligorSocialSecurityNumber = value != null
      ? TrimEnd(Substring(value, 1, ObligorSocialSecurityNumber_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the OFFSET_TAX_YEAR attribute.
  /// The tax year of the tax return that was offset (garnished) by the IRS to 
  /// pay CSE debt.
  /// </summary>
  [JsonPropertyName("offsetTaxYear")]
  [Member(Index = 25, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffsetTaxYear
  {
    get => offsetTaxYear;
    set => offsetTaxYear = value;
  }

  /// <summary>Length of the DEFAULTED_COLLECTION_DATE_IND attribute.</summary>
  public const int DefaultedCollectionDateInd_MaxLength = 1;

  /// <summary>
  /// The value of the DEFAULTED_COLLECTION_DATE_IND attribute.
  /// Is used to indicate that the collection date was defaulted to the current 
  /// date as opposed to being entered by the user.
  /// </summary>
  [JsonPropertyName("defaultedCollectionDateInd")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = DefaultedCollectionDateInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("Y")]
  public string DefaultedCollectionDateInd
  {
    get => defaultedCollectionDateInd;
    set => defaultedCollectionDateInd = value != null
      ? TrimEnd(Substring(value, 1, DefaultedCollectionDateInd_MaxLength)) : null
      ;
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
  [Member(Index = 27, Type = MemberType.Char, Length = MultiPayor_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("F")]
  [Value("")]
  [Value("M")]
  [ImplicitValue("F")]
  public string MultiPayor
  {
    get => multiPayor;
    set => multiPayor = value != null
      ? TrimEnd(Substring(value, 1, MultiPayor_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RECEIVED_AMOUNT attribute.
  /// The amount of money that was receipted in U.S. dollars.
  /// </summary>
  [JsonPropertyName("receivedAmount")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ReceivedAmount
  {
    get => receivedAmount;
    set => receivedAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// The amount paid by the obligor. The obligor  will receive distribution 
  /// credit for this amount.  CSE will receive credit for collecting this
  /// amount.
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => collectionAmount;
    set => collectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the PAYEE_FIRST_NAME attribute.</summary>
  public const int PayeeFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYEE_FIRST_NAME attribute.
  /// The first name of the payee.
  /// </summary>
  [JsonPropertyName("payeeFirstName")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = PayeeFirstName_MaxLength, Optional = true)]
  public string PayeeFirstName
  {
    get => payeeFirstName;
    set => payeeFirstName = value != null
      ? TrimEnd(Substring(value, 1, PayeeFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the PAYEE_MIDDLE_NAME attribute.</summary>
  public const int PayeeMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the PAYEE_MIDDLE_NAME attribute.
  /// The middle name of the payee.
  /// </summary>
  [JsonPropertyName("payeeMiddleName")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = PayeeMiddleName_MaxLength, Optional = true)]
  public string PayeeMiddleName
  {
    get => payeeMiddleName;
    set => payeeMiddleName = value != null
      ? TrimEnd(Substring(value, 1, PayeeMiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the PAYEE_LAST_NAME attribute.</summary>
  public const int PayeeLastName_MaxLength = 30;

  /// <summary>
  /// The value of the PAYEE_LAST_NAME attribute.
  /// The last name of the payee.
  /// </summary>
  [JsonPropertyName("payeeLastName")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = PayeeLastName_MaxLength, Optional = true)]
  public string PayeeLastName
  {
    get => payeeLastName;
    set => payeeLastName = value != null
      ? TrimEnd(Substring(value, 1, PayeeLastName_MaxLength)) : null;
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_FIRST_NAME attribute.</summary>
  public const int Attribute1SupportedPersonFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_FIRST_NAME attribute.
  /// The first name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonFirstName")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = Attribute1SupportedPersonFirstName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonFirstName
  {
    get => attribute1SupportedPersonFirstName;
    set => attribute1SupportedPersonFirstName = value != null
      ? TrimEnd(
        Substring(value, 1, Attribute1SupportedPersonFirstName_MaxLength)) : null
      ;
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_MIDDLE_NAME attribute.</summary>
  public const int Attribute1SupportedPersonMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_MIDDLE_NAME attribute.
  /// The middle name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonMiddleName")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = Attribute1SupportedPersonMiddleName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonMiddleName
  {
    get => attribute1SupportedPersonMiddleName;
    set => attribute1SupportedPersonMiddleName = value != null
      ? TrimEnd(Substring(
        value, 1, Attribute1SupportedPersonMiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the 1SUPPORTED_PERSON_LAST_NAME attribute.</summary>
  public const int Attribute1SupportedPersonLastName_MaxLength = 30;

  /// <summary>
  /// The value of the 1SUPPORTED_PERSON_LAST_NAME attribute.
  /// The last name of the supported person.
  /// </summary>
  [JsonPropertyName("attribute1SupportedPersonLastName")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = Attribute1SupportedPersonLastName_MaxLength, Optional = true)]
  public string Attribute1SupportedPersonLastName
  {
    get => attribute1SupportedPersonLastName;
    set => attribute1SupportedPersonLastName = value != null
      ? TrimEnd(Substring(value, 1, Attribute1SupportedPersonLastName_MaxLength))
      : null;
  }

  /// <summary>Length of the REFERENCE attribute.</summary>
  public const int Reference_MaxLength = 240;

  /// <summary>
  /// The value of the REFERENCE attribute.
  /// A number from the check that may be used to help identify the cash receipt
  /// detail item.
  /// </summary>
  [JsonPropertyName("reference")]
  [Member(Index = 36, Type = MemberType.Varchar, Length = Reference_MaxLength, Optional
    = true)]
  public string Reference
  {
    get => reference;
    set => reference = value != null
      ? Substring(value, 1, Reference_MaxLength) : null;
  }

  /// <summary>Length of the NOTES attribute.</summary>
  public const int Notes_MaxLength = 240;

  /// <summary>
  /// The value of the NOTES attribute.
  /// An area for entered any additional information.
  /// </summary>
  [JsonPropertyName("notes")]
  [Member(Index = 37, Type = MemberType.Varchar, Length = Notes_MaxLength, Optional
    = true)]
  public string Notes
  {
    get => notes;
    set => notes = value != null ? Substring(value, 1, Notes_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The userid or program id responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The system time the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 39, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 41, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the CASH_RECEIPT_TYPE attribute.
  /// Cash receipt type identifier.
  /// </summary>
  [JsonPropertyName("cashReceiptType")]
  [DefaultValue(0)]
  [Member(Index = 42, Type = MemberType.Number, Length = 3)]
  public int CashReceiptType
  {
    get => cashReceiptType;
    set => cashReceiptType = value;
  }

  /// <summary>
  /// The value of the CASH_RECEIPT_SOURCE_TYPE attribute.
  /// Cash receipt source type identifier.
  /// </summary>
  [JsonPropertyName("cashReceiptSourceType")]
  [DefaultValue(0)]
  [Member(Index = 43, Type = MemberType.Number, Length = 3)]
  public int CashReceiptSourceType
  {
    get => cashReceiptSourceType;
    set => cashReceiptSourceType = value;
  }

  private string collectionAmtFullyAppliedInd;
  private string interfaceTransId;
  private int? offsetTaxid;
  private string jointReturnInd;
  private string jointReturnName;
  private decimal? refundedAmount;
  private decimal? distributedAmount;
  private string adjustmentInd;
  private int? sequentialIdentifier;
  private string attribute2SupportedPersonFirstName;
  private string attribute2SupportedPersonLastName;
  private string attribute2SupportedPersonMiddleName;
  private int collectionTypeIdentifier;
  private int cashReceiptEventNumber;
  private int cashReceiptNumber;
  private DateTime? collectionDate;
  private string obligorPersonNumber;
  private string courtOrderNumber;
  private string caseNumber;
  private string obligorFirstName;
  private string obligorLastName;
  private string obligorMiddleName;
  private string obligorPhoneNumber;
  private string obligorSocialSecurityNumber;
  private int? offsetTaxYear;
  private string defaultedCollectionDateInd;
  private string multiPayor;
  private decimal? receivedAmount;
  private decimal? collectionAmount;
  private string payeeFirstName;
  private string payeeMiddleName;
  private string payeeLastName;
  private string attribute1SupportedPersonFirstName;
  private string attribute1SupportedPersonMiddleName;
  private string attribute1SupportedPersonLastName;
  private string reference;
  private string notes;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int cashReceiptType;
  private int cashReceiptSourceType;
}

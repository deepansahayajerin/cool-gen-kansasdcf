// The source file: RECEIPT_REFUND, ID: 371440005, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Contains information relevant to a refund of an undistributed cash receipt.
/// It holds the amount being refunded and it may be less than the total
/// receipt.
/// </summary>
[Serializable]
public partial class ReceiptRefund: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ReceiptRefund()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ReceiptRefund(ReceiptRefund that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ReceiptRefund Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ReceiptRefund that)
  {
    base.Assign(that);
    reasonCode = that.reasonCode;
    taxid = that.taxid;
    payeeName = that.payeeName;
    amount = that.amount;
    offsetTaxYear = that.offsetTaxYear;
    requestDate = that.requestDate;
    reasonText = that.reasonText;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    offsetClosed = that.offsetClosed;
    dateTransmitted = that.dateTransmitted;
    taxIdSuffix = that.taxIdSuffix;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    kpcNoticeReqInd = that.kpcNoticeReqInd;
    kpcNoticeProcessedDate = that.kpcNoticeProcessedDate;
    crdIdentifier = that.crdIdentifier;
    crvIdentifier = that.crvIdentifier;
    cstIdentifier = that.cstIdentifier;
    crtIdentifier = that.crtIdentifier;
    cdaIdentifier = that.cdaIdentifier;
    cstAIdentifier = that.cstAIdentifier;
    cspNumber = that.cspNumber;
    ftrIdentifier = that.ftrIdentifier;
    funIdentifier = that.funIdentifier;
    fttIdentifier = that.fttIdentifier;
    pcaEffectiveDt = that.pcaEffectiveDt;
    pcaCode = that.pcaCode;
    cltIdentifier = that.cltIdentifier;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 10;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// This is a coded value from the codes table to indicate as a quick 
  /// reference why this refund has occurred.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
  public string ReasonCode
  {
    get => reasonCode ?? "";
    set => reasonCode = TrimEnd(Substring(value, 1, ReasonCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ReasonCode attribute.</summary>
  [JsonPropertyName("reasonCode")]
  [Computed]
  public string ReasonCode_Json
  {
    get => NullIf(ReasonCode, "");
    set => ReasonCode = value;
  }

  /// <summary>Length of the TAXID attribute.</summary>
  public const int Taxid_MaxLength = 9;

  /// <summary>
  /// The value of the TAXID attribute.
  /// The business taxid (EIN) or SSN for the organization or person receiving 
  /// the refund.
  /// </summary>
  [JsonPropertyName("taxid")]
  [Member(Index = 2, Type = MemberType.Char, Length = Taxid_MaxLength, Optional
    = true)]
  public string Taxid
  {
    get => taxid;
    set => taxid = value != null
      ? TrimEnd(Substring(value, 1, Taxid_MaxLength)) : null;
  }

  /// <summary>Length of the PAYEE_NAME attribute.</summary>
  public const int PayeeName_MaxLength = 60;

  /// <summary>
  /// The value of the PAYEE_NAME attribute.
  /// The organization receiving the refund.
  /// </summary>
  [JsonPropertyName("payeeName")]
  [Member(Index = 3, Type = MemberType.Char, Length = PayeeName_MaxLength, Optional
    = true)]
  public string PayeeName
  {
    get => payeeName;
    set => payeeName = value != null
      ? TrimEnd(Substring(value, 1, PayeeName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// The amount of money being refunded from a particular cash receipt.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the OFFSET_TAX_YEAR attribute.
  /// The tax year of the tax return that was offset (garnished) by the IRS to 
  /// pay CSE debt.
  /// </summary>
  [JsonPropertyName("offsetTaxYear")]
  [Member(Index = 5, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffsetTaxYear
  {
    get => offsetTaxYear;
    set => offsetTaxYear = value;
  }

  /// <summary>
  /// The value of the REQUEST_DATE attribute.
  /// The refund date is the business transaction date.  This date may not be 
  /// the same as the date that the refund was entered into the system.  If a
  /// cash receipt was refunded when the system was down then the refund date
  /// might be a day prior to the creation date.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? RequestDate
  {
    get => requestDate;
    set => requestDate = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// The reason why some or all of the cash receipt was refunded.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 7, Type = MemberType.Varchar, Length = ReasonText_MaxLength, Optional
    = true)]
  public string ReasonText
  {
    get => reasonText;
    set => reasonText = value != null
      ? Substring(value, 1, ReasonText_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The userid or program id responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The system time the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the OFFSET_CLOSED attribute.</summary>
  public const int OffsetClosed_MaxLength = 1;

  /// <summary>
  /// The value of the OFFSET_CLOSED attribute.
  /// Defines whether or not an offset advancement is open or closed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = OffsetClosed_MaxLength)]
  [Value("N")]
  [Value("Y")]
  [Value("")]
  public string OffsetClosed
  {
    get => offsetClosed ?? "";
    set => offsetClosed = TrimEnd(Substring(value, 1, OffsetClosed_MaxLength));
  }

  /// <summary>
  /// The json value of the OffsetClosed attribute.</summary>
  [JsonPropertyName("offsetClosed")]
  [Computed]
  public string OffsetClosed_Json
  {
    get => NullIf(OffsetClosed, "");
    set => OffsetClosed = value;
  }

  /// <summary>
  /// The value of the DATE_TRANSMITTED attribute.
  /// This is a Date on which the Fed was notified that a refund was produced in
  /// the Kessep System.
  /// </summary>
  [JsonPropertyName("dateTransmitted")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? DateTransmitted
  {
    get => dateTransmitted;
    set => dateTransmitted = value;
  }

  /// <summary>Length of the TAX_ID_SUFFIX attribute.</summary>
  public const int TaxIdSuffix_MaxLength = 2;

  /// <summary>
  /// The value of the TAX_ID_SUFFIX attribute.
  /// This attribute represents the Tax_Id_Suffix.	Example: A corporation such 
  /// as OSCO may have Tax_Id 12345, however the suffix is used to denote the
  /// different stores for OSCO such as 12345-22.
  /// </summary>
  [JsonPropertyName("taxIdSuffix")]
  [Member(Index = 12, Type = MemberType.Char, Length = TaxIdSuffix_MaxLength, Optional
    = true)]
  public string TaxIdSuffix
  {
    get => taxIdSuffix;
    set => taxIdSuffix = value != null
      ? TrimEnd(Substring(value, 1, TaxIdSuffix_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The sign-on of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the KPC_NOTICE_REQ_IND attribute.</summary>
  public const int KpcNoticeReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the KPC_NOTICE_REQ_IND attribute.
  /// Defines whether or not a KPC notice is required for the refund.
  /// 				Values: Y - Yes, Court Notice is required.
  /// 			 	Values: N - No, Court Notice is NOT required. (Default).
  /// </summary>
  [JsonPropertyName("kpcNoticeReqInd")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = KpcNoticeReqInd_MaxLength, Optional = true)]
  public string KpcNoticeReqInd
  {
    get => kpcNoticeReqInd;
    set => kpcNoticeReqInd = value != null
      ? TrimEnd(Substring(value, 1, KpcNoticeReqInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the KPC_NOTICE_PROCESSED_DATE attribute.
  /// Defines the date that the KPC was notified of the refund.
  /// </summary>
  [JsonPropertyName("kpcNoticeProcessedDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? KpcNoticeProcessedDate
  {
    get => kpcNoticeProcessedDate;
    set => kpcNoticeProcessedDate = value;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdIdentifier")]
  [Member(Index = 17, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? CrdIdentifier
  {
    get => crdIdentifier;
    set => crdIdentifier = value;
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
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CrvIdentifier
  {
    get => crvIdentifier;
    set => crvIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [Member(Index = 19, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CstIdentifier
  {
    get => cstIdentifier;
    set => cstIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [Member(Index = 20, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that distinguishes a occurrance of an entity 
  /// from all other occurrances.
  /// </summary>
  [JsonPropertyName("cdaIdentifier")]
  [Member(Index = 21, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CdaIdentifier
  {
    get => cdaIdentifier;
    set => cdaIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstAIdentifier")]
  [Member(Index = 22, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CstAIdentifier
  {
    get => cstAIdentifier;
    set => cstAIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 23, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique number that specifies each occurrence of the entity type.
  /// </summary>
  [JsonPropertyName("ftrIdentifier")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FtrIdentifier
  {
    get => ftrIdentifier;
    set => ftrIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("funIdentifier")]
  [Member(Index = 25, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FunIdentifier
  {
    get => funIdentifier;
    set => funIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("fttIdentifier")]
  [Member(Index = 26, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FttIdentifier
  {
    get => fttIdentifier;
    set => fttIdentifier = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("pcaEffectiveDt")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? PcaEffectiveDt
  {
    get => pcaEffectiveDt;
    set => pcaEffectiveDt = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int PcaCode_MaxLength = 5;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation  for the purpose of quick identification.
  /// </summary>
  [JsonPropertyName("pcaCode")]
  [Member(Index = 28, Type = MemberType.Char, Length = PcaCode_MaxLength, Optional
    = true)]
  public string PcaCode
  {
    get => pcaCode;
    set => pcaCode = value != null
      ? TrimEnd(Substring(value, 1, PcaCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cltIdentifier")]
  [Member(Index = 29, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CltIdentifier
  {
    get => cltIdentifier;
    set => cltIdentifier = value;
  }

  private string reasonCode;
  private string taxid;
  private string payeeName;
  private decimal amount;
  private int? offsetTaxYear;
  private DateTime? requestDate;
  private string reasonText;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string offsetClosed;
  private DateTime? dateTransmitted;
  private string taxIdSuffix;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string kpcNoticeReqInd;
  private DateTime? kpcNoticeProcessedDate;
  private int? crdIdentifier;
  private int? crvIdentifier;
  private int? cstIdentifier;
  private int? crtIdentifier;
  private DateTime? cdaIdentifier;
  private int? cstAIdentifier;
  private string cspNumber;
  private int? ftrIdentifier;
  private int? funIdentifier;
  private int? fttIdentifier;
  private DateTime? pcaEffectiveDt;
  private string pcaCode;
  private int? cltIdentifier;
}

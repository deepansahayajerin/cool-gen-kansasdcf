// The source file: KS_DRIVERS_LICENSE, ID: 371334780, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Tracks the life cycle of KS driver's license restriction process for one AP 
/// and one Court Order
/// </summary>
[Serializable]
public partial class KsDriversLicense: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KsDriversLicense()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KsDriversLicense(KsDriversLicense that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KsDriversLicense Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the KS_DVR_LICENSE attribute.</summary>
  public const int KsDvrLicense_MaxLength = 9;

  /// <summary>
  /// The value of the KS_DVR_LICENSE attribute.
  /// The driver's license number from DMV
  /// </summary>
  [JsonPropertyName("ksDvrLicense")]
  [Member(Index = 1, Type = MemberType.Char, Length = KsDvrLicense_MaxLength, Optional
    = true)]
  public string KsDvrLicense
  {
    get => Get<string>("ksDvrLicense");
    set => Set(
      "ksDvrLicense", TrimEnd(Substring(value, 1, KsDvrLicense_MaxLength)));
  }

  /// <summary>
  /// The value of the VALIDATION_DATE attribute.
  /// The last date that the current KS driver's license was validated by DMV
  /// </summary>
  [JsonPropertyName("validationDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? ValidationDate
  {
    get => Get<DateTime?>("validationDate");
    set => Set("validationDate", value);
  }

  /// <summary>
  /// The value of the COURTESY_LETTER_SENT_DATE attribute.
  /// Date the courtesy letter was sent to the AP
  /// </summary>
  [JsonPropertyName("courtesyLetterSentDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? CourtesyLetterSentDate
  {
    get => Get<DateTime?>("courtesyLetterSentDate");
    set => Set("courtesyLetterSentDate", value);
  }

  /// <summary>
  /// The value of the 30_DAY_LETTER_CREATED_DATE attribute.
  /// Date the 30-day notice was created
  /// </summary>
  [JsonPropertyName("attribute30DayLetterCreatedDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? Attribute30DayLetterCreatedDate
  {
    get => Get<DateTime?>("attribute30DayLetterCreatedDate");
    set => Set("attribute30DayLetterCreatedDate", value);
  }

  /// <summary>Length of the SERVICE_COMPLETE_IND attribute.</summary>
  public const int ServiceCompleteInd_MaxLength = 1;

  /// <summary>
  /// The value of the SERVICE_COMPLETE_IND attribute.
  /// Indicates if the certified letter card has been returned
  /// </summary>
  [JsonPropertyName("serviceCompleteInd")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ServiceCompleteInd_MaxLength, Optional = true)]
  public string ServiceCompleteInd
  {
    get => Get<string>("serviceCompleteInd");
    set => Set(
      "serviceCompleteInd", TrimEnd(Substring(value, 1,
      ServiceCompleteInd_MaxLength)));
  }

  /// <summary>
  /// The value of the SERVICE_COMPLETE_DATE attribute.
  /// The date the status of the certified letter was changed or the date the 
  /// certified letter receipt was received
  /// </summary>
  [JsonPropertyName("serviceCompleteDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? ServiceCompleteDate
  {
    get => Get<DateTime?>("serviceCompleteDate");
    set => Set("serviceCompleteDate", value);
  }

  /// <summary>
  /// The value of the RESTRICTED_DATE attribute.
  /// Date the request to have the AP's driver's license restricted sent to the 
  /// DMV
  /// </summary>
  [JsonPropertyName("restrictedDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? RestrictedDate
  {
    get => Get<DateTime?>("restrictedDate");
    set => Set("restrictedDate", value);
  }

  /// <summary>
  /// The value of the REINSTATED_DATE attribute.
  /// Date request sent to the DMV to reinstate the AP's driver's license
  /// </summary>
  [JsonPropertyName("reinstatedDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ReinstatedDate
  {
    get => Get<DateTime?>("reinstatedDate");
    set => Set("reinstatedDate", value);
  }

  /// <summary>
  /// The value of the APPEAL_RECEIVED_DATE attribute.
  /// Date that an appeal was received from the AP
  /// </summary>
  [JsonPropertyName("appealReceivedDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? AppealReceivedDate
  {
    get => Get<DateTime?>("appealReceivedDate");
    set => Set("appealReceivedDate", value);
  }

  /// <summary>Length of the APPEAL_RESOLVED attribute.</summary>
  public const int AppealResolved_MaxLength = 1;

  /// <summary>
  /// The value of the APPEAL_RESOLVED attribute.
  /// The outcome of the appeal
  /// </summary>
  [JsonPropertyName("appealResolved")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AppealResolved_MaxLength, Optional = true)]
  public string AppealResolved
  {
    get => Get<string>("appealResolved");
    set => Set(
      "appealResolved", TrimEnd(Substring(value, 1, AppealResolved_MaxLength)));
      
  }

  /// <summary>
  /// The value of the PAYMENT_AGREEMENT_DATE attribute.
  /// </summary>
  [JsonPropertyName("paymentAgreementDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? PaymentAgreementDate
  {
    get => Get<DateTime?>("paymentAgreementDate");
    set => Set("paymentAgreementDate", value);
  }

  /// <summary>
  /// The value of the FIRST_PAYMENT_DUE_DATE attribute.
  /// Date the first payment of new payment agreement is due
  /// </summary>
  [JsonPropertyName("firstPaymentDueDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? FirstPaymentDueDate
  {
    get => Get<DateTime?>("firstPaymentDueDate");
    set => Set("firstPaymentDueDate", value);
  }

  /// <summary>
  /// The value of the AMOUNT_DUE attribute.
  /// Amount the AP has agreed to pay in the payment agreement
  /// </summary>
  [JsonPropertyName("amountDue")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AmountDue
  {
    get => Get<decimal?>("amountDue");
    set => Set("amountDue", Truncate(value, 2));
  }

  /// <summary>Length of the MANUAL_IND attribute.</summary>
  public const int ManualInd_MaxLength = 1;

  /// <summary>
  /// The value of the MANUAL_IND attribute.
  /// Indicates the driver's license restriction request was done manually
  /// </summary>
  [JsonPropertyName("manualInd")]
  [Member(Index = 14, Type = MemberType.Char, Length = ManualInd_MaxLength, Optional
    = true)]
  public string ManualInd
  {
    get => Get<string>("manualInd");
    set => Set("manualInd", TrimEnd(Substring(value, 1, ManualInd_MaxLength)));
  }

  /// <summary>
  /// The value of the MANUAL_DATE attribute.
  /// Date the driver's license restirction request was made
  /// </summary>
  [JsonPropertyName("manualDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? ManualDate
  {
    get => Get<DateTime?>("manualDate");
    set => Set("manualDate", value);
  }

  /// <summary>
  /// The value of the APPEAL_RESOLVED_DATE attribute.
  /// Date the appeal decision was entered on KDMV
  /// </summary>
  [JsonPropertyName("appealResolvedDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? AppealResolvedDate
  {
    get => Get<DateTime?>("appealResolvedDate");
    set => Set("appealResolvedDate", value);
  }

  /// <summary>
  /// The value of the LICENSE_RESTRICTION_SENT_DATE attribute.
  /// The date the driver's license restriction was sent to the DMV
  /// </summary>
  [JsonPropertyName("licenseRestrictionSentDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? LicenseRestrictionSentDate
  {
    get => Get<DateTime?>("licenseRestrictionSentDate");
    set => Set("licenseRestrictionSentDate", value);
  }

  /// <summary>Length of the RESTRICTION_STATUS attribute.</summary>
  public const int RestrictionStatus_MaxLength = 18;

  /// <summary>
  /// The value of the RESTRICTION_STATUS attribute.
  /// This is the status of the restriction request that was sent to the DMV.  
  /// Some of the statuses are: REQUEST SENT, RESTRICTED, RESTRICTED DENIED, and
  /// REINSTATED.
  /// </summary>
  [JsonPropertyName("restrictionStatus")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = RestrictionStatus_MaxLength, Optional = true)]
  public string RestrictionStatus
  {
    get => Get<string>("restrictionStatus");
    set => Set(
      "restrictionStatus", TrimEnd(Substring(value, 1,
      RestrictionStatus_MaxLength)));
  }

  /// <summary>
  /// The value of the AMOUNT_OWED attribute.
  /// The amount that the AP owed for a Court Order when the Driver's License 
  /// restriction was sent to DMV.
  /// </summary>
  [JsonPropertyName("amountOwed")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AmountOwed
  {
    get => Get<decimal?>("amountOwed");
    set => Set("amountOwed", Truncate(value, 2));
  }

  /// <summary>Length of the RECORD_CLOSURE_REASON attribute.</summary>
  public const int RecordClosureReason_MaxLength = 18;

  /// <summary>
  /// The value of the RECORD_CLOSURE_REASON attribute.
  /// The reason CSE is no longer pursuing a restriction on this AP's driver's 
  /// license.
  /// </summary>
  [JsonPropertyName("recordClosureReason")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = RecordClosureReason_MaxLength, Optional = true)]
  public string RecordClosureReason
  {
    get => Get<string>("recordClosureReason");
    set => Set(
      "recordClosureReason", TrimEnd(Substring(value, 1,
      RecordClosureReason_MaxLength)));
  }

  /// <summary>
  /// The value of the RECORD_CLOSURE_DATE attribute.
  /// The date CSE stopped pursuing a restriction on this AP's driver's license.
  /// </summary>
  [JsonPropertyName("recordClosureDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? RecordClosureDate
  {
    get => Get<DateTime?>("recordClosureDate");
    set => Set("recordClosureDate", value);
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 22, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => Get<DateTime?>("createdTstamp");
    set => Set("createdTstamp", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
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

  /// <summary>Length of the NOTE_1 attribute.</summary>
  public const int Note1_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE_1 attribute.
  /// Note area 1
  /// </summary>
  [JsonPropertyName("note1")]
  [Member(Index = 26, Type = MemberType.Varchar, Length = Note1_MaxLength, Optional
    = true)]
  public string Note1
  {
    get => Get<string>("note1");
    set => Set("note1", Substring(value, 1, Note1_MaxLength));
  }

  /// <summary>Length of the NOTE_2 attribute.</summary>
  public const int Note2_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE_2 attribute.
  /// Note area 2
  /// </summary>
  [JsonPropertyName("note2")]
  [Member(Index = 27, Type = MemberType.Varchar, Length = Note2_MaxLength, Optional
    = true)]
  public string Note2
  {
    get => Get<string>("note2");
    set => Set("note2", Substring(value, 1, Note2_MaxLength));
  }

  /// <summary>Length of the NOTE_3 attribute.</summary>
  public const int Note3_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE_3 attribute.
  /// Note area 3
  /// </summary>
  [JsonPropertyName("note3")]
  [Member(Index = 28, Type = MemberType.Varchar, Length = Note3_MaxLength, Optional
    = true)]
  public string Note3
  {
    get => Get<string>("note3");
    set => Set("note3", Substring(value, 1, Note3_MaxLength));
  }

  /// <summary>Length of the PROCESSED_IND attribute.</summary>
  public const int ProcessedInd_MaxLength = 1;

  /// <summary>
  /// The value of the PROCESSED_IND attribute.
  /// This indicator is only used for internal processing by various programs in
  /// the Driver's License Restriction process.
  /// </summary>
  [JsonPropertyName("processedInd")]
  [Member(Index = 29, Type = MemberType.Char, Length = ProcessedInd_MaxLength, Optional
    = true)]
  public string ProcessedInd
  {
    get => Get<string>("processedInd");
    set => Set(
      "processedInd", TrimEnd(Substring(value, 1, ProcessedInd_MaxLength)));
  }

  /// <summary>
  /// The value of the SEQUENCE_COUNTER attribute.
  /// </summary>
  [JsonPropertyName("sequenceCounter")]
  [DefaultValue(0)]
  [Member(Index = 30, Type = MemberType.Number, Length = 9)]
  public int SequenceCounter
  {
    get => Get<int?>("sequenceCounter") ?? 0;
    set => Set("sequenceCounter", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = CspNum_MaxLength)]
  public string CspNum
  {
    get => Get<string>("cspNum") ?? "";
    set => Set(
      "cspNum", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspNum_MaxLength)));
  }

  /// <summary>
  /// The json value of the CspNum attribute.</summary>
  [JsonPropertyName("cspNum")]
  [Computed]
  public string CspNum_Json
  {
    get => NullIf(CspNum, "");
    set => CspNum = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => Get<int?>("lgaIdentifier");
    set => Set("lgaIdentifier", value);
  }
}

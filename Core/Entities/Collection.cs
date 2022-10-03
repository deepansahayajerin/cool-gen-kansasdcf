// The source file: COLLECTION, ID: 371432301, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This represents an occurance of a Cash Receipt Detail being applied to a 
/// Debt.  This is known as a collection.
/// </summary>
[Serializable]
public partial class Collection: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Collection()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Collection(Collection that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Collection Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the PROGRAM_APPLIED_TO attribute.</summary>
  public const int ProgramAppliedTo_MaxLength = 3;

  /// <summary>
  /// The value of the PROGRAM_APPLIED_TO attribute.
  /// The distribution program type for the active program for the supported 
  /// person at the time the collection was distributed.
  /// Values are AF,FC,NA,NF or spaces.
  /// Distribution program types AF and FC are considered to be ADC collections 
  /// where the state retains the money; whereas, NA and NF are NADC collections
  /// where the money is distributed to the applicant recipient.
  /// Per. Beth Burrel 12/23/96
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ProgramAppliedTo_MaxLength)
    ]
  [Value("")]
  [Value("NF")]
  [Value("NA")]
  [Value("NC")]
  [Value("NAI")]
  [Value("FCI")]
  [Value("AFI")]
  [Value("FC")]
  [Value("AF")]
  public string ProgramAppliedTo
  {
    get => Get<string>("programAppliedTo") ?? "";
    set => Set(
      "programAppliedTo", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ProgramAppliedTo_MaxLength)));
  }

  /// <summary>
  /// The json value of the ProgramAppliedTo attribute.</summary>
  [JsonPropertyName("programAppliedTo")]
  [Computed]
  public string ProgramAppliedTo_Json
  {
    get => NullIf(ProgramAppliedTo, "");
    set => ProgramAppliedTo = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => Get<int?>("systemGeneratedIdentifier") ?? 0;
    set => Set("systemGeneratedIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// Amount of the Cash Receipt Detail that was applied to the Debt as a 
  /// COLLECTION.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => Get<decimal?>("amount") ?? 0M;
    set => Set("amount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>Length of the APPLIED_TO_CODE attribute.</summary>
  public const int AppliedToCode_MaxLength = 1;

  /// <summary>
  /// The value of the APPLIED_TO_CODE attribute.
  /// Defines whether the collection was applied to CURRENT, ARREARS, GIFT or 
  /// INTEREST.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = AppliedToCode_MaxLength)]
  [Value("C")]
  [Value("I")]
  [Value("A")]
  [Value("G")]
  public string AppliedToCode
  {
    get => Get<string>("appliedToCode") ?? "";
    set => Set(
      "appliedToCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AppliedToCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the AppliedToCode attribute.</summary>
  [JsonPropertyName("appliedToCode")]
  [Computed]
  public string AppliedToCode_Json
  {
    get => NullIf(AppliedToCode, "");
    set => AppliedToCode = value;
  }

  /// <summary>
  /// The value of the COLLECTION_DT attribute.
  /// The date of Collection.
  /// </summary>
  [JsonPropertyName("collectionDt")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? CollectionDt
  {
    get => Get<DateTime?>("collectionDt");
    set => Set("collectionDt", value);
  }

  /// <summary>
  /// The value of the DISBURSEMENT_DT attribute.
  /// Represents the date the collection is disbursed.
  /// </summary>
  [JsonPropertyName("disbursementDt")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DisbursementDt
  {
    get => Get<DateTime?>("disbursementDt");
    set => Set("disbursementDt", value);
  }

  /// <summary>Length of the ADJUSTED_IND attribute.</summary>
  public const int AdjustedInd_MaxLength = 1;

  /// <summary>
  /// The value of the ADJUSTED_IND attribute.
  /// Indicate whether or not the collection has been adjusted.
  /// </summary>
  [JsonPropertyName("adjustedInd")]
  [Member(Index = 7, Type = MemberType.Char, Length = AdjustedInd_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string AdjustedInd
  {
    get => Get<string>("adjustedInd");
    set => Set(
      "adjustedInd", TrimEnd(Substring(value, 1, AdjustedInd_MaxLength)));
  }

  /// <summary>Length of the CONCURRENT_IND attribute.</summary>
  public const int ConcurrentInd_MaxLength = 1;

  /// <summary>
  /// The value of the CONCURRENT_IND attribute.
  /// Identifies whether a specific collection is a for a concurrent obligation.
  /// If the collection represents a concurrent collection, then the funding
  /// and disbursement processes iqnore the collection.  Both the reqular and
  /// concurrent collections will be tied to the same cash receipt detail.
  /// Permitted Values:
  ///   Y - Yes
  ///   N - No
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ConcurrentInd_MaxLength)]
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("N")]
  public string ConcurrentInd
  {
    get => Get<string>("concurrentInd") ?? "";
    set => Set(
      "concurrentInd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ConcurrentInd_MaxLength)));
  }

  /// <summary>
  /// The json value of the ConcurrentInd attribute.</summary>
  [JsonPropertyName("concurrentInd")]
  [Computed]
  public string ConcurrentInd_Json
  {
    get => NullIf(ConcurrentInd, "");
    set => ConcurrentInd = value;
  }

  /// <summary>
  /// The value of the COLLECTION_ADJUSTMENT_DT attribute.
  /// The date the Collection Adjustment was applied to a specific Collection.
  /// </summary>
  [JsonPropertyName("collectionAdjustmentDt")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? CollectionAdjustmentDt
  {
    get => Get<DateTime?>("collectionAdjustmentDt");
    set => Set("collectionAdjustmentDt", value);
  }

  /// <summary>
  /// The value of the COLLECTION ADJ PROCESS DATE attribute.
  /// Represents the date that the collection was processed for the adjustment.
  /// </summary>
  [JsonPropertyName("collectionAdjProcessDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? CollectionAdjProcessDate
  {
    get => Get<DateTime?>("collectionAdjProcessDate");
    set => Set("collectionAdjProcessDate", value);
  }

  /// <summary>
  /// The value of the DISBURSEMENT_ADJ_PROCESS_DATE attribute.
  /// Represents the date that the adjustment was process by the disbursements 
  /// system.
  /// </summary>
  [JsonPropertyName("disbursementAdjProcessDate")]
  [Member(Index = 11, Type = MemberType.Date)]
  public DateTime? DisbursementAdjProcessDate
  {
    get => Get<DateTime?>("disbursementAdjProcessDate");
    set => Set("disbursementAdjProcessDate", value);
  }

  /// <summary>Length of the DISBURSEMENT_PROCESSING_NEED_IND attribute.
  /// </summary>
  public const int DisbursementProcessingNeedInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISBURSEMENT_PROCESSING_NEED_IND attribute.
  /// This indicator is set during the distribution process and is used to tell 
  /// the disburesements process that this row needs to be selected for
  /// processing.
  /// Example: 'Y' Select for disbursement processing.
  /// </summary>
  [JsonPropertyName("disbursementProcessingNeedInd")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = DisbursementProcessingNeedInd_MaxLength, Optional = true)]
  [Value(null)]
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("Y")]
  public string DisbursementProcessingNeedInd
  {
    get => Get<string>("disbursementProcessingNeedInd");
    set => Set(
      "disbursementProcessingNeedInd", TrimEnd(Substring(value, 1,
      DisbursementProcessingNeedInd_MaxLength)));
  }

  /// <summary>Length of the DISTRIBUTION_METHOD attribute.</summary>
  public const int DistributionMethod_MaxLength = 1;

  /// <summary>
  /// The value of the DISTRIBUTION_METHOD attribute.
  /// Defines the method of distribution. There are two types: A - Automatic, M 
  /// - Manual
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = DistributionMethod_MaxLength)]
  [Value("W")]
  [Value("M")]
  [Value("A")]
  [Value("P")]
  [Value("C")]
  public string DistributionMethod
  {
    get => Get<string>("distributionMethod") ?? "";
    set => Set(
      "distributionMethod", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, DistributionMethod_MaxLength)));
  }

  /// <summary>
  /// The json value of the DistributionMethod attribute.</summary>
  [JsonPropertyName("distributionMethod")]
  [Computed]
  public string DistributionMethod_Json
  {
    get => NullIf(DistributionMethod, "");
    set => DistributionMethod = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => Get<DateTime?>("createdTmst");
    set => Set("createdTmst", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 17, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => Get<DateTime?>("lastUpdatedTmst");
    set => Set("lastUpdatedTmst", value);
  }

  /// <summary>Length of the APPLIED_TO_ORDER_TYPE_CODE attribute.</summary>
  public const int AppliedToOrderTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the APPLIED_TO_ORDER_TYPE_CODE attribute.
  /// Determines type of obligation to which the collcetion was applied.
  /// 	I - Applied to Incoming Interstate Obl.
  /// 	K - Applied to Kansas Obl. (default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = AppliedToOrderTypeCode_MaxLength)]
  [Value("K")]
  [Value("I")]
  [ImplicitValue("K")]
  public string AppliedToOrderTypeCode
  {
    get => Get<string>("appliedToOrderTypeCode") ?? "";
    set => Set(
      "appliedToOrderTypeCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AppliedToOrderTypeCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the AppliedToOrderTypeCode attribute.</summary>
  [JsonPropertyName("appliedToOrderTypeCode")]
  [Computed]
  public string AppliedToOrderTypeCode_Json
  {
    get => NullIf(AppliedToOrderTypeCode, "");
    set => AppliedToOrderTypeCode = value;
  }

  /// <summary>Length of the MANUAL_DISTRIBUTION_REASON_TEXT attribute.
  /// </summary>
  public const int ManualDistributionReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the MANUAL_DISTRIBUTION_REASON_TEXT attribute.
  /// A descriptive reason for a manual distribution.
  /// </summary>
  [JsonPropertyName("manualDistributionReasonText")]
  [Member(Index = 19, Type = MemberType.Varchar, Length
    = ManualDistributionReasonText_MaxLength, Optional = true)]
  public string ManualDistributionReasonText
  {
    get => Get<string>("manualDistributionReasonText");
    set => Set(
      "manualDistributionReasonText", Substring(value, 1,
      ManualDistributionReasonText_MaxLength));
  }

  /// <summary>Length of the COLLECTION_ADJUSTMENT_REASON_TXT attribute.
  /// </summary>
  public const int CollectionAdjustmentReasonTxt_MaxLength = 240;

  /// <summary>
  /// The value of the COLLECTION_ADJUSTMENT_REASON_TXT attribute.
  /// Describes the reason for the adjustment.
  /// </summary>
  [JsonPropertyName("collectionAdjustmentReasonTxt")]
  [Member(Index = 20, Type = MemberType.Varchar, Length
    = CollectionAdjustmentReasonTxt_MaxLength, Optional = true)]
  public string CollectionAdjustmentReasonTxt
  {
    get => Get<string>("collectionAdjustmentReasonTxt");
    set => Set(
      "collectionAdjustmentReasonTxt", Substring(value, 1,
      CollectionAdjustmentReasonTxt_MaxLength));
  }

  /// <summary>Length of the COURT_NOTICE_REQ_IND attribute.</summary>
  public const int CourtNoticeReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_NOTICE_REQ_IND attribute.
  /// Defines whether or not a court notice is required for the collection. This
  /// attribute will be set by the Distribution Batch Program. The print
  /// program that follows this will process create court notices using records
  /// that have this attribute set to Y and the attribute below set to nulls.
  /// 	Values: Y - Yes, Court Notice is required.
  ///  	Values: N - No, Court Notice is NOT required. (Default)
  /// </summary>
  [JsonPropertyName("courtNoticeReqInd")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = CourtNoticeReqInd_MaxLength, Optional = true)]
  public string CourtNoticeReqInd
  {
    get => Get<string>("courtNoticeReqInd");
    set => Set(
      "courtNoticeReqInd", TrimEnd(Substring(value, 1,
      CourtNoticeReqInd_MaxLength)));
  }

  /// <summary>
  /// The value of the COURT_NOTICE_PROCESSED_DATE attribute.
  /// Defines the date that the court notice was produced for the collection. 
  /// This is used in conjunction with the above attribute.
  /// </summary>
  [JsonPropertyName("courtNoticeProcessedDate")]
  [Member(Index = 22, Type = MemberType.Date, Optional = true)]
  public DateTime? CourtNoticeProcessedDate
  {
    get => Get<DateTime?>("courtNoticeProcessedDate");
    set => Set("courtNoticeProcessedDate", value);
  }

  /// <summary>
  /// The value of the AE_NOTIFIED_DATE attribute.
  /// The date the IV-A agency was notified of this collection.
  /// </summary>
  [JsonPropertyName("aeNotifiedDate")]
  [Member(Index = 23, Type = MemberType.Date, Optional = true)]
  public DateTime? AeNotifiedDate
  {
    get => Get<DateTime?>("aeNotifiedDate");
    set => Set("aeNotifiedDate", value);
  }

  /// <summary>
  /// The value of the OCSE34_REPORTING_PERIOD attribute.
  /// Date Collection should be reported on OCSE34 if it is a Future Collection 
  /// that has not already been reported.
  /// </summary>
  [JsonPropertyName("ocse34ReportingPeriod")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? Ocse34ReportingPeriod
  {
    get => Get<DateTime?>("ocse34ReportingPeriod");
    set => Set("ocse34ReportingPeriod", value);
  }

  /// <summary>
  /// The value of the BAL_FOR_INT_COMP_BEF_COLL attribute.
  /// This attribute contains the debt balance befor this Collection. Interest 
  /// is chargeable on this balance up to this Collection Date.  In other words
  /// it gives the Last Balance used for computing the interest charged. This
  /// field gets computed and updated when the Collection record is created.
  /// </summary>
  [JsonPropertyName("balForIntCompBefColl")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? BalForIntCompBefColl
  {
    get => Get<decimal?>("balForIntCompBefColl");
    set => Set("balForIntCompBefColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CUM_INT_CHARGED_UPTO_COLL attribute.
  /// This attribute contains the cumulative interest charged as of the 
  /// Collection Date for the Debt. This facilitates recomputation of cumulative
  /// interest charged from the last collection date onwards instead of
  /// recomputing from the beginning. This field gets computed and updated when
  /// the Collection record is created.
  /// </summary>
  [JsonPropertyName("cumIntChargedUptoColl")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CumIntChargedUptoColl
  {
    get => Get<decimal?>("cumIntChargedUptoColl");
    set => Set("cumIntChargedUptoColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CUM_INT_COLL_AFTER_THIS_COLL attribute.
  /// This attribute contains the cumulative interest collected including this 
  /// collection for the Debt. It should be equal to the sum of all the
  /// collections applied towards a interest for the Debt. This avoids reading
  /// all the Collection records when it is required to compute the non-adc
  /// component of the interest during the refresh of the summary buckets. This
  /// field gets computed and updated when the Collection record is created.
  /// </summary>
  [JsonPropertyName("cumIntCollAfterThisColl")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CumIntCollAfterThisColl
  {
    get => Get<decimal?>("cumIntCollAfterThisColl");
    set => Set("cumIntCollAfterThisColl", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the INT_BAL_AFT_THIS_COLL attribute.
  /// This attribute contains the Debt Detail Interest Balance after this 
  /// Collection. This helps to rollback the Debt Detail Interest Balance if a
  /// collection is reversed due to any reason (namely new debts/AR changes/
  /// retro collection etc). If the interest balance is not rolled back the
  /// Distribution procedure may apply to an invalid interest balance. This
  /// field value must be equal to the Cum Interest Charged as of this
  /// Collection munus the sum of all the collections that are applied to
  /// towards interest upto and including this collection. Storing this value
  /// may improve the performance of the collection reversal process. This field
  /// gets computed and updated when the Collection record is created.
  /// </summary>
  [JsonPropertyName("intBalAftThisColl")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? IntBalAftThisColl
  {
    get => Get<decimal?>("intBalAftThisColl");
    set => Set("intBalAftThisColl", Truncate(value, 2));
  }

  /// <summary>Length of the DISBURSE_TO_AR_IND attribute.</summary>
  public const int DisburseToArInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISBURSE_TO_AR_IND attribute.
  /// This attribute specifies whether or not the collection should be 
  /// disburesed to AR. 		Y - The collection should be disbursed to AR					N -
  /// The collection should not be disbursed to AR
  /// </summary>
  [JsonPropertyName("disburseToArInd")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = DisburseToArInd_MaxLength, Optional = true)]
  public string DisburseToArInd
  {
    get => Get<string>("disburseToArInd");
    set => Set(
      "disburseToArInd",
      TrimEnd(Substring(value, 1, DisburseToArInd_MaxLength)));
  }

  /// <summary>Length of the COURT_ORDER_APPLIED_TO attribute.</summary>
  public const int CourtOrderAppliedTo_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_APPLIED_TO attribute.
  /// Describes the court order that collection was applied.
  /// </summary>
  [JsonPropertyName("courtOrderAppliedTo")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = CourtOrderAppliedTo_MaxLength, Optional = true)]
  public string CourtOrderAppliedTo
  {
    get => Get<string>("courtOrderAppliedTo");
    set => Set(
      "courtOrderAppliedTo", TrimEnd(Substring(value, 1,
      CourtOrderAppliedTo_MaxLength)));
  }

  /// <summary>Length of the APPLIED_TO_FUTURE attribute.</summary>
  public const int AppliedToFuture_MaxLength = 1;

  /// <summary>
  /// The value of the APPLIED_TO_FUTURE attribute.
  /// Describes whether or not the collection was applied to future debts. 
  /// &quot;Y&quot; will represent the collection was applied to future. &quot;N
  /// &quot; will represent the collection was not applied to future. This will
  /// greatly improve performance of the disbursement processing. The
  /// alternative is to derive this by reading two additional entities and
  /// comparing date ranges. The distribution process itself alredy knows
  /// whether or not the collection being applied to the future or not when it
  /// creates the collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = AppliedToFuture_MaxLength)
    ]
  [Value("N")]
  [Value("Y")]
  [ImplicitValue("N")]
  public string AppliedToFuture
  {
    get => Get<string>("appliedToFuture") ?? "";
    set => Set(
      "appliedToFuture", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, AppliedToFuture_MaxLength)));
  }

  /// <summary>
  /// The json value of the AppliedToFuture attribute.</summary>
  [JsonPropertyName("appliedToFuture")]
  [Computed]
  public string AppliedToFuture_Json
  {
    get => NullIf(AppliedToFuture, "");
    set => AppliedToFuture = value;
  }

  /// <summary>Length of the CSENET_OUTBOUND_REQ_IND attribute.</summary>
  public const int CsenetOutboundReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the CSENET_OUTBOUND_REQ_IND attribute.
  /// This indicator will be used to control the processing of CSENet outbound 
  /// collection reporting by identifying whether a CSENet reporting is
  /// required.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = CsenetOutboundReqInd_MaxLength)]
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("N")]
  public string CsenetOutboundReqInd
  {
    get => Get<string>("csenetOutboundReqInd") ?? "";
    set => Set(
      "csenetOutboundReqInd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CsenetOutboundReqInd_MaxLength)));
  }

  /// <summary>
  /// The json value of the CsenetOutboundReqInd attribute.</summary>
  [JsonPropertyName("csenetOutboundReqInd")]
  [Computed]
  public string CsenetOutboundReqInd_Json
  {
    get => NullIf(CsenetOutboundReqInd, "");
    set => CsenetOutboundReqInd = value;
  }

  /// <summary>
  /// The value of the CSENET_OUTBOUND_PROC_DT attribute.
  /// This date will be used to control the processing of CSENet outbound 
  /// collection reporting by identifying the processing date of the record.
  /// </summary>
  [JsonPropertyName("csenetOutboundProcDt")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? CsenetOutboundProcDt
  {
    get => Get<DateTime?>("csenetOutboundProcDt");
    set => Set("csenetOutboundProcDt", value);
  }

  /// <summary>
  /// The value of the CSENET_OUTBOUND_ADJ_PROJ_DT attribute.
  /// This date will be used to control the processing of CSENet outbound 
  /// collection reporting by identifying the date the adjustment was processed.
  /// </summary>
  [JsonPropertyName("csenetOutboundAdjProjDt")]
  [Member(Index = 34, Type = MemberType.Date, Optional = true)]
  public DateTime? CsenetOutboundAdjProjDt
  {
    get => Get<DateTime?>("csenetOutboundAdjProjDt");
    set => Set("csenetOutboundAdjProjDt", value);
  }

  /// <summary>
  /// The value of the COURT_NOTICE_ADJ_PROCESS_DATE attribute.
  /// This date will be used to control the processing of adjusted collection 
  /// for court notices. If the collection is adjusted and the
  /// court_notice_req_ind = Y and court_notice_process_date is not null, then
  /// the court notice processing is to be done. This attribute will be updated
  /// with the process date when successfully processed.
  /// </summary>
  [JsonPropertyName("courtNoticeAdjProcessDate")]
  [Member(Index = 35, Type = MemberType.Date)]
  public DateTime? CourtNoticeAdjProcessDate
  {
    get => Get<DateTime?>("courtNoticeAdjProcessDate");
    set => Set("courtNoticeAdjProcessDate", value);
  }

  /// <summary>Length of the DIST_PGM_STATE_APPLD_TO attribute.</summary>
  public const int DistPgmStateAppldTo_MaxLength = 2;

  /// <summary>
  /// The value of the DIST_PGM_STATE_APPLD_TO attribute.
  /// This attribute defines the state of the assigned program when the 
  /// collection was created. Only valid for NA, AF &amp; FC programs. The state
  /// will be assigned for both current and arrears payments. The valid values
  /// are as follows:
  /// 
  /// UP, Unassigned Pre-Assistance                                         UD,
  /// Unassigned During                                                      CA
  /// , Conditionally Assigned
  /// NA, Never Assigned
  /// PA,
  /// Permanently Assigned
  /// TA,
  /// Temporarily Assigned
  /// </summary>
  [JsonPropertyName("distPgmStateAppldTo")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = DistPgmStateAppldTo_MaxLength, Optional = true)]
  public string DistPgmStateAppldTo
  {
    get => Get<string>("distPgmStateAppldTo");
    set => Set(
      "distPgmStateAppldTo", TrimEnd(Substring(value, 1,
      DistPgmStateAppldTo_MaxLength)));
  }

  /// <summary>Length of the AR_NUMBER attribute.</summary>
  public const int ArNumber_MaxLength = 10;

  /// <summary>
  /// The value of the AR_NUMBER attribute.
  /// This field will contain the CSE Person number of the AR once an AR has 
  /// been determined for the collection.
  /// </summary>
  [JsonPropertyName("arNumber")]
  [Member(Index = 37, Type = MemberType.Char, Length = ArNumber_MaxLength, Optional
    = true)]
  public string ArNumber
  {
    get => Get<string>("arNumber");
    set => Set("arNumber", TrimEnd(Substring(value, 1, ArNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the UNADJUSTED_DATE attribute.
  /// The date a collection is un-adjusted due to retro processing.  This 
  /// attribute is set by distribution and is required so that OCSE34 can
  /// accurately report how and when money is applied.
  /// </summary>
  [JsonPropertyName("unadjustedDate")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? UnadjustedDate
  {
    get => Get<DateTime?>("unadjustedDate");
    set => Set("unadjustedDate", value);
  }

  /// <summary>
  /// The value of the PREVIOUS_COLLECTION_ADJ_DATE attribute.
  /// This attribute will be set to the collection adjustment date value before 
  /// a collection is un-adjusted due to retro processing.  This attribute is
  /// set by distribution and is required so that OCSE34 can accurately report
  /// how and when money is applied.
  /// </summary>
  [JsonPropertyName("previousCollectionAdjDate")]
  [Member(Index = 39, Type = MemberType.Date, Optional = true)]
  public DateTime? PreviousCollectionAdjDate
  {
    get => Get<DateTime?>("previousCollectionAdjDate");
    set => Set("previousCollectionAdjDate", value);
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdId")]
  [DefaultValue(0)]
  [Member(Index = 40, Type = MemberType.Number, Length = 4)]
  public int CrdId
  {
    get => Get<int?>("crdId") ?? 0;
    set => Set("crdId", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvId")]
  [DefaultValue(0)]
  [Member(Index = 41, Type = MemberType.Number, Length = 9)]
  public int CrvId
  {
    get => Get<int?>("crvId") ?? 0;
    set => Set("crvId", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstId")]
  [DefaultValue(0)]
  [Member(Index = 42, Type = MemberType.Number, Length = 3)]
  public int CstId
  {
    get => Get<int?>("cstId") ?? 0;
    set => Set("cstId", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtType")]
  [DefaultValue(0)]
  [Member(Index = 43, Type = MemberType.Number, Length = 3)]
  public int CrtType
  {
    get => Get<int?>("crtType") ?? 0;
    set => Set("crtType", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("carId")]
  [Member(Index = 44, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CarId
  {
    get => Get<int?>("carId");
    set => Set("carId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [DefaultValue(0)]
  [Member(Index = 45, Type = MemberType.Number, Length = 3)]
  public int OtyId
  {
    get => Get<int?>("otyId") ?? 0;
    set => Set("otyId", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => Get<string>("otrType") ?? "";
    set => Set(
      "otrType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, OtrType_MaxLength)));
  }

  /// <summary>
  /// The json value of the OtrType attribute.</summary>
  [JsonPropertyName("otrType")]
  [Computed]
  public string OtrType_Json
  {
    get => NullIf(OtrType, "");
    set => OtrType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrId")]
  [DefaultValue(0)]
  [Member(Index = 47, Type = MemberType.Number, Length = 9)]
  public int OtrId
  {
    get => Get<int?>("otrId") ?? 0;
    set => Set("otrId", value == 0 ? null : value as int?);
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => Get<string>("cpaType") ?? "";
    set => Set(
      "cpaType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CpaType_MaxLength)));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
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
  [Member(Index = 49, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [DefaultValue(0)]
  [Member(Index = 50, Type = MemberType.Number, Length = 3)]
  public int ObgId
  {
    get => Get<int?>("obgId") ?? 0;
    set => Set("obgId", value == 0 ? null : value as int?);
  }
}

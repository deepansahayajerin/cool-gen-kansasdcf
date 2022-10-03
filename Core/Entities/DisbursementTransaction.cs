// The source file: DISBURSEMENT_TRANSACTION, ID: 371433688, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Records common detail informatioin of financial transactions created for a 
/// disbursement and relates together those entities which further detail the
/// transaction.  Types of Disbursement transactions include Disb Collections,
/// Disbursements, Recaptures and Disbursement Fees.
/// Example: date, amounts, who created the transaction and the timestamp and 
/// who last updated the transaction and the timestamp.
/// </summary>
[Serializable]
public partial class DisbursementTransaction: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbursementTransaction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbursementTransaction(DisbursementTransaction that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbursementTransaction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the REFERENCE_NUMBER attribute.</summary>
  public const int ReferenceNumber_MaxLength = 14;

  /// <summary>
  /// The value of the REFERENCE_NUMBER attribute.
  /// This is a denormalized attribute that comes from cash management.  This 
  /// attribute is set using the action block to determine the reference number:
  /// FN_SET_REFERENCE_NUMBER.
  /// This attribute aids in the sorting of a number of on-line screens.	
  /// </summary>
  [JsonPropertyName("referenceNumber")]
  [Member(Index = 1, Type = MemberType.Varchar, Length
    = ReferenceNumber_MaxLength, Optional = true)]
  public string ReferenceNumber
  {
    get => Get<string>("referenceNumber");
    set =>
      Set("referenceNumber", Substring(value, 1, ReferenceNumber_MaxLength));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => Get<int?>("systemGeneratedIdentifier") ?? 0;
    set => Set("systemGeneratedIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the disbursement transaction type.
  /// Permitted Values:
  /// 	- disb collection
  /// 	- disbursement fee
  /// 	- disbursement
  /// 	- passthru
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("C")]
  [Value("F")]
  [Value("D")]
  [Value("P")]
  [Value("X")]
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
  /// The value of the AMOUNT attribute.
  /// Amount of the obligation transaction.
  /// Examples: Amount of a debt, refund, collection or adjustment.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => Get<decimal?>("amount") ?? 0M;
    set => Set("amount", value == 0M ? null : Truncate(value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// This date represents the deactivation of a obligation transaction.  This 
  /// will be caused by a retroactive collection being applied to a debt, thus
  /// all obligation transactions from that point on will be deactivated and
  /// reapplied.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? ProcessDate
  {
    get => Get<DateTime?>("processDate");
    set => Set("processDate", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATE_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTmst
  {
    get => Get<DateTime?>("lastUpdateTmst");
    set => Set("lastUpdateTmst", value);
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// DENORMALIZED from CASH RECEIPT DETAIL
  /// The date of the collection.  We get this date from the Collection 
  /// Obligation_Transaction and they get it from the Cash_Reciept_Detail.  This
  /// is the date that the payment was actually made.
  /// Example:
  /// 1.  We receive an IWO from a employer for an AP on 3/1/95.  The date of 
  /// the paycheck that was garnished was 2/1/95 so the collection date is 2/1/
  /// 95.
  /// 2.  A court receives a payment from a AP on 10/1/95 and they do not get it
  /// to us until 10/14/95.  The collection date is 10/14/95.
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionDate
  {
    get => Get<DateTime?>("collectionDate");
    set => Set("collectionDate", value);
  }

  /// <summary>Length of the INTERSTATE_IND attribute.</summary>
  public const int InterstateInd_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_IND attribute.
  /// This indicator will show that the disbursement transaction is paying an 
  /// Interstate obligation.
  /// </summary>
  [JsonPropertyName("interstateInd")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = InterstateInd_MaxLength, Optional = true)]
  public string InterstateInd
  {
    get => Get<string>("interstateInd");
    set => Set(
      "interstateInd", TrimEnd(Substring(value, 1, InterstateInd_MaxLength)));
  }

  /// <summary>
  /// The value of the PASSTHRU_PROC_DATE attribute.
  /// This attribute specifies the date on which the Disbursement Transaction 
  /// was considered for processing the Passthru. It is set by the Batch
  /// Passthru process that computes and creates the passthru credits.
  /// </summary>
  [JsonPropertyName("passthruProcDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? PassthruProcDate
  {
    get => Get<DateTime?>("passthruProcDate");
    set => Set("passthruProcDate", value);
  }

  /// <summary>Length of the DESIGNATED_PAYEE attribute.</summary>
  public const int DesignatedPayee_MaxLength = 10;

  /// <summary>
  /// The value of the DESIGNATED_PAYEE attribute.
  /// This attribute contains the obligee person number or the designated payee 
  /// person number.The Disbursement Debits process will set it when it creates
  /// the disbursement debit transaction. The Warrant generation process uses
  /// this to group the disbursement debits by the payee.		
  /// </summary>
  [JsonPropertyName("designatedPayee")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = DesignatedPayee_MaxLength, Optional = true)]
  public string DesignatedPayee
  {
    get => Get<string>("designatedPayee");
    set => Set(
      "designatedPayee",
      TrimEnd(Substring(value, 1, DesignatedPayee_MaxLength)));
  }

  /// <summary>Length of the EXCESS_URA_IND attribute.</summary>
  public const int ExcessUraInd_MaxLength = 1;

  /// <summary>
  /// The value of the EXCESS_URA_IND attribute.
  /// This attribute indicates that the disbursement was an excess ura 
  /// disbursement. This is determined by looking at the &quot;state&quot; on
  /// the collection record. If that state is UP or UD or any other excess ura
  /// state then this indicator will be set to a &quot;Y&quot;.
  /// </summary>
  [JsonPropertyName("excessUraInd")]
  [Member(Index = 14, Type = MemberType.Char, Length = ExcessUraInd_MaxLength, Optional
    = true)]
  public string ExcessUraInd
  {
    get => Get<string>("excessUraInd");
    set => Set(
      "excessUraInd", TrimEnd(Substring(value, 1, ExcessUraInd_MaxLength)));
  }

  /// <summary>
  /// The value of the DISBURSEMENT_DATE attribute.
  /// *** DRAFT ***
  /// The date the disbursement is effective.
  /// </summary>
  [JsonPropertyName("disbursementDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? DisbursementDate
  {
    get => Get<DateTime?>("disbursementDate");
    set => Set("disbursementDate", value);
  }

  /// <summary>Length of the CASH_NON_CASH_IND attribute.</summary>
  public const int CashNonCashInd_MaxLength = 1;

  /// <summary>
  /// The value of the CASH_NON_CASH_IND attribute.
  /// * Draft *
  /// An indication of the kind of collection received.  Cash or Non-Cash.  A 
  /// non-cash collection means that we received information about a collection
  /// but we did not receive the money.  Receiving information about a 'direct
  /// pay' payment is an example of a non-cash collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CashNonCashInd_MaxLength)]
    
  [Value("C")]
  [Value("N")]
  [ImplicitValue("C")]
  public string CashNonCashInd
  {
    get => Get<string>("cashNonCashInd") ?? "";
    set => Set(
      "cashNonCashInd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CashNonCashInd_MaxLength)));
  }

  /// <summary>
  /// The json value of the CashNonCashInd attribute.</summary>
  [JsonPropertyName("cashNonCashInd")]
  [Computed]
  public string CashNonCashInd_Json
  {
    get => NullIf(CashNonCashInd, "");
    set => CashNonCashInd = value;
  }

  /// <summary>Length of the RECAPTURED_IND attribute.</summary>
  public const int RecapturedInd_MaxLength = 1;

  /// <summary>
  /// The value of the RECAPTURED_IND attribute.
  /// The recaptured indicator will tell if the disbursement has been recaptured
  /// either by a default recapture rule or a negotiated recapture.
  /// Valid values are Y and N.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = RecapturedInd_MaxLength)]
  public string RecapturedInd
  {
    get => Get<string>("recapturedInd") ?? "";
    set => Set(
      "recapturedInd", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, RecapturedInd_MaxLength)));
  }

  /// <summary>
  /// The json value of the RecapturedInd attribute.</summary>
  [JsonPropertyName("recapturedInd")]
  [Computed]
  public string RecapturedInd_Json
  {
    get => NullIf(RecapturedInd, "");
    set => RecapturedInd = value;
  }

  /// <summary>
  /// The value of the PASSTHRU_DATE attribute.
  /// The date that the passthru was created.
  /// </summary>
  [JsonPropertyName("passthruDate")]
  [Member(Index = 18, Type = MemberType.Date)]
  public DateTime? PassthruDate
  {
    get => Get<DateTime?>("passthruDate");
    set => Set("passthruDate", value);
  }

  /// <summary>
  /// The value of the COLLECTION_PROCESS_DATE attribute.
  /// The date of the collection was processed.
  /// </summary>
  [JsonPropertyName("collectionProcessDate")]
  [Member(Index = 19, Type = MemberType.Date)]
  public DateTime? CollectionProcessDate
  {
    get => Get<DateTime?>("collectionProcessDate");
    set => Set("collectionProcessDate", value);
  }

  /// <summary>
  /// The value of the URA_EXCESS_COLL_SEQ_NBR attribute.
  /// Foreign key to the Excess URA Collection. It is a system generated 
  /// identifier (created by adding 1 to the max value of the identifier found
  /// in the table).
  /// </summary>
  [JsonPropertyName("uraExcessCollSeqNbr")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 9)]
  public int UraExcessCollSeqNbr
  {
    get => Get<int?>("uraExcessCollSeqNbr") ?? 0;
    set => Set("uraExcessCollSeqNbr", value == 0 ? null : value as int?);
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
  [Member(Index = 21, Type = MemberType.Char, Length = CpaType_MaxLength)]
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
  [Member(Index = 22, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intInterId")]
  [Member(Index = 23, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? IntInterId
  {
    get => Get<int?>("intInterId");
    set => Set("intInterId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbtGeneratedId")]
  [Member(Index = 24, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? DbtGeneratedId
  {
    get => Get<int?>("dbtGeneratedId");
    set => Set("dbtGeneratedId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqGeneratedId")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrqGeneratedId
  {
    get => Get<int?>("prqGeneratedId");
    set => Set("prqGeneratedId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("colId")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ColId
  {
    get => Get<int?>("colId");
    set => Set("colId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 27, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => Get<int?>("otyId");
    set => Set("otyId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [Member(Index = 28, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgId
  {
    get => Get<int?>("obgId");
    set => Set("obgId", value);
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberDisb_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumberDisb")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = CspNumberDisb_MaxLength, Optional = true)]
  public string CspNumberDisb
  {
    get => Get<string>("cspNumberDisb");
    set => Set(
      "cspNumberDisb", TrimEnd(Substring(value, 1, CspNumberDisb_MaxLength)));
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaTypeDisb_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaTypeDisb")]
  [Member(Index = 30, Type = MemberType.Char, Length = CpaTypeDisb_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaTypeDisb
  {
    get => Get<string>("cpaTypeDisb");
    set => Set(
      "cpaTypeDisb", TrimEnd(Substring(value, 1, CpaTypeDisb_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrId")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtrId
  {
    get => Get<int?>("otrId");
    set => Set("otrId", value);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrTypeDisb_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonPropertyName("otrTypeDisb")]
  [Member(Index = 32, Type = MemberType.Char, Length = OtrTypeDisb_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrTypeDisb
  {
    get => Get<string>("otrTypeDisb");
    set => Set(
      "otrTypeDisb", TrimEnd(Substring(value, 1, OtrTypeDisb_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtId")]
  [Member(Index = 33, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CrtId
  {
    get => Get<int?>("crtId");
    set => Set("crtId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstId")]
  [Member(Index = 34, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CstId
  {
    get => Get<int?>("cstId");
    set => Set("cstId", value);
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
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CrvId
  {
    get => Get<int?>("crvId");
    set => Set("crvId", value);
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdId")]
  [Member(Index = 36, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? CrdId
  {
    get => Get<int?>("crdId");
    set => Set("crdId", value);
  }
}

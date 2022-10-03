// The source file: OBLIGATION_TRANSACTION, ID: 371438179, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINAN
/// Records common detail information of financial transactions created for a 
/// monetary obligation, and relates together those entities which further
/// detail the transaction.
/// Examples: date, amounts, who created the transaction and the timestamp and 
/// who last updated the transaction and the timestamp.	
/// </summary>
[Serializable]
public partial class ObligationTransaction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationTransaction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationTransaction(ObligationTransaction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationTransaction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationTransaction that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    type1 = that.type1;
    amount = that.amount;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    zdelPreconversionReceiptNum = that.zdelPreconversionReceiptNum;
    zdelPreconversionIsn = that.zdelPreconversionIsn;
    debtAdjCollAdjProcReqInd = that.debtAdjCollAdjProcReqInd;
    debtAdjCollAdjProcDt = that.debtAdjCollAdjProcDt;
    debtAdjustmentType = that.debtAdjustmentType;
    debtAdjustmentDt = that.debtAdjustmentDt;
    debtAdjustmentProcessDate = that.debtAdjustmentProcessDate;
    reasonCode = that.reasonCode;
    reverseCollectionsInd = that.reverseCollectionsInd;
    debtType = that.debtType;
    debtAdjustmentInd = that.debtAdjustmentInd;
    voluntaryPercentageAmount = that.voluntaryPercentageAmount;
    newDebtProcessDate = that.newDebtProcessDate;
    uraUpdateProcDate = that.uraUpdateProcDate;
    lapId = that.lapId;
    otyType = that.otyType;
    obgGeneratedId = that.obgGeneratedId;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
    cpaSupType = that.cpaSupType;
    cspSupNumber = that.cspSupNumber;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
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
  /// Examples: Amount of a debt or debt adjustment.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the ZDEL_PRECONVERSION_RECEIPT_NUM attribute.
  /// The receipt number from the KAECSES system.  Used during system 
  /// conversion.
  /// </summary>
  [JsonPropertyName("zdelPreconversionReceiptNum")]
  [Member(Index = 8, Type = MemberType.Number, Length = 12, Optional = true)]
  public long? ZdelPreconversionReceiptNum
  {
    get => zdelPreconversionReceiptNum;
    set => zdelPreconversionReceiptNum = value;
  }

  /// <summary>
  /// The value of the ZDEL_PRECONVERSION_ISN attribute.
  /// The internal sequence number (ISN) from adabase which corresponds to this 
  /// converted record to the receipt history DBF record.
  /// Only used for conversion.
  /// </summary>
  [JsonPropertyName("zdelPreconversionIsn")]
  [Member(Index = 9, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? ZdelPreconversionIsn
  {
    get => zdelPreconversionIsn;
    set => zdelPreconversionIsn = value;
  }

  /// <summary>Length of the DEBT_ADJ_COLL_ADJ_PROC_REQ_IND attribute.</summary>
  public const int DebtAdjCollAdjProcReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_ADJ_COLL_ADJ_PROC_REQ_IND attribute.
  /// This indicator defines whether or not collection adjustment processing is 
  /// required against the debt.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = DebtAdjCollAdjProcReqInd_MaxLength)]
  [Value("N")]
  [Value("Y")]
  public string DebtAdjCollAdjProcReqInd
  {
    get => debtAdjCollAdjProcReqInd ?? "";
    set => debtAdjCollAdjProcReqInd =
      TrimEnd(Substring(value, 1, DebtAdjCollAdjProcReqInd_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtAdjCollAdjProcReqInd attribute.</summary>
  [JsonPropertyName("debtAdjCollAdjProcReqInd")]
  [Computed]
  public string DebtAdjCollAdjProcReqInd_Json
  {
    get => NullIf(DebtAdjCollAdjProcReqInd, "");
    set => DebtAdjCollAdjProcReqInd = value;
  }

  /// <summary>
  /// The value of the DEBT_ADJ_COLL_ADJ_PROC_DT attribute.
  /// Defines the date that collection adjustment processing has occurred.
  /// </summary>
  [JsonPropertyName("debtAdjCollAdjProcDt")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? DebtAdjCollAdjProcDt
  {
    get => debtAdjCollAdjProcDt;
    set => debtAdjCollAdjProcDt = value;
  }

  /// <summary>Length of the DEBT_ADJUSTMENT_TYPE attribute.</summary>
  public const int DebtAdjustmentType_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_ADJUSTMENT_TYPE attribute.
  /// Determines whether the Debt Adjustment increases or decreases the debt.
  ///   - I : Increase
  ///   - D : Decrease
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = DebtAdjustmentType_MaxLength)]
  [Value("D")]
  [Value("I")]
  [ImplicitValue("I")]
  public string DebtAdjustmentType
  {
    get => debtAdjustmentType ?? "";
    set => debtAdjustmentType =
      TrimEnd(Substring(value, 1, DebtAdjustmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtAdjustmentType attribute.</summary>
  [JsonPropertyName("debtAdjustmentType")]
  [Computed]
  public string DebtAdjustmentType_Json
  {
    get => NullIf(DebtAdjustmentType, "");
    set => DebtAdjustmentType = value;
  }

  /// <summary>
  /// The value of the DEBT_ADJUSTMENT_DT attribute.
  /// The date the Debt Adjustment was applied to a specific Debt.
  /// </summary>
  [JsonPropertyName("debtAdjustmentDt")]
  [Member(Index = 13, Type = MemberType.Date)]
  public DateTime? DebtAdjustmentDt
  {
    get => debtAdjustmentDt;
    set => debtAdjustmentDt = value;
  }

  /// <summary>
  /// The value of the DEBT ADJUSTMENT PROCESS DATE attribute.
  /// The date the Debt Adjustment was applied to a specific Debt.
  /// </summary>
  [JsonPropertyName("debtAdjustmentProcessDate")]
  [Member(Index = 14, Type = MemberType.Date)]
  public DateTime? DebtAdjustmentProcessDate
  {
    get => debtAdjustmentProcessDate;
    set => debtAdjustmentProcessDate = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 10;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = ReasonCode_MaxLength)]
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

  /// <summary>Length of the REVERSE_COLLECTIONS_IND attribute.</summary>
  public const int ReverseCollectionsInd_MaxLength = 1;

  /// <summary>
  /// The value of the REVERSE_COLLECTIONS_IND attribute.
  /// Indicates if the user specified that all collections associated with this 
  /// debt adjustment are supposed to be reversed. Values will be Y or N.
  /// </summary>
  [JsonPropertyName("reverseCollectionsInd")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ReverseCollectionsInd_MaxLength, Optional = true)]
  public string ReverseCollectionsInd
  {
    get => reverseCollectionsInd;
    set => reverseCollectionsInd = value != null
      ? TrimEnd(Substring(value, 1, ReverseCollectionsInd_MaxLength)) : null;
  }

  /// <summary>Length of the DEBT_TYPE attribute.</summary>
  public const int DebtType_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_TYPE attribute.
  /// Defines the type of debt.
  /// Example:
  ///   - Debt Detail
  ///   - Accruing Instructions
  ///   - Voluntary Debt
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = DebtType_MaxLength)]
  [Value("D")]
  [Value("V")]
  [Value("A")]
  [Value("H")]
  public string DebtType
  {
    get => debtType ?? "";
    set => debtType = TrimEnd(Substring(value, 1, DebtType_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtType attribute.</summary>
  [JsonPropertyName("debtType")]
  [Computed]
  public string DebtType_Json
  {
    get => NullIf(DebtType, "");
    set => DebtType = value;
  }

  /// <summary>Length of the DEBT_ADJUSTMENT_IND attribute.</summary>
  public const int DebtAdjustmentInd_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_ADJUSTMENT_IND attribute.
  /// Indicate whether or not a debt adjustment has been applied to this debt.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = DebtAdjustmentInd_MaxLength)]
  [Value("Y")]
  [Value("N")]
  [ImplicitValue("N")]
  public string DebtAdjustmentInd
  {
    get => debtAdjustmentInd ?? "";
    set => debtAdjustmentInd =
      TrimEnd(Substring(value, 1, DebtAdjustmentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtAdjustmentInd attribute.</summary>
  [JsonPropertyName("debtAdjustmentInd")]
  [Computed]
  public string DebtAdjustmentInd_Json
  {
    get => NullIf(DebtAdjustmentInd, "");
    set => DebtAdjustmentInd = value;
  }

  /// <summary>
  /// The value of the VOLUNTARY_PERCENTAGE_AMOUNT attribute.
  /// Defines the percentage amount of any voluntary payments that will be 
  /// distributed to each child.
  /// </summary>
  [JsonPropertyName("voluntaryPercentageAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? VoluntaryPercentageAmount
  {
    get => voluntaryPercentageAmount;
    set => voluntaryPercentageAmount = value;
  }

  /// <summary>
  /// The value of the NEW_DEBT_PROCESS_DATE attribute.
  /// This date is used to control the processing of debts and collections when 
  /// new debts are created retroactively.
  /// </summary>
  [JsonPropertyName("newDebtProcessDate")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? NewDebtProcessDate
  {
    get => newDebtProcessDate;
    set => newDebtProcessDate = value;
  }

  /// <summary>
  /// The value of the URA_UPDATE_PROC_DATE attribute.
  /// This attribute contains the date that the debt was processed to determine 
  /// if URA data needed to be retrieved from AE and a
  /// IM_HOUSEHOLD_MBR_MNTHLY_SUM record should be created or updated. Once this
  /// date is set by one of the 2 processes that does this check (SWEEB466 or
  /// SWEEB467) then this debt does not need to be processed again for its
  /// impact on URA
  /// </summary>
  [JsonPropertyName("uraUpdateProcDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? UraUpdateProcDate
  {
    get => uraUpdateProcDate;
    set => uraUpdateProcDate = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("lapId")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LapId
  {
    get => lapId;
    set => lapId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [DefaultValue(0)]
  [Member(Index = 23, Type = MemberType.Number, Length = 3)]
  public int OtyType
  {
    get => otyType;
    set => otyType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 24, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
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
  [Member(Index = 25, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
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
  [Member(Index = 26, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaSupType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaSupType")]
  [Member(Index = 27, Type = MemberType.Char, Length = CpaSupType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaSupType
  {
    get => cpaSupType;
    set => cpaSupType = value != null
      ? TrimEnd(Substring(value, 1, CpaSupType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspSupNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspSupNumber")]
  [Member(Index = 28, Type = MemberType.Char, Length = CspSupNumber_MaxLength, Optional
    = true)]
  public string CspSupNumber
  {
    get => cspSupNumber;
    set => cspSupNumber = value != null
      ? TrimEnd(Substring(value, 1, CspSupNumber_MaxLength)) : null;
  }

  private int systemGeneratedIdentifier;
  private string type1;
  private decimal amount;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private long? zdelPreconversionReceiptNum;
  private long? zdelPreconversionIsn;
  private string debtAdjCollAdjProcReqInd;
  private DateTime? debtAdjCollAdjProcDt;
  private string debtAdjustmentType;
  private DateTime? debtAdjustmentDt;
  private DateTime? debtAdjustmentProcessDate;
  private string reasonCode;
  private string reverseCollectionsInd;
  private string debtType;
  private string debtAdjustmentInd;
  private int? voluntaryPercentageAmount;
  private DateTime? newDebtProcessDate;
  private DateTime? uraUpdateProcDate;
  private int? lapId;
  private int otyType;
  private int obgGeneratedId;
  private string cspNumber;
  private string cpaType;
  private string cpaSupType;
  private string cspSupNumber;
}

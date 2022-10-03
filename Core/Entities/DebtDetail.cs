// The source file: DEBT_DETAIL, ID: 371433430, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Represents a amount due on a specific date.  Set Amount Debt's are tied to a
/// supported person through the program that is currently active (unless ADC,
/// in which the debt remains tied to the ADC program for that supported person
/// ).
/// Examples:
///   - Arrearage Judgement
///   - Recovery Debt
///   - A monthly accrued amount due based on the
///     accruing instructions.
/// </summary>
[Serializable]
public partial class DebtDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DebtDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DebtDetail(DebtDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DebtDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DebtDetail that)
  {
    base.Assign(that);
    dueDt = that.dueDt;
    balanceDueAmt = that.balanceDueAmt;
    interestBalanceDueAmt = that.interestBalanceDueAmt;
    adcDt = that.adcDt;
    retiredDt = that.retiredDt;
    coveredPrdStartDt = that.coveredPrdStartDt;
    coveredPrdEndDt = that.coveredPrdEndDt;
    preconversionProgramCode = that.preconversionProgramCode;
    createdTmst = that.createdTmst;
    createdBy = that.createdBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    otyType = that.otyType;
    obgGeneratedId = that.obgGeneratedId;
    otrType = that.otrType;
    otrGeneratedId = that.otrGeneratedId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the DUE_DT attribute.
  /// The date the payment is due and payable for that occurence of SET AMOUNT 
  /// DEBT.  Set amount debts not collected by this date will be considered past
  /// due.
  /// </summary>
  [JsonPropertyName("dueDt")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? DueDt
  {
    get => dueDt;
    set => dueDt = value;
  }

  /// <summary>
  /// The value of the BALANCE_DUE_AMT attribute.
  /// The amount remaining after all collections and adjustments have been 
  /// applied.  When the Balance Due amount is zero, the debt is retired.
  /// </summary>
  [JsonPropertyName("balanceDueAmt")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal BalanceDueAmt
  {
    get => balanceDueAmt;
    set => balanceDueAmt = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the INTEREST_BALANCE_DUE_AMT attribute.
  /// Represents the total interest balance due for this specific debt.
  /// </summary>
  [JsonPropertyName("interestBalanceDueAmt")]
  [Member(Index = 3, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? InterestBalanceDueAmt
  {
    get => interestBalanceDueAmt;
    set => interestBalanceDueAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ADC_DT attribute.
  /// The date the debt program type was changed to ADC.  At this time this debt
  /// would have been assigned to ADC and would be owed to the state from this
  /// point on regardless of other program changes that may occurr in the
  /// future.
  /// </summary>
  [JsonPropertyName("adcDt")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? AdcDt
  {
    get => adcDt;
    set => adcDt = value;
  }

  /// <summary>
  /// The value of the RETIRED_DT attribute.
  /// The date in which the debt was completely paid or adjusted off.  Balance 
  /// Due must be zero.
  /// </summary>
  [JsonPropertyName("retiredDt")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? RetiredDt
  {
    get => retiredDt;
    set => retiredDt = value;
  }

  /// <summary>
  /// The value of the COVERED_PRD_START_DT attribute.
  /// The beginning date for the period of time for which support is being 
  /// collected.
  /// Example:  A debt can be created 1-1-95 to cover support for the period of 
  /// 6-1-90 thru 12-31-94.  The covered period start date for this occurrence
  /// is 6-1-90.
  /// </summary>
  [JsonPropertyName("coveredPrdStartDt")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? CoveredPrdStartDt
  {
    get => coveredPrdStartDt;
    set => coveredPrdStartDt = value;
  }

  /// <summary>
  /// The value of the COVERED_PRD_END_DT attribute.
  /// The ending date for a period of time for which support is being collected.
  /// Example:  A debt is established 1-1-95 for the period of 6-1-90 thru 12-31
  /// -94. The covered period end date for this occurrence is 12-31-94.
  /// </summary>
  [JsonPropertyName("coveredPrdEndDt")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? CoveredPrdEndDt
  {
    get => coveredPrdEndDt;
    set => coveredPrdEndDt = value;
  }

  /// <summary>Length of the PRECONVERSION_PROGRAM_CODE attribute.</summary>
  public const int PreconversionProgramCode_MaxLength = 3;

  /// <summary>
  /// The value of the PRECONVERSION_PROGRAM_CODE attribute.
  /// The KAECSES program code recorded on this debt at time of conversion. Only
  /// used for conversion purposes.
  /// </summary>
  [JsonPropertyName("preconversionProgramCode")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = PreconversionProgramCode_MaxLength, Optional = true)]
  public string PreconversionProgramCode
  {
    get => preconversionProgramCode;
    set => preconversionProgramCode = value != null
      ? TrimEnd(Substring(value, 1, PreconversionProgramCode_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the CREATED_TMST attribute.
  /// The clock time the entity type was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Logon ID of the user that executed the CICS transaction that created the 
  /// entity; or, the name of the batch job that created the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The clock time the entity type was created.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// Logon ID of the user that executed the cics transaction that created the 
  /// entity; or, the name of the batch job that updated the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 14, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
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
  [Member(Index = 15, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType ?? "";
    set => otrType = TrimEnd(Substring(value, 1, OtrType_MaxLength));
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
  [JsonPropertyName("otrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 9)]
  public int OtrGeneratedId
  {
    get => otrGeneratedId;
    set => otrGeneratedId = value;
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
  [Member(Index = 17, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private DateTime? dueDt;
  private decimal balanceDueAmt;
  private decimal? interestBalanceDueAmt;
  private DateTime? adcDt;
  private DateTime? retiredDt;
  private DateTime? coveredPrdStartDt;
  private DateTime? coveredPrdEndDt;
  private string preconversionProgramCode;
  private DateTime? createdTmst;
  private string createdBy;
  private DateTime? lastUpdatedTmst;
  private string lastUpdatedBy;
  private int otyType;
  private int obgGeneratedId;
  private string otrType;
  private int otrGeneratedId;
  private string cpaType;
  private string cspNumber;
}

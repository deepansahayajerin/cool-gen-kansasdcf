// The source file: CHILD_CARE_TAX_CREDIT_FACTORS, ID: 371432187, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// A FEDERAL TAX CREDIT PERCENT THAT IS PULLED FROM THE FEDERAL CHILD CARE 
/// CREDIT TABLE BASED ON THE ADJ GROSS INCOME.
/// </summary>
[Serializable]
public partial class ChildCareTaxCreditFactors: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ChildCareTaxCreditFactors()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ChildCareTaxCreditFactors(ChildCareTaxCreditFactors that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ChildCareTaxCreditFactors Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ChildCareTaxCreditFactors that)
  {
    base.Assign(that);
    expirationDate = that.expirationDate;
    effectiveDate = that.effectiveDate;
    identifier = that.identifier;
    adjustedGrossIncomeMaximum = that.adjustedGrossIncomeMaximum;
    adjustedGrossIncomeMinimum = that.adjustedGrossIncomeMinimum;
    kansasTaxCreditPercent = that.kansasTaxCreditPercent;
    federalTaxCreditPercent = that.federalTaxCreditPercent;
    maxMonthlyCreditMultChildren = that.maxMonthlyCreditMultChildren;
    maxMonthlyCredit1Child = that.maxMonthlyCredit1Child;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// The date that the Child Care Credit Table expires.
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ExpirationDate
  {
    get => expirationDate;
    set => expirationDate = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date that the Child Care Credit Table goes into effect.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique number that descripts a particular line within the table.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the ADJUSTED_GROSS_INCOME_MAXIMUM attribute.
  /// Adjusted Gross income equals total annual income of the party incurring 
  /// the child care costs less: reimbursed employee business expense;
  /// deductible IRA, KEOGH, and SEP contributions; self-employed health
  /// insurance deduction; penalty on early withdrawal of savings; and alimony
  /// paid to another party.
  /// </summary>
  [JsonPropertyName("adjustedGrossIncomeMaximum")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 6)]
  public int AdjustedGrossIncomeMaximum
  {
    get => adjustedGrossIncomeMaximum;
    set => adjustedGrossIncomeMaximum = value;
  }

  /// <summary>
  /// The value of the ADJUSTED_GROSS_INCOME_MINIMUM attribute.
  /// The total annual income of the CSE Person incurring the child care cost 
  /// less reimbursed employee business expense; deductible IRA, Keogh, and SEP
  /// contributions; self-employed health insurance deduction; penalty on early
  /// withdrawal of savings; and alimony paid to another party.
  /// </summary>
  [JsonPropertyName("adjustedGrossIncomeMinimum")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 6)]
  public int AdjustedGrossIncomeMinimum
  {
    get => adjustedGrossIncomeMinimum;
    set => adjustedGrossIncomeMinimum = value;
  }

  /// <summary>
  /// The value of the KANSAS_TAX_CREDIT_PERCENT attribute.
  /// IN ADDITION TO THE FEDERAL CREDIT, A CREDIT SHALL BE APPLIED BASED ON THE 
  /// KANSAS CHILD CARE CREDIT. AT THE PRESENT TIME (10-94) THIS CREDIT SHALL BE
  /// APPLIED BY MULTIPLYING THE FEDERAL CREDIT CALCULATION BY 25%.
  /// </summary>
  [JsonPropertyName("kansasTaxCreditPercent")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal KansasTaxCreditPercent
  {
    get => kansasTaxCreditPercent;
    set => kansasTaxCreditPercent = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the FEDERAL_TAX_CREDIT_PERCENT attribute.
  /// A FEDERAL TAX CREDIT PERCENT THAT IS PULLED FROM THE FED CHILD CARE CREDIT
  /// TABLE BASED ON THE ADJ GROSS INCOME.
  /// </summary>
  [JsonPropertyName("federalTaxCreditPercent")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal FederalTaxCreditPercent
  {
    get => federalTaxCreditPercent;
    set => federalTaxCreditPercent = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MAX_MONTHLY_CREDIT_MULT_CHILDREN attribute.
  /// The maximum monthly amount is $400 per month for two or more children 
  /// receiving child care. See child care cost table.
  /// </summary>
  [JsonPropertyName("maxMonthlyCreditMultChildren")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 4)]
  public int MaxMonthlyCreditMultChildren
  {
    get => maxMonthlyCreditMultChildren;
    set => maxMonthlyCreditMultChildren = value;
  }

  /// <summary>
  /// The value of the MAX_MONTHLY_CREDIT_1_CHILD attribute.
  /// The maximum monthly amount is $200 per month for one child receiving child
  /// care. see child care cost table.
  /// </summary>
  [JsonPropertyName("maxMonthlyCredit1Child")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 4)]
  public int MaxMonthlyCredit1Child
  {
    get => maxMonthlyCredit1Child;
    set => maxMonthlyCredit1Child = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private DateTime? expirationDate;
  private DateTime? effectiveDate;
  private int identifier;
  private int adjustedGrossIncomeMaximum;
  private int adjustedGrossIncomeMinimum;
  private decimal kansasTaxCreditPercent;
  private decimal federalTaxCreditPercent;
  private int maxMonthlyCreditMultChildren;
  private int maxMonthlyCredit1Child;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}

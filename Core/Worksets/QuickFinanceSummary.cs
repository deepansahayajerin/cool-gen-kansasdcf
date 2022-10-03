// The source file: QUICK_FINANCE_SUMMARY, ID: 374543738, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickFinanceSummary: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickFinanceSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickFinanceSummary(QuickFinanceSummary that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickFinanceSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickFinanceSummary that)
  {
    base.Assign(that);
    monthlySupportAmount = that.monthlySupportAmount;
    monthlyArrearsAmount = that.monthlyArrearsAmount;
    otherMonthlyAmount = that.otherMonthlyAmount;
    totalMonthlyAmount = that.totalMonthlyAmount;
    lastPaymentAmount = that.lastPaymentAmount;
    lastPaymentDate = that.lastPaymentDate;
    totalArrearsOwed = that.totalArrearsOwed;
    totalInterestOwed = that.totalInterestOwed;
    totalNcpFeesOwed = that.totalNcpFeesOwed;
    totalJudgmentAmount = that.totalJudgmentAmount;
    totalAssignedArrears = that.totalAssignedArrears;
    totalOwedAmount = that.totalOwedAmount;
  }

  /// <summary>
  /// The value of the MONTHLY_SUPPORT_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("monthlySupportAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal MonthlySupportAmount
  {
    get => monthlySupportAmount;
    set => monthlySupportAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MONTHLY_ARREARS_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("monthlyArrearsAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal MonthlyArrearsAmount
  {
    get => monthlyArrearsAmount;
    set => monthlyArrearsAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the OTHER_MONTHLY_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("otherMonthlyAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal OtherMonthlyAmount
  {
    get => otherMonthlyAmount;
    set => otherMonthlyAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_MONTHLY_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("totalMonthlyAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalMonthlyAmount
  {
    get => totalMonthlyAmount;
    set => totalMonthlyAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the LAST_PAYMENT_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("lastPaymentAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal LastPaymentAmount
  {
    get => lastPaymentAmount;
    set => lastPaymentAmount = Truncate(value, 2);
  }

  /// <summary>Length of the LAST_PAYMENT_DATE attribute.</summary>
  public const int LastPaymentDate_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_PAYMENT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastPaymentDate_MaxLength)]
    
  public string LastPaymentDate
  {
    get => lastPaymentDate ?? "";
    set => lastPaymentDate =
      TrimEnd(Substring(value, 1, LastPaymentDate_MaxLength));
  }

  /// <summary>
  /// The json value of the LastPaymentDate attribute.</summary>
  [JsonPropertyName("lastPaymentDate")]
  [Computed]
  public string LastPaymentDate_Json
  {
    get => NullIf(LastPaymentDate, "");
    set => LastPaymentDate = value;
  }

  /// <summary>
  /// The value of the TOTAL_ARREARS_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalArrearsOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalArrearsOwed
  {
    get => totalArrearsOwed;
    set => totalArrearsOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_INTEREST_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalInterestOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalInterestOwed
  {
    get => totalInterestOwed;
    set => totalInterestOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_NCP_FEES_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalNcpFeesOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalNcpFeesOwed
  {
    get => totalNcpFeesOwed;
    set => totalNcpFeesOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_JUDGMENT_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("totalJudgmentAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 10, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalJudgmentAmount
  {
    get => totalJudgmentAmount;
    set => totalJudgmentAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_ASSIGNED_ARREARS attribute.
  /// </summary>
  [JsonPropertyName("totalAssignedArrears")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 11, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalAssignedArrears
  {
    get => totalAssignedArrears;
    set => totalAssignedArrears = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_OWED_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("totalOwedAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 12, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal TotalOwedAmount
  {
    get => totalOwedAmount;
    set => totalOwedAmount = Truncate(value, 2);
  }

  private decimal monthlySupportAmount;
  private decimal monthlyArrearsAmount;
  private decimal otherMonthlyAmount;
  private decimal totalMonthlyAmount;
  private decimal lastPaymentAmount;
  private string lastPaymentDate;
  private decimal totalArrearsOwed;
  private decimal totalInterestOwed;
  private decimal totalNcpFeesOwed;
  private decimal totalJudgmentAmount;
  private decimal totalAssignedArrears;
  private decimal totalOwedAmount;
}

// The source file: SMART_TRANSACTION_ENTRY, ID: 371440301, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// To store information releveant to producing stars vouchers, which are use to
/// record into the statewide accounting system the proper accounting
/// distribution of kessep daily child support payment expenses to the agency's
/// budgetary accounts and to record the amount of kessep daily child support
/// payments written outside the daily statewide accountion system.
/// </summary>
[Serializable]
public partial class SmartTransactionEntry: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SmartTransactionEntry()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SmartTransactionEntry(SmartTransactionEntry that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SmartTransactionEntry Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SmartTransactionEntry that)
  {
    base.Assign(that);
    smartClassType = that.smartClassType;
    finYr = that.finYr;
    suffix1 = that.suffix1;
    businessUnit = that.businessUnit;
    fundCode = that.fundCode;
    programCode = that.programCode;
    deptId = that.deptId;
    accountNumber = that.accountNumber;
    budgetUnit = that.budgetUnit;
    smartR = that.smartR;
  }

  /// <summary>Length of the SMART_CLASS_TYPE attribute.</summary>
  public const int SmartClassType_MaxLength = 10;

  /// <summary>
  /// The value of the SMART_CLASS_TYPE attribute.
  /// IDENTIFIES SPECIFIC TYPE OF CLASSIFICATION FOR EACH STARS VOUCHER
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SmartClassType_MaxLength)]
  public string SmartClassType
  {
    get => smartClassType ?? "";
    set => smartClassType =
      TrimEnd(Substring(value, 1, SmartClassType_MaxLength));
  }

  /// <summary>
  /// The json value of the SmartClassType attribute.</summary>
  [JsonPropertyName("smartClassType")]
  [Computed]
  public string SmartClassType_Json
  {
    get => NullIf(SmartClassType, "");
    set => SmartClassType = value;
  }

  /// <summary>Length of the FIN_YR attribute.</summary>
  public const int FinYr_MaxLength = 2;

  /// <summary>
  /// The value of the FIN_YR attribute.
  /// IDENTIFIES FINANCIAL YEAR OF A STARS VOUCHER
  /// </summary>
  [JsonPropertyName("finYr")]
  [Member(Index = 2, Type = MemberType.Char, Length = FinYr_MaxLength, Optional
    = true)]
  public string FinYr
  {
    get => finYr;
    set => finYr = value != null
      ? TrimEnd(Substring(value, 1, FinYr_MaxLength)) : null;
  }

  /// <summary>Length of the SUFFIX_1 attribute.</summary>
  public const int Suffix1_MaxLength = 2;

  /// <summary>
  /// The value of the SUFFIX_1 attribute.
  /// IDENTIFIES A SUFFIX FOR A STARS VOUCHER
  /// </summary>
  [JsonPropertyName("suffix1")]
  [Member(Index = 3, Type = MemberType.Char, Length = Suffix1_MaxLength, Optional
    = true)]
  public string Suffix1
  {
    get => suffix1;
    set => suffix1 = value != null
      ? TrimEnd(Substring(value, 1, Suffix1_MaxLength)) : null;
  }

  /// <summary>Length of the BUSINESS_UNIT attribute.</summary>
  public const int BusinessUnit_MaxLength = 5;

  /// <summary>
  /// The value of the BUSINESS_UNIT attribute.
  /// IDENTIFIES THE AGENCY CODE FOR A STARS VOUCHER
  /// </summary>
  [JsonPropertyName("businessUnit")]
  [Member(Index = 4, Type = MemberType.Char, Length = BusinessUnit_MaxLength, Optional
    = true)]
  public string BusinessUnit
  {
    get => businessUnit;
    set => businessUnit = value != null
      ? TrimEnd(Substring(value, 1, BusinessUnit_MaxLength)) : null;
  }

  /// <summary>Length of the FUND_CODE attribute.</summary>
  public const int FundCode_MaxLength = 4;

  /// <summary>
  /// The value of the FUND_CODE attribute.
  /// IDENTIFIES THE FUND CODE FOR A STARS VOUCHER
  /// </summary>
  [JsonPropertyName("fundCode")]
  [Member(Index = 5, Type = MemberType.Char, Length = FundCode_MaxLength, Optional
    = true)]
  public string FundCode
  {
    get => fundCode;
    set => fundCode = value != null
      ? TrimEnd(Substring(value, 1, FundCode_MaxLength)) : null;
  }

  /// <summary>Length of the PROGRAM_CODE attribute.</summary>
  public const int ProgramCode_MaxLength = 5;

  /// <summary>
  /// The value of the PROGRAM_CODE attribute.
  /// IDENTIFIES THE INDEX FOR A STARS VOUCHER
  /// </summary>
  [JsonPropertyName("programCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = ProgramCode_MaxLength, Optional
    = true)]
  public string ProgramCode
  {
    get => programCode;
    set => programCode = value != null
      ? TrimEnd(Substring(value, 1, ProgramCode_MaxLength)) : null;
  }

  /// <summary>Length of the DEPT_ID attribute.</summary>
  public const int DeptId_MaxLength = 10;

  /// <summary>
  /// The value of the DEPT_ID attribute.
  /// IDENTIFIES THE PCA_CODE FOR A STARS VOUCHER. STARS VOUCHER USAGE AND 
  /// DESCRIPTION CURRENTLY UNKNOWN
  /// </summary>
  [JsonPropertyName("deptId")]
  [Member(Index = 7, Type = MemberType.Char, Length = DeptId_MaxLength, Optional
    = true)]
  public string DeptId
  {
    get => deptId;
    set => deptId = value != null
      ? TrimEnd(Substring(value, 1, DeptId_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_NUMBER attribute.</summary>
  public const int AccountNumber_MaxLength = 6;

  /// <summary>
  /// The value of the ACCOUNT_NUMBER attribute.
  /// STARS VOUCHER USAGE AND DESCRIPTION CURRENTLY UNKNOWN.
  /// </summary>
  [JsonPropertyName("accountNumber")]
  [Member(Index = 8, Type = MemberType.Char, Length = AccountNumber_MaxLength, Optional
    = true)]
  public string AccountNumber
  {
    get => accountNumber;
    set => accountNumber = value != null
      ? TrimEnd(Substring(value, 1, AccountNumber_MaxLength)) : null;
  }

  /// <summary>Length of the BUDGET_UNIT attribute.</summary>
  public const int BudgetUnit_MaxLength = 4;

  /// <summary>
  /// The value of the BUDGET_UNIT attribute.
  /// STARS VOUCHER USAGE AND DESCRIPTION CURRENTLY UNKNOWN
  /// </summary>
  [JsonPropertyName("budgetUnit")]
  [Member(Index = 9, Type = MemberType.Char, Length = BudgetUnit_MaxLength, Optional
    = true)]
  public string BudgetUnit
  {
    get => budgetUnit;
    set => budgetUnit = value != null
      ? TrimEnd(Substring(value, 1, BudgetUnit_MaxLength)) : null;
  }

  /// <summary>Length of the SMART_R attribute.</summary>
  public const int SmartR_MaxLength = 1;

  /// <summary>
  /// The value of the SMART_R attribute.
  /// STARS VOUCHER USAGE AND DESCRIPTION CURRENTLY UNKNOWN
  /// </summary>
  [JsonPropertyName("smartR")]
  [Member(Index = 10, Type = MemberType.Char, Length = SmartR_MaxLength, Optional
    = true)]
  public string SmartR
  {
    get => smartR;
    set => smartR = value != null
      ? TrimEnd(Substring(value, 1, SmartR_MaxLength)) : null;
  }

  private string smartClassType;
  private string finYr;
  private string suffix1;
  private string businessUnit;
  private string fundCode;
  private string programCode;
  private string deptId;
  private string accountNumber;
  private string budgetUnit;
  private string smartR;
}

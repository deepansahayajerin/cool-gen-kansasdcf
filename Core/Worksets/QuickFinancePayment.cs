// The source file: QUICK_FINANCE_PAYMENT, ID: 374543734, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickFinancePayment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickFinancePayment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickFinancePayment(QuickFinancePayment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickFinancePayment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickFinancePayment that)
  {
    base.Assign(that);
    date = that.date;
    amount = that.amount;
    sourceCode = that.sourceCode;
  }

  /// <summary>
  /// The value of the DATE attribute.
  /// </summary>
  [JsonPropertyName("date")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? Date
  {
    get => date;
    set => date = value;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the SOURCE_CODE attribute.</summary>
  public const int SourceCode_MaxLength = 3;

  /// <summary>
  /// The value of the SOURCE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SourceCode_MaxLength)]
  public string SourceCode
  {
    get => sourceCode ?? "";
    set => sourceCode = TrimEnd(Substring(value, 1, SourceCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SourceCode attribute.</summary>
  [JsonPropertyName("sourceCode")]
  [Computed]
  public string SourceCode_Json
  {
    get => NullIf(SourceCode, "");
    set => SourceCode = value;
  }

  private DateTime? date;
  private decimal amount;
  private string sourceCode;
}

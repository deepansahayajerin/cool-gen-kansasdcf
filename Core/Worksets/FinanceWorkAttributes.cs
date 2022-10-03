// The source file: FINANCE_WORK_ATTRIBUTES, ID: 372126716, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FinanceWorkAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FinanceWorkAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FinanceWorkAttributes(FinanceWorkAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FinanceWorkAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FinanceWorkAttributes that)
  {
    base.Assign(that);
    textCentValue = that.textCentValue;
    textDecimalDollarPart = that.textDecimalDollarPart;
    textNonDecimalDollarPart = that.textNonDecimalDollarPart;
    textDollarValue = that.textDollarValue;
    numericalDollarValue = that.numericalDollarValue;
    numericalCentValue = that.numericalCentValue;
  }

  /// <summary>Length of the TEXT_CENT_VALUE attribute.</summary>
  public const int TextCentValue_MaxLength = 9;

  /// <summary>
  /// The value of the TEXT_CENT_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TextCentValue_MaxLength)]
  public string TextCentValue
  {
    get => textCentValue ?? "";
    set => textCentValue =
      TrimEnd(Substring(value, 1, TextCentValue_MaxLength));
  }

  /// <summary>
  /// The json value of the TextCentValue attribute.</summary>
  [JsonPropertyName("textCentValue")]
  [Computed]
  public string TextCentValue_Json
  {
    get => NullIf(TextCentValue, "");
    set => TextCentValue = value;
  }

  /// <summary>Length of the TEXT_DECIMAL_DOLLAR_PART attribute.</summary>
  public const int TextDecimalDollarPart_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_DECIMAL_DOLLAR_PART attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = TextDecimalDollarPart_MaxLength)]
  public string TextDecimalDollarPart
  {
    get => textDecimalDollarPart ?? "";
    set => textDecimalDollarPart =
      TrimEnd(Substring(value, 1, TextDecimalDollarPart_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDecimalDollarPart attribute.</summary>
  [JsonPropertyName("textDecimalDollarPart")]
  [Computed]
  public string TextDecimalDollarPart_Json
  {
    get => NullIf(TextDecimalDollarPart, "");
    set => TextDecimalDollarPart = value;
  }

  /// <summary>Length of the TEXT_NON_DECIMAL_DOLLAR_PART attribute.</summary>
  public const int TextNonDecimalDollarPart_MaxLength = 7;

  /// <summary>
  /// The value of the TEXT_NON_DECIMAL_DOLLAR_PART attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = TextNonDecimalDollarPart_MaxLength)]
  public string TextNonDecimalDollarPart
  {
    get => textNonDecimalDollarPart ?? "";
    set => textNonDecimalDollarPart =
      TrimEnd(Substring(value, 1, TextNonDecimalDollarPart_MaxLength));
  }

  /// <summary>
  /// The json value of the TextNonDecimalDollarPart attribute.</summary>
  [JsonPropertyName("textNonDecimalDollarPart")]
  [Computed]
  public string TextNonDecimalDollarPart_Json
  {
    get => NullIf(TextNonDecimalDollarPart, "");
    set => TextNonDecimalDollarPart = value;
  }

  /// <summary>Length of the TEXT_DOLLAR_VALUE attribute.</summary>
  public const int TextDollarValue_MaxLength = 10;

  /// <summary>
  /// The value of the TEXT_DOLLAR_VALUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextDollarValue_MaxLength)]
    
  public string TextDollarValue
  {
    get => textDollarValue ?? "";
    set => textDollarValue =
      TrimEnd(Substring(value, 1, TextDollarValue_MaxLength));
  }

  /// <summary>
  /// The json value of the TextDollarValue attribute.</summary>
  [JsonPropertyName("textDollarValue")]
  [Computed]
  public string TextDollarValue_Json
  {
    get => NullIf(TextDollarValue, "");
    set => TextDollarValue = value;
  }

  /// <summary>
  /// The value of the NUMERICAL_DOLLAR_VALUE attribute.
  /// </summary>
  [JsonPropertyName("numericalDollarValue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal NumericalDollarValue
  {
    get => numericalDollarValue;
    set => numericalDollarValue = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the NUMERICAL_CENT_VALUE attribute.
  /// </summary>
  [JsonPropertyName("numericalCentValue")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int NumericalCentValue
  {
    get => numericalCentValue;
    set => numericalCentValue = value;
  }

  private string textCentValue;
  private string textDecimalDollarPart;
  private string textNonDecimalDollarPart;
  private string textDollarValue;
  private decimal numericalDollarValue;
  private int numericalCentValue;
}

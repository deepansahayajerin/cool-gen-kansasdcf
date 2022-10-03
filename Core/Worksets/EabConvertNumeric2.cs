// The source file: EAB_CONVERT_NUMERIC, ID: 372155688, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EabConvertNumeric2: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EabConvertNumeric2()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EabConvertNumeric2(EabConvertNumeric2 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EabConvertNumeric2 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EabConvertNumeric2 that)
  {
    base.Assign(that);
    sendSign = that.sendSign;
    sendAmount = that.sendAmount;
    sendNonSuppressPos = that.sendNonSuppressPos;
    returnCurrencySigned = that.returnCurrencySigned;
    returnCurrencyNegInParens = that.returnCurrencyNegInParens;
    returnAmountNonDecimalSigned = that.returnAmountNonDecimalSigned;
    returnNoCommasInNonDecimal = that.returnNoCommasInNonDecimal;
    returnOkFlag = that.returnOkFlag;
  }

  /// <summary>Length of the SEND_SIGN attribute.</summary>
  public const int SendSign_MaxLength = 1;

  /// <summary>
  /// The value of the SEND_SIGN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = SendSign_MaxLength)]
  public string SendSign
  {
    get => sendSign ?? "";
    set => sendSign = TrimEnd(Substring(value, 1, SendSign_MaxLength));
  }

  /// <summary>
  /// The json value of the SendSign attribute.</summary>
  [JsonPropertyName("sendSign")]
  [Computed]
  public string SendSign_Json
  {
    get => NullIf(SendSign, "");
    set => SendSign = value;
  }

  /// <summary>Length of the SEND_AMOUNT attribute.</summary>
  public const int SendAmount_MaxLength = 15;

  /// <summary>
  /// The value of the SEND_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = SendAmount_MaxLength)]
  public string SendAmount
  {
    get => sendAmount ?? "";
    set => sendAmount = TrimEnd(Substring(value, 1, SendAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SendAmount attribute.</summary>
  [JsonPropertyName("sendAmount")]
  [Computed]
  public string SendAmount_Json
  {
    get => NullIf(SendAmount, "");
    set => SendAmount = value;
  }

  /// <summary>
  /// The value of the SEND_NON_SUPPRESS_POS attribute.
  /// </summary>
  [JsonPropertyName("sendNonSuppressPos")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int SendNonSuppressPos
  {
    get => sendNonSuppressPos;
    set => sendNonSuppressPos = value;
  }

  /// <summary>Length of the RETURN_CURRENCY_SIGNED attribute.</summary>
  public const int ReturnCurrencySigned_MaxLength = 21;

  /// <summary>
  /// The value of the RETURN_CURRENCY_SIGNED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ReturnCurrencySigned_MaxLength)]
  public string ReturnCurrencySigned
  {
    get => returnCurrencySigned ?? "";
    set => returnCurrencySigned =
      TrimEnd(Substring(value, 1, ReturnCurrencySigned_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnCurrencySigned attribute.</summary>
  [JsonPropertyName("returnCurrencySigned")]
  [Computed]
  public string ReturnCurrencySigned_Json
  {
    get => NullIf(ReturnCurrencySigned, "");
    set => ReturnCurrencySigned = value;
  }

  /// <summary>Length of the RETURN_CURRENCY_NEG_IN_PARENS attribute.</summary>
  public const int ReturnCurrencyNegInParens_MaxLength = 22;

  /// <summary>
  /// The value of the RETURN_CURRENCY_NEG_IN_PARENS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ReturnCurrencyNegInParens_MaxLength)]
  public string ReturnCurrencyNegInParens
  {
    get => returnCurrencyNegInParens ?? "";
    set => returnCurrencyNegInParens =
      TrimEnd(Substring(value, 1, ReturnCurrencyNegInParens_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnCurrencyNegInParens attribute.</summary>
  [JsonPropertyName("returnCurrencyNegInParens")]
  [Computed]
  public string ReturnCurrencyNegInParens_Json
  {
    get => NullIf(ReturnCurrencyNegInParens, "");
    set => ReturnCurrencyNegInParens = value;
  }

  /// <summary>Length of the RETURN_AMOUNT_NON_DECIMAL_SIGNED attribute.
  /// </summary>
  public const int ReturnAmountNonDecimalSigned_MaxLength = 20;

  /// <summary>
  /// The value of the RETURN_AMOUNT_NON_DECIMAL_SIGNED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = ReturnAmountNonDecimalSigned_MaxLength)]
  public string ReturnAmountNonDecimalSigned
  {
    get => returnAmountNonDecimalSigned ?? "";
    set => returnAmountNonDecimalSigned =
      TrimEnd(Substring(value, 1, ReturnAmountNonDecimalSigned_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnAmountNonDecimalSigned attribute.</summary>
  [JsonPropertyName("returnAmountNonDecimalSigned")]
  [Computed]
  public string ReturnAmountNonDecimalSigned_Json
  {
    get => NullIf(ReturnAmountNonDecimalSigned, "");
    set => ReturnAmountNonDecimalSigned = value;
  }

  /// <summary>Length of the RETURN_NO_COMMAS_IN_NON_DECIMAL attribute.
  /// </summary>
  public const int ReturnNoCommasInNonDecimal_MaxLength = 16;

  /// <summary>
  /// The value of the RETURN_NO_COMMAS_IN_NON_DECIMAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ReturnNoCommasInNonDecimal_MaxLength)]
  public string ReturnNoCommasInNonDecimal
  {
    get => returnNoCommasInNonDecimal ?? "";
    set => returnNoCommasInNonDecimal =
      TrimEnd(Substring(value, 1, ReturnNoCommasInNonDecimal_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnNoCommasInNonDecimal attribute.</summary>
  [JsonPropertyName("returnNoCommasInNonDecimal")]
  [Computed]
  public string ReturnNoCommasInNonDecimal_Json
  {
    get => NullIf(ReturnNoCommasInNonDecimal, "");
    set => ReturnNoCommasInNonDecimal = value;
  }

  /// <summary>Length of the RETURN_OK_FLAG attribute.</summary>
  public const int ReturnOkFlag_MaxLength = 1;

  /// <summary>
  /// The value of the RETURN_OK_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ReturnOkFlag_MaxLength)]
  public string ReturnOkFlag
  {
    get => returnOkFlag ?? "";
    set => returnOkFlag = TrimEnd(Substring(value, 1, ReturnOkFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnOkFlag attribute.</summary>
  [JsonPropertyName("returnOkFlag")]
  [Computed]
  public string ReturnOkFlag_Json
  {
    get => NullIf(ReturnOkFlag, "");
    set => ReturnOkFlag = value;
  }

  private string sendSign;
  private string sendAmount;
  private int sendNonSuppressPos;
  private string returnCurrencySigned;
  private string returnCurrencyNegInParens;
  private string returnAmountNonDecimalSigned;
  private string returnNoCommasInNonDecimal;
  private string returnOkFlag;
}

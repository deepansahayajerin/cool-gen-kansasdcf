// The source file: SCREEN_OWED_AMOUNTS, ID: 371738574, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: FINANCE
/// This work set will provide the screen fields used to display summary 
/// information regarding CSE Person Account's and Obligations.
/// </summary>
[Serializable]
public partial class ScreenOwedAmounts: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ScreenOwedAmounts()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ScreenOwedAmounts(ScreenOwedAmounts that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ScreenOwedAmounts Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ScreenOwedAmounts that)
  {
    base.Assign(that);
    currentAmountOwed = that.currentAmountOwed;
    arrearsAmountOwed = that.arrearsAmountOwed;
    interestAmountOwed = that.interestAmountOwed;
    totalAmountOwed = that.totalAmountOwed;
    errorInformationLine = that.errorInformationLine;
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("currentAmountOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 1, Type = MemberType.Number, Length = 7, Precision = 2)]
  public decimal CurrentAmountOwed
  {
    get => currentAmountOwed;
    set => currentAmountOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ARREARS_AMOUNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("arrearsAmountOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal ArrearsAmountOwed
  {
    get => arrearsAmountOwed;
    set => arrearsAmountOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the INTEREST_AMOUNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("interestAmountOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal InterestAmountOwed
  {
    get => interestAmountOwed;
    set => interestAmountOwed = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the TOTAL_AMOUNT_OWED attribute.
  /// </summary>
  [JsonPropertyName("totalAmountOwed")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalAmountOwed
  {
    get => totalAmountOwed;
    set => totalAmountOwed = Truncate(value, 2);
  }

  /// <summary>Length of the ERROR_INFORMATION_LINE attribute.</summary>
  public const int ErrorInformationLine_MaxLength = 30;

  /// <summary>
  /// The value of the ERROR_INFORMATION_LINE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ErrorInformationLine_MaxLength)]
  public string ErrorInformationLine
  {
    get => errorInformationLine ?? "";
    set => errorInformationLine =
      TrimEnd(Substring(value, 1, ErrorInformationLine_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorInformationLine attribute.</summary>
  [JsonPropertyName("errorInformationLine")]
  [Computed]
  public string ErrorInformationLine_Json
  {
    get => NullIf(ErrorInformationLine, "");
    set => ErrorInformationLine = value;
  }

  private decimal currentAmountOwed;
  private decimal arrearsAmountOwed;
  private decimal interestAmountOwed;
  private decimal totalAmountOwed;
  private string errorInformationLine;
}

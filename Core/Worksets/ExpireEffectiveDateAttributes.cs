// The source file: EXPIRE_EFFECTIVE_DATE_ATTRIBUTES, ID: 371726887, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This work attribute set is used with the COMPARE EFFECT AND EXPIRE DATES 
/// action block that checks the two dates for a variety of situations and
/// returns Ys and Ns in this work attribute set in order to communicate its
/// results.
/// Set the effective date and expiration date attributes as imports into the 
/// action block.
/// </summary>
[Serializable]
public partial class ExpireEffectiveDateAttributes: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExpireEffectiveDateAttributes()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExpireEffectiveDateAttributes(ExpireEffectiveDateAttributes that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExpireEffectiveDateAttributes Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExpireEffectiveDateAttributes that)
  {
    base.Assign(that);
    expirationDate = that.expirationDate;
    effectiveDate = that.effectiveDate;
    effectiveDateIsZero = that.effectiveDateIsZero;
    expirationDateIsZero = that.expirationDateIsZero;
    effectiveDateIsLtCurrent = that.effectiveDateIsLtCurrent;
    expirationDateIsLtCurrent = that.expirationDateIsLtCurrent;
    expirationDateLtEffectiveDat = that.expirationDateLtEffectiveDat;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// This date should be set to the expiration date (if one exists)  in the 
  /// calling action block to be used as import in the compare action block.
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
  /// This effective date should be set in the calling action block because it 
  /// is used as import in the compare action block.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the EFFECTIVE_DATE_IS_ZERO attribute.</summary>
  public const int EffectiveDateIsZero_MaxLength = 1;

  /// <summary>
  /// The value of the EFFECTIVE_DATE_IS_ZERO attribute.
  /// A Y indicates the effective date supplied did not have a value.  A N 
  /// indicates it did have a value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = EffectiveDateIsZero_MaxLength)]
  public string EffectiveDateIsZero
  {
    get => effectiveDateIsZero ?? "";
    set => effectiveDateIsZero =
      TrimEnd(Substring(value, 1, EffectiveDateIsZero_MaxLength));
  }

  /// <summary>
  /// The json value of the EffectiveDateIsZero attribute.</summary>
  [JsonPropertyName("effectiveDateIsZero")]
  [Computed]
  public string EffectiveDateIsZero_Json
  {
    get => NullIf(EffectiveDateIsZero, "");
    set => EffectiveDateIsZero = value;
  }

  /// <summary>Length of the EXPIRATION_DATE_IS_ZERO attribute.</summary>
  public const int ExpirationDateIsZero_MaxLength = 1;

  /// <summary>
  /// The value of the EXPIRATION_DATE_IS_ZERO attribute.
  /// A Y indicates the expiration date does not have a value.  A N indicates 
  /// the expiration date did have a value.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ExpirationDateIsZero_MaxLength)]
  public string ExpirationDateIsZero
  {
    get => expirationDateIsZero ?? "";
    set => expirationDateIsZero =
      TrimEnd(Substring(value, 1, ExpirationDateIsZero_MaxLength));
  }

  /// <summary>
  /// The json value of the ExpirationDateIsZero attribute.</summary>
  [JsonPropertyName("expirationDateIsZero")]
  [Computed]
  public string ExpirationDateIsZero_Json
  {
    get => NullIf(ExpirationDateIsZero, "");
    set => ExpirationDateIsZero = value;
  }

  /// <summary>Length of the EFFECTIVE_DATE_IS_LT_CURRENT attribute.</summary>
  public const int EffectiveDateIsLtCurrent_MaxLength = 1;

  /// <summary>
  /// The value of the EFFECTIVE_DATE_IS_LT_CURRENT attribute.
  /// A Y indicates the effective date is less than current date.  A N indicates
  /// the effective date is greater than or equal to current date.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = EffectiveDateIsLtCurrent_MaxLength)]
  public string EffectiveDateIsLtCurrent
  {
    get => effectiveDateIsLtCurrent ?? "";
    set => effectiveDateIsLtCurrent =
      TrimEnd(Substring(value, 1, EffectiveDateIsLtCurrent_MaxLength));
  }

  /// <summary>
  /// The json value of the EffectiveDateIsLtCurrent attribute.</summary>
  [JsonPropertyName("effectiveDateIsLtCurrent")]
  [Computed]
  public string EffectiveDateIsLtCurrent_Json
  {
    get => NullIf(EffectiveDateIsLtCurrent, "");
    set => EffectiveDateIsLtCurrent = value;
  }

  /// <summary>Length of the EXPIRATION_DATE_IS_LT_CURRENT attribute.</summary>
  public const int ExpirationDateIsLtCurrent_MaxLength = 1;

  /// <summary>
  /// The value of the EXPIRATION_DATE_IS_LT_CURRENT attribute.
  /// A Y indicates the expiration date is less than current date.  A N 
  /// indicates the expiration date is greater than or equal to current date.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = ExpirationDateIsLtCurrent_MaxLength)]
  public string ExpirationDateIsLtCurrent
  {
    get => expirationDateIsLtCurrent ?? "";
    set => expirationDateIsLtCurrent =
      TrimEnd(Substring(value, 1, ExpirationDateIsLtCurrent_MaxLength));
  }

  /// <summary>
  /// The json value of the ExpirationDateIsLtCurrent attribute.</summary>
  [JsonPropertyName("expirationDateIsLtCurrent")]
  [Computed]
  public string ExpirationDateIsLtCurrent_Json
  {
    get => NullIf(ExpirationDateIsLtCurrent, "");
    set => ExpirationDateIsLtCurrent = value;
  }

  /// <summary>Length of the EXPIRATION_DATE_LT_EFFECTIVE_DAT attribute.
  /// </summary>
  public const int ExpirationDateLtEffectiveDat_MaxLength = 1;

  /// <summary>
  /// The value of the EXPIRATION_DATE_LT_EFFECTIVE_DAT attribute.
  /// A Y indicates that the expiration date is less than the effective date.  A
  /// N indicates that the expiration date is greater than or equal to current
  /// date.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ExpirationDateLtEffectiveDat_MaxLength)]
  public string ExpirationDateLtEffectiveDat
  {
    get => expirationDateLtEffectiveDat ?? "";
    set => expirationDateLtEffectiveDat =
      TrimEnd(Substring(value, 1, ExpirationDateLtEffectiveDat_MaxLength));
  }

  /// <summary>
  /// The json value of the ExpirationDateLtEffectiveDat attribute.</summary>
  [JsonPropertyName("expirationDateLtEffectiveDat")]
  [Computed]
  public string ExpirationDateLtEffectiveDat_Json
  {
    get => NullIf(ExpirationDateLtEffectiveDat, "");
    set => ExpirationDateLtEffectiveDat = value;
  }

  private DateTime? expirationDate;
  private DateTime? effectiveDate;
  private string effectiveDateIsZero;
  private string expirationDateIsZero;
  private string effectiveDateIsLtCurrent;
  private string expirationDateIsLtCurrent;
  private string expirationDateLtEffectiveDat;
}

// The source file: AA_SCROLLING_FIELDS, ID: 371880790, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class AaScrollingFields: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AaScrollingFields()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AaScrollingFields(AaScrollingFields that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AaScrollingFields Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AaScrollingFields that)
  {
    base.Assign(that);
    moreIndicator = that.moreIndicator;
    minuCharacter = that.minuCharacter;
    plusCharacter = that.plusCharacter;
  }

  /// <summary>Length of the MORE_INDICATOR attribute.</summary>
  public const int MoreIndicator_MaxLength = 4;

  /// <summary>
  /// The value of the MORE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = MoreIndicator_MaxLength)]
  public string MoreIndicator
  {
    get => moreIndicator ?? "";
    set => moreIndicator =
      TrimEnd(Substring(value, 1, MoreIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MoreIndicator attribute.</summary>
  [JsonPropertyName("moreIndicator")]
  [Computed]
  public string MoreIndicator_Json
  {
    get => NullIf(MoreIndicator, "");
    set => MoreIndicator = value;
  }

  /// <summary>Length of the MINU_CHARACTER attribute.</summary>
  public const int MinuCharacter_MaxLength = 1;

  /// <summary>
  /// The value of the MINU_CHARACTER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = MinuCharacter_MaxLength)]
  public string MinuCharacter
  {
    get => minuCharacter ?? "";
    set => minuCharacter =
      TrimEnd(Substring(value, 1, MinuCharacter_MaxLength));
  }

  /// <summary>
  /// The json value of the MinuCharacter attribute.</summary>
  [JsonPropertyName("minuCharacter")]
  [Computed]
  public string MinuCharacter_Json
  {
    get => NullIf(MinuCharacter, "");
    set => MinuCharacter = value;
  }

  /// <summary>Length of the PLUS_CHARACTER attribute.</summary>
  public const int PlusCharacter_MaxLength = 1;

  /// <summary>
  /// The value of the PLUS_CHARACTER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = PlusCharacter_MaxLength)]
  public string PlusCharacter
  {
    get => plusCharacter ?? "";
    set => plusCharacter =
      TrimEnd(Substring(value, 1, PlusCharacter_MaxLength));
  }

  /// <summary>
  /// The json value of the PlusCharacter attribute.</summary>
  [JsonPropertyName("plusCharacter")]
  [Computed]
  public string PlusCharacter_Json
  {
    get => NullIf(PlusCharacter, "");
    set => PlusCharacter = value;
  }

  private string moreIndicator;
  private string minuCharacter;
  private string plusCharacter;
}

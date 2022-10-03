// The source file: DMON_TEXT, ID: 373017279, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class DmonText: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DmonText()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DmonText(DmonText that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DmonText Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DmonText that)
  {
    base.Assign(that);
    personNumbers1 = that.personNumbers1;
    personNumbers2 = that.personNumbers2;
    personNumbers3 = that.personNumbers3;
  }

  /// <summary>Length of the PERSON_NUMBERS_1 attribute.</summary>
  public const int PersonNumbers1_MaxLength = 253;

  /// <summary>
  /// The value of the PERSON_NUMBERS_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = PersonNumbers1_MaxLength)]
  public string PersonNumbers1
  {
    get => personNumbers1 ?? "";
    set => personNumbers1 =
      TrimEnd(Substring(value, 1, PersonNumbers1_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonNumbers1 attribute.</summary>
  [JsonPropertyName("personNumbers1")]
  [Computed]
  public string PersonNumbers1_Json
  {
    get => NullIf(PersonNumbers1, "");
    set => PersonNumbers1 = value;
  }

  /// <summary>Length of the PERSON_NUMBERS_2 attribute.</summary>
  public const int PersonNumbers2_MaxLength = 253;

  /// <summary>
  /// The value of the PERSON_NUMBERS_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PersonNumbers2_MaxLength)]
  public string PersonNumbers2
  {
    get => personNumbers2 ?? "";
    set => personNumbers2 =
      TrimEnd(Substring(value, 1, PersonNumbers2_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonNumbers2 attribute.</summary>
  [JsonPropertyName("personNumbers2")]
  [Computed]
  public string PersonNumbers2_Json
  {
    get => NullIf(PersonNumbers2, "");
    set => PersonNumbers2 = value;
  }

  /// <summary>Length of the PERSON_NUMBERS_3 attribute.</summary>
  public const int PersonNumbers3_MaxLength = 253;

  /// <summary>
  /// The value of the PERSON_NUMBERS_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = PersonNumbers3_MaxLength)]
  public string PersonNumbers3
  {
    get => personNumbers3 ?? "";
    set => personNumbers3 =
      TrimEnd(Substring(value, 1, PersonNumbers3_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonNumbers3 attribute.</summary>
  [JsonPropertyName("personNumbers3")]
  [Computed]
  public string PersonNumbers3_Json
  {
    get => NullIf(PersonNumbers3, "");
    set => PersonNumbers3 = value;
  }

  private string personNumbers1;
  private string personNumbers2;
  private string personNumbers3;
}

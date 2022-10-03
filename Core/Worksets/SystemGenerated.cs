// The source file: SYSTEM_GENERATED, ID: 371424126, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class SystemGenerated: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SystemGenerated()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SystemGenerated(SystemGenerated that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SystemGenerated Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SystemGenerated that)
  {
    base.Assign(that);
    attribute9DigitRandomNumber = that.attribute9DigitRandomNumber;
    attribute3DigitRandomNumber = that.attribute3DigitRandomNumber;
  }

  /// <summary>
  /// The value of the 9_DIGIT_RANDOM_NUMBER attribute.
  /// A nine digit random number generated from the right most side of the 
  /// current timestamp.  This number is generally used as part of an
  /// identifier.
  /// </summary>
  [JsonPropertyName("attribute9DigitRandomNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Attribute9DigitRandomNumber
  {
    get => attribute9DigitRandomNumber;
    set => attribute9DigitRandomNumber = value;
  }

  /// <summary>
  /// The value of the 3_DIGIT_RANDOM_NUMBER attribute.
  /// A three digit random number generated from the right most side of the 
  /// current timestamp.  This number is generally used as part of an
  /// identifier.
  /// </summary>
  [JsonPropertyName("attribute3DigitRandomNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Attribute3DigitRandomNumber
  {
    get => attribute3DigitRandomNumber;
    set => attribute3DigitRandomNumber = value;
  }

  private int attribute9DigitRandomNumber;
  private int attribute3DigitRandomNumber;
}

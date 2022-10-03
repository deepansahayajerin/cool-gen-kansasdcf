// The source file: INTERSTATE_WORK_AREA, ID: 372511279, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class InterstateWorkArea: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateWorkArea()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateWorkArea(InterstateWorkArea that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateWorkArea Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateWorkArea that)
  {
    base.Assign(that);
    phoneNumber = that.phoneNumber;
    phoneAreaCode = that.phoneAreaCode;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 7)]
  public int PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  private int phoneNumber;
  private int phoneAreaCode;
}

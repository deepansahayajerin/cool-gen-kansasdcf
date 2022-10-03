// The source file: FN_REFERENCE_NUMBER, ID: 371742778, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FnReferenceNumber: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FnReferenceNumber()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FnReferenceNumber(FnReferenceNumber that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FnReferenceNumber Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FnReferenceNumber that)
  {
    base.Assign(that);
    referenceNumber = that.referenceNumber;
  }

  /// <summary>Length of the REFERENCE_NUMBER attribute.</summary>
  public const int ReferenceNumber_MaxLength = 14;

  /// <summary>
  /// The value of the REFERENCE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ReferenceNumber_MaxLength)]
    
  public string ReferenceNumber
  {
    get => referenceNumber ?? "";
    set => referenceNumber =
      TrimEnd(Substring(value, 1, ReferenceNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferenceNumber attribute.</summary>
  [JsonPropertyName("referenceNumber")]
  [Computed]
  public string ReferenceNumber_Json
  {
    get => NullIf(ReferenceNumber, "");
    set => ReferenceNumber = value;
  }

  private string referenceNumber;
}

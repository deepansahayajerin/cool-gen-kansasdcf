// The source file: CONCATENATE_NAME, ID: 372737061, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ConcatenateName: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ConcatenateName()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ConcatenateName(ConcatenateName that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ConcatenateName Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ConcatenateName that)
  {
    base.Assign(that);
    formattedName = that.formattedName;
  }

  /// <summary>Length of the FORMATTED_NAME attribute.</summary>
  public const int FormattedName_MaxLength = 54;

  /// <summary>
  /// The value of the FORMATTED_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FormattedName_MaxLength)]
  public string FormattedName
  {
    get => formattedName ?? "";
    set => formattedName =
      TrimEnd(Substring(value, 1, FormattedName_MaxLength));
  }

  /// <summary>
  /// The json value of the FormattedName attribute.</summary>
  [JsonPropertyName("formattedName")]
  [Computed]
  public string FormattedName_Json
  {
    get => NullIf(FormattedName, "");
    set => FormattedName = value;
  }

  private string formattedName;
}

// The source file: 255_CHARACTER_TEXT_RECORD, ID: 1625350570, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Data255CharacterTextRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Data255CharacterTextRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Data255CharacterTextRecord(Data255CharacterTextRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Data255CharacterTextRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Data255CharacterTextRecord that)
  {
    base.Assign(that);
    data = that.data;
  }

  /// <summary>Length of the DATA attribute.</summary>
  public const int Data_MaxLength = 255;

  /// <summary>
  /// The value of the DATA attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Data_MaxLength)]
  public string Data
  {
    get => data ?? "";
    set => data = TrimEnd(Substring(value, 1, Data_MaxLength));
  }

  /// <summary>
  /// The json value of the Data attribute.</summary>
  [JsonPropertyName("data")]
  [Computed]
  public string Data_Json
  {
    get => NullIf(Data, "");
    set => Data = value;
  }

  private string data;
}

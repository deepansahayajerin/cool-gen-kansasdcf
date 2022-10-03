// The source file: NARRATIVE_WORK, ID: 945057216, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NarrativeWork: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NarrativeWork()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NarrativeWork(NarrativeWork that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NarrativeWork Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NarrativeWork that)
  {
    base.Assign(that);
    pageNo = that.pageNo;
    noteNo = that.noteNo;
    text = that.text;
  }

  /// <summary>
  /// The value of the PAGE_NO attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("pageNo")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int PageNo
  {
    get => pageNo;
    set => pageNo = value;
  }

  /// <summary>
  /// The value of the NOTE_NO attribute.
  /// This attribute assists in uniquely identifying each occurrence of 
  /// narrative.
  /// </summary>
  [JsonPropertyName("noteNo")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? NoteNo
  {
    get => noteNo;
    set => noteNo = value;
  }

  /// <summary>Length of the TEXT attribute.</summary>
  public const int Text_MaxLength = 240;

  /// <summary>
  /// The value of the TEXT attribute.
  /// The textual content of the narrative.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Text_MaxLength)]
  public string Text
  {
    get => text ?? "";
    set => text = TrimEnd(Substring(value, 1, Text_MaxLength));
  }

  /// <summary>
  /// The json value of the Text attribute.</summary>
  [JsonPropertyName("text")]
  [Computed]
  public string Text_Json
  {
    get => NullIf(Text, "");
    set => Text = value;
  }

  private int pageNo;
  private DateTime? noteNo;
  private string text;
}

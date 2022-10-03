// The source file: NONSTANDARD_SCROLLING, ID: 374577176, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class NonstandardScrolling: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NonstandardScrolling()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NonstandardScrolling(NonstandardScrolling that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NonstandardScrolling Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NonstandardScrolling that)
  {
    base.Assign(that);
    pageNumber = that.pageNumber;
    scrollingMessage = that.scrollingMessage;
  }

  /// <summary>
  /// The value of the PAGE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("pageNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int PageNumber
  {
    get => pageNumber;
    set => pageNumber = value;
  }

  /// <summary>Length of the SCROLLING_MESSAGE attribute.</summary>
  public const int ScrollingMessage_MaxLength = 20;

  /// <summary>
  /// The value of the SCROLLING_MESSAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ScrollingMessage_MaxLength)
    ]
  public string ScrollingMessage
  {
    get => scrollingMessage ?? "";
    set => scrollingMessage =
      TrimEnd(Substring(value, 1, ScrollingMessage_MaxLength));
  }

  /// <summary>
  /// The json value of the ScrollingMessage attribute.</summary>
  [JsonPropertyName("scrollingMessage")]
  [Computed]
  public string ScrollingMessage_Json
  {
    get => NullIf(ScrollingMessage, "");
    set => ScrollingMessage = value;
  }

  private int pageNumber;
  private string scrollingMessage;
}

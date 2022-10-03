// The source file: STANDARD, ID: 371424070, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVINIT
/// This work set contains data fields that will be used by all procedures.
/// </summary>
[Serializable]
public partial class Standard: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Standard()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Standard(Standard that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Standard Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Standard that)
  {
    base.Assign(that);
    command = that.command;
    oneChar = that.oneChar;
    nextTransaction = that.nextTransaction;
    scrollingMessage = that.scrollingMessage;
    menuOption = that.menuOption;
    pageNumber = that.pageNumber;
    deleteText = that.deleteText;
    deleteConfirmation = that.deleteConfirmation;
    promptField = that.promptField;
  }

  /// <summary>Length of the COMMAND attribute.</summary>
  public const int Command_MaxLength = 8;

  /// <summary>
  /// The value of the COMMAND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Command_MaxLength)]
  public string Command
  {
    get => command ?? "";
    set => command = TrimEnd(Substring(value, 1, Command_MaxLength));
  }

  /// <summary>
  /// The json value of the Command attribute.</summary>
  [JsonPropertyName("command")]
  [Computed]
  public string Command_Json
  {
    get => NullIf(Command, "");
    set => Command = value;
  }

  /// <summary>Length of the ONE_CHAR attribute.</summary>
  public const int OneChar_MaxLength = 1;

  /// <summary>
  /// The value of the ONE_CHAR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = OneChar_MaxLength)]
  public string OneChar
  {
    get => oneChar ?? "";
    set => oneChar = TrimEnd(Substring(value, 1, OneChar_MaxLength));
  }

  /// <summary>
  /// The json value of the OneChar attribute.</summary>
  [JsonPropertyName("oneChar")]
  [Computed]
  public string OneChar_Json
  {
    get => NullIf(OneChar, "");
    set => OneChar = value;
  }

  /// <summary>Length of the NEXT_TRANSACTION attribute.</summary>
  public const int NextTransaction_MaxLength = 4;

  /// <summary>
  /// The value of the NEXT_TRANSACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = NextTransaction_MaxLength)]
    
  public string NextTransaction
  {
    get => nextTransaction ?? "";
    set => nextTransaction =
      TrimEnd(Substring(value, 1, NextTransaction_MaxLength));
  }

  /// <summary>
  /// The json value of the NextTransaction attribute.</summary>
  [JsonPropertyName("nextTransaction")]
  [Computed]
  public string NextTransaction_Json
  {
    get => NullIf(NextTransaction, "");
    set => NextTransaction = value;
  }

  /// <summary>Length of the SCROLLING_MESSAGE attribute.</summary>
  public const int ScrollingMessage_MaxLength = 8;

  /// <summary>
  /// The value of the SCROLLING_MESSAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ScrollingMessage_MaxLength)
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

  /// <summary>
  /// The value of the MENU_OPTION attribute.
  /// </summary>
  [JsonPropertyName("menuOption")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int MenuOption
  {
    get => menuOption;
    set => menuOption = value;
  }

  /// <summary>
  /// The value of the PAGE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("pageNumber")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 2)]
  public int PageNumber
  {
    get => pageNumber;
    set => pageNumber = value;
  }

  /// <summary>Length of the DELETE_TEXT attribute.</summary>
  public const int DeleteText_MaxLength = 30;

  /// <summary>
  /// The value of the DELETE_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = DeleteText_MaxLength)]
  public string DeleteText
  {
    get => deleteText ?? "";
    set => deleteText = TrimEnd(Substring(value, 1, DeleteText_MaxLength));
  }

  /// <summary>
  /// The json value of the DeleteText attribute.</summary>
  [JsonPropertyName("deleteText")]
  [Computed]
  public string DeleteText_Json
  {
    get => NullIf(DeleteText, "");
    set => DeleteText = value;
  }

  /// <summary>Length of the DELETE_CONFIRMATION attribute.</summary>
  public const int DeleteConfirmation_MaxLength = 1;

  /// <summary>
  /// The value of the DELETE_CONFIRMATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = DeleteConfirmation_MaxLength)]
  public string DeleteConfirmation
  {
    get => deleteConfirmation ?? "";
    set => deleteConfirmation =
      TrimEnd(Substring(value, 1, DeleteConfirmation_MaxLength));
  }

  /// <summary>
  /// The json value of the DeleteConfirmation attribute.</summary>
  [JsonPropertyName("deleteConfirmation")]
  [Computed]
  public string DeleteConfirmation_Json
  {
    get => NullIf(DeleteConfirmation, "");
    set => DeleteConfirmation = value;
  }

  /// <summary>Length of the PROMPT_FIELD attribute.</summary>
  public const int PromptField_MaxLength = 1;

  /// <summary>
  /// The value of the PROMPT_FIELD attribute.
  /// Used as prompt field on the screen.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = PromptField_MaxLength)]
  public string PromptField
  {
    get => promptField ?? "";
    set => promptField = TrimEnd(Substring(value, 1, PromptField_MaxLength));
  }

  /// <summary>
  /// The json value of the PromptField attribute.</summary>
  [JsonPropertyName("promptField")]
  [Computed]
  public string PromptField_Json
  {
    get => NullIf(PromptField, "");
    set => PromptField = value;
  }

  private string command;
  private string oneChar;
  private string nextTransaction;
  private string scrollingMessage;
  private int menuOption;
  private int pageNumber;
  private string deleteText;
  private string deleteConfirmation;
  private string promptField;
}

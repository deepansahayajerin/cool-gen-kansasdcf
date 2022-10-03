// The source file: EXTERNAL, ID: 371755370, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This work set is for passing data back and forth to an external.
/// </summary>
[Serializable]
public partial class External: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public External()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public External(External that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new External Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(External that)
  {
    base.Assign(that);
    fileInstruction = that.fileInstruction;
    numericReturnCode = that.numericReturnCode;
    textReturnCode = that.textReturnCode;
    textLine130 = that.textLine130;
    textLine80 = that.textLine80;
    textLine8 = that.textLine8;
  }

  /// <summary>Length of the FILE_INSTRUCTION attribute.</summary>
  public const int FileInstruction_MaxLength = 10;

  /// <summary>
  /// The value of the FILE_INSTRUCTION attribute.
  /// File instruction is used to tell the external routine function is should 
  /// perform on any given call.
  /// Examples:
  /// OPEN = Open file
  /// WRITE = Write to file
  /// CLOSE = Close file
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FileInstruction_MaxLength)]
    
  public string FileInstruction
  {
    get => fileInstruction ?? "";
    set => fileInstruction =
      TrimEnd(Substring(value, 1, FileInstruction_MaxLength));
  }

  /// <summary>
  /// The json value of the FileInstruction attribute.</summary>
  [JsonPropertyName("fileInstruction")]
  [Computed]
  public string FileInstruction_Json
  {
    get => NullIf(FileInstruction, "");
    set => FileInstruction = value;
  }

  /// <summary>
  /// The value of the NUMERIC_RETURN_CODE attribute.
  /// A numeric return code passed back from an external action block.  It is 
  /// used to communicate the results of the call to the external.
  /// </summary>
  [JsonPropertyName("numericReturnCode")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int NumericReturnCode
  {
    get => numericReturnCode;
    set => numericReturnCode = value;
  }

  /// <summary>Length of the TEXT_RETURN_CODE attribute.</summary>
  public const int TextReturnCode_MaxLength = 2;

  /// <summary>
  /// The value of the TEXT_RETURN_CODE attribute.
  /// A text return code passed back from an external action block. It is used 
  /// to communicate the results of the call to the external.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = TextReturnCode_MaxLength)]
  public string TextReturnCode
  {
    get => textReturnCode ?? "";
    set => textReturnCode =
      TrimEnd(Substring(value, 1, TextReturnCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TextReturnCode attribute.</summary>
  [JsonPropertyName("textReturnCode")]
  [Computed]
  public string TextReturnCode_Json
  {
    get => NullIf(TextReturnCode, "");
    set => TextReturnCode = value;
  }

  /// <summary>Length of the TEXT_LINE_130 attribute.</summary>
  public const int TextLine130_MaxLength = 130;

  /// <summary>
  /// The value of the TEXT_LINE_130 attribute.
  /// Text line is a generic line of text that can be used to pass some 
  /// information to an external to write out to an error report, etc.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TextLine130_MaxLength)]
  public string TextLine130
  {
    get => textLine130 ?? "";
    set => textLine130 = TrimEnd(Substring(value, 1, TextLine130_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLine130 attribute.</summary>
  [JsonPropertyName("textLine130")]
  [Computed]
  public string TextLine130_Json
  {
    get => NullIf(TextLine130, "");
    set => TextLine130 = value;
  }

  /// <summary>Length of the TEXT_LINE_80 attribute.</summary>
  public const int TextLine80_MaxLength = 130;

  /// <summary>
  /// The value of the TEXT_LINE_80 attribute.
  /// Text line is a generic line of text that can be used to pass some 
  /// information to an external to write out to an error report, etc.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TextLine80_MaxLength)]
  public string TextLine80
  {
    get => textLine80 ?? "";
    set => textLine80 = TrimEnd(Substring(value, 1, TextLine80_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLine80 attribute.</summary>
  [JsonPropertyName("textLine80")]
  [Computed]
  public string TextLine80_Json
  {
    get => NullIf(TextLine80, "");
    set => TextLine80 = value;
  }

  /// <summary>Length of the TEXT_LINE_8 attribute.</summary>
  public const int TextLine8_MaxLength = 8;

  /// <summary>
  /// The value of the TEXT_LINE_8 attribute.
  /// Eight character text line.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TextLine8_MaxLength)]
  public string TextLine8
  {
    get => textLine8 ?? "";
    set => textLine8 = TrimEnd(Substring(value, 1, TextLine8_MaxLength));
  }

  /// <summary>
  /// The json value of the TextLine8 attribute.</summary>
  [JsonPropertyName("textLine8")]
  [Computed]
  public string TextLine8_Json
  {
    get => NullIf(TextLine8, "");
    set => TextLine8 = value;
  }

  private string fileInstruction;
  private int numericReturnCode;
  private string textReturnCode;
  private string textLine130;
  private string textLine80;
  private string textLine8;
}

// The source file: QUICK_ERROR_MESSAGES, ID: 374543722, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickErrorMessages: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickErrorMessages()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickErrorMessages(QuickErrorMessages that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickErrorMessages Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickErrorMessages that)
  {
    base.Assign(that);
    errorMessage = that.errorMessage;
    errorCode = that.errorCode;
  }

  /// <summary>Length of the ERROR_MESSAGE attribute.</summary>
  public const int ErrorMessage_MaxLength = 255;

  /// <summary>
  /// The value of the ERROR_MESSAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ErrorMessage_MaxLength)]
  public string ErrorMessage
  {
    get => errorMessage ?? "";
    set => errorMessage = TrimEnd(Substring(value, 1, ErrorMessage_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMessage attribute.</summary>
  [JsonPropertyName("errorMessage")]
  [Computed]
  public string ErrorMessage_Json
  {
    get => NullIf(ErrorMessage, "");
    set => ErrorMessage = value;
  }

  /// <summary>Length of the ERROR_CODE attribute.</summary>
  public const int ErrorCode_MaxLength = 5;

  /// <summary>
  /// The value of the ERROR_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ErrorCode_MaxLength)]
  public string ErrorCode
  {
    get => errorCode ?? "";
    set => errorCode = TrimEnd(Substring(value, 1, ErrorCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCode attribute.</summary>
  [JsonPropertyName("errorCode")]
  [Computed]
  public string ErrorCode_Json
  {
    get => NullIf(ErrorCode, "");
    set => ErrorCode = value;
  }

  private string errorMessage;
  private string errorCode;
}

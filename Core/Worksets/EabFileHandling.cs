// The source file: EAB_FILE_HANDLING, ID: 371790863, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EabFileHandling: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EabFileHandling()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EabFileHandling(EabFileHandling that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EabFileHandling Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EabFileHandling that)
  {
    base.Assign(that);
    action = that.action;
    status = that.status;
  }

  /// <summary>Length of the ACTION attribute.</summary>
  public const int Action_MaxLength = 8;

  /// <summary>
  /// The value of the ACTION attribute.
  /// Specifies the action to be taken by the report writer.  The following are 
  /// the only valid values:
  ///    OPEN
  ///    WRITE
  ///    CLOSE
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Action_MaxLength)]
  public string Action
  {
    get => action ?? "";
    set => action = TrimEnd(Substring(value, 1, Action_MaxLength));
  }

  /// <summary>
  /// The json value of the Action attribute.</summary>
  [JsonPropertyName("action")]
  [Computed]
  public string Action_Json
  {
    get => NullIf(Action, "");
    set => Action = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 8;

  /// <summary>
  /// The value of the STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Status_MaxLength)]
  public string Status
  {
    get => status ?? "";
    set => status = TrimEnd(Substring(value, 1, Status_MaxLength));
  }

  /// <summary>
  /// The json value of the Status attribute.</summary>
  [JsonPropertyName("status")]
  [Computed]
  public string Status_Json
  {
    get => NullIf(Status, "");
    set => Status = value;
  }

  private string action;
  private string status;
}

// The source file: COMMAND, ID: 371421527, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// this is the valid commands for a given screen. example.. add, update, 
/// delete, and display, etc.
/// </summary>
[Serializable]
public partial class Command: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Command()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Command(Command that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Command Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Command that)
  {
    base.Assign(that);
    value = that.value;
    desc = that.desc;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
  }

  /// <summary>Length of the VALUE attribute.</summary>
  public const int Value_MaxLength = 8;

  /// <summary>
  /// The value of the VALUE attribute.
  /// command value available to a transcation(screen).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Value_MaxLength)]
  public string Value
  {
    get => value ?? "";
    set => this.value = TrimEnd(Substring(value, 1, Value_MaxLength));
  }

  /// <summary>
  /// The json value of the Value attribute.</summary>
  [JsonPropertyName("value")]
  [Computed]
  public string Value_Json
  {
    get => NullIf(Value, "");
    set => Value = value;
  }

  /// <summary>Length of the DESC attribute.</summary>
  public const int Desc_MaxLength = 25;

  /// <summary>
  /// The value of the DESC attribute.
  /// The description of the command.
  /// </summary>
  [JsonPropertyName("desc")]
  [Member(Index = 2, Type = MemberType.Char, Length = Desc_MaxLength, Optional
    = true)]
  public string Desc
  {
    get => desc;
    set => desc = value != null
      ? TrimEnd(Substring(value, 1, Desc_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  private string value;
  private string desc;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
}

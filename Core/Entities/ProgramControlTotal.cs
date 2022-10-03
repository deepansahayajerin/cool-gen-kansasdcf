// The source file: PROGRAM_CONTROL_TOTAL, ID: 371439767, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity will contain control information that batch programs will write 
/// out.  Each run of a batch program may have multiple control totals that will
/// be written out to this table to provide information about the data
/// processed.
/// </summary>
[Serializable]
public partial class ProgramControlTotal: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramControlTotal()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramControlTotal(ProgramControlTotal that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramControlTotal Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramControlTotal that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    value = that.value;
    name = that.name;
    prrStartTstamp = that.prrStartTstamp;
    ppiName = that.ppiName;
    ppiCreatedTstamp = that.ppiCreatedTstamp;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// This attribute contains a unique system generated identifier.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the VALUE attribute.
  /// This attribute contains the number for the control total.  Examples of 
  /// this number may be the number of records processed or the amount of money
  /// processed.
  /// </summary>
  [JsonPropertyName("value")]
  [Member(Index = 2, Type = MemberType.Number, Length = 12, Precision = 2, Optional
    = true)]
  public decimal? Value
  {
    get => value;
    set => this.value = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 60;

  /// <summary>
  /// The value of the NAME attribute.
  /// This field contains some text that describes what kind of control total 
  /// count is being provided in this row.  Examples may be 'The number of
  /// records processed' or 'The amount of money processed'.
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 3, Type = MemberType.Varchar, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => name;
    set => name = value != null ? Substring(value, 1, Name_MaxLength) : null;
  }

  /// <summary>
  /// The value of the START_TIMESTAMP attribute.
  /// The date and time that the program was executed.
  /// </summary>
  [JsonPropertyName("prrStartTstamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? PrrStartTstamp
  {
    get => prrStartTstamp;
    set => prrStartTstamp = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int PpiName_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the program (from the exec card statement in the JCL) that is 
  /// to be run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = PpiName_MaxLength)]
  public string PpiName
  {
    get => ppiName ?? "";
    set => ppiName = TrimEnd(Substring(value, 1, PpiName_MaxLength));
  }

  /// <summary>
  /// The json value of the PpiName attribute.</summary>
  [JsonPropertyName("ppiName")]
  [Computed]
  public string PpiName_Json
  {
    get => NullIf(PpiName, "");
    set => PpiName = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("ppiCreatedTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? PpiCreatedTstamp
  {
    get => ppiCreatedTstamp;
    set => ppiCreatedTstamp = value;
  }

  private int systemGeneratedIdentifier;
  private decimal? value;
  private string name;
  private DateTime? prrStartTstamp;
  private string ppiName;
  private DateTime? ppiCreatedTstamp;
}

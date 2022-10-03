// The source file: PROGRAM_RUN, ID: 371439890, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This table will provide the ability to do online inquiries of the status of 
/// programs that have run.  The program creates a row at the start of
/// processing and updates it at the end of processing.
/// </summary>
[Serializable]
public partial class ProgramRun: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramRun()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramRun(ProgramRun that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramRun Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramRun that)
  {
    base.Assign(that);
    fromRestartInd = that.fromRestartInd;
    startTimestamp = that.startTimestamp;
    endTimestamp = that.endTimestamp;
    ppiCreatedTstamp = that.ppiCreatedTstamp;
    ppiName = that.ppiName;
  }

  /// <summary>Length of the FROM_RESTART_IND attribute.</summary>
  public const int FromRestartInd_MaxLength = 1;

  /// <summary>
  /// The value of the FROM_RESTART_IND attribute.
  /// This Y/N indicator specifies if the program run was the result of a 
  /// restart submission or an initial submission.  If indicator = Y must assume
  /// that it is a restart of the most recent program run with an N.
  /// </summary>
  [JsonPropertyName("fromRestartInd")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = FromRestartInd_MaxLength, Optional = true)]
  public string FromRestartInd
  {
    get => fromRestartInd;
    set => fromRestartInd = value != null
      ? TrimEnd(Substring(value, 1, FromRestartInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the START_TIMESTAMP attribute.
  /// The date and time that the program was executed.
  /// </summary>
  [JsonPropertyName("startTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? StartTimestamp
  {
    get => startTimestamp;
    set => startTimestamp = value;
  }

  /// <summary>
  /// The value of the END_TIMESTAMP attribute.
  /// The date and time that the program ended.
  /// </summary>
  [JsonPropertyName("endTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? EndTimestamp
  {
    get => endTimestamp;
    set => endTimestamp = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("ppiCreatedTstamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? PpiCreatedTstamp
  {
    get => ppiCreatedTstamp;
    set => ppiCreatedTstamp = value;
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

  private string fromRestartInd;
  private DateTime? startTimestamp;
  private DateTime? endTimestamp;
  private DateTime? ppiCreatedTstamp;
  private string ppiName;
}

// The source file: PROGRAM_CHECKPOINT_RESTART, ID: 371439738, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This table will contain checkpoint restart information that each batch job 
/// may use to determine how frequently to do commits to the database.  It may
/// also contain information about the last record processed so that in a
/// restart situation the job can position itself to the proper location in the
/// driver file before it begins to process again.
/// </summary>
[Serializable]
public partial class ProgramCheckpointRestart: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramCheckpointRestart()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramCheckpointRestart(ProgramCheckpointRestart that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramCheckpointRestart Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramCheckpointRestart that)
  {
    base.Assign(that);
    programName = that.programName;
    updateFrequencyCount = that.updateFrequencyCount;
    readFrequencyCount = that.readFrequencyCount;
    checkpointCount = that.checkpointCount;
    lastCheckpointTimestamp = that.lastCheckpointTimestamp;
    restartInd = that.restartInd;
    restartInfo = that.restartInfo;
  }

  /// <summary>Length of the PROGRAM_NAME attribute.</summary>
  public const int ProgramName_MaxLength = 8;

  /// <summary>
  /// The value of the PROGRAM_NAME attribute.
  /// The name of the program from the exec statement in the JCL.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ProgramName_MaxLength)]
  public string ProgramName
  {
    get => programName ?? "";
    set => programName = TrimEnd(Substring(value, 1, ProgramName_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramName attribute.</summary>
  [JsonPropertyName("programName")]
  [Computed]
  public string ProgramName_Json
  {
    get => NullIf(ProgramName, "");
    set => ProgramName = value;
  }

  /// <summary>
  /// The value of the UPDATE_FREQUENCY_COUNT attribute.
  /// This attribute is used to determine how many updates a batch process 
  /// should do before a DB2 commit should take place.
  /// </summary>
  [JsonPropertyName("updateFrequencyCount")]
  [Member(Index = 2, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? UpdateFrequencyCount
  {
    get => updateFrequencyCount;
    set => updateFrequencyCount = value;
  }

  /// <summary>
  /// The value of the READ_FREQUENCY_COUNT attribute.
  /// This attribute is used to determine how many reads a batch process should 
  /// do before a DB2 commit should be taken.
  /// </summary>
  [JsonPropertyName("readFrequencyCount")]
  [Member(Index = 3, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ReadFrequencyCount
  {
    get => readFrequencyCount;
    set => readFrequencyCount = value;
  }

  /// <summary>
  /// The value of the CHECKPOINT_COUNT attribute.
  /// This attribute is used to show how many checkpoints have been taken during
  /// the current run of this batch process.
  /// This value of this field is set to one the first time a checkpoint is 
  /// taken by the program and is then updated each time the program takes a
  /// checkpoint.
  /// </summary>
  [JsonPropertyName("checkpointCount")]
  [Member(Index = 4, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? CheckpointCount
  {
    get => checkpointCount;
    set => checkpointCount = value;
  }

  /// <summary>
  /// The value of the LAST_CHECKPOINT_TIMESTAMP attribute.
  /// This attribute is used to show the time that the last checkpoint was 
  /// taken.  This attribute will be updated each time a checkpoint is taken by
  /// the batch process.
  /// </summary>
  [JsonPropertyName("lastCheckpointTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastCheckpointTimestamp
  {
    get => lastCheckpointTimestamp;
    set => lastCheckpointTimestamp = value;
  }

  /// <summary>Length of the RESTART_IND attribute.</summary>
  public const int RestartInd_MaxLength = 1;

  /// <summary>
  /// The value of the RESTART_IND attribute.
  /// This attribute is used to tell the batch job whether or not it is in a 
  /// &quot;restart&quot; situtation.  If this field is &quot;Y&quot; that means
  /// that the previous run of this job failed with an abend and the batch
  /// process should start where the previous job stopped.  This is done by
  /// using the information contained in the restart info attribute.
  /// Example of how this attribute is used:
  /// The first time a checkpoint is taken this field is set to &quot;Y&quot;.
  /// At the successfull completion of this job this field is set back to &quot;
  /// N&quot;.
  /// </summary>
  [JsonPropertyName("restartInd")]
  [Member(Index = 6, Type = MemberType.Char, Length = RestartInd_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("N")]
  [Value("Y")]
  public string RestartInd
  {
    get => restartInd;
    set => restartInd = value != null
      ? TrimEnd(Substring(value, 1, RestartInd_MaxLength)) : null;
  }

  /// <summary>Length of the RESTART_INFO attribute.</summary>
  public const int RestartInfo_MaxLength = 250;

  /// <summary>
  /// The value of the RESTART_INFO attribute.
  /// This attribute may contain some key information that will enable the 
  /// process to restart at a point other than the begining of the job.  Each
  /// job will have to interrogate the the restart_info attribute and break it
  /// into its separate components.
  /// </summary>
  [JsonPropertyName("restartInfo")]
  [Member(Index = 7, Type = MemberType.Char, Length = RestartInfo_MaxLength, Optional
    = true)]
  public string RestartInfo
  {
    get => restartInfo;
    set => restartInfo = value != null
      ? TrimEnd(Substring(value, 1, RestartInfo_MaxLength)) : null;
  }

  private string programName;
  private int? updateFrequencyCount;
  private int? readFrequencyCount;
  private int? checkpointCount;
  private DateTime? lastCheckpointTimestamp;
  private string restartInd;
  private string restartInfo;
}

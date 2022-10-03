// The source file: TEMP_CHECKPOINT_RESTART, ID: 371802971, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class TempCheckpointRestart: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public TempCheckpointRestart()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public TempCheckpointRestart(TempCheckpointRestart that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new TempCheckpointRestart Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(TempCheckpointRestart that)
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
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 7)]
  public int UpdateFrequencyCount
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
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 7)]
  public int ReadFrequencyCount
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
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 7)]
  public int CheckpointCount
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
  [Member(Index = 5, Type = MemberType.Timestamp)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = RestartInd_MaxLength)]
  [Value("N")]
  [Value("Y")]
  public string RestartInd
  {
    get => restartInd ?? "";
    set => restartInd = TrimEnd(Substring(value, 1, RestartInd_MaxLength));
  }

  /// <summary>
  /// The json value of the RestartInd attribute.</summary>
  [JsonPropertyName("restartInd")]
  [Computed]
  public string RestartInd_Json
  {
    get => NullIf(RestartInd, "");
    set => RestartInd = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = RestartInfo_MaxLength)]
  public string RestartInfo
  {
    get => restartInfo ?? "";
    set => restartInfo = TrimEnd(Substring(value, 1, RestartInfo_MaxLength));
  }

  /// <summary>
  /// The json value of the RestartInfo attribute.</summary>
  [JsonPropertyName("restartInfo")]
  [Computed]
  public string RestartInfo_Json
  {
    get => NullIf(RestartInfo, "");
    set => RestartInfo = value;
  }

  private string programName;
  private int updateFrequencyCount;
  private int readFrequencyCount;
  private int checkpointCount;
  private DateTime? lastCheckpointTimestamp;
  private string restartInd;
  private string restartInfo;
}

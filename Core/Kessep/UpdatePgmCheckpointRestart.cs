// Program: UPDATE_PGM_CHECKPOINT_RESTART, ID: 371787294, model: 746.
// Short name: SWE01491
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_PGM_CHECKPOINT_RESTART.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will read and then update the Program Checkpoint Restart 
/// row for a particular program.
/// </para>
/// </summary>
[Serializable]
public partial class UpdatePgmCheckpointRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PGM_CHECKPOINT_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdatePgmCheckpointRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdatePgmCheckpointRestart.
  /// </summary>
  public UpdatePgmCheckpointRestart(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // 11/16/1998    elyman      added exception conditions
    // *****************************************************************
    if (ReadProgramCheckpointRestart())
    {
      try
      {
        UpdateProgramCheckpointRestart();
        export.ProgramCheckpointRestart.
          Assign(entities.ProgramCheckpointRestart);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_CHECKPOINT_RESTART_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF";
    }
  }

  private bool ReadProgramCheckpointRestart()
  {
    entities.ProgramCheckpointRestart.Populated = false;

    return Read("ReadProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(
          command, "programName", import.ProgramCheckpointRestart.ProgramName);
      },
      (db, reader) =>
      {
        entities.ProgramCheckpointRestart.ProgramName = db.GetString(reader, 0);
        entities.ProgramCheckpointRestart.UpdateFrequencyCount =
          db.GetNullableInt32(reader, 1);
        entities.ProgramCheckpointRestart.ReadFrequencyCount =
          db.GetNullableInt32(reader, 2);
        entities.ProgramCheckpointRestart.CheckpointCount =
          db.GetNullableInt32(reader, 3);
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ProgramCheckpointRestart.RestartInd =
          db.GetNullableString(reader, 5);
        entities.ProgramCheckpointRestart.RestartInfo =
          db.GetNullableString(reader, 6);
        entities.ProgramCheckpointRestart.Populated = true;
        CheckValid<ProgramCheckpointRestart>("RestartInd",
          entities.ProgramCheckpointRestart.RestartInd);
      });
  }

  private void UpdateProgramCheckpointRestart()
  {
    var checkpointCount =
      import.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault() + 1;
    var lastCheckpointTimestamp = Now();
    var restartInd = import.ProgramCheckpointRestart.RestartInd ?? "";
    var restartInfo = import.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetNullableInt32(command, "checkpointCount", checkpointCount);
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.CheckpointCount = checkpointCount;
    entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
      lastCheckpointTimestamp;
    entities.ProgramCheckpointRestart.RestartInd = restartInd;
    entities.ProgramCheckpointRestart.RestartInfo = restartInfo;
    entities.ProgramCheckpointRestart.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
  }
#endregion
}

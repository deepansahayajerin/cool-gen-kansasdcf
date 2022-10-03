// Program: UPDATE_PGM_CHKPNT_RESTART, ID: 371743975, model: 746.
// Short name: SWE01492
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_PGM_CHKPNT_RESTART.
/// </para>
/// <para>
/// RESP: ALL
/// This aciton block will update the checkpoint restart info for a program.
/// </para>
/// </summary>
[Serializable]
public partial class UpdatePgmChkpntRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PGM_CHKPNT_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdatePgmChkpntRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdatePgmChkpntRestart.
  /// </summary>
  public UpdatePgmChkpntRestart(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // SYSTEM:  KESSEP
    // MODULE:
    // MODULE TYPE:
    // ACTION BLOCKS:
    // 	UPDATE_PGM_CHKPNT_RESTART
    // ENTITY TYPES USED:
    // 	PROGRAM_CHECKPOINT_RESTART -U-
    // MAINTENANCE LOG:
    // DATE       AUTHOR                DESCRIPTION
    // 07/01/96   Sherri Newman - DIR   Initial Code
    // ---------------------------------------------
    if (ReadProgramCheckpointRestart())
    {
      try
      {
        UpdateProgramCheckpointRestart();
        ExitState = "ACO_NN0000_ALL_OK";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

            break;
          case ErrorCode.PermittedValueViolation:
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
    var updateFrequencyCount =
      import.ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault();
    var readFrequencyCount =
      import.ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault();
    var checkpointCount =
      import.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault();
    var lastCheckpointTimestamp =
      import.ProgramCheckpointRestart.LastCheckpointTimestamp;
    var restartInd = import.ProgramCheckpointRestart.RestartInd ?? "";
    var restartInfo = import.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetNullableInt32(command, "updateFrequencyC", updateFrequencyCount);
        db.SetNullableInt32(command, "readFrequencyCou", readFrequencyCount);
        db.SetNullableInt32(command, "checkpointCount", checkpointCount);
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

    entities.ProgramCheckpointRestart.UpdateFrequencyCount =
      updateFrequencyCount;
    entities.ProgramCheckpointRestart.ReadFrequencyCount = readFrequencyCount;
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

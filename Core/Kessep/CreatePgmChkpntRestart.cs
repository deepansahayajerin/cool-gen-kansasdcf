// Program: CREATE_PGM_CHKPNT_RESTART, ID: 371743972, model: 746.
// Short name: SWE00152
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_PGM_CHKPNT_RESTART.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will create a new program_checkpoint_restart row.
/// </para>
/// </summary>
[Serializable]
public partial class CreatePgmChkpntRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_PGM_CHKPNT_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreatePgmChkpntRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreatePgmChkpntRestart.
  /// </summary>
  public CreatePgmChkpntRestart(IContext context, Import import, Export export):
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
    // 	CREATE_PGM_CHKPNT_RESTART
    // 	
    // ENTITY TYPES USED:
    // 	PROGRAM_CHECKPOINT_RESTART -C-
    // MAINTENANCE LOG:
    // DATE       AUTHOR                DESCRIPTION
    // 07/01/96   Sherri Newman - DIR   Initial Code
    // ---------------------------------------------
    try
    {
      CreateProgramCheckpointRestart();
      ExitState = "ACO_NN0000_ALL_OK";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_AE";

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

  private void CreateProgramCheckpointRestart()
  {
    var programName = import.ProgramCheckpointRestart.ProgramName;
    var updateFrequencyCount =
      import.ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault();
    var readFrequencyCount =
      import.ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault();
    var checkpointCount =
      import.ProgramCheckpointRestart.CheckpointCount.GetValueOrDefault();
    var restartInd = "N";
    var restartInfo = import.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("CreateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(command, "programName", programName);
        db.SetNullableInt32(command, "updateFrequencyC", updateFrequencyCount);
        db.SetNullableInt32(command, "readFrequencyCou", readFrequencyCount);
        db.SetNullableInt32(command, "checkpointCount", checkpointCount);
        db.SetNullableDateTime(command, "lstChkpntTmst", null);
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
      });

    entities.ProgramCheckpointRestart.ProgramName = programName;
    entities.ProgramCheckpointRestart.UpdateFrequencyCount =
      updateFrequencyCount;
    entities.ProgramCheckpointRestart.ReadFrequencyCount = readFrequencyCount;
    entities.ProgramCheckpointRestart.CheckpointCount = checkpointCount;
    entities.ProgramCheckpointRestart.LastCheckpointTimestamp = null;
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

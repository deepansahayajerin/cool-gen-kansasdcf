// Program: DELETE_PGM_CHKPNT_RESTART, ID: 371743974, model: 746.
// Short name: SWE00189
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DELETE_PGM_CHKPNT_RESTART.
/// </para>
/// <para>
/// RESP: ALL	
/// This action block will delete a row from the program_checkpoint_restart 
/// table.
/// </para>
/// </summary>
[Serializable]
public partial class DeletePgmChkpntRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_PGM_CHKPNT_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeletePgmChkpntRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeletePgmChkpntRestart.
  /// </summary>
  public DeletePgmChkpntRestart(IContext context, Import import, Export export):
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
    // 	DELETE_PGM_CHKPNT_RESTART
    // ENTITY TYPES USED:
    // 	PROGRAM_CHECKPOINT_RESTART -D-
    // MAINTENANCE LOG:
    // DATE       AUTHOR                DESCRIPTION
    // 07/01/96   Sherri Newman - DIR   Initial Code
    // ---------------------------------------------
    if (ReadProgramCheckpointRestart())
    {
      DeleteProgramCheckpointRestart();
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF";
    }
  }

  private void DeleteProgramCheckpointRestart()
  {
    Update("DeleteProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });
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
        entities.ProgramCheckpointRestart.Populated = true;
      });
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

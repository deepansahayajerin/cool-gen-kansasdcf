// Program: READ_PGM_CHKPNT_RESTART, ID: 371743973, model: 746.
// Short name: SWE01035
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: READ_PGM_CHKPNT_RESTART.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will read the program_checkpoint_restart table.
/// </para>
/// </summary>
[Serializable]
public partial class ReadPgmChkpntRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the READ_PGM_CHKPNT_RESTART program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReadPgmChkpntRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReadPgmChkpntRestart.
  /// </summary>
  public ReadPgmChkpntRestart(IContext context, Import import, Export export):
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
    // 	READ_PGM_CHKPNT_RESTART
    // ENTITY TYPES USED:
    // 	PROGRAM_CHECKPOINT_RESTART -R-
    // MAINTENANCE LOG:
    // DATE       AUTHOR                DESCRIPTION
    // 07/01/96   Sherri Newman - DIR   Initial Code
    // ---------------------------------------------
    if (ReadProgramCheckpointRestart())
    {
      export.ProgramCheckpointRestart.Assign(entities.ProgramCheckpointRestart);
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

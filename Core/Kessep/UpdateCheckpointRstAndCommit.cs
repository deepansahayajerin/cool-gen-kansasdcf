// Program: UPDATE_CHECKPOINT_RST_AND_COMMIT, ID: 371092724, model: 746.
// Short name: SWE02935
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_CHECKPOINT_RST_AND_COMMIT.
/// </summary>
[Serializable]
public partial class UpdateCheckpointRstAndCommit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CHECKPOINT_RST_AND_COMMIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCheckpointRstAndCommit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCheckpointRstAndCommit.
  /// </summary>
  public UpdateCheckpointRstAndCommit(IContext context, Import import,
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
    if (!ReadProgramCheckpointRestart())
    {
      ExitState = "PROGRAM_CHECKPOINT_RESTART_NF";

      return;
    }

    try
    {
      UpdateProgramCheckpointRestart();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_CHECKPOINT_RESTART_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    UseExtToDoACommit();

    if (local.ForCommit.NumericReturnCode != 0)
    {
      ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";
    }
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
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
        entities.ProgramCheckpointRestart.LastCheckpointTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.ProgramCheckpointRestart.RestartInd =
          db.GetNullableString(reader, 2);
        entities.ProgramCheckpointRestart.RestartInfo =
          db.GetNullableString(reader, 3);
        entities.ProgramCheckpointRestart.Populated = true;
      });
  }

  private void UpdateProgramCheckpointRestart()
  {
    var lastCheckpointTimestamp = Now();
    var restartInd = import.ProgramCheckpointRestart.RestartInd ?? "";
    var restartInfo = import.ProgramCheckpointRestart.RestartInfo ?? "";

    CheckValid<ProgramCheckpointRestart>("RestartInd", restartInd);
    entities.ProgramCheckpointRestart.Populated = false;
    Update("UpdateProgramCheckpointRestart",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lstChkpntTmst", lastCheckpointTimestamp);
          
        db.SetNullableString(command, "restartInd", restartInd);
        db.SetNullableString(command, "restartInfo", restartInfo);
        db.SetString(
          command, "programName",
          entities.ProgramCheckpointRestart.ProgramName);
      });

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
  protected readonly Local local = new();
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private External forCommit;
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

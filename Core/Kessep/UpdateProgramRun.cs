// Program: UPDATE_PROGRAM_RUN, ID: 371801925, model: 746.
// Short name: SWE01502
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_PROGRAM_RUN.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will update a row in the Program_Run table.  This is done 
/// to add the stop timestamp for the current program.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateProgramRun: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PROGRAM_RUN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateProgramRun(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateProgramRun.
  /// </summary>
  public UpdateProgramRun(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadProgramRun())
    {
      try
      {
        UpdateProgramRun1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ZD_PROGRAM_RUN_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "ZD_PROGRAM_RUN_PV_AB";

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
      ExitState = "PROGRAM_RUN_NF_RB";
    }
  }

  private bool ReadProgramRun()
  {
    entities.ProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
        db.SetDateTime(
          command, "startTimestamp",
          import.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetString(command, "ppiName", import.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          import.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramRun.PpiCreatedTstamp = db.GetDateTime(reader, 0);
        entities.ProgramRun.PpiName = db.GetString(reader, 1);
        entities.ProgramRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ProgramRun.EndTimestamp = db.GetNullableDateTime(reader, 3);
        entities.ProgramRun.Populated = true;
      });
  }

  private void UpdateProgramRun1()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramRun.Populated);

    var endTimestamp = Now();

    entities.ProgramRun.Populated = false;
    Update("UpdateProgramRun",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "endTimestamp", endTimestamp);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          entities.ProgramRun.PpiCreatedTstamp.GetValueOrDefault());
        db.SetString(command, "ppiName", entities.ProgramRun.PpiName);
        db.SetDateTime(
          command, "startTimestamp",
          entities.ProgramRun.StartTimestamp.GetValueOrDefault());
      });

    entities.ProgramRun.EndTimestamp = endTimestamp;
    entities.ProgramRun.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }
#endregion
}

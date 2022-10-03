// Program: CREATE_PROGRAM_RUN, ID: 371801926, model: 746.
// Short name: SWE00160
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_PROGRAM_RUN.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will create a row on the Program_Run table.  We do this to
/// capture the start and stop timestamp for the current run of the program.
/// </para>
/// </summary>
[Serializable]
public partial class CreateProgramRun: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_PROGRAM_RUN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateProgramRun(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateProgramRun.
  /// </summary>
  public CreateProgramRun(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadProgramProcessingInfo())
    {
      try
      {
        CreateProgramRun1();
        export.ProgramRun.StartTimestamp = entities.ProgramRun.StartTimestamp;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ZD_PROGRAM_RUN_AE_AB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "ZD_PROGRAM_RUN_PV_AB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";

      return;
    }

    // *****  Commit the program run to the database.  This commit must be done 
    // here instead of at the first checkpoint because if the program aborts
    // before the first commit, the program run information should not be rolled
    // back.
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
    }
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void CreateProgramRun1()
  {
    var ppiCreatedTstamp = entities.ProgramProcessingInfo.CreatedTimestamp;
    var ppiName = entities.ProgramProcessingInfo.Name;
    var startTimestamp = Now();
    var fromRestartInd = import.ProgramRun.FromRestartInd ?? "";

    entities.ProgramRun.Populated = false;
    Update("CreateProgramRun",
      (db, command) =>
      {
        db.SetDateTime(command, "ppiCreatedTstamp", ppiCreatedTstamp);
        db.SetString(command, "ppiName", ppiName);
        db.SetDateTime(command, "startTimestamp", startTimestamp);
        db.SetNullableString(command, "fromRestartInd", fromRestartInd);
        db.SetNullableDateTime(command, "endTimestamp", default(DateTime));
      });

    entities.ProgramRun.PpiCreatedTstamp = ppiCreatedTstamp;
    entities.ProgramRun.PpiName = ppiName;
    entities.ProgramRun.StartTimestamp = startTimestamp;
    entities.ProgramRun.FromRestartInd = fromRestartInd;
    entities.ProgramRun.Populated = true;
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", import.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.Populated = true;
      });
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

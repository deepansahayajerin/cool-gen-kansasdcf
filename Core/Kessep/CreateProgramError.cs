// Program: CREATE_PROGRAM_ERROR, ID: 371801924, model: 746.
// Short name: SWE00158
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_PROGRAM_ERROR.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will create a row on the Program_Error table.  This is 
/// done because a batch job has encountered an error and we choose to write
/// this out to a DB2 table instead of an error report.
/// </para>
/// </summary>
[Serializable]
public partial class CreateProgramError: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_PROGRAM_ERROR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateProgramError(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateProgramError.
  /// </summary>
  public CreateProgramError(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // CHANGE LOG:
    // Removed coding using persistent view matching and
    // replaced with entity action read of Program Run. The use of
    // persistent view matching was generating deadlocks when
    // various batch jobs were run concurrently.  J.Rookard
    // 11-18-97
    // ------------------------------------------------------------
    if (ReadProgramRun())
    {
      ++export.ImportNumberOfReads.Count;
    }

    if (export.ImportNumberOfReads.Count == 0)
    {
      ExitState = "PROGRAM_RUN_NF_RB";
    }
    else
    {
      // ***** HARD CODE AREA *****
      local.HardcodeResolved.StatusInd = "R";
      local.HardcodeUnresolved.StatusInd = "U";

      // ------------------------------------------------------------
      // The Program_Error is associated to a Program_Run that is
      // read for currency in the prad and passed to this action block
      // as a persistent view.
      // ------------------------------------------------------------
      try
      {
        CreateProgramError1();
        ++export.ImportNumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_ERROR_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_ERROR_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void CreateProgramError1()
  {
    System.Diagnostics.Debug.Assert(entities.ProgramRun.Populated);

    var prrStartTstamp = entities.ProgramRun.StartTimestamp;
    var ppiCreatedTstamp = entities.ProgramRun.PpiCreatedTstamp;
    var ppiName = entities.ProgramRun.PpiName;
    var systemGeneratedIdentifier =
      import.ProgramError.SystemGeneratedIdentifier;
    var code = import.ProgramError.Code ?? "";
    var statusDate = Now().Date;
    var statusInd = local.HardcodeUnresolved.StatusInd ?? "";
    var programError1 = import.ProgramError.ProgramError1 ?? "";
    var keyInfo = import.ProgramError.KeyInfo ?? "";

    CheckValid<ProgramError>("StatusInd", statusInd);
    entities.ProgramError.Populated = false;
    Update("CreateProgramError",
      (db, command) =>
      {
        db.SetDateTime(command, "prrStartTstamp", prrStartTstamp);
        db.SetDateTime(command, "ppiCreatedTstamp", ppiCreatedTstamp);
        db.SetString(command, "ppiName", ppiName);
        db.SetInt32(command, "pgmErrorId", systemGeneratedIdentifier);
        db.SetNullableString(command, "code", code);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "statusInd", statusInd);
        db.SetNullableString(command, "programError", programError1);
        db.SetNullableString(command, "keyInfo", keyInfo);
        db.SetNullableString(command, "resolution", "");
      });

    entities.ProgramError.PrrStartTstamp = prrStartTstamp;
    entities.ProgramError.PpiCreatedTstamp = ppiCreatedTstamp;
    entities.ProgramError.PpiName = ppiName;
    entities.ProgramError.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.ProgramError.Code = code;
    entities.ProgramError.StatusDate = statusDate;
    entities.ProgramError.StatusInd = statusInd;
    entities.ProgramError.ProgramError1 = programError1;
    entities.ProgramError.KeyInfo = keyInfo;
    entities.ProgramError.Populated = true;
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
        entities.ProgramRun.Populated = true;
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
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
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

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramError programError;
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodeResolved.
    /// </summary>
    [JsonPropertyName("hardcodeResolved")]
    public ProgramError HardcodeResolved
    {
      get => hardcodeResolved ??= new();
      set => hardcodeResolved = value;
    }

    /// <summary>
    /// A value of HardcodeUnresolved.
    /// </summary>
    [JsonPropertyName("hardcodeUnresolved")]
    public ProgramError HardcodeUnresolved
    {
      get => hardcodeUnresolved ??= new();
      set => hardcodeUnresolved = value;
    }

    private ProgramError hardcodeResolved;
    private ProgramError hardcodeUnresolved;
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

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramError programError;
  }
#endregion
}

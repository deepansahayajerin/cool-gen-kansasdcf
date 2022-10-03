// Program: GB_B003_CREATE_CHKPNT_R_POST_CNV, ID: 372697498, model: 746.
// Short name: SWEG003B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_B003_CREATE_CHKPNT_R_POST_CNV.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class GbB003CreateChkpntRPostCnv: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_B003_CREATE_CHKPNT_R_POST_CNV program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbB003CreateChkpntRPostCnv(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbB003CreateChkpntRPostCnv.
  /// </summary>
  public GbB003CreateChkpntRPostCnv(IContext context, Import import,
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
    // BATCH Create Program Checkpoint Restart Records Post-Conversion.  This is
    // a one time batch job that should be run after b002
    // *****************************************************************
    // *****************************************************************
    // Maintenance Log:
    // Author		Date		Description
    // E. Parker		06/01/1998	Initial development.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ** CREATE PROGRAM CHECKPOINT RESTART FOR ALL ACTIVE PROGRAMS **
    foreach(var item in ReadPgmNameTable())
    {
      local.ProgramCheckpointRestart.ProgramName =
        entities.PgmNameTable.PgmName;
      local.TextWorkArea.Text4 =
        Substring(entities.PgmNameTable.PgmParmList, 1, 4);
      local.ProgramCheckpointRestart.UpdateFrequencyCount =
        (int?)StringToNumber(local.TextWorkArea.Text4);
      local.ProgramCheckpointRestart.ReadFrequencyCount =
        (int?)StringToNumber(local.TextWorkArea.Text4);

      try
      {
        CreateProgramCheckpointRestart();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_CHECKPOINT_RESTART_AE_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_CHECKPOINT_RESTART_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabRollbackSql();
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    Call(EabRollbackSql.Execute, useImport, useExport);
  }

  private void CreateProgramCheckpointRestart()
  {
    var programName = local.ProgramCheckpointRestart.ProgramName;
    var updateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault();
    var readFrequencyCount =
      local.ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault();

    entities.ProgramCheckpointRestart.Populated = false;
    Update("CreateProgramCheckpointRestart",
      (db, command) =>
      {
        db.SetString(command, "programName", programName);
        db.SetNullableInt32(command, "updateFrequencyC", updateFrequencyCount);
        db.SetNullableInt32(command, "readFrequencyCou", readFrequencyCount);
        db.SetNullableInt32(command, "checkpointCount", 0);
        db.SetNullableDateTime(command, "lstChkpntTmst", default(DateTime));
        db.SetNullableString(command, "restartInd", "");
        db.SetNullableString(command, "restartInfo", "");
      });

    entities.ProgramCheckpointRestart.ProgramName = programName;
    entities.ProgramCheckpointRestart.UpdateFrequencyCount =
      updateFrequencyCount;
    entities.ProgramCheckpointRestart.ReadFrequencyCount = readFrequencyCount;
    entities.ProgramCheckpointRestart.Populated = true;
  }

  private IEnumerable<bool> ReadPgmNameTable()
  {
    entities.PgmNameTable.Populated = false;

    return ReadEach("ReadPgmNameTable",
      null,
      (db, reader) =>
      {
        entities.PgmNameTable.PgmName = db.GetString(reader, 0);
        entities.PgmNameTable.PgmDescription = db.GetString(reader, 1);
        entities.PgmNameTable.PgmType = db.GetString(reader, 2);
        entities.PgmNameTable.PgmActive = db.GetString(reader, 3);
        entities.PgmNameTable.PgmParmList = db.GetNullableString(reader, 4);
        entities.PgmNameTable.Populated = true;

        return true;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private Common common;
    private TextWorkArea textWorkArea;
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

    /// <summary>
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private PgmNameTable pgmNameTable;
  }
#endregion
}

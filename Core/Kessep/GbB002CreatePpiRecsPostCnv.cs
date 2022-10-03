// Program: GB_B002_CREATE_PPI_RECS_POST_CNV, ID: 372697143, model: 746.
// Short name: SWEG002B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_B002_CREATE_PPI_RECS_POST_CNV.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class GbB002CreatePpiRecsPostCnv: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_B002_CREATE_PPI_RECS_POST_CNV program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbB002CreatePpiRecsPostCnv(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbB002CreatePpiRecsPostCnv.
  /// </summary>
  public GbB002CreatePpiRecsPostCnv(IContext context, Import import,
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
    // BATCH Create Program Processing Information Records Post-Conversion.  
    // This is a one time batch job to be run after Conversion and prior to any
    // other Batch Jobs.
    // *****************************************************************
    // *****************************************************************
    // Maintenance Log:
    // Author		Date		Description
    // E. Parker		06/01/1999	Initial development.
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    local.ProgramProcessingInfo.CreatedBy = global.UserId;
    UseGbGetEndOfMnthDtForBatch();
    local.FirstDayOfLastMonth.Date = AddMonths(local.FirstDayOfMonth.Date, -1);

    // ** CREATE PROGRAM PROCESSING INFO FOR ALL ACTIVE PROGRAMS **
    // ** POSSIBLE PROGRAM TYPES:
    // **   DAY = DAILY
    // **   WEK = WEEKLY
    // **   MON = MONTHLY
    // **   QTR = QUARTERLY
    // **   ANN = ANNUAL
    // **   CNV = ONCE
    // **   EOM = Requires an End of Month Date.
    foreach(var item in ReadPgmNameTable())
    {
      if (Equal(entities.PgmNameTable.PgmType, "MAX"))
      {
        local.ProgramProcessingInfo.ProcessDate = local.Max.Date;
      }
      else if (Equal(entities.PgmNameTable.PgmType, "EOM"))
      {
        local.ProgramProcessingInfo.ProcessDate = local.EndOfMonth.Date;
      }
      else if (Equal(entities.PgmNameTable.PgmType, "FLM"))
      {
        local.ProgramProcessingInfo.ProcessDate =
          local.FirstDayOfLastMonth.Date;
      }
      else if (Equal(entities.PgmNameTable.PgmType, "FOM"))
      {
        local.ProgramProcessingInfo.ProcessDate = local.FirstDayOfMonth.Date;
      }
      else
      {
        local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
      }

      local.ProgramProcessingInfo.Name = entities.PgmNameTable.PgmName;
      local.ProgramProcessingInfo.Description =
        entities.PgmNameTable.PgmDescription;
      local.ProgramProcessingInfo.ParameterList =
        Substring(entities.PgmNameTable.PgmParmList,
        PgmNameTable.PgmParmList_MaxLength, 5, 50);

      try
      {
        CreateProgramProcessingInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_AE_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_PROCESSING_INFO_PV_AB";

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

  private void UseGbGetEndOfMnthDtForBatch()
  {
    var useImport = new GbGetEndOfMnthDtForBatch.Import();
    var useExport = new GbGetEndOfMnthDtForBatch.Export();

    useImport.Current.Date = local.Current.Date;

    Call(GbGetEndOfMnthDtForBatch.Execute, useImport, useExport);

    local.FirstDayOfMonth.Date = useExport.FirstDayOfMonth.Date;
    local.EndOfMonth.Date = useExport.EndOfMonth.Date;
  }

  private void CreateProgramProcessingInfo()
  {
    var name = local.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var processDate = local.ProgramProcessingInfo.ProcessDate;
    var createdBy = local.ProgramProcessingInfo.CreatedBy;
    var parameterList = local.ProgramProcessingInfo.ParameterList ?? "";
    var description = local.ProgramProcessingInfo.Description ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("CreateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetNullableString(command, "pgmProcInfoDesc", description);
      });

    entities.ProgramProcessingInfo.Name = name;
    entities.ProgramProcessingInfo.CreatedTimestamp = createdTimestamp;
    entities.ProgramProcessingInfo.ProcessDate = processDate;
    entities.ProgramProcessingInfo.CreatedBy = createdBy;
    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Description = description;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of FirstDayOfLastMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfLastMonth")]
    public DateWorkArea FirstDayOfLastMonth
    {
      get => firstDayOfLastMonth ??= new();
      set => firstDayOfLastMonth = value;
    }

    /// <summary>
    /// A value of FirstDayOfMonth.
    /// </summary>
    [JsonPropertyName("firstDayOfMonth")]
    public DateWorkArea FirstDayOfMonth
    {
      get => firstDayOfMonth ??= new();
      set => firstDayOfMonth = value;
    }

    /// <summary>
    /// A value of EndOfMonth.
    /// </summary>
    [JsonPropertyName("endOfMonth")]
    public DateWorkArea EndOfMonth
    {
      get => endOfMonth ??= new();
      set => endOfMonth = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea firstDayOfLastMonth;
    private DateWorkArea firstDayOfMonth;
    private DateWorkArea endOfMonth;
    private DateWorkArea max;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
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
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private PgmNameTable pgmNameTable;
  }
#endregion
}

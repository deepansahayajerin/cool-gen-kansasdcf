// Program: GB_B004_UPDATE_PPI_RECS_PRCSS_DT, ID: 372697616, model: 746.
// Short name: SWEG004B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_B004_UPDATE_PPI_RECS_PRCSS_DT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class GbB004UpdatePpiRecsPrcssDt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_B004_UPDATE_PPI_RECS_PRCSS_DT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbB004UpdatePpiRecsPrcssDt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbB004UpdatePpiRecsPrcssDt.
  /// </summary>
  public GbB004UpdatePpiRecsPrcssDt(IContext context, Import import,
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
    // ***************************************************************************************
    // Maintenance Log:
    // Author		Date		Description
    // E. Parker       06/01/1999	Initial development.
    // RMathews        12/27/2010      Stop adding new PPI record daily for 
    // SWEPB302, instead
    //                                 
    // update process date on most
    // recent record
    // ***************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);
    UseGbGetEndOfMnthDtForBatch();
    local.FirstOfLastMonth.Date = AddMonths(local.FirstDayOfMonth.Date, -1);

    foreach(var item in ReadProgramProcessingInfo2())
    {
      local.ProgramProcessingInfo.ProcessDate = AddDays(local.Current.Date, 1);

      if (!ReadPgmNameTable())
      {
        ExitState = "PGM_NAME_TABLE_NF_AB";

        break;
      }

      try
      {
        UpdateProgramProcessingInfo();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_NU_AB";

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
    }
    else
    {
      foreach(var item in ReadProgramProcessingInfo1())
      {
        if (Equal(entities.ProgramProcessingInfo.Name, "SWEPB301") || Equal
          (entities.ProgramProcessingInfo.Name, "SWEP301B") || Equal
          (entities.ProgramProcessingInfo.Name, "SWEPB302") || Equal
          (entities.ProgramProcessingInfo.Name, "SWEPB306"))
        {
          continue;
        }

        if (ReadPgmNameTable())
        {
          if (Equal(entities.PgmNameTable.PgmType, "EOM"))
          {
            local.ProgramProcessingInfo.ProcessDate = local.EndOfMonth.Date;
          }
          else if (Equal(entities.PgmNameTable.PgmType, "FLM"))
          {
            local.ProgramProcessingInfo.ProcessDate =
              local.FirstOfLastMonth.Date;
          }
          else if (Equal(entities.PgmNameTable.PgmType, "FOM"))
          {
            local.ProgramProcessingInfo.ProcessDate =
              local.FirstDayOfMonth.Date;
          }
          else if (Equal(entities.PgmNameTable.PgmName, "MAX"))
          {
            local.ProgramProcessingInfo.ProcessDate = local.Max.Date;
          }
          else
          {
            local.ProgramProcessingInfo.ProcessDate = local.Current.Date;
          }
        }
        else
        {
          ExitState = "PGM_NAME_TABLE_NF_AB";

          break;
        }

        try
        {
          UpdateProgramProcessingInfo();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_PROCESSING_INFO_NU_AB";

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

  private bool ReadPgmNameTable()
  {
    entities.PgmNameTable.Populated = false;

    return Read("ReadPgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.PgmNameTable.PgmName = db.GetString(reader, 0);
        entities.PgmNameTable.PgmDescription = db.GetString(reader, 1);
        entities.PgmNameTable.PgmType = db.GetString(reader, 2);
        entities.PgmNameTable.PgmActive = db.GetString(reader, 3);
        entities.PgmNameTable.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgramProcessingInfo1()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return ReadEach("ReadProgramProcessingInfo1",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.CreatedBy = db.GetString(reader, 3);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 4);
        entities.ProgramProcessingInfo.Description =
          db.GetNullableString(reader, 5);
        entities.ProgramProcessingInfo.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgramProcessingInfo2()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return ReadEach("ReadProgramProcessingInfo2",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.CreatedBy = db.GetString(reader, 3);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 4);
        entities.ProgramProcessingInfo.Description =
          db.GetNullableString(reader, 5);
        entities.ProgramProcessingInfo.Populated = true;

        return true;
      });
  }

  private void UpdateProgramProcessingInfo()
  {
    var processDate = local.ProgramProcessingInfo.ProcessDate;

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ProcessDate = processDate;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of FirstOfLastMonth.
    /// </summary>
    [JsonPropertyName("firstOfLastMonth")]
    public DateWorkArea FirstOfLastMonth
    {
      get => firstOfLastMonth ??= new();
      set => firstOfLastMonth = value;
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

    private DateWorkArea firstOfLastMonth;
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

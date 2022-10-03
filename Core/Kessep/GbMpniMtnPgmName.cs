// Program: GB_MPNI_MTN_PGM_NAME, ID: 373403484, model: 746.
// Short name: SWEMPNIP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_MPNI_MTN_PGM_NAME.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbMpniMtnPgmName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_MPNI_MTN_PGM_NAME program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbMpniMtnPgmName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbMpniMtnPgmName.
  /// </summary>
  public GbMpniMtnPgmName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial development September, 2001 by Larry J. Bachura
    // Changed 9-19-01 @ 3:45.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.Hidden.Assign(import.NextTranInfo);
    export.PgmNameTable.Assign(import.PgmNameTable);
    export.HiddenId.PgmName = import.HiddenId.PgmName;
    MoveDateWorkArea(import.DateTime, export.DateTime);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // NEXT TRAN -- - If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action. Validate.
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      return;
    }

    // -------  Menu logic follows
    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // ----  Screen Edits Follow
    // --  UPDATE or DELETE cannot be done  without a display being done first.
    // --  Validate action level security
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") && IsEmpty
      (import.PgmNameTable.PgmName))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.PgmNameTable, "pgmName");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.  If key 
    // field is blank exit from PRAD prior to populating export views so that
    // the screen is cleared.
    if ((Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE")) && IsEmpty(import.PgmNameTable.PgmName))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.PgmNameTable, "pgmName");

      field.Error = true;

      return;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (AsChar(import.PgmNameTable.PgmActive) != 'Y' && AsChar
        (import.PgmNameTable.PgmActive) != 'N')
      {
        ExitState = "ACO_NE0000_PGM_ACTIVE_CODE";

        var field = GetField(export.PgmNameTable, "pgmActive");

        field.Error = true;

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE"))
    {
      if (IsEmpty(export.PgmNameTable.PgmName))
      {
        var field = GetField(export.PgmNameTable, "pgmName");

        field.Error = true;

        ExitState = "KEY_FIELD_IS_BLANK";

        return;
      }
    }

    if (Equal(global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (!Equal(export.PgmNameTable.PgmName, export.HiddenId.PgmName))
      {
        ExitState = "FN0000_DISPLAY_BEFORE_UPD_DEL";

        var field = GetField(export.PgmNameTable, "pgmName");

        field.Error = true;

        return;
      }
    }

    // Validation common to DELETE and UPDATE.
    // If an error is found, EXIT STATE should
    //  be set.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      // ---------------------------------------------
      // Cannot update or delete an occurrence of Program Name table if any runs
      // have been used against it, i.e. no relationship exists between Program
      // name table and Program Run.
      // ---------------------------------------------
      if (ReadProgramRun())
      {
        if (Equal(global.Command, "UPDATE"))
        {
          ExitState = "GB0000_PGM_TABLE_INF_UPD_NOT_ALL";
        }
        else
        {
          ExitState = "GB0000_PGM_TABLE_INF_DEL_NOT_ALL";
        }

        return;
      }
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "MPCR":
        export.ProgramCheckpointRestart.ProgramName =
          import.PgmNameTable.PgmName;
        ExitState = "ECO_LNK_TO_MPCR";

        break;
      case "MPPI":
        export.ProgramProcessingInfo.Name = import.PgmNameTable.PgmName;
        ExitState = "ECO_LNK_TO_MPPI";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "DISPLAY":
        if (ReadPgmNameTable())
        {
          export.PgmNameTable.Assign(entities.PgmNameTable);
          export.HiddenId.PgmName = export.PgmNameTable.PgmName;
        }
        else
        {
          export.HiddenId.PgmName = "";
          ExitState = "PGM_NAME_TABLE_NF";
        }

        break;
      case "ADD":
        try
        {
          CreatePgmNameTable();

          // Set the hidden key field to that of the new record.
          export.PgmNameTable.CreatedTimestamp = Now();
          export.PgmNameTable.CreatedBy = global.UserId;
          export.HiddenId.PgmName = export.PgmNameTable.PgmName;
          ExitState = "ACO_NI0000_CREATE_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "PROGRAM_TABLE_INFO_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "PROGRAM_TABLE_INFO_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        break;
      case "UPDATE":
        if (ReadPgmNameTable())
        {
          try
          {
            UpdatePgmNameTable();
            export.PgmNameTable.Assign(import.PgmNameTable);
            export.PgmNameTable.UpdatedTimestamp = Now();
            export.PgmNameTable.UpdatedBy = global.UserId;
            export.HiddenId.PgmName = export.PgmNameTable.PgmName;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PROGRAM_TABLE_INFO_PV";

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
          ExitState = "PGM_NAME_TABLE_NF";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        break;
      case "DELETE":
        if (ReadPgmNameTable())
        {
          DeletePgmNameTable();

          // Set the hidden key field to spaces or zero.
          export.HiddenId.PgmName = "";
          ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
        }
        else
        {
          ExitState = "PGM_NAME_TABLE_NF";
        }

        break;
      case "LIST":
        if (AsChar(import.Prompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_SCREEN";
        }
        else
        {
          export.Prompt.SelectChar = import.Prompt.SelectChar;

          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void CreatePgmNameTable()
  {
    var pgmName = export.PgmNameTable.PgmName;
    var pgmDescription = export.PgmNameTable.PgmDescription;
    var pgmType = export.PgmNameTable.PgmType;
    var pgmActive = export.PgmNameTable.PgmActive;
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var pgmParmList = export.PgmNameTable.PgmParmList ?? "";

    entities.PgmNameTable.Populated = false;
    Update("CreatePgmNameTable",
      (db, command) =>
      {
        db.SetDate(command, "lastRunDate", null);
        db.SetString(command, "pgmName", pgmName);
        db.SetString(command, "pgmDescription", pgmDescription);
        db.SetString(command, "pgmType", pgmType);
        db.SetString(command, "pgmActive", pgmActive);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "updatedTimestamp", null);
        db.SetString(command, "updatedBy", "");
        db.SetNullableString(command, "pgmParmList", pgmParmList);
      });

    entities.PgmNameTable.LastRunDate = null;
    entities.PgmNameTable.PgmName = pgmName;
    entities.PgmNameTable.PgmDescription = pgmDescription;
    entities.PgmNameTable.PgmType = pgmType;
    entities.PgmNameTable.PgmActive = pgmActive;
    entities.PgmNameTable.CreatedTimestamp = createdTimestamp;
    entities.PgmNameTable.CreatedBy = createdBy;
    entities.PgmNameTable.UpdatedTimestamp = null;
    entities.PgmNameTable.UpdatedBy = "";
    entities.PgmNameTable.PgmParmList = pgmParmList;
    entities.PgmNameTable.Populated = true;
  }

  private void DeletePgmNameTable()
  {
    Update("DeletePgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "pgmName", entities.PgmNameTable.PgmName);
      });
  }

  private bool ReadPgmNameTable()
  {
    entities.PgmNameTable.Populated = false;

    return Read("ReadPgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "pgmName", import.PgmNameTable.PgmName);
      },
      (db, reader) =>
      {
        entities.PgmNameTable.LastRunDate = db.GetDate(reader, 0);
        entities.PgmNameTable.PgmName = db.GetString(reader, 1);
        entities.PgmNameTable.PgmDescription = db.GetString(reader, 2);
        entities.PgmNameTable.PgmType = db.GetString(reader, 3);
        entities.PgmNameTable.PgmActive = db.GetString(reader, 4);
        entities.PgmNameTable.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PgmNameTable.CreatedBy = db.GetString(reader, 6);
        entities.PgmNameTable.UpdatedTimestamp = db.GetDateTime(reader, 7);
        entities.PgmNameTable.UpdatedBy = db.GetString(reader, 8);
        entities.PgmNameTable.PgmParmList = db.GetNullableString(reader, 9);
        entities.PgmNameTable.Populated = true;
      });
  }

  private bool ReadProgramRun()
  {
    entities.ProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
        db.SetString(command, "pgmName", import.PgmNameTable.PgmName);
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

  private void UpdatePgmNameTable()
  {
    var lastRunDate = import.PgmNameTable.LastRunDate;
    var pgmDescription = import.PgmNameTable.PgmDescription;
    var pgmType = import.PgmNameTable.PgmType;
    var pgmActive = import.PgmNameTable.PgmActive;
    var createdTimestamp = import.PgmNameTable.CreatedTimestamp;
    var createdBy = import.PgmNameTable.CreatedBy;
    var updatedTimestamp = Now();
    var updatedBy = global.UserId;
    var pgmParmList = import.PgmNameTable.PgmParmList ?? "";

    entities.PgmNameTable.Populated = false;
    Update("UpdatePgmNameTable",
      (db, command) =>
      {
        db.SetDate(command, "lastRunDate", lastRunDate);
        db.SetString(command, "pgmDescription", pgmDescription);
        db.SetString(command, "pgmType", pgmType);
        db.SetString(command, "pgmActive", pgmActive);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetString(command, "updatedBy", updatedBy);
        db.SetNullableString(command, "pgmParmList", pgmParmList);
        db.SetString(command, "pgmName", entities.PgmNameTable.PgmName);
      });

    entities.PgmNameTable.LastRunDate = lastRunDate;
    entities.PgmNameTable.PgmDescription = pgmDescription;
    entities.PgmNameTable.PgmType = pgmType;
    entities.PgmNameTable.PgmActive = pgmActive;
    entities.PgmNameTable.CreatedTimestamp = createdTimestamp;
    entities.PgmNameTable.CreatedBy = createdBy;
    entities.PgmNameTable.UpdatedTimestamp = updatedTimestamp;
    entities.PgmNameTable.UpdatedBy = updatedBy;
    entities.PgmNameTable.PgmParmList = pgmParmList;
    entities.PgmNameTable.Populated = true;
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

    /// <summary>
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PgmNameTable HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of DateTime.
    /// </summary>
    [JsonPropertyName("dateTime")]
    public DateWorkArea DateTime
    {
      get => dateTime ??= new();
      set => dateTime = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private NextTranInfo nextTranInfo;
    private PgmNameTable pgmNameTable;
    private PgmNameTable hiddenId;
    private DateWorkArea dateTime;
    private Standard standard;
    private ProgramProcessingInfo programProcessingInfo;
    private Common prompt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public PgmNameTable HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of DateTime.
    /// </summary>
    [JsonPropertyName("dateTime")]
    public DateWorkArea DateTime
    {
      get => dateTime ??= new();
      set => dateTime = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private NextTranInfo hidden;
    private PgmNameTable pgmNameTable;
    private PgmNameTable hiddenId;
    private DateWorkArea dateTime;
    private Standard standard;
    private Common prompt;
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
    /// A value of PgmNameTable.
    /// </summary>
    [JsonPropertyName("pgmNameTable")]
    public PgmNameTable PgmNameTable
    {
      get => pgmNameTable ??= new();
      set => pgmNameTable = value;
    }

    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private PgmNameTable pgmNameTable;
  }
#endregion
}

// Program: GB_MPPI_MTN_PGM_PROCESSING_INFO, ID: 371744217, model: 746.
// Short name: SWEMPPIP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_MPPI_MTN_PGM_PROCESSING_INFO.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbMppiMtnPgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_MPPI_MTN_PGM_PROCESSING_INFO program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbMppiMtnPgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbMppiMtnPgmProcessingInfo.
  /// </summary>
  public GbMppiMtnPgmProcessingInfo(IContext context, Import import,
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
    // ---------------------------------------------
    // Every initial development and change to that development needs to be 
    // documented.
    // ---------------------------------------------
    // ----------------------------------------------------------------------------
    // Date      Developer Name    Request #  Description
    // 04/04/96  Burrell - SRS                New Development
    // 12/12/96  R. Marchman                  Add new security and next tran
    // 07/25/06  GVandy	    WR230751-Segment H
    // 				       Increase parameter length to 240.
    // 04/26/07  mwf               pr307185   Modified to add new export entity 
    // view
    //                                        
    // to pass value to MPCR.
    // ----------------------------------------------------------------------------
    // ---------------------------------------------
    // Parameters passed when called by program LPPI
    // ---------------------------------------------
    // *  Program_processing_info_name
    // *  Program_processing_info_created_timestamp
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "SIGNOFF"))
    {
      UseScCabSignoff();

      return;
    }

    // NOTE **** begin group A ****
    export.Hidden.Assign(import.NextTranInfo);

    // NOTE **** end group A ****
    // ---------------------------------------------
    // If COMMAND is CLEAR, escape before moving imports to exports so that the 
    // screen is blanked out.
    // -------------------------------------------
    // 								
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }

    // If the key field is blank on a DISPLAY, send error message and exit from 
    // PRAD prior to populating export views so that the screen is cleared.
    if (Equal(global.Command, "DISPLAY") && IsEmpty
      (import.ProgramProcessingInfo.Name))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.ProgramProcessingInfo, "name");

      field.Error = true;

      return;
    }

    export.ProgramProcessingInfo.Assign(import.ProgramProcessingInfo);
    export.HiddenId.Name = import.HiddenId.Name;
    MoveDateWorkArea(import.DateTime, export.DateTime);

    // ---------------------------------------------
    //           *** Edit Screen ***
    // ---------------------------------------------
    // The logic assumes that a record cannot be UPDATEd or DELETEd without 
    // first being displayed.  Therefore, a key change with either command is
    // invalid.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !Equal(import.ProgramProcessingInfo.Name, import.HiddenId.Name))
    {
      // The above list of commands must be reviewed if
      // any new commands are made relevant to this
      //  procedure.
      ExitState = "ACO_NE0000_DISPLAY_BEFOR_UPD_DEL";

      var field = GetField(export.ProgramProcessingInfo, "name");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.  If key 
    // field is blank exit from PRAD prior to populating export views so that
    // the screen is cleared.
    if ((Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE")) && IsEmpty
      (import.ProgramProcessingInfo.Name))
    {
      // The above list of commands must be reviewed if
      // any new commands are made relevant to this
      //  procedure.
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.ProgramProcessingInfo, "name");

      field.Error = true;

      return;
    }

    // Validation common to DELETE and UPDATE.
    // If an error is found, EXIT STATE should
    //  be set.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      // ---------------------------------------------
      // Cannot update or delete an occurrence of Program Processing Info if any
      // runs have been used against it, i.e. no relationship exists between
      // Program Processing Info and Program Run.
      // ---------------------------------------------
      if (ReadProgramRun())
      {
        if (Equal(global.Command, "UPDATE"))
        {
          ExitState = "GB0000_PGM_PROC_INF_UPD_NOT_ALWD";
        }
        else
        {
          ExitState = "GB0000_PGM_PROC_INF_DEL_NOT_ALWD";
        }

        return;
      }
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      return;

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** end   group C ****
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "RETURN":
        break;
      case "MPCR":
        // PR307185 04/26/07 mwf Added following set statement.
        export.ProgramCheckpointRestart.ProgramName =
          import.ProgramProcessingInfo.Name;
        ExitState = "ECO_LNK_TO_MPCR";

        break;
      case "DISPLAY":
        if (!IsEmpty(export.HiddenId.Name))
        {
          if (!Equal(export.ProgramProcessingInfo.Name, export.HiddenId.Name))
          {
            export.ProgramProcessingInfo.CreatedTimestamp =
              local.Initialized.Timestamp;
          }
        }

        UseGbReadPgmProcessingInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the requested record.
          export.HiddenId.Name = export.ProgramProcessingInfo.Name;
          export.DateTime.Date =
            Date(export.ProgramProcessingInfo.CreatedTimestamp);
          export.DateTime.Time =
            TimeOfDay(export.ProgramProcessingInfo.CreatedTimestamp).
              GetValueOrDefault();
          ExitState = "ACO_NI0000_DISPLAY_OK";
        }
        else
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenId.Name = "";
        }

        break;
      case "ADD":
        if (ReadPgmNameTable())
        {
          // *** Continue
        }
        else
        {
          ExitState = "FN_ADD_PGM_NAME_TABLE_BEFORE_PPI";

          return;
        }

        UseGbCreatePgmProcessingInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenId.Name = export.ProgramProcessingInfo.Name;
          ExitState = "ACO_NI0000_CREATE_OK";
        }
        else
        {
        }

        break;
      case "UPDATE":
        UseUpdatePgmProcessingInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_3";
        }
        else
        {
        }

        break;
      case "DELETE":
        UseGbDeletePgmProcessingInfo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenId.Name = "";
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_3";
        }
        else
        {
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

          ExitState = "ZD_ACO_NE00_INVALID_SELECT_CODE1";
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

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseGbCreatePgmProcessingInfo()
  {
    var useImport = new GbCreatePgmProcessingInfo.Import();
    var useExport = new GbCreatePgmProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(import.ProgramProcessingInfo);

    Call(GbCreatePgmProcessingInfo.Execute, useImport, useExport);

    MoveProgramProcessingInfo1(useExport.ProgramProcessingInfo,
      export.ProgramProcessingInfo);
  }

  private void UseGbDeletePgmProcessingInfo()
  {
    var useImport = new GbDeletePgmProcessingInfo.Import();
    var useExport = new GbDeletePgmProcessingInfo.Export();

    MoveProgramProcessingInfo2(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(GbDeletePgmProcessingInfo.Execute, useImport, useExport);
  }

  private void UseGbReadPgmProcessingInfo()
  {
    var useImport = new GbReadPgmProcessingInfo.Import();
    var useExport = new GbReadPgmProcessingInfo.Export();

    MoveProgramProcessingInfo2(export.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);

    Call(GbReadPgmProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
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

  private void UseUpdatePgmProcessingInfo()
  {
    var useImport = new UpdatePgmProcessingInfo.Import();
    var useExport = new UpdatePgmProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Assign(import.ProgramProcessingInfo);

    Call(UpdatePgmProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadPgmNameTable()
  {
    entities.PgmNameTable.Populated = false;

    return Read("ReadPgmNameTable",
      (db, command) =>
      {
        db.SetString(command, "name", import.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.PgmNameTable.PgmName = db.GetString(reader, 0);
        entities.PgmNameTable.Populated = true;
      });
  }

  private bool ReadProgramRun()
  {
    entities.ProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
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
    /// A value of NextTranInfo.
    /// </summary>
    [JsonPropertyName("nextTranInfo")]
    public NextTranInfo NextTranInfo
    {
      get => nextTranInfo ??= new();
      set => nextTranInfo = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public ProgramProcessingInfo HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private NextTranInfo nextTranInfo;
    private DateWorkArea dateTime;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramProcessingInfo hiddenId;
    private Common prompt;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public ProgramProcessingInfo HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private DateWorkArea dateTime;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramProcessingInfo hiddenId;
    private Common prompt;
    private Standard standard;
    private NextTranInfo hidden;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private PgmNameTable pgmNameTable;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

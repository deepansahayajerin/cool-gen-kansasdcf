// Program: GB_MPCR_MTN_PGM_CHKPT_RESTART, ID: 371743945, model: 746.
// Short name: SWEMPCRP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GB_MPCR_MTN_PGM_CHKPT_RESTART.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class GbMpcrMtnPgmChkptRestart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_MPCR_MTN_PGM_CHKPT_RESTART program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbMpcrMtnPgmChkptRestart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbMpcrMtnPgmChkptRestart.
  /// </summary>
  public GbMpcrMtnPgmChkptRestart(IContext context, Import import, Export export)
    :
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
    // 	CREATE_PGM_CHKPNT_RESTART
    // 	UPDATE_PGM_CHKPNT_RESTART
    // 	DELETE_PGM_CHKPNT_RESTART
    // ENTITY TYPES USED:
    // 	PROGRAM_CHECKPOINT_RESTART C-R-U-D
    // MAINTENANCE LOG:
    // DATE       AUTHOR                DESCRIPTION
    // 07/01/96   Sherri Newman - DIR   Initial Code
    // 12/12/96   R. Marchman           Add new security/next tran
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    local.TestStatus.Flag = "Y";

    // Move all IMPORTs to EXPORTs.
    export.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);
    export.HiddenId.ProgramName = import.HiddenId.ProgramName;

    // The logic assumes that a record cannot be UPDATEd or DELETEd without 
    // first being displayed.  Therefore, a key change with either command is
    // invalid.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE")) &&
      !
      Equal(import.ProgramCheckpointRestart.ProgramName,
      import.HiddenId.ProgramName))
    {
      ExitState = "ACO_NE0000_DISPLAY_BEFOR_UPD_DEL";

      var field = GetField(export.ProgramCheckpointRestart, "programName");

      field.Error = true;

      return;
    }

    // If the key field is blank, certain commands are not allowed.
    if ((Equal(global.Command, "UPDATE") || Equal(global.Command, "CREATE") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE")) && IsEmpty
      (import.ProgramCheckpointRestart.ProgramName))
    {
      ExitState = "KEY_FIELD_IS_BLANK";

      var field = GetField(export.ProgramCheckpointRestart, "programName");

      field.Error = true;

      return;
    }

    // Validation for UPDATE of restart_ind.  Ind must be a "y" or "n".  If an 
    // error is found, EXIT STATE will be set.
    if (Equal(global.Command, "UPDATE"))
    {
      if (AsChar(import.ProgramCheckpointRestart.RestartInd) != 'Y' && AsChar
        (import.ProgramCheckpointRestart.RestartInd) != 'N')
      {
        ExitState = "ZD_ACO_NE000_INVALID_SELECT_CODE";

        var field = GetField(export.ProgramCheckpointRestart, "restartInd");

        field.Error = true;

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
    if (!Equal(global.Command, "MPPI"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** end   group C ****
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "HELP":
        ExitState = "ACO_NI0000_HELP_NOT_AVAILABLE";

        break;
      case "MPPI":
        ExitState = "ECO_LNK_TO_MPPI";

        break;
      case "RETURN":
        break;
      case "DISPLAY":
        UseReadPgmChkpntRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenId.ProgramName =
            export.ProgramCheckpointRestart.ProgramName;
          ExitState = "ACO_NI0000_DISPLAY_OK";
        }
        else
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenId.ProgramName = "";
        }

        break;
      case "CREATE":
        if (IsEmpty(import.ProgramCheckpointRestart.ProgramName))
        {
          ExitState = "KEY_FIELD_IS_BLANK";

          var field = GetField(export.ProgramCheckpointRestart, "programName");

          field.Error = true;

          return;
        }

        if (import.ProgramCheckpointRestart.ReadFrequencyCount.
          GetValueOrDefault() == 0)
        {
          export.ProgramCheckpointRestart.ReadFrequencyCount = 9999999;
        }

        if (import.ProgramCheckpointRestart.UpdateFrequencyCount.
          GetValueOrDefault() == 0)
        {
          export.ProgramCheckpointRestart.UpdateFrequencyCount = 9999999;
        }

        UseCreatePgmChkpntRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to that of the new record.
          export.HiddenId.ProgramName =
            export.ProgramCheckpointRestart.ProgramName;
          ExitState = "ACO_NI0000_CREATE_OK";
        }
        else
        {
        }

        break;
      case "UPDATE":
        UseUpdatePgmChkpntRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_UPD_3";
        }
        else
        {
        }

        break;
      case "DELETE":
        UseDeletePgmChkpntRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // Set the hidden key field to spaces or zero.
          export.HiddenId.ProgramName = "";
          ExitState = "ZD_ACO_NI0000_SUCCESSFUL_DEL_3";
        }
        else
        {
        }

        break;
      case "LIST":
        if (AsChar(import.PromptToList.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST1";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
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

  private void UseCreatePgmChkpntRestart()
  {
    var useImport = new CreatePgmChkpntRestart.Import();
    var useExport = new CreatePgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(export.ProgramCheckpointRestart);

    Call(CreatePgmChkpntRestart.Execute, useImport, useExport);
  }

  private void UseDeletePgmChkpntRestart()
  {
    var useImport = new DeletePgmChkpntRestart.Import();
    var useExport = new DeletePgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    Call(DeletePgmChkpntRestart.Execute, useImport, useExport);
  }

  private void UseReadPgmChkpntRestart()
  {
    var useImport = new ReadPgmChkpntRestart.Import();
    var useExport = new ReadPgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmChkpntRestart.Execute, useImport, useExport);

    export.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
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

  private void UseUpdatePgmChkpntRestart()
  {
    var useImport = new UpdatePgmChkpntRestart.Import();
    var useExport = new UpdatePgmChkpntRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(import.ProgramCheckpointRestart);

    Call(UpdatePgmChkpntRestart.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public ProgramCheckpointRestart HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of PromptToList.
    /// </summary>
    [JsonPropertyName("promptToList")]
    public Common PromptToList
    {
      get => promptToList ??= new();
      set => promptToList = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramCheckpointRestart hiddenId;
    private Common promptToList;
    private NextTranInfo hidden;
    private Standard standard;
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

    /// <summary>
    /// A value of HiddenId.
    /// </summary>
    [JsonPropertyName("hiddenId")]
    public ProgramCheckpointRestart HiddenId
    {
      get => hiddenId ??= new();
      set => hiddenId = value;
    }

    /// <summary>
    /// A value of PromptToList.
    /// </summary>
    [JsonPropertyName("promptToList")]
    public Common PromptToList
    {
      get => promptToList ??= new();
      set => promptToList = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramCheckpointRestart hiddenId;
    private Common promptToList;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TestStatus.
    /// </summary>
    [JsonPropertyName("testStatus")]
    public Common TestStatus
    {
      get => testStatus ??= new();
      set => testStatus = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    private Common testStatus;
    private Common temp;
  }
#endregion
}

// Program: LE_AACT_ADMINISTRATIVE_ACTION, ID: 372614670, model: 746.
// Short name: SWEAACTP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_AACT_ADMINISTRATIVE_ACTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeAactAdministrativeAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_AACT_ADMINISTRATIVE_ACTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeAactAdministrativeAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeAactAdministrativeAction.
  /// </summary>
  public LeAactAdministrativeAction(IContext context, Import import,
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
    // ************************************************************************************************
    //   Date	   Developer    Request #       Description
    // 05-08-95   S. Benton			Initial development
    // 04-01-1996 S.MALIK			RETROFIT
    // 09/16/98   P. Sharp			Removed unused views.
    // 					Fixed view match between AACTand ADAA.
    // 					Removed duplicate escape at statement 173.
    // 					Remove statements that set fields to spaces when a delete was 
    // successful
    // 12/11/00   GVandy	PR109212 	Set fields to spaces after a successful 
    // delete.
    // ************************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      // *********************************************
      // Clear out all data fields.
      // *********************************************
      return;
    }

    // *********************************************
    // Move Imports to Exports
    // *********************************************
    export.AdministrativeAction.Assign(import.AdministrativeAction);
    export.ListAdminActions.PromptField = import.ListAdminActions.PromptField;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // *********************************************
    // Move Hidden Imports to Hidden Exports.
    // *********************************************
    export.HiddenAdministrativeAction.Assign(import.HiddenAdministrativeAction);
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveSecurity2(import.HiddenSecurity, export.HiddenSecurity);

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);
      global.Command = "";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      MoveNextTranInfo(export.HiddenNextTranInfo, local.NextTranInfo);
      UseScCabNextTranPut();

      return;
    }

    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // *********************************************
    // Perform validations common to both CREATEs
    // and UPDATEs.
    // *********************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // *********************************************
      // Required fields  EDIT LOGIC
      // *********************************************
      if (IsEmpty(import.AdministrativeAction.Type1))
      {
        var field = GetField(export.AdministrativeAction, "type1");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(import.AdministrativeAction.Description))
      {
        var field = GetField(export.AdministrativeAction, "description");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }

      if (IsEmpty(import.AdministrativeAction.Indicator))
      {
        var field = GetField(export.AdministrativeAction, "indicator");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
      }
      else
      {
        switch(AsChar(import.AdministrativeAction.Indicator))
        {
          case 'A':
            break;
          case 'M':
            break;
          default:
            var field = GetField(export.AdministrativeAction, "indicator");

            field.Error = true;

            ExitState = "AUTOMATIC_MANUAL_IND_ERROR";

            break;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (!Equal(global.Command, "LIST"))
    {
      if (!IsEmpty(export.ListAdminActions.PromptField))
      {
        ExitState = "LE0000_PROMPT_ONLY_WITH_PF4";

        var field = GetField(export.ListAdminActions, "promptField");

        field.Error = true;

        return;
      }
    }

    // *********************************************
    //        P F K E Y   P R O C E S S I N G
    // *********************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // *********************************************
        // Required fields  EDIT LOGIC
        // *********************************************
        if (IsEmpty(import.AdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        UseCabReadAdminAction();

        if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "LE0000_ADMIN_ACTION_NF";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "EXIT":
        // ********************************************
        // Allows the user to flow back to the previous
        // screen.
        // ********************************************
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        if (!IsEmpty(export.ListAdminActions.PromptField) && AsChar
          (export.ListAdminActions.PromptField) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.ListAdminActions, "promptField");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.ListAdminActions.PromptField))
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          var field = GetField(export.ListAdminActions, "promptField");

          field.Error = true;

          return;
        }

        export.ListAdminActions.PromptField = "";
        ExitState = "ECO_LNK_TO_ADMIN_ACTION_AVAIL";

        break;
      case "ADD":
        // *********************************************
        // Insert the USE statement here to call the
        // CREATE action block.
        // *********************************************
        UseAddAdministrativeAction();

        if (IsExitState("ADMINISTRATIVE_ACTION_AE"))
        {
          ExitState = "LE0000_ADMIN_ACTION_AE";

          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        // *********************************************
        // Verify that a display has been performed
        // before the update can take place.
        // *********************************************
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // *********************************************
        // Check to see if any key fields have been
        // changed.
        // *********************************************
        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          return;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // UPDATE action block.
        // *********************************************
        UseUpdateAdministrativeAction();

        if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "LE0000_ADMIN_ACTION_NF";

          return;
        }
        else if (IsExitState("ADMINISTRATIVE_ACTION_NU"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "LE0000_ADMIN_ACTION_NU";

          return;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
        }

        break;
      case "RETURN":
        // **** begin group E ****
        ExitState = "ACO_NE0000_RETURN";

        // **** end   group E ****
        break;
      case "DELETE":
        // *********************************************
        // Verify that a display has been performed
        // before the delete can take place.
        // *********************************************
        if (IsEmpty(import.AdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        if (!Equal(import.AdministrativeAction.Type1,
          import.HiddenAdministrativeAction.Type1))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        // *********************************************
        // Insert the USE statement that checks to see
        // if the Administrative Action is associated to
        // any other entities.  If so, don't allow the
        // delete.
        // *********************************************
        UseLeReadAssocToAdminAction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          return;
        }

        // *********************************************
        // Insert the USE statement here to call the
        // DELETE action block.
        // *********************************************
        UseDeleteAdministrativeAction();

        if (IsExitState("ADMINISTRATIVE_ACTION_NF"))
        {
          ExitState = "LE0000_ADMIN_ACTION_NF";

          var field = GetField(export.AdministrativeAction, "type1");

          field.Error = true;

          return;
        }
        else
        {
          export.AdministrativeAction.Assign(local.Null1);
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      case "SIGNOFF":
        // ********************************************
        // Sign the user off the Kessep system
        // ********************************************
        // **** begin group F ****
        UseScCabSignoff();

        return;

        // **** end   group F ****
        break;
      case "CRIT":
        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        export.HiddenSecurity.LinkIndicator = "L";
        ExitState = "ECO_XFR_TO_MAIN_ADM_ACTION_CRIT";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
      default:
        break;
    }

    // *********************************************
    // If all processing completed successfully,
    // move all imports to previous exports .
    // *********************************************
    export.HiddenAdministrativeAction.Assign(export.AdministrativeAction);
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

  private static void MoveSecurity2(Security2 source, Security2 target)
  {
    target.LinkIndicator = source.LinkIndicator;
    target.Command = source.Command;
  }

  private void UseAddAdministrativeAction()
  {
    var useImport = new AddAdministrativeAction.Import();
    var useExport = new AddAdministrativeAction.Export();

    useImport.AdministrativeAction.Assign(import.AdministrativeAction);

    Call(AddAdministrativeAction.Execute, useImport, useExport);
  }

  private void UseCabReadAdminAction()
  {
    var useImport = new CabReadAdminAction.Import();
    var useExport = new CabReadAdminAction.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    Call(CabReadAdminAction.Execute, useImport, useExport);

    export.AdministrativeAction.Assign(useExport.AdministrativeAction);
  }

  private void UseDeleteAdministrativeAction()
  {
    var useImport = new DeleteAdministrativeAction.Import();
    var useExport = new DeleteAdministrativeAction.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    Call(DeleteAdministrativeAction.Execute, useImport, useExport);
  }

  private void UseLeReadAssocToAdminAction()
  {
    var useImport = new LeReadAssocToAdminAction.Import();
    var useExport = new LeReadAssocToAdminAction.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    Call(LeReadAssocToAdminAction.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

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

  private void UseUpdateAdministrativeAction()
  {
    var useImport = new UpdateAdministrativeAction.Import();
    var useExport = new UpdateAdministrativeAction.Export();

    useImport.AdministrativeAction.Assign(import.AdministrativeAction);

    Call(UpdateAdministrativeAction.Execute, useImport, useExport);
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
    /// A value of ListAdminActions.
    /// </summary>
    [JsonPropertyName("listAdminActions")]
    public Standard ListAdminActions
    {
      get => listAdminActions ??= new();
      set => listAdminActions = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    private Standard listAdminActions;
    private Standard standard;
    private AdministrativeAction administrativeAction;
    private AdministrativeAction hiddenAdministrativeAction;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ListAdminActions.
    /// </summary>
    [JsonPropertyName("listAdminActions")]
    public Standard ListAdminActions
    {
      get => listAdminActions ??= new();
      set => listAdminActions = value;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenAdministrativeAction.
    /// </summary>
    [JsonPropertyName("hiddenAdministrativeAction")]
    public AdministrativeAction HiddenAdministrativeAction
    {
      get => hiddenAdministrativeAction ??= new();
      set => hiddenAdministrativeAction = value;
    }

    /// <summary>
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of HiddenSecurity.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    public Security2 HiddenSecurity
    {
      get => hiddenSecurity ??= new();
      set => hiddenSecurity = value;
    }

    private Standard listAdminActions;
    private Standard standard;
    private AdministrativeAction administrativeAction;
    private AdministrativeAction hiddenAdministrativeAction;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public AdministrativeAction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    private AdministrativeAction null1;
    private NextTranInfo nextTranInfo;
  }
#endregion
}

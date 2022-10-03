// Program: LE_DELA_DELETE_LEGAL_ACTIONS, ID: 372113689, model: 746.
// Short name: SWEDELAP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_DELA_DELETE_LEGAL_ACTIONS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeDelaDeleteLegalActions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELA_DELETE_LEGAL_ACTIONS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDelaDeleteLegalActions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDelaDeleteLegalActions.
  /// </summary>
  public LeDelaDeleteLegalActions(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 06/15/95  Dave Allen			Initial Code
    // 04/24/96  Dale Brokaw     		Retro Fit
    // 12/02/98  R. Jean			Correct exit state messages issued; removed
    // 					F9 return; required diplay before delete;
    // 07/28/99  Anand Katuri			Modification to allow a DELETE on Legal Action
    // 					Petition without Court Case Number
    // 09/09/99  R. Jean	PR#73001	Include classes M, O, E, F that don't require
    // 					court case number.
    // 10/28/99  R. Jean	PR#H78286	Allow classifications U to be entered without
    // 					court case numbers like P, F, M, O, E are.
    // 04/12/00  DJean		WO# 160S	Set back new Establish Paternity Indicators
    // 04/03/02  K Cole	PR138221 	Changed to call new cab to get legal action
    // 					description.
    // 05/10/17  GVandy	CQ48108		IV-D PEP Changes.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.Foreign.Country = import.Foreign.Country;
    MoveFips(import.Fips, export.Fips);
    export.Tribunal.Assign(import.Tribunal);
    MoveLegalAction1(import.LegalAction, export.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    MoveOfficeServiceProvider(import.OfficeServiceProvider,
      export.OfficeServiceProvider);
    export.ServiceProvider.Assign(import.ServiceProvider);
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.PromptTribStateCounty.PromptField =
      import.PromptTribStateCounty.PromptField;

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
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      if (ReadLegalAction1())
      {
        MoveLegalAction2(entities.LegalAction, export.LegalAction);

        if (ReadTribunal2())
        {
          export.Tribunal.Assign(entities.Tribunal);

          if (ReadFips4())
          {
            export.Fips.Assign(entities.Fips);
          }
          else if (ReadFipsTribAddress3())
          {
            export.Foreign.Country = entities.FipsTribAddress.Country;
          }
          else
          {
            return;
          }
        }
        else
        {
          return;
        }
      }
      else
      {
        return;
      }

      global.Command = "REDISP";
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
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RLTRIB") || Equal
      (global.Command, "LIST") || Equal(global.Command, "LDET"))
    {
    }
    else if (Equal(global.Command, "ENTER") || Equal(global.Command, "INVALID"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "LIST") || Equal
      (global.Command, "REDISP"))
    {
      if (export.LegalAction.Identifier != 0)
      {
        UseLeGetPetitionerRespondent();
      }
    }

    if (Equal(global.Command, "REDISP"))
    {
      if (ReadFips1())
      {
        export.Fips.Assign(entities.Fips);
      }
      else if (ReadTribunal1())
      {
        export.Tribunal.Assign(entities.Tribunal);

        if (ReadFipsTribAddress2())
        {
          export.Foreign.Country = entities.FipsTribAddress.Country;
        }
      }
    }

    // ---------------------------------------------
    //               N E X T   T R A N
    // Use the CAB to nexttran to another procedure.
    // ---------------------------------------------
    // ---------------------------------------------
    //          S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "REDISP"))
    {
      if (!IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (export.Tribunal.Identifier != 0 && !IsEmpty(export.Foreign.Country))
        {
          goto Test;
        }

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          ExitState = "INVALID_STATE_ABBREVIATION";

          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          ExitState = "INVALID_COUNTY_ABBREVIATION";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          return;
        }
      }

Test:

      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          ExitState = "INVALID_COUNTY_ABBREVIATION";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.LegalAction.CourtCaseNumber) && AsChar
          (export.LegalAction.Classification) != 'P' && AsChar
          (export.LegalAction.Classification) != 'F' && AsChar
          (export.LegalAction.Classification) != 'M' && AsChar
          (export.LegalAction.Classification) != 'O' && AsChar
          (export.LegalAction.Classification) != 'U' && AsChar
          (export.LegalAction.Classification) != 'E')
        {
          ExitState = "LE0000_COURT_CASE_NO_RQD";

          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }
      }

      if (!IsEmpty(export.Fips.CountyAbbreviation))
      {
        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          ExitState = "INVALID_STATE_ABBREVIATION";

          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          return;
        }
      }

      if (Equal(global.Command, "DISPLAY"))
      {
        if (!IsEmpty(export.Fips.StateAbbreviation))
        {
          if (!ReadFips3())
          {
            ExitState = "INVALID_STATE_ABBREVIATION";

            var field = GetField(export.Fips, "stateAbbreviation");

            field.Error = true;

            return;
          }
        }

        if (!IsEmpty(export.Fips.CountyAbbreviation))
        {
          if (!ReadFips2())
          {
            ExitState = "INVALID_COUNTY_ABBREVIATION";

            var field = GetField(export.Fips, "countyAbbreviation");

            field.Error = true;

            return;
          }
        }
      }
    }

    // ---------------------------------------------
    // Validate Classification.
    // ---------------------------------------------
    if (!IsEmpty(export.LegalAction.Classification))
    {
      local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
      local.CodeValue.Cdvalue = export.LegalAction.Classification;
      UseCabValidateCodeValue();

      if (AsChar(local.ValidCode.Flag) != 'Y')
      {
        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }
      }
    }

    // ---------------------------------------------
    // Validate Prompt Classification.
    // ---------------------------------------------
    switch(AsChar(import.PromptClass.SelectChar))
    {
      case ' ':
        if (IsEmpty(import.PromptTribStateCounty.PromptField))
        {
          if (Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.PromptClass, "selectChar");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        break;
      case 'S':
        if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RLCVAL"))
        {
          var field1 = GetField(export.PromptClass, "selectChar");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
        }
        else if (AsChar(export.PromptTribStateCounty.PromptField) == 'S')
        {
          var field1 = GetField(export.PromptClass, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptTribStateCounty, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        break;
      default:
        var field = GetField(export.PromptClass, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
    }

    // ---------------------------------------------
    // Validate Prompt Tribunal State and County.
    // ---------------------------------------------
    switch(AsChar(import.PromptTribStateCounty.PromptField))
    {
      case ' ':
        if (IsEmpty(import.PromptClass.SelectChar))
        {
          if (Equal(global.Command, "LIST"))
          {
            var field1 = GetField(export.PromptTribStateCounty, "promptField");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
          }
        }

        break;
      case 'S':
        if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RLTRIB"))
        {
          var field1 = GetField(export.PromptTribStateCounty, "promptField");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
          }
        }

        break;
      default:
        var field = GetField(export.PromptTribStateCounty, "promptField");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "RLTRIB":
        if (AsChar(export.PromptTribStateCounty.PromptField) == 'S')
        {
          export.PromptTribStateCounty.PromptField = "";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Protected = false;
          field.Focused = true;

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            if (export.Tribunal.Identifier > 0)
            {
              if (ReadFipsTribAddress1())
              {
                export.Foreign.Country = entities.FipsTribAddress.Country;
              }
            }
          }

          return;
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "LIST":
        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.DisplayActiveCasesOnly.Flag = "Y";
          export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptTribStateCounty.PromptField) == 'S')
        {
          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            export.Fips.StateAbbreviation = "KS";
          }

          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }

        break;
      case "RLCVAL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (!IsEmpty(import.PromptClass.SelectChar))
        {
          export.PromptClass.SelectChar = "";

          var field = GetField(export.Fips, "stateAbbreviation");

          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;
          }
        }

        return;
      case "RLLGLACT":
        // ---------------------------------------------
        // Returned from List Legal Actions by Court
        // Case Number.
        // ---------------------------------------------
        if (import.DlgflwSelectedLegalAction.Identifier > 0)
        {
          MoveLegalAction2(import.DlgflwSelectedLegalAction, export.LegalAction);
            
          MoveLegalAction2(export.LegalAction, export.HiddenLegalAction);
          UseLeGetPetitionerRespondent();
        }

        global.Command = "REDISP";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ZD_ACO_NE0000_INVALID_FORWARD_1";

        return;
      case "DELETE":
        // ---------------------------------------------
        // Delete this Legal Action and associated data.
        // ---------------------------------------------
        if (export.LegalAction.Identifier == 0 || export
          .LegalAction.Identifier != export.HiddenLegalAction.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        // --05/10/17 GVandy  CQ48108  (IV-D PEP Changes)Do not update paternity
        // when deleting legal actions.
        UseDeleteLegalAction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }

        if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field1 = GetField(export.LegalAction, "courtCaseNumber");

          field1.Error = true;

          var field2 = GetField(export.LegalAction, "classification");

          field2.Error = true;
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.LegalAction.Assign(local.InitLegalAction);
          export.LegalAction.CourtCaseNumber =
            import.LegalAction.CourtCaseNumber ?? "";
          export.LegalAction.Classification = import.LegalAction.Classification;
          export.ActionTaken.Description =
            Spaces(CodeValue.Description_MaxLength);
          export.ServiceProvider.FirstName = "";
          export.ServiceProvider.LastName = "";
          export.ServiceProvider.MiddleInitial = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
        }

        return;
      case "LACT":
        // ---------------------------------------------
        // Transfer to "Legal Action" screen.
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_LEGAL_ACTION";

        return;
      case "LROL":
        // ---------------------------------------------
        // Transfer to "Legal Action Role" screen.
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_LEGAL_ROLE";

        return;
      case "LDET":
        // ---------------------------------------------
        // Transfer to "Legal Detail" screen.
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_LEGAL_DETAIL";

        return;
      case "CAPT":
        // --------------------------------------------
        // Link to "Caption" screen.
        // --------------------------------------------
        ExitState = "ECO_LNK_TO_COURT_CAPTION";

        return;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        return;
      default:
        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ---------------------------------------------
        // If the screen has already been displayed,
        // (identifier is present and equal to hidden
        // identifier) and the court case number and
        // classification haven't been changed, there is
        // no need to link to list screen to choose a
        // legal action. It is OK to display the screen.
        // ---------------------------------------------
        if (export.LegalAction.Identifier == export
          .HiddenLegalAction.Identifier && Equal
          (export.LegalAction.CourtCaseNumber,
          export.HiddenLegalAction.CourtCaseNumber) && AsChar
          (export.LegalAction.Classification) == AsChar
          (export.HiddenLegalAction.Classification) && export
          .LegalAction.Identifier > 0)
        {
          UseLeDisplayLegalAction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseLeGetPetitionerRespondent();
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            if (ReadFipsTribAddress1())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
          }
        }
        else
        {
          if (IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            ExitState = "ECO_LNK_LST_LEG_ACT_BY_CSE_PERSN";

            return;
          }

          // ---------------------------------------------
          // If the Court Case Number that was entered,
          // exists on more than one Legal Action, display
          // a list screen to select the desired one.
          // ---------------------------------------------
          local.ReadCounter.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            // --- US tribunal
            foreach(var item in ReadLegalAction2())
            {
              if (!IsEmpty(export.LegalAction.Classification) && AsChar
                (entities.LegalAction.Classification) != AsChar
                (export.LegalAction.Classification))
              {
                continue;
              }

              ++local.ReadCounter.Count;

              if (local.ReadCounter.Count > 1)
              {
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }
            }

            if (local.ReadCounter.Count == 0)
            {
              ExitState = "LEGAL_ACTION_NF";

              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Fips, "countyAbbreviation");

              field2.Error = true;

              var field3 = GetField(export.Fips, "stateAbbreviation");

              field3.Error = true;

              MoveLegalAction4(export.LegalAction, local.RetainLegalAction);
              local.RetainFips.Assign(export.Fips);
              export.LegalAction.Assign(local.InitLegalAction);
              MoveLegalAction4(local.RetainLegalAction, export.LegalAction);
              export.PetitionerRespondentDetails.Assign(
                local.InitPetitionerRespondentDetails);
              export.Fips.Assign(local.InitFips);
              MoveFips(local.RetainFips, export.Fips);
              export.Foreign.Country = local.InitFipsTribAddress.Country;
              export.Tribunal.Assign(local.InitTribunal);
              export.ActionTaken.Description =
                local.InitActionTaken.Description;
              export.ServiceProvider.Assign(local.InitServiceProvider);

              return;
            }
            else
            {
              MoveLegalAction2(entities.LegalAction, export.LegalAction);
            }
          }
          else
          {
            // --- Foreign tribunal
            if (export.Tribunal.Identifier == 0)
            {
              ExitState = "LE0000_TRIB_REQD_4_FOREIGN_TRIB";

              var field = GetField(export.PromptTribStateCounty, "promptField");

              field.Error = true;

              return;
            }
            else
            {
              foreach(var item in ReadLegalActionTribunal())
              {
                if (!IsEmpty(export.LegalAction.Classification) && AsChar
                  (entities.LegalAction.Classification) != AsChar
                  (export.LegalAction.Classification))
                {
                  continue;
                }

                local.ReadCounter.Count =
                  (int)((long)local.ReadCounter.Count + 1);

                if (local.ReadCounter.Count > 1)
                {
                  ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                  return;
                }
              }

              if (local.ReadCounter.Count == 0)
              {
                ExitState = "LEGAL_ACTION_NF";

                var field1 = GetField(export.LegalAction, "courtCaseNumber");

                field1.Error = true;

                var field2 = GetField(export.Foreign, "country");

                field2.Error = true;

                MoveLegalAction4(export.LegalAction, local.RetainLegalAction);
                local.RetainFips.Assign(export.Fips);
                export.LegalAction.Assign(local.InitLegalAction);
                MoveLegalAction4(local.RetainLegalAction, export.LegalAction);
                export.PetitionerRespondentDetails.Assign(
                  local.InitPetitionerRespondentDetails);
                export.Fips.Assign(local.InitFips);
                MoveFips(local.RetainFips, export.Fips);
                export.Foreign.Country = local.InitFipsTribAddress.Country;
                export.Tribunal.Assign(local.InitTribunal);
                export.ActionTaken.Description =
                  local.InitActionTaken.Description;
                export.ServiceProvider.Assign(local.InitServiceProvider);

                return;
              }
              else
              {
                MoveLegalAction2(entities.LegalAction, export.LegalAction);
              }
            }
          }

          UseLeDisplayLegalAction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseLeGetPetitionerRespondent();
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
          else if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            MoveLegalAction4(export.LegalAction, local.RetainLegalAction);
            local.RetainFips.Assign(export.Fips);
            export.LegalAction.Assign(local.InitLegalAction);
            MoveLegalAction4(local.RetainLegalAction, export.LegalAction);
            export.PetitionerRespondentDetails.Assign(
              local.InitPetitionerRespondentDetails);
            export.Fips.Assign(local.InitFips);
            MoveFips(local.RetainFips, export.Fips);
            export.Foreign.Country = local.InitFipsTribAddress.Country;
            export.Tribunal.Assign(local.InitTribunal);
            export.ActionTaken.Description = local.InitActionTaken.Description;
            export.ServiceProvider.Assign(local.InitServiceProvider);

            return;
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            if (ReadFipsTribAddress1())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
          }
        }

        break;
      case "REDISP":
        UseLeDisplayLegalAction();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseLeGetPetitionerRespondent();
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (ReadFipsTribAddress1())
          {
            export.Foreign.Country = entities.FipsTribAddress.Country;
          }
        }

        break;
      default:
        break;
    }

    // ---------------------------------------------
    // Use the code_value table to obtain the
    // description for the legal_action action_taken
    // ---------------------------------------------
    if (!IsEmpty(export.LegalAction.ActionTaken))
    {
      UseLeGetActionTakenDescription();
    }

    // ------------------------------------------------
    // If these dates were stored as max dates,
    // (12312099), convert them to zeros and don't
    // display them on the screen.
    // ------------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(export.LegalAction.EndDate, local.Max.Date))
    {
      export.LegalAction.EndDate = null;
    }

    if (Equal(export.LegalAction.LastModificationReviewDate, local.Max.Date))
    {
      export.LegalAction.LastModificationReviewDate = null;
    }

    // ------------------------------------------------
    // If all processing completed successfully, move
    // all exports to previous exports.
    // ------------------------------------------------
    MoveLegalAction2(export.LegalAction, export.HiddenLegalAction);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.CountyDescription = source.CountyDescription;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.LastModificationReviewDate = source.LastModificationReviewDate;
    target.AttorneyApproval = source.AttorneyApproval;
    target.ApprovalSentDate = source.ApprovalSentDate;
    target.PetitionerApproval = source.PetitionerApproval;
    target.ApprovalReceivedDate = source.ApprovalReceivedDate;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.EndDate = source.EndDate;
    target.ForeignOrderRegistrationDate = source.ForeignOrderRegistrationDate;
    target.UresaSentDate = source.UresaSentDate;
    target.UresaAcknowledgedDate = source.UresaAcknowledgedDate;
    target.UifsaSentDate = source.UifsaSentDate;
    target.UifsaAcknowledgedDate = source.UifsaAcknowledgedDate;
    target.InitiatingState = source.InitiatingState;
    target.InitiatingCounty = source.InitiatingCounty;
    target.RespondingState = source.RespondingState;
    target.RespondingCounty = source.RespondingCounty;
    target.OrderAuthority = source.OrderAuthority;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.LongArmStatuteIndicator = source.LongArmStatuteIndicator;
    target.PaymentLocation = source.PaymentLocation;
    target.EstablishmentCode = source.EstablishmentCode;
    target.DismissedWithoutPrejudiceInd = source.DismissedWithoutPrejudiceInd;
    target.DismissalCode = source.DismissalCode;
    target.RefileDate = source.RefileDate;
    target.NonCsePetitioner = source.NonCsePetitioner;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseDeleteLegalAction()
  {
    var useImport = new DeleteLegalAction.Import();
    var useExport = new DeleteLegalAction.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(DeleteLegalAction.Execute, useImport, useExport);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeDisplayLegalAction()
  {
    var useImport = new LeDisplayLegalAction.Import();
    var useExport = new LeDisplayLegalAction.Export();

    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(LeDisplayLegalAction.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Ap, export.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.Fips.Assign(useExport.Fips);
    export.Tribunal.Assign(useExport.Tribunal);
    export.LegalAction.Assign(useExport.LegalAction);
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
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

  private bool ReadFips1()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFips4()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips4",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress3()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunal",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 2);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 5);
        entities.Tribunal.Name = db.GetString(reader, 6);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of PromptTribStateCounty.
    /// </summary>
    [JsonPropertyName("promptTribStateCounty")]
    public Standard PromptTribStateCounty
    {
      get => promptTribStateCounty ??= new();
      set => promptTribStateCounty = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedCodeValue.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedCodeValue")]
    public CodeValue DlgflwSelectedCodeValue
    {
      get => dlgflwSelectedCodeValue ??= new();
      set => dlgflwSelectedCodeValue = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalAction")]
    public LegalAction DlgflwSelectedLegalAction
    {
      get => dlgflwSelectedLegalAction ??= new();
      set => dlgflwSelectedLegalAction = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    private CodeValue actionTaken;
    private Standard promptTribStateCounty;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private CodeValue dlgflwSelectedCodeValue;
    private LegalAction dlgflwSelectedLegalAction;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private FipsTribAddress foreign;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A HiddenSecurityGroup group.</summary>
    [Serializable]
    public class HiddenSecurityGroup
    {
      /// <summary>
      /// A value of HiddenSecurityCommand.
      /// </summary>
      [JsonPropertyName("hiddenSecurityCommand")]
      public Command HiddenSecurityCommand
      {
        get => hiddenSecurityCommand ??= new();
        set => hiddenSecurityCommand = value;
      }

      /// <summary>
      /// A value of HiddenSecurityProfileAuthorization.
      /// </summary>
      [JsonPropertyName("hiddenSecurityProfileAuthorization")]
      public ProfileAuthorization HiddenSecurityProfileAuthorization
      {
        get => hiddenSecurityProfileAuthorization ??= new();
        set => hiddenSecurityProfileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Command hiddenSecurityCommand;
      private ProfileAuthorization hiddenSecurityProfileAuthorization;
    }

    /// <summary>
    /// A value of ActionTaken.
    /// </summary>
    [JsonPropertyName("actionTaken")]
    public CodeValue ActionTaken
    {
      get => actionTaken ??= new();
      set => actionTaken = value;
    }

    /// <summary>
    /// A value of PromptTribStateCounty.
    /// </summary>
    [JsonPropertyName("promptTribStateCounty")]
    public Standard PromptTribStateCounty
    {
      get => promptTribStateCounty ??= new();
      set => promptTribStateCounty = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
    }

    /// <summary>
    /// A value of DisplayActiveCasesOnly.
    /// </summary>
    [JsonPropertyName("displayActiveCasesOnly")]
    public Common DisplayActiveCasesOnly
    {
      get => displayActiveCasesOnly ??= new();
      set => displayActiveCasesOnly = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of HiddenSecurity1.
    /// </summary>
    [JsonPropertyName("hiddenSecurity1")]
    public Security2 HiddenSecurity1
    {
      get => hiddenSecurity1 ??= new();
      set => hiddenSecurity1 = value;
    }

    /// <summary>
    /// Gets a value of HiddenSecurity.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenSecurityGroup> HiddenSecurity => hiddenSecurity ??= new(
      HiddenSecurityGroup.Capacity);

    /// <summary>
    /// Gets a value of HiddenSecurity for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenSecurity")]
    [Computed]
    public IList<HiddenSecurityGroup> HiddenSecurity_Json
    {
      get => hiddenSecurity;
      set => HiddenSecurity.Assign(value);
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of ListTribunal.
    /// </summary>
    [JsonPropertyName("listTribunal")]
    public Standard ListTribunal
    {
      get => listTribunal ??= new();
      set => listTribunal = value;
    }

    private CodeValue actionTaken;
    private Standard promptTribStateCounty;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Standard standard;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private Common displayActiveCasesOnly;
    private Code code;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private FipsTribAddress foreign;
    private Standard listTribunal;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of RetainLegalAction.
    /// </summary>
    [JsonPropertyName("retainLegalAction")]
    public LegalAction RetainLegalAction
    {
      get => retainLegalAction ??= new();
      set => retainLegalAction = value;
    }

    /// <summary>
    /// A value of RetainFips.
    /// </summary>
    [JsonPropertyName("retainFips")]
    public Fips RetainFips
    {
      get => retainFips ??= new();
      set => retainFips = value;
    }

    /// <summary>
    /// A value of InitServiceProvider.
    /// </summary>
    [JsonPropertyName("initServiceProvider")]
    public ServiceProvider InitServiceProvider
    {
      get => initServiceProvider ??= new();
      set => initServiceProvider = value;
    }

    /// <summary>
    /// A value of InitActionTaken.
    /// </summary>
    [JsonPropertyName("initActionTaken")]
    public CodeValue InitActionTaken
    {
      get => initActionTaken ??= new();
      set => initActionTaken = value;
    }

    /// <summary>
    /// A value of InitTribunal.
    /// </summary>
    [JsonPropertyName("initTribunal")]
    public Tribunal InitTribunal
    {
      get => initTribunal ??= new();
      set => initTribunal = value;
    }

    /// <summary>
    /// A value of InitFipsTribAddress.
    /// </summary>
    [JsonPropertyName("initFipsTribAddress")]
    public FipsTribAddress InitFipsTribAddress
    {
      get => initFipsTribAddress ??= new();
      set => initFipsTribAddress = value;
    }

    /// <summary>
    /// A value of InitFips.
    /// </summary>
    [JsonPropertyName("initFips")]
    public Fips InitFips
    {
      get => initFips ??= new();
      set => initFips = value;
    }

    /// <summary>
    /// A value of InitPetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("initPetitionerRespondentDetails")]
    public PetitionerRespondentDetails InitPetitionerRespondentDetails
    {
      get => initPetitionerRespondentDetails ??= new();
      set => initPetitionerRespondentDetails = value;
    }

    /// <summary>
    /// A value of InitLegalAction.
    /// </summary>
    [JsonPropertyName("initLegalAction")]
    public LegalAction InitLegalAction
    {
      get => initLegalAction ??= new();
      set => initLegalAction = value;
    }

    /// <summary>
    /// A value of ReadCounter.
    /// </summary>
    [JsonPropertyName("readCounter")]
    public Common ReadCounter
    {
      get => readCounter ??= new();
      set => readCounter = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of NoOfLegalActRecs.
    /// </summary>
    [JsonPropertyName("noOfLegalActRecs")]
    public Common NoOfLegalActRecs
    {
      get => noOfLegalActRecs ??= new();
      set => noOfLegalActRecs = value;
    }

    private LegalAction retainLegalAction;
    private Fips retainFips;
    private ServiceProvider initServiceProvider;
    private CodeValue initActionTaken;
    private Tribunal initTribunal;
    private FipsTribAddress initFipsTribAddress;
    private Fips initFips;
    private PetitionerRespondentDetails initPetitionerRespondentDetails;
    private LegalAction initLegalAction;
    private Common readCounter;
    private DateWorkArea max;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private LegalAction legalAction;
    private NextTranInfo nextTranInfo;
    private Common noOfLegalActRecs;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private FipsTribAddress fipsTribAddress;
  }
#endregion
}

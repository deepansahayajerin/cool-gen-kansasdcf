// Program: LE_SERV_SERVICE_INFORMATION, ID: 372015070, model: 746.
// Short name: SWESERVP
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
/// <para>
/// A program: LE_SERV_SERVICE_INFORMATION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeServServiceInformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_SERV_SERVICE_INFORMATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeServServiceInformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeServServiceInformation.
  /// </summary>
  public LeServServiceInformation(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	---------	
    // -------------------------------------------------------
    // 12/20/95  MaryRose M.			Add Petitioner/Respondent detail
    // 01/03/96  T.O.Redmond	   		Add Security and Next Tran
    // 08/26/97  JF.Caillouet	  	 	Removed Case #9 Error Msg
    // 11/18/98  R.Jean			Removed LEGAL ACTION read; set proper exist states;
    // 					flow to LACN if class changed; reorg logic for reading LEGAL ACTION,
    // 					TRIBUNAL, FIPS; eliminate unneeded USE LE_GET_PETITIONER_RESPONDENT
    // 03/13/02  GVandy	PR 139860	Send a notice to the AR for contempt hearings.
    // 03/13/02  GVandy	PR 139862	Send a notice to the AP and AR for paternity 
    // hearings.
    // 04/03/02  GVandy	PR 143212	Give a more meaningful message if document cab
    // cannot
    // 					find an assigned legal service provider.
    // 04/11/02  GVandy	PR 143440	Call new cab to retrieve end dated action 
    // taken descriptions.
    // 04/11/02  GVandy	PR 143549	Do not generate the ARNOTHR document if the AR
    // is an organization.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    local.HighlightError.Flag = "N";
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveFips(import.Fips, export.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.Next.Number = import.Next.Number;
    MoveLegalAction1(import.LegalAction, export.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.ServiceProcess.Assign(import.ServiceProcess);
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.PromptDocType.SelectChar = import.PromptDocType.SelectChar;
    export.PromptMethodOfService.SelectChar =
      import.PromptMethodOfService.SelectChar;
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenPrevLegalAction.Assign(import.HiddenPrevLegalAction);
    export.HiddenPrevServiceProcess.Assign(import.HiddenPrevServiceProcess);
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.CurrentDate.Date = Now().Date;

    // **************************************
    //   security / nextran logic
    // **************************************
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();
      export.HiddenNextTranInfo.Assign(local.NextTranInfo);

      // ---------------------------------------------
      // Populate export views from local next_tran_info view read from the data
      // base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      if (ReadLegalAction())
      {
        MoveLegalAction2(entities.LegalAction, export.LegalAction);
        MoveLegalAction2(entities.LegalAction, export.HiddenLegalAction);

        if (ReadTribunal())
        {
          export.Tribunal.Assign(entities.Tribunal);
          export.HiddenTribunal.Identifier = entities.Tribunal.Identifier;
        }
        else
        {
          return;
        }

        if (ReadFips1())
        {
          export.Fips.Assign(entities.Fips);
          export.HiddenFips.Assign(entities.Fips);
        }
        else if (ReadFipsTribAddress1())
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

      global.Command = "DISPLAY";
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ----------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // ----------------------------------------------------------
      local.NextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      local.NextTranInfo.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut1();

      return;
    }

    // ---------------------------------------------
    // Security and Nexttran code ends here
    // ---------------------------------------------
    if (Equal(global.Command, "RLLGLACT") || Equal
      (global.Command, "RETCDVL") || Equal(global.Command, "RLTRIB"))
    {
    }
    else if (Equal(global.Command, "ENTER"))
    {
      ExitState = "ACO_NE0000_INVALID_COMMAND";

      return;
    }
    else
    {
      // --- Validate action level security ---
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
        export.RequiredLegalAction.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber;
        export.HiddenPrevUserAction.Command = global.Command;
        ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CP";

        return;
      }
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL") && !
      Equal(global.Command, "RLTRIB"))
    {
      export.PromptClass.SelectChar = "";
      export.PromptDocType.SelectChar = "";
      export.PromptTribunal.SelectChar = "";
    }

    if (Equal(global.Command, "RLLGLACT"))
    {
      if (export.LegalAction.Identifier != 0)
      {
        global.Command = import.HiddenPrevUserAction.Command;
      }

      local.ReturnedFromListLgact.Flag = "Y";
    }
    else
    {
      local.ReturnedFromListLgact.Flag = "N";
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.LegalAction.Classification))
      {
        ExitState = "LE0000_LEG_ACT_CLASSIFN_REQD";

        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      if (IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
        (export.FipsTribAddress.Country))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        ExitState = "COUNTRY_OR_STATE_CODE_REQD";

        return;
      }

      if (!IsEmpty(entities.ExistingFips.StateAbbreviation) && !
        IsEmpty(export.FipsTribAddress.Country))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        ExitState = "EITHER_STATE_OR_CNTRY_CODE";

        return;
      }

      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        if (!ReadFips4())
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "INVALID_STATE_ABBREVIATION";

          return;
        }

        if (!ReadFips3())
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "INVALID_COUNTY_ABBREVIATION";

          return;
        }
      }
      else if (export.Tribunal.Identifier == 0)
      {
        ExitState = "INVALID_TRIBUNAL";

        var field = GetField(export.PromptTribunal, "selectChar");

        field.Protected = false;
        field.Focused = true;

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      if (AsChar(local.ReturnedFromListLgact.Flag) == 'N')
      {
        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) && !
          IsEmpty(export.HiddenPrevLegalAction.CourtCaseNumber) || export
          .HiddenTribunal.Identifier != export.Tribunal.Identifier && export
          .HiddenTribunal.Identifier != 0)
        {
          // ---------------------------------------------------------
          // Court Case No or Classification has been changed. So force
          // a legal action court case no.
          // ---------------------------------------------------------
          export.LegalAction.Identifier = 0;
          export.ServiceProcess.ServiceDocumentType = "";
          export.ServiceProcess.ServiceRequestDate =
            local.InitialisedToSpaces.ServiceRequestDate;
          export.Fips.CountyAbbreviation = "";
          export.Fips.StateAbbreviation = "";
          export.Fips.CountyDescription = "";
          export.Tribunal.Identifier = 0;
          export.Tribunal.Name = "";
          export.Tribunal.JudicialDistrict = "";
          export.Tribunal.JudicialDivision = "";
        }
        else if (!IsEmpty(export.HiddenPrevLegalAction.Classification) && AsChar
          (export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification))
        {
          export.ServiceProcess.ServiceRequestDate =
            local.InitialisedToSpaces.ServiceRequestDate;
          local.ClassChanged.Flag = "Y";
        }

        if (export.LegalAction.Identifier == 0 || AsChar
          (local.ClassChanged.Flag) == 'Y')
        {
          if (!IsEmpty(export.LegalAction.Classification))
          {
            local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
            local.CodeValue.Cdvalue = export.LegalAction.Classification;
            UseCabValidateCodeValue();

            if (AsChar(local.ValidCode.Flag) == 'N')
            {
              var field = GetField(export.LegalAction, "classification");

              field.Error = true;

              ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";

              return;
            }
          }

          local.NoOfLegalActionsFound.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            // --- US tribunal
            foreach(var item in ReadLegalActionTribunalFips())
            {
              if (!IsEmpty(export.LegalAction.Classification) && AsChar
                (entities.ExistingLegalAction.Classification) != AsChar
                (export.LegalAction.Classification))
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                MoveLegalAction3(export.LegalAction,
                  export.HiddenPrevLegalAction);
                export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
                export.RequiredLegalAction.CourtCaseNumber =
                  export.LegalAction.CourtCaseNumber;
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                export.HiddenPrevUserAction.Command = global.Command;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction2(entities.ExistingLegalAction, export.LegalAction);
                
              export.Tribunal.Assign(entities.ExistingTribunal);
              MoveFips(entities.ExistingFips, export.Fips);
            }
          }
          else
          {
            // --- Foreign tribunal
            if (export.Tribunal.Identifier == 0)
            {
              ExitState = "LE0000_TRIB_REQD_4_FOREIGN_TRIB";

              var field = GetField(export.PromptTribunal, "selectChar");

              field.Protected = false;
              field.Focused = true;

              return;
            }

            foreach(var item in ReadLegalActionTribunal2())
            {
              if (!IsEmpty(export.LegalAction.Classification) && AsChar
                (entities.ExistingLegalAction.Classification) != AsChar
                (export.LegalAction.Classification))
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                MoveLegalAction3(export.LegalAction,
                  export.HiddenPrevLegalAction);
                export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
                export.RequiredLegalAction.CourtCaseNumber =
                  export.LegalAction.CourtCaseNumber;
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                export.HiddenPrevUserAction.Command = global.Command;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction2(entities.ExistingLegalAction, export.LegalAction);
                
              export.Tribunal.Assign(entities.ExistingTribunal);
            }
          }
        }
        else
        {
          // --------------
          // Add processing
          // --------------
          if (export.LegalAction.Identifier != 0)
          {
            if (ReadLegalActionTribunal1())
            {
              MoveLegalAction2(entities.ExistingLegalAction, export.LegalAction);
                
              export.Tribunal.Assign(entities.ExistingTribunal);
              ++local.NoOfLegalActionsFound.Count;

              if (!IsEmpty(export.Fips.StateAbbreviation))
              {
                if (ReadFips2())
                {
                  MoveFips(entities.ExistingFips, export.Fips);
                }
                else
                {
                  local.NoOfLegalActionsFound.Count = 0;
                }
              }
            }
            else
            {
              local.NoOfLegalActionsFound.Count = 0;
            }
          }
        }
      }
      else
      {
        // --------------------------------------
        // Return from Legal Action by Court Case
        // --------------------------------------
        if (export.LegalAction.Identifier != 0)
        {
          if (ReadLegalActionTribunal1())
          {
            MoveLegalAction2(entities.ExistingLegalAction, export.LegalAction);
            export.Tribunal.Assign(entities.ExistingTribunal);
            ++local.NoOfLegalActionsFound.Count;

            if (!IsEmpty(export.Fips.StateAbbreviation))
            {
              if (ReadFips2())
              {
                MoveFips(entities.ExistingFips, export.Fips);
              }
              else
              {
                local.NoOfLegalActionsFound.Count = 0;
              }
            }
          }
          else
          {
            local.NoOfLegalActionsFound.Count = 0;
          }
        }
      }

      if (local.NoOfLegalActionsFound.Count == 0)
      {
        ExitState = "LEGAL_ACTION_NF";

        if (Equal(global.Command, "DISPLAY"))
        {
          export.ServiceProcess.Assign(local.InitialisedToSpaces);
        }

        return;
      }
      else
      {
        UseLeGetPetitionerRespondent();

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          if (ReadFipsTribAddress2())
          {
            export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
          }
        }
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      local.UserAction.Command = global.Command;
      UseLeServDisplayServiceProcess();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.HiddenDisplayPerformed.Flag = "Y";
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
        export.HiddenTribunal.Identifier = export.Tribunal.Identifier;
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";

        if (IsExitState("LEGAL_ACTION_NF"))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          export.LegalAction.Identifier = 0;
          export.ServiceProcess.Assign(local.InitialisedToSpaces);
        }
        else if (IsExitState("SERVICE_PROCESS_NF"))
        {
          var field1 = GetField(export.ServiceProcess, "serviceDocumentType");

          field1.Error = true;

          var field2 = GetField(export.ServiceProcess, "serviceRequestDate");

          field2.Error = true;

          export.ServiceProcess.Assign(local.InitialisedToSpaces);
        }
        else
        {
          var field = GetField(export.ServiceProcess, "serviceRequestDate");

          field.Protected = false;
          field.Focused = true;
        }
      }
    }

    // ---------------------------------------------
    // Use the code_value table to obtain the
    // description for the legal_action action_taken
    // ---------------------------------------------
    if (!IsEmpty(export.LegalAction.ActionTaken))
    {
      UseLeGetActionTakenDescription();
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // ----------------------------------------
          // It came to this screen from HIST or MONA
          // ----------------------------------------
          switch(TrimEnd(export.HiddenNextTranInfo.LastTran ?? ""))
          {
            case "SRPT":
              export.Standard.NextTransaction = "HIST";

              break;
            case "SRPU":
              export.Standard.NextTransaction = "MONA";

              break;
            default:
              break;
          }

          export.HiddenNextTranInfo.CourtCaseNumber =
            export.LegalAction.CourtCaseNumber ?? "";
          export.HiddenNextTranInfo.LegalActionIdentifier =
            export.LegalAction.Identifier;
          UseScCabNextTranPut2();

          return;
        }

        // --- Otherwise it is a normal return to the linking procedure
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "RLLGLACT":
        break;
      case "DISPLAY":
        break;
      case "PREV":
        break;
      case "NEXT":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

          return;
        }

        if (AsChar(export.PromptClass.SelectChar) != 'S' && !
          IsEmpty(export.PromptClass.SelectChar))
        {
          var field = GetField(export.PromptClass, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (AsChar(export.PromptMethodOfService.SelectChar) != 'S' && !
          IsEmpty(export.PromptMethodOfService.SelectChar))
        {
          var field = GetField(export.PromptMethodOfService, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (AsChar(export.PromptDocType.SelectChar) != 'S' && !
          IsEmpty(export.PromptDocType.SelectChar))
        {
          var field = GetField(export.PromptDocType, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
        }

        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.RequiredCode.CodeName = "LEGAL ACTION CLASSIFICATION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.PromptMethodOfService.SelectChar) == 'S')
        {
          export.RequiredCode.CodeName = "METHOD OF SERVICE";
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        if (AsChar(export.PromptDocType.SelectChar) == 'S')
        {
          export.RequiredCode.CodeName = "SERVICE PROCESS DOCUMENT";
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          break;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RLTRIB":
        // ---------------------------------------------
        // Returned from List Tribunals screen. Move
        // values to export.
        // ---------------------------------------------
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          export.PromptTribunal.SelectChar = "";

          if (import.DlgflwSelectedTribunal.Identifier > 0)
          {
            var field = GetField(export.ServiceProcess, "serviceRequestDate");

            field.Protected = false;
            field.Focused = true;

            export.Fips.Assign(import.DlgflwSelectedFips);
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);
          }
        }

        return;
      case "RETCDVL":
        // --------------------------------------------
        // Returned from list code value screen. Move
        // values to export.
        // --------------------------------------------
        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.PromptClass.SelectChar = "";

          if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.LegalAction, "classification");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.ServiceProcess, "serviceRequestDate");

            field.Protected = false;
            field.Focused = false;
          }

          break;
        }

        if (AsChar(export.PromptMethodOfService.SelectChar) == 'S')
        {
          export.PromptMethodOfService.SelectChar = "";

          if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.ServiceProcess, "methodOfService");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.ServiceProcess.MethodOfService =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.ServiceProcess, "serviceDocumentType");

            field.Protected = false;
            field.Focused = true;
          }

          break;
        }

        if (AsChar(export.PromptDocType.SelectChar) == 'S')
        {
          export.PromptDocType.SelectChar = "";

          if (IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.ServiceProcess, "serviceDocumentType");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            export.ServiceProcess.ServiceDocumentType =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.ServiceProcess, "requestedServee");

            field.Protected = false;
            field.Focused = true;
          }
        }

        break;
      case "ADD":
        local.UserAction.Command = global.Command;
        export.HiddenDisplayPerformed.Flag = "N";
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        UseLeServValidateServiceProcess();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        UseIdentifyServiceProcess();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // -- 3/13/02 GVandy PR 139860 & 139862  Send a notice to the AR for 
        // contempt hearings and send a
        // notice to the AP and AR for paternity hearings.
        if (!Equal(export.ServiceProcess.ServiceDate,
          local.InitialisedToSpaces.ServiceDate))
        {
          // -- If there is a hearing scheduled more than 7 days in the future 
          // and the service has been completed then
          //       1. Notify the AR if the hearing is for a contempt legal 
          // action
          //       2. Notify both the AR and AP if the hearing is for paternity 
          // legal action
          // -- Send document for the following contempt and paternity legal 
          // actions only.
          switch(TrimEnd(export.LegalAction.ActionTaken))
          {
            case "CONTEMPT":
              // -- Motion for Contempt
              break;
            case "DEFJPATM":
              // -- Motion Default Judgement for Paternity
              break;
            case "GENETICM":
              // -- Motion to Order Genetic Tests
              break;
            case "CONTEMPJ":
              // -- Jounal Entry of Contempt
              break;
            case "CONTINUO":
              // -- Order of Continuance - Contempt
              break;
            case "GENETICO":
              // -- Order Regarding Genetic Tests
              break;
            case "DET1PATP":
              // -- Petition to Determine Paternity - 1 AP
              break;
            case "DET2PATP":
              // -- Petition to Determine Paternity - 2 or more APs
              break;
            case "PATCSONP":
              // -- Petition to Determine Paternity and Child Support
              break;
            case "PATMEDP":
              // -- Petition to Determine Paternity and Medical
              break;
            default:
              goto Test1;
          }

          local.CurrentDatePlus7Days.Date = Now().Date.AddDays(7);

          if (ReadHearing())
          {
            // -- Notify the non-organization AR(s) for contempt and paternity 
            // hearings..
            local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
            local.Document.Name = "ARNOTHR";
            local.Previous.Number = "";

            foreach(var item in ReadCaseCsePerson())
            {
              if (Equal(entities.CsePerson.Number, local.Previous.Number))
              {
                // -- Only generate one notice per AR.
                continue;
              }
              else
              {
                local.Previous.Number = entities.CsePerson.Number;
              }

              local.SpDocKey.KeyCase = entities.Case1.Number;
              UseSpCabDetermineInterstateDoc();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("OFFICE_NF"))
                {
                  ExitState = "LE0000_NO_LEGAL_SERVICE_PROVIDER";
                }

                UseEabRollbackCics();

                goto Test3;
              }
            }

            if (Equal(export.LegalAction.ActionTaken, "DEFJPATM") || Equal
              (export.LegalAction.ActionTaken, "GENETICM") || Equal
              (export.LegalAction.ActionTaken, "GENETICO") || Equal
              (export.LegalAction.ActionTaken, "DET1PATP") || Equal
              (export.LegalAction.ActionTaken, "DET2PATP") || Equal
              (export.LegalAction.ActionTaken, "PATCSONP") || Equal
              (export.LegalAction.ActionTaken, "PATMEDP"))
            {
              // -- Notify the AP for paternity hearings.
              local.SpDocKey.KeyCase = "";
              local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
              local.Document.Name = "APNOTHR";
              UseSpCreateDocumentInfrastruct();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("OFFICE_NF"))
                {
                  ExitState = "LE0000_NO_LEGAL_SERVICE_PROVIDER";
                }

                UseEabRollbackCics();

                break;
              }
            }
          }
        }

Test1:

        UseLeServRaiseInfrastrucEvents();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
        export.HiddenTribunal.Identifier = export.Tribunal.Identifier;

        break;
      case "DELETE":
        // ---------------------------------------------
        //  Make sure that Court Case Number and
        //  Classification haven't changed before a
        //  delete.
        // ---------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "PREV") && !
          Equal(export.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (AsChar(export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (export.LegalAction.Identifier != export
          .HiddenPrevLegalAction.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (export.ServiceProcess.Identifier != export
          .HiddenPrevServiceProcess.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        export.HiddenDisplayPerformed.Flag = "N";
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        ExitState = "ACO_NN0000_ALL_OK";
        UseLeServDeleteServiceProcess();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        export.ServiceProcess.Assign(local.InitialisedToSpaces);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
        export.HiddenTribunal.Identifier = export.Tribunal.Identifier;

        break;
      case "UPDATE":
        // ---------------------------------------------
        //  Make sure that Court Case Number and
        //  Classification haven't changed before a
        //  update.
        // ---------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "PREV") && !
          Equal(export.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (AsChar(export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (export.LegalAction.Identifier != export
          .HiddenPrevLegalAction.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (export.ServiceProcess.Identifier != export
          .HiddenPrevServiceProcess.Identifier)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        UseLeServValidateServiceProcess();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        ExitState = "ACO_NN0000_ALL_OK";
        UseRecordServiceAttemptResult();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // -- 3/13/02 GVandy PR 139860 & 139862  Send a notice to the AR for 
        // contempt hearings and send a
        // notice to the AP and AR for paternity hearings.
        if (!Equal(export.ServiceProcess.ServiceDate,
          local.InitialisedToSpaces.ServiceDate) && !
          Equal(export.ServiceProcess.ServiceDate,
          export.HiddenPrevServiceProcess.ServiceDate))
        {
          // -- If there is a hearing scheduled more than 7 days in the future 
          // and the service has been completed then
          //       1. Notify the AR if the hearing is for a contempt legal 
          // action
          //       2. Notify both the AR and AP if the hearing is for paternity 
          // legal action
          // -- Send document for the following contempt and paternity legal 
          // actions only.
          switch(TrimEnd(export.LegalAction.ActionTaken))
          {
            case "CONTEMPT":
              // -- Motion for Contempt
              break;
            case "DEFJPATM":
              // -- Motion Default Judgement for Paternity
              break;
            case "GENETICM":
              // -- Motion to Order Genetic Tests
              break;
            case "CONTEMPJ":
              // -- Jounal Entry of Contempt
              break;
            case "CONTINUO":
              // -- Order of Continuance - Contempt
              break;
            case "GENETICO":
              // -- Order Regarding Genetic Tests
              break;
            case "DET1PATP":
              // -- Petition to Determine Paternity - 1 AP
              break;
            case "DET2PATP":
              // -- Petition to Determine Paternity - 2 or more APs
              break;
            case "PATCSONP":
              // -- Petition to Determine Paternity and Child Support
              break;
            case "PATMEDP":
              // -- Petition to Determine Paternity and Medical
              break;
            default:
              goto Test2;
          }

          local.CurrentDatePlus7Days.Date = Now().Date.AddDays(7);

          if (ReadHearing())
          {
            // -- Notify the non-organization AR(s) for contempt and paternity 
            // hearings..
            local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
            local.Document.Name = "ARNOTHR";
            local.Previous.Number = "";

            foreach(var item in ReadCaseCsePerson())
            {
              if (Equal(entities.CsePerson.Number, local.Previous.Number))
              {
                // -- Only generate one notice per AR.
                continue;
              }
              else
              {
                local.Previous.Number = entities.CsePerson.Number;
              }

              local.SpDocKey.KeyCase = entities.Case1.Number;
              UseSpCabDetermineInterstateDoc();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("OFFICE_NF"))
                {
                  ExitState = "LE0000_NO_LEGAL_SERVICE_PROVIDER";
                }

                UseEabRollbackCics();

                goto Test3;
              }
            }

            if (Equal(export.LegalAction.ActionTaken, "DEFJPATM") || Equal
              (export.LegalAction.ActionTaken, "GENETICM") || Equal
              (export.LegalAction.ActionTaken, "GENETICO") || Equal
              (export.LegalAction.ActionTaken, "DET1PATP") || Equal
              (export.LegalAction.ActionTaken, "DET2PATP") || Equal
              (export.LegalAction.ActionTaken, "PATCSONP") || Equal
              (export.LegalAction.ActionTaken, "PATMEDP"))
            {
              // -- Notify the AP for paternity hearings.
              local.SpDocKey.KeyCase = "";
              local.SpDocKey.KeyLegalAction = export.LegalAction.Identifier;
              local.Document.Name = "APNOTHR";
              UseSpCreateDocumentInfrastruct();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("OFFICE_NF"))
                {
                  ExitState = "LE0000_NO_LEGAL_SERVICE_PROVIDER";
                }

                UseEabRollbackCics();

                break;
              }
            }
          }
        }

Test2:

        UseLeServRaiseInfrastrucEvents();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        MoveLegalAction3(export.LegalAction, export.HiddenPrevLegalAction);
        export.HiddenPrevServiceProcess.Assign(export.ServiceProcess);
        export.HiddenTribunal.Identifier = export.Tribunal.Identifier;

        break;
      case "SIGNOFF":
        // --------------------------------------------
        // Sign the user off the Kessep system
        // --------------------------------------------
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

Test3:

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL"))
    {
      export.HiddenPrevUserAction.Command = global.Command;
    }

    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        switch(local.ErrorCodes.Item.DetailErrorCodes.Count)
        {
          case 1:
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            break;
          case 2:
            var field2 = GetField(export.ServiceProcess, "serviceDocumentType");

            field2.Error = true;

            var field3 = GetField(export.ServiceProcess, "serviceRequestDate");

            field3.Error = true;

            ExitState = "SERVICE_PROCESS_AE";

            break;
          case 3:
            var field4 = GetField(export.ServiceProcess, "serviceDocumentType");

            field4.Error = true;

            var field5 = GetField(export.ServiceProcess, "serviceRequestDate");

            field5.Error = true;

            ExitState = "SERVICE_PROCESS_NF";

            break;
          case 4:
            var field6 = GetField(export.ServiceProcess, "serviceRequestDate");

            field6.Error = true;

            ExitState = "LE0000_SERVICE_REQ_DATE_REQD";

            break;
          case 5:
            var field7 = GetField(export.ServiceProcess, "serviceDocumentType");

            field7.Error = true;

            if (IsEmpty(export.ServiceProcess.ServiceDocumentType))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_CODE";
            }

            break;
          case 6:
            var field8 = GetField(export.ServiceProcess, "requestedServee");

            field8.Error = true;

            ExitState = "LE0000_REQUESTED_SERVEE_REQD";

            break;
          case 7:
            var field9 = GetField(export.ServiceProcess, "serviceLocation");

            field9.Error = true;

            ExitState = "LE0000_SERVICE_LOCATION_REQD";

            break;
          case 8:
            var field10 = GetField(export.ServiceProcess, "serviceDate");

            field10.Error = true;

            ExitState = "LE0000_SERVICE_DATE_REQD";

            break;
          case 9:
            // ***  Removed  ***
            // Make export service_process server_name ERROR
            // EXIT STATE IS le000_server_name_reqd
            // ***
            break;
          case 10:
            var field11 = GetField(export.ServiceProcess, "servee");

            field11.Error = true;

            ExitState = "LE0000_PROCESS_SERVEE_REQD";

            break;
          case 11:
            var field12 = GetField(export.ServiceProcess, "serviceRequestDate");

            field12.Error = true;

            ExitState = "LE0000_INVALID_SERVICE_REQ_DT";

            break;
          case 12:
            var field13 = GetField(export.ServiceProcess, "serviceDate");

            field13.Error = true;

            ExitState = "LE0000_INVALID_SERVICE_DATE";

            break;
          case 13:
            if (Equal(export.ServiceProcess.ReturnDate, null))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              ExitState = "ACO_NE0000_DATE_RANGE_ERROR";
            }

            var field14 = GetField(export.ServiceProcess, "returnDate");

            field14.Error = true;

            break;
          case 14:
            var field15 = GetField(export.ServiceProcess, "methodOfService");

            field15.Error = true;

            if (IsEmpty(export.ServiceProcess.MethodOfService))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
            else
            {
              ExitState = "ACO_NE0000_INVALID_CODE";
            }

            break;
          default:
            ExitState = "ACO_NE0000_UNKNOWN_ERROR_CODE";

            break;
        }

        --local.ErrorCodes.Index;
        local.ErrorCodes.CheckSize();
      }
    }
  }

  private static void MoveErrorCodes(LeServValidateServiceProcess.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.DetailErrorCodes.Count = source.DetailErrorCode.Count;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.CountyDescription = source.CountyDescription;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveServiceProcess1(ServiceProcess source,
    ServiceProcess target)
  {
    target.Identifier = source.Identifier;
    target.ServiceDocumentType = source.ServiceDocumentType;
    target.ServiceRequestDate = source.ServiceRequestDate;
  }

  private static void MoveServiceProcess2(ServiceProcess source,
    ServiceProcess target)
  {
    target.ServiceDate = source.ServiceDate;
    target.ServiceRequestDate = source.ServiceRequestDate;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyLegalAction = source.KeyLegalAction;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseIdentifyServiceProcess()
  {
    var useImport = new IdentifyServiceProcess.Import();
    var useExport = new IdentifyServiceProcess.Export();

    MoveLegalAction3(export.LegalAction, useImport.LegalAction);
    useImport.ServiceProcess.Assign(export.ServiceProcess);

    Call(IdentifyServiceProcess.Execute, useImport, useExport);

    export.ServiceProcess.Assign(useExport.ServiceProcess);
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

  private void UseLeServDeleteServiceProcess()
  {
    var useImport = new LeServDeleteServiceProcess.Import();
    var useExport = new LeServDeleteServiceProcess.Export();

    MoveServiceProcess1(export.ServiceProcess, useImport.ServiceProcess);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeServDeleteServiceProcess.Execute, useImport, useExport);
  }

  private void UseLeServDisplayServiceProcess()
  {
    var useImport = new LeServDisplayServiceProcess.Import();
    var useExport = new LeServDisplayServiceProcess.Export();

    MoveServiceProcess1(export.ServiceProcess, useImport.ServiceProcess);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.UserAction.Command = local.UserAction.Command;

    Call(LeServDisplayServiceProcess.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.ServiceProcess.Assign(useExport.ServiceProcess);
  }

  private void UseLeServRaiseInfrastrucEvents()
  {
    var useImport = new LeServRaiseInfrastrucEvents.Import();
    var useExport = new LeServRaiseInfrastrucEvents.Export();

    MoveServiceProcess2(export.ServiceProcess, useImport.ServiceProcess);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeServRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseLeServValidateServiceProcess()
  {
    var useImport = new LeServValidateServiceProcess.Import();
    var useExport = new LeServValidateServiceProcess.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.ServiceProcess.Assign(export.ServiceProcess);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeServValidateServiceProcess.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseRecordServiceAttemptResult()
  {
    var useImport = new RecordServiceAttemptResult.Import();
    var useExport = new RecordServiceAttemptResult.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.ServiceProcess.Assign(export.ServiceProcess);

    Call(RecordServiceAttemptResult.Execute, useImport, useExport);

    export.ServiceProcess.Assign(useExport.ServiceProcess);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    local.NextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(local.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;

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

  private void UseSpCabDetermineInterstateDoc()
  {
    var useImport = new SpCabDetermineInterstateDoc.Import();
    var useExport = new SpCabDetermineInterstateDoc.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCabDetermineInterstateDoc.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCaseCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.CurrentDate.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Type1 = db.GetString(reader, 2);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private bool ReadFips1()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips1",
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

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips3()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips4()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips4",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFipsTribAddress1()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 11);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadFipsTribAddress2()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 11);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadHearing()
  {
    entities.Hearing.Populated = false;

    return Read("ReadHearing",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetDate(
          command, "hearingDt",
          local.CurrentDatePlus7Days.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Hearing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Hearing.LgaIdentifier = db.GetNullableInt32(reader, 1);
        entities.Hearing.ConductedDate = db.GetDate(reader, 2);
        entities.Hearing.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionTribunal1()
  {
    entities.ExistingTribunal.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalActionTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 5);
        entities.ExistingTribunal.Name = db.GetString(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal2()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.Tribunal.Populated = false;

    return ReadEach("ReadLegalActionTribunal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 5);
        entities.Tribunal.Name = db.GetString(reader, 6);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.ExistingLegalAction.Populated = true;
        entities.Tribunal.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return ReadEach("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 5);
        entities.Tribunal.Name = db.GetString(reader, 6);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.Fips.Location = db.GetInt32(reader, 7);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 9);
        entities.Fips.County = db.GetInt32(reader, 9);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 10);
        entities.Fips.State = db.GetInt32(reader, 10);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 11);
        entities.Fips.StateAbbreviation = db.GetString(reader, 12);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 13);
        entities.ExistingLegalAction.Populated = true;
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of PromptMethodOfService.
    /// </summary>
    [JsonPropertyName("promptMethodOfService")]
    public Common PromptMethodOfService
    {
      get => promptMethodOfService ??= new();
      set => promptMethodOfService = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of PromptDocType.
    /// </summary>
    [JsonPropertyName("promptDocType")]
    public Common PromptDocType
    {
      get => promptDocType ??= new();
      set => promptDocType = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevServiceProcess.
    /// </summary>
    [JsonPropertyName("hiddenPrevServiceProcess")]
    public ServiceProcess HiddenPrevServiceProcess
    {
      get => hiddenPrevServiceProcess ??= new();
      set => hiddenPrevServiceProcess = value;
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
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedFips.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedFips")]
    public Fips DlgflwSelectedFips
    {
      get => dlgflwSelectedFips ??= new();
      set => dlgflwSelectedFips = value;
    }

    /// <summary>
    /// A value of DlgflwSelectedTribunal.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedTribunal")]
    public Tribunal DlgflwSelectedTribunal
    {
      get => dlgflwSelectedTribunal ??= new();
      set => dlgflwSelectedTribunal = value;
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

    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common promptMethodOfService;
    private ScrollingAttributes scrollingAttributes;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private CodeValue dlgflwSelectedCodeValue;
    private Case1 next;
    private Common promptClass;
    private Common promptDocType;
    private LegalAction hiddenPrevLegalAction;
    private ServiceProcess hiddenPrevServiceProcess;
    private LegalAction legalAction;
    private ServiceProcess serviceProcess;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Fips fips;
    private Tribunal tribunal;
    private Common promptTribunal;
    private Tribunal hiddenTribunal;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
    private CodeValue actionTaken;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of PromptMethodOfService.
    /// </summary>
    [JsonPropertyName("promptMethodOfService")]
    public Common PromptMethodOfService
    {
      get => promptMethodOfService ??= new();
      set => promptMethodOfService = value;
    }

    /// <summary>
    /// A value of RequiredCode.
    /// </summary>
    [JsonPropertyName("requiredCode")]
    public Code RequiredCode
    {
      get => requiredCode ??= new();
      set => requiredCode = value;
    }

    /// <summary>
    /// A value of RequiredLegalAction.
    /// </summary>
    [JsonPropertyName("requiredLegalAction")]
    public LegalAction RequiredLegalAction
    {
      get => requiredLegalAction ??= new();
      set => requiredLegalAction = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
    }

    /// <summary>
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
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
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of PromptDocType.
    /// </summary>
    [JsonPropertyName("promptDocType")]
    public Common PromptDocType
    {
      get => promptDocType ??= new();
      set => promptDocType = value;
    }

    /// <summary>
    /// A value of HiddenPrevServiceProcess.
    /// </summary>
    [JsonPropertyName("hiddenPrevServiceProcess")]
    public ServiceProcess HiddenPrevServiceProcess
    {
      get => hiddenPrevServiceProcess ??= new();
      set => hiddenPrevServiceProcess = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common promptMethodOfService;
    private Code requiredCode;
    private LegalAction requiredLegalAction;
    private ScrollingAttributes scrollingAttributes;
    private Common hiddenDisplayPerformed;
    private Common hiddenPrevUserAction;
    private LegalAction hiddenPrevLegalAction;
    private LegalAction legalAction;
    private ServiceProcess serviceProcess;
    private Case1 next;
    private Common promptClass;
    private Common promptDocType;
    private ServiceProcess hiddenPrevServiceProcess;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Fips fips;
    private Tribunal tribunal;
    private Common promptTribunal;
    private Tribunal hiddenTribunal;
    private CodeValue actionTaken;
    private LegalAction hiddenLegalAction;
    private FipsTribAddress foreign;
    private Fips hiddenFips;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCodes.
      /// </summary>
      [JsonPropertyName("detailErrorCodes")]
      public Common DetailErrorCodes
      {
        get => detailErrorCodes ??= new();
        set => detailErrorCodes = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCodes;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of CurrentDatePlus7Days.
    /// </summary>
    [JsonPropertyName("currentDatePlus7Days")]
    public DateWorkArea CurrentDatePlus7Days
    {
      get => currentDatePlus7Days ??= new();
      set => currentDatePlus7Days = value;
    }

    /// <summary>
    /// A value of ClassChanged.
    /// </summary>
    [JsonPropertyName("classChanged")]
    public Common ClassChanged
    {
      get => classChanged ??= new();
      set => classChanged = value;
    }

    /// <summary>
    /// A value of ReturnedFromListLgact.
    /// </summary>
    [JsonPropertyName("returnedFromListLgact")]
    public Common ReturnedFromListLgact
    {
      get => returnedFromListLgact ??= new();
      set => returnedFromListLgact = value;
    }

    /// <summary>
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public ServiceProcess InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
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
    /// A value of TotalPromptsSelected.
    /// </summary>
    [JsonPropertyName("totalPromptsSelected")]
    public Common TotalPromptsSelected
    {
      get => totalPromptsSelected ??= new();
      set => totalPromptsSelected = value;
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

    private CsePerson previous;
    private DateWorkArea currentDate;
    private Document document;
    private SpDocKey spDocKey;
    private DateWorkArea currentDatePlus7Days;
    private Common classChanged;
    private Common returnedFromListLgact;
    private Common noOfLegalActionsFound;
    private Array<ErrorCodesGroup> errorCodes;
    private Common lastErrorEntryNo;
    private Common userAction;
    private Common highlightError;
    private ServiceProcess initialisedToSpaces;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private Common totalPromptsSelected;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
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

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private LegalActionCaseRole legalActionCaseRole;
    private Case1 case1;
    private Hearing hearing;
    private FipsTribAddress fipsTribAddress;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private LegalAction existingLegalAction;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}

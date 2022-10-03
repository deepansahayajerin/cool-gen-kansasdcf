// Program: LE_DISC_DISCOVERY_INFORMATION, ID: 372025517, model: 746.
// Short name: SWEDISCP
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
/// A program: LE_DISC_DISCOVERY_INFORMATION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeDiscDiscoveryInformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISC_DISCOVERY_INFORMATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDiscDiscoveryInformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDiscDiscoveryInformation.
  /// </summary>
  public LeDiscDiscoveryInformation(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date		Developer		Request #	Description
    // 09/20/95        govind		Initial coding
    // 12/20/95        maryrose m.	Add Petitioner/Respondent
    //                                 
    // detail
    // 01/03/96	T.O.Redmond	Add Security and Next Tran
    // 11/04/98        R. Jean         Change numerous exit states
    //  to conform to standards; modify logic for efficient reads;
    //  eliminated redundent reads
    // -------------------------------------------------------------------
    // *************************************************************
    // 06/21/07  G. Pan   PR308058  When automated flow from LACT
    //                    to this screen, after PF5 ADD pressed, return back
    //                    to LACT
    // *************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      if (AsChar(import.LegalActionFlow.Flag) == 'Y')
      {
        goto Test1;
      }

      return;
    }

Test1:

    MoveLegalAction2(import.LegalAction, export.LegalAction);

    // ---------------------------------------------
    // Move Imports to Exports
    // --------------------------------------------
    local.HighlightError.Flag = "N";
    MoveLegalAction2(import.LegalAction, export.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.Discovery.Assign(import.Discovery);
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    MoveScrollingAttributes(import.ScrollingAttributes,
      export.ScrollingAttributes);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveFips(import.Fips, export.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.LegalActionFlow.Flag = import.LegalActionFlow.Flag;

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    export.HiddenDisplayPerformed.Flag = import.HiddenDisplayPerformed.Flag;
    MoveLegalAction5(import.HiddenPrevLegalAction, export.HiddenPrevLegalAction);
      
    MoveDiscovery2(import.HiddenPrevDiscovery, export.HiddenPrevDiscovery);
    export.HiddenPrevUserAction.Command = import.HiddenPrevUserAction.Command;
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ---------------------------------------------
    // The following statements must be placed after
    //     MOVE imports to exports
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (AsChar(export.LegalActionFlow.Flag) == 'Y')
    {
      var field1 = GetField(export.LegalAction, "courtCaseNumber");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Fips, "stateAbbreviation");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.PromptClass, "selectChar");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.PromptTribunal, "selectChar");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Fips, "countyAbbreviation");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.LegalAction, "classification");

      field6.Color = "cyan";
      field6.Protected = true;
    }

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
      export.LegalAction.CourtCaseNumber =
        local.NextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        local.NextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      if (export.LegalAction.Identifier == 0)
      {
        return;
      }

      if (ReadLegalAction())
      {
        MoveLegalAction1(entities.LegalAction, export.LegalAction);

        if (ReadTribunal())
        {
          MoveTribunal(entities.Tribunal, export.Tribunal);

          if (ReadFips1())
          {
            export.Fips.Assign(entities.Fips);
          }
          else if (ReadFipsTribAddress1())
          {
            export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
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

      global.Command = "DISPLAY";
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
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** E D I T   C H E C K ********
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;
        }

        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          var field1 = GetField(export.Fips, "countyAbbreviation");

          field1.Error = true;
        }

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        return;
      }

      if (AsChar(export.LegalAction.Classification) != 'D')
      {
        ExitState = "LE0000_DISCVRY_CLASS_MUST_BE_D";

        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        return;
      }

      if (IsEmpty(export.Fips.StateAbbreviation))
      {
        if (export.Tribunal.Identifier == 0)
        {
          ExitState = "COUNTRY_OR_STATE_CODE_REQD";

          var field1 = GetField(export.FipsTribAddress, "country");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          return;
        }
      }
      else
      {
        if (!IsEmpty(export.FipsTribAddress.Country))
        {
          var field1 = GetField(export.FipsTribAddress, "country");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          ExitState = "EITHER_STATE_OR_CNTRY_CODE";

          return;
        }

        if (!ReadFips3())
        {
          ExitState = "INVALID_STATE_ABBREVIATION";

          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          return;
        }

        if (!ReadFips2())
        {
          ExitState = "INVALID_COUNTY_ABBREVIATION";

          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          return;
        }
      }
    }

    // ** Edit Check Ends ***
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCDVL") && !
      Equal(global.Command, "RLTRIB"))
    {
      export.PromptClass.SelectChar = "";
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        MoveLegalAction4(export.LegalAction, export.HiddenPrevLegalAction);
        export.RequiredLegalAction.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber;
        export.Discovery.RequestedDate =
          local.InitialisedToSpaces.RequestedDate;
        export.HiddenPrevUserAction.Command = global.Command;
        ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CP";

        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "RLLGLACT"))
    {
      if (export.LegalAction.Identifier != 0)
      {
        global.Command = import.HiddenPrevUserAction.Command;
      }
      else
      {
        goto Test2;
      }

      local.ReturnedFromListLgact.Flag = "Y";
    }
    else
    {
      local.ReturnedFromListLgact.Flag = "N";
    }

Test2:

    // ---------------------------------------------
    // For Commands ADD/DISPLAY/PREV/NEXT: check if legal_action is unique. 
    // Otherwise flow to list legal actions by crt case no.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY"))
    {
      // ---------------------------------------------
      // Dont check for multiple legal action records if it just returned from 
      // LACN or LAPS screen.
      // ---------------------------------------------
      if (AsChar(local.ReturnedFromListLgact.Flag) == 'N')
      {
        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) && !
          IsEmpty(export.HiddenPrevLegalAction.CourtCaseNumber) || !
          IsEmpty(export.HiddenPrevLegalAction.Classification) && AsChar
          (export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification) || export
          .HiddenTribunal.Identifier != export.Tribunal.Identifier && export
          .HiddenTribunal.Identifier != 0)
        {
          // ---------------------------------------------
          // Court Case No or classification has been changed. So force a new 
          // legal action court case no.
          // ---------------------------------------------
          export.LegalAction.Identifier = 0;
          export.Discovery.RequestedDate =
            local.InitialisedToSpaces.RequestedDate;
        }

        if (export.LegalAction.Identifier == 0)
        {
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
                MoveLegalAction4(export.LegalAction,
                  export.HiddenPrevLegalAction);
                MoveDiscovery2(export.Discovery, export.HiddenPrevDiscovery);
                export.RequiredLegalAction.CourtCaseNumber =
                  export.LegalAction.CourtCaseNumber;
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                export.HiddenPrevUserAction.Command = global.Command;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
                
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
                MoveLegalAction4(export.LegalAction,
                  export.HiddenPrevLegalAction);
                MoveDiscovery2(export.Discovery, export.HiddenPrevDiscovery);
                export.RequiredLegalAction.CourtCaseNumber =
                  export.LegalAction.CourtCaseNumber;
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                export.HiddenPrevUserAction.Command = global.Command;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
                
              export.Tribunal.Assign(entities.ExistingTribunal);

              if (ReadFipsTribAddress2())
              {
                export.FipsTribAddress.Country =
                  entities.FipsTribAddress.Country;
              }
            }
          }

          if (local.NoOfLegalActionsFound.Count == 0)
          {
            ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

            if (Equal(global.Command, "DISPLAY") || Equal
              (global.Command, "PREV") || Equal(global.Command, "NEXT"))
            {
              export.Discovery.Assign(local.InitialisedToSpaces);
            }

            return;
          }
        }
      }
      else if (ReadLegalActionTribunal1())
      {
        MoveLegalAction3(entities.ExistingLegalAction, export.LegalAction);
        export.Tribunal.Assign(entities.ExistingTribunal);

        if (ReadFips4())
        {
          MoveFips(entities.ExistingFips, export.Fips);
        }
        else if (ReadFipsTribAddress2())
        {
          export.FipsTribAddress.Country = entities.FipsTribAddress.Country;
        }
      }
      else
      {
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        if (Equal(global.Command, "DISPLAY") || Equal
          (global.Command, "PREV") || Equal(global.Command, "NEXT"))
        {
          export.Discovery.Assign(local.InitialisedToSpaces);
        }

        return;
      }

      if (export.LegalAction.Identifier != 0)
      {
        UseLeGetPetitionerRespondent();
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT"))
    {
      local.UserAction.Command = global.Command;
      UseLeDiscDisplayDiscovery();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        export.HiddenDisplayPerformed.Flag = "Y";

        // ********************************************
        // RCG   11-14-97
        // ******************************
      }
      else
      {
        export.HiddenDisplayPerformed.Flag = "N";

        if (IsExitState("DISCOVERY_NF"))
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          var field = GetField(export.Discovery, "requestedDate");

          field.Error = true;

          export.Discovery.Assign(local.InitialisedToSpaces);
        }
        else
        {
          if (AsChar(export.LegalActionFlow.Flag) == 'Y')
          {
            ExitState = "LE0000_NO_DISC_TO_DISPLAY_LACT";
          }
        }
      }
    }

    // ---------------------------------------------
    // Use the code_value table to obtain the
    // description for the legal_action action_taken
    // ---------------------------------------------
    if (!IsEmpty(export.LegalAction.ActionTaken))
    {
      local.Code.CodeName = "ACTION TAKEN";
      local.CodeValue.Cdvalue = export.LegalAction.ActionTaken;
      UseCabValidateCodeValue();
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        break;
      case "LIST":
        if (!IsEmpty(export.PromptTribunal.SelectChar))
        {
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

          return;
        }

        if (!IsEmpty(export.PromptClass.SelectChar) && AsChar
          (export.PromptClass.SelectChar) != 'S')
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field1 = GetField(export.PromptClass, "selectChar");

          field1.Error = true;

          break;
        }

        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.RequiredCode.CodeName = "LEGAL ACTION CLASSIFICATION";
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
        export.Fips.Assign(import.DlgflwSelectedFips);
        export.Tribunal.Assign(import.DlgflwSelectedTribunal);

        var field = GetField(export.Discovery, "requestedByCseInd");

        field.Protected = false;
        field.Focused = true;

        export.PromptTribunal.SelectChar = "";

        return;
      case "RETCDVL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // --------------------------------------------
        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.PromptClass.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field1 = GetField(export.Discovery, "lastName");

            field1.Protected = false;
            field1.Focused = false;

            var field2 = GetField(export.Fips, "stateAbbreviation");

            field2.Protected = false;
            field2.Focused = true;
          }
          else
          {
            var field1 = GetField(export.LegalAction, "classification");

            field1.Protected = false;
            field1.Focused = true;
          }
        }

        break;
      case "RLLGLACT":
        break;
      case "ADD":
        local.UserAction.Command = global.Command;
        export.HiddenDisplayPerformed.Flag = "N";
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        UseLeDiscValidateDiscovery();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        UseAddDiscovery();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        UseLeDiscRaiseInfrastrucEvents1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        // *************************************************************
        // 06/21/07  G. Pan   PR308058  When automated flow from LACT
        //                    to this screen, after PF5 ADD pressed, return back
        //                    to LACT
        // *************************************************************
        if (AsChar(export.LegalActionFlow.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_RETURN";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "DELETE":
        // ---------------------------------------------
        // Make sure that Court Case Number and Classification haven't changed 
        // before a delete.
        // ------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "PREV") && !
          Equal(export.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) || AsChar
          (export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification))
        {
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;
          }

          if (AsChar(export.LegalAction.Classification) != AsChar
            (export.HiddenPrevLegalAction.Classification))
          {
            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;
          }

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (!Equal(export.Discovery.RequestedDate,
          export.HiddenPrevDiscovery.RequestedDate))
        {
          var field1 = GetField(export.Discovery, "requestedDate");

          field1.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        export.ScrollingAttributes.MinusFlag = "";
        export.ScrollingAttributes.PlusFlag = "";
        UseLeDiscValidateDiscovery();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        // --------------------------------------------
        // Delete the current Discovery
        // --------------------------------------------
        export.HiddenDisplayPerformed.Flag = "N";
        UseLeDiscDeleteDiscovery();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        export.Discovery.Assign(local.InitialisedToSpaces);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "UPDATE":
        // ---------------------------------------------
        // Make sure that Court Case Number and Classification haven't changed 
        // before an update.
        // ---------------------------------------------
        if (!Equal(export.HiddenPrevUserAction.Command, "DISPLAY") && !
          Equal(export.HiddenPrevUserAction.Command, "PREV") && !
          Equal(export.HiddenPrevUserAction.Command, "NEXT") && !
          Equal(export.HiddenPrevUserAction.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        if (AsChar(export.HiddenDisplayPerformed.Flag) != 'Y')
        {
          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          break;
        }

        if (!Equal(export.LegalAction.CourtCaseNumber,
          export.HiddenPrevLegalAction.CourtCaseNumber) || AsChar
          (export.LegalAction.Classification) != AsChar
          (export.HiddenPrevLegalAction.Classification))
        {
          if (!Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenPrevLegalAction.CourtCaseNumber))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;
          }

          if (AsChar(export.LegalAction.Classification) != AsChar
            (export.HiddenPrevLegalAction.Classification))
          {
            var field1 = GetField(export.LegalAction, "classification");

            field1.Error = true;
          }

          export.HiddenDisplayPerformed.Flag = "N";
          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          break;
        }

        if (!Equal(export.Discovery.RequestedDate,
          export.HiddenPrevDiscovery.RequestedDate))
        {
          var field1 = GetField(export.Discovery, "requestedDate");

          field1.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
          export.HiddenDisplayPerformed.Flag = "N";

          break;
        }

        local.UserAction.Command = global.Command;
        UseLeDiscValidateDiscovery();

        if (local.LastErrorEntryNo.Count > 0)
        {
          local.HighlightError.Flag = "Y";

          break;
        }

        // --------------------------------------------
        // Update the current Discovery
        // --------------------------------------------
        UseUpdateDiscovery();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (AsChar(local.ResponseDateChanged.Flag) == 'Y')
        {
          UseLeDiscRaiseInfrastrucEvents2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            break;
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // --- It came to this screen from HIST or MONA
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
      case "PREV":
        break;
      case "NEXT":
        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ---------------------------------------------
    // If all processing completed successfully, move all imports to previous 
    // exports .
    // --------------------------------------------
    MoveLegalAction4(export.LegalAction, export.HiddenPrevLegalAction);
    MoveDiscovery2(export.Discovery, export.HiddenPrevDiscovery);
    export.HiddenPrevUserAction.Command = global.Command;

    if (AsChar(local.HighlightError.Flag) == 'Y')
    {
      local.ErrorCodes.Index = local.LastErrorEntryNo.Count - 1;
      local.ErrorCodes.CheckSize();

      while(local.ErrorCodes.Index >= 0)
      {
        switch(local.ErrorCodes.Item.DetailErrorCode.Count)
        {
          case 1:
            ExitState = "LEGAL_ACTION_NF";

            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            break;
          case 2:
            ExitState = "DISCOVERY_NF";

            var field2 = GetField(export.Discovery, "requestedDate");

            field2.Error = true;

            break;
          case 3:
            ExitState = "LE0000_RESP_REQUESTED_DATE_REQD";

            var field3 = GetField(export.Discovery, "requestedDate");

            field3.Error = true;

            break;
          case 4:
            ExitState = "LE0000_RESP_REQ_FIRST_NAME_REQD";

            var field4 = GetField(export.Discovery, "firstName");

            field4.Error = true;

            break;
          case 5:
            ExitState = "LE0000_RESP_REQ_LAST_NAME_REQD";

            var field5 = GetField(export.Discovery, "lastName");

            field5.Error = true;

            break;
          case 6:
            ExitState = "LE0000_INVALID_RESPONSE_DATE";

            var field6 = GetField(export.Discovery, "responseDate");

            field6.Error = true;

            break;
          case 7:
            ExitState = "LE0000_RESPONSE_DATE_REQD";

            var field7 = GetField(export.Discovery, "responseDate");

            field7.Error = true;

            break;
          case 8:
            ExitState = "LE0000_RESPONSE_DESC_REQD";

            var field8 = GetField(export.Discovery, "responseDescription");

            field8.Error = true;

            break;
          case 9:
            ExitState = "LE0000_INVALID_RESP_REQ_BY_CSE";

            var field9 = GetField(export.Discovery, "requestedByCseInd");

            field9.Error = true;

            break;
          case 10:
            ExitState = "LE0000_RESP_REQ_BY_LAST_NAME_REQ";

            var field10 = GetField(export.Discovery, "respReqByLastName");

            field10.Error = true;

            break;
          case 11:
            ExitState = "LE0000_RESP_REQ_BY_FRST_NAME_REQ";

            var field11 = GetField(export.Discovery, "respReqByFirstName");

            field11.Error = true;

            break;
          case 12:
            ExitState = "LE0000_DIS_REQ_DESC_REQD";

            var field12 = GetField(export.Discovery, "requestDescription");

            field12.Error = true;

            break;
          case 13:
            ExitState = "LE0000_DISC_INVALID_REQ_DATE";

            var field13 = GetField(export.Discovery, "requestedDate");

            field13.Error = true;

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

  private static void MoveDiscovery1(Discovery source, Discovery target)
  {
    target.RequestedDate = source.RequestedDate;
    target.ResponseDescription = source.ResponseDescription;
    target.ResponseDate = source.ResponseDate;
  }

  private static void MoveDiscovery2(Discovery source, Discovery target)
  {
    target.RequestedDate = source.RequestedDate;
    target.ResponseDate = source.ResponseDate;
  }

  private static void MoveDiscovery3(Discovery source, Discovery target)
  {
    target.RequestedDate = source.RequestedDate;
    target.ResponseDate = source.ResponseDate;
    target.RequestedByCseInd = source.RequestedByCseInd;
  }

  private static void MoveErrorCodes(LeDiscValidateDiscovery.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.DetailErrorCode.Count = source.DetailErrorCode.Count;
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
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
    target.PaymentLocation = source.PaymentLocation;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction5(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveScrollingAttributes(ScrollingAttributes source,
    ScrollingAttributes target)
  {
    target.PlusFlag = source.PlusFlag;
    target.MinusFlag = source.MinusFlag;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Identifier = source.Identifier;
    target.JudicialDistrict = source.JudicialDistrict;
    target.JudicialDivision = source.JudicialDivision;
  }

  private void UseAddDiscovery()
  {
    var useImport = new AddDiscovery.Import();
    var useExport = new AddDiscovery.Export();

    useImport.Discovery.Assign(export.Discovery);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(AddDiscovery.Execute, useImport, useExport);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    export.ActionTaken.Description = useExport.CodeValue.Description;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeDiscDeleteDiscovery()
  {
    var useImport = new LeDiscDeleteDiscovery.Import();
    var useExport = new LeDiscDeleteDiscovery.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveDiscovery1(export.Discovery, useImport.Discovery);

    Call(LeDiscDeleteDiscovery.Execute, useImport, useExport);
  }

  private void UseLeDiscDisplayDiscovery()
  {
    var useImport = new LeDiscDisplayDiscovery.Import();
    var useExport = new LeDiscDisplayDiscovery.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Discovery.RequestedDate = export.Discovery.RequestedDate;

    Call(LeDiscDisplayDiscovery.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.ScrollingAttributes,
      export.ScrollingAttributes);
    export.Discovery.Assign(useExport.Discovery);
  }

  private void UseLeDiscRaiseInfrastrucEvents1()
  {
    var useImport = new LeDiscRaiseInfrastrucEvents.Import();
    var useExport = new LeDiscRaiseInfrastrucEvents.Export();

    MoveDiscovery3(export.Discovery, useImport.Discovery);
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;

    Call(LeDiscRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseLeDiscRaiseInfrastrucEvents2()
  {
    var useImport = new LeDiscRaiseInfrastrucEvents.Import();
    var useExport = new LeDiscRaiseInfrastrucEvents.Export();

    MoveDiscovery3(export.Discovery, useImport.Discovery);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeDiscRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseLeDiscValidateDiscovery()
  {
    var useImport = new LeDiscValidateDiscovery.Import();
    var useExport = new LeDiscValidateDiscovery.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.Discovery.Assign(export.Discovery);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeDiscValidateDiscovery.Execute, useImport, useExport);

    local.LastErrorEntryNo.Count = useExport.LastErrorEntryNo.Count;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
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

  private void UseUpdateDiscovery()
  {
    var useImport = new UpdateDiscovery.Import();
    var useExport = new UpdateDiscovery.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Discovery.Assign(export.Discovery);

    Call(UpdateDiscovery.Execute, useImport, useExport);

    local.ResponseDateChanged.Flag = useExport.ResponseDateChanged.Flag;
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
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
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

  private bool ReadFips3()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips3",
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

  private bool ReadFips4()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips4",
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
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 1);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.Populated = true;
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
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 4);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionTribunal1()
  {
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingTribunal.Populated = false;

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
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingTribunal.Populated = true;
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
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 7);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 8);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 9);
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
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.Fips.Location = db.GetInt32(reader, 6);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 7);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 8);
        entities.Fips.County = db.GetInt32(reader, 8);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 9);
        entities.Fips.State = db.GetInt32(reader, 9);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 10);
        entities.Fips.StateAbbreviation = db.GetString(reader, 11);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 12);
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
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 2);
        entities.Tribunal.Identifier = db.GetInt32(reader, 3);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 4);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 5);
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
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
    /// A value of HiddenPrevDiscovery.
    /// </summary>
    [JsonPropertyName("hiddenPrevDiscovery")]
    public Discovery HiddenPrevDiscovery
    {
      get => hiddenPrevDiscovery ??= new();
      set => hiddenPrevDiscovery = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    /// <summary>
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common hiddenPrevUserAction;
    private Common hiddenDisplayPerformed;
    private ScrollingAttributes scrollingAttributes;
    private CodeValue dlgflwSelectedCodeValue;
    private Discovery hiddenPrevDiscovery;
    private Discovery discovery;
    private LegalAction hiddenPrevLegalAction;
    private LegalAction legalAction;
    private CodeValue codeValue;
    private Case1 next;
    private Common promptClass;
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
    private Common legalActionFlow;
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
    /// A value of RequiredLegalAction.
    /// </summary>
    [JsonPropertyName("requiredLegalAction")]
    public LegalAction RequiredLegalAction
    {
      get => requiredLegalAction ??= new();
      set => requiredLegalAction = value;
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
    /// A value of HiddenDisplayPerformed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayPerformed")]
    public Common HiddenDisplayPerformed
    {
      get => hiddenDisplayPerformed ??= new();
      set => hiddenDisplayPerformed = value;
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
    /// A value of HiddenPrevLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevLegalAction")]
    public LegalAction HiddenPrevLegalAction
    {
      get => hiddenPrevLegalAction ??= new();
      set => hiddenPrevLegalAction = value;
    }

    /// <summary>
    /// A value of HiddenPrevDiscovery.
    /// </summary>
    [JsonPropertyName("hiddenPrevDiscovery")]
    public Discovery HiddenPrevDiscovery
    {
      get => hiddenPrevDiscovery ??= new();
      set => hiddenPrevDiscovery = value;
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
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
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
    /// A value of PromptClass.
    /// </summary>
    [JsonPropertyName("promptClass")]
    public Common PromptClass
    {
      get => promptClass ??= new();
      set => promptClass = value;
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
    /// A value of LegalActionFlow.
    /// </summary>
    [JsonPropertyName("legalActionFlow")]
    public Common LegalActionFlow
    {
      get => legalActionFlow ??= new();
      set => legalActionFlow = value;
    }

    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private LegalAction requiredLegalAction;
    private Common hiddenPrevUserAction;
    private Common hiddenDisplayPerformed;
    private ScrollingAttributes scrollingAttributes;
    private LegalAction hiddenPrevLegalAction;
    private Discovery hiddenPrevDiscovery;
    private Code requiredCode;
    private Discovery discovery;
    private LegalAction legalAction;
    private Case1 next;
    private Common displayActiveCasesOnly;
    private Common promptClass;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Fips fips;
    private Tribunal tribunal;
    private Common promptTribunal;
    private Tribunal hiddenTribunal;
    private CodeValue actionTaken;
    private Common legalActionFlow;
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
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of ResponseDateChanged.
    /// </summary>
    [JsonPropertyName("responseDateChanged")]
    public Common ResponseDateChanged
    {
      get => responseDateChanged ??= new();
      set => responseDateChanged = value;
    }

    /// <summary>
    /// A value of InitialisedToSpaces.
    /// </summary>
    [JsonPropertyName("initialisedToSpaces")]
    public Discovery InitialisedToSpaces
    {
      get => initialisedToSpaces ??= new();
      set => initialisedToSpaces = value;
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
    /// A value of HighlightError.
    /// </summary>
    [JsonPropertyName("highlightError")]
    public Common HighlightError
    {
      get => highlightError ??= new();
      set => highlightError = value;
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
    /// A value of TotalPromptsSelected.
    /// </summary>
    [JsonPropertyName("totalPromptsSelected")]
    public Common TotalPromptsSelected
    {
      get => totalPromptsSelected ??= new();
      set => totalPromptsSelected = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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
    /// A value of ReturnedFromListLgact.
    /// </summary>
    [JsonPropertyName("returnedFromListLgact")]
    public Common ReturnedFromListLgact
    {
      get => returnedFromListLgact ??= new();
      set => returnedFromListLgact = value;
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

    private Common responseDateChanged;
    private Discovery initialisedToSpaces;
    private Array<ErrorCodesGroup> errorCodes;
    private Common lastErrorEntryNo;
    private Common highlightError;
    private Common userAction;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private LegalAction legalAction;
    private Common totalPromptsSelected;
    private Discovery discovery;
    private Common noOfLegalActionsFound;
    private Common returnedFromListLgact;
    private NextTranInfo nextTranInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
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
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
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

    private FipsTribAddress fipsTribAddress;
    private LegalAction existingLegalAction;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
  }
#endregion
}

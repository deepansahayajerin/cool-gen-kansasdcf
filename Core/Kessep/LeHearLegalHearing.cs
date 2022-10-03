// Program: LE_HEAR_LEGAL_HEARING, ID: 372011659, model: 746.
// Short name: SWEHEARP
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
/// A program: LE_HEAR_LEGAL_HEARING.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeHearLegalHearing: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_HEAR_LEGAL_HEARING program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeHearLegalHearing(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeHearLegalHearing.
  /// </summary>
  public LeHearLegalHearing(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/30/95	Dave Allen			Initial Code
    // 12/22/95        Add Petitioner/Respondent to screen
    // 05/01/97	govind		Prob Rept	Removed the edit for Mandatory Presiding 
    // Person Last Name and First Name.
    // 11/21/98	R.Jean		         	Correct bad exit states; correct view matching
    // ; reorg'd prompt edit logic; add edits; made reads more effcient by
    // removing scalar functions; changed test to edit against init views
    // instead of date/time(0) functions; eliminated reads where qualifiers
    // could be used
    // 04/21/99	PMcElderry
    // Added CSEnet logic for GIHER, PICHS.
    // 09/28/99	D Jean				PR73001: Allow classes P, F, M, O, E to be entered 
    // without Court Case Number
    // 10/11/99	PMcElderry
    // Added CSEnet logic for SICHS.
    // 12/23/99	PMcElderry
    // PR 83305 - validate against an abnormal date.
    // 11/27/00 GVandy
    // PR 106981 - Perform the DISPLAY logic on a nextran into HEAR.
    // 04/03/02 	K Cole								PR138221 - Use new cab 
    // le_get_action_taken_description to retrieve action taken descriptions
    // ------------------------------------------------------------
    // 09/28/2017    JHarden    CQ58574  Allow a not to be added to the HEAR 
    // screen.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction2(import.LegalAction, export.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.Fips.Assign(import.Fips);
    export.Foreign.Country = import.Foreign.Country;
    export.Hearing2.Assign(import.Hearing2);
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.Tribunal.Assign(import.Tribunal);
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    MoveScrollingAttributes(import.Hearing1, export.Hearing1);
    export.ListTribunal.PromptField = import.ListTribunal.PromptField;

    // CQ58574
    export.HiddenHearing.Note = import.HiddenHearing.Note;

    export.Addresses.Index = 0;
    export.Addresses.Clear();

    for(import.Addresses.Index = 0; import.Addresses.Index < import
      .Addresses.Count; ++import.Addresses.Index)
    {
      if (export.Addresses.IsFull)
      {
        break;
      }

      export.Addresses.Update.FipsTribAddress.Assign(
        import.Addresses.Item.FipsTribAddress);
      export.Addresses.Next();
    }

    if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue) && Equal
      (global.Command, "DISPLAY"))
    {
      export.LegalAction.Classification =
        import.DlgflwSelectedCodeValue.Cdvalue;
      export.PromptClass.SelectChar = "+";
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenFips.Assign(import.HiddenFips);
    export.HiddenTribunal.Identifier = import.HiddenTribunal.Identifier;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.Init1.Date = null;

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

      // ---------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // ----------------------------------------------------------
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
        export.HiddenLegalAction.Assign(entities.LegalAction);
        UseLeGetPetitionerRespondent();

        if (ReadTribunal())
        {
          MoveTribunal(entities.Tribunal, export.Tribunal);
          export.HiddenTribunal.Identifier = entities.Tribunal.Identifier;
        }
        else
        {
          return;
        }

        if (ReadFips())
        {
          export.Fips.Assign(entities.Fips);
          export.HiddenFips.Assign(entities.Fips);
        }
        else if (ReadFipsTribAddress2())
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

    if (Equal(global.Command, "RLCVAL") || Equal(global.Command, "RLLGLACT"))
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
    //     E D I T    C H E C K
    // ---------------------------------------------
    if (!Equal(global.Command, "EXIT") && !Equal(global.Command, "HELP") && !
      Equal(global.Command, "LIST") && !Equal(global.Command, "RETURN") && !
      Equal(global.Command, "SIGNOFF"))
    {
      if (IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
        (export.Fips.CountyAbbreviation))
      {
        var field1 = GetField(export.Fips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "countyAbbreviation");

        field2.Error = true;

        ExitState = "OE0014_MANDATORY_FIELD_MISSING";

        return;
      }

      // *****************************************************************
      // * PR's 73001, 78286  - 9/28/99: Allow classes P, F, M, O, E,
      // * U to be entered without Court Case Number.
      // *  D Jean
      // *****************************************************************
      if (IsEmpty(export.LegalAction.CourtCaseNumber) && AsChar
        (export.LegalAction.Classification) != 'P' && AsChar
        (export.LegalAction.Classification) != 'F' && AsChar
        (export.LegalAction.Classification) != 'M' && AsChar
        (export.LegalAction.Classification) != 'O' && AsChar
        (export.LegalAction.Classification) != 'E' && AsChar
        (export.LegalAction.Classification) != 'U' && !
        IsEmpty(export.LegalAction.Classification))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }

      if (IsEmpty(export.LegalAction.Classification))
      {
        if (Equal(global.Command, "DISPLAY") || Equal
          (global.Command, "RLLGLACT"))
        {
          // --- Classification is optional for Display.
        }
        else
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "LE0000_LEG_ACT_CLASSIFN_REQD";

          return;
        }
      }
      else
      {
        // ---------------------------------------------
        // Validate Classification.
        // ---------------------------------------------
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.LegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";

          return;
        }
      }
    }

    local.UserAction.Command = global.Command;

    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      // ---------------------------------------------
      // Validate Prompt Tribunal.
      // ---------------------------------------------
      if (!IsEmpty(export.ListTribunal.PromptField) && AsChar
        (export.ListTribunal.PromptField) != '+')
      {
        var field = GetField(export.ListTribunal, "promptField");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
      }

      // ---------------------------------------------
      // Validate Prompt Classification.
      // ---------------------------------------------
      if (!IsEmpty(export.PromptClass.SelectChar) && AsChar
        (export.PromptClass.SelectChar) != '+')
      {
        var field = GetField(export.PromptClass, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
      }

      if (!IsEmpty(export.Hearing2.LastName))
      {
        if (IsEmpty(export.Hearing2.FirstName))
        {
          var field = GetField(export.Hearing2, "firstName");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }

      if (!IsEmpty(export.Hearing2.FirstName))
      {
        if (IsEmpty(export.Hearing2.LastName))
        {
          var field = GetField(export.Hearing2, "lastName");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }

      // CQ58574 no error for not putting the hearing date, time & outcome 
      // received date (1)
      if (IsEmpty(export.Hearing2.Note) || !IsEmpty(export.HiddenHearing.Note))
      {
        if (Equal(export.Hearing2.ConductedDate, local.Init1.Date))
        {
          var field1 = GetField(export.Hearing2, "conductedDate");

          field1.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          var field2 = GetField(export.Hearing2, "outcomeReceivedDate");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }

        if (export.Hearing2.ConductedTime == local.Init1.Time)
        {
          var field = GetField(export.Hearing2, "conductedTime");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }

        if (!IsEmpty(export.Hearing2.Outcome))
        {
          if (Equal(export.Hearing2.OutcomeReceivedDate, local.Init1.Date))
          {
            var field = GetField(export.Hearing2, "outcomeReceivedDate");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }
        }

        if (Lt(local.Init1.Date, export.Hearing2.OutcomeReceivedDate))
        {
          if (IsEmpty(export.Hearing2.Outcome))
          {
            var field = GetField(export.Hearing2, "outcome");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
            }
          }
        }
      }

      // CQ58574 no error for not putting the hearing date, time & outcome 
      // received date
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "HRPV") || Equal(global.Command, "HRNX"))
    {
      local.UserAction.Command = global.Command;
      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // --------
      // PR 83305
      // --------
      if (Lt(new DateTime(2099, 12, 31), export.Hearing2.ConductedDate))
      {
        var field = GetField(export.Hearing2, "conductedDate");

        field.Error = true;

        ExitState = "LE0000_DATE_YEAR_LIMIT_EXCEEDED";
      }

      if (Lt(new DateTime(2099, 12, 31), export.Hearing2.OutcomeReceivedDate))
      {
        var field = GetField(export.Hearing2, "outcomeReceivedDate");

        field.Error = true;

        ExitState = "LE0000_DATE_YEAR_LIMIT_EXCEEDED";

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "LIST":
        if (AsChar(export.ListTribunal.PromptField) != 'S' && AsChar
          (export.PromptClass.SelectChar) != 'S')
        {
          var field1 = GetField(export.ListTribunal, "promptField");

          field1.Error = true;

          var field2 = GetField(export.PromptClass, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else if (AsChar(export.ListTribunal.PromptField) == 'S' && AsChar
          (export.PromptClass.SelectChar) == 'S')
        {
          var field1 = GetField(export.ListTribunal, "promptField");

          field1.Error = true;

          var field2 = GetField(export.PromptClass, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }
        else if (!IsEmpty(export.ListTribunal.PromptField) && AsChar
          (export.ListTribunal.PromptField) != '+' && AsChar
          (export.ListTribunal.PromptField) != 'S')
        {
          var field = GetField(export.ListTribunal, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else if (!IsEmpty(export.PromptClass.SelectChar) && AsChar
          (export.PromptClass.SelectChar) != '+' && AsChar
          (export.PromptClass.SelectChar) != 'S')
        {
          var field = GetField(export.PromptClass, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
        }
        else if (AsChar(export.ListTribunal.PromptField) == 'S')
        {
          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            export.Fips.StateAbbreviation = "KS";
          }

          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }
        else if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.DisplayActiveCasesOnly.Flag = "Y";

          if (!IsEmpty(export.PromptClass.SelectChar))
          {
            export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";
        }

        // ****************************************************************
        // * ESCAPE out here; either to flow for prompt or you have an error
        // ****************************************************************
        return;
      case "RLCVAL":
        // --------------------------------------------------------
        // Returned from List Code Values screen. Move values to
        // export.
        // --------------------------------------------------------
        if (AsChar(import.PromptClass.SelectChar) == 'S')
        {
          export.PromptClass.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            var field = GetField(export.LegalAction, "classification");

            field.Protected = false;
            field.Focused = true;

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
          MoveLegalAction3(import.DlgflwSelectedLegalAction, export.LegalAction);
            
          MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
          export.Fips.Assign(import.Fips);
          export.HiddenFips.Assign(import.Fips);
          export.Tribunal.Assign(import.Tribunal);

          if (IsEmpty(export.Fips.StateAbbreviation))
          {
            if (ReadFipsTribAddress1())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
          }

          UseLeGetPetitionerRespondent();
        }

        global.Command = "REDISP";

        break;
      case "ADD":
        if (export.LegalAction.Identifier == 0)
        {
          local.NoOfLegalActRecs.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            foreach(var item in ReadLegalActionTribunalFips())
            {
              if (!IsEmpty(export.LegalAction.Classification) && AsChar
                (entities.LegalAction.Classification) != AsChar
                (export.LegalAction.Classification))
              {
                continue;
              }

              ++local.NoOfLegalActRecs.Count;

              if (local.NoOfLegalActRecs.Count > 1)
              {
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              if (local.NoOfLegalActRecs.Count == 1)
              {
                MoveLegalAction2(entities.LegalAction, export.LegalAction);
                MoveTribunal(entities.Tribunal, export.Tribunal);
                export.Fips.Assign(entities.Fips);
                UseLeGetPetitionerRespondent();
              }
            }

            if (local.NoOfLegalActRecs.Count == 0)
            {
              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Fips, "stateAbbreviation");

              field2.Error = true;

              var field3 = GetField(export.Fips, "countyAbbreviation");

              field3.Error = true;

              return;
            }
          }
          else if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            var field = GetField(export.ListTribunal, "promptField");

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

              ++local.NoOfLegalActRecs.Count;

              if (local.NoOfLegalActRecs.Count > 1)
              {
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              if (local.NoOfLegalActRecs.Count == 1)
              {
                MoveLegalAction2(entities.LegalAction, export.LegalAction);
                MoveTribunal(entities.Tribunal, export.Tribunal);
                UseLeGetPetitionerRespondent();
              }
            }

            if (local.NoOfLegalActRecs.Count == 0)
            {
              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Foreign, "country");

              field2.Error = true;

              return;
            }
          }
        }

        // ---------------------------------------------
        // Add the current Legal Hearing.
        // ---------------------------------------------
        // ------------------------------------------------------------
        // All Legal Hearings (Judicial) are type "J".
        // All Administrative Hearings   are type "A".
        // ------------------------------------------------------------
        export.Hearing2.Type1 = "J";
        UseObtainHearingDate();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.LegalAction, "classification");

            field2.Error = true;
          }
          else if (IsExitState("HEARING_AE"))
          {
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.Hearing2, "conductedDate");

              field.Error = true;
            }
          }
          else if (IsExitState("CO0000_HEARING_AE_FOR_THIS_DATE"))
          {
            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.Hearing2, "conductedDate");

              field.Error = true;
            }
          }
          else
          {
          }

          return;
        }

        UseLeHearRaiseInfrastrucEvents();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // ---------------------------------------------------------
        // BEG of CSEnet functionality - for specific legal actions,
        // multiple csenet transaction codes are set - applicable
        // for responding interstate only
        // ---------------------------------------------------------
        if (Equal(export.LegalAction.ActionTaken, "DEFJPATM") || Equal
          (export.LegalAction.ActionTaken, "GENETICM"))
        {
          local.Specific.ActionReasonCode = "PICHS";
        }
        else if (Equal(export.LegalAction.ActionTaken, "CSMODM") || Equal
          (export.LegalAction.ActionTaken, "DELTSUPM"))
        {
          local.Specific.ActionReasonCode = "SICHS";
        }
        else
        {
          // --------------------
          // continue processing
          // --------------------
        }

        local.MiscellaneousInformation.Date = export.Hearing2.ConductedDate;
        local.ScreenIdentification.Command = "HEAR";
        UseSiCreateAutoCsenetTrans();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        // ---------------------------
        // END of CSEnet functionality
        // ---------------------------
        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          for(export.Addresses.Index = 0; export.Addresses.Index < export
            .Addresses.Count; ++export.Addresses.Index)
          {
            if (IsEmpty(export.Addresses.Item.FipsTribAddress.Country))
            {
              continue;
            }

            export.Foreign.Country =
              export.Addresses.Item.FipsTribAddress.Country ?? "";

            break;
          }
        }

        global.Command = "REDISP";

        break;
      case "UPDATE":
        // ---------------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before an update.
        // ---------------------------------------------------------
        if (IsEmpty(export.HiddenFips.StateAbbreviation))
        {
          if (!Equal(import.LegalAction.CourtCaseNumber,
            import.HiddenLegalAction.CourtCaseNumber) || import
            .Tribunal.Identifier != export.HiddenTribunal.Identifier)
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Foreign, "country");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Protected = true;
            field2.Focused = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            return;
          }
        }
        else if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber) || !
          Equal(import.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation) || !
          Equal(import.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // ---------------------------------------------
        // Verify that a display has been performed
        // before the update can take place.
        // ---------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }
        else
        {
          // ---------------------------------------------
          // Update the current Legal Hearing.
          // ---------------------------------------------
          UseReceiveLegalHearingResult();

          if (IsExitState("HEARING_NF"))
          {
            var field = GetField(export.Hearing2, "conductedDate");

            field.Error = true;
          }
          else if (IsExitState("HEARING_NU"))
          {
            var field = GetField(export.Hearing2, "conductedDate");

            field.Error = true;
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        if (AsChar(local.HearingDateChanged.Flag) == 'Y' || AsChar
          (local.OutcomeRcvdDateChanged.Flag) == 'Y')
        {
          UseLeHearRaiseInfrastrucEvents();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // ---------------------------------------------------------
        // BEG of CSEnet functionality - for specific legal actions,
        // multiple csenet transaction codes are set - applicable
        // for responding interstate only
        // ---------------------------------------------------------
        if (AsChar(local.HearingDateChanged.Flag) == 'Y')
        {
          if (Equal(export.LegalAction.ActionTaken, "DEFJPATM") || Equal
            (export.LegalAction.ActionTaken, "GENETICM"))
          {
            local.Specific.ActionReasonCode = "PICHS";
          }
          else if (Equal(export.LegalAction.ActionTaken, "CSMODM") || Equal
            (export.LegalAction.ActionTaken, "DELTSUPM"))
          {
            local.Specific.ActionReasonCode = "SICHS";
          }
          else
          {
            // --------------------
            // continue processing
            // --------------------
          }

          local.MiscellaneousInformation.Date = export.Hearing2.ConductedDate;
          local.ScreenIdentification.Command = "HEAR";
          UseSiCreateAutoCsenetTrans();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
          else
          {
            // ---------------------------
            // END of CSEnet functionality
            // ---------------------------
          }

          // CQ58574
          export.HiddenHearing.Note = export.Hearing2.Note;
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          // --------------------
          // continue processing
          // --------------------
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

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
      case "DELETE":
        // ---------------------------------------------
        // Verify that a display has been performed
        // before the delete can take place.
        // ---------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          return;
        }

        UseLeDeleteHearing();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();
        }

        if (IsExitState("HEARING_NF"))
        {
          var field = GetField(export.Hearing2, "conductedDate");

          field.Error = true;

          return;
        }

        export.Hearing2.Assign(local.Initialised);
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        return;
      case "SIGNOFF":
        // ---------------------------------------------
        // Sign the user off the KESSEP system.
        // ---------------------------------------------
        UseScCabSignoff();

        return;
      case "ENTER":
        // ---------------------------------------------
        // The ENTER key will not be used for functionality
        // here. If it is pressed, an exit state message
        // should be output.
        // ---------------------------------------------
        ExitState = "ACO_NI0000_SELECT_FUNCTION";

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
          .LegalAction.Identifier > 0 && (
            Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation) && Equal
          (export.Fips.CountyAbbreviation, export.HiddenFips.CountyAbbreviation) ||
          IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
          (export.Fips.CountyAbbreviation) && export.Tribunal.Identifier == export
          .HiddenTribunal.Identifier))
        {
          if (ReadLegalAction())
          {
            MoveLegalAction2(entities.LegalAction, export.LegalAction);
            export.HiddenLegalAction.Assign(entities.LegalAction);

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseLeGetPetitionerRespondent();
            }

            if (ReadTribunal())
            {
              MoveTribunal(entities.Tribunal, export.Tribunal);
              export.HiddenTribunal.Identifier = entities.Tribunal.Identifier;
            }
            else
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_TRIBUNAL_NF";

              return;
            }

            if (ReadFips())
            {
              export.Fips.Assign(entities.Fips);
              export.HiddenFips.Assign(entities.Fips);
            }
            else if (ReadFipsTribAddress2())
            {
              export.Foreign.Country = entities.FipsTribAddress.Country;
            }
            else
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_TRIBUNAL_ADDRESS_NF";

              return;
            }
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }
        }
        else
        {
          if (Equal(local.UserAction.Command, "HRPV") || Equal
            (local.UserAction.Command, "HRNX"))
          {
            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            return;
          }

          // ---------------------------------------------
          // If the Court Case Number that was entered,
          // exists on more than one Legal Action, display
          // a list screen to select the desired one.
          // ---------------------------------------------
          local.NoOfLegalActRecs.Count = 0;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            foreach(var item in ReadLegalActionTribunalFips())
            {
              if (!IsEmpty(export.LegalAction.Classification) && AsChar
                (entities.LegalAction.Classification) != AsChar
                (export.LegalAction.Classification))
              {
                continue;
              }

              ++local.NoOfLegalActRecs.Count;

              if (local.NoOfLegalActRecs.Count > 1)
              {
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              if (local.NoOfLegalActRecs.Count == 1)
              {
                MoveLegalAction2(entities.LegalAction, export.LegalAction);

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseLeGetPetitionerRespondent();
                }

                MoveTribunal(entities.Tribunal, export.Tribunal);
                export.Fips.Assign(entities.Fips);
              }
            }

            if (local.NoOfLegalActRecs.Count == 0)
            {
              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Fips, "stateAbbreviation");

              field2.Error = true;

              var field3 = GetField(export.Fips, "countyAbbreviation");

              field3.Error = true;

              return;
            }
          }
          else if (export.Tribunal.Identifier == 0)
          {
            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";

            var field = GetField(export.ListTribunal, "promptField");

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

              ++local.NoOfLegalActRecs.Count;

              if (local.NoOfLegalActRecs.Count > 1)
              {
                export.LegalAction.Classification =
                  import.LegalAction.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              if (local.NoOfLegalActRecs.Count == 1)
              {
                MoveLegalAction2(entities.LegalAction, export.LegalAction);

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseLeGetPetitionerRespondent();
                }

                MoveTribunal(entities.Tribunal, export.Tribunal);
              }
            }

            if (local.NoOfLegalActRecs.Count == 0)
            {
              ExitState = "LE0000_INVALID_CT_CASE_NO_N_TRIB";

              var field1 = GetField(export.LegalAction, "courtCaseNumber");

              field1.Error = true;

              var field2 = GetField(export.Foreign, "country");

              field2.Error = true;

              return;
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

        UseLeDisplayLegalHearing();

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          for(export.Addresses.Index = 0; export.Addresses.Index < export
            .Addresses.Count; ++export.Addresses.Index)
          {
            if (IsEmpty(export.Addresses.Item.FipsTribAddress.Country))
            {
              continue;
            }

            export.Foreign.Country =
              export.Addresses.Item.FipsTribAddress.Country ?? "";

            break;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          // CQ58574
          export.HiddenHearing.Note = export.Hearing2.Note;
        }
        else
        {
          // ------------------------------------------------
          // Check the exit state of the display.
          // ------------------------------------------------
          if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Fips, "stateAbbreviation");

            field2.Error = true;

            var field3 = GetField(export.Fips, "countyAbbreviation");

            field3.Error = true;

            var field4 = GetField(export.LegalAction, "classification");

            field4.Error = true;
          }
          else if (IsExitState("LE0000_MORE_HEARINGS_EXIST"))
          {
            var field = GetField(export.Hearing2, "conductedDate");

            field.Error = true;
          }
          else if (IsExitState("HEARING_NF"))
          {
            if (Lt(local.Initialised.ConductedDate,
              export.Hearing2.ConductedDate))
            {
              var field = GetField(export.Hearing2, "conductedDate");

              field.Error = true;
            }
          }
          else
          {
          }

          return;
        }

        break;
      case "REDISP":
        local.UserAction.Command = "DISPLAY";

        if (ReadLegalAction())
        {
          MoveLegalAction2(entities.LegalAction, export.LegalAction);
          export.HiddenLegalAction.Assign(entities.LegalAction);

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseLeGetPetitionerRespondent();
          }

          // ---------------------------------------------
          // Use the code_value table to obtain the
          // description for the legal_action action_taken
          // ---------------------------------------------
          if (!IsEmpty(export.LegalAction.ActionTaken))
          {
            UseLeGetActionTakenDescription();
          }

          if (ReadTribunal())
          {
            MoveTribunal(entities.Tribunal, export.Tribunal);
            export.HiddenTribunal.Identifier = entities.Tribunal.Identifier;
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_TRIBUNAL_NF";

            return;
          }

          if (ReadFips())
          {
            export.Fips.Assign(entities.Fips);
            export.HiddenFips.Assign(entities.Fips);
          }
          else if (ReadFipsTribAddress2())
          {
            export.Foreign.Country = entities.FipsTribAddress.Country;
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_TRIBUNAL_ADDRESS_NF";

            return;
          }
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        UseLeDisplayLegalHearing();

        if (IsEmpty(export.Fips.StateAbbreviation))
        {
          for(export.Addresses.Index = 0; export.Addresses.Index < export
            .Addresses.Count; ++export.Addresses.Index)
          {
            if (IsEmpty(export.Addresses.Item.FipsTribAddress.Country))
            {
              continue;
            }

            export.Foreign.Country =
              export.Addresses.Item.FipsTribAddress.Country ?? "";

            break;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          // CQ58574
          export.HiddenHearing.Note = export.Hearing2.Note;
        }
        else
        {
          // ------------------------------------------------
          // Check the exit state of the display.
          // ------------------------------------------------
          if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.Fips, "stateAbbreviation");

            field2.Error = true;

            var field3 = GetField(export.Fips, "countyAbbreviation");

            field3.Error = true;

            var field4 = GetField(export.LegalAction, "classification");

            field4.Error = true;
          }
          else if (IsExitState("LE0000_MORE_HEARINGS_EXIST"))
          {
            var field = GetField(export.Hearing2, "conductedDate");

            field.Error = true;
          }
          else if (IsExitState("HEARING_NF"))
          {
            if (Lt(local.Initialised.ConductedDate,
              export.Hearing2.ConductedDate))
            {
              var field = GetField(export.Hearing2, "conductedDate");

              field.Error = true;
            }
          }
          else
          {
          }

          return;
        }

        break;
      default:
        break;
    }

    // ------------------------------------------------
    // If these dates were stored as max dates,
    // (12312099), convert them to zeros and don't
    // display them on the screen.
    // ------------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(export.Hearing2.ConductedDate, local.Max.Date))
    {
      export.Hearing2.ConductedDate = null;
    }

    // ------------------------------------------------
    // If all processing completed successfully, move
    // all exports to previous exports.
    // ------------------------------------------------
    MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
    export.HiddenFips.Assign(export.Fips);
    export.HiddenTribunal.Identifier = export.Tribunal.Identifier;
    export.PromptClass.SelectChar = "";
    export.HiddenNextTranInfo.CourtCaseNumber =
      export.LegalAction.CourtCaseNumber ?? "";
    export.HiddenNextTranInfo.LegalActionIdentifier =
      export.LegalAction.Identifier;
  }

  private static void MoveAddresses(ObtainHearingDate.Export.
    AddressesGroup source, Export.AddressesGroup target)
  {
    target.FipsTribAddress.Assign(source.FipsTribAddress);
  }

  private static void MoveAddresses1(LeDisplayLegalHearing.Export.
    Addresses1Group source, Export.AddressesGroup target)
  {
    target.FipsTribAddress.Assign(source.FipsTribAddress);
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.TextDate = source.TextDate;
    target.Date = source.Date;
  }

  private static void MoveHearing(Hearing source, Hearing target)
  {
    target.ConductedDate = source.ConductedDate;
    target.OutcomeReceivedDate = source.OutcomeReceivedDate;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
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

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeDeleteHearing()
  {
    var useImport = new LeDeleteHearing.Import();
    var useExport = new LeDeleteHearing.Export();

    useImport.Hearing.SystemGeneratedIdentifier =
      export.Hearing2.SystemGeneratedIdentifier;

    Call(LeDeleteHearing.Execute, useImport, useExport);
  }

  private void UseLeDisplayLegalHearing()
  {
    var useImport = new LeDisplayLegalHearing.Import();
    var useExport = new LeDisplayLegalHearing.Export();

    useImport.UserAction.Command = local.UserAction.Command;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Hearing.Assign(export.Hearing2);

    Call(LeDisplayLegalHearing.Execute, useImport, useExport);

    MoveScrollingAttributes(useExport.Hearing1, export.Hearing1);
    export.Hearing2.Assign(useExport.Hearing2);
    MoveTribunal(useExport.Tribunal, export.Tribunal);
    useExport.Addresses1.CopyTo(export.Addresses, MoveAddresses1);
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

  private void UseLeHearRaiseInfrastrucEvents()
  {
    var useImport = new LeHearRaiseInfrastrucEvents.Import();
    var useExport = new LeHearRaiseInfrastrucEvents.Export();

    MoveHearing(export.Hearing2, useImport.Hearing);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeHearRaiseInfrastrucEvents.Execute, useImport, useExport);
  }

  private void UseObtainHearingDate()
  {
    var useImport = new ObtainHearingDate.Import();
    var useExport = new ObtainHearingDate.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Hearing.Assign(export.Hearing2);

    Call(ObtainHearingDate.Execute, useImport, useExport);

    useExport.Addresses.CopyTo(export.Addresses, MoveAddresses);
    export.Hearing2.Assign(useExport.Hearing);
  }

  private void UseReceiveLegalHearingResult()
  {
    var useImport = new ReceiveLegalHearingResult.Import();
    var useExport = new ReceiveLegalHearingResult.Export();

    useImport.Hearing.Assign(export.Hearing2);

    Call(ReceiveLegalHearingResult.Execute, useImport, useExport);

    local.OutcomeRcvdDateChanged.Flag = useExport.OutcomeRcvdDateChanged.Flag;
    local.HearingDateChanged.Flag = useExport.HearingDateChanged.Flag;
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

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.Specific.ActionReasonCode = local.Specific.ActionReasonCode;
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    MoveLegalAction1(export.LegalAction, useImport.LegalAction);
    MoveDateWorkArea(local.MiscellaneousInformation, useImport.Misc);

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
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
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.Name = db.GetString(reader, 5);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 7);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalActionTribunalFips",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 4);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.Name = db.GetString(reader, 5);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 6);
        entities.Fips.Location = db.GetInt32(reader, 6);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 7);
        entities.Fips.County = db.GetInt32(reader, 7);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 8);
        entities.Fips.State = db.GetInt32(reader, 8);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 9);
        entities.Fips.StateAbbreviation = db.GetString(reader, 10);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 11);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

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
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 3);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 4);
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
    /// <summary>A AddressesGroup group.</summary>
    [Serializable]
    public class AddressesGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FipsTribAddress fipsTribAddress;
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
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
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
    /// A value of Hearing1.
    /// </summary>
    [JsonPropertyName("hearing1")]
    public ScrollingAttributes Hearing1
    {
      get => hearing1 ??= new();
      set => hearing1 = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Hearing2.
    /// </summary>
    [JsonPropertyName("hearing2")]
    public Hearing Hearing2
    {
      get => hearing2 ??= new();
      set => hearing2 = value;
    }

    /// <summary>
    /// Gets a value of Addresses.
    /// </summary>
    [JsonIgnore]
    public Array<AddressesGroup> Addresses => addresses ??= new(
      AddressesGroup.Capacity);

    /// <summary>
    /// Gets a value of Addresses for json serialization.
    /// </summary>
    [JsonPropertyName("addresses")]
    [Computed]
    public IList<AddressesGroup> Addresses_Json
    {
      get => addresses;
      set => Addresses.Assign(value);
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of HiddenHearing.
    /// </summary>
    [JsonPropertyName("hiddenHearing")]
    public Hearing HiddenHearing
    {
      get => hiddenHearing ??= new();
      set => hiddenHearing = value;
    }

    private CodeValue actionTaken;
    private Tribunal hiddenTribunal;
    private Standard listTribunal;
    private NextTranInfo hiddenNextTranInfo;
    private Fips hiddenFips;
    private Fips fips;
    private ScrollingAttributes hearing1;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Tribunal tribunal;
    private Standard standard;
    private LegalAction legalAction;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private Hearing hearing2;
    private Array<AddressesGroup> addresses;
    private CodeValue dlgflwSelectedCodeValue;
    private LegalAction dlgflwSelectedLegalAction;
    private FipsTribAddress foreign;
    private Hearing hiddenHearing;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AddressesGroup group.</summary>
    [Serializable]
    public class AddressesGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FipsTribAddress fipsTribAddress;
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
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
    }

    /// <summary>
    /// A value of Hearing1.
    /// </summary>
    [JsonPropertyName("hearing1")]
    public ScrollingAttributes Hearing1
    {
      get => hearing1 ??= new();
      set => hearing1 = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Hearing2.
    /// </summary>
    [JsonPropertyName("hearing2")]
    public Hearing Hearing2
    {
      get => hearing2 ??= new();
      set => hearing2 = value;
    }

    /// <summary>
    /// Gets a value of Addresses.
    /// </summary>
    [JsonIgnore]
    public Array<AddressesGroup> Addresses => addresses ??= new(
      AddressesGroup.Capacity);

    /// <summary>
    /// Gets a value of Addresses for json serialization.
    /// </summary>
    [JsonPropertyName("addresses")]
    [Computed]
    public IList<AddressesGroup> Addresses_Json
    {
      get => addresses;
      set => Addresses.Assign(value);
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
    }

    /// <summary>
    /// A value of HiddenHearing.
    /// </summary>
    [JsonPropertyName("hiddenHearing")]
    public Hearing HiddenHearing
    {
      get => hiddenHearing ??= new();
      set => hiddenHearing = value;
    }

    private CodeValue actionTaken;
    private Tribunal hiddenTribunal;
    private Standard listTribunal;
    private Fips fips;
    private Fips hiddenFips;
    private ScrollingAttributes hearing1;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Tribunal tribunal;
    private Standard standard;
    private LegalAction legalAction;
    private Common promptClass;
    private LegalAction hiddenLegalAction;
    private Hearing hearing2;
    private Array<AddressesGroup> addresses;
    private Common displayActiveCasesOnly;
    private Code code;
    private NextTranInfo hiddenNextTranInfo;
    private FipsTribAddress foreign;
    private Hearing hiddenHearing;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MiscellaneousInformation.
    /// </summary>
    [JsonPropertyName("miscellaneousInformation")]
    public DateWorkArea MiscellaneousInformation
    {
      get => miscellaneousInformation ??= new();
      set => miscellaneousInformation = value;
    }

    /// <summary>
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
    }

    /// <summary>
    /// A value of ScreenIdentification.
    /// </summary>
    [JsonPropertyName("screenIdentification")]
    public Common ScreenIdentification
    {
      get => screenIdentification ??= new();
      set => screenIdentification = value;
    }

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of HearingDateChanged.
    /// </summary>
    [JsonPropertyName("hearingDateChanged")]
    public Common HearingDateChanged
    {
      get => hearingDateChanged ??= new();
      set => hearingDateChanged = value;
    }

    /// <summary>
    /// A value of OutcomeRcvdDateChanged.
    /// </summary>
    [JsonPropertyName("outcomeRcvdDateChanged")]
    public Common OutcomeRcvdDateChanged
    {
      get => outcomeRcvdDateChanged ??= new();
      set => outcomeRcvdDateChanged = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public Hearing Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
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
    /// A value of NoOfLegalActRecs.
    /// </summary>
    [JsonPropertyName("noOfLegalActRecs")]
    public Common NoOfLegalActRecs
    {
      get => noOfLegalActRecs ??= new();
      set => noOfLegalActRecs = value;
    }

    /// <summary>
    /// A value of TotalSelected.
    /// </summary>
    [JsonPropertyName("totalSelected")]
    public Common TotalSelected
    {
      get => totalSelected ??= new();
      set => totalSelected = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private DateWorkArea miscellaneousInformation;
    private InterstateRequestHistory specific;
    private Common screenIdentification;
    private DateWorkArea init1;
    private Common hearingDateChanged;
    private Common outcomeRcvdDateChanged;
    private Hearing initialised;
    private Common userAction;
    private Common noOfLegalActRecs;
    private Common totalSelected;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
    private DateWorkArea max;
    private NextTranInfo nextTranInfo;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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

    private Fips fips;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private LegalAction legalAction;
  }
#endregion
}

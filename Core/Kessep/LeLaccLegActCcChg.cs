// Program: LE_LACC_LEG_ACT_CC_CHG, ID: 373551754, model: 746.
// Short name: SWELACCP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_LACC_LEG_ACT_CC_CHG.
/// </para>
/// <para>
/// This procedure is to used by home office staff to change either/or the court
/// case number or the standard number of a legal action.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLaccLegActCcChg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACC_LEG_ACT_CC_CHG program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLaccLegActCcChg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLaccLegActCcChg.
  /// </summary>
  public LeLaccLegActCcChg(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ------------  ------------	
    // ---------------------------------------------------------------------------
    // 01/11/00  R. Jean	PR81987		Procedure allows changes to court case number,
    // tribunal, and standard
    // 					number.  This is a clone of LACT with modifications.
    // 03/28/00  R Jean	PR91341		Prevent any standard number greater than 12 
    // positions
    // 05/03/00  J Magat	PR#94087	Apply same changes as in LACT PR#89636 to 
    // prevent Substring Abend.
    // 07/24/00  J Magat	PR#93654	Apply standard number changes to all Legal 
    // Actions with the same
    // 					standard number.
    // 08/24/00  GVandy	PR101721	Perform court order edits only on update.
    // 08/30/00  GVandy	PR 102554	Protected Court Case Number and Tribunal 
    // fields on the screen
    // 					and disabled edits for these fields.  Re-coded the update pad for 
    // new
    // 					business rules.
    // 11/01/00  GVandy	WR209		Re-coded for new business rules/functional 
    // design.
    // 04/02/01  GVandy	PR 108247	Update monthly court order fees using standard
    // number instead of court
    // 					case number.
    // 08/21/01  GVandy	WR 10346	New rules for updating 'B' class actions verses
    // non 'B' class actions.
    // 06/21/02  GVandy	PR 00133508	Remove edit check for collections if 
    // previous standard number was blank
    // 06/21/02  GVandy	PR 00143438	Use cab LE_GET_ACTION_TAKEN_DESCRIPTION 
    // instead of CAB_VALIDATE_CODE_VALUE
    // 					to retrieve action taken description.
    // 05/06/03  GVandy	PR126879	Generate standard numbers for foreign court 
    // orders.
    // -------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.CurrentDate.Date = Now().Date;
    local.CurrentDate.Timestamp = Now();

    if (Equal(global.Command, "CLEAR"))
    {
      MoveLegalAction3(import.HiddenLegalAction, export.LegalAction);
      export.Tribunal.Assign(import.HiddenTribunal);
      export.Fips.Assign(import.HiddenFips);

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
    MoveOffice(import.OspEstabOffice, export.OspEstabOffice);
    MoveOfficeServiceProvider(import.OspEstabOfficeServiceProvider,
      export.OspEstabOfficeServiceProvider);
    export.OspEstabServiceProvider.Assign(import.OspEstabServiceProvider);
    MoveOffice(import.OspEnforcingOffice, export.OspEnforcingOffice);
    MoveOfficeServiceProvider(import.OspEnforcingOfficeServiceProvider,
      export.OspEnforcingOfficeServiceProvider);
    export.OspEnforcingServiceProvider.
      Assign(import.OspEnforcingServiceProvider);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonsWorkSet(import.Ap, export.Ap);
    MoveCsePersonsWorkSet(import.Ar, export.Ar);
    export.Fips.Assign(import.Fips);
    export.Tribunal.Assign(import.Tribunal);
    export.FipsTribAddress.Country = import.FipsTribAddress.Country;
    export.LegalAction.Assign(import.LegalAction);
    export.ActionTaken.Description = import.ActionTaken.Description;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    MoveCsePersonsWorkSet(import.AltBillingLocn, export.AltBillingLocn);

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenTribunal.Assign(import.HiddenTribunal);
    export.HiddenFips.Assign(import.HiddenFips);
    MoveLegalAction6(import.PreviousExecutionHLegalAction,
      export.PreviousExecutionHLegalAction);
    export.PreviousExecutionHTribunal.Identifier =
      import.PreviousExecutionHTribunal.Identifier;
    export.Phase.Count = import.Phase.Count;

    // ---------------------------------------------
    // Security and Nexttran code starts here
    // ---------------------------------------------
    // ------------------------------------------------------
    // The following statements must be placed after MOVE
    // imports to exports
    // ------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ----------------------------------------------------------
      // Populate export views from local next_tran_info view read
      // from the data base
      // Set command to initial command required or ESCAPE
      // -----------------------------------------------------------
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";
      export.LegalAction.Identifier =
        export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();

      if (export.LegalAction.Identifier == 0)
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
      // -------------------------------------------------------------
      // Set up local next_tran_info for saving the current values for
      // the next screen
      // -------------------------------------------------------------
      export.HiddenNextTranInfo.CourtCaseNumber =
        export.LegalAction.CourtCaseNumber ?? "";
      export.HiddenNextTranInfo.LegalActionIdentifier =
        export.LegalAction.Identifier;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
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
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        MoveLegalAction6(local.NullLegalAction,
          export.PreviousExecutionHLegalAction);
        export.PreviousExecutionHTribunal.Identifier =
          local.NullTribunal.Identifier;
        export.Phase.Count = 0;
        UseLeDisplayLegalAction();
        local.Max.Date = UseCabSetMaximumDiscontinueDate();

        // ---------------------------------------------------------------
        // If these dates were stored as max dates, (12312099),
        // convert them to zeros and don't display them on the screen.
        // ---------------------------------------------------------------
        if (Equal(export.LegalAction.EndDate, local.Max.Date))
        {
          export.LegalAction.EndDate = null;
        }

        if (Equal(export.LegalAction.LastModificationReviewDate, local.Max.Date))
          
        {
          export.LegalAction.LastModificationReviewDate = null;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          return;
        }

        // ---------------------------------------------------------------
        // Use the code_value table to obtain the description for the
        // legal_action action_taken
        // ---------------------------------------------------------------
        if (!IsEmpty(export.LegalAction.ActionTaken))
        {
          // 06/21/02  GVandy  PR 00143438  Use cab 
          // LE_GET_ACTION_TAKEN_DESCRIPTION instead of
          // CAB_VALIDATE_CODE_VALUE to retrieve action taken description.
          UseLeGetActionTakenDescription();
        }

        UseLeGetPetitionerRespondent();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
          export.HiddenFips.Assign(export.Fips);
          export.HiddenTribunal.Assign(export.Tribunal);

          // -- Move the current tribunal and legal action attributes to the
          // previous views (for the next execution of the prad).
          MoveLegalAction6(export.LegalAction,
            export.PreviousExecutionHLegalAction);
          export.PreviousExecutionHTribunal.Identifier =
            export.Tribunal.Identifier;
        }

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "LIST":
        // ---------------------------------------------
        // Validate Prompt Tribunal.
        // ---------------------------------------------
        switch(AsChar(import.PromptTribunal.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptTribunal, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // A Prompt must be selected when PF4 List is
        // pressed.
        // ---------------------------------------------
        if (local.TotalPromptsSelected.Count == 0)
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          return;
        }

        if (AsChar(export.PromptTribunal.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
        }

        break;
      case "RLTRIB":
        // ---------------------------------------------
        // Returned from List Tribunals screen. Move
        // values to export.
        // ---------------------------------------------
        if (AsChar(export.PromptTribunal.SelectChar) == 'S')
        {
          export.PromptTribunal.SelectChar = "";

          var field = GetField(export.PromptTribunal, "selectChar");

          field.Protected = false;
          field.Focused = true;

          if (import.DlgflwSelectedTribunal.Identifier > 0)
          {
            export.Fips.Assign(import.DlgflwSelectedFips);
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);
            export.LegalAction.ForeignFipsState = export.Fips.State;
            export.LegalAction.ForeignFipsCounty = export.Fips.County;
            export.LegalAction.ForeignFipsLocation = export.Fips.Location;
          }
        }

        break;
      case "UPDATE":
        // ****************************************************************
        // Verify a display has been performed before updating.
        // ****************************************************************
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // ****************************************************************
        // Determine if the tribunal is a tribal court.
        // ****************************************************************
        if (export.Fips.Location >= 20 && export.Fips.Location <= 99)
        {
          local.TribalTribunal.Flag = "Y";
        }

        // ------------------------------------------------------------
        //             U P D A T E    L O G I C
        // The update logic is divided into 6 phases as follows:
        // Phase 1: Tribunal and Court Case Number edits
        // Phase 2: (Optional) Standard Number generation
        // Phase 3: Initial Standard Number edits
        // Phase 4: Standard Number Confirmation
        // Phase 5: Final Standard Number edits
        // Phase 6: Performing the Requested Updates
        // Error, warning, or confirmation messages may be presented
        // to the user during each phase.  We don't advance to the
        // next phase until we successfully complete the current phase.
        // Also, once completed, we don't re-do a phase unless the
        // user modifies corresponding data on the screen.  This
        // prevents warning and confirmation messages being
        // displayed over and over again.
        // Note:  This approach was dictated by the new business rules
        // and functional design from WR 209.
        // ------------------------------------------------------------
        if (export.Phase.Count == 0)
        {
          export.Phase.Count = 1;
        }

        // ****************************************************************
        // If the user changes the standard number at confirmation
        // time, then return back to Phase 3, standard number
        // validation to insure the new number is valid.
        // ****************************************************************
        if ((export.Phase.Count == 5 || export.Phase.Count == 6) && !
          Equal(export.LegalAction.StandardNumber,
          import.PreviousExecutionHLegalAction.StandardNumber))
        {
          export.Phase.Count = 3;
        }

        // ****************************************************************
        // If the user blanks out the standard number then return
        // back to Phase 2, standard number generation.
        // ****************************************************************
        if (export.Phase.Count > 1 && IsEmpty
          (export.LegalAction.StandardNumber))
        {
          export.Phase.Count = 2;
        }

        // ****************************************************************
        // If the user changes the tribunal or court case number
        // then return back to Phase 1, tribunal and court case
        // number edits.
        // ****************************************************************
        if (export.Tribunal.Identifier != import
          .PreviousExecutionHTribunal.Identifier || !
          Equal(export.LegalAction.CourtCaseNumber,
          import.PreviousExecutionHLegalAction.CourtCaseNumber))
        {
          export.Phase.Count = 1;
        }

        // -- Move the current tribunal and legal action attributes to the
        // previous views (for the next execution of the prad).
        MoveLegalAction6(export.LegalAction,
          export.PreviousExecutionHLegalAction);
        export.PreviousExecutionHTribunal.Identifier =
          export.Tribunal.Identifier;

        // ****************************************************************
        // P H A S E 1 :  Tribunal and Court Case Number Edits
        // ****************************************************************
        if (export.Phase.Count == 1)
        {
          // ****************************************************************
          // A tribunal must be selected.
          // ****************************************************************
          if (export.Tribunal.Identifier == 0)
          {
            var field = GetField(export.PromptTribunal, "selectChar");

            field.Error = true;

            ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";
          }

          // ****************************************************************
          // The tribunal selected cannot be a court trustee (FIPS Location code
          // 12)
          // ****************************************************************
          if (export.Fips.Location == 12)
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Fips, "countyAbbreviation");

            field2.Error = true;

            ExitState = "LE0000_TRIBUNAL_NOT_ALLOWED";
          }

          // ****************************************************************
          // The court case number cannot be blank.
          // ****************************************************************
          if (IsEmpty(export.LegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // --- The remaining Phase I edits rely on a tribunal having been
            // selected and/or a court case number being entered.  If they
            // are not then escape.
            return;
          }

          // ****************************************************************
          // The court case number cannot contain embedded spaces.
          // ****************************************************************
          if (Find(TrimEnd(export.LegalAction.CourtCaseNumber), " ") > 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_NO_EMBED_SPACE_ALLOWED";

            return;
          }

          // ****************************************************************
          // For non-tribal Kansas courts, position 1-2 of the court case
          // number must be numeric.  Allow tribal tribunals to enter court
          // case numbers in any form.  Bankruptcy court case numbers
          // also may be in any form.
          // ****************************************************************
          if (Equal(export.Fips.StateAbbreviation, "KS") && AsChar
            (local.TribalTribunal.Flag) != 'Y' && AsChar
            (export.LegalAction.Classification) != 'B')
          {
            if (Verify(Substring(export.LegalAction.CourtCaseNumber, 17, 1, 1),
              "0123456789") != 0)
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_KS_CRT_CASE_NUM_NO_ALPHA";

              return;
            }

            if (Verify(Substring(export.LegalAction.CourtCaseNumber, 17, 1, 2),
              "0123456789") != 0)
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_CT_CASE_NO_1ST_2_BE_YEAR";

              return;
            }
          }

          // ****************************************************************
          // For multi-tribunal counties the last character of the court case
          // number must correspond to the city as follows:
          // Tribunal				Last Character
          // ---------------------------------
          // 	--------------
          // 669 (Cowley county - Winfield)		W
          // 670 (Cowley county - Arkansas City)	A
          // 671 (Crawford county - Girard)		G
          // 672 (Crawford county - Pittsburg)	P
          // 703 (Labette county - Parsons)		P
          // 704 (Labette county - Oswege)		O
          // 717 (Montgomery county - Independence)	I
          // 718 (Montgomery county - Coffeyville)	C
          // 722 (Neosho county - Chanute)		C
          // 723 (Neosho county - Erie)		E
          // ****************************************************************
          // *** Get last CHAR of Tribunal court case.
          local.LastChar.TextLength01 =
            Substring(export.LegalAction.CourtCaseNumber,
            Length(TrimEnd(export.LegalAction.CourtCaseNumber)), 1);

          // *** Ensure non-numeric last char is valid for Tribunal.
          local.MultTribError.Flag = "";

          switch(export.Tribunal.Identifier)
          {
            case 669:
              if (AsChar(local.LastChar.TextLength01) != 'W')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 670:
              if (AsChar(local.LastChar.TextLength01) != 'A')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 671:
              if (AsChar(local.LastChar.TextLength01) != 'G')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 672:
              if (AsChar(local.LastChar.TextLength01) != 'P')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 703:
              if (AsChar(local.LastChar.TextLength01) != 'P')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 704:
              if (AsChar(local.LastChar.TextLength01) != 'O')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 717:
              if (AsChar(local.LastChar.TextLength01) != 'I')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 718:
              if (AsChar(local.LastChar.TextLength01) != 'C')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 722:
              if (AsChar(local.LastChar.TextLength01) != 'C')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            case 723:
              if (AsChar(local.LastChar.TextLength01) != 'E')
              {
                local.MultTribError.Flag = "Y";
              }

              break;
            default:
              break;
          }

          if (AsChar(local.MultTribError.Flag) == 'Y')
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            if (Verify(local.LastChar.TextLength01, "0123456789") == 0)
            {
              ExitState = "LE0000_MULT_TRIB_CC_LST_POS_NBR";
            }
            else
            {
              ExitState = "LE0000_MULT_TRIB_CC_NOT_MATCH_2";
            }

            return;
          }

          // ****************************************************************
          // The requested change cannot result in a legal action with
          // a duplicate tribunal, court case number, action taken, and
          // filed date.
          // ****************************************************************
          if (ReadLegalAction1())
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LEGAL_ACTION_AE";

            return;
          }

          // ****************************************************************
          // We passed all the Phase 1 edits.  Advance to Phase 2.
          // -- Note that this is deliberately set to Phase 2 before the
          // following confirmation check.  We want to advance to
          // phase 2 whether or not we present the confirmation.
          // If we don't present the confirmation message then we'll
          // just continue to phase 2.  If we do present the confirmation
          // then when the user presses F6 again, we'll bypass phase 1
          // and start at phase 2.
          // ****************************************************************
          export.Phase.Count = 2;

          // ****************************************************************
          // Display a warning message if the tribunal and/or court
          // case number were changed and legal actions already exist
          // under the new tribunal and court case number.
          // ****************************************************************
          if (export.Tribunal.Identifier != export
            .HiddenTribunal.Identifier || !
            Equal(export.LegalAction.CourtCaseNumber,
            export.HiddenLegalAction.CourtCaseNumber))
          {
            ReadLegalAction2();

            if (local.Common.Count > 0)
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              ExitState = "LE0000_CONFIRM_COURT_CASE_NUMBER";

              // -- The following flag is set to correct the following 
              // condition.  If not set then on the next execution of the PrAD
              // the standard number is not generated in Phase 2 because the
              // export views have already been moved to the previous execution
              // views.
              export.GenerateStandardNumber.Flag = "Y";

              return;
            }
          }
        }

        // ****************************************************************
        // P H A S E 2 :  (Optional) Standard Number generation
        // ****************************************************************
        if (export.Phase.Count == 2)
        {
          if (export.Tribunal.Identifier != import
            .PreviousExecutionHTribunal.Identifier || !
            Equal(export.LegalAction.CourtCaseNumber,
            import.PreviousExecutionHLegalAction.CourtCaseNumber) || IsEmpty
            (export.LegalAction.StandardNumber) || AsChar
            (import.GenerateStandardNumber.Flag) == 'Y')
          {
            // ****************************************************************
            // The user changed the tribunal or court case number.
            // Generate a new standard number.
            // ****************************************************************
            UseLeCabConvCrtCaseNumToStd();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              return;
            }

            MoveLegalAction6(export.LegalAction,
              export.PreviousExecutionHLegalAction);
          }

          // ****************************************************************
          // Advance to Phase 3.
          // ****************************************************************
          export.Phase.Count = 3;
        }

        // ****************************************************************
        // P H A S E 3 :  Initial Standard Number Edits
        // ****************************************************************
        if (export.Phase.Count == 3)
        {
          // ****************************************************************
          // Validate the court case number to the standard number.
          // ****************************************************************
          UseLeCabValidateStdCtOrdNo();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("LE0000_CT_CASE_NO_1ST_2_BE_YEAR"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            return;
          }
          else
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            return;
          }

          // ****************************************************************
          // We passed all the Phase 3 edits.  Advance to Phase 4.
          // ****************************************************************
          export.Phase.Count = 4;
        }

        // ****************************************************************
        // P H A S E 4 :  Standard Number Confirmation
        // ****************************************************************
        if (export.Phase.Count == 4)
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Protected = false;
          field.Focused = true;

          ExitState = "LE0000_VERIFY_STD_NUM_BEFOR_UPDT";

          // ****************************************************************
          // Advance to Phase 5.  (Once the user confirms)
          // ****************************************************************
          export.Phase.Count = 5;

          return;
        }

        // ****************************************************************
        // P H A S E 5 :  Final Standard Number edits
        // ****************************************************************
        if (export.Phase.Count == 5)
        {
          // ****************************************************************
          // If collections exist under both the old and new standard
          // number give the user an error message.  The update is not
          // allowed.
          // ****************************************************************
          if (AsChar(export.LegalAction.Classification) == 'B')
          {
            // -- Skip the edit for collections under the old and new standard 
            // number for 'B' class legal actions.
          }
          else
          {
            // 06/21/02  GVandy  PR 00133508  Remove edit check for collections 
            // if previous standard number was blank
            if (!Equal(export.LegalAction.StandardNumber,
              export.HiddenLegalAction.StandardNumber) && !
              IsEmpty(export.HiddenLegalAction.StandardNumber))
            {
              ReadCollection2();

              if (local.NumberOfOldCollections.Count > 0)
              {
                ReadCollection1();

                if (local.NumberOfNewCollections.Count > 0)
                {
                  var field = GetField(export.LegalAction, "standardNumber");

                  field.Error = true;

                  ExitState = "LE0000_COLLECTIONS_EXIST_NO_UPD";

                  return;
                }
              }
            }
          }

          // ****************************************************************
          // We passed all the Phase 5 edits.  Advance to phase 6.
          // -- Note that this is deliberately set to Phase 6 before the
          // following confirmation check.  We want to advance to
          // phase 6 whether or not we present the confirmation.
          // If we don't present the confirmation message then we'll
          // just continue to phase 6.  If we do present the confirmation
          // then when the user presses F6 again, we'll bypass phase 5
          // and start at phase 6.
          // ****************************************************************
          export.Phase.Count = 6;

          // ****************************************************************
          // If legal actions exist under the new standard number give the user
          // a warning message and wait for confirmation.
          // ****************************************************************
          ReadLegalAction3();

          if (local.Common.Count > 0)
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;

            ExitState = "LE0000_CONFIRM_STANDARD_NUMBER2";

            return;
          }
        }

        // ****************************************************************
        // P H A S E 6 :  Perform the Updates
        // ****************************************************************
        if (export.Phase.Count == 6)
        {
          UseUpdateLegalActionCourtCase();

          if (IsExitState("LE0000_ALT_BILL_LOCN_CSE_PERS_NF"))
          {
          }
          else if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }
          else if (IsExitState("LEGAL_ACTION_NU"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }
          else if (IsExitState("TRIBUNAL_NF_RB"))
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Color = "red";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Fips, "countyDescription");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Protected = true;
          }
          else if (IsExitState("LE0000_PAYMT_MADE_STD_NOT_UPDT"))
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;
          }
          else if (IsExitState("LE0000_PAYMT_MADE_TRIB_NOT_UPDT"))
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Color = "red";
            field1.Intensity = Intensity.High;
            field1.Protected = true;

            var field2 = GetField(export.Fips, "countyDescription");

            field2.Color = "red";
            field2.Intensity = Intensity.High;
            field2.Protected = true;
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }

          if (AsChar(local.NoBClassUpdated.Flag) == 'Y')
          {
            ExitState = "LE0000_UPDATE_SUCCESSFUL_NO_B_CL";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }

          MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
          export.HiddenFips.Assign(export.Fips);
          export.HiddenTribunal.Assign(export.Tribunal);
        }

        break;
      case "RETURN":
        // -------------------------------------------------------------
        // Otherwise it is a normal return to the linking procedure
        // -------------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // ---------------------------------------------
        // Sign the user off the KESSEP system.
        // ---------------------------------------------
        UseScCabSignoff();

        break;
      case "ENTER":
        // ---------------------------------------------------------------
        // The ENTER key will not be used for functionality here. If it is
        // pressed, an exit state message should be output.
        // ---------------------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "INVALID":
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
      default:
        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction5(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction6(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeCabConvCrtCaseNumToStd()
  {
    var useImport = new LeCabConvCrtCaseNumToStd.Import();
    var useExport = new LeCabConvCrtCaseNumToStd.Export();

    MoveLegalAction4(export.LegalAction, useImport.LegalAction);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;

    Call(LeCabConvCrtCaseNumToStd.Execute, useImport, useExport);

    MoveLegalAction6(useExport.LegalAction, export.LegalAction);
  }

  private void UseLeCabValidateStdCtOrdNo()
  {
    var useImport = new LeCabValidateStdCtOrdNo.Import();
    var useExport = new LeCabValidateStdCtOrdNo.Export();

    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    useImport.LaccPrevTribunal.Identifier = export.HiddenTribunal.Identifier;
    MoveLegalAction6(export.HiddenLegalAction, useImport.LaccPrevLegalAction);
    MoveLegalAction5(export.LegalAction, useImport.LegalAction);

    Call(LeCabValidateStdCtOrdNo.Execute, useImport, useExport);
  }

  private void UseLeDisplayLegalAction()
  {
    var useImport = new LeDisplayLegalAction.Import();
    var useExport = new LeDisplayLegalAction.Export();

    MoveLegalAction1(export.LegalAction, useImport.LegalAction);

    Call(LeDisplayLegalAction.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Ap, export.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.Fips.Assign(useExport.Fips);
    export.Tribunal.Assign(useExport.Tribunal);
    export.LegalAction.Assign(useExport.LegalAction);
    MoveOffice(useExport.OspEnforcingOffice, export.OspEnforcingOffice);
    MoveOfficeServiceProvider(useExport.OspEnforcingOfficeServiceProvider,
      export.OspEnforcingOfficeServiceProvider);
    export.OspEnforcingServiceProvider.Assign(
      useExport.OspEnforcingServiceProvider);
    MoveCsePersonsWorkSet(useExport.AltBillingAddrLocn, export.AltBillingLocn);
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

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
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

    useImport.CsePersonsWorkSet.Number = export.Ap.Number;
    MoveLegalAction2(export.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseUpdateLegalActionCourtCase()
  {
    var useImport = new UpdateLegalActionCourtCase.Import();
    var useExport = new UpdateLegalActionCourtCase.Export();

    useImport.PriorTribunal.Identifier = import.HiddenTribunal.Identifier;
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveLegalAction5(export.LegalAction, useImport.LegalAction);
    useImport.PriorLegalAction.Assign(export.HiddenLegalAction);

    Call(UpdateLegalActionCourtCase.Execute, useImport, useExport);

    local.NoBClassUpdated.Flag = useExport.NoBClassUpdated.Flag;
  }

  private bool ReadCollection1()
  {
    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ctOrdAppliedTo", export.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        local.NumberOfNewCollections.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCollection2()
  {
    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ctOrdAppliedTo",
          export.HiddenLegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        local.NumberOfOldCollections.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadLegalAction1()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", export.LegalAction.FiledDate.GetValueOrDefault());
          
        db.SetString(command, "actionTaken", export.LegalAction.ActionTaken);
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadLegalAction3()
  {
    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
        db.SetNullableInt32(command, "trbId", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        local.Common.Count = db.GetInt32(reader, 0);
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
    /// A value of GenerateStandardNumber.
    /// </summary>
    [JsonPropertyName("generateStandardNumber")]
    public Common GenerateStandardNumber
    {
      get => generateStandardNumber ??= new();
      set => generateStandardNumber = value;
    }

    /// <summary>
    /// A value of Phase.
    /// </summary>
    [JsonPropertyName("phase")]
    public Common Phase
    {
      get => phase ??= new();
      set => phase = value;
    }

    /// <summary>
    /// A value of PreviousExecutionHTribunal.
    /// </summary>
    [JsonPropertyName("previousExecutionHTribunal")]
    public Tribunal PreviousExecutionHTribunal
    {
      get => previousExecutionHTribunal ??= new();
      set => previousExecutionHTribunal = value;
    }

    /// <summary>
    /// A value of PreviousExecutionHLegalAction.
    /// </summary>
    [JsonPropertyName("previousExecutionHLegalAction")]
    public LegalAction PreviousExecutionHLegalAction
    {
      get => previousExecutionHLegalAction ??= new();
      set => previousExecutionHLegalAction = value;
    }

    /// <summary>
    /// A value of PromptListAltAddrLoc.
    /// </summary>
    [JsonPropertyName("promptListAltAddrLoc")]
    public Common PromptListAltAddrLoc
    {
      get => promptListAltAddrLoc ??= new();
      set => promptListAltAddrLoc = value;
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
    /// A value of PromptListActionsTaken.
    /// </summary>
    [JsonPropertyName("promptListActionsTaken")]
    public Common PromptListActionsTaken
    {
      get => promptListActionsTaken ??= new();
      set => promptListActionsTaken = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
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
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of PromptPmtLocation.
    /// </summary>
    [JsonPropertyName("promptPmtLocation")]
    public Common PromptPmtLocation
    {
      get => promptPmtLocation ??= new();
      set => promptPmtLocation = value;
    }

    /// <summary>
    /// A value of PromptOrderAuth.
    /// </summary>
    [JsonPropertyName("promptOrderAuth")]
    public Common PromptOrderAuth
    {
      get => promptOrderAuth ??= new();
      set => promptOrderAuth = value;
    }

    /// <summary>
    /// A value of PromptType.
    /// </summary>
    [JsonPropertyName("promptType")]
    public Common PromptType
    {
      get => promptType ??= new();
      set => promptType = value;
    }

    /// <summary>
    /// A value of PromptInitState.
    /// </summary>
    [JsonPropertyName("promptInitState")]
    public Common PromptInitState
    {
      get => promptInitState ??= new();
      set => promptInitState = value;
    }

    /// <summary>
    /// A value of PromptRespState.
    /// </summary>
    [JsonPropertyName("promptRespState")]
    public Common PromptRespState
    {
      get => promptRespState ??= new();
      set => promptRespState = value;
    }

    /// <summary>
    /// A value of PromptDismissalCode.
    /// </summary>
    [JsonPropertyName("promptDismissalCode")]
    public Common PromptDismissalCode
    {
      get => promptDismissalCode ??= new();
      set => promptDismissalCode = value;
    }

    /// <summary>
    /// A value of PromptEstablishmentCode.
    /// </summary>
    [JsonPropertyName("promptEstablishmentCode")]
    public Common PromptEstablishmentCode
    {
      get => promptEstablishmentCode ??= new();
      set => promptEstablishmentCode = value;
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
    /// A value of HiddenNextTranInfo.
    /// </summary>
    [JsonPropertyName("hiddenNextTranInfo")]
    public NextTranInfo HiddenNextTranInfo
    {
      get => hiddenNextTranInfo ??= new();
      set => hiddenNextTranInfo = value;
    }

    /// <summary>
    /// A value of OspEstabOffice.
    /// </summary>
    [JsonPropertyName("ospEstabOffice")]
    public Office OspEstabOffice
    {
      get => ospEstabOffice ??= new();
      set => ospEstabOffice = value;
    }

    /// <summary>
    /// A value of OspEstabOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEstabOfficeServiceProvider")]
    public OfficeServiceProvider OspEstabOfficeServiceProvider
    {
      get => ospEstabOfficeServiceProvider ??= new();
      set => ospEstabOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEstabServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEstabServiceProvider")]
    public ServiceProvider OspEstabServiceProvider
    {
      get => ospEstabServiceProvider ??= new();
      set => ospEstabServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEnforcingOffice.
    /// </summary>
    [JsonPropertyName("ospEnforcingOffice")]
    public Office OspEnforcingOffice
    {
      get => ospEnforcingOffice ??= new();
      set => ospEnforcingOffice = value;
    }

    /// <summary>
    /// A value of OspEnforcingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingOfficeServiceProvider")]
    public OfficeServiceProvider OspEnforcingOfficeServiceProvider
    {
      get => ospEnforcingOfficeServiceProvider ??= new();
      set => ospEnforcingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEnforcingServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingServiceProvider")]
    public ServiceProvider OspEnforcingServiceProvider
    {
      get => ospEnforcingServiceProvider ??= new();
      set => ospEnforcingServiceProvider = value;
    }

    /// <summary>
    /// A value of AltBillingLocn.
    /// </summary>
    [JsonPropertyName("altBillingLocn")]
    public CsePersonsWorkSet AltBillingLocn
    {
      get => altBillingLocn ??= new();
      set => altBillingLocn = value;
    }

    /// <summary>
    /// A value of PromptRespCountry.
    /// </summary>
    [JsonPropertyName("promptRespCountry")]
    public Common PromptRespCountry
    {
      get => promptRespCountry ??= new();
      set => promptRespCountry = value;
    }

    /// <summary>
    /// A value of PromptInitCountry.
    /// </summary>
    [JsonPropertyName("promptInitCountry")]
    public Common PromptInitCountry
    {
      get => promptInitCountry ??= new();
      set => promptInitCountry = value;
    }

    private Common generateStandardNumber;
    private Common phase;
    private Tribunal previousExecutionHTribunal;
    private LegalAction previousExecutionHLegalAction;
    private Common promptListAltAddrLoc;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common promptListActionsTaken;
    private Standard standard;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Fips fips;
    private Fips hiddenFips;
    private Tribunal tribunal;
    private Tribunal hiddenTribunal;
    private LegalAction legalAction;
    private LegalAction hiddenLegalAction;
    private Common promptClass;
    private Common promptTribunal;
    private Common promptPmtLocation;
    private Common promptOrderAuth;
    private Common promptType;
    private Common promptInitState;
    private Common promptRespState;
    private Common promptDismissalCode;
    private Common promptEstablishmentCode;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
    private NextTranInfo hiddenNextTranInfo;
    private Office ospEstabOffice;
    private OfficeServiceProvider ospEstabOfficeServiceProvider;
    private ServiceProvider ospEstabServiceProvider;
    private Office ospEnforcingOffice;
    private OfficeServiceProvider ospEnforcingOfficeServiceProvider;
    private ServiceProvider ospEnforcingServiceProvider;
    private CsePersonsWorkSet altBillingLocn;
    private Common promptRespCountry;
    private Common promptInitCountry;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of GenerateStandardNumber.
    /// </summary>
    [JsonPropertyName("generateStandardNumber")]
    public Common GenerateStandardNumber
    {
      get => generateStandardNumber ??= new();
      set => generateStandardNumber = value;
    }

    /// <summary>
    /// A value of Phase.
    /// </summary>
    [JsonPropertyName("phase")]
    public Common Phase
    {
      get => phase ??= new();
      set => phase = value;
    }

    /// <summary>
    /// A value of PreviousExecutionHTribunal.
    /// </summary>
    [JsonPropertyName("previousExecutionHTribunal")]
    public Tribunal PreviousExecutionHTribunal
    {
      get => previousExecutionHTribunal ??= new();
      set => previousExecutionHTribunal = value;
    }

    /// <summary>
    /// A value of PreviousExecutionHLegalAction.
    /// </summary>
    [JsonPropertyName("previousExecutionHLegalAction")]
    public LegalAction PreviousExecutionHLegalAction
    {
      get => previousExecutionHLegalAction ??= new();
      set => previousExecutionHLegalAction = value;
    }

    /// <summary>
    /// A value of PromptListAltAddrLoc.
    /// </summary>
    [JsonPropertyName("promptListAltAddrLoc")]
    public Common PromptListAltAddrLoc
    {
      get => promptListAltAddrLoc ??= new();
      set => promptListAltAddrLoc = value;
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
    /// A value of PromptListActionsTaken.
    /// </summary>
    [JsonPropertyName("promptListActionsTaken")]
    public Common PromptListActionsTaken
    {
      get => promptListActionsTaken ??= new();
      set => promptListActionsTaken = value;
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
    /// A value of HiddenFips.
    /// </summary>
    [JsonPropertyName("hiddenFips")]
    public Fips HiddenFips
    {
      get => hiddenFips ??= new();
      set => hiddenFips = value;
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
    /// A value of HiddenTribunal.
    /// </summary>
    [JsonPropertyName("hiddenTribunal")]
    public Tribunal HiddenTribunal
    {
      get => hiddenTribunal ??= new();
      set => hiddenTribunal = value;
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
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of PromptTribunal.
    /// </summary>
    [JsonPropertyName("promptTribunal")]
    public Common PromptTribunal
    {
      get => promptTribunal ??= new();
      set => promptTribunal = value;
    }

    /// <summary>
    /// A value of PromptPmtLocation.
    /// </summary>
    [JsonPropertyName("promptPmtLocation")]
    public Common PromptPmtLocation
    {
      get => promptPmtLocation ??= new();
      set => promptPmtLocation = value;
    }

    /// <summary>
    /// A value of PromptOrderAuth.
    /// </summary>
    [JsonPropertyName("promptOrderAuth")]
    public Common PromptOrderAuth
    {
      get => promptOrderAuth ??= new();
      set => promptOrderAuth = value;
    }

    /// <summary>
    /// A value of PromptType.
    /// </summary>
    [JsonPropertyName("promptType")]
    public Common PromptType
    {
      get => promptType ??= new();
      set => promptType = value;
    }

    /// <summary>
    /// A value of PromptInitState.
    /// </summary>
    [JsonPropertyName("promptInitState")]
    public Common PromptInitState
    {
      get => promptInitState ??= new();
      set => promptInitState = value;
    }

    /// <summary>
    /// A value of PromptRespState.
    /// </summary>
    [JsonPropertyName("promptRespState")]
    public Common PromptRespState
    {
      get => promptRespState ??= new();
      set => promptRespState = value;
    }

    /// <summary>
    /// A value of PromptDismissalCode.
    /// </summary>
    [JsonPropertyName("promptDismissalCode")]
    public Common PromptDismissalCode
    {
      get => promptDismissalCode ??= new();
      set => promptDismissalCode = value;
    }

    /// <summary>
    /// A value of PromptEstablishmentCode.
    /// </summary>
    [JsonPropertyName("promptEstablishmentCode")]
    public Common PromptEstablishmentCode
    {
      get => promptEstablishmentCode ??= new();
      set => promptEstablishmentCode = value;
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
    /// A value of OspEstabOffice.
    /// </summary>
    [JsonPropertyName("ospEstabOffice")]
    public Office OspEstabOffice
    {
      get => ospEstabOffice ??= new();
      set => ospEstabOffice = value;
    }

    /// <summary>
    /// A value of OspEstabOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEstabOfficeServiceProvider")]
    public OfficeServiceProvider OspEstabOfficeServiceProvider
    {
      get => ospEstabOfficeServiceProvider ??= new();
      set => ospEstabOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEstabServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEstabServiceProvider")]
    public ServiceProvider OspEstabServiceProvider
    {
      get => ospEstabServiceProvider ??= new();
      set => ospEstabServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEnforcingOffice.
    /// </summary>
    [JsonPropertyName("ospEnforcingOffice")]
    public Office OspEnforcingOffice
    {
      get => ospEnforcingOffice ??= new();
      set => ospEnforcingOffice = value;
    }

    /// <summary>
    /// A value of OspEnforcingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingOfficeServiceProvider")]
    public OfficeServiceProvider OspEnforcingOfficeServiceProvider
    {
      get => ospEnforcingOfficeServiceProvider ??= new();
      set => ospEnforcingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OspEnforcingServiceProvider.
    /// </summary>
    [JsonPropertyName("ospEnforcingServiceProvider")]
    public ServiceProvider OspEnforcingServiceProvider
    {
      get => ospEnforcingServiceProvider ??= new();
      set => ospEnforcingServiceProvider = value;
    }

    /// <summary>
    /// A value of AltBillingLocn.
    /// </summary>
    [JsonPropertyName("altBillingLocn")]
    public CsePersonsWorkSet AltBillingLocn
    {
      get => altBillingLocn ??= new();
      set => altBillingLocn = value;
    }

    /// <summary>
    /// A value of PromptRespCountry.
    /// </summary>
    [JsonPropertyName("promptRespCountry")]
    public Common PromptRespCountry
    {
      get => promptRespCountry ??= new();
      set => promptRespCountry = value;
    }

    /// <summary>
    /// A value of PromptInitCountry.
    /// </summary>
    [JsonPropertyName("promptInitCountry")]
    public Common PromptInitCountry
    {
      get => promptInitCountry ??= new();
      set => promptInitCountry = value;
    }

    private Common generateStandardNumber;
    private Common phase;
    private Tribunal previousExecutionHTribunal;
    private LegalAction previousExecutionHLegalAction;
    private Common promptListAltAddrLoc;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common promptListActionsTaken;
    private Standard standard;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private Fips fips;
    private Fips hiddenFips;
    private Tribunal tribunal;
    private Tribunal hiddenTribunal;
    private LegalAction legalAction;
    private LegalAction hiddenLegalAction;
    private Common promptClass;
    private Common promptTribunal;
    private Common promptPmtLocation;
    private Common promptOrderAuth;
    private Common promptType;
    private Common promptInitState;
    private Common promptRespState;
    private Common promptDismissalCode;
    private Common promptEstablishmentCode;
    private NextTranInfo hiddenNextTranInfo;
    private Office ospEstabOffice;
    private OfficeServiceProvider ospEstabOfficeServiceProvider;
    private ServiceProvider ospEstabServiceProvider;
    private Office ospEnforcingOffice;
    private OfficeServiceProvider ospEnforcingOfficeServiceProvider;
    private ServiceProvider ospEnforcingServiceProvider;
    private CsePersonsWorkSet altBillingLocn;
    private Common promptRespCountry;
    private Common promptInitCountry;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NoBClassUpdated.
    /// </summary>
    [JsonPropertyName("noBClassUpdated")]
    public Common NoBClassUpdated
    {
      get => noBClassUpdated ??= new();
      set => noBClassUpdated = value;
    }

    /// <summary>
    /// A value of NullTribunal.
    /// </summary>
    [JsonPropertyName("nullTribunal")]
    public Tribunal NullTribunal
    {
      get => nullTribunal ??= new();
      set => nullTribunal = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    /// <summary>
    /// A value of NumberOfNewCollections.
    /// </summary>
    [JsonPropertyName("numberOfNewCollections")]
    public Common NumberOfNewCollections
    {
      get => numberOfNewCollections ??= new();
      set => numberOfNewCollections = value;
    }

    /// <summary>
    /// A value of NumberOfOldCollections.
    /// </summary>
    [JsonPropertyName("numberOfOldCollections")]
    public Common NumberOfOldCollections
    {
      get => numberOfOldCollections ??= new();
      set => numberOfOldCollections = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of MultTribError.
    /// </summary>
    [JsonPropertyName("multTribError")]
    public Common MultTribError
    {
      get => multTribError ??= new();
      set => multTribError = value;
    }

    /// <summary>
    /// A value of TribalTribunal.
    /// </summary>
    [JsonPropertyName("tribalTribunal")]
    public Common TribalTribunal
    {
      get => tribalTribunal ??= new();
      set => tribalTribunal = value;
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
    /// A value of TotalPromptsSelected.
    /// </summary>
    [JsonPropertyName("totalPromptsSelected")]
    public Common TotalPromptsSelected
    {
      get => totalPromptsSelected ??= new();
      set => totalPromptsSelected = value;
    }

    /// <summary>
    /// A value of LastChar.
    /// </summary>
    [JsonPropertyName("lastChar")]
    public AaWork LastChar
    {
      get => lastChar ??= new();
      set => lastChar = value;
    }

    private Common noBClassUpdated;
    private Tribunal nullTribunal;
    private LegalAction nullLegalAction;
    private Common numberOfNewCollections;
    private Common numberOfOldCollections;
    private Common common;
    private Common multTribError;
    private Common tribalTribunal;
    private DateWorkArea currentDate;
    private DateWorkArea max;
    private Code code;
    private CodeValue codeValue;
    private Common totalPromptsSelected;
    private AaWork lastChar;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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

    private Collection collection;
    private Tribunal tribunal;
    private LegalAction legalAction;
  }
#endregion
}

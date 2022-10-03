// Program: LE_LACT_LEGAL_ACTION, ID: 371984462, model: 746.
// Short name: SWELACTP
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
/// A program: LE_LACT_LEGAL_ACTION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class LeLactLegalAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LACT_LEGAL_ACTION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLactLegalAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLactLegalAction.
  /// </summary>
  public LeLactLegalAction(IContext context, Import import, Export export):
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
    // 			C H A N G E    L O G
    // ---------------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // 04/27/95  Dave Allen			Initial Code
    // 12/02/95  Dave Verrett			Call to task Engine
    // 					Removed references to Legal Action Office Service Provider
    // 					Added Dialog flow to APTK - Assign Plan Task
    // 					Added Print Logic
    // 12/22/95  Tom Redmond              	Add Petitioner and Respondent
    // 01/31/96  A. Hackler               	Change code name from state to state 
    // code, change exit state
    // 					from return_to_caller  to return
    // 02/16/96  Siraj K                  	Added flow to Document Maintenance 
    // from prompt on Action Taken.
    // 05/31/96  Henry Hooks			Added logic to derive standard number from court 
    // case number.
    // 09/25/96  G. Lofton			Replaced old security with the new test security 
    // cab.
    // 02/19/97  Govind	IDCR 291	Court case number optional for Class "F".
    // 07/16/97  Randy M	PR H00024909	Make "Legal Action Est. By" mandatory.
    // 09/08/97  Alan Samuels	PR 27862
    // 09/08/97  Alan Samuels	IDCR 376
    // 10/07/97  R Grey	H00029847	Add Msg
    // 10/07/97  R Grey	H00029622	Add Le Act
    // 10/20/97  R Grey	H00028636	Std Numb for P class updt
    // 11/19/97  R Grey	H00032620	Redisplay following update
    // 11/19/97  R Grey	H00032603	Don't allow update of action taken on existing
    // leact
    // 11/19/97  R Grey	H00032603	Non CSE Petitioner displays instead of 
    // petitioner.
    // 10/09/98  M Ramirez	 		Modified Print function
    // 12/07/98  P. Sharp			Cleaned up numerous issues per Phase II assessment.
    // 					Cleaned up prompt logic. Fixed problem report 48247,
    // 					40932, 40513. IDCR 338, 442.
    // 02/05/99  P. Sharp			Added logic to determine if the alt bill addr added 
    // is same
    // 					as alt bill addr on orbligation
    // 03/09/99  PMcElderry			Logic for monitored activities, Event 95.
    // 04/08/99  PMcElderry	400		Made "Filed Date" field nonenterable on adds.
    // 05/17/99  PMcElderry			Added CSEnet logic.
    // 07/19/99  Anand Katuri			Edits for Court Case Number. Do not allow any
    // 					alphabet as the first character if the tribunal state is KS.
    // 09/09/99  R. Jean	PR#H73001	Allow classifications M, O, and E to be 
    // entered without court
    // 					case numbers like P and F are.
    // 10/19/99  R. Jean	PR#H73506	Do not copy service provider assigned to 'N' 
    // classification legal
    // 					actions to subsequent legal actions.
    // 10/28/99  R. Jean	PR#H78286	Allow classifications U to be entered without
    // court case
    // 					numbers like P, F, M, O, E are.
    // 11/03/99  R. Jean	PR78746		Allow Class U legal actions to enter alternate
    // pay location
    // 12/13/99  R. Jean	PR81361		Flow to ORGZ instead of NAME
    // 12/13/99  R. Jean	PR81732		Don't allow embedded spaces in court case 
    // number
    // 12/13/99  R. Jean	PR80134		Allow tribal tribunals to enter court case 
    // numbers and standard
    // 					numbers any form
    // 12/13/99  R. Jean	PR81826		Validate the low order position of court case 
    // number is the
    // 					correct letter (indicating city) for counties of Kansas with
    // 					multiple tribunals.
    // 01/11/00  R. Jean	PR82883		Add edit for update function that won't allow 
    // to update a
    // 					legal action so that two legal action with the same
    // 					CC#, FIPS, Tribunal, Filed Date, Action Taken.
    // 01/12/00  M Ramirez	PR# 83300	Clear NEXT TRAN before invoking Print 
    // Process
    // 01/12/00  M Ramirez	PR# xxxxx	Change exitstate when lact is not related 
    // to a document
    // 01/18/00  R Jean	PR81987		Flow to procedure LACC in order to change court
    // case
    // 					number and or standard number.
    // 02/01/00  R Jean	PR82464		Prepopulate pay to fields if counties are DG, 
    // JO, CR,
    // 					NO, MP, HV
    // 03/28/00  R Jean	PR91341		Prevent any standard number greater than 12 
    // positions
    // 08/24/00  GVandy	PR101721	Modify so that court case number edits are only
    // performed
    // 					on Add and Update.
    // 09/11/00  GVandy	PR102972	Do not allow tribunal update.
    // 09/21/00  GVandy	WR000209	Access to LACC is disabled until business rules
    // for
    // 					updates to tribunal, court case number, and standard
    // 					number are defined.
    // 09/22/00  GVandy	PR102557	Tribal courts are now identified by FIPS 
    // location codes 20 through 99.
    // 10/03/00  GVandy	WR 000210	New rules for building standard numbers.
    // 11/08/00  GVandy	PR92039		Read and display the country for foreign 
    // tribunals.
    // 			PR106926	Make the standard number Green Underscore on confirmation.
    // 11/20/00  GVandy	WR000209	Changes to LACC are complete.  Re-enable the 
    // dialog flow to LACC.
    // 01/05/01  GVandy	WR#000264	Allow user to enter the state and county 
    // abbreviations
    // 					rather than forcing them to pick the tribunal on LTRB.
    // 04/02/01  GVandy	PR115974	Build standard numbers on updates of F, M, O, 
    // E, and
    // 					U class legal actions.
    // 04/02/01  GVandy	PR108247	Prevent adding legal actions with standard 
    // numbers
    // 					different from previous legal actions for the court case
    // 					number. (standard number validation cab change).  Also
    // 					remove call to cab updating standard numbers on previous
    // 					legal action to match the new legal action standard number.
    // 04/02/01  GVandy	PR115954	Prevent duplicate standard numbers on multiple 
    // court case
    // 					numbers (standard number validation cab change)
    // 07/01/02  GVandy	WR020338/	Dialog flow changes during additon of legal 
    // actions
    // 			WR020339	to enforce new LROL and CPAT requirements.
    // 07/01/02  GVandy	PR143437	Use cab le_cab_get_action_taken_description 
    // instead of
    // 					cab_validate_code_value to retrieve action taken descriptions.
    // 09/24/02  GVandy	PR132101	Allow S Class legal actions to be created 
    // without court
    // 					order numbers.
    // 10/22/02  GVandy	PR159443	Change standard number confirmation messages to
    // warnings.
    // 10/22/02  GVandy	WR020322	1. Default payment location to PAYCTR on Adds.
    // 					2. Remove Pay To default to Court Trustee for Douglass, Johnson,
    // 					   Crawford, Harvey, Neosho, and McPherson counties.
    // 12/23/02  GVandy	WR10492		Display new attribute legal_action 
    // system_gen_ind.
    // 03/04/03  GVandy	PR131100	Don't allow key information to be changed on a 
    // PRINT.
    // 03/04/03  GVandy	PR141366	Correct error causing assigned attorneys to not
    // display
    // 					when returning from printing documents.
    // 03/26/03  GVandy	PR136420	Do not require a caption to be entered for U 
    // class actions.
    // 03/26/03  GVandy	PR155936	Do not process CSENet transactions for VOLPATTJ
    // actions.
    // 03/26/03  GVandy	PR155938	Correct CSENet processing for REGMODNJ and 
    // REGENFNJ actions.
    // 05/06/03  GVandy	PR126879	Generate standard numbers for foreign court 
    // orders.
    // ??/??/??  DDupree	PR258198	??????
    // 02/17/06  GVandy	PR205434	Don't allow user to change key info before PF18
    // (ASIN).
    // 06/21/07  G. Pan       PR308058         Link to DISC when PF5 add class D
    // 09/20/07  G. Pan       CQ425		Added logic after the action block
    // 					le_cab_get_class_for_act_taken to validate
    // 					not allow J classification with court case
    // 					number = space.  The logic added is to fix the problem -
    // 					If the user is adding the legal action and starts out on
    // 					LACT with an existing court case number with J
    // 					classification, then changes the J class to P (or any other
    // 					classification that can be added with no court order
    // 					number), when the final add is done, the LACT screen
    // 					process changes the P back to the J. This problem creates
    // 					many J classification records with no court order number.
    // 10/28/10  GVandy	CQ109		Eliminate display of EST legal action service 
    // provider.
    // 03/10/11  GVandy	CQ25750		When adding new legal actions don't create 
    // events if no
    // 					service provider was copied from the previous legal action.
    // 06/09/15  GVandy	CQ22212		Changes to support electronic Income 
    // Withholding (e-IWO).
    // 10/27/16  AHockman      cq41786         changes needed for lump sum iwo 
    // docs
    // 4/25/17  JHarden        CQ55817         Prevent staff from adding the 
    // standard court
    // 					order number on LACT.
    // 05/10/17  GVandy	CQ48108		Changes to support IV-D PEP.
    // 07/27/17  GVandy	CQ58707		Modify IV-D PEP edit for filed date to apply
    // 					only to J class legal actions.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    local.CurrentDate.Date = Now().Date;
    local.CurrentDate.Timestamp = Now();

    if (Equal(global.Command, "CLEAR"))
    {
      export.PrintFxKey.Text8 = "24 Print";

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports.
    // ---------------------------------------------
    export.PetitionerRespondentDetails.
      Assign(import.PetitionerRespondentDetails);
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
    export.PromptClass.SelectChar = import.PromptClass.SelectChar;
    export.PromptTribunal.SelectChar = import.PromptTribunal.SelectChar;
    export.PromptPmtLocation.SelectChar = import.PromptPmtLocation.SelectChar;
    export.PromptOrderAuth.SelectChar = import.PromptOrderAuth.SelectChar;
    export.PromptType.SelectChar = import.PromptType.SelectChar;
    export.PromptEstablishmentCode.SelectChar =
      import.PromptEstablishmentCode.SelectChar;
    export.PromptInitState.SelectChar = import.PromptInitState.SelectChar;
    export.PromptRespState.SelectChar = import.PromptRespState.SelectChar;
    export.PromptDismissalCode.SelectChar =
      import.PromptDismissalCode.SelectChar;
    export.PromptListActionsTaken.SelectChar =
      import.PromptListActionsTaken.SelectChar;
    export.PromptListAltAddrLoc.SelectChar =
      import.PromptListAltAddrLoc.SelectChar;
    MoveCsePersonsWorkSet(import.AltBillingLocn, export.AltBillingLocn);
    export.PromptRespCountry.SelectChar = import.PromptRespCountry.SelectChar;
    export.PromptInitCountry.SelectChar = import.PromptInitCountry.SelectChar;
    export.Code.CodeName = import.Code.CodeName;
    MoveDocument(import.Document, export.Document);
    export.FromLrolCase.Number = import.FromLrolCase.Number;
    export.FromLrolCaseRole.Type1 = import.FromLrolCaseRole.Type1;
    export.FromLrolCsePerson.Number = import.FromLrolCsePerson.Number;
    export.PrintFxKey.Text8 = import.PrintFxKey.Text8;

    if (Equal(export.LegalAction.ActionTaken, "IWO") || Equal
      (export.LegalAction.ActionTaken, "IWOMODO") || Equal
      (export.LegalAction.ActionTaken, "IWOTERM") || Equal
      (export.LegalAction.ActionTaken, "ORDIWO2") || Equal
      (export.LegalAction.ActionTaken, "ORDIWOLS") || Equal
      (export.LegalAction.ActionTaken, "ORDIWOPT"))
    {
      export.PrintFxKey.Text8 = "22 LAIS";
    }
    else
    {
      export.PrintFxKey.Text8 = "24 Print";
    }

    export.EiwoSelection.Flag = "Y";

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export.
    // ---------------------------------------------
    export.HiddenLegalAction.Assign(import.HiddenLegalAction);
    export.HiddenTribunal.Assign(import.HiddenTribunal);
    export.HiddenFips.Assign(import.HiddenFips);
    export.HiddenPrevUserAction.Command = global.Command;
    local.ReturnedFromCaptAftAdd.Flag = "N";
    local.ReturnedFromDiscAftAdd.Flag = "N";

    // ****************************************************************
    // 12/13/99	R. Jean
    // PR80134 - Allow tribal tribunals to enter court case numbers and standard
    // numbers any form.
    // ****************************************************************
    // ****************************************************************
    // 09/22/00	GVandy
    // PR102557 - Tribal courts are now identified by FIPS location codes 20 
    // through 99.
    // ****************************************************************
    if (export.Fips.Location >= 20 && export.Fips.Location <= 99)
    {
      local.TribalTribunal.Flag = "Y";
    }

    if (Equal(global.Command, "FROMMENU"))
    {
      return;
    }

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

      global.Command = "REDISP";
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
      UseScCabNextTranPut1();

      return;
    }

    // ***********************************************************************
    // * 01/18/00	R Jean
    // PR81987 - Flow to procedure LACC in order to change court case number and
    // or standard number.  Add test of cmd CHGCC
    // ***********************************************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "CHGCC"))
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
    //             E D I T    L O G I C
    // ---------------------------------------------
    // ---------------------------------------------
    // Edit required fields.
    // ---------------------------------------------
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ***********************************************************************
      // 8/24/00 	GVandy
      // PR101721 -- The following edits were preventing the user from flowing 
      // to LACC to correct a bad court case number.  Modify so that the edits
      // are only executed on an add or update.
      // ***********************************************************************
      if (Equal(global.Command, "UPDATE"))
      {
        // ***********************************************************************
        // 9/11/00 	GVandy
        // PR102972 - Do not allow tribunal update.
        // ***********************************************************************
        if (export.Tribunal.Identifier != export.HiddenTribunal.Identifier || !
          Equal(export.Fips.StateAbbreviation,
          export.HiddenFips.StateAbbreviation) || !
          Equal(export.Fips.CountyAbbreviation,
          export.HiddenFips.CountyAbbreviation))
        {
          var field1 = GetField(export.Fips, "countyAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "stateAbbreviation");

          field2.Error = true;

          ExitState = "LE0000_TRIB_CHANGE_NOT_ALLOWED";

          return;
        }
      }

      if (Equal(global.Command, "ADD"))
      {
        if (Equal(import.HiddenPrevUserAction.Command, "LTRB"))
        {
          // -- On the last execution, we sent the user to LTRB to select a 
          // unique tribunal based on the entered state and county.  Now
          // determine if they selected a tribunal from LTRB.
          if (import.DlgflwSelectedTribunal.Identifier > 0)
          {
            export.Fips.Assign(import.DlgflwSelectedFips);
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);
            export.LegalAction.ForeignFipsState = export.Fips.State;
            export.LegalAction.ForeignFipsCounty = export.Fips.County;
            export.LegalAction.ForeignFipsLocation = export.Fips.Location;
          }
          else
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Fips, "countyAbbreviation");

            field2.Error = true;

            ExitState = "LE0000_MUST_SELECT_TRIBUNAL";
          }
        }
        else
        {
          // ***********************************************************************
          // 01/05/01 GVandy WR#264 - Allow user to enter the state and county
          // abbreviations rather than forcing them to pick the tribunal on 
          // LTRB.
          // ***********************************************************************
          if (!IsEmpty(export.Fips.StateAbbreviation) && !
            IsEmpty(export.Fips.CountyAbbreviation))
          {
            local.NumberOfTribunalsFound.Count = 0;

            foreach(var item in ReadTribunalFips())
            {
              if (entities.Fips.Location != 12)
              {
                // -- Ignore court trustees (fips location code 12) when 
                // determining how many tribunals match the entered state and
                // county.
                ++local.NumberOfTribunalsFound.Count;
                local.Tribunal.Assign(entities.Tribunal);
                local.Fips.Assign(entities.Fips);
              }

              if (entities.Tribunal.Identifier == export.Tribunal.Identifier)
              {
                // -- If the export tribunal id is populated then use the 
                // tribunal that matches that id.  For example, the user is
                // adding over an existing legal action.  The tribunal id would
                // already be populated from that legal action.  The new legal
                // action will be added under the same tribunal.
                // -- If the export tribunal id is populated then use the 
                // tribunal that matches that id.  For example, we send the user
                // to LTRB where they pick the tribunal that they want.  When
                // we return we go through this logic again and we don't want to
                // send them back to LTRB a second (and third...) time.
                local.NumberOfTribunalsFound.Count = 1;
                local.Tribunal.Assign(entities.Tribunal);
                local.Fips.Assign(entities.Fips);

                break;
              }
            }

            switch(local.NumberOfTribunalsFound.Count)
            {
              case 0:
                export.Tribunal.Assign(local.NullTribunal);

                if (ReadFips())
                {
                  export.Fips.Assign(entities.Fips);

                  var field1 = GetField(export.Fips, "stateAbbreviation");

                  field1.Error = true;

                  var field2 = GetField(export.Fips, "countyAbbreviation");

                  field2.Error = true;

                  ExitState = "LE0000_TRIBUNAL_NF";
                }
                else
                {
                  export.Fips.CountyDescription = "";
                  export.Fips.County = 0;
                  export.Fips.Location = 0;
                  export.Fips.State = 0;

                  var field1 = GetField(export.Fips, "stateAbbreviation");

                  field1.Error = true;

                  var field2 = GetField(export.Fips, "countyAbbreviation");

                  field2.Error = true;

                  ExitState = "SI0000_TRIB_FIPS_NF_RB";
                }

                break;
              case 1:
                export.Tribunal.Assign(local.Tribunal);
                export.Fips.Assign(local.Fips);
                export.FipsTribAddress.Country = "";

                break;
              default:
                export.Fips.Assign(local.Fips);
                export.Tribunal.Assign(local.NullTribunal);
                export.HiddenPrevUserAction.Command = "LTRB";
                ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";

                return;
            }
          }
        }
      }

      // *** A Kansas Court Case Number cannot start with an alphabet Anand 7/19
      // /99
      // ****************************************************************
      // 12/13/99	R. Jean
      // PR80134 - Allow tribal tribunals to enter court case numbers and 
      // standard numbers any form.  Bypass edit if Tribal tribunal.
      // 08/21/01 GVandy.  WR 10346 - Bypass the edit for Bankruptcy legal 
      // actions.
      // ****************************************************************
      if (!IsEmpty(export.LegalAction.CourtCaseNumber) && Equal
        (export.Fips.StateAbbreviation, "KS") && AsChar
        (local.TribalTribunal.Flag) != 'Y' && AsChar
        (export.LegalAction.Classification) != 'B' && export
        .Tribunal.Identifier != 0)
      {
        if (Verify(Substring(export.LegalAction.CourtCaseNumber, 17, 1, 1),
          "0123456789") == 0)
        {
        }
        else
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_KS_CRT_CASE_NUM_NO_ALPHA";

          return;
        }

        if (Verify(Substring(export.LegalAction.CourtCaseNumber, 17, 1, 2),
          "0123456789") > 0)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_CT_CASE_NO_1ST_2_BE_YEAR";

          return;
        }
      }

      // ****************************************************************
      // 12/13/99	R. Jean
      // PR81826 - Validate the low order position of court case number is the 
      // correct letter (indicating city) for counties of Kansas with multiple
      // tribunals.
      // ****************************************************************
      if (!IsEmpty(export.LegalAction.CourtCaseNumber) && Equal
        (export.Fips.StateAbbreviation, "KS") && export.Tribunal.Identifier != 0
        )
      {
        // *** Get last CHAR of Tribunal court case.
        local.LastChar.TextLength01 =
          Substring(export.LegalAction.CourtCaseNumber,
          Length(TrimEnd(export.LegalAction.CourtCaseNumber)), 1);

        if (Verify(local.LastChar.TextLength01, "0123456789") == 0)
        {
          // *** Numeric last character is invalid for the following tribunals.
          local.MultTribError.Flag = "Y";

          switch(export.Tribunal.Identifier)
          {
            case 669:
              break;
            case 670:
              break;
            case 671:
              break;
            case 672:
              break;
            case 703:
              break;
            case 704:
              break;
            case 717:
              break;
            case 718:
              break;
            case 722:
              break;
            case 723:
              break;
            default:
              local.MultTribError.Flag = "";

              break;
          }

          if (AsChar(local.MultTribError.Flag) == 'Y')
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LE0000_MULT_TRIB_CC_LST_POS_NBR";

            return;
          }
        }
        else
        {
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

            ExitState = "LE0000_MULT_TRIB_CC_NOT_MATCH_2";

            return;
          }
        }
      }

      // ****************************************************************
      // 12/13/99	R. Jean
      // PR81732 - Don't allow embedded spaces in court case number
      // ****************************************************************
      if (export.Tribunal.Identifier != 0 && !
        IsEmpty(export.LegalAction.CourtCaseNumber))
      {
        if (Find(TrimEnd(export.LegalAction.CourtCaseNumber), " ") > 0)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_NO_EMBED_SPACE_ALLOWED";

          return;
        }
      }
    }

    if (IsEmpty(export.LegalAction.CourtCaseNumber) && IsEmpty
      (export.LegalAction.ForeignOrderNumber))
    {
      if (export.LegalAction.Identifier == 0)
      {
        if (Equal(global.Command, "DISPLAY"))
        {
          ExitState = "ECO_LNK_TO_LST_LGL_ACT_BY_CP";

          return;
        }
      }

      // *********************************************
      // *          03-29-96  H HOOKS                *
      // * Court Case Number can be spaces if        *
      // * Classification is 'P' or 'F'.         *
      // * 9/9/99 R. Jean
      // * Added classes M, O, E
      // * 10/28/99 R.Jean
      // * Add class U
      // *********************************************
      if (AsChar(export.LegalAction.Classification) == 'P' || AsChar
        (export.LegalAction.Classification) == 'F' || AsChar
        (export.LegalAction.Classification) == 'M' || AsChar
        (export.LegalAction.Classification) == 'O' || AsChar
        (export.LegalAction.Classification) == 'E' || AsChar
        (export.LegalAction.Classification) == 'U' || AsChar
        (export.LegalAction.Classification) == 'S')
      {
      }
      else if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY"))
    {
      if (!IsEmpty(export.LegalAction.Classification))
      {
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = export.LegalAction.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";

          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          return;
        }
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE"))
    {
      if (IsEmpty(export.Fips.StateAbbreviation) && IsEmpty
        (export.FipsTribAddress.Country))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "COUNTRY_OR_STATE_CODE_REQD";
        }
      }

      if (!IsEmpty(export.Fips.StateAbbreviation) && !
        IsEmpty(export.FipsTribAddress.Country))
      {
        var field1 = GetField(export.FipsTribAddress, "country");

        field1.Error = true;

        var field2 = GetField(export.Fips, "stateAbbreviation");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "EITHER_STATE_OR_CNTRY_CODE";
        }
      }

      if (!IsEmpty(export.Fips.StateAbbreviation))
      {
        if (IsEmpty(export.Fips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (export.Tribunal.Identifier == 0)
      {
        var field1 = GetField(export.Fips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "countyAbbreviation");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_TRIBUNAL_MUST_BE_SELECTED";
        }
      }

      // ****************************************************************
      // * PR82464 RJEAN Court Trustee tribunals are not allowed
      // * for association to a legal action
      // ****************************************************************
      if (export.Fips.Location == 12)
      {
        var field1 = GetField(export.Fips, "stateAbbreviation");

        field1.Error = true;

        var field2 = GetField(export.Fips, "countyAbbreviation");

        field2.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Tribunal.Identifier = 0;
          ExitState = "LE0000_TRIBUNAL_NOT_ALLOWED";
        }
      }

      if (IsEmpty(export.LegalAction.Classification))
      {
        var field = GetField(export.LegalAction, "classification");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      if (IsEmpty(export.LegalAction.ActionTaken))
      {
        var field = GetField(export.PromptListActionsTaken, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_LACT_ACT_TAKEN_REQD";
        }
      }

      if (!IsEmpty(export.LegalAction.ActionTaken))
      {
        if (IsEmpty(export.LegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_INVALID_LEG_ACT_CLASSIFN";
          }
        }
      }

      if (AsChar(export.LegalAction.Classification) == 'O' || AsChar
        (export.LegalAction.Classification) == 'J')
      {
        if (IsEmpty(export.LegalAction.EstablishmentCode))
        {
          var field = GetField(export.LegalAction, "establishmentCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }

      if (Lt(local.InitialisedToZeros.Date, export.LegalAction.FiledDate))
      {
        if (Lt(local.CurrentDate.Date, export.LegalAction.FiledDate))
        {
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_FILED_DATE_GT_CURR_DATE";
          }
        }
        else if (Equal(global.Command, "UPDATE"))
        {
          if (!Equal(export.HiddenLegalAction.FiledDate,
            export.LegalAction.FiledDate))
          {
            // 07/27/17 GVandy CQ58707 Modify IV-D PEP edit for filed date to 
            // apply only to J class legal actions.
            if (AsChar(export.HiddenLegalAction.Classification) != 'J')
            {
              goto Test1;
            }

            // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Disallow filed date if an
            // EP legal detail
            //    exists and multiple active AP case roles for any case tied to 
            // the legal action on
            //    LROL.
            foreach(var item in ReadLegalActionDetail2())
            {
              // --The read each below is set to distinct occurrences.
              foreach(var item1 in ReadCase())
              {
                ReadCaseRoleCsePerson();

                if (local.ApCommon.Count > 1)
                {
                  var field = GetField(export.LegalAction, "filedDate");

                  field.Error = true;

                  if (IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    ExitState = "LE0000_MULT_APS_EXIST";
                  }

                  break;
                }
              }
            }
          }
        }
      }

Test1:

      if (Lt(local.InitialisedToZeros.Date, export.LegalAction.EndDate))
      {
        if (Lt(export.LegalAction.EndDate, export.LegalAction.FiledDate))
        {
          var field = GetField(export.LegalAction, "endDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_END_DATE_LT_FILE_DATE";
          }
        }

        if (Equal(export.LegalAction.FiledDate, local.InitialisedToZeros.Date))
        {
          var field1 = GetField(export.LegalAction, "endDate");

          field1.Error = true;

          var field2 = GetField(export.LegalAction, "filedDate");

          field2.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_FILE_DATE_REQUIRED";
          }
        }
        else if (Equal(global.Command, "UPDATE"))
        {
          if (!Equal(export.HiddenLegalAction.EndDate,
            export.LegalAction.EndDate))
          {
            // -- 05/10/2017 GVandy CQ48108 (IV-D PEP) Disallow end dating if 
            // paternity is locked
            //    for any active child on an EP legal detail.
            foreach(var item in ReadLegalActionDetail2())
            {
              if (ReadCsePerson1())
              {
                var field = GetField(export.LegalAction, "endDate");

                field.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "LE0000_CANNOT_END_DT_PAT_LOCKED";
                }

                break;
              }
            }
          }
        }
      }

      if (!IsEmpty(export.LegalAction.DismissedWithoutPrejudiceInd) && IsEmpty
        (export.LegalAction.DismissalCode))
      {
        var field = GetField(export.LegalAction, "dismissalCode");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      if (!IsEmpty(export.LegalAction.DismissalCode) && IsEmpty
        (export.LegalAction.DismissedWithoutPrejudiceInd))
      {
        var field =
          GetField(export.LegalAction, "dismissedWithoutPrejudiceInd");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }
      }

      if (AsChar(export.LegalAction.DismissedWithoutPrejudiceInd) == 'Y')
      {
        if (Equal(export.LegalAction.RefileDate, null) || Equal
          (export.LegalAction.RefileDate, local.Max.Date))
        {
          var field = GetField(export.LegalAction, "refileDate");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "ADD"))
    {
      // -------------------------------------------------------------
      // Get the classification again because the user might have
      // changed it.
      // -------------------------------------------------------------
      UseLeCabGetClassForActTaken();

      if (IsEmpty(export.LegalAction.Classification))
      {
        ExitState = "LE0000_COULD_NOT_DETERMINE_CLASS";

        return;
      }
    }

    // *********************************************************************************
    // 09/20/07  G. Pan   PR190374
    //                    Validate to not allow classification = "J" and court 
    // case number = space.
    // *********************************************************************************
    if (IsEmpty(export.LegalAction.CourtCaseNumber))
    {
      if (AsChar(export.LegalAction.Classification) == 'P' || AsChar
        (export.LegalAction.Classification) == 'F' || AsChar
        (export.LegalAction.Classification) == 'M' || AsChar
        (export.LegalAction.Classification) == 'O' || AsChar
        (export.LegalAction.Classification) == 'E' || AsChar
        (export.LegalAction.Classification) == 'U' || AsChar
        (export.LegalAction.Classification) == 'S')
      {
      }
      else if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        var field = GetField(export.LegalAction, "courtCaseNumber");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
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

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
        }
      }

      // ---------------------------------------------
      // Validate Establishment Code.
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.EstablishmentCode))
      {
        local.Code.CodeName = "LEGAL ACTION ESTABLISHMENT";
        local.CodeValue.Cdvalue = export.LegalAction.EstablishmentCode ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "establishmentCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Order Authority.
      // ---------------------------------------------
      if (AsChar(export.LegalAction.Classification) == 'J')
      {
        if (IsEmpty(export.LegalAction.OrderAuthority))
        {
          var field = GetField(export.LegalAction, "orderAuthority");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_ORD_AUTH_REQD";
          }
        }
      }

      if (!IsEmpty(export.LegalAction.OrderAuthority))
      {
        local.Code.CodeName = "LEGAL ACTION ORDER AUTHORITY";
        local.CodeValue.Cdvalue = export.LegalAction.OrderAuthority;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "orderAuthority");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Type.
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.Type1))
      {
        local.Code.CodeName = "LEGAL ACTION TYPE";
        local.CodeValue.Cdvalue = export.LegalAction.Type1;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "type1");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate alternate bill locn person number
      // ---------------------------------------------
      if (!IsEmpty(export.AltBillingLocn.Number))
      {
        // ****************************************************************
        // * PR78746 RJEAN Allow Class U legal actions to enter alternate pay 
        // location
        // ****************************************************************
        if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
          (export.LegalAction.Classification) == 'U')
        {
          if (ReadCsePerson2())
          {
            UseSiReadCsePerson();
          }
          else
          {
            var field = GetField(export.AltBillingLocn, "number");

            field.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_ALT_BILL_LOCN_CSE_PERS_NF";
            }
          }
        }
        else
        {
          var field = GetField(export.AltBillingLocn, "number");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_ALT_PAY_TO_VALID_F_J_ONLY";
          }
        }
      }
      else
      {
        export.AltBillingLocn.FormattedName = "";
      }

      // ---------------------------------------------
      // Validate Alt addr Ct ordered or not
      // ---------------------------------------------
      switch(AsChar(export.LegalAction.CtOrdAltBillingAddrInd))
      {
        case 'Y':
          if (IsEmpty(export.AltBillingLocn.Number))
          {
            var field1 = GetField(export.LegalAction, "ctOrdAltBillingAddrInd");

            field1.Error = true;

            var field2 = GetField(export.PromptListAltAddrLoc, "selectChar");

            field2.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_CT_ORD_IND_MUST_BE_BLANK";
            }
          }

          break;
        case 'N':
          if (IsEmpty(export.AltBillingLocn.Number))
          {
            var field1 = GetField(export.LegalAction, "ctOrdAltBillingAddrInd");

            field1.Error = true;

            var field2 = GetField(export.PromptListAltAddrLoc, "selectChar");

            field2.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_CT_ORD_IND_MUST_BE_BLANK";
            }
          }

          break;
        case ' ':
          if (IsEmpty(export.AltBillingLocn.Number))
          {
          }
          else
          {
            var field1 = GetField(export.LegalAction, "ctOrdAltBillingAddrInd");

            field1.Error = true;

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "LE0000_ALT_BILL_ADDR_CT_ORD_REQD";
            }
          }

          break;
        default:
          var field = GetField(export.LegalAction, "ctOrdAltBillingAddrInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";
          }

          break;
      }

      // ---------------------------------------------
      // Default Last Modification review Date only for "ADD". For updates blank
      // is valid
      // ---------------------------------------------
      if (Equal(global.Command, "ADD"))
      {
        if (AsChar(export.LegalAction.Classification) == 'J')
        {
          if (Equal(export.Fips.StateAbbreviation, "KS"))
          {
            if (Equal(export.LegalAction.LastModificationReviewDate,
              local.InitialisedToZeros.Date))
            {
              export.LegalAction.LastModificationReviewDate =
                export.LegalAction.FiledDate;
            }
          }
        }
      }

      // ---------------------------------------------
      // Validate Last Modification Review Date
      // ---------------------------------------------
      if (Lt(local.CurrentDate.Date,
        export.LegalAction.LastModificationReviewDate))
      {
        var field = GetField(export.LegalAction, "lastModificationReviewDate");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "LE0000_LAST_MOD_REV_DATE_GT_CURR";
        }
      }

      // ---------------------------------------------
      // Validate Long Arm Statute.
      // ---------------------------------------------
      switch(AsChar(export.LegalAction.LongArmStatuteIndicator))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          break;
        default:
          var field = GetField(export.LegalAction, "longArmStatuteIndicator");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";
          }

          break;
      }

      // ---------------------------------------------
      // Validate Payment Location.
      // ---------------------------------------------
      if (Equal(global.Command, "ADD"))
      {
        if (IsEmpty(export.LegalAction.PaymentLocation))
        {
          if (AsChar(export.LegalAction.Classification) != 'B' && export
            .Fips.State == 20)
          {
            // --  WR020322 For non-bankruptcy legal actions issued by Kansas 
            // tribunals, default the payment location to PAYCTR.
            export.LegalAction.PaymentLocation = "PAYCTR";
          }
        }
      }

      if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
        (export.LegalAction.Classification) == 'U')
      {
        if (IsEmpty(export.LegalAction.PaymentLocation))
        {
          var field = GetField(export.LegalAction, "paymentLocation");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "LE0000_PAYMENT_LOCN_REQD";
          }
        }
      }

      if (!IsEmpty(export.LegalAction.PaymentLocation))
      {
        local.Code.CodeName = "LEGAL ACTION PAYMENT LOCATION";
        local.CodeValue.Cdvalue = export.LegalAction.PaymentLocation ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "paymentLocation");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Initiating State.
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.InitiatingState))
      {
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = export.LegalAction.InitiatingState ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "initiatingState");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Responding State.
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.RespondingState))
      {
        local.Code.CodeName = "STATE CODE";
        local.CodeValue.Cdvalue = export.LegalAction.RespondingState ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "respondingState");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Init Country
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.InitiatingCountry))
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = export.LegalAction.InitiatingCountry ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "initiatingCountry");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validat Resp Country
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.RespondingCountry))
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = export.LegalAction.RespondingCountry ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "respondingCountry");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Dismissed Without Prejudice (Y/N).
      // ---------------------------------------------
      switch(AsChar(export.LegalAction.DismissedWithoutPrejudiceInd))
      {
        case 'Y':
          break;
        case 'N':
          break;
        case ' ':
          break;
        default:
          var field =
            GetField(export.LegalAction, "dismissedWithoutPrejudiceInd");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ZD_ACO_NE00_INVALID_INDICATOR_YN";
          }

          break;
      }

      // ---------------------------------------------
      // Validate Dismissal Code.
      // ---------------------------------------------
      if (!IsEmpty(export.LegalAction.DismissalCode))
      {
        local.Code.CodeName = "LEGAL ACTION DISMISSAL";
        local.CodeValue.Cdvalue = export.LegalAction.DismissalCode ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) != 'Y')
        {
          var field = GetField(export.LegalAction, "dismissalCode");

          field.Error = true;

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
      }

      // ---------------------------------------------
      // Validate Attorney / Contractor.
      // ---------------------------------------------
      // -------------------------------------------------------
      // Removed validation of attorney contractor.  This is
      // unnecessary since all reassignments will be done through
      // SP ASSIGN PLAN TASK.
      // -------------------------------------------------------
      // -----------------------------------------------
      // No prompt selection allowed with add or updates
      // -----------------------------------------------
      if (!IsEmpty(export.PromptClass.SelectChar))
      {
        var field = GetField(export.PromptClass, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptListActionsTaken.SelectChar))
      {
        var field = GetField(export.PromptListActionsTaken, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptTribunal.SelectChar))
      {
        var field = GetField(export.PromptTribunal, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptPmtLocation.SelectChar))
      {
        var field = GetField(export.PromptPmtLocation, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptType.SelectChar))
      {
        var field = GetField(export.PromptType, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptOrderAuth.SelectChar))
      {
        var field = GetField(export.PromptOrderAuth, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptInitState.SelectChar))
      {
        var field = GetField(export.PromptInitState, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptRespState.SelectChar))
      {
        var field = GetField(export.PromptRespState, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptDismissalCode.SelectChar))
      {
        var field = GetField(export.PromptDismissalCode, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptEstablishmentCode.SelectChar))
      {
        var field = GetField(export.PromptEstablishmentCode, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptListAltAddrLoc.SelectChar))
      {
        var field = GetField(export.PromptListAltAddrLoc, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptInitCountry.SelectChar))
      {
        var field = GetField(export.PromptInitCountry, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsEmpty(export.PromptRespCountry.SelectChar))
      {
        var field = GetField(export.PromptRespCountry, "selectChar");

        field.Error = true;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NE0000_INVALID_PROMPT_SELTD";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------
      // Validate Standard Number if not blanks.
      // If blanks, let the system compute it.
      // ---------------------------------------------
      if (IsEmpty(export.LegalAction.StandardNumber) || AsChar
        (export.LegalAction.Classification) != AsChar
        (export.HiddenLegalAction.Classification) && AsChar
        (import.HiddenStdNumbGenerated.Flag) != 'Y' && (
          AsChar(export.LegalAction.Classification) == 'B' || AsChar
        (export.HiddenLegalAction.Classification) == 'B'))
      {
        // -- GVandy 4/5/01 This logic was moved into the add and update cases.
      }
      else
      {
        UseLeCabValidateStdCtOrdNo();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          if (Equal(global.Command, "ADD") || Equal
            (global.Command, "UPDATE") && IsEmpty
            (export.HiddenLegalAction.CourtCaseNumber))
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Error = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "standardNumber");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (Equal(global.Command, "ADD"))
          {
            export.HiddenStdNumbGenerated.Flag =
              import.HiddenStdNumbGenerated.Flag;
          }

          return;
        }
      }
    }

    if (Equal(global.Command, "RETLROL"))
    {
      // -- If an AP/Case Role was selected on LROL then change to command to 
      // PRINT.
      if (IsEmpty(import.FromLrolCsePerson.Number))
      {
        ExitState = "LE0000_NO_AP_SELECTED";

        return;
      }
      else
      {
        global.Command = "LAIS";
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "CHGCC":
        // ****************************************************************
        // 01/18/00	R Jean
        // PR81987 - Flow to procedure LACC in order to change court case number
        // and or standard number.
        // ****************************************************************
        ExitState = "ECO_LNK_TO_LACC";

        return;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        return;
      case "HEAR":
        ExitState = "ECO_LNK_TO_HEAR";

        return;
      case "ASIN":
        // 02/17/06  GVandy	PR205434	Don't allow user to change key info before 
        // PF18 (ASIN).
        // -- This corrects the problem where user changes classification and 
        // flows to ASIN which then
        // allows an assignment to be made based on this "changed" 
        // classification.  Non attorneys were
        // being assigned to legal actions that required an attorney because of 
        // this problem.
        if (AsChar(import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (import.Tribunal.Identifier != import.HiddenTribunal.Identifier)
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.ActionTaken,
          import.HiddenLegalAction.ActionTaken))
        {
          var field = GetField(export.ActionTaken, "description");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (IsExitState("CO0000_KEY_CHANGE_NOT_ALLOWD"))
        {
          return;
        }

        // ---------------------------------------------
        // Make sure that the legal action has been read
        // ---------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          // *********************************************
          // *          03-29-96  H HOOKS                *
          // * Court Case Number can be spaces if        *
          // * Classification is 'P' or 'F'.         *
          // * 9/9/99 R. Jean
          // * Added classes M, O, E
          // *********************************************
          if (IsEmpty(export.HiddenLegalAction.CourtCaseNumber))
          {
            if (AsChar(export.LegalAction.Classification) == 'M' && AsChar
              (export.HiddenLegalAction.Classification) == 'M')
            {
              goto Test2;
            }

            if (AsChar(export.LegalAction.Classification) == 'O' && AsChar
              (export.HiddenLegalAction.Classification) == 'O')
            {
              goto Test2;
            }

            if (AsChar(export.LegalAction.Classification) == 'E' && AsChar
              (export.HiddenLegalAction.Classification) == 'E')
            {
              goto Test2;
            }

            if (AsChar(export.LegalAction.Classification) == 'P' && AsChar
              (export.HiddenLegalAction.Classification) == 'P')
            {
              goto Test2;
            }

            if (AsChar(export.LegalAction.Classification) == 'F' && AsChar
              (export.HiddenLegalAction.Classification) == 'F')
            {
              goto Test2;
            }

            if (AsChar(export.LegalAction.Classification) == 'U' && AsChar
              (export.HiddenLegalAction.Classification) == 'U')
            {
              goto Test2;
            }
          }

          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

Test2:

        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier || import.LegalAction.Identifier == 0)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "LE0000_DISP_BEF_ASIN";

          return;
        }

        export.DlgflwAsinHeaderObject.Text20 = "LEGAL ACTION";
        ExitState = "ECO_LNK_TO_ASIN";

        return;
      case "LIST":
        local.TotalPromptsSelected.Count = 0;

        // ---------------------------------------------
        // Validate Prompt pay to fips
        // ---------------------------------------------
        switch(AsChar(export.PromptListAltAddrLoc.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            // ****************************************************************
            // * PR78746 RJEAN Allow Class U legal actions to enter alternate 
            // pay location
            // ****************************************************************
            if (AsChar(export.LegalAction.Classification) == 'J' || AsChar
              (export.LegalAction.Classification) == 'U')
            {
            }
            else
            {
              var field1 = GetField(export.PromptListAltAddrLoc, "selectChar");

              field1.Error = true;

              ExitState = "LE0000_ALT_PAY_TO_VALID_F_J_ONLY";

              return;
            }

            break;
          default:
            var field = GetField(export.PromptListAltAddrLoc, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Classification.
        // ---------------------------------------------
        switch(AsChar(import.PromptClass.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptClass, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt List Actions Taken.
        // ---------------------------------------------
        switch(AsChar(import.PromptListActionsTaken.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptListActionsTaken, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

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
        // Validate Prompt Payment Location.
        // ---------------------------------------------
        switch(AsChar(import.PromptPmtLocation.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptPmtLocation, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";
            ++local.TotalPromptsSelected.Count;

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Type.
        // ---------------------------------------------
        switch(AsChar(import.PromptType.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptType, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Establishment Code.
        // ---------------------------------------------
        switch(AsChar(import.PromptEstablishmentCode.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptEstablishmentCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Order Authority.
        // ---------------------------------------------
        switch(AsChar(export.PromptOrderAuth.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptOrderAuth, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Initiating State.
        // ---------------------------------------------
        switch(AsChar(export.PromptInitState.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptInitState, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Responding State.
        // ---------------------------------------------
        switch(AsChar(export.PromptRespState.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptRespState, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Dismissal Code.
        // ---------------------------------------------
        switch(AsChar(export.PromptDismissalCode.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptDismissalCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Init Country.
        // ---------------------------------------------
        switch(AsChar(export.PromptInitCountry.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptInitCountry, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        // ---------------------------------------------
        // Validate Prompt Respt Country.
        // ---------------------------------------------
        switch(AsChar(export.PromptRespCountry.SelectChar))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ++local.TotalPromptsSelected.Count;

            break;
          default:
            var field = GetField(export.PromptRespCountry, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------
        // Only one Prompt can be selected at one time.
        // ---------------------------------------------
        if (local.TotalPromptsSelected.Count > 1)
        {
          if (!IsEmpty(export.PromptClass.SelectChar))
          {
            var field = GetField(export.PromptClass, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptTribunal.SelectChar))
          {
            var field = GetField(export.PromptTribunal, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptListActionsTaken.SelectChar))
          {
            var field = GetField(export.PromptListActionsTaken, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptEstablishmentCode.SelectChar))
          {
            var field = GetField(export.PromptEstablishmentCode, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptOrderAuth.SelectChar))
          {
            var field = GetField(export.PromptOrderAuth, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptType.SelectChar))
          {
            var field = GetField(export.PromptType, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptPmtLocation.SelectChar))
          {
            var field = GetField(export.PromptPmtLocation, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptListAltAddrLoc.SelectChar))
          {
            var field = GetField(export.PromptListAltAddrLoc, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptInitState.SelectChar))
          {
            var field = GetField(export.PromptInitState, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptRespState.SelectChar))
          {
            var field = GetField(export.PromptRespState, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptDismissalCode.SelectChar))
          {
            var field = GetField(export.PromptDismissalCode, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptInitCountry.SelectChar))
          {
            var field = GetField(export.PromptInitCountry, "selectChar");

            field.Error = true;
          }

          if (!IsEmpty(export.PromptRespCountry.SelectChar))
          {
            var field = GetField(export.PromptRespCountry, "selectChar");

            field.Error = true;
          }

          ExitState = "ZD_ACO_NE_INVALID_MULT_PROMPT_S";

          return;
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

        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptListActionsTaken.SelectChar) == 'S')
        {
          export.Code.CodeName = "ACTION TAKEN";

          if (!IsEmpty(export.LegalAction.Classification))
          {
            export.ValidWithCode.CodeName = "LEGAL ACTION CLASSIFICATION";
            export.ValidWithCodeValue.Cdvalue =
              export.LegalAction.Classification;
          }

          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptTribunal.SelectChar) == 'S')
        {
          export.HiddenSecurity1.LinkIndicator = "L";
          ExitState = "ECO_LNK_TO_LIST_TRIBUNALS";
          global.Command = "RLTRIB";

          return;
        }

        if (AsChar(export.PromptPmtLocation.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION PAYMENT LOCATION";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptType.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION TYPE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptEstablishmentCode.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION ESTABLISHMENT";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptOrderAuth.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION ORDER AUTHORITY";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptInitState.SelectChar) == 'S' || AsChar
          (export.PromptRespState.SelectChar) == 'S')
        {
          export.Code.CodeName = "STATE CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        if (AsChar(export.PromptDismissalCode.SelectChar) == 'S')
        {
          export.Code.CodeName = "LEGAL ACTION DISMISSAL";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        // ****************************************************************
        // 12/13/99	R. Jean
        // PR81361 - Flow to ORGZ instead of NAME
        // ****************************************************************
        if (AsChar(export.PromptListAltAddrLoc.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_ORGZ";

          return;
        }

        if (AsChar(export.PromptInitCountry.SelectChar) == 'S' || AsChar
          (export.PromptRespCountry.SelectChar) == 'S')
        {
          export.Code.CodeName = "COUNTRY CODE";
          ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

          return;
        }

        break;
      case "RETNAME":
        export.PromptListAltAddrLoc.SelectChar = "";

        if (!IsEmpty(export.AltBillingLocn.Number))
        {
          var field = GetField(export.LegalAction, "ctOrdAltBillingAddrInd");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.AltBillingLocn, "number");

          field.Protected = false;
          field.Focused = true;
        }

        return;
      case "RLCVAL":
        // ---------------------------------------------
        // Returned from List Code Values screen. Move
        // values to export.
        // ---------------------------------------------
        if (AsChar(export.PromptClass.SelectChar) == 'S')
        {
          export.PromptClass.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.Classification =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "classification");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptListActionsTaken.SelectChar) == 'S')
        {
          export.PromptListActionsTaken.SelectChar = "";
          export.LegalAction.Classification = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.ActionTaken =
              import.DlgflwSelectedCodeValue.Cdvalue;
            export.Document.Name = export.LegalAction.ActionTaken;
            export.ActionTaken.Description =
              import.DlgflwSelectedCodeValue.Description;
            UseLeCabGetClassForActTaken();

            if (IsEmpty(export.LegalAction.Classification))
            {
              var field1 =
                GetField(export.PromptListActionsTaken, "selectChar");

              field1.Protected = false;
              field1.Focused = true;

              ExitState = "LE0000_COULD_NOT_DETERMINE_CLASS";

              return;
            }

            var field = GetField(export.LegalAction, "establishmentCode");

            field.Protected = false;
            field.Focused = true;

            if (Equal(export.LegalAction.ActionTaken, "IWO") || Equal
              (export.LegalAction.ActionTaken, "IWOMODO") || Equal
              (export.LegalAction.ActionTaken, "IWOTERM") || Equal
              (export.LegalAction.ActionTaken, "ORDIWO2") || Equal
              (export.LegalAction.ActionTaken, "ORDIWOLS") || Equal
              (export.LegalAction.ActionTaken, "ORDIWOPT"))
            {
              export.PrintFxKey.Text8 = "22 LAIS";
            }
            else
            {
              export.PrintFxKey.Text8 = "24 Print";
            }
          }
          else
          {
            var field = GetField(export.PromptListActionsTaken, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptPmtLocation.SelectChar) == 'S')
        {
          export.PromptPmtLocation.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.PaymentLocation =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "paymentLocation");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "paymentLocation");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptType.SelectChar) == 'S')
        {
          export.PromptType.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.Type1 = import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "attorneyApproval");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "type1");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptEstablishmentCode.SelectChar) == 'S')
        {
          export.PromptEstablishmentCode.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.EstablishmentCode =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "filedDate");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "establishmentCode");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptOrderAuth.SelectChar) == 'S')
        {
          export.PromptOrderAuth.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.OrderAuthority =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "type1");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "orderAuthority");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptInitState.SelectChar) == 'S')
        {
          export.PromptInitState.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.InitiatingState =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "initiatingCounty");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "initiatingState");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptRespState.SelectChar) == 'S')
        {
          export.PromptRespState.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.RespondingState =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "respondingCounty");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "respondingState");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptDismissalCode.SelectChar) == 'S')
        {
          export.PromptDismissalCode.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.DismissalCode =
              import.DlgflwSelectedCodeValue.Cdvalue;

            if (AsChar(export.LegalAction.DismissedWithoutPrejudiceInd) == 'Y')
            {
              var field = GetField(export.LegalAction, "refileDate");

              field.Protected = false;
              field.Focused = true;
            }
            else
            {
              var field = GetField(export.LegalAction, "dateCpReqIwoBegin");

              field.Protected = false;
              field.Focused = true;
            }
          }
          else
          {
            var field = GetField(export.LegalAction, "dismissalCode");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptInitCountry.SelectChar) == 'S')
        {
          export.PromptInitCountry.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.InitiatingCountry =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "respondingState");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "initiatingCountry");

            field.Protected = false;
            field.Focused = true;
          }
        }

        if (AsChar(export.PromptRespCountry.SelectChar) == 'S')
        {
          export.PromptRespCountry.SelectChar = "";

          if (!IsEmpty(import.DlgflwSelectedCodeValue.Cdvalue))
          {
            export.LegalAction.RespondingCountry =
              import.DlgflwSelectedCodeValue.Cdvalue;

            var field = GetField(export.LegalAction, "uresaSentDate");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.LegalAction, "respondingCountry");

            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "RLTRIB":
        // ---------------------------------------------
        // Returned from List Tribunals screen. Move
        // values to export.
        // ---------------------------------------------
        if (AsChar(export.PromptTribunal.SelectChar) == 'S')
        {
          export.PromptTribunal.SelectChar = "";

          if (import.DlgflwSelectedTribunal.Identifier > 0)
          {
            export.Fips.Assign(import.DlgflwSelectedFips);
            export.Tribunal.Assign(import.DlgflwSelectedTribunal);
            export.LegalAction.ForeignFipsState = export.Fips.State;
            export.LegalAction.ForeignFipsCounty = export.Fips.County;
            export.LegalAction.ForeignFipsLocation = export.Fips.Location;

            var field = GetField(export.PromptListActionsTaken, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
          else
          {
            var field = GetField(export.PromptTribunal, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }
        }

        return;
      case "RLLGLACT":
        // -------------------------------------------------------------
        // Returned from List Legal Actions by Court Case Number OR
        // from List Legal Actions by CSE_Person 
        // -------------------------------------------------------------
        if (import.DlgflwSelectedLegalAction.Identifier > 0)
        {
          MoveLegalAction5(import.DlgflwSelectedLegalAction, export.LegalAction);
            
          MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
          global.Command = "REDISP";
        }
        else
        {
          export.LegalAction.FiledDate = null;
          export.LegalAction.Identifier = 0;
          export.LegalAction.ActionTaken = "";
          export.LegalAction.Identifier = 0;

          return;
        }

        break;
      case "ADD":
        if (export.LegalAction.FiledDate != null)
        {
          var field = GetField(export.LegalAction, "filedDate");

          field.Error = true;

          ExitState = "LE0000_NO_FILE_DATE_ON_ADD";

          return;
        }

        // -------------------------------------------------------------
        // If a Legal Action with the same Court Case Number, State,
        // County, Action taken, and Filed Date already exists, Add
        // command is not allowed. Prompt user to update instead.
        // -------------------------------------------------------------
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          // ------------------------------------------------------------------
          // Event 95 logic for a petition as Court Case # will only be
          // blank for an internal petition.  USER must enter information
          // on CAPT regardless of the situation.  When this processing
          // proceeds to LROL, a check will be made to determine if
          // this is the first petition for the chosen legal action.
          // ------------------------------------------------------------------
        }
        else if (ReadLegalAction2())
        {
          var field1 = GetField(export.LegalAction, "filedDate");

          field1.Error = true;

          var field2 = GetField(export.ActionTaken, "description");

          field2.Error = true;

          var field3 = GetField(export.Fips, "countyAbbreviation");

          field3.Error = true;

          var field4 = GetField(export.Fips, "stateAbbreviation");

          field4.Error = true;

          var field5 = GetField(export.LegalAction, "courtCaseNumber");

          field5.Error = true;

          ExitState = "LE0000_LEG_ACT_AE_TRY_UPDATE";

          return;
        }
        else
        {
          // -------------------
          // Continue Processing
          // -------------------
        }

        // -------------------------------------------------------------
        // If at this processing point, the export legal action standard
        // number is equal to spaces and the court case number is not
        // equal to spaces, then use cab to derive the standard
        // number from the court case number. After the derivation,
        // sent the results back to the user. We do not want to add
        // the record with the derived standard number without giving
        // the user the option to verify that the derived standard
        // number is accurate.
        // -------------------------------------------------------------
        if (IsEmpty(export.LegalAction.StandardNumber) && !
          IsEmpty(export.LegalAction.CourtCaseNumber) || AsChar
          (export.LegalAction.Classification) != AsChar
          (export.HiddenLegalAction.Classification) && AsChar
          (import.HiddenStdNumbGenerated.Flag) != 'Y' && (
            AsChar(export.LegalAction.Classification) == 'B' || AsChar
          (export.HiddenLegalAction.Classification) == 'B'))
        {
          UseLeCabConvCrtCaseNumToStd();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.LegalAction, "courtCaseNumber");

            field1.Error = true;

            var field2 = GetField(export.LegalAction, "standardNumber");

            field2.Error = true;

            return;
          }

          // CQ55817 Don't want them to be able to change standard_number.
        }

        // ---------------------------------------------
        // Add a new Legal Action.
        // ---------------------------------------------
        local.Local1StLegActCourtCaseNo.Flag = "Y";

        // -- This is a user added legal action so set the system_gen_ind to 
        // spaces.
        export.LegalAction.SystemGenInd = "";
        UseAddLegalAction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("LE0000_TRIBUNAL_MUST_BE_SELECTED"))
          {
            var field1 = GetField(export.Fips, "stateAbbreviation");

            field1.Error = true;

            var field2 = GetField(export.Fips, "countyDescription");

            field2.Error = true;
          }
          else if (IsExitState("LEGAL_ACTION_AE"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;
          }
          else
          {
          }

          UseEabRollbackCics();

          return;
        }

        // -------------------------------------------------------------------
        // If this is the first Legal Action of any kind for this court case
        // number or if this is an internally generated petition, link to
        // the Court Caption screen so that the Court Caption can be
        // entered by the user.  Otherwise, create the Court Caption for
        // this Legal Action by duplicating the one from the previous
        // Legal Action with the same court case.
        // -------------------------------------------------------------------
        if (AsChar(local.Local1StLegActCourtCaseNo.Flag) == 'Y')
        {
        }
        else
        {
          // ----------------------------------------------------------------------------
          // If this is the first legal action of any kind, the necessary
          // information will not be available until processing in LROL.  If
          // not then the necessary information will be available to set
          // off Event 95 w/in LACT.
          // ----------------------------------------------------------------------------
          // 03/10/11  GVandy  CQ25750  When adding new legal actions don't 
          // create events if no service
          // provider was copied from the previous legal action.
          if (ReadLegalActionAssigment())
          {
            UseLeLactRaiseInfrastrEvents1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              return;
            }
          }

          UseLeCreateDuplicatCourtCaption();

          if (IsExitState("LEGAL_ACTION_NF"))
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            UseEabRollbackCics();

            return;
          }
          else
          {
            local.DuplicateCaptForLeact.Flag = "Y";
            ExitState = "LE0000_LEGAL_ACT_AND_CAPTION_ADD";
          }

          if (AsChar(export.LegalAction.Classification) == 'B')
          {
            // -- For Bankruptcy legal actions don't do the automated flow to 
            // CAPT, LROL, and CPAT.  We copied the court caption from the
            // previous legal action which is all that is required for
            // Bankruptcy legal actions.
            break;
          }
        }

        if (AsChar(export.LegalAction.Classification) == 'B')
        {
          // -- Don't set the flag to indicate that LROL, and CPAT are required.
          // These are not required for Bankruptcy legal actions, just flow to
          // CAPT for the user to enter the caption.
          export.LegalActionFlow.Flag = "";
        }
        else
        {
          export.LegalActionFlow.Flag = "Y";
        }

        // ****************************************************************
        // 06/21/07	  G. Pan
        // PR308058 - When classification = "D" and this is not the
        //            first leagal action of any kind, flows to DISC
        // ****************************************************************
        if (AsChar(local.Local1StLegActCourtCaseNo.Flag) != 'Y' && AsChar
          (export.LegalAction.Classification) == 'D')
        {
          export.LegalActionFlow.Flag = "Y";
          ExitState = "ECO_LNK_TO_DISCOVERY";

          return;
        }

        ExitState = "ECO_LNK_TO_COURT_CAPTION";

        return;
      case "RCAP":
        if (ReadLegalAction3())
        {
          export.Document.Name = export.LegalAction.ActionTaken;
          MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
          export.HiddenFips.Assign(export.Fips);
          export.HiddenTribunal.Assign(export.Tribunal);
          UseLeGetPetitionerRespondent();

          if (!ReadCourtCaption2())
          {
            if (Equal(import.HiddenPrevUserAction.Command, "ADD") && AsChar
              (entities.LegalAction.Classification) == 'U')
            {
              // -- Caption is no longer required for U class legal actions.
              goto Read;
            }

            ExitState = "LE0000_NO_CAPT_LINE_ENTERED";

            return;
          }

Read:

          global.Command = "REDISP";

          if (Equal(import.HiddenPrevUserAction.Command, "ADD"))
          {
            // -- We are returning from CAPT after adding a legal action.
            local.ReturnedFromCaptAftAdd.Flag = "Y";

            if (AsChar(export.LegalAction.Classification) == 'N')
            {
              // -- If the user is adding an "N" class legal action then flow to
              // ASIN for assignment of a service provider.
              export.DlgflwAsinHeaderObject.Text20 = "LEGAL ACTION";
              ExitState = "ECO_LNK_TO_ASIN";

              return;
            }
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        // ****************************************************************
        // 06/21/07	G. Pan
        // PR308058 - Return back from DISC
        // ****************************************************************
        break;
      case "RDIS":
        if (ReadLegalAction3())
        {
          if (!ReadDiscovery())
          {
            ExitState = "LE0000_NO_DISCOVERY_ENTERED";

            return;
          }
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";

          return;
        }

        global.Command = "REDISP";

        if (Equal(import.HiddenPrevUserAction.Command, "ADD"))
        {
          // -- We are returning from CAPT after adding a legal action.
          local.ReturnedFromDiscAftAdd.Flag = "Y";
        }

        break;
      case "UPDATE":
        // -------------------------------------------------------------
        // Make sure that Court Case Number hasn't been changed
        // before an update Unless the Legal Action is a Petition or
        // Affidavit and the Import Hidden Legal Action Court Case No
        // is spaces.
        // -------------------------------------------------------------
        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          if ((AsChar(export.LegalAction.Classification) == 'P' || AsChar
            (export.LegalAction.Classification) == 'F' || AsChar
            (export.LegalAction.Classification) == 'M' || AsChar
            (export.LegalAction.Classification) == 'O' || AsChar
            (export.LegalAction.Classification) == 'E' || AsChar
            (export.LegalAction.Classification) == 'U' || AsChar
            (export.LegalAction.Classification) == 'S') && AsChar
            (export.LegalAction.Classification) == AsChar
            (export.HiddenLegalAction.Classification) && IsEmpty
            (export.HiddenLegalAction.CourtCaseNumber))
          {
          }
          else
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

            return;
          }
        }

        // -------------------------------------------------------------
        // Make sure that the Action Taken hasn't been changed before
        // an update.   RCG - 11/19/97     H00032712
        // -------------------------------------------------------------
        if (!Equal(import.LegalAction.ActionTaken,
          import.HiddenLegalAction.ActionTaken))
        {
          var field = GetField(export.ActionTaken, "description");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // -------------------------------------------------------------
        // Verify classification has not been changed before update
        // -------------------------------------------------------------
        if (AsChar(import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";

          return;
        }

        // -------------------------------------------------------------
        // Verify a display has been performed before the updating
        // -------------------------------------------------------------
        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        // cq65750
        if (AsChar(export.LegalAction.Classification) == 'J' && (
          Equal(export.LegalAction.EstablishmentCode, "CS") || Equal
          (export.LegalAction.EstablishmentCode, "CT")))
        {
          if (Lt(export.HiddenLegalAction.FiledDate,
            export.LegalAction.FiledDate) && !
            Lt(new DateTime(1, 1, 1), export.HiddenLegalAction.FiledDate))
          {
            if (ReadLegalActionDetail1())
            {
              if (ReadGuidelineDeviations())
              {
                // read yo see if there is already a record tied to the current 
                // legal action.
                // if there is then don't go, if there is not then go. continue 
                // to process record normally
              }
              else if (IsEmpty(import.FromLrolCsePerson.Number) && IsEmpty
                (import.FromLrolCase.Number))
              {
                local.ApCommon.Count = 0;
                local.PersonCase.Count = 0;

                foreach(var item in ReadCaseCsePersonCaseRole())
                {
                  ReadCaseRoleCsePerson();

                  if (local.ApCommon.Count > 1)
                  {
                    export.EiwoSelection.Flag = "N";
                    ExitState = "ECO_LNK_TO_LROL";

                    return;
                  }

                  ++local.PersonCase.Count;
                }

                if (local.PersonCase.Count == 1)
                {
                  // now need to go to gldv
                  export.FromLrolCase.Number = entities.Case1.Number;
                  export.FromLrolCsePerson.Number = entities.CsePerson.Number;
                  export.FromLrolCaseRole.Type1 = entities.CaseRole.Type1;
                  export.FromLact.Text1 = "Y";
                  ExitState = "ECO_LINK_TO_GLDV";

                  return;
                }
                else
                {
                  ExitState = "LE0000_EIWO_LOPS_MISSING";

                  return;
                }
              }
              else
              {
                export.FromLact.Text1 = "Y";
                ExitState = "ECO_LINK_TO_GLDV";

                return;
              }
            }
            else
            {
              // can proceed on in lact
            }
          }
        }

        // ****************************************************************
        // *01/11/99	R. Jean
        // PR82883 - Add edit for update function that won't allow to update
        // a legal action so that two legal action with the same CC#, FIPS,
        // Tribunal, Filed Date, Action Taken.  Read for dupe legal action,
        // read only if filed date is changed.
        // ****************************************************************
        if (IsEmpty(export.LegalAction.CourtCaseNumber))
        {
        }
        else if (ReadLegalAction1())
        {
          var field1 = GetField(export.LegalAction, "filedDate");

          field1.Error = true;

          var field2 = GetField(export.ActionTaken, "description");

          field2.Error = true;

          var field3 = GetField(export.Fips, "countyAbbreviation");

          field3.Error = true;

          var field4 = GetField(export.Fips, "stateAbbreviation");

          field4.Error = true;

          var field5 = GetField(export.LegalAction, "courtCaseNumber");

          field5.Error = true;

          ExitState = "LEGAL_ACTION_AE";

          return;
        }
        else
        {
          // -------------------
          // Continue Processing
          // -------------------
        }

        // -------------------------------------------------------------
        // If at this processing point, the export legal action standard
        // number is equal to spaces and the court case number is not
        // equal to spaces, then use cab to derive the standard
        // number from the court case number.  Give the user the
        // option to verify that the derived standard number is
        // accurate.
        // -------------------------------------------------------------
        if (IsEmpty(export.LegalAction.StandardNumber) && !
          IsEmpty(export.LegalAction.CourtCaseNumber))
        {
          if (AsChar(export.LegalAction.Classification) == 'P' || AsChar
            (export.LegalAction.Classification) == 'F' || AsChar
            (export.LegalAction.Classification) == 'M' || AsChar
            (export.LegalAction.Classification) == 'O' || AsChar
            (export.LegalAction.Classification) == 'E' || AsChar
            (export.LegalAction.Classification) == 'U' || AsChar
            (export.LegalAction.Classification) == 'S' || AsChar
            (local.TribalTribunal.Flag) == 'Y')
          {
            UseLeCabConvCrtCaseNumToStd();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              var field = GetField(export.LegalAction, "courtCaseNumber");

              field.Error = true;

              return;
            }

            // CQ55817 Don't want them to be able to change standard_number. :2
          }
        }

        // ---------------------------------------------
        // Update the Legal Action.
        // ---------------------------------------------
        UseUpdateLegalAction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("LE0000_ALT_BILL_LOCN_CSE_PERS_NF"))
          {
            var field = GetField(export.AltBillingLocn, "number");

            field.Error = true;
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

            field1.Error = true;

            var field2 = GetField(export.Fips, "countyDescription");

            field2.Error = true;
          }
          else
          {
          }

          UseEabRollbackCics();

          return;
        }

        if (AsChar(local.FiledDateChanged.Flag) == 'Y')
        {
          UseLeLactRaiseInfrastrEvents2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }

        // ------------------------------------------------------------
        // CSEnet - Depending on the legal action, a different RS code
        // will be created for CSEnet.
        // ------------------------------------------------------------
        if (Equal(export.LegalAction.FiledDate,
          export.HiddenLegalAction.FiledDate))
        {
          // ------------------------------------------------------------
          // Filed Date field has to have a date for CSEnet logic
          // ------------------------------------------------------------
        }
        else
        {
          if (export.LegalAction.FiledDate != null && Equal
            (export.HiddenLegalAction.FiledDate, null))
          {
            switch(TrimEnd(export.LegalAction.ActionTaken))
            {
              case "DEFJPATJ":
                local.Specific.ActionReasonCode = "PSESO";

                break;
              case "PATERNJ":
                local.Specific.ActionReasonCode = "PSESO";

                break;
              case "PATONLYJ":
                local.Specific.ActionReasonCode = "PSEST";

                break;
              case "VOLPATTJx":
                break;
              case "DISMISSO":
                local.Specific.ActionReasonCode = "SCDIS";

                break;
              case "CONTEMPT":
                local.Specific.ActionReasonCode = "SICPS";

                break;
              case "MODSUPPO":
                local.Specific.ActionReasonCode = "SSMOD";

                break;
              case "BENCHWO":
                local.Specific.ActionReasonCode = "EIWAR";

                break;
              case "REGENFNJ":
                local.Specific.ActionReasonCode = "ESREG";

                break;
              case "REGMODNJ":
                local.Specific.ActionReasonCode = "ESREG";

                break;
              case "IWO":
                local.Specific.ActionReasonCode = "ESWAG";

                break;
              case "ORDIWO2":
                local.Specific.ActionReasonCode = "ESWAG";

                break;
              default:
                // -----------------------------
                // No CSEnet processing required
                // -----------------------------
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                break;
            }
          }
          else
          {
            // ------------------------------------------------------------
            // Pertinent information has not changed - do not send another
            // ------------------------------------------------------------
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

            break;
          }

          local.ApCsePerson.Number = export.Ap.Number;
          local.ScreenIdentification.Command = "LACT";
          UseSiCreateAutoCsenetTrans();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            UseEabRollbackCics();

            return;
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "RETLAIS":
        global.Command = "REDISP";

        break;
      case "LAIS":
        if (!Equal(export.PrintFxKey.Text8, "22 LAIS"))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";

          return;
        }

        if (AsChar(import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (import.Tribunal.Identifier != import.HiddenTribunal.Identifier)
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.ActionTaken,
          import.HiddenLegalAction.ActionTaken))
        {
          var field = GetField(export.ActionTaken, "description");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (IsExitState("CO0000_KEY_CHANGE_NOT_ALLOWD"))
        {
          return;
        }

        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";

          return;
        }

        // mjr
        // -----------------------------------------------
        // 10/09/1998
        // Removed call to sp_print_lact_docs
        // 	WHICH IMPORTS:  export legal_action
        // 			export document
        // 	WHICH EXPORTS:  local_multi_obl_type_print_flag
        // Added new method to execute Print
        // ------------------------------------------------------------
        ReadDocument();

        if (IsEmpty(entities.Document.Name))
        {
          ExitState = "SP0000_LACT_HAS_NO_DOCUMENT";

          return;
        }

        // 06/09/15  GVandy  CQ22212  Changes to support electronic Income 
        // Withholding (e-IWO).
        if (AsChar(export.LegalAction.Classification) == 'I' && (
          Equal(export.LegalAction.ActionTaken, "IWO") || Equal
          (export.LegalAction.ActionTaken, "IWOMODO") || Equal
          (export.LegalAction.ActionTaken, "IWOTERM") || Equal
          (export.LegalAction.ActionTaken, "ORDIWO2") || Equal
          (export.LegalAction.ActionTaken, "ORDIWOLS") || Equal
          (export.LegalAction.ActionTaken, "ORDIWOPT")))
        {
          UseLeValidateForEiwo();

          // -- Escape.  Either an error message will be displayed or an exit 
          // state to initiate a dialog
          //    flow to LROL or LAIS will be initiated.
          return;
        }

        break;
      case "PRINT":
        if (!Equal(export.PrintFxKey.Text8, "24 Print"))
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";

          return;
        }

        if (AsChar(import.LegalAction.Classification) != AsChar
          (import.HiddenLegalAction.Classification))
        {
          var field = GetField(export.LegalAction, "classification");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.CourtCaseNumber,
          import.HiddenLegalAction.CourtCaseNumber))
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.StateAbbreviation,
          import.HiddenFips.StateAbbreviation))
        {
          var field = GetField(export.Fips, "stateAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.Fips.CountyAbbreviation,
          import.HiddenFips.CountyAbbreviation))
        {
          var field = GetField(export.Fips, "countyAbbreviation");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (import.Tribunal.Identifier != import.HiddenTribunal.Identifier)
        {
          var field1 = GetField(export.Fips, "stateAbbreviation");

          field1.Error = true;

          var field2 = GetField(export.Fips, "countyAbbreviation");

          field2.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(import.LegalAction.ActionTaken,
          import.HiddenLegalAction.ActionTaken))
        {
          var field = GetField(export.ActionTaken, "description");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (IsExitState("CO0000_KEY_CHANGE_NOT_ALLOWD"))
        {
          return;
        }

        if (import.LegalAction.Identifier != import
          .HiddenLegalAction.Identifier)
        {
          var field = GetField(export.LegalAction, "courtCaseNumber");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_PRINT";

          return;
        }

        // mjr
        // -----------------------------------------------
        // 10/09/1998
        // Removed call to sp_print_lact_docs
        // 	WHICH IMPORTS:  export legal_action
        // 			export document
        // 	WHICH EXPORTS:  local_multi_obl_type_print_flag
        // Added new method to execute Print
        // ------------------------------------------------------------
        ReadDocument();

        if (IsEmpty(entities.Document.Name))
        {
          ExitState = "SP0000_LACT_HAS_NO_DOCUMENT";

          return;
        }

        // mjr
        // ------------------------------------------------
        // 01/12/2000
        // Clear NEXT TRAN before invoking Print Process
        // -------------------------------------------------------------
        export.HiddenNextTranInfo.Assign(local.NullNextTranInfo);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        export.HiddenNextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        export.HiddenNextTranInfo.MiscText2 = "PRINT:" + entities.Document.Name;
        local.PrintProcess.Flag = "Y";
        local.PrintProcess.Command = "PRINT";

        // ***********************************************************************************************
        // 10/18/2005              DDupree                PR - 00258198
        // Added the following read so that we could pass the ap id which is 
        // need in some legal docs.
        // ***********************************************************************************************
        if (ReadLegalActionLegalActionDetailLegalActionPerson())
        {
          export.HiddenNextTranInfo.CsePersonNumberAp =
            entities.CsePerson.Number;
        }

        UseScCabNextTranPut3();

        return;
      case "PRINTRET":
        // mjr
        // -----------------------------------------------
        // 10/09/1998
        // After the document is Printed (the user may still be looking
        // at WordPerfect), control is returned here.  Any cleanup
        // processing which is necessary after a print, should be done now.
        // ------------------------------------------------------------
        UseScCabNextTranGet();
        export.LegalAction.CourtCaseNumber =
          export.HiddenNextTranInfo.CourtCaseNumber ?? "";
        export.LegalAction.Identifier =
          export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();
        global.Command = "REDISP";

        break;
      case "RETURN":
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // -------------------------------------------------------------
          // It came to this screen from HIST or MONA
          // -------------------------------------------------------------
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

        // -------------------------------------------------------------
        // Otherwise it is a normal return to the linking procedure
        // -------------------------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        // ---------------------------------------------
        // Sign the user off the KESSEP system.
        // ---------------------------------------------
        UseScCabSignoff();

        return;
      case "LROL":
        // ---------------------------------------------
        // Transfer to "Maintain Legal Action Role".
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_LEGAL_ROLE";

        return;
      case "LDET":
        // ---------------------------------------------
        // Transfer to "Maintain Legal Action Detail".
        // ---------------------------------------------
        ExitState = "ECO_XFR_TO_LEGAL_DETAIL";

        return;
      case "CAPT":
        // ---------------------------------------------
        // Link to "Maintain Court Caption".
        // ---------------------------------------------
        ExitState = "ECO_LNK_TO_COURT_CAPTION";

        return;
      case "ENTER":
        // ---------------------------------------------------------------
        // The ENTER key will not be used for functionality here. If it is
        // pressed, an exit state message should be output.
        // ---------------------------------------------------------------
        ExitState = "ACO_NE0000_INVALID_COMMAND";

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
        // -------------------------------------------------------------
        // This will occur when LACT is entered via the dialog flow and
        // only the legal action identifier is passed to this screen.
        // -------------------------------------------------------------
        if (export.HiddenLegalAction.Identifier == 0 && export
          .LegalAction.Identifier > 0)
        {
          UseLeDisplayLegalAction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("LE0000_LEGAL_ACT_AND_CAPTION_ADD"))
          {
          }
          else
          {
            return;
          }

          export.Document.Name = export.LegalAction.ActionTaken;
          UseLeGetPetitionerRespondent();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }

          break;
        }

        // ---------------------------------------------------------------
        // If the screen has already been displayed, (identifier is
        // present and equal to hidden identifier) and the court case
        // number and classification haven't been changed, there is
        // no need to link to list screen to choose a legal action. It is
        // OK to display the screen.
        // ---------------------------------------------------------------
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
          }
          else if (IsExitState("LE0000_LEGAL_ACT_AND_CAPTION_ADD"))
          {
          }
          else
          {
            return;
          }

          export.Document.Name = export.LegalAction.ActionTaken;
        }
        else
        {
          if (IsEmpty(export.LegalAction.CourtCaseNumber) && IsEmpty
            (export.LegalAction.ForeignOrderNumber))
          {
            ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

            return;
          }

          // ---------------------------------------------
          // If the Court Case Number that was entered,
          // exists on more than one Legal Action, display
          // a list screen to select the desired one.
          // ---------------------------------------------
          local.NoOfLegalActionsFound.Count = 0;
          local.Search.Classification = export.LegalAction.Classification;

          if (!IsEmpty(export.Fips.StateAbbreviation))
          {
            // -----------
            // US tribunal
            // -----------
            foreach(var item in ReadLegalActionTribunalFips())
            {
              if (IsEmpty(import.LegalAction.Classification))
              {
                // ---------------------------------------------------------------
                // If classification was not entered then count all legal action
                // regardless of classification.
                // ---------------------------------------------------------------
              }
              else if (AsChar(entities.LegalAction.Classification) == AsChar
                (import.LegalAction.Classification))
              {
                // ---------------------------------------------------------------
                // If classification was  entered then count only the legal
                // action for that classification.
                // ---------------------------------------------------------------
              }
              else
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                export.LegalAction.Classification = local.Search.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction4(entities.LegalAction, export.LegalAction);
            }
          }
          else
          {
            // -----------------
            // foreign tribunal
            // -----------------
            if (export.Tribunal.Identifier == 0)
            {
              ExitState = "LE0000_TRIB_REQD_4_FOREIGN_TRIB";

              var field = GetField(export.PromptTribunal, "selectChar");

              field.Protected = false;
              field.Focused = true;

              return;
            }

            foreach(var item in ReadLegalActionTribunal())
            {
              if (IsEmpty(import.LegalAction.Classification))
              {
                // ---------------------------------------------------------------
                // If classification was not entered then count all legal action
                // regardless of classification.
                // ---------------------------------------------------------------
              }
              else if (AsChar(entities.LegalAction.Classification) == AsChar
                (import.LegalAction.Classification))
              {
                // --------------------------------------------------------------
                // If classification was entered then count only the legal
                // action for that classification.
                // ---------------------------------------------------------------
              }
              else
              {
                continue;
              }

              ++local.NoOfLegalActionsFound.Count;

              if (local.NoOfLegalActionsFound.Count > 1)
              {
                export.LegalAction.Classification = local.Search.Classification;
                ExitState = "ECO_LNK_LST_LEG_ACT_BY_CRT_CASE";

                return;
              }

              MoveLegalAction4(entities.LegalAction, export.LegalAction);
            }
          }

          if (local.NoOfLegalActionsFound.Count == 0)
          {
            var field = GetField(export.LegalAction, "courtCaseNumber");

            field.Error = true;

            ExitState = "LEGAL_ACTION_NF";

            return;
          }

          UseLeDisplayLegalAction();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }

          export.Document.Name = export.LegalAction.ActionTaken;
        }

        UseLeGetPetitionerRespondent();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "REDISP":
        if (export.LegalAction.Identifier == 0 && IsEmpty
          (export.LegalAction.CourtCaseNumber))
        {
          return;
        }

        UseLeDisplayLegalAction();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Document.Name = export.LegalAction.ActionTaken;
          UseLeGetPetitionerRespondent();

          if (AsChar(local.ReturnedFromCaptAftAdd.Flag) == 'Y')
          {
            if (ReadCourtCaption1())
            {
              ExitState = "LE0000_LACT_CAP_LROL_INFO_ADDED";
            }
            else
            {
              ExitState = "LE0000_LACT_LROL_INFO_ADDED";
            }

            // ****************************************************************
            // 06/21/07	G. Pan
            // PR308058 - Return back from DISC and send out message.
            // ****************************************************************
          }
          else if (AsChar(local.ReturnedFromDiscAftAdd.Flag) == 'Y')
          {
            ExitState = "LE0000_LACT_DISC_INFO_ADDED";
          }
          else if (AsChar(local.DuplicateCaptForLeact.Flag) == 'Y')
          {
            ExitState = "LE0000_LEGAL_ACT_AND_CAPTION_ADD";
          }
          else
          {
            // mjr
            // ----------------------------------------------
            // 10/09/1998
            // Added check for an exitstate returned from Print
            // -----------------------------------------------------------
            local.Position.Count =
              Find(export.HiddenNextTranInfo.MiscText2, "PRINT:");

            if (local.Position.Count <= 0)
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
            else
            {
              // ----------------------------------------------------------
              // mjr---> Determines the appropriate exitstate for the Print
              // process
              // ------------------------------------------------------------
              local.PrintRetCode.Text50 =
                export.HiddenNextTranInfo.MiscText2 ?? Spaces(50);
              UseSpPrintDecodeReturnCode();
              export.HiddenNextTranInfo.MiscText2 = local.PrintRetCode.Text50;
            }
          }
        }

        break;
      default:
        break;
    }

    if (IsExitState("LEGAL_ACTION_NF"))
    {
      var field = GetField(export.LegalAction, "courtCaseNumber");

      field.Error = true;
    }
    else
    {
    }

    // ---------------------------------------------------------------
    // Use the code_value table to obtain the description for the
    // legal_action action_taken
    // ---------------------------------------------------------------
    if (!IsEmpty(export.LegalAction.ActionTaken))
    {
      UseLeGetActionTakenDescription();
    }

    if (Equal(export.LegalAction.ActionTaken, "IWO") || Equal
      (export.LegalAction.ActionTaken, "IWOMODO") || Equal
      (export.LegalAction.ActionTaken, "IWOTERM") || Equal
      (export.LegalAction.ActionTaken, "ORDIWO2") || Equal
      (export.LegalAction.ActionTaken, "ORDIWOLS") || Equal
      (export.LegalAction.ActionTaken, "ORDIWOPT"))
    {
      export.PrintFxKey.Text8 = "22 LAIS";
    }
    else
    {
      export.PrintFxKey.Text8 = "24 Print";
    }

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

    if (!IsEmpty(global.Command))
    {
      MoveLegalAction3(export.LegalAction, export.HiddenLegalAction);
    }

    // ---------------------------------------------------------------
    // If all processing completed successfully, move all exports
    // to previous exports.
    // ---------------------------------------------------------------
    export.HiddenFips.Assign(export.Fips);
    export.HiddenTribunal.Assign(export.Tribunal);
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.ForeignFipsState = source.ForeignFipsState;
    target.ForeignFipsCounty = source.ForeignFipsCounty;
    target.ForeignFipsLocation = source.ForeignFipsLocation;
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
    target.DateCpReqIwoBegin = source.DateCpReqIwoBegin;
    target.DateNonCpReqIwoBegin = source.DateNonCpReqIwoBegin;
    target.CtOrdAltBillingAddrInd = source.CtOrdAltBillingAddrInd;
    target.InitiatingCountry = source.InitiatingCountry;
    target.RespondingCountry = source.RespondingCountry;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.ForeignFipsState = source.ForeignFipsState;
    target.ForeignFipsCounty = source.ForeignFipsCounty;
    target.ForeignFipsLocation = source.ForeignFipsLocation;
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
    target.DateCpReqIwoBegin = source.DateCpReqIwoBegin;
    target.DateNonCpReqIwoBegin = source.DateNonCpReqIwoBegin;
    target.CtOrdAltBillingAddrInd = source.CtOrdAltBillingAddrInd;
    target.InitiatingCountry = source.InitiatingCountry;
    target.RespondingCountry = source.RespondingCountry;
    target.SystemGenInd = source.SystemGenInd;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.EndDate = source.EndDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.ForeignOrderNumber = source.ForeignOrderNumber;
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.ActionTaken = source.ActionTaken;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction5(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
  }

  private static void MoveLegalAction6(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction7(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction8(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.ActionTaken = source.ActionTaken;
  }

  private static void MoveLegalAction9(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction10(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveLegalAction11(LegalAction source, LegalAction target)
  {
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction12(LegalAction source, LegalAction target)
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

  private void UseAddLegalAction()
  {
    var useImport = new AddLegalAction.Import();
    var useExport = new AddLegalAction.Export();

    useImport.AltBillingLocn.Number = export.AltBillingLocn.Number;
    useImport.Import1StLegActCourtCaseNo.Flag =
      local.Local1StLegActCourtCaseNo.Flag;
    useImport.Tribunal.Assign(export.Tribunal);
    MoveLegalAction2(export.LegalAction, useImport.LegalAction);

    Call(AddLegalAction.Execute, useImport, useExport);

    local.Local1StLegActCourtCaseNo.Flag =
      useExport.Export1StLegActCourtCaseNo.Flag;
    MoveLegalAction2(useExport.LegalAction, export.LegalAction);
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

  private void UseLeCabConvCrtCaseNumToStd()
  {
    var useImport = new LeCabConvCrtCaseNumToStd.Import();
    var useExport = new LeCabConvCrtCaseNumToStd.Export();

    MoveLegalAction10(export.LegalAction, useImport.LegalAction);
    useImport.Tribunal.Identifier = export.Tribunal.Identifier;

    Call(LeCabConvCrtCaseNumToStd.Execute, useImport, useExport);

    MoveLegalAction12(useExport.LegalAction, export.LegalAction);
  }

  private void UseLeCabGetClassForActTaken()
  {
    var useImport = new LeCabGetClassForActTaken.Import();
    var useExport = new LeCabGetClassForActTaken.Export();

    useImport.LegalAction.ActionTaken = export.LegalAction.ActionTaken;

    Call(LeCabGetClassForActTaken.Execute, useImport, useExport);

    export.LegalAction.Classification = useExport.LegalAction.Classification;
  }

  private void UseLeCabValidateStdCtOrdNo()
  {
    var useImport = new LeCabValidateStdCtOrdNo.Import();
    var useExport = new LeCabValidateStdCtOrdNo.Export();

    useImport.Tribunal.Identifier = export.Tribunal.Identifier;
    MoveLegalAction11(export.LegalAction, useImport.LegalAction);

    Call(LeCabValidateStdCtOrdNo.Execute, useImport, useExport);
  }

  private void UseLeCreateDuplicatCourtCaption()
  {
    var useImport = new LeCreateDuplicatCourtCaption.Import();
    var useExport = new LeCreateDuplicatCourtCaption.Export();

    MoveLegalAction9(export.LegalAction, useImport.Current);

    Call(LeCreateDuplicatCourtCaption.Execute, useImport, useExport);
  }

  private void UseLeDisplayLegalAction()
  {
    var useImport = new LeDisplayLegalAction.Import();
    var useExport = new LeDisplayLegalAction.Export();

    MoveLegalAction6(export.LegalAction, useImport.LegalAction);

    Call(LeDisplayLegalAction.Execute, useImport, useExport);

    export.FipsTribAddress.Country = useExport.FipsTribAddress.Country;
    MoveCsePersonsWorkSet(useExport.AltBillingAddrLocn, export.AltBillingLocn);
    MoveCsePersonsWorkSet(useExport.Ap, export.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.Ar);
    export.LegalAction.Assign(useExport.LegalAction);
    export.Tribunal.Assign(useExport.Tribunal);
    export.Fips.Assign(useExport.Fips);
    MoveOffice(useExport.OspEnforcingOffice, export.OspEnforcingOffice);
    export.OspEnforcingServiceProvider.Assign(
      useExport.OspEnforcingServiceProvider);
    MoveOfficeServiceProvider(useExport.OspEnforcingOfficeServiceProvider,
      export.OspEnforcingOfficeServiceProvider);
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

  private void UseLeLactRaiseInfrastrEvents1()
  {
    var useImport = new LeLactRaiseInfrastrEvents.Import();
    var useExport = new LeLactRaiseInfrastrEvents.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Event95ForNewLegActn.Flag = local.Local1StLegActCourtCaseNo.Flag;

    Call(LeLactRaiseInfrastrEvents.Execute, useImport, useExport);
  }

  private void UseLeLactRaiseInfrastrEvents2()
  {
    var useImport = new LeLactRaiseInfrastrEvents.Import();
    var useExport = new LeLactRaiseInfrastrEvents.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeLactRaiseInfrastrEvents.Execute, useImport, useExport);
  }

  private void UseLeValidateForEiwo()
  {
    var useImport = new LeValidateForEiwo.Import();
    var useExport = new LeValidateForEiwo.Export();

    useImport.CsePerson.Number = import.FromLrolCsePerson.Number;
    useImport.CaseRole.Type1 = import.FromLrolCaseRole.Type1;
    useImport.Case1.Number = import.FromLrolCase.Number;
    MoveLegalAction8(export.LegalAction, useImport.LegalAction);

    Call(LeValidateForEiwo.Execute, useImport, useExport);

    export.ToLaisCsePersonsWorkSet.Number = useExport.CsePersonsWorkSet.Number;
    export.ToLaisCase.Number = useExport.Case1.Number;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

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

  private void UseScCabNextTranPut3()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);
    MoveCommon(local.PrintProcess, useImport.PrintProcess);

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
    MoveLegalAction7(export.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.Specific.ActionReasonCode = local.Specific.ActionReasonCode;
    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = local.ApCsePerson.Number;
    MoveLegalAction5(export.LegalAction, useImport.LegalAction);

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.AltBillingLocn.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.AltBillingLocn);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.PrintRetCode.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.PrintRetCode.Text50 = useExport.WorkArea.Text50;
  }

  private void UseUpdateLegalAction()
  {
    var useImport = new UpdateLegalAction.Import();
    var useExport = new UpdateLegalAction.Export();

    useImport.AltBillingLocn.Number = export.AltBillingLocn.Number;
    useImport.Tribunal.Assign(export.Tribunal);
    useImport.LegalAction.Assign(export.LegalAction);

    Call(UpdateLegalAction.Execute, useImport, useExport);

    local.FiledDateChanged.Flag = useExport.FiledDateChanged.Flag;
    MoveLegalAction1(useExport.LegalAction, export.LegalAction);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCsePersonCaseRole()
  {
    System.Diagnostics.Debug.Assert(entities.Cs.Populated);
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCaseCsePersonCaseRole",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(command, "lgaId1", export.LegalAction.Identifier);
        db.SetInt32(command, "lgaId2", entities.Cs.LgaIdentifier);
        db.SetNullableDate(command, "startDate", date);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Type1 = db.GetString(reader, 3);
        entities.CaseRole.Identifier = db.GetInt32(reader, 4);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 6);
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        local.ApCommon.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCourtCaption1()
  {
    entities.CourtCaption.Populated = false;

    return Read("ReadCourtCaption1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Populated = true;
      });
  }

  private bool ReadCourtCaption2()
  {
    entities.CourtCaption.Populated = false;

    return Read("ReadCourtCaption2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CourtCaption.LgaIdentifier = db.GetInt32(reader, 0);
        entities.CourtCaption.Number = db.GetInt32(reader, 1);
        entities.CourtCaption.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.EpLegalActionDetail.Populated);
    entities.EpCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDt", date);
        db.SetNullableInt32(
          command, "ladRNumber", entities.EpLegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier",
          entities.EpLegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.EpCsePerson.Number = db.GetString(reader, 0);
        entities.EpCsePerson.Type1 = db.GetString(reader, 1);
        entities.EpCsePerson.PaternityLockInd = db.GetNullableString(reader, 2);
        entities.EpCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.EpCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingAltBillLocn.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", export.AltBillingLocn.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAltBillLocn.Number = db.GetString(reader, 0);
        entities.ExistingAltBillLocn.Type1 = db.GetString(reader, 1);
        entities.ExistingAltBillLocn.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingAltBillLocn.Type1);
      });
  }

  private bool ReadDiscovery()
  {
    entities.Discovery.Populated = false;

    return Read("ReadDiscovery",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Discovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.Discovery.RequestedDate = db.GetDate(reader, 1);
        entities.Discovery.Populated = true;
      });
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "actionTaken", export.LegalAction.ActionTaken);
        db.
          SetDate(command, "expirationDate", local.Max.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.ExpirationDate = db.GetDate(reader, 2);
        entities.Document.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
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

  private bool ReadGuidelineDeviations()
  {
    entities.GuidelineDeviations.Populated = false;

    return Read("ReadGuidelineDeviations",
      (db, command) =>
      {
        db.SetInt32(command, "ckfk01738", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.GuidelineDeviations.Identifier = db.GetInt32(reader, 0);
        entities.GuidelineDeviations.FkCktLegalAclegalActionId =
          db.GetInt32(reader, 1);
        entities.GuidelineDeviations.Populated = true;
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
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", export.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableDate(
          command, "filedDt", export.LegalAction.FiledDate.GetValueOrDefault());
          
        db.SetString(command, "actionTaken", export.LegalAction.ActionTaken);
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction3",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigment()
  {
    entities.LegalActionAssigment.Populated = false;

    return Read("ReadLegalActionAssigment",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 1);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.LegalActionAssigment.Populated = true;
      });
  }

  private bool ReadLegalActionDetail1()
  {
    entities.Cs.Populated = false;

    return Read("ReadLegalActionDetail1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "endDt", date);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Cs.LgaIdentifier = db.GetInt32(reader, 0);
        entities.Cs.Number = db.GetInt32(reader, 1);
        entities.Cs.EndDate = db.GetNullableDate(reader, 2);
        entities.Cs.EffectiveDate = db.GetDate(reader, 3);
        entities.Cs.NonFinOblgType = db.GetNullableString(reader, 4);
        entities.Cs.DetailType = db.GetString(reader, 5);
        entities.Cs.OtyId = db.GetNullableInt32(reader, 6);
        entities.Cs.Populated = true;
        CheckValid<LegalActionDetail>("DetailType", entities.Cs.DetailType);
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail2()
  {
    entities.EpLegalActionDetail.Populated = false;

    return ReadEach("ReadLegalActionDetail2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDt", date);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.EpLegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.EpLegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.EpLegalActionDetail.EndDate = db.GetNullableDate(reader, 2);
        entities.EpLegalActionDetail.EffectiveDate = db.GetDate(reader, 3);
        entities.EpLegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 4);
        entities.EpLegalActionDetail.DetailType = db.GetString(reader, 5);
        entities.EpLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.EpLegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionLegalActionDetailLegalActionPerson()
  {
    entities.CsePerson.Populated = false;
    entities.LegalActionDetail.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadLegalActionLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 8);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 8);
        entities.LegalActionDetail.EndDate = db.GetNullableDate(reader, 9);
        entities.LegalActionDetail.EffectiveDate = db.GetDate(reader, 10);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 11);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 12);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 13);
        entities.CsePerson.Type1 = db.GetString(reader, 14);
        entities.CsePerson.Populated = true;
        entities.LegalActionDetail.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.LegalAction.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 7);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 8);
        entities.Tribunal.Name = db.GetString(reader, 9);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 10);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 11);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 12);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 13);
        entities.Tribunal.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunalFips()
  {
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;
    entities.LegalAction.Populated = false;

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
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 6);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.Tribunal.Identifier = db.GetInt32(reader, 7);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 8);
        entities.Tribunal.Name = db.GetString(reader, 9);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 10);
        entities.Fips.Location = db.GetInt32(reader, 10);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 11);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 12);
        entities.Fips.County = db.GetInt32(reader, 12);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 13);
        entities.Fips.State = db.GetInt32(reader, 13);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 14);
        entities.Fips.StateAbbreviation = db.GetString(reader, 15);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 16);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadTribunalFips()
  {
    entities.Tribunal.Populated = false;
    entities.Fips.Populated = false;

    return ReadEach("ReadTribunalFips",
      (db, command) =>
      {
        db.
          SetString(command, "stateAbbreviation", export.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", export.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 0);
        entities.Tribunal.Name = db.GetString(reader, 1);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.Tribunal.Identifier = db.GetInt32(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Fips.County = db.GetInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Fips.State = db.GetInt32(reader, 6);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 7);
        entities.Fips.StateAbbreviation = db.GetString(reader, 8);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 9);
        entities.Tribunal.Populated = true;
        entities.Fips.Populated = true;

        return true;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of DlgflwSelectedLegalAction.
    /// </summary>
    [JsonPropertyName("dlgflwSelectedLegalAction")]
    public LegalAction DlgflwSelectedLegalAction
    {
      get => dlgflwSelectedLegalAction ??= new();
      set => dlgflwSelectedLegalAction = value;
    }

    /// <summary>
    /// A value of ZzzImportDlgflwSelected.
    /// </summary>
    [JsonPropertyName("zzzImportDlgflwSelected")]
    public ServiceProvider ZzzImportDlgflwSelected
    {
      get => zzzImportDlgflwSelected ??= new();
      set => zzzImportDlgflwSelected = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
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

    /// <summary>
    /// A value of HiddenStdNumbGenerated.
    /// </summary>
    [JsonPropertyName("hiddenStdNumbGenerated")]
    public Common HiddenStdNumbGenerated
    {
      get => hiddenStdNumbGenerated ??= new();
      set => hiddenStdNumbGenerated = value;
    }

    /// <summary>
    /// A value of FromLrolCsePerson.
    /// </summary>
    [JsonPropertyName("fromLrolCsePerson")]
    public CsePerson FromLrolCsePerson
    {
      get => fromLrolCsePerson ??= new();
      set => fromLrolCsePerson = value;
    }

    /// <summary>
    /// A value of FromLrolCaseRole.
    /// </summary>
    [JsonPropertyName("fromLrolCaseRole")]
    public CaseRole FromLrolCaseRole
    {
      get => fromLrolCaseRole ??= new();
      set => fromLrolCaseRole = value;
    }

    /// <summary>
    /// A value of FromLrolCase.
    /// </summary>
    [JsonPropertyName("fromLrolCase")]
    public Case1 FromLrolCase
    {
      get => fromLrolCase ??= new();
      set => fromLrolCase = value;
    }

    /// <summary>
    /// A value of PrintFxKey.
    /// </summary>
    [JsonPropertyName("printFxKey")]
    public TextWorkArea PrintFxKey
    {
      get => printFxKey ??= new();
      set => printFxKey = value;
    }

    private Common promptListAltAddrLoc;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private Document document;
    private Code code;
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
    private LegalAction dlgflwSelectedLegalAction;
    private ServiceProvider zzzImportDlgflwSelected;
    private CodeValue dlgflwSelectedCodeValue;
    private Fips dlgflwSelectedFips;
    private Tribunal dlgflwSelectedTribunal;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Office ospEnforcingOffice;
    private OfficeServiceProvider ospEnforcingOfficeServiceProvider;
    private ServiceProvider ospEnforcingServiceProvider;
    private Common hiddenPrevUserAction;
    private CsePersonsWorkSet altBillingLocn;
    private Common promptRespCountry;
    private Common promptInitCountry;
    private Common hiddenStdNumbGenerated;
    private CsePerson fromLrolCsePerson;
    private CaseRole fromLrolCaseRole;
    private Case1 fromLrolCase;
    private TextWorkArea printFxKey;
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
    /// A value of DlgflwAsinHeaderObject.
    /// </summary>
    [JsonPropertyName("dlgflwAsinHeaderObject")]
    public SpTextWorkArea DlgflwAsinHeaderObject
    {
      get => dlgflwAsinHeaderObject ??= new();
      set => dlgflwAsinHeaderObject = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of ZdelESiraj.
    /// </summary>
    [JsonPropertyName("zdelESiraj")]
    public Document ZdelESiraj
    {
      get => zdelESiraj ??= new();
      set => zdelESiraj = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of DocumentResponseCommon1.
    /// </summary>
    [JsonPropertyName("documentResponseCommon1")]
    public Common DocumentResponseCommon1
    {
      get => documentResponseCommon1 ??= new();
      set => documentResponseCommon1 = value;
    }

    /// <summary>
    /// A value of DocumentResponseCommon2.
    /// </summary>
    [JsonPropertyName("documentResponseCommon2")]
    public Common DocumentResponseCommon2
    {
      get => documentResponseCommon2 ??= new();
      set => documentResponseCommon2 = value;
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
    /// A value of HiddenPrevUserAction.
    /// </summary>
    [JsonPropertyName("hiddenPrevUserAction")]
    public Common HiddenPrevUserAction
    {
      get => hiddenPrevUserAction ??= new();
      set => hiddenPrevUserAction = value;
    }

    /// <summary>
    /// A value of ValidWithCodeValue.
    /// </summary>
    [JsonPropertyName("validWithCodeValue")]
    public CodeValue ValidWithCodeValue
    {
      get => validWithCodeValue ??= new();
      set => validWithCodeValue = value;
    }

    /// <summary>
    /// A value of ValidWithCode.
    /// </summary>
    [JsonPropertyName("validWithCode")]
    public Code ValidWithCode
    {
      get => validWithCode ??= new();
      set => validWithCode = value;
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

    /// <summary>
    /// A value of HiddenStdNumbGenerated.
    /// </summary>
    [JsonPropertyName("hiddenStdNumbGenerated")]
    public Common HiddenStdNumbGenerated
    {
      get => hiddenStdNumbGenerated ??= new();
      set => hiddenStdNumbGenerated = value;
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

    /// <summary>
    /// A value of EiwoSelection.
    /// </summary>
    [JsonPropertyName("eiwoSelection")]
    public Common EiwoSelection
    {
      get => eiwoSelection ??= new();
      set => eiwoSelection = value;
    }

    /// <summary>
    /// A value of ToLaisCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("toLaisCsePersonsWorkSet")]
    public CsePersonsWorkSet ToLaisCsePersonsWorkSet
    {
      get => toLaisCsePersonsWorkSet ??= new();
      set => toLaisCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ToLaisCase.
    /// </summary>
    [JsonPropertyName("toLaisCase")]
    public Case1 ToLaisCase
    {
      get => toLaisCase ??= new();
      set => toLaisCase = value;
    }

    /// <summary>
    /// A value of PrintFxKey.
    /// </summary>
    [JsonPropertyName("printFxKey")]
    public TextWorkArea PrintFxKey
    {
      get => printFxKey ??= new();
      set => printFxKey = value;
    }

    /// <summary>
    /// A value of FromLrolCsePerson.
    /// </summary>
    [JsonPropertyName("fromLrolCsePerson")]
    public CsePerson FromLrolCsePerson
    {
      get => fromLrolCsePerson ??= new();
      set => fromLrolCsePerson = value;
    }

    /// <summary>
    /// A value of FromLrolCaseRole.
    /// </summary>
    [JsonPropertyName("fromLrolCaseRole")]
    public CaseRole FromLrolCaseRole
    {
      get => fromLrolCaseRole ??= new();
      set => fromLrolCaseRole = value;
    }

    /// <summary>
    /// A value of FromLrolCase.
    /// </summary>
    [JsonPropertyName("fromLrolCase")]
    public Case1 FromLrolCase
    {
      get => fromLrolCase ??= new();
      set => fromLrolCase = value;
    }

    /// <summary>
    /// A value of FromLact.
    /// </summary>
    [JsonPropertyName("fromLact")]
    public WorkArea FromLact
    {
      get => fromLact ??= new();
      set => fromLact = value;
    }

    private SpTextWorkArea dlgflwAsinHeaderObject;
    private Common promptListAltAddrLoc;
    private CodeValue actionTaken;
    private FipsTribAddress fipsTribAddress;
    private Document document;
    private Document zdelESiraj;
    private PetitionerRespondentDetails petitionerRespondentDetails;
    private Common promptListActionsTaken;
    private Code code;
    private Common displayActiveCasesOnly;
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
    private AbendData abendData;
    private Common documentResponseCommon1;
    private Common documentResponseCommon2;
    private NextTranInfo hiddenNextTranInfo;
    private Security2 hiddenSecurity1;
    private Array<HiddenSecurityGroup> hiddenSecurity;
    private Office ospEnforcingOffice;
    private OfficeServiceProvider ospEnforcingOfficeServiceProvider;
    private ServiceProvider ospEnforcingServiceProvider;
    private Common hiddenPrevUserAction;
    private CodeValue validWithCodeValue;
    private Code validWithCode;
    private CsePersonsWorkSet altBillingLocn;
    private Common promptRespCountry;
    private Common promptInitCountry;
    private Common hiddenStdNumbGenerated;
    private Common legalActionFlow;
    private Common eiwoSelection;
    private CsePersonsWorkSet toLaisCsePersonsWorkSet;
    private Case1 toLaisCase;
    private TextWorkArea printFxKey;
    private CsePerson fromLrolCsePerson;
    private CaseRole fromLrolCaseRole;
    private Case1 fromLrolCase;
    private WorkArea fromLact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PersonCase.
    /// </summary>
    [JsonPropertyName("personCase")]
    public Common PersonCase
    {
      get => personCase ??= new();
      set => personCase = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of DupFound.
    /// </summary>
    [JsonPropertyName("dupFound")]
    public Common DupFound
    {
      get => dupFound ??= new();
      set => dupFound = value;
    }

    /// <summary>
    /// A value of CompileNumber.
    /// </summary>
    [JsonPropertyName("compileNumber")]
    public LegalAction CompileNumber
    {
      get => compileNumber ??= new();
      set => compileNumber = value;
    }

    /// <summary>
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public Common Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of CompareDateFiled.
    /// </summary>
    [JsonPropertyName("compareDateFiled")]
    public DateWorkArea CompareDateFiled
    {
      get => compareDateFiled ??= new();
      set => compareDateFiled = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of Multipayor.
    /// </summary>
    [JsonPropertyName("multipayor")]
    public Common Multipayor
    {
      get => multipayor ??= new();
      set => multipayor = value;
    }

    /// <summary>
    /// A value of ConvertId.
    /// </summary>
    [JsonPropertyName("convertId")]
    public TextWorkArea ConvertId
    {
      get => convertId ??= new();
      set => convertId = value;
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
    /// A value of NullTribunal.
    /// </summary>
    [JsonPropertyName("nullTribunal")]
    public Tribunal NullTribunal
    {
      get => nullTribunal ??= new();
      set => nullTribunal = value;
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
    /// A value of NumberOfTribunalsFound.
    /// </summary>
    [JsonPropertyName("numberOfTribunalsFound")]
    public Common NumberOfTribunalsFound
    {
      get => numberOfTribunalsFound ??= new();
      set => numberOfTribunalsFound = value;
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
    /// A value of PrintRetCode.
    /// </summary>
    [JsonPropertyName("printRetCode")]
    public WorkArea PrintRetCode
    {
      get => printRetCode ??= new();
      set => printRetCode = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
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
    /// A value of DuplicateCaptForLeact.
    /// </summary>
    [JsonPropertyName("duplicateCaptForLeact")]
    public Common DuplicateCaptForLeact
    {
      get => duplicateCaptForLeact ??= new();
      set => duplicateCaptForLeact = value;
    }

    /// <summary>
    /// A value of ReturnedFromCaptAftAdd.
    /// </summary>
    [JsonPropertyName("returnedFromCaptAftAdd")]
    public Common ReturnedFromCaptAftAdd
    {
      get => returnedFromCaptAftAdd ??= new();
      set => returnedFromCaptAftAdd = value;
    }

    /// <summary>
    /// A value of ZdelLocalValidStandardNumber.
    /// </summary>
    [JsonPropertyName("zdelLocalValidStandardNumber")]
    public Common ZdelLocalValidStandardNumber
    {
      get => zdelLocalValidStandardNumber ??= new();
      set => zdelLocalValidStandardNumber = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of FiledDateChanged.
    /// </summary>
    [JsonPropertyName("filedDateChanged")]
    public Common FiledDateChanged
    {
      get => filedDateChanged ??= new();
      set => filedDateChanged = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalAction Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Local1StLegActCourtCaseNo.
    /// </summary>
    [JsonPropertyName("local1StLegActCourtCaseNo")]
    public Common Local1StLegActCourtCaseNo
    {
      get => local1StLegActCourtCaseNo ??= new();
      set => local1StLegActCourtCaseNo = value;
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
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
    /// A value of NoOfLegalActionsFound.
    /// </summary>
    [JsonPropertyName("noOfLegalActionsFound")]
    public Common NoOfLegalActionsFound
    {
      get => noOfLegalActionsFound ??= new();
      set => noOfLegalActionsFound = value;
    }

    /// <summary>
    /// A value of NullNextTranInfo.
    /// </summary>
    [JsonPropertyName("nullNextTranInfo")]
    public NextTranInfo NullNextTranInfo
    {
      get => nullNextTranInfo ??= new();
      set => nullNextTranInfo = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
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

    /// <summary>
    /// A value of ReturnedFromDiscAftAdd.
    /// </summary>
    [JsonPropertyName("returnedFromDiscAftAdd")]
    public Common ReturnedFromDiscAftAdd
    {
      get => returnedFromDiscAftAdd ??= new();
      set => returnedFromDiscAftAdd = value;
    }

    private Common personCase;
    private Common apCommon;
    private Common dupFound;
    private LegalAction compileNumber;
    private Common verify;
    private DateWorkArea compareDateFiled;
    private DateWorkArea nullDateWorkArea;
    private Common multipayor;
    private TextWorkArea convertId;
    private Fips fips;
    private Tribunal nullTribunal;
    private Tribunal tribunal;
    private Common numberOfTribunalsFound;
    private Common multTribError;
    private Common tribalTribunal;
    private InterstateRequestHistory specific;
    private Common screenIdentification;
    private WorkArea printRetCode;
    private Common position;
    private DateWorkArea currentDate;
    private Common duplicateCaptForLeact;
    private Common returnedFromCaptAftAdd;
    private Common zdelLocalValidStandardNumber;
    private DateWorkArea initialisedToZeros;
    private Common filedDateChanged;
    private LegalAction search;
    private Common local1StLegActCourtCaseNo;
    private DateWorkArea max;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common totalPromptsSelected;
    private Common noOfLegalActionsFound;
    private NextTranInfo nullNextTranInfo;
    private CsePerson apCsePerson;
    private Common printProcess;
    private AaWork lastChar;
    private Common returnedFromDiscAftAdd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of GuidelineDeviations.
    /// </summary>
    [JsonPropertyName("guidelineDeviations")]
    public GuidelineDeviations GuidelineDeviations
    {
      get => guidelineDeviations ??= new();
      set => guidelineDeviations = value;
    }

    /// <summary>
    /// A value of Lops.
    /// </summary>
    [JsonPropertyName("lops")]
    public LegalActionCaseRole Lops
    {
      get => lops ??= new();
      set => lops = value;
    }

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
    /// A value of Cs.
    /// </summary>
    [JsonPropertyName("cs")]
    public LegalActionDetail Cs
    {
      get => cs ??= new();
      set => cs = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
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
    /// A value of EpLegalActionPerson.
    /// </summary>
    [JsonPropertyName("epLegalActionPerson")]
    public LegalActionPerson EpLegalActionPerson
    {
      get => epLegalActionPerson ??= new();
      set => epLegalActionPerson = value;
    }

    /// <summary>
    /// A value of EpCsePerson.
    /// </summary>
    [JsonPropertyName("epCsePerson")]
    public CsePerson EpCsePerson
    {
      get => epCsePerson ??= new();
      set => epCsePerson = value;
    }

    /// <summary>
    /// A value of EpLegalActionDetail.
    /// </summary>
    [JsonPropertyName("epLegalActionDetail")]
    public LegalActionDetail EpLegalActionDetail
    {
      get => epLegalActionDetail ??= new();
      set => epLegalActionDetail = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public LegalAction Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Iclass.
    /// </summary>
    [JsonPropertyName("iclass")]
    public LegalAction Iclass
    {
      get => iclass ??= new();
      set => iclass = value;
    }

    /// <summary>
    /// A value of Multipayor.
    /// </summary>
    [JsonPropertyName("multipayor")]
    public CsePerson Multipayor
    {
      get => multipayor ??= new();
      set => multipayor = value;
    }

    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of ExistingAltBillLocn.
    /// </summary>
    [JsonPropertyName("existingAltBillLocn")]
    public CsePerson ExistingAltBillLocn
    {
      get => existingAltBillLocn ??= new();
      set => existingAltBillLocn = value;
    }

    /// <summary>
    /// A value of CourtCaption.
    /// </summary>
    [JsonPropertyName("courtCaption")]
    public CourtCaption CourtCaption
    {
      get => courtCaption ??= new();
      set => courtCaption = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    private GuidelineDeviations guidelineDeviations;
    private LegalActionCaseRole lops;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionDetail cs;
    private Obligation obligation;
    private ObligationType obligationType;
    private CaseRole caseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private CsePerson apCsePerson;
    private CaseRole apCaseRole;
    private Case1 case1;
    private LegalActionPerson epLegalActionPerson;
    private CsePerson epCsePerson;
    private LegalActionDetail epLegalActionDetail;
    private LegalAction previous;
    private LegalAction iclass;
    private CsePerson multipayor;
    private LegalActionAssigment legalActionAssigment;
    private CsePerson csePerson;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private Document document;
    private CsePerson existingAltBillLocn;
    private CourtCaption courtCaption;
    private FipsTribAddress fipsTribAddress;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction;
    private Discovery discovery;
  }
#endregion
}

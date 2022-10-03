// Program: FN_LDSP_LST_MTN_DISB_SUPPR, ID: 371753782, model: 746.
// Short name: SWELDSPP
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
/// A program: FN_LDSP_LST_MTN_DISB_SUPPR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnLdspLstMtnDisbSuppr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_LDSP_LST_MTN_DISB_SUPPR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnLdspLstMtnDisbSuppr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnLdspLstMtnDisbSuppr.
  /// </summary>
  public FnLdspLstMtnDisbSuppr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ------------	-----------	
    // ---------------------------------------------
    // ??/??/??  ?????????			Initial Development
    // 12/11/96  R. Marchman			Add new security and next tran
    // 11/23/97  Gary McGirr			Make Coll code shorter, add a release date,
    // 					fix group view display problem, plus other
    // 					cosmetic changes.
    // 11/11/98  RK		Phase II	Changes made as part of Phase II.
    // 					1. Put in flow to LPSP
    // 					2. Enable the prompt to NAME to work
    // 					3. The Released record should show the create
    // 					   date of when the disbursment was actually
    // 					   linked to a SUPP code and not when it was
    // 					   associated to the RELE code. No table
    // 					   values are changed, just the value to the
    // 					   screen is switched.
    // 					4. Can only Re-Suppress a Disbursement on the
    // 					   same day it was released.
    // 					5. Protect the Amount field on screen.
    // 					6. For records that are Released put in the
    // 					   ID of the person who released it, thus
    // 					   over-rideing the person who set up the
    // 					   suppression to begin with. This is again
    // 					   only for the screen display, no records
    // 					   are actually changed by doing this.
    // 04/20/99  RK		Phase II	Changed the following for phase II
    // 					integration testing
    // 					1. If an invalid CSE Person number is entered
    // 					   and a flow to another screen is requested
    // 					   (pf key or NEXT ) then an error message
    // 					   will show saying that the screen must be
    // 					   cleared on the invalid number prior to
    // 					   flowing.
    // 					2. Underline the Payee prompt field
    // 					3. 'S' shouldn't remain in select field upon
    // 					   return from NAME.
    // 					4. Payee number should display in blue not
    // 					   green
    // 					5. Should be able to flow to CSUP or LPSP
    // 					   with no CSE Person value entered.
    // 					6. Should be able to flow to CSUP or LPSP
    // 					   after a display without selecting a
    // 					   record.
    // 11/08/99  SWSRKXD	PR#79043	1. Change screen fields to display correct
    // 					   color and highlighting.
    // 					2. Display informational message after
    // 					   successful release and suppress actions.
    // 					3. Flag released/suppressed records in the
    // 					   list with an *
    // 					4. Clear Release Date field for suppressed
    // 					   records
    // 					5. Set Release Date to Current for released
    // 					   records
    // 					6. Set correct exit state when invalid cse
    // 					   person is entered on display action.
    // 02/17/00  SWSRKXD	PR#87896	Performance change - Add Payee qualification
    // 					to READ EACHes.
    // 05/22/00  ???????	PRWORA #164-L	Flag URA disbursements with an 'X' at the
    // end
    // 					of disb type. Also add code to maintain URA
    // 					bucket on Monthly_Obligee_Summary.
    // 06/06/00  ???????	PR#96599 	No selection necessary for flow to LPSP and
    // 					CSUP.
    // 10/17/00  Fangman 	PR#98039	Added column for new D suppression rule.
    // 02/22/01  Fangman 	PR#111602	Added 2 new filters (end date & reference
    // 					number).
    // 06/11/01  V.Madhira	PR# 120251	Commented code. In some cases it is 
    // possible
    // 					not to have disb_supp_stat_hist record.
    // 11/07/01  K Doshi	PR131584	Fix screen help Id problem.
    // 01/07/02  Fangman	WR000235	PSUM redesign.  Added new columns & deleted
    // 					unused columns.  Removed code for XA URA & XC
    // 					URA because they are no longer used & none
    // 					are suppressed.
    // 09/04/07  G. Pan	PR197953	Created new exit state -
    // 					FN0000_DISB_SEARCH_DATE_REDUCE
    // 					Changed group view from 50 to 120
    // 03/27/08  M. Fan	PR330408
    // 			(CQ3506)	Changed to let the procedure step run through
    // 					the security CAB for all actions.
    // 07/02/19  GVandy	CQ65423		Add System Suppressions and Suppression Type 
    // filter.
    // -------------------------------------------------------------------------------------
    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    // **** end   group A ****
    // Move all IMPORTs to EXPORTs.
    export.CsePerson.Number = import.CsePerson.Number;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.SearchStarting.ProcessDate = import.SearchStarting.ProcessDate;
    export.SearchEnding.ProcessDate = import.SearchEnding.ProcessDate;
    export.SearchRefNbr.ReferenceNumber = import.SearchRefNbr.ReferenceNumber;
    export.HiddenDisplayed.Number = import.HiddenDisplayed.Number;
    export.PromptPayee.SelectChar = import.PromptPayee.SelectChar;
    export.StorePreviousDisbursementTransaction.ReferenceNumber =
      import.StorePreviousDisbursementTransaction.ReferenceNumber;
    export.StorePreviousCommon.SelectChar =
      import.StorePreviousCommon.SelectChar;
    export.Search.Type1 = import.Search.Type1;

    if (!IsEmpty(import.Search.Type1))
    {
      export.SuppressionType.Description = import.SuppressionType.Description;
    }

    export.PromptSuppressionType.SelectChar =
      import.PromptSuppressionType.SelectChar;

    // *************************
    // Get the three status's.
    // *************************
    UseFnHardcodedDisbLessDisbType();

    // ****************************
    // If flowing back from NAME
    // ****************************
    if (Equal(global.Command, "RETCSENO"))
    {
      export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      export.PromptPayee.SelectChar = "+";
      global.Command = "DISPLAY";
    }

    UseCabSetMaximumDiscontinueDate();

    if (IsEmpty(export.CsePerson.Number))
    {
      if (Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        export.CsePersonsWorkSet.FormattedName = "";
        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }
    else
    {
      local.LeftPadding.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.LeftPadding.Text10;
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
      UseSiReadCsePerson();
      export.CsePersonsWorkSet.Number = export.CsePerson.Number;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ****************************************************************
      // If a flow is requested while an invalid CSE Person Number is on screen 
      // then error off. rk 4/20/99.
      // ****************************************************************
      if (Equal(global.Command, "CSUP") || Equal(global.Command, "PSUP") || Equal
        (global.Command, "LPSP") || Equal(global.Command, "PACC") || !
        IsEmpty(import.Standard.NextTransaction))
      {
        ExitState = "FN0000_NO_FLOW_ALLWD_INV_PERSON";
      }
      else if (Equal(global.Command, "DISPLAY"))
      {
        // -----------------------------------------------------------------
        // 11/08/99 SWSRKXD PR#79043
        // Set correct exit state for  display action.
        // -----------------------------------------------------------------
        ExitState = "OE0026_INVALID_CSE_PERSON_NO";
      }

      var field = GetField(export.CsePerson, "number");

      field.Error = true;

      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumberObligee = export.CsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      if (!import.Import1.IsEmpty)
      {
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Update.Common.SelectChar =
            import.Import1.Item.Common.SelectChar;
          export.Export1.Update.DisbursementTransaction.Assign(
            import.Import1.Item.DisbursementTransaction);
          MoveDisbursementStatus(import.Import1.Item.DisbursementStatus,
            export.Export1.Update.DisbursementStatus);
          export.Export1.Update.DisbursementType.Code =
            import.Import1.Item.DisbursementType.Code;
          export.Export1.Update.DisbType.Text10 =
            import.Import1.Item.DisbType.Text10;
          export.Export1.Update.DisbursementStatusHistory.Assign(
            import.Import1.Item.DisbursementStatusHistory);
          export.Export1.Update.CollSupp.Code =
            import.Import1.Item.CollSupp.Code;
          export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
            import.Import1.Item.DisbSuppressionStatusHistory.LastUpdatedBy;
          export.Export1.Update.AutoSupp.Text1 =
            import.Import1.Item.AutoSupp.Text1;
          export.Export1.Update.PersonSupp.Text1 =
            import.Import1.Item.PersonSupp.Text1;
          export.Export1.Update.AddrSupp.Text1 =
            import.Import1.Item.AddrSupp.Text1;
          export.Export1.Update.DodSupp.Text1 =
            import.Import1.Item.DodSupp.Text1;
          export.Export1.Update.SuppTypes.Text6 =
            import.Import1.Item.SuppTypes.Text6;

          // ---------------------------------
          // 11/09/99 SWSRKXD PR#79043
          // Clear * from previous updates.
          // ---------------------------------
          if (AsChar(import.Import1.Item.Common.SelectChar) == '*')
          {
            export.Export1.Update.Common.SelectChar = "";
          }

          export.Export1.Next();
        }
      }
    }

    if (Equal(global.Command, "RLSE") || Equal(global.Command, "SUPPRESS"))
    {
      if (!Equal(import.HiddenDisplayed.Number, export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    // -----------------------------------------------------------------------------
    // 03/27/2008 M. Fan PR330408 (CQ3506)
    // Moved the following block of codes for the IF statement to where it 
    // should be (i.e. after calling the security CAB and before Main CASE OF
    // COMMAND.
    // -----------------------------------------------------------------------------
    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      export.CsePerson.Number = export.Hidden.CsePersonNumberObligee ?? Spaces
        (10);
      UseScCabNextTranGet();

      // ****
      global.Command = "DISPLAY";

      return;
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
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    // 03/21/2008 M. Fan PR330408 (CQ3506)  Commented out following lines and 
    // let the security CAB validate all  actions.
    if (Equal(global.Command, "RETCDVL"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "RETCDVL"))
    {
      if (!IsEmpty(import.FromCdvl.Cdvalue))
      {
        export.Search.Type1 = import.FromCdvl.Cdvalue;
        export.SuppressionType.Description = import.FromCdvl.Description;
      }

      export.PromptSuppressionType.SelectChar = "";
      global.Command = "DISPLAY";
    }

    // **** end   group C ****
    // -----------------------------------------------------------------------------
    // 03/27/2008 M. Fan PR330408 (CQ3506) ---- Started ----
    // Copied the block of codes.
    // -----------------------------------------------------------------------------
    // ****************************************************************
    // PR#96599 - 6/6/2000
    // Do not perform this check for csup and lpsp commands.
    // ****************************************************************
    if (Equal(global.Command, "RLSE") || Equal(global.Command, "SUPPRESS"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        switch(AsChar(export.Export1.Item.Common.SelectChar))
        {
          case ' ':
            // **************************************************
            // To handle passthrus and URA disbursements.
            // **************************************************
            if (Equal(export.Export1.Item.DisbursementType.Code, "PT"))
            {
              break;
            }

            if (Equal(local.PreviousDisbursementTransaction.ReferenceNumber,
              export.Export1.Item.DisbursementTransaction.ReferenceNumber) && AsChar
              (local.PreviousCommon.SelectChar) == 'S')
            {
              ExitState = "FN0000_SELECT_ALL_REFERENCE_NBRS";

              return;
            }

            local.PreviousCommon.SelectChar =
              export.Export1.Item.Common.SelectChar;
            local.PreviousDisbursementTransaction.ReferenceNumber =
              export.Export1.Item.DisbursementTransaction.ReferenceNumber ?? ""
              ;

            break;
          case 'S':
            // **************************************************
            // To handle passthrus and URA disbursements.
            // **************************************************
            if (Equal(export.Export1.Item.DisbursementType.Code, "PT"))
            {
              local.SelectFound.Flag = "Y";
              export.Selected.Code = export.Export1.Item.CollSupp.Code;

              break;
            }

            if (Equal(local.PreviousDisbursementTransaction.ReferenceNumber,
              export.Export1.Item.DisbursementTransaction.ReferenceNumber) && AsChar
              (local.PreviousCommon.SelectChar) != 'S')
            {
              ExitState = "FN0000_SELECT_ALL_REFERENCE_NBRS";

              return;
            }
            else
            {
              local.PreviousCommon.SelectChar =
                export.Export1.Item.Common.SelectChar;
              local.PreviousDisbursementTransaction.ReferenceNumber =
                export.Export1.Item.DisbursementTransaction.ReferenceNumber ?? ""
                ;
              local.SelectFound.Flag = "Y";
              export.Selected.Code = export.Export1.Item.CollSupp.Code;
            }

            break;
          default:
            var field = GetField(export.Export1.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            break;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(global.Command, "RLSE") || Equal(global.Command, "SUPPRESS"))
      {
        if (AsChar(local.SelectFound.Flag) != 'Y')
        {
          ExitState = "ZD_ACO_NE0000_NO_SELECTN_MADE_2";

          return;
        }
      }
    }

    // -----------------------------------------------------------------------------
    // 03/27/2008 M. Fan PR330408 (CQ3506) ---- Ended ----
    // Copied the block of codes.
    // -----------------------------------------------------------------------------
    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "LIST":
        local.PromptCount.Count = 0;

        switch(AsChar(export.PromptSuppressionType.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptSuppressionType, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.PromptPayee.SelectChar))
        {
          case 'S':
            ++local.PromptCount.Count;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptPayee, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(local.PromptCount.Count)
        {
          case 0:
            var field1 = GetField(export.PromptSuppressionType, "selectChar");

            field1.Error = true;

            var field2 = GetField(export.PromptPayee, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 1:
            break;
          default:
            if (!IsEmpty(export.PromptSuppressionType.SelectChar))
            {
              var field = GetField(export.PromptSuppressionType, "selectChar");

              field.Error = true;
            }

            if (!IsEmpty(export.PromptPayee.SelectChar))
            {
              var field = GetField(export.PromptPayee, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            return;
        }

        if (AsChar(export.PromptSuppressionType.SelectChar) == 'S')
        {
          export.ToCdvl.CodeName = "SUPPRESSION TYPE";
          ExitState = "ECO_LNK_TO_CDVL";
        }

        if (AsChar(export.PromptPayee.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }

        break;
      case "RLSE":
        ExitState = "ACO_NN0000_ALL_OK";

        // Set status to release
        // ****************************************************************
        // If the Status is already Released then another Release request is in 
        // error.
        // ****************************************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && export
            .Export1.Item.DisbursementStatus.SystemGeneratedIdentifier != local
            .Suppressed.SystemGeneratedIdentifier)
          {
            var field =
              GetField(export.Export1.Item.DisbursementStatus, "code");

            field.Error = true;

            ExitState = "FN0000_DISB_STAT_IN_WRONG_STATE";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // -- 07/02/19  GVandy  CQ65423  Only users with the "DEVELOPERS" 
          // security profile
          //    (i.e. CSE Automation Unit) are permitted to release System 
          // Suppressions.
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && (
            AsChar(export.Export1.Item.DisbursementStatusHistory.
              SuppressionReason) == 'Y' || AsChar
            (export.Export1.Item.DisbursementStatusHistory.SuppressionReason) ==
              'Z'))
          {
            if (!ReadServiceProviderProfile())
            {
              var field =
                GetField(export.Export1.Item.DisbursementStatus, "code");

              field.Error = true;

              ExitState = "FN0000_CANT_RELEASE_SYSTEM_SUPPR";
            }

            break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!ReadDisbursementStatus1())
        {
          ExitState = "FN0000_DISB_STATUS_NF";

          return;
        }

        // @@@@
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            // ****
            if (Equal(export.Export1.Item.DisbursementType.Code, "PT"))
            {
              // -----------------------------------------------------------------
              // 06/20/2001 SWSRVXM PR#121266
              // Performance change - Add Payee qualification to READ.
              // -----------------------------------------------------------------
              if (ReadDisbursementStatusDisbursementStatusHistory1())
              {
                // 07/02/19  GVandy  CQ65423  Check for System Suppressions.
                // --Determine if a system suppression (AR is deceased or AR has
                // no address) should be applied to the disbursement.
                UseFnCheckForSystemSuppression();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.DisbSuppressionStatusHistory.Assign(local.Hold);

                if (AsChar(export.Export1.Item.DisbursementStatusHistory.
                  SuppressionReason) == 'Y' || AsChar
                  (export.Export1.Item.DisbursementStatusHistory.
                    SuppressionReason) == 'Z')
                {
                  // --A system suppression is being released.  Override 
                  // whatever was returned from the
                  //   check_for_system_suppression cab so that we don't system 
                  // suppress the disbursement
                  //   again.
                  local.DisbSuppressionStatusHistory.Type1 = "";
                }

                // --Either Release the disbursement or change it to be system 
                // suppressed depending on the
                //   returned info from fn_check_for_system_suppression.
                if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
                {
                  // --No system suppressions are in force.  Release the 
                  // disbursement.
                  UseFnChangeDisbursementStatus();

                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    -entities.DisbursementTransaction.Amount;
                  local.MonthlyObligeeSummary.PassthruAmount =
                    entities.DisbursementTransaction.Amount;
                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  MoveDisbursementStatus(entities.New1,
                    export.Export1.Update.DisbursementStatus);
                  export.Export1.Update.DisbSuppressionStatusHistory.
                    LastUpdatedBy = global.UserId;

                  // --------------------------------------------------
                  // 11/08/99  SWSRKXD  PR#79043
                  // Set Release Date to current. Flag processed record.
                  // --------------------------------------------------
                  export.Export1.Update.DisbursementStatusHistory.
                    EffectiveDate = Now().Date;
                  export.Export1.Update.Common.SelectChar = "*";
                }
                else
                {
                  // -- DO NOT RELEASE THE DISBURSEMENT.
                  // -- Change the disbursement to indicate it is system 
                  // suppressed.
                  try
                  {
                    UpdateDisbursementStatusHistory();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  switch(AsChar(local.DisbSuppressionStatusHistory.Type1))
                  {
                    case 'Y':
                      export.Export1.Update.DodSupp.Text1 = "Y";

                      break;
                    case 'Z':
                      export.Export1.Update.AddrSupp.Text1 = "Z";

                      break;
                    default:
                      break;
                  }

                  export.Export1.Update.SuppTypes.Text6 =
                    export.Export1.Item.PersonSupp.Text1 + Substring
                    (export.Export1.Item.CollSupp.Code,
                    CollectionType.Code_MaxLength, 1, 1) + export
                    .Export1.Item.AutoSupp.Text1 + export
                    .Export1.Item.DupPmtSupp.Text1 + export
                    .Export1.Item.AddrSupp.Text1 + export
                    .Export1.Item.DodSupp.Text1;

                  // --Raise the system suppression event.
                  local.DisbursementStatusHistory.SuppressionReason =
                    local.DisbSuppressionStatusHistory.Type1;
                  local.DisbursementStatusHistory.ReasonText =
                    local.DisbSuppressionStatusHistory.ReasonText ?? "";
                  UseFnRaiseSystemSupprEvent();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  export.Export1.Update.Common.SelectChar = "*";
                }
              }
              else
              {
                ExitState = "FN0000_DISB_SUPP_STAT_NF";

                return;
              }
            }
            else
            {
              // -----------------------------------------------------------------
              // Disbursements could be rolled up together.
              // Release/Suppress actions need to update all disb_tran
              // records that make up this line item.
              // -----------------------------------------------------------------
              // -----------------------------------------------------------------
              // 2/17/2000 SWSRKXD PR#87896
              // Performance change - Add Payee qualification to READ EACHes.
              // -----------------------------------------------------------------
              foreach(var item in ReadDisbursementTransactionDisbursementStatusHistory2())
                
              {
                if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) ==
                  'D' && AsChar(local.RuleHasBeenUpdated.Flag) != 'Y')
                {
                  // --------------------------------------------------
                  // If the disbursement(s) was a "D" then end date the 
                  // suppression rule
                  // --------------------------------------------------
                  if (ReadDisbSuppressionStatusHistory())
                  {
                    try
                    {
                      UpdateDisbSuppressionStatusHistory();
                      local.RuleHasBeenUpdated.Flag = "Y";
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "FN0000_DISB_SUPP_STAT_NU_RB";

                          return;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "FN0000_DISB_SUPP_STAT_PV_RB";

                          return;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  else
                  {
                    // ------------------------------------------------------------------------------
                    // Per PR# 120251 the code is commented. In some cases it is
                    // possible not to have disb_supp_stat_hist record.
                    // -----------------------------------------------------------------------------
                  }
                }

                // 07/02/19  GVandy  CQ65423  Check for System Suppressions.
                // --Determine if a system suppression (AR is deceased or AR has
                // no address) should be applied to the disbursement.
                UseFnCheckForSystemSuppression();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.DisbSuppressionStatusHistory.Assign(local.Hold);

                if (AsChar(export.Export1.Item.DisbursementStatusHistory.
                  SuppressionReason) == 'Y' || AsChar
                  (export.Export1.Item.DisbursementStatusHistory.
                    SuppressionReason) == 'Z')
                {
                  // --A system suppression is being released.  Override 
                  // whatever was returned from the
                  //   check_for_system_suppression cab so that we don't system 
                  // suppress the disbursement
                  //   again.
                  local.DisbSuppressionStatusHistory.Type1 = "";
                }

                // --Either Release the disbursement or change it to be system 
                // suppressed depending on the
                //   returned info from fn_check_for_system_suppression.
                if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
                {
                  // --No system suppressions are in force.  Release the 
                  // disbursement.
                  UseFnChangeDisbursementStatus();

                  // -----------------------------------------------------------------
                  // 05/22/2000 - Work Order # 164
                  // Increase Excess_URA bucket on MOS by disb amount.
                  // -----------------------------------------------------------------
                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    -entities.DisbursementTransaction.Amount;

                  if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y'
                    )
                  {
                    local.MonthlyObligeeSummary.TotExcessUraAmt =
                      entities.DisbursementTransaction.Amount;
                  }
                  else
                  {
                    local.MonthlyObligeeSummary.CollectionsDisbursedToAr =
                      entities.DisbursementTransaction.Amount;
                  }

                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  MoveDisbursementStatus(entities.New1,
                    export.Export1.Update.DisbursementStatus);
                  export.Export1.Update.DisbSuppressionStatusHistory.
                    LastUpdatedBy = global.UserId;

                  // --------------------------------------------------
                  // 11/08/99  SWSRKXD  PR#79043
                  // Set Release Date to current. Flag processed record.
                  // --------------------------------------------------
                  export.Export1.Update.DisbursementStatusHistory.
                    EffectiveDate = Now().Date;
                }
                else
                {
                  // -- DO NOT RELEASE THE DISBURSEMENT.
                  // -- Change the disbursement to indicate it is system 
                  // suppressed.
                  try
                  {
                    UpdateDisbursementStatusHistory();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  switch(AsChar(local.DisbSuppressionStatusHistory.Type1))
                  {
                    case 'Y':
                      export.Export1.Update.DodSupp.Text1 = "Y";

                      break;
                    case 'Z':
                      export.Export1.Update.AddrSupp.Text1 = "Z";

                      break;
                    default:
                      break;
                  }

                  export.Export1.Update.SuppTypes.Text6 =
                    export.Export1.Item.PersonSupp.Text1 + Substring
                    (export.Export1.Item.CollSupp.Code,
                    CollectionType.Code_MaxLength, 1, 1) + export
                    .Export1.Item.AutoSupp.Text1 + export
                    .Export1.Item.DupPmtSupp.Text1 + export
                    .Export1.Item.AddrSupp.Text1 + export
                    .Export1.Item.DodSupp.Text1;
                }
              }

              export.Export1.Update.Common.SelectChar = "*";

              if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
              {
              }
              else
              {
                // --Raise the system suppression event.
                //   Doing this outside the read each so that we get only one 
                // CSLN entry per reference number.
                local.DisbursementStatusHistory.SuppressionReason =
                  local.DisbSuppressionStatusHistory.Type1;
                local.DisbursementStatusHistory.ReasonText =
                  local.DisbSuppressionStatusHistory.ReasonText ?? "";
                UseFnRaiseSystemSupprEvent();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }
          }
        }

        if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
        {
          // --------------------------------------------------
          // 11/08/99  SWSRKXD  PR#79043
          // Display message on successful release.
          // --------------------------------------------------
          ExitState = "FN0000_RELEASE_SUCCESSFUL";
        }
        else
        {
          ExitState = "FN0000_SYSTEM_SUPPRESS_APPLIED";
        }

        break;
      case "RLSEALL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // -- 07/02/19  GVandy  CQ65423  Only users with the "DEVELOPERS" 
          // security profile
          //    (i.e. CSE Automation Unit) are permitted to release System 
          // Suppressions.
          if (AsChar(export.Export1.Item.DisbursementStatusHistory.
            SuppressionReason) == 'Y' || AsChar
            (export.Export1.Item.DisbursementStatusHistory.SuppressionReason) ==
              'Z')
          {
            if (!ReadServiceProviderProfile())
            {
              var field =
                GetField(export.Export1.Item.DisbursementStatus, "code");

              field.Error = true;

              ExitState = "FN0000_CANT_RELEASE_SYSTEM_SUPPR";
            }

            break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        ExitState = "ACO_NN0000_ALL_OK";

        if (!ReadDisbursementStatus1())
        {
          ExitState = "FN0000_DISB_STATUS_NF";

          return;
        }

        // ****
        // ***************************************************************
        // Release all the Disbursments that aren't already Released
        // ***************************************************************
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (export.Export1.Item.DisbursementStatus.
            SystemGeneratedIdentifier != local
            .Suppressed.SystemGeneratedIdentifier)
          {
          }
          else
          {
            // ****
            if (Equal(export.Export1.Item.DisbursementType.Code, "PT"))
            {
              // -----------------------------------------------------------------
              // 06/20/2001 SWSRVXM PR#121266
              // Performance change - Add Payee qualification to READ.
              // -----------------------------------------------------------------
              if (ReadDisbursementStatusDisbursementStatusHistory1())
              {
                // 07/02/19  GVandy  CQ65423  Check for System Suppressions.
                // --Determine if a system suppression (AR is deceased or AR has
                // no address) should be applied to the disbursement.
                UseFnCheckForSystemSuppression();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.DisbSuppressionStatusHistory.Assign(local.Hold);

                if (AsChar(export.Export1.Item.DisbursementStatusHistory.
                  SuppressionReason) == 'Y' || AsChar
                  (export.Export1.Item.DisbursementStatusHistory.
                    SuppressionReason) == 'Z')
                {
                  // --A system suppression is being released.  Override 
                  // whatever was returned from the
                  //   check_for_system_suppression cab so that we don't system 
                  // suppress the disbursement
                  //   again.
                  local.DisbSuppressionStatusHistory.Type1 = "";
                }

                // --Either Release the disbursement or change it to be system 
                // suppressed depending on the
                //   returned info from fn_check_for_system_suppression.
                if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
                {
                  // --No system suppressions are in force.  Release the 
                  // disbursement.
                  UseFnChangeDisbursementStatus();

                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    -entities.DisbursementTransaction.Amount;
                  local.MonthlyObligeeSummary.PassthruAmount =
                    entities.DisbursementTransaction.Amount;
                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  MoveDisbursementStatus(entities.New1,
                    export.Export1.Update.DisbursementStatus);
                  export.Export1.Update.DisbSuppressionStatusHistory.
                    LastUpdatedBy = global.UserId;

                  // --------------------------------------------------
                  // 11/08/99  SWSRKXD  PR#79043
                  // Set Release Date to current. Flag processed record.
                  // --------------------------------------------------
                  export.Export1.Update.DisbursementStatusHistory.
                    EffectiveDate = Now().Date;
                  export.Export1.Update.Common.SelectChar = "*";
                }
                else
                {
                  // -- DO NOT RELEASE THE DISBURSEMENT.
                  // -- Change the disbursement to indicate it is system 
                  // suppressed.
                  try
                  {
                    UpdateDisbursementStatusHistory();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  switch(AsChar(local.DisbSuppressionStatusHistory.Type1))
                  {
                    case 'Y':
                      export.Export1.Update.DodSupp.Text1 = "Y";

                      break;
                    case 'Z':
                      export.Export1.Update.AddrSupp.Text1 = "Z";

                      break;
                    default:
                      break;
                  }

                  export.Export1.Update.SuppTypes.Text6 =
                    export.Export1.Item.PersonSupp.Text1 + Substring
                    (export.Export1.Item.CollSupp.Code,
                    CollectionType.Code_MaxLength, 1, 1) + export
                    .Export1.Item.AutoSupp.Text1 + export
                    .Export1.Item.DupPmtSupp.Text1 + export
                    .Export1.Item.AddrSupp.Text1 + export
                    .Export1.Item.DodSupp.Text1;

                  // --Raise the system suppression event.
                  local.DisbursementStatusHistory.SuppressionReason =
                    local.DisbSuppressionStatusHistory.Type1;
                  local.DisbursementStatusHistory.ReasonText =
                    local.DisbSuppressionStatusHistory.ReasonText ?? "";
                  UseFnRaiseSystemSupprEvent();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  export.Export1.Update.Common.SelectChar = "*";
                }
              }
              else
              {
                ExitState = "FN0000_DISB_SUPP_STAT_NF";

                return;
              }
            }
            else
            {
              // -----------------------------------------------------------------
              // 2/17/2000 SWSRKXD PR#87896
              // Performance change - Add Payee qualification to READ EACHes.
              // -----------------------------------------------------------------
              foreach(var item in ReadDisbursementTransactionDisbursementStatusHistory2())
                
              {
                if (AsChar(entities.DisbursementStatusHistory.SuppressionReason) ==
                  'D' && AsChar(local.RuleHasBeenUpdated.Flag) != 'Y')
                {
                  // --------------------------------------------------
                  // If the disbursement(s) was a "D" then end date the 
                  // suppression rule
                  // --------------------------------------------------
                  if (ReadDisbSuppressionStatusHistory())
                  {
                    try
                    {
                      UpdateDisbSuppressionStatusHistory();
                      local.RuleHasBeenUpdated.Flag = "Y";
                    }
                    catch(Exception e)
                    {
                      switch(GetErrorCode(e))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "FN0000_DISB_SUPP_STAT_NU_RB";

                          return;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "FN0000_DISB_SUPP_STAT_PV_RB";

                          return;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  else
                  {
                    // Continue
                  }
                }

                // 07/02/19  GVandy  CQ65423  Check for System Suppressions.
                // --Determine if a system suppression (AR is deceased or AR has
                // no address) should be applied to the disbursement.
                UseFnCheckForSystemSuppression();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                local.DisbSuppressionStatusHistory.Assign(local.Hold);

                if (AsChar(export.Export1.Item.DisbursementStatusHistory.
                  SuppressionReason) == 'Y' || AsChar
                  (export.Export1.Item.DisbursementStatusHistory.
                    SuppressionReason) == 'Z')
                {
                  // --A system suppression is being released.  Override 
                  // whatever was returned from the
                  //   check_for_system_suppression cab so that we don't system 
                  // suppress the disbursement
                  //   again.
                  local.DisbSuppressionStatusHistory.Type1 = "";
                }

                // --Either Release the disbursement or change it to be system 
                // suppressed depending on the
                //   returned info from fn_check_for_system_suppression.
                if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
                {
                  // --No system suppressions are in force.  Release the 
                  // disbursement.
                  UseFnChangeDisbursementStatus();

                  // -----------------------------------------------------------------
                  // 05/22/2000 - Work Order # 164
                  // Increase Excess_URA bucket on MOS by disb amount.
                  // -----------------------------------------------------------------
                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    -entities.DisbursementTransaction.Amount;

                  if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y'
                    )
                  {
                    local.MonthlyObligeeSummary.TotExcessUraAmt =
                      entities.DisbursementTransaction.Amount;
                  }
                  else
                  {
                    local.MonthlyObligeeSummary.CollectionsDisbursedToAr =
                      entities.DisbursementTransaction.Amount;
                  }

                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }

                  MoveDisbursementStatus(entities.New1,
                    export.Export1.Update.DisbursementStatus);
                  export.Export1.Update.DisbSuppressionStatusHistory.
                    LastUpdatedBy = global.UserId;

                  // --------------------------------------------------
                  // 11/08/99  SWSRKXD  PR#79043
                  // Set Release Date to current. Flag processed record.
                  // --------------------------------------------------
                  export.Export1.Update.DisbursementStatusHistory.
                    EffectiveDate = Now().Date;
                  export.Export1.Update.Common.SelectChar = "*";
                }
                else
                {
                  // -- DO NOT RELEASE THE DISBURSEMENT.
                  // -- Change the disbursement to indicate it is system 
                  // suppressed.
                  try
                  {
                    UpdateDisbursementStatusHistory();
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

                        return;
                      case ErrorCode.PermittedValueViolation:
                        ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

                        return;
                      case ErrorCode.DatabaseError:
                        break;
                      default:
                        throw;
                    }
                  }

                  switch(AsChar(local.DisbSuppressionStatusHistory.Type1))
                  {
                    case 'Y':
                      export.Export1.Update.DodSupp.Text1 = "Y";

                      break;
                    case 'Z':
                      export.Export1.Update.AddrSupp.Text1 = "Z";

                      break;
                    default:
                      break;
                  }

                  export.Export1.Update.SuppTypes.Text6 =
                    export.Export1.Item.PersonSupp.Text1 + Substring
                    (export.Export1.Item.CollSupp.Code,
                    CollectionType.Code_MaxLength, 1, 1) + export
                    .Export1.Item.AutoSupp.Text1 + export
                    .Export1.Item.DupPmtSupp.Text1 + export
                    .Export1.Item.AddrSupp.Text1 + export
                    .Export1.Item.DodSupp.Text1;
                }
              }

              export.Export1.Update.Common.SelectChar = "*";

              if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
              {
              }
              else
              {
                // --Raise the system suppression event.
                //   Doing this outside the read each so that we get only one 
                // CSLN entry per reference number.
                local.DisbursementStatusHistory.SuppressionReason =
                  local.DisbSuppressionStatusHistory.Type1;
                local.DisbursementStatusHistory.ReasonText =
                  local.DisbSuppressionStatusHistory.ReasonText ?? "";
                UseFnRaiseSystemSupprEvent();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }
              }
            }
          }
        }

        if (IsEmpty(local.DisbSuppressionStatusHistory.Type1))
        {
          // --------------------------------------------------
          // 11/08/99  SWSRKXD  PR#79043
          // Display message on successful release.
          // --------------------------------------------------
          ExitState = "FN0000_RELEASE_SUCCESSFUL";
        }
        else
        {
          ExitState = "FN0000_SYSTEM_SUPPRESS_APPLIED";
        }

        break;
      case "SUPPRESS":
        ExitState = "ACO_NN0000_ALL_OK";

        // Set status to Suppressed
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S' && export
            .Export1.Item.DisbursementStatus.SystemGeneratedIdentifier != local
            .Released.SystemGeneratedIdentifier)
          {
            var field =
              GetField(export.Export1.Item.DisbursementStatus, "code");

            field.Error = true;

            ExitState = "FN0000_DISB_STAT_IN_WRONG_STATE";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!ReadDisbursementStatus2())
        {
          ExitState = "FN0000_DISB_STATUS_NF";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (Equal(export.Export1.Item.DisbursementType.Code, "PT"))
            {
              if (ReadDisbursementStatusDisbursementStatusHistory2())
              {
                if (Equal(entities.DisbursementStatusHistory.EffectiveDate,
                  Now().Date))
                {
                  UseFnChangeDisbursementStatus();

                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    entities.DisbursementTransaction.Amount;
                  local.MonthlyObligeeSummary.PassthruAmount =
                    -entities.DisbursementTransaction.Amount;
                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;

                  ExitState = "FN0000_SUPPRES_AGAIN_NOT_ALLOWED";

                  return;
                }
              }
              else
              {
                ExitState = "FN0000_DISB_SUPP_STAT_NF";

                return;
              }
            }
            else
            {
              // -----------------------------------------------------------------
              // 2/17/2000 SWSRKXD PR#87896
              // Performance change - Add Payee qualification to READ EACHes.
              // -----------------------------------------------------------------
              foreach(var item in ReadDisbursementTransactionDisbursementStatusHistory1())
                
              {
                if (Equal(entities.DisbursementStatusHistory.EffectiveDate,
                  Now().Date))
                {
                  UseFnChangeDisbursementStatus();

                  // -----------------------------------------------------------------
                  // 05/22/2000 - Work Order # 164
                  // Decrease Excess_URA bucket on MOS by disb amount.
                  // -----------------------------------------------------------------
                  if (!ReadCsePersonAccount())
                  {
                    ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF_RB";

                    return;
                  }

                  local.MonthlyObligeeSummary.Assign(
                    local.InitializedMonthlyObligeeSummary);
                  local.MonthlyObligeeSummary.Year =
                    Year(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.Month =
                    Month(entities.DisbursementTransaction.CollectionDate);
                  local.MonthlyObligeeSummary.DisbursementsSuppressed =
                    entities.DisbursementTransaction.Amount;

                  if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y'
                    )
                  {
                    local.MonthlyObligeeSummary.TotExcessUraAmt =
                      -entities.DisbursementTransaction.Amount;
                  }
                  else
                  {
                    local.MonthlyObligeeSummary.CollectionsDisbursedToAr =
                      -entities.DisbursementTransaction.Amount;
                  }

                  UseFnUpdateObligeeMonthlyTotals();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    return;
                  }
                }
                else
                {
                  var field =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field.Error = true;

                  ExitState = "FN0000_SUPPRES_AGAIN_NOT_ALLOWED";

                  return;
                }
              }
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            MoveDisbursementStatus(entities.New1,
              export.Export1.Update.DisbursementStatus);
            export.Export1.Update.DisbSuppressionStatusHistory.LastUpdatedBy =
              global.UserId;

            // --------------------------------------------------
            // 11/08/99  SWSRKXD  PR#79043
            // Clear Release Date. Flag processed record.
            // --------------------------------------------------
            export.Export1.Update.DisbursementStatusHistory.EffectiveDate =
              local.InitializedDateWorkArea.Date;
            export.Export1.Update.Common.SelectChar = "*";
          }
        }

        // --------------------------------------------------
        // 11/08/99  SWSRKXD  PR#79043
        // Display message on successful update.
        // --------------------------------------------------
        ExitState = "FN0000_SUPPRESSION_SUCCESSFUL";

        break;
      case "DISPLAY":
        // --Clear the export group.
        export.Export1.Index = 0;
        export.Export1.Clear();

        for(local.Null1.Index = 0; local.Null1.Index < local.Null1.Count; ++
          local.Null1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Next();
        }

        if (!IsEmpty(export.Search.Type1))
        {
          local.Code.CodeName = "SUPPRESSION TYPE";
          local.CodeValue.Cdvalue = export.Search.Type1;
          UseCabValidateCodeValue();

          if (AsChar(local.ValidCode.Flag) == 'Y')
          {
          }
          else
          {
            var field = GetField(export.Search, "type1");

            field.Error = true;

            ExitState = "FN0000_INVALID_SUPPRESSION_TYPE";

            return;
          }
        }

        if (!IsEmpty(export.SearchRefNbr.ReferenceNumber))
        {
          UseFnCabFormatReferenceNumber();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.SearchRefNbr, "referenceNumber");

            field.Error = true;

            return;
          }

          local.DisbFoundForRefNbr.Flag = "N";

          if (ReadDisbursementTransaction())
          {
            export.SearchEnding.ProcessDate = Now().Date;
            export.SearchStarting.ProcessDate =
              Date(entities.FirstDisbForRefNbr.CreatedTimestamp);
            local.DisbFoundForRefNbr.Flag = "Y";
          }

          if (AsChar(local.DisbFoundForRefNbr.Flag) == 'N')
          {
            var field = GetField(export.SearchRefNbr, "referenceNumber");

            field.Error = true;

            ExitState = "FN0000_REF_NBR_NF";

            return;
          }
        }
        else
        {
          if (Equal(export.SearchStarting.ProcessDate, null))
          {
            local.DateWorkArea.Date = Now().Date.AddMonths(-1);
            UseCabFirstAndLastDateOfMonth();
            export.SearchStarting.ProcessDate = local.DateWorkArea.Date;
          }

          if (Equal(export.SearchEnding.ProcessDate, null))
          {
            export.SearchEnding.ProcessDate = Now().Date;
          }

          if (Lt(export.SearchEnding.ProcessDate,
            export.SearchStarting.ProcessDate) || Lt
            (Now().Date, export.SearchEnding.ProcessDate))
          {
            var field1 = GetField(export.SearchEnding, "processDate");

            field1.Error = true;

            var field2 = GetField(export.SearchStarting, "processDate");

            field2.Error = true;

            ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

            return;
          }
        }

        // ***************************************************************
        // On screen you might see one record with a reference number of 0001026
        // -0001 with a Disb Code of NA ACS but that may represent several
        // records rolled up into that one record for screen display. These of
        // course will have the same reference number and Disb Code. So when the
        // user selects this one record on screen to Release it, we have to
        // Release all the disb trans that were rolled up into that one.
        // ****************************************************************
        UseFnDispDisbSuppressionsVer2();

        if (IsExitState("FN0000_NO_RECORDS_FOUND"))
        {
          export.HiddenDisplayed.Number = "";
        }
        else if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenDisplayed.Number = export.CsePerson.Number;
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (IsExitState("CSE_PERSON_NF"))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          export.HiddenDisplayed.Number = "";
        }
        else
        {
          export.HiddenDisplayed.Number = "";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "FN0000_DISB_SEARCH_DATE_REDUCE";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        if (!Equal(import.HiddenDisplayed.Number, export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        if (!Equal(import.HiddenDisplayed.Number, export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LPSP":
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        ExitState = "ECO_LNK_LST_PAYEE_W_DISB_SUP";

        break;
      case "PSUP":
        ExitState = "ECO_XFR_TO_MTN_PERSON_DISB_SUPP";

        break;
      case "CSUP":
        ExitState = "ECO_XFR_TO_MTN_COLL_DISB_SUPPR";

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonAccount(CsePersonAccount source,
    CsePersonAccount target)
  {
    target.Type1 = source.Type1;
    target.CspNumber = source.CspNumber;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisbursementStatus(DisbursementStatus source,
    DisbursementStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveDisbursementStatusHistory1(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveDisbursementStatusHistory2(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.ReasonText = source.ReasonText;
    target.SuppressionReason = source.SuppressionReason;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveExport1(FnDispDisbSuppressionsVer2.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.DisbursementStatusHistory.Assign(source.DisbursementStatusHistory);
    target.DisbType.Text10 = source.DisbType.Text10;
    MoveDisbursementStatus(source.DisbursementStatus, target.DisbursementStatus);
      
    target.DisbursementTransaction.Assign(source.DisbursementTransaction);
    target.PersonSupp.Text1 = source.PersonSupp.Text1;
    target.CollSupp.Code = source.CollSupp.Code;
    target.AutoSupp.Text1 = source.AutoSupp.Text1;
    target.DupPmtSupp.Text1 = source.DupPmtSupp.Text1;
    target.DisbSuppressionStatusHistory.LastUpdatedBy =
      source.DisbSuppressionStatusHistory.LastUpdatedBy;
    target.DisbursementType.Code = source.DisbursementType.Code;
    target.AddrSupp.Text1 = source.AddrSupp.Text1;
    target.DodSupp.Text1 = source.DodSupp.Text1;
    target.SuppTypes.Text6 = source.SuppTypes.Text6;
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.PassthruAmount = source.PassthruAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
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

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.First.Date;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Max.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
    export.SuppressionType.Description = useExport.CodeValue.Description;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnCabFormatReferenceNumber()
  {
    var useImport = new FnCabFormatReferenceNumber.Import();
    var useExport = new FnCabFormatReferenceNumber.Export();

    useImport.RefNbr.ReferenceNumber = export.SearchRefNbr.ReferenceNumber;

    Call(FnCabFormatReferenceNumber.Execute, useImport, useExport);

    export.SearchRefNbr.ReferenceNumber = useImport.RefNbr.ReferenceNumber;
    export.SearchRefNbr.ReferenceNumber = useExport.RefNbr.ReferenceNumber;
  }

  private void UseFnChangeDisbursementStatus()
  {
    var useImport = new FnChangeDisbursementStatus.Import();
    var useExport = new FnChangeDisbursementStatus.Export();

    MoveDisbursementStatusHistory1(entities.DisbursementStatusHistory,
      useImport.DisbursementStatusHistory);
    MoveDisbursementTransaction(entities.DisbursementTransaction,
      useImport.DisbursementTransaction);
    useImport.PerNew.Assign(entities.New1);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useExport.New1.Assign(entities.New1);

    Call(FnChangeDisbursementStatus.Execute, useImport, useExport);

    MoveDisbursementStatus(useImport.PerNew, entities.New1);
    MoveDisbursementStatus(useExport.New1, entities.New1);
    export.Export1.Update.DisbursementStatusHistory.SystemGeneratedIdentifier =
      useExport.DisbursementStatusHistory.SystemGeneratedIdentifier;
  }

  private void UseFnCheckForSystemSuppression()
  {
    var useImport = new FnCheckForSystemSuppression.Import();
    var useExport = new FnCheckForSystemSuppression.Export();

    useImport.Persistent.Assign(entities.DisbursementTransaction);
    useImport.Ar.Number = export.CsePerson.Number;

    Call(FnCheckForSystemSuppression.Execute, useImport, useExport);

    local.Hold.Assign(useExport.DisbSuppressionStatusHistory);
  }

  private void UseFnDispDisbSuppressionsVer2()
  {
    var useImport = new FnDispDisbSuppressionsVer2.Import();
    var useExport = new FnDispDisbSuppressionsVer2.Export();

    useImport.SearchEnding.ProcessDate = export.SearchEnding.ProcessDate;
    useImport.SearchRefNbr.ReferenceNumber =
      export.SearchRefNbr.ReferenceNumber;
    useImport.HardcodeProcessed.SystemGeneratedIdentifier =
      local.Processed.SystemGeneratedIdentifier;
    useImport.ImportedHardcodeSuppressed.SystemGeneratedIdentifier =
      local.Suppressed.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.SearchStarting.ProcessDate = export.SearchStarting.ProcessDate;
    useImport.Search.Type1 = export.Search.Type1;

    Call(FnDispDisbSuppressionsVer2.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnHardcodedDisbLessDisbType()
  {
    var useImport = new FnHardcodedDisbLessDisbType.Import();
    var useExport = new FnHardcodedDisbLessDisbType.Export();

    Call(FnHardcodedDisbLessDisbType.Execute, useImport, useExport);

    local.Processed.SystemGeneratedIdentifier =
      useExport.Processed.SystemGeneratedIdentifier;
    local.Released.SystemGeneratedIdentifier =
      useExport.Released.SystemGeneratedIdentifier;
    local.Suppressed.SystemGeneratedIdentifier =
      useExport.Suppressed.SystemGeneratedIdentifier;
  }

  private void UseFnRaiseSystemSupprEvent()
  {
    var useImport = new FnRaiseSystemSupprEvent.Import();
    var useExport = new FnRaiseSystemSupprEvent.Export();

    MoveDisbursementStatusHistory2(local.DisbursementStatusHistory,
      useImport.DisbursementStatusHistory);
    useImport.Ar.Number = export.CsePerson.Number;
    useImport.DisbursementTransaction.Assign(
      export.Export1.Item.DisbursementTransaction);

    Call(FnRaiseSystemSupprEvent.Execute, useImport, useExport);
  }

  private void UseFnUpdateObligeeMonthlyTotals()
  {
    var useImport = new FnUpdateObligeeMonthlyTotals.Import();
    var useExport = new FnUpdateObligeeMonthlyTotals.Export();

    useImport.Per.Assign(entities.CsePersonAccount);
    MoveMonthlyObligeeSummary(local.MonthlyObligeeSummary,
      useImport.MonthlyObligeeSummary);

    Call(FnUpdateObligeeMonthlyTotals.Execute, useImport, useExport);

    MoveCsePersonAccount(useImport.Per, entities.CsePersonAccount);
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

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCsePersonAccount()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "type", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          entities.DisbursementTransaction.DisbursementDate.
            GetValueOrDefault());
        db.SetDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbSuppressionStatusHistory.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbursementStatus1()
  {
    entities.New1.Populated = false;

    return Read("ReadDisbursementStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId", local.Released.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.Code = db.GetString(reader, 1);
        entities.New1.Populated = true;
      });
  }

  private bool ReadDisbursementStatus2()
  {
    entities.New1.Populated = false;

    return Read("ReadDisbursementStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId", local.Suppressed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.Code = db.GetString(reader, 1);
        entities.New1.Populated = true;
      });
  }

  private bool ReadDisbursementStatusDisbursementStatusHistory1()
  {
    entities.Current.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementStatusDisbursementStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          export.Export1.Item.DisbursementTransaction.
            SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Current.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.Current.Code = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 3);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 4);
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 4);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 6);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 10);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 11);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 12);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 13);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 16);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 17);
        entities.Current.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private bool ReadDisbursementStatusDisbursementStatusHistory2()
  {
    entities.Current.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementStatusDisbursementStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          export.Export1.Item.DisbursementTransaction.
            SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Current.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.Current.Code = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 3);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 4);
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 4);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 6);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 10);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 11);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 12);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 13);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 16);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 17);
        entities.Current.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private bool ReadDisbursementTransaction()
  {
    entities.FirstDisbForRefNbr.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetNullableString(
          command, "referenceNumber", export.SearchRefNbr.ReferenceNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.FirstDisbForRefNbr.CpaType = db.GetString(reader, 0);
        entities.FirstDisbForRefNbr.CspNumber = db.GetString(reader, 1);
        entities.FirstDisbForRefNbr.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.FirstDisbForRefNbr.Type1 = db.GetString(reader, 3);
        entities.FirstDisbForRefNbr.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.FirstDisbForRefNbr.ReferenceNumber =
          db.GetNullableString(reader, 5);
        entities.FirstDisbForRefNbr.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.FirstDisbForRefNbr.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.FirstDisbForRefNbr.Type1);
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionDisbursementStatusHistory1()
  {
    entities.Current.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementStatusHistory1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "referenceNumber",
          export.Export1.Item.DisbursementTransaction.ReferenceNumber ?? "");
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.
          SetString(command, "code", export.Export1.Item.DisbursementType.Code);
          
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 10);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 11);
        entities.Current.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 13);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 16);
        entities.Current.Code = db.GetString(reader, 17);
        entities.Current.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionDisbursementStatusHistory2()
  {
    entities.Current.Populated = false;
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementStatusHistory2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "referenceNumber",
          export.Export1.Item.DisbursementTransaction.ReferenceNumber ?? "");
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.
          SetString(command, "code", export.Export1.Item.DisbursementType.Code);
          
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 10);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 11);
        entities.Current.SystemGeneratedIdentifier = db.GetInt32(reader, 11);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 13);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 14);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 15);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 16);
        entities.Current.Code = db.GetString(reader, 17);
        entities.Current.Populated = true;
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
      });
  }

  private bool ReadServiceProviderProfile()
  {
    entities.ServiceProviderProfile.Populated = false;

    return Read("ReadServiceProviderProfile",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 1);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProviderProfile.Populated = true;
      });
  }

  private void UpdateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbSuppressionStatusHistory.Populated);

    var discontinueDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("UpdateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(
          command, "cpaType", entities.DisbSuppressionStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber",
          entities.DisbSuppressionStatusHistory.CspNumber);
        db.SetInt32(
          command, "dssGeneratedId",
          entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.LastUpdatedBy = lastUpdatedBy;
    entities.DisbSuppressionStatusHistory.LastUpdatedTmst = lastUpdatedTmst;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void UpdateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = local.DisbSuppressionStatusHistory.DiscontinueDate;
    var suppressionReason = local.DisbSuppressionStatusHistory.Type1;

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "suppressionReason", suppressionReason);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.SuppressionReason = suppressionReason;
    entities.DisbursementStatusHistory.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
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
      /// A value of DisbursementStatusHistory.
      /// </summary>
      [JsonPropertyName("disbursementStatusHistory")]
      public DisbursementStatusHistory DisbursementStatusHistory
      {
        get => disbursementStatusHistory ??= new();
        set => disbursementStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DisbursementStatus.
      /// </summary>
      [JsonPropertyName("disbursementStatus")]
      public DisbursementStatus DisbursementStatus
      {
        get => disbursementStatus ??= new();
        set => disbursementStatus = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>
      /// A value of PersonSupp.
      /// </summary>
      [JsonPropertyName("personSupp")]
      public TextWorkArea PersonSupp
      {
        get => personSupp ??= new();
        set => personSupp = value;
      }

      /// <summary>
      /// A value of CollSupp.
      /// </summary>
      [JsonPropertyName("collSupp")]
      public CollectionType CollSupp
      {
        get => collSupp ??= new();
        set => collSupp = value;
      }

      /// <summary>
      /// A value of AutoSupp.
      /// </summary>
      [JsonPropertyName("autoSupp")]
      public TextWorkArea AutoSupp
      {
        get => autoSupp ??= new();
        set => autoSupp = value;
      }

      /// <summary>
      /// A value of DupPmtSupp.
      /// </summary>
      [JsonPropertyName("dupPmtSupp")]
      public TextWorkArea DupPmtSupp
      {
        get => dupPmtSupp ??= new();
        set => dupPmtSupp = value;
      }

      /// <summary>
      /// A value of DisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("disbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
      {
        get => disbSuppressionStatusHistory ??= new();
        set => disbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of AddrSupp.
      /// </summary>
      [JsonPropertyName("addrSupp")]
      public TextWorkArea AddrSupp
      {
        get => addrSupp ??= new();
        set => addrSupp = value;
      }

      /// <summary>
      /// A value of DodSupp.
      /// </summary>
      [JsonPropertyName("dodSupp")]
      public TextWorkArea DodSupp
      {
        get => dodSupp ??= new();
        set => dodSupp = value;
      }

      /// <summary>
      /// A value of SuppTypes.
      /// </summary>
      [JsonPropertyName("suppTypes")]
      public WorkArea SuppTypes
      {
        get => suppTypes ??= new();
        set => suppTypes = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common common;
      private DisbursementStatusHistory disbursementStatusHistory;
      private TextWorkArea disbType;
      private DisbursementStatus disbursementStatus;
      private DisbursementTransaction disbursementTransaction;
      private TextWorkArea personSupp;
      private CollectionType collSupp;
      private TextWorkArea autoSupp;
      private TextWorkArea dupPmtSupp;
      private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
      private DisbursementType disbursementType;
      private TextWorkArea addrSupp;
      private TextWorkArea dodSupp;
      private WorkArea suppTypes;
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
    /// A value of PromptPayee.
    /// </summary>
    [JsonPropertyName("promptPayee")]
    public Common PromptPayee
    {
      get => promptPayee ??= new();
      set => promptPayee = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchStarting.
    /// </summary>
    [JsonPropertyName("searchStarting")]
    public DisbursementTransaction SearchStarting
    {
      get => searchStarting ??= new();
      set => searchStarting = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public DisbursementTransaction SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
    }

    /// <summary>
    /// A value of SearchRefNbr.
    /// </summary>
    [JsonPropertyName("searchRefNbr")]
    public DisbursementTransaction SearchRefNbr
    {
      get => searchRefNbr ??= new();
      set => searchRefNbr = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public CsePerson HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
    }

    /// <summary>
    /// A value of StorePreviousCommon.
    /// </summary>
    [JsonPropertyName("storePreviousCommon")]
    public Common StorePreviousCommon
    {
      get => storePreviousCommon ??= new();
      set => storePreviousCommon = value;
    }

    /// <summary>
    /// A value of StorePreviousDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("storePreviousDisbursementTransaction")]
    public DisbursementTransaction StorePreviousDisbursementTransaction
    {
      get => storePreviousDisbursementTransaction ??= new();
      set => storePreviousDisbursementTransaction = value;
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

    /// <summary>
    /// A value of SuppressionType.
    /// </summary>
    [JsonPropertyName("suppressionType")]
    public CodeValue SuppressionType
    {
      get => suppressionType ??= new();
      set => suppressionType = value;
    }

    /// <summary>
    /// A value of FromCdvl.
    /// </summary>
    [JsonPropertyName("fromCdvl")]
    public CodeValue FromCdvl
    {
      get => fromCdvl ??= new();
      set => fromCdvl = value;
    }

    /// <summary>
    /// A value of PromptSuppressionType.
    /// </summary>
    [JsonPropertyName("promptSuppressionType")]
    public Common PromptSuppressionType
    {
      get => promptSuppressionType ??= new();
      set => promptSuppressionType = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public DisbSuppressionStatusHistory Search
    {
      get => search ??= new();
      set => search = value;
    }

    private CsePerson csePerson;
    private Common promptPayee;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbursementTransaction searchStarting;
    private DisbursementTransaction searchEnding;
    private DisbursementTransaction searchRefNbr;
    private Array<ImportGroup> import1;
    private CsePerson hiddenDisplayed;
    private Common storePreviousCommon;
    private DisbursementTransaction storePreviousDisbursementTransaction;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue suppressionType;
    private CodeValue fromCdvl;
    private Common promptSuppressionType;
    private DisbSuppressionStatusHistory search;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
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
      /// A value of DisbursementStatusHistory.
      /// </summary>
      [JsonPropertyName("disbursementStatusHistory")]
      public DisbursementStatusHistory DisbursementStatusHistory
      {
        get => disbursementStatusHistory ??= new();
        set => disbursementStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DisbursementStatus.
      /// </summary>
      [JsonPropertyName("disbursementStatus")]
      public DisbursementStatus DisbursementStatus
      {
        get => disbursementStatus ??= new();
        set => disbursementStatus = value;
      }

      /// <summary>
      /// A value of DisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbursementTransaction")]
      public DisbursementTransaction DisbursementTransaction
      {
        get => disbursementTransaction ??= new();
        set => disbursementTransaction = value;
      }

      /// <summary>
      /// A value of PersonSupp.
      /// </summary>
      [JsonPropertyName("personSupp")]
      public TextWorkArea PersonSupp
      {
        get => personSupp ??= new();
        set => personSupp = value;
      }

      /// <summary>
      /// A value of CollSupp.
      /// </summary>
      [JsonPropertyName("collSupp")]
      public CollectionType CollSupp
      {
        get => collSupp ??= new();
        set => collSupp = value;
      }

      /// <summary>
      /// A value of AutoSupp.
      /// </summary>
      [JsonPropertyName("autoSupp")]
      public TextWorkArea AutoSupp
      {
        get => autoSupp ??= new();
        set => autoSupp = value;
      }

      /// <summary>
      /// A value of DupPmtSupp.
      /// </summary>
      [JsonPropertyName("dupPmtSupp")]
      public TextWorkArea DupPmtSupp
      {
        get => dupPmtSupp ??= new();
        set => dupPmtSupp = value;
      }

      /// <summary>
      /// A value of DisbSuppressionStatusHistory.
      /// </summary>
      [JsonPropertyName("disbSuppressionStatusHistory")]
      public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
      {
        get => disbSuppressionStatusHistory ??= new();
        set => disbSuppressionStatusHistory = value;
      }

      /// <summary>
      /// A value of DisbursementType.
      /// </summary>
      [JsonPropertyName("disbursementType")]
      public DisbursementType DisbursementType
      {
        get => disbursementType ??= new();
        set => disbursementType = value;
      }

      /// <summary>
      /// A value of AddrSupp.
      /// </summary>
      [JsonPropertyName("addrSupp")]
      public TextWorkArea AddrSupp
      {
        get => addrSupp ??= new();
        set => addrSupp = value;
      }

      /// <summary>
      /// A value of DodSupp.
      /// </summary>
      [JsonPropertyName("dodSupp")]
      public TextWorkArea DodSupp
      {
        get => dodSupp ??= new();
        set => dodSupp = value;
      }

      /// <summary>
      /// A value of SuppTypes.
      /// </summary>
      [JsonPropertyName("suppTypes")]
      public WorkArea SuppTypes
      {
        get => suppTypes ??= new();
        set => suppTypes = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common common;
      private DisbursementStatusHistory disbursementStatusHistory;
      private TextWorkArea disbType;
      private DisbursementStatus disbursementStatus;
      private DisbursementTransaction disbursementTransaction;
      private TextWorkArea personSupp;
      private CollectionType collSupp;
      private TextWorkArea autoSupp;
      private TextWorkArea dupPmtSupp;
      private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
      private DisbursementType disbursementType;
      private TextWorkArea addrSupp;
      private TextWorkArea dodSupp;
      private WorkArea suppTypes;
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
    /// A value of PromptPayee.
    /// </summary>
    [JsonPropertyName("promptPayee")]
    public Common PromptPayee
    {
      get => promptPayee ??= new();
      set => promptPayee = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchStarting.
    /// </summary>
    [JsonPropertyName("searchStarting")]
    public DisbursementTransaction SearchStarting
    {
      get => searchStarting ??= new();
      set => searchStarting = value;
    }

    /// <summary>
    /// A value of SearchEnding.
    /// </summary>
    [JsonPropertyName("searchEnding")]
    public DisbursementTransaction SearchEnding
    {
      get => searchEnding ??= new();
      set => searchEnding = value;
    }

    /// <summary>
    /// A value of SearchRefNbr.
    /// </summary>
    [JsonPropertyName("searchRefNbr")]
    public DisbursementTransaction SearchRefNbr
    {
      get => searchRefNbr ??= new();
      set => searchRefNbr = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of StorePreviousCommon.
    /// </summary>
    [JsonPropertyName("storePreviousCommon")]
    public Common StorePreviousCommon
    {
      get => storePreviousCommon ??= new();
      set => storePreviousCommon = value;
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public CsePerson HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
    }

    /// <summary>
    /// A value of StorePreviousDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("storePreviousDisbursementTransaction")]
    public DisbursementTransaction StorePreviousDisbursementTransaction
    {
      get => storePreviousDisbursementTransaction ??= new();
      set => storePreviousDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public DisbursementTransaction HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CollectionType Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    /// <summary>
    /// A value of SuppressionType.
    /// </summary>
    [JsonPropertyName("suppressionType")]
    public CodeValue SuppressionType
    {
      get => suppressionType ??= new();
      set => suppressionType = value;
    }

    /// <summary>
    /// A value of PromptSuppressionType.
    /// </summary>
    [JsonPropertyName("promptSuppressionType")]
    public Common PromptSuppressionType
    {
      get => promptSuppressionType ??= new();
      set => promptSuppressionType = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public DisbSuppressionStatusHistory Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of ToCdvl.
    /// </summary>
    [JsonPropertyName("toCdvl")]
    public Code ToCdvl
    {
      get => toCdvl ??= new();
      set => toCdvl = value;
    }

    private CsePerson csePerson;
    private Common promptPayee;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DisbursementTransaction searchStarting;
    private DisbursementTransaction searchEnding;
    private DisbursementTransaction searchRefNbr;
    private Array<ExportGroup> export1;
    private Common storePreviousCommon;
    private CsePerson hiddenDisplayed;
    private DisbursementTransaction storePreviousDisbursementTransaction;
    private DisbursementTransaction hiddenSelection;
    private CollectionType selected;
    private NextTranInfo hidden;
    private Standard standard;
    private CodeValue suppressionType;
    private Common promptSuppressionType;
    private DisbSuppressionStatusHistory search;
    private Code toCdvl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A NullGroup group.</summary>
    [Serializable]
    public class NullGroup
    {
      /// <summary>
      /// A value of Null2.
      /// </summary>
      [JsonPropertyName("null2")]
      public Common Null2
      {
        get => null2 ??= new();
        set => null2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 108;

      private Common null2;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DisbSuppressionStatusHistory Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// Gets a value of Null1.
    /// </summary>
    [JsonIgnore]
    public Array<NullGroup> Null1 => null1 ??= new(NullGroup.Capacity);

    /// <summary>
    /// Gets a value of Null1 for json serialization.
    /// </summary>
    [JsonPropertyName("null1")]
    [Computed]
    public IList<NullGroup> Null1_Json
    {
      get => null1;
      set => Null1.Assign(value);
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of PromptCount.
    /// </summary>
    [JsonPropertyName("promptCount")]
    public Common PromptCount
    {
      get => promptCount ??= new();
      set => promptCount = value;
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
    /// A value of DisbFoundForRefNbr.
    /// </summary>
    [JsonPropertyName("disbFoundForRefNbr")]
    public Common DisbFoundForRefNbr
    {
      get => disbFoundForRefNbr ??= new();
      set => disbFoundForRefNbr = value;
    }

    /// <summary>
    /// A value of RuleHasBeenUpdated.
    /// </summary>
    [JsonPropertyName("ruleHasBeenUpdated")]
    public Common RuleHasBeenUpdated
    {
      get => ruleHasBeenUpdated ??= new();
      set => ruleHasBeenUpdated = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of InitializedMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("initializedMonthlyObligeeSummary")]
    public MonthlyObligeeSummary InitializedMonthlyObligeeSummary
    {
      get => initializedMonthlyObligeeSummary ??= new();
      set => initializedMonthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of PreviousDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("previousDisbursementTransaction")]
    public DisbursementTransaction PreviousDisbursementTransaction
    {
      get => previousDisbursementTransaction ??= new();
      set => previousDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of PreviousCommon.
    /// </summary>
    [JsonPropertyName("previousCommon")]
    public Common PreviousCommon
    {
      get => previousCommon ??= new();
      set => previousCommon = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Processed.
    /// </summary>
    [JsonPropertyName("processed")]
    public DisbursementStatus Processed
    {
      get => processed ??= new();
      set => processed = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public DisbursementStatus Released
    {
      get => released ??= new();
      set => released = value;
    }

    /// <summary>
    /// A value of Suppressed.
    /// </summary>
    [JsonPropertyName("suppressed")]
    public DisbursementStatus Suppressed
    {
      get => suppressed ??= new();
      set => suppressed = value;
    }

    /// <summary>
    /// A value of SelectFound.
    /// </summary>
    [JsonPropertyName("selectFound")]
    public Common SelectFound
    {
      get => selectFound ??= new();
      set => selectFound = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    private DisbSuppressionStatusHistory hold;
    private Array<NullGroup> null1;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private Common promptCount;
    private Common validCode;
    private Code code;
    private CodeValue codeValue;
    private Common disbFoundForRefNbr;
    private Common ruleHasBeenUpdated;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private MonthlyObligeeSummary initializedMonthlyObligeeSummary;
    private DateWorkArea initializedDateWorkArea;
    private DisbursementTransaction previousDisbursementTransaction;
    private Common previousCommon;
    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private DisbursementStatus processed;
    private DisbursementStatus released;
    private DisbursementStatus suppressed;
    private Common selectFound;
    private TextWorkArea leftPadding;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of FirstDisbForRefNbr.
    /// </summary>
    [JsonPropertyName("firstDisbForRefNbr")]
    public DisbursementTransaction FirstDisbForRefNbr
    {
      get => firstDisbForRefNbr ??= new();
      set => firstDisbForRefNbr = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DisbursementStatus Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private ServiceProvider serviceProvider;
    private Profile profile;
    private ServiceProviderProfile serviceProviderProfile;
    private DisbursementTransaction firstDisbForRefNbr;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbursementStatus current;
    private DisbursementType disbursementType;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementStatus new1;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}

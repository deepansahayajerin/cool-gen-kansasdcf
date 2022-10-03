// Program: FN_OFEE_MTN_FEE_OBLIGATION, ID: 372159000, model: 746.
// Short name: SWEOFEEP
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
/// A program: FN_OFEE_MTN_FEE_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This Procedure maintains Recovery and Fee type obligations.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOfeeMtnFeeObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OFEE_MTN_FEE_OBLIGATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOfeeMtnFeeObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOfeeMtnFeeObligation.
  /// </summary>
  public FnOfeeMtnFeeObligation(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // ---------------------------------------------
    // ---------------------------------------------
    // Note to Finance Group from Siraj ---- 22 July 96
    // Q: All fields are being protected in case of some errors.
    // See last group of MAKE statements at the bottom of PRAD.
    // Q: Am not able to follow if the edits are being handled correctly. Need 
    // to test if the edits for print will be handled correctly. Edits required
    // are (i) Display before print (ii) Obligor, Obligation Type and Obligation
    // Amount cannot be changed.
    // --------------------------------------------
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#      Description
    // 02/16/96  Rick Delgado                New Development
    // 07/22/96  Siraj Konkader              Print Recovery Letters
    // 08/08/96  R.B.Mohapatra               Change in procedure logic due to
    //                                       
    // change in Data Model
    // 01/13/97  HOOKS		              Raise events
    // 01/23/97  HOOKS		              ADD LOGIC FOR HIST/MONA
    // 
    // AUTOMATIC NEXTTRAN
    // 04/29/97 - Sumanta - MTW - Made the following changes :-
    //     *- Removed the FIPS related logic.
    //     *- Added the new logic for alternate address.
    //        Added the screen field for alt address and prompt..
    //        flows to NAME
    //     *- Removed the link to FIPL
    //     *- Modified the existing code to set the error messages
    //        in prompt selection correctly
    //     *- fixed the problem of loosing an existing field when
    //        coming back from prompt selection without selecting
    //        any.
    // ****--- 05/01/97 - Changed DB2 current_date to IEF current_date
    // 05/16/97  Kennedy-MTW	               IDCR ??? - Sid's IDCR
    //                                        
    // Changed OBT to OBL
    // 06/16/97  Kennedy-MTW                  Fixed relationship to court
    //                                        
    // order.
    //                                        
    // Fixed designated payee
    //                                        
    // logic.
    //                                        
    // Fixed logic not to send
    //                                        
    // alert unless the address
    //                                        
    // is Changed
    //                                        
    // Added last updated timestamp
    // ----------------------------------------------------------------
    // ----------------------------------------------------------------
    //                                        
    // to the screen
    //                                        
    // removed PF21 ASIN moving
    //                                        
    // logic to PF4 list to be
    //                                        
    // consistent with OACC and
    //                                        
    // ONAC
    //                                        
    // Added pmt frequency literal
    //                                        
    // to the screen
    //                                        
    // Caused the interest amount
    //                                        
    // to be underscored
    // 07/15/97  Paul R. Egger-MTW            OREC copied to and created OFEE.
    // 07/24/97  Paul R. Egger-MTW            Made Interstate Obligation 
    // unprotected, required, and must contain a I or K, populated views for
    // when LDET is the next transaction, when flowing from LDET to OFEE -- be
    // sure to populate the name field, Other State was added and must contain a
    // valid state, passed due date to INMS as the accrual/due date.
    // 09/04/97  E. Parker - DIR		Removed logic which was setting 
    // primary_secondary_code to 'P'.
    // 12/29/97	Venkatesh Kamaraj	Set the Situation # to 0 instead of call to 
    // get_next_situation_no
    // ----------------------------------------------------------------
    // ================================================
    // 10/2/98  B Adams  -
    // 	Eliminated all uses of CURRENT_TIMESTAMP and CURRENT_DATE and the 
    // ROUNDED function - and there were MANY of them.
    // 	Fixed mistaken assignment of Day-of-Week-1  attribute  to Day-of-Week-2 
    // attribute
    // 	Deleted two USE fn-hardcode-debt-distribution statements.  One of those 
    // is enough.
    // 	Removed all references to PRINT function.
    // ================================================
    // =================================================
    // 12/11/98 - B Adams  -  Obligor prompt code, which had been
    //   deactivated, is now reactivated.
    //   Obligation type prompt is now temporarily deactivated.
    //   That function had been modified so that only Fee type of
    //   records were displayed, and as of now, there is only one
    //   type: FEE.  In the future, there probably will be more added.
    // 3/23/1999 - B Adams  -  READ properties set.
    // 6/9/99 - b adams  -  Changed the date being displayed as
    //   Retired Date from Debt_Detail Retired_Date to Debt_Detail_
    //   Status_History Effective_Date, if the Status is "D", and
    //   changed the label to be Deactive Date.
    // 6/23/99 - b adams  -  Allow Assigned To field to be specified
    //   by the user at ADD time instead of forcing it to default to the
    //   logged-on user, and then changing it at UPDATE
    // 8/11/99 - Fangman - Added logic to create events.
    // =================================================
    // =====================================================================
    // 3/1/00  K. Price - process country or state code for ADD
    // and UPDATE
    // 06/21/2000  V.Madhira  PR# 85900   Fixing the interstate fields. 
    // Including the missing edits.
    // 09/21/2000  V.Madhira   PR# 102555  Fixed the code for displaying an 
    // error message if invalid data enetered in NEXT field. Also now the screen
    // will  let the user to add an obligation for an AP on OPEN case only.
    // ==========================================================================
    // =================================================
    // Oct, 2000 M. Brown, pr# 106234 - NEXT TRAN updates.
    // =================================================
    // **************************************************************
    //  Cleaned up the certification number views from office-
    // service-provider entity .This view was never used .
    //   WR 286 :    Madhu Kumar
    // **************************************************************
    // **************************************************************
    //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
    // caseworker assignment
    // PR131579 08/19/2002 KDoshi. Fix screen Help Id.
    // **************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXFMMENU") || Equal
      (global.Command, "SPACES"))
    {
      var field1 = GetField(export.Obligation, "orderTypeCode");

      field1.Color = "green";
      field1.Highlighting = Highlighting.Underscore;
      field1.Protected = false;

      var field2 = GetField(export.Obligation, "otherStateAbbr");

      field2.Color = "green";
      field2.Highlighting = Highlighting.Underscore;
      field2.Protected = false;

      var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

      field3.Color = "green";
      field3.Highlighting = Highlighting.Underscore;
      field3.Protected = false;

      return;
    }

    // ***** MOVE IMPORT'S TO EXPORT'S *****
    MoveCsePersonsWorkSet2(import.AlternateAddr, export.AlternateAddr);
    export.AltAddPrompt.Text1 = import.AltAddPrompt.Text1;
    export.CountryPrompt.SelectChar = import.CountryPrompt.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.ProtectAll.Flag = import.ProtectAll.Flag;
    export.DebtDetailStatusHistory.EffectiveDt =
      import.DebtDetailStatusHistory.EffectiveDt;
    MoveObligationTransaction1(import.ObligationTransaction,
      export.ObligationTransaction);
    export.PreviousObligationTransaction.Amount =
      import.PreviousObligationTransaction.Amount;
    export.LegalAction.Assign(import.LegalAction);
    MoveLegalActionDetail1(import.LegalActionDetail, export.LegalActionDetail);
    MoveObligation5(import.Obligation, export.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.BalanceOwed.TotalCurrency = import.BalanceOwed.TotalCurrency;
    export.DebtDetail.Assign(import.DebtDetail);
    export.InterestOwed.TotalCurrency = import.InterestOwed.TotalCurrency;
    export.ObligationAmt.TotalCurrency = import.ObligationAmt.TotalCurrency;
    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    export.ObligorCsePersonsWorkSet.Assign(import.ObligorCsePersonsWorkSet);
    export.PaymentScheduleInd.Flag = import.PaymentScheduleInd.Flag;
    export.ManualDistributionInd.Flag = import.ManualDistributionInd.Flag;
    export.InterstateOblgInd.Flag = import.InterstateOblgInd.Flag;
    export.TotalOwed.TotalCurrency = import.TotalOwed.TotalCurrency;
    export.PreviousDebtDetail.DueDt = import.PreviousDebtDetail.DueDt;
    export.SuspendInterestInd.Flag = import.SuspendInterestInd.Flag;
    export.Object1.Text20 = import.Object1.Text20;

    if (IsEmpty(export.SuspendInterestInd.Flag))
    {
      export.SuspendInterestInd.Flag = "N";
    }

    export.Last.Command = import.Last.Command;
    export.ConfirmObligAdd.Flag = import.ConfirmObligAdd.Flag;
    export.ConfirmRetroDate.Flag = import.ConfirmRetroDate.Flag;
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.SetupDate.Date = import.SetupDate.Date;
    export.LastUpdDate.Date = import.LastUpdDate.Date;
    export.FrequencyWorkSet.FrequencyDescription =
      import.FrequencyWorkSet.FrequencyDescription;
    export.Assign1.FormattedName = import.Assign1.FormattedName;
    export.AssignPrompt.SelectChar = import.AssignPrompt.SelectChar;
    export.ObligorPrompt.SelectChar = import.ObligorPrompt.SelectChar;
    export.HiddenObligation.Assign(import.HiddenObligation);
    export.HiddenObligationType.Code = import.HiddenObligationType.Code;
    export.HiddenObligor.Number = import.HiddenObligor.Number;
    export.HiddenInterstateRequest.Assign(import.HiddenInterstateRequest);
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
    export.LegalIdPassed.Flag = import.LegalIdPassed.Flag;
    export.CollProtExistsInd.Flag = import.CollProtExistsInd.Flag;
    export.Country.Description = import.Country.Description;
    export.InterstateRequest.Assign(import.InterstateRequest);

    if (IsEmpty(export.InterstateRequest.Country) && !
      IsEmpty(export.Country.Cdvalue))
    {
      export.InterstateRequest.Country =
        Substring(export.Country.Cdvalue, 1, 2);
    }

    export.HiddenAlternateAddr.Number = import.HiddenAlternateAddr.Number;
    export.AdjustmentExists.Flag = import.AdjustmentExists.Flag;
    export.ObligationType.Code = "FEE";
    export.ObligationType.Classification = "F";

    // =================================================
    // 6/24/99 - B Adams  -  Return from ASIN is with Command of
    //   Display!  This view is only valued when linking to ASIN.
    // =================================================
    if (Equal(export.Object1.Text20, "OBLIGATION"))
    {
      global.Command = "BYPASS";
    }

    // ***** HARDCODE AREA *****
    UseFnHardcodedDebtDistribution();
    UseFnHardcodeLegal();

    // =================================================
    // 12/31/98 - b adams  -  Upon return from OCTO, "Obligation
    //   not created yet" was being displayed.  Can't grab dialog
    //   flow to change the command it's coming back with, so this
    //   attribute is being set in 'CASE OCTO' below and stored by
    //   dialog manager when returning via link.
    // =================================================
    if (Equal(export.Last.Command, "OCTO"))
    {
      export.Last.Command = "";
      global.Command = "DISPLAY";
    }

    local.EscapeFlag.Flag = "N";

    // <
    // ---------------------------------------------------------------------------------
    // >
    // RBM  11/07/97
    // This IF.... Statement embeds the rest of the Action diagram just to keep 
    // everything within its scope so that an "ESCAPE" out of its scope will
    // take the control to the "SCREEN FIELD PROTECTION" logic. Otherwise it is
    // very difficult to route all the escapes to that protection logic.
    // <
    // --------------------------------------------------------------------------------
    // >
    if (AsChar(local.EscapeFlag.Flag) == 'N')
    {
      if (Equal(global.Command, "UPDATE"))
      {
        // **************************************************************
        //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
        // caseworker assignment
        // **************************************************************
        // * * Removed * *
        // * * AND tbd_export_assign service_provider user_id IS EQUAL TO
        // * *       tbd_export_hidden_assign service_provider user_id
        if (Equal(export.ObligorCsePerson.Number, export.HiddenObligor.Number) &&
          Equal
          (export.AlternateAddr.Number, export.HiddenAlternateAddr.Number) && AsChar
          (export.Obligation.OrderTypeCode) == AsChar
          (export.HiddenObligation.OrderTypeCode) && Equal
          (export.Obligation.OtherStateAbbr,
          export.HiddenObligation.OtherStateAbbr) && Equal
          (export.Obligation.Description, export.HiddenObligation.Description) &&
          Equal(export.DebtDetail.DueDt, export.PreviousDebtDetail.DueDt) && export
          .ObligationTransaction.Amount == export
          .PreviousObligationTransaction.Amount)
        {
          ExitState = "FN0000_NO_CHANGE_TO_UPDATE";
        }
      }

      if (Equal(global.Command, "RETNAME"))
      {
        if (!IsEmpty(import.FlowFrom.Number))
        {
          export.ObligorCsePersonsWorkSet.Assign(import.FlowFrom);
          export.ObligorCsePerson.Number =
            export.ObligorCsePersonsWorkSet.Number;
        }

        if (Equal(export.SetupDate.Date, local.Current.Date))
        {
          var field = GetField(export.ObligationTransaction, "amount");

          field.Protected = false;
          field.Focused = true;
        }
        else
        {
          var field = GetField(export.Obligation, "description");

          field.Protected = false;
          field.Focused = true;
        }

        // ---------------------------------------------
        // Per PR# 85900 :   The  screen is not displaying the information 
        // message if multiple obligations exist for the obligor. This is
        // happening when flowing back to OFEE from NAME screen. The COMMAND
        // value is  reset from RETNAME to DISPLAY.
        //                                                  
        // Vithal Madhira (05/02/2000)
        // ---------------------------------------------
        global.Command = "DISPLAY";
      }

      // *** SECURITY
      export.Standard.NextTransaction = import.Standard.NextTransaction;

      if (Equal(global.Command, "XXNEXTXX"))
      {
        // ---------------------------------------------
        // User entered this screen from another screen
        // ---------------------------------------------
        UseScCabNextTranGet();
        export.ObligorCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);

        // =====================================
        // SRPT is the HIST screen.
        // SRPU is the MONA screen.
        // =====================================
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          // ***---  SRPT is TranCode for "HIST" screen
          // ***---  SRPU is TranCode for "MONA" screen
          local.FromHistMonaNxttran.Flag = "Y";
          export.ObligorCsePerson.Number =
            export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
          global.Command = "DISPLAY";

          goto Test1;
        }
        else
        {
          local.FromHistMonaNxttran.Flag = "N";
        }

        if (!IsEmpty(export.ObligorCsePerson.Number))
        {
          global.Command = "DISPLAY";
        }
        else
        {
          return;
        }
      }

Test1:

      if (!IsEmpty(import.Standard.NextTransaction))
      {
        // ---------------------------------------------
        // User is going out of this screen to another
        // ---------------------------------------------
        // ---------------------------------------------
        // Set up local next_tran_info for saving the current values for the 
        // next screen
        // ---------------------------------------------
        if (!IsEmpty(export.ObligorCsePerson.Number))
        {
          export.HiddenNextTranInfo.LegalActionIdentifier =
            export.LegalAction.Identifier;
          export.HiddenNextTranInfo.StandardCrtOrdNumber =
            export.LegalAction.StandardNumber ?? "";
          export.HiddenNextTranInfo.CsePersonNumberObligor =
            export.ObligorCsePerson.Number;
          export.HiddenNextTranInfo.CsePersonNumber =
            export.ObligorCsePerson.Number;
          export.HiddenNextTranInfo.ObligationId =
            export.Obligation.SystemGeneratedIdentifier;
          export.HiddenNextTranInfo.MiscNum1 =
            export.ObligationType.SystemGeneratedIdentifier;
        }

        UseScCabNextTranPut();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          // ===========================================================================
          //      Per PR# 102555 this code is added.
          //                                                 
          // Vithal Madhira (09/21/00)
          // ============================================================================
          var field = GetField(export.Standard, "nextTransaction");

          field.Error = true;

          global.Command = "BYPASS";

          goto Test5;
        }
      }

      if (Equal(global.Command, "FROMOPAY") || Equal
        (global.Command, "FROMOCTO"))
      {
        global.Command = "DISPLAY";
      }

      // ================================================
      // 10/9/98 - B Adams  -  deleted long string of negative IF test
      // and replaced with this positive test.
      // ================================================
      if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
        (global.Command, "DISPLAY") || Equal(global.Command, "UPDATE"))
      {
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test5;
        }
      }

      // ***** EDIT AREA *****
      // : If the Obligation System Generated ID is zero, Print, Update and 
      // Delete functions are invalid.
      if (export.Obligation.SystemGeneratedIdentifier == 0 || import
        .HiddenObligation.SystemGeneratedIdentifier == 0)
      {
        if (Equal(global.Command, "UPDATE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          goto Test5;
        }
        else if (Equal(global.Command, "DELETE"))
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";

          goto Test5;
        }
      }

      if (!IsEmpty(export.ObligorCsePerson.Number))
      {
        local.ForLeftPad.Text10 = export.ObligorCsePerson.Number;
        UseEabPadLeftWithZeros();
        export.ObligorCsePerson.Number = local.ForLeftPad.Text10;
      }

      if (!IsEmpty(export.AlternateAddr.Number))
      {
        local.ForLeftPad.Text10 = export.AlternateAddr.Number;
        UseEabPadLeftWithZeros();
        export.AlternateAddr.Number = local.ForLeftPad.Text10;
      }

      // *****************************************************************
      // *** If Flows from Legal screen with the Legal Identifiers  ,  ***
      // *** Command will be  "FROMLDET"
      // 
      // ***
      // *****************************************************************
      if (Equal(global.Command, "FROMLDET"))
      {
        export.ProtectAll.Flag = "N";
        export.LegalIdPassed.Flag = "N";

        if (export.LegalAction.Identifier == 0 || export
          .LegalActionDetail.Number == 0)
        {
          // ** Nothing has been selected from the Legal Action Detail ; So user
          // input is expected
        }
        else
        {
          // ** Legal Identifiers were passed....
          export.LegalIdPassed.Flag = "Y";

          // **************************************************************
          //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
          // caseworker assignment
          // **************************************************************
          // * * Remove EXPORT to tbd_export_assign service_provider
          UseFnRetrieveLeglForRecAndFee();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }

          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          export.BalanceOwed.TotalCurrency =
            export.ObligationTransaction.Amount;
          export.InterestOwed.TotalCurrency = 0;
          export.TotalOwed.TotalCurrency =
            export.LegalActionDetail.JudgementAmount.GetValueOrDefault();
          export.ProtectAll.Flag = "Y";
        }

        global.Command = "DISPLAY";
      }

      // *** The control will come to this position if the user inputs the keys
      //     If one or more key fields are blank, certain commands are not 
      // allowed.
      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
        (global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
      {
        if (IsEmpty(export.ObligorCsePerson.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.ObligorCsePerson, "number");

          field.Error = true;

          goto Test5;
        }

        // *** If the Legal_Action Identifier is available, READ it to have 
        // currency ***
        if (export.LegalAction.Identifier != 0)
        {
          if (ReadLegalAction())
          {
            // ** The Currency is Obtained...Continue Processing
          }
          else
          {
            ExitState = "LEGAL_ACTION_NF";

            goto Test5;
          }
        }

        if (!Equal(global.Command, "DISPLAY"))
        {
          if (ReadObligationType())
          {
            if (AsChar(entities.ObligationType.Classification) == AsChar
              (local.HardcodeFees.Classification))
            {
              // -----------------------------------------------------------------------------
              //   O.K. Obligation type Classification is valid.
              // ------------------------------------------------------------------------------
            }
            else
            {
              ExitState = "FN0000_OBLIG_TYPE_INVALID";

              var field = GetField(export.ObligationType, "code");

              field.Error = true;
            }

            export.ObligationType.Assign(entities.ObligationType);
          }
          else
          {
            ExitState = "FN0000_OBLIG_TYPE_NF";

            var field = GetField(export.ObligationType, "code");

            field.Error = true;
          }
        }
      }

      // : The logic assumes that a record cannot be UPDATED or DELETED without 
      // first being displayed.
      // ........ Therefore, a KEY CHANGE IS INVALID ........
      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
      {
        // : The above list of commands must be reviewed if any
        //   new commands are made relevant to this procedure.
        if (!Equal(import.HiddenLegalAction.StandardNumber,
          import.LegalAction.StandardNumber) && !
          IsEmpty(import.HiddenLegalAction.StandardNumber))
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        if (!Equal(export.ObligorCsePerson.Number, import.HiddenObligor.Number) &&
          !IsEmpty(import.HiddenObligor.Number))
        {
          var field = GetField(export.ObligorCsePerson, "number");

          field.Error = true;

          ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
        }

        export.LegalIdPassed.Flag = "N";
      }

      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
      {
        if (IsEmpty(export.ObligorCsePerson.Number))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.ObligorCsePerson, "number");

          field.Error = true;
        }

        if (IsEmpty(export.Obligation.Description))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.Obligation, "description");

          field.Error = true;
        }

        // *** DATE EDITS for ADD and UPDATE commands.
        // : Due Date is mandatory.
        if (Equal(export.DebtDetail.DueDt, local.BlankDateWorkArea.Date))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.DebtDetail, "dueDt");

          field.Error = true;
        }

        if (export.ObligationTransaction.Amount == 0)
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.ObligationTransaction, "amount");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        if (Lt(export.DebtDetail.DueDt, AddMonths(local.Current.Date, -1)) && (
          Equal(global.Command, "ADD") || Equal
          (export.SetupDate.Date, local.Current.Date)))
        {
          if (Lt(export.DebtDetail.DueDt, AddYears(local.Current.Date, -20)))
          {
            ExitState = "FN0000_DATE_CANT_BE_OVER_20_YRS";

            var field = GetField(export.DebtDetail, "dueDt");

            field.Error = true;

            goto Test2;
          }
        }
        else if (Lt(local.Current.Date, export.DebtDetail.DueDt))
        {
          // =================================================
          // 11/4/98 - B Adams  -  Due date cannot be in the future
          // =================================================
          var field = GetField(export.DebtDetail, "dueDt");

          field.Error = true;

          ExitState = "FN0000_DUE_DATE_CANT_BE_GT_TODAY";

          goto Test2;
        }

        if (!IsEmpty(export.AlternateAddr.Number))
        {
          UseFnCabCheckAltAddr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
        }
      }

Test2:

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        // =================================================
        // 3/1/00  K. Price - process country or state code for ADD
        // and UPDATE
        // =================================================
        switch(AsChar(export.Obligation.OrderTypeCode))
        {
          case 'I':
            // --------------------------------------------------------------------------------
            // Per business rules Interstate cases are valid only with Court 
            // ordered FEE type Obligations. Check if Court Order exists or not.
            //                                        
            // ------  Vithal Madhira ( 06/30/2000)
            // --------------------------------------------------------------------------------
            if (!IsEmpty(export.LegalAction.StandardNumber))
            {
              // -------------------------------------------------------------------
              // Continue processing.......
              // ---------------------------------------------------------------------
            }
            else
            {
              var field1 = GetField(export.Obligation, "orderTypeCode");

              field1.Error = true;

              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Error = true;

              if (!IsEmpty(export.InterstateRequest.Country))
              {
                var field3 = GetField(export.InterstateRequest, "country");

                field3.Error = true;
              }

              if (!IsEmpty(export.Obligation.OtherStateAbbr))
              {
                var field3 = GetField(export.Obligation, "otherStateAbbr");

                field3.Error = true;
              }

              ExitState = "FN0000_INTERSTATE_CASE_INVALID";

              goto Test3;
            }

            // =================================================
            // PR# 85900     06/21/2000  Vithal Madhira     Fixing Interstate 
            // fields. The following code will implement the missing edits for
            // interstate fields.
            // =================================================
            if (AsChar(export.Obligation.OrderTypeCode) == 'I')
            {
              if (IsEmpty(export.InterstateRequest.OtherStateCaseId))
              {
                var field1 =
                  GetField(export.InterstateRequest, "otherStateCaseId");

                field1.Error = true;

                ExitState = "FN0000_MANDATORY_FIELDS";
              }

              if (IsEmpty(export.Obligation.OtherStateAbbr))
              {
                if (!IsEmpty(export.InterstateRequest.Country))
                {
                  // --------------------------------------------------------
                  // O.K.   Continue processing.......
                  // ----------------------------------------------------------
                }
                else
                {
                  var field1 = GetField(export.Obligation, "otherStateAbbr");

                  field1.Error = true;

                  ExitState = "FN0000_MANDATORY_FIELDS";
                }
              }

              if (IsEmpty(export.InterstateRequest.Country))
              {
                if (!IsEmpty(export.Obligation.OtherStateAbbr))
                {
                  // --------------------------------------------------------
                  // O.K.   Continue processing.......
                  // ----------------------------------------------------------
                }
                else
                {
                  var field1 = GetField(export.InterstateRequest, "country");

                  field1.Error = true;

                  ExitState = "FN0000_MANDATORY_FIELDS";
                }
              }

              if (!IsEmpty(export.Obligation.OtherStateAbbr) && !
                IsEmpty(export.InterstateRequest.OtherStateCaseId) && !
                IsEmpty(export.InterstateRequest.Country))
              {
                var field1 = GetField(export.InterstateRequest, "country");

                field1.Error = true;

                var field2 = GetField(export.Obligation, "otherStateAbbr");

                field2.Error = true;

                var field3 =
                  GetField(export.InterstateRequest, "otherStateCaseId");

                field3.Error = true;

                ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
              }
            }

            UseFnRetrieveInterstateRequest();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else if (IsExitState("FN0000_FIPS_FOR_THE_STATE_NF"))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Error = true;
            }
            else if (IsExitState("INTERSTATE_REQUEST_NF"))
            {
              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field1.Error = true;

              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Error = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Error = true;
            }
            else if (IsExitState("LE0000_INVALID_COUNTRY_CODE"))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Error = true;
            }
            else if (IsExitState("ACO_NE0000_INVALID_STATE_CODE"))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_COUNTRY_INTERSTAT"))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_INVALID_STATE_INTERSTATE"))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_INTERSTATE_AP_MISMATCH"))
            {
              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field1.Error = true;

              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Error = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Error = true;
            }
            else
            {
            }

            break;
          case 'K':
            if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
            {
              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field1.Error = true;

              ExitState = "INVALID_VALUE";
            }

            if (!IsEmpty(export.Obligation.OtherStateAbbr))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Error = true;

              ExitState = "INVALID_VALUE";
            }

            if (!IsEmpty(export.InterstateRequest.Country))
            {
              var field1 = GetField(export.InterstateRequest, "country");

              field1.Error = true;

              ExitState = "INVALID_VALUE";
            }

            break;
          case ' ':
            export.Obligation.OrderTypeCode = "K";
            export.Obligation.OtherStateAbbr = "";
            export.InterstateRequest.OtherStateCaseId = "";
            export.InterstateRequest.Country = "";

            break;
          default:
            var field = GetField(export.Obligation, "orderTypeCode");

            field.Error = true;

            ExitState = "FN0000_INVALID_INTERSTATE_IND";

            break;
        }
      }

Test3:

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (AsChar(export.ProtectAll.Flag) == 'Y' && AsChar
          (export.LegalIdPassed.Flag) != 'Y')
        {
          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ObligationType, "code");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationTransaction, "amount");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DebtDetail, "dueDt");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        if (Equal(global.Command, "ADD") && !
          IsEmpty(export.LegalAction.StandardNumber))
        {
          // ----------------------------------------------------------------------
          // This is the condition where we are flowing from LDET to OFEE and 
          // trying to add the new Obligation. If ADD is not successful we must
          // protect the data came from LDET. User can not update the data
          // entered in Legal.
          // -----------------------------------------------------------------------
          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.ObligationType, "code");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.ObligationTransaction, "amount");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.DebtDetail, "dueDt");

          field4.Color = "cyan";
          field4.Protected = true;
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -----------------------------------------------------------------------------
        // 06/29/2000           As per new business rules ( Per SME Marilyn 
        // Gasperich), user can not ADD  (PF5);  if an obligation is already
        // displaying on the screen. We must prevent the user from creating
        // duplicate data.
        //                                                         
        // -----  Vithal Madhira
        // ------------------------------------------------------------------------------
        if (Equal(global.Command, "ADD") && export
          .Obligation.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_OBLIGATION_AE";
        }
      }

      if (AsChar(export.LegalIdPassed.Flag) == 'Y' && !
        Equal(global.Command, "RETURN"))
      {
        export.LegalIdPassed.Flag = "";

        // =================================================
        // 7/17/99 - bud adams  -  Read Each did not qualify
        //   on Legal_Action Identifier.
        // 1/11/00 - bud adams  -  PR# 80445: Qualification was based
        //   on relationship to Legal_Action_Person on the mistaken
        //   assumption that that entity type is reliably maintained.
        //   Removed references to it, added reference to identify oblig
        //   based on relationship to CSE_Person.  Also added select
        //   criteria on Debt_Detail Retired_Date.  Removed ob_type
        //   from Read so data would not be retrieved unnecessarily.
        //   Removed 'sorted by DESC oblig generated-id'
        // 1/28/00 - bud adams  -  PR# 83295: Removed qualifier from
        //   read that tested Debt_Detail Retired_Dt so a message can
        //   be displayed to user that a retired one already exists.
        // =================================================
        if (ReadObligationTransactionObligationDebtDetail())
        {
          // --- Obligation already created for this legal action detail. So 
          // display it.
          MoveObligation3(entities.Obligation, export.Obligation);
          export.ObligationTransaction.SystemGeneratedIdentifier =
            entities.ObligationTransaction.SystemGeneratedIdentifier;

          if (Equal(entities.DebtDetail.RetiredDt, local.BlankDateWorkArea.Date))
            
          {
            global.Command = "DISPLAY";
          }
          else
          {
            export.Obligation.OrderTypeCode = "K";
            export.Obligation.OtherStateAbbr = "";
            export.InterstateRequest.Country = "";
            export.Obligation.Description = "";
            export.Obligation.HistoryInd = "";
            export.Obligation.PrimarySecondaryCode = "";
            export.Obligation.CreatedBy = "";
            export.Obligation.LastUpdatedBy = "";
            export.Obligation.SystemGeneratedIdentifier = 0;
            export.SuspendInterestInd.Flag = "";
            ExitState = "FN0000_DEACTIVE_OB_CONFIRM_TO_AD";
            global.Command = "BYPASS";

            goto Test4;
          }

          goto Test4;
        }

        ExitState = "FN0000_OBLIG_NOT_YET_CREATED";
        global.Command = "BYPASS";
        export.Obligation.OrderTypeCode = "K";
        export.Obligation.OtherStateAbbr = "";
        export.InterstateRequest.Country = "";
        export.InterstateRequest.OtherStateCaseId = "";
      }

Test4:

      // **** If obligation adjustments exist, don't allow the amount to be 
      // changed.
      if (AsChar(export.AdjustmentExists.Flag) == 'Y')
      {
        var field = GetField(export.ObligationTransaction, "amount");

        field.Color = "cyan";
        field.Protected = true;
      }

      // ***** MAIN-LINE *****
      switch(TrimEnd(global.Command))
      {
        case "RETLINK":
          var field = GetField(export.AlternateAddr, "number");

          field.Color = "green";
          field.Highlighting = Highlighting.Underscore;
          field.Protected = false;
          field.Focused = true;

          if (!IsEmpty(import.FlowFrom.Number))
          {
            MoveCsePersonsWorkSet1(import.FlowFrom, export.AlternateAddr);
          }

          break;
        case "DISPLAY":
          if (!IsEmpty(export.ObligorCsePerson.Number))
          {
            export.ObligorCsePersonsWorkSet.Number =
              export.ObligorCsePerson.Number;
            UseSiReadCsePerson2();

            if (IsEmpty(export.ObligorCsePersonsWorkSet.FormattedName))
            {
              export.ObligorCsePersonsWorkSet.FormattedName =
                TrimEnd(export.ObligorCsePersonsWorkSet.LastName) + ", " + TrimEnd
                (export.ObligorCsePersonsWorkSet.FirstName) + " " + export
                .ObligorCsePersonsWorkSet.MiddleInitial;
            }

            if (!IsEmpty(local.Eab.Type1))
            {
              var field1 = GetField(export.ObligorCsePerson, "number");

              field1.Error = true;

              ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
            }
          }

          if (!IsEmpty(export.AlternateAddr.Number))
          {
            UseSiReadCsePerson1();

            if (IsEmpty(export.AlternateAddr.FormattedName))
            {
              export.AlternateAddr.FormattedName =
                TrimEnd(export.AlternateAddr.LastName) + ", " + TrimEnd
                (export.AlternateAddr.FirstName) + " " + export
                .AlternateAddr.MiddleInitial;
            }

            if (!IsEmpty(local.Eab.Type1))
            {
              var field1 = GetField(export.AlternateAddr, "number");

              field1.Error = true;

              ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
            }
          }

          if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
          {
            UseFnGetOblFromHistMonaNxtran();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }
          }

          // =================================================
          // 5/24/99 - bud adams  -  When flowing from LDET and an
          //   obligation exists, then DISPLAY that data.
          // =================================================
          if (AsChar(local.DebtExists.Flag) != 'Y')
          {
            export.ObligationType.Classification =
              local.HardcodeFees.Classification;

            // **************************************************************
            //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
            // caseworker assignment
            // **************************************************************
            // * * Remove EXPORT to tbd_export_assign service_provider
            UseFnReadRecoveryObligation1();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
            export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
            export.PreviousDebtDetail.DueDt = export.DebtDetail.DueDt;
            export.HiddenObligor.Number = export.ObligorCsePerson.Number;
            export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;
            export.HiddenObligationType.Code = export.ObligationType.Code;
            export.HiddenLegalAction.StandardNumber =
              export.LegalAction.StandardNumber;
            MoveObligation6(export.Obligation, export.HiddenObligation);
            export.PreviousObligationTransaction.Amount =
              export.ObligationTransaction.Amount;
            export.HiddenInterstateRequest.Assign(export.InterstateRequest);

            // **************************************************************
            //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
            // caseworker assignment
            // **************************************************************
            if (Lt(export.SetupDate.Date, local.Current.Date))
            {
              export.ProtectAll.Flag = "Y";
            }

            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

            // **** If obligation adjustments exist, don't allow the amount to 
            // be changed.
            var field1 = GetField(export.ObligationTransaction, "amount");

            field1.Color = "cyan";
            field1.Protected = true;

            // =================================================
            // 1/28/00 - b adams  -  PR# 83295: If a second FEE obligation
            //   is about to be added to a Legal_Action_Detail, display an
            //   appropriate message so the user is aware, before they
            //   add the next one.
            // =================================================
            if (AsChar(local.DeactiveObligationExists.Flag) == 'Y')
            {
              ExitState = "FN0000_DEACTIVE_OB_CONFIRM_TO_AD";
            }
          }
          else if (IsExitState("FN0000_MULT_RECOVERY_OBLIG"))
          {
            // ***---  Added CASE and exit state re-assignment - badams - 5/12/
            // 99
            ExitState = "FN0000_MULT_OBLIGATIONS_FOUND";
            export.HiddenObligor.Number = "";
            export.HiddenObligation.SystemGeneratedIdentifier = 0;
          }
          else
          {
            export.HiddenObligor.Number = "";
            export.HiddenObligation.SystemGeneratedIdentifier = 0;
            export.Obligation.OrderTypeCode = "K";
            export.Obligation.OtherStateAbbr = "";
            export.SetupDate.Date = local.BlankDateWorkArea.Date;

            return;
          }

          if (!IsEmpty(export.HiddenObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          // =================================================
          // 10/30/98 - B  Adams  -  Non-Court Ordered obligations can be
          //   interstate, and the Finance User must be able to enter the
          //   appropriate information for those instances.
          // =================================================
          switch(AsChar(export.Obligation.OrderTypeCode))
          {
            case 'I':
              UseFnRetrieveInterstateRequest();

              break;
            case 'K':
              export.Obligation.OtherStateAbbr = "";

              // ------------------------------------------------------------
              // This is to keep the Hidden_export  and  export in SYNC.
              // -----------------------------------------------------------------
              export.HiddenObligation.OtherStateAbbr = "";
              export.InterstateRequest.Country = "";
              export.InterstateRequest.OtherStateCaseId = "";

              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "orderTypeCode");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field4.Color = "cyan";
              field4.Protected = true;

              break;
            default:
              export.Obligation.OrderTypeCode = "K";
              export.Obligation.OtherStateAbbr = "";

              break;
          }

          if (ReadObligCollProtectionHist())
          {
            export.CollProtExistsInd.Flag = "Y";
          }
          else
          {
            export.CollProtExistsInd.Flag = "N";
          }

          // ***---  end of CASE DISPLAY
          break;
        case "ADD":
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }

          // ===========================================================================
          // PR# 102555     V.Madhira       09/21/00    This code let  the user 
          // to add an obligation for AP on an open case only.
          // ===========================================================================
          local.CreateObligation.Flag = "N";

          if (ReadCase())
          {
            local.CreateObligation.Flag = "Y";
          }

          if (AsChar(local.CreateObligation.Flag) == 'Y')
          {
            // =================================================================
            // This person is an AP on an OPEN case. So continue processing
            // ..........
            //                                                   
            // Vithal Madhira (09/21/00)
            // ===============================================================
          }
          else
          {
            var field1 = GetField(export.ObligorCsePerson, "number");

            field1.Error = true;

            ExitState = "FN0000_INVALID_CSE_PERSON";

            goto Test5;
          }

          if (IsEmpty(export.Obligation.OrderTypeCode))
          {
            export.Obligation.OrderTypeCode = "K";
          }

          if (IsEmpty(export.Obligation.OtherStateAbbr) && AsChar
            (export.Obligation.OrderTypeCode) == 'K')
          {
            export.Obligation.OtherStateAbbr = "KS";
          }

          // =================================================
          // 2/17/1999 - B Adams  -  Non-court-ordered fees will always
          //   the suspend interest flag set to "Y"
          // =================================================
          if (IsEmpty(export.LegalAction.StandardNumber))
          {
            export.SuspendInterestInd.Flag = "Y";
            local.HistoryIndicator.Flag = "Y";
          }
          else if (IsEmpty(export.SuspendInterestInd.Flag))
          {
            export.SuspendInterestInd.Flag = "N";
            local.HistoryIndicator.Flag = "N";
          }

          UseFnCreateObligation();
          export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
          export.Obligation.CreatedBy = global.UserId;

          if (Equal(export.Obligation.OtherStateAbbr, "KS"))
          {
            export.Obligation.OtherStateAbbr = "";
          }

          if (IsExitState("DEFAULT_RULE_NF"))
          {
            ExitState = "DEFAULT_RULE_NF_RB";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ProtectAll.Flag = "N";

            goto Test5;
          }

          // *** Set the Indicators ***
          if (IsEmpty(export.ObligationPaymentSchedule.FrequencyCode))
          {
            export.PaymentScheduleInd.Flag = "N";
          }
          else
          {
            export.PaymentScheduleInd.Flag = "Y";
          }

          if (AsChar(export.Obligation.OrderTypeCode) == 'I')
          {
            export.InterstateOblgInd.Flag = "Y";
          }
          else
          {
            export.InterstateOblgInd.Flag = "N";
          }

          export.ManualDistributionInd.Flag = "N";
          export.DebtDetail.BalanceDueAmt = export.ObligationTransaction.Amount;
          export.DebtDetail.InterestBalanceDueAmt = 0;
          UseFnCreateObligationTransaction();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ProtectAll.Flag = "N";

            goto Test5;
          }

          // **************************************************************
          //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
          // caseworker assignment
          // **************************************************************
          // VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
          // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  END WR020130  
          // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          // Aug 11, 1999, maf - Added set of legal attributes - events were not
          // working because the business object code needs to be set to 'LEA'
          // in order to end the legal monitored activity.
          if (entities.LegalAction.Identifier == 0)
          {
            local.Infrastructure.BusinessObjectCd = "OBL";
            local.Infrastructure.DenormNumeric12 =
              export.ObligationTransaction.SystemGeneratedIdentifier;
            local.Infrastructure.DenormText12 = local.HardcodeDebt.Type1;
            local.Infrastructure.ReferenceDate = local.Current.Date;
          }
          else
          {
            local.Infrastructure.BusinessObjectCd = "LEA";
            local.Infrastructure.DenormNumeric12 =
              import.LegalAction.Identifier;
            local.Infrastructure.DenormText12 =
              import.LegalAction.CourtCaseNumber ?? "";
            local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
          }

          local.Infrastructure.SituationNumber = 0;
          local.Infrastructure.CsePersonNumber = export.ObligorCsePerson.Number;
          local.Infrastructure.EventId = 45;
          local.Infrastructure.UserId = "OFEE";
          UseFnCabRaiseEvent();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.ProtectAll.Flag = "N";

            goto Test5;
          }

          export.BalanceOwed.TotalCurrency =
            export.ObligationTransaction.Amount;
          export.InterestOwed.TotalCurrency = 0;
          export.TotalOwed.TotalCurrency = export.ObligationTransaction.Amount;
          export.HiddenObligor.Number = export.ObligorCsePerson.Number;
          export.HiddenObligationType.Code = export.ObligationType.Code;
          export.PreviousDebtDetail.DueDt = export.DebtDetail.DueDt;
          export.HiddenLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.PreviousObligationTransaction.Amount =
            export.ObligationTransaction.Amount;

          // *** Since the Obligation is successfully created, Setup the 
          // alternate address for this Obligation, if any.
          // ***--- Sumanta - MTW - 04/30/97
          // ***--- Call to CAB_set_alternate_address has been deleted.
          //        The alternate (cse_person) will be associated to
          //        the obligation
          // ***---
          if (!IsEmpty(export.AlternateAddr.Number))
          {
            // =================================================
            // 1/4/99 - b adams  -  removed the CRUD and replaced it with
            //   this, which is used commonly among all Debt screens.  This
            //   makes maintenance much easier...
            // =================================================
            UseFnUpdateAlternateAddress();

            if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
            {
              var field1 = GetField(export.ObligorCsePerson, "number");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
            {
              var field1 = GetField(export.AlternateAddr, "number");

              field1.Error = true;
            }
            else
            {
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }
          }

          if (!IsEmpty(export.InterstateRequest.OtherStateCaseId) || !
            IsEmpty(export.InterstateRequest.Country))
          {
            // =================================================
            // 2/23/1999 - b adams  -  changed the IF constructs to make
            //   sense.  Replace the CRUD actions, which were not properly
            //   qualified, and made a CAB since it is functionality common
            //   to all debt PrADs.
            // =================================================
            UseFnCreateInterstateRqstOblign();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Obligation.Assign(local.BlankObligation);

              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field1.Error = true;

              export.BalanceOwed.TotalCurrency = 0;
              export.TotalOwed.TotalCurrency = 0;
              export.InterestOwed.TotalCurrency = 0;
              global.Command = "BYPASS";
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_CREATE_OK";
            export.ProtectAll.Flag = "Y";
            export.AddSuccess.Flag = "Y";
            export.HiddenInterstateRequest.Assign(export.InterstateRequest);

            // **************************************************************
            //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
            // caseworker assignment
            // **************************************************************
            if (!IsEmpty(export.HiddenObligation.OtherStateAbbr))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
            {
              var field1 = GetField(export.Obligation, "orderTypeCode");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.Country))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (AsChar(export.HiddenObligation.OrderTypeCode) == 'K')
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "orderTypeCode");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }

          // ***---  end of CASE ADD
          break;
        case "UPDATE":
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }

          // **************************************************************
          //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
          // caseworker assignment
          // **************************************************************
          // VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
          // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  END WR020130  
          // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
          if (IsEmpty(export.Obligation.OrderTypeCode))
          {
            export.Obligation.OrderTypeCode = "K";
          }

          // =================================================
          // 12/16/98 - B Adams  -  Interstate obligations are not going to
          //   be so designated by Legal now.  We will enter it here, but
          //   can only be updated on the same day it was created.
          // =================================================
          // =================================================
          // 1/5/00 - b adams  -  PR# xxxxx: Just like OREC, only if
          //   the obligation was created today and there are no
          //   adjustments can the debt_detail balance due amt be changed.
          // =================================================
          if (Equal(export.SetupDate.Date, local.Current.Date) && AsChar
            (export.AdjustmentExists.Flag) == 'N')
          {
            local.UpdateBalanceDueAmt.Flag = "Y";
          }
          else
          {
            local.UpdateBalanceDueAmt.Flag = "N";
          }

          UseFnUpdateRecoveryObligation();

          if (Equal(export.Obligation.OtherStateAbbr, "KS"))
          {
            export.Obligation.OtherStateAbbr = "";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }

          // : Update totals on CSE Person Account and Monthly Obligor Summary.
          // =================================================
          // 2/5/1999 - bud adams  -  deleted this action diagram; inappropriate
          //   for any on-line procedure, and no other debt screen does
          //   this kind of thing.
          // =================================================
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenObligor.Number = "";
            export.HiddenObligation.SystemGeneratedIdentifier = 0;
            export.PreviousDebtDetail.DueDt = export.DebtDetail.DueDt;
            export.PreviousObligationTransaction.Amount =
              export.ObligationTransaction.Amount;
          }
          else
          {
            goto Test5;
          }

          // *** Since the Obligation was successfully Updated, Set up the new 
          // alternate address for the Obligation if any.
          // ***--- Sumanta - MTW - 04/29/97
          // ***--- If the alt addr is present, associate to the obligation ..
          // ***---
          if (!Equal(export.AlternateAddr.Number,
            export.HiddenAlternateAddr.Number))
          {
            // =================================================
            // 1/4/99 - b adams  -  Took out the CRUD actions, common to
            //   all Debt screens, and put them in one CAB for ease of 
            // maintenance
            // =================================================
            UseFnUpdateAlternateAddress();

            if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
            {
              var field1 = GetField(export.AlternateAddr, "number");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
            {
              var field1 = GetField(export.AlternateAddr, "number");

              field1.Error = true;
            }
            else
            {
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }
          }

          if (!Equal(export.InterstateRequest.OtherStateCaseId,
            export.HiddenInterstateRequest.OtherStateCaseId))
          {
            // =================================================
            // 2/25/1999 - bud adams  -  The code in the 'update' action
            //   block was incorrect.  Removed it, and added this CAB here.
            // =================================================
            UseFnUpdateInterstateRqstOblign();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }
          }

          if (!Equal(export.InterstateRequest.Country,
            export.HiddenInterstateRequest.Country))
          {
            // =================================================
            // 2/25/1999 - bud adams  -  The code in the 'update' action
            //   block was incorrect.  Removed it, and added this CAB here.
            // =================================================
            UseFnUpdateInterstateRqstOblign();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }
          }

          // **************************************************************
          //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
          // caseworker assignment
          // **************************************************************
          // * * Remove EXPORT to tbd_export_assign service_provider
          UseFnReadRecoveryObligation2();

          if (Equal(export.Obligation.OtherStateAbbr, "KS"))
          {
            export.Obligation.OtherStateAbbr = "";
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
            export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
            export.PreviousDebtDetail.DueDt = export.DebtDetail.DueDt;
            export.HiddenObligor.Number = export.ObligorCsePerson.Number;
            export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;
            export.HiddenObligationType.Code = export.ObligationType.Code;
            export.HiddenLegalAction.StandardNumber =
              export.LegalAction.StandardNumber;
            MoveObligation6(export.Obligation, export.HiddenObligation);
            export.PreviousObligationTransaction.Amount =
              export.ObligationTransaction.Amount;
            export.HiddenInterstateRequest.Assign(export.InterstateRequest);

            // **************************************************************
            //  WR020130  09/26/01  PPhinney  Stop assigning Fee Obligations w/
            // caseworker assignment
            // **************************************************************
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

            if (!IsEmpty(export.HiddenObligation.OtherStateAbbr))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
            {
              var field1 = GetField(export.Obligation, "orderTypeCode");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.Country))
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (AsChar(export.HiddenObligation.OrderTypeCode) == 'K')
            {
              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "orderTypeCode");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field4.Color = "cyan";
              field4.Protected = true;
            }
          }
          else
          {
            export.HiddenObligor.Number = "";
            export.HiddenObligation.SystemGeneratedIdentifier = 0;
          }

          // ***---  end of CASE UPDATE
          break;
        case "DELETE":
          if (!Equal(export.SetupDate.Date, local.Current.Date))
          {
            ExitState = "FN0000_CANT_DEL_AFTER_CREAT_DATE";

            goto Test5;
          }

          // : If obligation is active, delete of obligation is not allowed
          // =================================================
          // 4/6/99 - b adams  -  Use FN_Check_Obligation_For_Activity
          //   deleted.  No longer necessary; only obligations created
          //   on CURRENT_DATE can be deleted.
          // =================================================
          UseFnRemoveObligation();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenObligor.Number = "";
            export.HiddenObligation.SystemGeneratedIdentifier = 0;
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
          else
          {
          }

          break;
        case "BYPASS":
          export.Object1.Text20 = "";

          break;
        case "LIST":
          local.NoOfPromptsSelected.Count = 0;

          if (AsChar(export.ObligorPrompt.SelectChar) != 'S' && AsChar
            (export.AssignPrompt.SelectChar) != 'S' && AsChar
            (export.AltAddPrompt.Text1) != 'S')
          {
            UseEabCursorPosition();

            switch(local.CursorPosition.Row)
            {
              case 3:
                export.ObligorPrompt.SelectChar = "S";

                break;
              case 4:
                export.AssignPrompt.SelectChar = "S";

                break;
              case 5:
                export.AltAddPrompt.Text1 = "S";

                break;
              default:
                break;
            }
          }

          switch(AsChar(export.AltAddPrompt.Text1))
          {
            case 'S':
              ++local.NoOfPromptsSelected.Count;

              break;
            case '+':
              break;
            case ' ':
              break;
            default:
              var field1 = GetField(export.AltAddPrompt, "text1");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
          }

          switch(AsChar(export.AssignPrompt.SelectChar))
          {
            case '+':
              break;
            case 'S':
              ++local.NoOfPromptsSelected.Count;

              break;
            case ' ':
              break;
            default:
              var field1 = GetField(export.AssignPrompt, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
          }

          switch(AsChar(export.ObligorPrompt.SelectChar))
          {
            case 'S':
              ++local.NoOfPromptsSelected.Count;

              break;
            case '+':
              break;
            case ' ':
              break;
            default:
              var field1 = GetField(export.ObligorPrompt, "selectChar");

              field1.Error = true;

              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              break;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }

          if (local.NoOfPromptsSelected.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            // =================================================
            // 12/31/98 - Highlite the offending "S"-es in reverse order, so
            //   the cursor ends up on the first one on the screen
            // =================================================
            if (AsChar(export.AltAddPrompt.Text1) == 'S')
            {
              var field1 = GetField(export.AltAddPrompt, "text1");

              field1.Error = true;
            }

            if (AsChar(export.AssignPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.AssignPrompt, "selectChar");

              field1.Error = true;
            }

            if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
            {
              var field1 = GetField(export.ObligorPrompt, "selectChar");

              field1.Error = true;
            }

            goto Test5;
          }
          else if (local.NoOfPromptsSelected.Count == 0)
          {
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
            global.Command = "DISPLAY";

            goto Test5;
          }

          if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
          {
            // : List for Obligor HERE!!!!
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
            export.ObligorPrompt.SelectChar = "+";

            goto Test5;
          }

          if (AsChar(export.AssignPrompt.SelectChar) == 'S')
          {
            // =================================================
            // 6/23/99 - b adams  -  Allow the user to specify the Assigned
            //   To person at ADD time instead of at UPDATE time.
            // =================================================
            // =================================================
            // 3/31/1999 - bud adams  -  Read improperly qualified.
            //   And, tonight is a blue moon.
            // =================================================
            if (ReadCsePersonAccount())
            {
              export.Pass.Type1 = entities.CsePersonAccount.Type1;
            }
            else
            {
              ExitState = "FN0000_OBLIGOR_ACCT_NF";
            }

            export.Object1.Text20 = "OBLIGATION";
            export.AssignPrompt.SelectChar = "+";
            ExitState = "ECO_LNK_TO_ASIN";

            goto Test5;
          }

          if (AsChar(export.AltAddPrompt.Text1) == 'S')
          {
            ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";
            export.AltAddPrompt.Text1 = "+";
          }

          break;
        case "MDIS":
          ExitState = "ECO_LNK_TO_MTN_MANUAL_DIST_INST";

          break;
        case "INMS":
          if (IsEmpty(export.LegalAction.StandardNumber))
          {
            // =================================================
            // 12/28/98 - B Adams  -  Non-court-ordered fees cannot have
            //   interest suspension because they have no interest.
            // =================================================
            ExitState = "FN0000_FLOW_FOR_C_O_ONLY";
          }
          else
          {
            ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";
          }

          if (!IsEmpty(export.HiddenObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (AsChar(export.HiddenObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "orderTypeCode");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "otherStateCaseId");

            field4.Color = "cyan";
            field4.Protected = true;
          }

          if (!Equal(export.SetupDate.Date, local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "country");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.ObligationTransaction, "amount");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DebtDetail, "dueDt");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.ObligorCsePerson, "number");

            field7.Color = "cyan";
            field7.Protected = true;
          }

          break;
        case "OPAY":
          ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

          break;
        case "OCTO":
          export.Last.Command = "OCTO";
          ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";

          break;
        case "OPSC":
          ExitState = "ECO_LNK_TO_LST_MTN_PYMNT_SCH";

          break;
        case "CSPM":
          local.PassedDueDate.Date = export.DebtDetail.DueDt;
          ExitState = "ECO_LNK_LST_MTN_OB_S_C_SUPP";

          break;
        case "COLP":
          ExitState = "ECO_LNK_TO_COLP";

          break;
        case "NEXT":
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
        case "PREV":
          ExitState = "ACO_NE0000_INVALID_COMMAND";

          break;
        case "RETURN":
          if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
            (export.HiddenNextTranInfo.LastTran, "SRPU"))
          {
            global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " +
              "XXNEXTXX";
          }
          else
          {
            ExitState = "ACO_NE0000_RETURN";
          }

          break;
        case "SIGNOFF":
          // : Sign-off Routine Here!!
          UseScCabSignoff();

          break;
        case "RTFRMLNK":
          break;
        case "RETFIPL":
          break;
        case "RETNAME":
          break;
        default:
          // : If hidden command is CONFIRM, the user was asked to confirm
          //   an add action. Any key may be pressed to cancel the add.
          if (AsChar(export.ConfirmObligAdd.Flag) == 'Y' || AsChar
            (export.ConfirmRetroDate.Flag) == 'Y')
          {
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            if (!Equal(export.SetupDate.Date, local.BlankDateWorkArea.Date))
            {
              var field1 = GetField(export.Obligation, "orderTypeCode");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "country");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.ObligationTransaction, "amount");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 = GetField(export.DebtDetail, "dueDt");

              field6.Color = "cyan";
              field6.Protected = true;

              var field7 = GetField(export.ObligorCsePerson, "number");

              field7.Color = "cyan";
              field7.Protected = true;
            }
          }

          break;
      }
    }

Test5:

    // <<< RBM   12/30/1997 >>>
    //    If the ADD, UPDATE or DELETE operations were not successful, perform a
    // CICS ROLLBACK
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE"))
    {
      if (!IsEmpty(export.HiddenObligation.OtherStateAbbr))
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
      {
        var field1 = GetField(export.Obligation, "orderTypeCode");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      if (!IsEmpty(export.HiddenInterstateRequest.Country))
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;
      }

      if (AsChar(export.HiddenObligation.OrderTypeCode) == 'K')
      {
        var field1 = GetField(export.Obligation, "otherStateAbbr");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.InterstateRequest, "country");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.Obligation, "orderTypeCode");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "otherStateCaseId");

        field4.Color = "cyan";
        field4.Protected = true;
      }

      if (!Equal(export.SetupDate.Date, local.BlankDateWorkArea.Date))
      {
        var field1 = GetField(export.Obligation, "orderTypeCode");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Obligation, "otherStateAbbr");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "country");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ObligationTransaction, "amount");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.DebtDetail, "dueDt");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ObligorCsePerson, "number");

        field7.Color = "cyan";
        field7.Protected = true;
      }

      if (!IsExitState("ACO_NI0000_CREATE_OK") && !
        IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") && !
        IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
      {
        // <<< Call to Fn_Eab_Rollback_CICS >>>
        UseEabRollbackCics();
      }
      else
      {
      }
    }

    if (AsChar(export.ProtectAll.Flag) == 'Y' && AsChar
      (export.LegalIdPassed.Flag) != 'Y')
    {
      var field1 = GetField(export.ObligorCsePerson, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligationTransaction, "amount");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.DebtDetail, "dueDt");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (!IsEmpty(export.LegalAction.StandardNumber) && export
      .Obligation.SystemGeneratedIdentifier == 0)
    {
      // -------------------------------------------------------------
      // This is the condition where user flowing from LDET to OFEE to ADD a new
      // obligation. Make sure that user can not update the data passed from
      // LDET. The following SET statements will take care of the above
      // condition.
      // ----------------------------------------------------------------
      var field1 = GetField(export.ObligorCsePerson, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligationTransaction, "amount");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.DebtDetail, "dueDt");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "RETLINK") || Equal(global.Command, "RTFRMLNK") || Equal
      (global.Command, "BYPASS"))
    {
      if (!Equal(export.SetupDate.Date, local.BlankDateWorkArea.Date))
      {
        var field1 = GetField(export.Obligation, "orderTypeCode");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Obligation, "otherStateAbbr");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.InterstateRequest, "country");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.ObligationTransaction, "amount");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.DebtDetail, "dueDt");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.ObligorCsePerson, "number");

        field7.Color = "cyan";
        field7.Protected = true;
      }
    }

    // =================================================
    // 12/31/98 - b adams  -  New Rule:  If alternate billing address
    //   is established in Legal, then it must be protected.
    // =================================================
    if (Equal(export.AlternateAddr.Char2, "LE"))
    {
      export.AltAddPrompt.Text1 = "";

      var field1 = GetField(export.AlternateAddr, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.AltAddPrompt, "text1");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    // =================================================
    // 1/28/00 - b adams  -  Until interest ever becomes part of
    //   KESSEP, we will display the Interest Suspension Indicator
    //   as a Y.  But we have NOT created interest suspension
    //   records for any obligations.
    // =================================================
    export.SuspendInterestInd.Flag = "Y";

    // ==============================================
    // 04/15/2000 - K. Price
    // SI_READ_CSE_PERSON calls ADABAS to get the first,
    // middle initial, and last name for a person.  When debugging
    // in trace mode, it will abend when calling this action block.
    // All calls to SI_READ_CSE_PERSON have been moved here
    // to facilitate disabling the calls quickly, and safely.
    // ===============================================
    if (!IsEmpty(export.ObligorCsePerson.Number))
    {
      export.ObligorCsePersonsWorkSet.Number = export.ObligorCsePerson.Number;
      UseSiReadCsePerson2();

      if (IsEmpty(export.ObligorCsePersonsWorkSet.FormattedName))
      {
        export.ObligorCsePersonsWorkSet.FormattedName =
          TrimEnd(export.ObligorCsePersonsWorkSet.LastName) + ", " + TrimEnd
          (export.ObligorCsePersonsWorkSet.FirstName) + " " + export
          .ObligorCsePersonsWorkSet.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field = GetField(export.ObligorCsePerson, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    if (!IsEmpty(export.AlternateAddr.Number))
    {
      UseSiReadCsePerson1();

      if (IsEmpty(export.AlternateAddr.FormattedName))
      {
        export.AlternateAddr.FormattedName =
          TrimEnd(export.AlternateAddr.LastName) + ", " + TrimEnd
          (export.AlternateAddr.FirstName) + " " + export
          .AlternateAddr.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field = GetField(export.AlternateAddr, "number");

        field.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
    target.Char2 = source.Char2;
  }

  private static void MoveCursorPosition(CursorPosition source,
    CursorPosition target)
  {
    target.Row = source.Row;
    target.Column = source.Column;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.CsePersonNumber = source.CsePersonNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveInterstateRequest1(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.OtherStateCaseStatus = source.OtherStateCaseStatus;
    target.Country = source.Country;
  }

  private static void MoveInterstateRequest2(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.OtherStateCaseId = source.OtherStateCaseId;
    target.Country = source.Country;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail1(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
  }

  private static void MoveLegalActionDetail2(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.DetailType = source.DetailType;
    target.JudgementAmount = source.JudgementAmount;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LastTran = source.LastTran;
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
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
  }

  private static void MoveObligation1(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.AsOfDtRecBal = source.AsOfDtRecBal;
    target.AsOdDtRecIntBal = source.AsOdDtRecIntBal;
    target.AsOfDtFeeBal = source.AsOfDtFeeBal;
    target.AsOfDtFeeIntBal = source.AsOfDtFeeIntBal;
    target.AsOfDtTotBalCurrArr = source.AsOfDtTotBalCurrArr;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation3(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation4(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation5(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.AsOfDtRecBal = source.AsOfDtRecBal;
    target.AsOdDtRecIntBal = source.AsOdDtRecIntBal;
    target.AsOfDtFeeBal = source.AsOfDtFeeBal;
    target.AsOfDtFeeIntBal = source.AsOfDtFeeIntBal;
    target.AsOfDtTotBalCurrArr = source.AsOfDtTotBalCurrArr;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation6(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation7(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation8(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation9(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private static void MoveObligationType3(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveObligationType4(ObligationType source,
    ObligationType target)
  {
    target.Code = source.Code;
    target.Classification = source.Classification;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabCursorPosition()
  {
    var useImport = new EabCursorPosition.Import();
    var useExport = new EabCursorPosition.Export();

    MoveCursorPosition(local.CursorPosition, useExport.CursorPosition);

    Call(EabCursorPosition.Execute, useImport, useExport);

    MoveCursorPosition(useExport.CursorPosition, local.CursorPosition);
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.ForLeftPad.Text10;
    useExport.TextWorkArea.Text10 = local.ForLeftPad.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.ForLeftPad.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Alternate.Number = export.AlternateAddr.Number;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCreateInterstateRqstOblign()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.ObligationType.SystemGeneratedIdentifier =
      import.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = import.ObligorCsePerson.Number;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnCreateObligation()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.HcOtCAccruingClassifi.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HistoryIndicator.Flag = local.HistoryIndicator.Flag;
    useImport.HardcodeCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeRecovery.Classification;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeFees.Classification;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    MoveCsePerson(export.ObligorCsePerson, useImport.CsePerson);
    MoveObligation8(export.Obligation, useImport.Obligation);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.ObligationType.Assign(export.ObligationType);

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation2(useExport.Obligation, export.Obligation);
  }

  private void UseFnCreateObligationTransaction()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodedLapObligor.AccountType;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    useImport.HcCpaSupportedPerson.Type1 =
      local.HardcodeCpaSupportedPerson.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrualInstruc,
      useImport.HcOtrnDtAccrual);
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeRecovery.Classification;
    useImport.HcDdshActiveStatus.Code = local.HardcodeActive.Code;
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeFees.Classification;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.Hardcoded.Type1 = local.HardcodeDebt.Type1;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.Amount =
      export.ObligationTransaction.Amount;
    MoveDebtDetail(export.DebtDetail, useImport.DebtDetail);
    MoveObligationType3(export.ObligationType, useImport.ObligationType);
    MoveLegalAction3(export.LegalAction, useImport.LegalAction);

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.ObligationTransaction.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnGetOblFromHistMonaNxtran()
  {
    var useImport = new FnGetOblFromHistMonaNxtran.Import();
    var useExport = new FnGetOblFromHistMonaNxtran.Export();

    useImport.NextTranInfo.InfrastructureId =
      export.HiddenNextTranInfo.InfrastructureId;

    Call(FnGetOblFromHistMonaNxtran.Execute, useImport, useExport);

    export.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
    MoveObligationType1(useExport.ObligationType, export.ObligationType);
  }

  private void UseFnHardcodeLegal()
  {
    var useImport = new FnHardcodeLegal.Import();
    var useExport = new FnHardcodeLegal.Export();

    Call(FnHardcodeLegal.Execute, useImport, useExport);

    local.HardcodedLapObligor.AccountType = useExport.Obligor.AccountType;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.HardcodeCpaSupportedPerson.Type1 = useExport.CpaSupportedPerson.Type1;
    MoveObligationTransaction2(useExport.OtrnDtVoluntary,
      local.HardcodeOtrnDtVoluntary);
    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HardcodeOtrnDtAccrualInstruc);
    local.HardcodeOtCAccruingClassific.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodeOtCVoluntaryClassifi.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    MoveObligationTransaction2(useExport.OtrnDtDebtDetail,
      local.HardcodeOtrnDtDebtDetail);
    local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeRecovery.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.HardcodeActive.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeSecondary.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodePrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodeFees.Classification =
      useExport.OtCFeesClassification.Classification;
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodeOtrnTDebtAdjustment.Type1 =
      useExport.OtrnTDebtAdjustment.Type1;
  }

  private void UseFnReadRecoveryObligation1()
  {
    var useImport = new FnReadRecoveryObligation.Import();
    var useExport = new FnReadRecoveryObligation.Export();

    useImport.HcOtcFee.Classification = local.HardcodeFees.Classification;
    useImport.Current.Date = local.Current.Date;
    useImport.HcDdshActiveStatus.Code = local.HardcodeActive.Code;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    MoveObligationType4(export.ObligationType, useImport.ObligationType);
    MoveCsePerson(export.ObligorCsePerson, useImport.Obligor);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOtrnTDebtAdjustment.Type1 =
      local.HardcodeOtrnTDebtAdjustment.Type1;

    Call(FnReadRecoveryObligation.Execute, useImport, useExport);

    export.Assign1.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
    export.SuspendInterestInd.Flag = useExport.InterestSuspensionInd.Flag;
    MoveObligation1(useExport.Obligation, export.Obligation);
    MoveObligationTransaction1(useExport.ObligationTransaction,
      export.ObligationTransaction);
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    MoveLegalAction1(useExport.OrderClassification, export.LegalAction);
    export.PaymentScheduleInd.Flag = useExport.PaymentScheduleInd.Flag;
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.ObligationAmt.TotalCurrency = useExport.ObligationAmt.TotalCurrency;
    export.BalanceOwed.TotalCurrency = useExport.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = useExport.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = useExport.TotalOwed.TotalCurrency;
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    MoveCsePersonsWorkSet2(useExport.Alternate, export.AlternateAddr);
    MoveInterstateRequest2(useExport.InterstateRequest, export.InterstateRequest);
      
    export.DebtDetailStatusHistory.EffectiveDt =
      useExport.DebtDetailStatusHistory.EffectiveDt;
    export.AdjustmentExists.Flag = useExport.AdjustmentExists.Flag;
    export.LegalActionDetail.Number = useExport.LegalActionDetail.Number;
    local.DeactiveObligationExists.Flag =
      useExport.DeactiveObligtionExists.Flag;
    MoveCodeValue(useExport.Country, export.Country);
  }

  private void UseFnReadRecoveryObligation2()
  {
    var useImport = new FnReadRecoveryObligation.Import();
    var useExport = new FnReadRecoveryObligation.Export();

    useImport.HcOtcFee.Classification = local.HardcodeFees.Classification;
    useImport.Current.Date = local.Current.Date;
    useImport.HcDdshActiveStatus.Code = local.HardcodeActive.Code;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    MoveObligationType4(export.ObligationType, useImport.ObligationType);
    MoveCsePerson(export.ObligorCsePerson, useImport.Obligor);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;

    Call(FnReadRecoveryObligation.Execute, useImport, useExport);

    export.Assign1.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
    export.SuspendInterestInd.Flag = useExport.InterestSuspensionInd.Flag;
    MoveObligation1(useExport.Obligation, export.Obligation);
    MoveObligationTransaction1(useExport.ObligationTransaction,
      export.ObligationTransaction);
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    MoveLegalAction1(useExport.OrderClassification, export.LegalAction);
    export.PaymentScheduleInd.Flag = useExport.PaymentScheduleInd.Flag;
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.ObligationAmt.TotalCurrency = useExport.ObligationAmt.TotalCurrency;
    export.BalanceOwed.TotalCurrency = useExport.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = useExport.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = useExport.TotalOwed.TotalCurrency;
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    MoveCsePersonsWorkSet2(useExport.Alternate, export.AlternateAddr);
    MoveInterstateRequest2(useExport.InterstateRequest, export.InterstateRequest);
      
    MoveCodeValue(useExport.Country, export.Country);
  }

  private void UseFnRemoveObligation()
  {
    var useImport = new FnRemoveObligation.Import();
    var useExport = new FnRemoveObligation.Export();

    MoveObligation9(import.Obligation, useImport.Obligation);
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCRecoveryClassifi.Classification =
      local.HardcodeRecovery.Classification;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Obligor.Number = import.ObligorCsePerson.Number;

    Call(FnRemoveObligation.Execute, useImport, useExport);
  }

  private void UseFnRetrieveInterstateRequest()
  {
    var useImport = new FnRetrieveInterstateRequest.Import();
    var useExport = new FnRetrieveInterstateRequest.Export();

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligation7(export.Obligation, useImport.Obligor);
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      
    useImport.Current.Date = local.Current.Date;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(FnRetrieveInterstateRequest.Execute, useImport, useExport);

    export.InterstateRequest.Assign(useExport.InterstateRequest);
    export.Country.Description = useExport.Country.Description;
  }

  private void UseFnRetrieveLeglForRecAndFee()
  {
    var useImport = new FnRetrieveLeglForRecAndFee.Import();
    var useExport = new FnRetrieveLeglForRecAndFee.Export();

    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodedLapObligor.AccountType;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtRecoveryOrFee.Classification =
      local.HardcodeFees.Classification;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;

    Call(FnRetrieveLeglForRecAndFee.Execute, useImport, useExport);

    export.AlternateAddr.Assign(useExport.Alternate);
    MoveLegalActionDetail2(useExport.LegalActionDetail, export.LegalActionDetail);
      
    MoveLegalAction2(useExport.LegalAction, export.LegalAction);
    export.ObligationTransaction.Amount =
      useExport.ObligationTransaction.Amount;
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    export.ObligationPaymentSchedule.
      Assign(useExport.ObligationPaymentSchedule);
    MoveCsePerson(useExport.ObligorCsePerson, export.ObligorCsePerson);
    export.ObligorCsePersonsWorkSet.Assign(useExport.ObligorCsePersonsWorkSet);
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    local.DebtExists.Flag = useExport.DebtExists.Flag;
    export.Assign1.FormattedName =
      useExport.AssignCsePersonsWorkSet.FormattedName;
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseFnUpdateAlternateAddress()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.AlternateBillingAddress.Number = export.AlternateAddr.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Max.Date = local.Max.Date;
    useImport.Old.IntHGeneratedId =
      export.HiddenInterstateRequest.IntHGeneratedId;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.New1.IntHGeneratedId = export.InterstateRequest.IntHGeneratedId;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnUpdateRecoveryObligation()
  {
    var useImport = new FnUpdateRecoveryObligation.Import();
    var useExport = new FnUpdateRecoveryObligation.Export();

    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveObligation4(export.Obligation, useImport.Obligation);
    MoveObligationTransaction1(import.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.DebtDetail.DueDt = import.DebtDetail.DueDt;
    useImport.CsePerson.Number = import.ObligorCsePerson.Number;
    useImport.HardcodedObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.UpdateBalanceDueAmt.Flag = local.UpdateBalanceDueAmt.Flag;

    Call(FnUpdateRecoveryObligation.Execute, useImport, useExport);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.AlternateAddr.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, export.AlternateAddr);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNoAp", export.ObligorCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetString(command, "type", local.HardcodeObligor.Type1);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
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
        entities.LegalAction.Type1 = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    entities.CpObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetInt32(
          command, "otyIdentifier",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgIdentifier",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetDate(
          command, "deactivationDate",
          local.BlankDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CpObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 0);
        entities.CpObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 1);
        entities.CpObligCollProtectionHist.CspNumber = db.GetString(reader, 2);
        entities.CpObligCollProtectionHist.CpaType = db.GetString(reader, 3);
        entities.CpObligCollProtectionHist.OtyIdentifier =
          db.GetInt32(reader, 4);
        entities.CpObligCollProtectionHist.ObgIdentifier =
          db.GetInt32(reader, 5);
        entities.CpObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.CpObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligationTransactionObligationDebtDetail()
  {
    entities.DebtDetail.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransactionObligationDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", export.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "numb", export.ObligorCsePerson.Number);
        db.SetString(command, "type", export.ObligorCsePerson.Type1);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 8);
        entities.Obligation.Description = db.GetNullableString(reader, 9);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 10);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 11);
        entities.Obligation.CreatedBy = db.GetString(reader, 12);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 15);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 16);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 17);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 18);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 19);
        entities.DebtDetail.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of AdjustmentExists.
    /// </summary>
    [JsonPropertyName("adjustmentExists")]
    public Common AdjustmentExists
    {
      get => adjustmentExists ??= new();
      set => adjustmentExists = value;
    }

    /// <summary>
    /// A value of Object1.
    /// </summary>
    [JsonPropertyName("object1")]
    public SpTextWorkArea Object1
    {
      get => object1 ??= new();
      set => object1 = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenAlternateAddr.
    /// </summary>
    [JsonPropertyName("hiddenAlternateAddr")]
    public CsePersonsWorkSet HiddenAlternateAddr
    {
      get => hiddenAlternateAddr ??= new();
      set => hiddenAlternateAddr = value;
    }

    /// <summary>
    /// A value of AddSuccess.
    /// </summary>
    [JsonPropertyName("addSuccess")]
    public Common AddSuccess
    {
      get => addSuccess ??= new();
      set => addSuccess = value;
    }

    /// <summary>
    /// A value of LegalIdPassed.
    /// </summary>
    [JsonPropertyName("legalIdPassed")]
    public Common LegalIdPassed
    {
      get => legalIdPassed ??= new();
      set => legalIdPassed = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of AssignPrompt.
    /// </summary>
    [JsonPropertyName("assignPrompt")]
    public Common AssignPrompt
    {
      get => assignPrompt ??= new();
      set => assignPrompt = value;
    }

    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public CsePersonsWorkSet Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of TbdImportAssign.
    /// </summary>
    [JsonPropertyName("tbdImportAssign")]
    public ServiceProvider TbdImportAssign
    {
      get => tbdImportAssign ??= new();
      set => tbdImportAssign = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of LastUpdDate.
    /// </summary>
    [JsonPropertyName("lastUpdDate")]
    public DateWorkArea LastUpdDate
    {
      get => lastUpdDate ??= new();
      set => lastUpdDate = value;
    }

    /// <summary>
    /// A value of ZdelImportDesignatedPayee.
    /// </summary>
    [JsonPropertyName("zdelImportDesignatedPayee")]
    public CsePersonsWorkSet ZdelImportDesignatedPayee
    {
      get => zdelImportDesignatedPayee ??= new();
      set => zdelImportDesignatedPayee = value;
    }

    /// <summary>
    /// A value of FlowFrom.
    /// </summary>
    [JsonPropertyName("flowFrom")]
    public CsePersonsWorkSet FlowFrom
    {
      get => flowFrom ??= new();
      set => flowFrom = value;
    }

    /// <summary>
    /// A value of AltAddPrompt.
    /// </summary>
    [JsonPropertyName("altAddPrompt")]
    public TextWorkArea AltAddPrompt
    {
      get => altAddPrompt ??= new();
      set => altAddPrompt = value;
    }

    /// <summary>
    /// A value of AlternateAddr.
    /// </summary>
    [JsonPropertyName("alternateAddr")]
    public CsePersonsWorkSet AlternateAddr
    {
      get => alternateAddr ??= new();
      set => alternateAddr = value;
    }

    /// <summary>
    /// A value of InterstateOblgInd.
    /// </summary>
    [JsonPropertyName("interstateOblgInd")]
    public Common InterstateOblgInd
    {
      get => interstateOblgInd ??= new();
      set => interstateOblgInd = value;
    }

    /// <summary>
    /// A value of SetupDate.
    /// </summary>
    [JsonPropertyName("setupDate")]
    public DateWorkArea SetupDate
    {
      get => setupDate ??= new();
      set => setupDate = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of PreviousObligationTransaction.
    /// </summary>
    [JsonPropertyName("previousObligationTransaction")]
    public ObligationTransaction PreviousObligationTransaction
    {
      get => previousObligationTransaction ??= new();
      set => previousObligationTransaction = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of PreviousDebtDetail.
    /// </summary>
    [JsonPropertyName("previousDebtDetail")]
    public DebtDetail PreviousDebtDetail
    {
      get => previousDebtDetail ??= new();
      set => previousDebtDetail = value;
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
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Common ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of SuspendInterestInd.
    /// </summary>
    [JsonPropertyName("suspendInterestInd")]
    public Common SuspendInterestInd
    {
      get => suspendInterestInd ??= new();
      set => suspendInterestInd = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of HiddenObligor.
    /// </summary>
    [JsonPropertyName("hiddenObligor")]
    public CsePerson HiddenObligor
    {
      get => hiddenObligor ??= new();
      set => hiddenObligor = value;
    }

    /// <summary>
    /// A value of HiddenObligation.
    /// </summary>
    [JsonPropertyName("hiddenObligation")]
    public Obligation HiddenObligation
    {
      get => hiddenObligation ??= new();
      set => hiddenObligation = value;
    }

    /// <summary>
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ConfirmObligAdd.
    /// </summary>
    [JsonPropertyName("confirmObligAdd")]
    public Common ConfirmObligAdd
    {
      get => confirmObligAdd ??= new();
      set => confirmObligAdd = value;
    }

    /// <summary>
    /// A value of ConfirmRetroDate.
    /// </summary>
    [JsonPropertyName("confirmRetroDate")]
    public Common ConfirmRetroDate
    {
      get => confirmRetroDate ??= new();
      set => confirmRetroDate = value;
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
    /// A value of ProtectAll.
    /// </summary>
    [JsonPropertyName("protectAll")]
    public Common ProtectAll
    {
      get => protectAll ??= new();
      set => protectAll = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of TbdImportHiddenAssign.
    /// </summary>
    [JsonPropertyName("tbdImportHiddenAssign")]
    public ServiceProvider TbdImportHiddenAssign
    {
      get => tbdImportHiddenAssign ??= new();
      set => tbdImportHiddenAssign = value;
    }

    /// <summary>
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
    }

    /// <summary>
    /// A value of CollProtExistsInd.
    /// </summary>
    [JsonPropertyName("collProtExistsInd")]
    public Common CollProtExistsInd
    {
      get => collProtExistsInd ??= new();
      set => collProtExistsInd = value;
    }

    private CodeValue country;
    private Common adjustmentExists;
    private SpTextWorkArea object1;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private CsePersonsWorkSet hiddenAlternateAddr;
    private Common addSuccess;
    private Common legalIdPassed;
    private InterstateRequest interstateRequest;
    private Common assignPrompt;
    private CsePersonsWorkSet assign1;
    private ServiceProvider tbdImportAssign;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea lastUpdDate;
    private CsePersonsWorkSet zdelImportDesignatedPayee;
    private CsePersonsWorkSet flowFrom;
    private TextWorkArea altAddPrompt;
    private CsePersonsWorkSet alternateAddr;
    private Common interstateOblgInd;
    private DateWorkArea setupDate;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction previousObligationTransaction;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private DebtDetail previousDebtDetail;
    private ObligationType obligationType;
    private Common obligationTypePrompt;
    private Common manualDistributionInd;
    private Common paymentScheduleInd;
    private Common suspendInterestInd;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private CsePerson hiddenObligor;
    private Obligation hiddenObligation;
    private ObligationType hiddenObligationType;
    private LegalAction hiddenLegalAction;
    private Common last;
    private Common confirmObligAdd;
    private Common confirmRetroDate;
    private NextTranInfo hiddenNextTranInfo;
    private Common protectAll;
    private InterstateRequest hiddenInterstateRequest;
    private ServiceProvider tbdImportHiddenAssign;
    private Common countryPrompt;
    private Common collProtExistsInd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of AdjustmentExists.
    /// </summary>
    [JsonPropertyName("adjustmentExists")]
    public Common AdjustmentExists
    {
      get => adjustmentExists ??= new();
      set => adjustmentExists = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    /// <summary>
    /// A value of HiddenAlternateAddr.
    /// </summary>
    [JsonPropertyName("hiddenAlternateAddr")]
    public CsePersonsWorkSet HiddenAlternateAddr
    {
      get => hiddenAlternateAddr ??= new();
      set => hiddenAlternateAddr = value;
    }

    /// <summary>
    /// A value of AddSuccess.
    /// </summary>
    [JsonPropertyName("addSuccess")]
    public Common AddSuccess
    {
      get => addSuccess ??= new();
      set => addSuccess = value;
    }

    /// <summary>
    /// A value of LegalIdPassed.
    /// </summary>
    [JsonPropertyName("legalIdPassed")]
    public Common LegalIdPassed
    {
      get => legalIdPassed ??= new();
      set => legalIdPassed = value;
    }

    /// <summary>
    /// A value of ZdelExportStateAsPayee.
    /// </summary>
    [JsonPropertyName("zdelExportStateAsPayee")]
    public CsePersonsWorkSet ZdelExportStateAsPayee
    {
      get => zdelExportStateAsPayee ??= new();
      set => zdelExportStateAsPayee = value;
    }

    /// <summary>
    /// A value of AssignPrompt.
    /// </summary>
    [JsonPropertyName("assignPrompt")]
    public Common AssignPrompt
    {
      get => assignPrompt ??= new();
      set => assignPrompt = value;
    }

    /// <summary>
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public CsePersonsWorkSet Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of TbdExportAssign.
    /// </summary>
    [JsonPropertyName("tbdExportAssign")]
    public ServiceProvider TbdExportAssign
    {
      get => tbdExportAssign ??= new();
      set => tbdExportAssign = value;
    }

    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of LastUpdDate.
    /// </summary>
    [JsonPropertyName("lastUpdDate")]
    public DateWorkArea LastUpdDate
    {
      get => lastUpdDate ??= new();
      set => lastUpdDate = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonAccount Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of Object1.
    /// </summary>
    [JsonPropertyName("object1")]
    public SpTextWorkArea Object1
    {
      get => object1 ??= new();
      set => object1 = value;
    }

    /// <summary>
    /// A value of ZdelExportDesignatedPayee.
    /// </summary>
    [JsonPropertyName("zdelExportDesignatedPayee")]
    public CsePersonsWorkSet ZdelExportDesignatedPayee
    {
      get => zdelExportDesignatedPayee ??= new();
      set => zdelExportDesignatedPayee = value;
    }

    /// <summary>
    /// A value of AltAddPrompt.
    /// </summary>
    [JsonPropertyName("altAddPrompt")]
    public TextWorkArea AltAddPrompt
    {
      get => altAddPrompt ??= new();
      set => altAddPrompt = value;
    }

    /// <summary>
    /// A value of AlternateAddr.
    /// </summary>
    [JsonPropertyName("alternateAddr")]
    public CsePersonsWorkSet AlternateAddr
    {
      get => alternateAddr ??= new();
      set => alternateAddr = value;
    }

    /// <summary>
    /// A value of InterstateOblgInd.
    /// </summary>
    [JsonPropertyName("interstateOblgInd")]
    public Common InterstateOblgInd
    {
      get => interstateOblgInd ??= new();
      set => interstateOblgInd = value;
    }

    /// <summary>
    /// A value of SetupDate.
    /// </summary>
    [JsonPropertyName("setupDate")]
    public DateWorkArea SetupDate
    {
      get => setupDate ??= new();
      set => setupDate = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorPrompt.
    /// </summary>
    [JsonPropertyName("obligorPrompt")]
    public Common ObligorPrompt
    {
      get => obligorPrompt ??= new();
      set => obligorPrompt = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of PreviousObligationTransaction.
    /// </summary>
    [JsonPropertyName("previousObligationTransaction")]
    public ObligationTransaction PreviousObligationTransaction
    {
      get => previousObligationTransaction ??= new();
      set => previousObligationTransaction = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of PreviousDebtDetail.
    /// </summary>
    [JsonPropertyName("previousDebtDetail")]
    public DebtDetail PreviousDebtDetail
    {
      get => previousDebtDetail ??= new();
      set => previousDebtDetail = value;
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
    /// A value of ObligationTypePrompt.
    /// </summary>
    [JsonPropertyName("obligationTypePrompt")]
    public Common ObligationTypePrompt
    {
      get => obligationTypePrompt ??= new();
      set => obligationTypePrompt = value;
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
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of SuspendInterestInd.
    /// </summary>
    [JsonPropertyName("suspendInterestInd")]
    public Common SuspendInterestInd
    {
      get => suspendInterestInd ??= new();
      set => suspendInterestInd = value;
    }

    /// <summary>
    /// A value of ObligationAmt.
    /// </summary>
    [JsonPropertyName("obligationAmt")]
    public Common ObligationAmt
    {
      get => obligationAmt ??= new();
      set => obligationAmt = value;
    }

    /// <summary>
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
    }

    /// <summary>
    /// A value of HiddenObligor.
    /// </summary>
    [JsonPropertyName("hiddenObligor")]
    public CsePerson HiddenObligor
    {
      get => hiddenObligor ??= new();
      set => hiddenObligor = value;
    }

    /// <summary>
    /// A value of HiddenObligation.
    /// </summary>
    [JsonPropertyName("hiddenObligation")]
    public Obligation HiddenObligation
    {
      get => hiddenObligation ??= new();
      set => hiddenObligation = value;
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
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Common Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ConfirmObligAdd.
    /// </summary>
    [JsonPropertyName("confirmObligAdd")]
    public Common ConfirmObligAdd
    {
      get => confirmObligAdd ??= new();
      set => confirmObligAdd = value;
    }

    /// <summary>
    /// A value of ConfirmRetroDate.
    /// </summary>
    [JsonPropertyName("confirmRetroDate")]
    public Common ConfirmRetroDate
    {
      get => confirmRetroDate ??= new();
      set => confirmRetroDate = value;
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
    /// A value of DlgflwLinkFromOrec.
    /// </summary>
    [JsonPropertyName("dlgflwLinkFromOrec")]
    public Common DlgflwLinkFromOrec
    {
      get => dlgflwLinkFromOrec ??= new();
      set => dlgflwLinkFromOrec = value;
    }

    /// <summary>
    /// A value of ProtectAll.
    /// </summary>
    [JsonPropertyName("protectAll")]
    public Common ProtectAll
    {
      get => protectAll ??= new();
      set => protectAll = value;
    }

    /// <summary>
    /// A value of PassedDueDate.
    /// </summary>
    [JsonPropertyName("passedDueDate")]
    public DateWorkArea PassedDueDate
    {
      get => passedDueDate ??= new();
      set => passedDueDate = value;
    }

    /// <summary>
    /// A value of ObligorObligation.
    /// </summary>
    [JsonPropertyName("obligorObligation")]
    public Obligation ObligorObligation
    {
      get => obligorObligation ??= new();
      set => obligorObligation = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of TbdExportHiddenAssign.
    /// </summary>
    [JsonPropertyName("tbdExportHiddenAssign")]
    public ServiceProvider TbdExportHiddenAssign
    {
      get => tbdExportHiddenAssign ??= new();
      set => tbdExportHiddenAssign = value;
    }

    /// <summary>
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    /// <summary>
    /// A value of CollProtExistsInd.
    /// </summary>
    [JsonPropertyName("collProtExistsInd")]
    public Common CollProtExistsInd
    {
      get => collProtExistsInd ??= new();
      set => collProtExistsInd = value;
    }

    private Code code;
    private CodeValue country;
    private Common adjustmentExists;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private CsePersonsWorkSet hiddenAlternateAddr;
    private Common addSuccess;
    private Common legalIdPassed;
    private CsePersonsWorkSet zdelExportStateAsPayee;
    private Common assignPrompt;
    private CsePersonsWorkSet assign1;
    private ServiceProvider tbdExportAssign;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea lastUpdDate;
    private CsePersonAccount pass;
    private SpTextWorkArea object1;
    private CsePersonsWorkSet zdelExportDesignatedPayee;
    private TextWorkArea altAddPrompt;
    private CsePersonsWorkSet alternateAddr;
    private Common interstateOblgInd;
    private DateWorkArea setupDate;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private LegalActionDetail legalActionDetail;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction previousObligationTransaction;
    private DebtDetail debtDetail;
    private DebtDetail previousDebtDetail;
    private ObligationType obligationType;
    private Common obligationTypePrompt;
    private LegalAction legalAction;
    private Common manualDistributionInd;
    private Common paymentScheduleInd;
    private Common suspendInterestInd;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private CsePerson hiddenObligor;
    private Obligation hiddenObligation;
    private LegalAction hiddenLegalAction;
    private ObligationType hiddenObligationType;
    private Common last;
    private Common confirmObligAdd;
    private Common confirmRetroDate;
    private NextTranInfo hiddenNextTranInfo;
    private Common dlgflwLinkFromOrec;
    private Common protectAll;
    private DateWorkArea passedDueDate;
    private Obligation obligorObligation;
    private InterstateRequest interstateRequest;
    private InterstateRequest hiddenInterstateRequest;
    private ServiceProvider tbdExportHiddenAssign;
    private Common countryPrompt;
    private CsePersonsWorkSet alternate;
    private Common collProtExistsInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of UpdateBalanceDueAmt.
    /// </summary>
    [JsonPropertyName("updateBalanceDueAmt")]
    public Common UpdateBalanceDueAmt
    {
      get => updateBalanceDueAmt ??= new();
      set => updateBalanceDueAmt = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnTDebtAdjustment.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnTDebtAdjustment")]
    public ObligationTransaction HardcodeOtrnTDebtAdjustment
    {
      get => hardcodeOtrnTDebtAdjustment ??= new();
      set => hardcodeOtrnTDebtAdjustment = value;
    }

    /// <summary>
    /// A value of DebtExists.
    /// </summary>
    [JsonPropertyName("debtExists")]
    public Common DebtExists
    {
      get => debtExists ??= new();
      set => debtExists = value;
    }

    /// <summary>
    /// A value of HardcodedLapObligor.
    /// </summary>
    [JsonPropertyName("hardcodedLapObligor")]
    public LegalActionPerson HardcodedLapObligor
    {
      get => hardcodedLapObligor ??= new();
      set => hardcodedLapObligor = value;
    }

    /// <summary>
    /// A value of HardcodeOt718BUraJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeOt718BUraJudgement")]
    public ObligationType HardcodeOt718BUraJudgement
    {
      get => hardcodeOt718BUraJudgement ??= new();
      set => hardcodeOt718BUraJudgement = value;
    }

    /// <summary>
    /// A value of HardcodeCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hardcodeCpaSupportedPerson")]
    public CsePersonAccount HardcodeCpaSupportedPerson
    {
      get => hardcodeCpaSupportedPerson ??= new();
      set => hardcodeCpaSupportedPerson = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtVoluntary")]
    public ObligationTransaction HardcodeOtrnDtVoluntary
    {
      get => hardcodeOtrnDtVoluntary ??= new();
      set => hardcodeOtrnDtVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtAccrualInstruc.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtAccrualInstruc")]
    public ObligationTransaction HardcodeOtrnDtAccrualInstruc
    {
      get => hardcodeOtrnDtAccrualInstruc ??= new();
      set => hardcodeOtrnDtAccrualInstruc = value;
    }

    /// <summary>
    /// A value of HardcodeOtCAccruingClassific.
    /// </summary>
    [JsonPropertyName("hardcodeOtCAccruingClassific")]
    public ObligationType HardcodeOtCAccruingClassific
    {
      get => hardcodeOtCAccruingClassific ??= new();
      set => hardcodeOtCAccruingClassific = value;
    }

    /// <summary>
    /// A value of HardcodeOtCVoluntaryClassifi.
    /// </summary>
    [JsonPropertyName("hardcodeOtCVoluntaryClassifi")]
    public ObligationType HardcodeOtCVoluntaryClassifi
    {
      get => hardcodeOtCVoluntaryClassifi ??= new();
      set => hardcodeOtCVoluntaryClassifi = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDtDebtDetail.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtDebtDetail")]
    public ObligationTransaction HardcodeOtrnDtDebtDetail
    {
      get => hardcodeOtrnDtDebtDetail ??= new();
      set => hardcodeOtrnDtDebtDetail = value;
    }

    /// <summary>
    /// A value of HardcodeOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hardcodeOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HardcodeOtrrConcurrentObliga
    {
      get => hardcodeOtrrConcurrentObliga ??= new();
      set => hardcodeOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of EscapeFlag.
    /// </summary>
    [JsonPropertyName("escapeFlag")]
    public Common EscapeFlag
    {
      get => escapeFlag ??= new();
      set => escapeFlag = value;
    }

    /// <summary>
    /// A value of ForRefreshProgram.
    /// </summary>
    [JsonPropertyName("forRefreshProgram")]
    public CsePersonAccount ForRefreshProgram
    {
      get => forRefreshProgram ??= new();
      set => forRefreshProgram = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePerson Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of MatchCode.
    /// </summary>
    [JsonPropertyName("matchCode")]
    public Code MatchCode
    {
      get => matchCode ??= new();
      set => matchCode = value;
    }

    /// <summary>
    /// A value of MatchCodeValue.
    /// </summary>
    [JsonPropertyName("matchCodeValue")]
    public CodeValue MatchCodeValue
    {
      get => matchCodeValue ??= new();
      set => matchCodeValue = value;
    }

    /// <summary>
    /// A value of NoOfPromptsSelected.
    /// </summary>
    [JsonPropertyName("noOfPromptsSelected")]
    public Common NoOfPromptsSelected
    {
      get => noOfPromptsSelected ??= new();
      set => noOfPromptsSelected = value;
    }

    /// <summary>
    /// A value of PassedDueDate.
    /// </summary>
    [JsonPropertyName("passedDueDate")]
    public DateWorkArea PassedDueDate
    {
      get => passedDueDate ??= new();
      set => passedDueDate = value;
    }

    /// <summary>
    /// A value of ForLeftPad.
    /// </summary>
    [JsonPropertyName("forLeftPad")]
    public TextWorkArea ForLeftPad
    {
      get => forLeftPad ??= new();
      set => forLeftPad = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of HistoryIndicator.
    /// </summary>
    [JsonPropertyName("historyIndicator")]
    public Common HistoryIndicator
    {
      get => historyIndicator ??= new();
      set => historyIndicator = value;
    }

    /// <summary>
    /// A value of DeactiveObligationExists.
    /// </summary>
    [JsonPropertyName("deactiveObligationExists")]
    public Common DeactiveObligationExists
    {
      get => deactiveObligationExists ??= new();
      set => deactiveObligationExists = value;
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
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    /// <summary>
    /// A value of HardcodeRecovery.
    /// </summary>
    [JsonPropertyName("hardcodeRecovery")]
    public ObligationType HardcodeRecovery
    {
      get => hardcodeRecovery ??= new();
      set => hardcodeRecovery = value;
    }

    /// <summary>
    /// A value of HardcodeActive.
    /// </summary>
    [JsonPropertyName("hardcodeActive")]
    public DebtDetailStatusHistory HardcodeActive
    {
      get => hardcodeActive ??= new();
      set => hardcodeActive = value;
    }

    /// <summary>
    /// A value of HardcodeSpousalArrears.
    /// </summary>
    [JsonPropertyName("hardcodeSpousalArrears")]
    public ObligationType HardcodeSpousalArrears
    {
      get => hardcodeSpousalArrears ??= new();
      set => hardcodeSpousalArrears = value;
    }

    /// <summary>
    /// A value of HardcodeSecondary.
    /// </summary>
    [JsonPropertyName("hardcodeSecondary")]
    public Obligation HardcodeSecondary
    {
      get => hardcodeSecondary ??= new();
      set => hardcodeSecondary = value;
    }

    /// <summary>
    /// A value of HardcodePrimary.
    /// </summary>
    [JsonPropertyName("hardcodePrimary")]
    public Obligation HardcodePrimary
    {
      get => hardcodePrimary ??= new();
      set => hardcodePrimary = value;
    }

    /// <summary>
    /// A value of HardcodeFees.
    /// </summary>
    [JsonPropertyName("hardcodeFees")]
    public ObligationType HardcodeFees
    {
      get => hardcodeFees ??= new();
      set => hardcodeFees = value;
    }

    /// <summary>
    /// A value of HardcodeNonAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeNonAccruing")]
    public ObligationTransaction HardcodeNonAccruing
    {
      get => hardcodeNonAccruing ??= new();
      set => hardcodeNonAccruing = value;
    }

    /// <summary>
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    /// <summary>
    /// A value of Eab.
    /// </summary>
    [JsonPropertyName("eab")]
    public AbendData Eab
    {
      get => eab ??= new();
      set => eab = value;
    }

    /// <summary>
    /// A value of LegalEdit.
    /// </summary>
    [JsonPropertyName("legalEdit")]
    public Common LegalEdit
    {
      get => legalEdit ??= new();
      set => legalEdit = value;
    }

    /// <summary>
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of FromHistMonaNxttran.
    /// </summary>
    [JsonPropertyName("fromHistMonaNxttran")]
    public Common FromHistMonaNxttran
    {
      get => fromHistMonaNxttran ??= new();
      set => fromHistMonaNxttran = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of OblAssignmentOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oblAssignmentOfficeServiceProvider")]
    public OfficeServiceProvider OblAssignmentOfficeServiceProvider
    {
      get => oblAssignmentOfficeServiceProvider ??= new();
      set => oblAssignmentOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OblAssignmentServiceProvider.
    /// </summary>
    [JsonPropertyName("oblAssignmentServiceProvider")]
    public ServiceProvider OblAssignmentServiceProvider
    {
      get => oblAssignmentServiceProvider ??= new();
      set => oblAssignmentServiceProvider = value;
    }

    /// <summary>
    /// A value of OblAssignmentOffice.
    /// </summary>
    [JsonPropertyName("oblAssignmentOffice")]
    public Office OblAssignmentOffice
    {
      get => oblAssignmentOffice ??= new();
      set => oblAssignmentOffice = value;
    }

    /// <summary>
    /// A value of PassToCreate.
    /// </summary>
    [JsonPropertyName("passToCreate")]
    public ObligationAssignment PassToCreate
    {
      get => passToCreate ??= new();
      set => passToCreate = value;
    }

    /// <summary>
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
    }

    /// <summary>
    /// A value of BlankObligation.
    /// </summary>
    [JsonPropertyName("blankObligation")]
    public Obligation BlankObligation
    {
      get => blankObligation ??= new();
      set => blankObligation = value;
    }

    /// <summary>
    /// A value of CreateObligation.
    /// </summary>
    [JsonPropertyName("createObligation")]
    public Common CreateObligation
    {
      get => createObligation ??= new();
      set => createObligation = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      updateBalanceDueAmt = null;
      hardcodeOtrnTDebtAdjustment = null;
      debtExists = null;
      hardcodedLapObligor = null;
      escapeFlag = null;
      forRefreshProgram = null;
      pass = null;
      validCode = null;
      matchCode = null;
      matchCodeValue = null;
      noOfPromptsSelected = null;
      passedDueDate = null;
      forLeftPad = null;
      supported = null;
      deactiveObligationExists = null;
      obligation = null;
      eab = null;
      legalEdit = null;
      blankDateWorkArea = null;
      infrastructure = null;
      fromHistMonaNxttran = null;
      obligor = null;
      csePerson = null;
      interstateRequest = null;
      oblAssignmentOfficeServiceProvider = null;
      oblAssignmentServiceProvider = null;
      oblAssignmentOffice = null;
      passToCreate = null;
      cursorPosition = null;
      blankObligation = null;
      createObligation = null;
    }

    private Common updateBalanceDueAmt;
    private ObligationTransaction hardcodeOtrnTDebtAdjustment;
    private Common debtExists;
    private LegalActionPerson hardcodedLapObligor;
    private ObligationType hardcodeOt718BUraJudgement;
    private CsePersonAccount hardcodeCpaSupportedPerson;
    private ObligationTransaction hardcodeOtrnDtVoluntary;
    private ObligationTransaction hardcodeOtrnDtAccrualInstruc;
    private ObligationType hardcodeOtCAccruingClassific;
    private ObligationType hardcodeOtCVoluntaryClassifi;
    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private ObligationTransactionRlnRsn hardcodeOtrrConcurrentObliga;
    private Common escapeFlag;
    private CsePersonAccount forRefreshProgram;
    private CsePerson pass;
    private Common validCode;
    private Code matchCode;
    private CodeValue matchCodeValue;
    private Common noOfPromptsSelected;
    private DateWorkArea passedDueDate;
    private TextWorkArea forLeftPad;
    private CsePerson supported;
    private Common historyIndicator;
    private Common deactiveObligationExists;
    private Obligation obligation;
    private CsePersonAccount hardcodeObligor;
    private ObligationType hardcodeRecovery;
    private DebtDetailStatusHistory hardcodeActive;
    private ObligationType hardcodeSpousalArrears;
    private Obligation hardcodeSecondary;
    private Obligation hardcodePrimary;
    private ObligationType hardcodeFees;
    private ObligationTransaction hardcodeNonAccruing;
    private ObligationTransaction hardcodeDebt;
    private AbendData eab;
    private Common legalEdit;
    private DateWorkArea blankDateWorkArea;
    private DateWorkArea max;
    private Infrastructure infrastructure;
    private Common fromHistMonaNxttran;
    private DateWorkArea current;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private OfficeServiceProvider oblAssignmentOfficeServiceProvider;
    private ServiceProvider oblAssignmentServiceProvider;
    private Office oblAssignmentOffice;
    private ObligationAssignment passToCreate;
    private CursorPosition cursorPosition;
    private Obligation blankObligation;
    private Common createObligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligationAssignment.
    /// </summary>
    [JsonPropertyName("obligationAssignment")]
    public ObligationAssignment ObligationAssignment
    {
      get => obligationAssignment ??= new();
      set => obligationAssignment = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public LegalActionPerson Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public OfficeServiceProvider Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of CpObligor1.
    /// </summary>
    [JsonPropertyName("cpObligor1")]
    public CsePerson CpObligor1
    {
      get => cpObligor1 ??= new();
      set => cpObligor1 = value;
    }

    /// <summary>
    /// A value of CpObligor2.
    /// </summary>
    [JsonPropertyName("cpObligor2")]
    public CsePersonAccount CpObligor2
    {
      get => cpObligor2 ??= new();
      set => cpObligor2 = value;
    }

    /// <summary>
    /// A value of CpObligationType.
    /// </summary>
    [JsonPropertyName("cpObligationType")]
    public ObligationType CpObligationType
    {
      get => cpObligationType ??= new();
      set => cpObligationType = value;
    }

    /// <summary>
    /// A value of CpObligation.
    /// </summary>
    [JsonPropertyName("cpObligation")]
    public Obligation CpObligation
    {
      get => cpObligation ??= new();
      set => cpObligation = value;
    }

    /// <summary>
    /// A value of CpObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("cpObligCollProtectionHist")]
    public ObligCollProtectionHist CpObligCollProtectionHist
    {
      get => cpObligCollProtectionHist ??= new();
      set => cpObligCollProtectionHist = value;
    }

    private DebtDetail debtDetail;
    private ObligationAssignment obligationAssignment;
    private CsePerson csePerson;
    private CsePerson obligor;
    private CsePersonAccount csePersonAccount;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson zdel;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider existing;
    private Office office;
    private Case1 case1;
    private CaseUnit caseUnit;
    private CsePerson cpObligor1;
    private CsePersonAccount cpObligor2;
    private ObligationType cpObligationType;
    private Obligation cpObligation;
    private ObligCollProtectionHist cpObligCollProtectionHist;
  }
#endregion
}

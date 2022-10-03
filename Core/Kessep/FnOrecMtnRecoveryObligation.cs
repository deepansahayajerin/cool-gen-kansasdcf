// Program: FN_OREC_MTN_RECOVERY_OBLIGATION, ID: 372163761, model: 746.
// Short name: SWEORECP
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
/// A program: FN_OREC_MTN_RECOVERY_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This Procedure maintains Recovery and Fee type obligations.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOrecMtnRecoveryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OREC_MTN_RECOVERY_OBLIGATION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOrecMtnRecoveryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOrecMtnRecoveryObligation.
  /// </summary>
  public FnOrecMtnRecoveryObligation(IContext context, Import import,
    Export export):
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
    // #################################################
    // ##
    // ##  When you get the chance, delete the Dialog Flow link
    // ##  FROM  OREC     TO  LDET
    // ##   <<<<  NOT  FROM  LDET     TO  OREC  >>>>
    // ##
    // #################################################
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
    // 07/28/97     Paul R. Egger             Changed the way the Interstate 
    // Obligation displayed to I or K, don't allow FEE to be valid on this
    // screen, fixed flow to LDET to pass some key info, on flow from LDET --
    // made sure obligor name was looked up, added Other State and make sure it
    // contains a valid date, passed due date to INMS as the accrual/due date.
    // 11/17/1997    Skip Hardy MTW		Added link to the CADS screen along with 
    // the PF Key.  Made changes to data past from OPAY.
    // 01/21/1998	AJS - MTW		Display address on create
    // 					flow from CREL
    // 01/22/1998	AJS - MTW		Create obl assignment on
    // 					add
    // 02/27/1998	AJS - MTW		PR#39026 - Interstate
    // 					recovery corrections
    // ----------------------------------------------------------------
    // **************
    //  8/26/98     Bud Adams  Added attributes to E/A views Obligation
    // 			and to Obligation_Type e/a view so
    // 			they would match properly to persist view.
    // **************
    // ================================================
    // 9-3-98  B Adams	When multiple recovery oblgs exist,
    //   don't force user to press PFkey to flow to OPAY; just do it.
    // 9-15-98  B Adams   Changed SET statement which had set
    //   Obl-Pymt-Sch DOM2 to Leg-Act-Dtl DOM1
    // 9-25-98   B Adams    Deleted Legal Action Prompt for Court Order.
    // 10-2-98   B Adams    Deleted FN_Hardcode_Legal; no attributes
    //   from it were exported or used.
    // 12/17/98  B Adams   changed logic to allow user to remove
    //   an alternate address; avoid database actions when it is
    //   unchanged
    // 12/28/98 - B Adams   The only address that the screen is
    //   supposed to display is for the Obligor.
    // 1/6/99 - B Adams   If there is no CSE_Person_Address for the
    //   Obligor then the address fields will be protected.  It cannot
    //   be added on this screen.
    // 3/23/1999 - b adams  -  READ properties set.
    // 6/9/99 - b adams  -  Changed the date displayed as Retired
    //   Date from Debt_Detail Retired_Date to Debt_Detail_Status
    //   History Effective_Date, and changed the label to Deactive
    //   Date.
    // 11/4/99 - b adams  -  PR# 78500: see notes below
    // 11/12/1999 - M Ramirez  -  PR# xxxxx Change POSTMAST
    //   document from batch print to online print
    // 1/21/00 - b adams  -  PR# 82849: Removing interstate fields
    //   after an ADD was not working.  See note below.
    // =================================================
    // =================================================
    // 3/1/00  K. Price - process country or state code for ADD
    // and UPDATE
    // 06/21/2000 V.Madhira PR# 85900  Fixed the code for Interstate  field 
    // edits.
    // =================================================
    // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
    // =================================================
    // Oct, 2000 M. Brown  PR# 106234 Updated NEXT TRAN.
    // =================================================
    // = Return from Link to:		COMMAND:
    // =
    // =	OPSC			      DISPLAY
    // =	CADS			      DISPLAY
    // =	CSPM			      RTFRMLNK
    // =	INMS			      RTFRMLNK
    // =	MDIS			      RTFRMLNK
    // =	OPAY			      RETOPAY
    // =	OCTO			      RTFRMLNK
    // =	OBTL			      RTFRMLNK
    // =
    // =
    // = Return from Prompt to:	COMMAND:
    // =
    // =	NAME (obligor)		RETNAME
    // =	NAME (alt addr)		RETLINK
    // =	ASIN			DISPLAY
    // =	COUNTRY		        DISPLAY
    // =
    // = Dialog Flow from:		COMMAND:
    // =
    // =	DEBT			FROMDEBT
    // =	OCTO			FROMOCTO
    // =================================================
    // **************************************************************
    //                PR to add edit checks for zip code .
    // PR# 114679  and PR# 116889.
    //                                         
    // Madhu Kumar
    // **************************************************************
    // -- 2008/03/17  GVandy CQ296  Adjust Disbursement_Summary non_taf_amount 
    // when recovery obligations are created for non-TAF cases.
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.AddressFound.Flag = "N";

    // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
    export.DisplayAddress.Flag = import.DisplayAddress.Flag;

    if (AsChar(export.DisplayAddress.Flag) == 'Y' || AsChar
      (export.DisplayAddress.Flag) == 'N')
    {
    }
    else
    {
      local.SaveCommand.Command = global.Command;
      global.Command = "RESEARCH";
      UseScCabTestSecurity2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // USER is Authorized to view Address
        export.DisplayAddress.Flag = "Y";
      }
      else
      {
        // USER is NOT Authorized to view Address
        export.DisplayAddress.Flag = "N";
      }

      ExitState = "ACO_NN0000_ALL_OK";
      global.Command = local.SaveCommand.Command;
    }

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "XXFMMENU"))
    {
      // =================================================
      // 10/30/98 - B  Adams  -  Non-Court Ordered obligations can be
      //   interstate, and the Finance User must be able to enter the
      //   appropriate information for those instances.
      // =================================================
      // 1/7/99 - B Adams  -  Addresses can only be updated, never
      //   added from this screen.  So, from a blank screen, these
      //   fields will be protected.
      // =================================================
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

      var field4 = GetField(export.InterstateRequest, "country");

      field4.Color = "green";
      field4.Highlighting = Highlighting.Underscore;
      field4.Protected = false;

      var field5 = GetField(export.CsePersonAddress, "type1");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.CsePersonAddress, "locationType");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.CsePersonAddress, "street1");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.CsePersonAddress, "street2");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.CsePersonAddress, "street3");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.CsePersonAddress, "street4");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.CsePersonAddress, "city");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.CsePersonAddress, "state");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 = GetField(export.CsePersonAddress, "zipCode");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 = GetField(export.CsePersonAddress, "zip3");

      field14.Color = "cyan";
      field14.Protected = true;

      var field15 = GetField(export.CsePersonAddress, "zip4");

      field15.Color = "cyan";
      field15.Protected = true;

      var field16 = GetField(export.CsePersonAddress, "province");

      field16.Color = "cyan";
      field16.Protected = true;

      var field17 = GetField(export.CsePersonAddress, "postalCode");

      field17.Color = "cyan";
      field17.Protected = true;

      var field18 = GetField(export.CsePersonAddress, "country");

      field18.Color = "cyan";
      field18.Protected = true;

      return;
    }

    // ***** HARDCODE AREA *****
    UseFnHardcodedDebtDistribution();
    UseFnHardcodeLegal();
    UseSpDocSetLiterals();

    // =================================================
    // 1/2/99 - B Adams  -  Flow from OCTO brings these values but
    //   there was no provision for the use of Obligation or Legal_
    //   Action since all references are made to EXPORT views.
    //   Changed the Command from DISPLAY to FROMOCTO.
    // 6/19/99 - B Adams  -  The same thing when flowing from
    //   DEBT; Command is now FROMDEBT.
    // 12/20/99 - B Adams  -  PR# 82602: Same when flowing from
    //   OREL to display an existing debt.  Command used to be
    //   DISPLAY, but there was no way for OREC to know the flow
    //   was coming from OREL.  (legal action view is irrelevant to
    //   OREL.)
    // =================================================
    if (Equal(global.Command, "FROMOCTO") || Equal
      (global.Command, "FROMDEBT") || Equal(global.Command, "RDISPLAY"))
    {
      MoveObligation5(import.Obligation, export.Obligation);
      export.LegalAction.Assign(import.LegalAction);
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETOPAY"))
    {
      MoveObligation5(import.Obligation, export.Obligation);
      export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
      global.Command = "DISPLAY";
    }

    // ================================================
    // 10/9/98 - B Adams  -  return from link to OPSC, CADS, and
    // ASIN define Command as DISPLAY - so none of the following
    // MOVEs will execute.  Until I can get the dialog managers of
    // those procedure steps to change that, this work view will be
    // used to keep track of the return from link.
    // ================================================
    // ***** MOVE IMPORT'S TO EXPORT'S *****
    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    export.FlowFrom.Text4 = import.FlowFromWorkArea.Text4;
    MoveObligationType2(import.ObligationType, export.ObligationType);
    export.MustClearFirst.Flag = import.MustClearFirst.Flag;
    export.LegalAction.Assign(import.LegalAction);
    MoveLegalActionDetail1(import.LegalActionDetail, export.LegalActionDetail);
    MoveObligation5(import.Obligation, export.Obligation);
    export.ObligorCsePersonsWorkSet.Assign(import.ObligorCsePersonsWorkSet);
    export.CountryPrompt.SelectChar = import.CountryPrompt.SelectChar;
    export.Country.Description = import.Country.Description;
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.ObCollProtAct.Flag = import.ObCollProtAct.Flag;

    if (IsEmpty(export.InterstateRequest.Country) && !
      IsEmpty(export.Country.Cdvalue))
    {
      export.InterstateRequest.Country =
        Substring(export.Country.Cdvalue, 1, 2);
    }

    if (IsEmpty(export.ObCollProtAct.Flag))
    {
      export.ObCollProtAct.Flag = "N";
    }

    export.ObligorPrompt.SelectChar = import.ObligorPrompt.SelectChar;
    export.AssignPrompt.SelectChar = import.AssignPrompt.SelectChar;
    export.AltAddPrompt.Text1 = import.AltAddPrompt.Text1;
    export.CountryPrompt.SelectChar = import.CountryPrompt.SelectChar;
    export.ObligationTypePrompt.SelectChar =
      import.ObligationTypePrompt.SelectChar;

    // =================================================
    // 1/4/99 - B Adams  -  If this is the same Obligor as a previous
    //   function, that means the obligation is still the same and so
    //   we DO know which one it is; no need to flow back to OPAY.
    //   Unless, they did and CLEAR and re-entered the Obligor.
    // =================================================
    if (Equal(global.Command, "DISPLAY") && Equal
      (import.ObligorCsePerson.Number, import.HiddenObligor.Number) && import
      .Obligation.SystemGeneratedIdentifier != 0)
    {
      MoveObligation5(import.Obligation, export.Obligation);
    }

    if (!IsEmpty(export.ObligorCsePerson.Number) && (
      !IsEmpty(export.LegalAction.StandardNumber) || export
      .Obligation.SystemGeneratedIdentifier > 0))
    {
      // ----------------------------------------------------------------
      // Do not let the user to update the AP#,  associated to a legal action, 
      // on OREC screen or if the obligation is processed.
      // ---------------------------------------------------------------
      var field1 = GetField(export.ObligorCsePerson, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligorPrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.FlowFromCsePersonsWorkSet.Number))
      {
        export.ObligorCsePersonsWorkSet.
          Assign(import.FlowFromCsePersonsWorkSet);
        export.ObligorCsePersonsWorkSet.
          Assign(import.FlowFromCsePersonsWorkSet);
        export.ObligorCsePerson.Number = export.ObligorCsePersonsWorkSet.Number;

        // =================================================
        // 6/9/99 - bud adams  -  If a new obligor number is returned
        //   from NAME, change COMMAND to DISPLAY.  This will make
        //   all the MOVE statements below to be avoided and for the
        //   DISPLAY function to process.
        // =================================================
        if (!Equal(import.FlowFromCsePersonsWorkSet.Number,
          import.ObligorCsePerson.Number))
        {
          export.ObligationType.Code = "";
          export.ObligationType.SystemGeneratedIdentifier = 0;
          global.Command = "DISPLAY";
        }
      }
    }

    if (!Equal(global.Command, "DISPLAY") || Equal
      (export.FlowFrom.Text4, "OPSC") || Equal
      (export.FlowFrom.Text4, "CADS") || Equal(export.FlowFrom.Text4, "ASIN"))
    {
      if (!Equal(export.FlowFrom.Text4, "OREL"))
      {
        export.FlowFrom.Text4 = "";
      }

      MoveCsePersonsWorkSet2(import.AlternateAddr, export.AlternateAddr);
      export.HiddenObligor.Number = import.HiddenObligor.Number;
      export.HiddenObligation.Assign(import.HiddenObligation);
      export.HiddenObligationType.Code = import.HiddenObligationType.Code;
      MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);
      export.HiddenObligationTransaction.Amount =
        import.HiddenObligationTransaction.Amount;
      MovePaymentRequest(import.H, export.HpaymentRequest);
      export.AltAddPrompt.Text1 = import.AltAddPrompt.Text1;
      export.CpaObligorOrObligee.Type1 = import.CpaObligorOrObligee.Type1;
      export.Standard.NextTransaction = import.Standard.NextTransaction;
      MoveObligationTransaction1(import.ObligationTransaction,
        export.ObligationTransaction);
      export.DebtDetailStatusHistory.EffectiveDt =
        import.DebtDetailStatusHistory.EffectiveDt;
      export.AdjustmentExists.Flag = import.AdjustmentExists.Flag;
      export.CsePersonAddress.Assign(import.CsePersonAddress);
      export.HiddenCsePersonAddress.Assign(import.HiddenCsePersonAddress);
      MoveObligation9(import.Obligation, local.Obligation);
      export.BalanceOwed.TotalCurrency = import.BalanceOwed.TotalCurrency;
      export.DebtDetail.Assign(import.DebtDetail);
      export.InterestOwed.TotalCurrency = import.InterestOwed.TotalCurrency;
      export.ObligationAmt.TotalCurrency = import.ObligationAmt.TotalCurrency;
      export.PaymentScheduleInd.Flag = import.PaymentScheduleInd.Flag;
      export.SuspendInterestInd.Flag = import.SuspendInterestInd.Flag;
      export.TotalOwed.TotalCurrency = import.TotalOwed.TotalCurrency;
      export.Previous.DueDt = import.Previous.DueDt;
      export.ManualDistributionInd.Flag = import.ManualDistributionInd.Flag;
      export.Last.Command = import.Last.Command;
      export.ConfirmObligAdd.Flag = import.ConfirmObligAdd.Flag;
      export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
      export.SetupDate.Date = import.SetupDate.Date;
      export.LastUpdDate.Date = import.LastUpdDate.Date;
      export.FrequencyWorkSet.FrequencyDescription =
        import.FrequencyWorkSet.FrequencyDescription;
      export.AssignServiceProvider.UserId = import.AssignServiceProvider.UserId;
      export.AssignCsePersonsWorkSet.FormattedName =
        import.AssignCsePersonsWorkSet.FormattedName;
      export.HiddenInterstateRequest.Assign(import.HiddenInterstateRequest);
      export.HiddenAssign.UserId = import.HiddenAssign.UserId;
      export.HiddenAlternateAddr.Number = import.HiddenAlternateAddr.Number;

      // =================================================
      // 1/21/00 - b adams  -  PR# 83294: generated ID from prior
      //   display was causing problems when the other-state-case-id
      //   was blanked out because it still had a value in it.
      // =================================================
      if (IsEmpty(export.InterstateRequest.OtherStateCaseId) && IsEmpty
        (export.InterstateRequest.Country))
      {
        export.InterstateRequest.IntHGeneratedId = 0;
      }

      if (IsEmpty(export.SuspendInterestInd.Flag))
      {
        export.SuspendInterestInd.Flag = "Y";
      }

      if (IsEmpty(export.Obligation.PrimarySecondaryCode))
      {
        export.Obligation.PrimarySecondaryCode =
          local.HardcodeObPrimSecCodPrimary.PrimarySecondaryCode ?? "";
      }
    }

    // =================================================
    // 6/24/99 - B Adams  -  Return from ASIN is with Command of
    //   Display!  This view is only valued when linking to ASIN.
    // =================================================
    if (Equal(export.FlowFrom.Text4, "ASIN"))
    {
      global.Command = "BYPASS";
    }

    if (Equal(global.Command, "RETNAME"))
    {
      export.ObligorCsePersonsWorkSet.Assign(import.FlowFromCsePersonsWorkSet);
      export.ObligorCsePerson.Number = export.ObligorCsePersonsWorkSet.Number;
    }

    // =================================================
    // 3/12/1999 - bud adams  -  Recoveries can have either the AR
    //   or AP as the obligor.  Defaulting to AP.
    // =================================================
    if (IsEmpty(export.CpaObligorOrObligee.Type1))
    {
      export.CpaObligorOrObligee.Type1 = local.HardcodeObligor.Type1;
    }

    // =================================================
    // 11/3/98 - B Adams  -  This data comes via dialog flow from RCAP
    //   and wasn't ever being MOVED to export views.
    // =================================================
    if (Equal(global.Command, "RCAP"))
    {
      MoveObligation5(import.Obligation, export.Obligation);
      export.LegalAction.Assign(import.LegalAction);
      export.ObligorCsePersonsWorkSet.Assign(import.FlowFromCsePersonsWorkSet);
      global.Command = "DISPLAY";
    }

    if (Equal(import.FlowFromWorkArea.Text4, "ASIN") && AsChar
      (import.AssignPrompt.SelectChar) == 'S')
    {
      export.AssignPrompt.SelectChar = "+";

      var field = GetField(export.AssignServiceProvider, "userId");

      field.Protected = false;
      field.Focused = true;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      if (!IsEmpty(import.FlowFromCsePersonsWorkSet.Number))
      {
        MoveCsePersonsWorkSet1(import.FlowFromCsePersonsWorkSet,
          export.AlternateAddr);
      }

      if (AsChar(import.AltAddPrompt.Text1) == 'S')
      {
        export.AltAddPrompt.Text1 = "+";

        var field = GetField(export.AlternateAddr, "number");

        field.Protected = false;
        field.Focused = true;
      }
    }

    if (Equal(global.Command, "RETLINK") || Equal(global.Command, "RETNAME"))
    {
      // ================================================
      // 10/6/98  Bud Adams  -  After a Display action, a user can only
      // update Alternate_Billing_Location, Address data, or prompt for a new 
      // Assigned To person.  All other fields should be protected - except for
      // Obligor Number, since the user may wish to display recovery debts for
      // another person.
      // ================================================
      if (Equal(export.SetupDate.Date, local.Current.Date) || Equal
        (export.SetupDate.Date, local.Blank.Date))
      {
      }
      else
      {
        var field1 = GetField(export.ObligationType, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.ObligationTypePrompt, "selectChar");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.ObligationTransaction, "amount");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.DebtDetail, "dueDt");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Obligation, "orderTypeCode");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Obligation, "otherStateAbbr");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.InterstateRequest, "otherStateCaseId");

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 = GetField(export.InterstateRequest, "country");

        field8.Color = "cyan";
        field8.Protected = true;
      }

      if (!IsEmpty(export.ObligorCsePerson.Number) || !
        IsEmpty(export.ObligorCsePersonsWorkSet.Number))
      {
        UseFnCabReadCsePersonAddress();
        export.CsePersonAddress.Assign(local.CsePersonAddress);
        export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          MoveCsePersonAddress2(export.CsePersonAddress,
            local.DisplayAddressSave);
          export.CsePersonAddress.Assign(local.Initialized);
          MoveCsePersonAddress2(local.DisplayAddressSave,
            export.CsePersonAddress);
          export.CsePersonAddress.Street1 = "Security Block on Address";
        }
      }
      else
      {
        local.AddressFound.Flag = "N";
      }

      global.Command = "BYPASS";
    }

    if (Equal(global.Command, "ADD") && AsChar(import.LastWasAdd.Flag) == 'Y')
    {
      global.Command = "BYPASS";
      export.LastWasAdd.Flag = import.LastWasAdd.Flag;
      ExitState = "FN0000_OBLIGATION_AE";
    }

    if (Equal(export.SetupDate.Date, local.Blank.Date))
    {
      local.AddressFound.Flag = "N";
    }
    else if (!Equal(export.SetupDate.Date, local.Current.Date))
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
    }

    // *** If it is a RETURN from MDIS, INMS, CSPM, OPSC, OPAY, OCTO or FIPL 
    // screens, then display the screen.   If command = addorel, display
    // address.
    // FIPL is gone from this list.  Also, ADDOREL is not a valid command in 
    // OREC.
    if (Equal(global.Command, "RTFRMLNK") || Equal(global.Command, "ADDOREL"))
    {
      // ************************************************************
      // Get the address of the obligor CSE Person.
      // ************************************************************
      UseFnCabReadCsePersonAddress();

      if (!Equal(local.CsePersonAddress.Identifier, local.Blank.Timestamp))
      {
        // *************************************************************
        // The address was found for the Obligor CSE Person.
        // *************************************************************
        export.CsePersonAddress.Assign(local.CsePersonAddress);
        export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          MoveCsePersonAddress2(export.CsePersonAddress,
            local.DisplayAddressSave);
          export.CsePersonAddress.Assign(local.Initialized);
          MoveCsePersonAddress2(local.DisplayAddressSave,
            export.CsePersonAddress);
          export.CsePersonAddress.Street1 = "Security Block on Address";
        }

        local.AddressFound.Flag = "Y";
      }

      if (Equal(global.Command, "ADDOREL"))
      {
        // ************************************************************
        // Get the address of the obligor CSE Person.
        // ************************************************************
        UseFnCabReadCsePersonAddress();

        if (!Equal(local.CsePersonAddress.Identifier, local.Blank.Timestamp))
        {
          // *************************************************************
          // The address was found for the Obligor CSE Person.
          // *************************************************************
          export.CsePersonAddress.Assign(local.CsePersonAddress);
          export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

          // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
          }
          else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
            IsEmpty(export.CsePersonAddress.Street1) || !
            IsEmpty(export.CsePersonAddress.Street3) || !
            IsEmpty(export.CsePersonAddress.State) || !
            IsEmpty(export.CsePersonAddress.Country))
          {
            MoveCsePersonAddress2(export.CsePersonAddress,
              local.DisplayAddressSave);
            export.CsePersonAddress.Assign(local.Initialized);
            MoveCsePersonAddress2(local.DisplayAddressSave,
              export.CsePersonAddress);
            export.CsePersonAddress.Street1 = "Security Block on Address";
          }

          local.AddressFound.Flag = "Y";
        }
        else
        {
        }

        // =================================================
        // 3/2/1999 - B Adams  -  Flowing from OREL is an amount from
        //   Payment_Request that is a negative amount which was put
        //   there by B652, the passthrough process.  Those negative
        //   values may be OK for them, but we don't want -$ amounts
        //   in Obligation_Transaction and Debt_Detail.
        //   It seems as if OREL is multiplying those values by -1 twice.
        //   But, regardless, we never want a negative value here.
        // =================================================
        if (import.ObligationTransaction.Amount < 0)
        {
          export.ObligationTransaction.Amount =
            -import.ObligationTransaction.Amount;
        }

        // =================================================
        // PR# 231: 9/20/99 - bud adams  -  When flowing from OREL,
        //   the address data must be protected and the Obligation Type
        //   field must be blank.
        // =================================================
        local.AddressFound.Flag = "N";
        export.ObligationType.Code = "";
        export.FlowFrom.Text4 = "OREL";
      }

      if (AsChar(import.AltAddPrompt.Text1) == 'S')
      {
        export.AltAddPrompt.Text1 = "+";

        var field = GetField(export.AlternateAddr, "number");

        field.Protected = false;
        field.Focused = true;
      }

      if (AsChar(import.ObligationTypePrompt.SelectChar) == 'S')
      {
        var field = GetField(export.ObligationType, "code");

        field.Protected = false;
        field.Focused = true;

        export.ObligationTypePrompt.SelectChar = "+";
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

      global.Command = "BYPASS";
    }

    // *** SECURITY
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------------------
      // User entered this screen from another screen
      // ---------------------------------------------
      UseScCabNextTranGet();

      // ---------------------------------------------
      // Populate views from local next_tran_info view read from the data base
      // Set command to initial command required or ESCAPE
      // ---------------------------------------------
      // Oct, 2000 M. Brown  PR# 106234 - Populating more attributes from next 
      // tran.
      export.ObligorCsePerson.Number =
        export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
      export.LegalAction.Identifier =
        export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();
      export.LegalAction.CourtCaseNumber =
        export.HiddenNextTranInfo.CourtCaseNumber ?? "";

      // =====================================
      // SRPT is the HIST screen.
      // SRPU is the MONA screen.
      // =====================================
      if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
        (export.HiddenNextTranInfo.LastTran, "SRPU"))
      {
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

      global.Command = "DISPLAY";
    }

Test1:

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------------------
      // User is going out of this screen to another
      // ---------------------------------------------
      // ---------------------------------------------
      // Set up local next_tran_info for saving the current values for the next 
      // screen
      // ---------------------------------------------
      // Oct, 2000 M. Brown  PR# 106234 - Populating more attributes from next 
      // tran.
      //  Update next tran if key info was changed.
      if (!Equal(export.HiddenObligor.Number,
        export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.HiddenNextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        export.HiddenNextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        export.HiddenNextTranInfo.StandardCrtOrdNumber =
          export.LegalAction.StandardNumber ?? "";
        export.HiddenNextTranInfo.MiscNum1 = export.LegalActionDetail.Number;
        export.HiddenNextTranInfo.CsePersonNumberObligor =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        export.HiddenNextTranInfo.MiscNum2 =
          export.ObligationType.SystemGeneratedIdentifier;
      }

      UseScCabNextTranPut1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        if (!IsEmpty(import.Obligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (!IsEmpty(import.InterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (!IsEmpty(import.InterstateRequest.Country))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(import.Obligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "otherStateAbbr");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "country");

          field4.Color = "cyan";
          field4.Protected = true;
        }

        global.Command = "BYPASS";
      }
    }

    // ================================================
    // 10/9/98 - B Adams  -  moved this IF from below so those
    // users would be subject to security checks.
    // ================================================
    if (Equal(global.Command, "FROMOPAY") || Equal(global.Command, "FROMOCTO"))
    {
      global.Command = "DISPLAY";
    }

    // =================================================
    // 10/8/98  B Adams  -  changed this security test from being a
    // long negative list to a short positive list.
    // =================================================
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ***** EDIT AREA *****
    if (!Equal(global.Command, "CONFIRM"))
    {
      export.Last.Command = "";
    }

    // : If the Obligation System Generated ID is zero, Print, Update and Delete
    // functions are invalid.
    if (export.Obligation.SystemGeneratedIdentifier == 0 || import
      .HiddenObligation.SystemGeneratedIdentifier == 0)
    {
      if (Equal(global.Command, "UPDATE"))
      {
        if (Equal(export.FlowFrom.Text4, "OREL"))
        {
          local.AddressFound.Flag = "N";
          global.Command = "BYPASS";
          ExitState = "FN0000_MUST_ADD_FROM_OREL";
        }
        else
        {
          global.Command = "BYPASS";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }
      }
      else if (Equal(global.Command, "DELETE"))
      {
        if (Equal(export.FlowFrom.Text4, "OREL"))
        {
          local.AddressFound.Flag = "N";
          global.Command = "BYPASS";
          ExitState = "FN0000_MUST_ADD_FROM_OREL";
        }
        else
        {
          global.Command = "BYPASS";
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
        }
      }
    }

    if (!IsEmpty(export.ObligorCsePerson.Number))
    {
      local.ForLeftPad.Text10 = export.ObligorCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.ObligorCsePerson.Number = local.ForLeftPad.Text10;
    }

    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      if (!IsEmpty(export.AlternateAddr.Number))
      {
        local.ForLeftPad.Text10 = export.AlternateAddr.Number;
        UseEabPadLeftWithZeros();

        // =================================================
        // 12/31/98 - B Adams  -  If alternate billing address person #
        //   does not have an address, that's not acceptable.
        // =================================================
        export.AlternateAddr.Number = local.ForLeftPad.Text10;
        UseFnCabCheckAltAddr();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field1 = GetField(export.AlternateAddr, "number");

          field1.Error = true;

          var field2 = GetField(export.AltAddPrompt, "text1");

          field2.Protected = false;
          field2.Focused = true;

          global.Command = "BYPASS";
        }
      }
    }

    // *************************************************************************
    // *** If Flows from Legal screen with the Legal Identifiers , Command will 
    // be "FROMLDET" ***
    // *************************************************************************
    if (Equal(global.Command, "FROMLDET"))
    {
      local.LegalIdPassed.Flag = "N";

      if (export.LegalAction.Identifier == 0 || export
        .LegalActionDetail.Number == 0)
      {
        // ** Nothing has been selected from the Legal Action Detail ; So user 
        // input is expected
        var field = GetField(export.LegalAction, "standardNumber");

        field.Color = "cyan";
        field.Protected = true;
      }
      else
      {
        // ** Legal Identifiers were passed....
        local.LegalIdPassed.Flag = "Y";
        UseFnRetrieveLeglForRecAndFee();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test2;
        }

        export.BalanceOwed.TotalCurrency = export.ObligationTransaction.Amount;
        export.InterestOwed.TotalCurrency = 0;
        export.TotalOwed.TotalCurrency =
          export.LegalActionDetail.JudgementAmount.GetValueOrDefault();
        UseFnCabReadCsePersonAddress();
        export.CsePersonAddress.Assign(local.CsePersonAddress);
        export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          MoveCsePersonAddress2(export.CsePersonAddress,
            local.DisplayAddressSave);
          export.CsePersonAddress.Assign(local.Initialized);
          MoveCsePersonAddress2(local.DisplayAddressSave,
            export.CsePersonAddress);
          export.CsePersonAddress.Street1 = "Security Block on Address";
        }
      }

      // =================================================
      // 5/24/99 - Bud Adams  -  When coming from LDET and an
      //   obligation exists, display that obligation information.
      // =================================================
      if (AsChar(local.LegalIdPassed.Flag) == 'Y' && !
        Equal(global.Command, "RETURN"))
      {
        local.LegalIdPassed.Flag = "";

        if (ReadObligationTransactionObligationDebtDetail())
        {
          MoveObligation3(entities.Obligation, export.Obligation);

          if (Equal(entities.DebtDetail.RetiredDt, local.Blank.Date))
          {
            global.Command = "DISPLAY";
          }
          else
          {
            export.Obligation.OrderTypeCode = "K";
            export.InterstateRequest.Country = "";
            export.Obligation.OtherStateAbbr = "";
            export.Obligation.Description = "";
            export.Obligation.HistoryInd = "";
            export.Obligation.PrimarySecondaryCode = "";
            export.Obligation.CreatedBy = "";
            export.Obligation.LastUpdatedBy = "";
            export.Obligation.SystemGeneratedIdentifier = 0;
            export.SuspendInterestInd.Flag = "";
            ExitState = "FN0000_DEACTIVE_OB_CONFIRM_TO_AD";
            global.Command = "BYPASS";

            goto Test2;
          }

          goto Test2;
        }

        ExitState = "FN0000_OBLIG_NOT_YET_CREATED";
        global.Command = "BYPASS";
        export.Obligation.OrderTypeCode = "K";
        export.InterstateRequest.Country = "";
        export.Obligation.OtherStateAbbr = "";
        export.InterstateRequest.OtherStateCaseId = "";
      }
      else
      {
        local.AddressFound.Flag = "N";
        global.Command = "BYPASS";
      }

      // ***
      // The objective is to DISPLAY all of the information compiled/passedover 
      // and wait for a User to initiate an action for subsequent processing
    }

Test2:

    // *** The control will come to this position if the user inputs the keys
    //     If one or more key fields are blank, certain commands are not 
    // allowed.
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "DELETE"))
    {
      // =================================================
      // 12/31/98  -  B Adams  -  All required field tests should be
      //   done at the same time.
      // =================================================
      if (IsEmpty(export.ObligorCsePerson.Number))
      {
        var field = GetField(export.ObligorCsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
      }

      // *** Obligation Type entered must exist and must have classification of
      // "recovery". ***
      if (IsEmpty(export.ObligationType.Code))
      {
        if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y' || Equal
          (global.Command, "DISPLAY"))
        {
          goto Test3;
        }

        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field = GetField(export.ObligationType, "code");

        field.Error = true;
      }

Test3:

      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
      {
        // : Due Date, Amount, Note are mandatory.
        if (IsEmpty(export.Obligation.Description))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field = GetField(export.Obligation, "description");

          field.Error = true;
        }

        if (Equal(export.DebtDetail.DueDt, local.Blank.Date))
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
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";

        goto Test4;
      }

      // When flowing directly to the OREC screen, if only 1 recovery obligation
      // exists -- display it.  If multiple exist, say so.  07/28/97 Pre.
      if (Equal(global.Command, "DISPLAY"))
      {
        goto Test4;
      }

      if (ReadObligationType1())
      {
        if (AsChar(entities.ObligationType.Classification) == AsChar
          (local.HardcodeRecovery.Classification))
        {
          // ** OK ** Obligation Type Classification is valid
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

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        global.Command = "BYPASS";

        goto Test4;
      }

      // *** If the Legal_Action Identifier is available, READ it to have 
      // currency ***
      if (export.LegalAction.Identifier != 0)
      {
        if (ReadLegalAction())
        {
          // ** The Currency is Obtained...Continue Processing
          MoveLegalAction1(entities.LegalAction, export.LegalAction);
        }
        else
        {
          ExitState = "LEGAL_ACTION_NF";
          global.Command = "BYPASS";
        }
      }
    }

Test4:

    // ================================================
    // Security is handled by Kessep Security modules exlusively.
    // The 'custom code' that had been here which further restricted
    // maintenance authority to the user-id recorded in the database
    // as the one who added the record is invalid and deleted.
    // ================================================
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE"))
    {
      // ************************************************************
      // If logged on user is not a collections officer in office 30,
      // add and update are not allowed.
      // ************************************************************
      local.Obligation.CreatedBy = global.UserId;

      // =================================================
      // 2/26/99 - Bud Adams  -  Using default Read property.  There
      //   are multiple entries in here, and no other way to identify
      //   a specific record - and it doesn't matter.
      // =================================================
      if (ReadOfficeServiceProviderServiceProviderOffice1())
      {
        MoveOfficeServiceProvider(entities.Existing,
          local.OblAssignmentOfficeServiceProvider);
        MoveServiceProvider(entities.ServiceProvider,
          local.OblAssignmentServiceProvider);
        local.OblAssignmentOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
      }
      else
      {
        ExitState = "SP0000_USER_NOT_AUTHORIZED";

        return;
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

      if (!Equal(import.HiddenObligationType.Code, export.ObligationType.Code) &&
        !IsEmpty(import.HiddenObligationType.Code))
      {
        var field = GetField(export.ObligationType, "code");

        field.Error = true;

        ExitState = "CO0000_KEY_CHANGE_NOT_ALLOWD";
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      // *** DATE EDITS for ADD and UPDATE commands.
      // ================================================
      // 10/6/98 - Bud Adams  -  Due Date only available for Update
      // when the creation date is current ddate.
      // ================================================
      if (Lt(export.DebtDetail.DueDt, AddMonths(local.Current.Date, -1)) && (
        Equal(global.Command, "ADD") || Equal
        (export.SetupDate.Date, local.Current.Date)))
      {
        if (Lt(export.DebtDetail.DueDt, AddYears(local.Current.Date, -20)))
        {
          ExitState = "FN0000_DATE_CANT_BE_OVER_20_YRS";

          var field = GetField(export.DebtDetail, "dueDt");

          field.Error = true;

          global.Command = "BYPASS";

          goto Test7;
        }
      }
      else if (Lt(local.Current.Date, export.DebtDetail.DueDt))
      {
        // ================================================
        // 10/6/98  Bud Adams  -  Due date cannot be in the future
        // ================================================
        var field = GetField(export.DebtDetail, "dueDt");

        field.Color = "red";
        field.Highlighting = Highlighting.ReverseVideo;
        field.Protected = false;
        field.Focused = true;

        ExitState = "FN0000_DUE_DATE_CANT_BE_GT_TODAY";
        global.Command = "BYPASS";

        goto Test7;
      }

      // =================================================
      // 1/22/99 - b adams  -  Required for all address types
      // =================================================
      if (IsEmpty(export.CsePersonAddress.LocationType))
      {
      }
      else
      {
        // =================================================
        // 3/31/1999 - Bud Adams  -  If an exit state is set and command
        //   set to BYPASS, all the address fields will be protected.
        //   (BTW: This evening is a blue moon.)
        // =================================================
        local.AddressFound.Flag = "Y";

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (Equal(export.CsePersonAddress.Street1, "Security Block on Address"))
        {
          goto Test5;
        }

        if (IsEmpty(export.CsePersonAddress.City))
        {
          var field = GetField(export.CsePersonAddress, "city");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CsePersonAddress.Street1))
        {
          var field = GetField(export.CsePersonAddress, "street1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CsePersonAddress.Type1))
        {
          var field = GetField(export.CsePersonAddress, "type1");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
        {
          global.Command = "BYPASS";

          goto Test7;
        }
      }

Test5:

      // =================================================
      // 1/27/99 - Bud Adams  -  Do not allow foreign address attributes
      //   to be entered with a "D"omestic address, and vice versa.
      // =================================================
      if (AsChar(export.CsePersonAddress.LocationType) == 'D')
      {
        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (Equal(export.CsePersonAddress.Street1, "Security Block on Address"))
        {
          goto Test6;
        }

        if (!IsEmpty(export.CsePersonAddress.Street3))
        {
          var field = GetField(export.CsePersonAddress, "street3");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.Street4))
        {
          var field = GetField(export.CsePersonAddress, "street4");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.Province))
        {
          var field = GetField(export.CsePersonAddress, "province");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.PostalCode))
        {
          var field = GetField(export.CsePersonAddress, "postalCode");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.Country))
        {
          var field = GetField(export.CsePersonAddress, "country");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (IsExitState("FN0000_NOT_VALID_WITH_ADDR_TYPE"))
        {
          global.Command = "BYPASS";

          goto Test7;
        }

        if (IsEmpty(export.CsePersonAddress.ZipCode))
        {
          var field = GetField(export.CsePersonAddress, "zipCode");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CsePersonAddress.State))
        {
          var field = GetField(export.CsePersonAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsExitState("ACO_NE0000_REQUIRED_DATA_MISSING"))
        {
          global.Command = "BYPASS";

          goto Test7;
        }

        if (AsChar(export.CsePersonAddress.Type1) != 'M' && AsChar
          (export.CsePersonAddress.Type1) != 'R')
        {
          var field = GetField(export.CsePersonAddress, "type1");

          field.Error = true;

          ExitState = "INVALID_ADDRESS_TYPE";

          goto Test7;
        }

        // *****
        // Validate State code and that zip code data is numeric
        // *****
        local.MatchCode.CodeName = "STATE CODE";
        local.MatchCodeValue.Cdvalue = export.CsePersonAddress.State ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'N')
        {
          var field = GetField(export.CsePersonAddress, "state");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_STATE_CODE";
          global.Command = "BYPASS";

          goto Test7;
        }

        local.AddrError.Flag = "";

        if (Length(TrimEnd(export.CsePersonAddress.ZipCode)) > 0 && Length
          (TrimEnd(export.CsePersonAddress.ZipCode)) < 5)
        {
          var field = GetField(export.CsePersonAddress, "zipCode");

          field.Error = true;

          ExitState = "SI_ZIP_CODE_MUST_HAVE_5_DIGITS";
          global.Command = "BYPASS";

          goto Test7;
        }

        if (Length(TrimEnd(export.CsePersonAddress.ZipCode)) == 0 && Length
          (TrimEnd(export.CsePersonAddress.Zip4)) > 0)
        {
          var field = GetField(export.CsePersonAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_5_DGT_REQ_4_DGTS_REQ";
          global.Command = "BYPASS";

          goto Test7;
        }

        if (Length(TrimEnd(export.CsePersonAddress.ZipCode)) > 0 && Length
          (TrimEnd(export.CsePersonAddress.Zip4)) > 0 && Length
          (TrimEnd(export.CsePersonAddress.Zip4)) < 4)
        {
          var field = GetField(export.CsePersonAddress, "zip4");

          field.Error = true;

          ExitState = "SI0000_ZIP_PLS_4_REQ_4_DGTS_OR_0";
          global.Command = "BYPASS";

          goto Test7;
        }

        if (Verify(export.CsePersonAddress.Zip3, " 0123456789") != 0)
        {
          var field = GetField(export.CsePersonAddress, "zip3");

          field.Error = true;

          local.AddrError.Flag = "Y";
        }

        if (Verify(export.CsePersonAddress.Zip4, " 0123456789") != 0)
        {
          var field = GetField(export.CsePersonAddress, "zip4");

          field.Error = true;

          local.AddrError.Flag = "Y";
        }

        if (Verify(export.CsePersonAddress.ZipCode, " 0123456789") != 0)
        {
          var field = GetField(export.CsePersonAddress, "zipCode");

          field.Error = true;

          local.AddrError.Flag = "Y";
        }

        if (AsChar(local.AddrError.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";
          global.Command = "BYPASS";

          goto Test7;
        }
      }
      else if (AsChar(export.CsePersonAddress.LocationType) == 'F')
      {
        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (Equal(export.CsePersonAddress.Street1, "Security Block on Address"))
        {
          goto Test6;
        }

        if (!IsEmpty(export.CsePersonAddress.State))
        {
          var field = GetField(export.CsePersonAddress, "state");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.ZipCode))
        {
          var field = GetField(export.CsePersonAddress, "zipCode");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.Zip3))
        {
          var field = GetField(export.CsePersonAddress, "zip3");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (!IsEmpty(export.CsePersonAddress.Zip4))
        {
          var field = GetField(export.CsePersonAddress, "zip4");

          field.Error = true;

          ExitState = "FN0000_NOT_VALID_WITH_ADDR_TYPE";
        }

        if (IsExitState("FN0000_NOT_VALID_WITH_ADDR_TYPE"))
        {
          global.Command = "BYPASS";

          goto Test7;
        }

        if (IsEmpty(export.CsePersonAddress.Country))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.CsePersonAddress, "country");

          field.Error = true;

          goto Test7;
        }

        // =================================================
        // 1/28/99 - b adams  -  Foreign addresses can only be of type 'M'
        // ailing.
        // 3/31/99 - b adams  -  Exit state changed
        // =================================================
        if (AsChar(export.CsePersonAddress.Type1) != 'M')
        {
          var field = GetField(export.CsePersonAddress, "type1");

          field.Error = true;

          ExitState = "FN0000_FOREIGN_ADDR_IS_MAILING";

          goto Test7;
        }
      }
      else
      {
        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (Equal(export.CsePersonAddress.Street1, "Security Block on Address"))
        {
          goto Test6;
        }

        if (CharAt(export.ObligorCsePerson.Number, 10) == 'O')
        {
          // ----------------------------------------------------------------------
          // PR# 100472 :  Tribunals do not have 'Location Type'  attribute. So,
          // Bypass the code.
          // -----------------------------------------------------------------------
          goto Test6;
        }

        if (!IsEmpty(export.CsePersonAddress.Type1) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street2) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.Street4) || !
          IsEmpty(export.CsePersonAddress.City) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.ZipCode) || !
          IsEmpty(export.CsePersonAddress.Zip3) || !
          IsEmpty(export.CsePersonAddress.Zip4) || !
          IsEmpty(export.CsePersonAddress.Province) || !
          IsEmpty(export.CsePersonAddress.PostalCode) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          var field = GetField(export.CsePersonAddress, "locationType");

          field.Error = true;

          ExitState = "FN0000_LOCATION_TYPE_F_OR_D";
          global.Command = "BYPASS";

          goto Test7;
        }
      }

Test6:

      // =================================================
      // 3/1/00  K. Price - process country or state code for ADD
      // and UPDATE
      // =================================================
      switch(AsChar(export.Obligation.OrderTypeCode))
      {
        case 'I':
          // ---------------------------------------------------------------------------------
          // Per business rules Interstate cases are valid only with court 
          // ordered  Recovery type obligations. Check if Court Order exists or
          // not.
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.LegalAction.StandardNumber))
          {
            // ---------------------------------------------------------------------------------
            //                     Continue processing..................
            // ---------------------------------------------------------------------------------
          }
          else
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Error = true;

            if (!IsEmpty(export.Obligation.OtherStateAbbr))
            {
              var field3 = GetField(export.Obligation, "otherStateAbbr");

              field3.Error = true;
            }

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            if (!IsEmpty(export.InterstateRequest.Country))
            {
              var field3 = GetField(export.InterstateRequest, "country");

              field3.Error = true;
            }

            ExitState = "FN0000_INTERSTATE_CASE_INVALID";

            goto Test7;
          }

          if (IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Error = true;

            ExitState = "FN0000_MANDATORY_FIELDS";
          }

          if (IsEmpty(export.Obligation.OtherStateAbbr))
          {
            if (!IsEmpty(export.InterstateRequest.Country))
            {
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
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Error = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Error = true;

            ExitState = "ACO_NE0000_INVALID_DATA_ENTERED";
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
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

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
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

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
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

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
          export.InterstateRequest.Country = "";
          export.InterstateRequest.OtherStateCaseId = "";
          export.Obligation.OtherStateAbbr = "";

          break;
        default:
          var field = GetField(export.Obligation, "orderTypeCode");

          field.Error = true;

          ExitState = "FN0000_INVALID_INTERSTATE_IND";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test7;
      }

      if (Equal(global.Command, "ADD"))
      {
        if (export.LegalAction.Identifier != 0)
        {
          // *** Check to see if the obligation being added is potentially a 
          // duplicate. ***
          if (AsChar(export.ConfirmObligAdd.Flag) == 'Y')
          {
            // *** This check has already been done if the flag is "Y". ***
            export.ConfirmObligAdd.Flag = "N";
          }
          else if (ReadObligation())
          {
            // ** CHECK IF THE OBLIGATION IS ACTIVE
            ExitState = "CONFIRM_ADD_OF_OBLIGATION";

            // *** Protect fields ***
            var field1 = GetField(export.ObligorCsePerson, "number");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.ObligorPrompt, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationType, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.ObligationTypePrompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DebtDetail, "dueDt");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.ObligationTransaction, "amount");

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 = GetField(export.Obligation, "description");

            field7.Color = "cyan";
            field7.Protected = true;

            // *** Set flag to "Y" - this will indicate in the next pass
            //  that the duplicate check has already been performed. ***
            export.ConfirmObligAdd.Flag = "Y";
            global.Command = "BYPASS";
          }
        }
        else
        {
        }
      }
    }

Test7:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      global.Command = "BYPASS";
    }

    if (AsChar(local.LegalIdPassed.Flag) == 'Y' && !
      Equal(global.Command, "BYPASS"))
    {
      // =================================================
      // 10/30/98 - B Adams  -  What we need to do is ensure that no
      //   more than one Recovery obligation is created for a legal-
      //   action-detail.  The prior Read Each was convoluted and did
      //   not provide the function.
      // =================================================
      if (ReadLegalActionPersonObligationTransactionObligation())
      {
        // --- Obligation already created for this legal action detail. So 
        // display it.
        MoveObligation3(entities.Obligation, export.Obligation);
        global.Command = "DISPLAY";

        goto Test8;
      }

      if (!IsEmpty(export.Obligation.OtherStateAbbr))
      {
        var field = GetField(export.InterstateRequest, "otherStateCaseId");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
      }

      ExitState = "FN0000_OBLIG_NOT_YET_CREATED";
    }

Test8:

    // ***---  PR# 78500 - if adjustments exist, can't change amount
    if (AsChar(export.AdjustmentExists.Flag) == 'Y')
    {
      var field = GetField(export.ObligationTransaction, "amount");

      field.Color = "cyan";
      field.Protected = true;
    }

    // mjr
    // -----------------------------------------------------
    // 11/12/1999
    // Added for after the Print process has completed.
    // ------------------------------------------------------------------
    if (Equal(global.Command, "PRINTRET"))
    {
      // mjr
      // -----------------------------------------------
      // 11/12/1999
      // After the document is Printed (the user may still be looking
      // at WordPerfect), control is returned here.  Any cleanup
      // processing which is necessary after a print, should be done
      // now.
      // ------------------------------------------------------------
      UseScCabNextTranGet();

      // mjr
      // ----------------------------------------------------
      // Extract identifiers from next tran
      // -------------------------------------------------------
      export.ObligorCsePerson.Number =
        export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
      export.Obligation.SystemGeneratedIdentifier =
        export.HiddenNextTranInfo.ObligationId.GetValueOrDefault();
      export.ObligationType.SystemGeneratedIdentifier =
        (int)export.HiddenNextTranInfo.MiscNum2.GetValueOrDefault();

      if (export.ObligationType.SystemGeneratedIdentifier > 0)
      {
        if (ReadObligationType2())
        {
          export.ObligationType.Assign(entities.ObligationType);
        }
        else
        {
          export.ObligationType.SystemGeneratedIdentifier = 0;
        }
      }

      global.Command = "DISPLAY";
    }

    // ***** MAIN-LINE *****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!Equal(export.HiddenObligor.Number, export.ObligorCsePerson.Number) &&
          !IsEmpty(export.HiddenObligor.Number))
        {
          ExitState = "FN0000_CLEAR_BEFORE_NEW_DISPLAY";

          break;
        }

        if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
        {
          UseFnGetOblFromHistMonaNxtran();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // *****************************************************************
        // The following statement will check to see if an interstate
        // request exists. It it does, it will show the interstate payment 
        // address.
        // Then it will check to see if the Alternate
        // Address CSE person is populated.  If so then the address will be
        // populated by that person's most current verified address.
        // If not the address will be populated by the must current verified
        // address of the obligor person.
        // *****************************************************************
        export.CsePersonAddress.Assign(local.Initialized);
        export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

        if (!IsEmpty(export.ObligorCsePerson.Number) || !
          IsEmpty(export.ObligorCsePersonsWorkSet.Number))
        {
          UseFnCabReadCsePersonAddress();
          export.CsePersonAddress.Assign(local.CsePersonAddress);
          export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);
        }
        else
        {
          local.AddressFound.Flag = "N";
        }

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          MoveCsePersonAddress2(export.CsePersonAddress,
            local.DisplayAddressSave);
          export.CsePersonAddress.Assign(local.Initialized);
          MoveCsePersonAddress2(local.DisplayAddressSave,
            export.CsePersonAddress);
          export.CsePersonAddress.Street1 = "Security Block on Address";
        }

        export.ObligationType.Classification =
          local.HardcodeRecovery.Classification;
        UseFnReadRecoveryObligation1();

        // ***---  PR# 78500 - if adjustments exist, can't change amount
        if (AsChar(export.AdjustmentExists.Flag) == 'Y')
        {
          var field = GetField(export.ObligationTransaction, "amount");

          field.Color = "cyan";
          field.Protected = true;
        }

        // =================================================
        // 9/2/99 - bud adams  -  Address changes are not permitted on
        //   the ADD function.
        // =================================================
        export.SetupDate.Date = Date(export.Obligation.CreatedTmst);

        if (Equal(export.SetupDate.Date, local.Blank.Date))
        {
          local.AddressFound.Flag = "N";
        }

        export.SetupDate.Date = local.Blank.Date;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
          export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
          export.Previous.DueDt = export.DebtDetail.DueDt;
          export.HiddenObligationType.Code = export.ObligationType.Code;
          export.HiddenLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.HiddenObligationTransaction.Amount =
            export.ObligationTransaction.Amount;
          export.HiddenAssign.UserId = export.AssignServiceProvider.UserId;
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          export.HiddenObligor.Number = export.ObligorCsePerson.Number;
          export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          if (AsChar(export.Obligation.OrderTypeCode) == 'K')
          {
            export.Obligation.OtherStateAbbr = "";
          }
          else if (Equal(export.SetupDate.Date, local.Current.Date) && !
            IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            var field = GetField(export.InterstateRequest, "otherStateCaseId");

            field.Color = "green";
            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
          }

          if (AsChar(local.DeactiveObligationExists.Flag) == 'Y')
          {
            ExitState = "FN0000_DEACTIVE_OB_CONFIRM_TO_AD";
          }

          // mjr
          // -----------------------------------------------
          // 11/12/1999
          // Added check for an exitstate returned from Print
          // ------------------------------------------------------------
          local.Position.Count =
            Find(String(
              export.HiddenNextTranInfo.MiscText2,
            NextTranInfo.MiscText2_MaxLength),
            TrimEnd(local.SpDocLiteral.IdDocument));

          if (local.Position.Count > 0)
          {
            // mjr---> Determines the appropriate exitstate for the Print 
            // process
            local.PrintWorkArea.Text50 =
              export.HiddenNextTranInfo.MiscText2 ?? Spaces(50);
            UseSpPrintDecodeReturnCode();
            export.HiddenNextTranInfo.MiscText2 = local.PrintWorkArea.Text50;
          }

          if (ReadObligCollProtectionHist())
          {
            export.ObCollProtAct.Flag = "Y";
          }
          else
          {
            export.ObCollProtAct.Flag = "N";
          }
        }
        else if (IsExitState("FN0000_MULT_RECOVERY_OBLIG"))
        {
          ExitState = "FN0000_MULT_OBLIGATIONS_FOUND";
          export.CsePersonAddress.Assign(local.Initialized);
          export.HiddenCsePersonAddress.Assign(local.Initialized);
          local.AddressFound.Flag = "N";
        }
        else
        {
          break;
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

        switch(AsChar(export.Obligation.OrderTypeCode))
        {
          case 'I':
            UseFnRetrieveInterstateRequest();

            break;
          case 'K':
            export.Obligation.OtherStateAbbr = "";
            export.HiddenObligation.OtherStateAbbr = "";
            export.InterstateRequest.Country = "";
            export.Country.Cdvalue = "";
            export.Country.Description =
              Spaces(CodeValue.Description_MaxLength);

            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "otherStateAbbr");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "country");

            field4.Color = "cyan";
            field4.Protected = true;

            break;
          case ' ':
            export.Obligation.OrderTypeCode = "K";
            export.Obligation.OtherStateAbbr = "";
            export.InterstateRequest.Country = "";

            break;
          default:
            export.Obligation.OrderTypeCode = "K";
            export.Obligation.OtherStateAbbr = "";

            break;
        }

        // ***---  end of CASE DISPLAY
        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!Equal(export.HiddenObligor.Number, export.ObligorCsePerson.Number) &&
          !IsEmpty(export.HiddenObligor.Number))
        {
          ExitState = "FN0000_CLEAR_BEFORE_ADD";

          break;
        }

        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_OBLIGATION_AE";

          break;
        }

        // =================================================
        // 6/23/99 - bud adams  -  Assigned To is not going to be forced
        //   into being the logged on user id.
        // =================================================
        if (IsEmpty(export.AssignServiceProvider.UserId))
        {
          local.OblAssignmentServiceProvider.UserId = global.UserId;
        }
        else
        {
          local.OblAssignmentServiceProvider.UserId =
            export.AssignServiceProvider.UserId;
        }

        if (ReadOfficeServiceProviderServiceProviderOffice2())
        {
          MoveOfficeServiceProvider(entities.Existing,
            local.OblAssignmentOfficeServiceProvider);
          MoveServiceProvider(entities.ServiceProvider,
            local.OblAssignmentServiceProvider);
          local.OblAssignmentOffice.SystemGeneratedId =
            entities.Office.SystemGeneratedId;
        }
        else
        {
          var field = GetField(export.AssignServiceProvider, "userId");

          field.Error = true;

          ExitState = "FN0000_ASSIGN_TO_NOT_VALID";

          break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseFnCreateObligation();

        if (IsExitState("DEFAULT_RULE_NF"))
        {
          ExitState = "DEFAULT_RULE_NF_RB";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ************************************************************************
        // Create obligation assignment to collection officer for logged on user
        // id.
        // ************************************************************************
        local.PassToCreate.ReasonCode = "RSP";
        local.PassToCreate.EffectiveDate = local.Current.Date;
        local.PassToCreate.DiscontinueDate = local.Max.Date;
        local.PassToCreate.OverrideInd = "N";

        // *****  Changed view match problem with CSE_Person!!  *****
        // 		B A
        UseSpCabCreateOblgAssignment();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (ReadServiceProvider())
        {
          export.AssignServiceProvider.UserId = entities.ServiceProvider.UserId;
          export.AssignCsePersonsWorkSet.FormattedName =
            TrimEnd(entities.ServiceProvider.LastName) + ", " + entities
            .ServiceProvider.FirstName + " " + entities
            .ServiceProvider.MiddleInitial;
        }

        export.Obligation.CreatedBy = global.UserId;
        export.SetupDate.Date = local.Current.Date;

        // *** Set the Indicators ***
        if (IsEmpty(export.ObligationPaymentSchedule.FrequencyCode))
        {
          export.PaymentScheduleInd.Flag = "N";
        }
        else
        {
          export.PaymentScheduleInd.Flag = "Y";
        }

        export.ManualDistributionInd.Flag = "N";
        export.SuspendInterestInd.Flag = "Y";

        // *** Create the Interest_Suspension_Status_Hist
        UseFnCabCreateIntSuspStatHist();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        export.DebtDetail.BalanceDueAmt = export.ObligationTransaction.Amount;
        export.DebtDetail.InterestBalanceDueAmt = 0;
        UseFnCreateObligationTransaction();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // -- 2008/03/17  GVandy CQ296  Adjust Disbursement_Summary 
        // non_taf_amount when recovery obligations are created for non-TAF
        // cases.
        UseFnDraFeeProcessRecoveryObg();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // -------------------------------------------------------------------
        // 12/29/97	Venkatesh Kamaraj	Set the situation # to 0 because of 
        // changes
        // to infrastructure
        // -------------------------------------------------------------------
        local.Infrastructure.SituationNumber = 0;
        local.Infrastructure.CsePersonNumber = export.ObligorCsePerson.Number;
        local.Infrastructure.UserId = "OREC";
        local.Infrastructure.EventId = 45;

        if (export.LegalAction.Identifier == 0)
        {
          local.Infrastructure.BusinessObjectCd = "OBL";
          local.Infrastructure.DenormNumeric12 =
            export.ObligationTransaction.SystemGeneratedIdentifier;
          local.Infrastructure.DenormText12 = local.HardcodeDebt.Type1;
          local.Infrastructure.ReferenceDate = local.Current.Date;
        }
        else
        {
          local.Infrastructure.ReferenceDate = export.LegalAction.FiledDate;
          local.Infrastructure.BusinessObjectCd = "LEA";
          local.Infrastructure.DenormNumeric12 = export.LegalAction.Identifier;
          local.Infrastructure.DenormText12 =
            export.LegalAction.CourtCaseNumber ?? "";
        }

        UseFnCabRaiseEvent();
        export.BalanceOwed.TotalCurrency = export.ObligationTransaction.Amount;
        export.InterestOwed.TotalCurrency = 0;
        export.TotalOwed.TotalCurrency = export.ObligationTransaction.Amount;

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenObligationType.Code = export.ObligationType.Code;
          export.HiddenLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.HiddenObligationTransaction.Amount =
            export.ObligationTransaction.Amount;
          export.Previous.DueDt = export.DebtDetail.DueDt;
        }
        else
        {
          break;
        }

        // *** Since the Obligation is successfully created, Setup the alternate
        // address for this Obligation, if any.
        // ***--- Sumanta - MTW - 04/30/97
        // ***--- Call to CAB_set_alternate_address has been deleted.
        //        The alternate (cse_person) will be associated to
        //        the obligation
        // ***---
        if (!IsEmpty(export.AlternateAddr.Number))
        {
          // =================================================
          // 1/4/99 - b adams  -  Replaced the CRUD, etc., with this CAB
          //   since this is functionality common throughout all Debt screens.
          // =================================================
          UseFnUpdateAlternateAddress();

          if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
          {
            var field = GetField(export.ObligorCsePerson, "number");

            field.Error = true;
          }
          else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
          {
            var field = GetField(export.AlternateAddr, "number");

            field.Error = true;
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        if (export.InterstateRequest.IntHGeneratedId != 0)
        {
          // =================================================
          // 2/23/1999 - bud adams  -  replaced CRUD actions that had
          //   been here.  Not fully qualified; used in each debt PrAD, and
          //   I didn't feel like fixing each one of them.  Made it common
          //   code instead.  Gee.  What a concept...
          // =================================================
          UseFnCreateInterstateRqstOblign();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        UseFnCabReadCsePersonAddress();

        if (AsChar(local.AddressFound.Flag) == 'N')
        {
          export.CsePersonAddress.Assign(local.Initialized);
        }
        else if (AsChar(local.AddressFound.Flag) == 'T')
        {
        }
        else if (IsEmpty(import.CsePersonAddress.Street1) && IsEmpty
          (import.CsePersonAddress.Street2) && IsEmpty
          (import.CsePersonAddress.Street3) && IsEmpty
          (import.CsePersonAddress.Street4) && IsEmpty
          (import.CsePersonAddress.City) && IsEmpty
          (import.CsePersonAddress.State) && IsEmpty
          (import.CsePersonAddress.ZipCode) && IsEmpty
          (import.CsePersonAddress.Zip3) && IsEmpty
          (import.CsePersonAddress.Zip4) && IsEmpty
          (import.CsePersonAddress.Province) && IsEmpty
          (import.CsePersonAddress.Country) && IsEmpty
          (import.CsePersonAddress.PostalCode) && IsEmpty
          (export.CsePersonAddress.LocationType) && IsEmpty
          (export.CsePersonAddress.Type1))
        {
          export.CsePersonAddress.Assign(local.CsePersonAddress);

          // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
          }
          else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
            IsEmpty(export.CsePersonAddress.Street1) || !
            IsEmpty(export.CsePersonAddress.Street3) || !
            IsEmpty(export.CsePersonAddress.State) || !
            IsEmpty(export.CsePersonAddress.Country))
          {
            MoveCsePersonAddress2(export.CsePersonAddress,
              local.DisplayAddressSave);
            export.CsePersonAddress.Assign(local.Initialized);
            MoveCsePersonAddress2(local.DisplayAddressSave,
              export.CsePersonAddress);
            export.CsePersonAddress.Street1 = "Security Block on Address";
          }

          local.AddressFound.Flag = "N";
        }

        // -----------------------------------------------------------------------------
        // Create trigger so that Recovery Debt letter is created and mailed 
        // out.
        // ---------------------------------------------------------------------------
        local.PrintDocument.Name = "RECOV";
        local.SpDocKey.KeyPersonAccount = local.HardcodeObligor.Type1;
        local.SpDocKey.KeyPerson = export.ObligorCsePerson.Number;
        local.SpDocKey.KeyObligation =
          export.Obligation.SystemGeneratedIdentifier;
        local.SpDocKey.KeyObligationType =
          export.ObligationType.SystemGeneratedIdentifier;
        UseSpCreateDocumentInfrastruct();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.HiddenObligation.OrderTypeCode =
            export.Obligation.OrderTypeCode;
          export.HiddenObligation.OtherStateAbbr =
            export.Obligation.OtherStateAbbr ?? "";
          export.MustClearFirst.Flag = "Y";
          export.AdjustmentExists.Flag = "N";
          export.HiddenAssign.UserId = export.AssignServiceProvider.UserId;
          export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;
          export.HiddenObligor.Number = export.ObligorCsePerson.Number;
          ExitState = "ACO_NI0000_CREATE_OK";

          if (Equal(export.FlowFrom.Text4, "OREL"))
          {
            export.FlowFrom.Text4 = "";
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
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "otherStateAbbr");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "country");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }
        else
        {
        }

        // ***---  end of CASE ADD
        break;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (!Equal(export.HiddenObligor.Number, export.ObligorCsePerson.Number))
        {
          ExitState = "FN0000_CLEAR_BEFORE_UPDATE";

          break;
        }

        // =================================================
        // 6/23/99 - bud adams  -  Allow user to Update the Assigned To person
        // =================================================
        if (!Equal(export.HiddenAssign.UserId,
          export.AssignServiceProvider.UserId))
        {
          if (IsEmpty(export.AssignServiceProvider.UserId))
          {
            var field = GetField(export.AssignServiceProvider, "userId");

            field.Error = true;

            export.AssignCsePersonsWorkSet.FormattedName = "";
            ExitState = "CO0000_UNABLE_TO_UPDATE_TO_SPACE";

            break;
          }

          local.OblAssignmentServiceProvider.UserId =
            export.HiddenAssign.UserId;

          // =================================================
          // 6/23/99 - Bud Adams  -  Using default Read property.  There
          //   are multiple entries in here, and no other way to identify
          //   a specific record - and it doesn't matter.  The logged-on
          //   user is the one this is going to be assigned to.
          // =================================================
          // ***---  get the existing assignment.
          if (ReadOfficeServiceProviderServiceProviderOffice2())
          {
            MoveOfficeServiceProvider(entities.Existing,
              local.OblAssignmentOfficeServiceProvider);
            MoveServiceProvider(entities.ServiceProvider,
              local.OblAssignmentServiceProvider);
            local.OblAssignmentOffice.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
          }
          else
          {
            var field = GetField(export.AssignServiceProvider, "userId");

            field.Error = true;

            ExitState = "FN0000_ASSIGN_TO_NOT_VALID";

            break;
          }

          // =================================================
          // Now discontinue the previous assignment
          // =================================================
          if (ReadObligationAssignment())
          {
            // =================================================
            // 08/09/99 - Alan Doty - Changed to fix a problem with overlapping
            // Effective/Discontinue dates on Obligation Assignment.
            // =================================================
            if (Equal(entities.ObligationAssignment.EffectiveDate,
              local.Current.Date))
            {
              DeleteObligationAssignment();
            }
            else
            {
              try
              {
                UpdateObligationAssignment();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }

          // ************************************************************************
          // Create the new obligation assignment to collection officer for user
          // -supplied user id.
          // ************************************************************************
          local.OblAssignmentServiceProvider.UserId =
            export.AssignServiceProvider.UserId;

          // =================================================
          // 3/26/99 - Bud Adams  -  Using default Read property.  There
          //   are multiple entries in here, and no other way to identify
          //   a specific record - and it doesn't matter.  The logged-on
          //   user is the one this is going to be assigned to.
          // =================================================
          if (ReadOfficeServiceProviderServiceProviderOffice2())
          {
            MoveOfficeServiceProvider(entities.Existing,
              local.OblAssignmentOfficeServiceProvider);
            MoveServiceProvider(entities.ServiceProvider,
              local.OblAssignmentServiceProvider);
            local.OblAssignmentOffice.SystemGeneratedId =
              entities.Office.SystemGeneratedId;
          }
          else
          {
            var field = GetField(export.AssignServiceProvider, "userId");

            field.Error = true;

            ExitState = "FN0000_ASSIGN_TO_NOT_VALID";

            break;
          }

          // ************************************************************************
          // Create obligation assignment for NEW collection officer.
          // ************************************************************************
          local.PassToCreate.ReasonCode = "RSP";
          local.PassToCreate.EffectiveDate = local.Current.Date;
          local.PassToCreate.DiscontinueDate = local.Max.Date;
          local.PassToCreate.OverrideInd = "N";
          UseSpCabCreateOblgAssignment();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          if (ReadServiceProvider())
          {
            export.AssignServiceProvider.UserId =
              entities.ServiceProvider.UserId;
            export.AssignCsePersonsWorkSet.FormattedName =
              TrimEnd(entities.ServiceProvider.LastName) + ", " + entities
              .ServiceProvider.FirstName + " " + entities
              .ServiceProvider.MiddleInitial;
          }
          else
          {
            break;
          }
        }

        export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
        export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
        export.Previous.DueDt = export.DebtDetail.DueDt;
        export.HiddenObligationType.Code = export.ObligationType.Code;
        export.HiddenLegalAction.StandardNumber =
          export.LegalAction.StandardNumber;
        MoveObligation6(export.Obligation, export.HiddenObligation);
        export.HiddenObligationTransaction.Amount =
          export.ObligationTransaction.Amount;
        export.SetupDate.Date = import.SetupDate.Date;

        if (Equal(export.SetupDate.Date, Now().Date) && AsChar
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

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.HiddenObligor.Number = "";
          export.HiddenObligation.SystemGeneratedIdentifier = 0;
          export.Previous.DueDt = export.DebtDetail.DueDt;
          export.HiddenObligationTransaction.Amount =
            export.ObligationTransaction.Amount;
          export.BalanceOwed.TotalCurrency =
            export.ObligationTransaction.Amount;
          export.InterestOwed.TotalCurrency = 0;
          export.TotalOwed.TotalCurrency =
            export.LegalActionDetail.JudgementAmount.GetValueOrDefault();
        }
        else
        {
          local.Failure.Flag = "U";

          break;
        }

        // *** Since the Obligation was successfully Updated, Set up the new 
        // alternate
        // address for the Obligation if any.
        // =================================================
        // 1/4/99 - b adams  -  These notes apply to the CAB below which
        //   was created so function common to all debt screens can be
        //   maintained in just one place.
        // =================================================
        // ***--- Sumanta - MTW - 04/29/97
        // ***--- If the alt addr is present, associate to the obligation ..
        // ***---
        // *** 12/17/98 - Bud - to allow the removal of an alternate addr;
        // ***                  changed the IF; also prevent all this database
        // ***                  activity if the alt-addr didn't change.
        if (!Equal(export.AlternateAddr.Number,
          export.HiddenAlternateAddr.Number))
        {
          UseFnUpdateAlternateAddress();

          if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
          {
            var field = GetField(export.AlternateAddr, "number");

            field.Error = true;
          }
          else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
          {
            var field = GetField(export.AlternateAddr, "number");

            field.Error = true;
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.Failure.Flag = "U";

            break;
          }
        }

        if (!Equal(export.HiddenInterstateRequest.OtherStateCaseId,
          export.InterstateRequest.OtherStateCaseId))
        {
          UseFnUpdateInterstateRqstOblign();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        UseFnCabReadCsePersonAddress();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // =================================================
        // 12/31/98 - b adams  -  The address processing had been
        //   incorrectly on the ELSE side of the above IF construct and
        //   was then prevented from being processed some of the time
        // =================================================
        if (AsChar(local.AddressFound.Flag) == 'Y')
        {
          // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
          if (Equal(export.CsePersonAddress.Street1, "Security Block on Address"))
            
          {
            goto Test9;
          }

          if (Equal(export.CsePersonAddress.Street1,
            export.HiddenCsePersonAddress.Street1) && Equal
            (export.CsePersonAddress.Street2,
            export.HiddenCsePersonAddress.Street2) && Equal
            (export.CsePersonAddress.Street3,
            export.HiddenCsePersonAddress.Street3) && Equal
            (export.CsePersonAddress.Street4,
            export.HiddenCsePersonAddress.Street4) && Equal
            (export.CsePersonAddress.City, export.HiddenCsePersonAddress.City) &&
            Equal
            (export.CsePersonAddress.State, export.HiddenCsePersonAddress.State) &&
            Equal
            (export.CsePersonAddress.ZipCode,
            export.HiddenCsePersonAddress.ZipCode) && Equal
            (export.CsePersonAddress.Zip4, export.HiddenCsePersonAddress.Zip4) &&
            Equal
            (export.CsePersonAddress.Zip3, export.HiddenCsePersonAddress.Zip3) &&
            Equal
            (export.CsePersonAddress.Province,
            export.HiddenCsePersonAddress.Province) && Equal
            (export.CsePersonAddress.PostalCode,
            export.HiddenCsePersonAddress.PostalCode) && Equal
            (export.CsePersonAddress.Country,
            export.HiddenCsePersonAddress.Country) && AsChar
            (export.CsePersonAddress.Type1) == AsChar
            (export.HiddenCsePersonAddress.Type1))
          {
            goto Test9;
          }

          // =================================================
          // 12/17/98 - B Adams  -  This was creating a new address record
          //   every time whenever an address existed.  Also, if only the ZIP
          //   code changes (meaning a mistake was made) all we want
          //   to do is Update - not create a new one.
          // =================================================
          if (Equal(export.CsePersonAddress.Street1,
            export.HiddenCsePersonAddress.Street1) && Equal
            (export.CsePersonAddress.Street2,
            export.HiddenCsePersonAddress.Street2) && Equal
            (export.CsePersonAddress.Street3,
            export.HiddenCsePersonAddress.Street3) && Equal
            (export.CsePersonAddress.Street4,
            export.HiddenCsePersonAddress.Street4) && Equal
            (export.CsePersonAddress.City, export.HiddenCsePersonAddress.City) &&
            Equal
            (export.CsePersonAddress.State, export.HiddenCsePersonAddress.State) &&
            Equal
            (export.CsePersonAddress.Province,
            export.HiddenCsePersonAddress.Province) && Equal
            (export.CsePersonAddress.PostalCode,
            export.HiddenCsePersonAddress.PostalCode) && Equal
            (export.CsePersonAddress.Country,
            export.HiddenCsePersonAddress.Country))
          {
            if (!Equal(export.CsePersonAddress.ZipCode,
              export.HiddenCsePersonAddress.ZipCode) || !
              Equal(export.CsePersonAddress.Zip4,
              export.HiddenCsePersonAddress.Zip4) || AsChar
              (export.CsePersonAddress.Type1) != AsChar
              (export.HiddenCsePersonAddress.Type1) || !
              Equal(export.CsePersonAddress.Zip3,
              export.HiddenCsePersonAddress.Zip3))
            {
              // =================================================
              // 1/6/99 - Bud Adams  -  If the only thing that changed is the 
              // ZIP
              //   Code, then we assume it's being done to correct a mistake
              //   and we will then update the address.
              // =================================================
              MoveCsePersonAddress1(export.CsePersonAddress,
                local.CsePersonAddress);
              local.CsePersonAddress.Source = "CRU";
              local.CsePersonAddress.SendDate = local.Current.Date;
              local.CsePersonAddress.VerifiedDate = local.Current.Date;
              local.CsePersonAddress.LastUpdatedBy = local.Obligation.CreatedBy;
              local.CsePersonAddress.CreatedBy = local.Obligation.CreatedBy;
              local.CsePersonAddress.EndDate = local.Max.Date;
              UseSiUpdateCsePersonAddress();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "FN0000_CSE_PRSN_ADDRS_NOT_ADD_RB";
                local.Failure.Flag = "U";

                break;
              }
            }

            // ***---  This is not considered to be an address change, so escape
            // ***---  beyond the infrastructure (alert / event) creation
            goto Test9;
          }
          else
          {
            // =================================================
            // 1/6/99 - b adams  -  If the address is changed, we first update
            //   the existing address and just change the end date.  Then we
            //   create a brand new address record.
            // 4/23/99 - b adams  -  End_Code was not being set
            // =================================================
            local.CsePersonAddress.EndDate = local.Current.Date;
            local.CsePersonAddress.EndCode = "CU";
            UseSiUpdateCsePersonAddress();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              ExitState = "FN0000_CSE_PRSN_ADDRS_NOT_ADD_RB";
              local.Failure.Flag = "U";

              break;
            }

            local.CaseRole.Type1 = "AP";
            UseFnDetermineCaseRoleForOrec();
            MoveCsePersonAddress1(export.CsePersonAddress,
              local.CsePersonAddress);

            if (Equal(local.CaseRole.Type1, "AP"))
            {
              local.CsePersonAddress.VerifiedDate = local.Blank.Date;
              local.CsePersonAddress.SendDate = local.Current.Date;
            }
            else
            {
              local.CsePersonAddress.VerifiedDate = local.Current.Date;
              local.CsePersonAddress.SendDate = local.Blank.Date;
            }

            local.CsePersonAddress.EndDate = local.Max.Date;
            local.CsePersonAddress.Source = "CRU";
            local.CsePersonAddress.EndCode = "";
            local.CsePersonAddress.LastUpdatedBy = local.Obligation.CreatedBy;
            local.CsePersonAddress.CreatedBy = local.Obligation.CreatedBy;
            UseSiCreateCsePersonAddress();
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_CSE_PRSN_ADDRS_NOT_ADD_RB";
            local.Failure.Flag = "U";

            break;
          }

          // =================================================
          // 12/16/98 - B Adams  -  This used to be inside an IF construct
          //   that tested for address Verified_Code="VE".  They decided
          //   to dump that attribute.
          // 4/20/99 - Bud Adams  -  business object code changed from
          //   "CPA" to "CAU"; added event type = "LOC"; one infra record
          //   for each case
          // =================================================
          if (Equal(local.CaseRole.Type1, "AR"))
          {
            // =================================================
            // PR# 228: 8/27/99 - Bud Adams  -  When an AR has had their
            //   address changed on OREC and that AR is involved in more
            //   than one Case, an alert needs to be sent to all OSPs.  The
            //   tag-line of  ", M-OPS" needs to be applied to the detail.
            //   The Read Eaches are 'DISTINCT'
            // =================================================
            foreach(var item in ReadCase3())
            {
              ++local.ValidCode.Count;
            }

            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.EventId = 10;
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = "OREC";
            local.Infrastructure.ReasonCode = "ARNEWADDRFN";
            local.Infrastructure.InitiatingStateCode = "KS";
            local.Infrastructure.EventType = "LOC";
            local.Infrastructure.BusinessObjectCd = "CAU";
            local.Infrastructure.ReferenceDate =
              local.CsePersonAddress.SendDate;
            local.Infrastructure.CsePersonNumber =
              export.ObligorCsePerson.Number;
            local.Infrastructure.DenormTimestamp =
              local.CsePersonAddress.Identifier;
            local.Infrastructure.ReferenceDate = local.Current.Date;
            local.Infrastructure.Detail =
              TrimEnd(local.CsePersonAddress.Street1) + "," + TrimEnd
              (local.CsePersonAddress.Street2) + "," + TrimEnd
              (local.CsePersonAddress.City);

            foreach(var item in ReadCaseCaseUnit())
            {
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

              if (AsChar(local.CsePersonAddress.LocationType) == 'D')
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + "," + TrimEnd
                  (local.CsePersonAddress.State) + "," + (
                    local.CsePersonAddress.ZipCode ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + "," + (
                    local.CsePersonAddress.Province ?? "") + "," + TrimEnd
                  (local.CsePersonAddress.Country);
              }

              if (local.ValidCode.Count == 1)
              {
              }
              else
              {
                local.Infrastructure.Detail =
                  (local.Infrastructure.Detail ?? "") + ", M-OSP";
              }

              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "FN0000_ERROR_ON_EVENT_CREATION";

                goto Test10;
              }
            }

            if (ReadProgramPersonProgram())
            {
              if (Equal(local.Program.Code, "NA") || Equal
                (local.Program.Code, "NC") || Equal
                (local.Program.Code, "AFI") || Equal
                (local.Program.Code, "FCI") || Equal
                (local.Program.Code, "MAI") || Equal
                (local.Program.Code, "NAI"))
              {
              }
              else
              {
                // ***---  External alert to the worker involved with the 
                // assistance
                // ***--- Codes of AF, NF, FS, CC, MA, MS, CI, SI, MP, MK, or FC
                // ***---  Alert code 45 is for either domestic or foreign 
                // address changes
                local.InterfaceAlert.AlertCode = "45";
                UseSpAddrExternalAlert();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "FN0000_ERROR_ON_EXTRNL_EVENT_CRE";

                  break;
                }
              }
            }
          }
          else
          {
            // mjr
            // ----------------------------------------------------
            // Place identifiers into next tran
            // -------------------------------------------------------
            export.HiddenNextTranInfo.MiscText2 =
              TrimEnd(local.SpDocLiteral.IdDocument) + "POSTMAST";

            // mjr---> Passing the Person number in the next_tran obligor
            // 	attribute counts for both the Person Number and the
            // 	Person Account
            export.HiddenNextTranInfo.CsePersonNumberObligor =
              export.ObligorCsePerson.Number;
            export.HiddenNextTranInfo.ObligationId =
              export.Obligation.SystemGeneratedIdentifier;
            export.HiddenNextTranInfo.MiscNum2 =
              export.ObligationType.SystemGeneratedIdentifier;
            local.BatchTimestampWorkArea.IefTimestamp =
              local.CsePersonAddress.Identifier;
            UseLeCabConvertTimestamp();
            export.HiddenNextTranInfo.MiscText1 =
              TrimEnd(local.SpDocLiteral.IdPersonAddress) + local
              .BatchTimestampWorkArea.TextTimestamp;

            if (ReadCase1())
            {
              export.HiddenNextTranInfo.CaseNumber = entities.Case1.Number;
            }

            export.Standard.NextTransaction = "DKEY";
            local.PrintProcess.Flag = "Y";
            local.PrintProcess.Command = "PRINT";
            UseScCabNextTranPut2();

            break;
          }

          // ***---  End of address change section  ---***
        }

Test9:

        UseFnReadRecoveryObligation2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
          export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
          export.Previous.DueDt = export.DebtDetail.DueDt;
          export.HiddenObligationType.Code = export.ObligationType.Code;
          export.HiddenLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.HiddenObligationTransaction.Amount =
            export.ObligationTransaction.Amount;

          // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
          // * * * Moved move statement inside IF
          if (!Equal(export.CsePersonAddress.Street1,
            "Security Block on Address"))
          {
            export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);
          }

          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          export.HiddenObligation.OrderTypeCode =
            export.Obligation.OrderTypeCode;
          export.HiddenObligation.OtherStateAbbr =
            export.Obligation.OtherStateAbbr ?? "";
          export.MustClearFirst.Flag = "Y";

          if (IsEmpty(export.AlternateAddr.Number))
          {
            export.AlternateAddr.FormattedName = "";
          }

          export.HiddenObligor.Number = export.ObligorCsePerson.Number;
          export.HiddenAssign.UserId = export.AssignServiceProvider.UserId;
          export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;

          if (AsChar(local.AddrError.Flag) == 'Y')
          {
            ExitState = "FN0000_UPDT_SUCCSS_ADDR_NOT_UPDT";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
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
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.Obligation, "otherStateAbbr");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "country");

            field4.Color = "cyan";
            field4.Protected = true;
          }
        }
        else
        {
          // =================================================
          // 1/7/99 - b adams  -  Even if the update fails, the basic data
          //   from the screen must be maintained for the next pass.
          // =================================================
          export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
          export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
          export.Previous.DueDt = export.DebtDetail.DueDt;
          export.HiddenObligationType.Code = export.ObligationType.Code;
          export.HiddenLegalAction.StandardNumber =
            export.LegalAction.StandardNumber;
          MoveObligation6(export.Obligation, export.HiddenObligation);
          export.HiddenObligationTransaction.Amount =
            export.ObligationTransaction.Amount;
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
        }

        // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
          IsEmpty(export.CsePersonAddress.Street1) || !
          IsEmpty(export.CsePersonAddress.Street3) || !
          IsEmpty(export.CsePersonAddress.State) || !
          IsEmpty(export.CsePersonAddress.Country))
        {
          MoveCsePersonAddress2(export.CsePersonAddress,
            local.DisplayAddressSave);
          export.CsePersonAddress.Assign(local.Initialized);
          MoveCsePersonAddress2(local.DisplayAddressSave,
            export.CsePersonAddress);
          export.CsePersonAddress.Street1 = "Security Block on Address";
        }

        // ***---  end of CASE UPDATE
        break;
      case "DELETE":
        if (!Equal(export.HiddenObligor.Number, export.ObligorCsePerson.Number))
        {
          ExitState = "FN0000_CLEAR_BEFORE_DELETE";

          break;
        }

        if (Equal(local.Current.Date, export.SetupDate.Date))
        {
        }
        else
        {
          ExitState = "FN0000_CANT_DEL_AFTER_CREAT_DATE";

          break;
        }

        // =================================================
        // 4/6/99 - b adams  -  use FN_Check_Obligation_For_Activity
        //   deleted.  Not a valid check.  Obligations can be deleted
        //   only on the day of creation.
        // =================================================
        UseFnRemoveObligation();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // =================================================
          // 3/11/1999 - bud adams  -  The RECOV document 'print' trigger
          //   has to be cancelled.
          // =================================================
          local.PrintDocument.Name = "RECOV";
          local.SpDocKey.KeyPersonAccount = local.HardcodeObligor.Type1;
          local.SpDocKey.KeyPerson = export.ObligorCsePerson.Number;
          local.SpDocKey.KeyObligation =
            export.Obligation.SystemGeneratedIdentifier;
          local.SpDocKey.KeyObligationType =
            export.ObligationType.SystemGeneratedIdentifier;
          UseSpDocFindOutgoingDocument();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          export.HiddenObligor.Number = "";
          export.HiddenObligation.SystemGeneratedIdentifier = 0;
          export.Obligation.SystemGeneratedIdentifier = 0;
          export.MustClearFirst.Flag = "Y";

          if (local.Infrastructure.SystemGeneratedIdentifier > 0)
          {
            // =================================================
            // Right now, we can only delete the same day the recovery
            //   debt was created.  So, the only condition we will find is "G"
            //   In case things change down the road, the other conditions
            //   are in this CASE OF construct to document their meaning.
            // =================================================
            switch(AsChar(local.OutgoingDocument.PrintSucessfulIndicator))
            {
              case 'Y':
                // ***---  Document is already printed
                break;
              case 'N':
                // ***---  Document was unsuccessfully processed
                break;
              case 'B':
                // ***---  Document is generated, awaiting for printing
                break;
              case 'G':
                UseSpDocCancelOutgoingDoc();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  break;
                }

                break;
              case 'C':
                // ***---  Document print is cancelled
                break;
              default:
                break;
            }
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else
        {
        }

        // ***---   end of CASE DELETE
        break;
      case "BYPASS":
        break;
      case "LIST":
        local.NoOfPromptsSelected.Count = 0;

        if (AsChar(export.ObligorPrompt.SelectChar) != 'S' && AsChar
          (export.AssignPrompt.SelectChar) != 'S' && AsChar
          (export.AltAddPrompt.Text1) != 'S' && AsChar
          (export.ObligationTypePrompt.SelectChar) != 'S')
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
            case 10:
              export.ObligationTypePrompt.SelectChar = "S";

              break;
            default:
              break;
          }
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
            var field = GetField(export.ObligorPrompt, "selectChar");

            field.Error = true;

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
            var field = GetField(export.AssignPrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
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
            var field = GetField(export.AltAddPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        switch(AsChar(export.ObligationTypePrompt.SelectChar))
        {
          case 'S':
            ++local.NoOfPromptsSelected.Count;

            break;
          case '+':
            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ObligationTypePrompt, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (local.NoOfPromptsSelected.Count > 1)
        {
          if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
          {
            var field = GetField(export.ObligorPrompt, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.AssignPrompt.SelectChar) == 'S')
          {
            var field = GetField(export.AssignPrompt, "selectChar");

            field.Error = true;
          }

          if (AsChar(export.AltAddPrompt.Text1) == 'S')
          {
            var field = GetField(export.AltAddPrompt, "text1");

            field.Error = true;
          }

          if (AsChar(export.ObligationTypePrompt.SelectChar) == 'S')
          {
            var field = GetField(export.ObligationTypePrompt, "selectChar");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          break;
        }
        else if (local.NoOfPromptsSelected.Count == 0)
        {
          var field1 = GetField(export.ObligationTypePrompt, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.AltAddPrompt, "text1");

          field2.Error = true;

          var field3 = GetField(export.AssignPrompt, "selectChar");

          field3.Error = true;

          var field4 = GetField(export.ObligorPrompt, "selectChar");

          field4.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

          break;
        }

        if (AsChar(export.ObligorPrompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          export.ObligorPrompt.SelectChar = "+";

          return;
        }

        if (AsChar(export.AssignPrompt.SelectChar) == 'S')
        {
          if (export.Obligation.SystemGeneratedIdentifier == 0)
          {
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            break;
          }

          // =================================================
          // 3/31/1999 - Bud Adams -  Read was improperly qualified
          // =================================================
          if (ReadCsePersonAccount())
          {
            export.Pass.Type1 = entities.CsePersonAccount.Type1;
            export.Object1.Text20 = "OBLIGATION";
            export.FlowFrom.Text4 = "ASIN";
            ExitState = "ECO_LNK_TO_ASIN";

            return;
          }
          else
          {
            ExitState = "FN0000_OBLIGOR_ACCT_NF";

            break;
          }
        }

        if (AsChar(export.AltAddPrompt.Text1) == 'S')
        {
          ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

          return;
        }

        if (AsChar(import.ObligationTypePrompt.SelectChar) == 'S')
        {
          // : List for Obligation Type HERE!!!!
          ExitState = "ECO_LNK_TO_LST_OBLIGATION_TYPE";

          // =================================================
          // 10/20/98 - B Adams  -  Only list Recovery type of Obligation
          //   Types for selection.
          // =================================================
          export.ObligationType.Classification =
            local.HardcodeRecovery.Classification;
          export.ObligationType.Code = "";

          return;
        }

        local.NoOfPromptsSelected.Count = 0;

        break;
      case "MDIS":
        ExitState = "ECO_LNK_TO_MTN_MANUAL_DIST_INST";

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        break;
      case "INMS":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

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

        // ************************************************************
        // Get the address of the obligor CSE Person.
        // ************************************************************
        UseFnCabReadCsePersonAddress();

        if (!Equal(local.CsePersonAddress.Identifier, local.Blank.Timestamp))
        {
          // *************************************************************
          // The address was found for the Obligor CSE Person.
          // *************************************************************
          export.CsePersonAddress.Assign(local.CsePersonAddress);
          export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

          // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
          }
          else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
            IsEmpty(export.CsePersonAddress.Street1) || !
            IsEmpty(export.CsePersonAddress.Street3) || !
            IsEmpty(export.CsePersonAddress.State) || !
            IsEmpty(export.CsePersonAddress.Country))
          {
            MoveCsePersonAddress2(export.CsePersonAddress,
              local.DisplayAddressSave);
            export.CsePersonAddress.Assign(local.Initialized);
            MoveCsePersonAddress2(local.DisplayAddressSave,
              export.CsePersonAddress);
            export.CsePersonAddress.Street1 = "Security Block on Address";
          }

          local.AddressFound.Flag = "Y";
        }

        break;
      case "CADS":
        // **************************************************************
        // This will flow the user to the Case Details screen for the case
        // for the person who is listed on the screen.
        // **************************************************************
        if (ReadCase2())
        {
          export.Hcase.Number = entities.Case1.Number;
        }

        if (IsEmpty(export.Hcase.Number))
        {
          ExitState = "CASE_NF";
        }
        else
        {
          export.FlowFrom.Text4 = "CADS";
          ExitState = "ECO_LNK_TO_LST_CASE_DETAILS";
        }

        break;
      case "OPAY":
        export.ObligorCsePersonsWorkSet.Number = import.ObligorCsePerson.Number;
        ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

        break;
      case "OCTO":
        ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";

        break;
      case "OPSC":
        export.FlowFrom.Text4 = "OPSC";
        ExitState = "ECO_LNK_TO_LST_MTN_PYMNT_SCH";

        break;
      case "CSPM":
        local.PassedDueDate.Date = export.DebtDetail.DueDt;
        ExitState = "ECO_LNK_LST_MTN_OB_S_C_SUPP";

        break;
      case "RETURN":
        // =====================================
        // SRPT is the HIST screen.
        // SRPU is the MONA screen.
        // =====================================
        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          global.NextTran = (export.HiddenNextTranInfo.LastTran ?? "") + " " + "XXNEXTXX"
            ;
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
      default:
        // : If hidden command is CONFIRM, the user was asked to confirm
        //   an add action. Any key may be pressed to cancel the add.
        // =================================================
        // 12/31/98 - B Adams  -  This IF used to have a test for "OR
        //   export_confirm_retro = 'Y'" as part of it.  It was used to get
        //   the user to press PF5 again if the due date was prior to the
        //   current date.
        // =================================================
        if (AsChar(export.ConfirmObligAdd.Flag) == 'Y')
        {
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }

        break;
    }

Test10:

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

    export.HiddenObligor.Number = export.ObligorCsePerson.Number;
    export.HiddenAssign.UserId = export.AssignServiceProvider.UserId;

    // ================================================
    // 10/6/98  Bud Adams  -  After a Display action, a user can only
    // update Alternate_Billing_Location, Address data, or prompt for a new 
    // Assigned To person.  All other fields should be protected - except for
    // Obligor Number, since the user may wish to display recovery debts for
    // another person.
    // 1/21/99 B Adams  -  After an unsuccessful display, leave the
    //   fields unprotected
    // ================================================
    if (Equal(export.SetupDate.Date, local.Current.Date) || Equal
      (export.SetupDate.Date, local.Blank.Date) || !
      IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL") && Equal
      (global.Command, "DISPLAY"))
    {
    }
    else
    {
      var field1 = GetField(export.ObligationType, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligationTypePrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.ObligationTransaction, "amount");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.DebtDetail, "dueDt");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Obligation, "orderTypeCode");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.Obligation, "otherStateAbbr");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.InterstateRequest, "otherStateCaseId");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.InterstateRequest, "country");

      field8.Color = "cyan";
      field8.Protected = true;
    }

    if (IsExitState("ACO_NI0000_DISPLAY_SUCCESSFUL") || !
      IsEmpty(export.LegalAction.StandardNumber) || export
      .HiddenObligation.SystemGeneratedIdentifier > 0)
    {
      var field1 = GetField(export.ObligationType, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligationTypePrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.ObligationTransaction, "amount");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.DebtDetail, "dueDt");

      field4.Color = "cyan";
      field4.Protected = true;
    }

    if (!IsEmpty(export.ObligorCsePerson.Number) && (
      !IsEmpty(export.LegalAction.StandardNumber) || export
      .Obligation.SystemGeneratedIdentifier > 0))
    {
      // ----------------------------------------------------------------
      // Do not let the user,   update the AP#,  associated to a legal action, 
      // on OREC screen .
      // ---------------------------------------------------------------
      var field1 = GetField(export.ObligorCsePerson, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ObligorPrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;
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
      var field1 = GetField(export.Obligation, "orderTypeCode");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Obligation, "otherStateAbbr");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.InterstateRequest, "country");

      field4.Color = "cyan";
      field4.Protected = true;
    }

    // ************************************************************
    // Get the address of the obligor CSE Person.
    // ************************************************************
    UseFnCabReadCsePersonAddress();

    if (!Equal(local.CsePersonAddress.Identifier, local.Blank.Timestamp))
    {
      // *************************************************************
      // The address was found for the Obligor CSE Person.
      // *************************************************************
      export.CsePersonAddress.Assign(local.CsePersonAddress);
      export.HiddenCsePersonAddress.Assign(export.CsePersonAddress);

      // 09/22/2000 P. Phinney H00101308  Allow ONLY CRU to view address
      if (AsChar(export.DisplayAddress.Flag) == 'Y')
      {
      }
      else if (!IsEmpty(export.CsePersonAddress.LocationType) || !
        IsEmpty(export.CsePersonAddress.Street1) || !
        IsEmpty(export.CsePersonAddress.Street3) || !
        IsEmpty(export.CsePersonAddress.State) || !
        IsEmpty(export.CsePersonAddress.Country))
      {
        MoveCsePersonAddress2(export.CsePersonAddress, local.DisplayAddressSave);
          
        export.CsePersonAddress.Assign(local.Initialized);
        MoveCsePersonAddress2(local.DisplayAddressSave, export.CsePersonAddress);
          
        export.CsePersonAddress.Street1 = "Security Block on Address";
      }

      local.AddressFound.Flag = "Y";
    }

    // =================================================
    // 1/26/99 - b adams  -  If either No address was found, or the
    //   one found was a Tribunal address, then protect all the
    //   address fields.  We want no cold addresses entered and
    //   no changes made to Tribunal addresses from here.
    // =================================================
    if (AsChar(local.AddressFound.Flag) == 'N' || AsChar
      (local.AddressFound.Flag) == 'T' || export
      .Obligation.SystemGeneratedIdentifier == 0 && AsChar
      (local.AddressFound.Flag) == 'Y')
    {
      var field1 = GetField(export.CsePersonAddress, "type1");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.CsePersonAddress, "locationType");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.CsePersonAddress, "street1");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.CsePersonAddress, "street2");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.CsePersonAddress, "street3");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.CsePersonAddress, "street4");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.CsePersonAddress, "city");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.CsePersonAddress, "state");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.CsePersonAddress, "zipCode");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.CsePersonAddress, "zip3");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.CsePersonAddress, "zip4");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.CsePersonAddress, "province");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 = GetField(export.CsePersonAddress, "postalCode");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 = GetField(export.CsePersonAddress, "country");

      field14.Color = "cyan";
      field14.Protected = true;
    }

    if (AsChar(local.Failure.Flag) == 'U')
    {
      export.SetupDate.Date = Date(export.Obligation.CreatedTmst);
      export.LastUpdDate.Date = Date(export.Obligation.LastUpdateTmst);
      export.Previous.DueDt = export.DebtDetail.DueDt;
      export.HiddenObligor.Number = export.ObligorCsePerson.Number;
      export.HiddenObligationType.Code = export.ObligationType.Code;
      export.HiddenLegalAction.StandardNumber =
        export.LegalAction.StandardNumber;
      MoveObligation6(export.Obligation, export.HiddenObligation);
      export.HiddenObligationTransaction.Amount =
        export.ObligationTransaction.Amount;
      export.HiddenAlternateAddr.Number = export.AlternateAddr.Number;
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.VerifiedDate = source.VerifiedDate;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
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

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveInfrastructure3(Infrastructure source,
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
    target.KsCaseInd = source.KsCaseInd;
    target.Country = source.Country;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
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
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation6(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation7(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation8(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligation9(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.CreatedBy = source.CreatedBy;
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

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyCase = source.KeyCase;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdObligationType = source.IdObligationType;
    target.IdPersonAcct = source.IdPersonAcct;
    target.IdPersonAddress = source.IdPersonAddress;
    target.IdPrNumber = source.IdPrNumber;
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

    useImport.Code.CodeName = local.MatchCode.CodeName;
    useImport.CodeValue.Cdvalue = local.MatchCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidCode.Flag = useExport.ValidCode.Flag;
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

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Alternate.Number = export.AlternateAddr.Number;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private void UseFnCabCreateIntSuspStatHist()
  {
    var useImport = new FnCabCreateIntSuspStatHist.Import();
    var useExport = new FnCabCreateIntSuspStatHist.Export();

    useImport.HcOtcRecovery.Classification =
      local.HardcodeRecovery.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligationType1(export.ObligationType, useImport.ObligationType);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;

    Call(FnCabCreateIntSuspStatHist.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    MoveInfrastructure3(local.Infrastructure, useImport.Infrastructure);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCabReadCsePersonAddress()
  {
    var useImport = new FnCabReadCsePersonAddress.Import();
    var useExport = new FnCabReadCsePersonAddress.Export();

    useImport.AsOfDate.Date = local.Current.Date;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(FnCabReadCsePersonAddress.Execute, useImport, useExport);

    local.AddressFound.Flag = useExport.AddressFound.Flag;
    MoveCsePersonAddress1(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseFnCreateInterstateRqstOblign()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnCreateObligation()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.ObligationType.Assign(entities.ObligationType);
    useImport.HcOtCAccruingClassifi.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
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
    MoveObligation7(export.Obligation, useImport.Obligation);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MovePaymentRequest(export.HpaymentRequest, useImport.PaymentRequest);
    useImport.HardcodeCpaObligor.Type1 = local.HardcodeObligor.Type1;

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation2(useExport.Obligation, export.Obligation);
  }

  private void UseFnCreateObligationTransaction()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodeLapObligor.AccountType;
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
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodeRecovery.Classification;
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
    MoveLegalAction4(export.LegalAction, useImport.LegalAction);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.ObligationTransaction.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnDetermineCaseRoleForOrec()
  {
    var useImport = new FnDetermineCaseRoleForOrec.Import();
    var useExport = new FnDetermineCaseRoleForOrec.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnDetermineCaseRoleForOrec.Execute, useImport, useExport);

    local.CaseRole.Type1 = useExport.CaseRole.Type1;
  }

  private void UseFnDraFeeProcessRecoveryObg()
  {
    var useImport = new FnDraFeeProcessRecoveryObg.Import();
    var useExport = new FnDraFeeProcessRecoveryObg.Export();

    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.Ar.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnDraFeeProcessRecoveryObg.Execute, useImport, useExport);
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

    local.HardcodeLapObligor.AccountType = useExport.Obligor.AccountType;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObPrimSecCodPrimary.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
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
    local.HardcodeDdshActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeRecovery.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.HardcodeFees.Classification =
      useExport.OtCFeesClassification.Classification;
    local.HardcodeCpaObligee.Type1 = useExport.CpaObligee.Type1;
    local.HardcodeOtrnTDebtAdjustment.Type1 =
      useExport.OtrnTDebtAdjustment.Type1;
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
  }

  private void UseFnReadRecoveryObligation1()
  {
    var useImport = new FnReadRecoveryObligation.Import();
    var useExport = new FnReadRecoveryObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcOtcFee.Classification = local.HardcodeFees.Classification;
    useImport.Current.Date = local.Current.Date;
    MoveCsePerson(export.ObligorCsePerson, useImport.Obligor);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType4(export.ObligationType, useImport.ObligationType);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;
    useImport.HcOtrnTDebtAdjustment.Type1 =
      local.HardcodeOtrnTDebtAdjustment.Type1;

    Call(FnReadRecoveryObligation.Execute, useImport, useExport);

    local.DeactiveObligationExists.Flag =
      useExport.DeactiveObligtionExists.Flag;
    MoveCodeValue(useExport.Country, export.Country);
    export.AssignCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    export.AssignServiceProvider.UserId = useExport.ServiceProvider.UserId;
    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
    MoveCsePersonsWorkSet2(useExport.Alternate, export.AlternateAddr);
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    MoveObligation1(useExport.Obligation, export.Obligation);
    MoveObligationTransaction1(useExport.ObligationTransaction,
      export.ObligationTransaction);
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    MoveLegalAction2(useExport.OrderClassification, export.LegalAction);
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.PaymentScheduleInd.Flag = useExport.PaymentScheduleInd.Flag;
    export.SuspendInterestInd.Flag = useExport.InterestSuspensionInd.Flag;
    export.ObligationAmt.TotalCurrency = useExport.ObligationAmt.TotalCurrency;
    export.BalanceOwed.TotalCurrency = useExport.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = useExport.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = useExport.TotalOwed.TotalCurrency;
    MovePaymentRequest(useExport.PaymentRequest, export.HpaymentRequest);
    MoveInterstateRequest2(useExport.InterstateRequest, export.InterstateRequest);
      
    export.DebtDetailStatusHistory.EffectiveDt =
      useExport.DebtDetailStatusHistory.EffectiveDt;
    export.AdjustmentExists.Flag = useExport.AdjustmentExists.Flag;
    export.LegalActionDetail.Number = useExport.LegalActionDetail.Number;
  }

  private void UseFnReadRecoveryObligation2()
  {
    var useImport = new FnReadRecoveryObligation.Import();
    var useExport = new FnReadRecoveryObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcOtcFee.Classification = local.HardcodeFees.Classification;
    useImport.Current.Date = local.Current.Date;
    MoveCsePerson(export.ObligorCsePerson, useImport.Obligor);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType4(export.ObligationType, useImport.ObligationType);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtCAccruing.Classification =
      local.HardcodeOtCAccruingClassific.Classification;
    useImport.HcOtCVoluntary.Classification =
      local.HardcodeOtCVoluntaryClassifi.Classification;

    Call(FnReadRecoveryObligation.Execute, useImport, useExport);

    export.AssignCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
    export.AssignServiceProvider.UserId = useExport.ServiceProvider.UserId;
    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    MoveObligation1(useExport.Obligation, export.Obligation);
    MoveObligationTransaction1(useExport.ObligationTransaction,
      export.ObligationTransaction);
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    MoveLegalAction2(useExport.OrderClassification, export.LegalAction);
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.PaymentScheduleInd.Flag = useExport.PaymentScheduleInd.Flag;
    export.SuspendInterestInd.Flag = useExport.InterestSuspensionInd.Flag;
    export.ObligationAmt.TotalCurrency = useExport.ObligationAmt.TotalCurrency;
    export.BalanceOwed.TotalCurrency = useExport.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = useExport.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = useExport.TotalOwed.TotalCurrency;
    MoveInterstateRequest2(useExport.InterstateRequest, export.InterstateRequest);
      
  }

  private void UseFnRemoveObligation()
  {
    var useImport = new FnRemoveObligation.Import();
    var useExport = new FnRemoveObligation.Export();

    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtCRecoveryClassifi.Classification =
      local.HardcodeRecovery.Classification;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligation8(export.Obligation, useImport.Obligation);
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.Obligor.Number = import.ObligorCsePerson.Number;

    Call(FnRemoveObligation.Execute, useImport, useExport);
  }

  private void UseFnRetrieveInterstateRequest()
  {
    var useImport = new FnRetrieveInterstateRequest.Import();
    var useExport = new FnRetrieveInterstateRequest.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligation6(export.Obligation, useImport.Obligor);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveInterstateRequest1(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(FnRetrieveInterstateRequest.Execute, useImport, useExport);

    export.Country.Description = useExport.Country.Description;
    MoveInterstateRequest1(useExport.InterstateRequest, export.InterstateRequest);
      
  }

  private void UseFnRetrieveLeglForRecAndFee()
  {
    var useImport = new FnRetrieveLeglForRecAndFee.Import();
    var useExport = new FnRetrieveLeglForRecAndFee.Export();

    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeLapObligor.AccountType;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtRecoveryOrFee.Classification =
      local.HardcodeRecovery.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.HcCpaObligee.Type1 = local.HardcodeCpaObligee.Type1;

    Call(FnRetrieveLeglForRecAndFee.Execute, useImport, useExport);

    export.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
    export.AlternateAddr.Assign(useExport.Alternate);
    export.ObligationPaymentSchedule.
      Assign(useExport.ObligationPaymentSchedule);
    MoveLegalActionDetail2(useExport.LegalActionDetail, export.LegalActionDetail);
      
    MoveCsePerson(useExport.ObligorCsePerson, export.ObligorCsePerson);
    export.ObligorCsePersonsWorkSet.Assign(useExport.ObligorCsePersonsWorkSet);
    MoveObligation6(useExport.Obligation, export.Obligation);
    export.ObligationTransaction.Amount =
      useExport.ObligationTransaction.Amount;
    export.DebtDetail.DueDt = useExport.DebtDetail.DueDt;
    MoveObligationType2(useExport.ObligationType, export.ObligationType);
    MoveLegalAction3(useExport.LegalAction, export.LegalAction);
    export.CpaObligorOrObligee.Type1 = useExport.CpaObligorOrObligee.Type1;
    local.DebtExists.Flag = useExport.DebtExists.Flag;
  }

  private void UseFnUpdateAlternateAddress()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.AlternateBillingAddress.Number = export.AlternateAddr.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.Old.IntHGeneratedId =
      export.HiddenInterstateRequest.IntHGeneratedId;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.New1.IntHGeneratedId = export.InterstateRequest.IntHGeneratedId;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnUpdateRecoveryObligation()
  {
    var useImport = new FnUpdateRecoveryObligation.Import();
    var useExport = new FnUpdateRecoveryObligation.Export();

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligationTransaction1(export.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.DebtDetail.DueDt = export.DebtDetail.DueDt;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HardcodedObligor.Type1 = local.HardcodeObligor.Type1;
    MoveDateWorkArea(local.Current, useImport.Current);
    MoveObligation4(export.Obligation, useImport.Obligation);
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.UpdateBalanceDueAmt.Flag = local.UpdateBalanceDueAmt.Flag;

    Call(FnUpdateRecoveryObligation.Execute, useImport, useExport);
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
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

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
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

  private void UseScCabTestSecurity1()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity2()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePerson.Number = import.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.ObligorCsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiCreateCsePersonAddress()
  {
    var useImport = new SiCreateCsePersonAddress.Import();
    var useExport = new SiCreateCsePersonAddress.Export();

    MoveCsePersonAddress1(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;

    Call(SiCreateCsePersonAddress.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.AlternateAddr.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, export.AlternateAddr);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    export.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiUpdateCsePersonAddress()
  {
    var useImport = new SiUpdateCsePersonAddress.Import();
    var useExport = new SiUpdateCsePersonAddress.Export();

    useImport.CsePersonAddress.Assign(local.CsePersonAddress);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;

    Call(SiUpdateCsePersonAddress.Execute, useImport, useExport);
  }

  private void UseSpAddrExternalAlert()
  {
    var useImport = new SpAddrExternalAlert.Import();
    var useExport = new SpAddrExternalAlert.Export();

    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    MoveCsePersonAddress3(local.CsePersonAddress, useImport.CsePersonAddress);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;

    Call(SpAddrExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpCabCreateOblgAssignment()
  {
    var useImport = new SpCabCreateOblgAssignment.Import();
    var useExport = new SpCabCreateOblgAssignment.Export();

    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveOfficeServiceProvider(local.OblAssignmentOfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ObligationAssignment.Assign(local.PassToCreate);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ServiceProvider.SystemGeneratedId =
      local.OblAssignmentServiceProvider.SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      local.OblAssignmentOffice.SystemGeneratedId;

    Call(SpCabCreateOblgAssignment.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.PrintDocument.Name;

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);
  }

  private void UseSpDocCancelOutgoingDoc()
  {
    var useImport = new SpDocCancelOutgoingDoc.Import();
    var useExport = new SpDocCancelOutgoingDoc.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SpDocCancelOutgoingDoc.Execute, useImport, useExport);
  }

  private void UseSpDocFindOutgoingDocument()
  {
    var useImport = new SpDocFindOutgoingDocument.Import();
    var useExport = new SpDocFindOutgoingDocument.Export();

    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);
    useImport.Document.Name = local.PrintDocument.Name;

    Call(SpDocFindOutgoingDocument.Execute, useImport, useExport);

    local.OutgoingDocument.PrintSucessfulIndicator =
      useExport.OutgoingDocument.PrintSucessfulIndicator;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.PrintWorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);

    local.PrintWorkArea.Text50 = useExport.WorkArea.Text50;
  }

  private void DeleteObligationAssignment()
  {
    Update("DeleteObligationAssignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.ObligationAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ObligationAssignment.OffId);
        db.SetString(command, "ospCode", entities.ObligationAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.ObligationAssignment.OtyId);
        db.SetString(command, "cpaType", entities.ObligationAssignment.CpaType);
        db.SetString(command, "cspNo", entities.ObligationAssignment.CspNo);
        db.SetInt32(command, "obgId", entities.ObligationAssignment.ObgId);
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetString(command, "type", local.HardcodeObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCase3()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetString(command, "type", local.HardcodeObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNoAr", export.ObligorCsePerson.Number);
          
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
        db.SetString(command, "type", local.HardcodeObligor.Type1);
      },
      (db, reader) =>
      {
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 3);
        entities.CaseUnit.StartDate = db.GetDate(reader, 4);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 5);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 6);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
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
        entities.LegalAction.ForeignOrderNumber =
          db.GetNullableString(reader, 5);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPersonObligationTransactionObligation()
  {
    entities.ObligationTransaction.Populated = false;
    entities.ObligationType.Populated = false;
    entities.LegalActionPerson.Populated = false;
    entities.Obligation.Populated = false;

    return Read("ReadLegalActionPersonObligationTransactionObligation",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetInt32(command, "laDetailNo", export.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", export.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 0);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 1);
        entities.LegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.LegalActionPerson.LadRNumber = db.GetNullableInt32(reader, 3);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 5);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 6);
        entities.Obligation.CspNumber = db.GetString(reader, 6);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 7);
        entities.Obligation.CpaType = db.GetString(reader, 7);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 9);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 11);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 12);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 12);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 12);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 13);
        entities.Obligation.Description = db.GetNullableString(reader, 14);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 15);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 16);
        entities.Obligation.CreatedBy = db.GetString(reader, 17);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 18);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 20);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 21);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 22);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 23);
        entities.ObligationType.Code = db.GetString(reader, 24);
        entities.ObligationType.Name = db.GetString(reader, 25);
        entities.ObligationType.Classification = db.GetString(reader, 26);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 27);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 28);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 29);
        entities.ObligationTransaction.Populated = true;
        entities.ObligationType.Populated = true;
        entities.LegalActionPerson.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligCollProtectionHist()
  {
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgIdentifier",
          export.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyIdentifier",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetDate(
          command, "deactivationDate", local.Blank.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 1);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 2);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 4);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 5);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 6);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 7);
        entities.Obligation.CreatedBy = db.GetString(reader, 8);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 9);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 11);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 12);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 13);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 14);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationAssignment()
  {
    entities.ObligationAssignment.Populated = false;

    return Read("ReadObligationAssignment",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgId", export.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", export.ObligationType.Code);
        db.SetString(command, "cpaType", local.HardcodeObligor.Type1);
        db.SetString(command, "cspNo", export.ObligorCsePerson.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.ObligationAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ObligationAssignment.CreatedBy = db.GetString(reader, 2);
        entities.ObligationAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.ObligationAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.ObligationAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 5);
        entities.ObligationAssignment.SpdId = db.GetInt32(reader, 6);
        entities.ObligationAssignment.OffId = db.GetInt32(reader, 7);
        entities.ObligationAssignment.OspCode = db.GetString(reader, 8);
        entities.ObligationAssignment.OspDate = db.GetDate(reader, 9);
        entities.ObligationAssignment.OtyId = db.GetInt32(reader, 10);
        entities.ObligationAssignment.CpaType = db.GetString(reader, 11);
        entities.ObligationAssignment.CspNo = db.GetString(reader, 12);
        entities.ObligationAssignment.ObgId = db.GetInt32(reader, 13);
        entities.ObligationAssignment.Populated = true;
        CheckValid<ObligationAssignment>("CpaType",
          entities.ObligationAssignment.CpaType);
      });
  }

  private bool ReadObligationTransactionObligationDebtDetail()
  {
    entities.ObligationTransaction.Populated = false;
    entities.Obligation.Populated = false;
    entities.DebtDetail.Populated = false;

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
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 8);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 9);
        entities.Obligation.Description = db.GetNullableString(reader, 10);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 11);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 12);
        entities.Obligation.CreatedBy = db.GetString(reader, 13);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 16);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 17);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 18);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 19);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 20);
        entities.ObligationTransaction.Populated = true;
        entities.Obligation.Populated = true;
        entities.DebtDetail.Populated = true;
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

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
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

  private bool ReadObligationType2()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          export.ObligationType.SystemGeneratedIdentifier);
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

  private bool ReadOfficeServiceProviderServiceProviderOffice1()
  {
    entities.Office.Populated = false;
    entities.Existing.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "userId", local.Obligation.CreatedBy);
      },
      (db, reader) =>
      {
        entities.Existing.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.Existing.RoleCode = db.GetString(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.Office.EffectiveDate = db.GetDate(reader, 9);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 10);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 11);
        entities.Office.Populated = true;
        entities.Existing.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice2()
  {
    entities.Office.Populated = false;
    entities.Existing.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "userId", local.OblAssignmentServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.Existing.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Existing.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.Existing.RoleCode = db.GetString(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.LastName = db.GetString(reader, 6);
        entities.ServiceProvider.FirstName = db.GetString(reader, 7);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 8);
        entities.Office.EffectiveDate = db.GetDate(reader, 9);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 10);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 11);
        entities.Office.Populated = true;
        entities.Existing.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadProgramPersonProgram()
  {
    entities.PersonProgram.Populated = false;
    entities.Program.Populated = false;

    return Read("ReadProgramPersonProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.PersonProgram.CspNumber = db.GetString(reader, 2);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 3);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.PersonProgram.Populated = true;
        entities.Program.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          local.OblAssignmentServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateObligationAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationAssignment.Populated);

    var discontinueDate = AddDays(local.Current.Date, -1);
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = local.Current.Timestamp;

    entities.ObligationAssignment.Populated = false;
    Update("UpdateObligationAssignment",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ObligationAssignment.CreatedTimestamp.GetValueOrDefault());
        db.SetInt32(command, "spdId", entities.ObligationAssignment.SpdId);
        db.SetInt32(command, "offId", entities.ObligationAssignment.OffId);
        db.SetString(command, "ospCode", entities.ObligationAssignment.OspCode);
        db.SetDate(
          command, "ospDate",
          entities.ObligationAssignment.OspDate.GetValueOrDefault());
        db.SetInt32(command, "otyId", entities.ObligationAssignment.OtyId);
        db.SetString(command, "cpaType", entities.ObligationAssignment.CpaType);
        db.SetString(command, "cspNo", entities.ObligationAssignment.CspNo);
        db.SetInt32(command, "obgId", entities.ObligationAssignment.ObgId);
      });

    entities.ObligationAssignment.DiscontinueDate = discontinueDate;
    entities.ObligationAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationAssignment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ObligationAssignment.Populated = true;
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
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
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
    /// A value of HiddenAssign.
    /// </summary>
    [JsonPropertyName("hiddenAssign")]
    public ServiceProvider HiddenAssign
    {
      get => hiddenAssign ??= new();
      set => hiddenAssign = value;
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
    /// A value of HiddenObligationType.
    /// </summary>
    [JsonPropertyName("hiddenObligationType")]
    public ObligationType HiddenObligationType
    {
      get => hiddenObligationType ??= new();
      set => hiddenObligationType = value;
    }

    /// <summary>
    /// A value of LastWasAdd.
    /// </summary>
    [JsonPropertyName("lastWasAdd")]
    public Common LastWasAdd
    {
      get => lastWasAdd ??= new();
      set => lastWasAdd = value;
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
    /// A value of FlowFromWorkArea.
    /// </summary>
    [JsonPropertyName("flowFromWorkArea")]
    public WorkArea FlowFromWorkArea
    {
      get => flowFromWorkArea ??= new();
      set => flowFromWorkArea = value;
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
    /// A value of HiddenCsePersonAddress.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonAddress")]
    public CsePersonAddress HiddenCsePersonAddress
    {
      get => hiddenCsePersonAddress ??= new();
      set => hiddenCsePersonAddress = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of AssignCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("assignCsePersonsWorkSet")]
    public CsePersonsWorkSet AssignCsePersonsWorkSet
    {
      get => assignCsePersonsWorkSet ??= new();
      set => assignCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AssignServiceProvider.
    /// </summary>
    [JsonPropertyName("assignServiceProvider")]
    public ServiceProvider AssignServiceProvider
    {
      get => assignServiceProvider ??= new();
      set => assignServiceProvider = value;
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
    /// A value of FlowFromCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("flowFromCsePersonsWorkSet")]
    public CsePersonsWorkSet FlowFromCsePersonsWorkSet
    {
      get => flowFromCsePersonsWorkSet ??= new();
      set => flowFromCsePersonsWorkSet = value;
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
    /// A value of MustClearFirst.
    /// </summary>
    [JsonPropertyName("mustClearFirst")]
    public Common MustClearFirst
    {
      get => mustClearFirst ??= new();
      set => mustClearFirst = value;
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
    /// A value of HiddenObligationTransaction.
    /// </summary>
    [JsonPropertyName("hiddenObligationTransaction")]
    public ObligationTransaction HiddenObligationTransaction
    {
      get => hiddenObligationTransaction ??= new();
      set => hiddenObligationTransaction = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DebtDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of AdjustmentExists.
    /// </summary>
    [JsonPropertyName("adjustmentExists")]
    public Common AdjustmentExists
    {
      get => adjustmentExists ??= new();
      set => adjustmentExists = value;
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
    /// A value of H.
    /// </summary>
    [JsonPropertyName("h")]
    public PaymentRequest H
    {
      get => h ??= new();
      set => h = value;
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
    /// A value of CpaObligorOrObligee.
    /// </summary>
    [JsonPropertyName("cpaObligorOrObligee")]
    public CsePersonAccount CpaObligorOrObligee
    {
      get => cpaObligorOrObligee ??= new();
      set => cpaObligorOrObligee = value;
    }

    /// <summary>
    /// A value of DisplayAddress.
    /// </summary>
    [JsonPropertyName("displayAddress")]
    public Common DisplayAddress
    {
      get => displayAddress ??= new();
      set => displayAddress = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private Common countryPrompt;
    private CodeValue country;
    private ServiceProvider hiddenAssign;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationType hiddenObligationType;
    private Common lastWasAdd;
    private CsePersonsWorkSet hiddenAlternateAddr;
    private WorkArea flowFromWorkArea;
    private InterstateRequest hiddenInterstateRequest;
    private CsePersonAddress hiddenCsePersonAddress;
    private CsePersonAddress csePersonAddress;
    private Common assignPrompt;
    private CsePersonsWorkSet assignCsePersonsWorkSet;
    private ServiceProvider assignServiceProvider;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea lastUpdDate;
    private CsePersonsWorkSet flowFromCsePersonsWorkSet;
    private TextWorkArea altAddPrompt;
    private CsePersonsWorkSet alternateAddr;
    private Common mustClearFirst;
    private DateWorkArea setupDate;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction hiddenObligationTransaction;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private DebtDetail debtDetail;
    private DebtDetail previous;
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
    private LegalAction hiddenLegalAction;
    private Common last;
    private Common adjustmentExists;
    private Common confirmObligAdd;
    private Common confirmRetroDate;
    private NextTranInfo hiddenNextTranInfo;
    private PaymentRequest h;
    private InterstateRequest interstateRequest;
    private CsePersonAccount cpaObligorOrObligee;
    private Common displayAddress;
    private Common obCollProtAct;
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
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
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
    /// A value of HiddenAssign.
    /// </summary>
    [JsonPropertyName("hiddenAssign")]
    public ServiceProvider HiddenAssign
    {
      get => hiddenAssign ??= new();
      set => hiddenAssign = value;
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
    /// A value of LastWasAdd.
    /// </summary>
    [JsonPropertyName("lastWasAdd")]
    public Common LastWasAdd
    {
      get => lastWasAdd ??= new();
      set => lastWasAdd = value;
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
    /// A value of FlowFrom.
    /// </summary>
    [JsonPropertyName("flowFrom")]
    public WorkArea FlowFrom
    {
      get => flowFrom ??= new();
      set => flowFrom = value;
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
    /// A value of HiddenCsePersonAddress.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonAddress")]
    public CsePersonAddress HiddenCsePersonAddress
    {
      get => hiddenCsePersonAddress ??= new();
      set => hiddenCsePersonAddress = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of Hcase.
    /// </summary>
    [JsonPropertyName("hcase")]
    public Case1 Hcase
    {
      get => hcase ??= new();
      set => hcase = value;
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
    /// A value of AssignCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("assignCsePersonsWorkSet")]
    public CsePersonsWorkSet AssignCsePersonsWorkSet
    {
      get => assignCsePersonsWorkSet ??= new();
      set => assignCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AssignServiceProvider.
    /// </summary>
    [JsonPropertyName("assignServiceProvider")]
    public ServiceProvider AssignServiceProvider
    {
      get => assignServiceProvider ??= new();
      set => assignServiceProvider = value;
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
    /// A value of MustClearFirst.
    /// </summary>
    [JsonPropertyName("mustClearFirst")]
    public Common MustClearFirst
    {
      get => mustClearFirst ??= new();
      set => mustClearFirst = value;
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
    /// A value of HiddenObligationTransaction.
    /// </summary>
    [JsonPropertyName("hiddenObligationTransaction")]
    public ObligationTransaction HiddenObligationTransaction
    {
      get => hiddenObligationTransaction ??= new();
      set => hiddenObligationTransaction = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DebtDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of AdjustmentExists.
    /// </summary>
    [JsonPropertyName("adjustmentExists")]
    public Common AdjustmentExists
    {
      get => adjustmentExists ??= new();
      set => adjustmentExists = value;
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
    /// A value of PassedDueDate.
    /// </summary>
    [JsonPropertyName("passedDueDate")]
    public DateWorkArea PassedDueDate
    {
      get => passedDueDate ??= new();
      set => passedDueDate = value;
    }

    /// <summary>
    /// A value of HpaymentRequest.
    /// </summary>
    [JsonPropertyName("hpaymentRequest")]
    public PaymentRequest HpaymentRequest
    {
      get => hpaymentRequest ??= new();
      set => hpaymentRequest = value;
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
    /// A value of CpaObligorOrObligee.
    /// </summary>
    [JsonPropertyName("cpaObligorOrObligee")]
    public CsePersonAccount CpaObligorOrObligee
    {
      get => cpaObligorOrObligee ??= new();
      set => cpaObligorOrObligee = value;
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
    /// A value of DisplayAddress.
    /// </summary>
    [JsonPropertyName("displayAddress")]
    public Common DisplayAddress
    {
      get => displayAddress ??= new();
      set => displayAddress = value;
    }

    /// <summary>
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private Code code;
    private Common countryPrompt;
    private CodeValue country;
    private ServiceProvider hiddenAssign;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private Common lastWasAdd;
    private CsePersonsWorkSet hiddenAlternateAddr;
    private WorkArea flowFrom;
    private InterstateRequest hiddenInterstateRequest;
    private CsePersonAddress hiddenCsePersonAddress;
    private CsePersonAddress csePersonAddress;
    private Case1 hcase;
    private Common assignPrompt;
    private CsePersonsWorkSet assignCsePersonsWorkSet;
    private ServiceProvider assignServiceProvider;
    private FrequencyWorkSet frequencyWorkSet;
    private DateWorkArea lastUpdDate;
    private CsePersonAccount pass;
    private SpTextWorkArea object1;
    private TextWorkArea altAddPrompt;
    private CsePersonsWorkSet alternateAddr;
    private Common mustClearFirst;
    private DateWorkArea setupDate;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private LegalActionDetail legalActionDetail;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private Common obligorPrompt;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationTransaction hiddenObligationTransaction;
    private DebtDetail debtDetail;
    private DebtDetail previous;
    private ObligationType obligationType;
    private Common obligationTypePrompt;
    private LegalAction legalAction;
    private Common adjustmentExists;
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
    private DateWorkArea passedDueDate;
    private PaymentRequest hpaymentRequest;
    private InterstateRequest interstateRequest;
    private CsePersonAccount cpaObligorOrObligee;
    private CsePersonsWorkSet alternate;
    private Common displayAddress;
    private Common obCollProtAct;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
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
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of PrintBatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("printBatchConvertNumToText")]
    public BatchConvertNumToText PrintBatchConvertNumToText
    {
      get => printBatchConvertNumToText ??= new();
      set => printBatchConvertNumToText = value;
    }

    /// <summary>
    /// A value of PrintWorkArea.
    /// </summary>
    [JsonPropertyName("printWorkArea")]
    public WorkArea PrintWorkArea
    {
      get => printWorkArea ??= new();
      set => printWorkArea = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
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
    /// A value of UpdateBalanceDueAmt.
    /// </summary>
    [JsonPropertyName("updateBalanceDueAmt")]
    public Common UpdateBalanceDueAmt
    {
      get => updateBalanceDueAmt ??= new();
      set => updateBalanceDueAmt = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Failure.
    /// </summary>
    [JsonPropertyName("failure")]
    public Common Failure
    {
      get => failure ??= new();
      set => failure = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of HardcodeObPrimSecCodPrimary.
    /// </summary>
    [JsonPropertyName("hardcodeObPrimSecCodPrimary")]
    public Obligation HardcodeObPrimSecCodPrimary
    {
      get => hardcodeObPrimSecCodPrimary ??= new();
      set => hardcodeObPrimSecCodPrimary = value;
    }

    /// <summary>
    /// A value of HardcodeLapObligor.
    /// </summary>
    [JsonPropertyName("hardcodeLapObligor")]
    public LegalActionPerson HardcodeLapObligor
    {
      get => hardcodeLapObligor ??= new();
      set => hardcodeLapObligor = value;
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
    /// A value of HardcodeDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDdshActiveStatus")]
    public DebtDetailStatusHistory HardcodeDdshActiveStatus
    {
      get => hardcodeDdshActiveStatus ??= new();
      set => hardcodeDdshActiveStatus = value;
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
    /// A value of DebtExists.
    /// </summary>
    [JsonPropertyName("debtExists")]
    public Common DebtExists
    {
      get => debtExists ??= new();
      set => debtExists = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public CsePersonAddress Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of ActiveOblgIndicator.
    /// </summary>
    [JsonPropertyName("activeOblgIndicator")]
    public Common ActiveOblgIndicator
    {
      get => activeOblgIndicator ??= new();
      set => activeOblgIndicator = value;
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
    /// A value of OblAssignmentOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oblAssignmentOfficeServiceProvider")]
    public OfficeServiceProvider OblAssignmentOfficeServiceProvider
    {
      get => oblAssignmentOfficeServiceProvider ??= new();
      set => oblAssignmentOfficeServiceProvider = value;
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
    /// A value of AddressFound.
    /// </summary>
    [JsonPropertyName("addressFound")]
    public Common AddressFound
    {
      get => addressFound ??= new();
      set => addressFound = value;
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
    /// A value of LegalIdPassed.
    /// </summary>
    [JsonPropertyName("legalIdPassed")]
    public Common LegalIdPassed
    {
      get => legalIdPassed ??= new();
      set => legalIdPassed = value;
    }

    /// <summary>
    /// A value of PrintDocument.
    /// </summary>
    [JsonPropertyName("printDocument")]
    public Document PrintDocument
    {
      get => printDocument ??= new();
      set => printDocument = value;
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
    /// A value of HardcodeFees.
    /// </summary>
    [JsonPropertyName("hardcodeFees")]
    public ObligationType HardcodeFees
    {
      get => hardcodeFees ??= new();
      set => hardcodeFees = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of HardcodeCpaObligee.
    /// </summary>
    [JsonPropertyName("hardcodeCpaObligee")]
    public CsePersonAccount HardcodeCpaObligee
    {
      get => hardcodeCpaObligee ??= new();
      set => hardcodeCpaObligee = value;
    }

    /// <summary>
    /// A value of AddrError.
    /// </summary>
    [JsonPropertyName("addrError")]
    public Common AddrError
    {
      get => addrError ??= new();
      set => addrError = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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
    /// A value of DeactiveObligationExists.
    /// </summary>
    [JsonPropertyName("deactiveObligationExists")]
    public Common DeactiveObligationExists
    {
      get => deactiveObligationExists ??= new();
      set => deactiveObligationExists = value;
    }

    /// <summary>
    /// A value of CursorLocation.
    /// </summary>
    [JsonPropertyName("cursorLocation")]
    public Common CursorLocation
    {
      get => cursorLocation ??= new();
      set => cursorLocation = value;
    }

    /// <summary>
    /// A value of SaveCommand.
    /// </summary>
    [JsonPropertyName("saveCommand")]
    public Standard SaveCommand
    {
      get => saveCommand ??= new();
      set => saveCommand = value;
    }

    /// <summary>
    /// A value of DisplayAddressSave.
    /// </summary>
    [JsonPropertyName("displayAddressSave")]
    public CsePersonAddress DisplayAddressSave
    {
      get => displayAddressSave ??= new();
      set => displayAddressSave = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public NextTranInfo Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      cursorPosition = null;
      position = null;
      printBatchConvertNumToText = null;
      printWorkArea = null;
      printProcess = null;
      spDocLiteral = null;
      hardcodeOtrnTDebtAdjustment = null;
      updateBalanceDueAmt = null;
      interfaceAlert = null;
      program = null;
      outgoingDocument = null;
      failure = null;
      spDocKey = null;
      caseRole = null;
      hardcodeObPrimSecCodPrimary = null;
      hardcodeLapObligor = null;
      debtExists = null;
      initialized = null;
      activeOblgIndicator = null;
      oblAssignmentServiceProvider = null;
      oblAssignmentOffice = null;
      oblAssignmentOfficeServiceProvider = null;
      passToCreate = null;
      addressFound = null;
      validCode = null;
      matchCode = null;
      matchCodeValue = null;
      noOfPromptsSelected = null;
      passedDueDate = null;
      forLeftPad = null;
      legalIdPassed = null;
      printDocument = null;
      obligation = null;
      eab = null;
      blank = null;
      infrastructure = null;
      fromHistMonaNxttran = null;
      csePersonAddress = null;
      hardcodeCpaObligee = null;
      addrError = null;
      batchTimestampWorkArea = null;
      csePersonsWorkSet = null;
      deactiveObligationExists = null;
      cursorLocation = null;
      saveCommand = null;
      displayAddressSave = null;
      zdel = null;
    }

    private CursorPosition cursorPosition;
    private Common position;
    private BatchConvertNumToText printBatchConvertNumToText;
    private WorkArea printWorkArea;
    private Common printProcess;
    private SpDocLiteral spDocLiteral;
    private ObligationTransaction hardcodeOtrnTDebtAdjustment;
    private Common updateBalanceDueAmt;
    private InterfaceAlert interfaceAlert;
    private Program program;
    private OutgoingDocument outgoingDocument;
    private Common failure;
    private SpDocKey spDocKey;
    private CaseRole caseRole;
    private Obligation hardcodeObPrimSecCodPrimary;
    private LegalActionPerson hardcodeLapObligor;
    private ObligationType hardcodeOt718BUraJudgement;
    private CsePersonAccount hardcodeCpaSupportedPerson;
    private ObligationTransaction hardcodeOtrnDtVoluntary;
    private ObligationTransaction hardcodeOtrnDtAccrualInstruc;
    private ObligationType hardcodeOtCAccruingClassific;
    private ObligationType hardcodeOtCVoluntaryClassifi;
    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private DebtDetailStatusHistory hardcodeDdshActiveStatus;
    private ObligationTransactionRlnRsn hardcodeOtrrConcurrentObliga;
    private Common debtExists;
    private CsePersonAddress initialized;
    private Common activeOblgIndicator;
    private ServiceProvider oblAssignmentServiceProvider;
    private Office oblAssignmentOffice;
    private OfficeServiceProvider oblAssignmentOfficeServiceProvider;
    private ObligationAssignment passToCreate;
    private Common addressFound;
    private Common validCode;
    private Code matchCode;
    private CodeValue matchCodeValue;
    private Common noOfPromptsSelected;
    private DateWorkArea passedDueDate;
    private TextWorkArea forLeftPad;
    private Common legalIdPassed;
    private Document printDocument;
    private Obligation obligation;
    private CsePersonAccount hardcodeObligor;
    private ObligationType hardcodeRecovery;
    private ObligationType hardcodeFees;
    private ObligationTransaction hardcodeDebt;
    private AbendData eab;
    private DateWorkArea blank;
    private DateWorkArea max;
    private Infrastructure infrastructure;
    private Common fromHistMonaNxttran;
    private DateWorkArea current;
    private CsePersonAddress csePersonAddress;
    private CsePersonAccount hardcodeCpaObligee;
    private Common addrError;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common deactiveObligationExists;
    private Common cursorLocation;
    private Standard saveCommand;
    private CsePersonAddress displayAddressSave;
    private NextTranInfo zdel;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
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
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public OfficeServiceProvider Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of AltAddr.
    /// </summary>
    [JsonPropertyName("altAddr")]
    public CsePerson AltAddr
    {
      get => altAddr ??= new();
      set => altAddr = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligorCsePersonAccount")]
    public CsePersonAccount ObligorCsePersonAccount
    {
      get => obligorCsePersonAccount ??= new();
      set => obligorCsePersonAccount = value;
    }

    private PersonProgram personProgram;
    private Program program;
    private CaseUnit caseUnit;
    private Tribunal tribunal;
    private InterstatePaymentAddress interstatePaymentAddress;
    private Office office;
    private OfficeServiceProvider existing;
    private CsePerson altAddr;
    private ObligationTransaction obligationTransaction;
    private CsePerson obligorCsePerson;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private LegalActionPerson legalActionPerson;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private CaseRole caseRole;
    private Fips fips;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
    private ObligationAssignment obligationAssignment;
    private DebtDetail debtDetail;
    private ObligCollProtectionHist obligCollProtectionHist;
    private CsePersonAccount obligorCsePersonAccount;
  }
#endregion
}

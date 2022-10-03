// Program: FN_DBWR_RECORD_ACCRD_ARREAGE_ADJ, ID: 372257151, model: 746.
// Short name: SWEDBWRP
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
/// A program: FN_DBWR_RECORD_ACCRD_ARREAGE_ADJ.
/// </para>
/// <para>
/// This screen provides a means to adjust the amount due on a set range of 
/// accrued debts for an accruing obligation.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDbwrRecordAccrdArreageAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DBWR_RECORD_ACCRD_ARREAGE_ADJ program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDbwrRecordAccrdArreageAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDbwrRecordAccrdArreageAdj.
  /// </summary>
  public FnDbwrRecordAccrdArreageAdj(IContext context, Import import,
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
    // ***********************************************
    // 12/27/96	R. Marchman	Add new security/next tran.
    // 12/27/96	R. Marchman	Add links from OPAY and OCTO.
    // 05/01/97	A.Kinney	Changed Current_Date
    // 05/12/97        S.Mahapatra     Modified screen to show 6 detail
    //        lines. Added new field accrual amt.
    // 02/13/98	govind		Fixed to get the SSN and pass it to the subseq acblks.
    // 03/08/99	E. Parker		Added logic to support 'Reverse Associated 
    // Collections' Indicator; only back off enough Collections to prevent bal
    // from becoming neg.; made debt detail stat his id consistent with rest of
    // system using random 3 digit num; allow adjustment of only one child even
    // when multiple exist and do validation by child; added info message if
    // manual dist colls are backed off; allow start and end date same; allow
    // positive adjs more than accrual amount;  take positive debt adjs into
    // consideration in calcs.
    // 3/19/1999 - B Adams  -  Read properties set
    // 3/23/1999 - b adams  -  Joint and Several logic added so that if one of a
    // J&S pair is adjusted, then the other one is also adjusted by the same
    // amount and in the same manner.
    // 08/11/99 - Maureen Brown - fixed problem where the program gave an edit 
    // error if the entire amt of the debt was being adjusted.  This was
    // happening in the Q region, but not in T.  We surmise that the database
    // properties are different in Q.  See 'mfb' to locate the fix.
    // 1/4/00 - Bud Adams  -  PR# 82453: required the 'reverse_
    //   collections_ind' be stored in the database for redisplay on
    //   the DBAJ screen.   Adapted this screen to use that new
    //   attribute and get rid of the non-database 'flag' for clarity &
    //   consistency.
    // **********************************************
    // *******************************************************************
    // April, 2000 - M Brown - Major changes to adjustment processing:
    //  - Reverse collections indicator removed from screen and logic.
    //  - Manual collections are never backed off.
    //  - If a decrease adjustment is done that requires a collection to be 
    // backed off,
    //    all collections for the debt are backed off.
    //  - If a decrease adjustment is done for a primary/secondary requiring a 
    // collection
    //    to be backed off, the 'other' obligations collections that came from 
    // the same
    //    CRD as the first ob are also backed off.
    //  - Writeoff function was added.  This will create an adjustment bringing 
    // the debt
    //    detail balance to zero.   It will also set collections to manual, and 
    // the debt
    //    adjustment process date to current date.
    //  - Reason code WO NA will write off NA debts only; reason code WO ALL
    //    will write off all debts for the selected child and timeframe.
    //  - Reinstate function was added.  The reason code is 'REINST'.
    //    It brings balance back to pre-writeoff balance by creating an 
    // offsetting adjustment.
    //  - A write-off for a debt can only be done once a day.
    // *******************************************************************
    // *******************************************************************
    // January, 2002 - M Brown - Work Order Number: 010504 - Retro Processing.
    //  - Do not reverse collections that are protected against retro 
    // processing.
    // January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
    //  - New value of 'C' for collection distribution method ind, to indicate 
    // closed
    //    case processing.  This is handled exactly the same way as writeoff.
    // *******************************************************************
    // PR# 128572, M. Brown, Jan, 2002: If someone was trying to do a writeoff, 
    // but entered the wrong code, fields not required for a writeoff were being
    // highlighted as errors, instead of the reason code.  Put the reason code
    // edit before those other edits.
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    ExitState = "ACO_NN0000_ALL_OK";
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    export.ApPayorCsePerson.Assign(import.ApPayorCsePerson);
    export.ApPayorCsePersonsWorkSet.Assign(import.ApPayorCsePersonsWorkSet);
    MoveObligation(import.Obligation, export.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.DebtObligationTransaction.Assign(import.DebtObligationTransaction);
    export.ObligationTransactionRln.Description =
      import.ObligationTransactionRln.Description;
    MoveObligationTransaction1(import.DebtAdjustment, export.DebtAdjustment);
    MoveObligationTransactionRlnRsn(import.ObligationTransactionRlnRsn,
      export.ObligationTransactionRlnRsn);
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.DebtAccrualInstructions.Assign(import.DebtAccrualInstructions);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.DebtAdjustmentInd.SelectChar = import.DebtAdjustmentInd.SelectChar;
    export.LocalAdjust.Date = import.LocalAdjust.Date;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.DebtAdjustmentFromDate.Date = import.DebtAdjustmentFromDate.Date;
    export.ConfirmWriteoff.Flag = import.ConfirmWriteoff.Flag;
    export.Hidden.Assign(import.Hidden);
    export.DebtAdjustmentToDate.Date = import.DebtAdjustmentToDate.Date;
    export.CollProtectionExists.Flag = import.CollProtectionExists.Flag;
    export.ConfirmWriteoff.Flag = "N";
    UseFnHardcodedDebtDistribution();
    local.HardcodeWriteoffNa.SystemGeneratedIdentifier = 6;
    local.HardcodeReinstate.SystemGeneratedIdentifier = 7;
    local.HardcodeWriteoffAll.SystemGeneratedIdentifier = 8;
    local.HardcodeCaseClosed.SystemGeneratedIdentifier = 9;
    local.HardcodedWriteoffAf.SystemGeneratedIdentifier = 10;
    local.HardcodeIncAdj.SystemGeneratedIdentifier = 11;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      MoveCsePersonsWorkSet2(import.Group.Item.Supported,
        export.Group.Update.Supported);
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      export.Group.Update.AccrualInstructions.Assign(
        import.Group.Item.AccrualInstructions);
      MoveObligationTransaction2(import.Group.Item.DebtAdjustment,
        export.Group.Update.DebtAdjustment);
      export.Group.Update.Debt.Amount = import.Group.Item.Debt.Amount;
      export.Group.Next();
    }

    if (Equal(global.Command, "CLEAR"))
    {
      export.DebtAdjustmentFromDate.Date = local.Initialised.Date;
      export.DebtAdjustmentToDate.Date = local.Initialised.Date;
      export.ObligationTransactionRln.Description =
        Spaces(ObligationTransactionRln.Description_MaxLength);
      export.ObligationTransactionRlnRsn.Code = "";
      export.Prompt.SelectChar = "+";
      export.DebtAdjustmentInd.SelectChar = "";
      export.DebtAdjustment.ReverseCollectionsInd = "";

      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        export.Group.Update.Common.SelectChar = "";
        export.Group.Update.DebtAdjustment.Amount = 0;
      }

      export.DebtAdjustment.CreatedTmst = local.Initialised.Timestamp;
      export.DebtAdjustment.CreatedBy = "";

      return;
    }

    if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Error = true;
    }

    if (Equal(global.Command, "RETDBWR"))
    {
      var field = GetField(export.Prompt, "selectChar");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;

      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      if (!IsEmpty(export.ApPayorCsePerson.Number))
      {
        export.Hidden.CsePersonNumber = export.ApPayorCsePersonsWorkSet.Number;
        export.Hidden.CsePersonNumberObligor =
          export.ApPayorCsePersonsWorkSet.Number;
        export.Hidden.CsePersonNumberAp =
          export.ApPayorCsePersonsWorkSet.Number;
        export.Hidden.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        export.Hidden.StandardCrtOrdNumber =
          export.LegalAction.StandardNumber ?? "";
        export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
        export.Hidden.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        export.Hidden.MiscNum1 =
          export.ObligationType.SystemGeneratedIdentifier;
      }

      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      export.ApPayorCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);
      export.LegalAction.Identifier =
        export.Hidden.LegalActionIdentifier.GetValueOrDefault();
      export.ObligationType.SystemGeneratedIdentifier =
        (int)export.Hidden.MiscNum1.GetValueOrDefault();

      if (IsEmpty(export.ApPayorCsePerson.Number) || export
        .Obligation.SystemGeneratedIdentifier == 0 || export
        .LegalAction.Identifier == 0)
      {
        ExitState = "FN0000_INSUFF_NEXXTRAN_INFO";

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (Equal(global.Command, "DEBT") || Equal(global.Command, "COLP"))
    {
    }
    else
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      MoveCsePersonsWorkSet2(import.Group.Item.Supported,
        export.Group.Update.Supported);
      export.Group.Update.Case1.Number = import.Group.Item.Case1.Number;
      MoveObligationTransaction2(import.Group.Item.DebtAdjustment,
        export.Group.Update.DebtAdjustment);
      export.Group.Update.Hidden.SystemGeneratedIdentifier =
        import.Group.Item.Hidden.SystemGeneratedIdentifier;
      export.Group.Update.Debt.Amount = import.Group.Item.Debt.Amount;

      switch(AsChar(import.Group.Item.Common.SelectChar))
      {
        case 'S':
          ++local.SelectionCounter.Count;

          break;
        case ' ':
          break;
        default:
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Group.Item.Common, "selectChar");

        field.Error = true;

        export.Group.Next();

        return;
      }

      export.Group.Next();
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // **** Get information for screen display
        // **** Get AP/Payor information
        if (ReadCsePersonObligor())
        {
          switch(AsChar(entities.Obligor1.Type1))
          {
            case 'C':
              // **** Client
              UseSiReadCsePerson2();
              UseSiFormatCsePersonName();

              break;
            case 'O':
              // **** Organization
              export.ApPayorCsePersonsWorkSet.Number = entities.Obligor1.Number;
              export.ApPayorCsePersonsWorkSet.FormattedName =
                entities.Obligor1.OrganizationName ?? Spaces(33);

              break;
            default:
              break;
          }
        }
        else
        {
          var field = GetField(export.ApPayorCsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        // **** Get obligation type
        if (ReadObligationType())
        {
          export.ObligationType.Assign(entities.ObligationType);
        }
        else
        {
          ExitState = "FN0000_OBLIG_TYPE_NF";

          return;
        }

        if (ReadObligation4())
        {
          MoveObligation(entities.Obligation, export.Obligation);
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          return;
        }

        if (ReadObligationPaymentSchedule())
        {
          export.ObligationPaymentSchedule.Assign(
            entities.ObligationPaymentSchedule);

          if (Equal(entities.ObligationPaymentSchedule.EndDt, local.Maximum.Date))
            
          {
            export.ObligationPaymentSchedule.EndDt = local.Initialised.Date;
          }
        }
        else
        {
          ExitState = "FN0000_PAYMENT_SCHEDULE_NF";

          return;
        }

        // : January, 2002 - M Brown - Work Order Number: 010504 - Retro 
        // Processing.
        if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
        {
          if (ReadObligCollProtectionHist1())
          {
            export.CollProtectionExists.Flag = "Y";
          }
          else
          {
            export.CollProtectionExists.Flag = "N";
          }
        }
        else if (ReadObligCollProtectionHist2())
        {
          export.CollProtectionExists.Flag = "Y";
        }
        else
        {
          export.CollProtectionExists.Flag = "N";
        }

        // **** Get court order
        if (ReadLegalAction())
        {
          export.LegalAction.StandardNumber =
            entities.LegalAction.StandardNumber;
        }
        else
        {
          // **** optional relationship
        }

        // ***  Duplicate read of  obligation-payment-schedule  deleted.  ***
        // ***		Bud - 9/2/98		***
        local.Date.Date = local.Current.Date;

        // **** Calculate current owed, arrears owed, interest owed, and total 
        // owed.
        // =================================================
        // 5/26/99 - bud adams  -  use zd_fn_calc_amt_owed_for_oblig
        //   replaced with this cab.
        // =================================================
        UseFnComputeSummaryTotals();

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          var field =
            GetField(export.ScreenOwedAmounts, "errorInformationLine");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // : Get latest adjustment, if there is one.
        if (ReadDebtAdjustment1())
        {
          export.LocalAdjust.Date = Date(entities.DebtAdjustment.CreatedTmst);
          export.DebtAdjustment.LastUpdatedBy =
            entities.DebtAdjustment.CreatedBy;
        }

        // **** Each supported person has a distinct accrual instruction.
        export.DebtAdjustment.Amount = 0;
        export.DebtObligationTransaction.Amount = 0;
        local.ReadEachFound.Flag = "N";

        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadAccrualInstructionsDebt())
        {
          // =================================================
          // 10/26/99 - bud adams  -  PR# 77879: Last Accrual Date was
          //   being displayed at the obligation level instead of at the
          //   ob_tran (supported person) level, which is where it really
          //   applies.  It can vary from person to person, if Accrual_
          //   Suspensions are in effect.
          // =================================================
          local.ReadEachFound.Flag = "Y";
          export.Group.Update.AccrualInstructions.Assign(
            entities.AccrualInstructions);

          // .............. Get discontinue date at support person level.
          if (Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
            local.Maximum.Date))
          {
            export.Group.Update.AccrualInstructions.DiscontinueDt =
              local.Initialised.Date;
          }

          export.Group.Update.Debt.Amount = entities.Distinct.Amount;

          // ..............Accumulate Obligation_Transaction level Accrual 
          // Amount
          export.DebtObligationTransaction.Amount += entities.Distinct.Amount;

          // ............ Get supported person information
          if (ReadCsePerson())
          {
            export.Group.Update.Supported.Number =
              entities.SupportedCsePerson.Number;

            switch(AsChar(entities.Obligor1.Type1))
            {
              case 'C':
                // **** Client
                UseSiReadCsePerson3();

                break;
              case 'O':
                // **** Organization
                export.Group.Update.Supported.Number =
                  export.ApPayorCsePersonsWorkSet.Number;
                export.Group.Update.Supported.FormattedName =
                  entities.Obligor1.OrganizationName ?? Spaces(33);

                break;
              default:
                break;
            }
          }
          else
          {
            ExitState = "FN0000_SUPP_PERSON_NF";
            export.Group.Next();

            return;
          }

          if (ReadCase())
          {
            export.Group.Update.Case1.Number = entities.Case1.Number;
          }
          else
          {
            // **** Optional relationship
          }

          export.Group.Next();
        }

        if (Equal(export.ObligationPaymentSchedule.EndDt, local.Maximum.Date))
        {
          export.ObligationPaymentSchedule.EndDt = local.Initialised.Date;
        }

        if (AsChar(local.ReadEachFound.Flag) == 'N')
        {
          ExitState = "OBLIGATION_TRANSACTION_NF";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        if (AsChar(import.Prompt.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_LST_OBLIG_TRNS_RLN_RS";
        }
        else
        {
          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "ADD":
        // =================================================
        // 11/19/98 - B Adams  -  Check all required data at the same
        //   time and highlight all missing data.
        // =================================================
        // ..........Adjustment reason blank
        if (IsEmpty(export.ObligationTransactionRlnRsn.Code))
        {
          var field = GetField(export.ObligationTransactionRlnRsn, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        // Jan, 2002, M. Brown, PR# 128572: Moved this edit from below the edits
        // following.
        // When the user entered 'WR ALL' instead of 'WO ALL', unnecessary edits
        // were being done.
        // .............Invalid reason
        if (ReadObligationTransactionRlnRsn())
        {
          MoveObligationTransactionRlnRsn(entities.ObligationTransactionRlnRsn,
            export.ObligationTransactionRlnRsn);

          // : Check to see if 'CONCUR' reason code used.  This is an invalid 
          // code
          //   for adjustments - mfb May, 2000.
          if (export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
            .HardcodeConcurrent.SystemGeneratedIdentifier)
          {
            var field = GetField(export.ObligationTransactionRlnRsn, "code");

            field.Error = true;

            ExitState = "FN0000_OB_TRN_RLN_RSN_INVALID";

            return;
          }
        }
        else
        {
          var field = GetField(export.ObligationTransactionRlnRsn, "code");

          field.Error = true;

          ExitState = "FN0000_ADJUST_REASON_NF";

          return;
        }

        // ..............Description blank
        if (IsEmpty(export.ObligationTransactionRln.Description))
        {
          var field = GetField(export.ObligationTransactionRln, "description");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        // : Set a flag to indicate if the debt adjustment is a writeoff or 
        // reinstate.
        //  This is simply to make the code more readable.
        // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
        // - added 'closeca' to the if statement for writeoff.
        if (Equal(export.ObligationTransactionRlnRsn.Code, "WO NA") || Equal
          (export.ObligationTransactionRlnRsn.Code, "WO ALL") || Equal
          (export.ObligationTransactionRlnRsn.Code, "CLOSECA") || Equal
          (export.ObligationTransactionRlnRsn.Code, "WO AF"))
        {
          local.WriteoffOrReinstate.Flag = "W";
        }
        else if (Equal(export.ObligationTransactionRlnRsn.Code, "REINST"))
        {
          local.WriteoffOrReinstate.Flag = "R";
        }
        else
        {
          local.WriteoffOrReinstate.Flag = "";
        }

        if (IsEmpty(local.WriteoffOrReinstate.Flag))
        {
          // .............From date blank
          if (Equal(export.DebtAdjustmentFromDate.Date, local.Initialised.Date))
          {
            ExitState = "FN0000_ADJUSTMENT_DATE_BLANK";

            var field = GetField(export.DebtAdjustmentFromDate, "date");

            field.Error = true;
          }

          // ...........To date blank
          if (Equal(export.DebtAdjustmentToDate.Date, local.Initialised.Date))
          {
            ExitState = "FN0000_ADJUSTMENT_DATE_BLANK";

            var field = GetField(export.DebtAdjustmentToDate, "date");

            field.Error = true;
          }

          // ..........Indicator blank
          if (IsEmpty(export.DebtAdjustmentInd.SelectChar))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field = GetField(export.DebtAdjustmentInd, "selectChar");

            field.Error = true;
          }
        }
        else
        {
          // : If writeoff or reinstate, we don't need to do the edits above, 
          // but we
          //   need to set the indicator to '+' for reinstate, and '-' for 
          // writeoff.
          if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
          {
            // : Function is writeoff
            export.DebtAdjustmentInd.SelectChar = "-";
          }
          else
          {
            // : Function is reinstate.
            export.DebtAdjustmentInd.SelectChar = "+";

            // M. Brown, Oct, 2000, wo # 197 - Put out a message if a date range
            //  was entered on reinstate.
            if (Equal(export.DebtAdjustmentFromDate.Date, local.Initialised.Date)
              && Equal
              (export.DebtAdjustmentToDate.Date, local.Initialised.Date))
            {
            }
            else
            {
              ExitState = "FN_DATE_RANGE_IGNORED_REINSTATE";
              export.DebtAdjustmentFromDate.Date = local.Initialised.Date;
              export.DebtAdjustmentToDate.Date = local.Initialised.Date;

              return;
            }
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // PR# 128572, M. Brown, Jan, 2002: There was a read of ob trn rln 
        // reason here.
        // Moved it to before the above edits.
        if (local.SelectionCounter.Count == 0)
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_NO_SELECTION_MADE";

            return;
          }
        }

        if (ReadObligation3())
        {
          MoveObligation(entities.Obligation, export.Obligation);
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          return;
        }

        if (IsEmpty(local.WriteoffOrReinstate.Flag))
        {
          // : These edits don't need to be done for writeoff or reinstate 
          // functions.
          // **** Make sure to date is less than from date
          if (Lt(export.DebtAdjustmentToDate.Date,
            export.DebtAdjustmentFromDate.Date))
          {
            var field1 = GetField(export.DebtAdjustmentToDate, "date");

            field1.Error = true;

            var field2 = GetField(export.DebtAdjustmentFromDate, "date");

            field2.Error = true;

            ExitState = "ACO_NE0000_END_LESS_THAN_START";

            return;
          }

          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              if (export.Group.Item.DebtAdjustment.Amount == 0)
              {
                var field =
                  GetField(export.Group.Item.DebtAdjustment, "amount");

                field.Error = true;

                ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

                return;
              }

              // .........From Date >= Discontinue Date
              if (!Equal(export.Group.Item.AccrualInstructions.DiscontinueDt,
                local.Initialised.Date))
              {
                if (Lt(export.Group.Item.AccrualInstructions.DiscontinueDt,
                  export.DebtAdjustmentFromDate.Date))
                {
                  var field = GetField(export.DebtAdjustmentFromDate, "date");

                  field.Error = true;

                  ExitState = "FN0000_ADJUSTMENT_DATE_ERROR";

                  return;
                }
              }

              // ................From Date < Accrual Start Date
              if (Lt(export.DebtAdjustmentFromDate.Date,
                export.Group.Item.AccrualInstructions.AsOfDt))
              {
                var field = GetField(export.DebtAdjustmentFromDate, "date");

                field.Error = true;

                ExitState = "FN0000_ADJUSTMENT_DT_LESS";

                return;
              }

              // ..............End Date > Last Accrual Date
              if (Lt(export.Group.Item.AccrualInstructions.LastAccrualDt,
                export.DebtAdjustmentToDate.Date))
              {
                var field = GetField(export.DebtAdjustmentToDate, "date");

                field.Error = true;

                ExitState = "FN0000_ADJ_END_DATE_GREATER";

                return;
              }
            }
          }
        }
        else if (AsChar(local.WriteoffOrReinstate.Flag) == 'W' || AsChar
          (local.WriteoffOrReinstate.Flag) == 'R')
        {
          // : A secondary obligation may not be selected for write-off or 
          // reinstate.
          if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'S')
          {
            if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
            {
              ExitState = "FN0000_CANT_WRITEOFF_SECONDARY";
            }
            else
            {
              ExitState = "FN0000_CANT_REINSTATE_SECONDARY";
            }

            return;
          }
        }

        // ..........Indicator edits
        switch(AsChar(export.DebtAdjustmentInd.SelectChar))
        {
          case '+':
            export.DebtAdjustment.DebtAdjustmentType =
              local.HardcodeIncrease.DebtAdjustmentType;

            break;
          case '-':
            export.DebtAdjustment.DebtAdjustmentType =
              local.HardcodeDecrease.DebtAdjustmentType;

            break;
          default:
            var field = GetField(export.DebtAdjustmentInd, "selectChar");

            field.Error = true;

            ExitState = "FN0000_INVALID_ADJ_INDICATOR";

            return;
        }

        local.ReadEachFound.Flag = "N";

        // ------------------------------------------------------------------
        // : Perform preliminary edits for writeoff, before asking user to 
        // confirm writeoff.
        //   Also will use the cab to verify that a writeoff exists for 
        // reinstate function.
        //   The program edits all debts for the supported person selected, and 
        // will continue
        //   processing if at least one of the debts is ok for the writeoff or 
        // reinstate.
        //   If none of the debts pass the edits, the supported person is 
        // highlighted,
        //   and the program escapes without checking the rest of the supported 
        // persons.
        // ------------------------------------------------------------------
        if (AsChar(import.ConfirmWriteoff.Flag) == 'N' && AsChar
          (local.WriteoffOrReinstate.Flag) == 'W' || AsChar
          (local.WriteoffOrReinstate.Flag) == 'R')
        {
          for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
            export.Group.Index)
          {
            if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
            {
              local.WoDebtAdjNf.Flag = "N";
              local.NothingToWriteoff.Flag = "N";
              local.WoForCurrDtAe.Flag = "N";
              local.NoDebtsFndForNaWo.Flag = "N";
              local.NoDebtFndFoStArrears.Flag = "N";

              if (!ReadSupported())
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                ExitState = "FN0000_SUPPORTED_PERSON_NF";

                return;
              }

              foreach(var item in ReadDebt2())
              {
                UseFnDebtAdjVerifyWoOrReinst();

                if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
                {
                  // : Action is reinstate, and write off debt adjustment was 
                  // not found for the debt.
                  local.WoDebtAdjNf.Flag = "Y";
                }
                else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
                {
                  // : Debt Detail Balance due is zero, and no automatic 
                  // collections were set to manual.
                  local.NothingToWriteoff.Flag = "Y";
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_NA_WO"))
                {
                  // : Action is NA WRITEOFF and debt is not NA
                  local.NoDebtsFndForNaWo.Flag = "Y";
                }
                else if (IsExitState("FN0000_WO_FOR_CURRENT_DATE_AE"))
                {
                  // : Debt has already been written off today.
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  return;
                }
                else if (IsExitState("FN0000_REINSTAT_WRITTEN_OFF_DEBT"))
                {
                  // -------------------------------------------------------------------
                  // : Action is a write off, and the debt was previously 
                  // written off, and there are
                  //  collections with a distribution method set to 'W'riteoff.
                  // Must do a reinstate of
                  //  this debt before doing the adjustment.
                  // -------------------------------------------------------------------
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  return;
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_ST_ARR"))
                {
                  // -------------------------------------------------------------------
                  // : No state owed arrears where found
                  // -------------------------------------------------------------------
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  local.NoDebtFndFoStArrears.Flag = "Y";
                }
                else if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // : If at least one debt for the supported person passes 
                  // edits, and the
                  //  condition for any errors does not require a rollback, we 
                  // will process
                  //  the debt adjustment.  So go to the next selected supported
                  // person.
                  goto Next;
                }
                else
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  return;
                }
              }

              // : End of verification of debts for the selected supported 
              // person.
              //   Handle write-off error messages.
              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (AsChar(local.WoForCurrDtAe.Flag) == 'Y')
                {
                  ExitState = "FN0000_WO_FOR_CURRENT_DATE_AE";
                }
                else if (AsChar(local.NoDebtsFndForNaWo.Flag) == 'Y')
                {
                  ExitState = "FN0000_NO_DEBTS_FOUND_FOR_NA_WO";
                }
                else
                {
                  if (AsChar(local.NothingToWriteoff.Flag) == 'Y')
                  {
                    ExitState = "FN0000_NO_BALANCE_TO_WRITE_OFF";
                  }

                  if (AsChar(local.NoDebtFndFoStArrears.Flag) == 'Y')
                  {
                    ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";
                  }
                }

                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                return;
              }
            }

Next:
            ;
          }
        }

        if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
        {
          // : Have user confirm writeoff function.
          if (AsChar(import.ConfirmWriteoff.Flag) == 'N')
          {
            // M. Brown, Oct, 2000, wo # 197 - Put out a different message from 
            // the usual
            // request to confirm if a date range has been entered.  This is so 
            // that the user
            // does not expect debts to be written off within a certain range of
            // dates (writeoff
            // and reinstate are done for all debts on the obligation).
            if (Equal(export.DebtAdjustmentFromDate.Date, local.Initialised.Date)
              && Equal
              (export.DebtAdjustmentToDate.Date, local.Initialised.Date))
            {
              ExitState = "FN0000_CONFIRM_WRITEOFF";
            }
            else if (Equal(export.ObligationTransactionRlnRsn.Code, "INC ADJ"))
            {
              ExitState = "FN0000_CONFIRM_WRITEOFF";
            }
            else
            {
              ExitState = "FN_DATE_RANGE_IGNORED_WO";
            }

            export.ConfirmWriteoff.Flag = "Y";

            var field1 = GetField(export.DebtAdjustmentFromDate, "date");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.DebtAdjustmentToDate, "date");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationTransactionRlnRsn, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.ObligationTransactionRln, "description");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 = GetField(export.DebtAdjustmentInd, "selectChar");

            field6.Color = "cyan";
            field6.Protected = true;

            for(export.Group.Index = 0; export.Group.Index < export
              .Group.Count; ++export.Group.Index)
            {
              var field7 = GetField(export.Group.Item.Common, "selectChar");

              field7.Color = "cyan";
              field7.Protected = true;

              var field8 = GetField(export.Group.Item.DebtAdjustment, "amount");

              field8.Color = "cyan";
              field8.Protected = true;
            }

            return;
          }
        }

        // ----------------------------------------
        //    ADJUST DEBTS
        // ----------------------------------------
        // : The following flags will help to determine what message should be 
        // set after processing adjustments.  Since not all the situations are
        // errors, in order to keep processing we need to reset the exitstate to
        // 'all ok' and use these flags to set the final exitstate.
        local.ReinstateOk.Flag = "N";
        local.WoOk.Flag = "N";
        local.StateDebtProcessed.Flag = "N";
        local.NothingToWriteoff.Flag = "N";
        local.WoDebtAdjNf.Flag = "N";
        local.WoForCurrDtAe.Flag = "N";
        local.NoDebtsFndForNaWo.Flag = "N";
        local.NoDebtFndFoStArrears.Flag = "N";

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (AsChar(export.Group.Item.Common.SelectChar) == 'S')
          {
            if (!ReadSupported())
            {
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              UseEabRollbackCics();
              ExitState = "FN0000_SUPPORTED_PERSON_NF";

              return;
            }

            // : Set date range for next read.
            if (!IsEmpty(local.WriteoffOrReinstate.Flag))
            {
              local.DebtDtlDueDateLow.Date = local.Initialised.Date;
              local.DebtDtlDueDateHigh.Date = local.Maximum.Date;
            }
            else
            {
              local.DebtDtlDueDateLow.Date = export.DebtAdjustmentFromDate.Date;
              local.DebtDtlDueDateHigh.Date = export.DebtAdjustmentToDate.Date;
            }

            foreach(var item in ReadDebtDetail())
            {
              if (!ReadDebt1())
              {
                ExitState = "FN0229_DEBT_NF";
                UseEabRollbackCics();

                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                return;
              }

              local.DebtDetail.DueDt = entities.DebtDetail.DueDt;

              if (IsEmpty(local.WriteoffOrReinstate.Flag))
              {
                // : These edits are not done for reinstate or writeoff 
                // functions.
                // : Check to see if the adjustment is a decrease.  If so, check
                // to make
                //   sure it will not bring the debt detail balance below zero.
                if (AsChar(export.DebtAdjustmentInd.SelectChar) == '-')
                {
                  local.TotalAdjustmentAmt.TotalCurrency = 0;

                  foreach(var item1 in ReadDebtAdjustment2())
                  {
                    if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == AsChar
                      (local.HardcodeIncrease.DebtAdjustmentType))
                    {
                      local.TotalAdjustmentAmt.TotalCurrency -= entities.
                        DebtAdjustment.Amount;
                    }
                    else if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) ==
                      AsChar(local.HardcodeDecrease.DebtAdjustmentType))
                    {
                      local.TotalAdjustmentAmt.TotalCurrency += entities.
                        DebtAdjustment.Amount;
                    }
                  }

                  // : Check to see if total adjustments, including proposed 
                  // adjustment
                  //   amount, are greater than the debt (accrual) amount.
                  if (entities.Debt.Amount - (
                    local.TotalAdjustmentAmt.TotalCurrency + export
                    .Group.Item.DebtAdjustment.Amount) < 0)
                  {
                    // : We have an error - now need to set the appropriate 
                    // exitstate.
                    // .............Adjustment Amount > Debt_Detail 
                    // Balance_Due_Amt
                    // =================================================
                    // 5/27/99 - bud adams  -  If the proposed adjusted amount 
                    // is
                    //   greater than the accrual amount, then The proposed
                    //   adjusted amount needs to be compared to the debt_detail
                    //   balance_due_amt values for each debt_detail to be
                    //   adjusted because there may have been a prior positive
                    //   adjustment.  This may only be one d_d or multiple,
                    //   depending on the date ranges input on the screen.
                    // =================================================
                    if (export.Group.Item.DebtAdjustment.Amount > entities
                      .DebtDetail.BalanceDueAmt && export
                      .Group.Item.DebtAdjustment.Amount > export
                      .Group.Item.Debt.Amount)
                    {
                      // =================================================
                      // If the proposed adjustment amount is greater than the 
                      // debt_
                      // detail balance_due_amount and the balance_due is the 
                      // same
                      // as the ob_tran amount (the accrual amount), then the 
                      // error
                      // message will be that the adustment amount is greater 
                      // than
                      // the accrual amount.  But if the balance_due is GREATER 
                      // than
                      // the accrual amount, then the error message will be 
                      // different.
                      // =================================================
                      if (entities.DebtDetail.BalanceDueAmt > export
                        .Group.Item.Debt.Amount)
                      {
                        ExitState = "FN0000_ADJUSTMENT_AMT_GT_OBL_AMT";
                      }
                      else
                      {
                        ExitState = "FN0000_ADJ_AMT_ERROR_RB";
                      }
                    }
                    else
                    {
                      ExitState = "FN0000_ADJUSTMENT_AMT_GT_OBL_AMT";
                    }

                    var field =
                      GetField(export.Group.Item.DebtAdjustment, "amount");

                    field.Error = true;

                    return;
                  }
                }
              }

              // : Ready to do the adjustment
              export.DebtAdjustment.Amount =
                export.Group.Item.DebtAdjustment.Amount;
              local.ReadEachFound.Flag = "Y";
              export.DebtAdjustment.DebtAdjustmentDt = local.Current.Date;
              UseFnApplyDebtAdjustment1();

              // : Check the exitstate.  If it's not all ok, but is not a 
              // serious error,
              //   set a flag to indicate what has occurred, so that at the end 
              // of processing
              //   we can determine what message should be displayed.
              if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
              {
                // : Action is reinstate, and write off debt adjustment was not 
                // found for the debt.
                local.WoDebtAdjNf.Flag = "Y";
              }
              else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
              {
                // : Debt Detail Balance due is zero, but automatic collections 
                // were updated to manual.
                local.NothingToWriteoff.Flag = "Y";
              }
              else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
              {
                // : Debt Detail Balance due is zero, and no automatic 
                // collections were set to manual.
                local.NothingToWriteoff.Flag = "Y";
              }
              else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_NA_WO"))
              {
                // : Action is NA WRITEOFF and debt is not NA
                local.NoDebtsFndForNaWo.Flag = "Y";
              }
              else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_ST_ARR"))
              {
                // : Action is NA WRITEOFF and debt is not NA
                local.NoDebtFndFoStArrears.Flag = "Y";
              }
              else if (IsExitState("FN0000_WO_FOR_CURRENT_DATE_AE"))
              {
                // : Debt has already been written off today.
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                UseEabRollbackCics();

                return;
              }
              else if (IsExitState("FN0000_REINSTAT_WRITTEN_OFF_DEBT"))
              {
                // : Action is a regular debt adjustment, and the debt is 
                // written off, and there are
                //  collections with a distribution method set to 'W'riteoff.  
                // Must do a reinstate of
                //  this debt before doing the adjustment.
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                UseEabRollbackCics();

                return;
              }
              else if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (AsChar(local.WriteoffOrReinstate.Flag) == 'R')
                {
                  local.ReinstateOk.Flag = "Y";
                }
                else if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
                {
                  local.WoOk.Flag = "Y";
                }

                if (Equal(export.ObligationTransactionRlnRsn.Code, "INC ADJ"))
                {
                  local.StateDebtProcessed.Flag = "Y";
                }
              }
              else
              {
                var field = GetField(export.Group.Item.Common, "selectChar");

                field.Error = true;

                UseEabRollbackCics();

                return;
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
              }

              // ----------------------------------------
              // : Joint and Several processing.
              //   We are in the loop that adjusts debts for a selected 
              // supported person.
              //   This loop is nested within the loop that goes through the 
              // list looking
              //   for selected supported persons.
              // ----------------------------------------
              // ---------------------------------------------------------------------------------------
              // : If this is a joint and several obligation, find the 
              // equivalent debt on this
              //  obligation as the one that was just processed.
              // ---------------------------------------------------------------------------------------
              if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
                (local.HardcodeJointSeveralConcurren.PrimarySecondaryCode))
              {
                if (!ReadObligationCsePersonDebtDetailDebt2())
                {
                  if (!ReadObligationCsePersonDebtDetailDebt1())
                  {
                    ExitState = "FN0000_JOINT_SEVERAL_DEBT_NF_RB";

                    return;
                  }
                }

                UseFnApplyDebtAdjustment2();

                // : Check the exitstate.  If it's not all ok, but is not a 
                // serious error,
                //   set a flag to indicate what has occurred, so that at the 
                // end of processing
                //   we can determine what message should be displayed.
                if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
                {
                  // : Action is reinstate, and write off debt adjustment was 
                  // not found for the debt.
                  local.WoDebtAdjNf.Flag = "Y";
                }
                else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
                {
                  // : Debt Detail Balance due is zero, but automatic 
                  // collections were updated to manual.
                  local.NothingToWriteoff.Flag = "Y";
                }
                else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
                {
                  // : Debt Detail Balance due is zero, and no automatic 
                  // collections were set to manual.
                  local.NothingToWriteoff.Flag = "Y";
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_NA_WO"))
                {
                  // : Action is NA WRITEOFF and debt is not NA
                  local.NoDebtsFndForNaWo.Flag = "Y";
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_ST_ARR"))
                {
                  // : Action is WO AF or INC ADJ and debt is not AF, FC, NC or 
                  // NF,
                  local.NoDebtFndFoStArrears.Flag = "Y";
                }
                else if (IsExitState("FN0000_WO_FOR_CURRENT_DATE_AE"))
                {
                  // : Debt has already been written off today.
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  UseEabRollbackCics();

                  return;
                }
                else if (IsExitState("FN0000_REINSTAT_WRITTEN_OFF_DEBT"))
                {
                  // : Action is a regular debt adjustment, but the debt is 
                  // written off.
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  UseEabRollbackCics();

                  return;
                }
                else if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  if (AsChar(local.WriteoffOrReinstate.Flag) == 'R')
                  {
                    local.ReinstateOk.Flag = "Y";
                  }
                  else if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
                  {
                    local.WoOk.Flag = "Y";
                  }

                  if (Equal(export.ObligationTransactionRlnRsn.Code, "INC ADJ"))
                  {
                    local.StateDebtProcessed.Flag = "Y";
                  }
                }
                else
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  UseEabRollbackCics();

                  return;
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";

                  continue;
                }
              }
            }

            // ------------------- PRIMARY/
            // SECONDARY
            // -----------------------------------
            // : April, 2000, M. Brown - Primary/Secondary logic.  If we are 
            // doing a
            //   writeoff or reinstate, we need to call the cab again for the '
            // other'
            //   obligation. We are in the supported person loop, so if the 
            // supported
            //   person was selected, we adjust all the debts for that person.
            // --------------------------------------------------------------------------
            if ((AsChar(entities.Obligation.PrimarySecondaryCode) == 'P' || AsChar
              (entities.Obligation.PrimarySecondaryCode) == 'S') && !
              IsEmpty(local.WriteoffOrReinstate.Flag))
            {
              if (AsChar(entities.Obligation.PrimarySecondaryCode) == 'P')
              {
                if (!ReadObligation2())
                {
                  // : Rollback will be handled in exitstate logic outside this 
                  // block of code.
                  ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";

                  goto Test1;
                }
              }
              else if (!ReadObligation1())
              {
                // : Rollback will be handled in exitstate logic outside this 
                // block of code.
                ExitState = "FN0000_PRIMARY_SECONDRY_OBLIG_NF";

                goto Test1;
              }

              foreach(var item in ReadDebt3())
              {
                UseFnApplyDebtAdjustment3();

                // : Check the exitstate.  If it's not all ok, but is not a 
                // serious error,
                //   set a flag to indicate what has occurred, so that at the 
                // end of processing
                //   we can determine what message should be displayed.
                //   (Only checking situations that apply to secondary, that's 
                // why this is not
                //   exactly the same as the exitstate checking for primary).
                if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
                {
                  // : Action is reinstate, and write off debt adjustment was 
                  // not found for the debt.
                }
                else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
                {
                  // : Debt Detail Balance due is zero, but automatic 
                  // collections were updated to manual.
                }
                else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
                {
                  // : Debt Detail Balance due is zero, and no automatic 
                  // collections were set to manual.
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_NA_WO"))
                {
                  // : Action is NA WRITEOFF and debt is not NA
                }
                else if (IsExitState("FN0000_REINSTAT_WRITTEN_OFF_DEBT"))
                {
                }
                else if (IsExitState("FN0000_NO_DEBTS_FOUND_FOR_ST_ARR"))
                {
                  // : Action is WO AF or INC ADJ and debt is not AF, FC, NC, or
                  // NF
                }
                else if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  var field = GetField(export.Group.Item.Common, "selectChar");

                  field.Error = true;

                  UseEabRollbackCics();

                  return;
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";

                  continue;
                }
              }
            }
          }

Test1:

          // : Sept 9, 1999, mfb: problem report 72838.  Set select char to 
          // spaces so that
          //  PF5 multiple times will not cause processing each time.
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Group.Update.Common.SelectChar = "";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        if (AsChar(local.ReadEachFound.Flag) == 'N')
        {
          ExitState = "FN0000_DEBT_DETAIL_FOR_ADJ_NF";

          return;
        }

        local.Obligor.Number = export.ApPayorCsePersonsWorkSet.Number;
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          return;
        }

        // =================================================
        // 1/28/99 - B Adams  -  deleted USE fn_summ_refresh_obligor_buckets
        //   IAW debt-adjustment meeting held 1/27/99.  Notes published
        //   by Darrin Greene, found in DBAJ file.
        // =================================================
        if (AsChar(local.WriteoffOrReinstate.Flag) == 'W')
        {
          if (AsChar(local.WoOk.Flag) == 'Y')
          {
            ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
          }
          else
          {
            if (AsChar(local.NothingToWriteoff.Flag) == 'Y')
            {
              ExitState = "FN0000_NOTHING_TO_WRITE_OFF";

              return;
            }

            if (AsChar(local.NoDebtFndFoStArrears.Flag) == 'Y')
            {
              // : No FC,AF,NC or NF debts found for state arrears writeoff.
              ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";

              return;
            }

            if (AsChar(local.NoDebtsFndForNaWo.Flag) == 'Y')
            {
              // : No NA debts found for NA writeoff.
              ExitState = "FN0000_NO_DEBTS_FOUND_FOR_NA_WO";

              return;
            }

            ExitState = "FN0000_WRITEOFF_NOT_SUCCESSFUL";

            return;
          }
        }
        else if (AsChar(local.WriteoffOrReinstate.Flag) == 'R')
        {
          if (AsChar(local.ReinstateOk.Flag) == 'Y')
          {
            ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
          }
          else
          {
            if (AsChar(local.WoDebtAdjNf.Flag) == 'Y')
            {
              // : Writeoff debt adjustment not found for reinstate.
              ExitState = "FN0000_WRITE_OFF_DEBT_ADJ_NF";
            }

            return;
          }
        }
        else if (IsEmpty(local.WriteoffOrReinstate.Flag))
        {
          if (AsChar(local.StateDebtProcessed.Flag) == 'Y')
          {
            ExitState = "FN0000_DEBT_ADJUSTMENT_OK";

            goto Test2;
          }

          if (AsChar(local.NoDebtFndFoStArrears.Flag) == 'Y')
          {
            // : No FC,AF,NC or NF debts found for state arrears writeoff.
            ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";

            return;
          }
        }

Test2:

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "FN0000_DEBT_ADJ_SUCCESSFUL";
        }

        export.DebtAdjustment.LastUpdatedBy = global.UserId;
        export.LocalAdjust.Date = local.Current.Date;

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "DEBT":
        ExitState = "ECO_LNK_TO_LST_DBT_ACT_BY_APPYR";

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.DebtAdjustmentType = source.DebtAdjustmentType;
    target.DebtAdjustmentDt = source.DebtAdjustmentDt;
    target.ReverseCollectionsInd = source.ReverseCollectionsInd;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction3(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtAdjustmentType = source.DebtAdjustmentType;
  }

  private static void MoveObligationTransaction4(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationTransactionRlnRsn(
    ObligationTransactionRlnRsn source, ObligationTransactionRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private void UseFnApplyDebtAdjustment1()
  {
    var useImport = new FnApplyDebtAdjustment.Import();
    var useExport = new FnApplyDebtAdjustment.Export();

    useImport.Debt.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.Max.Date = local.Maximum.Date;
    useImport.HardcodeActiveStatus.Code = local.HardcodeActiveStatus.Code;
    useImport.HardcodeDebtAdjustment.Type1 = local.HardcodeDebtAdjustment.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.ApPayorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDeactivatedStatus.Code;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardcodeCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodedWriteoffAf.SystemGeneratedIdentifier;
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnApplyDebtAdjustment2()
  {
    var useImport = new FnApplyDebtAdjustment.Import();
    var useExport = new FnApplyDebtAdjustment.Export();

    useImport.Debt.SystemGeneratedIdentifier =
      entities.JsOrPsDebt.SystemGeneratedIdentifier;
    useImport.HardcodeActiveStatus.Code = local.HardcodeActiveStatus.Code;
    useImport.HardcodeDebtAdjustment.Type1 = local.HardcodeDebtAdjustment.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = entities.JsObligor1.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.JsOrPsObligation.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.Max.Date = local.Maximum.Date;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDeactivatedStatus.Code;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardcodeCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodedWriteoffAf.SystemGeneratedIdentifier;
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnApplyDebtAdjustment3()
  {
    var useImport = new FnApplyDebtAdjustment.Import();
    var useExport = new FnApplyDebtAdjustment.Export();

    useImport.Debt.SystemGeneratedIdentifier =
      entities.JsOrPsDebt.SystemGeneratedIdentifier;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardcodeCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.JsOrPsObligation.SystemGeneratedIdentifier;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.Max.Date = local.Maximum.Date;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDeactivatedStatus.Code;
    useImport.HardcodeActiveStatus.Code = local.HardcodeActiveStatus.Code;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeDebt.Type1;
    useImport.HardcodeDebtAdjustment.Type1 = local.HardcodeDebtAdjustment.Type1;
    useImport.CsePerson.Number = export.ApPayorCsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodedWriteoffAf.SystemGeneratedIdentifier;
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.Filter.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = import.ApPayorCsePerson.Number;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnDebtAdjVerifyWoOrReinst()
  {
    var useImport = new FnDebtAdjVerifyWoOrReinst.Import();
    var useExport = new FnDebtAdjVerifyWoOrReinst.Export();

    useImport.Debt.SystemGeneratedIdentifier =
      entities.Debt.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardcodeCaseClosed.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.ApPayorCsePerson.Number;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.HardcodedWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodedWriteoffAf.SystemGeneratedIdentifier;
    useImport.HardcodedIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;

    Call(FnDebtAdjVerifyWoOrReinst.Execute, useImport, useExport);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeJointSeveralConcurren.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodeActiveStatus.Code = useExport.DdshActiveStatus.Code;
    MoveObligationTransaction4(useExport.OtrnDtDebtDetail,
      local.HardcodeDTypeDebt);
    MoveObligationTransaction4(useExport.OtrnDtAccrualInstructions,
      local.HardcodeAccrualInstruc);
    MoveObligationTransaction3(useExport.OtrnDatIncrease, local.HardcodeIncrease);
      
    MoveObligationTransaction3(useExport.OtrnDatDecrease, local.HardcodeDecrease);
      
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodeDebtAdjustment.Type1 = useExport.OtrnTDebtAdjustment.Type1;
    local.HardcodeDeactivatedStatus.Code = useExport.DdshDeactivedStatus.Code;
    local.HardcodeSupported.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeConcurrent.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodedAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
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

    useImport.CsePerson.Number = import.ApPayorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.ApPayorCsePersonsWorkSet.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.ApPayorCsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.ApPayorCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Obligor.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.Obligor);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ApPayorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.ApPayorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Group.Item.Supported.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.Group.Update.Supported.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadAccrualInstructionsDebt()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    return ReadEach("ReadAccrualInstructionsDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.Distinct.Type1 = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.Distinct.OtyType = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Distinct.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.Distinct.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.Distinct.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.Distinct.SystemGeneratedIdentifier = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.Distinct.Amount = db.GetDecimal(reader, 9);
        entities.Distinct.LastUpdatedTmst = db.GetNullableDateTime(reader, 10);
        entities.Distinct.DebtType = db.GetString(reader, 11);
        entities.Distinct.CspSupNumber = db.GetNullableString(reader, 12);
        entities.Distinct.CpaSupType = db.GetNullableString(reader, 13);
        entities.Distinct.Populated = true;
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Distinct.Type1);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Distinct.CpaType);
        CheckValid<ObligationTransaction>("DebtType", entities.Distinct.DebtType);
          
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.Distinct.CpaSupType);

        return true;
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", entities.Obligor1.Number);
        db.SetString(command, "cspNumber2", entities.SupportedCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Distinct.Populated);
    entities.SupportedCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Distinct.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.SupportedCsePerson.Number = db.GetString(reader, 0);
        entities.SupportedCsePerson.Type1 = db.GetString(reader, 1);
        entities.SupportedCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.SupportedCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.SupportedCsePerson.Type1);
      });
  }

  private bool ReadCsePersonObligor()
  {
    entities.Obligor1.Populated = false;
    entities.Obligor2.Populated = false;

    return Read("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", export.ApPayorCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor1.Number = db.GetString(reader, 0);
        entities.Obligor2.CspNumber = db.GetString(reader, 0);
        entities.Obligor1.Type1 = db.GetString(reader, 1);
        entities.Obligor1.OrganizationName = db.GetNullableString(reader, 2);
        entities.Obligor2.Type1 = db.GetString(reader, 3);
        entities.Obligor1.Populated = true;
        entities.Obligor2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Obligor1.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.Obligor2.Type1);
      });
  }

  private bool ReadDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "obTrnTyp", entities.DebtDetail.OtrType);
        db.SetInt32(command, "obTrnId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.DebtType = db.GetString(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private IEnumerable<bool> ReadDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;

    return ReadEach("ReadDebt2",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "debtTyp", local.HardcodeDTypeDebt.DebtType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.DebtType = db.GetString(reader, 6);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 7);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 8);
        entities.Debt.OtyType = db.GetInt32(reader, 9);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private IEnumerable<bool> ReadDebt3()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.JsOrPsObligation.Populated);
    entities.JsOrPsDebt.Populated = false;

    return ReadEach("ReadDebt3",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyType", entities.JsOrPsObligation.DtyGeneratedId);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.JsOrPsObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.JsOrPsObligation.CspNumber);
        db.SetString(command, "cpaType", entities.JsOrPsObligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetString(command, "debtTyp", local.HardcodeDTypeDebt.DebtType);
      },
      (db, reader) =>
      {
        entities.JsOrPsDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.JsOrPsDebt.CspNumber = db.GetString(reader, 1);
        entities.JsOrPsDebt.CpaType = db.GetString(reader, 2);
        entities.JsOrPsDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.JsOrPsDebt.Type1 = db.GetString(reader, 4);
        entities.JsOrPsDebt.DebtType = db.GetString(reader, 5);
        entities.JsOrPsDebt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.JsOrPsDebt.CpaSupType = db.GetNullableString(reader, 7);
        entities.JsOrPsDebt.OtyType = db.GetInt32(reader, 8);
        entities.JsOrPsDebt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.JsOrPsDebt.CpaType);
          
        CheckValid<ObligationTransaction>("Type1", entities.JsOrPsDebt.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.JsOrPsDebt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JsOrPsDebt.CpaSupType);

        return true;
      });
  }

  private bool ReadDebtAdjustment1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment1",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
      });
  }

  private IEnumerable<bool> ReadDebtAdjustment2()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadDebtAdjustment2",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 7);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 8);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 9);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 11);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "date1", local.DebtDtlDueDateLow.Date.GetValueOrDefault());
        db.SetDate(
          command, "date2", local.DebtDtlDueDateHigh.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.Obligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "deactivationDate",
          local.Initialised.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.Obligation.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "otyIdentifier", entities.Obligation.DtyGeneratedId);
          
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetDate(
          command, "deactivationDate",
          local.Initialised.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JsOrPsObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.JsOrPsObligation.CpaType = db.GetString(reader, 0);
        entities.JsOrPsObligation.CspNumber = db.GetString(reader, 1);
        entities.JsOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JsOrPsObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.JsOrPsObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.JsOrPsObligation.CpaType);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JsOrPsObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.JsOrPsObligation.CpaType = db.GetString(reader, 0);
        entities.JsOrPsObligation.CspNumber = db.GetString(reader, 1);
        entities.JsOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JsOrPsObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.JsOrPsObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.JsOrPsObligation.CpaType);
      });
  }

  private bool ReadObligation3()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation3",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", export.ApPayorCsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor2.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation4",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", entities.Obligor2.Type1);
        db.SetString(command, "cspNumber", entities.Obligor2.CspNumber);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationCsePersonDebtDetailDebt1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JsObligor1.Populated = false;
    entities.JsOrPsObligation.Populated = false;
    entities.JsOrPsDebt.Populated = false;
    entities.JsDebtDetail.Populated = false;

    return Read("ReadObligationCsePersonDebtDetailDebt1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.
          SetDate(command, "dueDt", local.DebtDetail.DueDt.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.JsOrPsObligation.CpaType = db.GetString(reader, 0);
        entities.JsOrPsObligation.CspNumber = db.GetString(reader, 1);
        entities.JsOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JsOrPsObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.JsObligor1.Number = db.GetString(reader, 4);
        entities.JsObligor1.Type1 = db.GetString(reader, 5);
        entities.JsObligor1.OrganizationName = db.GetNullableString(reader, 6);
        entities.JsDebtDetail.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.JsOrPsDebt.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.JsDebtDetail.CspNumber = db.GetString(reader, 8);
        entities.JsOrPsDebt.CspNumber = db.GetString(reader, 8);
        entities.JsDebtDetail.CpaType = db.GetString(reader, 9);
        entities.JsOrPsDebt.CpaType = db.GetString(reader, 9);
        entities.JsDebtDetail.OtrGeneratedId = db.GetInt32(reader, 10);
        entities.JsOrPsDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.JsDebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.JsOrPsDebt.OtyType = db.GetInt32(reader, 11);
        entities.JsDebtDetail.OtrType = db.GetString(reader, 12);
        entities.JsOrPsDebt.Type1 = db.GetString(reader, 12);
        entities.JsDebtDetail.DueDt = db.GetDate(reader, 13);
        entities.JsOrPsDebt.DebtType = db.GetString(reader, 14);
        entities.JsOrPsDebt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.JsOrPsDebt.CpaSupType = db.GetNullableString(reader, 16);
        entities.JsObligor1.Populated = true;
        entities.JsOrPsObligation.Populated = true;
        entities.JsOrPsDebt.Populated = true;
        entities.JsDebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.JsOrPsObligation.CpaType);
        CheckValid<CsePerson>("Type1", entities.JsObligor1.Type1);
        CheckValid<DebtDetail>("CpaType", entities.JsDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.JsOrPsDebt.CpaType);
          
        CheckValid<DebtDetail>("OtrType", entities.JsDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.JsOrPsDebt.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.JsOrPsDebt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JsOrPsDebt.CpaSupType);
      });
  }

  private bool ReadObligationCsePersonDebtDetailDebt2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JsObligor1.Populated = false;
    entities.JsOrPsObligation.Populated = false;
    entities.JsOrPsDebt.Populated = false;
    entities.JsDebtDetail.Populated = false;

    return Read("ReadObligationCsePersonDebtDetailDebt2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported.CspNumber);
        db.
          SetDate(command, "dueDt", local.DebtDetail.DueDt.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.JsOrPsObligation.CpaType = db.GetString(reader, 0);
        entities.JsOrPsObligation.CspNumber = db.GetString(reader, 1);
        entities.JsOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JsOrPsObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.JsObligor1.Number = db.GetString(reader, 4);
        entities.JsObligor1.Type1 = db.GetString(reader, 5);
        entities.JsObligor1.OrganizationName = db.GetNullableString(reader, 6);
        entities.JsDebtDetail.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.JsOrPsDebt.ObgGeneratedId = db.GetInt32(reader, 7);
        entities.JsDebtDetail.CspNumber = db.GetString(reader, 8);
        entities.JsOrPsDebt.CspNumber = db.GetString(reader, 8);
        entities.JsDebtDetail.CpaType = db.GetString(reader, 9);
        entities.JsOrPsDebt.CpaType = db.GetString(reader, 9);
        entities.JsDebtDetail.OtrGeneratedId = db.GetInt32(reader, 10);
        entities.JsOrPsDebt.SystemGeneratedIdentifier = db.GetInt32(reader, 10);
        entities.JsDebtDetail.OtyType = db.GetInt32(reader, 11);
        entities.JsOrPsDebt.OtyType = db.GetInt32(reader, 11);
        entities.JsDebtDetail.OtrType = db.GetString(reader, 12);
        entities.JsOrPsDebt.Type1 = db.GetString(reader, 12);
        entities.JsDebtDetail.DueDt = db.GetDate(reader, 13);
        entities.JsOrPsDebt.DebtType = db.GetString(reader, 14);
        entities.JsOrPsDebt.CspSupNumber = db.GetNullableString(reader, 15);
        entities.JsOrPsDebt.CpaSupType = db.GetNullableString(reader, 16);
        entities.JsObligor1.Populated = true;
        entities.JsOrPsObligation.Populated = true;
        entities.JsOrPsDebt.Populated = true;
        entities.JsDebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.JsOrPsObligation.CpaType);
        CheckValid<CsePerson>("Type1", entities.JsObligor1.Type1);
        CheckValid<DebtDetail>("CpaType", entities.JsDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.JsOrPsDebt.CpaType);
          
        CheckValid<DebtDetail>("OtrType", entities.JsDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.JsOrPsDebt.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.JsOrPsDebt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JsOrPsDebt.CpaSupType);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 5);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 6);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetString(
          command, "obTrnRlnRsnCd", export.ObligationTransactionRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 1);
        entities.ObligationTransactionRlnRsn.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
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
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
  }

  private bool ReadSupported()
  {
    entities.Supported.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.Group.Item.Supported.Number);
      },
      (db, reader) =>
      {
        entities.Supported.CspNumber = db.GetString(reader, 0);
        entities.Supported.Type1 = db.GetString(reader, 1);
        entities.Supported.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported.Type1);
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
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
      /// A value of DebtAdjustment.
      /// </summary>
      [JsonPropertyName("debtAdjustment")]
      public ObligationTransaction DebtAdjustment
      {
        get => debtAdjustment ??= new();
        set => debtAdjustment = value;
      }

      /// <summary>
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public Obligation Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common common;
      private CsePersonsWorkSet supported;
      private Case1 case1;
      private ObligationTransaction debtAdjustment;
      private AccrualInstructions accrualInstructions;
      private Obligation hidden;
      private ObligationTransaction debt;
    }

    /// <summary>
    /// A value of LastAccrualDate.
    /// </summary>
    [JsonPropertyName("lastAccrualDate")]
    public DateWorkArea LastAccrualDate
    {
      get => lastAccrualDate ??= new();
      set => lastAccrualDate = value;
    }

    /// <summary>
    /// A value of LocalAdjust.
    /// </summary>
    [JsonPropertyName("localAdjust")]
    public DateWorkArea LocalAdjust
    {
      get => localAdjust ??= new();
      set => localAdjust = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ApPayorCsePerson.
    /// </summary>
    [JsonPropertyName("apPayorCsePerson")]
    public CsePerson ApPayorCsePerson
    {
      get => apPayorCsePerson ??= new();
      set => apPayorCsePerson = value;
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
    /// A value of ApPayorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apPayorCsePersonsWorkSet")]
    public CsePersonsWorkSet ApPayorCsePersonsWorkSet
    {
      get => apPayorCsePersonsWorkSet ??= new();
      set => apPayorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of DebtAdjustmentInd.
    /// </summary>
    [JsonPropertyName("debtAdjustmentInd")]
    public Common DebtAdjustmentInd
    {
      get => debtAdjustmentInd ??= new();
      set => debtAdjustmentInd = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of DebtObligationTransaction.
    /// </summary>
    [JsonPropertyName("debtObligationTransaction")]
    public ObligationTransaction DebtObligationTransaction
    {
      get => debtObligationTransaction ??= new();
      set => debtObligationTransaction = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of DebtAccrualInstructions.
    /// </summary>
    [JsonPropertyName("debtAccrualInstructions")]
    public AccrualInstructions DebtAccrualInstructions
    {
      get => debtAccrualInstructions ??= new();
      set => debtAccrualInstructions = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of CollProtectionExists.
    /// </summary>
    [JsonPropertyName("collProtectionExists")]
    public Common CollProtectionExists
    {
      get => collProtectionExists ??= new();
      set => collProtectionExists = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentToDate.
    /// </summary>
    [JsonPropertyName("debtAdjustmentToDate")]
    public DateWorkArea DebtAdjustmentToDate
    {
      get => debtAdjustmentToDate ??= new();
      set => debtAdjustmentToDate = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentFromDate.
    /// </summary>
    [JsonPropertyName("debtAdjustmentFromDate")]
    public DateWorkArea DebtAdjustmentFromDate
    {
      get => debtAdjustmentFromDate ??= new();
      set => debtAdjustmentFromDate = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ConfirmWriteoff.
    /// </summary>
    [JsonPropertyName("confirmWriteoff")]
    public Common ConfirmWriteoff
    {
      get => confirmWriteoff ??= new();
      set => confirmWriteoff = value;
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

    private DateWorkArea lastAccrualDate;
    private DateWorkArea localAdjust;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePerson apPayorCsePerson;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private CsePersonsWorkSet apPayorCsePersonsWorkSet;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private Common debtAdjustmentInd;
    private ObligationType obligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction debtObligationTransaction;
    private ObligationTransaction debtAdjustment;
    private AccrualInstructions debtAccrualInstructions;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common collProtectionExists;
    private Common prompt;
    private Common nextTransaction;
    private DateWorkArea debtAdjustmentToDate;
    private DateWorkArea debtAdjustmentFromDate;
    private NextTranInfo hidden;
    private Standard standard;
    private CsePerson csePerson;
    private Common confirmWriteoff;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Supported.
      /// </summary>
      [JsonPropertyName("supported")]
      public CsePersonsWorkSet Supported
      {
        get => supported ??= new();
        set => supported = value;
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
      /// A value of DebtAdjustment.
      /// </summary>
      [JsonPropertyName("debtAdjustment")]
      public ObligationTransaction DebtAdjustment
      {
        get => debtAdjustment ??= new();
        set => debtAdjustment = value;
      }

      /// <summary>
      /// A value of AccrualInstructions.
      /// </summary>
      [JsonPropertyName("accrualInstructions")]
      public AccrualInstructions AccrualInstructions
      {
        get => accrualInstructions ??= new();
        set => accrualInstructions = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public Obligation Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of Debt.
      /// </summary>
      [JsonPropertyName("debt")]
      public ObligationTransaction Debt
      {
        get => debt ??= new();
        set => debt = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common common;
      private CsePersonsWorkSet supported;
      private Case1 case1;
      private ObligationTransaction debtAdjustment;
      private AccrualInstructions accrualInstructions;
      private Obligation hidden;
      private ObligationTransaction debt;
    }

    /// <summary>
    /// A value of LastAccrualDate.
    /// </summary>
    [JsonPropertyName("lastAccrualDate")]
    public DateWorkArea LastAccrualDate
    {
      get => lastAccrualDate ??= new();
      set => lastAccrualDate = value;
    }

    /// <summary>
    /// A value of LocalAdjust.
    /// </summary>
    [JsonPropertyName("localAdjust")]
    public DateWorkArea LocalAdjust
    {
      get => localAdjust ??= new();
      set => localAdjust = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ApPayorCsePerson.
    /// </summary>
    [JsonPropertyName("apPayorCsePerson")]
    public CsePerson ApPayorCsePerson
    {
      get => apPayorCsePerson ??= new();
      set => apPayorCsePerson = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of DebtAdjustmentInd.
    /// </summary>
    [JsonPropertyName("debtAdjustmentInd")]
    public Common DebtAdjustmentInd
    {
      get => debtAdjustmentInd ??= new();
      set => debtAdjustmentInd = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of ApPayorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apPayorCsePersonsWorkSet")]
    public CsePersonsWorkSet ApPayorCsePersonsWorkSet
    {
      get => apPayorCsePersonsWorkSet ??= new();
      set => apPayorCsePersonsWorkSet = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of DebtObligationTransaction.
    /// </summary>
    [JsonPropertyName("debtObligationTransaction")]
    public ObligationTransaction DebtObligationTransaction
    {
      get => debtObligationTransaction ??= new();
      set => debtObligationTransaction = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of DebtAccrualInstructions.
    /// </summary>
    [JsonPropertyName("debtAccrualInstructions")]
    public AccrualInstructions DebtAccrualInstructions
    {
      get => debtAccrualInstructions ??= new();
      set => debtAccrualInstructions = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of CollProtectionExists.
    /// </summary>
    [JsonPropertyName("collProtectionExists")]
    public Common CollProtectionExists
    {
      get => collProtectionExists ??= new();
      set => collProtectionExists = value;
    }

    /// <summary>
    /// A value of AccruedAmount.
    /// </summary>
    [JsonPropertyName("accruedAmount")]
    public ScreenOwedAmounts AccruedAmount
    {
      get => accruedAmount ??= new();
      set => accruedAmount = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentToDate.
    /// </summary>
    [JsonPropertyName("debtAdjustmentToDate")]
    public DateWorkArea DebtAdjustmentToDate
    {
      get => debtAdjustmentToDate ??= new();
      set => debtAdjustmentToDate = value;
    }

    /// <summary>
    /// A value of DebtAdjustmentFromDate.
    /// </summary>
    [JsonPropertyName("debtAdjustmentFromDate")]
    public DateWorkArea DebtAdjustmentFromDate
    {
      get => debtAdjustmentFromDate ??= new();
      set => debtAdjustmentFromDate = value;
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
    /// A value of ConfirmWriteoff.
    /// </summary>
    [JsonPropertyName("confirmWriteoff")]
    public Common ConfirmWriteoff
    {
      get => confirmWriteoff ??= new();
      set => confirmWriteoff = value;
    }

    private DateWorkArea lastAccrualDate;
    private DateWorkArea localAdjust;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePerson apPayorCsePerson;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Array<GroupGroup> group;
    private LegalAction legalAction;
    private Common debtAdjustmentInd;
    private Common nextTransaction;
    private CsePersonsWorkSet apPayorCsePersonsWorkSet;
    private ObligationType obligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction debtObligationTransaction;
    private ObligationTransaction debtAdjustment;
    private AccrualInstructions debtAccrualInstructions;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common collProtectionExists;
    private ScreenOwedAmounts accruedAmount;
    private Common prompt;
    private DateWorkArea debtAdjustmentToDate;
    private DateWorkArea debtAdjustmentFromDate;
    private NextTranInfo hidden;
    private Standard standard;
    private Common confirmWriteoff;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of StateDebtProcessed.
    /// </summary>
    [JsonPropertyName("stateDebtProcessed")]
    public Common StateDebtProcessed
    {
      get => stateDebtProcessed ??= new();
      set => stateDebtProcessed = value;
    }

    /// <summary>
    /// A value of NoDebtFndFoStArrears.
    /// </summary>
    [JsonPropertyName("noDebtFndFoStArrears")]
    public Common NoDebtFndFoStArrears
    {
      get => noDebtFndFoStArrears ??= new();
      set => noDebtFndFoStArrears = value;
    }

    /// <summary>
    /// A value of DebtDtlDueDateHigh.
    /// </summary>
    [JsonPropertyName("debtDtlDueDateHigh")]
    public DateWorkArea DebtDtlDueDateHigh
    {
      get => debtDtlDueDateHigh ??= new();
      set => debtDtlDueDateHigh = value;
    }

    /// <summary>
    /// A value of DebtDtlDueDateLow.
    /// </summary>
    [JsonPropertyName("debtDtlDueDateLow")]
    public DateWorkArea DebtDtlDueDateLow
    {
      get => debtDtlDueDateLow ??= new();
      set => debtDtlDueDateLow = value;
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
    /// A value of ReadEachFound.
    /// </summary>
    [JsonPropertyName("readEachFound")]
    public Common ReadEachFound
    {
      get => readEachFound ??= new();
      set => readEachFound = value;
    }

    /// <summary>
    /// A value of NoDebtsFndForNaWo.
    /// </summary>
    [JsonPropertyName("noDebtsFndForNaWo")]
    public Common NoDebtsFndForNaWo
    {
      get => noDebtsFndForNaWo ??= new();
      set => noDebtsFndForNaWo = value;
    }

    /// <summary>
    /// A value of WoForCurrDtAe.
    /// </summary>
    [JsonPropertyName("woForCurrDtAe")]
    public Common WoForCurrDtAe
    {
      get => woForCurrDtAe ??= new();
      set => woForCurrDtAe = value;
    }

    /// <summary>
    /// A value of WoDebtAdjNf.
    /// </summary>
    [JsonPropertyName("woDebtAdjNf")]
    public Common WoDebtAdjNf
    {
      get => woDebtAdjNf ??= new();
      set => woDebtAdjNf = value;
    }

    /// <summary>
    /// A value of VerifyOnly.
    /// </summary>
    [JsonPropertyName("verifyOnly")]
    public Common VerifyOnly
    {
      get => verifyOnly ??= new();
      set => verifyOnly = value;
    }

    /// <summary>
    /// A value of WoOk.
    /// </summary>
    [JsonPropertyName("woOk")]
    public Common WoOk
    {
      get => woOk ??= new();
      set => woOk = value;
    }

    /// <summary>
    /// A value of AlreadyWrittenOff.
    /// </summary>
    [JsonPropertyName("alreadyWrittenOff")]
    public Common AlreadyWrittenOff
    {
      get => alreadyWrittenOff ??= new();
      set => alreadyWrittenOff = value;
    }

    /// <summary>
    /// A value of NothingToWriteoff.
    /// </summary>
    [JsonPropertyName("nothingToWriteoff")]
    public Common NothingToWriteoff
    {
      get => nothingToWriteoff ??= new();
      set => nothingToWriteoff = value;
    }

    /// <summary>
    /// A value of ReinstateOk.
    /// </summary>
    [JsonPropertyName("reinstateOk")]
    public Common ReinstateOk
    {
      get => reinstateOk ??= new();
      set => reinstateOk = value;
    }

    /// <summary>
    /// A value of WriteoffOrReinstate.
    /// </summary>
    [JsonPropertyName("writeoffOrReinstate")]
    public Common WriteoffOrReinstate
    {
      get => writeoffOrReinstate ??= new();
      set => writeoffOrReinstate = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonsWorkSet Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of TotalAdjustmentAmt.
    /// </summary>
    [JsonPropertyName("totalAdjustmentAmt")]
    public Common TotalAdjustmentAmt
    {
      get => totalAdjustmentAmt ??= new();
      set => totalAdjustmentAmt = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public DateWorkArea Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of SelectionCounter.
    /// </summary>
    [JsonPropertyName("selectionCounter")]
    public Common SelectionCounter
    {
      get => selectionCounter ??= new();
      set => selectionCounter = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffNa.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffNa")]
    public ObligationTransactionRlnRsn HardcodeWriteoffNa
    {
      get => hardcodeWriteoffNa ??= new();
      set => hardcodeWriteoffNa = value;
    }

    /// <summary>
    /// A value of HardcodeReinstate.
    /// </summary>
    [JsonPropertyName("hardcodeReinstate")]
    public ObligationTransactionRlnRsn HardcodeReinstate
    {
      get => hardcodeReinstate ??= new();
      set => hardcodeReinstate = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffAll.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffAll")]
    public ObligationTransactionRlnRsn HardcodeWriteoffAll
    {
      get => hardcodeWriteoffAll ??= new();
      set => hardcodeWriteoffAll = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public CsePersonAccount HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <summary>
    /// A value of HardcodeDeactivatedStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDeactivatedStatus")]
    public DebtDetailStatusHistory HardcodeDeactivatedStatus
    {
      get => hardcodeDeactivatedStatus ??= new();
      set => hardcodeDeactivatedStatus = value;
    }

    /// <summary>
    /// A value of HardcodeJointSeveralConcurren.
    /// </summary>
    [JsonPropertyName("hardcodeJointSeveralConcurren")]
    public Obligation HardcodeJointSeveralConcurren
    {
      get => hardcodeJointSeveralConcurren ??= new();
      set => hardcodeJointSeveralConcurren = value;
    }

    /// <summary>
    /// A value of HardcodeActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeActiveStatus")]
    public DebtDetailStatusHistory HardcodeActiveStatus
    {
      get => hardcodeActiveStatus ??= new();
      set => hardcodeActiveStatus = value;
    }

    /// <summary>
    /// A value of HardcodeDTypeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDTypeDebt")]
    public ObligationTransaction HardcodeDTypeDebt
    {
      get => hardcodeDTypeDebt ??= new();
      set => hardcodeDTypeDebt = value;
    }

    /// <summary>
    /// A value of HardcodeAccrualInstruc.
    /// </summary>
    [JsonPropertyName("hardcodeAccrualInstruc")]
    public ObligationTransaction HardcodeAccrualInstruc
    {
      get => hardcodeAccrualInstruc ??= new();
      set => hardcodeAccrualInstruc = value;
    }

    /// <summary>
    /// A value of HardcodeIncrease.
    /// </summary>
    [JsonPropertyName("hardcodeIncrease")]
    public ObligationTransaction HardcodeIncrease
    {
      get => hardcodeIncrease ??= new();
      set => hardcodeIncrease = value;
    }

    /// <summary>
    /// A value of HardcodeDecrease.
    /// </summary>
    [JsonPropertyName("hardcodeDecrease")]
    public ObligationTransaction HardcodeDecrease
    {
      get => hardcodeDecrease ??= new();
      set => hardcodeDecrease = value;
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
    /// A value of HardcodeDebt.
    /// </summary>
    [JsonPropertyName("hardcodeDebt")]
    public ObligationTransaction HardcodeDebt
    {
      get => hardcodeDebt ??= new();
      set => hardcodeDebt = value;
    }

    /// <summary>
    /// A value of HardcodeDebtAdjustment.
    /// </summary>
    [JsonPropertyName("hardcodeDebtAdjustment")]
    public ObligationTransaction HardcodeDebtAdjustment
    {
      get => hardcodeDebtAdjustment ??= new();
      set => hardcodeDebtAdjustment = value;
    }

    /// <summary>
    /// A value of HardcodeConcurrent.
    /// </summary>
    [JsonPropertyName("hardcodeConcurrent")]
    public ObligationTransactionRlnRsn HardcodeConcurrent
    {
      get => hardcodeConcurrent ??= new();
      set => hardcodeConcurrent = value;
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
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of HardcodeCaseClosed.
    /// </summary>
    [JsonPropertyName("hardcodeCaseClosed")]
    public ObligationTransactionRlnRsn HardcodeCaseClosed
    {
      get => hardcodeCaseClosed ??= new();
      set => hardcodeCaseClosed = value;
    }

    /// <summary>
    /// A value of HardcodedWriteoffAf.
    /// </summary>
    [JsonPropertyName("hardcodedWriteoffAf")]
    public ObligationTransactionRlnRsn HardcodedWriteoffAf
    {
      get => hardcodedWriteoffAf ??= new();
      set => hardcodedWriteoffAf = value;
    }

    /// <summary>
    /// A value of HardcodeIncAdj.
    /// </summary>
    [JsonPropertyName("hardcodeIncAdj")]
    public ObligationTransactionRlnRsn HardcodeIncAdj
    {
      get => hardcodeIncAdj ??= new();
      set => hardcodeIncAdj = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      stateDebtProcessed = null;
      noDebtFndFoStArrears = null;
      debtDetail = null;
      readEachFound = null;
      noDebtsFndForNaWo = null;
      woForCurrDtAe = null;
      woDebtAdjNf = null;
      verifyOnly = null;
      woOk = null;
      alreadyWrittenOff = null;
      nothingToWriteoff = null;
      reinstateOk = null;
      writeoffOrReinstate = null;
      obligor = null;
      totalAdjustmentAmt = null;
      initialised = null;
      selectionCounter = null;
      hardcodeWriteoffNa = null;
      hardcodeReinstate = null;
      hardcodeWriteoffAll = null;
      hardcodeSupported = null;
      hardcodeDeactivatedStatus = null;
      hardcodeJointSeveralConcurren = null;
      hardcodeConcurrent = null;
      program = null;
      hardcodedAccruing = null;
      hardcodeCaseClosed = null;
      hardcodedWriteoffAf = null;
      hardcodeIncAdj = null;
    }

    private Common stateDebtProcessed;
    private Common noDebtFndFoStArrears;
    private DateWorkArea debtDtlDueDateHigh;
    private DateWorkArea debtDtlDueDateLow;
    private DebtDetail debtDetail;
    private Common readEachFound;
    private Common noDebtsFndForNaWo;
    private Common woForCurrDtAe;
    private Common woDebtAdjNf;
    private Common verifyOnly;
    private Common woOk;
    private Common alreadyWrittenOff;
    private Common nothingToWriteoff;
    private Common reinstateOk;
    private Common writeoffOrReinstate;
    private CsePersonsWorkSet obligor;
    private Common totalAdjustmentAmt;
    private DateWorkArea current;
    private DateWorkArea maximum;
    private DateWorkArea initialised;
    private DateWorkArea date;
    private Common selectionCounter;
    private ObligationTransactionRlnRsn hardcodeWriteoffNa;
    private ObligationTransactionRlnRsn hardcodeReinstate;
    private ObligationTransactionRlnRsn hardcodeWriteoffAll;
    private CsePersonAccount hardcodeSupported;
    private DebtDetailStatusHistory hardcodeDeactivatedStatus;
    private Obligation hardcodeJointSeveralConcurren;
    private DebtDetailStatusHistory hardcodeActiveStatus;
    private ObligationTransaction hardcodeDTypeDebt;
    private ObligationTransaction hardcodeAccrualInstruc;
    private ObligationTransaction hardcodeIncrease;
    private ObligationTransaction hardcodeDecrease;
    private CsePersonAccount hardcodeObligor;
    private ObligationTransaction hardcodeDebt;
    private ObligationTransaction hardcodeDebtAdjustment;
    private ObligationTransactionRlnRsn hardcodeConcurrent;
    private Program program;
    private ObligationType hardcodedAccruing;
    private ObligationTransactionRlnRsn hardcodeCaseClosed;
    private ObligationTransactionRlnRsn hardcodedWriteoffAf;
    private ObligationTransactionRlnRsn hardcodeIncAdj;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of SupportedCsePerson.
    /// </summary>
    [JsonPropertyName("supportedCsePerson")]
    public CsePerson SupportedCsePerson
    {
      get => supportedCsePerson ??= new();
      set => supportedCsePerson = value;
    }

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of JsObligor1.
    /// </summary>
    [JsonPropertyName("jsObligor1")]
    public CsePerson JsObligor1
    {
      get => jsObligor1 ??= new();
      set => jsObligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of JsObligor2.
    /// </summary>
    [JsonPropertyName("jsObligor2")]
    public CsePersonAccount JsObligor2
    {
      get => jsObligor2 ??= new();
      set => jsObligor2 = value;
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
    /// A value of JsOrPsObligation.
    /// </summary>
    [JsonPropertyName("jsOrPsObligation")]
    public Obligation JsOrPsObligation
    {
      get => jsOrPsObligation ??= new();
      set => jsOrPsObligation = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of Distinct.
    /// </summary>
    [JsonPropertyName("distinct")]
    public ObligationTransaction Distinct
    {
      get => distinct ??= new();
      set => distinct = value;
    }

    /// <summary>
    /// A value of JsOrPsDebt.
    /// </summary>
    [JsonPropertyName("jsOrPsDebt")]
    public ObligationTransaction JsOrPsDebt
    {
      get => jsOrPsDebt ??= new();
      set => jsOrPsDebt = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of JsDebtDetail.
    /// </summary>
    [JsonPropertyName("jsDebtDetail")]
    public DebtDetail JsDebtDetail
    {
      get => jsDebtDetail ??= new();
      set => jsDebtDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of SupportedCaseRole.
    /// </summary>
    [JsonPropertyName("supportedCaseRole")]
    public CaseRole SupportedCaseRole
    {
      get => supportedCaseRole ??= new();
      set => supportedCaseRole = value;
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

    private CsePerson supportedCsePerson;
    private CsePerson obligor1;
    private CsePerson jsObligor1;
    private CsePersonAccount obligor2;
    private CsePersonAccount supported;
    private CsePersonAccount jsObligor2;
    private Obligation obligation;
    private Obligation jsOrPsObligation;
    private ObligationRln obligationRln;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private ObligationTransaction distinct;
    private ObligationTransaction jsOrPsDebt;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private AccrualInstructions accrualInstructions;
    private DebtDetail debtDetail;
    private DebtDetail jsDebtDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private LegalAction legalAction;
    private LegalActionPerson legalActionPerson;
    private Case1 case1;
    private CaseRole absentParent;
    private CaseRole supportedCaseRole;
    private ObligCollProtectionHist obligCollProtectionHist;
  }
#endregion
}

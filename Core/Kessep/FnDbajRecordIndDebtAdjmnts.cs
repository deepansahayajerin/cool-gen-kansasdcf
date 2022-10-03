// Program: FN_DBAJ_RECORD_IND_DEBT_ADJMNTS, ID: 372376994, model: 746.
// Short name: SWEDBAJP
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
/// A program: FN_DBAJ_RECORD_IND_DEBT_ADJMNTS.
/// </para>
/// <para>
/// To provide a means to adjust the amount due on individual debts.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDbajRecordIndDebtAdjmnts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DBAJ_RECORD_IND_DEBT_ADJMNTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDbajRecordIndDebtAdjmnts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDbajRecordIndDebtAdjmnts.
  /// </summary>
  public FnDbajRecordIndDebtAdjmnts(IContext context, Import import,
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
    // *******************************************************************
    // 01/04/97	R. Marchman	Add new security/next tran.
    // 06/09/1997	A Samuels	Implement new screen design
    // 				with scrolling
    // 10/20/97	JeHoward	PR#35396 - Prior Adjustments
    // 				Date not displaying.
    // 1/12/1998  Gary McGirr	        Add CSE supported person to screen
    //                                 
    // PR#29078
    // 02/24/98	A Samuels	PR#38703
    // 3/1/1999	Bud Adams              Added 'reverse collections' switch to 
    // screen; added logic to inform user if manually distributed collections
    // exist; added logic so that if the debt being adjusted is a joint and
    // several, then the 'other' debt will be adjusted as well; deleted use of
    // code that attempted to maintain summary level attributes.
    // 3/15/99 - b adams  -  Exit state 'manual-colls-exist'  added
    //   to 'FN_Apply...', so exit state tests expanded here.
    // 5/12/99 - B Adams  -  Changed the date being displayed on
    //   the screen from Obligation_Transaction Debt_Adjustment_Process_Date
    //   to Ob_Tran Debt_Adjustment_Dt
    // *******************************************************************
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
    //  - Writeoff function was added.  For DBAJ the reason code used is WO ALL.
    //    This will create an adjustment bringing the debt detail balance to 
    // zero.
    //    It will also set collections to manual, and the debt adjustment 
    // process
    //    date to current date.
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
    local.NothingToWriteOff.Flag = "N";
    local.Current.Timestamp = Now();
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // =================================================
    // 12/3/98 - B Adams  -  The CLEAR function was leaving a
    //   blank screen.  All protected fields should be left populated,
    //   but none of the hidden fields.  The hardcode cab does not
    //   have to execute for CLEAR to work.
    // =================================================
    export.PassedInObligationType.Assign(import.PassedObligationType);
    export.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    export.AdjDebt.Assign(import.AdjDebt);
    export.Debt.Assign(import.Debt);
    export.CsePerson.Assign(import.CsePerson);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.Supported.Assign(import.Supported);
    export.HiddenLegalActionPerson.Identifier =
      import.HiddenLegalActionPerson.Identifier;
    export.Previous.Command = import.Previous.Command;
    export.ScreenOwedAmounts.TotalAmountOwed =
      import.ScreenOwedAmounts.TotalAmountOwed;
    MoveObligation(import.PassedObligation, export.PassedInObligation);
    MoveObligationTransaction1(import.PassedObligationTransaction,
      export.PassedInObligationTransaction);
    export.CollProtectionExists.Flag = import.CollProtectionExists.Flag;
    export.ConfirmWriteoff.Flag = "N";

    // : Sept 9, 1999, mfb: Previous command is used to check for PF5 pressed 
    // more than once.
    if (!Equal(global.Command, "ADD"))
    {
      export.Previous.Command = "";
    }

    if (Equal(global.Command, "CLEAR"))
    {
      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.ObligationTransaction.Assign(
          import.Group.Item.ObligationTransaction);
        export.Group.Update.ObligationTransactionRln.Description =
          import.Group.Item.ObligationTransactionRln.Description;
        export.Group.Update.ObligationTransactionRlnRsn.Code =
          import.Group.Item.ObligationTransactionRlnRsn.Code;
        export.Group.Update.Type1.SelectChar =
          import.Group.Item.Type1.SelectChar;
        export.Group.Next();
      }

      ExitState = "ACO_NI0000_FIELDS_CLEARED";

      return;
    }

    UseFnHardcodedDebtDistribution();
    local.HardcodeWriteoffNa.SystemGeneratedIdentifier = 6;
    local.HardcodeWriteoffAll.SystemGeneratedIdentifier = 8;
    local.HardcodeReinstate.SystemGeneratedIdentifier = 7;
    local.HardCaseClosed.SystemGeneratedIdentifier = 9;
    local.HardcodeWriteoffAf.SystemGeneratedIdentifier = 10;
    local.HardcodeIncAdj.SystemGeneratedIdentifier = 11;
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MoveObligation(import.PassedObligation, export.PassedInObligation);
    MoveObligationTransaction1(import.PassedObligationTransaction,
      export.PassedInObligationTransaction);

    if (Equal(global.Command, "DBAJ"))
    {
      local.OriginalCommand.Command = "DBAJ";

      if (!IsEmpty(export.Supported.Number))
      {
        UseEabReadCsePerson1();
        UseSiFormatCsePersonName1();
      }
    }
    else
    {
      MoveObligationTransaction3(import.DebtAdjustment, export.DebtAdjustment);
      export.EditedDebtAdjustType.SelectChar =
        import.EditedDebtAdjustType.SelectChar;
      MoveObligationTransactionRln(import.ObligationTransactionRln,
        export.ObligationTransactionRln);
      MoveObligationTransactionRlnRsn(import.ObligationTransactionRlnRsn,
        export.ObligationTransactionRlnRsn);

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.ObligationTransaction.Assign(
          import.Group.Item.ObligationTransaction);
        export.Group.Update.ObligationTransactionRln.Description =
          import.Group.Item.ObligationTransactionRln.Description;
        export.Group.Update.ObligationTransactionRlnRsn.Code =
          import.Group.Item.ObligationTransactionRlnRsn.Code;
        export.Group.Update.Type1.SelectChar =
          import.Group.Item.Type1.SelectChar;
        export.Group.Next();
      }
    }

    if (Equal(global.Command, "RETLINK"))
    {
      var field = GetField(export.ObligationTransactionRlnRsn, "code");

      field.Color = "green";
      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;

      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
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
      ExitState = "FN0000_NEXTTRAN_INFO_INVALID";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (!Equal(global.Command, "COLP"))
    {
      // : validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
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
      case "DBAJ":
        // Processing at bottom of PrAD
        break;
      case "ADD":
        // =================================================
        // 09/09/99 - mfb:  Problem report 72838
        // Add an edit to prevent processing the add again when PF5 is
        // pressed multiple times.
        // =================================================
        if (Equal(export.Previous.Command, global.Command))
        {
          ExitState = "FN0000_PF5_PRESSD_MULTIPLE_TIMES";

          return;
        }

        // Jan, 2002, M. Brown, PR# 128572: Moved this edit from below the edits
        // following.
        // When the user entered 'WR ALL' instead of 'WO ALL', unnecessary edits
        // were being done.
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

          ExitState = "FN0000_OBLIG_TRANS_RLN_RSN_NF";

          return;
        }

        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffNa.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_USE_WO_ALL_RC_FOR_DBAJ";

          var field = GetField(export.ObligationTransactionRlnRsn, "code");

          field.Error = true;

          return;
        }

        // =================================================
        // 12/3/98 - B Adams  -  All edits for blank fields done first and
        //   together.  In reverse order of appearance so that the cursor
        //   ends up on the first blank field.
        // =================================================
        if (IsEmpty(export.ObligationTransactionRln.Description))
        {
          var field = GetField(export.ObligationTransactionRln, "description");

          field.Color = "red";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = false;
          field.Focused = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }

        if (IsEmpty(export.ObligationTransactionRlnRsn.Code))
        {
          var field = GetField(export.ObligationTransactionRlnRsn, "code");

          field.Color = "red";
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = false;
          field.Focused = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }

        // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
        if (Equal(export.ObligationTransactionRlnRsn.Code, "WO ALL") || Equal
          (export.ObligationTransactionRlnRsn.Code, "WO NA") || Equal
          (export.ObligationTransactionRlnRsn.Code, "REINST") || Equal
          (export.ObligationTransactionRlnRsn.Code, "CLOSECA") || Equal
          (export.ObligationTransactionRlnRsn.Code, "WO AF"))
        {
          // mfb: if adjustment reason code is writeoff or reinstate, and it's 
          // an accruing obligation, must go to DBWR.
          if (AsChar(export.PassedInObligationType.Classification) == 'A')
          {
            ExitState = "FN0000_DBWR_TO_WRITEOFF_ACCRUING";

            return;
          }
        }
        else
        {
          // : These edits don't need to be done for reinstate and writeoff.
          if (IsEmpty(export.EditedDebtAdjustType.SelectChar))
          {
            var field = GetField(export.EditedDebtAdjustType, "selectChar");

            field.Color = "red";
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
            field.Focused = true;

            ExitState = "FN0000_MANDATORY_FIELDS";
          }

          if (export.DebtAdjustment.Amount == 0)
          {
            var field = GetField(export.DebtAdjustment, "amount");

            field.Color = "red";
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
            field.Focused = true;

            ExitState = "FN0000_MANDATORY_FIELDS";
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }

        if (import.PassedObligation.SystemGeneratedIdentifier == 0 || import
          .PassedObligationTransaction.SystemGeneratedIdentifier == 0 || IsEmpty
          (import.CsePerson.Number) || IsEmpty
          (import.PassedObligationType.Code))
        {
          ExitState = "FN0000_INSUFFICIENT_OBLIGATION";

          return;
        }

        // : January, 2002 - M Brown - Work Order Number: 020199 -
        //   Added Closed Case reason code to this IF stmt.
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffAll.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardCaseClosed.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffAf.SystemGeneratedIdentifier)
        {
          // : User is not required to enter adjustment type for writeoff.  Set 
          // it here.
          export.DebtAdjustment.Amount = export.AdjDebt.BalanceDueAmt;
          export.DebtAdjustment.DebtAdjustmentType =
            local.HardcodeOtrnDatDecrease.DebtAdjustmentType;
        }
        else if (entities.ObligationTransactionRlnRsn.
          SystemGeneratedIdentifier == local
          .HardcodeReinstate.SystemGeneratedIdentifier)
        {
          // : User is not required to enter adjustment type for reinstate.  Set
          // it here.
          export.DebtAdjustment.DebtAdjustmentType =
            local.HardcodeOtrnDatIncrease.DebtAdjustmentType;
        }
        else
        {
          switch(AsChar(import.EditedDebtAdjustType.SelectChar))
          {
            case '+':
              export.DebtAdjustment.DebtAdjustmentType =
                local.HardcodeOtrnDatIncrease.DebtAdjustmentType;

              break;
            case '-':
              export.DebtAdjustment.DebtAdjustmentType =
                local.HardcodeOtrnDatDecrease.DebtAdjustmentType;

              // *********************************************************************
              // Verify amount to be decreased is not more than the adj amt due.
              // *********************************************************************
              if (export.DebtAdjustment.Amount > export.AdjDebt.BalanceDueAmt)
              {
                var field1 = GetField(export.DebtAdjustment, "amount");

                field1.Error = true;

                ExitState = "FN0206_DEBT_ADJ_EXCEEDS_DEBT";

                return;
              }

              break;
            default:
              var field = GetField(export.EditedDebtAdjustType, "selectChar");

              field.Error = true;

              ExitState = "FN0000_PLUS_OR_MINUS_ONLY";

              return;
          }
        }

        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeReinstate.SystemGeneratedIdentifier)
        {
          if (AsChar(import.PassedObligation.PrimarySecondaryCode) == 'S')
          {
            // : A secondary obligation may not be selected for reinstate.
            ExitState = "FN0000_CANT_REINSTATE_SECONDARY";

            return;
          }
        }

        // : MFB - Writeoff edits
        // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffAll.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardCaseClosed.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffAf.SystemGeneratedIdentifier)
        {
          if (AsChar(import.PassedObligation.PrimarySecondaryCode) == 'S')
          {
            // : A secondary obligation may not be selected for write-off.
            ExitState = "FN0000_CANT_WRITEOFF_SECONDARY";

            return;
          }

          if (AsChar(import.ConfirmWriteoff.Flag) == 'Y')
          {
            // : Write-off was already confirmed.
          }
          else
          {
            // : Write-off needs to be confirmed.
            // changed ob trans rln description to unprotected
            // for ticket cq66291  A Hockman
            var field1 =
              GetField(export.ObligationTransactionRln, "description");

            field1.Color = "cyan";
            field1.Protected = false;

            var field2 = GetField(export.EditedDebtAdjustType, "selectChar");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.ObligationTransactionRlnRsn, "code");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Prompt, "selectChar");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.DebtAdjustment, "amount");

            field5.Color = "cyan";
            field5.Protected = true;

            ExitState = "FN0000_CONFIRM_WRITEOFF";
            export.ConfirmWriteoff.Flag = "Y";

            return;
          }
        }

        UseFnApplyDebtAdjustment3();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
        {
          // : Reset exitstate for possible further processing of j/s or p/s 
          // debt.
          ExitState = "ACO_NN0000_ALL_OK";
          local.NothingToWriteOff.Flag = "Y";
        }
        else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
        {
          // : Reset exitstate for possible further processing of j/s or p/s 
          // debt.
          ExitState = "ACO_NN0000_ALL_OK";
          local.NothingToWriteOff.Flag = "Y";
        }
        else
        {
          UseEabRollbackCics();
          export.DebtAdjustment.CreatedBy = "";
          export.DebtAdjustment.CreatedTmst = null;

          return;
        }

        // : Sept 9, 1999, mfb - Added previous command for problem report 
        // asking for an edit
        //   to prevent pressing PF5 add twice in a row.
        export.Previous.Command = global.Command;

        // : JOINT AND SEVERAL AND PRIMARY SECONDARY PROCESSING.
        if (!IsEmpty(export.PassedInObligation.PrimarySecondaryCode))
        {
          if (!ReadObligation1())
          {
            ExitState = "FN0000_OBLIGATION_NF";
            UseEabRollbackCics();
            export.DebtAdjustment.CreatedBy = "";
            export.DebtAdjustment.CreatedTmst = null;

            return;
          }
        }

        // =================================================
        // 3/18/1999 - Bud Adams  -  If an adjusted obligation is a Joint
        //   and Several, then the related obligation must also be
        //   adjusted by the identical amount in the identical manner.
        //   First we need to try to read the other obligation as if the
        //   one we just processed was the FIRST one related to
        //   Obligation_Rln; and if that doesn't work, then try to read it
        //   as if it was the SECOND one.
        // =================================================
        // : JOINT AND SEVERAL PROCESSING.
        if (AsChar(export.PassedInObligation.PrimarySecondaryCode) == AsChar
          (local.HcObligJointSeveralConcurren.PrimarySecondaryCode))
        {
          if (export.HiddenLegalActionPerson.Identifier == 0)
          {
            if (!ReadObligationObligationTransactionCsePersonDebtDetail4())
            {
              if (!ReadObligationObligationTransactionCsePersonDebtDetail3())
              {
                ExitState = "FN0000_JOINT_SEVERAL_DEBT_NF_RB";

                goto Test1;
              }
            }
          }
          else
          {
            // =================================================
            // 4/23/99 - bud adams  -  When there are more than one
            //   supported person associated with an obligation, there
            //   are more than obligation_transaction created: one for each
            //   person.
            // =================================================
            if (!ReadObligationObligationTransactionCsePersonDebtDetail2())
            {
              if (!ReadObligationObligationTransactionCsePersonDebtDetail1())
              {
                ExitState = "FN0000_JOINT_SEVERAL_DEBT_NF_RB";

                goto Test1;
              }
            }
          }

          UseFnApplyDebtAdjustment1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
          {
          }
          else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
          {
          }
          else
          {
            UseEabRollbackCics();
            export.DebtAdjustment.CreatedBy = "";
            export.DebtAdjustment.CreatedTmst = null;

            return;
          }
        }

Test1:

        // : PRIMARY/SECONDARY PROCESSING
        // April, 2000, M.Brown - If primary/secondary, and this is a write-off 
        // or
        // reinstate, we need to call the adjustment cab again to handle the '
        // other'
        // obligation.
        // : January, 2002 - M Brown - Work Order Number: 020199 - Closed Case.
        if ((AsChar(export.PassedInObligation.PrimarySecondaryCode) == 'P' || AsChar
          (export.PassedInObligation.PrimarySecondaryCode) == 'S') && (
            entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeWriteoffAll.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardCaseClosed.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == local
          .HardcodeReinstate.SystemGeneratedIdentifier))
        {
          if (AsChar(export.PassedInObligationType.SupportedPersonReqInd) == 'Y'
            )
          {
            if (ReadSupported())
            {
              // : These reads are with a supported person qualifier.
              if (AsChar(export.PassedInObligation.PrimarySecondaryCode) == 'P')
              {
                if (!ReadObligationObligationTransaction2())
                {
                  // : This is ok - no debt is set up for the other obligation.
                  goto Test2;
                }
              }
              else if (AsChar(export.PassedInObligation.PrimarySecondaryCode) ==
                'S')
              {
                if (!ReadObligationObligationTransaction1())
                {
                  // : This is ok - no debt is set up for the other obligation.
                  goto Test2;
                }
              }
            }
            else
            {
              // : Rollback will be handled in exitstate logic outside this 
              // block of code.
              ExitState = "SUPPORTED_PERSON_NF_RB";

              goto Test2;
            }
          }
          else
          {
            // : These reads are without a supported person qualifier.
            if (AsChar(export.PassedInObligation.PrimarySecondaryCode) == 'P')
            {
              if (!ReadObligationObligationTransaction4())
              {
                // : This is ok - no debt is set up for the other obligation.
                goto Test2;
              }
            }
            else if (AsChar(export.PassedInObligation.PrimarySecondaryCode) == 'S'
              )
            {
              if (!ReadObligationObligationTransaction3())
              {
                // : This is ok - no debt is set up for the other obligation.
                goto Test2;
              }
            }
          }

          UseFnApplyDebtAdjustment2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
          {
          }
          else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
          {
          }
          else if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
          {
          }
          else
          {
            UseEabRollbackCics();
            export.DebtAdjustment.CreatedBy = "";
            export.DebtAdjustment.CreatedTmst = null;

            return;
          }
        }

Test2:

        // : Debt Adjustment processing complete.  Set up final message for the 
        // adjustment.
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(local.NothingToWriteOff.Flag) == 'Y')
          {
            ExitState = "FN0000_NOTHING_TO_WRITE_OFF";
          }
          else
          {
            ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
          }
        }
        else if (IsExitState("FN0000_NOTHING_TO_WRITE_OFF"))
        {
          // : this means that the 'other' obligation debt detail balance was 
          // zero
          //   on a writeoff of a p/s obligation.  (and manual collections were 
          // backed off).
          ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
        }
        else if (IsExitState("FN0000_NO_BALANCE_TO_WRITE_OFF"))
        {
          // : this means that the 'other' obligation debt detail balance was 
          // zero
          //   on a writeoff of a p/s obligation.  (and no manual collections 
          // were backed off).
          ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
        }
        else if (IsExitState("FN0000_WRITE_OFF_DEBT_ADJ_NF"))
        {
          // : During reinstate, a writeoff was not found for the 'other' 
          // obligation.
          ExitState = "FN0000_DEBT_ADJUSTMENT_OK";
        }
        else
        {
          UseEabRollbackCics();
          export.DebtAdjustment.CreatedBy = "";
          export.DebtAdjustment.CreatedTmst = null;

          return;
        }

        // =================================================
        // 1/28/99 - B Adams  -  deleted USE fn_summ_refresh_obligor_buckets
        //   IAW debt-adjustment meeting held 1/27/99.  Notes published
        //   by Darrin Greene, found in DBAJ file.
        //   Also deleted a related USE of 'validate-code-value' CAB.
        // =================================================
        global.Command = "DBAJ";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        return;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

    // ***---  added 'manual-colls-reversed'
    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("FN0000_DEBT_ADJUSTMENT_OK") || IsExitState
      ("FN0000_MANUAL_COLLS_REVERSED"))
    {
      // --- continue processing
    }
    else
    {
      return;
    }

    if (Equal(global.Command, "DBAJ"))
    {
      if (import.PassedObligation.SystemGeneratedIdentifier == 0 || import
        .PassedObligationTransaction.SystemGeneratedIdentifier == 0 || IsEmpty
        (import.CsePerson.Number) || IsEmpty(import.PassedObligationType.Code))
      {
        ExitState = "FN0000_INSUFFICIENT_OBLIGATION";

        return;
      }

      if (ReadObligationType())
      {
        export.PassedInObligationType.Assign(entities.ObligationType);
      }
      else
      {
        var field = GetField(export.PassedInObligationType, "code");

        field.Error = true;

        ExitState = "FN0000_OBLIG_TYPE_NF";

        return;
      }

      if (ReadCsePersonAccountCsePerson())
      {
        export.CsePerson.Assign(entities.CsePerson);
        export.CsePersonsWorkSet.Number = entities.CsePerson.Number;

        switch(AsChar(entities.CsePerson.Type1))
        {
          case 'C':
            // **** Client
            UseEabReadCsePerson2();
            UseSiFormatCsePersonName2();

            break;
          case 'O':
            // **** Organization
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);

            break;
          default:
            ExitState = "FN0000_CSE_PERSON_TYPE";

            return;
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGOR_NF";

        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        return;
      }

      if (ReadObligation2())
      {
        MoveObligation(entities.Obligation, export.PassedInObligation);
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";

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

      if (Equal(import.PassedObligationTransaction.Type1, "DE"))
      {
        // **** Incoming Debt Transaction, user is allowed to make debt 
        // adjustment
        // =================================================
        // 4/1/99 - b adams  -  Read improperly qualified
        // =================================================
        if (ReadObligationTransaction())
        {
          MoveObligationTransaction2(entities.Debt, export.Debt);
          export.AdjDebt.BalanceDueAmt = entities.Debt.Amount;

          if (Equal(local.OriginalCommand.Command, "DBAJ"))
          {
            // =================================================
            // 3/2/1999 - bud adams  -  Exit state indicating manually
            //   distributed collections have been applied to this debt detail
            //   but only do this when flowing from DEBT, not after actually
            //   doing an adjustment.
            // =================================================
            if (ReadCollection())
            {
              ExitState = "FN0000_MANUAL_DIST_COLLCTNS_EXIS";
            }
            else
            {
              // OK
            }

            // =================================================
            // 4/23/99 - bud adams  -  In order to properly identify the
            //   correct partner Obligation_Transaction in the case of a
            //   Joint and Several obligation is to nail down the associated
            //   Legal_Action_Person.
            // =================================================
            if (AsChar(entities.Obligation.PrimarySecondaryCode) == AsChar
              (local.HcObligJointSeveralConcurren.PrimarySecondaryCode))
            {
              if (ReadLegalActionPerson())
              {
                export.HiddenLegalActionPerson.Identifier =
                  entities.LegalActionPerson.Identifier;
              }
              else
              {
                // ***---  OK; optional relationship
              }
            }
          }
        }
        else
        {
          ExitState = "FN0000_OBLIG_TRANS_NF";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_INVALID_TRANSACTION_TYPE";

        return;
      }

      export.Group.Index = 0;
      export.Group.Clear();

      foreach(var item in ReadObligationTransactionObligationTransactionRln())
      {
        // =================================================
        // 3/2/1999 - b adams  -  removed read of ob-tran-rln-rsn and
        //   included it in Read Each.  RI enforces the existenc of that
        //   record
        // =================================================
        export.Group.Update.ObligationTransaction.
          Assign(entities.DebtAdjustment);

        if (AsChar(entities.DebtAdjustment.DebtAdjustmentType) == AsChar
          (local.HardcodeOtrnDatIncrease.DebtAdjustmentType))
        {
          export.Group.Update.Type1.SelectChar = "+";
          export.AdjDebt.BalanceDueAmt += entities.DebtAdjustment.Amount;
        }
        else
        {
          export.Group.Update.Type1.SelectChar = "-";
          export.AdjDebt.BalanceDueAmt -= entities.DebtAdjustment.Amount;
        }

        export.Group.Update.ObligationTransactionRln.Description =
          entities.ObligationTransactionRln.Description;
        export.Group.Update.ObligationTransactionRlnRsn.Code =
          entities.ObligationTransactionRlnRsn.Code;
        export.Group.Next();
      }

      if (ReadDebtDetail())
      {
        export.AdjDebt.DueDt = entities.DebtDetail.DueDt;

        // =================================================
        // PR# 80976: 11/30/99 - b adams  -  Debt Detail Balance Owed
        //   added to screen.
        // =================================================
        export.ScreenOwedAmounts.TotalAmountOwed =
          entities.DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
      else
      {
        ExitState = "FN0211_DEBT_DETAIL_NF";
      }
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
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
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction3(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.DebtAdjustmentType = source.DebtAdjustmentType;
    target.DebtAdjustmentDt = source.DebtAdjustmentDt;
    target.DebtAdjustmentProcessDate = source.DebtAdjustmentProcessDate;
    target.ReverseCollectionsInd = source.ReverseCollectionsInd;
  }

  private static void MoveObligationTransaction4(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtAdjustmentType = source.DebtAdjustmentType;
  }

  private static void MoveObligationTransaction5(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationTransactionRln(
    ObligationTransactionRln source, ObligationTransactionRln target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
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

  private void UseEabReadCsePerson1()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Supported.Number;
    MoveCsePersonsWorkSet(export.Supported, useExport.CsePersonsWorkSet);

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Supported);
  }

  private void UseEabReadCsePerson2()
  {
    var useImport = new EabReadCsePerson.Import();
    var useExport = new EabReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveCsePersonsWorkSet(export.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(EabReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
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

    useImport.CsePerson.Number = entities.JointAndSeveralCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      entities.JointAndSeveralOrPsObligationTransaction.
        SystemGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.HcCpaObligor.Type1 = local.HardcodeCpaObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HardcodeDebtAdjustment.Type1 =
      local.HardcodeOtrnTDebtAdjustment.Type1;
    useImport.HardcodeActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.PassedInObligationType.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDdshDeactivatedStatus.Code;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAf.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnApplyDebtAdjustment2()
  {
    var useImport = new FnApplyDebtAdjustment.Import();
    var useExport = new FnApplyDebtAdjustment.Export();

    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      entities.JointAndSeveralOrPsObligationTransaction.
        SystemGeneratedIdentifier;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDdshDeactivatedStatus.Code;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.Max.Date = local.Max.Date;
    useImport.HcCpaObligor.Type1 = local.HardcodeCpaObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HardcodeDebtAdjustment.Type1 =
      local.HardcodeOtrnTDebtAdjustment.Type1;
    useImport.HardcodeActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.PassedInObligationType.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAf.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnApplyDebtAdjustment3()
  {
    var useImport = new FnApplyDebtAdjustment.Import();
    var useExport = new FnApplyDebtAdjustment.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.HcCpaObligor.Type1 = local.HardcodeCpaObligor.Type1;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HardcodeDebtAdjustment.Type1 =
      local.HardcodeOtrnTDebtAdjustment.Type1;
    useImport.HardcodeActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.PassedInObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.PassedInObligationType.SystemGeneratedIdentifier;
    useImport.ObligationTransactionRln.Description =
      export.ObligationTransactionRln.Description;
    useImport.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
      export.ObligationTransactionRlnRsn.SystemGeneratedIdentifier;
    useImport.Debt.SystemGeneratedIdentifier =
      export.Debt.SystemGeneratedIdentifier;
    useImport.DebtAdjustment.Assign(export.DebtAdjustment);
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.HardcodeDeactivatedStatus.Code =
      local.HardcodeDdshDeactivatedStatus.Code;
    useImport.HardcodeCloseCase.SystemGeneratedIdentifier =
      local.HardCaseClosed.SystemGeneratedIdentifier;
    useImport.CollProtExists.Flag = export.CollProtectionExists.Flag;
    useImport.HardcodeReinstate.SystemGeneratedIdentifier =
      local.HardcodeReinstate.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier =
      local.HardcodeWriteoffNa.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAll.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAll.SystemGeneratedIdentifier;
    useImport.HardcodeIncAdj.SystemGeneratedIdentifier =
      local.HardcodeIncAdj.SystemGeneratedIdentifier;
    useImport.HardcodeWriteoffAf.SystemGeneratedIdentifier =
      local.HardcodeWriteoffAf.SystemGeneratedIdentifier;

    Call(FnApplyDebtAdjustment.Execute, useImport, useExport);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HcObligJointSeveralConcurren.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    MoveObligationTransaction4(useExport.OtrnDatIncrease,
      local.HardcodeOtrnDatIncrease);
    MoveObligationTransaction4(useExport.OtrnDatDecrease,
      local.HardcodeOtrnDatDecrease);
    local.HardcodeCpaObligor.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeOtrnTDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodeOtrnTDebtAdjustment.Type1 =
      useExport.OtrnTDebtAdjustment.Type1;
    local.HardcodeDdshActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeOrrJointSeveral.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
    local.HardcodeOrrPrimarySecondary.SequentialGeneratedIdentifier =
      useExport.OrrPrimarySecondary.SequentialGeneratedIdentifier;
    local.HcObligSecondaryConcurrent.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
    local.HcObligPrimaryConcurrent.PrimarySecondaryCode =
      useExport.ObligPrimaryConcurrent.PrimarySecondaryCode;
    local.HardcodeDdshDeactivatedStatus.Code =
      useExport.DdshDeactivedStatus.Code;
    local.HardcodeConcurrent.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    MoveObligationTransaction5(useExport.OtrnDtDebtDetail,
      local.HardcodeOtrnDtDebtDetail);
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
    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);

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

    useImport.LegalAction.StandardNumber = import.LegalAction.StandardNumber;
    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiFormatCsePersonName1()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.Supported);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.Supported.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseSiFormatCsePersonName2()
  {
    var useImport = new SiFormatCsePersonName.Import();
    var useExport = new SiFormatCsePersonName.Export();

    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);

    Call(SiFormatCsePersonName.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Debt.OtyType);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(command, "otrId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.DistributionMethod = db.GetString(reader, 12);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
      });
  }

  private bool ReadCsePersonAccountCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccountCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
        db.SetString(command, "type", local.HardcodeCpaObligor.Type1);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.CsePersonAccount.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 3);
        entities.CsePerson.Type1 = db.GetString(reader, 4);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 5);
        entities.CsePerson.Populated = true;
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
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
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalActionPerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.LegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "laPersonId", entities.Debt.LapId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.Populated = true;
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
          local.Initialized.Date.GetValueOrDefault());
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
          local.Initialized.Date.GetValueOrDefault());
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
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", export.PassedInObligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          export.PassedInObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", import.PassedObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Supported1.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransaction1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetNullableString(command, "cpaSupType", entities.Supported1.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported1.CspNumber);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.Supported1.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetNullableString(command, "cpaSupType", entities.Supported1.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.Supported1.CspNumber);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationObligationTransaction3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransaction3",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationObligationTransaction4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransaction4",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
      });
  }

  private bool ReadObligationObligationTransactionCsePersonDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralDebtDetail.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransactionCsePersonDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cpaFType", local.HardcodeCpaObligor.Type1);
        db.SetNullableInt32(
          command, "lapId", export.HiddenLegalActionPerson.Identifier);
        db.SetDate(command, "dueDt", export.AdjDebt.DueDt.GetValueOrDefault());
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralDebtDetail.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralDebtDetail.CspNumber = db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralDebtDetail.CpaType = db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralDebtDetail.OtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralDebtDetail.OtrType = db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralDebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 17);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 18);
        entities.JointAndSeveralCsePerson.OrganizationName =
          db.GetNullableString(reader, 19);
        entities.JointAndSeveralDebtDetail.DueDt = db.GetDate(reader, 20);
        entities.JointAndSeveralDebtDetail.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType",
          entities.JointAndSeveralDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType",
          entities.JointAndSeveralDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
      });
  }

  private bool ReadObligationObligationTransactionCsePersonDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralDebtDetail.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransactionCsePersonDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetNullableInt32(
          command, "lapId", export.HiddenLegalActionPerson.Identifier);
        db.SetDate(command, "dueDt", export.AdjDebt.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralDebtDetail.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralDebtDetail.CspNumber = db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralDebtDetail.CpaType = db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralDebtDetail.OtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralDebtDetail.OtrType = db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralDebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 17);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 18);
        entities.JointAndSeveralCsePerson.OrganizationName =
          db.GetNullableString(reader, 19);
        entities.JointAndSeveralDebtDetail.DueDt = db.GetDate(reader, 20);
        entities.JointAndSeveralDebtDetail.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType",
          entities.JointAndSeveralDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType",
          entities.JointAndSeveralDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
      });
  }

  private bool ReadObligationObligationTransactionCsePersonDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralDebtDetail.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransactionCsePersonDebtDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cpaFType", local.HardcodeCpaObligor.Type1);
        db.SetDate(command, "dueDt", export.AdjDebt.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralDebtDetail.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralDebtDetail.CspNumber = db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralDebtDetail.CpaType = db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralDebtDetail.OtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralDebtDetail.OtrType = db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralDebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 17);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 18);
        entities.JointAndSeveralCsePerson.OrganizationName =
          db.GetNullableString(reader, 19);
        entities.JointAndSeveralDebtDetail.DueDt = db.GetDate(reader, 20);
        entities.JointAndSeveralDebtDetail.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType",
          entities.JointAndSeveralDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType",
          entities.JointAndSeveralDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
      });
  }

  private bool ReadObligationObligationTransactionCsePersonDebtDetail4()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.JointAndSeveralDebtDetail.Populated = false;
    entities.JointAndSeveralCsePerson.Populated = false;
    entities.JointAndSeveralOrPsObligation.Populated = false;
    entities.JointAndSeveralOrPsObligationTransaction.Populated = false;

    return Read("ReadObligationObligationTransactionCsePersonDebtDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaFType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
        db.SetString(command, "cpaType", local.HardcodeCpaObligor.Type1);
        db.SetDate(command, "dueDt", export.AdjDebt.DueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.JointAndSeveralOrPsObligation.CpaType =
          db.GetString(reader, 0);
        entities.JointAndSeveralOrPsObligation.CspNumber =
          db.GetString(reader, 1);
        entities.JointAndSeveralOrPsObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.JointAndSeveralOrPsObligation.DtyGeneratedId =
          db.GetInt32(reader, 3);
        entities.JointAndSeveralOrPsObligationTransaction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralDebtDetail.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.JointAndSeveralOrPsObligationTransaction.CspNumber =
          db.GetString(reader, 5);
        entities.JointAndSeveralDebtDetail.CspNumber = db.GetString(reader, 5);
        entities.JointAndSeveralOrPsObligationTransaction.CpaType =
          db.GetString(reader, 6);
        entities.JointAndSeveralDebtDetail.CpaType = db.GetString(reader, 6);
        entities.JointAndSeveralOrPsObligationTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.JointAndSeveralDebtDetail.OtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.JointAndSeveralOrPsObligationTransaction.Type1 =
          db.GetString(reader, 8);
        entities.JointAndSeveralDebtDetail.OtrType = db.GetString(reader, 8);
        entities.JointAndSeveralOrPsObligationTransaction.Amount =
          db.GetDecimal(reader, 9);
        entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd =
          db.GetString(reader, 10);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.JointAndSeveralOrPsObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.JointAndSeveralOrPsObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 13);
        entities.JointAndSeveralOrPsObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 14);
        entities.JointAndSeveralOrPsObligationTransaction.OtyType =
          db.GetInt32(reader, 15);
        entities.JointAndSeveralDebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.JointAndSeveralOrPsObligationTransaction.LapId =
          db.GetNullableInt32(reader, 16);
        entities.JointAndSeveralCsePerson.Number = db.GetString(reader, 17);
        entities.JointAndSeveralCsePerson.Type1 = db.GetString(reader, 18);
        entities.JointAndSeveralCsePerson.OrganizationName =
          db.GetNullableString(reader, 19);
        entities.JointAndSeveralDebtDetail.DueDt = db.GetDate(reader, 20);
        entities.JointAndSeveralDebtDetail.Populated = true;
        entities.JointAndSeveralCsePerson.Populated = true;
        entities.JointAndSeveralOrPsObligation.Populated = true;
        entities.JointAndSeveralOrPsObligationTransaction.Populated = true;
        CheckValid<Obligation>("CpaType",
          entities.JointAndSeveralOrPsObligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType",
          entities.JointAndSeveralDebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.JointAndSeveralOrPsObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType",
          entities.JointAndSeveralDebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.JointAndSeveralOrPsObligationTransaction.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.JointAndSeveralOrPsObligationTransaction.CpaSupType);
        CheckValid<CsePerson>("Type1", entities.JointAndSeveralCsePerson.Type1);
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          import.PassedObligationTransaction.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "obTrnTyp", local.HardcodeOtrnTDebt.Type1);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.DebtAdjustmentInd = db.GetString(reader, 6);
        entities.Debt.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Debt.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Debt.DebtType = db.GetString(reader, 9);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 10);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 11);
        entities.Debt.OtyType = db.GetInt32(reader, 12);
        entities.Debt.LapId = db.GetNullableInt32(reader, 13);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.Debt.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private IEnumerable<bool> ReadObligationTransactionObligationTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);

    return ReadEach("ReadObligationTransactionObligationTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(
          command, "obTrnTyp", local.HardcodeOtrnTDebtAdjustment.Type1);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentInd = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 7);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 9);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 10);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 11);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 12);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 13);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 13);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 14);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 15);
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 16);
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 17);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 18);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 19);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 20);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 21);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 22);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 23);
        entities.ObligationTransactionRln.Description =
          db.GetNullableString(reader, 24);
        entities.ObligationTransactionRlnRsn.Code = db.GetString(reader, 25);
        entities.ObligationTransactionRln.Populated = true;
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransactionRln>("CpaType",
          entities.ObligationTransactionRln.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransactionRln>("OtrType",
          entities.ObligationTransactionRln.OtrType);
        CheckValid<ObligationTransaction>("DebtAdjustmentInd",
          entities.DebtAdjustment.DebtAdjustmentInd);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransactionRln>("OtrPType",
          entities.ObligationTransactionRln.OtrPType);
        CheckValid<ObligationTransactionRln>("CpaPType",
          entities.ObligationTransactionRln.CpaPType);

        return true;
      });
  }

  private bool ReadObligationTransactionRlnRsn()
  {
    entities.ObligationTransactionRlnRsn.Populated = false;

    return Read("ReadObligationTransactionRlnRsn",
      (db, command) =>
      {
        db.SetString(
          command, "obTrnRlnRsnCd", import.ObligationTransactionRlnRsn.Code);
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
          import.PassedObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 3);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadSupported()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadSupported",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnId",
          export.PassedInObligationTransaction.SystemGeneratedIdentifier);
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Supported1.CspNumber = db.GetString(reader, 0);
        entities.Supported1.Type1 = db.GetString(reader, 1);
        entities.Supported1.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Supported1.Type1);
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
      /// A value of Type1.
      /// </summary>
      [JsonPropertyName("type1")]
      public Common Type1
      {
        get => type1 ??= new();
        set => type1 = value;
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
      /// A value of ObligationTransactionRln.
      /// </summary>
      [JsonPropertyName("obligationTransactionRln")]
      public ObligationTransactionRln ObligationTransactionRln
      {
        get => obligationTransactionRln ??= new();
        set => obligationTransactionRln = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common type1;
      private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
      private ObligationTransactionRln obligationTransactionRln;
      private ObligationTransaction obligationTransaction;
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
    /// A value of HiddenLegalActionPerson.
    /// </summary>
    [JsonPropertyName("hiddenLegalActionPerson")]
    public LegalActionPerson HiddenLegalActionPerson
    {
      get => hiddenLegalActionPerson ??= new();
      set => hiddenLegalActionPerson = value;
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
    /// A value of PassedObligation.
    /// </summary>
    [JsonPropertyName("passedObligation")]
    public Obligation PassedObligation
    {
      get => passedObligation ??= new();
      set => passedObligation = value;
    }

    /// <summary>
    /// A value of PassedObligationTransaction.
    /// </summary>
    [JsonPropertyName("passedObligationTransaction")]
    public ObligationTransaction PassedObligationTransaction
    {
      get => passedObligationTransaction ??= new();
      set => passedObligationTransaction = value;
    }

    /// <summary>
    /// A value of PassedObligationType.
    /// </summary>
    [JsonPropertyName("passedObligationType")]
    public ObligationType PassedObligationType
    {
      get => passedObligationType ??= new();
      set => passedObligationType = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AdjDebt.
    /// </summary>
    [JsonPropertyName("adjDebt")]
    public DebtDetail AdjDebt
    {
      get => adjDebt ??= new();
      set => adjDebt = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of TotalBalanceDue.
    /// </summary>
    [JsonPropertyName("totalBalanceDue")]
    public Common TotalBalanceDue
    {
      get => totalBalanceDue ??= new();
      set => totalBalanceDue = value;
    }

    /// <summary>
    /// A value of EditedDebtAdjustType.
    /// </summary>
    [JsonPropertyName("editedDebtAdjustType")]
    public Common EditedDebtAdjustType
    {
      get => editedDebtAdjustType ??= new();
      set => editedDebtAdjustType = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of CollProtectionExists.
    /// </summary>
    [JsonPropertyName("collProtectionExists")]
    public Common CollProtectionExists
    {
      get => collProtectionExists ??= new();
      set => collProtectionExists = value;
    }

    private ScreenOwedAmounts screenOwedAmounts;
    private LegalActionPerson hiddenLegalActionPerson;
    private Array<GroupGroup> group;
    private Obligation passedObligation;
    private ObligationTransaction passedObligationTransaction;
    private ObligationType passedObligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private LegalAction legalAction;
    private DebtDetail adjDebt;
    private ObligationTransaction debt;
    private ObligationTransaction debtAdjustment;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet supported;
    private Common totalBalanceDue;
    private Common editedDebtAdjustType;
    private Common nextTransaction;
    private Common prompt;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common previous;
    private Common confirmWriteoff;
    private Common collProtectionExists;
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
      /// A value of Type1.
      /// </summary>
      [JsonPropertyName("type1")]
      public Common Type1
      {
        get => type1 ??= new();
        set => type1 = value;
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
      /// A value of ObligationTransactionRln.
      /// </summary>
      [JsonPropertyName("obligationTransactionRln")]
      public ObligationTransactionRln ObligationTransactionRln
      {
        get => obligationTransactionRln ??= new();
        set => obligationTransactionRln = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common type1;
      private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
      private ObligationTransactionRln obligationTransactionRln;
      private ObligationTransaction obligationTransaction;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of HiddenLegalActionPerson.
    /// </summary>
    [JsonPropertyName("hiddenLegalActionPerson")]
    public LegalActionPerson HiddenLegalActionPerson
    {
      get => hiddenLegalActionPerson ??= new();
      set => hiddenLegalActionPerson = value;
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
    /// A value of PassedInObligation.
    /// </summary>
    [JsonPropertyName("passedInObligation")]
    public Obligation PassedInObligation
    {
      get => passedInObligation ??= new();
      set => passedInObligation = value;
    }

    /// <summary>
    /// A value of PassedInObligationTransaction.
    /// </summary>
    [JsonPropertyName("passedInObligationTransaction")]
    public ObligationTransaction PassedInObligationTransaction
    {
      get => passedInObligationTransaction ??= new();
      set => passedInObligationTransaction = value;
    }

    /// <summary>
    /// A value of PassedInObligationType.
    /// </summary>
    [JsonPropertyName("passedInObligationType")]
    public ObligationType PassedInObligationType
    {
      get => passedInObligationType ??= new();
      set => passedInObligationType = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of AdjDebt.
    /// </summary>
    [JsonPropertyName("adjDebt")]
    public DebtDetail AdjDebt
    {
      get => adjDebt ??= new();
      set => adjDebt = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of EditedDebtAdjustType.
    /// </summary>
    [JsonPropertyName("editedDebtAdjustType")]
    public Common EditedDebtAdjustType
    {
      get => editedDebtAdjustType ??= new();
      set => editedDebtAdjustType = value;
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
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
    }

    /// <summary>
    /// A value of TotalBalanceDue.
    /// </summary>
    [JsonPropertyName("totalBalanceDue")]
    public Common TotalBalanceDue
    {
      get => totalBalanceDue ??= new();
      set => totalBalanceDue = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public Common Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private Common confirmWriteoff;
    private ScreenOwedAmounts screenOwedAmounts;
    private LegalActionPerson hiddenLegalActionPerson;
    private Array<GroupGroup> group;
    private Obligation passedInObligation;
    private ObligationTransaction passedInObligationTransaction;
    private ObligationType passedInObligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private LegalAction legalAction;
    private DebtDetail adjDebt;
    private ObligationTransaction debt;
    private ObligationTransaction debtAdjustment;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet supported;
    private Common editedDebtAdjustType;
    private Common nextTransaction;
    private Common prompt;
    private Common totalBalanceDue;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common previous;
    private Common collProtectionExists;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
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
    /// A value of HardcodeIncAdj.
    /// </summary>
    [JsonPropertyName("hardcodeIncAdj")]
    public ObligationTransactionRlnRsn HardcodeIncAdj
    {
      get => hardcodeIncAdj ??= new();
      set => hardcodeIncAdj = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffAf.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffAf")]
    public ObligationTransactionRlnRsn HardcodeWriteoffAf
    {
      get => hardcodeWriteoffAf ??= new();
      set => hardcodeWriteoffAf = value;
    }

    /// <summary>
    /// A value of HardCaseClosed.
    /// </summary>
    [JsonPropertyName("hardCaseClosed")]
    public ObligationTransactionRlnRsn HardCaseClosed
    {
      get => hardCaseClosed ??= new();
      set => hardCaseClosed = value;
    }

    /// <summary>
    /// A value of NothingToWriteOff.
    /// </summary>
    [JsonPropertyName("nothingToWriteOff")]
    public Common NothingToWriteOff
    {
      get => nothingToWriteOff ??= new();
      set => nothingToWriteOff = value;
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
    /// A value of HardcodeDdshDeactivatedStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDdshDeactivatedStatus")]
    public DebtDetailStatusHistory HardcodeDdshDeactivatedStatus
    {
      get => hardcodeDdshDeactivatedStatus ??= new();
      set => hardcodeDdshDeactivatedStatus = value;
    }

    /// <summary>
    /// A value of HardcodeOrrJointSeveral.
    /// </summary>
    [JsonPropertyName("hardcodeOrrJointSeveral")]
    public ObligationRlnRsn HardcodeOrrJointSeveral
    {
      get => hardcodeOrrJointSeveral ??= new();
      set => hardcodeOrrJointSeveral = value;
    }

    /// <summary>
    /// A value of HardcodeOrrPrimarySecondary.
    /// </summary>
    [JsonPropertyName("hardcodeOrrPrimarySecondary")]
    public ObligationRlnRsn HardcodeOrrPrimarySecondary
    {
      get => hardcodeOrrPrimarySecondary ??= new();
      set => hardcodeOrrPrimarySecondary = value;
    }

    /// <summary>
    /// A value of HcObligSecondaryConcurrent.
    /// </summary>
    [JsonPropertyName("hcObligSecondaryConcurrent")]
    public Obligation HcObligSecondaryConcurrent
    {
      get => hcObligSecondaryConcurrent ??= new();
      set => hcObligSecondaryConcurrent = value;
    }

    /// <summary>
    /// A value of HcObligPrimaryConcurrent.
    /// </summary>
    [JsonPropertyName("hcObligPrimaryConcurrent")]
    public Obligation HcObligPrimaryConcurrent
    {
      get => hcObligPrimaryConcurrent ??= new();
      set => hcObligPrimaryConcurrent = value;
    }

    /// <summary>
    /// A value of HcObligJointSeveralConcurren.
    /// </summary>
    [JsonPropertyName("hcObligJointSeveralConcurren")]
    public Obligation HcObligJointSeveralConcurren
    {
      get => hcObligJointSeveralConcurren ??= new();
      set => hcObligJointSeveralConcurren = value;
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
    /// A value of HardcodeWriteoffNa.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffNa")]
    public ObligationTransactionRlnRsn HardcodeWriteoffNa
    {
      get => hardcodeWriteoffNa ??= new();
      set => hardcodeWriteoffNa = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDatIncrease.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDatIncrease")]
    public ObligationTransaction HardcodeOtrnDatIncrease
    {
      get => hardcodeOtrnDatIncrease ??= new();
      set => hardcodeOtrnDatIncrease = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnDatDecrease.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDatDecrease")]
    public ObligationTransaction HardcodeOtrnDatDecrease
    {
      get => hardcodeOtrnDatDecrease ??= new();
      set => hardcodeOtrnDatDecrease = value;
    }

    /// <summary>
    /// A value of HardcodeCpaObligor.
    /// </summary>
    [JsonPropertyName("hardcodeCpaObligor")]
    public CsePersonAccount HardcodeCpaObligor
    {
      get => hardcodeCpaObligor ??= new();
      set => hardcodeCpaObligor = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnTDebt")]
    public ObligationTransaction HardcodeOtrnTDebt
    {
      get => hardcodeOtrnTDebt ??= new();
      set => hardcodeOtrnTDebt = value;
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
    /// A value of HardcodeDdshActiveStatus.
    /// </summary>
    [JsonPropertyName("hardcodeDdshActiveStatus")]
    public DebtDetailStatusHistory HardcodeDdshActiveStatus
    {
      get => hardcodeDdshActiveStatus ??= new();
      set => hardcodeDdshActiveStatus = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of OriginalCommand.
    /// </summary>
    [JsonPropertyName("originalCommand")]
    public Standard OriginalCommand
    {
      get => originalCommand ??= new();
      set => originalCommand = value;
    }

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
    /// A value of HardcodedWriteoffAf.
    /// </summary>
    [JsonPropertyName("hardcodedWriteoffAf")]
    public ObligationTransactionRlnRsn HardcodedWriteoffAf
    {
      get => hardcodedWriteoffAf ??= new();
      set => hardcodedWriteoffAf = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      hardcodeOtrnDtDebtDetail = null;
      hardcodeIncAdj = null;
      hardcodeWriteoffAf = null;
      hardCaseClosed = null;
      nothingToWriteOff = null;
      hardcodeConcurrent = null;
      hardcodeDdshDeactivatedStatus = null;
      hardcodeOrrJointSeveral = null;
      hardcodeOrrPrimarySecondary = null;
      hcObligSecondaryConcurrent = null;
      hcObligPrimaryConcurrent = null;
      hcObligJointSeveralConcurren = null;
      hardcodeReinstate = null;
      hardcodeWriteoffNa = null;
      hardcodeWriteoffAll = null;
      max = null;
      initialized = null;
      originalCommand = null;
      hardcodedWriteoffAf = null;
    }

    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private ObligationTransactionRlnRsn hardcodeIncAdj;
    private ObligationTransactionRlnRsn hardcodeWriteoffAf;
    private ObligationTransactionRlnRsn hardCaseClosed;
    private Common nothingToWriteOff;
    private ObligationTransactionRlnRsn hardcodeConcurrent;
    private DebtDetailStatusHistory hardcodeDdshDeactivatedStatus;
    private ObligationRlnRsn hardcodeOrrJointSeveral;
    private ObligationRlnRsn hardcodeOrrPrimarySecondary;
    private Obligation hcObligSecondaryConcurrent;
    private Obligation hcObligPrimaryConcurrent;
    private Obligation hcObligJointSeveralConcurren;
    private ObligationTransactionRlnRsn hardcodeReinstate;
    private ObligationTransactionRlnRsn hardcodeWriteoffNa;
    private ObligationTransactionRlnRsn hardcodeWriteoffAll;
    private DateWorkArea max;
    private ObligationTransaction hardcodeOtrnDatIncrease;
    private ObligationTransaction hardcodeOtrnDatDecrease;
    private CsePersonAccount hardcodeCpaObligor;
    private ObligationTransaction hardcodeOtrnTDebt;
    private ObligationTransaction hardcodeOtrnTDebtAdjustment;
    private DebtDetailStatusHistory hardcodeDdshActiveStatus;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private Standard originalCommand;
    private Collection collection;
    private ObligationTransactionRlnRsn hardcodedWriteoffAf;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
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
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
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
    /// A value of JointAndSeveralDebtDetail.
    /// </summary>
    [JsonPropertyName("jointAndSeveralDebtDetail")]
    public DebtDetail JointAndSeveralDebtDetail
    {
      get => jointAndSeveralDebtDetail ??= new();
      set => jointAndSeveralDebtDetail = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of JointAndSeveralCsePerson.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePerson")]
    public CsePerson JointAndSeveralCsePerson
    {
      get => jointAndSeveralCsePerson ??= new();
      set => jointAndSeveralCsePerson = value;
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
    /// A value of JointAndSeveralCsePersonAccount.
    /// </summary>
    [JsonPropertyName("jointAndSeveralCsePersonAccount")]
    public CsePersonAccount JointAndSeveralCsePersonAccount
    {
      get => jointAndSeveralCsePersonAccount ??= new();
      set => jointAndSeveralCsePersonAccount = value;
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
    /// A value of JointAndSeveralOrPsObligation.
    /// </summary>
    [JsonPropertyName("jointAndSeveralOrPsObligation")]
    public Obligation JointAndSeveralOrPsObligation
    {
      get => jointAndSeveralOrPsObligation ??= new();
      set => jointAndSeveralOrPsObligation = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of JointAndSeveralOrPsObligationTransaction.
    /// </summary>
    [JsonPropertyName("jointAndSeveralOrPsObligationTransaction")]
    public ObligationTransaction JointAndSeveralOrPsObligationTransaction
    {
      get => jointAndSeveralOrPsObligationTransaction ??= new();
      set => jointAndSeveralOrPsObligationTransaction = value;
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

    private CsePersonAccount supported1;
    private CsePersonAccount obligor;
    private CsePerson supported2;
    private ObligationRlnRsn obligationRlnRsn;
    private LegalActionPerson legalActionPerson;
    private DebtDetail jointAndSeveralDebtDetail;
    private ObligationRln obligationRln;
    private Collection collection;
    private CsePerson csePerson;
    private CsePerson jointAndSeveralCsePerson;
    private CsePersonAccount csePersonAccount;
    private CsePersonAccount jointAndSeveralCsePersonAccount;
    private Obligation obligation;
    private Obligation jointAndSeveralOrPsObligation;
    private ObligationType obligationType;
    private ObligationTransactionRln obligationTransactionRln;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private ObligationTransaction debtAdjustment;
    private ObligationTransaction jointAndSeveralOrPsObligationTransaction;
    private ObligCollProtectionHist obligCollProtectionHist;
  }
#endregion
}

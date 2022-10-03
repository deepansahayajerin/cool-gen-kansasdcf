// Program: FN_CRIA_ADJ_RECPTD_INTERFACE, ID: 372342516, model: 746.
// Short name: SWECRIAP
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
/// A program: FN_CRIA_ADJ_RECPTD_INTERFACE.
/// </para>
/// <para>
/// RESP: FNCLMGMT
/// Cash receipts are recorded by the Receivables Unit when checks are received.
/// If the check received is made out to a different amount than what the
/// interface has indicated, or if the interface is out of balance for any
/// reason, an adjustment is required.
/// This screen allows them to record the balancing adjustment at the cash 
/// receipt level.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCriaAdjRecptdInterface: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRIA_ADJ_RECPTD_INTERFACE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCriaAdjRecptdInterface(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCriaAdjRecptdInterface.
  /// </summary>
  public FnCriaAdjRecptdInterface(IContext context, Import import, Export export)
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
    // ----------------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented.
    // ----------------------------------------------------------------------------
    // Date 	  Developer Name     Request #  Description
    // ----------------------------------------------------------------------------
    // 02/05/96  Holly Kennedy-MTW		Retrofits
    // 11/26/96  R. Marchman			Add new security and next tran
    // 02/26/99  J Katz - SRG			Redesign screen to create and
    // 					maintain only adjustments that
    // 					apply to the receipt amount.
    // 05/29/99  J Katz - SRG			Change the intensity of all
    // 					protected prompt fields from
    // 					Dark to Normal.
    // 					Default the Source Code
    // 					passed to CRBI to the source
    // 					code of either the increase
    // 					receipt or the decrease receipt,
    // 					whichever is available.
    // 					Pass a default source code
    // 					to CREL using the same default
    // 					criteria as used for CRBI.
    // 05/31/99  J Katz - SRG			Moved logic for Switch command
    // 					above main case of command,
    // 					reset the command to Display.
    // 10/20/99	J Katz - SRG		Added logic to support two
    // 					new Cash Receipt Rln Rsn codes.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ---------------------------------------------------------------
    // Next Tran logic
    // ---------------------------------------------------------------
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // ---------------------------------------------------------------
    // If the next tran info is not equal to spaces, the user
    // requested a next tran action.  Now validate.
    // ---------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // ---------------------------------
      // No valid values to set.
      // ---------------------------------
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ---------------------------------
      // No valid values to set.
      // ---------------------------------
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // --------------------------------------------------------------------
    // Determine the calling PrAD and set the screen accordingly.
    // --------------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "CRBI":
        export.CallingPrad.Command = global.Command;
        global.Command = "DISPLAY";

        break;
      case "CRAJ":
        export.CallingPrad.Command = global.Command;
        global.Command = "DISPLAY";

        break;
      default:
        export.CallingPrad.Command = import.CallingPrad.Command;

        break;
    }

    // ************************************************************
    // *               Move Imports to Exports.                   *
    // ************************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      // ---------------------------------------------------------------
      // If currency has been established on a Cash Receipt Balance
      // Adjustment, the only field enterable is the description.
      // Pressing the clear PFKey once will unprotect all the enterable
      // fields and allow the user to create a second adjustment for the
      // same cash receipts and reason.  Pressing clear a second time
      // will clear the screen completely.  If currency has not been
      // established, clear the screen completely.
      // ---------------------------------------------------------------
      if (!Equal(import.CashReceiptBalanceAdjustment.CreatedTimestamp,
        local.Null1.Timestamp))
      {
        // ---------------------------------------------------------------
        // Move all of the imports to exports except the Cash Receipt
        // Balance Adjustment.
        // ---------------------------------------------------------------
        MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn, export.CashReceiptRlnRsn);
          
        MoveCashReceiptSourceType(import.IncreaseCashReceiptSourceType,
          export.IncreaseCashReceiptSourceType);
        export.IncreaseCashReceiptEvent.Assign(import.IncreaseCashReceiptEvent);
        export.IncreaseCashReceipt.Assign(import.IncreaseCashReceipt);
        export.IncreaseCrAdjAmt.TotalCurrency =
          import.IncreaseCrAdjAmt.TotalCurrency;
        export.IncreaseNetReceiptAmt.TotalCurrency =
          import.IncreaseNetReceiptAmt.TotalCurrency;
        MoveCashReceiptSourceType(import.DecreaseCashReceiptSourceType,
          export.DecreaseCashReceiptSourceType);
        export.DecreaseCashReceiptEvent.Assign(import.DecreaseCashReceiptEvent);
        export.DecreaseCashReceipt.Assign(import.DecreaseCashReceipt);
        export.DecreaseCrAdjAmt.TotalCurrency =
          import.DecreaseCrAdjAmt.TotalCurrency;
        export.DecreaseNetReceiptAmt.TotalCurrency =
          import.DecreaseNetReceiptAmt.TotalCurrency;
        export.DecraseHidden.SystemGeneratedIdentifier =
          import.DecreaseHidden.SystemGeneratedIdentifier;
        export.HiddenCashReceiptRlnRsn.Code =
          import.HiddenCashReceiptRlnRsn.Code;
        export.HiddenIncrease.SequentialNumber =
          import.HiddenIncrease.SequentialNumber;
        export.HiddenDecrease.SequentialNumber =
          import.HiddenDecrease.SequentialNumber;
      }

      return;
    }

    // ---------------------------------------------------------------------
    // If a Cash Receipt Balance Adjustment is being imported, all fields
    // should be cleared by NOT moving the imports to exports; otherwise,
    // move imports to exports.
    // ---------------------------------------------------------------------
    if (Equal(import.PassAreaCashReceiptBalanceAdjustment.CreatedTimestamp,
      local.Null1.Timestamp))
    {
      // Move imports to exports.
      // ----------------------------------------------------------
      export.CashReceiptBalanceAdjustment.Assign(
        import.CashReceiptBalanceAdjustment);
      export.HiddenCashReceiptRlnRsn.Code = import.HiddenCashReceiptRlnRsn.Code;
      export.HiddenCashReceiptBalanceAdjustment.Description =
        import.HiddenCashReceiptBalanceAdjustment.Description;
      export.HiddenIncrease.SequentialNumber =
        import.HiddenIncrease.SequentialNumber;
      export.HiddenDecrease.SequentialNumber =
        import.HiddenDecrease.SequentialNumber;

      // -----------------------------------------------------------------
      // If this is the first execution after a transfer or link, populate
      // the export views with the imported information.  If any of the
      // enterable key fields have changed, re-initialized the associated
      // attributes and entities by not moving the imports to exports.
      // Otherwise, move the imports to exports.
      // -----------------------------------------------------------------
      if (import.PassAreaCashReceiptRlnRsn.SystemGeneratedIdentifier == 0 && IsEmpty
        (import.PassAreaCashReceiptRlnRsn.Code))
      {
        if (Equal(import.CashReceiptRlnRsn.Code,
          import.HiddenCashReceiptRlnRsn.Code) && !
          IsEmpty(import.HiddenCashReceiptRlnRsn.Code))
        {
          MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn,
            export.CashReceiptRlnRsn);
        }
        else
        {
          export.CashReceiptRlnRsn.Code = import.CashReceiptRlnRsn.Code;
        }
      }
      else
      {
        MoveCashReceiptRlnRsn(import.PassAreaCashReceiptRlnRsn,
          export.CashReceiptRlnRsn);
      }

      if (import.PassAreaIncrease.SequentialNumber == 0)
      {
        if (import.PassAreaSelection.SequentialNumber == 0 || IsEmpty
          (import.IncreaseCrPrompt.PromptField))
        {
          if (import.IncreaseCashReceipt.SequentialNumber == import
            .HiddenIncrease.SequentialNumber)
          {
            MoveCashReceiptSourceType(import.IncreaseCashReceiptSourceType,
              export.IncreaseCashReceiptSourceType);
            export.IncreaseCashReceiptEvent.Assign(
              import.IncreaseCashReceiptEvent);
            export.IncreaseCashReceipt.Assign(import.IncreaseCashReceipt);
            export.IncreaseCrAdjAmt.TotalCurrency =
              import.IncreaseCrAdjAmt.TotalCurrency;
            export.IncreaseNetReceiptAmt.TotalCurrency =
              import.IncreaseNetReceiptAmt.TotalCurrency;
          }
          else
          {
            export.IncreaseCashReceipt.SequentialNumber =
              import.IncreaseCashReceipt.SequentialNumber;
          }
        }
        else
        {
          export.IncreaseCashReceipt.SequentialNumber =
            import.PassAreaSelection.SequentialNumber;
        }
      }
      else
      {
        export.IncreaseCashReceipt.SequentialNumber =
          import.PassAreaIncrease.SequentialNumber;
      }

      if (import.PassAreaDecrease.SequentialNumber == 0)
      {
        if (import.PassAreaSelection.SequentialNumber == 0 || IsEmpty
          (import.DecreaseCrPrompt.PromptField))
        {
          if (import.DecreaseCashReceipt.SequentialNumber == import
            .HiddenDecrease.SequentialNumber)
          {
            MoveCashReceiptSourceType(import.DecreaseCashReceiptSourceType,
              export.DecreaseCashReceiptSourceType);
            export.DecreaseCashReceiptEvent.Assign(
              import.DecreaseCashReceiptEvent);
            export.DecreaseCashReceipt.Assign(import.DecreaseCashReceipt);
            export.DecreaseCrAdjAmt.TotalCurrency =
              import.DecreaseCrAdjAmt.TotalCurrency;
            export.DecreaseNetReceiptAmt.TotalCurrency =
              import.DecreaseNetReceiptAmt.TotalCurrency;
            export.DecraseHidden.SystemGeneratedIdentifier =
              import.DecreaseHidden.SystemGeneratedIdentifier;
          }
          else
          {
            export.DecreaseCashReceipt.SequentialNumber =
              import.DecreaseCashReceipt.SequentialNumber;
          }
        }
        else
        {
          export.DecreaseCashReceipt.SequentialNumber =
            import.PassAreaSelection.SequentialNumber;
        }
      }
      else
      {
        export.DecreaseCashReceipt.SequentialNumber =
          import.PassAreaDecrease.SequentialNumber;
      }
    }
    else
    {
      MoveCashReceiptRlnRsn(import.PassAreaCashReceiptRlnRsn,
        export.CashReceiptRlnRsn);
      export.CashReceiptBalanceAdjustment.CreatedTimestamp =
        import.PassAreaCashReceiptBalanceAdjustment.CreatedTimestamp;
      export.IncreaseCashReceipt.SequentialNumber =
        import.PassAreaIncrease.SequentialNumber;
      export.DecreaseCashReceipt.SequentialNumber =
        import.PassAreaDecrease.SequentialNumber;
    }

    // ---------------------------------------------------------------
    // If the Cash Receipt Balance Adjustment timestamp is populated,
    // the only option is to update the note.  Protect all enterable
    // fields except the Next Tran and the Note.
    // ---------------------------------------------------------------
    if (!Equal(export.CashReceiptBalanceAdjustment.CreatedTimestamp,
      local.Null1.Timestamp))
    {
      // -----------------------------------------------------------
      // Protect all of the enterable fields except the NextTran
      // and the Description.
      // -----------------------------------------------------------
      var field1 = GetField(export.CashReceiptRlnRsn, "code");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 =
        GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.IncreaseCashReceipt, "sequentialNumber");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.DecreaseCashReceipt, "sequentialNumber");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.AdjustmentReasonPrompt, "promptField");

      field5.Color = "cyan";
      field5.Intensity = Intensity.Normal;
      field5.Protected = true;

      var field6 = GetField(export.IncreaseCrPrompt, "promptField");

      field6.Color = "cyan";
      field6.Intensity = Intensity.Normal;
      field6.Protected = true;

      var field7 = GetField(export.DecreaseCrPrompt, "promptField");

      field7.Color = "cyan";
      field7.Intensity = Intensity.Normal;
      field7.Protected = true;

      var field8 = GetField(export.CashReceiptBalanceAdjustment, "description");

      field8.Color = "green";
      field8.Highlighting = Highlighting.Underscore;
      field8.Protected = false;
      field8.Focused = true;
    }

    // ---------------------------------------------------------------
    // Validate action level security.
    // ---------------------------------------------------------------
    // If the command is to link to another screen, the security
    // validation will be done in the called procedure.
    // ---------------------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "SWITCH"))
    {
      // ----------------------------------------------------------
      // PF15 Switch
      // Move the Increase Cash Receipt to the Decrease and the
      // Decrease Cash Receipt to the Increase.
      // ----------------------------------------------------------
      local.CashReceipt.Assign(export.DecreaseCashReceipt);
      export.DecreaseCashReceipt.Assign(export.IncreaseCashReceipt);
      export.IncreaseCashReceipt.Assign(local.CashReceipt);

      // -------------------------------------------------------
      // Clear the balance adjustment fields.
      // -------------------------------------------------------
      export.CashReceiptBalanceAdjustment.Assign(local.Clear);
      global.Command = "DISPLAY";
    }

    // ************************************************************
    // *              Main CASE OF Command Structure.             *
    // ************************************************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ---------------------------------------------------------------
        // PF2 Display
        // Validate that all required data was entered.
        // ---------------------------------------------------------------
        // ---------------------------------------------------------------
        // Only adjustments with an adjustment reason code of
        // ADDPMT, NETPMT, or REFUND can be displayed.
        // If the adjustment reason code is blank, skip this
        // edit but continue processing.   JLK  05/29/99
        // Added new adjustment reason codes PROCCSTFEE and
        // NETINTFERR, both which are used in conjunction with
        // BOGUS type cash receipts.   JLK  10/20/99
        // ---------------------------------------------------------------
        if (Equal(export.CashReceiptRlnRsn.Code, "ADDPMT") || Equal
          (export.CashReceiptRlnRsn.Code, "NETPMT") || Equal
          (export.CashReceiptRlnRsn.Code, "REFUND") || Equal
          (export.CashReceiptRlnRsn.Code, "PROCCSTFEE") || Equal
          (export.CashReceiptRlnRsn.Code, "NETINTFERR") || IsEmpty
          (export.CashReceiptRlnRsn.Code))
        {
          // --->  OK to continue
        }
        else
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          ExitState = "FN0000_INVALID_ADJ_REASON_CODE";

          return;
        }

        // ----------------------------------------------------------
        // A Cash Receipt cannot adjust itself.
        // ----------------------------------------------------------
        if (export.IncreaseCashReceipt.SequentialNumber > 0 && export
          .DecreaseCashReceipt.SequentialNumber > 0)
        {
          if (export.IncreaseCashReceipt.SequentialNumber == export
            .DecreaseCashReceipt.SequentialNumber)
          {
            ExitState = "FN0000_CR_CANNOT_ADJUST_ITSELF";

            var field9 =
              GetField(export.IncreaseCashReceipt, "sequentialNumber");

            field9.Error = true;

            var field10 =
              GetField(export.DecreaseCashReceipt, "sequentialNumber");

            field10.Error = true;

            return;
          }
        }

        // ---------------------------------------------------------------
        // Display whatever information can be displayed at this time.
        // ---------------------------------------------------------------
        UseFnDisplayReceiptAmtAdj();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ----------------------------------------------------------
          // Move export views to the hidden views.
          // ----------------------------------------------------------
          export.HiddenCashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;
          export.HiddenCashReceiptBalanceAdjustment.Description =
            export.CashReceiptBalanceAdjustment.Description;
          export.HiddenIncrease.SequentialNumber =
            export.IncreaseCashReceipt.SequentialNumber;
          export.HiddenDecrease.SequentialNumber =
            export.DecreaseCashReceipt.SequentialNumber;

          // ----------------------------------------------------------
          // Protect all fields on the screen except the NextTran
          // and the Description.
          // ----------------------------------------------------------
          var field9 = GetField(export.CashReceiptRlnRsn, "code");

          field9.Color = "cyan";
          field9.Protected = true;

          var field10 = GetField(export.AdjustmentReasonPrompt, "promptField");

          field10.Color = "cyan";
          field10.Intensity = Intensity.Normal;
          field10.Protected = true;

          var field11 =
            GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

          field11.Color = "cyan";
          field11.Protected = true;

          var field12 =
            GetField(export.CashReceiptBalanceAdjustment, "description");

          field12.Color = "green";
          field12.Highlighting = Highlighting.Underscore;
          field12.Protected = false;
          field12.Focused = true;

          var field13 =
            GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field13.Color = "cyan";
          field13.Protected = true;

          var field14 = GetField(export.IncreaseCrPrompt, "promptField");

          field14.Color = "cyan";
          field14.Intensity = Intensity.Normal;
          field14.Protected = true;

          var field15 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field15.Color = "cyan";
          field15.Protected = true;

          var field16 = GetField(export.DecreaseCrPrompt, "promptField");

          field16.Color = "cyan";
          field16.Intensity = Intensity.Normal;
          field16.Protected = true;

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (IsExitState("FN0000_PARTIAL_DISPLAY_OK"))
        {
          // ---------------------------------------------------------------
          // Validate that all required data was entered.
          // ---------------------------------------------------------------
          if (IsEmpty(export.CashReceiptRlnRsn.Code))
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field = GetField(export.CashReceiptRlnRsn, "code");

            field.Error = true;
          }
          else
          {
            export.HiddenCashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;
          }

          if (export.IncreaseCashReceipt.SequentialNumber == 0)
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field =
              GetField(export.IncreaseCashReceipt, "sequentialNumber");

            field.Error = true;
          }
          else
          {
            export.HiddenIncrease.SequentialNumber =
              export.IncreaseCashReceipt.SequentialNumber;
          }

          if (export.DecreaseCashReceipt.SequentialNumber == 0)
          {
            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            var field =
              GetField(export.DecreaseCashReceipt, "sequentialNumber");

            field.Error = true;
          }
          else
          {
            export.HiddenDecrease.SequentialNumber =
              export.DecreaseCashReceipt.SequentialNumber;
          }
        }
        else if (IsExitState("ACO_NI0000_NO_ITEMS_FOUND"))
        {
          // ----------------------------------------------------------
          // No adjustments exist for the specified Adjustment Reason
          // Code, Increase receipt and Decrease receipt.
          // Move export views to the hidden views.
          // ----------------------------------------------------------
          export.HiddenCashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;
          export.HiddenIncrease.SequentialNumber =
            export.IncreaseCashReceipt.SequentialNumber;
          export.HiddenDecrease.SequentialNumber =
            export.DecreaseCashReceipt.SequentialNumber;

          var field9 = GetField(export.CashReceiptRlnRsn, "code");

          field9.Protected = false;

          var field10 =
            GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

          field10.Protected = false;
          field10.Focused = true;

          var field11 =
            GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field11.Protected = false;

          var field12 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field12.Protected = false;

          ExitState = "FN0000_ADJ_RSN_AND_RCPTS_DISP_OK";
        }
        else if (IsExitState("FN0000_MULTIPLE_CR_BALANCE_ADJ"))
        {
          // ----------------------------------------------------------
          // Multiple adjustments exist for the specified Adjustment
          // Reason Code, Increase receipt and Decrease receipt.
          // Move export views to the hidden views.
          // ----------------------------------------------------------
          export.HiddenCashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;
          export.HiddenIncrease.SequentialNumber =
            export.IncreaseCashReceipt.SequentialNumber;
          export.HiddenDecrease.SequentialNumber =
            export.DecreaseCashReceipt.SequentialNumber;
        }
        else if (IsExitState("FN0000_INCR_CASH_RECEIPT_NF"))
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DECR_CASH_RECEIPT_NF"))
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0000_CASH_RECEIPT_NF"))
        {
          var field9 = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field9.Error = true;

          var field10 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field10.Error = true;
        }
        else if (IsExitState("FN0093_CASH_RCPT_RLN_RSN_NF"))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;
        }
        else
        {
        }

        break;
      case "LIST":
        // ----------------------------------------------------------
        // PF4 List  displays a list of adjustment reason codes.
        // ----------------------------------------------------------
        if (!IsEmpty(import.AdjustmentReasonPrompt.PromptField))
        {
          export.AdjustmentReasonPrompt.PromptField =
            import.AdjustmentReasonPrompt.PromptField;
        }

        if (!IsEmpty(import.IncreaseCrPrompt.PromptField))
        {
          export.IncreaseCrPrompt.PromptField =
            import.IncreaseCrPrompt.PromptField;
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.IncreaseCrPrompt, "promptField");

          field.Error = true;
        }

        if (!IsEmpty(import.DecreaseCrPrompt.PromptField))
        {
          export.DecreaseCrPrompt.PromptField =
            import.DecreaseCrPrompt.PromptField;
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.DecreaseCrPrompt, "promptField");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        switch(AsChar(import.AdjustmentReasonPrompt.PromptField))
        {
          case 'S':
            // -----------------------------------------------------
            // Flow to List Cash Receipt Balance Reason Codes
            // -----------------------------------------------------
            ExitState = "ECO_LNK_TO_LST_CR_BAL_RSN_CODE";

            break;
          case ' ':
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field9 = GetField(export.AdjustmentReasonPrompt, "promptField");

            field9.Error = true;

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field10 =
              GetField(export.AdjustmentReasonPrompt, "promptField");

            field10.Error = true;

            break;
        }

        break;
      case "ADD":
        // ----------------------------------------------------------
        // PF5 Add
        // Make sure all required fields are entered.
        // ----------------------------------------------------------
        if (export.IncreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.DecreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CashReceiptRlnRsn.Code))
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // Make sure the Adjustment Reason Code, Increase Cash
        // Receipt, and Decrease Cash Receipt have been displayed.
        // ---------------------------------------------------------------
        if (!Equal(export.CashReceiptRlnRsn.Code,
          export.HiddenCashReceiptRlnRsn.Code))
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;
        }

        if (export.IncreaseCashReceipt.SequentialNumber != export
          .HiddenIncrease.SequentialNumber)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }

        if (export.DecreaseCashReceipt.SequentialNumber != export
          .HiddenDecrease.SequentialNumber)
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // Once the Adjustment Reason Code, Increase Receipt, and
        // Decrease Receipt have been displayed, validate that an
        // Adjustment Amount greater than zero has been entered.
        // Note field is mandatory as of 03/11/99.  JLK
        // ---------------------------------------------------------------
        if (export.CashReceiptBalanceAdjustment.AdjustmentAmount == 0)
        {
          var field =
            GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

          field.Error = true;

          ExitState = "FN0000_ADJUSTMENT_AMOUNT_ERROR";
        }

        if (IsEmpty(export.CashReceiptBalanceAdjustment.Description))
        {
          var field =
            GetField(export.CashReceiptBalanceAdjustment, "description");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // If the Cash Receipt Balance Adjustment created timestamp is not null,
        // the adjustment has already been created.
        // ---------------------------------------------------------------
        if (Lt(local.Null1.Timestamp,
          export.CashReceiptBalanceAdjustment.CreatedTimestamp))
        {
          ExitState = "FN0030_CASH_RCPT_BAL_ADJ_AE";

          return;
        }

        // ---------------------------------------------------------------
        // Only Adjustment Reason Codes that are defined for
        // adjustments to the cash receipt Receipt Amount are valid.
        // ---------------------------------------------------------------
        switch(TrimEnd(export.CashReceiptRlnRsn.Code))
        {
          case "ADDPMT":
            // ----------------------------------------------------------
            // The Increase cash receipt must have a valid source
            // creation date.
            // The Decrease cash receipt must have a null source
            // creation date.
            // ----------------------------------------------------------
            if (Equal(export.IncreaseCashReceiptEvent.SourceCreationDate,
              local.Null1.Date))
            {
              ExitState = "FN0000_ADDPMT_CR_SELECTED_INVALI";

              var field9 =
                GetField(export.IncreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (Lt(local.Null1.Date,
              export.DecreaseCashReceiptEvent.SourceCreationDate))
            {
              var field9 =
                GetField(export.DecreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            // ----------------------------------------------------------
            // The Decrease cash receipt must be in DEP status.
            // JLK  07/23/99
            // ----------------------------------------------------------
            UseFnHardcodedCashReceipting();

            if (export.DecraseHidden.SystemGeneratedIdentifier != local
              .HardcodedCrsDeposited.SystemGeneratedIdentifier)
            {
              ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

              var field9 =
                GetField(export.DecreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "NETPMT":
            // ----------------------------------------------------------
            // Both the Increase and Decrease cash receipt must have
            // a valid source creation date.
            // ----------------------------------------------------------
            if (Equal(export.IncreaseCashReceiptEvent.SourceCreationDate,
              local.Null1.Date))
            {
              ExitState = "FN0000_NETPMT_CR_SELECTED_INVALI";

              var field9 =
                GetField(export.IncreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (Equal(export.DecreaseCashReceiptEvent.SourceCreationDate,
              local.Null1.Date))
            {
              ExitState = "FN0000_NETPMT_CR_SELECTED_INVALI";

              var field9 =
                GetField(export.DecreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "REFUND":
            // ----------------------------------------------------------
            // The Increase cash receipt must have a Cash Receipt
            // Type and Check Type of BOGUS.
            // The Decrease cash receipt must have a valid source
            // creation date.
            // ----------------------------------------------------------
            if (!Equal(export.IncreaseCashReceipt.CheckType, "BOGUS"))
            {
              ExitState = "FN0000_REFUND_CR_SELECTED_INVALI";

              var field9 =
                GetField(export.IncreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (Equal(export.DecreaseCashReceiptEvent.SourceCreationDate,
              local.Null1.Date))
            {
              ExitState = "FN0000_REFUND_CR_SELECTED_INVALI";

              var field9 =
                GetField(export.DecreaseCashReceipt, "sequentialNumber");

              field9.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "PROCCSTFEE":
            // ----------------------------------------------------------
            // The Decrease cash receipt must have a Cash Receipt
            // Type and Check Type of BOGUS.
            // The Increase cash receipt must have a valid source
            // creation date.
            // JLK  10/20/99
            // ----------------------------------------------------------
            if (!Equal(export.DecreaseCashReceipt.CheckType, "BOGUS"))
            {
              ExitState = "FN000_PROCCSTFEE_CR_SELECTED_INV";

              var field9 = GetField(export.DecreaseCashReceipt, "checkType");

              field9.Error = true;
            }

            if (Equal(export.IncreaseCashReceiptEvent.SourceCreationDate,
              local.Null1.Date))
            {
              ExitState = "FN000_PROCCSTFEE_CR_SELECTED_INV";

              var field9 =
                GetField(export.IncreaseCashReceiptEvent, "sourceCreationDate");
                

              field9.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          case "NETINTFERR":
            // ----------------------------------------------------------
            // Either the Increase OR the Decrease cash receipt must
            // have a Cash Receipt Type and Check Type of BOGUS.
            // The remaining cash receipt must have a valid source
            // creation date.
            // JLK  10/20/99
            // ----------------------------------------------------------
            if (Equal(export.IncreaseCashReceipt.CheckType, "BOGUS") && Equal
              (export.DecreaseCashReceipt.CheckType, "BOGUS"))
            {
              ExitState = "FN000_NETINTFERR_CR_SELECTED_INV";

              var field9 = GetField(export.IncreaseCashReceipt, "checkType");

              field9.Error = true;

              var field10 = GetField(export.DecreaseCashReceipt, "checkType");

              field10.Error = true;
            }
            else if (Equal(export.IncreaseCashReceipt.CheckType, "BOGUS") || Equal
              (export.DecreaseCashReceipt.CheckType, "BOGUS"))
            {
              // ---> continue
            }
            else
            {
              ExitState = "FN000_NETINTFERR_CR_SELECTED_INV";

              var field9 = GetField(export.IncreaseCashReceipt, "checkType");

              field9.Error = true;

              var field10 = GetField(export.DecreaseCashReceipt, "checkType");

              field10.Error = true;
            }

            if (Lt(local.Null1.Date,
              export.IncreaseCashReceiptEvent.SourceCreationDate) && Lt
              (local.Null1.Date,
              export.DecreaseCashReceiptEvent.SourceCreationDate))
            {
              ExitState = "FN000_NETINTFERR_CR_SELECTED_INV";

              var field9 =
                GetField(export.IncreaseCashReceiptEvent, "sourceCreationDate");
                

              field9.Error = true;

              var field10 =
                GetField(export.DecreaseCashReceiptEvent, "sourceCreationDate");
                

              field10.Error = true;
            }
            else if (Lt(local.Null1.Date,
              export.IncreaseCashReceiptEvent.SourceCreationDate) || Lt
              (local.Null1.Date,
              export.DecreaseCashReceiptEvent.SourceCreationDate))
            {
              // ---> continue
            }
            else
            {
              ExitState = "FN000_NETINTFERR_CR_SELECTED_INV";

              var field9 =
                GetField(export.IncreaseCashReceiptEvent, "sourceCreationDate");
                

              field9.Error = true;

              var field10 =
                GetField(export.DecreaseCashReceiptEvent, "sourceCreationDate");
                

              field10.Error = true;
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            break;
          default:
            ExitState = "FN0000_INV_RSN_FOR_RCPT_AMT_ADJ";

            var field = GetField(export.CashReceiptRlnRsn, "code");

            field.Error = true;

            return;
        }

        // ----------------------------------------------------------
        // The Cash Receipts must be from the same Source Type.
        // ----------------------------------------------------------
        if (export.IncreaseCashReceiptSourceType.SystemGeneratedIdentifier != export
          .DecreaseCashReceiptSourceType.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_CR_SOURCE_TYPES_DIFFERENT";

          var field9 = GetField(export.IncreaseCashReceiptSourceType, "code");

          field9.Error = true;

          var field10 = GetField(export.DecreaseCashReceiptSourceType, "code");

          field10.Error = true;

          return;
        }

        // ----------------------------------------------------------
        // A Cash Receipt cannot adjust itself.
        // ----------------------------------------------------------
        if (export.IncreaseCashReceipt.SequentialNumber == export
          .DecreaseCashReceipt.SequentialNumber)
        {
          ExitState = "FN0000_CR_CANNOT_ADJUST_ITSELF";

          var field9 = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field9.Error = true;

          var field10 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field10.Error = true;

          return;
        }

        // ---------------------------------------------------------------
        // Adjustment Amount must be less than or equal to the
        // Decrease net receipt amount IF the Decrease check
        // type is not BOGUS.   JLK  10/20/99
        // ---------------------------------------------------------------
        if (!Equal(export.DecreaseCashReceipt.CheckType, "BOGUS"))
        {
          if (export.CashReceiptBalanceAdjustment.AdjustmentAmount > export
            .DecreaseNetReceiptAmt.TotalCurrency && export
            .DecreaseCashReceipt.CashDue.GetValueOrDefault() >= 0)
          {
            ExitState = "INVALID_ADJUSTMENT_AMOUNT";

            var field9 =
              GetField(export.DecreaseNetReceiptAmt, "totalCurrency");

            field9.Error = true;

            var field10 =
              GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");
              

            field10.Error = true;

            return;
          }
        }

        // ----------------------------------------------------------
        // Create the adjustment.
        // ----------------------------------------------------------
        UseFnCreateReceiptAmtAdj();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          MoveCashReceiptRlnRsn(local.CashReceiptRlnRsn,
            export.CashReceiptRlnRsn);
          export.CashReceiptBalanceAdjustment.Assign(
            local.CashReceiptBalanceAdjustment);
          export.IncreaseCashReceipt.Assign(local.Increase);
          export.IncreaseCrAdjAmt.TotalCurrency =
            local.IncreaseCrAdjAmt.TotalCurrency;
          export.IncreaseNetReceiptAmt.TotalCurrency =
            local.IncreaseNetReceiptAmt.TotalCurrency;
          export.DecreaseCashReceipt.Assign(local.CashReceipt);
          export.DecreaseCrAdjAmt.TotalCurrency = local.CrAdjAmt.TotalCurrency;
          export.DecreaseNetReceiptAmt.TotalCurrency =
            local.NetReceiptAmt.TotalCurrency;
          export.HiddenCashReceiptRlnRsn.Code = export.CashReceiptRlnRsn.Code;
          export.HiddenCashReceiptBalanceAdjustment.Description =
            export.CashReceiptBalanceAdjustment.Description;
          export.HiddenIncrease.SequentialNumber =
            export.IncreaseCashReceipt.SequentialNumber;
          export.HiddenDecrease.SequentialNumber =
            export.DecreaseCashReceipt.SequentialNumber;
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }
        else
        {
          if (IsExitState("FN0000_INCR_CASH_RECEIPT_NF"))
          {
            var field =
              GetField(export.IncreaseCashReceipt, "sequentialNumber");

            field.Error = true;
          }
          else if (IsExitState("FN0000_DECR_CASH_RECEIPT_NF"))
          {
            var field =
              GetField(export.DecreaseCashReceipt, "sequentialNumber");

            field.Error = true;
          }
          else if (IsExitState("FN0093_CASH_RCPT_RLN_RSN_NF"))
          {
            var field = GetField(export.CashReceiptRlnRsn, "code");

            field.Error = true;
          }
          else
          {
          }

          return;
        }

        // ----------------------------------------------------------
        // Protect all fields on the screen except the NextTran and
        // the Description.
        // ----------------------------------------------------------
        var field1 = GetField(export.CashReceiptRlnRsn, "code");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 = GetField(export.IncreaseCashReceipt, "sequentialNumber");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 = GetField(export.DecreaseCashReceipt, "sequentialNumber");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.AdjustmentReasonPrompt, "promptField");

        field5.Color = "cyan";
        field5.Intensity = Intensity.Normal;
        field5.Protected = true;

        var field6 = GetField(export.IncreaseCrPrompt, "promptField");

        field6.Color = "cyan";
        field6.Intensity = Intensity.Normal;
        field6.Protected = true;

        var field7 = GetField(export.DecreaseCrPrompt, "promptField");

        field7.Color = "cyan";
        field7.Intensity = Intensity.Normal;
        field7.Protected = true;

        var field8 =
          GetField(export.CashReceiptBalanceAdjustment, "description");

        field8.Color = "green";
        field8.Highlighting = Highlighting.Underscore;
        field8.Protected = false;
        field8.Focused = true;

        break;
      case "UPDATE":
        // ---------------------------------------------------------------
        // PF6 Update
        // Modifies the Cash Receipt Balance Adj DESCRIPTION field.
        // Adjustment Reason Code, Increase cash receipt, and
        // Decrease cash receipt must be entered.
        // ---------------------------------------------------------------
        if (export.DecreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.IncreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.CashReceiptBalanceAdjustment.AdjustmentAmount == 0)
        {
          var field =
            GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CashReceiptRlnRsn.Code))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // Adjustment must be displayed prior to the Update action.
        // ---------------------------------------------------------------
        if (export.IncreaseCashReceipt.SequentialNumber != export
          .HiddenIncrease.SequentialNumber)
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }

        if (export.DecreaseCashReceipt.SequentialNumber != export
          .HiddenDecrease.SequentialNumber)
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }

        if (!Equal(export.CashReceiptRlnRsn.Code,
          export.HiddenCashReceiptRlnRsn.Code))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // If the cash receipt balance adjustment timestamp is null, the
        // adjustment has not been created.
        // ---------------------------------------------------------------
        if (Equal(export.CashReceiptBalanceAdjustment.CreatedTimestamp,
          local.Null1.Timestamp))
        {
          ExitState = "FN0000_CR_ADJ_NOT_CREATED_OR_DIS";

          return;
        }

        // ---------------------------------------------------------------
        // The adjustment Note field on the screen is mandatory.
        // JLK  03/11/99
        // ---------------------------------------------------------------
        if (IsEmpty(export.CashReceiptBalanceAdjustment.Description))
        {
          export.HiddenCashReceiptBalanceAdjustment.Description =
            Spaces(CashReceiptBalanceAdjustment.Description_MaxLength);

          var field =
            GetField(export.CashReceiptBalanceAdjustment, "description");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // ----------------------------------------------------------
        // Verify that an update is necessary.
        // ----------------------------------------------------------
        if (Equal(export.CashReceiptBalanceAdjustment.Description,
          export.HiddenCashReceiptBalanceAdjustment.Description))
        {
          ExitState = "FN0000_CR_BAL_ADJ_UPD_NOT_NEED";

          return;
        }

        // ----------------------------------------------------------
        // Update cash receipt balance adjustment description field.
        // ----------------------------------------------------------
        UseFnUpdateCrAdjDescription();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "DELETE":
        // ---------------------------------------------------------------
        // PF10 Delete
        // If the cash receipt balance adjustment timestamp is null, the
        // adjustment has not been created.
        // ---------------------------------------------------------------
        if (export.DecreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.IncreaseCashReceipt.SequentialNumber == 0)
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (IsEmpty(export.CashReceiptRlnRsn.Code))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // Adjustment must be displayed prior to the Delete action.
        // ---------------------------------------------------------------
        if (!Equal(export.CashReceiptRlnRsn.Code,
          export.HiddenCashReceiptRlnRsn.Code))
        {
          var field = GetField(export.CashReceiptRlnRsn, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
        }

        if (export.IncreaseCashReceipt.SequentialNumber != export
          .HiddenIncrease.SequentialNumber)
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
        }

        if (export.DecreaseCashReceipt.SequentialNumber != export
          .HiddenDecrease.SequentialNumber)
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_DELETE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------------------------------------
        // If the cash receipt balance adjustment timestamp is null, the
        // adjustment has not been created.
        // ---------------------------------------------------------------
        if (Equal(export.CashReceiptBalanceAdjustment.CreatedTimestamp,
          local.Null1.Timestamp))
        {
          var field9 = GetField(export.CashReceiptRlnRsn, "code");

          field9.Color = "red";
          field9.Highlighting = Highlighting.ReverseVideo;
          field9.Protected = false;
          field9.Focused = true;

          var field10 =
            GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field10.Error = true;

          var field11 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field11.Error = true;

          ExitState = "FN0000_CR_ADJ_NOT_CREATED_OR_DIS";

          return;
        }

        // ---------------------------------------------------------------
        // Only adjustments with an adjustment reason code of
        // ADDPMT, NETPMT, or REFUND can be deleted.
        // Added new adjustment reason codes PROCCSTFEE and
        // NETINTFERR, both which are used in conjunction with
        // BOGUS type cash receipts.   JLK  10/20/99
        // ---------------------------------------------------------------
        if (Equal(export.CashReceiptRlnRsn.Code, "ADDPMT") || Equal
          (export.CashReceiptRlnRsn.Code, "NETPMT") || Equal
          (export.CashReceiptRlnRsn.Code, "REFUND") || Equal
          (export.CashReceiptRlnRsn.Code, "PROCCSTFEE") || Equal
          (export.CashReceiptRlnRsn.Code, "NETINTFERR"))
        {
          // --->  OK to continue
        }
        else
        {
          ExitState = "FN0000_INVALID_ADJ_REASON_CODE";

          return;
        }

        // ---------------------------------------------------------------
        // Delete adjustment and update balance due on interface receipt.
        // ---------------------------------------------------------------
        UseFnDeleteCrBalanceAdj();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ---------------------------------------------------------------
          // Populate export views with new data after deleting the
          // adjustment.
          // ---------------------------------------------------------------
          export.IncreaseCashReceipt.Assign(local.Increase);
          export.IncreaseCrAdjAmt.TotalCurrency =
            local.IncreaseCrAdjAmt.TotalCurrency;
          export.IncreaseNetReceiptAmt.TotalCurrency =
            local.IncreaseNetReceiptAmt.TotalCurrency;
          export.DecreaseCashReceipt.Assign(local.CashReceipt);
          export.DecreaseCrAdjAmt.TotalCurrency = local.CrAdjAmt.TotalCurrency;
          export.DecreaseNetReceiptAmt.TotalCurrency =
            local.NetReceiptAmt.TotalCurrency;
          export.CashReceiptBalanceAdjustment.Assign(local.Clear);

          // ---------------------------------------------------------------
          // Unprotect the fields that were previously protected for an
          // existing adjustment record.
          // ---------------------------------------------------------------
          var field9 = GetField(export.CashReceiptRlnRsn, "code");

          field9.Protected = false;
          field9.Focused = true;

          var field10 = GetField(export.AdjustmentReasonPrompt, "promptField");

          field10.Protected = false;

          var field11 =
            GetField(export.CashReceiptBalanceAdjustment, "adjustmentAmount");

          field11.Protected = false;

          var field12 =
            GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field12.Protected = false;

          var field13 = GetField(export.IncreaseCrPrompt, "promptField");

          field13.Protected = false;

          var field14 =
            GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field14.Protected = false;

          var field15 = GetField(export.DecreaseCrPrompt, "promptField");

          field15.Protected = false;

          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }
        else if (IsExitState("FN0000_INCR_CASH_RECEIPT_NF"))
        {
          var field = GetField(export.IncreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0000_DECR_CASH_RECEIPT_NF"))
        {
          var field = GetField(export.DecreaseCashReceipt, "sequentialNumber");

          field.Error = true;
        }
        else if (IsExitState("FN0031_CASH_RCPT_BAL_ADJ_NF"))
        {
        }
        else if (IsExitState("FN0032_CASH_RCPT_BAL_ADJ_NU_RB"))
        {
        }
        else if (IsExitState("FN0033_CASH_RCPT_BAL_ADJ_PV_RB"))
        {
        }
        else
        {
        }

        break;
      case "SWITCH":
        // ----------------------------------------------------------
        // PF15 Switch
        // Move the Increase Cash Receipt to the Decrease and the
        // Decrease Cash Receipt to the Increase.
        // Logic moved to IF statement preceding the main case of
        // command.  Command reset to Display to refresh the screen
        // with any adjustments that may exist for the new
        // combination of receipts.   JLK  05/31/99
        // ----------------------------------------------------------
        break;
      case "LNK_CRBI":
        // ------------------------------------------------------------
        // PF16 CRBI  List Cash Receipt Interface Balance screen.
        // JLK  02/17/99
        // If the cash receipt for the selected prompt field has no
        // source code displayed, pass the source code for the 'other'
        // cash receipt.   JLK  05/29/99
        // ------------------------------------------------------------
        if (!IsEmpty(import.AdjustmentReasonPrompt.PromptField))
        {
          export.AdjustmentReasonPrompt.PromptField =
            import.AdjustmentReasonPrompt.PromptField;
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.AdjustmentReasonPrompt, "promptField");

          field.Error = true;

          return;
        }

        export.IncreaseCrPrompt.PromptField =
          import.IncreaseCrPrompt.PromptField;
        export.DecreaseCrPrompt.PromptField =
          import.DecreaseCrPrompt.PromptField;
        local.Common.Count = 0;

        switch(AsChar(import.IncreaseCrPrompt.PromptField))
        {
          case 'S':
            if (!IsEmpty(export.IncreaseCashReceiptSourceType.Code))
            {
              MoveCashReceiptSourceType(export.IncreaseCashReceiptSourceType,
                export.PassArea);
            }
            else
            {
              MoveCashReceiptSourceType(export.DecreaseCashReceiptSourceType,
                export.PassArea);
            }

            ++local.Common.Count;

            var field = GetField(export.IncreaseCrPrompt, "promptField");

            field.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(import.DecreaseCrPrompt.PromptField))
        {
          case 'S':
            if (!IsEmpty(export.DecreaseCashReceiptSourceType.Code))
            {
              MoveCashReceiptSourceType(export.DecreaseCashReceiptSourceType,
                export.PassArea);
            }
            else
            {
              MoveCashReceiptSourceType(export.IncreaseCashReceiptSourceType,
                export.PassArea);
            }

            ++local.Common.Count;

            var field = GetField(export.DecreaseCrPrompt, "promptField");

            field.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_BALANCE_INTERFACE";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "LNK_CREL":
        // ------------------------------------------------------------
        // PF17 CREL  List Cash Receipts screen.
        // JLK  02/17/99
        // ------------------------------------------------------------
        if (!IsEmpty(import.AdjustmentReasonPrompt.PromptField))
        {
          export.AdjustmentReasonPrompt.PromptField =
            import.AdjustmentReasonPrompt.PromptField;
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.AdjustmentReasonPrompt, "promptField");

          field.Error = true;

          return;
        }

        export.IncreaseCrPrompt.PromptField =
          import.IncreaseCrPrompt.PromptField;
        export.DecreaseCrPrompt.PromptField =
          import.DecreaseCrPrompt.PromptField;
        local.Common.Count = 0;

        switch(AsChar(import.IncreaseCrPrompt.PromptField))
        {
          case 'S':
            if (!IsEmpty(export.IncreaseCashReceiptSourceType.Code))
            {
              MoveCashReceiptSourceType(export.IncreaseCashReceiptSourceType,
                export.PassArea);
            }
            else
            {
              MoveCashReceiptSourceType(export.DecreaseCashReceiptSourceType,
                export.PassArea);
            }

            ++local.Common.Count;

            var field9 = GetField(export.IncreaseCrPrompt, "promptField");

            field9.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field10 = GetField(export.IncreaseCrPrompt, "promptField");

            field10.Error = true;

            return;
        }

        switch(AsChar(import.DecreaseCrPrompt.PromptField))
        {
          case 'S':
            if (!IsEmpty(export.DecreaseCashReceiptSourceType.Code))
            {
              MoveCashReceiptSourceType(export.DecreaseCashReceiptSourceType,
                export.PassArea);
            }
            else
            {
              MoveCashReceiptSourceType(export.IncreaseCashReceiptSourceType,
                export.PassArea);
            }

            ++local.Common.Count;

            var field9 = GetField(export.DecreaseCrPrompt, "promptField");

            field9.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            var field10 = GetField(export.DecreaseCrPrompt, "promptField");

            field10.Error = true;

            return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          case 1:
            ExitState = "ECO_LNK_TO_LST_CASH_RECEIPT";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "LNK_CRAJ":
        // ------------------------------------------------------------
        // PF18 CRAJ  List Interface Receipt Amount Adjustments screen.
        // JLK  01/19/99
        // ------------------------------------------------------------
        export.IncreaseCrPrompt.PromptField =
          import.IncreaseCrPrompt.PromptField;
        export.DecreaseCrPrompt.PromptField =
          import.DecreaseCrPrompt.PromptField;
        local.Common.Count = 0;

        switch(AsChar(import.IncreaseCrPrompt.PromptField))
        {
          case 'S':
            ++local.Common.Count;
            export.Pass.SequentialNumber =
              export.IncreaseCashReceipt.SequentialNumber;

            var field = GetField(export.IncreaseCrPrompt, "promptField");

            field.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(import.DecreaseCrPrompt.PromptField))
        {
          case 'S':
            ++local.Common.Count;
            export.Pass.SequentialNumber =
              export.DecreaseCashReceipt.SequentialNumber;

            var field = GetField(export.DecreaseCrPrompt, "promptField");

            field.Error = true;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(local.Common.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
          case 1:
            ExitState = "ECO_LNK_LST_CR_BAL_ADJ";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "RETURN":
        // ------------------------------------------------------------
        // PF9 Return
        // ------------------------------------------------------------
        if (Equal(export.CallingPrad.Command, "CRBI"))
        {
          if (!IsEmpty(export.IncreaseCashReceiptSourceType.Code))
          {
            MoveCashReceiptSourceType(export.IncreaseCashReceiptSourceType,
              export.PassArea);
          }
          else
          {
            MoveCashReceiptSourceType(export.DecreaseCashReceiptSourceType,
              export.PassArea);
          }
        }

        if (Equal(export.CallingPrad.Command, "CRAJ"))
        {
          export.IncreaseCrPrompt.PromptField =
            import.IncreaseCrPrompt.PromptField;
          export.DecreaseCrPrompt.PromptField =
            import.DecreaseCrPrompt.PromptField;
          local.Common.Count = 0;

          switch(AsChar(import.IncreaseCrPrompt.PromptField))
          {
            case 'S':
              ++local.Common.Count;
              export.Pass.SequentialNumber =
                export.IncreaseCashReceipt.SequentialNumber;

              var field = GetField(export.IncreaseCrPrompt, "promptField");

              field.Error = true;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
          }

          switch(AsChar(import.DecreaseCrPrompt.PromptField))
          {
            case 'S':
              ++local.Common.Count;
              export.Pass.SequentialNumber =
                export.DecreaseCashReceipt.SequentialNumber;

              var field = GetField(export.DecreaseCrPrompt, "promptField");

              field.Error = true;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              return;
          }

          switch(local.Common.Count)
          {
            case 0:
              ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

              break;
            case 1:
              // --->  OK to continue
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // ------------------------------------------------------------
        // PF12 Signoff
        // ------------------------------------------------------------
        UseScCabSignoff();

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.CashDue = source.CashDue;
    target.CashBalanceAmt = source.CashBalanceAmt;
  }

  private static void MoveCashReceiptBalanceAdjustment1(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.AdjustmentAmount = source.AdjustmentAmount;
    target.Description = source.Description;
  }

  private static void MoveCashReceiptBalanceAdjustment2(
    CashReceiptBalanceAdjustment source, CashReceiptBalanceAdjustment target)
  {
    target.Description = source.Description;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCashReceiptRlnRsn(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private void UseFnCreateReceiptAmtAdj()
  {
    var useImport = new FnCreateReceiptAmtAdj.Import();
    var useExport = new FnCreateReceiptAmtAdj.Export();

    useImport.Increase.SequentialNumber =
      export.IncreaseCashReceipt.SequentialNumber;
    useImport.Decrease.SequentialNumber =
      export.DecreaseCashReceipt.SequentialNumber;
    MoveCashReceiptRlnRsn(export.CashReceiptRlnRsn, useImport.CashReceiptRlnRsn);
      
    MoveCashReceiptBalanceAdjustment1(export.CashReceiptBalanceAdjustment,
      useImport.CashReceiptBalanceAdjustment);

    Call(FnCreateReceiptAmtAdj.Execute, useImport, useExport);

    MoveCashReceiptRlnRsn(useExport.CashReceiptRlnRsn, local.CashReceiptRlnRsn);
    local.CashReceiptBalanceAdjustment.Assign(
      useExport.CashReceiptBalanceAdjustment);
    local.Increase.Assign(useExport.Increase);
    local.IncreaseCrAdjAmt.TotalCurrency =
      useExport.IncreaseCrAdjAmt.TotalCurrency;
    local.IncreaseNetReceiptAmt.TotalCurrency =
      useExport.IncreaseNetReceiptAmt.TotalCurrency;
    local.CashReceipt.Assign(useExport.Decrease);
    local.CrAdjAmt.TotalCurrency = useExport.DecreaseCrAdjAmt.TotalCurrency;
    local.NetReceiptAmt.TotalCurrency =
      useExport.DecreaseNetAdjAmt.TotalCurrency;
  }

  private void UseFnDeleteCrBalanceAdj()
  {
    var useImport = new FnDeleteCrBalanceAdj.Import();
    var useExport = new FnDeleteCrBalanceAdj.Export();

    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      export.CashReceiptRlnRsn.SystemGeneratedIdentifier;
    useImport.Increase.SequentialNumber =
      export.IncreaseCashReceipt.SequentialNumber;
    useImport.Decrease.SequentialNumber =
      export.DecreaseCashReceipt.SequentialNumber;
    useImport.CashReceiptBalanceAdjustment.CreatedTimestamp =
      export.CashReceiptBalanceAdjustment.CreatedTimestamp;

    Call(FnDeleteCrBalanceAdj.Execute, useImport, useExport);

    MoveCashReceipt(useExport.Increase, local.Increase);
    local.IncreaseCrAdjAmt.TotalCurrency =
      useExport.IncreaseCrAdjAmt.TotalCurrency;
    local.IncreaseNetReceiptAmt.TotalCurrency =
      useExport.IncreaseNetReceiptAmt.TotalCurrency;
    MoveCashReceipt(useExport.Decrease, local.CashReceipt);
    local.CrAdjAmt.TotalCurrency = useExport.DecreaseCrAdjAmt.TotalCurrency;
    local.NetReceiptAmt.TotalCurrency =
      useExport.DecreaseNetReceiptAmt.TotalCurrency;
  }

  private void UseFnDisplayReceiptAmtAdj()
  {
    var useImport = new FnDisplayReceiptAmtAdj.Import();
    var useExport = new FnDisplayReceiptAmtAdj.Export();

    MoveCashReceiptRlnRsn(export.CashReceiptRlnRsn, useImport.CashReceiptRlnRsn);
      
    useImport.Increase.SequentialNumber =
      export.IncreaseCashReceipt.SequentialNumber;
    useImport.Decrease.SequentialNumber =
      export.DecreaseCashReceipt.SequentialNumber;
    useImport.CashReceiptBalanceAdjustment.CreatedTimestamp =
      export.CashReceiptBalanceAdjustment.CreatedTimestamp;

    Call(FnDisplayReceiptAmtAdj.Execute, useImport, useExport);

    MoveCashReceiptRlnRsn(useExport.CashReceiptRlnRsn, export.CashReceiptRlnRsn);
      
    export.CashReceiptBalanceAdjustment.Assign(
      useExport.CashReceiptBalanceAdjustment);
    MoveCashReceiptSourceType(useExport.IncreaseCashReceiptSourceType,
      export.IncreaseCashReceiptSourceType);
    export.IncreaseCashReceiptEvent.Assign(useExport.IncreaseCashReceiptEvent);
    export.IncreaseCashReceipt.Assign(useExport.IncreaseCashReceipt);
    export.IncreaseCrAdjAmt.TotalCurrency =
      useExport.IncreaseCrAdjAmt.TotalCurrency;
    export.IncreaseNetReceiptAmt.TotalCurrency =
      useExport.IncreaseNetReceiptAmt.TotalCurrency;
    MoveCashReceiptSourceType(useExport.DecreaseCashReceiptSourceType,
      export.DecreaseCashReceiptSourceType);
    export.DecreaseCashReceiptEvent.Assign(useExport.DecreaseCashReceiptEvent);
    export.DecreaseCashReceipt.Assign(useExport.DecreaseCashReceipt);
    export.DecreaseCrAdjAmt.TotalCurrency =
      useExport.DecreaseCrAdjAmt.TotalCurrency;
    export.DecreaseNetReceiptAmt.TotalCurrency =
      useExport.DecreaseNetReceiptAmt.TotalCurrency;
    export.DecraseHidden.SystemGeneratedIdentifier =
      useExport.DecreaseCashReceiptStatus.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsDeposited.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeposited.SystemGeneratedIdentifier;
  }

  private void UseFnUpdateCrAdjDescription()
  {
    var useImport = new FnUpdateCrAdjDescription.Import();
    var useExport = new FnUpdateCrAdjDescription.Export();

    useImport.CashReceiptRlnRsn.SystemGeneratedIdentifier =
      export.CashReceiptRlnRsn.SystemGeneratedIdentifier;
    MoveCashReceiptBalanceAdjustment2(export.CashReceiptBalanceAdjustment,
      useImport.CashReceiptBalanceAdjustment);
    useImport.Increase.SequentialNumber =
      export.IncreaseCashReceipt.SequentialNumber;
    useImport.Decrease.SequentialNumber =
      export.DecreaseCashReceipt.SequentialNumber;

    Call(FnUpdateCrAdjDescription.Execute, useImport, useExport);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of AdjustmentReasonPrompt.
    /// </summary>
    [JsonPropertyName("adjustmentReasonPrompt")]
    public Standard AdjustmentReasonPrompt
    {
      get => adjustmentReasonPrompt ??= new();
      set => adjustmentReasonPrompt = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptSourceType")]
    public CashReceiptSourceType IncreaseCashReceiptSourceType
    {
      get => increaseCashReceiptSourceType ??= new();
      set => increaseCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptEvent")]
    public CashReceiptEvent IncreaseCashReceiptEvent
    {
      get => increaseCashReceiptEvent ??= new();
      set => increaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("increaseCashReceipt")]
    public CashReceipt IncreaseCashReceipt
    {
      get => increaseCashReceipt ??= new();
      set => increaseCashReceipt = value;
    }

    /// <summary>
    /// A value of IncreaseCrPrompt.
    /// </summary>
    [JsonPropertyName("increaseCrPrompt")]
    public Standard IncreaseCrPrompt
    {
      get => increaseCrPrompt ??= new();
      set => increaseCrPrompt = value;
    }

    /// <summary>
    /// A value of IncreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseCrAdjAmt")]
    public Common IncreaseCrAdjAmt
    {
      get => increaseCrAdjAmt ??= new();
      set => increaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("increaseNetReceiptAmt")]
    public Common IncreaseNetReceiptAmt
    {
      get => increaseNetReceiptAmt ??= new();
      set => increaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptSourceType")]
    public CashReceiptSourceType DecreaseCashReceiptSourceType
    {
      get => decreaseCashReceiptSourceType ??= new();
      set => decreaseCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptEvent")]
    public CashReceiptEvent DecreaseCashReceiptEvent
    {
      get => decreaseCashReceiptEvent ??= new();
      set => decreaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("decreaseCashReceipt")]
    public CashReceipt DecreaseCashReceipt
    {
      get => decreaseCashReceipt ??= new();
      set => decreaseCashReceipt = value;
    }

    /// <summary>
    /// A value of DecreaseCrPrompt.
    /// </summary>
    [JsonPropertyName("decreaseCrPrompt")]
    public Standard DecreaseCrPrompt
    {
      get => decreaseCrPrompt ??= new();
      set => decreaseCrPrompt = value;
    }

    /// <summary>
    /// A value of DecreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseCrAdjAmt")]
    public Common DecreaseCrAdjAmt
    {
      get => decreaseCrAdjAmt ??= new();
      set => decreaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetReceiptAmt")]
    public Common DecreaseNetReceiptAmt
    {
      get => decreaseNetReceiptAmt ??= new();
      set => decreaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptRlnRsn")]
    public CashReceiptRlnRsn HiddenCashReceiptRlnRsn
    {
      get => hiddenCashReceiptRlnRsn ??= new();
      set => hiddenCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment HiddenCashReceiptBalanceAdjustment
    {
      get => hiddenCashReceiptBalanceAdjustment ??= new();
      set => hiddenCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of HiddenIncrease.
    /// </summary>
    [JsonPropertyName("hiddenIncrease")]
    public CashReceipt HiddenIncrease
    {
      get => hiddenIncrease ??= new();
      set => hiddenIncrease = value;
    }

    /// <summary>
    /// A value of HiddenDecrease.
    /// </summary>
    [JsonPropertyName("hiddenDecrease")]
    public CashReceipt HiddenDecrease
    {
      get => hiddenDecrease ??= new();
      set => hiddenDecrease = value;
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
    /// A value of CallingPrad.
    /// </summary>
    [JsonPropertyName("callingPrad")]
    public Standard CallingPrad
    {
      get => callingPrad ??= new();
      set => callingPrad = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptRlnRsn")]
    public CashReceiptRlnRsn PassAreaCashReceiptRlnRsn
    {
      get => passAreaCashReceiptRlnRsn ??= new();
      set => passAreaCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of PassAreaCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("passAreaCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment PassAreaCashReceiptBalanceAdjustment
    {
      get => passAreaCashReceiptBalanceAdjustment ??= new();
      set => passAreaCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of PassAreaSelection.
    /// </summary>
    [JsonPropertyName("passAreaSelection")]
    public CashReceipt PassAreaSelection
    {
      get => passAreaSelection ??= new();
      set => passAreaSelection = value;
    }

    /// <summary>
    /// A value of PassAreaIncrease.
    /// </summary>
    [JsonPropertyName("passAreaIncrease")]
    public CashReceipt PassAreaIncrease
    {
      get => passAreaIncrease ??= new();
      set => passAreaIncrease = value;
    }

    /// <summary>
    /// A value of PassAreaDecrease.
    /// </summary>
    [JsonPropertyName("passAreaDecrease")]
    public CashReceipt PassAreaDecrease
    {
      get => passAreaDecrease ??= new();
      set => passAreaDecrease = value;
    }

    /// <summary>
    /// A value of DecreaseHidden.
    /// </summary>
    [JsonPropertyName("decreaseHidden")]
    public CashReceiptStatus DecreaseHidden
    {
      get => decreaseHidden ??= new();
      set => decreaseHidden = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private Standard adjustmentReasonPrompt;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptSourceType increaseCashReceiptSourceType;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceipt increaseCashReceipt;
    private Standard increaseCrPrompt;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceiptSourceType decreaseCashReceiptSourceType;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceipt decreaseCashReceipt;
    private Standard decreaseCrPrompt;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetReceiptAmt;
    private CashReceiptRlnRsn hiddenCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment hiddenCashReceiptBalanceAdjustment;
    private CashReceipt hiddenIncrease;
    private CashReceipt hiddenDecrease;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Standard callingPrad;
    private CashReceiptRlnRsn passAreaCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment passAreaCashReceiptBalanceAdjustment;
    private CashReceipt passAreaSelection;
    private CashReceipt passAreaIncrease;
    private CashReceipt passAreaDecrease;
    private CashReceiptStatus decreaseHidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of AdjustmentReasonPrompt.
    /// </summary>
    [JsonPropertyName("adjustmentReasonPrompt")]
    public Standard AdjustmentReasonPrompt
    {
      get => adjustmentReasonPrompt ??= new();
      set => adjustmentReasonPrompt = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptSourceType")]
    public CashReceiptSourceType IncreaseCashReceiptSourceType
    {
      get => increaseCashReceiptSourceType ??= new();
      set => increaseCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("increaseCashReceiptEvent")]
    public CashReceiptEvent IncreaseCashReceiptEvent
    {
      get => increaseCashReceiptEvent ??= new();
      set => increaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of IncreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("increaseCashReceipt")]
    public CashReceipt IncreaseCashReceipt
    {
      get => increaseCashReceipt ??= new();
      set => increaseCashReceipt = value;
    }

    /// <summary>
    /// A value of IncreaseCrPrompt.
    /// </summary>
    [JsonPropertyName("increaseCrPrompt")]
    public Standard IncreaseCrPrompt
    {
      get => increaseCrPrompt ??= new();
      set => increaseCrPrompt = value;
    }

    /// <summary>
    /// A value of IncreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseCrAdjAmt")]
    public Common IncreaseCrAdjAmt
    {
      get => increaseCrAdjAmt ??= new();
      set => increaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("increaseNetReceiptAmt")]
    public Common IncreaseNetReceiptAmt
    {
      get => increaseNetReceiptAmt ??= new();
      set => increaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptSourceType")]
    public CashReceiptSourceType DecreaseCashReceiptSourceType
    {
      get => decreaseCashReceiptSourceType ??= new();
      set => decreaseCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("decreaseCashReceiptEvent")]
    public CashReceiptEvent DecreaseCashReceiptEvent
    {
      get => decreaseCashReceiptEvent ??= new();
      set => decreaseCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of DecreaseCashReceipt.
    /// </summary>
    [JsonPropertyName("decreaseCashReceipt")]
    public CashReceipt DecreaseCashReceipt
    {
      get => decreaseCashReceipt ??= new();
      set => decreaseCashReceipt = value;
    }

    /// <summary>
    /// A value of DecreaseCrPrompt.
    /// </summary>
    [JsonPropertyName("decreaseCrPrompt")]
    public Standard DecreaseCrPrompt
    {
      get => decreaseCrPrompt ??= new();
      set => decreaseCrPrompt = value;
    }

    /// <summary>
    /// A value of DecreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("decreaseCrAdjAmt")]
    public Common DecreaseCrAdjAmt
    {
      get => decreaseCrAdjAmt ??= new();
      set => decreaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of DecreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("decreaseNetReceiptAmt")]
    public Common DecreaseNetReceiptAmt
    {
      get => decreaseNetReceiptAmt ??= new();
      set => decreaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptRlnRsn")]
    public CashReceiptRlnRsn HiddenCashReceiptRlnRsn
    {
      get => hiddenCashReceiptRlnRsn ??= new();
      set => hiddenCashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment HiddenCashReceiptBalanceAdjustment
    {
      get => hiddenCashReceiptBalanceAdjustment ??= new();
      set => hiddenCashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of HiddenIncrease.
    /// </summary>
    [JsonPropertyName("hiddenIncrease")]
    public CashReceipt HiddenIncrease
    {
      get => hiddenIncrease ??= new();
      set => hiddenIncrease = value;
    }

    /// <summary>
    /// A value of HiddenDecrease.
    /// </summary>
    [JsonPropertyName("hiddenDecrease")]
    public CashReceipt HiddenDecrease
    {
      get => hiddenDecrease ??= new();
      set => hiddenDecrease = value;
    }

    /// <summary>
    /// A value of CallingPrad.
    /// </summary>
    [JsonPropertyName("callingPrad")]
    public Standard CallingPrad
    {
      get => callingPrad ??= new();
      set => callingPrad = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public CashReceiptSourceType PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CashReceipt Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of DecraseHidden.
    /// </summary>
    [JsonPropertyName("decraseHidden")]
    public CashReceiptStatus DecraseHidden
    {
      get => decraseHidden ??= new();
      set => decraseHidden = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private Standard adjustmentReasonPrompt;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptSourceType increaseCashReceiptSourceType;
    private CashReceiptEvent increaseCashReceiptEvent;
    private CashReceipt increaseCashReceipt;
    private Standard increaseCrPrompt;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceiptSourceType decreaseCashReceiptSourceType;
    private CashReceiptEvent decreaseCashReceiptEvent;
    private CashReceipt decreaseCashReceipt;
    private Standard decreaseCrPrompt;
    private Common decreaseCrAdjAmt;
    private Common decreaseNetReceiptAmt;
    private CashReceiptRlnRsn hiddenCashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment hiddenCashReceiptBalanceAdjustment;
    private CashReceipt hiddenIncrease;
    private CashReceipt hiddenDecrease;
    private Standard callingPrad;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptSourceType passArea;
    private CashReceipt pass;
    private CashReceiptStatus decraseHidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of IncreaseCrAdjAmt.
    /// </summary>
    [JsonPropertyName("increaseCrAdjAmt")]
    public Common IncreaseCrAdjAmt
    {
      get => increaseCrAdjAmt ??= new();
      set => increaseCrAdjAmt = value;
    }

    /// <summary>
    /// A value of IncreaseNetReceiptAmt.
    /// </summary>
    [JsonPropertyName("increaseNetReceiptAmt")]
    public Common IncreaseNetReceiptAmt
    {
      get => increaseNetReceiptAmt ??= new();
      set => increaseNetReceiptAmt = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CrAdjAmt.
    /// </summary>
    [JsonPropertyName("crAdjAmt")]
    public Common CrAdjAmt
    {
      get => crAdjAmt ??= new();
      set => crAdjAmt = value;
    }

    /// <summary>
    /// A value of NetReceiptAmt.
    /// </summary>
    [JsonPropertyName("netReceiptAmt")]
    public Common NetReceiptAmt
    {
      get => netReceiptAmt ??= new();
      set => netReceiptAmt = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public CashReceipt Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CashReceiptBalanceAdjustment Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeposited.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeposited")]
    public CashReceiptStatus HardcodedCrsDeposited
    {
      get => hardcodedCrsDeposited ??= new();
      set => hardcodedCrsDeposited = value;
    }

    private DateWorkArea null1;
    private Common common;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceipt increase;
    private Common increaseCrAdjAmt;
    private Common increaseNetReceiptAmt;
    private CashReceipt cashReceipt;
    private Common crAdjAmt;
    private Common netReceiptAmt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt hidden;
    private CashReceiptBalanceAdjustment clear;
    private CashReceiptStatus hardcodedCrsDeposited;
  }
#endregion
}

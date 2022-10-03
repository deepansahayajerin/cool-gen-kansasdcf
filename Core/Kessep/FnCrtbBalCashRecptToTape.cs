// Program: FN_CRTB_BAL_CASH_RECPT_TO_TAPE, ID: 371727173, model: 746.
// Short name: SWECRTBP
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
/// A program: FN_CRTB_BAL_CASH_RECPT_TO_TAPE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrtbBalCashRecptToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRTB_BAL_CASH_RECPT_TO_TAPE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrtbBalCashRecptToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrtbBalCashRecptToTape.
  /// </summary>
  public FnCrtbBalCashRecptToTape(IContext context, Import import, Export export)
    :
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
    // ----------------------------------------------------------------------------
    // Every initial development and change to that development
    // needs to be documented.
    // ----------------------------------------------------------------------------
    // Date 	  Developer Name	Description
    // ------	  ----------------	-------------
    // 02/07/96  Holly Kennedy-MTW	Retrofits
    // 03/25/96  Holly Kennedy-MTW	Added logic to prevent status changes
    // 				from invalid starting statuses.
    // 04/02/96  Holly Kennedy-MTW	Made changes to entire groups of
    // 				Processing logic that did not work
    // 				correctly.
    // 04/02/96  Holly Kennedy-MTW	Fixed confirm logic. Was setting
    // 				Balance timestamp to a view that was
    // 				never populated. Should be current
    // 				timestamp.
    // 10/15/96  Holly Kennedy-MTW	Added data level security and
    // 				fixed read problem.
    // 11/26/96  R. Marchman		Fix next tran
    // 04/29/97  JeHoward		Current date fix.
    // 06/18/97  M. D. Wheaton		Removed datenum
    // 11/14/97  Anita Massey		Added delete functionality with
    // 				a flow to CRDE.
    // 10/14/98  J. Katz		Do not allow Mach Tape Amt to be
    // 				negative.
    // 				Ensure that only active status is
    // 				considered when population list.
    // 				Remove extraneous code and views.
    // 				Allow both cash and non-cash receipts
    // 				to be BAL and DEP.
    // 10/16/98  J. Katz		Only allow non-cash receipts with a
    // 				type code of RDIR PMT and CSENet
    // 				to be displayed, forwarded, or
    // 				deposited on CRTB.
    // 				BAL and FWD action can only be
    // 				performed by person who created receipt.
    // 06/04/99  J. Katz		Analyzed READ statements and set
    // 				read property to Select Only where
    // 				appropriate.
    // 12/14/99	Sunya Sharp	Increased group view size.
    // ----------------------------------------------------------------------------
    // 03/07/00  pdp   H00089901       Allow Chief Security to
    // Process Receipts Created by others
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // ----------------------------------------------------------
    //                   Next Tran Logic
    // If the next tran info is not equal to spaces, this implies
    // the user requested a next tran action.  Now validate.
    // ----------------------------------------------------------
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
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
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    // -----------------------------------------
    // Security Validation
    // -----------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "BALANCE") || Equal(global.Command, "DEPOSIT"))
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

    // -----------------------------------------
    // Initialize Local Views
    // -----------------------------------------
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    UseFnHardcodedCashReceipting();

    // ----------------------------------------------------------------
    // The following set statements are required temporarily until the
    // Hardcoded Cash Receipting CAB is modified to include all of the
    // possible Cash Receipt Type codes.
    // ---------------------------------------------------------------
    local.HardcodedCrtCsenet.SystemGeneratedIdentifier = 8;
    local.HardcodedCrtRdirPmt.SystemGeneratedIdentifier = 11;

    // -----------------------------------------
    // Move Imports to Exports
    // -----------------------------------------
    export.WorkerId.CreatedBy = import.WorkerId.CreatedBy;
    MoveCommon(import.SysCalc, export.SysCalc);
    export.MachTapeAmt.TotalCurrency = import.MachTapeAmt.TotalCurrency;
    export.Difference.TotalCurrency = import.Difference.TotalCurrency;
    MoveFundTransaction(import.FundTransaction, export.FundTransaction);
    export.HiddenDisplayOk.Flag = import.HiddenDisplayOk.Flag;
    export.HiddenBalanceFlag.Flag = import.HiddenBalanceFlag.Flag;

    if (IsEmpty(import.WorkerId.CreatedBy))
    {
      export.WorkerId.CreatedBy = global.UserId;
    }

    if (!Equal(global.Command, "DISPLAY"))
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

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailCashReceipt.Assign(
          import.Import1.Item.DetailCashReceipt);
        MoveCashReceiptSourceType(import.Import1.Item.
          DetailCashReceiptSourceType,
          export.Export1.Update.DetailCashReceiptSourceType);
        MoveCashReceiptStatus(import.Import1.Item.Status,
          export.Export1.Update.Status);
        export.Export1.Update.DetailCashReceiptType.Assign(
          import.Import1.Item.DetailCashReceiptType);
        export.Export1.Update.HiddenDetail.SystemGeneratedIdentifier =
          import.Import1.Item.HiddenDetail.SystemGeneratedIdentifier;
        export.Export1.Next();
      }
    }

    // -----------------------------------------
    // Main Case of Command
    // -----------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        export.SysCalc.Count = 0;
        export.SysCalc.TotalCurrency = 0;
        export.MachTapeAmt.TotalCurrency = 0;
        export.Difference.TotalCurrency = 0;
        export.FundTransaction.Amount = 0;
        export.FundTransaction.DepositNumber = 0;

        // -----------------------------------------
        // Generate list of Cash Receipts
        // -----------------------------------------
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadCashReceipt())
        {
          if (!ReadCashReceiptStatusHistory())
          {
            ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";
            export.Export1.Next();

            return;
          }

          if (ReadCashReceiptStatus())
          {
            if (entities.CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedCrsReceipted.SystemGeneratedIdentifier || entities
              .CashReceiptStatus.SystemGeneratedIdentifier == local
              .HardcodedCrsBalanced.SystemGeneratedIdentifier)
            {
              // Receipt should be included in group list.
              // Continue.
            }
            else
            {
              export.Export1.Next();

              continue;
            }
          }
          else
          {
            ExitState = "FN0108_CASH_RCPT_STAT_NF";
            export.Export1.Next();

            return;
          }

          if (ReadCashReceiptEvent())
          {
            if (!ReadCashReceiptSourceType())
            {
              ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
              export.Export1.Next();

              return;
            }
          }
          else
          {
            ExitState = "FN0000_CASH_RCPT_EVENT_NF";
            export.Export1.Next();

            return;
          }

          if (!ReadCashReceiptType())
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";
            export.Export1.Next();

            return;
          }

          // ---------------------------------------------------------------
          // Validate that receipt type for current cash receipt is either
          // non-cash RDIR PMT, non-cash CSENet, or a cash type.
          // ---------------------------------------------------------------
          if (AsChar(entities.CashReceiptType.CategoryIndicator) == AsChar
            (local.HardcodedCrtCatNonCash.CategoryIndicator))
          {
            if (entities.CashReceiptType.SystemGeneratedIdentifier == local
              .HardcodedCrtCsenet.SystemGeneratedIdentifier || entities
              .CashReceiptType.SystemGeneratedIdentifier == local
              .HardcodedCrtRdirPmt.SystemGeneratedIdentifier)
            {
              // ---------------------------------------------------------------
              // Receipt type is a valid non-cash receipt type for
              // CRTB display, balance, and deposit actions.
              // ---------------------------------------------------------------
            }
            else
            {
              // ---------------------------------------------------------------
              // Receipt type is not a valid non-cash receipt type for
              // CRTB display, balance, and deposit actions.
              // ---------------------------------------------------------------
              export.Export1.Next();

              continue;
            }
          }
          else
          {
            // ---------------------------------------------------------------
            // Receipt type is cash -- continue processing.
            // ---------------------------------------------------------------
          }

          // ---------------------------------------------------------------
          // Increment system calculated count by 1.
          // Increment system calculated amount by amount of receipt.
          // ---------------------------------------------------------------
          ++export.SysCalc.Count;
          export.SysCalc.TotalCurrency += export.Export1.Item.DetailCashReceipt.
            ReceiptAmount;

          // ---------------------------------------------------------------
          // Move entity action views to group export views.
          // ---------------------------------------------------------------
          export.Export1.Update.DetailCashReceipt.Assign(entities.Eav);
          MoveCashReceiptSourceType(entities.CashReceiptSourceType,
            export.Export1.Update.DetailCashReceiptSourceType);
          MoveCashReceiptStatus(entities.CashReceiptStatus,
            export.Export1.Update.Status);
          export.Export1.Update.DetailCashReceiptType.Assign(
            entities.CashReceiptType);
          export.Export1.Update.HiddenDetail.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "FN0000_NO_RELEVANT_CR_FOUND";
        }
        else if (export.Export1.IsFull)
        {
          ExitState = "FN0306_CR_EXCEED_GRP_VIEW_SIZE";
        }
        else
        {
          ExitState = "FN0000_CASH_RCPT_SUCCESSFUL_DISP";
        }

        export.HiddenDisplayOk.Flag = "Y";

        break;
      case "DELETE":
        if (export.Export1.IsEmpty)
        {
          ExitState = "NO_CASH_RECEIPTS_AVAILABLE";

          return;
        }

        if (Equal(export.WorkerId.CreatedBy, "SWEFB610"))
        {
          // ---------------------------------------------------------------
          // Allow the cash receipt to be processed if it was created by a
          // batch job.    JLK  11/09/98
          // ---------------------------------------------------------------
        }
        else if (!Equal(export.WorkerId.CreatedBy, global.UserId))
        {
          // 03/07/00  pdp   H00089901       Allow Chief Security to
          // Balance Receipts Created by others
          if (ReadServiceProviderProfileServiceProviderProfile2())
          {
            goto Test1;
          }

          // ---------------------------------------------------------------
          // A cash receipt can only be deleted by the worker who
          // created the receipt.    JLK  10/16/98
          // ---------------------------------------------------------------
          ExitState = "ACO_NE0000_USR_NOT_AUTH";

          return;
        }

Test1:

        // ---------------------------------------------------------------
        //               Delete Selected Cash Receipt
        //              
        // -------------------------------
        // Get the selected row and move the group export views to
        // the export flow views.
        // ---------------------------------------------------------------
        local.SelectionCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              export.FlowCashReceipt.Assign(
                export.Export1.Item.DetailCashReceipt);
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.FlowCashReceiptSourceType);
              export.FlowCashReceiptType.Assign(
                export.Export1.Item.DetailCashReceiptType);
              export.FlowCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenDetail.SystemGeneratedIdentifier;
              MoveCashReceiptStatus(export.Export1.Item.Status, local.Selected);

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ++local.SelectionCount.Count;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              var field2 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field2.Error = true;

              goto Test5;
          }
        }

        // ---------------------------------------------------------------
        // Validate that a single receipt was selected.
        // Only allow flow to CRDE if active status is Receipted.
        // ---------------------------------------------------------------
        if (local.SelectionCount.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            break;
          }

          ExitState = "FN0139_CASH_RCPT_MUST_BE_SEL";
        }
        else if (local.SelectionCount.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
        else
        {
          // ---------------------------------------------------------------
          // Only allow flow to CRDE if active status is Receipted.
          // Link to the Delete Cash Receipt screen (CRDE) to allow the
          // user to enter a reason code for the delete action.
          // ---------------------------------------------------------------
          if (local.Selected.SystemGeneratedIdentifier == local
            .HardcodedCrsReceipted.SystemGeneratedIdentifier)
          {
            ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

            return;
          }
          else
          {
            ExitState = "FN0000_CANNOT_DELETE_STATUS_TYPE";

            break;
          }

          if (!Equal(entities.Eav.ReceiptDate, local.Current.Date))
          {
            ExitState = "FN0303_CR_CANNOT_BE_DELETED";
          }
        }

        break;
      case "BALANCE":
        if (export.Export1.IsEmpty)
        {
          ExitState = "NO_CASH_RECEIPTS_AVAILABLE";

          return;
        }

        if (AsChar(export.HiddenDisplayOk.Flag) != 'Y')
        {
          ExitState = "FN0000_DISPLAY_BEFORE_BALANCING";

          break;
        }

        if (AsChar(export.HiddenBalanceFlag.Flag) == 'Y')
        {
          ExitState = "FN0127_CASH_RCPT_ALREADY_BALANCE";

          break;
        }

        if (Equal(export.WorkerId.CreatedBy, "SWEFB610"))
        {
          // ---------------------------------------------------------------
          // Allow the cash receipt to be processed if it was created by a
          // batch job.    JLK  11/09/98
          // ---------------------------------------------------------------
        }
        else if (!Equal(export.WorkerId.CreatedBy, global.UserId))
        {
          // 03/07/00  pdp   H00089901       Allow Chief Security to
          // Balance Receipts Created by others
          if (ReadServiceProviderProfileServiceProviderProfile1())
          {
            goto Test2;
          }

          // ---------------------------------------------------------------
          // A cash receipt can only be balanced by the worker who
          // created the receipt.    JLK  10/16/98
          // ---------------------------------------------------------------
          ExitState = "ACO_NE0000_USR_NOT_AUTH";

          return;
        }

Test2:

        // ---------------------------------------------------------------
        // Validate Machine Tape Amount entered by the Worker.
        // ---------------------------------------------------------------
        if (export.MachTapeAmt.TotalCurrency == 0)
        {
          var field = GetField(export.MachTapeAmt, "totalCurrency");

          field.Error = true;

          ExitState = "FN0000_MACHINE_TAPE_AMT_REQD";

          break;
        }
        else if (export.MachTapeAmt.TotalCurrency < 0)
        {
          var field = GetField(export.MachTapeAmt, "totalCurrency");

          field.Error = true;

          ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";

          break;
        }
        else
        {
          var field = GetField(export.SysCalc, "totalCurrency");

          field.Color = "cyan";
          field.Protected = true;
        }

        // ---------------------------------------------------------------
        // Determine if receipts are in balance.
        // ---------------------------------------------------------------
        export.Difference.TotalCurrency = 0;
        export.SysCalc.Count = 0;
        export.SysCalc.TotalCurrency = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          ++export.SysCalc.Count;
          export.SysCalc.TotalCurrency += export.Export1.Item.DetailCashReceipt.
            ReceiptAmount;
        }

        if (export.MachTapeAmt.TotalCurrency != export.SysCalc.TotalCurrency)
        {
          ExitState = "OUT_OF_BALANCE";
          export.Difference.TotalCurrency = export.MachTapeAmt.TotalCurrency - export
            .SysCalc.TotalCurrency;

          var field1 = GetField(export.Difference, "totalCurrency");

          field1.Color = "red";
          field1.Intensity = Intensity.High;
          field1.Highlighting = Highlighting.Normal;
          field1.Protected = true;

          var field2 = GetField(export.MachTapeAmt, "totalCurrency");

          field2.Error = true;

          return;
        }
        else
        {
          var field = GetField(export.MachTapeAmt, "totalCurrency");

          field.Color = "cyan";
          field.Protected = true;

          export.HiddenBalanceFlag.Flag = "Y";
        }

        // ---------------------------------------------------------------
        // Receipts are in Balance.  Change status of each receipt to BAL.
        // ---------------------------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!Equal(export.Export1.Item.Status.Code, "BAL"))
          {
            export.Export1.Update.DetailCashReceipt.BalancedTimestamp =
              local.Current.Timestamp;
            UseRecordBalancedReceipt();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Export1.Update.Status.Code = "BAL";

              continue;
            }
            else if (IsExitState("FN0101_CASH_RCPT_STAT_HIST_AE_RB"))
            {
              ExitState = "FN0101_CASH_RCPT_STAT_HIST_AE_RB";
            }
            else if (IsExitState("FN0104_CASH_RCPT_STAT_HIST_PV"))
            {
              ExitState = "FN0105_CASH_RCPT_STAT_HIST_PV_RB";
            }
            else
            {
            }

            var field =
              GetField(export.Export1.Item.DetailCashReceipt, "sequentialNumber");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;

            goto Test5;
          }
        }

        ExitState = "FN0129_CASH_RCPT_ARE_BALANCED";

        break;
      case "CRFO":
        if (export.Export1.IsEmpty)
        {
          ExitState = "NO_CASH_RECEIPTS_AVAILABLE";

          return;
        }

        if (AsChar(export.HiddenBalanceFlag.Flag) != 'Y')
        {
          ExitState = "FN0000_BALANCE_BEFORE_FWD_OR_DEP";

          break;
        }

        if (Equal(export.WorkerId.CreatedBy, "SWEFB610"))
        {
          // ---------------------------------------------------------------
          // Allow the cash receipt to be processed if it was created by a
          // batch job.    JLK  11/09/98
          // ---------------------------------------------------------------
        }
        else if (!Equal(export.WorkerId.CreatedBy, global.UserId))
        {
          // 03/07/00  pdp   H00089901       Allow Chief Security to
          // Balance Receipts Created by others
          if (ReadServiceProviderProfileServiceProviderProfile1())
          {
            goto Test3;
          }

          // ---------------------------------------------------------------
          // A cash receipt can only be deleted by the worker who
          // created the receipt.    JLK  10/16/98
          // ---------------------------------------------------------------
          ExitState = "ACO_NE0000_USR_NOT_AUTH";

          return;
        }

Test3:

        local.SelectionCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              export.FlowCashReceipt.Assign(
                export.Export1.Item.DetailCashReceipt);
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.FlowCashReceiptSourceType);
              export.FlowCashReceiptType.Assign(
                export.Export1.Item.DetailCashReceiptType);
              export.FlowCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenDetail.SystemGeneratedIdentifier;
              MoveCashReceiptStatus(export.Export1.Item.Status, local.Selected);

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ++local.SelectionCount.Count;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              var field2 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field2.Error = true;

              goto Test5;
          }
        }

        if (local.SelectionCount.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Highlighting = Highlighting.Underscore;
            field.Protected = false;
            field.Focused = true;

            break;
          }

          ExitState = "FN0139_CASH_RCPT_SELECTION_REQD";
        }
        else if (local.SelectionCount.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
        else
        {
          if (AsChar(export.FlowCashReceiptType.CategoryIndicator) == AsChar
            (local.HardcodedCrtCatNonCash.CategoryIndicator))
          {
            ExitState = "FN0000_INVALID_CR_TYPE_4_FWDING";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CRFO";
          }

          return;
        }

        break;
      case "DEPOSIT":
        if (export.Export1.IsEmpty)
        {
          ExitState = "NO_CASH_RECEIPTS_AVAILABLE";

          return;
        }

        if (AsChar(export.HiddenBalanceFlag.Flag) != 'Y')
        {
          ExitState = "FN0000_BALANCE_BEFORE_FWD_OR_DEP";

          break;
        }

        if (Equal(export.WorkerId.CreatedBy, "SWEFB610"))
        {
          // ---------------------------------------------------------------
          // Allow the cash receipt to be processed if it was created by a
          // batch job.    JLK  11/09/98
          // ---------------------------------------------------------------
        }
        else if (!Equal(export.WorkerId.CreatedBy, global.UserId))
        {
          // 03/07/00  pdp   H00089901       Allow Chief Security to
          // Balance Receipts Created by others
          if (ReadServiceProviderProfileServiceProviderProfile1())
          {
            goto Test4;
          }

          // ---------------------------------------------------------------
          // A cash receipt can only be deleted by the worker who
          // created the receipt.    JLK  10/16/98
          // ---------------------------------------------------------------
          ExitState = "ACO_NE0000_USR_NOT_AUTH";

          return;
        }

Test4:

        export.FundTransaction.DepositNumber = 0;
        export.FundTransaction.Amount = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          UseFnRelCashRcptsForDeposit();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Export1.Update.Status.Code = "DEP";

            continue;
          }
          else
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceipt, "sequentialNumber");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;

            goto Test5;
          }
        }

        ExitState = "DEPOSITING_COMPLETE";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        local.SelectionCount.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              export.FlowCashReceipt.Assign(
                export.Export1.Item.DetailCashReceipt);
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.FlowCashReceiptSourceType);
              export.FlowCashReceiptType.Assign(
                export.Export1.Item.DetailCashReceiptType);
              export.FlowCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenDetail.SystemGeneratedIdentifier;
              MoveCashReceiptStatus(export.Export1.Item.Status, local.Selected);

              var field1 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field1.Error = true;

              ++local.SelectionCount.Count;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              var field2 =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field2.Error = true;

              return;
          }
        }

        if (local.SelectionCount.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
        }
        else
        {
          ExitState = "ACO_NE0000_RETURN";
        }

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

Test5:

    export.HiddenBalanceFlag.Flag = "Y";

    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (Equal(export.Export1.Item.Status.Code, "REC"))
      {
        if (export.MachTapeAmt.TotalCurrency == 0)
        {
          var field = GetField(export.SysCalc, "totalCurrency");

          field.Intensity = Intensity.Dark;
          field.Protected = true;
        }

        export.HiddenBalanceFlag.Flag = "N";

        break;
      }
    }

    if (AsChar(export.HiddenBalanceFlag.Flag) == 'Y')
    {
      var field = GetField(export.MachTapeAmt, "totalCurrency");

      field.Protected = true;

      export.Difference.TotalCurrency = 0;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.BalancedTimestamp = source.BalancedTimestamp;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveFundTransaction(FundTransaction source,
    FundTransaction target)
  {
    target.DepositNumber = source.DepositNumber;
    target.Amount = source.Amount;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsReceipted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedCrsBalanced.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdBalanced.SystemGeneratedIdentifier;
    local.HardcodedCrtCatNonCash.CategoryIndicator =
      useExport.CrtCategory.CrtCatNonCash.CategoryIndicator;
  }

  private void UseFnRelCashRcptsForDeposit()
  {
    var useImport = new FnRelCashRcptsForDeposit.Import();
    var useExport = new FnRelCashRcptsForDeposit.Export();

    useImport.CashReceipt.SequentialNumber =
      export.Export1.Item.DetailCashReceipt.SequentialNumber;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenDetail.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Export1.Item.DetailCashReceiptSourceType.SystemGeneratedIdentifier;
      
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.DetailCashReceiptType.SystemGeneratedIdentifier;

    Call(FnRelCashRcptsForDeposit.Execute, useImport, useExport);

    MoveFundTransaction(useExport.New1, export.FundTransaction);
  }

  private void UseRecordBalancedReceipt()
  {
    var useImport = new RecordBalancedReceipt.Import();
    var useExport = new RecordBalancedReceipt.Export();

    MoveCashReceipt(export.Export1.Item.DetailCashReceipt, useImport.CashReceipt);
      
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenDetail.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Export1.Item.DetailCashReceiptSourceType.SystemGeneratedIdentifier;
      
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.DetailCashReceiptType.SystemGeneratedIdentifier;

    Call(RecordBalancedReceipt.Execute, useImport, useExport);
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCashReceipt()
  {
    return ReadEach("ReadCashReceipt",
      (db, command) =>
      {
        db.SetString(command, "createdBy", export.WorkerId.CreatedBy);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.Eav.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Eav.CstIdentifier = db.GetInt32(reader, 1);
        entities.Eav.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Eav.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.Eav.SequentialNumber = db.GetInt32(reader, 4);
        entities.Eav.ReceiptDate = db.GetDate(reader, 5);
        entities.Eav.CheckNumber = db.GetNullableString(reader, 6);
        entities.Eav.BalancedTimestamp = db.GetNullableDateTime(reader, 7);
        entities.Eav.CreatedBy = db.GetString(reader, 8);
        entities.Eav.CreatedTimestamp = db.GetDateTime(reader, 9);
        entities.Eav.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.Eav.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.Eav.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", entities.Eav.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Eav.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", entities.Eav.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", entities.Eav.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", entities.Eav.CrvIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.Eav.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.Eav.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadServiceProviderProfileServiceProviderProfile1()
  {
    entities.ServiceProvider.Populated = false;
    entities.ServiceProviderProfile.Populated = false;
    entities.Profile.Populated = false;

    return Read("ReadServiceProviderProfileServiceProviderProfile1",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 1);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 3);
        entities.Profile.Name = db.GetString(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.Populated = true;
        entities.ServiceProviderProfile.Populated = true;
        entities.Profile.Populated = true;
      });
  }

  private bool ReadServiceProviderProfileServiceProviderProfile2()
  {
    entities.ServiceProvider.Populated = false;
    entities.ServiceProviderProfile.Populated = false;
    entities.Profile.Populated = false;

    return Read("ReadServiceProviderProfileServiceProviderProfile2",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 1);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 3);
        entities.Profile.Name = db.GetString(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.Populated = true;
        entities.ServiceProviderProfile.Populated = true;
        entities.Profile.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of HiddenDetail.
      /// </summary>
      [JsonPropertyName("hiddenDetail")]
      public CashReceiptEvent HiddenDetail
      {
        get => hiddenDetail ??= new();
        set => hiddenDetail = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common detailCommon;
      private CashReceipt detailCashReceipt;
      private CashReceiptEvent hiddenDetail;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private CashReceiptStatus status;
      private CashReceiptType detailCashReceiptType;
    }

    /// <summary>
    /// A value of WorkerId.
    /// </summary>
    [JsonPropertyName("workerId")]
    public CashReceipt WorkerId
    {
      get => workerId ??= new();
      set => workerId = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of SysCalc.
    /// </summary>
    [JsonPropertyName("sysCalc")]
    public Common SysCalc
    {
      get => sysCalc ??= new();
      set => sysCalc = value;
    }

    /// <summary>
    /// A value of MachTapeAmt.
    /// </summary>
    [JsonPropertyName("machTapeAmt")]
    public Common MachTapeAmt
    {
      get => machTapeAmt ??= new();
      set => machTapeAmt = value;
    }

    /// <summary>
    /// A value of Difference.
    /// </summary>
    [JsonPropertyName("difference")]
    public Common Difference
    {
      get => difference ??= new();
      set => difference = value;
    }

    /// <summary>
    /// A value of HiddenBalanceFlag.
    /// </summary>
    [JsonPropertyName("hiddenBalanceFlag")]
    public Common HiddenBalanceFlag
    {
      get => hiddenBalanceFlag ??= new();
      set => hiddenBalanceFlag = value;
    }

    /// <summary>
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
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

    private CashReceipt workerId;
    private FundTransaction fundTransaction;
    private Common sysCalc;
    private Common machTapeAmt;
    private Common difference;
    private Common hiddenBalanceFlag;
    private Common hiddenDisplayOk;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<ImportGroup> import1;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of HiddenDetail.
      /// </summary>
      [JsonPropertyName("hiddenDetail")]
      public CashReceiptEvent HiddenDetail
      {
        get => hiddenDetail ??= new();
        set => hiddenDetail = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common detailCommon;
      private CashReceipt detailCashReceipt;
      private CashReceiptEvent hiddenDetail;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private CashReceiptStatus status;
      private CashReceiptType detailCashReceiptType;
    }

    /// <summary>
    /// A value of WorkerId.
    /// </summary>
    [JsonPropertyName("workerId")]
    public CashReceipt WorkerId
    {
      get => workerId ??= new();
      set => workerId = value;
    }

    /// <summary>
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    /// <summary>
    /// A value of SysCalc.
    /// </summary>
    [JsonPropertyName("sysCalc")]
    public Common SysCalc
    {
      get => sysCalc ??= new();
      set => sysCalc = value;
    }

    /// <summary>
    /// A value of MachTapeAmt.
    /// </summary>
    [JsonPropertyName("machTapeAmt")]
    public Common MachTapeAmt
    {
      get => machTapeAmt ??= new();
      set => machTapeAmt = value;
    }

    /// <summary>
    /// A value of Difference.
    /// </summary>
    [JsonPropertyName("difference")]
    public Common Difference
    {
      get => difference ??= new();
      set => difference = value;
    }

    /// <summary>
    /// A value of HiddenBalanceFlag.
    /// </summary>
    [JsonPropertyName("hiddenBalanceFlag")]
    public Common HiddenBalanceFlag
    {
      get => hiddenBalanceFlag ??= new();
      set => hiddenBalanceFlag = value;
    }

    /// <summary>
    /// A value of HiddenDisplayOk.
    /// </summary>
    [JsonPropertyName("hiddenDisplayOk")]
    public Common HiddenDisplayOk
    {
      get => hiddenDisplayOk ??= new();
      set => hiddenDisplayOk = value;
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
    /// A value of FlowCashReceipt.
    /// </summary>
    [JsonPropertyName("flowCashReceipt")]
    public CashReceipt FlowCashReceipt
    {
      get => flowCashReceipt ??= new();
      set => flowCashReceipt = value;
    }

    /// <summary>
    /// A value of FlowCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("flowCashReceiptEvent")]
    public CashReceiptEvent FlowCashReceiptEvent
    {
      get => flowCashReceiptEvent ??= new();
      set => flowCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of FlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("flowCashReceiptSourceType")]
    public CashReceiptSourceType FlowCashReceiptSourceType
    {
      get => flowCashReceiptSourceType ??= new();
      set => flowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of FlowCashReceiptType.
    /// </summary>
    [JsonPropertyName("flowCashReceiptType")]
    public CashReceiptType FlowCashReceiptType
    {
      get => flowCashReceiptType ??= new();
      set => flowCashReceiptType = value;
    }

    private CashReceipt workerId;
    private FundTransaction fundTransaction;
    private Common sysCalc;
    private Common machTapeAmt;
    private Common difference;
    private Common hiddenBalanceFlag;
    private Common hiddenDisplayOk;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<ExportGroup> export1;
    private CashReceipt flowCashReceipt;
    private CashReceiptEvent flowCashReceiptEvent;
    private CashReceiptSourceType flowCashReceiptSourceType;
    private CashReceiptType flowCashReceiptType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
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
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CashReceiptStatus Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HardcodedCrsReceipted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsReceipted")]
    public CashReceiptStatus HardcodedCrsReceipted
    {
      get => hardcodedCrsReceipted ??= new();
      set => hardcodedCrsReceipted = value;
    }

    /// <summary>
    /// A value of HardcodedCrsBalanced.
    /// </summary>
    [JsonPropertyName("hardcodedCrsBalanced")]
    public CashReceiptStatus HardcodedCrsBalanced
    {
      get => hardcodedCrsBalanced ??= new();
      set => hardcodedCrsBalanced = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCatNonCash.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCatNonCash")]
    public CashReceiptType HardcodedCrtCatNonCash
    {
      get => hardcodedCrtCatNonCash ??= new();
      set => hardcodedCrtCatNonCash = value;
    }

    /// <summary>
    /// A value of HardcodedCrtCsenet.
    /// </summary>
    [JsonPropertyName("hardcodedCrtCsenet")]
    public CashReceiptStatus HardcodedCrtCsenet
    {
      get => hardcodedCrtCsenet ??= new();
      set => hardcodedCrtCsenet = value;
    }

    /// <summary>
    /// A value of HardcodedCrtRdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtRdirPmt")]
    public CashReceiptStatus HardcodedCrtRdirPmt
    {
      get => hardcodedCrtRdirPmt ??= new();
      set => hardcodedCrtRdirPmt = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      current = null;
      maximum = null;
      selected = null;
      hardcodedCrsReceipted = null;
      hardcodedCrsBalanced = null;
      hardcodedCrtCatNonCash = null;
      hardcodedCrtCsenet = null;
      hardcodedCrtRdirPmt = null;
    }

    private DateWorkArea current;
    private DateWorkArea maximum;
    private Common selectionCount;
    private CashReceiptStatus selected;
    private CashReceiptStatus hardcodedCrsReceipted;
    private CashReceiptStatus hardcodedCrsBalanced;
    private CashReceiptType hardcodedCrtCatNonCash;
    private CashReceiptStatus hardcodedCrtCsenet;
    private CashReceiptStatus hardcodedCrtRdirPmt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Eav.
    /// </summary>
    [JsonPropertyName("eav")]
    public CashReceipt Eav
    {
      get => eav ??= new();
      set => eav = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
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

    private CashReceipt eav;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
  }
#endregion
}

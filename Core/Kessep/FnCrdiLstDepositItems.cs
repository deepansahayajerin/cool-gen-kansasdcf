// Program: FN_CRDI_LST_DEPOSIT_ITEMS, ID: 371768207, model: 746.
// Short name: SWECRDIP
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
/// A program: FN_CRDI_LST_DEPOSIT_ITEMS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdiLstDepositItems: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDI_LST_DEPOSIT_ITEMS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdiLstDepositItems(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdiLstDepositItems.
  /// </summary>
  public FnCrdiLstDepositItems(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // Date 	  Developer Name	Description
    // --------  -------------- 	
    // ---------------------------------
    // 02/20/96  Holly Kennedy-MTW	Retrofits
    // 04/02/96  Holly Kennedy-MTW	Made Multiple changes to this Prad
    // 				and associated action blocks.
    // 				Processing in general was not
    // 				working properly.
    // 05/20/96  Konkader		Print functions
    // 12/03/96  R. Marchman		Add new Security and next tran
    // 10/24/98  J. Katz		Remove unused logic for command
    // 				xxfmmenu.
    // 				Add logic for CRTB command.
    // 10/27/98  J. Katz		Redesign screen and add Check Type
    // 				filter per Screen Assessment.
    // 				Add flow to CRTB.
    // 10/29/98  J. Katz		Remove PF24 Print Slip functionality.
    // 				Add Cash Receipt Type filter.
    // 				Add Program Fund Type # of Items.
    // ------------------------------------------------------------------
    // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to explicit
    // because of the limit on number of lines
    // didplayed (130)   
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // --------------------------------------------------
    // Move all imports to exports
    // --------------------------------------------------
    export.SelectedFundTransaction.Assign(import.SelectedFundTransaction);
    export.SelectedFundTransactionStatus.Assign(
      import.SelectedFundTransactionStatus);
    export.ClosedDate.Date = import.ClosedDate.Date;
    export.DepositNoOfItems.Count = import.DepositNoOfItems.Count;
    MoveCommon(import.PftTotals, export.PftTotals);
    export.FilterStarting.SequentialNumber =
      import.FilterStarting.SequentialNumber;
    export.Filter.Code = import.Filter.Code;
    export.FilterPft.CheckType = import.FilterPft.CheckType;
    export.ReceiptTypePrompt.PromptField = import.ReceiptTypePrompt.PromptField;
    export.PftPrompt.PromptField = import.PftPrompt.PromptField;
    MoveFund(import.Fund, export.Fund);
    export.SelectedClosed.CreatedBy = import.SelectedClosed.CreatedBy;
    export.HiddenFundTransaction.DepositNumber =
      import.HiddenFundTransaction.DepositNumber;
    MoveProgramCostAccount(import.HiddenProgramCostAccount,
      export.HiddenProgramCostAccount);

    // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to explicit
    export.Pf7Pf8MoreIndicator.SelectChar =
      import.Pf7Pf8MoreIndicator.SelectChar;
    export.Pf7Pf8PriorIndicator.SelectChar =
      import.Pf7Pf8PriorIndicator.SelectChar;
    import.New1.Index = -1;
    export.New1.Index = -1;

    while(import.New1.Count > 0 && import.New1.Index < 11)
    {
      ++import.New1.Index;
      import.New1.CheckSize();

      ++export.New1.Index;
      export.New1.CheckSize();

      export.New1.Update.NewExportGroupMemberSelect.SelectChar =
        import.New1.Item.NewImportGroupMemberSelect.SelectChar;
      export.New1.Update.NewExportGroupMemberCashReceipt.Assign(
        import.New1.Item.NewImportGroupMemberCashReceipt);
      MoveCashReceiptSourceType(import.New1.Item.
        NewImportGroupMemberCashReceiptSourceType,
        export.New1.Update.NewExportGroupMemberCashReceiptSourceType);
      MoveCashReceiptType(import.New1.Item.NewImportGroupMemberCashReceiptType,
        export.New1.Update.NewExportGroupMemberCashReceiptType);
      MoveCashReceiptEvent(import.New1.Item.
        NewImportGroupMemberCashReceiptEvent,
        export.New1.Update.NewExportGroupMemberCashReceiptEvent);

      if (import.New1.Count <= export.New1.Index + 1)
      {
        export.New1.Count = import.New1.Count;

        break;
      }
    }

    import.Page.Index = -1;
    export.Page.Index = -1;

    while(import.Page.Count > 0 && import.Page.Index + 1 < Import
      .PageGroup.Capacity)
    {
      ++import.Page.Index;
      import.Page.CheckSize();

      ++export.Page.Index;
      export.Page.CheckSize();

      export.Page.Update.PageExportGroupMember.SequentialNumber =
        import.Page.Item.PageImportGroupMember.SequentialNumber;

      if (import.Page.Count <= export.Page.Index + 1)
      {
        export.Page.Count = import.Page.Count;

        break;
      }
    }

    // --------------------------------------------------
    // Next Tran Logic
    // --------------------------------------------------
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

    // --------------------------------------------------
    // Security Logic
    // --------------------------------------------------
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "REOPEN") || Equal
      (global.Command, "REMOVE") || Equal(global.Command, "CLOSE"))
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

    // --------------------------------------------------
    // Set up local views for Hardcoded information.
    // --------------------------------------------------
    UseHardcodedFundingInformation();

    // --------------------------------------------------------------
    // Main Case of Command
    // --------------------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // -----------------------------------------------------------
        // PF2  Display
        // This case will list all cash receipts for a specific
        // deposit number.
        // -----------------------------------------------------------
        // -----------------------------------------------------------------
        // Validate deposit information used to populate list.
        // -----------------------------------------------------------------
        if (import.SelectedFundTransaction.DepositNumber.GetValueOrDefault() ==
          0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
          export.HiddenFundTransaction.DepositNumber = 0;

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          MoveFund(local.InitializedFund, export.Fund);
          export.SelectedFundTransaction.
            Assign(local.InitializedFundTransaction);
          export.SelectedFundTransactionStatus.Assign(
            local.InitializedFundTransactionStatus);
          MoveProgramCostAccount(local.InitializedProgramCostAccount,
            export.HiddenProgramCostAccount);
          export.DepositNoOfItems.Count = 0;
          export.PftTotals.TotalCurrency = 0;
          export.PftTotals.Count = 0;

          // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
          // explicit
          // This is to reinitialize the view so that it shows up blank.
          export.New1.Count = 0;
          import.New1.Count = 0;
          export.Page.Count = 0;
          import.Page.Count = 0;

          return;
        }

        // -----------------------------------------------------------------
        // Validate Cash Receipt Type used for filter.
        // -----------------------------------------------------------------
        if (!IsEmpty(export.Filter.Code))
        {
          if (!ReadCashReceiptType1())
          {
            ExitState = "FN0000_CASH_RECEIPT_TYPE_NF";
            export.DepositNoOfItems.Count = 0;
            export.PftTotals.TotalCurrency = 0;
            export.PftTotals.Count = 0;

            // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
            // explicit
            // This is to reinitialize the view so that it shows up blank.
            export.New1.Count = 0;
            import.New1.Count = 0;
            export.Page.Count = 0;
            import.Page.Count = 0;

            return;
          }
        }

        // -----------------------------------------------------------------
        // Validate Check Type (Program Fund Type) used for filter.
        // -----------------------------------------------------------------
        if (!IsEmpty(export.FilterPft.CheckType))
        {
          local.Pass.CodeName = "CHECK TYPE";
          local.SelectedPft.Cdvalue = export.FilterPft.CheckType ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.Common.Count == 1 || local.Common.Count == 2)
          {
            var field = GetField(export.FilterPft, "checkType");

            field.Error = true;

            ExitState = "CODE_VALUE_NF";
            export.DepositNoOfItems.Count = 0;
            export.PftTotals.TotalCurrency = 0;
            export.PftTotals.Count = 0;

            // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
            // explicit
            // This is to reinitialize the view so that it shows up blank.
            export.New1.Count = 0;
            import.New1.Count = 0;
            export.Page.Count = 0;
            import.Page.Count = 0;

            return;
          }
        }

        // -----------------------------------------------------------------
        // Read Fund Transaction (Deposit) information.
        // -----------------------------------------------------------------
        if (ReadFundTransaction())
        {
          export.SelectedFundTransaction.Assign(entities.FundTransaction);
          export.HiddenFundTransaction.DepositNumber =
            entities.FundTransaction.DepositNumber;
        }
        else
        {
          ExitState = "FN0000_FUND_TRANS_NF";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          export.HiddenFundTransaction.DepositNumber = 0;
          MoveFund(local.InitializedFund, export.Fund);
          export.SelectedFundTransaction.
            Assign(local.InitializedFundTransaction);
          export.SelectedFundTransactionStatus.Assign(
            local.InitializedFundTransactionStatus);
          MoveProgramCostAccount(local.InitializedProgramCostAccount,
            export.HiddenProgramCostAccount);
          export.DepositNoOfItems.Count = 0;
          export.SelectedFundTransaction.DepositNumber =
            import.SelectedFundTransaction.DepositNumber.GetValueOrDefault();

          // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
          // explicit
          // This is to reinitialize the view so that it shows up blank.
          export.New1.Count = 0;
          import.New1.Count = 0;
          export.Page.Count = 0;
          import.Page.Count = 0;

          return;
        }

        if (ReadFundTransactionStatusFundTransactionStatusHistory())
        {
          export.SelectedFundTransactionStatus.Assign(
            entities.FundTransactionStatus);

          if (Equal(entities.FundTransactionStatus.Code, "CLOSED"))
          {
            export.SelectedClosed.CreatedBy =
              entities.FundTransactionStatusHistory.CreatedBy;
            export.ClosedDate.Date =
              Date(entities.FundTransactionStatusHistory.CreatedTimestamp);
          }
          else
          {
            export.SelectedClosed.CreatedBy = "";
            export.ClosedDate.Date = local.Null1.Date;
          }
        }

        // -----------------------------------------------------------------
        // Determine how many receipts are included in the deposit.
        // -----------------------------------------------------------------
        ReadCashReceipt1();

        // -----------------------------------------------------------------
        // If a Check Type (Program Fund Type) filter is used, calculate
        // the amount and # of items for the specified Check Type that are
        // included in the current Deposit.
        // -----------------------------------------------------------------
        if (!IsEmpty(export.FilterPft.CheckType) && IsEmpty(export.Filter.Code))
        {
          ReadCashReceipt3();
          ReadCashReceipt2();
        }
        else if (!IsEmpty(export.FilterPft.CheckType) && !
          IsEmpty(export.Filter.Code))
        {
          // -----------------------------------------------------------------
          // If Check Type (Program Fund Type) and Cash Receipt Type filters
          // are used, calculate the amount and # of items for the specified
          // Check Type and Cash Receipt Type that are included in the
          // current Deposit.
          // -----------------------------------------------------------------
          ReadCashReceiptCashReceiptType2();
          ReadCashReceiptCashReceiptType1();
        }
        else
        {
          // -----------------------------------------------------------------
          // The Check Type (Program Fund Type)  filter was not entered.
          // Set Program Fund Type totals to 0.
          // -----------------------------------------------------------------
          export.PftTotals.TotalCurrency = 0;
          export.PftTotals.Count = 0;
        }

        // -----------------------------------------------------------------
        // Build list of Cash Receipts included in the deposit.
        // -----------------------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        export.New1.Count = 0;
        export.Page.Count = 0;
        export.Page.Index = -1;
        export.New1.Index = -1;

        foreach(var item in ReadCashReceiptEventCashReceipt3())
        {
          // -----------------------------------------------------------------
          // If a Check Type (Program Fund Type) filter was entered, test
          // to determine if the record should be included in the list.
          // -----------------------------------------------------------------
          if (!IsEmpty(export.FilterPft.CheckType) && !
            Equal(entities.CashReceipt.CheckType, export.FilterPft.CheckType))
          {
            continue;
          }

          if (ReadCashReceiptType2())
          {
            // -----------------------------------------------------------------
            // If a Cash Receipt Type filter was entered, test to determine
            // if the record should be included in the list.
            // -----------------------------------------------------------------
            if (!IsEmpty(export.Filter.Code) && !
              Equal(entities.CashReceiptType.Code, export.Filter.Code))
            {
              continue;
            }
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Cash Receipt is included in the list.  Continue processing.
          // -----------------------------------------------------------------
          if (!ReadCashReceiptSourceType())
          {
            ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Move entity action views to the export group view list.
          // -----------------------------------------------------------------
          ++export.New1.Count;

          ++export.New1.Index;
          export.New1.CheckSize();

          export.New1.Update.NewExportGroupMemberSelect.SelectChar = "";
          MoveCashReceipt(entities.CashReceipt,
            export.New1.Update.NewExportGroupMemberCashReceipt);
          MoveCashReceiptEvent(entities.CashReceiptEvent,
            export.New1.Update.NewExportGroupMemberCashReceiptEvent);
          MoveCashReceiptType(entities.CashReceiptType,
            export.New1.Update.NewExportGroupMemberCashReceiptType);
          MoveCashReceiptSourceType(entities.CashReceiptSourceType,
            export.New1.Update.NewExportGroupMemberCashReceiptSourceType);

          if (export.New1.Index == 0)
          {
            export.Page.Count = 1;

            export.Page.Index = 0;
            export.Page.CheckSize();

            export.Page.Update.PageExportGroupMember.SequentialNumber =
              entities.CashReceipt.SequentialNumber;
          }

          if (export.New1.Index >= 10)
          {
            export.Pf7Pf8MoreIndicator.SelectChar = "+";
            export.Page.Count = 2;

            export.Page.Index = 1;
            export.Page.CheckSize();

            export.Page.Update.PageExportGroupMember.SequentialNumber =
              entities.CashReceipt.SequentialNumber;

            break;
          }
        }

        if (import.HiddenFundTransaction.DepositNumber.GetValueOrDefault() == import
          .SelectedFundTransaction.DepositNumber.GetValueOrDefault())
        {
          // This check is to save I/O in case all of the deposit information 
          // was passed over from the list deposits screen.  That view matching
          // on the dialog flow should populate both the hidden and the screen
          // deposit numbers.  If the two do not match then the user probably
          // typed in a deposit number on this screen in which case we need to
          // read all that other stuff.
        }
        else
        {
          if (ReadFund())
          {
            MoveFund(entities.Fund, export.Fund);
          }
          else
          {
            ExitState = "FN0000_FUND_NF";

            return;
          }

          if (ReadProgramCostAccount())
          {
            MoveProgramCostAccount(entities.ProgramCostAccount,
              export.HiddenProgramCostAccount);
          }
          else
          {
            ExitState = "PROGRAM_COST_ACCOUNT_NF";

            return;
          }
        }

        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        if (export.New1.Count < 1)
        {
          export.Pf7Pf8MoreIndicator.SelectChar = "";
          export.Pf7Pf8PriorIndicator.SelectChar = "";
          ExitState = "FN0000_NO_RECORDS_FOUND";

          return;
        }

        if (!IsEmpty(export.FilterPft.CheckType))
        {
          if (export.PftTotals.Count >= Export.PageGroup.Capacity * 10)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";

            return;
          }
        }
        else if (export.DepositNoOfItems.Count >= Export.PageGroup.Capacity * 10
          )
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";

          return;
        }

        export.Pf7Pf8PriorIndicator.SelectChar = "";
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "LIST":
        // -------------------------------------------------
        // PF4 List Check Type Code Values
        // -------------------------------------------------
        local.NoOfPromptsSelected.Count = 0;

        switch(AsChar(export.ReceiptTypePrompt.PromptField))
        {
          case 'S':
            ++local.NoOfPromptsSelected.Count;
            ExitState = "ECO_LNK_TO_LST_CASH_RECIEPT_TYPE";

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.ReceiptTypePrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.PftPrompt.PromptField))
        {
          case 'S':
            ++local.NoOfPromptsSelected.Count;
            export.Pass.CodeName = "CHECK TYPE";
            ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PftPrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        if (local.NoOfPromptsSelected.Count == 0)
        {
          var field = GetField(export.ReceiptTypePrompt, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (local.NoOfPromptsSelected.Count == 1)
        {
          // --------------------------------------------------------
          // Flow to appropriate list screen.
          // --------------------------------------------------------
        }
        else
        {
          var field3 = GetField(export.ReceiptTypePrompt, "promptField");

          field3.Error = true;

          var field4 = GetField(export.PftPrompt, "promptField");

          field4.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "RETCRTL":
        // -------------------------------------------------
        // Return from PF4 List for Cash Receipt Types
        // -------------------------------------------------
        export.ReceiptTypePrompt.PromptField = "";

        if (!IsEmpty(import.ReturnedCrt.Code))
        {
          export.Filter.Code = import.ReturnedCrt.Code;
        }

        var field1 = GetField(export.FilterPft, "checkType");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "RETCDVL":
        // -------------------------------------------------
        // Return from PF4 List for Check Type Code Values
        // -------------------------------------------------
        export.PftPrompt.PromptField = "";

        if (!IsEmpty(import.ReturnedPft.Cdvalue))
        {
          export.FilterPft.CheckType = import.ReturnedPft.Cdvalue;
        }

        var field2 = GetField(export.SelectedFundTransaction, "depositNumber");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "DETAIL":
        // ------------------------------------------------
        // PF15  CREC
        // ------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        export.New1.Index = 0;
        export.New1.CheckSize();

        while(export.New1.Index < export.New1.Count)
        {
          switch(AsChar(export.New1.Item.NewExportGroupMemberSelect.SelectChar))
          {
            case '*':
              export.New1.Update.NewExportGroupMemberSelect.SelectChar = "";

              break;
            case 'S':
              ++local.Common.Count;

              if (local.Common.Count > 1)
              {
                var field3 =
                  GetField(export.New1.Item.NewExportGroupMemberSelect,
                  "selectChar");

                field3.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                return;
              }

              export.ForFlowCashReceipt.Assign(
                export.New1.Item.NewExportGroupMemberCashReceipt);
              MoveCashReceiptSourceType(export.New1.Item.
                NewExportGroupMemberCashReceiptSourceType,
                export.ForFlowCashReceiptSourceType);
              MoveCashReceiptType(export.New1.Item.
                NewExportGroupMemberCashReceiptType,
                export.ForFlowCashReceiptType);
              MoveCashReceiptEvent(export.New1.Item.
                NewExportGroupMemberCashReceiptEvent,
                export.ForFlowCashReceiptEvent);

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.New1.Item.NewExportGroupMemberSelect,
                "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }

          ++export.New1.Index;
          export.New1.CheckSize();
        }

        if (local.Common.Count == 1)
        {
          ExitState = "ECO_XFR_TO_CASH_RECEIPT";
        }
        else
        {
          // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
          // explicit
          export.New1.Index = 0;
          export.New1.CheckSize();

          var field =
            GetField(export.New1.Item.NewExportGroupMemberSelect, "selectChar");
            

          field.Error = true;

          export.New1.Count = import.New1.Count;
          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        break;
      case "REMOVE":
        // --------------------------------------------------------------------
        // PF16  Remove Receipt
        // This will remove a cash receipt that has been released for
        // deposit from the deposit transaction.
        // --------------------------------------------------------------------
        if (import.SelectedFundTransaction.DepositNumber.GetValueOrDefault() ==
          0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.HiddenFundTransaction.DepositNumber.GetValueOrDefault() != import
          .SelectedFundTransaction.DepositNumber.GetValueOrDefault())
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.SelectedFundTransactionStatus.SystemGeneratedIdentifier != local
          .HardcodedFtsOpen.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_FUND_TRANS_AC";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        // ------------------------------------------------------------
        // Remove each of the receipts selected from the deposit.
        // ------------------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        import.New1.Index = 0;
        import.New1.CheckSize();

        export.New1.Index = 0;
        export.New1.CheckSize();

        while(export.New1.Index < export.New1.Count)
        {
          switch(AsChar(export.New1.Item.NewExportGroupMemberSelect.SelectChar))
          {
            case 'S':
              // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to
              // explicit
              export.ForFlowCashReceipt.Assign(
                export.New1.Item.NewExportGroupMemberCashReceipt);
              UseFnRemoveCashRcptFromDeposit();

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                // ---------------------------------------------
                // Receipt successfully removed from deposit.
                // Continue processing.
                // ---------------------------------------------
              }
              else
              {
                var field3 =
                  GetField(export.New1.Item.NewExportGroupMemberSelect,
                  "selectChar");

                field3.Error = true;

                return;
              }

              break;
            case '*':
              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_RE0000_INVALID_SELECT_CODE";

              var field =
                GetField(export.New1.Item.NewExportGroupMemberSelect,
                "selectChar");

              field.Error = true;

              return;
          }

          ++import.New1.Index;
          import.New1.CheckSize();

          ++export.New1.Index;
          export.New1.CheckSize();
        }

        // ------------------------------------------------------------
        // If all selected receipts were successfully removed from the
        // deposit, place * in the selection char field and set
        // informational message.
        // ------------------------------------------------------------
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
          // explicit
          import.New1.Index = 0;
          import.New1.CheckSize();

          export.New1.Index = 0;
          export.New1.CheckSize();

          while(import.New1.Index < import.New1.Count)
          {
            export.New1.Update.NewExportGroupMemberCashReceipt.Assign(
              import.New1.Item.NewImportGroupMemberCashReceipt);
            MoveCashReceiptEvent(import.New1.Item.
              NewImportGroupMemberCashReceiptEvent,
              export.New1.Update.NewExportGroupMemberCashReceiptEvent);
            MoveCashReceiptSourceType(import.New1.Item.
              NewImportGroupMemberCashReceiptSourceType,
              export.New1.Update.NewExportGroupMemberCashReceiptSourceType);
            MoveCashReceiptType(import.New1.Item.
              NewImportGroupMemberCashReceiptType,
              export.New1.Update.NewExportGroupMemberCashReceiptType);

            if (AsChar(export.New1.Item.NewExportGroupMemberSelect.SelectChar) ==
              'S')
            {
              export.New1.Update.NewExportGroupMemberSelect.SelectChar = "*";
              --export.DepositNoOfItems.Count;
              export.SelectedFundTransaction.Amount -= export.New1.Item.
                NewExportGroupMemberCashReceipt.ReceiptAmount;

              // -------------------------------------------------------------
              // If Program Fund Type (Check Type) filter was used for the
              // display, adjust the program fund type totals on the screen.
              // -------------------------------------------------------------
              if (!IsEmpty(export.FilterPft.CheckType))
              {
                export.PftTotals.TotalCurrency -= entities.CashReceipt.
                  ReceiptAmount;
                --export.PftTotals.Count;
              }
            }
            else
            {
              export.New1.Update.NewExportGroupMemberSelect.SelectChar = "";
            }

            ++import.New1.Index;
            import.New1.CheckSize();

            ++export.New1.Index;
            export.New1.CheckSize();
          }

          ExitState = "FN0135_CASH_RCPT_REMOVED";
        }

        break;
      case "CLOSE":
        // ---------------------------------------------
        // PF17  Close
        // This command will Close an OPEN deposit.
        // ---------------------------------------------
        if (import.SelectedFundTransaction.DepositNumber.GetValueOrDefault() ==
          0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.HiddenFundTransaction.DepositNumber.GetValueOrDefault() != import
          .SelectedFundTransaction.DepositNumber.GetValueOrDefault())
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.SelectedFundTransactionStatus.SystemGeneratedIdentifier != local
          .HardcodedFtsOpen.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_FUND_TRANS_AC";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        UseCloseDeposit();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SelectedClosed.CreatedBy =
            local.FundTransactionStatusHistory.CreatedBy;
          export.SelectedFundTransactionStatus.SystemGeneratedIdentifier =
            local.HardcodedFtsClosed.SystemGeneratedIdentifier;
          export.SelectedFundTransactionStatus.Code = "CLOSED";
          export.ClosedDate.Date =
            Date(local.FundTransactionStatusHistory.CreatedTimestamp);
          ExitState = "FN0000_DEPOSIT_SUCCESSFLY_CLOSED";
        }
        else
        {
          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;
        }

        break;
      case "REOPEN":
        // ------------------------------------------------
        // PF18  ReOpen Deposit
        // ------------------------------------------------
        if (import.HiddenFundTransaction.DepositNumber.GetValueOrDefault() != import
          .SelectedFundTransaction.DepositNumber.GetValueOrDefault())
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.SelectedFundTransaction.DepositNumber.GetValueOrDefault() ==
          0)
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        if (import.SelectedFundTransactionStatus.SystemGeneratedIdentifier != local
          .HardcodedFtsClosed.SystemGeneratedIdentifier)
        {
          ExitState = "FN0000_DEPOSIT_ALREADY_OPEN";

          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;

          return;
        }

        UseFnReopenDeposit();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.SelectedFundTransactionStatus.Code = "OPEN";
          export.SelectedFundTransactionStatus.SystemGeneratedIdentifier =
            local.HardcodedFtsOpen.SystemGeneratedIdentifier;
          export.ClosedDate.Date = local.Null1.Date;
          export.SelectedClosed.CreatedBy = "";
          ExitState = "FN0000_DEPOSIT_REOPEN_SUCCESSFUL";
        }
        else
        {
          var field = GetField(export.SelectedFundTransaction, "depositNumber");

          field.Error = true;
        }

        break;
      case "CRTB":
        // ------------------------------------------------
        // PF19  CRTB
        // ------------------------------------------------
        ExitState = "ECO_XFR_TO_CRTB";

        break;
      case "EXIT":
        // ------------------------------------------------
        // PF3  Exit
        // ------------------------------------------------
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        // ------------------------------------------------
        // PF7  Prev
        // ------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        if (export.Page.Count <= 1)
        {
          export.Pf7Pf8PriorIndicator.SelectChar = "";
          ExitState = "ACO_NI0000_TOP_OF_LIST";

          return;
        }

        export.Pf7Pf8MoreIndicator.SelectChar = "";
        local.StartOfGroup.Flag = "N";

        if (export.Page.Count <= 2)
        {
          local.StartOfGroup.Flag = "Y";
          export.Page.Count = 1;
        }
        else if (Export.NewGroup.Capacity > export.New1.Count)
        {
          --export.Page.Count;
        }
        else
        {
          export.Page.Count -= 2;

          if (export.Page.Count <= 1)
          {
            local.StartOfGroup.Flag = "Y";
          }
        }

        export.Page.Index = export.Page.Count - 1;
        export.Page.CheckSize();

        local.Pf78Start.SequentialNumber =
          export.Page.Item.PageExportGroupMember.SequentialNumber;
        export.New1.Count = 0;
        export.New1.Index = -1;

        // -----------------------------------------------------------------
        // Build list of Cash Receipts included in the deposit.
        // -----------------------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        foreach(var item in ReadCashReceiptEventCashReceipt2())
        {
          // -----------------------------------------------------------------
          // If a Check Type (Program Fund Type) filter was entered, test
          // to determine if the record should be included in the list.
          // -----------------------------------------------------------------
          if (!IsEmpty(export.FilterPft.CheckType) && !
            Equal(entities.CashReceipt.CheckType, export.FilterPft.CheckType))
          {
            continue;
          }

          if (ReadCashReceiptType2())
          {
            // -----------------------------------------------------------------
            // If a Cash Receipt Type filter was entered, test to determine
            // if the record should be included in the list.
            // -----------------------------------------------------------------
            if (!IsEmpty(export.Filter.Code) && !
              Equal(entities.CashReceiptType.Code, export.Filter.Code))
            {
              continue;
            }
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Cash Receipt is included in the list.  Continue processing.
          // -----------------------------------------------------------------
          if (!ReadCashReceiptSourceType())
          {
            ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Move entity action views to the export group view list.
          // -----------------------------------------------------------------
          ++export.New1.Index;
          export.New1.CheckSize();

          export.New1.Update.NewExportGroupMemberSelect.SelectChar = "";
          MoveCashReceipt(entities.CashReceipt,
            export.New1.Update.NewExportGroupMemberCashReceipt);
          MoveCashReceiptEvent(entities.CashReceiptEvent,
            export.New1.Update.NewExportGroupMemberCashReceiptEvent);
          MoveCashReceiptType(entities.CashReceiptType,
            export.New1.Update.NewExportGroupMemberCashReceiptType);
          MoveCashReceiptSourceType(entities.CashReceiptSourceType,
            export.New1.Update.NewExportGroupMemberCashReceiptSourceType);

          if (export.New1.Index >= 10)
          {
            export.Pf7Pf8MoreIndicator.SelectChar = "+";

            ++export.Page.Index;
            export.Page.CheckSize();

            export.Page.Update.PageExportGroupMember.SequentialNumber =
              entities.CashReceipt.SequentialNumber;

            break;
          }
        }

        export.Pf7Pf8PriorIndicator.SelectChar = "-";

        if (AsChar(local.StartOfGroup.Flag) == 'Y')
        {
          export.Pf7Pf8PriorIndicator.SelectChar = "";
          ExitState = "ACO_NI0000_TOP_OF_LIST";
        }

        break;
      case "NEXT":
        // ------------------------------------------------
        // PF8  Next
        // ------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        export.Pf7Pf8MoreIndicator.SelectChar = "";

        if (import.New1.Count < 11)
        {
          if (import.Page.Count == 1)
          {
            export.Pf7Pf8PriorIndicator.SelectChar = "";
          }

          export.Pf7Pf8MoreIndicator.SelectChar = "";
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

          return;
        }

        local.EndOfPageGroup.Flag = "N";

        if (export.Page.Count == Export.PageGroup.Capacity - 1)
        {
          local.EndOfPageGroup.Flag = "Y";
        }

        if (export.Page.Count == Export.PageGroup.Capacity)
        {
          export.Pf7Pf8PriorIndicator.SelectChar = "";

          export.New1.Index = export.New1.Count - 1;
          export.New1.CheckSize();

          local.Pf78Start.SequentialNumber =
            export.New1.Item.NewExportGroupMemberCashReceipt.SequentialNumber;
          export.Page.Count = 1;

          export.Page.Index = 0;
          export.Page.CheckSize();

          export.Page.Update.PageExportGroupMember.SequentialNumber =
            export.New1.Item.NewExportGroupMemberCashReceipt.SequentialNumber;
        }
        else
        {
          export.Pf7Pf8PriorIndicator.SelectChar = "-";

          export.Page.Index = export.Page.Count - 1;
          export.Page.CheckSize();

          local.Pf78Start.SequentialNumber =
            export.Page.Item.PageExportGroupMember.SequentialNumber;
        }

        export.New1.Count = 0;
        export.New1.Index = -1;

        // -----------------------------------------------------------------
        // Build list of Cash Receipts included in the deposit.
        // -----------------------------------------------------------------
        // 01/07/00  P. Phinney  H00082492 Changed Table  from implicit to 
        // explicit
        foreach(var item in ReadCashReceiptEventCashReceipt1())
        {
          // -----------------------------------------------------------------
          // If a Check Type (Program Fund Type) filter was entered, test
          // to determine if the record should be included in the list.
          // -----------------------------------------------------------------
          if (!IsEmpty(export.FilterPft.CheckType) && !
            Equal(entities.CashReceipt.CheckType, export.FilterPft.CheckType))
          {
            continue;
          }

          if (ReadCashReceiptType2())
          {
            // -----------------------------------------------------------------
            // If a Cash Receipt Type filter was entered, test to determine
            // if the record should be included in the list.
            // -----------------------------------------------------------------
            if (!IsEmpty(export.Filter.Code) && !
              Equal(entities.CashReceiptType.Code, export.Filter.Code))
            {
              continue;
            }
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Cash Receipt is included in the list.  Continue processing.
          // -----------------------------------------------------------------
          if (!ReadCashReceiptSourceType())
          {
            ExitState = "FN0000_CASH_RCPT_SOURCE_TYPE_NF";

            return;
          }

          // -----------------------------------------------------------------
          // Move entity action views to the export group view list.
          // -----------------------------------------------------------------
          ++export.New1.Count;

          ++export.New1.Index;
          export.New1.CheckSize();

          export.New1.Update.NewExportGroupMemberSelect.SelectChar = "";
          MoveCashReceipt(entities.CashReceipt,
            export.New1.Update.NewExportGroupMemberCashReceipt);
          MoveCashReceiptEvent(entities.CashReceiptEvent,
            export.New1.Update.NewExportGroupMemberCashReceiptEvent);
          MoveCashReceiptType(entities.CashReceiptType,
            export.New1.Update.NewExportGroupMemberCashReceiptType);
          MoveCashReceiptSourceType(entities.CashReceiptSourceType,
            export.New1.Update.NewExportGroupMemberCashReceiptSourceType);

          if (export.New1.Index >= 10)
          {
            export.Pf7Pf8MoreIndicator.SelectChar = "+";
            ++export.Page.Count;

            ++export.Page.Index;
            export.Page.CheckSize();

            export.Page.Update.PageExportGroupMember.SequentialNumber =
              entities.CashReceipt.SequentialNumber;

            break;
          }
        }

        if (AsChar(local.EndOfPageGroup.Flag) == 'Y')
        {
          if (export.New1.Count == 11)
          {
            ExitState = "FN0000_CRDI_END_OF_PAGE_GROUP";
          }
          else
          {
            export.Pf7Pf8MoreIndicator.SelectChar = "";
            ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
          }

          return;
        }

        if (export.New1.Count < 11)
        {
          ExitState = "ACO_NI0000_BOTTOM_OF_LIST";
        }

        break;
      case "RETURN":
        // ---------------------------------------------
        // PF9  Return
        // Exit CRDI and Return to previous screen.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // ------------------------------------------------
        // PF12  Signoff
        // ------------------------------------------------
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
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveFund(Fund source, Fund target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Number = source.Number;
  }

  private static void MoveFundTransactionStatus1(FundTransactionStatus source,
    FundTransactionStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveFundTransactionStatus2(FundTransactionStatus source,
    FundTransactionStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
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

  private static void MoveProgramCostAccount(ProgramCostAccount source,
    ProgramCostAccount target)
  {
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Pass.CodeName;
    useImport.CodeValue.Cdvalue = local.SelectedPft.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Count = useExport.ReturnCode.Count;
  }

  private void UseCloseDeposit()
  {
    var useImport = new CloseDeposit.Import();
    var useExport = new CloseDeposit.Export();

    useImport.FundTransaction.SystemGeneratedIdentifier =
      import.SelectedFundTransaction.SystemGeneratedIdentifier;
    MoveFundTransactionStatus1(import.SelectedFundTransactionStatus,
      useImport.FundTransactionStatus);
    useImport.Fund.SystemGeneratedIdentifier =
      import.Fund.SystemGeneratedIdentifier;
    MoveProgramCostAccount(import.HiddenProgramCostAccount,
      useImport.ProgramCostAccount);
    useImport.FundTransactionStatusHistory.ReasonText =
      local.FundTransactionStatusHistory.ReasonText;
    useImport.FundTransactionType.SystemGeneratedIdentifier =
      local.HardcodedFttDeposit.SystemGeneratedIdentifier;

    Call(CloseDeposit.Execute, useImport, useExport);

    local.FundTransactionStatusHistory.Assign(
      useExport.FundTransactionStatusHistory);
  }

  private void UseFnRemoveCashRcptFromDeposit()
  {
    var useImport = new FnRemoveCashRcptFromDeposit.Import();
    var useExport = new FnRemoveCashRcptFromDeposit.Export();

    useImport.CashReceipt.SequentialNumber =
      export.ForFlowCashReceipt.SequentialNumber;

    Call(FnRemoveCashRcptFromDeposit.Execute, useImport, useExport);
  }

  private void UseFnReopenDeposit()
  {
    var useImport = new FnReopenDeposit.Import();
    var useExport = new FnReopenDeposit.Export();

    useImport.FundTransaction.SystemGeneratedIdentifier =
      import.SelectedFundTransaction.SystemGeneratedIdentifier;
    MoveFundTransactionStatus2(import.SelectedFundTransactionStatus,
      useImport.FundTransactionStatus);
    useImport.Fund.SystemGeneratedIdentifier =
      import.Fund.SystemGeneratedIdentifier;
    MoveProgramCostAccount(import.HiddenProgramCostAccount,
      useImport.ProgramCostAccount);
    useImport.FundTransactionStatusHistory.ReasonText =
      local.FundTransactionStatusHistory.ReasonText;
    useImport.FundTransactionType.SystemGeneratedIdentifier =
      local.HardcodedFttDeposit.SystemGeneratedIdentifier;

    Call(FnReopenDeposit.Execute, useImport, useExport);

    local.FundTransactionStatusHistory.Assign(
      useExport.FundTransactionStatusHistory);
  }

  private void UseHardcodedFundingInformation()
  {
    var useImport = new HardcodedFundingInformation.Import();
    var useExport = new HardcodedFundingInformation.Export();

    Call(HardcodedFundingInformation.Execute, useImport, useExport);

    local.HardcodedFtsOpen.SystemGeneratedIdentifier =
      useExport.Open.SystemGeneratedIdentifier;
    local.HardcodedFtsClosed.SystemGeneratedIdentifier =
      useExport.Closed.SystemGeneratedIdentifier;
    local.HardcodedFttDeposit.SystemGeneratedIdentifier =
      useExport.Deposit.SystemGeneratedIdentifier;
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

  private bool ReadCashReceipt1()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        export.DepositNoOfItems.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceipt2()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetNullableString(
          command, "checkType", export.FilterPft.CheckType ?? "");
      },
      (db, reader) =>
      {
        export.PftTotals.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceipt3()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceipt3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetNullableString(
          command, "checkType", export.FilterPft.CheckType ?? "");
      },
      (db, reader) =>
      {
        export.PftTotals.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private bool ReadCashReceiptCashReceiptType1()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceiptCashReceiptType1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetNullableString(
          command, "checkType", export.FilterPft.CheckType ?? "");
        db.SetString(command, "code", export.Filter.Code);
      },
      (db, reader) =>
      {
        export.PftTotals.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptCashReceiptType2()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);

    return Read("ReadCashReceiptCashReceiptType2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetNullableString(
          command, "checkType", export.FilterPft.CheckType ?? "");
        db.SetString(command, "code", export.Filter.Code);
      },
      (db, reader) =>
      {
        export.PftTotals.TotalCurrency = db.GetDecimal(reader, 0);
      });
  }

  private IEnumerable<bool> ReadCashReceiptEventCashReceipt1()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptEventCashReceipt1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          export.SelectedFundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(command, "cashReceiptId", local.Pf78Start.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 4);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 5);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 6);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 7);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 8);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 9);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 10);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptEventCashReceipt2()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptEventCashReceipt2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          export.SelectedFundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(command, "cashReceiptId", local.Pf78Start.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 4);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 5);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 6);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 7);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 8);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 9);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 10);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptEventCashReceipt3()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return ReadEach("ReadCashReceiptEventCashReceipt3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetNullableInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetNullableDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetNullableString(
          command, "pcaCode", entities.FundTransaction.PcaCode);
        db.SetInt32(
          command, "cashReceiptId", import.FilterStarting.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 4);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 5);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 6);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 7);
        entities.CashReceipt.FttIdentifier = db.GetNullableInt32(reader, 8);
        entities.CashReceipt.PcaCode = db.GetNullableString(reader, 9);
        entities.CashReceipt.PcaEffectiveDate = db.GetNullableDate(reader, 10);
        entities.CashReceipt.FunIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceipt.FtrIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;

        return true;
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

  private bool ReadCashReceiptType1()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetString(command, "code", export.Filter.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCashReceiptType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadFund()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.Fund.Populated = false;

    return Read("ReadFund",
      (db, command) =>
      {
        db.SetInt32(command, "fundId", entities.FundTransaction.FunIdentifier);
      },
      (db, reader) =>
      {
        entities.Fund.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Fund.Number = db.GetString(reader, 1);
        entities.Fund.BalanceAmount = db.GetDecimal(reader, 2);
        entities.Fund.Populated = true;
      });
  }

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "depositNumber",
          import.SelectedFundTransaction.DepositNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FundTransaction.FttIdentifier = db.GetInt32(reader, 0);
        entities.FundTransaction.PcaCode = db.GetString(reader, 1);
        entities.FundTransaction.PcaEffectiveDate = db.GetDate(reader, 2);
        entities.FundTransaction.FunIdentifier = db.GetInt32(reader, 3);
        entities.FundTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransaction.DepositNumber = db.GetNullableInt32(reader, 5);
        entities.FundTransaction.Amount = db.GetDecimal(reader, 6);
        entities.FundTransaction.BusinessDate = db.GetDate(reader, 7);
        entities.FundTransaction.CreatedBy = db.GetString(reader, 8);
        entities.FundTransaction.Populated = true;
      });
  }

  private bool ReadFundTransactionStatusFundTransactionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.FundTransactionStatusHistory.Populated = false;
    entities.FundTransactionStatus.Populated = false;

    return Read("ReadFundTransactionStatusFundTransactionStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "ftrIdentifier",
          entities.FundTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "funIdentifier", entities.FundTransaction.FunIdentifier);
        db.SetInt32(
          command, "fttIdentifier", entities.FundTransaction.FttIdentifier);
        db.SetDate(
          command, "pcaEffectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "pcaCode", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.FundTransactionStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatusHistory.FtsIdentifier =
          db.GetInt32(reader, 0);
        entities.FundTransactionStatus.Code = db.GetString(reader, 1);
        entities.FundTransactionStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.FundTransactionStatus.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.FundTransactionStatusHistory.FtrIdentifier =
          db.GetInt32(reader, 4);
        entities.FundTransactionStatusHistory.FunIdentifier =
          db.GetInt32(reader, 5);
        entities.FundTransactionStatusHistory.PcaEffectiveDate =
          db.GetDate(reader, 6);
        entities.FundTransactionStatusHistory.PcaCode = db.GetString(reader, 7);
        entities.FundTransactionStatusHistory.FttIdentifier =
          db.GetInt32(reader, 8);
        entities.FundTransactionStatusHistory.EffectiveTmst =
          db.GetDateTime(reader, 9);
        entities.FundTransactionStatusHistory.CreatedBy =
          db.GetString(reader, 10);
        entities.FundTransactionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.FundTransactionStatusHistory.ReasonText =
          db.GetNullableString(reader, 12);
        entities.FundTransactionStatusHistory.Populated = true;
        entities.FundTransactionStatus.Populated = true;
      });
  }

  private bool ReadProgramCostAccount()
  {
    System.Diagnostics.Debug.Assert(entities.FundTransaction.Populated);
    entities.ProgramCostAccount.Populated = false;

    return Read("ReadProgramCostAccount",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.FundTransaction.PcaEffectiveDate.GetValueOrDefault());
        db.SetString(command, "code", entities.FundTransaction.PcaCode);
      },
      (db, reader) =>
      {
        entities.ProgramCostAccount.Code = db.GetString(reader, 0);
        entities.ProgramCostAccount.EffectiveDate = db.GetDate(reader, 1);
        entities.ProgramCostAccount.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramCostAccount.Populated = true;
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
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of PageImportGroupMember.
      /// </summary>
      [JsonPropertyName("pageImportGroupMember")]
      public CashReceipt PageImportGroupMember
      {
        get => pageImportGroupMember ??= new();
        set => pageImportGroupMember = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CashReceipt pageImportGroupMember;
    }

    /// <summary>A NewGroup group.</summary>
    [Serializable]
    public class NewGroup
    {
      /// <summary>
      /// A value of NewImportGroupMemberSelect.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberSelect")]
      public Common NewImportGroupMemberSelect
      {
        get => newImportGroupMemberSelect ??= new();
        set => newImportGroupMemberSelect = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberCashReceipt.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberCashReceipt")]
      public CashReceipt NewImportGroupMemberCashReceipt
      {
        get => newImportGroupMemberCashReceipt ??= new();
        set => newImportGroupMemberCashReceipt = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberCashReceiptSourceType")]
      public CashReceiptSourceType NewImportGroupMemberCashReceiptSourceType
      {
        get => newImportGroupMemberCashReceiptSourceType ??= new();
        set => newImportGroupMemberCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberCashReceiptType.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberCashReceiptType")]
      public CashReceiptType NewImportGroupMemberCashReceiptType
      {
        get => newImportGroupMemberCashReceiptType ??= new();
        set => newImportGroupMemberCashReceiptType = value;
      }

      /// <summary>
      /// A value of NewImportGroupMemberCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("newImportGroupMemberCashReceiptEvent")]
      public CashReceiptEvent NewImportGroupMemberCashReceiptEvent
      {
        get => newImportGroupMemberCashReceiptEvent ??= new();
        set => newImportGroupMemberCashReceiptEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common newImportGroupMemberSelect;
      private CashReceipt newImportGroupMemberCashReceipt;
      private CashReceiptSourceType newImportGroupMemberCashReceiptSourceType;
      private CashReceiptType newImportGroupMemberCashReceiptType;
      private CashReceiptEvent newImportGroupMemberCashReceiptEvent;
    }

    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdImportGroupMemberSelection.
      /// </summary>
      [JsonPropertyName("zdImportGroupMemberSelection")]
      public Common ZdImportGroupMemberSelection
      {
        get => zdImportGroupMemberSelection ??= new();
        set => zdImportGroupMemberSelection = value;
      }

      /// <summary>
      /// A value of ZdelImportGroupMemberCashReceipt.
      /// </summary>
      [JsonPropertyName("zdelImportGroupMemberCashReceipt")]
      public CashReceipt ZdelImportGroupMemberCashReceipt
      {
        get => zdelImportGroupMemberCashReceipt ??= new();
        set => zdelImportGroupMemberCashReceipt = value;
      }

      /// <summary>
      /// A value of ZdelImportGroupMemberCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("zdelImportGroupMemberCashReceiptSourceType")]
      public CashReceiptSourceType ZdelImportGroupMemberCashReceiptSourceType
      {
        get => zdelImportGroupMemberCashReceiptSourceType ??= new();
        set => zdelImportGroupMemberCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of ZdelImportGroupMemberCashReceiptType.
      /// </summary>
      [JsonPropertyName("zdelImportGroupMemberCashReceiptType")]
      public CashReceiptType ZdelImportGroupMemberCashReceiptType
      {
        get => zdelImportGroupMemberCashReceiptType ??= new();
        set => zdelImportGroupMemberCashReceiptType = value;
      }

      /// <summary>
      /// A value of ZdelImportGroupMemberCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("zdelImportGroupMemberCashReceiptEvent")]
      public CashReceiptEvent ZdelImportGroupMemberCashReceiptEvent
      {
        get => zdelImportGroupMemberCashReceiptEvent ??= new();
        set => zdelImportGroupMemberCashReceiptEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common zdImportGroupMemberSelection;
      private CashReceipt zdelImportGroupMemberCashReceipt;
      private CashReceiptSourceType zdelImportGroupMemberCashReceiptSourceType;
      private CashReceiptType zdelImportGroupMemberCashReceiptType;
      private CashReceiptEvent zdelImportGroupMemberCashReceiptEvent;
    }

    /// <summary>
    /// A value of Pf7Pf8PriorIndicator.
    /// </summary>
    [JsonPropertyName("pf7Pf8PriorIndicator")]
    public Common Pf7Pf8PriorIndicator
    {
      get => pf7Pf8PriorIndicator ??= new();
      set => pf7Pf8PriorIndicator = value;
    }

    /// <summary>
    /// A value of Pf7Pf8MoreIndicator.
    /// </summary>
    [JsonPropertyName("pf7Pf8MoreIndicator")]
    public Common Pf7Pf8MoreIndicator
    {
      get => pf7Pf8MoreIndicator ??= new();
      set => pf7Pf8MoreIndicator = value;
    }

    /// <summary>
    /// A value of SelectedFundTransaction.
    /// </summary>
    [JsonPropertyName("selectedFundTransaction")]
    public FundTransaction SelectedFundTransaction
    {
      get => selectedFundTransaction ??= new();
      set => selectedFundTransaction = value;
    }

    /// <summary>
    /// A value of SelectedFundTransactionStatus.
    /// </summary>
    [JsonPropertyName("selectedFundTransactionStatus")]
    public FundTransactionStatus SelectedFundTransactionStatus
    {
      get => selectedFundTransactionStatus ??= new();
      set => selectedFundTransactionStatus = value;
    }

    /// <summary>
    /// A value of SelectedClosed.
    /// </summary>
    [JsonPropertyName("selectedClosed")]
    public FundTransactionStatusHistory SelectedClosed
    {
      get => selectedClosed ??= new();
      set => selectedClosed = value;
    }

    /// <summary>
    /// A value of ClosedDate.
    /// </summary>
    [JsonPropertyName("closedDate")]
    public DateWorkArea ClosedDate
    {
      get => closedDate ??= new();
      set => closedDate = value;
    }

    /// <summary>
    /// A value of DepositNoOfItems.
    /// </summary>
    [JsonPropertyName("depositNoOfItems")]
    public Common DepositNoOfItems
    {
      get => depositNoOfItems ??= new();
      set => depositNoOfItems = value;
    }

    /// <summary>
    /// A value of PftTotals.
    /// </summary>
    [JsonPropertyName("pftTotals")]
    public Common PftTotals
    {
      get => pftTotals ??= new();
      set => pftTotals = value;
    }

    /// <summary>
    /// A value of FilterStarting.
    /// </summary>
    [JsonPropertyName("filterStarting")]
    public CashReceipt FilterStarting
    {
      get => filterStarting ??= new();
      set => filterStarting = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public CashReceiptType Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of ReceiptTypePrompt.
    /// </summary>
    [JsonPropertyName("receiptTypePrompt")]
    public Standard ReceiptTypePrompt
    {
      get => receiptTypePrompt ??= new();
      set => receiptTypePrompt = value;
    }

    /// <summary>
    /// A value of FilterPft.
    /// </summary>
    [JsonPropertyName("filterPft")]
    public CashReceipt FilterPft
    {
      get => filterPft ??= new();
      set => filterPft = value;
    }

    /// <summary>
    /// A value of PftPrompt.
    /// </summary>
    [JsonPropertyName("pftPrompt")]
    public Standard PftPrompt
    {
      get => pftPrompt ??= new();
      set => pftPrompt = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
    }

    /// <summary>
    /// Gets a value of New1.
    /// </summary>
    [JsonIgnore]
    public Array<NewGroup> New1 => new1 ??= new(NewGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of New1 for json serialization.
    /// </summary>
    [JsonPropertyName("new1")]
    [Computed]
    public IList<NewGroup> New1_Json
    {
      get => new1;
      set => New1.Assign(value);
    }

    /// <summary>
    /// A value of HiddenFundTransaction.
    /// </summary>
    [JsonPropertyName("hiddenFundTransaction")]
    public FundTransaction HiddenFundTransaction
    {
      get => hiddenFundTransaction ??= new();
      set => hiddenFundTransaction = value;
    }

    /// <summary>
    /// A value of HiddenProgramCostAccount.
    /// </summary>
    [JsonPropertyName("hiddenProgramCostAccount")]
    public ProgramCostAccount HiddenProgramCostAccount
    {
      get => hiddenProgramCostAccount ??= new();
      set => hiddenProgramCostAccount = value;
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
    /// A value of ReturnedCrt.
    /// </summary>
    [JsonPropertyName("returnedCrt")]
    public CashReceiptType ReturnedCrt
    {
      get => returnedCrt ??= new();
      set => returnedCrt = value;
    }

    /// <summary>
    /// A value of ReturnedPft.
    /// </summary>
    [JsonPropertyName("returnedPft")]
    public CodeValue ReturnedPft
    {
      get => returnedPft ??= new();
      set => returnedPft = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
    }

    private Common pf7Pf8PriorIndicator;
    private Common pf7Pf8MoreIndicator;
    private FundTransaction selectedFundTransaction;
    private FundTransactionStatus selectedFundTransactionStatus;
    private FundTransactionStatusHistory selectedClosed;
    private DateWorkArea closedDate;
    private Common depositNoOfItems;
    private Common pftTotals;
    private CashReceipt filterStarting;
    private CashReceiptType filter;
    private Standard receiptTypePrompt;
    private CashReceipt filterPft;
    private Standard pftPrompt;
    private Fund fund;
    private Array<PageGroup> page;
    private Array<NewGroup> new1;
    private FundTransaction hiddenFundTransaction;
    private ProgramCostAccount hiddenProgramCostAccount;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptType returnedCrt;
    private CodeValue returnedPft;
    private Array<ZdelGroup> zdel;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PageGroup group.</summary>
    [Serializable]
    public class PageGroup
    {
      /// <summary>
      /// A value of PageExportGroupMember.
      /// </summary>
      [JsonPropertyName("pageExportGroupMember")]
      public CashReceipt PageExportGroupMember
      {
        get => pageExportGroupMember ??= new();
        set => pageExportGroupMember = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private CashReceipt pageExportGroupMember;
    }

    /// <summary>A NewGroup group.</summary>
    [Serializable]
    public class NewGroup
    {
      /// <summary>
      /// A value of NewExportGroupMemberSelect.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberSelect")]
      public Common NewExportGroupMemberSelect
      {
        get => newExportGroupMemberSelect ??= new();
        set => newExportGroupMemberSelect = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberCashReceipt.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberCashReceipt")]
      public CashReceipt NewExportGroupMemberCashReceipt
      {
        get => newExportGroupMemberCashReceipt ??= new();
        set => newExportGroupMemberCashReceipt = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberCashReceiptSourceType")]
      public CashReceiptSourceType NewExportGroupMemberCashReceiptSourceType
      {
        get => newExportGroupMemberCashReceiptSourceType ??= new();
        set => newExportGroupMemberCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberCashReceiptType.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberCashReceiptType")]
      public CashReceiptType NewExportGroupMemberCashReceiptType
      {
        get => newExportGroupMemberCashReceiptType ??= new();
        set => newExportGroupMemberCashReceiptType = value;
      }

      /// <summary>
      /// A value of NewExportGroupMemberCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("newExportGroupMemberCashReceiptEvent")]
      public CashReceiptEvent NewExportGroupMemberCashReceiptEvent
      {
        get => newExportGroupMemberCashReceiptEvent ??= new();
        set => newExportGroupMemberCashReceiptEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 11;

      private Common newExportGroupMemberSelect;
      private CashReceipt newExportGroupMemberCashReceipt;
      private CashReceiptSourceType newExportGroupMemberCashReceiptSourceType;
      private CashReceiptType newExportGroupMemberCashReceiptType;
      private CashReceiptEvent newExportGroupMemberCashReceiptEvent;
    }

    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdExportGroupMemberSelection.
      /// </summary>
      [JsonPropertyName("zdExportGroupMemberSelection")]
      public Common ZdExportGroupMemberSelection
      {
        get => zdExportGroupMemberSelection ??= new();
        set => zdExportGroupMemberSelection = value;
      }

      /// <summary>
      /// A value of ZdelExportGroupMemberCashReceipt.
      /// </summary>
      [JsonPropertyName("zdelExportGroupMemberCashReceipt")]
      public CashReceipt ZdelExportGroupMemberCashReceipt
      {
        get => zdelExportGroupMemberCashReceipt ??= new();
        set => zdelExportGroupMemberCashReceipt = value;
      }

      /// <summary>
      /// A value of ZdelExportGroupMemberCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("zdelExportGroupMemberCashReceiptSourceType")]
      public CashReceiptSourceType ZdelExportGroupMemberCashReceiptSourceType
      {
        get => zdelExportGroupMemberCashReceiptSourceType ??= new();
        set => zdelExportGroupMemberCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of ZdelExportGroupMemberCashReceiptType.
      /// </summary>
      [JsonPropertyName("zdelExportGroupMemberCashReceiptType")]
      public CashReceiptType ZdelExportGroupMemberCashReceiptType
      {
        get => zdelExportGroupMemberCashReceiptType ??= new();
        set => zdelExportGroupMemberCashReceiptType = value;
      }

      /// <summary>
      /// A value of ZdelExportGroupMemberCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("zdelExportGroupMemberCashReceiptEvent")]
      public CashReceiptEvent ZdelExportGroupMemberCashReceiptEvent
      {
        get => zdelExportGroupMemberCashReceiptEvent ??= new();
        set => zdelExportGroupMemberCashReceiptEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common zdExportGroupMemberSelection;
      private CashReceipt zdelExportGroupMemberCashReceipt;
      private CashReceiptSourceType zdelExportGroupMemberCashReceiptSourceType;
      private CashReceiptType zdelExportGroupMemberCashReceiptType;
      private CashReceiptEvent zdelExportGroupMemberCashReceiptEvent;
    }

    /// <summary>
    /// A value of Pf7Pf8PriorIndicator.
    /// </summary>
    [JsonPropertyName("pf7Pf8PriorIndicator")]
    public Common Pf7Pf8PriorIndicator
    {
      get => pf7Pf8PriorIndicator ??= new();
      set => pf7Pf8PriorIndicator = value;
    }

    /// <summary>
    /// A value of Pf7Pf8MoreIndicator.
    /// </summary>
    [JsonPropertyName("pf7Pf8MoreIndicator")]
    public Common Pf7Pf8MoreIndicator
    {
      get => pf7Pf8MoreIndicator ??= new();
      set => pf7Pf8MoreIndicator = value;
    }

    /// <summary>
    /// A value of SelectedFundTransaction.
    /// </summary>
    [JsonPropertyName("selectedFundTransaction")]
    public FundTransaction SelectedFundTransaction
    {
      get => selectedFundTransaction ??= new();
      set => selectedFundTransaction = value;
    }

    /// <summary>
    /// A value of SelectedFundTransactionStatus.
    /// </summary>
    [JsonPropertyName("selectedFundTransactionStatus")]
    public FundTransactionStatus SelectedFundTransactionStatus
    {
      get => selectedFundTransactionStatus ??= new();
      set => selectedFundTransactionStatus = value;
    }

    /// <summary>
    /// A value of SelectedClosed.
    /// </summary>
    [JsonPropertyName("selectedClosed")]
    public FundTransactionStatusHistory SelectedClosed
    {
      get => selectedClosed ??= new();
      set => selectedClosed = value;
    }

    /// <summary>
    /// A value of ClosedDate.
    /// </summary>
    [JsonPropertyName("closedDate")]
    public DateWorkArea ClosedDate
    {
      get => closedDate ??= new();
      set => closedDate = value;
    }

    /// <summary>
    /// A value of DepositNoOfItems.
    /// </summary>
    [JsonPropertyName("depositNoOfItems")]
    public Common DepositNoOfItems
    {
      get => depositNoOfItems ??= new();
      set => depositNoOfItems = value;
    }

    /// <summary>
    /// A value of PftTotals.
    /// </summary>
    [JsonPropertyName("pftTotals")]
    public Common PftTotals
    {
      get => pftTotals ??= new();
      set => pftTotals = value;
    }

    /// <summary>
    /// A value of FilterStarting.
    /// </summary>
    [JsonPropertyName("filterStarting")]
    public CashReceipt FilterStarting
    {
      get => filterStarting ??= new();
      set => filterStarting = value;
    }

    /// <summary>
    /// A value of FilterPft.
    /// </summary>
    [JsonPropertyName("filterPft")]
    public CashReceipt FilterPft
    {
      get => filterPft ??= new();
      set => filterPft = value;
    }

    /// <summary>
    /// A value of ReceiptTypePrompt.
    /// </summary>
    [JsonPropertyName("receiptTypePrompt")]
    public Standard ReceiptTypePrompt
    {
      get => receiptTypePrompt ??= new();
      set => receiptTypePrompt = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public CashReceiptType Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    /// <summary>
    /// A value of PftPrompt.
    /// </summary>
    [JsonPropertyName("pftPrompt")]
    public Standard PftPrompt
    {
      get => pftPrompt ??= new();
      set => pftPrompt = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// Gets a value of Page.
    /// </summary>
    [JsonIgnore]
    public Array<PageGroup> Page => page ??= new(PageGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Page for json serialization.
    /// </summary>
    [JsonPropertyName("page")]
    [Computed]
    public IList<PageGroup> Page_Json
    {
      get => page;
      set => Page.Assign(value);
    }

    /// <summary>
    /// Gets a value of New1.
    /// </summary>
    [JsonIgnore]
    public Array<NewGroup> New1 => new1 ??= new(NewGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of New1 for json serialization.
    /// </summary>
    [JsonPropertyName("new1")]
    [Computed]
    public IList<NewGroup> New1_Json
    {
      get => new1;
      set => New1.Assign(value);
    }

    /// <summary>
    /// A value of HiddenFundTransaction.
    /// </summary>
    [JsonPropertyName("hiddenFundTransaction")]
    public FundTransaction HiddenFundTransaction
    {
      get => hiddenFundTransaction ??= new();
      set => hiddenFundTransaction = value;
    }

    /// <summary>
    /// A value of HiddenProgramCostAccount.
    /// </summary>
    [JsonPropertyName("hiddenProgramCostAccount")]
    public ProgramCostAccount HiddenProgramCostAccount
    {
      get => hiddenProgramCostAccount ??= new();
      set => hiddenProgramCostAccount = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Code Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of ForFlowCashReceipt.
    /// </summary>
    [JsonPropertyName("forFlowCashReceipt")]
    public CashReceipt ForFlowCashReceipt
    {
      get => forFlowCashReceipt ??= new();
      set => forFlowCashReceipt = value;
    }

    /// <summary>
    /// A value of ForFlowCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("forFlowCashReceiptEvent")]
    public CashReceiptEvent ForFlowCashReceiptEvent
    {
      get => forFlowCashReceiptEvent ??= new();
      set => forFlowCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ForFlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("forFlowCashReceiptSourceType")]
    public CashReceiptSourceType ForFlowCashReceiptSourceType
    {
      get => forFlowCashReceiptSourceType ??= new();
      set => forFlowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ForFlowCashReceiptType.
    /// </summary>
    [JsonPropertyName("forFlowCashReceiptType")]
    public CashReceiptType ForFlowCashReceiptType
    {
      get => forFlowCashReceiptType ??= new();
      set => forFlowCashReceiptType = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
    }

    private Common pf7Pf8PriorIndicator;
    private Common pf7Pf8MoreIndicator;
    private FundTransaction selectedFundTransaction;
    private FundTransactionStatus selectedFundTransactionStatus;
    private FundTransactionStatusHistory selectedClosed;
    private DateWorkArea closedDate;
    private Common depositNoOfItems;
    private Common pftTotals;
    private CashReceipt filterStarting;
    private CashReceipt filterPft;
    private Standard receiptTypePrompt;
    private CashReceiptType filter;
    private Standard pftPrompt;
    private Fund fund;
    private Array<PageGroup> page;
    private Array<NewGroup> new1;
    private FundTransaction hiddenFundTransaction;
    private ProgramCostAccount hiddenProgramCostAccount;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Code pass;
    private CashReceipt forFlowCashReceipt;
    private CashReceiptEvent forFlowCashReceiptEvent;
    private CashReceiptSourceType forFlowCashReceiptSourceType;
    private CashReceiptType forFlowCashReceiptType;
    private Array<ZdelGroup> zdel;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of StartOfGroup.
    /// </summary>
    [JsonPropertyName("startOfGroup")]
    public Common StartOfGroup
    {
      get => startOfGroup ??= new();
      set => startOfGroup = value;
    }

    /// <summary>
    /// A value of EndOfPageGroup.
    /// </summary>
    [JsonPropertyName("endOfPageGroup")]
    public Common EndOfPageGroup
    {
      get => endOfPageGroup ??= new();
      set => endOfPageGroup = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Code Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of SelectedPft.
    /// </summary>
    [JsonPropertyName("selectedPft")]
    public CodeValue SelectedPft
    {
      get => selectedPft ??= new();
      set => selectedPft = value;
    }

    /// <summary>
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of HardcodedFtsOpen.
    /// </summary>
    [JsonPropertyName("hardcodedFtsOpen")]
    public FundTransactionStatus HardcodedFtsOpen
    {
      get => hardcodedFtsOpen ??= new();
      set => hardcodedFtsOpen = value;
    }

    /// <summary>
    /// A value of HardcodedFtsClosed.
    /// </summary>
    [JsonPropertyName("hardcodedFtsClosed")]
    public FundTransactionStatus HardcodedFtsClosed
    {
      get => hardcodedFtsClosed ??= new();
      set => hardcodedFtsClosed = value;
    }

    /// <summary>
    /// A value of HardcodedFttDeposit.
    /// </summary>
    [JsonPropertyName("hardcodedFttDeposit")]
    public FundTransactionType HardcodedFttDeposit
    {
      get => hardcodedFttDeposit ??= new();
      set => hardcodedFttDeposit = value;
    }

    /// <summary>
    /// A value of InitializedFundTransaction.
    /// </summary>
    [JsonPropertyName("initializedFundTransaction")]
    public FundTransaction InitializedFundTransaction
    {
      get => initializedFundTransaction ??= new();
      set => initializedFundTransaction = value;
    }

    /// <summary>
    /// A value of InitializedFundTransactionStatus.
    /// </summary>
    [JsonPropertyName("initializedFundTransactionStatus")]
    public FundTransactionStatus InitializedFundTransactionStatus
    {
      get => initializedFundTransactionStatus ??= new();
      set => initializedFundTransactionStatus = value;
    }

    /// <summary>
    /// A value of InitializedProgramCostAccount.
    /// </summary>
    [JsonPropertyName("initializedProgramCostAccount")]
    public ProgramCostAccount InitializedProgramCostAccount
    {
      get => initializedProgramCostAccount ??= new();
      set => initializedProgramCostAccount = value;
    }

    /// <summary>
    /// A value of InitializedFund.
    /// </summary>
    [JsonPropertyName("initializedFund")]
    public Fund InitializedFund
    {
      get => initializedFund ??= new();
      set => initializedFund = value;
    }

    /// <summary>
    /// A value of Pf78Start.
    /// </summary>
    [JsonPropertyName("pf78Start")]
    public CashReceipt Pf78Start
    {
      get => pf78Start ??= new();
      set => pf78Start = value;
    }

    private Common startOfGroup;
    private Common endOfPageGroup;
    private Common noOfPromptsSelected;
    private DateWorkArea null1;
    private Common common;
    private Code pass;
    private CodeValue selectedPft;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus hardcodedFtsOpen;
    private FundTransactionStatus hardcodedFtsClosed;
    private FundTransactionType hardcodedFttDeposit;
    private FundTransaction initializedFundTransaction;
    private FundTransactionStatus initializedFundTransactionStatus;
    private ProgramCostAccount initializedProgramCostAccount;
    private Fund initializedFund;
    private CashReceipt pf78Start;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of FundTransactionStatusHistory.
    /// </summary>
    [JsonPropertyName("fundTransactionStatusHistory")]
    public FundTransactionStatusHistory FundTransactionStatusHistory
    {
      get => fundTransactionStatusHistory ??= new();
      set => fundTransactionStatusHistory = value;
    }

    /// <summary>
    /// A value of FundTransactionStatus.
    /// </summary>
    [JsonPropertyName("fundTransactionStatus")]
    public FundTransactionStatus FundTransactionStatus
    {
      get => fundTransactionStatus ??= new();
      set => fundTransactionStatus = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Fund.
    /// </summary>
    [JsonPropertyName("fund")]
    public Fund Fund
    {
      get => fund ??= new();
      set => fund = value;
    }

    /// <summary>
    /// A value of ProgramCostAccount.
    /// </summary>
    [JsonPropertyName("programCostAccount")]
    public ProgramCostAccount ProgramCostAccount
    {
      get => programCostAccount ??= new();
      set => programCostAccount = value;
    }

    /// <summary>
    /// A value of PcaFundExplosionRule.
    /// </summary>
    [JsonPropertyName("pcaFundExplosionRule")]
    public PcaFundExplosionRule PcaFundExplosionRule
    {
      get => pcaFundExplosionRule ??= new();
      set => pcaFundExplosionRule = value;
    }

    private FundTransaction fundTransaction;
    private FundTransactionStatusHistory fundTransactionStatusHistory;
    private FundTransactionStatus fundTransactionStatus;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private Fund fund;
    private ProgramCostAccount programCostAccount;
    private PcaFundExplosionRule pcaFundExplosionRule;
  }
#endregion
}

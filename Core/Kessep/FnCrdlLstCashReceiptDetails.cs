// Program: FN_CRDL_LST_CASH_RECEIPT_DETAILS, ID: 371875590, model: 746.
// Short name: SWECRDLP
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
/// A program: FN_CRDL_LST_CASH_RECEIPT_DETAILS.
/// </para>
/// <para>
/// This is a list/maintenance screen for cash receipt details.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrdlLstCashReceiptDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRDL_LST_CASH_RECEIPT_DETAILS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrdlLstCashReceiptDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrdlLstCashReceiptDetails.
  /// </summary>
  public FnCrdlLstCashReceiptDetails(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ----------------------------------------------------------------------------
    // Date      Developer Name	        Description                         02/
    // 14/96  H. Kennedy-MTW  Retrofits
    // 04/01/96  H. Kennedy-MTW  Made changes to the display logic.  When 
    // Undistributed records were requested logic failed to produce the desired
    // results.  Made screen changes for sign off.
    // 11/27/96  R. Marchman     Add new security and next tran
    // 01/09/97  H. Kennedy      Changed logic to protect Receipt Date and 
    // Source Code fields.
    // 06/11/97  A. Samuels      Modified to display adjusted receipt amounts.
    // 11/10/98  S. Newman    Removed MCOL from screen and procedure, changed 
    // receipt date filter to received date (event), added default to the
    // received date filter, changed detail status and worker ID screen
    // literals, fixed starting collection ID logic to start at the one selected
    // instead of the next one, removed complete section of code that was
    // providing duplicate listings, adjusted view sizes to produce full page
    // viewing, adjusted security to allow flow to CRRU.  Added edits for
    // multiple prompt selections, invlaid command for enter, allowed display
    // upon return from RDSL, changed the field size  for Receipt Number, added
    // edits for undistributed flag, added logic for full group views.  Changed
    // logic to not calculate undistributed amount on Court Interface and COLA
    // adjustments. Removed logic which defaulted program fund to CSE.  Added
    // REIP indicator.  Created two new action blocks to ease strain on system.
    // Revised reads by converting 3 filters to If statements.
    // 
    // ---------------------------------------------------------------------------
    // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type and 
    // Interface_Tran_ID
    //  01/12/00    00083887     Sunya Sharp - do not display upon next tranning
    // to this screen.
    // 01/24/00    H00085803  PPhinney  - Add flow to COLL
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    local.CurrentDate.Date = Now().Date;
    export.UndistributedOnly.Flag = import.UndistributedOnly.Flag;
    export.UndistributedOnlyPrev.Flag = import.UndistributedOnlyPrev.Flag;
    export.UserInputCashReceipt.Assign(import.UserCashReceipt);
    export.UserInputFrom.ReceivedDate = import.UserInputFrom.ReceivedDate;
    export.UserInputToCashReceiptEvent.ReceivedDate =
      import.UserInputToCashReceiptEvent.ReceivedDate;
    export.UserInputCashReceiptSourceType.Code =
      import.UserCashReceiptSourceType.Code;
    export.UserInputPrevCashReceipt.Assign(import.UserInputPrevCashReceipt);
    export.UserInputPrevCashReceiptSourceType.Code =
      import.UserInputPrevCashReceiptSourceType.Code;
    export.PreviousLastCashReceipt.SequentialNumber =
      import.PreviousLastCashReceipt.SequentialNumber;
    export.PreviousLastCashReceiptDetail.SequentialIdentifier =
      import.PreviousLastCashReceiptDetail.SequentialIdentifier;
    export.MoreNext.Flag = import.MoreNext.Flag;
    export.Selection.Code = import.Selection.Code;
    export.StatusPrompt.PromptField = import.StatusPrompt.PromptField;
    export.StatusCodePrompt.PromptField = import.SourceCodePrompt.PromptField;
    export.Starting.SequentialIdentifier = import.Starting.SequentialIdentifier;
    export.HiddenSelection.Code = import.HiddenSelection.Code;
    export.ReipInd.Flag = import.ReipInd.Flag;

    // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type and 
    // Interface_Tran_ID
    export.FilterCashReceiptDetail.InterfaceTransId =
      import.FilterCashReceiptDetail.InterfaceTransId;
    export.FilterCollectionType.Code = import.FilterCollectionType.Code;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.CashReceipt.Assign(import.Import1.Item.CashReceipt);
      export.Export1.Update.CashReceiptDetail.Assign(
        import.Import1.Item.CashReceiptDetail);
      MoveCashReceiptDetailStatus(import.Import1.Item.CashReceiptDetailStatus,
        export.Export1.Update.CashReceiptDetailStatus);
      MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
        export.Export1.Update.CashReceiptSourceType);
      export.Export1.Update.CollectionType.Code =
        import.Import1.Item.CollectionType.Code;
      export.Export1.Update.UndistributedAmt.TotalCurrency =
        import.Import1.Item.UndistributedAmt.TotalCurrency;
      export.Export1.Update.DetailCashReceiptType.SystemGeneratedIdentifier =
        import.Import1.Item.DetailCashReceiptType.SystemGeneratedIdentifier;
      MoveCashReceiptEvent(import.Import1.Item.DetailCashReceiptEvent,
        export.Export1.Update.DetailCashReceiptEvent);
      export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
        import.Import1.Item.DetailCrdCrComboNo.CrdCrCombo;

      switch(AsChar(export.Export1.Item.Common.SelectChar))
      {
        case ' ':
          break;
        case '*':
          break;
        case 'S':
          ++local.Common.Count;

          if (local.Common.Count == 1)
          {
            MoveCashReceipt(export.Export1.Item.CashReceipt, export.CashReceipt);
              
            local.NextStartingCashReceipt.SequentialNumber =
              export.Export1.Item.CashReceipt.SequentialNumber;
            MoveCashReceiptDetail(export.Export1.Item.CashReceiptDetail,
              export.CashReceiptDetail);
            local.NextStartingCashReceiptDetail.SequentialIdentifier =
              export.Export1.Item.CashReceiptDetail.SequentialIdentifier;
            export.CashReceiptDetailStatHistory.ReasonCodeId =
              export.Export1.Item.CashReceiptDetailStatHistory.ReasonCodeId;
            MoveCashReceiptDetailStatus(export.Export1.Item.
              CashReceiptDetailStatus, export.CashReceiptDetailStatus);
            MoveCashReceiptSourceType(export.Export1.Item.CashReceiptSourceType,
              export.CashReceiptSourceType);
            export.CashReceiptEvent.SystemGeneratedIdentifier =
              export.Export1.Item.DetailCashReceiptEvent.
                SystemGeneratedIdentifier;
            export.CashReceiptType.SystemGeneratedIdentifier =
              export.Export1.Item.DetailCashReceiptType.
                SystemGeneratedIdentifier;
            local.Undis.TotalCurrency =
              export.Export1.Item.UndistributedAmt.TotalCurrency;

            // 01/24/00    H00085803  PPhinney  - Add flow to COLL
            export.Dlgflw.Number =
              export.Export1.Item.CashReceiptDetail.ObligorPersonNumber ?? Spaces
              (10);
          }
          else
          {
            switch(TrimEnd(global.Command))
            {
              case "DETAIL":
                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                break;
              case "REFUND":
                var field2 = GetField(export.Export1.Item.Common, "selectChar");

                field2.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                break;
              case "COLA":
                var field3 = GetField(export.Export1.Item.Common, "selectChar");

                field3.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                break;
              case "DISPLAY":
                var field4 = GetField(export.Export1.Item.Common, "selectChar");

                field4.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                break;
              case "COLL":
                var field5 = GetField(export.Export1.Item.Common, "selectChar");

                field5.Error = true;

                ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

                break;
              default:
                break;
            }
          }

          break;
        default:
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

          break;
      }

      export.Export1.Next();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "RETCRSL"))
    {
      if (!IsEmpty(import.FlowSelection.Code))
      {
        export.UserInputCashReceiptSourceType.Code = import.FlowSelection.Code;
      }
      else
      {
        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "RETRSDL"))
    {
      if (!IsEmpty(import.Selection.Code))
      {
        export.Selection.Code = import.Selection.Code;
      }
      else
      {
        export.Selection.Code = export.HiddenSelection.Code;
      }

      global.Command = "DISPLAY";
    }

    // *****
    // Next Tran/Security logic
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No views to populate
      // ****
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

    // *** PR# 00083887 - When next tranning to this screen it is taking to long
    // for the default to display.  User does not want it to display.  Sunya
    // Sharp 01/12/2000 ***
    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // No views to populate
      // ****
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type and 
    // Interface_Tran_ID
    if (!IsEmpty(export.FilterCollectionType.Code))
    {
      if (Equal(global.Command, "LIST") && AsChar
        (export.StatusCodePrompt.PromptField) != '+')
      {
        goto Test;
      }

      if (ReadCollectionType())
      {
        export.FilterCollectionType.Code = entities.CollectionType.Code;
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        var field = GetField(export.FilterCollectionType, "code");

        field.Error = true;

        return;
      }

      // 11/09/99 Per SPECIFIC instructions from TIM & LORI - If collection type
      // is entered - require EITHER Source Code or Receipt Number
      if (export.UserInputCashReceipt.SequentialNumber == 0 && IsEmpty
        (export.UserInputCashReceiptSourceType.Code))
      {
        var field1 = GetField(export.UserInputCashReceipt, "sequentialNumber");

        field1.Error = true;

        var field2 = GetField(export.UserInputCashReceiptSourceType, "code");

        field2.Error = true;

        ExitState = "FN0000_ENTER_ONE_OF_THESE_FIELDS";

        return;
      }
    }

Test:

    if (!IsEmpty(export.FilterCashReceiptDetail.InterfaceTransId))
    {
      if (IsEmpty(export.UserInputCashReceiptSourceType.Code))
      {
        var field1 = GetField(export.UserInputCashReceiptSourceType, "code");

        field1.Error = true;

        var field2 =
          GetField(export.FilterCashReceiptDetail, "interfaceTransId");

        field2.Error = true;

        ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

        return;
      }
    }

    // *****
    // Default Undistributed Only to 'N'
    // *****
    if (IsEmpty(import.UndistributedOnly.Flag))
    {
      export.UndistributedOnly.Flag = "N";
    }
    else if (AsChar(import.UndistributedOnly.Flag) == 'Y' || AsChar
      (import.UndistributedOnly.Flag) == 'N')
    {
      // These are valid values.  Continue processing.
    }
    else
    {
      var field = GetField(export.UndistributedOnly, "flag");

      field.Error = true;

      ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";
    }

    // to validate action level security
    // 01/24/00    H00085803  PPhinney  - Add flow to COLL
    if (Equal(global.Command, "REFUND") || Equal(global.Command, "DETAIL") || Equal
      (global.Command, "COLA") || Equal(global.Command, "RETURN") || Equal
      (global.Command, "ENTER") || Equal(global.Command, "RETRSDL") || Equal
      (global.Command, "COLL") || Equal(global.Command, "RETCOLL"))
    {
    }
    else
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

    switch(TrimEnd(global.Command))
    {
      case "COLA":
        ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";

        return;
      case "":
        break;
      case "HELP":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        return;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "RETCOLL":
        // 01/24/00    H00085803  PPhinney  - Add flow to COLL
        ExitState = "ACO_NN0000_ALL_OK";

        return;
      case "COLL":
        // 01/24/00    H00085803  PPhinney  - Add flow to COLL
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case ' ':
              break;
            case '*':
              break;
            case 'S':
              if (export.Export1.Item.CashReceiptDetail.DistributedAmount.
                GetValueOrDefault() == 0)
              {
                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "FN0000_NO_MONEY_HAS_BEEN_DIST";

                return;
              }

              export.Export1.Update.Common.SelectChar = "*";
              ++local.Common.Count;

              goto AfterCycle;
            default:
              break;
          }
        }

AfterCycle:

        ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

        if (local.Common.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_SELECTION_REQUIRED";
        }

        return;
      case "LIST":
        if (IsEmpty(export.StatusCodePrompt.PromptField))
        {
          var field1 = GetField(export.StatusCodePrompt, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (AsChar(export.StatusCodePrompt.PromptField) == 'S' && AsChar
          (export.StatusPrompt.PromptField) == 'S')
        {
          var field1 = GetField(export.StatusCodePrompt, "promptField");

          field1.Error = true;

          var field2 = GetField(export.StatusPrompt, "promptField");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }
        else if (AsChar(export.StatusCodePrompt.PromptField) == 'S')
        {
          export.StatusCodePrompt.PromptField = "+";
          ExitState = "ECO_LNK_TO_LIST1";

          return;
        }
        else if (AsChar(export.StatusCodePrompt.PromptField) != '+')
        {
          var field1 = GetField(export.StatusCodePrompt, "promptField");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        var field = GetField(export.StatusPrompt, "promptField");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

        switch(AsChar(export.StatusPrompt.PromptField))
        {
          case ' ':
            var field1 = GetField(export.StatusPrompt, "promptField");

            field1.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case '+':
            var field2 = GetField(export.StatusPrompt, "promptField");

            field2.Error = true;

            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            return;
          case 'S':
            if (AsChar(export.StatusCodePrompt.PromptField) == 'S' && AsChar
              (export.StatusPrompt.PromptField) == 'S')
            {
              var field4 = GetField(export.StatusCodePrompt, "promptField");

              field4.Error = true;

              var field5 = GetField(export.StatusPrompt, "promptField");

              field5.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

              return;
            }
            else
            {
              export.StatusPrompt.PromptField = "";
              export.HiddenSelection.Code = export.Selection.Code;
              ExitState = "ECO_LNK_TO_CASH_RECEIPT_SRC_TYPE";

              return;
            }

            break;
          default:
            var field3 = GetField(export.StatusPrompt, "promptField");

            field3.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        break;
      case "DISPLAY":
        switch(AsChar(export.ReipInd.Flag))
        {
          case 'Y':
            // ****Valid Option.  Continue Processing.
            break;
          case 'N':
            // ****Valid Option.  Continue Processing.
            break;
          case ' ':
            export.ReipInd.Flag = "N";

            break;
          default:
            var field1 = GetField(export.ReipInd, "flag");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

            break;
        }

        if (import.Starting.SequentialIdentifier > 0 && import
          .UserCashReceipt.SequentialNumber == 0)
        {
          var field1 =
            GetField(export.UserInputCashReceipt, "sequentialNumber");

          field1.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";

          return;
        }

        local.NextStartingCashReceiptDetail.SequentialIdentifier =
          import.Starting.SequentialIdentifier;

        // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type and
        // Interface_Tran_ID
        // Date range NOT used if Interface_Tran_ID is used.
        if (export.UserInputCashReceipt.SequentialNumber == 0 && IsEmpty
          (export.FilterCashReceiptDetail.InterfaceTransId))
        {
          if (Equal(export.UserInputFrom.ReceivedDate, null))
          {
            export.UserInputFrom.ReceivedDate = Now().Date.AddDays(-1);
          }

          if (Equal(export.UserInputToCashReceiptEvent.ReceivedDate, null))
          {
            export.UserInputToCashReceiptEvent.ReceivedDate = Now().Date;
          }
        }
        else
        {
          export.UserInputFrom.ReceivedDate = null;
          export.UserInputToCashReceiptEvent.ReceivedDate = null;
        }

        // ****Edit to Verify To Date is Greater Than From Date****
        if (Lt(export.UserInputToCashReceiptEvent.ReceivedDate,
          export.UserInputFrom.ReceivedDate))
        {
          var field1 = GetField(export.UserInputFrom, "receivedDate");

          field1.Error = true;

          var field2 =
            GetField(export.UserInputToCashReceiptEvent, "receivedDate");

          field2.Error = true;

          ExitState = "FN0000_THRU_DATE_NGTR_FROM_DATE";

          return;
        }

        if (AsChar(export.ReipInd.Flag) == 'Y')
        {
          // 11/05/99    H00077902  PPhinney  - Add filter for Collection_Type 
          // and Interface_Tran_ID
          UseFnAbYlistCashReceiptDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
          else if (Equal(export.UserInputToCashReceiptEvent.ReceivedDate,
            new DateTime(2099, 12, 31)))
          {
            export.UserInputToCashReceiptEvent.ReceivedDate = null;
          }

          if (export.Export1.IsEmpty)
          {
            // **** Check to see if this is a new display
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

            return;
          }

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

            return;
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }
        else if (AsChar(export.ReipInd.Flag) == 'N')
        {
          UseFnAbNlistCashReceiptDetail();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
          else if (Equal(export.UserInputToCashReceiptEvent.ReceivedDate,
            new DateTime(2099, 12, 31)))
          {
            export.UserInputToCashReceiptEvent.ReceivedDate = null;
          }

          if (export.Export1.IsEmpty)
          {
            // **** Check to see if this is a new display
            ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

            return;
          }

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_GROUP_VIEW_OVERFLOW";

            return;
          }

          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "DETAIL":
        // **** Transfer control to Record Collection Primary screen with "
        // DISPLAY" command and execute first.
        ExitState = "ECO_XFR_TO_RECORD_COLLECTION";

        return;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "REFUND":
        if (Equal(export.CashReceiptDetailStatus.Code, "REF"))
        {
          // **** Transfer control to Refund screen with display first.
          ExitState = "ECO_XFR_TO_REFUND_MAIN_SCREEN";

          return;
        }
        else if (local.Undis.TotalCurrency == 0)
        {
          ExitState = "FN0000_UNDISTRIB_AMOUNT_IS_ZERO";

          return;
        }
        else
        {
          // **** Transfer control to Refund screen with display first.
          ExitState = "ECO_XFR_TO_REFUND_MAIN_SCREEN";

          return;
        }

        break;
      default:
        if (Equal(global.Command, "ENTER"))
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_PF_KEY";
        }

        break;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private static void MoveExport2(FnAbYlistCashReceiptDetail.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.DetailCrdCrComboNo.CrdCrCombo = source.DetailCrdCrComboNo.CrdCrCombo;
    target.DetailCashReceiptType.SystemGeneratedIdentifier =
      source.DetailCashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(source.DetailCashReceiptEvent,
      target.DetailCashReceiptEvent);
    target.CashReceipt.Assign(source.CashReceipt);
    MoveCashReceiptSourceType(source.CashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.UndistributedAmt.TotalCurrency =
      source.UndistributedAmt.TotalCurrency;
    MoveCashReceiptDetailStatus(source.CashReceiptDetailStatus,
      target.CashReceiptDetailStatus);
    target.CashReceiptDetailStatHistory.ReasonCodeId =
      source.CashReceiptDetailStatHistory.ReasonCodeId;
    target.CollectionType.Code = source.CollectionType.Code;
  }

  private static void MoveExport3(FnAbNlistCashReceiptDetail.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.DetailCrdCrComboNo.CrdCrCombo = source.DetailCrdCrComboNo.CrdCrCombo;
    target.DetailCashReceiptType.SystemGeneratedIdentifier =
      source.DetailCashReceiptType.SystemGeneratedIdentifier;
    MoveCashReceiptEvent(source.DetailCashReceiptEvent,
      target.DetailCashReceiptEvent);
    target.CashReceipt.Assign(source.CashReceipt);
    MoveCashReceiptSourceType(source.CashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.UndistributedAmt.TotalCurrency =
      source.UndistributedAmt.TotalCurrency;
    MoveCashReceiptDetailStatus(source.CashReceiptDetailStatus,
      target.CashReceiptDetailStatus);
    target.CashReceiptDetailStatHistory.ReasonCodeId =
      source.CashReceiptDetailStatHistory.ReasonCodeId;
    target.CollectionType.Code = source.CollectionType.Code;
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

  private void UseFnAbNlistCashReceiptDetail()
  {
    var useImport = new FnAbNlistCashReceiptDetail.Import();
    var useExport = new FnAbNlistCashReceiptDetail.Export();

    useImport.UserInputFrom.ReceivedDate = export.UserInputFrom.ReceivedDate;
    useImport.UserInputToCashReceiptEvent.ReceivedDate =
      export.UserInputToCashReceiptEvent.ReceivedDate;
    useImport.CashReceiptDetailStatus.Code = export.Selection.Code;
    useImport.StartingCashReceiptDetail.SequentialIdentifier =
      local.NextStartingCashReceiptDetail.SequentialIdentifier;
    useImport.StartingCashReceipt.SequentialNumber =
      local.NextStartingCashReceipt.SequentialNumber;
    useImport.UndistributedOnly.Flag = export.UndistributedOnly.Flag;
    useImport.UserCashReceipt.Assign(export.UserInputCashReceipt);
    useImport.UserCashReceiptSourceType.Code =
      export.UserInputCashReceiptSourceType.Code;
    useImport.FilterCollectionType.Code = export.FilterCollectionType.Code;
    useImport.FilterCashReceiptDetail.InterfaceTransId =
      export.FilterCashReceiptDetail.InterfaceTransId;

    Call(FnAbNlistCashReceiptDetail.Execute, useImport, useExport);

    export.UserInputCashReceipt.Assign(useImport.UserCashReceipt);
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
  }

  private void UseFnAbYlistCashReceiptDetail()
  {
    var useImport = new FnAbYlistCashReceiptDetail.Import();
    var useExport = new FnAbYlistCashReceiptDetail.Export();

    useImport.UserInputFrom.ReceivedDate = export.UserInputFrom.ReceivedDate;
    useImport.UserInputToCashReceiptEvent.ReceivedDate =
      export.UserInputToCashReceiptEvent.ReceivedDate;
    useImport.CashReceiptDetailStatus.Code = export.Selection.Code;
    useImport.StartingCashReceiptDetail.SequentialIdentifier =
      local.NextStartingCashReceiptDetail.SequentialIdentifier;
    useImport.StartingCashReceipt.SequentialNumber =
      local.NextStartingCashReceipt.SequentialNumber;
    useImport.UndistributedOnly.Flag = export.UndistributedOnly.Flag;
    useImport.UserCashReceipt.Assign(export.UserInputCashReceipt);
    useImport.UserCashReceiptSourceType.Code =
      export.UserInputCashReceiptSourceType.Code;
    useImport.FilterCollectionType.Code = export.FilterCollectionType.Code;
    useImport.FilterCashReceiptDetail.InterfaceTransId =
      export.FilterCashReceiptDetail.InterfaceTransId;

    Call(FnAbYlistCashReceiptDetail.Execute, useImport, useExport);

    export.UserInputCashReceipt.Assign(useImport.UserCashReceipt);
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
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

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.FilterCollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
      /// A value of DetailCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("detailCrdCrComboNo")]
      public CrdCrComboNo DetailCrdCrComboNo
      {
        get => detailCrdCrComboNo ??= new();
        set => detailCrdCrComboNo = value;
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

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of UndistributedAmt.
      /// </summary>
      [JsonPropertyName("undistributedAmt")]
      public Common UndistributedAmt
      {
        get => undistributedAmt ??= new();
        set => undistributedAmt = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailStatus")]
      public CashReceiptDetailStatus CashReceiptDetailStatus
      {
        get => cashReceiptDetailStatus ??= new();
        set => cashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailStatHistory")]
      public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
      {
        get => cashReceiptDetailStatHistory ??= new();
        set => cashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private Common common;
      private CrdCrComboNo detailCrdCrComboNo;
      private CashReceiptType detailCashReceiptType;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceipt cashReceipt;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetail cashReceiptDetail;
      private Common undistributedAmt;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CollectionType collectionType;
    }

    /// <summary>
    /// A value of ReipInd.
    /// </summary>
    [JsonPropertyName("reipInd")]
    public Common ReipInd
    {
      get => reipInd ??= new();
      set => reipInd = value;
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptDetailStatus HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userInputToCashReceiptEvent")]
    public CashReceiptEvent UserInputToCashReceiptEvent
    {
      get => userInputToCashReceiptEvent ??= new();
      set => userInputToCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of UserInputFrom.
    /// </summary>
    [JsonPropertyName("userInputFrom")]
    public CashReceiptEvent UserInputFrom
    {
      get => userInputFrom ??= new();
      set => userInputFrom = value;
    }

    /// <summary>
    /// A value of AmountReadyForRelease.
    /// </summary>
    [JsonPropertyName("amountReadyForRelease")]
    public Common AmountReadyForRelease
    {
      get => amountReadyForRelease ??= new();
      set => amountReadyForRelease = value;
    }

    /// <summary>
    /// A value of SourceCodePrompt.
    /// </summary>
    [JsonPropertyName("sourceCodePrompt")]
    public Standard SourceCodePrompt
    {
      get => sourceCodePrompt ??= new();
      set => sourceCodePrompt = value;
    }

    /// <summary>
    /// A value of FlowSelection.
    /// </summary>
    [JsonPropertyName("flowSelection")]
    public CashReceiptSourceType FlowSelection
    {
      get => flowSelection ??= new();
      set => flowSelection = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceiptDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of StatusPrompt.
    /// </summary>
    [JsonPropertyName("statusPrompt")]
    public Standard StatusPrompt
    {
      get => statusPrompt ??= new();
      set => statusPrompt = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public CashReceiptDetailStatus Selection
    {
      get => selection ??= new();
      set => selection = value;
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
    /// A value of PreviousLastCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("previousLastCashReceiptDetail")]
    public CashReceiptDetail PreviousLastCashReceiptDetail
    {
      get => previousLastCashReceiptDetail ??= new();
      set => previousLastCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PreviousLastCashReceipt.
    /// </summary>
    [JsonPropertyName("previousLastCashReceipt")]
    public CashReceipt PreviousLastCashReceipt
    {
      get => previousLastCashReceipt ??= new();
      set => previousLastCashReceipt = value;
    }

    /// <summary>
    /// A value of UserCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userCashReceiptSourceType")]
    public CashReceiptSourceType UserCashReceiptSourceType
    {
      get => userCashReceiptSourceType ??= new();
      set => userCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of UserCashReceipt.
    /// </summary>
    [JsonPropertyName("userCashReceipt")]
    public CashReceipt UserCashReceipt
    {
      get => userCashReceipt ??= new();
      set => userCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputToCashReceipt")]
    public CashReceipt UserInputToCashReceipt
    {
      get => userInputToCashReceipt ??= new();
      set => userInputToCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputPrevCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userInputPrevCashReceiptSourceType")]
    public CashReceiptSourceType UserInputPrevCashReceiptSourceType
    {
      get => userInputPrevCashReceiptSourceType ??= new();
      set => userInputPrevCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of UserInputPrevCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputPrevCashReceipt")]
    public CashReceipt UserInputPrevCashReceipt
    {
      get => userInputPrevCashReceipt ??= new();
      set => userInputPrevCashReceipt = value;
    }

    /// <summary>
    /// A value of UndistributedOnly.
    /// </summary>
    [JsonPropertyName("undistributedOnly")]
    public Common UndistributedOnly
    {
      get => undistributedOnly ??= new();
      set => undistributedOnly = value;
    }

    /// <summary>
    /// A value of UndistributedOnlyPrev.
    /// </summary>
    [JsonPropertyName("undistributedOnlyPrev")]
    public Common UndistributedOnlyPrev
    {
      get => undistributedOnlyPrev ??= new();
      set => undistributedOnlyPrev = value;
    }

    /// <summary>
    /// A value of MoreNext.
    /// </summary>
    [JsonPropertyName("moreNext")]
    public Common MoreNext
    {
      get => moreNext ??= new();
      set => moreNext = value;
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
    /// A value of FilterCollectionType.
    /// </summary>
    [JsonPropertyName("filterCollectionType")]
    public CollectionType FilterCollectionType
    {
      get => filterCollectionType ??= new();
      set => filterCollectionType = value;
    }

    /// <summary>
    /// A value of FilterCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("filterCashReceiptDetail")]
    public CashReceiptDetail FilterCashReceiptDetail
    {
      get => filterCashReceiptDetail ??= new();
      set => filterCashReceiptDetail = value;
    }

    private Common reipInd;
    private CashReceiptDetailStatus hiddenSelection;
    private CashReceiptEvent userInputToCashReceiptEvent;
    private CashReceiptEvent userInputFrom;
    private Common amountReadyForRelease;
    private Standard sourceCodePrompt;
    private CashReceiptSourceType flowSelection;
    private CashReceiptDetail starting;
    private Standard statusPrompt;
    private CashReceiptDetailStatus selection;
    private Array<ImportGroup> import1;
    private CashReceiptDetail previousLastCashReceiptDetail;
    private CashReceipt previousLastCashReceipt;
    private CashReceiptSourceType userCashReceiptSourceType;
    private CashReceipt userCashReceipt;
    private CashReceipt userInputToCashReceipt;
    private CashReceiptSourceType userInputPrevCashReceiptSourceType;
    private CashReceipt userInputPrevCashReceipt;
    private Common undistributedOnly;
    private Common undistributedOnlyPrev;
    private Common moreNext;
    private NextTranInfo hidden;
    private Standard standard;
    private CollectionType filterCollectionType;
    private CashReceiptDetail filterCashReceiptDetail;
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
      /// A value of DetailCrdCrComboNo.
      /// </summary>
      [JsonPropertyName("detailCrdCrComboNo")]
      public CrdCrComboNo DetailCrdCrComboNo
      {
        get => detailCrdCrComboNo ??= new();
        set => detailCrdCrComboNo = value;
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

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("cashReceiptDetail")]
      public CashReceiptDetail CashReceiptDetail
      {
        get => cashReceiptDetail ??= new();
        set => cashReceiptDetail = value;
      }

      /// <summary>
      /// A value of UndistributedAmt.
      /// </summary>
      [JsonPropertyName("undistributedAmt")]
      public Common UndistributedAmt
      {
        get => undistributedAmt ??= new();
        set => undistributedAmt = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailStatus")]
      public CashReceiptDetailStatus CashReceiptDetailStatus
      {
        get => cashReceiptDetailStatus ??= new();
        set => cashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of CashReceiptDetailStatHistory.
      /// </summary>
      [JsonPropertyName("cashReceiptDetailStatHistory")]
      public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
      {
        get => cashReceiptDetailStatHistory ??= new();
        set => cashReceiptDetailStatHistory = value;
      }

      /// <summary>
      /// A value of CollectionType.
      /// </summary>
      [JsonPropertyName("collectionType")]
      public CollectionType CollectionType
      {
        get => collectionType ??= new();
        set => collectionType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 99;

      private Common common;
      private CrdCrComboNo detailCrdCrComboNo;
      private CashReceiptType detailCashReceiptType;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceipt cashReceipt;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptDetail cashReceiptDetail;
      private Common undistributedAmt;
      private CashReceiptDetailStatus cashReceiptDetailStatus;
      private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
      private CollectionType collectionType;
    }

    /// <summary>
    /// A value of ReipInd.
    /// </summary>
    [JsonPropertyName("reipInd")]
    public Common ReipInd
    {
      get => reipInd ??= new();
      set => reipInd = value;
    }

    /// <summary>
    /// A value of HiddenSelection.
    /// </summary>
    [JsonPropertyName("hiddenSelection")]
    public CashReceiptDetailStatus HiddenSelection
    {
      get => hiddenSelection ??= new();
      set => hiddenSelection = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("userInputToCashReceiptEvent")]
    public CashReceiptEvent UserInputToCashReceiptEvent
    {
      get => userInputToCashReceiptEvent ??= new();
      set => userInputToCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of UserInputFrom.
    /// </summary>
    [JsonPropertyName("userInputFrom")]
    public CashReceiptEvent UserInputFrom
    {
      get => userInputFrom ??= new();
      set => userInputFrom = value;
    }

    /// <summary>
    /// A value of AmountReadyForRelease.
    /// </summary>
    [JsonPropertyName("amountReadyForRelease")]
    public Common AmountReadyForRelease
    {
      get => amountReadyForRelease ??= new();
      set => amountReadyForRelease = value;
    }

    /// <summary>
    /// A value of StatusCodePrompt.
    /// </summary>
    [JsonPropertyName("statusCodePrompt")]
    public Standard StatusCodePrompt
    {
      get => statusCodePrompt ??= new();
      set => statusCodePrompt = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceiptDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Dlgflw.
    /// </summary>
    [JsonPropertyName("dlgflw")]
    public CsePerson Dlgflw
    {
      get => dlgflw ??= new();
      set => dlgflw = value;
    }

    /// <summary>
    /// A value of StatusPrompt.
    /// </summary>
    [JsonPropertyName("statusPrompt")]
    public Standard StatusPrompt
    {
      get => statusPrompt ??= new();
      set => statusPrompt = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public CashReceiptDetailStatus Selection
    {
      get => selection ??= new();
      set => selection = value;
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
    /// A value of UserInputCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userInputCashReceiptSourceType")]
    public CashReceiptSourceType UserInputCashReceiptSourceType
    {
      get => userInputCashReceiptSourceType ??= new();
      set => userInputCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of UserInputCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputCashReceipt")]
    public CashReceipt UserInputCashReceipt
    {
      get => userInputCashReceipt ??= new();
      set => userInputCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputToCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputToCashReceipt")]
    public CashReceipt UserInputToCashReceipt
    {
      get => userInputToCashReceipt ??= new();
      set => userInputToCashReceipt = value;
    }

    /// <summary>
    /// A value of UserInputPrevCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("userInputPrevCashReceiptSourceType")]
    public CashReceiptSourceType UserInputPrevCashReceiptSourceType
    {
      get => userInputPrevCashReceiptSourceType ??= new();
      set => userInputPrevCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of UserInputPrevCashReceipt.
    /// </summary>
    [JsonPropertyName("userInputPrevCashReceipt")]
    public CashReceipt UserInputPrevCashReceipt
    {
      get => userInputPrevCashReceipt ??= new();
      set => userInputPrevCashReceipt = value;
    }

    /// <summary>
    /// A value of PreviousLastCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("previousLastCashReceiptDetail")]
    public CashReceiptDetail PreviousLastCashReceiptDetail
    {
      get => previousLastCashReceiptDetail ??= new();
      set => previousLastCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PreviousLastCashReceipt.
    /// </summary>
    [JsonPropertyName("previousLastCashReceipt")]
    public CashReceipt PreviousLastCashReceipt
    {
      get => previousLastCashReceipt ??= new();
      set => previousLastCashReceipt = value;
    }

    /// <summary>
    /// A value of UndistributedOnly.
    /// </summary>
    [JsonPropertyName("undistributedOnly")]
    public Common UndistributedOnly
    {
      get => undistributedOnly ??= new();
      set => undistributedOnly = value;
    }

    /// <summary>
    /// A value of UndistributedOnlyPrev.
    /// </summary>
    [JsonPropertyName("undistributedOnlyPrev")]
    public Common UndistributedOnlyPrev
    {
      get => undistributedOnlyPrev ??= new();
      set => undistributedOnlyPrev = value;
    }

    /// <summary>
    /// A value of MoreNext.
    /// </summary>
    [JsonPropertyName("moreNext")]
    public Common MoreNext
    {
      get => moreNext ??= new();
      set => moreNext = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of FilterCollectionType.
    /// </summary>
    [JsonPropertyName("filterCollectionType")]
    public CollectionType FilterCollectionType
    {
      get => filterCollectionType ??= new();
      set => filterCollectionType = value;
    }

    /// <summary>
    /// A value of FilterCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("filterCashReceiptDetail")]
    public CashReceiptDetail FilterCashReceiptDetail
    {
      get => filterCashReceiptDetail ??= new();
      set => filterCashReceiptDetail = value;
    }

    private Common reipInd;
    private CashReceiptDetailStatus hiddenSelection;
    private CashReceiptEvent userInputToCashReceiptEvent;
    private CashReceiptEvent userInputFrom;
    private Common amountReadyForRelease;
    private Standard statusCodePrompt;
    private CashReceiptDetail starting;
    private CsePerson dlgflw;
    private Standard statusPrompt;
    private CashReceiptDetailStatus selection;
    private Array<ExportGroup> export1;
    private CashReceiptSourceType userInputCashReceiptSourceType;
    private CashReceipt userInputCashReceipt;
    private CashReceipt userInputToCashReceipt;
    private CashReceiptSourceType userInputPrevCashReceiptSourceType;
    private CashReceipt userInputPrevCashReceipt;
    private CashReceiptDetail previousLastCashReceiptDetail;
    private CashReceipt previousLastCashReceipt;
    private Common undistributedOnly;
    private Common undistributedOnlyPrev;
    private Common moreNext;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private NextTranInfo hidden;
    private Standard standard;
    private CollectionType filterCollectionType;
    private CashReceiptDetail filterCashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of UserInput.
    /// </summary>
    [JsonPropertyName("userInput")]
    public CashReceipt UserInput
    {
      get => userInput ??= new();
      set => userInput = value;
    }

    /// <summary>
    /// A value of UserInputTo.
    /// </summary>
    [JsonPropertyName("userInputTo")]
    public CashReceipt UserInputTo
    {
      get => userInputTo ??= new();
      set => userInputTo = value;
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
    /// A value of Undis.
    /// </summary>
    [JsonPropertyName("undis")]
    public Common Undis
    {
      get => undis ??= new();
      set => undis = value;
    }

    /// <summary>
    /// A value of NextStartingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("nextStartingCashReceiptDetail")]
    public CashReceiptDetail NextStartingCashReceiptDetail
    {
      get => nextStartingCashReceiptDetail ??= new();
      set => nextStartingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of NextStartingCashReceipt.
    /// </summary>
    [JsonPropertyName("nextStartingCashReceipt")]
    public CashReceipt NextStartingCashReceipt
    {
      get => nextStartingCashReceipt ??= new();
      set => nextStartingCashReceipt = value;
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
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of PreviousCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("previousCashReceiptSourceType")]
    public CashReceiptSourceType PreviousCashReceiptSourceType
    {
      get => previousCashReceiptSourceType ??= new();
      set => previousCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PreviousCashReceipt.
    /// </summary>
    [JsonPropertyName("previousCashReceipt")]
    public CashReceipt PreviousCashReceipt
    {
      get => previousCashReceipt ??= new();
      set => previousCashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    private CashReceipt userInput;
    private CashReceipt userInputTo;
    private DateWorkArea currentDate;
    private Common undis;
    private CashReceiptDetail nextStartingCashReceiptDetail;
    private CashReceipt nextStartingCashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatus suspended;
    private CashReceiptSourceType previousCashReceiptSourceType;
    private CashReceipt previousCashReceipt;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CollectionType collectionType;
  }
#endregion
}

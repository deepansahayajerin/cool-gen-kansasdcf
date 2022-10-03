// Program: FN_CREL_LST_CASH_RECEIPTS, ID: 371722667, model: 746.
// Short name: SWECRELP
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
/// A program: FN_CREL_LST_CASH_RECEIPTS.
/// </para>
/// <para>
/// Resp: Finance
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrelLstCashReceipts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREL_LST_CASH_RECEIPTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrelLstCashReceipts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrelLstCashReceipts.
  /// </summary>
  public FnCrelLstCashReceipts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // Date 	  	Developer Name	  Description
    // 06/27/95	D.M. Nilsen-MTW	  Source
    // 02/03/96	Holly Kennedy-MTW Retrofits
    // 10/01/96	SHERAZ MALIK-MTW  IDCRS
    // 12/13/96        R. Marchman	  Add new security/next tran
    // 02/09/97        T.O.Redmond       4 flows added to replace Process Key.
    // 				  Documentation of Prad improved.
    // 03/04/97	A Samuels	  Test Support fixes.
    // 03/12/97	T.O.Redmond	  Remove PF20 Release
    // 04/08/97        C. Dasgupta       Changed screen literal according to the
    // requirement
    // 
    // 9/30/09         S. Newman         Prevented CRs in forward status from
    // flowing to CRDE.  Prevented CRs in interfaced status (unless CR type
    // equal MAININT) and have collection details added from flowing to CRDE.
    // Also, allowed recorded cash receipts to flow to CRDE if the receipted
    // date equals the current date.  Added edits to only allow CRs in balanced
    // or forwarded status with a 'Cash' CR type code to flow to CRFO.  Added
    // edits to not allow cash receipts in delete, balance, recorded or forward
    // status or Non CSE program fund type to flow to CRRC.  Placed cursor on
    // source type, added Invalid Command for ENTER, and changed program to
    // leave both 'S's displayed when multiple selections are chosen.
    // 
    // --------------------------------------------------------------------
    // * * * * * * * * * * * * *
    // 09/09/99  PPhinney  H00072859
    // - - ADD Starting Cash Receipt Number to Screen
    // - - and A/B FN_CAB_NLIST_CASH_RECEIPT
    // - - and A/B FN_CAB_YLIST_CASH_RECEIPT
    // - - Changed starting date to 14 days prior
    // - -   instead of one month
    // * * * * * * * * * * * * *
    // 11/17/99  PPhinney  H00080433
    // - - ADD Flow to CRDI so a Receipt can be linked
    // - -  to it's Deposit.
    // * * * * * * * * * * * * *
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }
    else if (IsEmpty(global.Command))
    {
      global.Command = "DISPLAY";
    }

    export.TotalScreenAmount.TotalCurrency =
      import.TotalScreenAmount.TotalCurrency;
    export.CheckType.PromptField = import.CheckType.PromptField;
    MoveCashReceipt2(import.CashReceipt, export.CashReceipt);
    export.CashReceiptSourceType.Code = import.CashReceiptSourceType.Code;
    export.HiddenCashReceiptSourceType.Code =
      import.HiddenCashReceiptSourceType.Code;
    export.HiddenCashReceiptStatus.Code = import.HiddenCashReceiptStatus.Code;
    export.CashReceiptStatus.Code = import.CashReceiptStatus.Code;
    export.HiddenFirstTimeIn.Flag = import.HiddenFirstTime.Flag;
    export.ReipInd.Flag = import.ReipInd.Flag;

    if (Lt(local.Null1.Date, import.From.Date))
    {
      MoveDateWorkArea(import.From, export.From);
    }
    else
    {
      // 09/09/99  PPhinney  H00072859
      // - Changed starting date to 14 days prior instead of one month
      export.From.Date = Now().Date.AddDays(-14);
    }

    if (Lt(local.Null1.Date, import.Thru.Date))
    {
      MoveDateWorkArea(import.Thru, export.Thru);
    }
    else
    {
      export.Thru.Date = Now().Date;
    }

    export.HiddenExportDisplayed.Flag = import.HiddenImportDisplayed.Flag;
    export.PromptSourceCode.SelectChar = import.PromptSourceCode.SelectChar;
    export.PromptStatusCode.SelectChar = import.PromptStatusCode.SelectChar;

    // 09/09/99  PPhinney  H00072859
    // - - ADD Starting Cash Receipt Number to Screen
    export.Starting.Assign(import.Starting);
    MoveDateWorkArea(import.PassFrom, export.PassFrom);
    MoveDateWorkArea(import.PassThru, export.PassThru);
    export.PassStarting.Assign(import.PassStarting);
    export.PassLinkFlag.Flag = import.PassLinkFlag.Flag;
    local.Counter.Count = 0;
    local.Counter.Flag = "N";

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
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

        export.Export1.Update.DetailCashReceipt.Assign(
          import.Import1.Item.DetailCashReceipt);
        export.Export1.Update.DetailCashReceiptStatus.Code =
          import.Import1.Item.DetailCashReceiptStatus.Code;
        MoveCashReceiptSourceType(import.Import1.Item.
          DetailCashReceiptSourceType,
          export.Export1.Update.DetailCashReceiptSourceType);
        MoveCommon(import.Import1.Item.DetailCommon,
          export.Export1.Update.DetailCommon);
        export.Export1.Update.DetailHidden.SystemGeneratedIdentifier =
          import.Import1.Item.DetailHidden.SystemGeneratedIdentifier;
        MoveCashReceiptType(import.Import1.Item.DetailCashReceiptType,
          export.Export1.Update.DetailCashReceiptType);

        if (!IsEmpty(import.Import1.Item.DetailCommon.SelectChar))
        {
          if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Counter.Count;

            if (local.Counter.Count > 1)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              goto Test;
            }

            if (Equal(global.Command, "RETLINK"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "*";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Color = "green";
              field.Highlighting = Highlighting.Underscore;
              field.Protected = false;
              field.Focused = true;
            }

            if (Equal(global.Command, "RETURN"))
            {
              MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                export.HiddenExportSelectedCashReceipt);
              export.HiddenExportSelectedCashReceiptStatus.Code =
                export.Export1.Item.DetailCashReceiptStatus.Code;
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType,
                export.HiddenExportSelectedCashReceiptSourceType);
              MoveCashReceiptType(export.Export1.Item.DetailCashReceiptType,
                export.HiddenExportSelectedCashReceiptType);
              export.HiddenExportSelectedCashReceiptEvent.
                SystemGeneratedIdentifier =
                  export.Export1.Item.DetailHidden.SystemGeneratedIdentifier;
            }
          }
          else if (AsChar(import.Import1.Item.DetailCommon.SelectChar) == '*')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
          }
          else
          {
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;
          }
        }

Test:

        export.Export1.Next();
      }

      if (local.Counter.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "PRMPTRET"))
    {
      return;
    }
    else if (Equal(global.Command, "CRDE") || Equal(global.Command, "CRFO") || Equal
      (global.Command, "CREC") || Equal(global.Command, "CRRC") || Equal
      (global.Command, "RETCDVL") || Equal(global.Command, "RETCRSL") || Equal
      (global.Command, "RETRSTL") || Equal(global.Command, "ENTER") || Equal
      (global.Command, "CRDI"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    if (Equal(global.Command, "RETURN"))
    {
      ExitState = "ACO_NE0000_RETURN";

      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "RETCDVL":
        export.CheckType.PromptField = "";

        var field = GetField(export.CashReceipt, "checkType");

        field.Color = "green";
        field.Highlighting = Highlighting.Underscore;
        field.Protected = false;
        field.Focused = true;

        if (!IsEmpty(import.Selected.Cdvalue))
        {
          export.CashReceipt.CheckType = import.Selected.Cdvalue;
        }
        else
        {
        }

        global.Command = "DISPLAY";

        break;
      case "RETRSTL":
        export.PromptStatusCode.SelectChar = "";

        if (IsEmpty(import.CashReceiptStatus.Code))
        {
          export.CashReceiptStatus.Code = export.HiddenCashReceiptStatus.Code;
        }
        else
        {
        }

        global.Command = "DISPLAY";

        break;
      case "RETCRSL":
        export.PromptSourceCode.SelectChar = "";

        if (IsEmpty(import.CashReceiptSourceType.Code))
        {
          export.CashReceiptSourceType.Code =
            export.HiddenCashReceiptSourceType.Code;
        }
        else
        {
        }

        global.Command = "DISPLAY";

        break;
      default:
        break;
    }

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "DISPLAY":
        switch(AsChar(export.ReipInd.Flag))
        {
          case 'Y':
            // ****Valid Option.  Continue Processing.
            break;
          case ' ':
            export.ReipInd.Flag = "N";

            break;
          case 'N':
            // ****Valid Option.  Continue Processing.
            break;
          default:
            var field = GetField(export.ReipInd, "flag");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

            break;
        }

        if (!IsEmpty(export.CashReceipt.CheckType))
        {
          local.Pass.CodeName = "CHECK TYPE";
          local.CodeValue.Cdvalue = export.CashReceipt.CheckType ?? Spaces(10);
          UseCabValidateCodeValue();

          if (local.Common.Count == 1 || local.Common.Count == 2)
          {
            var field = GetField(export.CashReceipt, "checkType");

            field.Error = true;

            ExitState = "CODE_VALUE_NF";

            return;
          }
        }

        // ************************************************
        // *Check that the from date is less than the thru*
        // *date.
        // 
        // *
        // ************************************************
        if (Lt(export.Thru.Date, export.From.Date))
        {
          var field1 = GetField(export.Thru, "date");

          field1.Error = true;

          var field2 = GetField(export.From, "date");

          field2.Error = true;

          export.HiddenExportDisplayed.Flag = "N";
          ExitState = "FN0000_THRU_DATE_NGTR_FROM_DATE";

          return;
        }

        if (Equal(export.Starting.ReceiptDate, local.Null1.Date))
        {
          export.Starting.ReceiptDate = UseCabSetMaximumDiscontinueDate();
        }

        if (AsChar(export.ReipInd.Flag) == 'Y')
        {
          // * * * * * * * * * * * * *
          // 09/09/99  PPhinney  H00072859
          // - - ADD Starting Cash Receipt Number
          // 09/20/99  J Katz  H00074126
          // - - ADD Starting Check Number to Screen
          // - - Code check number filter as exact match [09/23]
          // * * * * * * * * * * * * *
          UseFnCabYlistCashReceipt();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Export1.IsEmpty)
            {
              export.HiddenExportDisplayed.Flag = "N";
              ExitState = "FN0000_NO_CASH_RECEIPTS_FOUND";
            }
            else if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_IS_FULL";
              export.HiddenExportDisplayed.Flag = "Y";
            }
            else
            {
              export.HiddenExportDisplayed.Flag = "Y";
              ExitState = "FN0000_CASH_RCPT_SUCCESSFUL_DISP";
            }
          }
          else if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF"))
          {
            var field = GetField(export.CashReceiptSourceType, "code");

            field.Error = true;
          }
          else if (IsExitState("FN0108_CASH_RCPT_STAT_NF"))
          {
            var field = GetField(export.CashReceiptStatus, "code");

            field.Error = true;
          }
          else
          {
          }
        }
        else if (AsChar(export.ReipInd.Flag) == 'N')
        {
          // * * * * * * * * * * * * *
          // 09/09/99  PPhinney  H00072859
          // - - ADD Starting Cash Receipt Number
          // 09/20/99  J Katz  H00074126
          // - - ADD Starting Check Number to Screen
          // - - Code check number filter as exact match [09/23]
          // * * * * * * * * * * * * *
          UseFnCabNlistCashReceipt();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Export1.IsEmpty)
            {
              export.HiddenExportDisplayed.Flag = "N";
              ExitState = "FN0000_NO_CASH_RECEIPTS_FOUND";
            }
            else if (export.Export1.IsFull)
            {
              ExitState = "ACO_NI0000_LIST_IS_FULL";
              export.HiddenExportDisplayed.Flag = "Y";
            }
            else
            {
              export.HiddenExportDisplayed.Flag = "Y";
              ExitState = "FN0000_CASH_RCPT_SUCCESSFUL_DISP";
            }
          }
          else if (IsExitState("FN0097_CASH_RCPT_SOURCE_TYPE_NF"))
          {
            var field = GetField(export.CashReceiptSourceType, "code");

            field.Error = true;
          }
          else if (IsExitState("FN0108_CASH_RCPT_STAT_NF"))
          {
            var field = GetField(export.CashReceiptStatus, "code");

            field.Error = true;
          }
          else
          {
          }
        }

        break;
      case "LIST":
        switch(AsChar(import.PromptSourceCode.SelectChar))
        {
          case 'S':
            ++local.Prompt.Count;
            ExitState = "ECO_LNK_LST_CASH_SOURCES";
            export.HiddenCashReceiptSourceType.Code =
              export.CashReceiptSourceType.Code;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptSourceCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(import.PromptStatusCode.SelectChar))
        {
          case 'S':
            ++local.Prompt.Count;
            ExitState = "ECO_LNK_TO_CASH_RECEIPT_STATUS";
            export.HiddenCashReceiptStatus.Code = export.CashReceiptStatus.Code;

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.PromptStatusCode, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(import.CheckType.PromptField))
        {
          case 'S':
            ++local.Prompt.Count;
            export.Pass.CodeName = "CHECK TYPE";
            ExitState = "ECO_LNK_TO_CODE_VALUES";

            break;
          case ' ':
            break;
          default:
            var field = GetField(export.CheckType, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(local.Prompt.Count)
        {
          case 0:
            ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

            break;
          case 1:
            break;
          default:
            if (AsChar(import.CheckType.PromptField) == 'S')
            {
              var field = GetField(export.CheckType, "promptField");

              field.Error = true;
            }

            if (AsChar(import.PromptSourceCode.SelectChar) == 'S')
            {
              var field = GetField(export.PromptSourceCode, "selectChar");

              field.Error = true;
            }

            if (AsChar(import.PromptStatusCode.SelectChar) == 'S')
            {
              var field = GetField(export.PromptStatusCode, "selectChar");

              field.Error = true;
            }

            ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

            break;
        }

        break;
      case "PREV":
        if (AsChar(import.HiddenImportDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // 09/09/99  PPhinney  H00072859
        // - - ADD Starting Cash Receipt Number to Screen
        if (AsChar(export.PassLinkFlag.Flag) == 'Y')
        {
          global.Command = "DISPLAY";
          ExitState = "ECO_LNK_RETURN_FROM_LINK";

          return;
        }

        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        if (AsChar(import.HiddenImportDisplayed.Flag) != 'Y')
        {
          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

          return;
        }

        // 09/09/99  PPhinney  H00072859
        // - - ADD Starting Cash Receipt Number to Screen
        // 09/20/99  J Katz  H00074126
        // - - ADD Starting Check Number to Screen
        // - - Code check number filter as exact match [09/23]
        export.PassStarting.SequentialNumber = 0;
        export.PassStarting.CheckNumber = "";
        export.PassStarting.ReceiptDate = local.Null1.Date;
        export.SetPassLinkFlag.Flag = "";

        if (export.Export1.IsFull)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.PassStarting.SequentialNumber =
              export.Export1.Item.DetailCashReceipt.SequentialNumber;
            export.PassStarting.CheckNumber = export.Starting.CheckNumber ?? "";
            export.PassFrom.Date =
              export.Export1.Item.DetailCashReceipt.ReceiptDate;
            export.PassThru.Date =
              export.Export1.Item.DetailCashReceipt.ReceiptDate;
          }
        }

        if (export.PassStarting.SequentialNumber > 0)
        {
          export.SetPassLinkFlag.Flag = "Y";
          ExitState = "ECO_XFR_TO_NEXT_SCRN";

          return;
        }

        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      default:
        // 11/17/99  PPhinney  H00080433
        // - - ADD Flow to CRDI so a Receipt can be linked
        // - -  to it's Deposit.
        if (Equal(global.Command, "CRDE") || Equal(global.Command, "CRFO") || Equal
          (global.Command, "CREC") || Equal(global.Command, "CRRC") || Equal
          (global.Command, "CRDI"))
        {
          if (AsChar(import.HiddenImportDisplayed.Flag) != 'Y')
          {
            ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

            return;
          }

          switch(local.Counter.Count)
          {
            case 0:
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              return;
            case 1:
              // Continue
              break;
            default:
              break;
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (AsChar(export.Export1.Item.DetailCommon.SelectChar) != 'S')
            {
              continue;
            }

            switch(TrimEnd(global.Command))
            {
              case "CREC":
                // ************************************************
                // *    Link to Cash Receipting Screen(CREC).     *
                // *User has requested that he be provided with   *
                // *more details for the selected cash Receipt.   *
                // *User has ability to change a Receipt on CREC  *
                // ************************************************
                if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
                {
                  MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                    export.HiddenExportSelectedCashReceipt);
                  export.HiddenExportSelectedCashReceiptStatus.Code =
                    export.Export1.Item.DetailCashReceiptStatus.Code;
                  MoveCashReceiptSourceType(export.Export1.Item.
                    DetailCashReceiptSourceType,
                    export.HiddenExportSelectedCashReceiptSourceType);
                  export.HiddenExportSelectedCashReceiptEvent.
                    SystemGeneratedIdentifier =
                      export.Export1.Item.DetailHidden.
                      SystemGeneratedIdentifier;
                  MoveCashReceiptType(export.Export1.Item.DetailCashReceiptType,
                    export.HiddenExportSelectedCashReceiptType);
                  ExitState = "ECO_LNK_TO_CASH_RECEIPT";

                  return;
                }

                break;
              case "CRDE":
                // *******************************************************************
                // *                      DELETE CASH RECEIPT
                // *
                // * Move the selected entry and link to Delete Cash Receipt (
                // CRDE)  *          * CAN ONLY DELETE:
                // *
                // 
                // * 1. 'REC' status with 'FCRT REC' or 'FDIR PMT' cash receipt
                // type *           *    & detail ind='*'
                // *
                // 
                // * 2. 'INTF' status with 'MANINT' cash receipt type & detail
                // ind='*'             *
                // 
                // * 3. 'DEL' status
                // *
                // *******************************************************************
                if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
                {
                  if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                    "REC"))
                  {
                    if (Equal(export.Export1.Item.DetailCashReceiptType.Code,
                      "FCRT REC") || Equal
                      (export.Export1.Item.DetailCashReceiptType.Code,
                      "FDIR PMT"))
                    {
                      MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                        export.HiddenExportSelectedCashReceipt);
                      export.HiddenExportSelectedCashReceiptStatus.Code =
                        export.Export1.Item.DetailCashReceiptStatus.Code;
                      MoveCashReceiptSourceType(export.Export1.Item.
                        DetailCashReceiptSourceType,
                        export.HiddenExportSelectedCashReceiptSourceType);
                      export.HiddenExportSelectedCashReceiptEvent.
                        SystemGeneratedIdentifier =
                          export.Export1.Item.DetailHidden.
                          SystemGeneratedIdentifier;
                      MoveCashReceiptType(export.Export1.Item.
                        DetailCashReceiptType,
                        export.HiddenExportSelectedCashReceiptType);
                      ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

                      return;
                    }
                    else if (Equal(export.Export1.Item.DetailCashReceipt.
                      ReceiptDate, Now().Date) && AsChar
                      (export.Export1.Item.DetailCommon.Flag) == '*')
                    {
                      MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                        export.HiddenExportSelectedCashReceipt);
                      export.HiddenExportSelectedCashReceiptStatus.Code =
                        export.Export1.Item.DetailCashReceiptStatus.Code;
                      MoveCashReceiptSourceType(export.Export1.Item.
                        DetailCashReceiptSourceType,
                        export.HiddenExportSelectedCashReceiptSourceType);
                      export.HiddenExportSelectedCashReceiptEvent.
                        SystemGeneratedIdentifier =
                          export.Export1.Item.DetailHidden.
                          SystemGeneratedIdentifier;
                      MoveCashReceiptType(export.Export1.Item.
                        DetailCashReceiptType,
                        export.HiddenExportSelectedCashReceiptType);
                      ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

                      return;
                    }
                    else
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Export1.Item.DetailCashReceiptType,
                        "code");

                      field2.Error = true;

                      var field3 =
                        GetField(export.Export1.Item.DetailCommon, "selectChar");
                        

                      field3.Error = true;

                      var field4 =
                        GetField(export.Export1.Item.DetailCommon, "flag");

                      field4.Error = true;

                      ExitState = "FN0000_CANT_DELETE_CR_DIFF_DAY";

                      return;
                    }
                  }
                  else if (Equal(export.Export1.Item.DetailCashReceiptStatus.
                    Code, "INTF"))
                  {
                    if (Equal(export.Export1.Item.DetailCashReceiptType.Code,
                      "MANINT"))
                    {
                      if (AsChar(export.Export1.Item.DetailCommon.Flag) == '*')
                      {
                        // *******Okay To Delete.  Continue Processing.
                        MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                          export.HiddenExportSelectedCashReceipt);
                        export.HiddenExportSelectedCashReceiptStatus.Code =
                          export.Export1.Item.DetailCashReceiptStatus.Code;
                        MoveCashReceiptSourceType(export.Export1.Item.
                          DetailCashReceiptSourceType,
                          export.HiddenExportSelectedCashReceiptSourceType);
                        export.HiddenExportSelectedCashReceiptEvent.
                          SystemGeneratedIdentifier =
                            export.Export1.Item.DetailHidden.
                            SystemGeneratedIdentifier;
                        MoveCashReceiptType(export.Export1.Item.
                          DetailCashReceiptType,
                          export.HiddenExportSelectedCashReceiptType);
                        ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

                        return;
                      }
                      else
                      {
                        var field1 =
                          GetField(export.Export1.Item.DetailCashReceiptStatus,
                          "code");

                        field1.Error = true;

                        var field2 =
                          GetField(export.Export1.Item.DetailCommon,
                          "selectChar");

                        field2.Error = true;

                        var field3 =
                          GetField(export.Export1.Item.DetailCashReceiptType,
                          "code");

                        field3.Error = true;

                        var field4 =
                          GetField(export.Export1.Item.DetailCommon, "flag");

                        field4.Error = true;

                        ExitState = "FN0000_CANT_DEL_TYPE_AND_STAT";

                        return;
                      }
                    }
                  }
                  else if (Equal(export.Export1.Item.DetailCashReceiptStatus.
                    Code, "DEL"))
                  {
                    // *******Okay To Delete.  Continue Processing.
                    MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                      export.HiddenExportSelectedCashReceipt);
                    export.HiddenExportSelectedCashReceiptStatus.Code =
                      export.Export1.Item.DetailCashReceiptStatus.Code;
                    MoveCashReceiptSourceType(export.Export1.Item.
                      DetailCashReceiptSourceType,
                      export.HiddenExportSelectedCashReceiptSourceType);
                    export.HiddenExportSelectedCashReceiptEvent.
                      SystemGeneratedIdentifier =
                        export.Export1.Item.DetailHidden.
                        SystemGeneratedIdentifier;
                    MoveCashReceiptType(export.Export1.Item.
                      DetailCashReceiptType,
                      export.HiddenExportSelectedCashReceiptType);
                    ExitState = "ECO_LNK_TO_DEL_CASH_RECEIPT";

                    return;
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCashReceiptStatus,
                      "code");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCashReceiptType, "code");
                      

                    field2.Error = true;

                    var field3 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field3.Error = true;

                    var field4 =
                      GetField(export.Export1.Item.DetailCommon, "flag");

                    field4.Error = true;

                    ExitState = "FN0000_CANNOT_DELETE_STATUS_TYPE";

                    return;
                  }
                }

                break;
              case "CRFO":
                // ************************************************
                // *            Forward a Cash Receipt            *
                // *Move the selected entry to the select fields  *
                // *and link to Forward Cash Receipt screen(CRFO).*
                // ************************************************
                // *********************************************************
                // * Add edit to only allow receipts in balanced or forward
                // * status to be
                // forwarded to CRFO screen.
                // 
                // *********************************************************
                if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
                {
                  if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                    "BAL") || Equal
                    (export.Export1.Item.DetailCashReceiptStatus.Code, "FWD") &&
                    (
                      Equal(export.Export1.Item.DetailCashReceiptType.Code,
                    "CHECK") || Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "MNY ORD") ||
                    Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "CURRENCY") ||
                    Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "CRDT CRD") ||
                    Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "EFT") || Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "CRT REC") ||
                    Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "INTERFUND")
                    || Equal
                    (export.Export1.Item.DetailCashReceiptType.Code, "MANINT")))
                  {
                    MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                      export.HiddenExportSelectedCashReceipt);
                    export.HiddenExportSelectedCashReceiptStatus.Code =
                      export.Export1.Item.DetailCashReceiptStatus.Code;
                    MoveCashReceiptSourceType(export.Export1.Item.
                      DetailCashReceiptSourceType,
                      export.HiddenExportSelectedCashReceiptSourceType);
                    export.HiddenExportSelectedCashReceiptEvent.
                      SystemGeneratedIdentifier =
                        export.Export1.Item.DetailHidden.
                        SystemGeneratedIdentifier;
                    MoveCashReceiptType(export.Export1.Item.
                      DetailCashReceiptType,
                      export.HiddenExportSelectedCashReceiptType);
                    ExitState = "ECO_LNK_TO_FORWARD_CASH_RECEIPT";

                    return;
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCashReceiptStatus,
                      "code");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCashReceiptType, "code");
                      

                    field2.Error = true;

                    var field3 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field3.Error = true;

                    ExitState = "FN0000_FORWARD_BALANCED_CASH_CR";

                    return;
                  }
                }

                break;
              case "CRRC":
                // ************************************************
                // *    Link to Cash Collections Screen(CRRC).    *
                // *User has requested that he be provided with   *
                // *more details for the selected cash Receipt.   *
                // *User has ability to provide and or change     *
                // *details on the cash collection.               *
                // ************************************************
                if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
                {
                  if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                    "FWD") || Equal
                    (export.Export1.Item.DetailCashReceiptStatus.Code, "DEL") ||
                    Equal
                    (export.Export1.Item.DetailCashReceiptStatus.Code, "BAL"))
                  {
                    // *********************************************************
                    // Cash Receipt selected is in FWD (Forwarded Status) or DEL
                    // (Deleted Status) or REC (
                    // Receipted Status) or BAL (Balanced Status),)therefore
                    // flow to CRRC is not allowed.
                    // *********************************************************
                    var field =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field.Error = true;

                    if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                      "FWD"))
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field1.Error = true;

                      ExitState = "FN0000_SEL_CASH_REC_IN_FWD_STAT";
                    }
                    else if (Equal(export.Export1.Item.DetailCashReceiptStatus.
                      Code, "DEL"))
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field1.Error = true;

                      ExitState = "FN0000_SEL_CASH_REC_IN_DEL_STAT";
                    }
                    else if (Equal(export.Export1.Item.DetailCashReceiptStatus.
                      Code, "BAL"))
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field1.Error = true;

                      ExitState = "FN0000_SEL_CASH_REC_IN_BAL_STAT";
                    }

                    return;
                  }

                  // ***********************************************************************
                  // 
                  // *  Add logic to allow any payments with details to flow to
                  // CRRC to add,
                  // 
                  // *  review, or  change details.
                  // 
                  // ***********************************************************************
                  if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                    "REC"))
                  {
                    if (AsChar(export.Export1.Item.DetailCommon.Flag) == '*')
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCommon, "selectChar");
                        

                      field1.Error = true;

                      var field2 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field2.Error = true;

                      ExitState = "FN0000_SEL_CASH_REC_IN_REC_STAT";

                      return;
                    }
                    else
                    {
                      MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                        export.HiddenExportSelectedCashReceipt);
                      export.HiddenExportSelectedCashReceiptStatus.Code =
                        export.Export1.Item.DetailCashReceiptStatus.Code;
                      MoveCashReceiptSourceType(export.Export1.Item.
                        DetailCashReceiptSourceType,
                        export.HiddenExportSelectedCashReceiptSourceType);
                      export.HiddenExportSelectedCashReceiptEvent.
                        SystemGeneratedIdentifier =
                          export.Export1.Item.DetailHidden.
                          SystemGeneratedIdentifier;
                      MoveCashReceiptType(export.Export1.Item.
                        DetailCashReceiptType,
                        export.HiddenExportSelectedCashReceiptType);
                      ExitState = "ECO_LNK_TO_COLLECT_DET_SCRN";

                      return;
                    }
                  }

                  // ********************************************************
                  // 
                  // *  Add logic to only allow program fund type "CSE"
                  // 
                  // *  to flow to CRRC to add, review, or change details.
                  // 
                  // ********************************************************
                  if (Equal(export.Export1.Item.DetailCashReceipt.CheckType,
                    "CSE"))
                  {
                    MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                      export.HiddenExportSelectedCashReceipt);
                    export.HiddenExportSelectedCashReceiptStatus.Code =
                      export.Export1.Item.DetailCashReceiptStatus.Code;
                    MoveCashReceiptSourceType(export.Export1.Item.
                      DetailCashReceiptSourceType,
                      export.HiddenExportSelectedCashReceiptSourceType);
                    export.HiddenExportSelectedCashReceiptEvent.
                      SystemGeneratedIdentifier =
                        export.Export1.Item.DetailHidden.
                        SystemGeneratedIdentifier;
                    MoveCashReceiptType(export.Export1.Item.
                      DetailCashReceiptType,
                      export.HiddenExportSelectedCashReceiptType);
                    ExitState = "ECO_LNK_TO_COLLECT_DET_SCRN";

                    return;
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCashReceipt,
                      "checkType");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field2.Error = true;

                    ExitState = "FN0000_INVALID_CHECK_TYPE_4_COL";

                    return;
                  }
                }

                break;
              case "CRDI":
                // 11/17/99  PPhinney  H00080433
                // - - ADD Flow to CRDI so a Receipt can be linked
                // - -  to it's Deposit.
                // * * * * * * * * * * * * * * * * * * * *
                // ************************************************
                // *            Forward a Cash Receipt            *
                // *Move the selected entry to the select fields  *
                // *and link to Forward Cash Receipt screen(CRFO).*
                // ************************************************
                // *********************************************************
                // * Add edit to only allow receipts in balanced or forward
                // * status to be
                // forwarded to CRFO screen.
                // 
                // *********************************************************
                if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
                {
                  if (Equal(export.Export1.Item.DetailCashReceiptStatus.Code,
                    "DEP O") || Equal
                    (export.Export1.Item.DetailCashReceiptStatus.Code, "DEP C"))
                  {
                    MoveCashReceipt1(export.Export1.Item.DetailCashReceipt,
                      export.HiddenExportSelectedCashReceipt);

                    if (ReadFundTransaction())
                    {
                      export.Selected.DepositNumber =
                        entities.FundTransaction.DepositNumber;
                    }

                    if (export.Selected.DepositNumber.GetValueOrDefault() == 0)
                    {
                      var field1 =
                        GetField(export.Export1.Item.DetailCashReceiptStatus,
                        "code");

                      field1.Error = true;

                      var field2 =
                        GetField(export.Export1.Item.DetailCommon, "selectChar");
                        

                      field2.Error = true;

                      ExitState = "FN0000_FUND_TRANS_NF_RB";

                      return;
                    }

                    ExitState = "ECO_LNK_LST_DEPOSITS";

                    return;
                  }
                  else
                  {
                    var field1 =
                      GetField(export.Export1.Item.DetailCashReceiptStatus,
                      "code");

                    field1.Error = true;

                    var field2 =
                      GetField(export.Export1.Item.DetailCommon, "selectChar");

                    field2.Error = true;

                    ExitState = "FN0000_INVALD_STAT_4_REQ_ACT_RB";

                    return;
                  }
                }

                break;
              default:
                break;
            }
          }

          // *****************************************************************
          // If a rollback has occurred, change any '*' back to 'S'.
          // *****************************************************************
          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == '*')
              {
                export.Export1.Update.DetailCommon.SelectChar = "S";
              }
            }

            return;
          }

          UseFnCabListCashReceipt();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (export.Export1.IsEmpty)
            {
              export.HiddenExportDisplayed.Flag = "N";
              ExitState = "FN0000_NO_CASH_RECEIPTS_FOUND";
            }
            else
            {
              export.HiddenExportDisplayed.Flag = "Y";
            }
          }
          else if (IsExitState("FN0000_CASH_RCPT_SOURCE_TYPE_NF"))
          {
            var field = GetField(export.CashReceiptSourceType, "code");

            field.Error = true;
          }
          else if (IsExitState("FN0108_CASH_RCPT_STAT_NF"))
          {
            var field = GetField(export.CashReceiptStatus, "code");

            field.Error = true;
          }
          else
          {
          }
        }
        else if (Equal(global.Command, "ENTER"))
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

  private static void MoveCashReceipt1(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.ReceiptDate = source.ReceiptDate;
    target.CheckType = source.CheckType;
    target.CheckNumber = source.CheckNumber;
    target.CreatedBy = source.CreatedBy;
  }

  private static void MoveCashReceipt2(CashReceipt source, CashReceipt target)
  {
    target.CheckType = source.CheckType;
    target.CreatedBy = source.CreatedBy;
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
    target.Flag = source.Flag;
    target.SelectChar = source.SelectChar;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveExport2(FnCabListCashReceipt.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCommon(source.DetailCommon, target.DetailCommon);
    MoveCashReceiptSourceType(source.DetailCashReceiptSourceType,
      target.DetailCashReceiptSourceType);
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.DetailCashReceiptStatus.Code = source.DetailCashReceiptStatus.Code;
    target.DetailHidden.SystemGeneratedIdentifier =
      source.DetailHiddenCashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptType(source.DetailHiddenCashReceiptType,
      target.DetailCashReceiptType);
  }

  private static void MoveExport3(FnCabYlistCashReceipt.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCommon(source.DetailCommon, target.DetailCommon);
    MoveCashReceiptSourceType(source.DetailCashReceiptSourceType,
      target.DetailCashReceiptSourceType);
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.DetailCashReceiptStatus.Code = source.DetailCashReceiptStatus.Code;
    target.DetailHidden.SystemGeneratedIdentifier =
      source.DetailHiddenCashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptType(source.DetailHiddenCashReceiptType,
      target.DetailCashReceiptType);
  }

  private static void MoveExport4(FnCabNlistCashReceipt.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    MoveCommon(source.DetailCommon, target.DetailCommon);
    MoveCashReceiptSourceType(source.DetailCashReceiptSourceType,
      target.DetailCashReceiptSourceType);
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.DetailCashReceiptStatus.Code = source.DetailCashReceiptStatus.Code;
    target.DetailHidden.SystemGeneratedIdentifier =
      source.DetailHiddenCashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptType(source.DetailHiddenCashReceiptType,
      target.DetailCashReceiptType);
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

    useImport.DateWorkArea.Date = local.Null1.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Pass.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.Common.Count = useExport.ReturnCode.Count;
  }

  private void UseFnCabListCashReceipt()
  {
    var useImport = new FnCabListCashReceipt.Import();
    var useExport = new FnCabListCashReceipt.Export();

    useImport.CashReceiptSourceType.Code = import.CashReceiptSourceType.Code;
    MoveCashReceipt2(import.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptStatus.Code = import.CashReceiptStatus.Code;
    MoveDateWorkArea(import.From, useImport.From);
    MoveDateWorkArea(import.Thru, useImport.Thru);

    Call(FnCabListCashReceipt.Execute, useImport, useExport);

    export.TotalScreenAmount.TotalCurrency =
      useExport.TotalScreenAmount.TotalCurrency;
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
  }

  private void UseFnCabNlistCashReceipt()
  {
    var useImport = new FnCabNlistCashReceipt.Import();
    var useExport = new FnCabNlistCashReceipt.Export();

    useImport.Starting.Assign(export.Starting);
    MoveCashReceipt2(export.CashReceipt, useImport.FilterCashReceipt);
    useImport.FilterCashReceiptStatus.Code = import.CashReceiptStatus.Code;
    useImport.FilterCashReceiptSourceType.Code =
      import.CashReceiptSourceType.Code;
    MoveDateWorkArea(export.From, useImport.From);
    MoveDateWorkArea(export.Thru, useImport.Thru);

    Call(FnCabNlistCashReceipt.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport4);
    export.TotalScreenAmount.TotalCurrency =
      useExport.TotalScreenAmount.TotalCurrency;
  }

  private void UseFnCabYlistCashReceipt()
  {
    var useImport = new FnCabYlistCashReceipt.Import();
    var useExport = new FnCabYlistCashReceipt.Export();

    useImport.Starting.Assign(export.Starting);
    MoveCashReceipt2(export.CashReceipt, useImport.CashReceipt);
    useImport.CashReceiptStatus.Code = import.CashReceiptStatus.Code;
    useImport.CashReceiptSourceType.Code = import.CashReceiptSourceType.Code;
    MoveDateWorkArea(export.From, useImport.From);
    MoveDateWorkArea(export.Thru, useImport.Thru);

    Call(FnCabYlistCashReceipt.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport3);
    export.TotalScreenAmount.TotalCurrency =
      useExport.TotalScreenAmount.TotalCurrency;
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

  private bool ReadFundTransaction()
  {
    entities.FundTransaction.Populated = false;

    return Read("ReadFundTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId",
          export.HiddenExportSelectedCashReceipt.SequentialNumber);
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
        entities.FundTransaction.Populated = true;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
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
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptStatus")]
      public CashReceiptStatus DetailCashReceiptStatus
      {
        get => detailCashReceiptStatus ??= new();
        set => detailCashReceiptStatus = value;
      }

      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public CashReceiptEvent DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
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
      private CashReceiptSourceType detailCashReceiptSourceType;
      private CashReceipt detailCashReceipt;
      private CashReceiptStatus detailCashReceiptStatus;
      private CashReceiptEvent detailHidden;
      private CashReceiptType detailCashReceiptType;
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
    /// A value of HiddenCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptStatus")]
    public CashReceiptStatus HiddenCashReceiptStatus
    {
      get => hiddenCashReceiptStatus ??= new();
      set => hiddenCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of TotalScreenAmount.
    /// </summary>
    [JsonPropertyName("totalScreenAmount")]
    public Common TotalScreenAmount
    {
      get => totalScreenAmount ??= new();
      set => totalScreenAmount = value;
    }

    /// <summary>
    /// A value of CheckType.
    /// </summary>
    [JsonPropertyName("checkType")]
    public Standard CheckType
    {
      get => checkType ??= new();
      set => checkType = value;
    }

    /// <summary>
    /// A value of HiddenFirstTime.
    /// </summary>
    [JsonPropertyName("hiddenFirstTime")]
    public Common HiddenFirstTime
    {
      get => hiddenFirstTime ??= new();
      set => hiddenFirstTime = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceipt Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
    }

    /// <summary>
    /// A value of PromptStatusCode.
    /// </summary>
    [JsonPropertyName("promptStatusCode")]
    public Common PromptStatusCode
    {
      get => promptStatusCode ??= new();
      set => promptStatusCode = value;
    }

    /// <summary>
    /// A value of HiddenImportDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenImportDisplayed")]
    public Common HiddenImportDisplayed
    {
      get => hiddenImportDisplayed ??= new();
      set => hiddenImportDisplayed = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Thru.
    /// </summary>
    [JsonPropertyName("thru")]
    public DateWorkArea Thru
    {
      get => thru ??= new();
      set => thru = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public CodeValue Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of PassFrom.
    /// </summary>
    [JsonPropertyName("passFrom")]
    public DateWorkArea PassFrom
    {
      get => passFrom ??= new();
      set => passFrom = value;
    }

    /// <summary>
    /// A value of PassThru.
    /// </summary>
    [JsonPropertyName("passThru")]
    public DateWorkArea PassThru
    {
      get => passThru ??= new();
      set => passThru = value;
    }

    /// <summary>
    /// A value of PassStarting.
    /// </summary>
    [JsonPropertyName("passStarting")]
    public CashReceipt PassStarting
    {
      get => passStarting ??= new();
      set => passStarting = value;
    }

    /// <summary>
    /// A value of PassLinkFlag.
    /// </summary>
    [JsonPropertyName("passLinkFlag")]
    public Common PassLinkFlag
    {
      get => passLinkFlag ??= new();
      set => passLinkFlag = value;
    }

    private Common reipInd;
    private CashReceiptStatus hiddenCashReceiptStatus;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private Common totalScreenAmount;
    private Standard checkType;
    private Common hiddenFirstTime;
    private CashReceipt starting;
    private Common promptSourceCode;
    private Common promptStatusCode;
    private Common hiddenImportDisplayed;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private DateWorkArea from;
    private DateWorkArea thru;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CodeValue selected;
    private DateWorkArea passFrom;
    private DateWorkArea passThru;
    private CashReceipt passStarting;
    private Common passLinkFlag;
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
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
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
      /// A value of DetailCashReceiptStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptStatus")]
      public CashReceiptStatus DetailCashReceiptStatus
      {
        get => detailCashReceiptStatus ??= new();
        set => detailCashReceiptStatus = value;
      }

      /// <summary>
      /// A value of DetailHidden.
      /// </summary>
      [JsonPropertyName("detailHidden")]
      public CashReceiptEvent DetailHidden
      {
        get => detailHidden ??= new();
        set => detailHidden = value;
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
      private CashReceiptSourceType detailCashReceiptSourceType;
      private CashReceipt detailCashReceipt;
      private CashReceiptStatus detailCashReceiptStatus;
      private CashReceiptEvent detailHidden;
      private CashReceiptType detailCashReceiptType;
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
    /// A value of HiddenCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptStatus")]
    public CashReceiptStatus HiddenCashReceiptStatus
    {
      get => hiddenCashReceiptStatus ??= new();
      set => hiddenCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HiddenCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptSourceType")]
    public CashReceiptSourceType HiddenCashReceiptSourceType
    {
      get => hiddenCashReceiptSourceType ??= new();
      set => hiddenCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of TotalScreenAmount.
    /// </summary>
    [JsonPropertyName("totalScreenAmount")]
    public Common TotalScreenAmount
    {
      get => totalScreenAmount ??= new();
      set => totalScreenAmount = value;
    }

    /// <summary>
    /// A value of CheckType.
    /// </summary>
    [JsonPropertyName("checkType")]
    public Standard CheckType
    {
      get => checkType ??= new();
      set => checkType = value;
    }

    /// <summary>
    /// A value of HiddenFirstTimeIn.
    /// </summary>
    [JsonPropertyName("hiddenFirstTimeIn")]
    public Common HiddenFirstTimeIn
    {
      get => hiddenFirstTimeIn ??= new();
      set => hiddenFirstTimeIn = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CashReceipt Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PromptSourceCode.
    /// </summary>
    [JsonPropertyName("promptSourceCode")]
    public Common PromptSourceCode
    {
      get => promptSourceCode ??= new();
      set => promptSourceCode = value;
    }

    /// <summary>
    /// A value of PromptStatusCode.
    /// </summary>
    [JsonPropertyName("promptStatusCode")]
    public Common PromptStatusCode
    {
      get => promptStatusCode ??= new();
      set => promptStatusCode = value;
    }

    /// <summary>
    /// A value of HiddenExportDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenExportDisplayed")]
    public Common HiddenExportDisplayed
    {
      get => hiddenExportDisplayed ??= new();
      set => hiddenExportDisplayed = value;
    }

    /// <summary>
    /// A value of HiddenExportSelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("hiddenExportSelectedCashReceiptSourceType")]
    public CashReceiptSourceType HiddenExportSelectedCashReceiptSourceType
    {
      get => hiddenExportSelectedCashReceiptSourceType ??= new();
      set => hiddenExportSelectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of HiddenExportSelectedCashReceipt.
    /// </summary>
    [JsonPropertyName("hiddenExportSelectedCashReceipt")]
    public CashReceipt HiddenExportSelectedCashReceipt
    {
      get => hiddenExportSelectedCashReceipt ??= new();
      set => hiddenExportSelectedCashReceipt = value;
    }

    /// <summary>
    /// A value of HiddenExportSelectedCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("hiddenExportSelectedCashReceiptStatus")]
    public CashReceiptStatus HiddenExportSelectedCashReceiptStatus
    {
      get => hiddenExportSelectedCashReceiptStatus ??= new();
      set => hiddenExportSelectedCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of HiddenExportSelectedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenExportSelectedCashReceiptEvent")]
    public CashReceiptEvent HiddenExportSelectedCashReceiptEvent
    {
      get => hiddenExportSelectedCashReceiptEvent ??= new();
      set => hiddenExportSelectedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of HiddenExportSelectedCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenExportSelectedCashReceiptType")]
    public CashReceiptType HiddenExportSelectedCashReceiptType
    {
      get => hiddenExportSelectedCashReceiptType ??= new();
      set => hiddenExportSelectedCashReceiptType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of Thru.
    /// </summary>
    [JsonPropertyName("thru")]
    public DateWorkArea Thru
    {
      get => thru ??= new();
      set => thru = value;
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
    /// A value of PassFrom.
    /// </summary>
    [JsonPropertyName("passFrom")]
    public DateWorkArea PassFrom
    {
      get => passFrom ??= new();
      set => passFrom = value;
    }

    /// <summary>
    /// A value of PassThru.
    /// </summary>
    [JsonPropertyName("passThru")]
    public DateWorkArea PassThru
    {
      get => passThru ??= new();
      set => passThru = value;
    }

    /// <summary>
    /// A value of PassStarting.
    /// </summary>
    [JsonPropertyName("passStarting")]
    public CashReceipt PassStarting
    {
      get => passStarting ??= new();
      set => passStarting = value;
    }

    /// <summary>
    /// A value of PassLinkFlag.
    /// </summary>
    [JsonPropertyName("passLinkFlag")]
    public Common PassLinkFlag
    {
      get => passLinkFlag ??= new();
      set => passLinkFlag = value;
    }

    /// <summary>
    /// A value of SetPassLinkFlag.
    /// </summary>
    [JsonPropertyName("setPassLinkFlag")]
    public Common SetPassLinkFlag
    {
      get => setPassLinkFlag ??= new();
      set => setPassLinkFlag = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public FundTransaction Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    private Common reipInd;
    private CashReceiptStatus hiddenCashReceiptStatus;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private Common totalScreenAmount;
    private Standard checkType;
    private Common hiddenFirstTimeIn;
    private CashReceipt starting;
    private Common promptSourceCode;
    private Common promptStatusCode;
    private Common hiddenExportDisplayed;
    private CashReceiptSourceType hiddenExportSelectedCashReceiptSourceType;
    private CashReceipt hiddenExportSelectedCashReceipt;
    private CashReceiptStatus hiddenExportSelectedCashReceiptStatus;
    private CashReceiptEvent hiddenExportSelectedCashReceiptEvent;
    private CashReceiptType hiddenExportSelectedCashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptStatus cashReceiptStatus;
    private DateWorkArea from;
    private DateWorkArea thru;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Code pass;
    private DateWorkArea passFrom;
    private DateWorkArea passThru;
    private CashReceipt passStarting;
    private Common passLinkFlag;
    private Common setPassLinkFlag;
    private FundTransaction selected;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A TempGroup group.</summary>
    [Serializable]
    public class TempGroup
    {
      /// <summary>
      /// A value of TempCommon.
      /// </summary>
      [JsonPropertyName("tempCommon")]
      public Common TempCommon
      {
        get => tempCommon ??= new();
        set => tempCommon = value;
      }

      /// <summary>
      /// A value of TempCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("tempCashReceiptSourceType")]
      public CashReceiptSourceType TempCashReceiptSourceType
      {
        get => tempCashReceiptSourceType ??= new();
        set => tempCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of TempCashReceipt.
      /// </summary>
      [JsonPropertyName("tempCashReceipt")]
      public CashReceipt TempCashReceipt
      {
        get => tempCashReceipt ??= new();
        set => tempCashReceipt = value;
      }

      /// <summary>
      /// A value of TempCashReceiptStatus.
      /// </summary>
      [JsonPropertyName("tempCashReceiptStatus")]
      public CashReceiptStatus TempCashReceiptStatus
      {
        get => tempCashReceiptStatus ??= new();
        set => tempCashReceiptStatus = value;
      }

      /// <summary>
      /// A value of TempCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("tempCashReceiptEvent")]
      public CashReceiptEvent TempCashReceiptEvent
      {
        get => tempCashReceiptEvent ??= new();
        set => tempCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of TempCashReceiptType.
      /// </summary>
      [JsonPropertyName("tempCashReceiptType")]
      public CashReceiptType TempCashReceiptType
      {
        get => tempCashReceiptType ??= new();
        set => tempCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 120;

      private Common tempCommon;
      private CashReceiptSourceType tempCashReceiptSourceType;
      private CashReceipt tempCashReceipt;
      private CashReceiptStatus tempCashReceiptStatus;
      private CashReceiptEvent tempCashReceiptEvent;
      private CashReceiptType tempCashReceiptType;
    }

    /// <summary>
    /// A value of TotalScreenAmount.
    /// </summary>
    [JsonPropertyName("totalScreenAmount")]
    public Common TotalScreenAmount
    {
      get => totalScreenAmount ??= new();
      set => totalScreenAmount = value;
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
    /// Gets a value of Temp.
    /// </summary>
    [JsonIgnore]
    public Array<TempGroup> Temp => temp ??= new(TempGroup.Capacity);

    /// <summary>
    /// Gets a value of Temp for json serialization.
    /// </summary>
    [JsonPropertyName("temp")]
    [Computed]
    public IList<TempGroup> Temp_Json
    {
      get => temp;
      set => Temp.Assign(value);
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of SelectFound.
    /// </summary>
    [JsonPropertyName("selectFound")]
    public Common SelectFound
    {
      get => selectFound ??= new();
      set => selectFound = value;
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
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
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

    private Common totalScreenAmount;
    private Common reipInd;
    private Array<TempGroup> temp;
    private Common prompt;
    private DateWorkArea null1;
    private Common counter;
    private CashReceipt cashReceipt;
    private Common selectFound;
    private Code pass;
    private CodeValue codeValue;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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
    /// A value of FundTransaction.
    /// </summary>
    [JsonPropertyName("fundTransaction")]
    public FundTransaction FundTransaction
    {
      get => fundTransaction ??= new();
      set => fundTransaction = value;
    }

    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private FundTransaction fundTransaction;
  }
#endregion
}

// Program: FN_CRRL_LIST_REFUNDS, ID: 371879607, model: 746.
// Short name: SWECRRLP
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
/// A program: FN_CRRL_LIST_REFUNDS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrrlListRefunds: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRRL_LIST_REFUNDS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrrlListRefunds(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrrlListRefunds.
  /// </summary>
  public FnCrrlListRefunds(IContext context, Import import, Export export):
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
    // *****
    // NOTE: This screen is designed to list refunds that have been made
    //       for case receipt detail but have not yet been dispersed.
    // *****
    // -------------------------------------------------------------------
    // Date	  Developer	Description
    // 12/07/95  H. Kennedy  Source
    // 
    // 02/08/96  H. Kennedy  Changed display code to display all Receipt Refunds
    // if the proper flag is sent.
    // 
    // 02/08/96  H. Kennedy  Retrofits
    // 12/02/96  R. Marchman Add new security and next tran
    // 2/7/1997  R. Welborn  Several fixes from previous development.
    // 11/11/97  Ski         Added Worker ID as a search value on the screen and
    // in the read in this procedure. Added logic to enable the user to search
    // on a partial name in the Payee Name field.  Fix scrolling problem on
    // screen by setting the implicit scrolling options on.
    // 
    // 11/19/97  S. Newman   Increased group size to 96 to allow for a full page
    // view.  Added default Receipt From Date and Receipt To Date.  Allowed
    // display after next tran to CRRL.  Allowed display upon returning from
    // CRSL.  Increased Receipt Number size and changed Payee Name Literal on
    // screen.  Also, allowed original source filter not to be erased upon
    // return from CRSL if no source was chosen.
    // 
    // 3/26/99   S. Newman   Rewrote read statement to allow for partial
    // adjustments from FDSO.  They were not being displayed because they did
    // not have a payment request
    // 12/6/99  Sunya Sharp  Make changes per PR# 78062.  Add two filters.  One 
    // for CSE Person and one to display only misc. refunds.
    // 02/03/00  Paul Phinney  H00086127 - Modify code for reading refunds by 
    // Source-Type.
    // 4/28/00 - bud adams  -  PR# 93001: Performance fix; details
    //   listed below.
    // 05/04/2001	E.Shirk	PR118005
    // Discontinued the moving of the entity view for payment request when the 
    // payment request was not found.   This was incorrectly moving the values
    // for the previously found payment request to the export view displayed on
    // the screen.
    // 11/20/01	Kalpesh Doshi	WR020147 - KPC Recoupment. Add new flag to list.
    // 07/10/02	Paul  Phinney	PR0149921 - Add NEW filters for Receipt Number and
    // Warrant number.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    // *****
    // Move Imports to Exports
    // *****
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PromptCsePerson.PromptField = import.PromptCsePerson.PromptField;
    MoveCashReceiptDetail(import.CashReceiptDetail, export.CashReceiptDetail);
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    MoveReceiptRefund(import.StartKey, export.StartKey);
    export.AaScrollingFields.Assign(import.AaScrollingFields);
    export.Previous.Assign(import.Previous);
    export.CollectionType.Code = import.CollectionType.Code;
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptSourceType(import.HiddenCashReceiptSourceType,
      export.HiddenCashReceiptSourceType);
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.DisplayAll.Flag = import.DisplayAll.Flag;
    export.From.Date = import.From.Date;
    export.SelectedCashReceiptSourceType.Code =
      import.SelectedCashReceiptSourceType.Code;
    export.SelectedCsePerson.Number = import.SelectedCsePerson.Number;
    export.Source.PromptField = import.Source.PromptField;
    export.To.Date = import.To.Date;
    export.PromptCsePerson.PromptField = import.PromptCsePerson.PromptField;
    export.HiddenFlowedFrom.Text4 = import.HiddenFlowedFrom.Text4;
    export.Selection.Assign(import.Selection);
    export.DisplayMiscOnly.Flag = import.DisplayMiscOnly.Flag;
    local.MaxSystemDate.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);

    // 07/10/02	Paul  Phinney	PR0149921 - Add NEW filters for Receipt Number and
    // Warrant number.
    export.SortByCashReceipt.SequentialNumber =
      import.SortByCashReceipt.SequentialNumber;
    export.SortByCashReceiptDetail.SequentialIdentifier =
      import.SortByCashReceiptDetail.SequentialIdentifier;
    MovePaymentRequest(import.SortByPaymentRequest, export.SortByPaymentRequest);
      

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

        export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
          import.Import1.Item.DetailCrdCrComboNo.CrdCrCombo;
        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailPaymentRequest.Assign(
          import.Import1.Item.DetailPaymentRequest);
        export.Export1.Update.DetailReceiptRefund.Assign(
          import.Import1.Item.DetailReceiptRefund);
        export.Export1.Update.Hidden.Number = import.Import1.Item.Hidden.Number;
        export.Export1.Update.HiddenMiscUndis.Flag =
          import.Import1.Item.HiddenMiscUndis.Flag;
        MovePaymentStatus(import.Import1.Item.DetailPaymentStatus,
          export.Export1.Update.DetailPaymentStatus);

        switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
        {
          case 'S':
            export.PassTypeOfRefund.Flag =
              export.Export1.Item.HiddenMiscUndis.Flag;
            export.PassReceiptRefund.Assign(
              export.Export1.Item.DetailReceiptRefund);
            export.PassCsePersonsWorkSet.Number =
              export.Export1.Item.Hidden.Number;
            ++local.Common.Count;

            break;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
        }

        export.Export1.Next();
      }
    }

    // *** PR# 78062  Add logic to default the new flag.  Sunya Sharp 12/6/1999 
    // ***
    switch(AsChar(export.DisplayMiscOnly.Flag))
    {
      case 'N':
        // *** Value ok ***
        break;
      case 'Y':
        // *** Value ok ***
        break;
      case ' ':
        export.DisplayMiscOnly.Flag = "N";

        break;
      default:
        var field = GetField(export.DisplayMiscOnly, "flag");

        field.Error = true;

        ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

        return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (Equal(global.Command, "RETURN") || Equal(global.Command, "RMSR") || Equal
      (global.Command, "CRRU"))
    {
      if (local.Common.Count > 1)
      {
        ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

        return;
      }
    }

    // *****
    // Move hidden Imports to hidden Exports
    // *****
    export.Previous.Assign(import.Previous);

    if (Equal(global.Command, "FROMRMSR"))
    {
      export.DisplayAll.Flag = "Y";
      global.Command = "DISPLAY";
    }

    // *** PR#78062  Added logic for prompt to NAME for the new cse person 
    // filter.  Sunya Sharp 12/7/1999 ***
    if (Equal(global.Command, "RETNAME"))
    {
      if (!IsEmpty(import.Pass.Number))
      {
        export.SelectedCsePerson.Number = import.Pass.Number;
        global.Command = "DISPLAY";
      }
      else
      {
        return;
      }
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
      // *****This is where you would set the local next_tran_info attributes to
      // the import view attributes for the data to be passed to the next
      // transaction.*****
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();

      // 02/03/00  Paul Phinney  H00086127 - Per Lori Glissman - no longer do a 
      // display - allow user to enter filters and then display.
      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      global.Command = "DISPLAY";
    }

    if (local.Common.Count > 0 && (Equal(global.Command, "PREV") || Equal
      (global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (Equal(global.Command, "LIST") || Equal(global.Command, "CRRD") || Equal
      (global.Command, "RETCRSL"))
    {
    }
    else
    {
      // to validate action level security
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // *****
    // Validate PF Keys
    // *****
    if (Equal(global.Command, "RETCRSL"))
    {
      if (!IsEmpty(import.SelectedCashReceiptSourceType.Code))
      {
        export.SelectedCashReceiptSourceType.Code =
          import.SelectedCashReceiptSourceType.Code;
      }
      else
      {
        export.SelectedCashReceiptSourceType.Code =
          export.HiddenCashReceiptSourceType.Code;
      }

      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "RMSR":
        if (AsChar(export.PassTypeOfRefund.Flag) == 'U')
        {
          ExitState = "FN0000_UNDISTRB_COLL_REF_SELECTD";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_RMSR";

        break;
      case "CRRU":
        if (AsChar(export.PassTypeOfRefund.Flag) == 'M')
        {
          ExitState = "FN0000_FN_MISC_REFUND_SELECTED";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_REFUND_COLLECTION";

        break;
      case "":
        break;
      case "HELP":
        break;
      case "DISPLAY":
        // *** PR# 78062  Do not allow both the source code and person number to
        // be selected.  Sunya Sharp 12/6/1999 ***
        if (!IsEmpty(export.SelectedCashReceiptSourceType.Code) && !
          IsEmpty(export.SelectedCsePerson.Number))
        {
          var field1 = GetField(export.SelectedCashReceiptSourceType, "code");

          field1.Error = true;

          var field2 = GetField(export.SelectedCsePerson, "number");

          field2.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

          return;
        }

        // 07/10/02	Paul  Phinney	PR0149921 - Add NEW filters for Receipt Number
        // and Warrant number.
        if (export.SortByCashReceipt.SequentialNumber == 0)
        {
          export.SortByCashReceiptDetail.SequentialIdentifier = 0;
        }

        if (export.SortByCashReceipt.SequentialNumber > 0 && !
          IsEmpty(export.SortByPaymentRequest.Number))
        {
          var field1 = GetField(export.SortByCashReceipt, "sequentialNumber");

          field1.Error = true;

          var field2 = GetField(export.SortByPaymentRequest, "number");

          field2.Error = true;

          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        if (AsChar(export.DisplayMiscOnly.Flag) == 'Y' && export
          .SortByCashReceipt.SequentialNumber > 0)
        {
          var field1 = GetField(export.DisplayMiscOnly, "flag");

          field1.Error = true;

          var field2 = GetField(export.SortByCashReceipt, "sequentialNumber");

          field2.Error = true;

          ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

          return;
        }

        if (!IsEmpty(export.SortByPaymentRequest.Number))
        {
          local.CseNumber.Text10 = export.SortByPaymentRequest.Number ?? Spaces
            (10);
          UseEabPadLeftWithZeros();
          export.SortByPaymentRequest.Number =
            Substring(local.CseNumber.Text10, 2, 9);
        }

        // ****************************************************************
        // The following IF statement will set the filter for a Payee Name
        // and will give any like names.  (I.E. If the user enters "KA" the
        // system will return both "KATHY" and "KATE", but will not return
        // "KEVIN".)
        // Skip Hardy  MTW  11/11/1997
        // ****************************************************************
        if (!IsEmpty(export.Selection.PayeeName))
        {
          local.SelectionBeginSearchVal.PayeeName =
            TrimEnd(export.Selection.PayeeName) + "%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%"
            ;
        }

        // *** PR#78062  Add logic to left pad the CSE Person number if entered.
        // Sunya Sharp 12/7/1999 ***
        if (!IsEmpty(export.SelectedCsePerson.Number))
        {
          local.CseNumber.Text10 = export.SelectedCsePerson.Number;
          UseEabPadLeftWithZeros();
          export.SelectedCsePerson.Number = local.CseNumber.Text10;
        }

        // *** PR# 78062  Per user request, set the date range to blanks if the 
        // person number or taxid is entered.  Sunya Sharp 12/6/1999 ***
        // 07/10/02	Paul  Phinney	PR0149921 - Remove Date range reset
        if (Equal(export.From.Date, local.BlankDateWorkArea.Date))
        {
          export.From.Date = AddMonths(local.Current.Date, -1);
        }

        if (Equal(export.To.Date, local.BlankDateWorkArea.Date))
        {
          export.To.Date = local.Current.Date;
        }

        // ===============================================
        // 4/28/00 - bud adams  -  PR# 93001: Performance.  Remove
        //   OR conditions in the READ EACH action.
        //   Whenever the CSE_Person filter is used, up to 16,000
        //   Receipt_Refund rows may be retrieved - and as many
        //   attempts to Read CSE_Person will be executed until all the
        //   right ones are found.  That number will increase.  So, have
        //   two different read each actions and avoid all that overhead.
        // ===============================================
        if (Equal(export.To.Date, local.BlankDateWorkArea.Date))
        {
          local.To.Date = local.Max.Date;
        }
        else
        {
          local.To.Date = export.To.Date;
        }

        // 07/10/02	Paul  Phinney	PR0149921 - Add NEW filters for Receipt Number
        // and Warrant number.
        // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        if (!IsEmpty(export.SelectedCsePerson.Number) && export
          .SortByCashReceipt.SequentialNumber > 0)
        {
          if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
          {
            goto Test;
          }

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefundCsePersonCashReceiptCashReceiptDetail())
            
          {
            if (entities.CashReceipt.SequentialNumber == export
              .SortByCashReceipt.SequentialNumber)
            {
              if (entities.CashReceiptDetail.SequentialIdentifier < export
                .SortByCashReceiptDetail.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            export.Export1.Update.HiddenMiscUndis.Flag = "U";
            local.CashReceipt1.Text8 =
              NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
            local.CashReceipt1.Text8 =
              Substring(local.CashReceipt1.Text8, TextWorkArea.Text8_MaxLength,
              1, 7) + "-";
            local.CrDetail.Text4 =
              NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
              12, 15);
            export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
              local.CashReceipt1.Text8 + local.CrDetail.Text4;

            if (ReadPaymentRequest())
            {
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (ReadPaymentStatusHistoryPaymentStatus())
              {
                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.PaymentStatus.Code;
              }
            }
            else
            {
              // **** Valid. Refund for FDSO partial adjustments do not have 
              // payment requests.****
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }
        }
        else if (export.SortByCashReceipt.SequentialNumber > 0)
        {
          if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
          {
            goto Test;
          }

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefundCashReceiptDetailCashReceipt())
          {
            if (entities.CashReceipt.SequentialNumber == export
              .SortByCashReceipt.SequentialNumber)
            {
              if (entities.CashReceiptDetail.SequentialIdentifier < export
                .SortByCashReceiptDetail.SequentialIdentifier)
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            export.Export1.Update.HiddenMiscUndis.Flag = "U";
            local.CashReceipt1.Text8 =
              NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
            local.CashReceipt1.Text8 =
              Substring(local.CashReceipt1.Text8, TextWorkArea.Text8_MaxLength,
              1, 7) + "-";
            local.CrDetail.Text4 =
              NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
              12, 15);
            export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
              local.CashReceipt1.Text8 + local.CrDetail.Text4;

            if (ReadPaymentRequest())
            {
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (ReadPaymentStatusHistoryPaymentStatus())
              {
                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.PaymentStatus.Code;
              }
            }
            else
            {
              // **** Valid. Refund for FDSO partial adjustments do not have 
              // payment requests.****
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.SelectedCsePerson.Number) && !
          IsEmpty(export.SortByPaymentRequest.Number))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefundCsePersonPaymentRequest())
          {
            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            if (ReadCashReceiptDetailCashReceipt())
            {
              if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.HiddenMiscUndis.Flag = "U";
              local.CashReceipt1.Text8 =
                NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
              local.CashReceipt1.Text8 =
                Substring(local.CashReceipt1.Text8,
                TextWorkArea.Text8_MaxLength, 1, 7) + "-";
              local.CrDetail.Text4 =
                NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
                12, 15);
              export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
                local.CashReceipt1.Text8 + local.CrDetail.Text4;
            }
            else
            {
              export.Export1.Update.HiddenMiscUndis.Flag = "M";
            }

            export.Export1.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (ReadPaymentStatusHistoryPaymentStatus())
            {
              export.Export1.Update.DetailPaymentStatus.Code =
                entities.PaymentStatus.Code;
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }
        }
        else if (!IsEmpty(export.SortByPaymentRequest.Number))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefundPaymentRequest())
          {
            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            if (ReadCashReceiptDetailCashReceipt())
            {
              if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.HiddenMiscUndis.Flag = "U";
              local.CashReceipt1.Text8 =
                NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
              local.CashReceipt1.Text8 =
                Substring(local.CashReceipt1.Text8,
                TextWorkArea.Text8_MaxLength, 1, 7) + "-";
              local.CrDetail.Text4 =
                NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
                12, 15);
              export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
                local.CashReceipt1.Text8 + local.CrDetail.Text4;
            }
            else
            {
              export.Export1.Update.HiddenMiscUndis.Flag = "M";
            }

            export.Export1.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (ReadPaymentStatusHistoryPaymentStatus())
            {
              export.Export1.Update.DetailPaymentStatus.Code =
                entities.PaymentStatus.Code;
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }

          // 07/10/02	Paul  Phinney	PR0149921 - Add NEW filters for Receipt 
          // Number and Warrant number.
          // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        }
        else if (!IsEmpty(export.SelectedCsePerson.Number))
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefundCsePerson())
          {
            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            if (ReadCashReceiptDetailCashReceipt())
            {
              if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.HiddenMiscUndis.Flag = "U";
              local.CashReceipt1.Text8 =
                NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
              local.CashReceipt1.Text8 =
                Substring(local.CashReceipt1.Text8,
                TextWorkArea.Text8_MaxLength, 1, 7) + "-";
              local.CrDetail.Text4 =
                NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
                12, 15);
              export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
                local.CashReceipt1.Text8 + local.CrDetail.Text4;
            }
            else
            {
              export.Export1.Update.HiddenMiscUndis.Flag = "M";
            }

            if (ReadPaymentRequest())
            {
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (ReadPaymentStatusHistoryPaymentStatus())
              {
                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.PaymentStatus.Code;
              }
            }
            else
            {
              // **** Valid. Refund for FDSO partial adjustments do not have 
              // payment requests.****
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }
        }
        else
        {
          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadReceiptRefund())
          {
            if (!IsEmpty(export.Selection.Taxid))
            {
              if (!Equal(entities.ReceiptRefund.Taxid, export.Selection.Taxid))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.Selection.CreatedBy))
            {
              if (!Equal(entities.ReceiptRefund.CreatedBy,
                export.Selection.CreatedBy))
              {
                export.Export1.Next();

                continue;
              }
            }

            if (!IsEmpty(export.SelectedCashReceiptSourceType.Code))
            {
              if (ReadCashReceiptSourceType1())
              {
                // * continue *
              }
              else
              {
                // 02/03/00  Paul Phinney  H00086127 - Modify code for reading 
                // refunds by Source-Type.
                if (ReadCashReceiptSourceType2())
                {
                  // * continue *
                }
                else
                {
                  export.Export1.Next();

                  continue;
                }
              }
            }

            if (ReadCashReceiptDetailCashReceipt())
            {
              if (AsChar(export.DisplayMiscOnly.Flag) == 'Y')
              {
                export.Export1.Next();

                continue;
              }

              export.Export1.Update.HiddenMiscUndis.Flag = "U";
              local.CashReceipt1.Text8 =
                NumberToString(entities.CashReceipt.SequentialNumber, 9, 15);
              local.CashReceipt1.Text8 =
                Substring(local.CashReceipt1.Text8,
                TextWorkArea.Text8_MaxLength, 1, 7) + "-";
              local.CrDetail.Text4 =
                NumberToString(entities.CashReceiptDetail.SequentialIdentifier,
                12, 15);
              export.Export1.Update.DetailCrdCrComboNo.CrdCrCombo =
                local.CashReceipt1.Text8 + local.CrDetail.Text4;
            }
            else
            {
              export.Export1.Update.HiddenMiscUndis.Flag = "M";
            }

            if (ReadPaymentRequest())
            {
              export.Export1.Update.DetailPaymentRequest.Assign(
                entities.PaymentRequest);

              if (ReadPaymentStatusHistoryPaymentStatus())
              {
                export.Export1.Update.DetailPaymentStatus.Code =
                  entities.PaymentStatus.Code;
              }
            }
            else
            {
              // **** Valid. Refund for FDSO partial adjustments do not have 
              // payment requests.****
            }

            export.Export1.Update.DetailReceiptRefund.Assign(
              entities.ReceiptRefund);
            export.Export1.Update.Hidden.Number = entities.CsePerson.Number;
            export.Export1.Next();
          }
        }

Test:

        if (!export.Export1.IsEmpty)
        {
          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else
        {
          ExitState = "FN0018_REFUND_TRANS_NF";
        }

        if (Equal(export.To.Date, local.Max.Date))
        {
          export.To.Date = local.BlankDateWorkArea.Date;
        }

        break;
      case "EXIT":
        // *****
        // Flow to menu still needs to be added when the Cash Management Menu is
        // developed
        // *****
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      case "LIST":
        // ****
        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        if (IsEmpty(export.PromptCsePerson.PromptField) && IsEmpty
          (export.Source.PromptField))
        {
          ExitState = "ZD_ACO_NE00_MUST_SELECT_4_PROMPT";

          var field = GetField(export.Source, "promptField");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        switch(AsChar(export.Source.PromptField))
        {
          case 'S':
            export.Source.PromptField = "";
            export.HiddenCashReceiptSourceType.Code =
              export.SelectedCashReceiptSourceType.Code;
            ExitState = "ECO_LNK_LST_CASH_SOURCES";

            return;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.Source, "promptField");

            field.Error = true;

            return;
        }

        switch(AsChar(export.PromptCsePerson.PromptField))
        {
          case 'S':
            export.PromptCsePerson.PromptField = "";
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          case ' ':
            break;
          default:
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field = GetField(export.PromptCsePerson, "promptField");

            field.Error = true;

            return;
        }

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "RETURN":
        if (Equal(export.HiddenFlowedFrom.Text4, "RMSR"))
        {
          if (AsChar(export.PassTypeOfRefund.Flag) == 'U')
          {
            ExitState = "FN0000_UNDISTRB_COLL_REF_SELECTD";

            return;
          }
        }

        if (Equal(export.HiddenFlowedFrom.Text4, "CRRU"))
        {
          if (AsChar(export.PassTypeOfRefund.Flag) == 'M')
          {
            ExitState = "FN0000_FN_MISC_REFUND_SELECTED";

            return;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "CLEAR":
        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // *****
    // If all processing completed successfully, all imports are moved to 
    // previous exports .
    // *****
    export.Previous.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
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

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MovePaymentStatus(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveReceiptRefund(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.RequestDate = source.RequestDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxSystemDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.CseNumber.Text10;
    useExport.TextWorkArea.Text10 = local.CseNumber.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.CseNumber.Text10 = useExport.TextWorkArea.Text10;
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

  private bool ReadCashReceiptDetailCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId",
          entities.ReceiptRefund.CrdIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.ReceiptRefund.CrvIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.ReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          entities.ReceiptRefund.CrtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 7);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 8);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 9);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 10);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 11);
        entities.CashReceipt.Note = db.GetNullableString(reader, 12);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.ReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.
          SetString(command, "code", export.SelectedCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.ReceiptRefund.CstAIdentifier.GetValueOrDefault());
        db.
          SetString(command, "code", export.SelectedCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Classification = db.GetString(reader, 1);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 2);
        entities.PaymentRequest.Type1 = db.GetString(reader, 3);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 4);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatusHistoryPaymentStatus()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatusHistoryPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.MaxSystemDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatus.Code = db.GetString(reader, 5);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.ReceiptRefund.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCashReceiptDetailCashReceipt()
  {
    return ReadEach("ReadReceiptRefundCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
        db.SetInt32(
          command, "cashReceiptId", export.SortByCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 10);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 12);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 13);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 13);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 17);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 18);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 19);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 20);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 21);
        entities.CashReceipt.Note = db.GetNullableString(reader, 22);
        entities.CashReceipt.Populated = true;
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCsePerson()
  {
    return ReadEach("ReadReceiptRefundCsePerson",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
        db.SetNullableString(
          command, "cspNumber", export.SelectedCsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.CsePerson.Populated = true;
        entities.ReceiptRefund.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadReceiptRefundCsePersonCashReceiptCashReceiptDetail()
  {
    return ReadEach("ReadReceiptRefundCsePersonCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
        db.SetNullableString(
          command, "cspNumber", export.SelectedCsePerson.Number);
        db.SetInt32(
          command, "cashReceiptId", export.SortByCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 13);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 14);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 14);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 15);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 15);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 16);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 16);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 17);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 18);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 19);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 20);
        entities.CashReceipt.Note = db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 25);
        entities.CashReceipt.Populated = true;
        entities.CsePerson.Populated = true;
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundCsePersonPaymentRequest()
  {
    return ReadEach("ReadReceiptRefundCsePersonPaymentRequest",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
        db.SetNullableString(
          command, "cspNumber", export.SelectedCsePerson.Number);
        db.SetNullableString(
          command, "number", export.SortByPaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentRequest.Classification = db.GetString(reader, 15);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 16);
        entities.PaymentRequest.Type1 = db.GetString(reader, 17);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 19);
        entities.CsePerson.Populated = true;
        entities.PaymentRequest.Populated = true;
        entities.ReceiptRefund.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefundPaymentRequest()
  {
    return ReadEach("ReadReceiptRefundPaymentRequest",
      (db, command) =>
      {
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "payeeName1", export.Selection.PayeeName ?? "");
        db.SetNullableString(
          command, "payeeName2", local.SelectionBeginSearchVal.PayeeName ?? ""
          );
        db.SetNullableString(
          command, "number", export.SortByPaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 2);
        entities.ReceiptRefund.PayeeName = db.GetNullableString(reader, 3);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 4);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 6);
        entities.ReceiptRefund.CreatedBy = db.GetString(reader, 7);
        entities.ReceiptRefund.CspNumber = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.CstAIdentifier = db.GetNullableInt32(reader, 9);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 13);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentRequest.Classification = db.GetString(reader, 15);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 16);
        entities.PaymentRequest.Type1 = db.GetString(reader, 17);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 19);
        entities.PaymentRequest.Populated = true;
        entities.ReceiptRefund.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
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
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of HiddenMiscUndis.
      /// </summary>
      [JsonPropertyName("hiddenMiscUndis")]
      public Common HiddenMiscUndis
      {
        get => hiddenMiscUndis ??= new();
        set => hiddenMiscUndis = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePerson Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

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
      /// A value of DetailReceiptRefund.
      /// </summary>
      [JsonPropertyName("detailReceiptRefund")]
      public ReceiptRefund DetailReceiptRefund
      {
        get => detailReceiptRefund ??= new();
        set => detailReceiptRefund = value;
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
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private PaymentStatus detailPaymentStatus;
      private Common hiddenMiscUndis;
      private CsePerson hidden;
      private Common detailCommon;
      private ReceiptRefund detailReceiptRefund;
      private CrdCrComboNo detailCrdCrComboNo;
      private PaymentRequest detailPaymentRequest;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CsePersonsWorkSet Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of DisplayMiscOnly.
    /// </summary>
    [JsonPropertyName("displayMiscOnly")]
    public Common DisplayMiscOnly
    {
      get => displayMiscOnly ??= new();
      set => displayMiscOnly = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public ReceiptRefund Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    /// <summary>
    /// A value of HiddenFlowedFrom.
    /// </summary>
    [JsonPropertyName("hiddenFlowedFrom")]
    public TextWorkArea HiddenFlowedFrom
    {
      get => hiddenFlowedFrom ??= new();
      set => hiddenFlowedFrom = value;
    }

    /// <summary>
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Standard PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptSourceType")]
    public CashReceiptSourceType SelectedCashReceiptSourceType
    {
      get => selectedCashReceiptSourceType ??= new();
      set => selectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
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
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
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

    /// <summary>
    /// A value of Source.
    /// </summary>
    [JsonPropertyName("source")]
    public Standard Source
    {
      get => source ??= new();
      set => source = value;
    }

    /// <summary>
    /// A value of StartKey.
    /// </summary>
    [JsonPropertyName("startKey")]
    public ReceiptRefund StartKey
    {
      get => startKey ??= new();
      set => startKey = value;
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
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of AaScrollingFields.
    /// </summary>
    [JsonPropertyName("aaScrollingFields")]
    public AaScrollingFields AaScrollingFields
    {
      get => aaScrollingFields ??= new();
      set => aaScrollingFields = value;
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
    /// A value of SortByPaymentRequest.
    /// </summary>
    [JsonPropertyName("sortByPaymentRequest")]
    public PaymentRequest SortByPaymentRequest
    {
      get => sortByPaymentRequest ??= new();
      set => sortByPaymentRequest = value;
    }

    /// <summary>
    /// A value of SortByCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("sortByCashReceiptDetail")]
    public CashReceiptDetail SortByCashReceiptDetail
    {
      get => sortByCashReceiptDetail ??= new();
      set => sortByCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of SortByCashReceipt.
    /// </summary>
    [JsonPropertyName("sortByCashReceipt")]
    public CashReceipt SortByCashReceipt
    {
      get => sortByCashReceipt ??= new();
      set => sortByCashReceipt = value;
    }

    private CsePersonsWorkSet pass;
    private Common displayMiscOnly;
    private CsePerson selectedCsePerson;
    private ReceiptRefund selection;
    private TextWorkArea hiddenFlowedFrom;
    private Standard promptCsePerson;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private DateWorkArea to;
    private DateWorkArea from;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CollectionType collectionType;
    private Standard source;
    private ReceiptRefund startKey;
    private Standard standard;
    private Common displayAll;
    private CashReceiptDetail previous;
    private AaScrollingFields aaScrollingFields;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private PaymentRequest sortByPaymentRequest;
    private CashReceiptDetail sortByCashReceiptDetail;
    private CashReceipt sortByCashReceipt;
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
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of HiddenMiscUndis.
      /// </summary>
      [JsonPropertyName("hiddenMiscUndis")]
      public Common HiddenMiscUndis
      {
        get => hiddenMiscUndis ??= new();
        set => hiddenMiscUndis = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CsePerson Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

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
      /// A value of DetailReceiptRefund.
      /// </summary>
      [JsonPropertyName("detailReceiptRefund")]
      public ReceiptRefund DetailReceiptRefund
      {
        get => detailReceiptRefund ??= new();
        set => detailReceiptRefund = value;
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
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 96;

      private PaymentStatus detailPaymentStatus;
      private Common hiddenMiscUndis;
      private CsePerson hidden;
      private Common detailCommon;
      private ReceiptRefund detailReceiptRefund;
      private CrdCrComboNo detailCrdCrComboNo;
      private PaymentRequest detailPaymentRequest;
    }

    /// <summary>
    /// A value of DisplayMiscOnly.
    /// </summary>
    [JsonPropertyName("displayMiscOnly")]
    public Common DisplayMiscOnly
    {
      get => displayMiscOnly ??= new();
      set => displayMiscOnly = value;
    }

    /// <summary>
    /// A value of SelectedCsePerson.
    /// </summary>
    [JsonPropertyName("selectedCsePerson")]
    public CsePerson SelectedCsePerson
    {
      get => selectedCsePerson ??= new();
      set => selectedCsePerson = value;
    }

    /// <summary>
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public ReceiptRefund Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    /// <summary>
    /// A value of HiddenFlowedFrom.
    /// </summary>
    [JsonPropertyName("hiddenFlowedFrom")]
    public TextWorkArea HiddenFlowedFrom
    {
      get => hiddenFlowedFrom ??= new();
      set => hiddenFlowedFrom = value;
    }

    /// <summary>
    /// A value of PassTypeOfRefund.
    /// </summary>
    [JsonPropertyName("passTypeOfRefund")]
    public Common PassTypeOfRefund
    {
      get => passTypeOfRefund ??= new();
      set => passTypeOfRefund = value;
    }

    /// <summary>
    /// A value of PassCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("passCsePersonsWorkSet")]
    public CsePersonsWorkSet PassCsePersonsWorkSet
    {
      get => passCsePersonsWorkSet ??= new();
      set => passCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Standard PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptSourceType")]
    public CashReceiptSourceType SelectedCashReceiptSourceType
    {
      get => selectedCashReceiptSourceType ??= new();
      set => selectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Source.
    /// </summary>
    [JsonPropertyName("source")]
    public Standard Source
    {
      get => source ??= new();
      set => source = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of HiddenCashReceiptType.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptType")]
    public CashReceiptType HiddenCashReceiptType
    {
      get => hiddenCashReceiptType ??= new();
      set => hiddenCashReceiptType = value;
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
    /// A value of HiddenCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("hiddenCashReceiptEvent")]
    public CashReceiptEvent HiddenCashReceiptEvent
    {
      get => hiddenCashReceiptEvent ??= new();
      set => hiddenCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of StartKey.
    /// </summary>
    [JsonPropertyName("startKey")]
    public ReceiptRefund StartKey
    {
      get => startKey ??= new();
      set => startKey = value;
    }

    /// <summary>
    /// A value of PassReceiptRefund.
    /// </summary>
    [JsonPropertyName("passReceiptRefund")]
    public ReceiptRefund PassReceiptRefund
    {
      get => passReceiptRefund ??= new();
      set => passReceiptRefund = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of AaScrollingFields.
    /// </summary>
    [JsonPropertyName("aaScrollingFields")]
    public AaScrollingFields AaScrollingFields
    {
      get => aaScrollingFields ??= new();
      set => aaScrollingFields = value;
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
    /// A value of SortByPaymentRequest.
    /// </summary>
    [JsonPropertyName("sortByPaymentRequest")]
    public PaymentRequest SortByPaymentRequest
    {
      get => sortByPaymentRequest ??= new();
      set => sortByPaymentRequest = value;
    }

    /// <summary>
    /// A value of SortByCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("sortByCashReceiptDetail")]
    public CashReceiptDetail SortByCashReceiptDetail
    {
      get => sortByCashReceiptDetail ??= new();
      set => sortByCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of SortByCashReceipt.
    /// </summary>
    [JsonPropertyName("sortByCashReceipt")]
    public CashReceipt SortByCashReceipt
    {
      get => sortByCashReceipt ??= new();
      set => sortByCashReceipt = value;
    }

    private Common displayMiscOnly;
    private CsePerson selectedCsePerson;
    private ReceiptRefund selection;
    private TextWorkArea hiddenFlowedFrom;
    private Common passTypeOfRefund;
    private CsePersonsWorkSet passCsePersonsWorkSet;
    private Standard promptCsePerson;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private Standard source;
    private DateWorkArea to;
    private DateWorkArea from;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType hiddenCashReceiptType;
    private CashReceiptSourceType hiddenCashReceiptSourceType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private ReceiptRefund startKey;
    private ReceiptRefund passReceiptRefund;
    private CashReceiptDetail cashReceiptDetail;
    private Standard standard;
    private Common displayAll;
    private CashReceiptDetail previous;
    private AaScrollingFields aaScrollingFields;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private PaymentRequest sortByPaymentRequest;
    private CashReceiptDetail sortByCashReceiptDetail;
    private CashReceipt sortByCashReceipt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
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
      /// A value of DetailCsePerson.
      /// </summary>
      [JsonPropertyName("detailCsePerson")]
      public CsePerson DetailCsePerson
      {
        get => detailCsePerson ??= new();
        set => detailCsePerson = value;
      }

      /// <summary>
      /// A value of DetailReceiptRefund.
      /// </summary>
      [JsonPropertyName("detailReceiptRefund")]
      public ReceiptRefund DetailReceiptRefund
      {
        get => detailReceiptRefund ??= new();
        set => detailReceiptRefund = value;
      }

      /// <summary>
      /// A value of DetailCancel.
      /// </summary>
      [JsonPropertyName("detailCancel")]
      public Common DetailCancel
      {
        get => detailCancel ??= new();
        set => detailCancel = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Common detailCommon;
      private CsePerson detailCsePerson;
      private ReceiptRefund detailReceiptRefund;
      private Common detailCancel;
      private PaymentRequest detailPaymentRequest;
      private CrdCrComboNo detailCrdCrComboNo;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
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
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
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
    /// A value of MaxSystemDate.
    /// </summary>
    [JsonPropertyName("maxSystemDate")]
    public DateWorkArea MaxSystemDate
    {
      get => maxSystemDate ??= new();
      set => maxSystemDate = value;
    }

    /// <summary>
    /// A value of SelectionBeginSearchVal.
    /// </summary>
    [JsonPropertyName("selectionBeginSearchVal")]
    public ReceiptRefund SelectionBeginSearchVal
    {
      get => selectionBeginSearchVal ??= new();
      set => selectionBeginSearchVal = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public PaymentRequest Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of BlankCrdCrComboNo.
    /// </summary>
    [JsonPropertyName("blankCrdCrComboNo")]
    public CrdCrComboNo BlankCrdCrComboNo
    {
      get => blankCrdCrComboNo ??= new();
      set => blankCrdCrComboNo = value;
    }

    /// <summary>
    /// A value of BlankReceiptRefund.
    /// </summary>
    [JsonPropertyName("blankReceiptRefund")]
    public ReceiptRefund BlankReceiptRefund
    {
      get => blankReceiptRefund ??= new();
      set => blankReceiptRefund = value;
    }

    /// <summary>
    /// A value of CrDetail.
    /// </summary>
    [JsonPropertyName("crDetail")]
    public TextWorkArea CrDetail
    {
      get => crDetail ??= new();
      set => crDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt1.
    /// </summary>
    [JsonPropertyName("cashReceipt1")]
    public TextWorkArea CashReceipt1
    {
      get => cashReceipt1 ??= new();
      set => cashReceipt1 = value;
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
    /// A value of CseNumber.
    /// </summary>
    [JsonPropertyName("cseNumber")]
    public TextWorkArea CseNumber
    {
      get => cseNumber ??= new();
      set => cseNumber = value;
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
    /// A value of CashReceipt2.
    /// </summary>
    [JsonPropertyName("cashReceipt2")]
    public CashReceipt CashReceipt2
    {
      get => cashReceipt2 ??= new();
      set => cashReceipt2 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ReceiptRefund Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public ReceiptRefund Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ReceiptRefund Null1
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
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      to = null;
      max = null;
      blankDateWorkArea = null;
      maxSystemDate = null;
      selectionBeginSearchVal = null;
      zdel = null;
      blankCrdCrComboNo = null;
      blankReceiptRefund = null;
      crDetail = null;
      cashReceipt1 = null;
      csePersonsWorkSet = null;
      cseNumber = null;
      cashReceiptSourceType = null;
      cashReceiptDetail = null;
      cashReceipt2 = null;
      prev = null;
      next = null;
      null1 = null;
      common = null;
      local1 = null;
    }

    private DateWorkArea to;
    private DateWorkArea max;
    private DateWorkArea blankDateWorkArea;
    private DateWorkArea current;
    private DateWorkArea maxSystemDate;
    private ReceiptRefund selectionBeginSearchVal;
    private PaymentRequest zdel;
    private CrdCrComboNo blankCrdCrComboNo;
    private ReceiptRefund blankReceiptRefund;
    private TextWorkArea crDetail;
    private TextWorkArea cashReceipt1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea cseNumber;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt2;
    private ReceiptRefund prev;
    private ReceiptRefund next;
    private ReceiptRefund null1;
    private Common common;
    private Array<LocalGroup> local1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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

    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CsePerson csePerson;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}

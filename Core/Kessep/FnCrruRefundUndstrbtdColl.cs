// Program: FN_CRRU_REFUND_UNDSTRBTD_COLL, ID: 372307453, model: 746.
// Short name: SWECRRUP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CRRU_REFUND_UNDSTRBTD_COLL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrruRefundUndstrbtdColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRRU_REFUND_UNDSTRBTD_COLL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrruRefundUndstrbtdColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrruRefundUndstrbtdColl.
  /// </summary>
  public FnCrruRefundUndstrbtdColl(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // This screen is designed to Create a Refund for any undistributed
    // collections.  The screen also provides the ability to update or
    // delete any currently existing Refunds.
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented.
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 12/13/95  Holly Kennedy-MTW			Source
    // 02/09/96  Holly Kennedy-MTW			Retrofits
    // 03/18/96  Holly Kennedy-MTW			Removed funding logic
    // 01/02/97  R. Marchman				Add new security/next tran.
    // 06/18/97  M. D. Wheaton                         Removed datenum
    // 12/03/97  Skip Hardy                            Fix read for multiple
    // 						refunds.
    // 02/18/99  Sunya Sharp		Make changes per screen assessment approved by the
    // user.
    // 03/26/99  Sunya Sharp		Fix problems with displaying refunds that do not 
    // have payment requests.  This is new due to how FDSO partial adjustments
    // are being handled.  Add prompt for state field.  Remove EFT literals from
    // the screen.  Use action block si_get_cse_person_mailing_addr instead of
    // fn_get_active_cse_person_address.
    // 05/04/99  Sunya Sharp		Fix integration problems.  Do not allow certain 
    // source codes to have refunds created.  Create a row on the cash receipt
    // detail history when the cash receipt detail is updated.  Add literal
    // after payee name.  Undistributed amount not displaying correctly in
    // conjunction with adjusted cash receipt details.  Do not allow refunds to
    // be created for source codes with no organization number.  Do not allow
    // adjusted details to be refunded.  Add cash receipt detail status to the
    // screen.
    // 06/09/99	Sunya Sharp	Screen is not displaying multiple refunds correctly.
    // 06/16/99  Sunya Sharp	User was getting an error when adding a address 
    // that was the same as another refund for that detail.  Need to ensure that
    // the protected fields stay protected when error.  Need to handle all exit
    // states when returning from the display logic.
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented.
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 10/18/99	Sunya Sharp	H00077672	Add logic to prevent the refund amount 
    // from being negative.
    // 10/29/99	Sunya Sharp	00078409	Do not allow refunding of a CRD in pended 
    // status.
    // 11/5/99	Sunya Sharp	00079394	Confirm Key is not working when the CRD is 
    // in suspended status.  Also when deleting an address associated to a
    // refund ensure that it is not associated to another refund.
    // 11/18/99	Sunya Sharp	00079637	Send partial description of source code to 
    // LTRB.
    // ------------------------------------------------------------------------
    // 06/20/00        PPhinney        H00097714       Changed read of address 
    // to read either
    // FIPS or Organization Address
    // Use    FN_CAB_READ_CSE_PERSON_ADDRESS
    // instead of SI_GET_CSE_PERSON_MAILING_ADDR
    // 09/12/00        PPhinney        H00102727       0nly allow CRU workers to
    // see the
    // Address.
    // 11/17/00        PPhinney        I00106654       Prevent REFUNDS in Excess
    // of Available
    // 
    // Funds.
    // 02/13/01        PPhinney        I00113282       Prevent Blanking of TAX 
    // ID/SSN
    // when address is Retrieved.
    // 09/24/01        MBrown        127609       Added confirm on add and 
    // update of payee name and address.  Note that update logic (after edits)
    // is now in 2 places - in the update case of command, and in the confirm
    // case of command.
    // - The logic that performs the actual create of the refund is now within 
    // the case of command.
    // - Also fixed problem where a blank address was being created  sometimes.
    // 12/05/01         KDoshi          WR020147       Add KPC Recoupment flag.
    // 06/22/12	 GVandy		 CQ33628	Do not allow refunds to KSDLUI source type.
    // 08/12/14	 GVandy		 CQ42192	Do not allow refunds to CSSI source type.
    // 03/08/19         RMathews        CQ65539        Modify street 1 edit on 
    // add.
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    // *****
    // Move all imports to the exports
    // *****
    // 09/24/01        MBrown        127609
    if (!Equal(global.Command, "CONFIRM"))
    {
      export.HiddenPrevious.Command = "";
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    export.PreviousCsePersonsWorkSet.Number =
      import.PreviousCsePersonsWorkSet.Number;
    MoveCodeValue(import.PreviousCodeValue, export.PreviousCodeValue);
    export.RefundToPreviousCsePerson.Number =
      import.RefundToPreviousCsePerson.Number;
    export.RefundToPreviousCashReceiptSourceType.Code =
      import.RefundToPreviousCashReceiptSourceType.Code;
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    export.CollectionType.Code = import.CollectionType.Code;
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    export.CashReceiptType.Assign(import.CashReceiptType);
    export.RefundToCashReceiptSourceType.Code = import.RefundTo.Code;
    MoveCsePersonsWorkSet2(import.CashReceivedFrom, export.CashReceivedFrom);
    export.CrdCrComboNo.CrdCrCombo = import.CrdCrComboNo.CrdCrCombo;
    export.CashReceiptDetailStatus.Code = import.CashReceiptDetailStatus.Code;
    export.RefundToCsePerson.Assign(import.CsePerson2);
    export.PaymentRequest.Assign(import.PaymentRequest);
    export.ReceiptRefund.Assign(import.ReceiptRefund);
    export.Current.Assign(import.Current);
    export.SendTo.Assign(import.SendTo);
    export.DisplayComplete.Flag = import.DisplayComplete.Flag;
    export.PreviousReceiptRefund.Assign(import.PreviousReceiptRefund);
    export.SendToPrevious.Assign(import.SendToPrevious);
    export.Confirm.Flag = import.Confirm.Flag;
    export.CsePerson.PromptField = import.CsePerson1.PromptField;
    export.RefundReason.PromptField = import.RefundReason.PromptField;
    export.PaymentStatus.Code = import.PaymentStatus.Code;
    export.AvailableForRefund.Amount = import.AvailableForRefund.Amount;
    export.PromptReturnToSource.PromptField =
      import.PromptReturnToSource.PromptField;
    export.StatePrompt.PromptField = import.StatePrompt.PromptField;

    // 09/24/01        MBrown        127609       For confirm.
    export.HiddenPrevious.Command = import.HiddenPrevious.Command;

    // 09/12/00        PPhinney        H00102727       0nly allow CRU workers to
    // see the
    export.SendToHidden.Assign(import.SendToHidden);
    export.CurrentHidden.Assign(import.CurrentHidden);
    export.DisplayAddress.Flag = import.DisplayAddrsss.Flag;

    // 09/12/00        PPhinney        H00102727       Is USER authorized to 
    // view addresses
    if (AsChar(export.DisplayAddress.Flag) == 'Y' || AsChar
      (export.DisplayAddress.Flag) == 'N')
    {
    }
    else
    {
      local.SaveCommand.Command = global.Command;
      global.Command = "RESEARCH";
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // USER is Authorized to view Addresses
        export.DisplayAddress.Flag = "Y";
      }
      else
      {
        // USER is NOT Authorized to view Addresses
        export.DisplayAddress.Flag = "N";
      }

      ExitState = "ACO_NN0000_ALL_OK";
      global.Command = local.SaveCommand.Command;
    }

    // ---------------------------------------
    // Left Pad with Zeros : RBM    02/06/1997
    // ---------------------------------------
    if (!IsEmpty(export.RefundToCsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.RefundToCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.RefundToCsePerson.Number = local.TextWorkArea.Text10;
    }

    // *** When returning from CDVL and NAME if there was data in field before 
    // the flow and nothing is selected the information is lost.  Removed logic
    // that was causing this problem.  Sunya Sharp 2/22/1999 ***
    // *** When returning from NAME with a CSE person number the payee name, 
    // mail to address and tax id should be populated with default information.
    // Sunya Sharp 2/22/1999 ***
    if (Equal(global.Command, "PRMPTRET"))
    {
      switch(TrimEnd(import.Passed.CodeName))
      {
        case "STATE CODE":
          if (!IsEmpty(import.PassCodeValue.Cdvalue))
          {
            export.SendTo.State = import.PassCodeValue.Cdvalue;
          }

          break;
        case "REFUND REASON":
          if (!IsEmpty(import.PassCodeValue.Cdvalue))
          {
            export.ReceiptRefund.ReasonCode = import.PassCodeValue.Cdvalue;
          }

          break;
        default:
          break;
      }

      if (!IsEmpty(import.PassCsePersonsWorkSet.Number))
      {
        export.RefundToCsePerson.Number = import.PassCsePersonsWorkSet.Number;
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.RefundToCsePerson, "number");

          field.Error = true;

          return;
        }

        // 02/13/01        PPhinney        I00113282       Prevent Blanking of 
        // TAX ID/SSN
        if (!IsEmpty(export.ReceiptRefund.Taxid))
        {
          // * * * Save and Use ANY previous Value
        }
        else
        {
          export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
        }

        export.ReceiptRefund.PayeeName = local.CsePersonsWorkSet.FormattedName;
        local.CsePerson.Number = local.CsePersonsWorkSet.Number;

        // 06/20/00        PPhinney         097714        Changed read of 
        // address to read either FIPS or Organization Address
        UseFnCabReadCsePersonAddress();

        if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
          (local.CsePersonAddress.Street2) && IsEmpty
          (local.CsePersonAddress.City) && IsEmpty
          (local.CsePersonAddress.State))
        {
          // * *   Nothing Returned - Blank-Out Display and Hidden
          MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
            export.SendTo);

          // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see
          // the Address
          MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
            export.SendToHidden);
        }
        else
        {
          // * *   Address Returned - Save in Hidden
          // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see
          // the Address
          export.SendToHidden.Street1 = local.CsePersonAddress.Street1 ?? Spaces
            (25);
          export.SendToHidden.Street2 = local.CsePersonAddress.Street2 ?? "";
          export.SendToHidden.City = local.CsePersonAddress.City ?? Spaces(30);
          export.SendToHidden.State = local.CsePersonAddress.State ?? Spaces(2);
          export.SendToHidden.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
            (5);
          export.SendToHidden.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
          export.SendToHidden.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";

          // * *   Display Either Address or Security Message
          // * *       depending on display_address flag
          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
            export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
              (25);
            export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
            export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
            export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
            export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
              (5);
            export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";
          }
          else
          {
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.SendTo);
            export.SendTo.Street1 = "Security Block on Address";
          }
        }
      }

      return;
    }

    // *** Add logic to determine if there was a refund passed back from CRRL.  
    // If there was display.  Else determine is there was a detail display
    // before flow.  If there was display previous value.  If there was nothing
    // displayed before the flow return exit state message.  Sunya Sharp 2/18/
    // 1999 ***
    if (Equal(global.Command, "RETCRRL"))
    {
      if (Lt(local.NullReceiptRefund.CreatedTimestamp,
        import.ReturnedFromFlowReceiptRefund.CreatedTimestamp))
      {
        export.ReceiptRefund.Assign(import.ReturnedFromFlowReceiptRefund);
        global.Command = "DISPLAY";
      }
      else if (IsEmpty(export.CrdCrComboNo.CrdCrCombo))
      {
        var field1 = GetField(export.CrdCrComboNo, "crdCrCombo");

        field1.Error = true;

        var field2 = GetField(export.CashReceiptDetail, "refundedAmount");

        field2.Error = true;

        ExitState = "FN0000_MUST_ENTER_CRD_OR_REFUND";

        return;
      }
      else
      {
        return;
      }
    }

    // *** Add logic to determine if there was a refund passed back from CRDL.  
    // If there was display.  Else determine is there was a detail display
    // before flow.  If there was display previous value.  If there was nothing
    // displayed before the flow return exit state message.  Sunya Sharp 2/18/
    // 1999 ***
    if (Equal(global.Command, "RETCRDL"))
    {
      if (import.ReturnedFromFlowCashReceiptDetail.SequentialIdentifier > 0)
      {
        export.CashReceipt.SequentialNumber =
          import.ReturnedFromFlowCashReceipt.SequentialNumber;
        export.CashReceiptDetail.
          Assign(import.ReturnedFromFlowCashReceiptDetail);
        export.CashReceiptEvent.SystemGeneratedIdentifier =
          import.ReturnedFromFlowCashReceiptEvent.SystemGeneratedIdentifier;
        MoveCashReceiptSourceType(import.ReturnedFromFlowCashReceiptSourceType,
          export.CashReceiptSourceType);
        export.CashReceiptType.Assign(import.ReturnedFromFlowCashReceiptType);
        MoveReceiptRefund3(local.NullReceiptRefund, export.ReceiptRefund);
        global.Command = "DISPLAY";
      }
      else if (IsEmpty(export.CrdCrComboNo.CrdCrCombo))
      {
        var field1 = GetField(export.CrdCrComboNo, "crdCrCombo");

        field1.Error = true;

        var field2 = GetField(export.CashReceiptDetail, "refundedAmount");

        field2.Error = true;

        ExitState = "FN0000_MUST_ENTER_CRD_OR_REFUND";

        return;
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
      export.Hidden.CsePersonNumber = export.RefundToCsePerson.Number;
      export.Hidden.CsePersonNumberObligor = export.RefundToCsePerson.Number;
      export.Hidden.CsePersonNumberAp = export.RefundToCsePerson.Number;
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

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.RefundToCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.RefundToCsePerson.Number =
          export.Hidden.CsePersonNumberObligor ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberAp))
      {
        export.RefundToCsePerson.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
      }

      ExitState = "FN0000_FLOW_TO_CRUC_CRDL_OR_CRRL";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    // H00102727  08/12/00  bypass RESEARCH as it is for Display of Address ONLY
    if (Equal(global.Command, "RETLTRB") || Equal(global.Command, "CRRL") || Equal
      (global.Command, "LIST_COL") || Equal(global.Command, "RETCRSL") || Equal
      (global.Command, "RETRIEVE") || Equal(global.Command, "RESEARCH"))
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
      case "DISPLAY":
        export.Confirm.Flag = "N";

        // *****
        // Read entities to populate the screen
        // *****
        UseFnAbDisplayReceiptRefund1();

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        export.CurrentHidden.Assign(export.Current);

        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
          // * *   Security Authorized - Allow Display
        }
        else
        {
          // * *   Security NOT Authorized - Do NOT Allow Display
          if (IsEmpty(export.Current.City) && IsEmpty(export.Current.State) && IsEmpty
            (export.Current.Street1) && IsEmpty(export.Current.Street2) && IsEmpty
            (export.Current.ZipCode5))
          {
            // * *   Nothing Returned - Display and Hidden are BLANK
          }
          else
          {
            // * *   Security NOT Authorized - Display SECURITY message
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.Current);
            export.Current.Street1 = "Security Block on Address";
          }
        }

        // *****
        // Validate Exitstate upon return from the read CAB
        // *****
        // *** PR# 00079394  Add command that is needed to bypass not allowing a
        // suspended detail to be refunded.  This is only used in the
        // distribution batch process.  Sunya Sharp 11/5/1999 ***
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // *** Continue processing... ***
        }
        else if (IsExitState("FN0000_RCPT_REFUND_NF"))
        {
          ExitState = "FN0000_VALID_RFND_NF";

          return;
        }
        else if (IsExitState("FN0000_CASH_RECEIPT_DETAIL_NF"))
        {
          var field8 = GetField(export.CrdCrComboNo, "crdCrCombo");

          field8.Error = true;

          var field9 = GetField(export.CashReceiptDetail, "refundedAmount");

          field9.Error = true;

          ExitState = "FN0000_MUST_ENTER_CRD_OR_REFUND";

          return;
        }
        else if (IsExitState("FN0000_CRD_AND_REFUND_NOT_ENTERD"))
        {
          var field8 = GetField(export.CrdCrComboNo, "crdCrCombo");

          field8.Error = true;

          var field9 = GetField(export.CashReceiptDetail, "refundedAmount");

          field9.Error = true;

          ExitState = "FN0000_MUST_ENTER_CRD_OR_REFUND";

          return;
        }
        else if (IsExitState("FN0000_ENTER_CRD_OR_REFUND"))
        {
          return;
        }
        else if (IsExitState("FN0000_PROTECT_RECEIPT_REFUND"))
        {
          // *** No longer protecting fields.  There were problems with the add 
          // and update process.  The user requested it be removed.  Sunya 3/2/
          // 1999 ***
        }
        else if (IsExitState("FN0000_MULT_REFUNDS_EXIST_DETAIL"))
        {
          // *** Continue processing... ***
        }
        else
        {
          return;
        }

        // *** Screen was not displaying undistributed information correctly.  
        // The undistributed amount and amount available for refund were not
        // correct.  Sunya Sharp 5/4/1999 ***
        if (Equal(local.CashReceiptDetailStatus.Code, "ADJ"))
        {
          if (Equal(export.ReceiptRefund.CreatedTimestamp,
            local.NullReceiptRefund.CreatedTimestamp))
          {
            export.ReceiptRefund.Taxid =
              export.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
            export.ReceiptRefund.Amount = 0;
          }

          export.AvailableForRefund.Amount = 0;
        }
        else
        {
          if (Equal(export.ReceiptRefund.CreatedTimestamp,
            local.NullReceiptRefund.CreatedTimestamp))
          {
            export.ReceiptRefund.Taxid =
              export.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
            export.ReceiptRefund.Amount =
              export.CashReceiptDetail.ReceivedAmount - (
                export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() +
              export.CashReceiptDetail.RefundedAmount.GetValueOrDefault());
          }

          export.AvailableForRefund.Amount =
            export.CashReceiptDetail.ReceivedAmount - export
            .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
            .CashReceiptDetail.RefundedAmount.GetValueOrDefault();

          // 11/17/00  PPhinney   00106654 Prevent REFUNDS in Excess of 
          // Available  Funds
          // Only HIGHLIGHT the Errors - This is on the DISPLAY Action
          if (export.AvailableForRefund.Amount < 0)
          {
            var field = GetField(export.AvailableForRefund, "amount");

            field.Error = true;
          }

          if (export.CashReceiptDetail.RefundedAmount.GetValueOrDefault() < 0)
          {
            var field = GetField(export.CashReceiptDetail, "refundedAmount");

            field.Error = true;
          }
        }

        export.DisplayComplete.Flag = "Y";
        MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
          export.SendTo);
        export.CashReceiptDetailStatus.Code =
          local.CashReceiptDetailStatus.Code;

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
          export.SendToHidden);

        // *** Added logic to check to see if there is a refund and money 
        // available to determine the exit state that is to be displayed.  Sunya
        // Sharp 2/18/1999 ***
        if (export.AvailableForRefund.Amount > 0 && Equal
          (export.ReceiptRefund.CreatedTimestamp,
          local.NullReceiptRefund.CreatedTimestamp))
        {
          ExitState = "FN0000_COLL_SUCCESSFULLY_READ";
        }
        else if (IsExitState("FN0000_MULT_REFUNDS_EXIST_DETAIL"))
        {
        }
        else if (AsChar(export.PaymentRequest.RecoupmentIndKpc) == 'Y')
        {
          ExitState = "FN0000_DISPL_SUCC_KPC_RECOUPMENT";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "LIST":
        export.Confirm.Flag = "N";

        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        // *****
        // Prompt for Source type
        // *****
        switch(AsChar(export.PromptReturnToSource.PromptField))
        {
          case 'S':
            if (AsChar(export.RefundReason.PromptField) == 'S')
            {
              var field8 = GetField(export.PromptReturnToSource, "promptField");

              field8.Error = true;

              var field9 = GetField(export.RefundReason, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.CsePerson.PromptField) == 'S')
            {
              var field8 = GetField(export.PromptReturnToSource, "promptField");

              field8.Error = true;

              var field9 = GetField(export.CsePerson, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.StatePrompt.PromptField) == 'S')
            {
              var field8 = GetField(export.PromptReturnToSource, "promptField");

              field8.Error = true;

              var field9 = GetField(export.StatePrompt, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            export.PromptReturnToSource.PromptField = "+";
            ExitState = "ECO_LNK_TO_CRSL";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.PromptReturnToSource, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        // *****
        // Prompt for Cse Person.
        // *****
        switch(AsChar(export.CsePerson.PromptField))
        {
          case 'S':
            if (AsChar(export.RefundReason.PromptField) == 'S')
            {
              var field8 = GetField(export.CsePerson, "promptField");

              field8.Error = true;

              var field9 = GetField(export.RefundReason, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (AsChar(export.StatePrompt.PromptField) == 'S')
            {
              var field8 = GetField(export.CsePerson, "promptField");

              field8.Error = true;

              var field9 = GetField(export.StatePrompt, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            export.CsePerson.PromptField = "+";
            export.Phonetic.Percentage = 35;
            export.Phonetic.Flag = "Y";
            ExitState = "ECO_LNK_TO_LIST_CASE_CSE_PERSON";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.CsePerson, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        // *****
        // Prompt for Reason Code.
        // *****
        switch(AsChar(export.RefundReason.PromptField))
        {
          case 'S':
            if (AsChar(export.StatePrompt.PromptField) == 'S')
            {
              var field8 = GetField(export.RefundReason, "promptField");

              field8.Error = true;

              var field9 = GetField(export.StatePrompt, "promptField");

              field9.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
            }

            if (IsExitState("ACO_NE0000_INVALID_MULT_PROMPT_S"))
            {
              return;
            }

            export.RefundReason.PromptField = "+";
            export.Code.CodeName = "REFUND REASON";
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.RefundReason, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        // *****
        // Prompt for State Code.
        // *****
        switch(AsChar(export.StatePrompt.PromptField))
        {
          case 'S':
            export.StatePrompt.PromptField = "+";
            export.Code.CodeName = "STATE CODE";
            ExitState = "ECO_LNK_TO_CODE_TABLES";

            return;
          case ' ':
            break;
          case '+':
            break;
          default:
            var field = GetField(export.StatePrompt, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        // *****
        // If no prompt has been selected instruct the user to select field to 
        // prompt on.
        // *****
        var field1 = GetField(export.StatePrompt, "promptField");

        field1.Error = true;

        var field2 = GetField(export.PromptReturnToSource, "promptField");

        field2.Error = true;

        var field3 = GetField(export.CsePerson, "promptField");

        field3.Error = true;

        var field4 = GetField(export.RefundReason, "promptField");

        field4.Error = true;

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        return;
      case "RETCRSL":
        export.Confirm.Flag = "N";

        // *** When returning from CRSL if there was data in field before the 
        // flow and nothing is selected the information is lost.  Removed logic
        // that was causing this problem.  Sunya Sharp 2/22/1999 ***
        // *** When returning from CRSL with a source code, the payee name, mail
        // to address and tax id should be populated with default information.
        // Sunya Sharp 2/22/1999 ***
        if (!IsEmpty(import.ReturnedFromFlowCashReceiptSourceType.Code))
        {
          export.RefundToCashReceiptSourceType.Code =
            import.ReturnedFromFlowCashReceiptSourceType.Code;

          if (Equal(export.RefundToCashReceiptSourceType.Code, 4, 5, "STATE"))
          {
            ExitState = "FN0000_FLOW_TO_LTRB_FOR_ADDRESS";

            return;
          }
          else if (ReadCashReceiptSourceType2())
          {
            local.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);

            if (ReadTribunal1())
            {
              export.ReceiptRefund.PayeeName = entities.Tribunal.Name;

              // 02/13/01        PPhinney        I00113282       Prevent 
              // Blanking of TAX ID/SSN
              if (!IsEmpty(export.ReceiptRefund.Taxid))
              {
                // * * * Save and Use ANY previous Value
              }
              else
              {
                export.ReceiptRefund.Taxid = entities.Tribunal.TaxId;
              }
            }
            else
            {
              var field =
                GetField(export.RefundToCashReceiptSourceType, "code");

              field.Error = true;

              ExitState = "TRIBUNAL_NF";

              return;
            }

            if (ReadFipsTribAddress())
            {
              export.SendTo.Street1 = entities.FipsTribAddress.Street1;
              export.SendTo.Street2 = entities.FipsTribAddress.Street2;
              export.SendTo.City = entities.FipsTribAddress.City;
              export.SendTo.State = entities.FipsTribAddress.State;
              export.SendTo.ZipCode5 = entities.FipsTribAddress.ZipCode;
              export.SendTo.ZipCode4 = entities.FipsTribAddress.Zip4;
              export.SendTo.ZipCode3 = entities.FipsTribAddress.Zip3;
            }
          }
        }

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        export.SendToHidden.Assign(export.SendTo);

        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
          // * *   Security Authorized - Allow Display
        }
        else
        {
          // * *   Security NOT Authorized - Do NOT Allow Display
          if (IsEmpty(export.SendTo.Street1) && IsEmpty
            (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
            (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
          {
            // * *   Nothing Returned - Display and Hidden are BLANK
          }
          else
          {
            // * *   Security NOT Authorized - Display SECURITY message
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.SendTo);
            export.SendTo.Street1 = "Security Block on Address";
          }
        }

        return;
      case "RETLTRB":
        export.Confirm.Flag = "N";

        if (import.DlgflwSelected.Identifier > 0)
        {
          if (ReadTribunal2())
          {
            export.ReceiptRefund.PayeeName = entities.Tribunal.Name;

            // 02/13/01        PPhinney        I00113282       Prevent Blanking 
            // of TAX ID/SSN
            if (!IsEmpty(export.ReceiptRefund.Taxid))
            {
              // * * * Save and Use ANY previous Value
            }
            else
            {
              export.ReceiptRefund.Taxid = entities.Tribunal.TaxId;
            }
          }
          else
          {
            ExitState = "TRIBUNAL_NF";

            return;
          }

          if (ReadFipsTribAddress())
          {
            export.SendTo.Street1 = entities.FipsTribAddress.Street1;
            export.SendTo.Street2 = entities.FipsTribAddress.Street2;
            export.SendTo.City = entities.FipsTribAddress.City;
            export.SendTo.State = entities.FipsTribAddress.State;
            export.SendTo.ZipCode5 = entities.FipsTribAddress.ZipCode;
            export.SendTo.ZipCode4 = entities.FipsTribAddress.Zip4;
            export.SendTo.ZipCode3 = entities.FipsTribAddress.Zip3;
          }
        }

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        export.SendToHidden.Assign(export.SendTo);

        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
          // * *   Security Authorized - Allow Display
        }
        else
        {
          // * *   Security NOT Authorized - Do NOT Allow Display
          if (IsEmpty(export.SendTo.Street1) && IsEmpty
            (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
            (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
          {
            // * *   Nothing Returned - Display and Hidden are BLANK
          }
          else
          {
            // * *   Security NOT Authorized - Display SECURITY message
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.SendTo);
            export.SendTo.Street1 = "Security Block on Address";
          }
        }

        return;
      case "RETRIEVE":
        export.Confirm.Flag = "N";

        // *** Added new PF key to retrieve address.  The new key will eliminate
        // any confusion for the user when trying to get the correct current
        // address for the selection made.  Sunya Sharp 2/22/1999 ***
        if (IsEmpty(export.CrdCrComboNo.CrdCrCombo))
        {
          ExitState = "FN0000_FLOW_TO_CRRC_FOR_DETAIL";

          return;
        }

        if (!IsEmpty(export.RefundToCashReceiptSourceType.Code) && !
          IsEmpty(export.RefundToCsePerson.Number))
        {
          var field8 = GetField(export.RefundToCashReceiptSourceType, "code");

          field8.Error = true;

          var field9 = GetField(export.RefundToCsePerson, "number");

          field9.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

          return;
        }

        if (!IsEmpty(export.RefundToCashReceiptSourceType.Code))
        {
          if (Equal(export.RefundToCashReceiptSourceType.Code, 4, 5, "STATE"))
          {
            ExitState = "FN0000_FLOW_TO_LTRB_FOR_ADDRESS";

            return;
          }
          else if (ReadCashReceiptSourceType1())
          {
            local.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);

            if (ReadTribunal1())
            {
              export.ReceiptRefund.PayeeName = entities.Tribunal.Name;

              // 02/13/01        PPhinney        I00113282       Prevent 
              // Blanking of TAX ID/SSN
              if (!IsEmpty(export.ReceiptRefund.Taxid))
              {
                // * * * Save and Use ANY previous Value
              }
              else
              {
                export.ReceiptRefund.Taxid = entities.Tribunal.TaxId;
              }
            }
            else
            {
              ExitState = "TRIBUNAL_NF";

              return;
            }

            if (ReadFipsTribAddress())
            {
              export.SendTo.Street1 = entities.FipsTribAddress.Street1;
              export.SendTo.Street2 = entities.FipsTribAddress.Street2;
              export.SendTo.City = entities.FipsTribAddress.City;
              export.SendTo.State = entities.FipsTribAddress.State;
              export.SendTo.ZipCode5 = entities.FipsTribAddress.ZipCode;
              export.SendTo.ZipCode4 = entities.FipsTribAddress.Zip4;
              export.SendTo.ZipCode3 = entities.FipsTribAddress.Zip3;
              ExitState = "FN0000_ADDRESS_RETRIEVED_SUCCESS";

              var field = GetField(export.ReceiptRefund, "reasonText");

              field.Protected = false;
              field.Focused = true;
            }

            // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to 
            // see the Address
            export.SendToHidden.Assign(export.SendTo);

            if (AsChar(export.DisplayAddress.Flag) == 'Y')
            {
              // * *   Security Authorized - Allow Display
            }
            else
            {
              // * *   Security NOT Authorized - Do NOT Allow Display
              if (IsEmpty(export.SendTo.Street1) && IsEmpty
                (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
                (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.SendTo);
                export.SendTo.Street1 = "Security Block on Address";
              }
            }
          }
          else
          {
            var field = GetField(export.RefundToCashReceiptSourceType, "code");

            field.Error = true;

            ExitState = "CASH_RECEIPT_SOURCE_TYPE_NF";

            return;
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "FN0000_NO_ADDRESS_FOUND";
          }

          return;
        }

        if (!IsEmpty(export.RefundToCsePerson.Number))
        {
          local.CsePersonsWorkSet.Number = export.RefundToCsePerson.Number;
          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.RefundToCsePerson, "number");

            field.Error = true;

            return;
          }

          // 02/13/01        PPhinney        I00113282       Prevent Blanking of
          // TAX ID/SSN
          if (!IsEmpty(export.ReceiptRefund.Taxid))
          {
            // * * * Save and Use ANY previous Value
          }
          else
          {
            export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
          }

          export.ReceiptRefund.PayeeName =
            local.CsePersonsWorkSet.FormattedName;
          local.CsePerson.Number = local.CsePersonsWorkSet.Number;

          // 06/20/00        PPhinney         097714        Changed read of 
          // address to read either FIPS or Organization Address
          UseFnCabReadCsePersonAddress();

          if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
            (local.CsePersonAddress.Street2) && IsEmpty
            (local.CsePersonAddress.City) && IsEmpty
            (local.CsePersonAddress.State))
          {
            ExitState = "FN0000_NO_ADDRESS_FOUND";
          }
          else
          {
            export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
              (25);
            export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
            export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
            export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
            export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
              (5);
            export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";
            ExitState = "FN0000_ADDRESS_RETRIEVED_SUCCESS";

            var field = GetField(export.ReceiptRefund, "reasonText");

            field.Protected = false;
            field.Focused = true;
          }

          // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see
          // the Address
          export.SendToHidden.Assign(export.SendTo);

          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
            // * *   Security Authorized - Allow Display
          }
          else
          {
            // * *   Security NOT Authorized - Do NOT Allow Display
            if (IsEmpty(export.SendTo.Street1) && IsEmpty
              (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
              (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
            {
              // * *   Nothing Returned - Display and Hidden are BLANK
            }
            else
            {
              // * *   Security NOT Authorized - Display SECURITY message
              MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                export.SendTo);
              export.SendTo.Street1 = "Security Block on Address";
            }
          }

          return;
        }

        if (!IsEmpty(export.ReceiptRefund.Taxid))
        {
          local.CsePersonsWorkSet.Ssn = export.ReceiptRefund.Taxid ?? Spaces(9);
          UseFnReadCsePersonUsingSsnO();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.ReceiptRefund, "taxid");

            field.Error = true;

            return;
          }

          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.ReceiptRefund, "taxid");

            field.Error = true;

            return;
          }

          export.RefundToCsePerson.Number = local.CsePersonsWorkSet.Number;
          export.ReceiptRefund.PayeeName =
            local.CsePersonsWorkSet.FormattedName;
          local.CsePerson.Number = local.CsePersonsWorkSet.Number;

          // 06/20/00        PPhinney         097714        Changed read of 
          // address to read either FIPS or Organization Address
          UseFnCabReadCsePersonAddress();

          if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
            (local.CsePersonAddress.Street2) && IsEmpty
            (local.CsePersonAddress.City) && IsEmpty
            (local.CsePersonAddress.State))
          {
            ExitState = "FN0000_NO_ADDRESS_FOUND";
          }
          else
          {
            export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
              (25);
            export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
            export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
            export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
            export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
              (5);
            export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
            export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";
            ExitState = "FN0000_ADDRESS_RETRIEVED_SUCCESS";

            var field = GetField(export.ReceiptRefund, "reasonText");

            field.Protected = false;
            field.Focused = true;
          }

          // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see
          // the Address
          export.SendToHidden.Assign(export.SendTo);

          if (AsChar(export.DisplayAddress.Flag) == 'Y')
          {
            // * *   Security Authorized - Allow Display
          }
          else
          {
            // * *   Security NOT Authorized - Do NOT Allow Display
            if (IsEmpty(export.SendTo.Street1) && IsEmpty
              (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
              (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
            {
              // * *   Nothing Returned - Display and Hidden are BLANK
            }
            else
            {
              // * *   Security NOT Authorized - Display SECURITY message
              MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                export.SendTo);
              export.SendTo.Street1 = "Security Block on Address";
            }
          }

          return;
        }

        var field5 = GetField(export.RefundToCashReceiptSourceType, "code");

        field5.Error = true;

        var field6 = GetField(export.RefundToCsePerson, "number");

        field6.Error = true;

        var field7 = GetField(export.ReceiptRefund, "taxid");

        field7.Error = true;

        ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

        return;
      case "ADD":
        // : Sept 21, 2001, M. Brown, pr# 127609
        // : Perform edits, then display a message telling user to confirm add.
        // *****************************************
        // Require User to display before updating
        // *****************************************
        // *** Change exit state per user request.  Sunya Sharp 2/23/1999 ***
        if (AsChar(export.DisplayComplete.Flag) != 'Y')
        {
          if (import.CashReceiptDetail.SequentialIdentifier == 0 || import
            .CashReceipt.SequentialNumber == 0 || import
            .CashReceiptEvent.SystemGeneratedIdentifier == 0 || import
            .CashReceiptSourceType.SystemGeneratedIdentifier == 0)
          {
            ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

            return;
          }
        }

        // *** Add logic to make fields error.  Sunya Sharp 2/26/1999
        // Changed logic to check the available for refund field.  This is the 
        // correct undistributed amount incase the detail is adjusted.  Sunya
        // Sharp 5/4/1999 ***
        if (export.CashReceipt.SequentialNumber > 0 && export
          .CashReceiptDetail.SequentialIdentifier > 0)
        {
          if (export.AvailableForRefund.Amount < export.ReceiptRefund.Amount)
          {
            var field8 = GetField(export.AvailableForRefund, "amount");

            field8.Error = true;

            var field9 = GetField(export.ReceiptRefund, "amount");

            field9.Error = true;

            ExitState = "FN0000_CANT_REF_UNDST_AMT_LT_REF";

            return;
          }
        }

        // *** PR# 78409 - Do not allow refunding of a CRD in pended status.  
        // Sunya Sharp 10/29/1999 ***
        if (Equal(export.CashReceiptDetailStatus.Code, "PEND"))
        {
          var field = GetField(export.CashReceiptDetailStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANNOT_REFUND_CRD_PENDED";

          return;
        }

        if (!IsEmpty(export.RefundToCsePerson.Number))
        {
          // *****
          // Validate CSE PERSON
          // *****
          if (!ReadCsePerson())
          {
            var field = GetField(export.RefundToCsePerson, "number");

            field.Error = true;

            ExitState = "CSE_PERSON_NF";

            return;
          }

          local.CsePersonsWorkSet.Number = export.RefundToCsePerson.Number;
          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.RefundToCsePerson, "number");

            field.Error = true;

            return;
          }

          // *****
          // Get Payee name and Address
          // *****
          if (IsEmpty(export.ReceiptRefund.PayeeName))
          {
            export.ReceiptRefund.PayeeName =
              local.CsePersonsWorkSet.FormattedName;
          }

          if (IsEmpty(export.ReceiptRefund.Taxid))
          {
            if (AsChar(entities.CsePerson.Type1) == 'C')
            {
              export.ReceiptRefund.Taxid = local.CsePersonsWorkSet.Ssn;
            }
            else
            {
              export.ReceiptRefund.Taxid = entities.CsePerson.TaxId;
            }
          }

          // *** Changed logic to use the FN_GET_ACTIVE_CSE_PERSON_ADDRESS to 
          // get the correct address.  Sunya Sharp 2/23/1999 ***
          if (IsEmpty(export.SendTo.City) && IsEmpty(export.SendTo.State) && IsEmpty
            (export.SendTo.Street1) && IsEmpty(export.SendTo.Street2) && IsEmpty
            (export.SendTo.ZipCode5))
          {
            local.CsePerson.Number = local.CsePersonsWorkSet.Number;

            // 06/20/00        PPhinney         097714        Changed read of 
            // address to read either FIPS or Organization Address
            UseFnCabReadCsePersonAddress();

            if (IsEmpty(local.CsePersonAddress.Street1) && IsEmpty
              (local.CsePersonAddress.Street2) && IsEmpty
              (local.CsePersonAddress.City) && IsEmpty
              (local.CsePersonAddress.State))
            {
            }
            else
            {
              export.SendTo.Street1 = local.CsePersonAddress.Street1 ?? Spaces
                (25);
              export.SendTo.Street2 = local.CsePersonAddress.Street2 ?? "";
              export.SendTo.City = local.CsePersonAddress.City ?? Spaces(30);
              export.SendTo.State = local.CsePersonAddress.State ?? Spaces(2);
              export.SendTo.ZipCode5 = local.CsePersonAddress.ZipCode ?? Spaces
                (5);
              export.SendTo.ZipCode4 = local.CsePersonAddress.Zip4 ?? "";
              export.SendTo.ZipCode3 = local.CsePersonAddress.Zip3 ?? "";
            }

            // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to 
            // see the Address
            export.SendToHidden.Assign(export.SendTo);

            if (AsChar(export.DisplayAddress.Flag) == 'Y')
            {
              // * *   Security Authorized - Allow Display
            }
            else
            {
              // * *   Security NOT Authorized - Do NOT Allow Display
              if (IsEmpty(export.SendTo.Street1) && IsEmpty
                (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
                (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.SendTo);
                export.SendTo.Street1 = "Security Block on Address";
              }
            }
          }
        }

        if (!IsEmpty(export.RefundToCashReceiptSourceType.Code))
        {
          if (ReadCashReceiptSourceType3())
          {
            // *** Add logic to not allow refunds to be created for the 
            // following source codes: SDSO, FDSO, MISC, and EMP.  Sunya Sharp 5
            // /4/1999 ***
            // 06/22/12  GVandy  CQ33628  Do not allow refunds to KSDLUI source 
            // type.
            // 08/12/12  GVandy  CQ42192  Do not allow refunds to CSSI source 
            // type.
            if (Equal(entities.RefundTo.Code, "SDSO") || Equal
              (entities.RefundTo.Code, "FDSO") || Equal
              (entities.RefundTo.Code, "MISC") || Equal
              (entities.RefundTo.Code, "EMP") || Equal
              (entities.RefundTo.Code, "KSDLUI") || Equal
              (entities.RefundTo.Code, "CSSI"))
            {
              var field =
                GetField(export.RefundToCashReceiptSourceType, "code");

              field.Error = true;

              ExitState = "FN0000_REFUND_NOT_ALLOW_FOR_SRC";

              return;
            }
          }
          else
          {
            ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

            var field = GetField(export.RefundToCashReceiptSourceType, "code");

            field.Error = true;

            return;
          }
        }

        // *****
        // Validate Required entry fields
        // *****
        if (export.CashReceiptDetail.SequentialIdentifier == 0)
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (export.ReceiptRefund.Amount == 0)
        {
          var field = GetField(export.ReceiptRefund, "amount");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        // *** Add logic to prevent the refund amount from being negative.  HEAT
        // H00077672.  Sunya Sharp 10/18/1999. ***
        if (export.ReceiptRefund.Amount < 0)
        {
          var field = GetField(export.ReceiptRefund, "amount");

          field.Error = true;

          ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";

          return;
        }

        if (IsEmpty(export.RefundToCashReceiptSourceType.Code) && IsEmpty
          (export.RefundToCsePerson.Number) || !
          IsEmpty(export.RefundToCashReceiptSourceType.Code) && !
          IsEmpty(export.RefundToCsePerson.Number))
        {
          var field8 = GetField(export.RefundToCsePerson, "number");

          field8.Error = true;

          var field9 = GetField(export.RefundToCashReceiptSourceType, "code");

          field9.Error = true;

          ExitState = "OE0000_ONLY_ONE_VALUE_PERMITTED";

          return;
        }

        if (IsEmpty(export.ReceiptRefund.ReasonCode))
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }

        if (IsEmpty(export.ReceiptRefund.Taxid))
        {
          var field = GetField(export.ReceiptRefund, "taxid");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (IsEmpty(export.ReceiptRefund.PayeeName))
        {
          var field = GetField(export.ReceiptRefund, "payeeName");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // *****
        // Validate that zip code entry is numeric
        // *****
        if (Verify(export.SendTo.ZipCode5, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (Verify(export.SendTo.ZipCode4, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode4");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (Verify(export.SendTo.ZipCode3, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode3");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

          return;
        }

        // *****
        // Validate Reason Code
        // *****
        local.Pass.Cdvalue = export.ReceiptRefund.ReasonCode;
        local.Code.CodeName = "REFUND REASON";
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }

        // *****
        // Determine if the collection is a non-cash collection and if it is 
        // send a warning message to the user and prompt the user for
        // confirmation on the refund.
        // *****
        if (IsEmpty(export.CashReceiptType.CategoryIndicator))
        {
          if (ReadCashReceiptType())
          {
            export.CashReceiptType.Assign(entities.CashReceiptType);
          }
        }

        // *****
        // Determine which address to associate to the RECEIPT REFUND
        // *****
        // *** Change the logic to determine if the cash receipt detail address 
        // is the same as the sent to address.  Sunya Sharp 2/23/1999 ***
        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        // ReSet Blocked Values for Checking and Updating
        // 09/24/01        MBrown        127609
        // Save the addresses.  The logic after this can end up repopulating the
        // export address
        // for a  security blocked address, so we will restore the export 
        // address when the edits are done.
        local.TempSendTo.Assign(export.SendTo);
        local.TempCurrent.Assign(export.Current);

        if (Equal(export.Current.Street1, "Security Block on Address"))
        {
          export.Current.Assign(export.CurrentHidden);
        }

        if (IsEmpty(export.SendTo.Street1) && IsEmpty
          (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
          (export.SendTo.State))
        {
          local.Validate.Assign(export.Current);
          local.NewAddress.Flag = "";
        }
        else
        {
          local.Validate.Assign(export.SendTo);

          // 09/24/01  MBrown  PR# 127609
          // moved this from below this structure.  The new address flag was 
          // being set when the sent to address was spaces, resulting in blank
          // addresses being set up.
          if (Equal(export.SendTo.Street1, export.Current.Street1) && Equal
            (export.SendTo.Street2, export.Current.Street2) && Equal
            (export.SendTo.City, export.Current.City) && Equal
            (export.SendTo.State, export.Current.State) && Equal
            (export.SendTo.ZipCode5, export.Current.ZipCode5))
          {
            local.NewAddress.Flag = "";
          }
          else
          {
            local.NewAddress.Flag = "Y";
          }
        }

        // *****
        // Determine if the address is complete
        // *****
        // CQ#65539 Modify street edit on add.  Street 1 is required and street 
        // 2 is optional.
        if (IsEmpty(local.Validate.Street1))
        {
          var field = GetField(export.SendTo, "street1");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (IsEmpty(local.Validate.City))
        {
          var field = GetField(export.SendTo, "city");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (IsEmpty(local.Validate.State))
        {
          var field = GetField(export.SendTo, "state");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (IsEmpty(local.Validate.ZipCode5))
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          // 09/24/01        MBrown        127609
          // If there's an error, populate send address with validate local 
          // view.
          // Since this code validates current address if no send to address,
          // the current address will need to be put in the send to address in 
          // order to fix it.
          export.SendTo.Assign(local.Validate);
          ExitState = "ADDRESS_INCOMPLETE";

          return;
        }

        // *****
        // Verify entered State code
        // *****
        local.Pass.Cdvalue = local.Validate.State;
        local.Code.CodeName = "STATE CODE";
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.SendTo, "state");

          field.Error = true;

          ExitState = "CODE_NF";

          return;
        }

        // 09/24/01        MBrown        127609
        // : Restore address now that edits are complete.  This is to handle
        //  a security blocked address.
        MoveCashReceiptDetailAddress(local.TempSendTo, export.SendTo);
        MoveCashReceiptDetailAddress(local.TempCurrent, export.Current);

        // *** Add logic to ensure that the note for warrant stub is populated.
        // Sunya Sharp 2/26/1999 ***
        if (IsEmpty(export.ReceiptRefund.ReasonText))
        {
          var field = GetField(export.ReceiptRefund, "reasonText");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // 11/17/00  PPhinney   00106654 Prevent REFUNDS in Excess of Available
        // Funds
        local.AvailiableForRefund.Amount =
          export.CashReceiptDetail.ReceivedAmount - export
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

        if (local.AvailiableForRefund.Amount < export.ReceiptRefund.Amount)
        {
          var field8 = GetField(export.AvailableForRefund, "amount");

          field8.Error = true;

          var field9 = GetField(export.ReceiptRefund, "amount");

          field9.Error = true;

          ExitState = "FN0000_EXCESSIVE_REFUND_RB";

          return;
        }

        // : Sept, 2001, MBrown, pr#127609: Moved protection logic to the end of
        // the pstep.
        // ---- Protect all fields, so that no changes are possible before 
        // confirmation ----
        local.ProtectFields.Flag = "Y";

        if (AsChar(export.CashReceiptType.CategoryIndicator) == 'N')
        {
          // : User has to confirm twice for non cash, so confirm flag won't
          // be set until the second confirm (which is issued in the case of 
          // confirm logic).
        }
        else
        {
          export.Confirm.Flag = "Y";
        }

        ExitState = "FN0000_CRRU_VERIFY_NAME_AND_ADDR";
        export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
        export.SendToPrevious.Assign(export.SendTo);
        export.HiddenPrevious.Command = global.Command;

        // 09/24/01  MBrown  PR# 127609
        // moved add processing into confirm logic.
        break;
      case "UPDATE":
        export.Confirm.Flag = "N";

        // *****
        // Require User to display before updating
        // *****
        if (AsChar(export.DisplayComplete.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          return;
        }

        // *** If the refund was created earlier than current date do not allow 
        // the refund to be updated.  Sunya Sharp 2/27/1999 ***
        if (Lt(export.ReceiptRefund.RequestDate, Now().Date) && Lt
          (local.NullReceiptRefund.CreatedTimestamp,
          export.ReceiptRefund.CreatedTimestamp))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

          return;
        }

        // *** If refund was displayed and the user tries to update the person 
        // or source code send error message to the screen.  Sunya 3/1/1999 ***
        if (Lt(local.NullReceiptRefund.CreatedTimestamp,
          export.ReceiptRefund.CreatedTimestamp))
        {
          if (!Equal(export.RefundToCashReceiptSourceType.Code,
            export.RefundToPreviousCashReceiptSourceType.Code))
          {
            var field = GetField(export.RefundToCashReceiptSourceType, "code");

            field.Error = true;

            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
          }

          if (!Equal(export.RefundToCsePerson.Number,
            export.RefundToPreviousCsePerson.Number))
          {
            var field = GetField(export.RefundToCsePerson, "number");

            field.Error = true;

            ExitState = "ACO_NE0000_CANT_UPD_HIGHLTD_FLDS";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // *****
        // Validate Required entry fields
        // *****
        if (export.ReceiptRefund.Amount == 0)
        {
          var field = GetField(export.ReceiptRefund, "amount");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // *** Add logic to prevent the refund amount from being negative.  HEAT
        // H00077672.  Sunya Sharp 10/18/1999. ***
        if (export.ReceiptRefund.Amount < 0)
        {
          var field = GetField(export.ReceiptRefund, "amount");

          field.Error = true;

          ExitState = "FN0000_AMT_CANNOT_BE_NEGATIVE";

          return;
        }

        if (IsEmpty(export.ReceiptRefund.ReasonCode))
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }

        if (IsEmpty(export.ReceiptRefund.Taxid))
        {
          var field = GetField(export.ReceiptRefund, "taxid");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (IsEmpty(export.ReceiptRefund.PayeeName))
        {
          var field = GetField(export.ReceiptRefund, "payeeName");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // *****
        // Validate that zip code entry is numeric
        // *****
        if (Verify(export.SendTo.ZipCode5, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode5");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (Verify(export.SendTo.ZipCode4, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode4");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (Verify(export.SendTo.ZipCode3, " 0123456789") != 0)
        {
          var field = GetField(export.SendTo, "zipCode3");

          field.Error = true;

          local.Error.Flag = "Y";
        }

        if (AsChar(local.Error.Flag) == 'Y')
        {
          ExitState = "ACO_NE0000_ZIP_CODE_NOT_NUMERIC";

          return;
        }

        // *****
        // Validate Reason Code
        // *****
        local.Pass.Cdvalue = export.ReceiptRefund.ReasonCode;
        local.Code.CodeName = "REFUND REASON";
        UseCabValidateCodeValue();

        if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
        {
          var field = GetField(export.ReceiptRefund, "reasonCode");

          field.Error = true;

          ExitState = "FN0000_ENTER_VALID_REFUND_REASON";

          return;
        }

        // *****
        // If no changes to the valid change fields have been made display 
        // exitstate
        // *****
        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        // Added OR to check for Security Block
        if (export.ReceiptRefund.Amount == export
          .PreviousReceiptRefund.Amount && Equal
          (export.ReceiptRefund.PayeeName,
          export.PreviousReceiptRefund.PayeeName) && Equal
          (export.ReceiptRefund.ReasonCode,
          export.PreviousReceiptRefund.ReasonCode) && Equal
          (export.ReceiptRefund.ReasonText,
          export.PreviousReceiptRefund.ReasonText) && Equal
          (export.ReceiptRefund.Taxid, export.PreviousReceiptRefund.Taxid) && (
            Equal(export.SendTo.Street1, export.SendToPrevious.Street1) && Equal
          (export.SendTo.Street2, export.SendToPrevious.Street2) && Equal
          (export.SendTo.City, export.SendToPrevious.City) && Equal
          (export.SendTo.State, export.SendToPrevious.State) && Equal
          (export.SendTo.ZipCode5, export.SendToPrevious.ZipCode5) && Equal
          (export.SendTo.ZipCode4, export.SendToPrevious.ZipCode4) && Equal
          (export.SendTo.ZipCode3, export.SendToPrevious.ZipCode3) || Equal
          (export.SendTo.Street1, "Security Block on Address")))
        {
          // M Brown, Sept 2001, PR# 127609
          if (Equal(export.HiddenPrevious.Command, "UPDATE"))
          {
            // : The user pressed PF6 twice in a row instead of confirming with 
            // PF19.
            ExitState = "FN0000_CRRU_VERIFY_NAME_AND_ADDR";
            local.ProtectFields.Flag = "Y";

            break;
          }

          ExitState = "INVALID_UPDATE";

          return;
        }

        // 09/24/01        MBrown        127609
        // User must confirm if payee name was changed.
        if (!Equal(export.ReceiptRefund.PayeeName,
          export.PreviousReceiptRefund.PayeeName))
        {
          local.ConfirmNeeded.Flag = "Y";
        }

        // *****
        // Determine which address to associate to the RECEIPT REFUND
        // *****
        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        // ReSet Blocked Values for Updating
        // 09/24/01        MBrown        127609
        // FYI - this logic is also in update within case confirm.
        if (IsEmpty(export.SendTo.Street1) && IsEmpty
          (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
          (export.SendTo.State))
        {
        }
        else
        {
          // 09/24/01        MBrown        127609
          // User must confirm if payee address was changed.
          local.Validate.Assign(export.SendTo);
          local.NewAddress.Flag = "Y";
          local.ConfirmNeeded.Flag = "Y";
        }

        // 09/24/01        MBrown        127609
        // Added the following 'if' statment so that address edit is done
        // only if address was changed.
        if (AsChar(local.NewAddress.Flag) == 'Y')
        {
          // *****
          // Determine if the address is complete
          // *****
          if (IsEmpty(local.Validate.Street1) && IsEmpty
            (local.Validate.Street2) || IsEmpty(local.Validate.City) || IsEmpty
            (local.Validate.ZipCode5) || IsEmpty(local.Validate.State))
          {
            var field8 = GetField(export.SendTo, "city");

            field8.Error = true;

            var field9 = GetField(export.SendTo, "state");

            field9.Error = true;

            var field10 = GetField(export.SendTo, "street1");

            field10.Error = true;

            var field11 = GetField(export.SendTo, "street2");

            field11.Error = true;

            var field12 = GetField(export.SendTo, "zipCode3");

            field12.Error = true;

            var field13 = GetField(export.SendTo, "zipCode4");

            field13.Error = true;

            var field14 = GetField(export.SendTo, "zipCode5");

            field14.Error = true;

            ExitState = "ADDRESS_INCOMPLETE";

            return;
          }
        }

        // 09/24/01        MBrown        127609
        // FYI - this logic is also in update within case confirm.
        if (export.ReceiptRefund.Amount != export.PreviousReceiptRefund.Amount)
        {
          local.Difference.Amount = export.ReceiptRefund.Amount - export
            .PreviousReceiptRefund.Amount;
        }

        // 09/24/01        MBrown        127609
        // Added the following 'if' statment so that this edit is done
        // only if address was changed.
        if (AsChar(local.NewAddress.Flag) == 'Y')
        {
          // *****
          // Verify entered State code
          // *****
          local.Pass.Cdvalue = local.Validate.State;
          local.Code.CodeName = "STATE CODE";
          UseCabValidateCodeValue();

          if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
          {
            var field = GetField(export.SendTo, "state");

            field.Error = true;

            ExitState = "CODE_NF";

            return;
          }
        }

        // *** Add logic to ensure that the note for warrant stub is populated.
        // Sunya Sharp 2/26/1999 ***
        if (IsEmpty(export.ReceiptRefund.ReasonText))
        {
          var field = GetField(export.ReceiptRefund, "reasonText");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

          return;
        }

        // 11/17/00  PPhinney   00106654 Prevent REFUNDS in Excess of Available
        // Funds
        local.AvailiableForRefund.Amount =
          export.CashReceiptDetail.ReceivedAmount - export
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

        if (local.AvailiableForRefund.Amount < export.ReceiptRefund.Amount)
        {
          var field8 = GetField(export.AvailableForRefund, "amount");

          field8.Error = true;

          var field9 = GetField(export.ReceiptRefund, "amount");

          field9.Error = true;

          ExitState = "FN0000_EXCESSIVE_REFUND_RB";

          return;
        }

        if (AsChar(local.ConfirmNeeded.Flag) == 'Y')
        {
          // : Payee name and/or address changed - user needs to verify.  The 
          // update
          // logic will be performed in the case of confirm command.
          export.HiddenPrevious.Command = global.Command;

          // 09/24/01  MBrown  PR# 127609: Added a flag for protection of fields
          //  at the end of the pstep.
          local.ProtectFields.Flag = "Y";
          export.Confirm.Flag = "Y";
          export.HiddenPrevious.Command = global.Command;
          export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
          export.SendToPrevious.Assign(export.SendTo);
          ExitState = "FN0000_CRRU_VERIFY_NAME_AND_ADDR";

          break;
        }

        // *****
        // Update the Receipt Refund
        // *****
        UseUpdateRefundedCollection();

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        export.SendToHidden.Assign(export.SendTo);
        export.CurrentHidden.Assign(export.Current);

        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
          // * *   Security Authorized - Allow Display
        }
        else
        {
          // * *   Security NOT Authorized - Do NOT Allow Display
          if (IsEmpty(export.SendTo.Street1) && IsEmpty
            (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
            (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
          {
            // * *   Nothing Returned - Display and Hidden are BLANK
          }
          else
          {
            // * *   Security NOT Authorized - Display SECURITY message
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.SendTo);
            export.SendTo.Street1 = "Security Block on Address";
          }

          if (IsEmpty(export.Current.Street1) && IsEmpty
            (export.Current.Street2) && IsEmpty(export.Current.City) && IsEmpty
            (export.Current.State) && IsEmpty(export.Current.ZipCode5))
          {
            // * *   Nothing Returned - Display and Hidden are BLANK
          }
          else
          {
            // * *   Security NOT Authorized - Display SECURITY message
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.Current);
            export.Current.Street1 = "Security Block on Address";
          }
        }

        // *****
        // Validate Exitstate
        // *****
        if (IsExitState("CO0000_CASH_REC_DTL_ADD_AE") || IsExitState
          ("CO0000_CASH_REC_DTL_ADD_PV"))
        {
          var field8 = GetField(export.SendTo, "street1");

          field8.Error = true;

          var field9 = GetField(export.SendTo, "street2");

          field9.Error = true;

          var field10 = GetField(export.SendTo, "state");

          field10.Error = true;

          var field11 = GetField(export.SendTo, "city");

          field11.Error = true;

          var field12 = GetField(export.SendTo, "zipCode5");

          field12.Error = true;

          var field13 = GetField(export.SendTo, "zipCode4");

          field13.Error = true;

          var field14 = GetField(export.SendTo, "zipCode3");

          field14.Error = true;

          return;
        }
        else if (IsExitState("FN0000_RCPT_REFUND_PV") || IsExitState
          ("FN0000_RCPT_REFUND_NF") || IsExitState("FN0000_RCPT_REFUND_NU"))
        {
          var field8 = GetField(export.ReceiptRefund, "amount");

          field8.Error = true;

          var field9 = GetField(export.ReceiptRefund, "payeeName");

          field9.Error = true;

          var field10 = GetField(export.ReceiptRefund, "reasonCode");

          field10.Error = true;

          var field11 = GetField(export.ReceiptRefund, "reasonText");

          field11.Error = true;

          var field12 = GetField(export.ReceiptRefund, "taxid");

          field12.Error = true;

          var field13 = GetField(export.ReceiptRefund, "offsetTaxYear");

          field13.Error = true;

          var field14 = GetField(export.ReceiptRefund, "requestDate");

          field14.Error = true;

          if (IsExitState("FN0000_RCPT_REFUND_NF"))
          {
            ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
          }

          return;
        }
        else if (IsExitState("FN0054_CASH_RCPT_DTL_NU") || IsExitState
          ("FN0056_CASH_RCPT_DTL_PV"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }
        else if (IsExitState("FN0000_RFND_AMT_EXCEED_AVAILABLE"))
        {
          var field = GetField(export.ReceiptRefund, "amount");

          field.Error = true;

          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.AvailableForRefund.Amount =
          export.CashReceiptDetail.ReceivedAmount - export
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
        MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
          export.SendTo);
        export.ReceiptRefund.CreatedBy = global.UserId;
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        export.Confirm.Flag = "N";

        // *****
        // Require User to display before updating
        // *****
        if (AsChar(export.DisplayComplete.Flag) != 'Y')
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";

          return;
        }

        // *** If the refund was created earlier than current date do not allow 
        // the refund to be updated.  Sunya Sharp 2/27/1999 ***
        if (Lt(export.ReceiptRefund.RequestDate, Now().Date) && Lt
          (local.NullReceiptRefund.CreatedTimestamp,
          export.ReceiptRefund.CreatedTimestamp))
        {
          var field = GetField(export.PaymentStatus, "code");

          field.Error = true;

          ExitState = "FN0000_CANT_UPD_OR_DEL_REFUND";

          return;
        }

        // *****
        // Delete the Refund
        // *****
        UseDeletedRefundedCollection();

        // *****
        // Exitstate validation.
        // *****
        if (IsExitState("FN0000_PMT_REQ_EXISTS_CANNOT_DEL"))
        {
          return;
        }
        else if (IsExitState("FN0052_CASH_RCPT_DTL_NF") || IsExitState
          ("FN0054_CASH_RCPT_DTL_NU") || IsExitState
          ("FN0056_CASH_RCPT_DTL_PV"))
        {
          var field = GetField(export.CashReceipt, "sequentialNumber");

          field.Error = true;

          return;
        }
        else if (IsExitState("FN0000_RCPT_REFUND_NF"))
        {
          var field8 = GetField(export.ReceiptRefund, "amount");

          field8.Error = true;

          var field9 = GetField(export.ReceiptRefund, "offsetTaxYear");

          field9.Error = true;

          var field10 = GetField(export.ReceiptRefund, "payeeName");

          field10.Error = true;

          var field11 = GetField(export.ReceiptRefund, "reasonCode");

          field11.Error = true;

          var field12 = GetField(export.ReceiptRefund, "requestDate");

          field12.Error = true;

          var field13 = GetField(export.ReceiptRefund, "taxid");

          field13.Error = true;

          var field14 = GetField(export.ReceiptRefund, "reasonText");

          field14.Error = true;

          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *** Need to redisplay after delete to reset the amount fields and 
        // clear the views.  Sunya Sharp 2/23/1999 ***
        UseFnAbDisplayReceiptRefund2();

        // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to see 
        // the Address
        export.CurrentHidden.Street1 = export.Current.Street1;
        export.CurrentHidden.Street2 = export.Current.Street2 ?? "";
        export.CurrentHidden.City = export.Current.City;
        export.CurrentHidden.State = export.Current.State;
        export.CurrentHidden.ZipCode5 = export.Current.ZipCode5;
        export.CurrentHidden.ZipCode4 = export.Current.ZipCode4 ?? "";
        export.CurrentHidden.ZipCode3 = export.Current.ZipCode3 ?? "";

        if (AsChar(export.DisplayAddress.Flag) == 'Y')
        {
        }
        else
        {
          MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
            export.Current);
          export.Current.Street1 = "Security Block on Address";
        }

        // *****
        // Validate Exitstate upon return from the read CAB
        // *****
        if (IsExitState("FN0000_RCPT_REFUND_NF"))
        {
          ExitState = "FN0000_VALID_RFND_NF";

          return;
        }
        else if (IsExitState("FN0000_CASH_RECEIPT_DETAIL_NF"))
        {
          var field8 = GetField(export.CashReceiptDetail, "collectionAmount");

          field8.Error = true;

          var field9 = GetField(export.CashReceiptDetail, "collectionDate");

          field9.Error = true;

          var field10 =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field10.Error = true;

          var field11 = GetField(export.CashReceipt, "sequentialNumber");

          field11.Error = true;

          return;
        }
        else if (IsExitState("FN0000_CRD_AND_REFUND_NOT_ENTERD"))
        {
          ExitState = "FN0000_MUST_ENTER_CRD_OR_REFUND";

          return;
        }
        else if (IsExitState("FN0000_ENTER_CRD_OR_REFUND"))
        {
          return;
        }
        else if (IsExitState("FN0000_PROTECT_RECEIPT_REFUND"))
        {
          // *** No longer protecting fields.  There were problems with the add 
          // and update process.  The user requested it be removed.  Sunya 3/2/
          // 1999 ***
        }
        else
        {
        }

        if (Equal(export.ReceiptRefund.CreatedTimestamp,
          local.NullReceiptRefund.CreatedTimestamp))
        {
          export.ReceiptRefund.Taxid =
            export.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
          export.ReceiptRefund.Amount =
            export.CashReceiptDetail.ReceivedAmount - (
              export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + export
            .CashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        export.AvailableForRefund.Amount =
          export.CashReceiptDetail.ReceivedAmount - export
          .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
          .CashReceiptDetail.RefundedAmount.GetValueOrDefault();

        // 11/17/00  PPhinney   00106654 Prevent REFUNDS in Excess of Available
        // Funds
        // This is on a DELETE/Display after - DO NOT stop the Delete
        // - just highlight so user will notice problem
        if (export.AvailableForRefund.Amount < 0)
        {
          var field = GetField(export.AvailableForRefund, "amount");

          field.Error = true;
        }

        if (export.CashReceiptDetail.RefundedAmount.GetValueOrDefault() < 0)
        {
          var field = GetField(export.CashReceiptDetail, "refundedAmount");

          field.Error = true;
        }

        export.CashReceiptDetailStatus.Code =
          local.CashReceiptDetailStatus.Code;
        export.DisplayComplete.Flag = "Y";
        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "CRRL":
        export.Confirm.Flag = "N";

        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        // *** Removing logic that loses the detail information if nothing is 
        // selected on CRRL.  Also passing only the taxid to CRRL per request of
        // user.  Sunya Sharp 2/18/1999 ***
        export.PassReceiptRefund.Taxid = export.ReceiptRefund.Taxid ?? "";
        export.PassTextWorkArea.Text4 = "CRRU";
        ExitState = "ECO_XFR_TO_LIST_UNDISP_RFND";

        return;
      case "LIST_COL":
        export.Confirm.Flag = "N";

        // for the cases where you link from 1 procedure to another procedure, 
        // you must set the export_hidden security link_indicator to "L".
        // this will tell the called procedure that we are on a link and not a 
        // transfer.  Don't forget to do the view matching on the dialog design
        // screen.
        // ****
        // *** Removed logic that cleared out the detail and/or refund displayed
        // before flow to CRDL. ***
        ExitState = "ECO_LNK_LST_COLLECTIONS";

        return;
      case "CONFIRM":
        // 09/24/01  MBrown  PR# 127609: Major changes to Confirm logic.
        // Added confirm function for add, and for update when the payee name
        // or address is changed.
        // There was already confirm logic for a non cash refund during add 
        // processing,
        // and it has been expanded to include confirm for all adds and updates 
        // when required.
        // --------------------------------------------------------------
        // PF19 Confirm
        // Non Cash collection refund confirmed.  Add the Refund.
        // --------------------------------------------------------------
        // --------------------------------------------------------------
        // No changes can occur before add or update on a confirm.
        // --------------------------------------------------------------
        // H00102727 HERE
        if (export.ReceiptRefund.Amount != export
          .PreviousReceiptRefund.Amount || !
          Equal(export.ReceiptRefund.PayeeName,
          export.PreviousReceiptRefund.PayeeName) || !
          Equal(export.ReceiptRefund.ReasonCode,
          export.PreviousReceiptRefund.ReasonCode) || !
          Equal(export.ReceiptRefund.ReasonText,
          export.PreviousReceiptRefund.ReasonText) || !
          Equal(export.ReceiptRefund.Taxid, export.PreviousReceiptRefund.Taxid) ||
          !Equal(export.SendTo.Street1, export.SendToPrevious.Street1) || !
          Equal(export.SendTo.Street2, export.SendToPrevious.Street2) || !
          Equal(export.SendTo.City, export.SendToPrevious.City) || !
          Equal(export.SendTo.State, export.SendToPrevious.State) || !
          Equal(export.SendTo.ZipCode5, export.SendToPrevious.ZipCode5) || !
          Equal(export.SendTo.ZipCode4, export.SendToPrevious.ZipCode4) || !
          Equal(export.SendTo.ZipCode3, export.SendToPrevious.ZipCode3))
        {
          if (Equal(export.HiddenPrevious.Command, "UPDATE"))
          {
            ExitState = "FN0000_CHANGE_SINCE_UPDATE";
          }
          else if (Equal(export.HiddenPrevious.Command, "ADD"))
          {
            ExitState = "FN0000_CHANGE_SINCE_ADD";
          }

          return;
        }

        switch(TrimEnd(export.HiddenPrevious.Command))
        {
          case "ADD":
            export.SendToPrevious.Assign(local.NullCashReceiptDetailAddress);
            export.PreviousReceiptRefund.RequestDate =
              local.NullReceiptRefund.RequestDate;

            // *****
            // Determine whether or not PF5 has been pressed prior to confirm 
            // being pressed.
            // *****
            if (AsChar(export.Confirm.Flag) == 'Y')
            {
              export.Confirm.Flag = "N";
            }
            else if (Lt(local.NullReceiptRefund.CreatedTimestamp,
              export.ReceiptRefund.CreatedTimestamp))
            {
              ExitState = "FN0000_CONFIRM_NOT_NEEDED";

              return;
            }
            else
            {
              if (AsChar(export.CashReceiptType.CategoryIndicator) == 'N')
              {
                export.Confirm.Flag = "Y";
                local.ProtectFields.Flag = "Y";
                export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
                export.SendToPrevious.Assign(export.SendTo);
                ExitState = "FN0000_NONCASH_COL_REFUND_CONFRM";

                goto Test;
              }
              else
              {
                ExitState = "FN0000_MUST_ADD_BEFORE_CONFIRM";
              }

              return;
            }

            // *****
            // Determine which address to associate to the RECEIPT REFUND
            // *****
            // 09/24/01        MBrown        127609
            // moved this from case add area.
            // ReSet Blocked Values for Checking and Updating
            if (Equal(export.SendTo.Street1, "Security Block on Address"))
            {
              export.SendTo.Assign(export.SendToHidden);
            }

            if (Equal(export.Current.Street1, "Security Block on Address"))
            {
              export.Current.Assign(export.CurrentHidden);
            }

            // *** Change the logic to determine if the cash receipt detail 
            // address is the same as the sent to address.  Sunya Sharp 2/23/
            // 1999 ***
            if (IsEmpty(export.SendTo.Street1) && IsEmpty
              (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
              (export.SendTo.State))
            {
              local.NewAddress.Flag = "";
            }
            else if (Equal(export.SendTo.Street1, export.Current.Street1) && Equal
              (export.SendTo.Street2, export.Current.Street2) && Equal
              (export.SendTo.City, export.Current.City) && Equal
              (export.SendTo.State, export.Current.State) && Equal
              (export.SendTo.ZipCode5, export.Current.ZipCode5))
            {
              local.NewAddress.Flag = "";
            }
            else
            {
              local.NewAddress.Flag = "Y";
            }

            // *** PR# 00079394  Add command that is needed to bypass not 
            // allowing a suspended detail to be refunded.  This is only used in
            // the distribution batch process.  Sunya Sharp 11/5/1999 ***
            export.ReceiptRefund.RequestDate = Now().Date;
            local.AllowSuspendRefund.Command = "DISTIB";
            UseRefundUndistributedCollection();

            // 09/24/01  MBrown  PR# 127609
            export.Confirm.Flag = "N";

            // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to 
            // see the Address
            export.SendToHidden.Assign(export.SendTo);
            export.CurrentHidden.Assign(export.Current);

            if (AsChar(export.DisplayAddress.Flag) == 'Y')
            {
              // * *   Security Authorized - Allow Display
            }
            else
            {
              // * *   Security NOT Authorized - Do NOT Allow Display
              if (IsEmpty(export.SendTo.Street1) && IsEmpty
                (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
                (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.SendTo);
                export.SendTo.Street1 = "Security Block on Address";
              }

              if (IsEmpty(export.Current.Street1) && IsEmpty
                (export.Current.Street2) && IsEmpty(export.Current.City) && IsEmpty
                (export.Current.State) && IsEmpty(export.Current.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.Current);
                export.Current.Street1 = "Security Block on Address";
              }
            }

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (AsChar(local.NewAddress.Flag) == 'Y')
              {
                export.Current.Assign(export.SendTo);
                export.CurrentHidden.Assign(export.SendToHidden);
              }

              MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                export.SendTo);
              export.AvailableForRefund.Amount =
                export.CashReceiptDetail.ReceivedAmount - export
                .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
                .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
              export.CashReceiptDetailStatus.Code = "REF";
            }
            else if (IsExitState("FN0039_CASH_RCPT_DTL_ADDR_NF"))
            {
              var field8 = GetField(export.Current, "street1");

              field8.Error = true;

              var field9 = GetField(export.Current, "street2");

              field9.Error = true;

              var field10 = GetField(export.Current, "zipCode3");

              field10.Error = true;

              var field11 = GetField(export.Current, "zipCode4");

              field11.Error = true;

              var field12 = GetField(export.Current, "city");

              field12.Error = true;

              var field13 = GetField(export.Current, "zipCode5");

              field13.Error = true;

              var field14 = GetField(export.Current, "state");

              field14.Error = true;

              return;
            }
            else if (IsExitState("FN0000_RFND_AMT_EXCEED_AVAILABLE"))
            {
              var field = GetField(export.ReceiptRefund, "amount");

              field.Error = true;

              return;
            }
            else if (IsExitState("CSE_PERSON_NF"))
            {
              var field = GetField(export.RefundToCsePerson, "number");

              field.Error = true;

              return;
            }
            else if (IsExitState("CO0000_CASH_REC_DTL_ADD_AE"))
            {
              var field = GetField(export.CashReceipt, "sequentialNumber");

              field.Error = true;

              return;
            }
            else if (IsExitState("CO0000_CASH_REC_DTL_ADD_PV"))
            {
              var field = GetField(export.CashReceipt, "sequentialNumber");

              field.Error = true;

              return;
            }
            else if (IsExitState("FN0052_CASH_RCPT_DTL_NF"))
            {
              var field = GetField(export.CashReceipt, "sequentialNumber");

              field.Error = true;

              return;
            }
            else if (IsExitState("FN0000_RCPT_REFUND_AE"))
            {
              var field8 = GetField(export.ReceiptRefund, "amount");

              field8.Error = true;

              var field9 = GetField(export.ReceiptRefund, "payeeName");

              field9.Error = true;

              var field10 = GetField(export.ReceiptRefund, "reasonCode");

              field10.Error = true;

              var field11 = GetField(export.ReceiptRefund, "reasonText");

              field11.Error = true;

              var field12 = GetField(export.ReceiptRefund, "taxid");

              field12.Error = true;

              var field13 = GetField(export.ReceiptRefund, "offsetTaxYear");

              field13.Error = true;

              var field14 = GetField(export.ReceiptRefund, "requestDate");

              field14.Error = true;

              return;
            }
            else if (IsExitState("FN0000_RCPT_REFUND_PV"))
            {
              var field8 = GetField(export.ReceiptRefund, "amount");

              field8.Error = true;

              var field9 = GetField(export.ReceiptRefund, "payeeName");

              field9.Error = true;

              var field10 = GetField(export.ReceiptRefund, "reasonCode");

              field10.Error = true;

              var field11 = GetField(export.ReceiptRefund, "reasonText");

              field11.Error = true;

              var field12 = GetField(export.ReceiptRefund, "taxid");

              field12.Error = true;

              var field13 = GetField(export.ReceiptRefund, "offsetTaxYear");

              field13.Error = true;

              var field14 = GetField(export.ReceiptRefund, "requestDate");

              field14.Error = true;

              return;
            }
            else
            {
              return;
            }

            if (AsChar(export.CashReceiptType.CategoryIndicator) == 'N')
            {
              ExitState = "FN0000_CONFIRM_AND_ADD";
            }
            else
            {
              ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
            }

            break;
          case "UPDATE":
            // *****
            // Update the Receipt Refund
            // *****
            // : Set local fields used by the update process.
            if (IsEmpty(export.SendTo.Street1) && IsEmpty
              (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
              (export.SendTo.State))
            {
            }
            else
            {
              local.NewAddress.Flag = "Y";
            }

            if (export.ReceiptRefund.Amount != export
              .PreviousReceiptRefund.Amount)
            {
              local.Difference.Amount = export.ReceiptRefund.Amount - export
                .PreviousReceiptRefund.Amount;
            }

            UseUpdateRefundedCollection();

            // 09/24/01  MBrown  PR# 127609
            export.Confirm.Flag = "N";

            // 09/12/00   PPhinney   H00102727       0nly allow CRU workers to 
            // see the Address
            export.SendToHidden.Assign(export.SendTo);
            export.CurrentHidden.Assign(export.Current);

            if (AsChar(export.DisplayAddress.Flag) == 'Y')
            {
              // * *   Security Authorized - Allow Display
            }
            else
            {
              // * *   Security NOT Authorized - Do NOT Allow Display
              if (IsEmpty(export.SendTo.Street1) && IsEmpty
                (export.SendTo.Street2) && IsEmpty(export.SendTo.City) && IsEmpty
                (export.SendTo.State) && IsEmpty(export.SendTo.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.SendTo);
                export.SendTo.Street1 = "Security Block on Address";
              }

              if (IsEmpty(export.Current.Street1) && IsEmpty
                (export.Current.Street2) && IsEmpty(export.Current.City) && IsEmpty
                (export.Current.State) && IsEmpty(export.Current.ZipCode5))
              {
                // * *   Nothing Returned - Display and Hidden are BLANK
              }
              else
              {
                // * *   Security NOT Authorized - Display SECURITY message
                MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
                  export.Current);
                export.Current.Street1 = "Security Block on Address";
              }
            }

            // *****
            // Validate Exitstate
            // *****
            if (IsExitState("CO0000_CASH_REC_DTL_ADD_AE") || IsExitState
              ("CO0000_CASH_REC_DTL_ADD_PV"))
            {
              var field8 = GetField(export.SendTo, "street1");

              field8.Error = true;

              var field9 = GetField(export.SendTo, "street2");

              field9.Error = true;

              var field10 = GetField(export.SendTo, "state");

              field10.Error = true;

              var field11 = GetField(export.SendTo, "city");

              field11.Error = true;

              var field12 = GetField(export.SendTo, "zipCode5");

              field12.Error = true;

              var field13 = GetField(export.SendTo, "zipCode4");

              field13.Error = true;

              var field14 = GetField(export.SendTo, "zipCode3");

              field14.Error = true;

              return;
            }
            else if (IsExitState("FN0000_RCPT_REFUND_PV") || IsExitState
              ("FN0000_RCPT_REFUND_NF") || IsExitState
              ("FN0000_RCPT_REFUND_NU"))
            {
              var field8 = GetField(export.ReceiptRefund, "amount");

              field8.Error = true;

              var field9 = GetField(export.ReceiptRefund, "payeeName");

              field9.Error = true;

              var field10 = GetField(export.ReceiptRefund, "reasonCode");

              field10.Error = true;

              var field11 = GetField(export.ReceiptRefund, "reasonText");

              field11.Error = true;

              var field12 = GetField(export.ReceiptRefund, "taxid");

              field12.Error = true;

              var field13 = GetField(export.ReceiptRefund, "offsetTaxYear");

              field13.Error = true;

              var field14 = GetField(export.ReceiptRefund, "requestDate");

              field14.Error = true;

              if (IsExitState("FN0000_RCPT_REFUND_NF"))
              {
                ExitState = "ACO_NE0000_DISPLAY_BEFORE_AUD";
              }

              return;
            }
            else if (IsExitState("FN0054_CASH_RCPT_DTL_NU") || IsExitState
              ("FN0056_CASH_RCPT_DTL_PV"))
            {
              var field = GetField(export.CashReceipt, "sequentialNumber");

              field.Error = true;

              return;
            }
            else if (IsExitState("FN0000_RFND_AMT_EXCEED_AVAILABLE"))
            {
              var field = GetField(export.ReceiptRefund, "amount");

              field.Error = true;

              return;
            }
            else if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.AvailableForRefund.Amount =
              export.CashReceiptDetail.ReceivedAmount - export
              .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - export
              .CashReceiptDetail.RefundedAmount.GetValueOrDefault();
            MoveCashReceiptDetailAddress(local.NullCashReceiptDetailAddress,
              export.SendTo);
            export.ReceiptRefund.CreatedBy = global.UserId;
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

            break;
          default:
            ExitState = "ACO_NE0000_INVALID_COMMAND";

            return;
        }

        export.HiddenPrevious.Command = "";

        break;
      case "LTRB":
        // 09/24/01  MBrown  PR# 127609
        export.Confirm.Flag = "N";

        // -------------------------------------------------
        // PF18 LTRB
        // -------------------------------------------------
        if (!IsEmpty(export.RefundToCsePerson.Number))
        {
          ExitState = "FN0000_CANNOT_FLOW_TO_LTRB";

          var field = GetField(export.RefundToCsePerson, "number");

          field.Error = true;

          return;
        }

        // *** Added logic to require the source code to be populated before 
        // flowing to LTRB.  Also added logic to determine data to be passed to
        // LTRB.  If the source code is a state, send to state and nothing in
        // the county.  If the source is a county, send the state as KS and the
        // source code as the county.  This is per user request.  Sunya Sharp 2/
        // 19/1999 ***
        if (IsEmpty(export.RefundToCashReceiptSourceType.Code))
        {
          var field = GetField(export.RefundToCashReceiptSourceType, "code");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }
        else
        {
          // *** PR# 00079637 - Add logic to get the description for the source 
          // code to pass to LTRB.  Sunya Sharp 11/18/1999 ***
          ReadCashReceiptSourceType1();

          if (Equal(export.RefundToCashReceiptSourceType.Code, 4, 5, "STATE"))
          {
            export.PassToLtrb.StateAbbreviation =
              Substring(export.RefundToCashReceiptSourceType.Code, 1, 2);
            export.PassToLtrb.CountyDescription = "";
          }
          else
          {
            export.PassToLtrb.StateAbbreviation = "KS";
            export.PassToLtrb.CountyDescription =
              Substring(entities.CashReceiptSourceType.Name, 1, 4);
          }

          ExitState = "ECO_LNK_TO_LTRB";

          return;
        }

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
      case "":
        // 09/24/01  MBrown  PR# 127609
        export.Confirm.Flag = "N";

        break;
      default:
        // 09/24/01  MBrown  PR# 127609
        export.Confirm.Flag = "N";
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

Test:

    // *****
    // All is successful move exports to previous exports
    // *****
    export.PreviousReceiptRefund.Assign(export.ReceiptRefund);
    export.SendToPrevious.Assign(export.SendTo);
    export.RefundToPreviousCsePerson.Number = export.RefundToCsePerson.Number;
    export.RefundToPreviousCashReceiptSourceType.Code =
      export.RefundToCashReceiptSourceType.Code;

    // Sept, 2001, M. Brown, pr#999999 - moved this protection logic, since it's
    // being done in a lot of places.
    if (AsChar(local.ProtectFields.Flag) == 'Y')
    {
      // ---- Protect all fields, so that no changes are possible before 
      // confirmation ----
      var field1 = GetField(export.ReceiptRefund, "amount");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.ReceiptRefund, "payeeName");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.ReceiptRefund, "requestDate");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.ReceiptRefund, "taxid");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.ReceiptRefund, "taxIdSuffix");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.RefundToCsePerson, "number");

      field6.Color = "cyan";
      field6.Protected = true;

      var field7 = GetField(export.RefundToCashReceiptSourceType, "code");

      field7.Color = "cyan";
      field7.Protected = true;

      var field8 = GetField(export.SendTo, "street1");

      field8.Color = "cyan";
      field8.Protected = true;

      var field9 = GetField(export.SendTo, "street2");

      field9.Color = "cyan";
      field9.Protected = true;

      var field10 = GetField(export.SendTo, "city");

      field10.Color = "cyan";
      field10.Protected = true;

      var field11 = GetField(export.SendTo, "state");

      field11.Color = "cyan";
      field11.Protected = true;

      var field12 = GetField(export.SendTo, "zipCode5");

      field12.Color = "cyan";
      field12.Protected = true;

      var field13 = GetField(export.SendTo, "zipCode4");

      field13.Color = "cyan";
      field13.Protected = true;

      var field14 = GetField(export.SendTo, "zipCode3");

      field14.Color = "cyan";
      field14.Protected = true;

      var field15 = GetField(export.ReceiptRefund, "reasonCode");

      field15.Color = "cyan";
      field15.Protected = true;

      var field16 = GetField(export.ReceiptRefund, "reasonText");

      field16.Color = "cyan";
      field16.Protected = true;
    }
  }

  private static void MoveCashReceiptDetailAddress(
    CashReceiptDetailAddress source, CashReceiptDetailAddress target)
  {
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode5 = source.ZipCode5;
    target.ZipCode4 = source.ZipCode4;
    target.ZipCode3 = source.ZipCode3;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Id = source.Id;
    target.Cdvalue = source.Cdvalue;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
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
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MoveReceiptRefund1(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Taxid = source.Taxid;
    target.PayeeName = source.PayeeName;
    target.Amount = source.Amount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.RequestDate = source.RequestDate;
    target.ReasonText = source.ReasonText;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveReceiptRefund2(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.Amount = source.Amount;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveReceiptRefund3(ReceiptRefund source,
    ReceiptRefund target)
  {
    target.RequestDate = source.RequestDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Pass.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseDeletedRefundedCollection()
  {
    var useImport = new DeletedRefundedCollection.Import();
    var useExport = new DeletedRefundedCollection.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    MoveReceiptRefund2(export.ReceiptRefund, useImport.ReceiptRefund);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(DeletedRefundedCollection.Execute, useImport, useExport);

    local.CashReceiptDetailStatus.Code = useExport.PreviousStatus.Code;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnAbDisplayReceiptRefund1()
  {
    var useImport = new FnAbDisplayReceiptRefund.Import();
    var useExport = new FnAbDisplayReceiptRefund.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.ReceiptRefund.CreatedTimestamp =
      export.ReceiptRefund.CreatedTimestamp;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbDisplayReceiptRefund.Execute, useImport, useExport);

    export.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
    MoveCsePersonsWorkSet2(useExport.CashReceivedFromCsePersonsWorkSet,
      export.CashReceivedFrom);
    export.RefundToCashReceiptSourceType.Code = useExport.RefundTo.Code;
    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    export.CollectionType.Code = useExport.CollectionType.Code;
    export.RefundToCsePerson.Number = useExport.CsePerson.Number;
    export.CashReceiptType.Assign(useExport.CashReceiptType);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveReceiptRefund1(useExport.ReceiptRefund, export.ReceiptRefund);
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    export.Current.Assign(useExport.CashReceiptDetailAddress);
    export.PaymentRequest.Assign(useExport.PaymentRequest);
    local.CashReceiptDetailStatus.Code = useExport.CashReceiptDetailStatus.Code;
  }

  private void UseFnAbDisplayReceiptRefund2()
  {
    var useImport = new FnAbDisplayReceiptRefund.Import();
    var useExport = new FnAbDisplayReceiptRefund.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbDisplayReceiptRefund.Execute, useImport, useExport);

    export.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
    MoveCsePersonsWorkSet2(useExport.CashReceivedFromCsePersonsWorkSet,
      export.CashReceivedFrom);
    export.RefundToCashReceiptSourceType.Code = useExport.RefundTo.Code;
    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    export.CollectionType.Code = useExport.CollectionType.Code;
    export.RefundToCsePerson.Number = useExport.CsePerson.Number;
    export.CashReceiptType.Assign(useExport.CashReceiptType);
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
    MoveCashReceiptSourceType(useExport.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveReceiptRefund1(useExport.ReceiptRefund, export.ReceiptRefund);
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    export.Current.Assign(useExport.CashReceiptDetailAddress);
    export.PaymentRequest.Assign(useExport.PaymentRequest);
  }

  private void UseFnCabReadCsePersonAddress()
  {
    var useImport = new FnCabReadCsePersonAddress.Import();
    var useExport = new FnCabReadCsePersonAddress.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(FnCabReadCsePersonAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private void UseFnReadCsePersonUsingSsnO()
  {
    var useImport = new FnReadCsePersonUsingSsnO.Import();
    var useExport = new FnReadCsePersonUsingSsnO.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
      

    Call(FnReadCsePersonUsingSsnO.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseRefundUndistributedCollection()
  {
    var useImport = new RefundUndistributedCollection.Import();
    var useExport = new RefundUndistributedCollection.Export();

    useImport.NewAddress.Flag = local.NewAddress.Flag;
    useImport.RefundToCashReceiptSourceType.Code =
      export.RefundToCashReceiptSourceType.Code;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);
    useImport.RefundToCsePerson.Number = export.RefundToCsePerson.Number;
    useImport.CashReceiptDetailAddress.Assign(export.Current);
    useImport.SendTo.Assign(export.SendTo);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.FromDistributionProcess.Command =
      local.AllowSuspendRefund.Command;

    Call(RefundUndistributedCollection.Execute, useImport, useExport);

    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    export.CashReceiptDetail.Assign(useExport.CashReceiptDetail);
    MoveReceiptRefund1(useExport.ReceiptRefund, export.ReceiptRefund);
    export.RefundToCsePerson.Number = useExport.CsePerson.Number;
    export.Current.Assign(useExport.CashReceiptDetailAddress);
    export.SendTo.Assign(useExport.SendTo);
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

    useImport.CsePersonsWorkSet.Number = import.PassCsePersonsWorkSet.Number;
    useImport.CsePerson.Number = import.CsePerson2.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = import.PassCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseUpdateRefundedCollection()
  {
    var useImport = new UpdateRefundedCollection.Import();
    var useExport = new UpdateRefundedCollection.Export();

    useImport.Difference.Amount = local.Difference.Amount;
    useImport.AddressChanged.Flag = local.NewAddress.Flag;
    useImport.ReceiptRefund.Assign(export.ReceiptRefund);
    useImport.CashReceiptDetailAddress.Assign(export.Current);
    useImport.SendTo.Assign(export.SendTo);

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;

    Call(UpdateRefundedCollection.Execute, useImport, useExport);

    export.Current.Assign(useExport.CashReceiptDetailAddress);
    export.CashReceiptDetail.RefundedAmount =
      useExport.CashReceiptDetail.RefundedAmount;
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.
          SetString(command, "code", export.RefundToCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.ReturnedFromFlowCashReceiptSourceType.
            SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.Name = db.GetString(reader, 2);
        entities.CashReceiptSourceType.State = db.GetNullableInt32(reader, 3);
        entities.CashReceiptSourceType.County = db.GetNullableInt32(reader, 4);
        entities.CashReceiptSourceType.Location =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType3()
  {
    entities.RefundTo.Populated = false;

    return Read("ReadCashReceiptSourceType3",
      (db, command) =>
      {
        db.
          SetString(command, "code", export.RefundToCashReceiptSourceType.Code);
          
      },
      (db, reader) =>
      {
        entities.RefundTo.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.RefundTo.Code = db.GetString(reader, 1);
        entities.RefundTo.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId",
          export.CashReceiptType.SystemGeneratedIdentifier);
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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.RefundToCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 3);
        entities.CsePerson.TaxIdSuffix = db.GetNullableString(reader, 4);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", entities.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 2);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 3);
        entities.FipsTribAddress.City = db.GetString(reader, 4);
        entities.FipsTribAddress.State = db.GetString(reader, 5);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 6);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.TrbId = db.GetNullableInt32(reader, 9);
        entities.FipsTribAddress.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fipState",
          local.CashReceiptSourceType.State.GetValueOrDefault());
        db.SetNullableInt32(
          command, "fipCounty",
          local.CashReceiptSourceType.County.GetValueOrDefault());
        db.SetNullableInt32(
          command, "fipLocation",
          local.CashReceiptSourceType.Location.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.DlgflwSelected.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Name = db.GetString(reader, 0);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 1);
        entities.Tribunal.Identifier = db.GetInt32(reader, 2);
        entities.Tribunal.TaxIdSuffix = db.GetNullableString(reader, 3);
        entities.Tribunal.TaxId = db.GetNullableString(reader, 4);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.Tribunal.Populated = true;
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
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Standard StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of RefundToPreviousCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("refundToPreviousCashReceiptSourceType")]
    public CashReceiptSourceType RefundToPreviousCashReceiptSourceType
    {
      get => refundToPreviousCashReceiptSourceType ??= new();
      set => refundToPreviousCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of RefundToPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("refundToPreviousCsePerson")]
    public CsePerson RefundToPreviousCsePerson
    {
      get => refundToPreviousCsePerson ??= new();
      set => refundToPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("returnedFromFlowCashReceiptEvent")]
    public CashReceiptEvent ReturnedFromFlowCashReceiptEvent
    {
      get => returnedFromFlowCashReceiptEvent ??= new();
      set => returnedFromFlowCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("returnedFromFlowCashReceiptSourceType")]
    public CashReceiptSourceType ReturnedFromFlowCashReceiptSourceType
    {
      get => returnedFromFlowCashReceiptSourceType ??= new();
      set => returnedFromFlowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("returnedFromFlowCashReceiptDetail")]
    public CashReceiptDetail ReturnedFromFlowCashReceiptDetail
    {
      get => returnedFromFlowCashReceiptDetail ??= new();
      set => returnedFromFlowCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowCashReceipt.
    /// </summary>
    [JsonPropertyName("returnedFromFlowCashReceipt")]
    public CashReceipt ReturnedFromFlowCashReceipt
    {
      get => returnedFromFlowCashReceipt ??= new();
      set => returnedFromFlowCashReceipt = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowCashReceiptType.
    /// </summary>
    [JsonPropertyName("returnedFromFlowCashReceiptType")]
    public CashReceiptType ReturnedFromFlowCashReceiptType
    {
      get => returnedFromFlowCashReceiptType ??= new();
      set => returnedFromFlowCashReceiptType = value;
    }

    /// <summary>
    /// A value of ReturnedFromFlowReceiptRefund.
    /// </summary>
    [JsonPropertyName("returnedFromFlowReceiptRefund")]
    public ReceiptRefund ReturnedFromFlowReceiptRefund
    {
      get => returnedFromFlowReceiptRefund ??= new();
      set => returnedFromFlowReceiptRefund = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of CashReceivedFrom.
    /// </summary>
    [JsonPropertyName("cashReceivedFrom")]
    public CsePersonsWorkSet CashReceivedFrom
    {
      get => cashReceivedFrom ??= new();
      set => cashReceivedFrom = value;
    }

    /// <summary>
    /// A value of PreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousCsePersonsWorkSet
    {
      get => previousCsePersonsWorkSet ??= new();
      set => previousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousCodeValue.
    /// </summary>
    [JsonPropertyName("previousCodeValue")]
    public CodeValue PreviousCodeValue
    {
      get => previousCodeValue ??= new();
      set => previousCodeValue = value;
    }

    /// <summary>
    /// A value of PromptReturnToSource.
    /// </summary>
    [JsonPropertyName("promptReturnToSource")]
    public Standard PromptReturnToSource
    {
      get => promptReturnToSource ??= new();
      set => promptReturnToSource = value;
    }

    /// <summary>
    /// A value of DlgflwSelected.
    /// </summary>
    [JsonPropertyName("dlgflwSelected")]
    public Tribunal DlgflwSelected
    {
      get => dlgflwSelected ??= new();
      set => dlgflwSelected = value;
    }

    /// <summary>
    /// A value of RefundTo.
    /// </summary>
    [JsonPropertyName("refundTo")]
    public CashReceiptSourceType RefundTo
    {
      get => refundTo ??= new();
      set => refundTo = value;
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
    /// A value of PassCodeValue.
    /// </summary>
    [JsonPropertyName("passCodeValue")]
    public CodeValue PassCodeValue
    {
      get => passCodeValue ??= new();
      set => passCodeValue = value;
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
    /// A value of RefundReason.
    /// </summary>
    [JsonPropertyName("refundReason")]
    public Standard RefundReason
    {
      get => refundReason ??= new();
      set => refundReason = value;
    }

    /// <summary>
    /// A value of CsePerson1.
    /// </summary>
    [JsonPropertyName("csePerson1")]
    public Standard CsePerson1
    {
      get => csePerson1 ??= new();
      set => csePerson1 = value;
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
    /// A value of AvailableForRefund.
    /// </summary>
    [JsonPropertyName("availableForRefund")]
    public ReceiptRefund AvailableForRefund
    {
      get => availableForRefund ??= new();
      set => availableForRefund = value;
    }

    /// <summary>
    /// A value of Confirm.
    /// </summary>
    [JsonPropertyName("confirm")]
    public Common Confirm
    {
      get => confirm ??= new();
      set => confirm = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CsePerson2.
    /// </summary>
    [JsonPropertyName("csePerson2")]
    public CsePerson CsePerson2
    {
      get => csePerson2 ??= new();
      set => csePerson2 = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CashReceiptDetailAddress Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SendTo.
    /// </summary>
    [JsonPropertyName("sendTo")]
    public CashReceiptDetailAddress SendTo
    {
      get => sendTo ??= new();
      set => sendTo = value;
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
    /// A value of PreviousReceiptRefund.
    /// </summary>
    [JsonPropertyName("previousReceiptRefund")]
    public ReceiptRefund PreviousReceiptRefund
    {
      get => previousReceiptRefund ??= new();
      set => previousReceiptRefund = value;
    }

    /// <summary>
    /// A value of SendToPrevious.
    /// </summary>
    [JsonPropertyName("sendToPrevious")]
    public CashReceiptDetailAddress SendToPrevious
    {
      get => sendToPrevious ??= new();
      set => sendToPrevious = value;
    }

    /// <summary>
    /// A value of DisplayComplete.
    /// </summary>
    [JsonPropertyName("displayComplete")]
    public Common DisplayComplete
    {
      get => displayComplete ??= new();
      set => displayComplete = value;
    }

    /// <summary>
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public Code Passed
    {
      get => passed ??= new();
      set => passed = value;
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
    /// A value of CurrentHidden.
    /// </summary>
    [JsonPropertyName("currentHidden")]
    public CashReceiptDetailAddress CurrentHidden
    {
      get => currentHidden ??= new();
      set => currentHidden = value;
    }

    /// <summary>
    /// A value of SendToHidden.
    /// </summary>
    [JsonPropertyName("sendToHidden")]
    public CashReceiptDetailAddress SendToHidden
    {
      get => sendToHidden ??= new();
      set => sendToHidden = value;
    }

    /// <summary>
    /// A value of DisplayAddrsss.
    /// </summary>
    [JsonPropertyName("displayAddrsss")]
    public Common DisplayAddrsss
    {
      get => displayAddrsss ??= new();
      set => displayAddrsss = value;
    }

    /// <summary>
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public Common HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Standard statePrompt;
    private CashReceiptSourceType refundToPreviousCashReceiptSourceType;
    private CsePerson refundToPreviousCsePerson;
    private CashReceiptEvent returnedFromFlowCashReceiptEvent;
    private CashReceiptSourceType returnedFromFlowCashReceiptSourceType;
    private CashReceiptDetail returnedFromFlowCashReceiptDetail;
    private CashReceipt returnedFromFlowCashReceipt;
    private CashReceiptType returnedFromFlowCashReceiptType;
    private ReceiptRefund returnedFromFlowReceiptRefund;
    private CrdCrComboNo crdCrComboNo;
    private CsePersonsWorkSet cashReceivedFrom;
    private CsePersonsWorkSet previousCsePersonsWorkSet;
    private CodeValue previousCodeValue;
    private Standard promptReturnToSource;
    private Tribunal dlgflwSelected;
    private CashReceiptSourceType refundTo;
    private PaymentStatus paymentStatus;
    private CodeValue passCodeValue;
    private CsePersonsWorkSet passCsePersonsWorkSet;
    private Standard refundReason;
    private Standard csePerson1;
    private CollectionType collectionType;
    private ReceiptRefund availableForRefund;
    private Common confirm;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private ReceiptRefund receiptRefund;
    private CsePerson csePerson2;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailAddress current;
    private CashReceiptDetailAddress sendTo;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund previousReceiptRefund;
    private CashReceiptDetailAddress sendToPrevious;
    private Common displayComplete;
    private Code passed;
    private NextTranInfo hidden;
    private Standard standard;
    private CashReceiptDetailAddress currentHidden;
    private CashReceiptDetailAddress sendToHidden;
    private Common displayAddrsss;
    private Common hiddenPrevious;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of StatePrompt.
    /// </summary>
    [JsonPropertyName("statePrompt")]
    public Standard StatePrompt
    {
      get => statePrompt ??= new();
      set => statePrompt = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of RefundToPreviousCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("refundToPreviousCashReceiptSourceType")]
    public CashReceiptSourceType RefundToPreviousCashReceiptSourceType
    {
      get => refundToPreviousCashReceiptSourceType ??= new();
      set => refundToPreviousCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of RefundToPreviousCsePerson.
    /// </summary>
    [JsonPropertyName("refundToPreviousCsePerson")]
    public CsePerson RefundToPreviousCsePerson
    {
      get => refundToPreviousCsePerson ??= new();
      set => refundToPreviousCsePerson = value;
    }

    /// <summary>
    /// A value of PassToLtrb.
    /// </summary>
    [JsonPropertyName("passToLtrb")]
    public Fips PassToLtrb
    {
      get => passToLtrb ??= new();
      set => passToLtrb = value;
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
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of CashReceivedFrom.
    /// </summary>
    [JsonPropertyName("cashReceivedFrom")]
    public CsePersonsWorkSet CashReceivedFrom
    {
      get => cashReceivedFrom ??= new();
      set => cashReceivedFrom = value;
    }

    /// <summary>
    /// A value of PreviousCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("previousCsePersonsWorkSet")]
    public CsePersonsWorkSet PreviousCsePersonsWorkSet
    {
      get => previousCsePersonsWorkSet ??= new();
      set => previousCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PreviousCodeValue.
    /// </summary>
    [JsonPropertyName("previousCodeValue")]
    public CodeValue PreviousCodeValue
    {
      get => previousCodeValue ??= new();
      set => previousCodeValue = value;
    }

    /// <summary>
    /// A value of PromptReturnToSource.
    /// </summary>
    [JsonPropertyName("promptReturnToSource")]
    public Standard PromptReturnToSource
    {
      get => promptReturnToSource ??= new();
      set => promptReturnToSource = value;
    }

    /// <summary>
    /// A value of RefundToCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("refundToCashReceiptSourceType")]
    public CashReceiptSourceType RefundToCashReceiptSourceType
    {
      get => refundToCashReceiptSourceType ??= new();
      set => refundToCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassTextWorkArea.
    /// </summary>
    [JsonPropertyName("passTextWorkArea")]
    public TextWorkArea PassTextWorkArea
    {
      get => passTextWorkArea ??= new();
      set => passTextWorkArea = value;
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
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of RefundReason.
    /// </summary>
    [JsonPropertyName("refundReason")]
    public Standard RefundReason
    {
      get => refundReason ??= new();
      set => refundReason = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of AvailableForRefund.
    /// </summary>
    [JsonPropertyName("availableForRefund")]
    public ReceiptRefund AvailableForRefund
    {
      get => availableForRefund ??= new();
      set => availableForRefund = value;
    }

    /// <summary>
    /// A value of Confirm.
    /// </summary>
    [JsonPropertyName("confirm")]
    public Common Confirm
    {
      get => confirm ??= new();
      set => confirm = value;
    }

    /// <summary>
    /// A value of PassCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passCashReceiptDetail")]
    public CashReceiptDetail PassCashReceiptDetail
    {
      get => passCashReceiptDetail ??= new();
      set => passCashReceiptDetail = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of RefundToCsePerson.
    /// </summary>
    [JsonPropertyName("refundToCsePerson")]
    public CsePerson RefundToCsePerson
    {
      get => refundToCsePerson ??= new();
      set => refundToCsePerson = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CashReceiptDetailAddress Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of SendTo.
    /// </summary>
    [JsonPropertyName("sendTo")]
    public CashReceiptDetailAddress SendTo
    {
      get => sendTo ??= new();
      set => sendTo = value;
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
    /// A value of PreviousReceiptRefund.
    /// </summary>
    [JsonPropertyName("previousReceiptRefund")]
    public ReceiptRefund PreviousReceiptRefund
    {
      get => previousReceiptRefund ??= new();
      set => previousReceiptRefund = value;
    }

    /// <summary>
    /// A value of SendToPrevious.
    /// </summary>
    [JsonPropertyName("sendToPrevious")]
    public CashReceiptDetailAddress SendToPrevious
    {
      get => sendToPrevious ??= new();
      set => sendToPrevious = value;
    }

    /// <summary>
    /// A value of DisplayComplete.
    /// </summary>
    [JsonPropertyName("displayComplete")]
    public Common DisplayComplete
    {
      get => displayComplete ??= new();
      set => displayComplete = value;
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
    /// A value of CurrentHidden.
    /// </summary>
    [JsonPropertyName("currentHidden")]
    public CashReceiptDetailAddress CurrentHidden
    {
      get => currentHidden ??= new();
      set => currentHidden = value;
    }

    /// <summary>
    /// A value of SendToHidden.
    /// </summary>
    [JsonPropertyName("sendToHidden")]
    public CashReceiptDetailAddress SendToHidden
    {
      get => sendToHidden ??= new();
      set => sendToHidden = value;
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
    /// A value of HiddenPrevious.
    /// </summary>
    [JsonPropertyName("hiddenPrevious")]
    public Common HiddenPrevious
    {
      get => hiddenPrevious ??= new();
      set => hiddenPrevious = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Standard statePrompt;
    private Common phonetic;
    private CashReceiptSourceType refundToPreviousCashReceiptSourceType;
    private CsePerson refundToPreviousCsePerson;
    private Fips passToLtrb;
    private ReceiptRefund passReceiptRefund;
    private CrdCrComboNo crdCrComboNo;
    private CsePersonsWorkSet cashReceivedFrom;
    private CsePersonsWorkSet previousCsePersonsWorkSet;
    private CodeValue previousCodeValue;
    private Standard promptReturnToSource;
    private CashReceiptSourceType refundToCashReceiptSourceType;
    private TextWorkArea passTextWorkArea;
    private PaymentStatus paymentStatus;
    private Code code;
    private Standard refundReason;
    private Standard csePerson;
    private CollectionType collectionType;
    private ReceiptRefund availableForRefund;
    private Common confirm;
    private CashReceiptDetail passCashReceiptDetail;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private ReceiptRefund receiptRefund;
    private CsePerson refundToCsePerson;
    private PaymentRequest paymentRequest;
    private CashReceiptDetailAddress current;
    private CashReceiptDetailAddress sendTo;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund previousReceiptRefund;
    private CashReceiptDetailAddress sendToPrevious;
    private Common displayComplete;
    private NextTranInfo hidden;
    private Standard standard;
    private CashReceiptDetailAddress currentHidden;
    private CashReceiptDetailAddress sendToHidden;
    private Common displayAddress;
    private Common hiddenPrevious;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AvailiableForRefund.
    /// </summary>
    [JsonPropertyName("availiableForRefund")]
    public ReceiptRefund AvailiableForRefund
    {
      get => availiableForRefund ??= new();
      set => availiableForRefund = value;
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
    /// A value of AllowSuspendRefund.
    /// </summary>
    [JsonPropertyName("allowSuspendRefund")]
    public Common AllowSuspendRefund
    {
      get => allowSuspendRefund ??= new();
      set => allowSuspendRefund = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CsePersonAddressExists.
    /// </summary>
    [JsonPropertyName("csePersonAddressExists")]
    public Common CsePersonAddressExists
    {
      get => csePersonAddressExists ??= new();
      set => csePersonAddressExists = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of NoOfRefundTrans.
    /// </summary>
    [JsonPropertyName("noOfRefundTrans")]
    public Common NoOfRefundTrans
    {
      get => noOfRefundTrans ??= new();
      set => noOfRefundTrans = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CodeValue Pass
    {
      get => pass ??= new();
      set => pass = value;
    }

    /// <summary>
    /// A value of NullCashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("nullCashReceiptDetailAddress")]
    public CashReceiptDetailAddress NullCashReceiptDetailAddress
    {
      get => nullCashReceiptDetailAddress ??= new();
      set => nullCashReceiptDetailAddress = value;
    }

    /// <summary>
    /// A value of Difference.
    /// </summary>
    [JsonPropertyName("difference")]
    public ReceiptRefund Difference
    {
      get => difference ??= new();
      set => difference = value;
    }

    /// <summary>
    /// A value of NewAddress.
    /// </summary>
    [JsonPropertyName("newAddress")]
    public Common NewAddress
    {
      get => newAddress ??= new();
      set => newAddress = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public Common ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of NullReceiptRefund.
    /// </summary>
    [JsonPropertyName("nullReceiptRefund")]
    public ReceiptRefund NullReceiptRefund
    {
      get => nullReceiptRefund ??= new();
      set => nullReceiptRefund = value;
    }

    /// <summary>
    /// A value of Validate.
    /// </summary>
    [JsonPropertyName("validate")]
    public CashReceiptDetailAddress Validate
    {
      get => validate ??= new();
      set => validate = value;
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
    /// A value of ProtectFields.
    /// </summary>
    [JsonPropertyName("protectFields")]
    public Common ProtectFields
    {
      get => protectFields ??= new();
      set => protectFields = value;
    }

    /// <summary>
    /// A value of TempSendTo.
    /// </summary>
    [JsonPropertyName("tempSendTo")]
    public CashReceiptDetailAddress TempSendTo
    {
      get => tempSendTo ??= new();
      set => tempSendTo = value;
    }

    /// <summary>
    /// A value of TempCurrent.
    /// </summary>
    [JsonPropertyName("tempCurrent")]
    public CashReceiptDetailAddress TempCurrent
    {
      get => tempCurrent ??= new();
      set => tempCurrent = value;
    }

    /// <summary>
    /// A value of SecurityBlockOnAddr.
    /// </summary>
    [JsonPropertyName("securityBlockOnAddr")]
    public Common SecurityBlockOnAddr
    {
      get => securityBlockOnAddr ??= new();
      set => securityBlockOnAddr = value;
    }

    /// <summary>
    /// A value of ConfirmNeeded.
    /// </summary>
    [JsonPropertyName("confirmNeeded")]
    public Common ConfirmNeeded
    {
      get => confirmNeeded ??= new();
      set => confirmNeeded = value;
    }

    private ReceiptRefund availiableForRefund;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private Common allowSuspendRefund;
    private CsePerson csePerson;
    private CashReceiptSourceType cashReceiptSourceType;
    private Common csePersonAddressExists;
    private DateWorkArea zero;
    private CsePersonAddress csePersonAddress;
    private Common noOfRefundTrans;
    private TextWorkArea textWorkArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CodeValue pass;
    private CashReceiptDetailAddress nullCashReceiptDetailAddress;
    private ReceiptRefund difference;
    private Common newAddress;
    private Common returnCode;
    private Code code;
    private Common error;
    private ReceiptRefund nullReceiptRefund;
    private CashReceiptDetailAddress validate;
    private Standard saveCommand;
    private Common protectFields;
    private CashReceiptDetailAddress tempSendTo;
    private CashReceiptDetailAddress tempCurrent;
    private Common securityBlockOnAddr;
    private Common confirmNeeded;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
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
    /// A value of RefundTo.
    /// </summary>
    [JsonPropertyName("refundTo")]
    public CashReceiptSourceType RefundTo
    {
      get => refundTo ??= new();
      set => refundTo = value;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CashReceiptType cashReceiptType;
    private Fips fips;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType refundTo;
    private Tribunal tribunal;
    private FipsTribAddress fipsTribAddress;
    private CashReceiptSourceType cashReceiptSourceType;
    private PaymentRequest paymentRequest;
    private CashReceiptDetail cashReceiptDetail;
    private ReceiptRefund receiptRefund;
    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
  }
#endregion
}

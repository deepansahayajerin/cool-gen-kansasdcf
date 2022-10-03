// Program: FN_WDTL_LST_WARRANT_DTL, ID: 371869747, model: 746.
// Short name: SWEWDTLP
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
/// A program: FN_WDTL_LST_WARRANT_DTL.
/// </para>
/// <para>
/// This Procedure will list the details of a Warrant or Payment-Request. The 
/// user may input a Warrant Number or may pass a Payment_request number thru a
/// flow to this Procedure.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnWdtlLstWarrantDtl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_WDTL_LST_WARRANT_DTL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnWdtlLstWarrantDtl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnWdtlLstWarrantDtl.
  /// </summary>
  public FnWdtlLstWarrantDtl(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************
    //  Procedure    : FN_LIST WARRANT DETAIL
    //  Developed by : R B MOHAPATRA
    // Change Log :
    //  1. 02-26-96  Added two export views -->
    //               Export_Pass_Thru_Flow cse_person
    //               Export_Pass_Thru_Flow Payment_Request
    //     for setting up the Dialog Flows.
    //  2. 02-26-96  Included the following commands   WAST, DHST, WARA
    //  3. 02-28-96  Two more new commands added by user - PSUM => flows to "
    // List Monthly AR/Payee Summary" screen AND CRRC => flows to "List Cash
    // Receipts" screen
    // 12/16/96	R. Marchman	Add new security/next tran
    //  4. 05/06/97 Alan Samuels
    // Changed Reference Number to include Cash Receipt Date, Cash Receipt 
    // Number, and Cash Receipt Detail Number.
    //  5. 09/05/97 Newman/Parker-DIR  Modified Screen to replace Process Date 
    // with Disbursement Date in detail lines.
    // 6. 12/5/98  Kalpesh Doshi  - Various phase 2 changes as suggested by SMEs
    // 7. 04/06/00 C. Scroggins - Added modifications for family violence 
    // security
    // 8. 05/10/00 K. Doshi - PR93001. Flow from WDTL to CRRC was taking a long 
    // time. Add Obligee Qualification to a READ to use Index on Disb_tran.
    // 9. 01/24/01 K. Doshi - WR 263. Cater for new warrant status 'KPC'.
    // 06/26/2001  K. Doshi - PR# 111410-Add family violence code for DP. Move 
    // FV checks at the end of display processing.
    // 06/26/2001  K. Doshi - WR# 285-Cater for Duplicate warrant #s.
    // 11/28/2001  K. Doshi - WR# 020147-KPC Recoupment. Add 'KPC RCP' and '
    // Interstate' flags.
    // 01/14/2002   K. Cole - PR#118855.  Increased group view for disb detail.
    // 12/03/2014   SWDPLSS - CQ#46048  Increased group views for disb detail 
    // from 150 to 175.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }
    else if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MovePaymentRequest4(import.HiddenPaymentRequest, export.HiddenPaymentRequest);
      
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PaymentRequest.Number = import.PaymentRequest.Number ?? "";

    // **************************************
    // Move the IMPORT VIEWs to EXPORT VIEWs
    // **************************************
    if (!Equal(global.Command, "DISPLAY"))
    {
      export.PaymentRequest.Assign(import.PaymentRequest);
      export.PaymentStatus.Code = import.PaymentStatus.Code;
      export.AddrMailed.Assign(import.AddrMailed);

      // --------------------------------------------------------------
      // KD - 12/3/98
      // Add statement below to MOVE import_mailed_to to export_mailed_to.
      // -------------------------------------------------------------
      export.MailedTo.FormattedName = import.MailedTo.FormattedName;
      MoveCsePersonsWorkSet(import.Payee, export.Payee);
      MoveCsePersonsWorkSet(import.DesigPayee, export.DesigPayee);
      MovePaymentRequest4(import.ReisFrom, export.ReisFrom);
      MovePaymentRequest4(import.ReisTo, export.ReisTo);

      export.Disb.Index = 0;
      export.Disb.Clear();

      for(import.Disb.Index = 0; import.Disb.Index < import.Disb.Count; ++
        import.Disb.Index)
      {
        if (export.Disb.IsFull)
        {
          break;
        }

        export.Disb.Update.DisbDetailDisbursementTransaction.Assign(
          import.Disb.Item.DisbDetailDisbursementTransaction);
        export.Disb.Update.DisbType.Text10 = import.Disb.Item.DisbType.Text10;
        export.Disb.Update.DisbDetailCashReceipt.ReceivedDate =
          import.Disb.Item.DisbDetailCashReceipt.ReceivedDate;
        export.Disb.Update.RefNumber.Text14 = import.Disb.Item.RefNumber.Text14;
        export.Disb.Update.Coll.SystemGeneratedIdentifier =
          import.Disb.Item.Coll.SystemGeneratedIdentifier;

        if (!IsEmpty(import.Disb.Item.DetailSelect.Flag))
        {
          ++local.Common.Count;
          local.Common.SelectChar = import.Disb.Item.DetailSelect.Flag;
          local.DisbursementTransaction.SystemGeneratedIdentifier =
            import.Disb.Item.Coll.SystemGeneratedIdentifier;
        }

        export.Disb.Next();
      }

      export.PassThruFlowCsePerson.Number =
        import.PaymentRequest.CsePersonNumber ?? Spaces(10);
      MovePaymentRequest2(import.PaymentRequest,
        export.PassThruFlowPaymentRequest);
    }

    if (Equal(global.Command, "REISFROM"))
    {
      // -----------------------------------------------------
      // KD - 01/24/01
      // Cater for new warrant status - KPC.
      // -----------------------------------------------------
      if (IsEmpty(import.ReisFrom.Number) || Equal
        (import.ReisFrom.Number, "Not Av.") || Equal
        (import.ReisFrom.Number, "DOA") || Equal
        (import.ReisFrom.Number, "KPC"))
      {
        ExitState = "INVALID_REIS_WARRANT_NO";

        var field = GetField(export.ReisFrom, "number");

        field.Color = "red";
        field.Intensity = Intensity.High;
        field.Protected = true;

        return;
      }

      // -----------------------------------------------------
      // KD - 12/3/98
      // Clear export views before performing a re-display with the
      // Reiss-from Payment request number.
      // -----------------------------------------------------
      UseCabClearExportViews();
      export.PaymentRequest.Number = import.ReisFrom.Number ?? "";
      global.Command = "DISPLAY";
    }
    else if (Equal(global.Command, "REIS_TO"))
    {
      // -----------------------------------------------------
      // KD - 01/24/01
      // Cater for new warrant status - KPC.
      // -----------------------------------------------------
      if (IsEmpty(import.ReisTo.Number) || Equal
        (import.ReisTo.Number, "Not Av.") || Equal
        (import.ReisTo.Number, "DOA") || Equal(import.ReisTo.Number, "KPC"))
      {
        ExitState = "INVALID_REIS_WARRANT_NO";

        var field = GetField(export.ReisTo, "number");

        field.Color = "red";
        field.Intensity = Intensity.High;
        field.Protected = true;

        return;
      }

      // -----------------------------------------------------
      // KD - 12/3/98
      // Clear export views before performing a re-display with the
      // Reiss-to Payment request number.
      // -----------------------------------------------------
      UseCabClearExportViews();
      export.PaymentRequest.Number = import.ReisTo.Number ?? "";
      global.Command = "DISPLAY";
    }

    // *** Left Pad Warrent #, Payee and Designated Payee ***
    // ---------------------------------------------------
    // KD - 12/2/98
    // Amend the check to use export view instead of import.
    // --------------------------------------------------
    if (!IsEmpty(export.PaymentRequest.Number))
    {
      local.TextWorkArea.Text10 = export.PaymentRequest.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.PaymentRequest.Number = Substring(local.TextWorkArea.Text10, 2, 9);
    }

    // ---------------------------------------------------
    // KD - 12/2/98
    // Do not set export views of cse_person_no and
    // dp_sce_person_no here
    // --------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumberObligee = import.Payee.Number;
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

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "WARA") || Equal
      (global.Command, "WARR") || Equal(global.Command, "WAST") || Equal
      (global.Command, "WHST") || Equal(global.Command, "CRRC") || Equal
      (global.Command, "PSUM") || Equal(global.Command, "REIS_TO") || Equal
      (global.Command, "REISFROM"))
    {
      // ---------------------------------------------------
      // K. Doshi 06/26/2001 -  PR# 111410.
      // Don't pass Payee # during dislpay action.
      // ----------------------------------------------------
    }
    else if (Equal(global.Command, "DISPLAY"))
    {
      UseScCabTestSecurity1();
    }
    else
    {
      UseScCabTestSecurity2();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ****************************
    // MAIN  CASE-OF-COMMAND  STRUCTURE
    // ********************************
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // --------------------------
        //     INPUT VALIDATION
        // --------------------------
        // ****
        // If both IMPORT PAYMET_REQUEST ID  and IMPORT WARRANT NUMBER are not 
        // available, then input error.
        // ****
        if (import.PaymentRequest.SystemGeneratedIdentifier <= 0 && IsEmpty
          (export.PaymentRequest.Number))
        {
          ExitState = "FN0000_INVALID_INPUT";

          var field = GetField(export.PaymentRequest, "number");

          field.Error = true;

          return;
        }

        // ****
        // Prompt not allowed on Display action
        // ****
        if (!IsEmpty(import.WarrantNoPrompt.SelectChar))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.WarrantNoPromptIn, "selectChar");

          field.Error = true;

          return;
        }

        // *** START OF  MAIN PROCESSING LOGIC FOR DISPLAY ***
        if (import.PaymentRequest.SystemGeneratedIdentifier == 0 || !
          Equal(import.HiddenPaymentRequest.Number, import.PaymentRequest.Number)
          && !IsEmpty(import.HiddenPaymentRequest.Number))
        {
          // ---------------------------------------------
          // 06/26/01 - K. Doshi - WR# 285
          // Cater for Duplicate warrant #s.
          // ---------------------------------------------
          ReadPaymentRequest3();

          if (local.NbrOfWarrants.Count > 1)
          {
            // ------------------------------------------------------------
            // 06/26/01 - K. Doshi - WR# 285.
            // Duplicate exists. Only send warrant # and duplicate_warrant
            // flag via dialog flow.
            // ------------------------------------------------------------
            export.Payee.Number = "";
            export.Payee.FormattedName = "";
            MovePaymentRequest2(import.PaymentRequest,
              export.PassThruFlowPaymentRequest);
            export.HiddenPaymentRequest.Number = "";
            export.DuplicateWarrants.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          }

          if (!ReadPaymentRequest1())
          {
            // --------------------------------------------------------------
            // KD - 12/3/98
            // Display message if EFT has been entered or passed as
            // search criteria. In this case, system_generated_id would
            // have been entered in the text field.
            // ---------------------------------------------------------------
            if (Verify(export.PaymentRequest.Number, "0123456789") == 0)
            {
              local.PaymentRequest.SystemGeneratedIdentifier =
                (int)StringToNumber(export.PaymentRequest.Number);

              // ----------------------------------------------------------
              // KD - This is a full key read and hence defined as select only.
              // ------------------------------------------------------------
              if (ReadPaymentRequest6())
              {
                if (Equal(entities.PaymentRequest.Type1, "EFT"))
                {
                  ExitState = "FN0000_PAYMENT_REQUEST_IS_AN_EFT";

                  var field1 = GetField(export.PaymentRequest, "number");

                  field1.Error = true;

                  return;
                }
              }
              else
              {
                // -----------------------------------
                // OK
                // ------------------------------------
              }
            }

            var field = GetField(export.PaymentRequest, "number");

            field.Error = true;

            ExitState = "FN0000_WARRANT_NF";

            return;
          }
        }
        else if (!ReadPaymentRequest2())
        {
          ExitState = "FN0000_PASSED_PAYMENT_REQ_NF";

          return;
        }

        export.PaymentRequest.Assign(entities.PaymentRequest);
        MovePaymentRequest4(entities.PaymentRequest, export.HiddenPaymentRequest);
          

        if (AsChar(entities.PaymentRequest.InterstateInd) == 'Y')
        {
          export.PaymentRequest.InterstateInd = "Y";
        }
        else
        {
          export.PaymentRequest.InterstateInd = "N";
        }

        if (Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
          export.PaymentRequest.CsePersonNumber))
        {
          export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
          export.DesigPayee.FormattedName = "";
        }

        // *****************************************************
        // GETTING the Reissued-from-Payment-request-number
        // *****************************************************
        if (ReadPaymentRequest5())
        {
          if (IsEmpty(entities.ForReissue.Number))
          {
            // -----------------------------------------------------
            // KD - 01/24/01
            // Cater for new warrant status - KPC.
            // -----------------------------------------------------
            export.ReisFrom.Number = "KPC";
          }
          else
          {
            export.ReisFrom.Number = entities.ForReissue.Number;
          }

          if (AsChar(entities.ForReissue.InterstateInd) == 'Y')
          {
            export.PaymentRequest.InterstateInd = "Y";
          }
          else
          {
            export.PaymentRequest.InterstateInd = "N";
          }
        }
        else
        {
          export.ReisFrom.Number = "";
        }

        // ****************************************************
        // GETTING the Reissued-to-Payment-request-number
        // ****************************************************
        if (ReadPaymentRequest4())
        {
          if (IsEmpty(entities.ForReissue.Number))
          {
            // -----------------------------------------------------
            // KD - 01/24/01
            // Cater for new warrant status - KPC.
            // -----------------------------------------------------
            export.ReisTo.Number = "KPC";
          }
          else
          {
            export.ReisTo.Number = entities.ForReissue.Number;
          }
        }
        else
        {
          export.ReisTo.Number = "";
        }

        // *****************************************************
        // Getting the MOST RECENT Status-code of the given Payment-request 
        // Number.
        // *****************************************************
        local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

        // ****
        // The following READ can be UNSUCCESSFUL because an ATTRIBUTE is also 
        // involved in the scope of SELECTION along with the MANDATORY
        // RELATIONSHIP between the operand Entity Types. Hence the EXCEPTION
        // handling is felt to be necessary at this point.
        // ****
        if (ReadPaymentStatusHistory1())
        {
          // --------
          // The Following READ will cause an ABORT when UNSUCCESSFUL due to 
          // MANDATORY RELATIONSHIP between the operand Entity Types
          // --------
          if (ReadPaymentStatus())
          {
            export.PaymentStatus.Code = entities.PaymentStatus.Code;
          }
        }
        else
        {
          ExitState = "FN0000_PYMNT_STAT_HIST_NF";

          return;
        }

        // *****************************************
        // Get the Payee and Designated Payee Names
        // *****************************************
        export.Payee.Number = export.PaymentRequest.CsePersonNumber ?? Spaces
          (10);
        export.DesigPayee.Number =
          export.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
        export.DesigPayee.FormattedName = "";
        export.Payee.FormattedName = "";
        UseSiReadCsePerson2();

        if (IsExitState("CSE_PERSON_NF"))
        {
          ExitState = "PAYEE_CSE_PERSON_NF";

          return;
        }

        if (ReadPaymentStatusHistory2())
        {
          export.AddrMailed.RemailDate =
            entities.PaymentStatusHistory.EffectiveDate;
        }

        // ***************************************
        //  Get the compiled Disbursement Details
        // ***************************************
        // The System_generated_id in the group_export contains the ID of the 
        // corresponding Collection for the disbursement
        UseFnCompileDisbursementDetail();

        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          UseSiReadCsePerson1();

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "DESIGNATED_PAYEE_CSE_PERSON_NF";

            return;
          }

          // ------------------------------------------------------------------
          // 06/26/01 - K. Doshi - PR# 111410.
          // Add family violence check for DP.
          // Moved fv check at the end of display processing.
          // ------------------------------------------------------------------
          UseScSecurityCheckForFv1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }

        // ---------------------------------------------------------------------------------------
        // Added call to security cab to check person for family violence. CLS. 
        // 04/06/00
        // ---------------------------------------------------------------------------------------
        UseScSecurityCheckForFv2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // *******************************************
        // Getting the ADDRESS_MAILED_TO field values
        // *******************************************
        if (ReadWarrantRemailAddress())
        {
          export.MailedTo.FormattedName =
            entities.WarrantRemailAddress.Name ?? Spaces(33);
          export.AddrMailed.AddressLine1 =
            entities.WarrantRemailAddress.Street1;
          export.AddrMailed.AddressLine2 =
            entities.WarrantRemailAddress.Street2 ?? Spaces(30);
          export.AddrMailed.City = entities.WarrantRemailAddress.City;
          export.AddrMailed.State = entities.WarrantRemailAddress.State;
          export.AddrMailed.ZipCode = entities.WarrantRemailAddress.ZipCode5 + " " +
            entities.WarrantRemailAddress.ZipCode4 + " " + entities
            .WarrantRemailAddress.ZipCode3;

          // ------------------------------------------------------
          // KD - 12/2/98
          // Remail Date will now be set to the last 'REML' status date
          // ------------------------------------------------------
        }

        if (AsChar(export.PaymentRequest.RecoupmentIndKpc) == 'Y')
        {
          ExitState = "FN0000_DISPLAY_SUCCES_KPC_RCPMNT";
        }
        else if (export.Disb.IsEmpty)
        {
          ExitState = "FN0000_DISB_DETAILS_NOT_FOUND";
        }
        else if (export.Disb.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "EXIT":
        break;
      case "LIST":
        // *** Processing for  PROMPT ***
        if (AsChar(import.WarrantNoPrompt.SelectChar) != 'S')
        {
          ExitState = "ZD_ACO_NE0_INVALID_PROMPT_VALUE2";

          var field = GetField(export.WarrantNoPromptIn, "selectChar");

          field.Error = true;
        }
        else
        {
          ExitState = "ECO_LNK_TO_LST_WARRANTS";
        }

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "WARA":
        ExitState = "ECO_LNK_TO_WARA";

        break;
      case "WAST":
        ExitState = "ECO_LNK_TO_WAST";

        break;
      case "WARR":
        ExitState = "ECO_LNK_TO_LST_WARRANTS";

        break;
      case "WHST":
        ExitState = "ECO_LNK_TO_WHST";

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // *** Set Proper Exit_State to flow to the SIGNOFF Procedure ***
        UseScCabSignoff();

        break;
      case "PSUM":
        ExitState = "ECO_LNK_TO_PSUM";

        break;
      case "CRRC":
        if (local.Common.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }
        else if (local.Common.Count == 0)
        {
          if (import.Disb.IsEmpty)
          {
            ExitState = "FN0000_CANT_FLOW_TO_CRRC_NO_DATA";
          }
          else
          {
            ExitState = "FN0000_SEL_ROW_B4_FLOW_TO_CRCC";
          }

          return;
        }

        if (AsChar(local.Common.SelectChar) == 'S')
        {
          // ** Continue Processing
        }
        else
        {
          ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE1";

          return;
        }

        // ----------------------------------------------------------------
        // PR93001 - Add Obligee Qualification in read below to use Index on 
        // Disb_tran.
        // ---------------------------------------------------------------
        if (ReadCashReceiptDetail())
        {
          // *** Cash_receipt_detail found..pass it to the CRRC with other 
          // Identifiers...Continue Processing
          export.PassThruFlowCashReceiptDetail.SequentialIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
        }
        else
        {
          ExitState = "CASH_RECEIPT_DETAIL_NF";

          return;
        }

        if (ReadCashReceiptEventCashReceiptSourceType())
        {
          // *** Cash_receipt event and source_type found.. pass their IDs with 
          // other Ids to CRRC... Continue processing
          export.PassThruFlowCashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          export.PassThruFlowCashReceiptSourceType.SystemGeneratedIdentifier =
            entities.CashReceiptSourceType.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "CASH_RCPT_EV_SRCTYPE_NF";

          return;
        }

        if (ReadCashReceiptType())
        {
          // ***Cash_receipt_type found...pass its ID with other Ids to CRRC...
          // continue processing
          export.PassThruFlowCashReceiptType.SystemGeneratedIdentifier =
            entities.CashReceiptType.SystemGeneratedIdentifier;
        }
        else
        {
          ExitState = "FN0113_CASH_RCPT_TYPE_NF";

          return;
        }

        if (ReadCollectionType())
        {
          // ***Collection_type is found...pass its ID with others to CRRC
          export.PassThruFlowCollectionType.SequentialIdentifier =
            entities.CollectionType.SequentialIdentifier;
        }
        else
        {
          ExitState = "FN0000_COLLECTION_TYPE_NF";

          return;
        }

        for(export.Disb.Index = 0; export.Disb.Index < export.Disb.Count; ++
          export.Disb.Index)
        {
          if (!IsEmpty(export.Disb.Item.DetailSelect.Flag))
          {
            export.Disb.Update.DetailSelect.Flag = "";

            break;
          }
        }

        // *** All the Identifiers were obtained ; set exitstate to transfer to 
        // CRRC passing these IDs
        ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_3";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisb(CabClearExportViews.Export.DisbGroup source,
    Export.DisbGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move fit weakly.");

    target.DetailSelect.Flag = source.DetailSelect.Flag;
    target.Coll.SystemGeneratedIdentifier =
      source.Coll.SystemGeneratedIdentifier;
    target.DisbDetailDisbursementTransaction.Assign(
      source.DisbDetailDisbursementTransaction);
    target.RefNumber.Text14 = source.RefNumber.Text14;
  }

  private static void MoveGroupToDisb(FnCompileDisbursementDetail.Export.
    GroupGroup source, Export.DisbGroup target)
  {
    target.DisbType.Text10 = source.GrDisbType.Text10;
    target.DisbDetailCashReceipt.ReceivedDate =
      source.GrCashReceipt.ReceivedDate;
    target.DetailSelect.Flag = source.GrCommon.Flag;
    target.Coll.SystemGeneratedIdentifier =
      source.Coll.SystemGeneratedIdentifier;
    target.DisbDetailDisbursementTransaction.Assign(
      source.GrDisbursementTransaction);
    target.RefNumber.Text14 = source.GrWorkArea.Text14;
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

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Classification = source.Classification;
    target.ImprestFundCode = source.ImprestFundCode;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DesignatedPayeeCsePersonNo = source.DesignatedPayeeCsePersonNo;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.CsePersonNumber = source.CsePersonNumber;
    target.Number = source.Number;
  }

  private static void MovePaymentRequest3(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MovePaymentRequest4(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private void UseCabClearExportViews()
  {
    var useImport = new CabClearExportViews.Import();
    var useExport = new CabClearExportViews.Export();

    Call(CabClearExportViews.Execute, useImport, useExport);

    MovePaymentRequest1(useExport.PaymentRequest, export.PaymentRequest);
    export.PaymentStatus.Code = useExport.PaymentStatus.Code;
    export.AddrMailed.Assign(useExport.AddrMailed);
    MoveCsePersonsWorkSet(useExport.Payee, export.Payee);
    MoveCsePersonsWorkSet(useExport.DesigPayee, export.DesigPayee);
    MovePaymentRequest4(useExport.ReisFrom, export.ReisFrom);
    MovePaymentRequest4(useExport.ReisTo, export.ReisTo);
    useExport.Disb.CopyTo(export.Disb, MoveDisb);
    export.PassThruFlowCsePerson.Number =
      useExport.PassThruFlowCsePerson.Number;
    MovePaymentRequest3(useExport.PassThruFlowPaymentRequest,
      export.PassThruFlowPaymentRequest);
    export.MailedTo.FormattedName = useExport.MailedTo.FormattedName;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.Maximum.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
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

  private void UseFnCompileDisbursementDetail()
  {
    var useImport = new FnCompileDisbursementDetail.Import();
    var useExport = new FnCompileDisbursementDetail.Export();

    useImport.PaymentRequest.Assign(entities.PaymentRequest);

    Call(FnCompileDisbursementDetail.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Disb, MoveGroupToDisb);
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

    useImport.CsePersonsWorkSet.Number = import.Payee.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv1()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.DesigPayee.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv2()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.DesigPayee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.DesigPayee);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Payee);
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          local.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", import.PaymentRequest.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 4);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest3()
  {
    return Read("ReadPaymentRequest3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        local.NbrOfWarrants.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadPaymentRequest4()
  {
    entities.ForReissue.Populated = false;

    return Read("ReadPaymentRequest4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ForReissue.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ForReissue.Number = db.GetNullableString(reader, 1);
        entities.ForReissue.Type1 = db.GetString(reader, 2);
        entities.ForReissue.PrqRGeneratedId = db.GetNullableInt32(reader, 3);
        entities.ForReissue.InterstateInd = db.GetNullableString(reader, 4);
        entities.ForReissue.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ForReissue.Type1);
      });
  }

  private bool ReadPaymentRequest5()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.ForReissue.Populated = false;

    return Read("ReadPaymentRequest5",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.PrqRGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ForReissue.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ForReissue.Number = db.GetNullableString(reader, 1);
        entities.ForReissue.Type1 = db.GetString(reader, 2);
        entities.ForReissue.PrqRGeneratedId = db.GetNullableInt32(reader, 3);
        entities.ForReissue.InterstateInd = db.GetNullableString(reader, 4);
        entities.ForReissue.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ForReissue.Type1);
      });
  }

  private bool ReadPaymentRequest6()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest6",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          local.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedBy = db.GetString(reader, 3);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.Classification = db.GetString(reader, 8);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 9);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 10);
        entities.PaymentRequest.Type1 = db.GetString(reader, 11);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 12);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 13);
        entities.PaymentRequest.RecoupmentIndKpc =
          db.GetNullableString(reader, 14);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentStatus()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatusHistory.PstGeneratedId);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory1()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory2()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private bool ReadWarrantRemailAddress()
  {
    entities.WarrantRemailAddress.Populated = false;

    return Read("ReadWarrantRemailAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqId", entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrantRemailAddress.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrantRemailAddress.Street1 = db.GetString(reader, 1);
        entities.WarrantRemailAddress.Street2 = db.GetNullableString(reader, 2);
        entities.WarrantRemailAddress.City = db.GetString(reader, 3);
        entities.WarrantRemailAddress.State = db.GetString(reader, 4);
        entities.WarrantRemailAddress.ZipCode4 =
          db.GetNullableString(reader, 5);
        entities.WarrantRemailAddress.ZipCode5 = db.GetString(reader, 6);
        entities.WarrantRemailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.WarrantRemailAddress.Name = db.GetNullableString(reader, 8);
        entities.WarrantRemailAddress.RemailDate = db.GetDate(reader, 9);
        entities.WarrantRemailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 11);
        entities.WarrantRemailAddress.Populated = true;
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
    /// <summary>A DisbGroup group.</summary>
    [Serializable]
    public class DisbGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DisbDetailCashReceipt.
      /// </summary>
      [JsonPropertyName("disbDetailCashReceipt")]
      public CashReceipt DisbDetailCashReceipt
      {
        get => disbDetailCashReceipt ??= new();
        set => disbDetailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of Coll.
      /// </summary>
      [JsonPropertyName("coll")]
      public DisbursementTransaction Coll
      {
        get => coll ??= new();
        set => coll = value;
      }

      /// <summary>
      /// A value of DisbDetailDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbDetailDisbursementTransaction")]
      public DisbursementTransaction DisbDetailDisbursementTransaction
      {
        get => disbDetailDisbursementTransaction ??= new();
        set => disbDetailDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of RefNumber.
      /// </summary>
      [JsonPropertyName("refNumber")]
      public WorkArea RefNumber
      {
        get => refNumber ??= new();
        set => refNumber = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 175;

      private TextWorkArea disbType;
      private CashReceipt disbDetailCashReceipt;
      private Common detailSelect;
      private DisbursementTransaction coll;
      private DisbursementTransaction disbDetailDisbursementTransaction;
      private WorkArea refNumber;
    }

    /// <summary>
    /// A value of MailedTo.
    /// </summary>
    [JsonPropertyName("mailedTo")]
    public CsePersonsWorkSet MailedTo
    {
      get => mailedTo ??= new();
      set => mailedTo = value;
    }

    /// <summary>
    /// A value of ReceivedPayee.
    /// </summary>
    [JsonPropertyName("receivedPayee")]
    public CsePersonsWorkSet ReceivedPayee
    {
      get => receivedPayee ??= new();
      set => receivedPayee = value;
    }

    /// <summary>
    /// A value of WarrantNoPrompt.
    /// </summary>
    [JsonPropertyName("warrantNoPrompt")]
    public Common WarrantNoPrompt
    {
      get => warrantNoPrompt ??= new();
      set => warrantNoPrompt = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of ReisTo.
    /// </summary>
    [JsonPropertyName("reisTo")]
    public PaymentRequest ReisTo
    {
      get => reisTo ??= new();
      set => reisTo = value;
    }

    /// <summary>
    /// A value of ReisFrom.
    /// </summary>
    [JsonPropertyName("reisFrom")]
    public PaymentRequest ReisFrom
    {
      get => reisFrom ??= new();
      set => reisFrom = value;
    }

    /// <summary>
    /// A value of AddrMailed.
    /// </summary>
    [JsonPropertyName("addrMailed")]
    public LocalWorkAddr AddrMailed
    {
      get => addrMailed ??= new();
      set => addrMailed = value;
    }

    /// <summary>
    /// Gets a value of Disb.
    /// </summary>
    [JsonIgnore]
    public Array<DisbGroup> Disb => disb ??= new(DisbGroup.Capacity);

    /// <summary>
    /// Gets a value of Disb for json serialization.
    /// </summary>
    [JsonPropertyName("disb")]
    [Computed]
    public IList<DisbGroup> Disb_Json
    {
      get => disb;
      set => Disb.Assign(value);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    private CsePersonsWorkSet mailedTo;
    private CsePersonsWorkSet receivedPayee;
    private Common warrantNoPrompt;
    private CsePersonsWorkSet desigPayee;
    private CsePersonsWorkSet payee;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentRequest reisTo;
    private PaymentRequest reisFrom;
    private LocalWorkAddr addrMailed;
    private Array<DisbGroup> disb;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePerson csePerson;
    private PaymentRequest hiddenPaymentRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DisbGroup group.</summary>
    [Serializable]
    public class DisbGroup
    {
      /// <summary>
      /// A value of DisbType.
      /// </summary>
      [JsonPropertyName("disbType")]
      public TextWorkArea DisbType
      {
        get => disbType ??= new();
        set => disbType = value;
      }

      /// <summary>
      /// A value of DisbDetailCashReceipt.
      /// </summary>
      [JsonPropertyName("disbDetailCashReceipt")]
      public CashReceipt DisbDetailCashReceipt
      {
        get => disbDetailCashReceipt ??= new();
        set => disbDetailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailSelect.
      /// </summary>
      [JsonPropertyName("detailSelect")]
      public Common DetailSelect
      {
        get => detailSelect ??= new();
        set => detailSelect = value;
      }

      /// <summary>
      /// A value of Coll.
      /// </summary>
      [JsonPropertyName("coll")]
      public DisbursementTransaction Coll
      {
        get => coll ??= new();
        set => coll = value;
      }

      /// <summary>
      /// A value of DisbDetailDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("disbDetailDisbursementTransaction")]
      public DisbursementTransaction DisbDetailDisbursementTransaction
      {
        get => disbDetailDisbursementTransaction ??= new();
        set => disbDetailDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of RefNumber.
      /// </summary>
      [JsonPropertyName("refNumber")]
      public WorkArea RefNumber
      {
        get => refNumber ??= new();
        set => refNumber = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 175;

      private TextWorkArea disbType;
      private CashReceipt disbDetailCashReceipt;
      private Common detailSelect;
      private DisbursementTransaction coll;
      private DisbursementTransaction disbDetailDisbursementTransaction;
      private WorkArea refNumber;
    }

    /// <summary>
    /// A value of MailedTo.
    /// </summary>
    [JsonPropertyName("mailedTo")]
    public CsePersonsWorkSet MailedTo
    {
      get => mailedTo ??= new();
      set => mailedTo = value;
    }

    /// <summary>
    /// A value of PassThruFlowObligation.
    /// </summary>
    [JsonPropertyName("passThruFlowObligation")]
    public Obligation PassThruFlowObligation
    {
      get => passThruFlowObligation ??= new();
      set => passThruFlowObligation = value;
    }

    /// <summary>
    /// A value of PassThruFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("passThruFlowPaymentRequest")]
    public PaymentRequest PassThruFlowPaymentRequest
    {
      get => passThruFlowPaymentRequest ??= new();
      set => passThruFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of WarrantNoPromptIn.
    /// </summary>
    [JsonPropertyName("warrantNoPromptIn")]
    public Common WarrantNoPromptIn
    {
      get => warrantNoPromptIn ??= new();
      set => warrantNoPromptIn = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of ReisTo.
    /// </summary>
    [JsonPropertyName("reisTo")]
    public PaymentRequest ReisTo
    {
      get => reisTo ??= new();
      set => reisTo = value;
    }

    /// <summary>
    /// A value of ReisFrom.
    /// </summary>
    [JsonPropertyName("reisFrom")]
    public PaymentRequest ReisFrom
    {
      get => reisFrom ??= new();
      set => reisFrom = value;
    }

    /// <summary>
    /// A value of AddrMailed.
    /// </summary>
    [JsonPropertyName("addrMailed")]
    public LocalWorkAddr AddrMailed
    {
      get => addrMailed ??= new();
      set => addrMailed = value;
    }

    /// <summary>
    /// Gets a value of Disb.
    /// </summary>
    [JsonIgnore]
    public Array<DisbGroup> Disb => disb ??= new(DisbGroup.Capacity);

    /// <summary>
    /// Gets a value of Disb for json serialization.
    /// </summary>
    [JsonPropertyName("disb")]
    [Computed]
    public IList<DisbGroup> Disb_Json
    {
      get => disb;
      set => Disb.Assign(value);
    }

    /// <summary>
    /// A value of PassThruFlowCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passThruFlowCashReceiptDetail")]
    public CashReceiptDetail PassThruFlowCashReceiptDetail
    {
      get => passThruFlowCashReceiptDetail ??= new();
      set => passThruFlowCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PassThruFlowCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("passThruFlowCashReceiptEvent")]
    public CashReceiptEvent PassThruFlowCashReceiptEvent
    {
      get => passThruFlowCashReceiptEvent ??= new();
      set => passThruFlowCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of PassThruFlowCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("passThruFlowCashReceiptSourceType")]
    public CashReceiptSourceType PassThruFlowCashReceiptSourceType
    {
      get => passThruFlowCashReceiptSourceType ??= new();
      set => passThruFlowCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassThruFlowCashReceiptType.
    /// </summary>
    [JsonPropertyName("passThruFlowCashReceiptType")]
    public CashReceiptType PassThruFlowCashReceiptType
    {
      get => passThruFlowCashReceiptType ??= new();
      set => passThruFlowCashReceiptType = value;
    }

    /// <summary>
    /// A value of PassThruFlowCollectionType.
    /// </summary>
    [JsonPropertyName("passThruFlowCollectionType")]
    public CollectionType PassThruFlowCollectionType
    {
      get => passThruFlowCollectionType ??= new();
      set => passThruFlowCollectionType = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    /// <summary>
    /// A value of DuplicateWarrants.
    /// </summary>
    [JsonPropertyName("duplicateWarrants")]
    public Common DuplicateWarrants
    {
      get => duplicateWarrants ??= new();
      set => duplicateWarrants = value;
    }

    private CsePersonsWorkSet mailedTo;
    private Obligation passThruFlowObligation;
    private PaymentRequest passThruFlowPaymentRequest;
    private CsePerson passThruFlowCsePerson;
    private Common warrantNoPromptIn;
    private CsePersonsWorkSet desigPayee;
    private CsePersonsWorkSet payee;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentRequest reisTo;
    private PaymentRequest reisFrom;
    private LocalWorkAddr addrMailed;
    private Array<DisbGroup> disb;
    private CashReceiptDetail passThruFlowCashReceiptDetail;
    private CashReceiptEvent passThruFlowCashReceiptEvent;
    private CashReceiptSourceType passThruFlowCashReceiptSourceType;
    private CashReceiptType passThruFlowCashReceiptType;
    private CollectionType passThruFlowCollectionType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePerson csePerson;
    private PaymentRequest hiddenPaymentRequest;
    private Common duplicateWarrants;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of AddrFoundFlagIn.
    /// </summary>
    [JsonPropertyName("addrFoundFlagIn")]
    public Common AddrFoundFlagIn
    {
      get => addrFoundFlagIn ??= new();
      set => addrFoundFlagIn = value;
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
    /// A value of PaySthFound.
    /// </summary>
    [JsonPropertyName("paySthFound")]
    public Common PaySthFound
    {
      get => paySthFound ??= new();
      set => paySthFound = value;
    }

    /// <summary>
    /// A value of RemlAddrFound.
    /// </summary>
    [JsonPropertyName("remlAddrFound")]
    public Common RemlAddrFound
    {
      get => remlAddrFound ??= new();
      set => remlAddrFound = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of NbrOfWarrants.
    /// </summary>
    [JsonPropertyName("nbrOfWarrants")]
    public Common NbrOfWarrants
    {
      get => nbrOfWarrants ??= new();
      set => nbrOfWarrants = value;
    }

    private PaymentRequest paymentRequest;
    private DisbursementTransaction disbursementTransaction;
    private Common common;
    private Common addrFoundFlagIn;
    private DateWorkArea maximum;
    private Common paySthFound;
    private Common remlAddrFound;
    private CsePerson csePerson;
    private TextWorkArea textWorkArea;
    private Common nbrOfWarrants;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ForReissue.
    /// </summary>
    [JsonPropertyName("forReissue")]
    public PaymentRequest ForReissue
    {
      get => forReissue ??= new();
      set => forReissue = value;
    }

    /// <summary>
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private CsePersonAccount obligor;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CsePersonAddress csePersonAddress;
    private PaymentRequest forReissue;
    private WarrantRemailAddress warrantRemailAddress;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private CashReceipt cashReceipt;
  }
#endregion
}

// Program: FN_EDTL_LST_EFT_DETAIL, ID: 372155944, model: 746.
// Short name: SWEEDTLP
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
/// A program: FN_EDTL_LST_EFT_DETAIL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnEdtlLstEftDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EDTL_LST_EFT_DETAIL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEdtlLstEftDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEdtlLstEftDetail.
  /// </summary>
  public FnEdtlLstEftDetail(IContext context, Import import, Export export):
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
    // ---------------------------------------------------------------------------------------
    // Date		Developer Name		Description
    // 02/28/96	Holly kennedy-MTW 	Source
    // 04/30/96        R.B.Mohapatra-MTW	Addition of three Commands : PACC, 
    // CRRC, OSUM
    // 						and their Processing logic
    // 12/10/96	R. Marchman		Add new security and next tran
    // 07/01/97	A Samuels		Completed development
    // 01/05/99        G Sharp                 Phase 2 changes.
    // 05/10/99   Fangman       Changed process to be driven by EFT # rather 
    // than Payment Request #, and other smaller changes.  Added Re-issue
    // functionality.
    // 07/01/99   Fangman       Changed prompt field to use a work attribute 
    // that did not have permitted values so that the correct error msg would
    // appear.  Fixed problem with reading the correct Cash Receipt Detail for
    // flow to CRRC.  Added Issued To field to show the name of the intestate
    // state who received the money.
    // 07/17/99   Fangman     Add check digit behind Routing number.
    // 07/19/99   Fangman     Added move of import to export for Issued To name.
    // 07/30/99   Fangman     Added code to set Issued To Name for non-
    // interstate EFTs.
    // 05/23/00    E.Shirk    Added the excess ura indicator after each disb 
    // code.  Part of PRWORA requirement.
    // 08/01/00   E.Shirk PR#96968-D   Modified the disb. transaction read for 
    // unqiue qualification by adding obligee criteria.
    // ---------------------------------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Maximized.Date = UseCabSetMaximumDiscontinueDate();

    // *****
    // Move imports to exports
    // *****
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.ElectronicFundTransmission.
        Assign(import.ElectronicFundTransmission);
      export.PaymentRequest.Assign(import.PaymentRequest);
      MovePaymentRequest(import.ReissuedTo, export.ReissuedTo);
      export.PaymentStatus.Code = import.PaymentStatus.Code;
      MoveCsePersonsWorkSet(import.Payee, export.Payee);
      MoveCsePersonsWorkSet(import.DesignatedPayee, export.DesignatedPayee);
      export.IssuedTo.FormattedName = import.IssuedTo.FormattedName;
      MovePersonPreferredPaymentMethod(import.PersonPreferredPaymentMethod,
        export.PersonPreferredPaymentMethod);
      export.PassThruFlowCsePerson.Number = export.Payee.Number;
      local.Common.Count = 0;

      export.Disbursements.Index = 0;
      export.Disbursements.Clear();

      for(import.Disbursements.Index = 0; import.Disbursements.Index < import
        .Disbursements.Count; ++import.Disbursements.Index)
      {
        if (export.Disbursements.IsFull)
        {
          break;
        }

        export.Disbursements.Update.DetailCommon.SelectChar =
          import.Disbursements.Item.DetailCommon.SelectChar;
        export.Disbursements.Update.DetailDisbursementTransaction.Assign(
          import.Disbursements.Item.DetailDisbursementTransaction);
        export.Disbursements.Update.DetailDisbursementType.Code =
          import.Disbursements.Item.DetailDisbursementType.Code;
        export.Disbursements.Update.DetailDisbCode.Text10 =
          import.Disbursements.Item.DetailDisbCode.Text10;

        if (!IsEmpty(import.Disbursements.Item.DetailCommon.SelectChar))
        {
          if (AsChar(export.Disbursements.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Common.Count;
            local.Common.SelectChar =
              import.Disbursements.Item.DetailCommon.SelectChar;
            local.DisbursementTransaction.SystemGeneratedIdentifier =
              import.Disbursements.Item.DetailDisbursementTransaction.
                SystemGeneratedIdentifier;
          }
          else
          {
            var field =
              GetField(export.Disbursements.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }
        }

        export.Disbursements.Next();
      }
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      return;
    }

    if (local.Common.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = import.Payee.Number;
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
      export.Payee.Number = export.Hidden.CsePersonNumber ?? Spaces(10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "PSUM") || Equal
      (global.Command, "CRRC") || Equal(global.Command, "EFTL") || Equal
      (global.Command, "EHST") || Equal(global.Command, "MTRN") || Equal
      (global.Command, "REIS_TO"))
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

    // *****
    // Case of Command
    // *****
    switch(TrimEnd(global.Command))
    {
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "MTRN":
        ExitState = "ECO_LNK_MTN_EFT_TRANSMISSION";

        break;
      case "DISPLAY":
        if (import.ElectronicFundTransmission.TransmissionIdentifier == 0)
        {
          var field =
            GetField(export.ElectronicFundTransmission, "transmissionIdentifier");
            

          field.Error = true;

          ExitState = "SP0000_REQUIRED_FIELD_MISSING";

          return;
        }

        // *****
        // Display the Payment Request and related Disbursements.
        // *****
        if (ReadElectronicFundTransmission())
        {
          export.ElectronicFundTransmission.Assign(
            entities.ElectronicFundTransmission);
        }
        else
        {
          var field =
            GetField(export.ElectronicFundTransmission, "transmissionIdentifier");
            

          field.Error = true;

          ExitState = "FN0000_EFT_NF";

          return;
        }

        if (ReadPaymentRequest1())
        {
          export.PaymentRequest.Assign(entities.PaymentRequest);
        }
        else
        {
          ExitState = "FN0000_PAYMENT_REQUEST_NF";

          return;
        }

        if (ReadPaymentRequest2())
        {
          if (IsEmpty(entities.ReissuedTo.Number))
          {
            export.ReissuedTo.Number = "Not Avail";
          }
          else
          {
            export.ReissuedTo.Number = entities.ReissuedTo.Number;
          }

          export.ReissuedDate.Date = Date(entities.ReissuedTo.CreatedTimestamp);
        }
        else
        {
          // This is OK.  Most EFTs are not reissued as warrants.
        }

        export.Disbursements.Index = 0;
        export.Disbursements.Clear();

        foreach(var item in ReadDisbursementTransactionDisbursementType())
        {
          MoveDisbursementTransaction(entities.DisbursementTransaction,
            export.Disbursements.Update.DetailDisbursementTransaction);
          export.Disbursements.Update.DetailDisbursementType.Code =
            entities.DisbursementType.Code;

          if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y')
          {
            export.Disbursements.Update.DetailDisbCode.Text10 =
              TrimEnd(entities.DisbursementType.Code) + " " + "X";
          }
          else
          {
            export.Disbursements.Update.DetailDisbCode.Text10 =
              entities.DisbursementType.Code;
          }

          export.Disbursements.Next();
        }

        if (ReadPaymentStatusHistoryPaymentStatus())
        {
          export.PaymentStatus.Code = entities.PaymentStatus.Code;
        }
        else
        {
          ExitState = "FN0000_PYMNT_STAT_HIST_NF";

          return;
        }

        export.Payee.Number = export.PaymentRequest.CsePersonNumber ?? Spaces
          (10);
        UseSiReadCsePerson1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Payee.FormattedName = "Cse_person not found";
        }

        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          export.DesignatedPayee.Number =
            export.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseSiReadCsePerson2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.DesignatedPayee.FormattedName = "Cse_person not found";
          }
        }

        if (AsChar(entities.PaymentRequest.InterstateInd) == 'Y')
        {
          export.IssuedTo.FormattedName =
            entities.ElectronicFundTransmission.ReceivingCompanyName ?? Spaces
            (33);
        }
        else if (!IsEmpty(export.DesignatedPayee.FormattedName))
        {
          export.IssuedTo.FormattedName = export.DesignatedPayee.FormattedName;
        }
        else
        {
          export.IssuedTo.FormattedName = export.Payee.FormattedName;
        }

        // *****
        // If group view is empty send message back to the user.  If the group 
        // view is full send message back to the user.
        // ****
        if (export.Disbursements.IsEmpty)
        {
          ExitState = "FN0000_DISB_DETAILS_NF";
        }

        if (export.Disbursements.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        if (!IsEmpty(import.EftNumber.PromptField))
        {
          if (AsChar(import.EftNumber.PromptField) == 'S')
          {
            ExitState = "ECO_LNK_TO_LST_EFTS";
          }
          else
          {
            var field = GetField(export.EftNumber, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }
        else
        {
          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "EFTL":
        ExitState = "ECO_LNK_TO_LST_EFTS";

        break;
      case "EHST":
        ExitState = "ECO_XFR_TO_LST_EFT_STAT_HIST";

        break;
      case "PSUM":
        ExitState = "ECO_XFR_TO_LST_MNTHLY_PAYEE_SUM";

        break;
      case "CRRC":
        if (local.Common.Count == 1)
        {
          if (ReadDisbursementTransaction1())
          {
            if (ReadDisbursementTransaction2())
            {
              if (ReadCollection())
              {
                if (ReadCashReceiptDetail())
                {
                  // *** Cash_receipt_detail found..pass it to the CRRC with 
                  // other Identifiers...Continue Processing
                  export.PassThruFlowCashReceiptDetail.SequentialIdentifier =
                    entities.CashReceiptDetail.SequentialIdentifier;
                }
                else
                {
                  ExitState = "CASH_RECEIPT_DETAIL_NF";

                  return;
                }
              }
              else
              {
                ExitState = "FN0000_COLLECTION_NF";

                return;
              }
            }
            else
            {
              ExitState = "FN0000_DISB_TRAN_CREDIT_NF";

              return;
            }
          }
          else
          {
            ExitState = "FN0000_DISB_TRANSACTION_NF";

            return;
          }

          if (ReadCashReceiptEventCashReceiptSourceType())
          {
            // *** Cash_receipt event and source_type found.. pass their IDs 
            // with other Ids to CRRC... Continue processing
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
            // ***Cash_receipt_type found...pass its ID with other Ids to CRRC.
            // ..continue processing
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
        }

        ExitState = "ECO_XFR_TO_REC_CRRC";

        break;
      case "REIS_TO":
        if (Equal(export.PaymentStatus.Code, "REIS"))
        {
          ExitState = "FN0000_PMT_REQ_ALREADY_REISSUED";

          return;
        }
        else if (Equal(export.PaymentStatus.Code, "SENT"))
        {
          // These are the only two status valid for a re-issue.
        }
        else
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";

          return;
        }

        // ---------------------------------------------------------------
        // For re-issues:
        // 1.  The status of the current EFT Payment Request must be set to "
        // REIS".
        // 2.  A new WAR Payment Request must be created from the EFT Payment 
        // Request.
        // 3.  The status of the new WAR Payment Request must be set to "REQ".
        // 4.  The disbursements associated to the old EFT Payment Request must 
        // be unassociated and then associated to the new WAR Payment Request.
        // ---------------------------------------------------------------
        // Get currency on the Reissue status.
        if (!ReadPaymentStatus2())
        {
          ExitState = "FN0000_PYMNT_STAT_NF";

          return;
        }

        if (ReadPaymentRequestPaymentStatusHistory())
        {
          local.PaymentStatusHistory.SystemGeneratedIdentifier =
            entities.PaymentStatusHistory.SystemGeneratedIdentifier;

          try
          {
            UpdatePaymentStatusHistory();

            try
            {
              CreatePaymentStatusHistory1();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PMNT_STAT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PMNT_STAT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_PMNT_STAT_HIST_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PMNT_STAT_HIST_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ExitState = "FN0000_PMNT_STAT_HIST_NF";

          return;
        }

        if (!ReadPaymentStatus1())
        {
          ExitState = "FN0000_PYMNT_STAT_NF";

          return;
        }

        local.NbrOfCreateAttempts.Count = 0;

        do
        {
          try
          {
            CreatePaymentRequest();

            try
            {
              CreatePaymentStatusHistory2();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "FN0000_PMNT_STAT_HIST_AE_RB";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "FN0000_PMNT_STAT_HIST_PV_RB";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            ExitState = "ACO_NN0000_ALL_OK";

            break;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ++local.NbrOfCreateAttempts.Count;
                ExitState = "FN0000_PAYMENT_REQUEST_AE_RB";

                continue;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_PAYMENT_REQUEST_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        while(local.NbrOfCreateAttempts.Count < 10);

        if (IsExitState("FN0000_PAYMENT_REQUEST_AE_RB"))
        {
          return;
        }

        foreach(var item in ReadDisbursementTransaction3())
        {
          DisassociateDisbursementTransaction();
          AssociateDisbursementTransaction();
        }

        export.PaymentStatus.Code = "REIS";
        export.ReissuedTo.Number = "Not Avail";
        export.ReissuedDate.Date = Now().Date;

        if (ReadPersonPreferredPaymentMethod())
        {
          try
          {
            UpdatePersonPreferredPaymentMethod();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "PERSON_PREFERRED_PAYMENT_MET_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "PERSON_PREFERRED_PAYMENT_MET_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          // The preferred payment method may have already been turned off.
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
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

  private static void MovePersonPreferredPaymentMethod(
    PersonPreferredPaymentMethod source, PersonPreferredPaymentMethod target)
  {
    target.AbaRoutingNumber = source.AbaRoutingNumber;
    target.DfiAccountNumber = source.DfiAccountNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
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

    useImport.CsePersonsWorkSet.Number = import.Payee.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.Payee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.Payee);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.DesignatedPayee.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.DesignatedPayee);
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var prqGeneratedId = entities.Warrant.SystemGeneratedIdentifier;

    entities.DisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.PrqGeneratedId = prqGeneratedId;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var processDate = local.InitializeWarrant.ProcessDate;
    var amount = entities.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      entities.PaymentRequest.DesignatedPayeeCsePersonNo;
    var csePersonNumber = entities.PaymentRequest.CsePersonNumber;
    var imprestFundCode = entities.PaymentRequest.ImprestFundCode;
    var classification = entities.PaymentRequest.Classification;
    var type1 = "WAR";
    var prqRGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var interstateInd = entities.PaymentRequest.InterstateInd;

    CheckValid<PaymentRequest>("Type1", type1);
    entities.Warrant.Populated = false;
    Update("CreatePaymentRequest",
      (db, command) =>
      {
        db.SetInt32(command, "paymentRequestId", systemGeneratedIdentifier);
        db.SetDate(command, "processDate", processDate);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.
          SetNullableString(command, "dpCsePerNum", designatedPayeeCsePersonNo);
          
        db.SetNullableString(command, "csePersonNumber", csePersonNumber);
        db.SetNullableString(command, "imprestFundCode", imprestFundCode);
        db.SetString(command, "classification", classification);
        db.SetString(command, "recoveryFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
        db.SetNullableInt32(command, "prqRGeneratedId", prqRGeneratedId);
        db.SetNullableString(command, "interstateInd", interstateInd);
      });

    entities.Warrant.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Warrant.ProcessDate = processDate;
    entities.Warrant.Amount = amount;
    entities.Warrant.CreatedBy = createdBy;
    entities.Warrant.CreatedTimestamp = createdTimestamp;
    entities.Warrant.DesignatedPayeeCsePersonNo = designatedPayeeCsePersonNo;
    entities.Warrant.CsePersonNumber = csePersonNumber;
    entities.Warrant.ImprestFundCode = imprestFundCode;
    entities.Warrant.Classification = classification;
    entities.Warrant.Type1 = type1;
    entities.Warrant.PrqRGeneratedId = prqRGeneratedId;
    entities.Warrant.InterstateInd = interstateInd;
    entities.Warrant.Populated = true;
  }

  private void CreatePaymentStatusHistory1()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistory.SystemGeneratedIdentifier + 1;
    var effectiveDate = Now().Date;
    var discontinueDate = local.Maximized.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void CreatePaymentStatusHistory2()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.Warrant.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = Now().Date;
    var discontinueDate = local.Maximized.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void DisassociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransaction.Populated = false;
    Update("DisassociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.PrqGeneratedId = null;
    entities.DisbursementTransaction.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
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
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

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
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
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

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          entities.DisbursementTransaction.ColId.GetValueOrDefault());
        db.SetInt32(
          command, "otyId",
          entities.DisbursementTransaction.OtyId.GetValueOrDefault());
        db.SetInt32(
          command, "obgId",
          entities.DisbursementTransaction.ObgId.GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.DisbursementTransaction.CspNumberDisb ?? "");
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaTypeDisb ?? ""
          );
        db.SetInt32(
          command, "otrId",
          entities.DisbursementTransaction.OtrId.GetValueOrDefault());
        db.SetString(
          command, "otrType", entities.DisbursementTransaction.OtrTypeDisb ?? ""
          );
        db.SetInt32(
          command, "crtType",
          entities.DisbursementTransaction.CrtId.GetValueOrDefault());
        db.SetInt32(
          command, "cstId",
          entities.DisbursementTransaction.CstId.GetValueOrDefault());
        db.SetInt32(
          command, "crvId",
          entities.DisbursementTransaction.CrvId.GetValueOrDefault());
        db.SetInt32(
          command, "crdId",
          entities.DisbursementTransaction.CrdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
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

  private bool ReadDisbursementTransaction1()
  {
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          local.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", export.PaymentRequest.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.DisbursementTransaction.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 11);
        entities.DisbursementTransaction.OtrId =
          db.GetNullableInt32(reader, 12);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 14);
        entities.DisbursementTransaction.ObgId =
          db.GetNullableInt32(reader, 15);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 16);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 17);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 18);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 19);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 20);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 21);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 22);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.DisbursementTransaction.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 11);
        entities.DisbursementTransaction.OtrId =
          db.GetNullableInt32(reader, 12);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 14);
        entities.DisbursementTransaction.ObgId =
          db.GetNullableInt32(reader, 15);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 16);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 17);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 18);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 19);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 20);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 21);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 22);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction3()
  {
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.DisbursementTransaction.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 11);
        entities.DisbursementTransaction.OtrId =
          db.GetNullableInt32(reader, 12);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 14);
        entities.DisbursementTransaction.ObgId =
          db.GetNullableInt32(reader, 15);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 16);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 17);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 18);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 19);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 20);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 21);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 22);
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionDisbursementType()
  {
    return ReadEach("ReadDisbursementTransactionDisbursementType",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Disbursements.IsFull)
        {
          return false;
        }

        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 8);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 9);
        entities.DisbursementTransaction.OtyId =
          db.GetNullableInt32(reader, 10);
        entities.DisbursementTransaction.OtrTypeDisb =
          db.GetNullableString(reader, 11);
        entities.DisbursementTransaction.OtrId =
          db.GetNullableInt32(reader, 12);
        entities.DisbursementTransaction.CpaTypeDisb =
          db.GetNullableString(reader, 13);
        entities.DisbursementTransaction.CspNumberDisb =
          db.GetNullableString(reader, 14);
        entities.DisbursementTransaction.ObgId =
          db.GetNullableInt32(reader, 15);
        entities.DisbursementTransaction.CrdId =
          db.GetNullableInt32(reader, 16);
        entities.DisbursementTransaction.CrvId =
          db.GetNullableInt32(reader, 17);
        entities.DisbursementTransaction.CstId =
          db.GetNullableInt32(reader, 18);
        entities.DisbursementTransaction.CrtId =
          db.GetNullableInt32(reader, 19);
        entities.DisbursementTransaction.ColId =
          db.GetNullableInt32(reader, 20);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 21);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 22);
        entities.DisbursementType.Code = db.GetString(reader, 23);
        entities.DisbursementType.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbursementTransaction.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbursementTransaction.CpaTypeDisb);

        return true;
      });
  }

  private bool ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId",
          import.ElectronicFundTransmission.TransmissionIdentifier);
      },
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 0);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 1);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 2);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 3);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 5);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 6);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 7);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 8);
        entities.ElectronicFundTransmission.Populated = true;
      });
  }

  private bool ReadPaymentRequest1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ElectronicFundTransmission.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.ElectronicFundTransmission.PrqGeneratedId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Classification = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 8);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.ReissuedTo.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReissuedTo.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReissuedTo.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.ReissuedTo.Number = db.GetNullableString(reader, 2);
        entities.ReissuedTo.Type1 = db.GetString(reader, 3);
        entities.ReissuedTo.PrqRGeneratedId = db.GetNullableInt32(reader, 4);
        entities.ReissuedTo.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.ReissuedTo.Type1);
      });
  }

  private bool ReadPaymentRequestPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequestPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Maximized.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ImprestFundCode =
          db.GetNullableString(reader, 5);
        entities.PaymentRequest.Classification = db.GetString(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentRequest.InterstateInd = db.GetNullableString(reader, 8);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 9);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 11);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 12);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 13);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatus1()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus1",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus2",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
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
          command, "discontinueDate", local.Maximized.Date.GetValueOrDefault());
          
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
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatus.Code = db.GetString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPersonPreferredPaymentMethod()
  {
    entities.TurnOffDueToReissuAsWar.Populated = false;

    return Read("ReadPersonPreferredPaymentMethod",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cspPNumber", export.Payee.Number);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.TurnOffDueToReissuAsWar.PmtGeneratedId =
          db.GetInt32(reader, 0);
        entities.TurnOffDueToReissuAsWar.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.TurnOffDueToReissuAsWar.EffectiveDate = db.GetDate(reader, 2);
        entities.TurnOffDueToReissuAsWar.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.TurnOffDueToReissuAsWar.LastUpdateBy =
          db.GetNullableString(reader, 4);
        entities.TurnOffDueToReissuAsWar.LastUpdateTmst =
          db.GetNullableDateTime(reader, 5);
        entities.TurnOffDueToReissuAsWar.CspPNumber = db.GetString(reader, 6);
        entities.TurnOffDueToReissuAsWar.Populated = true;
      });
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = Now().Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
  }

  private void UpdatePersonPreferredPaymentMethod()
  {
    System.Diagnostics.Debug.Assert(entities.TurnOffDueToReissuAsWar.Populated);

    var discontinueDate = Now().Date;
    var lastUpdateBy = global.UserId;
    var lastUpdateTmst = Now();

    entities.TurnOffDueToReissuAsWar.Populated = false;
    Update("UpdatePersonPreferredPaymentMethod",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdateBy", lastUpdateBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetInt32(
          command, "pmtGeneratedId",
          entities.TurnOffDueToReissuAsWar.PmtGeneratedId);
        db.SetInt32(
          command, "persnPmntMethId",
          entities.TurnOffDueToReissuAsWar.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspPNumber", entities.TurnOffDueToReissuAsWar.CspPNumber);
      });

    entities.TurnOffDueToReissuAsWar.DiscontinueDate = discontinueDate;
    entities.TurnOffDueToReissuAsWar.LastUpdateBy = lastUpdateBy;
    entities.TurnOffDueToReissuAsWar.LastUpdateTmst = lastUpdateTmst;
    entities.TurnOffDueToReissuAsWar.Populated = true;
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
    /// <summary>A DisbursementsGroup group.</summary>
    [Serializable]
    public class DisbursementsGroup
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
      /// A value of DetailDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("detailDisbursementTransaction")]
      public DisbursementTransaction DetailDisbursementTransaction
      {
        get => detailDisbursementTransaction ??= new();
        set => detailDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of DetailDisbursementType.
      /// </summary>
      [JsonPropertyName("detailDisbursementType")]
      public DisbursementType DetailDisbursementType
      {
        get => detailDisbursementType ??= new();
        set => detailDisbursementType = value;
      }

      /// <summary>
      /// A value of DetailDisbCode.
      /// </summary>
      [JsonPropertyName("detailDisbCode")]
      public TextWorkArea DetailDisbCode
      {
        get => detailDisbCode ??= new();
        set => detailDisbCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private DisbursementTransaction detailDisbursementTransaction;
      private DisbursementType detailDisbursementType;
      private TextWorkArea detailDisbCode;
    }

    /// <summary>
    /// A value of ReissuedTo.
    /// </summary>
    [JsonPropertyName("reissuedTo")]
    public PaymentRequest ReissuedTo
    {
      get => reissuedTo ??= new();
      set => reissuedTo = value;
    }

    /// <summary>
    /// A value of ReissuedDate.
    /// </summary>
    [JsonPropertyName("reissuedDate")]
    public DateWorkArea ReissuedDate
    {
      get => reissuedDate ??= new();
      set => reissuedDate = value;
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
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
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
    /// A value of IssuedTo.
    /// </summary>
    [JsonPropertyName("issuedTo")]
    public CsePersonsWorkSet IssuedTo
    {
      get => issuedTo ??= new();
      set => issuedTo = value;
    }

    /// <summary>
    /// A value of EftNumber.
    /// </summary>
    [JsonPropertyName("eftNumber")]
    public Standard EftNumber
    {
      get => eftNumber ??= new();
      set => eftNumber = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
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
    /// Gets a value of Disbursements.
    /// </summary>
    [JsonIgnore]
    public Array<DisbursementsGroup> Disbursements => disbursements ??= new(
      DisbursementsGroup.Capacity);

    /// <summary>
    /// Gets a value of Disbursements for json serialization.
    /// </summary>
    [JsonPropertyName("disbursements")]
    [Computed]
    public IList<DisbursementsGroup> Disbursements_Json
    {
      get => disbursements;
      set => Disbursements.Assign(value);
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
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private PaymentRequest reissuedTo;
    private DateWorkArea reissuedDate;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet designatedPayee;
    private PaymentRequest paymentRequest;
    private CsePersonsWorkSet issuedTo;
    private Standard eftNumber;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentStatus paymentStatus;
    private Array<DisbursementsGroup> disbursements;
    private NextTranInfo hidden;
    private Standard standard;
    private ElectronicFundTransmission electronicFundTransmission;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DisbursementsGroup group.</summary>
    [Serializable]
    public class DisbursementsGroup
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
      /// A value of DetailDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("detailDisbursementTransaction")]
      public DisbursementTransaction DetailDisbursementTransaction
      {
        get => detailDisbursementTransaction ??= new();
        set => detailDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of DetailDisbursementType.
      /// </summary>
      [JsonPropertyName("detailDisbursementType")]
      public DisbursementType DetailDisbursementType
      {
        get => detailDisbursementType ??= new();
        set => detailDisbursementType = value;
      }

      /// <summary>
      /// A value of DetailDisbCode.
      /// </summary>
      [JsonPropertyName("detailDisbCode")]
      public TextWorkArea DetailDisbCode
      {
        get => detailDisbCode ??= new();
        set => detailDisbCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private DisbursementTransaction detailDisbursementTransaction;
      private DisbursementType detailDisbursementType;
      private TextWorkArea detailDisbCode;
    }

    /// <summary>
    /// A value of ReissuedTo.
    /// </summary>
    [JsonPropertyName("reissuedTo")]
    public PaymentRequest ReissuedTo
    {
      get => reissuedTo ??= new();
      set => reissuedTo = value;
    }

    /// <summary>
    /// A value of ReissuedDate.
    /// </summary>
    [JsonPropertyName("reissuedDate")]
    public DateWorkArea ReissuedDate
    {
      get => reissuedDate ??= new();
      set => reissuedDate = value;
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
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
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
    /// A value of IssuedTo.
    /// </summary>
    [JsonPropertyName("issuedTo")]
    public CsePersonsWorkSet IssuedTo
    {
      get => issuedTo ??= new();
      set => issuedTo = value;
    }

    /// <summary>
    /// A value of EftNumber.
    /// </summary>
    [JsonPropertyName("eftNumber")]
    public Standard EftNumber
    {
      get => eftNumber ??= new();
      set => eftNumber = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
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
    /// Gets a value of Disbursements.
    /// </summary>
    [JsonIgnore]
    public Array<DisbursementsGroup> Disbursements => disbursements ??= new(
      DisbursementsGroup.Capacity);

    /// <summary>
    /// Gets a value of Disbursements for json serialization.
    /// </summary>
    [JsonPropertyName("disbursements")]
    [Computed]
    public IList<DisbursementsGroup> Disbursements_Json
    {
      get => disbursements;
      set => Disbursements.Assign(value);
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
    /// A value of PassThruFlowObligation.
    /// </summary>
    [JsonPropertyName("passThruFlowObligation")]
    public Obligation PassThruFlowObligation
    {
      get => passThruFlowObligation ??= new();
      set => passThruFlowObligation = value;
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
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private PaymentRequest reissuedTo;
    private DateWorkArea reissuedDate;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet designatedPayee;
    private PaymentRequest paymentRequest;
    private CsePersonsWorkSet issuedTo;
    private Standard eftNumber;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentStatus paymentStatus;
    private Array<DisbursementsGroup> disbursements;
    private NextTranInfo hidden;
    private Standard standard;
    private Obligation passThruFlowObligation;
    private CashReceiptDetail passThruFlowCashReceiptDetail;
    private CashReceiptEvent passThruFlowCashReceiptEvent;
    private CashReceiptSourceType passThruFlowCashReceiptSourceType;
    private CashReceiptType passThruFlowCashReceiptType;
    private CollectionType passThruFlowCollectionType;
    private CsePerson passThruFlowCsePerson;
    private ElectronicFundTransmission electronicFundTransmission;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitializeWarrant.
    /// </summary>
    [JsonPropertyName("initializeWarrant")]
    public PaymentRequest InitializeWarrant
    {
      get => initializeWarrant ??= new();
      set => initializeWarrant = value;
    }

    /// <summary>
    /// A value of NbrOfCreateAttempts.
    /// </summary>
    [JsonPropertyName("nbrOfCreateAttempts")]
    public Common NbrOfCreateAttempts
    {
      get => nbrOfCreateAttempts ??= new();
      set => nbrOfCreateAttempts = value;
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
    /// A value of Maximized.
    /// </summary>
    [JsonPropertyName("maximized")]
    public DateWorkArea Maximized
    {
      get => maximized ??= new();
      set => maximized = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private PaymentRequest initializeWarrant;
    private Common nbrOfCreateAttempts;
    private PaymentStatusHistory paymentStatusHistory;
    private DateWorkArea maximized;
    private Common common;
    private DisbursementTransaction disbursementTransaction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    public PaymentRequest Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of ReissuedTo.
    /// </summary>
    [JsonPropertyName("reissuedTo")]
    public PaymentRequest ReissuedTo
    {
      get => reissuedTo ??= new();
      set => reissuedTo = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PersonPreferredPaymentMethod.
    /// </summary>
    [JsonPropertyName("personPreferredPaymentMethod")]
    public PersonPreferredPaymentMethod PersonPreferredPaymentMethod
    {
      get => personPreferredPaymentMethod ??= new();
      set => personPreferredPaymentMethod = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of TurnOffDueToReissuAsWar.
    /// </summary>
    [JsonPropertyName("turnOffDueToReissuAsWar")]
    public PersonPreferredPaymentMethod TurnOffDueToReissuAsWar
    {
      get => turnOffDueToReissuAsWar ??= new();
      set => turnOffDueToReissuAsWar = value;
    }

    private PaymentRequest warrant;
    private PaymentRequest reissuedTo;
    private ElectronicFundTransmission electronicFundTransmission;
    private DisbursementType disbursementType;
    private CsePersonAccount csePersonAccount;
    private DisbursementTransaction disbursementTransaction;
    private PaymentStatusHistory paymentStatusHistory;
    private PersonPreferredPaymentMethod personPreferredPaymentMethod;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private DisbursementTransactionRln disbursementTransactionRln;
    private Collection collection;
    private CsePerson csePerson;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private ObligationTransaction obligationTransaction;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private PersonPreferredPaymentMethod turnOffDueToReissuAsWar;
  }
#endregion
}

// Program: FN_PROCESS_SDSO_COLL_DTL_RECORD, ID: 372428127, model: 746.
// Short name: SWE01675
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_SDSO_COLL_DTL_RECORD.
/// </summary>
[Serializable]
public partial class FnProcessSdsoCollDtlRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_SDSO_COLL_DTL_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessSdsoCollDtlRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessSdsoCollDtlRecord.
  /// </summary>
  public FnProcessSdsoCollDtlRecord(IContext context, Import import,
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
    // 04/01/2000 SWSRPDP   H00092103  -   Added check of OFFSET_CLOSED Flag 
    // because Receipt_Refund could be MANUALLY closed and NOT associated to a
    // Cash_Receipt_Detail
    // *****
    // Hardcoded Area.
    // *****
    local.HardcodedPotentialRecovery.SystemGeneratedIdentifier = 27;
    local.HardcodedPotentialRecovery.Code = "RCVPOT";
    UseFnHardcodedCashReceipting();
    local.HardcodedViews.HardcodedInterfund.SystemGeneratedIdentifier = 10;
    export.PndCollections.Count = import.PndCollections.Count;
    export.RecCollections.Count = import.RecCollections.Count;
    export.RefCollections.Count = import.RefCollections.Count;
    export.RelCollections.Count = import.RelCollections.Count;
    export.SusCollections.Count = import.SusCollections.Count;
    export.PndCollectionsAmt.TotalCurrency =
      import.PndCollectionsAmt.TotalCurrency;
    export.RecCollectionsAmt.TotalCurrency =
      import.RecCollectionsAmt.TotalCurrency;
    export.RefCollectionsAmt.TotalCurrency =
      import.RefCollectionsAmt.TotalCurrency;
    export.RelCollectionsAmt.TotalCurrency =
      import.RelCollectionsAmt.TotalCurrency;
    export.SusCollectionsAmt.TotalCurrency =
      import.SusCollectionsAmt.TotalCurrency;

    // *****
    // Make sure currency has not been lost.  If it has, reread the persistent 
    // view.
    // *****
    if (!import.P.Populated)
    {
      if (ReadCashReceipt2())
      {
        export.ImportCashReceipt.SequentialNumber = import.P.SequentialNumber;
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }
    }

    // *****
    // Set the local variables to the appropriate Cash Receipt values.
    // *****
    local.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedInterfund.SystemGeneratedIdentifier;
    local.CashReceipt.SequentialNumber =
      export.ImportCashReceipt.SequentialNumber;
    local.CsePersonsWorkSet.Number =
      import.Detail.DetailDetail.ObligorPersonNumber ?? Spaces(10);
    UseSiReadCsePersonBatch();
    local.CashReceiptDetail.ObligorPhoneNumber = "";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // 01/06/1999  SWSRPDP  Format the Obligor Home-Phone for the 
      // Cash_Receipt_Detail
      if (local.HomePhone.HomePhone.GetValueOrDefault() > 0)
      {
        if (local.HomePhone.HomePhoneAreaCode.GetValueOrDefault() > 0)
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.HomePhone.HomePhoneAreaCode.
              GetValueOrDefault(), 13, 3);
          local.CashReceiptDetail.ObligorPhoneNumber =
            Substring(local.CashReceiptDetail.ObligorPhoneNumber, 12, 1, 3) + NumberToString
            (local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
        }
        else
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
            
        }
      }
    }
    else if (IsExitState("CSE_PERSON_NF"))
    {
      // 12/30/98  If person is NOT FOUND, create the cash_detail               
      // and it will be SUSPENDED later in the processing.
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      return;
    }

    // *****
    // Set up the cash receipt detail in preparation for recording it.
    // *****
    // 01/04/1998   SWSRPDP   Set Sequence to AVOID already exist condition on 
    // MIXED Collection Types.
    local.Sequence.SequentialIdentifier = 0;

    if (ReadCashReceiptDetail2())
    {
      local.Sequence.SequentialIdentifier =
        entities.CashReceiptDetail.SequentialIdentifier;
    }

    export.ImportNextCrdId.SequentialIdentifier =
      local.Sequence.SequentialIdentifier + 1;
    MoveCashReceiptDetail2(import.Detail.DetailDetail, local.CashReceiptDetail);
    local.CashReceiptDetail.ReceivedAmount =
      import.Detail.DetailDetail.CollectionAmount;
    local.CashReceiptDetail.CollectionAmount =
      import.Detail.DetailDetail.CollectionAmount;
    local.CashReceiptDetail.SequentialIdentifier =
      export.ImportNextCrdId.SequentialIdentifier;

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * *
    // 01/20/1999   SWSRPDP  -- SSN Should NOT be set for SDSO
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * *
    local.CashReceiptDetail.ObligorSocialSecurityNumber =
      local.CsePersonsWorkSet.Ssn;
    local.CashReceiptDetail.ObligorFirstName =
      local.CsePersonsWorkSet.FirstName;
    local.CashReceiptDetail.ObligorLastName = local.CsePersonsWorkSet.LastName;
    local.CashReceiptDetail.ObligorMiddleName =
      local.CsePersonsWorkSet.MiddleInitial;
    local.CashReceiptDetail.CollectionDate =
      import.SourceCreationCashReceiptEvent.SourceCreationDate;

    // *****
    // Read the appropriate Collection Type to pass to the Record CAB.
    // *****
    local.ValidCollectionType.Flag = "Y";

    if (ReadCollectionType())
    {
      MoveCollectionType(entities.CollectionType, local.CollectionType);
    }
    else
    {
      local.ValidCollectionType.Flag = "N";
    }

    // *****
    // Create the cash receipt detail now.
    // *****
    UseRecordCollection();

    // 01/07/98  SWSRPDP   Initialize Fields.
    local.CashReceiptDetailStatus.SystemGeneratedIdentifier = 0;
    local.FullyAppliedFlag.Flag = "";

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****
      // Establish currency on the Cash Receipt Detail.  This will prevent 
      // additional reads subsequent action blocks, and allow for Receipt Refund
      // processing.
      // *****
      if (ReadCashReceiptDetail1())
      {
        // *****
        // Determine whether or not a Receipt Refund exists for the SDSO
        // *****
        // 01/04/1998  SWSRPDP  Added relationship to Collection_Type
        // - Per Sunya Sharp and Lori Glissman
        local.ReceiptFound.Flag = "N";

        if (AsChar(local.ValidCollectionType.Flag) == 'N')
        {
          goto Test3;
        }

        // 04/01/2000 SWSRPDP   H00092103  -   Added check of OFFSET_CLOSED Flag
        // because Receipt_Refund could be MANUALLY closed and NOT associated
        // to a Cash_Receipt_Detail
        // Used "is not equal" because Receipt_Refunds can contain EITHER " " or
        // "N" when NOT closed
        // * * *  Find REFUND that covers FULL COLLECTION Amount
        foreach(var item in ReadReceiptRefund2())
        {
          // *****
          // Determine if the Receipt Refund is already associated to a Cash 
          // Receipt Detail
          // *****
          if (ReadCashReceiptDetail3())
          {
            continue;
          }
          else
          {
            // *****
            // Continue Processing
            // *****
          }

          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // Refund is EQUAL to the Collection Amount from the 
          // Cash_Receipt_Detail
          // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
          // *****
          // When a Receipt Refund is found for the Cash Receipt Detail and the 
          // full amount of the Refund is covered.  Update the Receipt Refund
          // setting the Closed Offset indicator to 'Y'.
          // *****
          // * *
          // TOTAL Collection Amount has been Refunded
          // * *
          local.TotalRefunded.TotalCurrency = entities.ReceiptRefund.Amount;

          // 01/07/98  SWSRPDP **** NEED TO SET FULLY APPLIED AMOUNT
          // FLAG on Cash_Receipt_Detail
          local.FullyAppliedFlag.Flag = "Y";
          local.ReceiptFound.Flag = "Y";

          break;
        }

        // * * *  Find LARGEST REFUND that is LESS THAN the FULL COLLECTION 
        // Amount
        // 04/01/2000 SWSRPDP   H00092103  -   Added check of OFFSET_CLOSED Flag
        // because Receipt_Refund could be MANUALLY closed and NOT associated
        // to a Cash_Receipt_Detail
        // Used "is not equal" because Receipt_Refunds can contain EITHER " " or
        // "N" when NOT closed
        if (AsChar(local.ReceiptFound.Flag) == 'N')
        {
          foreach(var item in ReadReceiptRefund3())
          {
            // *****
            // Determine if the Receipt Refund is already associated to a Cash 
            // Receipt Detail
            // *****
            if (ReadCashReceiptDetail3())
            {
              continue;
            }
            else
            {
              // *****
              // Continue Processing
              // *****
            }

            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            // Refund is Less than the Collection Amount from the 
            // Cash_Receipt_Detail
            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            // ONLY Part of Collection Amount has been Refunded
            local.TotalRefunded.TotalCurrency = entities.ReceiptRefund.Amount;
            local.FullyAppliedFlag.Flag = "";
            local.ReceiptFound.Flag = "Y";

            goto Test1;
          }
        }

Test1:

        // * * *  Find SMALLEST REFUND that is MORE THAN the FULL COLLECTION 
        // Amount
        if (AsChar(local.ReceiptFound.Flag) == 'N')
        {
          // 04/01/2000 SWSRPDP   H00092103  -   Added check of OFFSET_CLOSED 
          // Flag because Receipt_Refund could be MANUALLY closed and NOT
          // associated to a Cash_Receipt_Detail
          // Used "is not equal" because Receipt_Refunds can contain EITHER " " 
          // or "N" when NOT closed
          foreach(var item in ReadReceiptRefund1())
          {
            // *****
            // Determine if the Receipt Refund is already associated to a Cash 
            // Receipt Detail
            // *****
            if (ReadCashReceiptDetail3())
            {
              continue;
            }
            else
            {
              // *****
              // Continue Processing
              // *****
            }

            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            // Refund is Greater than the Collection Amount from the 
            // Cash_Receipt_Detail
            // Create a PAYMENT_REQUEST
            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            local.PaymentRequest.Type1 = "RCV";
            local.PaymentRequest.Amount = entities.ReceiptRefund.Amount - entities
              .CashReceiptDetail.CollectionAmount;
            local.PaymentRequest.Classification = "REF";
            local.PaymentRequest.ImprestFundCode = "";
            local.PaymentRequest.InterstateInd = "N";
            local.PaymentRequest.CsePersonNumber =
              local.CashReceiptDetail.ObligorPersonNumber ?? "";
            local.PaymentRequest.DesignatedPayeeCsePersonNo =
              local.CashReceiptDetail.ObligorPersonNumber ?? "";
            local.PaymentRequest.RecoveryFiller = "";

            for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 10
              ; ++local.CreateAttempts.Count)
            {
              local.PaymentRequest.SystemGeneratedIdentifier =
                UseGenerate9DigitRandomNumber();

              try
              {
                CreatePaymentRequest();
                local.Date.ProcessDate = import.Save.Date;
                UseFnCreatePaymentStatusHist();

                // 05/20/99 This was added because the A/B was changed
                if (IsExitState("FN0000_PYMNT_STAT_HIST_NF"))
                {
                  ExitState = "ACO_NN0000_ALL_OK";
                }

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  return;
                }

                break;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

                    continue;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "FN0000_PYMNT_REQ_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }

            // TOTAL Collection Amount has been Refunded
            local.TotalRefunded.TotalCurrency =
              entities.CashReceiptDetail.CollectionAmount;
            local.ReceiptFound.Flag = "Y";
            local.FullyAppliedFlag.Flag = "Y";

            goto Test2;
          }
        }

Test2:

        // REFUND matched to Cash_Receipt_Detail -- Do Final Updates
        if (AsChar(local.ReceiptFound.Flag) == 'Y')
        {
          // *****************************************************************
          // 01/05/1998  SWSRPDP  Added update to ASSOCIATE receipt_refund with 
          // CORRECT cash_recietpt_detail.
          // *****************************************************************
          try
          {
            UpdateReceiptRefund();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_RCPT_REFUND_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_RCPT_REFUND_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          try
          {
            UpdateCashReceiptDetail();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0054_CASH_RCPT_DTL_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0056_CASH_RCPT_DTL_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          // *****************************************************************
          // *****************************************************************
          // CHANGE ** 01/06/98   SWSRPDP      Need to set STATUS to "REF"
          // *****************************************************************
          // *****************************************************************
          local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
            local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier;

          // 03/13/99  SWSRPDP   --  Change due to Testing.
          local.CashReceiptDetailStatHistory.ReasonCodeId = "REFUNDED";
          local.CashReceiptDetailStatHistory.ReasonCodeId = "";

          if (!ReadCashReceiptSourceType1())
          {
            ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

            return;
          }

          UseFnChangeCashRcptDtlStatHis1();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
          else
          {
            return;
          }
        }
      }
      else
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }
    }
    else
    {
      return;
    }

Test3:

    // *****
    // Release the CRD if the CSE Person # is found.  Suspend the Cash Receipt 
    // Detail if it is not.
    // *****
    if (ReadCsePerson())
    {
      if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier != local
        .HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier)
      {
        local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;
      }

      local.CseAddressFound.Flag = "F";
    }
    else
    {
      local.CseAddressFound.Flag = "Z";
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;

      // *****
      // Changed reason_code to "INVPERSNBR" per "UNIT TEST PLAN"             
      // for SWEB613.
      // *****
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
    }

    if (AsChar(local.ValidCollectionType.Flag) != 'Y' && local
      .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier)
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCOLTYPE";
    }

    if (!ReadCashReceiptDetail4())
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    UseFnChangeCashRcptDtlStatHis2();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // *****
    // Update Cash Receipt and Cash Receipt Event totals
    // *****
    if (ReadCashReceiptEvent())
    {
      // 05/11/1999  Was updating NON-Cash Amounts
      // Changed to Update Cash Amounts
      try
      {
        UpdateCashReceiptEvent();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0079_CASH_RCPT_EVENT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0080_CASH_RCPT_EVENT_PV";

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
      ExitState = "FN0077_CASH_RCPT_EVENT_NF";

      return;
    }

    if (ReadCashReceipt1())
    {
      // 05/11/1999  Was updating NON-Cash Amounts
      // Changed to Update Cash Amounts
      try
      {
        UpdateCashReceipt();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0088_CASH_RCPT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0090_CASH_RCPT_PV";

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
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // *****************************************************************
    // *****************************************************************
    // ALL PROCESSING O.K. --- ADD ADDRESS HERE
    // *****************************************************************
    // *****************************************************************
    if (AsChar(local.CseAddressFound.Flag) == 'F')
    {
      local.GetAddress.Number =
        import.Detail.DetailDetail.ObligorPersonNumber ?? Spaces(10);
      UseSiGetCsePersonMailingAddr();

      if (!IsEmpty(local.Returned.Street1))
      {
        local.Create.City = local.Returned.City ?? Spaces(30);
        local.Create.State = local.Returned.State ?? Spaces(2);
        local.Create.Street1 = local.Returned.Street1 ?? Spaces(25);
        local.Create.Street2 = local.Returned.Street2 ?? "";
        local.Create.ZipCode3 = local.Returned.Zip3 ?? "";
        local.Create.ZipCode4 = local.Returned.Zip4 ?? "";
        local.Create.ZipCode5 = local.Returned.ZipCode ?? Spaces(5);

        if (ReadCashReceiptSourceType2())
        {
          local.CreateAddressCashReceiptSourceType.SystemGeneratedIdentifier =
            entities.CashReceiptSourceType.SystemGeneratedIdentifier;
        }

        local.CreateAddressCashReceiptType.SystemGeneratedIdentifier =
          local.CashReceiptType.SystemGeneratedIdentifier;
        UseCreateCrDetailAddress();
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    // *****
    // Update the accumulating buckets for the cash receipt.  Going to keep a 
    // running total and then update the cash receipt(s) with the totals at the
    // end of processing.
    // *****
    if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier)
    {
      export.SusCollections.Count = import.SusCollections.Count + 1;
      export.SusCollectionsAmt.TotalCurrency =
        import.SusCollectionsAmt.TotalCurrency + import
        .Detail.DetailDetail.CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedPended.SystemGeneratedIdentifier)
    {
      export.PndCollections.Count = import.PndCollections.Count + 1;
      export.PndCollectionsAmt.TotalCurrency += import.Detail.DetailDetail.
        CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier)
    {
      export.RefCollections.Count = import.RefCollections.Count + 1;
      export.RefCollectionsAmt.TotalCurrency =
        import.RefCollectionsAmt.TotalCurrency + import
        .Detail.DetailDetail.CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier)
    {
      export.RelCollections.Count = import.RelCollections.Count + 1;
      export.RelCollectionsAmt.TotalCurrency =
        import.RelCollectionsAmt.TotalCurrency + import
        .Detail.DetailDetail.CollectionAmount;
    }
    else
    {
    }

    export.RecCollections.Count = import.RecCollections.Count + 1;
    export.RecCollectionsAmt.TotalCurrency =
      import.RecCollectionsAmt.TotalCurrency + import
      .Detail.DetailDetail.CollectionAmount;
    UseFnCabAccumulateNetTotal();
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private void UseCreateCrDetailAddress()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CreateAddressCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CreateAddressCashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailAddress.Assign(local.Create);
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.ImportNextCrdId.SequentialIdentifier;

    Call(CreateCrDetailAddress.Execute, useImport, useExport);

    local.Create.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
  }

  private void UseFnCabAccumulateNetTotal()
  {
    var useImport = new FnCabAccumulateNetTotal.Import();
    var useExport = new FnCabAccumulateNetTotal.Export();

    useImport.CashReceiptDetail.CollectionAmount =
      import.Detail.DetailDetail.CollectionAmount;
    useExport.ImportCashReceiptDetail.CollectionAmount =
      export.ImportCashReceiptDetail.CollectionAmount;
    useExport.ImportNet.TotalCurrency = export.ImportNet.TotalCurrency;
    useExport.ImportCashReceipt.Assign(export.ImportCashReceipt);

    Call(FnCabAccumulateNetTotal.Execute, useImport, useExport);

    export.ImportCashReceiptDetail.CollectionAmount =
      useExport.ImportCashReceiptDetail.CollectionAmount;
    export.ImportNet.TotalCurrency = useExport.ImportNet.TotalCurrency;
    export.ImportCashReceipt.Assign(useExport.ImportCashReceipt);
  }

  private void UseFnChangeCashRcptDtlStatHis1()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      local.CashReceiptDetailStatHistory.ReasonCodeId;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnChangeCashRcptDtlStatHis2()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.Persistent.Assign(entities.Persistent);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      local.CashReceiptDetailStatHistory.ReasonCodeId;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.Persistent.Assign(useImport.Persistent);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCreatePaymentStatusHist()
  {
    var useImport = new FnCreatePaymentStatusHist.Import();
    var useExport = new FnCreatePaymentStatusHist.Export();

    useImport.PaymentRequest.SystemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate = local.Date.ProcessDate;
    useImport.Requested.SystemGeneratedIdentifier =
      local.HardcodedPotentialRecovery.SystemGeneratedIdentifier;

    Call(FnCreatePaymentStatusHist.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail1(local.CashReceiptDetail, useImport.CashReceiptDetail);
      
    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useExport.CashReceiptDetail, local.CashReceiptDetail);
      
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.GetAddress.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Returned);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.HomePhone);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void CreatePaymentRequest()
  {
    var systemGeneratedIdentifier =
      local.PaymentRequest.SystemGeneratedIdentifier;
    var processDate = import.Save.Date;
    var amount = local.PaymentRequest.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var designatedPayeeCsePersonNo =
      local.PaymentRequest.DesignatedPayeeCsePersonNo ?? "";
    var csePersonNumber = local.PaymentRequest.CsePersonNumber ?? "";
    var imprestFundCode = local.PaymentRequest.ImprestFundCode ?? "";
    var classification = local.PaymentRequest.Classification;
    var recoveryFiller = local.PaymentRequest.RecoveryFiller;
    var type1 = local.PaymentRequest.Type1;
    var rctRTstamp = entities.ReceiptRefund.CreatedTimestamp;
    var interstateInd = local.PaymentRequest.InterstateInd ?? "";

    CheckValid<PaymentRequest>("Type1", type1);
    entities.PaymentRequest.Populated = false;
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
        db.SetString(command, "recoveryFiller", recoveryFiller);
        db.SetString(command, "recaptureFiller", "");
        db.SetNullableString(command, "achFormatCode", "");
        db.SetNullableString(command, "number", "");
        db.SetNullableDate(command, "printDate", default(DateTime));
        db.SetString(command, "type", type1);
        db.SetNullableDateTime(command, "rctRTstamp", rctRTstamp);
        db.SetNullableString(command, "interstateInd", interstateInd);
      });

    entities.PaymentRequest.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Amount = amount;
    entities.PaymentRequest.CreatedBy = createdBy;
    entities.PaymentRequest.CreatedTimestamp = createdTimestamp;
    entities.PaymentRequest.DesignatedPayeeCsePersonNo =
      designatedPayeeCsePersonNo;
    entities.PaymentRequest.CsePersonNumber = csePersonNumber;
    entities.PaymentRequest.ImprestFundCode = imprestFundCode;
    entities.PaymentRequest.Classification = classification;
    entities.PaymentRequest.RecoveryFiller = recoveryFiller;
    entities.PaymentRequest.Type1 = type1;
    entities.PaymentRequest.RctRTstamp = rctRTstamp;
    entities.PaymentRequest.PrqRGeneratedId = null;
    entities.PaymentRequest.InterstateInd = interstateInd;
    entities.PaymentRequest.Populated = true;
  }

  private bool ReadCashReceipt1()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 7);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 8);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 9);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 10);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 11);
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceipt2()
  {
    import.P.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.ImportCashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.P.CrvIdentifier = db.GetInt32(reader, 0);
        import.P.CstIdentifier = db.GetInt32(reader, 1);
        import.P.CrtIdentifier = db.GetInt32(reader, 2);
        import.P.SequentialNumber = db.GetInt32(reader, 3);
        import.P.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "cashReceiptId", import.P.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCashReceiptDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.Associated.Populated = false;

    return Read("ReadCashReceiptDetail3",
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
        entities.Associated.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Associated.CstIdentifier = db.GetInt32(reader, 1);
        entities.Associated.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Associated.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Associated.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail4()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.Persistent.Populated = false;

    return Read("ReadCashReceiptDetail4",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        entities.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Persistent.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Persistent.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 2);
        entities.CashReceiptEvent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 3);
        entities.CashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 4);
        entities.CashReceiptEvent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 5);
        entities.CashReceiptEvent.TotalNonCashAmt =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          import.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", import.Detail.DetailDetail.ObligorPersonNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund1()
  {
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "taxid",
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "");
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
        db.SetDecimal(
          command, "amount", local.CashReceiptDetail.CollectionAmount);
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 1);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 6);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 7);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund2()
  {
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "taxid",
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "");
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
        db.SetDecimal(
          command, "amount", local.CashReceiptDetail.CollectionAmount);
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 1);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 6);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 7);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund3()
  {
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "taxid",
          local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "");
        db.SetNullableInt32(
          command, "cltIdentifier",
          entities.CollectionType.SequentialIdentifier);
        db.SetDecimal(
          command, "amount", local.CashReceiptDetail.CollectionAmount);
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 1);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ReceiptRefund.OffsetTaxYear = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.OffsetClosed = db.GetString(reader, 6);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 7);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ReceiptRefund.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 9);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 10);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 11);
        entities.ReceiptRefund.Populated = true;
        CheckValid<ReceiptRefund>("OffsetClosed",
          entities.ReceiptRefund.OffsetClosed);

        return true;
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var totalCashTransactionAmount =
      entities.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault() +
      import.Detail.DetailDetail.CollectionAmount;
    var totalCashTransactionCount =
      entities.CashReceipt.TotalCashTransactionCount.GetValueOrDefault() + 1;
    var cashBalanceAmt =
      entities.CashReceipt.CashBalanceAmt.GetValueOrDefault() +
      import.Detail.DetailDetail.CollectionAmount;
    var cashBalanceReason = "UNDER";
    var cashDue =
      entities.CashReceipt.CashDue.GetValueOrDefault() +
      import.Detail.DetailDetail.CollectionAmount;

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.CashReceipt.CashBalanceReason = cashBalanceReason;
    entities.CashReceipt.CashDue = cashDue;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount = local.TotalRefunded.TotalCurrency;
    var collectionAmtFullyAppliedInd = local.FullyAppliedFlag.Flag;

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "refundedAmt", refundedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.RefundedAmount = refundedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var anticipatedCheckAmt =
      entities.CashReceiptEvent.AnticipatedCheckAmt.GetValueOrDefault() +
      import.Detail.DetailDetail.CollectionAmount;
    var totalCashAmt =
      entities.CashReceiptEvent.TotalCashAmt.GetValueOrDefault() +
      import.Detail.DetailDetail.CollectionAmount;
    var totalCashTransactionCount =
      entities.CashReceiptEvent.TotalCashTransactionCount.GetValueOrDefault() +
      1;

    entities.CashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "anticCheckAmt", anticipatedCheckAmt);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.CashReceiptEvent.AnticipatedCheckAmt = anticipatedCheckAmt;
    entities.CashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.CashReceiptEvent.TotalCashTransactionCount =
      totalCashTransactionCount;
    entities.CashReceiptEvent.Populated = true;
  }

  private void UpdateReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var crvIdentifier = entities.CashReceiptDetail.CrvIdentifier;
    var crdIdentifier = entities.CashReceiptDetail.SequentialIdentifier;
    var offsetClosed = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var crtIdentifier = entities.CashReceiptDetail.CrtIdentifier;
    var cstIdentifier = entities.CashReceiptDetail.CstIdentifier;

    CheckValid<ReceiptRefund>("OffsetClosed", offsetClosed);
    entities.ReceiptRefund.Populated = false;
    Update("UpdateReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(command, "crvIdentifier", crvIdentifier);
        db.SetNullableInt32(command, "crdIdentifier", crdIdentifier);
        db.SetString(command, "offsetClosed", offsetClosed);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "crtIdentifier", crtIdentifier);
        db.SetNullableInt32(command, "cstIdentifier", cstIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ReceiptRefund.CrvIdentifier = crvIdentifier;
    entities.ReceiptRefund.CrdIdentifier = crdIdentifier;
    entities.ReceiptRefund.OffsetClosed = offsetClosed;
    entities.ReceiptRefund.LastUpdatedBy = lastUpdatedBy;
    entities.ReceiptRefund.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ReceiptRefund.CrtIdentifier = crtIdentifier;
    entities.ReceiptRefund.CstIdentifier = cstIdentifier;
    entities.ReceiptRefund.Populated = true;
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
    /// <summary>A HeaderGroup group.</summary>
    [Serializable]
    public class HeaderGroup
    {
      /// <summary>
      /// A value of HeaderDetail.
      /// </summary>
      [JsonPropertyName("headerDetail")]
      public CashReceiptEvent HeaderDetail
      {
        get => headerDetail ??= new();
        set => headerDetail = value;
      }

      private CashReceiptEvent headerDetail;
    }

    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DetailDtlCollType.
      /// </summary>
      [JsonPropertyName("detailDtlCollType")]
      public Common DetailDtlCollType
      {
        get => detailDtlCollType ??= new();
        set => detailDtlCollType = value;
      }

      /// <summary>
      /// A value of DetailDetail.
      /// </summary>
      [JsonPropertyName("detailDetail")]
      public CashReceiptDetail DetailDetail
      {
        get => detailDetail ??= new();
        set => detailDetail = value;
      }

      private Common detailDtlCollType;
      private CashReceiptDetail detailDetail;
    }

    /// <summary>A TotalGroup group.</summary>
    [Serializable]
    public class TotalGroup
    {
      /// <summary>
      /// A value of TotalDetail.
      /// </summary>
      [JsonPropertyName("totalDetail")]
      public CashReceiptEvent TotalDetail
      {
        get => totalDetail ??= new();
        set => totalDetail = value;
      }

      private CashReceiptEvent totalDetail;
    }

    /// <summary>A TbdGrpImportCollectionRecordGroup group.</summary>
    [Serializable]
    public class TbdGrpImportCollectionRecordGroup
    {
      /// <summary>
      /// A value of TbdImportGrpDetailLocalCode.
      /// </summary>
      [JsonPropertyName("tbdImportGrpDetailLocalCode")]
      public TextWorkArea TbdImportGrpDetailLocalCode
      {
        get => tbdImportGrpDetailLocalCode ??= new();
        set => tbdImportGrpDetailLocalCode = value;
      }

      /// <summary>
      /// A value of TbdImportGrpDetail.
      /// </summary>
      [JsonPropertyName("tbdImportGrpDetail")]
      public CashReceiptDetail TbdImportGrpDetail
      {
        get => tbdImportGrpDetail ??= new();
        set => tbdImportGrpDetail = value;
      }

      /// <summary>
      /// A value of TbdImportGrpDetailAdjAmt.
      /// </summary>
      [JsonPropertyName("tbdImportGrpDetailAdjAmt")]
      public Common TbdImportGrpDetailAdjAmt
      {
        get => tbdImportGrpDetailAdjAmt ??= new();
        set => tbdImportGrpDetailAdjAmt = value;
      }

      private TextWorkArea tbdImportGrpDetailLocalCode;
      private CashReceiptDetail tbdImportGrpDetail;
      private Common tbdImportGrpDetailAdjAmt;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public DateWorkArea Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of SourceCreationCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("sourceCreationCashReceiptEvent")]
    public CashReceiptEvent SourceCreationCashReceiptEvent
    {
      get => sourceCreationCashReceiptEvent ??= new();
      set => sourceCreationCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SourceCreationDateWorkArea.
    /// </summary>
    [JsonPropertyName("sourceCreationDateWorkArea")]
    public DateWorkArea SourceCreationDateWorkArea
    {
      get => sourceCreationDateWorkArea ??= new();
      set => sourceCreationDateWorkArea = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// Gets a value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public HeaderGroup Header
    {
      get => header ?? (header = new());
      set => header = value;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public DetailGroup Detail
    {
      get => detail ?? (detail = new());
      set => detail = value;
    }

    /// <summary>
    /// Gets a value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public TotalGroup Total
    {
      get => total ?? (total = new());
      set => total = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CashReceipt P
    {
      get => p ??= new();
      set => p = value;
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
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of RecCollections.
    /// </summary>
    [JsonPropertyName("recCollections")]
    public Common RecCollections
    {
      get => recCollections ??= new();
      set => recCollections = value;
    }

    /// <summary>
    /// A value of RefCollections.
    /// </summary>
    [JsonPropertyName("refCollections")]
    public Common RefCollections
    {
      get => refCollections ??= new();
      set => refCollections = value;
    }

    /// <summary>
    /// A value of RelCollections.
    /// </summary>
    [JsonPropertyName("relCollections")]
    public Common RelCollections
    {
      get => relCollections ??= new();
      set => relCollections = value;
    }

    /// <summary>
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RecCollectionsAmt.
    /// </summary>
    [JsonPropertyName("recCollectionsAmt")]
    public Common RecCollectionsAmt
    {
      get => recCollectionsAmt ??= new();
      set => recCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RefCollectionsAmt.
    /// </summary>
    [JsonPropertyName("refCollectionsAmt")]
    public Common RefCollectionsAmt
    {
      get => refCollectionsAmt ??= new();
      set => refCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RelCollectionsAmt.
    /// </summary>
    [JsonPropertyName("relCollectionsAmt")]
    public Common RelCollectionsAmt
    {
      get => relCollectionsAmt ??= new();
      set => relCollectionsAmt = value;
    }

    /// <summary>
    /// A value of SusCollectionsAmt.
    /// </summary>
    [JsonPropertyName("susCollectionsAmt")]
    public Common SusCollectionsAmt
    {
      get => susCollectionsAmt ??= new();
      set => susCollectionsAmt = value;
    }

    /// <summary>
    /// Gets a value of TbdGrpImportCollectionRecord.
    /// </summary>
    [JsonPropertyName("tbdGrpImportCollectionRecord")]
    public TbdGrpImportCollectionRecordGroup TbdGrpImportCollectionRecord
    {
      get => tbdGrpImportCollectionRecord ?? (tbdGrpImportCollectionRecord = new
        ());
      set => tbdGrpImportCollectionRecord = value;
    }

    private DateWorkArea save;
    private CashReceiptEvent sourceCreationCashReceiptEvent;
    private DateWorkArea sourceCreationDateWorkArea;
    private CashReceipt cashReceipt;
    private CollectionType collectionType;
    private HeaderGroup header;
    private DetailGroup detail;
    private TotalGroup total;
    private CashReceipt p;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private Common pndCollections;
    private Common recCollections;
    private Common refCollections;
    private Common relCollections;
    private Common susCollections;
    private Common pndCollectionsAmt;
    private Common recCollectionsAmt;
    private Common refCollectionsAmt;
    private Common relCollectionsAmt;
    private Common susCollectionsAmt;
    private TbdGrpImportCollectionRecordGroup tbdGrpImportCollectionRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImportRecordsProcessed.
    /// </summary>
    [JsonPropertyName("importRecordsProcessed")]
    public ProgramControlTotal ImportRecordsProcessed
    {
      get => importRecordsProcessed ??= new();
      set => importRecordsProcessed = value;
    }

    /// <summary>
    /// A value of ImportRecordsRead.
    /// </summary>
    [JsonPropertyName("importRecordsRead")]
    public ProgramControlTotal ImportRecordsRead
    {
      get => importRecordsRead ??= new();
      set => importRecordsRead = value;
    }

    /// <summary>
    /// A value of ImportCashReceipt.
    /// </summary>
    [JsonPropertyName("importCashReceipt")]
    public CashReceipt ImportCashReceipt
    {
      get => importCashReceipt ??= new();
      set => importCashReceipt = value;
    }

    /// <summary>
    /// A value of ImportNextCrdId.
    /// </summary>
    [JsonPropertyName("importNextCrdId")]
    public CashReceiptDetail ImportNextCrdId
    {
      get => importNextCrdId ??= new();
      set => importNextCrdId = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    /// <summary>
    /// A value of ImportNet.
    /// </summary>
    [JsonPropertyName("importNet")]
    public Common ImportNet
    {
      get => importNet ??= new();
      set => importNet = value;
    }

    /// <summary>
    /// A value of ImportCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("importCashReceiptDetail")]
    public CashReceiptDetail ImportCashReceiptDetail
    {
      get => importCashReceiptDetail ??= new();
      set => importCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of RecCollections.
    /// </summary>
    [JsonPropertyName("recCollections")]
    public Common RecCollections
    {
      get => recCollections ??= new();
      set => recCollections = value;
    }

    /// <summary>
    /// A value of RefCollections.
    /// </summary>
    [JsonPropertyName("refCollections")]
    public Common RefCollections
    {
      get => refCollections ??= new();
      set => refCollections = value;
    }

    /// <summary>
    /// A value of RelCollections.
    /// </summary>
    [JsonPropertyName("relCollections")]
    public Common RelCollections
    {
      get => relCollections ??= new();
      set => relCollections = value;
    }

    /// <summary>
    /// A value of SusCollections.
    /// </summary>
    [JsonPropertyName("susCollections")]
    public Common SusCollections
    {
      get => susCollections ??= new();
      set => susCollections = value;
    }

    /// <summary>
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RecCollectionsAmt.
    /// </summary>
    [JsonPropertyName("recCollectionsAmt")]
    public Common RecCollectionsAmt
    {
      get => recCollectionsAmt ??= new();
      set => recCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RefCollectionsAmt.
    /// </summary>
    [JsonPropertyName("refCollectionsAmt")]
    public Common RefCollectionsAmt
    {
      get => refCollectionsAmt ??= new();
      set => refCollectionsAmt = value;
    }

    /// <summary>
    /// A value of RelCollectionsAmt.
    /// </summary>
    [JsonPropertyName("relCollectionsAmt")]
    public Common RelCollectionsAmt
    {
      get => relCollectionsAmt ??= new();
      set => relCollectionsAmt = value;
    }

    /// <summary>
    /// A value of SusCollectionsAmt.
    /// </summary>
    [JsonPropertyName("susCollectionsAmt")]
    public Common SusCollectionsAmt
    {
      get => susCollectionsAmt ??= new();
      set => susCollectionsAmt = value;
    }

    private ProgramControlTotal importRecordsProcessed;
    private ProgramControlTotal importRecordsRead;
    private CashReceipt importCashReceipt;
    private CashReceiptDetail importNextCrdId;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
    private Common importNet;
    private CashReceiptDetail importCashReceiptDetail;
    private Common pndCollections;
    private Common recCollections;
    private Common refCollections;
    private Common relCollections;
    private Common susCollections;
    private Common pndCollectionsAmt;
    private Common recCollectionsAmt;
    private Common refCollectionsAmt;
    private Common relCollectionsAmt;
    private Common susCollectionsAmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedInterfund.
      /// </summary>
      [JsonPropertyName("hardcodedInterfund")]
      public CashReceiptType HardcodedInterfund
      {
        get => hardcodedInterfund ??= new();
        set => hardcodedInterfund = value;
      }

      /// <summary>
      /// A value of HardcodedReleased.
      /// </summary>
      [JsonPropertyName("hardcodedReleased")]
      public CashReceiptDetailStatus HardcodedReleased
      {
        get => hardcodedReleased ??= new();
        set => hardcodedReleased = value;
      }

      /// <summary>
      /// A value of HardcodedRecorded.
      /// </summary>
      [JsonPropertyName("hardcodedRecorded")]
      public CashReceiptDetailStatus HardcodedRecorded
      {
        get => hardcodedRecorded ??= new();
        set => hardcodedRecorded = value;
      }

      /// <summary>
      /// A value of HardcodedSuspended.
      /// </summary>
      [JsonPropertyName("hardcodedSuspended")]
      public CashReceiptDetailStatus HardcodedSuspended
      {
        get => hardcodedSuspended ??= new();
        set => hardcodedSuspended = value;
      }

      /// <summary>
      /// A value of HardcodedRefunded.
      /// </summary>
      [JsonPropertyName("hardcodedRefunded")]
      public CashReceiptDetailStatus HardcodedRefunded
      {
        get => hardcodedRefunded ??= new();
        set => hardcodedRefunded = value;
      }

      /// <summary>
      /// A value of HardcodedPended.
      /// </summary>
      [JsonPropertyName("hardcodedPended")]
      public CashReceiptDetailStatus HardcodedPended
      {
        get => hardcodedPended ??= new();
        set => hardcodedPended = value;
      }

      private CashReceiptType hardcodedInterfund;
      private CashReceiptDetailStatus hardcodedReleased;
      private CashReceiptDetailStatus hardcodedRecorded;
      private CashReceiptDetailStatus hardcodedSuspended;
      private CashReceiptDetailStatus hardcodedRefunded;
      private CashReceiptDetailStatus hardcodedPended;
    }

    /// <summary>A TbdGrpLocalHardcodedViews1Group group.</summary>
    [Serializable]
    public class TbdGrpLocalHardcodedViews1Group
    {
      /// <summary>
      /// A value of TbdHardcodedPending1.
      /// </summary>
      [JsonPropertyName("tbdHardcodedPending1")]
      public CashReceiptDetailStatus TbdHardcodedPending1
      {
        get => tbdHardcodedPending1 ??= new();
        set => tbdHardcodedPending1 = value;
      }

      /// <summary>
      /// A value of TbdHardcodedReleased1.
      /// </summary>
      [JsonPropertyName("tbdHardcodedReleased1")]
      public CashReceiptDetailStatus TbdHardcodedReleased1
      {
        get => tbdHardcodedReleased1 ??= new();
        set => tbdHardcodedReleased1 = value;
      }

      /// <summary>
      /// A value of TbdHardcodedEft.
      /// </summary>
      [JsonPropertyName("tbdHardcodedEft")]
      public CashReceiptType TbdHardcodedEft
      {
        get => tbdHardcodedEft ??= new();
        set => tbdHardcodedEft = value;
      }

      /// <summary>
      /// A value of TbdHardcodedRecorded1.
      /// </summary>
      [JsonPropertyName("tbdHardcodedRecorded1")]
      public CashReceiptDetailStatus TbdHardcodedRecorded1
      {
        get => tbdHardcodedRecorded1 ??= new();
        set => tbdHardcodedRecorded1 = value;
      }

      /// <summary>
      /// A value of TbdHardcodedSuspended1.
      /// </summary>
      [JsonPropertyName("tbdHardcodedSuspended1")]
      public CashReceiptDetailStatus TbdHardcodedSuspended1
      {
        get => tbdHardcodedSuspended1 ??= new();
        set => tbdHardcodedSuspended1 = value;
      }

      /// <summary>
      /// A value of TbdHardcodedRefunded1.
      /// </summary>
      [JsonPropertyName("tbdHardcodedRefunded1")]
      public CashReceiptDetailStatus TbdHardcodedRefunded1
      {
        get => tbdHardcodedRefunded1 ??= new();
        set => tbdHardcodedRefunded1 = value;
      }

      private CashReceiptDetailStatus tbdHardcodedPending1;
      private CashReceiptDetailStatus tbdHardcodedReleased1;
      private CashReceiptType tbdHardcodedEft;
      private CashReceiptDetailStatus tbdHardcodedRecorded1;
      private CashReceiptDetailStatus tbdHardcodedSuspended1;
      private CashReceiptDetailStatus tbdHardcodedRefunded1;
    }

    /// <summary>
    /// A value of ValidCollectionType.
    /// </summary>
    [JsonPropertyName("validCollectionType")]
    public Common ValidCollectionType
    {
      get => validCollectionType ??= new();
      set => validCollectionType = value;
    }

    /// <summary>
    /// A value of Reason.
    /// </summary>
    [JsonPropertyName("reason")]
    public CashReceipt Reason
    {
      get => reason ??= new();
      set => reason = value;
    }

    /// <summary>
    /// A value of CreateAddressCashReceiptType.
    /// </summary>
    [JsonPropertyName("createAddressCashReceiptType")]
    public CashReceiptType CreateAddressCashReceiptType
    {
      get => createAddressCashReceiptType ??= new();
      set => createAddressCashReceiptType = value;
    }

    /// <summary>
    /// A value of CreateAddressCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("createAddressCashReceiptSourceType")]
    public CashReceiptSourceType CreateAddressCashReceiptSourceType
    {
      get => createAddressCashReceiptSourceType ??= new();
      set => createAddressCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public CashReceiptDetailAddress Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of GetAddress.
    /// </summary>
    [JsonPropertyName("getAddress")]
    public CsePerson GetAddress
    {
      get => getAddress ??= new();
      set => getAddress = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonAddress Returned
    {
      get => returned ??= new();
      set => returned = value;
    }

    /// <summary>
    /// A value of CseAddressFound.
    /// </summary>
    [JsonPropertyName("cseAddressFound")]
    public Common CseAddressFound
    {
      get => cseAddressFound ??= new();
      set => cseAddressFound = value;
    }

    /// <summary>
    /// A value of FullyAppliedFlag.
    /// </summary>
    [JsonPropertyName("fullyAppliedFlag")]
    public Common FullyAppliedFlag
    {
      get => fullyAppliedFlag ??= new();
      set => fullyAppliedFlag = value;
    }

    /// <summary>
    /// A value of HomePhone.
    /// </summary>
    [JsonPropertyName("homePhone")]
    public CsePerson HomePhone
    {
      get => homePhone ??= new();
      set => homePhone = value;
    }

    /// <summary>
    /// A value of Sequence.
    /// </summary>
    [JsonPropertyName("sequence")]
    public CashReceiptDetail Sequence
    {
      get => sequence ??= new();
      set => sequence = value;
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
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
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
    /// A value of RemainingOnCrd.
    /// </summary>
    [JsonPropertyName("remainingOnCrd")]
    public Common RemainingOnCrd
    {
      get => remainingOnCrd ??= new();
      set => remainingOnCrd = value;
    }

    /// <summary>
    /// A value of TotalRefunded.
    /// </summary>
    [JsonPropertyName("totalRefunded")]
    public Common TotalRefunded
    {
      get => totalRefunded ??= new();
      set => totalRefunded = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public ProgramProcessingInfo Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of HardcodedPotentialRecovery.
    /// </summary>
    [JsonPropertyName("hardcodedPotentialRecovery")]
    public PaymentStatus HardcodedPotentialRecovery
    {
      get => hardcodedPotentialRecovery ??= new();
      set => hardcodedPotentialRecovery = value;
    }

    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    /// <summary>
    /// Gets a value of TbdGrpLocalHardcodedViews1.
    /// </summary>
    [JsonPropertyName("tbdGrpLocalHardcodedViews1")]
    public TbdGrpLocalHardcodedViews1Group TbdGrpLocalHardcodedViews1
    {
      get => tbdGrpLocalHardcodedViews1 ?? (tbdGrpLocalHardcodedViews1 = new());
      set => tbdGrpLocalHardcodedViews1 = value;
    }

    /// <summary>
    /// A value of ReceiptFound.
    /// </summary>
    [JsonPropertyName("receiptFound")]
    public Common ReceiptFound
    {
      get => receiptFound ??= new();
      set => receiptFound = value;
    }

    private Common validCollectionType;
    private CashReceipt reason;
    private CashReceiptType createAddressCashReceiptType;
    private CashReceiptSourceType createAddressCashReceiptSourceType;
    private CashReceiptDetailAddress create;
    private CsePerson getAddress;
    private CsePersonAddress returned;
    private Common cseAddressFound;
    private Common fullyAppliedFlag;
    private CsePerson homePhone;
    private CashReceiptDetail sequence;
    private CsePersonsWorkSet csePersonsWorkSet;
    private HardcodedViewsGroup hardcodedViews;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private Common remainingOnCrd;
    private Common totalRefunded;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private PaymentRequest paymentRequest;
    private ProgramProcessingInfo date;
    private PaymentStatus hardcodedPotentialRecovery;
    private Common createAttempts;
    private TbdGrpLocalHardcodedViews1Group tbdGrpLocalHardcodedViews1;
    private Common receiptFound;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Associated.
    /// </summary>
    [JsonPropertyName("associated")]
    public CashReceiptDetail Associated
    {
      get => associated ??= new();
      set => associated = value;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CsePerson csePerson;
    private CashReceiptDetail associated;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetail persistent;
    private PaymentRequest paymentRequest;
  }
#endregion
}

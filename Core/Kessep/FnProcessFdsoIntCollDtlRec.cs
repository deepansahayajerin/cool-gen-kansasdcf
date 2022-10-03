// Program: FN_PROCESS_FDSO_INT_COLL_DTL_REC, ID: 372529959, model: 746.
// Short name: SWE01525
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_FDSO_INT_COLL_DTL_REC.
/// </summary>
[Serializable]
public partial class FnProcessFdsoIntCollDtlRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_FDSO_INT_COLL_DTL_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessFdsoIntCollDtlRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessFdsoIntCollDtlRec.
  /// </summary>
  public FnProcessFdsoIntCollDtlRec(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------------
    //                          M A I N T E N A N C E      L O G
    // ---------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	------------	
    // -----------------------------------------------------
    // ????????  ??????			Initial Development
    // 05/13/99  SWSRPDP			The layout (Exhibit N) says pos. 15-29 of the INPUT
    // 					file contains the CASE NYMBER. This is NOT correct.
    // 					The certification program is sending the Person
    // 					Number in this Field and the Feds are passing it back.
    // 04/01/00  SWSRPDP	H00092103	Added check of OFFSET_CLOSED Flag because 
    // Receipt_Refund
    // 					could be MANUALLY closed and NOT associated to a
    // 					Cash_Receipt_Detail
    // 11/01/17  GVandy	CQ60305		Receipt Manual Payments (MPY) as Z collection 
    // type.
    // ---------------------------------------------------------------------------------------------
    // *****
    // Hardcoded Area.
    // *****
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
    export.Swefb612CollectionCount.Count = import.Swefb612CollectionCount.Count;
    export.Swefb612CollectionAmt.TotalCurrency =
      import.Swefb612CollectionAmt.TotalCurrency;
    export.Swefb612NetAmt.TotalCurrency = import.Swefb612NetAmt.TotalCurrency;
    UseFnHardcodedCashReceipting();

    // *****
    // Was hardcoded to   10   - Interfunded  should be  EFT = 6
    // *****
    local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier = 6;
    local.HardcodedPotentialRecovery.Code = "RCVPOT";
    local.HardcodedPotentialRecovery.SystemGeneratedIdentifier = 27;

    // *****
    // Make sure currency has not been lost.  If it has, reread the persistent 
    // view.
    // *****
    if (!import.P.Populated)
    {
      if (ReadCashReceipt())
      {
        MoveCashReceipt(import.P, export.ImportCashReceipt);
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
      local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
    local.CashReceipt.SequentialNumber =
      export.ImportCashReceipt.SequentialNumber;

    // *****
    // Set up the cash receipt detail in preparation for recording it.
    // *****
    MoveCashReceiptDetail2(import.CollectionRecord.Detail,
      local.CashReceiptDetail);
    local.CashReceiptDetail.SequentialIdentifier = 0;

    if (ReadCashReceiptDetail3())
    {
      local.CashReceiptDetail.SequentialIdentifier =
        entities.MaxId.SequentialIdentifier;
    }

    ++local.CashReceiptDetail.SequentialIdentifier;
    local.CashReceiptDetail.ReceivedAmount =
      local.CashReceiptDetail.CollectionAmount;
    local.CashReceiptDetail.OffsetTaxid =
      (int?)StringToNumber(import.CollectionRecord.Detail.
        ObligorSocialSecurityNumber);
    local.CashReceiptDetail.CollectionDate = import.Save.Date;
    local.CashReceiptDetail.JointReturnInd =
      import.CollectionRecord.Detail.JointReturnInd ?? "";

    // *****
    // Read the appropriate Collection Type to pass to the Record CAB.
    // *****
    switch(TrimEnd(Substring(
      import.AdditionalInfo.Adjustment.InterfaceTransId, 1, 3)))
    {
      case "TAX":
        local.CollectionType.Code = "F";

        break;
      case "SAL":
        local.CollectionType.Code = "T";

        break;
      case "RET":
        local.CollectionType.Code = "Y";

        break;
      case "VEN":
        local.CollectionType.Code = "Z";

        break;
      case "ADM":
        local.CollectionType.Code = "T";

        break;
      case "MPY":
        // 11/01/17  GVandy  CQ60305  Receipt Manual Payments (MPY) as Z 
        // collection type.
        local.CollectionType.Code = "Z";

        break;
      default:
        local.CollectionType.Code = "";

        break;
    }

    local.ValidCollectionType.Flag = "N";

    if (!IsEmpty(local.CollectionType.Code))
    {
      if (ReadCollectionType())
      {
        MoveCollectionType(entities.CollectionType, local.CollectionType);
        local.ValidCollectionType.Flag = "Y";
      }
    }

    // Store the CASE TYPE in the Joint Return Name field as there is NO field 
    // in the system for it.
    // H00093019 - 06/15/00 -Need to display joint return name - so it was 
    // CONCATENATED after the Case Type information.
    local.SaveJointName.JointReturnName =
      import.CollectionRecord.Detail.JointReturnName;
    local.CashReceiptDetail.JointReturnName = "CASE TYPE = " + Substring
      (import.AdditionalInfo.Adjustment.InterfaceTransId, 14, 4, 1);

    if (CharAt(import.AdditionalInfo.Adjustment.InterfaceTransId, 4) == 'N')
    {
      local.CashReceiptDetail.JointReturnName =
        Substring(local.CashReceiptDetail.JointReturnName, 60, 1, 14) + " NON-TANF";
        
    }
    else
    {
      local.CashReceiptDetail.JointReturnName =
        Substring(local.CashReceiptDetail.JointReturnName, 60, 1, 14) + " TANF";
        
    }

    local.CashReceiptDetail.JointReturnName =
      Substring(local.CashReceiptDetail.JointReturnName, 60, 1, 25) + Substring
      (local.SaveJointName.JointReturnName, 60, 1, 35);

    // --- Check if SSN is valid. Also validate the corresponding Person_Number 
    // ---
    local.CsePersonsWorkSet.Assign(local.Blank);
    local.CsePersonsWorkSet.Ssn =
      import.CollectionRecord.Detail.ObligorSocialSecurityNumber ?? Spaces(9);
    local.SsnSearch.Flag = "1";
    UseEabReadCsePersonUsingSsn();

    if (IsEmpty(local.Adabas.AdabasFileAction) && IsEmpty
      (local.Adabas.AdabasFileNumber) && IsEmpty
      (local.Adabas.AdabasResponseCd))
    {
      // --- Successful retrieval ---
      if (!IsEmpty(local.CsePersonsWorkSet.Number))
      {
        local.ValidSsn.Flag = "Y";

        // 05/13/99  If person is NOT FOUND, create the cash_detail
        // and it will be SUSPENDED later in the processing.
        local.ValidPerson.Flag = "Y";

        // SSN is only Key.   Person Number may contain Value but this coulsd 
        // change and is optional.
        local.CashReceiptDetail.ObligorPersonNumber =
          local.CsePersonsWorkSet.Number;
        UseSiReadCsePersonBatch();
        ExitState = "ACO_NN0000_ALL_OK";
        local.CashReceiptDetail.ObligorPhoneNumber = "";

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
              Substring(local.CashReceiptDetail.ObligorPhoneNumber, 12, 1, 3) +
              NumberToString
              (local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
          }
          else
          {
            local.CashReceiptDetail.ObligorPhoneNumber =
              NumberToString(local.HomePhone.HomePhone.GetValueOrDefault(), 9, 7);
              
          }
        }
      }
      else
      {
        local.CashReceiptDetail.ObligorPersonNumber =
          import.CollectionRecord.Detail.ObligorPersonNumber ?? "";
        local.ValidSsn.Flag = "N";
      }
    }
    else
    {
      local.CashReceiptDetail.ObligorPersonNumber =
        import.CollectionRecord.Detail.ObligorPersonNumber ?? "";
      local.ValidSsn.Flag = "N";
    }

    local.CashReceiptDetail.ObligorFirstName =
      import.CollectionRecord.Detail.ObligorFirstName ?? "";
    local.CashReceiptDetail.ObligorLastName =
      import.CollectionRecord.Detail.ObligorLastName ?? "";

    // *****
    // The CASE_NUMBER is NO LONGER being populated
    // *****
    local.CashReceiptDetail.CaseNumber = "";

    // *****
    // Create the cash receipt detail now.
    // *****
    UseRecordCollection();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;

      // The following logic for Cash_Receipt_Refunds was               copied 
      // from the SDSO Program
      // *****
      // Establish currency on the Cash Receipt Detail.  This will prevent 
      // additional reads subsequent action blocks, and allow for Receipt Refund
      // processing.
      // *****
      if (ReadCashReceiptDetail1())
      {
        // *****
        // Determine whether or not a Receipt Refund exists for the FDSO
        // *****
        // 01/04/1998  SWSRPDP  Added relationship to Collection_Type
        // - Per Sunya Sharp and Lori Glissman
        if (!entities.CollectionType.Populated)
        {
          goto Test3;
        }

        local.ReceiptFound.Flag = "N";

        // H00092103  -  PDP - Add check of OFFSET_CLOSED Flag
        // * * *  Find REFUND that covers FULL COLLECTION Amount
        foreach(var item in ReadReceiptRefund2())
        {
          // *****
          // Determine if the Receipt Refund is already associated to a Cash 
          // Receipt Detail
          // *****
          if (ReadCashReceiptDetail2())
          {
            continue;
          }
          else
          {
            // *****
            // Continue Processing
            // *****
          }

          // * * * * * * * * * * * * * * * * * * * * * *
          // Changed to select on DATE
          // ONLY for a Collection Type of "F"
          // Per Tim Hood  memo on 06/08/99
          // * * * * * * * * * * * * * * * * * * * * * *
          if (Equal(entities.CollectionType.Code, "F"))
          {
            if (!Equal(entities.ReceiptRefund.OffsetTaxYear,
              local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault()))
            {
              continue;
            }
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

        // H00092103  -  PDP - Add check of OFFSET_CLOSED Flag
        // * * *  Find LARGEST REFUND that is LESS THAN the FULL COLLECTION 
        // Amount
        if (AsChar(local.ReceiptFound.Flag) == 'N')
        {
          foreach(var item in ReadReceiptRefund3())
          {
            // *****
            // Determine if the Receipt Refund is already associated to a Cash 
            // Receipt Detail
            // *****
            if (ReadCashReceiptDetail2())
            {
              continue;
            }
            else
            {
              // *****
              // Continue Processing
              // *****
            }

            // * * * * * * * * * * * * * * * * * * * * * *
            // Changed to select on DATE
            // ONLY for a Collection Type of "F"
            // Per Tim Hood  memo on 06/08/99
            // * * * * * * * * * * * * * * * * * * * * * *
            if (Equal(entities.CollectionType.Code, "F"))
            {
              if (!Equal(entities.ReceiptRefund.OffsetTaxYear,
                local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault()))
              {
                continue;
              }
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

        // H00092103  -  PDP - Add check of OFFSET_CLOSED Flag
        // * * *  Find SMALLEST REFUND that is MORE THAN the FULL COLLECTION 
        // Amount
        if (AsChar(local.ReceiptFound.Flag) == 'N')
        {
          foreach(var item in ReadReceiptRefund1())
          {
            // *****
            // Determine if the Receipt Refund is already associated to a Cash 
            // Receipt Detail
            // *****
            if (ReadCashReceiptDetail2())
            {
              continue;
            }
            else
            {
              // *****
              // Continue Processing
              // *****
            }

            // * * * * * * * * * * * * * * * * * * * * * *
            // Changed to select on DATE
            // ONLY for a Collection Type of "F"
            // Per Tim Hood  memo on 06/08/99
            // * * * * * * * * * * * * * * * * * * * * * *
            if (Equal(entities.CollectionType.Code, "F"))
            {
              if (!Equal(entities.ReceiptRefund.OffsetTaxYear,
                local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault()))
              {
                continue;
              }
            }

            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            // Refund is Greater than the Collection Amount from the 
            // Cash_Receipt_Detail
            // Create a PAYMENT_REQUEST
            // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
            local.PaymentRequest.Type1 = "RCV";
            local.PaymentRequest.Amount = entities.ReceiptRefund.Amount - local
              .CashReceiptDetail.CollectionAmount;
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
            UpdateCashReceiptDetail1();
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
          local.CashReceiptDetailStatHistory.ReasonCodeId = "REFUNDED";
          local.CashReceiptDetailStatHistory.ReasonCodeId = "";
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
    // Release the CRD if the SSN# is accurate.  Suspend the Cash Receipt Detail
    // if it is not.
    // *****
    // *****
    // Release the CRD if the CSE Person # is found.  Suspend the Cash Receipt 
    // Detail if it is not.
    // *****
    // *****
    // Will need to READ on CASE and CASE_ROLE for CSE Person #.  Suspend the 
    // Cash Receipt Detail if it is not.
    // *****
    if (AsChar(local.ValidSsn.Flag) != 'Y')
    {
      // INVALID SSN
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVSSN";
    }
    else if (AsChar(local.ValidPerson.Flag) != 'Y')
    {
      // INVALID Person Number
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
    }
    else
    {
    }

    if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier)
    {
    }
    else
    {
      UseFnChangeCashRcptDtlStatHis2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * *
    // INVALID COLLECTION TYPE - SUSPEND
    // * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
    // * *
    if (AsChar(local.ValidCollectionType.Flag) != 'Y' && local
      .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier)
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCOLTYPE";
      UseFnChangeCashRcptDtlStatHis2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    if (ReadCashReceiptDetail4())
    {
      if (Equal(local.CsePersonsWorkSet.FirstName,
        import.CollectionRecord.Detail.ObligorFirstName) && Equal
        (local.CsePersonsWorkSet.LastName,
        import.CollectionRecord.Detail.ObligorLastName))
      {
        if (!IsEmpty(local.CsePersonsWorkSet.MiddleInitial))
        {
          try
          {
            UpdateCashReceiptDetail2();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    // *****
    // 
    // ADD the Cash_Receipt_Detail_Address here from input Record
    // *****
    // Use the ADDRESS "WE" have NOT the input Address.
    local.GetAddress.Number = local.CashReceiptDetail.ObligorPersonNumber ?? Spaces
      (10);
    UseSiGetCsePersonMailingAddr();

    if (!IsEmpty(local.Returned.Street1) || !IsEmpty(local.Returned.City))
    {
      local.Create.City = local.Returned.City ?? Spaces(30);
      local.Create.State = local.Returned.State ?? Spaces(2);
      local.Create.Street1 = local.Returned.Street1 ?? Spaces(25);
      local.Create.Street2 = local.Returned.Street2 ?? "";
      local.Create.ZipCode3 = local.Returned.Zip3 ?? "";
      local.Create.ZipCode4 = local.Returned.Zip4 ?? "";
      local.Create.ZipCode5 = local.Returned.ZipCode ?? Spaces(5);
    }
    else
    {
      local.Create.City = import.AdditionalInfo.Address.City;
      local.Create.State = import.AdditionalInfo.Address.State;
      local.Create.Street1 = import.AdditionalInfo.Address.Street1;
      local.Create.Street2 = "";
      local.Create.ZipCode3 = "";
      local.Create.ZipCode4 = import.AdditionalInfo.Address.ZipCode4 ?? "";
      local.Create.ZipCode5 = import.AdditionalInfo.Address.ZipCode5;
    }

    UseCreateCrDetailAddress();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier)
    {
      export.SusCollections.Count = import.SusCollections.Count + 1;
      export.SusCollectionsAmt.TotalCurrency =
        import.SusCollectionsAmt.TotalCurrency + import
        .CollectionRecord.Detail.CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedPending.SystemGeneratedIdentifier)
    {
      export.PndCollections.Count = import.PndCollections.Count + 1;
      export.PndCollectionsAmt.TotalCurrency += import.CollectionRecord.Detail.
        CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier)
    {
      export.RefCollections.Count = import.RefCollections.Count + 1;
      export.RefCollectionsAmt.TotalCurrency =
        import.RefCollectionsAmt.TotalCurrency + import
        .CollectionRecord.Detail.CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier)
    {
      export.RelCollections.Count = import.RelCollections.Count + 1;
      export.RelCollectionsAmt.TotalCurrency =
        import.RelCollectionsAmt.TotalCurrency + import
        .CollectionRecord.Detail.CollectionAmount;
    }
    else
    {
    }

    export.RecCollections.Count = import.RecCollections.Count + 1;
    export.RecCollectionsAmt.TotalCurrency =
      import.RecCollectionsAmt.TotalCurrency + import
      .CollectionRecord.Detail.CollectionAmount;

    // **** Update cash receipt ****
    try
    {
      UpdateCashReceipt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // *****
    // Update the accumulating buckets for the cash receipt.  Going to keep a 
    // running total and then update the cash receipt(s) with the totals at the
    // end of processing.
    // *****
    export.Swefb612CollectionAmt.TotalCurrency += import.CollectionRecord.
      Detail.CollectionAmount;
    export.Swefb612NetAmt.TotalCurrency += import.CollectionRecord.Detail.
      CollectionAmount;
    ++export.Swefb612CollectionCount.Count;
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.SequentialNumber = source.SequentialNumber;
    target.TotalNoncashTransactionAmount = source.TotalNoncashTransactionAmount;
    target.TotalNoncashTransactionCount = source.TotalNoncashTransactionCount;
    target.TotalDetailAdjustmentCount = source.TotalDetailAdjustmentCount;
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
    target.InjuredSpouseInd = source.InjuredSpouseInd;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.InjuredSpouseInd = source.InjuredSpouseInd;
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
    target.TaxId = source.TaxId;
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

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCreateCrDetailAddress()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.Persistent.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedEft.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailAddress.Assign(local.Create);

    Call(CreateCrDetailAddress.Execute, useImport, useExport);

    local.Create.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    useExport.AbendData.Assign(local.Adabas);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    local.Adabas.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseFnChangeCashRcptDtlStatHis1()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
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

    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnChangeCashRcptDtlStatHis2()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
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

  private void UseFnCreatePaymentStatusHist()
  {
    var useImport = new FnCreatePaymentStatusHist.Import();
    var useExport = new FnCreatePaymentStatusHist.Export();

    useImport.PaymentRequest.SystemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;
    useImport.Requested.SystemGeneratedIdentifier =
      local.HardcodedPotentialRecovery.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate = local.Date.ProcessDate;

    Call(FnCreatePaymentStatusHist.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedPending.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
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

    useImport.PersistentCollectionType.Assign(entities.CollectionType);
    MoveCollectionType(local.CollectionType, useImport.CollectionType);
    MoveCashReceiptDetail1(local.CashReceiptDetail, useImport.CashReceiptDetail);
      
    useImport.CashReceipt.SequentialNumber =
      export.ImportCashReceipt.SequentialNumber;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    MoveCollectionType(useImport.PersistentCollectionType,
      entities.CollectionType);
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

  private bool ReadCashReceipt()
  {
    import.P.Populated = false;

    return Read("ReadCashReceipt",
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
        import.P.ReceiptAmount = db.GetDecimal(reader, 3);
        import.P.SequentialNumber = db.GetInt32(reader, 4);
        import.P.ReceiptDate = db.GetDate(reader, 5);
        import.P.CheckType = db.GetNullableString(reader, 6);
        import.P.CheckNumber = db.GetNullableString(reader, 7);
        import.P.CheckDate = db.GetNullableDate(reader, 8);
        import.P.ReceivedDate = db.GetDate(reader, 9);
        import.P.DepositReleaseDate = db.GetNullableDate(reader, 10);
        import.P.ReferenceNumber = db.GetNullableString(reader, 11);
        import.P.PayorOrganization = db.GetNullableString(reader, 12);
        import.P.PayorFirstName = db.GetNullableString(reader, 13);
        import.P.PayorMiddleName = db.GetNullableString(reader, 14);
        import.P.PayorLastName = db.GetNullableString(reader, 15);
        import.P.ForwardedToName = db.GetNullableString(reader, 16);
        import.P.ForwardedStreet1 = db.GetNullableString(reader, 17);
        import.P.ForwardedStreet2 = db.GetNullableString(reader, 18);
        import.P.ForwardedCity = db.GetNullableString(reader, 19);
        import.P.ForwardedState = db.GetNullableString(reader, 20);
        import.P.ForwardedZip5 = db.GetNullableString(reader, 21);
        import.P.ForwardedZip4 = db.GetNullableString(reader, 22);
        import.P.ForwardedZip3 = db.GetNullableString(reader, 23);
        import.P.BalancedTimestamp = db.GetNullableDateTime(reader, 24);
        import.P.TotalCashTransactionAmount = db.GetNullableDecimal(reader, 25);
        import.P.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        import.P.TotalCashTransactionCount = db.GetNullableInt32(reader, 27);
        import.P.TotalNoncashTransactionCount = db.GetNullableInt32(reader, 28);
        import.P.TotalDetailAdjustmentCount = db.GetNullableInt32(reader, 29);
        import.P.CreatedBy = db.GetString(reader, 30);
        import.P.CreatedTimestamp = db.GetDateTime(reader, 31);
        import.P.CashBalanceAmt = db.GetNullableDecimal(reader, 32);
        import.P.CashBalanceReason = db.GetNullableString(reader, 33);
        import.P.CashDue = db.GetNullableDecimal(reader, 34);
        import.P.TotalNonCashFeeAmount = db.GetNullableDecimal(reader, 35);
        import.P.TotalCashFeeAmount = db.GetNullableDecimal(reader, 36);
        import.P.Note = db.GetNullableString(reader, 37);
        import.P.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason", import.P.CashBalanceReason);
          
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
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

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.Associated.Populated = false;

    return Read("ReadCashReceiptDetail2",
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

  private bool ReadCashReceiptDetail3()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.MaxId.Populated = false;

    return Read("ReadCashReceiptDetail3",
      (db, command) =>
      {
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.MaxId.CrvIdentifier = db.GetInt32(reader, 0);
        entities.MaxId.CstIdentifier = db.GetInt32(reader, 1);
        entities.MaxId.CrtIdentifier = db.GetInt32(reader, 2);
        entities.MaxId.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.MaxId.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail4()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.Persistent.Populated = false;

    return Read("ReadCashReceiptDetail4",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        entities.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Persistent.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Persistent.ObligorMiddleName = db.GetNullableString(reader, 4);
        entities.Persistent.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", local.CollectionType.Code);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
    System.Diagnostics.Debug.Assert(import.P.Populated);

    var cashDue =
      import.P.CashDue.GetValueOrDefault() +
      import.CollectionRecord.Detail.CollectionAmount;

    import.P.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
      });

    import.P.CashDue = cashDue;
    import.P.Populated = true;
  }

  private void UpdateCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount = local.TotalRefunded.TotalCurrency;
    var collectionAmtFullyAppliedInd = local.FullyAppliedFlag.Flag;

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail1",
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

  private void UpdateCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.Persistent.Populated);

    var obligorMiddleName = local.CsePersonsWorkSet.MiddleInitial;

    entities.Persistent.Populated = false;
    Update("UpdateCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.
          SetInt32(command, "crvIdentifier", entities.Persistent.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.Persistent.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.Persistent.CrtIdentifier);
          
        db.SetInt32(command, "crdId", entities.Persistent.SequentialIdentifier);
      });

    entities.Persistent.ObligorMiddleName = obligorMiddleName;
    entities.Persistent.Populated = true;
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
    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of DetailLocalCode.
      /// </summary>
      [JsonPropertyName("detailLocalCode")]
      public TextWorkArea DetailLocalCode
      {
        get => detailLocalCode ??= new();
        set => detailLocalCode = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CashReceiptDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailAdjAmt.
      /// </summary>
      [JsonPropertyName("detailAdjAmt")]
      public Common DetailAdjAmt
      {
        get => detailAdjAmt ??= new();
        set => detailAdjAmt = value;
      }

      private TextWorkArea detailLocalCode;
      private CashReceiptDetail detail;
      private Common detailAdjAmt;
    }

    /// <summary>A AdditionalInfoGroup group.</summary>
    [Serializable]
    public class AdditionalInfoGroup
    {
      /// <summary>
      /// A value of Adjustment.
      /// </summary>
      [JsonPropertyName("adjustment")]
      public CashReceiptDetail Adjustment
      {
        get => adjustment ??= new();
        set => adjustment = value;
      }

      /// <summary>
      /// A value of Address.
      /// </summary>
      [JsonPropertyName("address")]
      public CashReceiptDetailAddress Address
      {
        get => address ??= new();
        set => address = value;
      }

      private CashReceiptDetail adjustment;
      private CashReceiptDetailAddress address;
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
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
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
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionAmt.
    /// </summary>
    [JsonPropertyName("swefb612CollectionAmt")]
    public Common Swefb612CollectionAmt
    {
      get => swefb612CollectionAmt ??= new();
      set => swefb612CollectionAmt = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionCount.
    /// </summary>
    [JsonPropertyName("swefb612CollectionCount")]
    public Common Swefb612CollectionCount
    {
      get => swefb612CollectionCount ??= new();
      set => swefb612CollectionCount = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmt.
    /// </summary>
    [JsonPropertyName("swefb612NetAmt")]
    public Common Swefb612NetAmt
    {
      get => swefb612NetAmt ??= new();
      set => swefb612NetAmt = value;
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
    /// Gets a value of CollectionRecord.
    /// </summary>
    [JsonPropertyName("collectionRecord")]
    public CollectionRecordGroup CollectionRecord
    {
      get => collectionRecord ?? (collectionRecord = new());
      set => collectionRecord = value;
    }

    /// <summary>
    /// Gets a value of AdditionalInfo.
    /// </summary>
    [JsonPropertyName("additionalInfo")]
    public AdditionalInfoGroup AdditionalInfo
    {
      get => additionalInfo ?? (additionalInfo = new());
      set => additionalInfo = value;
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

    private DateWorkArea save;
    private Common recCollectionsAmt;
    private Common refCollectionsAmt;
    private Common relCollectionsAmt;
    private Common susCollectionsAmt;
    private Common pndCollectionsAmt;
    private Common recCollections;
    private Common refCollections;
    private Common relCollections;
    private Common susCollections;
    private Common pndCollections;
    private Common swefb612CollectionAmt;
    private Common swefb612CollectionCount;
    private Common swefb612NetAmt;
    private CashReceipt p;
    private CollectionRecordGroup collectionRecord;
    private AdditionalInfoGroup additionalInfo;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of PndCollectionsAmt.
    /// </summary>
    [JsonPropertyName("pndCollectionsAmt")]
    public Common PndCollectionsAmt
    {
      get => pndCollectionsAmt ??= new();
      set => pndCollectionsAmt = value;
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
    /// A value of PndCollections.
    /// </summary>
    [JsonPropertyName("pndCollections")]
    public Common PndCollections
    {
      get => pndCollections ??= new();
      set => pndCollections = value;
    }

    /// <summary>
    /// A value of Swefb612NetAmt.
    /// </summary>
    [JsonPropertyName("swefb612NetAmt")]
    public Common Swefb612NetAmt
    {
      get => swefb612NetAmt ??= new();
      set => swefb612NetAmt = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionAmt.
    /// </summary>
    [JsonPropertyName("swefb612CollectionAmt")]
    public Common Swefb612CollectionAmt
    {
      get => swefb612CollectionAmt ??= new();
      set => swefb612CollectionAmt = value;
    }

    /// <summary>
    /// A value of Swefb612CollectionCount.
    /// </summary>
    [JsonPropertyName("swefb612CollectionCount")]
    public Common Swefb612CollectionCount
    {
      get => swefb612CollectionCount ??= new();
      set => swefb612CollectionCount = value;
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
    /// A value of ImportNet.
    /// </summary>
    [JsonPropertyName("importNet")]
    public Common ImportNet
    {
      get => importNet ??= new();
      set => importNet = value;
    }

    /// <summary>
    /// A value of ImportAdjustment.
    /// </summary>
    [JsonPropertyName("importAdjustment")]
    public Common ImportAdjustment
    {
      get => importAdjustment ??= new();
      set => importAdjustment = value;
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
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
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private Common recCollectionsAmt;
    private Common refCollectionsAmt;
    private Common relCollectionsAmt;
    private Common susCollectionsAmt;
    private Common pndCollectionsAmt;
    private Common recCollections;
    private Common refCollections;
    private Common relCollections;
    private Common susCollections;
    private Common pndCollections;
    private Common swefb612NetAmt;
    private Common swefb612CollectionAmt;
    private Common swefb612CollectionCount;
    private CashReceiptDetail importCashReceiptDetail;
    private Common importNet;
    private Common importAdjustment;
    private CashReceipt importCashReceipt;
    private Common importNumberOfReads;
    private CashReceiptDetail importNextCrdId;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Dummy5.
      /// </summary>
      [JsonPropertyName("dummy5")]
      public Common Dummy5
      {
        get => dummy5 ??= new();
        set => dummy5 = value;
      }

      /// <summary>
      /// A value of Dummy4.
      /// </summary>
      [JsonPropertyName("dummy4")]
      public Common Dummy4
      {
        get => dummy4 ??= new();
        set => dummy4 = value;
      }

      /// <summary>
      /// A value of Dummy3.
      /// </summary>
      [JsonPropertyName("dummy3")]
      public Common Dummy3
      {
        get => dummy3 ??= new();
        set => dummy3 = value;
      }

      /// <summary>
      /// A value of Dummy2.
      /// </summary>
      [JsonPropertyName("dummy2")]
      public Common Dummy2
      {
        get => dummy2 ??= new();
        set => dummy2 = value;
      }

      /// <summary>
      /// A value of Dummy1.
      /// </summary>
      [JsonPropertyName("dummy1")]
      public Common Dummy1
      {
        get => dummy1 ??= new();
        set => dummy1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 2;

      private CsePersonsWorkSet csePersonsWorkSet;
      private Common dummy5;
      private Common dummy4;
      private Common dummy3;
      private Common dummy2;
      private Common dummy1;
    }

    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedPending.
      /// </summary>
      [JsonPropertyName("hardcodedPending")]
      public CashReceiptDetailStatus HardcodedPending
      {
        get => hardcodedPending ??= new();
        set => hardcodedPending = value;
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
      /// A value of HardcodedEft.
      /// </summary>
      [JsonPropertyName("hardcodedEft")]
      public CashReceiptType HardcodedEft
      {
        get => hardcodedEft ??= new();
        set => hardcodedEft = value;
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

      private CashReceiptDetailStatus hardcodedPending;
      private CashReceiptDetailStatus hardcodedReleased;
      private CashReceiptType hardcodedEft;
      private CashReceiptDetailStatus hardcodedRecorded;
      private CashReceiptDetailStatus hardcodedSuspended;
      private CashReceiptDetailStatus hardcodedRefunded;
    }

    /// <summary>
    /// A value of SaveJointName.
    /// </summary>
    [JsonPropertyName("saveJointName")]
    public CashReceiptDetail SaveJointName
    {
      get => saveJointName ??= new();
      set => saveJointName = value;
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

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonsWorkSet Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of ValidPerson.
    /// </summary>
    [JsonPropertyName("validPerson")]
    public Common ValidPerson
    {
      get => validPerson ??= new();
      set => validPerson = value;
    }

    /// <summary>
    /// A value of ValidateState.
    /// </summary>
    [JsonPropertyName("validateState")]
    public Common ValidateState
    {
      get => validateState ??= new();
      set => validateState = value;
    }

    /// <summary>
    /// A value of Validate.
    /// </summary>
    [JsonPropertyName("validate")]
    public Fips Validate
    {
      get => validate ??= new();
      set => validate = value;
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public ProgramProcessingInfo Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of ValidCollectionType.
    /// </summary>
    [JsonPropertyName("validCollectionType")]
    public Common ValidCollectionType
    {
      get => validCollectionType ??= new();
      set => validCollectionType = value;
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
    /// A value of Adabas.
    /// </summary>
    [JsonPropertyName("adabas")]
    public AbendData Adabas
    {
      get => adabas ??= new();
      set => adabas = value;
    }

    /// <summary>
    /// A value of SsnSearch.
    /// </summary>
    [JsonPropertyName("ssnSearch")]
    public Common SsnSearch
    {
      get => ssnSearch ??= new();
      set => ssnSearch = value;
    }

    /// <summary>
    /// A value of ValidSsn.
    /// </summary>
    [JsonPropertyName("validSsn")]
    public Common ValidSsn
    {
      get => validSsn ??= new();
      set => validSsn = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// A value of OutstandingAdvancemntAmt.
    /// </summary>
    [JsonPropertyName("outstandingAdvancemntAmt")]
    public Common OutstandingAdvancemntAmt
    {
      get => outstandingAdvancemntAmt ??= new();
      set => outstandingAdvancemntAmt = value;
    }

    /// <summary>
    /// A value of CsePersonWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonWorkSet")]
    public CsePersonsWorkSet CsePersonWorkSet
    {
      get => csePersonWorkSet ??= new();
      set => csePersonWorkSet = value;
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
    /// A value of TotalRefunded.
    /// </summary>
    [JsonPropertyName("totalRefunded")]
    public Common TotalRefunded
    {
      get => totalRefunded ??= new();
      set => totalRefunded = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of FullyAppliedFlag.
    /// </summary>
    [JsonPropertyName("fullyAppliedFlag")]
    public Common FullyAppliedFlag
    {
      get => fullyAppliedFlag ??= new();
      set => fullyAppliedFlag = value;
    }

    /// <summary>
    /// A value of CsePersonFound.
    /// </summary>
    [JsonPropertyName("csePersonFound")]
    public Common CsePersonFound
    {
      get => csePersonFound ??= new();
      set => csePersonFound = value;
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
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
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

    private CashReceiptDetail saveJointName;
    private Common receiptFound;
    private CsePersonsWorkSet blank;
    private Common validPerson;
    private Common validateState;
    private Fips validate;
    private PaymentStatus hardcodedPotentialRecovery;
    private ProgramProcessingInfo date;
    private PaymentRequest paymentRequest;
    private Common validCollectionType;
    private CashReceiptDetailAddress create;
    private AbendData adabas;
    private Common ssnSearch;
    private Common validSsn;
    private Array<GroupGroup> group;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common outstandingAdvancemntAmt;
    private CsePersonsWorkSet csePersonWorkSet;
    private HardcodedViewsGroup hardcodedViews;
    private Common totalRefunded;
    private Common remainingOnCrd;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Common fullyAppliedFlag;
    private Common csePersonFound;
    private CsePerson homePhone;
    private Common createAttempts;
    private CsePerson getAddress;
    private CsePersonAddress returned;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of MaxId.
    /// </summary>
    [JsonPropertyName("maxId")]
    public CashReceiptDetail MaxId
    {
      get => maxId ??= new();
      set => maxId = value;
    }

    /// <summary>
    /// A value of AlreadyAssociated.
    /// </summary>
    [JsonPropertyName("alreadyAssociated")]
    public CashReceiptDetail AlreadyAssociated
    {
      get => alreadyAssociated ??= new();
      set => alreadyAssociated = value;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of Associated.
    /// </summary>
    [JsonPropertyName("associated")]
    public CashReceiptDetail Associated
    {
      get => associated ??= new();
      set => associated = value;
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

    private Fips fips;
    private PaymentRequest paymentRequest;
    private CaseRole caseRole;
    private Case1 case1;
    private CashReceiptDetail maxId;
    private CashReceiptDetail alreadyAssociated;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetail persistent;
    private ReceiptRefund receiptRefund;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetail associated;
    private CsePerson csePerson;
  }
#endregion
}

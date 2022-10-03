// Program: FN_PROCESS_CT_INT_COLL_DTL_REC, ID: 372565712, model: 746.
// Short name: SWE00520
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_PROCESS_CT_INT_COLL_DTL_REC.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block will load a collection record into the cash receipt
/// detail and related entity types for distribution processing.
/// </para>
/// </summary>
[Serializable]
public partial class FnProcessCtIntCollDtlRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_CT_INT_COLL_DTL_REC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessCtIntCollDtlRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessCtIntCollDtlRec.
  /// </summary>
  public FnProcessCtIntCollDtlRec(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *****************************************************************
    // RICH GALICHON - 2/99
    // Unit test and fixes.
    // *****************************************************************
    // *************************************************************************************************
    // 06/14/2004  PR 002020821 Added exit state checking to see if ADABAS was 
    // unavailable.
    // **********************************************************************************************
    // **************
    // HARDCODED AREA
    // **************
    export.Swefb610CashAmt.TotalCurrency = import.Swefb610CashAmt.TotalCurrency;
    export.Swefb610CashCount.Count = import.Swefb610CashCount.Count;
    export.Swefb610NonCashAmt.TotalCurrency =
      import.Swefb610NonCashAmt.TotalCurrency;
    export.Swefb610NonCashCount.Count = import.Swefb610NonCashCount.Count;
    UseFnHardcodedCashReceipting();

    // *****************************************************************
    // Verify that the imported Cash Receipt Detail interface
    // transaction id does not already exist for the imported
    // source code.  If it does exist, set error message and roll
    // back.    JLK  04/09/99
    // *****************************************************************
    if (ReadCashReceiptDetail2())
    {
      ExitState = "FN0000_INTF_TRAN_ID_AE_FOR_SOURC";

      return;
    }
    else
    {
      // ----> ok to continue
    }

    // *****************************************************************
    // Make sure currency has not been lost.  If it has, reread the
    // persistent views.
    // *****************************************************************
    if (!import.Pcheck.Populated)
    {
      if (!ReadCashReceipt())
      {
        ExitState = "FN0086_CASH_RCPT_NF_RB";

        return;
      }
    }

    // *****************************************************************
    // Validate collection type.  If the collection type is not valid,
    // set the detail status history reason code to Invalid Collection
    // Type.    JLK  04/09/99
    // *****************************************************************
    local.DatePassed.EffectiveDate =
      import.CollectionRecord.CollectionCashReceiptDetail.CollectionDate;
    UseFnReadCollectionTypeViaCode();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCOLTYPE";
    }

    // *****************************************************************
    // Set up the cash receipt detail in preparation for recording it.
    // *****************************************************************
    MoveCashReceiptDetail2(import.CollectionRecord.CollectionCashReceiptDetail,
      local.CashReceiptDetail);
    local.CashReceiptDetail.CollectionAmount =
      local.CashReceiptDetail.ReceivedAmount;
    local.CashReceiptDetail.Notes =
      TrimEnd(local.CashReceiptDetail.ObligorLastName) + " " + TrimEnd
      (local.CashReceiptDetail.ObligorFirstName) + " " + TrimEnd
      (local.CashReceiptDetail.ObligorMiddleName);

    // *****************************************************************
    // Populate a group view with the imported fees.
    // *****************************************************************
    local.Fees.Index = -1;

    if (import.FeesRecord.FeesInputCourt.Amount > 0)
    {
      local.CashReceiptDetail.CollectionAmount += import.FeesRecord.
        FeesInputCourt.Amount;

      ++local.Fees.Index;
      local.Fees.CheckSize();

      local.Fees.Update.Fees1.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedCourt.SystemGeneratedIdentifier;
      local.Fees.Update.Fee.Amount = import.FeesRecord.FeesInputCourt.Amount;
      local.Fees.Update.Fee.SystemGeneratedIdentifier = Now();
    }

    if (import.FeesRecord.FeesInputSrs.Amount > 0)
    {
      local.CashReceiptDetail.CollectionAmount += import.FeesRecord.
        FeesInputSrs.Amount;

      ++local.Fees.Index;
      local.Fees.CheckSize();

      local.Fees.Update.Fees1.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSrs.SystemGeneratedIdentifier;
      local.Fees.Update.Fee.Amount = import.FeesRecord.FeesInputSrs.Amount;
      local.Fees.Update.Fee.SystemGeneratedIdentifier = Now();
    }

    if (import.FeesRecord.FeesInputSendingState.Amount > 0)
    {
      local.CashReceiptDetail.CollectionAmount += import.FeesRecord.
        FeesInputSendingState.Amount;

      ++local.Fees.Index;
      local.Fees.CheckSize();

      local.Fees.Update.Fees1.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedOtherState.SystemGeneratedIdentifier;
      local.Fees.Update.Fee.Amount =
        import.FeesRecord.FeesInputSendingState.Amount;
      local.Fees.Update.Fee.SystemGeneratedIdentifier = Now();
    }

    if (import.FeesRecord.FeesInputMiscellaneous.Amount > 0)
    {
      local.CashReceiptDetail.CollectionAmount += import.FeesRecord.
        FeesInputMiscellaneous.Amount;

      ++local.Fees.Index;
      local.Fees.CheckSize();

      local.Fees.Update.Fees1.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedMiscellaneous.SystemGeneratedIdentifier;
      local.Fees.Update.Fee.Amount =
        import.FeesRecord.FeesInputMiscellaneous.Amount;
      local.Fees.Update.Fee.SystemGeneratedIdentifier = Now();
    }

    // *****************************************************************
    // Determine obligor information based on imported court
    // order.  If information cannot be determined, populate the
    // detail status history reason code which describes why the
    // detail record will be suspended after being created.
    // Reason codes corrected to reflect valid suspense codes.
    // JLK  04/09/99
    // *****************************************************************
    if (IsEmpty(local.CashReceiptDetail.CourtOrderNumber))
    {
      local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";
    }
    else
    {
      UseFnCabDetermObligorFCiBatch();

      // *******************************************************************************************************************
      // Added a check to see if ADABAS was unavailable while creating the cash
      // reciept detail.  If not abend the job.  MJQuinn June 2004  00208021
      // ******************************************************************************************************
      if (IsExitState("ADABAS_UNAVAILABLE_RB"))
      {
        return;
      }
      else if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else if (IsExitState("FN0000_CRD_OBLIGOR_UNKWN_4_CTORD"))
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVCTORDER";
      }
      else if (IsExitState("FN0000_MIS_OBLR_SSN_FOR_CT_ORD"))
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "CTORSSN";
      }
      else if (IsExitState("FN0000_MULT_PAYR_IND_REQD"))
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "MULTIPAYOR";
      }
      else if (IsExitState("FN0000_INV_MULTI_PAYOR_IND_N_CRD"))
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVMPIND";
      }
      else if (IsExitState("FN0000_MIS_OBLR_PERS_NO_F_CT_ORD"))
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
      }
      else
      {
        local.CashReceiptDetailStatHistory.ReasonCodeId = "INVPERSNBR";
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ****************************************************************
    // Validate the suspense reason code assigned to the cash receipt
    // detail status history record if a value has been assigned.
    // JLK  04/09/99
    // ****************************************************************
    if (!IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
    {
      local.ValidateCode.CodeName = "PEND/SUSP REASON";
      local.ValidateCodeValue.Cdvalue =
        local.CashReceiptDetailStatHistory.ReasonCodeId ?? Spaces(10);
      UseCabValidateCodeValue();

      if (local.ReturnCode.Count == 1 || local.ReturnCode.Count == 2)
      {
        ExitState = "CODE_VALUE_NF_RB";

        return;
      }
    }

    // ****************************************************************
    // CREATE THE CASH RECEIPT DETAIL NOW
    // ****************************************************************
    ++export.ImportNextCheckId.SequentialIdentifier;
    local.CashReceiptDetail.SequentialIdentifier =
      export.ImportNextCheckId.SequentialIdentifier;

    // ****************************************************************
    // If SSN is ALL ZERO's -- Set it to ALL SPACES.
    // ****************************************************************
    if (Equal(local.CashReceiptDetail.ObligorSocialSecurityNumber, "000000000"))
    {
      local.CashReceiptDetail.ObligorSocialSecurityNumber = "";
    }

    UseRecordCollection();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ++export.Swefb610CashCount.Count;
      export.Swefb610CashAmt.TotalCurrency += import.CollectionRecord.
        CollectionCashReceiptDetail.ReceivedAmount;

      // *****************************************************************
      // Establish currency on the Cash Receipt Detail.  This will
      // prevent additional reads subsequent action blocks.
      // *****************************************************************
      if (ReadCashReceiptDetail1())
      {
        // --->  continue
      }
      else
      {
        ExitState = "FN0053_CASH_RCPT_DTL_NF_RB";

        return;
      }

      // *************************************************************
      // SET UP THE CASH RECEIPT DETAIL ADDRESS (BY GETTING THE
      // CSE ADDRESS).
      // *************************************************************
      local.CsePerson.Number = local.CashReceiptDetail.ObligorPersonNumber ?? Spaces
        (10);
      UseSiGetCsePersonMailingAddr();

      // *************************************************************
      // MAP CSE ADDRESS TO CASH RECEIPT DETAIL ADDRESS
      // *************************************************************
      local.CashReceiptDetailAddress.City = local.CsePersonAddress.City ?? Spaces
        (30);
      local.CashReceiptDetailAddress.State = local.CsePersonAddress.State ?? Spaces
        (2);
      local.CashReceiptDetailAddress.Street1 =
        local.CsePersonAddress.Street1 ?? Spaces(25);
      local.CashReceiptDetailAddress.Street2 =
        local.CsePersonAddress.Street2 ?? "";
      local.CashReceiptDetailAddress.ZipCode3 =
        local.CashReceiptDetailAddress.ZipCode3 ?? "";
      local.CashReceiptDetailAddress.ZipCode4 = local.CsePersonAddress.Zip4 ?? ""
        ;
      local.CashReceiptDetailAddress.ZipCode5 =
        local.CsePersonAddress.ZipCode ?? Spaces(5);

      // *************************************************************
      // Now create the Cash Receipt Detail Address if the street1, city, and 
      // state attributes are populated.
      // *************************************************************
      if (!IsEmpty(local.CashReceiptDetailAddress.Street1) && !
        IsEmpty(local.CashReceiptDetailAddress.City) && !
        IsEmpty(local.CashReceiptDetailAddress.State))
      {
        UseCreateCrDetailAddress();
      }
    }
    else
    {
      return;
    }

    // ***************************************
    // CREATE THE CASH RECEIPT DETAIL FEES
    // ***************************************
    for(local.Fees.Index = 0; local.Fees.Index < Local.FeesGroup.Capacity; ++
      local.Fees.Index)
    {
      if (!local.Fees.CheckSize())
      {
        break;
      }

      if (local.Fees.Item.Fees1.SystemGeneratedIdentifier > 0)
      {
        local.TotalFees.TotalCurrency += local.Fees.Item.Fee.Amount;
        UseFnCreateCashRcptDtlFee();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
        }
        else
        {
          return;
        }
      }
    }

    local.Fees.CheckIndex();

    // *****************************************************************
    // Check to see it there is any reason to suspend the collection,
    // else release it.
    // *****************************************************************
    if (IsEmpty(local.CashReceiptDetailStatHistory.ReasonCodeId))
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;
      export.SuspendedDetail.Flag = "";
      export.ReleasedDetail.Flag = "Y";
    }
    else
    {
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      export.SuspendedDetail.Flag = "Y";
      export.ReleasedDetail.Flag = "";
    }

    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.Notes = source.Notes;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
  }

  private static void MoveCashReceiptDetail3(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.MultiPayor = source.MultiPayor;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
  }

  private static void MoveCashReceiptDetail4(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.MultiPayor = source.MultiPayor;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
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

  private static void MoveCashReceiptDetailFee(CashReceiptDetailFee source,
    CashReceiptDetailFee target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveCollectionType1(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType2(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
    target.CashNonCashInd = source.CashNonCashInd;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.ValidateCode.CodeName;
    useImport.CodeValue.Cdvalue = local.ValidateCodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Count = useExport.ReturnCode.Count;
  }

  private void UseCreateCrDetailAddress()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    MoveCashReceiptDetailAddress(local.CashReceiptDetailAddress,
      useImport.CashReceiptDetailAddress);

    Call(CreateCrDetailAddress.Execute, useImport, useExport);
  }

  private void UseFnCabDetermObligorFCiBatch()
  {
    var useImport = new FnCabDetermObligorFCiBatch.Import();
    var useExport = new FnCabDetermObligorFCiBatch.Export();

    MoveCashReceiptDetail3(local.CashReceiptDetail, useImport.CashReceiptDetail);
      

    Call(FnCabDetermObligorFCiBatch.Execute, useImport, useExport);

    MoveCashReceiptDetail4(useExport.CashReceiptDetail, local.CashReceiptDetail);
      
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.Persistent.Assign(entities.CashReceiptDetail);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      local.CashReceiptDetailStatHistory.ReasonCodeId;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.CashReceiptDetail.Assign(useImport.Persistent);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnCreateCashRcptDtlFee()
  {
    var useImport = new FnCreateCashRcptDtlFee.Import();
    var useExport = new FnCreateCashRcptDtlFee.Export();

    useImport.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
      local.Fees.Item.Fees1.SystemGeneratedIdentifier;
    MoveCashReceiptDetailFee(local.Fees.Item.Fee, useImport.CashReceiptDetailFee);
      
    useImport.PersistentCashReceiptDetail.Assign(entities.CashReceiptDetail);
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber = local.CashReceipt.SequentialNumber;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(FnCreateCashRcptDtlFee.Execute, useImport, useExport);

    entities.CashReceiptDetail.Assign(useImport.PersistentCashReceiptDetail);
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedOtherState.SystemGeneratedIdentifier =
      useExport.CrdFeeType.CrdftOtherState.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedMiscellaneous.SystemGeneratedIdentifier =
      useExport.CrdFeeType.CrdftMiscellaneous.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSrs.SystemGeneratedIdentifier =
      useExport.CrdFeeType.CrdftSrsCostRec.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedCourt.SystemGeneratedIdentifier =
      useExport.CrdFeeType.CrdftCourt.SystemGeneratedIdentifier;
  }

  private void UseFnReadCollectionTypeViaCode()
  {
    var useImport = new FnReadCollectionTypeViaCode.Import();
    var useExport = new FnReadCollectionTypeViaCode.Export();

    useImport.CollectionType.Code =
      import.CollectionRecord.CollectionCollectionType.Code;
    useImport.Date.EffectiveDate = local.DatePassed.EffectiveDate;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;

    Call(FnReadCollectionTypeViaCode.Execute, useImport, useExport);

    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    MoveCollectionType2(useExport.CollectionType, local.CollectionType);
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.PersistentCashReceipt.Assign(import.Pcheck);
    MoveCollectionType1(local.CollectionType, useImport.CollectionType);
    MoveCashReceiptDetail1(local.CashReceiptDetail, useImport.CashReceiptDetail);
      
    useImport.CashReceipt.SequentialNumber =
      export.ImportCheck.SequentialNumber;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;
    useExport.ImportNumberOfReads.Count = export.ImportNumberOfReads.Count;
    useExport.ImportNumberOfUpdates.Count = export.ImportNumberOfUpdates.Count;

    Call(RecordCollection.Execute, useImport, useExport);

    import.Pcheck.Assign(useImport.PersistentCashReceipt);
    MoveCashReceiptDetail1(useExport.CashReceiptDetail, local.CashReceiptDetail);
      
    export.ImportNumberOfReads.Count = useExport.ImportNumberOfReads.Count;
    export.ImportNumberOfUpdates.Count = useExport.ImportNumberOfUpdates.Count;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.CsePersonAddress);
  }

  private bool ReadCashReceipt()
  {
    import.Pcheck.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", export.ImportCheck.SequentialNumber);
      },
      (db, reader) =>
      {
        import.Pcheck.CrvIdentifier = db.GetInt32(reader, 0);
        import.Pcheck.CstIdentifier = db.GetInt32(reader, 1);
        import.Pcheck.CrtIdentifier = db.GetInt32(reader, 2);
        import.Pcheck.SequentialNumber = db.GetInt32(reader, 3);
        import.Pcheck.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(import.Pcheck.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crtIdentifier", import.Pcheck.CrtIdentifier);
        db.SetInt32(command, "cstIdentifier", import.Pcheck.CstIdentifier);
        db.SetInt32(command, "crvIdentifier", import.Pcheck.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    entities.ExistingIntfTranId.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "interfaceTranId",
          import.CollectionRecord.CollectionCashReceiptDetail.
            InterfaceTransId ?? "");
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingIntfTranId.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ExistingIntfTranId.CstIdentifier = db.GetInt32(reader, 1);
        entities.ExistingIntfTranId.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ExistingIntfTranId.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingIntfTranId.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.ExistingIntfTranId.Populated = true;
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
    /// <summary>A CollectionRecordGroup group.</summary>
    [Serializable]
    public class CollectionRecordGroup
    {
      /// <summary>
      /// A value of CollectionCollectionType.
      /// </summary>
      [JsonPropertyName("collectionCollectionType")]
      public CollectionType CollectionCollectionType
      {
        get => collectionCollectionType ??= new();
        set => collectionCollectionType = value;
      }

      /// <summary>
      /// A value of CollectionCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("collectionCashReceiptDetail")]
      public CashReceiptDetail CollectionCashReceiptDetail
      {
        get => collectionCashReceiptDetail ??= new();
        set => collectionCashReceiptDetail = value;
      }

      private CollectionType collectionCollectionType;
      private CashReceiptDetail collectionCashReceiptDetail;
    }

    /// <summary>A FeesRecordGroup group.</summary>
    [Serializable]
    public class FeesRecordGroup
    {
      /// <summary>
      /// A value of FeesInputCourt.
      /// </summary>
      [JsonPropertyName("feesInputCourt")]
      public CashReceiptDetailFee FeesInputCourt
      {
        get => feesInputCourt ??= new();
        set => feesInputCourt = value;
      }

      /// <summary>
      /// A value of FeesInputSendingState.
      /// </summary>
      [JsonPropertyName("feesInputSendingState")]
      public CashReceiptDetailFee FeesInputSendingState
      {
        get => feesInputSendingState ??= new();
        set => feesInputSendingState = value;
      }

      /// <summary>
      /// A value of FeesInputSrs.
      /// </summary>
      [JsonPropertyName("feesInputSrs")]
      public CashReceiptDetailFee FeesInputSrs
      {
        get => feesInputSrs ??= new();
        set => feesInputSrs = value;
      }

      /// <summary>
      /// A value of FeesInputMiscellaneous.
      /// </summary>
      [JsonPropertyName("feesInputMiscellaneous")]
      public CashReceiptDetailFee FeesInputMiscellaneous
      {
        get => feesInputMiscellaneous ??= new();
        set => feesInputMiscellaneous = value;
      }

      private CashReceiptDetailFee feesInputCourt;
      private CashReceiptDetailFee feesInputSendingState;
      private CashReceiptDetailFee feesInputSrs;
      private CashReceiptDetailFee feesInputMiscellaneous;
    }

    /// <summary>
    /// A value of Swefb610NonCashCount.
    /// </summary>
    [JsonPropertyName("swefb610NonCashCount")]
    public Common Swefb610NonCashCount
    {
      get => swefb610NonCashCount ??= new();
      set => swefb610NonCashCount = value;
    }

    /// <summary>
    /// A value of Swefb610CashCount.
    /// </summary>
    [JsonPropertyName("swefb610CashCount")]
    public Common Swefb610CashCount
    {
      get => swefb610CashCount ??= new();
      set => swefb610CashCount = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashAmt.
    /// </summary>
    [JsonPropertyName("swefb610NonCashAmt")]
    public Common Swefb610NonCashAmt
    {
      get => swefb610NonCashAmt ??= new();
      set => swefb610NonCashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610CashAmt.
    /// </summary>
    [JsonPropertyName("swefb610CashAmt")]
    public Common Swefb610CashAmt
    {
      get => swefb610CashAmt ??= new();
      set => swefb610CashAmt = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of Pcheck.
    /// </summary>
    [JsonPropertyName("pcheck")]
    public CashReceipt Pcheck
    {
      get => pcheck ??= new();
      set => pcheck = value;
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
    /// Gets a value of FeesRecord.
    /// </summary>
    [JsonPropertyName("feesRecord")]
    public FeesRecordGroup FeesRecord
    {
      get => feesRecord ?? (feesRecord = new());
      set => feesRecord = value;
    }

    private Common swefb610NonCashCount;
    private Common swefb610CashCount;
    private Common swefb610NonCashAmt;
    private Common swefb610CashAmt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceipt pcheck;
    private CollectionRecordGroup collectionRecord;
    private FeesRecordGroup feesRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Swefb610CashAmt.
    /// </summary>
    [JsonPropertyName("swefb610CashAmt")]
    public Common Swefb610CashAmt
    {
      get => swefb610CashAmt ??= new();
      set => swefb610CashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashAmt.
    /// </summary>
    [JsonPropertyName("swefb610NonCashAmt")]
    public Common Swefb610NonCashAmt
    {
      get => swefb610NonCashAmt ??= new();
      set => swefb610NonCashAmt = value;
    }

    /// <summary>
    /// A value of Swefb610CashCount.
    /// </summary>
    [JsonPropertyName("swefb610CashCount")]
    public Common Swefb610CashCount
    {
      get => swefb610CashCount ??= new();
      set => swefb610CashCount = value;
    }

    /// <summary>
    /// A value of Swefb610NonCashCount.
    /// </summary>
    [JsonPropertyName("swefb610NonCashCount")]
    public Common Swefb610NonCashCount
    {
      get => swefb610NonCashCount ??= new();
      set => swefb610NonCashCount = value;
    }

    /// <summary>
    /// A value of ImportCheck.
    /// </summary>
    [JsonPropertyName("importCheck")]
    public CashReceipt ImportCheck
    {
      get => importCheck ??= new();
      set => importCheck = value;
    }

    /// <summary>
    /// A value of ImportNextCheckId.
    /// </summary>
    [JsonPropertyName("importNextCheckId")]
    public CashReceiptDetail ImportNextCheckId
    {
      get => importNextCheckId ??= new();
      set => importNextCheckId = value;
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
    /// A value of ReleasedDetail.
    /// </summary>
    [JsonPropertyName("releasedDetail")]
    public Common ReleasedDetail
    {
      get => releasedDetail ??= new();
      set => releasedDetail = value;
    }

    /// <summary>
    /// A value of SuspendedDetail.
    /// </summary>
    [JsonPropertyName("suspendedDetail")]
    public Common SuspendedDetail
    {
      get => suspendedDetail ??= new();
      set => suspendedDetail = value;
    }

    private Common swefb610CashAmt;
    private Common swefb610NonCashAmt;
    private Common swefb610CashCount;
    private Common swefb610NonCashCount;
    private CashReceipt importCheck;
    private CashReceiptDetail importNextCheckId;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
    private Common releasedDetail;
    private Common suspendedDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A FeesGroup group.</summary>
    [Serializable]
    public class FeesGroup
    {
      /// <summary>
      /// A value of Fees1.
      /// </summary>
      [JsonPropertyName("fees1")]
      public CashReceiptDetailFeeType Fees1
      {
        get => fees1 ??= new();
        set => fees1 = value;
      }

      /// <summary>
      /// A value of Fee.
      /// </summary>
      [JsonPropertyName("fee")]
      public CashReceiptDetailFee Fee
      {
        get => fee ??= new();
        set => fee = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CashReceiptDetailFeeType fees1;
      private CashReceiptDetailFee fee;
    }

    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
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
      /// A value of HardcodedReleased.
      /// </summary>
      [JsonPropertyName("hardcodedReleased")]
      public CashReceiptDetailStatus HardcodedReleased
      {
        get => hardcodedReleased ??= new();
        set => hardcodedReleased = value;
      }

      /// <summary>
      /// A value of HardcodedOtherState.
      /// </summary>
      [JsonPropertyName("hardcodedOtherState")]
      public CashReceiptDetailFeeType HardcodedOtherState
      {
        get => hardcodedOtherState ??= new();
        set => hardcodedOtherState = value;
      }

      /// <summary>
      /// A value of HardcodedMiscellaneous.
      /// </summary>
      [JsonPropertyName("hardcodedMiscellaneous")]
      public CashReceiptDetailFeeType HardcodedMiscellaneous
      {
        get => hardcodedMiscellaneous ??= new();
        set => hardcodedMiscellaneous = value;
      }

      /// <summary>
      /// A value of HardcodedSrs.
      /// </summary>
      [JsonPropertyName("hardcodedSrs")]
      public CashReceiptDetailFeeType HardcodedSrs
      {
        get => hardcodedSrs ??= new();
        set => hardcodedSrs = value;
      }

      /// <summary>
      /// A value of HardcodedCourt.
      /// </summary>
      [JsonPropertyName("hardcodedCourt")]
      public CashReceiptDetailFeeType HardcodedCourt
      {
        get => hardcodedCourt ??= new();
        set => hardcodedCourt = value;
      }

      private CashReceiptDetailStatus hardcodedRecorded;
      private CashReceiptDetailStatus hardcodedSuspended;
      private CashReceiptDetailStatus hardcodedReleased;
      private CashReceiptDetailFeeType hardcodedOtherState;
      private CashReceiptDetailFeeType hardcodedMiscellaneous;
      private CashReceiptDetailFeeType hardcodedSrs;
      private CashReceiptDetailFeeType hardcodedCourt;
    }

    /// <summary>
    /// A value of DatePassed.
    /// </summary>
    [JsonPropertyName("datePassed")]
    public CollectionType DatePassed
    {
      get => datePassed ??= new();
      set => datePassed = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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
    /// A value of ValidateCode.
    /// </summary>
    [JsonPropertyName("validateCode")]
    public Code ValidateCode
    {
      get => validateCode ??= new();
      set => validateCode = value;
    }

    /// <summary>
    /// A value of ValidateCodeValue.
    /// </summary>
    [JsonPropertyName("validateCodeValue")]
    public CodeValue ValidateCodeValue
    {
      get => validateCodeValue ??= new();
      set => validateCodeValue = value;
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
    /// A value of TotalFees.
    /// </summary>
    [JsonPropertyName("totalFees")]
    public Common TotalFees
    {
      get => totalFees ??= new();
      set => totalFees = value;
    }

    /// <summary>
    /// Gets a value of Fees.
    /// </summary>
    [JsonIgnore]
    public Array<FeesGroup> Fees => fees ??= new(FeesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Fees for json serialization.
    /// </summary>
    [JsonPropertyName("fees")]
    [Computed]
    public IList<FeesGroup> Fees_Json
    {
      get => fees;
      set => Fees.Assign(value);
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

    private CollectionType datePassed;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private Code validateCode;
    private CodeValue validateCodeValue;
    private Common returnCode;
    private Common totalFees;
    private Array<FeesGroup> fees;
    private HardcodedViewsGroup hardcodedViews;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingIntfTranId.
    /// </summary>
    [JsonPropertyName("existingIntfTranId")]
    public CashReceiptDetail ExistingIntfTranId
    {
      get => existingIntfTranId ??= new();
      set => existingIntfTranId = value;
    }

    /// <summary>
    /// A value of KeyCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("keyCashReceiptSourceType")]
    public CashReceiptSourceType KeyCashReceiptSourceType
    {
      get => keyCashReceiptSourceType ??= new();
      set => keyCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of KeyCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("keyCashReceiptEvent")]
    public CashReceiptEvent KeyCashReceiptEvent
    {
      get => keyCashReceiptEvent ??= new();
      set => keyCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of KeyCashReceipt.
    /// </summary>
    [JsonPropertyName("keyCashReceipt")]
    public CashReceipt KeyCashReceipt
    {
      get => keyCashReceipt ??= new();
      set => keyCashReceipt = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetail existingIntfTranId;
    private CashReceiptSourceType keyCashReceiptSourceType;
    private CashReceiptEvent keyCashReceiptEvent;
    private CashReceipt keyCashReceipt;
  }
#endregion
}

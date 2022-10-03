// Program: FN_AB_DISPLAY_RECEIPT_REFUND, ID: 372307972, model: 746.
// Short name: SWE00243
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_AB_DISPLAY_RECEIPT_REFUND.
/// </summary>
[Serializable]
public partial class FnAbDisplayReceiptRefund: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_AB_DISPLAY_RECEIPT_REFUND program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAbDisplayReceiptRefund(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAbDisplayReceiptRefund.
  /// </summary>
  public FnAbDisplayReceiptRefund(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // ---------------------------------------------
    // ------------------------------------------------------------------------
    // Date	  Developer Name	Request #	Description
    // 12/14/95  Holly Kennedy-MTW			Source
    // 04/28/97  Ty Hill - MTW                         Change Current_date
    // 02/18/99  Sunya Sharp		Add logic to return the collection type.
    // 03/26/99  Sunya Sharp		Change logic to be able to display refunds that do
    // not have payment requests.  This is new due to how FDSO partial
    // adjustments are being handled.
    // 05/04/99  Sunya Sharp		Add logic to get the cash receipt detail status to
    // assist in determining the undistributed amount.
    // 06/09/99  Sunya Sharp		Screen was not displaying multiple refunds 
    // correctly.
    // 06/16/99  Sunya Sharp		Change escape after the payment status history not
    // found exit states.  Need to let processing finish.
    // 12/6/01  Kalpesh Doshi	        WR020147 - Add KPC Recoupment ind.
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();
    export.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    export.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    export.ReceiptRefund.CreatedTimestamp =
      import.ReceiptRefund.CreatedTimestamp;

    // *****
    // Read the RECEIPT REFUND.
    // *****
    ReadReceiptRefund1();

    if (entities.ReceiptRefund.Populated)
    {
      MoveReceiptRefund1(entities.ReceiptRefund, export.ReceiptRefund);

      if (!IsEmpty(entities.ReceiptRefund.LastUpdatedBy))
      {
        export.ReceiptRefund.CreatedBy =
          entities.ReceiptRefund.LastUpdatedBy ?? Spaces(8);
      }

      if (ReadCashReceiptDetailCashReceiptCashReceiptEvent())
      {
        MoveCashReceiptDetail(entities.CashReceiptDetail,
          export.CashReceiptDetail);
        export.CashReceipt.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        export.CashReceiptEvent.SystemGeneratedIdentifier =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;
        MoveCashReceiptSourceType(entities.CashReceiptSourceType,
          export.CashReceiptSourceType);
        export.CashReceiptType.Assign(entities.CashReceiptType);

        // *** Added logic to populate the collection type.  Sunya Sharp 2/18/
        // 1999 ***
        if (ReadCollectionType())
        {
          export.CollectionType.Code = entities.CollectionType.Code;
        }

        // *** Add logic to get the status of the detail to help determine the 
        // undistributed amount.  Sunya Sharp 5/4/1999 ***
        if (ReadCashReceiptDetailStatus())
        {
          export.CashReceiptDetailStatus.Code =
            entities.CashReceiptDetailStatus.Code;
        }

        // *****
        // Determine if the selected Receipt Refund has an address associated to
        // it and display it
        // *****
        if (ReadCashReceiptDetailAddress1())
        {
          export.CashReceiptDetailAddress.Assign(
            entities.CashReceiptDetailAddress);
        }
        else
        {
          // *****
          // If an address does not exist for the Receipt Refund, display the 
          // address for the Cash Receipt Detail
          // *****
          if (ReadCashReceiptDetailAddress2())
          {
            export.CashReceiptDetailAddress.Assign(
              entities.CashReceiptDetailAddress);
          }
        }

        // *****
        // Determine if the selected Receipt Refund has been dispersed.  If it 
        // has display the warrant number if it exists.
        // *****
        if (ReadPaymentRequest())
        {
          export.PaymentRequest.Assign(entities.PaymentRequest);

          if (ReadPaymentStatusPaymentStatusHistory())
          {
            export.PaymentStatus.Code = entities.PaymentStatus.Code;
          }
          else
          {
            ExitState = "FN0000_PYMNT_STAT_NF_RB";

            goto Test1;
          }
        }
        else
        {
          // *** This is ok.  If a refund was created to act like a partial 
          // adjustment, it will not have a payment request associated with it.
          // The money for the partial adjustment is refunded by the Feds.
          // Sunya Sharp 3/26/1999 ***
        }

        if (Lt(Date(entities.ReceiptRefund.CreatedTimestamp), local.Current.Date))
          
        {
          ExitState = "FN0000_PROTECT_RECEIPT_REFUND";
        }
      }
      else
      {
        ExitState = "FN0000_CASH_RECEIPT_DETAIL_NF";

        return;
      }
    }
    else
    {
      // *****
      // Read the CASH RECEIPT DETAIL.
      // *****
      if (!ReadCashReceiptDetailCashReceiptCashReceiptSourceType())
      {
        ExitState = "FN0000_CASH_RECEIPT_DETAIL_NF";

        return;
      }

      MoveCashReceiptDetail(entities.CashReceiptDetail, export.CashReceiptDetail);
        

      // --- Read adjustments if any for the cash receipt detail ---
      if (!ReadCashReceiptDetailCashReceiptDetailBalanceAdj())
      {
        // -- continue --
      }

      MoveCashReceiptSourceType(entities.CashReceiptSourceType,
        export.CashReceiptSourceType);
      export.CashReceipt.SequentialNumber =
        entities.CashReceipt.SequentialNumber;
      export.CashReceiptType.Assign(entities.CashReceiptType);

      if (ReadCollectionType())
      {
        export.CollectionType.Code = entities.CollectionType.Code;
      }

      // *** Add logic to get the status of the detail to help determine the 
      // undistributed amount.  Sunya Sharp 5/4/1999 ***
      if (ReadCashReceiptDetailStatus())
      {
        export.CashReceiptDetailStatus.Code =
          entities.CashReceiptDetailStatus.Code;
      }

      local.ReadCount.Count = 0;

      foreach(var item in ReadReceiptRefund3())
      {
        ++local.ReadCount.Count;
        MoveReceiptRefund2(entities.ReceiptRefund, local.Null1);
      }

      // *** Add logic to get the warrant information if there is a refund for 
      // the detail that was requested.  Added logic to display the correct
      // address for the refund is there is one.  Sunya Sharp 2/18/1999 ***
      if (ReadReceiptRefund2())
      {
        MoveReceiptRefund1(entities.ReceiptRefund, export.ReceiptRefund);

        if (!IsEmpty(entities.ReceiptRefund.LastUpdatedBy))
        {
          export.ReceiptRefund.CreatedBy =
            entities.ReceiptRefund.LastUpdatedBy ?? Spaces(8);
        }

        if (ReadCashReceiptDetailAddress1())
        {
          export.CashReceiptDetailAddress.Assign(
            entities.CashReceiptDetailAddress);
        }

        if (ReadPaymentRequest())
        {
          export.PaymentRequest.Assign(entities.PaymentRequest);

          if (ReadPaymentStatusPaymentStatusHistory())
          {
            export.PaymentStatus.Code = entities.PaymentStatus.Code;
          }
          else
          {
            ExitState = "FN0000_PYMNT_STAT_NF_RB";
          }
        }
        else
        {
          // *** This is ok.  If a refund was created to act like a partial 
          // adjustment, it will not have a payment request associated with it.
          // The money for the partial adjustment is refunded by the Feds.
          // Sunya Sharp 3/26/1999 ***
        }
      }
      else
      {
        // *****
        // If an address does not exist for the Receipt Refund, display the 
        // address for the Cash Receipt Detail
        // *****
        if (ReadCashReceiptDetailAddress2())
        {
          export.CashReceiptDetailAddress.Assign(
            entities.CashReceiptDetailAddress);
        }
      }
    }

Test1:

    UseFnAbConcatCrAndCrd();

    if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
    {
      UseFnAbObligorListForCtOrder();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test2;
      }

      if (local.WorkNoOfObligors.Count == 1)
      {
        for(local.ObligorList.Index = 0; local.ObligorList.Index < local
          .ObligorList.Count; ++local.ObligorList.Index)
        {
          MoveCsePersonsWorkSet(local.ObligorList.Item.GrpsWork,
            export.CashReceivedFromCsePersonsWorkSet);

          goto Test2;
        }
      }

      if (AsChar(entities.CashReceiptDetail.MultiPayor) == 'M')
      {
        local.WorkSex.Sex = "F";
      }
      else
      {
        local.WorkSex.Sex = "M";
      }

      for(local.ObligorList.Index = 0; local.ObligorList.Index < local
        .ObligorList.Count; ++local.ObligorList.Index)
      {
        if (AsChar(local.ObligorList.Item.GrpsWork.Sex) == AsChar
          (local.WorkSex.Sex))
        {
          MoveCsePersonsWorkSet(local.ObligorList.Item.GrpsWork,
            export.CashReceivedFromCsePersonsWorkSet);

          goto Test2;
        }
      }
    }
    else if (!IsEmpty(entities.CashReceiptDetail.ObligorPersonNumber))
    {
      export.CashReceivedFromCsePersonsWorkSet.Number =
        entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
      UseSiReadCsePerson();
    }
    else if (!IsEmpty(entities.CashReceiptDetail.ObligorSocialSecurityNumber))
    {
      local.WorkSex.Ssn =
        entities.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces(9);
      UseFnReadCsePersonUsingSsnO();
    }

Test2:

    if (entities.ReceiptRefund.Populated)
    {
      if (ReadCsePerson())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
      }
      else if (ReadCashReceiptSourceType())
      {
        export.RefundTo.Code = entities.RefundTo.Code;
      }
    }

    if (local.ReadCount.Count > 1)
    {
      ExitState = "FN0000_MULT_REFUNDS_EXIST_DETAIL";
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.JointReturnName = source.JointReturnName;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveObligorList(FnAbObligorListForCtOrder.Export.
    ObligorListGroup source, Local.ObligorListGroup target)
  {
    target.Grps.Assign(source.Grps);
    target.GrpsWork.Assign(source.GrpsWork);
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
    target.RequestDate = source.RequestDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnAbObligorListForCtOrder()
  {
    var useImport = new FnAbObligorListForCtOrder.Import();
    var useExport = new FnAbObligorListForCtOrder.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CashReceiptDetail.CourtOrderNumber;

    Call(FnAbObligorListForCtOrder.Execute, useImport, useExport);

    useExport.ObligorList.CopyTo(local.ObligorList, MoveObligorList);
    local.WorkNoOfObligors.Count = useExport.WorkNoOfObligors.Count;
  }

  private void UseFnReadCsePersonUsingSsnO()
  {
    var useImport = new FnReadCsePersonUsingSsnO.Import();
    var useExport = new FnReadCsePersonUsingSsnO.Export();

    useImport.CsePersonsWorkSet.Ssn = local.WorkSex.Ssn;
    MoveCsePersonsWorkSet(export.CashReceivedFromCsePersonsWorkSet,
      useExport.CsePersonsWorkSet);

    Call(FnReadCsePersonUsingSsnO.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.CashReceivedFromCsePersonsWorkSet);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.CashReceivedFromCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.CashReceivedFromCsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetailAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "crdetailAddressI",
          entities.ReceiptRefund.CdaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptEvent",
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
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 13);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 15);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 17);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 19);
        entities.CashReceiptType.Code = db.GetString(reader, 20);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 21);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptSourceType()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 13);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 15);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 17);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 18);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 19);
        entities.CashReceiptType.Code = db.GetString(reader, 20);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 21);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptDetailCashReceiptDetailBalanceAdj()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.AdjustmentCashReceiptDetailBalanceAdj.Populated = false;
    entities.AdjustmentCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptDetailBalanceAdj",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustmentCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustmentCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustmentCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustmentCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustmentCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 4);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 5);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 6);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 7);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 8);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 9);
        entities.AdjustmentCashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.AdjustmentCashReceiptDetailBalanceAdj.Populated = true;
        entities.AdjustmentCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.MaxDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.RefundTo.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          entities.ReceiptRefund.CstAIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.RefundTo.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.RefundTo.Code = db.GetString(reader, 1);
        entities.RefundTo.Populated = true;
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
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ReceiptRefund.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
        entities.PaymentRequest.Number = db.GetNullableString(reader, 1);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 2);
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

  private bool ReadPaymentStatusPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadReceiptRefund1()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
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
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadReceiptRefund2()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Null1.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
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
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
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
        entities.ReceiptRefund.CdaIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.ReceiptRefund.ReasonText = db.GetNullableString(reader, 13);
        entities.ReceiptRefund.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 15);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 16);
        entities.ReceiptRefund.Populated = true;

        return true;
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

    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetail cashReceiptDetail;
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
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of CashReceivedFromCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("cashReceivedFromCsePersonsWorkSet")]
    public CsePersonsWorkSet CashReceivedFromCsePersonsWorkSet
    {
      get => cashReceivedFromCsePersonsWorkSet ??= new();
      set => cashReceivedFromCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CashReceivedFromCsePerson.
    /// </summary>
    [JsonPropertyName("cashReceivedFromCsePerson")]
    public CsePerson CashReceivedFromCsePerson
    {
      get => cashReceivedFromCsePerson ??= new();
      set => cashReceivedFromCsePerson = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CrdCrComboNo crdCrComboNo;
    private CsePersonsWorkSet cashReceivedFromCsePersonsWorkSet;
    private CsePerson cashReceivedFromCsePerson;
    private CashReceiptSourceType refundTo;
    private PaymentStatus paymentStatus;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptSourceType cashReceiptSourceType;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ObligorListGroup group.</summary>
    [Serializable]
    public class ObligorListGroup
    {
      /// <summary>
      /// A value of Grps.
      /// </summary>
      [JsonPropertyName("grps")]
      public CsePerson Grps
      {
        get => grps ??= new();
        set => grps = value;
      }

      /// <summary>
      /// A value of GrpsWork.
      /// </summary>
      [JsonPropertyName("grpsWork")]
      public CsePersonsWorkSet GrpsWork
      {
        get => grpsWork ??= new();
        set => grpsWork = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private CsePerson grps;
      private CsePersonsWorkSet grpsWork;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of WorkSex.
    /// </summary>
    [JsonPropertyName("workSex")]
    public CsePersonsWorkSet WorkSex
    {
      get => workSex ??= new();
      set => workSex = value;
    }

    /// <summary>
    /// Gets a value of ObligorList.
    /// </summary>
    [JsonIgnore]
    public Array<ObligorListGroup> ObligorList => obligorList ??= new(
      ObligorListGroup.Capacity);

    /// <summary>
    /// Gets a value of ObligorList for json serialization.
    /// </summary>
    [JsonPropertyName("obligorList")]
    [Computed]
    public IList<ObligorListGroup> ObligorList_Json
    {
      get => obligorList;
      set => ObligorList.Assign(value);
    }

    /// <summary>
    /// A value of WorkNoOfObligors.
    /// </summary>
    [JsonPropertyName("workNoOfObligors")]
    public Common WorkNoOfObligors
    {
      get => workNoOfObligors ??= new();
      set => workNoOfObligors = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public ReceiptRefund Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
    }

    private DateWorkArea maxDate;
    private CsePersonsWorkSet workSex;
    private Array<ObligorListGroup> obligorList;
    private Common workNoOfObligors;
    private DateWorkArea current;
    private ReceiptRefund null1;
    private Common readCount;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CollectedFrom.
    /// </summary>
    [JsonPropertyName("collectedFrom")]
    public CsePerson CollectedFrom
    {
      get => collectedFrom ??= new();
      set => collectedFrom = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj AdjustmentCashReceiptDetailBalanceAdj
    {
      get => adjustmentCashReceiptDetailBalanceAdj ??= new();
      set => adjustmentCashReceiptDetailBalanceAdj = value;
    }

    /// <summary>
    /// A value of AdjustmentCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustmentCashReceiptDetail")]
    public CashReceiptDetail AdjustmentCashReceiptDetail
    {
      get => adjustmentCashReceiptDetail ??= new();
      set => adjustmentCashReceiptDetail = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CsePerson collectedFrom;
    private CashReceiptDetailBalanceAdj adjustmentCashReceiptDetailBalanceAdj;
    private CashReceiptDetail adjustmentCashReceiptDetail;
    private CashReceiptSourceType refundTo;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private CollectionType collectionType;
    private CsePerson csePerson;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}

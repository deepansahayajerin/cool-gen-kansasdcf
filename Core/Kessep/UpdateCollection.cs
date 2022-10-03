// Program: UPDATE_COLLECTION, ID: 371770023, model: 746.
// Short name: SWE01467
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_COLLECTION.
/// </summary>
[Serializable]
public partial class UpdateCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateCollection.
  /// </summary>
  public UpdateCollection(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // 04/29/97    JeHoward          Current date fix.
    // 02/24/00    Sunya Sharp      Add new view for collection fully applied 
    // indiciator. PR# 85889		
    // ---------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnHardcodedCashReceipting();

    if (ReadCashReceiptDetail1())
    {
      MoveCashReceiptDetail1(entities.CashReceiptDetail, local.CashReceiptDetail);
        
      MoveCashReceiptDetail2(entities.CashReceiptDetail, local.Amount);

      if (ReadCashReceiptDetailStatus())
      {
        if (AsChar(import.Adjust.Flag) == 'Y')
        {
          // *** CAB is being used to adjust a CRD as well.  The status for this
          // function is checked in the procedure.  Sunya Sharp 2/24/2000 ***
        }
        else if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedRefunded.SystemGeneratedIdentifier || entities
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedDistributed.SystemGeneratedIdentifier || entities
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
          .HardcodedAdjusted.SystemGeneratedIdentifier)
        {
          ExitState = "FN0051_CASH_RCPT_DTL_INVALD_STAT";

          return;
        }
      }
      else
      {
        ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";

        return;
      }

      if (ReadCollectionType1())
      {
        local.CollectionType.SequentialIdentifier =
          entities.Current.SequentialIdentifier;

        if (import.CollectionType.SequentialIdentifier == entities
          .Current.SequentialIdentifier)
        {
          // Collection type is the same so no action is required.
        }
        else if (ReadCollectionType2())
        {
          TransferCashReceiptDetail();
        }
        else
        {
          ExitState = "NEW_COLLECTION_TYPE_NF";

          return;
        }
      }
      else if (ReadCollectionType2())
      {
        AssociateCollectionType();
      }
      else
      {
        ExitState = "NEW_COLLECTION_TYPE_NF";

        return;
      }

      if (AsChar(import.Adjust.Flag) == 'Y')
      {
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
      }
      else if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedSuspended.SystemGeneratedIdentifier)
      {
        // -------------------------------------------------
        // The Received Amount, Collection Amount and Collection Date cannot be 
        // updated because it is in suspended status and all/part of the money
        // may have got distributed
        // -------------------------------------------------
        try
        {
          UpdateCashReceiptDetail2();
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
      }
      else
      {
        try
        {
          UpdateCashReceiptDetail3();
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
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    // Set all history attributes to the current cash receipt detail attributes.
    local.CashReceiptDetailHistory.Attribute1SupportedPersonFirstName =
      local.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
    local.CashReceiptDetailHistory.Attribute1SupportedPersonLastName =
      local.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
    local.CashReceiptDetailHistory.Attribute1SupportedPersonMiddleName =
      local.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
    local.CashReceiptDetailHistory.Attribute2SupportedPersonFirstName =
      local.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
    local.CashReceiptDetailHistory.Attribute2SupportedPersonLastName =
      local.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
    local.CashReceiptDetailHistory.Attribute2SupportedPersonMiddleName =
      local.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
    local.CashReceiptDetailHistory.AdjustmentInd =
      local.CashReceiptDetail.AdjustmentInd ?? "";
    local.CashReceiptDetailHistory.CaseNumber =
      local.CashReceiptDetail.CaseNumber ?? "";
    local.CashReceiptDetailHistory.CashReceiptEventNumber =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptSourceType =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptType =
      import.CashReceiptType.SystemGeneratedIdentifier;
    local.CashReceiptDetailHistory.CashReceiptNumber =
      import.CashReceipt.SequentialNumber;
    local.CashReceiptDetailHistory.CollectionAmtFullyAppliedInd =
      local.CashReceiptDetail.CollectionAmtFullyAppliedInd ?? "";
    local.CashReceiptDetailHistory.CollectionAmount =
      local.CashReceiptDetail.CollectionAmount;
    local.CashReceiptDetailHistory.CollectionDate =
      local.CashReceiptDetail.CollectionDate;
    local.CashReceiptDetailHistory.CollectionTypeIdentifier =
      local.CollectionType.SequentialIdentifier;
    local.CashReceiptDetailHistory.CourtOrderNumber =
      local.CashReceiptDetail.CourtOrderNumber ?? "";
    local.CashReceiptDetailHistory.CreatedBy =
      local.CashReceiptDetail.CreatedBy;
    local.CashReceiptDetailHistory.CreatedTmst =
      local.CashReceiptDetail.CreatedTmst;
    local.CashReceiptDetailHistory.DefaultedCollectionDateInd =
      local.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
    local.CashReceiptDetailHistory.DistributedAmount =
      local.CashReceiptDetail.DistributedAmount.GetValueOrDefault();
    local.CashReceiptDetailHistory.InterfaceTransId =
      local.CashReceiptDetail.InterfaceTransId ?? "";
    local.CashReceiptDetailHistory.JointReturnInd =
      local.CashReceiptDetail.JointReturnInd ?? "";
    local.CashReceiptDetailHistory.JointReturnName =
      local.CashReceiptDetail.JointReturnName ?? "";

    if (IsEmpty(local.CashReceiptDetail.LastUpdatedBy))
    {
      local.CashReceiptDetailHistory.LastUpdatedBy =
        local.CashReceiptDetail.CreatedBy;
      local.CashReceiptDetailHistory.LastUpdatedTmst =
        local.CashReceiptDetail.CreatedTmst;
    }
    else
    {
      local.CashReceiptDetailHistory.LastUpdatedBy =
        local.CashReceiptDetail.LastUpdatedBy ?? Spaces(8);
      local.CashReceiptDetailHistory.LastUpdatedTmst =
        local.CashReceiptDetail.LastUpdatedTmst;
    }

    local.CashReceiptDetailHistory.MultiPayor =
      local.CashReceiptDetail.MultiPayor ?? "";
    local.CashReceiptDetailHistory.Notes = local.CashReceiptDetail.Notes ?? "";
    local.CashReceiptDetailHistory.ObligorFirstName =
      local.CashReceiptDetail.ObligorFirstName ?? "";
    local.CashReceiptDetailHistory.ObligorLastName =
      local.CashReceiptDetail.ObligorLastName ?? "";
    local.CashReceiptDetailHistory.ObligorMiddleName =
      local.CashReceiptDetail.ObligorMiddleName ?? "";
    local.CashReceiptDetailHistory.ObligorPersonNumber =
      local.CashReceiptDetail.ObligorPersonNumber ?? "";
    local.CashReceiptDetailHistory.ObligorPhoneNumber =
      local.CashReceiptDetail.ObligorPhoneNumber ?? "";
    local.CashReceiptDetailHistory.ObligorSocialSecurityNumber =
      local.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    local.CashReceiptDetailHistory.OffsetTaxYear =
      local.CashReceiptDetail.OffsetTaxYear.GetValueOrDefault();
    local.CashReceiptDetailHistory.OffsetTaxid =
      local.CashReceiptDetail.OffsetTaxid.GetValueOrDefault();
    local.CashReceiptDetailHistory.PayeeFirstName =
      local.CashReceiptDetail.PayeeFirstName ?? "";
    local.CashReceiptDetailHistory.PayeeLastName =
      local.CashReceiptDetail.PayeeLastName ?? "";
    local.CashReceiptDetailHistory.PayeeMiddleName =
      local.CashReceiptDetail.PayeeMiddleName ?? "";
    local.CashReceiptDetailHistory.ReceivedAmount =
      local.CashReceiptDetail.ReceivedAmount;
    local.CashReceiptDetailHistory.Reference =
      local.CashReceiptDetail.Reference ?? "";
    local.CashReceiptDetailHistory.RefundedAmount =
      local.CashReceiptDetail.RefundedAmount.GetValueOrDefault();
    local.CashReceiptDetailHistory.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    UseFnLogCashRcptDtlHistory();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // This is o.k., want to continue.
    }
    else
    {
      return;
    }

    if (AsChar(import.ToBeUnpended.Flag) == 'Y')
    {
      local.CashReceiptDetailStatHistory.ReasonText =
        Spaces(CashReceiptDetailStatHistory.ReasonText_MaxLength);
      local.CashReceiptDetailStatHistory.ReasonCodeId = "";
      UseFnChangeCashRcptDtlStatHis2();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "UNPEND_SUCCESSFUL";
      }
      else
      {
        return;
      }
    }

    if (local.Amount.ReceivedAmount == import
      .CashReceiptDetail.ReceivedAmount || local.Amount.CollectionAmount == import
      .CashReceiptDetail.CollectionAmount)
    {
      // --------------------------------------------------
      // The collection and received amounts have not changed since the last 
      // value in the table and so we need not bring the released collections
      // back to recorded status
      // --------------------------------------------------
      return;
    }

    // ---- Once successfully updated, update the status of crdetails that are 
    // related to this cash receipt from 'REL' to 'REC'
    if (!ReadCashReceipt())
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // *** Changed read each to only go against the cash receipt detail table.  
    // Also set a flag for the exitstate.  It was setting after first successful
    // change and only the first 2 records were getting changed to recorded
    // status.  Sunya Sharp 11/2/98 ***
    foreach(var item in ReadCashReceiptDetail2())
    {
      UseFnChangeCashRcptDtlStatHis1();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ReleaseToRecorded.Flag = "Y";
      }
      else
      {
        return;
      }
    }

    if (AsChar(local.ReleaseToRecorded.Flag) == 'Y')
    {
      ExitState = "FN0000_UPDATE_SUCCESSFUL_WA";
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmtFullyAppliedInd = source.CollectionAmtFullyAppliedInd;
    target.InterfaceTransId = source.InterfaceTransId;
    target.AdjustmentInd = source.AdjustmentInd;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
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
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Attribute1SupportedPersonFirstName =
      source.Attribute1SupportedPersonFirstName;
    target.Attribute1SupportedPersonMiddleName =
      source.Attribute1SupportedPersonMiddleName;
    target.Attribute1SupportedPersonLastName =
      source.Attribute1SupportedPersonLastName;
    target.Attribute2SupportedPersonFirstName =
      source.Attribute2SupportedPersonFirstName;
    target.Attribute2SupportedPersonLastName =
      source.Attribute2SupportedPersonLastName;
    target.Attribute2SupportedPersonMiddleName =
      source.Attribute2SupportedPersonMiddleName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.RefundedAmount = source.RefundedAmount;
    target.DistributedAmount = source.DistributedAmount;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
  }

  private static void MoveCashReceiptDetailStatHistory(
    CashReceiptDetailStatHistory source, CashReceiptDetailStatHistory target)
  {
    target.ReasonCodeId = source.ReasonCodeId;
    target.ReasonText = source.ReasonText;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnChangeCashRcptDtlStatHis1()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.KeyCashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedRecorded.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnChangeCashRcptDtlStatHis2()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    MoveCashReceiptDetailStatHistory(local.CashReceiptDetailStatHistory,
      useImport.New1);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedRecorded.SystemGeneratedIdentifier;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedDistributed.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdDistributed.SystemGeneratedIdentifier;
    local.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
  }

  private void UseFnLogCashRcptDtlHistory()
  {
    var useImport = new FnLogCashRcptDtlHistory.Import();
    var useExport = new FnLogCashRcptDtlHistory.Export();

    useImport.CashReceiptDetailHistory.Assign(local.CashReceiptDetailHistory);

    Call(FnLogCashRcptDtlHistory.Execute, useImport, useExport);
  }

  private void AssociateCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var cltIdentifier = entities.New1.SequentialIdentifier;

    entities.CashReceiptDetail.Populated = false;
    Update("AssociateCollectionType",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private bool ReadCashReceipt()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail1()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.OffsetTaxid = db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 9);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 11);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 14);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 15);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 16);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 17);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 18);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 19);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 22);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 24);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 26);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 27);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 28);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 29);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 30);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 31);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 32);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 33);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 35);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 37);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 38);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 39);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.KeyCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
        db.SetInt32(
          command, "cdsIdentifier",
          local.HardcodedReleased.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.KeyCashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.KeyCashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.KeyCashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.KeyCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.KeyCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
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
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCollectionType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Current.Populated = false;

    return Read("ReadCollectionType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Current.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.Current.Populated = true;
      });
  }

  private bool ReadCollectionType2()
  {
    entities.New1.Populated = false;

    return Read("ReadCollectionType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          import.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.New1.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.New1.Populated = true;
      });
  }

  private void TransferCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var cltIdentifier = entities.New1.SequentialIdentifier;

    entities.CashReceiptDetail.Populated = false;
    Update("TransferCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.CltIdentifier = cltIdentifier;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var refundedAmount = 0M;
    var collectionAmtFullyAppliedInd = "Y";
    var notes = import.CashReceiptDetail.Notes ?? "";

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
        db.SetNullableString(command, "notes", notes);
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
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var interfaceTransId = import.CashReceiptDetail.InterfaceTransId ?? "";
    var courtOrderNumber = import.CashReceiptDetail.CourtOrderNumber ?? "";
    var caseNumber = import.CashReceiptDetail.CaseNumber ?? "";
    var multiPayor = import.CashReceiptDetail.MultiPayor ?? "";
    var defaultedCollectionDateInd =
      import.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
    var obligorPersonNumber = import.CashReceiptDetail.ObligorPersonNumber ?? ""
      ;
    var obligorSocialSecurityNumber =
      import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    var obligorFirstName = import.CashReceiptDetail.ObligorFirstName ?? "";
    var obligorLastName = import.CashReceiptDetail.ObligorLastName ?? "";
    var obligorMiddleName = import.CashReceiptDetail.ObligorMiddleName ?? "";
    var obligorPhoneNumber = import.CashReceiptDetail.ObligorPhoneNumber ?? "";
    var payeeFirstName = import.CashReceiptDetail.PayeeFirstName ?? "";
    var payeeMiddleName = import.CashReceiptDetail.PayeeMiddleName ?? "";
    var payeeLastName = import.CashReceiptDetail.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
    var attribute1SupportedPersonLastName =
      import.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
    var attribute2SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      import.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.CashReceiptDetail.LastUpdatedTmst;
    var reference = import.CashReceiptDetail.Reference ?? "";
    var notes = import.CashReceiptDetail.Notes ?? "";

    CheckValid<CashReceiptDetail>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail2",
      (db, command) =>
      {
        db.SetNullableString(command, "interfaceTranId", interfaceTransId);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableString(
          command, "dfltdCollDatInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnLstNm2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnMidNm2", attribute2SupportedPersonMiddleName);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.CaseNumber = caseNumber;
    entities.CashReceiptDetail.MultiPayor = multiPayor;
    entities.CashReceiptDetail.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CashReceiptDetail.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetail.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetail.PayeeLastName = payeeLastName;
    entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Reference = reference;
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.Populated = true;
  }

  private void UpdateCashReceiptDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var interfaceTransId = import.CashReceiptDetail.InterfaceTransId ?? "";
    var courtOrderNumber = import.CashReceiptDetail.CourtOrderNumber ?? "";
    var caseNumber = import.CashReceiptDetail.CaseNumber ?? "";
    var receivedAmount = import.CashReceiptDetail.ReceivedAmount;
    var collectionAmount = import.CashReceiptDetail.CollectionAmount;
    var collectionDate = import.CashReceiptDetail.CollectionDate;
    var multiPayor = import.CashReceiptDetail.MultiPayor ?? "";
    var defaultedCollectionDateInd =
      import.CashReceiptDetail.DefaultedCollectionDateInd ?? "";
    var obligorPersonNumber = import.CashReceiptDetail.ObligorPersonNumber ?? ""
      ;
    var obligorSocialSecurityNumber =
      import.CashReceiptDetail.ObligorSocialSecurityNumber ?? "";
    var obligorFirstName = import.CashReceiptDetail.ObligorFirstName ?? "";
    var obligorLastName = import.CashReceiptDetail.ObligorLastName ?? "";
    var obligorMiddleName = import.CashReceiptDetail.ObligorMiddleName ?? "";
    var obligorPhoneNumber = import.CashReceiptDetail.ObligorPhoneNumber ?? "";
    var payeeFirstName = import.CashReceiptDetail.PayeeFirstName ?? "";
    var payeeMiddleName = import.CashReceiptDetail.PayeeMiddleName ?? "";
    var payeeLastName = import.CashReceiptDetail.PayeeLastName ?? "";
    var attribute1SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute1SupportedPersonFirstName ?? "";
    var attribute1SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute1SupportedPersonMiddleName ?? "";
    var attribute1SupportedPersonLastName =
      import.CashReceiptDetail.Attribute1SupportedPersonLastName ?? "";
    var attribute2SupportedPersonFirstName =
      import.CashReceiptDetail.Attribute2SupportedPersonFirstName ?? "";
    var attribute2SupportedPersonLastName =
      import.CashReceiptDetail.Attribute2SupportedPersonLastName ?? "";
    var attribute2SupportedPersonMiddleName =
      import.CashReceiptDetail.Attribute2SupportedPersonMiddleName ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.CashReceiptDetail.LastUpdatedTmst;
    var reference = import.CashReceiptDetail.Reference ?? "";
    var notes = import.CashReceiptDetail.Notes ?? "";

    CheckValid<CashReceiptDetail>("MultiPayor", multiPayor);
    CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
      defaultedCollectionDateInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail3",
      (db, command) =>
      {
        db.SetNullableString(command, "interfaceTranId", interfaceTransId);
        db.SetNullableString(command, "courtOrderNumber", courtOrderNumber);
        db.SetNullableString(command, "caseNumber", caseNumber);
        db.SetDecimal(command, "receivedAmount", receivedAmount);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetNullableString(command, "multiPayor", multiPayor);
        db.SetNullableString(
          command, "dfltdCollDatInd", defaultedCollectionDateInd);
        db.SetNullableString(command, "oblgorPrsnNbr", obligorPersonNumber);
        db.SetNullableString(command, "oblgorSsn", obligorSocialSecurityNumber);
        db.SetNullableString(command, "oblgorFirstNm", obligorFirstName);
        db.SetNullableString(command, "oblgorLastNm", obligorLastName);
        db.SetNullableString(command, "oblgorMidNm", obligorMiddleName);
        db.SetNullableString(command, "oblgorPhoneNbr", obligorPhoneNumber);
        db.SetNullableString(command, "payeeFirstName", payeeFirstName);
        db.SetNullableString(command, "payeeMiddleName", payeeMiddleName);
        db.SetNullableString(command, "payeeLastName", payeeLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm1", attribute1SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnMidNm1", attribute1SupportedPersonMiddleName);
        db.SetNullableString(
          command, "supPrsnLstNm1", attribute1SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnFrstNm2", attribute2SupportedPersonFirstName);
        db.SetNullableString(
          command, "supPrsnLstNm2", attribute2SupportedPersonLastName);
        db.SetNullableString(
          command, "supPrsnMidNm2", attribute2SupportedPersonMiddleName);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "referenc", reference);
        db.SetNullableString(command, "notes", notes);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.InterfaceTransId = interfaceTransId;
    entities.CashReceiptDetail.CourtOrderNumber = courtOrderNumber;
    entities.CashReceiptDetail.CaseNumber = caseNumber;
    entities.CashReceiptDetail.ReceivedAmount = receivedAmount;
    entities.CashReceiptDetail.CollectionAmount = collectionAmount;
    entities.CashReceiptDetail.CollectionDate = collectionDate;
    entities.CashReceiptDetail.MultiPayor = multiPayor;
    entities.CashReceiptDetail.DefaultedCollectionDateInd =
      defaultedCollectionDateInd;
    entities.CashReceiptDetail.ObligorPersonNumber = obligorPersonNumber;
    entities.CashReceiptDetail.ObligorSocialSecurityNumber =
      obligorSocialSecurityNumber;
    entities.CashReceiptDetail.ObligorFirstName = obligorFirstName;
    entities.CashReceiptDetail.ObligorLastName = obligorLastName;
    entities.CashReceiptDetail.ObligorMiddleName = obligorMiddleName;
    entities.CashReceiptDetail.ObligorPhoneNumber = obligorPhoneNumber;
    entities.CashReceiptDetail.PayeeFirstName = payeeFirstName;
    entities.CashReceiptDetail.PayeeMiddleName = payeeMiddleName;
    entities.CashReceiptDetail.PayeeLastName = payeeLastName;
    entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
      attribute1SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
      attribute1SupportedPersonMiddleName;
    entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
      attribute1SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
      attribute2SupportedPersonFirstName;
    entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
      attribute2SupportedPersonLastName;
    entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
      attribute2SupportedPersonMiddleName;
    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Reference = reference;
    entities.CashReceiptDetail.Notes = notes;
    entities.CashReceiptDetail.Populated = true;
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
    /// A value of Adjust.
    /// </summary>
    [JsonPropertyName("adjust")]
    public Common Adjust
    {
      get => adjust ??= new();
      set => adjust = value;
    }

    /// <summary>
    /// A value of ToBeUnpended.
    /// </summary>
    [JsonPropertyName("toBeUnpended")]
    public Common ToBeUnpended
    {
      get => toBeUnpended ??= new();
      set => toBeUnpended = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private Common adjust;
    private Common toBeUnpended;
    private CollectionType collectionType;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
    }

    /// <summary>
    /// A value of ReleaseToRecorded.
    /// </summary>
    [JsonPropertyName("releaseToRecorded")]
    public Common ReleaseToRecorded
    {
      get => releaseToRecorded ??= new();
      set => releaseToRecorded = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of HardcodedRecorded.
    /// </summary>
    [JsonPropertyName("hardcodedRecorded")]
    public CashReceiptDetailStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
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
    /// A value of HardcodedDistributed.
    /// </summary>
    [JsonPropertyName("hardcodedDistributed")]
    public CashReceiptDetailStatus HardcodedDistributed
    {
      get => hardcodedDistributed ??= new();
      set => hardcodedDistributed = value;
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
    /// A value of HardcodedPended.
    /// </summary>
    [JsonPropertyName("hardcodedPended")]
    public CashReceiptDetailStatus HardcodedPended
    {
      get => hardcodedPended ??= new();
      set => hardcodedPended = value;
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
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public CashReceiptDetail Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of CollectionStatusFound.
    /// </summary>
    [JsonPropertyName("collectionStatusFound")]
    public Common CollectionStatusFound
    {
      get => collectionStatusFound ??= new();
      set => collectionStatusFound = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
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

    private CashReceiptDetailStatus hardcodedAdjusted;
    private Common releaseToRecorded;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private DateWorkArea max;
    private DateWorkArea current;
    private CashReceiptDetailStatus hardcodedRecorded;
    private CashReceiptDetailStatus hardcodedRefunded;
    private CashReceiptDetailStatus hardcodedDistributed;
    private CashReceiptDetailStatus hardcodedSuspended;
    private CashReceiptDetailStatus hardcodedPended;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptDetail amount;
    private Common collectionStatusFound;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("keyCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory KeyCashReceiptDetailStatHistory
    {
      get => keyCashReceiptDetailStatHistory ??= new();
      set => keyCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of KeyCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("keyCashReceiptDetailStatus")]
    public CashReceiptDetailStatus KeyCashReceiptDetailStatus
    {
      get => keyCashReceiptDetailStatus ??= new();
      set => keyCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of KeyCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("keyCashReceiptDetail")]
    public CashReceiptDetail KeyCashReceiptDetail
    {
      get => keyCashReceiptDetail ??= new();
      set => keyCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CollectionType New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public CollectionType Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CashReceiptDetailHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailHistory")]
    public CashReceiptDetailHistory CashReceiptDetailHistory
    {
      get => cashReceiptDetailHistory ??= new();
      set => cashReceiptDetailHistory = value;
    }

    private CashReceiptDetailStatHistory keyCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus keyCashReceiptDetailStatus;
    private CashReceiptDetail keyCashReceiptDetail;
    private CollectionType new1;
    private CollectionType current;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailHistory cashReceiptDetailHistory;
  }
#endregion
}

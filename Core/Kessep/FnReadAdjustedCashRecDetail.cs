// Program: FN_READ_ADJUSTED_CASH_REC_DETAIL, ID: 372379095, model: 746.
// Short name: SWE00539
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_ADJUSTED_CASH_REC_DETAIL.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will read a collection based on the obligation and cash receipt 
/// information.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadAdjustedCashRecDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_ADJUSTED_CASH_REC_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadAdjustedCashRecDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadAdjustedCashRecDetail.
  /// </summary>
  public FnReadAdjustedCashRecDetail(IContext context, Import import,
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
    // -----------------------------------------------
    // 11/11/1998  N.Engoor
    // If coming from either COLL or DEBT the import debt coll flag is set to '
    // Y' and all cash receipt & cash receipt detail related information is
    // retrieved back using the collection sysgen_id selected on either screen
    // and passed to COLA.
    // 05/10/1999 N.Engoor
    // Modified the READ statements for retrieving the cash receipt detail and 
    // the cash receipt.
    // 02/03/2000 N.Engoor
    // Read of collection entity to see if any collection manually posted. If so
    // set the flag.
    // -----------------------------------------------
    // Work order # 197, M Brown, Oct 2000 - added Collection dist method of 'W'
    // wherever dist method of 'M' is referred to. They mean the same thing in 
    // batch processing.  The 'W' is set by debt adjustment when a debt is
    // written off, to
    // enable us to set it back to 'A'utomatic if the debt is reinstated.
    // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's deleted by 
    // REIP.
    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    // Check for collection protection for the cash receipt, and set a flag 
    // accordingly.
    // *****************************************************************
    local.Current.Date = Now().Date;

    if (AsChar(import.DebtCollCommon.Flag) == 'Y')
    {
      if (ReadCollectionObligationTransactionObligationObligationType())
      {
        export.Debcoll.Amount = entities.Collection.Amount;

        if (ReadCashReceiptDetailCashReceiptCashReceiptType())
        {
          export.CashReceipt.SequentialNumber =
            entities.CashReceipt.SequentialNumber;
          MoveCashReceiptDetail(entities.CashReceiptDetail,
            export.CashReceiptDetail);
          MoveCashReceiptSourceType(entities.CashReceiptSourceType,
            export.CashReceiptSourceType);
          MoveCashReceiptType(entities.CashReceiptType, export.CashReceiptType);
          export.CashReceiptEvent.SystemGeneratedIdentifier =
            entities.CashReceiptEvent.SystemGeneratedIdentifier;
          ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus();

          // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's 
          // deleted by REIP.
          // *****************************************************************
          if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
            "REIPDELETE"))
          {
            ExitState = "CASH_RECEIPT_DETAIL_NF";

            return;
          }

          export.CashReceiptDetailStatus.Code =
            entities.CashReceiptDetailStatus.Code;

          if (!Equal(export.CashReceiptDetailStatus.Code, "ADJ"))
          {
            export.UndistAmt.TotalCurrency =
              export.CashReceiptDetail.CollectionAmount - (
                export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() +
              export.CashReceiptDetail.RefundedAmount.GetValueOrDefault());
          }

          if (ReadCollectionType())
          {
            MoveCollectionType(entities.CollectionType, export.CollectionType);
          }
          else
          {
            // -------
            // No Collection Type to display
            // -------
          }

          if (ReadCollectionAdjustmentReason())
          {
            export.CollectionAdjustmentReason.Assign(
              entities.CollectionAdjustmentReason);
          }

          if (!IsEmpty(entities.Collection.CollectionAdjustmentReasonTxt))
          {
            MoveCollection(entities.Collection, export.Collection);
            export.Collection.CollectionAdjustmentDt =
              entities.Collection.CollectionAdjustmentDt;
          }

          if (IsEmpty(entities.CashReceiptDetail.ObligorPersonNumber))
          {
            UseFnReadCsePerson();
            export.CsePersonsWorkSet.Number = export.CsePerson.Number;

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            export.CsePersonsWorkSet.Number =
              entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
            export.CsePerson.Number =
              entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
          }

          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        else
        {
          ExitState = "FN0000_CASH_RECEIPT_NF";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_COLLECTION_NF";

        return;
      }
    }
    else if (ReadCashReceiptSourceTypeCashReceiptTypeCashReceipt())
    {
      if (ReadCashReceiptDetail())
      {
        export.CashReceipt.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        MoveCashReceiptDetail(entities.CashReceiptDetail,
          export.CashReceiptDetail);
        MoveCashReceiptSourceType(entities.CashReceiptSourceType,
          export.CashReceiptSourceType);
        MoveCashReceiptType(entities.CashReceiptType, export.CashReceiptType);
        export.CashReceiptEvent.SystemGeneratedIdentifier =
          entities.CashReceiptEvent.SystemGeneratedIdentifier;

        // ---------------------------------------------
        // MTW - Chayan 03/28/1997 Change start
        // For populating the status field
        // ---------------------------------------------
        ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus();

        // 09/19/01    P. Phinney     WR010561  Prevent display of CRD's deleted
        // by REIP.
        // *****************************************************************
        if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
          "REIPDELETE"))
        {
          ExitState = "CASH_RECEIPT_DETAIL_NF";

          return;
        }

        export.CashReceiptDetailStatus.Code =
          entities.CashReceiptDetailStatus.Code;

        if (!Equal(export.CashReceiptDetailStatus.Code, "ADJ"))
        {
          export.UndistAmt.TotalCurrency =
            export.CashReceiptDetail.CollectionAmount - (
              export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() + export
            .CashReceiptDetail.RefundedAmount.GetValueOrDefault());
        }

        // ---------------------------------------------
        // MTW - Chayan 03/28/1997 Change End
        // ---------------------------------------------
        if (ReadCollectionType())
        {
          MoveCollectionType(entities.CollectionType, export.CollectionType);
        }
        else
        {
          // ---------
          // No Collection Type to display
          // ---------
        }

        if (export.CashReceiptDetail.DistributedAmount.GetValueOrDefault() == 0)
        {
          if (ReadCollection1())
          {
            if (ReadCollectionAdjustmentReason())
            {
              export.CollectionAdjustmentReason.Assign(
                entities.CollectionAdjustmentReason);
            }

            if (!IsEmpty(entities.Collection.CollectionAdjustmentReasonTxt))
            {
              MoveCollection(entities.Collection, export.Collection);
              export.Collection.CollectionAdjustmentDt =
                entities.Collection.CollectionAdjustmentDt;
            }
          }
        }

        if (IsEmpty(entities.CashReceiptDetail.ObligorPersonNumber))
        {
          UseFnReadCsePerson();
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        else
        {
          export.CsePersonsWorkSet.Number =
            entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
          export.CsePerson.Number =
            entities.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
        }

        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
      else
      {
        ExitState = "CASH_RECEIPT_DETAIL_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_CASH_RECEIPT_NF";

      return;
    }

    // Jan., 2002 M. Brown Work Order # 010504 Retro Processing
    if (ReadObligCollProtectionHist())
    {
      export.CollProtExists.Flag = "Y";
    }
    else
    {
      export.CollProtExists.Flag = "N";
    }

    // Work order # 197, M Brown, Oct 2000
    if (ReadCollection2())
    {
      export.ManuallyPostedColl.SelectChar = "Y";
    }
    else
    {
      export.ManuallyPostedColl.SelectChar = "N";
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.InterfaceTransId = source.InterfaceTransId;
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
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

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private void UseFnReadCsePerson()
  {
    var useImport = new FnReadCsePerson.Import();
    var useExport = new FnReadCsePerson.Export();

    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.Assign(export.CashReceiptDetail);

    Call(FnReadCsePerson.Execute, useImport, useExport);

    export.CsePerson.Number = useExport.CsePerson.Number;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
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
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptType",
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
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CaseNumber = db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 14);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 15);
        entities.CashReceiptType.Code = db.GetString(reader, 16);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 18);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
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
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 8);
        entities.CashReceiptDetailStatus.EffectiveDate = db.GetDate(reader, 9);
        entities.CashReceiptDetailStatus.DiscontinueDate =
          db.GetNullableDate(reader, 10);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptTypeCashReceipt()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptTypeCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.Code = db.GetString(reader, 3);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 6);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptType.Populated = true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Collection.CarId = db.GetNullableInt32(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedBy = db.GetString(reader, 16);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 17);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 18);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.DistributionMethod = db.GetString(reader, 21);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 22);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ManuallyPosted.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ManuallyPosted.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ManuallyPosted.AdjustedInd = db.GetNullableString(reader, 1);
        entities.ManuallyPosted.CrtType = db.GetInt32(reader, 2);
        entities.ManuallyPosted.CstId = db.GetInt32(reader, 3);
        entities.ManuallyPosted.CrvId = db.GetInt32(reader, 4);
        entities.ManuallyPosted.CrdId = db.GetInt32(reader, 5);
        entities.ManuallyPosted.ObgId = db.GetInt32(reader, 6);
        entities.ManuallyPosted.CspNumber = db.GetString(reader, 7);
        entities.ManuallyPosted.CpaType = db.GetString(reader, 8);
        entities.ManuallyPosted.OtrId = db.GetInt32(reader, 9);
        entities.ManuallyPosted.OtrType = db.GetString(reader, 10);
        entities.ManuallyPosted.OtyId = db.GetInt32(reader, 11);
        entities.ManuallyPosted.DistributionMethod = db.GetString(reader, 12);
        entities.ManuallyPosted.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ManuallyPosted.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.ManuallyPosted.CpaType);
        CheckValid<Collection>("OtrType", entities.ManuallyPosted.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.ManuallyPosted.DistributionMethod);
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.EffectiveDt = db.GetDate(reader, 3);
        entities.CollectionAdjustmentReason.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionObligationTransactionObligationObligationType()
  {
    entities.CsePersonAccount.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Collection.Populated = false;

    return Read("ReadCollectionObligationTransactionObligationObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          import.DebtCollCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 8);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.Obligation.CspNumber = db.GetString(reader, 9);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 9);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 9);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 9);
        entities.CsePerson.Number = db.GetString(reader, 9);
        entities.CsePerson.Number = db.GetString(reader, 9);
        entities.CsePerson.Number = db.GetString(reader, 9);
        entities.CsePerson.Number = db.GetString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 10);
        entities.Obligation.CpaType = db.GetString(reader, 10);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 10);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 10);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 12);
        entities.Collection.CarId = db.GetNullableInt32(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 14);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedBy = db.GetString(reader, 16);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 17);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 18);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.DistributionMethod = db.GetString(reader, 21);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 22);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 23);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 24);
        entities.CsePersonAccount.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
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

  private bool ReadObligCollProtectionHist()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return Read("ReadObligCollProtectionHist",
      (db, command) =>
      {
        db.SetInt32(command, "crtType", entities.CashReceipt.CrtIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceipt.CstIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceipt.CrvIdentifier);
        db.SetDate(
          command, "deactivationDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 3);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 4);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 6);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 7);
        entities.ObligCollProtectionHist.Populated = true;
        CheckValid<ObligCollProtectionHist>("CpaType",
          entities.ObligCollProtectionHist.CpaType);
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of DebtCollCollection.
    /// </summary>
    [JsonPropertyName("debtCollCollection")]
    public Collection DebtCollCollection
    {
      get => debtCollCollection ??= new();
      set => debtCollCollection = value;
    }

    /// <summary>
    /// A value of DebtCollCommon.
    /// </summary>
    [JsonPropertyName("debtCollCommon")]
    public Common DebtCollCommon
    {
      get => debtCollCommon ??= new();
      set => debtCollCommon = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceipt cashReceipt;
    private Collection debtCollCollection;
    private Common debtCollCommon;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ManuallyPostedColl.
    /// </summary>
    [JsonPropertyName("manuallyPostedColl")]
    public Common ManuallyPostedColl
    {
      get => manuallyPostedColl ??= new();
      set => manuallyPostedColl = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Debcoll.
    /// </summary>
    [JsonPropertyName("debcoll")]
    public Collection Debcoll
    {
      get => debcoll ??= new();
      set => debcoll = value;
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of UndistAmt.
    /// </summary>
    [JsonPropertyName("undistAmt")]
    public Common UndistAmt
    {
      get => undistAmt ??= new();
      set => undistAmt = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CollProtExists.
    /// </summary>
    [JsonPropertyName("collProtExists")]
    public Common CollProtExists
    {
      get => collProtExists ??= new();
      set => collProtExists = value;
    }

    private Common manuallyPostedColl;
    private CashReceiptEvent cashReceiptEvent;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private Collection debcoll;
    private CashReceipt cashReceipt;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CollectionType collectionType;
    private Common undistAmt;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private Common collProtExists;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private DateWorkArea null1;
    private ScreenOwedAmounts screenOwedAmounts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of ManuallyPosted.
    /// </summary>
    [JsonPropertyName("manuallyPosted")]
    public Collection ManuallyPosted
    {
      get => manuallyPosted ??= new();
      set => manuallyPosted = value;
    }

    /// <summary>
    /// A value of LastAdjusted.
    /// </summary>
    [JsonPropertyName("lastAdjusted")]
    public Collection LastAdjusted
    {
      get => lastAdjusted ??= new();
      set => lastAdjusted = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Collection manuallyPosted;
    private Collection lastAdjusted;
    private CsePerson csePerson;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private Collection collection;
    private ObligCollProtectionHist obligCollProtectionHist;
  }
#endregion
}

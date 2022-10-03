// Program: DISPLAY_CR_DETAILS, ID: 371770021, model: 746.
// Short name: SWE00227
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DISPLAY_CR_DETAILS.
/// </para>
/// <para>
/// RESP: FINCLMGNT	
/// This action block will display a cash receipt detail (collection).  It does 
/// require the entire key.	
/// </para>
/// </summary>
[Serializable]
public partial class DisplayCrDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DISPLAY_CR_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DisplayCrDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DisplayCrDetails.
  /// </summary>
  public DisplayCrDetails(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 02/06/1999	Sunya Sharp	Added views to support change from cash due to 
    // total cash transaction amount.
    // 04/19/1999	Sunya Sharp	Joint return name was removed from page 2.  
    // Removing logic for check.
    // 05/25/1999	Sunya Sharp	Integration changes.
    export.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    export.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;

    if (ReadCashReceiptDetail1())
    {
      export.CashReceiptDetail.Assign(entities.CashReceiptDetail);
      export.OriginalCollAmt.AverageCurrency =
        entities.CashReceiptDetail.CollectionAmount;

      // --- Read any adjustments associated to this receipt ---
      if (ReadCashReceiptDetail2())
      {
        export.AdjustmentAmt.AverageCurrency =
          entities.Adjustment.CollectionAmount;
      }

      if (ReadCashReceipt())
      {
        export.CashReceipt.Assign(entities.CashReceipt);

        if (ReadCashReceiptSourceType())
        {
          export.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);
        }

        UseFnAbDetermineCollAmtApplied();
      }

      UseCabSetMaximumDiscontinueDate();

      if (ReadCashReceiptDetailStatHistory())
      {
        export.CashReceiptDetailStatHistory.ReasonCodeId =
          entities.CashReceiptDetailStatHistory.ReasonCodeId;

        if (ReadCashReceiptDetailStatus())
        {
          MoveCashReceiptDetailStatus(entities.CashReceiptDetailStatus,
            export.CashReceiptDetailStatus);

          // *** Added logic to determine if the cash receipt detail is in 
          // released or pended.  If the detail is in one of these statuses need
          // to use the created by from the cash receipt detail stat history
          // record to be displayed in the cash receipt detail last updated by
          // field returned to the screen.  This is because when a detail is
          // placed in released or pended the detail is not updated but the user
          // needs to see who did the last action to the detail.  Sunya Sharp
          // 11/2/98 ***
          UseFnHardcodedCashReceipting();

          if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
            .Pended.SystemGeneratedIdentifier || entities
            .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
            .Released.SystemGeneratedIdentifier || entities
            .CashReceiptDetailStatus.SystemGeneratedIdentifier == local
            .Suspended.SystemGeneratedIdentifier)
          {
            if (Lt(export.CashReceiptDetail.LastUpdatedTmst,
              entities.CashReceiptDetailStatHistory.CreatedTimestamp))
            {
              export.CashReceiptDetail.LastUpdatedBy =
                entities.CashReceiptDetailStatHistory.CreatedBy;
            }
          }

          if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
            .Adjusted.SystemGeneratedIdentifier)
          {
            if (export.AdjustmentAmt.AverageCurrency == 0)
            {
              export.AdjustmentAmt.AverageCurrency =
                entities.CashReceiptDetail.CollectionAmount;
              export.CashReceiptLiterals.AdjustmentsExist = "ADJUSTMENTS EXIST";
            }
          }

          export.Suspended.TotalCurrency =
            entities.CashReceiptDetail.CollectionAmount - entities
            .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
            .CashReceiptDetail.RefundedAmount.GetValueOrDefault() - export
            .AdjustmentAmt.AverageCurrency;
        }
      }
      else
      {
        ExitState = "FN0064_CASH_RCPT_DTL_STAT_HST_NF";
      }

      // *** Determine if multi-payor court.  Sunya Sharp 5/25/1999 ***
      if (!IsEmpty(entities.CashReceiptDetail.CourtOrderNumber))
      {
        UseFnAbObligorListForCtOrder();

        if (local.WorkNoOfObligors.Count > 1)
        {
          export.WorkIsMultiPayor.Flag = "Y";
        }
        else
        {
          export.WorkIsMultiPayor.Flag = "N";
        }
      }

      // *** Added logic to say that no active address was found for the cash 
      // receipt detail.  Sunya Sharp 10/29/98 ***
      if (ReadCashReceiptDetailAddress())
      {
        export.CashReceiptDetailAddress.
          Assign(entities.CashReceiptDetailAddress);
      }
      else
      {
        // ok, not all details have to have an address.
        export.CashReceiptDetailAddress.Street1 = "No Active address found";
        export.CashReceiptDetailAddress.State = "KS";
      }

      if (ReadCollectionType())
      {
        MoveCollectionType(entities.CollectionType, export.CollectionType);
      }
      else
      {
        // ok, not all collections may have a type when they are first set up.
      }

      if (ReadCashReceiptDetailFee())
      {
        export.CashReceiptLiterals.FeesExist = "FEES EXIST";
      }
      else
      {
        // OK NOT ALL COLLECTIONS WILL HAVE FEES.
      }

      if (ReadCashReceiptDetailBalanceAdj1())
      {
        export.CashReceiptLiterals.AdjustmentsExist = "ADJUSTMENTS EXIST";
      }
      else
      {
        // OK NOT ALL COLLECTIONS WILL HAVE ADJUSTMENTS.
      }

      if (ReadCashReceiptDetailBalanceAdj2())
      {
        export.CashReceiptLiterals.AdjustmentsExist = "ADJUSTMENTS EXIST";
      }
      else
      {
        // OK NOT ALL COLLECTIONS WILL HAVE ADJUSTMENTS.
      }

      // *** When a manual adjustment is done and the cash receipt detail is 
      // reverted to REC status, the user would like the information message "
      // ADJUSTMENT EXIST" is displayed if there were any collections adjusted.
      // Sunya Sharp 5/25/1999 ***
      if (IsEmpty(export.CashReceiptLiterals.AdjustmentsExist))
      {
        if (ReadCollection())
        {
          export.CashReceiptLiterals.AdjustmentsExist = "ADJUSTMENTS EXIST";
        }
        else
        {
          // OK NOT ALL COLLECTIONS WILL HAVE ADJUSTMENTS.
        }
      }

      if (!IsEmpty(entities.CashReceiptDetail.Notes))
      {
        export.CashReceiptLiterals.NotesExist = "NOTES EXIST";
      }

      // *** Removed check to see if multi-payor is not equal to spaces.  This 
      // is no longer contained on page 2.  Sunya Sharp 10/29/98.
      if (!IsEmpty(entities.CashReceiptDetail.Attribute1SupportedPersonFirstName)
        || !
        IsEmpty(entities.CashReceiptDetail.Attribute1SupportedPersonLastName) ||
        !
        IsEmpty(entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName) ||
        !
        IsEmpty(entities.CashReceiptDetail.Attribute2SupportedPersonFirstName) ||
        !
        IsEmpty(entities.CashReceiptDetail.Attribute2SupportedPersonLastName) ||
        !
        IsEmpty(entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName) ||
        !IsEmpty(entities.CashReceiptDetail.JointReturnInd) || !
        Equal(entities.CashReceiptDetail.OffsetTaxYear, 0) || !
        Equal(entities.CashReceiptDetail.OffsetTaxid, 0) || !
        IsEmpty(entities.CashReceiptDetail.PayeeFirstName) || !
        IsEmpty(entities.CashReceiptDetail.PayeeLastName) || !
        IsEmpty(entities.CashReceiptDetail.PayeeMiddleName))
      {
        export.CashReceiptLiterals.MoreOnPage2 = "NEXT PAGE";
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private void UseFnAbDetermineCollAmtApplied()
  {
    var useImport = new FnAbDetermineCollAmtApplied.Import();
    var useExport = new FnAbDetermineCollAmtApplied.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;

    Call(FnAbDetermineCollAmtApplied.Execute, useImport, useExport);

    export.CollAmtApplied.TotalCurrency =
      useExport.CollAmtApplied.TotalCurrency;
  }

  private void UseFnAbObligorListForCtOrder()
  {
    var useImport = new FnAbObligorListForCtOrder.Import();
    var useExport = new FnAbObligorListForCtOrder.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CashReceiptDetail.CourtOrderNumber;

    Call(FnAbObligorListForCtOrder.Execute, useImport, useExport);

    local.WorkNoOfObligors.Count = useExport.WorkNoOfObligors.Count;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.Adjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
    local.Pended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
    local.Released.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.Suspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
  }

  private bool ReadCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 5);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 7);
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
        entities.CashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 41);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 42);
        entities.CashReceiptDetail.InjuredSpouseInd =
          db.GetNullableString(reader, 43);
        entities.CashReceiptDetail.JfaReceivedDate =
          db.GetNullableDate(reader, 44);
        entities.CashReceiptDetail.CruProcessedDate =
          db.GetNullableDate(reader, 45);
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

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjustment.Populated = false;

    return Read("ReadCashReceiptDetail2",
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
        entities.Adjustment.CrvIdentifier = db.GetInt32(reader, 0);
        entities.Adjustment.CstIdentifier = db.GetInt32(reader, 1);
        entities.Adjustment.CrtIdentifier = db.GetInt32(reader, 2);
        entities.Adjustment.SequentialIdentifier = db.GetInt32(reader, 3);
        entities.Adjustment.AdjustmentInd = db.GetNullableString(reader, 4);
        entities.Adjustment.CollectionAmount = db.GetDecimal(reader, 5);
        entities.Adjustment.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
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

  private bool ReadCashReceiptDetailBalanceAdj1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj1",
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
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailBalanceAdj2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return Read("ReadCashReceiptDetailBalanceAdj2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdSIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvSIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstSIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtSIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return Read("ReadCashReceiptDetailFee",
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
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;

    return Read("ReadCashReceiptDetailStatHistory",
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
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
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
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailStatHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          entities.CashReceiptDetailStatHistory.CdsIdentifier);
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
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crSrceTypeId", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
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
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
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
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WorkIsMultiPayor.
    /// </summary>
    [JsonPropertyName("workIsMultiPayor")]
    public Common WorkIsMultiPayor
    {
      get => workIsMultiPayor ??= new();
      set => workIsMultiPayor = value;
    }

    /// <summary>
    /// A value of OriginalCollAmt.
    /// </summary>
    [JsonPropertyName("originalCollAmt")]
    public Common OriginalCollAmt
    {
      get => originalCollAmt ??= new();
      set => originalCollAmt = value;
    }

    /// <summary>
    /// A value of AdjustmentAmt.
    /// </summary>
    [JsonPropertyName("adjustmentAmt")]
    public Common AdjustmentAmt
    {
      get => adjustmentAmt ??= new();
      set => adjustmentAmt = value;
    }

    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public Common Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
    }

    /// <summary>
    /// A value of CollAmtApplied.
    /// </summary>
    [JsonPropertyName("collAmtApplied")]
    public Common CollAmtApplied
    {
      get => collAmtApplied ??= new();
      set => collAmtApplied = value;
    }

    /// <summary>
    /// A value of CashReceiptLiterals.
    /// </summary>
    [JsonPropertyName("cashReceiptLiterals")]
    public CashReceiptLiterals CashReceiptLiterals
    {
      get => cashReceiptLiterals ??= new();
      set => cashReceiptLiterals = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private Common workIsMultiPayor;
    private Common originalCollAmt;
    private Common adjustmentAmt;
    private Common suspended;
    private Common collAmtApplied;
    private CashReceiptLiterals cashReceiptLiterals;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CollectionType collectionType;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Suspended.
    /// </summary>
    [JsonPropertyName("suspended")]
    public CashReceiptDetailStatus Suspended
    {
      get => suspended ??= new();
      set => suspended = value;
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
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public CashReceiptDetailStatus Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
    }

    /// <summary>
    /// A value of Pended.
    /// </summary>
    [JsonPropertyName("pended")]
    public CashReceiptDetailStatus Pended
    {
      get => pended ??= new();
      set => pended = value;
    }

    /// <summary>
    /// A value of Released.
    /// </summary>
    [JsonPropertyName("released")]
    public CashReceiptDetailStatus Released
    {
      get => released ??= new();
      set => released = value;
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

    private CashReceiptDetailStatus suspended;
    private Common workNoOfObligors;
    private CashReceiptDetailStatus adjusted;
    private CashReceiptDetailStatus pended;
    private CashReceiptDetailStatus released;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceiptDetail Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public CashReceiptDetail Total
    {
      get => total ??= new();
      set => total = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
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
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
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

    private Collection collection;
    private CashReceiptDetail adjustment;
    private CashReceiptDetail total;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CollectionType collectionType;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}

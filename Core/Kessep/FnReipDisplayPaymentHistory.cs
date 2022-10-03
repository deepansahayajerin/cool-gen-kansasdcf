// Program: FN_REIP_DISPLAY_PAYMENT_HISTORY, ID: 372418909, model: 746.
// Short name: SWE02036
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_REIP_DISPLAY_PAYMENT_HISTORY.
/// </summary>
[Serializable]
public partial class FnReipDisplayPaymentHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_DISPLAY_PAYMENT_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipDisplayPaymentHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipDisplayPaymentHistory.
  /// </summary>
  public FnReipDisplayPaymentHistory(IContext context, Import import,
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
    // -------------------------------------------------------------------------
    // Date	  Programmer		Reason #	Description
    // -------------------------------------------------------------------------
    // 01/12/95  Holly Kennedy-MTW			Source
    // 12/30/97  Syed Hasan,MTW       PR# 32883	Modified logic to
    // 						total for all collection
    // 						amounts regardless of
    // 						what is moved into
    // 						export view.
    // 11/18/98  Judy Katz-SRG			Revised code to reflect REIP
    // 					screen redesign as described
    // 					in the Screen Assessment and
    // 					Correction Form.
    // 12/17/98  Judy Katz-SRG			Added total amount logic for
    // 					payment history records
    // 					with ADJ and REF statuses.
    // 					Revised logic to skip payment
    // 					history records with an
    // 					invalid PEND status.
    // 01/22/99  Judy Katz - SRG		Revised error handling
    // 					to include records with data
    // 					integrity problems in display
    // 					list.  These records are not
    // 					included in the totals.
    // 03/15/99  Judy Katz-SRG			Modify summary total buckets
    // 					to be consistent with other
    // 					screens.
    // 					Remove edit that designates
    // 					the PEND detail status as an
    // 					error condition.
    // 06/08/99  Judy Katz-SRG			Analyzed READ statements and
    // 					changed read property to
    // 					Select Only where appropriate.
    // -------------------------------------------------------------------------
    // ---------------------------------------------------------------
    // AP/Payor Person Number and Court Order Number must be imported.
    // ---------------------------------------------------------------
    if (IsEmpty(import.CsePerson.Number) || IsEmpty
      (import.LegalAction.StandardNumber))
    {
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }

    // ---------------------------------------------------------------
    // Set up local views.
    // ---------------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (import.ListStarting.CollectionDate != null)
    {
      local.Starting.Date = import.ListStarting.CollectionDate;
    }
    else
    {
      local.Starting.Date = local.Current.Date;
    }

    UseFnHardcodedCashReceipting();
    export.Export1.Index = -1;

    // ---------------------------------------------------------------
    // Retrieve all payment history records for the imported AP/Payor
    // (Obligor) and Court Order.
    // Modify sort logic to display payment history in descending
    // collection date and descending receipt number order.
    // JLK  06/11/99
    // ---------------------------------------------------------------
    foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptType())
    {
      // ---------------------------------------------------------------
      // If payment history Cash Receipt has an active status of DEL,
      // skip to the next record.
      // ---------------------------------------------------------------
      if (ReadCashReceiptStatusHistoryCashReceiptStatus())
      {
        // ---------------------------------------------------------------
        // Skip deleted payment history records.    JLK  12/17/98
        // ---------------------------------------------------------------
        if (entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier == local
          .HardcodedCrsDeleted.SystemGeneratedIdentifier)
        {
          continue;
        }
      }
      else
      {
        // ----------------------------------------------------------
        // Although all payment history records should have an active
        // Cash Receipt Status History record, processing will not be
        // stopped for this error.
        // ----------------------------------------------------------
      }

      // ---------------------------------------------------------------
      // Read active Cash Receipt Detail Status.
      // ---------------------------------------------------------------
      if (ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
      {
        // ---------------------------------------------------------------
        // Payment history records should never have a Cash Receipt
        // Detail Status of Pended.  Records with this status should
        // be considered a data integrity problem.   JLK  01/22/99
        // Removed edit that handled Pended status as data integrity
        // problem.  A portion of the payment history record may have
        // been processed (REF or DIST) and these amounts should
        // be included in the calculated summary totals.  There is no
        // way of knowing if all or part of the remaining dollars are in
        // Pended status.    JLK  03/15/99
        // ---------------------------------------------------------------
        MoveCashReceiptDetailStatus(entities.ActiveCashReceiptDetailStatus,
          local.Active);
      }
      else
      {
        // ---------------------------------------------------------------
        // Payment history records without an active Cash Receipt Detail
        // Status are considered a data integrity problem.  JLK  01/22/99
        // ---------------------------------------------------------------
        local.Active.SystemGeneratedIdentifier = 0;
        local.Active.Code = "";
        local.Error.Flag = "Y";
        ExitState = "FN0000_LIST_DISPLAYED_WITH_ERROR";
      }

      // ---------------------------------------------------------------
      // Additional logic only required if the record is to be included
      // in group export list.
      // ---------------------------------------------------------------
      if (!Lt(local.Starting.Date,
        entities.PaymentHistoryCashReceiptDetail.CollectionDate) && !
        Lt(entities.PaymentHistoryCashReceiptDetail.CollectionDate,
        import.ListEnding.CollectionDate))
      {
        // ---------------------------------------------------------------
        // Complete the remaining Reads required to populate the group view.
        // ---------------------------------------------------------------
        if (ReadCashReceiptEventCashReceiptSourceType())
        {
          MoveCashReceiptSourceType(entities.
            PaymentHistoryCashReceiptSourceType,
            local.PaymentHistoryCashReceiptSourceType);
        }
        else
        {
          local.PaymentHistoryCashReceiptSourceType.SystemGeneratedIdentifier =
            0;
          local.PaymentHistoryCashReceiptSourceType.Code = "";
          local.Error.Flag = "Y";
          ExitState = "FN0000_LIST_DISPLAYED_WITH_ERROR";
        }

        if (ReadCollectionType())
        {
          MoveCollectionType(entities.PaymentHistoryCollectionType,
            local.PaymentHistoryCollectionType);
        }
        else
        {
          // ---------------------------------------------------------------
          // It is expected that all payment history records will have a
          // collection type assigned.   If none is found, a data integrity
          // problem has occurred.
          // The user should be allowed to update the payment history
          // record with a valid collection type.   JLK  01/22/99
          // ---------------------------------------------------------------
          local.PaymentHistoryCollectionType.SequentialIdentifier = 0;
          local.PaymentHistoryCollectionType.Code = "";
          local.Error.Flag = "Y";
          ExitState = "FN0000_LIST_DISPLAYED_WITH_ERROR";
        }

        // ---------------------------------------------------------------
        // Increment subscript and determine if the maximum number of
        // records has been reached.
        // ---------------------------------------------------------------
        if (export.Export1.Index + 1 < Export.ExportGroup.Capacity)
        {
          ++export.Export1.Index;
          export.Export1.CheckSize();

          // ---------------------------------------------------------------
          // Translate the Cash Receipt Type to a C or D for display purposes.
          // ---------------------------------------------------------------
          if (entities.PaymentHistoryCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedCrtFcourtPmt.SystemGeneratedIdentifier)
          {
            export.Export1.Update.CourtOrDp.SelectChar = "C";
          }
          else if (entities.PaymentHistoryCashReceiptType.
            SystemGeneratedIdentifier == local
            .HardcodedCrtFdirPmt.SystemGeneratedIdentifier)
          {
            export.Export1.Update.CourtOrDp.SelectChar = "D";
          }
          else
          {
            // --------------------------------------------------------
            // Receipt does not have a payment history Type Code.
            // --------------------------------------------------------
            continue;
          }

          if (!IsEmpty(entities.PaymentHistoryCashReceiptDetail.Notes))
          {
            export.Export1.Update.DetNoteInd.Flag = "Y";
          }

          if (Equal(entities.ActiveCashReceiptDetailStatus.Code, "SUSP"))
          {
            // ---------------------------------------------------------------
            // If suspended for manual distribution, set Man Dist Ind to Y.
            // ---------------------------------------------------------------
            if (Equal(entities.ActiveCashReceiptDetailStatHistory.ReasonCodeId,
              "MANUALDIST"))
            {
              export.Export1.Update.DetManDistInd.Flag = "Y";
            }
          }

          // ---------------------------------------------------------------
          // Move entity action views to the group export list.
          // ---------------------------------------------------------------
          export.Export1.Update.DetailCashReceiptDetail.Assign(
            entities.PaymentHistoryCashReceiptDetail);
          MoveCashReceiptSourceType(local.PaymentHistoryCashReceiptSourceType,
            export.Export1.Update.DetailCashReceiptSourceType);
          MoveCollectionType(local.PaymentHistoryCollectionType,
            export.Export1.Update.DetailCollectionType);
          export.Export1.Update.DetailCashReceipt.Assign(
            entities.PaymentHistoryCashReceipt);
          export.Export1.Update.DetailCashReceiptDetailStatus.Code =
            local.Active.Code;
          export.Export1.Update.HiddenCashReceiptEvent.
            SystemGeneratedIdentifier =
              entities.PaymentHistoryCashReceiptEvent.SystemGeneratedIdentifier;
            
          export.Export1.Update.HiddenCashReceiptType.
            SystemGeneratedIdentifier =
              entities.PaymentHistoryCashReceiptType.SystemGeneratedIdentifier;
        }
        else
        {
          // ------------------------------------------------------------
          // Group View is full.  Continue processing totals.
          // ------------------------------------------------------------
        }
      }

      // ---------------------------------------------------------------
      // Calculate Total # of Items and Total Amounts for display.
      // ---------------------------------------------------------------
      // The Total # of Items and the Total Payment Amt do not
      // include payment history details with an active status of ADJ,
      // REF, or PEND.    JLK  12/17/98
      // Records with data integrity errors are not included in the totals.
      // JLK  01/22/99
      // Modify the summary total buckets on REIP to be consistent
      // with other Cash Management screens.  The new buckets will
      // be Distributed, Refunded, Adjusted, and Undistributed.
      // Undistributed will include dollar amounts in REC, REL,
      // SUSP, and PEND statuses that are not distributed or
      // refunded.   JLK  03/15/99
      // Allow Pended records (count and dollars) to be accumulated
      // in the Total Payment values and the Undistributed Amount
      // bucket.    JLK  03/15/99
      // ---------------------------------------------------------------
      if (AsChar(local.Error.Flag) == 'Y')
      {
        local.Error.Flag = "";
      }
      else if (entities.ActiveCashReceiptDetailStatus.
        SystemGeneratedIdentifier == local
        .HardcodedCrdsAdj.SystemGeneratedIdentifier)
      {
        export.TotalAdj.TotalCurrency += entities.
          PaymentHistoryCashReceiptDetail.CollectionAmount;
      }
      else
      {
        if (entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier ==
          local.HardcodedCrdsRef.SystemGeneratedIdentifier && Equal
          (entities.PaymentHistoryCashReceiptDetail.CollectionAmount,
          entities.PaymentHistoryCashReceiptDetail.RefundedAmount))
        {
        }
        else
        {
          ++export.TotalItems.Count;
        }

        export.TotalPmt.TotalCurrency += entities.
          PaymentHistoryCashReceiptDetail.CollectionAmount - entities
          .PaymentHistoryCashReceiptDetail.RefundedAmount.GetValueOrDefault();
        export.TotalDist.TotalCurrency += entities.
          PaymentHistoryCashReceiptDetail.DistributedAmount.GetValueOrDefault();
          
        export.TotalRef.TotalCurrency += entities.
          PaymentHistoryCashReceiptDetail.RefundedAmount.GetValueOrDefault();
        export.TotalUndist.TotalCurrency += entities.
          PaymentHistoryCashReceiptDetail.CollectionAmount - entities
          .PaymentHistoryCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() - entities
          .PaymentHistoryCashReceiptDetail.RefundedAmount.GetValueOrDefault();
      }
    }
  }

  private static void MoveCashReceiptDetailStatus(
    CashReceiptDetailStatus source, CashReceiptDetailStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrsDeleted.SystemGeneratedIdentifier =
      useExport.CrsSystemId.CrsIdDeleted.SystemGeneratedIdentifier;
    local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
    local.HardcodedCrdsRef.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedCrdsAdj.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private IEnumerable<bool> ReadCashReceiptCashReceiptDetailCashReceiptType()
  {
    entities.PaymentHistoryCashReceiptType.Populated = false;
    entities.PaymentHistoryCashReceipt.Populated = false;
    entities.PaymentHistoryCashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptType",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetNullableString(
          command, "courtOrderNumber", import.LegalAction.StandardNumber ?? ""
          );
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentHistoryCashReceipt.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentHistoryCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentHistoryCashReceipt.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.PaymentHistoryCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.PaymentHistoryCashReceipt.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentHistoryCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentHistoryCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentHistoryCashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentHistoryCashReceipt.SequentialNumber =
          db.GetInt32(reader, 3);
        entities.PaymentHistoryCashReceipt.CheckNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentHistoryCashReceipt.ReferenceNumber =
          db.GetNullableString(reader, 5);
        entities.PaymentHistoryCashReceipt.CreatedBy = db.GetString(reader, 6);
        entities.PaymentHistoryCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.PaymentHistoryCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 8);
        entities.PaymentHistoryCashReceiptDetail.CollectionAmount =
          db.GetDecimal(reader, 9);
        entities.PaymentHistoryCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 10);
        entities.PaymentHistoryCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 11);
        entities.PaymentHistoryCashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.PaymentHistoryCashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.PaymentHistoryCashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.PaymentHistoryCashReceiptDetail.Notes =
          db.GetNullableString(reader, 15);
        entities.PaymentHistoryCashReceiptType.Populated = true;
        entities.PaymentHistoryCashReceipt.Populated = true;
        entities.PaymentHistoryCashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(
      entities.PaymentHistoryCashReceiptDetail.Populated);
    entities.ActiveCashReceiptDetailStatHistory.Populated = false;
    entities.ActiveCashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.PaymentHistoryCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.PaymentHistoryCashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.PaymentHistoryCashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.PaymentHistoryCashReceiptDetail.CrtIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.ActiveCashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ActiveCashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.ActiveCashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ActiveCashReceiptDetailStatus.Code = db.GetString(reader, 8);
        entities.ActiveCashReceiptDetailStatHistory.Populated = true;
        entities.ActiveCashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.
      Assert(entities.PaymentHistoryCashReceipt.Populated);
    entities.PaymentHistoryCashReceiptSourceType.Populated = false;
    entities.PaymentHistoryCashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "cstIdentifier",
          entities.PaymentHistoryCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.PaymentHistoryCashReceipt.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentHistoryCashReceiptEvent.CstIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentHistoryCashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.PaymentHistoryCashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentHistoryCashReceiptSourceType.Code =
          db.GetString(reader, 3);
        entities.PaymentHistoryCashReceiptSourceType.Populated = true;
        entities.PaymentHistoryCashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.PaymentHistoryCashReceipt.Populated);
    entities.ActiveCashReceiptStatusHistory.Populated = false;
    entities.ActiveCashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier",
          entities.PaymentHistoryCashReceipt.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.PaymentHistoryCashReceipt.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.PaymentHistoryCashReceipt.CrvIdentifier);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ActiveCashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.ActiveCashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ActiveCashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.ActiveCashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ActiveCashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.ActiveCashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.ActiveCashReceiptStatusHistory.Populated = true;
        entities.ActiveCashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(
      entities.PaymentHistoryCashReceiptDetail.Populated);
    entities.PaymentHistoryCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.PaymentHistoryCashReceiptDetail.CltIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentHistoryCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentHistoryCollectionType.Code = db.GetString(reader, 1);
        entities.PaymentHistoryCollectionType.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of ListStarting.
    /// </summary>
    [JsonPropertyName("listStarting")]
    public CashReceiptDetail ListStarting
    {
      get => listStarting ??= new();
      set => listStarting = value;
    }

    /// <summary>
    /// A value of ListEnding.
    /// </summary>
    [JsonPropertyName("listEnding")]
    public CashReceiptDetail ListEnding
    {
      get => listEnding ??= new();
      set => listEnding = value;
    }

    private LegalAction legalAction;
    private CsePerson csePerson;
    private CashReceiptDetail listStarting;
    private CashReceiptDetail listEnding;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
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
      /// A value of DetailCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetail")]
      public CashReceiptDetail DetailCashReceiptDetail
      {
        get => detailCashReceiptDetail ??= new();
        set => detailCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of RecurringToDate.
      /// </summary>
      [JsonPropertyName("recurringToDate")]
      public CashReceiptDetail RecurringToDate
      {
        get => recurringToDate ??= new();
        set => recurringToDate = value;
      }

      /// <summary>
      /// A value of Frequency.
      /// </summary>
      [JsonPropertyName("frequency")]
      public TextWorkArea Frequency
      {
        get => frequency ??= new();
        set => frequency = value;
      }

      /// <summary>
      /// A value of CourtOrDp.
      /// </summary>
      [JsonPropertyName("courtOrDp")]
      public Common CourtOrDp
      {
        get => courtOrDp ??= new();
        set => courtOrDp = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetSourcePrompt.
      /// </summary>
      [JsonPropertyName("detSourcePrompt")]
      public Standard DetSourcePrompt
      {
        get => detSourcePrompt ??= new();
        set => detSourcePrompt = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetCollTypPrompt.
      /// </summary>
      [JsonPropertyName("detCollTypPrompt")]
      public Standard DetCollTypPrompt
      {
        get => detCollTypPrompt ??= new();
        set => detCollTypPrompt = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailStatus")]
      public CashReceiptDetailStatus DetailCashReceiptDetailStatus
      {
        get => detailCashReceiptDetailStatus ??= new();
        set => detailCashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of DetManDistInd.
      /// </summary>
      [JsonPropertyName("detManDistInd")]
      public Common DetManDistInd
      {
        get => detManDistInd ??= new();
        set => detManDistInd = value;
      }

      /// <summary>
      /// A value of DetNoteInd.
      /// </summary>
      [JsonPropertyName("detNoteInd")]
      public Common DetNoteInd
      {
        get => detNoteInd ??= new();
        set => detNoteInd = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private CashReceiptDetail detailCashReceiptDetail;
      private CashReceiptDetail recurringToDate;
      private TextWorkArea frequency;
      private Common courtOrDp;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private Standard detSourcePrompt;
      private CollectionType detailCollectionType;
      private Standard detCollTypPrompt;
      private CashReceipt detailCashReceipt;
      private CashReceiptDetailStatus detailCashReceiptDetailStatus;
      private Common detManDistInd;
      private Common detNoteInd;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptType hiddenCashReceiptType;
    }

    /// <summary>
    /// A value of TotalItems.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public Common TotalItems
    {
      get => totalItems ??= new();
      set => totalItems = value;
    }

    /// <summary>
    /// A value of TotalPmt.
    /// </summary>
    [JsonPropertyName("totalPmt")]
    public Common TotalPmt
    {
      get => totalPmt ??= new();
      set => totalPmt = value;
    }

    /// <summary>
    /// A value of TotalDist.
    /// </summary>
    [JsonPropertyName("totalDist")]
    public Common TotalDist
    {
      get => totalDist ??= new();
      set => totalDist = value;
    }

    /// <summary>
    /// A value of TotalRef.
    /// </summary>
    [JsonPropertyName("totalRef")]
    public Common TotalRef
    {
      get => totalRef ??= new();
      set => totalRef = value;
    }

    /// <summary>
    /// A value of TotalAdj.
    /// </summary>
    [JsonPropertyName("totalAdj")]
    public Common TotalAdj
    {
      get => totalAdj ??= new();
      set => totalAdj = value;
    }

    /// <summary>
    /// A value of TotalUndist.
    /// </summary>
    [JsonPropertyName("totalUndist")]
    public Common TotalUndist
    {
      get => totalUndist ??= new();
      set => totalUndist = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Common totalItems;
    private Common totalPmt;
    private Common totalDist;
    private Common totalRef;
    private Common totalAdj;
    private Common totalUndist;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceiptSourceType")]
    public CashReceiptSourceType PaymentHistoryCashReceiptSourceType
    {
      get => paymentHistoryCashReceiptSourceType ??= new();
      set => paymentHistoryCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCollectionType.
    /// </summary>
    [JsonPropertyName("paymentHistoryCollectionType")]
    public CollectionType PaymentHistoryCollectionType
    {
      get => paymentHistoryCollectionType ??= new();
      set => paymentHistoryCollectionType = value;
    }

    /// <summary>
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public CashReceiptDetailStatus Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of HardcodedCrsDeleted.
    /// </summary>
    [JsonPropertyName("hardcodedCrsDeleted")]
    public CashReceiptStatus HardcodedCrsDeleted
    {
      get => hardcodedCrsDeleted ??= new();
      set => hardcodedCrsDeleted = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFdirPmt")]
    public CashReceiptType HardcodedCrtFdirPmt
    {
      get => hardcodedCrtFdirPmt ??= new();
      set => hardcodedCrtFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFcourtPmt")]
    public CashReceiptType HardcodedCrtFcourtPmt
    {
      get => hardcodedCrtFcourtPmt ??= new();
      set => hardcodedCrtFcourtPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsRef.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsRef")]
    public CashReceiptDetailStatus HardcodedCrdsRef
    {
      get => hardcodedCrdsRef ??= new();
      set => hardcodedCrdsRef = value;
    }

    /// <summary>
    /// A value of HardcodedCrdsAdj.
    /// </summary>
    [JsonPropertyName("hardcodedCrdsAdj")]
    public CashReceiptDetailStatus HardcodedCrdsAdj
    {
      get => hardcodedCrdsAdj ??= new();
      set => hardcodedCrdsAdj = value;
    }

    private DateWorkArea starting;
    private DateWorkArea current;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Common error;
    private CashReceiptSourceType paymentHistoryCashReceiptSourceType;
    private CollectionType paymentHistoryCollectionType;
    private CashReceiptDetailStatus active;
    private CashReceiptStatus hardcodedCrsDeleted;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptType hardcodedCrtFcourtPmt;
    private CashReceiptDetailStatus hardcodedCrdsRef;
    private CashReceiptDetailStatus hardcodedCrdsAdj;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentHistoryCashReceiptType.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceiptType")]
    public CashReceiptType PaymentHistoryCashReceiptType
    {
      get => paymentHistoryCashReceiptType ??= new();
      set => paymentHistoryCashReceiptType = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCashReceipt.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceipt")]
    public CashReceipt PaymentHistoryCashReceipt
    {
      get => paymentHistoryCashReceipt ??= new();
      set => paymentHistoryCashReceipt = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceiptDetail")]
    public CashReceiptDetail PaymentHistoryCashReceiptDetail
    {
      get => paymentHistoryCashReceiptDetail ??= new();
      set => paymentHistoryCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatusHistory")]
    public CashReceiptStatusHistory ActiveCashReceiptStatusHistory
    {
      get => activeCashReceiptStatusHistory ??= new();
      set => activeCashReceiptStatusHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptStatus")]
    public CashReceiptStatus ActiveCashReceiptStatus
    {
      get => activeCashReceiptStatus ??= new();
      set => activeCashReceiptStatus = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCollectionType.
    /// </summary>
    [JsonPropertyName("paymentHistoryCollectionType")]
    public CollectionType PaymentHistoryCollectionType
    {
      get => paymentHistoryCollectionType ??= new();
      set => paymentHistoryCollectionType = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ActiveCashReceiptDetailStatHistory
    {
      get => activeCashReceiptDetailStatHistory ??= new();
      set => activeCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ActiveCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("activeCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ActiveCashReceiptDetailStatus
    {
      get => activeCashReceiptDetailStatus ??= new();
      set => activeCashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceiptSourceType")]
    public CashReceiptSourceType PaymentHistoryCashReceiptSourceType
    {
      get => paymentHistoryCashReceiptSourceType ??= new();
      set => paymentHistoryCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PaymentHistoryCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("paymentHistoryCashReceiptEvent")]
    public CashReceiptEvent PaymentHistoryCashReceiptEvent
    {
      get => paymentHistoryCashReceiptEvent ??= new();
      set => paymentHistoryCashReceiptEvent = value;
    }

    private CashReceiptType paymentHistoryCashReceiptType;
    private CashReceipt paymentHistoryCashReceipt;
    private CashReceiptDetail paymentHistoryCashReceiptDetail;
    private CashReceiptStatusHistory activeCashReceiptStatusHistory;
    private CashReceiptStatus activeCashReceiptStatus;
    private CollectionType paymentHistoryCollectionType;
    private CashReceiptDetailStatHistory activeCashReceiptDetailStatHistory;
    private CashReceiptDetailStatus activeCashReceiptDetailStatus;
    private CashReceiptSourceType paymentHistoryCashReceiptSourceType;
    private CashReceiptEvent paymentHistoryCashReceiptEvent;
  }
#endregion
}

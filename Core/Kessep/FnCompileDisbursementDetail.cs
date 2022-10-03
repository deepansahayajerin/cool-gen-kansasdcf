// Program: FN_COMPILE_DISBURSEMENT_DETAIL, ID: 371870091, model: 746.
// Short name: SWE00326
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
/// A program: FN_COMPILE_DISBURSEMENT_DETAIL.
/// </para>
/// <para>
/// This action block compiles the Disbursement details for the given warrant/
/// payment request.
/// The details compiled are Disbursement_Type Code, Disbursement_Transaction 
/// amount and Process_date AND Cash_Receipt_Detail Interface_trans_id.
/// </para>
/// </summary>
[Serializable]
public partial class FnCompileDisbursementDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPILE_DISBURSEMENT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCompileDisbursementDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCompileDisbursementDetail.
  /// </summary>
  public FnCompileDisbursementDetail(IContext context, Import import,
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
    // ---------------------------------------------------------------------------
    // Date 	 Name	           Description
    // 09/09/1997  Newman/Parker  Add disbursement date to disbursement 
    // transaction views and change read from process date to disbursement date
    // 12/9/1998   K Doshi  Replace work view crd_cr_combo_no with work_area in 
    // the group view.
    // 01/20/2000  N.Engoor Replace the Cash_Receipt Receipt_Date being 
    // displayed in the group view with the Cash_Receipt Received_Date.
    // ---------------------------------------------------------------------------
    // 12/03/2014   SWDPLSS - CQ#46048  Increased export_group view from 150 to 
    // 175.
    // ---------------------------------------------------------------------------
    // ***********************************************************
    // The following loop reads thru the ERD and creates the individual 
    // disbursement detail lines in the group view GR_LOCAL_DISB_DETAIL.
    // If any of the desired data can not be extracted for the given 
    // payment_request,the EXECUTION ABORTS since this situation implies a
    // DATABASE INCONSISTENCY
    // ***********************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    local.Group.Index = 0;
    local.Group.Clear();

    foreach(var item in ReadDisbursementTransactionDisbursementType())
    {
      local.DisbDetFoundFlagIn.Flag = "Y";
      MoveDisbursementTransaction(entities.DisbursementTransaction,
        local.Group.Update.GrDisbursementTransaction);

      if (AsChar(entities.DisbursementTransaction.ExcessUraInd) == 'Y')
      {
        local.Group.Update.GrDisbType.Text10 =
          TrimEnd(entities.DisbursementType.Code) + " " + "X";
      }
      else
      {
        local.Group.Update.GrDisbType.Text10 = entities.DisbursementType.Code;
      }

      local.DisbTrnRlnFoundFlagIn.Flag = "";

      if (ReadDisbursementTransactionRln())
      {
        local.DisbTrnRlnFoundFlagIn.Flag = "Y";

        // ****
        // The following READ will cause an ABORT when UNSUCCESSFUL due to the 
        // MANDATORY RELATIONSHIP between the operand Entity Types
        // ****
        if (ReadDisbursementTransaction())
        {
          // *** If Disbursement_Transaction Type is = PASSTHRU, then escape.
          if (AsChar(entities.Collection2.Type1) == 'P')
          {
            local.Group.Update.GrDisbursementTransaction.ReferenceNumber =
              Spaces(DisbursementTransaction.ReferenceNumber_MaxLength);
          }
          else
          {
            local.Group.Update.GrColl.SystemGeneratedIdentifier =
              entities.Collection2.SystemGeneratedIdentifier;

            if (ReadCashReceiptDetailCashReceiptCashReceiptEvent())
            {
              local.Group.Update.GrCashReceipt.ReceivedDate =
                entities.CashReceiptEvent.ReceivedDate;
              local.Group.Update.GrWorkArea.Text14 =
                NumberToString(entities.CashReceipt.SequentialNumber, 7, 9) + "-"
                + NumberToString
                (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
            }
            else
            {
              local.Group.Update.GrCashReceipt.ReceivedDate = null;
              local.Group.Update.GrWorkArea.Text14 = "";
            }
          }
        }
      }

      if (AsChar(local.DisbTrnRlnFoundFlagIn.Flag) != 'Y')
      {
        local.Group.Update.GrDisbursementTransaction.ReferenceNumber =
          "DISB_TRNRLN_NF";
      }

      local.Group.Next();
    }

    if (AsChar(local.DisbDetFoundFlagIn.Flag) != 'Y')
    {
      ExitState = "FN0000_DISB_DETAILS_NF";

      return;
    }

    // ***
    // The following loop combines the detail lines from the local group view 
    // GR_LOCAL_DISB_DETAIL where the disbursement date, disbursement type and
    // cash receipt reference number are the same.The group elements with
    // cash_receipt_reference_no = DISB_TRNRLN_NF are not processed.
    // ***
    local.Common.Count = 0;
    export.Group.Index = -1;

    for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
      local.Group.Index)
    {
      if (!Equal(local.Group.Item.GrDisbursementTransaction.ReferenceNumber,
        "DISB_TRNRLN_NF") && Equal
        (local.Group.Item.GrDisbType.Text10, local.TempLocalDisbType.Text10) &&
        (
          Equal(local.Group.Item.GrDisbursementTransaction.ReferenceNumber,
        local.TempDisbursementTransaction.ReferenceNumber) && Equal
        (local.Group.Item.GrDisbursementTransaction.DisbursementDate,
        local.TempDisbursementTransaction.DisbursementDate) || Equal
        (local.Group.Item.GrDisbursementTransaction.ReferenceNumber,
        local.TempNegative.ReferenceNumber) && Equal
        (local.Group.Item.GrDisbursementTransaction.DisbursementDate,
        local.TempNegative.DisbursementDate)))
      {
        if (local.Group.Item.GrDisbursementTransaction.Amount > 0)
        {
          local.TempDisbursementTransaction.Amount += local.Group.Item.
            GrDisbursementTransaction.Amount;
          local.TempDisbursementTransaction.DisbursementDate =
            local.Group.Item.GrDisbursementTransaction.DisbursementDate;
          local.TempDisbursementTransaction.SystemGeneratedIdentifier =
            local.Group.Item.GrDisbursementTransaction.
              SystemGeneratedIdentifier;
        }
        else
        {
          local.TempNegative.Amount += local.Group.Item.
            GrDisbursementTransaction.Amount;
          local.TempNegative.DisbursementDate =
            local.Group.Item.GrDisbursementTransaction.DisbursementDate;
          local.TempNegative.SystemGeneratedIdentifier =
            local.Group.Item.GrDisbursementTransaction.
              SystemGeneratedIdentifier;
        }
      }
      else
      {
        if (local.Common.Count == 1)
        {
          if (local.TempDisbursementTransaction.Amount > 0)
          {
            if (export.Group.IsFull)
            {
              return;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.GrDisbursementTransaction.Assign(
              local.TempDisbursementTransaction);
            export.Group.Update.GrDisbType.Text10 =
              local.TempLocalDisbType.Text10;
            export.Group.Update.GrCashReceipt.ReceivedDate =
              local.TempCashReceipt.ReceivedDate;
            export.Group.Update.GrWorkArea.Text14 = local.TempWorkArea.Text14;
            export.Group.Update.Coll.SystemGeneratedIdentifier =
              local.TempLocalColl.SystemGeneratedIdentifier;

            // -------------------------
            // Clear temp view of Disb_tran.
            // -------------------------
            local.TempDisbursementTransaction.Assign(local.Null1);
          }

          if (local.TempNegative.Amount < 0)
          {
            if (export.Group.IsFull)
            {
              return;
            }

            ++export.Group.Index;
            export.Group.CheckSize();

            export.Group.Update.GrDisbursementTransaction.Assign(
              local.TempNegative);
            export.Group.Update.GrDisbType.Text10 =
              local.TempLocalDisbType.Text10;
            export.Group.Update.GrCashReceipt.ReceivedDate =
              local.TempCashReceipt.ReceivedDate;
            export.Group.Update.GrWorkArea.Text14 = local.TempWorkArea.Text14;
            export.Group.Update.Coll.SystemGeneratedIdentifier =
              local.TempLocalColl.SystemGeneratedIdentifier;

            // -------------------------
            // Clear temp view of Disb_tran.
            // -------------------------
            local.TempNegative.Assign(local.Null1);
          }
        }

        local.Common.Count = 1;
        local.TempLocalDisbType.Text10 = local.Group.Item.GrDisbType.Text10;

        if (local.Group.Item.GrDisbursementTransaction.Amount > 0)
        {
          local.TempDisbursementTransaction.Assign(
            local.Group.Item.GrDisbursementTransaction);
        }
        else
        {
          local.TempNegative.Assign(local.Group.Item.GrDisbursementTransaction);
        }

        local.TempCashReceipt.ReceivedDate =
          local.Group.Item.GrCashReceipt.ReceivedDate;
        local.TempWorkArea.Text14 = local.Group.Item.GrWorkArea.Text14;
        local.TempLocalColl.SystemGeneratedIdentifier =
          local.Group.Item.GrColl.SystemGeneratedIdentifier;
      }
    }

    // *** Move the disbursement detail values for the last iteration of the 
    // above loop from local_finance_work_area to the group view export_compiled
    // ***
    if (local.Common.Count > 0)
    {
      if (local.TempDisbursementTransaction.Amount > 0)
      {
        if (export.Group.IsFull)
        {
          return;
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GrDisbursementTransaction.Assign(
          local.TempDisbursementTransaction);
        export.Group.Update.GrDisbType.Text10 = local.TempLocalDisbType.Text10;
        export.Group.Update.GrCashReceipt.ReceivedDate =
          local.TempCashReceipt.ReceivedDate;
        export.Group.Update.GrWorkArea.Text14 = local.TempWorkArea.Text14;
        export.Group.Update.Coll.SystemGeneratedIdentifier =
          local.TempLocalColl.SystemGeneratedIdentifier;
      }

      if (local.TempNegative.Amount < 0)
      {
        if (export.Group.IsFull)
        {
          return;
        }

        ++export.Group.Index;
        export.Group.CheckSize();

        export.Group.Update.GrDisbursementTransaction.
          Assign(local.TempNegative);
        export.Group.Update.GrDisbType.Text10 = local.TempLocalDisbType.Text10;
        export.Group.Update.GrCashReceipt.ReceivedDate =
          local.TempCashReceipt.ReceivedDate;
        export.Group.Update.GrWorkArea.Text14 = local.TempWorkArea.Text14;
        export.Group.Update.Coll.SystemGeneratedIdentifier =
          local.TempLocalColl.SystemGeneratedIdentifier;
      }
    }
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.DisbursementDate = source.DisbursementDate;
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.Collection2.Populated);
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crvId", entities.Collection2.CrvId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "cstId", entities.Collection2.CstId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crtId", entities.Collection2.CrtId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crdId", entities.Collection2.CrdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 6);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 7);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 8);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 9);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.Collection2.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaPType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspPNumber);
          
      },
      (db, reader) =>
      {
        entities.Collection2.CpaType = db.GetString(reader, 0);
        entities.Collection2.CspNumber = db.GetString(reader, 1);
        entities.Collection2.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Collection2.Type1 = db.GetString(reader, 3);
        entities.Collection2.Amount = db.GetDecimal(reader, 4);
        entities.Collection2.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Collection2.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Collection2.DisbursementDate = db.GetNullableDate(reader, 7);
        entities.Collection2.CollectionProcessDate = db.GetDate(reader, 8);
        entities.Collection2.OtyId = db.GetNullableInt32(reader, 9);
        entities.Collection2.OtrTypeDisb = db.GetNullableString(reader, 10);
        entities.Collection2.OtrId = db.GetNullableInt32(reader, 11);
        entities.Collection2.CpaTypeDisb = db.GetNullableString(reader, 12);
        entities.Collection2.CspNumberDisb = db.GetNullableString(reader, 13);
        entities.Collection2.ObgId = db.GetNullableInt32(reader, 14);
        entities.Collection2.CrdId = db.GetNullableInt32(reader, 15);
        entities.Collection2.CrvId = db.GetNullableInt32(reader, 16);
        entities.Collection2.CstId = db.GetNullableInt32(reader, 17);
        entities.Collection2.CrtId = db.GetNullableInt32(reader, 18);
        entities.Collection2.ColId = db.GetNullableInt32(reader, 19);
        entities.Collection2.ReferenceNumber = db.GetNullableString(reader, 20);
        entities.Collection2.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Collection2.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Collection2.Type1);
          
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Collection2.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Collection2.CpaTypeDisb);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionDisbursementType()
  {
    return ReadEach("ReadDisbursementTransactionDisbursementType",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (local.Group.IsFull)
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
        entities.DisbursementTransaction.CreatedBy = db.GetString(reader, 6);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 10);
        entities.DisbursementTransaction.CashNonCashInd =
          db.GetString(reader, 11);
        entities.DisbursementTransaction.RecapturedInd =
          db.GetString(reader, 12);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 13);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.DisbursementTransaction.PrqGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 15);
        entities.DisbursementTransaction.ExcessUraInd =
          db.GetNullableString(reader, 16);
        entities.DisbursementType.Code = db.GetString(reader, 17);
        entities.DisbursementTransaction.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.DisbursementTransaction.CashNonCashInd);

        return true;
      });
  }

  private bool ReadDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransactionRln.Populated = false;

    return Read("ReadDisbursementTransactionRln",
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
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.Description =
          db.GetNullableString(reader, 1);
        entities.DisbursementTransactionRln.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 3);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 4);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 5);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 6);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 7);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 8);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 9);
        entities.DisbursementTransactionRln.Populated = true;
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GrDisbType.
      /// </summary>
      [JsonPropertyName("grDisbType")]
      public TextWorkArea GrDisbType
      {
        get => grDisbType ??= new();
        set => grDisbType = value;
      }

      /// <summary>
      /// A value of GrCashReceipt.
      /// </summary>
      [JsonPropertyName("grCashReceipt")]
      public CashReceipt GrCashReceipt
      {
        get => grCashReceipt ??= new();
        set => grCashReceipt = value;
      }

      /// <summary>
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
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
      /// A value of GrDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("grDisbursementTransaction")]
      public DisbursementTransaction GrDisbursementTransaction
      {
        get => grDisbursementTransaction ??= new();
        set => grDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of GrWorkArea.
      /// </summary>
      [JsonPropertyName("grWorkArea")]
      public WorkArea GrWorkArea
      {
        get => grWorkArea ??= new();
        set => grWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 175;

      private TextWorkArea grDisbType;
      private CashReceipt grCashReceipt;
      private Common grCommon;
      private DisbursementTransaction coll;
      private DisbursementTransaction grDisbursementTransaction;
      private WorkArea grWorkArea;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

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

    private Array<GroupGroup> group;
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
      /// A value of GrDisbType.
      /// </summary>
      [JsonPropertyName("grDisbType")]
      public TextWorkArea GrDisbType
      {
        get => grDisbType ??= new();
        set => grDisbType = value;
      }

      /// <summary>
      /// A value of GrCashReceipt.
      /// </summary>
      [JsonPropertyName("grCashReceipt")]
      public CashReceipt GrCashReceipt
      {
        get => grCashReceipt ??= new();
        set => grCashReceipt = value;
      }

      /// <summary>
      /// A value of GrColl.
      /// </summary>
      [JsonPropertyName("grColl")]
      public DisbursementTransaction GrColl
      {
        get => grColl ??= new();
        set => grColl = value;
      }

      /// <summary>
      /// A value of GrDisbursementTransaction.
      /// </summary>
      [JsonPropertyName("grDisbursementTransaction")]
      public DisbursementTransaction GrDisbursementTransaction
      {
        get => grDisbursementTransaction ??= new();
        set => grDisbursementTransaction = value;
      }

      /// <summary>
      /// A value of GrWorkArea.
      /// </summary>
      [JsonPropertyName("grWorkArea")]
      public WorkArea GrWorkArea
      {
        get => grWorkArea ??= new();
        set => grWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private TextWorkArea grDisbType;
      private CashReceipt grCashReceipt;
      private DisbursementTransaction grColl;
      private DisbursementTransaction grDisbursementTransaction;
      private WorkArea grWorkArea;
    }

    /// <summary>
    /// A value of TempLocalDisbType.
    /// </summary>
    [JsonPropertyName("tempLocalDisbType")]
    public TextWorkArea TempLocalDisbType
    {
      get => tempLocalDisbType ??= new();
      set => tempLocalDisbType = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DisbursementTransaction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of TempNegative.
    /// </summary>
    [JsonPropertyName("tempNegative")]
    public DisbursementTransaction TempNegative
    {
      get => tempNegative ??= new();
      set => tempNegative = value;
    }

    /// <summary>
    /// A value of TempLocalColl.
    /// </summary>
    [JsonPropertyName("tempLocalColl")]
    public DisbursementTransaction TempLocalColl
    {
      get => tempLocalColl ??= new();
      set => tempLocalColl = value;
    }

    /// <summary>
    /// A value of TempCashReceipt.
    /// </summary>
    [JsonPropertyName("tempCashReceipt")]
    public CashReceipt TempCashReceipt
    {
      get => tempCashReceipt ??= new();
      set => tempCashReceipt = value;
    }

    /// <summary>
    /// A value of TempWorkArea.
    /// </summary>
    [JsonPropertyName("tempWorkArea")]
    public WorkArea TempWorkArea
    {
      get => tempWorkArea ??= new();
      set => tempWorkArea = value;
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
    /// A value of DisbTrnRlnFoundFlagIn.
    /// </summary>
    [JsonPropertyName("disbTrnRlnFoundFlagIn")]
    public Common DisbTrnRlnFoundFlagIn
    {
      get => disbTrnRlnFoundFlagIn ??= new();
      set => disbTrnRlnFoundFlagIn = value;
    }

    /// <summary>
    /// A value of DisbDetFoundFlagIn.
    /// </summary>
    [JsonPropertyName("disbDetFoundFlagIn")]
    public Common DisbDetFoundFlagIn
    {
      get => disbDetFoundFlagIn ??= new();
      set => disbDetFoundFlagIn = value;
    }

    /// <summary>
    /// A value of TempDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("tempDisbursementTransaction")]
    public DisbursementTransaction TempDisbursementTransaction
    {
      get => tempDisbursementTransaction ??= new();
      set => tempDisbursementTransaction = value;
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

    private TextWorkArea tempLocalDisbType;
    private DisbursementTransaction null1;
    private DisbursementTransaction tempNegative;
    private DisbursementTransaction tempLocalColl;
    private CashReceipt tempCashReceipt;
    private WorkArea tempWorkArea;
    private Common common;
    private Common disbTrnRlnFoundFlagIn;
    private Common disbDetFoundFlagIn;
    private DisbursementTransaction tempDisbursementTransaction;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Collection1.
    /// </summary>
    [JsonPropertyName("collection1")]
    public Collection Collection1
    {
      get => collection1 ??= new();
      set => collection1 = value;
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
    /// A value of Collection2.
    /// </summary>
    [JsonPropertyName("collection2")]
    public DisbursementTransaction Collection2
    {
      get => collection2 ??= new();
      set => collection2 = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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

    private CashReceiptEvent cashReceiptEvent;
    private Collection collection1;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private DisbursementTransaction collection2;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
  }
#endregion
}

// Program: OE_DETERMINE_PAYMENTS_FOR_KDMV_2, ID: 371387652, model: 746.
// Short name: SWE03615
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_DETERMINE_PAYMENTS_FOR_KDMV_2.
/// </summary>
[Serializable]
public partial class OeDeterminePaymentsForKdmv2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_DETERMINE_PAYMENTS_FOR_KDMV_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeDeterminePaymentsForKdmv2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeDeterminePaymentsForKdmv2.
  /// </summary>
  public OeDeterminePaymentsForKdmv2(IContext context, Import import,
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
    // **********************************************************************************************
    // Initial Code       Dwayne Dupree        10/08/2007
    // This is determining if payments were made as defined in the driver's 
    // license restriction process.
    // **********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.From.Date = import.ReadFromDate.Date;
    local.Common.Count = 0;
    local.EndDate.Date = new DateTime(2099, 12, 31);
    local.NullDate.Date = new DateTime(1, 1, 1);
    local.To.Date = import.PaymentDueToDate.Date;

    foreach(var item in ReadCashReceiptDetail())
    {
      local.OkToProcess.Flag = "Y";

      // CRDETAIL_STAT_ID  CODE        NAME
      // ---------+---------+---------+---------+---------+---------
      //                1  REC         RECORDED
      //                2  ADJ         ADJUSTED
      //                3  SUSP        SUSPENDED
      //                4  DIST        DISTRIBUTED
      //                5  REF         REFUNDED
      //                6  REL         RELEASED FOR DISTRIBUTION
      //                7  PEND        PENDED
      //                8  REIPDELETE  REIP DELETE
      foreach(var item1 in ReadCollectionType())
      {
        if (entities.CollectionType.SequentialIdentifier == 1 || entities
          .CollectionType.SequentialIdentifier == 5 || entities
          .CollectionType.SequentialIdentifier == 6)
        {
          if (AsChar(import.ReinstateProcess.Flag) == 'Y' && entities
            .CollectionType.SequentialIdentifier == 5)
          {
            local.OkToProcess.Flag = "N";

            goto ReadEach;
          }
        }
        else
        {
          local.OkToProcess.Flag = "N";

          goto ReadEach;

          // next CASH RECEIPT
        }

        // COLLECTION_TYPE_ID  PRINT_NAME            CODE        NAME
        // ---------+---------+---------+---------+---------+---------+---------
        // +---------
        //                  1  REGULAR COLLECTION    C           REGULAR 
        // COLLECTION
        //                  2  IV-D RECOVERY         D           IV-D RECOVERY
        //                  3  FEDERAL OFFSET        F           FEDERAL OFFSET
        //                  4  STATE OFFSET          S           STATE OFFSET-
        // MISC
        //                  5  STATE OFFSET          U           UNEMPLOYMENT 
        // OFFSET
        //                  6  INCOME WITHHOLDING    I           INCOME 
        // WITHHOLDING
        //                  9  FEE PAYMENT           P           FEE PAYMENT
        //                 10  STATE OFFSET          K           KPERS
        //                 11  STATE OFFSET          R           STATE RECOVERY
        //                 14  DIR PMT AP            4           DIRECT PAYMENT 
        // - AP
        //                 15  COLLECTION AGENCY     A           COLLECTION 
        // AGENCY
        //                 16  BAD CK REC            B           BAD CHECK 
        // RECOVERY
        //                 17  MIS DIR REC           M           MISDIRECTED AR,
        // AP, NON
        //                 18  1040X                 N           1040X RECOVERY
        //                 19  FEDERAL OFFSET        T           TREASURY OFFSET
        //                 20  DIR PMT CT            5           DIRECT PAYMENT 
        // - COURT
        //               21  DIR PMT CRU           6           DIRECT PAYMENT - 
        // CRU
        //               23  VOLUNTARY PAYMENT     V           VOLUNTARY PAYMENT
        //               25  TREASURY OFFSET RET   Y           TREASURY OFFSET -
        // RETIREME
        //               26  TREASURY OFFSET VEN   Z           TREASURY OFFSET -
        // VENDOR
        //               27  CSENET/IRS TAX INTCP  7           CSENET/IRS TAX 
        // INTERCEPT
        //               28  CSENET/ST TAX INTCP   8           CSENET/STATE TAX 
        // INTERCEPT
        //               29  CSENET/UI TAX INTCP   9           CSENET/UI
      }

      if (ReadCashReceiptType())
      {
        if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 7 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 8 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 11)
        {
          local.OkToProcess.Flag = "N";

          continue;

          // CRTYPE_ID  CODE        CATEGORY_IND  NAME
          // ---------+---------+---------+---------+---------+---------+
          // ---------+--------
          //         1  CHECK       C             CHECK
          //         2  FCRT REC    N             COURT RECORD - ENTERED BY 
          // FIELD
          //         3  MNY ORD     C             MONEY ORDER
          //         4  CURRENCY    C             CURRENCY
          //         5  CRDT CRD    C             CREDIT CARD
          //         6  EFT         C             ELECTRONIC FUNDS TRANSFER
          //         7  FDIR PMT    N             DIRECT PAY FROM AP TO AR - REC
          // BY FIELD
          //         8  CSENET      N             CSENET NOTIFIED
          //         9  CRT REC     C             COURT RECORD
          //        10  INTERFUND   C             INTERFUND VOUCHER
          //        11  RDIR PMT    N             DIRECT PAY FROM AP TO AR - REC
          // BY CRU
          //        12  MANINT      C             MANUAL INTERFACE
          // do not want fcrt rec, fdir pmt, rdir pmt , csenet
        }
      }

      if (AsChar(local.OkToProcess.Flag) == 'Y')
      {
        export.AmountPaid.TotalCurrency += entities.CashReceiptDetail.
          CollectionAmount;
      }

ReadEach:
      ;
    }
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetDate(command, "date1", local.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", local.To.Date.GetValueOrDefault());
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetNullableString(
          command, "courtOrderNumber", import.LegalAction.StandardNumber ?? ""
          );
        db.SetNullableDate(
          command, "discontinueDate", local.EndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.Populated = true;

        return true;
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

  private IEnumerable<bool> ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCollectionType",
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
    /// A value of ReinstateProcess.
    /// </summary>
    [JsonPropertyName("reinstateProcess")]
    public Common ReinstateProcess
    {
      get => reinstateProcess ??= new();
      set => reinstateProcess = value;
    }

    /// <summary>
    /// A value of PaymentDueToDate.
    /// </summary>
    [JsonPropertyName("paymentDueToDate")]
    public DateWorkArea PaymentDueToDate
    {
      get => paymentDueToDate ??= new();
      set => paymentDueToDate = value;
    }

    /// <summary>
    /// A value of ReadFromDate.
    /// </summary>
    [JsonPropertyName("readFromDate")]
    public DateWorkArea ReadFromDate
    {
      get => readFromDate ??= new();
      set => readFromDate = value;
    }

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

    private Common reinstateProcess;
    private DateWorkArea paymentDueToDate;
    private DateWorkArea readFromDate;
    private LegalAction legalAction;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AmountPaid.
    /// </summary>
    [JsonPropertyName("amountPaid")]
    public Common AmountPaid
    {
      get => amountPaid ??= new();
      set => amountPaid = value;
    }

    private Common amountPaid;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OkToProcess.
    /// </summary>
    [JsonPropertyName("okToProcess")]
    public Common OkToProcess
    {
      get => okToProcess ??= new();
      set => okToProcess = value;
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
    /// A value of AlreadyDone.
    /// </summary>
    [JsonPropertyName("alreadyDone")]
    public Common AlreadyDone
    {
      get => alreadyDone ??= new();
      set => alreadyDone = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private Common okToProcess;
    private Common common;
    private Common alreadyDone;
    private DateWorkArea from;
    private DateWorkArea to;
    private DateWorkArea endDate;
    private DateWorkArea nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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

    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
  }
#endregion
}

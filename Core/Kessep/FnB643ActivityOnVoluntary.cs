// Program: FN_B643_ACTIVITY_ON_VOLUNTARY, ID: 372850205, model: 746.
// Short name: SWE02595
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_ACTIVITY_ON_VOLUNTARY.
/// </summary>
[Serializable]
public partial class FnB643ActivityOnVoluntary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_ACTIVITY_ON_VOLUNTARY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ActivityOnVoluntary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ActivityOnVoluntary.
  /// </summary>
  public FnB643ActivityOnVoluntary(IContext context, Import import,
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
    export.ActivityFound.Flag = "N";

    // **************************************************************
    // Get payments received during the statement period.
    // Summarize collections coming from the same cash receipt detail.
    // **************************************************************
    foreach(var item in ReadCashReceiptDetailCollectionType())
    {
      if (ReadCollection2())
      {
        export.ActivityFound.Flag = "Y";

        return;
      }
    }

    // **************************************************************
    // Get debt adjustments made during the statement period.
    // **************************************************************
    if (ReadDebtAdjustment())
    {
      export.ActivityFound.Flag = "Y";

      return;
    }

    // **************************************************************
    // Get collection adjustments made during the statement period.
    // **************************************************************
    if (ReadCollection1())
    {
      export.ActivityFound.Flag = "Y";
    }
  }

  private IEnumerable<bool> ReadCashReceiptDetailCollectionType()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CollectionType.Populated = false;

    return ReadEach("ReadCashReceiptDetailCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.StmtEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 5);
        entities.CollectionType.Code = db.GetString(reader, 6);
        entities.CashReceiptDetail.Populated = true;
        entities.CollectionType.Populated = true;

        return true;
      });
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDate(command, "date1", import.StmtBegin.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.StmtEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 21);
        entities.Collection.DistributionMethod = db.GetString(reader, 22);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 23);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 24);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 25);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 26);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 27);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 28);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 29);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 30);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 31);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 32);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 33);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.StmtEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 3);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 4);
        entities.Collection.ConcurrentInd = db.GetString(reader, 5);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 6);
        entities.Collection.CrtType = db.GetInt32(reader, 7);
        entities.Collection.CstId = db.GetInt32(reader, 8);
        entities.Collection.CrvId = db.GetInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 10);
        entities.Collection.ObgId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Collection.Amount = db.GetDecimal(reader, 20);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 21);
        entities.Collection.DistributionMethod = db.GetString(reader, 22);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 23);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 24);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 25);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 26);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 27);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 28);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 29);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 30);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 31);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 32);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 33);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 34);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
      });
  }

  private bool ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "timestamp1",
          import.StmtBegin.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2", import.StmtEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 5);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 6);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 7);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 8);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of StmtBegin.
    /// </summary>
    [JsonPropertyName("stmtBegin")]
    public DateWorkArea StmtBegin
    {
      get => stmtBegin ??= new();
      set => stmtBegin = value;
    }

    /// <summary>
    /// A value of StmtEnd.
    /// </summary>
    [JsonPropertyName("stmtEnd")]
    public DateWorkArea StmtEnd
    {
      get => stmtEnd ??= new();
      set => stmtEnd = value;
    }

    private Obligation obligation;
    private DateWorkArea stmtBegin;
    private DateWorkArea stmtEnd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActivityFound.
    /// </summary>
    [JsonPropertyName("activityFound")]
    public Common ActivityFound
    {
      get => activityFound ??= new();
      set => activityFound = value;
    }

    private Common activityFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    private ObligationTransaction debt;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private ObligationTransaction debtAdjustment;
  }
#endregion
}

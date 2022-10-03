// Program: FN_CAB_REPLICATE_COLLECTIONS, ID: 372275529, model: 746.
// Short name: SWE02357
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_REPLICATE_COLLECTIONS.
/// </summary>
[Serializable]
public partial class FnCabReplicateCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_REPLICATE_COLLECTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReplicateCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReplicateCollections.
  /// </summary>
  public FnCabReplicateCollections(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // DATE      DEVELOPER NAME          DESCRIPTION
    // 02/06/99  elyman                  New action block.
    // 12/01/99  elyman    PR# 81189     Add type of change and process
    //                                   
    // concurrent collections.
    // 06/01/00  elyman    WR# 164-G     Add new attribute to Collection
    //                                   
    // as part of URA PRWORA.
    // 11/30/00 elyman     PR# 108246    New attributes are not being
    //                                   
    // replicated:
    //                                      
    // dist_pgm_state_appld_to
    //                                      
    // ocse34_reporting_period.
    // 12/15/01  Ed Lyman   WR#010504    Added logic to bypass replicating 
    // collections
    //                                   
    // that have a distribution method of
    // 'P' for
    //                                   
    // protected collection.
    // ----------------------------------------------------------------
    // *****************************************************************
    // This action block is called by procedure:
    //   SWEFB668 - Process Case Role Changes
    // *****************************************************************
    export.NoOfIncrementalUpdates.Count = import.NoOfIncrementalUpdates.Count;
    export.NoCollectionsReplicated.Count = import.NoCollectionsReplicated.Count;
    local.JointAlreadyProcessed.Flag = "N";
    local.TypeOfChange.SelectChar = "R";

    if (!ReadCollectionAdjustmentReason())
    {
      ExitState = "FN0000_COLL_ADJUST_REASON_NF";

      return;
    }

    // ****  Main Logic ****
    local.CollRevThisCrd.Count = 0;

    if (ReadCsePersonAccount())
    {
      // ***  In  the following logic, we are using the timestamp to make sure
      // ***  that we do not read a collection that we have just replicated.
      foreach(var item in ReadCollectionDebtDebtDetail())
      {
        if (AsChar(entities.Collection.DistributionMethod) == 'P')
        {
          // ***  P = PROTECTED COLLECTION ***
          continue;
        }

        ++export.NoCollectionsReplicated.Count;
        export.NoOfIncrementalUpdates.Count += 3;
        UseFnCabReplicateOneCollection();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
        else
        {
          ++local.CollRevThisCrd.Count;
        }
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseFnCabReplicateOneCollection()
  {
    var useImport = new FnCabReplicateOneCollection.Import();
    var useExport = new FnCabReplicateOneCollection.Export();

    useImport.Pers.Assign(entities.Collection);
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      import.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(import.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveCollection(import.Collection, useImport.Collection);
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.ReportNeeded.Flag = import.ReportNeeded.Flag;

    Call(FnCabReplicateOneCollection.Execute, useImport, useExport);

    entities.Collection.Assign(useImport.Pers);
  }

  private bool ReadCollectionAdjustmentReason()
  {
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          import.CollectionAdjustmentReason.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollectionDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.Collection.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadCollectionDebtDebtDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cpaSupType", entities.CsePersonAccount.Type1);
        db.SetNullableString(
          command, "cspSupNumber", entities.CsePersonAccount.CspNumber);
        db.SetDateTime(
          command, "createdTmst",
          import.ProgramStart.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "disbDt", local.Zero.Date.GetValueOrDefault());
        db.SetDate(
          command, "collDt",
          import.Collection.CollectionAdjustmentDt.GetValueOrDefault());
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
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.Collection.CspNumber = db.GetString(reader, 12);
        entities.Debt.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.Collection.CpaType = db.GetString(reader, 13);
        entities.Debt.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.Collection.OtrId = db.GetInt32(reader, 14);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 14);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.Collection.OtrType = db.GetString(reader, 15);
        entities.Debt.Type1 = db.GetString(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 15);
        entities.Collection.OtyId = db.GetInt32(reader, 16);
        entities.Debt.OtyType = db.GetInt32(reader, 16);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 16);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 17);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 18);
        entities.Collection.CreatedBy = db.GetString(reader, 19);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 20);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 21);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 22);
        entities.Collection.Amount = db.GetDecimal(reader, 23);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 24);
        entities.Collection.DistributionMethod = db.GetString(reader, 25);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 26);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 27);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 28);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 29);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 30);
        entities.Collection.Ocse34ReportingPeriod =
          db.GetNullableDate(reader, 31);
        entities.Collection.BalForIntCompBefColl =
          db.GetNullableDecimal(reader, 32);
        entities.Collection.CumIntChargedUptoColl =
          db.GetNullableDecimal(reader, 33);
        entities.Collection.CumIntCollAfterThisColl =
          db.GetNullableDecimal(reader, 34);
        entities.Collection.IntBalAftThisColl =
          db.GetNullableDecimal(reader, 35);
        entities.Collection.DisburseToArInd = db.GetNullableString(reader, 36);
        entities.Collection.ManualDistributionReasonText =
          db.GetNullableString(reader, 37);
        entities.Collection.CollectionAdjustmentReasonTxt =
          db.GetNullableString(reader, 38);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 39);
        entities.Collection.AppliedToFuture = db.GetString(reader, 40);
        entities.Collection.CsenetOutboundReqInd = db.GetString(reader, 41);
        entities.Collection.CsenetOutboundProcDt =
          db.GetNullableDate(reader, 42);
        entities.Collection.CsenetOutboundAdjProjDt =
          db.GetNullableDate(reader, 43);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 44);
        entities.Collection.DistPgmStateAppldTo =
          db.GetNullableString(reader, 45);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 46);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 47);
        entities.DebtDetail.DueDt = db.GetDate(reader, 48);
        entities.Collection.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("DistributionMethod",
          entities.Collection.DistributionMethod);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<Collection>("AppliedToFuture",
          entities.Collection.AppliedToFuture);
        CheckValid<Collection>("CsenetOutboundReqInd",
          entities.Collection.CsenetOutboundReqInd);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Supported.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
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
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of NoCollectionsReplicated.
    /// </summary>
    [JsonPropertyName("noCollectionsReplicated")]
    public Common NoCollectionsReplicated
    {
      get => noCollectionsReplicated ??= new();
      set => noCollectionsReplicated = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private DateWorkArea programStart;
    private CsePerson obligee;
    private CsePerson supported;
    private Common noOfIncrementalUpdates;
    private Common noCollectionsReplicated;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private ProgramProcessingInfo programProcessingInfo;
    private Common reportNeeded;
    private Collection collection;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of NoCollectionsReplicated.
    /// </summary>
    [JsonPropertyName("noCollectionsReplicated")]
    public Common NoCollectionsReplicated
    {
      get => noCollectionsReplicated ??= new();
      set => noCollectionsReplicated = value;
    }

    private Common noOfIncrementalUpdates;
    private Common noCollectionsReplicated;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of JointAlreadyProcessed.
    /// </summary>
    [JsonPropertyName("jointAlreadyProcessed")]
    public Common JointAlreadyProcessed
    {
      get => jointAlreadyProcessed ??= new();
      set => jointAlreadyProcessed = value;
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
    /// A value of Revised.
    /// </summary>
    [JsonPropertyName("revised")]
    public CashReceiptDetail Revised
    {
      get => revised ??= new();
      set => revised = value;
    }

    /// <summary>
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of Refunded.
    /// </summary>
    [JsonPropertyName("refunded")]
    public EabConvertNumeric2 Refunded
    {
      get => refunded ??= new();
      set => refunded = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public EabConvertNumeric2 Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of Receipt.
    /// </summary>
    [JsonPropertyName("receipt")]
    public EabConvertNumeric2 Receipt
    {
      get => receipt ??= new();
      set => receipt = value;
    }

    /// <summary>
    /// A value of Distributed.
    /// </summary>
    [JsonPropertyName("distributed")]
    public EabConvertNumeric2 Distributed
    {
      get => distributed ??= new();
      set => distributed = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of CollRevThisCrd.
    /// </summary>
    [JsonPropertyName("collRevThisCrd")]
    public Common CollRevThisCrd
    {
      get => collRevThisCrd ??= new();
      set => collRevThisCrd = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public EabConvertNumeric2 Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of OtherObligor.
    /// </summary>
    [JsonPropertyName("otherObligor")]
    public CashReceiptDetail OtherObligor
    {
      get => otherObligor ??= new();
      set => otherObligor = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private Common typeOfChange;
    private Common jointAlreadyProcessed;
    private DateWorkArea null1;
    private CashReceiptDetail revised;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabConvertNumeric2 refunded;
    private EabConvertNumeric2 clear;
    private EabConvertNumeric2 receipt;
    private EabConvertNumeric2 distributed;
    private TextWorkArea formatDate;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common collRevThisCrd;
    private EabConvertNumeric2 collection;
    private CashReceiptDetail otherObligor;
    private DateWorkArea zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private CsePersonAccount csePersonAccount;
  }
#endregion
}

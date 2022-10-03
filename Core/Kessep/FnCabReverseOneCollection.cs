// Program: FN_CAB_REVERSE_ONE_COLLECTION, ID: 372265481, model: 746.
// Short name: SWE02347
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_REVERSE_ONE_COLLECTION.
/// </summary>
[Serializable]
public partial class FnCabReverseOneCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_REVERSE_ONE_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReverseOneCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReverseOneCollection.
  /// </summary>
  public FnCabReverseOneCollection(IContext context, Import import,
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
    // ************************************************************************
    //   Date      Programmer   Problem #   Description
    // ----------  ----------   ---------
    // 
    // ----------------------------------
    // 10/27/1999  E. Lyman     H00A74221   Don't set Disbursement Processing 
    // Need
    //                                      
    // Ind to 'N' on Collection.
    // 04/18/2000  E. Lyman     00090886    Remove coding that determines if 
    // person
    //                                      
    // program has changed before reversing
    //                                      
    // the collection.
    // 05/17/2000  E. Shirk    PRWORA     Added call to fn_update_ura_amount 
    // when collection was reversed.
    // ************************************************************************
    local.Timestamp.Timestamp = Now();

    if (ReadDebtDetailDebtDetailStatusHistoryDebt())
    {
      local.DebtDetail.Assign(entities.DebtDetail);

      if (AsChar(entities.DebtDetailStatusHistory.Code) != 'A')
      {
        local.DebtDetailStatusHistory.Code = "A";
        local.DebtDetailStatusHistory.CreatedBy =
          import.ProgramProcessingInfo.Name;
        local.DebtDetailStatusHistory.EffectiveDt =
          import.ProgramProcessingInfo.ProcessDate;
        local.DebtDetailStatusHistory.ReasonTxt =
          import.Collection.CollectionAdjustmentReasonTxt ?? "";
        UseFnCabUpdateDebtDtlStatus();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      if (AsChar(import.Pers.AppliedToCode) == 'I')
      {
        // *** If the collection was applied to interest, back off the 
        // collection amount from the interest balance ***
        local.DebtDetail.InterestBalanceDueAmt =
          local.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault() + import
          .Pers.Amount;
      }
      else
      {
        // *** If the collection were applied to the principal, back off the 
        // collection amount from the debt balance amount ***
        local.DebtDetail.BalanceDueAmt += import.Pers.Amount;
      }

      local.DebtDetail.RetiredDt = local.Null1.Date;
      local.DebtDetail.LastUpdatedBy = import.ProgramProcessingInfo.Name;
      local.DebtDetail.LastUpdatedTmst = local.Timestamp.Timestamp;
      UseFnCabUpdateDebtDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      try
      {
        UpdateCollection();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_COLLECTION_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_COLLECTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // ***************************************************************************************
      // Only update ura amount if for primary collection, and a FC or AF
      // program.
      // **************************************************************************************
      if (AsChar(import.Pers.ConcurrentInd) != 'Y')
      {
        if (Equal(import.Pers.ProgramAppliedTo, "FC") || Equal
          (import.Pers.ProgramAppliedTo, "AF"))
        {
          // ***************************************************************************************
          // Get AP number
          // **************************************************************************************
          if (!ReadCsePerson())
          {
            ExitState = "CSE_PERSON_NF";

            return;
          }

          // ***************************************************************************************
          // Call update URA amount action block.
          // **************************************************************************************
          UseFnUpdateUraAmount();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
          }
        }
      }
    }
    else
    {
      ExitState = "FN0229_DEBT_NF";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.RetiredDt = source.RetiredDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseFnCabUpdateDebtDetail()
  {
    var useImport = new FnCabUpdateDebtDetail.Import();
    var useExport = new FnCabUpdateDebtDetail.Export();

    useImport.Persistent.Assign(entities.DebtDetail);
    useImport.DebtDetail.Assign(local.DebtDetail);

    Call(FnCabUpdateDebtDetail.Execute, useImport, useExport);

    MoveDebtDetail(useImport.Persistent, entities.DebtDetail);
  }

  private void UseFnCabUpdateDebtDtlStatus()
  {
    var useImport = new FnCabUpdateDebtDtlStatus.Import();
    var useExport = new FnCabUpdateDebtDtlStatus.Export();

    useImport.Persistent.Assign(entities.DebtDetail);
    useImport.DebtDetailStatusHistory.Assign(local.DebtDetailStatusHistory);
    useImport.Max.Date = import.Max.Date;
    useImport.Current.Timestamp = local.Timestamp.Timestamp;

    Call(FnCabUpdateDebtDtlStatus.Execute, useImport, useExport);

    MoveDebtDetail(useImport.Persistent, entities.DebtDetail);
  }

  private void UseFnUpdateUraAmount()
  {
    var useImport = new FnUpdateUraAmount.Import();
    var useExport = new FnUpdateUraAmount.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    MoveCollection(import.Pers, useImport.Collection);

    Call(FnUpdateUraAmount.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Pers.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadDebtDetailDebtDetailStatusHistoryDebt()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);
    entities.DebtDetailStatusHistory.Populated = false;
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return Read("ReadDebtDetailDebtDetailStatusHistoryDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Pers.OtyId);
        db.SetString(command, "obTrnTyp", import.Pers.OtrType);
        db.SetInt32(command, "obTrnId", import.Pers.OtrId);
        db.SetString(command, "cpaType", import.Pers.CpaType);
        db.SetString(command, "cspNumber", import.Pers.CspNumber);
        db.SetInt32(command, "obgGeneratedId", import.Pers.ObgId);
        db.SetNullableDate(
          command, "discontinueDt", import.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 14);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 17);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 18);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 19);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 20);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 21);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 22);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 23);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 24);
        entities.DebtDetailStatusHistory.Populated = true;
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.Pers.Populated);

    var adjustedInd = "Y";
    var carId = import.Persistent.SystemGeneratedIdentifier;
    var collectionAdjustmentDt = import.Collection.CollectionAdjustmentDt;
    var collectionAdjProcessDate = import.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = local.Timestamp.Timestamp;
    var collectionAdjustmentReasonTxt =
      import.Collection.CollectionAdjustmentReasonTxt ?? "";

    CheckValid<Collection>("AdjustedInd", adjustedInd);
    import.Pers.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableString(command, "adjInd", adjustedInd);
        db.SetNullableInt32(command, "carId", carId);
        db.SetDate(command, "collAdjDt", collectionAdjustmentDt);
        db.SetDate(command, "collAdjProcDate", collectionAdjProcessDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(
          command, "colAdjRsnTxt", collectionAdjustmentReasonTxt);
        db.SetInt32(command, "collId", import.Pers.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.Pers.CrtType);
        db.SetInt32(command, "cstId", import.Pers.CstId);
        db.SetInt32(command, "crvId", import.Pers.CrvId);
        db.SetInt32(command, "crdId", import.Pers.CrdId);
        db.SetInt32(command, "obgId", import.Pers.ObgId);
        db.SetString(command, "cspNumber", import.Pers.CspNumber);
        db.SetString(command, "cpaType", import.Pers.CpaType);
        db.SetInt32(command, "otrId", import.Pers.OtrId);
        db.SetString(command, "otrType", import.Pers.OtrType);
        db.SetInt32(command, "otyId", import.Pers.OtyId);
      });

    import.Pers.AdjustedInd = adjustedInd;
    import.Pers.CarId = carId;
    import.Pers.CollectionAdjustmentDt = collectionAdjustmentDt;
    import.Pers.CollectionAdjProcessDate = collectionAdjProcessDate;
    import.Pers.LastUpdatedBy = lastUpdatedBy;
    import.Pers.LastUpdatedTmst = lastUpdatedTmst;
    import.Pers.CollectionAdjustmentReasonTxt = collectionAdjustmentReasonTxt;
    import.Pers.Populated = true;
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
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CollectionAdjustmentReason Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public Collection Pers
    {
      get => pers ??= new();
      set => pers = value;
    }

    private Common typeOfChange;
    private CollectionAdjustmentReason persistent;
    private Collection collection;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private Collection pers;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Timestamp.
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateWorkArea Timestamp
    {
      get => timestamp ??= new();
      set => timestamp = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private DateWorkArea collection;
    private Program program;
    private DateWorkArea timestamp;
    private DateWorkArea null1;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private ObligationTransaction obligationTransaction;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson client;
    private CsePersonAccount supported;
  }
#endregion
}

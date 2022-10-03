// Program: FN_UPDATE_RECOVERY_OBLIGATION, ID: 372159458, model: 746.
// Short name: SWE00675
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_RECOVERY_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block updates a Recovery or Fee Obligation.
/// Attributes updated:
///  - Obligation Description
///  - Obligation Transaction Amount
///  - Debt Detail Amount and Due Date
/// The update will only occur on an Obligation that is not yet active.  For 
/// Recovery/Fees, this means there will be only one occurrence of Obligation
/// Transaction/Debt Detail.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateRecoveryObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_RECOVERY_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateRecoveryObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateRecoveryObligation.
  /// </summary>
  public FnUpdateRecoveryObligation(IContext context, Import import,
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
    // =================================================
    // 2/19/1999 - bud adams  -  removed Read of CSE_Person and
    //   related persistent view logic.
    // 11/4/99 - b adams  -  PR# 78500: see note below.
    // =================================================
    if (ReadObligation())
    {
      // =================================================
      // 4/6/99 - bud adams  -  Business rules allow a debt to be
      //   updated only on the day it was created.  This check is no
      //   longer required.  FN_Check_Obligation_For_Activity gone.
      // =================================================
      local.BeforeUpdate.Assign(entities.Obligation);

      // ***  9-18-98   B Adams    order-type-code is not allowed to be
      // ***     updated IAW business rules  dated 9/17/98  from Marilyn.
      try
      {
        UpdateObligation();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGATION_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGATION_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================================
    // 2/25/1999 - Bud Adams  -  Removed interstate request CRUD
    //   because it was wrong.  Put a CAB that does this into the PrAD
    // =================================================
    // =================================================
    // 4/6/99 - B Adams  -  No longer a valid situation.  Check on
    //   active obligation obsolete requirement
    // =================================================
    // : We only expect one Obligation Transaction and Debt Detail for a
    //   Recovery or Fee Obligation that is not yet active.
    if (ReadObligationTransaction())
    {
      try
      {
        UpdateObligationTransaction();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIG_TRANS_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIG_TRANS_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIG_TRANS_NF_RB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // =================================================
    // 1/5/99 - b adams  -  PR# xxxxx: Cannot update the Debt_
    //   Detail Balance_Due_Amt if the creation date has passed,
    //   if debt_adjustments exist, or if collections have already
    //   been applied.
    //   This value will be Y if the obligation was created today and
    //   if there is no debt adjustment.
    // =================================================
    local.UpdateBalanceDueAmt.Flag = import.UpdateBalanceDueAmt.Flag;

    if (AsChar(local.UpdateBalanceDueAmt.Flag) == 'Y')
    {
      // =================================================
      // Probably won't be any, but there may be some.  We don't
      // care about the data, only if any exists.
      // =================================================
      if (ReadCollection())
      {
        local.UpdateBalanceDueAmt.Flag = "N";
      }
    }

    if (ReadDebtDetail())
    {
      // =================================================
      // 11/4/99 - b adams  -  PR# 78500: Balance_Due_Amt can be
      //   updated on OREC ONLY if this is the day the obligation
      //   was created AND if no debt adjustments have been made
      //   yet.  Otherwise, no.
      // =================================================
      if (AsChar(local.UpdateBalanceDueAmt.Flag) == 'N')
      {
        local.DebtDetail.BalanceDueAmt = entities.DebtDetail.BalanceDueAmt;
      }
      else
      {
        local.DebtDetail.BalanceDueAmt = import.ObligationTransaction.Amount;
      }

      try
      {
        UpdateDebtDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0214_DEBT_DETAIL_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0218_DEBT_DETAIL_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_DEBT_DETAIL_NF_RB";
    }
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgId", entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CrtType = db.GetInt32(reader, 1);
        entities.Collection.CstId = db.GetInt32(reader, 2);
        entities.Collection.CrvId = db.GetInt32(reader, 3);
        entities.Collection.CrdId = db.GetInt32(reader, 4);
        entities.Collection.ObgId = db.GetInt32(reader, 5);
        entities.Collection.CspNumber = db.GetString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 7);
        entities.Collection.OtrId = db.GetInt32(reader, 8);
        entities.Collection.OtrType = db.GetString(reader, 9);
        entities.Collection.OtyId = db.GetInt32(reader, 10);
        entities.Collection.Populated = true;
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cpaType", import.HardcodedObligor.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 4);
        entities.Obligation.Description = db.GetNullableString(reader, 5);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 7);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 8);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationTransaction.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetInt32(
          command, "obTrnId",
          import.ObligationTransaction.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
      });
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var dueDt = import.DebtDetail.DueDt;
    var balanceDueAmt = local.DebtDetail.BalanceDueAmt;
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetDate(command, "dueDt", dueDt);
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(
          command, "obgGeneratedId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(
          command, "otrGeneratedId", entities.DebtDetail.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      });

    entities.DebtDetail.DueDt = dueDt;
    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
  }

  private void UpdateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otherStateAbbr = import.Obligation.OtherStateAbbr ?? "";
    var description = import.Obligation.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;
    var orderTypeCode = import.Obligation.OrderTypeCode;

    CheckValid<Obligation>("OrderTypeCode", orderTypeCode);
    entities.Obligation.Populated = false;
    Update("UpdateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateAbbr", otherStateAbbr);
        db.SetNullableString(command, "obDsc", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "ordTypCd", orderTypeCode);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.OtherStateAbbr = otherStateAbbr;
    entities.Obligation.Description = description;
    entities.Obligation.LastUpdatedBy = lastUpdatedBy;
    entities.Obligation.LastUpdateTmst = lastUpdateTmst;
    entities.Obligation.OrderTypeCode = orderTypeCode;
    entities.Obligation.Populated = true;
  }

  private void UpdateObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);

    var amount = import.ObligationTransaction.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;

    entities.ObligationTransaction.Populated = false;
    Update("UpdateObligationTransaction",
      (db, command) =>
      {
        db.SetDecimal(command, "obTrnAmt", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetInt32(
          command, "obTrnId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
      });

    entities.ObligationTransaction.Amount = amount;
    entities.ObligationTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.ObligationTransaction.Populated = true;
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
    /// A value of UpdateBalanceDueAmt.
    /// </summary>
    [JsonPropertyName("updateBalanceDueAmt")]
    public Common UpdateBalanceDueAmt
    {
      get => updateBalanceDueAmt ??= new();
      set => updateBalanceDueAmt = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HcOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hcOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HcOtrrConcurrentObliga
    {
      get => hcOtrrConcurrentObliga ??= new();
      set => hcOtrrConcurrentObliga = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of HardcodedObligor.
    /// </summary>
    [JsonPropertyName("hardcodedObligor")]
    public CsePersonAccount HardcodedObligor
    {
      get => hardcodedObligor ??= new();
      set => hardcodedObligor = value;
    }

    private Common updateBalanceDueAmt;
    private CsePersonAccount hcCpaObligor;
    private ObligationTransactionRlnRsn hcOtrrConcurrentObliga;
    private DateWorkArea current;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson csePerson;
    private CsePersonAccount hardcodedObligor;
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
    /// A value of UpdateBalanceDueAmt.
    /// </summary>
    [JsonPropertyName("updateBalanceDueAmt")]
    public Common UpdateBalanceDueAmt
    {
      get => updateBalanceDueAmt ??= new();
      set => updateBalanceDueAmt = value;
    }

    /// <summary>
    /// A value of BeforeUpdate.
    /// </summary>
    [JsonPropertyName("beforeUpdate")]
    public Obligation BeforeUpdate
    {
      get => beforeUpdate ??= new();
      set => beforeUpdate = value;
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

    private Common updateBalanceDueAmt;
    private Obligation beforeUpdate;
    private DebtDetail debtDetail;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private Collection collection;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}

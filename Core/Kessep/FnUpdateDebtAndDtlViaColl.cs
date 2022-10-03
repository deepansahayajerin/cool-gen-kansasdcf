// Program: FN_UPDATE_DEBT_AND_DTL_VIA_COLL, ID: 372258437, model: 746.
// Short name: SWE00643
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_DEBT_AND_DTL_VIA_COLL.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block updates an occurrence of the Debt subtype of the 
/// Obligation Transaction entity and the associated Debt Detail entity.  The
/// persistent collection allows the debt information to be updated without
/// explicitly knowing all of the associated obligation identifiers.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateDebtAndDtlViaColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DEBT_AND_DTL_VIA_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDebtAndDtlViaColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDebtAndDtlViaColl.
  /// </summary>
  public FnUpdateDebtAndDtlViaColl(IContext context, Import import,
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
    // 4/7/99 - bud adams  -  cleaned up views; deleted extraneous
    //   logic, moves, escapes, etc.  Read property set.  Remove
    //   SETs from UPDATE that the using CAB does not provide.
    // =================================================
    if (!ReadDebt())
    {
      ExitState = "FN0229_DEBT_NF";

      return;
    }

    if (ReadDebtDetail())
    {
      try
      {
        UpdateDebtDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0213_DEBT_DETAIL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0217_DEBT_DETAIL_PV";

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
      ExitState = "FN0211_DEBT_DETAIL_NF";
    }
  }

  private bool ReadDebt()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Persistent.OtyId);
        db.SetString(command, "obTrnTyp", import.Persistent.OtrType);
        db.SetInt32(command, "obTrnId", import.Persistent.OtrId);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetInt32(command, "obgGeneratedId", import.Persistent.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.LastUpdatedTmst = db.GetNullableDateTime(reader, 9);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);

    var balanceDueAmt = import.DebtDetail.BalanceDueAmt;
    var interestBalanceDueAmt =
      import.DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
    var retiredDt = import.DebtDetail.RetiredDt;
    var lastUpdatedTmst = import.Current.Timestamp;
    var lastUpdatedBy = global.UserId;

    entities.DebtDetail.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetDecimal(command, "balDueAmt", balanceDueAmt);
        db.SetNullableDecimal(command, "intBalDueAmt", interestBalanceDueAmt);
        db.SetNullableDate(command, "retiredDt", retiredDt);
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

    entities.DebtDetail.BalanceDueAmt = balanceDueAmt;
    entities.DebtDetail.InterestBalanceDueAmt = interestBalanceDueAmt;
    entities.DebtDetail.RetiredDt = retiredDt;
    entities.DebtDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.DebtDetail.LastUpdatedBy = lastUpdatedBy;
    entities.DebtDetail.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Collection Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private Collection persistent;
    private DebtDetail debtDetail;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
  }
#endregion
}

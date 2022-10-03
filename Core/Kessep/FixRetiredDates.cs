// Program: FIX_RETIRED_DATES, ID: 372888371, model: 746.
// Short name: FIXRETI1
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FIX_RETIRED_DATES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FixRetiredDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FIX_RETIRED_DATES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FixRetiredDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FixRetiredDates.
  /// </summary>
  public FixRetiredDates(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.DateWorkArea.Timestamp = Now();

    foreach(var item in ReadObligationTransactionObligationTransactionRln())
    {
      if (entities.Debt.SystemGeneratedIdentifier == local
        .ObligationTransaction.SystemGeneratedIdentifier)
      {
        continue;
      }

      local.ObligationTransaction.SystemGeneratedIdentifier =
        entities.Debt.SystemGeneratedIdentifier;
      ReadObligationTransaction();

      if (!entities.Adjustment.Populated)
      {
        continue;
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
              break;
            case ErrorCode.PermittedValueViolation:
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

        return;
      }
    }
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Update.Populated = false;

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
        entities.Update.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Update.CspNumber = db.GetString(reader, 1);
        entities.Update.CpaType = db.GetString(reader, 2);
        entities.Update.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Update.OtyType = db.GetInt32(reader, 4);
        entities.Update.OtrType = db.GetString(reader, 5);
        entities.Update.RetiredDt = db.GetNullableDate(reader, 6);
        entities.Update.LastUpdatedTmst = db.GetNullableDateTime(reader, 7);
        entities.Update.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.Update.Populated = true;
      });
  }

  private bool ReadObligationTransaction()
  {
    System.Diagnostics.Debug.
      Assert(entities.ObligationTransactionRln.Populated);
    entities.Adjustment.Populated = false;

    return Read("ReadObligationTransaction",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ObligationTransactionRln.OtyTypeSecondary);
        db.SetString(
          command, "obTrnTyp", entities.ObligationTransactionRln.OtrType);
        db.SetInt32(
          command, "obTrnId", entities.ObligationTransactionRln.OtrGeneratedId);
          
        db.SetString(
          command, "cpaType", entities.ObligationTransactionRln.CpaType);
        db.SetString(
          command, "cspNumber", entities.ObligationTransactionRln.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransactionRln.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Adjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Adjustment.CspNumber = db.GetString(reader, 1);
        entities.Adjustment.CpaType = db.GetString(reader, 2);
        entities.Adjustment.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Adjustment.Type1 = db.GetString(reader, 4);
        entities.Adjustment.DebtAdjustmentDt = db.GetDate(reader, 5);
        entities.Adjustment.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Adjustment.CpaSupType = db.GetNullableString(reader, 7);
        entities.Adjustment.OtyType = db.GetInt32(reader, 8);
        entities.Adjustment.Populated = true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionObligationTransactionRln()
  {
    entities.CsePersonAccount.Populated = false;
    entities.Obligation.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Debt.Populated = false;
    entities.ObligationTransactionRln.Populated = false;
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadObligationTransactionObligationTransactionRln",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.DateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransactionRln.ObgPGeneratedId =
          db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransactionRln.CspPNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.ObligationTransactionRln.CpaPType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.ObligationTransactionRln.OtrPGeneratedId =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.ObligationTransactionRln.OtrPType = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.OtyTypePrimary =
          db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationTransactionRln.OnrGeneratedId =
          db.GetInt32(reader, 8);
        entities.ObligationTransactionRln.OtrType = db.GetString(reader, 9);
        entities.ObligationTransactionRln.OtrGeneratedId =
          db.GetInt32(reader, 10);
        entities.ObligationTransactionRln.CpaType = db.GetString(reader, 11);
        entities.ObligationTransactionRln.CspNumber = db.GetString(reader, 12);
        entities.ObligationTransactionRln.ObgGeneratedId =
          db.GetInt32(reader, 13);
        entities.ObligationTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.ObligationTransactionRln.CreatedTmst =
          db.GetDateTime(reader, 15);
        entities.ObligationTransactionRln.OtyTypeSecondary =
          db.GetInt32(reader, 16);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 17);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 18);
        entities.CsePersonAccount.Populated = true;
        entities.Obligation.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Debt.Populated = true;
        entities.ObligationTransactionRln.Populated = true;
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private void UpdateDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Update.Populated);

    var retiredDt = entities.Adjustment.DebtAdjustmentDt;
    var lastUpdatedTmst = local.DateWorkArea.Timestamp;
    var lastUpdatedBy = "RETIREDT";

    entities.Update.Populated = false;
    Update("UpdateDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(command, "retiredDt", retiredDt);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetInt32(command, "obgGeneratedId", entities.Update.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Update.CspNumber);
        db.SetString(command, "cpaType", entities.Update.CpaType);
        db.SetInt32(command, "otrGeneratedId", entities.Update.OtrGeneratedId);
        db.SetInt32(command, "otyType", entities.Update.OtyType);
        db.SetString(command, "otrType", entities.Update.OtrType);
      });

    entities.Update.RetiredDt = retiredDt;
    entities.Update.LastUpdatedTmst = lastUpdatedTmst;
    entities.Update.LastUpdatedBy = lastUpdatedBy;
    entities.Update.Populated = true;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private ObligationTransaction obligationTransaction;
    private DateWorkArea dateWorkArea;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public DebtDetail Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public ObligationTransaction Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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

    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private CsePerson csePerson;
    private DebtDetail update;
    private ObligationTransaction adjustment;
    private ObligationTransaction debt;
    private ObligationTransactionRln obligationTransactionRln;
    private DebtDetail debtDetail;
  }
#endregion
}

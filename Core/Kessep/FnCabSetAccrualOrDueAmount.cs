// Program: FN_CAB_SET_ACCRUAL_OR_DUE_AMOUNT, ID: 371737118, model: 746.
// Short name: SWE01600
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
/// A program: FN_CAB_SET_ACCRUAL_OR_DUE_AMOUNT.
/// </para>
/// <para>
/// Resp: Finance
/// This CAB will accumulate totals from Obligation Transactions based on 
/// whether or not the obligation type is accruing or non-accruing.
/// _______________________________________________
/// If accruing, it is the summary of all OBLIGATION_TRANSACTION Amt's for each 
/// Obligation_Transaction for the Obligation IF there are no
/// Accrual_Suspensions for the Obligation_Transaction for the current date.  If
/// there are, then the Obligation_Transaction Amt's need to be reduced by the
/// Accrual_Suspension Amt.  Additionally, only those Obligation_Transactions
/// that are currently in effect(that the current date falls within
/// Accrual_Instructions AsOfDte and Discontinue Dte) are to be included.
/// _______________________________________________
/// IF it is NOT an accrual Obligation_Type, then the summary of all 
/// Obligation_Transaction Amt's where the associated Debt_Detail Retire Date =
/// Null_Date will be the Due Amt.
/// _______________________________________________
/// </para>
/// </summary>
[Serializable]
public partial class FnCabSetAccrualOrDueAmount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_SET_ACCRUAL_OR_DUE_AMOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabSetAccrualOrDueAmount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabSetAccrualOrDueAmount.
  /// </summary>
  public FnCabSetAccrualOrDueAmount(IContext context, Import import,
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
    // ------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // 07/23/96   R. Welborn - MTW			Initial code.
    // 05/01/97   A.Kinney				Changed Current_Date
    // ------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // *******************************************************************
    //   First, determine whether or not the Obligation Type
    //   Classification is an A, for Accrual, or other.
    // *******************************************************************
    if (ReadObligationType())
    {
      if (AsChar(entities.ObligationType.Classification) != 'A')
      {
        // *******************************************************************
        //   This is a non-accruing obligation.  Therefore, the amount
        //   owed is equal to the sum of all Obligation Transaction Amt's
        //   associated to the Obligation where the associated Debt Detail
        //   Retire_Dte = Null_Date
        // *******************************************************************
        export.Common.TotalCurrency = 0;

        foreach(var item in ReadObligationTransactionDebtDetail())
        {
          if (Lt(export.StartDte.Date, DaysFromADToDate(100)))
          {
            export.StartDte.Date = entities.DebtDetail.DueDt;
          }

          if (Equal(entities.DebtDetail.RetiredDt, local.NullDte.Date))
          {
            export.Common.TotalCurrency += entities.ObligationTransaction.
              Amount;
          }
        }
      }
      else
      {
        // *******************************************************************
        //   This is an accruing obligation.  Therefore, the amount owed is
        //   equal to the sum of all Obligation Transaction Amt's
        //   associated to the Obligation reduced by the suspension percentage,
        //   if any, where the current processing date falls between the
        //   accrual_instructions as_of_dte and discontinue_dte.
        // *******************************************************************
        export.Common.TotalCurrency = 0;

        foreach(var item in ReadObligationTransactionAccrualInstructions())
        {
          if (Lt(export.StartDte.Date, DaysFromADToDate(100)))
          {
            export.StartDte.Date = entities.AccrualInstructions.AsOfDt;
          }

          if (!Lt(local.Current.Date, entities.AccrualInstructions.AsOfDt) && Lt
            (local.Current.Date, entities.AccrualInstructions.DiscontinueDt))
          {
            if (ReadAccrualSuspension())
            {
              export.Common.TotalCurrency =
                entities.ObligationTransaction.Amount - entities
                .ObligationTransaction.Amount * (
                  entities.AccrualSuspension.ReductionPercentage.
                  GetValueOrDefault() / 100);
            }
            else
            {
              export.Common.TotalCurrency =
                entities.ObligationTransaction.Amount + export
                .Common.TotalCurrency;
            }
          }
        }
      }
    }
  }

  private bool ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.
          SetDate(command, "suspendDt", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 4);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 7);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 8);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationTransactionAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 8);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 9);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 10);
        entities.AccrualInstructions.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionDebtDetail()
  {
    entities.DebtDetail.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 6);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of StartDte.
    /// </summary>
    [JsonPropertyName("startDte")]
    public DateWorkArea StartDte
    {
      get => startDte ??= new();
      set => startDte = value;
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

    private DateWorkArea startDte;
    private Common common;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of NullDte.
    /// </summary>
    [JsonPropertyName("nullDte")]
    public DateWorkArea NullDte
    {
      get => nullDte ??= new();
      set => nullDte = value;
    }

    private DateWorkArea current;
    private DateWorkArea nullDte;
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

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private AccrualSuspension accrualSuspension;
    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}

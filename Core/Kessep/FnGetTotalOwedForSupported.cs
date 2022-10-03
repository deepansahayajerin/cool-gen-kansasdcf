// Program: FN_GET_TOTAL_OWED_FOR_SUPPORTED, ID: 374458509, model: 746.
// Short name: SWE03002
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GET_TOTAL_OWED_FOR_SUPPORTED.
/// </summary>
[Serializable]
public partial class FnGetTotalOwedForSupported: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_TOTAL_OWED_FOR_SUPPORTED program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetTotalOwedForSupported(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetTotalOwedForSupported.
  /// </summary>
  public FnGetTotalOwedForSupported(IContext context, Import import,
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
    // *********************** 
    // MAINTENANCE LOG
    // **********************************
    // AUTHOR          DATE  	  CHG REQ#     DESCRIPTION
    // Ed Lyman    10/30/2000    PR # 104991  The total suppport field should 
    // not
    //                                        
    // include accrual.
    // **************************************************************************
    local.DateWorkArea.Date = Now().Date.AddMonths(1);
    local.BeginNextMonth.Year = Year(local.DateWorkArea.Date);
    local.BeginNextMonth.Month = Month(local.DateWorkArea.Date);
    local.BeginNextMonth.Day = 1;
    local.BeginNextMonth.Date = IntToDate(local.BeginNextMonth.Year * 10000 + local
      .BeginNextMonth.Month * 100 + local.BeginNextMonth.Day);

    foreach(var item in ReadDebtDebtDetailObligationObligationType1())
    {
      if (!Lt(entities.DebtDetail.DueDt, local.BeginNextMonth.Date))
      {
        continue;
      }

      if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
        .ObligationType.SystemGeneratedIdentifier == 10 || entities
        .ObligationType.SystemGeneratedIdentifier == 19)
      {
        export.TotMedicalOwed.TotalCurrency =
          export.TotMedicalOwed.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
        .ObligationType.SystemGeneratedIdentifier == 2 || entities
        .ObligationType.SystemGeneratedIdentifier == 11 || entities
        .ObligationType.SystemGeneratedIdentifier == 12 || entities
        .ObligationType.SystemGeneratedIdentifier == 13 || entities
        .ObligationType.SystemGeneratedIdentifier == 14 || entities
        .ObligationType.SystemGeneratedIdentifier == 17 || entities
        .ObligationType.SystemGeneratedIdentifier == 18 || entities
        .ObligationType.SystemGeneratedIdentifier == 20 || entities
        .ObligationType.SystemGeneratedIdentifier == 21 || entities
        .ObligationType.SystemGeneratedIdentifier == 22)
      {
        export.TotAfFcOwed.TotalCurrency = export.TotAfFcOwed.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
    }

    foreach(var item in ReadDebtDebtDetailObligationObligationType2())
    {
      if (!Lt(entities.DebtDetail.DueDt, local.BeginNextMonth.Date))
      {
        continue;
      }

      if (entities.ObligationType.SystemGeneratedIdentifier == 3 || entities
        .ObligationType.SystemGeneratedIdentifier == 10 || entities
        .ObligationType.SystemGeneratedIdentifier == 19)
      {
        local.TotMedOwedForJoint.TotalCurrency =
          local.TotMedOwedForJoint.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
      else if (entities.ObligationType.SystemGeneratedIdentifier == 1 || entities
        .ObligationType.SystemGeneratedIdentifier == 2 || entities
        .ObligationType.SystemGeneratedIdentifier == 11 || entities
        .ObligationType.SystemGeneratedIdentifier == 12 || entities
        .ObligationType.SystemGeneratedIdentifier == 13 || entities
        .ObligationType.SystemGeneratedIdentifier == 14 || entities
        .ObligationType.SystemGeneratedIdentifier == 17 || entities
        .ObligationType.SystemGeneratedIdentifier == 18 || entities
        .ObligationType.SystemGeneratedIdentifier == 20 || entities
        .ObligationType.SystemGeneratedIdentifier == 21 || entities
        .ObligationType.SystemGeneratedIdentifier == 22)
      {
        local.TotAfFcOwedForJoint.TotalCurrency =
          local.TotAfFcOwedForJoint.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
    }

    export.TotMedicalOwed.TotalCurrency += local.TotMedOwedForJoint.
      TotalCurrency / 2;
    export.TotAfFcOwed.TotalCurrency += local.TotAfFcOwedForJoint.
      TotalCurrency / 2;
  }

  private IEnumerable<bool> ReadDebtDebtDetailObligationObligationType1()
  {
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtDebtDetailObligationObligationType1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
        db.SetNullableDate(
          command, "retiredDt",
          local.Initialized.RetiredDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.DebtDetail.DueDt = db.GetDate(reader, 8);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 9);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 10);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 11);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDebtDetailObligationObligationType2()
  {
    entities.DebtDetail.Populated = false;
    entities.ObligationType.Populated = false;
    entities.Debt.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadDebtDebtDetailObligationObligationType2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspSupNumber", import.Supported.Number);
        db.SetNullableDate(
          command, "retiredDt",
          local.Initialized.RetiredDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Obligation.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 5);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 6);
        entities.Debt.OtyType = db.GetInt32(reader, 7);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 7);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.DebtDetail.DueDt = db.GetDate(reader, 8);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 9);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 10);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 11);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 12);
        entities.ObligationType.Classification = db.GetString(reader, 13);
        entities.DebtDetail.Populated = true;
        entities.ObligationType.Populated = true;
        entities.Debt.Populated = true;
        entities.Obligation.Populated = true;

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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    private CsePerson supported;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotMedicalOwed.
    /// </summary>
    [JsonPropertyName("totMedicalOwed")]
    public Common TotMedicalOwed
    {
      get => totMedicalOwed ??= new();
      set => totMedicalOwed = value;
    }

    /// <summary>
    /// A value of TotAfFcOwed.
    /// </summary>
    [JsonPropertyName("totAfFcOwed")]
    public Common TotAfFcOwed
    {
      get => totAfFcOwed ??= new();
      set => totAfFcOwed = value;
    }

    private Common totMedicalOwed;
    private Common totAfFcOwed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BeginNextMonth.
    /// </summary>
    [JsonPropertyName("beginNextMonth")]
    public DateWorkArea BeginNextMonth
    {
      get => beginNextMonth ??= new();
      set => beginNextMonth = value;
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

    /// <summary>
    /// A value of TotMedOwedForJoint.
    /// </summary>
    [JsonPropertyName("totMedOwedForJoint")]
    public Common TotMedOwedForJoint
    {
      get => totMedOwedForJoint ??= new();
      set => totMedOwedForJoint = value;
    }

    /// <summary>
    /// A value of TotAfFcOwedForJoint.
    /// </summary>
    [JsonPropertyName("totAfFcOwedForJoint")]
    public Common TotAfFcOwedForJoint
    {
      get => totAfFcOwedForJoint ??= new();
      set => totAfFcOwedForJoint = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DebtDetail Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea beginNextMonth;
    private DateWorkArea dateWorkArea;
    private Common totMedOwedForJoint;
    private Common totAfFcOwedForJoint;
    private DebtDetail initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
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

    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount supported;
    private CsePerson csePerson;
  }
#endregion
}

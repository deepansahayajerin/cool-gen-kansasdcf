// Program: FN_RCAP_COMP_REC_DEBTS_BAL_F_OBE, ID: 372674867, model: 746.
// Short name: SWE02162
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
/// A program: FN_RCAP_COMP_REC_DEBTS_BAL_F_OBE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block computes the total recovery obligations balance that is 
/// available for recapture.
/// </para>
/// </summary>
[Serializable]
public partial class FnRcapCompRecDebtsBalFObe: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCAP_COMP_REC_DEBTS_BAL_F_OBE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcapCompRecDebtsBalFObe(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcapCompRecDebtsBalFObe.
  /// </summary>
  public FnRcapCompRecDebtsBalFObe(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block computes and returns the total recovery obligation 
    // balance available for recapture.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ---------------------------------------------
    // 120397	govind		Initial code.
    // 05/22/99  Fangman    Added changes discovered in walkthrus.
    // ---------------------------------------------
    if (!ReadObligor())
    {
      // --- Nothing to recapture
      return;
    }

    foreach(var item in ReadObligationObligationType())
    {
      if (!ReadRecaptureInclusion())
      {
        // --- This obligation is not marked for recapture.
        continue;
      }

      foreach(var item1 in ReadDebtDebtDetail())
      {
        export.TotalRecoveryDebtBal.TotalCurrency =
          export.TotalRecoveryDebtBal.TotalCurrency + entities
          .DebtDetail.BalanceDueAmt + entities
          .DebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
    }
  }

  private IEnumerable<bool> ReadDebtDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;

    return ReadEach("ReadDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 4);
        entities.Debt.DebtType = db.GetString(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetail.DueDt = db.GetDate(reader, 9);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 10);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 11);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligor.Populated);
    entities.ObligationType.Populated = false;
    entities.Obligation.Populated = false;

    return ReadEach("ReadObligationObligationType",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligor.Type1);
        db.SetString(command, "cspNumber", entities.Obligor.CspNumber);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationType.Code = db.GetString(reader, 4);
        entities.ObligationType.Classification = db.GetString(reader, 5);
        entities.ObligationType.Populated = true;
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);

        return true;
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
      });
  }

  private bool ReadRecaptureInclusion()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.RecaptureInclusion.Populated = false;

    return Read("ReadRecaptureInclusion",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.RecaptureInclusion.OtyType = db.GetInt32(reader, 0);
        entities.RecaptureInclusion.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.RecaptureInclusion.CspNumber = db.GetString(reader, 2);
        entities.RecaptureInclusion.CpaType = db.GetString(reader, 3);
        entities.RecaptureInclusion.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.RecaptureInclusion.DiscontinueDate = db.GetDate(reader, 5);
        entities.RecaptureInclusion.EffectiveDate = db.GetDate(reader, 6);
        entities.RecaptureInclusion.Populated = true;
        CheckValid<RecaptureInclusion>("CpaType",
          entities.RecaptureInclusion.CpaType);
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson obligee;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TotalRecoveryDebtBal.
    /// </summary>
    [JsonPropertyName("totalRecoveryDebtBal")]
    public Common TotalRecoveryDebtBal
    {
      get => totalRecoveryDebtBal ??= new();
      set => totalRecoveryDebtBal = value;
    }

    private Common totalRecoveryDebtBal;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of RecaptureInclusion.
    /// </summary>
    [JsonPropertyName("recaptureInclusion")]
    public RecaptureInclusion RecaptureInclusion
    {
      get => recaptureInclusion ??= new();
      set => recaptureInclusion = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private RecaptureInclusion recaptureInclusion;
    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private CsePerson obligee;
  }
#endregion
}

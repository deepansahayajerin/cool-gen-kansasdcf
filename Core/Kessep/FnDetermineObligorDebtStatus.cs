// Program: FN_DETERMINE_OBLIGOR_DEBT_STATUS, ID: 372279918, model: 746.
// Short name: SWE02265
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_OBLIGOR_DEBT_STATUS.
/// </summary>
[Serializable]
public partial class FnDetermineObligorDebtStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_OBLIGOR_DEBT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineObligorDebtStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineObligorDebtStatus.
  /// </summary>
  public FnDetermineObligorDebtStatus(IContext context, Import import,
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
    export.PifInd.Flag = "N";

    if (Equal(import.Process.Date, local.Null1.Date))
    {
      local.Process.Date = Now().Date;
    }
    else
    {
      local.Process.Date = import.Process.Date;
    }

    foreach(var item in ReadDebtDetail())
    {
      local.TotalAmountDue.TotalCurrency += entities.ExistingDebtDetail.
        BalanceDueAmt + entities
        .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
    }

    if (local.TotalAmountDue.TotalCurrency > 0)
    {
      return;
    }

    if (!ReadAccrualInstructions())
    {
      export.PifInd.Flag = "Y";
    }
  }

  private bool ReadAccrualInstructions()
  {
    entities.ExistingAccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetDate(command, "asOfDt", local.Process.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.ExistingAccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.ExistingAccrualInstructions.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingAccrualInstructions.CspNumber =
          db.GetString(reader, 3);
        entities.ExistingAccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.ExistingAccrualInstructions.OtrGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingAccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.ExistingAccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.ExistingAccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.ExistingAccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.ExistingAccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);

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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    private DateWorkArea process;
    private CsePerson obligor;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PifInd.
    /// </summary>
    [JsonPropertyName("pifInd")]
    public Common PifInd
    {
      get => pifInd ??= new();
      set => pifInd = value;
    }

    private Common pifInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    private DateWorkArea process;
    private DateWorkArea null1;
    private Common totalAmountDue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingAccrualInstructions.
    /// </summary>
    [JsonPropertyName("existingAccrualInstructions")]
    public AccrualInstructions ExistingAccrualInstructions
    {
      get => existingAccrualInstructions ??= new();
      set => existingAccrualInstructions = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingObligation;
    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private AccrualInstructions existingAccrualInstructions;
  }
#endregion
}

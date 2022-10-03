// Program: FN_DETERMINE_OBLIGATION_DBT_STAT, ID: 372730518, model: 746.
// Short name: SWE02857
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_OBLIGATION_DBT_STAT.
/// </summary>
[Serializable]
public partial class FnDetermineObligationDbtStat: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_OBLIGATION_DBT_STAT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineObligationDbtStat(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineObligationDbtStat.
  /// </summary>
  public FnDetermineObligationDbtStat(IContext context, Import import,
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
    export.PaidOffInd.Flag = "N";

    if (Equal(import.Process.Date, local.Null1.Date))
    {
      local.Process.Date = Now().Date;
    }
    else
    {
      local.Process.Date = import.Process.Date;
    }

    if (ReadAccrualInstructions())
    {
      return;
    }

    if (ReadDebtDetail())
    {
      return;
    }

    // *****************************************************************
    // No Active Accural Instructions and No Debt amount Due
    // *****************************************************************
    export.PaidOffInd.Flag = "Y";
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingAccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistant.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Persistant.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistant.CspNumber);
        db.SetString(command, "cpaType", import.Persistant.CpaType);
        db.SetNullableDate(
          command, "discontinueDt", local.Process.Date.GetValueOrDefault());
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
        entities.ExistingAccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.ExistingAccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 7);
        entities.ExistingAccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.ExistingAccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.ExistingAccrualInstructions.CpaType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Persistant.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Persistant.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistant.CspNumber);
        db.SetString(command, "cpaType", import.Persistant.CpaType);
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
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
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
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public Obligation Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    private Obligation persistant;
    private DateWorkArea process;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaidOffInd.
    /// </summary>
    [JsonPropertyName("paidOffInd")]
    public Common PaidOffInd
    {
      get => paidOffInd ??= new();
      set => paidOffInd = value;
    }

    private Common paidOffInd;
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

    private DateWorkArea process;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private ObligationTransaction existingDebt;
    private DebtDetail existingDebtDetail;
    private AccrualInstructions existingAccrualInstructions;
  }
#endregion
}

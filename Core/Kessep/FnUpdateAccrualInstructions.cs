// Program: FN_UPDATE_ACCRUAL_INSTRUCTIONS, ID: 371968319, model: 746.
// Short name: SWE00624
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_ACCRUAL_INSTRUCTIONS.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block updates an occurrence of Accrual Instructions.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateAccrualInstructions: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_ACCRUAL_INSTRUCTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateAccrualInstructions(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateAccrualInstructions.
  /// </summary>
  public FnUpdateAccrualInstructions(IContext context, Import import,
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
    if (ReadAccrualInstructions())
    {
      ++export.ImportNumberOfReads.Count;
    }
    else
    {
      ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

      return;
    }

    try
    {
      UpdateAccrualInstructions();
      ++export.ImportNumberOfUpdates.Count;
      export.AccrualInstructions.Assign(entities.AccrualInstructions);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_ACCRUAL_INSTRUCTIONS_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrGeneratedId", import.Debt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private void UpdateAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);

    var asOfDt = import.AccrualInstructions.AsOfDt;
    var discontinueDt = import.AccrualInstructions.DiscontinueDt;
    var lastAccrualDt = import.AccrualInstructions.LastAccrualDt;

    entities.AccrualInstructions.Populated = false;
    Update("UpdateAccrualInstructions",
      (db, command) =>
      {
        db.SetDate(command, "asOfDt", asOfDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableDate(command, "lastAccrualDt", lastAccrualDt);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.AccrualInstructions.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.AccrualInstructions.OtrGeneratedId);
      });

    entities.AccrualInstructions.AsOfDt = asOfDt;
    entities.AccrualInstructions.DiscontinueDt = discontinueDt;
    entities.AccrualInstructions.LastAccrualDt = lastAccrualDt;
    entities.AccrualInstructions.Populated = true;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private CsePerson obligor;
    private Obligation obligation;
    private ObligationTransaction debt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private AccrualInstructions accrualInstructions;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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

    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private ObligationTransaction debt;
  }
#endregion
}

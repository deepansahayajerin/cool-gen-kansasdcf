// Program: FN_DELETE_MANUAL_DISTRIB_AUDIT, ID: 372039877, model: 746.
// Short name: SWE00418
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_MANUAL_DISTRIB_AUDIT.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteManualDistribAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_MANUAL_DISTRIB_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteManualDistribAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteManualDistribAudit.
  /// </summary>
  public FnDeleteManualDistribAudit(IContext context, Import import,
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
    // ***---  Changed Read proerties to select only - b adams
    if (ReadObligation())
    {
      if (ReadManualDistributionAudit())
      {
        DeleteManualDistributionAudit();
      }
      else
      {
        ExitState = "FN0000_MANUAL_DIST_AUDIT_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";
    }
  }

  private void DeleteManualDistributionAudit()
  {
    Update("DeleteManualDistributionAudit",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyType", entities.ManualDistributionAudit.OtyType);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ManualDistributionAudit.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ManualDistributionAudit.CspNumber);
        db.SetString(
          command, "cpaType", entities.ManualDistributionAudit.CpaType);
        db.SetDate(
          command, "effectiveDt",
          entities.ManualDistributionAudit.EffectiveDt.GetValueOrDefault());
      });
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.ManualDistributionAudit.EffectiveDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt",
          import.ManualDistributionAudit.DiscontinueDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ManualDistributionAudit.OtyType = db.GetInt32(reader, 0);
        entities.ManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ManualDistributionAudit.CspNumber = db.GetString(reader, 2);
        entities.ManualDistributionAudit.CpaType = db.GetString(reader, 3);
        entities.ManualDistributionAudit.EffectiveDt = db.GetDate(reader, 4);
        entities.ManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ManualDistributionAudit.CreatedBy = db.GetString(reader, 6);
        entities.ManualDistributionAudit.CreatedTmst =
          db.GetDateTime(reader, 7);
        entities.ManualDistributionAudit.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ManualDistributionAudit.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ManualDistributionAudit.Instructions =
          db.GetNullableString(reader, 10);
        entities.ManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ManualDistributionAudit.CpaType);
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
          
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
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
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
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
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
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

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private ManualDistributionAudit manualDistributionAudit;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private ObligationType obligationType;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
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

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private Obligation obligation;
    private ManualDistributionAudit manualDistributionAudit;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private ObligationType obligationType;
  }
#endregion
}

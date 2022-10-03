// Program: FN_CREATE_MANUAL_DIST_AUDIT, ID: 372039878, model: 746.
// Short name: SWE00370
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_MANUAL_DIST_AUDIT.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateManualDistAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_MANUAL_DIST_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateManualDistAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateManualDistAudit.
  /// </summary>
  public FnCreateManualDistAudit(IContext context, Import import, Export export):
    
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
    // 5/11/99 - Bud Adams  -  Deleted MOVEs to export and deleted
    //   the view matching of export views.  ZDEL those views.
    //   Changed Read properties to be select only.  Imported
    //   current-timestamp value.
    // =================================================
    if (ReadObligation())
    {
      if (ReadManualDistributionAudit())
      {
        ExitState = "FN0000_OVERLAPPING_MAN_DIST_INST";

        return;
      }

      try
      {
        CreateManualDistributionAudit();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MANUAL_DIST_AUDIT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MANUAL_DIST_AUDIT_PV";

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
      ExitState = "FN0000_OBLIGATION_NF";
    }
  }

  private void CreateManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var otyType = entities.Obligation.DtyGeneratedId;
    var obgGeneratedId = entities.Obligation.SystemGeneratedIdentifier;
    var cspNumber = entities.Obligation.CspNumber;
    var cpaType = entities.Obligation.CpaType;
    var effectiveDt = import.ManualDistributionAudit.EffectiveDt;
    var discontinueDt = import.ManualDistributionAudit.DiscontinueDt;
    var createdBy = import.ManualDistributionAudit.CreatedBy;
    var createdTmst = import.Current.Timestamp;
    var lastUpdatedBy = entities.ManualDistributionAudit.CreatedBy;
    var instructions = import.ManualDistributionAudit.Instructions ?? "";

    CheckValid<ManualDistributionAudit>("CpaType", cpaType);
    entities.ManualDistributionAudit.Populated = false;
    Update("CreateManualDistributionAudit",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTmst);
        db.SetNullableString(command, "mnlDistInstr", instructions);
      });

    entities.ManualDistributionAudit.OtyType = otyType;
    entities.ManualDistributionAudit.ObgGeneratedId = obgGeneratedId;
    entities.ManualDistributionAudit.CspNumber = cspNumber;
    entities.ManualDistributionAudit.CpaType = cpaType;
    entities.ManualDistributionAudit.EffectiveDt = effectiveDt;
    entities.ManualDistributionAudit.DiscontinueDt = discontinueDt;
    entities.ManualDistributionAudit.CreatedBy = createdBy;
    entities.ManualDistributionAudit.CreatedTmst = createdTmst;
    entities.ManualDistributionAudit.LastUpdatedTmst = createdTmst;
    entities.ManualDistributionAudit.Instructions = instructions;
    entities.ManualDistributionAudit.Populated = true;
  }

  private bool ReadManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Overlapping.Populated = false;

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
          import.ManualDistributionAudit.DiscontinueDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt",
          import.ManualDistributionAudit.EffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Overlapping.OtyType = db.GetInt32(reader, 0);
        entities.Overlapping.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.Overlapping.CspNumber = db.GetString(reader, 2);
        entities.Overlapping.CpaType = db.GetString(reader, 3);
        entities.Overlapping.EffectiveDt = db.GetDate(reader, 4);
        entities.Overlapping.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.Overlapping.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.Overlapping.CpaType);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private ManualDistributionAudit manualDistributionAudit;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ZdelManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("zdelManualDistributionAudit")]
    public ManualDistributionAudit ZdelManualDistributionAudit
    {
      get => zdelManualDistributionAudit ??= new();
      set => zdelManualDistributionAudit = value;
    }

    /// <summary>
    /// A value of ZdelObligation.
    /// </summary>
    [JsonPropertyName("zdelObligation")]
    public Obligation ZdelObligation
    {
      get => zdelObligation ??= new();
      set => zdelObligation = value;
    }

    private ManualDistributionAudit zdelManualDistributionAudit;
    private Obligation zdelObligation;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Overlapping.
    /// </summary>
    [JsonPropertyName("overlapping")]
    public ManualDistributionAudit Overlapping
    {
      get => overlapping ??= new();
      set => overlapping = value;
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

    private ManualDistributionAudit overlapping;
    private ManualDistributionAudit manualDistributionAudit;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private Obligation obligation;
  }
#endregion
}

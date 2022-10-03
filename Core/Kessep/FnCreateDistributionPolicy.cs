// Program: FN_CREATE_DISTRIBUTION_POLICY, ID: 371959583, model: 746.
// Short name: SWE00367
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process creates a Distribution Policy.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISTRIBUTION_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDistributionPolicy.
  /// </summary>
  public FnCreateDistributionPolicy(IContext context, Import import,
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
    // ****************************************************
    // A.Kinney  04/30/97	Changed Current_Date
    // 03/24/98	Siraj Konkader		ZDEL cleanup
    // ****************************************************
    local.Current.Date = Now().Date;

    // ***** HARD CODE AREA *****
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    // ***** EDIT AREA *****
    if (Equal(import.DistributionPolicy.DiscontinueDt, local.Initialized.Date))
    {
      local.DistributionPolicy.DiscontinueDt = local.Maximum.Date;
    }
    else
    {
      local.DistributionPolicy.DiscontinueDt =
        import.DistributionPolicy.DiscontinueDt;
    }

    if (Lt(import.DistributionPolicy.EffectiveDt, local.Current.Date))
    {
      ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

      return;
    }

    if (!Lt(import.DistributionPolicy.EffectiveDt,
      local.DistributionPolicy.DiscontinueDt))
    {
      ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadCollectionType())
      {
        // : Collection Type successfully retrieved.
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }
    }

    // ***** MAIN-LINE AREA *****
    CreateDistributionPolicy();

    // : Distribution Policy successfully created.
    export.DistributionPolicy.SystemGeneratedIdentifier =
      entities.DistributionPolicy.SystemGeneratedIdentifier;

    // : Permitted Value and Already Exists exceptions will cause an abort.
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignDistPlcyId()
  {
    var useImport = new FnAssignDistPlcyId.Import();
    var useExport = new FnAssignDistPlcyId.Export();

    Call(FnAssignDistPlcyId.Execute, useImport, useExport);

    return useExport.DistributionPolicy.SystemGeneratedIdentifier;
  }

  private void CreateDistributionPolicy()
  {
    var systemGeneratedIdentifier = UseFnAssignDistPlcyId();
    var name = import.DistributionPolicy.Name;
    var effectiveDt = import.DistributionPolicy.EffectiveDt;
    var discontinueDt = local.DistributionPolicy.DiscontinueDt;
    var createdBy = global.UserId;
    var createdTmst = Now();
    var cltIdentifier = import.P.SequentialIdentifier;
    var description = import.DistributionPolicy.Description;

    entities.DistributionPolicy.Populated = false;
    Update("CreateDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(command, "distPlcyId", systemGeneratedIdentifier);
        db.SetString(command, "distPlcyNm", name);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableDate(command, "maxPrcdDt", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetString(command, "distPlcyDsc", description);
      });

    entities.DistributionPolicy.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DistributionPolicy.Name = name;
    entities.DistributionPolicy.EffectiveDt = effectiveDt;
    entities.DistributionPolicy.DiscontinueDt = discontinueDt;
    entities.DistributionPolicy.MaximumProcessedDt = null;
    entities.DistributionPolicy.CreatedBy = createdBy;
    entities.DistributionPolicy.CreatedTmst = createdTmst;
    entities.DistributionPolicy.LastUpdatedBy = "";
    entities.DistributionPolicy.LastUpdatedTmst = null;
    entities.DistributionPolicy.CltIdentifier = cltIdentifier;
    entities.DistributionPolicy.Description = description;
    entities.DistributionPolicy.Populated = true;
  }

  private bool ReadCollectionType()
  {
    import.P.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        import.P.SequentialIdentifier = db.GetInt32(reader, 0);
        import.P.Code = db.GetString(reader, 1);
        import.P.Name = db.GetString(reader, 2);
        import.P.EffectiveDate = db.GetDate(reader, 3);
        import.P.DiscontinueDate = db.GetNullableDate(reader, 4);
        import.P.Populated = true;
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
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CollectionType P
    {
      get => p ??= new();
      set => p = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private CollectionType p;
    private CollectionType collectionType;
    private DistributionPolicy distributionPolicy;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy distributionPolicy;
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
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea current;
    private DistributionPolicy distributionPolicy;
    private DateWorkArea initialized;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private DistributionPolicy distributionPolicy;
  }
#endregion
}

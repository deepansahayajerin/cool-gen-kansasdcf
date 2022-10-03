// Program: FN_UPDATE_DISTRIBUTION_POLICY, ID: 371959580, model: 746.
// Short name: SWE00651
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process updates a Distribution Policy.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_DISTRIBUTION_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateDistributionPolicy.
  /// </summary>
  public FnUpdateDistributionPolicy(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **************************************************
    // Maintenance Log
    // Author             Date  Chg Req#   Description
    // M. D. Wheaton       04/28/97        Change Current_Date
    // **************************************************
    local.Current.Date = Now().Date;

    // ***** HARD CODE AREA *****
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (Equal(import.DistributionPolicy.DiscontinueDt, local.Initialized.Date))
    {
      local.DistributionPolicy.DiscontinueDt = local.Maximum.Date;
    }
    else
    {
      local.DistributionPolicy.DiscontinueDt =
        import.DistributionPolicy.DiscontinueDt;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadCollectionType())
      {
        MoveCollectionType(import.PersistentCollectionType,
          export.CollectionType);
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadDistributionPolicy())
      {
        // : Set flag to indicate whether or not DP is active.
        if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
          local.Blank.Date) || !
          Lt(local.Current.Date, entities.DistributionPolicy.EffectiveDt))
        {
          local.DpActive.Flag = "Y";
        }

        if (!Equal(import.PersistentDistributionPolicy.MaximumProcessedDt,
          local.Initialized.Date))
        {
          ExitState = "CANNOT_CHANGE_DUE_TO_ACTIVITY";

          return;
        }

        if (!Equal(import.DistributionPolicy.EffectiveDt,
          import.PersistentDistributionPolicy.EffectiveDt))
        {
          if (!Lt(local.Current.Date,
            import.PersistentDistributionPolicy.EffectiveDt))
          {
            ExitState = "CANNOT_CHANGE_EFFECTIVE_DATE";

            return;
          }
          else if (!Lt(local.Current.Date, import.DistributionPolicy.EffectiveDt))
            
          {
            ExitState = "EFFECTIVE_DATE_PRIOR_TO_CURRENT";

            return;
          }
        }

        if (!Lt(local.Current.Date,
          import.PersistentDistributionPolicy.DiscontinueDt))
        {
          ExitState = "CANNOT_CHANGE_A_DISCONTINUED_REC";

          return;
        }

        if (!Lt(import.DistributionPolicy.EffectiveDt,
          local.DistributionPolicy.DiscontinueDt))
        {
          ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";

          return;
        }

        if (!Lt(local.Current.Date, local.DistributionPolicy.DiscontinueDt))
        {
          ExitState = "CANNOT_DISCONTINUE_DISC_DATE";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_DIST_PLCY_NF";

        return;
      }
    }

    // ***** MAIN-LINE AREA *****
    // : If the Distribution Policy is active, only the discontinue date can be 
    // updated.
    if (AsChar(local.DpActive.Flag) == 'Y')
    {
      local.DistributionPolicy.Assign(entities.DistributionPolicy);
      local.DistributionPolicy.DiscontinueDt =
        import.DistributionPolicy.DiscontinueDt;
    }
    else
    {
      MoveDistributionPolicy(import.DistributionPolicy, local.DistributionPolicy);
        
    }

    UpdateDistributionPolicy();

    // : Distribution Policy successfully updated.
    // Permitted Value and Already Exists exceptions will cause an abort.
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveDistributionPolicy(DistributionPolicy source,
    DistributionPolicy target)
  {
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCollectionType()
  {
    import.PersistentCollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        import.PersistentCollectionType.SequentialIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCollectionType.Code = db.GetString(reader, 1);
        import.PersistentCollectionType.Name = db.GetString(reader, 2);
        import.PersistentCollectionType.EffectiveDate = db.GetDate(reader, 3);
        import.PersistentCollectionType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        import.PersistentCollectionType.Populated = true;
      });
  }

  private bool ReadDistributionPolicy()
  {
    import.PersistentDistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.PersistentDistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentDistributionPolicy.Name = db.GetString(reader, 1);
        import.PersistentDistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        import.PersistentDistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        import.PersistentDistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        import.PersistentDistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        import.PersistentDistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        import.PersistentDistributionPolicy.Description =
          db.GetString(reader, 7);
        import.PersistentDistributionPolicy.Populated = true;
      });
  }

  private void UpdateDistributionPolicy()
  {
    var name = local.DistributionPolicy.Name;
    var effectiveDt = local.DistributionPolicy.EffectiveDt;
    var discontinueDt = local.DistributionPolicy.DiscontinueDt;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var description = local.DistributionPolicy.Description;

    import.PersistentDistributionPolicy.Populated = false;
    Update("UpdateDistributionPolicy",
      (db, command) =>
      {
        db.SetString(command, "distPlcyNm", name);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distPlcyDsc", description);
        db.SetInt32(
          command, "distPlcyId",
          import.PersistentDistributionPolicy.SystemGeneratedIdentifier);
      });

    import.PersistentDistributionPolicy.Name = name;
    import.PersistentDistributionPolicy.EffectiveDt = effectiveDt;
    import.PersistentDistributionPolicy.DiscontinueDt = discontinueDt;
    import.PersistentDistributionPolicy.LastUpdatedBy = lastUpdatedBy;
    import.PersistentDistributionPolicy.LastUpdatedTmst = lastUpdatedTmst;
    import.PersistentDistributionPolicy.Description = description;
    import.PersistentDistributionPolicy.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of PersistentDistributionPolicy.
    /// </summary>
    [JsonPropertyName("persistentDistributionPolicy")]
    public DistributionPolicy PersistentDistributionPolicy
    {
      get => persistentDistributionPolicy ??= new();
      set => persistentDistributionPolicy = value;
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
    /// A value of PersistentCollectionType.
    /// </summary>
    [JsonPropertyName("persistentCollectionType")]
    public CollectionType PersistentCollectionType
    {
      get => persistentCollectionType ??= new();
      set => persistentCollectionType = value;
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

    private DistributionPolicy persistentDistributionPolicy;
    private DistributionPolicy distributionPolicy;
    private CollectionType persistentCollectionType;
    private CollectionType collectionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private CollectionType collectionType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
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

    /// <summary>
    /// A value of DpActive.
    /// </summary>
    [JsonPropertyName("dpActive")]
    public Common DpActive
    {
      get => dpActive ??= new();
      set => dpActive = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      distributionPolicy = null;
      initialized = null;
      maximum = null;
      dpActive = null;
      blank = null;
    }

    private DateWorkArea current;
    private DistributionPolicy distributionPolicy;
    private DateWorkArea initialized;
    private DateWorkArea maximum;
    private Common dpActive;
    private DateWorkArea blank;
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

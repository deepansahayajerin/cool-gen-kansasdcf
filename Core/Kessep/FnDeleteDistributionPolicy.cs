// Program: FN_DELETE_DISTRIBUTION_POLICY, ID: 371959581, model: 746.
// Short name: SWE00412
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process deletes a Distribution Policy.
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_DISTRIBUTION_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteDistributionPolicy.
  /// </summary>
  public FnDeleteDistributionPolicy(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // 04/29/97      JeHoward                        Current date fix.
    // ----------------------------------------------------------------
    local.Current.Date = Now().Date;

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      if (ReadCollectionType())
      {
        MoveCollectionType(import.Persistent, export.CollectionType);
      }
      else
      {
        ExitState = "FN0000_COLLECTION_TYPE_NF";

        return;
      }
    }

    if (ReadDistributionPolicy())
    {
      if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
        local.Initialized.Date))
      {
        ExitState = "CANNOT_DELETE_DUE_TO_ACTIVITY";

        return;
      }

      if (ReadDistributionPolicyRule())
      {
        ExitState = "FN0000_CANNOT_DEL_DP_W_DPR";

        return;
      }

      // : Distribution Policy successfully retrieved.
    }
    else
    {
      ExitState = "FN0000_DIST_PLCY_NF";

      return;
    }

    DeleteDistributionPolicy();
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private void DeleteDistributionPolicy()
  {
    Update("DeleteDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          entities.DistributionPolicy.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCollectionType()
  {
    import.Persistent.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        import.Persistent.SequentialIdentifier = db.GetInt32(reader, 0);
        import.Persistent.Code = db.GetString(reader, 1);
        import.Persistent.Name = db.GetString(reader, 2);
        import.Persistent.EffectiveDate = db.GetDate(reader, 3);
        import.Persistent.DiscontinueDate = db.GetNullableDate(reader, 4);
        import.Persistent.Populated = true;
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.DistributionPolicy.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "cltIdentifier", import.Persistent.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 1);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicyRule()
  {
    entities.DistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicyRule.DbpGeneratedId = db.GetInt32(reader, 0);
        entities.DistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 2);
        entities.DistributionPolicyRule.Populated = true;
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
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CollectionType Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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

    private DistributionPolicy distributionPolicy;
    private CollectionType persistent;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea current;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
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

    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicy distributionPolicy;
  }
#endregion
}

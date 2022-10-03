// Program: FN_READ_DISTRIBUTION_POLICY, ID: 371959584, model: 746.
// Short name: SWE00562
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DISTRIBUTION_POLICY.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will read the distribution policy base on the distribution 
/// policy code passed to it.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDistributionPolicy: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DISTRIBUTION_POLICY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDistributionPolicy(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDistributionPolicy.
  /// </summary>
  public FnReadDistributionPolicy(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***** MAIN-LINE AREA *****
    // *******************************************************
    // AUTHOR		DATE			DESCRIPTION
    // Sheraz Malik	04/28/97	Change current date	
    // *******************************************************
    local.Current.Date = Now().Date;

    if (!import.Persistent.Populated)
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

    if (import.DistributionPolicy.SystemGeneratedIdentifier == 0)
    {
      if (!Equal(import.DistributionPolicy.EffectiveDt, local.Blank.Date))
      {
        if (ReadDistributionPolicy1())
        {
          export.DistributionPolicy.Assign(entities.DistributionPolicy);
        }
        else
        {
          ExitState = "FN0000_DIST_PLCY_NF";
        }
      }
      else
      {
        if (ReadDistributionPolicy3())
        {
          export.DistributionPolicy.Assign(entities.DistributionPolicy);
        }

        if (!entities.DistributionPolicy.Populated)
        {
          ExitState = "FN0000_DIST_PLCY_NF";
        }
      }
    }
    else if (ReadDistributionPolicy2())
    {
      if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
        local.Blank.Date) || !
        Lt(local.Current.Date, entities.DistributionPolicy.EffectiveDt))
      {
        export.DpActive.Flag = "Y";
      }

      export.DistributionPolicy.Assign(entities.DistributionPolicy);
    }
    else
    {
      ExitState = "FN0000_DIST_PLCY_NF";
    }
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
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

  private bool ReadDistributionPolicy1()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDt",
          import.DistributionPolicy.EffectiveDt.GetValueOrDefault());
        db.SetNullableInt32(
          command, "cltIdentifier", import.Persistent.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicy.Description = db.GetString(reader, 8);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicy2()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy2",
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
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicy.Description = db.GetString(reader, 8);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicy3()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "cltIdentifier", import.Persistent.SequentialIdentifier);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DistributionPolicy.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.DistributionPolicy.CltIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DistributionPolicy.Description = db.GetString(reader, 8);
        entities.DistributionPolicy.Populated = true;
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

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    private CollectionType persistent;
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
    /// A value of DpActive.
    /// </summary>
    [JsonPropertyName("dpActive")]
    public Common DpActive
    {
      get => dpActive ??= new();
      set => dpActive = value;
    }

    private DistributionPolicy distributionPolicy;
    private CollectionType collectionType;
    private Common dpActive;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private DateWorkArea current;
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

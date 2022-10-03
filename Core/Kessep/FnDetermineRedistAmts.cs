// Program: FN_DETERMINE_REDIST_AMTS, ID: 372279907, model: 746.
// Short name: SWE02309
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_REDIST_AMTS.
/// </summary>
[Serializable]
public partial class FnDetermineRedistAmts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_REDIST_AMTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineRedistAmts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineRedistAmts.
  /// </summary>
  public FnDetermineRedistAmts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // MAINTENANCE LOG
    // : Work order # 197, M Brown, Oct 2000 - added Collection dist method of '
    // W' wherever dist method of 'M' is referred to. They mean the same thing
    // in batch processing.  The 'W' is set by debt adjustment when a debt is
    // written off, to
    // enable us to set it back to 'A'utomatic if the debt is reinstated.
    // : WR010504 by A Doty, Dec 2001 - Added Collection Distribution Method of
    // "P" and "C" (Same as above)
    // : If the lastest set of Collections for this CRD are in adjusted status 
    // and the adjustment reason is for Wrong Account, then escape out and
    // redistribute the CRD as if it is the first time.
    local.AdjustmentFoundInd.Flag = "N";

    foreach(var item in ReadCollection3())
    {
      local.AdjustmentFoundInd.Flag = "Y";
      local.Hold.CollectionAdjustmentDt =
        entities.ExistingCollection.CollectionAdjustmentDt;

      if (ReadCollectionAdjustmentReason())
      {
        return;
      }
      else
      {
        break;
      }
    }

    if (AsChar(local.AdjustmentFoundInd.Flag) == 'N')
    {
      return;
    }

    // : The latest set of adjustments are not for Wrong Account.
    //   Determine the amounts to redistribute by court oder applied to.
    foreach(var item in ReadCollection1())
    {
      local.Total.Amount += entities.ExistingCollection.Amount;

      if (export.Group.IsEmpty)
      {
        export.Group.Index = 0;
        export.Group.CheckSize();

        MoveCollection(entities.ExistingCollection,
          export.Group.Update.Collection);

        continue;
      }

      if (Equal(export.Group.Item.Collection.CourtOrderAppliedTo,
        entities.ExistingCollection.CourtOrderAppliedTo))
      {
        export.Group.Update.Collection.Amount =
          export.Group.Item.Collection.Amount + entities
          .ExistingCollection.Amount;

        continue;
      }

      ++export.Group.Index;
      export.Group.CheckSize();

      MoveCollection(entities.ExistingCollection, export.Group.Update.Collection);
        
    }

    if (local.Total.Amount > import.AmtToDistribute.TotalCurrency)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (export.Group.Index + 1 == export.Group.Count)
        {
          export.Group.Update.Collection.Amount =
            import.AmtToDistribute.TotalCurrency - local.TotalReapplied.Amount;

          break;
        }

        export.Group.Update.Collection.Amount =
          export.Group.Item.Collection.Amount * (
            import.AmtToDistribute.TotalCurrency / local.Total.Amount);
        local.TotalReapplied.Amount += export.Group.Item.Collection.Amount;
      }

      export.Group.CheckIndex();
    }

    // mfb, oct 2000 - Added check for 'W' or 'M' distribution method, instead 
    // of just 'M'.  Removed qualification on the read, and put it in an IF
    // instead.
    foreach(var item in ReadCollection2())
    {
      if (AsChar(entities.ExistingCollection.DistributionMethod) == 'A' || (
        AsChar(entities.ExistingCollection.DistributionMethod) == 'M' || AsChar
        (entities.ExistingCollection.DistributionMethod) == 'W' || AsChar
        (entities.ExistingCollection.DistributionMethod) == 'P' || AsChar
        (entities.ExistingCollection.DistributionMethod) == 'C') && !
        Lt(Date(entities.ExistingCollection.CreatedTmst),
        local.Hold.CollectionAdjustmentDt))
      {
        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (!export.Group.CheckSize())
          {
            break;
          }

          if (Equal(export.Group.Item.Collection.CourtOrderAppliedTo,
            entities.ExistingCollection.CourtOrderAppliedTo))
          {
            export.Group.Update.Collection.Amount =
              export.Group.Item.Collection.Amount - entities
              .ExistingCollection.Amount;
          }
        }

        export.Group.CheckIndex();
      }
    }

    local.Total.Amount = 0;

    for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
      export.Group.Index)
    {
      if (!export.Group.CheckSize())
      {
        break;
      }

      if (export.Group.Item.Collection.Amount < 0)
      {
        export.Group.Update.Collection.Amount = 0;

        continue;
      }

      local.Total.Amount += export.Group.Item.Collection.Amount;
    }

    export.Group.CheckIndex();

    if (local.Total.Amount == 0)
    {
      export.Group.Count = 0;
      export.Group.Index = -1;

      return;
    }

    if (local.Total.Amount < import.AmtToDistribute.TotalCurrency)
    {
      for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
        export.Group.Index)
      {
        if (!export.Group.CheckSize())
        {
          break;
        }

        if (IsEmpty(export.Group.Item.Collection.CourtOrderAppliedTo))
        {
          export.Group.Update.Collection.Amount =
            export.Group.Item.Collection.Amount + (
              import.AmtToDistribute.TotalCurrency - local.Total.Amount);

          return;
        }
      }

      export.Group.CheckIndex();

      export.Group.Index = export.Group.Count;
      export.Group.CheckSize();

      export.Group.Update.Collection.Amount =
        import.AmtToDistribute.TotalCurrency - local.Total.Amount;
      export.Group.Update.Collection.CourtOrderAppliedTo = "";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.Amount = source.Amount;
    target.CourtOrderAppliedTo = source.CourtOrderAppliedTo;
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistant.SequentialIdentifier);
        db.SetInt32(command, "crvId", import.Persistant.CrvIdentifier);
        db.SetInt32(command, "cstId", import.Persistant.CstIdentifier);
        db.SetInt32(command, "crtType", import.Persistant.CrtIdentifier);
        db.SetDate(
          command, "collAdjDt",
          local.Hold.CollectionAdjustmentDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.CarId = db.GetNullableInt32(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 14);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 16);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 17);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.ExistingCollection.DistributionMethod);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistant.SequentialIdentifier);
        db.SetInt32(command, "crvId", import.Persistant.CrvIdentifier);
        db.SetInt32(command, "cstId", import.Persistant.CstIdentifier);
        db.SetInt32(command, "crtType", import.Persistant.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.CarId = db.GetNullableInt32(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 14);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 16);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 17);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.ExistingCollection.DistributionMethod);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(import.Persistant.Populated);
    entities.ExistingCollection.Populated = false;

    return ReadEach("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistant.SequentialIdentifier);
        db.SetInt32(command, "crvId", import.Persistant.CrvIdentifier);
        db.SetInt32(command, "cstId", import.Persistant.CstIdentifier);
        db.SetInt32(command, "crtType", import.Persistant.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.ConcurrentInd = db.GetString(reader, 2);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 3);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 5);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 6);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 7);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 8);
        entities.ExistingCollection.CpaType = db.GetString(reader, 9);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 10);
        entities.ExistingCollection.OtrType = db.GetString(reader, 11);
        entities.ExistingCollection.CarId = db.GetNullableInt32(reader, 12);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 13);
        entities.ExistingCollection.CollectionAdjustmentDt =
          db.GetDate(reader, 14);
        entities.ExistingCollection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.ExistingCollection.Amount = db.GetDecimal(reader, 16);
        entities.ExistingCollection.DistributionMethod =
          db.GetString(reader, 17);
        entities.ExistingCollection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 18);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.ExistingCollection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
        CheckValid<Collection>("DistributionMethod",
          entities.ExistingCollection.DistributionMethod);

        return true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCollection.Populated);
    entities.ExistingCollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId1",
          entities.ExistingCollection.CarId.GetValueOrDefault());
        db.SetInt32(
          command, "obTrnRlnRsnId2",
          import.HardcodedWrongAcct.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingCollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollectionAdjustmentReason.Populated = true;
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
    /// A value of AmtToDistribute.
    /// </summary>
    [JsonPropertyName("amtToDistribute")]
    public Common AmtToDistribute
    {
      get => amtToDistribute ??= new();
      set => amtToDistribute = value;
    }

    /// <summary>
    /// A value of Persistant.
    /// </summary>
    [JsonPropertyName("persistant")]
    public CashReceiptDetail Persistant
    {
      get => persistant ??= new();
      set => persistant = value;
    }

    /// <summary>
    /// A value of HardcodedWrongAcct.
    /// </summary>
    [JsonPropertyName("hardcodedWrongAcct")]
    public CollectionAdjustmentReason HardcodedWrongAcct
    {
      get => hardcodedWrongAcct ??= new();
      set => hardcodedWrongAcct = value;
    }

    private Common amtToDistribute;
    private CashReceiptDetail persistant;
    private CollectionAdjustmentReason hardcodedWrongAcct;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Collection.
      /// </summary>
      [JsonPropertyName("collection")]
      public Collection Collection
      {
        get => collection ??= new();
        set => collection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private Collection collection;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AdjustmentFoundInd.
    /// </summary>
    [JsonPropertyName("adjustmentFoundInd")]
    public Common AdjustmentFoundInd
    {
      get => adjustmentFoundInd ??= new();
      set => adjustmentFoundInd = value;
    }

    /// <summary>
    /// A value of TotalReapplied.
    /// </summary>
    [JsonPropertyName("totalReapplied")]
    public Collection TotalReapplied
    {
      get => totalReapplied ??= new();
      set => totalReapplied = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public Collection Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Collection Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Collection Total
    {
      get => total ??= new();
      set => total = value;
    }

    private Common adjustmentFoundInd;
    private Collection totalReapplied;
    private Collection hold;
    private Collection null1;
    private Collection total;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("existingCollectionAdjustmentReason")]
    public CollectionAdjustmentReason ExistingCollectionAdjustmentReason
    {
      get => existingCollectionAdjustmentReason ??= new();
      set => existingCollectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    private CollectionAdjustmentReason existingCollectionAdjustmentReason;
    private Collection existingCollection;
  }
#endregion
}

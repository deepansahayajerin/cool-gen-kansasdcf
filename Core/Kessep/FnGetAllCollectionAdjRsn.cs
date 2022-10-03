// Program: FN_GET_ALL_COLLECTION_ADJ_RSN, ID: 372378452, model: 746.
// Short name: SWE00463
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_GET_ALL_COLLECTION_ADJ_RSN.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnGetAllCollectionAdjRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_ALL_COLLECTION_ADJ_RSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetAllCollectionAdjRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetAllCollectionAdjRsn.
  /// </summary>
  public FnGetAllCollectionAdjRsn(IContext context, Import import, Export export)
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
    // *****************************************************
    // A.Kinney	04/28/97	Changed Current_date
    // *****************************************************
    local.Current.Date = Now().Date;

    if (AsChar(import.ShowHistory.SelectChar) == 'Y')
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCollectionAdjustmentReason2())
      {
        export.Export1.Update.CollectionAdjustmentReason.Assign(
          entities.CollectionAdjustmentReason);
        local.DateWorkArea.Date =
          entities.CollectionAdjustmentReason.DiscontinueDt;
        export.Export1.Update.CollectionAdjustmentReason.DiscontinueDt =
          UseCabSetMaximumDiscontinueDate();
        export.Export1.Next();
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCollectionAdjustmentReason1())
      {
        export.Export1.Update.CollectionAdjustmentReason.Assign(
          entities.CollectionAdjustmentReason);
        local.DateWorkArea.Date =
          entities.CollectionAdjustmentReason.DiscontinueDt;
        export.Export1.Update.CollectionAdjustmentReason.DiscontinueDt =
          UseCabSetMaximumDiscontinueDate();
        export.Export1.Next();
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadCollectionAdjustmentReason1()
  {
    return ReadEach("ReadCollectionAdjustmentReason1",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", import.Starting.Code);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.EffectiveDt = db.GetDate(reader, 3);
        entities.CollectionAdjustmentReason.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.CollectionAdjustmentReason.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionAdjustmentReason2()
  {
    return ReadEach("ReadCollectionAdjustmentReason2",
      (db, command) =>
      {
        db.SetString(command, "obTrnRlnRsnCd", import.Starting.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.EffectiveDt = db.GetDate(reader, 3);
        entities.CollectionAdjustmentReason.DiscontinueDt =
          db.GetNullableDate(reader, 4);
        entities.CollectionAdjustmentReason.Populated = true;

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
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public CollectionAdjustmentReason Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private Common showHistory;
    private CollectionAdjustmentReason starting;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of Import.
      /// </summary>
      [JsonPropertyName("import")]
      public Common Import
      {
        get => import ??= new();
        set => import = value;
      }

      /// <summary>
      /// A value of CollectionAdjustmentReason.
      /// </summary>
      [JsonPropertyName("collectionAdjustmentReason")]
      public CollectionAdjustmentReason CollectionAdjustmentReason
      {
        get => collectionAdjustmentReason ??= new();
        set => collectionAdjustmentReason = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 52;

      private Common import;
      private CollectionAdjustmentReason collectionAdjustmentReason;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private Array<ExportGroup> export1;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    private CollectionAdjustmentReason collectionAdjustmentReason;
  }
#endregion
}

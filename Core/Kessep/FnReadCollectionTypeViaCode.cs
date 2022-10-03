// Program: FN_READ_COLLECTION_TYPE_VIA_CODE, ID: 372566792, model: 746.
// Short name: SWE00542
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_COLLECTION_TYPE_VIA_CODE.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block reads an occurrence of the Collection Type entity 
/// based on the imported code and AS_OF date.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCollectionTypeViaCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_COLLECTION_TYPE_VIA_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCollectionTypeViaCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCollectionTypeViaCode.
  /// </summary>
  public FnReadCollectionTypeViaCode(IContext context, Import import,
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
    if (ReadCollectionType())
    {
      ++export.ImportNumberOfReads.Count;
      export.CollectionType.Assign(entities.CollectionType);
    }
    else
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF";
    }
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CollectionType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.Date.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.PrintName = db.GetNullableString(reader, 1);
        entities.CollectionType.Code = db.GetString(reader, 2);
        entities.CollectionType.Name = db.GetString(reader, 3);
        entities.CollectionType.CashNonCashInd = db.GetString(reader, 4);
        entities.CollectionType.DisbursementInd = db.GetString(reader, 5);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 6);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.CollectionType.CreatedBy = db.GetString(reader, 8);
        entities.CollectionType.CreatedTmst = db.GetDateTime(reader, 9);
        entities.CollectionType.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.CollectionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 11);
        entities.CollectionType.Description = db.GetNullableString(reader, 12);
        entities.CollectionType.Populated = true;
        CheckValid<CollectionType>("CashNonCashInd",
          entities.CollectionType.CashNonCashInd);
        CheckValid<CollectionType>("DisbursementInd",
          entities.CollectionType.DisbursementInd);
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public CollectionType Date
    {
      get => date ??= new();
      set => date = value;
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

    private CollectionType date;
    private CollectionType collectionType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private Common importNumberOfReads;
    private CollectionType collectionType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}

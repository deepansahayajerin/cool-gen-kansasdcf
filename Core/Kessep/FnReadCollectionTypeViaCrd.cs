// Program: FN_READ_COLLECTION_TYPE_VIA_CRD, ID: 372532812, model: 746.
// Short name: SWE00543
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_COLLECTION_TYPE_VIA_CRD.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block reads an occurrence of the Collection_Type entity 
/// based on the imported Cash Receipt Detail.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCollectionTypeViaCrd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_COLLECTION_TYPE_VIA_CRD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCollectionTypeViaCrd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCollectionTypeViaCrd.
  /// </summary>
  public FnReadCollectionTypeViaCrd(IContext context, Import import,
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
    if (!import.Persistent.Populated)
    {
      if (!ReadCashReceiptDetail())
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }
    }

    if (ReadCollectionType())
    {
      export.CollectionType.Assign(entities.CollectionType);
    }
    else
    {
      ExitState = "FN0000_COLLECTION_TYPE_NF";
    }
  }

  private bool ReadCashReceiptDetail()
  {
    import.Persistent.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.Persistent.CrvIdentifier = db.GetInt32(reader, 0);
        import.Persistent.CstIdentifier = db.GetInt32(reader, 1);
        import.Persistent.CrtIdentifier = db.GetInt32(reader, 2);
        import.Persistent.SequentialIdentifier = db.GetInt32(reader, 3);
        import.Persistent.CltIdentifier = db.GetNullableInt32(reader, 4);
        import.Persistent.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          import.Persistent.CltIdentifier.GetValueOrDefault());
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CashReceiptDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptDetail persistent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    private CashReceipt cashReceipt;
    private CollectionType collectionType;
  }
#endregion
}

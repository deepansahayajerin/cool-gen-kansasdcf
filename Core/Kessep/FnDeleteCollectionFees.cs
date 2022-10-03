// Program: FN_DELETE_COLLECTION_FEES, ID: 371774292, model: 746.
// Short name: SWE02245
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_COLLECTION_FEES.
/// </summary>
[Serializable]
public partial class FnDeleteCollectionFees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_COLLECTION_FEES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteCollectionFees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteCollectionFees.
  /// </summary>
  public FnDeleteCollectionFees(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 10/13/98	Sunya Sharp				Initial Creation.
    // ---------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCashReceiptDetailFee())
    {
      DeleteCashReceiptDetailFee();
    }
    else
    {
      ExitState = "FN0130_CASH_RCPT_DTL_FEES_NF";
    }
  }

  private void DeleteCashReceiptDetailFee()
  {
    Update("DeleteCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetailFee.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailFee.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailFee.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailFee.CrtIdentifier);
        db.SetDateTime(
          command, "crdetailFeeId",
          entities.CashReceiptDetailFee.SystemGeneratedIdentifier.
            GetValueOrDefault());
      });
  }

  private bool ReadCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return Read("ReadCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", import.P.SequentialIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetDateTime(
          command, "crdetailFeeId",
          import.CashReceiptDetailFee.SystemGeneratedIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Amount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetailFee.CreatedBy = db.GetString(reader, 6);
        entities.CashReceiptDetailFee.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptDetailFee.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetailFee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceiptDetailFee.Populated = true;
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
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CashReceiptDetail P
    {
      get => p ??= new();
      set => p = value;
    }

    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetail p;
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
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    private CashReceiptDetailFee cashReceiptDetailFee;
  }
#endregion
}

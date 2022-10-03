// Program: SI_DELETE_INTERSTATE_ORDER, ID: 373441256, model: 746.
// Short name: SWE02752
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_DELETE_INTERSTATE_ORDER.
/// </summary>
[Serializable]
public partial class SiDeleteInterstateOrder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_INTERSTATE_ORDER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteInterstateOrder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteInterstateOrder.
  /// </summary>
  public SiDeleteInterstateOrder(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadInterstateSupportOrder())
    {
      DeleteInterstateSupportOrder();
    }
    else if (ReadInterstateCase())
    {
      // -----------------------------------------------------------
      // Attempting to DELETE and it doesn't exist - not an error
      // -----------------------------------------------------------
    }
    else
    {
      ExitState = "INTERSTATE_CASE_NF";
    }
  }

  private void DeleteInterstateSupportOrder()
  {
    Update("DeleteInterstateSupportOrder",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateSupportOrder.CcaTransactionDt.GetValueOrDefault());
          
        db.SetInt32(
          command, "sysGeneratedId",
          entities.InterstateSupportOrder.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTranSerNum",
          entities.InterstateSupportOrder.CcaTranSerNum);
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateSupportOrder()
  {
    entities.InterstateSupportOrder.Populated = false;

    return Read("ReadInterstateSupportOrder",
      (db, command) =>
      {
        db.SetInt32(
          command, "sysGeneratedId",
          import.InterstateSupportOrder.SystemGeneratedSequenceNum);
        db.SetInt64(
          command, "ccaTranSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateSupportOrder.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateSupportOrder.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 1);
        entities.InterstateSupportOrder.CcaTranSerNum = db.GetInt64(reader, 2);
        entities.InterstateSupportOrder.Populated = true;
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
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateSupportOrder interstateSupportOrder;
    private InterstateCase interstateCase;
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
    /// A value of InterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("interstateSupportOrder")]
    public InterstateSupportOrder InterstateSupportOrder
    {
      get => interstateSupportOrder ??= new();
      set => interstateSupportOrder = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateSupportOrder interstateSupportOrder;
    private InterstateCase interstateCase;
  }
#endregion
}

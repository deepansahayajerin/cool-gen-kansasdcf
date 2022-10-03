// Program: FN_UPDATE_URA_EXCESS_COLLECTION, ID: 372551169, model: 746.
// Short name: SWE02014
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_URA_EXCESS_COLLECTION.
/// </summary>
[Serializable]
public partial class FnUpdateUraExcessCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_URA_EXCESS_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateUraExcessCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateUraExcessCollection.
  /// </summary>
  public FnUpdateUraExcessCollection(IContext context, Import import,
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
    // -------------------------------------------------
    // Initial Version - SWSRKXD
    // Date - 3/31/99
    // -------------------------------------------------
    // ---------------------------------------------------
    // WARNING!!!
    //  All attribues of URA_Excess_Collection must be fed
    // into the CAB since UPDATE statement sets them to import.
    // ---------------------------------------------------
    if (!ReadUraExcessCollection())
    {
      ExitState = "FN0000_URA_EXCESS_COLLECTION_NF";

      return;
    }

    try
    {
      UpdateUraExcessCollection();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_URA_EXCESS_COLLECTION_NU";

          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private bool ReadUraExcessCollection()
  {
    entities.UraExcessCollection.Populated = false;

    return Read("ReadUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "seqNumber", import.UraExcessCollection.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.UraExcessCollection.SequenceNumber = db.GetInt32(reader, 0);
        entities.UraExcessCollection.Month = db.GetInt32(reader, 1);
        entities.UraExcessCollection.Year = db.GetInt32(reader, 2);
        entities.UraExcessCollection.Amount = db.GetDecimal(reader, 3);
        entities.UraExcessCollection.Type1 = db.GetString(reader, 4);
        entities.UraExcessCollection.Action = db.GetString(reader, 5);
        entities.UraExcessCollection.ActionImHousehold =
          db.GetString(reader, 6);
        entities.UraExcessCollection.SupplyingCsePerson =
          db.GetString(reader, 7);
        entities.UraExcessCollection.InitiatingCollection =
          db.GetInt32(reader, 8);
        entities.UraExcessCollection.ReceivingCsePerson =
          db.GetNullableString(reader, 9);
        entities.UraExcessCollection.InitiatingCsePerson =
          db.GetString(reader, 10);
        entities.UraExcessCollection.InitiatingImHousehold =
          db.GetString(reader, 11);
        entities.UraExcessCollection.Populated = true;
      });
  }

  private void UpdateUraExcessCollection()
  {
    var month = import.UraExcessCollection.Month;
    var year = import.UraExcessCollection.Year;
    var amount = import.UraExcessCollection.Amount;
    var type1 = import.UraExcessCollection.Type1;
    var action1 = import.UraExcessCollection.Action;
    var actionImHousehold = import.UraExcessCollection.ActionImHousehold;
    var supplyingCsePerson = import.UraExcessCollection.SupplyingCsePerson;
    var initiatingCollection = import.UraExcessCollection.InitiatingCollection;
    var receivingCsePerson = import.UraExcessCollection.ReceivingCsePerson ?? ""
      ;
    var initiatingCsePerson = import.UraExcessCollection.InitiatingCsePerson;
    var initiatingImHousehold =
      import.UraExcessCollection.InitiatingImHousehold;

    entities.UraExcessCollection.Populated = false;
    Update("UpdateUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(command, "collectionMonth", month);
        db.SetInt32(command, "collectionYear", year);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "type", type1);
        db.SetString(command, "collectionAction", action1);
        db.SetString(command, "actImHousehold", actionImHousehold);
        db.SetString(command, "supplyingCsePer", supplyingCsePerson);
        db.SetInt32(command, "initiatingColl", initiatingCollection);
        db.SetNullableString(command, "recvCsePerson", receivingCsePerson);
        db.SetString(command, "initiatingCsePer", initiatingCsePerson);
        db.SetString(command, "initiateImHhold", initiatingImHousehold);
        db.SetInt32(
          command, "seqNumber", entities.UraExcessCollection.SequenceNumber);
      });

    entities.UraExcessCollection.Month = month;
    entities.UraExcessCollection.Year = year;
    entities.UraExcessCollection.Amount = amount;
    entities.UraExcessCollection.Type1 = type1;
    entities.UraExcessCollection.Action = action1;
    entities.UraExcessCollection.ActionImHousehold = actionImHousehold;
    entities.UraExcessCollection.SupplyingCsePerson = supplyingCsePerson;
    entities.UraExcessCollection.InitiatingCollection = initiatingCollection;
    entities.UraExcessCollection.ReceivingCsePerson = receivingCsePerson;
    entities.UraExcessCollection.InitiatingCsePerson = initiatingCsePerson;
    entities.UraExcessCollection.InitiatingImHousehold = initiatingImHousehold;
    entities.UraExcessCollection.Populated = true;
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private UraExcessCollection uraExcessCollection;
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private UraExcessCollection uraExcessCollection;
  }
#endregion
}

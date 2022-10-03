// Program: FN_CREATE_URA_EXCESS_COLLECTION, ID: 372551164, model: 746.
// Short name: SWE02012
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_URA_EXCESS_COLLECTION.
/// </summary>
[Serializable]
public partial class FnCreateUraExcessCollection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_URA_EXCESS_COLLECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateUraExcessCollection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateUraExcessCollection.
  /// </summary>
  public FnCreateUraExcessCollection(IContext context, Import import,
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
    // -------------------------------------------------
    // Description
    // This Action Block will create a URA_Excess_Collection
    // record and associate it with import IM_Household.
    // NB :- All validation is assumed to have been performed by
    // calling CAB.  Please ensure that IM_Household.ae_case_no
    // is fed into this CAB.
    // -------------------------------------------------
    if (!ReadImHousehold())
    {
      ExitState = "IM_HOUSEHOLD_NF";

      return;
    }

    // -------------------------------------------------
    // SWSRKXD - 6/30/99
    // Replace READ EACH with Summarize at the suggestion of Carl Galka.
    // -------------------------------------------------
    ReadUraExcessCollection();

    try
    {
      CreateUraExcessCollection();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          // -------------------------------------------------
          // SWSRKXD - 6/30/99
          // Set correct ES.
          // -------------------------------------------------
          ExitState = "URA_EXCESS_COLLECTION_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    export.UraExcessCollection.SequenceNumber =
      entities.UraExcessCollection.SequenceNumber;
  }

  private void CreateUraExcessCollection()
  {
    var sequenceNumber = local.UraExcessCollection.SequenceNumber + 1;
    var month = import.UraExcessCollection.Month;
    var year = import.UraExcessCollection.Year;
    var amount = import.UraExcessCollection.Amount;
    var type1 = import.UraExcessCollection.Type1;
    var action1 = import.UraExcessCollection.Action;
    var actionImHousehold = import.UraExcessCollection.ActionImHousehold;
    var supplyingCsePerson = import.UraExcessCollection.SupplyingCsePerson;
    var initiatingCollection = import.UraExcessCollection.InitiatingCollection;
    var imhAeCaseNo = entities.ImHousehold.AeCaseNo;
    var receivingCsePerson = import.UraExcessCollection.ReceivingCsePerson ?? ""
      ;
    var initiatingCsePerson = import.UraExcessCollection.InitiatingCsePerson;
    var initiatingImHousehold =
      import.UraExcessCollection.InitiatingImHousehold;

    entities.UraExcessCollection.Populated = false;
    Update("CreateUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(command, "seqNumber", sequenceNumber);
        db.SetInt32(command, "collectionMonth", month);
        db.SetInt32(command, "collectionYear", year);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "type", type1);
        db.SetString(command, "collectionAction", action1);
        db.SetString(command, "actImHousehold", actionImHousehold);
        db.SetString(command, "supplyingCsePer", supplyingCsePerson);
        db.SetInt32(command, "initiatingColl", initiatingCollection);
        db.SetNullableString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetNullableString(command, "recvCsePerson", receivingCsePerson);
        db.SetString(command, "initiatingCsePer", initiatingCsePerson);
        db.SetString(command, "initiateImHhold", initiatingImHousehold);
      });

    entities.UraExcessCollection.SequenceNumber = sequenceNumber;
    entities.UraExcessCollection.Month = month;
    entities.UraExcessCollection.Year = year;
    entities.UraExcessCollection.Amount = amount;
    entities.UraExcessCollection.Type1 = type1;
    entities.UraExcessCollection.Action = action1;
    entities.UraExcessCollection.ActionImHousehold = actionImHousehold;
    entities.UraExcessCollection.SupplyingCsePerson = supplyingCsePerson;
    entities.UraExcessCollection.InitiatingCollection = initiatingCollection;
    entities.UraExcessCollection.ImhAeCaseNo = imhAeCaseNo;
    entities.UraExcessCollection.ReceivingCsePerson = receivingCsePerson;
    entities.UraExcessCollection.InitiatingCsePerson = initiatingCsePerson;
    entities.UraExcessCollection.InitiatingImHousehold = initiatingImHousehold;
    entities.UraExcessCollection.Populated = true;
  }

  private bool ReadImHousehold()
  {
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(command, "aeCaseNo", import.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadUraExcessCollection()
  {
    local.UraExcessCollection.Populated = false;

    return Read("ReadUraExcessCollection",
      null,
      (db, reader) =>
      {
        local.UraExcessCollection.SequenceNumber = db.GetInt32(reader, 0);
        local.UraExcessCollection.Populated = true;
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private ImHousehold imHousehold;
    private UraExcessCollection uraExcessCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private UraExcessCollection uraExcessCollection;
    private ImHousehold imHousehold;
  }
#endregion
}

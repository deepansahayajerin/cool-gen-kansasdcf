// Program: LE_TRIB_DELETE_TRIBUNAL, ID: 372021820, model: 746.
// Short name: SWE00822
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_TRIB_DELETE_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes Tribunal and associated entity occurrences
/// </para>
/// </summary>
[Serializable]
public partial class LeTribDeleteTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_DELETE_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribDeleteTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribDeleteTribunal.
  /// </summary>
  public LeTribDeleteTribunal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *************************************************************
    // when          who          ref      description
    // *************************************************************
    // 05/09/2007   Gloria        PR288534  Modified to disallow delete
    //                                      
    // where tribunal_id is equal to
    //                                      
    // kpc_tribunal_id.
    // *************************************************************
    if (!ReadTribunal())
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    if (ReadLegalAction2())
    {
      ExitState = "LE0000_LEG_ACT_PREVENTS_DEL_TRIB";

      return;
    }

    if (ReadLegalAction1())
    {
      ExitState = "LE0000_LEG_ACT_PRVNTDEL_KPC_TRIB";

      return;
    }

    if (ReadAppeal())
    {
      ExitState = "LE0000_APPEAL_PREVENTS_DEL_TRIB";

      return;
    }

    if (ReadTribunalFeeInformation())
    {
      ExitState = "LE0000_FEE_INFO_PREVENT_DEL_TRIB";

      return;
    }

    foreach(var item in ReadFipsTribAddress())
    {
      if (ReadFips())
      {
        // --- Do not delete the address.
        DisassociateFipsTribAddress();
      }
      else
      {
        // --- This address has no FIPS associated with it.
        DeleteFipsTribAddress();
      }
    }

    DeleteTribunal();
  }

  private void DeleteFipsTribAddress()
  {
    Update("DeleteFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ExistingFipsTribAddress.Identifier);
      });
  }

  private void DeleteTribunal()
  {
    bool exists;

    exists = Read("DeleteTribunal#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_APPEAL\".", "50001");
    }

    Update("DeleteTribunal#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      });

    Update("DeleteTribunal#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      });
  }

  private void DisassociateFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;
    Update("DisassociateFipsTribAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", entities.ExistingFipsTribAddress.Identifier);
      });

    entities.ExistingFipsTribAddress.TrbId = null;
    entities.ExistingFipsTribAddress.Populated = true;
  }

  private bool ReadAppeal()
  {
    entities.ExistingAppeal.Populated = false;

    return Read("ReadAppeal",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingAppeal.Identifier = db.GetInt32(reader, 0);
        entities.ExistingAppeal.TrbId = db.GetNullableInt32(reader, 1);
        entities.ExistingAppeal.Populated = true;
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingFipsTribAddress.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingFipsTribAddress.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingFipsTribAddress.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingFipsTribAddress.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    entities.ExistingFipsTribAddress.Populated = false;

    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 1);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 3);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.TrbId = db.GetNullableInt32(reader, 5);
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "kpcTribunalId", import.Tribunal.Identifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.ExistingLegalAction.KpcTribunalId =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalAction2()
  {
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 1);
        entities.ExistingLegalAction.KpcTribunalId =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 2);
        entities.ExistingTribunal.Populated = true;
      });
  }

  private bool ReadTribunalFeeInformation()
  {
    entities.ExistingTribunalFeeInformation.Populated = false;

    return Read("ReadTribunalFeeInformation",
      (db, command) =>
      {
        db.SetInt32(command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunalFeeInformation.TrbId = db.GetInt32(reader, 0);
        entities.ExistingTribunalFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingTribunalFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingTribunalFeeInformation.Populated = true;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Tribunal tribunal;
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
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
    }

    /// <summary>
    /// A value of ExistingTribunalFeeInformation.
    /// </summary>
    [JsonPropertyName("existingTribunalFeeInformation")]
    public TribunalFeeInformation ExistingTribunalFeeInformation
    {
      get => existingTribunalFeeInformation ??= new();
      set => existingTribunalFeeInformation = value;
    }

    /// <summary>
    /// A value of ExistingAppeal.
    /// </summary>
    [JsonPropertyName("existingAppeal")]
    public Appeal ExistingAppeal
    {
      get => existingAppeal ??= new();
      set => existingAppeal = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    private Fips existingFips;
    private TribunalFeeInformation existingTribunalFeeInformation;
    private Appeal existingAppeal;
    private LegalAction existingLegalAction;
    private FipsTribAddress existingFipsTribAddress;
    private Tribunal existingTribunal;
  }
#endregion
}

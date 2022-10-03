// Program: LE_DELETE_HEARING_ADDRESS, ID: 372582883, model: 746.
// Short name: SWE00754
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_HEARING_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes the Hearing Address for a selected Hearing.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteHearingAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_HEARING_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteHearingAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteHearingAddress.
  /// </summary>
  public LeDeleteHearingAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 11/21/98	R. Jean			        Eliminate read of HEARING
    // ------------------------------------------------------------
    if (ReadHearingAddress())
    {
      DeleteHearingAddress();
    }
    else
    {
      ExitState = "HEARING_ADDRESS_NF";
    }
  }

  private void DeleteHearingAddress()
  {
    var hrgGeneratedId = entities.HearingAddress.HrgGeneratedId;
    bool exists;

    Update("DeleteHearingAddress#1",
      (db, command) =>
      {
        db.SetInt32(command, "hrgGeneratedId1", hrgGeneratedId);
        db.SetString(command, "type", entities.HearingAddress.Type1);
      });

    exists = Read("DeleteHearingAddress#2",
      (db, command) =>
      {
        db.SetInt32(command, "hrgGeneratedId2", hrgGeneratedId);
      },
      null);

    if (!exists)
    {
      Update("DeleteHearingAddress#3",
        (db, command) =>
        {
          db.SetInt32(command, "hrgGeneratedId2", hrgGeneratedId);
        });
    }
  }

  private bool ReadHearingAddress()
  {
    entities.HearingAddress.Populated = false;

    return Read("ReadHearingAddress",
      (db, command) =>
      {
        db.SetString(command, "type", import.HearingAddress.Type1);
        db.SetInt32(
          command, "hrgGeneratedId", import.Hearing.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.HearingAddress.HrgGeneratedId = db.GetInt32(reader, 0);
        entities.HearingAddress.Type1 = db.GetString(reader, 1);
        entities.HearingAddress.Location = db.GetNullableString(reader, 2);
        entities.HearingAddress.Street1 = db.GetString(reader, 3);
        entities.HearingAddress.Street2 = db.GetNullableString(reader, 4);
        entities.HearingAddress.City = db.GetString(reader, 5);
        entities.HearingAddress.StateProvince = db.GetString(reader, 6);
        entities.HearingAddress.County = db.GetNullableString(reader, 7);
        entities.HearingAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.HearingAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.HearingAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.HearingAddress.CreatedBy = db.GetString(reader, 11);
        entities.HearingAddress.CreatedTstamp = db.GetDateTime(reader, 12);
        entities.HearingAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.HearingAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 14);
        entities.HearingAddress.Populated = true;
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
    /// A value of HearingAddress.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    public HearingAddress HearingAddress
    {
      get => hearingAddress ??= new();
      set => hearingAddress = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private HearingAddress hearingAddress;
    private Hearing hearing;
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
    /// A value of HearingAddress.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    public HearingAddress HearingAddress
    {
      get => hearingAddress ??= new();
      set => hearingAddress = value;
    }

    /// <summary>
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    private HearingAddress hearingAddress;
    private Hearing hearing;
  }
#endregion
}

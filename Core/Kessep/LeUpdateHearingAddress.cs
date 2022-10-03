// Program: LE_UPDATE_HEARING_ADDRESS, ID: 372582886, model: 746.
// Short name: SWE00830
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_UPDATE_HEARING_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block updates Hearing Addresses.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateHearingAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_HEARING_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateHearingAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateHearingAddress.
  /// </summary>
  public LeUpdateHearingAddress(IContext context, Import import, Export export):
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
    // 06/30/95	Stephen Benton			Initial Code
    // ------------------------------------------------------------
    if (ReadFips())
    {
      export.Fips.Assign(entities.Existing);
    }

    if (ReadHearingAddress())
    {
      try
      {
        UpdateHearingAddress();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEARING_ADDRESS_NU";

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
    else
    {
      UseLeCreateHearingAddress();
    }
  }

  private void UseLeCreateHearingAddress()
  {
    var useImport = new LeCreateHearingAddress.Import();
    var useExport = new LeCreateHearingAddress.Export();

    useImport.Hearing.SystemGeneratedIdentifier =
      import.Hearing.SystemGeneratedIdentifier;
    useImport.HearingAddress.Assign(import.HearingAddress);

    Call(LeCreateHearingAddress.Execute, useImport, useExport);
  }

  private bool ReadFips()
  {
    entities.Existing.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetString(
          command, "stateAbbreviation", import.HearingAddress.StateProvince);
        db.SetNullableString(
          command, "countyAbbr", import.HearingAddress.County ?? "");
      },
      (db, reader) =>
      {
        entities.Existing.State = db.GetInt32(reader, 0);
        entities.Existing.County = db.GetInt32(reader, 1);
        entities.Existing.Location = db.GetInt32(reader, 2);
        entities.Existing.StateDescription = db.GetNullableString(reader, 3);
        entities.Existing.CountyDescription = db.GetNullableString(reader, 4);
        entities.Existing.StateAbbreviation = db.GetString(reader, 5);
        entities.Existing.CountyAbbreviation = db.GetNullableString(reader, 6);
        entities.Existing.Populated = true;
      });
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
        entities.HearingAddress.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.HearingAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 12);
        entities.HearingAddress.Populated = true;
      });
  }

  private void UpdateHearingAddress()
  {
    System.Diagnostics.Debug.Assert(entities.HearingAddress.Populated);

    var location = import.HearingAddress.Location ?? "";
    var street1 = import.HearingAddress.Street1;
    var street2 = import.HearingAddress.Street2 ?? "";
    var city = import.HearingAddress.City;
    var stateProvince = import.HearingAddress.StateProvince;
    var county = import.HearingAddress.County ?? "";
    var zipCode = import.HearingAddress.ZipCode ?? "";
    var zip4 = import.HearingAddress.Zip4 ?? "";
    var zip3 = import.HearingAddress.Zip3 ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.HearingAddress.Populated = false;
    Update("UpdateHearingAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "location", location);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetString(command, "stateProvince", stateProvince);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "zipCd", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(
          command, "hrgGeneratedId", entities.HearingAddress.HrgGeneratedId);
        db.SetString(command, "type", entities.HearingAddress.Type1);
      });

    entities.HearingAddress.Location = location;
    entities.HearingAddress.Street1 = street1;
    entities.HearingAddress.Street2 = street2;
    entities.HearingAddress.City = city;
    entities.HearingAddress.StateProvince = stateProvince;
    entities.HearingAddress.County = county;
    entities.HearingAddress.ZipCode = zipCode;
    entities.HearingAddress.Zip4 = zip4;
    entities.HearingAddress.Zip3 = zip3;
    entities.HearingAddress.LastUpdatedBy = lastUpdatedBy;
    entities.HearingAddress.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.HearingAddress.Populated = true;
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
    /// A value of Hearing.
    /// </summary>
    [JsonPropertyName("hearing")]
    public Hearing Hearing
    {
      get => hearing ??= new();
      set => hearing = value;
    }

    /// <summary>
    /// A value of HearingAddress.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    public HearingAddress HearingAddress
    {
      get => hearingAddress ??= new();
      set => hearingAddress = value;
    }

    private Hearing hearing;
    private HearingAddress hearingAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    private Fips fips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Fips Existing
    {
      get => existing ??= new();
      set => existing = value;
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

    /// <summary>
    /// A value of HearingAddress.
    /// </summary>
    [JsonPropertyName("hearingAddress")]
    public HearingAddress HearingAddress
    {
      get => hearingAddress ??= new();
      set => hearingAddress = value;
    }

    private Fips existing;
    private Hearing hearing;
    private HearingAddress hearingAddress;
  }
#endregion
}

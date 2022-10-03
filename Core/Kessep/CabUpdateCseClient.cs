// Program: CAB_UPDATE_CSE_CLIENT, ID: 372254683, model: 746.
// Short name: SWE01561
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_UPDATE_CSE_CLIENT.
/// </summary>
[Serializable]
public partial class CabUpdateCseClient: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_UPDATE_CSE_CLIENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabUpdateCseClient(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabUpdateCseClient.
  /// </summary>
  public CabUpdateCseClient(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadClient())
    {
      try
      {
        UpdateClient();
        export.Client.Assign(entities.Client);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CLIENT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CLIENT_PV";

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
      ExitState = "CLIENT_NF";
    }
  }

  private bool ReadClient()
  {
    entities.Client.Populated = false;

    return Read("ReadClient",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Client.Number);
      },
      (db, reader) =>
      {
        entities.Client.Number = db.GetString(reader, 0);
        entities.Client.Type1 = db.GetString(reader, 1);
        entities.Client.Occupation = db.GetNullableString(reader, 2);
        entities.Client.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.Client.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.Client.IllegalAlienIndicator = db.GetNullableString(reader, 5);
        entities.Client.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.Client.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.Client.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.Client.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.Client.NameMiddle = db.GetNullableString(reader, 10);
        entities.Client.NameMaiden = db.GetNullableString(reader, 11);
        entities.Client.HomePhone = db.GetNullableInt32(reader, 12);
        entities.Client.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.Client.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.Client.CurrentMaritalStatus = db.GetNullableString(reader, 15);
        entities.Client.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.Client.Race = db.GetNullableString(reader, 17);
        entities.Client.HairColor = db.GetNullableString(reader, 18);
        entities.Client.EyeColor = db.GetNullableString(reader, 19);
        entities.Client.Weight = db.GetNullableInt32(reader, 20);
        entities.Client.HeightFt = db.GetNullableInt32(reader, 21);
        entities.Client.HeightIn = db.GetNullableInt32(reader, 22);
        entities.Client.CreatedBy = db.GetString(reader, 23);
        entities.Client.CreatedTimestamp = db.GetDateTime(reader, 24);
        entities.Client.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 25);
        entities.Client.LastUpdatedBy = db.GetNullableString(reader, 26);
        entities.Client.KscaresNumber = db.GetNullableString(reader, 27);
        entities.Client.OtherAreaCode = db.GetNullableInt32(reader, 28);
        entities.Client.EmergencyAreaCode = db.GetNullableInt32(reader, 29);
        entities.Client.HomePhoneAreaCode = db.GetNullableInt32(reader, 30);
        entities.Client.WorkPhoneAreaCode = db.GetNullableInt32(reader, 31);
        entities.Client.WorkPhone = db.GetNullableInt32(reader, 32);
        entities.Client.WorkPhoneExt = db.GetNullableString(reader, 33);
        entities.Client.OtherPhoneType = db.GetNullableString(reader, 34);
        entities.Client.OtherIdInfo = db.GetNullableString(reader, 35);
        entities.Client.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Client.Type1);
      });
  }

  private void UpdateClient()
  {
    var homePhone = import.Client.HomePhone.GetValueOrDefault();
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var homePhoneAreaCode = import.Client.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode = import.Client.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = import.Client.WorkPhone.GetValueOrDefault();

    entities.Client.Populated = false;
    Update("UpdateClient",
      (db, command) =>
      {
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetString(command, "numb", entities.Client.Number);
      });

    entities.Client.HomePhone = homePhone;
    entities.Client.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Client.LastUpdatedBy = lastUpdatedBy;
    entities.Client.HomePhoneAreaCode = homePhoneAreaCode;
    entities.Client.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.Client.WorkPhone = workPhone;
    entities.Client.Populated = true;
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
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    private CsePerson client;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    private CsePerson client;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
    }

    private CsePerson client;
  }
#endregion
}

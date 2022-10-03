// Program: CAB_CREATE_CSE_CLIENT, ID: 372254682, model: 746.
// Short name: SWE01560
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_CREATE_CSE_CLIENT.
/// </summary>
[Serializable]
public partial class CabCreateCseClient: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CREATE_CSE_CLIENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabCreateCseClient(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabCreateCseClient.
  /// </summary>
  public CabCreateCseClient(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    try
    {
      CreateClient();
      export.Client.Assign(entities.Client);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CLIENT_AE";

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

  private void CreateClient()
  {
    var number = import.Client.Number;
    var type1 = "C";
    var homePhone = import.Client.HomePhone.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var homePhoneAreaCode = import.Client.HomePhoneAreaCode.GetValueOrDefault();
    var workPhoneAreaCode = import.Client.WorkPhoneAreaCode.GetValueOrDefault();
    var workPhone = import.Client.WorkPhone.GetValueOrDefault();

    CheckValid<CsePerson>("Type1", type1);
    entities.Client.Populated = false;
    Update("CreateClient",
      (db, command) =>
      {
        db.SetString(command, "numb", number);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "occupation", "");
        db.SetNullableString(command, "aeCaseNumber", "");
        db.SetNullableDate(command, "dateOfDeath", null);
        db.SetNullableString(command, "illegalAlienInd", "");
        db.SetNullableString(command, "currentSpouseMi", "");
        db.SetNullableString(command, "currSpouse1StNm", "");
        db.SetNullableString(command, "birthPlaceState", "");
        db.SetNullableInt32(command, "emergencyPhone", 0);
        db.SetNullableString(command, "nameMiddle", "");
        db.SetNullableString(command, "nameMaiden", "");
        db.SetNullableInt32(command, "homePhone", homePhone);
        db.SetNullableInt32(command, "otherNumber", 0);
        db.SetNullableString(command, "birthPlaceCity", "");
        db.SetNullableString(command, "currMaritalSts", "");
        db.SetNullableString(command, "curSpouseLastNm", "");
        db.SetNullableString(command, "race", "");
        db.SetNullableString(command, "hairColor", "");
        db.SetNullableString(command, "eyeColor", "");
        db.SetNullableString(command, "taxId", "");
        db.SetNullableString(command, "organizationName", "");
        db.SetNullableInt32(command, "weight", 0);
        db.SetNullableInt32(command, "heightFt", 0);
        db.SetNullableInt32(command, "heightIn", 0);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableString(command, "kscaresNumber", "");
        db.SetNullableInt32(command, "otherAreaCode", 0);
        db.SetNullableInt32(command, "emergencyAreaCd", 0);
        db.SetNullableInt32(command, "homePhoneAreaCd", homePhoneAreaCode);
        db.SetNullableInt32(command, "workPhoneAreaCd", workPhoneAreaCode);
        db.SetNullableInt32(command, "workPhone", workPhone);
        db.SetNullableString(command, "workPhoneExt", "");
        db.SetNullableString(command, "otherPhoneType", "");
        db.SetNullableString(command, "unemploymentInd", "");
        db.SetNullableString(command, "taxIdSuffix", "");
        db.SetNullableString(command, "otherIdInfo", "");
        db.SetDate(command, "datePaternEstab", default(DateTime));
        db.SetNullableString(command, "bcFatherLastNm", "");
        db.SetNullableString(command, "bcFatherFirstNm", "");
        db.SetNullableString(command, "fviUpdatedBy", "");
        db.SetNullableString(command, "tribalCode", "");
      });

    entities.Client.Number = number;
    entities.Client.Type1 = type1;
    entities.Client.Occupation = "";
    entities.Client.AeCaseNumber = "";
    entities.Client.DateOfDeath = null;
    entities.Client.IllegalAlienIndicator = "";
    entities.Client.CurrentSpouseMi = "";
    entities.Client.CurrentSpouseFirstName = "";
    entities.Client.BirthPlaceState = "";
    entities.Client.EmergencyPhone = 0;
    entities.Client.NameMiddle = "";
    entities.Client.NameMaiden = "";
    entities.Client.HomePhone = homePhone;
    entities.Client.OtherNumber = 0;
    entities.Client.BirthPlaceCity = "";
    entities.Client.CurrentMaritalStatus = "";
    entities.Client.CurrentSpouseLastName = "";
    entities.Client.Race = "";
    entities.Client.HairColor = "";
    entities.Client.EyeColor = "";
    entities.Client.Weight = 0;
    entities.Client.HeightFt = 0;
    entities.Client.HeightIn = 0;
    entities.Client.CreatedBy = createdBy;
    entities.Client.CreatedTimestamp = createdTimestamp;
    entities.Client.LastUpdatedTimestamp = null;
    entities.Client.LastUpdatedBy = "";
    entities.Client.KscaresNumber = "";
    entities.Client.OtherAreaCode = 0;
    entities.Client.EmergencyAreaCode = 0;
    entities.Client.HomePhoneAreaCode = homePhoneAreaCode;
    entities.Client.WorkPhoneAreaCode = workPhoneAreaCode;
    entities.Client.WorkPhone = workPhone;
    entities.Client.WorkPhoneExt = "";
    entities.Client.OtherPhoneType = "";
    entities.Client.OtherIdInfo = "";
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

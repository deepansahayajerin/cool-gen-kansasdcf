// Program: CREATE_SERV_PROV_PROFILE, ID: 371452063, model: 746.
// Short name: SWE00162
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_SERV_PROV_PROFILE.
/// </summary>
[Serializable]
public partial class CreateServProvProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_SERV_PROV_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateServProvProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateServProvProfile.
  /// </summary>
  public CreateServProvProfile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadProfile())
    {
      ExitState = "SC0015_PROFILE_NF";

      return;
    }

    if (!ReadServiceProvider())
    {
      ExitState = "SC0005_USER_NF";

      return;
    }

    try
    {
      CreateServiceProviderProfile();
      export.ServiceProviderProfile.CreatedTimestamp =
        entities.ExistingServiceProviderProfile.CreatedTimestamp;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0023_SERV_PROVIDER_PROFILE_AE";

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

  private void CreateServiceProviderProfile()
  {
    var createdTimestamp = Now();
    var effectiveDate = import.ServiceProviderProfile.EffectiveDate;
    var discontinueDate = import.ServiceProviderProfile.DiscontinueDate;
    var createdBy = global.UserId;
    var proName = entities.ExistingProfile.Name;
    var spdGenId = entities.ExistingServiceProvider.SystemGeneratedId;

    entities.ExistingServiceProviderProfile.Populated = false;
    Update("CreateServiceProviderProfile",
      (db, command) =>
      {
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTimes", default(DateTime));
        db.SetString(command, "proName", proName);
        db.SetInt32(command, "spdGenId", spdGenId);
      });

    entities.ExistingServiceProviderProfile.CreatedTimestamp = createdTimestamp;
    entities.ExistingServiceProviderProfile.EffectiveDate = effectiveDate;
    entities.ExistingServiceProviderProfile.DiscontinueDate = discontinueDate;
    entities.ExistingServiceProviderProfile.CreatedBy = createdBy;
    entities.ExistingServiceProviderProfile.ProName = proName;
    entities.ExistingServiceProviderProfile.SpdGenId = spdGenId;
    entities.ExistingServiceProviderProfile.Populated = true;
  }

  private bool ReadProfile()
  {
    entities.ExistingProfile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", import.Profile.Name);
      },
      (db, reader) =>
      {
        entities.ExistingProfile.Name = db.GetString(reader, 0);
        entities.ExistingProfile.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", import.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.Populated = true;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private Profile profile;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    private ServiceProviderProfile serviceProviderProfile;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
    }

    private ServiceProviderProfile existingServiceProviderProfile;
    private ServiceProvider existingServiceProvider;
    private Profile existingProfile;
  }
#endregion
}

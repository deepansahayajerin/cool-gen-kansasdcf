// Program: DELETE_SERV_PROV_PROFILE, ID: 371452061, model: 746.
// Short name: SWE00195
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_SERV_PROV_PROFILE.
/// </summary>
[Serializable]
public partial class DeleteServProvProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_SERV_PROV_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteServProvProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteServProvProfile.
  /// </summary>
  public DeleteServProvProfile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadServiceProviderProfile())
    {
      DeleteServiceProviderProfile();
    }
    else
    {
      ExitState = "SC0005_USER_NF";
    }
  }

  private void DeleteServiceProviderProfile()
  {
    Update("DeleteServiceProviderProfile",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ExistingServiceProviderProfile.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(
          command, "proName", entities.ExistingServiceProviderProfile.ProName);
        db.SetInt32(
          command, "spdGenId",
          entities.ExistingServiceProviderProfile.SpdGenId);
      });
  }

  private bool ReadServiceProviderProfile()
  {
    entities.ExistingServiceProviderProfile.Populated = false;

    return Read("ReadServiceProviderProfile",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.ServiceProviderProfile.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "proName", import.Profile.Name);
        db.SetString(command, "userId", import.ServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 1);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 2);
        entities.ExistingServiceProviderProfile.Populated = true;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
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
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
    }

    private Profile profile;
    private ServiceProvider serviceProvider;
    private ServiceProviderProfile serviceProviderProfile;
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
    /// A value of ExistingProfile.
    /// </summary>
    [JsonPropertyName("existingProfile")]
    public Profile ExistingProfile
    {
      get => existingProfile ??= new();
      set => existingProfile = value;
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
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
    }

    private Profile existingProfile;
    private ServiceProvider existingServiceProvider;
    private ServiceProviderProfile existingServiceProviderProfile;
  }
#endregion
}

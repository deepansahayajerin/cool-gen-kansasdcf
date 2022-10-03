// Program: UPDATE_SERV_PROV_PROFILE, ID: 371452060, model: 746.
// Short name: SWE01504
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: UPDATE_SERV_PROV_PROFILE.
/// </summary>
[Serializable]
public partial class UpdateServProvProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_SERV_PROV_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateServProvProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateServProvProfile.
  /// </summary>
  public UpdateServProvProfile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadServiceProviderProfile())
    {
      ExitState = "SC0005_USER_NF";

      return;
    }

    try
    {
      UpdateServiceProviderProfile();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0005_USER_NF";

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
        entities.ExistingServiceProviderProfile.EffectiveDate =
          db.GetDate(reader, 1);
        entities.ExistingServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingServiceProviderProfile.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ExistingServiceProviderProfile.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ExistingServiceProviderProfile.ProName =
          db.GetString(reader, 5);
        entities.ExistingServiceProviderProfile.SpdGenId =
          db.GetInt32(reader, 6);
        entities.ExistingServiceProviderProfile.Populated = true;
      });
  }

  private void UpdateServiceProviderProfile()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingServiceProviderProfile.Populated);

    var effectiveDate = import.ServiceProviderProfile.EffectiveDate;
    var discontinueDate = import.ServiceProviderProfile.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ExistingServiceProviderProfile.Populated = false;
    Update("UpdateServiceProviderProfile",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
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

    entities.ExistingServiceProviderProfile.EffectiveDate = effectiveDate;
    entities.ExistingServiceProviderProfile.DiscontinueDate = discontinueDate;
    entities.ExistingServiceProviderProfile.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingServiceProviderProfile.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingServiceProviderProfile.Populated = true;
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

    private ServiceProviderProfile serviceProviderProfile;
    private Profile profile;
    private ServiceProvider serviceProvider;
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
    /// A value of ExistingServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("existingServiceProviderProfile")]
    public ServiceProviderProfile ExistingServiceProviderProfile
    {
      get => existingServiceProviderProfile ??= new();
      set => existingServiceProviderProfile = value;
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

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    private ServiceProviderProfile existingServiceProviderProfile;
    private Profile existingProfile;
    private ServiceProvider existingServiceProvider;
  }
#endregion
}

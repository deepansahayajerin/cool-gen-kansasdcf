// Program: UPDATE_PROFILE, ID: 371454129, model: 746.
// Short name: SWE01498
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: UPDATE_PROFILE.
/// </summary>
[Serializable]
public partial class UpdateProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateProfile.
  /// </summary>
  public UpdateProfile(IContext context, Import import, Export export):
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

    try
    {
      UpdateProfile1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0015_PROFILE_NU";

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

  private bool ReadProfile()
  {
    entities.Profile.Populated = false;

    return Read("ReadProfile",
      (db, command) =>
      {
        db.SetString(command, "name", import.Profile.Name);
      },
      (db, reader) =>
      {
        entities.Profile.Name = db.GetString(reader, 0);
        entities.Profile.Desc = db.GetNullableString(reader, 1);
        entities.Profile.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Profile.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.Profile.RestrictionCode1 = db.GetNullableString(reader, 4);
        entities.Profile.RestrictionCode2 = db.GetNullableString(reader, 5);
        entities.Profile.RestrictionCode3 = db.GetNullableString(reader, 6);
        entities.Profile.Populated = true;
      });
  }

  private void UpdateProfile1()
  {
    var desc = import.Profile.Desc ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var restrictionCode1 = import.Profile.RestrictionCode1 ?? "";
    var restrictionCode2 = import.Profile.RestrictionCode2 ?? "";
    var restrictionCode3 = import.Profile.RestrictionCode3 ?? "";

    entities.Profile.Populated = false;
    Update("UpdateProfile",
      (db, command) =>
      {
        db.SetNullableString(command, "profileDesc", desc);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "restrictionCd1", restrictionCode1);
        db.SetNullableString(command, "restrictionCd2", restrictionCode2);
        db.SetNullableString(command, "restrictionCd3", restrictionCode3);
        db.SetString(command, "name", entities.Profile.Name);
      });

    entities.Profile.Desc = desc;
    entities.Profile.LastUpdatedBy = lastUpdatedBy;
    entities.Profile.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Profile.RestrictionCode1 = restrictionCode1;
    entities.Profile.RestrictionCode2 = restrictionCode2;
    entities.Profile.RestrictionCode3 = restrictionCode3;
    entities.Profile.Populated = true;
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

    private Profile profile;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    private Profile profile;
  }
#endregion
}

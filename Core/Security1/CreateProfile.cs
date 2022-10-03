// Program: CREATE_PROFILE, ID: 371454127, model: 746.
// Short name: SWE00155
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: CREATE_PROFILE.
/// </summary>
[Serializable]
public partial class CreateProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateProfile.
  /// </summary>
  public CreateProfile(IContext context, Import import, Export export):
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
      CreateProfile1();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SC0015_PROFILE_AE";

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

  private void CreateProfile1()
  {
    var name = import.Profile.Name;
    var desc = import.Profile.Desc ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var restrictionCode1 = import.Profile.RestrictionCode1 ?? "";
    var restrictionCode2 = import.Profile.RestrictionCode2 ?? "";
    var restrictionCode3 = import.Profile.RestrictionCode3 ?? "";

    entities.Profile.Populated = false;
    Update("CreateProfile",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetNullableString(command, "profileDesc", desc);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTimes", default(DateTime));
        db.SetNullableString(command, "restrictionCd1", restrictionCode1);
        db.SetNullableString(command, "restrictionCd2", restrictionCode2);
        db.SetNullableString(command, "restrictionCd3", restrictionCode3);
      });

    entities.Profile.Name = name;
    entities.Profile.Desc = desc;
    entities.Profile.CreatedBy = createdBy;
    entities.Profile.CreatedTimestamp = createdTimestamp;
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

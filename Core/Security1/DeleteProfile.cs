// Program: DELETE_PROFILE, ID: 371454128, model: 746.
// Short name: SWE00192
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: DELETE_PROFILE.
/// </summary>
[Serializable]
public partial class DeleteProfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DELETE_PROFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DeleteProfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DeleteProfile.
  /// </summary>
  public DeleteProfile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadProfile())
    {
      DeleteProfile1();
    }
    else
    {
      ExitState = "SC0015_PROFILE_NF";
    }
  }

  private void DeleteProfile1()
  {
    Update("DeleteProfile",
      (db, command) =>
      {
        db.SetString(command, "name", entities.Profile.Name);
      });
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
        entities.Profile.Populated = true;
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

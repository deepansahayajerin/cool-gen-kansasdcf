// Program: SC_CAB_VERIFY_ACTION_LVL_SECURTY, ID: 371453009, model: 746.
// Short name: SWE01084
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// <para>
/// A program: SC_CAB_VERIFY_ACTION_LVL_SECURTY.
/// </para>
/// <para>
/// RESP: SECUR
/// </para>
/// </summary>
[Serializable]
public partial class ScCabVerifyActionLvlSecurty: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_VERIFY_ACTION_LVL_SECURTY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabVerifyActionLvlSecurty(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabVerifyActionLvlSecurty.
  /// </summary>
  public ScCabVerifyActionLvlSecurty(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(global.Command, "PREV") || Equal(global.Command, "NEXT") || Equal
      (global.Command, "SIGNOFF") || Equal(global.Command, "RETURN") || Equal
      (global.Command, "LIST") || Equal(global.Command, "CLEAR") || Equal
      (global.Command, "INVALID") || IsEmpty(global.Command) || Equal
      (global.Command, "EXIT") || Equal(global.Command, "CHPV") || Equal
      (global.Command, "CHNX") || Equal(global.Command, "APNX") || Equal
      (global.Command, "APPV"))
    {
      return;
    }

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (Equal(import.Group.Item.Command.Value, global.Command))
      {
        if (AsChar(import.Group.Item.ProfileAuthorization.ActiveInd) == 'Y')
        {
          return;
        }
        else
        {
          ExitState = "SC0009_ACTN_CURRENTLY_NOT_AVAIL";

          return;
        }
      }
    }

    ExitState = "SC0039_SERV_PROV_NOT_AUTH_CMND";
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Command.
      /// </summary>
      [JsonPropertyName("command")]
      public Command Command
      {
        get => command ??= new();
        set => command = value;
      }

      /// <summary>
      /// A value of ProfileAuthorization.
      /// </summary>
      [JsonPropertyName("profileAuthorization")]
      public ProfileAuthorization ProfileAuthorization
      {
        get => profileAuthorization ??= new();
        set => profileAuthorization = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Command command;
      private ProfileAuthorization profileAuthorization;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    private Common auth;
  }
#endregion
}

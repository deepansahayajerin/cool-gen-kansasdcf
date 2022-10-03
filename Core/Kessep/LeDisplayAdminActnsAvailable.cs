// Program: LE_DISPLAY_ADMIN_ACTNS_AVAILABLE, ID: 372582125, model: 746.
// Short name: SWE00765
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DISPLAY_ADMIN_ACTNS_AVAILABLE.
/// </para>
/// <para>
/// RESP: LEGAL	
/// This action block lists the Administrative Actions Available for 
/// enforcement.
/// </para>
/// </summary>
[Serializable]
public partial class LeDisplayAdminActnsAvailable: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISPLAY_ADMIN_ACTNS_AVAILABLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDisplayAdminActnsAvailable(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDisplayAdminActnsAvailable.
  /// </summary>
  public LeDisplayAdminActnsAvailable(IContext context, Import import,
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
    // -------------------------------------------------------------------
    // Date  	  Developer      Request #  Description
    // 05/05/95  C.W.Hedenskog             Initial Code  
    // -------------------------------------------------------------------
    export.AdminAction.Index = 0;
    export.AdminAction.Clear();

    foreach(var item in ReadAdministrativeAction())
    {
      export.AdminAction.Update.AdministrativeAction.Assign(
        entities.AdministrativeAction);
      export.AdminAction.Next();
    }
  }

  private IEnumerable<bool> ReadAdministrativeAction()
  {
    return ReadEach("ReadAdministrativeAction",
      null,
      (db, reader) =>
      {
        if (export.AdminAction.IsFull)
        {
          return false;
        }

        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Indicator = db.GetString(reader, 2);
        entities.AdministrativeAction.Populated = true;

        return true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A AdminActionGroup group.</summary>
    [Serializable]
    public class AdminActionGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of AdministrativeAction.
      /// </summary>
      [JsonPropertyName("administrativeAction")]
      public AdministrativeAction AdministrativeAction
      {
        get => administrativeAction ??= new();
        set => administrativeAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 102;

      private Common common;
      private AdministrativeAction administrativeAction;
    }

    /// <summary>
    /// Gets a value of AdminAction.
    /// </summary>
    [JsonIgnore]
    public Array<AdminActionGroup> AdminAction => adminAction ??= new(
      AdminActionGroup.Capacity);

    /// <summary>
    /// Gets a value of AdminAction for json serialization.
    /// </summary>
    [JsonPropertyName("adminAction")]
    [Computed]
    public IList<AdminActionGroup> AdminAction_Json
    {
      get => adminAction;
      set => AdminAction.Assign(value);
    }

    private Array<AdminActionGroup> adminAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    private AdministrativeAction administrativeAction;
  }
#endregion
}

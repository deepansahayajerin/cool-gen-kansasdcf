// Program: CAB_READ_ADMIN_ACTION_CRITERIA, ID: 372615757, model: 746.
// Short name: SWE00072
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_READ_ADMIN_ACTION_CRITERIA.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This common action block reads each Administrative Action Criteria for a 
/// specified Administrative Action Type.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdminActionCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADMIN_ACTION_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdminActionCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdminActionCriteria.
  /// </summary>
  public CabReadAdminActionCriteria(IContext context, Import import,
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
    export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    if (ReadAdministrativeAction())
    {
      MoveAdministrativeAction(entities.AdministrativeAction,
        export.AdministrativeAction);

      export.Import1.Index = 0;
      export.Import1.Clear();

      foreach(var item in ReadAdministrativeActionCriteria())
      {
        export.Import1.Update.AdministrativeActionCriteria.Assign(
          entities.AdministrativeActionCriteria);
        local.DateWorkArea.Date =
          export.Import1.Item.AdministrativeActionCriteria.EndDate;
        UseCabSetMaximumDiscontinueDate();
        export.Import1.Update.AdministrativeActionCriteria.EndDate =
          local.DateWorkArea.Date;
        export.Import1.Update.Common.SelectChar = "";
        export.Import1.Next();
      }
    }
    else
    {
      ExitState = "ADMINISTRATIVE_ACTION_NF";
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.DateWorkArea.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadAdministrativeAction()
  {
    entities.AdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.AdministrativeAction.Description = db.GetString(reader, 1);
        entities.AdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadAdministrativeActionCriteria()
  {
    return ReadEach("ReadAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetString(command, "aatType", entities.AdministrativeAction.Type1);
        db.SetInt32(command, "adminActionId", import.Starting.Identifier);
      },
      (db, reader) =>
      {
        if (export.Import1.IsFull)
        {
          return false;
        }

        entities.AdministrativeActionCriteria.AatType = db.GetString(reader, 0);
        entities.AdministrativeActionCriteria.Identifier =
          db.GetInt32(reader, 1);
        entities.AdministrativeActionCriteria.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeActionCriteria.EndDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeActionCriteria.Description =
          db.GetString(reader, 4);
        entities.AdministrativeActionCriteria.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public AdministrativeActionCriteria Starting
    {
      get => starting ??= new();
      set => starting = value;
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

    private AdministrativeActionCriteria starting;
    private AdministrativeAction administrativeAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
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
      /// A value of AdministrativeActionCriteria.
      /// </summary>
      [JsonPropertyName("administrativeActionCriteria")]
      public AdministrativeActionCriteria AdministrativeActionCriteria
      {
        get => administrativeActionCriteria ??= new();
        set => administrativeActionCriteria = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private Common common;
      private AdministrativeActionCriteria administrativeActionCriteria;
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

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private AdministrativeAction administrativeAction;
    private Array<ImportGroup> import1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeActionCriteria.
    /// </summary>
    [JsonPropertyName("administrativeActionCriteria")]
    public AdministrativeActionCriteria AdministrativeActionCriteria
    {
      get => administrativeActionCriteria ??= new();
      set => administrativeActionCriteria = value;
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

    private AdministrativeActionCriteria administrativeActionCriteria;
    private AdministrativeAction administrativeAction;
  }
#endregion
}

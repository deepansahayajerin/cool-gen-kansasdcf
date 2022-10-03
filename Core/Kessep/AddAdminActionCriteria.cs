// Program: ADD_ADMIN_ACTION_CRITERIA, ID: 372615760, model: 746.
// Short name: SWE00004
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ADD_ADMIN_ACTION_CRITERIA.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates ADMINISTRATIVE ACTION CRITERIA.
/// </para>
/// </summary>
[Serializable]
public partial class AddAdminActionCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ADD_ADMIN_ACTION_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AddAdminActionCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AddAdminActionCriteria.
  /// </summary>
  public AddAdminActionCriteria(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************
    // 10/24/1998   P. Sharp    Removed duplicate use of max date action block.
    if (ReadAdministrativeAction())
    {
      MoveAdministrativeAction(entities.AdministrativeAction,
        export.AdministrativeAction);
      UseLeGenAdminActionCriteriaId();

      if (Equal(import.AdministrativeActionCriteria.EndDate, null))
      {
        local.DateWorkArea.Date = UseCabSetMaximumDiscontinueDate();
      }
      else
      {
        local.DateWorkArea.Date = import.AdministrativeActionCriteria.EndDate;
      }

      try
      {
        CreateAdministrativeActionCriteria();
        export.AdministrativeActionCriteria.Assign(
          entities.AdministrativeActionCriteria);

        if (Equal(export.AdministrativeActionCriteria.EndDate,
          local.MaxDate.Date))
        {
          export.AdministrativeActionCriteria.EndDate =
            local.InitialisedToZeros.Date;
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ADMIN_ACTION_CRITERIA_AE";

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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeGenAdminActionCriteriaId()
  {
    var useImport = new LeGenAdminActionCriteriaId.Import();
    var useExport = new LeGenAdminActionCriteriaId.Export();

    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;

    Call(LeGenAdminActionCriteriaId.Execute, useImport, useExport);

    local.AdministrativeActionCriteria.Identifier =
      useExport.AdministrativeActionCriteria.Identifier;
  }

  private void CreateAdministrativeActionCriteria()
  {
    var aatType = entities.AdministrativeAction.Type1;
    var identifier = local.AdministrativeActionCriteria.Identifier;
    var effectiveDate = import.AdministrativeActionCriteria.EffectiveDate;
    var endDate = local.DateWorkArea.Date;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var description = import.AdministrativeActionCriteria.Description;

    entities.AdministrativeActionCriteria.Populated = false;
    Update("CreateAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetString(command, "aatType", aatType);
        db.SetInt32(command, "adminActionId", identifier);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetString(command, "description", description);
      });

    entities.AdministrativeActionCriteria.AatType = aatType;
    entities.AdministrativeActionCriteria.Identifier = identifier;
    entities.AdministrativeActionCriteria.EffectiveDate = effectiveDate;
    entities.AdministrativeActionCriteria.EndDate = endDate;
    entities.AdministrativeActionCriteria.CreatedBy = createdBy;
    entities.AdministrativeActionCriteria.CreatedTstamp = createdTstamp;
    entities.AdministrativeActionCriteria.Description = description;
    entities.AdministrativeActionCriteria.Populated = true;
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

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    /// <summary>
    /// A value of AdministrativeActionCriteria.
    /// </summary>
    [JsonPropertyName("administrativeActionCriteria")]
    public AdministrativeActionCriteria AdministrativeActionCriteria
    {
      get => administrativeActionCriteria ??= new();
      set => administrativeActionCriteria = value;
    }

    private AdministrativeAction administrativeAction;
    private AdministrativeActionCriteria administrativeActionCriteria;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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

    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private DateWorkArea dateWorkArea;
    private AdministrativeActionCriteria administrativeActionCriteria;
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

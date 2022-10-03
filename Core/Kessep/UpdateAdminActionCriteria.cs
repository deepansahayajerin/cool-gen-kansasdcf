// Program: UPDATE_ADMIN_ACTION_CRITERIA, ID: 372615762, model: 746.
// Short name: SWE01462
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_ADMIN_ACTION_CRITERIA.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates ADMINISTRATIVE ACTION CRITERIA.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateAdminActionCriteria: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_ADMIN_ACTION_CRITERIA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateAdminActionCriteria(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateAdminActionCriteria.
  /// </summary>
  public UpdateAdminActionCriteria(IContext context, Import import,
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
    // ******************************************************
    // 10/24/1998   P. Sharp    Removed duplicate use of max date action block.
    if (ReadAdministrativeActionCriteria())
    {
      if (Equal(import.AdministrativeActionCriteria.EndDate,
        local.InitialisedToZeros.Date))
      {
        local.DateWorkArea.Date = UseCabSetMaximumDiscontinueDate();
      }
      else
      {
        local.DateWorkArea.Date = import.AdministrativeActionCriteria.EndDate;
      }

      try
      {
        UpdateAdministrativeActionCriteria();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "ADMIN_ACTION_CRITERIA_NU";

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
      ExitState = "ADMIN_ACTION_CRITERIA_NF";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadAdministrativeActionCriteria()
  {
    entities.AdministrativeActionCriteria.Populated = false;

    return Read("ReadAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminActionId",
          import.AdministrativeActionCriteria.Identifier);
        db.SetString(command, "aatType", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.AdministrativeActionCriteria.AatType = db.GetString(reader, 0);
        entities.AdministrativeActionCriteria.Identifier =
          db.GetInt32(reader, 1);
        entities.AdministrativeActionCriteria.EffectiveDate =
          db.GetDate(reader, 2);
        entities.AdministrativeActionCriteria.EndDate =
          db.GetNullableDate(reader, 3);
        entities.AdministrativeActionCriteria.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.AdministrativeActionCriteria.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.AdministrativeActionCriteria.Description =
          db.GetString(reader, 6);
        entities.AdministrativeActionCriteria.Populated = true;
      });
  }

  private void UpdateAdministrativeActionCriteria()
  {
    System.Diagnostics.Debug.Assert(
      entities.AdministrativeActionCriteria.Populated);

    var effectiveDate = import.AdministrativeActionCriteria.EffectiveDate;
    var endDate = local.DateWorkArea.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var description = import.AdministrativeActionCriteria.Description;

    entities.AdministrativeActionCriteria.Populated = false;
    Update("UpdateAdministrativeActionCriteria",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "description", description);
        db.SetString(
          command, "aatType", entities.AdministrativeActionCriteria.AatType);
        db.SetInt32(
          command, "adminActionId",
          entities.AdministrativeActionCriteria.Identifier);
      });

    entities.AdministrativeActionCriteria.EffectiveDate = effectiveDate;
    entities.AdministrativeActionCriteria.EndDate = endDate;
    entities.AdministrativeActionCriteria.LastUpdatedBy = lastUpdatedBy;
    entities.AdministrativeActionCriteria.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.AdministrativeActionCriteria.Description = description;
    entities.AdministrativeActionCriteria.Populated = true;
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

    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private DateWorkArea dateWorkArea;
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
#endregion
}

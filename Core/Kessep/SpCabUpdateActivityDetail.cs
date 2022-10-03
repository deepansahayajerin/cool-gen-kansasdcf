// Program: SP_CAB_UPDATE_ACTIVITY_DETAIL, ID: 371744530, model: 746.
// Short name: SWE01748
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_ACTIVITY_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabUpdateActivityDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ACTIVITY_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateActivityDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateActivityDetail.
  /// </summary>
  public SpCabUpdateActivityDetail(IContext context, Import import,
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
    if (ReadActivityDetail())
    {
      try
      {
        UpdateActivityDetail();
        export.ActivityDetail.Assign(entities.ActivityDetail);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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
      ExitState = "SP0000_ACTIVITY_DETAIL_NF";
    }
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.ActivityDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.ActivityDetail.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDetail.FedNonComplianceDays =
          db.GetNullableInt32(reader, 3);
        entities.ActivityDetail.FedNearNonComplDays =
          db.GetNullableInt32(reader, 4);
        entities.ActivityDetail.OtherNonComplianceDays =
          db.GetNullableInt32(reader, 5);
        entities.ActivityDetail.OtherNearNonComplDays =
          db.GetNullableInt32(reader, 6);
        entities.ActivityDetail.RegulationSourceId =
          db.GetNullableString(reader, 7);
        entities.ActivityDetail.RegulationSourceDescription =
          db.GetNullableString(reader, 8);
        entities.ActivityDetail.EffectiveDate = db.GetDate(reader, 9);
        entities.ActivityDetail.DiscontinueDate = db.GetDate(reader, 10);
        entities.ActivityDetail.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.ActivityDetail.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 13);
        entities.ActivityDetail.Populated = true;
      });
  }

  private void UpdateActivityDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);

    var businessObjectCode = import.ActivityDetail.BusinessObjectCode ?? "";
    var caseUnitFunction = import.ActivityDetail.CaseUnitFunction ?? "";
    var fedNonComplianceDays =
      import.ActivityDetail.FedNonComplianceDays.GetValueOrDefault();
    var fedNearNonComplDays =
      import.ActivityDetail.FedNearNonComplDays.GetValueOrDefault();
    var otherNonComplianceDays =
      import.ActivityDetail.OtherNonComplianceDays.GetValueOrDefault();
    var otherNearNonComplDays =
      import.ActivityDetail.OtherNearNonComplDays.GetValueOrDefault();
    var regulationSourceId = import.ActivityDetail.RegulationSourceId ?? "";
    var regulationSourceDescription =
      import.ActivityDetail.RegulationSourceDescription ?? "";
    var effectiveDate = import.ActivityDetail.EffectiveDate;
    var discontinueDate = import.ActivityDetail.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ActivityDetail.Populated = false;
    Update("UpdateActivityDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "businessObject", businessObjectCode);
        db.SetNullableString(command, "caseUnitFunction", caseUnitFunction);
        db.SetNullableInt32(command, "fedNoCompliDays", fedNonComplianceDays);
        db.SetNullableInt32(command, "fedNrNCmplDays", fedNearNonComplDays);
        db.SetNullableInt32(command, "othNoCompliDays", otherNonComplianceDays);
        db.SetNullableInt32(command, "othNrNComplDay", otherNearNonComplDays);
        db.SetNullableString(command, "regSourceId", regulationSourceId);
        db.
          SetNullableString(command, "regSrcDesc", regulationSourceDescription);
          
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", entities.ActivityDetail.ActNo);
      });

    entities.ActivityDetail.BusinessObjectCode = businessObjectCode;
    entities.ActivityDetail.CaseUnitFunction = caseUnitFunction;
    entities.ActivityDetail.FedNonComplianceDays = fedNonComplianceDays;
    entities.ActivityDetail.FedNearNonComplDays = fedNearNonComplDays;
    entities.ActivityDetail.OtherNonComplianceDays = otherNonComplianceDays;
    entities.ActivityDetail.OtherNearNonComplDays = otherNearNonComplDays;
    entities.ActivityDetail.RegulationSourceId = regulationSourceId;
    entities.ActivityDetail.RegulationSourceDescription =
      regulationSourceDescription;
    entities.ActivityDetail.EffectiveDate = effectiveDate;
    entities.ActivityDetail.DiscontinueDate = discontinueDate;
    entities.ActivityDetail.LastUpdatedBy = lastUpdatedBy;
    entities.ActivityDetail.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ActivityDetail.Populated = true;
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
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDetail activityDetail;
    private Activity activity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    private ActivityDetail activityDetail;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

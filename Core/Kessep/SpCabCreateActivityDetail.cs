// Program: SP_CAB_CREATE_ACTIVITY_DETAIL, ID: 371744531, model: 746.
// Short name: SWE01746
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ACTIVITY_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabCreateActivityDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ACTIVITY_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateActivityDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateActivityDetail.
  /// </summary>
  public SpCabCreateActivityDetail(IContext context, Import import,
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
    ReadActivityDetail();
    local.ActivityDetail.SystemGeneratedIdentifier =
      entities.ActivityDetail.SystemGeneratedIdentifier + 1;

    if (ReadActivity())
    {
      try
      {
        CreateActivityDetail();
        export.ActivityDetail.Assign(entities.New1);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_ACTIVITY_DETAIL_AE";

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
      ExitState = "SP0000_ACTIVITY_NF";
    }
  }

  private void CreateActivityDetail()
  {
    var systemGeneratedIdentifier =
      local.ActivityDetail.SystemGeneratedIdentifier;
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var actNo = entities.Activity.ControlNumber;

    entities.New1.Populated = false;
    Update("CreateActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "actNo", actNo);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.BusinessObjectCode = businessObjectCode;
    entities.New1.CaseUnitFunction = caseUnitFunction;
    entities.New1.FedNonComplianceDays = fedNonComplianceDays;
    entities.New1.FedNearNonComplDays = fedNearNonComplDays;
    entities.New1.OtherNonComplianceDays = otherNonComplianceDays;
    entities.New1.OtherNearNonComplDays = otherNearNonComplDays;
    entities.New1.RegulationSourceId = regulationSourceId;
    entities.New1.RegulationSourceDescription = regulationSourceDescription;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ActNo = actNo;
    entities.New1.Populated = true;
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Populated = true;
      });
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 1);
        entities.ActivityDetail.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ActivityDetail New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    private ActivityDetail new1;
    private Activity activity;
  }
#endregion
}

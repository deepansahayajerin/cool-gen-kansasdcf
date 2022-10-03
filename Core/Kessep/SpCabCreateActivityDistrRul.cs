// Program: SP_CAB_CREATE_ACTIVITY_DISTR_RUL, ID: 371748629, model: 746.
// Short name: SWE01730
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ACTIVITY_DISTR_RUL.
/// </summary>
[Serializable]
public partial class SpCabCreateActivityDistrRul: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ACTIVITY_DISTR_RUL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateActivityDistrRul(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateActivityDistrRul.
  /// </summary>
  public SpCabCreateActivityDistrRul(IContext context, Import import,
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
    if (!ReadActivityDetail())
    {
      ExitState = "SP0000_ACTIVITY_DETAIL_NF";

      return;
    }

    ReadActivityDistributionRule();
    local.ActivityDistributionRule.SystemGeneratedIdentifier =
      entities.ActivityDistributionRule.SystemGeneratedIdentifier + 1;

    try
    {
      CreateActivityDistributionRule();
      export.ActivityDistributionRule.Assign(entities.New1);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_ACTIVITY_DISTR_RULE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SP0000_ACTIVITY_DISTR_RULE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateActivityDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);

    var systemGeneratedIdentifier =
      local.ActivityDistributionRule.SystemGeneratedIdentifier;
    var businessObjectCode = import.ActivityDistributionRule.BusinessObjectCode;
    var caseUnitFunction = import.ActivityDistributionRule.CaseUnitFunction ?? ""
      ;
    var reasonCode = import.ActivityDistributionRule.ReasonCode;
    var responsibilityCode = import.ActivityDistributionRule.ResponsibilityCode;
    var caseRoleCode = import.ActivityDistributionRule.CaseRoleCode ?? "";
    var csePersonAcctCode =
      import.ActivityDistributionRule.CsePersonAcctCode ?? "";
    var csenetRoleCode = import.ActivityDistributionRule.CsenetRoleCode ?? "";
    var laCaseRoleCode = import.ActivityDistributionRule.LaCaseRoleCode ?? "";
    var laPersonCode = import.ActivityDistributionRule.LaPersonCode ?? "";
    var effectiveDate = import.ActivityDistributionRule.EffectiveDate;
    var discontinueDate = import.ActivityDistributionRule.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var actControlNo = entities.ActivityDetail.ActNo;
    var acdId = entities.ActivityDetail.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "businessObject", businessObjectCode);
        db.SetNullableString(command, "caseUnitFunction", caseUnitFunction);
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "respCode", responsibilityCode);
        db.SetNullableString(command, "caseRoleCode", caseRoleCode);
        db.SetNullableString(command, "csePersonAcctCd", csePersonAcctCode);
        db.SetNullableString(command, "csenetRoleCode", csenetRoleCode);
        db.SetNullableString(command, "laCaseRoleCode", laCaseRoleCode);
        db.SetNullableString(command, "laPersonCode", laPersonCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "actControlNo", actControlNo);
        db.SetInt32(command, "acdId", acdId);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.BusinessObjectCode = businessObjectCode;
    entities.New1.CaseUnitFunction = caseUnitFunction;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.ResponsibilityCode = responsibilityCode;
    entities.New1.CaseRoleCode = caseRoleCode;
    entities.New1.CsePersonAcctCode = csePersonAcctCode;
    entities.New1.CsenetRoleCode = csenetRoleCode;
    entities.New1.LaCaseRoleCode = laCaseRoleCode;
    entities.New1.LaPersonCode = laPersonCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ActControlNo = actControlNo;
    entities.New1.AcdId = acdId;
    entities.New1.Populated = true;
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.ActivityDetail.SystemGeneratedIdentifier);
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

  private bool ReadActivityDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);
    entities.ActivityDistributionRule.Populated = false;

    return Read("ReadActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "actControlNo", entities.ActivityDetail.ActNo);
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ActivityDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDistributionRule.ActControlNo = db.GetInt32(reader, 1);
        entities.ActivityDistributionRule.AcdId = db.GetInt32(reader, 2);
        entities.ActivityDistributionRule.Populated = true;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    /// <summary>
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    /// <summary>
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    private Activity activity;
    private ActivityDistributionRule activityDistributionRule;
    private ActivityDetail activityDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    private ActivityDistributionRule activityDistributionRule;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    private ActivityDistributionRule activityDistributionRule;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

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
    /// A value of ActivityDistributionRule.
    /// </summary>
    [JsonPropertyName("activityDistributionRule")]
    public ActivityDistributionRule ActivityDistributionRule
    {
      get => activityDistributionRule ??= new();
      set => activityDistributionRule = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ActivityDistributionRule New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Activity activity;
    private ActivityDetail activityDetail;
    private ActivityDistributionRule activityDistributionRule;
    private ActivityDistributionRule new1;
  }
#endregion
}

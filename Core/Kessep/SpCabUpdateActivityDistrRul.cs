// Program: SP_CAB_UPDATE_ACTIVITY_DISTR_RUL, ID: 371748628, model: 746.
// Short name: SWE01732
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_ACTIVITY_DISTR_RUL.
/// </summary>
[Serializable]
public partial class SpCabUpdateActivityDistrRul: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ACTIVITY_DISTR_RUL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateActivityDistrRul(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateActivityDistrRul.
  /// </summary>
  public SpCabUpdateActivityDistrRul(IContext context, Import import,
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
    if (ReadActivityDistributionRule())
    {
      try
      {
        UpdateActivityDistributionRule();
        export.ActivityDistributionRule.
          Assign(entities.ActivityDistributionRule);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_ACTIVITY_DISTR_RULE_AE";

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
      ExitState = "SP0000_ACTIVITY_DISTR_RULE_NF";
    }
  }

  private bool ReadActivityDistributionRule()
  {
    entities.ActivityDistributionRule.Populated = false;

    return Read("ReadActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.ActivityDistributionRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "acdId", import.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actControlNo", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDistributionRule.BusinessObjectCode =
          db.GetString(reader, 1);
        entities.ActivityDistributionRule.CaseUnitFunction =
          db.GetNullableString(reader, 2);
        entities.ActivityDistributionRule.ReasonCode = db.GetString(reader, 3);
        entities.ActivityDistributionRule.ResponsibilityCode =
          db.GetString(reader, 4);
        entities.ActivityDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 5);
        entities.ActivityDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 6);
        entities.ActivityDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 7);
        entities.ActivityDistributionRule.LaCaseRoleCode =
          db.GetNullableString(reader, 8);
        entities.ActivityDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 9);
        entities.ActivityDistributionRule.EffectiveDate =
          db.GetDate(reader, 10);
        entities.ActivityDistributionRule.DiscontinueDate =
          db.GetDate(reader, 11);
        entities.ActivityDistributionRule.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.ActivityDistributionRule.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.ActivityDistributionRule.ActControlNo =
          db.GetInt32(reader, 14);
        entities.ActivityDistributionRule.AcdId = db.GetInt32(reader, 15);
        entities.ActivityDistributionRule.Populated = true;
      });
  }

  private void UpdateActivityDistributionRule()
  {
    System.Diagnostics.Debug.
      Assert(entities.ActivityDistributionRule.Populated);

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
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ActivityDistributionRule.Populated = false;
    Update("UpdateActivityDistributionRule",
      (db, command) =>
      {
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
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ActivityDistributionRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "actControlNo",
          entities.ActivityDistributionRule.ActControlNo);
        db.SetInt32(command, "acdId", entities.ActivityDistributionRule.AcdId);
      });

    entities.ActivityDistributionRule.CaseUnitFunction = caseUnitFunction;
    entities.ActivityDistributionRule.ReasonCode = reasonCode;
    entities.ActivityDistributionRule.ResponsibilityCode = responsibilityCode;
    entities.ActivityDistributionRule.CaseRoleCode = caseRoleCode;
    entities.ActivityDistributionRule.CsePersonAcctCode = csePersonAcctCode;
    entities.ActivityDistributionRule.CsenetRoleCode = csenetRoleCode;
    entities.ActivityDistributionRule.LaCaseRoleCode = laCaseRoleCode;
    entities.ActivityDistributionRule.LaPersonCode = laPersonCode;
    entities.ActivityDistributionRule.EffectiveDate = effectiveDate;
    entities.ActivityDistributionRule.DiscontinueDate = discontinueDate;
    entities.ActivityDistributionRule.LastUpdatedBy = lastUpdatedBy;
    entities.ActivityDistributionRule.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ActivityDistributionRule.Populated = true;
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

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDistributionRule activityDistributionRule;
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

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDistributionRule activityDistributionRule;
    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

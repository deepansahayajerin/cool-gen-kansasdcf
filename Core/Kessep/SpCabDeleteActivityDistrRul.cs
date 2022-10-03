// Program: SP_CAB_DELETE_ACTIVITY_DISTR_RUL, ID: 371748627, model: 746.
// Short name: SWE01731
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_ACTIVITY_DISTR_RUL.
/// </summary>
[Serializable]
public partial class SpCabDeleteActivityDistrRul: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_ACTIVITY_DISTR_RUL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteActivityDistrRul(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteActivityDistrRul.
  /// </summary>
  public SpCabDeleteActivityDistrRul(IContext context, Import import,
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
      DeleteActivityDistributionRule();
    }
    else
    {
      ExitState = "SP0000_ACTIVITY_DISTR_RULE_NF";
    }
  }

  private void DeleteActivityDistributionRule()
  {
    Update("DeleteActivityDistributionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.ActivityDistributionRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "actControlNo",
          entities.ActivityDistributionRule.ActControlNo);
        db.SetInt32(command, "acdId", entities.ActivityDistributionRule.AcdId);
      });
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
        entities.ActivityDistributionRule.ActControlNo = db.GetInt32(reader, 1);
        entities.ActivityDistributionRule.AcdId = db.GetInt32(reader, 2);
        entities.ActivityDistributionRule.Populated = true;
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

    private Activity activity;
    private ActivityDetail activityDetail;
    private ActivityDistributionRule activityDistributionRule;
  }
#endregion
}

// Program: SP_CAB_UPDATE_ALERT_DISTR_RULES, ID: 371747454, model: 746.
// Short name: SWE01723
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_ALERT_DISTR_RULES.
/// </summary>
[Serializable]
public partial class SpCabUpdateAlertDistrRules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ALERT_DISTR_RULES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateAlertDistrRules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateAlertDistrRules.
  /// </summary>
  public SpCabUpdateAlertDistrRules(IContext context, Import import,
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
    if (!ReadAlertDistributionRule())
    {
      ExitState = "SP0000_ALERT_DISTR_RULE_NF";

      return;
    }

    try
    {
      UpdateAlertDistributionRule();
      export.AlertDistributionRule.Assign(entities.AlertDistributionRule);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_ALERT_DISTR_RULE_AE";

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

  private bool ReadAlertDistributionRule()
  {
    entities.AlertDistributionRule.Populated = false;

    return Read("ReadAlertDistributionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.AlertDistributionRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "evdId", import.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.BusinessObjectCode =
          db.GetNullableString(reader, 1);
        entities.AlertDistributionRule.CaseUnitFunction =
          db.GetString(reader, 2);
        entities.AlertDistributionRule.PrioritizationCode =
          db.GetInt32(reader, 3);
        entities.AlertDistributionRule.OptimizationInd =
          db.GetString(reader, 4);
        entities.AlertDistributionRule.ReasonCode =
          db.GetNullableString(reader, 5);
        entities.AlertDistributionRule.CaseRoleCode =
          db.GetNullableString(reader, 6);
        entities.AlertDistributionRule.CsePersonAcctCode =
          db.GetNullableString(reader, 7);
        entities.AlertDistributionRule.CsenetRoleCode =
          db.GetNullableString(reader, 8);
        entities.AlertDistributionRule.LaCaseRoleCode =
          db.GetNullableString(reader, 9);
        entities.AlertDistributionRule.LaPersonCode =
          db.GetNullableString(reader, 10);
        entities.AlertDistributionRule.EffectiveDate = db.GetDate(reader, 11);
        entities.AlertDistributionRule.DiscontinueDate = db.GetDate(reader, 12);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 13);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 14);
        entities.AlertDistributionRule.Populated = true;
      });
  }

  private void UpdateAlertDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.AlertDistributionRule.Populated);

    var prioritizationCode = import.AlertDistributionRule.PrioritizationCode;
    var optimizationInd = import.AlertDistributionRule.OptimizationInd;
    var reasonCode = import.AlertDistributionRule.ReasonCode ?? "";
    var effectiveDate = import.AlertDistributionRule.EffectiveDate;
    var discontinueDate = import.AlertDistributionRule.DiscontinueDate;

    entities.AlertDistributionRule.Populated = false;
    Update("UpdateAlertDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "prioritizationCd", prioritizationCode);
        db.SetString(command, "optimizationInd", optimizationInd);
        db.SetNullableString(command, "reasonCode", reasonCode);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.AlertDistributionRule.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.AlertDistributionRule.EveNo);
        db.SetInt32(command, "evdId", entities.AlertDistributionRule.EvdId);
      });

    entities.AlertDistributionRule.PrioritizationCode = prioritizationCode;
    entities.AlertDistributionRule.OptimizationInd = optimizationInd;
    entities.AlertDistributionRule.ReasonCode = reasonCode;
    entities.AlertDistributionRule.EffectiveDate = effectiveDate;
    entities.AlertDistributionRule.DiscontinueDate = discontinueDate;
    entities.AlertDistributionRule.Populated = true;
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
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    private AlertDistributionRule alertDistributionRule;
    private Event1 event1;
    private EventDetail eventDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    private AlertDistributionRule alertDistributionRule;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private AlertDistributionRule alertDistributionRule;
    private Event1 event1;
    private EventDetail eventDetail;
    private Alert alert;
  }
#endregion
}

// Program: SP_CAB_CREATE_ALERT_DISTR_RULE, ID: 371747455, model: 746.
// Short name: SWE01721
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_ALERT_DISTR_RULE.
/// </summary>
[Serializable]
public partial class SpCabCreateAlertDistrRule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ALERT_DISTR_RULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateAlertDistrRule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateAlertDistrRule.
  /// </summary>
  public SpCabCreateAlertDistrRule(IContext context, Import import,
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
    if (!ReadEventEventDetail())
    {
      ExitState = "ZD_SP0000_EVENT_DETAIL_NF1";

      return;
    }

    if (!ReadAlert())
    {
      ExitState = "ZD_SP0000_ALERT_NF_2";

      return;
    }

    // -- 01/17/13  GVandy CQ33617 Implement alert distribution to a single 
    // office
    // -- service provider entered on DRLM.
    if (import.ServiceProvider.SystemGeneratedId > 0)
    {
      if (!ReadOfficeServiceProvider())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }
    }

    ReadAlertDistributionRule();
    local.AlertDistributionRule.SystemGeneratedIdentifier =
      entities.AlertDistributionRule.SystemGeneratedIdentifier + 1;

    try
    {
      CreateAlertDistributionRule();

      // -- 01/17/13  GVandy CQ33617 Implement alert distribution to a single 
      // office
      // -- service provider entered on DRLM.
      if (entities.OfficeServiceProvider.Populated)
      {
        AssociateAlertDistributionRule();
      }

      MoveAlertDistributionRule(entities.New1, export.AlertDistributionRule);
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

  private static void MoveAlertDistributionRule(AlertDistributionRule source,
    AlertDistributionRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.BusinessObjectCode = source.BusinessObjectCode;
    target.CaseUnitFunction = source.CaseUnitFunction;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.ReasonCode = source.ReasonCode;
    target.CaseRoleCode = source.CaseRoleCode;
    target.CsePersonAcctCode = source.CsePersonAcctCode;
    target.CsenetRoleCode = source.CsenetRoleCode;
    target.LaCaseRoleCode = source.LaCaseRoleCode;
    target.LaPersonCode = source.LaPersonCode;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void AssociateAlertDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var ospGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var ospEffectiveDt = entities.OfficeServiceProvider.EffectiveDate;

    entities.New1.Populated = false;
    Update("AssociateAlertDistributionRule",
      (db, command) =>
      {
        db.SetNullableInt32(command, "ospGeneratedId", ospGeneratedId);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospEffectiveDt", ospEffectiveDt);
        db.SetInt32(
          command, "systemGeneratedI", entities.New1.SystemGeneratedIdentifier);
          
        db.SetInt32(command, "eveNo", entities.New1.EveNo);
        db.SetInt32(command, "evdId", entities.New1.EvdId);
      });

    entities.New1.OspGeneratedId = ospGeneratedId;
    entities.New1.OffGeneratedId = offGeneratedId;
    entities.New1.OspRoleCode = ospRoleCode;
    entities.New1.OspEffectiveDt = ospEffectiveDt;
    entities.New1.Populated = true;
  }

  private void CreateAlertDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);

    var systemGeneratedIdentifier =
      local.AlertDistributionRule.SystemGeneratedIdentifier;
    var businessObjectCode =
      import.AlertDistributionRule.BusinessObjectCode ?? "";
    var caseUnitFunction = import.AlertDistributionRule.CaseUnitFunction;
    var prioritizationCode = import.AlertDistributionRule.PrioritizationCode;
    var optimizationInd = import.AlertDistributionRule.OptimizationInd;
    var reasonCode = import.AlertDistributionRule.ReasonCode ?? "";
    var caseRoleCode = import.AlertDistributionRule.CaseRoleCode ?? "";
    var csePersonAcctCode = import.AlertDistributionRule.CsePersonAcctCode ?? ""
      ;
    var csenetRoleCode = import.AlertDistributionRule.CsenetRoleCode ?? "";
    var laCaseRoleCode = import.AlertDistributionRule.LaCaseRoleCode ?? "";
    var laPersonCode = import.AlertDistributionRule.LaPersonCode ?? "";
    var effectiveDate = import.AlertDistributionRule.EffectiveDate;
    var discontinueDate = import.AlertDistributionRule.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var eveNo = entities.EventDetail.EveNo;
    var evdId = entities.EventDetail.SystemGeneratedIdentifier;
    var aleNo = entities.Alert.ControlNumber;

    entities.New1.Populated = false;
    Update("CreateAlertDistributionRule",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetNullableString(command, "businessObjectCd", businessObjectCode);
        db.SetString(command, "caseUnitFunction", caseUnitFunction);
        db.SetInt32(command, "prioritizationCd", prioritizationCode);
        db.SetString(command, "optimizationInd", optimizationInd);
        db.SetNullableString(command, "reasonCode", reasonCode);
        db.SetString(command, "responsibilityCod", "");
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
        db.SetInt32(command, "eveNo", eveNo);
        db.SetInt32(command, "evdId", evdId);
        db.SetNullableInt32(command, "aleNo", aleNo);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.BusinessObjectCode = businessObjectCode;
    entities.New1.CaseUnitFunction = caseUnitFunction;
    entities.New1.PrioritizationCode = prioritizationCode;
    entities.New1.OptimizationInd = optimizationInd;
    entities.New1.ReasonCode = reasonCode;
    entities.New1.CaseRoleCode = caseRoleCode;
    entities.New1.CsePersonAcctCode = csePersonAcctCode;
    entities.New1.CsenetRoleCode = csenetRoleCode;
    entities.New1.LaCaseRoleCode = laCaseRoleCode;
    entities.New1.LaPersonCode = laPersonCode;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.EveNo = eveNo;
    entities.New1.EvdId = evdId;
    entities.New1.AleNo = aleNo;
    entities.New1.OspGeneratedId = null;
    entities.New1.OffGeneratedId = null;
    entities.New1.OspRoleCode = null;
    entities.New1.OspEffectiveDt = null;
    entities.New1.Populated = true;
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Alert.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Populated = true;
      });
  }

  private bool ReadAlertDistributionRule()
  {
    System.Diagnostics.Debug.Assert(entities.EventDetail.Populated);
    entities.AlertDistributionRule.Populated = false;

    return Read("ReadAlertDistributionRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdId", entities.EventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", entities.EventDetail.EveNo);
      },
      (db, reader) =>
      {
        entities.AlertDistributionRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AlertDistributionRule.EveNo = db.GetInt32(reader, 1);
        entities.AlertDistributionRule.EvdId = db.GetInt32(reader, 2);
        entities.AlertDistributionRule.Populated = true;
      });
  }

  private bool ReadEventEventDetail()
  {
    entities.EventDetail.Populated = false;
    entities.Event1.Populated = false;

    return Read("ReadEventEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", import.Event1.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.EventDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Event1.ControlNumber = db.GetInt32(reader, 0);
        entities.EventDetail.EveNo = db.GetInt32(reader, 0);
        entities.EventDetail.SystemGeneratedIdentifier = db.GetInt32(reader, 1);
        entities.EventDetail.Populated = true;
        entities.Event1.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.Populated = true;
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
    /// A value of AlertDistributionRule.
    /// </summary>
    [JsonPropertyName("alertDistributionRule")]
    public AlertDistributionRule AlertDistributionRule
    {
      get => alertDistributionRule ??= new();
      set => alertDistributionRule = value;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private AlertDistributionRule alertDistributionRule;
    private Alert alert;
    private EventDetail eventDetail;
    private Event1 event1;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public AlertDistributionRule New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
    private AlertDistributionRule alertDistributionRule;
    private AlertDistributionRule new1;
    private EventDetail eventDetail;
    private Event1 event1;
    private Alert alert;
  }
#endregion
}

// Program: CREATE_MONITORED_ACTIVITY, ID: 371785882, model: 746.
// Short name: SWE01859
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_MONITORED_ACTIVITY.
/// </para>
/// <para>
/// Elementary process.
/// </para>
/// </summary>
[Serializable]
public partial class CreateMonitoredActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_MONITORED_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateMonitoredActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateMonitoredActivity.
  /// </summary>
  public CreateMonitoredActivity(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // CHANGE LOG:
    // ----------------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------  ----------	-------------	
    // ------------------------------------------------------------------------------
    // ??/??/??  ??????	????????	Initial Development
    // 06/23/99 M.Lachowicz			Change property of READs to generate SELECT ONLY.
    // 02/05/09  GVandy	CQ#8964		Modified to use 9 digit random number cab 
    // instead of setting system
    // 					generated identifier using a control table value.
    // ----------------------------------------------------------------------------------------------------------------------
    if (!ReadInfrastructure())
    {
      ExitState = "SP0000_HISTORY_INFRASTRUCTURE_NF";

      return;
    }

    do
    {
      try
      {
        CreateMonitoredActivity1();
        export.MonitoredActivity.Assign(entities.MonitoredActivity);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            // -- Repeat until successful.
            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "MONITORED_ACTIVITY_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    while(export.MonitoredActivity.SystemGeneratedIdentifier == 0);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateMonitoredActivity1()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var name = import.MonitoredActivity.Name;
    var activityControlNumber = import.MonitoredActivity.ActivityControlNumber;
    var typeCode = import.MonitoredActivity.TypeCode ?? "";
    var fedNonComplianceDate = import.MonitoredActivity.FedNonComplianceDate;
    var fedNearNonComplDate = import.MonitoredActivity.FedNearNonComplDate;
    var otherNonComplianceDate =
      import.MonitoredActivity.OtherNonComplianceDate;
    var otherNearNonComplDate = import.MonitoredActivity.OtherNearNonComplDate;
    var startDate = import.MonitoredActivity.StartDate;
    var closureDate = import.MonitoredActivity.ClosureDate;
    var closureReasonCode = import.MonitoredActivity.ClosureReasonCode ?? "";
    var caseUnitClosedInd = import.MonitoredActivity.CaseUnitClosedInd;
    var createdBy = import.MonitoredActivity.CreatedBy;
    var createdTimestamp = import.MonitoredActivity.CreatedTimestamp;
    var lastUpdatedBy = import.MonitoredActivity.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = import.MonitoredActivity.LastUpdatedTimestamp;
    var infSysGenId = entities.Infrastructure.SystemGeneratedIdentifier;

    entities.MonitoredActivity.Populated = false;
    Update("CreateMonitoredActivity",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratedI", systemGeneratedIdentifier);
        db.SetString(command, "name", name);
        db.SetInt32(command, "activityCtrlNum", activityControlNumber);
        db.SetNullableString(command, "typeCode", typeCode);
        db.SetNullableDate(command, "fedNcompDte", fedNonComplianceDate);
        db.SetNullableDate(command, "fedNearNcompDte", fedNearNonComplDate);
        db.SetNullableDate(command, "otherNcompDte", otherNonComplianceDate);
        db.SetNullableDate(command, "othrNearNcomDte", otherNearNonComplDate);
        db.SetDate(command, "startDate", startDate);
        db.SetNullableDate(command, "closureDate", closureDate);
        db.SetNullableString(command, "closureReasonCod", closureReasonCode);
        db.SetString(command, "caseUnitClosedI", caseUnitClosedInd);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "infSysGenId", infSysGenId);
      });

    entities.MonitoredActivity.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.MonitoredActivity.Name = name;
    entities.MonitoredActivity.ActivityControlNumber = activityControlNumber;
    entities.MonitoredActivity.TypeCode = typeCode;
    entities.MonitoredActivity.FedNonComplianceDate = fedNonComplianceDate;
    entities.MonitoredActivity.FedNearNonComplDate = fedNearNonComplDate;
    entities.MonitoredActivity.OtherNonComplianceDate = otherNonComplianceDate;
    entities.MonitoredActivity.OtherNearNonComplDate = otherNearNonComplDate;
    entities.MonitoredActivity.StartDate = startDate;
    entities.MonitoredActivity.ClosureDate = closureDate;
    entities.MonitoredActivity.ClosureReasonCode = closureReasonCode;
    entities.MonitoredActivity.CaseUnitClosedInd = caseUnitClosedInd;
    entities.MonitoredActivity.CreatedBy = createdBy;
    entities.MonitoredActivity.CreatedTimestamp = createdTimestamp;
    entities.MonitoredActivity.LastUpdatedBy = lastUpdatedBy;
    entities.MonitoredActivity.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.MonitoredActivity.InfSysGenId = infSysGenId;
    entities.MonitoredActivity.Populated = true;
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private MonitoredActivity monitoredActivity;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    private Infrastructure infrastructure;
    private MonitoredActivity monitoredActivity;
  }
#endregion
}

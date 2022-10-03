// Program: SP_EVENT_RELATED_UPDATES, ID: 374589424, model: 746.
// Short name: SWE02139
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_EVENT_RELATED_UPDATES.
/// </summary>
[Serializable]
public partial class SpEventRelatedUpdates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVENT_RELATED_UPDATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEventRelatedUpdates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEventRelatedUpdates.
  /// </summary>
  public SpEventRelatedUpdates(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER   REQUEST#	DESCRIPTION
    // 04/15/10  GVandy	CQ 966	Initial Development of this cab.  Logic was 
    // broken out from
    // 				sp_process_infrastructure cab.
    // ------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------
    // -- Process all alerts, lifecycle transformations, new events, starting
    // -- monitored activities, and ending monitored activities.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          // -------------------------------------------------------------------------------------
          // -- Process alerts which are created because of this event.
          // -------------------------------------------------------------------------------------
          for(import.Alerts.Index = 0; import.Alerts.Index < import
            .Alerts.Count; ++import.Alerts.Index)
          {
            if (!import.Alerts.CheckSize())
            {
              break;
            }

            UseSpCabCreateOfcSrvPrvdAlert();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          import.Alerts.CheckIndex();

          break;
        case 2:
          // -------------------------------------------------------------------------------------
          // -- Process case unit lifecycle transformations which occur because 
          // of this event.
          // -------------------------------------------------------------------------------------
          for(import.CaseUnits.Index = 0; import.CaseUnits.Index < import
            .CaseUnits.Count; ++import.CaseUnits.Index)
          {
            if (!import.CaseUnits.CheckSize())
            {
              break;
            }

            UseUpdateCaseUnit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          import.CaseUnits.CheckIndex();

          break;
        case 3:
          // -------------------------------------------------------------------------------------
          // -- Process new events which are raised because of this event.
          // -------------------------------------------------------------------------------------
          for(import.Events.Index = 0; import.Events.Index < import
            .Events.Count; ++import.Events.Index)
          {
            if (!import.Events.CheckSize())
            {
              break;
            }

            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          import.Events.CheckIndex();

          break;
        case 4:
          // -------------------------------------------------------------------------------------
          // -- Process monitored activities which are started because of this 
          // event.
          // -------------------------------------------------------------------------------------
          import.StartMona.Index = 0;

          for(var limit = import.StartMona.Count; import.StartMona.Index < limit
            ; ++import.StartMona.Index)
          {
            if (!import.StartMona.CheckSize())
            {
              break;
            }

            UseCreateMonitoredActivity();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            for(import.StartMona.Item.StartMonaAssgnmnt.Index = 0; import
              .StartMona.Item.StartMonaAssgnmnt.Index < import
              .StartMona.Item.StartMonaAssgnmnt.Count; ++
              import.StartMona.Item.StartMonaAssgnmnt.Index)
            {
              if (!import.StartMona.Item.StartMonaAssgnmnt.CheckSize())
              {
                break;
              }

              UseSpCabCreateMonActAssignment();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            import.StartMona.Item.StartMonaAssgnmnt.CheckIndex();
          }

          import.StartMona.CheckIndex();

          break;
        case 5:
          // -------------------------------------------------------------------------------------
          // -- Process monitored activities which are ended because of this 
          // event.
          // -------------------------------------------------------------------------------------
          for(import.EndMona.Index = 0; import.EndMona.Index < import
            .EndMona.Count; ++import.EndMona.Index)
          {
            if (!import.EndMona.CheckSize())
            {
              break;
            }

            UseSpCabUpdateMonitoredActivity();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }

          import.EndMona.CheckIndex();

          break;
        default:
          break;
      }
    }
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveMonitoredActivity1(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
    target.TypeCode = source.TypeCode;
  }

  private static void MoveMonitoredActivity2(MonitoredActivity source,
    MonitoredActivity target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ClosureDate = source.ClosureDate;
    target.ClosureReasonCode = source.ClosureReasonCode;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
    target.RecipientUserId = source.RecipientUserId;
  }

  private void UseCreateMonitoredActivity()
  {
    var useImport = new CreateMonitoredActivity.Import();
    var useExport = new CreateMonitoredActivity.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.MonitoredActivity.Assign(import.StartMona.Item.StartMona1);

    Call(CreateMonitoredActivity.Execute, useImport, useExport);

    MoveMonitoredActivity1(useExport.MonitoredActivity, local.MonitoredActivity);
      
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(import.Events.Item.G, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private void UseSpCabCreateMonActAssignment()
  {
    var useImport = new SpCabCreateMonActAssignment.Import();
    var useExport = new SpCabCreateMonActAssignment.Export();

    useImport.MonitoredActivity.Assign(local.MonitoredActivity);
    useImport.MonitoredActivityAssignment.Assign(
      import.StartMona.Item.StartMonaAssgnmnt.Item.
        StartMonaMonitoredActivityAssignment);
    MoveOfficeServiceProvider(import.StartMona.Item.StartMonaAssgnmnt.Item.
      StartMonaOfficeServiceProvider, useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      import.StartMona.Item.StartMonaAssgnmnt.Item.StartMonaServiceProvider.
        SystemGeneratedId;
    useImport.Office.SystemGeneratedId =
      import.StartMona.Item.StartMonaAssgnmnt.Item.StartMonaOffice.
        SystemGeneratedId;

    Call(SpCabCreateMonActAssignment.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    useImport.Alerts.SystemGeneratedIdentifier =
      import.Infrastructure.SystemGeneratedIdentifier;
    useImport.Office.SystemGeneratedId =
      import.Alerts.Item.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.Alerts.Item.GofficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.ServiceProvider.SystemGeneratedId =
      import.Alerts.Item.GserviceProvider.SystemGeneratedId;
    MoveOfficeServiceProviderAlert(import.Alerts.Item.
      GofficeServiceProviderAlert, useImport.OfficeServiceProviderAlert);

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateMonitoredActivity()
  {
    var useImport = new SpCabUpdateMonitoredActivity.Import();
    var useExport = new SpCabUpdateMonitoredActivity.Export();

    MoveMonitoredActivity2(import.EndMona.Item.End, useImport.MonitoredActivity);
      

    Call(SpCabUpdateMonitoredActivity.Execute, useImport, useExport);
  }

  private void UseUpdateCaseUnit()
  {
    var useImport = new UpdateCaseUnit.Import();
    var useExport = new UpdateCaseUnit.Export();

    useImport.Case1.Number = import.CaseUnits.Item.Case1.Number;
    MoveCaseUnit(import.CaseUnits.Item.CaseUnit, useImport.CaseUnit);

    Call(UpdateCaseUnit.Execute, useImport, useExport);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A EndMonaGroup group.</summary>
    [Serializable]
    public class EndMonaGroup
    {
      /// <summary>
      /// A value of End.
      /// </summary>
      [JsonPropertyName("end")]
      public MonitoredActivity End
      {
        get => end ??= new();
        set => end = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 750;

      private MonitoredActivity end;
    }

    /// <summary>A StartMonaGroup group.</summary>
    [Serializable]
    public class StartMonaGroup
    {
      /// <summary>
      /// A value of StartMona1.
      /// </summary>
      [JsonPropertyName("startMona1")]
      public MonitoredActivity StartMona1
      {
        get => startMona1 ??= new();
        set => startMona1 = value;
      }

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt.
      /// </summary>
      [JsonIgnore]
      public Array<StartMonaAssgnmntGroup> StartMonaAssgnmnt =>
        startMonaAssgnmnt ??= new(StartMonaAssgnmntGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of StartMonaAssgnmnt for json serialization.
      /// </summary>
      [JsonPropertyName("startMonaAssgnmnt")]
      [Computed]
      public IList<StartMonaAssgnmntGroup> StartMonaAssgnmnt_Json
      {
        get => startMonaAssgnmnt;
        set => StartMonaAssgnmnt.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private MonitoredActivity startMona1;
      private Array<StartMonaAssgnmntGroup> startMonaAssgnmnt;
    }

    /// <summary>A StartMonaAssgnmntGroup group.</summary>
    [Serializable]
    public class StartMonaAssgnmntGroup
    {
      /// <summary>
      /// A value of StartMonaMonitoredActivityAssignment.
      /// </summary>
      [JsonPropertyName("startMonaMonitoredActivityAssignment")]
      public MonitoredActivityAssignment StartMonaMonitoredActivityAssignment
      {
        get => startMonaMonitoredActivityAssignment ??= new();
        set => startMonaMonitoredActivityAssignment = value;
      }

      /// <summary>
      /// A value of StartMonaOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaOfficeServiceProvider")]
      public OfficeServiceProvider StartMonaOfficeServiceProvider
      {
        get => startMonaOfficeServiceProvider ??= new();
        set => startMonaOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaServiceProvider.
      /// </summary>
      [JsonPropertyName("startMonaServiceProvider")]
      public ServiceProvider StartMonaServiceProvider
      {
        get => startMonaServiceProvider ??= new();
        set => startMonaServiceProvider = value;
      }

      /// <summary>
      /// A value of StartMonaOffice.
      /// </summary>
      [JsonPropertyName("startMonaOffice")]
      public Office StartMonaOffice
      {
        get => startMonaOffice ??= new();
        set => startMonaOffice = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private MonitoredActivityAssignment startMonaMonitoredActivityAssignment;
      private OfficeServiceProvider startMonaOfficeServiceProvider;
      private ServiceProvider startMonaServiceProvider;
      private Office startMonaOffice;
    }

    /// <summary>A CaseUnitsGroup group.</summary>
    [Serializable]
    public class CaseUnitsGroup
    {
      /// <summary>
      /// A value of Case1.
      /// </summary>
      [JsonPropertyName("case1")]
      public Case1 Case1
      {
        get => case1 ??= new();
        set => case1 = value;
      }

      /// <summary>
      /// A value of CaseUnit.
      /// </summary>
      [JsonPropertyName("caseUnit")]
      public CaseUnit CaseUnit
      {
        get => caseUnit ??= new();
        set => caseUnit = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Case1 case1;
      private CaseUnit caseUnit;
    }

    /// <summary>A EventsGroup group.</summary>
    [Serializable]
    public class EventsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Infrastructure G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure g;
    }

    /// <summary>A AlertsGroup group.</summary>
    [Serializable]
    public class AlertsGroup
    {
      /// <summary>
      /// A value of Goffice.
      /// </summary>
      [JsonPropertyName("goffice")]
      public Office Goffice
      {
        get => goffice ??= new();
        set => goffice = value;
      }

      /// <summary>
      /// A value of GofficeServiceProvider.
      /// </summary>
      [JsonPropertyName("gofficeServiceProvider")]
      public OfficeServiceProvider GofficeServiceProvider
      {
        get => gofficeServiceProvider ??= new();
        set => gofficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GserviceProvider.
      /// </summary>
      [JsonPropertyName("gserviceProvider")]
      public ServiceProvider GserviceProvider
      {
        get => gserviceProvider ??= new();
        set => gserviceProvider = value;
      }

      /// <summary>
      /// A value of GofficeServiceProviderAlert.
      /// </summary>
      [JsonPropertyName("gofficeServiceProviderAlert")]
      public OfficeServiceProviderAlert GofficeServiceProviderAlert
      {
        get => gofficeServiceProviderAlert ??= new();
        set => gofficeServiceProviderAlert = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Office goffice;
      private OfficeServiceProvider gofficeServiceProvider;
      private ServiceProvider gserviceProvider;
      private OfficeServiceProviderAlert gofficeServiceProviderAlert;
    }

    /// <summary>
    /// Gets a value of EndMona.
    /// </summary>
    [JsonIgnore]
    public Array<EndMonaGroup> EndMona => endMona ??= new(
      EndMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of EndMona for json serialization.
    /// </summary>
    [JsonPropertyName("endMona")]
    [Computed]
    public IList<EndMonaGroup> EndMona_Json
    {
      get => endMona;
      set => EndMona.Assign(value);
    }

    /// <summary>
    /// Gets a value of StartMona.
    /// </summary>
    [JsonIgnore]
    public Array<StartMonaGroup> StartMona => startMona ??= new(
      StartMonaGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StartMona for json serialization.
    /// </summary>
    [JsonPropertyName("startMona")]
    [Computed]
    public IList<StartMonaGroup> StartMona_Json
    {
      get => startMona;
      set => StartMona.Assign(value);
    }

    /// <summary>
    /// Gets a value of CaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<CaseUnitsGroup> CaseUnits => caseUnits ??= new(
      CaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("caseUnits")]
    [Computed]
    public IList<CaseUnitsGroup> CaseUnits_Json
    {
      get => caseUnits;
      set => CaseUnits.Assign(value);
    }

    /// <summary>
    /// Gets a value of Events.
    /// </summary>
    [JsonIgnore]
    public Array<EventsGroup> Events => events ??= new(EventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Events for json serialization.
    /// </summary>
    [JsonPropertyName("events")]
    [Computed]
    public IList<EventsGroup> Events_Json
    {
      get => events;
      set => Events.Assign(value);
    }

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
    /// Gets a value of Alerts.
    /// </summary>
    [JsonIgnore]
    public Array<AlertsGroup> Alerts => alerts ??= new(AlertsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alerts for json serialization.
    /// </summary>
    [JsonPropertyName("alerts")]
    [Computed]
    public IList<AlertsGroup> Alerts_Json
    {
      get => alerts;
      set => Alerts.Assign(value);
    }

    private Array<EndMonaGroup> endMona;
    private Array<StartMonaGroup> startMona;
    private Array<CaseUnitsGroup> caseUnits;
    private Array<EventsGroup> events;
    private Infrastructure infrastructure;
    private Array<AlertsGroup> alerts;
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
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private MonitoredActivity monitoredActivity;
    private Common common;
  }
#endregion
}

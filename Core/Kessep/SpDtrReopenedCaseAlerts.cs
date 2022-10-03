// Program: SP_DTR_REOPENED_CASE_ALERTS, ID: 372070763, model: 746.
// Short name: SWE02132
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DTR_REOPENED_CASE_ALERTS.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrReopenedCaseAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_REOPENED_CASE_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrReopenedCaseAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrReopenedCaseAlerts.
  /// </summary>
  public SpDtrReopenedCaseAlerts(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial development - October 16, 1997
    // Developer - Jack Rookard, MTW
    // This action block is invoked by the Event Processor when a "Case Reopened
    // in another Office" type event is encountered.  It determines the OSP "
    // RSP" assigned to the Case when Case closure occurred and generates a hand
    // -crafted OSP alert to that OSP reminding them to forward the Case paper
    // file to the Office that the Case has been reopened in. If the "case
    // closure" Case Coordinator is discontinued, it determines the supervisor
    // of that Case Coordinator and sends the alert to that supervisor.  If a
    // supervisor cannot be located, it determines who the Office Assistant is
    // for that "case closure" office and sends an alert to the OA. If an OA OSP
    // cannot be found, it finds "any" Collection Officer in the target "case
    // closure" office and sends the alert to that OSP.
    // 05/13/2010 GVandy  CQ966  Return alert to be created in a group view 
    // rather than create the alert from this cab.
    local.Current.Date = Now().Date;

    if (!ReadInfrastructure())
    {
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    if (IsEmpty(import.Infrastructure.CaseNumber))
    {
      ExitState = "CASE_NUMBER_REQUIRED";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    foreach(var item in ReadCaseAssignment())
    {
      ++local.CaseAssignmentCount.Count;

      if (local.CaseAssignmentCount.Count == 1)
      {
        if (ReadOffice())
        {
          // Currency obtained on the current OSP Case Coordinator.  Continue.
        }
        else
        {
          ExitState = "OFFICE_NF";

          return;
        }
      }
      else if (local.CaseAssignmentCount.Count == 2)
      {
        // Currency obtained on the case assignment in effect at time of case 
        // closure.
        // Escape from read each case assignment.
        break;
      }
    }

    if (local.CaseAssignmentCount.Count == 2)
    {
    }
    else
    {
      // The previous Case assignment (prior to case closure) could not be 
      // found. Escape.
      ExitState = "CASE_ASSIGNMENT_NF";

      return;
    }

    if (ReadOfficeServiceProviderOfficeServiceProvider())
    {
      if (Lt(entities.Office.DiscontinueDate, local.Current.Date))
      {
        ExitState = "OFFICE_NF";

        return;
      }
    }
    else
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    if (!Lt(entities.OfficeServiceProvider.DiscontinueDate, local.Current.Date))
    {
      // Send the alert to the last effective Caseworker for the Case.
      local.OfficeServiceProviderAlert.RecipientUserId =
        entities.ServiceProvider.UserId;
      local.ServiceProvider.SystemGeneratedId =
        entities.ServiceProvider.SystemGeneratedId;
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        local.OfficeServiceProvider);
    }
    else
    {
      // The last effective Caseworker for the Case is discontinued. Find an 
      // alternate service provider.
      // Send the alert to the supervisor.
      ReadServiceProviderOfficeServiceProvider3();

      if (!entities.AlternateServiceProvider.Populated)
      {
        // Send the alert to the Office Assistant for the Office in which the 
        // last Caseworker was located.
        ReadServiceProviderOfficeServiceProvider2();
      }

      if (!entities.AlternateServiceProvider.Populated)
      {
        // Send the alert to the first encounted active Collection Officer for 
        // the Office in which the last Caseworker was located.
        ReadServiceProviderOfficeServiceProvider1();
      }

      if (!entities.AlternateServiceProvider.Populated)
      {
        // No appropriate OSP could be found to deliver the alert to.  Set an 
        // exit state and escape.
        ExitState = "INVALID_OPTION_SELECTED";

        return;
      }

      local.OfficeServiceProviderAlert.RecipientUserId =
        entities.AlternateServiceProvider.UserId;
      local.ServiceProvider.SystemGeneratedId =
        entities.AlternateServiceProvider.SystemGeneratedId;
      MoveOfficeServiceProvider(entities.AlternateOfficeServiceProvider,
        local.OfficeServiceProvider);
    }

    export.ImportExportAlerts.Index = export.ImportExportAlerts.Count;
    export.ImportExportAlerts.CheckSize();

    export.ImportExportAlerts.Update.Goffice.SystemGeneratedId =
      entities.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      export.ImportExportAlerts.Update.GofficeServiceProvider);
    export.ImportExportAlerts.Update.GserviceProvider.SystemGeneratedId =
      local.ServiceProvider.SystemGeneratedId;
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
      RecipientUserId = local.OfficeServiceProviderAlert.RecipientUserId;
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
      DistributionDate = local.Current.Date;
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
      OptimizationInd = "N";
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.OptimizedFlag =
      "2";
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.
      PrioritizationCode = 1;
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.TypeCode =
      "AUT";
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Description =
      "Case reopened in a different office from where it was closed. This alert reminds previous CO to forward Case paper file.";
      
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      "Case reopened in Ofc:" + NumberToString
      (entities.New1.SystemGeneratedId, 12, 4);
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      TrimEnd(export.ImportExportAlerts.Item.GofficeServiceProviderAlert.Message)
      + "-pls forward paper file";
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;

        return true;
      });
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

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.New1.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", entities.CaseAssignment.OffId);
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.New1.OffOffice = db.GetNullableInt32(reader, 1);
        entities.New1.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.EffectiveDate = db.GetDate(reader, 5);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.ServiceProvider.UserId = db.GetString(reader, 8);
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider1()
  {
    entities.AlternateOfficeServiceProvider.Populated = false;
    entities.AlternateServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.AlternateServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateServiceProvider.UserId = db.GetString(reader, 1);
        entities.AlternateOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.AlternateOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.AlternateOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.AlternateOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.AlternateOfficeServiceProvider.Populated = true;
        entities.AlternateServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider2()
  {
    entities.AlternateOfficeServiceProvider.Populated = false;
    entities.AlternateServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.AlternateServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateServiceProvider.UserId = db.GetString(reader, 1);
        entities.AlternateOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.AlternateOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.AlternateOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.AlternateOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.AlternateOfficeServiceProvider.Populated = true;
        entities.AlternateServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.AlternateOfficeServiceProvider.Populated = false;
    entities.AlternateServiceProvider.Populated = false;

    return Read("ReadServiceProviderOfficeServiceProvider3",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.AlternateServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.AlternateServiceProvider.UserId = db.GetString(reader, 1);
        entities.AlternateOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.AlternateOfficeServiceProvider.RoleCode =
          db.GetString(reader, 3);
        entities.AlternateOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 4);
        entities.AlternateOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.AlternateOfficeServiceProvider.Populated = true;
        entities.AlternateServiceProvider.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportAlertsGroup group.</summary>
    [Serializable]
    public class ImportExportAlertsGroup
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
    /// Gets a value of ImportExportAlerts.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportAlertsGroup> ImportExportAlerts =>
      importExportAlerts ??= new(ImportExportAlertsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportAlerts for json serialization.
    /// </summary>
    [JsonPropertyName("importExportAlerts")]
    [Computed]
    public IList<ImportExportAlertsGroup> ImportExportAlerts_Json
    {
      get => importExportAlerts;
      set => ImportExportAlerts.Assign(value);
    }

    private Array<ImportExportAlertsGroup> importExportAlerts;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of CaseAssignmentCount.
    /// </summary>
    [JsonPropertyName("caseAssignmentCount")]
    public Common CaseAssignmentCount
    {
      get => caseAssignmentCount ??= new();
      set => caseAssignmentCount = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Common caseAssignmentCount;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AlternateOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("alternateOfficeServiceProvider")]
    public OfficeServiceProvider AlternateOfficeServiceProvider
    {
      get => alternateOfficeServiceProvider ??= new();
      set => alternateOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of AlternateServiceProvider.
    /// </summary>
    [JsonPropertyName("alternateServiceProvider")]
    public ServiceProvider AlternateServiceProvider
    {
      get => alternateServiceProvider ??= new();
      set => alternateServiceProvider = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Office New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private OfficeServiceProvider alternateOfficeServiceProvider;
    private ServiceProvider alternateServiceProvider;
    private Office new1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private Infrastructure infrastructure;
    private Case1 case1;
  }
#endregion
}

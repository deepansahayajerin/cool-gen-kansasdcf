// Program: SP_DTR_CLOSED_CASE_ALERTS, ID: 372070765, model: 746.
// Short name: SWE02123
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
/// A program: SP_DTR_CLOSED_CASE_ALERTS.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrClosedCaseAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_CLOSED_CASE_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrClosedCaseAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrClosedCaseAlerts.
  /// </summary>
  public SpDtrClosedCaseAlerts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // Initial development - October 1, 1997
    // Developer - Jack Rookard, MTW
    // This action block is invoked by the Event Processor when a "AP located" 
    // type event is encountered.  It determines if the import Case is closed,
    // and if so, the type of case closure.  If the case was closed for "
    // inability to locate," an alert is created for the last Case Coordinator.
    // If the last Case Coordinator is discontinued, it determines the
    // supervisor of that Case Coordinator.  If a supervisor cannot be located,
    // it determines who the Office Assistant is for that office and sends an
    // alert. If an Office Assistance OSP cannot be found, it finds "any"
    // Collection Officer in the target office and sends the alert to that OSP.
    // 05/13/2010 GVandy  CQ966  Return alert to be created in a group view 
    // rather than create the alert from this cab.
    local.Current.Date = Now().Date;

    if (!ReadInfrastructure())
    {
      ExitState = "INFRASTRUCTURE_NF";

      return;
    }

    if (IsEmpty(import.Infrastructure.CsePersonNumber))
    {
      ExitState = "CO0000_AP_NF";

      return;
    }

    if (IsEmpty(import.Infrastructure.CaseNumber))
    {
      ExitState = "CASE_NUMBER_REQUIRED";

      return;
    }

    if (import.Infrastructure.CaseUnitNumber.GetValueOrDefault() == 0)
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // -- Continue only if case is currently closed for "NO LOCATE"
    if (AsChar(entities.Case1.Status) == 'C' && !
      Lt(local.Current.Date, entities.Case1.StatusDate))
    {
      if (Equal(entities.Case1.ClosureReason, "NL"))
      {
      }
      else
      {
        return;
      }
    }
    else
    {
      return;
    }

    ReadCaseUnit();

    if (!entities.CaseUnit.Populated)
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    if (import.Infrastructure.CaseUnitNumber.GetValueOrDefault() == entities
      .CaseUnit.CuNumber)
    {
    }
    else
    {
      return;
    }

    if (!ReadCsePerson())
    {
      ExitState = "CO0000_AP_CSE_PERSON_NF";

      return;
    }

    // -- Find a service provider to whom the closed case alert will be sent.
    ReadCaseAssignment();

    if (!entities.CaseAssignment.Populated)
    {
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
        // No appropriate service provider could be found to deliver the alert 
        // to.  Set an exit state and escape.
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
      "An AP has been located on a Closed Case where the Case was closed for inability to locate AP.";
      
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      "AP " + (import.Infrastructure.CsePersonNumber ?? "");
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      TrimEnd(export.ImportExportAlerts.Item.GofficeServiceProviderAlert.Message)
      + " on Closed Case:";
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      TrimEnd(export.ImportExportAlerts.Item.GofficeServiceProviderAlert.Message)
      + entities.Case1.Number;
    export.ImportExportAlerts.Update.GofficeServiceProviderAlert.Message =
      TrimEnd(export.ImportExportAlerts.Item.GofficeServiceProviderAlert.Message)
      + " located";
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
        entities.Case1.ClosureReason = db.GetNullableString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Status = db.GetNullableString(reader, 2);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 3);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
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
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(
          command, "cspNoAp", import.Infrastructure.CsePersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 2);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ap.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.Ap.Number = db.GetString(reader, 0);
        entities.Ap.Type1 = db.GetString(reader, 1);
        entities.Ap.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ap.Type1);
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

  private bool ReadOfficeServiceProviderOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

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
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider1()
  {
    entities.AlternateServiceProvider.Populated = false;
    entities.AlternateOfficeServiceProvider.Populated = false;

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
        entities.AlternateServiceProvider.Populated = true;
        entities.AlternateOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider2()
  {
    entities.AlternateServiceProvider.Populated = false;
    entities.AlternateOfficeServiceProvider.Populated = false;

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
        entities.AlternateServiceProvider.Populated = true;
        entities.AlternateOfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProviderOfficeServiceProvider3()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.AlternateServiceProvider.Populated = false;
    entities.AlternateOfficeServiceProvider.Populated = false;

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
        entities.AlternateServiceProvider.Populated = true;
        entities.AlternateOfficeServiceProvider.Populated = true;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of AlternateOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("alternateOfficeServiceProvider")]
    public OfficeServiceProvider AlternateOfficeServiceProvider
    {
      get => alternateOfficeServiceProvider ??= new();
      set => alternateOfficeServiceProvider = value;
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private ServiceProvider alternateServiceProvider;
    private OfficeServiceProvider alternateOfficeServiceProvider;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private Office office;
    private CaseAssignment caseAssignment;
    private Infrastructure infrastructure;
    private CsePerson ap;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}

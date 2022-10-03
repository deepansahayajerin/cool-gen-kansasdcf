// Program: SP_EVENT_PROC_EXCEPTN_ROUTINE, ID: 372068785, model: 746.
// Short name: SWE02119
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_EVENT_PROC_EXCEPTN_ROUTINE.
/// </para>
/// <para>
/// This is an exception routine for handling scenarios which do not fit the 
/// normal processing of events.
/// Eg. If an AP's address is no longer verified, we need to ensure that he is 
/// still &quot;located&quot; (by checking for other locate info - jail etc.)
/// before transforming the case unit back to locate phase.
/// </para>
/// </summary>
[Serializable]
public partial class SpEventProcExceptnRoutine: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_EVENT_PROC_EXCEPTN_ROUTINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpEventProcExceptnRoutine(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpEventProcExceptnRoutine.
  /// </summary>
  public SpEventProcExceptnRoutine(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Insert Exception Routine CAB here.
    // Set exit states as needed.
    // If u need to abort any further processing of this Infrast
    // record set the Abort Flag to Y.
    // Aborts should be set rarely, if at all.
    // --------------------------------------------------------------
    switch(TrimEnd(import.EventDetail.ExceptionRoutine ?? ""))
    {
      case "DOCPRINT":
        break;
      case "CASEREOP":
        UseSpDtrReopenedCaseAlerts();

        break;
      case "DELOBLIG":
        UseSpDtrCuOblgStatus();

        break;
      case "VERLOCAT":
        UseSpDtrApLocateStatus();

        break;
      case "PREV_CC":
        UseSpDtrClosedCaseAlerts();

        break;
      default:
        break;
    }
  }

  private static void MoveCaseUnit(CaseUnit source, CaseUnit target)
  {
    target.CuNumber = source.CuNumber;
    target.State = source.State;
  }

  private static void MoveImportExportAlerts1(SpDtrClosedCaseAlerts.Export.
    ImportExportAlertsGroup source, Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportAlerts2(SpDtrReopenedCaseAlerts.Export.
    ImportExportAlertsGroup source, Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportAlerts3(Export.
    ImportExportAlertsGroup source,
    SpDtrClosedCaseAlerts.Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportAlerts4(Export.
    ImportExportAlertsGroup source,
    SpDtrReopenedCaseAlerts.Export.ImportExportAlertsGroup target)
  {
    target.Goffice.SystemGeneratedId = source.Goffice.SystemGeneratedId;
    MoveOfficeServiceProvider(source.GofficeServiceProvider,
      target.GofficeServiceProvider);
    target.GserviceProvider.SystemGeneratedId =
      source.GserviceProvider.SystemGeneratedId;
    target.GofficeServiceProviderAlert.
      Assign(source.GofficeServiceProviderAlert);
  }

  private static void MoveImportExportCaseUnits1(SpDtrCuOblgStatus.Export.
    ImportExportCaseUnitsGroup source,
    Export.ImportExportCaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveImportExportCaseUnits2(Export.
    ImportExportCaseUnitsGroup source,
    SpDtrCuOblgStatus.Export.ImportExportCaseUnitsGroup target)
  {
    target.Case1.Number = source.Case1.Number;
    MoveCaseUnit(source.CaseUnit, target.CaseUnit);
  }

  private static void MoveImportExportEvents1(SpDtrApLocateStatus.Export.
    ImportExportEventsGroup source, Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportEvents2(SpDtrCuOblgStatus.Export.
    ImportExportEventsGroup source, Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportEvents3(Export.
    ImportExportEventsGroup source,
    SpDtrApLocateStatus.Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveImportExportEvents4(Export.
    ImportExportEventsGroup source,
    SpDtrCuOblgStatus.Export.ImportExportEventsGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CaseNumber = source.CaseNumber;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.CaseNumber = source.CaseNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private void UseSpDtrApLocateStatus()
  {
    var useImport = new SpDtrApLocateStatus.Import();
    var useExport = new SpDtrApLocateStatus.Export();

    MoveInfrastructure3(import.Infrastructure, useImport.Infrastructure);
    export.ImportExportEvents.CopyTo(
      useExport.ImportExportEvents, MoveImportExportEvents3);

    Call(SpDtrApLocateStatus.Execute, useImport, useExport);

    useExport.ImportExportEvents.CopyTo(
      export.ImportExportEvents, MoveImportExportEvents1);
  }

  private void UseSpDtrClosedCaseAlerts()
  {
    var useImport = new SpDtrClosedCaseAlerts.Import();
    var useExport = new SpDtrClosedCaseAlerts.Export();

    MoveInfrastructure2(import.Infrastructure, useImport.Infrastructure);
    export.ImportExportAlerts.CopyTo(
      useExport.ImportExportAlerts, MoveImportExportAlerts3);

    Call(SpDtrClosedCaseAlerts.Execute, useImport, useExport);

    useExport.ImportExportAlerts.CopyTo(
      export.ImportExportAlerts, MoveImportExportAlerts1);
  }

  private void UseSpDtrCuOblgStatus()
  {
    var useImport = new SpDtrCuOblgStatus.Import();
    var useExport = new SpDtrCuOblgStatus.Export();

    MoveInfrastructure2(import.Infrastructure, useImport.Infrastructure);
    export.ImportExportEvents.CopyTo(
      useExport.ImportExportEvents, MoveImportExportEvents4);
    export.ImportExportCaseUnits.CopyTo(
      useExport.ImportExportCaseUnits, MoveImportExportCaseUnits2);

    Call(SpDtrCuOblgStatus.Execute, useImport, useExport);

    useExport.ImportExportEvents.CopyTo(
      export.ImportExportEvents, MoveImportExportEvents2);
    useExport.ImportExportCaseUnits.CopyTo(
      export.ImportExportCaseUnits, MoveImportExportCaseUnits1);
  }

  private void UseSpDtrReopenedCaseAlerts()
  {
    var useImport = new SpDtrReopenedCaseAlerts.Import();
    var useExport = new SpDtrReopenedCaseAlerts.Export();

    MoveInfrastructure1(import.Infrastructure, useImport.Infrastructure);
    export.ImportExportAlerts.CopyTo(
      useExport.ImportExportAlerts, MoveImportExportAlerts4);

    Call(SpDtrReopenedCaseAlerts.Execute, useImport, useExport);

    useExport.ImportExportAlerts.CopyTo(
      export.ImportExportAlerts, MoveImportExportAlerts2);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private EventDetail eventDetail;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportEventsGroup group.</summary>
    [Serializable]
    public class ImportExportEventsGroup
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

    /// <summary>A ImportExportCaseUnitsGroup group.</summary>
    [Serializable]
    public class ImportExportCaseUnitsGroup
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

    /// <summary>
    /// Gets a value of ImportExportEvents.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportEventsGroup> ImportExportEvents =>
      importExportEvents ??= new(ImportExportEventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportEvents for json serialization.
    /// </summary>
    [JsonPropertyName("importExportEvents")]
    [Computed]
    public IList<ImportExportEventsGroup> ImportExportEvents_Json
    {
      get => importExportEvents;
      set => ImportExportEvents.Assign(value);
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

    /// <summary>
    /// Gets a value of ImportExportCaseUnits.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportCaseUnitsGroup> ImportExportCaseUnits =>
      importExportCaseUnits ??= new(ImportExportCaseUnitsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportCaseUnits for json serialization.
    /// </summary>
    [JsonPropertyName("importExportCaseUnits")]
    [Computed]
    public IList<ImportExportCaseUnitsGroup> ImportExportCaseUnits_Json
    {
      get => importExportCaseUnits;
      set => ImportExportCaseUnits.Assign(value);
    }

    private Array<ImportExportEventsGroup> importExportEvents;
    private Array<ImportExportAlertsGroup> importExportAlerts;
    private Array<ImportExportCaseUnitsGroup> importExportCaseUnits;
  }
#endregion
}

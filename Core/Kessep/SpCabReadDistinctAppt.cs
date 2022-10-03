// Program: SP_CAB_READ_DISTINCT_APPT, ID: 371749363, model: 746.
// Short name: SWE01906
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
/// A program: SP_CAB_READ_DISTINCT_APPT.
/// </para>
/// <para>
///   CAB created to read a particular appointment based on a passed timestamp 
/// view from HIST only at this point, but can read a distinct appointment based
/// on the population of the local timestamp view.
/// RVW 1/23/97
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReadDistinctAppt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_DISTINCT_APPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadDistinctAppt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadDistinctAppt.
  /// </summary>
  public SpCabReadDistinctAppt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 10/24/96 Regan Welborn               Initial Development
    // ------------------------------------------------------------
    export.Export1.Index = -1;

    if (ReadAppointmentOfficeServiceProviderCsePersonCase())
    {
      export.Export1.Index = 0;
      export.Export1.CheckSize();

      export.HiddenCheck.Index = export.Export1.Index;
      export.HiddenCheck.CheckSize();

      MoveAppointment(entities.Appointment, export.Export1.Update.GrAppointment);
        

      if (entities.Appointment.Time > new TimeSpan(13, 0, 0))
      {
        export.Export1.Update.GrAppointment.Time = entities.Appointment.Time - new
          TimeSpan(12, 0, 0);
        export.Export1.Update.GrExportApptAmPmInd.SelectChar = "P";
      }
      else if (entities.Appointment.Time >= new TimeSpan(12, 0, 1))
      {
        export.Export1.Update.GrExportApptAmPmInd.SelectChar = "P";
      }
      else
      {
        export.Export1.Update.GrExportApptAmPmInd.SelectChar = "A";
      }

      export.Export1.Update.GrCsePerson.Number = entities.CsePerson.Number;
      export.Export1.Update.GrOffice.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
      export.Export1.Update.GrServiceProvider.UserId =
        entities.ServiceProvider.UserId;
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        export.Export1.Update.GrOfficeServiceProvider);
      export.Export1.Update.GrCase.Number = entities.Case1.Number;
      MoveCaseRole(entities.CaseRole, export.Export1.Update.GrCaseRole);
      export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
        export.Export1.Item.GrAppointment);
      export.HiddenCheck.Update.GrExportHiddenApptAmPmInd.SelectChar =
        export.Export1.Item.GrExportApptAmPmInd.SelectChar;
      export.HiddenCheck.Update.GrExportHiddenCheckCsePerson.Number =
        entities.CsePerson.Number;
      export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
      export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider.UserId =
        entities.ServiceProvider.UserId;
      export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
        RoleCode = entities.OfficeServiceProvider.RoleCode;
      export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
        entities.Case1.Number;
      export.SearchCase.Number = entities.Case1.Number;
      export.SearchCsePerson.Number = entities.CsePerson.Number;
      MoveOffice(entities.Office, export.SearchOffice);
      MoveOfficeServiceProvider(entities.OfficeServiceProvider,
        export.SearchOfficeServiceProvider);
      export.SearchServiceProvider.UserId = entities.ServiceProvider.UserId;
      export.PageNumber.PageNumber = 1;
      export.DisplayAll.Flag = "Y";
      export.Starting.Date = entities.Appointment.Date;
    }
    else
    {
      ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

      return;
    }

    export.Standard.ScrollingMessage = "More";
  }

  private static void MoveAppointment(Appointment source, Appointment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Type1 = source.Type1;
    target.Result = source.Result;
    target.Date = source.Date;
    target.Time = source.Time;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadAppointmentOfficeServiceProviderCsePersonCase()
  {
    entities.Appointment.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadAppointmentOfficeServiceProviderCsePersonCase",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.DistinctTimestamp.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Appointment.ReasonCode = db.GetString(reader, 0);
        entities.Appointment.Type1 = db.GetString(reader, 1);
        entities.Appointment.Result = db.GetString(reader, 2);
        entities.Appointment.Date = db.GetDate(reader, 3);
        entities.Appointment.Time = db.GetTimeSpan(reader, 4);
        entities.Appointment.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Appointment.SpdId = db.GetNullableInt32(reader, 6);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 6);
        entities.Appointment.OffId = db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 7);
        entities.Appointment.OspRoleCode = db.GetNullableString(reader, 8);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 8);
        entities.Appointment.OspDate = db.GetNullableDate(reader, 9);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 9);
        entities.Appointment.AppTstamp = db.GetNullableDateTime(reader, 10);
        entities.Appointment.CasNumber = db.GetNullableString(reader, 11);
        entities.Case1.Number = db.GetString(reader, 11);
        entities.CaseRole.CasNumber = db.GetString(reader, 11);
        entities.CaseRole.CasNumber = db.GetString(reader, 11);
        entities.Appointment.CspNumber = db.GetNullableString(reader, 12);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.CaseRole.CspNumber = db.GetString(reader, 12);
        entities.CaseRole.CspNumber = db.GetString(reader, 12);
        entities.Appointment.CroType = db.GetNullableString(reader, 13);
        entities.CaseRole.Type1 = db.GetString(reader, 13);
        entities.Appointment.CroId = db.GetNullableInt32(reader, 14);
        entities.CaseRole.Identifier = db.GetInt32(reader, 14);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 15);
        entities.Office.Name = db.GetString(reader, 16);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 17);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 18);
        entities.ServiceProvider.UserId = db.GetString(reader, 19);
        entities.Appointment.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// A value of DistinctTimestamp.
    /// </summary>
    [JsonPropertyName("distinctTimestamp")]
    public Appointment DistinctTimestamp
    {
      get => distinctTimestamp ??= new();
      set => distinctTimestamp = value;
    }

    private Appointment distinctTimestamp;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of GrOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grOfficeServiceProvider")]
      public OfficeServiceProvider GrOfficeServiceProvider
      {
        get => grOfficeServiceProvider ??= new();
        set => grOfficeServiceProvider = value;
      }

      /// <summary>
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrCase.
      /// </summary>
      [JsonPropertyName("grCase")]
      public Case1 GrCase
      {
        get => grCase ??= new();
        set => grCase = value;
      }

      /// <summary>
      /// A value of GrCsePerson.
      /// </summary>
      [JsonPropertyName("grCsePerson")]
      public CsePerson GrCsePerson
      {
        get => grCsePerson ??= new();
        set => grCsePerson = value;
      }

      /// <summary>
      /// A value of GrExportCsePerson.
      /// </summary>
      [JsonPropertyName("grExportCsePerson")]
      public Common GrExportCsePerson
      {
        get => grExportCsePerson ??= new();
        set => grExportCsePerson = value;
      }

      /// <summary>
      /// A value of GrCaseRole.
      /// </summary>
      [JsonPropertyName("grCaseRole")]
      public CaseRole GrCaseRole
      {
        get => grCaseRole ??= new();
        set => grCaseRole = value;
      }

      /// <summary>
      /// A value of GrAppointment.
      /// </summary>
      [JsonPropertyName("grAppointment")]
      public Appointment GrAppointment
      {
        get => grAppointment ??= new();
        set => grAppointment = value;
      }

      /// <summary>
      /// A value of GrExportApptAmPmInd.
      /// </summary>
      [JsonPropertyName("grExportApptAmPmInd")]
      public Common GrExportApptAmPmInd
      {
        get => grExportApptAmPmInd ??= new();
        set => grExportApptAmPmInd = value;
      }

      /// <summary>
      /// A value of GrExportCdvlReason.
      /// </summary>
      [JsonPropertyName("grExportCdvlReason")]
      public Common GrExportCdvlReason
      {
        get => grExportCdvlReason ??= new();
        set => grExportCdvlReason = value;
      }

      /// <summary>
      /// A value of GrExportCdvlType.
      /// </summary>
      [JsonPropertyName("grExportCdvlType")]
      public Common GrExportCdvlType
      {
        get => grExportCdvlType ??= new();
        set => grExportCdvlType = value;
      }

      /// <summary>
      /// A value of GrExportCdvlResult.
      /// </summary>
      [JsonPropertyName("grExportCdvlResult")]
      public Common GrExportCdvlResult
      {
        get => grExportCdvlResult ??= new();
        set => grExportCdvlResult = value;
      }

      /// <summary>
      /// A value of GrOffice.
      /// </summary>
      [JsonPropertyName("grOffice")]
      public Office GrOffice
      {
        get => grOffice ??= new();
        set => grOffice = value;
      }

      /// <summary>
      /// A value of GrServiceProvider.
      /// </summary>
      [JsonPropertyName("grServiceProvider")]
      public ServiceProvider GrServiceProvider
      {
        get => grServiceProvider ??= new();
        set => grServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportServiceProvider")]
      public Common GrExportServiceProvider
      {
        get => grExportServiceProvider ??= new();
        set => grExportServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private OfficeServiceProvider grOfficeServiceProvider;
      private Common grCommon;
      private Case1 grCase;
      private CsePerson grCsePerson;
      private Common grExportCsePerson;
      private CaseRole grCaseRole;
      private Appointment grAppointment;
      private Common grExportApptAmPmInd;
      private Common grExportCdvlReason;
      private Common grExportCdvlType;
      private Common grExportCdvlResult;
      private Office grOffice;
      private ServiceProvider grServiceProvider;
      private Common grExportServiceProvider;
    }

    /// <summary>A HiddenCheckGroup group.</summary>
    [Serializable]
    public class HiddenCheckGroup
    {
      /// <summary>
      /// A value of GrExportHiddenCheckCase.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckCase")]
      public Case1 GrExportHiddenCheckCase
      {
        get => grExportHiddenCheckCase ??= new();
        set => grExportHiddenCheckCase = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckCsePerson.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckCsePerson")]
      public CsePerson GrExportHiddenCheckCsePerson
      {
        get => grExportHiddenCheckCsePerson ??= new();
        set => grExportHiddenCheckCsePerson = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckAppointment.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckAppointment")]
      public Appointment GrExportHiddenCheckAppointment
      {
        get => grExportHiddenCheckAppointment ??= new();
        set => grExportHiddenCheckAppointment = value;
      }

      /// <summary>
      /// A value of GrExportHiddenApptAmPmInd.
      /// </summary>
      [JsonPropertyName("grExportHiddenApptAmPmInd")]
      public Common GrExportHiddenApptAmPmInd
      {
        get => grExportHiddenApptAmPmInd ??= new();
        set => grExportHiddenApptAmPmInd = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckOffice.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckOffice")]
      public Office GrExportHiddenCheckOffice
      {
        get => grExportHiddenCheckOffice ??= new();
        set => grExportHiddenCheckOffice = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckServiceProvider")]
      public ServiceProvider GrExportHiddenCheckServiceProvider
      {
        get => grExportHiddenCheckServiceProvider ??= new();
        set => grExportHiddenCheckServiceProvider = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCheckOfficeServiceProvider.
      /// </summary>
      [JsonPropertyName("grExportHiddenCheckOfficeServiceProvider")]
      public OfficeServiceProvider GrExportHiddenCheckOfficeServiceProvider
      {
        get => grExportHiddenCheckOfficeServiceProvider ??= new();
        set => grExportHiddenCheckOfficeServiceProvider = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Case1 grExportHiddenCheckCase;
      private CsePerson grExportHiddenCheckCsePerson;
      private Appointment grExportHiddenCheckAppointment;
      private Common grExportHiddenApptAmPmInd;
      private Office grExportHiddenCheckOffice;
      private ServiceProvider grExportHiddenCheckServiceProvider;
      private OfficeServiceProvider grExportHiddenCheckOfficeServiceProvider;
    }

    /// <summary>
    /// A value of DisplayAll.
    /// </summary>
    [JsonPropertyName("displayAll")]
    public Common DisplayAll
    {
      get => displayAll ??= new();
      set => displayAll = value;
    }

    /// <summary>
    /// A value of SearchCase.
    /// </summary>
    [JsonPropertyName("searchCase")]
    public Case1 SearchCase
    {
      get => searchCase ??= new();
      set => searchCase = value;
    }

    /// <summary>
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public Office SearchOffice
    {
      get => searchOffice ??= new();
      set => searchOffice = value;
    }

    /// <summary>
    /// A value of SearchOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchOfficeServiceProvider")]
    public OfficeServiceProvider SearchOfficeServiceProvider
    {
      get => searchOfficeServiceProvider ??= new();
      set => searchOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of PageKeys.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    public Appointment PageKeys
    {
      get => pageKeys ??= new();
      set => pageKeys = value;
    }

    /// <summary>
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenCheck.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenCheckGroup> HiddenCheck => hiddenCheck ??= new(
      HiddenCheckGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenCheck for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    [Computed]
    public IList<HiddenCheckGroup> HiddenCheck_Json
    {
      get => hiddenCheck;
      set => HiddenCheck.Assign(value);
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Common displayAll;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private Office searchOffice;
    private OfficeServiceProvider searchOfficeServiceProvider;
    private ServiceProvider searchServiceProvider;
    private DateWorkArea starting;
    private Appointment pageKeys;
    private Standard pageNumber;
    private Array<ExportGroup> export1;
    private Array<HiddenCheckGroup> hiddenCheck;
    private Standard standard;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private Appointment appointment;
    private CsePerson csePerson;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseRole caseRole;
  }
#endregion
}

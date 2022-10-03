// Program: SP_CAB_READ_PREDECESSOR_APPTS, ID: 371749356, model: 746.
// Short name: SWE01773
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
/// A program: SP_CAB_READ_PREDECESSOR_APPTS.
/// </para>
/// <para>
///   This cab will read, beginning with the import appointment, the predecessor
/// chain of appointments ending with the earliest appointment.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReadPredecessorAppts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_PREDECESSOR_APPTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadPredecessorAppts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadPredecessorAppts.
  /// </summary>
  public SpCabReadPredecessorAppts(IContext context, Import import,
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
    local.Rolling.CreatedTimestamp = import.PageKeys.CreatedTimestamp;

    if (Equal(global.Command, "PRED"))
    {
      export.Export1.Index = 0;
      export.Export1.CheckSize();

      export.HiddenCheck.Index = 0;
      export.HiddenCheck.CheckSize();

      export.Export1.Update.GrAppointment.Assign(import.Import1.GrAppointment);
      export.Export1.Update.GrExportApptAmPmInd.SelectChar =
        import.Import1.GrImportApptAmPmInd.SelectChar;
      MoveCaseRole(import.Import1.GrCaseRole, export.Export1.Update.GrCaseRole);
      export.Export1.Update.GrCase.Number = import.Import1.GrCase.Number;
      MoveCsePerson(import.Import1.GrCsePerson,
        export.Export1.Update.GrCsePerson);
      export.Export1.Update.GrOffice.SystemGeneratedId =
        import.Import1.GrOffice.SystemGeneratedId;
      export.Export1.Update.GrOfficeServiceProvider.RoleCode =
        import.Import1.GrOfficeServiceProvider.RoleCode;
      MoveServiceProvider(import.Import1.GrServiceProvider,
        export.Export1.Update.GrServiceProvider);
      export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
        import.Import1.GrAppointment);
      export.HiddenCheck.Update.GrExportHiddenApptAmPmInd.SelectChar =
        import.Import1.GrImportApptAmPmInd.SelectChar;
      MoveCaseRole(import.Import1.GrCaseRole,
        export.HiddenCheck.Update.GrExportHiddenCaseRole);
      export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
        import.Import1.GrCase.Number;
      MoveCsePerson(import.Import1.GrCsePerson,
        export.HiddenCheck.Update.GrExportHiddenCheckCsePerson);
      export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
        import.Import1.GrOffice.SystemGeneratedId;
      export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
        RoleCode = import.Import1.GrOfficeServiceProvider.RoleCode;
      MoveServiceProvider(import.Import1.GrServiceProvider,
        export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
    }
    else if (ReadAppointmentOfficeServiceProviderCsePersonCase2())
    {
      ReadCaseCaseRoleCsePerson();

      export.Export1.Index = 0;
      export.Export1.CheckSize();

      export.HiddenCheck.Index = 0;
      export.HiddenCheck.CheckSize();

      MoveAppointment1(entities.Appointment, export.Export1.Update.GrAppointment);
        

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

      export.Export1.Update.GrCase.Number = entities.Case1.Number;
      export.Export1.Update.GrCsePerson.Number = entities.CsePerson.Number;
      MoveCaseRole(entities.CaseRole, export.Export1.Update.GrCaseRole);
      export.Export1.Update.GrOffice.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
      MoveServiceProvider(entities.ServiceProvider,
        export.Export1.Update.GrServiceProvider);
      export.Export1.Update.GrOfficeServiceProvider.RoleCode =
        entities.OfficeServiceProvider.RoleCode;
      export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
        export.Export1.Item.GrAppointment);
      export.HiddenCheck.Update.GrExportHiddenApptAmPmInd.SelectChar =
        export.Export1.Item.GrExportApptAmPmInd.SelectChar;
      export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
        entities.Case1.Number;
      export.HiddenCheck.Update.GrExportHiddenCheckCsePerson.Number =
        entities.CsePerson.Number;
      export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
        entities.Office.SystemGeneratedId;
      MoveServiceProvider(entities.ServiceProvider,
        export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
      export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
        RoleCode = entities.OfficeServiceProvider.RoleCode;
    }

    while(!export.Export1.IsFull)
    {
      if (ReadAppointmentOfficeServiceProviderCsePersonCase1())
      {
        ReadCaseCaseRoleCsePerson();

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.HiddenCheck.Index = export.Export1.Index;
        export.HiddenCheck.CheckSize();

        MoveAppointment1(entities.Appointment,
          export.Export1.Update.GrAppointment);

        if (entities.Appointment.Time > new TimeSpan(13, 0, 0))
        {
          export.Export1.Update.GrAppointment.Time =
            entities.Appointment.Time - new TimeSpan(12, 0, 0);
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

        export.Export1.Update.GrCase.Number = entities.Case1.Number;
        export.Export1.Update.GrCsePerson.Number = entities.CsePerson.Number;
        MoveCaseRole(entities.CaseRole, export.Export1.Update.GrCaseRole);
        export.Export1.Update.GrOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        MoveServiceProvider(entities.ServiceProvider,
          export.Export1.Update.GrServiceProvider);
        export.Export1.Update.GrOfficeServiceProvider.RoleCode =
          entities.OfficeServiceProvider.RoleCode;
        export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
          export.Export1.Item.GrAppointment);
        export.HiddenCheck.Update.GrExportHiddenApptAmPmInd.SelectChar =
          export.Export1.Item.GrExportApptAmPmInd.SelectChar;
        export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
          entities.Case1.Number;
        export.HiddenCheck.Update.GrExportHiddenCheckCsePerson.Number =
          entities.CsePerson.Number;
        export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        MoveServiceProvider(entities.ServiceProvider,
          export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
        export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
          RoleCode = entities.OfficeServiceProvider.RoleCode;
        local.Rolling.CreatedTimestamp = entities.Appointment.CreatedTimestamp;
      }
      else
      {
        break;
      }
    }

    if (export.Export1.IsEmpty)
    {
      if (import.PageNumber.PageNumber == 1)
      {
        export.Standard.ScrollingMessage = "";
        ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

        return;
      }
    }

    if (export.Export1.IsFull)
    {
      if (entities.Appointment.Populated)
      {
        if (!Equal(global.Command, "PREV"))
        {
          export.PageNumber.PageNumber = import.PageNumber.PageNumber + 1;
          MoveAppointment2(entities.Appointment, export.PageKeys);
        }

        if (import.PageNumber.PageNumber > 1)
        {
          export.Standard.ScrollingMessage = "More - +";
        }
        else
        {
          export.PageNumber.PageNumber = import.PageNumber.PageNumber + 1;
          export.Standard.ScrollingMessage = "More   +";
        }
      }
      else
      {
        export.PageKeys.Assign(import.PageKeys);
        export.Standard.ScrollingMessage = "";
      }
    }
    else
    {
      export.PageNumber.PageNumber = import.PageNumber.PageNumber;
      export.PageKeys.Assign(import.PageKeys);

      if (import.PageNumber.PageNumber <= 1)
      {
        export.Standard.ScrollingMessage = "";
      }
      else
      {
        export.Standard.ScrollingMessage = "More -";
      }
    }
  }

  private static void MoveAppointment1(Appointment source, Appointment target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Type1 = source.Type1;
    target.Result = source.Result;
    target.Date = source.Date;
    target.Time = source.Time;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveAppointment2(Appointment source, Appointment target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private bool ReadAppointmentOfficeServiceProviderCsePersonCase1()
  {
    entities.Appointment.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadAppointmentOfficeServiceProviderCsePersonCase1",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "appTstamp",
          local.Rolling.CreatedTimestamp.GetValueOrDefault());
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
        entities.Appointment.CspNumber = db.GetNullableString(reader, 12);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.Appointment.CroType = db.GetNullableString(reader, 13);
        entities.Appointment.CroId = db.GetNullableInt32(reader, 14);
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
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);
      });
  }

  private bool ReadAppointmentOfficeServiceProviderCsePersonCase2()
  {
    entities.Appointment.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;

    return Read("ReadAppointmentOfficeServiceProviderCsePersonCase2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Rolling.CreatedTimestamp.GetValueOrDefault());
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
        entities.Appointment.CspNumber = db.GetNullableString(reader, 12);
        entities.CsePerson.Number = db.GetString(reader, 12);
        entities.Appointment.CroType = db.GetNullableString(reader, 13);
        entities.Appointment.CroId = db.GetNullableInt32(reader, 14);
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
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);
      });
  }

  private bool ReadCaseCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Case1.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CsePerson.Populated = true;
        entities.Case1.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
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
      /// A value of GrImportApptAmPmInd.
      /// </summary>
      [JsonPropertyName("grImportApptAmPmInd")]
      public Common GrImportApptAmPmInd
      {
        get => grImportApptAmPmInd ??= new();
        set => grImportApptAmPmInd = value;
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

      private OfficeServiceProvider grOfficeServiceProvider;
      private Case1 grCase;
      private CsePerson grCsePerson;
      private CaseRole grCaseRole;
      private Appointment grAppointment;
      private Common grImportApptAmPmInd;
      private Office grOffice;
      private ServiceProvider grServiceProvider;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonPropertyName("import1")]
    public ImportGroup Import1
    {
      get => import1 ?? (import1 = new());
      set => import1 = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Appointment Starting
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

    private ImportGroup import1;
    private Appointment starting;
    private Appointment pageKeys;
    private Standard pageNumber;
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
      /// A value of GrExportHiddenCaseRole.
      /// </summary>
      [JsonPropertyName("grExportHiddenCaseRole")]
      public CaseRole GrExportHiddenCaseRole
      {
        get => grExportHiddenCaseRole ??= new();
        set => grExportHiddenCaseRole = value;
      }

      /// <summary>
      /// A value of GrExportHiddenCommon.
      /// </summary>
      [JsonPropertyName("grExportHiddenCommon")]
      public Common GrExportHiddenCommon
      {
        get => grExportHiddenCommon ??= new();
        set => grExportHiddenCommon = value;
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
      private CaseRole grExportHiddenCaseRole;
      private Common grExportHiddenCommon;
      private Appointment grExportHiddenCheckAppointment;
      private Common grExportHiddenApptAmPmInd;
      private Office grExportHiddenCheckOffice;
      private ServiceProvider grExportHiddenCheckServiceProvider;
      private OfficeServiceProvider grExportHiddenCheckOfficeServiceProvider;
    }

    /// <summary>A ZdelGroupExport1Group group.</summary>
    [Serializable]
    public class ZdelGroupExport1Group
    {
      /// <summary>
      /// A value of Zzzzz.
      /// </summary>
      [JsonPropertyName("zzzzz")]
      public Common Zzzzz
      {
        get => zzzzz ??= new();
        set => zzzzz = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 13;

      private Common zzzzz;
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
    /// Gets a value of ZdelGroupExport1.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroupExport1Group> ZdelGroupExport1 =>
      zdelGroupExport1 ??= new(ZdelGroupExport1Group.Capacity, 0);

    /// <summary>
    /// Gets a value of ZdelGroupExport1 for json serialization.
    /// </summary>
    [JsonPropertyName("zdelGroupExport1")]
    [Computed]
    public IList<ZdelGroupExport1Group> ZdelGroupExport1_Json
    {
      get => zdelGroupExport1;
      set => ZdelGroupExport1.Assign(value);
    }

    private Array<ExportGroup> export1;
    private Array<HiddenCheckGroup> hiddenCheck;
    private Standard standard;
    private Appointment pageKeys;
    private Standard pageNumber;
    private Array<ZdelGroupExport1Group> zdelGroupExport1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Rolling.
    /// </summary>
    [JsonPropertyName("rolling")]
    public Appointment Rolling
    {
      get => rolling ??= new();
      set => rolling = value;
    }

    private Appointment rolling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Appointment Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

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

    private Appointment starting;
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

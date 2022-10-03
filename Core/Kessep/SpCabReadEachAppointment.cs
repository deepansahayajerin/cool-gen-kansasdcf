// Program: SP_CAB_READ_EACH_APPOINTMENT, ID: 371749359, model: 746.
// Short name: SWE01774
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
/// A program: SP_CAB_READ_EACH_APPOINTMENT.
/// </para>
/// <para>
///   Specifically designed for use by AMEN screen, Appointment Management.  
/// This read each populates a repeating group using filters for the service
/// provider userid, case role, case and cse person number, office, and date.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReadEachAppointment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_EACH_APPOINTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadEachAppointment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadEachAppointment.
  /// </summary>
  public SpCabReadEachAppointment(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.DisplayAll.Flag = import.DisplayAll.Flag;
    export.SearchCase.Number = import.SearchCase.Number;
    MoveCsePerson(import.SearchCsePerson, export.SearchCsePerson);
    MoveOffice(import.SearchOffice, export.SearchOffice);
    MoveOfficeServiceProvider(import.SearchOfficeServiceProvider,
      export.SearchOfficeServiceProvider);
    export.SearchServiceProvider.UserId = import.SearchServiceProvider.UserId;
    export.Starting.Date = import.Starting.Date;
    export.PageKeys.Assign(import.PageKeys);
    export.PageNumber.PageNumber = import.PageNumber.PageNumber;

    if (IsEmpty(import.SearchServiceProvider.UserId))
    {
      export.SearchServiceProvider.UserId = "*";
    }
    else
    {
      local.Read.UserId = import.SearchServiceProvider.UserId;
      export.SearchServiceProvider.UserId = import.SearchServiceProvider.UserId;
    }

    export.Export1.Index = -1;

    if (!IsEmpty(local.Read.UserId))
    {
      // To make the read each more efficient, we establish the search Service 
      // Provider sys gen id, if a qualifying Service Provider user id is
      // supplied.
      // Since the Sys Gen id resides on the Appointment table as a foreign key,
      // if we read against the Sys Gen Id rather than User Id, we can avoid
      // doing a DB2 join of the Service Provider table during the read each.
      if (ReadServiceProvider2())
      {
        MoveServiceProvider(entities.GetUserid, local.Read);
      }
      else
      {
        ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

        return;
      }
    }

    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PREV") && import
      .PageNumber.PageNumber == 1)
    {
      foreach(var item in ReadAppointmentOfficeServiceProviderCsePersonCase2())
      {
        if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
        {
          break;
        }

        ReadCaseCaseRoleCsePerson();

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.HiddenCheck.Index = export.Export1.Index;
        export.HiddenCheck.CheckSize();

        if (ReadServiceProvider1())
        {
          MoveServiceProvider(entities.GetUserid,
            export.Export1.Update.GrServiceProvider);
          MoveServiceProvider(entities.GetUserid,
            export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
        }

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

        export.Export1.Update.GrCsePerson.Number = entities.CsePerson.Number;
        MoveCaseRole(entities.CaseRole, export.Export1.Update.GrCaseRole);
        export.Export1.Update.GrOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          export.Export1.Update.GrOfficeServiceProvider);
        export.Export1.Update.GrCase.Number = entities.Case1.Number;
        export.HiddenCheck.Update.GrExportHiddenCheckAppointment.Assign(
          export.Export1.Item.GrAppointment);
        export.HiddenCheck.Update.GrExportHiddenApptAmPmInd.SelectChar =
          export.Export1.Item.GrExportApptAmPmInd.SelectChar;
        export.HiddenCheck.Update.GrExportHiddenCheckCsePerson.Number =
          entities.CsePerson.Number;
        export.HiddenCheck.Update.GrExportHiddenCheckOffice.SystemGeneratedId =
          entities.Office.SystemGeneratedId;
        export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
          RoleCode = entities.OfficeServiceProvider.RoleCode;
        export.HiddenCheck.Update.GrExportHiddenCheckCase.Number =
          entities.Case1.Number;
      }
    }
    else
    {
      foreach(var item in ReadAppointmentOfficeServiceProviderCsePersonCase1())
      {
        // ÚÚÚ SAK 4/29/97
        // The clause in the double parenthesis should be able to handle all 
        // types of situations during scrolling. The part in the first pair
        // should handle all appointments with the same date and time. The
        // second part will handle all succeeding appointments on the same day
        // while the third part would handle all appointments for previous
        // dates.
        if (export.Export1.Index + 1 == Export.ExportGroup.Capacity)
        {
          break;
        }

        ReadCaseCaseRoleCsePerson();

        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.HiddenCheck.Index = export.Export1.Index;
        export.HiddenCheck.CheckSize();

        if (ReadServiceProvider1())
        {
          MoveServiceProvider(entities.GetUserid,
            export.Export1.Update.GrServiceProvider);
          MoveServiceProvider(entities.GetUserid,
            export.HiddenCheck.Update.GrExportHiddenCheckServiceProvider);
        }

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
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          export.Export1.Update.GrOfficeServiceProvider);
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
        export.HiddenCheck.Update.GrExportHiddenCheckOfficeServiceProvider.
          RoleCode = entities.OfficeServiceProvider.RoleCode;
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
      if (!Equal(global.Command, "PREV"))
      {
        MoveAppointment2(entities.Appointment, export.PageKeys);
        export.PageNumber.PageNumber = import.PageNumber.PageNumber + 1;
      }

      if (import.PageNumber.PageNumber > 1)
      {
        export.Standard.ScrollingMessage = "More - +";
      }
      else
      {
        export.Standard.ScrollingMessage = "More   +";
      }
    }
    else if (!export.Export1.IsFull)
    {
      export.PageNumber.PageNumber = import.PageNumber.PageNumber;

      if (import.PageNumber.PageNumber <= 1)
      {
        export.Standard.ScrollingMessage = "More";
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

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private IEnumerable<bool> ReadAppointmentOfficeServiceProviderCsePersonCase1()
  {
    entities.Appointment.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadAppointmentOfficeServiceProviderCsePersonCase1",
      (db, command) =>
      {
        db.SetString(command, "number1", import.SearchCase.Number);
        db.SetNullableString(command, "casNumber", export.SearchCase.Number);
        db.SetString(command, "number2", import.SearchCsePerson.Number);
        db.
          SetNullableString(command, "cspNumber", export.SearchCsePerson.Number);
          
        db.SetString(command, "userId", local.Read.UserId);
        db.SetNullableInt32(command, "spdId", local.Read.SystemGeneratedId);
        db.SetString(
          command, "roleCode", import.SearchOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "systemGeneratedId", import.SearchOffice.SystemGeneratedId);
        db.SetDate(
          command, "appointmentDate1",
          import.Starting.Date.GetValueOrDefault());
        db.SetString(command, "flag", import.DisplayAll.Flag);
        db.SetDate(
          command, "appointmentDate2",
          import.PageKeys.Date.GetValueOrDefault());
        db.SetTimeSpan(command, "appointmentTime", import.PageKeys.Time);
        db.SetDateTime(
          command, "createdTimestamp",
          import.PageKeys.CreatedTimestamp.GetValueOrDefault());
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
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
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
        entities.Office.OffOffice = db.GetNullableInt32(reader, 16);
        entities.Appointment.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadAppointmentOfficeServiceProviderCsePersonCase2()
  {
    entities.Appointment.Populated = false;
    entities.CsePerson.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadAppointmentOfficeServiceProviderCsePersonCase2",
      (db, command) =>
      {
        db.SetString(command, "number1", import.SearchCase.Number);
        db.SetNullableString(command, "casNumber", export.SearchCase.Number);
        db.SetString(command, "number2", import.SearchCsePerson.Number);
        db.
          SetNullableString(command, "cspNumber", export.SearchCsePerson.Number);
          
        db.SetString(command, "userId", local.Read.UserId);
        db.SetNullableInt32(command, "spdId", local.Read.SystemGeneratedId);
        db.SetString(
          command, "roleCode", import.SearchOfficeServiceProvider.RoleCode);
        db.SetInt32(
          command, "systemGeneratedId", import.SearchOffice.SystemGeneratedId);
        db.SetDate(
          command, "appointmentDate", export.Starting.Date.GetValueOrDefault());
          
        db.SetString(command, "flag", import.DisplayAll.Flag);
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
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 6);
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
        entities.Office.OffOffice = db.GetNullableInt32(reader, 16);
        entities.Appointment.Populated = true;
        entities.CsePerson.Populated = true;
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<Appointment>("CroType", entities.Appointment.CroType);

        return true;
      });
  }

  private bool ReadCaseCaseRoleCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;
    entities.Case1.Populated = false;

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
        entities.CaseRole.Populated = true;
        entities.Case1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.GetUserid.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.GetUserid.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.GetUserid.UserId = db.GetString(reader, 1);
        entities.GetUserid.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.GetUserid.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "userId", export.SearchServiceProvider.UserId);
      },
      (db, reader) =>
      {
        entities.GetUserid.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.GetUserid.UserId = db.GetString(reader, 1);
        entities.GetUserid.Populated = true;
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
    /// A value of PageKeys.
    /// </summary>
    [JsonPropertyName("pageKeys")]
    public Appointment PageKeys
    {
      get => pageKeys ??= new();
      set => pageKeys = value;
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
    /// A value of SearchOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchOfficeServiceProvider")]
    public OfficeServiceProvider SearchOfficeServiceProvider
    {
      get => searchOfficeServiceProvider ??= new();
      set => searchOfficeServiceProvider = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
    }

    private Appointment pageKeys;
    private ServiceProvider searchServiceProvider;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private OfficeServiceProvider searchOfficeServiceProvider;
    private Office searchOffice;
    private DateWorkArea starting;
    private Common displayAll;
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
    /// A value of PageNumber.
    /// </summary>
    [JsonPropertyName("pageNumber")]
    public Standard PageNumber
    {
      get => pageNumber ??= new();
      set => pageNumber = value;
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
    /// A value of SearchServiceProvider.
    /// </summary>
    [JsonPropertyName("searchServiceProvider")]
    public ServiceProvider SearchServiceProvider
    {
      get => searchServiceProvider ??= new();
      set => searchServiceProvider = value;
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
    /// A value of SearchOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("searchOfficeServiceProvider")]
    public OfficeServiceProvider SearchOfficeServiceProvider
    {
      get => searchOfficeServiceProvider ??= new();
      set => searchOfficeServiceProvider = value;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public DateWorkArea Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private Standard pageNumber;
    private Appointment pageKeys;
    private Array<ExportGroup> export1;
    private Array<HiddenCheckGroup> hiddenCheck;
    private ServiceProvider searchServiceProvider;
    private Case1 searchCase;
    private CsePerson searchCsePerson;
    private OfficeServiceProvider searchOfficeServiceProvider;
    private Office searchOffice;
    private DateWorkArea starting;
    private Common displayAll;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Overflow.
    /// </summary>
    [JsonPropertyName("overflow")]
    public Common Overflow
    {
      get => overflow ??= new();
      set => overflow = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public ServiceProvider Read
    {
      get => read ??= new();
      set => read = value;
    }

    private Common overflow;
    private DateWorkArea initialized;
    private ServiceProvider read;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of GetUserid.
    /// </summary>
    [JsonPropertyName("getUserid")]
    public ServiceProvider GetUserid
    {
      get => getUserid ??= new();
      set => getUserid = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private ServiceProvider getUserid;
    private Appointment appointment;
    private CsePerson csePerson;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}

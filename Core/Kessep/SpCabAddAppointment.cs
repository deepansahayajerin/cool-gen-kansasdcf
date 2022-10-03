// Program: SP_CAB_ADD_APPOINTMENT, ID: 371749358, model: 746.
// Short name: SWE01777
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_ADD_APPOINTMENT.
/// </summary>
[Serializable]
public partial class SpCabAddAppointment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_ADD_APPOINTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabAddAppointment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabAddAppointment.
  /// </summary>
  public SpCabAddAppointment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Export1.Update.GrAppointment.Assign(import.Import1.GrAppointment);
    export.Export1.Update.GrExportApptAmPmInd.SelectChar =
      import.Import1.GrImportApptAmPmInd.SelectChar;

    switch(AsChar(export.Export1.Item.GrExportApptAmPmInd.SelectChar))
    {
      case 'A':
        local.Start.Time = export.Export1.Item.GrAppointment.Time;

        break;
      case 'P':
        if (export.Export1.Item.GrAppointment.Time > new TimeSpan(12, 0, 0))
        {
          local.Start.Time = export.Export1.Item.GrAppointment.Time;
        }
        else
        {
          local.Start.Time = export.Export1.Item.GrAppointment.Time + new
            TimeSpan(12, 0, 0);
        }

        break;
      default:
        break;
    }

    export.Export1.Update.GrCase.Number = import.Import1.GrCase.Number;
    export.Export1.Update.GrOffice.SystemGeneratedId =
      import.Import1.GrOffice.SystemGeneratedId;
    MoveOfficeServiceProvider(import.Import1.GrOfficeServiceProvider,
      export.Export1.Update.GrOfficeServiceProvider);
    MoveServiceProvider(import.Import1.GrServiceProvider,
      export.Export1.Update.GrServiceProvider);

    if (ReadCase())
    {
      if (ReadCsePerson())
      {
        if (ReadCaseRole())
        {
          if (!Lt(Now().Date, entities.CaseRole.StartDate) && Lt
            (Now().Date, entities.CaseRole.EndDate))
          {
          }
          else
          {
            ExitState = "CO0000_CASE_ROLE_NOT_ACTIVE";
          }

          if (ReadOffice())
          {
            if (ReadServiceProvider())
            {
              if (ReadOfficeServiceProvider())
              {
                try
                {
                  CreateAppointment();
                  export.Pass.CreatedTimestamp =
                    entities.Appointment.CreatedTimestamp;
                  ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
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
              else
              {
                ExitState = "CO0000_SRVC_PRVDR_NOT_W_OFFICE";
              }
            }
            else
            {
              ExitState = "SERVICE_PROVIDER_NF";
            }
          }
          else
          {
            ExitState = "OFFICE_NF";
          }
        }
        else
        {
          ExitState = "CO0000_CASE_ROLE_NF";
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
    else
    {
      ExitState = "CASE_NF";
    }
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

  private void CreateAppointment()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var reasonCode = import.Import1.GrAppointment.ReasonCode;
    var type1 = import.Import1.GrAppointment.Type1;
    var date = import.Import1.GrAppointment.Date;
    var time = local.Start.Time;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.Initialized.Timestamp;
    var spdId = entities.OfficeServiceProvider.SpdGeneratedId;
    var offId = entities.OfficeServiceProvider.OffGeneratedId;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var ospDate = entities.OfficeServiceProvider.EffectiveDate;
    var casNumber = entities.CaseRole.CasNumber;
    var cspNumber = entities.CaseRole.CspNumber;
    var croType = entities.CaseRole.Type1;
    var croId = entities.CaseRole.Identifier;

    CheckValid<Appointment>("CroType", croType);
    entities.Appointment.Populated = false;
    Update("CreateAppointment",
      (db, command) =>
      {
        db.SetString(command, "reasonCode", reasonCode);
        db.SetString(command, "type", type1);
        db.SetString(command, "result", "");
        db.SetDate(command, "appointmentDate", date);
        db.SetTimeSpan(command, "appointmentTime", time);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "spdId", spdId);
        db.SetNullableInt32(command, "offId", offId);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableDate(command, "ospDate", ospDate);
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croId", croId);
      });

    entities.Appointment.ReasonCode = reasonCode;
    entities.Appointment.Type1 = type1;
    entities.Appointment.Result = "";
    entities.Appointment.Date = date;
    entities.Appointment.Time = time;
    entities.Appointment.CreatedBy = createdBy;
    entities.Appointment.CreatedTimestamp = createdTimestamp;
    entities.Appointment.LastUpdatedBy = "";
    entities.Appointment.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Appointment.SpdId = spdId;
    entities.Appointment.OffId = offId;
    entities.Appointment.OspRoleCode = ospRoleCode;
    entities.Appointment.OspDate = ospDate;
    entities.Appointment.AppTstamp = null;
    entities.Appointment.CasNumber = casNumber;
    entities.Appointment.CspNumber = cspNumber;
    entities.Appointment.CroType = croType;
    entities.Appointment.CroId = croId;
    entities.Appointment.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Import1.GrCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Import1.GrCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", import.Import1.GrOffice.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetDate(
          command, "effectiveDate",
          export.Export1.Item.GrOfficeServiceProvider.EffectiveDate.
            GetValueOrDefault());
        db.SetString(
          command, "roleCode", import.Import1.GrOfficeServiceProvider.RoleCode);
          
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

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.
          SetString(command, "userId", import.Import1.GrServiceProvider.UserId);
          
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
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

    private ImportGroup import1;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 199;

      private OfficeServiceProvider grOfficeServiceProvider;
      private Case1 grCase;
      private CsePerson grCsePerson;
      private Appointment grAppointment;
      private Common grExportApptAmPmInd;
      private Office grOffice;
      private ServiceProvider grServiceProvider;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public Appointment Pass
    {
      get => pass ??= new();
      set => pass = value;
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

    private Appointment pass;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    private DateWorkArea initialized;
    private DateWorkArea start;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Appointment appointment;
  }
#endregion
}

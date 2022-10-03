// Program: SP_CAB_READ_OFF_SRV_PRV_ALERT, ID: 371748103, model: 746.
// Short name: SWE01829
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_READ_OFF_SRV_PRV_ALERT.
/// </summary>
[Serializable]
public partial class SpCabReadOffSrvPrvAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_OFF_SRV_PRV_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadOffSrvPrvAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadOffSrvPrvAlert.
  /// </summary>
  public SpCabReadOffSrvPrvAlert(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadOfficeServiceProviderAlert())
    {
      export.OfficeServiceProviderAlert.Assign(
        entities.OfficeServiceProviderAlert);

      if (ReadOfficeServiceProviderServiceProviderOffice())
      {
        export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
        MoveOfficeServiceProvider(entities.OfficeServiceProvider,
          export.OfficeServiceProvider);
        MoveServiceProvider(entities.ServiceProvider, export.ServiceProvider);
        export.Sender.UserId = export.OfficeServiceProviderAlert.CreatedBy;

        if (ReadInfrastructure())
        {
          export.Infrastructure.Assign(entities.Infrastructure);
          export.WorkArea.Text80 = entities.Infrastructure.Detail ?? Spaces(80);

          if (Equal(entities.Infrastructure.BusinessObjectCd, "LEA"))
          {
            // *** October 26, 1999  David Lowry
            // Added this set statement and changed to read of legal action to 
            // avoid tablespace scans.
            local.LegalAction.Identifier =
              (int)entities.Infrastructure.DenormNumeric12.GetValueOrDefault();

            if (ReadLegalAction())
            {
              MoveLegalAction(entities.LegalAction, export.LegalAction);
            }
          }
        }
      }
    }
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
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

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(
      entities.OfficeServiceProviderAlert.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.InfId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 1);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 2);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 3);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 5);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 6);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 7);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    entities.OfficeServiceProviderAlert.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 1);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 2);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 3);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 4);
        entities.OfficeServiceProviderAlert.SituationIdentifier =
          db.GetString(reader, 5);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 6);
        entities.OfficeServiceProviderAlert.CreatedBy = db.GetString(reader, 7);
        entities.OfficeServiceProviderAlert.InfId =
          db.GetNullableInt32(reader, 8);
        entities.OfficeServiceProviderAlert.SpdId =
          db.GetNullableInt32(reader, 9);
        entities.OfficeServiceProviderAlert.OffId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeServiceProviderAlert.OspCode =
          db.GetNullableString(reader, 11);
        entities.OfficeServiceProviderAlert.OspDate =
          db.GetNullableDate(reader, 12);
        entities.OfficeServiceProviderAlert.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(
      entities.OfficeServiceProviderAlert.Populated);
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "spdId",
          entities.OfficeServiceProviderAlert.SpdId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offId",
          entities.OfficeServiceProviderAlert.OffId.GetValueOrDefault());
        db.SetNullableString(
          command, "ospCode", entities.OfficeServiceProviderAlert.OspCode ?? ""
          );
        db.SetNullableDate(
          command, "ospDate",
          entities.OfficeServiceProviderAlert.OspDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 6);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 7);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private Infrastructure infrastructure;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Sender.
    /// </summary>
    [JsonPropertyName("sender")]
    public ServiceProvider Sender
    {
      get => sender ??= new();
      set => sender = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private ServiceProvider sender;
    private WorkArea workArea;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    private LegalAction legalAction;
    private Infrastructure infrastructure;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
  }
#endregion
}

// Program: SP_CAB_DET_OSP_ASSGND_TO_LACT, ID: 371986298, model: 746.
// Short name: SWE01818
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DET_OSP_ASSGND_TO_LACT.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This common action block returns the office service provider,  office and 
/// the service provider assigned to the legal action.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDetOspAssgndToLact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DET_OSP_ASSGND_TO_LACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDetOspAssgndToLact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDetOspAssgndToLact.
  /// </summary>
  public SpCabDetOspAssgndToLact(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *********************************************
    // Date	 By	Change Req#	Description
    // 12/11/96 Govind			Initial code.
    // *********************************************
    // ---------------------------------------------
    // This common action block returns the office service provider, service 
    // provider and the office assigned to the import legal action.
    // ---------------------------------------------
    // --- Default as-of-date to Current Date if not specified.
    if (!Lt(local.InitialisedToZeros.Date, import.AsOfDate.Date))
    {
      local.AsOfDate.Date = Now().Date;
    }
    else
    {
      local.AsOfDate.Date = import.AsOfDate.Date;
    }

    // --- Default assignment reason to 'RSP' if not specified.
    if (IsEmpty(import.Search.ReasonCode))
    {
      local.Search.ReasonCode = "RSP";
    }
    else
    {
      local.Search.ReasonCode = import.Search.ReasonCode;
    }

    if (!ReadLegalAction())
    {
      ExitState = "ZD_LEGAL_ACTION_NF_3";

      return;
    }

    if (ReadLegalActionAssigmentOfficeServiceProvider())
    {
      export.LegalActionAssigment.Assign(entities.LegalActionAssigment);
      export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
      export.ServiceProvider.Assign(entities.ServiceProvider);
      export.Office.Assign(entities.Office);

      return;
    }

    ExitState = "LEGAL_ACTION_ASSIGNMENT_NF";
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionAssigmentOfficeServiceProvider()
  {
    entities.LegalActionAssigment.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadLegalActionAssigmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDate(
          command, "effectiveDt", local.AsOfDate.Date.GetValueOrDefault());
        db.SetString(command, "reasonCode", local.Search.ReasonCode);
      },
      (db, reader) =>
      {
        entities.LegalActionAssigment.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.LegalActionAssigment.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalActionAssigment.OspRoleCode =
          db.GetNullableString(reader, 2);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.LegalActionAssigment.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 3);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 3);
        entities.LegalActionAssigment.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.LegalActionAssigment.EffectiveDate = db.GetDate(reader, 5);
        entities.LegalActionAssigment.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.LegalActionAssigment.ReasonCode = db.GetString(reader, 7);
        entities.LegalActionAssigment.OverrideInd = db.GetString(reader, 8);
        entities.LegalActionAssigment.CreatedBy = db.GetString(reader, 9);
        entities.LegalActionAssigment.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.LegalActionAssigment.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.LegalActionAssigment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.OfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 13);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 14);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 16);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 17);
        entities.OfficeServiceProvider.CreatedBy = db.GetString(reader, 18);
        entities.OfficeServiceProvider.CreatedTimestamp =
          db.GetDateTime(reader, 19);
        entities.OfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 20);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 21);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 22);
        entities.OfficeServiceProvider.LocalContactCodeForIrs =
          db.GetNullableInt32(reader, 23);
        entities.ServiceProvider.CreatedBy = db.GetString(reader, 24);
        entities.ServiceProvider.CreatedTimestamp = db.GetDateTime(reader, 25);
        entities.ServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 26);
        entities.ServiceProvider.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 27);
        entities.ServiceProvider.UserId = db.GetString(reader, 28);
        entities.ServiceProvider.LastName = db.GetString(reader, 29);
        entities.ServiceProvider.FirstName = db.GetString(reader, 30);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 31);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 32);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 33);
        entities.Office.TypeCode = db.GetString(reader, 34);
        entities.Office.Name = db.GetString(reader, 35);
        entities.Office.LastUpdatedBy = db.GetNullableString(reader, 36);
        entities.Office.LastUpdatdTstamp = db.GetNullableDateTime(reader, 37);
        entities.Office.CreatedBy = db.GetString(reader, 38);
        entities.Office.CreatedTimestamp = db.GetDateTime(reader, 39);
        entities.Office.EffectiveDate = db.GetDate(reader, 40);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 41);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 42);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 43);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 44);
        entities.LegalActionAssigment.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Office.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalActionAssigment Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
    }

    private LegalAction legalAction;
    private LegalActionAssigment search;
    private DateWorkArea asOfDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public LegalActionAssigment Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private LegalActionAssigment search;
    private DateWorkArea asOfDate;
    private DateWorkArea initialisedToZeros;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private LegalAction legalAction;
    private LegalActionAssigment legalActionAssigment;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}

// Program: SP_CAB_DET_OSP_ASSGND_TO_CSECASE, ID: 371739735, model: 746.
// Short name: SWE01791
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DET_OSP_ASSGND_TO_CSECASE.
/// </para>
/// <para>
/// RESP: SERVPLAN
/// This common block returns the Office SErvice Provider, Office and the 
/// Service Provider assigned to the import cse case.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDetOspAssgndToCsecase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DET_OSP_ASSGND_TO_CSECASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDetOspAssgndToCsecase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDetOspAssgndToCsecase.
  /// </summary>
  public SpCabDetOspAssgndToCsecase(IContext context, Import import,
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
    // *********************************************
    // Date	 By	Change Req#	Description
    // 12/11/96 Govind			Initial Code
    // *********************************************
    // ---------------------------------------------
    // This common action block returns the office service provider, service 
    // provider and the office assigned to the import cse case.
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
    if (IsEmpty(import.CaseAssignment.ReasonCode))
    {
      local.Search.ReasonCode = "RSP";
    }
    else
    {
      local.Search.ReasonCode = import.CaseAssignment.ReasonCode;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCaseAssignmentOfficeServiceProviderServiceProvider())
    {
      export.CaseAssignment.Assign(entities.CaseAssignment);
      export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);
      export.ServiceProvider.Assign(entities.ServiceProvider);
      export.Office.Assign(entities.Office);

      return;
    }

    ExitState = "CASE_ASSIGNMENT_NF";
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeServiceProviderServiceProvider()
  {
    entities.Office.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.CaseAssignment.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProviderServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetString(command, "reasonCode", local.Search.ReasonCode);
        db.SetDate(
          command, "effectiveDate", local.AsOfDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedBy = db.GetString(reader, 4);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.CaseAssignment.LastUpdatedBy = db.GetNullableString(reader, 6);
        entities.CaseAssignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 7);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 8);
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 8);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 8);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 9);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 9);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.CaseAssignment.OspCode = db.GetString(reader, 10);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 10);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 11);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 11);
        entities.CaseAssignment.CasNo = db.GetString(reader, 12);
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
        entities.Office.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.CaseAssignment.Populated = true;
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
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private DateWorkArea asOfDate;
    private CaseAssignment caseAssignment;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
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
    public CaseAssignment Search
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

    private CaseAssignment search;
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Case1 case1;
    private CaseAssignment caseAssignment;
  }
#endregion
}

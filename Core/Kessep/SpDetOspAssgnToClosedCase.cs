// Program: SP_DET_OSP_ASSGN_TO_CLOSED_CASE, ID: 372646346, model: 746.
// Short name: SWE02107
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DET_OSP_ASSGN_TO_CLOSED_CASE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpDetOspAssgnToClosedCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DET_OSP_ASSGN_TO_CLOSED_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDetOspAssgnToClosedCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDetOspAssgnToClosedCase.
  /// </summary>
  public SpDetOspAssgnToClosedCase(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      Req #    	Description
    // 09/17/97 R. Grey	IDCR 357 	Initial Code
    // ---------------------------------------------
    local.Search.ReasonCode = "RSP";

    if (ReadCase())
    {
      local.CaseClosedDate.Date = entities.Closed.StatusDate;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCaseAssignment())
    {
      export.CaseAssignment.Assign(entities.Discontinued);

      if (ReadOfficeServiceProvider())
      {
        export.OfficeServiceProvider.Assign(entities.OfficeServiceProvider);

        if (ReadOffice())
        {
          export.Office.Assign(entities.Office);
        }
        else
        {
          ExitState = "FN0000_OFFICE_NF";

          return;
        }

        if (ReadServiceProvider())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
        }
        else
        {
          ExitState = "SERVICE_PROVIDER_NF";
        }
      }
      else
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";
      }
    }
    else
    {
      ExitState = "CASE_ASSIGNMENT_NF";
    }
  }

  private bool ReadCase()
  {
    entities.Closed.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Closed.Number);
      },
      (db, reader) =>
      {
        entities.Closed.Number = db.GetString(reader, 0);
        entities.Closed.StatusDate = db.GetNullableDate(reader, 1);
        entities.Closed.Populated = true;
      });
  }

  private bool ReadCaseAssignment()
  {
    entities.Discontinued.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Closed.Number);
        db.SetDate(
          command, "date", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Discontinued.ReasonCode = db.GetString(reader, 0);
        entities.Discontinued.EffectiveDate = db.GetDate(reader, 1);
        entities.Discontinued.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.Discontinued.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.Discontinued.SpdId = db.GetInt32(reader, 4);
        entities.Discontinued.OffId = db.GetInt32(reader, 5);
        entities.Discontinued.OspCode = db.GetString(reader, 6);
        entities.Discontinued.OspDate = db.GetDate(reader, 7);
        entities.Discontinued.CasNo = db.GetString(reader, 8);
        entities.Discontinued.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.EffectiveDate = db.GetDate(reader, 3);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.Discontinued.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.Discontinued.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.Discontinued.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.Discontinued.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.Discontinued.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId",
          entities.OfficeServiceProvider.SpdGeneratedId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
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
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    private Case1 closed;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
    }

    /// <summary>
    /// A value of InitializedDate.
    /// </summary>
    [JsonPropertyName("initializedDate")]
    public DateWorkArea InitializedDate
    {
      get => initializedDate ??= new();
      set => initializedDate = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CaseAssignment Search
    {
      get => search ??= new();
      set => search = value;
    }

    private DateWorkArea caseClosedDate;
    private DateWorkArea initializedDate;
    private CaseAssignment search;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
    }

    /// <summary>
    /// A value of Discontinued.
    /// </summary>
    [JsonPropertyName("discontinued")]
    public CaseAssignment Discontinued
    {
      get => discontinued ??= new();
      set => discontinued = value;
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

    private Case1 closed;
    private CaseAssignment discontinued;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Office office;
  }
#endregion
}

// Program: SI_B277_FIND_OFFICE_AND_RSP, ID: 372746268, model: 746.
// Short name: SWE02582
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B277_FIND_OFFICE_AND_RSP.
/// </summary>
[Serializable]
public partial class SiB277FindOfficeAndRsp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B277_FIND_OFFICE_AND_RSP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB277FindOfficeAndRsp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB277FindOfficeAndRsp.
  /// </summary>
  public SiB277FindOfficeAndRsp(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *                   Maintenance Log
    // 
    // *
    // ***********************************************************************
    // *    DATE       NAME      REQUEST      DESCRIPTION                    *
    // * ----------  ---------  
    // ---------
    // --------------------------------
    // *
    // * 05/15/2000  Ed Lyman   PR# 90538   Report is displaying an inactive *
    // *
    // 
    // service provider.                *
    // * 06/22/2000  G Vandy    PR# 91162   Report is displaying an inactive *
    // *
    // 
    // case.                            *
    // ***********************************************************************
    local.CaseAssignment.ReasonCode = "RSP";
    MoveOffice(import.Office, export.Office);

    if (ReadCase())
    {
      if (AsChar(entities.Case1.Status) == 'C')
      {
        ExitState = "SC0000_CASE_IS_NOT_OPEN";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCaseAssignment())
    {
      local.CaseFound.Flag = "Y";

      if (!ReadOfficeServiceProvider())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }

      if (ReadOffice())
      {
        MoveOffice(entities.Office, export.Office);
      }
      else
      {
        ExitState = "OFFICE_NF";

        return;
      }

      if (ReadServiceProvider())
      {
        export.CaseWorker.FirstName = entities.ServiceProvider.FirstName;
        export.CaseWorker.MiddleInitial =
          entities.ServiceProvider.MiddleInitial;
        export.CaseWorker.LastName = entities.ServiceProvider.LastName;
      }
      else
      {
        ExitState = "SERVICE_PROVIDER_NF";

        return;
      }
    }

    if (AsChar(local.CaseFound.Flag) != 'Y')
    {
      ExitState = "CASE_ASSIGNMENT_NF";
    }
  }

  private static void MoveOffice(Office source, Office target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.Name = source.Name;
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
        entities.Case1.Status = db.GetNullableString(reader, 1);
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
        db.SetString(command, "reasonCode", local.CaseAssignment.ReasonCode);
        db.SetDate(
          command, "effectiveDate",
          import.ProcessDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.OverrideInd = db.GetString(reader, 1);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 2);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 6);
        entities.CaseAssignment.OspCode = db.GetString(reader, 7);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 8);
        entities.CaseAssignment.CasNo = db.GetString(reader, 9);
        entities.CaseAssignment.Populated = true;
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
        entities.Office.Name = db.GetString(reader, 1);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 2);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
        db.SetNullableDate(
          command, "discontinueDate",
          import.ProcessDate.Date.GetValueOrDefault());
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    private Case1 case1;
    private Office office;
    private DateWorkArea processDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseWorker.
    /// </summary>
    [JsonPropertyName("caseWorker")]
    public CsePersonsWorkSet CaseWorker
    {
      get => caseWorker ??= new();
      set => caseWorker = value;
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

    private CsePersonsWorkSet caseWorker;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of CaseWorker.
    /// </summary>
    [JsonPropertyName("caseWorker")]
    public Office CaseWorker
    {
      get => caseWorker ??= new();
      set => caseWorker = value;
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

    private Common caseFound;
    private Office caseWorker;
    private CaseAssignment caseAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private Office office;
  }
#endregion
}

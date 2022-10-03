// Program: SC_SECURITY_BASED_ON_CASE, ID: 371456900, model: 746.
// Short name: SWE01664
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SC_SECURITY_BASED_ON_CASE.
/// </summary>
[Serializable]
public partial class ScSecurityBasedOnCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_SECURITY_BASED_ON_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScSecurityBasedOnCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScSecurityBasedOnCase.
  /// </summary>
  public ScSecurityBasedOnCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************************
    // PR#	Date		Developer	Description
    // ***********************************************************************************
    // 111258	02/09/01	Kalpesh Doshi	Change security for Closed cases.  A worker
    // will be authorized to take that action if he/she works in the office
    // where case was 'most recently' assigned.
    // 125827	08/24/01	E.Shirk		Modified read against case assignment for closed
    // cases to discontinue qualification on the office service provider
    // effective and discontinue dates.  This was causing the read for case to
    // be not found when that provider was no longer assigned to the office.
    // ***********************************************************************************
    export.Auth.Flag = "N";
    local.CurrentDateWorkArea.Date = Now().Date;

    if (!ReadServiceProvider())
    {
      ExitState = "SERVICE_PROVIDER_NF";

      return;
    }

    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(entities.Case2.Status) == 'C')
    {
      // ===============================================
      // This is a Closed case and case assignments are ended.
      // Read most recent assignment, ignoring future-dated entries.
      // ===============================================
      if (ReadCaseAssignmentOffice())
      {
        // =======================================================
        // SP could be active on multiple offices simultaneously.
        // Check each office for authorization.
        // ======================================================
        foreach(var item in ReadOffice())
        {
          // =======================================================
          // SP is authorized if he/she is active on most recent
          // case_assignment office.
          // ======================================================
          if (entities.Sp.SystemGeneratedId == entities
            .Office.SystemGeneratedId)
          {
            export.Auth.Flag = "Y";

            return;
          }
        }

        // ===============================================
        // We will only get here if SP is not authorized.
        // ===============================================
      }
    }
    else if (ReadCaseAssignmentOfficeServiceProviderOffice())
    {
      export.Auth.Flag = "Y";
    }
  }

  private bool ReadCase()
  {
    entities.Case2.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case2.Number = db.GetString(reader, 0);
        entities.Case2.Status = db.GetNullableString(reader, 1);
        entities.Case2.OfficeIdentifier = db.GetNullableInt32(reader, 2);
        entities.Case2.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOffice()
  {
    entities.CaseAssignment.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCaseAssignmentOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case2.Number);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.CaseAssignment.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadCaseAssignmentOfficeServiceProviderOffice()
  {
    entities.CaseAssignment.Populated = false;
    entities.Case1.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadCaseAssignmentOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case2.Number);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.Case1.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.Case1.OffGeneratedId = db.GetInt32(reader, 5);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.Case1.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.Case1.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.Case1.DiscontinueDate = db.GetNullableDate(reader, 9);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 10);
        entities.CaseAssignment.Populated = true;
        entities.Case1.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOffice()
  {
    entities.Sp.Populated = false;

    return ReadEach("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          local.CurrentDateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Sp.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Sp.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Sp.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
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
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Auth.
    /// </summary>
    [JsonPropertyName("auth")]
    public Common Auth
    {
      get => auth ??= new();
      set => auth = value;
    }

    private Common auth;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentDateWorkArea.
    /// </summary>
    [JsonPropertyName("currentDateWorkArea")]
    public DateWorkArea CurrentDateWorkArea
    {
      get => currentDateWorkArea ??= new();
      set => currentDateWorkArea = value;
    }

    private DateWorkArea currentDateWorkArea;
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
    public OfficeServiceProvider Case1
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
    /// A value of Sp.
    /// </summary>
    [JsonPropertyName("sp")]
    public Office Sp
    {
      get => sp ??= new();
      set => sp = value;
    }

    /// <summary>
    /// A value of General.
    /// </summary>
    [JsonPropertyName("general")]
    public OfficeServiceProvider General
    {
      get => general ??= new();
      set => general = value;
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
    /// A value of Case2.
    /// </summary>
    [JsonPropertyName("case2")]
    public Case1 Case2
    {
      get => case2 ??= new();
      set => case2 = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private CaseAssignment caseAssignment;
    private OfficeServiceProvider case1;
    private Office office;
    private Office sp;
    private OfficeServiceProvider general;
    private ServiceProvider serviceProvider;
    private Case1 case2;
  }
#endregion
}

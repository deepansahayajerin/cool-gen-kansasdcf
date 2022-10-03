// Program: CO_CAB_IS_USER_ASSIGNED_TO_CASE, ID: 372355022, model: 746.
// Short name: SWE00518
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CO_CAB_IS_USER_ASSIGNED_TO_CASE.
/// </summary>
[Serializable]
public partial class CoCabIsUserAssignedToCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CO_CAB_IS_USER_ASSIGNED_TO_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CoCabIsUserAssignedToCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CoCabIsUserAssignedToCase.
  /// </summary>
  public CoCabIsUserAssignedToCase(IContext context, Import import,
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
    // ******** MAINTENANCE LOG *********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // E. Lyman	02/02/99	Initial code.
    // Sree Veettil   02/08/2000         PR#87246
    //                   The supervisor should have access to FPLS info and 1099
    // info. Check if the case worker is is assigned to this case, if yes give
    // the case worked the access to view the data, if no check whether the
    // person is the supervisor of the caseworker assigned to this case. If yes
    // give him authrity to view the data.
    // Sree Veettil : 28-03-2000
    //      The changes have been made so that the action block can be used by
    // . Either the case number or the person number or both
    // can be passed to this CAB. The CAB should be able to take care of all the
    // conditions.
    // ******** END MAINTENANCE LOG *****************
    export.OnTheCase.Flag = "N";
    export.Supervisor.Flag = "N";
    local.ProgramProcessingInfo.ProcessDate = Now().Date;
    local.ServiceProvider.UserId = global.UserId;

    if (IsEmpty(import.Case1.Number) && !IsEmpty(import.CsePerson.Number))
    {
      if (ReadCsePerson())
      {
        foreach(var item in ReadCase2())
        {
          if (ReadServiceProvider1())
          {
            export.OnTheCase.Flag = "Y";

            return;
          }
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }

      // ******** MAINTENANCE LOG *********************
      // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
      // Sree Veettil              02/08/2000
      // Check whether user is supervisor or not, if yes give authority to view 
      // data.
      // ******** END MAINTENANCE LOG *****************
      foreach(var item in ReadCase2())
      {
        foreach(var item1 in ReadCaseAssignmentOfficeServiceProvider())
        {
          foreach(var item2 in ReadOfficeServiceProvRelationship())
          {
            if (ReadOfficeServiceProvider())
            {
              if (ReadServiceProvider2())
              {
                export.OnTheCase.Flag = "Y";
                export.Supervisor.Flag = "Y";

                return;
              }
            }
          }
        }
      }
    }

    if (!IsEmpty(import.Case1.Number))
    {
      if (ReadCase1())
      {
        if (ReadServiceProvider1())
        {
          export.OnTheCase.Flag = "Y";
        }
        else
        {
          foreach(var item in ReadCaseAssignmentOfficeServiceProvider())
          {
            foreach(var item1 in ReadOfficeServiceProvRelationship())
            {
              if (ReadOfficeServiceProvider())
              {
                if (ReadServiceProvider2())
                {
                  export.OnTheCase.Flag = "Y";

                  return;
                }
              }
            }
          }
        }
      }
      else
      {
        ExitState = "CASE_NF";
      }
    }
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
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

  private IEnumerable<bool> ReadCase2()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignmentOfficeServiceProvider()
  {
    entities.CollectionOfficer.Populated = false;
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CollectionOfficer.SpdGeneratedId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CollectionOfficer.OffGeneratedId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CollectionOfficer.RoleCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CollectionOfficer.EffectiveDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CollectionOfficer.DiscontinueDate =
          db.GetNullableDate(reader, 9);
        entities.CollectionOfficer.Populated = true;
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOfficeServiceProvRelationship()
  {
    System.Diagnostics.Debug.Assert(entities.CollectionOfficer.Populated);
    entities.OfficeServiceProvRelationship.Populated = false;

    return ReadEach("ReadOfficeServiceProvRelationship",
      (db, command) =>
      {
        db.
          SetString(command, "ospRoleCode", entities.CollectionOfficer.RoleCode);
          
        db.SetDate(
          command, "ospEffectiveDate",
          entities.CollectionOfficer.EffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId", entities.CollectionOfficer.OffGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId", entities.CollectionOfficer.SpdGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvRelationship.OspEffectiveDate =
          db.GetDate(reader, 0);
        entities.OfficeServiceProvRelationship.OspRoleCode =
          db.GetString(reader, 1);
        entities.OfficeServiceProvRelationship.OffGeneratedId =
          db.GetInt32(reader, 2);
        entities.OfficeServiceProvRelationship.SpdGeneratedId =
          db.GetInt32(reader, 3);
        entities.OfficeServiceProvRelationship.OspREffectiveDt =
          db.GetDate(reader, 4);
        entities.OfficeServiceProvRelationship.OspRRoleCode =
          db.GetString(reader, 5);
        entities.OfficeServiceProvRelationship.OffRGeneratedId =
          db.GetInt32(reader, 6);
        entities.OfficeServiceProvRelationship.SpdRGeneratedId =
          db.GetInt32(reader, 7);
        entities.OfficeServiceProvRelationship.ReasonCode =
          db.GetString(reader, 8);
        entities.OfficeServiceProvRelationship.CreatedBy =
          db.GetString(reader, 9);
        entities.OfficeServiceProvRelationship.CreatedDtstamp =
          db.GetDateTime(reader, 10);
        entities.OfficeServiceProvRelationship.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(
      entities.OfficeServiceProvRelationship.Populated);
    entities.Supervisor.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "roleCode",
          entities.OfficeServiceProvRelationship.OspRRoleCode);
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeServiceProvRelationship.OspREffectiveDt.
            GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvRelationship.OffRGeneratedId);
        db.SetInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvRelationship.SpdRGeneratedId);
      },
      (db, reader) =>
      {
        entities.Supervisor.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.Supervisor.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Supervisor.RoleCode = db.GetString(reader, 2);
        entities.Supervisor.EffectiveDate = db.GetDate(reader, 3);
        entities.Supervisor.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Supervisor.Populated = true;
      });
  }

  private bool ReadServiceProvider1()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(command, "userId", local.ServiceProvider.UserId);
        db.SetDate(
          command, "effectiveDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.Supervisor.Populated);
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetInt32(
          command, "servicePrvderId", entities.Supervisor.SpdGeneratedId);
        db.SetString(command, "userId", local.ServiceProvider.UserId);
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

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Case1 case1;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    private Common supervisor;
    private Common onTheCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    private ProgramProcessingInfo programProcessingInfo;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CaseRole Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvRelationship.
    /// </summary>
    [JsonPropertyName("officeServiceProvRelationship")]
    public OfficeServiceProvRelationship OfficeServiceProvRelationship
    {
      get => officeServiceProvRelationship ??= new();
      set => officeServiceProvRelationship = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public OfficeServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of CollectionOfficer.
    /// </summary>
    [JsonPropertyName("collectionOfficer")]
    public OfficeServiceProvider CollectionOfficer
    {
      get => collectionOfficer ??= new();
      set => collectionOfficer = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private CaseRole existing;
    private OfficeServiceProvRelationship officeServiceProvRelationship;
    private OfficeServiceProvider supervisor;
    private OfficeServiceProvider collectionOfficer;
    private CsePerson csePerson;
    private CaseRole absentParent;
    private CaseAssignment caseAssignment;
    private Case1 case1;
    private ServiceProvider serviceProvider;
  }
#endregion
}

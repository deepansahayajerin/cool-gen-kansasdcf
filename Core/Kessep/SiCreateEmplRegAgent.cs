// Program: SI_CREATE_EMPL_REG_AGENT, ID: 371767026, model: 746.
// Short name: SWE01126
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_EMPL_REG_AGENT.
/// </para>
/// <para>
/// This AB links a  registered agent to an employer
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateEmplRegAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_EMPL_REG_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateEmplRegAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateEmplRegAgent.
  /// </summary>
  public SiCreateEmplRegAgent(IContext context, Import import, Export export):
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
    //     M A I N T E N A N C E    L O G
    //   Date   Developer      Description
    // 10-21-95 H. Sharland    Initial Development
    // 04/29/97 SHERAZ MALIK   CHANGE CURRENT_DATE
    // 10/30/98 W. Campbell    A block of code was removed
    //                         which was used to determine
    //                         whether the employer had a
    //                         current registered agent.
    //                         If so, it was end dating that
    //                         one before creating a new one.
    // 10/30/98 W. Campbell    Code added to prevent
    //                         multiple EMPL_REG_AGENTs
    //                         from being created for a given
    //                         employer.
    // ------------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadEmployer())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    if (!ReadRegisteredAgent())
    {
      ExitState = "REGISTERED_AGENT_NF";

      return;
    }

    if (ReadEmployerRegisteredAgent())
    {
      ExitState = "EMPLOYER_REG_AGENT_FOUND";

      return;
    }
    else
    {
      // ------------------------------------------------------------
      // 10/30/98 W. Campbell -  A block of code was removed
      // which was used to determine whether the employer
      // had a current registered agent.  If so, it was end
      // dating that one before creating a new one.
      // -----------------------------------------------------------
    }

    // ------------------------------------------------
    // 10/30/98 W. Campbell  Code added to prevent
    // multiple EMPL_REG_AGENTs from being created
    // for a given employer.
    // ------------------------------------------------
    if (ReadEmployerRegisteredAgentRegisteredAgent())
    {
      ExitState = "ANOTHER_EMPLOYER_REG_AGENT_AE";
    }

    // ------------------------------------------------
    // 10/30/98 W. Campbell  End of code added to
    // prevent multiple EMPL_REG_AGENTs from
    // being created for a given employer.
    // ------------------------------------------------
    local.ControlTable.Identifier = "EMPLOYER REG AGENT";
    UseAccessControlTable();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EmployerRegisteredAgent.Identifier =
      NumberToString(local.ControlTable.LastUsedNumber, 9);

    try
    {
      CreateEmployerRegisteredAgent();
      export.EmployerRegisteredAgent.Identifier =
        entities.EmployerRegisteredAgent.Identifier;
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "EMPLOYER_REGISTERED_AGENT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "EMPLOYER_REGISTERED_AGENT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateEmployerRegisteredAgent()
  {
    var identifier = local.EmployerRegisteredAgent.Identifier;
    var effectiveDate = local.Current.Date;
    var endDate = UseCabSetMaximumDiscontinueDate();
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var raaId = entities.RegisteredAgent.Identifier;
    var empId = entities.Employer.Identifier;

    entities.EmployerRegisteredAgent.Populated = false;
    Update("CreateEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetString(command, "identifier", identifier);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDateTime(command, "lastUpdatedTmst", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetString(command, "raaId", raaId);
        db.SetInt32(command, "empId", empId);
        db.SetNullableString(command, "note", "");
      });

    entities.EmployerRegisteredAgent.Identifier = identifier;
    entities.EmployerRegisteredAgent.EffectiveDate = effectiveDate;
    entities.EmployerRegisteredAgent.EndDate = endDate;
    entities.EmployerRegisteredAgent.CreatedTimestamp = createdTimestamp;
    entities.EmployerRegisteredAgent.UpdatedTimestamp = null;
    entities.EmployerRegisteredAgent.CreatedBy = createdBy;
    entities.EmployerRegisteredAgent.UpdatedBy = "";
    entities.EmployerRegisteredAgent.RaaId = raaId;
    entities.EmployerRegisteredAgent.EmpId = empId;
    entities.EmployerRegisteredAgent.Populated = true;
  }

  private bool ReadEmployer()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerRegisteredAgent()
  {
    entities.EmployerRegisteredAgent.Populated = false;

    return Read("ReadEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetString(command, "raaId", entities.RegisteredAgent.Identifier);
        db.SetInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.EmployerRegisteredAgent.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.EmployerRegisteredAgent.EndDate =
          db.GetNullableDate(reader, 2);
        entities.EmployerRegisteredAgent.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.EmployerRegisteredAgent.UpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.EmployerRegisteredAgent.CreatedBy = db.GetString(reader, 5);
        entities.EmployerRegisteredAgent.UpdatedBy = db.GetString(reader, 6);
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 7);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 8);
        entities.EmployerRegisteredAgent.Populated = true;
      });
  }

  private bool ReadEmployerRegisteredAgentRegisteredAgent()
  {
    entities.EmployerRegisteredAgent.Populated = false;
    entities.Other.Populated = false;

    return Read("ReadEmployerRegisteredAgentRegisteredAgent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerRegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.EmployerRegisteredAgent.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.EmployerRegisteredAgent.EndDate =
          db.GetNullableDate(reader, 2);
        entities.EmployerRegisteredAgent.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.EmployerRegisteredAgent.UpdatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.EmployerRegisteredAgent.CreatedBy = db.GetString(reader, 5);
        entities.EmployerRegisteredAgent.UpdatedBy = db.GetString(reader, 6);
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 7);
        entities.Other.Identifier = db.GetString(reader, 7);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 8);
        entities.EmployerRegisteredAgent.Populated = true;
        entities.Other.Populated = true;
      });
  }

  private bool ReadRegisteredAgent()
  {
    entities.RegisteredAgent.Populated = false;

    return Read("ReadRegisteredAgent",
      (db, command) =>
      {
        db.SetString(command, "identifier", import.RegisteredAgent.Identifier);
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgent.Populated = true;
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
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private RegisteredAgent registeredAgent;
    private Employer employer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    private EmployerRegisteredAgent employerRegisteredAgent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    private DateWorkArea current;
    private ControlTable controlTable;
    private EmployerRegisteredAgent employerRegisteredAgent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    /// <summary>
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public RegisteredAgent Other
    {
      get => other ??= new();
      set => other = value;
    }

    private EmployerRegisteredAgent employerRegisteredAgent;
    private RegisteredAgent registeredAgent;
    private Employer employer;
    private RegisteredAgent other;
  }
#endregion
}

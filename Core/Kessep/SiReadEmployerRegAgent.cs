// Program: SI_READ_EMPLOYER_REG_AGENT, ID: 371767024, model: 746.
// Short name: SWE01220
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_EMPLOYER_REG_AGENT.
/// </para>
/// <para>
/// This AB reads the current registered agent for the selected employer
/// </para>
/// </summary>
[Serializable]
public partial class SiReadEmployerRegAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_EMPLOYER_REG_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadEmployerRegAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadEmployerRegAgent.
  /// </summary>
  public SiReadEmployerRegAgent(IContext context, Import import, Export export):
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
    //   Date   Developer   	Description
    // 10-21-95 H. Sharland    Initial Development
    // 04-30-97 JF. Caillouet	Change Current Date
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    if (!ReadEmployer())
    {
      ExitState = "EMPLOYER_NF";

      return;
    }

    if (ReadRegisteredAgentEmployerRegisteredAgent())
    {
      MoveRegisteredAgent(entities.RegisteredAgent, export.RegisteredAgent);
      export.EmployerRegisteredAgent.Identifier =
        entities.EmployerRegisteredAgent.Identifier;
    }
    else
    {
      if (ReadEmployerRegisteredAgent())
      {
        ExitState = "REGISTERED_AGENT_NF";
      }
      else
      {
        ExitState = "EMPLOYER_REGISTERED_AGENT_NF";
      }

      return;
    }

    if (ReadRegisteredAgentAddress())
    {
      export.RegisteredAgentAddress.Assign(entities.RegisteredAgentAddress);
    }
    else
    {
      ExitState = "REGISTERED_AGENT_ADDRESS_NF";
    }
  }

  private static void MoveRegisteredAgent(RegisteredAgent source,
    RegisteredAgent target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
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
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 3);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 4);
        entities.EmployerRegisteredAgent.Populated = true;
      });
  }

  private bool ReadRegisteredAgentAddress()
  {
    entities.RegisteredAgentAddress.Populated = false;

    return Read("ReadRegisteredAgentAddress",
      (db, command) =>
      {
        db.SetString(command, "ragId", entities.RegisteredAgent.Identifier);
      },
      (db, reader) =>
      {
        entities.RegisteredAgentAddress.Identifier = db.GetString(reader, 0);
        entities.RegisteredAgentAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.RegisteredAgentAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.RegisteredAgentAddress.City = db.GetNullableString(reader, 3);
        entities.RegisteredAgentAddress.State = db.GetNullableString(reader, 4);
        entities.RegisteredAgentAddress.ZipCode5 =
          db.GetNullableString(reader, 5);
        entities.RegisteredAgentAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 7);
        entities.RegisteredAgentAddress.Populated = true;
      });
  }

  private bool ReadRegisteredAgentEmployerRegisteredAgent()
  {
    entities.RegisteredAgent.Populated = false;
    entities.EmployerRegisteredAgent.Populated = false;

    return Read("ReadRegisteredAgentEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.RegisteredAgent.Identifier = db.GetString(reader, 0);
        entities.EmployerRegisteredAgent.RaaId = db.GetString(reader, 0);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 1);
        entities.EmployerRegisteredAgent.Identifier = db.GetString(reader, 2);
        entities.EmployerRegisteredAgent.EffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.EmployerRegisteredAgent.EndDate =
          db.GetNullableDate(reader, 4);
        entities.EmployerRegisteredAgent.EmpId = db.GetInt32(reader, 5);
        entities.RegisteredAgent.Populated = true;
        entities.EmployerRegisteredAgent.Populated = true;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

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
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    private EmployerRegisteredAgent employerRegisteredAgent;
    private RegisteredAgent registeredAgent;
    private RegisteredAgentAddress registeredAgentAddress;
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

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
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
    private RegisteredAgentAddress registeredAgentAddress;
    private EmployerRegisteredAgent employerRegisteredAgent;
    private Employer employer;
  }
#endregion
}

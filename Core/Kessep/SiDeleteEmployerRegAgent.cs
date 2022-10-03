// Program: SI_DELETE_EMPLOYER_REG_AGENT, ID: 371767025, model: 746.
// Short name: SWE01154
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_DELETE_EMPLOYER_REG_AGENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiDeleteEmployerRegAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_EMPLOYER_REG_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteEmployerRegAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteEmployerRegAgent.
  /// </summary>
  public SiDeleteEmployerRegAgent(IContext context, Import import, Export export)
    :
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
    //   Date   Developer   Description
    // 02-29-96 K. Evans    Initial Development
    // --------------------------------------------
    if (ReadEmployerRegisteredAgent())
    {
      DeleteEmployerRegisteredAgent();
    }
    else
    {
      ExitState = "EMPLOYER_REGISTERED_AGENT_NF";
    }
  }

  private void DeleteEmployerRegisteredAgent()
  {
    Update("DeleteEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", entities.EmployerRegisteredAgent.Identifier);
        db.SetString(command, "raaId", entities.EmployerRegisteredAgent.RaaId);
        db.SetInt32(command, "empId", entities.EmployerRegisteredAgent.EmpId);
      });
  }

  private bool ReadEmployerRegisteredAgent()
  {
    entities.EmployerRegisteredAgent.Populated = false;

    return Read("ReadEmployerRegisteredAgent",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", import.EmployerRegisteredAgent.Identifier);
        db.SetInt32(command, "empId", import.Employer.Identifier);
        db.SetString(command, "raaId", import.RegisteredAgent.Identifier);
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
        entities.EmployerRegisteredAgent.Note = db.GetNullableString(reader, 9);
        entities.EmployerRegisteredAgent.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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

    /// <summary>
    /// A value of EmployerRegisteredAgent.
    /// </summary>
    [JsonPropertyName("employerRegisteredAgent")]
    public EmployerRegisteredAgent EmployerRegisteredAgent
    {
      get => employerRegisteredAgent ??= new();
      set => employerRegisteredAgent = value;
    }

    private RegisteredAgent registeredAgent;
    private Employer employer;
    private EmployerRegisteredAgent employerRegisteredAgent;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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

    private RegisteredAgent registeredAgent;
    private Employer employer;
    private EmployerRegisteredAgent employerRegisteredAgent;
  }
#endregion
}

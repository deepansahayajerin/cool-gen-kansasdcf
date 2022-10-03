// Program: SI_CREATE_REGISTERED_AGENT, ID: 371767441, model: 746.
// Short name: SWE01149
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_REGISTERED_AGENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateRegisteredAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_REGISTERED_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateRegisteredAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateRegisteredAgent.
  /// </summary>
  public SiCreateRegisteredAgent(IContext context, Import import, Export export):
    
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
    // 10-24-95 K Evans     Initial Development
    // ---------------------------------------------
    try
    {
      CreateRegisteredAgent();
      export.RegisteredAgent.Assign(entities.RegisteredAgent);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "REGISTERED_AGENT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "REGISTERED_AGENT_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateRegisteredAgent()
  {
    var identifier = import.RegisteredAgent.Identifier;
    var phoneNumber = import.RegisteredAgent.PhoneNumber.GetValueOrDefault();
    var areaCode = import.RegisteredAgent.AreaCode;
    var name = import.RegisteredAgent.Name ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.RegisteredAgent.Populated = false;
    Update("CreateRegisteredAgent",
      (db, command) =>
      {
        db.SetString(command, "identifier", identifier);
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "name", name);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDateTime(command, "updatedTimestamp", null);
        db.SetString(command, "updatedBy", "");
      });

    entities.RegisteredAgent.Identifier = identifier;
    entities.RegisteredAgent.PhoneNumber = phoneNumber;
    entities.RegisteredAgent.AreaCode = areaCode;
    entities.RegisteredAgent.Name = name;
    entities.RegisteredAgent.CreatedBy = createdBy;
    entities.RegisteredAgent.CreatedTimestamp = createdTimestamp;
    entities.RegisteredAgent.UpdatedTimestamp = null;
    entities.RegisteredAgent.UpdatedBy = "";
    entities.RegisteredAgent.Populated = true;
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

    private RegisteredAgent registeredAgent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private RegisteredAgent registeredAgent;
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

    private RegisteredAgent registeredAgent;
  }
#endregion
}

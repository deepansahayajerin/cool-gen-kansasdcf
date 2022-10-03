// Program: SI_UPDATE_REGISTERED_AGENT, ID: 371767442, model: 746.
// Short name: SWE01259
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_REGISTERED_AGENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateRegisteredAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_REGISTERED_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateRegisteredAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateRegisteredAgent.
  /// </summary>
  public SiUpdateRegisteredAgent(IContext context, Import import, Export export):
    
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
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    if (ReadRegisteredAgent())
    {
      try
      {
        UpdateRegisteredAgent();
        export.RegisteredAgent.Assign(entities.RegisteredAgent);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "REGISTERED_AGENT_NU";

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
    else
    {
      ExitState = "REGISTERED_AGENT_NF";
    }
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
        entities.RegisteredAgent.PhoneNumber = db.GetNullableInt32(reader, 1);
        entities.RegisteredAgent.AreaCode = db.GetInt32(reader, 2);
        entities.RegisteredAgent.Name = db.GetNullableString(reader, 3);
        entities.RegisteredAgent.UpdatedTimestamp = db.GetDateTime(reader, 4);
        entities.RegisteredAgent.UpdatedBy = db.GetString(reader, 5);
        entities.RegisteredAgent.Populated = true;
      });
  }

  private void UpdateRegisteredAgent()
  {
    var phoneNumber = import.RegisteredAgent.PhoneNumber.GetValueOrDefault();
    var areaCode = import.RegisteredAgent.AreaCode;
    var name = import.RegisteredAgent.Name ?? "";
    var updatedTimestamp = Now();
    var updatedBy = global.UserId;

    entities.RegisteredAgent.Populated = false;
    Update("UpdateRegisteredAgent",
      (db, command) =>
      {
        db.SetNullableInt32(command, "phoneNumber", phoneNumber);
        db.SetInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "name", name);
        db.SetDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetString(command, "updatedBy", updatedBy);
        db.
          SetString(command, "identifier", entities.RegisteredAgent.Identifier);
          
      });

    entities.RegisteredAgent.PhoneNumber = phoneNumber;
    entities.RegisteredAgent.AreaCode = areaCode;
    entities.RegisteredAgent.Name = name;
    entities.RegisteredAgent.UpdatedTimestamp = updatedTimestamp;
    entities.RegisteredAgent.UpdatedBy = updatedBy;
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

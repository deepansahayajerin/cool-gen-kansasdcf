// Program: SI_CREATE_REGISTERED_AGENT_ADDR, ID: 371767443, model: 746.
// Short name: SWE01150
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_REGISTERED_AGENT_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateRegisteredAgentAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_REGISTERED_AGENT_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateRegisteredAgentAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateRegisteredAgentAddr.
  /// </summary>
  public SiCreateRegisteredAgentAddr(IContext context, Import import,
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
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 09-24-95 K Evans     Initial Development
    // ---------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    if (ReadRegisteredAgent())
    {
      try
      {
        CreateRegisteredAgentAddress();
        export.RegisteredAgentAddress.Assign(entities.RegisteredAgentAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "REGISTERED_AGENT_ADDRESS_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "REGISTERED_AGENT_ADDRESS_PV_RB";

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
      ExitState = "REGISTERED_AGENT_NF_RB";
    }
  }

  private void CreateRegisteredAgentAddress()
  {
    var identifier = import.RegisteredAgentAddress.Identifier;
    var street1 = import.RegisteredAgentAddress.Street1 ?? "";
    var street2 = import.RegisteredAgentAddress.Street2 ?? "";
    var city = import.RegisteredAgentAddress.City ?? "";
    var state = import.RegisteredAgentAddress.State ?? "";
    var zipCode5 = import.RegisteredAgentAddress.ZipCode5 ?? "";
    var zipCode4 = import.RegisteredAgentAddress.ZipCode4 ?? "";
    var zip3 = import.RegisteredAgentAddress.Zip3 ?? "";
    var createdTimestamp = Now();
    var createdBy = global.UserId;
    var ragId = entities.RegisteredAgent.Identifier;

    entities.RegisteredAgentAddress.Populated = false;
    Update("CreateRegisteredAgentAddress",
      (db, command) =>
      {
        db.SetString(command, "identifier", identifier);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetDateTime(command, "updatedTimestamp", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetString(command, "updatedBy", "");
        db.SetString(command, "ragId", ragId);
      });

    entities.RegisteredAgentAddress.Identifier = identifier;
    entities.RegisteredAgentAddress.Street1 = street1;
    entities.RegisteredAgentAddress.Street2 = street2;
    entities.RegisteredAgentAddress.City = city;
    entities.RegisteredAgentAddress.State = state;
    entities.RegisteredAgentAddress.ZipCode5 = zipCode5;
    entities.RegisteredAgentAddress.ZipCode4 = zipCode4;
    entities.RegisteredAgentAddress.Zip3 = zip3;
    entities.RegisteredAgentAddress.CreatedTimestamp = createdTimestamp;
    entities.RegisteredAgentAddress.UpdatedTimestamp = null;
    entities.RegisteredAgentAddress.CreatedBy = createdBy;
    entities.RegisteredAgentAddress.UpdatedBy = "";
    entities.RegisteredAgentAddress.RagId = ragId;
    entities.RegisteredAgentAddress.Populated = true;
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
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    private RegisteredAgent registeredAgent;
    private RegisteredAgentAddress registeredAgentAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RegisteredAgentAddress.
    /// </summary>
    [JsonPropertyName("registeredAgentAddress")]
    public RegisteredAgentAddress RegisteredAgentAddress
    {
      get => registeredAgentAddress ??= new();
      set => registeredAgentAddress = value;
    }

    private RegisteredAgentAddress registeredAgentAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of RegisteredAgent.
    /// </summary>
    [JsonPropertyName("registeredAgent")]
    public RegisteredAgent RegisteredAgent
    {
      get => registeredAgent ??= new();
      set => registeredAgent = value;
    }

    private RegisteredAgentAddress registeredAgentAddress;
    private RegisteredAgent registeredAgent;
  }
#endregion
}

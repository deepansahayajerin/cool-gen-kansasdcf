// Program: SI_UPDATE_REGISTERED_AGENT_ADDR, ID: 371767446, model: 746.
// Short name: SWE01260
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_REGISTERED_AGENT_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateRegisteredAgentAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_REGISTERED_AGENT_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateRegisteredAgentAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateRegisteredAgentAddr.
  /// </summary>
  public SiUpdateRegisteredAgentAddr(IContext context, Import import,
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
    // 10-24-95 K Evans     Initial Development
    // ---------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    if (ReadRegisteredAgentAddress())
    {
      try
      {
        UpdateRegisteredAgentAddress();
        export.RegisteredAgentAddress.Assign(entities.RegisteredAgentAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "REGISTERED_AGENT_ADDRESS_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "REGISTERED_AGENT_ADDRESS_PV";

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

  private bool ReadRegisteredAgentAddress()
  {
    entities.RegisteredAgentAddress.Populated = false;

    return Read("ReadRegisteredAgentAddress",
      (db, command) =>
      {
        db.SetString(
          command, "identifier", import.RegisteredAgentAddress.Identifier);
        db.SetString(command, "ragId", import.RegisteredAgent.Identifier);
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
        entities.RegisteredAgentAddress.Zip3 = db.GetNullableString(reader, 7);
        entities.RegisteredAgentAddress.UpdatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.RegisteredAgentAddress.UpdatedBy = db.GetString(reader, 9);
        entities.RegisteredAgentAddress.RagId = db.GetString(reader, 10);
        entities.RegisteredAgentAddress.Populated = true;
      });
  }

  private void UpdateRegisteredAgentAddress()
  {
    System.Diagnostics.Debug.Assert(entities.RegisteredAgentAddress.Populated);

    var street1 = import.RegisteredAgentAddress.Street1 ?? "";
    var street2 = import.RegisteredAgentAddress.Street2 ?? "";
    var city = import.RegisteredAgentAddress.City ?? "";
    var state = import.RegisteredAgentAddress.State ?? "";
    var zipCode5 = import.RegisteredAgentAddress.ZipCode5 ?? "";
    var zipCode4 = import.RegisteredAgentAddress.ZipCode4 ?? "";
    var zip3 = import.RegisteredAgentAddress.Zip3 ?? "";
    var updatedTimestamp = Now();
    var updatedBy = global.UserId;

    entities.RegisteredAgentAddress.Populated = false;
    Update("UpdateRegisteredAgentAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetDateTime(command, "updatedTimestamp", updatedTimestamp);
        db.SetString(command, "updatedBy", updatedBy);
        db.SetString(
          command, "identifier", entities.RegisteredAgentAddress.Identifier);
        db.SetString(command, "ragId", entities.RegisteredAgentAddress.RagId);
      });

    entities.RegisteredAgentAddress.Street1 = street1;
    entities.RegisteredAgentAddress.Street2 = street2;
    entities.RegisteredAgentAddress.City = city;
    entities.RegisteredAgentAddress.State = state;
    entities.RegisteredAgentAddress.ZipCode5 = zipCode5;
    entities.RegisteredAgentAddress.ZipCode4 = zipCode4;
    entities.RegisteredAgentAddress.Zip3 = zip3;
    entities.RegisteredAgentAddress.UpdatedTimestamp = updatedTimestamp;
    entities.RegisteredAgentAddress.UpdatedBy = updatedBy;
    entities.RegisteredAgentAddress.Populated = true;
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

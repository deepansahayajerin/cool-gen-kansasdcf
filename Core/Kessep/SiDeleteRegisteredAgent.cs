// Program: SI_DELETE_REGISTERED_AGENT, ID: 371767444, model: 746.
// Short name: SWE01162
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_DELETE_REGISTERED_AGENT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiDeleteRegisteredAgent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_DELETE_REGISTERED_AGENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiDeleteRegisteredAgent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiDeleteRegisteredAgent.
  /// </summary>
  public SiDeleteRegisteredAgent(IContext context, Import import, Export export):
    
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
    if (ReadRegisteredAgent())
    {
      DeleteRegisteredAgent();
    }
    else
    {
      ExitState = "REGISTERED_AGENT_NF";
    }
  }

  private void DeleteRegisteredAgent()
  {
    Update("DeleteRegisteredAgent",
      (db, command) =>
      {
        db.
          SetString(command, "identifier", entities.RegisteredAgent.Identifier);
          
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

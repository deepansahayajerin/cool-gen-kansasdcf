// Program: LE_DISC_DELETE_DISCOVERY, ID: 372025876, model: 746.
// Short name: SWE00761
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DISC_DELETE_DISCOVERY.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process deletes DISCOVERY.
/// </para>
/// </summary>
[Serializable]
public partial class LeDiscDeleteDiscovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISC_DELETE_DISCOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDiscDeleteDiscovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDiscDeleteDiscovery.
  /// </summary>
  public LeDiscDeleteDiscovery(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDiscovery())
    {
      DeleteDiscovery();
    }
    else
    {
      ExitState = "DISCOVERY_NF";
    }
  }

  private void DeleteDiscovery()
  {
    Update("DeleteDiscovery",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.Discovery.LgaIdentifier);
        db.SetDate(
          command, "requestedDt",
          entities.Discovery.RequestedDate.GetValueOrDefault());
      });
  }

  private bool ReadDiscovery()
  {
    entities.Discovery.Populated = false;

    return Read("ReadDiscovery",
      (db, command) =>
      {
        db.SetDate(
          command, "requestedDt",
          import.Discovery.RequestedDate.GetValueOrDefault());
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Discovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.Discovery.RequestedDate = db.GetDate(reader, 1);
        entities.Discovery.ResponseDate = db.GetNullableDate(reader, 2);
        entities.Discovery.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Discovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 4);
        entities.Discovery.ResponseDescription =
          db.GetNullableString(reader, 5);
        entities.Discovery.Populated = true;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    private LegalAction legalAction;
    private Discovery discovery;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    private LegalAction legalAction;
    private Discovery discovery;
  }
#endregion
}

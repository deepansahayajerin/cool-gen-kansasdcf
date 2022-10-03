// Program: LE_DELETE_LEGAL_ACTION_RESPONSE, ID: 371981365, model: 746.
// Short name: SWE00759
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_DELETE_LEGAL_ACTION_RESPONSE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// Delete an occurrence of the Legal Action Response related to the specified 
/// Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeDeleteLegalActionResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DELETE_LEGAL_ACTION_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDeleteLegalActionResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDeleteLegalActionResponse.
  /// </summary>
  public LeDeleteLegalActionResponse(IContext context, Import import,
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
    // ------------------------------------------------------------
    // Date		Developer	Request #	Description
    // 06/20/95	Dave Allen			Initial Code
    // ------------------------------------------------------------
    if (ReadLegalAction())
    {
      if (ReadLegalActionResponse())
      {
        DeleteLegalActionResponse();
      }
      else
      {
        ExitState = "CO0000_LEGAL_ACTION_RESPONSE_NF";
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private void DeleteLegalActionResponse()
  {
    Update("DeleteLegalActionResponse",
      (db, command) =>
      {
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionResponse.LgaIdentifier);
          
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalActionResponse.CreatedTstamp.GetValueOrDefault());
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionResponse()
  {
    entities.LegalActionResponse.Populated = false;

    return Read("ReadLegalActionResponse",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetDateTime(
          command, "createdTstamp",
          import.LegalActionResponse.CreatedTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalActionResponse.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionResponse.CreatedTstamp = db.GetDateTime(reader, 1);
        entities.LegalActionResponse.Populated = true;
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
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
    }

    private LegalAction legalAction;
    private LegalActionResponse legalActionResponse;
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
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
    }

    private LegalAction legalAction;
    private LegalActionResponse legalActionResponse;
  }
#endregion
}

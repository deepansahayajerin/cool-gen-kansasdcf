// Program: LE_UPDATE_LEGAL_ACTION_RESPONSE, ID: 371981367, model: 746.
// Short name: SWE00833
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_UPDATE_LEGAL_ACTION_RESPONSE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// Update an occurrence of the Legal Action Response related to a specific 
/// Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeUpdateLegalActionResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_UPDATE_LEGAL_ACTION_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeUpdateLegalActionResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeUpdateLegalActionResponse.
  /// </summary>
  public LeUpdateLegalActionResponse(IContext context, Import import,
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
        try
        {
          UpdateLegalActionResponse();

          // -------------------
          // Continue processing
          // -------------------
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CO0000_LEGAL_ACTION_RESPONSE_NU";

              break;
            case ErrorCode.PermittedValueViolation:
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
        ExitState = "CO0000_LEGAL_ACTION_RESPONSE_NF";
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";
    }
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
        entities.LegalActionResponse.Type1 = db.GetString(reader, 2);
        entities.LegalActionResponse.ReceivedDate = db.GetDate(reader, 3);
        entities.LegalActionResponse.LastName = db.GetString(reader, 4);
        entities.LegalActionResponse.FirstName = db.GetString(reader, 5);
        entities.LegalActionResponse.MiddleInitial =
          db.GetNullableString(reader, 6);
        entities.LegalActionResponse.Relationship =
          db.GetNullableString(reader, 7);
        entities.LegalActionResponse.CreatedBy = db.GetString(reader, 8);
        entities.LegalActionResponse.RespForFirstName =
          db.GetNullableString(reader, 9);
        entities.LegalActionResponse.RespForMiddleInitial =
          db.GetNullableString(reader, 10);
        entities.LegalActionResponse.RespForLastName =
          db.GetNullableString(reader, 11);
        entities.LegalActionResponse.Narrative =
          db.GetNullableString(reader, 12);
        entities.LegalActionResponse.Populated = true;
      });
  }

  private void UpdateLegalActionResponse()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionResponse.Populated);

    var type1 = import.LegalActionResponse.Type1;
    var receivedDate = import.LegalActionResponse.ReceivedDate;
    var lastName = import.LegalActionResponse.LastName;
    var firstName = import.LegalActionResponse.FirstName;
    var middleInitial = import.LegalActionResponse.MiddleInitial ?? "";
    var relationship = import.LegalActionResponse.Relationship ?? "";
    var respForFirstName = import.LegalActionResponse.RespForFirstName ?? "";
    var respForMiddleInitial =
      import.LegalActionResponse.RespForMiddleInitial ?? "";
    var respForLastName = import.LegalActionResponse.RespForLastName ?? "";
    var narrative = import.LegalActionResponse.Narrative ?? "";

    entities.LegalActionResponse.Populated = false;
    Update("UpdateLegalActionResponse",
      (db, command) =>
      {
        db.SetString(command, "type", type1);
        db.SetDate(command, "receivedDt", receivedDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "relationship", relationship);
        db.SetNullableString(command, "respForFirstNam", respForFirstName);
        db.SetNullableString(command, "respForMiddleIn", respForMiddleInitial);
        db.SetNullableString(command, "respForLastName", respForLastName);
        db.SetNullableString(command, "narrative", narrative);
        db.SetInt32(
          command, "lgaIdentifier", entities.LegalActionResponse.LgaIdentifier);
          
        db.SetDateTime(
          command, "createdTstamp",
          entities.LegalActionResponse.CreatedTstamp.GetValueOrDefault());
      });

    entities.LegalActionResponse.Type1 = type1;
    entities.LegalActionResponse.ReceivedDate = receivedDate;
    entities.LegalActionResponse.LastName = lastName;
    entities.LegalActionResponse.FirstName = firstName;
    entities.LegalActionResponse.MiddleInitial = middleInitial;
    entities.LegalActionResponse.Relationship = relationship;
    entities.LegalActionResponse.RespForFirstName = respForFirstName;
    entities.LegalActionResponse.RespForMiddleInitial = respForMiddleInitial;
    entities.LegalActionResponse.RespForLastName = respForLastName;
    entities.LegalActionResponse.Narrative = narrative;
    entities.LegalActionResponse.Populated = true;
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

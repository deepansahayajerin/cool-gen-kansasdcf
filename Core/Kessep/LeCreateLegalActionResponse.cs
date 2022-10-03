// Program: LE_CREATE_LEGAL_ACTION_RESPONSE, ID: 371981368, model: 746.
// Short name: SWE00743
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CREATE_LEGAL_ACTION_RESPONSE.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// Create an occurrence of a Legal Action Response and relate it to the 
/// specified Legal Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeCreateLegalActionResponse: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_LEGAL_ACTION_RESPONSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateLegalActionResponse(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateLegalActionResponse.
  /// </summary>
  public LeCreateLegalActionResponse(IContext context, Import import,
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
      try
      {
        CreateLegalActionResponse();
        export.LegalActionResponse.Assign(entities.LegalActionResponse);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "LEGAL_ACTION_RESPONSE_AE";

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
      ExitState = "LEGAL_ACTION_NF";
    }
  }

  private void CreateLegalActionResponse()
  {
    var lgaIdentifier = entities.LegalAction.Identifier;
    var createdTstamp = Now();
    var type1 = import.LegalActionResponse.Type1;
    var receivedDate = import.LegalActionResponse.ReceivedDate;
    var lastName = import.LegalActionResponse.LastName;
    var firstName = import.LegalActionResponse.FirstName;
    var middleInitial = import.LegalActionResponse.MiddleInitial ?? "";
    var relationship = import.LegalActionResponse.Relationship ?? "";
    var createdBy = global.UserId;
    var respForFirstName = import.LegalActionResponse.RespForFirstName ?? "";
    var respForMiddleInitial =
      import.LegalActionResponse.RespForMiddleInitial ?? "";
    var respForLastName = import.LegalActionResponse.RespForLastName ?? "";
    var narrative = import.LegalActionResponse.Narrative ?? "";

    entities.LegalActionResponse.Populated = false;
    Update("CreateLegalActionResponse",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "type", type1);
        db.SetDate(command, "receivedDt", receivedDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "suffix", "");
        db.SetNullableString(command, "relationship", relationship);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "respForFirstNam", respForFirstName);
        db.SetNullableString(command, "respForMiddleIn", respForMiddleInitial);
        db.SetNullableString(command, "respForLastName", respForLastName);
        db.SetNullableString(command, "narrative", narrative);
      });

    entities.LegalActionResponse.LgaIdentifier = lgaIdentifier;
    entities.LegalActionResponse.CreatedTstamp = createdTstamp;
    entities.LegalActionResponse.Type1 = type1;
    entities.LegalActionResponse.ReceivedDate = receivedDate;
    entities.LegalActionResponse.LastName = lastName;
    entities.LegalActionResponse.FirstName = firstName;
    entities.LegalActionResponse.MiddleInitial = middleInitial;
    entities.LegalActionResponse.Relationship = relationship;
    entities.LegalActionResponse.CreatedBy = createdBy;
    entities.LegalActionResponse.RespForFirstName = respForFirstName;
    entities.LegalActionResponse.RespForMiddleInitial = respForMiddleInitial;
    entities.LegalActionResponse.RespForLastName = respForLastName;
    entities.LegalActionResponse.Narrative = narrative;
    entities.LegalActionResponse.Populated = true;
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
    /// <summary>
    /// A value of LegalActionResponse.
    /// </summary>
    [JsonPropertyName("legalActionResponse")]
    public LegalActionResponse LegalActionResponse
    {
      get => legalActionResponse ??= new();
      set => legalActionResponse = value;
    }

    private LegalActionResponse legalActionResponse;
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

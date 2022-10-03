// Program: ADD_DISCOVERY, ID: 372025874, model: 746.
// Short name: SWE00006
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: ADD_DISCOVERY.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates DISCOVERY.
/// </para>
/// </summary>
[Serializable]
public partial class AddDiscovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ADD_DISCOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new AddDiscovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of AddDiscovery.
  /// </summary>
  public AddDiscovery(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadLegalAction())
    {
      try
      {
        CreateDiscovery();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISCOVERY_AE";

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

  private void CreateDiscovery()
  {
    var lgaIdentifier = entities.Existing.Identifier;
    var requestedDate = import.Discovery.RequestedDate;
    var lastName = import.Discovery.LastName;
    var firstName = import.Discovery.FirstName;
    var middleInt = import.Discovery.MiddleInt ?? "";
    var suffix = import.Discovery.Suffix ?? "";
    var responseDate = import.Discovery.ResponseDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var requestedByCseInd = import.Discovery.RequestedByCseInd ?? "";
    var respReqByFirstName = import.Discovery.RespReqByFirstName ?? "";
    var respReqByMi = import.Discovery.RespReqByMi ?? "";
    var respReqByLastName = import.Discovery.RespReqByLastName ?? "";
    var requestDescription = import.Discovery.RequestDescription ?? "";
    var responseDescription = import.Discovery.ResponseDescription ?? "";

    entities.New1.Populated = false;
    Update("CreateDiscovery",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetDate(command, "requestedDt", requestedDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInt", middleInt);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableDate(command, "responseDt", responseDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "reqByCseInd", requestedByCseInd);
        db.SetNullableString(command, "respReqFirst", respReqByFirstName);
        db.SetNullableString(command, "respReqByMi", respReqByMi);
        db.SetNullableString(command, "respReqByLast", respReqByLastName);
        db.SetNullableString(command, "requestDesc", requestDescription);
        db.SetNullableString(command, "responseDesc", responseDescription);
      });

    entities.New1.LgaIdentifier = lgaIdentifier;
    entities.New1.RequestedDate = requestedDate;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.MiddleInt = middleInt;
    entities.New1.Suffix = suffix;
    entities.New1.ResponseDate = responseDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.RequestedByCseInd = requestedByCseInd;
    entities.New1.RespReqByFirstName = respReqByFirstName;
    entities.New1.RespReqByMi = respReqByMi;
    entities.New1.RespReqByLastName = respReqByLastName;
    entities.New1.RequestDescription = requestDescription;
    entities.New1.ResponseDescription = responseDescription;
    entities.New1.Populated = true;
  }

  private bool ReadLegalAction()
  {
    entities.Existing.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.Identifier = db.GetInt32(reader, 0);
        entities.Existing.Populated = true;
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
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private Discovery discovery;
    private LegalAction legalAction;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public Discovery New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public LegalAction Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private Discovery new1;
    private LegalAction existing;
  }
#endregion
}

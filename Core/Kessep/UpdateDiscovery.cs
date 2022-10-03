// Program: UPDATE_DISCOVERY, ID: 372025878, model: 746.
// Short name: SWE01477
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_DISCOVERY.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process updates DISCOVERY.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateDiscovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_DISCOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateDiscovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateDiscovery.
  /// </summary>
  public UpdateDiscovery(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ResponseDateChanged.Flag = "N";

    if (ReadDiscovery())
    {
      if (!Equal(import.Discovery.ResponseDate,
        entities.ExistingDiscovery.ResponseDate))
      {
        export.ResponseDateChanged.Flag = "Y";
      }

      try
      {
        UpdateDiscovery1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISCOVERY_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DISCOVERY_PV";

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
      ExitState = "DISCOVERY_NF";
    }
  }

  private bool ReadDiscovery()
  {
    entities.ExistingDiscovery.Populated = false;

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
        entities.ExistingDiscovery.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ExistingDiscovery.RequestedDate = db.GetDate(reader, 1);
        entities.ExistingDiscovery.LastName = db.GetString(reader, 2);
        entities.ExistingDiscovery.FirstName = db.GetString(reader, 3);
        entities.ExistingDiscovery.MiddleInt = db.GetNullableString(reader, 4);
        entities.ExistingDiscovery.Suffix = db.GetNullableString(reader, 5);
        entities.ExistingDiscovery.ResponseDate = db.GetNullableDate(reader, 6);
        entities.ExistingDiscovery.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ExistingDiscovery.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingDiscovery.RequestedByCseInd =
          db.GetNullableString(reader, 9);
        entities.ExistingDiscovery.RespReqByFirstName =
          db.GetNullableString(reader, 10);
        entities.ExistingDiscovery.RespReqByMi =
          db.GetNullableString(reader, 11);
        entities.ExistingDiscovery.RespReqByLastName =
          db.GetNullableString(reader, 12);
        entities.ExistingDiscovery.RequestDescription =
          db.GetNullableString(reader, 13);
        entities.ExistingDiscovery.ResponseDescription =
          db.GetNullableString(reader, 14);
        entities.ExistingDiscovery.Populated = true;
      });
  }

  private void UpdateDiscovery1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDiscovery.Populated);

    var lastName = import.Discovery.LastName;
    var firstName = import.Discovery.FirstName;
    var middleInt = import.Discovery.MiddleInt ?? "";
    var suffix = import.Discovery.Suffix ?? "";
    var responseDate = import.Discovery.ResponseDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var requestedByCseInd = import.Discovery.RequestedByCseInd ?? "";
    var respReqByFirstName = import.Discovery.RespReqByFirstName ?? "";
    var respReqByMi = import.Discovery.RespReqByMi ?? "";
    var respReqByLastName = import.Discovery.RespReqByLastName ?? "";
    var requestDescription = import.Discovery.RequestDescription ?? "";
    var responseDescription = import.Discovery.ResponseDescription ?? "";

    entities.ExistingDiscovery.Populated = false;
    Update("UpdateDiscovery",
      (db, command) =>
      {
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInt", middleInt);
        db.SetNullableString(command, "suffix", suffix);
        db.SetNullableDate(command, "responseDt", responseDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "reqByCseInd", requestedByCseInd);
        db.SetNullableString(command, "respReqFirst", respReqByFirstName);
        db.SetNullableString(command, "respReqByMi", respReqByMi);
        db.SetNullableString(command, "respReqByLast", respReqByLastName);
        db.SetNullableString(command, "requestDesc", requestDescription);
        db.SetNullableString(command, "responseDesc", responseDescription);
        db.SetInt32(
          command, "lgaIdentifier", entities.ExistingDiscovery.LgaIdentifier);
        db.SetDate(
          command, "requestedDt",
          entities.ExistingDiscovery.RequestedDate.GetValueOrDefault());
      });

    entities.ExistingDiscovery.LastName = lastName;
    entities.ExistingDiscovery.FirstName = firstName;
    entities.ExistingDiscovery.MiddleInt = middleInt;
    entities.ExistingDiscovery.Suffix = suffix;
    entities.ExistingDiscovery.ResponseDate = responseDate;
    entities.ExistingDiscovery.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingDiscovery.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.ExistingDiscovery.RequestedByCseInd = requestedByCseInd;
    entities.ExistingDiscovery.RespReqByFirstName = respReqByFirstName;
    entities.ExistingDiscovery.RespReqByMi = respReqByMi;
    entities.ExistingDiscovery.RespReqByLastName = respReqByLastName;
    entities.ExistingDiscovery.RequestDescription = requestDescription;
    entities.ExistingDiscovery.ResponseDescription = responseDescription;
    entities.ExistingDiscovery.Populated = true;
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
    /// <summary>
    /// A value of ResponseDateChanged.
    /// </summary>
    [JsonPropertyName("responseDateChanged")]
    public Common ResponseDateChanged
    {
      get => responseDateChanged ??= new();
      set => responseDateChanged = value;
    }

    private Common responseDateChanged;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingDiscovery.
    /// </summary>
    [JsonPropertyName("existingDiscovery")]
    public Discovery ExistingDiscovery
    {
      get => existingDiscovery ??= new();
      set => existingDiscovery = value;
    }

    private LegalAction existingLegalAction;
    private Discovery existingDiscovery;
  }
#endregion
}

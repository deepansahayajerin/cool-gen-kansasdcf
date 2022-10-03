// Program: OE_FPLS_UPDATE_FPLS_REQUEST, ID: 372355021, model: 746.
// Short name: SWE01210
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FPLS_UPDATE_FPLS_REQUEST.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block is used by a Maintenance screen and updates existing FPLS-
/// REQUEST's.
/// a FPLS_REQUEST is a request to the Federal Person Locator Service for 
/// information.
/// The only imported Transaction-Status allowed is 'C'(Created). This allows an
/// existing 'S'(Send) or 'R'(Resend) FPLS status to be changed back to a 'C'(
/// Created).
/// For 'C'(Created) status FPLS_REQUEST's, the only updatable field is the SESA
/// states.
/// </para>
/// </summary>
[Serializable]
public partial class OeFplsUpdateFplsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_UPDATE_FPLS_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsUpdateFplsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsUpdateFplsRequest.
  /// </summary>
  public OeFplsUpdateFplsRequest(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // Developer     Date
    // MK            9/16/98
    // Modified NOT FOUND on extended read to determine
    // if CSE PERSON or FPLS REQUEST was NOT FOUND
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadCsePersonFplsLocateRequest())
    {
      local.Sesa.Text32 = "";

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (!IsEmpty(import.Group.Item.Det.State))
        {
          if (Equal(import.Group.Item.Det.State, "KS"))
          {
            ExitState = "OE0142_KS_IS_NOT_ENTERABLE";

            return;
          }

          local.Sesa.Text32 = TrimEnd(local.Sesa.Text32) + "B" + import
            .Group.Item.Det.State;
        }
      }

      local.SendDirective.UsersField = import.FplsLocateRequest.UsersField ?? ""
        ;

      if (IsEmpty(local.Sesa.Text32))
      {
        local.SendDirective.UsersField = "Y";
      }

      if (!Equal(local.SendDirective.UsersField, "Y") && !
        Equal(local.SendDirective.UsersField, "N"))
      {
        local.SendDirective.UsersField = "Y";
      }

      try
      {
        UpdateFplsLocateRequest();
        export.FplsLocateRequest.Assign(entities.ExistingFplsLocateRequest);
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OE0016_NO_UPDATE_DONE";

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
      // ************************************************
      // MK            9/16/98
      // Modified NOT FOUND on extended read to determine
      // if CSE PERSON or FPLS REQUEST was NOT FOUND
      // ************************************************
      if (ReadCsePerson())
      {
        ExitState = "OE0000_FPLS_REQUEST_NF";
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }
    }
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonFplsLocateRequest()
  {
    entities.ExistingCsePerson.Populated = false;
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadCsePersonFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "identifier", import.FplsLocateRequest.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.ZdelReqCreatDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingFplsLocateRequest.ZdelCreatUserId =
          db.GetString(reader, 4);
        entities.ExistingFplsLocateRequest.StateAbbr =
          db.GetNullableString(reader, 5);
        entities.ExistingFplsLocateRequest.StationNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingFplsLocateRequest.TransactionType =
          db.GetNullableString(reader, 7);
        entities.ExistingFplsLocateRequest.Ssn =
          db.GetNullableString(reader, 8);
        entities.ExistingFplsLocateRequest.CaseId =
          db.GetNullableString(reader, 9);
        entities.ExistingFplsLocateRequest.LocalCode =
          db.GetNullableString(reader, 10);
        entities.ExistingFplsLocateRequest.UsersField =
          db.GetNullableString(reader, 11);
        entities.ExistingFplsLocateRequest.TypeOfCase =
          db.GetNullableString(reader, 12);
        entities.ExistingFplsLocateRequest.ApFirstName =
          db.GetNullableString(reader, 13);
        entities.ExistingFplsLocateRequest.ApMiddleName =
          db.GetNullableString(reader, 14);
        entities.ExistingFplsLocateRequest.ApFirstLastName =
          db.GetNullableString(reader, 15);
        entities.ExistingFplsLocateRequest.ApSecondLastName =
          db.GetNullableString(reader, 16);
        entities.ExistingFplsLocateRequest.ApThirdLastName =
          db.GetNullableString(reader, 17);
        entities.ExistingFplsLocateRequest.ApDateOfBirth =
          db.GetNullableDate(reader, 18);
        entities.ExistingFplsLocateRequest.Sex =
          db.GetNullableString(reader, 19);
        entities.ExistingFplsLocateRequest.CollectAllResponsesTogether =
          db.GetNullableString(reader, 20);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 21);
        entities.ExistingFplsLocateRequest.ApCityOfBirth =
          db.GetNullableString(reader, 22);
        entities.ExistingFplsLocateRequest.ApStateOrCountryOfBirth =
          db.GetNullableString(reader, 23);
        entities.ExistingFplsLocateRequest.ApsFathersFirstName =
          db.GetNullableString(reader, 24);
        entities.ExistingFplsLocateRequest.ApsFathersMi =
          db.GetNullableString(reader, 25);
        entities.ExistingFplsLocateRequest.ApsFathersLastName =
          db.GetNullableString(reader, 26);
        entities.ExistingFplsLocateRequest.ApsMothersFirstName =
          db.GetNullableString(reader, 27);
        entities.ExistingFplsLocateRequest.ApsMothersMi =
          db.GetNullableString(reader, 28);
        entities.ExistingFplsLocateRequest.ApsMothersMaidenName =
          db.GetNullableString(reader, 29);
        entities.ExistingFplsLocateRequest.CpSsn =
          db.GetNullableString(reader, 30);
        entities.ExistingFplsLocateRequest.CreatedBy = db.GetString(reader, 31);
        entities.ExistingFplsLocateRequest.CreatedTimestamp =
          db.GetDateTime(reader, 32);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 33);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 34);
        entities.ExistingFplsLocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 35);
        entities.ExistingFplsLocateRequest.SendRequestTo =
          db.GetNullableString(reader, 36);
        entities.ExistingCsePerson.Populated = true;
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);

    var transactionStatus = import.FplsLocateRequest.TransactionStatus ?? "";
    var usersField = local.SendDirective.UsersField ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var sendRequestTo = local.Sesa.Text32;

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetNullableString(command, "usersField", usersField);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingFplsLocateRequest.Identifier);
          
      });

    entities.ExistingFplsLocateRequest.TransactionStatus = transactionStatus;
    entities.ExistingFplsLocateRequest.UsersField = usersField;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.ExistingFplsLocateRequest.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Det.
      /// </summary>
      [JsonPropertyName("det")]
      public OblgWork Det
      {
        get => det ??= new();
        set => det = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private OblgWork det;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private Array<GroupGroup> group;
    private FplsLocateRequest fplsLocateRequest;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FplsLocateRequest.
    /// </summary>
    [JsonPropertyName("fplsLocateRequest")]
    public FplsLocateRequest FplsLocateRequest
    {
      get => fplsLocateRequest ??= new();
      set => fplsLocateRequest = value;
    }

    private FplsLocateRequest fplsLocateRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of SendDirective.
    /// </summary>
    [JsonPropertyName("sendDirective")]
    public FplsLocateRequest SendDirective
    {
      get => sendDirective ??= new();
      set => sendDirective = value;
    }

    /// <summary>
    /// A value of Sesa.
    /// </summary>
    [JsonPropertyName("sesa")]
    public WorkArea Sesa
    {
      get => sesa ??= new();
      set => sesa = value;
    }

    private FplsLocateRequest sendDirective;
    private WorkArea sesa;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    private CsePerson existingCsePerson;
    private FplsLocateRequest existingFplsLocateRequest;
  }
#endregion
}

// Program: SI_B276_CREATE_FPLS_LOCATE_REQST, ID: 373400582, model: 746.
// Short name: SWE01300
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B276_CREATE_FPLS_LOCATE_REQST.
/// </summary>
[Serializable]
public partial class SiB276CreateFplsLocateReqst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B276_CREATE_FPLS_LOCATE_REQST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB276CreateFplsLocateReqst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB276CreateFplsLocateReqst.
  /// </summary>
  public SiB276CreateFplsLocateReqst(IContext context, Import import,
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
    if (ReadCsePerson())
    {
      try
      {
        CreateFplsLocateRequest();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FPLS_LOCATE_REQUEST_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FPLS_LOCATE_REQUEST_PV";

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
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void CreateFplsLocateRequest()
  {
    var cspNumber = entities.CsePerson.Number;
    var identifier = import.FplsLocateRequest.Identifier;
    var transactionStatus = import.FplsLocateRequest.TransactionStatus ?? "";
    var zdelReqCreatDt = import.FplsLocateRequest.ZdelReqCreatDt;
    var zdelCreatUserId = import.FplsLocateRequest.ZdelCreatUserId;
    var stateAbbr = import.FplsLocateRequest.StateAbbr ?? "";
    var stationNumber = import.FplsLocateRequest.StationNumber ?? "";
    var transactionType = import.FplsLocateRequest.TransactionType ?? "";
    var ssn = import.FplsLocateRequest.Ssn ?? "";
    var caseId = import.FplsLocateRequest.CaseId ?? "";
    var localCode = import.FplsLocateRequest.LocalCode ?? "";
    var usersField = import.FplsLocateRequest.UsersField ?? "";
    var typeOfCase = import.FplsLocateRequest.TypeOfCase ?? "";
    var apFirstName = import.FplsLocateRequest.ApFirstName ?? "";
    var apMiddleName = import.FplsLocateRequest.ApMiddleName ?? "";
    var apFirstLastName = import.FplsLocateRequest.ApFirstLastName ?? "";
    var apSecondLastName = import.FplsLocateRequest.ApSecondLastName ?? "";
    var apThirdLastName = import.FplsLocateRequest.ApThirdLastName ?? "";
    var apDateOfBirth = import.FplsLocateRequest.ApDateOfBirth;
    var sex = import.FplsLocateRequest.Sex ?? "";
    var collectAllResponsesTogether =
      import.FplsLocateRequest.CollectAllResponsesTogether ?? "";
    var transactionError = import.FplsLocateRequest.TransactionError ?? "";
    var apCityOfBirth = import.FplsLocateRequest.ApCityOfBirth ?? "";
    var apStateOrCountryOfBirth =
      import.FplsLocateRequest.ApStateOrCountryOfBirth ?? "";
    var apsFathersFirstName = import.FplsLocateRequest.ApsFathersFirstName ?? ""
      ;
    var apsFathersMi = import.FplsLocateRequest.ApsFathersMi ?? "";
    var apsFathersLastName = import.FplsLocateRequest.ApsFathersLastName ?? "";
    var apsMothersFirstName = import.FplsLocateRequest.ApsMothersFirstName ?? ""
      ;
    var apsMothersMi = import.FplsLocateRequest.ApsMothersMi ?? "";
    var apsMothersMaidenName =
      import.FplsLocateRequest.ApsMothersMaidenName ?? "";
    var cpSsn = import.FplsLocateRequest.CpSsn ?? "";
    var createdBy = import.FplsLocateRequest.CreatedBy;
    var createdTimestamp = Now();
    var lastUpdatedBy = import.FplsLocateRequest.LastUpdatedBy;
    var requestSentDate = import.FplsLocateRequest.RequestSentDate;
    var sendRequestTo = import.FplsLocateRequest.SendRequestTo ?? "";

    entities.FplsLocateRequest.Populated = false;
    Update("CreateFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "transactionStatus", transactionStatus);
        db.SetNullableDate(command, "zdelReqCreatDt", zdelReqCreatDt);
        db.SetString(command, "zdelCreatUserId", zdelCreatUserId);
        db.SetNullableString(command, "stateAbbr", stateAbbr);
        db.SetNullableString(command, "stationNumber", stationNumber);
        db.SetNullableString(command, "transactionType", transactionType);
        db.SetNullableString(command, "ssn", ssn);
        db.SetNullableString(command, "caseId", caseId);
        db.SetNullableString(command, "localCode", localCode);
        db.SetNullableString(command, "usersField", usersField);
        db.SetNullableString(command, "typeOfCase", typeOfCase);
        db.SetNullableString(command, "apFirstName", apFirstName);
        db.SetNullableString(command, "apMiddleName", apMiddleName);
        db.SetNullableString(command, "apFirstLastName", apFirstLastName);
        db.SetNullableString(command, "apSecondLastNam", apSecondLastName);
        db.SetNullableString(command, "apThirdLastName", apThirdLastName);
        db.SetNullableDate(command, "apDateOfBirth", apDateOfBirth);
        db.SetNullableString(command, "sex", sex);
        db.SetNullableString(
          command, "collectAllRespon", collectAllResponsesTogether);
        db.SetNullableString(command, "transactionError", transactionError);
        db.SetNullableString(command, "apCityOfBirth", apCityOfBirth);
        db.
          SetNullableString(command, "apStateOrCountr", apStateOrCountryOfBirth);
          
        db.SetNullableString(command, "apsFathersFirst", apsFathersFirstName);
        db.SetNullableString(command, "apsFathersMi", apsFathersMi);
        db.SetNullableString(command, "apsFathersLastN", apsFathersLastName);
        db.SetNullableString(command, "apsMothersFirst", apsMothersFirstName);
        db.SetNullableString(command, "apsMothersMi", apsMothersMi);
        db.SetNullableString(command, "apsMothersMaiden", apsMothersMaidenName);
        db.SetNullableString(command, "cpSsn", cpSsn);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableDate(command, "requestSentDate", requestSentDate);
        db.SetNullableString(command, "sendRequestTo", sendRequestTo);
      });

    entities.FplsLocateRequest.CspNumber = cspNumber;
    entities.FplsLocateRequest.Identifier = identifier;
    entities.FplsLocateRequest.TransactionStatus = transactionStatus;
    entities.FplsLocateRequest.ZdelReqCreatDt = zdelReqCreatDt;
    entities.FplsLocateRequest.ZdelCreatUserId = zdelCreatUserId;
    entities.FplsLocateRequest.StateAbbr = stateAbbr;
    entities.FplsLocateRequest.StationNumber = stationNumber;
    entities.FplsLocateRequest.TransactionType = transactionType;
    entities.FplsLocateRequest.Ssn = ssn;
    entities.FplsLocateRequest.CaseId = caseId;
    entities.FplsLocateRequest.LocalCode = localCode;
    entities.FplsLocateRequest.UsersField = usersField;
    entities.FplsLocateRequest.TypeOfCase = typeOfCase;
    entities.FplsLocateRequest.ApFirstName = apFirstName;
    entities.FplsLocateRequest.ApMiddleName = apMiddleName;
    entities.FplsLocateRequest.ApFirstLastName = apFirstLastName;
    entities.FplsLocateRequest.ApSecondLastName = apSecondLastName;
    entities.FplsLocateRequest.ApThirdLastName = apThirdLastName;
    entities.FplsLocateRequest.ApDateOfBirth = apDateOfBirth;
    entities.FplsLocateRequest.Sex = sex;
    entities.FplsLocateRequest.CollectAllResponsesTogether =
      collectAllResponsesTogether;
    entities.FplsLocateRequest.TransactionError = transactionError;
    entities.FplsLocateRequest.ApCityOfBirth = apCityOfBirth;
    entities.FplsLocateRequest.ApStateOrCountryOfBirth =
      apStateOrCountryOfBirth;
    entities.FplsLocateRequest.ApsFathersFirstName = apsFathersFirstName;
    entities.FplsLocateRequest.ApsFathersMi = apsFathersMi;
    entities.FplsLocateRequest.ApsFathersLastName = apsFathersLastName;
    entities.FplsLocateRequest.ApsMothersFirstName = apsMothersFirstName;
    entities.FplsLocateRequest.ApsMothersMi = apsMothersMi;
    entities.FplsLocateRequest.ApsMothersMaidenName = apsMothersMaidenName;
    entities.FplsLocateRequest.CpSsn = cpSsn;
    entities.FplsLocateRequest.CreatedBy = createdBy;
    entities.FplsLocateRequest.CreatedTimestamp = createdTimestamp;
    entities.FplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.FplsLocateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.FplsLocateRequest.RequestSentDate = requestSentDate;
    entities.FplsLocateRequest.SendRequestTo = sendRequestTo;
    entities.FplsLocateRequest.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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

    private FplsLocateRequest fplsLocateRequest;
    private CsePerson csePerson;
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

    private FplsLocateRequest fplsLocateRequest;
    private CsePerson csePerson;
  }
#endregion
}

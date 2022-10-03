// Program: OE_CAB_CREATE_PROACTIVE_MATCH, ID: 373414901, model: 746.
// Short name: SWE01965
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CAB_CREATE_PROACTIVE_MATCH.
/// </summary>
[Serializable]
public partial class OeCabCreateProactiveMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_CREATE_PROACTIVE_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabCreateProactiveMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabCreateProactiveMatch.
  /// </summary>
  public OeCabCreateProactiveMatch(IContext context, Import import,
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
    try
    {
      CreateFcrProactiveMatchResponse();
      export.FcrProactiveMatchResponse.
        Assign(entities.FcrProactiveMatchResponse);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FCR_PROACTIVE_MATCH_RESPONSE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FCR_PROACTIVE_MATCH_RESPONSE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateFcrProactiveMatchResponse()
  {
    var actionTypeCode = import.FcrProactiveMatchResponse.ActionTypeCode ?? "";
    var transmitterStateOrTerrCode =
      import.FcrProactiveMatchResponse.TransmitterStateOrTerrCode ?? "";
    var userField = import.FcrProactiveMatchResponse.UserField ?? "";
    var fipsCountyCode = import.FcrProactiveMatchResponse.FipsCountyCode ?? "";
    var firstName = import.FcrProactiveMatchResponse.FirstName ?? "";
    var middleName = import.FcrProactiveMatchResponse.MiddleName ?? "";
    var submittedOrMatchedSsn =
      import.FcrProactiveMatchResponse.SubmittedOrMatchedSsn ?? "";
    var stateMemberId = import.FcrProactiveMatchResponse.StateMemberId ?? "";
    var submittedCaseId = import.FcrProactiveMatchResponse.SubmittedCaseId ?? ""
      ;
    var responseCode = import.FcrProactiveMatchResponse.ResponseCode ?? "";
    var matchedCaseId = import.FcrProactiveMatchResponse.MatchedCaseId ?? "";
    var matchedCaseType = import.FcrProactiveMatchResponse.MatchedCaseType ?? ""
      ;
    var matchFcrFipsCountyCd =
      import.FcrProactiveMatchResponse.MatchFcrFipsCountyCd ?? "";
    var matchedFcrCaseRegDate =
      import.FcrProactiveMatchResponse.MatchedFcrCaseRegDate;
    var matchedCaseOrderInd =
      import.FcrProactiveMatchResponse.MatchedCaseOrderInd ?? "";
    var matchedParticipantType =
      import.FcrProactiveMatchResponse.MatchedParticipantType ?? "";
    var matchedMemberId = import.FcrProactiveMatchResponse.MatchedMemberId ?? ""
      ;
    var matchedPersonDod = import.FcrProactiveMatchResponse.MatchedPersonDod;
    var matchedPersonAddFirstName1 =
      import.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 ?? "";
    var matchedPersonAddMiddleName1 =
      import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 ?? "";
    var matchedPersonAddLastName1 =
      import.FcrProactiveMatchResponse.MatchedPersonAddLastName1 ?? "";
    var matchedPersonAddFirstName2 =
      import.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 ?? "";
    var matchedPersonAddMiddleName2 =
      import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 ?? "";
    var matchedPersonAddLastName2 =
      import.FcrProactiveMatchResponse.MatchedPersonAddLastName2 ?? "";
    var matchedPersonAddFirstName3 =
      import.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 ?? "";
    var matchedPersonAddMiddleName3 =
      import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 ?? "";
    var matchedPersonAddLastName3 =
      import.FcrProactiveMatchResponse.MatchedPersonAddLastName3 ?? "";
    var matchedPersonAddFirstName4 =
      import.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 ?? "";
    var matchedPersonAddMiddleName4 =
      import.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 ?? "";
    var matchedPersonAddLastName4 =
      import.FcrProactiveMatchResponse.MatchedPersonAddLastName4 ?? "";
    var associatedSsn1 = import.FcrProactiveMatchResponse.AssociatedSsn1 ?? "";
    var associatedFirstName1 =
      import.FcrProactiveMatchResponse.AssociatedFirstName1 ?? "";
    var associatedMiddleName1 =
      import.FcrProactiveMatchResponse.AssociatedMiddleName1 ?? "";
    var associatedLastName1 =
      import.FcrProactiveMatchResponse.AssociatedLastName1 ?? "";
    var associatedPersonSexCode1 =
      import.FcrProactiveMatchResponse.AssociatedPersonSexCode1 ?? "";
    var associatedParticipantType1 =
      import.FcrProactiveMatchResponse.AssociatedParticipantType1 ?? "";
    var associatedOthStTerrMembId1 =
      import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 ?? "";
    var associatedDob1 = import.FcrProactiveMatchResponse.AssociatedDob1;
    var associatedDod1 = import.FcrProactiveMatchResponse.AssociatedDod1;
    var associatedSsn2 = import.FcrProactiveMatchResponse.AssociatedSsn2 ?? "";
    var associatedFirstName2 =
      import.FcrProactiveMatchResponse.AssociatedFirstName2 ?? "";
    var associatedMiddleName2 =
      import.FcrProactiveMatchResponse.AssociatedMiddleName2 ?? "";
    var associatedLastName2 =
      import.FcrProactiveMatchResponse.AssociatedLastName2 ?? "";
    var associatedPersonSexCode2 =
      import.FcrProactiveMatchResponse.AssociatedPersonSexCode2 ?? "";
    var associatedParticipantType2 =
      import.FcrProactiveMatchResponse.AssociatedParticipantType2 ?? "";
    var associatedOthStTerrMembId2 =
      import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 ?? "";
    var associatedDob2 = import.FcrProactiveMatchResponse.AssociatedDob2;
    var associatedDod2 = import.FcrProactiveMatchResponse.AssociatedDod2;
    var associatedSsn3 = import.FcrProactiveMatchResponse.AssociatedSsn3 ?? "";
    var associatedFirstName3 =
      import.FcrProactiveMatchResponse.AssociatedFirstName3 ?? "";
    var associatedMiddleName3 =
      import.FcrProactiveMatchResponse.AssociatedMiddleName3 ?? "";
    var associatedLastName3 =
      import.FcrProactiveMatchResponse.AssociatedLastName3 ?? "";
    var associatedPersonSexCode3 =
      import.FcrProactiveMatchResponse.AssociatedPersonSexCode3 ?? "";
    var associatedParticipantType3 =
      import.FcrProactiveMatchResponse.AssociatedParticipantType3 ?? "";
    var associatedOthStTerrMembId3 =
      import.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 ?? "";
    var associatedDob3 = import.FcrProactiveMatchResponse.AssociatedDob3;
    var associatedDod3 = import.FcrProactiveMatchResponse.AssociatedDod3;
    var associatedStateMembId1 =
      import.FcrProactiveMatchResponse.AssociatedStateMembId1 ?? "";
    var associatedStateMembId2 =
      import.FcrProactiveMatchResponse.AssociatedStateMembId2 ?? "";
    var associatedStateMembId3 =
      import.FcrProactiveMatchResponse.AssociatedStateMembId3 ?? "";
    var identifier = import.FcrProactiveMatchResponse.Identifier;
    var dateReceived = import.FcrProactiveMatchResponse.DateReceived;
    var lastName = import.FcrProactiveMatchResponse.LastName ?? "";

    entities.FcrProactiveMatchResponse.Populated = false;
    Update("CreateFcrProactiveMatchResponse",
      (db, command) =>
      {
        db.SetNullableString(command, "actionTypeCode", actionTypeCode);
        db.SetNullableString(
          command, "tranStateTerrCd", transmitterStateOrTerrCode);
        db.SetNullableString(command, "userField", userField);
        db.SetNullableString(command, "fipsCountyCode", fipsCountyCode);
        db.SetNullableString(command, "firstName", firstName);
        db.SetNullableString(command, "middleName", middleName);
        db.SetNullableString(command, "subOrMatchSsn", submittedOrMatchedSsn);
        db.SetNullableString(command, "stateMemberId", stateMemberId);
        db.SetNullableString(command, "submittedCaseId", submittedCaseId);
        db.SetNullableString(command, "responseCode", responseCode);
        db.SetNullableString(command, "matchedCaseId", matchedCaseId);
        db.SetNullableString(command, "matchedCaseType", matchedCaseType);
        db.SetNullableString(command, "mtchFipsCntCd", matchFcrFipsCountyCd);
        db.SetNullableDate(command, "maCaseRegDate", matchedFcrCaseRegDate);
        db.SetNullableString(command, "maCaseOrdInd", matchedCaseOrderInd);
        db.SetNullableString(command, "maPartType", matchedParticipantType);
        db.SetNullableString(command, "maMemberId", matchedMemberId);
        db.SetNullableDate(command, "maPersonDod", matchedPersonDod);
        db.SetNullableString(command, "maAddFNm1", matchedPersonAddFirstName1);
        db.SetNullableString(command, "maAddMNm1", matchedPersonAddMiddleName1);
        db.SetNullableString(command, "maAdd1Nm1", matchedPersonAddLastName1);
        db.SetNullableString(command, "maAddFNm2", matchedPersonAddFirstName2);
        db.SetNullableString(command, "maAddMNm2", matchedPersonAddMiddleName2);
        db.SetNullableString(command, "maAdd1Nm2", matchedPersonAddLastName2);
        db.SetNullableString(command, "maAddFNm3", matchedPersonAddFirstName3);
        db.SetNullableString(command, "maAddMNm3", matchedPersonAddMiddleName3);
        db.SetNullableString(command, "maAdd1Nm3", matchedPersonAddLastName3);
        db.
          SetNullableString(command, "maAddFName4", matchedPersonAddFirstName4);
          
        db.
          SetNullableString(command, "maAddMName4", matchedPersonAddMiddleName4);
          
        db.SetNullableString(command, "maAdd1Name4", matchedPersonAddLastName4);
        db.SetNullableString(command, "assoSsn1", associatedSsn1);
        db.SetNullableString(command, "assoFirstName1", associatedFirstName1);
        db.SetNullableString(command, "assoMiddleName1", associatedMiddleName1);
        db.SetNullableString(command, "assoLastName1", associatedLastName1);
        db.SetNullableString(command, "assoSex1", associatedPersonSexCode1);
        db.SetNullableString(
          command, "assoPartType1", associatedParticipantType1);
        db.SetNullableString(command, "othStMemb1", associatedOthStTerrMembId1);
        db.SetNullableDate(command, "assoDob1", associatedDob1);
        db.SetNullableDate(command, "assoDod1", associatedDod1);
        db.SetNullableString(command, "assoSsn2", associatedSsn2);
        db.SetNullableString(command, "assoFirstName2", associatedFirstName2);
        db.SetNullableString(command, "assoMiddleName2", associatedMiddleName2);
        db.SetNullableString(command, "assoLastName2", associatedLastName2);
        db.SetNullableString(command, "assoSex2", associatedPersonSexCode2);
        db.SetNullableString(
          command, "assoPartType2", associatedParticipantType2);
        db.SetNullableString(command, "othStMemb2", associatedOthStTerrMembId2);
        db.SetNullableDate(command, "assoDob2", associatedDob2);
        db.SetNullableDate(command, "assoDod2", associatedDod2);
        db.SetNullableString(command, "assoSsn3", associatedSsn3);
        db.SetNullableString(command, "assoFirstName3", associatedFirstName3);
        db.SetNullableString(command, "assoMiddleName3", associatedMiddleName3);
        db.SetNullableString(command, "assoLastName3", associatedLastName3);
        db.SetNullableString(command, "assoSex3", associatedPersonSexCode3);
        db.SetNullableString(
          command, "assoPartType3", associatedParticipantType3);
        db.SetNullableString(command, "othStMemb3", associatedOthStTerrMembId3);
        db.SetNullableDate(command, "assoDob3", associatedDob3);
        db.SetNullableDate(command, "assoDod3", associatedDod3);
        db.SetNullableString(command, "assoStId1", associatedStateMembId1);
        db.SetNullableString(command, "assoStId2", associatedStateMembId2);
        db.SetNullableString(command, "assoStId3", associatedStateMembId3);
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableDate(command, "dateReceived", dateReceived);
        db.SetNullableString(command, "lastName", lastName);
      });

    entities.FcrProactiveMatchResponse.ActionTypeCode = actionTypeCode;
    entities.FcrProactiveMatchResponse.TransmitterStateOrTerrCode =
      transmitterStateOrTerrCode;
    entities.FcrProactiveMatchResponse.UserField = userField;
    entities.FcrProactiveMatchResponse.FipsCountyCode = fipsCountyCode;
    entities.FcrProactiveMatchResponse.FirstName = firstName;
    entities.FcrProactiveMatchResponse.MiddleName = middleName;
    entities.FcrProactiveMatchResponse.SubmittedOrMatchedSsn =
      submittedOrMatchedSsn;
    entities.FcrProactiveMatchResponse.StateMemberId = stateMemberId;
    entities.FcrProactiveMatchResponse.SubmittedCaseId = submittedCaseId;
    entities.FcrProactiveMatchResponse.ResponseCode = responseCode;
    entities.FcrProactiveMatchResponse.MatchedCaseId = matchedCaseId;
    entities.FcrProactiveMatchResponse.MatchedCaseType = matchedCaseType;
    entities.FcrProactiveMatchResponse.MatchFcrFipsCountyCd =
      matchFcrFipsCountyCd;
    entities.FcrProactiveMatchResponse.MatchedFcrCaseRegDate =
      matchedFcrCaseRegDate;
    entities.FcrProactiveMatchResponse.MatchedCaseOrderInd =
      matchedCaseOrderInd;
    entities.FcrProactiveMatchResponse.MatchedParticipantType =
      matchedParticipantType;
    entities.FcrProactiveMatchResponse.MatchedMemberId = matchedMemberId;
    entities.FcrProactiveMatchResponse.MatchedPersonDod = matchedPersonDod;
    entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName1 =
      matchedPersonAddFirstName1;
    entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName1 =
      matchedPersonAddMiddleName1;
    entities.FcrProactiveMatchResponse.MatchedPersonAddLastName1 =
      matchedPersonAddLastName1;
    entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName2 =
      matchedPersonAddFirstName2;
    entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName2 =
      matchedPersonAddMiddleName2;
    entities.FcrProactiveMatchResponse.MatchedPersonAddLastName2 =
      matchedPersonAddLastName2;
    entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName3 =
      matchedPersonAddFirstName3;
    entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName3 =
      matchedPersonAddMiddleName3;
    entities.FcrProactiveMatchResponse.MatchedPersonAddLastName3 =
      matchedPersonAddLastName3;
    entities.FcrProactiveMatchResponse.MatchedPersonAddFirstName4 =
      matchedPersonAddFirstName4;
    entities.FcrProactiveMatchResponse.MatchedPersonAddMiddleName4 =
      matchedPersonAddMiddleName4;
    entities.FcrProactiveMatchResponse.MatchedPersonAddLastName4 =
      matchedPersonAddLastName4;
    entities.FcrProactiveMatchResponse.AssociatedSsn1 = associatedSsn1;
    entities.FcrProactiveMatchResponse.AssociatedFirstName1 =
      associatedFirstName1;
    entities.FcrProactiveMatchResponse.AssociatedMiddleName1 =
      associatedMiddleName1;
    entities.FcrProactiveMatchResponse.AssociatedLastName1 =
      associatedLastName1;
    entities.FcrProactiveMatchResponse.AssociatedPersonSexCode1 =
      associatedPersonSexCode1;
    entities.FcrProactiveMatchResponse.AssociatedParticipantType1 =
      associatedParticipantType1;
    entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId1 =
      associatedOthStTerrMembId1;
    entities.FcrProactiveMatchResponse.AssociatedDob1 = associatedDob1;
    entities.FcrProactiveMatchResponse.AssociatedDod1 = associatedDod1;
    entities.FcrProactiveMatchResponse.AssociatedSsn2 = associatedSsn2;
    entities.FcrProactiveMatchResponse.AssociatedFirstName2 =
      associatedFirstName2;
    entities.FcrProactiveMatchResponse.AssociatedMiddleName2 =
      associatedMiddleName2;
    entities.FcrProactiveMatchResponse.AssociatedLastName2 =
      associatedLastName2;
    entities.FcrProactiveMatchResponse.AssociatedPersonSexCode2 =
      associatedPersonSexCode2;
    entities.FcrProactiveMatchResponse.AssociatedParticipantType2 =
      associatedParticipantType2;
    entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId2 =
      associatedOthStTerrMembId2;
    entities.FcrProactiveMatchResponse.AssociatedDob2 = associatedDob2;
    entities.FcrProactiveMatchResponse.AssociatedDod2 = associatedDod2;
    entities.FcrProactiveMatchResponse.AssociatedSsn3 = associatedSsn3;
    entities.FcrProactiveMatchResponse.AssociatedFirstName3 =
      associatedFirstName3;
    entities.FcrProactiveMatchResponse.AssociatedMiddleName3 =
      associatedMiddleName3;
    entities.FcrProactiveMatchResponse.AssociatedLastName3 =
      associatedLastName3;
    entities.FcrProactiveMatchResponse.AssociatedPersonSexCode3 =
      associatedPersonSexCode3;
    entities.FcrProactiveMatchResponse.AssociatedParticipantType3 =
      associatedParticipantType3;
    entities.FcrProactiveMatchResponse.AssociatedOthStTerrMembId3 =
      associatedOthStTerrMembId3;
    entities.FcrProactiveMatchResponse.AssociatedDob3 = associatedDob3;
    entities.FcrProactiveMatchResponse.AssociatedDod3 = associatedDod3;
    entities.FcrProactiveMatchResponse.AssociatedStateMembId1 =
      associatedStateMembId1;
    entities.FcrProactiveMatchResponse.AssociatedStateMembId2 =
      associatedStateMembId2;
    entities.FcrProactiveMatchResponse.AssociatedStateMembId3 =
      associatedStateMembId3;
    entities.FcrProactiveMatchResponse.Identifier = identifier;
    entities.FcrProactiveMatchResponse.DateReceived = dateReceived;
    entities.FcrProactiveMatchResponse.LastName = lastName;
    entities.FcrProactiveMatchResponse.Populated = true;
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
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
  }
#endregion
}

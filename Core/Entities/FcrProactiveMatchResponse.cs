// The source file: FCR_PROACTIVE_MATCH_RESPONSE, ID: 373539154, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:    OBLGE
/// 
/// This entity contains infromation sent to the state or territory in response
/// to an Input Query Record. Entity type also gets created when a person is
/// registered by multiple States or territories that include the State or
/// territory and another state or territory is adding or changing information
/// about the person on FCR. This privides the submitter with information
/// regarding the cases and associated participants on those cases for the
/// person.
/// </summary>
[Serializable]
public partial class FcrProactiveMatchResponse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrProactiveMatchResponse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrProactiveMatchResponse(FcrProactiveMatchResponse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrProactiveMatchResponse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrProactiveMatchResponse that)
  {
    base.Assign(that);
    actionTypeCode = that.actionTypeCode;
    transmitterStateOrTerrCode = that.transmitterStateOrTerrCode;
    userField = that.userField;
    fipsCountyCode = that.fipsCountyCode;
    firstName = that.firstName;
    middleName = that.middleName;
    submittedOrMatchedSsn = that.submittedOrMatchedSsn;
    stateMemberId = that.stateMemberId;
    submittedCaseId = that.submittedCaseId;
    responseCode = that.responseCode;
    matchedCaseId = that.matchedCaseId;
    matchedCaseType = that.matchedCaseType;
    matchFcrFipsCountyCd = that.matchFcrFipsCountyCd;
    matchedFcrCaseRegDate = that.matchedFcrCaseRegDate;
    matchedCaseOrderInd = that.matchedCaseOrderInd;
    matchedParticipantType = that.matchedParticipantType;
    matchedMemberId = that.matchedMemberId;
    matchedPersonDod = that.matchedPersonDod;
    matchedPersonAddFirstName1 = that.matchedPersonAddFirstName1;
    matchedPersonAddMiddleName1 = that.matchedPersonAddMiddleName1;
    matchedPersonAddLastName1 = that.matchedPersonAddLastName1;
    matchedPersonAddFirstName2 = that.matchedPersonAddFirstName2;
    matchedPersonAddMiddleName2 = that.matchedPersonAddMiddleName2;
    matchedPersonAddLastName2 = that.matchedPersonAddLastName2;
    matchedPersonAddFirstName3 = that.matchedPersonAddFirstName3;
    matchedPersonAddMiddleName3 = that.matchedPersonAddMiddleName3;
    matchedPersonAddLastName3 = that.matchedPersonAddLastName3;
    matchedPersonAddFirstName4 = that.matchedPersonAddFirstName4;
    matchedPersonAddMiddleName4 = that.matchedPersonAddMiddleName4;
    matchedPersonAddLastName4 = that.matchedPersonAddLastName4;
    associatedSsn1 = that.associatedSsn1;
    associatedFirstName1 = that.associatedFirstName1;
    associatedMiddleName1 = that.associatedMiddleName1;
    associatedLastName1 = that.associatedLastName1;
    associatedPersonSexCode1 = that.associatedPersonSexCode1;
    associatedParticipantType1 = that.associatedParticipantType1;
    associatedOthStTerrMembId1 = that.associatedOthStTerrMembId1;
    associatedDob1 = that.associatedDob1;
    associatedDod1 = that.associatedDod1;
    associatedSsn2 = that.associatedSsn2;
    associatedFirstName2 = that.associatedFirstName2;
    associatedMiddleName2 = that.associatedMiddleName2;
    associatedLastName2 = that.associatedLastName2;
    associatedPersonSexCode2 = that.associatedPersonSexCode2;
    associatedParticipantType2 = that.associatedParticipantType2;
    associatedOthStTerrMembId2 = that.associatedOthStTerrMembId2;
    associatedDob2 = that.associatedDob2;
    associatedDod2 = that.associatedDod2;
    associatedSsn3 = that.associatedSsn3;
    associatedFirstName3 = that.associatedFirstName3;
    associatedMiddleName3 = that.associatedMiddleName3;
    associatedLastName3 = that.associatedLastName3;
    associatedPersonSexCode3 = that.associatedPersonSexCode3;
    associatedParticipantType3 = that.associatedParticipantType3;
    associatedOthStTerrMembId3 = that.associatedOthStTerrMembId3;
    associatedDob3 = that.associatedDob3;
    associatedDod3 = that.associatedDod3;
    associatedStateMembId1 = that.associatedStateMembId1;
    associatedStateMembId2 = that.associatedStateMembId2;
    associatedStateMembId3 = that.associatedStateMembId3;
    identifier = that.identifier;
    dateReceived = that.dateReceived;
    lastName = that.lastName;
  }

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// This field will contain one of the following codes to indicate the action 
  /// that initiated the generation of this record.              C-Proactive FCR
  /// response for case change from non IV-D to IV-D.
  /// 
  /// F-FCR query response.
  /// P-Proactive
  /// FCR response for a new person or a change to an existing person.
  /// </summary>
  [JsonPropertyName("actionTypeCode")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ActionTypeCode_MaxLength, Optional = true)]
  public string ActionTypeCode
  {
    get => actionTypeCode;
    set => actionTypeCode = value != null
      ? TrimEnd(Substring(value, 1, ActionTypeCode_MaxLength)) : null;
  }

  /// <summary>Length of the TRANSMITTER_STATE_OR_TERR_CODE attribute.</summary>
  public const int TransmitterStateOrTerrCode_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSMITTER_STATE_OR_TERR_CODE attribute.
  /// This attribute contains the numeric 
  /// FIPS abbreviation Code for the
  /// submitted if the action type is 'F'.
  /// 
  /// This attribute also will contain the
  /// numeric FIPS Abbreviation Code for
  /// the State or Territory receiving the
  /// proactive response if the action
  /// type code equals 'C' or 'P'.
  /// </summary>
  [JsonPropertyName("transmitterStateOrTerrCode")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = TransmitterStateOrTerrCode_MaxLength, Optional = true)]
  public string TransmitterStateOrTerrCode
  {
    get => transmitterStateOrTerrCode;
    set => transmitterStateOrTerrCode = value != null
      ? TrimEnd(Substring(value, 1, TransmitterStateOrTerrCode_MaxLength)) : null
      ;
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 8;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// If the action type is 'F', this attribute will contain the information 
  /// submitted on the FCR Input Query Record. If the action type is 'C' or 'P',
  /// this attribute will contain spaces or User Field on the case for the
  /// person located on the FCR.
  /// </summary>
  [JsonPropertyName("userField")]
  [Member(Index = 3, Type = MemberType.Char, Length = UserField_MaxLength, Optional
    = true)]
  public string UserField
  {
    get => userField;
    set => userField = value != null
      ? TrimEnd(Substring(value, 1, UserField_MaxLength)) : null;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// If the action type is 'F', this attribute will contain the information 
  /// submitted on the FCR Input Query Record.          If the action type is '
  /// C' or 'P', this attribute will contain spaces or FIPS county code on the
  /// case for the person located on the FCR.
  /// </summary>
  [JsonPropertyName("fipsCountyCode")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = FipsCountyCode_MaxLength, Optional = true)]
  public string FipsCountyCode
  {
    get => fipsCountyCode;
    set => fipsCountyCode = value != null
      ? TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// This attribute contains the first name stored for the person located on 
  /// the FCR.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 5, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// This attribute contains the middle name stored for the person located on 
  /// the FCR.
  /// </summary>
  [JsonPropertyName("middleName")]
  [Member(Index = 6, Type = MemberType.Char, Length = MiddleName_MaxLength, Optional
    = true)]
  public string MiddleName
  {
    get => middleName;
    set => middleName = value != null
      ? TrimEnd(Substring(value, 1, MiddleName_MaxLength)) : null;
  }

  /// <summary>Length of the SUBMITTED_OR_MATCHED_SSN attribute.</summary>
  public const int SubmittedOrMatchedSsn_MaxLength = 9;

  /// <summary>
  /// The value of the SUBMITTED_OR_MATCHED_SSN attribute.
  /// Contains either the SSN retrieved, if no SSN was submitted. This field 
  /// will contain the SSN used in the proactive matching process for Action
  /// Type Codes 'C' or 'P' records.
  /// </summary>
  [JsonPropertyName("submittedOrMatchedSsn")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = SubmittedOrMatchedSsn_MaxLength, Optional = true)]
  public string SubmittedOrMatchedSsn
  {
    get => submittedOrMatchedSsn;
    set => submittedOrMatchedSsn = value != null
      ? TrimEnd(Substring(value, 1, SubmittedOrMatchedSsn_MaxLength)) : null;
  }

  /// <summary>Length of the STATE_MEMBER_ID attribute.</summary>
  public const int StateMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the STATE_MEMBER_ID attribute.
  /// This attribute contains the Member ID submitted by the State or territory 
  /// in the FCR Input query Record or the member ID retrieved, if no Member ID
  /// was submitted. This attribute contains receiving states Member ID for the
  /// proactive matched person.
  /// </summary>
  [JsonPropertyName("stateMemberId")]
  [Member(Index = 8, Type = MemberType.Char, Length = StateMemberId_MaxLength, Optional
    = true)]
  public string StateMemberId
  {
    get => stateMemberId;
    set => stateMemberId = value != null
      ? TrimEnd(Substring(value, 1, StateMemberId_MaxLength)) : null;
  }

  /// <summary>Length of the SUBMITTED_CASE_ID attribute.</summary>
  public const int SubmittedCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the SUBMITTED_CASE_ID attribute.
  /// For case type 'F', this contains case-id, for proactive match this 
  /// attribute contains receiving state's case ID.
  /// </summary>
  [JsonPropertyName("submittedCaseId")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = SubmittedCaseId_MaxLength, Optional = true)]
  public string SubmittedCaseId
  {
    get => submittedCaseId;
    set => submittedCaseId = value != null
      ? TrimEnd(Substring(value, 1, SubmittedCaseId_MaxLength)) : null;
  }

  /// <summary>Length of the RESPONSE_CODE attribute.</summary>
  public const int ResponseCode_MaxLength = 2;

  /// <summary>
  /// The value of the RESPONSE_CODE attribute.
  /// It contains following codes.
  /// 
  /// MA-Match was made to one or more cases on the FCR for
  /// the person and one to three persons were associated
  /// with the matched case. MM-Match was made to one or
  /// more cases on the FCR for the person and more than
  /// three persons were associated with the matched case.
  /// </summary>
  [JsonPropertyName("responseCode")]
  [Member(Index = 10, Type = MemberType.Char, Length = ResponseCode_MaxLength, Optional
    = true)]
  public string ResponseCode
  {
    get => responseCode;
    set => responseCode = value != null
      ? TrimEnd(Substring(value, 1, ResponseCode_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_CASE_ID attribute.</summary>
  public const int MatchedCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the MATCHED_CASE_ID attribute.
  /// This attribute contains state's or territory's Case ID for the person 
  /// matched on the FCR.
  /// </summary>
  [JsonPropertyName("matchedCaseId")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = MatchedCaseId_MaxLength, Optional = true)]
  public string MatchedCaseId
  {
    get => matchedCaseId;
    set => matchedCaseId = value != null
      ? TrimEnd(Substring(value, 1, MatchedCaseId_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_CASE_TYPE attribute.</summary>
  public const int MatchedCaseType_MaxLength = 1;

  /// <summary>
  /// The value of the MATCHED_CASE_TYPE attribute.
  /// It contains following codes.
  /// F-IV-D cases.
  /// 
  /// N-Non IV-D cases.
  /// </summary>
  [JsonPropertyName("matchedCaseType")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = MatchedCaseType_MaxLength, Optional = true)]
  public string MatchedCaseType
  {
    get => matchedCaseType;
    set => matchedCaseType = value != null
      ? TrimEnd(Substring(value, 1, MatchedCaseType_MaxLength)) : null;
  }

  /// <summary>Length of the MATCH_FCR_FIPS_COUNTY_CD attribute.</summary>
  public const int MatchFcrFipsCountyCd_MaxLength = 3;

  /// <summary>
  /// The value of the MATCH_FCR_FIPS_COUNTY_CD attribute.
  /// Last three numeric positions of the FIPS County code associated with 
  /// matched case on the FCR.
  /// </summary>
  [JsonPropertyName("matchFcrFipsCountyCd")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = MatchFcrFipsCountyCd_MaxLength, Optional = true)]
  public string MatchFcrFipsCountyCd
  {
    get => matchFcrFipsCountyCd;
    set => matchFcrFipsCountyCd = value != null
      ? TrimEnd(Substring(value, 1, MatchFcrFipsCountyCd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MATCHED_FCR_CASE_REG_DATE attribute.
  /// Contains the date the matched date was added to the FCR.
  /// </summary>
  [JsonPropertyName("matchedFcrCaseRegDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? MatchedFcrCaseRegDate
  {
    get => matchedFcrCaseRegDate;
    set => matchedFcrCaseRegDate = value;
  }

  /// <summary>Length of the MATCHED_CASE_ORDER_IND attribute.</summary>
  public const int MatchedCaseOrderInd_MaxLength = 1;

  /// <summary>
  /// The value of the MATCHED_CASE_ORDER_IND attribute.
  /// This attribute contains value of the order indicator stored on the FCR for
  /// the matched case.
  /// </summary>
  [JsonPropertyName("matchedCaseOrderInd")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = MatchedCaseOrderInd_MaxLength, Optional = true)]
  public string MatchedCaseOrderInd
  {
    get => matchedCaseOrderInd;
    set => matchedCaseOrderInd = value != null
      ? TrimEnd(Substring(value, 1, MatchedCaseOrderInd_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_PARTICIPANT_TYPE attribute.</summary>
  public const int MatchedParticipantType_MaxLength = 2;

  /// <summary>
  /// The value of the MATCHED_PARTICIPANT_TYPE attribute.
  /// It contains following codes.
  /// CH-child
  /// 
  /// AR-Custodial party
  /// AP-Non-
  /// Custodial Parent
  /// PF-Putative Father.
  /// </summary>
  [JsonPropertyName("matchedParticipantType")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = MatchedParticipantType_MaxLength, Optional = true)]
  public string MatchedParticipantType
  {
    get => matchedParticipantType;
    set => matchedParticipantType = value != null
      ? TrimEnd(Substring(value, 1, MatchedParticipantType_MaxLength)) : null;
  }

  /// <summary>Length of the MATCHED_MEMBER_ID attribute.</summary>
  public const int MatchedMemberId_MaxLength = 15;

  /// <summary>
  /// The value of the MATCHED_MEMBER_ID attribute.
  /// This attribute contains the Member id assigned by the state or territory 
  /// that added the person to this case on the FCR.
  /// </summary>
  [JsonPropertyName("matchedMemberId")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = MatchedMemberId_MaxLength, Optional = true)]
  public string MatchedMemberId
  {
    get => matchedMemberId;
    set => matchedMemberId = value != null
      ? TrimEnd(Substring(value, 1, MatchedMemberId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MATCHED_PERSON_DOD attribute.
  /// This attribute is reserved for the future.
  /// </summary>
  [JsonPropertyName("matchedPersonDod")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? MatchedPersonDod
  {
    get => matchedPersonDod;
    set => matchedPersonDod = value;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_FIRST_NAME1 attribute.</summary>
  public const int MatchedPersonAddFirstName1_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_FIRST_NAME1 attribute.
  /// This contains the first additional first name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddFirstName1")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = MatchedPersonAddFirstName1_MaxLength, Optional = true)]
  public string MatchedPersonAddFirstName1
  {
    get => matchedPersonAddFirstName1;
    set => matchedPersonAddFirstName1 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddFirstName1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_MIDDLE_NAME_1 attribute.
  /// </summary>
  public const int MatchedPersonAddMiddleName1_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_MIDDLE_NAME_1 attribute.
  /// This contains the first additional middle name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddMiddleName1")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = MatchedPersonAddMiddleName1_MaxLength, Optional = true)]
  public string MatchedPersonAddMiddleName1
  {
    get => matchedPersonAddMiddleName1;
    set => matchedPersonAddMiddleName1 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddMiddleName1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_LAST_NAME_1 attribute.</summary>
  public const int MatchedPersonAddLastName1_MaxLength = 30;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_LAST_NAME_1 attribute.
  /// This contains the first additional last name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddLastName1")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = MatchedPersonAddLastName1_MaxLength, Optional = true)]
  public string MatchedPersonAddLastName1
  {
    get => matchedPersonAddLastName1;
    set => matchedPersonAddLastName1 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddLastName1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_FIRST_NAME2 attribute.</summary>
  public const int MatchedPersonAddFirstName2_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_FIRST_NAME2 attribute.
  /// This contains the second additional first name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddFirstName2")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = MatchedPersonAddFirstName2_MaxLength, Optional = true)]
  public string MatchedPersonAddFirstName2
  {
    get => matchedPersonAddFirstName2;
    set => matchedPersonAddFirstName2 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddFirstName2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_MIDDLE_NAME_2 attribute.
  /// </summary>
  public const int MatchedPersonAddMiddleName2_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_MIDDLE_NAME_2 attribute.
  /// This contains the second additional middle name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddMiddleName2")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = MatchedPersonAddMiddleName2_MaxLength, Optional = true)]
  public string MatchedPersonAddMiddleName2
  {
    get => matchedPersonAddMiddleName2;
    set => matchedPersonAddMiddleName2 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddMiddleName2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_LAST_NAME_2 attribute.</summary>
  public const int MatchedPersonAddLastName2_MaxLength = 30;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_LAST_NAME_2 attribute.
  /// This contains the second additional last name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddLastName2")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = MatchedPersonAddLastName2_MaxLength, Optional = true)]
  public string MatchedPersonAddLastName2
  {
    get => matchedPersonAddLastName2;
    set => matchedPersonAddLastName2 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddLastName2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_FIRST_NAME3 attribute.</summary>
  public const int MatchedPersonAddFirstName3_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_FIRST_NAME3 attribute.
  /// This contains the third additional first name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddFirstName3")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = MatchedPersonAddFirstName3_MaxLength, Optional = true)]
  public string MatchedPersonAddFirstName3
  {
    get => matchedPersonAddFirstName3;
    set => matchedPersonAddFirstName3 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddFirstName3_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_MIDDLE_NAME_3 attribute.
  /// </summary>
  public const int MatchedPersonAddMiddleName3_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_MIDDLE_NAME_3 attribute.
  /// This contains the third additional middle name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddMiddleName3")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = MatchedPersonAddMiddleName3_MaxLength, Optional = true)]
  public string MatchedPersonAddMiddleName3
  {
    get => matchedPersonAddMiddleName3;
    set => matchedPersonAddMiddleName3 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddMiddleName3_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_LAST_NAME_3 attribute.</summary>
  public const int MatchedPersonAddLastName3_MaxLength = 30;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_LAST_NAME_3 attribute.
  /// This contains the third additional last name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddLastName3")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = MatchedPersonAddLastName3_MaxLength, Optional = true)]
  public string MatchedPersonAddLastName3
  {
    get => matchedPersonAddLastName3;
    set => matchedPersonAddLastName3 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddLastName3_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_FIRST_NAME4 attribute.</summary>
  public const int MatchedPersonAddFirstName4_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_FIRST_NAME4 attribute.
  /// This contains the fourth additional first name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddFirstName4")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = MatchedPersonAddFirstName4_MaxLength, Optional = true)]
  public string MatchedPersonAddFirstName4
  {
    get => matchedPersonAddFirstName4;
    set => matchedPersonAddFirstName4 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddFirstName4_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_MIDDLE_NAME_4 attribute.
  /// </summary>
  public const int MatchedPersonAddMiddleName4_MaxLength = 16;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_MIDDLE_NAME_4 attribute.
  /// This contains the fourth additional middle name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddMiddleName4")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = MatchedPersonAddMiddleName4_MaxLength, Optional = true)]
  public string MatchedPersonAddMiddleName4
  {
    get => matchedPersonAddMiddleName4;
    set => matchedPersonAddMiddleName4 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddMiddleName4_MaxLength)) : null
      ;
  }

  /// <summary>Length of the MATCHED_PERSON_ADD_LAST_NAME_4 attribute.</summary>
  public const int MatchedPersonAddLastName4_MaxLength = 30;

  /// <summary>
  /// The value of the MATCHED_PERSON_ADD_LAST_NAME_4 attribute.
  /// This contains the fourth additional last name stored on FCR for this 
  /// person. If there is no information in FCR this attribute will have spaces.
  /// </summary>
  [JsonPropertyName("matchedPersonAddLastName4")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = MatchedPersonAddLastName4_MaxLength, Optional = true)]
  public string MatchedPersonAddLastName4
  {
    get => matchedPersonAddLastName4;
    set => matchedPersonAddLastName4 = value != null
      ? TrimEnd(Substring(value, 1, MatchedPersonAddLastName4_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_SSN_1 attribute.</summary>
  public const int AssociatedSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the ASSOCIATED_SSN_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// verified SSN of the associated person.
  /// </summary>
  [JsonPropertyName("associatedSsn1")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = AssociatedSsn1_MaxLength, Optional = true)]
  public string AssociatedSsn1
  {
    get => associatedSsn1;
    set => associatedSsn1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedSsn1_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_FIRST_NAME_1 attribute.</summary>
  public const int AssociatedFirstName1_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_FIRST_NAME_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// first of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedFirstName1")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = AssociatedFirstName1_MaxLength, Optional = true)]
  public string AssociatedFirstName1
  {
    get => associatedFirstName1;
    set => associatedFirstName1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedFirstName1_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_MIDDLE_NAME_1 attribute.</summary>
  public const int AssociatedMiddleName1_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_MIDDLE_NAME_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Middle of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedMiddleName1")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = AssociatedMiddleName1_MaxLength, Optional = true)]
  public string AssociatedMiddleName1
  {
    get => associatedMiddleName1;
    set => associatedMiddleName1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedMiddleName1_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_LAST_NAME_1 attribute.</summary>
  public const int AssociatedLastName1_MaxLength = 30;

  /// <summary>
  /// The value of the ASSOCIATED_LAST_NAME_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Last name of the associated person 1.
  /// </summary>
  [JsonPropertyName("associatedLastName1")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = AssociatedLastName1_MaxLength, Optional = true)]
  public string AssociatedLastName1
  {
    get => associatedLastName1;
    set => associatedLastName1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedLastName1_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_PERSON_SEX_CODE_1 attribute.</summary>
  public const int AssociatedPersonSexCode1_MaxLength = 1;

  /// <summary>
  /// The value of the ASSOCIATED_PERSON_SEX_CODE_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// sex code of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedPersonSexCode1")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = AssociatedPersonSexCode1_MaxLength, Optional = true)]
  public string AssociatedPersonSexCode1
  {
    get => associatedPersonSexCode1;
    set => associatedPersonSexCode1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedPersonSexCode1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_PARTICIPANT_TYPE_1 attribute.</summary>
  public const int AssociatedParticipantType1_MaxLength = 2;

  /// <summary>
  /// The value of the ASSOCIATED_PARTICIPANT_TYPE_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// participant type of the associated person 1.
  /// </summary>
  [JsonPropertyName("associatedParticipantType1")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = AssociatedParticipantType1_MaxLength, Optional = true)]
  public string AssociatedParticipantType1
  {
    get => associatedParticipantType1;
    set => associatedParticipantType1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedParticipantType1_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_1 attribute.
  /// </summary>
  public const int AssociatedOthStTerrMembId1_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// member id assigned by other state of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedOthStTerrMembId1")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = AssociatedOthStTerrMembId1_MaxLength, Optional = true)]
  public string AssociatedOthStTerrMembId1
  {
    get => associatedOthStTerrMembId1;
    set => associatedOthStTerrMembId1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedOthStTerrMembId1_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOB_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Date of birth of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedDob1")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDob1
  {
    get => associatedDob1;
    set => associatedDob1 = value;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOD_1 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// date of Death of the associated person1.
  /// </summary>
  [JsonPropertyName("associatedDod1")]
  [Member(Index = 39, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDod1
  {
    get => associatedDod1;
    set => associatedDod1 = value;
  }

  /// <summary>Length of the ASSOCIATED_SSN_2 attribute.</summary>
  public const int AssociatedSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the ASSOCIATED_SSN_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// verified SSN of the associated person2.
  /// </summary>
  [JsonPropertyName("associatedSsn2")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = AssociatedSsn2_MaxLength, Optional = true)]
  public string AssociatedSsn2
  {
    get => associatedSsn2;
    set => associatedSsn2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedSsn2_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_FIRST_NAME_2 attribute.</summary>
  public const int AssociatedFirstName2_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_FIRST_NAME_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// first of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedFirstName2")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = AssociatedFirstName2_MaxLength, Optional = true)]
  public string AssociatedFirstName2
  {
    get => associatedFirstName2;
    set => associatedFirstName2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedFirstName2_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_MIDDLE_NAME_2 attribute.</summary>
  public const int AssociatedMiddleName2_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_MIDDLE_NAME_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// middle of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedMiddleName2")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = AssociatedMiddleName2_MaxLength, Optional = true)]
  public string AssociatedMiddleName2
  {
    get => associatedMiddleName2;
    set => associatedMiddleName2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedMiddleName2_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_LAST_NAME_2 attribute.</summary>
  public const int AssociatedLastName2_MaxLength = 30;

  /// <summary>
  /// The value of the ASSOCIATED_LAST_NAME_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// last name of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedLastName2")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = AssociatedLastName2_MaxLength, Optional = true)]
  public string AssociatedLastName2
  {
    get => associatedLastName2;
    set => associatedLastName2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedLastName2_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_PERSON_SEX_CODE_2 attribute.</summary>
  public const int AssociatedPersonSexCode2_MaxLength = 1;

  /// <summary>
  /// The value of the ASSOCIATED_PERSON_SEX_CODE_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// sex code of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedPersonSexCode2")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = AssociatedPersonSexCode2_MaxLength, Optional = true)]
  public string AssociatedPersonSexCode2
  {
    get => associatedPersonSexCode2;
    set => associatedPersonSexCode2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedPersonSexCode2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_PARTICIPANT_TYPE_2 attribute.</summary>
  public const int AssociatedParticipantType2_MaxLength = 2;

  /// <summary>
  /// The value of the ASSOCIATED_PARTICIPANT_TYPE_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// participant type of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedParticipantType2")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = AssociatedParticipantType2_MaxLength, Optional = true)]
  public string AssociatedParticipantType2
  {
    get => associatedParticipantType2;
    set => associatedParticipantType2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedParticipantType2_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_2 attribute.
  /// </summary>
  public const int AssociatedOthStTerrMembId2_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// member id assigned by the state, of the associated person2.
  /// </summary>
  [JsonPropertyName("associatedOthStTerrMembId2")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = AssociatedOthStTerrMembId2_MaxLength, Optional = true)]
  public string AssociatedOthStTerrMembId2
  {
    get => associatedOthStTerrMembId2;
    set => associatedOthStTerrMembId2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedOthStTerrMembId2_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOB_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Date of birth of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedDob2")]
  [Member(Index = 47, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDob2
  {
    get => associatedDob2;
    set => associatedDob2 = value;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOD_2 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Date of Death of the associated person 2.
  /// </summary>
  [JsonPropertyName("associatedDod2")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDod2
  {
    get => associatedDod2;
    set => associatedDod2 = value;
  }

  /// <summary>Length of the ASSOCIATED_SSN_3 attribute.</summary>
  public const int AssociatedSsn3_MaxLength = 9;

  /// <summary>
  /// The value of the ASSOCIATED_SSN_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// verified SSN of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedSsn3")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = AssociatedSsn3_MaxLength, Optional = true)]
  public string AssociatedSsn3
  {
    get => associatedSsn3;
    set => associatedSsn3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedSsn3_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_FIRST_NAME_3 attribute.</summary>
  public const int AssociatedFirstName3_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_FIRST_NAME_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// first of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedFirstName3")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = AssociatedFirstName3_MaxLength, Optional = true)]
  public string AssociatedFirstName3
  {
    get => associatedFirstName3;
    set => associatedFirstName3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedFirstName3_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_MIDDLE_NAME_3 attribute.</summary>
  public const int AssociatedMiddleName3_MaxLength = 16;

  /// <summary>
  /// The value of the ASSOCIATED_MIDDLE_NAME_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// middle name of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedMiddleName3")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = AssociatedMiddleName3_MaxLength, Optional = true)]
  public string AssociatedMiddleName3
  {
    get => associatedMiddleName3;
    set => associatedMiddleName3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedMiddleName3_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_LAST_NAME_3 attribute.</summary>
  public const int AssociatedLastName3_MaxLength = 30;

  /// <summary>
  /// The value of the ASSOCIATED_LAST_NAME_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Last name of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedLastName3")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = AssociatedLastName3_MaxLength, Optional = true)]
  public string AssociatedLastName3
  {
    get => associatedLastName3;
    set => associatedLastName3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedLastName3_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_PERSON_SEX_CODE_3 attribute.</summary>
  public const int AssociatedPersonSexCode3_MaxLength = 1;

  /// <summary>
  /// The value of the ASSOCIATED_PERSON_SEX_CODE_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// sex code of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedPersonSexCode3")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = AssociatedPersonSexCode3_MaxLength, Optional = true)]
  public string AssociatedPersonSexCode3
  {
    get => associatedPersonSexCode3;
    set => associatedPersonSexCode3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedPersonSexCode3_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_PARTICIPANT_TYPE_3 attribute.</summary>
  public const int AssociatedParticipantType3_MaxLength = 2;

  /// <summary>
  /// The value of the ASSOCIATED_PARTICIPANT_TYPE_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// participant type of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedParticipantType3")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = AssociatedParticipantType3_MaxLength, Optional = true)]
  public string AssociatedParticipantType3
  {
    get => associatedParticipantType3;
    set => associatedParticipantType3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedParticipantType3_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_3 attribute.
  /// </summary>
  public const int AssociatedOthStTerrMembId3_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_OTH_ST_TERR_MEMB_ID_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// member id assigned by the state, of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedOthStTerrMembId3")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = AssociatedOthStTerrMembId3_MaxLength, Optional = true)]
  public string AssociatedOthStTerrMembId3
  {
    get => associatedOthStTerrMembId3;
    set => associatedOthStTerrMembId3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedOthStTerrMembId3_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOB_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Date Of birth of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedDob3")]
  [Member(Index = 56, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDob3
  {
    get => associatedDob3;
    set => associatedDob3 = value;
  }

  /// <summary>
  /// The value of the ASSOCIATED_DOD_3 attribute.
  /// If there is an associated person in the matched case, this field will have
  /// Date of Death of the associated person 3.
  /// </summary>
  [JsonPropertyName("associatedDod3")]
  [Member(Index = 57, Type = MemberType.Date, Optional = true)]
  public DateTime? AssociatedDod3
  {
    get => associatedDod3;
    set => associatedDod3 = value;
  }

  /// <summary>Length of the ASSOCIATED_STATE_MEMB_ID_1 attribute.</summary>
  public const int AssociatedStateMembId1_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_STATE_MEMB_ID_1 attribute.
  /// This attribute will have the receiving State's Member ID assigned to an 
  /// associated person 1 on the FCR matched record.
  /// </summary>
  [JsonPropertyName("associatedStateMembId1")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = AssociatedStateMembId1_MaxLength, Optional = true)]
  public string AssociatedStateMembId1
  {
    get => associatedStateMembId1;
    set => associatedStateMembId1 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedStateMembId1_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_STATE_MEMB_ID_2 attribute.</summary>
  public const int AssociatedStateMembId2_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_STATE_MEMB_ID_2 attribute.
  /// This attribute will have the receiving State's Member ID assigned to an 
  /// associated person 2 on the FCR matched record.
  /// </summary>
  [JsonPropertyName("associatedStateMembId2")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = AssociatedStateMembId2_MaxLength, Optional = true)]
  public string AssociatedStateMembId2
  {
    get => associatedStateMembId2;
    set => associatedStateMembId2 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedStateMembId2_MaxLength)) : null;
  }

  /// <summary>Length of the ASSOCIATED_STATE_MEMB_ID_3 attribute.</summary>
  public const int AssociatedStateMembId3_MaxLength = 15;

  /// <summary>
  /// The value of the ASSOCIATED_STATE_MEMB_ID_3 attribute.
  /// This attribute will have the receiving State's Member ID assigned to an 
  /// associated person 3 on the FCR matched record.
  /// </summary>
  [JsonPropertyName("associatedStateMembId3")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = AssociatedStateMembId3_MaxLength, Optional = true)]
  public string AssociatedStateMembId3
  {
    get => associatedStateMembId3;
    set => associatedStateMembId3 = value != null
      ? TrimEnd(Substring(value, 1, AssociatedStateMembId3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute is used as the primary key of the entity.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 61, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the DATE_RECEIVED attribute.
  /// This attribute contains the date on which we received information from 
  /// FCR.
  /// </summary>
  [JsonPropertyName("dateReceived")]
  [Member(Index = 62, Type = MemberType.Date, Optional = true)]
  public DateTime? DateReceived
  {
    get => dateReceived;
    set => dateReceived = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// This attribute contains the last name stored for the person located on the
  /// FCR.
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 63, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  private string actionTypeCode;
  private string transmitterStateOrTerrCode;
  private string userField;
  private string fipsCountyCode;
  private string firstName;
  private string middleName;
  private string submittedOrMatchedSsn;
  private string stateMemberId;
  private string submittedCaseId;
  private string responseCode;
  private string matchedCaseId;
  private string matchedCaseType;
  private string matchFcrFipsCountyCd;
  private DateTime? matchedFcrCaseRegDate;
  private string matchedCaseOrderInd;
  private string matchedParticipantType;
  private string matchedMemberId;
  private DateTime? matchedPersonDod;
  private string matchedPersonAddFirstName1;
  private string matchedPersonAddMiddleName1;
  private string matchedPersonAddLastName1;
  private string matchedPersonAddFirstName2;
  private string matchedPersonAddMiddleName2;
  private string matchedPersonAddLastName2;
  private string matchedPersonAddFirstName3;
  private string matchedPersonAddMiddleName3;
  private string matchedPersonAddLastName3;
  private string matchedPersonAddFirstName4;
  private string matchedPersonAddMiddleName4;
  private string matchedPersonAddLastName4;
  private string associatedSsn1;
  private string associatedFirstName1;
  private string associatedMiddleName1;
  private string associatedLastName1;
  private string associatedPersonSexCode1;
  private string associatedParticipantType1;
  private string associatedOthStTerrMembId1;
  private DateTime? associatedDob1;
  private DateTime? associatedDod1;
  private string associatedSsn2;
  private string associatedFirstName2;
  private string associatedMiddleName2;
  private string associatedLastName2;
  private string associatedPersonSexCode2;
  private string associatedParticipantType2;
  private string associatedOthStTerrMembId2;
  private DateTime? associatedDob2;
  private DateTime? associatedDod2;
  private string associatedSsn3;
  private string associatedFirstName3;
  private string associatedMiddleName3;
  private string associatedLastName3;
  private string associatedPersonSexCode3;
  private string associatedParticipantType3;
  private string associatedOthStTerrMembId3;
  private DateTime? associatedDob3;
  private DateTime? associatedDod3;
  private string associatedStateMembId1;
  private string associatedStateMembId2;
  private string associatedStateMembId3;
  private int identifier;
  private DateTime? dateReceived;
  private string lastName;
}

// The source file: DISCOVERY, ID: 371433862, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This is when a CSE attorney requests for or responds to discovery 
/// information for legal action proceedings.
/// Discovery is a pre-trial procedure by which one party gains vital 
/// information concerning the case held by the adverse party; the disclosure by
/// the adverse party of facts, deeds, documents and other such things which
/// are within his/her possession or knowledge exclusively, and which are
/// necessary to the other party's defense.
/// Examples may include interrogatories, request for production of documents, 
/// request for admissions, subpeona, and depositions.
/// </summary>
[Serializable]
public partial class Discovery: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Discovery()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Discovery(Discovery that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Discovery Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Discovery that)
  {
    base.Assign(that);
    requestedDate = that.requestedDate;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInt = that.middleInt;
    suffix = that.suffix;
    requestDescription = that.requestDescription;
    responseDescription = that.responseDescription;
    responseDate = that.responseDate;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    requestedByCseInd = that.requestedByCseInd;
    respReqByFirstName = that.respReqByFirstName;
    respReqByMi = that.respReqByMi;
    respReqByLastName = that.respReqByLastName;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>
  /// The value of the REQUESTED_DATE attribute.
  /// The specific date the discovery information is requested.
  /// </summary>
  [JsonPropertyName("requestedDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? RequestedDate
  {
    get => requestedDate;
    set => requestedDate = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// This attribute specifies the last name of the person being asked to 
  /// respond to the discovery information.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// This attribute specifies the first name of the person being asked to 
  /// respond to the discovery information.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the MIDDLE_INT attribute.</summary>
  public const int MiddleInt_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INT attribute.
  /// This attribute specifies the middle initial of the person being asked to 
  /// respond to the discovery information.
  /// </summary>
  [JsonPropertyName("middleInt")]
  [Member(Index = 4, Type = MemberType.Char, Length = MiddleInt_MaxLength, Optional
    = true)]
  public string MiddleInt
  {
    get => middleInt;
    set => middleInt = value != null
      ? TrimEnd(Substring(value, 1, MiddleInt_MaxLength)) : null;
  }

  /// <summary>Length of the SUFFIX attribute.</summary>
  public const int Suffix_MaxLength = 3;

  /// <summary>
  /// The value of the SUFFIX attribute.
  /// The suffix of the person requesting  or being asked to respond to 
  /// discovery information.
  /// </summary>
  [JsonPropertyName("suffix")]
  [Member(Index = 5, Type = MemberType.Char, Length = Suffix_MaxLength, Optional
    = true)]
  public string Suffix
  {
    get => suffix;
    set => suffix = value != null
      ? TrimEnd(Substring(value, 1, Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the REQUEST_DESCRIPTION attribute.</summary>
  public const int RequestDescription_MaxLength = 240;

  /// <summary>
  /// The value of the REQUEST_DESCRIPTION attribute.
  /// This is what is being requested for the discovery process.
  /// </summary>
  [JsonPropertyName("requestDescription")]
  [Member(Index = 6, Type = MemberType.Varchar, Length
    = RequestDescription_MaxLength, Optional = true)]
  public string RequestDescription
  {
    get => requestDescription;
    set => requestDescription = value != null
      ? Substring(value, 1, RequestDescription_MaxLength) : null;
  }

  /// <summary>Length of the RESPONSE_DESCRIPTION attribute.</summary>
  public const int ResponseDescription_MaxLength = 240;

  /// <summary>
  /// The value of the RESPONSE_DESCRIPTION attribute.
  /// This is the responding information on a discovery request.
  /// </summary>
  [JsonPropertyName("responseDescription")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = ResponseDescription_MaxLength, Optional = true)]
  public string ResponseDescription
  {
    get => responseDescription;
    set => responseDescription = value != null
      ? Substring(value, 1, ResponseDescription_MaxLength) : null;
  }

  /// <summary>
  /// The value of the RESPONSE_DATE attribute.
  /// The specific date a response is returned for a specific request.
  /// </summary>
  [JsonPropertyName("responseDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ResponseDate
  {
    get => responseDate;
    set => responseDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrence of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the REQUESTED_BY_CSE_IND attribute.</summary>
  public const int RequestedByCseInd_MaxLength = 1;

  /// <summary>
  /// The value of the REQUESTED_BY_CSE_IND attribute.
  /// This attributes specifies whether the information was requested by CSE.
  /// &quot;Y&quot; The information was requested by CSE
  /// &quot;N&quot; The information was requested by the other party.
  /// </summary>
  [JsonPropertyName("requestedByCseInd")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = RequestedByCseInd_MaxLength, Optional = true)]
  public string RequestedByCseInd
  {
    get => requestedByCseInd;
    set => requestedByCseInd = value != null
      ? TrimEnd(Substring(value, 1, RequestedByCseInd_MaxLength)) : null;
  }

  /// <summary>Length of the RESP_REQ_BY_FIRST_NAME attribute.</summary>
  public const int RespReqByFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the RESP_REQ_BY_FIRST_NAME attribute.
  /// This attribute specifies the first name of the person who has requested 
  /// the information.
  /// </summary>
  [JsonPropertyName("respReqByFirstName")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = RespReqByFirstName_MaxLength, Optional = true)]
  public string RespReqByFirstName
  {
    get => respReqByFirstName;
    set => respReqByFirstName = value != null
      ? TrimEnd(Substring(value, 1, RespReqByFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the RESP_REQ_BY_MI attribute.</summary>
  public const int RespReqByMi_MaxLength = 1;

  /// <summary>
  /// The value of the RESP_REQ_BY_MI attribute.
  /// This attribute specifies the middle initial of the person who has 
  /// requested the information.
  /// </summary>
  [JsonPropertyName("respReqByMi")]
  [Member(Index = 15, Type = MemberType.Char, Length = RespReqByMi_MaxLength, Optional
    = true)]
  public string RespReqByMi
  {
    get => respReqByMi;
    set => respReqByMi = value != null
      ? TrimEnd(Substring(value, 1, RespReqByMi_MaxLength)) : null;
  }

  /// <summary>Length of the RESP_REQ_BY_LAST_NAME attribute.</summary>
  public const int RespReqByLastName_MaxLength = 17;

  /// <summary>
  /// The value of the RESP_REQ_BY_LAST_NAME attribute.
  /// This attribute specifies the last name of the person who has requested the
  /// information.
  /// </summary>
  [JsonPropertyName("respReqByLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = RespReqByLastName_MaxLength, Optional = true)]
  public string RespReqByLastName
  {
    get => respReqByLastName;
    set => respReqByLastName = value != null
      ? TrimEnd(Substring(value, 1, RespReqByLastName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private DateTime? requestedDate;
  private string lastName;
  private string firstName;
  private string middleInt;
  private string suffix;
  private string requestDescription;
  private string responseDescription;
  private DateTime? responseDate;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string requestedByCseInd;
  private string respReqByFirstName;
  private string respReqByMi;
  private string respReqByLastName;
  private int lgaIdentifier;
}

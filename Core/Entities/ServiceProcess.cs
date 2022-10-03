﻿// The source file: SERVICE_PROCESS, ID: 371440192, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// The delivery of legal action documents whereby the respondant is furnished 
/// with reasonable notice of the proceedings against him/her to afford him/her
/// opportunity to appear and be heard.  The legal action documents can be
/// served by a Private Server, Certified Mail, First Class Mail or Entry of
/// Appearance.
/// </summary>
[Serializable]
public partial class ServiceProcess: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ServiceProcess()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ServiceProcess(ServiceProcess that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ServiceProcess Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ServiceProcess that)
  {
    base.Assign(that);
    identifier = that.identifier;
    methodOfService = that.methodOfService;
    serviceDocumentType = that.serviceDocumentType;
    serviceLocation = that.serviceLocation;
    serviceDate = that.serviceDate;
    serviceRequestDate = that.serviceRequestDate;
    returnDate = that.returnDate;
    serviceResult = that.serviceResult;
    serverName = that.serverName;
    requestedServee = that.requestedServee;
    servee = that.servee;
    serveeRelationship = that.serveeRelationship;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute together with the relationshyip &quot;IS_REQUIRED_FOR one 
  /// LEGAL ACTION&quot; uniquely identifies a service process record. It is
  /// automatically generated by the system starting from 1 for each legal
  /// action..
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the METHOD_OF_SERVICE attribute.</summary>
  public const int MethodOfService_MaxLength = 4;

  /// <summary>
  /// The value of the METHOD_OF_SERVICE attribute.
  /// The means of delivering a notice to the party to whom it is directed for 
  /// the purpose of obtaining jurisdication over and notice to that party.
  /// The valid values are maintained in the CODE_VALUE endity for code name 
  /// &quot;LE METHOD OF SERVICE&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = MethodOfService_MaxLength)]
    
  public string MethodOfService
  {
    get => methodOfService ?? "";
    set => methodOfService =
      TrimEnd(Substring(value, 1, MethodOfService_MaxLength));
  }

  /// <summary>
  /// The json value of the MethodOfService attribute.</summary>
  [JsonPropertyName("methodOfService")]
  [Computed]
  public string MethodOfService_Json
  {
    get => NullIf(MethodOfService, "");
    set => MethodOfService = value;
  }

  /// <summary>Length of the SERVICE_DOCUMENT_TYPE attribute.</summary>
  public const int ServiceDocumentType_MaxLength = 1;

  /// <summary>
  /// The value of the SERVICE_DOCUMENT_TYPE attribute.
  /// The document to request the court to serve a Legal Action.
  /// Valid codes are:
  /// 	P - Praecipe
  /// 	A - Praecipe for Alias Summons
  /// 	R - Request for Service
  /// 	C - Certificate of Service
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = ServiceDocumentType_MaxLength)]
  public string ServiceDocumentType
  {
    get => serviceDocumentType ?? "";
    set => serviceDocumentType =
      TrimEnd(Substring(value, 1, ServiceDocumentType_MaxLength));
  }

  /// <summary>
  /// The json value of the ServiceDocumentType attribute.</summary>
  [JsonPropertyName("serviceDocumentType")]
  [Computed]
  public string ServiceDocumentType_Json
  {
    get => NullIf(ServiceDocumentType, "");
    set => ServiceDocumentType = value;
  }

  /// <summary>Length of the SERVICE_LOCATION attribute.</summary>
  public const int ServiceLocation_MaxLength = 240;

  /// <summary>
  /// The value of the SERVICE_LOCATION attribute.
  /// The specific physical location where the notice is to be delivered to the 
  /// person/agency.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = ServiceLocation_MaxLength)]
  public string ServiceLocation
  {
    get => serviceLocation ?? "";
    set => serviceLocation = Substring(value, 1, ServiceLocation_MaxLength);
  }

  /// <summary>
  /// The json value of the ServiceLocation attribute.</summary>
  [JsonPropertyName("serviceLocation")]
  [Computed]
  public string ServiceLocation_Json
  {
    get => NullIf(ServiceLocation, "");
    set => ServiceLocation = value;
  }

  /// <summary>
  /// The value of the SERVICE_DATE attribute.
  /// The specific date the notice was delivered to the person/agency 
  /// successfully.
  /// </summary>
  [JsonPropertyName("serviceDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? ServiceDate
  {
    get => serviceDate;
    set => serviceDate = value;
  }

  /// <summary>
  /// The value of the SERVICE_REQUEST_DATE attribute.
  /// The specific date that the person/agency requests a notice be delivered to
  /// a person/agency.
  /// </summary>
  [JsonPropertyName("serviceRequestDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? ServiceRequestDate
  {
    get => serviceRequestDate;
    set => serviceRequestDate = value;
  }

  /// <summary>
  /// The value of the RETURN_DATE attribute.
  /// The specific date that the court receives the notice regarding the results
  /// of the delivery of the notice.
  /// </summary>
  [JsonPropertyName("returnDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnDate
  {
    get => returnDate;
    set => returnDate = value;
  }

  /// <summary>Length of the SERVICE_RESULT attribute.</summary>
  public const int ServiceResult_MaxLength = 240;

  /// <summary>
  /// The value of the SERVICE_RESULT attribute.
  /// This is the result of the delivery service to the person/agency.
  /// </summary>
  [JsonPropertyName("serviceResult")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = ServiceResult_MaxLength, Optional = true)]
  public string ServiceResult
  {
    get => serviceResult;
    set => serviceResult = value != null
      ? Substring(value, 1, ServiceResult_MaxLength) : null;
  }

  /// <summary>Length of the SERVER_NAME attribute.</summary>
  public const int ServerName_MaxLength = 37;

  /// <summary>
  /// The value of the SERVER_NAME attribute.
  /// The name of the person/agency who delivered the notice to the person/
  /// agency.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ServerName_MaxLength)]
  public string ServerName
  {
    get => serverName ?? "";
    set => serverName = TrimEnd(Substring(value, 1, ServerName_MaxLength));
  }

  /// <summary>
  /// The json value of the ServerName attribute.</summary>
  [JsonPropertyName("serverName")]
  [Computed]
  public string ServerName_Json
  {
    get => NullIf(ServerName, "");
    set => ServerName = value;
  }

  /// <summary>Length of the REQUESTED_SERVEE attribute.</summary>
  public const int RequestedServee_MaxLength = 37;

  /// <summary>
  /// The value of the REQUESTED_SERVEE attribute.
  /// The name of the person/agency that is to be served with the notice.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = RequestedServee_MaxLength)
    ]
  public string RequestedServee
  {
    get => requestedServee ?? "";
    set => requestedServee =
      TrimEnd(Substring(value, 1, RequestedServee_MaxLength));
  }

  /// <summary>
  /// The json value of the RequestedServee attribute.</summary>
  [JsonPropertyName("requestedServee")]
  [Computed]
  public string RequestedServee_Json
  {
    get => NullIf(RequestedServee, "");
    set => RequestedServee = value;
  }

  /// <summary>Length of the SERVEE attribute.</summary>
  public const int Servee_MaxLength = 37;

  /// <summary>
  /// The value of the SERVEE attribute.
  /// The name of the person/agency that was served with the notice. This person
  /// may or may not be the requested servee.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Servee_MaxLength)]
  public string Servee
  {
    get => servee ?? "";
    set => servee = TrimEnd(Substring(value, 1, Servee_MaxLength));
  }

  /// <summary>
  /// The json value of the Servee attribute.</summary>
  [JsonPropertyName("servee")]
  [Computed]
  public string Servee_Json
  {
    get => NullIf(Servee, "");
    set => Servee = value;
  }

  /// <summary>Length of the SERVEE_RELATIONSHIP attribute.</summary>
  public const int ServeeRelationship_MaxLength = 15;

  /// <summary>
  /// The value of the SERVEE_RELATIONSHIP attribute.
  /// The relationship of the servee to the requested servee.
  /// This attribute may be left blank if the servee is the same as the 
  /// requested servee.
  /// </summary>
  [JsonPropertyName("serveeRelationship")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ServeeRelationship_MaxLength, Optional = true)]
  public string ServeeRelationship
  {
    get => serveeRelationship;
    set => serveeRelationship = value != null
      ? TrimEnd(Substring(value, 1, ServeeRelationship_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 14, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 16, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
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

  private int identifier;
  private string methodOfService;
  private string serviceDocumentType;
  private string serviceLocation;
  private DateTime? serviceDate;
  private DateTime? serviceRequestDate;
  private DateTime? returnDate;
  private string serviceResult;
  private string serverName;
  private string requestedServee;
  private string servee;
  private string serveeRelationship;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private int lgaIdentifier;
}
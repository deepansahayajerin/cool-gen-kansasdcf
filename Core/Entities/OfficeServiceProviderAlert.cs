// The source file: OFFICE_SERVICE_PROVIDER_ALERT, ID: 371438581, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Office Service Provider Alert contains the alert records generated and 
/// delivered to a specific office service provider as a result of processing
/// and Event or created manually by the user.
/// Manually created alerts have no associated infrastructure record, are 
/// created from the office service provider alert procedure and are associated
/// to an office service provider for delivery.
/// DATA MODEL ALERT!!!!!
/// *	The relationship between OFFICE SERVICE PROVIDER ALERT and OFFICE SERVICE 
/// PROVIDER is not drawn.
/// each OFFICE SERVICE PROVIDER ALERT always informs one OFFICE SERVICE 
/// PROVIDER
/// each OFFICE SERVICE PROVIDER sometimes is informed by one or more OFFICE 
/// SERVICE PROVIDER ALERT
/// </summary>
[Serializable]
public partial class OfficeServiceProviderAlert: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeServiceProviderAlert()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeServiceProviderAlert(OfficeServiceProviderAlert that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeServiceProviderAlert Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeServiceProviderAlert that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    typeCode = that.typeCode;
    message = that.message;
    description = that.description;
    distributionDate = that.distributionDate;
    situationIdentifier = that.situationIdentifier;
    prioritizationCode = that.prioritizationCode;
    optimizationInd = that.optimizationInd;
    optimizedFlag = that.optimizedFlag;
    recipientUserId = that.recipientUserId;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    infId = that.infId;
    ospDate = that.ospDate;
    ospCode = that.ospCode;
    offId = that.offId;
    spdId = that.spdId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the TYPE_CODE attribute.</summary>
  public const int TypeCode_MaxLength = 3;

  /// <summary>
  /// The value of the TYPE_CODE attribute.
  /// Type classifies an alert as system generated or user generated.  This 
  /// attribute is always populated by the application.  It is only used by the
  /// user as a filtering agent in the list office service provider alerts
  /// screen.
  /// Values are:
  ///   MAN = manually generated (by a service provider)
  ///   AUT = automatically generated (by the event processor)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = TypeCode_MaxLength)]
  public string TypeCode
  {
    get => typeCode ?? "";
    set => typeCode = TrimEnd(Substring(value, 1, TypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TypeCode attribute.</summary>
  [JsonPropertyName("typeCode")]
  [Computed]
  public string TypeCode_Json
  {
    get => NullIf(TypeCode, "");
    set => TypeCode = value;
  }

  /// <summary>Length of the MESSAGE attribute.</summary>
  public const int Message_MaxLength = 55;

  /// <summary>
  /// The value of the MESSAGE attribute.
  /// The full text message raised by the alert.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Message_MaxLength)]
  public string Message
  {
    get => message ?? "";
    set => message = TrimEnd(Substring(value, 1, Message_MaxLength));
  }

  /// <summary>
  /// The json value of the Message attribute.</summary>
  [JsonPropertyName("message")]
  [Computed]
  public string Message_Json
  {
    get => NullIf(Message, "");
    set => Message = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Description is the full text message for an office service provider alert.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? TrimEnd(Substring(value, 1, Description_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DISTRIBUTION_DATE attribute.
  /// </summary>
  [JsonPropertyName("distributionDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? DistributionDate
  {
    get => distributionDate;
    set => distributionDate = value;
  }

  /// <summary>Length of the SITUATION_IDENTIFIER attribute.</summary>
  public const int SituationIdentifier_MaxLength = 9;

  /// <summary>
  /// The value of the SITUATION_IDENTIFIER attribute.
  /// The Situation Identifier is used by the application for optimization of 
  /// Service Provider Alert distribution.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = SituationIdentifier_MaxLength)]
  public string SituationIdentifier
  {
    get => situationIdentifier ?? "";
    set => situationIdentifier =
      TrimEnd(Substring(value, 1, SituationIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the SituationIdentifier attribute.</summary>
  [JsonPropertyName("situationIdentifier")]
  [Computed]
  public string SituationIdentifier_Json
  {
    get => NullIf(SituationIdentifier, "");
    set => SituationIdentifier = value;
  }

  /// <summary>
  /// The value of the PRIORITIZATION_CODE attribute.
  /// The prioritization code ranks the distribution rules into a hierarchy for 
  /// processing.
  /// Values are 1 through 9 with '1' having the highest value and '9' having 
  /// the lowest value.
  /// Default is '1', unless otherwise set.
  /// </summary>
  [JsonPropertyName("prioritizationCode")]
  [Member(Index = 7, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? PrioritizationCode
  {
    get => prioritizationCode;
    set => prioritizationCode = value;
  }

  /// <summary>Length of the OPTIMIZATION_IND attribute.</summary>
  public const int OptimizationInd_MaxLength = 1;

  /// <summary>
  /// The value of the OPTIMIZATION_IND attribute.
  /// The optimization indicator supplements distribution of alerts by business 
  /// object and assignment.  The indicator identifies those alerts requiring
  /// optimization.
  /// Optimization results in a deletion of a specified 'set' of the total 
  /// alerts which resulted from processing multiple infrastructure records of
  /// the same situation number.
  /// Default is set to 'N', indicating that the alert does not require 
  /// optimization.
  /// Values are:
  /// Y =  optimize alert distribution
  /// N =  do not optimize
  /// </summary>
  [JsonPropertyName("optimizationInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = OptimizationInd_MaxLength, Optional = true)]
  public string OptimizationInd
  {
    get => optimizationInd;
    set => optimizationInd = value != null
      ? TrimEnd(Substring(value, 1, OptimizationInd_MaxLength)) : null;
  }

  /// <summary>Length of the OPTIMIZED_FLAG attribute.</summary>
  public const int OptimizedFlag_MaxLength = 1;

  /// <summary>
  /// The value of the OPTIMIZED_FLAG attribute.
  /// Optimization Flag identifies those Office Service Provider Alerts which 
  /// have been processed for removal of duplicate alerts caused by single
  /// string processing of events for case/case unit object.
  /// Values are:
  /// Y  =	Process completed for optimization
  /// N  =	Not yet processed for optimization
  /// </summary>
  [JsonPropertyName("optimizedFlag")]
  [Member(Index = 9, Type = MemberType.Char, Length = OptimizedFlag_MaxLength, Optional
    = true)]
  public string OptimizedFlag
  {
    get => optimizedFlag;
    set => optimizedFlag = value != null
      ? TrimEnd(Substring(value, 1, OptimizedFlag_MaxLength)) : null;
  }

  /// <summary>Length of the RECIPIENT_USER_ID attribute.</summary>
  public const int RecipientUserId_MaxLength = 8;

  /// <summary>
  /// The value of the RECIPIENT_USER_ID attribute.
  /// The user identifier of the service provider responsible for the data 
  /// transformation which has resulted in creation of this alert.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = RecipientUserId_MaxLength)
    ]
  public string RecipientUserId
  {
    get => recipientUserId ?? "";
    set => recipientUserId =
      TrimEnd(Substring(value, 1, RecipientUserId_MaxLength));
  }

  /// <summary>
  /// The json value of the RecipientUserId attribute.</summary>
  [JsonPropertyName("recipientUserId")]
  [Computed]
  public string RecipientUserId_Json
  {
    get => NullIf(RecipientUserId, "");
    set => RecipientUserId = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? InfId
  {
    get => infId;
    set => infId = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? OspDate
  {
    get => ospDate;
    set => ospDate = value;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospCode")]
  [Member(Index = 17, Type = MemberType.Char, Length = OspCode_MaxLength, Optional
    = true)]
  public string OspCode
  {
    get => ospCode;
    set => ospCode = value != null
      ? TrimEnd(Substring(value, 1, OspCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffId
  {
    get => offId;
    set => offId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdId")]
  [Member(Index = 19, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdId
  {
    get => spdId;
    set => spdId = value;
  }

  private int systemGeneratedIdentifier;
  private string typeCode;
  private string message;
  private string description;
  private DateTime? distributionDate;
  private string situationIdentifier;
  private int? prioritizationCode;
  private string optimizationInd;
  private string optimizedFlag;
  private string recipientUserId;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? infId;
  private DateTime? ospDate;
  private string ospCode;
  private int? offId;
  private int? spdId;
}

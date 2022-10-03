// The source file: SERVICE_PROVIDER, ID: 371422667, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Persons or organizations that provide CSE services.
/// This may include:
/// Attorneys
/// Contractors
/// Collection Officers
/// CSE Administrative Personnel
/// Court Personnel
/// Office Assistants
/// Legal secretary
/// Receivables Collection Officers
/// excluding VENDORS and anyone else that does not have access to the system.
/// </summary>
[Serializable]
public partial class ServiceProvider: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ServiceProvider()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ServiceProvider(ServiceProvider that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ServiceProvider Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ServiceProvider that)
  {
    base.Assign(that);
    systemGeneratedId = that.systemGeneratedId;
    userId = that.userId;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    certificationNumber = that.certificationNumber;
    emailAddress = that.emailAddress;
    roleCode = that.roleCode;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    phoneAreaCode = that.phoneAreaCode;
    phoneNumber = that.phoneNumber;
    phoneExtension = that.phoneExtension;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int SystemGeneratedId
  {
    get => systemGeneratedId;
    set => systemGeneratedId = value;
  }

  /// <summary>Length of the USER_ID attribute.</summary>
  public const int UserId_MaxLength = 8;

  /// <summary>
  /// The value of the USER_ID attribute.
  /// Users of the CSE system.  This is their sign on id.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = UserId_MaxLength)]
  public string UserId
  {
    get => userId ?? "";
    set => userId = TrimEnd(Substring(value, 1, UserId_MaxLength));
  }

  /// <summary>
  /// The json value of the UserId attribute.</summary>
  [JsonPropertyName("userId")]
  [Computed]
  public string UserId_Json
  {
    get => NullIf(UserId, "");
    set => UserId = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// service provider's last name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  /// service provider's first name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// service provider's middle initial
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
  public string MiddleInitial
  {
    get => middleInitial ?? "";
    set => middleInitial =
      TrimEnd(Substring(value, 1, MiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleInitial attribute.</summary>
  [JsonPropertyName("middleInitial")]
  [Computed]
  public string MiddleInitial_Json
  {
    get => NullIf(MiddleInitial, "");
    set => MiddleInitial = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// the timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the CERTIFICATION_NUMBER attribute.</summary>
  public const int CertificationNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CERTIFICATION_NUMBER attribute.
  /// A service providers legal certificate number assigned to them by an 
  /// agencey outside of Child Support Enforcement to show their standing to
  /// practice law. Each service provider has one certificate number. Eg: An
  /// attorney.
  /// </summary>
  [JsonPropertyName("certificationNumber")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = CertificationNumber_MaxLength, Optional = true)]
  public string CertificationNumber
  {
    get => certificationNumber;
    set => certificationNumber = value != null
      ? TrimEnd(Substring(value, 1, CertificationNumber_MaxLength)) : null;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 50;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// This field contains the full E_mail address of the Service Provider.
  /// </summary>
  [JsonPropertyName("emailAddress")]
  [Member(Index = 11, Type = MemberType.Char, Length = EmailAddress_MaxLength, Optional
    = true)]
  public string EmailAddress
  {
    get => emailAddress;
    set => emailAddress = value != null
      ? TrimEnd(Substring(value, 1, EmailAddress_MaxLength)) : null;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int RoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// </summary>
  [JsonPropertyName("roleCode")]
  [Member(Index = 12, Type = MemberType.Char, Length = RoleCode_MaxLength, Optional
    = true)]
  public string RoleCode
  {
    get => roleCode;
    set => roleCode = value != null
      ? TrimEnd(Substring(value, 1, RoleCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the service provider became active
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date the service provider became inactive
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 15, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [Member(Index = 16, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the PHONE_EXTENSION attribute.
  /// </summary>
  [JsonPropertyName("phoneExtension")]
  [Member(Index = 17, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? PhoneExtension
  {
    get => phoneExtension;
    set => phoneExtension = value;
  }

  private int systemGeneratedId;
  private string userId;
  private string lastName;
  private string firstName;
  private string middleInitial;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? lastUpdatdTstamp;
  private string lastUpdatedBy;
  private string certificationNumber;
  private string emailAddress;
  private string roleCode;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private int? phoneAreaCode;
  private int? phoneNumber;
  private int? phoneExtension;
}

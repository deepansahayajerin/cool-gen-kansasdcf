// The source file: OFFICE, ID: 371422441, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// A physical location within the state of Kansas where CSE Services are 
/// managed or provided.
/// Every location will have a LOCATION TYPE.
/// Every Location may have one or more LOCATION ADDESSS/Phone.
/// Every location may have one or more service providers.
/// </summary>
[Serializable]
public partial class Office: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Office()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Office(Office that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Office Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Office that)
  {
    base.Assign(that);
    mainPhoneAreaCode = that.mainPhoneAreaCode;
    mainFaxAreaCode = that.mainFaxAreaCode;
    systemGeneratedId = that.systemGeneratedId;
    typeCode = that.typeCode;
    name = that.name;
    mainPhoneNumber = that.mainPhoneNumber;
    mainFaxPhoneNumber = that.mainFaxPhoneNumber;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    offOffice = that.offOffice;
    cogTypeCode = that.cogTypeCode;
    cogCode = that.cogCode;
  }

  /// <summary>
  /// The value of the MAIN_PHONE_AREA_CODE attribute.
  /// The area code for the primary voice phone number for an office.
  /// </summary>
  [JsonPropertyName("mainPhoneAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? MainPhoneAreaCode
  {
    get => mainPhoneAreaCode;
    set => mainPhoneAreaCode = value;
  }

  /// <summary>
  /// The value of the MAIN_FAX_AREA_CODE attribute.
  /// The area code for the primary fax phone number in an office.
  /// </summary>
  [JsonPropertyName("mainFaxAreaCode")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? MainFaxAreaCode
  {
    get => mainFaxAreaCode;
    set => mainFaxAreaCode = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 4)]
  public int SystemGeneratedId
  {
    get => systemGeneratedId;
    set => systemGeneratedId = value;
  }

  /// <summary>Length of the TYPE_CODE attribute.</summary>
  public const int TypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE_CODE attribute.
  /// CSE FIELD OFFICE
  /// CSE RECEIVABLES OFFICE
  /// CENTRAL OFFICE
  /// INTERSTATE OFFICE
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TypeCode_MaxLength)]
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

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 30;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the CSE office.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>
  /// The value of the MAIN_PHONE_NUMBER attribute.
  /// The primary phone number in an office.
  /// </summary>
  [JsonPropertyName("mainPhoneNumber")]
  [Member(Index = 6, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? MainPhoneNumber
  {
    get => mainPhoneNumber;
    set => mainPhoneNumber = value;
  }

  /// <summary>
  /// The value of the MAIN_FAX_PHONE_NUMBER attribute.
  /// The primary fax number in an office.
  /// </summary>
  [JsonPropertyName("mainFaxPhoneNumber")]
  [Member(Index = 7, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? MainFaxPhoneNumber
  {
    get => mainFaxPhoneNumber;
    set => mainFaxPhoneNumber = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offOffice")]
  [Member(Index = 14, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffOffice
  {
    get => offOffice;
    set => offOffice = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CogTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// REGION
  /// DIVISION
  /// COUNTY
  /// </summary>
  [JsonPropertyName("cogTypeCode")]
  [Member(Index = 15, Type = MemberType.Char, Length = CogTypeCode_MaxLength, Optional
    = true)]
  public string CogTypeCode
  {
    get => cogTypeCode;
    set => cogTypeCode = value != null
      ? TrimEnd(Substring(value, 1, CogTypeCode_MaxLength)) : null;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int CogCode_MaxLength = 2;

  /// <summary>
  /// The value of the CODE attribute.
  /// Identifies the CSE organization.  It distinguishes one occurrence of the 
  /// entity type from another.
  /// </summary>
  [JsonPropertyName("cogCode")]
  [Member(Index = 16, Type = MemberType.Char, Length = CogCode_MaxLength, Optional
    = true)]
  public string CogCode
  {
    get => cogCode;
    set => cogCode = value != null
      ? TrimEnd(Substring(value, 1, CogCode_MaxLength)) : null;
  }

  private int? mainPhoneAreaCode;
  private int? mainFaxAreaCode;
  private int systemGeneratedId;
  private string typeCode;
  private string name;
  private int? mainPhoneNumber;
  private int? mainFaxPhoneNumber;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int? offOffice;
  private string cogTypeCode;
  private string cogCode;
}

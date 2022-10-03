// The source file: OFFICE_SERVICE_PROVIDER, ID: 371422576, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This entity places SERVICE PROVIDERS in a OFFICE.
/// Example:
/// Office		Title		Name
/// -------------------------------------------
/// Central 	Director	Dennis H
/// Central		Scretary	Virgina
/// Central		NE Chief	Lorrie B
/// Central		Interstate	Mike Beech
/// 		Manager	
/// Central		Coll. Off.	Peggy S.
/// Liberal		Staff Sup     Dennis Hayzlett
/// Douglas		DA Contractor	Brian Farley
/// Dickenson	DCT		Audie Magana
/// Geary		DCT		Audie Magana
/// Marion		DCT		Audie Magana
/// Marshall	DCT		Audie Magana
/// Jackson		S2AT		Vacant
/// Jefferson	S2AT		Vacant	
/// </summary>
[Serializable]
public partial class OfficeServiceProvider: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OfficeServiceProvider()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OfficeServiceProvider(OfficeServiceProvider that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OfficeServiceProvider Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OfficeServiceProvider that)
  {
    base.Assign(that);
    workFaxAreaCode = that.workFaxAreaCode;
    workPhoneExtension = that.workPhoneExtension;
    workPhoneAreaCode = that.workPhoneAreaCode;
    zdelCertificationNumber = that.zdelCertificationNumber;
    roleCode = that.roleCode;
    workPhoneNumber = that.workPhoneNumber;
    workFaxNumber = that.workFaxNumber;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedDtstamp = that.lastUpdatedDtstamp;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    localContactCodeForIrs = that.localContactCodeForIrs;
    offGeneratedId = that.offGeneratedId;
    spdGeneratedId = that.spdGeneratedId;
  }

  /// <summary>
  /// The value of the WORK_FAX_AREA_CODE attribute.
  /// The area code for the fax phone number for a specific office service 
  /// provider.
  /// </summary>
  [JsonPropertyName("workFaxAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? WorkFaxAreaCode
  {
    get => workFaxAreaCode;
    set => workFaxAreaCode = value;
  }

  /// <summary>Length of the WORK_PHONE_EXTENSION attribute.</summary>
  public const int WorkPhoneExtension_MaxLength = 5;

  /// <summary>
  /// The value of the WORK_PHONE_EXTENSION attribute.
  /// The optional 5 charactor extension for the voice work phone number for a 
  /// specific office service provider.
  /// </summary>
  [JsonPropertyName("workPhoneExtension")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = WorkPhoneExtension_MaxLength, Optional = true)]
  public string WorkPhoneExtension
  {
    get => workPhoneExtension;
    set => workPhoneExtension = value != null
      ? TrimEnd(Substring(value, 1, WorkPhoneExtension_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the WORK_PHONE_AREA_CODE attribute.
  /// The area code for the voice phone number for a specific office service 
  /// provider.
  /// </summary>
  [JsonPropertyName("workPhoneAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int WorkPhoneAreaCode
  {
    get => workPhoneAreaCode;
    set => workPhoneAreaCode = value;
  }

  /// <summary>Length of the ZDEL_CERTIFICATION_NUMBER attribute.</summary>
  public const int ZdelCertificationNumber_MaxLength = 10;

  /// <summary>
  /// The value of the ZDEL_CERTIFICATION_NUMBER attribute.
  /// This attribute is used to identify an Office Service Provider to something
  /// outside of the Kessep system.
  /// </summary>
  [JsonPropertyName("zdelCertificationNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ZdelCertificationNumber_MaxLength, Optional = true)]
  public string ZdelCertificationNumber
  {
    get => zdelCertificationNumber;
    set => zdelCertificationNumber = value != null
      ? TrimEnd(Substring(value, 1, ZdelCertificationNumber_MaxLength)) : null;
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int RoleCode_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = RoleCode_MaxLength)]
  public string RoleCode
  {
    get => roleCode ?? "";
    set => roleCode = TrimEnd(Substring(value, 1, RoleCode_MaxLength));
  }

  /// <summary>
  /// The json value of the RoleCode attribute.</summary>
  [JsonPropertyName("roleCode")]
  [Computed]
  public string RoleCode_Json
  {
    get => NullIf(RoleCode, "");
    set => RoleCode = value;
  }

  /// <summary>
  /// The value of the WORK_PHONE_NUMBER attribute.
  /// This is an individuals phone number for him/her at the office that they 
  /// are related to.
  /// </summary>
  [JsonPropertyName("workPhoneNumber")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 7)]
  public int WorkPhoneNumber
  {
    get => workPhoneNumber;
    set => workPhoneNumber = value;
  }

  /// <summary>
  /// The value of the WORK_FAX_NUMBER attribute.
  /// This can support a fax number other then the main fax number for the 
  /// office.
  /// </summary>
  [JsonPropertyName("workFaxNumber")]
  [Member(Index = 7, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? WorkFaxNumber
  {
    get => workFaxNumber;
    set => workFaxNumber = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
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
  /// * Draft *
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
  /// * Draft *
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
  /// The value of the LAST_UPDATED_DTSTAMP attribute.
  /// * Draft *
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedDtstamp")]
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedDtstamp
  {
    get => lastUpdatedDtstamp;
    set => lastUpdatedDtstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// * Draft *
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
  /// * Draft *
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
  /// The value of the LOCAL_CONTACT_CODE_FOR_IRS attribute.
  /// This attribute specifies the Local Contact Code to be sent with FDSO 
  /// certifications.  We need to send an address tape containing these local
  /// codes and the corresponding contact addresses.  These contact addresses
  /// will be used in the pre-offset notices that are sent to the obligors by O.
  /// C.S.E.
  /// </summary>
  [JsonPropertyName("localContactCodeForIrs")]
  [Member(Index = 14, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? LocalContactCodeForIrs
  {
    get => localContactCodeForIrs;
    set => localContactCodeForIrs = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 4)]
  public int OffGeneratedId
  {
    get => offGeneratedId;
    set => offGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 16, Type = MemberType.Number, Length = 5)]
  public int SpdGeneratedId
  {
    get => spdGeneratedId;
    set => spdGeneratedId = value;
  }

  private int? workFaxAreaCode;
  private string workPhoneExtension;
  private int workPhoneAreaCode;
  private string zdelCertificationNumber;
  private string roleCode;
  private int workPhoneNumber;
  private int? workFaxNumber;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedDtstamp;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int? localContactCodeForIrs;
  private int offGeneratedId;
  private int spdGeneratedId;
}

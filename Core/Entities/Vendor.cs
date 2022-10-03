// The source file: VENDOR, ID: 371440478, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Originally in RESOURCES subject area.
/// SRS SCOPE:An individual
/// or group who could provide goods or services to the
/// agency.  Includes  consultants, equipment suppliers,
/// training suppliers, collection agencies, credit
/// reporting agencies, process servers, test labs, FPLS (
/// Federal Parent Locator Service), Department of
/// Administration.  It excludes providers of direct client
/// services, court trustees, prosecuting attorneys,
/// district attorneys.  YAS Examples:  Family preservation
/// consultant, computer retail suppliers.
/// 
/// CSE SCOPE: A person or organization who provides
/// services TO CSE.
/// QUALIFICATIONS:  To allow CSE to perform their SERVICE ACTIVITIES.
/// 
/// INCLUDES: Credit Bureaus, Collection Agencies.
/// EXCLUDES: CONTRACTORS (Court Trustees, County Attorneys, 
/// District Attorneys), CSE Employees (Collection Officers,
/// Supervisors).
/// 
/// Example: A collection officer assigned to a case may request
/// a collection agency to assist in collecting any money owed.
/// The collection agency has not been assigned that SERVICE
/// ACTIVITY, and is not responsible for it's outcome.  They
/// merely perform as requested and report back to the collection
/// officer.
/// </summary>
[Serializable]
public partial class Vendor: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Vendor()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Vendor(Vendor that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Vendor Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Vendor that)
  {
    base.Assign(that);
    faxAreaCode = that.faxAreaCode;
    phoneAreaCode = that.phoneAreaCode;
    faxExt = that.faxExt;
    phoneExt = that.phoneExt;
    identifier = that.identifier;
    name = that.name;
    number = that.number;
    phoneNumber = that.phoneNumber;
    fax = that.fax;
    contactPerson = that.contactPerson;
    serviceTypeCode = that.serviceTypeCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
  }

  /// <summary>
  /// The value of the FAX_AREA_CODE attribute.
  /// The 3-digit area code for the fax number of the vendor.
  /// </summary>
  [JsonPropertyName("faxAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FaxAreaCode
  {
    get => faxAreaCode;
    set => faxAreaCode = value;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number of the vendor.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 2, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the FAX_EXT attribute.</summary>
  public const int FaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the FAX_EXT attribute.
  /// The 5 digit extension for the fax number of the vendor.
  /// </summary>
  [JsonPropertyName("faxExt")]
  [Member(Index = 3, Type = MemberType.Char, Length = FaxExt_MaxLength, Optional
    = true)]
  public string FaxExt
  {
    get => faxExt;
    set => faxExt = value != null
      ? TrimEnd(Substring(value, 1, FaxExt_MaxLength)) : null;
  }

  /// <summary>Length of the PHONE_EXT attribute.</summary>
  public const int PhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXT attribute.
  /// The 5 digit extension for the phone number of the vendor.
  /// </summary>
  [JsonPropertyName("phoneExt")]
  [Member(Index = 4, Type = MemberType.Char, Length = PhoneExt_MaxLength, Optional
    = true)]
  public string PhoneExt
  {
    get => phoneExt;
    set => phoneExt = value != null
      ? TrimEnd(Substring(value, 1, PhoneExt_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique system generated number that descripts the details of a particular 
  /// vendor.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 8)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 30;

  /// <summary>
  /// The value of the NAME attribute.
  /// A particular name that descripts the vendor.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Name_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Unique number that a vendor uses for tax purposes such as EIN.
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 7, Type = MemberType.Char, Length = Number_MaxLength, Optional
    = true)]
  public string Number
  {
    get => number;
    set => number = value != null
      ? TrimEnd(Substring(value, 1, Number_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// The phone number of the vendor as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the FAX attribute.
  /// The fax number of the vendor as 7 digit phone number.
  /// </summary>
  [JsonPropertyName("fax")]
  [Member(Index = 9, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Fax
  {
    get => fax;
    set => fax = value;
  }

  /// <summary>Length of the CONTACT_PERSON attribute.</summary>
  public const int ContactPerson_MaxLength = 32;

  /// <summary>
  /// The value of the CONTACT_PERSON attribute.
  /// A particular person at the vendor address that can be contacted about 
  /// additional information.
  /// </summary>
  [JsonPropertyName("contactPerson")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ContactPerson_MaxLength, Optional = true)]
  public string ContactPerson
  {
    get => contactPerson;
    set => contactPerson = value != null
      ? TrimEnd(Substring(value, 1, ContactPerson_MaxLength)) : null;
  }

  /// <summary>Length of the SERVICE_TYPE_CODE attribute.</summary>
  public const int ServiceTypeCode_MaxLength = 4;

  /// <summary>
  /// The value of the SERVICE_TYPE_CODE attribute.
  /// Like Test site vendor/ Draw site vendor etc.
  /// Code values and descriptions are kept in CODE_VALUE table.
  /// </summary>
  [JsonPropertyName("serviceTypeCode")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ServiceTypeCode_MaxLength, Optional = true)]
  public string ServiceTypeCode
  {
    get => serviceTypeCode;
    set => serviceTypeCode = value != null
      ? TrimEnd(Substring(value, 1, ServiceTypeCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  private int? faxAreaCode;
  private int? phoneAreaCode;
  private string faxExt;
  private string phoneExt;
  private int identifier;
  private string name;
  private string number;
  private int? phoneNumber;
  private int? fax;
  private string contactPerson;
  private string serviceTypeCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
}

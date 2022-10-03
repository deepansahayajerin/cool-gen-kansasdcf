// The source file: INCOME_SOURCE_CONTACT, ID: 371435577, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// The name, phone number or fax number of a source of income for a given CSE 
/// PERSON.
/// </summary>
[Serializable]
public partial class IncomeSourceContact: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public IncomeSourceContact()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public IncomeSourceContact(IncomeSourceContact that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new IncomeSourceContact Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(IncomeSourceContact that)
  {
    base.Assign(that);
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    identifier = that.identifier;
    name = that.name;
    extensionNo = that.extensionNo;
    number = that.number;
    type1 = that.type1;
    areaCode = that.areaCode;
    emailAddress = that.emailAddress;
    csePerson = that.csePerson;
    isrIdentifier = that.isrIdentifier;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 2, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the IDENTIFIER attribute.
  /// A unique system generated identifier for an income source contact
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 30;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the contact for an income source
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 6, Type = MemberType.Char, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => name;
    set => name = value != null
      ? TrimEnd(Substring(value, 1, Name_MaxLength)) : null;
  }

  /// <summary>Length of the EXTENSION_NO attribute.</summary>
  public const int ExtensionNo_MaxLength = 6;

  /// <summary>
  /// The value of the EXTENSION_NO attribute.
  /// The extension number associated to a contact's telephone number. Length 
  /// changed from 4 to 6 by CQ 59705
  /// </summary>
  [JsonPropertyName("extensionNo")]
  [Member(Index = 7, Type = MemberType.Char, Length = ExtensionNo_MaxLength, Optional
    = true)]
  public string ExtensionNo
  {
    get => extensionNo;
    set => extensionNo = value != null
      ? TrimEnd(Substring(value, 1, ExtensionNo_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// The telephone or fax number of the income source contact
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Number
  {
    get => number;
    set => number = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This describes the type of phone number held.
  /// Hdqtrs fax Number             HF
  /// Hdqtrs Phone Number           HP
  /// Registered Agent Fax Number   RF
  /// Registered Agent Phone Number RP	
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// The area code of an income source contact phone, fax, or whatever.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 65;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// The email address that an income source contact can be reached at.
  /// </summary>
  [JsonPropertyName("emailAddress")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = EmailAddress_MaxLength, Optional = true)]
  public string EmailAddress
  {
    get => emailAddress;
    set => emailAddress = value != null
      ? Substring(value, 1, EmailAddress_MaxLength) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CsePerson_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CsePerson_MaxLength)]
  public string CsePerson
  {
    get => csePerson ?? "";
    set => csePerson = TrimEnd(Substring(value, 1, CsePerson_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePerson attribute.</summary>
  [JsonPropertyName("csePerson")]
  [Computed]
  public string CsePerson_Json
  {
    get => NullIf(CsePerson, "");
    set => CsePerson = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private int identifier;
  private string name;
  private string extensionNo;
  private int? number;
  private string type1;
  private int? areaCode;
  private string emailAddress;
  private string csePerson;
  private DateTime? isrIdentifier;
}

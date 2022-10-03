// The source file: INTERSTATE_CONTACT, ID: 371436359, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// A person working in a CSE office that is not located in the state of Kansas 
/// and to/from whom interstate activities have been requested.
/// </summary>
[Serializable]
public partial class InterstateContact: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateContact()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateContact(InterstateContact that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateContact Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateContact that)
  {
    base.Assign(that);
    contactPhoneNum = that.contactPhoneNum;
    startDate = that.startDate;
    endDate = that.endDate;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    nameLast = that.nameLast;
    nameFirst = that.nameFirst;
    nameMiddle = that.nameMiddle;
    contactNameSuffix = that.contactNameSuffix;
    areaCode = that.areaCode;
    contactPhoneExtension = that.contactPhoneExtension;
    contactFaxNumber = that.contactFaxNumber;
    contactFaxAreaCode = that.contactFaxAreaCode;
    contactInternetAddress = that.contactInternetAddress;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    intGeneratedId = that.intGeneratedId;
  }

  /// <summary>
  /// The value of the CONTACT_PHONE_NUM attribute.
  /// Work phone for Contact
  /// </summary>
  [JsonPropertyName("contactPhoneNum")]
  [Member(Index = 1, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ContactPhoneNum
  {
    get => contactPhoneNum;
    set => contactPhoneNum = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// The date this contact became effective.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date this contact is no longer effective.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the NAME_LAST attribute.</summary>
  public const int NameLast_MaxLength = 17;

  /// <summary>
  /// The value of the NAME_LAST attribute.
  /// Contact's last name
  /// </summary>
  [JsonPropertyName("nameLast")]
  [Member(Index = 6, Type = MemberType.Char, Length = NameLast_MaxLength, Optional
    = true)]
  public string NameLast
  {
    get => nameLast;
    set => nameLast = value != null
      ? TrimEnd(Substring(value, 1, NameLast_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_FIRST attribute.</summary>
  public const int NameFirst_MaxLength = 12;

  /// <summary>
  /// The value of the NAME_FIRST attribute.
  /// Contact's first name
  /// </summary>
  [JsonPropertyName("nameFirst")]
  [Member(Index = 7, Type = MemberType.Char, Length = NameFirst_MaxLength, Optional
    = true)]
  public string NameFirst
  {
    get => nameFirst;
    set => nameFirst = value != null
      ? TrimEnd(Substring(value, 1, NameFirst_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_MIDDLE attribute.</summary>
  public const int NameMiddle_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_MIDDLE attribute.
  /// Contact's middle initial
  /// </summary>
  [JsonPropertyName("nameMiddle")]
  [Member(Index = 8, Type = MemberType.Char, Length = NameMiddle_MaxLength, Optional
    = true)]
  public string NameMiddle
  {
    get => nameMiddle;
    set => nameMiddle = value != null
      ? TrimEnd(Substring(value, 1, NameMiddle_MaxLength)) : null;
  }

  /// <summary>Length of the CONTACT_NAME_SUFFIX attribute.</summary>
  public const int ContactNameSuffix_MaxLength = 3;

  /// <summary>
  /// The value of the CONTACT_NAME_SUFFIX attribute.
  /// Contact's name suffix (Jr, Sr, III, etc.)
  /// </summary>
  [JsonPropertyName("contactNameSuffix")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ContactNameSuffix_MaxLength, Optional = true)]
  public string ContactNameSuffix
  {
    get => contactNameSuffix;
    set => contactNameSuffix = value != null
      ? TrimEnd(Substring(value, 1, ContactNameSuffix_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// The area code of the work number for this person.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>Length of the CONTACT_PHONE_EXTENSION attribute.</summary>
  public const int ContactPhoneExtension_MaxLength = 6;

  /// <summary>
  /// The value of the CONTACT_PHONE_EXTENSION attribute.
  /// The extension number associated to a person's telephone number.
  /// </summary>
  [JsonPropertyName("contactPhoneExtension")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ContactPhoneExtension_MaxLength, Optional = true)]
  public string ContactPhoneExtension
  {
    get => contactPhoneExtension;
    set => contactPhoneExtension = value != null
      ? TrimEnd(Substring(value, 1, ContactPhoneExtension_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CONTACT_FAX_NUMBER attribute.
  /// The telephone number where faxes can be sent to the contact person.
  /// </summary>
  [JsonPropertyName("contactFaxNumber")]
  [Member(Index = 12, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ContactFaxNumber
  {
    get => contactFaxNumber;
    set => contactFaxNumber = value;
  }

  /// <summary>
  /// The value of the CONTACT_FAX_AREA_CODE attribute.
  /// The area code for the telephone number where faxes can be sent.
  /// </summary>
  [JsonPropertyName("contactFaxAreaCode")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ContactFaxAreaCode
  {
    get => contactFaxAreaCode;
    set => contactFaxAreaCode = value;
  }

  /// <summary>Length of the CONTACT_INTERNET_ADDRESS attribute.</summary>
  public const int ContactInternetAddress_MaxLength = 35;

  /// <summary>
  /// The value of the CONTACT_INTERNET_ADDRESS attribute.
  /// The internet address of the contact person.
  /// </summary>
  [JsonPropertyName("contactInternetAddress")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = ContactInternetAddress_MaxLength, Optional = true)]
  public string ContactInternetAddress
  {
    get => contactInternetAddress;
    set => contactInternetAddress = value != null
      ? TrimEnd(Substring(value, 1, ContactInternetAddress_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The sign on of the person that last updated the occurrence of the entity.
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
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// The date and time that the occurence of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 16, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 8)]
  public int IntGeneratedId
  {
    get => intGeneratedId;
    set => intGeneratedId = value;
  }

  private int? contactPhoneNum;
  private DateTime? startDate;
  private DateTime? endDate;
  private string createdBy;
  private DateTime? createdTstamp;
  private string nameLast;
  private string nameFirst;
  private string nameMiddle;
  private string contactNameSuffix;
  private int? areaCode;
  private string contactPhoneExtension;
  private int? contactFaxNumber;
  private int? contactFaxAreaCode;
  private string contactInternetAddress;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int intGeneratedId;
}

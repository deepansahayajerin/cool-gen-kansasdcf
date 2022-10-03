// The source file: CONTACT_DETAIL, ID: 371432655, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB	
/// The date, time, and narrative about a contact.
/// </summary>
[Serializable]
public partial class ContactDetail: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ContactDetail()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ContactDetail(ContactDetail that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ContactDetail Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ContactDetail that)
  {
    base.Assign(that);
    identifier = that.identifier;
    contactTime = that.contactTime;
    contactDate = that.contactDate;
    contactedUserid = that.contactedUserid;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    note = that.note;
    conNumber = that.conNumber;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// An identifier that identifies a particular contact detail information.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the CONTACT_TIME attribute.
  /// The time at which the contact was made.
  /// </summary>
  [JsonPropertyName("contactTime")]
  [Member(Index = 2, Type = MemberType.Time, Optional = true)]
  public TimeSpan? ContactTime
  {
    get => contactTime;
    set => contactTime = value;
  }

  /// <summary>
  /// The value of the CONTACT_DATE attribute.
  /// The date that a particular contact was made.
  /// </summary>
  [JsonPropertyName("contactDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ContactDate
  {
    get => contactDate;
    set => contactDate = value;
  }

  /// <summary>Length of the CONTACTED_USERID attribute.</summary>
  public const int ContactedUserid_MaxLength = 6;

  /// <summary>
  /// The value of the CONTACTED_USERID attribute.
  /// User ID of the user who has contacted/ verified.
  /// </summary>
  [JsonPropertyName("contactedUserid")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ContactedUserid_MaxLength, Optional = true)]
  public string ContactedUserid
  {
    get => contactedUserid;
    set => contactedUserid = value != null
      ? TrimEnd(Substring(value, 1, ContactedUserid_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 76;

  /// <summary>
  /// The value of the NOTE attribute.
  /// A description field to further identify contact.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>
  /// The value of the CONTACT_NUMBER attribute.
  /// Identifier that indicates a particular CSE contact person.
  /// </summary>
  [JsonPropertyName("conNumber")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 2)]
  public int ConNumber
  {
    get => conNumber;
    set => conNumber = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  private int identifier;
  private TimeSpan? contactTime;
  private DateTime? contactDate;
  private string contactedUserid;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string note;
  private int conNumber;
  private string cspNumber;
}

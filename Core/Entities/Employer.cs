// The source file: EMPLOYER, ID: 371434124, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// This entity type contains details about an employer known to CSE.
/// </summary>
[Serializable]
public partial class Employer: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Employer()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Employer(Employer that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Employer Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Employer that)
  {
    base.Assign(that);
    identifier = that.identifier;
    ein = that.ein;
    kansasId = that.kansasId;
    name = that.name;
    phoneNo = that.phoneNo;
    areaCode = that.areaCode;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    eiwoEndDate = that.eiwoEndDate;
    eiwoStartDate = that.eiwoStartDate;
    faxAreaCode = that.faxAreaCode;
    faxPhoneNo = that.faxPhoneNo;
    emailAddress = that.emailAddress;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the EIN attribute.</summary>
  public const int Ein_MaxLength = 9;

  /// <summary>
  /// The value of the EIN attribute.
  /// National number assigned to an employer.
  /// </summary>
  [JsonPropertyName("ein")]
  [Member(Index = 2, Type = MemberType.Char, Length = Ein_MaxLength, Optional
    = true)]
  public string Ein
  {
    get => ein;
    set => ein = value != null ? TrimEnd(Substring(value, 1, Ein_MaxLength)) : null
      ;
  }

  /// <summary>Length of the KANSAS_ID attribute.</summary>
  public const int KansasId_MaxLength = 6;

  /// <summary>
  /// The value of the KANSAS_ID attribute.
  /// Number assigned to a Kansas Employer.
  /// </summary>
  [JsonPropertyName("kansasId")]
  [Member(Index = 3, Type = MemberType.Char, Length = KansasId_MaxLength, Optional
    = true)]
  public string KansasId
  {
    get => kansasId;
    set => kansasId = value != null
      ? TrimEnd(Substring(value, 1, KansasId_MaxLength)) : null;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 33;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the employer.
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 4, Type = MemberType.Char, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => name;
    set => name = value != null
      ? TrimEnd(Substring(value, 1, Name_MaxLength)) : null;
  }

  /// <summary>Length of the PHONE_NO attribute.</summary>
  public const int PhoneNo_MaxLength = 7;

  /// <summary>
  /// The value of the PHONE_NO attribute.
  /// Business phone number of employer
  /// </summary>
  [JsonPropertyName("phoneNo")]
  [Member(Index = 5, Type = MemberType.Char, Length = PhoneNo_MaxLength, Optional
    = true)]
  public string PhoneNo
  {
    get => phoneNo;
    set => phoneNo = value != null
      ? TrimEnd(Substring(value, 1, PhoneNo_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// Area code of business phone number of employer.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [Member(Index = 6, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
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
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
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
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>
  /// The value of the EIWO_END_DATE attribute.
  /// The date the employer ended participation in the electronic income 
  /// withholding program.
  /// </summary>
  [JsonPropertyName("eiwoEndDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? EiwoEndDate
  {
    get => eiwoEndDate;
    set => eiwoEndDate = value;
  }

  /// <summary>
  /// The value of the EIWO_START_DATE attribute.
  /// The date the employer began participation in the electronic income 
  /// withholding program.
  /// </summary>
  [JsonPropertyName("eiwoStartDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? EiwoStartDate
  {
    get => eiwoStartDate;
    set => eiwoStartDate = value;
  }

  /// <summary>
  /// The value of the FAX_AREA_CODE attribute.
  /// The area code for the employer's FAX phone number.
  /// </summary>
  [JsonPropertyName("faxAreaCode")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FaxAreaCode
  {
    get => faxAreaCode;
    set => faxAreaCode = value;
  }

  /// <summary>Length of the FAX_PHONE_NO attribute.</summary>
  public const int FaxPhoneNo_MaxLength = 7;

  /// <summary>
  /// The value of the FAX_PHONE_NO attribute.
  /// The employer's FAX number
  /// </summary>
  [JsonPropertyName("faxPhoneNo")]
  [Member(Index = 14, Type = MemberType.Char, Length = FaxPhoneNo_MaxLength, Optional
    = true)]
  public string FaxPhoneNo
  {
    get => faxPhoneNo;
    set => faxPhoneNo = value != null
      ? TrimEnd(Substring(value, 1, FaxPhoneNo_MaxLength)) : null;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 65;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// The email address that an employer can be reached at.
  /// </summary>
  [JsonPropertyName("emailAddress")]
  [Member(Index = 15, Type = MemberType.Varchar, Length
    = EmailAddress_MaxLength, Optional = true)]
  public string EmailAddress
  {
    get => emailAddress;
    set => emailAddress = value != null
      ? Substring(value, 1, EmailAddress_MaxLength) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the employer became active.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date the employer is no longer active.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  private int identifier;
  private string ein;
  private string kansasId;
  private string name;
  private string phoneNo;
  private int? areaCode;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private DateTime? eiwoEndDate;
  private DateTime? eiwoStartDate;
  private int? faxAreaCode;
  private string faxPhoneNo;
  private string emailAddress;
  private DateTime? effectiveDate;
  private DateTime? endDate;
}

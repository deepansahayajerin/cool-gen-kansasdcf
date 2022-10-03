// The source file: CONTACT, ID: 371432504, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// An acquaintance, relative, friend, neighbor, or a business associate to a 
/// CSE person.
/// FED REQ: B-2.a.3
/// </summary>
[Serializable]
public partial class Contact: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Contact()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Contact(Contact that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Contact Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Contact that)
  {
    base.Assign(that);
    verifiedDate = that.verifiedDate;
    verifiedUserId = that.verifiedUserId;
    workPhoneAreaCode = that.workPhoneAreaCode;
    homePhoneAreaCode = that.homePhoneAreaCode;
    faxAreaCode = that.faxAreaCode;
    workPhoneExt = that.workPhoneExt;
    faxExt = that.faxExt;
    fax = that.fax;
    contactNumber = that.contactNumber;
    nameTitle = that.nameTitle;
    companyName = that.companyName;
    relationshipToCsePerson = that.relationshipToCsePerson;
    nameLast = that.nameLast;
    nameFirst = that.nameFirst;
    middleInitial = that.middleInitial;
    homePhone = that.homePhone;
    workPhone = that.workPhone;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// This attribute specifies the date on which the information about the 
  /// contact was verified.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 1, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// This attribute specifies the logon user id of the CSE worker who verified 
  /// the information about the contact.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the WORK_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the work phone number of the contact.
  /// </summary>
  [JsonPropertyName("workPhoneAreaCode")]
  [Member(Index = 3, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? WorkPhoneAreaCode
  {
    get => workPhoneAreaCode;
    set => workPhoneAreaCode = value;
  }

  /// <summary>
  /// The value of the HOME_PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the home phone number of the contact.
  /// </summary>
  [JsonPropertyName("homePhoneAreaCode")]
  [Member(Index = 4, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? HomePhoneAreaCode
  {
    get => homePhoneAreaCode;
    set => homePhoneAreaCode = value;
  }

  /// <summary>
  /// The value of the FAX_AREA_CODE attribute.
  /// The 3-digit area code for the fax number of the contact.
  /// </summary>
  [JsonPropertyName("faxAreaCode")]
  [Member(Index = 5, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? FaxAreaCode
  {
    get => faxAreaCode;
    set => faxAreaCode = value;
  }

  /// <summary>Length of the WORK_PHONE_EXT attribute.</summary>
  public const int WorkPhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the WORK_PHONE_EXT attribute.
  /// The 5 digit extension for the work phone of the contact.
  /// </summary>
  [JsonPropertyName("workPhoneExt")]
  [Member(Index = 6, Type = MemberType.Char, Length = WorkPhoneExt_MaxLength, Optional
    = true)]
  public string WorkPhoneExt
  {
    get => workPhoneExt;
    set => workPhoneExt = value != null
      ? TrimEnd(Substring(value, 1, WorkPhoneExt_MaxLength)) : null;
  }

  /// <summary>Length of the FAX_EXT attribute.</summary>
  public const int FaxExt_MaxLength = 5;

  /// <summary>
  /// The value of the FAX_EXT attribute.
  /// The 5 digit extension for the fax number of the contact.
  /// </summary>
  [JsonPropertyName("faxExt")]
  [Member(Index = 7, Type = MemberType.Char, Length = FaxExt_MaxLength, Optional
    = true)]
  public string FaxExt
  {
    get => faxExt;
    set => faxExt = value != null
      ? TrimEnd(Substring(value, 1, FaxExt_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FAX attribute.
  /// The 7-digit fax number of the contact.
  /// </summary>
  [JsonPropertyName("fax")]
  [Member(Index = 8, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Fax
  {
    get => fax;
    set => fax = value;
  }

  /// <summary>
  /// The value of the CONTACT_NUMBER attribute.
  /// Identifier that indicates a particular CSE contact person.
  /// </summary>
  [JsonPropertyName("contactNumber")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 2)]
  public int ContactNumber
  {
    get => contactNumber;
    set => contactNumber = value;
  }

  /// <summary>Length of the NAME_TITLE attribute.</summary>
  public const int NameTitle_MaxLength = 3;

  /// <summary>
  /// The value of the NAME_TITLE attribute.
  /// The title given a child such as JR, SR, or III.
  /// </summary>
  [JsonPropertyName("nameTitle")]
  [Member(Index = 10, Type = MemberType.Char, Length = NameTitle_MaxLength, Optional
    = true)]
  public string NameTitle
  {
    get => nameTitle;
    set => nameTitle = value != null
      ? TrimEnd(Substring(value, 1, NameTitle_MaxLength)) : null;
  }

  /// <summary>Length of the COMPANY_NAME attribute.</summary>
  public const int CompanyName_MaxLength = 33;

  /// <summary>
  /// The value of the COMPANY_NAME attribute.
  /// Possible name of school, union, hospital, or business that contact is 
  /// associated with.
  /// </summary>
  [JsonPropertyName("companyName")]
  [Member(Index = 11, Type = MemberType.Char, Length = CompanyName_MaxLength, Optional
    = true)]
  public string CompanyName
  {
    get => companyName;
    set => companyName = value != null
      ? TrimEnd(Substring(value, 1, CompanyName_MaxLength)) : null;
  }

  /// <summary>Length of the RELATIONSHIP_TO_CSE_PERSON attribute.</summary>
  public const int RelationshipToCsePerson_MaxLength = 15;

  /// <summary>
  /// The value of the RELATIONSHIP_TO_CSE_PERSON attribute.
  /// Identifier as to what type of contact. Example:  relative, business 
  /// associate, hospital(separate VA out from civilian), friend, etc.
  /// </summary>
  [JsonPropertyName("relationshipToCsePerson")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = RelationshipToCsePerson_MaxLength, Optional = true)]
  public string RelationshipToCsePerson
  {
    get => relationshipToCsePerson;
    set => relationshipToCsePerson = value != null
      ? TrimEnd(Substring(value, 1, RelationshipToCsePerson_MaxLength)) : null;
  }

  /// <summary>Length of the NAME_LAST attribute.</summary>
  public const int NameLast_MaxLength = 17;

  /// <summary>
  /// The value of the NAME_LAST attribute.
  /// The surname of the person who has had contact with the CSE Person.
  /// </summary>
  [JsonPropertyName("nameLast")]
  [Member(Index = 13, Type = MemberType.Char, Length = NameLast_MaxLength, Optional
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
  /// The given name of the person who has had contact with the CSE Person.
  /// </summary>
  [JsonPropertyName("nameFirst")]
  [Member(Index = 14, Type = MemberType.Char, Length = NameFirst_MaxLength, Optional
    = true)]
  public string NameFirst
  {
    get => nameFirst;
    set => nameFirst = value != null
      ? TrimEnd(Substring(value, 1, NameFirst_MaxLength)) : null;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// The initial of the person who has had contact with the CSE Person.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = MiddleInitial_MaxLength, Optional = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the HOME_PHONE attribute.
  /// The 7-digit home phone number where the contact can be reached.
  /// </summary>
  [JsonPropertyName("homePhone")]
  [Member(Index = 16, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? HomePhone
  {
    get => homePhone;
    set => homePhone = value;
  }

  /// <summary>
  /// The value of the WORK_PHONE attribute.
  /// The 7-digit Work or other phone number for contact.
  /// </summary>
  [JsonPropertyName("workPhone")]
  [Member(Index = 17, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? WorkPhone
  {
    get => workPhone;
    set => workPhone = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 19, Type = MemberType.Timestamp)]
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
  [Member(Index = 20, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 21, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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
  [Member(Index = 22, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private DateTime? verifiedDate;
  private string verifiedUserId;
  private int? workPhoneAreaCode;
  private int? homePhoneAreaCode;
  private int? faxAreaCode;
  private string workPhoneExt;
  private string faxExt;
  private int? fax;
  private int contactNumber;
  private string nameTitle;
  private string companyName;
  private string relationshipToCsePerson;
  private string nameLast;
  private string nameFirst;
  private string middleInitial;
  private int? homePhone;
  private int? workPhone;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
}

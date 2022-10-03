// The source file: LEGAL_ACTION_RESPONSE, ID: 371436917, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// Legal Action Response is the response given by someone to the legal action 
/// being served.
/// </summary>
[Serializable]
public partial class LegalActionResponse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionResponse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionResponse(LegalActionResponse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionResponse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionResponse that)
  {
    base.Assign(that);
    type1 = that.type1;
    receivedDate = that.receivedDate;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    suffix = that.suffix;
    relationship = that.relationship;
    narrative = that.narrative;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    respForFirstName = that.respForFirstName;
    respForMiddleInitial = that.respForMiddleInitial;
    respForLastName = that.respForLastName;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Type is the answer to a potential legal via a telephone call, letter, fax,
  /// office visit, or legal pleading.
  /// Valid codes are:
  /// 	TC - Telephone Call
  /// 	LT - Letter
  /// 	FX - Fax
  /// 	OV - Office Visit
  /// 	LP - Legal Pending
  /// 	OT - Other
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The value of the RECEIVED_DATE attribute.
  /// The specific date that a response is received by the court or CSE, to a 
  /// legal notice served upon a specific person.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? ReceivedDate
  {
    get => receivedDate;
    set => receivedDate = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// The last name of the person responding to legal action.
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
  /// The first name of the person responding to the legal action.
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
  /// The middle initial of the person responding to the legal action.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 5, Type = MemberType.Char, Length = MiddleInitial_MaxLength, Optional
    = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  /// <summary>Length of the SUFFIX attribute.</summary>
  public const int Suffix_MaxLength = 2;

  /// <summary>
  /// The value of the SUFFIX attribute.
  /// The suffix of the person responding to the legal action.
  /// Examples: Jr, Sr, etc.
  /// </summary>
  [JsonPropertyName("suffix")]
  [Member(Index = 6, Type = MemberType.Char, Length = Suffix_MaxLength, Optional
    = true)]
  public string Suffix
  {
    get => suffix;
    set => suffix = value != null
      ? TrimEnd(Substring(value, 1, Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the RELATIONSHIP attribute.</summary>
  public const int Relationship_MaxLength = 15;

  /// <summary>
  /// The value of the RELATIONSHIP attribute.
  /// The relationship of the person to the absent parent/applicant recipient 
  /// responding to the legal action.
  /// Examples:  mother, sister, attorney, spouse, etc.
  /// </summary>
  [JsonPropertyName("relationship")]
  [Member(Index = 7, Type = MemberType.Char, Length = Relationship_MaxLength, Optional
    = true)]
  public string Relationship
  {
    get => relationship;
    set => relationship = value != null
      ? TrimEnd(Substring(value, 1, Relationship_MaxLength)) : null;
  }

  /// <summary>Length of the NARRATIVE attribute.</summary>
  public const int Narrative_MaxLength = 240;

  /// <summary>
  /// The value of the NARRATIVE attribute.
  /// Narrative of what the responding person actually said.
  /// </summary>
  [JsonPropertyName("narrative")]
  [Member(Index = 8, Type = MemberType.Varchar, Length = Narrative_MaxLength, Optional
    = true)]
  public string Narrative
  {
    get => narrative;
    set => narrative = value != null
      ? Substring(value, 1, Narrative_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrence of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the RESP_FOR_FIRST_NAME attribute.</summary>
  public const int RespForFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the RESP_FOR_FIRST_NAME attribute.
  /// This attributes specifies the first name of the person on behalf of whom 
  /// the legal response was raised.
  /// </summary>
  [JsonPropertyName("respForFirstName")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = RespForFirstName_MaxLength, Optional = true)]
  public string RespForFirstName
  {
    get => respForFirstName;
    set => respForFirstName = value != null
      ? TrimEnd(Substring(value, 1, RespForFirstName_MaxLength)) : null;
  }

  /// <summary>Length of the RESP_FOR_MIDDLE_INITIAL attribute.</summary>
  public const int RespForMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the RESP_FOR_MIDDLE_INITIAL attribute.
  /// This attributes specifies the middle initial  of the person on behalf of 
  /// whom the legal response was raised.
  /// </summary>
  [JsonPropertyName("respForMiddleInitial")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = RespForMiddleInitial_MaxLength, Optional = true)]
  public string RespForMiddleInitial
  {
    get => respForMiddleInitial;
    set => respForMiddleInitial = value != null
      ? TrimEnd(Substring(value, 1, RespForMiddleInitial_MaxLength)) : null;
  }

  /// <summary>Length of the RESP_FOR_LAST_NAME attribute.</summary>
  public const int RespForLastName_MaxLength = 17;

  /// <summary>
  /// The value of the RESP_FOR_LAST_NAME attribute.
  /// This attribute specifies the last name of the person on behalf of whom the
  /// legal response was raised.
  /// </summary>
  [JsonPropertyName("respForLastName")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = RespForLastName_MaxLength, Optional = true)]
  public string RespForLastName
  {
    get => respForLastName;
    set => respForLastName = value != null
      ? TrimEnd(Substring(value, 1, RespForLastName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private string type1;
  private DateTime? receivedDate;
  private string lastName;
  private string firstName;
  private string middleInitial;
  private string suffix;
  private string relationship;
  private string narrative;
  private string createdBy;
  private DateTime? createdTstamp;
  private string respForFirstName;
  private string respForMiddleInitial;
  private string respForLastName;
  private int lgaIdentifier;
}

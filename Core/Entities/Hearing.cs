// The source file: HEARING, ID: 371435201, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// A proceeding wherein evidence is taken for the purpose of determining an 
/// issue of fact and reaching a decision on the basis of that evidence.
/// </summary>
[Serializable]
public partial class Hearing: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Hearing()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Hearing(Hearing that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Hearing Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Hearing that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    conductedDate = that.conductedDate;
    conductedTime = that.conductedTime;
    type1 = that.type1;
    lastName = that.lastName;
    firstName = that.firstName;
    middleInt = that.middleInt;
    suffix = that.suffix;
    title = that.title;
    outcome = that.outcome;
    outcomeReceivedDate = that.outcomeReceivedDate;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    note = that.note;
    aapIdentifier = that.aapIdentifier;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// This is a system generated identifier to uniquely identify a hearing.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the CONDUCTED_DATE attribute.
  /// A specific date that the hearing is conducted.
  /// </summary>
  [JsonPropertyName("conductedDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? ConductedDate
  {
    get => conductedDate;
    set => conductedDate = value;
  }

  /// <summary>
  /// The value of the CONDUCTED_TIME attribute.
  /// A specific time that the hearing takes place.
  /// </summary>
  [JsonPropertyName("conductedTime")]
  [Member(Index = 3, Type = MemberType.Time)]
  public TimeSpan ConductedTime
  {
    get => conductedTime;
    set => conductedTime = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of hearing.  This is used to differentiate between legal actions,
  /// and the different types of administrative appeal hearings.
  /// Valid codes are:
  /// 	A - Administrative Hearing
  /// 	J - Judicial (Legal)
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 4, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// The last name of the person who presides over an administrative hearing 
  /// process and has the authority to make a ruling.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength)]
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
  /// The first name of the person who presides over an administrative hearing 
  /// process and has the authority to make a ruling.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FirstName_MaxLength)]
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

  /// <summary>Length of the MIDDLE_INT attribute.</summary>
  public const int MiddleInt_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INT attribute.
  /// The middle initial  of the person who presides over an administrative 
  /// hearing process and has the authority to make a ruling.
  /// </summary>
  [JsonPropertyName("middleInt")]
  [Member(Index = 7, Type = MemberType.Char, Length = MiddleInt_MaxLength, Optional
    = true)]
  public string MiddleInt
  {
    get => middleInt;
    set => middleInt = value != null
      ? TrimEnd(Substring(value, 1, MiddleInt_MaxLength)) : null;
  }

  /// <summary>Length of the SUFFIX attribute.</summary>
  public const int Suffix_MaxLength = 3;

  /// <summary>
  /// The value of the SUFFIX attribute.
  /// The suffix of the person who presides over an administrative hearing 
  /// process and has the authority to make a ruling.
  /// Ex. Jr, Sr, I, II, etc.
  /// </summary>
  [JsonPropertyName("suffix")]
  [Member(Index = 8, Type = MemberType.Char, Length = Suffix_MaxLength, Optional
    = true)]
  public string Suffix
  {
    get => suffix;
    set => suffix = value != null
      ? TrimEnd(Substring(value, 1, Suffix_MaxLength)) : null;
  }

  /// <summary>Length of the TITLE attribute.</summary>
  public const int Title_MaxLength = 15;

  /// <summary>
  /// The value of the TITLE attribute.
  /// The title of the person who presides over an administrative hearing 
  /// process.
  /// </summary>
  [JsonPropertyName("title")]
  [Member(Index = 9, Type = MemberType.Char, Length = Title_MaxLength, Optional
    = true)]
  public string Title
  {
    get => title;
    set => title = value != null
      ? TrimEnd(Substring(value, 1, Title_MaxLength)) : null;
  }

  /// <summary>Length of the OUTCOME attribute.</summary>
  public const int Outcome_MaxLength = 240;

  /// <summary>
  /// The value of the OUTCOME attribute.
  /// The  ruling which indicates specific directions to a person or agency.
  /// </summary>
  [JsonPropertyName("outcome")]
  [Member(Index = 10, Type = MemberType.Varchar, Length = Outcome_MaxLength, Optional
    = true)]
  public string Outcome
  {
    get => outcome;
    set => outcome = value != null ? Substring(value, 1, Outcome_MaxLength) : null
      ;
  }

  /// <summary>
  /// The value of the OUTCOME_RECEIVED_DATE attribute.
  /// The specific date that the tribunal issues its ruling.
  /// </summary>
  [JsonPropertyName("outcomeReceivedDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? OutcomeReceivedDate
  {
    get => outcomeReceivedDate;
    set => outcomeReceivedDate = value;
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrence of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 15, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 65;

  /// <summary>
  /// The value of the NOTE attribute.
  /// A place to put a note at the bottom of screen
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 16, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify an Administrative 
  /// Appeal.
  /// </summary>
  [JsonPropertyName("aapIdentifier")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? AapIdentifier
  {
    get => aapIdentifier;
    set => aapIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private int systemGeneratedIdentifier;
  private DateTime? conductedDate;
  private TimeSpan conductedTime;
  private string type1;
  private string lastName;
  private string firstName;
  private string middleInt;
  private string suffix;
  private string title;
  private string outcome;
  private DateTime? outcomeReceivedDate;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string note;
  private int? aapIdentifier;
  private int? lgaIdentifier;
}

// The source file: PERSON_PROGRAM, ID: 371439457, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// A particular program on a particular person.
/// </summary>
[Serializable]
public partial class PersonProgram: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonProgram()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonProgram(PersonProgram that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonProgram Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonProgram that)
  {
    base.Assign(that);
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    status = that.status;
    closureReason = that.closureReason;
    effectiveDate = that.effectiveDate;
    assignedDate = that.assignedDate;
    discontinueDate = that.discontinueDate;
    changedInd = that.changedInd;
    changeDate = that.changeDate;
    medTypeDiscontinueDate = that.medTypeDiscontinueDate;
    medType = that.medType;
    cspNumber = that.cspNumber;
    prgGeneratedId = that.prgGeneratedId;
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
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
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
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// * Draft *
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// Identifies if a CSE person is currently participating in a program or if 
  /// that program is closed.
  /// </summary>
  [JsonPropertyName("status")]
  [Member(Index = 5, Type = MemberType.Char, Length = Status_MaxLength, Optional
    = true)]
  public string Status
  {
    get => status;
    set => status = value != null
      ? TrimEnd(Substring(value, 1, Status_MaxLength)) : null;
  }

  /// <summary>Length of the CLOSURE_REASON attribute.</summary>
  public const int ClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the CLOSURE_REASON attribute.
  /// The reason why a program for a person on a case was closed.
  /// </summary>
  [JsonPropertyName("closureReason")]
  [Member(Index = 6, Type = MemberType.Char, Length = ClosureReason_MaxLength, Optional
    = true)]
  public string ClosureReason
  {
    get => closureReason;
    set => closureReason = value != null
      ? TrimEnd(Substring(value, 1, ClosureReason_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// This date is used to show when the program is effective for a Cse Person.
  /// For example, a Cse Person may be placed on a Program in the middle of
  /// the month(ASSIGNED_DATE), however, the program was effective from the
  /// first day of that month(EFFECTIVE_BEGIN_DATE).
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the ASSIGNED_DATE attribute.
  /// This date is used to show when the program is assigned to a Cse Person.  
  /// For example, a Cse Person may be placed on a Program in the middle of the
  /// month(ASSIGNED_DATE), however, the program was effective from the first
  /// day of that month(EFFECTIVE_BEGIN_DATE).
  /// </summary>
  [JsonPropertyName("assignedDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignedDate
  {
    get => assignedDate;
    set => assignedDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// This date is the date the program for a Cse Person is closed.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CHANGED_IND attribute.</summary>
  public const int ChangedInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHANGED_IND attribute.
  /// An indicator used when a person goes from ADC to Non-ADC or Non_ADC to 
  /// ADC.
  /// </summary>
  [JsonPropertyName("changedInd")]
  [Member(Index = 10, Type = MemberType.Char, Length = ChangedInd_MaxLength, Optional
    = true)]
  public string ChangedInd
  {
    get => changedInd;
    set => changedInd = value != null
      ? TrimEnd(Substring(value, 1, ChangedInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CHANGE_DATE attribute.
  /// This date will remain null until program details are changed. This will be
  /// used by financial to switch financial details from one program to
  /// another.
  /// </summary>
  [JsonPropertyName("changeDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? ChangeDate
  {
    get => changeDate;
    set => changeDate = value;
  }

  /// <summary>
  /// The value of the MED_TYPE_DISCONTINUE_DATE attribute.
  /// RESP: KESSEP
  /// 
  /// The date the EM or WT program closed in the automated eligibility
  /// system.
  /// </summary>
  [JsonPropertyName("medTypeDiscontinueDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? MedTypeDiscontinueDate
  {
    get => medTypeDiscontinueDate;
    set => medTypeDiscontinueDate = value;
  }

  /// <summary>Length of the MED_TYPE attribute.</summary>
  public const int MedType_MaxLength = 2;

  /// <summary>
  /// The value of the MED_TYPE attribute.
  /// RESP: KESSEP
  /// 
  /// Extended medical (EM) or trans med (WT) program code.
  /// </summary>
  [JsonPropertyName("medType")]
  [Member(Index = 13, Type = MemberType.Char, Length = MedType_MaxLength, Optional
    = true)]
  public string MedType
  {
    get => medType;
    set => medType = value != null
      ? TrimEnd(Substring(value, 1, MedType_MaxLength)) : null;
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
  [Member(Index = 14, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("prgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 3)]
  public int PrgGeneratedId
  {
    get => prgGeneratedId;
    set => prgGeneratedId = value;
  }

  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private string status;
  private string closureReason;
  private DateTime? effectiveDate;
  private DateTime? assignedDate;
  private DateTime? discontinueDate;
  private string changedInd;
  private DateTime? changeDate;
  private DateTime? medTypeDiscontinueDate;
  private string medType;
  private string cspNumber;
  private int prgGeneratedId;
}

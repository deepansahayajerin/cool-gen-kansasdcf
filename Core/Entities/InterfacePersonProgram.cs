// The source file: INTERFACE_PERSON_PROGRAM, ID: 371435823, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVIN
/// This table represents the interface data store for Person Program 
/// information that will be passed from AE and KSCares.  The information will
/// be used to apply to cases referred to CSE from these areas.
/// </summary>
[Serializable]
public partial class InterfacePersonProgram: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfacePersonProgram()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfacePersonProgram(InterfacePersonProgram that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfacePersonProgram Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfacePersonProgram that)
  {
    base.Assign(that);
    recordType = that.recordType;
    sourceOfFunds = that.sourceOfFunds;
    programCode = that.programCode;
    csePersonNumber = that.csePersonNumber;
    statusInd = that.statusInd;
    closureReason = that.closureReason;
    from = that.from;
    progEffectiveDate = that.progEffectiveDate;
    programEndDate = that.programEndDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    processDate = that.processDate;
    assignedDate = that.assignedDate;
    aeProgramSubtype = that.aeProgramSubtype;
    participationCode = that.participationCode;
    paCaseNumber = that.paCaseNumber;
    medTypeDiscontinueDate = that.medTypeDiscontinueDate;
    medType = that.medType;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// The type of Person-Program information being sent:
  /// 	A - Add a new Person-Program
  /// 	U - Update, a change to an existing 			Person-Program
  /// 	D - Delete, a previous Person-Program 			was sent in error and should be
  /// deleted.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the SOURCE_OF_FUNDS attribute.</summary>
  public const int SourceOfFunds_MaxLength = 3;

  /// <summary>
  /// The value of the SOURCE_OF_FUNDS attribute.
  /// Indicates whether this Person-Program's funding is state or federal;  this
  /// is only valid for foster care:
  /// 	AF - Federal
  /// 	GA - State
  /// </summary>
  [JsonPropertyName("sourceOfFunds")]
  [Member(Index = 2, Type = MemberType.Char, Length = SourceOfFunds_MaxLength, Optional
    = true)]
  public string SourceOfFunds
  {
    get => sourceOfFunds;
    set => sourceOfFunds = value != null
      ? TrimEnd(Substring(value, 1, SourceOfFunds_MaxLength)) : null;
  }

  /// <summary>Length of the PROGRAM_CODE attribute.</summary>
  public const int ProgramCode_MaxLength = 3;

  /// <summary>
  /// The value of the PROGRAM_CODE attribute.
  /// Two-digit alphabetical code indicating the current program participation.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ProgramCode_MaxLength)]
  public string ProgramCode
  {
    get => programCode ?? "";
    set => programCode = TrimEnd(Substring(value, 1, ProgramCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramCode attribute.</summary>
  [JsonPropertyName("programCode")]
  [Computed]
  public string ProgramCode_Json
  {
    get => NullIf(ProgramCode, "");
    set => ProgramCode = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// CSE person number for the person program.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>Length of the STATUS_IND attribute.</summary>
  public const int StatusInd_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS_IND attribute.
  /// The status indicator defines the status of the record. All unprocessed 
  /// records will have 'spaces' in this field. Successfully processed records
  /// will have 'P' in this field and error records will have 'E' in it.
  /// </summary>
  [JsonPropertyName("statusInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = StatusInd_MaxLength, Optional
    = true)]
  public string StatusInd
  {
    get => statusInd;
    set => statusInd = value != null
      ? TrimEnd(Substring(value, 1, StatusInd_MaxLength)) : null;
  }

  /// <summary>Length of the CLOSURE_REASON attribute.</summary>
  public const int ClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the CLOSURE_REASON attribute.
  /// The reason why AE has closed their case.
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

  /// <summary>Length of the FROM attribute.</summary>
  public const int From_MaxLength = 3;

  /// <summary>
  /// The value of the FROM attribute.
  /// The origin of the referral as of 0796 may be:
  ///        KAE  - Kansas Automated Eligibility
  ///        KSC - Kansas Cares
  /// </summary>
  [JsonPropertyName("from")]
  [Member(Index = 7, Type = MemberType.Char, Length = From_MaxLength, Optional
    = true)]
  public string From
  {
    get => from;
    set => from = value != null
      ? TrimEnd(Substring(value, 1, From_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PROG_EFFECTIVE_DATE attribute.
  /// Date upon which this AE or KSCARES program began for this person.
  /// </summary>
  [JsonPropertyName("progEffectiveDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? ProgEffectiveDate
  {
    get => progEffectiveDate;
    set => progEffectiveDate = value;
  }

  /// <summary>
  /// The value of the PROGRAM_END_DATE attribute.
  /// Date upon which this AE or KSCARES program is discontinued for this 
  /// person.
  /// </summary>
  [JsonPropertyName("programEndDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? ProgramEndDate
  {
    get => programEndDate;
    set => programEndDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 10, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// Date which this record is processed.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>
  /// The value of the ASSIGNED_DATE attribute.
  /// This date is used to show when the program is assigned to a Cse Person.  
  /// For example, a Cse Person may be placed on a Program in the middle of the
  /// month(ASSIGNED_DATE), however, the program was effective from the first
  /// day of that month(EFFECTIVE_BEGIN_DATE).
  /// </summary>
  [JsonPropertyName("assignedDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignedDate
  {
    get => assignedDate;
    set => assignedDate = value;
  }

  /// <summary>Length of the AE_PROGRAM_SUBTYPE attribute.</summary>
  public const int AeProgramSubtype_MaxLength = 2;

  /// <summary>
  /// The value of the AE_PROGRAM_SUBTYPE attribute.
  /// Codes from AE that provide additional information about the participants 
  /// in a program.
  /// </summary>
  [JsonPropertyName("aeProgramSubtype")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = AeProgramSubtype_MaxLength, Optional = true)]
  public string AeProgramSubtype
  {
    get => aeProgramSubtype;
    set => aeProgramSubtype = value != null
      ? TrimEnd(Substring(value, 1, AeProgramSubtype_MaxLength)) : null;
  }

  /// <summary>Length of the PARTICIPATION_CODE attribute.</summary>
  public const int ParticipationCode_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPATION_CODE attribute.
  /// Identifies if a CSE person is currently participating is a AE or KSCARES 
  /// program or if that program is closed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ParticipationCode_MaxLength)]
  public string ParticipationCode
  {
    get => participationCode ?? "";
    set => participationCode =
      TrimEnd(Substring(value, 1, ParticipationCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ParticipationCode attribute.</summary>
  [JsonPropertyName("participationCode")]
  [Computed]
  public string ParticipationCode_Json
  {
    get => NullIf(ParticipationCode, "");
    set => ParticipationCode = value;
  }

  /// <summary>Length of the PA_CASE_NUMBER attribute.</summary>
  public const int PaCaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PA_CASE_NUMBER attribute.
  /// The number assigned by AE or KSCARES for their case.
  /// </summary>
  [JsonPropertyName("paCaseNumber")]
  [Member(Index = 16, Type = MemberType.Char, Length = PaCaseNumber_MaxLength, Optional
    = true)]
  public string PaCaseNumber
  {
    get => paCaseNumber;
    set => paCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, PaCaseNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MED_TYPE_DISCONTINUE_DATE attribute.
  /// RESP: KESSEP
  /// 
  /// The date the EM or WT program closed in the automated eligibility
  /// system.
  /// </summary>
  [JsonPropertyName("medTypeDiscontinueDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
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
  [Member(Index = 18, Type = MemberType.Char, Length = MedType_MaxLength, Optional
    = true)]
  public string MedType
  {
    get => medType;
    set => medType = value != null
      ? TrimEnd(Substring(value, 1, MedType_MaxLength)) : null;
  }

  private string recordType;
  private string sourceOfFunds;
  private string programCode;
  private string csePersonNumber;
  private string statusInd;
  private string closureReason;
  private string from;
  private DateTime? progEffectiveDate;
  private DateTime? programEndDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private DateTime? processDate;
  private DateTime? assignedDate;
  private string aeProgramSubtype;
  private string participationCode;
  private string paCaseNumber;
  private DateTime? medTypeDiscontinueDate;
  private string medType;
}

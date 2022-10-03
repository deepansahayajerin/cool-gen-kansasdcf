// The source file: PROGRAM_ERROR, ID: 371439805, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This table will contain errors that were encountered during batch program 
/// runs.  Each row should contain information that will identify the record
/// being processed and what type of error occurred.
/// </summary>
[Serializable]
public partial class ProgramError: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramError()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramError(ProgramError that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramError Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramError that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    keyInfo = that.keyInfo;
    code = that.code;
    statusInd = that.statusInd;
    statusDate = that.statusDate;
    resolution = that.resolution;
    programError1 = that.programError1;
    prrStartTstamp = that.prrStartTstamp;
    ppiCreatedTstamp = that.ppiCreatedTstamp;
    ppiName = that.ppiName;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the KEY_INFO attribute.</summary>
  public const int KeyInfo_MaxLength = 254;

  /// <summary>
  /// The value of the KEY_INFO attribute.
  /// This attribute will contain the key information that can be used to point 
  /// back to the record/row that was being processed when an error occurred.
  /// </summary>
  [JsonPropertyName("keyInfo")]
  [Member(Index = 2, Type = MemberType.Char, Length = KeyInfo_MaxLength, Optional
    = true)]
  public string KeyInfo
  {
    get => keyInfo;
    set => keyInfo = value != null
      ? TrimEnd(Substring(value, 1, KeyInfo_MaxLength)) : null;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 8;

  /// <summary>
  /// The value of the CODE attribute.
  /// This attribute may contain an error code that represents the kind of error
  /// occurred.
  /// To look this code up for a more detailed description use the following 
  /// tables in the Design Objects subject area:
  /// CODE tbl ID attribute of &quot;Batch Error&quot;
  /// CODE_VALUE tbl ID attribute = JOB_ERROR tbl CODE attribute
  /// </summary>
  [JsonPropertyName("code")]
  [Member(Index = 3, Type = MemberType.Char, Length = Code_MaxLength, Optional
    = true)]
  public string Code
  {
    get => code;
    set => code = value != null
      ? TrimEnd(Substring(value, 1, Code_MaxLength)) : null;
  }

  /// <summary>Length of the STATUS_IND attribute.</summary>
  public const int StatusInd_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS_IND attribute.
  /// This attribute may contain an indicator that will show if this error has 
  /// been taken care of.
  /// Example:
  /// If we are processing a file of transactions and we encounter an error on 
  /// transaction number 100 we may write this out to this table with a status
  /// ind of 'U' for unresolved.  Once this transaction record has been examined
  /// and resolved then we would update the status ind to 'R' for resolved.
  /// </summary>
  [JsonPropertyName("statusInd")]
  [Member(Index = 4, Type = MemberType.Char, Length = StatusInd_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("R")]
  [Value("U")]
  public string StatusInd
  {
    get => statusInd;
    set => statusInd = value != null
      ? TrimEnd(Substring(value, 1, StatusInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// This attribute may be used to show when an error row was set to its 
  /// current status.
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? StatusDate
  {
    get => statusDate;
    set => statusDate = value;
  }

  /// <summary>Length of the RESOLUTION attribute.</summary>
  public const int Resolution_MaxLength = 240;

  /// <summary>
  /// The value of the RESOLUTION attribute.
  /// A description of the resolution taken on this program error.  It will be 
  /// supplied by the DIR staff member working control reports.
  /// </summary>
  [JsonPropertyName("resolution")]
  [Member(Index = 6, Type = MemberType.Varchar, Length = Resolution_MaxLength, Optional
    = true)]
  public string Resolution
  {
    get => resolution;
    set => resolution = value != null
      ? Substring(value, 1, Resolution_MaxLength) : null;
  }

  /// <summary>Length of the PROGRAM_ERROR attribute.</summary>
  public const int ProgramError1_MaxLength = 80;

  /// <summary>
  /// The value of the PROGRAM_ERROR attribute.
  /// This attribute will contain a detailed description of the error that 
  /// occurred.  This message may supplement or replace the message contained on
  /// the CODE_VALUE table.
  /// </summary>
  [JsonPropertyName("programError1")]
  [Member(Index = 7, Type = MemberType.Char, Length = ProgramError1_MaxLength, Optional
    = true)]
  public string ProgramError1
  {
    get => programError1;
    set => programError1 = value != null
      ? TrimEnd(Substring(value, 1, ProgramError1_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the START_TIMESTAMP attribute.
  /// The date and time that the program was executed.
  /// </summary>
  [JsonPropertyName("prrStartTstamp")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? PrrStartTstamp
  {
    get => prrStartTstamp;
    set => prrStartTstamp = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("ppiCreatedTstamp")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? PpiCreatedTstamp
  {
    get => ppiCreatedTstamp;
    set => ppiCreatedTstamp = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int PpiName_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the program (from the exec card statement in the JCL) that is 
  /// to be run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = PpiName_MaxLength)]
  public string PpiName
  {
    get => ppiName ?? "";
    set => ppiName = TrimEnd(Substring(value, 1, PpiName_MaxLength));
  }

  /// <summary>
  /// The json value of the PpiName attribute.</summary>
  [JsonPropertyName("ppiName")]
  [Computed]
  public string PpiName_Json
  {
    get => NullIf(PpiName, "");
    set => PpiName = value;
  }

  private int systemGeneratedIdentifier;
  private string keyInfo;
  private string code;
  private string statusInd;
  private DateTime? statusDate;
  private string resolution;
  private string programError1;
  private DateTime? prrStartTstamp;
  private DateTime? ppiCreatedTstamp;
  private string ppiName;
}

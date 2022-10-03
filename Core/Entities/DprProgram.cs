// The source file: DPR_PROGRAM, ID: 371434034, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity resolves the many-many relationship between DISTRIBUTION POLICY 
/// RULE and PROGRAM. It PROGRAM. It specifies the apploicable Programs for a
/// Distribution Policy Rule.
/// </summary>
[Serializable]
public partial class DprProgram: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DprProgram()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DprProgram(DprProgram that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DprProgram Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DprProgram that)
  {
    base.Assign(that);
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    assistanceInd = that.assistanceInd;
    programState = that.programState;
    dprGeneratedId = that.dprGeneratedId;
    dbpGeneratedId = that.dbpGeneratedId;
    prgGeneratedId = that.prgGeneratedId;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the ASSISTANCE_IND attribute.</summary>
  public const int AssistanceInd_MaxLength = 1;

  /// <summary>
  /// The value of the ASSISTANCE_IND attribute.
  /// Identifies whether the distribution policy rule applies to supported 
  /// persons who are currently TAF, non-TAF, or both.  Values are 'T' (TAF only
  /// ), 'N' (non-TAF only) or space (applies to both TAF and non-TAF).
  /// </summary>
  [JsonPropertyName("assistanceInd")]
  [Member(Index = 3, Type = MemberType.Char, Length = AssistanceInd_MaxLength, Optional
    = true)]
  public string AssistanceInd
  {
    get => assistanceInd;
    set => assistanceInd = value != null
      ? TrimEnd(Substring(value, 1, AssistanceInd_MaxLength)) : null;
  }

  /// <summary>Length of the PROGRAM_STATE attribute.</summary>
  public const int ProgramState_MaxLength = 2;

  /// <summary>
  /// The value of the PROGRAM_STATE attribute.
  /// This attribute will support the stae value of a program in 
  /// relation to the distribution policy. The valid values are as
  /// follows.          UP , Unassigned Pre-Assistance
  /// UD, Unassigned During Assistance
  /// CA , Conditionally
  /// Assigned                                                 NA,
  /// Never Assigned
  /// 
  /// PA , Permanently Assigned
  /// TA ,
  /// Temporarily Assigned
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ProgramState_MaxLength)]
  public string ProgramState
  {
    get => programState ?? "";
    set => programState = TrimEnd(Substring(value, 1, ProgramState_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramState attribute.</summary>
  [JsonPropertyName("programState")]
  [Computed]
  public string ProgramState_Json
  {
    get => NullIf(ProgramState, "");
    set => ProgramState = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dprGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int DprGeneratedId
  {
    get => dprGeneratedId;
    set => dprGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbpGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int DbpGeneratedId
  {
    get => dbpGeneratedId;
    set => dbpGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("prgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int PrgGeneratedId
  {
    get => prgGeneratedId;
    set => prgGeneratedId = value;
  }

  private string createdBy;
  private DateTime? createdTimestamp;
  private string assistanceInd;
  private string programState;
  private int dprGeneratedId;
  private int dbpGeneratedId;
  private int prgGeneratedId;
}

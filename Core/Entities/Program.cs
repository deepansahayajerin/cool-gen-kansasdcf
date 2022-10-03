// The source file: PROGRAM, ID: 371439656, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Scope: A categorization of CSE CASEs, mandated by the federal government, 
/// based on receipt or non-receipt of public assistance, to determine the type
/// of services available, the source of funds (federal or state), and
/// applicable policies and regulations.
/// In the federal regulations, this is called CASE TYPE. It is important to 
/// help determine share of federal$ vs State$ (different splits on different
/// case types).
/// Examples:
/// Non-ADC
/// Non-ADC Transitional Child Care
/// ADC
/// ADC Arrears only
/// GA Foster Care
/// GA Foster Care Arrears only
/// ADC Foster Care
/// ADC Foster Care Arrears only
/// Non-ADC Medicaid only
/// Non-ADC Food Stamp
/// In Addition, depending on the relationship to Location, any of the above may
/// be Interstate or Instate.  Interstate programs are programs on cases
/// received from another state.  Instate programs are programs on cases
/// originated and worked in Kansas.
/// </summary>
[Serializable]
public partial class Program: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Program()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Program(Program that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Program Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Program that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    title = that.title;
    distributionProgramType = that.distributionProgramType;
    interstateIndicator = that.interstateIndicator;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 3;

  /// <summary>
  /// The value of the CODE attribute.
  /// the code describing the program
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => code ?? "";
    set => code = TrimEnd(Substring(value, 1, Code_MaxLength));
  }

  /// <summary>
  /// The json value of the Code attribute.</summary>
  [JsonPropertyName("code")]
  [Computed]
  public string Code_Json
  {
    get => NullIf(Code, "");
    set => Code = value;
  }

  /// <summary>Length of the TITLE attribute.</summary>
  public const int Title_MaxLength = 30;

  /// <summary>
  /// The value of the TITLE attribute.
  /// This will indicate the type of public assistance program involvement or if
  /// there is no public assistance involvement (Non-ADC).  Pulic assistance
  /// program types include ADC, GA, Food Stamps, Medical Assistance, Child
  /// Care, Foster Care and JOBS.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Title_MaxLength)]
  public string Title
  {
    get => title ?? "";
    set => title = TrimEnd(Substring(value, 1, Title_MaxLength));
  }

  /// <summary>
  /// The json value of the Title attribute.</summary>
  [JsonPropertyName("title")]
  [Computed]
  public string Title_Json
  {
    get => NullIf(Title, "");
    set => Title = value;
  }

  /// <summary>Length of the DISTRIBUTION_PROGRAM_TYPE attribute.</summary>
  public const int DistributionProgramType_MaxLength = 2;

  /// <summary>
  /// The value of the DISTRIBUTION_PROGRAM_TYPE attribute.
  /// This is to indicate whether there is federal funding participation in the 
  /// program.  The field will indicate either ADC, GA or will be blank to
  /// indicate Non-ADC.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = DistributionProgramType_MaxLength)]
  public string DistributionProgramType
  {
    get => distributionProgramType ?? "";
    set => distributionProgramType =
      TrimEnd(Substring(value, 1, DistributionProgramType_MaxLength));
  }

  /// <summary>
  /// The json value of the DistributionProgramType attribute.</summary>
  [JsonPropertyName("distributionProgramType")]
  [Computed]
  public string DistributionProgramType_Json
  {
    get => NullIf(DistributionProgramType, "");
    set => DistributionProgramType = value;
  }

  /// <summary>Length of the INTERSTATE_INDICATOR attribute.</summary>
  public const int InterstateIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the INTERSTATE_INDICATOR attribute.
  /// indicates if program is interstate or instate. Y=interstate; N=instate
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = InterstateIndicator_MaxLength)]
  public string InterstateIndicator
  {
    get => interstateIndicator ?? "";
    set => interstateIndicator =
      TrimEnd(Substring(value, 1, InterstateIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the InterstateIndicator attribute.</summary>
  [JsonPropertyName("interstateIndicator")]
  [Computed]
  public string InterstateIndicator_Json
  {
    get => NullIf(InterstateIndicator, "");
    set => InterstateIndicator = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// date program begins
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// date program ends.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
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
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 9, Type = MemberType.Timestamp)]
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
  [Member(Index = 10, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
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
  [Member(Index = 11, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  private int systemGeneratedIdentifier;
  private string code;
  private string title;
  private string distributionProgramType;
  private string interstateIndicator;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
}

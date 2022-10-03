// The source file: INTERFACE_INCOME_NOTIFICATION, ID: 371435675, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE	
/// This entity will hold collection information which is passed monthly to the 
/// IV-A agency. The information is used by the KAECSES-AE system then to update
/// their system ADABAS files according to Income Maintenance's business rules.
/// Data from this file will be deleted periodically as needed to minimize disk 
/// space requirements and to improve performance.
/// </summary>
[Serializable]
public partial class InterfaceIncomeNotification: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfaceIncomeNotification()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfaceIncomeNotification(InterfaceIncomeNotification that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfaceIncomeNotification Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfaceIncomeNotification that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    supportedCsePersonNumber = that.supportedCsePersonNumber;
    obligorCsePersonNumber = that.obligorCsePersonNumber;
    caseNumber = that.caseNumber;
    collectionDate = that.collectionDate;
    collectionAmount = that.collectionAmount;
    personProgram = that.personProgram;
    programAppliedTo = that.programAppliedTo;
    appliedToCode = that.appliedToCode;
    distributionDate = that.distributionDate;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    processDate = that.processDate;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A random generated numeric value uniquely identifying the entity 
  /// occurence.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the SUPPORTED_CSE_PERSON_NUMBER attribute.</summary>
  public const int SupportedCsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the SUPPORTED_CSE_PERSON_NUMBER attribute.
  /// The person number the collection is in support of.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = SupportedCsePersonNumber_MaxLength)]
  public string SupportedCsePersonNumber
  {
    get => supportedCsePersonNumber ?? "";
    set => supportedCsePersonNumber =
      TrimEnd(Substring(value, 1, SupportedCsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportedCsePersonNumber attribute.</summary>
  [JsonPropertyName("supportedCsePersonNumber")]
  [Computed]
  public string SupportedCsePersonNumber_Json
  {
    get => NullIf(SupportedCsePersonNumber, "");
    set => SupportedCsePersonNumber = value;
  }

  /// <summary>Length of the OBLIGOR_CSE_PERSON_NUMBER attribute.</summary>
  public const int ObligorCsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_CSE_PERSON_NUMBER attribute.
  /// The person number of the obligor given credit for the collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = ObligorCsePersonNumber_MaxLength)]
  public string ObligorCsePersonNumber
  {
    get => obligorCsePersonNumber ?? "";
    set => obligorCsePersonNumber =
      TrimEnd(Substring(value, 1, ObligorCsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligorCsePersonNumber attribute.</summary>
  [JsonPropertyName("obligorCsePersonNumber")]
  [Computed]
  public string ObligorCsePersonNumber_Json
  {
    get => NullIf(ObligorCsePersonNumber, "");
    set => ObligorCsePersonNumber = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The CSE case number of the case the supported person was active on when 
  /// the collection was created.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
  public string CaseNumber
  {
    get => caseNumber ?? "";
    set => caseNumber = TrimEnd(Substring(value, 1, CaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseNumber attribute.</summary>
  [JsonPropertyName("caseNumber")]
  [Computed]
  public string CaseNumber_Json
  {
    get => NullIf(CaseNumber, "");
    set => CaseNumber = value;
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// The date the money is paid on behalf of the obligor.
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? CollectionDate
  {
    get => collectionDate;
    set => collectionDate = value;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// The amount of money applied to the obligation.
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal CollectionAmount
  {
    get => collectionAmount;
    set => collectionAmount = Truncate(value, 2);
  }

  /// <summary>Length of the PERSON_PROGRAM attribute.</summary>
  public const int PersonProgram_MaxLength = 3;

  /// <summary>
  /// The value of the PERSON_PROGRAM attribute.
  /// The CSE program the supported person was participating in when the 
  /// collection was distributed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = PersonProgram_MaxLength)]
  public string PersonProgram
  {
    get => personProgram ?? "";
    set => personProgram =
      TrimEnd(Substring(value, 1, PersonProgram_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonProgram attribute.</summary>
  [JsonPropertyName("personProgram")]
  [Computed]
  public string PersonProgram_Json
  {
    get => NullIf(PersonProgram, "");
    set => PersonProgram = value;
  }

  /// <summary>Length of the PROGRAM_APPLIED_TO attribute.</summary>
  public const int ProgramAppliedTo_MaxLength = 2;

  /// <summary>
  /// The value of the PROGRAM_APPLIED_TO attribute.
  /// Indicates the distribution program the collection was applied to at the 
  /// time of distribution.
  /// NA = NADC
  /// AD = ADC
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ProgramAppliedTo_MaxLength)
    ]
  public string ProgramAppliedTo
  {
    get => programAppliedTo ?? "";
    set => programAppliedTo =
      TrimEnd(Substring(value, 1, ProgramAppliedTo_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramAppliedTo attribute.</summary>
  [JsonPropertyName("programAppliedTo")]
  [Computed]
  public string ProgramAppliedTo_Json
  {
    get => NullIf(ProgramAppliedTo, "");
    set => ProgramAppliedTo = value;
  }

  /// <summary>Length of the APPLIED_TO_CODE attribute.</summary>
  public const int AppliedToCode_MaxLength = 1;

  /// <summary>
  /// The value of the APPLIED_TO_CODE attribute.
  /// Indicates whether the collection was applied to current support due, 
  /// support arrearage, accrued interest on the arrearage, or as a gift.
  /// C = Current
  /// A = Arrearage
  /// I = Interest
  /// G = Gift
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AppliedToCode_MaxLength)]
  public string AppliedToCode
  {
    get => appliedToCode ?? "";
    set => appliedToCode =
      TrimEnd(Substring(value, 1, AppliedToCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AppliedToCode attribute.</summary>
  [JsonPropertyName("appliedToCode")]
  [Computed]
  public string AppliedToCode_Json
  {
    get => NullIf(AppliedToCode, "");
    set => AppliedToCode = value;
  }

  /// <summary>
  /// The value of the DISTRIBUTION_DATE attribute.
  /// Indicates the date the collection was applied to the obligation.
  /// </summary>
  [JsonPropertyName("distributionDate")]
  [Member(Index = 10, Type = MemberType.Date)]
  public DateTime? DistributionDate
  {
    get => distributionDate;
    set => distributionDate = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// indicates the date and time this occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Indicates the module name of the program which created the occurrence of 
  /// this record.
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
  /// The value of the PROCESS_DATE attribute.
  /// Indicates whether this occurrence of the entity has been copied to ADABAS 
  /// by the NATURAL copy program. The date is set to nulls when the occurrence
  /// is created and has a valid date when the NATURAL copy program has copied
  /// it.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 13, Type = MemberType.Date)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  private int systemGeneratedIdentifier;
  private string supportedCsePersonNumber;
  private string obligorCsePersonNumber;
  private string caseNumber;
  private DateTime? collectionDate;
  private decimal collectionAmount;
  private string personProgram;
  private string programAppliedTo;
  private string appliedToCode;
  private DateTime? distributionDate;
  private DateTime? createdTimestamp;
  private string createdBy;
  private DateTime? processDate;
}

// The source file: CONTRACTOR_FEE_INFORMATION, ID: 371432679, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity contains information relevent to collection agency fees.  It was
/// created to hold the fee amount charged by a Collection Agency for
/// collecting money on behalf of CSE.  A collection agency may charge different
/// rates over time.
/// </summary>
[Serializable]
public partial class ContractorFeeInformation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ContractorFeeInformation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ContractorFeeInformation(ContractorFeeInformation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ContractorFeeInformation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ContractorFeeInformation that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    rate = that.rate;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    distributionProgramType = that.distributionProgramType;
    judicialDistrict = that.judicialDistrict;
    otyId = that.otyId;
    offId = that.offId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the RATE attribute.
  /// The fee rate that a Collection Agency Vendor charges on a collection.
  /// </summary>
  [JsonPropertyName("rate")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 4, Precision = 2)]
  public decimal Rate
  {
    get => rate;
    set => rate = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the DISTRIBUTION_PROGRAM_TYPE attribute.</summary>
  public const int DistributionProgramType_MaxLength = 2;

  /// <summary>
  /// The value of the DISTRIBUTION_PROGRAM_TYPE attribute.
  /// Represents a catagory of programs.  This attribute is &quot;denormalized
  /// &quot; from the Distribution_Program_type attribute in program.
  /// 	Example: AF - AFDC
  /// 		 NA - Non-AFDC
  /// 		 FC - Foster Care
  /// 		 NF - Non-AFDC Foster Care
  /// 		 blank - represents all 				distribution program types
  /// </summary>
  [JsonPropertyName("distributionProgramType")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = DistributionProgramType_MaxLength, Optional = true)]
  public string DistributionProgramType
  {
    get => distributionProgramType;
    set => distributionProgramType = value != null
      ? TrimEnd(Substring(value, 1, DistributionProgramType_MaxLength)) : null;
  }

  /// <summary>Length of the JUDICIAL_DISTRICT attribute.</summary>
  public const int JudicialDistrict_MaxLength = 5;

  /// <summary>
  /// The value of the JUDICIAL_DISTRICT attribute.
  /// This attribute specifies the Judicial District to which tribunals belong.
  /// The state is divided into a number of Judicial Districts.  Each Judicial
  /// District consists of one or more county courts.
  /// </summary>
  [JsonPropertyName("judicialDistrict")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = JudicialDistrict_MaxLength, Optional = true)]
  public string JudicialDistrict
  {
    get => judicialDistrict;
    set => judicialDistrict = value != null
      ? TrimEnd(Substring(value, 1, JudicialDistrict_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 11, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 4)]
  public int OffId
  {
    get => offId;
    set => offId = value;
  }

  private int systemGeneratedIdentifier;
  private decimal rate;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string distributionProgramType;
  private string judicialDistrict;
  private int? otyId;
  private int offId;
}

// The source file: PCA_FUND_EXPLOSION_RULE, ID: 371439243, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// The PCA explosion rule converts a supplied PCA code into the affect on a 
/// specific fund based upon percentage of a transaction.
/// 	
/// An example is for the quarterly ADC fund transfer, the money out of the 
/// clearing fund is debited to two funds to split the federal share from the
/// state share.  This Debit is coded with pca 30120 which puts 41.10% in fund
/// 1000 and 58.9% in fund 3441.
/// </summary>
[Serializable]
public partial class PcaFundExplosionRule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PcaFundExplosionRule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PcaFundExplosionRule(PcaFundExplosionRule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PcaFundExplosionRule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PcaFundExplosionRule that)
  {
    base.Assign(that);
    indexNumber = that.indexNumber;
    distributionPct = that.distributionPct;
    federalFiscalYr = that.federalFiscalYr;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    funIdentifier = that.funIdentifier;
    pcaEffectiveDate = that.pcaEffectiveDate;
    pcaCode = that.pcaCode;
  }

  /// <summary>
  /// The value of the INDEX_NUMBER attribute.
  /// The index represents a further categorization of monies within a fund.  It
  /// could be thought of as a sub-fund.
  /// </summary>
  [JsonPropertyName("indexNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int IndexNumber
  {
    get => indexNumber;
    set => indexNumber = value;
  }

  /// <summary>
  /// The value of the DISTRIBUTION_PCT attribute.
  /// The percentage of the funding amount that is to affect the associated 
  /// fund.
  /// </summary>
  [JsonPropertyName("distributionPct")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 8, Precision = 5)]
  public decimal DistributionPct
  {
    get => distributionPct;
    set => distributionPct = Truncate(value, 5);
  }

  /// <summary>
  /// The value of the FEDERAL_FISCAL_YR attribute.
  /// The federal fiscal year that these funds are to be counted toward.
  /// </summary>
  [JsonPropertyName("federalFiscalYr")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int FederalFiscalYr
  {
    get => federalFiscalYr;
    set => federalFiscalYr = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("funIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int FunIdentifier
  {
    get => funIdentifier;
    set => funIdentifier = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("pcaEffectiveDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? PcaEffectiveDate
  {
    get => pcaEffectiveDate;
    set => pcaEffectiveDate = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int PcaCode_MaxLength = 5;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation  for the purpose of quick identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = PcaCode_MaxLength)]
  public string PcaCode
  {
    get => pcaCode ?? "";
    set => pcaCode = TrimEnd(Substring(value, 1, PcaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PcaCode attribute.</summary>
  [JsonPropertyName("pcaCode")]
  [Computed]
  public string PcaCode_Json
  {
    get => NullIf(PcaCode, "");
    set => PcaCode = value;
  }

  private int indexNumber;
  private decimal distributionPct;
  private int federalFiscalYr;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int funIdentifier;
  private DateTime? pcaEffectiveDate;
  private string pcaCode;
}

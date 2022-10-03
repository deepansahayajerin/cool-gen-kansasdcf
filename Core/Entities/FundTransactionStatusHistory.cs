// The source file: FUND_TRANSACTION_STATUS_HISTORY, ID: 371434828, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Provides an audit trail representing each period in time the Fund 
/// Transaction was in a particular state.
/// </summary>
[Serializable]
public partial class FundTransactionStatusHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FundTransactionStatusHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FundTransactionStatusHistory(FundTransactionStatusHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FundTransactionStatusHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FundTransactionStatusHistory that)
  {
    base.Assign(that);
    effectiveTmst = that.effectiveTmst;
    reasonText = that.reasonText;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    ftrIdentifier = that.ftrIdentifier;
    funIdentifier = that.funIdentifier;
    fttIdentifier = that.fttIdentifier;
    pcaEffectiveDate = that.pcaEffectiveDate;
    pcaCode = that.pcaCode;
    ftsIdentifier = that.ftsIdentifier;
  }

  /// <summary>
  /// The value of the EFFECTIVE_TMST attribute.
  /// Defines the effective timestamp for a specific state of a cash receipt.
  /// </summary>
  [JsonPropertyName("effectiveTmst")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? EffectiveTmst
  {
    get => effectiveTmst;
    set => effectiveTmst = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// May contain some reason why the entity was put in a particular status if 
  /// outside normal business circumstances.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 2, Type = MemberType.Varchar, Length = ReasonText_MaxLength, Optional
    = true)]
  public string ReasonText
  {
    get => reasonText;
    set => reasonText = value != null
      ? Substring(value, 1, ReasonText_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique number that specifies each occurrence of the entity type.
  /// </summary>
  [JsonPropertyName("ftrIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 9)]
  public int FtrIdentifier
  {
    get => ftrIdentifier;
    set => ftrIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("funIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 3)]
  public int FunIdentifier
  {
    get => funIdentifier;
    set => funIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("fttIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int FttIdentifier
  {
    get => fttIdentifier;
    set => fttIdentifier = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("pcaEffectiveDate")]
  [Member(Index = 8, Type = MemberType.Date)]
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
  [Member(Index = 9, Type = MemberType.Char, Length = PcaCode_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system, generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("ftsIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int FtsIdentifier
  {
    get => ftsIdentifier;
    set => ftsIdentifier = value;
  }

  private DateTime? effectiveTmst;
  private string reasonText;
  private string createdBy;
  private DateTime? createdTimestamp;
  private int ftrIdentifier;
  private int funIdentifier;
  private int fttIdentifier;
  private DateTime? pcaEffectiveDate;
  private string pcaCode;
  private int ftsIdentifier;
}

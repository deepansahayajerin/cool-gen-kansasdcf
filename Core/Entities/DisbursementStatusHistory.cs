// The source file: DISBURSEMENT_STATUS_HISTORY, ID: 371433631, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Provides an audit trail representing each period in time the Disbursement 
/// was in a particular state.  This information is derived thru the
/// relationship to Disbursement and Disbursement_Status.
/// </summary>
[Serializable]
public partial class DisbursementStatusHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbursementStatusHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbursementStatusHistory(DisbursementStatusHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbursementStatusHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisbursementStatusHistory that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    reasonText = that.reasonText;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    suppressionReason = that.suppressionReason;
    dtrGeneratedId = that.dtrGeneratedId;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
    dbsGeneratedId = that.dbsGeneratedId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number that distinguishes one occurrence
  /// of the entity type from another.
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
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
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
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// May contain some reason why the entity was changed or deleted.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 4, Type = MemberType.Varchar, Length = ReasonText_MaxLength, Optional
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
  /// The timestamp the occurrence was created.  This is also the date that the 
  /// status history record becomes effective.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the SUPPRESSION_REASON attribute.</summary>
  public const int SuppressionReason_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPRESSION_REASON attribute.
  /// This field will contain the reason for suppressing the disbursement. If 
  /// the disbursement is not suppressed then this field will contain a blank.
  /// The reason will be derived from the suppression rule that was used to
  /// apply the suppression. If more than one rule could have been applied to
  /// the suppression then the reason will be set to the suppression rule with
  /// the largest discontinue date since that rule overrides all other rules for
  /// settling the discontinue date.
  /// </summary>
  [JsonPropertyName("suppressionReason")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = SuppressionReason_MaxLength, Optional = true)]
  public string SuppressionReason
  {
    get => suppressionReason;
    set => suppressionReason = value != null
      ? TrimEnd(Substring(value, 1, SuppressionReason_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("dtrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int DtrGeneratedId
  {
    get => dtrGeneratedId;
    set => dtrGeneratedId = value;
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
  [Member(Index = 9, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbsGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int DbsGeneratedId
  {
    get => dbsGeneratedId;
    set => dbsGeneratedId = value;
  }

  private int systemGeneratedIdentifier;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string reasonText;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string suppressionReason;
  private int dtrGeneratedId;
  private string cspNumber;
  private string cpaType;
  private int dbsGeneratedId;
}

// The source file: DISB_SUPPRESSION_STATUS_HISTORY, ID: 371433541, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This table will record the dates that disbursements were suppressed for an 
/// obligee.  Disbursement suppression can be set up for a future time period if
/// it is known that we do not want to sends checks out during certain time
/// periods.
/// </summary>
[Serializable]
public partial class DisbSuppressionStatusHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbSuppressionStatusHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbSuppressionStatusHistory(DisbSuppressionStatusHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbSuppressionStatusHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisbSuppressionStatusHistory that)
  {
    base.Assign(that);
    type1 = that.type1;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    reasonText = that.reasonText;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    excessUraFiller = that.excessUraFiller;
    duplicateFiller = that.duplicateFiller;
    courtOrderSuppr = that.courtOrderSuppr;
    collectionFiller = that.collectionFiller;
    personDisbFiller = that.personDisbFiller;
    automaticFiller = that.automaticFiller;
    xuraFiller = that.xuraFiller;
    deceasedFiller = that.deceasedFiller;
    noActiveAddressFiller = that.noActiveAddressFiller;
    lgaIdentifier = that.lgaIdentifier;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    cltSequentialId = that.cltSequentialId;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Divides the entity into the 2 following subtypes:	
  /// Person_Disb_Suppression
  /// Collection_Type_Disb_Suppression
  /// Example:  P (for Person)
  ///           C (for Collection Type)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("D")]
  [Value("O")]
  [Value("P")]
  [Value("C")]
  [Value("A")]
  [Value("X")]
  [Value("Y")]
  [Value("Z")]
  [ImplicitValue("D")]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number that distinguishes one occurrence
  /// of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// May contain some reason why disbursements were suppressed for the Obligee.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = ReasonText_MaxLength, Optional
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
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 7, Type = MemberType.Timestamp)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the EXCESS_URA_FILLER attribute.</summary>
  public const int ExcessUraFiller_MaxLength = 1;

  /// <summary>
  /// The value of the EXCESS_URA_FILLER attribute.
  /// This attribute is needed because IEF requires each subtype to have at 
  /// least one unique attribute.
  /// </summary>
  [JsonPropertyName("excessUraFiller")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ExcessUraFiller_MaxLength, Optional = true)]
  public string ExcessUraFiller
  {
    get => excessUraFiller;
    set => excessUraFiller = value != null
      ? TrimEnd(Substring(value, 1, ExcessUraFiller_MaxLength)) : null;
  }

  /// <summary>Length of the DUPLICATE_FILLER attribute.</summary>
  public const int DuplicateFiller_MaxLength = 1;

  /// <summary>
  /// The value of the DUPLICATE_FILLER attribute.
  /// </summary>
  [JsonPropertyName("duplicateFiller")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = DuplicateFiller_MaxLength, Optional = true)]
  public string DuplicateFiller
  {
    get => duplicateFiller;
    set => duplicateFiller = value != null
      ? TrimEnd(Substring(value, 1, DuplicateFiller_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_ORDER_SUPPR attribute.</summary>
  public const int CourtOrderSuppr_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_ORDER_SUPPR attribute.
  /// </summary>
  [JsonPropertyName("courtOrderSuppr")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CourtOrderSuppr_MaxLength, Optional = true)]
  public string CourtOrderSuppr
  {
    get => courtOrderSuppr;
    set => courtOrderSuppr = value != null
      ? TrimEnd(Substring(value, 1, CourtOrderSuppr_MaxLength)) : null;
  }

  /// <summary>Length of the COLLECTION_FILLER attribute.</summary>
  public const int CollectionFiller_MaxLength = 1;

  /// <summary>
  /// The value of the COLLECTION_FILLER attribute.
  /// This attribute is needed because IEF requires each subtype to have at 
  /// least one unique attribute.
  /// </summary>
  [JsonPropertyName("collectionFiller")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = CollectionFiller_MaxLength, Optional = true)]
  public string CollectionFiller
  {
    get => collectionFiller;
    set => collectionFiller = value != null
      ? TrimEnd(Substring(value, 1, CollectionFiller_MaxLength)) : null;
  }

  /// <summary>Length of the PERSON_DISB_FILLER attribute.</summary>
  public const int PersonDisbFiller_MaxLength = 1;

  /// <summary>
  /// The value of the PERSON_DISB_FILLER attribute.
  /// This attribute is needed because IEF requires each subtype to have at 
  /// least one unique attribute.
  /// </summary>
  [JsonPropertyName("personDisbFiller")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = PersonDisbFiller_MaxLength, Optional = true)]
  public string PersonDisbFiller
  {
    get => personDisbFiller;
    set => personDisbFiller = value != null
      ? TrimEnd(Substring(value, 1, PersonDisbFiller_MaxLength)) : null;
  }

  /// <summary>Length of the AUTOMATIC_FILLER attribute.</summary>
  public const int AutomaticFiller_MaxLength = 1;

  /// <summary>
  /// The value of the AUTOMATIC_FILLER attribute.
  /// In keeping with the other subtypes on this entity, a subtype must have a 
  /// least one attribute.
  /// </summary>
  [JsonPropertyName("automaticFiller")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = AutomaticFiller_MaxLength, Optional = true)]
  public string AutomaticFiller
  {
    get => automaticFiller;
    set => automaticFiller = value != null
      ? TrimEnd(Substring(value, 1, AutomaticFiller_MaxLength)) : null;
  }

  /// <summary>Length of the XURA_FILLER attribute.</summary>
  public const int XuraFiller_MaxLength = 1;

  /// <summary>
  /// The value of the XURA_FILLER attribute.
  /// </summary>
  [JsonPropertyName("xuraFiller")]
  [Member(Index = 16, Type = MemberType.Char, Length = XuraFiller_MaxLength, Optional
    = true)]
  public string XuraFiller
  {
    get => xuraFiller;
    set => xuraFiller = value != null
      ? TrimEnd(Substring(value, 1, XuraFiller_MaxLength)) : null;
  }

  /// <summary>Length of the DECEASED_FILLER attribute.</summary>
  public const int DeceasedFiller_MaxLength = 1;

  /// <summary>
  /// The value of the DECEASED_FILLER attribute.
  /// In keeping with the other subtypes on this entity, a subtype must have a 
  /// least one attribute.
  /// </summary>
  [JsonPropertyName("deceasedFiller")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = DeceasedFiller_MaxLength, Optional = true)]
  public string DeceasedFiller
  {
    get => deceasedFiller;
    set => deceasedFiller = value != null
      ? TrimEnd(Substring(value, 1, DeceasedFiller_MaxLength)) : null;
  }

  /// <summary>Length of the NO_ACTIVE_ADDRESS_FILLER attribute.</summary>
  public const int NoActiveAddressFiller_MaxLength = 1;

  /// <summary>
  /// The value of the NO_ACTIVE_ADDRESS_FILLER attribute.
  /// </summary>
  [JsonPropertyName("noActiveAddressFiller")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = NoActiveAddressFiller_MaxLength, Optional = true)]
  public string NoActiveAddressFiller
  {
    get => noActiveAddressFiller;
    set => noActiveAddressFiller = value != null
      ? TrimEnd(Substring(value, 1, NoActiveAddressFiller_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
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
  [Member(Index = 20, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cltSequentialId")]
  [Member(Index = 22, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CltSequentialId
  {
    get => cltSequentialId;
    set => cltSequentialId = value;
  }

  private string type1;
  private int systemGeneratedIdentifier;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string reasonText;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string excessUraFiller;
  private string duplicateFiller;
  private string courtOrderSuppr;
  private string collectionFiller;
  private string personDisbFiller;
  private string automaticFiller;
  private string xuraFiller;
  private string deceasedFiller;
  private string noActiveAddressFiller;
  private int? lgaIdentifier;
  private string cpaType;
  private string cspNumber;
  private int? cltSequentialId;
}

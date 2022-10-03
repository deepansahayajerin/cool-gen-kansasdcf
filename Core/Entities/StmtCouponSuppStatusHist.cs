﻿// The source file: STMT_COUPON_SUPP_STATUS_HIST, ID: 371440322, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINAN
/// Provides a means of suppressing statements and/or coupons for a person or a 
/// specific obligation.
/// </summary>
[Serializable]
public partial class StmtCouponSuppStatusHist: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StmtCouponSuppStatusHist()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StmtCouponSuppStatusHist(StmtCouponSuppStatusHist that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StmtCouponSuppStatusHist Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StmtCouponSuppStatusHist that)
  {
    base.Assign(that);
    docTypeToSuppress = that.docTypeToSuppress;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    type1 = that.type1;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    reasonText = that.reasonText;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    obligorFiller = that.obligorFiller;
    obligationFiller = that.obligationFiller;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    obgId = that.obgId;
    cspNumberOblig = that.cspNumberOblig;
    cpaTypeOblig = that.cpaTypeOblig;
    otyId = that.otyId;
  }

  /// <summary>Length of the DOC_TYPE_TO_SUPPRESS attribute.</summary>
  public const int DocTypeToSuppress_MaxLength = 1;

  /// <summary>
  /// The value of the DOC_TYPE_TO_SUPPRESS attribute.
  /// Determines the type of document to suppress.
  /// Values: &quot;S&quot; - Statement
  /// 	&quot;C&quot; - Coupon
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = DocTypeToSuppress_MaxLength)]
  [Value("C")]
  [Value("S")]
  public string DocTypeToSuppress
  {
    get => docTypeToSuppress ?? "";
    set => docTypeToSuppress =
      TrimEnd(Substring(value, 1, DocTypeToSuppress_MaxLength));
  }

  /// <summary>
  /// The json value of the DocTypeToSuppress attribute.</summary>
  [JsonPropertyName("docTypeToSuppress")]
  [Computed]
  public string DocTypeToSuppress_Json
  {
    get => NullIf(DocTypeToSuppress, "");
    set => DocTypeToSuppress = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each collection.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of relationship.  If the type is &quot;R&quot;, then the 
  /// suppression is tied to the Obligor.  If the type is &quot;O&quot;, then
  /// the suppression is tied to the Obligation.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Type1_MaxLength)]
  [Value("O")]
  [Value("R")]
  [ImplicitValue("O")]
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
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the suppression is in effect.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date that the suppression is no longer in effect.  The first date of 
  /// the discontinuance.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// A description of the reason for the suppression.
  /// </summary>
  [JsonPropertyName("reasonText")]
  [Member(Index = 6, Type = MemberType.Char, Length = ReasonText_MaxLength, Optional
    = true)]
  public string ReasonText
  {
    get => reasonText;
    set => reasonText = value != null
      ? TrimEnd(Substring(value, 1, ReasonText_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the OBLIGOR_FILLER attribute.</summary>
  public const int ObligorFiller_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGOR_FILLER attribute.
  /// FILLER - No value, just a place holder.
  /// </summary>
  [JsonPropertyName("obligorFiller")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ObligorFiller_MaxLength, Optional = true)]
  public string ObligorFiller
  {
    get => obligorFiller;
    set => obligorFiller = value != null
      ? TrimEnd(Substring(value, 1, ObligorFiller_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGATION_FILLER attribute.</summary>
  public const int ObligationFiller_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGATION_FILLER attribute.
  /// FILLER - No value, just a place holder.
  /// </summary>
  [JsonPropertyName("obligationFiller")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ObligationFiller_MaxLength, Optional = true)]
  public string ObligationFiller
  {
    get => obligationFiller;
    set => obligationFiller = value != null
      ? TrimEnd(Substring(value, 1, ObligationFiller_MaxLength)) : null;
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
  [Member(Index = 13, Type = MemberType.Char, Length = CpaType_MaxLength)]
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
  [Member(Index = 14, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [Member(Index = 15, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgId
  {
    get => obgId;
    set => obgId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberOblig_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumberOblig")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = CspNumberOblig_MaxLength, Optional = true)]
  public string CspNumberOblig
  {
    get => cspNumberOblig;
    set => cspNumberOblig = value != null
      ? TrimEnd(Substring(value, 1, CspNumberOblig_MaxLength)) : null;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaTypeOblig_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaTypeOblig")]
  [Member(Index = 17, Type = MemberType.Char, Length = CpaTypeOblig_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaTypeOblig
  {
    get => cpaTypeOblig;
    set => cpaTypeOblig = value != null
      ? TrimEnd(Substring(value, 1, CpaTypeOblig_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  private string docTypeToSuppress;
  private int systemGeneratedIdentifier;
  private string type1;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string reasonText;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string obligorFiller;
  private string obligationFiller;
  private string cpaType;
  private string cspNumber;
  private int? obgId;
  private string cspNumberOblig;
  private string cpaTypeOblig;
  private int? otyId;
}

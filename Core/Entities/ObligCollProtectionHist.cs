// The source file: OBLIG_COLL_PROTECTION_HIST, ID: 373381332, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINANCE
/// 
/// Defines the time frames in which Collection Protection was active for the
/// specified Obligation.
/// </summary>
[Serializable]
public partial class ObligCollProtectionHist: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligCollProtectionHist()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligCollProtectionHist(ObligCollProtectionHist that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligCollProtectionHist Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligCollProtectionHist that)
  {
    base.Assign(that);
    cvrdCollStrtDt = that.cvrdCollStrtDt;
    cvrdCollEndDt = that.cvrdCollEndDt;
    deactivationDate = that.deactivationDate;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    protectionLevel = that.protectionLevel;
    reasonText = that.reasonText;
    cpaType = that.cpaType;
    obgIdentifier = that.obgIdentifier;
    otyIdentifier = that.otyIdentifier;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the CVRD_COLL_STRT_DT attribute.
  /// Defines the start date for the period of time in which the Collections, 
  /// based on collection date, were covered by Collection Protection time frame
  /// for the specified Obligation.
  /// </summary>
  [JsonPropertyName("cvrdCollStrtDt")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? CvrdCollStrtDt
  {
    get => cvrdCollStrtDt;
    set => cvrdCollStrtDt = value;
  }

  /// <summary>
  /// The value of the CVRD_COLL_END_DT attribute.
  /// Defines the end date for the period of time in which the Collections, 
  /// based on collection date, were covered by Collection Protection time frame
  /// for the specified Obligation.
  /// </summary>
  [JsonPropertyName("cvrdCollEndDt")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? CvrdCollEndDt
  {
    get => cvrdCollEndDt;
    set => cvrdCollEndDt = value;
  }

  /// <summary>
  /// The value of the DEACTIVATION_DATE attribute.
  /// Defines the date in which the Collection Protection was deactivated for 
  /// the specified Obligation.
  /// </summary>
  [JsonPropertyName("deactivationDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? DeactivationDate
  {
    get => deactivationDate;
    set => deactivationDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Standard Description.
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
  /// The value of the CREATED_TMST attribute.
  /// Standard Description.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// Standard Description.
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
  /// Standard Description
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the PROTECTION_LEVEL attribute.</summary>
  public const int ProtectionLevel_MaxLength = 1;

  /// <summary>
  /// The value of the PROTECTION_LEVEL attribute.
  /// Describes the level of the Collection Protection to
  /// be given.   Examples:      C Apply to Current
  /// Support payments only.
  /// A Apply to arrears
  /// payments only.
  /// G Apply
  /// to gift payments only.
  /// 
  /// I Apply to interest payments only.
  /// SPACE
  /// Apply to all payments.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ProtectionLevel_MaxLength)]
    
  public string ProtectionLevel
  {
    get => protectionLevel ?? "";
    set => protectionLevel =
      TrimEnd(Substring(value, 1, ProtectionLevel_MaxLength));
  }

  /// <summary>
  /// The json value of the ProtectionLevel attribute.</summary>
  [JsonPropertyName("protectionLevel")]
  [Computed]
  public string ProtectionLevel_Json
  {
    get => NullIf(ProtectionLevel, "");
    set => ProtectionLevel = value;
  }

  /// <summary>Length of the REASON_TEXT attribute.</summary>
  public const int ReasonText_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TEXT attribute.
  /// Describes the reason for the collection protection time frame.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = ReasonText_MaxLength)]
  public string ReasonText
  {
    get => reasonText ?? "";
    set => reasonText = Substring(value, 1, ReasonText_MaxLength);
  }

  /// <summary>
  /// The json value of the ReasonText attribute.</summary>
  [JsonPropertyName("reasonText")]
  [Computed]
  public string ReasonText_Json
  {
    get => NullIf(ReasonText, "");
    set => ReasonText = value;
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
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int ObgIdentifier
  {
    get => obgIdentifier;
    set => obgIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int OtyIdentifier
  {
    get => otyIdentifier;
    set => otyIdentifier = value;
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
  [Member(Index = 13, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private DateTime? cvrdCollStrtDt;
  private DateTime? cvrdCollEndDt;
  private DateTime? deactivationDate;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string protectionLevel;
  private string reasonText;
  private string cpaType;
  private int obgIdentifier;
  private int otyIdentifier;
  private string cspNumber;
}

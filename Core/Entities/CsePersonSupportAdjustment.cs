// The source file: CSE_PERSON_SUPPORT_ADJUSTMENT, ID: 371433319, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// THIS CONTAINS THE ADJUSTMENT AMOUNT FOR A CSE PERSON SUPPORT WORKSHEET FOR A
/// PARTICULAR TYPE OF ADJUSTMENT.
/// </summary>
[Serializable]
public partial class CsePersonSupportAdjustment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonSupportAdjustment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonSupportAdjustment(CsePersonSupportAdjustment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonSupportAdjustment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonSupportAdjustment that)
  {
    base.Assign(that);
    adjustmentAmount = that.adjustmentAmount;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    csdNumber = that.csdNumber;
    cpsIdentifier = that.cpsIdentifier;
    cspNumber = that.cspNumber;
    casNumber = that.casNumber;
    croType = that.croType;
    croIdentifier = that.croIdentifier;
    cswIdentifier = that.cswIdentifier;
    cssGuidelineYr = that.cssGuidelineYr;
  }

  /// <summary>
  /// The value of the ADJUSTMENT_AMOUNT attribute.
  /// COURT ORDERED Allowed dollar amount, positive or negative, to the child 
  /// support.
  /// </summary>
  [JsonPropertyName("adjustmentAmount")]
  [Member(Index = 1, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? AdjustmentAmount
  {
    get => adjustmentAmount;
    set => adjustmentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is the sequence Number in which the adjustments are ordered.
  /// </summary>
  [JsonPropertyName("csdNumber")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 2)]
  public int CsdNumber
  {
    get => csdNumber;
    set => csdNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFER attribute.
  /// Identifier that indicates a particular child support worksheet for a cse 
  /// person.
  /// </summary>
  [JsonPropertyName("cpsIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int CpsIdentifier
  {
    get => cpsIdentifier;
    set => cpsIdentifier = value;
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
  [Member(Index = 8, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CasNumber_MaxLength)]
  public string CasNumber
  {
    get => casNumber ?? "";
    set => casNumber = TrimEnd(Substring(value, 1, CasNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNumber attribute.</summary>
  [JsonPropertyName("casNumber")]
  [Computed]
  public string CasNumber_Json
  {
    get => NullIf(CasNumber, "");
    set => CasNumber = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CroType_MaxLength)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroType
  {
    get => croType ?? "";
    set => croType = TrimEnd(Substring(value, 1, CroType_MaxLength));
  }

  /// <summary>
  /// The json value of the CroType attribute.</summary>
  [JsonPropertyName("croType")]
  [Computed]
  public string CroType_Json
  {
    get => NullIf(CroType, "");
    set => CroType = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int CroIdentifier
  {
    get => croIdentifier;
    set => croIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify a record.
  /// </summary>
  [JsonPropertyName("cswIdentifier")]
  [DefaultValue(0L)]
  [Member(Index = 12, Type = MemberType.Number, Length = 10)]
  public long CswIdentifier
  {
    get => cswIdentifier;
    set => cswIdentifier = value;
  }

  /// <summary>
  /// The value of the CS_GUIDELINE_YEAR attribute.
  /// The year the child support guidelines values are set.  This routinely 
  /// changes approximately every four years. This attribute is stored as a four
  /// character year like 2008, 2012, 2016.   Each time the guidelines change (
  /// every four years or so) the numbers will be entered onto the table along
  /// with the new values.  The existing values for prior years will remain.
  /// </summary>
  [JsonPropertyName("cssGuidelineYr")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 4)]
  public int CssGuidelineYr
  {
    get => cssGuidelineYr;
    set => cssGuidelineYr = value;
  }

  private decimal? adjustmentAmount;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int csdNumber;
  private int cpsIdentifier;
  private string cspNumber;
  private string casNumber;
  private string croType;
  private int croIdentifier;
  private long cswIdentifier;
  private int cssGuidelineYr;
}

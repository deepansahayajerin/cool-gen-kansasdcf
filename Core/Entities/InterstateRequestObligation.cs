// The source file: INTERSTATE_REQUEST_OBLIGATION, ID: 371436551, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Relates the Interstate Requests to the Orders they relate.
/// </summary>
[Serializable]
public partial class InterstateRequestObligation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterstateRequestObligation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterstateRequestObligation(InterstateRequestObligation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterstateRequestObligation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterstateRequestObligation that)
  {
    base.Assign(that);
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    orderFreqAmount = that.orderFreqAmount;
    orderEffectiveDate = that.orderEffectiveDate;
    orderEndDate = that.orderEndDate;
    intGeneratedId = that.intGeneratedId;
    otyType = that.otyType;
    obgGeneratedId = that.obgGeneratedId;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the ORDER_FREQ_AMOUNT attribute.
  /// Amount of each payment
  /// </summary>
  [JsonPropertyName("orderFreqAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 3, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal OrderFreqAmount
  {
    get => orderFreqAmount;
    set => orderFreqAmount = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the ORDER_EFFECTIVE_DATE attribute.
  /// Date obigations start to accrue
  /// </summary>
  [JsonPropertyName("orderEffectiveDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? OrderEffectiveDate
  {
    get => orderEffectiveDate;
    set => orderEffectiveDate = value;
  }

  /// <summary>
  /// The value of the ORDER_END_DATE attribute.
  /// Order this Order ends
  /// </summary>
  [JsonPropertyName("orderEndDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? OrderEndDate
  {
    get => orderEndDate;
    set => orderEndDate = value;
  }

  /// <summary>
  /// The value of the INT_H_GENERATED_ID attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("intGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 8)]
  public int IntGeneratedId
  {
    get => intGeneratedId;
    set => intGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int OtyType
  {
    get => otyType;
    set => otyType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
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

  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private decimal orderFreqAmount;
  private DateTime? orderEffectiveDate;
  private DateTime? orderEndDate;
  private int intGeneratedId;
  private int otyType;
  private int obgGeneratedId;
  private string cspNumber;
  private string cpaType;
}

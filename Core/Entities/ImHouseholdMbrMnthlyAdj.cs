// The source file: IM_HOUSEHOLD_MBR_MNTHLY_ADJ, ID: 374416378, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  OBLGESTB
/// 
/// This entity tracks URA adjustments for a specific member of a IM
/// Households by month.
/// </summary>
[Serializable]
public partial class ImHouseholdMbrMnthlyAdj: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ImHouseholdMbrMnthlyAdj()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ImHouseholdMbrMnthlyAdj(ImHouseholdMbrMnthlyAdj that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ImHouseholdMbrMnthlyAdj Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ImHouseholdMbrMnthlyAdj that)
  {
    base.Assign(that);
    type1 = that.type1;
    adjustmentAmount = that.adjustmentAmount;
    levelAppliedTo = that.levelAppliedTo;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    adjustmentReason = that.adjustmentReason;
    imsYear = that.imsYear;
    imsMonth = that.imsMonth;
    imhAeCaseNo = that.imhAeCaseNo;
    cspNumber = that.cspNumber;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This attribute describes what URA balances will be impacted by the 
  /// adjustment. The values are:
  /// A=Applied to the AF/FC URA
  /// 
  /// M=Applied to the Medical URA
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
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
  /// The value of the ADJUSTMENT_AMOUNT attribute.
  /// This attribute represents the total amount of adjustments (positive &amp; 
  /// negative) that have been applied to a specific year/month for a member of
  /// a household.
  /// </summary>
  [JsonPropertyName("adjustmentAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal AdjustmentAmount
  {
    get => adjustmentAmount;
    set => adjustmentAmount = Truncate(value, 2);
  }

  /// <summary>Length of the LEVEL_APPLIED_TO attribute.</summary>
  public const int LevelAppliedTo_MaxLength = 1;

  /// <summary>
  /// The value of the LEVEL_APPLIED_TO attribute.
  /// This attribute describes how the adjustment is to be applied. 
  /// Possible value are as follows:
  /// H=Household Level
  /// 
  /// M=Member Level
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LevelAppliedTo_MaxLength)]
  public string LevelAppliedTo
  {
    get => levelAppliedTo ?? "";
    set => levelAppliedTo =
      TrimEnd(Substring(value, 1, LevelAppliedTo_MaxLength));
  }

  /// <summary>
  /// The json value of the LevelAppliedTo attribute.</summary>
  [JsonPropertyName("levelAppliedTo")]
  [Computed]
  public string LevelAppliedTo_Json
  {
    get => NullIf(LevelAppliedTo, "");
    set => LevelAppliedTo = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Standard Definition
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
  /// Standard Definition.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the ADJUSTMENT_REASON attribute.</summary>
  public const int AdjustmentReason_MaxLength = 240;

  /// <summary>
  /// The value of the ADJUSTMENT_REASON attribute.
  /// Defines the reason for the adjustment.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = AdjustmentReason_MaxLength)
    ]
  public string AdjustmentReason
  {
    get => adjustmentReason ?? "";
    set => adjustmentReason =
      TrimEnd(Substring(value, 1, AdjustmentReason_MaxLength));
  }

  /// <summary>
  /// The json value of the AdjustmentReason attribute.</summary>
  [JsonPropertyName("adjustmentReason")]
  [Computed]
  public string AdjustmentReason_Json
  {
    get => NullIf(AdjustmentReason, "");
    set => AdjustmentReason = value;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// The year in which the member received some for of grant
  /// </summary>
  [JsonPropertyName("imsYear")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 4)]
  public int ImsYear
  {
    get => imsYear;
    set => imsYear = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// The month in which the member received some for of grant.
  /// </summary>
  [JsonPropertyName("imsMonth")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 2)]
  public int ImsMonth
  {
    get => imsMonth;
    set => imsMonth = value;
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo ?? "";
    set => imhAeCaseNo = TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ImhAeCaseNo attribute.</summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Computed]
  public string ImhAeCaseNo_Json
  {
    get => NullIf(ImhAeCaseNo, "");
    set => ImhAeCaseNo = value;
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
  [Member(Index = 10, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private string type1;
  private decimal adjustmentAmount;
  private string levelAppliedTo;
  private string createdBy;
  private DateTime? createdTmst;
  private string adjustmentReason;
  private int imsYear;
  private int imsMonth;
  private string imhAeCaseNo;
  private string cspNumber;
}

// The source file: CHILD_SUPPORT_ADJUSTMENT, ID: 371432213, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Special circumstances of the AP, AR or Child that will serve to modify the 
/// amount of support calculated using the guidelines.
/// Includes: Special needs (medical, therapy, etc) of the Child, long distance 
/// visitation for the AP, overall financial condition of the AP, cost of living
/// differential, etc.
/// </summary>
[Serializable]
public partial class ChildSupportAdjustment: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ChildSupportAdjustment()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ChildSupportAdjustment(ChildSupportAdjustment that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ChildSupportAdjustment Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ChildSupportAdjustment that)
  {
    base.Assign(that);
    number = that.number;
    adjustmentType = that.adjustmentType;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    description = that.description;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is the sequence Number in which the adjustments are ordered.
  /// </summary>
  [JsonPropertyName("number")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 2)]
  public int Number
  {
    get => number;
    set => number = value;
  }

  /// <summary>Length of the ADJUSTMENT_TYPE attribute.</summary>
  public const int AdjustmentType_MaxLength = 4;

  /// <summary>
  /// The value of the ADJUSTMENT_TYPE attribute.
  /// This attribute specifies the type of adjustment made to child support 
  /// worksheet computations.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AdjustmentType_MaxLength)]
  public string AdjustmentType
  {
    get => adjustmentType ?? "";
    set => adjustmentType =
      TrimEnd(Substring(value, 1, AdjustmentType_MaxLength));
  }

  /// <summary>
  /// The json value of the AdjustmentType attribute.</summary>
  [JsonPropertyName("adjustmentType")]
  [Computed]
  public string AdjustmentType_Json
  {
    get => NullIf(AdjustmentType, "");
    set => AdjustmentType = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 45;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A freeform narrative to describe what kind of adjustment.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  private int number;
  private string adjustmentType;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string description;
}

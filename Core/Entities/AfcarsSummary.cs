// The source file: AFCARS_SUMMARY, ID: 1902602446, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Monthly totals of child support payments per supported person.  This data is
/// necessary for the AFCARS federal reporting by FACTS.
/// </summary>
[Serializable]
public partial class AfcarsSummary: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AfcarsSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AfcarsSummary(AfcarsSummary that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AfcarsSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AfcarsSummary that)
  {
    base.Assign(that);
    cspNumber = that.cspNumber;
    reportMonth = that.reportMonth;
    collectionAmount = that.collectionAmount;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>Length of the CSP_NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSP_NUMBER attribute.
  /// Supported person number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the REPORT_MONTH attribute.
  /// The AFCARS reporting month.  This will be the reporting year and month in 
  /// a YYYMM format.
  /// </summary>
  [JsonPropertyName("reportMonth")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 6)]
  public int ReportMonth
  {
    get => reportMonth;
    set => reportMonth = value;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// Total amount of collections received for the supported person during the 
  /// report month.
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => collectionAmount;
    set => collectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private string cspNumber;
  private int reportMonth;
  private decimal? collectionAmount;
  private string createdBy;
  private DateTime? createdTimestamp;
}

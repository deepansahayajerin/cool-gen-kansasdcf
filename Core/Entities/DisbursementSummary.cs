// The source file: DISBURSEMENT_SUMMARY, ID: 371343364, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// 
/// This entity captures the amount of disbursements to a non_TAF case (i.e.,
/// obligor/obligee combination) during a federal fiscal year.  Additionally,
/// the table identifies the date upon which the $500 Deficit Reduction Act (DRA
/// ) threshold is exceeded.  This date will be used to determine the quarter in
/// which the case is reported on OCSE-396a as meeting the DRA fee citeria.
/// </summary>
[Serializable]
public partial class DisbursementSummary: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbursementSummary()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbursementSummary(DisbursementSummary that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbursementSummary Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisbursementSummary that)
  {
    base.Assign(that);
    fiscalYear = that.fiscalYear;
    nonTafAmount = that.nonTafAmount;
    thresholdDate = that.thresholdDate;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
    cspNumberOblgee = that.cspNumberOblgee;
    cpaTypeOblgee = that.cpaTypeOblgee;
    cspNumberOblgr = that.cspNumberOblgr;
    cpaTypeOblgr = that.cpaTypeOblgr;
  }

  /// <summary>
  /// The value of the FISCAL_YEAR attribute.
  /// The federal fiscal year to which the occurrence pertains.
  /// </summary>
  [JsonPropertyName("fiscalYear")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int FiscalYear
  {
    get => fiscalYear;
    set => fiscalYear = value;
  }

  /// <summary>
  /// The value of the NON_TAF_AMOUNT attribute.
  /// The amount disbursed to the obligor/obligee combination during the fiscal 
  /// year.
  /// </summary>
  [JsonPropertyName("nonTafAmount")]
  [Member(Index = 2, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? NonTafAmount
  {
    get => nonTafAmount;
    set => nonTafAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the THRESHOLD_DATE attribute.
  /// The date the amount disbursed exceeded the $500 DRA fee threshold amount.
  /// </summary>
  [JsonPropertyName("thresholdDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ThresholdDate
  {
    get => thresholdDate;
    set => thresholdDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
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
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberOblgee_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CspNumberOblgee_MaxLength)]
    
  public string CspNumberOblgee
  {
    get => cspNumberOblgee ?? "";
    set => cspNumberOblgee =
      TrimEnd(Substring(value, 1, CspNumberOblgee_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumberOblgee attribute.</summary>
  [JsonPropertyName("cspNumberOblgee")]
  [Computed]
  public string CspNumberOblgee_Json
  {
    get => NullIf(CspNumberOblgee, "");
    set => CspNumberOblgee = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaTypeOblgee_MaxLength = 1;

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
  [Member(Index = 9, Type = MemberType.Char, Length = CpaTypeOblgee_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaTypeOblgee
  {
    get => cpaTypeOblgee ?? "";
    set => cpaTypeOblgee =
      TrimEnd(Substring(value, 1, CpaTypeOblgee_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaTypeOblgee attribute.</summary>
  [JsonPropertyName("cpaTypeOblgee")]
  [Computed]
  public string CpaTypeOblgee_Json
  {
    get => NullIf(CpaTypeOblgee, "");
    set => CpaTypeOblgee = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumberOblgr_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CspNumberOblgr_MaxLength)]
    
  public string CspNumberOblgr
  {
    get => cspNumberOblgr ?? "";
    set => cspNumberOblgr =
      TrimEnd(Substring(value, 1, CspNumberOblgr_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumberOblgr attribute.</summary>
  [JsonPropertyName("cspNumberOblgr")]
  [Computed]
  public string CspNumberOblgr_Json
  {
    get => NullIf(CspNumberOblgr, "");
    set => CspNumberOblgr = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaTypeOblgr_MaxLength = 1;

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
  [Member(Index = 11, Type = MemberType.Char, Length = CpaTypeOblgr_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaTypeOblgr
  {
    get => cpaTypeOblgr ?? "";
    set => cpaTypeOblgr = TrimEnd(Substring(value, 1, CpaTypeOblgr_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaTypeOblgr attribute.</summary>
  [JsonPropertyName("cpaTypeOblgr")]
  [Computed]
  public string CpaTypeOblgr_Json
  {
    get => NullIf(CpaTypeOblgr, "");
    set => CpaTypeOblgr = value;
  }

  private int fiscalYear;
  private decimal? nonTafAmount;
  private DateTime? thresholdDate;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
  private string cspNumberOblgee;
  private string cpaTypeOblgee;
  private string cspNumberOblgr;
  private string cpaTypeOblgr;
}

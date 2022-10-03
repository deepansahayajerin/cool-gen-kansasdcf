// The source file: ACCRUAL_SUSPENSION, ID: 371430001, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Defines a specific timeframe that an accuring debt or payment schedule is 
/// suspended.
/// </summary>
[Serializable]
public partial class AccrualSuspension: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AccrualSuspension()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AccrualSuspension(AccrualSuspension that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AccrualSuspension Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AccrualSuspension that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    suspendDt = that.suspendDt;
    resumeDt = that.resumeDt;
    reductionPercentage = that.reductionPercentage;
    reasonTxt = that.reasonTxt;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    reductionAmount = that.reductionAmount;
    otrId = that.otrId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    obgId = that.obgId;
    otyId = that.otyId;
    otrType = that.otrType;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>
  /// The value of the SUSPEND_DT attribute.
  /// Defines the date an accruing debt or payment schedule is to be suspended.
  /// </summary>
  [JsonPropertyName("suspendDt")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? SuspendDt
  {
    get => suspendDt;
    set => suspendDt = value;
  }

  /// <summary>
  /// The value of the RESUME_DT attribute.
  /// Defines the date an accruing debt or payment schedule is to resume.
  /// </summary>
  [JsonPropertyName("resumeDt")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ResumeDt
  {
    get => resumeDt;
    set => resumeDt = value;
  }

  /// <summary>
  /// The value of the REDUCTION_PERCENTAGE attribute.
  /// Represents the percentage reduction in the accrual amount for a specific 
  /// obligation during the time frame specified in the frequency suspension.
  /// </summary>
  [JsonPropertyName("reductionPercentage")]
  [Member(Index = 4, Type = MemberType.Number, Length = 5, Precision = 2, Optional
    = true)]
  public decimal? ReductionPercentage
  {
    get => reductionPercentage;
    set => reductionPercentage = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the REASON_TXT attribute.</summary>
  public const int ReasonTxt_MaxLength = 240;

  /// <summary>
  /// The value of the REASON_TXT attribute.
  /// Details the reason why the accruing debt or payment schedule was suspened.
  /// </summary>
  [JsonPropertyName("reasonTxt")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = ReasonTxt_MaxLength, Optional
    = true)]
  public string ReasonTxt
  {
    get => reasonTxt;
    set => reasonTxt = value != null
      ? Substring(value, 1, ReasonTxt_MaxLength) : null;
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
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
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the REDUCTION_AMOUNT attribute.
  /// this attribute is used to comply with most court orders by allowing a 
  /// reduction in the accrual amount for a designated period of time.
  /// </summary>
  [JsonPropertyName("reductionAmount")]
  [Member(Index = 10, Type = MemberType.Number, Length = 7, Precision = 2, Optional
    = true)]
  public decimal? ReductionAmount
  {
    get => reductionAmount;
    set => reductionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int OtrId
  {
    get => otrId;
    set => otrId = value;
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
  [Member(Index = 12, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgId")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 3)]
  public int ObgId
  {
    get => obgId;
    set => obgId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 3)]
  public int OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType ?? "";
    set => otrType = TrimEnd(Substring(value, 1, OtrType_MaxLength));
  }

  /// <summary>
  /// The json value of the OtrType attribute.</summary>
  [JsonPropertyName("otrType")]
  [Computed]
  public string OtrType_Json
  {
    get => NullIf(OtrType, "");
    set => OtrType = value;
  }

  private int systemGeneratedIdentifier;
  private DateTime? suspendDt;
  private DateTime? resumeDt;
  private decimal? reductionPercentage;
  private string reasonTxt;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private decimal? reductionAmount;
  private int otrId;
  private string cpaType;
  private string cspNumber;
  private int obgId;
  private int otyId;
  private string otrType;
}

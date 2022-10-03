// The source file: HEALTH_INSURANCE_VIABILITY, ID: 371435181, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This describes whether or not it is viable to seek modification of an 
/// existing order to include health insurance coverage obligation. This also
/// helps to keep tract fo the last date that health insurance viability was
/// determined, the workwer who made the determination and date it was made. The
/// following criteria is used for determining viability: 1. % of gross income,
/// 2. can not decrease current child support by greater than 5%, 3. child
/// turns 18 within 12 months, 4. non-ADC non-medicaid waived, and 5. child
/// covered by 3rd party insurance.
/// This entity is maintained by the process 'DETERMINE_HEALTH_INS_VIABILITY'
/// If there is no existing health insurance coverage for the child and if there
/// is at least 10% increase in gross income then the viablility to seek
/// heealth insurance coverage for the child is determined as below.
/// If any of the following is true, then seeking health insurance coverage is 
/// not viable.
/// * Decreases current support by more than 5%
/// * Child turns 18 in next 12 months
/// * NADC / non medicaid AR waives medical for child covered by 3rd party 
/// insurance.
/// Otherwise it is viable to seek health insurance coverage.
/// </summary>
[Serializable]
public partial class HealthInsuranceViability: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HealthInsuranceViability()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HealthInsuranceViability(HealthInsuranceViability that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HealthInsuranceViability Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HealthInsuranceViability that)
  {
    base.Assign(that);
    identifier = that.identifier;
    hinsViableInd = that.hinsViableInd;
    hinsViableIndWorkerId = that.hinsViableIndWorkerId;
    hinsViableIndUpdatedDate = that.hinsViableIndUpdatedDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    croIdentifier = that.croIdentifier;
    croType = that.croType;
    casNumber = that.casNumber;
    cspNumber = that.cspNumber;
    cspNum = that.cspNum;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is generated automatically by the system as a value from 1 to 999 for
  /// a particular child. This attribute together with the relationship to
  /// CHILD uniquely identifies one instance of the entity.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the HINS_VIABLE_IND attribute.</summary>
  public const int HinsViableInd_MaxLength = 1;

  /// <summary>
  /// The value of the HINS_VIABLE_IND attribute.
  /// This attribute specifies whether or not it is viable to seek modification 
  /// of an existing order to include health insurance coverage for the child.
  /// It must be &quot;Y&quot; or &quot;N&quot;.
  /// Y - It has been determined that the health insurance is viable.
  /// N - The child's case was looked at and it was determined that the health 
  /// insurance is not viable at the particular point in time.
  /// </summary>
  [JsonPropertyName("hinsViableInd")]
  [Member(Index = 2, Type = MemberType.Char, Length = HinsViableInd_MaxLength, Optional
    = true)]
  public string HinsViableInd
  {
    get => hinsViableInd;
    set => hinsViableInd = value != null
      ? TrimEnd(Substring(value, 1, HinsViableInd_MaxLength)) : null;
  }

  /// <summary>Length of the HINS_VIABLE_IND_WORKER_ID attribute.</summary>
  public const int HinsViableIndWorkerId_MaxLength = 8;

  /// <summary>
  /// The value of the HINS_VIABLE_IND_WORKER_ID attribute.
  /// This attribute specifies the log-on user id or the (batch) program name 
  /// that updated the health insurance viability indicator.
  /// </summary>
  [JsonPropertyName("hinsViableIndWorkerId")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = HinsViableIndWorkerId_MaxLength, Optional = true)]
  public string HinsViableIndWorkerId
  {
    get => hinsViableIndWorkerId;
    set => hinsViableIndWorkerId = value != null
      ? TrimEnd(Substring(value, 1, HinsViableIndWorkerId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the HINS_VIABLE_IND_UPDATED_DATE attribute.
  /// This attribute specifies the date on which the determination (that health 
  /// insurance is viable or not) has been made.
  /// </summary>
  [JsonPropertyName("hinsViableIndUpdatedDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? HinsViableIndUpdatedDate
  {
    get => hinsViableIndUpdatedDate;
    set => hinsViableIndUpdatedDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int CroIdentifier
  {
    get => croIdentifier;
    set => croIdentifier = value;
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CasNumber_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  public const int CspNum_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNum")]
  [Member(Index = 13, Type = MemberType.Char, Length = CspNum_MaxLength, Optional
    = true)]
  public string CspNum
  {
    get => cspNum;
    set => cspNum = value != null
      ? TrimEnd(Substring(value, 1, CspNum_MaxLength)) : null;
  }

  private int identifier;
  private string hinsViableInd;
  private string hinsViableIndWorkerId;
  private DateTime? hinsViableIndUpdatedDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int croIdentifier;
  private string croType;
  private string casNumber;
  private string cspNumber;
  private string cspNum;
}

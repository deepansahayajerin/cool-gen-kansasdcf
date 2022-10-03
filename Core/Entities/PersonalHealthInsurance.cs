// The source file: PERSONAL_HEALTH_INSURANCE, ID: 371439502, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Health insurance coverage for a particular CSE_PERSON.
/// </summary>
[Serializable]
public partial class PersonalHealthInsurance: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonalHealthInsurance()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonalHealthInsurance(PersonalHealthInsurance that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonalHealthInsurance Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonalHealthInsurance that)
  {
    base.Assign(that);
    verifiedUserId = that.verifiedUserId;
    coverageVerifiedDate = that.coverageVerifiedDate;
    premiumVerifiedDate = that.premiumVerifiedDate;
    alertFlagInsuranceExistsInd = that.alertFlagInsuranceExistsInd;
    coverageCostAmount = that.coverageCostAmount;
    coverageBeginDate = that.coverageBeginDate;
    coverageEndDate = that.coverageEndDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
    hcvId = that.hcvId;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// User ID of the user who has verified.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COVERAGE_VERIFIED_DATE attribute.
  /// Date the coverage was verified.
  /// </summary>
  [JsonPropertyName("coverageVerifiedDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? CoverageVerifiedDate
  {
    get => coverageVerifiedDate;
    set => coverageVerifiedDate = value;
  }

  /// <summary>
  /// The value of the PREMIUM_VERIFIED_DATE attribute.
  /// Date the premium was verified.
  /// </summary>
  [JsonPropertyName("premiumVerifiedDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? PremiumVerifiedDate
  {
    get => premiumVerifiedDate;
    set => premiumVerifiedDate = value;
  }

  /// <summary>Length of the ALERT_FLAG_INSURANCE_EXISTS_IND attribute.
  /// </summary>
  public const int AlertFlagInsuranceExistsInd_MaxLength = 1;

  /// <summary>
  /// The value of the ALERT_FLAG_INSURANCE_EXISTS_IND attribute.
  /// Flag set when verification that insurance exists.  It is a reminder to 
  /// notify MEDI and the custodial parent that insurance coverage exists for
  /// the child.
  /// </summary>
  [JsonPropertyName("alertFlagInsuranceExistsInd")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = AlertFlagInsuranceExistsInd_MaxLength, Optional = true)]
  public string AlertFlagInsuranceExistsInd
  {
    get => alertFlagInsuranceExistsInd;
    set => alertFlagInsuranceExistsInd = value != null
      ? TrimEnd(Substring(value, 1, AlertFlagInsuranceExistsInd_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the COVERAGE_COST_AMOUNT attribute.
  /// Monthly out-of-pocket cost for premium on the insurance policy.
  /// </summary>
  [JsonPropertyName("coverageCostAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? CoverageCostAmount
  {
    get => coverageCostAmount;
    set => coverageCostAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COVERAGE_BEGIN_DATE attribute.
  /// The date that indicates when health insurance coverage starts.
  /// </summary>
  [JsonPropertyName("coverageBeginDate")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? CoverageBeginDate
  {
    get => coverageBeginDate;
    set => coverageBeginDate = value;
  }

  /// <summary>
  /// The value of the COVERAGE_END_DATE attribute.
  /// The last date that indicates when health insurance coverage stops.
  /// </summary>
  [JsonPropertyName("coverageEndDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? CoverageEndDate
  {
    get => coverageEndDate;
    set => coverageEndDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 9, Type = MemberType.Timestamp)]
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
  [Member(Index = 10, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify the entity type. This was 
  /// required since Insurance Policy number may not be present sometime. So {
  /// Insurance Company, Policy no} cannot be used as primary identifier.
  /// The identifier will be generated in the form yynnnnnnnn where yy = (2000 
  /// minus year) and nnnnnn is the next number available for the year.
  /// </summary>
  [JsonPropertyName("hcvId")]
  [DefaultValue(0L)]
  [Member(Index = 13, Type = MemberType.Number, Length = 10)]
  public long HcvId
  {
    get => hcvId;
    set => hcvId = value;
  }

  private string verifiedUserId;
  private DateTime? coverageVerifiedDate;
  private DateTime? premiumVerifiedDate;
  private string alertFlagInsuranceExistsInd;
  private decimal? coverageCostAmount;
  private DateTime? coverageBeginDate;
  private DateTime? coverageEndDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
  private long hcvId;
}

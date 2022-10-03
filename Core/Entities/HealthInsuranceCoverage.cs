// The source file: HEALTH_INSURANCE_COVERAGE, ID: 371435102, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Scope: A Health insurance policy paid by the AP that provides medical 
/// coverage for the children or family members.
/// The health insurance policy describes who is covered, what type of insurance
/// (medical,eye, hospital, dental etc), the coverage period, policy number and
/// who provides the insurance.
/// </summary>
[Serializable]
public partial class HealthInsuranceCoverage: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HealthInsuranceCoverage()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HealthInsuranceCoverage(HealthInsuranceCoverage that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HealthInsuranceCoverage Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HealthInsuranceCoverage that)
  {
    base.Assign(that);
    identifier = that.identifier;
    policyPaidByCsePersonInd = that.policyPaidByCsePersonInd;
    insuranceGroupNumber = that.insuranceGroupNumber;
    verifiedDate = that.verifiedDate;
    verifiedUserId = that.verifiedUserId;
    insurancePolicyNumber = that.insurancePolicyNumber;
    policyExpirationDate = that.policyExpirationDate;
    coverageCode1 = that.coverageCode1;
    coverageCode2 = that.coverageCode2;
    coverageCode3 = that.coverageCode3;
    coverageCode4 = that.coverageCode4;
    coverageCode5 = that.coverageCode5;
    coverageCode6 = that.coverageCode6;
    coverageCode7 = that.coverageCode7;
    policyEffectiveDate = that.policyEffectiveDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    otherCoveredPersons = that.otherCoveredPersons;
    isrIdentifier = that.isrIdentifier;
    cseNumber = that.cseNumber;
    conHNumber = that.conHNumber;
    cspHNumber = that.cspHNumber;
    cspNumber = that.cspNumber;
    hicIdentifier = that.hicIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify the entity type. This was 
  /// required since Insurance Policy number may not be present sometime. So {
  /// Insurance Company, Policy no} cannot be used as primary identifier.
  /// The identifier will be generated in the form yynnnnnnnn where yy = (2000 
  /// minus year) and nnnnnn is the next number available for the year.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0L)]
  [Member(Index = 1, Type = MemberType.Number, Length = 10)]
  public long Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the POLICY_PAID_BY_CSE_PERSON_IND attribute.</summary>
  public const int PolicyPaidByCsePersonInd_MaxLength = 1;

  /// <summary>
  /// The value of the POLICY_PAID_BY_CSE_PERSON_IND attribute.
  /// Indicates whether or not the policy is paid by a CSE Person.
  /// Y - Policy is paid by a CSE Person
  /// N - Policy is paid by a CONTACT of a CSE Person.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = PolicyPaidByCsePersonInd_MaxLength)]
  public string PolicyPaidByCsePersonInd
  {
    get => policyPaidByCsePersonInd ?? "";
    set => policyPaidByCsePersonInd =
      TrimEnd(Substring(value, 1, PolicyPaidByCsePersonInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PolicyPaidByCsePersonInd attribute.</summary>
  [JsonPropertyName("policyPaidByCsePersonInd")]
  [Computed]
  public string PolicyPaidByCsePersonInd_Json
  {
    get => NullIf(PolicyPaidByCsePersonInd, "");
    set => PolicyPaidByCsePersonInd = value;
  }

  /// <summary>Length of the INSURANCE_GROUP_NUMBER attribute.</summary>
  public const int InsuranceGroupNumber_MaxLength = 20;

  /// <summary>
  /// The value of the INSURANCE_GROUP_NUMBER attribute.
  /// A number that identifies a particular group within a health insurance 
  /// company.
  /// </summary>
  [JsonPropertyName("insuranceGroupNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = InsuranceGroupNumber_MaxLength, Optional = true)]
  public string InsuranceGroupNumber
  {
    get => insuranceGroupNumber;
    set => insuranceGroupNumber = value != null
      ? TrimEnd(Substring(value, 1, InsuranceGroupNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// Date verification was received that the insurance policy existed and the 
  /// information recorded was correct.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// 6-char alpha-numeric value identifying a KAECSES user who verified the 
  /// health insurance coverage.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>Length of the INSURANCE_POLICY_NUMBER attribute.</summary>
  public const int InsurancePolicyNumber_MaxLength = 20;

  /// <summary>
  /// The value of the INSURANCE_POLICY_NUMBER attribute.
  /// The individual's unique insurance policy number.  This is not a group 
  /// number.
  /// </summary>
  [JsonPropertyName("insurancePolicyNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = InsurancePolicyNumber_MaxLength, Optional = true)]
  public string InsurancePolicyNumber
  {
    get => insurancePolicyNumber;
    set => insurancePolicyNumber = value != null
      ? TrimEnd(Substring(value, 1, InsurancePolicyNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the POLICY_EXPIRATION_DATE attribute.
  /// Date insurance policy expires and thereby no longer provides coverage.
  /// </summary>
  [JsonPropertyName("policyExpirationDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? PolicyExpirationDate
  {
    get => policyExpirationDate;
    set => policyExpirationDate = value;
  }

  /// <summary>Length of the COVERAGE_CODE_1 attribute.</summary>
  public const int CoverageCode1_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_1 attribute.
  /// Coverage types, as determined by Medi system.
  /// </summary>
  [JsonPropertyName("coverageCode1")]
  [Member(Index = 8, Type = MemberType.Char, Length = CoverageCode1_MaxLength, Optional
    = true)]
  public string CoverageCode1
  {
    get => coverageCode1;
    set => coverageCode1 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode1_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_2 attribute.</summary>
  public const int CoverageCode2_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_2 attribute.
  /// Coverage type as determined by MEDI system
  /// </summary>
  [JsonPropertyName("coverageCode2")]
  [Member(Index = 9, Type = MemberType.Char, Length = CoverageCode2_MaxLength, Optional
    = true)]
  public string CoverageCode2
  {
    get => coverageCode2;
    set => coverageCode2 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode2_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_3 attribute.</summary>
  public const int CoverageCode3_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_3 attribute.
  /// Coverage type as determined by MEDI system.
  /// </summary>
  [JsonPropertyName("coverageCode3")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = CoverageCode3_MaxLength, Optional = true)]
  public string CoverageCode3
  {
    get => coverageCode3;
    set => coverageCode3 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode3_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_4 attribute.</summary>
  public const int CoverageCode4_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_4 attribute.
  /// Coverage type as determined by MEDI system
  /// </summary>
  [JsonPropertyName("coverageCode4")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = CoverageCode4_MaxLength, Optional = true)]
  public string CoverageCode4
  {
    get => coverageCode4;
    set => coverageCode4 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode4_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_5 attribute.</summary>
  public const int CoverageCode5_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_5 attribute.
  /// Coverage type as determined by MEDI system
  /// </summary>
  [JsonPropertyName("coverageCode5")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CoverageCode5_MaxLength, Optional = true)]
  public string CoverageCode5
  {
    get => coverageCode5;
    set => coverageCode5 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode5_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_6 attribute.</summary>
  public const int CoverageCode6_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_6 attribute.
  /// Coverage type as determined by MEDI system
  /// </summary>
  [JsonPropertyName("coverageCode6")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = CoverageCode6_MaxLength, Optional = true)]
  public string CoverageCode6
  {
    get => coverageCode6;
    set => coverageCode6 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode6_MaxLength)) : null;
  }

  /// <summary>Length of the COVERAGE_CODE_7 attribute.</summary>
  public const int CoverageCode7_MaxLength = 2;

  /// <summary>
  /// The value of the COVERAGE_CODE_7 attribute.
  /// Coverage type as determined by MEDI system
  /// </summary>
  [JsonPropertyName("coverageCode7")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = CoverageCode7_MaxLength, Optional = true)]
  public string CoverageCode7
  {
    get => coverageCode7;
    set => coverageCode7 = value != null
      ? TrimEnd(Substring(value, 1, CoverageCode7_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the POLICY_EFFECTIVE_DATE attribute.
  /// Date insurance coverage becomes effective.  This may or may not be the 
  /// date purchased.
  /// </summary>
  [JsonPropertyName("policyEffectiveDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? PolicyEffectiveDate
  {
    get => policyEffectiveDate;
    set => policyEffectiveDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 17, Type = MemberType.Timestamp)]
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
  [Member(Index = 18, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 19, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the OTHER_COVERED_PERSONS attribute.</summary>
  public const int OtherCoveredPersons_MaxLength = 80;

  /// <summary>
  /// The value of the OTHER_COVERED_PERSONS attribute.
  /// Non CSE Persons covered by the policy. Could be child(ren) or spouse. It 
  /// contains free formatted text.
  /// </summary>
  [JsonPropertyName("otherCoveredPersons")]
  [Member(Index = 20, Type = MemberType.Varchar, Length
    = OtherCoveredPersons_MaxLength, Optional = true)]
  public string OtherCoveredPersons
  {
    get => otherCoveredPersons;
    set => otherCoveredPersons = value != null
      ? Substring(value, 1, OtherCoveredPersons_MaxLength) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 21, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cseNumber")]
  [Member(Index = 22, Type = MemberType.Char, Length = CseNumber_MaxLength, Optional
    = true)]
  public string CseNumber
  {
    get => cseNumber;
    set => cseNumber = value != null
      ? TrimEnd(Substring(value, 1, CseNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CONTACT_NUMBER attribute.
  /// Identifier that indicates a particular CSE contact person.
  /// </summary>
  [JsonPropertyName("conHNumber")]
  [Member(Index = 23, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? ConHNumber
  {
    get => conHNumber;
    set => conHNumber = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspHNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspHNumber")]
  [Member(Index = 24, Type = MemberType.Char, Length = CspHNumber_MaxLength, Optional
    = true)]
  public string CspHNumber
  {
    get => cspHNumber;
    set => cspHNumber = value != null
      ? TrimEnd(Substring(value, 1, CspHNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 25, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify the entity occurrence.
  /// </summary>
  [JsonPropertyName("hicIdentifier")]
  [Member(Index = 26, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? HicIdentifier
  {
    get => hicIdentifier;
    set => hicIdentifier = value;
  }

  private long identifier;
  private string policyPaidByCsePersonInd;
  private string insuranceGroupNumber;
  private DateTime? verifiedDate;
  private string verifiedUserId;
  private string insurancePolicyNumber;
  private DateTime? policyExpirationDate;
  private string coverageCode1;
  private string coverageCode2;
  private string coverageCode3;
  private string coverageCode4;
  private string coverageCode5;
  private string coverageCode6;
  private string coverageCode7;
  private DateTime? policyEffectiveDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string otherCoveredPersons;
  private DateTime? isrIdentifier;
  private string cseNumber;
  private int? conHNumber;
  private string cspHNumber;
  private string cspNumber;
  private int? hicIdentifier;
}

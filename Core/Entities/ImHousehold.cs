// The source file: IM_HOUSEHOLD, ID: 371435300, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity defines an IM Household. The individual members (only the CSE 
/// Persons) in the household are defined by the entity IM_HOUSEHOLD_MEMEBER.
/// </summary>
[Serializable]
public partial class ImHousehold: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ImHousehold()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ImHousehold(ImHousehold that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ImHousehold Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ImHousehold that)
  {
    base.Assign(that);
    aeCaseNo = that.aeCaseNo;
    zdelHouseholdSize = that.zdelHouseholdSize;
    caseStatus = that.caseStatus;
    statusDate = that.statusDate;
    firstBenefitDate = that.firstBenefitDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    zdelType = that.zdelType;
    zdelCalculateFlag = that.zdelCalculateFlag;
    zdelMultiCaseIndicator = that.zdelMultiCaseIndicator;
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int AeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AeCaseNo_MaxLength)]
  public string AeCaseNo
  {
    get => aeCaseNo ?? "";
    set => aeCaseNo = TrimEnd(Substring(value, 1, AeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the AeCaseNo attribute.</summary>
  [JsonPropertyName("aeCaseNo")]
  [Computed]
  public string AeCaseNo_Json
  {
    get => NullIf(AeCaseNo, "");
    set => AeCaseNo = value;
  }

  /// <summary>
  /// The value of the ZDEL_HOUSEHOLD_SIZE attribute.
  /// This attribute defines the total number of persons included in IM's grant.
  /// The person included may or may not be a CSE Person.
  /// </summary>
  [JsonPropertyName("zdelHouseholdSize")]
  [Member(Index = 2, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? ZdelHouseholdSize
  {
    get => zdelHouseholdSize;
    set => zdelHouseholdSize = value;
  }

  /// <summary>Length of the CASE_STATUS attribute.</summary>
  public const int CaseStatus_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_STATUS attribute.
  /// This attribute specifies the status of the IM case.
  /// The valid values are maintained in CODE_VALUE entity for CODE_NAME '
  /// IM_CASE_STATUS'.
  /// Typical values are:
  /// &quot;O&quot; - case is open
  /// &quot;C&quot; - case is closed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseStatus_MaxLength)]
  public string CaseStatus
  {
    get => caseStatus ?? "";
    set => caseStatus = TrimEnd(Substring(value, 1, CaseStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseStatus attribute.</summary>
  [JsonPropertyName("caseStatus")]
  [Computed]
  public string CaseStatus_Json
  {
    get => NullIf(CaseStatus, "");
    set => CaseStatus = value;
  }

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// This attribute specifies the date associated with the case status. If the 
  /// staus is open, the status date specifies the date the case was effective
  /// from. If the status is closed, the status date specifies the date the IM
  /// case was closed.
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? StatusDate
  {
    get => statusDate;
    set => statusDate = value;
  }

  /// <summary>
  /// The value of the FIRST_BENEFIT_DATE attribute.
  /// This attribute specifies the 1st day of the first ADC benefit month for 
  /// the AR. This attribute is required to consider the collections received in
  /// or after that month for the purposes of URA computation. Any payment
  /// history created for collections before the AR started receiving benefits
  /// should not be considered for the purpose of URA computation.
  /// </summary>
  [JsonPropertyName("firstBenefitDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? FirstBenefitDate
  {
    get => firstBenefitDate;
    set => firstBenefitDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the ZDEL_TYPE attribute.</summary>
  public const int ZdelType_MaxLength = 3;

  /// <summary>
  /// The value of the ZDEL_TYPE attribute.
  /// This attribute identifies the type of AE Case (IM Household) as AFDC (
  /// designated by AF) or Foster Care (designated by FC).
  /// </summary>
  [JsonPropertyName("zdelType")]
  [Member(Index = 10, Type = MemberType.Char, Length = ZdelType_MaxLength, Optional
    = true)]
  public string ZdelType
  {
    get => zdelType;
    set => zdelType = value != null
      ? TrimEnd(Substring(value, 1, ZdelType_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_CALCULATE_FLAG attribute.</summary>
  public const int ZdelCalculateFlag_MaxLength = 1;

  /// <summary>
  /// The value of the ZDEL_CALCULATE_FLAG attribute.
  /// This attribute identifies the need to calculate or recalculate URA for the
  /// IM Household.
  /// </summary>
  [JsonPropertyName("zdelCalculateFlag")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ZdelCalculateFlag_MaxLength, Optional = true)]
  public string ZdelCalculateFlag
  {
    get => zdelCalculateFlag;
    set => zdelCalculateFlag = value != null
      ? TrimEnd(Substring(value, 1, ZdelCalculateFlag_MaxLength)) : null;
  }

  /// <summary>Length of the ZDEL_MULTI_CASE_INDICATOR attribute.</summary>
  public const int ZdelMultiCaseIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ZDEL_MULTI_CASE_INDICATOR attribute.
  /// The multi_case_indicator is used to identify if any of its Im Household 
  /// Members have been or are now part of a different Im Household. This
  /// attribute is populated during the adding of an Im Household Member. When
  /// adding a member to a household, the adding program will need to determine
  /// if that CSE Person has been in another Im Household. If that is the case,
  /// both Im Households will have the multi_case_indicator set to &quot;Y
  /// &quot;. This attribute will allow for better performance of the excess URA
  /// processing.
  /// </summary>
  [JsonPropertyName("zdelMultiCaseIndicator")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ZdelMultiCaseIndicator_MaxLength, Optional = true)]
  public string ZdelMultiCaseIndicator
  {
    get => zdelMultiCaseIndicator;
    set => zdelMultiCaseIndicator = value != null
      ? TrimEnd(Substring(value, 1, ZdelMultiCaseIndicator_MaxLength)) : null;
  }

  private string aeCaseNo;
  private int? zdelHouseholdSize;
  private string caseStatus;
  private DateTime? statusDate;
  private DateTime? firstBenefitDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string zdelType;
  private string zdelCalculateFlag;
  private string zdelMultiCaseIndicator;
}

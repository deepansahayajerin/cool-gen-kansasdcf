// The source file: CHILD_SUPPORT_WORKSHEET, ID: 371432255, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// The worksheet should contain the actual calculation of the child support 
/// based on the child support income, work related child care costs, health and
/// dental insurance premiums, and any child support adjustments.
/// </summary>
[Serializable]
public partial class ChildSupportWorksheet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ChildSupportWorksheet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ChildSupportWorksheet(ChildSupportWorksheet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ChildSupportWorksheet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ChildSupportWorksheet that)
  {
    base.Assign(that);
    noOfChildrenInAgeGrp3 = that.noOfChildrenInAgeGrp3;
    noOfChildrenInAgeGrp2 = that.noOfChildrenInAgeGrp2;
    noOfChildrenInAgeGrp1 = that.noOfChildrenInAgeGrp1;
    additionalNoOfChildren = that.additionalNoOfChildren;
    status = that.status;
    costOfLivingDiffAdjInd = that.costOfLivingDiffAdjInd;
    identifier = that.identifier;
    multipleFamilyAdjInd = that.multipleFamilyAdjInd;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    authorizingAuthority = that.authorizingAuthority;
    parentingTimeAdjPercent = that.parentingTimeAdjPercent;
    csGuidelineYear = that.csGuidelineYear;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>
  /// The value of the NO_OF_CHILDREN_IN_AGE_GRP3 attribute.
  /// The number of children that you are calculating current Child Support for,
  /// within the age category 16 to 18 years old.
  /// Attribute renamed (November, 2007) to refelect dynamic changing of the age
  /// range contained within it.  The name is changed from
  /// NO_OF_CHILDREN_IN_AGE_GRP_16_18
  /// </summary>
  [JsonPropertyName("noOfChildrenInAgeGrp3")]
  [Member(Index = 1, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? NoOfChildrenInAgeGrp3
  {
    get => noOfChildrenInAgeGrp3;
    set => noOfChildrenInAgeGrp3 = value;
  }

  /// <summary>
  /// The value of the NO_OF_CHILDREN_IN_AGE_GRP2 attribute.
  /// The number of children that you are calculating current Child Support for,
  /// within the age category 7 to 15 years old.
  /// Attribute renamed (November, 2007) to refelect dynamic changing of the age
  /// range contained within it.  The name is changed from
  /// NO_OF_CHILDREN_IN_AGE_GRP_7_15
  /// </summary>
  [JsonPropertyName("noOfChildrenInAgeGrp2")]
  [Member(Index = 2, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? NoOfChildrenInAgeGrp2
  {
    get => noOfChildrenInAgeGrp2;
    set => noOfChildrenInAgeGrp2 = value;
  }

  /// <summary>
  /// The value of the NO_OF_CHILDREN_IN_AGE_GRP1 attribute.
  /// The number of children that you are calculating current Child Support for,
  /// within the age category 0 to 6 years old.
  /// Attribute renamed (November, 2007) to refelect dynamic changing of the age
  /// range contained within it.  The name is changed from
  /// NO_OF_CHILDREN_IN_AGE_GRP_0_6
  /// </summary>
  [JsonPropertyName("noOfChildrenInAgeGrp1")]
  [Member(Index = 3, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? NoOfChildrenInAgeGrp1
  {
    get => noOfChildrenInAgeGrp1;
    set => noOfChildrenInAgeGrp1 = value;
  }

  /// <summary>
  /// The value of the ADDITIONAL_NO_OF_CHILDREN attribute.
  /// ADDITIONAL NUMBER OF CHILDREN THAT THE PARENTS MAY BE SUPPORTING OTHER 
  /// THAN THE COMMON CHILDREN.
  /// </summary>
  [JsonPropertyName("additionalNoOfChildren")]
  [Member(Index = 4, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? AdditionalNoOfChildren
  {
    get => additionalNoOfChildren;
    set => additionalNoOfChildren = value;
  }

  /// <summary>Length of the STATUS attribute.</summary>
  public const int Status_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS attribute.
  /// Describes status of the worksheet.
  /// e.g. T - Temp
  ///      S - Submit to court
  ///      D - Discard
  ///      J - Journalised etc
  /// The permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// WORKSHEET_STATUS.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Status_MaxLength)]
  public string Status
  {
    get => status ?? "";
    set => status = TrimEnd(Substring(value, 1, Status_MaxLength));
  }

  /// <summary>
  /// The json value of the Status attribute.</summary>
  [JsonPropertyName("status")]
  [Computed]
  public string Status_Json
  {
    get => NullIf(Status, "");
    set => Status = value;
  }

  /// <summary>Length of the COST_OF_LIVING_DIFF_ADJ_IND attribute.</summary>
  public const int CostOfLivingDiffAdjInd_MaxLength = 1;

  /// <summary>
  /// The value of the COST_OF_LIVING_DIFF_ADJ_IND attribute.
  /// The cost of living may vary amoung states.  The ACCRA Cost of Living Index
  /// provides relative costs of living throughout the United States.
  /// </summary>
  [JsonPropertyName("costOfLivingDiffAdjInd")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = CostOfLivingDiffAdjInd_MaxLength, Optional = true)]
  public string CostOfLivingDiffAdjInd
  {
    get => costOfLivingDiffAdjInd;
    set => costOfLivingDiffAdjInd = value != null
      ? TrimEnd(Substring(value, 1, CostOfLivingDiffAdjInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Artificial attribute to uniquely identify a record.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0L)]
  [Member(Index = 7, Type = MemberType.Number, Length = 10)]
  public long Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the MULTIPLE_FAMILY_ADJ_IND attribute.</summary>
  public const int MultipleFamilyAdjInd_MaxLength = 1;

  /// <summary>
  /// The value of the MULTIPLE_FAMILY_ADJ_IND attribute.
  /// If the noncustodial parent has children by another relationship who reside
  /// with them, the child support schedule representing the total number of
  /// children that the noncustodial parent legally is obligated to support
  /// shall be used in determining the basic support obligation.
  /// </summary>
  [JsonPropertyName("multipleFamilyAdjInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = MultipleFamilyAdjInd_MaxLength, Optional = true)]
  public string MultipleFamilyAdjInd
  {
    get => multipleFamilyAdjInd;
    set => multipleFamilyAdjInd = value != null
      ? TrimEnd(Substring(value, 1, MultipleFamilyAdjInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 10, Type = MemberType.Timestamp)]
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
  [Member(Index = 11, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the AUTHORIZING_AUTHORITY attribute.</summary>
  public const int AuthorizingAuthority_MaxLength = 5;

  /// <summary>
  /// The value of the AUTHORIZING_AUTHORITY attribute.
  /// This field will contain one of the following code.                       
  /// JUDGE = Judge
  /// 
  /// HEARO = Hearing Officer
  /// OTHER = Other
  /// </summary>
  [JsonPropertyName("authorizingAuthority")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = AuthorizingAuthority_MaxLength, Optional = true)]
  public string AuthorizingAuthority
  {
    get => authorizingAuthority;
    set => authorizingAuthority = value != null
      ? TrimEnd(Substring(value, 1, AuthorizingAuthority_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PARENTING_TIME_ADJ_PERCENT attribute.
  /// Stores parenting time adjustment percentage value.  This value will be 
  /// entered through CSE Child Support Worksheet screen 3 (WOR3).
  /// </summary>
  [JsonPropertyName("parentingTimeAdjPercent")]
  [Member(Index = 14, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ParentingTimeAdjPercent
  {
    get => parentingTimeAdjPercent;
    set => parentingTimeAdjPercent = value;
  }

  /// <summary>
  /// The value of the CS_GUIDELINE_YEAR attribute.
  /// The year the child support guidelines values are set.  This routinely 
  /// changes approximately every four years. This attribute is stored as a four
  /// character year like 2008, 2012, 2016.   Each time the guidelines change (
  /// every four years or so) the numbers will be entered onto the table along
  /// with the new values.  The existing values for prior years will remain.
  /// </summary>
  [JsonPropertyName("csGuidelineYear")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 4)]
  public int CsGuidelineYear
  {
    get => csGuidelineYear;
    set => csGuidelineYear = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private int? noOfChildrenInAgeGrp3;
  private int? noOfChildrenInAgeGrp2;
  private int? noOfChildrenInAgeGrp1;
  private int? additionalNoOfChildren;
  private string status;
  private string costOfLivingDiffAdjInd;
  private long identifier;
  private string multipleFamilyAdjInd;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string authorizingAuthority;
  private int? parentingTimeAdjPercent;
  private int csGuidelineYear;
  private int? lgaIdentifier;
}

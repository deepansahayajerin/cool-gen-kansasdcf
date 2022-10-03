// The source file: CSE_PERSON_SUPPORT_WORKSHEET, ID: 371433331, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains the details of Parent-A or Parent-B for the Child 
/// Support Worksheet.
/// </summary>
[Serializable]
public partial class CsePersonSupportWorksheet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonSupportWorksheet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonSupportWorksheet(CsePersonSupportWorksheet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonSupportWorksheet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonSupportWorksheet that)
  {
    base.Assign(that);
    noOfChildrenInDayCare = that.noOfChildrenInDayCare;
    workRelatedChildCareCosts = that.workRelatedChildCareCosts;
    identifer = that.identifer;
    wageEarnerGrossIncome = that.wageEarnerGrossIncome;
    selfEmploymentGrossIncome = that.selfEmploymentGrossIncome;
    reasonableBusinessExpense = that.reasonableBusinessExpense;
    courtOrderedChildSupportPaid = that.courtOrderedChildSupportPaid;
    childSupprtPaidCourtOrderNo = that.childSupprtPaidCourtOrderNo;
    courtOrderedMaintenancePaid = that.courtOrderedMaintenancePaid;
    maintenancePaidCourtOrderNo = that.maintenancePaidCourtOrderNo;
    courtOrderedMaintenanceRecvd = that.courtOrderedMaintenanceRecvd;
    maintenanceRecvdCourtOrderNo = that.maintenanceRecvdCourtOrderNo;
    healthAndDentalInsurancePrem = that.healthAndDentalInsurancePrem;
    eligibleForFederalTaxCredit = that.eligibleForFederalTaxCredit;
    eligibleForKansasTaxCredit = that.eligibleForKansasTaxCredit;
    netAdjParentalChildSuppAmt = that.netAdjParentalChildSuppAmt;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    enforcementFeeType = that.enforcementFeeType;
    enforcementFeeAllowance = that.enforcementFeeAllowance;
    insuranceWorkRelatedCcCredit = that.insuranceWorkRelatedCcCredit;
    abilityToPay = that.abilityToPay;
    equalParentingTimeObligation = that.equalParentingTimeObligation;
    socialSecDependentBenefit = that.socialSecDependentBenefit;
    croIdentifier = that.croIdentifier;
    croType = that.croType;
    casNumber = that.casNumber;
    cspNumber = that.cspNumber;
    cswIdentifier = that.cswIdentifier;
    cssGuidelineYr = that.cssGuidelineYr;
  }

  /// <summary>
  /// The value of the NO_OF_CHILDREN_IN_DAY_CARE attribute.
  /// The total number of children that are in Day Care within this case.
  /// </summary>
  [JsonPropertyName("noOfChildrenInDayCare")]
  [Member(Index = 1, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? NoOfChildrenInDayCare
  {
    get => noOfChildrenInDayCare;
    set => noOfChildrenInDayCare = value;
  }

  /// <summary>
  /// The value of the WORK_RELATED_CHILD_CARE_COSTS attribute.
  /// Costs incurred by the parent while placing the Kid at day care.
  /// </summary>
  [JsonPropertyName("workRelatedChildCareCosts")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? WorkRelatedChildCareCosts
  {
    get => workRelatedChildCareCosts;
    set => workRelatedChildCareCosts = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the IDENTIFER attribute.
  /// Identifier that indicates a particular child support worksheet for a cse 
  /// person.
  /// </summary>
  [JsonPropertyName("identifer")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int Identifer
  {
    get => identifer;
    set => identifer = value;
  }

  /// <summary>
  /// The value of the WAGE_EARNER_GROSS_INCOME attribute.
  /// The wages including all income which is regularly and periodically receive
  /// from any surces, excludes public assistance.
  /// </summary>
  [JsonPropertyName("wageEarnerGrossIncome")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? WageEarnerGrossIncome
  {
    get => wageEarnerGrossIncome;
    set => wageEarnerGrossIncome = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the SELF_EMPLOYMENT_GROSS_INCOME attribute.
  /// The wages from self-employment, including all income which is regularly 
  /// and periodically received from any sources.
  /// </summary>
  [JsonPropertyName("selfEmploymentGrossIncome")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SelfEmploymentGrossIncome
  {
    get => selfEmploymentGrossIncome;
    set => selfEmploymentGrossIncome = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the REASONABLE_BUSINESS_EXPENSE attribute.
  /// In cases of self-employed person, actual expenditures reasonably necessary
  /// for the production of income.
  /// </summary>
  [JsonPropertyName("reasonableBusinessExpense")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ReasonableBusinessExpense
  {
    get => reasonableBusinessExpense;
    set => reasonableBusinessExpense = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the COURT_ORDERED_CHILD_SUPPORT_PAID attribute.
  /// PRE-EXISTING CHILD SUPPORT OBLIGATIONS IN OTHER CASES SHALL BE DEDUCTED TO
  /// THE EXTENT THAT THESE SUPPORT OBLIGATIONS ARE ACTUALLY PAID. THESE
  /// AMOUNTS ARE ENTERED ON LINE C2 OF THE CHILD SUPPORT WORKSHEET.
  /// </summary>
  [JsonPropertyName("courtOrderedChildSupportPaid")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CourtOrderedChildSupportPaid
  {
    get => courtOrderedChildSupportPaid;
    set => courtOrderedChildSupportPaid = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the CHILD_SUPPRT_PAID_COURT_ORDER_NO attribute.
  /// </summary>
  public const int ChildSupprtPaidCourtOrderNo_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_SUPPRT_PAID_COURT_ORDER_NO attribute.
  /// The court case number that relates to the amount of Child Support paid on 
  /// another Child Support Case.
  /// </summary>
  [JsonPropertyName("childSupprtPaidCourtOrderNo")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = ChildSupprtPaidCourtOrderNo_MaxLength, Optional = true)]
  public string ChildSupprtPaidCourtOrderNo
  {
    get => childSupprtPaidCourtOrderNo;
    set => childSupprtPaidCourtOrderNo = value != null
      ? TrimEnd(Substring(value, 1, ChildSupprtPaidCourtOrderNo_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the COURT_ORDERED_MAINTENANCE_PAID attribute.
  /// The amount of court-ordered maintenance paid pursuant to a court order in 
  /// this or a prior divorce case that shall be deducted to the extent that the
  /// maintenance is actually paid.
  /// </summary>
  [JsonPropertyName("courtOrderedMaintenancePaid")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CourtOrderedMaintenancePaid
  {
    get => courtOrderedMaintenancePaid;
    set => courtOrderedMaintenancePaid = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the MAINTENANCE_PAID_COURT_ORDER_NO attribute.
  /// </summary>
  public const int MaintenancePaidCourtOrderNo_MaxLength = 10;

  /// <summary>
  /// The value of the MAINTENANCE_PAID_COURT_ORDER_NO attribute.
  /// The court case number that relates to the amount of maintenance paid on 
  /// another Child Support/Maintenance case,
  /// </summary>
  [JsonPropertyName("maintenancePaidCourtOrderNo")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = MaintenancePaidCourtOrderNo_MaxLength, Optional = true)]
  public string MaintenancePaidCourtOrderNo
  {
    get => maintenancePaidCourtOrderNo;
    set => maintenancePaidCourtOrderNo = value != null
      ? TrimEnd(Substring(value, 1, MaintenancePaidCourtOrderNo_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the COURT_ORDERED_MAINTENANCE_RECVD attribute.
  /// The amount of any court-ordered maintenance received by a party pursuant 
  /// to a court order in this or a prior divorce case, that shall be added as
  /// income to the extent that the maintenance is actually received.
  /// </summary>
  [JsonPropertyName("courtOrderedMaintenanceRecvd")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CourtOrderedMaintenanceRecvd
  {
    get => courtOrderedMaintenanceRecvd;
    set => courtOrderedMaintenanceRecvd = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the MAINTENANCE_RECVD_COURT_ORDER_NO attribute.
  /// </summary>
  public const int MaintenanceRecvdCourtOrderNo_MaxLength = 10;

  /// <summary>
  /// The value of the MAINTENANCE_RECVD_COURT_ORDER_NO attribute.
  /// The court case number that relates to the amount that was recieved from 
  /// maintenance on another Child Support/Maintenance case.
  /// </summary>
  [JsonPropertyName("maintenanceRecvdCourtOrderNo")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = MaintenanceRecvdCourtOrderNo_MaxLength, Optional = true)]
  public string MaintenanceRecvdCourtOrderNo
  {
    get => maintenanceRecvdCourtOrderNo;
    set => maintenanceRecvdCourtOrderNo = value != null
      ? TrimEnd(Substring(value, 1, MaintenanceRecvdCourtOrderNo_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the HEALTH_AND_DENTAL_INSURANCE_PREM attribute.
  /// The cost to the parent for health, dental, or optometric insurance for the
  /// child(ren).
  /// </summary>
  [JsonPropertyName("healthAndDentalInsurancePrem")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? HealthAndDentalInsurancePrem
  {
    get => healthAndDentalInsurancePrem;
    set => healthAndDentalInsurancePrem = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the ELIGIBLE_FOR_FEDERAL_TAX_CREDIT attribute.
  /// </summary>
  public const int EligibleForFederalTaxCredit_MaxLength = 1;

  /// <summary>
  /// The value of the ELIGIBLE_FOR_FEDERAL_TAX_CREDIT attribute.
  /// Y=YES
  /// N=NO
  /// The federal anticipant tax credit amount for child care cost which would 
  /// apply to the actual child care expenditure upto $200 per month for one
  /// child or $400 per month for two or more children receiving child care. See
  /// Child Care Cost Table.
  /// </summary>
  [JsonPropertyName("eligibleForFederalTaxCredit")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = EligibleForFederalTaxCredit_MaxLength, Optional = true)]
  public string EligibleForFederalTaxCredit
  {
    get => eligibleForFederalTaxCredit;
    set => eligibleForFederalTaxCredit = value != null
      ? TrimEnd(Substring(value, 1, EligibleForFederalTaxCredit_MaxLength)) : null
      ;
  }

  /// <summary>Length of the ELIGIBLE_FOR_KANSAS_TAX_CREDIT attribute.</summary>
  public const int EligibleForKansasTaxCredit_MaxLength = 1;

  /// <summary>
  /// The value of the ELIGIBLE_FOR_KANSAS_TAX_CREDIT attribute.
  /// Y=YES
  /// N=NO
  /// The Kansas tax credit in addition to the Federal child care credit. The 
  /// credit shall be applied by multiplying the federal credit calculated by 25
  /// %.
  /// </summary>
  [JsonPropertyName("eligibleForKansasTaxCredit")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = EligibleForKansasTaxCredit_MaxLength, Optional = true)]
  public string EligibleForKansasTaxCredit
  {
    get => eligibleForKansasTaxCredit;
    set => eligibleForKansasTaxCredit = value != null
      ? TrimEnd(Substring(value, 1, EligibleForKansasTaxCredit_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the NET_ADJ_PARENTAL_CHILD_SUPP_AMT attribute.
  /// THIS IS THE FINAL PARENTAL CHILD SUPPORT OBLIGATION AFTER ALL ADJUSTMENTS.
  /// </summary>
  [JsonPropertyName("netAdjParentalChildSuppAmt")]
  [Member(Index = 16, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? NetAdjParentalChildSuppAmt
  {
    get => netAdjParentalChildSuppAmt;
    set => netAdjParentalChildSuppAmt = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 18, Type = MemberType.Timestamp)]
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
  [Member(Index = 19, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 20, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the ENFORCEMENT_FEE_TYPE attribute.</summary>
  public const int EnforcementFeeType_MaxLength = 1;

  /// <summary>
  /// The value of the ENFORCEMENT_FEE_TYPE attribute.
  /// The enforcement_fee_type is an indicator used to calculate the enforcement
  /// fee. If it's a flat fee, the court trustee's
  /// fee=Enforcement_Fee_Allowance*.5. If it's a percentage, the trustee's fee=
  /// (Adjusted Subtotal* Enforcement_Fee_Allowance/100.0)*.5.
  /// </summary>
  [JsonPropertyName("enforcementFeeType")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = EnforcementFeeType_MaxLength, Optional = true)]
  public string EnforcementFeeType
  {
    get => enforcementFeeType;
    set => enforcementFeeType = value != null
      ? TrimEnd(Substring(value, 1, EnforcementFeeType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ENFORCEMENT_FEE_ALLOWANCE attribute.
  /// This is the actual amount for the flat fee or percentage. If 
  /// enforcement_fee_type='P', it holds the acutal percentage. If the
  /// enforcement_fee_type='F', it holds the total flat fee.
  /// </summary>
  [JsonPropertyName("enforcementFeeAllowance")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? EnforcementFeeAllowance
  {
    get => enforcementFeeAllowance;
    set => enforcementFeeAllowance = value;
  }

  /// <summary>
  /// The value of the INSURANCE_WORK_RELATED_CC_CREDIT attribute.
  /// Amount credited on the child support worksheet for health insurance and 
  /// work related child care costs paid by a parent
  /// </summary>
  [JsonPropertyName("insuranceWorkRelatedCcCredit")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? InsuranceWorkRelatedCcCredit
  {
    get => insuranceWorkRelatedCcCredit;
    set => insuranceWorkRelatedCcCredit = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>Length of the ABILITY_TO_PAY attribute.</summary>
  public const int AbilityToPay_MaxLength = 1;

  /// <summary>
  /// The value of the ABILITY_TO_PAY attribute.
  /// Y/N flag indicating whether the Ability to Pay calculation should be 
  /// applied to this parent.
  /// </summary>
  [JsonPropertyName("abilityToPay")]
  [Member(Index = 24, Type = MemberType.Char, Length = AbilityToPay_MaxLength, Optional
    = true)]
  public string AbilityToPay
  {
    get => abilityToPay;
    set => abilityToPay = value != null
      ? TrimEnd(Substring(value, 1, AbilityToPay_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EQUAL_PARENTING_TIME_OBLIGATION attribute.
  /// Equal parenting time obligation amount allowed when using the shared 
  /// expense formula or the equal parenting time formula to determine the child
  /// support obligation.  This amount applies to the deviation from rebuttable
  /// presumption amount.
  /// </summary>
  [JsonPropertyName("equalParentingTimeObligation")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? EqualParentingTimeObligation
  {
    get => equalParentingTimeObligation;
    set => equalParentingTimeObligation = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the SOCIAL_SEC_DEPENDENT_BENEFIT attribute.
  /// Amount of social security dependent or auxiliary benefit received by the 
  /// child as applied to the deviation from rebuttable presumption amount.
  /// </summary>
  [JsonPropertyName("socialSecDependentBenefit")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? SocialSecDependentBenefit
  {
    get => socialSecDependentBenefit;
    set => socialSecDependentBenefit = value != null ? Truncate(value, 2) : null
      ;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 27, Type = MemberType.Number, Length = 3)]
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
  [Member(Index = 28, Type = MemberType.Char, Length = CroType_MaxLength)]
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
  [Member(Index = 29, Type = MemberType.Char, Length = CasNumber_MaxLength)]
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
  [Member(Index = 30, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// Artificial attribute to uniquely identify a record.
  /// </summary>
  [JsonPropertyName("cswIdentifier")]
  [DefaultValue(0L)]
  [Member(Index = 31, Type = MemberType.Number, Length = 10)]
  public long CswIdentifier
  {
    get => cswIdentifier;
    set => cswIdentifier = value;
  }

  /// <summary>
  /// The value of the CS_GUIDELINE_YEAR attribute.
  /// The year the child support guidelines values are set.  This routinely 
  /// changes approximately every four years. This attribute is stored as a four
  /// character year like 2008, 2012, 2016.   Each time the guidelines change (
  /// every four years or so) the numbers will be entered onto the table along
  /// with the new values.  The existing values for prior years will remain.
  /// </summary>
  [JsonPropertyName("cssGuidelineYr")]
  [DefaultValue(0)]
  [Member(Index = 32, Type = MemberType.Number, Length = 4)]
  public int CssGuidelineYr
  {
    get => cssGuidelineYr;
    set => cssGuidelineYr = value;
  }

  private int? noOfChildrenInDayCare;
  private decimal? workRelatedChildCareCosts;
  private int identifer;
  private decimal? wageEarnerGrossIncome;
  private decimal? selfEmploymentGrossIncome;
  private decimal? reasonableBusinessExpense;
  private decimal? courtOrderedChildSupportPaid;
  private string childSupprtPaidCourtOrderNo;
  private decimal? courtOrderedMaintenancePaid;
  private string maintenancePaidCourtOrderNo;
  private decimal? courtOrderedMaintenanceRecvd;
  private string maintenanceRecvdCourtOrderNo;
  private decimal? healthAndDentalInsurancePrem;
  private string eligibleForFederalTaxCredit;
  private string eligibleForKansasTaxCredit;
  private decimal? netAdjParentalChildSuppAmt;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string enforcementFeeType;
  private int? enforcementFeeAllowance;
  private decimal? insuranceWorkRelatedCcCredit;
  private string abilityToPay;
  private decimal? equalParentingTimeObligation;
  private decimal? socialSecDependentBenefit;
  private int croIdentifier;
  private string croType;
  private string casNumber;
  private string cspNumber;
  private long cswIdentifier;
  private int cssGuidelineYr;
}

// The source file: OCSE34, ID: 372373918, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// FINAN:  Contains totals for the Child Support Enforcement program, quarterly
/// report of collections (OCSE-34 report).
/// </summary>
[Serializable]
public partial class Ocse34: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Ocse34()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Ocse34(Ocse34 that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Ocse34 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the PERIOD attribute.
  /// Identifier for OCSE34. The year and quarter for which the report was 
  /// produced.
  /// </summary>
  [JsonPropertyName("period")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 6)]
  public int Period
  {
    get => Get<int?>("period") ?? 0;
    set => Set("period", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the PREVIOUS_UNDISTRIB_AMOUNT attribute.
  /// Balance remaining undistributed from previous (From line 9B last quarter).
  /// Line 1 on the OCSE-34 report.
  /// </summary>
  [JsonPropertyName("previousUndistribAmount")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int PreviousUndistribAmount
  {
    get => Get<int?>("previousUndistribAmount") ?? 0;
    set => Set("previousUndistribAmount", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the TOTAL_COLLECTIONS_AMOUNT attribute.
  /// Total collections received during the quarter. Line 2 on the OCSE-34 
  /// report.
  /// </summary>
  [JsonPropertyName("totalCollectionsAmount")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int TotalCollectionsAmount
  {
    get => Get<int?>("totalCollectionsAmount") ?? 0;
    set => Set("totalCollectionsAmount", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the OFFSET_FEDERAL_TAXREFUND_AMOUNT attribute.
  /// Collections received from offset of federal tax refund.  Line 2A on report
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("offsetFederalTaxrefundAmount")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OffsetFederalTaxrefundAmount
  {
    get => Get<int?>("offsetFederalTaxrefundAmount");
    set => Set("offsetFederalTaxrefundAmount", value);
  }

  /// <summary>
  /// The value of the OFFSET_STATE_TAX_REFUND_AMOUNT attribute.
  /// Collections received from offset of state tax refund.  Line 2B on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("offsetStateTaxRefundAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OffsetStateTaxRefundAmount
  {
    get => Get<int?>("offsetStateTaxRefundAmount");
    set => Set("offsetStateTaxRefundAmount", value);
  }

  /// <summary>
  /// The value of the UNEMPLOYMENT_COMP_AMOUNT attribute.
  /// Collections received from offset of unemployment compensations.  Line 2C 
  /// on report OCSE-34.
  /// </summary>
  [JsonPropertyName("unemploymentCompAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? UnemploymentCompAmount
  {
    get => Get<int?>("unemploymentCompAmount");
    set => Set("unemploymentCompAmount", value);
  }

  /// <summary>
  /// The value of the ADMINSTRATIVE_ENFORCE_AMOUNT attribute.
  /// Collections received from Administrative Enforcement.  Line 2D on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("adminstrativeEnforceAmount")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? AdminstrativeEnforceAmount
  {
    get => Get<int?>("adminstrativeEnforceAmount");
    set => Set("adminstrativeEnforceAmount", value);
  }

  /// <summary>
  /// The value of the INCOME_WITHHOLDING_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("incomeWithholdingAmount")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncomeWithholdingAmount
  {
    get => Get<int?>("incomeWithholdingAmount");
    set => Set("incomeWithholdingAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_STATES_AMOUNT attribute.
  /// Collections received from other states.  Line 2F on report OCSE-34.
  /// </summary>
  [JsonPropertyName("otherStatesAmount")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherStatesAmount
  {
    get => Get<int?>("otherStatesAmount");
    set => Set("otherStatesAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_SOURCES_AMOUNT attribute.
  /// Collections received from other sources.  Line 2G on report OCSE-34.
  /// </summary>
  [JsonPropertyName("otherSourcesAmount")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherSourcesAmount
  {
    get => Get<int?>("otherSourcesAmount");
    set => Set("otherSourcesAmount", value);
  }

  /// <summary>
  /// The value of the ADJUSTMENTS_AMOUNT attribute.
  /// Net amount of increasing and decreasing adjustments.  Line 3 on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("adjustmentsAmount")]
  [Member(Index = 11, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? AdjustmentsAmount
  {
    get => Get<int?>("adjustmentsAmount");
    set => Set("adjustmentsAmount", value);
  }

  /// <summary>
  /// The value of the NON_IVD_CASES_AMOUNT attribute.
  /// Collections forwarded to non IV-D cases.  Line 4 on report OCSE-34.
  /// </summary>
  [JsonPropertyName("nonIvdCasesAmount")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NonIvdCasesAmount
  {
    get => Get<int?>("nonIvdCasesAmount");
    set => Set("nonIvdCasesAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_STATES_CURRENT_IVA_AMOUNT attribute.
  /// Collections forwarded to other states (current IV-A assistance).  Line 5(A
  /// ) on report OCSE-34.
  /// </summary>
  [JsonPropertyName("otherStatesCurrentIvaAmount")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherStatesCurrentIvaAmount
  {
    get => Get<int?>("otherStatesCurrentIvaAmount");
    set => Set("otherStatesCurrentIvaAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_STATES_CURRENT_IVE_AMOUNT attribute.
  /// Collections forwarded to other states (current IV-E assistance).  Line 5(B
  /// ) on report OCSE-34.
  /// </summary>
  [JsonPropertyName("otherStatesCurrentIveAmount")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherStatesCurrentIveAmount
  {
    get => Get<int?>("otherStatesCurrentIveAmount");
    set => Set("otherStatesCurrentIveAmount", value);
  }

  /// <summary>
  /// The value of the OTHERSTATE_FORMER_ASSIST_AMOUNT attribute.
  /// Collections forwarded to other states (former assistance). Line 5(C) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("otherstateFormerAssistAmount")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherstateFormerAssistAmount
  {
    get => Get<int?>("otherstateFormerAssistAmount");
    set => Set("otherstateFormerAssistAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_STATE_NEVER_ASSIST_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("otherStateNeverAssistAmount")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherStateNeverAssistAmount
  {
    get => Get<int?>("otherStateNeverAssistAmount");
    set => Set("otherStateNeverAssistAmount", value);
  }

  /// <summary>
  /// The value of the OTHER_STATE_AMT_FORWARD attribute.
  /// Total collections forwarded to other states. Line 5 on report OCSE-34. 
  /// This is a total of current IV-A assistance (5A), current IV-E assistance (
  /// 5B), former assistance (5C) and never assistance (5D).
  /// </summary>
  [JsonPropertyName("otherStateAmtForward")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherStateAmtForward
  {
    get => Get<int?>("otherStateAmtForward");
    set => Set("otherStateAmtForward", value);
  }

  /// <summary>
  /// The value of the AVAIL_FOR_DISTRIBUTION_AMOUNT attribute.
  /// Collections available for distribution.  Line 6 on report OCSE-34.
  /// </summary>
  [JsonPropertyName("availForDistributionAmount")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? AvailForDistributionAmount
  {
    get => Get<int?>("availForDistributionAmount");
    set => Set("availForDistributionAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIB_ASSIST_REIMB_IVA_AMOUNT attribute.
  /// Distributed as assistance reimbursement (current IV-A assistance).  Line 
  /// 7a(A) on report OCSE-34.
  /// </summary>
  [JsonPropertyName("distribAssistReimbIvaAmount")]
  [Member(Index = 19, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistribAssistReimbIvaAmount
  {
    get => Get<int?>("distribAssistReimbIvaAmount");
    set => Set("distribAssistReimbIvaAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIB_ASSIST_REIMB_IVE_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("distribAssistReimbIveAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistribAssistReimbIveAmount
  {
    get => Get<int?>("distribAssistReimbIveAmount");
    set => Set("distribAssistReimbIveAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIB_ASSIST_REIMB_FMR_AMOUNT attribute.
  /// Distributed as assistance reimbursement (former assistance).  Line 7a(C) 
  /// on report OCSE-34.
  /// </summary>
  [JsonPropertyName("distribAssistReimbFmrAmount")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistribAssistReimbFmrAmount
  {
    get => Get<int?>("distribAssistReimbFmrAmount");
    set => Set("distribAssistReimbFmrAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIB_ASSIST_REIMB_AMOUNT attribute.
  /// Distributed as assistance reimbursement.  Line 7a on report OCSE-34.  This
  /// is a total of current IV-A assistance (7aA), current IV-E assistance (7aB
  /// ) and former assistance (7aC).
  /// </summary>
  [JsonPropertyName("distribAssistReimbAmount")]
  [Member(Index = 22, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistribAssistReimbAmount
  {
    get => Get<int?>("distribAssistReimbAmount");
    set => Set("distribAssistReimbAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_MED_SUPPORT_IVA_AMT attribute.
  /// Distributed as medical support, current IV-A assistance.  Line 7b(A) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedMedSupportIvaAmt")]
  [Member(Index = 23, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedMedSupportIvaAmt
  {
    get => Get<int?>("distributedMedSupportIvaAmt");
    set => Set("distributedMedSupportIvaAmt", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_MED_SUPPORT_IVE_AMT attribute.
  /// Distributed as medical support, current IV-E assistance.  Line 7b(B) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedMedSupportIveAmt")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedMedSupportIveAmt
  {
    get => Get<int?>("distributedMedSupportIveAmt");
    set => Set("distributedMedSupportIveAmt", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_MED_SUPPORT_FMR_AMT attribute.
  /// Distributed as medical support, former assistance.  Line 7b(C) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedMedSupportFmrAmt")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedMedSupportFmrAmt
  {
    get => Get<int?>("distributedMedSupportFmrAmt");
    set => Set("distributedMedSupportFmrAmt", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_MED_SUPPORT_NVR_AMT attribute.
  /// Distributed as medical support, never assistance.  Line 7b(D) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedMedSupportNvrAmt")]
  [Member(Index = 26, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedMedSupportNvrAmt
  {
    get => Get<int?>("distributedMedSupportNvrAmt");
    set => Set("distributedMedSupportNvrAmt", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_MED_SUPPORT_AMOUNT attribute.
  /// Total collections distributed as medical support.  Line 7b on report OCSE-
  /// 34.  This is a total of curent IV-A assistance (7bA), current IV-E
  /// assistance (7bB), former assistance (7bC) and never assistance (7bD).
  /// </summary>
  [JsonPropertyName("distributedMedSupportAmount")]
  [Member(Index = 27, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedMedSupportAmount
  {
    get => Get<int?>("distributedMedSupportAmount");
    set => Set("distributedMedSupportAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_FAMILY_IVA_AMOUNT attribute.
  /// Distributed as to family, current IV-A assistance.  Line 7c(A) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedFamilyIvaAmount")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedFamilyIvaAmount
  {
    get => Get<int?>("distributedFamilyIvaAmount");
    set => Set("distributedFamilyIvaAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_FAMILY_IVE_AMOUNT attribute.
  /// Distributed as to family, current IV-E assistance.  Line 7c(B) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedFamilyIveAmount")]
  [Member(Index = 29, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedFamilyIveAmount
  {
    get => Get<int?>("distributedFamilyIveAmount");
    set => Set("distributedFamilyIveAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_FAMILY_FORMER_AMT attribute.
  /// Distributed as to family, former assistance.  Line 7c(C) on report OCSE-
  /// 34.
  /// </summary>
  [JsonPropertyName("distributedFamilyFormerAmt")]
  [Member(Index = 30, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedFamilyFormerAmt
  {
    get => Get<int?>("distributedFamilyFormerAmt");
    set => Set("distributedFamilyFormerAmt", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_FAMILY_NEVER_AMOUNT attribute.
  /// Distributed as to family, never assistance.  Line 7c(D) on report OCSE-34.
  /// </summary>
  [JsonPropertyName("distributedFamilyNeverAmount")]
  [Member(Index = 31, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedFamilyNeverAmount
  {
    get => Get<int?>("distributedFamilyNeverAmount");
    set => Set("distributedFamilyNeverAmount", value);
  }

  /// <summary>
  /// The value of the DISTRIBUTED_FAMILY_AMOUNT attribute.
  /// Total distributed to family.  Line 7c on report OCSE-34.  This is a total 
  /// of current IV-A assistance (7cA), current IV-E assistance (7cB), former
  /// assistance (7cC) and never assistance (7cD).
  /// </summary>
  [JsonPropertyName("distributedFamilyAmount")]
  [Member(Index = 32, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistributedFamilyAmount
  {
    get => Get<int?>("distributedFamilyAmount");
    set => Set("distributedFamilyAmount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DISTRIBUTED_IVA_AMOUNT attribute.
  /// Total collections distributed, current IV-A assistance.  Line 8(A) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("totalDistributedIvaAmount")]
  [Member(Index = 33, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalDistributedIvaAmount
  {
    get => Get<int?>("totalDistributedIvaAmount");
    set => Set("totalDistributedIvaAmount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DISTRIBUTED_IVE_AMOUNT attribute.
  /// Total collections distributed, current IV-E assistance.  Line 8(B) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("totalDistributedIveAmount")]
  [Member(Index = 34, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalDistributedIveAmount
  {
    get => Get<int?>("totalDistributedIveAmount");
    set => Set("totalDistributedIveAmount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DISTRIBUTED_FORMER_AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("totalDistributedFormerAmount")]
  [Member(Index = 35, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalDistributedFormerAmount
  {
    get => Get<int?>("totalDistributedFormerAmount");
    set => Set("totalDistributedFormerAmount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DISTRIBUTED_NEVER_AMOUNT attribute.
  /// Total collections distributed, never assistance.  Line 8(D) on report OCSE
  /// -34.
  /// </summary>
  [JsonPropertyName("totalDistributedNeverAmount")]
  [Member(Index = 36, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalDistributedNeverAmount
  {
    get => Get<int?>("totalDistributedNeverAmount");
    set => Set("totalDistributedNeverAmount", value);
  }

  /// <summary>
  /// The value of the TOTAL_DISTRIBUTED_AMOUNT attribute.
  /// Total collections distributed.  Line 8 on report OCSE-34.  This is a total
  /// of current IV-A assistance (8A), current IV-E assistance (8B), former
  /// assistance (8C) and never assistance (8D).
  /// </summary>
  [JsonPropertyName("totalDistributedAmount")]
  [Member(Index = 37, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotalDistributedAmount
  {
    get => Get<int?>("totalDistributedAmount");
    set => Set("totalDistributedAmount", value);
  }

  /// <summary>
  /// The value of the GROSS_UNDISTRIBUTED_AMOUNT attribute.
  /// Total undistributed collections.  Line 9 on report OCSE-34.
  /// </summary>
  [JsonPropertyName("grossUndistributedAmount")]
  [Member(Index = 38, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? GrossUndistributedAmount
  {
    get => Get<int?>("grossUndistributedAmount");
    set => Set("grossUndistributedAmount", value);
  }

  /// <summary>
  /// The value of the UNDISTRIBUTED_AMOUNT attribute.
  /// Undistributed collections.  Line 9a on report OCSE-34.
  /// </summary>
  [JsonPropertyName("undistributedAmount")]
  [Member(Index = 39, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? UndistributedAmount
  {
    get => Get<int?>("undistributedAmount");
    set => Set("undistributedAmount", value);
  }

  /// <summary>
  /// The value of the NET_UNDISTRIBUTED_AMOUNT attribute.
  /// Total undistributed collections.  Line 9b on report OCSE-34.
  /// </summary>
  [JsonPropertyName("netUndistributedAmount")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetUndistributedAmount
  {
    get => Get<int?>("netUndistributedAmount");
    set => Set("netUndistributedAmount", value);
  }

  /// <summary>
  /// The value of the FEDERAL_SHARE_IVA_AMOUNT attribute.
  /// Federal share of collections, current IV-A assistance.  Line 10(A) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("federalShareIvaAmount")]
  [Member(Index = 41, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FederalShareIvaAmount
  {
    get => Get<int?>("federalShareIvaAmount");
    set => Set("federalShareIvaAmount", value);
  }

  /// <summary>
  /// The value of the FEDERAL_SHARE_IVE_AMOUNT attribute.
  /// Federal share of collections, current IV-E assistance.  Line 10(B) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("federalShareIveAmount")]
  [Member(Index = 42, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FederalShareIveAmount
  {
    get => Get<int?>("federalShareIveAmount");
    set => Set("federalShareIveAmount", value);
  }

  /// <summary>
  /// The value of the FEDERAL_SHARE_FORMER_AMOUNT attribute.
  /// Federal share of collections, former assistance.  Line 10(C) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("federalShareFormerAmount")]
  [Member(Index = 43, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FederalShareFormerAmount
  {
    get => Get<int?>("federalShareFormerAmount");
    set => Set("federalShareFormerAmount", value);
  }

  /// <summary>
  /// The value of the FEDERAL_SHARE_TOTAL_AMOUNT attribute.
  /// Total federal share of collections.  Line 10 on report OCSE-34.  This is a
  /// total of current IV-A assistance (10A), current IV-E assistance (10B) and
  /// former assistance (10C).
  /// </summary>
  [JsonPropertyName("federalShareTotalAmount")]
  [Member(Index = 44, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FederalShareTotalAmount
  {
    get => Get<int?>("federalShareTotalAmount");
    set => Set("federalShareTotalAmount", value);
  }

  /// <summary>
  /// The value of the INCENTIVE_PAYMENT_IVA_AMOUNT attribute.
  /// Estimated incentive payments, current IV-A assistance.  Line 11(A) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("incentivePaymentIvaAmount")]
  [Member(Index = 45, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncentivePaymentIvaAmount
  {
    get => Get<int?>("incentivePaymentIvaAmount");
    set => Set("incentivePaymentIvaAmount", value);
  }

  /// <summary>
  /// The value of the INCENTIVE_PAYMENT_FORMER_AMOUNT attribute.
  /// Estimated incentive payments, former assistance.  Line 11(C) on report 
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("incentivePaymentFormerAmount")]
  [Member(Index = 46, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncentivePaymentFormerAmount
  {
    get => Get<int?>("incentivePaymentFormerAmount");
    set => Set("incentivePaymentFormerAmount", value);
  }

  /// <summary>
  /// The value of the INCENTIVE_PAYMENT_AMOUNT attribute.
  /// Total estimated incentive payments.  Line 11 on report OCSE-34.  This is a
  /// total of current IV-A assistance (11A) and former assistance (11C).
  /// </summary>
  [JsonPropertyName("incentivePaymentAmount")]
  [Member(Index = 47, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IncentivePaymentAmount
  {
    get => Get<int?>("incentivePaymentAmount");
    set => Set("incentivePaymentAmount", value);
  }

  /// <summary>
  /// The value of the NET_FEDERAL_SHARE_IVA_AMOUNT attribute.
  /// Net federal share of collections, current IV-A assistance.  Line 12(A) on 
  /// report OCSE-34.
  /// </summary>
  [JsonPropertyName("netFederalShareIvaAmount")]
  [Member(Index = 48, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetFederalShareIvaAmount
  {
    get => Get<int?>("netFederalShareIvaAmount");
    set => Set("netFederalShareIvaAmount", value);
  }

  /// <summary>
  /// The value of the NET_FEDERAL_SHARE_FORMER_AMOUNT attribute.
  /// Net federal share of collections, former assistance.  Line 12(A) on report
  /// OCSE-34.
  /// </summary>
  [JsonPropertyName("netFederalShareFormerAmount")]
  [Member(Index = 49, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetFederalShareFormerAmount
  {
    get => Get<int?>("netFederalShareFormerAmount");
    set => Set("netFederalShareFormerAmount", value);
  }

  /// <summary>
  /// The value of the NET_FEDERAL_SHARE_AMOUNT attribute.
  /// Net federal share of collections, former assistance.  Line 12(A) on report
  /// OCSE-34.  This is a total of current IV-A assistance (12A) and former
  /// assistance (12C).
  /// </summary>
  [JsonPropertyName("netFederalShareAmount")]
  [Member(Index = 50, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetFederalShareAmount
  {
    get => Get<int?>("netFederalShareAmount");
    set => Set("netFederalShareAmount", value);
  }

  /// <summary>
  /// The value of the FEES_RETAIN_OTHER_STATES_AMOUNT attribute.
  /// Fees retained by other states.  Line 13 on report OCSE-34.
  /// </summary>
  [JsonPropertyName("feesRetainOtherStatesAmount")]
  [Member(Index = 51, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FeesRetainOtherStatesAmount
  {
    get => Get<int?>("feesRetainOtherStatesAmount");
    set => Set("feesRetainOtherStatesAmount", value);
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Identifier for ocse34. The date and time the ocse-34 report was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 52, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user who created the ocse-34 report.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// The value of the FMAP_RATE attribute.
  /// The rate is used for calculating the Federal Share of Collections (line10)
  /// amounts on the OCSE-34A report.
  /// </summary>
  [JsonPropertyName("fmapRate")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 54, Type = MemberType.Number, Length = 4, Precision = 4)]
  public decimal FmapRate
  {
    get => Get<decimal?>("fmapRate") ?? 0M;
    set => Set("fmapRate", value == 0M ? null : Truncate(value, 4) as decimal?);
  }

  /// <summary>
  /// The value of the OTH_ST_FMR_2_AMT attribute.
  /// New amount of former assistance on form.
  /// </summary>
  [JsonPropertyName("othStFmr2Amt")]
  [Member(Index = 55, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OthStFmr2Amt
  {
    get => Get<int?>("othStFmr2Amt");
    set => Set("othStFmr2Amt", value);
  }

  /// <summary>
  /// The value of the OTH_ST_NEVR_2_AMT attribute.
  /// New amount of never assistance on form.
  /// </summary>
  [JsonPropertyName("othStNevr2Amt")]
  [Member(Index = 56, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OthStNevr2Amt
  {
    get => Get<int?>("othStNevr2Amt");
    set => Set("othStNevr2Amt", value);
  }

  /// <summary>
  /// The value of the DISTRIB_FMR_2_AMT attribute.
  /// New amount of former distributed assistance on form.
  /// </summary>
  [JsonPropertyName("distribFmr2Amt")]
  [Member(Index = 57, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistribFmr2Amt
  {
    get => Get<int?>("distribFmr2Amt");
    set => Set("distribFmr2Amt", value);
  }

  /// <summary>
  /// The value of the DIST_MED_FMR_2_AMT attribute.
  /// New amount of distributed medical former assistance on form.
  /// </summary>
  [JsonPropertyName("distMedFmr2Amt")]
  [Member(Index = 58, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistMedFmr2Amt
  {
    get => Get<int?>("distMedFmr2Amt");
    set => Set("distMedFmr2Amt", value);
  }

  /// <summary>
  /// The value of the DIST_MED_NVR_2_AMT attribute.
  /// New amount of distributed medical never assistance on form.
  /// </summary>
  [JsonPropertyName("distMedNvr2Amt")]
  [Member(Index = 59, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistMedNvr2Amt
  {
    get => Get<int?>("distMedNvr2Amt");
    set => Set("distMedNvr2Amt", value);
  }

  /// <summary>
  /// The value of the DIST_FAM_FMR_2_AMT attribute.
  /// New amount of distributed family former assistance on form.
  /// </summary>
  [JsonPropertyName("distFamFmr2Amt")]
  [Member(Index = 60, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistFamFmr2Amt
  {
    get => Get<int?>("distFamFmr2Amt");
    set => Set("distFamFmr2Amt", value);
  }

  /// <summary>
  /// The value of the DIST_FAM_NVR_2_AMT attribute.
  /// New amount of distributed family never assistance on form.
  /// </summary>
  [JsonPropertyName("distFamNvr2Amt")]
  [Member(Index = 61, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DistFamNvr2Amt
  {
    get => Get<int?>("distFamNvr2Amt");
    set => Set("distFamNvr2Amt", value);
  }

  /// <summary>
  /// The value of the TOT_DIST_FMR_2_AMT attribute.
  /// New amount of total distributed former assistance on form.
  /// </summary>
  [JsonPropertyName("totDistFmr2Amt")]
  [Member(Index = 62, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotDistFmr2Amt
  {
    get => Get<int?>("totDistFmr2Amt");
    set => Set("totDistFmr2Amt", value);
  }

  /// <summary>
  /// The value of the TOT_DIST_NVR_2_AMT attribute.
  /// New amount of total distributed never assistance on form.
  /// </summary>
  [JsonPropertyName("totDistNvr2Amt")]
  [Member(Index = 63, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? TotDistNvr2Amt
  {
    get => Get<int?>("totDistNvr2Amt");
    set => Set("totDistNvr2Amt", value);
  }

  /// <summary>
  /// The value of the NET_UNDIST_PND_AMT attribute.
  /// New amount of net undistributed pending on form.
  /// </summary>
  [JsonPropertyName("netUndistPndAmt")]
  [Member(Index = 64, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetUndistPndAmt
  {
    get => Get<int?>("netUndistPndAmt");
    set => Set("netUndistPndAmt", value);
  }

  /// <summary>
  /// The value of the NET_UNDIST_URS_AMT attribute.
  /// New amount of net undistributed unresolved on form.
  /// </summary>
  [JsonPropertyName("netUndistUrsAmt")]
  [Member(Index = 65, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? NetUndistUrsAmt
  {
    get => Get<int?>("netUndistUrsAmt");
    set => Set("netUndistUrsAmt", value);
  }

  /// <summary>
  /// The value of the FED_SHR_FMR_2_AMT attribute.
  /// New amount of federal share former assistance on form.
  /// </summary>
  [JsonPropertyName("fedShrFmr2Amt")]
  [Member(Index = 66, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FedShrFmr2Amt
  {
    get => Get<int?>("fedShrFmr2Amt");
    set => Set("fedShrFmr2Amt", value);
  }

  /// <summary>
  /// The value of the FED_SHR_2_AMT attribute.
  /// New amount of federal share assistance on form.
  /// </summary>
  [JsonPropertyName("fedShr2Amt")]
  [Member(Index = 67, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FedShr2Amt
  {
    get => Get<int?>("fedShr2Amt");
    set => Set("fedShr2Amt", value);
  }

  /// <summary>
  /// The value of the CSE_DISB_CREDIT_AMT attribute.
  /// new amount storing cse disbursement credits amount.
  /// </summary>
  [JsonPropertyName("cseDisbCreditAmt")]
  [Member(Index = 68, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseDisbCreditAmt
  {
    get => Get<int?>("cseDisbCreditAmt");
    set => Set("cseDisbCreditAmt", value);
  }

  /// <summary>
  /// The value of the CSE_DISB_DEBIT_AMT attribute.
  /// new amount storing the cse disbursement debit amount.
  /// </summary>
  [JsonPropertyName("cseDisbDebitAmt")]
  [Member(Index = 69, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseDisbDebitAmt
  {
    get => Get<int?>("cseDisbDebitAmt");
    set => Set("cseDisbDebitAmt", value);
  }

  /// <summary>
  /// The value of the CSE_WARRANT_AMT attribute.
  /// new amount storing the cse warrant error total amount.
  /// </summary>
  [JsonPropertyName("cseWarrantAmt")]
  [Member(Index = 70, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseWarrantAmt
  {
    get => Get<int?>("cseWarrantAmt");
    set => Set("cseWarrantAmt", value);
  }

  /// <summary>
  /// The value of the CSE_PAYMENT_AMT attribute.
  /// new amount storing the cse payment error total amount.
  /// </summary>
  [JsonPropertyName("csePaymentAmt")]
  [Member(Index = 71, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CsePaymentAmt
  {
    get => Get<int?>("csePaymentAmt");
    set => Set("csePaymentAmt", value);
  }

  /// <summary>
  /// The value of the CSE_INTERSTATE_AMT attribute.
  /// new amount storing the cse interstate error total amount.
  /// </summary>
  [JsonPropertyName("cseInterstateAmt")]
  [Member(Index = 72, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseInterstateAmt
  {
    get => Get<int?>("cseInterstateAmt");
    set => Set("cseInterstateAmt", value);
  }

  /// <summary>
  /// The value of the CSE_CSH_RCPT_DTL_SUSP_AMT attribute.
  /// new amount storing the cash receipt detail suspense amount.
  /// </summary>
  [JsonPropertyName("cseCshRcptDtlSuspAmt")]
  [Member(Index = 73, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseCshRcptDtlSuspAmt
  {
    get => Get<int?>("cseCshRcptDtlSuspAmt");
    set => Set("cseCshRcptDtlSuspAmt", value);
  }

  /// <summary>
  /// The value of the CSE_DISB_SUPPRESS_AMT attribute.
  /// new amount storing the cse suppressed disbursements amount.
  /// </summary>
  [JsonPropertyName("cseDisbSuppressAmt")]
  [Member(Index = 74, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CseDisbSuppressAmt
  {
    get => Get<int?>("cseDisbSuppressAmt");
    set => Set("cseDisbSuppressAmt", value);
  }

  /// <summary>
  /// The value of the REPORTING_PERIOD_BEGIN_DATE attribute.
  /// new amount storing the reporting period begin date.
  /// </summary>
  [JsonPropertyName("reportingPeriodBeginDate")]
  [Member(Index = 75, Type = MemberType.Date, Optional = true)]
  public DateTime? ReportingPeriodBeginDate
  {
    get => Get<DateTime?>("reportingPeriodBeginDate");
    set => Set("reportingPeriodBeginDate", value);
  }

  /// <summary>
  /// The value of the REPORTING_PERIOD_END_DATE attribute.
  /// new amount storing the reporting period end date.
  /// </summary>
  [JsonPropertyName("reportingPeriodEndDate")]
  [Member(Index = 76, Type = MemberType.Date, Optional = true)]
  public DateTime? ReportingPeriodEndDate
  {
    get => Get<DateTime?>("reportingPeriodEndDate");
    set => Set("reportingPeriodEndDate", value);
  }

  /// <summary>Length of the ADJUST_FOOTER_TEXT attribute.</summary>
  public const int AdjustFooterText_MaxLength = 77;

  /// <summary>
  /// The value of the ADJUST_FOOTER_TEXT attribute.
  /// new field storing supporting text for line 3.
  /// </summary>
  [JsonPropertyName("adjustFooterText")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = AdjustFooterText_MaxLength, Optional = true)]
  public string AdjustFooterText
  {
    get => Get<string>("adjustFooterText");
    set => Set(
      "adjustFooterText",
      TrimEnd(Substring(value, 1, AdjustFooterText_MaxLength)));
  }

  /// <summary>
  /// The value of the KPC_NON_4D_IWO_COLL_AMT attribute.
  /// new amount storing the KPC non_IVD IWO amount.
  /// </summary>
  [JsonPropertyName("kpcNon4DIwoCollAmt")]
  [Member(Index = 78, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcNon4DIwoCollAmt
  {
    get => Get<int?>("kpcNon4DIwoCollAmt");
    set => Set("kpcNon4DIwoCollAmt", value);
  }

  /// <summary>
  /// The value of the KPC_IVD_NON_IWO_COLL_AMT attribute.
  /// new amount storing the KPC IVD non-IWO amount.
  /// </summary>
  [JsonPropertyName("kpcIvdNonIwoCollAmt")]
  [Member(Index = 79, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcIvdNonIwoCollAmt
  {
    get => Get<int?>("kpcIvdNonIwoCollAmt");
    set => Set("kpcIvdNonIwoCollAmt", value);
  }

  /// <summary>
  /// The value of the KPC_NON_IVD_IWO_FORW_COLL_AMT attribute.
  /// new amount storing the KPC non ivd iwo forwarded collection amount.
  /// </summary>
  [JsonPropertyName("kpcNonIvdIwoForwCollAmt")]
  [Member(Index = 80, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcNonIvdIwoForwCollAmt
  {
    get => Get<int?>("kpcNonIvdIwoForwCollAmt");
    set => Set("kpcNonIvdIwoForwCollAmt", value);
  }

  /// <summary>
  /// The value of the KPC_STALE_DATE_AMT attribute.
  /// new amount storing the KPC stale date amount.
  /// </summary>
  [JsonPropertyName("kpcStaleDateAmt")]
  [Member(Index = 81, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcStaleDateAmt
  {
    get => Get<int?>("kpcStaleDateAmt");
    set => Set("kpcStaleDateAmt", value);
  }

  /// <summary>
  /// The value of the KPC_HELD_DISB_AMT attribute.
  /// new amount storing the KPC held disbursement amount.
  /// </summary>
  [JsonPropertyName("kpcHeldDisbAmt")]
  [Member(Index = 82, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcHeldDisbAmt
  {
    get => Get<int?>("kpcHeldDisbAmt");
    set => Set("kpcHeldDisbAmt", value);
  }

  /// <summary>
  /// The value of the UI_IVD_NON_IWO_INT_AMT attribute.
  /// new amount storing the KPC unidentified IVD non-IWO interstate amount.
  /// </summary>
  [JsonPropertyName("uiIvdNonIwoIntAmt")]
  [Member(Index = 83, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? UiIvdNonIwoIntAmt
  {
    get => Get<int?>("uiIvdNonIwoIntAmt");
    set => Set("uiIvdNonIwoIntAmt", value);
  }

  /// <summary>
  /// The value of the KPC_UI_IVD_NON_IWO_NON_INT_AMT attribute.
  /// new amount storing the KPC unidentified IVD non-IWO non-interstate amount.
  /// </summary>
  [JsonPropertyName("kpcUiIvdNonIwoNonIntAmt")]
  [Member(Index = 84, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiIvdNonIwoNonIntAmt
  {
    get => Get<int?>("kpcUiIvdNonIwoNonIntAmt");
    set => Set("kpcUiIvdNonIwoNonIntAmt", value);
  }

  /// <summary>
  /// The value of the KPC_UI_IVD_NIVD_IWO_AMT attribute.
  /// new amount storing the KPC unidentified IVD non-IVD IWO amounts.
  /// </summary>
  [JsonPropertyName("kpcUiIvdNivdIwoAmt")]
  [Member(Index = 85, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiIvdNivdIwoAmt
  {
    get => Get<int?>("kpcUiIvdNivdIwoAmt");
    set => Set("kpcUiIvdNivdIwoAmt", value);
  }

  /// <summary>
  /// The value of the KPC_UI_NON_IVD_IWO_AMT attribute.
  /// new amount storing the KPC unidentified non-IVD IWO amount.
  /// </summary>
  [JsonPropertyName("kpcUiNonIvdIwoAmt")]
  [Member(Index = 86, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiNonIvdIwoAmt
  {
    get => Get<int?>("kpcUiNonIvdIwoAmt");
    set => Set("kpcUiNonIvdIwoAmt", value);
  }

  /// <summary>
  /// The value of the KPC_NIVD_IWO_LDA attribute.
  /// New amount storing the KPC non-IVD IWO last business day amount.
  /// </summary>
  [JsonPropertyName("kpcNivdIwoLda")]
  [Member(Index = 87, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcNivdIwoLda
  {
    get => Get<int?>("kpcNivdIwoLda");
    set => Set("kpcNivdIwoLda", value);
  }

  /// <summary>
  /// The value of the ALT_REPORTING_PERIOD_BEGIN_DATE attribute.
  /// New amount storing the alternate reporting period begin date.
  /// </summary>
  [JsonPropertyName("altReportingPeriodBeginDate")]
  [Member(Index = 88, Type = MemberType.Date, Optional = true)]
  public DateTime? AltReportingPeriodBeginDate
  {
    get => Get<DateTime?>("altReportingPeriodBeginDate");
    set => Set("altReportingPeriodBeginDate", value);
  }

  /// <summary>
  /// The value of the ALT_REPORTING_PERIOD_END_DATE attribute.
  /// New amount storing the alternate reporting period end date.
  /// </summary>
  [JsonPropertyName("altReportingPeriodEndDate")]
  [Member(Index = 89, Type = MemberType.Date, Optional = true)]
  public DateTime? AltReportingPeriodEndDate
  {
    get => Get<DateTime?>("altReportingPeriodEndDate");
    set => Set("altReportingPeriodEndDate", value);
  }

  /// <summary>
  /// The value of the FDSO_DSB_SUPP_AMT attribute.
  /// New amount fdso disbursement suppressed amounts.
  /// </summary>
  [JsonPropertyName("fdsoDsbSuppAmt")]
  [Member(Index = 90, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FdsoDsbSuppAmt
  {
    get => Get<int?>("fdsoDsbSuppAmt");
    set => Set("fdsoDsbSuppAmt", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CRD_LDA attribute.
  /// </summary>
  [JsonPropertyName("suspendCrdLda")]
  [Member(Index = 91, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrdLda
  {
    get => Get<int?>("suspendCrdLda");
    set => Set("suspendCrdLda", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CRD_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrdGt2")]
  [Member(Index = 92, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrdGt2
  {
    get => Get<int?>("suspendCrdGt2");
    set => Set("suspendCrdGt2", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CRD_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrdGt30")]
  [Member(Index = 93, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrdGt30
  {
    get => Get<int?>("suspendCrdGt30");
    set => Set("suspendCrdGt30", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CRD_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrdGt180")]
  [Member(Index = 94, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrdGt180
  {
    get => Get<int?>("suspendCrdGt180");
    set => Set("suspendCrdGt180", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CRD_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrdGt365")]
  [Member(Index = 95, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrdGt365
  {
    get => Get<int?>("suspendCrdGt365");
    set => Set("suspendCrdGt365", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CR_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrGt1095")]
  [Member(Index = 96, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrGt1095
  {
    get => Get<int?>("suspendCrGt1095");
    set => Set("suspendCrGt1095", value);
  }

  /// <summary>
  /// The value of the SUSPEND_CR_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("suspendCrGt1825")]
  [Member(Index = 97, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspendCrGt1825
  {
    get => Get<int?>("suspendCrGt1825");
    set => Set("suspendCrGt1825", value);
  }

  /// <summary>
  /// The value of the DISB_CREDIT_LDA attribute.
  /// </summary>
  [JsonPropertyName("disbCreditLda")]
  [Member(Index = 98, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCreditLda
  {
    get => Get<int?>("disbCreditLda");
    set => Set("disbCreditLda", value);
  }

  /// <summary>
  /// The value of the DISB_CREDIT_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("disbCreditGt2")]
  [Member(Index = 99, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCreditGt2
  {
    get => Get<int?>("disbCreditGt2");
    set => Set("disbCreditGt2", value);
  }

  /// <summary>
  /// The value of the DISB_CREDIT_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("disbCreditGt30")]
  [Member(Index = 100, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCreditGt30
  {
    get => Get<int?>("disbCreditGt30");
    set => Set("disbCreditGt30", value);
  }

  /// <summary>
  /// The value of the DISB_CREDIT_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("disbCreditGt180")]
  [Member(Index = 101, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCreditGt180
  {
    get => Get<int?>("disbCreditGt180");
    set => Set("disbCreditGt180", value);
  }

  /// <summary>
  /// The value of the DISB_CREDIT_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("disbCreditGt365")]
  [Member(Index = 102, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCreditGt365
  {
    get => Get<int?>("disbCreditGt365");
    set => Set("disbCreditGt365", value);
  }

  /// <summary>
  /// The value of the DISB_CRD_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("disbCrdGt1095")]
  [Member(Index = 103, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCrdGt1095
  {
    get => Get<int?>("disbCrdGt1095");
    set => Set("disbCrdGt1095", value);
  }

  /// <summary>
  /// The value of the DISB_CRD_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("disbCrdGt1825")]
  [Member(Index = 104, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbCrdGt1825
  {
    get => Get<int?>("disbCrdGt1825");
    set => Set("disbCrdGt1825", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_LDA attribute.
  /// </summary>
  [JsonPropertyName("disbDebitLda")]
  [Member(Index = 105, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitLda
  {
    get => Get<int?>("disbDebitLda");
    set => Set("disbDebitLda", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt2")]
  [Member(Index = 106, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt2
  {
    get => Get<int?>("disbDebitGt2");
    set => Set("disbDebitGt2", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt30")]
  [Member(Index = 107, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt30
  {
    get => Get<int?>("disbDebitGt30");
    set => Set("disbDebitGt30", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt180")]
  [Member(Index = 108, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt180
  {
    get => Get<int?>("disbDebitGt180");
    set => Set("disbDebitGt180", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt365")]
  [Member(Index = 109, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt365
  {
    get => Get<int?>("disbDebitGt365");
    set => Set("disbDebitGt365", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt1095")]
  [Member(Index = 110, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt1095
  {
    get => Get<int?>("disbDebitGt1095");
    set => Set("disbDebitGt1095", value);
  }

  /// <summary>
  /// The value of the DISB_DEBIT_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("disbDebitGt1825")]
  [Member(Index = 111, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DisbDebitGt1825
  {
    get => Get<int?>("disbDebitGt1825");
    set => Set("disbDebitGt1825", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_LDA attribute.
  /// </summary>
  [JsonPropertyName("payErrorLda")]
  [Member(Index = 112, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorLda
  {
    get => Get<int?>("payErrorLda");
    set => Set("payErrorLda", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt2")]
  [Member(Index = 113, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt2
  {
    get => Get<int?>("payErrorGt2");
    set => Set("payErrorGt2", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt30")]
  [Member(Index = 114, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt30
  {
    get => Get<int?>("payErrorGt30");
    set => Set("payErrorGt30", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt180")]
  [Member(Index = 115, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt180
  {
    get => Get<int?>("payErrorGt180");
    set => Set("payErrorGt180", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt365")]
  [Member(Index = 116, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt365
  {
    get => Get<int?>("payErrorGt365");
    set => Set("payErrorGt365", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt1095")]
  [Member(Index = 117, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt1095
  {
    get => Get<int?>("payErrorGt1095");
    set => Set("payErrorGt1095", value);
  }

  /// <summary>
  /// The value of the PAY_ERROR_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("payErrorGt1825")]
  [Member(Index = 118, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PayErrorGt1825
  {
    get => Get<int?>("payErrorGt1825");
    set => Set("payErrorGt1825", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_LDA attribute.
  /// </summary>
  [JsonPropertyName("warrErrorLda")]
  [Member(Index = 119, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorLda
  {
    get => Get<int?>("warrErrorLda");
    set => Set("warrErrorLda", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt2")]
  [Member(Index = 120, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt2
  {
    get => Get<int?>("warrErrorGt2");
    set => Set("warrErrorGt2", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt30")]
  [Member(Index = 121, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt30
  {
    get => Get<int?>("warrErrorGt30");
    set => Set("warrErrorGt30", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt180")]
  [Member(Index = 122, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt180
  {
    get => Get<int?>("warrErrorGt180");
    set => Set("warrErrorGt180", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt365")]
  [Member(Index = 123, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt365
  {
    get => Get<int?>("warrErrorGt365");
    set => Set("warrErrorGt365", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt1095")]
  [Member(Index = 124, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt1095
  {
    get => Get<int?>("warrErrorGt1095");
    set => Set("warrErrorGt1095", value);
  }

  /// <summary>
  /// The value of the WARR_ERROR_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("warrErrorGt1825")]
  [Member(Index = 125, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? WarrErrorGt1825
  {
    get => Get<int?>("warrErrorGt1825");
    set => Set("warrErrorGt1825", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_LDA attribute.
  /// </summary>
  [JsonPropertyName("intErrorLda")]
  [Member(Index = 126, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorLda
  {
    get => Get<int?>("intErrorLda");
    set => Set("intErrorLda", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt2")]
  [Member(Index = 127, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt2
  {
    get => Get<int?>("intErrorGt2");
    set => Set("intErrorGt2", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt30")]
  [Member(Index = 128, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt30
  {
    get => Get<int?>("intErrorGt30");
    set => Set("intErrorGt30", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt180")]
  [Member(Index = 129, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt180
  {
    get => Get<int?>("intErrorGt180");
    set => Set("intErrorGt180", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt365")]
  [Member(Index = 130, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt365
  {
    get => Get<int?>("intErrorGt365");
    set => Set("intErrorGt365", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt1095")]
  [Member(Index = 131, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt1095
  {
    get => Get<int?>("intErrorGt1095");
    set => Set("intErrorGt1095", value);
  }

  /// <summary>
  /// The value of the INT_ERROR_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("intErrorGt1825")]
  [Member(Index = 132, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? IntErrorGt1825
  {
    get => Get<int?>("intErrorGt1825");
    set => Set("intErrorGt1825", value);
  }

  /// <summary>
  /// The value of the HELD_LDA attribute.
  /// </summary>
  [JsonPropertyName("heldLda")]
  [Member(Index = 133, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldLda
  {
    get => Get<int?>("heldLda");
    set => Set("heldLda", value);
  }

  /// <summary>
  /// The value of the HELD_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("heldGt2")]
  [Member(Index = 134, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt2
  {
    get => Get<int?>("heldGt2");
    set => Set("heldGt2", value);
  }

  /// <summary>
  /// The value of the HELD_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("heldGt30")]
  [Member(Index = 135, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt30
  {
    get => Get<int?>("heldGt30");
    set => Set("heldGt30", value);
  }

  /// <summary>
  /// The value of the HELD_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("heldGt180")]
  [Member(Index = 136, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt180
  {
    get => Get<int?>("heldGt180");
    set => Set("heldGt180", value);
  }

  /// <summary>
  /// The value of the HELD_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("heldGt365")]
  [Member(Index = 137, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt365
  {
    get => Get<int?>("heldGt365");
    set => Set("heldGt365", value);
  }

  /// <summary>
  /// The value of the HELD_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("heldGt1095")]
  [Member(Index = 138, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt1095
  {
    get => Get<int?>("heldGt1095");
    set => Set("heldGt1095", value);
  }

  /// <summary>
  /// The value of the HELD_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("heldGt1825")]
  [Member(Index = 139, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? HeldGt1825
  {
    get => Get<int?>("heldGt1825");
    set => Set("heldGt1825", value);
  }

  /// <summary>
  /// The value of the STALE_LDA attribute.
  /// </summary>
  [JsonPropertyName("staleLda")]
  [Member(Index = 140, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleLda
  {
    get => Get<int?>("staleLda");
    set => Set("staleLda", value);
  }

  /// <summary>
  /// The value of the STALE_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("staleGt2")]
  [Member(Index = 141, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt2
  {
    get => Get<int?>("staleGt2");
    set => Set("staleGt2", value);
  }

  /// <summary>
  /// The value of the STALE_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("staleGt30")]
  [Member(Index = 142, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt30
  {
    get => Get<int?>("staleGt30");
    set => Set("staleGt30", value);
  }

  /// <summary>
  /// The value of the STALE_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("staleGt180")]
  [Member(Index = 143, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt180
  {
    get => Get<int?>("staleGt180");
    set => Set("staleGt180", value);
  }

  /// <summary>
  /// The value of the STALE_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("staleGt365")]
  [Member(Index = 144, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt365
  {
    get => Get<int?>("staleGt365");
    set => Set("staleGt365", value);
  }

  /// <summary>
  /// The value of the STALE_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("staleGt1095")]
  [Member(Index = 145, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt1095
  {
    get => Get<int?>("staleGt1095");
    set => Set("staleGt1095", value);
  }

  /// <summary>
  /// The value of the STALE_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("staleGt1825")]
  [Member(Index = 146, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? StaleGt1825
  {
    get => Get<int?>("staleGt1825");
    set => Set("staleGt1825", value);
  }

  /// <summary>
  /// The value of the KPC_UI_LDA attribute.
  /// </summary>
  [JsonPropertyName("kpcUiLda")]
  [Member(Index = 147, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiLda
  {
    get => Get<int?>("kpcUiLda");
    set => Set("kpcUiLda", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt2")]
  [Member(Index = 148, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt2
  {
    get => Get<int?>("kpcUiGt2");
    set => Set("kpcUiGt2", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt30")]
  [Member(Index = 149, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt30
  {
    get => Get<int?>("kpcUiGt30");
    set => Set("kpcUiGt30", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt180")]
  [Member(Index = 150, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt180
  {
    get => Get<int?>("kpcUiGt180");
    set => Set("kpcUiGt180", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt365")]
  [Member(Index = 151, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt365
  {
    get => Get<int?>("kpcUiGt365");
    set => Set("kpcUiGt365", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt1095")]
  [Member(Index = 152, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt1095
  {
    get => Get<int?>("kpcUiGt1095");
    set => Set("kpcUiGt1095", value);
  }

  /// <summary>
  /// The value of the KPC_UI_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("kpcUiGt1825")]
  [Member(Index = 153, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? KpcUiGt1825
  {
    get => Get<int?>("kpcUiGt1825");
    set => Set("kpcUiGt1825", value);
  }

  /// <summary>
  /// The value of the SUPPRESS_DISB_LDA attribute.
  /// </summary>
  [JsonPropertyName("suppressDisbLda")]
  [Member(Index = 154, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuppressDisbLda
  {
    get => Get<int?>("suppressDisbLda");
    set => Set("suppressDisbLda", value);
  }

  /// <summary>
  /// The value of the SUPPRESS_DISB_GT_2 attribute.
  /// </summary>
  [JsonPropertyName("suppressDisbGt2")]
  [Member(Index = 155, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuppressDisbGt2
  {
    get => Get<int?>("suppressDisbGt2");
    set => Set("suppressDisbGt2", value);
  }

  /// <summary>
  /// The value of the SUPRS_DISB_GT_30 attribute.
  /// </summary>
  [JsonPropertyName("suprsDisbGt30")]
  [Member(Index = 156, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuprsDisbGt30
  {
    get => Get<int?>("suprsDisbGt30");
    set => Set("suprsDisbGt30", value);
  }

  /// <summary>
  /// The value of the SUPRS_DISB_GT_180 attribute.
  /// </summary>
  [JsonPropertyName("suprsDisbGt180")]
  [Member(Index = 157, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuprsDisbGt180
  {
    get => Get<int?>("suprsDisbGt180");
    set => Set("suprsDisbGt180", value);
  }

  /// <summary>
  /// The value of the SUPRS_DISB_GT_365 attribute.
  /// </summary>
  [JsonPropertyName("suprsDisbGt365")]
  [Member(Index = 158, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuprsDisbGt365
  {
    get => Get<int?>("suprsDisbGt365");
    set => Set("suprsDisbGt365", value);
  }

  /// <summary>
  /// The value of the SUPRS_DISB_GT_1095 attribute.
  /// </summary>
  [JsonPropertyName("suprsDisbGt1095")]
  [Member(Index = 159, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuprsDisbGt1095
  {
    get => Get<int?>("suprsDisbGt1095");
    set => Set("suprsDisbGt1095", value);
  }

  /// <summary>
  /// The value of the SUPRS_DISB_GT_1825 attribute.
  /// </summary>
  [JsonPropertyName("suprsDisbGt1825")]
  [Member(Index = 160, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuprsDisbGt1825
  {
    get => Get<int?>("suprsDisbGt1825");
    set => Set("suprsDisbGt1825", value);
  }

  /// <summary>
  /// The value of the LAST_TWO_DAYS_COLL attribute.
  /// </summary>
  [JsonPropertyName("lastTwoDaysColl")]
  [Member(Index = 161, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LastTwoDaysColl
  {
    get => Get<int?>("lastTwoDaysColl");
    set => Set("lastTwoDaysColl", value);
  }

  /// <summary>
  /// The value of the SUSP_CRD_FOR_FUT attribute.
  /// </summary>
  [JsonPropertyName("suspCrdForFut")]
  [Member(Index = 162, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuspCrdForFut
  {
    get => Get<int?>("suspCrdForFut");
    set => Set("suspCrdForFut", value);
  }

  /// <summary>
  /// The value of the SUPP_DISB_LEGAL attribute.
  /// </summary>
  [JsonPropertyName("suppDisbLegal")]
  [Member(Index = 163, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SuppDisbLegal
  {
    get => Get<int?>("suppDisbLegal");
    set => Set("suppDisbLegal", value);
  }

  /// <summary>
  /// The value of the OTHER_COUNTRY_AMT attribute.
  /// Collections received during the quarter from other countries.
  /// </summary>
  [JsonPropertyName("otherCountryAmt")]
  [Member(Index = 164, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtherCountryAmt
  {
    get => Get<int?>("otherCountryAmt");
    set => Set("otherCountryAmt", value);
  }

  /// <summary>
  /// The value of the OUTSIDE_IVD_AMT attribute.
  /// Total amount of collections sent outside the Reporting State's IV-D 
  /// program
  /// </summary>
  [JsonPropertyName("outsideIvdAmt")]
  [Member(Index = 165, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OutsideIvdAmt
  {
    get => Get<int?>("outsideIvdAmt");
    set => Set("outsideIvdAmt", value);
  }

  /// <summary>
  /// The value of the FEES_RET_NEVER_AMT attribute.
  /// Amount of fees retained by the state for Medicaid never assistance cases
  /// </summary>
  [JsonPropertyName("feesRetNeverAmt")]
  [Member(Index = 166, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FeesRetNeverAmt
  {
    get => Get<int?>("feesRetNeverAmt");
    set => Set("feesRetNeverAmt", value);
  }

  /// <summary>
  /// The value of the FEES_RET_OTHER_AMT attribute.
  /// Amount of fees retained by the state for other never assistance cases
  /// </summary>
  [JsonPropertyName("feesRetOtherAmt")]
  [Member(Index = 167, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FeesRetOtherAmt
  {
    get => Get<int?>("feesRetOtherAmt");
    set => Set("feesRetOtherAmt", value);
  }

  /// <summary>
  /// The value of the FEES_RET_TOTAL_AMT attribute.
  /// Total amount of fees retained by the state
  /// </summary>
  [JsonPropertyName("feesRetTotalAmt")]
  [Member(Index = 168, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? FeesRetTotalAmt
  {
    get => Get<int?>("feesRetTotalAmt");
    set => Set("feesRetTotalAmt", value);
  }

  /// <summary>
  /// The value of the PENDED_JOINT_FDSO attribute.
  /// The amount of FDSO money from joint filers in Pended status with a reason 
  /// code of RESEARCH.  This money will be reported on Part 2 Line 4 (
  /// Collections from tax offsets being held for up to six months).
  /// </summary>
  [JsonPropertyName("pendedJointFdso")]
  [Member(Index = 169, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PendedJointFdso
  {
    get => Get<int?>("pendedJointFdso");
    set => Set("pendedJointFdso", value);
  }

  /// <summary>
  /// The value of the OTH_CNTRY_AMT_FORW attribute.
  /// Amount of collections sent to other countries
  /// </summary>
  [JsonPropertyName("othCntryAmtForw")]
  [Member(Index = 170, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OthCntryAmtForw
  {
    get => Get<int?>("othCntryAmtForw");
    set => Set("othCntryAmtForw", value);
  }

  /// <summary>
  /// The value of the PASSTHRU_4A_AMT attribute.
  /// Amount of collections passed throught to current IV-A cases
  /// </summary>
  [JsonPropertyName("passthru4AAmt")]
  [Member(Index = 171, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Passthru4AAmt
  {
    get => Get<int?>("passthru4AAmt");
    set => Set("passthru4AAmt", value);
  }

  /// <summary>
  /// The value of the PASSTHRU_4E_AMT attribute.
  /// Amount of collections passed through to current IV-E cases
  /// </summary>
  [JsonPropertyName("passthru4EAmt")]
  [Member(Index = 172, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? Passthru4EAmt
  {
    get => Get<int?>("passthru4EAmt");
    set => Set("passthru4EAmt", value);
  }

  /// <summary>
  /// The value of the PASSTHRU_FMR4A_AMT attribute.
  /// Amount of collections passed through to former IV-A cases
  /// </summary>
  [JsonPropertyName("passthruFmr4AAmt")]
  [Member(Index = 173, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PassthruFmr4AAmt
  {
    get => Get<int?>("passthruFmr4AAmt");
    set => Set("passthruFmr4AAmt", value);
  }

  /// <summary>
  /// The value of the PASSTHRU_FMR4E_AMT attribute.
  /// Amount of collections passed through to former IV-E cases
  /// </summary>
  [JsonPropertyName("passthruFmr4EAmt")]
  [Member(Index = 174, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PassthruFmr4EAmt
  {
    get => Get<int?>("passthruFmr4EAmt");
    set => Set("passthruFmr4EAmt", value);
  }

  /// <summary>
  /// The value of the PASSTHRU_TOTAL_AMT attribute.
  /// Total amount of collections passed through
  /// </summary>
  [JsonPropertyName("passthruTotalAmt")]
  [Member(Index = 175, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PassthruTotalAmt
  {
    get => Get<int?>("passthruTotalAmt");
    set => Set("passthruTotalAmt", value);
  }
}

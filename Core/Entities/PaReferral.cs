// The source file: PA_REFERRAL, ID: 371438650, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// This contains information regarding referrals from other agencies except 
/// Interstate (CSENet).
/// </summary>
[Serializable]
public partial class PaReferral: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PaReferral()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PaReferral(PaReferral that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PaReferral Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the CS_ORDER_PLACE attribute.</summary>
  public const int CsOrderPlace_MaxLength = 17;

  /// <summary>
  /// The value of the CS_ORDER_PLACE attribute.
  /// County or city win which the child support order was established.
  /// </summary>
  [JsonPropertyName("csOrderPlace")]
  [Member(Index = 1, Type = MemberType.Char, Length = CsOrderPlace_MaxLength, Optional
    = true)]
  public string CsOrderPlace
  {
    get => Get<string>("csOrderPlace");
    set => Set(
      "csOrderPlace", TrimEnd(Substring(value, 1, CsOrderPlace_MaxLength)));
  }

  /// <summary>Length of the CS_ORDER_STATE attribute.</summary>
  public const int CsOrderState_MaxLength = 2;

  /// <summary>
  /// The value of the CS_ORDER_STATE attribute.
  /// Valid state-code for state in which the child support order was 
  /// established.
  /// </summary>
  [JsonPropertyName("csOrderState")]
  [Member(Index = 2, Type = MemberType.Char, Length = CsOrderState_MaxLength, Optional
    = true)]
  public string CsOrderState
  {
    get => Get<string>("csOrderState");
    set => Set(
      "csOrderState", TrimEnd(Substring(value, 1, CsOrderState_MaxLength)));
  }

  /// <summary>Length of the CS_FREQ attribute.</summary>
  public const int CsFreq_MaxLength = 1;

  /// <summary>
  /// The value of the CS_FREQ attribute.
  /// The frequency with wich child support paymeht are to be made.
  /// M - Monthly
  /// W - Weekly
  /// B - Biweekly
  /// S - Semi Monthly
  /// </summary>
  [JsonPropertyName("csFreq")]
  [Member(Index = 3, Type = MemberType.Char, Length = CsFreq_MaxLength, Optional
    = true)]
  public string CsFreq
  {
    get => Get<string>("csFreq");
    set => Set("csFreq", TrimEnd(Substring(value, 1, CsFreq_MaxLength)));
  }

  /// <summary>Length of the FROM attribute.</summary>
  public const int From_MaxLength = 3;

  /// <summary>
  /// The value of the FROM attribute.
  /// The origin of the referral. As of 7/96 may be: KAE; KSC.
  /// </summary>
  [JsonPropertyName("from")]
  [Member(Index = 4, Type = MemberType.Char, Length = From_MaxLength, Optional
    = true)]
  public string From
  {
    get => Get<string>("from");
    set => Set("from", TrimEnd(Substring(value, 1, From_MaxLength)));
  }

  /// <summary>
  /// The value of the AP_PHONE_NUMBER attribute.
  /// The telephone number of the ap
  /// </summary>
  [JsonPropertyName("apPhoneNumber")]
  [Member(Index = 5, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ApPhoneNumber
  {
    get => Get<int?>("apPhoneNumber");
    set => Set("apPhoneNumber", value);
  }

  /// <summary>
  /// The value of the AP_AREA_CODE attribute.
  /// The area code of the AP's phone number.
  /// </summary>
  [JsonPropertyName("apAreaCode")]
  [Member(Index = 6, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ApAreaCode
  {
    get => Get<int?>("apAreaCode");
    set => Set("apAreaCode", value);
  }

  /// <summary>
  /// The value of the CC_START_DATE attribute.
  /// The date a child care plan is created and the referral is sent to CSE from
  /// KSCares.
  /// </summary>
  [JsonPropertyName("ccStartDate")]
  [Member(Index = 7, Type = MemberType.Date, Optional = true)]
  public DateTime? CcStartDate
  {
    get => Get<DateTime?>("ccStartDate");
    set => Set("ccStartDate", value);
  }

  /// <summary>Length of the AR_EMPLOYER_NAME attribute.</summary>
  public const int ArEmployerName_MaxLength = 28;

  /// <summary>
  /// The value of the AR_EMPLOYER_NAME attribute.
  /// The name of the AR;s employer
  /// </summary>
  [JsonPropertyName("arEmployerName")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = ArEmployerName_MaxLength, Optional = true)]
  public string ArEmployerName
  {
    get => Get<string>("arEmployerName");
    set => Set(
      "arEmployerName", TrimEnd(Substring(value, 1, ArEmployerName_MaxLength)));
      
  }

  /// <summary>Length of the SUPPORT ORDER FREQ attribute.</summary>
  public const int SupportOrderFreq_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT ORDER FREQ attribute.
  /// Frequency of medical support payments:
  /// W - Weekly
  /// B - Biweekly
  /// S - Semimonthly
  /// M - Monthly
  /// </summary>
  [JsonPropertyName("supportOrderFreq")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = SupportOrderFreq_MaxLength, Optional = true)]
  public string SupportOrderFreq
  {
    get => Get<string>("supportOrderFreq");
    set => Set(
      "supportOrderFreq",
      TrimEnd(Substring(value, 1, SupportOrderFreq_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 10, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => Get<string>("createdBy");
    set => Set("createdBy", TrimEnd(Substring(value, 1, CreatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Miscellaneous information (free-form) entered by the Economic Assistance 
  /// worker.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 14, Type = MemberType.Char, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => Get<string>("note");
    set => Set("note", TrimEnd(Substring(value, 1, Note_MaxLength)));
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// This is the date the PA Referral record was created by the interface 
  /// program.  It is used as a control to ensure timely working of Referrals.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? ReceivedDate
  {
    get => Get<DateTime?>("receivedDate");
    set => Set("receivedDate", value);
  }

  /// <summary>Length of the ASSIGN_DEACTIVATE_IND attribute.</summary>
  public const int AssignDeactivateInd_MaxLength = 1;

  /// <summary>
  /// The value of the ASSIGN_DEACTIVATE_IND attribute.
  /// Indicates whether this Referral was Assigned or Deactivated.
  /// A - Assigned  (New Case)
  /// D - Deactivated  (Possible new Case, might
  ///                   also be update to existing
  ///                   Case)
  /// ' ' Not worked
  /// </summary>
  [JsonPropertyName("assignDeactivateInd")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = AssignDeactivateInd_MaxLength, Optional = true)]
  public string AssignDeactivateInd
  {
    get => Get<string>("assignDeactivateInd");
    set => Set(
      "assignDeactivateInd", TrimEnd(Substring(value, 1,
      AssignDeactivateInd_MaxLength)));
  }

  /// <summary>
  /// The value of the ASSIGN_DEACTIVATE_DATE attribute.
  /// Date CSE worker decides to complete (Assign or Deactivate) this Referral.
  /// </summary>
  [JsonPropertyName("assignDeactivateDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignDeactivateDate
  {
    get => Get<DateTime?>("assignDeactivateDate");
    set => Set("assignDeactivateDate", value);
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The number assigned to the PA case.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 18, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => Get<string>("caseNumber");
    set =>
      Set("caseNumber", TrimEnd(Substring(value, 1, CaseNumber_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Referral Number (Unique identifier)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => Get<string>("number") ?? "";
    set => Set(
      "number", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Number_MaxLength)));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 6;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This will indicate whether this Referral is:
  /// 'New'
  /// 'Reopen'
  /// 'Change'
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => Get<string>("type1") ?? "";
    set => Set(
      "type1", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, Type1_MaxLength)));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>
  /// The value of the MEDICAL_PAYMENT_DUE_DATE attribute.
  /// Date payment is due for medical support payment.
  /// </summary>
  [JsonPropertyName("medicalPaymentDueDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalPaymentDueDate
  {
    get => Get<DateTime?>("medicalPaymentDueDate");
    set => Set("medicalPaymentDueDate", value);
  }

  /// <summary>
  /// The value of the MEDICAL_AMT attribute.
  /// The amount of the Medical Support Order.
  /// </summary>
  [JsonPropertyName("medicalAmt")]
  [Member(Index = 22, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalAmt
  {
    get => Get<decimal?>("medicalAmt");
    set => Set("medicalAmt", Truncate(value, 2));
  }

  /// <summary>Length of the MEDICAL_FREQ attribute.</summary>
  public const int MedicalFreq_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_FREQ attribute.
  /// The frequency of Medical Support Order payments:
  /// W - Weekly
  /// B - Biweekly
  /// S - Semimonthly
  /// M - Monthly
  /// </summary>
  [JsonPropertyName("medicalFreq")]
  [Member(Index = 23, Type = MemberType.Char, Length = MedicalFreq_MaxLength, Optional
    = true)]
  public string MedicalFreq
  {
    get => Get<string>("medicalFreq");
    set => Set(
      "medicalFreq", TrimEnd(Substring(value, 1, MedicalFreq_MaxLength)));
  }

  /// <summary>
  /// The value of the MEDICAL_LAST_PAYMENT attribute.
  /// The amount of the last Medical Support Order payment.
  /// </summary>
  [JsonPropertyName("medicalLastPayment")]
  [Member(Index = 24, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalLastPayment
  {
    get => Get<decimal?>("medicalLastPayment");
    set => Set("medicalLastPayment", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the MEDICAL_LAST_PAYMENT_DATE attribute.
  /// The date of the last Medical Support Order payment.
  /// </summary>
  [JsonPropertyName("medicalLastPaymentDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalLastPaymentDate
  {
    get => Get<DateTime?>("medicalLastPaymentDate");
    set => Set("medicalLastPaymentDate", value);
  }

  /// <summary>
  /// The value of the MEDICAL_ORDER_EFFECTIVE_DATE attribute.
  /// The date on which the Medical Support Order was effective.
  /// </summary>
  [JsonPropertyName("medicalOrderEffectiveDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalOrderEffectiveDate
  {
    get => Get<DateTime?>("medicalOrderEffectiveDate");
    set => Set("medicalOrderEffectiveDate", value);
  }

  /// <summary>Length of the MEDICAL_ORDER_STATE attribute.</summary>
  public const int MedicalOrderState_MaxLength = 2;

  /// <summary>
  /// The value of the MEDICAL_ORDER_STATE attribute.
  /// The state where the Medical Support Order was issued.
  /// </summary>
  [JsonPropertyName("medicalOrderState")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = MedicalOrderState_MaxLength, Optional = true)]
  public string MedicalOrderState
  {
    get => Get<string>("medicalOrderState");
    set => Set(
      "medicalOrderState", TrimEnd(Substring(value, 1,
      MedicalOrderState_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_ORDER_PLACE attribute.</summary>
  public const int MedicalOrderPlace_MaxLength = 17;

  /// <summary>
  /// The value of the MEDICAL_ORDER_PLACE attribute.
  /// The city or county where the Medical Support Order was issued.
  /// </summary>
  [JsonPropertyName("medicalOrderPlace")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = MedicalOrderPlace_MaxLength, Optional = true)]
  public string MedicalOrderPlace
  {
    get => Get<string>("medicalOrderPlace");
    set => Set(
      "medicalOrderPlace", TrimEnd(Substring(value, 1,
      MedicalOrderPlace_MaxLength)));
  }

  /// <summary>
  /// The value of the MEDICAL_ARREARAGE attribute.
  /// The total arrearage of Medical Support Order payments.
  /// </summary>
  [JsonPropertyName("medicalArrearage")]
  [Member(Index = 29, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalArrearage
  {
    get => Get<decimal?>("medicalArrearage");
    set => Set("medicalArrearage", Truncate(value, 2));
  }

  /// <summary>Length of the MEDICAL_PAID_TO attribute.</summary>
  public const int MedicalPaidTo_MaxLength = 2;

  /// <summary>
  /// The value of the MEDICAL_PAID_TO attribute.
  /// Destination for Medical Support Order payments:
  /// AR - Applicant Recipient
  /// CT - Court
  /// OT - Other
  /// </summary>
  [JsonPropertyName("medicalPaidTo")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = MedicalPaidTo_MaxLength, Optional = true)]
  public string MedicalPaidTo
  {
    get => Get<string>("medicalPaidTo");
    set => Set(
      "medicalPaidTo", TrimEnd(Substring(value, 1, MedicalPaidTo_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_PAYMENT_TYPE attribute.</summary>
  public const int MedicalPaymentType_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_PAYMENT_TYPE attribute.
  /// The type of Medical Support Order payments:
  /// I - Insurance
  /// M - Money
  /// B - Both
  /// </summary>
  [JsonPropertyName("medicalPaymentType")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = MedicalPaymentType_MaxLength, Optional = true)]
  public string MedicalPaymentType
  {
    get => Get<string>("medicalPaymentType");
    set => Set(
      "medicalPaymentType", TrimEnd(Substring(value, 1,
      MedicalPaymentType_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_INSURANCE_CO attribute.</summary>
  public const int MedicalInsuranceCo_MaxLength = 25;

  /// <summary>
  /// The value of the MEDICAL_INSURANCE_CO attribute.
  /// The name of the company providing medical insurance coverage.
  /// </summary>
  [JsonPropertyName("medicalInsuranceCo")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = MedicalInsuranceCo_MaxLength, Optional = true)]
  public string MedicalInsuranceCo
  {
    get => Get<string>("medicalInsuranceCo");
    set => Set(
      "medicalInsuranceCo", TrimEnd(Substring(value, 1,
      MedicalInsuranceCo_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_POLICY_NUMBER attribute.</summary>
  public const int MedicalPolicyNumber_MaxLength = 20;

  /// <summary>
  /// The value of the MEDICAL_POLICY_NUMBER attribute.
  /// The policy number for medical insurance coverage.
  /// </summary>
  [JsonPropertyName("medicalPolicyNumber")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = MedicalPolicyNumber_MaxLength, Optional = true)]
  public string MedicalPolicyNumber
  {
    get => Get<string>("medicalPolicyNumber");
    set => Set(
      "medicalPolicyNumber", TrimEnd(Substring(value, 1,
      MedicalPolicyNumber_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_ORDER_NUMBER attribute.</summary>
  public const int MedicalOrderNumber_MaxLength = 17;

  /// <summary>
  /// The value of the MEDICAL_ORDER_NUMBER attribute.
  /// The identifier of the Medical Support Order
  /// </summary>
  [JsonPropertyName("medicalOrderNumber")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = MedicalOrderNumber_MaxLength, Optional = true)]
  public string MedicalOrderNumber
  {
    get => Get<string>("medicalOrderNumber");
    set => Set(
      "medicalOrderNumber", TrimEnd(Substring(value, 1,
      MedicalOrderNumber_MaxLength)));
  }

  /// <summary>Length of the MEDICAL_ORDER_IND attribute.</summary>
  public const int MedicalOrderInd_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_ORDER_IND attribute.
  /// Indicates that Medical Support was/was not ordered in the same order as 
  /// the child support. Can be blank.
  /// Y - Medical Support included.
  /// N - Medical Support not included.
  /// </summary>
  [JsonPropertyName("medicalOrderInd")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = MedicalOrderInd_MaxLength, Optional = true)]
  public string MedicalOrderInd
  {
    get => Get<string>("medicalOrderInd");
    set => Set(
      "medicalOrderInd",
      TrimEnd(Substring(value, 1, MedicalOrderInd_MaxLength)));
  }

  /// <summary>
  /// The value of the ASSIGNMENT_DATE attribute.
  /// Populated by AE's EFF-DATE-OF-ASSIGNMENT from their REFERRAL-DBF.  Their 
  /// definition reads:  The date on which child support payments are assigned
  /// to CSE.
  /// </summary>
  [JsonPropertyName("assignmentDate")]
  [Member(Index = 36, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignmentDate
  {
    get => Get<DateTime?>("assignmentDate");
    set => Set("assignmentDate", value);
  }

  /// <summary>Length of the CSE_REGION attribute.</summary>
  public const int CseRegion_MaxLength = 2;

  /// <summary>
  /// The value of the CSE_REGION attribute.
  /// CSE region handling Case
  /// </summary>
  [JsonPropertyName("cseRegion")]
  [Member(Index = 37, Type = MemberType.Char, Length = CseRegion_MaxLength, Optional
    = true)]
  public string CseRegion
  {
    get => Get<string>("cseRegion");
    set => Set("cseRegion", TrimEnd(Substring(value, 1, CseRegion_MaxLength)));
  }

  /// <summary>
  /// The value of the CSE_REFERRAL_REC_DATE attribute.
  /// The date the referral was received by CSE.
  /// </summary>
  [JsonPropertyName("cseReferralRecDate")]
  [Member(Index = 38, Type = MemberType.Date, Optional = true)]
  public DateTime? CseReferralRecDate
  {
    get => Get<DateTime?>("cseReferralRecDate");
    set => Set("cseReferralRecDate", value);
  }

  /// <summary>Length of the AR_RETAINED_IND attribute.</summary>
  public const int ArRetainedInd_MaxLength = 1;

  /// <summary>
  /// The value of the AR_RETAINED_IND attribute.
  /// This indicates whether AR kept payments.
  /// Y - AR received and kept payments	
  /// N - AR received payments and turned in to CSE
  /// </summary>
  [JsonPropertyName("arRetainedInd")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = ArRetainedInd_MaxLength, Optional = true)]
  public string ArRetainedInd
  {
    get => Get<string>("arRetainedInd");
    set => Set(
      "arRetainedInd", TrimEnd(Substring(value, 1, ArRetainedInd_MaxLength)));
  }

  /// <summary>Length of the PGM_CODE attribute.</summary>
  public const int PgmCode_MaxLength = 2;

  /// <summary>
  /// The value of the PGM_CODE attribute.
  /// The program of the participants on this referral.
  /// FS - Food Stamps
  /// CC - Child care
  /// FC - Foster Care
  /// MA - Medical Assistance
  /// AF - ADC
  /// MS - Medical Assistance for elderly or disabled
  /// CI - MA - Child in institution PA related
  /// SI - MA Child receiving SSI
  /// MP MA or poverty level children
  /// </summary>
  [JsonPropertyName("pgmCode")]
  [Member(Index = 40, Type = MemberType.Char, Length = PgmCode_MaxLength, Optional
    = true)]
  public string PgmCode
  {
    get => Get<string>("pgmCode");
    set => Set("pgmCode", TrimEnd(Substring(value, 1, PgmCode_MaxLength)));
  }

  /// <summary>Length of the CASE_WORKER attribute.</summary>
  public const int CaseWorker_MaxLength = 8;

  /// <summary>
  /// The value of the CASE_WORKER attribute.
  /// Partial name of the AE EA, or DSCares worker assigned to the PA case.
  /// </summary>
  [JsonPropertyName("caseWorker")]
  [Member(Index = 41, Type = MemberType.Char, Length = CaseWorker_MaxLength, Optional
    = true)]
  public string CaseWorker
  {
    get => Get<string>("caseWorker");
    set =>
      Set("caseWorker", TrimEnd(Substring(value, 1, CaseWorker_MaxLength)));
  }

  /// <summary>Length of the PAYMENT_MADE_TO attribute.</summary>
  public const int PaymentMadeTo_MaxLength = 2;

  /// <summary>
  /// The value of the PAYMENT_MADE_TO attribute.
  /// This is the code for the current destination of child support payments:	
  /// AR - Applicant Recipient
  /// CT - Court
  /// OT - Other
  /// </summary>
  [JsonPropertyName("paymentMadeTo")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = PaymentMadeTo_MaxLength, Optional = true)]
  public string PaymentMadeTo
  {
    get => Get<string>("paymentMadeTo");
    set => Set(
      "paymentMadeTo", TrimEnd(Substring(value, 1, PaymentMadeTo_MaxLength)));
  }

  /// <summary>
  /// The value of the CS_ARREARAGE_AMT attribute.
  /// This is the amount of child support arrears.
  /// </summary>
  [JsonPropertyName("csArrearageAmt")]
  [Member(Index = 43, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsArrearageAmt
  {
    get => Get<decimal?>("csArrearageAmt");
    set => Set("csArrearageAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the CS_LAST_PAYMENT_AMT attribute.
  /// The amount of the last child support payment.
  /// </summary>
  [JsonPropertyName("csLastPaymentAmt")]
  [Member(Index = 44, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsLastPaymentAmt
  {
    get => Get<decimal?>("csLastPaymentAmt");
    set => Set("csLastPaymentAmt", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the LAST_PAYMENT_DATE attribute.
  /// This is the last date on which a child support payment was received from 
  /// the AP.
  /// </summary>
  [JsonPropertyName("lastPaymentDate")]
  [Member(Index = 45, Type = MemberType.Date, Optional = true)]
  public DateTime? LastPaymentDate
  {
    get => Get<DateTime?>("lastPaymentDate");
    set => Set("lastPaymentDate", value);
  }

  /// <summary>Length of the GOOD_CAUSE_CODE attribute.</summary>
  public const int GoodCauseCode_MaxLength = 2;

  /// <summary>
  /// The value of the GOOD_CAUSE_CODE attribute.
  /// This indicate whether the AR has claimed 'Good Cause' for not pursuing the
  /// named AP for child support.
  /// Codes are:
  /// 'GC' - Good Cause Determined;
  /// 'PD' - Good Cause Pending
  /// 'CO' - AR is now cooperating.
  /// </summary>
  [JsonPropertyName("goodCauseCode")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = GoodCauseCode_MaxLength, Optional = true)]
  public string GoodCauseCode
  {
    get => Get<string>("goodCauseCode");
    set => Set(
      "goodCauseCode", TrimEnd(Substring(value, 1, GoodCauseCode_MaxLength)));
  }

  /// <summary>
  /// The value of the GOOD_CAUSE_DATE attribute.
  /// Date 'Good Cause' established
  /// </summary>
  [JsonPropertyName("goodCauseDate")]
  [Member(Index = 47, Type = MemberType.Date, Optional = true)]
  public DateTime? GoodCauseDate
  {
    get => Get<DateTime?>("goodCauseDate");
    set => Set("goodCauseDate", value);
  }

  /// <summary>
  /// The value of the CS_PAYMENT_AMOUNT attribute.
  /// This is the ordered amount of each child support payment.
  /// </summary>
  [JsonPropertyName("csPaymentAmount")]
  [Member(Index = 48, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsPaymentAmount
  {
    get => Get<decimal?>("csPaymentAmount");
    set => Set("csPaymentAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the ORDER_EFFECTIVE_DATE attribute.
  /// The is the date on which the Child Support Order was effective.
  /// </summary>
  [JsonPropertyName("orderEffectiveDate")]
  [Member(Index = 49, Type = MemberType.Date, Optional = true)]
  public DateTime? OrderEffectiveDate
  {
    get => Get<DateTime?>("orderEffectiveDate");
    set => Set("orderEffectiveDate", value);
  }

  /// <summary>
  /// The value of the PAYMENT_DUE_DATE attribute.
  /// This is the first date on which child support payment was due.
  /// </summary>
  [JsonPropertyName("paymentDueDate")]
  [Member(Index = 50, Type = MemberType.Date, Optional = true)]
  public DateTime? PaymentDueDate
  {
    get => Get<DateTime?>("paymentDueDate");
    set => Set("paymentDueDate", value);
  }

  /// <summary>Length of the SUPPORT_ORDER_ID attribute.</summary>
  public const int SupportOrderId_MaxLength = 17;

  /// <summary>
  /// The value of the SUPPORT_ORDER_ID attribute.
  /// This is the Child Support Court Order number that establishes support.
  /// </summary>
  [JsonPropertyName("supportOrderId")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = SupportOrderId_MaxLength, Optional = true)]
  public string SupportOrderId
  {
    get => Get<string>("supportOrderId");
    set => Set(
      "supportOrderId", TrimEnd(Substring(value, 1, SupportOrderId_MaxLength)));
      
  }

  /// <summary>
  /// The value of the LAST_AP_CONTACT_DATE attribute.
  /// Last date on which AP contacted child
  /// </summary>
  [JsonPropertyName("lastApContactDate")]
  [Member(Index = 52, Type = MemberType.Date, Optional = true)]
  public DateTime? LastApContactDate
  {
    get => Get<DateTime?>("lastApContactDate");
    set => Set("lastApContactDate", value);
  }

  /// <summary>Length of the VOLUNTARY_SUPPORT_IND attribute.</summary>
  public const int VoluntarySupportInd_MaxLength = 1;

  /// <summary>
  /// The value of the VOLUNTARY_SUPPORT_IND attribute.
  /// This indicates voluntary support:
  /// 'Y' - Yes
  /// 'N' - No
  /// </summary>
  [JsonPropertyName("voluntarySupportInd")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = VoluntarySupportInd_MaxLength, Optional = true)]
  public string VoluntarySupportInd
  {
    get => Get<string>("voluntarySupportInd");
    set => Set(
      "voluntarySupportInd", TrimEnd(Substring(value, 1,
      VoluntarySupportInd_MaxLength)));
  }

  /// <summary>
  /// The value of the AP_EMPLOYER_PHONE attribute.
  /// This is the phone number of the AP Employer.
  /// </summary>
  [JsonPropertyName("apEmployerPhone")]
  [Member(Index = 54, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? ApEmployerPhone
  {
    get => Get<long?>("apEmployerPhone");
    set => Set("apEmployerPhone", value);
  }

  /// <summary>Length of the AP_EMPLOYER_NAME attribute.</summary>
  public const int ApEmployerName_MaxLength = 28;

  /// <summary>
  /// The value of the AP_EMPLOYER_NAME attribute.
  /// This is the name of the AP Employer
  /// </summary>
  [JsonPropertyName("apEmployerName")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = ApEmployerName_MaxLength, Optional = true)]
  public string ApEmployerName
  {
    get => Get<string>("apEmployerName");
    set => Set(
      "apEmployerName", TrimEnd(Substring(value, 1, ApEmployerName_MaxLength)));
      
  }

  /// <summary>
  /// The value of the FC_NEXT_JUVENILE_CT_DT attribute.
  /// This is the date of the next session of the Juvenile Court which manages 
  /// this Case.
  /// </summary>
  [JsonPropertyName("fcNextJuvenileCtDt")]
  [Member(Index = 56, Type = MemberType.Date, Optional = true)]
  public DateTime? FcNextJuvenileCtDt
  {
    get => Get<DateTime?>("fcNextJuvenileCtDt");
    set => Set("fcNextJuvenileCtDt", value);
  }

  /// <summary>Length of the FC_ORDER_EST_BY attribute.</summary>
  public const int FcOrderEstBy_MaxLength = 2;

  /// <summary>
  /// The value of the FC_ORDER_EST_BY attribute.
  /// Indicates who established the foster care order
  /// PA - public attorney
  /// SA - SRS attorney
  /// CA - county attorney
  /// OT - other
  /// </summary>
  [JsonPropertyName("fcOrderEstBy")]
  [Member(Index = 57, Type = MemberType.Char, Length = FcOrderEstBy_MaxLength, Optional
    = true)]
  public string FcOrderEstBy
  {
    get => Get<string>("fcOrderEstBy");
    set => Set(
      "fcOrderEstBy", TrimEnd(Substring(value, 1, FcOrderEstBy_MaxLength)));
  }

  /// <summary>Length of the FC_JUVENILE_COURT_ORDER attribute.</summary>
  public const int FcJuvenileCourtOrder_MaxLength = 12;

  /// <summary>
  /// The value of the FC_JUVENILE_COURT_ORDER attribute.
  /// Juvenile court order number
  /// </summary>
  [JsonPropertyName("fcJuvenileCourtOrder")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = FcJuvenileCourtOrder_MaxLength, Optional = true)]
  public string FcJuvenileCourtOrder
  {
    get => Get<string>("fcJuvenileCourtOrder");
    set => Set(
      "fcJuvenileCourtOrder", TrimEnd(Substring(value, 1,
      FcJuvenileCourtOrder_MaxLength)));
  }

  /// <summary>Length of the FC_JUVENILE_OFFENDER_IND attribute.</summary>
  public const int FcJuvenileOffenderInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_JUVENILE_OFFENDER_IND attribute.
  /// Indicates child was a Juvenile Offender when placed in Foster Care.
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcJuvenileOffenderInd")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = FcJuvenileOffenderInd_MaxLength, Optional = true)]
  public string FcJuvenileOffenderInd
  {
    get => Get<string>("fcJuvenileOffenderInd");
    set => Set(
      "fcJuvenileOffenderInd", TrimEnd(Substring(value, 1,
      FcJuvenileOffenderInd_MaxLength)));
  }

  /// <summary>Length of the FC_CINC_IND attribute.</summary>
  public const int FcCincInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_CINC_IND attribute.
  /// Indicates Child in need of care
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcCincInd")]
  [Member(Index = 60, Type = MemberType.Char, Length = FcCincInd_MaxLength, Optional
    = true)]
  public string FcCincInd
  {
    get => Get<string>("fcCincInd");
    set => Set("fcCincInd", TrimEnd(Substring(value, 1, FcCincInd_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_PLACEMENT_DATE attribute.
  /// Date child placed in FC
  /// </summary>
  [JsonPropertyName("fcPlacementDate")]
  [Member(Index = 61, Type = MemberType.Date, Optional = true)]
  public DateTime? FcPlacementDate
  {
    get => Get<DateTime?>("fcPlacementDate");
    set => Set("fcPlacementDate", value);
  }

  /// <summary>Length of the FC_SRS_PAYEE attribute.</summary>
  public const int FcSrsPayee_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SRS_PAYEE attribute.
  /// Indicates whether or not SRS has been determined to be the payee.
  /// </summary>
  [JsonPropertyName("fcSrsPayee")]
  [Member(Index = 62, Type = MemberType.Char, Length = FcSrsPayee_MaxLength, Optional
    = true)]
  public string FcSrsPayee
  {
    get => Get<string>("fcSrsPayee");
    set =>
      Set("fcSrsPayee", TrimEnd(Substring(value, 1, FcSrsPayee_MaxLength)));
  }

  /// <summary>Length of the FC_COST_OF_CARE_FREQ attribute.</summary>
  public const int FcCostOfCareFreq_MaxLength = 1;

  /// <summary>
  /// The value of the FC_COST_OF_CARE_FREQ attribute.
  /// The frequency of the cost of the child's care
  /// W - Weekly
  /// B - Biweekly
  /// S - Semimonthly
  /// M - Monthly
  /// D - Daily
  /// </summary>
  [JsonPropertyName("fcCostOfCareFreq")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = FcCostOfCareFreq_MaxLength, Optional = true)]
  public string FcCostOfCareFreq
  {
    get => Get<string>("fcCostOfCareFreq");
    set => Set(
      "fcCostOfCareFreq",
      TrimEnd(Substring(value, 1, FcCostOfCareFreq_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_COST_OF_CARE attribute.
  /// The Amount paid for the care of the child.
  /// </summary>
  [JsonPropertyName("fcCostOfCare")]
  [Member(Index = 64, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? FcCostOfCare
  {
    get => Get<decimal?>("fcCostOfCare");
    set => Set("fcCostOfCare", Truncate(value, 2));
  }

  /// <summary>Length of the FC_ADOPTION_DISRUPTION_IND attribute.</summary>
  public const int FcAdoptionDisruptionInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_ADOPTION_DISRUPTION_IND attribute.
  /// Indicates adoption disruption:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcAdoptionDisruptionInd")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = FcAdoptionDisruptionInd_MaxLength, Optional = true)]
  public string FcAdoptionDisruptionInd
  {
    get => Get<string>("fcAdoptionDisruptionInd");
    set => Set(
      "fcAdoptionDisruptionInd", TrimEnd(Substring(value, 1,
      FcAdoptionDisruptionInd_MaxLength)));
  }

  /// <summary>Length of the FC_PLACEMENT_TYPE attribute.</summary>
  public const int FcPlacementType_MaxLength = 2;

  /// <summary>
  /// The value of the FC_PLACEMENT_TYPE attribute.
  /// Type of foster care placement (facility or level of care).
  /// 01 - Family foster home
  /// 02 - Parents
  /// 03 - Relatives
  /// 04 - Level III
  /// 05 - Level IV
  /// 06 - Level V
  /// 07 - Level VI
  /// 08 - Level not assigned
  /// 13 - SI - LSH (Larned)
  /// 14 - SI - OSH (Ossowatatomie)
  /// 15 - SI - TSH (Topeka)
  /// 16 - Si - WSH (Winfield)
  /// 17 - SI - PSH (Parsons)
  /// 18 - SI - R (Rainbow)
  /// 19 - Independent living
  /// 20 - State School for Blind/Deaf
  /// 21 - Adoption non-subsidy
  /// 22 - Adoption subsidy
  /// 23 - Runaway
  /// 24 - Detention
  /// 25 - Other
  /// </summary>
  [JsonPropertyName("fcPlacementType")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = FcPlacementType_MaxLength, Optional = true)]
  public string FcPlacementType
  {
    get => Get<string>("fcPlacementType");
    set => Set(
      "fcPlacementType",
      TrimEnd(Substring(value, 1, FcPlacementType_MaxLength)));
  }

  /// <summary>Length of the FC_PREVIOUS_PA attribute.</summary>
  public const int FcPreviousPa_MaxLength = 1;

  /// <summary>
  /// The value of the FC_PREVIOUS_PA attribute.
  /// Indicates previous Public Assistance
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcPreviousPa")]
  [Member(Index = 67, Type = MemberType.Char, Length = FcPreviousPa_MaxLength, Optional
    = true)]
  public string FcPreviousPa
  {
    get => Get<string>("fcPreviousPa");
    set => Set(
      "fcPreviousPa", TrimEnd(Substring(value, 1, FcPreviousPa_MaxLength)));
  }

  /// <summary>
  /// The value of the FC_DATE_OF_INITIAL_CUSTODY attribute.
  /// Initial date FC began
  /// </summary>
  [JsonPropertyName("fcDateOfInitialCustody")]
  [Member(Index = 68, Type = MemberType.Date, Optional = true)]
  public DateTime? FcDateOfInitialCustody
  {
    get => Get<DateTime?>("fcDateOfInitialCustody");
    set => Set("fcDateOfInitialCustody", value);
  }

  /// <summary>Length of the FC_RIGHTS_SEVERED attribute.</summary>
  public const int FcRightsSevered_MaxLength = 1;

  /// <summary>
  /// The value of the FC_RIGHTS_SEVERED attribute.
  /// Indicates whether or not parental rights are severed:
  /// Y - Yes
  /// N - No
  /// P - Pending
  /// </summary>
  [JsonPropertyName("fcRightsSevered")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = FcRightsSevered_MaxLength, Optional = true)]
  public string FcRightsSevered
  {
    get => Get<string>("fcRightsSevered");
    set => Set(
      "fcRightsSevered",
      TrimEnd(Substring(value, 1, FcRightsSevered_MaxLength)));
  }

  /// <summary>Length of the FC_IV_E_CASE_NUMBER attribute.</summary>
  public const int FcIvECaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the FC_IV_E_CASE_NUMBER attribute.
  /// Foster Care IV-E Case #
  /// </summary>
  [JsonPropertyName("fcIvECaseNumber")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = FcIvECaseNumber_MaxLength, Optional = true)]
  public string FcIvECaseNumber
  {
    get => Get<string>("fcIvECaseNumber");
    set => Set(
      "fcIvECaseNumber",
      TrimEnd(Substring(value, 1, FcIvECaseNumber_MaxLength)));
  }

  /// <summary>Length of the FC_PLACEMENT_NAME attribute.</summary>
  public const int FcPlacementName_MaxLength = 25;

  /// <summary>
  /// The value of the FC_PLACEMENT_NAME attribute.
  /// Name of institution where child placed
  /// </summary>
  [JsonPropertyName("fcPlacementName")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = FcPlacementName_MaxLength, Optional = true)]
  public string FcPlacementName
  {
    get => Get<string>("fcPlacementName");
    set => Set(
      "fcPlacementName",
      TrimEnd(Substring(value, 1, FcPlacementName_MaxLength)));
  }

  /// <summary>Length of the FC_SOURCE_OF_FUNDING attribute.</summary>
  public const int FcSourceOfFunding_MaxLength = 2;

  /// <summary>
  /// The value of the FC_SOURCE_OF_FUNDING attribute.
  /// Whether the funds are state or federal
  /// AF - Federal funding
  /// GA - State funding
  /// </summary>
  [JsonPropertyName("fcSourceOfFunding")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = FcSourceOfFunding_MaxLength, Optional = true)]
  public string FcSourceOfFunding
  {
    get => Get<string>("fcSourceOfFunding");
    set => Set(
      "fcSourceOfFunding", TrimEnd(Substring(value, 1,
      FcSourceOfFunding_MaxLength)));
  }

  /// <summary>Length of the FC_OTHER_BENEFIT_IND attribute.</summary>
  public const int FcOtherBenefitInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_OTHER_BENEFIT_IND attribute.
  /// Denotes whether or not the child receives any other benefits
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcOtherBenefitInd")]
  [Member(Index = 73, Type = MemberType.Char, Length
    = FcOtherBenefitInd_MaxLength, Optional = true)]
  public string FcOtherBenefitInd
  {
    get => Get<string>("fcOtherBenefitInd");
    set => Set(
      "fcOtherBenefitInd", TrimEnd(Substring(value, 1,
      FcOtherBenefitInd_MaxLength)));
  }

  /// <summary>Length of the FC_ZEB_IND attribute.</summary>
  public const int FcZebInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_ZEB_IND attribute.
  /// A lawsuit filed against social security on behalf of children denied SSI 
  /// benefits as their disabilities were determined using adult standards, not
  /// child standards. There was a settlement and SSI payments to children who
  /// were denied SSI based on adult standards and then approved using child
  /// standards are placed in a &quot;zeb&quot; by account.
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcZebInd")]
  [Member(Index = 74, Type = MemberType.Char, Length = FcZebInd_MaxLength, Optional
    = true)]
  public string FcZebInd
  {
    get => Get<string>("fcZebInd");
    set => Set("fcZebInd", TrimEnd(Substring(value, 1, FcZebInd_MaxLength)));
  }

  /// <summary>Length of the FC_VA_IND attribute.</summary>
  public const int FcVaInd_MaxLength = 1;

  /// <summary>
  /// The value of the FC_VA_IND attribute.
  /// Vetrans Administration Benefits indicator:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcVaInd")]
  [Member(Index = 75, Type = MemberType.Char, Length = FcVaInd_MaxLength, Optional
    = true)]
  public string FcVaInd
  {
    get => Get<string>("fcVaInd");
    set => Set("fcVaInd", TrimEnd(Substring(value, 1, FcVaInd_MaxLength)));
  }

  /// <summary>Length of the FC_SSI attribute.</summary>
  public const int FcSsi_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SSI attribute.
  /// Social Security for disability benefits Indicator:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcSsi")]
  [Member(Index = 76, Type = MemberType.Char, Length = FcSsi_MaxLength, Optional
    = true)]
  public string FcSsi
  {
    get => Get<string>("fcSsi");
    set => Set("fcSsi", TrimEnd(Substring(value, 1, FcSsi_MaxLength)));
  }

  /// <summary>Length of the FC_SSA attribute.</summary>
  public const int FcSsa_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SSA attribute.
  /// Any Social Security death benefits
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcSsa")]
  [Member(Index = 77, Type = MemberType.Char, Length = FcSsa_MaxLength, Optional
    = true)]
  public string FcSsa
  {
    get => Get<string>("fcSsa");
    set => Set("fcSsa", TrimEnd(Substring(value, 1, FcSsa_MaxLength)));
  }

  /// <summary>Length of the FC_WARDS_ACCOUNT attribute.</summary>
  public const int FcWardsAccount_MaxLength = 1;

  /// <summary>
  /// The value of the FC_WARDS_ACCOUNT attribute.
  /// Indicates that there is money available to the child after foster care is 
  /// complete.
  /// </summary>
  [JsonPropertyName("fcWardsAccount")]
  [Member(Index = 78, Type = MemberType.Char, Length
    = FcWardsAccount_MaxLength, Optional = true)]
  public string FcWardsAccount
  {
    get => Get<string>("fcWardsAccount");
    set => Set(
      "fcWardsAccount", TrimEnd(Substring(value, 1, FcWardsAccount_MaxLength)));
      
  }

  /// <summary>Length of the FC_COUNTY_CHILD_REMOVED_FROM attribute.</summary>
  public const int FcCountyChildRemovedFrom_MaxLength = 2;

  /// <summary>
  /// The value of the FC_COUNTY_CHILD_REMOVED_FROM attribute.
  /// Abbreviation of the county from which the child was removed.
  /// </summary>
  [JsonPropertyName("fcCountyChildRemovedFrom")]
  [Member(Index = 79, Type = MemberType.Char, Length
    = FcCountyChildRemovedFrom_MaxLength, Optional = true)]
  public string FcCountyChildRemovedFrom
  {
    get => Get<string>("fcCountyChildRemovedFrom");
    set => Set(
      "fcCountyChildRemovedFrom", TrimEnd(Substring(value, 1,
      FcCountyChildRemovedFrom_MaxLength)));
  }

  /// <summary>Length of the FC_AP_NOTIFIED attribute.</summary>
  public const int FcApNotified_MaxLength = 1;

  /// <summary>
  /// The value of the FC_AP_NOTIFIED attribute.
  /// Indicates whether or not AP('s) notified of the responsibility to pay:
  /// Y - Yes
  /// N - No
  /// </summary>
  [JsonPropertyName("fcApNotified")]
  [Member(Index = 80, Type = MemberType.Char, Length = FcApNotified_MaxLength, Optional
    = true)]
  public string FcApNotified
  {
    get => Get<string>("fcApNotified");
    set => Set(
      "fcApNotified", TrimEnd(Substring(value, 1, FcApNotified_MaxLength)));
  }

  /// <summary>Length of the KS_COUNTY attribute.</summary>
  public const int KsCounty_MaxLength = 2;

  /// <summary>
  /// The value of the KS_COUNTY attribute.
  /// This determines the kansas county that the pa referral is from.
  /// </summary>
  [JsonPropertyName("ksCounty")]
  [Member(Index = 81, Type = MemberType.Char, Length = KsCounty_MaxLength, Optional
    = true)]
  public string KsCounty
  {
    get => Get<string>("ksCounty");
    set => Set("ksCounty", TrimEnd(Substring(value, 1, KsCounty_MaxLength)));
  }

  /// <summary>Length of the CSE_INVOLVEMENT_IND attribute.</summary>
  public const int CseInvolvementInd_MaxLength = 1;

  /// <summary>
  /// The value of the CSE_INVOLVEMENT_IND attribute.
  /// </summary>
  [JsonPropertyName("cseInvolvementInd")]
  [Member(Index = 82, Type = MemberType.Char, Length
    = CseInvolvementInd_MaxLength, Optional = true)]
  public string CseInvolvementInd
  {
    get => Get<string>("cseInvolvementInd");
    set => Set(
      "cseInvolvementInd", TrimEnd(Substring(value, 1,
      CseInvolvementInd_MaxLength)));
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casNumber")]
  [Member(Index = 83, Type = MemberType.Char, Length = CasNumber_MaxLength, Optional
    = true)]
  public string CasNumber
  {
    get => Get<string>("casNumber");
    set => Set("casNumber", TrimEnd(Substring(value, 1, CasNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// * Draft *
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("ospEffectiveDt")]
  [Member(Index = 84, Type = MemberType.Date, Optional = true)]
  public DateTime? OspEffectiveDt
  {
    get => Get<DateTime?>("ospEffectiveDt");
    set => Set("ospEffectiveDt", value);
  }

  /// <summary>Length of the ROLE_CODE attribute.</summary>
  public const int OspRoleCd_MaxLength = 2;

  /// <summary>
  /// The value of the ROLE_CODE attribute.
  /// This is the job title or role the person is fulfilling at or for a 
  /// particular location.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// Use Set Mnemonics
  /// </summary>
  [JsonPropertyName("ospRoleCd")]
  [Member(Index = 85, Type = MemberType.Char, Length = OspRoleCd_MaxLength, Optional
    = true)]
  public string OspRoleCd
  {
    get => Get<string>("ospRoleCd");
    set => Set("ospRoleCd", TrimEnd(Substring(value, 1, OspRoleCd_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offId")]
  [Member(Index = 86, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffId
  {
    get => Get<int?>("offId");
    set => Set("offId", value);
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdId")]
  [Member(Index = 87, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? SpdId
  {
    get => Get<int?>("spdId");
    set => Set("spdId", value);
  }
}

// The source file: INTERFACE_PA_REFERRAL, ID: 371435712, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVIN
/// Designer added interface record used as a temporary table for referrals that
/// have been copied from AE/KSCares.  A batch job will load these records into
/// the actual referral table.
/// </summary>
[Serializable]
public partial class InterfacePaReferral: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InterfacePaReferral()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InterfacePaReferral(InterfacePaReferral that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InterfacePaReferral Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InterfacePaReferral that)
  {
    base.Assign(that);
    interfaceIdentifier = that.interfaceIdentifier;
    csOrderPlace = that.csOrderPlace;
    csOrderState = that.csOrderState;
    csFreq = that.csFreq;
    from = that.from;
    apPhoneNumber = that.apPhoneNumber;
    apAreaCode = that.apAreaCode;
    ccStartDate = that.ccStartDate;
    arEmployerName = that.arEmployerName;
    supportOrderFreq = that.supportOrderFreq;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    note = that.note;
    receivedDate = that.receivedDate;
    assignDeactivateInd = that.assignDeactivateInd;
    assignDeactivateDate = that.assignDeactivateDate;
    caseNumber = that.caseNumber;
    number = that.number;
    type1 = that.type1;
    medicalPaymentDueDate = that.medicalPaymentDueDate;
    medicalAmt = that.medicalAmt;
    medicalFreq = that.medicalFreq;
    medicalLastPayment = that.medicalLastPayment;
    medicalLastPaymentDate = that.medicalLastPaymentDate;
    medicalOrderEffectiveDate = that.medicalOrderEffectiveDate;
    medicalOrderState = that.medicalOrderState;
    medicalOrderPlace = that.medicalOrderPlace;
    medicalArrearage = that.medicalArrearage;
    medicalPaidTo = that.medicalPaidTo;
    medicalPaymentType = that.medicalPaymentType;
    medicalInsuranceCo = that.medicalInsuranceCo;
    medicalPolicyNumber = that.medicalPolicyNumber;
    medicalOrderNumber = that.medicalOrderNumber;
    medicalOrderInd = that.medicalOrderInd;
    approvalDate = that.approvalDate;
    cseRegion = that.cseRegion;
    cseReferralRecDate = that.cseReferralRecDate;
    arRetainedInd = that.arRetainedInd;
    pgmCode = that.pgmCode;
    caseWorker = that.caseWorker;
    paymentMadeTo = that.paymentMadeTo;
    csArrearageAmt = that.csArrearageAmt;
    csLastPaymentAmt = that.csLastPaymentAmt;
    lastPaymentDate = that.lastPaymentDate;
    goodCauseCode = that.goodCauseCode;
    goodCauseDate = that.goodCauseDate;
    csPaymentAmount = that.csPaymentAmount;
    orderEffectiveDate = that.orderEffectiveDate;
    paymentDueDate = that.paymentDueDate;
    supportOrderId = that.supportOrderId;
    lastApContactDate = that.lastApContactDate;
    voluntarySupportInd = that.voluntarySupportInd;
    apEmployerPhone = that.apEmployerPhone;
    apEmployerName = that.apEmployerName;
    fcNextJuvenileCtDt = that.fcNextJuvenileCtDt;
    fcOrderEstBy = that.fcOrderEstBy;
    fcJuvenileCourtOrder = that.fcJuvenileCourtOrder;
    fcJuvenileOffenderInd = that.fcJuvenileOffenderInd;
    fcCincInd = that.fcCincInd;
    fcPlacementDate = that.fcPlacementDate;
    fcSrsPayee = that.fcSrsPayee;
    fcCostOfCareFreq = that.fcCostOfCareFreq;
    fcCostOfCare = that.fcCostOfCare;
    fcAdoptionDisruptionInd = that.fcAdoptionDisruptionInd;
    fcPlacementType = that.fcPlacementType;
    fcPreviousPa = that.fcPreviousPa;
    fcDateOfInitialCustody = that.fcDateOfInitialCustody;
    fcRightsSevered = that.fcRightsSevered;
    fcIvECaseNumber = that.fcIvECaseNumber;
    fcPlacementName = that.fcPlacementName;
    fcSourceOfFunding = that.fcSourceOfFunding;
    fcOtherBenefitInd = that.fcOtherBenefitInd;
    fcZebInd = that.fcZebInd;
    fcVaInd = that.fcVaInd;
    fcSsi = that.fcSsi;
    fcSsa = that.fcSsa;
    fcWardsAccount = that.fcWardsAccount;
    fcCountyChildRemovedFrom = that.fcCountyChildRemovedFrom;
    fcApNotified = that.fcApNotified;
    ksCounty = that.ksCounty;
    cseInvolvementInd = that.cseInvolvementInd;
  }

  /// <summary>Length of the INTERFACE_IDENTIFIER attribute.</summary>
  public const int InterfaceIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the INTERFACE_IDENTIFIER attribute.
  /// Unique identifier for this referral
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = InterfaceIdentifier_MaxLength)]
  public string InterfaceIdentifier
  {
    get => interfaceIdentifier ?? "";
    set => interfaceIdentifier =
      TrimEnd(Substring(value, 1, InterfaceIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the InterfaceIdentifier attribute.</summary>
  [JsonPropertyName("interfaceIdentifier")]
  [Computed]
  public string InterfaceIdentifier_Json
  {
    get => NullIf(InterfaceIdentifier, "");
    set => InterfaceIdentifier = value;
  }

  /// <summary>Length of the CS_ORDER_PLACE attribute.</summary>
  public const int CsOrderPlace_MaxLength = 17;

  /// <summary>
  /// The value of the CS_ORDER_PLACE attribute.
  /// County or city win which the child support order was established.
  /// </summary>
  [JsonPropertyName("csOrderPlace")]
  [Member(Index = 2, Type = MemberType.Char, Length = CsOrderPlace_MaxLength, Optional
    = true)]
  public string CsOrderPlace
  {
    get => csOrderPlace;
    set => csOrderPlace = value != null
      ? TrimEnd(Substring(value, 1, CsOrderPlace_MaxLength)) : null;
  }

  /// <summary>Length of the CS_ORDER_STATE attribute.</summary>
  public const int CsOrderState_MaxLength = 2;

  /// <summary>
  /// The value of the CS_ORDER_STATE attribute.
  /// Valid state-code for state in which the child support order was 
  /// established.
  /// </summary>
  [JsonPropertyName("csOrderState")]
  [Member(Index = 3, Type = MemberType.Char, Length = CsOrderState_MaxLength, Optional
    = true)]
  public string CsOrderState
  {
    get => csOrderState;
    set => csOrderState = value != null
      ? TrimEnd(Substring(value, 1, CsOrderState_MaxLength)) : null;
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
  [Member(Index = 4, Type = MemberType.Char, Length = CsFreq_MaxLength, Optional
    = true)]
  public string CsFreq
  {
    get => csFreq;
    set => csFreq = value != null
      ? TrimEnd(Substring(value, 1, CsFreq_MaxLength)) : null;
  }

  /// <summary>Length of the FROM attribute.</summary>
  public const int From_MaxLength = 3;

  /// <summary>
  /// The value of the FROM attribute.
  /// The origin of the referral. As of 11/95 may be: AE; KSC.
  /// </summary>
  [JsonPropertyName("from")]
  [Member(Index = 5, Type = MemberType.Char, Length = From_MaxLength, Optional
    = true)]
  public string From
  {
    get => from;
    set => from = value != null
      ? TrimEnd(Substring(value, 1, From_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AP_PHONE_NUMBER attribute.
  /// The telephone number of the ap
  /// </summary>
  [JsonPropertyName("apPhoneNumber")]
  [Member(Index = 6, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? ApPhoneNumber
  {
    get => apPhoneNumber;
    set => apPhoneNumber = value;
  }

  /// <summary>
  /// The value of the AP_AREA_CODE attribute.
  /// The area code of the AP's phone number.
  /// </summary>
  [JsonPropertyName("apAreaCode")]
  [Member(Index = 7, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ApAreaCode
  {
    get => apAreaCode;
    set => apAreaCode = value;
  }

  /// <summary>
  /// The value of the CC_START_DATE attribute.
  /// The date a child care plan is created and the referral is sent to CSE from
  /// KSCares.
  /// </summary>
  [JsonPropertyName("ccStartDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? CcStartDate
  {
    get => ccStartDate;
    set => ccStartDate = value;
  }

  /// <summary>Length of the AR_EMPLOYER_NAME attribute.</summary>
  public const int ArEmployerName_MaxLength = 28;

  /// <summary>
  /// The value of the AR_EMPLOYER_NAME attribute.
  /// The name of the AR;s employer
  /// </summary>
  [JsonPropertyName("arEmployerName")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ArEmployerName_MaxLength, Optional = true)]
  public string ArEmployerName
  {
    get => arEmployerName;
    set => arEmployerName = value != null
      ? TrimEnd(Substring(value, 1, ArEmployerName_MaxLength)) : null;
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
  [Member(Index = 10, Type = MemberType.Char, Length
    = SupportOrderFreq_MaxLength, Optional = true)]
  public string SupportOrderFreq
  {
    get => supportOrderFreq;
    set => supportOrderFreq = value != null
      ? TrimEnd(Substring(value, 1, SupportOrderFreq_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 14, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 80;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Miscellaneous information (free-form) entered by the Economic Assistance 
  /// worker.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 15, Type = MemberType.Char, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null
      ? TrimEnd(Substring(value, 1, Note_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// This is the date the PA Referral record was created by the interface 
  /// program.  It is used as a control to ensure timely working of Referrals.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? ReceivedDate
  {
    get => receivedDate;
    set => receivedDate = value;
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
  [Member(Index = 17, Type = MemberType.Char, Length
    = AssignDeactivateInd_MaxLength, Optional = true)]
  public string AssignDeactivateInd
  {
    get => assignDeactivateInd;
    set => assignDeactivateInd = value != null
      ? TrimEnd(Substring(value, 1, AssignDeactivateInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ASSIGN_DEACTIVATE_DATE attribute.
  /// Date CSE worker decides to complete (Assign or Deactivate) this Referral.
  /// </summary>
  [JsonPropertyName("assignDeactivateDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? AssignDeactivateDate
  {
    get => assignDeactivateDate;
    set => assignDeactivateDate = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The number assigned to the PA case.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 19, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Referral Number (Unique identifier)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => number ?? "";
    set => number = TrimEnd(Substring(value, 1, Number_MaxLength));
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
  [Member(Index = 21, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
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
  [Member(Index = 22, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalPaymentDueDate
  {
    get => medicalPaymentDueDate;
    set => medicalPaymentDueDate = value;
  }

  /// <summary>
  /// The value of the MEDICAL_AMT attribute.
  /// The amount of the Medical Support Order.
  /// </summary>
  [JsonPropertyName("medicalAmt")]
  [Member(Index = 23, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalAmt
  {
    get => medicalAmt;
    set => medicalAmt = value != null ? Truncate(value, 2) : null;
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
  [Member(Index = 24, Type = MemberType.Char, Length = MedicalFreq_MaxLength, Optional
    = true)]
  public string MedicalFreq
  {
    get => medicalFreq;
    set => medicalFreq = value != null
      ? TrimEnd(Substring(value, 1, MedicalFreq_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MEDICAL_LAST_PAYMENT attribute.
  /// The amount of the last Medical Support Order payment.
  /// </summary>
  [JsonPropertyName("medicalLastPayment")]
  [Member(Index = 25, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalLastPayment
  {
    get => medicalLastPayment;
    set => medicalLastPayment = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the MEDICAL_LAST_PAYMENT_DATE attribute.
  /// The date of the last Medical Support Order payment.
  /// </summary>
  [JsonPropertyName("medicalLastPaymentDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalLastPaymentDate
  {
    get => medicalLastPaymentDate;
    set => medicalLastPaymentDate = value;
  }

  /// <summary>
  /// The value of the MEDICAL_ORDER_EFFECTIVE_DATE attribute.
  /// The date on which the Medical Support Order was effective.
  /// </summary>
  [JsonPropertyName("medicalOrderEffectiveDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? MedicalOrderEffectiveDate
  {
    get => medicalOrderEffectiveDate;
    set => medicalOrderEffectiveDate = value;
  }

  /// <summary>Length of the MEDICAL_ORDER_STATE attribute.</summary>
  public const int MedicalOrderState_MaxLength = 2;

  /// <summary>
  /// The value of the MEDICAL_ORDER_STATE attribute.
  /// The state where the Medical Support Order was issued.
  /// </summary>
  [JsonPropertyName("medicalOrderState")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = MedicalOrderState_MaxLength, Optional = true)]
  public string MedicalOrderState
  {
    get => medicalOrderState;
    set => medicalOrderState = value != null
      ? TrimEnd(Substring(value, 1, MedicalOrderState_MaxLength)) : null;
  }

  /// <summary>Length of the MEDICAL_ORDER_PLACE attribute.</summary>
  public const int MedicalOrderPlace_MaxLength = 17;

  /// <summary>
  /// The value of the MEDICAL_ORDER_PLACE attribute.
  /// The city or county where the Medical Support Order was issued.
  /// </summary>
  [JsonPropertyName("medicalOrderPlace")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = MedicalOrderPlace_MaxLength, Optional = true)]
  public string MedicalOrderPlace
  {
    get => medicalOrderPlace;
    set => medicalOrderPlace = value != null
      ? TrimEnd(Substring(value, 1, MedicalOrderPlace_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MEDICAL_ARREARAGE attribute.
  /// The total arrearage of Medical Support Order payments.
  /// </summary>
  [JsonPropertyName("medicalArrearage")]
  [Member(Index = 30, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? MedicalArrearage
  {
    get => medicalArrearage;
    set => medicalArrearage = value != null ? Truncate(value, 2) : null;
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
  [Member(Index = 31, Type = MemberType.Char, Length
    = MedicalPaidTo_MaxLength, Optional = true)]
  public string MedicalPaidTo
  {
    get => medicalPaidTo;
    set => medicalPaidTo = value != null
      ? TrimEnd(Substring(value, 1, MedicalPaidTo_MaxLength)) : null;
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
  [Member(Index = 32, Type = MemberType.Char, Length
    = MedicalPaymentType_MaxLength, Optional = true)]
  public string MedicalPaymentType
  {
    get => medicalPaymentType;
    set => medicalPaymentType = value != null
      ? TrimEnd(Substring(value, 1, MedicalPaymentType_MaxLength)) : null;
  }

  /// <summary>Length of the MEDICAL_INSURANCE_CO attribute.</summary>
  public const int MedicalInsuranceCo_MaxLength = 25;

  /// <summary>
  /// The value of the MEDICAL_INSURANCE_CO attribute.
  /// The name of the company providing medical insurance coverage.
  /// </summary>
  [JsonPropertyName("medicalInsuranceCo")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = MedicalInsuranceCo_MaxLength, Optional = true)]
  public string MedicalInsuranceCo
  {
    get => medicalInsuranceCo;
    set => medicalInsuranceCo = value != null
      ? TrimEnd(Substring(value, 1, MedicalInsuranceCo_MaxLength)) : null;
  }

  /// <summary>Length of the MEDICAL_POLICY_NUMBER attribute.</summary>
  public const int MedicalPolicyNumber_MaxLength = 20;

  /// <summary>
  /// The value of the MEDICAL_POLICY_NUMBER attribute.
  /// The policy number for medical insurance coverage.
  /// </summary>
  [JsonPropertyName("medicalPolicyNumber")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = MedicalPolicyNumber_MaxLength, Optional = true)]
  public string MedicalPolicyNumber
  {
    get => medicalPolicyNumber;
    set => medicalPolicyNumber = value != null
      ? TrimEnd(Substring(value, 1, MedicalPolicyNumber_MaxLength)) : null;
  }

  /// <summary>Length of the MEDICAL_ORDER_NUMBER attribute.</summary>
  public const int MedicalOrderNumber_MaxLength = 17;

  /// <summary>
  /// The value of the MEDICAL_ORDER_NUMBER attribute.
  /// The identifier of the Medical Support Order
  /// </summary>
  [JsonPropertyName("medicalOrderNumber")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = MedicalOrderNumber_MaxLength, Optional = true)]
  public string MedicalOrderNumber
  {
    get => medicalOrderNumber;
    set => medicalOrderNumber = value != null
      ? TrimEnd(Substring(value, 1, MedicalOrderNumber_MaxLength)) : null;
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
  [Member(Index = 36, Type = MemberType.Char, Length
    = MedicalOrderInd_MaxLength, Optional = true)]
  public string MedicalOrderInd
  {
    get => medicalOrderInd;
    set => medicalOrderInd = value != null
      ? TrimEnd(Substring(value, 1, MedicalOrderInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the APPROVAL_DATE attribute.
  /// Referral approval date
  /// </summary>
  [JsonPropertyName("approvalDate")]
  [Member(Index = 37, Type = MemberType.Date, Optional = true)]
  public DateTime? ApprovalDate
  {
    get => approvalDate;
    set => approvalDate = value;
  }

  /// <summary>Length of the CSE_REGION attribute.</summary>
  public const int CseRegion_MaxLength = 2;

  /// <summary>
  /// The value of the CSE_REGION attribute.
  /// CSE region handling Case
  /// </summary>
  [JsonPropertyName("cseRegion")]
  [Member(Index = 38, Type = MemberType.Char, Length = CseRegion_MaxLength, Optional
    = true)]
  public string CseRegion
  {
    get => cseRegion;
    set => cseRegion = value != null
      ? TrimEnd(Substring(value, 1, CseRegion_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CSE_REFERRAL_REC_DATE attribute.
  /// The date the referral was received by CSE.
  /// </summary>
  [JsonPropertyName("cseReferralRecDate")]
  [Member(Index = 39, Type = MemberType.Date, Optional = true)]
  public DateTime? CseReferralRecDate
  {
    get => cseReferralRecDate;
    set => cseReferralRecDate = value;
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
  [Member(Index = 40, Type = MemberType.Char, Length
    = ArRetainedInd_MaxLength, Optional = true)]
  public string ArRetainedInd
  {
    get => arRetainedInd;
    set => arRetainedInd = value != null
      ? TrimEnd(Substring(value, 1, ArRetainedInd_MaxLength)) : null;
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
  [Member(Index = 41, Type = MemberType.Char, Length = PgmCode_MaxLength, Optional
    = true)]
  public string PgmCode
  {
    get => pgmCode;
    set => pgmCode = value != null
      ? TrimEnd(Substring(value, 1, PgmCode_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_WORKER attribute.</summary>
  public const int CaseWorker_MaxLength = 8;

  /// <summary>
  /// The value of the CASE_WORKER attribute.
  /// Partial name of the AE EA, or DSCares worker assigned to the PA case.
  /// </summary>
  [JsonPropertyName("caseWorker")]
  [Member(Index = 42, Type = MemberType.Char, Length = CaseWorker_MaxLength, Optional
    = true)]
  public string CaseWorker
  {
    get => caseWorker;
    set => caseWorker = value != null
      ? TrimEnd(Substring(value, 1, CaseWorker_MaxLength)) : null;
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
  [Member(Index = 43, Type = MemberType.Char, Length
    = PaymentMadeTo_MaxLength, Optional = true)]
  public string PaymentMadeTo
  {
    get => paymentMadeTo;
    set => paymentMadeTo = value != null
      ? TrimEnd(Substring(value, 1, PaymentMadeTo_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CS_ARREARAGE_AMT attribute.
  /// This is the amount of child support arrears.
  /// </summary>
  [JsonPropertyName("csArrearageAmt")]
  [Member(Index = 44, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsArrearageAmt
  {
    get => csArrearageAmt;
    set => csArrearageAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CS_LAST_PAYMENT_AMT attribute.
  /// The amount of the last child support payment.
  /// </summary>
  [JsonPropertyName("csLastPaymentAmt")]
  [Member(Index = 45, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsLastPaymentAmt
  {
    get => csLastPaymentAmt;
    set => csLastPaymentAmt = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the LAST_PAYMENT_DATE attribute.
  /// This is the last date on which a child support payment was received from 
  /// the AP.
  /// </summary>
  [JsonPropertyName("lastPaymentDate")]
  [Member(Index = 46, Type = MemberType.Date, Optional = true)]
  public DateTime? LastPaymentDate
  {
    get => lastPaymentDate;
    set => lastPaymentDate = value;
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
  [Member(Index = 47, Type = MemberType.Char, Length
    = GoodCauseCode_MaxLength, Optional = true)]
  public string GoodCauseCode
  {
    get => goodCauseCode;
    set => goodCauseCode = value != null
      ? TrimEnd(Substring(value, 1, GoodCauseCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the GOOD_CAUSE_DATE attribute.
  /// Date 'Good Cause' established
  /// </summary>
  [JsonPropertyName("goodCauseDate")]
  [Member(Index = 48, Type = MemberType.Date, Optional = true)]
  public DateTime? GoodCauseDate
  {
    get => goodCauseDate;
    set => goodCauseDate = value;
  }

  /// <summary>
  /// The value of the CS_PAYMENT_AMOUNT attribute.
  /// This is the ordered amount of each child support payment.
  /// </summary>
  [JsonPropertyName("csPaymentAmount")]
  [Member(Index = 49, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CsPaymentAmount
  {
    get => csPaymentAmount;
    set => csPaymentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the ORDER_EFFECTIVE_DATE attribute.
  /// The is the date on which the Child Support Order was effective.
  /// </summary>
  [JsonPropertyName("orderEffectiveDate")]
  [Member(Index = 50, Type = MemberType.Date, Optional = true)]
  public DateTime? OrderEffectiveDate
  {
    get => orderEffectiveDate;
    set => orderEffectiveDate = value;
  }

  /// <summary>
  /// The value of the PAYMENT_DUE_DATE attribute.
  /// This is the first date on which child support payment was due.
  /// </summary>
  [JsonPropertyName("paymentDueDate")]
  [Member(Index = 51, Type = MemberType.Date, Optional = true)]
  public DateTime? PaymentDueDate
  {
    get => paymentDueDate;
    set => paymentDueDate = value;
  }

  /// <summary>Length of the SUPPORT_ORDER_ID attribute.</summary>
  public const int SupportOrderId_MaxLength = 17;

  /// <summary>
  /// The value of the SUPPORT_ORDER_ID attribute.
  /// This is the Child Support Court Order number that establishes support.
  /// </summary>
  [JsonPropertyName("supportOrderId")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = SupportOrderId_MaxLength, Optional = true)]
  public string SupportOrderId
  {
    get => supportOrderId;
    set => supportOrderId = value != null
      ? TrimEnd(Substring(value, 1, SupportOrderId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_AP_CONTACT_DATE attribute.
  /// Last date on which AP contacted child
  /// </summary>
  [JsonPropertyName("lastApContactDate")]
  [Member(Index = 53, Type = MemberType.Date, Optional = true)]
  public DateTime? LastApContactDate
  {
    get => lastApContactDate;
    set => lastApContactDate = value;
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
  [Member(Index = 54, Type = MemberType.Char, Length
    = VoluntarySupportInd_MaxLength, Optional = true)]
  public string VoluntarySupportInd
  {
    get => voluntarySupportInd;
    set => voluntarySupportInd = value != null
      ? TrimEnd(Substring(value, 1, VoluntarySupportInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the AP_EMPLOYER_PHONE attribute.
  /// This is the phone number of the AP Employer.
  /// </summary>
  [JsonPropertyName("apEmployerPhone")]
  [Member(Index = 55, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? ApEmployerPhone
  {
    get => apEmployerPhone;
    set => apEmployerPhone = value;
  }

  /// <summary>Length of the AP_EMPLOYER_NAME attribute.</summary>
  public const int ApEmployerName_MaxLength = 28;

  /// <summary>
  /// The value of the AP_EMPLOYER_NAME attribute.
  /// This is the name of the AP Employer
  /// </summary>
  [JsonPropertyName("apEmployerName")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = ApEmployerName_MaxLength, Optional = true)]
  public string ApEmployerName
  {
    get => apEmployerName;
    set => apEmployerName = value != null
      ? TrimEnd(Substring(value, 1, ApEmployerName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FC_NEXT_JUVENILE_CT_DT attribute.
  /// This is the date of the next session of the Juvenile Court which manages 
  /// this Case.
  /// </summary>
  [JsonPropertyName("fcNextJuvenileCtDt")]
  [Member(Index = 57, Type = MemberType.Date, Optional = true)]
  public DateTime? FcNextJuvenileCtDt
  {
    get => fcNextJuvenileCtDt;
    set => fcNextJuvenileCtDt = value;
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
  [Member(Index = 58, Type = MemberType.Char, Length = FcOrderEstBy_MaxLength, Optional
    = true)]
  public string FcOrderEstBy
  {
    get => fcOrderEstBy;
    set => fcOrderEstBy = value != null
      ? TrimEnd(Substring(value, 1, FcOrderEstBy_MaxLength)) : null;
  }

  /// <summary>Length of the FC_JUVENILE_COURT_ORDER attribute.</summary>
  public const int FcJuvenileCourtOrder_MaxLength = 12;

  /// <summary>
  /// The value of the FC_JUVENILE_COURT_ORDER attribute.
  /// Juvenile court order number
  /// </summary>
  [JsonPropertyName("fcJuvenileCourtOrder")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = FcJuvenileCourtOrder_MaxLength, Optional = true)]
  public string FcJuvenileCourtOrder
  {
    get => fcJuvenileCourtOrder;
    set => fcJuvenileCourtOrder = value != null
      ? TrimEnd(Substring(value, 1, FcJuvenileCourtOrder_MaxLength)) : null;
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
  [Member(Index = 60, Type = MemberType.Char, Length
    = FcJuvenileOffenderInd_MaxLength, Optional = true)]
  public string FcJuvenileOffenderInd
  {
    get => fcJuvenileOffenderInd;
    set => fcJuvenileOffenderInd = value != null
      ? TrimEnd(Substring(value, 1, FcJuvenileOffenderInd_MaxLength)) : null;
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
  [Member(Index = 61, Type = MemberType.Char, Length = FcCincInd_MaxLength, Optional
    = true)]
  public string FcCincInd
  {
    get => fcCincInd;
    set => fcCincInd = value != null
      ? TrimEnd(Substring(value, 1, FcCincInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FC_PLACEMENT_DATE attribute.
  /// Date child placed in FC
  /// </summary>
  [JsonPropertyName("fcPlacementDate")]
  [Member(Index = 62, Type = MemberType.Date, Optional = true)]
  public DateTime? FcPlacementDate
  {
    get => fcPlacementDate;
    set => fcPlacementDate = value;
  }

  /// <summary>Length of the FC_SRS_PAYEE attribute.</summary>
  public const int FcSrsPayee_MaxLength = 1;

  /// <summary>
  /// The value of the FC_SRS_PAYEE attribute.
  /// Indicates whether or not SRS has been determined to be the payee.
  /// </summary>
  [JsonPropertyName("fcSrsPayee")]
  [Member(Index = 63, Type = MemberType.Char, Length = FcSrsPayee_MaxLength, Optional
    = true)]
  public string FcSrsPayee
  {
    get => fcSrsPayee;
    set => fcSrsPayee = value != null
      ? TrimEnd(Substring(value, 1, FcSrsPayee_MaxLength)) : null;
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
  [Member(Index = 64, Type = MemberType.Char, Length
    = FcCostOfCareFreq_MaxLength, Optional = true)]
  public string FcCostOfCareFreq
  {
    get => fcCostOfCareFreq;
    set => fcCostOfCareFreq = value != null
      ? TrimEnd(Substring(value, 1, FcCostOfCareFreq_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FC_COST_OF_CARE attribute.
  /// The Amount paid for the care of the child.
  /// </summary>
  [JsonPropertyName("fcCostOfCare")]
  [Member(Index = 65, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? FcCostOfCare
  {
    get => fcCostOfCare;
    set => fcCostOfCare = value != null ? Truncate(value, 2) : null;
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
  [Member(Index = 66, Type = MemberType.Char, Length
    = FcAdoptionDisruptionInd_MaxLength, Optional = true)]
  public string FcAdoptionDisruptionInd
  {
    get => fcAdoptionDisruptionInd;
    set => fcAdoptionDisruptionInd = value != null
      ? TrimEnd(Substring(value, 1, FcAdoptionDisruptionInd_MaxLength)) : null;
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
  [Member(Index = 67, Type = MemberType.Char, Length
    = FcPlacementType_MaxLength, Optional = true)]
  public string FcPlacementType
  {
    get => fcPlacementType;
    set => fcPlacementType = value != null
      ? TrimEnd(Substring(value, 1, FcPlacementType_MaxLength)) : null;
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
  [Member(Index = 68, Type = MemberType.Char, Length = FcPreviousPa_MaxLength, Optional
    = true)]
  public string FcPreviousPa
  {
    get => fcPreviousPa;
    set => fcPreviousPa = value != null
      ? TrimEnd(Substring(value, 1, FcPreviousPa_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the FC_DATE_OF_INITIAL_CUSTODY attribute.
  /// Initial date FC began
  /// </summary>
  [JsonPropertyName("fcDateOfInitialCustody")]
  [Member(Index = 69, Type = MemberType.Date, Optional = true)]
  public DateTime? FcDateOfInitialCustody
  {
    get => fcDateOfInitialCustody;
    set => fcDateOfInitialCustody = value;
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
  [Member(Index = 70, Type = MemberType.Char, Length
    = FcRightsSevered_MaxLength, Optional = true)]
  public string FcRightsSevered
  {
    get => fcRightsSevered;
    set => fcRightsSevered = value != null
      ? TrimEnd(Substring(value, 1, FcRightsSevered_MaxLength)) : null;
  }

  /// <summary>Length of the FC_IV_E_CASE_NUMBER attribute.</summary>
  public const int FcIvECaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the FC_IV_E_CASE_NUMBER attribute.
  /// Foster Care IV-E Case #
  /// </summary>
  [JsonPropertyName("fcIvECaseNumber")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = FcIvECaseNumber_MaxLength, Optional = true)]
  public string FcIvECaseNumber
  {
    get => fcIvECaseNumber;
    set => fcIvECaseNumber = value != null
      ? TrimEnd(Substring(value, 1, FcIvECaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the FC_PLACEMENT_NAME attribute.</summary>
  public const int FcPlacementName_MaxLength = 25;

  /// <summary>
  /// The value of the FC_PLACEMENT_NAME attribute.
  /// Name of institution where child placed
  /// </summary>
  [JsonPropertyName("fcPlacementName")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = FcPlacementName_MaxLength, Optional = true)]
  public string FcPlacementName
  {
    get => fcPlacementName;
    set => fcPlacementName = value != null
      ? TrimEnd(Substring(value, 1, FcPlacementName_MaxLength)) : null;
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
  [Member(Index = 73, Type = MemberType.Char, Length
    = FcSourceOfFunding_MaxLength, Optional = true)]
  public string FcSourceOfFunding
  {
    get => fcSourceOfFunding;
    set => fcSourceOfFunding = value != null
      ? TrimEnd(Substring(value, 1, FcSourceOfFunding_MaxLength)) : null;
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
  [Member(Index = 74, Type = MemberType.Char, Length
    = FcOtherBenefitInd_MaxLength, Optional = true)]
  public string FcOtherBenefitInd
  {
    get => fcOtherBenefitInd;
    set => fcOtherBenefitInd = value != null
      ? TrimEnd(Substring(value, 1, FcOtherBenefitInd_MaxLength)) : null;
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
  [Member(Index = 75, Type = MemberType.Char, Length = FcZebInd_MaxLength, Optional
    = true)]
  public string FcZebInd
  {
    get => fcZebInd;
    set => fcZebInd = value != null
      ? TrimEnd(Substring(value, 1, FcZebInd_MaxLength)) : null;
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
  [Member(Index = 76, Type = MemberType.Char, Length = FcVaInd_MaxLength, Optional
    = true)]
  public string FcVaInd
  {
    get => fcVaInd;
    set => fcVaInd = value != null
      ? TrimEnd(Substring(value, 1, FcVaInd_MaxLength)) : null;
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
  [Member(Index = 77, Type = MemberType.Char, Length = FcSsi_MaxLength, Optional
    = true)]
  public string FcSsi
  {
    get => fcSsi;
    set => fcSsi = value != null
      ? TrimEnd(Substring(value, 1, FcSsi_MaxLength)) : null;
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
  [Member(Index = 78, Type = MemberType.Char, Length = FcSsa_MaxLength, Optional
    = true)]
  public string FcSsa
  {
    get => fcSsa;
    set => fcSsa = value != null
      ? TrimEnd(Substring(value, 1, FcSsa_MaxLength)) : null;
  }

  /// <summary>Length of the FC_WARDS_ACCOUNT attribute.</summary>
  public const int FcWardsAccount_MaxLength = 1;

  /// <summary>
  /// The value of the FC_WARDS_ACCOUNT attribute.
  /// Indicates that there is money available to the child after foster care is 
  /// complete.
  /// </summary>
  [JsonPropertyName("fcWardsAccount")]
  [Member(Index = 79, Type = MemberType.Char, Length
    = FcWardsAccount_MaxLength, Optional = true)]
  public string FcWardsAccount
  {
    get => fcWardsAccount;
    set => fcWardsAccount = value != null
      ? TrimEnd(Substring(value, 1, FcWardsAccount_MaxLength)) : null;
  }

  /// <summary>Length of the FC_COUNTY_CHILD_REMOVED_FROM attribute.</summary>
  public const int FcCountyChildRemovedFrom_MaxLength = 2;

  /// <summary>
  /// The value of the FC_COUNTY_CHILD_REMOVED_FROM attribute.
  /// Abbreviation of the county from which the child was removed.
  /// </summary>
  [JsonPropertyName("fcCountyChildRemovedFrom")]
  [Member(Index = 80, Type = MemberType.Char, Length
    = FcCountyChildRemovedFrom_MaxLength, Optional = true)]
  public string FcCountyChildRemovedFrom
  {
    get => fcCountyChildRemovedFrom;
    set => fcCountyChildRemovedFrom = value != null
      ? TrimEnd(Substring(value, 1, FcCountyChildRemovedFrom_MaxLength)) : null
      ;
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
  [Member(Index = 81, Type = MemberType.Char, Length = FcApNotified_MaxLength, Optional
    = true)]
  public string FcApNotified
  {
    get => fcApNotified;
    set => fcApNotified = value != null
      ? TrimEnd(Substring(value, 1, FcApNotified_MaxLength)) : null;
  }

  /// <summary>Length of the KS_COUNTY attribute.</summary>
  public const int KsCounty_MaxLength = 2;

  /// <summary>
  /// The value of the KS_COUNTY attribute.
  /// This determines the kansas county that the pa referral is from.
  /// </summary>
  [JsonPropertyName("ksCounty")]
  [Member(Index = 82, Type = MemberType.Char, Length = KsCounty_MaxLength, Optional
    = true)]
  public string KsCounty
  {
    get => ksCounty;
    set => ksCounty = value != null
      ? TrimEnd(Substring(value, 1, KsCounty_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_INVOLVEMENT_IND attribute.</summary>
  public const int CseInvolvementInd_MaxLength = 1;

  /// <summary>
  /// The value of the CSE_INVOLVEMENT_IND attribute.
  /// Indicates that any one or more of the participants on this referral have, 
  /// at some time, CSE involvement.
  /// 	
  /// 	Values:	Y, space
  /// </summary>
  [JsonPropertyName("cseInvolvementInd")]
  [Member(Index = 83, Type = MemberType.Char, Length
    = CseInvolvementInd_MaxLength, Optional = true)]
  public string CseInvolvementInd
  {
    get => cseInvolvementInd;
    set => cseInvolvementInd = value != null
      ? TrimEnd(Substring(value, 1, CseInvolvementInd_MaxLength)) : null;
  }

  private string interfaceIdentifier;
  private string csOrderPlace;
  private string csOrderState;
  private string csFreq;
  private string from;
  private int? apPhoneNumber;
  private int? apAreaCode;
  private DateTime? ccStartDate;
  private string arEmployerName;
  private string supportOrderFreq;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string note;
  private DateTime? receivedDate;
  private string assignDeactivateInd;
  private DateTime? assignDeactivateDate;
  private string caseNumber;
  private string number;
  private string type1;
  private DateTime? medicalPaymentDueDate;
  private decimal? medicalAmt;
  private string medicalFreq;
  private decimal? medicalLastPayment;
  private DateTime? medicalLastPaymentDate;
  private DateTime? medicalOrderEffectiveDate;
  private string medicalOrderState;
  private string medicalOrderPlace;
  private decimal? medicalArrearage;
  private string medicalPaidTo;
  private string medicalPaymentType;
  private string medicalInsuranceCo;
  private string medicalPolicyNumber;
  private string medicalOrderNumber;
  private string medicalOrderInd;
  private DateTime? approvalDate;
  private string cseRegion;
  private DateTime? cseReferralRecDate;
  private string arRetainedInd;
  private string pgmCode;
  private string caseWorker;
  private string paymentMadeTo;
  private decimal? csArrearageAmt;
  private decimal? csLastPaymentAmt;
  private DateTime? lastPaymentDate;
  private string goodCauseCode;
  private DateTime? goodCauseDate;
  private decimal? csPaymentAmount;
  private DateTime? orderEffectiveDate;
  private DateTime? paymentDueDate;
  private string supportOrderId;
  private DateTime? lastApContactDate;
  private string voluntarySupportInd;
  private long? apEmployerPhone;
  private string apEmployerName;
  private DateTime? fcNextJuvenileCtDt;
  private string fcOrderEstBy;
  private string fcJuvenileCourtOrder;
  private string fcJuvenileOffenderInd;
  private string fcCincInd;
  private DateTime? fcPlacementDate;
  private string fcSrsPayee;
  private string fcCostOfCareFreq;
  private decimal? fcCostOfCare;
  private string fcAdoptionDisruptionInd;
  private string fcPlacementType;
  private string fcPreviousPa;
  private DateTime? fcDateOfInitialCustody;
  private string fcRightsSevered;
  private string fcIvECaseNumber;
  private string fcPlacementName;
  private string fcSourceOfFunding;
  private string fcOtherBenefitInd;
  private string fcZebInd;
  private string fcVaInd;
  private string fcSsi;
  private string fcSsa;
  private string fcWardsAccount;
  private string fcCountyChildRemovedFrom;
  private string fcApNotified;
  private string ksCounty;
  private string cseInvolvementInd;
}

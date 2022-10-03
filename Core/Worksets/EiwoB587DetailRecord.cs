// The source file: EIWO_B587_DETAIL_RECORD, ID: 1902488057, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EiwoB587DetailRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EiwoB587DetailRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EiwoB587DetailRecord(EiwoB587DetailRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EiwoB587DetailRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EiwoB587DetailRecord that)
  {
    base.Assign(that);
    incomeWithholdingStartInstruc = that.incomeWithholdingStartInstruc;
    documentCode = that.documentCode;
    documentActionCode = that.documentActionCode;
    documentDate = that.documentDate;
    issuingStateTribeTerritoryNm = that.issuingStateTribeTerritoryNm;
    issuingJurisdictionName = that.issuingJurisdictionName;
    caseIdentifier = that.caseIdentifier;
    employerName = that.employerName;
    employerAddressLine1 = that.employerAddressLine1;
    employerAddressLine2 = that.employerAddressLine2;
    employerAddressCityName = that.employerAddressCityName;
    employerAddressStateCode = that.employerAddressStateCode;
    employerAddressZipCode = that.employerAddressZipCode;
    employerAddressExtZipCode = that.employerAddressExtZipCode;
    ein = that.ein;
    employeeLastName = that.employeeLastName;
    employeeFirstName = that.employeeFirstName;
    employeeMiddleName = that.employeeMiddleName;
    employeeSuffix = that.employeeSuffix;
    employeeSsn = that.employeeSsn;
    employeeBirthDate = that.employeeBirthDate;
    obligeeLastName = that.obligeeLastName;
    obligeeFirstName = that.obligeeFirstName;
    obligeeMiddleName = that.obligeeMiddleName;
    obligeeNameSuffix = that.obligeeNameSuffix;
    issuingTribunalName = that.issuingTribunalName;
    supportCurrentChildAmount = that.supportCurrentChildAmount;
    supportCurrentChildFrequency = that.supportCurrentChildFrequency;
    supportPastDueChildAmount = that.supportPastDueChildAmount;
    supportPastDueChildFrequency = that.supportPastDueChildFrequency;
    supportCurrentMedicalAmount = that.supportCurrentMedicalAmount;
    supportCurrentMedicalFrequenc = that.supportCurrentMedicalFrequenc;
    supportPastDueMedicalAmount = that.supportPastDueMedicalAmount;
    supportPastDueMedicalFrequen = that.supportPastDueMedicalFrequen;
    supportCurrentSpousalAmount = that.supportCurrentSpousalAmount;
    supportCurrentSpousalFrequenc = that.supportCurrentSpousalFrequenc;
    supportPastDueSpousalAmount = that.supportPastDueSpousalAmount;
    supportPastDueSpousalFrequen = that.supportPastDueSpousalFrequen;
    obligationOtherAmount = that.obligationOtherAmount;
    obligationOtherFrequencyCode = that.obligationOtherFrequencyCode;
    obligationOtherDescription = that.obligationOtherDescription;
    obligationTotalAmount = that.obligationTotalAmount;
    obligationTotalFrequency = that.obligationTotalFrequency;
    arrears12WkOverdueCode = that.arrears12WkOverdueCode;
    iwoDeductionWeeklyAmount = that.iwoDeductionWeeklyAmount;
    iwoDeductionBiweeklyAmount = that.iwoDeductionBiweeklyAmount;
    iwoDeductionSemimonthlyAmount = that.iwoDeductionSemimonthlyAmount;
    iwoDeductionMonthlyAmount = that.iwoDeductionMonthlyAmount;
    stateTribeTerritoryName = that.stateTribeTerritoryName;
    beginWithholdingWithinDays = that.beginWithholdingWithinDays;
    incomeWithholdingStartDate = that.incomeWithholdingStartDate;
    sendPaymentWithhinDays = that.sendPaymentWithhinDays;
    iwoCcpaPercentRate = that.iwoCcpaPercentRate;
    payeeName = that.payeeName;
    payeeAddressLine1 = that.payeeAddressLine1;
    payeeAddressLine2 = that.payeeAddressLine2;
    payeeAddressCity = that.payeeAddressCity;
    payeeAddressStateCode = that.payeeAddressStateCode;
    payeeAddressZipCode = that.payeeAddressZipCode;
    payeeAddressExtZipCode = that.payeeAddressExtZipCode;
    payeeRemittanceFipsCode = that.payeeRemittanceFipsCode;
    governmentOfficialName = that.governmentOfficialName;
    issuingOfficialTitle = that.issuingOfficialTitle;
    sendEmployeeCopyIndicator = that.sendEmployeeCopyIndicator;
    penaltyLiabilityInfoText = that.penaltyLiabilityInfoText;
    antidiscriminationProvisionTxt = that.antidiscriminationProvisionTxt;
    specificPayeeWithholdingLimit = that.specificPayeeWithholdingLimit;
    employeeStateContactName = that.employeeStateContactName;
    employeeStateContactPhone = that.employeeStateContactPhone;
    employeeStateContactFax = that.employeeStateContactFax;
    employeeStateContactEmail = that.employeeStateContactEmail;
    documentTrackingNumber = that.documentTrackingNumber;
    orderIdentifier = that.orderIdentifier;
    employerContactName = that.employerContactName;
    employerContactAddressLine1 = that.employerContactAddressLine1;
    employerContactAddressLine2 = that.employerContactAddressLine2;
    employerContactAddressCity = that.employerContactAddressCity;
    employerContactAddressState = that.employerContactAddressState;
    employerContactAddressZip = that.employerContactAddressZip;
    employerContactAddressExtZip = that.employerContactAddressExtZip;
    employerContactPhone = that.employerContactPhone;
    employerContactFax = that.employerContactFax;
    employerContactEmail = that.employerContactEmail;
    child1LastName = that.child1LastName;
    child1FirstName = that.child1FirstName;
    child1MiddleName = that.child1MiddleName;
    child1SuffixName = that.child1SuffixName;
    child1BirthDate = that.child1BirthDate;
    child2LastName = that.child2LastName;
    child2FirstName = that.child2FirstName;
    child2MiddleName = that.child2MiddleName;
    child2SuffixName = that.child2SuffixName;
    child2BirthDate = that.child2BirthDate;
    child3LastName = that.child3LastName;
    child3FirstName = that.child3FirstName;
    child3MiddleName = that.child3MiddleName;
    child3SuffixName = that.child3SuffixName;
    child3BirthDate = that.child3BirthDate;
    child4LastName = that.child4LastName;
    child4FirstName = that.child4FirstName;
    child4MiddleName = that.child4MiddleName;
    child4SuffixName = that.child4SuffixName;
    child4BirthDate = that.child4BirthDate;
    child5LastName = that.child5LastName;
    child5FirstName = that.child5FirstName;
    child5MiddleName = that.child5MiddleName;
    child5SuffixName = that.child5SuffixName;
    child5BirthDate = that.child5BirthDate;
    child6LastName = that.child6LastName;
    child6FirstName = that.child6FirstName;
    child6MiddleName = that.child6MiddleName;
    child6SuffixName = that.child6SuffixName;
    child6BirthDate = that.child6BirthDate;
    lumpSumPaymentAmount = that.lumpSumPaymentAmount;
    remittanceIdentifier = that.remittanceIdentifier;
    documentImageText = that.documentImageText;
  }

  /// <summary>Length of the INCOME_WITHHOLDING_START_INSTRUC attribute.
  /// </summary>
  public const int IncomeWithholdingStartInstruc_MaxLength = 8;

  /// <summary>
  /// The value of the INCOME_WITHHOLDING_START_INSTRUC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = IncomeWithholdingStartInstruc_MaxLength)]
  public string IncomeWithholdingStartInstruc
  {
    get => incomeWithholdingStartInstruc ?? "";
    set => incomeWithholdingStartInstruc =
      TrimEnd(Substring(value, 1, IncomeWithholdingStartInstruc_MaxLength));
  }

  /// <summary>
  /// The json value of the IncomeWithholdingStartInstruc attribute.</summary>
  [JsonPropertyName("incomeWithholdingStartInstruc")]
  [Computed]
  public string IncomeWithholdingStartInstruc_Json
  {
    get => NullIf(IncomeWithholdingStartInstruc, "");
    set => IncomeWithholdingStartInstruc = value;
  }

  /// <summary>Length of the DOCUMENT_CODE attribute.</summary>
  public const int DocumentCode_MaxLength = 3;

  /// <summary>
  /// The value of the DOCUMENT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = DocumentCode_MaxLength)]
  public string DocumentCode
  {
    get => documentCode ?? "";
    set => documentCode = TrimEnd(Substring(value, 1, DocumentCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentCode attribute.</summary>
  [JsonPropertyName("documentCode")]
  [Computed]
  public string DocumentCode_Json
  {
    get => NullIf(DocumentCode, "");
    set => DocumentCode = value;
  }

  /// <summary>Length of the DOCUMENT_ACTION_CODE attribute.</summary>
  public const int DocumentActionCode_MaxLength = 3;

  /// <summary>
  /// The value of the DOCUMENT_ACTION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = DocumentActionCode_MaxLength)]
  public string DocumentActionCode
  {
    get => documentActionCode ?? "";
    set => documentActionCode =
      TrimEnd(Substring(value, 1, DocumentActionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentActionCode attribute.</summary>
  [JsonPropertyName("documentActionCode")]
  [Computed]
  public string DocumentActionCode_Json
  {
    get => NullIf(DocumentActionCode, "");
    set => DocumentActionCode = value;
  }

  /// <summary>Length of the DOCUMENT_DATE attribute.</summary>
  public const int DocumentDate_MaxLength = 8;

  /// <summary>
  /// The value of the DOCUMENT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = DocumentDate_MaxLength)]
  public string DocumentDate
  {
    get => documentDate ?? "";
    set => documentDate = TrimEnd(Substring(value, 1, DocumentDate_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentDate attribute.</summary>
  [JsonPropertyName("documentDate")]
  [Computed]
  public string DocumentDate_Json
  {
    get => NullIf(DocumentDate, "");
    set => DocumentDate = value;
  }

  /// <summary>Length of the ISSUING_STATE_TRIBE_TERRITORY_NM attribute.
  /// </summary>
  public const int IssuingStateTribeTerritoryNm_MaxLength = 35;

  /// <summary>
  /// The value of the ISSUING_STATE_TRIBE_TERRITORY_NM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = IssuingStateTribeTerritoryNm_MaxLength)]
  public string IssuingStateTribeTerritoryNm
  {
    get => issuingStateTribeTerritoryNm ?? "";
    set => issuingStateTribeTerritoryNm =
      TrimEnd(Substring(value, 1, IssuingStateTribeTerritoryNm_MaxLength));
  }

  /// <summary>
  /// The json value of the IssuingStateTribeTerritoryNm attribute.</summary>
  [JsonPropertyName("issuingStateTribeTerritoryNm")]
  [Computed]
  public string IssuingStateTribeTerritoryNm_Json
  {
    get => NullIf(IssuingStateTribeTerritoryNm, "");
    set => IssuingStateTribeTerritoryNm = value;
  }

  /// <summary>Length of the ISSUING_JURISDICTION_NAME attribute.</summary>
  public const int IssuingJurisdictionName_MaxLength = 35;

  /// <summary>
  /// The value of the ISSUING_JURISDICTION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = IssuingJurisdictionName_MaxLength)]
  public string IssuingJurisdictionName
  {
    get => issuingJurisdictionName ?? "";
    set => issuingJurisdictionName =
      TrimEnd(Substring(value, 1, IssuingJurisdictionName_MaxLength));
  }

  /// <summary>
  /// The json value of the IssuingJurisdictionName attribute.</summary>
  [JsonPropertyName("issuingJurisdictionName")]
  [Computed]
  public string IssuingJurisdictionName_Json
  {
    get => NullIf(IssuingJurisdictionName, "");
    set => IssuingJurisdictionName = value;
  }

  /// <summary>Length of the CASE_IDENTIFIER attribute.</summary>
  public const int CaseIdentifier_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CaseIdentifier_MaxLength)]
  public string CaseIdentifier
  {
    get => caseIdentifier ?? "";
    set => caseIdentifier =
      TrimEnd(Substring(value, 1, CaseIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseIdentifier attribute.</summary>
  [JsonPropertyName("caseIdentifier")]
  [Computed]
  public string CaseIdentifier_Json
  {
    get => NullIf(CaseIdentifier, "");
    set => CaseIdentifier = value;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 57;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = EmployerName_MaxLength)]
  public string EmployerName
  {
    get => employerName ?? "";
    set => employerName = TrimEnd(Substring(value, 1, EmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerName attribute.</summary>
  [JsonPropertyName("employerName")]
  [Computed]
  public string EmployerName_Json
  {
    get => NullIf(EmployerName, "");
    set => EmployerName = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_1 attribute.</summary>
  public const int EmployerAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = EmployerAddressLine1_MaxLength)]
  public string EmployerAddressLine1
  {
    get => employerAddressLine1 ?? "";
    set => employerAddressLine1 =
      TrimEnd(Substring(value, 1, EmployerAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressLine1 attribute.</summary>
  [JsonPropertyName("employerAddressLine1")]
  [Computed]
  public string EmployerAddressLine1_Json
  {
    get => NullIf(EmployerAddressLine1, "");
    set => EmployerAddressLine1 = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_LINE_2 attribute.</summary>
  public const int EmployerAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = EmployerAddressLine2_MaxLength)]
  public string EmployerAddressLine2
  {
    get => employerAddressLine2 ?? "";
    set => employerAddressLine2 =
      TrimEnd(Substring(value, 1, EmployerAddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressLine2 attribute.</summary>
  [JsonPropertyName("employerAddressLine2")]
  [Computed]
  public string EmployerAddressLine2_Json
  {
    get => NullIf(EmployerAddressLine2, "");
    set => EmployerAddressLine2 = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_CITY_NAME attribute.</summary>
  public const int EmployerAddressCityName_MaxLength = 22;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_CITY_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = EmployerAddressCityName_MaxLength)]
  public string EmployerAddressCityName
  {
    get => employerAddressCityName ?? "";
    set => employerAddressCityName =
      TrimEnd(Substring(value, 1, EmployerAddressCityName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressCityName attribute.</summary>
  [JsonPropertyName("employerAddressCityName")]
  [Computed]
  public string EmployerAddressCityName_Json
  {
    get => NullIf(EmployerAddressCityName, "");
    set => EmployerAddressCityName = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_STATE_CODE attribute.</summary>
  public const int EmployerAddressStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = EmployerAddressStateCode_MaxLength)]
  public string EmployerAddressStateCode
  {
    get => employerAddressStateCode ?? "";
    set => employerAddressStateCode =
      TrimEnd(Substring(value, 1, EmployerAddressStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressStateCode attribute.</summary>
  [JsonPropertyName("employerAddressStateCode")]
  [Computed]
  public string EmployerAddressStateCode_Json
  {
    get => NullIf(EmployerAddressStateCode, "");
    set => EmployerAddressStateCode = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_ZIP_CODE attribute.</summary>
  public const int EmployerAddressZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = EmployerAddressZipCode_MaxLength)]
  public string EmployerAddressZipCode
  {
    get => employerAddressZipCode ?? "";
    set => employerAddressZipCode =
      TrimEnd(Substring(value, 1, EmployerAddressZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressZipCode attribute.</summary>
  [JsonPropertyName("employerAddressZipCode")]
  [Computed]
  public string EmployerAddressZipCode_Json
  {
    get => NullIf(EmployerAddressZipCode, "");
    set => EmployerAddressZipCode = value;
  }

  /// <summary>Length of the EMPLOYER_ADDRESS_EXT_ZIP_CODE attribute.</summary>
  public const int EmployerAddressExtZipCode_MaxLength = 4;

  /// <summary>
  /// The value of the EMPLOYER_ADDRESS_EXT_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = EmployerAddressExtZipCode_MaxLength)]
  public string EmployerAddressExtZipCode
  {
    get => employerAddressExtZipCode ?? "";
    set => employerAddressExtZipCode =
      TrimEnd(Substring(value, 1, EmployerAddressExtZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerAddressExtZipCode attribute.</summary>
  [JsonPropertyName("employerAddressExtZipCode")]
  [Computed]
  public string EmployerAddressExtZipCode_Json
  {
    get => NullIf(EmployerAddressExtZipCode, "");
    set => EmployerAddressExtZipCode = value;
  }

  /// <summary>Length of the EIN attribute.</summary>
  public const int Ein_MaxLength = 9;

  /// <summary>
  /// The value of the EIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = Ein_MaxLength)]
  public string Ein
  {
    get => ein ?? "";
    set => ein = TrimEnd(Substring(value, 1, Ein_MaxLength));
  }

  /// <summary>
  /// The json value of the Ein attribute.</summary>
  [JsonPropertyName("ein")]
  [Computed]
  public string Ein_Json
  {
    get => NullIf(Ein, "");
    set => Ein = value;
  }

  /// <summary>Length of the EMPLOYEE_LAST_NAME attribute.</summary>
  public const int EmployeeLastName_MaxLength = 20;

  /// <summary>
  /// The value of the EMPLOYEE_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = EmployeeLastName_MaxLength)]
  public string EmployeeLastName
  {
    get => employeeLastName ?? "";
    set => employeeLastName =
      TrimEnd(Substring(value, 1, EmployeeLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeLastName attribute.</summary>
  [JsonPropertyName("employeeLastName")]
  [Computed]
  public string EmployeeLastName_Json
  {
    get => NullIf(EmployeeLastName, "");
    set => EmployeeLastName = value;
  }

  /// <summary>Length of the EMPLOYEE_FIRST_NAME attribute.</summary>
  public const int EmployeeFirstName_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYEE_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = EmployeeFirstName_MaxLength)]
  public string EmployeeFirstName
  {
    get => employeeFirstName ?? "";
    set => employeeFirstName =
      TrimEnd(Substring(value, 1, EmployeeFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeFirstName attribute.</summary>
  [JsonPropertyName("employeeFirstName")]
  [Computed]
  public string EmployeeFirstName_Json
  {
    get => NullIf(EmployeeFirstName, "");
    set => EmployeeFirstName = value;
  }

  /// <summary>Length of the EMPLOYEE_MIDDLE_NAME attribute.</summary>
  public const int EmployeeMiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the EMPLOYEE_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = EmployeeMiddleName_MaxLength)]
  public string EmployeeMiddleName
  {
    get => employeeMiddleName ?? "";
    set => employeeMiddleName =
      TrimEnd(Substring(value, 1, EmployeeMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeMiddleName attribute.</summary>
  [JsonPropertyName("employeeMiddleName")]
  [Computed]
  public string EmployeeMiddleName_Json
  {
    get => NullIf(EmployeeMiddleName, "");
    set => EmployeeMiddleName = value;
  }

  /// <summary>Length of the EMPLOYEE_SUFFIX attribute.</summary>
  public const int EmployeeSuffix_MaxLength = 4;

  /// <summary>
  /// The value of the EMPLOYEE_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = EmployeeSuffix_MaxLength)]
    
  public string EmployeeSuffix
  {
    get => employeeSuffix ?? "";
    set => employeeSuffix =
      TrimEnd(Substring(value, 1, EmployeeSuffix_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeSuffix attribute.</summary>
  [JsonPropertyName("employeeSuffix")]
  [Computed]
  public string EmployeeSuffix_Json
  {
    get => NullIf(EmployeeSuffix, "");
    set => EmployeeSuffix = value;
  }

  /// <summary>Length of the EMPLOYEE_SSN attribute.</summary>
  public const int EmployeeSsn_MaxLength = 9;

  /// <summary>
  /// The value of the EMPLOYEE_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = EmployeeSsn_MaxLength)]
  public string EmployeeSsn
  {
    get => employeeSsn ?? "";
    set => employeeSsn = TrimEnd(Substring(value, 1, EmployeeSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeSsn attribute.</summary>
  [JsonPropertyName("employeeSsn")]
  [Computed]
  public string EmployeeSsn_Json
  {
    get => NullIf(EmployeeSsn, "");
    set => EmployeeSsn = value;
  }

  /// <summary>Length of the EMPLOYEE_BIRTH_DATE attribute.</summary>
  public const int EmployeeBirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the EMPLOYEE_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = EmployeeBirthDate_MaxLength)]
  public string EmployeeBirthDate
  {
    get => employeeBirthDate ?? "";
    set => employeeBirthDate =
      TrimEnd(Substring(value, 1, EmployeeBirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeBirthDate attribute.</summary>
  [JsonPropertyName("employeeBirthDate")]
  [Computed]
  public string EmployeeBirthDate_Json
  {
    get => NullIf(EmployeeBirthDate, "");
    set => EmployeeBirthDate = value;
  }

  /// <summary>Length of the OBLIGEE_LAST_NAME attribute.</summary>
  public const int ObligeeLastName_MaxLength = 57;

  /// <summary>
  /// The value of the OBLIGEE_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = ObligeeLastName_MaxLength)
    ]
  public string ObligeeLastName
  {
    get => obligeeLastName ?? "";
    set => obligeeLastName =
      TrimEnd(Substring(value, 1, ObligeeLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligeeLastName attribute.</summary>
  [JsonPropertyName("obligeeLastName")]
  [Computed]
  public string ObligeeLastName_Json
  {
    get => NullIf(ObligeeLastName, "");
    set => ObligeeLastName = value;
  }

  /// <summary>Length of the OBLIGEE_FIRST_NAME attribute.</summary>
  public const int ObligeeFirstName_MaxLength = 15;

  /// <summary>
  /// The value of the OBLIGEE_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ObligeeFirstName_MaxLength)]
  public string ObligeeFirstName
  {
    get => obligeeFirstName ?? "";
    set => obligeeFirstName =
      TrimEnd(Substring(value, 1, ObligeeFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligeeFirstName attribute.</summary>
  [JsonPropertyName("obligeeFirstName")]
  [Computed]
  public string ObligeeFirstName_Json
  {
    get => NullIf(ObligeeFirstName, "");
    set => ObligeeFirstName = value;
  }

  /// <summary>Length of the OBLIGEE_MIDDLE_NAME attribute.</summary>
  public const int ObligeeMiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the OBLIGEE_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = ObligeeMiddleName_MaxLength)]
  public string ObligeeMiddleName
  {
    get => obligeeMiddleName ?? "";
    set => obligeeMiddleName =
      TrimEnd(Substring(value, 1, ObligeeMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligeeMiddleName attribute.</summary>
  [JsonPropertyName("obligeeMiddleName")]
  [Computed]
  public string ObligeeMiddleName_Json
  {
    get => NullIf(ObligeeMiddleName, "");
    set => ObligeeMiddleName = value;
  }

  /// <summary>Length of the OBLIGEE_NAME_SUFFIX attribute.</summary>
  public const int ObligeeNameSuffix_MaxLength = 4;

  /// <summary>
  /// The value of the OBLIGEE_NAME_SUFFIX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = ObligeeNameSuffix_MaxLength)]
  public string ObligeeNameSuffix
  {
    get => obligeeNameSuffix ?? "";
    set => obligeeNameSuffix =
      TrimEnd(Substring(value, 1, ObligeeNameSuffix_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligeeNameSuffix attribute.</summary>
  [JsonPropertyName("obligeeNameSuffix")]
  [Computed]
  public string ObligeeNameSuffix_Json
  {
    get => NullIf(ObligeeNameSuffix, "");
    set => ObligeeNameSuffix = value;
  }

  /// <summary>Length of the ISSUING_TRIBUNAL_NAME attribute.</summary>
  public const int IssuingTribunalName_MaxLength = 35;

  /// <summary>
  /// The value of the ISSUING_TRIBUNAL_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = IssuingTribunalName_MaxLength)]
  public string IssuingTribunalName
  {
    get => issuingTribunalName ?? "";
    set => issuingTribunalName =
      TrimEnd(Substring(value, 1, IssuingTribunalName_MaxLength));
  }

  /// <summary>
  /// The json value of the IssuingTribunalName attribute.</summary>
  [JsonPropertyName("issuingTribunalName")]
  [Computed]
  public string IssuingTribunalName_Json
  {
    get => NullIf(IssuingTribunalName, "");
    set => IssuingTribunalName = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_CHILD_AMOUNT attribute.</summary>
  public const int SupportCurrentChildAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_CHILD_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = SupportCurrentChildAmount_MaxLength)]
  public string SupportCurrentChildAmount
  {
    get => supportCurrentChildAmount ?? "";
    set => supportCurrentChildAmount =
      TrimEnd(Substring(value, 1, SupportCurrentChildAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentChildAmount attribute.</summary>
  [JsonPropertyName("supportCurrentChildAmount")]
  [Computed]
  public string SupportCurrentChildAmount_Json
  {
    get => NullIf(SupportCurrentChildAmount, "");
    set => SupportCurrentChildAmount = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_CHILD_FREQUENCY attribute.
  /// </summary>
  public const int SupportCurrentChildFrequency_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_CHILD_FREQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = SupportCurrentChildFrequency_MaxLength)]
  public string SupportCurrentChildFrequency
  {
    get => supportCurrentChildFrequency ?? "";
    set => supportCurrentChildFrequency =
      TrimEnd(Substring(value, 1, SupportCurrentChildFrequency_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentChildFrequency attribute.</summary>
  [JsonPropertyName("supportCurrentChildFrequency")]
  [Computed]
  public string SupportCurrentChildFrequency_Json
  {
    get => NullIf(SupportCurrentChildFrequency, "");
    set => SupportCurrentChildFrequency = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_CHILD_AMOUNT attribute.</summary>
  public const int SupportPastDueChildAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_CHILD_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = SupportPastDueChildAmount_MaxLength)]
  public string SupportPastDueChildAmount
  {
    get => supportPastDueChildAmount ?? "";
    set => supportPastDueChildAmount =
      TrimEnd(Substring(value, 1, SupportPastDueChildAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueChildAmount attribute.</summary>
  [JsonPropertyName("supportPastDueChildAmount")]
  [Computed]
  public string SupportPastDueChildAmount_Json
  {
    get => NullIf(SupportPastDueChildAmount, "");
    set => SupportPastDueChildAmount = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_CHILD_FREQUENCY attribute.
  /// </summary>
  public const int SupportPastDueChildFrequency_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_CHILD_FREQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = SupportPastDueChildFrequency_MaxLength)]
  public string SupportPastDueChildFrequency
  {
    get => supportPastDueChildFrequency ?? "";
    set => supportPastDueChildFrequency =
      TrimEnd(Substring(value, 1, SupportPastDueChildFrequency_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueChildFrequency attribute.</summary>
  [JsonPropertyName("supportPastDueChildFrequency")]
  [Computed]
  public string SupportPastDueChildFrequency_Json
  {
    get => NullIf(SupportPastDueChildFrequency, "");
    set => SupportPastDueChildFrequency = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_MEDICAL_AMOUNT attribute.</summary>
  public const int SupportCurrentMedicalAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_MEDICAL_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = SupportCurrentMedicalAmount_MaxLength)]
  public string SupportCurrentMedicalAmount
  {
    get => supportCurrentMedicalAmount ?? "";
    set => supportCurrentMedicalAmount =
      TrimEnd(Substring(value, 1, SupportCurrentMedicalAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentMedicalAmount attribute.</summary>
  [JsonPropertyName("supportCurrentMedicalAmount")]
  [Computed]
  public string SupportCurrentMedicalAmount_Json
  {
    get => NullIf(SupportCurrentMedicalAmount, "");
    set => SupportCurrentMedicalAmount = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_MEDICAL_FREQUENC attribute.
  /// </summary>
  public const int SupportCurrentMedicalFrequenc_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_MEDICAL_FREQUENC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = SupportCurrentMedicalFrequenc_MaxLength)]
  public string SupportCurrentMedicalFrequenc
  {
    get => supportCurrentMedicalFrequenc ?? "";
    set => supportCurrentMedicalFrequenc =
      TrimEnd(Substring(value, 1, SupportCurrentMedicalFrequenc_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentMedicalFrequenc attribute.</summary>
  [JsonPropertyName("supportCurrentMedicalFrequenc")]
  [Computed]
  public string SupportCurrentMedicalFrequenc_Json
  {
    get => NullIf(SupportCurrentMedicalFrequenc, "");
    set => SupportCurrentMedicalFrequenc = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_MEDICAL_AMOUNT attribute.
  /// </summary>
  public const int SupportPastDueMedicalAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_MEDICAL_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = SupportPastDueMedicalAmount_MaxLength)]
  public string SupportPastDueMedicalAmount
  {
    get => supportPastDueMedicalAmount ?? "";
    set => supportPastDueMedicalAmount =
      TrimEnd(Substring(value, 1, SupportPastDueMedicalAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueMedicalAmount attribute.</summary>
  [JsonPropertyName("supportPastDueMedicalAmount")]
  [Computed]
  public string SupportPastDueMedicalAmount_Json
  {
    get => NullIf(SupportPastDueMedicalAmount, "");
    set => SupportPastDueMedicalAmount = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_MEDICAL_FREQUEN attribute.
  /// </summary>
  public const int SupportPastDueMedicalFrequen_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_MEDICAL_FREQUEN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = SupportPastDueMedicalFrequen_MaxLength)]
  public string SupportPastDueMedicalFrequen
  {
    get => supportPastDueMedicalFrequen ?? "";
    set => supportPastDueMedicalFrequen =
      TrimEnd(Substring(value, 1, SupportPastDueMedicalFrequen_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueMedicalFrequen attribute.</summary>
  [JsonPropertyName("supportPastDueMedicalFrequen")]
  [Computed]
  public string SupportPastDueMedicalFrequen_Json
  {
    get => NullIf(SupportPastDueMedicalFrequen, "");
    set => SupportPastDueMedicalFrequen = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_SPOUSAL_AMOUNT attribute.</summary>
  public const int SupportCurrentSpousalAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_SPOUSAL_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = SupportCurrentSpousalAmount_MaxLength)]
  public string SupportCurrentSpousalAmount
  {
    get => supportCurrentSpousalAmount ?? "";
    set => supportCurrentSpousalAmount =
      TrimEnd(Substring(value, 1, SupportCurrentSpousalAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentSpousalAmount attribute.</summary>
  [JsonPropertyName("supportCurrentSpousalAmount")]
  [Computed]
  public string SupportCurrentSpousalAmount_Json
  {
    get => NullIf(SupportCurrentSpousalAmount, "");
    set => SupportCurrentSpousalAmount = value;
  }

  /// <summary>Length of the SUPPORT_CURRENT_SPOUSAL_FREQUENC attribute.
  /// </summary>
  public const int SupportCurrentSpousalFrequenc_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_CURRENT_SPOUSAL_FREQUENC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = SupportCurrentSpousalFrequenc_MaxLength)]
  public string SupportCurrentSpousalFrequenc
  {
    get => supportCurrentSpousalFrequenc ?? "";
    set => supportCurrentSpousalFrequenc =
      TrimEnd(Substring(value, 1, SupportCurrentSpousalFrequenc_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportCurrentSpousalFrequenc attribute.</summary>
  [JsonPropertyName("supportCurrentSpousalFrequenc")]
  [Computed]
  public string SupportCurrentSpousalFrequenc_Json
  {
    get => NullIf(SupportCurrentSpousalFrequenc, "");
    set => SupportCurrentSpousalFrequenc = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_SPOUSAL_AMOUNT attribute.
  /// </summary>
  public const int SupportPastDueSpousalAmount_MaxLength = 11;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_SPOUSAL_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = SupportPastDueSpousalAmount_MaxLength)]
  public string SupportPastDueSpousalAmount
  {
    get => supportPastDueSpousalAmount ?? "";
    set => supportPastDueSpousalAmount =
      TrimEnd(Substring(value, 1, SupportPastDueSpousalAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueSpousalAmount attribute.</summary>
  [JsonPropertyName("supportPastDueSpousalAmount")]
  [Computed]
  public string SupportPastDueSpousalAmount_Json
  {
    get => NullIf(SupportPastDueSpousalAmount, "");
    set => SupportPastDueSpousalAmount = value;
  }

  /// <summary>Length of the SUPPORT_PAST_DUE_SPOUSAL_FREQUEN attribute.
  /// </summary>
  public const int SupportPastDueSpousalFrequen_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORT_PAST_DUE_SPOUSAL_FREQUEN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = SupportPastDueSpousalFrequen_MaxLength)]
  public string SupportPastDueSpousalFrequen
  {
    get => supportPastDueSpousalFrequen ?? "";
    set => supportPastDueSpousalFrequen =
      TrimEnd(Substring(value, 1, SupportPastDueSpousalFrequen_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportPastDueSpousalFrequen attribute.</summary>
  [JsonPropertyName("supportPastDueSpousalFrequen")]
  [Computed]
  public string SupportPastDueSpousalFrequen_Json
  {
    get => NullIf(SupportPastDueSpousalFrequen, "");
    set => SupportPastDueSpousalFrequen = value;
  }

  /// <summary>Length of the OBLIGATION_OTHER_AMOUNT attribute.</summary>
  public const int ObligationOtherAmount_MaxLength = 11;

  /// <summary>
  /// The value of the OBLIGATION_OTHER_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = ObligationOtherAmount_MaxLength)]
  public string ObligationOtherAmount
  {
    get => obligationOtherAmount ?? "";
    set => obligationOtherAmount =
      TrimEnd(Substring(value, 1, ObligationOtherAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationOtherAmount attribute.</summary>
  [JsonPropertyName("obligationOtherAmount")]
  [Computed]
  public string ObligationOtherAmount_Json
  {
    get => NullIf(ObligationOtherAmount, "");
    set => ObligationOtherAmount = value;
  }

  /// <summary>Length of the OBLIGATION_OTHER_FREQUENCY_CODE attribute.
  /// </summary>
  public const int ObligationOtherFrequencyCode_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGATION_OTHER_FREQUENCY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = ObligationOtherFrequencyCode_MaxLength)]
  public string ObligationOtherFrequencyCode
  {
    get => obligationOtherFrequencyCode ?? "";
    set => obligationOtherFrequencyCode =
      TrimEnd(Substring(value, 1, ObligationOtherFrequencyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationOtherFrequencyCode attribute.</summary>
  [JsonPropertyName("obligationOtherFrequencyCode")]
  [Computed]
  public string ObligationOtherFrequencyCode_Json
  {
    get => NullIf(ObligationOtherFrequencyCode, "");
    set => ObligationOtherFrequencyCode = value;
  }

  /// <summary>Length of the OBLIGATION_OTHER_DESCRIPTION attribute.</summary>
  public const int ObligationOtherDescription_MaxLength = 35;

  /// <summary>
  /// The value of the OBLIGATION_OTHER_DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = ObligationOtherDescription_MaxLength)]
  public string ObligationOtherDescription
  {
    get => obligationOtherDescription ?? "";
    set => obligationOtherDescription =
      TrimEnd(Substring(value, 1, ObligationOtherDescription_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationOtherDescription attribute.</summary>
  [JsonPropertyName("obligationOtherDescription")]
  [Computed]
  public string ObligationOtherDescription_Json
  {
    get => NullIf(ObligationOtherDescription, "");
    set => ObligationOtherDescription = value;
  }

  /// <summary>Length of the OBLIGATION_TOTAL_AMOUNT attribute.</summary>
  public const int ObligationTotalAmount_MaxLength = 11;

  /// <summary>
  /// The value of the OBLIGATION_TOTAL_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = ObligationTotalAmount_MaxLength)]
  public string ObligationTotalAmount
  {
    get => obligationTotalAmount ?? "";
    set => obligationTotalAmount =
      TrimEnd(Substring(value, 1, ObligationTotalAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationTotalAmount attribute.</summary>
  [JsonPropertyName("obligationTotalAmount")]
  [Computed]
  public string ObligationTotalAmount_Json
  {
    get => NullIf(ObligationTotalAmount, "");
    set => ObligationTotalAmount = value;
  }

  /// <summary>Length of the OBLIGATION_TOTAL_FREQUENCY attribute.</summary>
  public const int ObligationTotalFrequency_MaxLength = 1;

  /// <summary>
  /// The value of the OBLIGATION_TOTAL_FREQUENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = ObligationTotalFrequency_MaxLength)]
  public string ObligationTotalFrequency
  {
    get => obligationTotalFrequency ?? "";
    set => obligationTotalFrequency =
      TrimEnd(Substring(value, 1, ObligationTotalFrequency_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationTotalFrequency attribute.</summary>
  [JsonPropertyName("obligationTotalFrequency")]
  [Computed]
  public string ObligationTotalFrequency_Json
  {
    get => NullIf(ObligationTotalFrequency, "");
    set => ObligationTotalFrequency = value;
  }

  /// <summary>Length of the ARREARS_12WK_OVERDUE_CODE attribute.</summary>
  public const int Arrears12WkOverdueCode_MaxLength = 1;

  /// <summary>
  /// The value of the ARREARS_12WK_OVERDUE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = Arrears12WkOverdueCode_MaxLength)]
  public string Arrears12WkOverdueCode
  {
    get => arrears12WkOverdueCode ?? "";
    set => arrears12WkOverdueCode =
      TrimEnd(Substring(value, 1, Arrears12WkOverdueCode_MaxLength));
  }

  /// <summary>
  /// The json value of the Arrears12WkOverdueCode attribute.</summary>
  [JsonPropertyName("arrears12WkOverdueCode")]
  [Computed]
  public string Arrears12WkOverdueCode_Json
  {
    get => NullIf(Arrears12WkOverdueCode, "");
    set => Arrears12WkOverdueCode = value;
  }

  /// <summary>Length of the IWO_DEDUCTION_WEEKLY_AMOUNT attribute.</summary>
  public const int IwoDeductionWeeklyAmount_MaxLength = 11;

  /// <summary>
  /// The value of the IWO_DEDUCTION_WEEKLY_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = IwoDeductionWeeklyAmount_MaxLength)]
  public string IwoDeductionWeeklyAmount
  {
    get => iwoDeductionWeeklyAmount ?? "";
    set => iwoDeductionWeeklyAmount =
      TrimEnd(Substring(value, 1, IwoDeductionWeeklyAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the IwoDeductionWeeklyAmount attribute.</summary>
  [JsonPropertyName("iwoDeductionWeeklyAmount")]
  [Computed]
  public string IwoDeductionWeeklyAmount_Json
  {
    get => NullIf(IwoDeductionWeeklyAmount, "");
    set => IwoDeductionWeeklyAmount = value;
  }

  /// <summary>Length of the IWO_DEDUCTION_BIWEEKLY_AMOUNT attribute.</summary>
  public const int IwoDeductionBiweeklyAmount_MaxLength = 11;

  /// <summary>
  /// The value of the IWO_DEDUCTION_BIWEEKLY_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = IwoDeductionBiweeklyAmount_MaxLength)]
  public string IwoDeductionBiweeklyAmount
  {
    get => iwoDeductionBiweeklyAmount ?? "";
    set => iwoDeductionBiweeklyAmount =
      TrimEnd(Substring(value, 1, IwoDeductionBiweeklyAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the IwoDeductionBiweeklyAmount attribute.</summary>
  [JsonPropertyName("iwoDeductionBiweeklyAmount")]
  [Computed]
  public string IwoDeductionBiweeklyAmount_Json
  {
    get => NullIf(IwoDeductionBiweeklyAmount, "");
    set => IwoDeductionBiweeklyAmount = value;
  }

  /// <summary>Length of the IWO_DEDUCTION_SEMIMONTHLY_AMOUNT attribute.
  /// </summary>
  public const int IwoDeductionSemimonthlyAmount_MaxLength = 11;

  /// <summary>
  /// The value of the IWO_DEDUCTION_SEMIMONTHLY_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = IwoDeductionSemimonthlyAmount_MaxLength)]
  public string IwoDeductionSemimonthlyAmount
  {
    get => iwoDeductionSemimonthlyAmount ?? "";
    set => iwoDeductionSemimonthlyAmount =
      TrimEnd(Substring(value, 1, IwoDeductionSemimonthlyAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the IwoDeductionSemimonthlyAmount attribute.</summary>
  [JsonPropertyName("iwoDeductionSemimonthlyAmount")]
  [Computed]
  public string IwoDeductionSemimonthlyAmount_Json
  {
    get => NullIf(IwoDeductionSemimonthlyAmount, "");
    set => IwoDeductionSemimonthlyAmount = value;
  }

  /// <summary>Length of the IWO_DEDUCTION_MONTHLY_AMOUNT attribute.</summary>
  public const int IwoDeductionMonthlyAmount_MaxLength = 11;

  /// <summary>
  /// The value of the IWO_DEDUCTION_MONTHLY_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = IwoDeductionMonthlyAmount_MaxLength)]
  public string IwoDeductionMonthlyAmount
  {
    get => iwoDeductionMonthlyAmount ?? "";
    set => iwoDeductionMonthlyAmount =
      TrimEnd(Substring(value, 1, IwoDeductionMonthlyAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the IwoDeductionMonthlyAmount attribute.</summary>
  [JsonPropertyName("iwoDeductionMonthlyAmount")]
  [Computed]
  public string IwoDeductionMonthlyAmount_Json
  {
    get => NullIf(IwoDeductionMonthlyAmount, "");
    set => IwoDeductionMonthlyAmount = value;
  }

  /// <summary>Length of the STATE_TRIBE_TERRITORY_NAME attribute.</summary>
  public const int StateTribeTerritoryName_MaxLength = 35;

  /// <summary>
  /// The value of the STATE_TRIBE_TERRITORY_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = StateTribeTerritoryName_MaxLength)]
  public string StateTribeTerritoryName
  {
    get => stateTribeTerritoryName ?? "";
    set => stateTribeTerritoryName =
      TrimEnd(Substring(value, 1, StateTribeTerritoryName_MaxLength));
  }

  /// <summary>
  /// The json value of the StateTribeTerritoryName attribute.</summary>
  [JsonPropertyName("stateTribeTerritoryName")]
  [Computed]
  public string StateTribeTerritoryName_Json
  {
    get => NullIf(StateTribeTerritoryName, "");
    set => StateTribeTerritoryName = value;
  }

  /// <summary>Length of the BEGIN_WITHHOLDING_WITHIN_DAYS attribute.</summary>
  public const int BeginWithholdingWithinDays_MaxLength = 2;

  /// <summary>
  /// The value of the BEGIN_WITHHOLDING_WITHIN_DAYS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = BeginWithholdingWithinDays_MaxLength)]
  public string BeginWithholdingWithinDays
  {
    get => beginWithholdingWithinDays ?? "";
    set => beginWithholdingWithinDays =
      TrimEnd(Substring(value, 1, BeginWithholdingWithinDays_MaxLength));
  }

  /// <summary>
  /// The json value of the BeginWithholdingWithinDays attribute.</summary>
  [JsonPropertyName("beginWithholdingWithinDays")]
  [Computed]
  public string BeginWithholdingWithinDays_Json
  {
    get => NullIf(BeginWithholdingWithinDays, "");
    set => BeginWithholdingWithinDays = value;
  }

  /// <summary>Length of the INCOME_WITHHOLDING_START_DATE attribute.</summary>
  public const int IncomeWithholdingStartDate_MaxLength = 8;

  /// <summary>
  /// The value of the INCOME_WITHHOLDING_START_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = IncomeWithholdingStartDate_MaxLength)]
  public string IncomeWithholdingStartDate
  {
    get => incomeWithholdingStartDate ?? "";
    set => incomeWithholdingStartDate =
      TrimEnd(Substring(value, 1, IncomeWithholdingStartDate_MaxLength));
  }

  /// <summary>
  /// The json value of the IncomeWithholdingStartDate attribute.</summary>
  [JsonPropertyName("incomeWithholdingStartDate")]
  [Computed]
  public string IncomeWithholdingStartDate_Json
  {
    get => NullIf(IncomeWithholdingStartDate, "");
    set => IncomeWithholdingStartDate = value;
  }

  /// <summary>Length of the SEND_PAYMENT_WITHHIN_DAYS attribute.</summary>
  public const int SendPaymentWithhinDays_MaxLength = 2;

  /// <summary>
  /// The value of the SEND_PAYMENT_WITHHIN_DAYS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = SendPaymentWithhinDays_MaxLength)]
  public string SendPaymentWithhinDays
  {
    get => sendPaymentWithhinDays ?? "";
    set => sendPaymentWithhinDays =
      TrimEnd(Substring(value, 1, SendPaymentWithhinDays_MaxLength));
  }

  /// <summary>
  /// The json value of the SendPaymentWithhinDays attribute.</summary>
  [JsonPropertyName("sendPaymentWithhinDays")]
  [Computed]
  public string SendPaymentWithhinDays_Json
  {
    get => NullIf(SendPaymentWithhinDays, "");
    set => SendPaymentWithhinDays = value;
  }

  /// <summary>Length of the IWO_CCPA_PERCENT_RATE attribute.</summary>
  public const int IwoCcpaPercentRate_MaxLength = 2;

  /// <summary>
  /// The value of the IWO_CCPA_PERCENT_RATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = IwoCcpaPercentRate_MaxLength)]
  public string IwoCcpaPercentRate
  {
    get => iwoCcpaPercentRate ?? "";
    set => iwoCcpaPercentRate =
      TrimEnd(Substring(value, 1, IwoCcpaPercentRate_MaxLength));
  }

  /// <summary>
  /// The json value of the IwoCcpaPercentRate attribute.</summary>
  [JsonPropertyName("iwoCcpaPercentRate")]
  [Computed]
  public string IwoCcpaPercentRate_Json
  {
    get => NullIf(IwoCcpaPercentRate, "");
    set => IwoCcpaPercentRate = value;
  }

  /// <summary>Length of the PAYEE_NAME attribute.</summary>
  public const int PayeeName_MaxLength = 57;

  /// <summary>
  /// The value of the PAYEE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length = PayeeName_MaxLength)]
  public string PayeeName
  {
    get => payeeName ?? "";
    set => payeeName = TrimEnd(Substring(value, 1, PayeeName_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeName attribute.</summary>
  [JsonPropertyName("payeeName")]
  [Computed]
  public string PayeeName_Json
  {
    get => NullIf(PayeeName, "");
    set => PayeeName = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_LINE_1 attribute.</summary>
  public const int PayeeAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = PayeeAddressLine1_MaxLength)]
  public string PayeeAddressLine1
  {
    get => payeeAddressLine1 ?? "";
    set => payeeAddressLine1 =
      TrimEnd(Substring(value, 1, PayeeAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressLine1 attribute.</summary>
  [JsonPropertyName("payeeAddressLine1")]
  [Computed]
  public string PayeeAddressLine1_Json
  {
    get => NullIf(PayeeAddressLine1, "");
    set => PayeeAddressLine1 = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_LINE_2 attribute.</summary>
  public const int PayeeAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = PayeeAddressLine2_MaxLength)]
  public string PayeeAddressLine2
  {
    get => payeeAddressLine2 ?? "";
    set => payeeAddressLine2 =
      TrimEnd(Substring(value, 1, PayeeAddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressLine2 attribute.</summary>
  [JsonPropertyName("payeeAddressLine2")]
  [Computed]
  public string PayeeAddressLine2_Json
  {
    get => NullIf(PayeeAddressLine2, "");
    set => PayeeAddressLine2 = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_CITY attribute.</summary>
  public const int PayeeAddressCity_MaxLength = 22;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = PayeeAddressCity_MaxLength)]
  public string PayeeAddressCity
  {
    get => payeeAddressCity ?? "";
    set => payeeAddressCity =
      TrimEnd(Substring(value, 1, PayeeAddressCity_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressCity attribute.</summary>
  [JsonPropertyName("payeeAddressCity")]
  [Computed]
  public string PayeeAddressCity_Json
  {
    get => NullIf(PayeeAddressCity, "");
    set => PayeeAddressCity = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_STATE_CODE attribute.</summary>
  public const int PayeeAddressStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = PayeeAddressStateCode_MaxLength)]
  public string PayeeAddressStateCode
  {
    get => payeeAddressStateCode ?? "";
    set => payeeAddressStateCode =
      TrimEnd(Substring(value, 1, PayeeAddressStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressStateCode attribute.</summary>
  [JsonPropertyName("payeeAddressStateCode")]
  [Computed]
  public string PayeeAddressStateCode_Json
  {
    get => NullIf(PayeeAddressStateCode, "");
    set => PayeeAddressStateCode = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_ZIP_CODE attribute.</summary>
  public const int PayeeAddressZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = PayeeAddressZipCode_MaxLength)]
  public string PayeeAddressZipCode
  {
    get => payeeAddressZipCode ?? "";
    set => payeeAddressZipCode =
      TrimEnd(Substring(value, 1, PayeeAddressZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressZipCode attribute.</summary>
  [JsonPropertyName("payeeAddressZipCode")]
  [Computed]
  public string PayeeAddressZipCode_Json
  {
    get => NullIf(PayeeAddressZipCode, "");
    set => PayeeAddressZipCode = value;
  }

  /// <summary>Length of the PAYEE_ADDRESS_EXT_ZIP_CODE attribute.</summary>
  public const int PayeeAddressExtZipCode_MaxLength = 4;

  /// <summary>
  /// The value of the PAYEE_ADDRESS_EXT_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = PayeeAddressExtZipCode_MaxLength)]
  public string PayeeAddressExtZipCode
  {
    get => payeeAddressExtZipCode ?? "";
    set => payeeAddressExtZipCode =
      TrimEnd(Substring(value, 1, PayeeAddressExtZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeAddressExtZipCode attribute.</summary>
  [JsonPropertyName("payeeAddressExtZipCode")]
  [Computed]
  public string PayeeAddressExtZipCode_Json
  {
    get => NullIf(PayeeAddressExtZipCode, "");
    set => PayeeAddressExtZipCode = value;
  }

  /// <summary>Length of the PAYEE_REMITTANCE_FIPS_CODE attribute.</summary>
  public const int PayeeRemittanceFipsCode_MaxLength = 7;

  /// <summary>
  /// The value of the PAYEE_REMITTANCE_FIPS_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = PayeeRemittanceFipsCode_MaxLength)]
  public string PayeeRemittanceFipsCode
  {
    get => payeeRemittanceFipsCode ?? "";
    set => payeeRemittanceFipsCode =
      TrimEnd(Substring(value, 1, PayeeRemittanceFipsCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PayeeRemittanceFipsCode attribute.</summary>
  [JsonPropertyName("payeeRemittanceFipsCode")]
  [Computed]
  public string PayeeRemittanceFipsCode_Json
  {
    get => NullIf(PayeeRemittanceFipsCode, "");
    set => PayeeRemittanceFipsCode = value;
  }

  /// <summary>Length of the GOVERNMENT_OFFICIAL_NAME attribute.</summary>
  public const int GovernmentOfficialName_MaxLength = 70;

  /// <summary>
  /// The value of the GOVERNMENT_OFFICIAL_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = GovernmentOfficialName_MaxLength)]
  public string GovernmentOfficialName
  {
    get => governmentOfficialName ?? "";
    set => governmentOfficialName =
      TrimEnd(Substring(value, 1, GovernmentOfficialName_MaxLength));
  }

  /// <summary>
  /// The json value of the GovernmentOfficialName attribute.</summary>
  [JsonPropertyName("governmentOfficialName")]
  [Computed]
  public string GovernmentOfficialName_Json
  {
    get => NullIf(GovernmentOfficialName, "");
    set => GovernmentOfficialName = value;
  }

  /// <summary>Length of the ISSUING_OFFICIAL_TITLE attribute.</summary>
  public const int IssuingOfficialTitle_MaxLength = 50;

  /// <summary>
  /// The value of the ISSUING_OFFICIAL_TITLE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = IssuingOfficialTitle_MaxLength)]
  public string IssuingOfficialTitle
  {
    get => issuingOfficialTitle ?? "";
    set => issuingOfficialTitle =
      TrimEnd(Substring(value, 1, IssuingOfficialTitle_MaxLength));
  }

  /// <summary>
  /// The json value of the IssuingOfficialTitle attribute.</summary>
  [JsonPropertyName("issuingOfficialTitle")]
  [Computed]
  public string IssuingOfficialTitle_Json
  {
    get => NullIf(IssuingOfficialTitle, "");
    set => IssuingOfficialTitle = value;
  }

  /// <summary>Length of the SEND_EMPLOYEE_COPY_INDICATOR attribute.</summary>
  public const int SendEmployeeCopyIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the SEND_EMPLOYEE_COPY_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = SendEmployeeCopyIndicator_MaxLength)]
  public string SendEmployeeCopyIndicator
  {
    get => sendEmployeeCopyIndicator ?? "";
    set => sendEmployeeCopyIndicator =
      TrimEnd(Substring(value, 1, SendEmployeeCopyIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the SendEmployeeCopyIndicator attribute.</summary>
  [JsonPropertyName("sendEmployeeCopyIndicator")]
  [Computed]
  public string SendEmployeeCopyIndicator_Json
  {
    get => NullIf(SendEmployeeCopyIndicator, "");
    set => SendEmployeeCopyIndicator = value;
  }

  /// <summary>Length of the PENALTY_LIABILITY_INFO_TEXT attribute.</summary>
  public const int PenaltyLiabilityInfoText_MaxLength = 160;

  /// <summary>
  /// The value of the PENALTY_LIABILITY_INFO_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = PenaltyLiabilityInfoText_MaxLength)]
  public string PenaltyLiabilityInfoText
  {
    get => penaltyLiabilityInfoText ?? "";
    set => penaltyLiabilityInfoText =
      TrimEnd(Substring(value, 1, PenaltyLiabilityInfoText_MaxLength));
  }

  /// <summary>
  /// The json value of the PenaltyLiabilityInfoText attribute.</summary>
  [JsonPropertyName("penaltyLiabilityInfoText")]
  [Computed]
  public string PenaltyLiabilityInfoText_Json
  {
    get => NullIf(PenaltyLiabilityInfoText, "");
    set => PenaltyLiabilityInfoText = value;
  }

  /// <summary>Length of the ANTIDISCRIMINATION_PROVISION_TXT attribute.
  /// </summary>
  public const int AntidiscriminationProvisionTxt_MaxLength = 160;

  /// <summary>
  /// The value of the ANTIDISCRIMINATION_PROVISION_TXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = AntidiscriminationProvisionTxt_MaxLength)]
  public string AntidiscriminationProvisionTxt
  {
    get => antidiscriminationProvisionTxt ?? "";
    set => antidiscriminationProvisionTxt =
      TrimEnd(Substring(value, 1, AntidiscriminationProvisionTxt_MaxLength));
  }

  /// <summary>
  /// The json value of the AntidiscriminationProvisionTxt attribute.</summary>
  [JsonPropertyName("antidiscriminationProvisionTxt")]
  [Computed]
  public string AntidiscriminationProvisionTxt_Json
  {
    get => NullIf(AntidiscriminationProvisionTxt, "");
    set => AntidiscriminationProvisionTxt = value;
  }

  /// <summary>Length of the SPECIFIC_PAYEE_WITHHOLDING_LIMIT attribute.
  /// </summary>
  public const int SpecificPayeeWithholdingLimit_MaxLength = 160;

  /// <summary>
  /// The value of the SPECIFIC_PAYEE_WITHHOLDING_LIMIT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = SpecificPayeeWithholdingLimit_MaxLength)]
  public string SpecificPayeeWithholdingLimit
  {
    get => specificPayeeWithholdingLimit ?? "";
    set => specificPayeeWithholdingLimit =
      TrimEnd(Substring(value, 1, SpecificPayeeWithholdingLimit_MaxLength));
  }

  /// <summary>
  /// The json value of the SpecificPayeeWithholdingLimit attribute.</summary>
  [JsonPropertyName("specificPayeeWithholdingLimit")]
  [Computed]
  public string SpecificPayeeWithholdingLimit_Json
  {
    get => NullIf(SpecificPayeeWithholdingLimit, "");
    set => SpecificPayeeWithholdingLimit = value;
  }

  /// <summary>Length of the EMPLOYEE_STATE_CONTACT_NAME attribute.</summary>
  public const int EmployeeStateContactName_MaxLength = 57;

  /// <summary>
  /// The value of the EMPLOYEE_STATE_CONTACT_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = EmployeeStateContactName_MaxLength)]
  public string EmployeeStateContactName
  {
    get => employeeStateContactName ?? "";
    set => employeeStateContactName =
      TrimEnd(Substring(value, 1, EmployeeStateContactName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeStateContactName attribute.</summary>
  [JsonPropertyName("employeeStateContactName")]
  [Computed]
  public string EmployeeStateContactName_Json
  {
    get => NullIf(EmployeeStateContactName, "");
    set => EmployeeStateContactName = value;
  }

  /// <summary>Length of the EMPLOYEE_STATE_CONTACT_PHONE attribute.</summary>
  public const int EmployeeStateContactPhone_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYEE_STATE_CONTACT_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = EmployeeStateContactPhone_MaxLength)]
  public string EmployeeStateContactPhone
  {
    get => employeeStateContactPhone ?? "";
    set => employeeStateContactPhone =
      TrimEnd(Substring(value, 1, EmployeeStateContactPhone_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeStateContactPhone attribute.</summary>
  [JsonPropertyName("employeeStateContactPhone")]
  [Computed]
  public string EmployeeStateContactPhone_Json
  {
    get => NullIf(EmployeeStateContactPhone, "");
    set => EmployeeStateContactPhone = value;
  }

  /// <summary>Length of the EMPLOYEE_STATE_CONTACT_FAX attribute.</summary>
  public const int EmployeeStateContactFax_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYEE_STATE_CONTACT_FAX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = EmployeeStateContactFax_MaxLength)]
  public string EmployeeStateContactFax
  {
    get => employeeStateContactFax ?? "";
    set => employeeStateContactFax =
      TrimEnd(Substring(value, 1, EmployeeStateContactFax_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeStateContactFax attribute.</summary>
  [JsonPropertyName("employeeStateContactFax")]
  [Computed]
  public string EmployeeStateContactFax_Json
  {
    get => NullIf(EmployeeStateContactFax, "");
    set => EmployeeStateContactFax = value;
  }

  /// <summary>Length of the EMPLOYEE_STATE_CONTACT_EMAIL attribute.</summary>
  public const int EmployeeStateContactEmail_MaxLength = 48;

  /// <summary>
  /// The value of the EMPLOYEE_STATE_CONTACT_EMAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = EmployeeStateContactEmail_MaxLength)]
  public string EmployeeStateContactEmail
  {
    get => employeeStateContactEmail ?? "";
    set => employeeStateContactEmail =
      TrimEnd(Substring(value, 1, EmployeeStateContactEmail_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployeeStateContactEmail attribute.</summary>
  [JsonPropertyName("employeeStateContactEmail")]
  [Computed]
  public string EmployeeStateContactEmail_Json
  {
    get => NullIf(EmployeeStateContactEmail, "");
    set => EmployeeStateContactEmail = value;
  }

  /// <summary>Length of the DOCUMENT_TRACKING_NUMBER attribute.</summary>
  public const int DocumentTrackingNumber_MaxLength = 30;

  /// <summary>
  /// The value of the DOCUMENT_TRACKING_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = DocumentTrackingNumber_MaxLength)]
  public string DocumentTrackingNumber
  {
    get => documentTrackingNumber ?? "";
    set => documentTrackingNumber =
      TrimEnd(Substring(value, 1, DocumentTrackingNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentTrackingNumber attribute.</summary>
  [JsonPropertyName("documentTrackingNumber")]
  [Computed]
  public string DocumentTrackingNumber_Json
  {
    get => NullIf(DocumentTrackingNumber, "");
    set => DocumentTrackingNumber = value;
  }

  /// <summary>Length of the ORDER_IDENTIFIER attribute.</summary>
  public const int OrderIdentifier_MaxLength = 30;

  /// <summary>
  /// The value of the ORDER_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 73, Type = MemberType.Char, Length = OrderIdentifier_MaxLength)
    ]
  public string OrderIdentifier
  {
    get => orderIdentifier ?? "";
    set => orderIdentifier =
      TrimEnd(Substring(value, 1, OrderIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the OrderIdentifier attribute.</summary>
  [JsonPropertyName("orderIdentifier")]
  [Computed]
  public string OrderIdentifier_Json
  {
    get => NullIf(OrderIdentifier, "");
    set => OrderIdentifier = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_NAME attribute.</summary>
  public const int EmployerContactName_MaxLength = 57;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 74, Type = MemberType.Char, Length
    = EmployerContactName_MaxLength)]
  public string EmployerContactName
  {
    get => employerContactName ?? "";
    set => employerContactName =
      TrimEnd(Substring(value, 1, EmployerContactName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactName attribute.</summary>
  [JsonPropertyName("employerContactName")]
  [Computed]
  public string EmployerContactName_Json
  {
    get => NullIf(EmployerContactName, "");
    set => EmployerContactName = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_LINE_1 attribute.
  /// </summary>
  public const int EmployerContactAddressLine1_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 75, Type = MemberType.Char, Length
    = EmployerContactAddressLine1_MaxLength)]
  public string EmployerContactAddressLine1
  {
    get => employerContactAddressLine1 ?? "";
    set => employerContactAddressLine1 =
      TrimEnd(Substring(value, 1, EmployerContactAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressLine1 attribute.</summary>
  [JsonPropertyName("employerContactAddressLine1")]
  [Computed]
  public string EmployerContactAddressLine1_Json
  {
    get => NullIf(EmployerContactAddressLine1, "");
    set => EmployerContactAddressLine1 = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_LINE_2 attribute.
  /// </summary>
  public const int EmployerContactAddressLine2_MaxLength = 25;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = EmployerContactAddressLine2_MaxLength)]
  public string EmployerContactAddressLine2
  {
    get => employerContactAddressLine2 ?? "";
    set => employerContactAddressLine2 =
      TrimEnd(Substring(value, 1, EmployerContactAddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressLine2 attribute.</summary>
  [JsonPropertyName("employerContactAddressLine2")]
  [Computed]
  public string EmployerContactAddressLine2_Json
  {
    get => NullIf(EmployerContactAddressLine2, "");
    set => EmployerContactAddressLine2 = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_CITY attribute.</summary>
  public const int EmployerContactAddressCity_MaxLength = 22;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = EmployerContactAddressCity_MaxLength)]
  public string EmployerContactAddressCity
  {
    get => employerContactAddressCity ?? "";
    set => employerContactAddressCity =
      TrimEnd(Substring(value, 1, EmployerContactAddressCity_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressCity attribute.</summary>
  [JsonPropertyName("employerContactAddressCity")]
  [Computed]
  public string EmployerContactAddressCity_Json
  {
    get => NullIf(EmployerContactAddressCity, "");
    set => EmployerContactAddressCity = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_STATE attribute.</summary>
  public const int EmployerContactAddressState_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 78, Type = MemberType.Char, Length
    = EmployerContactAddressState_MaxLength)]
  public string EmployerContactAddressState
  {
    get => employerContactAddressState ?? "";
    set => employerContactAddressState =
      TrimEnd(Substring(value, 1, EmployerContactAddressState_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressState attribute.</summary>
  [JsonPropertyName("employerContactAddressState")]
  [Computed]
  public string EmployerContactAddressState_Json
  {
    get => NullIf(EmployerContactAddressState, "");
    set => EmployerContactAddressState = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_ZIP attribute.</summary>
  public const int EmployerContactAddressZip_MaxLength = 5;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 79, Type = MemberType.Char, Length
    = EmployerContactAddressZip_MaxLength)]
  public string EmployerContactAddressZip
  {
    get => employerContactAddressZip ?? "";
    set => employerContactAddressZip =
      TrimEnd(Substring(value, 1, EmployerContactAddressZip_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressZip attribute.</summary>
  [JsonPropertyName("employerContactAddressZip")]
  [Computed]
  public string EmployerContactAddressZip_Json
  {
    get => NullIf(EmployerContactAddressZip, "");
    set => EmployerContactAddressZip = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_ADDRESS_EXT_ZIP attribute.
  /// </summary>
  public const int EmployerContactAddressExtZip_MaxLength = 4;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_ADDRESS_EXT_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 80, Type = MemberType.Char, Length
    = EmployerContactAddressExtZip_MaxLength)]
  public string EmployerContactAddressExtZip
  {
    get => employerContactAddressExtZip ?? "";
    set => employerContactAddressExtZip =
      TrimEnd(Substring(value, 1, EmployerContactAddressExtZip_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactAddressExtZip attribute.</summary>
  [JsonPropertyName("employerContactAddressExtZip")]
  [Computed]
  public string EmployerContactAddressExtZip_Json
  {
    get => NullIf(EmployerContactAddressExtZip, "");
    set => EmployerContactAddressExtZip = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_PHONE attribute.</summary>
  public const int EmployerContactPhone_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 81, Type = MemberType.Char, Length
    = EmployerContactPhone_MaxLength)]
  public string EmployerContactPhone
  {
    get => employerContactPhone ?? "";
    set => employerContactPhone =
      TrimEnd(Substring(value, 1, EmployerContactPhone_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactPhone attribute.</summary>
  [JsonPropertyName("employerContactPhone")]
  [Computed]
  public string EmployerContactPhone_Json
  {
    get => NullIf(EmployerContactPhone, "");
    set => EmployerContactPhone = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_FAX attribute.</summary>
  public const int EmployerContactFax_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_FAX attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 82, Type = MemberType.Char, Length
    = EmployerContactFax_MaxLength)]
  public string EmployerContactFax
  {
    get => employerContactFax ?? "";
    set => employerContactFax =
      TrimEnd(Substring(value, 1, EmployerContactFax_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactFax attribute.</summary>
  [JsonPropertyName("employerContactFax")]
  [Computed]
  public string EmployerContactFax_Json
  {
    get => NullIf(EmployerContactFax, "");
    set => EmployerContactFax = value;
  }

  /// <summary>Length of the EMPLOYER_CONTACT_EMAIL attribute.</summary>
  public const int EmployerContactEmail_MaxLength = 48;

  /// <summary>
  /// The value of the EMPLOYER_CONTACT_EMAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 83, Type = MemberType.Char, Length
    = EmployerContactEmail_MaxLength)]
  public string EmployerContactEmail
  {
    get => employerContactEmail ?? "";
    set => employerContactEmail =
      TrimEnd(Substring(value, 1, EmployerContactEmail_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerContactEmail attribute.</summary>
  [JsonPropertyName("employerContactEmail")]
  [Computed]
  public string EmployerContactEmail_Json
  {
    get => NullIf(EmployerContactEmail, "");
    set => EmployerContactEmail = value;
  }

  /// <summary>Length of the CHILD_1_LAST_NAME attribute.</summary>
  public const int Child1LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_1_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 84, Type = MemberType.Char, Length = Child1LastName_MaxLength)]
    
  public string Child1LastName
  {
    get => child1LastName ?? "";
    set => child1LastName =
      TrimEnd(Substring(value, 1, Child1LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child1LastName attribute.</summary>
  [JsonPropertyName("child1LastName")]
  [Computed]
  public string Child1LastName_Json
  {
    get => NullIf(Child1LastName, "");
    set => Child1LastName = value;
  }

  /// <summary>Length of the CHILD_1_FIRST_NAME attribute.</summary>
  public const int Child1FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_1_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 85, Type = MemberType.Char, Length = Child1FirstName_MaxLength)
    ]
  public string Child1FirstName
  {
    get => child1FirstName ?? "";
    set => child1FirstName =
      TrimEnd(Substring(value, 1, Child1FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child1FirstName attribute.</summary>
  [JsonPropertyName("child1FirstName")]
  [Computed]
  public string Child1FirstName_Json
  {
    get => NullIf(Child1FirstName, "");
    set => Child1FirstName = value;
  }

  /// <summary>Length of the CHILD_1_MIDDLE_NAME attribute.</summary>
  public const int Child1MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_1_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 86, Type = MemberType.Char, Length
    = Child1MiddleName_MaxLength)]
  public string Child1MiddleName
  {
    get => child1MiddleName ?? "";
    set => child1MiddleName =
      TrimEnd(Substring(value, 1, Child1MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child1MiddleName attribute.</summary>
  [JsonPropertyName("child1MiddleName")]
  [Computed]
  public string Child1MiddleName_Json
  {
    get => NullIf(Child1MiddleName, "");
    set => Child1MiddleName = value;
  }

  /// <summary>Length of the CHILD_1_SUFFIX_NAME attribute.</summary>
  public const int Child1SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_1_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 87, Type = MemberType.Char, Length
    = Child1SuffixName_MaxLength)]
  public string Child1SuffixName
  {
    get => child1SuffixName ?? "";
    set => child1SuffixName =
      TrimEnd(Substring(value, 1, Child1SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child1SuffixName attribute.</summary>
  [JsonPropertyName("child1SuffixName")]
  [Computed]
  public string Child1SuffixName_Json
  {
    get => NullIf(Child1SuffixName, "");
    set => Child1SuffixName = value;
  }

  /// <summary>Length of the CHILD_1_BIRTH_DATE attribute.</summary>
  public const int Child1BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_1_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 88, Type = MemberType.Char, Length = Child1BirthDate_MaxLength)
    ]
  public string Child1BirthDate
  {
    get => child1BirthDate ?? "";
    set => child1BirthDate =
      TrimEnd(Substring(value, 1, Child1BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child1BirthDate attribute.</summary>
  [JsonPropertyName("child1BirthDate")]
  [Computed]
  public string Child1BirthDate_Json
  {
    get => NullIf(Child1BirthDate, "");
    set => Child1BirthDate = value;
  }

  /// <summary>Length of the CHILD_2_LAST_NAME attribute.</summary>
  public const int Child2LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_2_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 89, Type = MemberType.Char, Length = Child2LastName_MaxLength)]
    
  public string Child2LastName
  {
    get => child2LastName ?? "";
    set => child2LastName =
      TrimEnd(Substring(value, 1, Child2LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child2LastName attribute.</summary>
  [JsonPropertyName("child2LastName")]
  [Computed]
  public string Child2LastName_Json
  {
    get => NullIf(Child2LastName, "");
    set => Child2LastName = value;
  }

  /// <summary>Length of the CHILD_2_FIRST_NAME attribute.</summary>
  public const int Child2FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_2_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 90, Type = MemberType.Char, Length = Child2FirstName_MaxLength)
    ]
  public string Child2FirstName
  {
    get => child2FirstName ?? "";
    set => child2FirstName =
      TrimEnd(Substring(value, 1, Child2FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child2FirstName attribute.</summary>
  [JsonPropertyName("child2FirstName")]
  [Computed]
  public string Child2FirstName_Json
  {
    get => NullIf(Child2FirstName, "");
    set => Child2FirstName = value;
  }

  /// <summary>Length of the CHILD_2_MIDDLE_NAME attribute.</summary>
  public const int Child2MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_2_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 91, Type = MemberType.Char, Length
    = Child2MiddleName_MaxLength)]
  public string Child2MiddleName
  {
    get => child2MiddleName ?? "";
    set => child2MiddleName =
      TrimEnd(Substring(value, 1, Child2MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child2MiddleName attribute.</summary>
  [JsonPropertyName("child2MiddleName")]
  [Computed]
  public string Child2MiddleName_Json
  {
    get => NullIf(Child2MiddleName, "");
    set => Child2MiddleName = value;
  }

  /// <summary>Length of the CHILD_2_SUFFIX_NAME attribute.</summary>
  public const int Child2SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_2_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 92, Type = MemberType.Char, Length
    = Child2SuffixName_MaxLength)]
  public string Child2SuffixName
  {
    get => child2SuffixName ?? "";
    set => child2SuffixName =
      TrimEnd(Substring(value, 1, Child2SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child2SuffixName attribute.</summary>
  [JsonPropertyName("child2SuffixName")]
  [Computed]
  public string Child2SuffixName_Json
  {
    get => NullIf(Child2SuffixName, "");
    set => Child2SuffixName = value;
  }

  /// <summary>Length of the CHILD_2_BIRTH_DATE attribute.</summary>
  public const int Child2BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_2_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 93, Type = MemberType.Char, Length = Child2BirthDate_MaxLength)
    ]
  public string Child2BirthDate
  {
    get => child2BirthDate ?? "";
    set => child2BirthDate =
      TrimEnd(Substring(value, 1, Child2BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child2BirthDate attribute.</summary>
  [JsonPropertyName("child2BirthDate")]
  [Computed]
  public string Child2BirthDate_Json
  {
    get => NullIf(Child2BirthDate, "");
    set => Child2BirthDate = value;
  }

  /// <summary>Length of the CHILD_3_LAST_NAME attribute.</summary>
  public const int Child3LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_3_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 94, Type = MemberType.Char, Length = Child3LastName_MaxLength)]
    
  public string Child3LastName
  {
    get => child3LastName ?? "";
    set => child3LastName =
      TrimEnd(Substring(value, 1, Child3LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child3LastName attribute.</summary>
  [JsonPropertyName("child3LastName")]
  [Computed]
  public string Child3LastName_Json
  {
    get => NullIf(Child3LastName, "");
    set => Child3LastName = value;
  }

  /// <summary>Length of the CHILD_3_FIRST_NAME attribute.</summary>
  public const int Child3FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_3_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 95, Type = MemberType.Char, Length = Child3FirstName_MaxLength)
    ]
  public string Child3FirstName
  {
    get => child3FirstName ?? "";
    set => child3FirstName =
      TrimEnd(Substring(value, 1, Child3FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child3FirstName attribute.</summary>
  [JsonPropertyName("child3FirstName")]
  [Computed]
  public string Child3FirstName_Json
  {
    get => NullIf(Child3FirstName, "");
    set => Child3FirstName = value;
  }

  /// <summary>Length of the CHILD_3_MIDDLE_NAME attribute.</summary>
  public const int Child3MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_3_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 96, Type = MemberType.Char, Length
    = Child3MiddleName_MaxLength)]
  public string Child3MiddleName
  {
    get => child3MiddleName ?? "";
    set => child3MiddleName =
      TrimEnd(Substring(value, 1, Child3MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child3MiddleName attribute.</summary>
  [JsonPropertyName("child3MiddleName")]
  [Computed]
  public string Child3MiddleName_Json
  {
    get => NullIf(Child3MiddleName, "");
    set => Child3MiddleName = value;
  }

  /// <summary>Length of the CHILD_3_SUFFIX_NAME attribute.</summary>
  public const int Child3SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_3_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 97, Type = MemberType.Char, Length
    = Child3SuffixName_MaxLength)]
  public string Child3SuffixName
  {
    get => child3SuffixName ?? "";
    set => child3SuffixName =
      TrimEnd(Substring(value, 1, Child3SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child3SuffixName attribute.</summary>
  [JsonPropertyName("child3SuffixName")]
  [Computed]
  public string Child3SuffixName_Json
  {
    get => NullIf(Child3SuffixName, "");
    set => Child3SuffixName = value;
  }

  /// <summary>Length of the CHILD_3_BIRTH_DATE attribute.</summary>
  public const int Child3BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_3_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 98, Type = MemberType.Char, Length = Child3BirthDate_MaxLength)
    ]
  public string Child3BirthDate
  {
    get => child3BirthDate ?? "";
    set => child3BirthDate =
      TrimEnd(Substring(value, 1, Child3BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child3BirthDate attribute.</summary>
  [JsonPropertyName("child3BirthDate")]
  [Computed]
  public string Child3BirthDate_Json
  {
    get => NullIf(Child3BirthDate, "");
    set => Child3BirthDate = value;
  }

  /// <summary>Length of the CHILD_4_LAST_NAME attribute.</summary>
  public const int Child4LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_4_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 99, Type = MemberType.Char, Length = Child4LastName_MaxLength)]
    
  public string Child4LastName
  {
    get => child4LastName ?? "";
    set => child4LastName =
      TrimEnd(Substring(value, 1, Child4LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child4LastName attribute.</summary>
  [JsonPropertyName("child4LastName")]
  [Computed]
  public string Child4LastName_Json
  {
    get => NullIf(Child4LastName, "");
    set => Child4LastName = value;
  }

  /// <summary>Length of the CHILD_4_FIRST_NAME attribute.</summary>
  public const int Child4FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_4_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 100, Type = MemberType.Char, Length
    = Child4FirstName_MaxLength)]
  public string Child4FirstName
  {
    get => child4FirstName ?? "";
    set => child4FirstName =
      TrimEnd(Substring(value, 1, Child4FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child4FirstName attribute.</summary>
  [JsonPropertyName("child4FirstName")]
  [Computed]
  public string Child4FirstName_Json
  {
    get => NullIf(Child4FirstName, "");
    set => Child4FirstName = value;
  }

  /// <summary>Length of the CHILD_4_MIDDLE_NAME attribute.</summary>
  public const int Child4MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_4_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 101, Type = MemberType.Char, Length
    = Child4MiddleName_MaxLength)]
  public string Child4MiddleName
  {
    get => child4MiddleName ?? "";
    set => child4MiddleName =
      TrimEnd(Substring(value, 1, Child4MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child4MiddleName attribute.</summary>
  [JsonPropertyName("child4MiddleName")]
  [Computed]
  public string Child4MiddleName_Json
  {
    get => NullIf(Child4MiddleName, "");
    set => Child4MiddleName = value;
  }

  /// <summary>Length of the CHILD_4_SUFFIX_NAME attribute.</summary>
  public const int Child4SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_4_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 102, Type = MemberType.Char, Length
    = Child4SuffixName_MaxLength)]
  public string Child4SuffixName
  {
    get => child4SuffixName ?? "";
    set => child4SuffixName =
      TrimEnd(Substring(value, 1, Child4SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child4SuffixName attribute.</summary>
  [JsonPropertyName("child4SuffixName")]
  [Computed]
  public string Child4SuffixName_Json
  {
    get => NullIf(Child4SuffixName, "");
    set => Child4SuffixName = value;
  }

  /// <summary>Length of the CHILD_4_BIRTH_DATE attribute.</summary>
  public const int Child4BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_4_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 103, Type = MemberType.Char, Length
    = Child4BirthDate_MaxLength)]
  public string Child4BirthDate
  {
    get => child4BirthDate ?? "";
    set => child4BirthDate =
      TrimEnd(Substring(value, 1, Child4BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child4BirthDate attribute.</summary>
  [JsonPropertyName("child4BirthDate")]
  [Computed]
  public string Child4BirthDate_Json
  {
    get => NullIf(Child4BirthDate, "");
    set => Child4BirthDate = value;
  }

  /// <summary>Length of the CHILD_5_LAST_NAME attribute.</summary>
  public const int Child5LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_5_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 104, Type = MemberType.Char, Length = Child5LastName_MaxLength)
    ]
  public string Child5LastName
  {
    get => child5LastName ?? "";
    set => child5LastName =
      TrimEnd(Substring(value, 1, Child5LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child5LastName attribute.</summary>
  [JsonPropertyName("child5LastName")]
  [Computed]
  public string Child5LastName_Json
  {
    get => NullIf(Child5LastName, "");
    set => Child5LastName = value;
  }

  /// <summary>Length of the CHILD_5_FIRST_NAME attribute.</summary>
  public const int Child5FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_5_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 105, Type = MemberType.Char, Length
    = Child5FirstName_MaxLength)]
  public string Child5FirstName
  {
    get => child5FirstName ?? "";
    set => child5FirstName =
      TrimEnd(Substring(value, 1, Child5FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child5FirstName attribute.</summary>
  [JsonPropertyName("child5FirstName")]
  [Computed]
  public string Child5FirstName_Json
  {
    get => NullIf(Child5FirstName, "");
    set => Child5FirstName = value;
  }

  /// <summary>Length of the CHILD_5_MIDDLE_NAME attribute.</summary>
  public const int Child5MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_5_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 106, Type = MemberType.Char, Length
    = Child5MiddleName_MaxLength)]
  public string Child5MiddleName
  {
    get => child5MiddleName ?? "";
    set => child5MiddleName =
      TrimEnd(Substring(value, 1, Child5MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child5MiddleName attribute.</summary>
  [JsonPropertyName("child5MiddleName")]
  [Computed]
  public string Child5MiddleName_Json
  {
    get => NullIf(Child5MiddleName, "");
    set => Child5MiddleName = value;
  }

  /// <summary>Length of the CHILD_5_SUFFIX_NAME attribute.</summary>
  public const int Child5SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_5_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 107, Type = MemberType.Char, Length
    = Child5SuffixName_MaxLength)]
  public string Child5SuffixName
  {
    get => child5SuffixName ?? "";
    set => child5SuffixName =
      TrimEnd(Substring(value, 1, Child5SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child5SuffixName attribute.</summary>
  [JsonPropertyName("child5SuffixName")]
  [Computed]
  public string Child5SuffixName_Json
  {
    get => NullIf(Child5SuffixName, "");
    set => Child5SuffixName = value;
  }

  /// <summary>Length of the CHILD_5_BIRTH_DATE attribute.</summary>
  public const int Child5BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_5_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 108, Type = MemberType.Char, Length
    = Child5BirthDate_MaxLength)]
  public string Child5BirthDate
  {
    get => child5BirthDate ?? "";
    set => child5BirthDate =
      TrimEnd(Substring(value, 1, Child5BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child5BirthDate attribute.</summary>
  [JsonPropertyName("child5BirthDate")]
  [Computed]
  public string Child5BirthDate_Json
  {
    get => NullIf(Child5BirthDate, "");
    set => Child5BirthDate = value;
  }

  /// <summary>Length of the CHILD_6_LAST_NAME attribute.</summary>
  public const int Child6LastName_MaxLength = 20;

  /// <summary>
  /// The value of the CHILD_6_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 109, Type = MemberType.Char, Length = Child6LastName_MaxLength)
    ]
  public string Child6LastName
  {
    get => child6LastName ?? "";
    set => child6LastName =
      TrimEnd(Substring(value, 1, Child6LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child6LastName attribute.</summary>
  [JsonPropertyName("child6LastName")]
  [Computed]
  public string Child6LastName_Json
  {
    get => NullIf(Child6LastName, "");
    set => Child6LastName = value;
  }

  /// <summary>Length of the CHILD_6_FIRST_NAME attribute.</summary>
  public const int Child6FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_6_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 110, Type = MemberType.Char, Length
    = Child6FirstName_MaxLength)]
  public string Child6FirstName
  {
    get => child6FirstName ?? "";
    set => child6FirstName =
      TrimEnd(Substring(value, 1, Child6FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child6FirstName attribute.</summary>
  [JsonPropertyName("child6FirstName")]
  [Computed]
  public string Child6FirstName_Json
  {
    get => NullIf(Child6FirstName, "");
    set => Child6FirstName = value;
  }

  /// <summary>Length of the CHILD_6_MIDDLE_NAME attribute.</summary>
  public const int Child6MiddleName_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_6_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 111, Type = MemberType.Char, Length
    = Child6MiddleName_MaxLength)]
  public string Child6MiddleName
  {
    get => child6MiddleName ?? "";
    set => child6MiddleName =
      TrimEnd(Substring(value, 1, Child6MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child6MiddleName attribute.</summary>
  [JsonPropertyName("child6MiddleName")]
  [Computed]
  public string Child6MiddleName_Json
  {
    get => NullIf(Child6MiddleName, "");
    set => Child6MiddleName = value;
  }

  /// <summary>Length of the CHILD_6_SUFFIX_NAME attribute.</summary>
  public const int Child6SuffixName_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_6_SUFFIX_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 112, Type = MemberType.Char, Length
    = Child6SuffixName_MaxLength)]
  public string Child6SuffixName
  {
    get => child6SuffixName ?? "";
    set => child6SuffixName =
      TrimEnd(Substring(value, 1, Child6SuffixName_MaxLength));
  }

  /// <summary>
  /// The json value of the Child6SuffixName attribute.</summary>
  [JsonPropertyName("child6SuffixName")]
  [Computed]
  public string Child6SuffixName_Json
  {
    get => NullIf(Child6SuffixName, "");
    set => Child6SuffixName = value;
  }

  /// <summary>Length of the CHILD_6_BIRTH_DATE attribute.</summary>
  public const int Child6BirthDate_MaxLength = 8;

  /// <summary>
  /// The value of the CHILD_6_BIRTH_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 113, Type = MemberType.Char, Length
    = Child6BirthDate_MaxLength)]
  public string Child6BirthDate
  {
    get => child6BirthDate ?? "";
    set => child6BirthDate =
      TrimEnd(Substring(value, 1, Child6BirthDate_MaxLength));
  }

  /// <summary>
  /// The json value of the Child6BirthDate attribute.</summary>
  [JsonPropertyName("child6BirthDate")]
  [Computed]
  public string Child6BirthDate_Json
  {
    get => NullIf(Child6BirthDate, "");
    set => Child6BirthDate = value;
  }

  /// <summary>Length of the LUMP_SUM_PAYMENT_AMOUNT attribute.</summary>
  public const int LumpSumPaymentAmount_MaxLength = 11;

  /// <summary>
  /// The value of the LUMP_SUM_PAYMENT_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 114, Type = MemberType.Char, Length
    = LumpSumPaymentAmount_MaxLength)]
  public string LumpSumPaymentAmount
  {
    get => lumpSumPaymentAmount ?? "";
    set => lumpSumPaymentAmount =
      TrimEnd(Substring(value, 1, LumpSumPaymentAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the LumpSumPaymentAmount attribute.</summary>
  [JsonPropertyName("lumpSumPaymentAmount")]
  [Computed]
  public string LumpSumPaymentAmount_Json
  {
    get => NullIf(LumpSumPaymentAmount, "");
    set => LumpSumPaymentAmount = value;
  }

  /// <summary>Length of the REMITTANCE_IDENTIFIER attribute.</summary>
  public const int RemittanceIdentifier_MaxLength = 20;

  /// <summary>
  /// The value of the REMITTANCE_IDENTIFIER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 115, Type = MemberType.Char, Length
    = RemittanceIdentifier_MaxLength)]
  public string RemittanceIdentifier
  {
    get => remittanceIdentifier ?? "";
    set => remittanceIdentifier =
      TrimEnd(Substring(value, 1, RemittanceIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the RemittanceIdentifier attribute.</summary>
  [JsonPropertyName("remittanceIdentifier")]
  [Computed]
  public string RemittanceIdentifier_Json
  {
    get => NullIf(RemittanceIdentifier, "");
    set => RemittanceIdentifier = value;
  }

  /// <summary>Length of the DOCUMENT_IMAGE_TEXT attribute.</summary>
  public const int DocumentImageText_MaxLength = 25;

  /// <summary>
  /// The value of the DOCUMENT_IMAGE_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 116, Type = MemberType.Char, Length
    = DocumentImageText_MaxLength)]
  public string DocumentImageText
  {
    get => documentImageText ?? "";
    set => documentImageText =
      TrimEnd(Substring(value, 1, DocumentImageText_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentImageText attribute.</summary>
  [JsonPropertyName("documentImageText")]
  [Computed]
  public string DocumentImageText_Json
  {
    get => NullIf(DocumentImageText, "");
    set => DocumentImageText = value;
  }

  private string incomeWithholdingStartInstruc;
  private string documentCode;
  private string documentActionCode;
  private string documentDate;
  private string issuingStateTribeTerritoryNm;
  private string issuingJurisdictionName;
  private string caseIdentifier;
  private string employerName;
  private string employerAddressLine1;
  private string employerAddressLine2;
  private string employerAddressCityName;
  private string employerAddressStateCode;
  private string employerAddressZipCode;
  private string employerAddressExtZipCode;
  private string ein;
  private string employeeLastName;
  private string employeeFirstName;
  private string employeeMiddleName;
  private string employeeSuffix;
  private string employeeSsn;
  private string employeeBirthDate;
  private string obligeeLastName;
  private string obligeeFirstName;
  private string obligeeMiddleName;
  private string obligeeNameSuffix;
  private string issuingTribunalName;
  private string supportCurrentChildAmount;
  private string supportCurrentChildFrequency;
  private string supportPastDueChildAmount;
  private string supportPastDueChildFrequency;
  private string supportCurrentMedicalAmount;
  private string supportCurrentMedicalFrequenc;
  private string supportPastDueMedicalAmount;
  private string supportPastDueMedicalFrequen;
  private string supportCurrentSpousalAmount;
  private string supportCurrentSpousalFrequenc;
  private string supportPastDueSpousalAmount;
  private string supportPastDueSpousalFrequen;
  private string obligationOtherAmount;
  private string obligationOtherFrequencyCode;
  private string obligationOtherDescription;
  private string obligationTotalAmount;
  private string obligationTotalFrequency;
  private string arrears12WkOverdueCode;
  private string iwoDeductionWeeklyAmount;
  private string iwoDeductionBiweeklyAmount;
  private string iwoDeductionSemimonthlyAmount;
  private string iwoDeductionMonthlyAmount;
  private string stateTribeTerritoryName;
  private string beginWithholdingWithinDays;
  private string incomeWithholdingStartDate;
  private string sendPaymentWithhinDays;
  private string iwoCcpaPercentRate;
  private string payeeName;
  private string payeeAddressLine1;
  private string payeeAddressLine2;
  private string payeeAddressCity;
  private string payeeAddressStateCode;
  private string payeeAddressZipCode;
  private string payeeAddressExtZipCode;
  private string payeeRemittanceFipsCode;
  private string governmentOfficialName;
  private string issuingOfficialTitle;
  private string sendEmployeeCopyIndicator;
  private string penaltyLiabilityInfoText;
  private string antidiscriminationProvisionTxt;
  private string specificPayeeWithholdingLimit;
  private string employeeStateContactName;
  private string employeeStateContactPhone;
  private string employeeStateContactFax;
  private string employeeStateContactEmail;
  private string documentTrackingNumber;
  private string orderIdentifier;
  private string employerContactName;
  private string employerContactAddressLine1;
  private string employerContactAddressLine2;
  private string employerContactAddressCity;
  private string employerContactAddressState;
  private string employerContactAddressZip;
  private string employerContactAddressExtZip;
  private string employerContactPhone;
  private string employerContactFax;
  private string employerContactEmail;
  private string child1LastName;
  private string child1FirstName;
  private string child1MiddleName;
  private string child1SuffixName;
  private string child1BirthDate;
  private string child2LastName;
  private string child2FirstName;
  private string child2MiddleName;
  private string child2SuffixName;
  private string child2BirthDate;
  private string child3LastName;
  private string child3FirstName;
  private string child3MiddleName;
  private string child3SuffixName;
  private string child3BirthDate;
  private string child4LastName;
  private string child4FirstName;
  private string child4MiddleName;
  private string child4SuffixName;
  private string child4BirthDate;
  private string child5LastName;
  private string child5FirstName;
  private string child5MiddleName;
  private string child5SuffixName;
  private string child5BirthDate;
  private string child6LastName;
  private string child6FirstName;
  private string child6MiddleName;
  private string child6SuffixName;
  private string child6BirthDate;
  private string lumpSumPaymentAmount;
  private string remittanceIdentifier;
  private string documentImageText;
}

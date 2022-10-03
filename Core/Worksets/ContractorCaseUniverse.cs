// The source file: CONTRACTOR_CASE_UNIVERSE, ID: 1902464228, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ContractorCaseUniverse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ContractorCaseUniverse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ContractorCaseUniverse(ContractorCaseUniverse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ContractorCaseUniverse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ContractorCaseUniverse that)
  {
    base.Assign(that);
    headerFooter = that.headerFooter;
    contractorName = that.contractorName;
    judicialDistrict = that.judicialDistrict;
    caseNumber = that.caseNumber;
    caseOpenDate = that.caseOpenDate;
    assignedCaseworkerFirstName = that.assignedCaseworkerFirstName;
    assignedCaseworkerLastName = that.assignedCaseworkerLastName;
    pyramidCategory = that.pyramidCategory;
    addressActive = that.addressActive;
    employerActive = that.employerActive;
    currentSupportDue = that.currentSupportDue;
    currentSupportPaid = that.currentSupportPaid;
    collectionRate = that.collectionRate;
    casePayingArrears = that.casePayingArrears;
    caseFunction = that.caseFunction;
    caseProgram = that.caseProgram;
    paProgramEndDate = that.paProgramEndDate;
    curaAmount = that.curaAmount;
    familyViolence = that.familyViolence;
    cpNoncooperation = that.cpNoncooperation;
    pendingCaseClosureDate = that.pendingCaseClosureDate;
    dateOfEmancipation = that.dateOfEmancipation;
    youngestEmancipationDate = that.youngestEmancipationDate;
    childBow = that.childBow;
    cpDateOfBirth = that.cpDateOfBirth;
    cpEthnicity = that.cpEthnicity;
    cpZipCode = that.cpZipCode;
    cpHomePhoneAreaCode = that.cpHomePhoneAreaCode;
    cpHomePhone = that.cpHomePhone;
    cpCellPhoneAreaCode = that.cpCellPhoneAreaCode;
    cpCellPhone = that.cpCellPhone;
    ncpPersonNumber = that.ncpPersonNumber;
    ncpLastName = that.ncpLastName;
    ncpFirstName = that.ncpFirstName;
    ncpDateOfBirth = that.ncpDateOfBirth;
    ncpEthnicity = that.ncpEthnicity;
    ncpLocateDate = that.ncpLocateDate;
    ncpZipCode = that.ncpZipCode;
    ncpForeignCountryCode = that.ncpForeignCountryCode;
    ncpHomePhoneAreaCode = that.ncpHomePhoneAreaCode;
    ncpHomePhone = that.ncpHomePhone;
    ncpCellPhoneAreaCode = that.ncpCellPhoneAreaCode;
    ncpCellPhone = that.ncpCellPhone;
    ncpIncarcerated = that.ncpIncarcerated;
    ncpInBankruptcy = that.ncpInBankruptcy;
    ncpRepresentedByCouncil = that.ncpRepresentedByCouncil;
    ncpEmployerName = that.ncpEmployerName;
    ncpOtherIncomeSource = that.ncpOtherIncomeSource;
    ncpInterstateInitiating = that.ncpInterstateInitiating;
    ncpInterstateResponding = that.ncpInterstateResponding;
    coCourtOrderNumber = that.coCourtOrderNumber;
    coCsCollectedInMonth = that.coCsCollectedInMonth;
    coCsDueInMonth = that.coCsDueInMonth;
    coCsCollectedFfytd = that.coCsCollectedFfytd;
    coCsDueFfytd = that.coCsDueFfytd;
    coTotalArrearsAmountDue = that.coTotalArrearsAmountDue;
    coArrearsPaidInMonth = that.coArrearsPaidInMonth;
    coArrearsPaidFfytd = that.coArrearsPaidFfytd;
    coLastPaymentAmount = that.coLastPaymentAmount;
    coLastPaymentDate = that.coLastPaymentDate;
    coLastPaymentType = that.coLastPaymentType;
    coLastDsoPaymentDate = that.coLastDsoPaymentDate;
    coLastIClassCreatedDate = that.coLastIClassCreatedDate;
    coLastIClassActionTaken = that.coLastIClassActionTaken;
    coLastIClassFiledDate = that.coLastIClassFiledDate;
    coLastIClassIwgl = that.coLastIClassIwgl;
    coMonthlyIwoWaAmount = that.coMonthlyIwoWaAmount;
    coContemptHearingDate = that.coContemptHearingDate;
    coContemptServiceDate = that.coContemptServiceDate;
    coDemandLetterCreatedDate = that.coDemandLetterCreatedDate;
    coPetitionCreatedDate = that.coPetitionCreatedDate;
    coPetitionFiledDate = that.coPetitionFiledDate;
  }

  /// <summary>Length of the HEADER_FOOTER attribute.</summary>
  public const int HeaderFooter_MaxLength = 200;

  /// <summary>
  /// The value of the HEADER_FOOTER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = HeaderFooter_MaxLength)]
  public string HeaderFooter
  {
    get => headerFooter ?? "";
    set => headerFooter = TrimEnd(Substring(value, 1, HeaderFooter_MaxLength));
  }

  /// <summary>
  /// The json value of the HeaderFooter attribute.</summary>
  [JsonPropertyName("headerFooter")]
  [Computed]
  public string HeaderFooter_Json
  {
    get => NullIf(HeaderFooter, "");
    set => HeaderFooter = value;
  }

  /// <summary>Length of the CONTRACTOR_NAME attribute.</summary>
  public const int ContractorName_MaxLength = 20;

  /// <summary>
  /// The value of the CONTRACTOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ContractorName_MaxLength)]
  public string ContractorName
  {
    get => contractorName ?? "";
    set => contractorName =
      TrimEnd(Substring(value, 1, ContractorName_MaxLength));
  }

  /// <summary>
  /// The json value of the ContractorName attribute.</summary>
  [JsonPropertyName("contractorName")]
  [Computed]
  public string ContractorName_Json
  {
    get => NullIf(ContractorName, "");
    set => ContractorName = value;
  }

  /// <summary>Length of the JUDICIAL_DISTRICT attribute.</summary>
  public const int JudicialDistrict_MaxLength = 2;

  /// <summary>
  /// The value of the JUDICIAL_DISTRICT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = JudicialDistrict_MaxLength)
    ]
  public string JudicialDistrict
  {
    get => judicialDistrict ?? "";
    set => judicialDistrict =
      TrimEnd(Substring(value, 1, JudicialDistrict_MaxLength));
  }

  /// <summary>
  /// The json value of the JudicialDistrict attribute.</summary>
  [JsonPropertyName("judicialDistrict")]
  [Computed]
  public string JudicialDistrict_Json
  {
    get => NullIf(JudicialDistrict, "");
    set => JudicialDistrict = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
  public string CaseNumber
  {
    get => caseNumber ?? "";
    set => caseNumber = TrimEnd(Substring(value, 1, CaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseNumber attribute.</summary>
  [JsonPropertyName("caseNumber")]
  [Computed]
  public string CaseNumber_Json
  {
    get => NullIf(CaseNumber, "");
    set => CaseNumber = value;
  }

  /// <summary>Length of the CASE_OPEN_DATE attribute.</summary>
  public const int CaseOpenDate_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_OPEN_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CaseOpenDate_MaxLength)]
  public string CaseOpenDate
  {
    get => caseOpenDate ?? "";
    set => caseOpenDate = TrimEnd(Substring(value, 1, CaseOpenDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseOpenDate attribute.</summary>
  [JsonPropertyName("caseOpenDate")]
  [Computed]
  public string CaseOpenDate_Json
  {
    get => NullIf(CaseOpenDate, "");
    set => CaseOpenDate = value;
  }

  /// <summary>Length of the ASSIGNED_CASEWORKER_FIRST_NAME attribute.</summary>
  public const int AssignedCaseworkerFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the ASSIGNED_CASEWORKER_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = AssignedCaseworkerFirstName_MaxLength)]
  public string AssignedCaseworkerFirstName
  {
    get => assignedCaseworkerFirstName ?? "";
    set => assignedCaseworkerFirstName =
      TrimEnd(Substring(value, 1, AssignedCaseworkerFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the AssignedCaseworkerFirstName attribute.</summary>
  [JsonPropertyName("assignedCaseworkerFirstName")]
  [Computed]
  public string AssignedCaseworkerFirstName_Json
  {
    get => NullIf(AssignedCaseworkerFirstName, "");
    set => AssignedCaseworkerFirstName = value;
  }

  /// <summary>Length of the ASSIGNED_CASEWORKER_LAST_NAME attribute.</summary>
  public const int AssignedCaseworkerLastName_MaxLength = 17;

  /// <summary>
  /// The value of the ASSIGNED_CASEWORKER_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = AssignedCaseworkerLastName_MaxLength)]
  public string AssignedCaseworkerLastName
  {
    get => assignedCaseworkerLastName ?? "";
    set => assignedCaseworkerLastName =
      TrimEnd(Substring(value, 1, AssignedCaseworkerLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the AssignedCaseworkerLastName attribute.</summary>
  [JsonPropertyName("assignedCaseworkerLastName")]
  [Computed]
  public string AssignedCaseworkerLastName_Json
  {
    get => NullIf(AssignedCaseworkerLastName, "");
    set => AssignedCaseworkerLastName = value;
  }

  /// <summary>Length of the PYRAMID_CATEGORY attribute.</summary>
  public const int PyramidCategory_MaxLength = 27;

  /// <summary>
  /// The value of the PYRAMID_CATEGORY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = PyramidCategory_MaxLength)]
    
  public string PyramidCategory
  {
    get => pyramidCategory ?? "";
    set => pyramidCategory =
      TrimEnd(Substring(value, 1, PyramidCategory_MaxLength));
  }

  /// <summary>
  /// The json value of the PyramidCategory attribute.</summary>
  [JsonPropertyName("pyramidCategory")]
  [Computed]
  public string PyramidCategory_Json
  {
    get => NullIf(PyramidCategory, "");
    set => PyramidCategory = value;
  }

  /// <summary>Length of the ADDRESS_ACTIVE attribute.</summary>
  public const int AddressActive_MaxLength = 1;

  /// <summary>
  /// The value of the ADDRESS_ACTIVE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = AddressActive_MaxLength)]
  public string AddressActive
  {
    get => addressActive ?? "";
    set => addressActive =
      TrimEnd(Substring(value, 1, AddressActive_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressActive attribute.</summary>
  [JsonPropertyName("addressActive")]
  [Computed]
  public string AddressActive_Json
  {
    get => NullIf(AddressActive, "");
    set => AddressActive = value;
  }

  /// <summary>Length of the EMPLOYER_ACTIVE attribute.</summary>
  public const int EmployerActive_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYER_ACTIVE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = EmployerActive_MaxLength)]
    
  public string EmployerActive
  {
    get => employerActive ?? "";
    set => employerActive =
      TrimEnd(Substring(value, 1, EmployerActive_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerActive attribute.</summary>
  [JsonPropertyName("employerActive")]
  [Computed]
  public string EmployerActive_Json
  {
    get => NullIf(EmployerActive, "");
    set => EmployerActive = value;
  }

  /// <summary>Length of the CURRENT_SUPPORT_DUE attribute.</summary>
  public const int CurrentSupportDue_MaxLength = 15;

  /// <summary>
  /// The value of the CURRENT_SUPPORT_DUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = CurrentSupportDue_MaxLength)]
  public string CurrentSupportDue
  {
    get => currentSupportDue ?? "";
    set => currentSupportDue =
      TrimEnd(Substring(value, 1, CurrentSupportDue_MaxLength));
  }

  /// <summary>
  /// The json value of the CurrentSupportDue attribute.</summary>
  [JsonPropertyName("currentSupportDue")]
  [Computed]
  public string CurrentSupportDue_Json
  {
    get => NullIf(CurrentSupportDue, "");
    set => CurrentSupportDue = value;
  }

  /// <summary>Length of the CURRENT_SUPPORT_PAID attribute.</summary>
  public const int CurrentSupportPaid_MaxLength = 15;

  /// <summary>
  /// The value of the CURRENT_SUPPORT_PAID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CurrentSupportPaid_MaxLength)]
  public string CurrentSupportPaid
  {
    get => currentSupportPaid ?? "";
    set => currentSupportPaid =
      TrimEnd(Substring(value, 1, CurrentSupportPaid_MaxLength));
  }

  /// <summary>
  /// The json value of the CurrentSupportPaid attribute.</summary>
  [JsonPropertyName("currentSupportPaid")]
  [Computed]
  public string CurrentSupportPaid_Json
  {
    get => NullIf(CurrentSupportPaid, "");
    set => CurrentSupportPaid = value;
  }

  /// <summary>Length of the COLLECTION_RATE attribute.</summary>
  public const int CollectionRate_MaxLength = 9;

  /// <summary>
  /// The value of the COLLECTION_RATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CollectionRate_MaxLength)]
    
  public string CollectionRate
  {
    get => collectionRate ?? "";
    set => collectionRate =
      TrimEnd(Substring(value, 1, CollectionRate_MaxLength));
  }

  /// <summary>
  /// The json value of the CollectionRate attribute.</summary>
  [JsonPropertyName("collectionRate")]
  [Computed]
  public string CollectionRate_Json
  {
    get => NullIf(CollectionRate, "");
    set => CollectionRate = value;
  }

  /// <summary>Length of the CASE_PAYING_ARREARS attribute.</summary>
  public const int CasePayingArrears_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_PAYING_ARREARS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = CasePayingArrears_MaxLength)]
  public string CasePayingArrears
  {
    get => casePayingArrears ?? "";
    set => casePayingArrears =
      TrimEnd(Substring(value, 1, CasePayingArrears_MaxLength));
  }

  /// <summary>
  /// The json value of the CasePayingArrears attribute.</summary>
  [JsonPropertyName("casePayingArrears")]
  [Computed]
  public string CasePayingArrears_Json
  {
    get => NullIf(CasePayingArrears, "");
    set => CasePayingArrears = value;
  }

  /// <summary>Length of the CASE_FUNCTION attribute.</summary>
  public const int CaseFunction_MaxLength = 3;

  /// <summary>
  /// The value of the CASE_FUNCTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = CaseFunction_MaxLength)]
  public string CaseFunction
  {
    get => caseFunction ?? "";
    set => caseFunction = TrimEnd(Substring(value, 1, CaseFunction_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseFunction attribute.</summary>
  [JsonPropertyName("caseFunction")]
  [Computed]
  public string CaseFunction_Json
  {
    get => NullIf(CaseFunction, "");
    set => CaseFunction = value;
  }

  /// <summary>Length of the CASE_PROGRAM attribute.</summary>
  public const int CaseProgram_MaxLength = 3;

  /// <summary>
  /// The value of the CASE_PROGRAM attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CaseProgram_MaxLength)]
  public string CaseProgram
  {
    get => caseProgram ?? "";
    set => caseProgram = TrimEnd(Substring(value, 1, CaseProgram_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseProgram attribute.</summary>
  [JsonPropertyName("caseProgram")]
  [Computed]
  public string CaseProgram_Json
  {
    get => NullIf(CaseProgram, "");
    set => CaseProgram = value;
  }

  /// <summary>Length of the PA_PROGRAM_END_DATE attribute.</summary>
  public const int PaProgramEndDate_MaxLength = 10;

  /// <summary>
  /// The value of the PA_PROGRAM_END_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = PaProgramEndDate_MaxLength)]
  public string PaProgramEndDate
  {
    get => paProgramEndDate ?? "";
    set => paProgramEndDate =
      TrimEnd(Substring(value, 1, PaProgramEndDate_MaxLength));
  }

  /// <summary>
  /// The json value of the PaProgramEndDate attribute.</summary>
  [JsonPropertyName("paProgramEndDate")]
  [Computed]
  public string PaProgramEndDate_Json
  {
    get => NullIf(PaProgramEndDate, "");
    set => PaProgramEndDate = value;
  }

  /// <summary>Length of the CURA_AMOUNT attribute.</summary>
  public const int CuraAmount_MaxLength = 15;

  /// <summary>
  /// The value of the CURA_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CuraAmount_MaxLength)]
  public string CuraAmount
  {
    get => curaAmount ?? "";
    set => curaAmount = TrimEnd(Substring(value, 1, CuraAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the CuraAmount attribute.</summary>
  [JsonPropertyName("curaAmount")]
  [Computed]
  public string CuraAmount_Json
  {
    get => NullIf(CuraAmount, "");
    set => CuraAmount = value;
  }

  /// <summary>Length of the FAMILY_VIOLENCE attribute.</summary>
  public const int FamilyViolence_MaxLength = 1;

  /// <summary>
  /// The value of the FAMILY_VIOLENCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = FamilyViolence_MaxLength)]
    
  public string FamilyViolence
  {
    get => familyViolence ?? "";
    set => familyViolence =
      TrimEnd(Substring(value, 1, FamilyViolence_MaxLength));
  }

  /// <summary>
  /// The json value of the FamilyViolence attribute.</summary>
  [JsonPropertyName("familyViolence")]
  [Computed]
  public string FamilyViolence_Json
  {
    get => NullIf(FamilyViolence, "");
    set => FamilyViolence = value;
  }

  /// <summary>Length of the CP_NONCOOPERATION attribute.</summary>
  public const int CpNoncooperation_MaxLength = 1;

  /// <summary>
  /// The value of the CP_NONCOOPERATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = CpNoncooperation_MaxLength)]
  public string CpNoncooperation
  {
    get => cpNoncooperation ?? "";
    set => cpNoncooperation =
      TrimEnd(Substring(value, 1, CpNoncooperation_MaxLength));
  }

  /// <summary>
  /// The json value of the CpNoncooperation attribute.</summary>
  [JsonPropertyName("cpNoncooperation")]
  [Computed]
  public string CpNoncooperation_Json
  {
    get => NullIf(CpNoncooperation, "");
    set => CpNoncooperation = value;
  }

  /// <summary>Length of the PENDING_CASE_CLOSURE_DATE attribute.</summary>
  public const int PendingCaseClosureDate_MaxLength = 10;

  /// <summary>
  /// The value of the PENDING_CASE_CLOSURE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = PendingCaseClosureDate_MaxLength)]
  public string PendingCaseClosureDate
  {
    get => pendingCaseClosureDate ?? "";
    set => pendingCaseClosureDate =
      TrimEnd(Substring(value, 1, PendingCaseClosureDate_MaxLength));
  }

  /// <summary>
  /// The json value of the PendingCaseClosureDate attribute.</summary>
  [JsonPropertyName("pendingCaseClosureDate")]
  [Computed]
  public string PendingCaseClosureDate_Json
  {
    get => NullIf(PendingCaseClosureDate, "");
    set => PendingCaseClosureDate = value;
  }

  /// <summary>Length of the DATE_OF_EMANCIPATION attribute.</summary>
  public const int DateOfEmancipation_MaxLength = 10;

  /// <summary>
  /// The value of the DATE_OF_EMANCIPATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = DateOfEmancipation_MaxLength)]
  public string DateOfEmancipation
  {
    get => dateOfEmancipation ?? "";
    set => dateOfEmancipation =
      TrimEnd(Substring(value, 1, DateOfEmancipation_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfEmancipation attribute.</summary>
  [JsonPropertyName("dateOfEmancipation")]
  [Computed]
  public string DateOfEmancipation_Json
  {
    get => NullIf(DateOfEmancipation, "");
    set => DateOfEmancipation = value;
  }

  /// <summary>Length of the YOUNGEST_EMANCIPATION_DATE attribute.</summary>
  public const int YoungestEmancipationDate_MaxLength = 10;

  /// <summary>
  /// The value of the YOUNGEST_EMANCIPATION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = YoungestEmancipationDate_MaxLength)]
  public string YoungestEmancipationDate
  {
    get => youngestEmancipationDate ?? "";
    set => youngestEmancipationDate =
      TrimEnd(Substring(value, 1, YoungestEmancipationDate_MaxLength));
  }

  /// <summary>
  /// The json value of the YoungestEmancipationDate attribute.</summary>
  [JsonPropertyName("youngestEmancipationDate")]
  [Computed]
  public string YoungestEmancipationDate_Json
  {
    get => NullIf(YoungestEmancipationDate, "");
    set => YoungestEmancipationDate = value;
  }

  /// <summary>Length of the CHILD_BOW attribute.</summary>
  public const int ChildBow_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_BOW attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = ChildBow_MaxLength)]
  public string ChildBow
  {
    get => childBow ?? "";
    set => childBow = TrimEnd(Substring(value, 1, ChildBow_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildBow attribute.</summary>
  [JsonPropertyName("childBow")]
  [Computed]
  public string ChildBow_Json
  {
    get => NullIf(ChildBow, "");
    set => ChildBow = value;
  }

  /// <summary>Length of the CP_DATE_OF_BIRTH attribute.</summary>
  public const int CpDateOfBirth_MaxLength = 10;

  /// <summary>
  /// The value of the CP_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = CpDateOfBirth_MaxLength)]
  public string CpDateOfBirth
  {
    get => cpDateOfBirth ?? "";
    set => cpDateOfBirth =
      TrimEnd(Substring(value, 1, CpDateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the CpDateOfBirth attribute.</summary>
  [JsonPropertyName("cpDateOfBirth")]
  [Computed]
  public string CpDateOfBirth_Json
  {
    get => NullIf(CpDateOfBirth, "");
    set => CpDateOfBirth = value;
  }

  /// <summary>Length of the CP_ETHNICITY attribute.</summary>
  public const int CpEthnicity_MaxLength = 2;

  /// <summary>
  /// The value of the CP_ETHNICITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = CpEthnicity_MaxLength)]
  public string CpEthnicity
  {
    get => cpEthnicity ?? "";
    set => cpEthnicity = TrimEnd(Substring(value, 1, CpEthnicity_MaxLength));
  }

  /// <summary>
  /// The json value of the CpEthnicity attribute.</summary>
  [JsonPropertyName("cpEthnicity")]
  [Computed]
  public string CpEthnicity_Json
  {
    get => NullIf(CpEthnicity, "");
    set => CpEthnicity = value;
  }

  /// <summary>Length of the CP_ZIP_CODE attribute.</summary>
  public const int CpZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the CP_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = CpZipCode_MaxLength)]
  public string CpZipCode
  {
    get => cpZipCode ?? "";
    set => cpZipCode = TrimEnd(Substring(value, 1, CpZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CpZipCode attribute.</summary>
  [JsonPropertyName("cpZipCode")]
  [Computed]
  public string CpZipCode_Json
  {
    get => NullIf(CpZipCode, "");
    set => CpZipCode = value;
  }

  /// <summary>Length of the CP_HOME_PHONE_AREA_CODE attribute.</summary>
  public const int CpHomePhoneAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the CP_HOME_PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = CpHomePhoneAreaCode_MaxLength)]
  public string CpHomePhoneAreaCode
  {
    get => cpHomePhoneAreaCode ?? "";
    set => cpHomePhoneAreaCode =
      TrimEnd(Substring(value, 1, CpHomePhoneAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CpHomePhoneAreaCode attribute.</summary>
  [JsonPropertyName("cpHomePhoneAreaCode")]
  [Computed]
  public string CpHomePhoneAreaCode_Json
  {
    get => NullIf(CpHomePhoneAreaCode, "");
    set => CpHomePhoneAreaCode = value;
  }

  /// <summary>Length of the CP_HOME_PHONE attribute.</summary>
  public const int CpHomePhone_MaxLength = 7;

  /// <summary>
  /// The value of the CP_HOME_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = CpHomePhone_MaxLength)]
  public string CpHomePhone
  {
    get => cpHomePhone ?? "";
    set => cpHomePhone = TrimEnd(Substring(value, 1, CpHomePhone_MaxLength));
  }

  /// <summary>
  /// The json value of the CpHomePhone attribute.</summary>
  [JsonPropertyName("cpHomePhone")]
  [Computed]
  public string CpHomePhone_Json
  {
    get => NullIf(CpHomePhone, "");
    set => CpHomePhone = value;
  }

  /// <summary>Length of the CP_CELL_PHONE_AREA_CODE attribute.</summary>
  public const int CpCellPhoneAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the CP_CELL_PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = CpCellPhoneAreaCode_MaxLength)]
  public string CpCellPhoneAreaCode
  {
    get => cpCellPhoneAreaCode ?? "";
    set => cpCellPhoneAreaCode =
      TrimEnd(Substring(value, 1, CpCellPhoneAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CpCellPhoneAreaCode attribute.</summary>
  [JsonPropertyName("cpCellPhoneAreaCode")]
  [Computed]
  public string CpCellPhoneAreaCode_Json
  {
    get => NullIf(CpCellPhoneAreaCode, "");
    set => CpCellPhoneAreaCode = value;
  }

  /// <summary>Length of the CP_CELL_PHONE attribute.</summary>
  public const int CpCellPhone_MaxLength = 7;

  /// <summary>
  /// The value of the CP_CELL_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = CpCellPhone_MaxLength)]
  public string CpCellPhone
  {
    get => cpCellPhone ?? "";
    set => cpCellPhone = TrimEnd(Substring(value, 1, CpCellPhone_MaxLength));
  }

  /// <summary>
  /// The json value of the CpCellPhone attribute.</summary>
  [JsonPropertyName("cpCellPhone")]
  [Computed]
  public string CpCellPhone_Json
  {
    get => NullIf(CpCellPhone, "");
    set => CpCellPhone = value;
  }

  /// <summary>Length of the NCP_PERSON_NUMBER attribute.</summary>
  public const int NcpPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NCP_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = NcpPersonNumber_MaxLength)
    ]
  public string NcpPersonNumber
  {
    get => ncpPersonNumber ?? "";
    set => ncpPersonNumber =
      TrimEnd(Substring(value, 1, NcpPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpPersonNumber attribute.</summary>
  [JsonPropertyName("ncpPersonNumber")]
  [Computed]
  public string NcpPersonNumber_Json
  {
    get => NullIf(NcpPersonNumber, "");
    set => NcpPersonNumber = value;
  }

  /// <summary>Length of the NCP_LAST_NAME attribute.</summary>
  public const int NcpLastName_MaxLength = 17;

  /// <summary>
  /// The value of the NCP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = NcpLastName_MaxLength)]
  public string NcpLastName
  {
    get => ncpLastName ?? "";
    set => ncpLastName = TrimEnd(Substring(value, 1, NcpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpLastName attribute.</summary>
  [JsonPropertyName("ncpLastName")]
  [Computed]
  public string NcpLastName_Json
  {
    get => NullIf(NcpLastName, "");
    set => NcpLastName = value;
  }

  /// <summary>Length of the NCP_FIRST_NAME attribute.</summary>
  public const int NcpFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the NCP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = NcpFirstName_MaxLength)]
  public string NcpFirstName
  {
    get => ncpFirstName ?? "";
    set => ncpFirstName = TrimEnd(Substring(value, 1, NcpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpFirstName attribute.</summary>
  [JsonPropertyName("ncpFirstName")]
  [Computed]
  public string NcpFirstName_Json
  {
    get => NullIf(NcpFirstName, "");
    set => NcpFirstName = value;
  }

  /// <summary>Length of the NCP_DATE_OF_BIRTH attribute.</summary>
  public const int NcpDateOfBirth_MaxLength = 10;

  /// <summary>
  /// The value of the NCP_DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = NcpDateOfBirth_MaxLength)]
    
  public string NcpDateOfBirth
  {
    get => ncpDateOfBirth ?? "";
    set => ncpDateOfBirth =
      TrimEnd(Substring(value, 1, NcpDateOfBirth_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpDateOfBirth attribute.</summary>
  [JsonPropertyName("ncpDateOfBirth")]
  [Computed]
  public string NcpDateOfBirth_Json
  {
    get => NullIf(NcpDateOfBirth, "");
    set => NcpDateOfBirth = value;
  }

  /// <summary>Length of the NCP_ETHNICITY attribute.</summary>
  public const int NcpEthnicity_MaxLength = 2;

  /// <summary>
  /// The value of the NCP_ETHNICITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = NcpEthnicity_MaxLength)]
  public string NcpEthnicity
  {
    get => ncpEthnicity ?? "";
    set => ncpEthnicity = TrimEnd(Substring(value, 1, NcpEthnicity_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpEthnicity attribute.</summary>
  [JsonPropertyName("ncpEthnicity")]
  [Computed]
  public string NcpEthnicity_Json
  {
    get => NullIf(NcpEthnicity, "");
    set => NcpEthnicity = value;
  }

  /// <summary>Length of the NCP_LOCATE_DATE attribute.</summary>
  public const int NcpLocateDate_MaxLength = 10;

  /// <summary>
  /// The value of the NCP_LOCATE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = NcpLocateDate_MaxLength)]
  public string NcpLocateDate
  {
    get => ncpLocateDate ?? "";
    set => ncpLocateDate =
      TrimEnd(Substring(value, 1, NcpLocateDate_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpLocateDate attribute.</summary>
  [JsonPropertyName("ncpLocateDate")]
  [Computed]
  public string NcpLocateDate_Json
  {
    get => NullIf(NcpLocateDate, "");
    set => NcpLocateDate = value;
  }

  /// <summary>Length of the NCP_ZIP_CODE attribute.</summary>
  public const int NcpZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the NCP_ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = NcpZipCode_MaxLength)]
  public string NcpZipCode
  {
    get => ncpZipCode ?? "";
    set => ncpZipCode = TrimEnd(Substring(value, 1, NcpZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpZipCode attribute.</summary>
  [JsonPropertyName("ncpZipCode")]
  [Computed]
  public string NcpZipCode_Json
  {
    get => NullIf(NcpZipCode, "");
    set => NcpZipCode = value;
  }

  /// <summary>Length of the NCP_FOREIGN_COUNTRY_CODE attribute.</summary>
  public const int NcpForeignCountryCode_MaxLength = 2;

  /// <summary>
  /// The value of the NCP_FOREIGN_COUNTRY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = NcpForeignCountryCode_MaxLength)]
  public string NcpForeignCountryCode
  {
    get => ncpForeignCountryCode ?? "";
    set => ncpForeignCountryCode =
      TrimEnd(Substring(value, 1, NcpForeignCountryCode_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpForeignCountryCode attribute.</summary>
  [JsonPropertyName("ncpForeignCountryCode")]
  [Computed]
  public string NcpForeignCountryCode_Json
  {
    get => NullIf(NcpForeignCountryCode, "");
    set => NcpForeignCountryCode = value;
  }

  /// <summary>Length of the NCP_HOME_PHONE_AREA_CODE attribute.</summary>
  public const int NcpHomePhoneAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the NCP_HOME_PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = NcpHomePhoneAreaCode_MaxLength)]
  public string NcpHomePhoneAreaCode
  {
    get => ncpHomePhoneAreaCode ?? "";
    set => ncpHomePhoneAreaCode =
      TrimEnd(Substring(value, 1, NcpHomePhoneAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpHomePhoneAreaCode attribute.</summary>
  [JsonPropertyName("ncpHomePhoneAreaCode")]
  [Computed]
  public string NcpHomePhoneAreaCode_Json
  {
    get => NullIf(NcpHomePhoneAreaCode, "");
    set => NcpHomePhoneAreaCode = value;
  }

  /// <summary>Length of the NCP_HOME_PHONE attribute.</summary>
  public const int NcpHomePhone_MaxLength = 7;

  /// <summary>
  /// The value of the NCP_HOME_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = NcpHomePhone_MaxLength)]
  public string NcpHomePhone
  {
    get => ncpHomePhone ?? "";
    set => ncpHomePhone = TrimEnd(Substring(value, 1, NcpHomePhone_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpHomePhone attribute.</summary>
  [JsonPropertyName("ncpHomePhone")]
  [Computed]
  public string NcpHomePhone_Json
  {
    get => NullIf(NcpHomePhone, "");
    set => NcpHomePhone = value;
  }

  /// <summary>Length of the NCP_CELL_PHONE_AREA_CODE attribute.</summary>
  public const int NcpCellPhoneAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the NCP_CELL_PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = NcpCellPhoneAreaCode_MaxLength)]
  public string NcpCellPhoneAreaCode
  {
    get => ncpCellPhoneAreaCode ?? "";
    set => ncpCellPhoneAreaCode =
      TrimEnd(Substring(value, 1, NcpCellPhoneAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpCellPhoneAreaCode attribute.</summary>
  [JsonPropertyName("ncpCellPhoneAreaCode")]
  [Computed]
  public string NcpCellPhoneAreaCode_Json
  {
    get => NullIf(NcpCellPhoneAreaCode, "");
    set => NcpCellPhoneAreaCode = value;
  }

  /// <summary>Length of the NCP_CELL_PHONE attribute.</summary>
  public const int NcpCellPhone_MaxLength = 7;

  /// <summary>
  /// The value of the NCP_CELL_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = NcpCellPhone_MaxLength)]
  public string NcpCellPhone
  {
    get => ncpCellPhone ?? "";
    set => ncpCellPhone = TrimEnd(Substring(value, 1, NcpCellPhone_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpCellPhone attribute.</summary>
  [JsonPropertyName("ncpCellPhone")]
  [Computed]
  public string NcpCellPhone_Json
  {
    get => NullIf(NcpCellPhone, "");
    set => NcpCellPhone = value;
  }

  /// <summary>Length of the NCP_INCARCERATED attribute.</summary>
  public const int NcpIncarcerated_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_INCARCERATED attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length = NcpIncarcerated_MaxLength)
    ]
  public string NcpIncarcerated
  {
    get => ncpIncarcerated ?? "";
    set => ncpIncarcerated =
      TrimEnd(Substring(value, 1, NcpIncarcerated_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpIncarcerated attribute.</summary>
  [JsonPropertyName("ncpIncarcerated")]
  [Computed]
  public string NcpIncarcerated_Json
  {
    get => NullIf(NcpIncarcerated, "");
    set => NcpIncarcerated = value;
  }

  /// <summary>Length of the NCP_IN_BANKRUPTCY attribute.</summary>
  public const int NcpInBankruptcy_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_IN_BANKRUPTCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = NcpInBankruptcy_MaxLength)
    ]
  public string NcpInBankruptcy
  {
    get => ncpInBankruptcy ?? "";
    set => ncpInBankruptcy =
      TrimEnd(Substring(value, 1, NcpInBankruptcy_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpInBankruptcy attribute.</summary>
  [JsonPropertyName("ncpInBankruptcy")]
  [Computed]
  public string NcpInBankruptcy_Json
  {
    get => NullIf(NcpInBankruptcy, "");
    set => NcpInBankruptcy = value;
  }

  /// <summary>Length of the NCP_REPRESENTED_BY_COUNCIL attribute.</summary>
  public const int NcpRepresentedByCouncil_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_REPRESENTED_BY_COUNCIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = NcpRepresentedByCouncil_MaxLength)]
  public string NcpRepresentedByCouncil
  {
    get => ncpRepresentedByCouncil ?? "";
    set => ncpRepresentedByCouncil =
      TrimEnd(Substring(value, 1, NcpRepresentedByCouncil_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpRepresentedByCouncil attribute.</summary>
  [JsonPropertyName("ncpRepresentedByCouncil")]
  [Computed]
  public string NcpRepresentedByCouncil_Json
  {
    get => NullIf(NcpRepresentedByCouncil, "");
    set => NcpRepresentedByCouncil = value;
  }

  /// <summary>Length of the NCP_EMPLOYER_NAME attribute.</summary>
  public const int NcpEmployerName_MaxLength = 36;

  /// <summary>
  /// The value of the NCP_EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = NcpEmployerName_MaxLength)
    ]
  public string NcpEmployerName
  {
    get => ncpEmployerName ?? "";
    set => ncpEmployerName =
      TrimEnd(Substring(value, 1, NcpEmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpEmployerName attribute.</summary>
  [JsonPropertyName("ncpEmployerName")]
  [Computed]
  public string NcpEmployerName_Json
  {
    get => NullIf(NcpEmployerName, "");
    set => NcpEmployerName = value;
  }

  /// <summary>Length of the NCP_OTHER_INCOME_SOURCE attribute.</summary>
  public const int NcpOtherIncomeSource_MaxLength = 36;

  /// <summary>
  /// The value of the NCP_OTHER_INCOME_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = NcpOtherIncomeSource_MaxLength)]
  public string NcpOtherIncomeSource
  {
    get => ncpOtherIncomeSource ?? "";
    set => ncpOtherIncomeSource =
      TrimEnd(Substring(value, 1, NcpOtherIncomeSource_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpOtherIncomeSource attribute.</summary>
  [JsonPropertyName("ncpOtherIncomeSource")]
  [Computed]
  public string NcpOtherIncomeSource_Json
  {
    get => NullIf(NcpOtherIncomeSource, "");
    set => NcpOtherIncomeSource = value;
  }

  /// <summary>Length of the NCP_INTERSTATE_INITIATING attribute.</summary>
  public const int NcpInterstateInitiating_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_INTERSTATE_INITIATING attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = NcpInterstateInitiating_MaxLength)]
  public string NcpInterstateInitiating
  {
    get => ncpInterstateInitiating ?? "";
    set => ncpInterstateInitiating =
      TrimEnd(Substring(value, 1, NcpInterstateInitiating_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpInterstateInitiating attribute.</summary>
  [JsonPropertyName("ncpInterstateInitiating")]
  [Computed]
  public string NcpInterstateInitiating_Json
  {
    get => NullIf(NcpInterstateInitiating, "");
    set => NcpInterstateInitiating = value;
  }

  /// <summary>Length of the NCP_INTERSTATE_RESPONDING attribute.</summary>
  public const int NcpInterstateResponding_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_INTERSTATE_RESPONDING attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = NcpInterstateResponding_MaxLength)]
  public string NcpInterstateResponding
  {
    get => ncpInterstateResponding ?? "";
    set => ncpInterstateResponding =
      TrimEnd(Substring(value, 1, NcpInterstateResponding_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpInterstateResponding attribute.</summary>
  [JsonPropertyName("ncpInterstateResponding")]
  [Computed]
  public string NcpInterstateResponding_Json
  {
    get => NullIf(NcpInterstateResponding, "");
    set => NcpInterstateResponding = value;
  }

  /// <summary>Length of the CO_COURT_ORDER_NUMBER attribute.</summary>
  public const int CoCourtOrderNumber_MaxLength = 12;

  /// <summary>
  /// The value of the CO_COURT_ORDER_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = CoCourtOrderNumber_MaxLength)]
  public string CoCourtOrderNumber
  {
    get => coCourtOrderNumber ?? "";
    set => coCourtOrderNumber =
      TrimEnd(Substring(value, 1, CoCourtOrderNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CoCourtOrderNumber attribute.</summary>
  [JsonPropertyName("coCourtOrderNumber")]
  [Computed]
  public string CoCourtOrderNumber_Json
  {
    get => NullIf(CoCourtOrderNumber, "");
    set => CoCourtOrderNumber = value;
  }

  /// <summary>Length of the CO_CS_COLLECTED_IN_MONTH attribute.</summary>
  public const int CoCsCollectedInMonth_MaxLength = 15;

  /// <summary>
  /// The value of the CO_CS_COLLECTED_IN_MONTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = CoCsCollectedInMonth_MaxLength)]
  public string CoCsCollectedInMonth
  {
    get => coCsCollectedInMonth ?? "";
    set => coCsCollectedInMonth =
      TrimEnd(Substring(value, 1, CoCsCollectedInMonth_MaxLength));
  }

  /// <summary>
  /// The json value of the CoCsCollectedInMonth attribute.</summary>
  [JsonPropertyName("coCsCollectedInMonth")]
  [Computed]
  public string CoCsCollectedInMonth_Json
  {
    get => NullIf(CoCsCollectedInMonth, "");
    set => CoCsCollectedInMonth = value;
  }

  /// <summary>Length of the CO_CS_DUE_IN_MONTH attribute.</summary>
  public const int CoCsDueInMonth_MaxLength = 15;

  /// <summary>
  /// The value of the CO_CS_DUE_IN_MONTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = CoCsDueInMonth_MaxLength)]
    
  public string CoCsDueInMonth
  {
    get => coCsDueInMonth ?? "";
    set => coCsDueInMonth =
      TrimEnd(Substring(value, 1, CoCsDueInMonth_MaxLength));
  }

  /// <summary>
  /// The json value of the CoCsDueInMonth attribute.</summary>
  [JsonPropertyName("coCsDueInMonth")]
  [Computed]
  public string CoCsDueInMonth_Json
  {
    get => NullIf(CoCsDueInMonth, "");
    set => CoCsDueInMonth = value;
  }

  /// <summary>Length of the CO_CS_COLLECTED_FFYTD attribute.</summary>
  public const int CoCsCollectedFfytd_MaxLength = 15;

  /// <summary>
  /// The value of the CO_CS_COLLECTED_FFYTD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = CoCsCollectedFfytd_MaxLength)]
  public string CoCsCollectedFfytd
  {
    get => coCsCollectedFfytd ?? "";
    set => coCsCollectedFfytd =
      TrimEnd(Substring(value, 1, CoCsCollectedFfytd_MaxLength));
  }

  /// <summary>
  /// The json value of the CoCsCollectedFfytd attribute.</summary>
  [JsonPropertyName("coCsCollectedFfytd")]
  [Computed]
  public string CoCsCollectedFfytd_Json
  {
    get => NullIf(CoCsCollectedFfytd, "");
    set => CoCsCollectedFfytd = value;
  }

  /// <summary>Length of the CO_CS_DUE_FFYTD attribute.</summary>
  public const int CoCsDueFfytd_MaxLength = 15;

  /// <summary>
  /// The value of the CO_CS_DUE_FFYTD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = CoCsDueFfytd_MaxLength)]
  public string CoCsDueFfytd
  {
    get => coCsDueFfytd ?? "";
    set => coCsDueFfytd = TrimEnd(Substring(value, 1, CoCsDueFfytd_MaxLength));
  }

  /// <summary>
  /// The json value of the CoCsDueFfytd attribute.</summary>
  [JsonPropertyName("coCsDueFfytd")]
  [Computed]
  public string CoCsDueFfytd_Json
  {
    get => NullIf(CoCsDueFfytd, "");
    set => CoCsDueFfytd = value;
  }

  /// <summary>Length of the CO_TOTAL_ARREARS_AMOUNT_DUE attribute.</summary>
  public const int CoTotalArrearsAmountDue_MaxLength = 15;

  /// <summary>
  /// The value of the CO_TOTAL_ARREARS_AMOUNT_DUE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = CoTotalArrearsAmountDue_MaxLength)]
  public string CoTotalArrearsAmountDue
  {
    get => coTotalArrearsAmountDue ?? "";
    set => coTotalArrearsAmountDue =
      TrimEnd(Substring(value, 1, CoTotalArrearsAmountDue_MaxLength));
  }

  /// <summary>
  /// The json value of the CoTotalArrearsAmountDue attribute.</summary>
  [JsonPropertyName("coTotalArrearsAmountDue")]
  [Computed]
  public string CoTotalArrearsAmountDue_Json
  {
    get => NullIf(CoTotalArrearsAmountDue, "");
    set => CoTotalArrearsAmountDue = value;
  }

  /// <summary>Length of the CO_ARREARS_PAID_IN_MONTH attribute.</summary>
  public const int CoArrearsPaidInMonth_MaxLength = 15;

  /// <summary>
  /// The value of the CO_ARREARS_PAID_IN_MONTH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = CoArrearsPaidInMonth_MaxLength)]
  public string CoArrearsPaidInMonth
  {
    get => coArrearsPaidInMonth ?? "";
    set => coArrearsPaidInMonth =
      TrimEnd(Substring(value, 1, CoArrearsPaidInMonth_MaxLength));
  }

  /// <summary>
  /// The json value of the CoArrearsPaidInMonth attribute.</summary>
  [JsonPropertyName("coArrearsPaidInMonth")]
  [Computed]
  public string CoArrearsPaidInMonth_Json
  {
    get => NullIf(CoArrearsPaidInMonth, "");
    set => CoArrearsPaidInMonth = value;
  }

  /// <summary>Length of the CO_ARREARS_PAID_FFYTD attribute.</summary>
  public const int CoArrearsPaidFfytd_MaxLength = 15;

  /// <summary>
  /// The value of the CO_ARREARS_PAID_FFYTD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length
    = CoArrearsPaidFfytd_MaxLength)]
  public string CoArrearsPaidFfytd
  {
    get => coArrearsPaidFfytd ?? "";
    set => coArrearsPaidFfytd =
      TrimEnd(Substring(value, 1, CoArrearsPaidFfytd_MaxLength));
  }

  /// <summary>
  /// The json value of the CoArrearsPaidFfytd attribute.</summary>
  [JsonPropertyName("coArrearsPaidFfytd")]
  [Computed]
  public string CoArrearsPaidFfytd_Json
  {
    get => NullIf(CoArrearsPaidFfytd, "");
    set => CoArrearsPaidFfytd = value;
  }

  /// <summary>Length of the CO_LAST_PAYMENT_AMOUNT attribute.</summary>
  public const int CoLastPaymentAmount_MaxLength = 15;

  /// <summary>
  /// The value of the CO_LAST_PAYMENT_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = CoLastPaymentAmount_MaxLength)]
  public string CoLastPaymentAmount
  {
    get => coLastPaymentAmount ?? "";
    set => coLastPaymentAmount =
      TrimEnd(Substring(value, 1, CoLastPaymentAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastPaymentAmount attribute.</summary>
  [JsonPropertyName("coLastPaymentAmount")]
  [Computed]
  public string CoLastPaymentAmount_Json
  {
    get => NullIf(CoLastPaymentAmount, "");
    set => CoLastPaymentAmount = value;
  }

  /// <summary>Length of the CO_LAST_PAYMENT_DATE attribute.</summary>
  public const int CoLastPaymentDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_LAST_PAYMENT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = CoLastPaymentDate_MaxLength)]
  public string CoLastPaymentDate
  {
    get => coLastPaymentDate ?? "";
    set => coLastPaymentDate =
      TrimEnd(Substring(value, 1, CoLastPaymentDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastPaymentDate attribute.</summary>
  [JsonPropertyName("coLastPaymentDate")]
  [Computed]
  public string CoLastPaymentDate_Json
  {
    get => NullIf(CoLastPaymentDate, "");
    set => CoLastPaymentDate = value;
  }

  /// <summary>Length of the CO_LAST_PAYMENT_TYPE attribute.</summary>
  public const int CoLastPaymentType_MaxLength = 1;

  /// <summary>
  /// The value of the CO_LAST_PAYMENT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = CoLastPaymentType_MaxLength)]
  public string CoLastPaymentType
  {
    get => coLastPaymentType ?? "";
    set => coLastPaymentType =
      TrimEnd(Substring(value, 1, CoLastPaymentType_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastPaymentType attribute.</summary>
  [JsonPropertyName("coLastPaymentType")]
  [Computed]
  public string CoLastPaymentType_Json
  {
    get => NullIf(CoLastPaymentType, "");
    set => CoLastPaymentType = value;
  }

  /// <summary>Length of the CO_LAST_DSO_PAYMENT_DATE attribute.</summary>
  public const int CoLastDsoPaymentDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_LAST_DSO_PAYMENT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length
    = CoLastDsoPaymentDate_MaxLength)]
  public string CoLastDsoPaymentDate
  {
    get => coLastDsoPaymentDate ?? "";
    set => coLastDsoPaymentDate =
      TrimEnd(Substring(value, 1, CoLastDsoPaymentDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastDsoPaymentDate attribute.</summary>
  [JsonPropertyName("coLastDsoPaymentDate")]
  [Computed]
  public string CoLastDsoPaymentDate_Json
  {
    get => NullIf(CoLastDsoPaymentDate, "");
    set => CoLastDsoPaymentDate = value;
  }

  /// <summary>Length of the CO_LAST_I_CLASS_CREATED_DATE attribute.</summary>
  public const int CoLastIClassCreatedDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_LAST_I_CLASS_CREATED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = CoLastIClassCreatedDate_MaxLength)]
  public string CoLastIClassCreatedDate
  {
    get => coLastIClassCreatedDate ?? "";
    set => coLastIClassCreatedDate =
      TrimEnd(Substring(value, 1, CoLastIClassCreatedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastIClassCreatedDate attribute.</summary>
  [JsonPropertyName("coLastIClassCreatedDate")]
  [Computed]
  public string CoLastIClassCreatedDate_Json
  {
    get => NullIf(CoLastIClassCreatedDate, "");
    set => CoLastIClassCreatedDate = value;
  }

  /// <summary>Length of the CO_LAST_I_CLASS_ACTION_TAKEN attribute.</summary>
  public const int CoLastIClassActionTaken_MaxLength = 8;

  /// <summary>
  /// The value of the CO_LAST_I_CLASS_ACTION_TAKEN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = CoLastIClassActionTaken_MaxLength)]
  public string CoLastIClassActionTaken
  {
    get => coLastIClassActionTaken ?? "";
    set => coLastIClassActionTaken =
      TrimEnd(Substring(value, 1, CoLastIClassActionTaken_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastIClassActionTaken attribute.</summary>
  [JsonPropertyName("coLastIClassActionTaken")]
  [Computed]
  public string CoLastIClassActionTaken_Json
  {
    get => NullIf(CoLastIClassActionTaken, "");
    set => CoLastIClassActionTaken = value;
  }

  /// <summary>Length of the CO_LAST_I_CLASS_FILED_DATE attribute.</summary>
  public const int CoLastIClassFiledDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_LAST_I_CLASS_FILED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = CoLastIClassFiledDate_MaxLength)]
  public string CoLastIClassFiledDate
  {
    get => coLastIClassFiledDate ?? "";
    set => coLastIClassFiledDate =
      TrimEnd(Substring(value, 1, CoLastIClassFiledDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastIClassFiledDate attribute.</summary>
  [JsonPropertyName("coLastIClassFiledDate")]
  [Computed]
  public string CoLastIClassFiledDate_Json
  {
    get => NullIf(CoLastIClassFiledDate, "");
    set => CoLastIClassFiledDate = value;
  }

  /// <summary>Length of the CO_LAST_I_CLASS_IWGL attribute.</summary>
  public const int CoLastIClassIwgl_MaxLength = 1;

  /// <summary>
  /// The value of the CO_LAST_I_CLASS_IWGL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = CoLastIClassIwgl_MaxLength)]
  public string CoLastIClassIwgl
  {
    get => coLastIClassIwgl ?? "";
    set => coLastIClassIwgl =
      TrimEnd(Substring(value, 1, CoLastIClassIwgl_MaxLength));
  }

  /// <summary>
  /// The json value of the CoLastIClassIwgl attribute.</summary>
  [JsonPropertyName("coLastIClassIwgl")]
  [Computed]
  public string CoLastIClassIwgl_Json
  {
    get => NullIf(CoLastIClassIwgl, "");
    set => CoLastIClassIwgl = value;
  }

  /// <summary>Length of the CO_MONTHLY_IWO_WA_AMOUNT attribute.</summary>
  public const int CoMonthlyIwoWaAmount_MaxLength = 15;

  /// <summary>
  /// The value of the CO_MONTHLY_IWO_WA_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = CoMonthlyIwoWaAmount_MaxLength)]
  public string CoMonthlyIwoWaAmount
  {
    get => coMonthlyIwoWaAmount ?? "";
    set => coMonthlyIwoWaAmount =
      TrimEnd(Substring(value, 1, CoMonthlyIwoWaAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the CoMonthlyIwoWaAmount attribute.</summary>
  [JsonPropertyName("coMonthlyIwoWaAmount")]
  [Computed]
  public string CoMonthlyIwoWaAmount_Json
  {
    get => NullIf(CoMonthlyIwoWaAmount, "");
    set => CoMonthlyIwoWaAmount = value;
  }

  /// <summary>Length of the CO_CONTEMPT_HEARING_DATE attribute.</summary>
  public const int CoContemptHearingDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_CONTEMPT_HEARING_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = CoContemptHearingDate_MaxLength)]
  public string CoContemptHearingDate
  {
    get => coContemptHearingDate ?? "";
    set => coContemptHearingDate =
      TrimEnd(Substring(value, 1, CoContemptHearingDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoContemptHearingDate attribute.</summary>
  [JsonPropertyName("coContemptHearingDate")]
  [Computed]
  public string CoContemptHearingDate_Json
  {
    get => NullIf(CoContemptHearingDate, "");
    set => CoContemptHearingDate = value;
  }

  /// <summary>Length of the CO_CONTEMPT_SERVICE_DATE attribute.</summary>
  public const int CoContemptServiceDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_CONTEMPT_SERVICE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = CoContemptServiceDate_MaxLength)]
  public string CoContemptServiceDate
  {
    get => coContemptServiceDate ?? "";
    set => coContemptServiceDate =
      TrimEnd(Substring(value, 1, CoContemptServiceDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoContemptServiceDate attribute.</summary>
  [JsonPropertyName("coContemptServiceDate")]
  [Computed]
  public string CoContemptServiceDate_Json
  {
    get => NullIf(CoContemptServiceDate, "");
    set => CoContemptServiceDate = value;
  }

  /// <summary>Length of the CO_DEMAND_LETTER_CREATED_DATE attribute.</summary>
  public const int CoDemandLetterCreatedDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_DEMAND_LETTER_CREATED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = CoDemandLetterCreatedDate_MaxLength)]
  public string CoDemandLetterCreatedDate
  {
    get => coDemandLetterCreatedDate ?? "";
    set => coDemandLetterCreatedDate =
      TrimEnd(Substring(value, 1, CoDemandLetterCreatedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoDemandLetterCreatedDate attribute.</summary>
  [JsonPropertyName("coDemandLetterCreatedDate")]
  [Computed]
  public string CoDemandLetterCreatedDate_Json
  {
    get => NullIf(CoDemandLetterCreatedDate, "");
    set => CoDemandLetterCreatedDate = value;
  }

  /// <summary>Length of the CO_PETITION_CREATED_DATE attribute.</summary>
  public const int CoPetitionCreatedDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_PETITION_CREATED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = CoPetitionCreatedDate_MaxLength)]
  public string CoPetitionCreatedDate
  {
    get => coPetitionCreatedDate ?? "";
    set => coPetitionCreatedDate =
      TrimEnd(Substring(value, 1, CoPetitionCreatedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoPetitionCreatedDate attribute.</summary>
  [JsonPropertyName("coPetitionCreatedDate")]
  [Computed]
  public string CoPetitionCreatedDate_Json
  {
    get => NullIf(CoPetitionCreatedDate, "");
    set => CoPetitionCreatedDate = value;
  }

  /// <summary>Length of the CO_PETITION_FILED_DATE attribute.</summary>
  public const int CoPetitionFiledDate_MaxLength = 10;

  /// <summary>
  /// The value of the CO_PETITION_FILED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = CoPetitionFiledDate_MaxLength)]
  public string CoPetitionFiledDate
  {
    get => coPetitionFiledDate ?? "";
    set => coPetitionFiledDate =
      TrimEnd(Substring(value, 1, CoPetitionFiledDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CoPetitionFiledDate attribute.</summary>
  [JsonPropertyName("coPetitionFiledDate")]
  [Computed]
  public string CoPetitionFiledDate_Json
  {
    get => NullIf(CoPetitionFiledDate, "");
    set => CoPetitionFiledDate = value;
  }

  private string headerFooter;
  private string contractorName;
  private string judicialDistrict;
  private string caseNumber;
  private string caseOpenDate;
  private string assignedCaseworkerFirstName;
  private string assignedCaseworkerLastName;
  private string pyramidCategory;
  private string addressActive;
  private string employerActive;
  private string currentSupportDue;
  private string currentSupportPaid;
  private string collectionRate;
  private string casePayingArrears;
  private string caseFunction;
  private string caseProgram;
  private string paProgramEndDate;
  private string curaAmount;
  private string familyViolence;
  private string cpNoncooperation;
  private string pendingCaseClosureDate;
  private string dateOfEmancipation;
  private string youngestEmancipationDate;
  private string childBow;
  private string cpDateOfBirth;
  private string cpEthnicity;
  private string cpZipCode;
  private string cpHomePhoneAreaCode;
  private string cpHomePhone;
  private string cpCellPhoneAreaCode;
  private string cpCellPhone;
  private string ncpPersonNumber;
  private string ncpLastName;
  private string ncpFirstName;
  private string ncpDateOfBirth;
  private string ncpEthnicity;
  private string ncpLocateDate;
  private string ncpZipCode;
  private string ncpForeignCountryCode;
  private string ncpHomePhoneAreaCode;
  private string ncpHomePhone;
  private string ncpCellPhoneAreaCode;
  private string ncpCellPhone;
  private string ncpIncarcerated;
  private string ncpInBankruptcy;
  private string ncpRepresentedByCouncil;
  private string ncpEmployerName;
  private string ncpOtherIncomeSource;
  private string ncpInterstateInitiating;
  private string ncpInterstateResponding;
  private string coCourtOrderNumber;
  private string coCsCollectedInMonth;
  private string coCsDueInMonth;
  private string coCsCollectedFfytd;
  private string coCsDueFfytd;
  private string coTotalArrearsAmountDue;
  private string coArrearsPaidInMonth;
  private string coArrearsPaidFfytd;
  private string coLastPaymentAmount;
  private string coLastPaymentDate;
  private string coLastPaymentType;
  private string coLastDsoPaymentDate;
  private string coLastIClassCreatedDate;
  private string coLastIClassActionTaken;
  private string coLastIClassFiledDate;
  private string coLastIClassIwgl;
  private string coMonthlyIwoWaAmount;
  private string coContemptHearingDate;
  private string coContemptServiceDate;
  private string coDemandLetterCreatedDate;
  private string coPetitionCreatedDate;
  private string coPetitionFiledDate;
}

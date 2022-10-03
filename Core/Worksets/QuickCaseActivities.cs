// The source file: QUICK_CASE_ACTIVITIES, ID: 374543670, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class QuickCaseActivities: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickCaseActivities()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickCaseActivities(QuickCaseActivities that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickCaseActivities Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickCaseActivities that)
  {
    base.Assign(that);
    caseClosedDate = that.caseClosedDate;
    caseClosureReason = that.caseClosureReason;
    caseOpenDate = that.caseOpenDate;
    enfCntrlOrderDetermineDate = that.enfCntrlOrderDetermineDate;
    enfIwoDate = that.enfIwoDate;
    enfIwoEmployerName = that.enfIwoEmployerName;
    enfNmsnDate = that.enfNmsnDate;
    enfNmsnEmployerName = that.enfNmsnEmployerName;
    enfMedicalCoverageDate = that.enfMedicalCoverageDate;
    enfCreditBureauDate = that.enfCreditBureauDate;
    enfSdsoDate = that.enfSdsoDate;
    enfFsdoDate = that.enfFsdoDate;
    enfPassportDenialDate = that.enfPassportDenialDate;
    enfFidmDate = that.enfFidmDate;
    enfFinancialInstitutionName = that.enfFinancialInstitutionName;
    enfDriversLicenseDate = that.enfDriversLicenseDate;
    enfProfLicenseDate = that.enfProfLicenseDate;
    enfLotteryDate = that.enfLotteryDate;
    enfLienDate = that.enfLienDate;
    locIncarceratedInd = that.locIncarceratedInd;
    locIncarceratedReleaseDate = that.locIncarceratedReleaseDate;
    locIncarceratedDate = that.locIncarceratedDate;
    zdelLocParoleEligibilityDate = that.zdelLocParoleEligibilityDate;
    locDateOfDeath = that.locDateOfDeath;
    locDeathDateInd = that.locDeathDateInd;
    oeLastReviewDate = that.oeLastReviewDate;
    oeLastModifiedDate = that.oeLastModifiedDate;
  }

  /// <summary>Length of the CASE_CLOSED_DATE attribute.</summary>
  public const int CaseClosedDate_MaxLength = 8;

  /// <summary>
  /// The value of the CASE_CLOSED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CaseClosedDate_MaxLength)]
  public string CaseClosedDate
  {
    get => caseClosedDate ?? "";
    set => caseClosedDate =
      TrimEnd(Substring(value, 1, CaseClosedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseClosedDate attribute.</summary>
  [JsonPropertyName("caseClosedDate")]
  [Computed]
  public string CaseClosedDate_Json
  {
    get => NullIf(CaseClosedDate, "");
    set => CaseClosedDate = value;
  }

  /// <summary>Length of the CASE_CLOSURE_REASON attribute.</summary>
  public const int CaseClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_CLOSURE_REASON attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = CaseClosureReason_MaxLength)]
  public string CaseClosureReason
  {
    get => caseClosureReason ?? "";
    set => caseClosureReason =
      TrimEnd(Substring(value, 1, CaseClosureReason_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseClosureReason attribute.</summary>
  [JsonPropertyName("caseClosureReason")]
  [Computed]
  public string CaseClosureReason_Json
  {
    get => NullIf(CaseClosureReason, "");
    set => CaseClosureReason = value;
  }

  /// <summary>Length of the CASE_OPEN_DATE attribute.</summary>
  public const int CaseOpenDate_MaxLength = 8;

  /// <summary>
  /// The value of the CASE_OPEN_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseOpenDate_MaxLength)]
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

  /// <summary>Length of the ENF_CNTRL_ORDER_DETERMINE_DATE attribute.</summary>
  public const int EnfCntrlOrderDetermineDate_MaxLength = 10;

  /// <summary>
  /// The value of the ENF_CNTRL_ORDER_DETERMINE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = EnfCntrlOrderDetermineDate_MaxLength)]
  public string EnfCntrlOrderDetermineDate
  {
    get => enfCntrlOrderDetermineDate ?? "";
    set => enfCntrlOrderDetermineDate =
      TrimEnd(Substring(value, 1, EnfCntrlOrderDetermineDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfCntrlOrderDetermineDate attribute.</summary>
  [JsonPropertyName("enfCntrlOrderDetermineDate")]
  [Computed]
  public string EnfCntrlOrderDetermineDate_Json
  {
    get => NullIf(EnfCntrlOrderDetermineDate, "");
    set => EnfCntrlOrderDetermineDate = value;
  }

  /// <summary>Length of the ENF_IWO_DATE attribute.</summary>
  public const int EnfIwoDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_IWO_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = EnfIwoDate_MaxLength)]
  public string EnfIwoDate
  {
    get => enfIwoDate ?? "";
    set => enfIwoDate = TrimEnd(Substring(value, 1, EnfIwoDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfIwoDate attribute.</summary>
  [JsonPropertyName("enfIwoDate")]
  [Computed]
  public string EnfIwoDate_Json
  {
    get => NullIf(EnfIwoDate, "");
    set => EnfIwoDate = value;
  }

  /// <summary>Length of the ENF_IWO_EMPLOYER_NAME attribute.</summary>
  public const int EnfIwoEmployerName_MaxLength = 60;

  /// <summary>
  /// The value of the ENF_IWO_EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = EnfIwoEmployerName_MaxLength)]
  public string EnfIwoEmployerName
  {
    get => enfIwoEmployerName ?? "";
    set => enfIwoEmployerName =
      TrimEnd(Substring(value, 1, EnfIwoEmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfIwoEmployerName attribute.</summary>
  [JsonPropertyName("enfIwoEmployerName")]
  [Computed]
  public string EnfIwoEmployerName_Json
  {
    get => NullIf(EnfIwoEmployerName, "");
    set => EnfIwoEmployerName = value;
  }

  /// <summary>Length of the ENF_NMSN_DATE attribute.</summary>
  public const int EnfNmsnDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_NMSN_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = EnfNmsnDate_MaxLength)]
  public string EnfNmsnDate
  {
    get => enfNmsnDate ?? "";
    set => enfNmsnDate = TrimEnd(Substring(value, 1, EnfNmsnDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfNmsnDate attribute.</summary>
  [JsonPropertyName("enfNmsnDate")]
  [Computed]
  public string EnfNmsnDate_Json
  {
    get => NullIf(EnfNmsnDate, "");
    set => EnfNmsnDate = value;
  }

  /// <summary>Length of the ENF_NMSN_EMPLOYER_NAME attribute.</summary>
  public const int EnfNmsnEmployerName_MaxLength = 60;

  /// <summary>
  /// The value of the ENF_NMSN_EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = EnfNmsnEmployerName_MaxLength)]
  public string EnfNmsnEmployerName
  {
    get => enfNmsnEmployerName ?? "";
    set => enfNmsnEmployerName =
      TrimEnd(Substring(value, 1, EnfNmsnEmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfNmsnEmployerName attribute.</summary>
  [JsonPropertyName("enfNmsnEmployerName")]
  [Computed]
  public string EnfNmsnEmployerName_Json
  {
    get => NullIf(EnfNmsnEmployerName, "");
    set => EnfNmsnEmployerName = value;
  }

  /// <summary>Length of the ENF_MEDICAL_COVERAGE_DATE attribute.</summary>
  public const int EnfMedicalCoverageDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_MEDICAL_COVERAGE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = EnfMedicalCoverageDate_MaxLength)]
  public string EnfMedicalCoverageDate
  {
    get => enfMedicalCoverageDate ?? "";
    set => enfMedicalCoverageDate =
      TrimEnd(Substring(value, 1, EnfMedicalCoverageDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfMedicalCoverageDate attribute.</summary>
  [JsonPropertyName("enfMedicalCoverageDate")]
  [Computed]
  public string EnfMedicalCoverageDate_Json
  {
    get => NullIf(EnfMedicalCoverageDate, "");
    set => EnfMedicalCoverageDate = value;
  }

  /// <summary>Length of the ENF_CREDIT_BUREAU_DATE attribute.</summary>
  public const int EnfCreditBureauDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_CREDIT_BUREAU_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = EnfCreditBureauDate_MaxLength)]
  public string EnfCreditBureauDate
  {
    get => enfCreditBureauDate ?? "";
    set => enfCreditBureauDate =
      TrimEnd(Substring(value, 1, EnfCreditBureauDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfCreditBureauDate attribute.</summary>
  [JsonPropertyName("enfCreditBureauDate")]
  [Computed]
  public string EnfCreditBureauDate_Json
  {
    get => NullIf(EnfCreditBureauDate, "");
    set => EnfCreditBureauDate = value;
  }

  /// <summary>Length of the ENF_SDSO_DATE attribute.</summary>
  public const int EnfSdsoDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_SDSO_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = EnfSdsoDate_MaxLength)]
  public string EnfSdsoDate
  {
    get => enfSdsoDate ?? "";
    set => enfSdsoDate = TrimEnd(Substring(value, 1, EnfSdsoDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfSdsoDate attribute.</summary>
  [JsonPropertyName("enfSdsoDate")]
  [Computed]
  public string EnfSdsoDate_Json
  {
    get => NullIf(EnfSdsoDate, "");
    set => EnfSdsoDate = value;
  }

  /// <summary>Length of the ENF_FSDO_DATE attribute.</summary>
  public const int EnfFsdoDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_FSDO_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = EnfFsdoDate_MaxLength)]
  public string EnfFsdoDate
  {
    get => enfFsdoDate ?? "";
    set => enfFsdoDate = TrimEnd(Substring(value, 1, EnfFsdoDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfFsdoDate attribute.</summary>
  [JsonPropertyName("enfFsdoDate")]
  [Computed]
  public string EnfFsdoDate_Json
  {
    get => NullIf(EnfFsdoDate, "");
    set => EnfFsdoDate = value;
  }

  /// <summary>Length of the ENF_PASSPORT_DENIAL_DATE attribute.</summary>
  public const int EnfPassportDenialDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_PASSPORT_DENIAL_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = EnfPassportDenialDate_MaxLength)]
  public string EnfPassportDenialDate
  {
    get => enfPassportDenialDate ?? "";
    set => enfPassportDenialDate =
      TrimEnd(Substring(value, 1, EnfPassportDenialDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfPassportDenialDate attribute.</summary>
  [JsonPropertyName("enfPassportDenialDate")]
  [Computed]
  public string EnfPassportDenialDate_Json
  {
    get => NullIf(EnfPassportDenialDate, "");
    set => EnfPassportDenialDate = value;
  }

  /// <summary>Length of the ENF_FIDM_DATE attribute.</summary>
  public const int EnfFidmDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_FIDM_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = EnfFidmDate_MaxLength)]
  public string EnfFidmDate
  {
    get => enfFidmDate ?? "";
    set => enfFidmDate = TrimEnd(Substring(value, 1, EnfFidmDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfFidmDate attribute.</summary>
  [JsonPropertyName("enfFidmDate")]
  [Computed]
  public string EnfFidmDate_Json
  {
    get => NullIf(EnfFidmDate, "");
    set => EnfFidmDate = value;
  }

  /// <summary>Length of the ENF_FINANCIAL_INSTITUTION_NAME attribute.</summary>
  public const int EnfFinancialInstitutionName_MaxLength = 40;

  /// <summary>
  /// The value of the ENF_FINANCIAL_INSTITUTION_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = EnfFinancialInstitutionName_MaxLength)]
  public string EnfFinancialInstitutionName
  {
    get => enfFinancialInstitutionName ?? "";
    set => enfFinancialInstitutionName =
      TrimEnd(Substring(value, 1, EnfFinancialInstitutionName_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfFinancialInstitutionName attribute.</summary>
  [JsonPropertyName("enfFinancialInstitutionName")]
  [Computed]
  public string EnfFinancialInstitutionName_Json
  {
    get => NullIf(EnfFinancialInstitutionName, "");
    set => EnfFinancialInstitutionName = value;
  }

  /// <summary>Length of the ENF_DRIVERS_LICENSE_DATE attribute.</summary>
  public const int EnfDriversLicenseDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_DRIVERS_LICENSE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = EnfDriversLicenseDate_MaxLength)]
  public string EnfDriversLicenseDate
  {
    get => enfDriversLicenseDate ?? "";
    set => enfDriversLicenseDate =
      TrimEnd(Substring(value, 1, EnfDriversLicenseDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfDriversLicenseDate attribute.</summary>
  [JsonPropertyName("enfDriversLicenseDate")]
  [Computed]
  public string EnfDriversLicenseDate_Json
  {
    get => NullIf(EnfDriversLicenseDate, "");
    set => EnfDriversLicenseDate = value;
  }

  /// <summary>Length of the ENF_PROF_LICENSE_DATE attribute.</summary>
  public const int EnfProfLicenseDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_PROF_LICENSE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = EnfProfLicenseDate_MaxLength)]
  public string EnfProfLicenseDate
  {
    get => enfProfLicenseDate ?? "";
    set => enfProfLicenseDate =
      TrimEnd(Substring(value, 1, EnfProfLicenseDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfProfLicenseDate attribute.</summary>
  [JsonPropertyName("enfProfLicenseDate")]
  [Computed]
  public string EnfProfLicenseDate_Json
  {
    get => NullIf(EnfProfLicenseDate, "");
    set => EnfProfLicenseDate = value;
  }

  /// <summary>Length of the ENF_LOTTERY_DATE attribute.</summary>
  public const int EnfLotteryDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_LOTTERY_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = EnfLotteryDate_MaxLength)]
    
  public string EnfLotteryDate
  {
    get => enfLotteryDate ?? "";
    set => enfLotteryDate =
      TrimEnd(Substring(value, 1, EnfLotteryDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfLotteryDate attribute.</summary>
  [JsonPropertyName("enfLotteryDate")]
  [Computed]
  public string EnfLotteryDate_Json
  {
    get => NullIf(EnfLotteryDate, "");
    set => EnfLotteryDate = value;
  }

  /// <summary>Length of the ENF_LIEN_DATE attribute.</summary>
  public const int EnfLienDate_MaxLength = 8;

  /// <summary>
  /// The value of the ENF_LIEN_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = EnfLienDate_MaxLength)]
  public string EnfLienDate
  {
    get => enfLienDate ?? "";
    set => enfLienDate = TrimEnd(Substring(value, 1, EnfLienDate_MaxLength));
  }

  /// <summary>
  /// The json value of the EnfLienDate attribute.</summary>
  [JsonPropertyName("enfLienDate")]
  [Computed]
  public string EnfLienDate_Json
  {
    get => NullIf(EnfLienDate, "");
    set => EnfLienDate = value;
  }

  /// <summary>Length of the LOC_INCARCERATED_IND attribute.</summary>
  public const int LocIncarceratedInd_MaxLength = 1;

  /// <summary>
  /// The value of the LOC_INCARCERATED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = LocIncarceratedInd_MaxLength)]
  public string LocIncarceratedInd
  {
    get => locIncarceratedInd ?? "";
    set => locIncarceratedInd =
      TrimEnd(Substring(value, 1, LocIncarceratedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the LocIncarceratedInd attribute.</summary>
  [JsonPropertyName("locIncarceratedInd")]
  [Computed]
  public string LocIncarceratedInd_Json
  {
    get => NullIf(LocIncarceratedInd, "");
    set => LocIncarceratedInd = value;
  }

  /// <summary>Length of the LOC_INCARCERATED_RELEASE_DATE attribute.</summary>
  public const int LocIncarceratedReleaseDate_MaxLength = 8;

  /// <summary>
  /// The value of the LOC_INCARCERATED_RELEASE_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = LocIncarceratedReleaseDate_MaxLength)]
  public string LocIncarceratedReleaseDate
  {
    get => locIncarceratedReleaseDate ?? "";
    set => locIncarceratedReleaseDate =
      TrimEnd(Substring(value, 1, LocIncarceratedReleaseDate_MaxLength));
  }

  /// <summary>
  /// The json value of the LocIncarceratedReleaseDate attribute.</summary>
  [JsonPropertyName("locIncarceratedReleaseDate")]
  [Computed]
  public string LocIncarceratedReleaseDate_Json
  {
    get => NullIf(LocIncarceratedReleaseDate, "");
    set => LocIncarceratedReleaseDate = value;
  }

  /// <summary>Length of the LOC_INCARCERATED_DATE attribute.</summary>
  public const int LocIncarceratedDate_MaxLength = 8;

  /// <summary>
  /// The value of the LOC_INCARCERATED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = LocIncarceratedDate_MaxLength)]
  public string LocIncarceratedDate
  {
    get => locIncarceratedDate ?? "";
    set => locIncarceratedDate =
      TrimEnd(Substring(value, 1, LocIncarceratedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the LocIncarceratedDate attribute.</summary>
  [JsonPropertyName("locIncarceratedDate")]
  [Computed]
  public string LocIncarceratedDate_Json
  {
    get => NullIf(LocIncarceratedDate, "");
    set => LocIncarceratedDate = value;
  }

  /// <summary>Length of the ZDEL_LOC_PAROLE_ELIGIBILITY_DATE attribute.
  /// </summary>
  public const int ZdelLocParoleEligibilityDate_MaxLength = 8;

  /// <summary>
  /// The value of the ZDEL_LOC_PAROLE_ELIGIBILITY_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ZdelLocParoleEligibilityDate_MaxLength)]
  public string ZdelLocParoleEligibilityDate
  {
    get => zdelLocParoleEligibilityDate ?? "";
    set => zdelLocParoleEligibilityDate =
      TrimEnd(Substring(value, 1, ZdelLocParoleEligibilityDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ZdelLocParoleEligibilityDate attribute.</summary>
  [JsonPropertyName("zdelLocParoleEligibilityDate")]
  [Computed]
  public string ZdelLocParoleEligibilityDate_Json
  {
    get => NullIf(ZdelLocParoleEligibilityDate, "");
    set => ZdelLocParoleEligibilityDate = value;
  }

  /// <summary>Length of the LOC_DATE_OF_DEATH attribute.</summary>
  public const int LocDateOfDeath_MaxLength = 8;

  /// <summary>
  /// The value of the LOC_DATE_OF_DEATH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = LocDateOfDeath_MaxLength)]
    
  public string LocDateOfDeath
  {
    get => locDateOfDeath ?? "";
    set => locDateOfDeath =
      TrimEnd(Substring(value, 1, LocDateOfDeath_MaxLength));
  }

  /// <summary>
  /// The json value of the LocDateOfDeath attribute.</summary>
  [JsonPropertyName("locDateOfDeath")]
  [Computed]
  public string LocDateOfDeath_Json
  {
    get => NullIf(LocDateOfDeath, "");
    set => LocDateOfDeath = value;
  }

  /// <summary>Length of the LOC_DEATH_DATE_IND attribute.</summary>
  public const int LocDeathDateInd_MaxLength = 10;

  /// <summary>
  /// The value of the LOC_DEATH_DATE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = LocDeathDateInd_MaxLength)
    ]
  public string LocDeathDateInd
  {
    get => locDeathDateInd ?? "";
    set => locDeathDateInd =
      TrimEnd(Substring(value, 1, LocDeathDateInd_MaxLength));
  }

  /// <summary>
  /// The json value of the LocDeathDateInd attribute.</summary>
  [JsonPropertyName("locDeathDateInd")]
  [Computed]
  public string LocDeathDateInd_Json
  {
    get => NullIf(LocDeathDateInd, "");
    set => LocDeathDateInd = value;
  }

  /// <summary>Length of the OE_LAST_REVIEW_DATE attribute.</summary>
  public const int OeLastReviewDate_MaxLength = 8;

  /// <summary>
  /// The value of the OE_LAST_REVIEW_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = OeLastReviewDate_MaxLength)]
  public string OeLastReviewDate
  {
    get => oeLastReviewDate ?? "";
    set => oeLastReviewDate =
      TrimEnd(Substring(value, 1, OeLastReviewDate_MaxLength));
  }

  /// <summary>
  /// The json value of the OeLastReviewDate attribute.</summary>
  [JsonPropertyName("oeLastReviewDate")]
  [Computed]
  public string OeLastReviewDate_Json
  {
    get => NullIf(OeLastReviewDate, "");
    set => OeLastReviewDate = value;
  }

  /// <summary>Length of the OE_LAST_MODIFIED_DATE attribute.</summary>
  public const int OeLastModifiedDate_MaxLength = 8;

  /// <summary>
  /// The value of the OE_LAST_MODIFIED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = OeLastModifiedDate_MaxLength)]
  public string OeLastModifiedDate
  {
    get => oeLastModifiedDate ?? "";
    set => oeLastModifiedDate =
      TrimEnd(Substring(value, 1, OeLastModifiedDate_MaxLength));
  }

  /// <summary>
  /// The json value of the OeLastModifiedDate attribute.</summary>
  [JsonPropertyName("oeLastModifiedDate")]
  [Computed]
  public string OeLastModifiedDate_Json
  {
    get => NullIf(OeLastModifiedDate, "");
    set => OeLastModifiedDate = value;
  }

  private string caseClosedDate;
  private string caseClosureReason;
  private string caseOpenDate;
  private string enfCntrlOrderDetermineDate;
  private string enfIwoDate;
  private string enfIwoEmployerName;
  private string enfNmsnDate;
  private string enfNmsnEmployerName;
  private string enfMedicalCoverageDate;
  private string enfCreditBureauDate;
  private string enfSdsoDate;
  private string enfFsdoDate;
  private string enfPassportDenialDate;
  private string enfFidmDate;
  private string enfFinancialInstitutionName;
  private string enfDriversLicenseDate;
  private string enfProfLicenseDate;
  private string enfLotteryDate;
  private string enfLienDate;
  private string locIncarceratedInd;
  private string locIncarceratedReleaseDate;
  private string locIncarceratedDate;
  private string zdelLocParoleEligibilityDate;
  private string locDateOfDeath;
  private string locDeathDateInd;
  private string oeLastReviewDate;
  private string oeLastModifiedDate;
}

// The source file: GENETIC_TEST_INFORMATION, ID: 371793996, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: OBLGESTB
/// Input data for maintain genetic test.
/// </summary>
[Serializable]
public partial class GeneticTestInformation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GeneticTestInformation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GeneticTestInformation(GeneticTestInformation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GeneticTestInformation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GeneticTestInformation that)
  {
    base.Assign(that);
    childDob = that.childDob;
    caseNumber = that.caseNumber;
    courtOrderNo = that.courtOrderNo;
    geneticTestAccountNo = that.geneticTestAccountNo;
    labCaseNo = that.labCaseNo;
    testType = that.testType;
    fatherPersonNo = that.fatherPersonNo;
    fatherFormattedName = that.fatherFormattedName;
    fatherLastName = that.fatherLastName;
    fatherMi = that.fatherMi;
    fatherFirstName = that.fatherFirstName;
    fatherDrawSiteId = that.fatherDrawSiteId;
    fatherDrawSiteVendorName = that.fatherDrawSiteVendorName;
    fatherDrawSiteCity = that.fatherDrawSiteCity;
    fatherDrawSiteState = that.fatherDrawSiteState;
    fatherSchedTestDate = that.fatherSchedTestDate;
    fatherSchedTestTime = that.fatherSchedTestTime;
    fatherCollectSampleInd = that.fatherCollectSampleInd;
    fatherReuseSampleInd = that.fatherReuseSampleInd;
    fatherShowInd = that.fatherShowInd;
    fatherSampleCollectedInd = that.fatherSampleCollectedInd;
    fatherPrevSampExistsInd = that.fatherPrevSampExistsInd;
    fatherPrevSampGtestNumber = that.fatherPrevSampGtestNumber;
    fatherPrevSampTestType = that.fatherPrevSampTestType;
    fatherPrevSampleLabCaseNo = that.fatherPrevSampleLabCaseNo;
    fatherPrevSampSpecimenId = that.fatherPrevSampSpecimenId;
    fatherPrevSampPerGenTestId = that.fatherPrevSampPerGenTestId;
    fatherSpecimenId = that.fatherSpecimenId;
    fatherRescheduledInd = that.fatherRescheduledInd;
    motherPersonNo = that.motherPersonNo;
    motherFormattedName = that.motherFormattedName;
    motherLastName = that.motherLastName;
    motherMi = that.motherMi;
    motherFirstName = that.motherFirstName;
    motherDrawSiteId = that.motherDrawSiteId;
    motherDrawSiteVendorName = that.motherDrawSiteVendorName;
    motherDrawSiteCity = that.motherDrawSiteCity;
    motherDrawSiteState = that.motherDrawSiteState;
    motherSchedTestDate = that.motherSchedTestDate;
    motherSchedTestTime = that.motherSchedTestTime;
    motherCollectSampleInd = that.motherCollectSampleInd;
    motherReuseSampleInd = that.motherReuseSampleInd;
    motherShowInd = that.motherShowInd;
    motherSampleCollectedInd = that.motherSampleCollectedInd;
    motherPrevSampExistsInd = that.motherPrevSampExistsInd;
    motherPrevSampGtestNumber = that.motherPrevSampGtestNumber;
    motherPrevSampTestType = that.motherPrevSampTestType;
    motherPrevSampLabCaseNo = that.motherPrevSampLabCaseNo;
    motherPrevSampSpecimenId = that.motherPrevSampSpecimenId;
    motherPrevSampPerGenTestId = that.motherPrevSampPerGenTestId;
    motherSpecimenId = that.motherSpecimenId;
    motherRescheduledInd = that.motherRescheduledInd;
    childPersonNo = that.childPersonNo;
    childFormattedName = that.childFormattedName;
    childLastName = that.childLastName;
    childMi = that.childMi;
    childFirstName = that.childFirstName;
    childDrawSiteId = that.childDrawSiteId;
    childDrawSiteVendorName = that.childDrawSiteVendorName;
    childDrawSiteCity = that.childDrawSiteCity;
    childDrawSiteState = that.childDrawSiteState;
    childSchedTestDate = that.childSchedTestDate;
    childSchedTestTime = that.childSchedTestTime;
    childCollectSampleInd = that.childCollectSampleInd;
    childReuseSampleInd = that.childReuseSampleInd;
    childShowInd = that.childShowInd;
    childSampleCollectedInd = that.childSampleCollectedInd;
    childPrevSampExistsInd = that.childPrevSampExistsInd;
    childPrevSampGtestNumber = that.childPrevSampGtestNumber;
    childPrevSampTestType = that.childPrevSampTestType;
    childPrevSampLabCaseNo = that.childPrevSampLabCaseNo;
    childPrevSampSpecimenId = that.childPrevSampSpecimenId;
    childPrevSampPerGenTestId = that.childPrevSampPerGenTestId;
    childSpecimenId = that.childSpecimenId;
    childReschedInd = that.childReschedInd;
    testSiteVendorId = that.testSiteVendorId;
    testSiteVendorName = that.testSiteVendorName;
    testSiteCity = that.testSiteCity;
    testSiteState = that.testSiteState;
    actualTestDate = that.actualTestDate;
    scheduledTestDate = that.scheduledTestDate;
    resultReceivedDate = that.resultReceivedDate;
    paternityExcludedInd = that.paternityExcludedInd;
    prevPaternityExcludedInd = that.prevPaternityExcludedInd;
    paternityProbability = that.paternityProbability;
    resultContestedInd = that.resultContestedInd;
    contestStartedDate = that.contestStartedDate;
    contestEndedDate = that.contestEndedDate;
  }

  /// <summary>
  /// The value of the CHILD_DOB attribute.
  /// </summary>
  [JsonPropertyName("childDob")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ChildDob
  {
    get => childDob;
    set => childDob = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
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

  /// <summary>Length of the COURT_ORDER_NO attribute.</summary>
  public const int CourtOrderNo_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_ORDER_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CourtOrderNo_MaxLength)]
  public string CourtOrderNo
  {
    get => courtOrderNo ?? "";
    set => courtOrderNo = TrimEnd(Substring(value, 1, CourtOrderNo_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtOrderNo attribute.</summary>
  [JsonPropertyName("courtOrderNo")]
  [Computed]
  public string CourtOrderNo_Json
  {
    get => NullIf(CourtOrderNo, "");
    set => CourtOrderNo = value;
  }

  /// <summary>Length of the GENETIC_TEST_ACCOUNT_NO attribute.</summary>
  public const int GeneticTestAccountNo_MaxLength = 8;

  /// <summary>
  /// The value of the GENETIC_TEST_ACCOUNT_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = GeneticTestAccountNo_MaxLength)]
  public string GeneticTestAccountNo
  {
    get => geneticTestAccountNo ?? "";
    set => geneticTestAccountNo =
      TrimEnd(Substring(value, 1, GeneticTestAccountNo_MaxLength));
  }

  /// <summary>
  /// The json value of the GeneticTestAccountNo attribute.</summary>
  [JsonPropertyName("geneticTestAccountNo")]
  [Computed]
  public string GeneticTestAccountNo_Json
  {
    get => NullIf(GeneticTestAccountNo, "");
    set => GeneticTestAccountNo = value;
  }

  /// <summary>Length of the LAB_CASE_NO attribute.</summary>
  public const int LabCaseNo_MaxLength = 11;

  /// <summary>
  /// The value of the LAB_CASE_NO attribute.
  /// Laboratory case no assigned to the genetic test by test site. (generally 
  /// of the format CYY-999-99999 where YY is the year).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LabCaseNo_MaxLength)]
  public string LabCaseNo
  {
    get => labCaseNo ?? "";
    set => labCaseNo = TrimEnd(Substring(value, 1, LabCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the LabCaseNo attribute.</summary>
  [JsonPropertyName("labCaseNo")]
  [Computed]
  public string LabCaseNo_Json
  {
    get => NullIf(LabCaseNo, "");
    set => LabCaseNo = value;
  }

  /// <summary>Length of the TEST_TYPE attribute.</summary>
  public const int TestType_MaxLength = 2;

  /// <summary>
  /// The value of the TEST_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TestType_MaxLength)]
  public string TestType
  {
    get => testType ?? "";
    set => testType = TrimEnd(Substring(value, 1, TestType_MaxLength));
  }

  /// <summary>
  /// The json value of the TestType attribute.</summary>
  [JsonPropertyName("testType")]
  [Computed]
  public string TestType_Json
  {
    get => NullIf(TestType, "");
    set => TestType = value;
  }

  /// <summary>Length of the FATHER_PERSON_NO attribute.</summary>
  public const int FatherPersonNo_MaxLength = 10;

  /// <summary>
  /// The value of the FATHER_PERSON_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FatherPersonNo_MaxLength)]
  public string FatherPersonNo
  {
    get => fatherPersonNo ?? "";
    set => fatherPersonNo =
      TrimEnd(Substring(value, 1, FatherPersonNo_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherPersonNo attribute.</summary>
  [JsonPropertyName("fatherPersonNo")]
  [Computed]
  public string FatherPersonNo_Json
  {
    get => NullIf(FatherPersonNo, "");
    set => FatherPersonNo = value;
  }

  /// <summary>Length of the FATHER_FORMATTED_NAME attribute.</summary>
  public const int FatherFormattedName_MaxLength = 33;

  /// <summary>
  /// The value of the FATHER_FORMATTED_NAME attribute.
  /// This is used by the format cse person name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = FatherFormattedName_MaxLength)]
  public string FatherFormattedName
  {
    get => fatherFormattedName ?? "";
    set => fatherFormattedName =
      TrimEnd(Substring(value, 1, FatherFormattedName_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherFormattedName attribute.</summary>
  [JsonPropertyName("fatherFormattedName")]
  [Computed]
  public string FatherFormattedName_Json
  {
    get => NullIf(FatherFormattedName, "");
    set => FatherFormattedName = value;
  }

  /// <summary>Length of the FATHER_LAST_NAME attribute.</summary>
  public const int FatherLastName_MaxLength = 17;

  /// <summary>
  /// The value of the FATHER_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = FatherLastName_MaxLength)]
  public string FatherLastName
  {
    get => fatherLastName ?? "";
    set => fatherLastName =
      TrimEnd(Substring(value, 1, FatherLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherLastName attribute.</summary>
  [JsonPropertyName("fatherLastName")]
  [Computed]
  public string FatherLastName_Json
  {
    get => NullIf(FatherLastName, "");
    set => FatherLastName = value;
  }

  /// <summary>Length of the FATHER_MI attribute.</summary>
  public const int FatherMi_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_MI attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = FatherMi_MaxLength)]
  public string FatherMi
  {
    get => fatherMi ?? "";
    set => fatherMi = TrimEnd(Substring(value, 1, FatherMi_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherMi attribute.</summary>
  [JsonPropertyName("fatherMi")]
  [Computed]
  public string FatherMi_Json
  {
    get => NullIf(FatherMi, "");
    set => FatherMi = value;
  }

  /// <summary>Length of the FATHER_FIRST_NAME attribute.</summary>
  public const int FatherFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FATHER_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = FatherFirstName_MaxLength)
    ]
  public string FatherFirstName
  {
    get => fatherFirstName ?? "";
    set => fatherFirstName =
      TrimEnd(Substring(value, 1, FatherFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherFirstName attribute.</summary>
  [JsonPropertyName("fatherFirstName")]
  [Computed]
  public string FatherFirstName_Json
  {
    get => NullIf(FatherFirstName, "");
    set => FatherFirstName = value;
  }

  /// <summary>Length of the FATHER_DRAW_SITE_ID attribute.</summary>
  public const int FatherDrawSiteId_MaxLength = 10;

  /// <summary>
  /// The value of the FATHER_DRAW_SITE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = FatherDrawSiteId_MaxLength)]
  public string FatherDrawSiteId
  {
    get => fatherDrawSiteId ?? "";
    set => fatherDrawSiteId =
      TrimEnd(Substring(value, 1, FatherDrawSiteId_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherDrawSiteId attribute.</summary>
  [JsonPropertyName("fatherDrawSiteId")]
  [Computed]
  public string FatherDrawSiteId_Json
  {
    get => NullIf(FatherDrawSiteId, "");
    set => FatherDrawSiteId = value;
  }

  /// <summary>Length of the FATHER_DRAW_SITE_VENDOR_NAME attribute.</summary>
  public const int FatherDrawSiteVendorName_MaxLength = 30;

  /// <summary>
  /// The value of the FATHER_DRAW_SITE_VENDOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = FatherDrawSiteVendorName_MaxLength)]
  public string FatherDrawSiteVendorName
  {
    get => fatherDrawSiteVendorName ?? "";
    set => fatherDrawSiteVendorName =
      TrimEnd(Substring(value, 1, FatherDrawSiteVendorName_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherDrawSiteVendorName attribute.</summary>
  [JsonPropertyName("fatherDrawSiteVendorName")]
  [Computed]
  public string FatherDrawSiteVendorName_Json
  {
    get => NullIf(FatherDrawSiteVendorName, "");
    set => FatherDrawSiteVendorName = value;
  }

  /// <summary>Length of the FATHER_DRAW_SITE_CITY attribute.</summary>
  public const int FatherDrawSiteCity_MaxLength = 15;

  /// <summary>
  /// The value of the FATHER_DRAW_SITE_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = FatherDrawSiteCity_MaxLength)]
  public string FatherDrawSiteCity
  {
    get => fatherDrawSiteCity ?? "";
    set => fatherDrawSiteCity =
      TrimEnd(Substring(value, 1, FatherDrawSiteCity_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherDrawSiteCity attribute.</summary>
  [JsonPropertyName("fatherDrawSiteCity")]
  [Computed]
  public string FatherDrawSiteCity_Json
  {
    get => NullIf(FatherDrawSiteCity, "");
    set => FatherDrawSiteCity = value;
  }

  /// <summary>Length of the FATHER_DRAW_SITE_STATE attribute.</summary>
  public const int FatherDrawSiteState_MaxLength = 2;

  /// <summary>
  /// The value of the FATHER_DRAW_SITE_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = FatherDrawSiteState_MaxLength)]
  public string FatherDrawSiteState
  {
    get => fatherDrawSiteState ?? "";
    set => fatherDrawSiteState =
      TrimEnd(Substring(value, 1, FatherDrawSiteState_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherDrawSiteState attribute.</summary>
  [JsonPropertyName("fatherDrawSiteState")]
  [Computed]
  public string FatherDrawSiteState_Json
  {
    get => NullIf(FatherDrawSiteState, "");
    set => FatherDrawSiteState = value;
  }

  /// <summary>
  /// The value of the FATHER_SCHED_TEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("fatherSchedTestDate")]
  [Member(Index = 16, Type = MemberType.Date)]
  public DateTime? FatherSchedTestDate
  {
    get => fatherSchedTestDate;
    set => fatherSchedTestDate = value;
  }

  /// <summary>Length of the FATHER_SCHED_TEST_TIME attribute.</summary>
  public const int FatherSchedTestTime_MaxLength = 5;

  /// <summary>
  /// The value of the FATHER_SCHED_TEST_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = FatherSchedTestTime_MaxLength)]
  public string FatherSchedTestTime
  {
    get => fatherSchedTestTime ?? "";
    set => fatherSchedTestTime =
      TrimEnd(Substring(value, 1, FatherSchedTestTime_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherSchedTestTime attribute.</summary>
  [JsonPropertyName("fatherSchedTestTime")]
  [Computed]
  public string FatherSchedTestTime_Json
  {
    get => NullIf(FatherSchedTestTime, "");
    set => FatherSchedTestTime = value;
  }

  /// <summary>Length of the FATHER_COLLECT_SAMPLE_IND attribute.</summary>
  public const int FatherCollectSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_COLLECT_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = FatherCollectSampleInd_MaxLength)]
  public string FatherCollectSampleInd
  {
    get => fatherCollectSampleInd ?? "";
    set => fatherCollectSampleInd =
      TrimEnd(Substring(value, 1, FatherCollectSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherCollectSampleInd attribute.</summary>
  [JsonPropertyName("fatherCollectSampleInd")]
  [Computed]
  public string FatherCollectSampleInd_Json
  {
    get => NullIf(FatherCollectSampleInd, "");
    set => FatherCollectSampleInd = value;
  }

  /// <summary>Length of the FATHER_REUSE_SAMPLE_IND attribute.</summary>
  public const int FatherReuseSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_REUSE_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = FatherReuseSampleInd_MaxLength)]
  public string FatherReuseSampleInd
  {
    get => fatherReuseSampleInd ?? "";
    set => fatherReuseSampleInd =
      TrimEnd(Substring(value, 1, FatherReuseSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherReuseSampleInd attribute.</summary>
  [JsonPropertyName("fatherReuseSampleInd")]
  [Computed]
  public string FatherReuseSampleInd_Json
  {
    get => NullIf(FatherReuseSampleInd, "");
    set => FatherReuseSampleInd = value;
  }

  /// <summary>Length of the FATHER_SHOW_IND attribute.</summary>
  public const int FatherShowInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_SHOW_IND attribute.
  /// Specifies whether or not father showed up for sample collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = FatherShowInd_MaxLength)]
  public string FatherShowInd
  {
    get => fatherShowInd ?? "";
    set => fatherShowInd =
      TrimEnd(Substring(value, 1, FatherShowInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherShowInd attribute.</summary>
  [JsonPropertyName("fatherShowInd")]
  [Computed]
  public string FatherShowInd_Json
  {
    get => NullIf(FatherShowInd, "");
    set => FatherShowInd = value;
  }

  /// <summary>Length of the FATHER_SAMPLE_COLLECTED_IND attribute.</summary>
  public const int FatherSampleCollectedInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_SAMPLE_COLLECTED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = FatherSampleCollectedInd_MaxLength)]
  public string FatherSampleCollectedInd
  {
    get => fatherSampleCollectedInd ?? "";
    set => fatherSampleCollectedInd =
      TrimEnd(Substring(value, 1, FatherSampleCollectedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherSampleCollectedInd attribute.</summary>
  [JsonPropertyName("fatherSampleCollectedInd")]
  [Computed]
  public string FatherSampleCollectedInd_Json
  {
    get => NullIf(FatherSampleCollectedInd, "");
    set => FatherSampleCollectedInd = value;
  }

  /// <summary>Length of the FATHER_PREV_SAMP_EXISTS_IND attribute.</summary>
  public const int FatherPrevSampExistsInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_PREV_SAMP_EXISTS_IND attribute.
  /// Whether or not a previous sample exists and known to the system.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = FatherPrevSampExistsInd_MaxLength)]
  public string FatherPrevSampExistsInd
  {
    get => fatherPrevSampExistsInd ?? "";
    set => fatherPrevSampExistsInd =
      TrimEnd(Substring(value, 1, FatherPrevSampExistsInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherPrevSampExistsInd attribute.</summary>
  [JsonPropertyName("fatherPrevSampExistsInd")]
  [Computed]
  public string FatherPrevSampExistsInd_Json
  {
    get => NullIf(FatherPrevSampExistsInd, "");
    set => FatherPrevSampExistsInd = value;
  }

  /// <summary>
  /// The value of the FATHER_PREV_SAMP_GTEST_NUMBER attribute.
  /// Identifier of the genetic test for the sample being reused.
  /// </summary>
  [JsonPropertyName("fatherPrevSampGtestNumber")]
  [DefaultValue(0)]
  [Member(Index = 23, Type = MemberType.Number, Length = 8)]
  public int FatherPrevSampGtestNumber
  {
    get => fatherPrevSampGtestNumber;
    set => fatherPrevSampGtestNumber = value;
  }

  /// <summary>Length of the FATHER_PREV_SAMP_TEST_TYPE attribute.</summary>
  public const int FatherPrevSampTestType_MaxLength = 2;

  /// <summary>
  /// The value of the FATHER_PREV_SAMP_TEST_TYPE attribute.
  /// The type of the scientific test used to determine parentage for the 
  /// previous sample taken.
  /// e.g. DNA, Blood test, Buckle Swab etc.
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// GENETIC_TEST_TYPE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = FatherPrevSampTestType_MaxLength)]
  public string FatherPrevSampTestType
  {
    get => fatherPrevSampTestType ?? "";
    set => fatherPrevSampTestType =
      TrimEnd(Substring(value, 1, FatherPrevSampTestType_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherPrevSampTestType attribute.</summary>
  [JsonPropertyName("fatherPrevSampTestType")]
  [Computed]
  public string FatherPrevSampTestType_Json
  {
    get => NullIf(FatherPrevSampTestType, "");
    set => FatherPrevSampTestType = value;
  }

  /// <summary>Length of the FATHER_PREV_SAMPLE_LAB_CASE_NO attribute.</summary>
  public const int FatherPrevSampleLabCaseNo_MaxLength = 11;

  /// <summary>
  /// The value of the FATHER_PREV_SAMPLE_LAB_CASE_NO attribute.
  /// Laboratory case no assigned to the genetic test by test site for the 
  /// previous sample. (generally of the format CYY-999-99999 where YY is the
  /// year).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = FatherPrevSampleLabCaseNo_MaxLength)]
  public string FatherPrevSampleLabCaseNo
  {
    get => fatherPrevSampleLabCaseNo ?? "";
    set => fatherPrevSampleLabCaseNo =
      TrimEnd(Substring(value, 1, FatherPrevSampleLabCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherPrevSampleLabCaseNo attribute.</summary>
  [JsonPropertyName("fatherPrevSampleLabCaseNo")]
  [Computed]
  public string FatherPrevSampleLabCaseNo_Json
  {
    get => NullIf(FatherPrevSampleLabCaseNo, "");
    set => FatherPrevSampleLabCaseNo = value;
  }

  /// <summary>Length of the FATHER_PREV_SAMP_SPECIMEN_ID attribute.</summary>
  public const int FatherPrevSampSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the FATHER_PREV_SAMP_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche) for the previous sample.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = FatherPrevSampSpecimenId_MaxLength)]
  public string FatherPrevSampSpecimenId
  {
    get => fatherPrevSampSpecimenId ?? "";
    set => fatherPrevSampSpecimenId =
      TrimEnd(Substring(value, 1, FatherPrevSampSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherPrevSampSpecimenId attribute.</summary>
  [JsonPropertyName("fatherPrevSampSpecimenId")]
  [Computed]
  public string FatherPrevSampSpecimenId_Json
  {
    get => NullIf(FatherPrevSampSpecimenId, "");
    set => FatherPrevSampSpecimenId = value;
  }

  /// <summary>
  /// The value of the FATHER_PREV_SAMP_PER_GEN_TEST_ID attribute.
  /// IDENTIFIER of the previous PERSON GENETIC TEST from which the sample is 
  /// being reused.
  /// </summary>
  [JsonPropertyName("fatherPrevSampPerGenTestId")]
  [DefaultValue(0)]
  [Member(Index = 27, Type = MemberType.Number, Length = 3)]
  public int FatherPrevSampPerGenTestId
  {
    get => fatherPrevSampPerGenTestId;
    set => fatherPrevSampPerGenTestId = value;
  }

  /// <summary>Length of the FATHER_SPECIMEN_ID attribute.</summary>
  public const int FatherSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the FATHER_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche).
  /// This is known at the time of receipt of test result only.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = FatherSpecimenId_MaxLength)]
  public string FatherSpecimenId
  {
    get => fatherSpecimenId ?? "";
    set => fatherSpecimenId =
      TrimEnd(Substring(value, 1, FatherSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherSpecimenId attribute.</summary>
  [JsonPropertyName("fatherSpecimenId")]
  [Computed]
  public string FatherSpecimenId_Json
  {
    get => NullIf(FatherSpecimenId, "");
    set => FatherSpecimenId = value;
  }

  /// <summary>Length of the FATHER_RESCHEDULED_IND attribute.</summary>
  public const int FatherRescheduledInd_MaxLength = 1;

  /// <summary>
  /// The value of the FATHER_RESCHEDULED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = FatherRescheduledInd_MaxLength)]
  public string FatherRescheduledInd
  {
    get => fatherRescheduledInd ?? "";
    set => fatherRescheduledInd =
      TrimEnd(Substring(value, 1, FatherRescheduledInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FatherRescheduledInd attribute.</summary>
  [JsonPropertyName("fatherRescheduledInd")]
  [Computed]
  public string FatherRescheduledInd_Json
  {
    get => NullIf(FatherRescheduledInd, "");
    set => FatherRescheduledInd = value;
  }

  /// <summary>Length of the MOTHER_PERSON_NO attribute.</summary>
  public const int MotherPersonNo_MaxLength = 10;

  /// <summary>
  /// The value of the MOTHER_PERSON_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = MotherPersonNo_MaxLength)]
    
  public string MotherPersonNo
  {
    get => motherPersonNo ?? "";
    set => motherPersonNo =
      TrimEnd(Substring(value, 1, MotherPersonNo_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherPersonNo attribute.</summary>
  [JsonPropertyName("motherPersonNo")]
  [Computed]
  public string MotherPersonNo_Json
  {
    get => NullIf(MotherPersonNo, "");
    set => MotherPersonNo = value;
  }

  /// <summary>Length of the MOTHER_FORMATTED_NAME attribute.</summary>
  public const int MotherFormattedName_MaxLength = 33;

  /// <summary>
  /// The value of the MOTHER_FORMATTED_NAME attribute.
  /// This is used by the format cse person name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = MotherFormattedName_MaxLength)]
  public string MotherFormattedName
  {
    get => motherFormattedName ?? "";
    set => motherFormattedName =
      TrimEnd(Substring(value, 1, MotherFormattedName_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherFormattedName attribute.</summary>
  [JsonPropertyName("motherFormattedName")]
  [Computed]
  public string MotherFormattedName_Json
  {
    get => NullIf(MotherFormattedName, "");
    set => MotherFormattedName = value;
  }

  /// <summary>Length of the MOTHER_LAST_NAME attribute.</summary>
  public const int MotherLastName_MaxLength = 17;

  /// <summary>
  /// The value of the MOTHER_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = MotherLastName_MaxLength)]
    
  public string MotherLastName
  {
    get => motherLastName ?? "";
    set => motherLastName =
      TrimEnd(Substring(value, 1, MotherLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherLastName attribute.</summary>
  [JsonPropertyName("motherLastName")]
  [Computed]
  public string MotherLastName_Json
  {
    get => NullIf(MotherLastName, "");
    set => MotherLastName = value;
  }

  /// <summary>Length of the MOTHER_MI attribute.</summary>
  public const int MotherMi_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_MI attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = MotherMi_MaxLength)]
  public string MotherMi
  {
    get => motherMi ?? "";
    set => motherMi = TrimEnd(Substring(value, 1, MotherMi_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherMi attribute.</summary>
  [JsonPropertyName("motherMi")]
  [Computed]
  public string MotherMi_Json
  {
    get => NullIf(MotherMi, "");
    set => MotherMi = value;
  }

  /// <summary>Length of the MOTHER_FIRST_NAME attribute.</summary>
  public const int MotherFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the MOTHER_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = MotherFirstName_MaxLength)
    ]
  public string MotherFirstName
  {
    get => motherFirstName ?? "";
    set => motherFirstName =
      TrimEnd(Substring(value, 1, MotherFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherFirstName attribute.</summary>
  [JsonPropertyName("motherFirstName")]
  [Computed]
  public string MotherFirstName_Json
  {
    get => NullIf(MotherFirstName, "");
    set => MotherFirstName = value;
  }

  /// <summary>Length of the MOTHER_DRAW_SITE_ID attribute.</summary>
  public const int MotherDrawSiteId_MaxLength = 10;

  /// <summary>
  /// The value of the MOTHER_DRAW_SITE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = MotherDrawSiteId_MaxLength)]
  public string MotherDrawSiteId
  {
    get => motherDrawSiteId ?? "";
    set => motherDrawSiteId =
      TrimEnd(Substring(value, 1, MotherDrawSiteId_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherDrawSiteId attribute.</summary>
  [JsonPropertyName("motherDrawSiteId")]
  [Computed]
  public string MotherDrawSiteId_Json
  {
    get => NullIf(MotherDrawSiteId, "");
    set => MotherDrawSiteId = value;
  }

  /// <summary>Length of the MOTHER_DRAW_SITE_VENDOR_NAME attribute.</summary>
  public const int MotherDrawSiteVendorName_MaxLength = 30;

  /// <summary>
  /// The value of the MOTHER_DRAW_SITE_VENDOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = MotherDrawSiteVendorName_MaxLength)]
  public string MotherDrawSiteVendorName
  {
    get => motherDrawSiteVendorName ?? "";
    set => motherDrawSiteVendorName =
      TrimEnd(Substring(value, 1, MotherDrawSiteVendorName_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherDrawSiteVendorName attribute.</summary>
  [JsonPropertyName("motherDrawSiteVendorName")]
  [Computed]
  public string MotherDrawSiteVendorName_Json
  {
    get => NullIf(MotherDrawSiteVendorName, "");
    set => MotherDrawSiteVendorName = value;
  }

  /// <summary>Length of the MOTHER_DRAW_SITE_CITY attribute.</summary>
  public const int MotherDrawSiteCity_MaxLength = 15;

  /// <summary>
  /// The value of the MOTHER_DRAW_SITE_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = MotherDrawSiteCity_MaxLength)]
  public string MotherDrawSiteCity
  {
    get => motherDrawSiteCity ?? "";
    set => motherDrawSiteCity =
      TrimEnd(Substring(value, 1, MotherDrawSiteCity_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherDrawSiteCity attribute.</summary>
  [JsonPropertyName("motherDrawSiteCity")]
  [Computed]
  public string MotherDrawSiteCity_Json
  {
    get => NullIf(MotherDrawSiteCity, "");
    set => MotherDrawSiteCity = value;
  }

  /// <summary>Length of the MOTHER_DRAW_SITE_STATE attribute.</summary>
  public const int MotherDrawSiteState_MaxLength = 2;

  /// <summary>
  /// The value of the MOTHER_DRAW_SITE_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = MotherDrawSiteState_MaxLength)]
  public string MotherDrawSiteState
  {
    get => motherDrawSiteState ?? "";
    set => motherDrawSiteState =
      TrimEnd(Substring(value, 1, MotherDrawSiteState_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherDrawSiteState attribute.</summary>
  [JsonPropertyName("motherDrawSiteState")]
  [Computed]
  public string MotherDrawSiteState_Json
  {
    get => NullIf(MotherDrawSiteState, "");
    set => MotherDrawSiteState = value;
  }

  /// <summary>
  /// The value of the MOTHER_SCHED_TEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("motherSchedTestDate")]
  [Member(Index = 39, Type = MemberType.Date)]
  public DateTime? MotherSchedTestDate
  {
    get => motherSchedTestDate;
    set => motherSchedTestDate = value;
  }

  /// <summary>Length of the MOTHER_SCHED_TEST_TIME attribute.</summary>
  public const int MotherSchedTestTime_MaxLength = 5;

  /// <summary>
  /// The value of the MOTHER_SCHED_TEST_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = MotherSchedTestTime_MaxLength)]
  public string MotherSchedTestTime
  {
    get => motherSchedTestTime ?? "";
    set => motherSchedTestTime =
      TrimEnd(Substring(value, 1, MotherSchedTestTime_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherSchedTestTime attribute.</summary>
  [JsonPropertyName("motherSchedTestTime")]
  [Computed]
  public string MotherSchedTestTime_Json
  {
    get => NullIf(MotherSchedTestTime, "");
    set => MotherSchedTestTime = value;
  }

  /// <summary>Length of the MOTHER_COLLECT_SAMPLE_IND attribute.</summary>
  public const int MotherCollectSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_COLLECT_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = MotherCollectSampleInd_MaxLength)]
  public string MotherCollectSampleInd
  {
    get => motherCollectSampleInd ?? "";
    set => motherCollectSampleInd =
      TrimEnd(Substring(value, 1, MotherCollectSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherCollectSampleInd attribute.</summary>
  [JsonPropertyName("motherCollectSampleInd")]
  [Computed]
  public string MotherCollectSampleInd_Json
  {
    get => NullIf(MotherCollectSampleInd, "");
    set => MotherCollectSampleInd = value;
  }

  /// <summary>Length of the MOTHER_REUSE_SAMPLE_IND attribute.</summary>
  public const int MotherReuseSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_REUSE_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = MotherReuseSampleInd_MaxLength)]
  public string MotherReuseSampleInd
  {
    get => motherReuseSampleInd ?? "";
    set => motherReuseSampleInd =
      TrimEnd(Substring(value, 1, MotherReuseSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherReuseSampleInd attribute.</summary>
  [JsonPropertyName("motherReuseSampleInd")]
  [Computed]
  public string MotherReuseSampleInd_Json
  {
    get => NullIf(MotherReuseSampleInd, "");
    set => MotherReuseSampleInd = value;
  }

  /// <summary>Length of the MOTHER_SHOW_IND attribute.</summary>
  public const int MotherShowInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_SHOW_IND attribute.
  /// Specifies whether or not mother showed up for sample collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length = MotherShowInd_MaxLength)]
  public string MotherShowInd
  {
    get => motherShowInd ?? "";
    set => motherShowInd =
      TrimEnd(Substring(value, 1, MotherShowInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherShowInd attribute.</summary>
  [JsonPropertyName("motherShowInd")]
  [Computed]
  public string MotherShowInd_Json
  {
    get => NullIf(MotherShowInd, "");
    set => MotherShowInd = value;
  }

  /// <summary>Length of the MOTHER_SAMPLE_COLLECTED_IND attribute.</summary>
  public const int MotherSampleCollectedInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_SAMPLE_COLLECTED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = MotherSampleCollectedInd_MaxLength)]
  public string MotherSampleCollectedInd
  {
    get => motherSampleCollectedInd ?? "";
    set => motherSampleCollectedInd =
      TrimEnd(Substring(value, 1, MotherSampleCollectedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherSampleCollectedInd attribute.</summary>
  [JsonPropertyName("motherSampleCollectedInd")]
  [Computed]
  public string MotherSampleCollectedInd_Json
  {
    get => NullIf(MotherSampleCollectedInd, "");
    set => MotherSampleCollectedInd = value;
  }

  /// <summary>Length of the MOTHER_PREV_SAMP_EXISTS_IND attribute.</summary>
  public const int MotherPrevSampExistsInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_EXISTS_IND attribute.
  /// Whether or not a previous sample exists and known to the system.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = MotherPrevSampExistsInd_MaxLength)]
  public string MotherPrevSampExistsInd
  {
    get => motherPrevSampExistsInd ?? "";
    set => motherPrevSampExistsInd =
      TrimEnd(Substring(value, 1, MotherPrevSampExistsInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherPrevSampExistsInd attribute.</summary>
  [JsonPropertyName("motherPrevSampExistsInd")]
  [Computed]
  public string MotherPrevSampExistsInd_Json
  {
    get => NullIf(MotherPrevSampExistsInd, "");
    set => MotherPrevSampExistsInd = value;
  }

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_GTEST_NUMBER attribute.
  /// Identifier of the genetic test for the sample being reused.
  /// </summary>
  [JsonPropertyName("motherPrevSampGtestNumber")]
  [DefaultValue(0)]
  [Member(Index = 46, Type = MemberType.Number, Length = 8)]
  public int MotherPrevSampGtestNumber
  {
    get => motherPrevSampGtestNumber;
    set => motherPrevSampGtestNumber = value;
  }

  /// <summary>Length of the MOTHER_PREV_SAMP_TEST_TYPE attribute.</summary>
  public const int MotherPrevSampTestType_MaxLength = 2;

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_TEST_TYPE attribute.
  /// The type of the scientific test used to determine parentage for the 
  /// previous sample taken.
  /// e.g. DNA, Blood test, Buckle Swab etc.
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// GENETIC_TEST_TYPE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = MotherPrevSampTestType_MaxLength)]
  public string MotherPrevSampTestType
  {
    get => motherPrevSampTestType ?? "";
    set => motherPrevSampTestType =
      TrimEnd(Substring(value, 1, MotherPrevSampTestType_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherPrevSampTestType attribute.</summary>
  [JsonPropertyName("motherPrevSampTestType")]
  [Computed]
  public string MotherPrevSampTestType_Json
  {
    get => NullIf(MotherPrevSampTestType, "");
    set => MotherPrevSampTestType = value;
  }

  /// <summary>Length of the MOTHER_PREV_SAMP_LAB_CASE_NO attribute.</summary>
  public const int MotherPrevSampLabCaseNo_MaxLength = 11;

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_LAB_CASE_NO attribute.
  /// Laboratory case no assigned to the genetic test by test site for the 
  /// previous sample. (generally of the format CYY-999-99999 where YY is the
  /// year).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = MotherPrevSampLabCaseNo_MaxLength)]
  public string MotherPrevSampLabCaseNo
  {
    get => motherPrevSampLabCaseNo ?? "";
    set => motherPrevSampLabCaseNo =
      TrimEnd(Substring(value, 1, MotherPrevSampLabCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherPrevSampLabCaseNo attribute.</summary>
  [JsonPropertyName("motherPrevSampLabCaseNo")]
  [Computed]
  public string MotherPrevSampLabCaseNo_Json
  {
    get => NullIf(MotherPrevSampLabCaseNo, "");
    set => MotherPrevSampLabCaseNo = value;
  }

  /// <summary>Length of the MOTHER_PREV_SAMP_SPECIMEN_ID attribute.</summary>
  public const int MotherPrevSampSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche) for the previous sample.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = MotherPrevSampSpecimenId_MaxLength)]
  public string MotherPrevSampSpecimenId
  {
    get => motherPrevSampSpecimenId ?? "";
    set => motherPrevSampSpecimenId =
      TrimEnd(Substring(value, 1, MotherPrevSampSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherPrevSampSpecimenId attribute.</summary>
  [JsonPropertyName("motherPrevSampSpecimenId")]
  [Computed]
  public string MotherPrevSampSpecimenId_Json
  {
    get => NullIf(MotherPrevSampSpecimenId, "");
    set => MotherPrevSampSpecimenId = value;
  }

  /// <summary>
  /// The value of the MOTHER_PREV_SAMP_PER_GEN_TEST_ID attribute.
  /// IDENTIFIER of the previous PERSON GENETIC TEST from which the sample is 
  /// being reused.
  /// </summary>
  [JsonPropertyName("motherPrevSampPerGenTestId")]
  [DefaultValue(0)]
  [Member(Index = 50, Type = MemberType.Number, Length = 3)]
  public int MotherPrevSampPerGenTestId
  {
    get => motherPrevSampPerGenTestId;
    set => motherPrevSampPerGenTestId = value;
  }

  /// <summary>Length of the MOTHER_SPECIMEN_ID attribute.</summary>
  public const int MotherSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the MOTHER_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche).
  /// This is known at the time of receipt of test result only.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = MotherSpecimenId_MaxLength)]
  public string MotherSpecimenId
  {
    get => motherSpecimenId ?? "";
    set => motherSpecimenId =
      TrimEnd(Substring(value, 1, MotherSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherSpecimenId attribute.</summary>
  [JsonPropertyName("motherSpecimenId")]
  [Computed]
  public string MotherSpecimenId_Json
  {
    get => NullIf(MotherSpecimenId, "");
    set => MotherSpecimenId = value;
  }

  /// <summary>Length of the MOTHER_RESCHEDULED_IND attribute.</summary>
  public const int MotherRescheduledInd_MaxLength = 1;

  /// <summary>
  /// The value of the MOTHER_RESCHEDULED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length
    = MotherRescheduledInd_MaxLength)]
  public string MotherRescheduledInd
  {
    get => motherRescheduledInd ?? "";
    set => motherRescheduledInd =
      TrimEnd(Substring(value, 1, MotherRescheduledInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MotherRescheduledInd attribute.</summary>
  [JsonPropertyName("motherRescheduledInd")]
  [Computed]
  public string MotherRescheduledInd_Json
  {
    get => NullIf(MotherRescheduledInd, "");
    set => MotherRescheduledInd = value;
  }

  /// <summary>Length of the CHILD_PERSON_NO attribute.</summary>
  public const int ChildPersonNo_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_PERSON_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = ChildPersonNo_MaxLength)]
  public string ChildPersonNo
  {
    get => childPersonNo ?? "";
    set => childPersonNo =
      TrimEnd(Substring(value, 1, ChildPersonNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPersonNo attribute.</summary>
  [JsonPropertyName("childPersonNo")]
  [Computed]
  public string ChildPersonNo_Json
  {
    get => NullIf(ChildPersonNo, "");
    set => ChildPersonNo = value;
  }

  /// <summary>Length of the CHILD_FORMATTED_NAME attribute.</summary>
  public const int ChildFormattedName_MaxLength = 33;

  /// <summary>
  /// The value of the CHILD_FORMATTED_NAME attribute.
  /// This is used by the format cse person name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = ChildFormattedName_MaxLength)]
  public string ChildFormattedName
  {
    get => childFormattedName ?? "";
    set => childFormattedName =
      TrimEnd(Substring(value, 1, ChildFormattedName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildFormattedName attribute.</summary>
  [JsonPropertyName("childFormattedName")]
  [Computed]
  public string ChildFormattedName_Json
  {
    get => NullIf(ChildFormattedName, "");
    set => ChildFormattedName = value;
  }

  /// <summary>Length of the CHILD_LAST_NAME attribute.</summary>
  public const int ChildLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CHILD_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length = ChildLastName_MaxLength)]
  public string ChildLastName
  {
    get => childLastName ?? "";
    set => childLastName =
      TrimEnd(Substring(value, 1, ChildLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildLastName attribute.</summary>
  [JsonPropertyName("childLastName")]
  [Computed]
  public string ChildLastName_Json
  {
    get => NullIf(ChildLastName, "");
    set => ChildLastName = value;
  }

  /// <summary>Length of the CHILD_MI attribute.</summary>
  public const int ChildMi_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_MI attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length = ChildMi_MaxLength)]
  public string ChildMi
  {
    get => childMi ?? "";
    set => childMi = TrimEnd(Substring(value, 1, ChildMi_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildMi attribute.</summary>
  [JsonPropertyName("childMi")]
  [Computed]
  public string ChildMi_Json
  {
    get => NullIf(ChildMi, "");
    set => ChildMi = value;
  }

  /// <summary>Length of the CHILD_FIRST_NAME attribute.</summary>
  public const int ChildFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CHILD_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length = ChildFirstName_MaxLength)]
    
  public string ChildFirstName
  {
    get => childFirstName ?? "";
    set => childFirstName =
      TrimEnd(Substring(value, 1, ChildFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildFirstName attribute.</summary>
  [JsonPropertyName("childFirstName")]
  [Computed]
  public string ChildFirstName_Json
  {
    get => NullIf(ChildFirstName, "");
    set => ChildFirstName = value;
  }

  /// <summary>Length of the CHILD_DRAW_SITE_ID attribute.</summary>
  public const int ChildDrawSiteId_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_DRAW_SITE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length = ChildDrawSiteId_MaxLength)
    ]
  public string ChildDrawSiteId
  {
    get => childDrawSiteId ?? "";
    set => childDrawSiteId =
      TrimEnd(Substring(value, 1, ChildDrawSiteId_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildDrawSiteId attribute.</summary>
  [JsonPropertyName("childDrawSiteId")]
  [Computed]
  public string ChildDrawSiteId_Json
  {
    get => NullIf(ChildDrawSiteId, "");
    set => ChildDrawSiteId = value;
  }

  /// <summary>Length of the CHILD_DRAW_SITE_VENDOR_NAME attribute.</summary>
  public const int ChildDrawSiteVendorName_MaxLength = 30;

  /// <summary>
  /// The value of the CHILD_DRAW_SITE_VENDOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = ChildDrawSiteVendorName_MaxLength)]
  public string ChildDrawSiteVendorName
  {
    get => childDrawSiteVendorName ?? "";
    set => childDrawSiteVendorName =
      TrimEnd(Substring(value, 1, ChildDrawSiteVendorName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildDrawSiteVendorName attribute.</summary>
  [JsonPropertyName("childDrawSiteVendorName")]
  [Computed]
  public string ChildDrawSiteVendorName_Json
  {
    get => NullIf(ChildDrawSiteVendorName, "");
    set => ChildDrawSiteVendorName = value;
  }

  /// <summary>Length of the CHILD_DRAW_SITE_CITY attribute.</summary>
  public const int ChildDrawSiteCity_MaxLength = 15;

  /// <summary>
  /// The value of the CHILD_DRAW_SITE_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = ChildDrawSiteCity_MaxLength)]
  public string ChildDrawSiteCity
  {
    get => childDrawSiteCity ?? "";
    set => childDrawSiteCity =
      TrimEnd(Substring(value, 1, ChildDrawSiteCity_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildDrawSiteCity attribute.</summary>
  [JsonPropertyName("childDrawSiteCity")]
  [Computed]
  public string ChildDrawSiteCity_Json
  {
    get => NullIf(ChildDrawSiteCity, "");
    set => ChildDrawSiteCity = value;
  }

  /// <summary>Length of the CHILD_DRAW_SITE_STATE attribute.</summary>
  public const int ChildDrawSiteState_MaxLength = 2;

  /// <summary>
  /// The value of the CHILD_DRAW_SITE_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = ChildDrawSiteState_MaxLength)]
  public string ChildDrawSiteState
  {
    get => childDrawSiteState ?? "";
    set => childDrawSiteState =
      TrimEnd(Substring(value, 1, ChildDrawSiteState_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildDrawSiteState attribute.</summary>
  [JsonPropertyName("childDrawSiteState")]
  [Computed]
  public string ChildDrawSiteState_Json
  {
    get => NullIf(ChildDrawSiteState, "");
    set => ChildDrawSiteState = value;
  }

  /// <summary>
  /// The value of the CHILD_SCHED_TEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("childSchedTestDate")]
  [Member(Index = 62, Type = MemberType.Date)]
  public DateTime? ChildSchedTestDate
  {
    get => childSchedTestDate;
    set => childSchedTestDate = value;
  }

  /// <summary>Length of the CHILD_SCHED_TEST_TIME attribute.</summary>
  public const int ChildSchedTestTime_MaxLength = 5;

  /// <summary>
  /// The value of the CHILD_SCHED_TEST_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = ChildSchedTestTime_MaxLength)]
  public string ChildSchedTestTime
  {
    get => childSchedTestTime ?? "";
    set => childSchedTestTime =
      TrimEnd(Substring(value, 1, ChildSchedTestTime_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildSchedTestTime attribute.</summary>
  [JsonPropertyName("childSchedTestTime")]
  [Computed]
  public string ChildSchedTestTime_Json
  {
    get => NullIf(ChildSchedTestTime, "");
    set => ChildSchedTestTime = value;
  }

  /// <summary>Length of the CHILD_COLLECT_SAMPLE_IND attribute.</summary>
  public const int ChildCollectSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_COLLECT_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = ChildCollectSampleInd_MaxLength)]
  public string ChildCollectSampleInd
  {
    get => childCollectSampleInd ?? "";
    set => childCollectSampleInd =
      TrimEnd(Substring(value, 1, ChildCollectSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildCollectSampleInd attribute.</summary>
  [JsonPropertyName("childCollectSampleInd")]
  [Computed]
  public string ChildCollectSampleInd_Json
  {
    get => NullIf(ChildCollectSampleInd, "");
    set => ChildCollectSampleInd = value;
  }

  /// <summary>Length of the CHILD_REUSE_SAMPLE_IND attribute.</summary>
  public const int ChildReuseSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_REUSE_SAMPLE_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length
    = ChildReuseSampleInd_MaxLength)]
  public string ChildReuseSampleInd
  {
    get => childReuseSampleInd ?? "";
    set => childReuseSampleInd =
      TrimEnd(Substring(value, 1, ChildReuseSampleInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildReuseSampleInd attribute.</summary>
  [JsonPropertyName("childReuseSampleInd")]
  [Computed]
  public string ChildReuseSampleInd_Json
  {
    get => NullIf(ChildReuseSampleInd, "");
    set => ChildReuseSampleInd = value;
  }

  /// <summary>Length of the CHILD_SHOW_IND attribute.</summary>
  public const int ChildShowInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_SHOW_IND attribute.
  /// Specifies whether or not child showed up for sample collection.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length = ChildShowInd_MaxLength)]
  public string ChildShowInd
  {
    get => childShowInd ?? "";
    set => childShowInd = TrimEnd(Substring(value, 1, ChildShowInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildShowInd attribute.</summary>
  [JsonPropertyName("childShowInd")]
  [Computed]
  public string ChildShowInd_Json
  {
    get => NullIf(ChildShowInd, "");
    set => ChildShowInd = value;
  }

  /// <summary>Length of the CHILD_SAMPLE_COLLECTED_IND attribute.</summary>
  public const int ChildSampleCollectedInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_SAMPLE_COLLECTED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length
    = ChildSampleCollectedInd_MaxLength)]
  public string ChildSampleCollectedInd
  {
    get => childSampleCollectedInd ?? "";
    set => childSampleCollectedInd =
      TrimEnd(Substring(value, 1, ChildSampleCollectedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildSampleCollectedInd attribute.</summary>
  [JsonPropertyName("childSampleCollectedInd")]
  [Computed]
  public string ChildSampleCollectedInd_Json
  {
    get => NullIf(ChildSampleCollectedInd, "");
    set => ChildSampleCollectedInd = value;
  }

  /// <summary>Length of the CHILD_PREV_SAMP_EXISTS_IND attribute.</summary>
  public const int ChildPrevSampExistsInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_EXISTS_IND attribute.
  /// Whether or not a previous sample exists and known to the system.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length
    = ChildPrevSampExistsInd_MaxLength)]
  public string ChildPrevSampExistsInd
  {
    get => childPrevSampExistsInd ?? "";
    set => childPrevSampExistsInd =
      TrimEnd(Substring(value, 1, ChildPrevSampExistsInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPrevSampExistsInd attribute.</summary>
  [JsonPropertyName("childPrevSampExistsInd")]
  [Computed]
  public string ChildPrevSampExistsInd_Json
  {
    get => NullIf(ChildPrevSampExistsInd, "");
    set => ChildPrevSampExistsInd = value;
  }

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_GTEST_NUMBER attribute.
  /// Identifier of the genetic test for the sample being reused.
  /// </summary>
  [JsonPropertyName("childPrevSampGtestNumber")]
  [DefaultValue(0)]
  [Member(Index = 69, Type = MemberType.Number, Length = 8)]
  public int ChildPrevSampGtestNumber
  {
    get => childPrevSampGtestNumber;
    set => childPrevSampGtestNumber = value;
  }

  /// <summary>Length of the CHILD_PREV_SAMP_TEST_TYPE attribute.</summary>
  public const int ChildPrevSampTestType_MaxLength = 2;

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_TEST_TYPE attribute.
  /// The type of the scientific test used to determine parentage for the 
  /// previous sample taken.
  /// e.g. DNA, Blood test, Buckle Swab etc.
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// GENETIC_TEST_TYPE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length
    = ChildPrevSampTestType_MaxLength)]
  public string ChildPrevSampTestType
  {
    get => childPrevSampTestType ?? "";
    set => childPrevSampTestType =
      TrimEnd(Substring(value, 1, ChildPrevSampTestType_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPrevSampTestType attribute.</summary>
  [JsonPropertyName("childPrevSampTestType")]
  [Computed]
  public string ChildPrevSampTestType_Json
  {
    get => NullIf(ChildPrevSampTestType, "");
    set => ChildPrevSampTestType = value;
  }

  /// <summary>Length of the CHILD_PREV_SAMP_LAB_CASE_NO attribute.</summary>
  public const int ChildPrevSampLabCaseNo_MaxLength = 11;

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_LAB_CASE_NO attribute.
  /// Laboratory case no assigned to the genetic test by test site for the 
  /// previous sample. (generally of the format CYY-999-99999 where YY is the
  /// year).
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 71, Type = MemberType.Char, Length
    = ChildPrevSampLabCaseNo_MaxLength)]
  public string ChildPrevSampLabCaseNo
  {
    get => childPrevSampLabCaseNo ?? "";
    set => childPrevSampLabCaseNo =
      TrimEnd(Substring(value, 1, ChildPrevSampLabCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPrevSampLabCaseNo attribute.</summary>
  [JsonPropertyName("childPrevSampLabCaseNo")]
  [Computed]
  public string ChildPrevSampLabCaseNo_Json
  {
    get => NullIf(ChildPrevSampLabCaseNo, "");
    set => ChildPrevSampLabCaseNo = value;
  }

  /// <summary>Length of the CHILD_PREV_SAMP_SPECIMEN_ID attribute.</summary>
  public const int ChildPrevSampSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche) for the previous sample.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 72, Type = MemberType.Char, Length
    = ChildPrevSampSpecimenId_MaxLength)]
  public string ChildPrevSampSpecimenId
  {
    get => childPrevSampSpecimenId ?? "";
    set => childPrevSampSpecimenId =
      TrimEnd(Substring(value, 1, ChildPrevSampSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPrevSampSpecimenId attribute.</summary>
  [JsonPropertyName("childPrevSampSpecimenId")]
  [Computed]
  public string ChildPrevSampSpecimenId_Json
  {
    get => NullIf(ChildPrevSampSpecimenId, "");
    set => ChildPrevSampSpecimenId = value;
  }

  /// <summary>
  /// The value of the CHILD_PREV_SAMP_PER_GEN_TEST_ID attribute.
  /// IDENTIFIER of the previous PERSON GENETIC TEST from which the sample is 
  /// being reused.
  /// </summary>
  [JsonPropertyName("childPrevSampPerGenTestId")]
  [DefaultValue(0)]
  [Member(Index = 73, Type = MemberType.Number, Length = 3)]
  public int ChildPrevSampPerGenTestId
  {
    get => childPrevSampPerGenTestId;
    set => childPrevSampPerGenTestId = value;
  }

  /// <summary>Length of the CHILD_SPECIMEN_ID attribute.</summary>
  public const int ChildSpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche).
  /// This is known at the time of receipt of test result only.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 74, Type = MemberType.Char, Length = ChildSpecimenId_MaxLength)
    ]
  public string ChildSpecimenId
  {
    get => childSpecimenId ?? "";
    set => childSpecimenId =
      TrimEnd(Substring(value, 1, ChildSpecimenId_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildSpecimenId attribute.</summary>
  [JsonPropertyName("childSpecimenId")]
  [Computed]
  public string ChildSpecimenId_Json
  {
    get => NullIf(ChildSpecimenId, "");
    set => ChildSpecimenId = value;
  }

  /// <summary>Length of the CHILD_RESCHED_IND attribute.</summary>
  public const int ChildReschedInd_MaxLength = 1;

  /// <summary>
  /// The value of the CHILD_RESCHED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 75, Type = MemberType.Char, Length = ChildReschedInd_MaxLength)
    ]
  public string ChildReschedInd
  {
    get => childReschedInd ?? "";
    set => childReschedInd =
      TrimEnd(Substring(value, 1, ChildReschedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildReschedInd attribute.</summary>
  [JsonPropertyName("childReschedInd")]
  [Computed]
  public string ChildReschedInd_Json
  {
    get => NullIf(ChildReschedInd, "");
    set => ChildReschedInd = value;
  }

  /// <summary>Length of the TEST_SITE_VENDOR_ID attribute.</summary>
  public const int TestSiteVendorId_MaxLength = 10;

  /// <summary>
  /// The value of the TEST_SITE_VENDOR_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 76, Type = MemberType.Char, Length
    = TestSiteVendorId_MaxLength)]
  public string TestSiteVendorId
  {
    get => testSiteVendorId ?? "";
    set => testSiteVendorId =
      TrimEnd(Substring(value, 1, TestSiteVendorId_MaxLength));
  }

  /// <summary>
  /// The json value of the TestSiteVendorId attribute.</summary>
  [JsonPropertyName("testSiteVendorId")]
  [Computed]
  public string TestSiteVendorId_Json
  {
    get => NullIf(TestSiteVendorId, "");
    set => TestSiteVendorId = value;
  }

  /// <summary>Length of the TEST_SITE_VENDOR_NAME attribute.</summary>
  public const int TestSiteVendorName_MaxLength = 30;

  /// <summary>
  /// The value of the TEST_SITE_VENDOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 77, Type = MemberType.Char, Length
    = TestSiteVendorName_MaxLength)]
  public string TestSiteVendorName
  {
    get => testSiteVendorName ?? "";
    set => testSiteVendorName =
      TrimEnd(Substring(value, 1, TestSiteVendorName_MaxLength));
  }

  /// <summary>
  /// The json value of the TestSiteVendorName attribute.</summary>
  [JsonPropertyName("testSiteVendorName")]
  [Computed]
  public string TestSiteVendorName_Json
  {
    get => NullIf(TestSiteVendorName, "");
    set => TestSiteVendorName = value;
  }

  /// <summary>Length of the TEST_SITE_CITY attribute.</summary>
  public const int TestSiteCity_MaxLength = 15;

  /// <summary>
  /// The value of the TEST_SITE_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 78, Type = MemberType.Char, Length = TestSiteCity_MaxLength)]
  public string TestSiteCity
  {
    get => testSiteCity ?? "";
    set => testSiteCity = TrimEnd(Substring(value, 1, TestSiteCity_MaxLength));
  }

  /// <summary>
  /// The json value of the TestSiteCity attribute.</summary>
  [JsonPropertyName("testSiteCity")]
  [Computed]
  public string TestSiteCity_Json
  {
    get => NullIf(TestSiteCity, "");
    set => TestSiteCity = value;
  }

  /// <summary>Length of the TEST_SITE_STATE attribute.</summary>
  public const int TestSiteState_MaxLength = 2;

  /// <summary>
  /// The value of the TEST_SITE_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 79, Type = MemberType.Char, Length = TestSiteState_MaxLength)]
  public string TestSiteState
  {
    get => testSiteState ?? "";
    set => testSiteState =
      TrimEnd(Substring(value, 1, TestSiteState_MaxLength));
  }

  /// <summary>
  /// The json value of the TestSiteState attribute.</summary>
  [JsonPropertyName("testSiteState")]
  [Computed]
  public string TestSiteState_Json
  {
    get => NullIf(TestSiteState, "");
    set => TestSiteState = value;
  }

  /// <summary>
  /// The value of the ACTUAL_TEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("actualTestDate")]
  [Member(Index = 80, Type = MemberType.Date)]
  public DateTime? ActualTestDate
  {
    get => actualTestDate;
    set => actualTestDate = value;
  }

  /// <summary>
  /// The value of the SCHEDULED_TEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("scheduledTestDate")]
  [Member(Index = 81, Type = MemberType.Date)]
  public DateTime? ScheduledTestDate
  {
    get => scheduledTestDate;
    set => scheduledTestDate = value;
  }

  /// <summary>
  /// The value of the RESULT_RECEIVED_DATE attribute.
  /// </summary>
  [JsonPropertyName("resultReceivedDate")]
  [Member(Index = 82, Type = MemberType.Date)]
  public DateTime? ResultReceivedDate
  {
    get => resultReceivedDate;
    set => resultReceivedDate = value;
  }

  /// <summary>Length of the PATERNITY_EXCLUDED_IND attribute.</summary>
  public const int PaternityExcludedInd_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_EXCLUDED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 83, Type = MemberType.Char, Length
    = PaternityExcludedInd_MaxLength)]
  public string PaternityExcludedInd
  {
    get => paternityExcludedInd ?? "";
    set => paternityExcludedInd =
      TrimEnd(Substring(value, 1, PaternityExcludedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PaternityExcludedInd attribute.</summary>
  [JsonPropertyName("paternityExcludedInd")]
  [Computed]
  public string PaternityExcludedInd_Json
  {
    get => NullIf(PaternityExcludedInd, "");
    set => PaternityExcludedInd = value;
  }

  /// <summary>Length of the PREV_PATERNITY_EXCLUDED_IND attribute.</summary>
  public const int PrevPaternityExcludedInd_MaxLength = 1;

  /// <summary>
  /// The value of the PREV_PATERNITY_EXCLUDED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 84, Type = MemberType.Char, Length
    = PrevPaternityExcludedInd_MaxLength)]
  public string PrevPaternityExcludedInd
  {
    get => prevPaternityExcludedInd ?? "";
    set => prevPaternityExcludedInd =
      TrimEnd(Substring(value, 1, PrevPaternityExcludedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PrevPaternityExcludedInd attribute.</summary>
  [JsonPropertyName("prevPaternityExcludedInd")]
  [Computed]
  public string PrevPaternityExcludedInd_Json
  {
    get => NullIf(PrevPaternityExcludedInd, "");
    set => PrevPaternityExcludedInd = value;
  }

  /// <summary>
  /// The value of the PATERNITY_PROBABILITY attribute.
  /// </summary>
  [JsonPropertyName("paternityProbability")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 85, Type = MemberType.Number, Length = 6, Precision = 2)]
  public decimal PaternityProbability
  {
    get => paternityProbability;
    set => paternityProbability = Truncate(value, 2);
  }

  /// <summary>Length of the RESULT_CONTESTED_IND attribute.</summary>
  public const int ResultContestedInd_MaxLength = 1;

  /// <summary>
  /// The value of the RESULT_CONTESTED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 86, Type = MemberType.Char, Length
    = ResultContestedInd_MaxLength)]
  public string ResultContestedInd
  {
    get => resultContestedInd ?? "";
    set => resultContestedInd =
      TrimEnd(Substring(value, 1, ResultContestedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ResultContestedInd attribute.</summary>
  [JsonPropertyName("resultContestedInd")]
  [Computed]
  public string ResultContestedInd_Json
  {
    get => NullIf(ResultContestedInd, "");
    set => ResultContestedInd = value;
  }

  /// <summary>
  /// The value of the CONTEST_STARTED_DATE attribute.
  /// Date on which contest of result started.
  /// </summary>
  [JsonPropertyName("contestStartedDate")]
  [Member(Index = 87, Type = MemberType.Date)]
  public DateTime? ContestStartedDate
  {
    get => contestStartedDate;
    set => contestStartedDate = value;
  }

  /// <summary>
  /// The value of the CONTEST_ENDED_DATE attribute.
  /// Date on which the contest of result ended.
  /// </summary>
  [JsonPropertyName("contestEndedDate")]
  [Member(Index = 88, Type = MemberType.Date)]
  public DateTime? ContestEndedDate
  {
    get => contestEndedDate;
    set => contestEndedDate = value;
  }

  private DateTime? childDob;
  private string caseNumber;
  private string courtOrderNo;
  private string geneticTestAccountNo;
  private string labCaseNo;
  private string testType;
  private string fatherPersonNo;
  private string fatherFormattedName;
  private string fatherLastName;
  private string fatherMi;
  private string fatherFirstName;
  private string fatherDrawSiteId;
  private string fatherDrawSiteVendorName;
  private string fatherDrawSiteCity;
  private string fatherDrawSiteState;
  private DateTime? fatherSchedTestDate;
  private string fatherSchedTestTime;
  private string fatherCollectSampleInd;
  private string fatherReuseSampleInd;
  private string fatherShowInd;
  private string fatherSampleCollectedInd;
  private string fatherPrevSampExistsInd;
  private int fatherPrevSampGtestNumber;
  private string fatherPrevSampTestType;
  private string fatherPrevSampleLabCaseNo;
  private string fatherPrevSampSpecimenId;
  private int fatherPrevSampPerGenTestId;
  private string fatherSpecimenId;
  private string fatherRescheduledInd;
  private string motherPersonNo;
  private string motherFormattedName;
  private string motherLastName;
  private string motherMi;
  private string motherFirstName;
  private string motherDrawSiteId;
  private string motherDrawSiteVendorName;
  private string motherDrawSiteCity;
  private string motherDrawSiteState;
  private DateTime? motherSchedTestDate;
  private string motherSchedTestTime;
  private string motherCollectSampleInd;
  private string motherReuseSampleInd;
  private string motherShowInd;
  private string motherSampleCollectedInd;
  private string motherPrevSampExistsInd;
  private int motherPrevSampGtestNumber;
  private string motherPrevSampTestType;
  private string motherPrevSampLabCaseNo;
  private string motherPrevSampSpecimenId;
  private int motherPrevSampPerGenTestId;
  private string motherSpecimenId;
  private string motherRescheduledInd;
  private string childPersonNo;
  private string childFormattedName;
  private string childLastName;
  private string childMi;
  private string childFirstName;
  private string childDrawSiteId;
  private string childDrawSiteVendorName;
  private string childDrawSiteCity;
  private string childDrawSiteState;
  private DateTime? childSchedTestDate;
  private string childSchedTestTime;
  private string childCollectSampleInd;
  private string childReuseSampleInd;
  private string childShowInd;
  private string childSampleCollectedInd;
  private string childPrevSampExistsInd;
  private int childPrevSampGtestNumber;
  private string childPrevSampTestType;
  private string childPrevSampLabCaseNo;
  private string childPrevSampSpecimenId;
  private int childPrevSampPerGenTestId;
  private string childSpecimenId;
  private string childReschedInd;
  private string testSiteVendorId;
  private string testSiteVendorName;
  private string testSiteCity;
  private string testSiteState;
  private DateTime? actualTestDate;
  private DateTime? scheduledTestDate;
  private DateTime? resultReceivedDate;
  private string paternityExcludedInd;
  private string prevPaternityExcludedInd;
  private decimal paternityProbability;
  private string resultContestedInd;
  private DateTime? contestStartedDate;
  private DateTime? contestEndedDate;
}

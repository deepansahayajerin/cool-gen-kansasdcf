// The source file: GENETIC_TEST, ID: 371434878, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Scope: A clinical test done by an independent laboratory.
/// Qualification: to determine the probability of paternity of a particular 
/// Absent Parent for a particular Child.
/// Includes:  Blood Test, DNA Test
/// Excludes: Lie Detector Test, Driving Test
/// Examples:
/// </summary>
[Serializable]
public partial class GeneticTest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GeneticTest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GeneticTest(GeneticTest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GeneticTest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GeneticTest that)
  {
    base.Assign(that);
    labCaseNo = that.labCaseNo;
    testNumber = that.testNumber;
    testType = that.testType;
    actualTestDate = that.actualTestDate;
    testResultReceivedDate = that.testResultReceivedDate;
    paternityExclusionInd = that.paternityExclusionInd;
    paternityProbability = that.paternityProbability;
    noticeOfContestReceivedInd = that.noticeOfContestReceivedInd;
    startDateOfContest = that.startDateOfContest;
    endDateOfContest = that.endDateOfContest;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    croAIdentifier = that.croAIdentifier;
    croAType = that.croAType;
    casANumber = that.casANumber;
    cspANumber = that.cspANumber;
    croIdentifier = that.croIdentifier;
    croType = that.croType;
    casNumber = that.casNumber;
    cspNumber = that.cspNumber;
    croMIdentifier = that.croMIdentifier;
    croMType = that.croMType;
    casMNumber = that.casMNumber;
    cspMNumber = that.cspMNumber;
    gtaAccountNumber = that.gtaAccountNumber;
    lgaIdentifier = that.lgaIdentifier;
    venIdentifier = that.venIdentifier;
  }

  /// <summary>Length of the LAB_CASE_NO attribute.</summary>
  public const int LabCaseNo_MaxLength = 11;

  /// <summary>
  /// The value of the LAB_CASE_NO attribute.
  /// Laboratory case no assigned to the genetic test by test site. (generally 
  /// of the format CYY-999-99999 where YY is the year).
  /// </summary>
  [JsonPropertyName("labCaseNo")]
  [Member(Index = 1, Type = MemberType.Char, Length = LabCaseNo_MaxLength, Optional
    = true)]
  public string LabCaseNo
  {
    get => labCaseNo;
    set => labCaseNo = value != null
      ? TrimEnd(Substring(value, 1, LabCaseNo_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TEST_NUMBER attribute.
  /// A unique identifier of a genetic test.
  /// </summary>
  [JsonPropertyName("testNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 8)]
  public int TestNumber
  {
    get => testNumber;
    set => testNumber = value;
  }

  /// <summary>Length of the TEST_TYPE attribute.</summary>
  public const int TestType_MaxLength = 2;

  /// <summary>
  /// The value of the TEST_TYPE attribute.
  /// The type of the scientific test used to determine parentage.
  /// e.g. DNA, Blood test, Buckle Swab etc.
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// GENETIC_TEST_TYPE.
  /// </summary>
  [JsonPropertyName("testType")]
  [Member(Index = 3, Type = MemberType.Char, Length = TestType_MaxLength, Optional
    = true)]
  public string TestType
  {
    get => testType;
    set => testType = value != null
      ? TrimEnd(Substring(value, 1, TestType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACTUAL_TEST_DATE attribute.
  /// The date the genetic test was taken at a particular test site.
  /// </summary>
  [JsonPropertyName("actualTestDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? ActualTestDate
  {
    get => actualTestDate;
    set => actualTestDate = value;
  }

  /// <summary>
  /// The value of the TEST_RESULT_RECEIVED_DATE attribute.
  /// The date a local CSE office received the genetic test results.
  /// </summary>
  [JsonPropertyName("testResultReceivedDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? TestResultReceivedDate
  {
    get => testResultReceivedDate;
    set => testResultReceivedDate = value;
  }

  /// <summary>Length of the PATERNITY_EXCLUSION_IND attribute.</summary>
  public const int PaternityExclusionInd_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_EXCLUSION_IND attribute.
  /// An indicator stating that Absent Parent has been excluded as the father of
  /// said child.
  /// </summary>
  [JsonPropertyName("paternityExclusionInd")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = PaternityExclusionInd_MaxLength, Optional = true)]
  public string PaternityExclusionInd
  {
    get => paternityExclusionInd;
    set => paternityExclusionInd = value != null
      ? TrimEnd(Substring(value, 1, PaternityExclusionInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PATERNITY_PROBABILITY attribute.
  /// A percentage indicating the results of the genetic test on a particular 
  /// father.
  /// </summary>
  [JsonPropertyName("paternityProbability")]
  [Member(Index = 7, Type = MemberType.Number, Length = 4, Precision = 2, Optional
    = true)]
  public decimal? PaternityProbability
  {
    get => paternityProbability;
    set => paternityProbability = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the NOTICE_OF_CONTEST_RECEIVED_IND attribute.</summary>
  public const int NoticeOfContestReceivedInd_MaxLength = 3;

  /// <summary>
  /// The value of the NOTICE_OF_CONTEST_RECEIVED_IND attribute.
  /// An indicator stating that one party disagrees with the genetic test 
  /// results.
  /// </summary>
  [JsonPropertyName("noticeOfContestReceivedInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = NoticeOfContestReceivedInd_MaxLength, Optional = true)]
  public string NoticeOfContestReceivedInd
  {
    get => noticeOfContestReceivedInd;
    set => noticeOfContestReceivedInd = value != null
      ? TrimEnd(Substring(value, 1, NoticeOfContestReceivedInd_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the START_DATE_OF_CONTEST attribute.
  /// The date on which on the contest of paternity result was recorded.
  /// </summary>
  [JsonPropertyName("startDateOfContest")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDateOfContest
  {
    get => startDateOfContest;
    set => startDateOfContest = value;
  }

  /// <summary>
  /// The value of the END_DATE_OF_CONTEST attribute.
  /// The date on which the contest of paternity result was lifted.
  /// </summary>
  [JsonPropertyName("endDateOfContest")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDateOfContest
  {
    get => endDateOfContest;
    set => endDateOfContest = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 14, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croAIdentifier")]
  [Member(Index = 15, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CroAIdentifier
  {
    get => croAIdentifier;
    set => croAIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroAType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonPropertyName("croAType")]
  [Member(Index = 16, Type = MemberType.Char, Length = CroAType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroAType
  {
    get => croAType;
    set => croAType = value != null
      ? TrimEnd(Substring(value, 1, CroAType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasANumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casANumber")]
  [Member(Index = 17, Type = MemberType.Char, Length = CasANumber_MaxLength, Optional
    = true)]
  public string CasANumber
  {
    get => casANumber;
    set => casANumber = value != null
      ? TrimEnd(Substring(value, 1, CasANumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspANumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspANumber")]
  [Member(Index = 18, Type = MemberType.Char, Length = CspANumber_MaxLength, Optional
    = true)]
  public string CspANumber
  {
    get => cspANumber;
    set => cspANumber = value != null
      ? TrimEnd(Substring(value, 1, CspANumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croIdentifier")]
  [Member(Index = 19, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CroIdentifier
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
  [JsonPropertyName("croType")]
  [Member(Index = 20, Type = MemberType.Char, Length = CroType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroType
  {
    get => croType;
    set => croType = value != null
      ? TrimEnd(Substring(value, 1, CroType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casNumber")]
  [Member(Index = 21, Type = MemberType.Char, Length = CasNumber_MaxLength, Optional
    = true)]
  public string CasNumber
  {
    get => casNumber;
    set => casNumber = value != null
      ? TrimEnd(Substring(value, 1, CasNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 22, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier.  System generated using a 3-digit random generator
  /// </summary>
  [JsonPropertyName("croMIdentifier")]
  [Member(Index = 23, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CroMIdentifier
  {
    get => croMIdentifier;
    set => croMIdentifier = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CroMType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of role played by a CSE Person in a given case.
  /// </summary>
  [JsonPropertyName("croMType")]
  [Member(Index = 24, Type = MemberType.Char, Length = CroMType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("MO")]
  [Value("FA")]
  [Value("CH")]
  [Value("AR")]
  [Value("AP")]
  public string CroMType
  {
    get => croMType;
    set => croMType = value != null
      ? TrimEnd(Substring(value, 1, CroMType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasMNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("casMNumber")]
  [Member(Index = 25, Type = MemberType.Char, Length = CasMNumber_MaxLength, Optional
    = true)]
  public string CasMNumber
  {
    get => casMNumber;
    set => casMNumber = value != null
      ? TrimEnd(Substring(value, 1, CasMNumber_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspMNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspMNumber")]
  [Member(Index = 26, Type = MemberType.Char, Length = CspMNumber_MaxLength, Optional
    = true)]
  public string CspMNumber
  {
    get => cspMNumber;
    set => cspMNumber = value != null
      ? TrimEnd(Substring(value, 1, CspMNumber_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_NUMBER attribute.</summary>
  public const int GtaAccountNumber_MaxLength = 8;

  /// <summary>
  /// The value of the ACCOUNT_NUMBER attribute.
  /// A billing number for the cost of test result to a particular CSE office.
  /// </summary>
  [JsonPropertyName("gtaAccountNumber")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = GtaAccountNumber_MaxLength, Optional = true)]
  public string GtaAccountNumber
  {
    get => gtaAccountNumber;
    set => gtaAccountNumber = value != null
      ? TrimEnd(Substring(value, 1, GtaAccountNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique system generated number that descripts the details of a particular 
  /// vendor.
  /// </summary>
  [JsonPropertyName("venIdentifier")]
  [Member(Index = 29, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? VenIdentifier
  {
    get => venIdentifier;
    set => venIdentifier = value;
  }

  private string labCaseNo;
  private int testNumber;
  private string testType;
  private DateTime? actualTestDate;
  private DateTime? testResultReceivedDate;
  private string paternityExclusionInd;
  private decimal? paternityProbability;
  private string noticeOfContestReceivedInd;
  private DateTime? startDateOfContest;
  private DateTime? endDateOfContest;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? croAIdentifier;
  private string croAType;
  private string casANumber;
  private string cspANumber;
  private int? croIdentifier;
  private string croType;
  private string casNumber;
  private string cspNumber;
  private int? croMIdentifier;
  private string croMType;
  private string casMNumber;
  private string cspMNumber;
  private string gtaAccountNumber;
  private int? lgaIdentifier;
  private int? venIdentifier;
}

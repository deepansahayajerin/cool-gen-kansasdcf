// The source file: OCSE157_VERIFICATION, ID: 371092476, model: 746.
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINNANCE
/// 
/// This entity type is used to store ocse157 report verification data for
/// audit purposes.
/// </summary>
[Serializable]
public partial class Ocse157Verification: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Ocse157Verification()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Ocse157Verification(Ocse157Verification that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Ocse157Verification Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the FISCAL_YEAR attribute.
  /// Reporting year.
  /// </summary>
  [JsonPropertyName("fiscalYear")]
  [Member(Index = 1, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? FiscalYear
  {
    get => Get<int?>("fiscalYear");
    set => Set("fiscalYear", value);
  }

  /// <summary>
  /// The value of the RUN_NUMBER attribute.
  /// Used as additional qualifier incase report needs to be ran multiple times 
  /// for the same FY. Set to 1 for first run, incremented by 1 for subsequent
  /// runs.
  /// </summary>
  [JsonPropertyName("runNumber")]
  [Member(Index = 2, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? RunNumber
  {
    get => Get<int?>("runNumber");
    set => Set("runNumber", value);
  }

  /// <summary>Length of the LINE_NUMBER attribute.</summary>
  public const int LineNumber_MaxLength = 3;

  /// <summary>
  /// The value of the LINE_NUMBER attribute.
  /// Line being reported. Eg. 1d, 24, 18a, ..etc.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length = LineNumber_MaxLength, Optional
    = true)]
  public string LineNumber
  {
    get => Get<string>("lineNumber");
    set =>
      Set("lineNumber", TrimEnd(Substring(value, 1, LineNumber_MaxLength)));
  }

  /// <summary>Length of the COLUMN attribute.</summary>
  public const int Column_MaxLength = 1;

  /// <summary>
  /// The value of the COLUMN attribute.
  /// Column being reported. Example a-total, b-current assistance, c-former 
  /// assistance, d-never assistance.
  /// </summary>
  [JsonPropertyName("column")]
  [Member(Index = 4, Type = MemberType.Char, Length = Column_MaxLength, Optional
    = true)]
  public string Column
  {
    get => Get<string>("column");
    set => Set("column", TrimEnd(Substring(value, 1, Column_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Created_timestamp-also used as primary identifier.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// KAECSES CSE Case#.Eg. 0000000154
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => Get<string>("caseNumber");
    set =>
      Set("caseNumber", TrimEnd(Substring(value, 1, CaseNumber_MaxLength)));
  }

  /// <summary>Length of the SUPP_PERSON_NUMBER attribute.</summary>
  public const int SuppPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the SUPP_PERSON_NUMBER attribute.
  /// Usually the child or AR-.Eg.0000010454
  /// </summary>
  [JsonPropertyName("suppPersonNumber")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = SuppPersonNumber_MaxLength, Optional = true)]
  public string SuppPersonNumber
  {
    get => Get<string>("suppPersonNumber");
    set => Set(
      "suppPersonNumber",
      TrimEnd(Substring(value, 1, SuppPersonNumber_MaxLength)));
  }

  /// <summary>Length of the OBLIGOR_PERSON_NBR attribute.</summary>
  public const int ObligorPersonNbr_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_PERSON_NBR attribute.
  /// Usually the AP-.Eg.0000010454.
  /// </summary>
  [JsonPropertyName("obligorPersonNbr")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = ObligorPersonNbr_MaxLength, Optional = true)]
  public string ObligorPersonNbr
  {
    get => Get<string>("obligorPersonNbr");
    set => Set(
      "obligorPersonNbr",
      TrimEnd(Substring(value, 1, ObligorPersonNbr_MaxLength)));
  }

  /// <summary>
  /// The value of the DTE_PATERNITY_EST attribute.
  /// This field is set to Cse_person. date_paternity_established.
  /// </summary>
  [JsonPropertyName("dtePaternityEst")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? DtePaternityEst
  {
    get => Get<DateTime?>("dtePaternityEst");
    set => Set("dtePaternityEst", value);
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// This field is set to Legal_Action. standard_number
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => Get<string>("courtOrderNumber");
    set => Set(
      "courtOrderNumber",
      TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the LEGAL_CREATED_DTE attribute.
  /// This field is set to Legal_action_detail Created_date. Written for Line 
  /// 17.
  /// </summary>
  [JsonPropertyName("legalCreatedDte")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? LegalCreatedDte
  {
    get => Get<DateTime?>("legalCreatedDte");
    set => Set("legalCreatedDte", value);
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// Date of Birth.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfBirth
  {
    get => Get<DateTime?>("dateOfBirth");
    set => Set("dateOfBirth", value);
  }

  /// <summary>Length of the PLACE_OF_BIRTH attribute.</summary>
  public const int PlaceOfBirth_MaxLength = 2;

  /// <summary>
  /// The value of the PLACE_OF_BIRTH attribute.
  /// Indicates the state of birth.
  /// </summary>
  [JsonPropertyName("placeOfBirth")]
  [Member(Index = 13, Type = MemberType.Char, Length = PlaceOfBirth_MaxLength, Optional
    = true)]
  public string PlaceOfBirth
  {
    get => Get<string>("placeOfBirth");
    set => Set(
      "placeOfBirth", TrimEnd(Substring(value, 1, PlaceOfBirth_MaxLength)));
  }

  /// <summary>
  /// The value of the SOCIAL_SECURITY_NUMBER attribute.
  /// Social Security Number
  /// </summary>
  [JsonPropertyName("socialSecurityNumber")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SocialSecurityNumber
  {
    get => Get<int?>("socialSecurityNumber");
    set => Set("socialSecurityNumber", value);
  }

  /// <summary>
  /// The value of the OB_TRAN_SGI attribute.
  /// Sequential Generated Identifier on Obligation_transaction.
  /// </summary>
  [JsonPropertyName("obTranSgi")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ObTranSgi
  {
    get => Get<int?>("obTranSgi");
    set => Set("obTranSgi", value);
  }

  /// <summary>Length of the OB_TRAN_TYPE attribute.</summary>
  public const int ObTranType_MaxLength = 2;

  /// <summary>
  /// The value of the OB_TRAN_TYPE attribute.
  /// Indicates DE-debt or DA-debt adjustment.
  /// </summary>
  [JsonPropertyName("obTranType")]
  [Member(Index = 16, Type = MemberType.Char, Length = ObTranType_MaxLength, Optional
    = true)]
  public string ObTranType
  {
    get => Get<string>("obTranType");
    set =>
      Set("obTranType", TrimEnd(Substring(value, 1, ObTranType_MaxLength)));
  }

  /// <summary>
  /// The value of the OB_TRAN_AMOUNT attribute.
  /// Indicates the amount of Debt or Debt Adjustment.
  /// </summary>
  [JsonPropertyName("obTranAmount")]
  [Member(Index = 17, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ObTranAmount
  {
    get => Get<decimal?>("obTranAmount");
    set => Set("obTranAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the OBLIGATION_SGI attribute.
  /// Sequential Generated Identifier on Obligation.
  /// </summary>
  [JsonPropertyName("obligationSgi")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ObligationSgi
  {
    get => Get<int?>("obligationSgi");
    set => Set("obligationSgi", value);
  }

  /// <summary>Length of the DEBT_ADJ_TYPE attribute.</summary>
  public const int DebtAdjType_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_ADJ_TYPE attribute.
  /// D-decreases original debt amount. I-increases it.
  /// </summary>
  [JsonPropertyName("debtAdjType")]
  [Member(Index = 19, Type = MemberType.Char, Length = DebtAdjType_MaxLength, Optional
    = true)]
  public string DebtAdjType
  {
    get => Get<string>("debtAdjType");
    set => Set(
      "debtAdjType", TrimEnd(Substring(value, 1, DebtAdjType_MaxLength)));
  }

  /// <summary>
  /// The value of the DEBT_DETAIL_DUE_DT attribute.
  /// Debt Detail Due Date
  /// </summary>
  [JsonPropertyName("debtDetailDueDt")]
  [Member(Index = 20, Type = MemberType.Date, Optional = true)]
  public DateTime? DebtDetailDueDt
  {
    get => Get<DateTime?>("debtDetailDueDt");
    set => Set("debtDetailDueDt", value);
  }

  /// <summary>
  /// The value of the DEBT_DETAIL_BALANCE_DUE attribute.
  /// Debt Detail Balance Due
  /// </summary>
  [JsonPropertyName("debtDetailBalanceDue")]
  [Member(Index = 21, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? DebtDetailBalanceDue
  {
    get => Get<decimal?>("debtDetailBalanceDue");
    set => Set("debtDetailBalanceDue", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the OB_TYPE_SGI attribute.
  /// Sequential Generated Identifier on Obligation Type.
  /// </summary>
  [JsonPropertyName("obTypeSgi")]
  [Member(Index = 22, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObTypeSgi
  {
    get => Get<int?>("obTypeSgi");
    set => Set("obTypeSgi", value);
  }

  /// <summary>Length of the OB_TYPE_CLASSFCTN attribute.</summary>
  public const int ObTypeClassfctn_MaxLength = 1;

  /// <summary>
  /// The value of the OB_TYPE_CLASSFCTN attribute.
  /// Obligation Type Classification. F-Fees,R-Recoveries.
  /// </summary>
  [JsonPropertyName("obTypeClassfctn")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = ObTypeClassfctn_MaxLength, Optional = true)]
  public string ObTypeClassfctn
  {
    get => Get<string>("obTypeClassfctn");
    set => Set(
      "obTypeClassfctn",
      TrimEnd(Substring(value, 1, ObTypeClassfctn_MaxLength)));
  }

  /// <summary>
  /// The value of the COLLECTION_SGI attribute.
  /// Sequential Generated Identifier on Collection.
  /// </summary>
  [JsonPropertyName("collectionSgi")]
  [Member(Index = 24, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollectionSgi
  {
    get => Get<int?>("collectionSgi");
    set => Set("collectionSgi", value);
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// Amount field on Collection
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 25, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => Get<decimal?>("collectionAmount");
    set => Set("collectionAmount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTION_DTE attribute.
  /// Collection date from Collection.
  /// </summary>
  [JsonPropertyName("collectionDte")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionDte
  {
    get => Get<DateTime?>("collectionDte");
    set => Set("collectionDte", value);
  }

  /// <summary>Length of the COLL_APPL_TO_CODE attribute.</summary>
  public const int CollApplToCode_MaxLength = 1;

  /// <summary>
  /// The value of the COLL_APPL_TO_CODE attribute.
  /// Applied_to_code from Collection. Defines whether collection was applied to
  /// C-Current, A-Arrears, G-Gift or I-Interest.
  /// </summary>
  [JsonPropertyName("collApplToCode")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = CollApplToCode_MaxLength, Optional = true)]
  public string CollApplToCode
  {
    get => Get<string>("collApplToCode");
    set => Set(
      "collApplToCode", TrimEnd(Substring(value, 1, CollApplToCode_MaxLength)));
      
  }

  /// <summary>
  /// The value of the COLL_CREATED_DTE attribute.
  /// From created_timestamp on collection. Indicates the date of distribution.
  /// </summary>
  [JsonPropertyName("collCreatedDte")]
  [Member(Index = 28, Type = MemberType.Date, Optional = true)]
  public DateTime? CollCreatedDte
  {
    get => Get<DateTime?>("collCreatedDte");
    set => Set("collCreatedDte", value);
  }

  /// <summary>Length of the CASE_ROLE_TYPE attribute.</summary>
  public const int CaseRoleType_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_ROLE_TYPE attribute.
  /// Type From Case Role. AR,AP,CH.
  /// </summary>
  [JsonPropertyName("caseRoleType")]
  [Member(Index = 29, Type = MemberType.Char, Length = CaseRoleType_MaxLength, Optional
    = true)]
  public string CaseRoleType
  {
    get => Get<string>("caseRoleType");
    set => Set(
      "caseRoleType", TrimEnd(Substring(value, 1, CaseRoleType_MaxLength)));
  }

  /// <summary>Length of the CASE_WORKER_NUMBER attribute.</summary>
  public const int CaseWorkerNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_WORKER_NUMBER attribute.
  /// Required for Line 9a and/or Line 16. Possibly the service provider user 
  /// id.
  /// </summary>
  [JsonPropertyName("caseWorkerNumber")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = CaseWorkerNumber_MaxLength, Optional = true)]
  public string CaseWorkerNumber
  {
    get => Get<string>("caseWorkerNumber");
    set => Set(
      "caseWorkerNumber",
      TrimEnd(Substring(value, 1, CaseWorkerNumber_MaxLength)));
  }

  /// <summary>Length of the CASE_WORKER_NAME attribute.</summary>
  public const int CaseWorkerName_MaxLength = 30;

  /// <summary>
  /// The value of the CASE_WORKER_NAME attribute.
  /// Required for Line 9a and/or Line16. Possibly the service provider name.
  /// </summary>
  [JsonPropertyName("caseWorkerName")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = CaseWorkerName_MaxLength, Optional = true)]
  public string CaseWorkerName
  {
    get => Get<string>("caseWorkerName");
    set => Set(
      "caseWorkerName", TrimEnd(Substring(value, 1, CaseWorkerName_MaxLength)));
      
  }

  /// <summary>
  /// The value of the CASE_ASIN_EFF_DTE attribute.
  /// Eff Date of case assignment.
  /// </summary>
  [JsonPropertyName("caseAsinEffDte")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseAsinEffDte
  {
    get => Get<DateTime?>("caseAsinEffDte");
    set => Set("caseAsinEffDte", value);
  }

  /// <summary>
  /// The value of the CASE_ASIN_END_DTE attribute.
  /// End Date of case assignment.
  /// </summary>
  [JsonPropertyName("caseAsinEndDte")]
  [Member(Index = 33, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseAsinEndDte
  {
    get => Get<DateTime?>("caseAsinEndDte");
    set => Set("caseAsinEndDte", value);
  }

  /// <summary>
  /// The value of the INT_REQUEST_IDENT attribute.
  /// Identifier from Ineterstate Request.
  /// </summary>
  [JsonPropertyName("intRequestIdent")]
  [Member(Index = 34, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? IntRequestIdent
  {
    get => Get<int?>("intRequestIdent");
    set => Set("intRequestIdent", value);
  }

  /// <summary>
  /// The value of the INT_RQST_RQST_DTE attribute.
  /// Request Date from Interstate Request.
  /// </summary>
  [JsonPropertyName("intRqstRqstDte")]
  [Member(Index = 35, Type = MemberType.Date, Optional = true)]
  public DateTime? IntRqstRqstDte
  {
    get => Get<DateTime?>("intRqstRqstDte");
    set => Set("intRqstRqstDte", value);
  }

  /// <summary>Length of the KANSAS_CASE_IND attribute.</summary>
  public const int KansasCaseInd_MaxLength = 1;

  /// <summary>
  /// The value of the KANSAS_CASE_IND attribute.
  /// From Interstate Reqeust. Set to Y-outgoing, N-incoming.
  /// </summary>
  [JsonPropertyName("kansasCaseInd")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = KansasCaseInd_MaxLength, Optional = true)]
  public string KansasCaseInd
  {
    get => Get<string>("kansasCaseInd");
    set => Set(
      "kansasCaseInd", TrimEnd(Substring(value, 1, KansasCaseInd_MaxLength)));
  }

  /// <summary>Length of the PERSON_PROG_CODE attribute.</summary>
  public const int PersonProgCode_MaxLength = 3;

  /// <summary>
  /// The value of the PERSON_PROG_CODE attribute.
  /// Read from Program. Indicates person program.
  /// </summary>
  [JsonPropertyName("personProgCode")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = PersonProgCode_MaxLength, Optional = true)]
  public string PersonProgCode
  {
    get => Get<string>("personProgCode");
    set => Set(
      "personProgCode", TrimEnd(Substring(value, 1, PersonProgCode_MaxLength)));
      
  }

  /// <summary>
  /// The value of the HLTH_INS_COVRG_ID attribute.
  /// Health Insurance Coverage Identifier.
  /// </summary>
  [JsonPropertyName("hlthInsCovrgId")]
  [Member(Index = 38, Type = MemberType.Number, Length = 10, Optional = true)]
  public long? HlthInsCovrgId
  {
    get => Get<long?>("hlthInsCovrgId");
    set => Set("hlthInsCovrgId", value);
  }

  /// <summary>
  /// The value of the GOOD_CAUSE_EFF_DTE attribute.
  /// Effective date of Good Cause. Good Cause is tied to AR case_role.
  /// </summary>
  [JsonPropertyName("goodCauseEffDte")]
  [Member(Index = 39, Type = MemberType.Date, Optional = true)]
  public DateTime? GoodCauseEffDte
  {
    get => Get<DateTime?>("goodCauseEffDte");
    set => Set("goodCauseEffDte", value);
  }

  /// <summary>
  /// The value of the NO_COOP_EFF_DTE attribute.
  /// Eff date of Non Cooperation. Non cooperation is tied to AR case_role.
  /// </summary>
  [JsonPropertyName("noCoopEffDte")]
  [Member(Index = 40, Type = MemberType.Date, Optional = true)]
  public DateTime? NoCoopEffDte
  {
    get => Get<DateTime?>("noCoopEffDte");
    set => Set("noCoopEffDte", value);
  }

  /// <summary>Length of the COMMENT attribute.</summary>
  public const int Comment_MaxLength = 40;

  /// <summary>
  /// The value of the COMMENT attribute.
  /// Miscellaneous field-used as an aid to testing.
  /// </summary>
  [JsonPropertyName("comment")]
  [Member(Index = 41, Type = MemberType.Char, Length = Comment_MaxLength, Optional
    = true)]
  public string Comment
  {
    get => Get<string>("comment");
    set => Set("comment", TrimEnd(Substring(value, 1, Comment_MaxLength)));
  }

  /// <summary>Length of the CHILD_4_DIGIT_SSN attribute.</summary>
  public const int Child4DigitSsn_MaxLength = 4;

  /// <summary>
  /// The value of the CHILD_4_DIGIT_SSN attribute.
  /// The last four digits of the SSN for each child who is included in line 5, 
  /// 6, 9a, and 888 of the OCSE157 report. This is required for federal audit
  /// purposes.
  /// </summary>
  [JsonPropertyName("child4DigitSsn")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = Child4DigitSsn_MaxLength, Optional = true)]
  public string Child4DigitSsn
  {
    get => Get<string>("child4DigitSsn");
    set => Set(
      "child4DigitSsn", TrimEnd(Substring(value, 1, Child4DigitSsn_MaxLength)));
      
  }

  /// <summary>Length of the AP_4_DIGIT_SSN attribute.</summary>
  public const int Ap4DigitSsn_MaxLength = 4;

  /// <summary>
  /// The value of the AP_4_DIGIT_SSN attribute.
  /// The last four digits of the SSN for each absent parent who is included in 
  /// line 888 of the OCSE157 report.  This is required for federal audit
  /// purposes.
  /// </summary>
  [JsonPropertyName("ap4DigitSsn")]
  [Member(Index = 43, Type = MemberType.Char, Length = Ap4DigitSsn_MaxLength, Optional
    = true)]
  public string Ap4DigitSsn
  {
    get => Get<string>("ap4DigitSsn");
    set => Set(
      "ap4DigitSsn", TrimEnd(Substring(value, 1, Ap4DigitSsn_MaxLength)));
  }

  /// <summary>Length of the AR_4_DIGIT_SSN attribute.</summary>
  public const int Ar4DigitSsn_MaxLength = 4;

  /// <summary>
  /// The value of the AR_4_DIGIT_SSN attribute.
  /// The last four digits of the SSN for each applicant recipient who is 
  /// included in line 888 of the OCSE157 report.  This is required for federal
  /// audit purposes.
  /// </summary>
  [JsonPropertyName("ar4DigitSsn")]
  [Member(Index = 44, Type = MemberType.Char, Length = Ar4DigitSsn_MaxLength, Optional
    = true)]
  public string Ar4DigitSsn
  {
    get => Get<string>("ar4DigitSsn");
    set => Set(
      "ar4DigitSsn", TrimEnd(Substring(value, 1, Ar4DigitSsn_MaxLength)));
  }

  /// <summary>Length of the AR_NAME attribute.</summary>
  public const int ArName_MaxLength = 33;

  /// <summary>
  /// The value of the AR_NAME attribute.
  /// This field contains the formatted applicant recipient name.
  /// </summary>
  [JsonPropertyName("arName")]
  [Member(Index = 45, Type = MemberType.Varchar, Length = ArName_MaxLength, Optional
    = true)]
  public string ArName
  {
    get => Get<string>("arName");
    set => Set("arName", Substring(value, 1, ArName_MaxLength));
  }

  /// <summary>Length of the AP_NAME attribute.</summary>
  public const int ApName_MaxLength = 33;

  /// <summary>
  /// The value of the AP_NAME attribute.
  /// This field contains the formatted absent parent name.
  /// </summary>
  [JsonPropertyName("apName")]
  [Member(Index = 46, Type = MemberType.Varchar, Length = ApName_MaxLength, Optional
    = true)]
  public string ApName
  {
    get => Get<string>("apName");
    set => Set("apName", Substring(value, 1, ApName_MaxLength));
  }

  /// <summary>Length of the CHILD_NAME attribute.</summary>
  public const int ChildName_MaxLength = 33;

  /// <summary>
  /// The value of the CHILD_NAME attribute.
  /// This field contains the formatted child name.
  /// </summary>
  [JsonPropertyName("childName")]
  [Member(Index = 47, Type = MemberType.Varchar, Length = ChildName_MaxLength, Optional
    = true)]
  public string ChildName
  {
    get => Get<string>("childName");
    set => Set("childName", Substring(value, 1, ChildName_MaxLength));
  }
}

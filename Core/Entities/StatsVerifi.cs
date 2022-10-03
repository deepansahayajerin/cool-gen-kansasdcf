// The source file: STATS_VERIFI, ID: 373347071, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINANCE
/// 
/// This entity type is used to store verification data for audit
/// purposes.
/// </summary>
[Serializable]
public partial class StatsVerifi: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StatsVerifi()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StatsVerifi(StatsVerifi that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StatsVerifi Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StatsVerifi that)
  {
    base.Assign(that);
    yearMonth = that.yearMonth;
    firstRunNumber = that.firstRunNumber;
    lineNumber = that.lineNumber;
    programType = that.programType;
    createdTimestamp = that.createdTimestamp;
    servicePrvdrId = that.servicePrvdrId;
    officeId = that.officeId;
    caseWrkRole = that.caseWrkRole;
    parentId = that.parentId;
    chiefId = that.chiefId;
    caseNumber = that.caseNumber;
    suppPersonNumber = that.suppPersonNumber;
    obligorPersonNbr = that.obligorPersonNbr;
    datePaternityEst = that.datePaternityEst;
    courtOrderNumber = that.courtOrderNumber;
    tranAmount = that.tranAmount;
    dddd = that.dddd;
    debtDetailBaldue = that.debtDetailBaldue;
    obligationType = that.obligationType;
    collectionAmount = that.collectionAmount;
    collectionDate = that.collectionDate;
    collCreatedDate = that.collCreatedDate;
    caseRoleType = that.caseRoleType;
    caseAsinEffDte = that.caseAsinEffDte;
    caseAsinEndDte = that.caseAsinEndDte;
    personProgCode = that.personProgCode;
    comment = that.comment;
  }

  /// <summary>
  /// The value of the YEAR_MONTH attribute.
  /// Reporting month.
  /// </summary>
  [JsonPropertyName("yearMonth")]
  [Member(Index = 1, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? YearMonth
  {
    get => yearMonth;
    set => yearMonth = value;
  }

  /// <summary>
  /// The value of the FIRST_RUN_NUMBER attribute.
  /// Used as additional qualifier incase report needs to be ran multiple times 
  /// for the same report month. Set to 1 for first run, incremented by 1 for
  /// subsequent runs.
  /// </summary>
  [JsonPropertyName("firstRunNumber")]
  [Member(Index = 2, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? FirstRunNumber
  {
    get => firstRunNumber;
    set => firstRunNumber = value;
  }

  /// <summary>
  /// The value of the LINE_NUMBER attribute.
  /// Line being reported. Eg. 1,24, ..etc.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [Member(Index = 3, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? LineNumber
  {
    get => lineNumber;
    set => lineNumber = value;
  }

  /// <summary>Length of the PROGRAM_TYPE attribute.</summary>
  public const int ProgramType_MaxLength = 4;

  /// <summary>
  /// The value of the PROGRAM_TYPE attribute.
  /// Program category for the report.
  /// </summary>
  [JsonPropertyName("programType")]
  [Member(Index = 4, Type = MemberType.Char, Length = ProgramType_MaxLength, Optional
    = true)]
  public string ProgramType
  {
    get => programType;
    set => programType = value != null
      ? TrimEnd(Substring(value, 1, ProgramType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// created_timestamp-also used as primary identifier.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the SERVICE_PRVDR_ID attribute.
  /// Service Provider Id of case worker.
  /// </summary>
  [JsonPropertyName("servicePrvdrId")]
  [Member(Index = 6, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? ServicePrvdrId
  {
    get => servicePrvdrId;
    set => servicePrvdrId = value;
  }

  /// <summary>
  /// The value of the OFFICE_ID attribute.
  /// office id of case worker.
  /// </summary>
  [JsonPropertyName("officeId")]
  [Member(Index = 7, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OfficeId
  {
    get => officeId;
    set => officeId = value;
  }

  /// <summary>Length of the CASE_WRK_ROLE attribute.</summary>
  public const int CaseWrkRole_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_WRK_ROLE attribute.
  /// Role of case worker. eg.CO-collection officer.
  /// </summary>
  [JsonPropertyName("caseWrkRole")]
  [Member(Index = 8, Type = MemberType.Char, Length = CaseWrkRole_MaxLength, Optional
    = true)]
  public string CaseWrkRole
  {
    get => caseWrkRole;
    set => caseWrkRole = value != null
      ? TrimEnd(Substring(value, 1, CaseWrkRole_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PARENT_ID attribute.
  /// Service Provider Id of supervisor.
  /// </summary>
  [JsonPropertyName("parentId")]
  [Member(Index = 9, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? ParentId
  {
    get => parentId;
    set => parentId = value;
  }

  /// <summary>
  /// The value of the CHIEF_ID attribute.
  /// Service Provider Id of chief.
  /// </summary>
  [JsonPropertyName("chiefId")]
  [Member(Index = 10, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? ChiefId
  {
    get => chiefId;
    set => chiefId = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// KAECSES CSE Case#.Eg.0000000154
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 11, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the SUPP_PERSON_NUMBER attribute.</summary>
  public const int SuppPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the SUPP_PERSON_NUMBER attribute.
  /// Usually the child or AR-.Eg.0000010454
  /// </summary>
  [JsonPropertyName("suppPersonNumber")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = SuppPersonNumber_MaxLength, Optional = true)]
  public string SuppPersonNumber
  {
    get => suppPersonNumber;
    set => suppPersonNumber = value != null
      ? TrimEnd(Substring(value, 1, SuppPersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the OBLIGOR_PERSON_NBR attribute.</summary>
  public const int ObligorPersonNbr_MaxLength = 10;

  /// <summary>
  /// The value of the OBLIGOR_PERSON_NBR attribute.
  /// Usually the AP-.Eg.0000010454
  /// </summary>
  [JsonPropertyName("obligorPersonNbr")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = ObligorPersonNbr_MaxLength, Optional = true)]
  public string ObligorPersonNbr
  {
    get => obligorPersonNbr;
    set => obligorPersonNbr = value != null
      ? TrimEnd(Substring(value, 1, ObligorPersonNbr_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the DATE_PATERNITY_EST attribute.
  /// This field is set to Cse_person.date_paternity_established
  /// </summary>
  [JsonPropertyName("datePaternityEst")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? DatePaternityEst
  {
    get => datePaternityEst;
    set => datePaternityEst = value;
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// This field is set to Legal_Action.standard_number
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => courtOrderNumber;
    set => courtOrderNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TRAN_AMOUNT attribute.
  /// Indicates the amount of Debt or Debt Adjustment.
  /// </summary>
  [JsonPropertyName("tranAmount")]
  [Member(Index = 16, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TranAmount
  {
    get => tranAmount;
    set => tranAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the DDDD attribute.
  /// Debt Detail Due Date.
  /// </summary>
  [JsonPropertyName("dddd")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? Dddd
  {
    get => dddd;
    set => dddd = value;
  }

  /// <summary>
  /// The value of the DEBT_DETAIL_BALDUE attribute.
  /// Debt Detail balance_due
  /// </summary>
  [JsonPropertyName("debtDetailBaldue")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal DebtDetailBaldue
  {
    get => debtDetailBaldue;
    set => debtDetailBaldue = Truncate(value, 2);
  }

  /// <summary>Length of the OBLIGATION_TYPE attribute.</summary>
  public const int ObligationType_MaxLength = 4;

  /// <summary>
  /// The value of the OBLIGATION_TYPE attribute.
  /// Obligation Type Code
  /// </summary>
  [JsonPropertyName("obligationType")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = ObligationType_MaxLength, Optional = true)]
  public string ObligationType
  {
    get => obligationType;
    set => obligationType = value != null
      ? TrimEnd(Substring(value, 1, ObligationType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// Amount field on Collection
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 20, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => collectionAmount;
    set => collectionAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// Collection date from Collection
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 21, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionDate
  {
    get => collectionDate;
    set => collectionDate = value;
  }

  /// <summary>
  /// The value of the COLL_CREATED_DATE attribute.
  /// From created_timestamp on collection. Indicates the date of distribution.
  /// </summary>
  [JsonPropertyName("collCreatedDate")]
  [Member(Index = 22, Type = MemberType.Date, Optional = true)]
  public DateTime? CollCreatedDate
  {
    get => collCreatedDate;
    set => collCreatedDate = value;
  }

  /// <summary>Length of the CASE_ROLE_TYPE attribute.</summary>
  public const int CaseRoleType_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_ROLE_TYPE attribute.
  /// Type From Case Role. AR,AP,CH.
  /// </summary>
  [JsonPropertyName("caseRoleType")]
  [Member(Index = 23, Type = MemberType.Char, Length = CaseRoleType_MaxLength, Optional
    = true)]
  public string CaseRoleType
  {
    get => caseRoleType;
    set => caseRoleType = value != null
      ? TrimEnd(Substring(value, 1, CaseRoleType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CASE_ASIN_EFF_DTE attribute.
  /// Eff Date of case assignment.
  /// </summary>
  [JsonPropertyName("caseAsinEffDte")]
  [Member(Index = 24, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseAsinEffDte
  {
    get => caseAsinEffDte;
    set => caseAsinEffDte = value;
  }

  /// <summary>
  /// The value of the CASE_ASIN_END_DTE attribute.
  /// End Date of case assignment.
  /// </summary>
  [JsonPropertyName("caseAsinEndDte")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseAsinEndDte
  {
    get => caseAsinEndDte;
    set => caseAsinEndDte = value;
  }

  /// <summary>Length of the PERSON_PROG_CODE attribute.</summary>
  public const int PersonProgCode_MaxLength = 3;

  /// <summary>
  /// The value of the PERSON_PROG_CODE attribute.
  /// Read from Program. Indicates person program.
  /// </summary>
  [JsonPropertyName("personProgCode")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = PersonProgCode_MaxLength, Optional = true)]
  public string PersonProgCode
  {
    get => personProgCode;
    set => personProgCode = value != null
      ? TrimEnd(Substring(value, 1, PersonProgCode_MaxLength)) : null;
  }

  /// <summary>Length of the COMMENT attribute.</summary>
  public const int Comment_MaxLength = 40;

  /// <summary>
  /// The value of the COMMENT attribute.
  /// Miscellaneous field-used as an aid to testing.
  /// </summary>
  [JsonPropertyName("comment")]
  [Member(Index = 27, Type = MemberType.Char, Length = Comment_MaxLength, Optional
    = true)]
  public string Comment
  {
    get => comment;
    set => comment = value != null
      ? TrimEnd(Substring(value, 1, Comment_MaxLength)) : null;
  }

  private int? yearMonth;
  private int? firstRunNumber;
  private int? lineNumber;
  private string programType;
  private DateTime? createdTimestamp;
  private int? servicePrvdrId;
  private int? officeId;
  private string caseWrkRole;
  private int? parentId;
  private int? chiefId;
  private string caseNumber;
  private string suppPersonNumber;
  private string obligorPersonNbr;
  private DateTime? datePaternityEst;
  private string courtOrderNumber;
  private decimal? tranAmount;
  private DateTime? dddd;
  private decimal debtDetailBaldue;
  private string obligationType;
  private decimal? collectionAmount;
  private DateTime? collectionDate;
  private DateTime? collCreatedDate;
  private string caseRoleType;
  private DateTime? caseAsinEffDte;
  private DateTime? caseAsinEndDte;
  private string personProgCode;
  private string comment;
}

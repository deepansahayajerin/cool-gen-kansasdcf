// The source file: FA_ARREARS_COLLECTIONS, ID: 1625373388, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:SRVINIT   This is a table to capture the financial impact of adding 
/// adding FA only cases to the system.
/// </summary>
[Serializable]
public partial class FaArrearsCollections: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FaArrearsCollections()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FaArrearsCollections(FaArrearsCollections that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FaArrearsCollections Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FaArrearsCollections that)
  {
    base.Assign(that);
    office = that.office;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    contractor = that.contractor;
    caseNumber = that.caseNumber;
    caseOpenDate = that.caseOpenDate;
    fiscalInd = that.fiscalInd;
    fiscalYear = that.fiscalYear;
    runNumber = that.runNumber;
    childPersonNumber = that.childPersonNumber;
    fsStartDate = that.fsStartDate;
    standardNumber = that.standardNumber;
    courtOrderEstBy = that.courtOrderEstBy;
    arrearsAmountDue = that.arrearsAmountDue;
    totalCollectionsAmount = that.totalCollectionsAmount;
    caseworkerLastName = that.caseworkerLastName;
    caseworkerFirstName = that.caseworkerFirstName;
    caseClosedDate = that.caseClosedDate;
    caseClosureReason = that.caseClosureReason;
    nonCoopCd = that.nonCoopCd;
    currentNonCoopCd = that.currentNonCoopCd;
  }

  /// <summary>Length of the OFFICE attribute.</summary>
  public const int Office_MaxLength = 20;

  /// <summary>
  /// The value of the OFFICE attribute.
  /// The name of the office that the case is assigned to.	
  /// </summary>
  [JsonPropertyName("office")]
  [Member(Index = 1, Type = MemberType.Varchar, Length = Office_MaxLength, Optional
    = true)]
  public string Office
  {
    get => office;
    set => office = value != null ? Substring(value, 1, Office_MaxLength) : null
      ;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Who created the record.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 2, Type = MemberType.Varchar, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? Substring(value, 1, CreatedBy_MaxLength) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// This is the date and time when this record was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 3, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the CONTRACTOR attribute.</summary>
  public const int Contractor_MaxLength = 20;

  /// <summary>
  /// The value of the CONTRACTOR attribute.
  /// The name of the contractor
  /// </summary>
  [JsonPropertyName("contractor")]
  [Member(Index = 4, Type = MemberType.Varchar, Length = Contractor_MaxLength, Optional
    = true)]
  public string Contractor
  {
    get => contractor;
    set => contractor = value != null
      ? Substring(value, 1, Contractor_MaxLength) : null;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// The case number of the child receiving FA
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
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

  /// <summary>
  /// The value of the CASE_OPEN_DATE attribute.
  /// The date the case was open
  /// </summary>
  [JsonPropertyName("caseOpenDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? CaseOpenDate
  {
    get => caseOpenDate;
    set => caseOpenDate = value;
  }

  /// <summary>Length of the FISCAL_IND attribute.</summary>
  public const int FiscalInd_MaxLength = 1;

  /// <summary>
  /// The value of the FISCAL_IND attribute.
  /// Indicator stating which fiscal year this period will be. State or Federal
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FiscalInd_MaxLength)]
  public string FiscalInd
  {
    get => fiscalInd ?? "";
    set => fiscalInd = TrimEnd(Substring(value, 1, FiscalInd_MaxLength));
  }

  /// <summary>
  /// The json value of the FiscalInd attribute.</summary>
  [JsonPropertyName("fiscalInd")]
  [Computed]
  public string FiscalInd_Json
  {
    get => NullIf(FiscalInd, "");
    set => FiscalInd = value;
  }

  /// <summary>Length of the FISCAL_YEAR attribute.</summary>
  public const int FiscalYear_MaxLength = 4;

  /// <summary>
  /// The value of the FISCAL_YEAR attribute.
  /// The fiscal year that the case was counted in
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = FiscalYear_MaxLength)]
  public string FiscalYear
  {
    get => fiscalYear ?? "";
    set => fiscalYear = TrimEnd(Substring(value, 1, FiscalYear_MaxLength));
  }

  /// <summary>
  /// The json value of the FiscalYear attribute.</summary>
  [JsonPropertyName("fiscalYear")]
  [Computed]
  public string FiscalYear_Json
  {
    get => NullIf(FiscalYear, "");
    set => FiscalYear = value;
  }

  /// <summary>
  /// The value of the RUN_NUMBER attribute.
  /// The number of times the program has been run for the same time period
  /// </summary>
  [JsonPropertyName("runNumber")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int RunNumber
  {
    get => runNumber;
    set => runNumber = value;
  }

  /// <summary>Length of the CHILD_PERSON_NUMBER attribute.</summary>
  public const int ChildPersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CHILD_PERSON_NUMBER attribute.
  /// The person number for the child on FA
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ChildPersonNumber_MaxLength)]
  public string ChildPersonNumber
  {
    get => childPersonNumber ?? "";
    set => childPersonNumber =
      TrimEnd(Substring(value, 1, ChildPersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildPersonNumber attribute.</summary>
  [JsonPropertyName("childPersonNumber")]
  [Computed]
  public string ChildPersonNumber_Json
  {
    get => NullIf(ChildPersonNumber, "");
    set => ChildPersonNumber = value;
  }

  /// <summary>
  /// The value of the FS_START_DATE attribute.
  /// The date the food stamps program started
  /// </summary>
  [JsonPropertyName("fsStartDate")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? FsStartDate
  {
    get => fsStartDate;
    set => fsStartDate = value;
  }

  /// <summary>Length of the STANDARD_NUMBER attribute.</summary>
  public const int StandardNumber_MaxLength = 20;

  /// <summary>
  /// The value of the STANDARD_NUMBER attribute.
  /// The standard number on the legal action that was established
  /// </summary>
  [JsonPropertyName("standardNumber")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = StandardNumber_MaxLength, Optional = true)]
  public string StandardNumber
  {
    get => standardNumber;
    set => standardNumber = value != null
      ? TrimEnd(Substring(value, 1, StandardNumber_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_ORDER_EST_BY attribute.</summary>
  public const int CourtOrderEstBy_MaxLength = 2;

  /// <summary>
  /// The value of the COURT_ORDER_EST_BY attribute.
  /// How the court order was established (CT, CS, PR or OS).
  /// </summary>
  [JsonPropertyName("courtOrderEstBy")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = CourtOrderEstBy_MaxLength, Optional = true)]
  public string CourtOrderEstBy
  {
    get => courtOrderEstBy;
    set => courtOrderEstBy = value != null
      ? TrimEnd(Substring(value, 1, CourtOrderEstBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ARREARS_AMOUNT_DUE attribute.
  /// The arrears amount due at the time of the case opening.
  /// </summary>
  [JsonPropertyName("arrearsAmountDue")]
  [Member(Index = 14, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? ArrearsAmountDue
  {
    get => arrearsAmountDue;
    set => arrearsAmountDue = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the TOTAL_COLLECTIONS_AMOUNT attribute.
  /// The total amount collected from the arrears amount that were due when the 
  /// case open
  /// </summary>
  [JsonPropertyName("totalCollectionsAmount")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? TotalCollectionsAmount
  {
    get => totalCollectionsAmount;
    set => totalCollectionsAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CASEWORKER_LAST_NAME attribute.</summary>
  public const int CaseworkerLastName_MaxLength = 17;

  /// <summary>
  /// The value of the CASEWORKER_LAST_NAME attribute.
  /// The last name of the caseworker when case was open
  /// </summary>
  [JsonPropertyName("caseworkerLastName")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = CaseworkerLastName_MaxLength, Optional = true)]
  public string CaseworkerLastName
  {
    get => caseworkerLastName;
    set => caseworkerLastName = value != null
      ? TrimEnd(Substring(value, 1, CaseworkerLastName_MaxLength)) : null;
  }

  /// <summary>Length of the CASEWORKER_FIRST_NAME attribute.</summary>
  public const int CaseworkerFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the CASEWORKER_FIRST_NAME attribute.
  /// The first name of the caseworker when case was open
  /// </summary>
  [JsonPropertyName("caseworkerFirstName")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = CaseworkerFirstName_MaxLength, Optional = true)]
  public string CaseworkerFirstName
  {
    get => caseworkerFirstName;
    set => caseworkerFirstName = value != null
      ? TrimEnd(Substring(value, 1, CaseworkerFirstName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CASE_CLOSED_DATE attribute.
  /// The date the case was closed
  /// </summary>
  [JsonPropertyName("caseClosedDate")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseClosedDate
  {
    get => caseClosedDate;
    set => caseClosedDate = value;
  }

  /// <summary>Length of the CASE_CLOSURE_REASON attribute.</summary>
  public const int CaseClosureReason_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_CLOSURE_REASON attribute.
  /// The reason the case was closed
  /// </summary>
  [JsonPropertyName("caseClosureReason")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = CaseClosureReason_MaxLength, Optional = true)]
  public string CaseClosureReason
  {
    get => caseClosureReason;
    set => caseClosureReason = value != null
      ? TrimEnd(Substring(value, 1, CaseClosureReason_MaxLength)) : null;
  }

  /// <summary>Length of the NON_COOP_CD attribute.</summary>
  public const int NonCoopCd_MaxLength = 1;

  /// <summary>
  /// The value of the NON_COOP_CD attribute.
  /// The code if the CP has an open com-coop
  /// </summary>
  [JsonPropertyName("nonCoopCd")]
  [Member(Index = 20, Type = MemberType.Char, Length = NonCoopCd_MaxLength, Optional
    = true)]
  public string NonCoopCd
  {
    get => nonCoopCd;
    set => nonCoopCd = value != null
      ? TrimEnd(Substring(value, 1, NonCoopCd_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_NON_COOP_CD attribute.</summary>
  public const int CurrentNonCoopCd_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_NON_COOP_CD attribute.
  /// The code if the CP has an open com-coop at the end of the reporting 
  /// period.	
  /// </summary>
  [JsonPropertyName("currentNonCoopCd")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = CurrentNonCoopCd_MaxLength, Optional = true)]
  public string CurrentNonCoopCd
  {
    get => currentNonCoopCd;
    set => currentNonCoopCd = value != null
      ? TrimEnd(Substring(value, 1, CurrentNonCoopCd_MaxLength)) : null;
  }

  private string office;
  private string createdBy;
  private DateTime? createdTstamp;
  private string contractor;
  private string caseNumber;
  private DateTime? caseOpenDate;
  private string fiscalInd;
  private string fiscalYear;
  private int runNumber;
  private string childPersonNumber;
  private DateTime? fsStartDate;
  private string standardNumber;
  private string courtOrderEstBy;
  private decimal? arrearsAmountDue;
  private decimal? totalCollectionsAmount;
  private string caseworkerLastName;
  private string caseworkerFirstName;
  private DateTime? caseClosedDate;
  private string caseClosureReason;
  private string nonCoopCd;
  private string currentNonCoopCd;
}

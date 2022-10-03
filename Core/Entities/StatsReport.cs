// The source file: STATS_REPORT, ID: 373347036, model: 746.
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINANCE
/// 
/// This entity type is used to store data for the statistical report. A cool:
/// gen program will populate this DB2 table. A Natural program will then read
/// these numbers to create the report.
/// </summary>
[Serializable]
public partial class StatsReport: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public StatsReport()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public StatsReport(StatsReport that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new StatsReport Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(StatsReport that)
  {
    base.Assign(that);
    yearMonth = that.yearMonth;
    firstRunNumber = that.firstRunNumber;
    lineNumber = that.lineNumber;
    createdTimestamp = that.createdTimestamp;
    servicePrvdrId = that.servicePrvdrId;
    officeId = that.officeId;
    caseWrkRole = that.caseWrkRole;
    caseEffDate = that.caseEffDate;
    parentId = that.parentId;
    chiefId = that.chiefId;
    column1 = that.column1;
    column2 = that.column2;
    column3 = that.column3;
    column4 = that.column4;
    column5 = that.column5;
    column6 = that.column6;
    column7 = that.column7;
    column8 = that.column8;
    column9 = that.column9;
    column10 = that.column10;
    column11 = that.column11;
    column12 = that.column12;
    column13 = that.column13;
    column14 = that.column14;
    column15 = that.column15;
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
  /// Line being reported. Eg. 1, 24, ..etc.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [Member(Index = 3, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? LineNumber
  {
    get => lineNumber;
    set => lineNumber = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Created_timestamp-also used as primary identifier.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
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
  [Member(Index = 5, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? ServicePrvdrId
  {
    get => servicePrvdrId;
    set => servicePrvdrId = value;
  }

  /// <summary>
  /// The value of the OFFICE_ID attribute.
  /// Office id of case worker.
  /// </summary>
  [JsonPropertyName("officeId")]
  [Member(Index = 6, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OfficeId
  {
    get => officeId;
    set => officeId = value;
  }

  /// <summary>Length of the CASE_WRK_ROLE attribute.</summary>
  public const int CaseWrkRole_MaxLength = 2;

  /// <summary>
  /// The value of the CASE_WRK_ROLE attribute.
  /// Role of case worker. eg. CO- collection officer.
  /// </summary>
  [JsonPropertyName("caseWrkRole")]
  [Member(Index = 7, Type = MemberType.Char, Length = CaseWrkRole_MaxLength, Optional
    = true)]
  public string CaseWrkRole
  {
    get => caseWrkRole;
    set => caseWrkRole = value != null
      ? TrimEnd(Substring(value, 1, CaseWrkRole_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CASE_EFF_DATE attribute.
  /// Eff. date of case worker's role.
  /// </summary>
  [JsonPropertyName("caseEffDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? CaseEffDate
  {
    get => caseEffDate;
    set => caseEffDate = value;
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

  /// <summary>
  /// The value of the COLUMN1 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column1")]
  [Member(Index = 11, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column1
  {
    get => column1;
    set => column1 = value;
  }

  /// <summary>
  /// The value of the COLUMN2 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column2")]
  [Member(Index = 12, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column2
  {
    get => column2;
    set => column2 = value;
  }

  /// <summary>
  /// The value of the COLUMN3 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column3")]
  [Member(Index = 13, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column3
  {
    get => column3;
    set => column3 = value;
  }

  /// <summary>
  /// The value of the COLUMN4 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column4")]
  [Member(Index = 14, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column4
  {
    get => column4;
    set => column4 = value;
  }

  /// <summary>
  /// The value of the COLUMN5 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrers.
  /// </summary>
  [JsonPropertyName("column5")]
  [Member(Index = 15, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column5
  {
    get => column5;
    set => column5 = value;
  }

  /// <summary>
  /// The value of the COLUMN6 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column6")]
  [Member(Index = 16, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column6
  {
    get => column6;
    set => column6 = value;
  }

  /// <summary>
  /// The value of the COLUMN7 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column7")]
  [Member(Index = 17, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column7
  {
    get => column7;
    set => column7 = value;
  }

  /// <summary>
  /// The value of the COLUMN8 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column8")]
  [Member(Index = 18, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column8
  {
    get => column8;
    set => column8 = value;
  }

  /// <summary>
  /// The value of the COLUMN9 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column9")]
  [Member(Index = 19, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column9
  {
    get => column9;
    set => column9 = value;
  }

  /// <summary>
  /// The value of the COLUMN10 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column10")]
  [Member(Index = 20, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column10
  {
    get => column10;
    set => column10 = value;
  }

  /// <summary>
  /// The value of the COLUMN11 attribute.
  /// Number being reported for this line. It could be the number of cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column11")]
  [Member(Index = 21, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column11
  {
    get => column11;
    set => column11 = value;
  }

  /// <summary>
  /// The value of the COLUMN12 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column12")]
  [Member(Index = 22, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column12
  {
    get => column12;
    set => column12 = value;
  }

  /// <summary>
  /// The value of the COLUMN13 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column13")]
  [Member(Index = 23, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column13
  {
    get => column13;
    set => column13 = value;
  }

  /// <summary>
  /// The value of the COLUMN14 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column14")]
  [Member(Index = 24, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column14
  {
    get => column14;
    set => column14 = value;
  }

  /// <summary>
  /// The value of the COLUMN15 attribute.
  /// Number being reported for this line. It could be the number of Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("column15")]
  [Member(Index = 25, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Column15
  {
    get => column15;
    set => column15 = value;
  }

  private int? yearMonth;
  private int? firstRunNumber;
  private int? lineNumber;
  private DateTime? createdTimestamp;
  private int? servicePrvdrId;
  private int? officeId;
  private string caseWrkRole;
  private DateTime? caseEffDate;
  private int? parentId;
  private int? chiefId;
  private long? column1;
  private long? column2;
  private long? column3;
  private long? column4;
  private long? column5;
  private long? column6;
  private long? column7;
  private long? column8;
  private long? column9;
  private long? column10;
  private long? column11;
  private long? column12;
  private long? column13;
  private long? column14;
  private long? column15;
}

// The source file: ACTIVITY_STATEMENT, ID: 371430138, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// SCOPE:	A monthly report of an Obligors activity or outstanding debts.
/// Qualification: That reflects the prior period's activity, an itemization of 
/// the amount now due, adjustments, and payments received.
/// An obligor will receive one activity statement per month summarizing all 
/// obligations owed by the CSE person.  Accompanying an activity statement may
/// be one or more remittance coupons.
/// NOTE: Many of the attributes of ACTIVITY STATEMENT can be obtained through a
/// relationship to other entity types.  We have attributes for them for
/// historical purposes since an activity statement is a snapshot in time.
/// </summary>
[Serializable]
public partial class ActivityStatement: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ActivityStatement()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ActivityStatement(ActivityStatement that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ActivityStatement Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ActivityStatement that)
  {
    base.Assign(that);
    reportingYear = that.reportingYear;
    reportingMonth = that.reportingMonth;
    asOfDate = that.asOfDate;
    obligorName = that.obligorName;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    gsmEffectiveMont = that.gsmEffectiveMont;
    gsmEffectiveYear = that.gsmEffectiveYear;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
    otrType = that.otrType;
    otrGeneratedId = that.otrGeneratedId;
    obgGeneratedId = that.obgGeneratedId;
    otyType = that.otyType;
    cpaRType = that.cpaRType;
    cspRNumber = that.cspRNumber;
  }

  /// <summary>
  /// The value of the REPORTING_YEAR attribute.
  /// The year that the activity statement is reporting activity for.
  /// </summary>
  [JsonPropertyName("reportingYear")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int ReportingYear
  {
    get => reportingYear;
    set => reportingYear = value;
  }

  /// <summary>
  /// The value of the REPORTING_MONTH attribute.
  /// The month that the activity statement is reporting activity for.
  /// </summary>
  [JsonPropertyName("reportingMonth")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int ReportingMonth
  {
    get => reportingMonth;
    set => reportingMonth = value;
  }

  /// <summary>
  /// The value of the AS_OF_DATE attribute.
  /// The date through which posted activity will be included.
  /// Example:
  /// If the as-of-date is 1/20/95 then any activity processed through the 
  /// system with a date less than or equal to this date and since the last
  /// statement date will be included on this statement.
  /// </summary>
  [JsonPropertyName("asOfDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? AsOfDate
  {
    get => asOfDate;
    set => asOfDate = value;
  }

  /// <summary>Length of the OBLIGOR_NAME attribute.</summary>
  public const int ObligorName_MaxLength = 33;

  /// <summary>
  /// The value of the OBLIGOR_NAME attribute.
  /// The full name of the obligor to whom the statement is being sent.  It is a
  /// derived attribute from the obligor first, middle, and last names from the
  /// CSE person entity type.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = ObligorName_MaxLength)]
  public string ObligorName
  {
    get => obligorName ?? "";
    set => obligorName = TrimEnd(Substring(value, 1, ObligorName_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligorName attribute.</summary>
  [JsonPropertyName("obligorName")]
  [Computed]
  public string ObligorName_Json
  {
    get => NullIf(ObligorName, "");
    set => ObligorName = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TMST attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_MONTH attribute.
  /// The month in which the message is to be communicated on the statements.
  /// </summary>
  [JsonPropertyName("gsmEffectiveMont")]
  [Member(Index = 7, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? GsmEffectiveMont
  {
    get => gsmEffectiveMont;
    set => gsmEffectiveMont = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_YEAR attribute.
  /// The year in which the message is to be communicated on the statement
  /// </summary>
  [JsonPropertyName("gsmEffectiveYear")]
  [Member(Index = 8, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? GsmEffectiveYear
  {
    get => gsmEffectiveYear;
    set => gsmEffectiveYear = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CpaType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaType
  {
    get => cpaType ?? "";
    set => cpaType = TrimEnd(Substring(value, 1, CpaType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaType attribute.</summary>
  [JsonPropertyName("cpaType")]
  [Computed]
  public string CpaType_Json
  {
    get => NullIf(CpaType, "");
    set => CpaType = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int OtrType_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the obligation transaction type.
  /// Permitted Values:
  /// 	- DE = DEBT
  /// 	- DA = DEBT ADJUSTMENT
  /// </summary>
  [JsonPropertyName("otrType")]
  [Member(Index = 11, Type = MemberType.Char, Length = OtrType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType;
    set => otrType = value != null
      ? TrimEnd(Substring(value, 1, OtrType_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrGeneratedId")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? OtrGeneratedId
  {
    get => otrGeneratedId;
    set => otrGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyType")]
  [Member(Index = 14, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? OtyType
  {
    get => otyType;
    set => otyType = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaRType_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonPropertyName("cpaRType")]
  [Member(Index = 15, Type = MemberType.Char, Length = CpaRType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaRType
  {
    get => cpaRType;
    set => cpaRType = value != null
      ? TrimEnd(Substring(value, 1, CpaRType_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspRNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspRNumber")]
  [Member(Index = 16, Type = MemberType.Char, Length = CspRNumber_MaxLength, Optional
    = true)]
  public string CspRNumber
  {
    get => cspRNumber;
    set => cspRNumber = value != null
      ? TrimEnd(Substring(value, 1, CspRNumber_MaxLength)) : null;
  }

  private int reportingYear;
  private int reportingMonth;
  private DateTime? asOfDate;
  private string obligorName;
  private string createdBy;
  private DateTime? createdTmst;
  private int? gsmEffectiveMont;
  private int? gsmEffectiveYear;
  private string cspNumber;
  private string cpaType;
  private string otrType;
  private int? otrGeneratedId;
  private int? obgGeneratedId;
  private int? otyType;
  private string cpaRType;
  private string cspRNumber;
}

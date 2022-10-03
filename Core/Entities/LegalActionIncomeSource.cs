// The source file: LEGAL_ACTION_INCOME_SOURCE, ID: 371436829, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// Legal Action Employer is the Employer that is served with an Income 
/// Withholding Order or a Garnishment. This resolves a many to many between
/// Legal Action and Employer.
/// </summary>
[Serializable]
public partial class LegalActionIncomeSource: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionIncomeSource()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionIncomeSource(LegalActionIncomeSource that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionIncomeSource Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionIncomeSource that)
  {
    base.Assign(that);
    wageOrNonWage = that.wageOrNonWage;
    orderType = that.orderType;
    withholdingType = that.withholdingType;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    identifier = that.identifier;
    cspNumber = that.cspNumber;
    isrIdentifier = that.isrIdentifier;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>Length of the WAGE_OR_NON_WAGE attribute.</summary>
  public const int WageOrNonWage_MaxLength = 1;

  /// <summary>
  /// The value of the WAGE_OR_NON_WAGE attribute.
  /// This attribute specifies whether the garnishment is of wages or non-wages.
  /// &quot;W&quot; Garnishment of wages
  /// &quot;N&quot; Garnishment of non-wages.
  /// </summary>
  [JsonPropertyName("wageOrNonWage")]
  [Member(Index = 1, Type = MemberType.Char, Length = WageOrNonWage_MaxLength, Optional
    = true)]
  public string WageOrNonWage
  {
    get => wageOrNonWage;
    set => wageOrNonWage = value != null
      ? TrimEnd(Substring(value, 1, WageOrNonWage_MaxLength)) : null;
  }

  /// <summary>Length of the ORDER_TYPE attribute.</summary>
  public const int OrderType_MaxLength = 2;

  /// <summary>
  /// The value of the ORDER_TYPE attribute.
  /// This attribute specifies the thpe of order for withholding. e.g. Initial 
  /// IWO, Modified IWO, IWO termination. The valid values are maintained in
  /// CODE_VALUE entity for LAIS ORDER TYPE.
  /// </summary>
  [JsonPropertyName("orderType")]
  [Member(Index = 2, Type = MemberType.Char, Length = OrderType_MaxLength, Optional
    = true)]
  public string OrderType
  {
    get => orderType;
    set => orderType = value != null
      ? TrimEnd(Substring(value, 1, OrderType_MaxLength)) : null;
  }

  /// <summary>Length of the WITHHOLDING_TYPE attribute.</summary>
  public const int WithholdingType_MaxLength = 4;

  /// <summary>
  /// The value of the WITHHOLDING_TYPE attribute.
  /// This attribute specifies the thpe of withholding. e.g. IWO, IWO for 
  /// medical, Withholding from Worker's Compensation, Garnishment, etc. The
  /// valid values are maintained in the CODE_VALUE entiy for LAIS WITHHOLDING
  /// TYPE.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = WithholdingType_MaxLength)]
    
  public string WithholdingType
  {
    get => withholdingType ?? "";
    set => withholdingType =
      TrimEnd(Substring(value, 1, WithholdingType_MaxLength));
  }

  /// <summary>
  /// The json value of the WithholdingType attribute.</summary>
  [JsonPropertyName("withholdingType")]
  [Computed]
  public string WithholdingType_Json
  {
    get => NullIf(WithholdingType, "");
    set => WithholdingType = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the Income Withholding Order and/or Garnishment is set to begin.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date the Income Withholding Order and/or Garnishment is to end.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute together with the relationships with INCOME SOURCE and 
  /// LEGAL ACTION uniquely identifies an occurrence of this entity type.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("isrIdentifier")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? IsrIdentifier
  {
    get => isrIdentifier;
    set => isrIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private string wageOrNonWage;
  private string orderType;
  private string withholdingType;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private string createdBy;
  private DateTime? createdTstamp;
  private int identifier;
  private string cspNumber;
  private DateTime? isrIdentifier;
  private int lgaIdentifier;
}

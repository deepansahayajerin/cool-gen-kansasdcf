// The source file: LEGAL_ACTION_PERSON, ID: 371436857, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// Identifies the CSE Person as the Petitioner or Respondent.
/// </summary>
[Serializable]
public partial class LegalActionPerson: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LegalActionPerson()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LegalActionPerson(LegalActionPerson that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LegalActionPerson Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LegalActionPerson that)
  {
    base.Assign(that);
    identifier = that.identifier;
    accountType = that.accountType;
    role = that.role;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    endReason = that.endReason;
    createdTstamp = that.createdTstamp;
    createdBy = that.createdBy;
    arrearsAmount = that.arrearsAmount;
    currentAmount = that.currentAmount;
    judgementAmount = that.judgementAmount;
    cspNumber = that.cspNumber;
    lgaIdentifier = that.lgaIdentifier;
    ladRNumber = that.ladRNumber;
    lgaRIdentifier = that.lgaRIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the ACCOUNT_TYPE attribute.</summary>
  public const int AccountType_MaxLength = 1;

  /// <summary>
  /// The value of the ACCOUNT_TYPE attribute.
  /// This attribute indicates whether this CSE Person is an Obligee, Obligor, 
  /// or Supported Person.
  /// EX:   E - Obligee
  ///       R - Obligor
  ///       S - Supported Person
  /// </summary>
  [JsonPropertyName("accountType")]
  [Member(Index = 2, Type = MemberType.Char, Length = AccountType_MaxLength, Optional
    = true)]
  public string AccountType
  {
    get => accountType;
    set => accountType = value != null
      ? TrimEnd(Substring(value, 1, AccountType_MaxLength)) : null;
  }

  /// <summary>Length of the ROLE attribute.</summary>
  public const int Role_MaxLength = 1;

  /// <summary>
  /// The value of the ROLE attribute.
  /// The role identifies the CSE Person as the Petitioner, Respondent or Child 
  /// on the Legal Action.
  /// Valid codes are:
  /// 	C - Child
  /// 	P - Petitioner
  /// 	R - Respondent
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Role_MaxLength)]
  public string Role
  {
    get => role ?? "";
    set => role = TrimEnd(Substring(value, 1, Role_MaxLength));
  }

  /// <summary>
  /// The json value of the Role attribute.</summary>
  [JsonPropertyName("role")]
  [Computed]
  public string Role_Json
  {
    get => NullIf(Role, "");
    set => Role = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The beginning date the CSE person is associated to a particular legal 
  /// action.
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
  /// The  date the CSE person is no longer associated to a court case.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the END_REASON attribute.</summary>
  public const int EndReason_MaxLength = 2;

  /// <summary>
  /// The value of the END_REASON attribute.
  /// This is the coded reason that the CSE Person was removed from a Legal 
  /// Action.
  /// Valid codes are:
  /// 	?? -
  /// </summary>
  [JsonPropertyName("endReason")]
  [Member(Index = 6, Type = MemberType.Char, Length = EndReason_MaxLength, Optional
    = true)]
  public string EndReason
  {
    get => endReason;
    set => endReason = value != null
      ? TrimEnd(Substring(value, 1, EndReason_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date the CSE Person is named on the legal action as the Petitioner or 
  /// Respondent.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrence of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the ARREARS AMOUNT attribute.
  /// This indicates the amount that is to be applied to arrears per interval 
  /// for the person. This field is applicable only for legal obligation person
  /// with ACCOUNT_TYPE = &quot;S&quot;.
  /// </summary>
  [JsonPropertyName("arrearsAmount")]
  [Member(Index = 9, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? ArrearsAmount
  {
    get => arrearsAmount;
    set => arrearsAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT AMOUNT attribute.
  /// This indicates the amount that is owed.  This amount is per interval for 
  /// the person. This field is applicable only for legal obligation person with
  /// ACCOUNT_TYPE = &quot;S&quot;.
  /// </summary>
  [JsonPropertyName("currentAmount")]
  [Member(Index = 10, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? CurrentAmount
  {
    get => currentAmount;
    set => currentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the JUDGEMENT AMOUNT attribute.
  /// The total amount to be paid for a particular detail type for the person. 
  /// This field is applicable only for legal obligation person with
  /// ACCOUNT_TYPE = &quot;S&quot;.
  /// </summary>
  [JsonPropertyName("judgementAmount")]
  [Member(Index = 11, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? JudgementAmount
  {
    get => judgementAmount;
    set => judgementAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspNumber")]
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength, Optional
    = true)]
  public string CspNumber
  {
    get => cspNumber;
    set => cspNumber = value != null
      ? TrimEnd(Substring(value, 1, CspNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A number, unique within the legal action, used to identify each detail of 
  /// the Legal Action.  Starts with one and moves sequentially.
  /// </summary>
  [JsonPropertyName("ladRNumber")]
  [Member(Index = 14, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? LadRNumber
  {
    get => ladRNumber;
    set => ladRNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaRIdentifier")]
  [Member(Index = 15, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaRIdentifier
  {
    get => lgaRIdentifier;
    set => lgaRIdentifier = value;
  }

  private int identifier;
  private string accountType;
  private string role;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private string endReason;
  private DateTime? createdTstamp;
  private string createdBy;
  private decimal? arrearsAmount;
  private decimal? currentAmount;
  private decimal? judgementAmount;
  private string cspNumber;
  private int? lgaIdentifier;
  private int? ladRNumber;
  private int? lgaRIdentifier;
}

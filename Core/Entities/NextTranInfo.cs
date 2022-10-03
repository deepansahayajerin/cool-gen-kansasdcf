// The source file: NEXT_TRAN_INFO, ID: 371422398, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// this will contain all the information necessary for passing data from screen
/// to screen around kessep.
/// </summary>
[Serializable]
public partial class NextTranInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NextTranInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NextTranInfo(NextTranInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NextTranInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NextTranInfo that)
  {
    base.Assign(that);
    lastTran = that.lastTran;
    legalActionIdentifier = that.legalActionIdentifier;
    courtCaseNumber = that.courtCaseNumber;
    caseNumber = that.caseNumber;
    csePersonNumber = that.csePersonNumber;
    csePersonNumberAp = that.csePersonNumberAp;
    csePersonNumberObligee = that.csePersonNumberObligee;
    csePersonNumberObligor = that.csePersonNumberObligor;
    courtOrderNumber = that.courtOrderNumber;
    obligationId = that.obligationId;
    standardCrtOrdNumber = that.standardCrtOrdNumber;
    infrastructureId = that.infrastructureId;
    miscText1 = that.miscText1;
    miscText2 = that.miscText2;
    miscNum1 = that.miscNum1;
    miscNum2 = that.miscNum2;
    miscNum1V2 = that.miscNum1V2;
    miscNum2V2 = that.miscNum2V2;
    ospId = that.ospId;
  }

  /// <summary>Length of the LAST_TRAN attribute.</summary>
  public const int LastTran_MaxLength = 4;

  /// <summary>
  /// The value of the LAST_TRAN attribute.
  /// The transaction where the user came from on a next tran action.
  /// </summary>
  [JsonPropertyName("lastTran")]
  [Member(Index = 1, Type = MemberType.Char, Length = LastTran_MaxLength, Optional
    = true)]
  public string LastTran
  {
    get => lastTran;
    set => lastTran = value != null
      ? TrimEnd(Substring(value, 1, LastTran_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LEGAL_ACTION_IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("legalActionIdentifier")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LegalActionIdentifier
  {
    get => legalActionIdentifier;
    set => legalActionIdentifier = value;
  }

  /// <summary>Length of the COURT_CASE_NUMBER attribute.</summary>
  public const int CourtCaseNumber_MaxLength = 17;

  /// <summary>
  /// The value of the COURT_CASE_NUMBER attribute.
  /// Identifying number assigned by the tribunal.
  /// </summary>
  [JsonPropertyName("courtCaseNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = CourtCaseNumber_MaxLength, Optional = true)]
  public string CourtCaseNumber
  {
    get => courtCaseNumber;
    set => courtCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtCaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonPropertyName("caseNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength, Optional
    = true)]
  public string CaseNumber
  {
    get => caseNumber;
    set => caseNumber = value != null
      ? TrimEnd(Substring(value, 1, CaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => csePersonNumber;
    set => csePersonNumber = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER_AP attribute.</summary>
  public const int CsePersonNumberAp_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER_AP attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("csePersonNumberAp")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = CsePersonNumberAp_MaxLength, Optional = true)]
  public string CsePersonNumberAp
  {
    get => csePersonNumberAp;
    set => csePersonNumberAp = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumberAp_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER_OBLIGEE attribute.</summary>
  public const int CsePersonNumberObligee_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER_OBLIGEE attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("csePersonNumberObligee")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CsePersonNumberObligee_MaxLength, Optional = true)]
  public string CsePersonNumberObligee
  {
    get => csePersonNumberObligee;
    set => csePersonNumberObligee = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumberObligee_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER_OBLIGOR attribute.</summary>
  public const int CsePersonNumberObligor_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER_OBLIGOR attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("csePersonNumberObligor")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = CsePersonNumberObligor_MaxLength, Optional = true)]
  public string CsePersonNumberObligor
  {
    get => csePersonNumberObligor;
    set => csePersonNumberObligor = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumberObligor_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// The unique identifier assigned to the court order by the court.
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => courtOrderNumber;
    set => courtOrderNumber = value != null
      ? TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the OBLIGATION_ID attribute.
  /// Obligation identifier to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("obligationId")]
  [Member(Index = 10, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? ObligationId
  {
    get => obligationId;
    set => obligationId = value;
  }

  /// <summary>Length of the STANDARD_CRT_ORD_NUMBER attribute.</summary>
  public const int StandardCrtOrdNumber_MaxLength = 20;

  /// <summary>
  /// The value of the STANDARD_CRT_ORD_NUMBER attribute.
  /// Standard court order number to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("standardCrtOrdNumber")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = StandardCrtOrdNumber_MaxLength, Optional = true)]
  public string StandardCrtOrdNumber
  {
    get => standardCrtOrdNumber;
    set => standardCrtOrdNumber = value != null
      ? TrimEnd(Substring(value, 1, StandardCrtOrdNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the INFRASTRUCTURE_ID attribute.
  /// Infrastructure identifier to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("infrastructureId")]
  [Member(Index = 12, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? InfrastructureId
  {
    get => infrastructureId;
    set => infrastructureId = value;
  }

  /// <summary>Length of the MISC_TEXT_1 attribute.</summary>
  public const int MiscText1_MaxLength = 50;

  /// <summary>
  /// The value of the MISC_TEXT_1 attribute.
  /// Miscellaneous text 1 to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("miscText1")]
  [Member(Index = 13, Type = MemberType.Char, Length = MiscText1_MaxLength, Optional
    = true)]
  public string MiscText1
  {
    get => miscText1;
    set => miscText1 = value != null
      ? TrimEnd(Substring(value, 1, MiscText1_MaxLength)) : null;
  }

  /// <summary>Length of the MISC_TEXT_2 attribute.</summary>
  public const int MiscText2_MaxLength = 50;

  /// <summary>
  /// The value of the MISC_TEXT_2 attribute.
  /// Miscellaneous text 2 to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("miscText2")]
  [Member(Index = 14, Type = MemberType.Char, Length = MiscText2_MaxLength, Optional
    = true)]
  public string MiscText2
  {
    get => miscText2;
    set => miscText2 = value != null
      ? TrimEnd(Substring(value, 1, MiscText2_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the MISC_NUM_1 attribute.
  /// Miscellaneous whole number 1 to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("miscNum1")]
  [Member(Index = 15, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? MiscNum1
  {
    get => miscNum1;
    set => miscNum1 = value;
  }

  /// <summary>
  /// The value of the MISC_NUM_2 attribute.
  /// Micellaneous whole number 2 to be passed on the next tran action.
  /// </summary>
  [JsonPropertyName("miscNum2")]
  [Member(Index = 16, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? MiscNum2
  {
    get => miscNum2;
    set => miscNum2 = value;
  }

  /// <summary>
  /// The value of the MISC_NUM_1_V2 attribute.
  /// Miscellaneous number 1 with 2 decimal places to be passed on the next tran
  /// action.
  /// </summary>
  [JsonPropertyName("miscNum1V2")]
  [Member(Index = 17, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? MiscNum1V2
  {
    get => miscNum1V2;
    set => miscNum1V2 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the MISC_NUM_2_V2 attribute.
  /// Miscellaneous number 2 with 2 decimal places to be passed on the next tran
  /// action.
  /// </summary>
  [JsonPropertyName("miscNum2V2")]
  [Member(Index = 18, Type = MemberType.Number, Length = 15, Precision = 2, Optional
    = true)]
  public decimal? MiscNum2V2
  {
    get => miscNum2V2;
    set => miscNum2V2 = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("ospId")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 5)]
  public int OspId
  {
    get => ospId;
    set => ospId = value;
  }

  private string lastTran;
  private int? legalActionIdentifier;
  private string courtCaseNumber;
  private string caseNumber;
  private string csePersonNumber;
  private string csePersonNumberAp;
  private string csePersonNumberObligee;
  private string csePersonNumberObligor;
  private string courtOrderNumber;
  private int? obligationId;
  private string standardCrtOrdNumber;
  private int? infrastructureId;
  private string miscText1;
  private string miscText2;
  private long? miscNum1;
  private long? miscNum2;
  private decimal? miscNum1V2;
  private decimal? miscNum2V2;
  private int ospId;
}

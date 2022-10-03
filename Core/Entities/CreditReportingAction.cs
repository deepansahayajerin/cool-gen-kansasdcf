﻿// The source file: CREDIT_REPORTING_ACTION, ID: 371432756, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLEN
/// This entity contains the history of all the credit reporting actions we have
/// taken.
/// </summary>
[Serializable]
public partial class CreditReportingAction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CreditReportingAction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CreditReportingAction(CreditReportingAction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CreditReportingAction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CreditReportingAction that)
  {
    base.Assign(that);
    identifier = that.identifier;
    cseActionCode = that.cseActionCode;
    craTransCode = that.craTransCode;
    craTransDate = that.craTransDate;
    dateSentToCra = that.dateSentToCra;
    originalAmount = that.originalAmount;
    currentAmount = that.currentAmount;
    highestAmount = that.highestAmount;
    aacTakenDate = that.aacTakenDate;
    aacType = that.aacType;
    aacTanfCode = that.aacTanfCode;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This attribute together with the relationsip to CREDIT_REPORTING_ACTION 
  /// identifies one occurrence of CREDIT_REPORTING_ACTION. This is
  /// automatically generated by the system starting from 1 for each
  /// CREDIT_REPORTING (subtype of ADMINISTRATIVE_ACT_CERTIFICATION).
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 4)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CSE_ACTION_CODE attribute.</summary>
  public const int CseActionCode_MaxLength = 3;

  /// <summary>
  /// The value of the CSE_ACTION_CODE attribute.
  /// This attribute specifies the action taken by CSE. The valid values are 
  /// maintained in CODE_VALUE entity for the CODE code_name CRED_CSE_ACTION.
  /// The typical values are
  ///   &quot;ACT&quot; when a new record is sent to CRA
  ///   &quot;UPD&quot; when an update is sent to CRA
  ///   &quot;CAN&quot; when the worker cancels a notification
  ///   &quot;DEL&quot; when D04 records needs to be sent to CRA
  /// </summary>
  [JsonPropertyName("cseActionCode")]
  [Member(Index = 2, Type = MemberType.Char, Length = CseActionCode_MaxLength, Optional
    = true)]
  public string CseActionCode
  {
    get => cseActionCode;
    set => cseActionCode = value != null
      ? TrimEnd(Substring(value, 1, CseActionCode_MaxLength)) : null;
  }

  /// <summary>Length of the CRA_TRANS_CODE attribute.</summary>
  public const int CraTransCode_MaxLength = 3;

  /// <summary>
  /// The value of the CRA_TRANS_CODE attribute.
  /// This attributes specifies the transaction code for CRA record.  The valid 
  /// values are maintained in CODE_VALUE entity for the CODE code_value
  /// CRED_CRA_TRANS. The typical values are
  ///   &quot;A70&quot;, &quot;A93&quot; when a new record is sent to CRA
  ///   &quot;U70&quot;, U90&quot; when an update is sent to CRA
  ///   &quot;D04&quot; when a deletion records needs to be sent to CRA
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CraTransCode_MaxLength)]
  public string CraTransCode
  {
    get => craTransCode ?? "";
    set => craTransCode = TrimEnd(Substring(value, 1, CraTransCode_MaxLength));
  }

  /// <summary>
  /// The json value of the CraTransCode attribute.</summary>
  [JsonPropertyName("craTransCode")]
  [Computed]
  public string CraTransCode_Json
  {
    get => NullIf(CraTransCode, "");
    set => CraTransCode = value;
  }

  /// <summary>
  /// The value of the CRA_TRANS_DATE attribute.
  /// This attribute specifies the date action was taken by CSE.
  /// </summary>
  [JsonPropertyName("craTransDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? CraTransDate
  {
    get => craTransDate;
    set => craTransDate = value;
  }

  /// <summary>
  /// The value of the DATE_SENT_TO_CRA attribute.
  /// This attribute specifies the date the record was sent to Credit Reporting 
  /// Agency.
  /// </summary>
  [JsonPropertyName("dateSentToCra")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? DateSentToCra
  {
    get => dateSentToCra;
    set => dateSentToCra = value;
  }

  /// <summary>
  /// The value of the ORIGINAL_AMOUNT attribute.
  /// This attribute specifies the original amount certified for Credit 
  /// Reporting. If a D04 record was sent, a new original amount will be
  /// subsequently sent.
  /// </summary>
  [JsonPropertyName("originalAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? OriginalAmount
  {
    get => originalAmount;
    set => originalAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the CURRENT_AMOUNT attribute.
  /// This attribute specifies the current amount certified for Credit 
  /// Reporting.
  /// </summary>
  [JsonPropertyName("currentAmount")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? CurrentAmount
  {
    get => currentAmount;
    set => currentAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the HIGHEST_AMOUNT attribute.
  /// This attribute specifies the highest amount reported to the Credit 
  /// Reporting Agency. This amount will not go down. It can only go up. However
  /// if a D04 record was sent, a new highest amount will be subsequently sent.
  /// </summary>
  [JsonPropertyName("highestAmount")]
  [Member(Index = 8, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? HighestAmount
  {
    get => highestAmount;
    set => highestAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the TAKEN_DATE attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("aacTakenDate")]
  [Member(Index = 9, Type = MemberType.Date)]
  public DateTime? AacTakenDate
  {
    get => aacTakenDate;
    set => aacTakenDate = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int AacType_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of certified enforcement action taken.
  /// They can be: FDSO
  ///              SDSO
  ///              CRED
  ///              RECA
  ///              IRS
  ///              KSMW
  /// 
  /// KDWP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = AacType_MaxLength)]
  [Value("KDWP")]
  [Value("KDMV")]
  [Value("FDSO")]
  [Value("IRSC")]
  [Value("COAG")]
  [Value("CRED")]
  [Value("SDSO")]
  [Value("KSMW")]
  public string AacType
  {
    get => aacType ?? "";
    set => aacType = TrimEnd(Substring(value, 1, AacType_MaxLength));
  }

  /// <summary>
  /// The json value of the AacType attribute.</summary>
  [JsonPropertyName("aacType")]
  [Computed]
  public string AacType_Json
  {
    get => NullIf(AacType, "");
    set => AacType = value;
  }

  /// <summary>Length of the TANF_CODE attribute.</summary>
  public const int AacTanfCode_MaxLength = 1;

  /// <summary>
  /// The value of the TANF_CODE attribute.
  /// Code used to identify TANF or non-TANF.                                 T 
  /// - TANF
  /// 
  /// N - Non-TANF
  /// 
  /// Space - Not Seperated by TANF (Default)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = AacTanfCode_MaxLength)]
  public string AacTanfCode
  {
    get => aacTanfCode ?? "";
    set => aacTanfCode = TrimEnd(Substring(value, 1, AacTanfCode_MaxLength));
  }

  /// <summary>
  /// The json value of the AacTanfCode attribute.</summary>
  [JsonPropertyName("aacTanfCode")]
  [Computed]
  public string AacTanfCode_Json
  {
    get => NullIf(AacTanfCode, "");
    set => AacTanfCode = value;
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
  [Member(Index = 12, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  [Member(Index = 13, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  private int identifier;
  private string cseActionCode;
  private string craTransCode;
  private DateTime? craTransDate;
  private DateTime? dateSentToCra;
  private decimal? originalAmount;
  private decimal? currentAmount;
  private decimal? highestAmount;
  private DateTime? aacTakenDate;
  private string aacType;
  private string aacTanfCode;
  private string cspNumber;
  private string cpaType;
}

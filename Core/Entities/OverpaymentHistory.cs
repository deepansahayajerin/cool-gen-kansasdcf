// The source file: OVERPAYMENT_HISTORY, ID: 371438604, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This entity will store information regarding the overpayment intent for any 
/// specific time period.
/// </summary>
[Serializable]
public partial class OverpaymentHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OverpaymentHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OverpaymentHistory(OverpaymentHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OverpaymentHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OverpaymentHistory that)
  {
    base.Assign(that);
    overpaymentInd = that.overpaymentInd;
    effectiveDt = that.effectiveDt;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
  }

  /// <summary>Length of the OVERPAYMENT_IND attribute.</summary>
  public const int OverpaymentInd_MaxLength = 1;

  /// <summary>
  /// The value of the OVERPAYMENT_IND attribute.
  /// Represents the intent of a Obligor when additional money is received over 
  /// the current balance due.
  /// The ONLY time a payment may be applied treated as a future payment or as a
  /// gift is if all obligations are paid in full and there is an active
  /// support judgement.
  /// Example:
  /// N - Notice sent to obligor		        F - Indicates Future Payment
  /// G - Treat additional money as a gift
  /// R - Refund all money overage		       	
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = OverpaymentInd_MaxLength)]
  [Value("G")]
  [Value("R")]
  [Value("N")]
  [Value("S")]
  [Value("F")]
  [Value("C")]
  [ImplicitValue("G")]
  public string OverpaymentInd
  {
    get => overpaymentInd ?? "";
    set => overpaymentInd =
      TrimEnd(Substring(value, 1, OverpaymentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the OverpaymentInd attribute.</summary>
  [JsonPropertyName("overpaymentInd")]
  [Computed]
  public string OverpaymentInd_Json
  {
    get => NullIf(OverpaymentInd, "");
    set => OverpaymentInd = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DT attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDt")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDt
  {
    get => effectiveDt;
    set => effectiveDt = value;
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
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
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
  [Member(Index = 5, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private string overpaymentInd;
  private DateTime? effectiveDt;
  private string createdBy;
  private DateTime? createdTmst;
  private string cpaType;
  private string cspNumber;
}

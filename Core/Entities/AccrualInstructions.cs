// The source file: ACCRUAL_INSTRUCTIONS, ID: 371429980, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This represents the accruing instructions.  It specifies the amount due, a 
/// &quot;as of date&quot; and a accrual schedule.  When accrual is run, Set
/// Amount Debts will be created to support a specific accrual period debt
/// encured.  The Accruing Debt is tied to a specific supported child through
/// the program that is currently in effect.  This can change over time and when
/// this happens, the Accruing Debt is transfered to the new program for that
/// supported person.
/// Examples:
///   - Child Support Order
///   - Spousal Support Order
/// </summary>
[Serializable]
public partial class AccrualInstructions: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AccrualInstructions()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AccrualInstructions(AccrualInstructions that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AccrualInstructions Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AccrualInstructions that)
  {
    base.Assign(that);
    asOfDt = that.asOfDt;
    discontinueDt = that.discontinueDt;
    lastAccrualDt = that.lastAccrualDt;
    otrType = that.otrType;
    otyId = that.otyId;
    otrGeneratedId = that.otrGeneratedId;
    cpaType = that.cpaType;
    cspNumber = that.cspNumber;
    obgGeneratedId = that.obgGeneratedId;
  }

  /// <summary>
  /// The value of the AS_OF_DT attribute.
  /// The date the accruing debt instructions are to become effective.  This 
  /// date can be in the past, current or future when initially creating the
  /// accruing debt.  Once the accruing debt is created and transactions
  /// relating to it are recorded, the as of date can not be made retroactive,
  /// only future.
  /// </summary>
  [JsonPropertyName("asOfDt")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? AsOfDt
  {
    get => asOfDt;
    set => asOfDt = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DT attribute.
  /// The date the instructions for the accruing date become obsolete.  The 
  /// discontinue date can only be in the future. If a discontinue date is not
  /// specified, the date is set to 12-31-2099.
  /// </summary>
  [JsonPropertyName("discontinueDt")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDt
  {
    get => discontinueDt;
    set => discontinueDt = value;
  }

  /// <summary>
  /// The value of the LAST_ACCRUAL_DT attribute.
  /// The date these accrual instructions were last used in accruing the 
  /// associated debt.
  /// </summary>
  [JsonPropertyName("lastAccrualDt")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? LastAccrualDt
  {
    get => lastAccrualDt;
    set => lastAccrualDt = value;
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = OtrType_MaxLength)]
  [Value("DE")]
  [Value("Z2")]
  [Value("DA")]
  [ImplicitValue("DE")]
  public string OtrType
  {
    get => otrType ?? "";
    set => otrType = TrimEnd(Substring(value, 1, OtrType_MaxLength));
  }

  /// <summary>
  /// The json value of the OtrType attribute.</summary>
  [JsonPropertyName("otrType")]
  [Computed]
  public string OtrType_Json
  {
    get => NullIf(OtrType, "");
    set => OtrType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int OtyId
  {
    get => otyId;
    set => otyId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("otrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 9)]
  public int OtrGeneratedId
  {
    get => otrGeneratedId;
    set => otrGeneratedId = value;
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
  [Member(Index = 7, Type = MemberType.Char, Length = CpaType_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 3)]
  public int ObgGeneratedId
  {
    get => obgGeneratedId;
    set => obgGeneratedId = value;
  }

  private DateTime? asOfDt;
  private DateTime? discontinueDt;
  private DateTime? lastAccrualDt;
  private string otrType;
  private int otyId;
  private int otrGeneratedId;
  private string cpaType;
  private string cspNumber;
  private int obgGeneratedId;
}

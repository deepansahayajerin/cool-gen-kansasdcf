// The source file: DISTRIBUTION_POLICY_RULE, ID: 371433963, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Represents the distribution policy rule applied to a collection based on the
/// distribution policy, debt type, program, debt accrual type, and debt state.
/// </summary>
[Serializable]
public partial class DistributionPolicyRule: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DistributionPolicyRule()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DistributionPolicyRule(DistributionPolicyRule that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DistributionPolicyRule Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DistributionPolicyRule that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    firstLastIndicator = that.firstLastIndicator;
    debtFunctionType = that.debtFunctionType;
    debtState = that.debtState;
    applyTo = that.applyTo;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    distributeToOrderTypeCode = that.distributeToOrderTypeCode;
    dbpGeneratedId = that.dbpGeneratedId;
    dprNextId = that.dprNextId;
    dbpNextId = that.dbpNextId;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the FIRST_LAST_INDICATOR attribute.</summary>
  public const int FirstLastIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FIRST_LAST_INDICATOR attribute.
  /// ZDELETE
  /// </summary>
  [JsonPropertyName("firstLastIndicator")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = FirstLastIndicator_MaxLength, Optional = true)]
  [Value(null)]
  [Value("")]
  [Value("L")]
  [Value("F")]
  [ImplicitValue("")]
  public string FirstLastIndicator
  {
    get => firstLastIndicator;
    set => firstLastIndicator = value != null
      ? TrimEnd(Substring(value, 1, FirstLastIndicator_MaxLength)) : null;
  }

  /// <summary>Length of the DEBT_FUNCTION_TYPE attribute.</summary>
  public const int DebtFunctionType_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_FUNCTION_TYPE attribute.
  /// Represents the functionality of the debt.
  /// Examples:
  ///   - Accruing
  ///   - Non-Accuring w/ Payment Schedule
  ///   - Non-Accuring w/o Payment Schedule
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = DebtFunctionType_MaxLength)
    ]
  [Value("A")]
  [Value("")]
  [Value("R")]
  [Value("O")]
  [Value("W")]
  [ImplicitValue("A")]
  public string DebtFunctionType
  {
    get => debtFunctionType ?? "";
    set => debtFunctionType =
      TrimEnd(Substring(value, 1, DebtFunctionType_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtFunctionType attribute.</summary>
  [JsonPropertyName("debtFunctionType")]
  [Computed]
  public string DebtFunctionType_Json
  {
    get => NullIf(DebtFunctionType, "");
    set => DebtFunctionType = value;
  }

  /// <summary>Length of the DEBT_STATE attribute.</summary>
  public const int DebtState_MaxLength = 1;

  /// <summary>
  /// The value of the DEBT_STATE attribute.
  /// Defines whether the debt is for current support or arrears.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = DebtState_MaxLength)]
  [Value("C")]
  [Value("A")]
  [ImplicitValue("C")]
  public string DebtState
  {
    get => debtState ?? "";
    set => debtState = TrimEnd(Substring(value, 1, DebtState_MaxLength));
  }

  /// <summary>
  /// The json value of the DebtState attribute.</summary>
  [JsonPropertyName("debtState")]
  [Computed]
  public string DebtState_Json
  {
    get => NullIf(DebtState, "");
    set => DebtState = value;
  }

  /// <summary>Length of the APPLY_TO attribute.</summary>
  public const int ApplyTo_MaxLength = 1;

  /// <summary>
  /// The value of the APPLY_TO attribute.
  /// Determines whether the collection will be applied to the debt itself or 
  /// the interest incurred on the debt.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ApplyTo_MaxLength)]
  [Value("I")]
  [Value("D")]
  [ImplicitValue("D")]
  public string ApplyTo
  {
    get => applyTo ?? "";
    set => applyTo = TrimEnd(Substring(value, 1, ApplyTo_MaxLength));
  }

  /// <summary>
  /// The json value of the ApplyTo attribute.</summary>
  [JsonPropertyName("applyTo")]
  [Computed]
  public string ApplyTo_Json
  {
    get => NullIf(ApplyTo, "");
    set => ApplyTo = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
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
  /// The value of the CREATED_TMST attribute.
  /// The timestamp of when the occurrence of the entity type was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id repsonsible for the last udate to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the entity occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the DISTRIBUTE_TO_ORDER_TYPE_CODE attribute.</summary>
  public const int DistributeToOrderTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the DISTRIBUTE_TO_ORDER_TYPE_CODE attribute.
  /// Defines the type of obligation to distribute collections to.
  /// 	I - Dist. to an Incoming Interstate Obl.
  /// 	K - Dist. to a Kansas Obl
  ///     space - Dist. to either
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = DistributeToOrderTypeCode_MaxLength)]
  [Value("I")]
  [Value("")]
  [Value("K")]
  public string DistributeToOrderTypeCode
  {
    get => distributeToOrderTypeCode ?? "";
    set => distributeToOrderTypeCode =
      TrimEnd(Substring(value, 1, DistributeToOrderTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DistributeToOrderTypeCode attribute.</summary>
  [JsonPropertyName("distributeToOrderTypeCode")]
  [Computed]
  public string DistributeToOrderTypeCode_Json
  {
    get => NullIf(DistributeToOrderTypeCode, "");
    set => DistributeToOrderTypeCode = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbpGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int DbpGeneratedId
  {
    get => dbpGeneratedId;
    set => dbpGeneratedId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dprNextId")]
  [Member(Index = 12, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? DprNextId
  {
    get => dprNextId;
    set => dprNextId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("dbpNextId")]
  [Member(Index = 13, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? DbpNextId
  {
    get => dbpNextId;
    set => dbpNextId = value;
  }

  private int systemGeneratedIdentifier;
  private string firstLastIndicator;
  private string debtFunctionType;
  private string debtState;
  private string applyTo;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string distributeToOrderTypeCode;
  private int dbpGeneratedId;
  private int? dprNextId;
  private int? dbpNextId;
}

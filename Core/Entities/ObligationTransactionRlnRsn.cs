// The source file: OBLIGATION_TRANSACTION_RLN_RSN, ID: 371438305, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Defines the reason for financial transaction relationships.
/// Examples: Accrual creates debt, adjustment modifies debt, etc.
/// </summary>
[Serializable]
public partial class ObligationTransactionRlnRsn: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationTransactionRlnRsn()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationTransactionRlnRsn(ObligationTransactionRlnRsn that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationTransactionRlnRsn Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationTransactionRlnRsn that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    name = that.name;
    description = that.description;
    effectiveDt = that.effectiveDt;
    discontinueDt = that.discontinueDt;
    classification = that.classification;
    applyTo = that.applyTo;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
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

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 7;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name fro the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => code ?? "";
    set => code = TrimEnd(Substring(value, 1, Code_MaxLength));
  }

  /// <summary>
  /// The json value of the Code attribute.</summary>
  [JsonPropertyName("code")]
  [Computed]
  public string Code_Json
  {
    get => NullIf(Code, "");
    set => Code = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the code.  This name will be more descriptive than the code 
  /// but will be much less shorter than the description.  The reason for
  /// needing the name is that the code is not descriptive enough and the
  /// description is too long for the list screens.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// An explanation of the business function.  The description should be 
  /// specific enough to allow a person to distinguish/understand the business
  /// function.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DT attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An effective date allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDt")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? EffectiveDt
  {
    get => effectiveDt;
    set => effectiveDt = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DT attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDt")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDt
  {
    get => discontinueDt;
    set => discontinueDt = value;
  }

  /// <summary>Length of the CLASSIFICATION attribute.</summary>
  public const int Classification_MaxLength = 4;

  /// <summary>
  /// The value of the CLASSIFICATION attribute.
  /// Describes a classification of relationship reason. This will enable 
  /// logical grouping of reasons.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Classification_MaxLength)]
  [Value("CA")]
  [Value("A")]
  [Value("CC")]
  [Value("DIST")]
  [Value("DA")]
  public string Classification
  {
    get => classification ?? "";
    set => classification =
      TrimEnd(Substring(value, 1, Classification_MaxLength));
  }

  /// <summary>
  /// The json value of the Classification attribute.</summary>
  [JsonPropertyName("classification")]
  [Computed]
  public string Classification_Json
  {
    get => NullIf(Classification, "");
    set => Classification = value;
  }

  /// <summary>Length of the APPLY_TO attribute.</summary>
  public const int ApplyTo_MaxLength = 1;

  /// <summary>
  /// The value of the APPLY_TO attribute.
  /// Defines whether or not the colloection or debt adjustment was applied to 
  /// the balance owed or the interest owed. Not valid for collection
  /// adjustments.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ApplyTo_MaxLength)]
  [Value("G")]
  [Value("I")]
  [Value("B")]
  [Value("")]
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
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  private int systemGeneratedIdentifier;
  private string code;
  private string name;
  private string description;
  private DateTime? effectiveDt;
  private DateTime? discontinueDt;
  private string classification;
  private string applyTo;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
}

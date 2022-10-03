// The source file: OBLIGATION_TYPE, ID: 371438364, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Represents the terms of the monetary obligation owed.  In addition to 
/// providing a description of the obligation owed, it is used in conjunction
/// with the source and program type to indicate the distribution policy used in
/// applying collections for that debt.
/// Examples: Lump sum judgements, Interest, Fee, Recovery, Arrearage, 
/// Recurring, Medical, Spousal support
/// Defined Categories are:
///   S - Support Debts (Result of Accrual)
///   A - Accrual Instructions
///   R - Recovery Debts
///   M - Medical Debts
///   O - Other Valid Debts
/// </summary>
[Serializable]
public partial class ObligationType: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationType()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationType(ObligationType that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationType Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationType that)
  {
    base.Assign(that);
    supportedPersonReqInd = that.supportedPersonReqInd;
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    name = that.name;
    description = that.description;
    classification = that.classification;
    effectiveDt = that.effectiveDt;
    discontinueDt = that.discontinueDt;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
  }

  /// <summary>Length of the SUPPORTED_PERSON_REQ_IND attribute.</summary>
  public const int SupportedPersonReqInd_MaxLength = 1;

  /// <summary>
  /// The value of the SUPPORTED_PERSON_REQ_IND attribute.
  /// Indicates the requirement of a supported person for a specific obligation 
  /// type.  Support types require a supported person.  Recovery types do not
  /// require a supported person.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = SupportedPersonReqInd_MaxLength)]
  [Value("Y")]
  [Value("N")]
  public string SupportedPersonReqInd
  {
    get => supportedPersonReqInd ?? "";
    set => supportedPersonReqInd =
      TrimEnd(Substring(value, 1, SupportedPersonReqInd_MaxLength));
  }

  /// <summary>
  /// The json value of the SupportedPersonReqInd attribute.</summary>
  [JsonPropertyName("supportedPersonReqInd")]
  [Computed]
  public string SupportedPersonReqInd_Json
  {
    get => NullIf(SupportedPersonReqInd, "");
    set => SupportedPersonReqInd = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 7;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Code_MaxLength)]
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
  [Member(Index = 4, Type = MemberType.Char, Length = Name_MaxLength)]
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
  /// An explanation of the entity type.  The description should be specific 
  /// enough to allow a person to distinguish/understand the entity type.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 5, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>Length of the CLASSIFICATION attribute.</summary>
  public const int Classification_MaxLength = 1;

  /// <summary>
  /// The value of the CLASSIFICATION attribute.
  /// Defines a class of debt type to aid in determining how processes will 
  /// enteract with the various debt types.
  /// Examples:
  ///   A - Accrual Instructions
  ///   S - Support Debts (Result of Accrual)
  ///   R - Recovery Debts
  ///   M - Medical Debts
  ///   O - Other Valid Debts
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Classification_MaxLength)]
  [Value("F")]
  [Value("V")]
  [Value("N")]
  [Value("H")]
  [Value("O")]
  [Value("A")]
  [Value("R")]
  [Value("M")]
  [Value("S")]
  [ImplicitValue("S")]
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

  /// <summary>
  /// The value of the EFFECTIVE_DT attribute.
  /// The date upon which this occurrence of the entity is activated by the 
  /// system.  An EFFECTIVE_DATE allows the entity to be entered into the system
  /// with a future date.  The occurrence of the entity will &quot;take effect
  /// &quot; on the EFFECTIVE_DATE.
  /// </summary>
  [JsonPropertyName("effectiveDt")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? EffectiveDt
  {
    get => effectiveDt;
    set => effectiveDt = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DT attribute.
  /// The date upon wich this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDt")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDt
  {
    get => discontinueDt;
    set => discontinueDt = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
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
  /// The timestamp of when the occurrence of the entity type was created.
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
  /// The user id or program id responsible for the last update to the 
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
  /// The timestamp of the most recent update to the entity occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 12, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  private string supportedPersonReqInd;
  private int systemGeneratedIdentifier;
  private string code;
  private string name;
  private string description;
  private string classification;
  private DateTime? effectiveDt;
  private DateTime? discontinueDt;
  private string createdBy;
  private DateTime? createdTmst;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
}

// The source file: OBLIGATION_RLN, ID: 371438135, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Relates two Obligation entity types together for the reason of joint and 
/// several obligations.  This is when both the mother and father are ordered to
/// pay an amount for child support.  Both parents are equally responsible.
/// This will set up two entirely separate obligations exactly the same and then
/// relate them together.  All collection will be applied to both obligations
/// in full.
/// </summary>
[Serializable]
public partial class ObligationRln: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ObligationRln()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ObligationRln(ObligationRln that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ObligationRln Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ObligationRln that)
  {
    base.Assign(that);
    description = that.description;
    createdBy = that.createdBy;
    createdTmst = that.createdTmst;
    otyFirstId = that.otyFirstId;
    obgFGeneratedId = that.obgFGeneratedId;
    cspFNumber = that.cspFNumber;
    cpaFType = that.cpaFType;
    otySecondId = that.otySecondId;
    obgGeneratedId = that.obgGeneratedId;
    cspNumber = that.cspNumber;
    cpaType = that.cpaType;
    orrGeneratedId = that.orrGeneratedId;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// Represent the description of the reason for relating two transactions 
  /// together.
  /// Example: Collection &quot;satisifies&quot; debt - where &quot;satisifies
  /// &quot; respresents the reason.  This may have a description of &quot;
  /// received from AP's mother&quot;.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Varchar, Length = Description_MaxLength)]
  public string Description
  {
    get => description ?? "";
    set => description = Substring(value, 1, Description_MaxLength);
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => createdTmst;
    set => createdTmst = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otyFirstId")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int OtyFirstId
  {
    get => otyFirstId;
    set => otyFirstId = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("obgFGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int ObgFGeneratedId
  {
    get => obgFGeneratedId;
    set => obgFGeneratedId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspFNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CspFNumber_MaxLength)]
  public string CspFNumber
  {
    get => cspFNumber ?? "";
    set => cspFNumber = TrimEnd(Substring(value, 1, CspFNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspFNumber attribute.</summary>
  [JsonPropertyName("cspFNumber")]
  [Computed]
  public string CspFNumber_Json
  {
    get => NullIf(CspFNumber, "");
    set => CspFNumber = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int CpaFType_MaxLength = 1;

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
  [Member(Index = 7, Type = MemberType.Char, Length = CpaFType_MaxLength)]
  [Value("S")]
  [Value("R")]
  [Value("E")]
  [ImplicitValue("S")]
  public string CpaFType
  {
    get => cpaFType ?? "";
    set => cpaFType = TrimEnd(Substring(value, 1, CpaFType_MaxLength));
  }

  /// <summary>
  /// The json value of the CpaFType attribute.</summary>
  [JsonPropertyName("cpaFType")]
  [Computed]
  public string CpaFType_Json
  {
    get => NullIf(CpaFType, "");
    set => CpaFType = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("otySecondId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 3)]
  public int OtySecondId
  {
    get => otySecondId;
    set => otySecondId = value;
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

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  [Member(Index = 11, Type = MemberType.Char, Length = CpaType_MaxLength)]
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

  /// <summary>
  /// The value of the SEQUENTIAL_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("orrGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int OrrGeneratedId
  {
    get => orrGeneratedId;
    set => orrGeneratedId = value;
  }

  private string description;
  private string createdBy;
  private DateTime? createdTmst;
  private int otyFirstId;
  private int obgFGeneratedId;
  private string cspFNumber;
  private string cpaFType;
  private int otySecondId;
  private int obgGeneratedId;
  private string cspNumber;
  private string cpaType;
  private int orrGeneratedId;
}

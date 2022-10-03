// The source file: CODE_VALUE_COMBINATION, ID: 371421514, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Used when a combination of code values are needed to validate with.
/// </summary>
[Serializable]
public partial class CodeValueCombination: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CodeValueCombination()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CodeValueCombination(CodeValueCombination that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CodeValueCombination Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CodeValueCombination that)
  {
    base.Assign(that);
    id = that.id;
    effectiveDate = that.effectiveDate;
    expirationDate = that.expirationDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    covId = that.covId;
    covSId = that.covSId;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// An attribute to uniquely identify one instance of the 
  /// CODE_VALUE_COMBINATION entity type.
  /// </summary>
  [JsonPropertyName("id")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int Id
  {
    get => id;
    set => id = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date (including it) from which the code value combination is valid.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// The date, (including it and) upto which the code value combination is 
  /// valid
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? ExpirationDate
  {
    get => expirationDate;
    set => expirationDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// Attribute to uniquely identify one instance of CODE_VALUE entity type.
  /// </summary>
  [JsonPropertyName("covId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 8)]
  public int CovId
  {
    get => covId;
    set => covId = value;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// Attribute to uniquely identify one instance of CODE_VALUE entity type.
  /// </summary>
  [JsonPropertyName("covSId")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 8)]
  public int CovSId
  {
    get => covSId;
    set => covSId = value;
  }

  private int id;
  private DateTime? effectiveDate;
  private DateTime? expirationDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int covId;
  private int covSId;
}

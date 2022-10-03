// The source file: CODE_VALUE, ID: 371421477, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// The value that is valid for a code when used as an allowed value for 
/// validation.
/// </summary>
[Serializable]
public partial class CodeValue: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CodeValue()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CodeValue(CodeValue that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CodeValue Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CodeValue that)
  {
    base.Assign(that);
    id = that.id;
    cdvalue = that.cdvalue;
    effectiveDate = that.effectiveDate;
    expirationDate = that.expirationDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    description = that.description;
    codId = that.codId;
  }

  /// <summary>
  /// The value of the ID attribute.
  /// Attribute to uniquely identify one instance of CODE_VALUE entity type.
  /// </summary>
  [JsonPropertyName("id")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 8)]
  public int Id
  {
    get => id;
    set => id = value;
  }

  /// <summary>Length of the CDVALUE attribute.</summary>
  public const int Cdvalue_MaxLength = 10;

  /// <summary>
  /// The value of the CDVALUE attribute.
  /// The permitted value.
  /// e.g. CODE CODE_NAME = &quot;STATE_CODE&quot;
  ///      CODE_VALUE VALUE = &quot;KS&quot;
  ///      CODE_VALUE DESCRIPTION = &quot;Kansas State&quot;
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Cdvalue_MaxLength)]
  public string Cdvalue
  {
    get => cdvalue ?? "";
    set => cdvalue = TrimEnd(Substring(value, 1, Cdvalue_MaxLength));
  }

  /// <summary>
  /// The json value of the Cdvalue attribute.</summary>
  [JsonPropertyName("cdvalue")]
  [Computed]
  public string Cdvalue_Json
  {
    get => NullIf(Cdvalue, "");
    set => Cdvalue = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date from which (including it) the permitted value is valid.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the EXPIRATION_DATE attribute.
  /// The date (including and) upto which the permitted value is valid.
  /// </summary>
  [JsonPropertyName("expirationDate")]
  [Member(Index = 4, Type = MemberType.Date)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Timestamp)]
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
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 64;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// The description for the permitted value.
  /// e.g. CODE CODE_NAME         = &quot;STATE_CODE&quot;
  ///      CODE_VALUE VALUE       = &quot;KS&quot;
  ///      CODE_VALUE DESCRIPTION = &quot;Kansas State&quot;
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Varchar, Length = Description_MaxLength)]
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

  /// <summary>
  /// The value of the ID attribute.
  /// Attribute to uniquely identify one instance of the CODE entity type.
  /// </summary>
  [JsonPropertyName("codId")]
  [Member(Index = 10, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? CodId
  {
    get => codId;
    set => codId = value;
  }

  private int id;
  private string cdvalue;
  private DateTime? effectiveDate;
  private DateTime? expirationDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string description;
  private int? codId;
}

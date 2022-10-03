// The source file: GLOBAL_STATEMENT_MESSAGE, ID: 371434978, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// A common place where management can record messages to be communicated on 
/// all obligor activity statements.
/// Example:  Interest Rate increase to 50% effective June 1, 1999.
/// </summary>
[Serializable]
public partial class GlobalStatementMessage: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GlobalStatementMessage()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GlobalStatementMessage(GlobalStatementMessage that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GlobalStatementMessage Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GlobalStatementMessage that)
  {
    base.Assign(that);
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdateTimestamp = that.lastUpdateTimestamp;
    effectiveMonth = that.effectiveMonth;
    effectiveYear = that.effectiveYear;
    textArea = that.textArea;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp the occurrence was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATE_TIMESTAMP attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdateTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdateTimestamp
  {
    get => lastUpdateTimestamp;
    set => lastUpdateTimestamp = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_MONTH attribute.
  /// The month in which the message is to be communicated on the statements.
  /// </summary>
  [JsonPropertyName("effectiveMonth")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int EffectiveMonth
  {
    get => effectiveMonth;
    set => effectiveMonth = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_YEAR attribute.
  /// The year in which the message is to be communicated on the statement
  /// </summary>
  [JsonPropertyName("effectiveYear")]
  [DefaultValue(0)]
  [Member(Index = 6, Type = MemberType.Number, Length = 4)]
  public int EffectiveYear
  {
    get => effectiveYear;
    set => effectiveYear = value;
  }

  /// <summary>Length of the TEXT_AREA attribute.</summary>
  public const int TextArea_MaxLength = 240;

  /// <summary>
  /// The value of the TEXT_AREA attribute.
  /// The message text area where the message is recorded.
  /// </summary>
  [JsonPropertyName("textArea")]
  [Member(Index = 7, Type = MemberType.Varchar, Length = TextArea_MaxLength, Optional
    = true)]
  public string TextArea
  {
    get => textArea;
    set => textArea = value != null
      ? Substring(value, 1, TextArea_MaxLength) : null;
  }

  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdateTimestamp;
  private int effectiveMonth;
  private int effectiveYear;
  private string textArea;
}

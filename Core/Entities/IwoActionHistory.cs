// The source file: IWO_ACTION_HISTORY, ID: 1902467078, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Records a history of the actions taken for an IWO_ACTION along with user id 
/// or batch program that initiated the action.  Example, the IWO_ACTION was
/// queued for portal processing, processed to the portal, accepted by the
/// portal, accepted or rejected by the employer, etc.
/// </summary>
[Serializable]
public partial class IwoActionHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public IwoActionHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public IwoActionHistory(IwoActionHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new IwoActionHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(IwoActionHistory that)
  {
    base.Assign(that);
    identifier = that.identifier;
    actionTaken = that.actionTaken;
    actionDate = that.actionDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
    lgaIdentifier = that.lgaIdentifier;
    iwtIdentifier = that.iwtIdentifier;
    iwaIdentifier = that.iwaIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// IWO_ACTION
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the ACTION_TAKEN attribute.</summary>
  public const int ActionTaken_MaxLength = 3;

  /// <summary>
  /// The value of the ACTION_TAKEN attribute.
  /// A code indicating the action that was initiated.
  /// </summary>
  [JsonPropertyName("actionTaken")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionTaken_MaxLength, Optional
    = true)]
  public string ActionTaken
  {
    get => actionTaken;
    set => actionTaken = value != null
      ? TrimEnd(Substring(value, 1, ActionTaken_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACTION_DATE attribute.
  /// The date the action was initiated for the IWO.
  /// </summary>
  [JsonPropertyName("actionDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? ActionDate
  {
    get => actionDate;
    set => actionDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// 	
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
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
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// cse_person and legal_action.
  /// </summary>
  [JsonPropertyName("iwtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 3)]
  public int IwtIdentifier
  {
    get => iwtIdentifier;
    set => iwtIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// IWO_TRANSACTION
  /// </summary>
  [JsonPropertyName("iwaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int IwaIdentifier
  {
    get => iwaIdentifier;
    set => iwaIdentifier = value;
  }

  private int identifier;
  private string actionTaken;
  private DateTime? actionDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
  private int lgaIdentifier;
  private int iwtIdentifier;
  private int iwaIdentifier;
}

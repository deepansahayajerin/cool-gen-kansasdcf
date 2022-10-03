// The source file: EMPLOYER_HISTORY, ID: 1902587093, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

[Serializable]
public partial class EmployerHistory: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EmployerHistory()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EmployerHistory(EmployerHistory that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EmployerHistory Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EmployerHistory that)
  {
    base.Assign(that);
    actionTaken = that.actionTaken;
    actionDate = that.actionDate;
    note = that.note;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    empId = that.empId;
  }

  /// <summary>Length of the ACTION_TAKEN attribute.</summary>
  public const int ActionTaken_MaxLength = 6;

  /// <summary>
  /// The value of the ACTION_TAKEN attribute.
  /// The action taken to the record (add/update).
  /// </summary>
  [JsonPropertyName("actionTaken")]
  [Member(Index = 1, Type = MemberType.Char, Length = ActionTaken_MaxLength, Optional
    = true)]
  public string ActionTaken
  {
    get => actionTaken;
    set => actionTaken = value != null
      ? TrimEnd(Substring(value, 1, ActionTaken_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACTION_DATE attribute.
  /// The date of the action was taken.
  /// </summary>
  [JsonPropertyName("actionDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? ActionDate
  {
    get => actionDate;
    set => actionDate = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 75;

  /// <summary>
  /// The value of the NOTE attribute.
  /// Miscellaneous info entered by a user.
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 3, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
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

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("empId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 9)]
  public int EmpId
  {
    get => empId;
    set => empId = value;
  }

  private string actionTaken;
  private DateTime? actionDate;
  private string note;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int empId;
}

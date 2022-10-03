// The source file: 529_ACCOUNT_PARTICIPANT, ID: 1902440211, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This table identifies non custodial parents participating in the 529 College
/// Savings Initiative program.  For each dollar deposited into the 529 account
/// qualifying NCPs receive a $2 credit towards arrears owed.  The NCP must pay
/// current child support due for the month and at least one dollar towards
/// arrears to receive the match.
/// </summary>
[Serializable]
public partial class Data529AccountParticipant: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Data529AccountParticipant()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Data529AccountParticipant(Data529AccountParticipant that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Data529AccountParticipant Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Data529AccountParticipant that)
  {
    base.Assign(that);
    identifier = that.identifier;
    standardNumber = that.standardNumber;
    startDate = that.startDate;
    endDate = that.endDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the STANDARD_NUMBER attribute.</summary>
  public const int StandardNumber_MaxLength = 20;

  /// <summary>
  /// The value of the STANDARD_NUMBER attribute.
  /// Standard court order number associated with the children for whom the NCP 
  /// is participating in the 529 college savings initiative.
  /// </summary>
  [JsonPropertyName("standardNumber")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = StandardNumber_MaxLength, Optional = true)]
  public string StandardNumber
  {
    get => standardNumber;
    set => standardNumber = value != null
      ? TrimEnd(Substring(value, 1, StandardNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// Standard court order number associated with the children for whom the NCP 
  /// is participating in the 529 college savings initiative.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// Date the NCPs participation in the 529 college savings initiative ends.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// 	
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 7, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 8, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 9, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private int identifier;
  private string standardNumber;
  private DateTime? startDate;
  private DateTime? endDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
}

// The source file: DISPLACED_PERSON, ID: 371255199, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:
/// </summary>
[Serializable]
public partial class DisplacedPerson: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisplacedPerson()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisplacedPerson(DisplacedPerson that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisplacedPerson Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisplacedPerson that)
  {
    base.Assign(that);
    number = that.number;
    effectiveDate = that.effectiveDate;
    endDate = that.endDate;
    displacedInd = that.displacedInd;
    displacedInterfaceInd = that.displacedInterfaceInd;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Number_MaxLength)]
  public string Number
  {
    get => number ?? "";
    set => number = TrimEnd(Substring(value, 1, Number_MaxLength));
  }

  /// <summary>
  /// The json value of the Number attribute.</summary>
  [JsonPropertyName("number")]
  [Computed]
  public string Number_Json
  {
    get => NullIf(Number, "");
    set => Number = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// Date the individual became impacted by Hurrican Katrina.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// Date the individual is no impacted by Hurricane Katrina.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the DISPLACED_IND attribute.</summary>
  public const int DisplacedInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISPLACED_IND attribute.
  /// </summary>
  [JsonPropertyName("displacedInd")]
  [Member(Index = 4, Type = MemberType.Char, Length = DisplacedInd_MaxLength, Optional
    = true)]
  public string DisplacedInd
  {
    get => displacedInd;
    set => displacedInd = value != null
      ? TrimEnd(Substring(value, 1, DisplacedInd_MaxLength)) : null;
  }

  /// <summary>Length of the DISPLACED_INTERFACE_IND attribute.</summary>
  public const int DisplacedInterfaceInd_MaxLength = 1;

  /// <summary>
  /// The value of the DISPLACED_INTERFACE_IND attribute.
  /// </summary>
  [JsonPropertyName("displacedInterfaceInd")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = DisplacedInterfaceInd_MaxLength, Optional = true)]
  public string DisplacedInterfaceInd
  {
    get => displacedInterfaceInd;
    set => displacedInterfaceInd = value != null
      ? TrimEnd(Substring(value, 1, DisplacedInterfaceInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
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
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
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

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  private string number;
  private DateTime? effectiveDate;
  private DateTime? endDate;
  private string displacedInd;
  private string displacedInterfaceInd;
  private string createdBy;
  private DateTime? createdTimestamp;
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
}

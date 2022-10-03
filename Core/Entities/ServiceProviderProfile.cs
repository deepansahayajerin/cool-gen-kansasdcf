// The source file: SERVICE_PROVIDER_PROFILE, ID: 371422766, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// INDICATES WHICH GROUP A USER IS PART OF
/// </summary>
[Serializable]
public partial class ServiceProviderProfile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ServiceProviderProfile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ServiceProviderProfile(ServiceProviderProfile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ServiceProviderProfile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ServiceProviderProfile that)
  {
    base.Assign(that);
    createdTimestamp = that.createdTimestamp;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    proName = that.proName;
    spdGenId = that.spdGenId;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// The date and time when the service provider profile is created (date is 
  /// inclusive).
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date the user becomes active in the profile.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The data the user is no longer active in the profile.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
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
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int ProName_MaxLength = 10;

  /// <summary>
  /// The value of the NAME attribute.
  /// NAME OF THE GROUP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ProName_MaxLength)]
  public string ProName
  {
    get => proName ?? "";
    set => proName = TrimEnd(Substring(value, 1, ProName_MaxLength));
  }

  /// <summary>
  /// The json value of the ProName attribute.</summary>
  [JsonPropertyName("proName")]
  [Computed]
  public string ProName_Json
  {
    get => NullIf(ProName, "");
    set => ProName = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("spdGenId")]
  [DefaultValue(0)]
  [Member(Index = 8, Type = MemberType.Number, Length = 5)]
  public int SpdGenId
  {
    get => spdGenId;
    set => spdGenId = value;
  }

  private DateTime? createdTimestamp;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string proName;
  private int spdGenId;
}

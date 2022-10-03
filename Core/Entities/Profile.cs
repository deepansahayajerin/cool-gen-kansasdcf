// The source file: PROFILE, ID: 371422639, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// CONTAINS THE NAMES OF THE GROUPS FOR WHICH AUTHORIZATION IS GRANTED.
/// </summary>
[Serializable]
public partial class Profile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Profile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Profile(Profile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Profile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Profile that)
  {
    base.Assign(that);
    name = that.name;
    desc = that.desc;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    restrictionCode1 = that.restrictionCode1;
    restrictionCode2 = that.restrictionCode2;
    restrictionCode3 = that.restrictionCode3;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 10;

  /// <summary>
  /// The value of the NAME attribute.
  /// NAME OF THE GROUP
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Name_MaxLength)]
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

  /// <summary>Length of the DESC attribute.</summary>
  public const int Desc_MaxLength = 25;

  /// <summary>
  /// The value of the DESC attribute.
  /// Description of the profile.
  /// </summary>
  [JsonPropertyName("desc")]
  [Member(Index = 2, Type = MemberType.Char, Length = Desc_MaxLength, Optional
    = true)]
  public string Desc
  {
    get => desc;
    set => desc = value != null
      ? TrimEnd(Substring(value, 1, Desc_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
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

  /// <summary>Length of the RESTRICTION_CODE_1 attribute.</summary>
  public const int RestrictionCode1_MaxLength = 4;

  /// <summary>
  /// The value of the RESTRICTION_CODE_1 attribute.
  /// Defines data restrictions associated with the security profile (e.g., FTIR
  /// - Federal Tax Information Restricted)
  /// </summary>
  [JsonPropertyName("restrictionCode1")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = RestrictionCode1_MaxLength, Optional = true)]
  public string RestrictionCode1
  {
    get => restrictionCode1;
    set => restrictionCode1 = value != null
      ? TrimEnd(Substring(value, 1, RestrictionCode1_MaxLength)) : null;
  }

  /// <summary>Length of the RESTRICTION_CODE_2 attribute.</summary>
  public const int RestrictionCode2_MaxLength = 4;

  /// <summary>
  /// The value of the RESTRICTION_CODE_2 attribute.
  /// Defines data restrictions associated with the security profile (e.g., FTIR
  /// - Federal Tax Information Restricted)
  /// </summary>
  [JsonPropertyName("restrictionCode2")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = RestrictionCode2_MaxLength, Optional = true)]
  public string RestrictionCode2
  {
    get => restrictionCode2;
    set => restrictionCode2 = value != null
      ? TrimEnd(Substring(value, 1, RestrictionCode2_MaxLength)) : null;
  }

  /// <summary>Length of the RESTRICTION_CODE_3 attribute.</summary>
  public const int RestrictionCode3_MaxLength = 4;

  /// <summary>
  /// The value of the RESTRICTION_CODE_3 attribute.
  /// Defines data restrictions associated with the security profile (e.g., FTIR
  /// - Federal Tax Information Restricted)
  /// </summary>
  [JsonPropertyName("restrictionCode3")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = RestrictionCode3_MaxLength, Optional = true)]
  public string RestrictionCode3
  {
    get => restrictionCode3;
    set => restrictionCode3 = value != null
      ? TrimEnd(Substring(value, 1, RestrictionCode3_MaxLength)) : null;
  }

  private string name;
  private string desc;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string restrictionCode1;
  private string restrictionCode2;
  private string restrictionCode3;
}

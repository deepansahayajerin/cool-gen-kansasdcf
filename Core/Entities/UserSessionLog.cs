// The source file: USER_SESSION_LOG, ID: 371337863, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// 
/// This entity is used to log users who attempt to violate application security
/// access.
/// </summary>
[Serializable]
public partial class UserSessionLog: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public UserSessionLog()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public UserSessionLog(UserSessionLog that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new UserSessionLog Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(UserSessionLog that)
  {
    base.Assign(that);
    userId = that.userId;
    createdTstamp = that.createdTstamp;
    systemName = that.systemName;
    tranId = that.tranId;
    profileName = that.profileName;
    violationMessage = that.violationMessage;
  }

  /// <summary>Length of the USER_ID attribute.</summary>
  public const int UserId_MaxLength = 8;

  /// <summary>
  /// The value of the USER_ID attribute.
  /// UserID of the individual who violated security access
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = UserId_MaxLength)]
  public string UserId
  {
    get => userId ?? "";
    set => userId = TrimEnd(Substring(value, 1, UserId_MaxLength));
  }

  /// <summary>
  /// The json value of the UserId attribute.</summary>
  [JsonPropertyName("userId")]
  [Computed]
  public string UserId_Json
  {
    get => NullIf(UserId, "");
    set => UserId = value;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the SYSTEM_NAME attribute.</summary>
  public const int SystemName_MaxLength = 20;

  /// <summary>
  /// The value of the SYSTEM_NAME attribute.
  /// System that created this record
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = SystemName_MaxLength)]
  public string SystemName
  {
    get => systemName ?? "";
    set => systemName = TrimEnd(Substring(value, 1, SystemName_MaxLength));
  }

  /// <summary>
  /// The json value of the SystemName attribute.</summary>
  [JsonPropertyName("systemName")]
  [Computed]
  public string SystemName_Json
  {
    get => NullIf(SystemName, "");
    set => SystemName = value;
  }

  /// <summary>Length of the TRAN_ID attribute.</summary>
  public const int TranId_MaxLength = 4;

  /// <summary>
  /// The value of the TRAN_ID attribute.
  /// The CICS transaction id that was used to violate the system
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = TranId_MaxLength)]
  public string TranId
  {
    get => tranId ?? "";
    set => tranId = TrimEnd(Substring(value, 1, TranId_MaxLength));
  }

  /// <summary>
  /// The json value of the TranId attribute.</summary>
  [JsonPropertyName("tranId")]
  [Computed]
  public string TranId_Json
  {
    get => NullIf(TranId, "");
    set => TranId = value;
  }

  /// <summary>Length of the PROFILE_NAME attribute.</summary>
  public const int ProfileName_MaxLength = 10;

  /// <summary>
  /// The value of the PROFILE_NAME attribute.
  /// The profile name that the user is tied to.
  /// </summary>
  [JsonPropertyName("profileName")]
  [Member(Index = 5, Type = MemberType.Char, Length = ProfileName_MaxLength, Optional
    = true)]
  public string ProfileName
  {
    get => profileName;
    set => profileName = value != null
      ? TrimEnd(Substring(value, 1, ProfileName_MaxLength)) : null;
  }

  /// <summary>Length of the VIOLATION_MESSAGE attribute.</summary>
  public const int ViolationMessage_MaxLength = 80;

  /// <summary>
  /// The value of the VIOLATION_MESSAGE attribute.
  /// Free form message field
  /// </summary>
  [JsonPropertyName("violationMessage")]
  [Member(Index = 6, Type = MemberType.Varchar, Length
    = ViolationMessage_MaxLength, Optional = true)]
  public string ViolationMessage
  {
    get => violationMessage;
    set => violationMessage = value != null
      ? Substring(value, 1, ViolationMessage_MaxLength) : null;
  }

  private string userId;
  private DateTime? createdTstamp;
  private string systemName;
  private string tranId;
  private string profileName;
  private string violationMessage;
}

// The source file: REGISTERED_AGENT, ID: 371440079, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Company to which an out of state employer is registered.
/// Example: MTW is based in Missouri. Employee x works for MTWin Kansas. An IWO
/// for that employee has to be served by a Kansas Company.
/// </summary>
[Serializable]
public partial class RegisteredAgent: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public RegisteredAgent()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public RegisteredAgent(RegisteredAgent that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new RegisteredAgent Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(RegisteredAgent that)
  {
    base.Assign(that);
    identifier = that.identifier;
    phoneNumber = that.phoneNumber;
    areaCode = that.areaCode;
    name = that.name;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    updatedTimestamp = that.updatedTimestamp;
    updatedBy = that.updatedBy;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 9;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier for this agent.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Identifier_MaxLength)]
  public string Identifier
  {
    get => identifier ?? "";
    set => identifier = TrimEnd(Substring(value, 1, Identifier_MaxLength));
  }

  /// <summary>
  /// The json value of the Identifier attribute.</summary>
  [JsonPropertyName("identifier")]
  [Computed]
  public string Identifier_Json
  {
    get => NullIf(Identifier, "");
    set => Identifier = value;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// The phone number at which the registered agent can be contacted.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [Member(Index = 2, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// The area code of the telephone number of the registered agent.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 33;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the registered agent.
  /// </summary>
  [JsonPropertyName("name")]
  [Member(Index = 4, Type = MemberType.Char, Length = Name_MaxLength, Optional
    = true)]
  public string Name
  {
    get => name;
    set => name = value != null
      ? TrimEnd(Substring(value, 1, Name_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
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
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 7, Type = MemberType.Timestamp)]
  public DateTime? UpdatedTimestamp
  {
    get => updatedTimestamp;
    set => updatedTimestamp = value;
  }

  /// <summary>Length of the UPDATED_BY attribute.</summary>
  public const int UpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the UPDATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = UpdatedBy_MaxLength)]
  public string UpdatedBy
  {
    get => updatedBy ?? "";
    set => updatedBy = TrimEnd(Substring(value, 1, UpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the UpdatedBy attribute.</summary>
  [JsonPropertyName("updatedBy")]
  [Computed]
  public string UpdatedBy_Json
  {
    get => NullIf(UpdatedBy, "");
    set => UpdatedBy = value;
  }

  private string identifier;
  private int? phoneNumber;
  private int areaCode;
  private string name;
  private string createdBy;
  private DateTime? createdTimestamp;
  private DateTime? updatedTimestamp;
  private string updatedBy;
}

// The source file: REGISTERED_AGENT_ADDRESS, ID: 371440097, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// Address at which a registered agent is located.
/// </summary>
[Serializable]
public partial class RegisteredAgentAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public RegisteredAgentAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public RegisteredAgentAddress(RegisteredAgentAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new RegisteredAgentAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(RegisteredAgentAddress that)
  {
    base.Assign(that);
    identifier = that.identifier;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zip3 = that.zip3;
    createdTimestamp = that.createdTimestamp;
    updatedTimestamp = that.updatedTimestamp;
    createdBy = that.createdBy;
    updatedBy = that.updatedBy;
    ragId = that.ragId;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 9;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique identifier for this address.
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

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET1 attribute.
  /// First line of postal address for a registered agent.
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 2, Type = MemberType.Char, Length = Street1_MaxLength, Optional
    = true)]
  public string Street1
  {
    get => street1;
    set => street1 = value != null
      ? TrimEnd(Substring(value, 1, Street1_MaxLength)) : null;
  }

  /// <summary>Length of the STREET2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET2 attribute.
  /// Second line of postal address for a registered agent.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 3, Type = MemberType.Char, Length = Street2_MaxLength, Optional
    = true)]
  public string Street2
  {
    get => street2;
    set => street2 = value != null
      ? TrimEnd(Substring(value, 1, Street2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// City in which registered agent is located.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 4, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// State in which the registered agent is located.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 5, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE5 attribute.
  /// Basic postal code for a registered agent's address.
  /// </summary>
  [JsonPropertyName("zipCode5")]
  [Member(Index = 6, Type = MemberType.Char, Length = ZipCode5_MaxLength, Optional
    = true)]
  public string ZipCode5
  {
    get => zipCode5;
    set => zipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE4 attribute.
  /// Zip + 4 postal code for a registered agent's address.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 7, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP3 attribute.</summary>
  public const int Zip3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP3 attribute.
  /// Zip + 3 postal code for a registered agent's address.
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 8, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 9, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the UPDATED_TIMESTAMP attribute.
  /// </summary>
  [JsonPropertyName("updatedTimestamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? UpdatedTimestamp
  {
    get => updatedTimestamp;
    set => updatedTimestamp = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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

  /// <summary>Length of the UPDATED_BY attribute.</summary>
  public const int UpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the UPDATED_BY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = UpdatedBy_MaxLength)]
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

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int RagId_MaxLength = 9;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The unique identifier for this agent.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = RagId_MaxLength)]
  public string RagId
  {
    get => ragId ?? "";
    set => ragId = TrimEnd(Substring(value, 1, RagId_MaxLength));
  }

  /// <summary>
  /// The json value of the RagId attribute.</summary>
  [JsonPropertyName("ragId")]
  [Computed]
  public string RagId_Json
  {
    get => NullIf(RagId, "");
    set => RagId = value;
  }

  private string identifier;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string zip3;
  private DateTime? createdTimestamp;
  private DateTime? updatedTimestamp;
  private string createdBy;
  private string updatedBy;
  private string ragId;
}

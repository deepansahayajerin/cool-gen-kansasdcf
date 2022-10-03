// The source file: CSE_PERSON_VEHICLE, ID: 371433368, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Details of vehicles owned/used by a CSE Person. (This entity has been moved 
/// out from CSE_PERSON_RESOURCE since a vehicle is used for &quot;Locate&quot;
/// as well as &quot;Resource&quot; purposes.
/// </summary>
[Serializable]
public partial class CsePersonVehicle: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonVehicle()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonVehicle(CsePersonVehicle that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonVehicle Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonVehicle that)
  {
    base.Assign(that);
    inactiveInd = that.inactiveInd;
    identifier = that.identifier;
    verifiedUserId = that.verifiedUserId;
    vehicleRegistrationState = that.vehicleRegistrationState;
    vehicleColor = that.vehicleColor;
    vehicleModel = that.vehicleModel;
    vehicleMake = that.vehicleMake;
    vehicleIdentificationNumber = that.vehicleIdentificationNumber;
    vehicleLicenseTag = that.vehicleLicenseTag;
    vehicleYear = that.vehicleYear;
    vehicleOwnedInd = that.vehicleOwnedInd;
    verifiedDate = that.verifiedDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
    cprCResourceNo = that.cprCResourceNo;
    cspCNumber = that.cspCNumber;
  }

  /// <summary>Length of the INACTIVE_IND attribute.</summary>
  public const int InactiveInd_MaxLength = 1;

  /// <summary>
  /// The value of the INACTIVE_IND attribute.
  /// An indicator which specifies whether or not the vehicle usage/ownership is
  /// inactive or not.
  /// </summary>
  [JsonPropertyName("inactiveInd")]
  [Member(Index = 1, Type = MemberType.Char, Length = InactiveInd_MaxLength, Optional
    = true)]
  public string InactiveInd
  {
    get => inactiveInd;
    set => inactiveInd = value != null
      ? TrimEnd(Substring(value, 1, InactiveInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Identifier that indicates a particular CSE Person Vehicle record.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// 6-character alpha-numeric value identifying a KAECSES user who verified 
  /// the given information.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>Length of the VEHICLE_REGISTRATION_STATE attribute.</summary>
  public const int VehicleRegistrationState_MaxLength = 2;

  /// <summary>
  /// The value of the VEHICLE_REGISTRATION_STATE attribute.
  /// The state the vehicle is registered in. The code values and descriptions 
  /// are kept in CODE_VALUE table.
  /// </summary>
  [JsonPropertyName("vehicleRegistrationState")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = VehicleRegistrationState_MaxLength, Optional = true)]
  public string VehicleRegistrationState
  {
    get => vehicleRegistrationState;
    set => vehicleRegistrationState = value != null
      ? TrimEnd(Substring(value, 1, VehicleRegistrationState_MaxLength)) : null
      ;
  }

  /// <summary>Length of the VEHICLE_COLOR attribute.</summary>
  public const int VehicleColor_MaxLength = 10;

  /// <summary>
  /// The value of the VEHICLE_COLOR attribute.
  /// The color of the vehicle.
  /// </summary>
  [JsonPropertyName("vehicleColor")]
  [Member(Index = 5, Type = MemberType.Char, Length = VehicleColor_MaxLength, Optional
    = true)]
  public string VehicleColor
  {
    get => vehicleColor;
    set => vehicleColor = value != null
      ? TrimEnd(Substring(value, 1, VehicleColor_MaxLength)) : null;
  }

  /// <summary>Length of the VEHICLE_MODEL attribute.</summary>
  public const int VehicleModel_MaxLength = 15;

  /// <summary>
  /// The value of the VEHICLE_MODEL attribute.
  /// The model of the vehicle.
  /// </summary>
  [JsonPropertyName("vehicleModel")]
  [Member(Index = 6, Type = MemberType.Char, Length = VehicleModel_MaxLength, Optional
    = true)]
  public string VehicleModel
  {
    get => vehicleModel;
    set => vehicleModel = value != null
      ? TrimEnd(Substring(value, 1, VehicleModel_MaxLength)) : null;
  }

  /// <summary>Length of the VEHICLE_MAKE attribute.</summary>
  public const int VehicleMake_MaxLength = 15;

  /// <summary>
  /// The value of the VEHICLE_MAKE attribute.
  /// The make of the vehicle.
  /// e.g. GM, Ford, Nissan etc.
  /// </summary>
  [JsonPropertyName("vehicleMake")]
  [Member(Index = 7, Type = MemberType.Char, Length = VehicleMake_MaxLength, Optional
    = true)]
  public string VehicleMake
  {
    get => vehicleMake;
    set => vehicleMake = value != null
      ? TrimEnd(Substring(value, 1, VehicleMake_MaxLength)) : null;
  }

  /// <summary>Length of the VEHICLE_IDENTIFICATION_NUMBER attribute.</summary>
  public const int VehicleIdentificationNumber_MaxLength = 30;

  /// <summary>
  /// The value of the VEHICLE_IDENTIFICATION_NUMBER attribute.
  /// A number that identifies a vehicle.
  /// </summary>
  [JsonPropertyName("vehicleIdentificationNumber")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = VehicleIdentificationNumber_MaxLength, Optional = true)]
  public string VehicleIdentificationNumber
  {
    get => vehicleIdentificationNumber;
    set => vehicleIdentificationNumber = value != null
      ? TrimEnd(Substring(value, 1, VehicleIdentificationNumber_MaxLength)) : null
      ;
  }

  /// <summary>Length of the VEHICLE_LICENSE_TAG attribute.</summary>
  public const int VehicleLicenseTag_MaxLength = 10;

  /// <summary>
  /// The value of the VEHICLE_LICENSE_TAG attribute.
  /// A series of numbers that a state identifies a vehicle ownership.
  /// </summary>
  [JsonPropertyName("vehicleLicenseTag")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = VehicleLicenseTag_MaxLength, Optional = true)]
  public string VehicleLicenseTag
  {
    get => vehicleLicenseTag;
    set => vehicleLicenseTag = value != null
      ? TrimEnd(Substring(value, 1, VehicleLicenseTag_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VEHICLE_YEAR attribute.
  /// 4-digit number indicating the year the vehicle was made.
  /// </summary>
  [JsonPropertyName("vehicleYear")]
  [Member(Index = 10, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? VehicleYear
  {
    get => vehicleYear;
    set => vehicleYear = value;
  }

  /// <summary>Length of the VEHICLE_OWNED_IND attribute.</summary>
  public const int VehicleOwnedInd_MaxLength = 1;

  /// <summary>
  /// The value of the VEHICLE_OWNED_IND attribute.
  /// Indicates whether or not the vehicle is owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("vehicleOwnedInd")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = VehicleOwnedInd_MaxLength, Optional = true)]
  public string VehicleOwnedInd
  {
    get => vehicleOwnedInd;
    set => vehicleOwnedInd = value != null
      ? TrimEnd(Substring(value, 1, VehicleOwnedInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// The date that was documented or confirmed about the resource information.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 14, Type = MemberType.Timestamp)]
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
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 16, Type = MemberType.Timestamp)]
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
  [Member(Index = 17, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the RESOURCE_NO attribute.
  /// A running serial number of the resource owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("cprCResourceNo")]
  [Member(Index = 18, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CprCResourceNo
  {
    get => cprCResourceNo;
    set => cprCResourceNo = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspCNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspCNumber")]
  [Member(Index = 19, Type = MemberType.Char, Length = CspCNumber_MaxLength, Optional
    = true)]
  public string CspCNumber
  {
    get => cspCNumber;
    set => cspCNumber = value != null
      ? TrimEnd(Substring(value, 1, CspCNumber_MaxLength)) : null;
  }

  private string inactiveInd;
  private int identifier;
  private string verifiedUserId;
  private string vehicleRegistrationState;
  private string vehicleColor;
  private string vehicleModel;
  private string vehicleMake;
  private string vehicleIdentificationNumber;
  private string vehicleLicenseTag;
  private int? vehicleYear;
  private string vehicleOwnedInd;
  private DateTime? verifiedDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
  private int? cprCResourceNo;
  private string cspCNumber;
}

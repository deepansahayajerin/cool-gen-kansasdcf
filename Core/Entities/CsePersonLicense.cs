﻿// The source file: CSE_PERSON_LICENSE, ID: 371433088, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVINIT
/// This contains data about any kind of license a CSE Person can have.
/// e.g. Driver's, CPA (Accountant), Doctor, Nurses, Lawyers, Beauticians, 
/// Liquor!
/// </summary>
[Serializable]
public partial class CsePersonLicense: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonLicense()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonLicense(CsePersonLicense that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonLicense Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonLicense that)
  {
    base.Assign(that);
    identifier = that.identifier;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    createdTimestamp = that.createdTimestamp;
    createdBy = that.createdBy;
    note = that.note;
    issuingState = that.issuingState;
    issuingAgencyName = that.issuingAgencyName;
    number = that.number;
    description = that.description;
    expirationDt = that.expirationDt;
    startDt = that.startDt;
    type1 = that.type1;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique identifier generated by the system within each CSE Person
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp, Optional = true)]
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
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
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

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 78;

  /// <summary>
  /// The value of the NOTE attribute.
  /// This can be used for any freeform text relating to the license
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 6, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>Length of the ISSUING_STATE attribute.</summary>
  public const int IssuingState_MaxLength = 2;

  /// <summary>
  /// The value of the ISSUING_STATE attribute.
  /// The state the license was issued in
  /// </summary>
  [JsonPropertyName("issuingState")]
  [Member(Index = 7, Type = MemberType.Char, Length = IssuingState_MaxLength, Optional
    = true)]
  public string IssuingState
  {
    get => issuingState;
    set => issuingState = value != null
      ? TrimEnd(Substring(value, 1, IssuingState_MaxLength)) : null;
  }

  /// <summary>Length of the ISSUING_AGENCY_NAME attribute.</summary>
  public const int IssuingAgencyName_MaxLength = 25;

  /// <summary>
  /// The value of the ISSUING_AGENCY_NAME attribute.
  /// the name of the agency that issued the license
  /// </summary>
  [JsonPropertyName("issuingAgencyName")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = IssuingAgencyName_MaxLength, Optional = true)]
  public string IssuingAgencyName
  {
    get => issuingAgencyName;
    set => issuingAgencyName = value != null
      ? TrimEnd(Substring(value, 1, IssuingAgencyName_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int Number_MaxLength = 25;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// The number assigned to the license by the license agency.
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 9, Type = MemberType.Char, Length = Number_MaxLength, Optional
    = true)]
  public string Number
  {
    get => number;
    set => number = value != null
      ? TrimEnd(Substring(value, 1, Number_MaxLength)) : null;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 40;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// describes the type of license held
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 10, Type = MemberType.Char, Length = Description_MaxLength, Optional
    = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? TrimEnd(Substring(value, 1, Description_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EXPIRATION_DT attribute.
  /// The date the license expires(d)
  /// </summary>
  [JsonPropertyName("expirationDt")]
  [Member(Index = 11, Type = MemberType.Date, Optional = true)]
  public DateTime? ExpirationDt
  {
    get => expirationDt;
    set => expirationDt = value;
  }

  /// <summary>
  /// The value of the START_DT attribute.
  /// The date the license was obtained
  /// </summary>
  [JsonPropertyName("startDt")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDt
  {
    get => startDt;
    set => startDt = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of license held eg.
  /// L - Liquor
  /// P - Professional
  /// T - Trade
  /// D - Drivers
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 13, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
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
  [Member(Index = 14, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  private DateTime? lastUpdatedTimestamp;
  private string lastUpdatedBy;
  private DateTime? createdTimestamp;
  private string createdBy;
  private string note;
  private string issuingState;
  private string issuingAgencyName;
  private string number;
  private string description;
  private DateTime? expirationDt;
  private DateTime? startDt;
  private string type1;
  private string cspNumber;
}

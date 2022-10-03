// The source file: CSE_PERSON_RESOURCE, ID: 371433112, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Scope: Financial and tangible resources owned by a CSE_Person.
/// 
/// Includes:  assets (bank accounts, checking, CDs, houses, livestock, stock,
/// bonds, boats, motorcycles, real estate, precious metals, business ownership,
/// inventory).
/// </summary>
[Serializable]
public partial class CsePersonResource: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsePersonResource()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsePersonResource(CsePersonResource that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsePersonResource Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsePersonResource that)
  {
    base.Assign(that);
    locationCounty = that.locationCounty;
    lienHolderStateOfKansasInd = that.lienHolderStateOfKansasInd;
    otherLienHolderName = that.otherLienHolderName;
    otherLienPlacedDate = that.otherLienPlacedDate;
    otherLienRemovedDate = that.otherLienRemovedDate;
    coOwnerName = that.coOwnerName;
    verifiedUserId = that.verifiedUserId;
    resourceDisposalDate = that.resourceDisposalDate;
    verifiedDate = that.verifiedDate;
    lienIndicator = that.lienIndicator;
    resourceNo = that.resourceNo;
    type1 = that.type1;
    accountHolderName = that.accountHolderName;
    accountBalance = that.accountBalance;
    accountNumber = that.accountNumber;
    resourceDescription = that.resourceDescription;
    location = that.location;
    value = that.value;
    equity = that.equity;
    cseActionTakenCode = that.cseActionTakenCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    conONumber = that.conONumber;
    cspONumber = that.cspONumber;
    cspNumber = that.cspNumber;
    exaId = that.exaId;
  }

  /// <summary>Length of the LOCATION_COUNTY attribute.</summary>
  public const int LocationCounty_MaxLength = 15;

  /// <summary>
  /// The value of the LOCATION_COUNTY attribute.
  /// The name of the County where resource is located at. It is not a code.
  /// </summary>
  [JsonPropertyName("locationCounty")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = LocationCounty_MaxLength, Optional = true)]
  public string LocationCounty
  {
    get => locationCounty;
    set => locationCounty = value != null
      ? TrimEnd(Substring(value, 1, LocationCounty_MaxLength)) : null;
  }

  /// <summary>Length of the LIEN_HOLDER_STATE_OF_KANSAS_IND attribute.
  /// </summary>
  public const int LienHolderStateOfKansasInd_MaxLength = 1;

  /// <summary>
  /// The value of the LIEN_HOLDER_STATE_OF_KANSAS_IND attribute.
  /// Whether or not the lien holder is State of Kansas.
  /// </summary>
  [JsonPropertyName("lienHolderStateOfKansasInd")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = LienHolderStateOfKansasInd_MaxLength, Optional = true)]
  public string LienHolderStateOfKansasInd
  {
    get => lienHolderStateOfKansasInd;
    set => lienHolderStateOfKansasInd = value != null
      ? TrimEnd(Substring(value, 1, LienHolderStateOfKansasInd_MaxLength)) : null
      ;
  }

  /// <summary>Length of the OTHER_LIEN_HOLDER_NAME attribute.</summary>
  public const int OtherLienHolderName_MaxLength = 33;

  /// <summary>
  /// The value of the OTHER_LIEN_HOLDER_NAME attribute.
  /// The surname, title, given name and first letter of the middle name of the 
  /// actual lien holder,when the lien is not placed by CSE.
  /// </summary>
  [JsonPropertyName("otherLienHolderName")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = OtherLienHolderName_MaxLength, Optional = true)]
  public string OtherLienHolderName
  {
    get => otherLienHolderName;
    set => otherLienHolderName = value != null
      ? TrimEnd(Substring(value, 1, OtherLienHolderName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the OTHER_LIEN_PLACED_DATE attribute.
  /// The beginning date that a lien is placed on a person's personal or real 
  /// property, for a lien placed by someone other than CSE.
  /// </summary>
  [JsonPropertyName("otherLienPlacedDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? OtherLienPlacedDate
  {
    get => otherLienPlacedDate;
    set => otherLienPlacedDate = value;
  }

  /// <summary>
  /// The value of the OTHER_LIEN_REMOVED_DATE attribute.
  /// The date that a lien is no longer in effect on a person's personal or real
  /// property, for a lien placed by someone other than CSE.
  /// </summary>
  [JsonPropertyName("otherLienRemovedDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? OtherLienRemovedDate
  {
    get => otherLienRemovedDate;
    set => otherLienRemovedDate = value;
  }

  /// <summary>Length of the CO_OWNER_NAME attribute.</summary>
  public const int CoOwnerName_MaxLength = 33;

  /// <summary>
  /// The value of the CO_OWNER_NAME attribute.
  /// Name of the co-owner of the resource.
  /// </summary>
  [JsonPropertyName("coOwnerName")]
  [Member(Index = 6, Type = MemberType.Char, Length = CoOwnerName_MaxLength, Optional
    = true)]
  public string CoOwnerName
  {
    get => coOwnerName;
    set => coOwnerName = value != null
      ? TrimEnd(Substring(value, 1, CoOwnerName_MaxLength)) : null;
  }

  /// <summary>Length of the VERIFIED_USER_ID attribute.</summary>
  public const int VerifiedUserId_MaxLength = 8;

  /// <summary>
  /// The value of the VERIFIED_USER_ID attribute.
  /// User ID of the user who had verified the details.
  /// </summary>
  [JsonPropertyName("verifiedUserId")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = VerifiedUserId_MaxLength, Optional = true)]
  public string VerifiedUserId
  {
    get => verifiedUserId;
    set => verifiedUserId = value != null
      ? TrimEnd(Substring(value, 1, VerifiedUserId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RESOURCE_DISPOSAL_DATE attribute.
  /// Date the resource has been disposed of.
  /// </summary>
  [JsonPropertyName("resourceDisposalDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ResourceDisposalDate
  {
    get => resourceDisposalDate;
    set => resourceDisposalDate = value;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// The date that was documented or confirmed about the resource information.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>Length of the LIEN_INDICATOR attribute.</summary>
  public const int LienIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the LIEN_INDICATOR attribute.
  /// An indicator that states whether or not a lien is held on this resource by
  /// CSE or by others.
  /// </summary>
  [JsonPropertyName("lienIndicator")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = LienIndicator_MaxLength, Optional = true)]
  public string LienIndicator
  {
    get => lienIndicator;
    set => lienIndicator = value != null
      ? TrimEnd(Substring(value, 1, LienIndicator_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the RESOURCE_NO attribute.
  /// A running serial number of the resource owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("resourceNo")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int ResourceNo
  {
    get => resourceNo;
    set => resourceNo = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 2;

  /// <summary>
  /// The value of the TYPE attribute.
  /// Scope: An indicator that specifies the type of resource.
  /// Examples:
  /// 	Bank Account
  /// 	Other assets
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// RESOURCE_TYPE.
  /// </summary>
  [JsonPropertyName("type1")]
  [Member(Index = 12, Type = MemberType.Char, Length = Type1_MaxLength, Optional
    = true)]
  public string Type1
  {
    get => type1;
    set => type1 = value != null
      ? TrimEnd(Substring(value, 1, Type1_MaxLength)) : null;
  }

  /// <summary>Length of the ACCOUNT_HOLDER_NAME attribute.</summary>
  public const int AccountHolderName_MaxLength = 33;

  /// <summary>
  /// The value of the ACCOUNT_HOLDER_NAME attribute.
  /// The name of the person authorized to make transactions on an account.
  /// </summary>
  [JsonPropertyName("accountHolderName")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = AccountHolderName_MaxLength, Optional = true)]
  public string AccountHolderName
  {
    get => accountHolderName;
    set => accountHolderName = value != null
      ? TrimEnd(Substring(value, 1, AccountHolderName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the ACCOUNT_BALANCE attribute.
  /// The total value of an account at a specified time.
  /// </summary>
  [JsonPropertyName("accountBalance")]
  [Member(Index = 14, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? AccountBalance
  {
    get => accountBalance;
    set => accountBalance = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the ACCOUNT_NUMBER attribute.</summary>
  public const int AccountNumber_MaxLength = 20;

  /// <summary>
  /// The value of the ACCOUNT_NUMBER attribute.
  /// A number assigned to an account for identification purposes.
  /// </summary>
  [JsonPropertyName("accountNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = AccountNumber_MaxLength, Optional = true)]
  public string AccountNumber
  {
    get => accountNumber;
    set => accountNumber = value != null
      ? TrimEnd(Substring(value, 1, AccountNumber_MaxLength)) : null;
  }

  /// <summary>Length of the RESOURCE_DESCRIPTION attribute.</summary>
  public const int ResourceDescription_MaxLength = 30;

  /// <summary>
  /// The value of the RESOURCE_DESCRIPTION attribute.
  /// Describes A CSE person's resources.
  /// </summary>
  [JsonPropertyName("resourceDescription")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ResourceDescription_MaxLength, Optional = true)]
  public string ResourceDescription
  {
    get => resourceDescription;
    set => resourceDescription = value != null
      ? TrimEnd(Substring(value, 1, ResourceDescription_MaxLength)) : null;
  }

  /// <summary>Length of the LOCATION attribute.</summary>
  public const int Location_MaxLength = 32;

  /// <summary>
  /// The value of the LOCATION attribute.
  /// The physical location of tangible resources.
  /// </summary>
  [JsonPropertyName("location")]
  [Member(Index = 17, Type = MemberType.Char, Length = Location_MaxLength, Optional
    = true)]
  public string Location
  {
    get => location;
    set => location = value != null
      ? TrimEnd(Substring(value, 1, Location_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VALUE attribute.
  /// The estimated/actual value of a particular resource.  This would be the 
  /// amount indicated, multiplied by the income frequency for the resource. The
  /// balance in cases using bank accounts, current value of an investment or
  /// physical resources (cars, artwork, jewelry etc)
  /// </summary>
  [JsonPropertyName("value")]
  [Member(Index = 18, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? Value
  {
    get => value;
    set => this.value = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the EQUITY attribute.
  /// The amount of the value of the resource that represents the CSE_PERSON's 
  /// interest in the resource.
  /// </summary>
  [JsonPropertyName("equity")]
  [Member(Index = 19, Type = MemberType.Number, Length = 11, Precision = 2, Optional
    = true)]
  public decimal? Equity
  {
    get => equity;
    set => equity = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the CSE_ACTION_TAKEN_CODE attribute.</summary>
  public const int CseActionTakenCode_MaxLength = 1;

  /// <summary>
  /// The value of the CSE_ACTION_TAKEN_CODE attribute.
  /// Scope: An indicator of any CSE involvement with the resource in question.
  /// Examples:
  /// 
  /// L - Lien
  /// A - Attachment
  /// 
  /// G - Garnishment
  /// Permitted values are maintained in CODE_VALUE entity for CODE_NAME 
  /// CSE_ACTION_ON_RESOURCE
  /// </summary>
  [JsonPropertyName("cseActionTakenCode")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = CseActionTakenCode_MaxLength, Optional = true)]
  public string CseActionTakenCode
  {
    get => cseActionTakenCode;
    set => cseActionTakenCode = value != null
      ? TrimEnd(Substring(value, 1, CseActionTakenCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 22, Type = MemberType.Timestamp)]
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
  [Member(Index = 23, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 24, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the CONTACT_NUMBER attribute.
  /// Identifier that indicates a particular CSE contact person.
  /// </summary>
  [JsonPropertyName("conONumber")]
  [Member(Index = 25, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? ConONumber
  {
    get => conONumber;
    set => conONumber = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspONumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspONumber")]
  [Member(Index = 26, Type = MemberType.Char, Length = CspONumber_MaxLength, Optional
    = true)]
  public string CspONumber
  {
    get => cspONumber;
    set => cspONumber = value != null
      ? TrimEnd(Substring(value, 1, CspONumber_MaxLength)) : null;
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
  [Member(Index = 27, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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
  /// The value of the IDENTIFIER attribute.
  /// This attributes uniquely identifies an occurrence of the entity type. It 
  /// is automatically generated by the system.
  /// </summary>
  [JsonPropertyName("exaId")]
  [Member(Index = 28, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? ExaId
  {
    get => exaId;
    set => exaId = value;
  }

  private string locationCounty;
  private string lienHolderStateOfKansasInd;
  private string otherLienHolderName;
  private DateTime? otherLienPlacedDate;
  private DateTime? otherLienRemovedDate;
  private string coOwnerName;
  private string verifiedUserId;
  private DateTime? resourceDisposalDate;
  private DateTime? verifiedDate;
  private string lienIndicator;
  private int resourceNo;
  private string type1;
  private string accountHolderName;
  private decimal? accountBalance;
  private string accountNumber;
  private string resourceDescription;
  private string location;
  private decimal? value;
  private decimal? equity;
  private string cseActionTakenCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? conONumber;
  private string cspONumber;
  private string cspNumber;
  private int? exaId;
}

// The source file: CONV_ALT_BILLING_ADDRESS, ID: 371432706, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINAN
/// This entity contains the alternate billing address converted from KAECSES 
/// systems. The records in this entity will only be temporary. They will be
/// deleted over a period of time manually using the conversion cleanup screen
/// or automatically by LACT screen when an alternate billing location is
/// updated.
/// </summary>
[Serializable]
public partial class ConvAltBillingAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ConvAltBillingAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ConvAltBillingAddress(ConvAltBillingAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ConvAltBillingAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ConvAltBillingAddress that)
  {
    base.Assign(that);
    billingLine1 = that.billingLine1;
    billingLine2 = that.billingLine2;
    billingCity = that.billingCity;
    billingState = that.billingState;
    billingZipCode = that.billingZipCode;
    billingZip4 = that.billingZip4;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    lgaIdentifier = that.lgaIdentifier;
  }

  /// <summary>Length of the BILLING_LINE_1 attribute.</summary>
  public const int BillingLine1_MaxLength = 20;

  /// <summary>
  /// The value of the BILLING_LINE_1 attribute.
  /// This attribute specifies the first line of address ofr the alternate 
  /// billing location. This is loaded from COURT-ORDER-DBF field BILLING LINE
  /// 1.
  /// </summary>
  [JsonPropertyName("billingLine1")]
  [Member(Index = 1, Type = MemberType.Char, Length = BillingLine1_MaxLength, Optional
    = true)]
  public string BillingLine1
  {
    get => billingLine1;
    set => billingLine1 = value != null
      ? TrimEnd(Substring(value, 1, BillingLine1_MaxLength)) : null;
  }

  /// <summary>Length of the BILLING LINE 2 attribute.</summary>
  public const int BillingLine2_MaxLength = 20;

  /// <summary>
  /// The value of the BILLING LINE 2 attribute.
  /// This attribute specifies the SECOND line of address ofr the alternate 
  /// billing location. This is loaded from COURT-ORDER-DBF field BILLING LINE
  /// 2.
  /// </summary>
  [JsonPropertyName("billingLine2")]
  [Member(Index = 2, Type = MemberType.Char, Length = BillingLine2_MaxLength, Optional
    = true)]
  public string BillingLine2
  {
    get => billingLine2;
    set => billingLine2 = value != null
      ? TrimEnd(Substring(value, 1, BillingLine2_MaxLength)) : null;
  }

  /// <summary>Length of the BILLING_CITY attribute.</summary>
  public const int BillingCity_MaxLength = 15;

  /// <summary>
  /// The value of the BILLING_CITY attribute.
  /// This attribute specifies the city of the address for the alternate billing
  /// location. This is loaded from COURT-ORDER-DBF field BILLING CITY.
  /// </summary>
  [JsonPropertyName("billingCity")]
  [Member(Index = 3, Type = MemberType.Char, Length = BillingCity_MaxLength, Optional
    = true)]
  public string BillingCity
  {
    get => billingCity;
    set => billingCity = value != null
      ? TrimEnd(Substring(value, 1, BillingCity_MaxLength)) : null;
  }

  /// <summary>Length of the BILLING STATE attribute.</summary>
  public const int BillingState_MaxLength = 2;

  /// <summary>
  /// The value of the BILLING STATE attribute.
  /// This attribute specifies the STATE of the address for the alternate 
  /// billing location. This is loaded from COURT-ORDER-DBF field BILLING STATE.
  /// </summary>
  [JsonPropertyName("billingState")]
  [Member(Index = 4, Type = MemberType.Char, Length = BillingState_MaxLength, Optional
    = true)]
  public string BillingState
  {
    get => billingState;
    set => billingState = value != null
      ? TrimEnd(Substring(value, 1, BillingState_MaxLength)) : null;
  }

  /// <summary>Length of the BILLING_ZIP_CODE attribute.</summary>
  public const int BillingZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the BILLING_ZIP_CODE attribute.
  /// This attribute specifies the 5-digit ZIP code of the address for the 
  /// alternate billing location. This is loaded brom COURT-ORDER-DBF field
  /// BILLING ZIP CODE.
  /// </summary>
  [JsonPropertyName("billingZipCode")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = BillingZipCode_MaxLength, Optional = true)]
  public string BillingZipCode
  {
    get => billingZipCode;
    set => billingZipCode = value != null
      ? TrimEnd(Substring(value, 1, BillingZipCode_MaxLength)) : null;
  }

  /// <summary>Length of the BILLING_ZIP_4 attribute.</summary>
  public const int BillingZip4_MaxLength = 4;

  /// <summary>
  /// The value of the BILLING_ZIP_4 attribute.
  /// This attribute specifies the 4-digit ZIP code extension of the address for
  /// the alternate billing location. This is loaded from COURT-ORDER-DBF field
  /// BILLING ZIP PLUS FOUR.
  /// </summary>
  [JsonPropertyName("billingZip4")]
  [Member(Index = 6, Type = MemberType.Char, Length = BillingZip4_MaxLength, Optional
    = true)]
  public string BillingZip4
  {
    get => billingZip4;
    set => billingZip4 = value != null
      ? TrimEnd(Substring(value, 1, BillingZip4_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 8, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
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

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 10, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  private string billingLine1;
  private string billingLine2;
  private string billingCity;
  private string billingState;
  private string billingZipCode;
  private string billingZip4;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private int lgaIdentifier;
}

// The source file: CASH_RECEIPT_DETAIL_ADDRESS, ID: 371431683, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// An address related to an occurrance of a cash receipt detail.  There may be 
/// multiple cash receipts associated with each address.
/// </summary>
[Serializable]
public partial class CashReceiptDetailAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CashReceiptDetailAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CashReceiptDetailAddress(CashReceiptDetailAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CashReceiptDetailAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CashReceiptDetailAddress that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zipCode3 = that.zipCode3;
    crdIdentifier = that.crdIdentifier;
    crvIdentifier = that.crvIdentifier;
    cstIdentifier = that.cstIdentifier;
    crtIdentifier = that.crtIdentifier;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that distinguishes a occurrance of an entity 
  /// from all other occurrances.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [Member(Index = 1, Type = MemberType.Timestamp)]
  public DateTime? SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of the address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Street1_MaxLength)]
  public string Street1
  {
    get => street1 ?? "";
    set => street1 = TrimEnd(Substring(value, 1, Street1_MaxLength));
  }

  /// <summary>
  /// The json value of the Street1 attribute.</summary>
  [JsonPropertyName("street1")]
  [Computed]
  public string Street1_Json
  {
    get => NullIf(Street1, "");
    set => Street1 = value;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The second line of the address.
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
  public const int City_MaxLength = 30;

  /// <summary>
  /// The value of the CITY attribute.
  /// The city part of the address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = City_MaxLength)]
  public string City
  {
    get => city ?? "";
    set => city = TrimEnd(Substring(value, 1, City_MaxLength));
  }

  /// <summary>
  /// The json value of the City attribute.</summary>
  [JsonPropertyName("city")]
  [Computed]
  public string City_Json
  {
    get => NullIf(City, "");
    set => City = value;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The state part of the address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = State_MaxLength)]
  public string State
  {
    get => state ?? "";
    set => state = TrimEnd(Substring(value, 1, State_MaxLength));
  }

  /// <summary>
  /// The json value of the State attribute.</summary>
  [JsonPropertyName("state")]
  [Computed]
  public string State_Json
  {
    get => NullIf(State, "");
    set => State = value;
  }

  /// <summary>Length of the ZIP_CODE_5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE_5 attribute.
  /// The first five digits of the zip code.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ZipCode5_MaxLength)]
  public string ZipCode5
  {
    get => zipCode5 ?? "";
    set => zipCode5 = TrimEnd(Substring(value, 1, ZipCode5_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode5 attribute.</summary>
  [JsonPropertyName("zipCode5")]
  [Computed]
  public string ZipCode5_Json
  {
    get => NullIf(ZipCode5, "");
    set => ZipCode5 = value;
  }

  /// <summary>Length of the ZIP_CODE_4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE_4 attribute.
  /// The second part of the zip code.  It consists of four digits.
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

  /// <summary>Length of the ZIP_CODE_3 attribute.</summary>
  public const int ZipCode3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP_CODE_3 attribute.
  /// The third part of zip code.  It consists of a two digit house number and a
  /// one digit check code number.
  /// </summary>
  [JsonPropertyName("zipCode3")]
  [Member(Index = 8, Type = MemberType.Char, Length = ZipCode3_MaxLength, Optional
    = true)]
  public string ZipCode3
  {
    get => zipCode3;
    set => zipCode3 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode3_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SEQUENTIAL_IDENTIFIER attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crdIdentifier")]
  [Member(Index = 9, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? CrdIdentifier
  {
    get => crdIdentifier;
    set => crdIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("crvIdentifier")]
  [Member(Index = 10, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CrvIdentifier
  {
    get => crvIdentifier;
    set => crvIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("cstIdentifier")]
  [Member(Index = 11, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CstIdentifier
  {
    get => cstIdentifier;
    set => cstIdentifier = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("crtIdentifier")]
  [Member(Index = 12, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? CrtIdentifier
  {
    get => crtIdentifier;
    set => crtIdentifier = value;
  }

  private DateTime? systemGeneratedIdentifier;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zipCode5;
  private string zipCode4;
  private string zipCode3;
  private int? crdIdentifier;
  private int? crvIdentifier;
  private int? cstIdentifier;
  private int? crtIdentifier;
}

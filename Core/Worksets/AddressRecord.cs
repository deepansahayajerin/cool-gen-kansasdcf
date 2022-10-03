// The source file: ADDRESS_RECORD, ID: 374396708, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class AddressRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AddressRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AddressRecord(AddressRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AddressRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AddressRecord that)
  {
    base.Assign(that);
    recordType = that.recordType;
    street = that.street;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    postalCode = that.postalCode;
    country = that.country;
    phoneNumber = that.phoneNumber;
    province = that.province;
    source = that.source;
    type1 = that.type1;
    updatedBy = that.updatedBy;
    filler = that.filler;
  }

  /// <summary>Length of the RECORD_TYPE attribute.</summary>
  public const int RecordType_MaxLength = 1;

  /// <summary>
  /// The value of the RECORD_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordType_MaxLength)]
  public string RecordType
  {
    get => recordType ?? "";
    set => recordType = TrimEnd(Substring(value, 1, RecordType_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordType attribute.</summary>
  [JsonPropertyName("recordType")]
  [Computed]
  public string RecordType_Json
  {
    get => NullIf(RecordType, "");
    set => RecordType = value;
  }

  /// <summary>Length of the STREET attribute.</summary>
  public const int Street_MaxLength = 30;

  /// <summary>
  /// The value of the STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Street_MaxLength)]
  public string Street
  {
    get => street ?? "";
    set => street = TrimEnd(Substring(value, 1, Street_MaxLength));
  }

  /// <summary>
  /// The json value of the Street attribute.</summary>
  [JsonPropertyName("street")]
  [Computed]
  public string Street_Json
  {
    get => NullIf(Street, "");
    set => Street = value;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 30;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Street2_MaxLength)]
  public string Street2
  {
    get => street2 ?? "";
    set => street2 = TrimEnd(Substring(value, 1, Street2_MaxLength));
  }

  /// <summary>
  /// The json value of the Street2 attribute.</summary>
  [JsonPropertyName("street2")]
  [Computed]
  public string Street2_Json
  {
    get => NullIf(Street2, "");
    set => Street2 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 20;

  /// <summary>
  /// The value of the CITY attribute.
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

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = PostalCode_MaxLength)]
  public string PostalCode
  {
    get => postalCode ?? "";
    set => postalCode = TrimEnd(Substring(value, 1, PostalCode_MaxLength));
  }

  /// <summary>
  /// The json value of the PostalCode attribute.</summary>
  [JsonPropertyName("postalCode")]
  [Computed]
  public string PostalCode_Json
  {
    get => NullIf(PostalCode, "");
    set => PostalCode = value;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 20;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Country_MaxLength)]
  public string Country
  {
    get => country ?? "";
    set => country = TrimEnd(Substring(value, 1, Country_MaxLength));
  }

  /// <summary>
  /// The json value of the Country attribute.</summary>
  [JsonPropertyName("country")]
  [Computed]
  public string Country_Json
  {
    get => NullIf(Country, "");
    set => Country = value;
  }

  /// <summary>Length of the PHONE_NUMBER attribute.</summary>
  public const int PhoneNumber_MaxLength = 10;

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = PhoneNumber_MaxLength)]
  public string PhoneNumber
  {
    get => phoneNumber ?? "";
    set => phoneNumber = TrimEnd(Substring(value, 1, PhoneNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PhoneNumber attribute.</summary>
  [JsonPropertyName("phoneNumber")]
  [Computed]
  public string PhoneNumber_Json
  {
    get => NullIf(PhoneNumber, "");
    set => PhoneNumber = value;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Province_MaxLength)]
  public string Province
  {
    get => province ?? "";
    set => province = TrimEnd(Substring(value, 1, Province_MaxLength));
  }

  /// <summary>
  /// The json value of the Province attribute.</summary>
  [JsonPropertyName("province")]
  [Computed]
  public string Province_Json
  {
    get => NullIf(Province, "");
    set => Province = value;
  }

  /// <summary>Length of the SOURCE attribute.</summary>
  public const int Source_MaxLength = 4;

  /// <summary>
  /// The value of the SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Source_MaxLength)]
  public string Source
  {
    get => source ?? "";
    set => source = TrimEnd(Substring(value, 1, Source_MaxLength));
  }

  /// <summary>
  /// The json value of the Source attribute.</summary>
  [JsonPropertyName("source")]
  [Computed]
  public string Source_Json
  {
    get => NullIf(Source, "");
    set => Source = value;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
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

  /// <summary>Length of the FILLER attribute.</summary>
  public const int Filler_MaxLength = 54;

  /// <summary>
  /// The value of the FILLER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = Filler_MaxLength)]
  public string Filler
  {
    get => filler ?? "";
    set => filler = TrimEnd(Substring(value, 1, Filler_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler attribute.</summary>
  [JsonPropertyName("filler")]
  [Computed]
  public string Filler_Json
  {
    get => NullIf(Filler, "");
    set => Filler = value;
  }

  private string recordType;
  private string street;
  private string street2;
  private string city;
  private string state;
  private string postalCode;
  private string country;
  private string phoneNumber;
  private string province;
  private string source;
  private string type1;
  private string updatedBy;
  private string filler;
}

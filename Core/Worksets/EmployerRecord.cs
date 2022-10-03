// The source file: EMPLOYER_RECORD, ID: 374401529, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EmployerRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EmployerRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EmployerRecord(EmployerRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EmployerRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EmployerRecord that)
  {
    base.Assign(that);
    name = that.name;
    street1 = that.street1;
    street2 = that.street2;
    street3 = that.street3;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    phoneNumber = that.phoneNumber;
    extension = that.extension;
    ein = that.ein;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 50;

  /// <summary>
  /// The value of the NAME attribute.
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

  /// <summary>Length of the STREET1 attribute.</summary>
  public const int Street1_MaxLength = 50;

  /// <summary>
  /// The value of the STREET1 attribute.
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

  /// <summary>Length of the STREET2 attribute.</summary>
  public const int Street2_MaxLength = 50;

  /// <summary>
  /// The value of the STREET2 attribute.
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

  /// <summary>Length of the STREET3 attribute.</summary>
  public const int Street3_MaxLength = 50;

  /// <summary>
  /// The value of the STREET3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Street3_MaxLength)]
  public string Street3
  {
    get => street3 ?? "";
    set => street3 = TrimEnd(Substring(value, 1, Street3_MaxLength));
  }

  /// <summary>
  /// The json value of the Street3 attribute.</summary>
  [JsonPropertyName("street3")]
  [Computed]
  public string Street3_Json
  {
    get => NullIf(Street3, "");
    set => Street3 = value;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 20;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = City_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Char, Length = State_MaxLength)]
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

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ZipCode_MaxLength)]
  public string ZipCode
  {
    get => zipCode ?? "";
    set => zipCode = TrimEnd(Substring(value, 1, ZipCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ZipCode attribute.</summary>
  [JsonPropertyName("zipCode")]
  [Computed]
  public string ZipCode_Json
  {
    get => NullIf(ZipCode, "");
    set => ZipCode = value;
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

  /// <summary>Length of the EXTENSION attribute.</summary>
  public const int Extension_MaxLength = 5;

  /// <summary>
  /// The value of the EXTENSION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Extension_MaxLength)]
  public string Extension
  {
    get => extension ?? "";
    set => extension = TrimEnd(Substring(value, 1, Extension_MaxLength));
  }

  /// <summary>
  /// The json value of the Extension attribute.</summary>
  [JsonPropertyName("extension")]
  [Computed]
  public string Extension_Json
  {
    get => NullIf(Extension, "");
    set => Extension = value;
  }

  /// <summary>Length of the EIN attribute.</summary>
  public const int Ein_MaxLength = 10;

  /// <summary>
  /// The value of the EIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Ein_MaxLength)]
  public string Ein
  {
    get => ein ?? "";
    set => ein = TrimEnd(Substring(value, 1, Ein_MaxLength));
  }

  /// <summary>
  /// The json value of the Ein attribute.</summary>
  [JsonPropertyName("ein")]
  [Computed]
  public string Ein_Json
  {
    get => NullIf(Ein, "");
    set => Ein = value;
  }

  private string name;
  private string street1;
  private string street2;
  private string street3;
  private string city;
  private string state;
  private string zipCode;
  private string phoneNumber;
  private string extension;
  private string ein;
}

// The source file: WORK_ADDRESS, ID: 371872398, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class WorkAddress: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkAddress()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkAddress(WorkAddress that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkAddress Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WorkAddress that)
  {
    base.Assign(that);
    cityStZip = that.cityStZip;
    addressLine1 = that.addressLine1;
    addressLine2 = that.addressLine2;
    city = that.city;
    state = that.state;
    zipCode = that.zipCode;
    name = that.name;
  }

  /// <summary>Length of the CITY_ST_ZIP attribute.</summary>
  public const int CityStZip_MaxLength = 44;

  /// <summary>
  /// The value of the CITY_ST_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CityStZip_MaxLength)]
  public string CityStZip
  {
    get => cityStZip ?? "";
    set => cityStZip = TrimEnd(Substring(value, 1, CityStZip_MaxLength));
  }

  /// <summary>
  /// The json value of the CityStZip attribute.</summary>
  [JsonPropertyName("cityStZip")]
  [Computed]
  public string CityStZip_Json
  {
    get => NullIf(CityStZip, "");
    set => CityStZip = value;
  }

  /// <summary>Length of the ADDRESS_LINE_1 attribute.</summary>
  public const int AddressLine1_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AddressLine1_MaxLength)]
  public string AddressLine1
  {
    get => addressLine1 ?? "";
    set => addressLine1 = TrimEnd(Substring(value, 1, AddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine1 attribute.</summary>
  [JsonPropertyName("addressLine1")]
  [Computed]
  public string AddressLine1_Json
  {
    get => NullIf(AddressLine1, "");
    set => AddressLine1 = value;
  }

  /// <summary>Length of the ADDRESS_LINE_2 attribute.</summary>
  public const int AddressLine2_MaxLength = 20;

  /// <summary>
  /// The value of the ADDRESS_LINE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = AddressLine2_MaxLength)]
  public string AddressLine2
  {
    get => addressLine2 ?? "";
    set => addressLine2 = TrimEnd(Substring(value, 1, AddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressLine2 attribute.</summary>
  [JsonPropertyName("addressLine2")]
  [Computed]
  public string AddressLine2_Json
  {
    get => NullIf(AddressLine2, "");
    set => AddressLine2 = value;
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

  /// <summary>Length of the ZIP_CODE attribute.</summary>
  public const int ZipCode_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ZipCode_MaxLength)]
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

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 30;

  /// <summary>
  /// The value of the NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Name_MaxLength)]
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

  private string cityStZip;
  private string addressLine1;
  private string addressLine2;
  private string city;
  private string state;
  private string zipCode;
  private string name;
}

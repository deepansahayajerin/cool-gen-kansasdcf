// The source file: LE_AUTO_FIPS_UPDATE, ID: 374360450, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class LeAutoFipsUpdate: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public LeAutoFipsUpdate()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public LeAutoFipsUpdate(LeAutoFipsUpdate that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new LeAutoFipsUpdate Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(LeAutoFipsUpdate that)
  {
    base.Assign(that);
    addressType1 = that.addressType1;
    addressType2 = that.addressType2;
    stateCode = that.stateCode;
    localCode = that.localCode;
    subLocalCode = that.subLocalCode;
    departmentName = that.departmentName;
    title = that.title;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    stateOrCountry = that.stateOrCountry;
    zipCode = that.zipCode;
    areaCode = that.areaCode;
    phoneNumber = that.phoneNumber;
    extension = that.extension;
    actionCode = that.actionCode;
    faxAreaCode = that.faxAreaCode;
    faxNumber = that.faxNumber;
    recordDate = that.recordDate;
  }

  /// <summary>Length of the ADDRESS_TYPE_1 attribute.</summary>
  public const int AddressType1_MaxLength = 3;

  /// <summary>
  /// The value of the ADDRESS_TYPE_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AddressType1_MaxLength)]
  public string AddressType1
  {
    get => addressType1 ?? "";
    set => addressType1 = TrimEnd(Substring(value, 1, AddressType1_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressType1 attribute.</summary>
  [JsonPropertyName("addressType1")]
  [Computed]
  public string AddressType1_Json
  {
    get => NullIf(AddressType1, "");
    set => AddressType1 = value;
  }

  /// <summary>Length of the ADDRESS_TYPE_2 attribute.</summary>
  public const int AddressType2_MaxLength = 3;

  /// <summary>
  /// The value of the ADDRESS_TYPE_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AddressType2_MaxLength)]
  public string AddressType2
  {
    get => addressType2 ?? "";
    set => addressType2 = TrimEnd(Substring(value, 1, AddressType2_MaxLength));
  }

  /// <summary>
  /// The json value of the AddressType2 attribute.</summary>
  [JsonPropertyName("addressType2")]
  [Computed]
  public string AddressType2_Json
  {
    get => NullIf(AddressType2, "");
    set => AddressType2 = value;
  }

  /// <summary>
  /// The value of the STATE_CODE attribute.
  /// </summary>
  [JsonPropertyName("stateCode")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 2)]
  public int StateCode
  {
    get => stateCode;
    set => stateCode = value;
  }

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// </summary>
  [JsonPropertyName("localCode")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 3)]
  public int LocalCode
  {
    get => localCode;
    set => localCode = value;
  }

  /// <summary>
  /// The value of the SUB_LOCAL_CODE attribute.
  /// </summary>
  [JsonPropertyName("subLocalCode")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 2)]
  public int SubLocalCode
  {
    get => subLocalCode;
    set => subLocalCode = value;
  }

  /// <summary>Length of the DEPARTMENT_NAME attribute.</summary>
  public const int DepartmentName_MaxLength = 35;

  /// <summary>
  /// The value of the DEPARTMENT_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = DepartmentName_MaxLength)]
  public string DepartmentName
  {
    get => departmentName ?? "";
    set => departmentName =
      TrimEnd(Substring(value, 1, DepartmentName_MaxLength));
  }

  /// <summary>
  /// The json value of the DepartmentName attribute.</summary>
  [JsonPropertyName("departmentName")]
  [Computed]
  public string DepartmentName_Json
  {
    get => NullIf(DepartmentName, "");
    set => DepartmentName = value;
  }

  /// <summary>Length of the TITLE attribute.</summary>
  public const int Title_MaxLength = 35;

  /// <summary>
  /// The value of the TITLE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Title_MaxLength)]
  public string Title
  {
    get => title ?? "";
    set => title = TrimEnd(Substring(value, 1, Title_MaxLength));
  }

  /// <summary>
  /// The json value of the Title attribute.</summary>
  [JsonPropertyName("title")]
  [Computed]
  public string Title_Json
  {
    get => NullIf(Title, "");
    set => Title = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 35;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Street1_MaxLength)]
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
  public const int Street2_MaxLength = 35;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Street2_MaxLength)]
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
  public const int City_MaxLength = 30;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = City_MaxLength)]
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

  /// <summary>Length of the STATE_OR_COUNTRY attribute.</summary>
  public const int StateOrCountry_MaxLength = 20;

  /// <summary>
  /// The value of the STATE_OR_COUNTRY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = StateOrCountry_MaxLength)]
    
  public string StateOrCountry
  {
    get => stateOrCountry ?? "";
    set => stateOrCountry =
      TrimEnd(Substring(value, 1, StateOrCountry_MaxLength));
  }

  /// <summary>
  /// The json value of the StateOrCountry attribute.</summary>
  [JsonPropertyName("stateOrCountry")]
  [Computed]
  public string StateOrCountry_Json
  {
    get => NullIf(StateOrCountry, "");
    set => StateOrCountry = value;
  }

  /// <summary>
  /// The value of the ZIP_CODE attribute.
  /// </summary>
  [JsonPropertyName("zipCode")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 9)]
  public int ZipCode
  {
    get => zipCode;
    set => zipCode = value;
  }

  /// <summary>
  /// The value of the AREA_CODE attribute.
  /// </summary>
  [JsonPropertyName("areaCode")]
  [DefaultValue(0)]
  [Member(Index = 13, Type = MemberType.Number, Length = 3)]
  public int AreaCode
  {
    get => areaCode;
    set => areaCode = value;
  }

  /// <summary>
  /// The value of the PHONE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("phoneNumber")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 7)]
  public int PhoneNumber
  {
    get => phoneNumber;
    set => phoneNumber = value;
  }

  /// <summary>
  /// The value of the EXTENSION attribute.
  /// </summary>
  [JsonPropertyName("extension")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 5)]
  public int Extension
  {
    get => extension;
    set => extension = value;
  }

  /// <summary>Length of the ACTION_CODE attribute.</summary>
  public const int ActionCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = ActionCode_MaxLength)]
  public string ActionCode
  {
    get => actionCode ?? "";
    set => actionCode = TrimEnd(Substring(value, 1, ActionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionCode attribute.</summary>
  [JsonPropertyName("actionCode")]
  [Computed]
  public string ActionCode_Json
  {
    get => NullIf(ActionCode, "");
    set => ActionCode = value;
  }

  /// <summary>
  /// The value of the FAX_AREA_CODE attribute.
  /// </summary>
  [JsonPropertyName("faxAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 3)]
  public int FaxAreaCode
  {
    get => faxAreaCode;
    set => faxAreaCode = value;
  }

  /// <summary>
  /// The value of the FAX_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("faxNumber")]
  [DefaultValue(0)]
  [Member(Index = 18, Type = MemberType.Number, Length = 7)]
  public int FaxNumber
  {
    get => faxNumber;
    set => faxNumber = value;
  }

  /// <summary>
  /// The value of the RECORD_DATE attribute.
  /// </summary>
  [JsonPropertyName("recordDate")]
  [DefaultValue(0)]
  [Member(Index = 19, Type = MemberType.Number, Length = 8)]
  public int RecordDate
  {
    get => recordDate;
    set => recordDate = value;
  }

  private string addressType1;
  private string addressType2;
  private int stateCode;
  private int localCode;
  private int subLocalCode;
  private string departmentName;
  private string title;
  private string street1;
  private string street2;
  private string city;
  private string stateOrCountry;
  private int zipCode;
  private int areaCode;
  private int phoneNumber;
  private int extension;
  private string actionCode;
  private int faxAreaCode;
  private int faxNumber;
  private int recordDate;
}

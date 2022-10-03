// The source file: QUICK_WORKER_CONTACT_INFO, ID: 374543798, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This workset view contains all of the QUICK worker contact attributes.
/// </summary>
[Serializable]
public partial class QuickWorkerContactInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public QuickWorkerContactInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public QuickWorkerContactInfo(QuickWorkerContactInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new QuickWorkerContactInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(QuickWorkerContactInfo that)
  {
    base.Assign(that);
    contactTypeCode = that.contactTypeCode;
    effectiveDate = that.effectiveDate;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    lastName = that.lastName;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    stateProvince = that.stateProvince;
    postalCode = that.postalCode;
    zip = that.zip;
    zip4 = that.zip4;
    workPhoneNumber = that.workPhoneNumber;
    workPhoneAreaCode = that.workPhoneAreaCode;
    workPhoneExtension = that.workPhoneExtension;
    emailAddress = that.emailAddress;
    name = that.name;
    mainFaxAreaCode = that.mainFaxAreaCode;
    mainFaxPhoneNumber = that.mainFaxPhoneNumber;
  }

  /// <summary>Length of the CONTACT_TYPE_CODE attribute.</summary>
  public const int ContactTypeCode_MaxLength = 10;

  /// <summary>
  /// The value of the CONTACT_TYPE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ContactTypeCode_MaxLength)]
    
  public string ContactTypeCode
  {
    get => contactTypeCode ?? "";
    set => contactTypeCode =
      TrimEnd(Substring(value, 1, ContactTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ContactTypeCode attribute.</summary>
  [JsonPropertyName("contactTypeCode")]
  [Computed]
  public string ContactTypeCode_Json
  {
    get => NullIf(ContactTypeCode, "");
    set => ContactTypeCode = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
  public string MiddleInitial
  {
    get => middleInitial ?? "";
    set => middleInitial =
      TrimEnd(Substring(value, 1, MiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleInitial attribute.</summary>
  [JsonPropertyName("middleInitial")]
  [Computed]
  public string MiddleInitial_Json
  {
    get => NullIf(MiddleInitial, "");
    set => MiddleInitial = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Street1_MaxLength)]
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
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Street2_MaxLength)]
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
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = City_MaxLength)]
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

  /// <summary>Length of the STATE_PROVINCE attribute.</summary>
  public const int StateProvince_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_PROVINCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = StateProvince_MaxLength)]
  public string StateProvince
  {
    get => stateProvince ?? "";
    set => stateProvince =
      TrimEnd(Substring(value, 1, StateProvince_MaxLength));
  }

  /// <summary>
  /// The json value of the StateProvince attribute.</summary>
  [JsonPropertyName("stateProvince")]
  [Computed]
  public string StateProvince_Json
  {
    get => NullIf(StateProvince, "");
    set => StateProvince = value;
  }

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = PostalCode_MaxLength)]
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

  /// <summary>Length of the ZIP attribute.</summary>
  public const int Zip_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = Zip_MaxLength)]
  public string Zip
  {
    get => zip ?? "";
    set => zip = TrimEnd(Substring(value, 1, Zip_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip attribute.</summary>
  [JsonPropertyName("zip")]
  [Computed]
  public string Zip_Json
  {
    get => NullIf(Zip, "");
    set => Zip = value;
  }

  /// <summary>Length of the ZIP4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Zip4_MaxLength)]
  public string Zip4
  {
    get => zip4 ?? "";
    set => zip4 = TrimEnd(Substring(value, 1, Zip4_MaxLength));
  }

  /// <summary>
  /// The json value of the Zip4 attribute.</summary>
  [JsonPropertyName("zip4")]
  [Computed]
  public string Zip4_Json
  {
    get => NullIf(Zip4, "");
    set => Zip4 = value;
  }

  /// <summary>Length of the WORK_PHONE_NUMBER attribute.</summary>
  public const int WorkPhoneNumber_MaxLength = 7;

  /// <summary>
  /// The value of the WORK_PHONE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = WorkPhoneNumber_MaxLength)
    ]
  public string WorkPhoneNumber
  {
    get => workPhoneNumber ?? "";
    set => workPhoneNumber =
      TrimEnd(Substring(value, 1, WorkPhoneNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the WorkPhoneNumber attribute.</summary>
  [JsonPropertyName("workPhoneNumber")]
  [Computed]
  public string WorkPhoneNumber_Json
  {
    get => NullIf(WorkPhoneNumber, "");
    set => WorkPhoneNumber = value;
  }

  /// <summary>Length of the WORK_PHONE_AREA_CODE attribute.</summary>
  public const int WorkPhoneAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the WORK_PHONE_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = WorkPhoneAreaCode_MaxLength)]
  public string WorkPhoneAreaCode
  {
    get => workPhoneAreaCode ?? "";
    set => workPhoneAreaCode =
      TrimEnd(Substring(value, 1, WorkPhoneAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the WorkPhoneAreaCode attribute.</summary>
  [JsonPropertyName("workPhoneAreaCode")]
  [Computed]
  public string WorkPhoneAreaCode_Json
  {
    get => NullIf(WorkPhoneAreaCode, "");
    set => WorkPhoneAreaCode = value;
  }

  /// <summary>Length of the WORK_PHONE_EXTENSION attribute.</summary>
  public const int WorkPhoneExtension_MaxLength = 5;

  /// <summary>
  /// The value of the WORK_PHONE_EXTENSION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = WorkPhoneExtension_MaxLength)]
  public string WorkPhoneExtension
  {
    get => workPhoneExtension ?? "";
    set => workPhoneExtension =
      TrimEnd(Substring(value, 1, WorkPhoneExtension_MaxLength));
  }

  /// <summary>
  /// The json value of the WorkPhoneExtension attribute.</summary>
  [JsonPropertyName("workPhoneExtension")]
  [Computed]
  public string WorkPhoneExtension_Json
  {
    get => NullIf(WorkPhoneExtension, "");
    set => WorkPhoneExtension = value;
  }

  /// <summary>Length of the EMAIL_ADDRESS attribute.</summary>
  public const int EmailAddress_MaxLength = 50;

  /// <summary>
  /// The value of the EMAIL_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = EmailAddress_MaxLength)]
  public string EmailAddress
  {
    get => emailAddress ?? "";
    set => emailAddress = TrimEnd(Substring(value, 1, EmailAddress_MaxLength));
  }

  /// <summary>
  /// The json value of the EmailAddress attribute.</summary>
  [JsonPropertyName("emailAddress")]
  [Computed]
  public string EmailAddress_Json
  {
    get => NullIf(EmailAddress, "");
    set => EmailAddress = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 30;

  /// <summary>
  /// The value of the NAME attribute.
  /// Office name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = Name_MaxLength)]
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

  /// <summary>Length of the MAIN_FAX_AREA_CODE attribute.</summary>
  public const int MainFaxAreaCode_MaxLength = 3;

  /// <summary>
  /// The value of the MAIN_FAX_AREA_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = MainFaxAreaCode_MaxLength)
    ]
  public string MainFaxAreaCode
  {
    get => mainFaxAreaCode ?? "";
    set => mainFaxAreaCode =
      TrimEnd(Substring(value, 1, MainFaxAreaCode_MaxLength));
  }

  /// <summary>
  /// The json value of the MainFaxAreaCode attribute.</summary>
  [JsonPropertyName("mainFaxAreaCode")]
  [Computed]
  public string MainFaxAreaCode_Json
  {
    get => NullIf(MainFaxAreaCode, "");
    set => MainFaxAreaCode = value;
  }

  /// <summary>Length of the MAIN_FAX_PHONE_NUMBER attribute.</summary>
  public const int MainFaxPhoneNumber_MaxLength = 7;

  /// <summary>
  /// The value of the MAIN_FAX_PHONE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = MainFaxPhoneNumber_MaxLength)]
  public string MainFaxPhoneNumber
  {
    get => mainFaxPhoneNumber ?? "";
    set => mainFaxPhoneNumber =
      TrimEnd(Substring(value, 1, MainFaxPhoneNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the MainFaxPhoneNumber attribute.</summary>
  [JsonPropertyName("mainFaxPhoneNumber")]
  [Computed]
  public string MainFaxPhoneNumber_Json
  {
    get => NullIf(MainFaxPhoneNumber, "");
    set => MainFaxPhoneNumber = value;
  }

  private string contactTypeCode;
  private DateTime? effectiveDate;
  private string firstName;
  private string middleInitial;
  private string lastName;
  private string street1;
  private string street2;
  private string city;
  private string stateProvince;
  private string postalCode;
  private string zip;
  private string zip4;
  private string workPhoneNumber;
  private string workPhoneAreaCode;
  private string workPhoneExtension;
  private string emailAddress;
  private string name;
  private string mainFaxAreaCode;
  private string mainFaxPhoneNumber;
}

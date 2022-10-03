// The source file: MILITARY_SERVICE, ID: 371437230, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// Record of military history including branch, rank, commanding officer, and 
/// current status.
/// </summary>
[Serializable]
public partial class MilitaryService: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public MilitaryService()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public MilitaryService(MilitaryService that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new MilitaryService Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(MilitaryService that)
  {
    base.Assign(that);
    phoneAreaCode = that.phoneAreaCode;
    phoneCountryCode = that.phoneCountryCode;
    phoneExt = that.phoneExt;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    province = that.province;
    postalCode = that.postalCode;
    zipCode5 = that.zipCode5;
    zipCode4 = that.zipCode4;
    zip3 = that.zip3;
    country = that.country;
    apo = that.apo;
    expectedReturnDateToStates = that.expectedReturnDateToStates;
    overseasDutyStation = that.overseasDutyStation;
    expectedDischargeDate = that.expectedDischargeDate;
    phone = that.phone;
    branchCode = that.branchCode;
    commandingOfficerLastName = that.commandingOfficerLastName;
    commandingOfficerFirstName = that.commandingOfficerFirstName;
    commandingOfficerMi = that.commandingOfficerMi;
    currentUsDutyStation = that.currentUsDutyStation;
    dutyStatusCode = that.dutyStatusCode;
    rank = that.rank;
    endDate = that.endDate;
    startDate = that.startDate;
    effectiveDate = that.effectiveDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the PHONE_AREA_CODE attribute.
  /// The 3-digit area code for the phone number at the military service.
  /// </summary>
  [JsonPropertyName("phoneAreaCode")]
  [Member(Index = 1, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PhoneAreaCode
  {
    get => phoneAreaCode;
    set => phoneAreaCode = value;
  }

  /// <summary>Length of the PHONE_COUNTRY_CODE attribute.</summary>
  public const int PhoneCountryCode_MaxLength = 4;

  /// <summary>
  /// The value of the PHONE_COUNTRY_CODE attribute.
  /// The 4 digit foreign code consisting of 3 digit country code and 1 digit 
  /// city code for the overseas duty station phone number.
  /// </summary>
  [JsonPropertyName("phoneCountryCode")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = PhoneCountryCode_MaxLength, Optional = true)]
  public string PhoneCountryCode
  {
    get => phoneCountryCode;
    set => phoneCountryCode = value != null
      ? TrimEnd(Substring(value, 1, PhoneCountryCode_MaxLength)) : null;
  }

  /// <summary>Length of the PHONE_EXT attribute.</summary>
  public const int PhoneExt_MaxLength = 5;

  /// <summary>
  /// The value of the PHONE_EXT attribute.
  /// The 5 digit phone extension of the duty station.
  /// </summary>
  [JsonPropertyName("phoneExt")]
  [Member(Index = 3, Type = MemberType.Char, Length = PhoneExt_MaxLength, Optional
    = true)]
  public string PhoneExt
  {
    get => phoneExt;
    set => phoneExt = value != null
      ? TrimEnd(Substring(value, 1, PhoneExt_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// the first line of postal address of military service duty station.
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 4, Type = MemberType.Char, Length = Street1_MaxLength, Optional
    = true)]
  public string Street1
  {
    get => street1;
    set => street1 = value != null
      ? TrimEnd(Substring(value, 1, Street1_MaxLength)) : null;
  }

  /// <summary>Length of the STREET_2 attribute.</summary>
  public const int Street2_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_2 attribute.
  /// The second line of a postal address for military service duty station.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 5, Type = MemberType.Char, Length = Street2_MaxLength, Optional
    = true)]
  public string Street2
  {
    get => street2;
    set => street2 = value != null
      ? TrimEnd(Substring(value, 1, Street2_MaxLength)) : null;
  }

  /// <summary>Length of the CITY attribute.</summary>
  public const int City_MaxLength = 15;

  /// <summary>
  /// The value of the CITY attribute.
  /// The community where the military service duty station address is located.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 6, Type = MemberType.Char, Length = City_MaxLength, Optional
    = true)]
  public string City
  {
    get => city;
    set => city = value != null
      ? TrimEnd(Substring(value, 1, City_MaxLength)) : null;
  }

  /// <summary>Length of the STATE attribute.</summary>
  public const int State_MaxLength = 2;

  /// <summary>
  /// The value of the STATE attribute.
  /// The politically autonomous or semi-autonomous region in which the military
  /// service duty station is located.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 7, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the PROVINCE attribute.</summary>
  public const int Province_MaxLength = 5;

  /// <summary>
  /// The value of the PROVINCE attribute.
  /// The administrative district in which the military service duty station 
  /// address is located.
  /// </summary>
  [JsonPropertyName("province")]
  [Member(Index = 8, Type = MemberType.Char, Length = Province_MaxLength, Optional
    = true)]
  public string Province
  {
    get => province;
    set => province = value != null
      ? TrimEnd(Substring(value, 1, Province_MaxLength)) : null;
  }

  /// <summary>Length of the POSTAL_CODE attribute.</summary>
  public const int PostalCode_MaxLength = 10;

  /// <summary>
  /// The value of the POSTAL_CODE attribute.
  /// Identifying the area in which the military service duty station foreign 
  /// address is located.
  /// </summary>
  [JsonPropertyName("postalCode")]
  [Member(Index = 9, Type = MemberType.Char, Length = PostalCode_MaxLength, Optional
    = true)]
  public string PostalCode
  {
    get => postalCode;
    set => postalCode = value != null
      ? TrimEnd(Substring(value, 1, PostalCode_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE5 attribute.</summary>
  public const int ZipCode5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_CODE5 attribute.
  /// The 5-digit addressing standard US postal code that identifies the region 
  /// in which the military service duty station address is located.
  /// </summary>
  [JsonPropertyName("zipCode5")]
  [Member(Index = 10, Type = MemberType.Char, Length = ZipCode5_MaxLength, Optional
    = true)]
  public string ZipCode5
  {
    get => zipCode5;
    set => zipCode5 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_CODE4 attribute.</summary>
  public const int ZipCode4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_CODE4 attribute.
  /// The 4-digit addressing standard US postal code used in conjunction with 5-
  /// digit zip code to further identify the region in which the military
  /// service duty station address is located.
  /// </summary>
  [JsonPropertyName("zipCode4")]
  [Member(Index = 11, Type = MemberType.Char, Length = ZipCode4_MaxLength, Optional
    = true)]
  public string ZipCode4
  {
    get => zipCode4;
    set => zipCode4 = value != null
      ? TrimEnd(Substring(value, 1, ZipCode4_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP3 attribute.</summary>
  public const int Zip3_MaxLength = 3;

  /// <summary>
  /// The value of the ZIP3 attribute.
  /// The 3-digit US postal code used in conjunction with 5-digit and 4-digit 
  /// zip codes to further identify the region in which the military service
  /// duty station address is located.
  /// </summary>
  [JsonPropertyName("zip3")]
  [Member(Index = 12, Type = MemberType.Char, Length = Zip3_MaxLength, Optional
    = true)]
  public string Zip3
  {
    get => zip3;
    set => zip3 = value != null
      ? TrimEnd(Substring(value, 1, Zip3_MaxLength)) : null;
  }

  /// <summary>Length of the COUNTRY attribute.</summary>
  public const int Country_MaxLength = 2;

  /// <summary>
  /// The value of the COUNTRY attribute.
  /// Identifies the part of the world in which the military service duty 
  /// station address is located.
  /// </summary>
  [JsonPropertyName("country")]
  [Member(Index = 13, Type = MemberType.Char, Length = Country_MaxLength, Optional
    = true)]
  public string Country
  {
    get => country;
    set => country = value != null
      ? TrimEnd(Substring(value, 1, Country_MaxLength)) : null;
  }

  /// <summary>Length of the APO attribute.</summary>
  public const int Apo_MaxLength = 2;

  /// <summary>
  /// The value of the APO attribute.
  /// Extension to ZIP that identifies military location.
  /// </summary>
  [JsonPropertyName("apo")]
  [Member(Index = 14, Type = MemberType.Char, Length = Apo_MaxLength, Optional
    = true)]
  public string Apo
  {
    get => apo;
    set => apo = value != null ? TrimEnd(Substring(value, 1, Apo_MaxLength)) : null
      ;
  }

  /// <summary>
  /// The value of the EXPECTED_RETURN_DATE_TO_STATES attribute.
  /// The date that a military officer plans on returning to the United States
  /// </summary>
  [JsonPropertyName("expectedReturnDateToStates")]
  [Member(Index = 15, Type = MemberType.Date, Optional = true)]
  public DateTime? ExpectedReturnDateToStates
  {
    get => expectedReturnDateToStates;
    set => expectedReturnDateToStates = value;
  }

  /// <summary>Length of the OVERSEAS_DUTY_STATION attribute.</summary>
  public const int OverseasDutyStation_MaxLength = 30;

  /// <summary>
  /// The value of the OVERSEAS_DUTY_STATION attribute.
  /// The location of the military officer that is serving duty in other 
  /// country.
  /// </summary>
  [JsonPropertyName("overseasDutyStation")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = OverseasDutyStation_MaxLength, Optional = true)]
  public string OverseasDutyStation
  {
    get => overseasDutyStation;
    set => overseasDutyStation = value != null
      ? TrimEnd(Substring(value, 1, OverseasDutyStation_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EXPECTED_DISCHARGE_DATE attribute.
  /// The date that a military officer plans to be relieved from military duty.
  /// </summary>
  [JsonPropertyName("expectedDischargeDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? ExpectedDischargeDate
  {
    get => expectedDischargeDate;
    set => expectedDischargeDate = value;
  }

  /// <summary>
  /// The value of the PHONE attribute.
  /// Phone number at the military service comprising of the 7-digit phone no.
  /// </summary>
  [JsonPropertyName("phone")]
  [Member(Index = 18, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? Phone
  {
    get => phone;
    set => phone = value;
  }

  /// <summary>Length of the BRANCH_CODE attribute.</summary>
  public const int BranchCode_MaxLength = 4;

  /// <summary>
  /// The value of the BRANCH_CODE attribute.
  /// Branch of service.  Army, navy, marines, air force, coast guard, etc.
  /// The code values and descriptions are kept in CODE_VALUE entity for 
  /// CODE_NAME MILITARY_BRANCH
  /// </summary>
  [JsonPropertyName("branchCode")]
  [Member(Index = 19, Type = MemberType.Char, Length = BranchCode_MaxLength, Optional
    = true)]
  public string BranchCode
  {
    get => branchCode;
    set => branchCode = value != null
      ? TrimEnd(Substring(value, 1, BranchCode_MaxLength)) : null;
  }

  /// <summary>Length of the COMMANDING_OFFICER_LAST_NAME attribute.</summary>
  public const int CommandingOfficerLastName_MaxLength = 17;

  /// <summary>
  /// The value of the COMMANDING_OFFICER_LAST_NAME attribute.
  /// The last name of the Commanding Officer.
  /// </summary>
  [JsonPropertyName("commandingOfficerLastName")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = CommandingOfficerLastName_MaxLength, Optional = true)]
  public string CommandingOfficerLastName
  {
    get => commandingOfficerLastName;
    set => commandingOfficerLastName = value != null
      ? TrimEnd(Substring(value, 1, CommandingOfficerLastName_MaxLength)) : null
      ;
  }

  /// <summary>Length of the COMMANDING_OFFICER_FIRST_NAME attribute.</summary>
  public const int CommandingOfficerFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the COMMANDING_OFFICER_FIRST_NAME attribute.
  /// The first name of the Commanding Officer.
  /// </summary>
  [JsonPropertyName("commandingOfficerFirstName")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = CommandingOfficerFirstName_MaxLength, Optional = true)]
  public string CommandingOfficerFirstName
  {
    get => commandingOfficerFirstName;
    set => commandingOfficerFirstName = value != null
      ? TrimEnd(Substring(value, 1, CommandingOfficerFirstName_MaxLength)) : null
      ;
  }

  /// <summary>Length of the COMMANDING_OFFICER_MI attribute.</summary>
  public const int CommandingOfficerMi_MaxLength = 1;

  /// <summary>
  /// The value of the COMMANDING_OFFICER_MI attribute.
  /// Middle Initial of the Commanding Officer.
  /// </summary>
  [JsonPropertyName("commandingOfficerMi")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = CommandingOfficerMi_MaxLength, Optional = true)]
  public string CommandingOfficerMi
  {
    get => commandingOfficerMi;
    set => commandingOfficerMi = value != null
      ? TrimEnd(Substring(value, 1, CommandingOfficerMi_MaxLength)) : null;
  }

  /// <summary>Length of the CURRENT_US_DUTY_STATION attribute.</summary>
  public const int CurrentUsDutyStation_MaxLength = 30;

  /// <summary>
  /// The value of the CURRENT_US_DUTY_STATION attribute.
  /// The company name/unit and location of the military post within the United 
  /// States.
  /// </summary>
  [JsonPropertyName("currentUsDutyStation")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = CurrentUsDutyStation_MaxLength, Optional = true)]
  public string CurrentUsDutyStation
  {
    get => currentUsDutyStation;
    set => currentUsDutyStation = value != null
      ? TrimEnd(Substring(value, 1, CurrentUsDutyStation_MaxLength)) : null;
  }

  /// <summary>Length of the DUTY_STATUS_CODE attribute.</summary>
  public const int DutyStatusCode_MaxLength = 1;

  /// <summary>
  /// The value of the DUTY_STATUS_CODE attribute.
  /// Describes current state relating to whether active, inactive, retired, or 
  /// reserves.
  /// The code values and descriptions are kept in CODE_VALUE table for 
  /// CODE_NAME DUTY_STATUS_CODE.
  /// </summary>
  [JsonPropertyName("dutyStatusCode")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = DutyStatusCode_MaxLength, Optional = true)]
  public string DutyStatusCode
  {
    get => dutyStatusCode;
    set => dutyStatusCode = value != null
      ? TrimEnd(Substring(value, 1, DutyStatusCode_MaxLength)) : null;
  }

  /// <summary>Length of the RANK attribute.</summary>
  public const int Rank_MaxLength = 4;

  /// <summary>
  /// The value of the RANK attribute.
  /// Rank code of the Last rank person held.
  /// The permitted values are in CODE_VALUE entity for CODE_NAME MILITARY_RANK
  /// </summary>
  [JsonPropertyName("rank")]
  [Member(Index = 25, Type = MemberType.Char, Length = Rank_MaxLength, Optional
    = true)]
  public string Rank
  {
    get => rank;
    set => rank = value != null
      ? TrimEnd(Substring(value, 1, Rank_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// Date released from military service.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// Date first entered military service.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// This attribue specifies the date the service in the branch and rank 
  /// started.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 28, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  [Member(Index = 30, Type = MemberType.Timestamp)]
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
  [Member(Index = 31, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 32, Type = MemberType.Timestamp)]
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
  [Member(Index = 33, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private int? phoneAreaCode;
  private string phoneCountryCode;
  private string phoneExt;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string province;
  private string postalCode;
  private string zipCode5;
  private string zipCode4;
  private string zip3;
  private string country;
  private string apo;
  private DateTime? expectedReturnDateToStates;
  private string overseasDutyStation;
  private DateTime? expectedDischargeDate;
  private int? phone;
  private string branchCode;
  private string commandingOfficerLastName;
  private string commandingOfficerFirstName;
  private string commandingOfficerMi;
  private string currentUsDutyStation;
  private string dutyStatusCode;
  private string rank;
  private DateTime? endDate;
  private DateTime? startDate;
  private DateTime? effectiveDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
}

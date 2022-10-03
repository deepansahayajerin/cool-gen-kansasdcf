// The source file: HEALTH_INSURANCE_AVAILABILITY, ID: 371845709, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// 
/// This entity type stores health insurance for an individual cse person.
/// This is not a court order insurance coverage. It's a kind of insurance
/// that is available for the cse person to purchase coverage for his or her
/// children.
/// </summary>
[Serializable]
public partial class HealthInsuranceAvailability: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public HealthInsuranceAvailability()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public HealthInsuranceAvailability(HealthInsuranceAvailability that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new HealthInsuranceAvailability Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(HealthInsuranceAvailability that)
  {
    base.Assign(that);
    insuranceId = that.insuranceId;
    insurancePolicyNumber = that.insurancePolicyNumber;
    insuranceGroupNumber = that.insuranceGroupNumber;
    insuranceName = that.insuranceName;
    street1 = that.street1;
    street2 = that.street2;
    city = that.city;
    state = that.state;
    zip5 = that.zip5;
    zip4 = that.zip4;
    cost = that.cost;
    costFrequency = that.costFrequency;
    verifiedDate = that.verifiedDate;
    endDate = that.endDate;
    employerName = that.employerName;
    empStreet1 = that.empStreet1;
    empStreet2 = that.empStreet2;
    empCity = that.empCity;
    empState = that.empState;
    empZip5 = that.empZip5;
    empZip4 = that.empZip4;
    empPhoneAreaCode = that.empPhoneAreaCode;
    empPhoneNo = that.empPhoneNo;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the INSURANCE_ID attribute.
  /// System generate number for identify each health insurance availability.
  /// </summary>
  [JsonPropertyName("insuranceId")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 5)]
  public int InsuranceId
  {
    get => insuranceId;
    set => insuranceId = value;
  }

  /// <summary>Length of the INSURANCE_POLICY_NUMBER attribute.</summary>
  public const int InsurancePolicyNumber_MaxLength = 20;

  /// <summary>
  /// The value of the INSURANCE_POLICY_NUMBER attribute.
  /// The individual's unique insurance policy number. This is not a group 
  /// number.
  /// </summary>
  [JsonPropertyName("insurancePolicyNumber")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = InsurancePolicyNumber_MaxLength, Optional = true)]
  public string InsurancePolicyNumber
  {
    get => insurancePolicyNumber;
    set => insurancePolicyNumber = value != null
      ? TrimEnd(Substring(value, 1, InsurancePolicyNumber_MaxLength)) : null;
  }

  /// <summary>Length of the INSURANCE_GROUP_NUMBER attribute.</summary>
  public const int InsuranceGroupNumber_MaxLength = 20;

  /// <summary>
  /// The value of the INSURANCE_GROUP_NUMBER attribute.
  /// A number that identifies a particular group within a health insurance 
  /// company.
  /// </summary>
  [JsonPropertyName("insuranceGroupNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = InsuranceGroupNumber_MaxLength, Optional = true)]
  public string InsuranceGroupNumber
  {
    get => insuranceGroupNumber;
    set => insuranceGroupNumber = value != null
      ? TrimEnd(Substring(value, 1, InsuranceGroupNumber_MaxLength)) : null;
  }

  /// <summary>Length of the INSURANCE_NAME attribute.</summary>
  public const int InsuranceName_MaxLength = 30;

  /// <summary>
  /// The value of the INSURANCE_NAME attribute.
  /// Name of insurance company.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = InsuranceName_MaxLength)]
  public string InsuranceName
  {
    get => insuranceName ?? "";
    set => insuranceName =
      TrimEnd(Substring(value, 1, InsuranceName_MaxLength));
  }

  /// <summary>
  /// The json value of the InsuranceName attribute.</summary>
  [JsonPropertyName("insuranceName")]
  [Computed]
  public string InsuranceName_Json
  {
    get => NullIf(InsuranceName, "");
    set => InsuranceName = value;
  }

  /// <summary>Length of the STREET_1 attribute.</summary>
  public const int Street1_MaxLength = 25;

  /// <summary>
  /// The value of the STREET_1 attribute.
  /// The first line of postal address of a health insurance address.
  /// </summary>
  [JsonPropertyName("street1")]
  [Member(Index = 5, Type = MemberType.Char, Length = Street1_MaxLength, Optional
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
  /// The second line of postal address of a health insurance address.
  /// </summary>
  [JsonPropertyName("street2")]
  [Member(Index = 6, Type = MemberType.Char, Length = Street2_MaxLength, Optional
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
  /// The community where the health insurance address is located.
  /// </summary>
  [JsonPropertyName("city")]
  [Member(Index = 7, Type = MemberType.Char, Length = City_MaxLength, Optional
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
  /// The politically autonomous or semi-autonomous region in which the health 
  /// insurance address is located.
  /// </summary>
  [JsonPropertyName("state")]
  [Member(Index = 8, Type = MemberType.Char, Length = State_MaxLength, Optional
    = true)]
  public string State
  {
    get => state;
    set => state = value != null
      ? TrimEnd(Substring(value, 1, State_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_5 attribute.</summary>
  public const int Zip5_MaxLength = 5;

  /// <summary>
  /// The value of the ZIP_5 attribute.
  /// The 5-digit addressing standard US postal code that identifies the region 
  /// in which the health insurance address is located.
  /// </summary>
  [JsonPropertyName("zip5")]
  [Member(Index = 9, Type = MemberType.Char, Length = Zip5_MaxLength, Optional
    = true)]
  public string Zip5
  {
    get => zip5;
    set => zip5 = value != null
      ? TrimEnd(Substring(value, 1, Zip5_MaxLength)) : null;
  }

  /// <summary>Length of the ZIP_4 attribute.</summary>
  public const int Zip4_MaxLength = 4;

  /// <summary>
  /// The value of the ZIP_4 attribute.
  /// The 4-digit addressing standard US postal code used in conjunction with 5-
  /// digit zip code to further identify the region in which the health
  /// insurance address is located.
  /// </summary>
  [JsonPropertyName("zip4")]
  [Member(Index = 10, Type = MemberType.Char, Length = Zip4_MaxLength, Optional
    = true)]
  public string Zip4
  {
    get => zip4;
    set => zip4 = value != null
      ? TrimEnd(Substring(value, 1, Zip4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the COST attribute.
  /// The cost of health insurance that each individual cse person pays for his 
  /// or her children with this health insurance company.
  /// </summary>
  [JsonPropertyName("cost")]
  [Member(Index = 11, Type = MemberType.Number, Length = 6, Precision = 2, Optional
    = true)]
  public decimal? Cost
  {
    get => cost;
    set => cost = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>Length of the COST_FREQUENCY attribute.</summary>
  public const int CostFrequency_MaxLength = 2;

  /// <summary>
  /// The value of the COST_FREQUENCY attribute.
  /// Use to determines to frequency of the cost,i.e. monthly,bi-weekly, etc. 
  /// Must validate it against the FREQUENCY code value table. Can not be blank
  /// when cost >0.
  /// </summary>
  [JsonPropertyName("costFrequency")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CostFrequency_MaxLength, Optional = true)]
  public string CostFrequency
  {
    get => costFrequency;
    set => costFrequency = value != null
      ? TrimEnd(Substring(value, 1, CostFrequency_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the VERIFIED_DATE attribute.
  /// The date that the work verifies the information given by each cse person.
  /// </summary>
  [JsonPropertyName("verifiedDate")]
  [Member(Index = 13, Type = MemberType.Date, Optional = true)]
  public DateTime? VerifiedDate
  {
    get => verifiedDate;
    set => verifiedDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The expiration of an insurance coverage.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 30;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// Name of employer that provided the insurance coverage.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = EmployerName_MaxLength)]
  public string EmployerName
  {
    get => employerName ?? "";
    set => employerName = TrimEnd(Substring(value, 1, EmployerName_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerName attribute.</summary>
  [JsonPropertyName("employerName")]
  [Computed]
  public string EmployerName_Json
  {
    get => NullIf(EmployerName, "");
    set => EmployerName = value;
  }

  /// <summary>Length of the EMP_STREET_1 attribute.</summary>
  public const int EmpStreet1_MaxLength = 25;

  /// <summary>
  /// The value of the EMP_STREET_1 attribute.
  /// The first line of postal address of a employer address.
  /// </summary>
  [JsonPropertyName("empStreet1")]
  [Member(Index = 16, Type = MemberType.Char, Length = EmpStreet1_MaxLength, Optional
    = true)]
  public string EmpStreet1
  {
    get => empStreet1;
    set => empStreet1 = value != null
      ? TrimEnd(Substring(value, 1, EmpStreet1_MaxLength)) : null;
  }

  /// <summary>Length of the EMP_STREET_2 attribute.</summary>
  public const int EmpStreet2_MaxLength = 25;

  /// <summary>
  /// The value of the EMP_STREET_2 attribute.
  /// The second line of postal address of a employer address.
  /// </summary>
  [JsonPropertyName("empStreet2")]
  [Member(Index = 17, Type = MemberType.Char, Length = EmpStreet2_MaxLength, Optional
    = true)]
  public string EmpStreet2
  {
    get => empStreet2;
    set => empStreet2 = value != null
      ? TrimEnd(Substring(value, 1, EmpStreet2_MaxLength)) : null;
  }

  /// <summary>Length of the EMP_CITY attribute.</summary>
  public const int EmpCity_MaxLength = 15;

  /// <summary>
  /// The value of the EMP_CITY attribute.
  /// The community where the employer address is located.
  /// </summary>
  [JsonPropertyName("empCity")]
  [Member(Index = 18, Type = MemberType.Char, Length = EmpCity_MaxLength, Optional
    = true)]
  public string EmpCity
  {
    get => empCity;
    set => empCity = value != null
      ? TrimEnd(Substring(value, 1, EmpCity_MaxLength)) : null;
  }

  /// <summary>Length of the EMP_STATE attribute.</summary>
  public const int EmpState_MaxLength = 2;

  /// <summary>
  /// The value of the EMP_STATE attribute.
  /// The politically autonomous or semi-autonomous region in which the employer
  /// address is located.
  /// </summary>
  [JsonPropertyName("empState")]
  [Member(Index = 19, Type = MemberType.Char, Length = EmpState_MaxLength, Optional
    = true)]
  public string EmpState
  {
    get => empState;
    set => empState = value != null
      ? TrimEnd(Substring(value, 1, EmpState_MaxLength)) : null;
  }

  /// <summary>Length of the EMP_ZIP_5 attribute.</summary>
  public const int EmpZip5_MaxLength = 5;

  /// <summary>
  /// The value of the EMP_ZIP_5 attribute.
  /// The 5-digit addressing standard US postal code that identifies the region 
  /// in which the employer address is located.
  /// </summary>
  [JsonPropertyName("empZip5")]
  [Member(Index = 20, Type = MemberType.Char, Length = EmpZip5_MaxLength, Optional
    = true)]
  public string EmpZip5
  {
    get => empZip5;
    set => empZip5 = value != null
      ? TrimEnd(Substring(value, 1, EmpZip5_MaxLength)) : null;
  }

  /// <summary>Length of the EMP_ZIP_4 attribute.</summary>
  public const int EmpZip4_MaxLength = 4;

  /// <summary>
  /// The value of the EMP_ZIP_4 attribute.
  /// The 4-digit addressing standard US postal code used in conjunction with 5-
  /// digit zip code to further identify the region in which the employer
  /// address is located.
  /// </summary>
  [JsonPropertyName("empZip4")]
  [Member(Index = 21, Type = MemberType.Char, Length = EmpZip4_MaxLength, Optional
    = true)]
  public string EmpZip4
  {
    get => empZip4;
    set => empZip4 = value != null
      ? TrimEnd(Substring(value, 1, EmpZip4_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EMP_PHONE_AREA_CODE attribute.
  /// The area code of the employer phone.
  /// </summary>
  [JsonPropertyName("empPhoneAreaCode")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 3)]
  public int EmpPhoneAreaCode
  {
    get => empPhoneAreaCode;
    set => empPhoneAreaCode = value;
  }

  /// <summary>
  /// The value of the EMP_PHONE_NO attribute.
  /// The telephone number of the employer.
  /// </summary>
  [JsonPropertyName("empPhoneNo")]
  [Member(Index = 23, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? EmpPhoneNo
  {
    get => empPhoneNo;
    set => empPhoneNo = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
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
  [Member(Index = 25, Type = MemberType.Timestamp)]
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
  [Member(Index = 26, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private int insuranceId;
  private string insurancePolicyNumber;
  private string insuranceGroupNumber;
  private string insuranceName;
  private string street1;
  private string street2;
  private string city;
  private string state;
  private string zip5;
  private string zip4;
  private decimal? cost;
  private string costFrequency;
  private DateTime? verifiedDate;
  private DateTime? endDate;
  private string employerName;
  private string empStreet1;
  private string empStreet2;
  private string empCity;
  private string empState;
  private string empZip5;
  private string empZip4;
  private int empPhoneAreaCode;
  private int? empPhoneNo;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
}

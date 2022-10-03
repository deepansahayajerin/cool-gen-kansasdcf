// The source file: WORKERS_COMP_CLAIM, ID: 1902561443, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Workers compensation claim information provided by KDOL.
/// </summary>
[Serializable]
public partial class WorkersCompClaim: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkersCompClaim()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkersCompClaim(WorkersCompClaim that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkersCompClaim Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Number used in uniquely identifing the workers comp claim.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => Get<int?>("identifier") ?? 0;
    set => Set("identifier", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the CLAIMANT_FIRST_NAME attribute.</summary>
  public const int ClaimantFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_FIRST_NAME attribute.
  /// The first name of the workers comp claimant.
  /// </summary>
  [JsonPropertyName("claimantFirstName")]
  [Member(Index = 2, Type = MemberType.Varchar, Length
    = ClaimantFirstName_MaxLength, Optional = true)]
  public string ClaimantFirstName
  {
    get => Get<string>("claimantFirstName");
    set => Set(
      "claimantFirstName", Substring(value, 1, ClaimantFirstName_MaxLength));
  }

  /// <summary>Length of the CLAIMANT_MIDDLE_NAME attribute.</summary>
  public const int ClaimantMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_MIDDLE_NAME attribute.
  /// The middle name of the workers comp claimant.
  /// </summary>
  [JsonPropertyName("claimantMiddleName")]
  [Member(Index = 3, Type = MemberType.Varchar, Length
    = ClaimantMiddleName_MaxLength, Optional = true)]
  public string ClaimantMiddleName
  {
    get => Get<string>("claimantMiddleName");
    set => Set(
      "claimantMiddleName", Substring(value, 1, ClaimantMiddleName_MaxLength));
  }

  /// <summary>Length of the CLAIMANT_LAST_NAME attribute.</summary>
  public const int ClaimantLastName_MaxLength = 39;

  /// <summary>
  /// The value of the CLAIMANT_LAST_NAME attribute.
  /// The surname of the workers comp claimant.
  /// </summary>
  [JsonPropertyName("claimantLastName")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = ClaimantLastName_MaxLength, Optional = true)]
  public string ClaimantLastName
  {
    get => Get<string>("claimantLastName");
    set => Set(
      "claimantLastName", Substring(value, 1, ClaimantLastName_MaxLength));
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_FIRST_NAME attribute.</summary>
  public const int ClaimantAttorneyFirstName_MaxLength = 11;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_FIRST_NAME attribute.
  /// The first name of the claimants attorney.
  /// </summary>
  [JsonPropertyName("claimantAttorneyFirstName")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = ClaimantAttorneyFirstName_MaxLength, Optional = true)]
  public string ClaimantAttorneyFirstName
  {
    get => Get<string>("claimantAttorneyFirstName");
    set => Set(
      "claimantAttorneyFirstName", TrimEnd(Substring(value, 1,
      ClaimantAttorneyFirstName_MaxLength)));
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_LAST_NAME attribute.</summary>
  public const int ClaimantAttorneyLastName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_LAST_NAME attribute.
  /// The surname of the claimants attorney.
  /// </summary>
  [JsonPropertyName("claimantAttorneyLastName")]
  [Member(Index = 6, Type = MemberType.Varchar, Length
    = ClaimantAttorneyLastName_MaxLength, Optional = true)]
  public string ClaimantAttorneyLastName
  {
    get => Get<string>("claimantAttorneyLastName");
    set => Set(
      "claimantAttorneyLastName", Substring(value, 1,
      ClaimantAttorneyLastName_MaxLength));
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_FIRM_NAME attribute.</summary>
  public const int ClaimantAttorneyFirmName_MaxLength = 50;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_FIRM_NAME attribute.
  /// The firm name of the claimants attorney.
  /// </summary>
  [JsonPropertyName("claimantAttorneyFirmName")]
  [Member(Index = 7, Type = MemberType.Varchar, Length
    = ClaimantAttorneyFirmName_MaxLength, Optional = true)]
  public string ClaimantAttorneyFirmName
  {
    get => Get<string>("claimantAttorneyFirmName");
    set => Set(
      "claimantAttorneyFirmName", Substring(value, 1,
      ClaimantAttorneyFirmName_MaxLength));
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 71;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// The name of the claimants employer.
  /// </summary>
  [JsonPropertyName("employerName")]
  [Member(Index = 8, Type = MemberType.Varchar, Length
    = EmployerName_MaxLength, Optional = true)]
  public string EmployerName
  {
    get => Get<string>("employerName");
    set => Set("employerName", Substring(value, 1, EmployerName_MaxLength));
  }

  /// <summary>Length of the DOCKET_NUMBER attribute.</summary>
  public const int DocketNumber_MaxLength = 7;

  /// <summary>
  /// The value of the DOCKET_NUMBER attribute.
  /// The court docket number assigned to the claim.
  /// </summary>
  [JsonPropertyName("docketNumber")]
  [Member(Index = 9, Type = MemberType.Char, Length = DocketNumber_MaxLength, Optional
    = true)]
  public string DocketNumber
  {
    get => Get<string>("docketNumber");
    set => Set(
      "docketNumber", TrimEnd(Substring(value, 1, DocketNumber_MaxLength)));
  }

  /// <summary>Length of the INSURER_NAME attribute.</summary>
  public const int InsurerName_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_NAME attribute.
  /// The name of the employers insurance carrier.
  /// </summary>
  [JsonPropertyName("insurerName")]
  [Member(Index = 10, Type = MemberType.Varchar, Length
    = InsurerName_MaxLength, Optional = true)]
  public string InsurerName
  {
    get => Get<string>("insurerName");
    set => Set("insurerName", Substring(value, 1, InsurerName_MaxLength));
  }

  /// <summary>Length of the INSURER_ATTORNEY_FIRM_NAME attribute.</summary>
  public const int InsurerAttorneyFirmName_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_FIRM_NAME attribute.
  /// The firm name of the insurance companys attorney.
  /// </summary>
  [JsonPropertyName("insurerAttorneyFirmName")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = InsurerAttorneyFirmName_MaxLength, Optional = true)]
  public string InsurerAttorneyFirmName
  {
    get => Get<string>("insurerAttorneyFirmName");
    set => Set(
      "insurerAttorneyFirmName", Substring(value, 1,
      InsurerAttorneyFirmName_MaxLength));
  }

  /// <summary>Length of the INSURER_CONTACT_NAME_1 attribute.</summary>
  public const int InsurerContactName1_MaxLength = 35;

  /// <summary>
  /// The value of the INSURER_CONTACT_NAME_1 attribute.
  /// The first contact name at the insurance company.
  /// </summary>
  [JsonPropertyName("insurerContactName1")]
  [Member(Index = 12, Type = MemberType.Varchar, Length
    = InsurerContactName1_MaxLength, Optional = true)]
  public string InsurerContactName1
  {
    get => Get<string>("insurerContactName1");
    set => Set(
      "insurerContactName1",
      Substring(value, 1, InsurerContactName1_MaxLength));
  }

  /// <summary>Length of the INSURER_CONTACT_NAME_2 attribute.</summary>
  public const int InsurerContactName2_MaxLength = 35;

  /// <summary>
  /// The value of the INSURER_CONTACT_NAME_2 attribute.
  /// The second contact name at the insurance company.
  /// </summary>
  [JsonPropertyName("insurerContactName2")]
  [Member(Index = 13, Type = MemberType.Varchar, Length
    = InsurerContactName2_MaxLength, Optional = true)]
  public string InsurerContactName2
  {
    get => Get<string>("insurerContactName2");
    set => Set(
      "insurerContactName2",
      Substring(value, 1, InsurerContactName2_MaxLength));
  }

  /// <summary>Length of the INSURER_CONTACT_PHONE attribute.</summary>
  public const int InsurerContactPhone_MaxLength = 20;

  /// <summary>
  /// The value of the INSURER_CONTACT_PHONE attribute.
  /// The phone number for the contact at the insurance company.
  /// </summary>
  [JsonPropertyName("insurerContactPhone")]
  [Member(Index = 14, Type = MemberType.Varchar, Length
    = InsurerContactPhone_MaxLength, Optional = true)]
  public string InsurerContactPhone
  {
    get => Get<string>("insurerContactPhone");
    set => Set(
      "insurerContactPhone",
      Substring(value, 1, InsurerContactPhone_MaxLength));
  }

  /// <summary>Length of the POLICY_NUMBER attribute.</summary>
  public const int PolicyNumber_MaxLength = 30;

  /// <summary>
  /// The value of the POLICY_NUMBER attribute.
  /// The insurance company's policy number.
  /// </summary>
  [JsonPropertyName("policyNumber")]
  [Member(Index = 15, Type = MemberType.Varchar, Length
    = PolicyNumber_MaxLength, Optional = true)]
  public string PolicyNumber
  {
    get => Get<string>("policyNumber");
    set => Set("policyNumber", Substring(value, 1, PolicyNumber_MaxLength));
  }

  /// <summary>
  /// The value of the DATE_OF_LOSS attribute.
  /// The date the injury was first reported.
  /// </summary>
  [JsonPropertyName("dateOfLoss")]
  [Member(Index = 16, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfLoss
  {
    get => Get<DateTime?>("dateOfLoss");
    set => Set("dateOfLoss", value);
  }

  /// <summary>Length of the EMPLOYER_FEIN attribute.</summary>
  public const int EmployerFein_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYER_FEIN attribute.
  /// The federal employer identification number of the claimants employer.
  /// </summary>
  [JsonPropertyName("employerFein")]
  [Member(Index = 17, Type = MemberType.Char, Length = EmployerFein_MaxLength, Optional
    = true)]
  public string EmployerFein
  {
    get => Get<string>("employerFein");
    set => Set(
      "employerFein", TrimEnd(Substring(value, 1, EmployerFein_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_OF_ACCIDENT attribute.
  /// The date the accident occurred.
  /// </summary>
  [JsonPropertyName("dateOfAccident")]
  [Member(Index = 18, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfAccident
  {
    get => Get<DateTime?>("dateOfAccident");
    set => Set("dateOfAccident", value);
  }

  /// <summary>Length of the WAGE_AMOUNT attribute.</summary>
  public const int WageAmount_MaxLength = 12;

  /// <summary>
  /// The value of the WAGE_AMOUNT attribute.
  /// The amount of the claimants gross average weekly wages.
  /// </summary>
  [JsonPropertyName("wageAmount")]
  [Member(Index = 19, Type = MemberType.Char, Length = WageAmount_MaxLength, Optional
    = true)]
  public string WageAmount
  {
    get => Get<string>("wageAmount");
    set =>
      Set("wageAmount", TrimEnd(Substring(value, 1, WageAmount_MaxLength)));
  }

  /// <summary>Length of the ACCIDENT_CITY attribute.</summary>
  public const int AccidentCity_MaxLength = 25;

  /// <summary>
  /// The value of the ACCIDENT_CITY attribute.
  /// The city in which the accident occurred.
  /// </summary>
  [JsonPropertyName("accidentCity")]
  [Member(Index = 20, Type = MemberType.Varchar, Length
    = AccidentCity_MaxLength, Optional = true)]
  public string AccidentCity
  {
    get => Get<string>("accidentCity");
    set => Set("accidentCity", Substring(value, 1, AccidentCity_MaxLength));
  }

  /// <summary>Length of the ACCIDENT_STATE attribute.</summary>
  public const int AccidentState_MaxLength = 12;

  /// <summary>
  /// The value of the ACCIDENT_STATE attribute.
  /// The state in which the accident occurred.
  /// </summary>
  [JsonPropertyName("accidentState")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = AccidentState_MaxLength, Optional = true)]
  public string AccidentState
  {
    get => Get<string>("accidentState");
    set => Set(
      "accidentState", TrimEnd(Substring(value, 1, AccidentState_MaxLength)));
  }

  /// <summary>Length of the ACCIDENT_COUNTY attribute.</summary>
  public const int AccidentCounty_MaxLength = 20;

  /// <summary>
  /// The value of the ACCIDENT_COUNTY attribute.
  /// The county where the accident occurred.
  /// </summary>
  [JsonPropertyName("accidentCounty")]
  [Member(Index = 22, Type = MemberType.Varchar, Length
    = AccidentCounty_MaxLength, Optional = true)]
  public string AccidentCounty
  {
    get => Get<string>("accidentCounty");
    set => Set("accidentCounty", Substring(value, 1, AccidentCounty_MaxLength));
  }

  /// <summary>Length of the ACCIDENT_DESCRIPTION attribute.</summary>
  public const int AccidentDescription_MaxLength = 500;

  /// <summary>
  /// The value of the ACCIDENT_DESCRIPTION attribute.
  /// Freeform description of what caused the accident.
  /// </summary>
  [JsonPropertyName("accidentDescription")]
  [Member(Index = 23, Type = MemberType.Varchar, Length
    = AccidentDescription_MaxLength, Optional = true)]
  public string AccidentDescription
  {
    get => Get<string>("accidentDescription");
    set => Set(
      "accidentDescription",
      Substring(value, 1, AccidentDescription_MaxLength));
  }

  /// <summary>Length of the SEVERITY_CODE_DESCRIPTION attribute.</summary>
  public const int SeverityCodeDescription_MaxLength = 14;

  /// <summary>
  /// The value of the SEVERITY_CODE_DESCRIPTION attribute.
  /// Code description describing the severity of the accident/injury.
  /// </summary>
  [JsonPropertyName("severityCodeDescription")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = SeverityCodeDescription_MaxLength, Optional = true)]
  public string SeverityCodeDescription
  {
    get => Get<string>("severityCodeDescription");
    set => Set(
      "severityCodeDescription", TrimEnd(Substring(value, 1,
      SeverityCodeDescription_MaxLength)));
  }

  /// <summary>
  /// The value of the RETURNED_TO_WORK_DATE attribute.
  /// The date the claimant returned to work.
  /// </summary>
  [JsonPropertyName("returnedToWorkDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? ReturnedToWorkDate
  {
    get => Get<DateTime?>("returnedToWorkDate");
    set => Set("returnedToWorkDate", value);
  }

  /// <summary>Length of the COMPENSATION_PAID_FLAG attribute.</summary>
  public const int CompensationPaidFlag_MaxLength = 1;

  /// <summary>
  /// The value of the COMPENSATION_PAID_FLAG attribute.
  /// A flag indicating whether compensation has been paid.
  /// </summary>
  [JsonPropertyName("compensationPaidFlag")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = CompensationPaidFlag_MaxLength, Optional = true)]
  public string CompensationPaidFlag
  {
    get => Get<string>("compensationPaidFlag");
    set => Set(
      "compensationPaidFlag", TrimEnd(Substring(value, 1,
      CompensationPaidFlag_MaxLength)));
  }

  /// <summary>
  /// The value of the COMPENSATION_PAID_DATE attribute.
  /// The date compensation was paid to the claimant.
  /// </summary>
  [JsonPropertyName("compensationPaidDate")]
  [Member(Index = 27, Type = MemberType.Date, Optional = true)]
  public DateTime? CompensationPaidDate
  {
    get => Get<DateTime?>("compensationPaidDate");
    set => Set("compensationPaidDate", value);
  }

  /// <summary>Length of the WEEKLY_RATE attribute.</summary>
  public const int WeeklyRate_MaxLength = 12;

  /// <summary>
  /// The value of the WEEKLY_RATE attribute.
  /// The weekly rate from the accident report.
  /// </summary>
  [JsonPropertyName("weeklyRate")]
  [Member(Index = 28, Type = MemberType.Char, Length = WeeklyRate_MaxLength, Optional
    = true)]
  public string WeeklyRate
  {
    get => Get<string>("weeklyRate");
    set =>
      Set("weeklyRate", TrimEnd(Substring(value, 1, WeeklyRate_MaxLength)));
  }

  /// <summary>
  /// The value of the DATE_OF_DEATH attribute.
  /// The claimants date of death.
  /// </summary>
  [JsonPropertyName("dateOfDeath")]
  [Member(Index = 29, Type = MemberType.Date, Optional = true)]
  public DateTime? DateOfDeath
  {
    get => Get<DateTime?>("dateOfDeath");
    set => Set("dateOfDeath", value);
  }

  /// <summary>Length of the THIRD_PARTY_ADMINISTRATOR_NAME attribute.</summary>
  public const int ThirdPartyAdministratorName_MaxLength = 40;

  /// <summary>
  /// The value of the THIRD_PARTY_ADMINISTRATOR_NAME attribute.
  /// The name of the third party administrator over the claim.
  /// </summary>
  [JsonPropertyName("thirdPartyAdministratorName")]
  [Member(Index = 30, Type = MemberType.Varchar, Length
    = ThirdPartyAdministratorName_MaxLength, Optional = true)]
  public string ThirdPartyAdministratorName
  {
    get => Get<string>("thirdPartyAdministratorName");
    set => Set(
      "thirdPartyAdministratorName", Substring(value, 1,
      ThirdPartyAdministratorName_MaxLength));
  }

  /// <summary>Length of the ADMINISTRATIVE_CLAIM_NO attribute.</summary>
  public const int AdministrativeClaimNo_MaxLength = 25;

  /// <summary>
  /// The value of the ADMINISTRATIVE_CLAIM_NO attribute.
  /// The administrative claim number.
  /// </summary>
  [JsonPropertyName("administrativeClaimNo")]
  [Member(Index = 31, Type = MemberType.Varchar, Length
    = AdministrativeClaimNo_MaxLength, Optional = true)]
  public string AdministrativeClaimNo
  {
    get => Get<string>("administrativeClaimNo");
    set => Set(
      "administrativeClaimNo", Substring(value, 1,
      AdministrativeClaimNo_MaxLength));
  }

  /// <summary>
  /// The value of the CLAIM_FILED_DATE attribute.
  /// The date the claim was filed.
  /// </summary>
  [JsonPropertyName("claimFiledDate")]
  [Member(Index = 32, Type = MemberType.Date, Optional = true)]
  public DateTime? ClaimFiledDate
  {
    get => Get<DateTime?>("claimFiledDate");
    set => Set("claimFiledDate", value);
  }

  /// <summary>Length of the AGENCY_CLAIM_NO attribute.</summary>
  public const int AgencyClaimNo_MaxLength = 12;

  /// <summary>
  /// The value of the AGENCY_CLAIM_NO attribute.
  /// The KDOL claim number.
  /// </summary>
  [JsonPropertyName("agencyClaimNo")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = AgencyClaimNo_MaxLength, Optional = true)]
  public string AgencyClaimNo
  {
    get => Get<string>("agencyClaimNo");
    set => Set(
      "agencyClaimNo", TrimEnd(Substring(value, 1, AgencyClaimNo_MaxLength)));
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 35, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 36, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 37, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
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
  [Member(Index = 38, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => Get<string>("cspNumber") ?? "";
    set => Set(
      "cspNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CspNumber_MaxLength)));
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
}

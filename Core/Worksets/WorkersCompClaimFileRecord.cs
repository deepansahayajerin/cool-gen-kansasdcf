// The source file: WORKERS_COMP_CLAIM_FILE_RECORD, ID: 1902564051, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class WorkersCompClaimFileRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public WorkersCompClaimFileRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public WorkersCompClaimFileRecord(WorkersCompClaimFileRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new WorkersCompClaimFileRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(WorkersCompClaimFileRecord that)
  {
    base.Assign(that);
    ncpNumber = that.ncpNumber;
    claimantFirstName = that.claimantFirstName;
    claimantLastName = that.claimantLastName;
    claimantMiddleName = that.claimantMiddleName;
    claimantStreet = that.claimantStreet;
    claimantCity = that.claimantCity;
    claimantState = that.claimantState;
    claimantZip = that.claimantZip;
    claimantAttorneyFirstName = that.claimantAttorneyFirstName;
    claimantAttorneyLastName = that.claimantAttorneyLastName;
    claimantAttorneyFirmName = that.claimantAttorneyFirmName;
    claimantAttorneyStreet = that.claimantAttorneyStreet;
    claimantAttorneyCity = that.claimantAttorneyCity;
    claimantAttorneyState = that.claimantAttorneyState;
    claimantAttorneyZip = that.claimantAttorneyZip;
    employerName = that.employerName;
    docketNumber = that.docketNumber;
    insurerName = that.insurerName;
    insurerStreet = that.insurerStreet;
    insurerCity = that.insurerCity;
    insurerState = that.insurerState;
    insurerZip = that.insurerZip;
    insurerAttorneyFirmName = that.insurerAttorneyFirmName;
    insurerAttorneyStreet = that.insurerAttorneyStreet;
    insurerAttorneyCity = that.insurerAttorneyCity;
    insurerAttorneyState = that.insurerAttorneyState;
    insurerAttorneyZip = that.insurerAttorneyZip;
    insurerContactName1 = that.insurerContactName1;
    insurerContactName2 = that.insurerContactName2;
    insurerContactPhone = that.insurerContactPhone;
    policyNumber = that.policyNumber;
    dateOfLoss = that.dateOfLoss;
    employerFein = that.employerFein;
    employerStreet = that.employerStreet;
    employerCity = that.employerCity;
    employerState = that.employerState;
    employerZip = that.employerZip;
    dateOfAccident = that.dateOfAccident;
    wageAmount = that.wageAmount;
    accidentCity = that.accidentCity;
    accidentCounty = that.accidentCounty;
    accidentState = that.accidentState;
    accidentDescription = that.accidentDescription;
    severityCodeDescription = that.severityCodeDescription;
    returnedToWorkDate = that.returnedToWorkDate;
    compensationPaidFlag = that.compensationPaidFlag;
    compensationPaidDate = that.compensationPaidDate;
    weeklyRate = that.weeklyRate;
    dateOfDeath = that.dateOfDeath;
    thirdPartyAdministratorName = that.thirdPartyAdministratorName;
    administrativeClaimNumber = that.administrativeClaimNumber;
    claimFiledDate = that.claimFiledDate;
    agencyClaimNo = that.agencyClaimNo;
  }

  /// <summary>Length of the NCP_NUMBER attribute.</summary>
  public const int NcpNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NCP_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = NcpNumber_MaxLength)]
  public string NcpNumber
  {
    get => ncpNumber ?? "";
    set => ncpNumber = TrimEnd(Substring(value, 1, NcpNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpNumber attribute.</summary>
  [JsonPropertyName("ncpNumber")]
  [Computed]
  public string NcpNumber_Json
  {
    get => NullIf(NcpNumber, "");
    set => NcpNumber = value;
  }

  /// <summary>Length of the CLAIMANT_FIRST_NAME attribute.</summary>
  public const int ClaimantFirstName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = ClaimantFirstName_MaxLength)]
  public string ClaimantFirstName
  {
    get => claimantFirstName ?? "";
    set => claimantFirstName =
      TrimEnd(Substring(value, 1, ClaimantFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantFirstName attribute.</summary>
  [JsonPropertyName("claimantFirstName")]
  [Computed]
  public string ClaimantFirstName_Json
  {
    get => NullIf(ClaimantFirstName, "");
    set => ClaimantFirstName = value;
  }

  /// <summary>Length of the CLAIMANT_LAST_NAME attribute.</summary>
  public const int ClaimantLastName_MaxLength = 39;

  /// <summary>
  /// The value of the CLAIMANT_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ClaimantLastName_MaxLength)
    ]
  public string ClaimantLastName
  {
    get => claimantLastName ?? "";
    set => claimantLastName =
      TrimEnd(Substring(value, 1, ClaimantLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantLastName attribute.</summary>
  [JsonPropertyName("claimantLastName")]
  [Computed]
  public string ClaimantLastName_Json
  {
    get => NullIf(ClaimantLastName, "");
    set => ClaimantLastName = value;
  }

  /// <summary>Length of the CLAIMANT_MIDDLE_NAME attribute.</summary>
  public const int ClaimantMiddleName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = ClaimantMiddleName_MaxLength)]
  public string ClaimantMiddleName
  {
    get => claimantMiddleName ?? "";
    set => claimantMiddleName =
      TrimEnd(Substring(value, 1, ClaimantMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantMiddleName attribute.</summary>
  [JsonPropertyName("claimantMiddleName")]
  [Computed]
  public string ClaimantMiddleName_Json
  {
    get => NullIf(ClaimantMiddleName, "");
    set => ClaimantMiddleName = value;
  }

  /// <summary>Length of the CLAIMANT_STREET attribute.</summary>
  public const int ClaimantStreet_MaxLength = 55;

  /// <summary>
  /// The value of the CLAIMANT_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ClaimantStreet_MaxLength)]
  public string ClaimantStreet
  {
    get => claimantStreet ?? "";
    set => claimantStreet =
      TrimEnd(Substring(value, 1, ClaimantStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantStreet attribute.</summary>
  [JsonPropertyName("claimantStreet")]
  [Computed]
  public string ClaimantStreet_Json
  {
    get => NullIf(ClaimantStreet, "");
    set => ClaimantStreet = value;
  }

  /// <summary>Length of the CLAIMANT_CITY attribute.</summary>
  public const int ClaimantCity_MaxLength = 50;

  /// <summary>
  /// The value of the CLAIMANT_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ClaimantCity_MaxLength)]
  public string ClaimantCity
  {
    get => claimantCity ?? "";
    set => claimantCity = TrimEnd(Substring(value, 1, ClaimantCity_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantCity attribute.</summary>
  [JsonPropertyName("claimantCity")]
  [Computed]
  public string ClaimantCity_Json
  {
    get => NullIf(ClaimantCity, "");
    set => ClaimantCity = value;
  }

  /// <summary>Length of the CLAIMANT_STATE attribute.</summary>
  public const int ClaimantState_MaxLength = 2;

  /// <summary>
  /// The value of the CLAIMANT_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ClaimantState_MaxLength)]
  public string ClaimantState
  {
    get => claimantState ?? "";
    set => claimantState =
      TrimEnd(Substring(value, 1, ClaimantState_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantState attribute.</summary>
  [JsonPropertyName("claimantState")]
  [Computed]
  public string ClaimantState_Json
  {
    get => NullIf(ClaimantState, "");
    set => ClaimantState = value;
  }

  /// <summary>Length of the CLAIMANT_ZIP attribute.</summary>
  public const int ClaimantZip_MaxLength = 10;

  /// <summary>
  /// The value of the CLAIMANT_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ClaimantZip_MaxLength)]
  public string ClaimantZip
  {
    get => claimantZip ?? "";
    set => claimantZip = TrimEnd(Substring(value, 1, ClaimantZip_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantZip attribute.</summary>
  [JsonPropertyName("claimantZip")]
  [Computed]
  public string ClaimantZip_Json
  {
    get => NullIf(ClaimantZip, "");
    set => ClaimantZip = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_FIRST_NAME attribute.</summary>
  public const int ClaimantAttorneyFirstName_MaxLength = 11;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = ClaimantAttorneyFirstName_MaxLength)]
  public string ClaimantAttorneyFirstName
  {
    get => claimantAttorneyFirstName ?? "";
    set => claimantAttorneyFirstName =
      TrimEnd(Substring(value, 1, ClaimantAttorneyFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyFirstName attribute.</summary>
  [JsonPropertyName("claimantAttorneyFirstName")]
  [Computed]
  public string ClaimantAttorneyFirstName_Json
  {
    get => NullIf(ClaimantAttorneyFirstName, "");
    set => ClaimantAttorneyFirstName = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_LAST_NAME attribute.</summary>
  public const int ClaimantAttorneyLastName_MaxLength = 20;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ClaimantAttorneyLastName_MaxLength)]
  public string ClaimantAttorneyLastName
  {
    get => claimantAttorneyLastName ?? "";
    set => claimantAttorneyLastName =
      TrimEnd(Substring(value, 1, ClaimantAttorneyLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyLastName attribute.</summary>
  [JsonPropertyName("claimantAttorneyLastName")]
  [Computed]
  public string ClaimantAttorneyLastName_Json
  {
    get => NullIf(ClaimantAttorneyLastName, "");
    set => ClaimantAttorneyLastName = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_FIRM_NAME attribute.</summary>
  public const int ClaimantAttorneyFirmName_MaxLength = 50;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_FIRM_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = ClaimantAttorneyFirmName_MaxLength)]
  public string ClaimantAttorneyFirmName
  {
    get => claimantAttorneyFirmName ?? "";
    set => claimantAttorneyFirmName =
      TrimEnd(Substring(value, 1, ClaimantAttorneyFirmName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyFirmName attribute.</summary>
  [JsonPropertyName("claimantAttorneyFirmName")]
  [Computed]
  public string ClaimantAttorneyFirmName_Json
  {
    get => NullIf(ClaimantAttorneyFirmName, "");
    set => ClaimantAttorneyFirmName = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_STREET attribute.</summary>
  public const int ClaimantAttorneyStreet_MaxLength = 55;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ClaimantAttorneyStreet_MaxLength)]
  public string ClaimantAttorneyStreet
  {
    get => claimantAttorneyStreet ?? "";
    set => claimantAttorneyStreet =
      TrimEnd(Substring(value, 1, ClaimantAttorneyStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyStreet attribute.</summary>
  [JsonPropertyName("claimantAttorneyStreet")]
  [Computed]
  public string ClaimantAttorneyStreet_Json
  {
    get => NullIf(ClaimantAttorneyStreet, "");
    set => ClaimantAttorneyStreet = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_CITY attribute.</summary>
  public const int ClaimantAttorneyCity_MaxLength = 50;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = ClaimantAttorneyCity_MaxLength)]
  public string ClaimantAttorneyCity
  {
    get => claimantAttorneyCity ?? "";
    set => claimantAttorneyCity =
      TrimEnd(Substring(value, 1, ClaimantAttorneyCity_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyCity attribute.</summary>
  [JsonPropertyName("claimantAttorneyCity")]
  [Computed]
  public string ClaimantAttorneyCity_Json
  {
    get => NullIf(ClaimantAttorneyCity, "");
    set => ClaimantAttorneyCity = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_STATE attribute.</summary>
  public const int ClaimantAttorneyState_MaxLength = 2;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = ClaimantAttorneyState_MaxLength)]
  public string ClaimantAttorneyState
  {
    get => claimantAttorneyState ?? "";
    set => claimantAttorneyState =
      TrimEnd(Substring(value, 1, ClaimantAttorneyState_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyState attribute.</summary>
  [JsonPropertyName("claimantAttorneyState")]
  [Computed]
  public string ClaimantAttorneyState_Json
  {
    get => NullIf(ClaimantAttorneyState, "");
    set => ClaimantAttorneyState = value;
  }

  /// <summary>Length of the CLAIMANT_ATTORNEY_ZIP attribute.</summary>
  public const int ClaimantAttorneyZip_MaxLength = 10;

  /// <summary>
  /// The value of the CLAIMANT_ATTORNEY_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ClaimantAttorneyZip_MaxLength)]
  public string ClaimantAttorneyZip
  {
    get => claimantAttorneyZip ?? "";
    set => claimantAttorneyZip =
      TrimEnd(Substring(value, 1, ClaimantAttorneyZip_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimantAttorneyZip attribute.</summary>
  [JsonPropertyName("claimantAttorneyZip")]
  [Computed]
  public string ClaimantAttorneyZip_Json
  {
    get => NullIf(ClaimantAttorneyZip, "");
    set => ClaimantAttorneyZip = value;
  }

  /// <summary>Length of the EMPLOYER_NAME attribute.</summary>
  public const int EmployerName_MaxLength = 71;

  /// <summary>
  /// The value of the EMPLOYER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = EmployerName_MaxLength)]
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

  /// <summary>Length of the DOCKET_NUMBER attribute.</summary>
  public const int DocketNumber_MaxLength = 7;

  /// <summary>
  /// The value of the DOCKET_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = DocketNumber_MaxLength)]
  public string DocketNumber
  {
    get => docketNumber ?? "";
    set => docketNumber = TrimEnd(Substring(value, 1, DocketNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the DocketNumber attribute.</summary>
  [JsonPropertyName("docketNumber")]
  [Computed]
  public string DocketNumber_Json
  {
    get => NullIf(DocketNumber, "");
    set => DocketNumber = value;
  }

  /// <summary>Length of the INSURER_NAME attribute.</summary>
  public const int InsurerName_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = InsurerName_MaxLength)]
  public string InsurerName
  {
    get => insurerName ?? "";
    set => insurerName = TrimEnd(Substring(value, 1, InsurerName_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerName attribute.</summary>
  [JsonPropertyName("insurerName")]
  [Computed]
  public string InsurerName_Json
  {
    get => NullIf(InsurerName, "");
    set => InsurerName = value;
  }

  /// <summary>Length of the INSURER_STREET attribute.</summary>
  public const int InsurerStreet_MaxLength = 55;

  /// <summary>
  /// The value of the INSURER_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = InsurerStreet_MaxLength)]
  public string InsurerStreet
  {
    get => insurerStreet ?? "";
    set => insurerStreet =
      TrimEnd(Substring(value, 1, InsurerStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerStreet attribute.</summary>
  [JsonPropertyName("insurerStreet")]
  [Computed]
  public string InsurerStreet_Json
  {
    get => NullIf(InsurerStreet, "");
    set => InsurerStreet = value;
  }

  /// <summary>Length of the INSURER_CITY attribute.</summary>
  public const int InsurerCity_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = InsurerCity_MaxLength)]
  public string InsurerCity
  {
    get => insurerCity ?? "";
    set => insurerCity = TrimEnd(Substring(value, 1, InsurerCity_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerCity attribute.</summary>
  [JsonPropertyName("insurerCity")]
  [Computed]
  public string InsurerCity_Json
  {
    get => NullIf(InsurerCity, "");
    set => InsurerCity = value;
  }

  /// <summary>Length of the INSURER_STATE attribute.</summary>
  public const int InsurerState_MaxLength = 2;

  /// <summary>
  /// The value of the INSURER_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = InsurerState_MaxLength)]
  public string InsurerState
  {
    get => insurerState ?? "";
    set => insurerState = TrimEnd(Substring(value, 1, InsurerState_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerState attribute.</summary>
  [JsonPropertyName("insurerState")]
  [Computed]
  public string InsurerState_Json
  {
    get => NullIf(InsurerState, "");
    set => InsurerState = value;
  }

  /// <summary>Length of the INSURER_ZIP attribute.</summary>
  public const int InsurerZip_MaxLength = 10;

  /// <summary>
  /// The value of the INSURER_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = InsurerZip_MaxLength)]
  public string InsurerZip
  {
    get => insurerZip ?? "";
    set => insurerZip = TrimEnd(Substring(value, 1, InsurerZip_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerZip attribute.</summary>
  [JsonPropertyName("insurerZip")]
  [Computed]
  public string InsurerZip_Json
  {
    get => NullIf(InsurerZip, "");
    set => InsurerZip = value;
  }

  /// <summary>Length of the INSURER_ATTORNEY_FIRM_NAME attribute.</summary>
  public const int InsurerAttorneyFirmName_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_FIRM_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = InsurerAttorneyFirmName_MaxLength)]
  public string InsurerAttorneyFirmName
  {
    get => insurerAttorneyFirmName ?? "";
    set => insurerAttorneyFirmName =
      TrimEnd(Substring(value, 1, InsurerAttorneyFirmName_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerAttorneyFirmName attribute.</summary>
  [JsonPropertyName("insurerAttorneyFirmName")]
  [Computed]
  public string InsurerAttorneyFirmName_Json
  {
    get => NullIf(InsurerAttorneyFirmName, "");
    set => InsurerAttorneyFirmName = value;
  }

  /// <summary>Length of the INSURER_ATTORNEY_STREET attribute.</summary>
  public const int InsurerAttorneyStreet_MaxLength = 55;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = InsurerAttorneyStreet_MaxLength)]
  public string InsurerAttorneyStreet
  {
    get => insurerAttorneyStreet ?? "";
    set => insurerAttorneyStreet =
      TrimEnd(Substring(value, 1, InsurerAttorneyStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerAttorneyStreet attribute.</summary>
  [JsonPropertyName("insurerAttorneyStreet")]
  [Computed]
  public string InsurerAttorneyStreet_Json
  {
    get => NullIf(InsurerAttorneyStreet, "");
    set => InsurerAttorneyStreet = value;
  }

  /// <summary>Length of the INSURER_ATTORNEY_CITY attribute.</summary>
  public const int InsurerAttorneyCity_MaxLength = 50;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length
    = InsurerAttorneyCity_MaxLength)]
  public string InsurerAttorneyCity
  {
    get => insurerAttorneyCity ?? "";
    set => insurerAttorneyCity =
      TrimEnd(Substring(value, 1, InsurerAttorneyCity_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerAttorneyCity attribute.</summary>
  [JsonPropertyName("insurerAttorneyCity")]
  [Computed]
  public string InsurerAttorneyCity_Json
  {
    get => NullIf(InsurerAttorneyCity, "");
    set => InsurerAttorneyCity = value;
  }

  /// <summary>Length of the INSURER_ATTORNEY_STATE attribute.</summary>
  public const int InsurerAttorneyState_MaxLength = 2;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length
    = InsurerAttorneyState_MaxLength)]
  public string InsurerAttorneyState
  {
    get => insurerAttorneyState ?? "";
    set => insurerAttorneyState =
      TrimEnd(Substring(value, 1, InsurerAttorneyState_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerAttorneyState attribute.</summary>
  [JsonPropertyName("insurerAttorneyState")]
  [Computed]
  public string InsurerAttorneyState_Json
  {
    get => NullIf(InsurerAttorneyState, "");
    set => InsurerAttorneyState = value;
  }

  /// <summary>Length of the INSURER_ATTORNEY_ZIP attribute.</summary>
  public const int InsurerAttorneyZip_MaxLength = 10;

  /// <summary>
  /// The value of the INSURER_ATTORNEY_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = InsurerAttorneyZip_MaxLength)]
  public string InsurerAttorneyZip
  {
    get => insurerAttorneyZip ?? "";
    set => insurerAttorneyZip =
      TrimEnd(Substring(value, 1, InsurerAttorneyZip_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerAttorneyZip attribute.</summary>
  [JsonPropertyName("insurerAttorneyZip")]
  [Computed]
  public string InsurerAttorneyZip_Json
  {
    get => NullIf(InsurerAttorneyZip, "");
    set => InsurerAttorneyZip = value;
  }

  /// <summary>Length of the INSURER_CONTACT_NAME_1 attribute.</summary>
  public const int InsurerContactName1_MaxLength = 35;

  /// <summary>
  /// The value of the INSURER_CONTACT_NAME_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = InsurerContactName1_MaxLength)]
  public string InsurerContactName1
  {
    get => insurerContactName1 ?? "";
    set => insurerContactName1 =
      TrimEnd(Substring(value, 1, InsurerContactName1_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerContactName1 attribute.</summary>
  [JsonPropertyName("insurerContactName1")]
  [Computed]
  public string InsurerContactName1_Json
  {
    get => NullIf(InsurerContactName1, "");
    set => InsurerContactName1 = value;
  }

  /// <summary>Length of the INSURER_CONTACT_NAME_2 attribute.</summary>
  public const int InsurerContactName2_MaxLength = 35;

  /// <summary>
  /// The value of the INSURER_CONTACT_NAME_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = InsurerContactName2_MaxLength)]
  public string InsurerContactName2
  {
    get => insurerContactName2 ?? "";
    set => insurerContactName2 =
      TrimEnd(Substring(value, 1, InsurerContactName2_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerContactName2 attribute.</summary>
  [JsonPropertyName("insurerContactName2")]
  [Computed]
  public string InsurerContactName2_Json
  {
    get => NullIf(InsurerContactName2, "");
    set => InsurerContactName2 = value;
  }

  /// <summary>Length of the INSURER_CONTACT_PHONE attribute.</summary>
  public const int InsurerContactPhone_MaxLength = 20;

  /// <summary>
  /// The value of the INSURER_CONTACT_PHONE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = InsurerContactPhone_MaxLength)]
  public string InsurerContactPhone
  {
    get => insurerContactPhone ?? "";
    set => insurerContactPhone =
      TrimEnd(Substring(value, 1, InsurerContactPhone_MaxLength));
  }

  /// <summary>
  /// The json value of the InsurerContactPhone attribute.</summary>
  [JsonPropertyName("insurerContactPhone")]
  [Computed]
  public string InsurerContactPhone_Json
  {
    get => NullIf(InsurerContactPhone, "");
    set => InsurerContactPhone = value;
  }

  /// <summary>Length of the POLICY_NUMBER attribute.</summary>
  public const int PolicyNumber_MaxLength = 30;

  /// <summary>
  /// The value of the POLICY_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = PolicyNumber_MaxLength)]
  public string PolicyNumber
  {
    get => policyNumber ?? "";
    set => policyNumber = TrimEnd(Substring(value, 1, PolicyNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the PolicyNumber attribute.</summary>
  [JsonPropertyName("policyNumber")]
  [Computed]
  public string PolicyNumber_Json
  {
    get => NullIf(PolicyNumber, "");
    set => PolicyNumber = value;
  }

  /// <summary>Length of the DATE_OF_LOSS attribute.</summary>
  public const int DateOfLoss_MaxLength = 10;

  /// <summary>
  /// The value of the DATE_OF_LOSS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = DateOfLoss_MaxLength)]
  public string DateOfLoss
  {
    get => dateOfLoss ?? "";
    set => dateOfLoss = TrimEnd(Substring(value, 1, DateOfLoss_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfLoss attribute.</summary>
  [JsonPropertyName("dateOfLoss")]
  [Computed]
  public string DateOfLoss_Json
  {
    get => NullIf(DateOfLoss, "");
    set => DateOfLoss = value;
  }

  /// <summary>Length of the EMPLOYER_FEIN attribute.</summary>
  public const int EmployerFein_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYER_FEIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = EmployerFein_MaxLength)]
  public string EmployerFein
  {
    get => employerFein ?? "";
    set => employerFein = TrimEnd(Substring(value, 1, EmployerFein_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerFein attribute.</summary>
  [JsonPropertyName("employerFein")]
  [Computed]
  public string EmployerFein_Json
  {
    get => NullIf(EmployerFein, "");
    set => EmployerFein = value;
  }

  /// <summary>Length of the EMPLOYER_STREET attribute.</summary>
  public const int EmployerStreet_MaxLength = 55;

  /// <summary>
  /// The value of the EMPLOYER_STREET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = EmployerStreet_MaxLength)]
    
  public string EmployerStreet
  {
    get => employerStreet ?? "";
    set => employerStreet =
      TrimEnd(Substring(value, 1, EmployerStreet_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerStreet attribute.</summary>
  [JsonPropertyName("employerStreet")]
  [Computed]
  public string EmployerStreet_Json
  {
    get => NullIf(EmployerStreet, "");
    set => EmployerStreet = value;
  }

  /// <summary>Length of the EMPLOYER_CITY attribute.</summary>
  public const int EmployerCity_MaxLength = 50;

  /// <summary>
  /// The value of the EMPLOYER_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = EmployerCity_MaxLength)]
  public string EmployerCity
  {
    get => employerCity ?? "";
    set => employerCity = TrimEnd(Substring(value, 1, EmployerCity_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerCity attribute.</summary>
  [JsonPropertyName("employerCity")]
  [Computed]
  public string EmployerCity_Json
  {
    get => NullIf(EmployerCity, "");
    set => EmployerCity = value;
  }

  /// <summary>Length of the EMPLOYER_STATE attribute.</summary>
  public const int EmployerState_MaxLength = 2;

  /// <summary>
  /// The value of the EMPLOYER_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = EmployerState_MaxLength)]
  public string EmployerState
  {
    get => employerState ?? "";
    set => employerState =
      TrimEnd(Substring(value, 1, EmployerState_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerState attribute.</summary>
  [JsonPropertyName("employerState")]
  [Computed]
  public string EmployerState_Json
  {
    get => NullIf(EmployerState, "");
    set => EmployerState = value;
  }

  /// <summary>Length of the EMPLOYER_ZIP attribute.</summary>
  public const int EmployerZip_MaxLength = 10;

  /// <summary>
  /// The value of the EMPLOYER_ZIP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length = EmployerZip_MaxLength)]
  public string EmployerZip
  {
    get => employerZip ?? "";
    set => employerZip = TrimEnd(Substring(value, 1, EmployerZip_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerZip attribute.</summary>
  [JsonPropertyName("employerZip")]
  [Computed]
  public string EmployerZip_Json
  {
    get => NullIf(EmployerZip, "");
    set => EmployerZip = value;
  }

  /// <summary>Length of the DATE_OF_ACCIDENT attribute.</summary>
  public const int DateOfAccident_MaxLength = 10;

  /// <summary>
  /// The value of the DATE_OF_ACCIDENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = DateOfAccident_MaxLength)]
    
  public string DateOfAccident
  {
    get => dateOfAccident ?? "";
    set => dateOfAccident =
      TrimEnd(Substring(value, 1, DateOfAccident_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfAccident attribute.</summary>
  [JsonPropertyName("dateOfAccident")]
  [Computed]
  public string DateOfAccident_Json
  {
    get => NullIf(DateOfAccident, "");
    set => DateOfAccident = value;
  }

  /// <summary>Length of the WAGE_AMOUNT attribute.</summary>
  public const int WageAmount_MaxLength = 12;

  /// <summary>
  /// The value of the WAGE_AMOUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length = WageAmount_MaxLength)]
  public string WageAmount
  {
    get => wageAmount ?? "";
    set => wageAmount = TrimEnd(Substring(value, 1, WageAmount_MaxLength));
  }

  /// <summary>
  /// The json value of the WageAmount attribute.</summary>
  [JsonPropertyName("wageAmount")]
  [Computed]
  public string WageAmount_Json
  {
    get => NullIf(WageAmount, "");
    set => WageAmount = value;
  }

  /// <summary>Length of the ACCIDENT_CITY attribute.</summary>
  public const int AccidentCity_MaxLength = 25;

  /// <summary>
  /// The value of the ACCIDENT_CITY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length = AccidentCity_MaxLength)]
  public string AccidentCity
  {
    get => accidentCity ?? "";
    set => accidentCity = TrimEnd(Substring(value, 1, AccidentCity_MaxLength));
  }

  /// <summary>
  /// The json value of the AccidentCity attribute.</summary>
  [JsonPropertyName("accidentCity")]
  [Computed]
  public string AccidentCity_Json
  {
    get => NullIf(AccidentCity, "");
    set => AccidentCity = value;
  }

  /// <summary>Length of the ACCIDENT_COUNTY attribute.</summary>
  public const int AccidentCounty_MaxLength = 20;

  /// <summary>
  /// The value of the ACCIDENT_COUNTY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length = AccidentCounty_MaxLength)]
    
  public string AccidentCounty
  {
    get => accidentCounty ?? "";
    set => accidentCounty =
      TrimEnd(Substring(value, 1, AccidentCounty_MaxLength));
  }

  /// <summary>
  /// The json value of the AccidentCounty attribute.</summary>
  [JsonPropertyName("accidentCounty")]
  [Computed]
  public string AccidentCounty_Json
  {
    get => NullIf(AccidentCounty, "");
    set => AccidentCounty = value;
  }

  /// <summary>Length of the ACCIDENT_STATE attribute.</summary>
  public const int AccidentState_MaxLength = 2;

  /// <summary>
  /// The value of the ACCIDENT_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = AccidentState_MaxLength)]
  public string AccidentState
  {
    get => accidentState ?? "";
    set => accidentState =
      TrimEnd(Substring(value, 1, AccidentState_MaxLength));
  }

  /// <summary>
  /// The json value of the AccidentState attribute.</summary>
  [JsonPropertyName("accidentState")]
  [Computed]
  public string AccidentState_Json
  {
    get => NullIf(AccidentState, "");
    set => AccidentState = value;
  }

  /// <summary>Length of the ACCIDENT_DESCRIPTION attribute.</summary>
  public const int AccidentDescription_MaxLength = 500;

  /// <summary>
  /// The value of the ACCIDENT_DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Varchar, Length
    = AccidentDescription_MaxLength)]
  public string AccidentDescription
  {
    get => accidentDescription ?? "";
    set => accidentDescription =
      Substring(value, 1, AccidentDescription_MaxLength);
  }

  /// <summary>
  /// The json value of the AccidentDescription attribute.</summary>
  [JsonPropertyName("accidentDescription")]
  [Computed]
  public string AccidentDescription_Json
  {
    get => NullIf(AccidentDescription, "");
    set => AccidentDescription = value;
  }

  /// <summary>Length of the SEVERITY_CODE_DESCRIPTION attribute.</summary>
  public const int SeverityCodeDescription_MaxLength = 14;

  /// <summary>
  /// The value of the SEVERITY_CODE_DESCRIPTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = SeverityCodeDescription_MaxLength)]
  public string SeverityCodeDescription
  {
    get => severityCodeDescription ?? "";
    set => severityCodeDescription =
      TrimEnd(Substring(value, 1, SeverityCodeDescription_MaxLength));
  }

  /// <summary>
  /// The json value of the SeverityCodeDescription attribute.</summary>
  [JsonPropertyName("severityCodeDescription")]
  [Computed]
  public string SeverityCodeDescription_Json
  {
    get => NullIf(SeverityCodeDescription, "");
    set => SeverityCodeDescription = value;
  }

  /// <summary>Length of the RETURNED_TO_WORK_DATE attribute.</summary>
  public const int ReturnedToWorkDate_MaxLength = 10;

  /// <summary>
  /// The value of the RETURNED_TO_WORK_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = ReturnedToWorkDate_MaxLength)]
  public string ReturnedToWorkDate
  {
    get => returnedToWorkDate ?? "";
    set => returnedToWorkDate =
      TrimEnd(Substring(value, 1, ReturnedToWorkDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnedToWorkDate attribute.</summary>
  [JsonPropertyName("returnedToWorkDate")]
  [Computed]
  public string ReturnedToWorkDate_Json
  {
    get => NullIf(ReturnedToWorkDate, "");
    set => ReturnedToWorkDate = value;
  }

  /// <summary>Length of the COMPENSATION_PAID_FLAG attribute.</summary>
  public const int CompensationPaidFlag_MaxLength = 1;

  /// <summary>
  /// The value of the COMPENSATION_PAID_FLAG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = CompensationPaidFlag_MaxLength)]
  public string CompensationPaidFlag
  {
    get => compensationPaidFlag ?? "";
    set => compensationPaidFlag =
      TrimEnd(Substring(value, 1, CompensationPaidFlag_MaxLength));
  }

  /// <summary>
  /// The json value of the CompensationPaidFlag attribute.</summary>
  [JsonPropertyName("compensationPaidFlag")]
  [Computed]
  public string CompensationPaidFlag_Json
  {
    get => NullIf(CompensationPaidFlag, "");
    set => CompensationPaidFlag = value;
  }

  /// <summary>Length of the COMPENSATION_PAID_DATE attribute.</summary>
  public const int CompensationPaidDate_MaxLength = 10;

  /// <summary>
  /// The value of the COMPENSATION_PAID_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length
    = CompensationPaidDate_MaxLength)]
  public string CompensationPaidDate
  {
    get => compensationPaidDate ?? "";
    set => compensationPaidDate =
      TrimEnd(Substring(value, 1, CompensationPaidDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CompensationPaidDate attribute.</summary>
  [JsonPropertyName("compensationPaidDate")]
  [Computed]
  public string CompensationPaidDate_Json
  {
    get => NullIf(CompensationPaidDate, "");
    set => CompensationPaidDate = value;
  }

  /// <summary>Length of the WEEKLY_RATE attribute.</summary>
  public const int WeeklyRate_MaxLength = 12;

  /// <summary>
  /// The value of the WEEKLY_RATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length = WeeklyRate_MaxLength)]
  public string WeeklyRate
  {
    get => weeklyRate ?? "";
    set => weeklyRate = TrimEnd(Substring(value, 1, WeeklyRate_MaxLength));
  }

  /// <summary>
  /// The json value of the WeeklyRate attribute.</summary>
  [JsonPropertyName("weeklyRate")]
  [Computed]
  public string WeeklyRate_Json
  {
    get => NullIf(WeeklyRate, "");
    set => WeeklyRate = value;
  }

  /// <summary>Length of the DATE_OF_DEATH attribute.</summary>
  public const int DateOfDeath_MaxLength = 10;

  /// <summary>
  /// The value of the DATE_OF_DEATH attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length = DateOfDeath_MaxLength)]
  public string DateOfDeath
  {
    get => dateOfDeath ?? "";
    set => dateOfDeath = TrimEnd(Substring(value, 1, DateOfDeath_MaxLength));
  }

  /// <summary>
  /// The json value of the DateOfDeath attribute.</summary>
  [JsonPropertyName("dateOfDeath")]
  [Computed]
  public string DateOfDeath_Json
  {
    get => NullIf(DateOfDeath, "");
    set => DateOfDeath = value;
  }

  /// <summary>Length of the THIRD_PARTY_ADMINISTRATOR_NAME attribute.</summary>
  public const int ThirdPartyAdministratorName_MaxLength = 40;

  /// <summary>
  /// The value of the THIRD_PARTY_ADMINISTRATOR_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = ThirdPartyAdministratorName_MaxLength)]
  public string ThirdPartyAdministratorName
  {
    get => thirdPartyAdministratorName ?? "";
    set => thirdPartyAdministratorName =
      TrimEnd(Substring(value, 1, ThirdPartyAdministratorName_MaxLength));
  }

  /// <summary>
  /// The json value of the ThirdPartyAdministratorName attribute.</summary>
  [JsonPropertyName("thirdPartyAdministratorName")]
  [Computed]
  public string ThirdPartyAdministratorName_Json
  {
    get => NullIf(ThirdPartyAdministratorName, "");
    set => ThirdPartyAdministratorName = value;
  }

  /// <summary>Length of the ADMINISTRATIVE_CLAIM_NUMBER attribute.</summary>
  public const int AdministrativeClaimNumber_MaxLength = 25;

  /// <summary>
  /// The value of the ADMINISTRATIVE_CLAIM_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = AdministrativeClaimNumber_MaxLength)]
  public string AdministrativeClaimNumber
  {
    get => administrativeClaimNumber ?? "";
    set => administrativeClaimNumber =
      TrimEnd(Substring(value, 1, AdministrativeClaimNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AdministrativeClaimNumber attribute.</summary>
  [JsonPropertyName("administrativeClaimNumber")]
  [Computed]
  public string AdministrativeClaimNumber_Json
  {
    get => NullIf(AdministrativeClaimNumber, "");
    set => AdministrativeClaimNumber = value;
  }

  /// <summary>Length of the CLAIM_FILED_DATE attribute.</summary>
  public const int ClaimFiledDate_MaxLength = 10;

  /// <summary>
  /// The value of the CLAIM_FILED_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = ClaimFiledDate_MaxLength)]
    
  public string ClaimFiledDate
  {
    get => claimFiledDate ?? "";
    set => claimFiledDate =
      TrimEnd(Substring(value, 1, ClaimFiledDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimFiledDate attribute.</summary>
  [JsonPropertyName("claimFiledDate")]
  [Computed]
  public string ClaimFiledDate_Json
  {
    get => NullIf(ClaimFiledDate, "");
    set => ClaimFiledDate = value;
  }

  /// <summary>Length of the AGENCY_CLAIM_NO attribute.</summary>
  public const int AgencyClaimNo_MaxLength = 12;

  /// <summary>
  /// The value of the AGENCY_CLAIM_NO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length = AgencyClaimNo_MaxLength)]
  public string AgencyClaimNo
  {
    get => agencyClaimNo ?? "";
    set => agencyClaimNo =
      TrimEnd(Substring(value, 1, AgencyClaimNo_MaxLength));
  }

  /// <summary>
  /// The json value of the AgencyClaimNo attribute.</summary>
  [JsonPropertyName("agencyClaimNo")]
  [Computed]
  public string AgencyClaimNo_Json
  {
    get => NullIf(AgencyClaimNo, "");
    set => AgencyClaimNo = value;
  }

  private string ncpNumber;
  private string claimantFirstName;
  private string claimantLastName;
  private string claimantMiddleName;
  private string claimantStreet;
  private string claimantCity;
  private string claimantState;
  private string claimantZip;
  private string claimantAttorneyFirstName;
  private string claimantAttorneyLastName;
  private string claimantAttorneyFirmName;
  private string claimantAttorneyStreet;
  private string claimantAttorneyCity;
  private string claimantAttorneyState;
  private string claimantAttorneyZip;
  private string employerName;
  private string docketNumber;
  private string insurerName;
  private string insurerStreet;
  private string insurerCity;
  private string insurerState;
  private string insurerZip;
  private string insurerAttorneyFirmName;
  private string insurerAttorneyStreet;
  private string insurerAttorneyCity;
  private string insurerAttorneyState;
  private string insurerAttorneyZip;
  private string insurerContactName1;
  private string insurerContactName2;
  private string insurerContactPhone;
  private string policyNumber;
  private string dateOfLoss;
  private string employerFein;
  private string employerStreet;
  private string employerCity;
  private string employerState;
  private string employerZip;
  private string dateOfAccident;
  private string wageAmount;
  private string accidentCity;
  private string accidentCounty;
  private string accidentState;
  private string accidentDescription;
  private string severityCodeDescription;
  private string returnedToWorkDate;
  private string compensationPaidFlag;
  private string compensationPaidDate;
  private string weeklyRate;
  private string dateOfDeath;
  private string thirdPartyAdministratorName;
  private string administrativeClaimNumber;
  private string claimFiledDate;
  private string agencyClaimNo;
}

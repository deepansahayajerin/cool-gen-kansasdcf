// The source file: DMDC_PRO_MATCH_RESPONSE, ID: 371307998, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class DmdcProMatchResponse: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DmdcProMatchResponse()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DmdcProMatchResponse(DmdcProMatchResponse that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DmdcProMatchResponse Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DmdcProMatchResponse that)
  {
    base.Assign(that);
    recordId = that.recordId;
    transmitterStateCode = that.transmitterStateCode;
    fipsCountyCode = that.fipsCountyCode;
    caseId = that.caseId;
    orderIndicator = that.orderIndicator;
    chFirstName = that.chFirstName;
    chMiddleName = that.chMiddleName;
    chLastName = that.chLastName;
    chSsn = that.chSsn;
    chSsnVerifiedInd = that.chSsnVerifiedInd;
    chMemberId = that.chMemberId;
    chDeathIndicator = that.chDeathIndicator;
    chMedicalCoverageIndicator = that.chMedicalCoverageIndicator;
    chMedicalCoverageSponsorCode = that.chMedicalCoverageSponsorCode;
    chMedicalCoverageBeginDate = that.chMedicalCoverageBeginDate;
    chMedicalCoverageEndDate = that.chMedicalCoverageEndDate;
    ncpFirstName = that.ncpFirstName;
    ncpMiddleName = that.ncpMiddleName;
    ncpLastName = that.ncpLastName;
    ncpSsn = that.ncpSsn;
    ncpSsnVerifiedIndicator = that.ncpSsnVerifiedIndicator;
    ncpMemberId = that.ncpMemberId;
    ncpDeathIndicator = that.ncpDeathIndicator;
    ncpInTheMilitaryIndicator = that.ncpInTheMilitaryIndicator;
    pfFirstName = that.pfFirstName;
    pfMiddleName = that.pfMiddleName;
    pfLastName = that.pfLastName;
    pfSsn = that.pfSsn;
    pfSsnVerifiedIndicator = that.pfSsnVerifiedIndicator;
    pfMemberId = that.pfMemberId;
    pfDeathIndicator = that.pfDeathIndicator;
    pfInTheMilitaryIndicator = that.pfInTheMilitaryIndicator;
    cpFirstName = that.cpFirstName;
    cpMiddleName = that.cpMiddleName;
    cpLastName = that.cpLastName;
    cpSsn = that.cpSsn;
    cpSsnVerifiedIndicator = that.cpSsnVerifiedIndicator;
    cpMemberId = that.cpMemberId;
    cpDeathIndicator = that.cpDeathIndicator;
    cpInTheMilitaryIndicator = that.cpInTheMilitaryIndicator;
    chSponsorRelCode = that.chSponsorRelCode;
    chSponsorSsn = that.chSponsorSsn;
    chSponsorLastName = that.chSponsorLastName;
    chSponsorFirstName = that.chSponsorFirstName;
    chSponsorMiddleName = that.chSponsorMiddleName;
    chSponsorLastNameSuffix = that.chSponsorLastNameSuffix;
  }

  /// <summary>Length of the RECORD_ID attribute.</summary>
  public const int RecordId_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordId_MaxLength)]
  public string RecordId
  {
    get => recordId ?? "";
    set => recordId = TrimEnd(Substring(value, 1, RecordId_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordId attribute.</summary>
  [JsonPropertyName("recordId")]
  [Computed]
  public string RecordId_Json
  {
    get => NullIf(RecordId, "");
    set => RecordId = value;
  }

  /// <summary>Length of the TRANSMITTER_STATE_CODE attribute.</summary>
  public const int TransmitterStateCode_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSMITTER_STATE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = TransmitterStateCode_MaxLength)]
  public string TransmitterStateCode
  {
    get => transmitterStateCode ?? "";
    set => transmitterStateCode =
      TrimEnd(Substring(value, 1, TransmitterStateCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TransmitterStateCode attribute.</summary>
  [JsonPropertyName("transmitterStateCode")]
  [Computed]
  public string TransmitterStateCode_Json
  {
    get => NullIf(TransmitterStateCode, "");
    set => TransmitterStateCode = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
  public string FipsCountyCode
  {
    get => fipsCountyCode ?? "";
    set => fipsCountyCode =
      TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => caseId ?? "";
    set => caseId = TrimEnd(Substring(value, 1, CaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the ORDER_INDICATOR attribute.</summary>
  public const int OrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = OrderIndicator_MaxLength)]
  public string OrderIndicator
  {
    get => orderIndicator ?? "";
    set => orderIndicator =
      TrimEnd(Substring(value, 1, OrderIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the OrderIndicator attribute.</summary>
  [JsonPropertyName("orderIndicator")]
  [Computed]
  public string OrderIndicator_Json
  {
    get => NullIf(OrderIndicator, "");
    set => OrderIndicator = value;
  }

  /// <summary>Length of the CH_FIRST_NAME attribute.</summary>
  public const int ChFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the CH_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ChFirstName_MaxLength)]
  public string ChFirstName
  {
    get => chFirstName ?? "";
    set => chFirstName = TrimEnd(Substring(value, 1, ChFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChFirstName attribute.</summary>
  [JsonPropertyName("chFirstName")]
  [Computed]
  public string ChFirstName_Json
  {
    get => NullIf(ChFirstName, "");
    set => ChFirstName = value;
  }

  /// <summary>Length of the CH_MIDDLE_NAME attribute.</summary>
  public const int ChMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the CH_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ChMiddleName_MaxLength)]
  public string ChMiddleName
  {
    get => chMiddleName ?? "";
    set => chMiddleName = TrimEnd(Substring(value, 1, ChMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChMiddleName attribute.</summary>
  [JsonPropertyName("chMiddleName")]
  [Computed]
  public string ChMiddleName_Json
  {
    get => NullIf(ChMiddleName, "");
    set => ChMiddleName = value;
  }

  /// <summary>Length of the CH_LAST_NAME attribute.</summary>
  public const int ChLastName_MaxLength = 30;

  /// <summary>
  /// The value of the CH_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ChLastName_MaxLength)]
  public string ChLastName
  {
    get => chLastName ?? "";
    set => chLastName = TrimEnd(Substring(value, 1, ChLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChLastName attribute.</summary>
  [JsonPropertyName("chLastName")]
  [Computed]
  public string ChLastName_Json
  {
    get => NullIf(ChLastName, "");
    set => ChLastName = value;
  }

  /// <summary>Length of the CH_SSN attribute.</summary>
  public const int ChSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CH_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ChSsn_MaxLength)]
  public string ChSsn
  {
    get => chSsn ?? "";
    set => chSsn = TrimEnd(Substring(value, 1, ChSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSsn attribute.</summary>
  [JsonPropertyName("chSsn")]
  [Computed]
  public string ChSsn_Json
  {
    get => NullIf(ChSsn, "");
    set => ChSsn = value;
  }

  /// <summary>Length of the CH_SSN_VERIFIED_IND attribute.</summary>
  public const int ChSsnVerifiedInd_MaxLength = 1;

  /// <summary>
  /// The value of the CH_SSN_VERIFIED_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ChSsnVerifiedInd_MaxLength)]
  public string ChSsnVerifiedInd
  {
    get => chSsnVerifiedInd ?? "";
    set => chSsnVerifiedInd =
      TrimEnd(Substring(value, 1, ChSsnVerifiedInd_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSsnVerifiedInd attribute.</summary>
  [JsonPropertyName("chSsnVerifiedInd")]
  [Computed]
  public string ChSsnVerifiedInd_Json
  {
    get => NullIf(ChSsnVerifiedInd, "");
    set => ChSsnVerifiedInd = value;
  }

  /// <summary>
  /// The value of the CH_MEMBER_ID attribute.
  /// </summary>
  [JsonPropertyName("chMemberId")]
  [DefaultValue(0L)]
  [Member(Index = 11, Type = MemberType.Number, Length = 15)]
  public long ChMemberId
  {
    get => chMemberId;
    set => chMemberId = value;
  }

  /// <summary>Length of the CH_DEATH_INDICATOR attribute.</summary>
  public const int ChDeathIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CH_DEATH_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = ChDeathIndicator_MaxLength)]
  public string ChDeathIndicator
  {
    get => chDeathIndicator ?? "";
    set => chDeathIndicator =
      TrimEnd(Substring(value, 1, ChDeathIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ChDeathIndicator attribute.</summary>
  [JsonPropertyName("chDeathIndicator")]
  [Computed]
  public string ChDeathIndicator_Json
  {
    get => NullIf(ChDeathIndicator, "");
    set => ChDeathIndicator = value;
  }

  /// <summary>Length of the CH_MEDICAL_COVERAGE_INDICATOR attribute.</summary>
  public const int ChMedicalCoverageIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CH_MEDICAL_COVERAGE_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = ChMedicalCoverageIndicator_MaxLength)]
  public string ChMedicalCoverageIndicator
  {
    get => chMedicalCoverageIndicator ?? "";
    set => chMedicalCoverageIndicator =
      TrimEnd(Substring(value, 1, ChMedicalCoverageIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the ChMedicalCoverageIndicator attribute.</summary>
  [JsonPropertyName("chMedicalCoverageIndicator")]
  [Computed]
  public string ChMedicalCoverageIndicator_Json
  {
    get => NullIf(ChMedicalCoverageIndicator, "");
    set => ChMedicalCoverageIndicator = value;
  }

  /// <summary>Length of the CH_MEDICAL_COVERAGE_SPONSOR_CODE attribute.
  /// </summary>
  public const int ChMedicalCoverageSponsorCode_MaxLength = 1;

  /// <summary>
  /// The value of the CH_MEDICAL_COVERAGE_SPONSOR_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = ChMedicalCoverageSponsorCode_MaxLength)]
  public string ChMedicalCoverageSponsorCode
  {
    get => chMedicalCoverageSponsorCode ?? "";
    set => chMedicalCoverageSponsorCode =
      TrimEnd(Substring(value, 1, ChMedicalCoverageSponsorCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ChMedicalCoverageSponsorCode attribute.</summary>
  [JsonPropertyName("chMedicalCoverageSponsorCode")]
  [Computed]
  public string ChMedicalCoverageSponsorCode_Json
  {
    get => NullIf(ChMedicalCoverageSponsorCode, "");
    set => ChMedicalCoverageSponsorCode = value;
  }

  /// <summary>
  /// The value of the CH_MEDICAL_COVERAGE_BEGIN_DATE attribute.
  /// </summary>
  [JsonPropertyName("chMedicalCoverageBeginDate")]
  [Member(Index = 15, Type = MemberType.Date)]
  public DateTime? ChMedicalCoverageBeginDate
  {
    get => chMedicalCoverageBeginDate;
    set => chMedicalCoverageBeginDate = value;
  }

  /// <summary>
  /// The value of the CH_MEDICAL_COVERAGE_END_DATE attribute.
  /// </summary>
  [JsonPropertyName("chMedicalCoverageEndDate")]
  [Member(Index = 16, Type = MemberType.Date)]
  public DateTime? ChMedicalCoverageEndDate
  {
    get => chMedicalCoverageEndDate;
    set => chMedicalCoverageEndDate = value;
  }

  /// <summary>Length of the NCP_FIRST_NAME attribute.</summary>
  public const int NcpFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the NCP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = NcpFirstName_MaxLength)]
  public string NcpFirstName
  {
    get => ncpFirstName ?? "";
    set => ncpFirstName = TrimEnd(Substring(value, 1, NcpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpFirstName attribute.</summary>
  [JsonPropertyName("ncpFirstName")]
  [Computed]
  public string NcpFirstName_Json
  {
    get => NullIf(NcpFirstName, "");
    set => NcpFirstName = value;
  }

  /// <summary>Length of the NCP_MIDDLE_NAME attribute.</summary>
  public const int NcpMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the NCP_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = NcpMiddleName_MaxLength)]
  public string NcpMiddleName
  {
    get => ncpMiddleName ?? "";
    set => ncpMiddleName =
      TrimEnd(Substring(value, 1, NcpMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpMiddleName attribute.</summary>
  [JsonPropertyName("ncpMiddleName")]
  [Computed]
  public string NcpMiddleName_Json
  {
    get => NullIf(NcpMiddleName, "");
    set => NcpMiddleName = value;
  }

  /// <summary>Length of the NCP_LAST_NAME attribute.</summary>
  public const int NcpLastName_MaxLength = 30;

  /// <summary>
  /// The value of the NCP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = NcpLastName_MaxLength)]
  public string NcpLastName
  {
    get => ncpLastName ?? "";
    set => ncpLastName = TrimEnd(Substring(value, 1, NcpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpLastName attribute.</summary>
  [JsonPropertyName("ncpLastName")]
  [Computed]
  public string NcpLastName_Json
  {
    get => NullIf(NcpLastName, "");
    set => NcpLastName = value;
  }

  /// <summary>Length of the NCP_SSN attribute.</summary>
  public const int NcpSsn_MaxLength = 9;

  /// <summary>
  /// The value of the NCP_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = NcpSsn_MaxLength)]
  public string NcpSsn
  {
    get => ncpSsn ?? "";
    set => ncpSsn = TrimEnd(Substring(value, 1, NcpSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpSsn attribute.</summary>
  [JsonPropertyName("ncpSsn")]
  [Computed]
  public string NcpSsn_Json
  {
    get => NullIf(NcpSsn, "");
    set => NcpSsn = value;
  }

  /// <summary>Length of the NCP_SSN_VERIFIED_INDICATOR attribute.</summary>
  public const int NcpSsnVerifiedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_SSN_VERIFIED_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = NcpSsnVerifiedIndicator_MaxLength)]
  public string NcpSsnVerifiedIndicator
  {
    get => ncpSsnVerifiedIndicator ?? "";
    set => ncpSsnVerifiedIndicator =
      TrimEnd(Substring(value, 1, NcpSsnVerifiedIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpSsnVerifiedIndicator attribute.</summary>
  [JsonPropertyName("ncpSsnVerifiedIndicator")]
  [Computed]
  public string NcpSsnVerifiedIndicator_Json
  {
    get => NullIf(NcpSsnVerifiedIndicator, "");
    set => NcpSsnVerifiedIndicator = value;
  }

  /// <summary>
  /// The value of the NCP_MEMBER_ID attribute.
  /// </summary>
  [JsonPropertyName("ncpMemberId")]
  [DefaultValue(0L)]
  [Member(Index = 22, Type = MemberType.Number, Length = 15)]
  public long NcpMemberId
  {
    get => ncpMemberId;
    set => ncpMemberId = value;
  }

  /// <summary>Length of the NCP_DEATH_INDICATOR attribute.</summary>
  public const int NcpDeathIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_DEATH_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = NcpDeathIndicator_MaxLength)]
  public string NcpDeathIndicator
  {
    get => ncpDeathIndicator ?? "";
    set => ncpDeathIndicator =
      TrimEnd(Substring(value, 1, NcpDeathIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpDeathIndicator attribute.</summary>
  [JsonPropertyName("ncpDeathIndicator")]
  [Computed]
  public string NcpDeathIndicator_Json
  {
    get => NullIf(NcpDeathIndicator, "");
    set => NcpDeathIndicator = value;
  }

  /// <summary>Length of the NCP_IN_THE_MILITARY_INDICATOR attribute.</summary>
  public const int NcpInTheMilitaryIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_IN_THE_MILITARY_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = NcpInTheMilitaryIndicator_MaxLength)]
  public string NcpInTheMilitaryIndicator
  {
    get => ncpInTheMilitaryIndicator ?? "";
    set => ncpInTheMilitaryIndicator =
      TrimEnd(Substring(value, 1, NcpInTheMilitaryIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NcpInTheMilitaryIndicator attribute.</summary>
  [JsonPropertyName("ncpInTheMilitaryIndicator")]
  [Computed]
  public string NcpInTheMilitaryIndicator_Json
  {
    get => NullIf(NcpInTheMilitaryIndicator, "");
    set => NcpInTheMilitaryIndicator = value;
  }

  /// <summary>Length of the PF_FIRST_NAME attribute.</summary>
  public const int PfFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the PF_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = PfFirstName_MaxLength)]
  public string PfFirstName
  {
    get => pfFirstName ?? "";
    set => pfFirstName = TrimEnd(Substring(value, 1, PfFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the PfFirstName attribute.</summary>
  [JsonPropertyName("pfFirstName")]
  [Computed]
  public string PfFirstName_Json
  {
    get => NullIf(PfFirstName, "");
    set => PfFirstName = value;
  }

  /// <summary>Length of the PF_MIDDLE_NAME attribute.</summary>
  public const int PfMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the PF_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = PfMiddleName_MaxLength)]
  public string PfMiddleName
  {
    get => pfMiddleName ?? "";
    set => pfMiddleName = TrimEnd(Substring(value, 1, PfMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the PfMiddleName attribute.</summary>
  [JsonPropertyName("pfMiddleName")]
  [Computed]
  public string PfMiddleName_Json
  {
    get => NullIf(PfMiddleName, "");
    set => PfMiddleName = value;
  }

  /// <summary>Length of the PF_LAST_NAME attribute.</summary>
  public const int PfLastName_MaxLength = 30;

  /// <summary>
  /// The value of the PF_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = PfLastName_MaxLength)]
  public string PfLastName
  {
    get => pfLastName ?? "";
    set => pfLastName = TrimEnd(Substring(value, 1, PfLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the PfLastName attribute.</summary>
  [JsonPropertyName("pfLastName")]
  [Computed]
  public string PfLastName_Json
  {
    get => NullIf(PfLastName, "");
    set => PfLastName = value;
  }

  /// <summary>Length of the PF_SSN attribute.</summary>
  public const int PfSsn_MaxLength = 9;

  /// <summary>
  /// The value of the PF_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = PfSsn_MaxLength)]
  public string PfSsn
  {
    get => pfSsn ?? "";
    set => pfSsn = TrimEnd(Substring(value, 1, PfSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the PfSsn attribute.</summary>
  [JsonPropertyName("pfSsn")]
  [Computed]
  public string PfSsn_Json
  {
    get => NullIf(PfSsn, "");
    set => PfSsn = value;
  }

  /// <summary>Length of the PF_SSN_VERIFIED_INDICATOR attribute.</summary>
  public const int PfSsnVerifiedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PF_SSN_VERIFIED_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = PfSsnVerifiedIndicator_MaxLength)]
  public string PfSsnVerifiedIndicator
  {
    get => pfSsnVerifiedIndicator ?? "";
    set => pfSsnVerifiedIndicator =
      TrimEnd(Substring(value, 1, PfSsnVerifiedIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the PfSsnVerifiedIndicator attribute.</summary>
  [JsonPropertyName("pfSsnVerifiedIndicator")]
  [Computed]
  public string PfSsnVerifiedIndicator_Json
  {
    get => NullIf(PfSsnVerifiedIndicator, "");
    set => PfSsnVerifiedIndicator = value;
  }

  /// <summary>
  /// The value of the PF_MEMBER_ID attribute.
  /// </summary>
  [JsonPropertyName("pfMemberId")]
  [DefaultValue(0L)]
  [Member(Index = 30, Type = MemberType.Number, Length = 15)]
  public long PfMemberId
  {
    get => pfMemberId;
    set => pfMemberId = value;
  }

  /// <summary>Length of the PF_DEATH_INDICATOR attribute.</summary>
  public const int PfDeathIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PF_DEATH_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = PfDeathIndicator_MaxLength)]
  public string PfDeathIndicator
  {
    get => pfDeathIndicator ?? "";
    set => pfDeathIndicator =
      TrimEnd(Substring(value, 1, PfDeathIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the PfDeathIndicator attribute.</summary>
  [JsonPropertyName("pfDeathIndicator")]
  [Computed]
  public string PfDeathIndicator_Json
  {
    get => NullIf(PfDeathIndicator, "");
    set => PfDeathIndicator = value;
  }

  /// <summary>Length of the PF_IN_THE_MILITARY_INDICATOR attribute.</summary>
  public const int PfInTheMilitaryIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the PF_IN_THE_MILITARY_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = PfInTheMilitaryIndicator_MaxLength)]
  public string PfInTheMilitaryIndicator
  {
    get => pfInTheMilitaryIndicator ?? "";
    set => pfInTheMilitaryIndicator =
      TrimEnd(Substring(value, 1, PfInTheMilitaryIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the PfInTheMilitaryIndicator attribute.</summary>
  [JsonPropertyName("pfInTheMilitaryIndicator")]
  [Computed]
  public string PfInTheMilitaryIndicator_Json
  {
    get => NullIf(PfInTheMilitaryIndicator, "");
    set => PfInTheMilitaryIndicator = value;
  }

  /// <summary>Length of the CP_FIRST_NAME attribute.</summary>
  public const int CpFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the CP_FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length = CpFirstName_MaxLength)]
  public string CpFirstName
  {
    get => cpFirstName ?? "";
    set => cpFirstName = TrimEnd(Substring(value, 1, CpFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpFirstName attribute.</summary>
  [JsonPropertyName("cpFirstName")]
  [Computed]
  public string CpFirstName_Json
  {
    get => NullIf(CpFirstName, "");
    set => CpFirstName = value;
  }

  /// <summary>Length of the CP_MIDDLE_NAME attribute.</summary>
  public const int CpMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the CP_MIDDLE_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length = CpMiddleName_MaxLength)]
  public string CpMiddleName
  {
    get => cpMiddleName ?? "";
    set => cpMiddleName = TrimEnd(Substring(value, 1, CpMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpMiddleName attribute.</summary>
  [JsonPropertyName("cpMiddleName")]
  [Computed]
  public string CpMiddleName_Json
  {
    get => NullIf(CpMiddleName, "");
    set => CpMiddleName = value;
  }

  /// <summary>Length of the CP_LAST_NAME attribute.</summary>
  public const int CpLastName_MaxLength = 30;

  /// <summary>
  /// The value of the CP_LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = CpLastName_MaxLength)]
  public string CpLastName
  {
    get => cpLastName ?? "";
    set => cpLastName = TrimEnd(Substring(value, 1, CpLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the CpLastName attribute.</summary>
  [JsonPropertyName("cpLastName")]
  [Computed]
  public string CpLastName_Json
  {
    get => NullIf(CpLastName, "");
    set => CpLastName = value;
  }

  /// <summary>Length of the CP_SSN attribute.</summary>
  public const int CpSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CP_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = CpSsn_MaxLength)]
  public string CpSsn
  {
    get => cpSsn ?? "";
    set => cpSsn = TrimEnd(Substring(value, 1, CpSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CpSsn attribute.</summary>
  [JsonPropertyName("cpSsn")]
  [Computed]
  public string CpSsn_Json
  {
    get => NullIf(CpSsn, "");
    set => CpSsn = value;
  }

  /// <summary>Length of the CP_SSN_VERIFIED_INDICATOR attribute.</summary>
  public const int CpSsnVerifiedIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CP_SSN_VERIFIED_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = CpSsnVerifiedIndicator_MaxLength)]
  public string CpSsnVerifiedIndicator
  {
    get => cpSsnVerifiedIndicator ?? "";
    set => cpSsnVerifiedIndicator =
      TrimEnd(Substring(value, 1, CpSsnVerifiedIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CpSsnVerifiedIndicator attribute.</summary>
  [JsonPropertyName("cpSsnVerifiedIndicator")]
  [Computed]
  public string CpSsnVerifiedIndicator_Json
  {
    get => NullIf(CpSsnVerifiedIndicator, "");
    set => CpSsnVerifiedIndicator = value;
  }

  /// <summary>
  /// The value of the CP_MEMBER_ID attribute.
  /// </summary>
  [JsonPropertyName("cpMemberId")]
  [DefaultValue(0L)]
  [Member(Index = 38, Type = MemberType.Number, Length = 15)]
  public long CpMemberId
  {
    get => cpMemberId;
    set => cpMemberId = value;
  }

  /// <summary>Length of the CP_DEATH_INDICATOR attribute.</summary>
  public const int CpDeathIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CP_DEATH_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = CpDeathIndicator_MaxLength)]
  public string CpDeathIndicator
  {
    get => cpDeathIndicator ?? "";
    set => cpDeathIndicator =
      TrimEnd(Substring(value, 1, CpDeathIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CpDeathIndicator attribute.</summary>
  [JsonPropertyName("cpDeathIndicator")]
  [Computed]
  public string CpDeathIndicator_Json
  {
    get => NullIf(CpDeathIndicator, "");
    set => CpDeathIndicator = value;
  }

  /// <summary>Length of the CP_IN_THE_MILITARY_INDICATOR attribute.</summary>
  public const int CpInTheMilitaryIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CP_IN_THE_MILITARY_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = CpInTheMilitaryIndicator_MaxLength)]
  public string CpInTheMilitaryIndicator
  {
    get => cpInTheMilitaryIndicator ?? "";
    set => cpInTheMilitaryIndicator =
      TrimEnd(Substring(value, 1, CpInTheMilitaryIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CpInTheMilitaryIndicator attribute.</summary>
  [JsonPropertyName("cpInTheMilitaryIndicator")]
  [Computed]
  public string CpInTheMilitaryIndicator_Json
  {
    get => NullIf(CpInTheMilitaryIndicator, "");
    set => CpInTheMilitaryIndicator = value;
  }

  /// <summary>Length of the CH_SPONSOR_REL_CODE attribute.</summary>
  public const int ChSponsorRelCode_MaxLength = 1;

  /// <summary>
  /// The value of the CH_SPONSOR_REL_CODE attribute.
  /// CH Sponsor Relationship Code - DMDC FCR file (position 521) valid values 
  /// are:
  /// ' ' - None
  /// 1 -  Child
  /// 2 -  Foster Child
  /// 3 -  Pre-Adoptive Child
  /// 4 -  Ward
  /// 5 -  Step Child
  /// 6 -  Self
  /// 7 -  Spouse
  /// 8 -  Other/Unknown
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = ChSponsorRelCode_MaxLength)]
  public string ChSponsorRelCode
  {
    get => chSponsorRelCode ?? "";
    set => chSponsorRelCode =
      TrimEnd(Substring(value, 1, ChSponsorRelCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorRelCode attribute.</summary>
  [JsonPropertyName("chSponsorRelCode")]
  [Computed]
  public string ChSponsorRelCode_Json
  {
    get => NullIf(ChSponsorRelCode, "");
    set => ChSponsorRelCode = value;
  }

  /// <summary>Length of the CH_SPONSOR_SSN attribute.</summary>
  public const int ChSponsorSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CH_SPONSOR_SSN attribute.
  /// Child Sponsor SSN# - FCR File Position 522-530.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length = ChSponsorSsn_MaxLength)]
  public string ChSponsorSsn
  {
    get => chSponsorSsn ?? "";
    set => chSponsorSsn = TrimEnd(Substring(value, 1, ChSponsorSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorSsn attribute.</summary>
  [JsonPropertyName("chSponsorSsn")]
  [Computed]
  public string ChSponsorSsn_Json
  {
    get => NullIf(ChSponsorSsn, "");
    set => ChSponsorSsn = value;
  }

  /// <summary>Length of the CH_SPONSOR_LAST_NAME attribute.</summary>
  public const int ChSponsorLastName_MaxLength = 30;

  /// <summary>
  /// The value of the CH_SPONSOR_LAST_NAME attribute.
  /// Child Sponsor Last Name - FCR File Position 531-560.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = ChSponsorLastName_MaxLength)]
  public string ChSponsorLastName
  {
    get => chSponsorLastName ?? "";
    set => chSponsorLastName =
      TrimEnd(Substring(value, 1, ChSponsorLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorLastName attribute.</summary>
  [JsonPropertyName("chSponsorLastName")]
  [Computed]
  public string ChSponsorLastName_Json
  {
    get => NullIf(ChSponsorLastName, "");
    set => ChSponsorLastName = value;
  }

  /// <summary>Length of the CH_SPONSOR_FIRST_NAME attribute.</summary>
  public const int ChSponsorFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the CH_SPONSOR_FIRST_NAME attribute.
  /// Child Sponsor First Name - FCR File Position 561-576.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = ChSponsorFirstName_MaxLength)]
  public string ChSponsorFirstName
  {
    get => chSponsorFirstName ?? "";
    set => chSponsorFirstName =
      TrimEnd(Substring(value, 1, ChSponsorFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorFirstName attribute.</summary>
  [JsonPropertyName("chSponsorFirstName")]
  [Computed]
  public string ChSponsorFirstName_Json
  {
    get => NullIf(ChSponsorFirstName, "");
    set => ChSponsorFirstName = value;
  }

  /// <summary>Length of the CH_SPONSOR_MIDDLE_NAME attribute.</summary>
  public const int ChSponsorMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the CH_SPONSOR_MIDDLE_NAME attribute.
  /// Child Sponsor Middle Name - FCR File Position 577-592.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length
    = ChSponsorMiddleName_MaxLength)]
  public string ChSponsorMiddleName
  {
    get => chSponsorMiddleName ?? "";
    set => chSponsorMiddleName =
      TrimEnd(Substring(value, 1, ChSponsorMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorMiddleName attribute.</summary>
  [JsonPropertyName("chSponsorMiddleName")]
  [Computed]
  public string ChSponsorMiddleName_Json
  {
    get => NullIf(ChSponsorMiddleName, "");
    set => ChSponsorMiddleName = value;
  }

  /// <summary>Length of the CH_SPONSOR_LAST_NAME_SUFFIX attribute.</summary>
  public const int ChSponsorLastNameSuffix_MaxLength = 4;

  /// <summary>
  /// The value of the CH_SPONSOR_LAST_NAME_SUFFIX attribute.
  /// Child Sponsor First Name - FCR File Position 593-596.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length
    = ChSponsorLastNameSuffix_MaxLength)]
  public string ChSponsorLastNameSuffix
  {
    get => chSponsorLastNameSuffix ?? "";
    set => chSponsorLastNameSuffix =
      TrimEnd(Substring(value, 1, ChSponsorLastNameSuffix_MaxLength));
  }

  /// <summary>
  /// The json value of the ChSponsorLastNameSuffix attribute.</summary>
  [JsonPropertyName("chSponsorLastNameSuffix")]
  [Computed]
  public string ChSponsorLastNameSuffix_Json
  {
    get => NullIf(ChSponsorLastNameSuffix, "");
    set => ChSponsorLastNameSuffix = value;
  }

  private string recordId;
  private string transmitterStateCode;
  private string fipsCountyCode;
  private string caseId;
  private string orderIndicator;
  private string chFirstName;
  private string chMiddleName;
  private string chLastName;
  private string chSsn;
  private string chSsnVerifiedInd;
  private long chMemberId;
  private string chDeathIndicator;
  private string chMedicalCoverageIndicator;
  private string chMedicalCoverageSponsorCode;
  private DateTime? chMedicalCoverageBeginDate;
  private DateTime? chMedicalCoverageEndDate;
  private string ncpFirstName;
  private string ncpMiddleName;
  private string ncpLastName;
  private string ncpSsn;
  private string ncpSsnVerifiedIndicator;
  private long ncpMemberId;
  private string ncpDeathIndicator;
  private string ncpInTheMilitaryIndicator;
  private string pfFirstName;
  private string pfMiddleName;
  private string pfLastName;
  private string pfSsn;
  private string pfSsnVerifiedIndicator;
  private long pfMemberId;
  private string pfDeathIndicator;
  private string pfInTheMilitaryIndicator;
  private string cpFirstName;
  private string cpMiddleName;
  private string cpLastName;
  private string cpSsn;
  private string cpSsnVerifiedIndicator;
  private long cpMemberId;
  private string cpDeathIndicator;
  private string cpInTheMilitaryIndicator;
  private string chSponsorRelCode;
  private string chSponsorSsn;
  private string chSponsorLastName;
  private string chSponsorFirstName;
  private string chSponsorMiddleName;
  private string chSponsorLastNameSuffix;
}

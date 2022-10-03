// Program: EAB_READ_ALL_FCR_SVES_TYPE_RECS, ID: 945066144, model: 746.
// Short name: SWEXIC08
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_ALL_FCR_SVES_TYPE_RECS.
/// </para>
/// <para>
/// This External Action Block(EAB) reads all the SVES records type as well maps
/// the SVES data into the respective CSE Entities (i.e. SVES Generation
/// Information, Title-II Pending, Title-II, Title-XVI etc.).
/// </para>
/// </summary>
[Serializable]
public partial class EabReadAllFcrSvesTypeRecs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_ALL_FCR_SVES_TYPE_RECS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadAllFcrSvesTypeRecs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadAllFcrSvesTypeRecs.
  /// </summary>
  public EabReadAllFcrSvesTypeRecs(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXIC08", context, import, export, EabOptions.Hpvp);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SvesAddressListGroup group.</summary>
    [Serializable]
    public class SvesAddressListGroup
    {
      /// <summary>
      /// A value of FcrSvesAddress.
      /// </summary>
      [JsonPropertyName("fcrSvesAddress")]
      [Member(Index = 1, AccessFields = false, Members = new[]
      {
        "SvesAddressTypeCode",
        "AddressLine1",
        "AddressLine2",
        "AddressLine3",
        "AddressLine4",
        "City",
        "State",
        "ZipCode5",
        "ZipCode4",
        "Zip3",
        "AddressScrubIndicator1",
        "AddressScrubIndicator2",
        "AddressScrubIndicator3",
        "CreatedBy",
        "CreatedTimestamp",
        "LastUpdatedBy",
        "LastUpdatedTimestamp"
      })]
      public FcrSvesAddress FcrSvesAddress
      {
        get => fcrSvesAddress ??= new();
        set => fcrSvesAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private FcrSvesAddress fcrSvesAddress;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "MemberId",
      "LocateSourceResponseAgencyCo",
      "SvesMatchType",
      "TransmitterStateTerritoryCode",
      "ReturnedFirstName",
      "ReturnedMiddleName",
      "ReturnedLastName",
      "SexCode",
      "ReturnedDateOfBirth",
      "ReturnedDateOfDeath",
      "SubmittedFirstName",
      "SubmittedMiddleName",
      "SubmittedLastName",
      "SubmittedDateOfBirth",
      "Ssn",
      "UserField",
      "LocateClosedIndicator",
      "FipsCountyCode",
      "LocateRequestType",
      "LocateResponseCode",
      "MultipleSsnIndicator",
      "MultipleSsn",
      "ParticipantType",
      "FamilyViolenceState1",
      "FamilyViolenceState2",
      "FamilyViolenceState3",
      "SortStateCode",
      "RequestDate",
      "ResponseReceivedDate",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp"
    })]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// Gets a value of SvesAddressList.
    /// </summary>
    [JsonIgnore]
    [Member(Index = 3)]
    public Array<SvesAddressListGroup> SvesAddressList =>
      svesAddressList ??= new(SvesAddressListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SvesAddressList for json serialization.
    /// </summary>
    [JsonPropertyName("svesAddressList")]
    [Computed]
    public IList<SvesAddressListGroup> SvesAddressList_Json
    {
      get => svesAddressList;
      set => SvesAddressList.Assign(value);
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPend")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "SeqNo",
      "NameMatchedCode",
      "FirstNameText",
      "MiddleNameText",
      "LastNameText",
      "AdditionalFirstName1Text",
      "AdditionalMiddleName1Text",
      "AdditionalLastName1Text",
      "AdditionalFirstName2Text",
      "AdditionalMiddleName2Text",
      "AdditionalLastName2Text",
      "ResponseDate",
      "OtherSsn",
      "SsnMatchCode",
      "ClaimTypeCode",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp"
    })]
    public FcrSvesTitleIiPend FcrSvesTitleIiPend
    {
      get => fcrSvesTitleIiPend ??= new();
      set => fcrSvesTitleIiPend = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIi")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "SeqNo",
      "CanAndBic",
      "StateCode",
      "CountyCode",
      "DirectDepositIndicator",
      "LafCode",
      "DeferredPaymentDate",
      "InitialTitleIiEntitlementDt",
      "CurrentTitleIiEntitlementDt",
      "TitleIiSuspendTerminateDt",
      "NetMonthlyTitleIiBenefit",
      "HiOptionCode",
      "HiStartDate",
      "HiStopDate",
      "SmiOptionCode",
      "SmiStartDate",
      "SmiStopDate",
      "CategoryOfAssistance",
      "BlackLungEntitlementCode",
      "BlackLungPaymentAmount",
      "RailroadIndicator",
      "MbcNumberOfEntries",
      "MbcDate1",
      "MbcAmount1",
      "MbcType1",
      "MbcDate2",
      "MbcAmount2",
      "MbcType2",
      "MbcDate3",
      "MbcAmount3",
      "MbcType3",
      "MbcDate4",
      "MbcAmount4",
      "MbcType4",
      "MbcDate5",
      "MbcAmount5",
      "MbcType5",
      "MbcDate6",
      "MbcAmount6",
      "MbcType6",
      "MbcDate7",
      "MbcAmount7",
      "MbcType7",
      "MbcDate8",
      "MbcAmount8",
      "MbcType8",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp"
    })]
    public FcrSvesTitleIi FcrSvesTitleIi
    {
      get => fcrSvesTitleIi ??= new();
      set => fcrSvesTitleIi = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleXvi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleXvi")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "SeqNo",
      "OtherName",
      "RaceCode",
      "DateOfDeathSourceCode",
      "PayeeStateOfJurisdiction",
      "PayeeCountyOfJurisdiction",
      "PayeeDistrictOfficeCode",
      "TypeOfPayeeCode",
      "TypeOfRecipient",
      "RecordEstablishmentDate",
      "DateOfTitleXviEligibility",
      "TitleXviAppealCode",
      "DateOfTitleXviAppeal",
      "TitleXviLastRedeterminDate",
      "TitleXviDenialDate",
      "CurrentPaymentStatusCode",
      "PaymentStatusCode",
      "PaymentStatusDate",
      "TelephoneNumber",
      "ThirdPartyInsuranceIndicator",
      "DirectDepositIndicator",
      "RepresentativePayeeIndicator",
      "CustodyCode",
      "EstimatedSelfEmploymentAmount",
      "UnearnedIncomeNumOfEntries",
      "UnearnedIncomeTypeCode1",
      "UnearnedIncomeVerifiCd1",
      "UnearnedIncomeStartDate1",
      "UnearnedIncomeStopDate1",
      "UnearnedIncomeTypeCode2",
      "UnearnedIncomeVerifiCd2",
      "UnearnedIncomeStartDate2",
      "UnearnedIncomeStopDate2",
      "UnearnedIncomeTypeCode3",
      "UnearnedIncomeVerifiCd3",
      "UnearnedIncomeStartDate3",
      "UnearnedIncomeStopDate3",
      "UnearnedIncomeTypeCode4",
      "UnearnedIncomeVerifiCd4",
      "UnearnedIncomeStartDate4",
      "UnearnedIncomeStopDate4",
      "UnearnedIncomeTypeCode5",
      "UnearnedIncomeVerifiCd5",
      "UnearnedIncomeStartDate5",
      "UnearnedIncomeStopDate5",
      "UnearnedIncomeTypeCode6",
      "UnearnedIncomeVerifiCd6",
      "UnearnedIncomeStartDate6",
      "UnearnedIncomeStopDate6",
      "UnearnedIncomeTypeCode7",
      "UnearnedIncomeVerifiCd7",
      "UnearnedIncomeStartDate7",
      "UnearnedIncomeStopDate7",
      "UnearnedIncomeTypeCode8",
      "UnearnedIncomeVerifiCd8",
      "UnearnedIncomeStartDate8",
      "UnearnedIncomeStopDate8",
      "UnearnedIncomeTypeCode9",
      "UnearnedIncomeVerifiCd9",
      "UnearnedIncomeStartDate9",
      "UnearnedIncomeStopDate9",
      "PhistNumberOfEntries",
      "PhistPaymentDate1",
      "SsiMonthlyAssistanceAmount1",
      "PhistPaymentPayFlag1",
      "PhistPaymentDate2",
      "SsiMonthlyAssistanceAmount2",
      "PhistPaymentPayFlag2",
      "PhistPaymentDate3",
      "SsiMonthlyAssistanceAmount3",
      "PhistPaymentPayFlag3",
      "PhistPaymentDate4",
      "SsiMonthlyAssistanceAmount4",
      "PhistPaymentPayFlag4",
      "PhistPaymentDate5",
      "SsiMonthlyAssistanceAmount5",
      "PhistPaymentPayFlag5",
      "PhistPaymentDate6",
      "SsiMonthlyAssistanceAmount6",
      "PhistPaymentPayFlag6",
      "PhistPaymentDate7",
      "SsiMonthlyAssistanceAmount7",
      "PhistPaymentPayFlag7",
      "PhistPaymentDate8",
      "SsiMonthlyAssistanceAmount8",
      "PhistPaymentPayFlag8",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp"
    })]
    public FcrSvesTitleXvi FcrSvesTitleXvi
    {
      get => fcrSvesTitleXvi ??= new();
      set => fcrSvesTitleXvi = value;
    }

    /// <summary>
    /// A value of FcrSvesPrison.
    /// </summary>
    [JsonPropertyName("fcrSvesPrison")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "SeqNo",
      "PrisonFacilityType",
      "PrisonFacilityName",
      "PrisonFacilityContactName",
      "PrisonFacilityPhone",
      "PrisonFacilityFaxNum",
      "PrisonReportedSsn",
      "ConfinementDate",
      "ReleaseDate",
      "ReportDate",
      "PrisonerReporterName",
      "PrisonerIdNumber",
      "PrisonReportedSuffix",
      "CreatedBy",
      "CreatedTimestamp",
      "LastUpdatedBy",
      "LastUpdatedTimestamp"
    })]
    public FcrSvesPrison FcrSvesPrison
    {
      get => fcrSvesPrison ??= new();
      set => fcrSvesPrison = value;
    }

    private EabFileHandling eabFileHandling;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private Array<SvesAddressListGroup> svesAddressList;
    private FcrSvesTitleIiPend fcrSvesTitleIiPend;
    private FcrSvesTitleIi fcrSvesTitleIi;
    private FcrSvesTitleXvi fcrSvesTitleXvi;
    private FcrSvesPrison fcrSvesPrison;
  }
#endregion
}

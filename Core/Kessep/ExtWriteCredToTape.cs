// Program: EXT_WRITE_CRED_TO_TAPE, ID: 372739683, model: 746.
// Short name: SWEXLE04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EXT_WRITE_CRED_TO_TAPE.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will be used in batch processes to access a driver file.  
/// It will recieve a access instruction (OPEN, READ, CLOSE, POSITION) and will
/// send a return code and driver record.
/// </para>
/// </summary>
[Serializable]
public partial class ExtWriteCredToTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EXT_WRITE_CRED_TO_TAPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ExtWriteCredToTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ExtWriteCredToTape.
  /// </summary>
  public ExtWriteCredToTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXLE04", context, import, export, 0);
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

    /// <summary>
    /// A value of Metro2CraHeaderRecord.
    /// </summary>
    [JsonPropertyName("metro2CraHeaderRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "RecordDescriptorWord",
      "RecordIdentifier",
      "CycleIdentifier",
      "InnovisProgramIdentifier",
      "EquifaxProgramIdentifier",
      "ExperianProgramIdentifier",
      "TransunionProgramIdentifier",
      "ActivityDate",
      "DateCreated",
      "ProgramDate",
      "ProgramRevisionDate",
      "ReporterName",
      "ReporterAddress",
      "ReporterTelephoneNumber",
      "SoftwareVendorName",
      "SoftwareVersionNumber",
      "MicrobiltprbcProgramIdentifier",
      "Reserved"
    })]
    public Metro2CraHeaderRecord Metro2CraHeaderRecord
    {
      get => metro2CraHeaderRecord ??= new();
      set => metro2CraHeaderRecord = value;
    }

    /// <summary>
    /// A value of Metro2CraBaseRecord.
    /// </summary>
    [JsonPropertyName("metro2CraBaseRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "RecordDescriptorWord",
      "ProcessingIndicator",
      "Timestamp",
      "Reserved4",
      "IdentificationNumber",
      "CycleIdentifier",
      "ConsumerAccountNumber",
      "PortfolioType",
      "AccountType",
      "DateOpened",
      "CreditLimit",
      "HighestCreditAmount",
      "TermsDuration",
      "TermsFrequency",
      "ScheduledMonthlyPaymentAmount",
      "ActualPaymentAmount",
      "AccountStatus",
      "PaymentRating",
      "PaymentHistoryProfile",
      "SpecialComment",
      "ComplianceConditionCode",
      "CurrentBalance",
      "AmountPastDue",
      "OriginalChargeOffAmount",
      "DateOfAccountInformation",
      "DateOfFirstDelinquency",
      "DateClosed",
      "DateOfLastPayment",
      "InterestTypeIndicator",
      "Reserved29",
      "Surname",
      "FirstName",
      "MiddleName",
      "GenerationCode",
      "SocialSecurityNumber",
      "DateOfBirth",
      "TelephoneNumber",
      "EcoaCode",
      "ConsumerInformationIndicator",
      "CountryCode",
      "FirstLineOfAddress",
      "SecondLineOfAddress",
      "City",
      "State",
      "PostalZipCode",
      "AddressIndicator",
      "ResidenceCode"
    })]
    public Metro2CraBaseRecord Metro2CraBaseRecord
    {
      get => metro2CraBaseRecord ??= new();
      set => metro2CraBaseRecord = value;
    }

    /// <summary>
    /// A value of Metro2CraTrailerRecord.
    /// </summary>
    [JsonPropertyName("metro2CraTrailerRecord")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "RecordDesciptorWord",
      "RecordIdentifier",
      "TotalBaseRecords",
      "Reserved4",
      "TotalOfStatusCodeDf",
      "TotalJ1Segments",
      "TotalJ2Segments",
      "BlockCount",
      "TotalOfStatusCodeDa",
      "TotalOfStatusCode05",
      "TotalOfStatusCode11",
      "TotalOfStatusCode13",
      "TotalOfStatusCode61",
      "TotalOfStatusCode62",
      "TotalOfStatusCode63",
      "TotalOfStatusCode64",
      "TotalOfStatusCode65",
      "TotalOfStatusCode71",
      "TotalOfStatusCode78",
      "TotalOfStatusCode80",
      "TotalOfStatusCode82",
      "TotalOfStatusCode83",
      "TotalOfStatusCode84",
      "TotalOfStatusCode88",
      "TotalOfStatusCode89",
      "TotalOfStatusCode93",
      "TotalOfStatusCode94",
      "TotalOfStatusCode95",
      "TotalOfStatusCode96",
      "TotalOfStatusCode97",
      "TotalOfEcoaCodeZ",
      "TotalEmploymentSegments",
      "TotalOriginalCreditorSegments",
      "TotalPurchasedFromSoldToSeg",
      "TotalMorgageInformationSegmen",
      "TotalSpecicalizedPaymentInfo",
      "TotalChangeSegements",
      "TotalSsnsAllSegments",
      "TotalSsnsBaseSegments",
      "TotalSsnsJ1Segements",
      "TotalSsnsJ2Segments",
      "TotalBirthDatesAllSegments",
      "TotalBirthDatesBaseSegments",
      "TotalBirthDatesJ1Segments",
      "TotalBirthDatesJ2Segments",
      "TotalPhoneNumberAllSegments",
      "Reserved47"
    })]
    public Metro2CraTrailerRecord Metro2CraTrailerRecord
    {
      get => metro2CraTrailerRecord ??= new();
      set => metro2CraTrailerRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private Metro2CraHeaderRecord metro2CraHeaderRecord;
    private Metro2CraBaseRecord metro2CraBaseRecord;
    private Metro2CraTrailerRecord metro2CraTrailerRecord;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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

    private EabFileHandling eabFileHandling;
  }
#endregion
}

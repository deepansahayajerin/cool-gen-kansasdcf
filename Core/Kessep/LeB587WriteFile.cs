// Program: LE_B587_WRITE_FILE, ID: 1902480122, model: 746.
// Short name: SWEXLW07
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B587_WRITE_FILE.
/// </summary>
[Serializable]
public partial class LeB587WriteFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B587_WRITE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB587WriteFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB587WriteFile.
  /// </summary>
  public LeB587WriteFile(IContext context, Import import, Export export):
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
      "SWEXLW07", context, import, export, EabOptions.Hpvp);
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
    /// A value of EiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("eiwoB587HeaderRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "DocumentCode",
      "ControlNumber",
      "StateFipsCode",
      "Ein",
      "PrimaryEin",
      "CreationDate",
      "CreationTime",
      "ErrorFieldName"
    })]
    public EiwoB587HeaderRecord EiwoB587HeaderRecord
    {
      get => eiwoB587HeaderRecord ??= new();
      set => eiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of EiwoB587DetailRecord.
    /// </summary>
    [JsonPropertyName("eiwoB587DetailRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "DocumentCode",
      "DocumentActionCode",
      "DocumentDate",
      "IssuingStateTribeTerritoryNm",
      "IssuingJurisdictionName",
      "CaseIdentifier",
      "EmployerName",
      "EmployerAddressLine1",
      "EmployerAddressLine2",
      "EmployerAddressCityName",
      "EmployerAddressStateCode",
      "EmployerAddressZipCode",
      "EmployerAddressExtZipCode",
      "Ein",
      "EmployeeLastName",
      "EmployeeFirstName",
      "EmployeeMiddleName",
      "EmployeeSuffix",
      "EmployeeSsn",
      "EmployeeBirthDate",
      "ObligeeLastName",
      "ObligeeFirstName",
      "ObligeeMiddleName",
      "ObligeeNameSuffix",
      "IssuingTribunalName",
      "SupportCurrentChildAmount",
      "SupportCurrentChildFrequency",
      "SupportPastDueChildAmount",
      "SupportPastDueChildFrequency",
      "SupportCurrentMedicalAmount",
      "SupportCurrentMedicalFrequenc",
      "SupportPastDueMedicalAmount",
      "SupportPastDueMedicalFrequen",
      "SupportCurrentSpousalAmount",
      "SupportCurrentSpousalFrequenc",
      "SupportPastDueSpousalAmount",
      "SupportPastDueSpousalFrequen",
      "ObligationOtherAmount",
      "ObligationOtherFrequencyCode",
      "ObligationOtherDescription",
      "ObligationTotalAmount",
      "ObligationTotalFrequency",
      "Arrears12WkOverdueCode",
      "IwoDeductionWeeklyAmount",
      "IwoDeductionBiweeklyAmount",
      "IwoDeductionSemimonthlyAmount",
      "IwoDeductionMonthlyAmount",
      "StateTribeTerritoryName",
      "BeginWithholdingWithinDays",
      "IncomeWithholdingStartInstruc",
      "SendPaymentWithhinDays",
      "IwoCcpaPercentRate",
      "PayeeName",
      "PayeeAddressLine1",
      "PayeeAddressLine2",
      "PayeeAddressCity",
      "PayeeAddressStateCode",
      "PayeeAddressZipCode",
      "PayeeAddressExtZipCode",
      "PayeeRemittanceFipsCode",
      "GovernmentOfficialName",
      "IssuingOfficialTitle",
      "SendEmployeeCopyIndicator",
      "PenaltyLiabilityInfoText",
      "AntidiscriminationProvisionTxt",
      "SpecificPayeeWithholdingLimit",
      "EmployeeStateContactName",
      "EmployeeStateContactPhone",
      "EmployeeStateContactFax",
      "EmployeeStateContactEmail",
      "DocumentTrackingNumber",
      "OrderIdentifier",
      "EmployerContactName",
      "EmployerContactAddressLine1",
      "EmployerContactAddressLine2",
      "EmployerContactAddressCity",
      "EmployerContactAddressState",
      "EmployerContactAddressZip",
      "EmployerContactAddressExtZip",
      "EmployerContactPhone",
      "EmployerContactFax",
      "EmployerContactEmail",
      "Child1LastName",
      "Child1FirstName",
      "Child1MiddleName",
      "Child1SuffixName",
      "Child1BirthDate",
      "Child2LastName",
      "Child2FirstName",
      "Child2MiddleName",
      "Child2SuffixName",
      "Child2BirthDate",
      "Child3LastName",
      "Child3FirstName",
      "Child3MiddleName",
      "Child3SuffixName",
      "Child3BirthDate",
      "Child4LastName",
      "Child4FirstName",
      "Child4MiddleName",
      "Child4SuffixName",
      "Child4BirthDate",
      "Child5LastName",
      "Child5FirstName",
      "Child5MiddleName",
      "Child5SuffixName",
      "Child5BirthDate",
      "Child6LastName",
      "Child6FirstName",
      "Child6MiddleName",
      "Child6SuffixName",
      "Child6BirthDate",
      "LumpSumPaymentAmount",
      "RemittanceIdentifier",
      "DocumentImageText",
      "IncomeWithholdingStartDate"
    })]
    public EiwoB587DetailRecord EiwoB587DetailRecord
    {
      get => eiwoB587DetailRecord ??= new();
      set => eiwoB587DetailRecord = value;
    }

    /// <summary>
    /// A value of EiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("eiwoB587TrailerRecord")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "DocumentCode",
      "ControlNumber",
      "BatchCount",
      "RecordCount",
      "EmployerSentCount",
      "StateSentCount",
      "ErrorFieldName"
    })]
    public EiwoB587TrailerRecord EiwoB587TrailerRecord
    {
      get => eiwoB587TrailerRecord ??= new();
      set => eiwoB587TrailerRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private EiwoB587HeaderRecord eiwoB587HeaderRecord;
    private EiwoB587DetailRecord eiwoB587DetailRecord;
    private EiwoB587TrailerRecord eiwoB587TrailerRecord;
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

    /// <summary>
    /// A value of ExtendedFileEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("extendedFileEiwoB587HeaderRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "ControlNumber" })
      ]
    public EiwoB587HeaderRecord ExtendedFileEiwoB587HeaderRecord
    {
      get => extendedFileEiwoB587HeaderRecord ??= new();
      set => extendedFileEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of ExtendedFileEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("extendedFileEiwoB587TrailerRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "BatchCount" })]
    public EiwoB587TrailerRecord ExtendedFileEiwoB587TrailerRecord
    {
      get => extendedFileEiwoB587TrailerRecord ??= new();
      set => extendedFileEiwoB587TrailerRecord = value;
    }

    /// <summary>
    /// A value of ExtendedBatchEiwoB587HeaderRecord.
    /// </summary>
    [JsonPropertyName("extendedBatchEiwoB587HeaderRecord")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "ControlNumber" })
      ]
    public EiwoB587HeaderRecord ExtendedBatchEiwoB587HeaderRecord
    {
      get => extendedBatchEiwoB587HeaderRecord ??= new();
      set => extendedBatchEiwoB587HeaderRecord = value;
    }

    /// <summary>
    /// A value of ExtendedBatchEiwoB587TrailerRecord.
    /// </summary>
    [JsonPropertyName("extendedBatchEiwoB587TrailerRecord")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "RecordCount" })]
    public EiwoB587TrailerRecord ExtendedBatchEiwoB587TrailerRecord
    {
      get => extendedBatchEiwoB587TrailerRecord ??= new();
      set => extendedBatchEiwoB587TrailerRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private EiwoB587HeaderRecord extendedFileEiwoB587HeaderRecord;
    private EiwoB587TrailerRecord extendedFileEiwoB587TrailerRecord;
    private EiwoB587HeaderRecord extendedBatchEiwoB587HeaderRecord;
    private EiwoB587TrailerRecord extendedBatchEiwoB587TrailerRecord;
  }
#endregion
}

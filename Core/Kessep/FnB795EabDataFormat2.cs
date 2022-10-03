// Program: FN_B795_EAB_DATA_FORMAT_2, ID: 1902457514, model: 746.
// Short name: SWEXEW23
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B795_EAB_DATA_FORMAT_2.
/// </summary>
[Serializable]
public partial class FnB795EabDataFormat2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B795_EAB_DATA_FORMAT_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB795EabDataFormat2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB795EabDataFormat2.
  /// </summary>
  public FnB795EabDataFormat2(IContext context, Import import, Export export):
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
      "SWEXEW23", context, import, export, EabOptions.Hpvp);
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
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of HeaderFooter.
    /// </summary>
    [JsonPropertyName("headerFooter")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "HeaderFooter" })]
      
    public ContractorCaseUniverse HeaderFooter
    {
      get => headerFooter ??= new();
      set => headerFooter = value;
    }

    /// <summary>
    /// A value of CaseNcp.
    /// </summary>
    [JsonPropertyName("caseNcp")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "ContractorName",
      "JudicialDistrict",
      "CaseNumber",
      "CaseOpenDate",
      "AssignedCaseworkerFirstName",
      "AssignedCaseworkerLastName",
      "PyramidCategory",
      "AddressActive",
      "EmployerActive",
      "CurrentSupportDue",
      "CurrentSupportPaid",
      "CollectionRate",
      "CasePayingArrears",
      "CaseFunction",
      "CaseProgram",
      "PaProgramEndDate",
      "CuraAmount",
      "FamilyViolence",
      "CpNoncooperation",
      "PendingCaseClosureDate",
      "DateOfEmancipation",
      "YoungestEmancipationDate",
      "ChildBow",
      "CpDateOfBirth",
      "CpEthnicity",
      "CpZipCode",
      "CpHomePhoneAreaCode",
      "CpHomePhone",
      "CpCellPhoneAreaCode",
      "CpCellPhone",
      "NcpPersonNumber",
      "NcpLastName",
      "NcpFirstName",
      "NcpDateOfBirth",
      "NcpEthnicity",
      "NcpLocateDate",
      "NcpZipCode",
      "NcpForeignCountryCode",
      "NcpHomePhoneAreaCode",
      "NcpHomePhone",
      "NcpCellPhoneAreaCode",
      "NcpCellPhone",
      "NcpIncarcerated",
      "NcpInBankruptcy",
      "NcpRepresentedByCouncil",
      "NcpEmployerName",
      "NcpOtherIncomeSource",
      "NcpInterstateInitiating",
      "NcpInterstateResponding"
    })]
    public ContractorCaseUniverse CaseNcp
    {
      get => caseNcp ??= new();
      set => caseNcp = value;
    }

    /// <summary>
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "ContractorName",
      "JudicialDistrict",
      "CaseNumber",
      "NcpPersonNumber",
      "CoCourtOrderNumber",
      "CoCsCollectedInMonth",
      "CoCsDueInMonth",
      "CoCsCollectedFfytd",
      "CoCsDueFfytd",
      "CoTotalArrearsAmountDue",
      "CoArrearsPaidInMonth",
      "CoArrearsPaidFfytd",
      "CoLastPaymentAmount",
      "CoLastPaymentDate",
      "CoLastPaymentType",
      "CoLastDsoPaymentDate",
      "CoLastIClassCreatedDate",
      "CoLastIClassActionTaken",
      "CoLastIClassFiledDate",
      "CoLastIClassIwgl",
      "CoMonthlyIwoWaAmount",
      "CoContemptHearingDate",
      "CoContemptServiceDate",
      "CoDemandLetterCreatedDate",
      "CoPetitionCreatedDate",
      "CoPetitionFiledDate"
    })]
    public ContractorCaseUniverse CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    private EabFileHandling eabFileHandling;
    private TextWorkArea recordType;
    private ContractorCaseUniverse headerFooter;
    private ContractorCaseUniverse caseNcp;
    private ContractorCaseUniverse courtOrder;
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

// Program: EAB_READ_FED_QTRLY_UNEMPL_INCOME, ID: 373313648, model: 746.
// Short name: SWEXIC05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_FED_QTRLY_UNEMPL_INCOME.
/// </summary>
[Serializable]
public partial class EabReadFedQtrlyUnemplIncome: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_FED_QTRLY_UNEMPL_INCOME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadFedQtrlyUnemplIncome(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadFedQtrlyUnemplIncome.
  /// </summary>
  public EabReadFedQtrlyUnemplIncome(IContext context, Import import,
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
      "SWEXIC05", context, import, export, EabOptions.Hpvp);
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
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "Flag",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReportingState.
    /// </summary>
    [JsonPropertyName("reportingState")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common ReportingState
    {
      get => reportingState ??= new();
      set => reportingState = value;
    }

    /// <summary>
    /// A value of BenefitAmount.
    /// </summary>
    [JsonPropertyName("benefitAmount")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common BenefitAmount
    {
      get => benefitAmount ??= new();
      set => benefitAmount = value;
    }

    /// <summary>
    /// A value of SsnMatchIndicator.
    /// </summary>
    [JsonPropertyName("ssnMatchIndicator")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Flag" })]
    public Common SsnMatchIndicator
    {
      get => ssnMatchIndicator ??= new();
      set => ssnMatchIndicator = value;
    }

    /// <summary>
    /// A value of ReportingYear.
    /// </summary>
    [JsonPropertyName("reportingYear")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Year" })]
    public DateWorkArea ReportingYear
    {
      get => reportingYear ??= new();
      set => reportingYear = value;
    }

    /// <summary>
    /// A value of ReportingQuarter.
    /// </summary>
    [JsonPropertyName("reportingQuarter")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea ReportingQuarter
    {
      get => reportingQuarter ??= new();
      set => reportingQuarter = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "Identifier",
      "Ein",
      "KansasId",
      "Name",
      "PhoneNo",
      "AreaCode",
      "CreatedBy",
      "CreatedTstamp",
      "LastUpdatedBy",
      "LastUpdatedTstamp"
    })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "LocationType",
      "LastUpdatedTimestamp",
      "LastUpdatedBy",
      "CreatedTimestamp",
      "CreatedBy",
      "Street1",
      "Street2",
      "City",
      "Identifier",
      "Note",
      "County",
      "State",
      "ZipCode",
      "Zip4",
      "Zip3",
      "Street3",
      "Street4",
      "Province",
      "Country",
      "PostalCode"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common reportingState;
    private Common benefitAmount;
    private Common ssnMatchIndicator;
    private DateWorkArea reportingYear;
    private WorkArea reportingQuarter;
    private EabFileHandling eabFileHandling;
    private Employer employer;
    private EmployerAddress employerAddress;
  }
#endregion
}

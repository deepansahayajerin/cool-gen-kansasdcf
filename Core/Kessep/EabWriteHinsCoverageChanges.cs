// Program: EAB_WRITE_HINS_COVERAGE_CHANGES, ID: 372871045, model: 746.
// Short name: SWEXEE33
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_HINS_COVERAGE_CHANGES.
/// </summary>
[Serializable]
public partial class EabWriteHinsCoverageChanges: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_HINS_COVERAGE_CHANGES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteHinsCoverageChanges(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteHinsCoverageChanges.
  /// </summary>
  public EabWriteHinsCoverageChanges(IContext context, Import import,
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
      "SWEXEE33", context, import, export, EabOptions.Hpvp);
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "CarrierCode" })]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "PolicyPaidByCsePersonInd",
      "InsuranceGroupNumber",
      "InsurancePolicyNumber",
      "CoverageCode1",
      "CoverageCode2",
      "CoverageCode3",
      "CoverageCode4",
      "CoverageCode5",
      "CoverageCode6",
      "CoverageCode7"
    })]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public CsePersonsWorkSet CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    /// <summary>
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FirstName",
      "LastName"
    })]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    /// <summary>
    /// A value of RelationToPolicyHolder.
    /// </summary>
    [JsonPropertyName("relationToPolicyHolder")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "SelectChar" })]
    public Common RelationToPolicyHolder
    {
      get => relationToPolicyHolder ??= new();
      set => relationToPolicyHolder = value;
    }

    /// <summary>
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "CoverageVerifiedDate",
      "CoverageBeginDate",
      "CoverageEndDate"
    })]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Ein", "Name" })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Record.
    /// </summary>
    [JsonPropertyName("record")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Count" })]
    public Common Record
    {
      get => record ??= new();
      set => record = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private CsePersonsWorkSet coveredPerson;
    private CsePersonsWorkSet policyHolder;
    private Common relationToPolicyHolder;
    private PersonalHealthInsurance personalHealthInsurance;
    private Employer employer;
    private EmployerAddress employerAddress;
    private DateWorkArea dateWorkArea;
    private Common record;
    private EabFileHandling eabFileHandling;
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

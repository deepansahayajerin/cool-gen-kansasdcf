// Program: EAB_READ_TERMINATED_POLICY_FILE, ID: 371174446, model: 746.
// Short name: SWEXEE28
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_TERMINATED_POLICY_FILE.
/// </summary>
[Serializable]
public partial class EabReadTerminatedPolicyFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_TERMINATED_POLICY_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadTerminatedPolicyFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadTerminatedPolicyFile.
  /// </summary>
  public EabReadTerminatedPolicyFile(IContext context, Import import,
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
      "SWEXEE28", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "InsurancePolicyNumber" })]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "CarrierCode" })]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of FileHeader.
    /// </summary>
    [JsonPropertyName("fileHeader")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea FileHeader
    {
      get => fileHeader ??= new();
      set => fileHeader = value;
    }

    /// <summary>
    /// A value of FileTrailer.
    /// </summary>
    [JsonPropertyName("fileTrailer")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Count" })]
    public Common FileTrailer
    {
      get => fileTrailer ??= new();
      set => fileTrailer = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private DateWorkArea fileHeader;
    private Common fileTrailer;
    private EabFileHandling eabFileHandling;
  }
#endregion
}

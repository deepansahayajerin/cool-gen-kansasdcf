// Program: EAB_READ_HINS_ALERTS, ID: 372869041, model: 746.
// Short name: SWEXEE25
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_READ_HINS_ALERTS.
/// </para>
/// <para>
/// Reads alerts from EDS regarding Health Insurance Claims and Adjustments.
/// </para>
/// </summary>
[Serializable]
public partial class EabReadHinsAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_HINS_ALERTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadHinsAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadHinsAlerts.
  /// </summary>
  public EabReadHinsAlerts(IContext context, Import import, Export export):
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
      "SWEXEE25", context, import, export, EabOptions.Hpvp);
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
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Text8" })]
    public WorkArea CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
    }

    /// <summary>
    /// A value of BeneficiaryId.
    /// </summary>
    [JsonPropertyName("beneficiaryId")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public CsePersonsWorkSet BeneficiaryId
    {
      get => beneficiaryId ??= new();
      set => beneficiaryId = value;
    }

    /// <summary>
    /// A value of MessageType.
    /// </summary>
    [JsonPropertyName("messageType")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common MessageType
    {
      get => messageType ??= new();
      set => messageType = value;
    }

    /// <summary>
    /// A value of DateSentToState.
    /// </summary>
    [JsonPropertyName("dateSentToState")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateSentToState
    {
      get => dateSentToState ??= new();
      set => dateSentToState = value;
    }

    /// <summary>
    /// A value of BirthExpenseCaseNum.
    /// </summary>
    [JsonPropertyName("birthExpenseCaseNum")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea BirthExpenseCaseNum
    {
      get => birthExpenseCaseNum ??= new();
      set => birthExpenseCaseNum = value;
    }

    /// <summary>
    /// A value of OriginalIcn.
    /// </summary>
    [JsonPropertyName("originalIcn")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text13" })]
    public WorkArea OriginalIcn
    {
      get => originalIcn ??= new();
      set => originalIcn = value;
    }

    /// <summary>
    /// A value of AdjustedIcn.
    /// </summary>
    [JsonPropertyName("adjustedIcn")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text13" })]
    public WorkArea AdjustedIcn
    {
      get => adjustedIcn ??= new();
      set => adjustedIcn = value;
    }

    /// <summary>
    /// A value of AmountRecovered.
    /// </summary>
    [JsonPropertyName("amountRecovered")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common AmountRecovered
    {
      get => amountRecovered ??= new();
      set => amountRecovered = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    [Member(Index = 9, AccessFields = false, Members
      = new[] { "InsurancePolicyNumber" })]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of TerminationDate.
    /// </summary>
    [JsonPropertyName("terminationDate")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea TerminationDate
    {
      get => terminationDate ??= new();
      set => terminationDate = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private WorkArea caseNumber;
    private CsePersonsWorkSet beneficiaryId;
    private Common messageType;
    private DateWorkArea dateSentToState;
    private WorkArea birthExpenseCaseNum;
    private WorkArea originalIcn;
    private WorkArea adjustedIcn;
    private Common amountRecovered;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private DateWorkArea terminationDate;
    private EabFileHandling eabFileHandling;
  }
#endregion
}

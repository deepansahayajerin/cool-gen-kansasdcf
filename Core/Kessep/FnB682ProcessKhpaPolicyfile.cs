// Program: FN_B682_PROCESS_KHPA_POLICYFILE, ID: 374579437, model: 746.
// Short name: SWEXER19
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B682_PROCESS_KHPA_POLICYFILE.
/// </summary>
[Serializable]
public partial class FnB682ProcessKhpaPolicyfile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B682_PROCESS_KHPA_POLICYFILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB682ProcessKhpaPolicyfile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB682ProcessKhpaPolicyfile.
  /// </summary>
  public FnB682ProcessKhpaPolicyfile(IContext context, Import import,
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
      "SWEXER19", context, import, export, EabOptions.Hpvp);
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
    /// A value of PolicyHolder.
    /// </summary>
    [JsonPropertyName("policyHolder")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FirstName",
      "LastName",
      "Ssn"
    })]
    public CsePersonsWorkSet PolicyHolder
    {
      get => policyHolder ??= new();
      set => policyHolder = value;
    }

    /// <summary>
    /// A value of PolicyHolderBenfitNumb.
    /// </summary>
    [JsonPropertyName("policyHolderBenfitNumb")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text12" })]
    public TextWorkArea PolicyHolderBenfitNumb
    {
      get => policyHolderBenfitNumb ??= new();
      set => policyHolderBenfitNumb = value;
    }

    /// <summary>
    /// A value of CoveredPerson.
    /// </summary>
    [JsonPropertyName("coveredPerson")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "FirstName",
      "LastName",
      "Ssn"
    })]
    public CsePersonsWorkSet CoveredPerson
    {
      get => coveredPerson ??= new();
      set => coveredPerson = value;
    }

    /// <summary>
    /// A value of CoverPersonBenfitNumb.
    /// </summary>
    [JsonPropertyName("coverPersonBenfitNumb")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text12" })]
    public TextWorkArea CoverPersonBenfitNumb
    {
      get => coverPersonBenfitNumb ??= new();
      set => coverPersonBenfitNumb = value;
    }

    /// <summary>
    /// A value of CarrierName.
    /// </summary>
    [JsonPropertyName("carrierName")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text50" })]
    public WorkArea CarrierName
    {
      get => carrierName ??= new();
      set => carrierName = value;
    }

    /// <summary>
    /// A value of Carrier.
    /// </summary>
    [JsonPropertyName("carrier")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "LocationType",
      "City",
      "ZipCode",
      "Zip4"
    })]
    public EmployerAddress Carrier
    {
      get => carrier ??= new();
      set => carrier = value;
    }

    /// <summary>
    /// A value of CarrierStreetAddr1.
    /// </summary>
    [JsonPropertyName("carrierStreetAddr1")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CarrierStreetAddr1
    {
      get => carrierStreetAddr1 ??= new();
      set => carrierStreetAddr1 = value;
    }

    /// <summary>
    /// A value of CarrierStreetAddr2.
    /// </summary>
    [JsonPropertyName("carrierStreetAddr2")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CarrierStreetAddr2
    {
      get => carrierStreetAddr2 ??= new();
      set => carrierStreetAddr2 = value;
    }

    /// <summary>
    /// A value of CarrrierState.
    /// </summary>
    [JsonPropertyName("carrrierState")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text5" })]
    public WorkArea CarrrierState
    {
      get => carrrierState ??= new();
      set => carrrierState = value;
    }

    /// <summary>
    /// A value of PolicyNumber.
    /// </summary>
    [JsonPropertyName("policyNumber")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Text20" })]
    public WorkArea PolicyNumber
    {
      get => policyNumber ??= new();
      set => policyNumber = value;
    }

    /// <summary>
    /// A value of GroupNumber.
    /// </summary>
    [JsonPropertyName("groupNumber")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text20" })]
    public WorkArea GroupNumber
    {
      get => groupNumber ??= new();
      set => groupNumber = value;
    }

    /// <summary>
    /// A value of CoverageCode.
    /// </summary>
    [JsonPropertyName("coverageCode")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea CoverageCode
    {
      get => coverageCode ??= new();
      set => coverageCode = value;
    }

    /// <summary>
    /// A value of PolicyStartDate.
    /// </summary>
    [JsonPropertyName("policyStartDate")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea PolicyStartDate
    {
      get => policyStartDate ??= new();
      set => policyStartDate = value;
    }

    /// <summary>
    /// A value of PolicyEndDate.
    /// </summary>
    [JsonPropertyName("policyEndDate")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea PolicyEndDate
    {
      get => policyEndDate ??= new();
      set => policyEndDate = value;
    }

    /// <summary>
    /// A value of EdxportCarrier.
    /// </summary>
    [JsonPropertyName("edxportCarrier")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Name" })]
    public Employer EdxportCarrier
    {
      get => edxportCarrier ??= new();
      set => edxportCarrier = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 16, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80",
      "FileInstruction"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private CsePersonsWorkSet policyHolder;
    private TextWorkArea policyHolderBenfitNumb;
    private CsePersonsWorkSet coveredPerson;
    private TextWorkArea coverPersonBenfitNumb;
    private WorkArea carrierName;
    private EmployerAddress carrier;
    private TextWorkArea carrierStreetAddr1;
    private TextWorkArea carrierStreetAddr2;
    private WorkArea carrrierState;
    private WorkArea policyNumber;
    private WorkArea groupNumber;
    private WorkArea coverageCode;
    private WorkArea policyStartDate;
    private WorkArea policyEndDate;
    private Employer edxportCarrier;
    private External external;
  }
#endregion
}

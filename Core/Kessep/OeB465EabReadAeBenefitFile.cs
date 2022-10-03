// Program: OE_B465_EAB_READ_AE_BENEFIT_FILE, ID: 370980930, model: 746.
// Short name: SWEXOE05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B465_EAB_READ_AE_BENEFIT_FILE.
/// </summary>
[Serializable]
public partial class OeB465EabReadAeBenefitFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B465_EAB_READ_AE_BENEFIT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB465EabReadAeBenefitFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB465EabReadAeBenefitFile.
  /// </summary>
  public OeB465EabReadAeBenefitFile(IContext context, Import import,
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
      "SWEXOE05", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "AeCaseNo" })]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "Year",
      "Month",
      "Relationship",
      "GrantAmount"
    })]
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ProgramType.
    /// </summary>
    [JsonPropertyName("programType")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea ProgramType
    {
      get => programType ??= new();
      set => programType = value;
    }

    /// <summary>
    /// A value of IssueType.
    /// </summary>
    [JsonPropertyName("issueType")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea IssueType
    {
      get => issueType ??= new();
      set => issueType = value;
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

    private CsePerson csePerson;
    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private WorkArea programType;
    private WorkArea issueType;
    private EabFileHandling eabFileHandling;
  }
#endregion
}

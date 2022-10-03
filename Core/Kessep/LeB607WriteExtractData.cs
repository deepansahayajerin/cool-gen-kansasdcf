// Program: LE_B607_WRITE_EXTRACT_DATA, ID: 945198302, model: 746.
// Short name: SWEXEW18
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B607_WRITE_EXTRACT_DATA.
/// </summary>
[Serializable]
public partial class LeB607WriteExtractData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B607_WRITE_EXTRACT_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB607WriteExtractData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB607WriteExtractData.
  /// </summary>
  public LeB607WriteExtractData(IContext context, Import import, Export export):
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
      "SWEXEW18", context, import, export, EabOptions.Hpvp);
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "CountyAbbreviation", "County" })]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LeadAttrny.
    /// </summary>
    [JsonPropertyName("leadAttrny")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text2" })]
    public TextWorkArea LeadAttrny
    {
      get => leadAttrny ??= new();
      set => leadAttrny = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "StandardNumber", "Identifier" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of AttorInfo.
    /// </summary>
    [JsonPropertyName("attorInfo")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea AttorInfo
    {
      get => attorInfo ??= new();
      set => attorInfo = value;
    }

    /// <summary>
    /// A value of ApNumber.
    /// </summary>
    [JsonPropertyName("apNumber")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea ApNumber
    {
      get => apNumber ??= new();
      set => apNumber = value;
    }

    /// <summary>
    /// A value of ArNumber.
    /// </summary>
    [JsonPropertyName("arNumber")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea ArNumber
    {
      get => arNumber ??= new();
      set => arNumber = value;
    }

    private Fips fips;
    private TextWorkArea leadAttrny;
    private LegalAction legalAction;
    private EabFileHandling eabFileHandling;
    private WorkArea attorInfo;
    private WorkArea apNumber;
    private WorkArea arNumber;
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

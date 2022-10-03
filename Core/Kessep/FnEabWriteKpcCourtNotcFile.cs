// Program: FN_EAB_WRITE_KPC_COURT_NOTC_FILE, ID: 374440819, model: 746.
// Short name: SWEXFW01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EAB_WRITE_KPC_COURT_NOTC_FILE.
/// </summary>
[Serializable]
public partial class FnEabWriteKpcCourtNotcFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_WRITE_KPC_COURT_NOTC_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabWriteKpcCourtNotcFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabWriteKpcCourtNotcFile.
  /// </summary>
  public FnEabWriteKpcCourtNotcFile(IContext context, Import import,
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
      "SWEXFW01", context, import, export, EabOptions.Hpvp);
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
    /// A value of FnKpcCourtNotc.
    /// </summary>
    [JsonPropertyName("fnKpcCourtNotc")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "ObligorSsn",
      "LegalActionStandardNumber",
      "DistributionDate",
      "Amount",
      "CollectionType"
    })]
    public FnKpcCourtNotc FnKpcCourtNotc
    {
      get => fnKpcCourtNotc ??= new();
      set => fnKpcCourtNotc = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private FnKpcCourtNotc fnKpcCourtNotc;
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

// Program: FN_EXT_SWEFF15_WRITE_FILE, ID: 373404884, model: 746.
// Short name: SWEXWF15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_SWEFF15_WRITE_FILE.
/// </summary>
[Serializable]
public partial class FnExtSweff15WriteFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_SWEFF15_WRITE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtSweff15WriteFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtSweff15WriteFile.
  /// </summary>
  public FnExtSweff15WriteFile(IContext context, Import import, Export export):
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
      "SWEXWF15", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Action", "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of NumberDdshUpdated.
    /// </summary>
    [JsonPropertyName("numberDdshUpdated")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Count" })]
    public Common NumberDdshUpdated
    {
      get => numberDdshUpdated ??= new();
      set => numberDdshUpdated = value;
    }

    /// <summary>
    /// A value of NotUsed.
    /// </summary>
    [JsonPropertyName("notUsed")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "EffectiveDt",
      "DiscontinueDt",
      "SystemGeneratedIdentifier"
    })]
    public DebtDetailStatusHistory NotUsed
    {
      get => notUsed ??= new();
      set => notUsed = value;
    }

    /// <summary>
    /// A value of OldNotUsed.
    /// </summary>
    [JsonPropertyName("oldNotUsed")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "DiscontinueDt" })
      ]
    public DebtDetailStatusHistory OldNotUsed
    {
      get => oldNotUsed ??= new();
      set => oldNotUsed = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Common numberDdshUpdated;
    private DebtDetailStatusHistory notUsed;
    private DebtDetailStatusHistory oldNotUsed;
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

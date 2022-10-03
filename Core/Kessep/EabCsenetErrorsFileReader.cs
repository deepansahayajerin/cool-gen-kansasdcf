// Program: EAB_CSENET_ERRORS_FILE_READER, ID: 372895060, model: 746.
// Short name: SWEXI740
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CSENET_ERRORS_FILE_READER.
/// </summary>
[Serializable]
public partial class EabCsenetErrorsFileReader: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CSENET_ERRORS_FILE_READER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCsenetErrorsFileReader(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCsenetErrorsFileReader.
  /// </summary>
  public EabCsenetErrorsFileReader(IContext context, Import import,
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
      "SWEXI740", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsenetHostErrorFile.
    /// </summary>
    [JsonPropertyName("csenetHostErrorFile")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "TxnCounter",
      "LocalFipsCd",
      "OtherFipsCd",
      "LocalCaseId",
      "OtherCaseId",
      "ActionCd",
      "FunctionalTypeCd",
      "ActionReasonCd",
      "ActionResolutionDt",
      "TransactionDt",
      "ErrorCd",
      "ErrorMsg",
      "TransactionId"
    })]
    public CsenetHostErrorFile CsenetHostErrorFile
    {
      get => csenetHostErrorFile ??= new();
      set => csenetHostErrorFile = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private CsenetHostErrorFile csenetHostErrorFile;
    private EabFileHandling eabFileHandling;
  }
#endregion
}

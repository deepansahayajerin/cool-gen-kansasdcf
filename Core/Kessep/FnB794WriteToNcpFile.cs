// Program: FN_B794_WRITE_TO_NCP_FILE, ID: 1902454046, model: 746.
// Short name: SWEXEW21
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B794_WRITE_TO_NCP_FILE.
/// </summary>
[Serializable]
public partial class FnB794WriteToNcpFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B794_WRITE_TO_NCP_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB794WriteToNcpFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB794WriteToNcpFile.
  /// </summary>
  public FnB794WriteToNcpFile(IContext context, Import import, Export export):
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
      "SWEXEW21", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of RecordLength.
    /// </summary>
    [JsonPropertyName("recordLength")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common RecordLength
    {
      get => recordLength ??= new();
      set => recordLength = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common recordLength;
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

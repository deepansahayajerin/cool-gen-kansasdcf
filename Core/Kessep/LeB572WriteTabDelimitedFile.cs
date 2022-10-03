// Program: LE_B572_WRITE_TAB_DELIMITED_FILE, ID: 1902535069, model: 746.
// Short name: SWEXEW29
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B572_WRITE_TAB_DELIMITED_FILE.
/// </summary>
[Serializable]
public partial class LeB572WriteTabDelimitedFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B572_WRITE_TAB_DELIMITED_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB572WriteTabDelimitedFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB572WriteTabDelimitedFile.
  /// </summary>
  public LeB572WriteTabDelimitedFile(IContext context, Import import,
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
      "SWEXEW29", context, import, export, EabOptions.Hpvp);
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
    /// A value of AlaskaPermanentFund.
    /// </summary>
    [JsonPropertyName("alaskaPermanentFund")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "TabDelimitedRecord" })]
    public AlaskaPermanentFund AlaskaPermanentFund
    {
      get => alaskaPermanentFund ??= new();
      set => alaskaPermanentFund = value;
    }

    private EabFileHandling eabFileHandling;
    private AlaskaPermanentFund alaskaPermanentFund;
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

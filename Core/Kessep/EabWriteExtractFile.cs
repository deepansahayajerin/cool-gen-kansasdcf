// Program: EAB_WRITE_EXTRACT_FILE, ID: 372817553, model: 746.
// Short name: SWEXF740
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_WRITE_EXTRACT_FILE.
/// </summary>
[Serializable]
public partial class EabWriteExtractFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_EXTRACT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWriteExtractFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWriteExtractFile.
  /// </summary>
  public EabWriteExtractFile(IContext context, Import import, Export export):
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
      "SWEXF740", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of CollectionsExtract.
    /// </summary>
    [JsonPropertyName("collectionsExtract")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Office",
      "SectionSupervisor",
      "CollectionOfficer",
      "CaseNumber",
      "ObligationCode",
      "AppliedTo",
      "Amount1",
      "Amount2",
      "Amount3",
      "Amount4",
      "Amount5",
      "Amount6",
      "Amount7"
    })]
    public CollectionsExtract CollectionsExtract
    {
      get => collectionsExtract ??= new();
      set => collectionsExtract = value;
    }

    private ReportParms reportParms;
    private CollectionsExtract collectionsExtract;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}

// Program: OE_EAB_READ_IM_HHOLD_MULTI_CASE, ID: 372804996, model: 746.
// Short name: SWEXE666
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_READ_IM_HHOLD_MULTI_CASE.
/// </summary>
[Serializable]
public partial class OeEabReadImHholdMultiCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_IM_HHOLD_MULTI_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadImHholdMultiCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadImHholdMultiCase.
  /// </summary>
  public OeEabReadImHholdMultiCase(IContext context, Import import,
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
      "SWEXE666", context, import, export, EabOptions.Hpvp);
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "AeCaseNo" })]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
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

    /// <summary>
    /// A value of LocalEndOfFile.
    /// </summary>
    [JsonPropertyName("localEndOfFile")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common LocalEndOfFile
    {
      get => localEndOfFile ??= new();
      set => localEndOfFile = value;
    }

    private ImHousehold imHousehold;
    private EabFileHandling eabFileHandling;
    private Common localEndOfFile;
  }
#endregion
}

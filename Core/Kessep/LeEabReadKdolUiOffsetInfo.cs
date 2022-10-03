// Program: LE_EAB_READ_KDOL_UI_OFFSET_INFO, ID: 945096055, model: 746.
// Short name: SWEXLR03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EAB_READ_KDOL_UI_OFFSET_INFO.
/// </summary>
[Serializable]
public partial class LeEabReadKdolUiOffsetInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_READ_KDOL_UI_OFFSET_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabReadKdolUiOffsetInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabReadKdolUiOffsetInfo.
  /// </summary>
  public LeEabReadKdolUiOffsetInfo(IContext context, Import import,
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
      "SWEXLR03", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of KdolUiInboundFile.
    /// </summary>
    [JsonPropertyName("kdolUiInboundFile")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "UiWithholdingRecord" })]
    public KdolUiInboundFile KdolUiInboundFile
    {
      get => kdolUiInboundFile ??= new();
      set => kdolUiInboundFile = value;
    }

    private EabFileHandling eabFileHandling;
    private KdolUiInboundFile kdolUiInboundFile;
  }
#endregion
}

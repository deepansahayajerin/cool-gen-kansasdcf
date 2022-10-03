// Program: EAB_EXTRACT_EXIT_STATE_MESSAGE, ID: 371801929, model: 746.
// Short name: SWEXGE01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_EXTRACT_EXIT_STATE_MESSAGE.
/// </para>
/// <para>
/// This is an external action block that sets a work set attribute to the 
/// message text of the current exit state.
/// Whenever this action block is regenerated, the following changes must be 
/// made to the external COBOL stub:
/// 1)  Add the following statement to the Working-Storage Section:
///          77  GLOBDATA-ADDRESS POINTER.
/// 2)  Copy the full definition of '01  GLOBDATA.' from an IEF generated action
/// block into the Linkage Section. The statement '03  PSMGR-EAB-DATA.' within
/// '01  GLOBDATA.' should be changed to '03  PSMGR-EAB-DATA-X.'
/// 3)  Add the following two statements to the initialization paragrah within 
/// the Procedure Division:
///         CALL 'EABLINK' USING GLOBDATA-ADDRESS.
///         SET ADDRESS OF GLOBDATA TO GLOBDATA-ADDRESS.
/// 4)  Add the following statement to the main paragraph within the Procedure 
/// Division:
///         MOVE PSMGR-EXIT-INFOMSG TO MESSAGE-0001.
/// 5)  Compile the external action block and then recompile and install the IEF
/// action blocks.
/// </para>
/// </summary>
[Serializable]
public partial class EabExtractExitStateMessage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_EXTRACT_EXIT_STATE_MESSAGE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabExtractExitStateMessage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabExtractExitStateMessage.
  /// </summary>
  public EabExtractExitStateMessage(IContext context, Import import,
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
    GetService<IEabStub>().Execute("SWEXGE01", context, import, export, 0);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    [Member(Index = 1, Members = new[] { "Message" })]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
  }
#endregion
}

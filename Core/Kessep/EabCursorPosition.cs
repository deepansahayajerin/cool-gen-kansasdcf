// Program: EAB_CURSOR_POSITION, ID: 373512014, model: 746.
// Short name: SWE00086
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CURSOR_POSITION.
/// </summary>
[Serializable]
public partial class EabCursorPosition: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CURSOR_POSITION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCursorPosition(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCursorPosition.
  /// </summary>
  public EabCursorPosition(IContext context, Import import, Export export):
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
      "SWE00086", context, import, export, EabOptions.NoIefParams |
      EabOptions.NoAS);
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
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Row", "Column" })
      ]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
    }

    private CursorPosition cursorPosition;
  }
#endregion
}

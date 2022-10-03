// Program: W, ID: 371008891, model: 746.
// Short name: W
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse;

/// <summary>
/// A program: W.
/// </summary>
[Serializable]
public partial class W: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the W program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new W(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of W.
  /// </summary>
  public W(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
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
  }
#endregion
}

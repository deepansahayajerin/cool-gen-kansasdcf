// Program: EAB_CICS_COMMIT, ID: 374546248, model: 746.
// Short name: SWEXGC06
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CICS_COMMIT.
/// </summary>
[Serializable]
public partial class EabCicsCommit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CICS_COMMIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCicsCommit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCicsCommit.
  /// </summary>
  public EabCicsCommit(IContext context, Import import, Export export):
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
      "SWEXGC06", context, import, export, EabOptions.Hpvp |
      EabOptions.NoIefParams);
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

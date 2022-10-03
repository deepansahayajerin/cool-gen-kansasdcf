// Program: SC_EAB_SIGNOFF, ID: 371456893, model: 746.
// Short name: SWEXGW01
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SC_EAB_SIGNOFF.
/// </summary>
[Serializable]
public partial class ScEabSignoff: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_EAB_SIGNOFF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScEabSignoff(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScEabSignoff.
  /// </summary>
  public ScEabSignoff(IContext context, Import import, Export export):
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
      "SWEXGW01", context, import, export, EabOptions.NoIefParams);
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

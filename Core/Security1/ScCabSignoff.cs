// Program: SC_CAB_SIGNOFF, ID: 371424263, model: 746.
// Short name: SWE01081
using System;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Kessep;

namespace Gov.Kansas.DCF.Cse.Security1;

/// <summary>
/// A program: SC_CAB_SIGNOFF.
/// </summary>
[Serializable]
public partial class ScCabSignoff: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SC_CAB_SIGNOFF program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ScCabSignoff(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ScCabSignoff.
  /// </summary>
  public ScCabSignoff(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    UseScEabSignoff();
  }

  private void UseScEabSignoff()
  {
    var useImport = new ScEabSignoff.Import();
    var useExport = new ScEabSignoff.Export();

    Call(ScEabSignoff.Execute, useImport, useExport);
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

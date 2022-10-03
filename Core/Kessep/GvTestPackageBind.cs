// Program: GV_TEST_PACKAGE_BIND, ID: 945052454, model: 746.
// Short name: GVTESTPA
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: GV_TEST_PACKAGE_BIND.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class GvTestPackageBind: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GV_TEST_PACKAGE_BIND program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GvTestPackageBind(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GvTestPackageBind.
  /// </summary>
  public GvTestPackageBind(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // This procedure was written at Jim Fox's request to attempt to solve a 
    // package bind problem.
    // This procedure should never be executed.
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    UseGvCopyOfSwe02989();
  }

  private void UseGvCopyOfSwe02989()
  {
    var useImport = new GvCopyOfSwe02989.Import();
    var useExport = new GvCopyOfSwe02989.Export();

    Call(GvCopyOfSwe02989.Execute, useImport, useExport);
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

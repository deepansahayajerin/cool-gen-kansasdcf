// Program: EAB_ROLLBACK_CICS, ID: 371424352, model: 746.
// Short name: SWEXGRLB
using System;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_ROLLBACK_CICS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This external Common Action Block issues rollback using EXEC CICS SYNCPOINT 
/// ROLLBACK command. This needs to be used in CICS programs since EXEC SQL
/// ROLLBACK is not allowed in CICS environment.
/// </para>
/// </summary>
[Serializable]
public partial class EabRollbackCics: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ROLLBACK_CICS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabRollbackCics(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabRollbackCics.
  /// </summary>
  public EabRollbackCics(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    #region  MANUAL CODE by ADVANCED, REVIEWED by ... at ...
    //Orginal Code Begin
    //GetService<IEabStub>().Execute("SWEXGRLB", context, import, export, EabOptions.NoIefParams);
    //Orginal Code End
    context.Transaction.Rollback();
    #endregion
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

// Program: EXT_TO_DO_A_COMMIT, ID: 371787296, model: 746.
// Short name: SWEXFE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EXT_TO_DO_A_COMMIT.
/// </para>
/// <para>
/// This external will call a COBOL program to do a DB2 Commit.
/// </para>
/// </summary>
[Serializable]
public partial class ExtToDoACommit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EXT_TO_DO_A_COMMIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ExtToDoACommit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ExtToDoACommit.
  /// </summary>
  public ExtToDoACommit(IContext context, Import import, Export export):
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
    //GetService<IEabStub>().Execute("SWEXFE02", context, import, export, 0);
    //Orginal Code End
    context.Transaction.Commit();
    export.External.NumericReturnCode = 0;
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
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "NumericReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}

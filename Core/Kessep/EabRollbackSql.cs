// Program: EAB_ROLLBACK_SQL, ID: 371787297, model: 746.
// Short name: SWEXLE03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_ROLLBACK_SQL.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This external action block executes
/// EXEC SQL ROLLBACK.
/// For use in CICS environment, do not use this external action block. Instead 
/// use EAB_ROLLBACK_CICS.
/// </para>
/// </summary>
[Serializable]
public partial class EabRollbackSql: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_ROLLBACK_SQL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabRollbackSql(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabRollbackSql.
  /// </summary>
  public EabRollbackSql(IContext context, Import import, Export export):
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
    //GetService<IEabStub>().Execute("SWEXLE03", context, import, export, 0);
    //Orginal Code End
    context.Transaction.Rollback();
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

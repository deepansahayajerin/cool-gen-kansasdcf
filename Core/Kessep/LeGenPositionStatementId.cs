// Program: LE_GEN_POSITION_STATEMENT_ID, ID: 372604169, model: 746.
// Short name: SWE00775
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_GEN_POSITION_STATEMENT_ID.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This generates the sequential number identifier for Position Statement.
/// </para>
/// </summary>
[Serializable]
public partial class LeGenPositionStatementId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_GEN_POSITION_STATEMENT_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeGenPositionStatementId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeGenPositionStatementId.
  /// </summary>
  public LeGenPositionStatementId(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.PositionStatement.Number = 0;

    if (ReadPositionStatement())
    {
      // *********************************************
      // Get the last Position Statement Identifier
      // assigned.
      // *********************************************
    }

    export.PositionStatement.Number = entities.PositionStatement.Number + 1;
  }

  private bool ReadPositionStatement()
  {
    entities.PositionStatement.Populated = false;

    return Read("ReadPositionStatement",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.PositionStatement.AapIdentifier = db.GetInt32(reader, 0);
        entities.PositionStatement.Number = db.GetInt32(reader, 1);
        entities.PositionStatement.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PositionStatement.
    /// </summary>
    [JsonPropertyName("positionStatement")]
    public PositionStatement PositionStatement
    {
      get => positionStatement ??= new();
      set => positionStatement = value;
    }

    private PositionStatement positionStatement;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of PositionStatement.
    /// </summary>
    [JsonPropertyName("positionStatement")]
    public PositionStatement PositionStatement
    {
      get => positionStatement ??= new();
      set => positionStatement = value;
    }

    private AdministrativeAppeal administrativeAppeal;
    private PositionStatement positionStatement;
  }
#endregion
}

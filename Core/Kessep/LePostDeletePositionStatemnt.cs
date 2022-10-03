// Program: LE_POST_DELETE_POSITION_STATEMNT, ID: 372603748, model: 746.
// Short name: SWE00809
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_POST_DELETE_POSITION_STATEMNT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block deletes Position Statements.
/// </para>
/// </summary>
[Serializable]
public partial class LePostDeletePositionStatemnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_POST_DELETE_POSITION_STATEMNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LePostDeletePositionStatemnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LePostDeletePositionStatemnt.
  /// </summary>
  public LePostDeletePositionStatemnt(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAdministrativeAppeal())
    {
      if (ReadPositionStatement())
      {
        DeletePositionStatement();
      }
      else
      {
        ExitState = "POSITION_STATEMENT_NF";
      }
    }
    else
    {
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private void DeletePositionStatement()
  {
    Update("DeletePositionStatement",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier", entities.PositionStatement.AapIdentifier);
        db.
          SetInt32(command, "positionStmtNo", entities.PositionStatement.Number);
          
      });
  }

  private bool ReadAdministrativeAppeal()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadPositionStatement()
  {
    entities.PositionStatement.Populated = false;

    return Read("ReadPositionStatement",
      (db, command) =>
      {
        db.SetInt32(command, "positionStmtNo", import.PositionStatement.Number);
        db.SetInt32(
          command, "aapIdentifier", entities.AdministrativeAppeal.Identifier);
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
    /// A value of PositionStatement.
    /// </summary>
    [JsonPropertyName("positionStatement")]
    public PositionStatement PositionStatement
    {
      get => positionStatement ??= new();
      set => positionStatement = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private PositionStatement positionStatement;
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private PositionStatement positionStatement;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}

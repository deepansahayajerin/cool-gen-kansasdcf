// Program: LE_POST_UPDATE_POSITION_STATEMNT, ID: 372603745, model: 746.
// Short name: SWE00811
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_POST_UPDATE_POSITION_STATEMNT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block updates the Position Statement.
/// </para>
/// </summary>
[Serializable]
public partial class LePostUpdatePositionStatemnt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_POST_UPDATE_POSITION_STATEMNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LePostUpdatePositionStatemnt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LePostUpdatePositionStatemnt.
  /// </summary>
  public LePostUpdatePositionStatemnt(IContext context, Import import,
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
        try
        {
          UpdatePositionStatement();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "POSITION_STATEMENT_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
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
        entities.PositionStatement.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.PositionStatement.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 3);
        entities.PositionStatement.Explanation = db.GetString(reader, 4);
        entities.PositionStatement.Populated = true;
      });
  }

  private void UpdatePositionStatement()
  {
    System.Diagnostics.Debug.Assert(entities.PositionStatement.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var explanation = import.PositionStatement.Explanation;

    entities.PositionStatement.Populated = false;
    Update("UpdatePositionStatement",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetString(command, "explanation", explanation);
        db.SetInt32(
          command, "aapIdentifier", entities.PositionStatement.AapIdentifier);
        db.
          SetInt32(command, "positionStmtNo", entities.PositionStatement.Number);
          
      });

    entities.PositionStatement.LastUpdatedBy = lastUpdatedBy;
    entities.PositionStatement.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.PositionStatement.Explanation = explanation;
    entities.PositionStatement.Populated = true;
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

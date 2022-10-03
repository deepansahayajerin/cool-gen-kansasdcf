// Program: DEVELOP_POSITION_STATEMENT, ID: 372603747, model: 746.
// Short name: SWE00221
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DEVELOP_POSITION_STATEMENT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This process creates POSITION STATEMENT and associates it to ADMINISTRATIVE 
/// APPEAL.
/// </para>
/// </summary>
[Serializable]
public partial class DevelopPositionStatement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DEVELOP_POSITION_STATEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DevelopPositionStatement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DevelopPositionStatement.
  /// </summary>
  public DevelopPositionStatement(IContext context, Import import, Export export)
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
    if (ReadAdministrativeAppeal())
    {
      UseLeGenPositionStatementId();

      try
      {
        CreatePositionStatement();
        export.PositionStatement.Assign(entities.PositionStatement);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "POSITION_STATEMENT_AE";

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
      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";
    }
  }

  private void UseLeGenPositionStatementId()
  {
    var useImport = new LeGenPositionStatementId.Import();
    var useExport = new LeGenPositionStatementId.Export();

    useImport.AdministrativeAppeal.Identifier =
      import.AdministrativeAppeal.Identifier;

    Call(LeGenPositionStatementId.Execute, useImport, useExport);

    local.PositionStatement.Number = useExport.PositionStatement.Number;
  }

  private void CreatePositionStatement()
  {
    var aapIdentifier = entities.AdministrativeAppeal.Identifier;
    var number = local.PositionStatement.Number;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var explanation = import.PositionStatement.Explanation;

    entities.PositionStatement.Populated = false;
    Update("CreatePositionStatement",
      (db, command) =>
      {
        db.SetInt32(command, "aapIdentifier", aapIdentifier);
        db.SetInt32(command, "positionStmtNo", number);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetString(command, "explanation", explanation);
      });

    entities.PositionStatement.AapIdentifier = aapIdentifier;
    entities.PositionStatement.Number = number;
    entities.PositionStatement.CreatedBy = createdBy;
    entities.PositionStatement.CreatedTstamp = createdTstamp;
    entities.PositionStatement.LastUpdatedBy = "";
    entities.PositionStatement.LastUpdatedTstamp = null;
    entities.PositionStatement.Explanation = explanation;
    entities.PositionStatement.Populated = true;
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
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

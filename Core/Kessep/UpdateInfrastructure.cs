// Program: UPDATE_INFRASTRUCTURE, ID: 372067481, model: 746.
// Short name: SWE01857
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_INFRASTRUCTURE.
/// </para>
/// <para>
/// Elementary process.
/// </para>
/// </summary>
[Serializable]
public partial class UpdateInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_INFRASTRUCTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateInfrastructure.
  /// </summary>
  public UpdateInfrastructure(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsEmpty(import.Infrastructure.LastUpdatedBy))
    {
      local.Infrastructure.LastUpdatedBy =
        import.Infrastructure.LastUpdatedBy ?? "";
    }
    else
    {
      local.Infrastructure.LastUpdatedBy = global.UserId;
    }

    if (ReadInfrastructure())
    {
      try
      {
        UpdateInfrastructure1();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_INFRASTRUCTURE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_INFRASTRUCTURE_PV";

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
      ExitState = "SP0000_HISTORY_INFRASTRUCTURE_NF";
    }
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 1);
        entities.Infrastructure.LastUpdatedBy = db.GetNullableString(reader, 2);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.Infrastructure.Populated = true;
      });
  }

  private void UpdateInfrastructure1()
  {
    var processStatus = import.Infrastructure.ProcessStatus;
    var lastUpdatedBy = local.Infrastructure.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = Now();

    entities.Infrastructure.Populated = false;
    Update("UpdateInfrastructure",
      (db, command) =>
      {
        db.SetString(command, "processStatus", processStatus);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.ProcessStatus = processStatus;
    entities.Infrastructure.LastUpdatedBy = lastUpdatedBy;
    entities.Infrastructure.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Infrastructure.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }
#endregion
}

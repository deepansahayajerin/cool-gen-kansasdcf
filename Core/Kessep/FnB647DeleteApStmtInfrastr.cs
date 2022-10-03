// Program: FN_B647_DELETE_AP_STMT_INFRASTR, ID: 372995147, model: 746.
// Short name: SWE02398
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B647_DELETE_AP_STMT_INFRASTR.
/// </summary>
[Serializable]
public partial class FnB647DeleteApStmtInfrastr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B647_DELETE_AP_STMT_INFRASTR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB647DeleteApStmtInfrastr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB647DeleteApStmtInfrastr.
  /// </summary>
  public FnB647DeleteApStmtInfrastr(IContext context, Import import,
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
    foreach(var item in ReadInfrastructure())
    {
      DeleteInfrastructure();
      ++local.Deleted.Count;
      ++export.Deleted.Count;

      if (local.Deleted.Count > import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // ***** Call an external that does a DB2 commit using a Cobol program.
        UseExtToDoACommit();
        local.Deleted.Count = 0;
      }
    }
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void DeleteInfrastructure()
  {
    Update("DeleteInfrastructure#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });

    Update("DeleteInfrastructure#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      });
  }

  private IEnumerable<bool> ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(command, "eventId", import.Event1.ControlNumber);
        db.SetString(command, "type", import.Event1.Type1);
        db.SetString(command, "reasonCode", import.EventDetail.ReasonCode);
        db.SetNullableDate(
          command, "referenceDate1", import.Purge.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "referenceDate2", import.Deletion.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.EventId = db.GetInt32(reader, 1);
        entities.Infrastructure.EventType = db.GetString(reader, 2);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 3);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 4);
        entities.Infrastructure.Populated = true;

        return true;
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
    /// A value of Purge.
    /// </summary>
    [JsonPropertyName("purge")]
    public DateWorkArea Purge
    {
      get => purge ??= new();
      set => purge = value;
    }

    /// <summary>
    /// A value of Deletion.
    /// </summary>
    [JsonPropertyName("deletion")]
    public DateWorkArea Deletion
    {
      get => deletion ??= new();
      set => deletion = value;
    }

    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea purge;
    private DateWorkArea deletion;
    private Event1 event1;
    private EventDetail eventDetail;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public Common Deleted
    {
      get => deleted ??= new();
      set => deleted = value;
    }

    private Common deleted;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public Common Deleted
    {
      get => deleted ??= new();
      set => deleted = value;
    }

    private Common deleted;
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

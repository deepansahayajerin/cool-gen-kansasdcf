// Program: SP_DELETE_GLOBAL_REASSIGNMENT, ID: 372453374, model: 746.
// Short name: SWE02197
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DELETE_GLOBAL_REASSIGNMENT.
/// </para>
/// <para>
/// This action block deletes an occurrence of Global_Reassignment.
/// </para>
/// </summary>
[Serializable]
public partial class SpDeleteGlobalReassignment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_GLOBAL_REASSIGNMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteGlobalReassignment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteGlobalReassignment.
  /// </summary>
  public SpDeleteGlobalReassignment(IContext context, Import import,
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
    // Initial development:  J.Rookard, MTW  01/28/1998
    if (ReadGlobalReassignment())
    {
      DeleteGlobalReassignment();
    }
    else
    {
      ExitState = "SP0000_GLOBAL_REASSIGNMENT_NF";
    }
  }

  private void DeleteGlobalReassignment()
  {
    Update("DeleteGlobalReassignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GlobalReassignment.CreatedTimestamp.GetValueOrDefault());
      });
  }

  private bool ReadGlobalReassignment()
  {
    entities.GlobalReassignment.Populated = false;

    return Read("ReadGlobalReassignment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.GlobalReassignment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.GlobalReassignment.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.GlobalReassignment.CreatedBy = db.GetString(reader, 1);
        entities.GlobalReassignment.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.GlobalReassignment.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.GlobalReassignment.ProcessDate = db.GetDate(reader, 4);
        entities.GlobalReassignment.StatusFlag = db.GetString(reader, 5);
        entities.GlobalReassignment.OverrideFlag = db.GetString(reader, 6);
        entities.GlobalReassignment.BusinessObjectCode =
          db.GetString(reader, 7);
        entities.GlobalReassignment.AssignmentReasonCode =
          db.GetString(reader, 8);
        entities.GlobalReassignment.BoCount = db.GetNullableInt32(reader, 9);
        entities.GlobalReassignment.MonCount = db.GetNullableInt32(reader, 10);
        entities.GlobalReassignment.Populated = true;
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
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    private GlobalReassignment globalReassignment;
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
    /// A value of GlobalReassignment.
    /// </summary>
    [JsonPropertyName("globalReassignment")]
    public GlobalReassignment GlobalReassignment
    {
      get => globalReassignment ??= new();
      set => globalReassignment = value;
    }

    private GlobalReassignment globalReassignment;
  }
#endregion
}

// Program: SP_CAB_DELETE_ACTIVITY, ID: 371745521, model: 746.
// Short name: SWE01738
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_ACTIVITY.
/// </summary>
[Serializable]
public partial class SpCabDeleteActivity: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_ACTIVITY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteActivity(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteActivity.
  /// </summary>
  public SpCabDeleteActivity(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Delete is not allowed if related activity
    // details exist.
    // ---------------------------------------------
    if (ReadActivityDetail())
    {
      ExitState = "SP0000_RELATED_DETAILS_EXIST";
    }
    else if (ReadActivity())
    {
      DeleteActivity();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "SP0000_ACTIVITY_NF";
    }
  }

  private void DeleteActivity()
  {
    Update("DeleteActivity#1",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", entities.Activity.ControlNumber);
      });

    Update("DeleteActivity#2",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", entities.Activity.ControlNumber);
      });

    Update("DeleteActivity#3",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", entities.Activity.ControlNumber);
      });
  }

  private bool ReadActivity()
  {
    entities.Activity.Populated = false;

    return Read("ReadActivity",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Activity.ControlNumber = db.GetInt32(reader, 0);
        entities.Activity.Populated = true;
      });
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 1);
        entities.ActivityDetail.Populated = true;
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
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private Activity activity;
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
    /// A value of ActivityDetail.
    /// </summary>
    [JsonPropertyName("activityDetail")]
    public ActivityDetail ActivityDetail
    {
      get => activityDetail ??= new();
      set => activityDetail = value;
    }

    /// <summary>
    /// A value of Activity.
    /// </summary>
    [JsonPropertyName("activity")]
    public Activity Activity
    {
      get => activity ??= new();
      set => activity = value;
    }

    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

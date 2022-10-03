// Program: SP_CAB_DELETE_ACTIVITY_DETAIL, ID: 371744532, model: 746.
// Short name: SWE01747
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_ACTIVITY_DETAIL.
/// </summary>
[Serializable]
public partial class SpCabDeleteActivityDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_ACTIVITY_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteActivityDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteActivityDetail.
  /// </summary>
  public SpCabDeleteActivityDetail(IContext context, Import import,
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
    // ---------------------------------------------
    // Entity cannot be deleted if dependent
    // entities exist.
    // ---------------------------------------------
    if (ReadActivityDetail())
    {
      if (ReadActivityStartStop())
      {
        ExitState = "SP0000_RELATED_DETAILS_EXIST";
      }
      else
      {
        DeleteActivityDetail();
      }
    }
    else
    {
      ExitState = "SP0000_ACTIVITY_DETAIL_NF";
    }
  }

  private void DeleteActivityDetail()
  {
    Update("DeleteActivityDetail#1",
      (db, command) =>
      {
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", entities.ActivityDetail.ActNo);
      });

    Update("DeleteActivityDetail#2",
      (db, command) =>
      {
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", entities.ActivityDetail.ActNo);
      });
  }

  private bool ReadActivityDetail()
  {
    entities.ActivityDetail.Populated = false;

    return Read("ReadActivityDetail",
      (db, command) =>
      {
        db.SetInt32(command, "actNo", import.Activity.ControlNumber);
        db.SetInt32(
          command, "systemGeneratedI",
          import.ActivityDetail.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ActivityDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ActivityDetail.ActNo = db.GetInt32(reader, 1);
        entities.ActivityDetail.Populated = true;
      });
  }

  private bool ReadActivityStartStop()
  {
    System.Diagnostics.Debug.Assert(entities.ActivityDetail.Populated);
    entities.ActivityStartStop.Populated = false;

    return Read("ReadActivityStartStop",
      (db, command) =>
      {
        db.SetInt32(
          command, "acdId", entities.ActivityDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "actNo", entities.ActivityDetail.ActNo);
      },
      (db, reader) =>
      {
        entities.ActivityStartStop.ActionCode = db.GetString(reader, 0);
        entities.ActivityStartStop.ActNo = db.GetInt32(reader, 1);
        entities.ActivityStartStop.AcdId = db.GetInt32(reader, 2);
        entities.ActivityStartStop.EveNo = db.GetInt32(reader, 3);
        entities.ActivityStartStop.EvdId = db.GetInt32(reader, 4);
        entities.ActivityStartStop.Populated = true;
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
    /// A value of ActivityStartStop.
    /// </summary>
    [JsonPropertyName("activityStartStop")]
    public ActivityStartStop ActivityStartStop
    {
      get => activityStartStop ??= new();
      set => activityStartStop = value;
    }

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

    private ActivityStartStop activityStartStop;
    private ActivityDetail activityDetail;
    private Activity activity;
  }
#endregion
}

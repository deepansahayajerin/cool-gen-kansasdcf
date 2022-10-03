// Program: SP_CAB_UPDATE_LC_TRANSFORMATION, ID: 371779419, model: 746.
// Short name: SWE01729
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_UPDATE_LC_TRANSFORMATION.
/// </summary>
[Serializable]
public partial class SpCabUpdateLcTransformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_LC_TRANSFORMATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateLcTransformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateLcTransformation.
  /// </summary>
  public SpCabUpdateLcTransformation(IContext context, Import import,
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
    if (ReadLifecycleTransformation())
    {
      try
      {
        UpdateLifecycleTransformation();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_LIFECYCLE_TRANS_AE";

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
      ExitState = "SP0000_LIFECYCLE_TRANS_NF";
    }
  }

  private bool ReadLifecycleTransformation()
  {
    entities.LifecycleTransformation.Populated = false;

    return Read("ReadLifecycleTransformation",
      (db, command) =>
      {
        db.SetInt32(
          command, "evdIdPri",
          import.CausedByEventDetail.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "eveCtrlNoPri", import.CausedByEvent.ControlNumber);
          
        db.SetNullableInt32(
          command, "evdLctIdSec",
          import.TargetedEventDetail.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "eveNoSec", import.TargetedEvent.ControlNumber);
        db.SetString(command, "lcsLctIdSec", import.Resulting.Identifier);
        db.SetString(command, "lcsIdPri", import.Current.Identifier);
      },
      (db, reader) =>
      {
        entities.LifecycleTransformation.Description = db.GetString(reader, 0);
        entities.LifecycleTransformation.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.LifecycleTransformation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.LifecycleTransformation.LcsIdPri = db.GetString(reader, 3);
        entities.LifecycleTransformation.EveCtrlNoPri = db.GetInt32(reader, 4);
        entities.LifecycleTransformation.EvdIdPri = db.GetInt32(reader, 5);
        entities.LifecycleTransformation.LcsLctIdSec = db.GetString(reader, 6);
        entities.LifecycleTransformation.EveNoSec =
          db.GetNullableInt32(reader, 7);
        entities.LifecycleTransformation.EvdLctIdSec =
          db.GetNullableInt32(reader, 8);
        entities.LifecycleTransformation.Populated = true;
      });
  }

  private void UpdateLifecycleTransformation()
  {
    System.Diagnostics.Debug.Assert(entities.LifecycleTransformation.Populated);

    var description = import.LifecycleTransformation.Description;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.LifecycleTransformation.Populated = false;
    Update("UpdateLifecycleTransformation",
      (db, command) =>
      {
        db.SetString(command, "description", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(
          command, "lcsIdPri", entities.LifecycleTransformation.LcsIdPri);
        db.SetInt32(
          command, "eveCtrlNoPri",
          entities.LifecycleTransformation.EveCtrlNoPri);
        db.SetInt32(
          command, "evdIdPri", entities.LifecycleTransformation.EvdIdPri);
        db.SetString(
          command, "lcsLctIdSec", entities.LifecycleTransformation.LcsLctIdSec);
          
      });

    entities.LifecycleTransformation.Description = description;
    entities.LifecycleTransformation.LastUpdatedBy = lastUpdatedBy;
    entities.LifecycleTransformation.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.LifecycleTransformation.Populated = true;
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
    /// A value of LifecycleTransformation.
    /// </summary>
    [JsonPropertyName("lifecycleTransformation")]
    public LifecycleTransformation LifecycleTransformation
    {
      get => lifecycleTransformation ??= new();
      set => lifecycleTransformation = value;
    }

    /// <summary>
    /// A value of TargetedEventDetail.
    /// </summary>
    [JsonPropertyName("targetedEventDetail")]
    public EventDetail TargetedEventDetail
    {
      get => targetedEventDetail ??= new();
      set => targetedEventDetail = value;
    }

    /// <summary>
    /// A value of TargetedEvent.
    /// </summary>
    [JsonPropertyName("targetedEvent")]
    public Event1 TargetedEvent
    {
      get => targetedEvent ??= new();
      set => targetedEvent = value;
    }

    /// <summary>
    /// A value of CausedByEvent.
    /// </summary>
    [JsonPropertyName("causedByEvent")]
    public Event1 CausedByEvent
    {
      get => causedByEvent ??= new();
      set => causedByEvent = value;
    }

    /// <summary>
    /// A value of CausedByEventDetail.
    /// </summary>
    [JsonPropertyName("causedByEventDetail")]
    public EventDetail CausedByEventDetail
    {
      get => causedByEventDetail ??= new();
      set => causedByEventDetail = value;
    }

    /// <summary>
    /// A value of Resulting.
    /// </summary>
    [JsonPropertyName("resulting")]
    public LifecycleState Resulting
    {
      get => resulting ??= new();
      set => resulting = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public LifecycleState Current
    {
      get => current ??= new();
      set => current = value;
    }

    private LifecycleTransformation lifecycleTransformation;
    private EventDetail targetedEventDetail;
    private Event1 targetedEvent;
    private Event1 causedByEvent;
    private EventDetail causedByEventDetail;
    private LifecycleState resulting;
    private LifecycleState current;
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
    /// A value of LifecycleTransformation.
    /// </summary>
    [JsonPropertyName("lifecycleTransformation")]
    public LifecycleTransformation LifecycleTransformation
    {
      get => lifecycleTransformation ??= new();
      set => lifecycleTransformation = value;
    }

    /// <summary>
    /// A value of TargetedEventDetail.
    /// </summary>
    [JsonPropertyName("targetedEventDetail")]
    public EventDetail TargetedEventDetail
    {
      get => targetedEventDetail ??= new();
      set => targetedEventDetail = value;
    }

    /// <summary>
    /// A value of TargetedEvent.
    /// </summary>
    [JsonPropertyName("targetedEvent")]
    public Event1 TargetedEvent
    {
      get => targetedEvent ??= new();
      set => targetedEvent = value;
    }

    /// <summary>
    /// A value of CausedByEvent.
    /// </summary>
    [JsonPropertyName("causedByEvent")]
    public Event1 CausedByEvent
    {
      get => causedByEvent ??= new();
      set => causedByEvent = value;
    }

    /// <summary>
    /// A value of CausedByEventDetail.
    /// </summary>
    [JsonPropertyName("causedByEventDetail")]
    public EventDetail CausedByEventDetail
    {
      get => causedByEventDetail ??= new();
      set => causedByEventDetail = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public LifecycleState Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Resulting.
    /// </summary>
    [JsonPropertyName("resulting")]
    public LifecycleState Resulting
    {
      get => resulting ??= new();
      set => resulting = value;
    }

    private LifecycleTransformation lifecycleTransformation;
    private EventDetail targetedEventDetail;
    private Event1 targetedEvent;
    private Event1 causedByEvent;
    private EventDetail causedByEventDetail;
    private LifecycleState current;
    private LifecycleState resulting;
  }
#endregion
}

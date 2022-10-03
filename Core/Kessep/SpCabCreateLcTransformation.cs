// Program: SP_CAB_CREATE_LC_TRANSFORMATION, ID: 371779420, model: 746.
// Short name: SWE01727
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_CREATE_LC_TRANSFORMATION.
/// </summary>
[Serializable]
public partial class SpCabCreateLcTransformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_LC_TRANSFORMATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateLcTransformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateLcTransformation.
  /// </summary>
  public SpCabCreateLcTransformation(IContext context, Import import,
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
    if (ReadEventDetailEvent1())
    {
      if (ReadEventDetailEvent2())
      {
        if (ReadLifecycleState1())
        {
          if (ReadLifecycleState2())
          {
            try
            {
              CreateLifecycleTransformation();
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
            ExitState = "SP0000_LIFECYCLE_STATE_NF";
          }
        }
        else
        {
          ExitState = "SP0000_LIFECYCLE_STATE_NF";
        }
      }
      else
      {
        ExitState = "ZD_SP0000_EVENT_DETAIL_NF1";
      }
    }
    else
    {
      ExitState = "ZD_SP0000_EVENT_DETAIL_NF1";
    }
  }

  private void CreateLifecycleTransformation()
  {
    System.Diagnostics.Debug.Assert(entities.TargetedEventDetail.Populated);
    System.Diagnostics.Debug.Assert(entities.CausedByEventDetail.Populated);

    var description = import.LifecycleTransformation.Description;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lcsIdPri = entities.Current.Identifier;
    var eveCtrlNoPri = entities.CausedByEventDetail.EveNo;
    var evdIdPri = entities.CausedByEventDetail.SystemGeneratedIdentifier;
    var lcsLctIdSec = entities.Resulting.Identifier;
    var eveNoSec = entities.TargetedEventDetail.EveNo;
    var evdLctIdSec = entities.TargetedEventDetail.SystemGeneratedIdentifier;

    entities.New1.Populated = false;
    Update("CreateLifecycleTransformation",
      (db, command) =>
      {
        db.SetString(command, "description", description);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "lcsIdPri", lcsIdPri);
        db.SetInt32(command, "eveCtrlNoPri", eveCtrlNoPri);
        db.SetInt32(command, "evdIdPri", evdIdPri);
        db.SetString(command, "lcsLctIdSec", lcsLctIdSec);
        db.SetNullableInt32(command, "eveNoSec", eveNoSec);
        db.SetNullableInt32(command, "evdLctIdSec", evdLctIdSec);
      });

    entities.New1.Description = description;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LcsIdPri = lcsIdPri;
    entities.New1.EveCtrlNoPri = eveCtrlNoPri;
    entities.New1.EvdIdPri = evdIdPri;
    entities.New1.LcsLctIdSec = lcsLctIdSec;
    entities.New1.EveNoSec = eveNoSec;
    entities.New1.EvdLctIdSec = evdLctIdSec;
    entities.New1.Populated = true;
  }

  private bool ReadEventDetailEvent1()
  {
    entities.CausedByEvent.Populated = false;
    entities.CausedByEventDetail.Populated = false;

    return Read("ReadEventDetailEvent1",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.CausedByEventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.CausedByEvent.ControlNumber);
      },
      (db, reader) =>
      {
        entities.CausedByEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CausedByEventDetail.EveNo = db.GetInt32(reader, 1);
        entities.CausedByEvent.ControlNumber = db.GetInt32(reader, 1);
        entities.CausedByEvent.Populated = true;
        entities.CausedByEventDetail.Populated = true;
      });
  }

  private bool ReadEventDetailEvent2()
  {
    entities.TargetedEventDetail.Populated = false;
    entities.TargetedEvent.Populated = false;

    return Read("ReadEventDetailEvent2",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.TargetedEventDetail.SystemGeneratedIdentifier);
        db.SetInt32(command, "eveNo", import.TargetedEvent.ControlNumber);
      },
      (db, reader) =>
      {
        entities.TargetedEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.TargetedEventDetail.EveNo = db.GetInt32(reader, 1);
        entities.TargetedEvent.ControlNumber = db.GetInt32(reader, 1);
        entities.TargetedEventDetail.Populated = true;
        entities.TargetedEvent.Populated = true;
      });
  }

  private bool ReadLifecycleState1()
  {
    entities.Current.Populated = false;

    return Read("ReadLifecycleState1",
      (db, command) =>
      {
        db.SetString(command, "identifier", import.Current.Identifier);
      },
      (db, reader) =>
      {
        entities.Current.Identifier = db.GetString(reader, 0);
        entities.Current.Populated = true;
      });
  }

  private bool ReadLifecycleState2()
  {
    entities.Resulting.Populated = false;

    return Read("ReadLifecycleState2",
      (db, command) =>
      {
        db.SetString(command, "identifier", import.Resulting.Identifier);
      },
      (db, reader) =>
      {
        entities.Resulting.Identifier = db.GetString(reader, 0);
        entities.Resulting.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public LifecycleTransformation New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private LifecycleTransformation new1;
    private EventDetail targetedEventDetail;
    private Event1 targetedEvent;
    private Event1 causedByEvent;
    private EventDetail causedByEventDetail;
    private LifecycleState current;
    private LifecycleState resulting;
  }
#endregion
}

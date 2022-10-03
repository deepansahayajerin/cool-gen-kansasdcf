// Program: SP_CAB_READ_EACH_EVENT_W_START, ID: 371778675, model: 746.
// Short name: SWE01679
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_READ_EACH_EVENT_W_START.
/// </para>
/// <para>
///   Read each of table EVENT with starting position.  Can be initialized to 
/// spaces.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabReadEachEventWStart: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_READ_EACH_EVENT_W_START program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabReadEachEventWStart(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabReadEachEventWStart.
  /// </summary>
  public SpCabReadEachEventWStart(IContext context, Import import, Export export)
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
    // ---------------------------------------------------------------------------------------
    //                      M A I N T E N A N C E       L O G
    //   Date   Devloper PR/WR #  Description
    // 08/16/00 SWSRCHF  000170   Do not select external events when the event 
    // detail
    //                            does not have discontinue date of '2099-12-31'
    // ---------------------------------------------------------------------------------------
    export.Group.Index = 0;
    export.Group.Clear();

    foreach(var item in ReadEvent())
    {
      // *** Work request 000170
      // *** 08/16/00 SWSRCHF
      // *** start
      if (Equal(import.PassFilterEventType.Type1, "EXTERNAL"))
      {
        local.Max.DiscontinueDate = new DateTime(2099, 12, 31);

        if (ReadEventDetail())
        {
          goto Test;
        }

        export.Group.Next();

        continue;
      }

Test:

      // *** end
      // *** 08/16/00 SWSRCHF
      // *** Work request 000170
      export.Group.Update.GrEvent.Assign(entities.ExistingEvent);
      export.Group.Next();
    }

    if (export.Group.IsEmpty)
    {
      ExitState = "ACO_NI0000_GROUP_VIEW_IS_EMPTY";
    }
    else
    {
      ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
    }
  }

  private IEnumerable<bool> ReadEvent()
  {
    return ReadEach("ReadEvent",
      (db, command) =>
      {
        db.SetString(command, "name", import.Start.Name);
        db.SetString(command, "type", import.PassFilterEventType.Type1);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

        entities.ExistingEvent.ControlNumber = db.GetInt32(reader, 0);
        entities.ExistingEvent.Name = db.GetString(reader, 1);
        entities.ExistingEvent.Type1 = db.GetString(reader, 2);
        entities.ExistingEvent.BusinessObjectCode = db.GetString(reader, 3);
        entities.ExistingEvent.Populated = true;

        return true;
      });
  }

  private bool ReadEventDetail()
  {
    entities.ExistingEventDetail.Populated = false;

    return Read("ReadEventDetail",
      (db, command) =>
      {
        db.SetInt32(command, "eveNo", entities.ExistingEvent.ControlNumber);
        db.SetDate(
          command, "discontinueDate",
          local.Max.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingEventDetail.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingEventDetail.DiscontinueDate = db.GetDate(reader, 1);
        entities.ExistingEventDetail.EveNo = db.GetInt32(reader, 2);
        entities.ExistingEventDetail.Populated = true;
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
    /// A value of PassFilterEventType.
    /// </summary>
    [JsonPropertyName("passFilterEventType")]
    public Event1 PassFilterEventType
    {
      get => passFilterEventType ??= new();
      set => passFilterEventType = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Event1 Start
    {
      get => start ??= new();
      set => start = value;
    }

    private Event1 passFilterEventType;
    private Event1 start;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GrCommon.
      /// </summary>
      [JsonPropertyName("grCommon")]
      public Common GrCommon
      {
        get => grCommon ??= new();
        set => grCommon = value;
      }

      /// <summary>
      /// A value of GrEvent.
      /// </summary>
      [JsonPropertyName("grEvent")]
      public Event1 GrEvent
      {
        get => grEvent ??= new();
        set => grEvent = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common grCommon;
      private Event1 grEvent;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public EventDetail Max
    {
      get => max ??= new();
      set => max = value;
    }

    private EventDetail max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    private EventDetail existingEventDetail;
    private Event1 existingEvent;
  }
#endregion
}

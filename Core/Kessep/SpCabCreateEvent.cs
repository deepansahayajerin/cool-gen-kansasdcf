// Program: SP_CAB_CREATE_EVENT, ID: 371778862, model: 746.
// Short name: SWE01682
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_CREATE_EVENT.
/// </para>
/// <para>
///   Creates event from input values.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabCreateEvent: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_EVENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateEvent(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateEvent.
  /// </summary>
  public SpCabCreateEvent(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.Event1.Assign(import.Event1);

    try
    {
      CreateEvent();
      export.HiddenCheck.Assign(entities.Event1);
      ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "SP0000_EVENT_AE";

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

  private void CreateEvent()
  {
    var controlNumber = import.Event1.ControlNumber;
    var name = import.Event1.Name;
    var type1 = import.Event1.Type1;
    var description = import.Event1.Description ?? "";
    var businessObjectCode = import.Event1.BusinessObjectCode;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.Event1.Populated = false;
    Update("CreateEvent",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", controlNumber);
        db.SetString(command, "name", name);
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "businessObjectCd", businessObjectCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.Event1.ControlNumber = controlNumber;
    entities.Event1.Name = name;
    entities.Event1.Type1 = type1;
    entities.Event1.Description = description;
    entities.Event1.BusinessObjectCode = businessObjectCode;
    entities.Event1.CreatedBy = createdBy;
    entities.Event1.CreatedTimestamp = createdTimestamp;
    entities.Event1.LastUpdatedBy = "";
    entities.Event1.LastUpdatedTimestamp = null;
    entities.Event1.Populated = true;
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private Event1 event1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of HiddenCheck.
    /// </summary>
    [JsonPropertyName("hiddenCheck")]
    public Event1 HiddenCheck
    {
      get => hiddenCheck ??= new();
      set => hiddenCheck = value;
    }

    private Event1 event1;
    private Event1 hiddenCheck;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
    }

    private Event1 event1;
  }
#endregion
}

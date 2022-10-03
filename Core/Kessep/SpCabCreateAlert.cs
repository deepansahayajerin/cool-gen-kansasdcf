// Program: SP_CAB_CREATE_ALERT, ID: 371749001, model: 746.
// Short name: SWE01743
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_CREATE_ALERT.
/// </para>
/// <para>
///   Performs create of table ALERT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabCreateAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_CREATE_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabCreateAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabCreateAlert.
  /// </summary>
  public SpCabCreateAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (AsChar(import.Alert.ExternalIndicator) == 'N')
    {
      if (ReadAlert())
      {
        local.Alert.ControlNumber = entities.Alert.ControlNumber + 1;
      }

      if (entities.Alert.ControlNumber < 100)
      {
        local.Alert.ControlNumber = 100;
      }
    }
    else
    {
      local.Alert.ControlNumber = import.Alert.ControlNumber;
    }

    try
    {
      CreateAlert();
      export.Alert.Assign(entities.New1);
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

  private void CreateAlert()
  {
    var controlNumber = local.Alert.ControlNumber;
    var name = import.Alert.Name;
    var message = import.Alert.Message;
    var description = import.Alert.Description ?? "";
    var externalIndicator = import.Alert.ExternalIndicator;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.New1.Populated = false;
    Update("CreateAlert",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", controlNumber);
        db.SetString(command, "name", name);
        db.SetString(command, "message", message);
        db.SetNullableString(command, "description", description);
        db.SetString(command, "externalIndicator", externalIndicator);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
      });

    entities.New1.ControlNumber = controlNumber;
    entities.New1.Name = name;
    entities.New1.Message = message;
    entities.New1.Description = description;
    entities.New1.ExternalIndicator = externalIndicator;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = null;
    entities.New1.Populated = true;
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      null,
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Populated = true;
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
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Alert alert;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Alert alert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Alert alert;
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
    public Alert New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    private Alert new1;
    private Alert alert;
  }
#endregion
}

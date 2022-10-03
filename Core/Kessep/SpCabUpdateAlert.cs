// Program: SP_CAB_UPDATE_ALERT, ID: 371749003, model: 746.
// Short name: SWE01745
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_ALERT.
/// </para>
/// <para>
///   Performs update of table ALERT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateAlert.
  /// </summary>
  public SpCabUpdateAlert(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAlert())
    {
      try
      {
        UpdateAlert();
        export.Alert.Assign(entities.Alert);
        ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_ALERT_NU";

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
      ExitState = "SP0000_ALERT_NF";
    }
  }

  private bool ReadAlert()
  {
    entities.Alert.Populated = false;

    return Read("ReadAlert",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", import.Alert.ControlNumber);
      },
      (db, reader) =>
      {
        entities.Alert.ControlNumber = db.GetInt32(reader, 0);
        entities.Alert.Name = db.GetString(reader, 1);
        entities.Alert.Message = db.GetString(reader, 2);
        entities.Alert.Description = db.GetNullableString(reader, 3);
        entities.Alert.ExternalIndicator = db.GetString(reader, 4);
        entities.Alert.CreatedBy = db.GetString(reader, 5);
        entities.Alert.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Alert.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Alert.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 8);
        entities.Alert.Populated = true;
      });
  }

  private void UpdateAlert()
  {
    var name = import.Alert.Name;
    var message = import.Alert.Message;
    var description = import.Alert.Description ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Alert.Populated = false;
    Update("UpdateAlert",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetString(command, "message", message);
        db.SetNullableString(command, "description", description);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "controlNumber", entities.Alert.ControlNumber);
      });

    entities.Alert.Name = name;
    entities.Alert.Message = message;
    entities.Alert.Description = description;
    entities.Alert.LastUpdatedBy = lastUpdatedBy;
    entities.Alert.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Alert.Populated = true;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}

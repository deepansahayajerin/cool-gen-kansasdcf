// Program: SP_CAB_DELETE_ALERT, ID: 371749002, model: 746.
// Short name: SWE01744
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_DELETE_ALERT.
/// </para>
/// <para>
///   Performs delete of ALERT.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabDeleteAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteAlert.
  /// </summary>
  public SpCabDeleteAlert(IContext context, Import import, Export export):
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
      DeleteAlert();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
    else
    {
      ExitState = "SP0000_ALERT_NF";
    }
  }

  private void DeleteAlert()
  {
    Update("DeleteAlert",
      (db, command) =>
      {
        db.SetInt32(command, "controlNumber", entities.Alert.ControlNumber);
      });
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

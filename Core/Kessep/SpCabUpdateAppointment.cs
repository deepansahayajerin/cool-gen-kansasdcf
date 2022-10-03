// Program: SP_CAB_UPDATE_APPOINTMENT, ID: 371749366, model: 746.
// Short name: SWE01710
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_CAB_UPDATE_APPOINTMENT.
/// </para>
/// <para>
///   This cab will update an appointment.  The ONLY thing that can be updated 
/// on an appointment is the result code.
/// </para>
/// </summary>
[Serializable]
public partial class SpCabUpdateAppointment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_UPDATE_APPOINTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabUpdateAppointment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabUpdateAppointment.
  /// </summary>
  public SpCabUpdateAppointment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAppointment())
    {
      try
      {
        UpdateAppointment();
        ExitState = "ACO_NI0000_UPDATE_SUCCESSFUL";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_APPOINTMENT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_APPOINTMENT_PV";

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
      ExitState = "SP0000_APPOINTMENT_NF";
    }
  }

  private bool ReadAppointment()
  {
    entities.Appointment.Populated = false;

    return Read("ReadAppointment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.Appointment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Appointment.Result = db.GetString(reader, 0);
        entities.Appointment.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Appointment.AppTstamp = db.GetNullableDateTime(reader, 2);
        entities.Appointment.Populated = true;
      });
  }

  private void UpdateAppointment()
  {
    var result = import.Appointment.Result;

    entities.Appointment.Populated = false;
    Update("UpdateAppointment",
      (db, command) =>
      {
        db.SetString(command, "result", result);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Appointment.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Appointment.Result = result;
    entities.Appointment.Populated = true;
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
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    private Appointment appointment;
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
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    private Appointment appointment;
  }
#endregion
}

// Program: SP_CAB_DELETE_APPOINTMENT, ID: 371749365, model: 746.
// Short name: SWE01775
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_DELETE_APPOINTMENT.
/// </summary>
[Serializable]
public partial class SpCabDeleteAppointment: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_DELETE_APPOINTMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabDeleteAppointment(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabDeleteAppointment.
  /// </summary>
  public SpCabDeleteAppointment(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadAppointment1())
    {
      if (ReadInfrastructure())
      {
        ExitState = "CO0000_CANT_DEL_APPT_REL_HIST";

        return;
      }
      else if (ReadAppointment2())
      {
        ExitState = "CO0000_CANT_DEL_APPT_REL_2_APPT";

        return;
      }

      DeleteAppointment();
      ExitState = "ACO_NI0000_DELETE_SUCCESSFUL";
    }
  }

  private void DeleteAppointment()
  {
    Update("DeleteAppointment#1",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "appTstamp",
          entities.Appointment.CreatedTimestamp.GetValueOrDefault());
      });

    Update("DeleteAppointment#2",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "appTstamp",
          entities.Appointment.CreatedTimestamp.GetValueOrDefault());
      });
  }

  private bool ReadAppointment1()
  {
    entities.Appointment.Populated = false;

    return Read("ReadAppointment1",
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
        entities.Appointment.InfId = db.GetNullableInt32(reader, 2);
        entities.Appointment.AppTstamp = db.GetNullableDateTime(reader, 3);
        entities.Appointment.Populated = true;
      });
  }

  private bool ReadAppointment2()
  {
    System.Diagnostics.Debug.Assert(entities.Appointment.Populated);
    entities.Related.Populated = false;

    return Read("ReadAppointment2",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "appTstamp",
          entities.Appointment.CreatedTimestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Appointment.AppTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Related.Result = db.GetString(reader, 0);
        entities.Related.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Related.AppTstamp = db.GetNullableDateTime(reader, 2);
        entities.Related.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.Appointment.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Appointment.InfId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Populated = true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Appointment.
    /// </summary>
    [JsonPropertyName("appointment")]
    public Appointment Appointment
    {
      get => appointment ??= new();
      set => appointment = value;
    }

    /// <summary>
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public Appointment Related
    {
      get => related ??= new();
      set => related = value;
    }

    private Infrastructure infrastructure;
    private Appointment appointment;
    private Appointment related;
  }
#endregion
}

// Program: SP_LINK_PRED_SUCC_APPOINTMENTS, ID: 371749357, model: 746.
// Short name: SWE01711
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_LINK_PRED_SUCC_APPOINTMENTS.
/// </para>
/// <para>
///   This action block will link two appointments as a successor/predecessor.  
/// The earliest created appointment will be the predecessor, the latest the
/// successor.  Validation prior to entry must be done to ensure that the
/// appointments are: 1) for the same case and person, and 2) the predecessor
/// result code is an R for Needs Rescheduled, and the successor type is R for
/// Rescheduled.
/// RVW 11/25/96
/// </para>
/// </summary>
[Serializable]
public partial class SpLinkPredSuccAppointments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_LINK_PRED_SUCC_APPOINTMENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpLinkPredSuccAppointments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpLinkPredSuccAppointments.
  /// </summary>
  public SpLinkPredSuccAppointments(IContext context, Import import,
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
    if (ReadAppointment1())
    {
      if (ReadAppointment2())
      {
        AssociateAppointment1();
        AssociateAppointment2();
        ExitState = "SP0000_PRED_LINKED_SUCCESSFUL";
      }
      else
      {
        ExitState = "ACO_NE0000_DATABASE_CORRUPTION";
      }
    }
    else
    {
      ExitState = "ACO_NE0000_DATABASE_CORRUPTION";
    }
  }

  private void AssociateAppointment1()
  {
    var appTstamp = entities.Succ.CreatedTimestamp;

    entities.Pred.Populated = false;
    Update("AssociateAppointment1",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "appTstamp", appTstamp);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Pred.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Pred.AppTstamp = appTstamp;
    entities.Pred.Populated = true;
  }

  private void AssociateAppointment2()
  {
    var appTstamp = entities.Succ.CreatedTimestamp;

    entities.Pred.Populated = false;
    Update("AssociateAppointment2",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "appTstamp", appTstamp);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Pred.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Pred.AppTstamp = appTstamp;
    entities.Pred.Populated = true;
  }

  private bool ReadAppointment1()
  {
    entities.Pred.Populated = false;

    return Read("ReadAppointment1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.PredecessorAppointment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Pred.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Pred.AppTstamp = db.GetNullableDateTime(reader, 1);
        entities.Pred.Populated = true;
      });
  }

  private bool ReadAppointment2()
  {
    entities.Succ.Populated = false;

    return Read("ReadAppointment2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.SuccessorAppointment.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Succ.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Succ.AppTstamp = db.GetNullableDateTime(reader, 1);
        entities.Succ.Populated = true;
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
    /// A value of PredecessorAppointment.
    /// </summary>
    [JsonPropertyName("predecessorAppointment")]
    public Appointment PredecessorAppointment
    {
      get => predecessorAppointment ??= new();
      set => predecessorAppointment = value;
    }

    /// <summary>
    /// A value of SuccessorAppointment.
    /// </summary>
    [JsonPropertyName("successorAppointment")]
    public Appointment SuccessorAppointment
    {
      get => successorAppointment ??= new();
      set => successorAppointment = value;
    }

    private Appointment predecessorAppointment;
    private Appointment successorAppointment;
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
    /// A value of Pred.
    /// </summary>
    [JsonPropertyName("pred")]
    public Appointment Pred
    {
      get => pred ??= new();
      set => pred = value;
    }

    /// <summary>
    /// A value of Succ.
    /// </summary>
    [JsonPropertyName("succ")]
    public Appointment Succ
    {
      get => succ ??= new();
      set => succ = value;
    }

    private Appointment pred;
    private Appointment succ;
  }
#endregion
}

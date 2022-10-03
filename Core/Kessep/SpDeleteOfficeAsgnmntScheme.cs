// Program: SP_DELETE_OFFICE_ASGNMNT_SCHEME, ID: 372331394, model: 746.
// Short name: SWE01331
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_DELETE_OFFICE_ASGNMNT_SCHEME.
/// </summary>
[Serializable]
public partial class SpDeleteOfficeAsgnmntScheme: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DELETE_OFFICE_ASGNMNT_SCHEME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDeleteOfficeAsgnmntScheme(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDeleteOfficeAsgnmntScheme.
  /// </summary>
  public SpDeleteOfficeAsgnmntScheme(IContext context, Import import,
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
    if (ReadOfficeAssignmentPlan())
    {
      // Disallow deletion of an ongoing assignment
      if (!Lt(Now().Date, entities.OfficeAssignmentPlan.EffectiveDate) && !
        Lt(entities.OfficeAssignmentPlan.DiscontinueDate, Now().Date))
      {
        ExitState = "CANNOT_DELETE_EFFECTIVE_RECORD";

        return;
      }

      DeleteOfficeAssignmentPlan();
    }
    else
    {
      ExitState = "OFFICE_ASSIGNMENT_TABLE_NF";
    }
  }

  private void DeleteOfficeAssignmentPlan()
  {
    Update("DeleteOfficeAssignmentPlan",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId",
          entities.OfficeAssignmentPlan.OffGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          entities.OfficeAssignmentPlan.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "assignmentType",
          entities.OfficeAssignmentPlan.AssignmentType);
      });
  }

  private bool ReadOfficeAssignmentPlan()
  {
    entities.OfficeAssignmentPlan.Populated = false;

    return Read("ReadOfficeAssignmentPlan",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.OfficeAssignmentPlan.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "assignmentType",
          import.OfficeAssignmentPlan.AssignmentType);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeAssignmentPlan.OffGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeAssignmentPlan.EffectiveDate = db.GetDate(reader, 1);
        entities.OfficeAssignmentPlan.AssignmentType = db.GetString(reader, 2);
        entities.OfficeAssignmentPlan.CountyAssignmentInd =
          db.GetString(reader, 3);
        entities.OfficeAssignmentPlan.AlphaAssignmentInd =
          db.GetString(reader, 4);
        entities.OfficeAssignmentPlan.FunctionAssignmentInd =
          db.GetString(reader, 5);
        entities.OfficeAssignmentPlan.ProgramAssignmentInd =
          db.GetString(reader, 6);
        entities.OfficeAssignmentPlan.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.OfficeAssignmentPlan.CreatedTstamp = db.GetDateTime(reader, 8);
        entities.OfficeAssignmentPlan.CreatedBy = db.GetString(reader, 9);
        entities.OfficeAssignmentPlan.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 10);
        entities.OfficeAssignmentPlan.LastUpdatedBy =
          db.GetNullableString(reader, 11);
        entities.OfficeAssignmentPlan.Populated = true;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
    }

    private Office office;
    private OfficeAssignmentPlan officeAssignmentPlan;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
    }

    private Office office;
    private OfficeAssignmentPlan officeAssignmentPlan;
  }
#endregion
}

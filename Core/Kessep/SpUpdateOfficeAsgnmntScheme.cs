// Program: SP_UPDATE_OFFICE_ASGNMNT_SCHEME, ID: 372331393, model: 746.
// Short name: SWE01443
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE_ASGNMNT_SCHEME.
/// </summary>
[Serializable]
public partial class SpUpdateOfficeAsgnmntScheme: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE_ASGNMNT_SCHEME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOfficeAsgnmntScheme(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOfficeAsgnmntScheme.
  /// </summary>
  public SpUpdateOfficeAsgnmntScheme(IContext context, Import import,
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
    if (ReadOfficeAssignmentPlan1())
    {
      // Disallow updates to assignments already ended
      if (Lt(entities.OfficeAssignmentPlan.DiscontinueDate, Now().Date))
      {
        ExitState = "CANNOT_CHANGE_A_DISCONTINUED_REC";

        return;
      }

      // For an ongoing assignment, only end date can be updated
      if (!Lt(entities.OfficeAssignmentPlan.DiscontinueDate, Now().Date) && !
        Lt(Now().Date, entities.OfficeAssignmentPlan.EffectiveDate))
      {
        if (AsChar(import.OfficeAssignmentPlan.AlphaAssignmentInd) != AsChar
          (entities.OfficeAssignmentPlan.AlphaAssignmentInd) || AsChar
          (import.OfficeAssignmentPlan.CountyAssignmentInd) != AsChar
          (entities.OfficeAssignmentPlan.CountyAssignmentInd) || AsChar
          (import.OfficeAssignmentPlan.FunctionAssignmentInd) != AsChar
          (entities.OfficeAssignmentPlan.FunctionAssignmentInd) || AsChar
          (import.OfficeAssignmentPlan.ProgramAssignmentInd) != AsChar
          (entities.OfficeAssignmentPlan.ProgramAssignmentInd) || !
          Equal(import.OfficeAssignmentPlan.EffectiveDate,
          entities.OfficeAssignmentPlan.EffectiveDate))
        {
          ExitState = "SP0000_END_DATE_ONLY_UPDATABLE";

          return;
        }
      }

      if (ReadOfficeAssignmentPlan2())
      {
        ExitState = "SP0000_OFFC_ASSIGNMENT_NC";

        return;
      }

      try
      {
        UpdateOfficeAssignmentPlan();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OFFICE_ASSIGNMENT_TABLE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OFFICE_ASSIGNMENT_TABLE_PV";

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
      ExitState = "OFFICE_ASSIGNMENT_TABLE_NF";
    }
  }

  private bool ReadOfficeAssignmentPlan1()
  {
    entities.OfficeAssignmentPlan.Populated = false;

    return Read("ReadOfficeAssignmentPlan1",
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
        entities.OfficeAssignmentPlan.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeAssignmentPlan.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.OfficeAssignmentPlan.Populated = true;
      });
  }

  private bool ReadOfficeAssignmentPlan2()
  {
    entities.Ro.Populated = false;

    return Read("ReadOfficeAssignmentPlan2",
      (db, command) =>
      {
        db.SetString(
          command, "assignmentType",
          import.OfficeAssignmentPlan.AssignmentType);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate1",
          import.OfficeAssignmentPlan.DiscontinueDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          entities.OfficeAssignmentPlan.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ro.OffGeneratedId = db.GetInt32(reader, 0);
        entities.Ro.EffectiveDate = db.GetDate(reader, 1);
        entities.Ro.AssignmentType = db.GetString(reader, 2);
        entities.Ro.CountyAssignmentInd = db.GetString(reader, 3);
        entities.Ro.AlphaAssignmentInd = db.GetString(reader, 4);
        entities.Ro.FunctionAssignmentInd = db.GetString(reader, 5);
        entities.Ro.ProgramAssignmentInd = db.GetString(reader, 6);
        entities.Ro.DiscontinueDate = db.GetNullableDate(reader, 7);
        entities.Ro.LastUpdatdTstamp = db.GetNullableDateTime(reader, 8);
        entities.Ro.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Ro.Populated = true;
      });
  }

  private void UpdateOfficeAssignmentPlan()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeAssignmentPlan.Populated);

    var discontinueDate = import.OfficeAssignmentPlan.DiscontinueDate;
    var lastUpdatdTstamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.OfficeAssignmentPlan.Populated = false;
    Update("UpdateOfficeAssignmentPlan",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
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

    entities.OfficeAssignmentPlan.DiscontinueDate = discontinueDate;
    entities.OfficeAssignmentPlan.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.OfficeAssignmentPlan.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeAssignmentPlan.Populated = true;
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
    /// A value of Ro.
    /// </summary>
    [JsonPropertyName("ro")]
    public OfficeAssignmentPlan Ro
    {
      get => ro ??= new();
      set => ro = value;
    }

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

    private OfficeAssignmentPlan ro;
    private Office office;
    private OfficeAssignmentPlan officeAssignmentPlan;
  }
#endregion
}

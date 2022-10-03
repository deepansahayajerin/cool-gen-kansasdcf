// Program: SP_CREATE_OFFICE_ASGNMNT_SCHEME, ID: 372331395, model: 746.
// Short name: SWE01313
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE_ASGNMNT_SCHEME.
/// </summary>
[Serializable]
public partial class SpCreateOfficeAsgnmntScheme: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE_ASGNMNT_SCHEME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOfficeAsgnmntScheme(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOfficeAsgnmntScheme.
  /// </summary>
  public SpCreateOfficeAsgnmntScheme(IContext context, Import import,
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
    // -------------------------------------------------------------------------------
    //  Maintenance Log:
    //  CQ60203 R Mathews Allow referral assignment by program and/or alpha
    //  
    // -------------------------------------------------------------------------------
    export.OfficeAssignmentPlan.Assign(import.OfficeAssignmentPlan);

    if (ReadOffice())
    {
      // Check if an assignment is already active
      if (ReadOfficeAssignmentPlan())
      {
        ExitState = "SP0000_OFFICE_ASSIGNMENT_ALR_EFF";

        return;
      }
      else
      {
        if (Equal(import.OfficeAssignmentPlan.AssignmentType, "CA"))
        {
          try
          {
            CreateOfficeAssignmentPlan1();
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OFFICE_ASSIGNMENT_TABLE_AE";

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

        if (Equal(import.OfficeAssignmentPlan.AssignmentType, "RE"))
        {
          // CQ60203 Changed hard coded move of 'Y'/'N' to program and alpha 
          // assignment indicators
          try
          {
            CreateOfficeAssignmentPlan2();
            ExitState = "ACO_NI0000_ADD_SUCCESSFUL";
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "OFFICE_ASSIGNMENT_TABLE_AE";

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
      }
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
      ("ACO_NI0000_ADD_SUCCESSFUL"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      export.OfficeAssignmentPlan.Assign(entities.OfficeAssignmentPlan);
    }
  }

  private void CreateOfficeAssignmentPlan1()
  {
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var effectiveDate = import.OfficeAssignmentPlan.EffectiveDate;
    var assignmentType = import.OfficeAssignmentPlan.AssignmentType;
    var countyAssignmentInd = "N";
    var alphaAssignmentInd = import.OfficeAssignmentPlan.AlphaAssignmentInd;
    var functionAssignmentInd =
      import.OfficeAssignmentPlan.FunctionAssignmentInd;
    var programAssignmentInd = import.OfficeAssignmentPlan.ProgramAssignmentInd;
    var discontinueDate = import.OfficeAssignmentPlan.DiscontinueDate;
    var createdTstamp = Now();
    var createdBy = global.UserId;
    var tribunalInd = import.OfficeAssignmentPlan.TribunalInd;

    entities.OfficeAssignmentPlan.Populated = false;

    // Duplicate field CNTY_ASSGNMNT_IND in the insert.
    Update("CreateOfficeAssignmentPlan1",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "assignmentType", assignmentType);
        db.SetString(command, "cntyAssgnmntInd", countyAssignmentInd);
        db.SetString(command, "alphaAssgnmntInd", alphaAssignmentInd);
        db.SetString(command, "fnctnAssgnmntInd", functionAssignmentInd);
        db.SetString(command, "prgrmAssgnmntInd", programAssignmentInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetString(command, "tribunalInd", tribunalInd);
      });

    entities.OfficeAssignmentPlan.OffGeneratedId = offGeneratedId;
    entities.OfficeAssignmentPlan.EffectiveDate = effectiveDate;
    entities.OfficeAssignmentPlan.AssignmentType = assignmentType;
    entities.OfficeAssignmentPlan.CountyAssignmentInd = countyAssignmentInd;
    entities.OfficeAssignmentPlan.AlphaAssignmentInd = alphaAssignmentInd;
    entities.OfficeAssignmentPlan.FunctionAssignmentInd = functionAssignmentInd;
    entities.OfficeAssignmentPlan.ProgramAssignmentInd = programAssignmentInd;
    entities.OfficeAssignmentPlan.DiscontinueDate = discontinueDate;
    entities.OfficeAssignmentPlan.CreatedTstamp = createdTstamp;
    entities.OfficeAssignmentPlan.CreatedBy = createdBy;
    entities.OfficeAssignmentPlan.LastUpdatdTstamp = null;
    entities.OfficeAssignmentPlan.LastUpdatedBy = "";
    entities.OfficeAssignmentPlan.TribunalInd = tribunalInd;
    entities.OfficeAssignmentPlan.Populated = true;
  }

  private void CreateOfficeAssignmentPlan2()
  {
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var effectiveDate = import.OfficeAssignmentPlan.EffectiveDate;
    var assignmentType = import.OfficeAssignmentPlan.AssignmentType;
    var countyAssignmentInd = "N";
    var alphaAssignmentInd = import.OfficeAssignmentPlan.AlphaAssignmentInd;
    var programAssignmentInd = import.OfficeAssignmentPlan.ProgramAssignmentInd;
    var discontinueDate = import.OfficeAssignmentPlan.DiscontinueDate;
    var createdTstamp = Now();
    var createdBy = global.UserId;

    entities.OfficeAssignmentPlan.Populated = false;
    Update("CreateOfficeAssignmentPlan2",
      (db, command) =>
      {
        db.SetInt32(command, "offGeneratedId", offGeneratedId);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "assignmentType", assignmentType);
        db.SetString(command, "cntyAssgnmntInd", countyAssignmentInd);
        db.SetString(command, "alphaAssgnmntInd", alphaAssignmentInd);
        db.SetString(command, "fnctnAssgnmntInd", countyAssignmentInd);
        db.SetString(command, "prgrmAssgnmntInd", programAssignmentInd);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetString(command, "tribunalInd", countyAssignmentInd);
      });

    entities.OfficeAssignmentPlan.OffGeneratedId = offGeneratedId;
    entities.OfficeAssignmentPlan.EffectiveDate = effectiveDate;
    entities.OfficeAssignmentPlan.AssignmentType = assignmentType;
    entities.OfficeAssignmentPlan.CountyAssignmentInd = countyAssignmentInd;
    entities.OfficeAssignmentPlan.AlphaAssignmentInd = alphaAssignmentInd;
    entities.OfficeAssignmentPlan.FunctionAssignmentInd = countyAssignmentInd;
    entities.OfficeAssignmentPlan.ProgramAssignmentInd = programAssignmentInd;
    entities.OfficeAssignmentPlan.DiscontinueDate = discontinueDate;
    entities.OfficeAssignmentPlan.CreatedTstamp = createdTstamp;
    entities.OfficeAssignmentPlan.CreatedBy = createdBy;
    entities.OfficeAssignmentPlan.LastUpdatdTstamp = null;
    entities.OfficeAssignmentPlan.LastUpdatedBy = "";
    entities.OfficeAssignmentPlan.TribunalInd = countyAssignmentInd;
    entities.OfficeAssignmentPlan.Populated = true;
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeAssignmentPlan()
  {
    entities.OfficeAssignmentPlan.Populated = false;

    return Read("ReadOfficeAssignmentPlan",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetNullableDate(
          command, "discontinueDate",
          import.OfficeAssignmentPlan.EffectiveDate.GetValueOrDefault());
        db.SetString(
          command, "assignmentType",
          import.OfficeAssignmentPlan.AssignmentType);
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
        entities.OfficeAssignmentPlan.TribunalInd = db.GetString(reader, 12);
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
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
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

    private OfficeAssignmentPlan officeAssignmentPlan;
    private Office office;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
    }

    private OfficeAssignmentPlan officeAssignmentPlan;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OfficeAssignmentPlan.
    /// </summary>
    [JsonPropertyName("officeAssignmentPlan")]
    public OfficeAssignmentPlan OfficeAssignmentPlan
    {
      get => officeAssignmentPlan ??= new();
      set => officeAssignmentPlan = value;
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

    private OfficeAssignmentPlan officeAssignmentPlan;
    private Office office;
  }
#endregion
}

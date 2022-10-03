// Program: SP_CREATE_OFFICE_CASELOAD_ASSIGN, ID: 372559322, model: 746.
// Short name: SWE01314
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_OFFICE_CASELOAD_ASSIGN.
/// </summary>
[Serializable]
public partial class SpCreateOfficeCaseloadAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_OFFICE_CASELOAD_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateOfficeCaseloadAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateOfficeCaseloadAssign.
  /// </summary>
  public SpCreateOfficeCaseloadAssign(IContext context, Import import,
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
    local.ControlTable.Identifier = "OFFICE_CASELOAD_ASSIGNMENT";

    if (ReadOffice())
    {
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    }
    else
    {
      ExitState = "OFFICE_NF";

      return;
    }

    if (import.Tribunal.Identifier > 0)
    {
      if (!ReadTribunal())
      {
        ExitState = "TRIBUNAL_NF";

        return;
      }
    }

    if (import.Program.SystemGeneratedIdentifier > 0)
    {
      if (!ReadProgram())
      {
        ExitState = "PROGRAM_NF";

        return;
      }
    }

    if (!IsEmpty(import.OfficeServiceProvider.RoleCode))
    {
      if (!ReadOfficeServiceProvider())
      {
        ExitState = "OFFICE_SERVICE_PROVIDER_NF";

        return;
      }
    }

    try
    {
      CreateOfficeCaseloadAssignment();
      export.OfficeCaseloadAssignment.Assign(entities.OfficeCaseloadAssignment);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (entities.Tribunal.Populated)
    {
      AssociateOfficeCaseloadAssignment2();
    }

    if (entities.Program.Populated)
    {
      AssociateOfficeCaseloadAssignment3();
    }

    if (entities.OfficeServiceProvider.Populated)
    {
      AssociateOfficeCaseloadAssignment1();
    }

    ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
  }

  private int UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    return useExport.ControlTable.LastUsedNumber;
  }

  private void AssociateOfficeCaseloadAssignment1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offDGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateOfficeCaseloadAssignment1",
      (db, command) =>
      {
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offDGeneratedId", offDGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.OspEffectiveDate = ospEffectiveDate;
    entities.OfficeCaseloadAssignment.OspRoleCode = ospRoleCode;
    entities.OfficeCaseloadAssignment.OffDGeneratedId = offDGeneratedId;
    entities.OfficeCaseloadAssignment.SpdGeneratedId = spdGeneratedId;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void AssociateOfficeCaseloadAssignment2()
  {
    var trbId = entities.Tribunal.Identifier;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateOfficeCaseloadAssignment2",
      (db, command) =>
      {
        db.SetNullableInt32(command, "trbId", trbId);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.TrbId = trbId;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void AssociateOfficeCaseloadAssignment3()
  {
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateOfficeCaseloadAssignment3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.PrgGeneratedId = prgGeneratedId;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void CreateOfficeCaseloadAssignment()
  {
    var systemGeneratedIdentifier = UseAccessControlTable();
    var endingAlpha = import.OfficeCaseloadAssignment.EndingAlpha;
    var beginingAlpha = import.OfficeCaseloadAssignment.BeginingAlpha;
    var effectiveDate = import.OfficeCaseloadAssignment.EffectiveDate;
    var priority = import.OfficeCaseloadAssignment.Priority;
    var discontinueDate = import.OfficeCaseloadAssignment.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var assignmentIndicator =
      import.OfficeCaseloadAssignment.AssignmentIndicator;
    var function = import.OfficeCaseloadAssignment.Function ?? "";
    var assignmentType = import.OfficeCaseloadAssignment.AssignmentType;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("CreateOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "ofceCsldAssgnId", systemGeneratedIdentifier);
        db.SetString(command, "endingAlpha", endingAlpha);
        db.SetString(command, "beginingAlpha", beginingAlpha);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "priority", priority);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "endFirstInitial", "");
        db.SetString(command, "assignmentInd", assignmentIndicator);
        db.SetNullableString(command, "function", function);
        db.SetString(command, "assignmentType", assignmentType);
      });

    entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.OfficeCaseloadAssignment.EndingAlpha = endingAlpha;
    entities.OfficeCaseloadAssignment.BeginingAlpha = beginingAlpha;
    entities.OfficeCaseloadAssignment.EffectiveDate = effectiveDate;
    entities.OfficeCaseloadAssignment.Priority = priority;
    entities.OfficeCaseloadAssignment.DiscontinueDate = discontinueDate;
    entities.OfficeCaseloadAssignment.LastUpdatedBy = "";
    entities.OfficeCaseloadAssignment.LastUpdatedTstamp = null;
    entities.OfficeCaseloadAssignment.CreatedBy = createdBy;
    entities.OfficeCaseloadAssignment.CreatedTimestamp = createdTimestamp;
    entities.OfficeCaseloadAssignment.OffGeneratedId = offGeneratedId;
    entities.OfficeCaseloadAssignment.OspEffectiveDate = null;
    entities.OfficeCaseloadAssignment.OspRoleCode = null;
    entities.OfficeCaseloadAssignment.AssignmentIndicator = assignmentIndicator;
    entities.OfficeCaseloadAssignment.Function = function;
    entities.OfficeCaseloadAssignment.AssignmentType = assignmentType;
    entities.OfficeCaseloadAssignment.PrgGeneratedId = null;
    entities.OfficeCaseloadAssignment.OffDGeneratedId = null;
    entities.OfficeCaseloadAssignment.SpdGeneratedId = null;
    entities.OfficeCaseloadAssignment.TrbId = null;
    entities.OfficeCaseloadAssignment.Populated = true;
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

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.
          SetInt32(command, "offGeneratedId", entities.Office.SystemGeneratedId);
          
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.Tribunal.Identifier = db.GetInt32(reader, 0);
        entities.Tribunal.Populated = true;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private OfficeServiceProvider officeServiceProvider;
    private Program program;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private ServiceProvider serviceProvider;
    private Office office;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    private ControlTable controlTable;
    private CodeValue codeValue;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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

    private Tribunal tribunal;
    private Program program;
    private OfficeAssignmentPlan officeAssignmentPlan;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}

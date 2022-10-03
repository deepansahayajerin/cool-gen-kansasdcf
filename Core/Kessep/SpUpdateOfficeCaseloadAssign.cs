// Program: SP_UPDATE_OFFICE_CASELOAD_ASSIGN, ID: 372559321, model: 746.
// Short name: SWE01444
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_UPDATE_OFFICE_CASELOAD_ASSIGN.
/// </summary>
[Serializable]
public partial class SpUpdateOfficeCaseloadAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_UPDATE_OFFICE_CASELOAD_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpUpdateOfficeCaseloadAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpUpdateOfficeCaseloadAssign.
  /// </summary>
  public SpUpdateOfficeCaseloadAssign(IContext context, Import import,
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
    // ************************************************************
    // 03/23/98	Siraj Konkader			ZDEL Cleanup
    // ************************************************************
    MoveProgram(import.Program, export.Program);
    export.ServiceProvider.SystemGeneratedId =
      import.ServiceProvider.SystemGeneratedId;
    export.Tribunal.Identifier = import.Tribunal.Identifier;

    if (!ReadOfficeCaseloadAssignmentOfficeServiceProvider())
    {
      ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NF";

      return;
    }

    try
    {
      UpdateOfficeCaseloadAssignment();
      export.OfficeCaseloadAssignment.Assign(entities.OfficeCaseloadAssignment);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OFFICE_CASELOAD_ASSIGNMENT_T_NU";

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

    // Perform Disassociates and Associates.
    if (import.Tribunal.Identifier != import.OldTribunal.Identifier)
    {
      if (import.OldTribunal.Identifier > 0)
      {
        if (ReadTribunal2())
        {
          DisassociateTribunal();
        }
        else
        {
          ExitState = "TRIBUNAL_NF";

          return;
        }
      }

      if (import.Tribunal.Identifier > 0)
      {
        if (ReadTribunal1())
        {
          export.Tribunal.Identifier = entities.Tribunal.Identifier;
          AssociateTribunal();
        }
        else
        {
          ExitState = "TRIBUNAL_NF";

          return;
        }
      }
    }

    if (!Equal(import.Program.Code, import.OldProgram.Code))
    {
      if (import.OldProgram.SystemGeneratedIdentifier > 0)
      {
        if (ReadProgram2())
        {
          DisassociateProgram();
        }
        else
        {
          ExitState = "PROGRAM_NF";

          return;
        }
      }

      if (import.Program.SystemGeneratedIdentifier > 0)
      {
        if (ReadProgram1())
        {
          MoveProgram(entities.Program, export.Program);
          AssociateProgram();
        }
        else
        {
          ExitState = "PROGRAM_NF";

          return;
        }
      }
    }

    if (import.ServiceProvider.SystemGeneratedId != import
      .OldServiceProvider.SystemGeneratedId || !
      Equal(entities.OldOfficeServiceProvider.RoleCode,
      import.OfficeServiceProvider.RoleCode) || !
      Equal(entities.OldOfficeServiceProvider.EffectiveDate,
      import.OfficeServiceProvider.EffectiveDate))
    {
      DisassociateOfficeServiceProvider();

      if (import.ServiceProvider.SystemGeneratedId > 0)
      {
        if (ReadOfficeServiceProvider())
        {
          export.ServiceProvider.Assign(entities.ServiceProvider);
          AssociateOfficeServiceProvider();
        }
        else
        {
          ExitState = "OFFICE_SERVICE_PROVIDER_NF";

          return;
        }
      }
    }

    ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void AssociateOfficeServiceProvider()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offDGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateOfficeServiceProvider",
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

  private void AssociateProgram()
  {
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateProgram",
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

  private void AssociateTribunal()
  {
    var trbId = entities.Tribunal.Identifier;

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("AssociateTribunal",
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

  private void DisassociateOfficeServiceProvider()
  {
    entities.OfficeCaseloadAssignment.Populated = false;
    Update("DisassociateOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.OspEffectiveDate = null;
    entities.OfficeCaseloadAssignment.OspRoleCode = null;
    entities.OfficeCaseloadAssignment.OffDGeneratedId = null;
    entities.OfficeCaseloadAssignment.SpdGeneratedId = null;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void DisassociateProgram()
  {
    entities.OfficeCaseloadAssignment.Populated = false;
    Update("DisassociateProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.PrgGeneratedId = null;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private void DisassociateTribunal()
  {
    entities.OfficeCaseloadAssignment.Populated = false;
    Update("DisassociateTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.TrbId = null;
    entities.OfficeCaseloadAssignment.Populated = true;
  }

  private bool ReadOfficeCaseloadAssignmentOfficeServiceProvider()
  {
    entities.OldOfficeServiceProvider.Populated = false;
    entities.OfficeCaseloadAssignment.Populated = false;

    return Read("ReadOfficeCaseloadAssignmentOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "ofceCsldAssgnId",
          import.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "offGeneratedId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeCaseloadAssignment.EndingAlpha = db.GetString(reader, 1);
        entities.OfficeCaseloadAssignment.BeginingAlpha =
          db.GetString(reader, 2);
        entities.OfficeCaseloadAssignment.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeCaseloadAssignment.Priority = db.GetInt32(reader, 4);
        entities.OfficeCaseloadAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.OfficeCaseloadAssignment.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.OfficeCaseloadAssignment.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.OfficeCaseloadAssignment.CreatedBy = db.GetString(reader, 8);
        entities.OfficeCaseloadAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.OfficeCaseloadAssignment.OffGeneratedId =
          db.GetNullableInt32(reader, 10);
        entities.OfficeCaseloadAssignment.OspEffectiveDate =
          db.GetNullableDate(reader, 11);
        entities.OldOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 11);
        entities.OfficeCaseloadAssignment.OspRoleCode =
          db.GetNullableString(reader, 12);
        entities.OldOfficeServiceProvider.RoleCode = db.GetString(reader, 12);
        entities.OfficeCaseloadAssignment.AssignmentIndicator =
          db.GetString(reader, 13);
        entities.OfficeCaseloadAssignment.Function =
          db.GetNullableString(reader, 14);
        entities.OfficeCaseloadAssignment.AssignmentType =
          db.GetString(reader, 15);
        entities.OfficeCaseloadAssignment.PrgGeneratedId =
          db.GetNullableInt32(reader, 16);
        entities.OfficeCaseloadAssignment.OffDGeneratedId =
          db.GetNullableInt32(reader, 17);
        entities.OldOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 17);
        entities.OfficeCaseloadAssignment.SpdGeneratedId =
          db.GetNullableInt32(reader, 18);
        entities.OldOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 18);
        entities.OfficeCaseloadAssignment.TrbId =
          db.GetNullableInt32(reader, 19);
        entities.OldOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 20);
        entities.OldOfficeServiceProvider.Populated = true;
        entities.OfficeCaseloadAssignment.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
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

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId", import.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.Populated = true;
      });
  }

  private bool ReadProgram2()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.OldProgram.Populated = false;

    return Read("ReadProgram2",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          entities.OfficeCaseloadAssignment.PrgGeneratedId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.OldProgram.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.OldProgram.Code = db.GetString(reader, 1);
        entities.OldProgram.Populated = true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.Tribunal.Populated = false;

    return Read("ReadTribunal1",
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

  private bool ReadTribunal2()
  {
    System.Diagnostics.Debug.
      Assert(entities.OfficeCaseloadAssignment.Populated);
    entities.OldTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.OfficeCaseloadAssignment.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OldTribunal.Identifier = db.GetInt32(reader, 0);
        entities.OldTribunal.Populated = true;
      });
  }

  private void UpdateOfficeCaseloadAssignment()
  {
    var endingAlpha = import.OfficeCaseloadAssignment.EndingAlpha;
    var beginingAlpha = import.OfficeCaseloadAssignment.BeginingAlpha;
    var effectiveDate = import.OfficeCaseloadAssignment.EffectiveDate;
    var priority = import.OfficeCaseloadAssignment.Priority;
    var discontinueDate = import.OfficeCaseloadAssignment.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var assignmentIndicator =
      import.OfficeCaseloadAssignment.AssignmentIndicator;
    var function = import.OfficeCaseloadAssignment.Function ?? "";

    entities.OfficeCaseloadAssignment.Populated = false;
    Update("UpdateOfficeCaseloadAssignment",
      (db, command) =>
      {
        db.SetString(command, "endingAlpha", endingAlpha);
        db.SetString(command, "beginingAlpha", beginingAlpha);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "priority", priority);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatedTstamp);
        db.SetString(command, "assignmentInd", assignmentIndicator);
        db.SetNullableString(command, "function", function);
        db.SetInt32(
          command, "ofceCsldAssgnId",
          entities.OfficeCaseloadAssignment.SystemGeneratedIdentifier);
      });

    entities.OfficeCaseloadAssignment.EndingAlpha = endingAlpha;
    entities.OfficeCaseloadAssignment.BeginingAlpha = beginingAlpha;
    entities.OfficeCaseloadAssignment.EffectiveDate = effectiveDate;
    entities.OfficeCaseloadAssignment.Priority = priority;
    entities.OfficeCaseloadAssignment.DiscontinueDate = discontinueDate;
    entities.OfficeCaseloadAssignment.LastUpdatedBy = lastUpdatedBy;
    entities.OfficeCaseloadAssignment.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.OfficeCaseloadAssignment.AssignmentIndicator = assignmentIndicator;
    entities.OfficeCaseloadAssignment.Function = function;
    entities.OfficeCaseloadAssignment.Populated = true;
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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
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
    /// A value of OldProgram.
    /// </summary>
    [JsonPropertyName("oldProgram")]
    public Program OldProgram
    {
      get => oldProgram ??= new();
      set => oldProgram = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of OldTribunal.
    /// </summary>
    [JsonPropertyName("oldTribunal")]
    public Tribunal OldTribunal
    {
      get => oldTribunal ??= new();
      set => oldTribunal = value;
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
    private ServiceProvider oldServiceProvider;
    private ServiceProvider serviceProvider;
    private Program oldProgram;
    private Program program;
    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Tribunal oldTribunal;
    private Tribunal tribunal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
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

    private Tribunal tribunal;
    private ServiceProvider serviceProvider;
    private Program program;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of OldTribunal.
    /// </summary>
    [JsonPropertyName("oldTribunal")]
    public Tribunal OldTribunal
    {
      get => oldTribunal ??= new();
      set => oldTribunal = value;
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

    /// <summary>
    /// A value of OldOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("oldOfficeServiceProvider")]
    public OfficeServiceProvider OldOfficeServiceProvider
    {
      get => oldOfficeServiceProvider ??= new();
      set => oldOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of OldServiceProvider.
    /// </summary>
    [JsonPropertyName("oldServiceProvider")]
    public ServiceProvider OldServiceProvider
    {
      get => oldServiceProvider ??= new();
      set => oldServiceProvider = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OldProgram.
    /// </summary>
    [JsonPropertyName("oldProgram")]
    public Program OldProgram
    {
      get => oldProgram ??= new();
      set => oldProgram = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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

    private Tribunal oldTribunal;
    private Tribunal tribunal;
    private OfficeServiceProvider oldOfficeServiceProvider;
    private ServiceProvider oldServiceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private Program oldProgram;
    private Program program;
    private Office office;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
  }
#endregion
}

// Program: SP_CREATE_PROGRAM, ID: 371745836, model: 746.
// Short name: SWE01320
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_PROGRAM.
/// </summary>
[Serializable]
public partial class SpCreateProgram: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_PROGRAM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateProgram(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateProgram.
  /// </summary>
  public SpCreateProgram(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ControlTable.Identifier = "PROGRAM";
    MoveProgram(import.Program, export.Program);
    export.ProgramIndicator.Assign(import.ProgramIndicator);

    try
    {
      CreateProgram();
      export.Program.Assign(entities.Program);

      try
      {
        CreateProgramIndicator();
        export.ProgramIndicator.Assign(entities.ProgramIndicator);
        ExitState = "ZD_ACO_NI0000_SUCCESSFUL_ADD_2";
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_INDICATORS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_INDICATORS_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PROGRAM_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveProgram(Program source, Program target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Title = source.Title;
    target.DistributionProgramType = source.DistributionProgramType;
    target.InterstateIndicator = source.InterstateIndicator;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private int UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    return useExport.ControlTable.LastUsedNumber;
  }

  private void CreateProgram()
  {
    var systemGeneratedIdentifier = UseAccessControlTable();
    var code = import.Program.Code;
    var title = import.Program.Title;
    var interstateIndicator = import.Program.InterstateIndicator;
    var effectiveDate = import.Program.EffectiveDate;
    var discontinueDate = import.Program.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var distributionProgramType = import.Program.DistributionProgramType;

    entities.Program.Populated = false;
    Update("CreateProgram",
      (db, command) =>
      {
        db.SetInt32(command, "programId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "title", title);
        db.SetString(command, "interstateInd", interstateIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", default(DateTime));
        db.SetString(command, "distrbtnPrgmType", distributionProgramType);
      });

    entities.Program.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.Program.Code = code;
    entities.Program.Title = title;
    entities.Program.InterstateIndicator = interstateIndicator;
    entities.Program.EffectiveDate = effectiveDate;
    entities.Program.DiscontinueDate = discontinueDate;
    entities.Program.CreatedBy = createdBy;
    entities.Program.CreatedTimestamp = createdTimestamp;
    entities.Program.DistributionProgramType = distributionProgramType;
    entities.Program.Populated = true;
  }

  private void CreateProgramIndicator()
  {
    var childSupportRetentionCode =
      import.ProgramIndicator.ChildSupportRetentionCode;
    var ivDFeeIndicator = import.ProgramIndicator.IvDFeeIndicator;
    var effectiveDate = import.Program.EffectiveDate;
    var discontinueDate = import.Program.DiscontinueDate;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.ProgramIndicator.Populated = false;
    Update("CreateProgramIndicator",
      (db, command) =>
      {
        db.SetString(command, "chdSpprtRtntnCd", childSupportRetentionCode);
        db.SetString(command, "ivDFeeIndicator", ivDFeeIndicator);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.ProgramIndicator.ChildSupportRetentionCode =
      childSupportRetentionCode;
    entities.ProgramIndicator.IvDFeeIndicator = ivDFeeIndicator;
    entities.ProgramIndicator.EffectiveDate = effectiveDate;
    entities.ProgramIndicator.DiscontinueDate = discontinueDate;
    entities.ProgramIndicator.PrgGeneratedId = prgGeneratedId;
    entities.ProgramIndicator.Populated = true;
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
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
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

    private ProgramIndicator programIndicator;
    private Program program;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
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

    private ProgramIndicator programIndicator;
    private Program program;
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

    private ControlTable controlTable;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramIndicator.
    /// </summary>
    [JsonPropertyName("programIndicator")]
    public ProgramIndicator ProgramIndicator
    {
      get => programIndicator ??= new();
      set => programIndicator = value;
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

    private ProgramIndicator programIndicator;
    private Program program;
  }
#endregion
}

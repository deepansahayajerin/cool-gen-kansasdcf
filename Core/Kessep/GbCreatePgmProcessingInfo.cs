// Program: GB_CREATE_PGM_PROCESSING_INFO, ID: 371744243, model: 746.
// Short name: SWE00694
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GB_CREATE_PGM_PROCESSING_INFO.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will create a program_processing_info row.
/// </para>
/// </summary>
[Serializable]
public partial class GbCreatePgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_CREATE_PGM_PROCESSING_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbCreatePgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbCreatePgmProcessingInfo.
  /// </summary>
  public GbCreatePgmProcessingInfo(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";

    try
    {
      CreateProgramProcessingInfo();
      export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "PROGRAM_PROCESSING_INFO_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "PROGRAM_PROCESSING_INFO_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateProgramProcessingInfo()
  {
    var name = import.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var createdBy = global.UserId;
    var parameterList = import.ProgramProcessingInfo.ParameterList ?? "";
    var description = import.ProgramProcessingInfo.Description ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("CreateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", name);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetNullableString(command, "pgmProcInfoDesc", description);
      });

    entities.ProgramProcessingInfo.Name = name;
    entities.ProgramProcessingInfo.CreatedTimestamp = createdTimestamp;
    entities.ProgramProcessingInfo.ProcessDate = processDate;
    entities.ProgramProcessingInfo.CreatedBy = createdBy;
    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Description = description;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

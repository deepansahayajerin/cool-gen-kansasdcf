// Program: UPDATE_PGM_PROCESSING_INFO, ID: 371744241, model: 746.
// Short name: SWE01494
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: UPDATE_PGM_PROCESSING_INFO.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will update a program_processing_info row.
/// </para>
/// </summary>
[Serializable]
public partial class UpdatePgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PGM_PROCESSING_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdatePgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdatePgmProcessingInfo.
  /// </summary>
  public UpdatePgmProcessingInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadProgramProcessingInfo())
    {
      try
      {
        UpdateProgramProcessingInfo();
        export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_NU";

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
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";
    }
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", import.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.CreatedBy = db.GetString(reader, 3);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 4);
        entities.ProgramProcessingInfo.Description =
          db.GetNullableString(reader, 5);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateProgramProcessingInfo()
  {
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var parameterList = import.ProgramProcessingInfo.ParameterList ?? "";
    var description = import.ProgramProcessingInfo.Description ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetNullableString(command, "pgmProcInfoDesc", description);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ProcessDate = processDate;
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

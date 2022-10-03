// Program: UPDATE_PROGRAM_PROCESSING_INFO, ID: 372067572, model: 746.
// Short name: SWE02312
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_PROGRAM_PROCESSING_INFO.
/// </summary>
[Serializable]
public partial class UpdateProgramProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_PROGRAM_PROCESSING_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateProgramProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateProgramProcessingInfo.
  /// </summary>
  public UpdateProgramProcessingInfo(IContext context, Import import,
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
    // *****************************************************************
    // Date         Developer      Request #      Description
    // *****************************************************************
    // 09Jan99  John C Crook  -                      Initial
    // ***  Crook  09Jan99  
    // ********************************************
    // *****************************************************************
    // Is Name blank ? (Checking Keys)
    // ***  Crook  09 Jan 99  
    // ******************************************
    if (Equal(import.ProgramProcessingInfo.Name, local.Blank.Name))
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NU";

      return;
    }

    local.Key.Name = import.ProgramProcessingInfo.Name;
    local.Key.CreatedTimestamp = import.ProgramProcessingInfo.CreatedTimestamp;

    // *****************************************************************
    // If TS blank READ EACH (Checking Keys)
    // ***  Crook  09 Jan 99  
    // ******************************************
    if (Equal(import.ProgramProcessingInfo.CreatedTimestamp,
      local.Blank.CreatedTimestamp))
    {
      if (ReadProgramProcessingInfo2())
      {
        local.Key.CreatedTimestamp =
          entities.ProgramProcessingInfo.CreatedTimestamp;
      }
    }

    // *****************************************************************
    // Read for Update
    // ***  Crook  09 Jan 99  
    // ******************************************
    if (ReadProgramProcessingInfo1())
    {
      try
      {
        UpdateProgramProcessingInfo1();

        // *****************************************************************
        // We re turn the export because it might have been the READ EACH that 
        // was updated
        // ***  Crook  09 Jan 99  
        // ******************************************
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

  private bool ReadProgramProcessingInfo1()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo1",
      (db, command) =>
      {
        db.SetString(command, "name", local.Key.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          local.Key.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo2()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo2",
      (db, command) =>
      {
        db.SetString(command, "name", local.Key.Name);
      },
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 3);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateProgramProcessingInfo1()
  {
    var processDate = import.ProgramProcessingInfo.ProcessDate;
    var parameterList = import.ProgramProcessingInfo.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableDate(command, "processDate", processDate);
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ProcessDate = processDate;
    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Key.
    /// </summary>
    [JsonPropertyName("key")]
    public ProgramProcessingInfo Key
    {
      get => key ??= new();
      set => key = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public ProgramProcessingInfo Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private ProgramProcessingInfo key;
    private ProgramProcessingInfo blank;
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

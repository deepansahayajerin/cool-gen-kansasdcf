// Program: GB_READ_PGM_PROCESSING_INFO, ID: 371744242, model: 746.
// Short name: SWE00696
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GB_READ_PGM_PROCESSING_INFO.
/// </para>
/// <para>
/// RESP:  ALL
/// This action block will read the program_processing_info table.
/// </para>
/// </summary>
[Serializable]
public partial class GbReadPgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_READ_PGM_PROCESSING_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbReadPgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbReadPgmProcessingInfo.
  /// </summary>
  public GbReadPgmProcessingInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (Equal(import.ProgramProcessingInfo.CreatedTimestamp,
      local.Initialized.CreatedTimestamp))
    {
      if (ReadProgramProcessingInfo2())
      {
        export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);

        return;
      }

      ExitState = "PROGRAM_PROCESSING_INFO_NF";
    }
    else if (ReadProgramProcessingInfo1())
    {
      export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
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

  private bool ReadProgramProcessingInfo2()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo2",
      (db, command) =>
      {
        db.SetString(command, "name", import.ProgramProcessingInfo.Name);
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public ProgramProcessingInfo Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private ProgramProcessingInfo initialized;
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

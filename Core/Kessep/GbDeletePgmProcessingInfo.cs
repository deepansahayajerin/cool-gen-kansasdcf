// Program: GB_DELETE_PGM_PROCESSING_INFO, ID: 371744244, model: 746.
// Short name: SWE00695
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: GB_DELETE_PGM_PROCESSING_INFO.
/// </para>
/// <para>
/// RESP: ALL
/// This action block will delete a program_processing_info row.
/// </para>
/// </summary>
[Serializable]
public partial class GbDeletePgmProcessingInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the GB_DELETE_PGM_PROCESSING_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new GbDeletePgmProcessingInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of GbDeletePgmProcessingInfo.
  /// </summary>
  public GbDeletePgmProcessingInfo(IContext context, Import import,
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
    if (ReadProgramProcessingInfo())
    {
      DeleteProgramProcessingInfo();
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF";
    }
  }

  private void DeleteProgramProcessingInfo()
  {
    Update("DeleteProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });
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
        entities.ProgramProcessingInfo.Populated = true;
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

// Program: READ_PGM_PROCESS_INFO_MULTI_DAY, ID: 372055828, model: 746.
// Short name: SWE01765
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: READ_PGM_PROCESS_INFO_MULTI_DAY.
/// </para>
/// <para>
/// RESP: FNCLMNGT
/// This action block will read the program processing info to get run 
/// parameters such as process date using the program name as input.
/// </para>
/// </summary>
[Serializable]
public partial class ReadPgmProcessInfoMultiDay: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the READ_PGM_PROCESS_INFO_MULTI_DAY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ReadPgmProcessInfoMultiDay(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ReadPgmProcessInfoMultiDay.
  /// </summary>
  public ReadPgmProcessInfoMultiDay(IContext context, Import import,
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
    // 12/27/2010  RMathews     CQ23999 Pull starting date for date monitor from
    // parameter list on SWEPB302 record
    foreach(var item in ReadProgramProcessingInfo())
    {
      if (IsEmpty(export.Ending.Name))
      {
        export.Ending.Assign(entities.ProgramProcessingInfo);

        if (IsEmpty(
          Substring(entities.ProgramProcessingInfo.ParameterList, 1, 8)))
        {
          export.Starting.ProcessDate =
            AddDays(entities.ProgramProcessingInfo.ProcessDate, -1);
        }
        else
        {
          export.Starting.ProcessDate =
            IntToDate((int)StringToNumber(Substring(
              entities.ProgramProcessingInfo.ParameterList, 1, 8)));
        }

        return;
      }
    }

    if (IsEmpty(export.Ending.Name))
    {
      ExitState = "ZD_PROGRAM_PROCESSING_INFO_NF_AB";
    }
  }

  private IEnumerable<bool> ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return ReadEach("ReadProgramProcessingInfo",
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

        return true;
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
    /// <summary>
    /// A value of Ending.
    /// </summary>
    [JsonPropertyName("ending")]
    public ProgramProcessingInfo Ending
    {
      get => ending ??= new();
      set => ending = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ProgramProcessingInfo Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private ProgramProcessingInfo ending;
    private ProgramProcessingInfo starting;
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

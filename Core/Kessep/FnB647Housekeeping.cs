// Program: FN_B647_HOUSEKEEPING, ID: 372995237, model: 746.
// Short name: SWE02395
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B647_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class FnB647Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B647_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB647Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB647Housekeeping.
  /// </summary>
  public FnB647Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************************************
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // **********************************************************
    local.ProgramProcessingInfo.Name = "SWEFB647";

    if (ReadProgramProcessingInfo())
    {
      local.PpiFound.Flag = "Y";
      export.ProgramProcessingInfo.Assign(entities.ProgramProcessingInfo);
    }

    if (AsChar(local.PpiFound.Flag) != 'Y')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = entities.ProgramProcessingInfo.Name;

    // **********************************************************
    // OPEN OUTPUT ERROR REPORT 99
    // **********************************************************
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      return;
    }

    // **********************************************************
    // OPEN OUTPUT CONTROL REPORT 98
    // **********************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      return;
    }

    // **********************************************************
    // OPEN INPUT STMT FILE
    // **********************************************************
    UseEabReadApStmtsVendorFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      return;
    }

    // ****************************************************************
    // LOOK AT PPI PARMLIST TO SEE IF A DATE WAS SUPPLIED
    // If date is supplied, any infrastructure records created on
    // that date will be deleted, otherwise no deletes will take place.
    // ****************************************************************
    if (Equal(export.ProgramProcessingInfo.ParameterList, 10, 5, "PURGE"))
    {
      if (Lt("00", Substring(export.ProgramProcessingInfo.ParameterList, 1, 2)) &&
        !Lt("99", Substring(export.ProgramProcessingInfo.ParameterList, 1, 2)))
      {
        local.PurgeAfterMonths.TotalInteger =
          StringToNumber(Substring(
            export.ProgramProcessingInfo.ParameterList, 1, 2));
        export.Purge.Date =
          AddMonths(export.ProgramProcessingInfo.ProcessDate, (int)(-
          local.PurgeAfterMonths.TotalInteger));

        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The parm list requested that infrastructure records older than " + Substring
          (export.ProgramProcessingInfo.ParameterList, 1, 2) + " months be deleted.";
          
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The PPI Parmlist did not include a valid number of months columns 1-2 (MM)." +
          " " + "   ";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FILE_WRITE_ERROR_RB";
        }
        else
        {
          ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
        }

        return;
      }
    }
    else if (Equal(export.ProgramProcessingInfo.ParameterList, 10, 6, "DELETE"))
    {
      UseFnB644ValidateParameters();

      if (AsChar(local.ValidDate.Flag) == 'N')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The PPI Parmlist did not include a valid deletion date in column 1 (MMDDYYYY)." +
          " " + "   ";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FILE_WRITE_ERROR_RB";
        }
        else
        {
          ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
        }

        return;
      }
      else
      {
        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "The parm list requested that infrastructure records created on: " + Substring
          (export.ProgramProcessingInfo.ParameterList, 1, 8) + " be deleted.   ";
          
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }
    else if (!IsEmpty(export.ProgramProcessingInfo.ParameterList))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Invalid PPI Parmlist: " + (
        export.ProgramProcessingInfo.ParameterList ?? "") + "   ";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FILE_WRITE_ERROR_RB";
      }
      else
      {
        ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
      }

      return;
    }

    // **********************************************************
    // GET COMMIT FREQUENCY AND SEE IF THIS IS A RESTART
    // **********************************************************
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // **** ok, continue processing. Pick up the last Cash Receipt Detail 
      // processed, so that during restart we can skip it ****
      if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
      {
        export.Restart.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo, 1, 10);
        export.RestartStatementNumber.Text4 =
          Substring(export.ProgramCheckpointRestart.RestartInfo, 11, 4);

        // **********************************************************
        // WRITE TO CONTROL REPORT 98
        // **********************************************************
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "This program is being restarted after obligor number: " + export
          .Restart.Number + "   ";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_CHECKPOINT_NF_RB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadApStmtsVendorFile()
  {
    var useImport = new EabReadApStmtsVendorFile.Import();
    var useExport = new EabReadApStmtsVendorFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadApStmtsVendorFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB644ValidateParameters()
  {
    var useImport = new FnB644ValidateParameters.Import();
    var useExport = new FnB644ValidateParameters.Export();

    useImport.ProgramProcessingInfo.ParameterList =
      export.ProgramProcessingInfo.ParameterList;

    Call(FnB644ValidateParameters.Execute, useImport, useExport);

    local.ValidDate.Flag = useExport.ValidDate.Flag;
    export.Deletion.Date = useExport.Conversion.Date;
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      export.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      export.ProgramCheckpointRestart);
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      (db, command) =>
      {
        db.SetString(command, "name", local.ProgramProcessingInfo.Name);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Deletion.
    /// </summary>
    [JsonPropertyName("deletion")]
    public DateWorkArea Deletion
    {
      get => deletion ??= new();
      set => deletion = value;
    }

    /// <summary>
    /// A value of Purge.
    /// </summary>
    [JsonPropertyName("purge")]
    public DateWorkArea Purge
    {
      get => purge ??= new();
      set => purge = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of RestartStatementNumber.
    /// </summary>
    [JsonPropertyName("restartStatementNumber")]
    public TextWorkArea RestartStatementNumber
    {
      get => restartStatementNumber ??= new();
      set => restartStatementNumber = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    private DateWorkArea deletion;
    private DateWorkArea purge;
    private CsePerson restart;
    private TextWorkArea restartStatementNumber;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PurgeAfterMonths.
    /// </summary>
    [JsonPropertyName("purgeAfterMonths")]
    public Common PurgeAfterMonths
    {
      get => purgeAfterMonths ??= new();
      set => purgeAfterMonths = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of PpiFound.
    /// </summary>
    [JsonPropertyName("ppiFound")]
    public Common PpiFound
    {
      get => ppiFound ??= new();
      set => ppiFound = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of ValidDate.
    /// </summary>
    [JsonPropertyName("validDate")]
    public Common ValidDate
    {
      get => validDate ??= new();
      set => validDate = value;
    }

    private Common purgeAfterMonths;
    private ProgramProcessingInfo programProcessingInfo;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common validDate;
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

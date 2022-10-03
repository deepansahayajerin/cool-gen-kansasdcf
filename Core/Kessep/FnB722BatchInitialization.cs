// Program: FN_B722_BATCH_INITIALIZATION, ID: 371198853, model: 746.
// Short name: SWE02062
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B722_BATCH_INITIALIZATION.
/// </summary>
[Serializable]
public partial class FnB722BatchInitialization: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B722_BATCH_INITIALIZATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB722BatchInitialization(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB722BatchInitialization.
  /// </summary>
  public FnB722BatchInitialization(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // ----------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // --------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------
    // -- Read the PPI record.
    // -------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // -------------------------------------------------------------------------------------------
    // -- Open Error Report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = global.UserId;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.  Had to 
      // open the error report before escaping to the PrAD.
      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Open Control Report.
    // -------------------------------------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_CONTROL_RPT";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Determine if this is a restart.
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------------
      // -- We are restarting.
      // -------------------------------------------------------------------------------------------
      // -- Extract the restart file number and reporting period from the 
      // restart info.
      export.RestartFileNumber.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 2));

      // -- Extract the timestamp of the OCSE34 record from the program 
      // processing info record.  The timestamp was set by SWEFB721.
      export.Ocse34.CreatedTimestamp =
        Timestamp(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 11, 26));

      // -- Read the OCSE34 record to get reporting period dates...
      if (ReadOcse34())
      {
        MoveOcse34(entities.Ocse34, export.Ocse34);
      }
      else
      {
        local.BatchTimestampWorkArea.IefTimestamp =
          export.Ocse34.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.EabReportSend.RptDetail =
          "Error reading OCSE34 record for created timestamp " + local
          .BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport1();
        ExitState = "FN0000_OCSE34_NF";

        return;
      }

      // -------------------------------------------------------------------------------------------
      // -- Write Restart info to the Control Report.
      // -------------------------------------------------------------------------------------------
      local.EabReportSend.RptDetail = "RESTARTING AT FILE " + Substring
        (local.ProgramCheckpointRestart.RestartInfo, 250, 1, 2);
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }
    else
    {
      // -------------------------------------------------------------------------------------------
      // -- We are not restarting.
      // -------------------------------------------------------------------------------------------
      export.RestartFileNumber.Count = 1;

      // -- Extract the timestamp of the OCSE34 record from the program 
      // processing info record.  The timestamp was set by SWEFB721.
      export.Ocse34.CreatedTimestamp =
        Timestamp(Substring(local.ProgramProcessingInfo.ParameterList, 8, 26));

      // -- Determine the reporting period and reporting period end date.
      if (ReadOcse34())
      {
        MoveOcse34(entities.Ocse34, export.Ocse34);
      }
      else
      {
        local.BatchTimestampWorkArea.IefTimestamp =
          export.Ocse34.CreatedTimestamp;
        UseLeCabConvertTimestamp();
        local.EabReportSend.RptDetail =
          "Error reading OCSE34 record for created timestamp " + local
          .BatchTimestampWorkArea.TextTimestamp;
        UseCabErrorReport1();
        ExitState = "FN0000_OCSE34_NF";

        return;
      }

      // -------------------------------------------------------------------------------------------
      // -- Take an initial Checkpoint
      // -------------------------------------------------------------------------------------------
      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      local.ProgramCheckpointRestart.RestartInd = "Y";
      local.ProgramCheckpointRestart.RestartInfo = "01 " + Substring
        (local.ProgramProcessingInfo.ParameterList, 1, 34);
      UseUpdateCheckpointRstAndCommit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Parameters to the Control Report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "RUN PARAMETERS";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Report Quarter...............................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (entities.Ocse34.Period, 10, 6);
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail =
      Substring("Report Period...............................................",
      1, 30);
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(entities.Ocse34.ReportingPeriodBeginDate), 8, 8) + "-";
    local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
      (DateToInt(export.Ocse34.ReportingPeriodEndDate), 8, 8) + " ";
    UseCabControlReport3();

    if (!Equal(local.Status.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail = "OCSE34 KPC EXTRACT TOTALS";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";
    }
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.ReportingPeriodEndDate = source.ReportingPeriodEndDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.CheckpointCount = source.CheckpointCount;
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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Status.Status = useExport.EabFileHandling.Status;
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
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    local.BatchTimestampWorkArea.Assign(useExport.BatchTimestampWorkArea);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          export.Ocse34.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.Ocse34.ReportingPeriodBeginDate =
          db.GetNullableDate(reader, 2);
        entities.Ocse34.ReportingPeriodEndDate = db.GetNullableDate(reader, 3);
        entities.Ocse34.Populated = true;
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
    /// A value of RestartFileNumber.
    /// </summary>
    [JsonPropertyName("restartFileNumber")]
    public Common RestartFileNumber
    {
      get => restartFileNumber ??= new();
      set => restartFileNumber = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Common restartFileNumber;
    private Ocse34 ocse34;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public EabFileHandling Status
    {
      get => status ??= new();
      set => status = value;
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend eabReportSend;
    private EabFileHandling status;
    private ProgramProcessingInfo programProcessingInfo;
    private EabReportSend open;
    private BatchTimestampWorkArea batchTimestampWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}

// Program: OE_B414_DELETE_OLD_1099_REQ_RESP, ID: 373547213, model: 746.
// Short name: SWEE414B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B414_DELETE_OLD_1099_REQ_RESP.
/// </para>
/// <para>
/// 1. Purge 180 days old 1099 responses information from production database.
/// 2. Purge one year old sent 1099 requests information from production 
/// database after purging all responses for that request (make sure, there are
/// no responses for the request before purging the 1099  request) .
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB414DeleteOld1099ReqResp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B414_DELETE_OLD_1099_REQ_RESP program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB414DeleteOld1099ReqResp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB414DeleteOld1099ReqResp.
  /// </summary>
  public OeB414DeleteOld1099ReqResp(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    //   DATE		Developer	Description
    // 03/06/00     	Srini Ganji	Initial Creation.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // ************************************************
    // Get the RUN Paramateres for this program
    // ************************************************
    local.ProgramProcessingInfo.Name = "SWEEB414";
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****************************************************************
      // Program process date = Null then set it to Current Date
      // *****************************************************************
      if (!Lt(local.NullDate.Date, local.ProgramProcessingInfo.ProcessDate))
      {
        local.ProgramProcessingInfo.ProcessDate = Now().Date;
      }

      // *****************************************************************
      // Read Input Parameters, if any...
      // *****************************************************************
      // *****************************************************************
      // Set default value for Delete Days to 180 Days
      // *****************************************************************
      local.NnnNumericDays.Count = 180;

      if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
      {
        // *****************************************************************
        // Extract Delete days from Prarameter list.
        // If DAYS parameter is non numeric or less than 180 then default to 
        // 180.
        // *****************************************************************
        local.Position.Count =
          Find(local.ProgramProcessingInfo.ParameterList, "DAYS=");

        if (local.Position.Count > 0)
        {
          local.NnnTextDays.TextLine8 = "00000" + Substring
            (local.ProgramProcessingInfo.ParameterList, local.Position.Count +
            5, 3);

          if (Verify(local.NnnTextDays.TextLine8, "0123456789") == 0)
          {
            local.NnnNumericDays.Count =
              (int)StringToNumber(local.NnnTextDays.TextLine8);

            if (local.NnnNumericDays.Count < 180)
            {
              local.NnnNumericDays.Count = 180;
            }
          }
        }
      }
    }
    else
    {
      return;
    }

    // *****************************************************************
    // Open ERROR Report, DD Name = RPT99
    // *****************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // *****************************************************************
    // Open CONTROL Report, DD Name = RPT98
    // *****************************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error opening Control Report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ************************************************
    // *Get the DB2 commit frequency count.           *
    // ************************************************
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "checkpoint restart not found";
      UseCabErrorReport1();

      return;
    }

    // ************************************************
    // Begin FPLS Delete processing
    // ************************************************
    local.Commits.Count = 0;
    local.ReqRead.Count = 0;
    local.ResRead.Count = 0;
    local.ReqDelCount.Count = 0;
    local.ResDelCount.Count = 0;

    // ************************************************
    // Set NNN days old date to ( Process date - input NNN days)
    // ************************************************
    local.NnnDaysOldDate.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate, -
      local.NnnNumericDays.Count);

    // ************************************************
    // Set one year old date to ( Process date - one year)
    // ************************************************
    local.LoclaOneYearOldDate.Date =
      AddYears(local.ProgramProcessingInfo.ProcessDate, -1);

    // *****************************************************************
    // Write Input parameter Info on CONTROL Report
    // *****************************************************************
    local.Repeat.Count = 1;

    do
    {
      switch(local.Repeat.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Parameter List    : " + (
            local.ProgramProcessingInfo.ParameterList ?? "");

          break;
        case 2:
          local.TempText.TextLine80 =
            NumberToString(Year(local.ProgramProcessingInfo.ProcessDate), 12, 4) +
            "-" + NumberToString
            (Month(local.ProgramProcessingInfo.ProcessDate), 14, 2) + "-";
          local.TempText.TextLine80 =
            Substring(local.TempText.TextLine80, External.TextLine80_MaxLength,
            1, 8) + NumberToString
            (Day(local.ProgramProcessingInfo.ProcessDate), 14, 2);
          local.EabReportSend.RptDetail = "Process Date      : " + Substring
            (local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Delete Days       : " + NumberToString
            (local.NnnNumericDays.Count, 13, 3);

          break;
        case 4:
          local.TempText.TextLine80 =
            NumberToString(Year(local.NnnDaysOldDate.Date), 12, 4) + "-" + NumberToString
            (Month(local.NnnDaysOldDate.Date), 14, 2) + "-";
          local.TempText.TextLine80 =
            Substring(local.TempText.TextLine80, External.TextLine80_MaxLength,
            1, 8) + NumberToString(Day(local.NnnDaysOldDate.Date), 14, 2);
          local.EabReportSend.RptDetail = "Delete Date       : " + Substring
            (local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10);

          break;
        case 5:
          local.TempText.TextLine80 =
            NumberToString(Year(local.LoclaOneYearOldDate.Date), 12, 4) + "-"
            + NumberToString(Month(local.LoclaOneYearOldDate.Date), 14, 2) + "-"
            ;
          local.TempText.TextLine80 =
            Substring(local.TempText.TextLine80, External.TextLine80_MaxLength,
            1, 8) + NumberToString(Day(local.LoclaOneYearOldDate.Date), 14, 2);
          local.EabReportSend.RptDetail = "One year old Date : " + Substring
            (local.TempText.TextLine80, External.TextLine80_MaxLength, 1, 10);

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error while writing control report for Input parameter Info";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Repeat.Count;
    }
    while(local.Repeat.Count != 6);

    // ************************************************
    // Read only 180 days old Sent 1099_Requests
    // ************************************************
    foreach(var item in Read1099LocateRequest())
    {
      ++local.ReqRead.Count;

      // *****************************************************************
      // Read 1099 Responses for read 1099 Request
      // *****************************************************************
      local.ReqDel.Flag = "Y";

      foreach(var item1 in Read1099LocateResponse())
      {
        ++local.ResRead.Count;

        // *****************************************************************
        // Delete if 1099 Response 180 days old
        // *****************************************************************
        if (Lt(entities.Data1099LocateResponse.DateReceived,
          local.NnnDaysOldDate.Date))
        {
          Delete1099LocateResponse();
          ++local.ResDelCount.Count;
          ++local.Commits.Count;
        }
        else
        {
          local.ReqDel.Flag = "N";
        }
      }

      if (AsChar(local.ReqDel.Flag) == 'Y')
      {
        // *****************************************************************
        // Delete if 1099 Request One year old
        // *****************************************************************
        if (Lt(entities.Data1099LocateRequest.RequestSentDate,
          local.LoclaOneYearOldDate.Date))
        {
          Delete1099LocateRequest();
          ++local.ReqDelCount.Count;
          ++local.Commits.Count;
        }
      }

      // *****************************************************************
      // Check for Commit count
      // *****************************************************************
      if (local.Commits.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // Commit database
        // *****************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.EabReportSend.RptDetail = "1099 Requests read so far : " + NumberToString
          (local.ReqRead.Count, 15);
        local.EabReportSend.RptDetail =
          TrimEnd(local.EabReportSend.RptDetail) + " Time : " + NumberToString
          (TimeToInt(Time(Now())), 10, 6);
        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error while writing control report for Input parameter Info";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.Commits.Count = 0;
      }
    }

    // *****************************************************************
    // Do final Commit
    // *****************************************************************
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    // *****************************************************************
    // Write Control Report Summary
    // *****************************************************************
    local.Repeat.Count = 1;

    do
    {
      switch(local.Repeat.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "Total 1099 Requests read     :" + NumberToString
            (local.ReqRead.Count, 15);

          break;
        case 2:
          local.EabReportSend.RptDetail = "Total 1099 Responses read    :" + NumberToString
            (local.ResRead.Count, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Total 1099 Requests deleted  :" + NumberToString
            (local.ReqDelCount.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "Total 1099 Responses deleted :" + NumberToString
            (local.ResDelCount.Count, 15);

          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error while writing control report for summary details";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.Repeat.Count;
    }
    while(local.Repeat.Count != 5);

    // *****************************************************************
    // Close Control Report File
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered while closing control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****************************************************************
    // Close Error Report File
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";

    // *****************************************************************
    // End of 1099 delete Process
    // *****************************************************************
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

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
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

  private void Delete1099LocateRequest()
  {
    Update("Delete1099LocateRequest",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.Data1099LocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", entities.Data1099LocateRequest.Identifier);
      });
  }

  private void Delete1099LocateResponse()
  {
    Update("Delete1099LocateResponse",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier",
          entities.Data1099LocateResponse.LrqIdentifier);
        db.SetString(
          command, "cspNumber", entities.Data1099LocateResponse.CspNumber);
        db.SetInt32(
          command, "identifier", entities.Data1099LocateResponse.Identifier);
      });
  }

  private IEnumerable<bool> Read1099LocateRequest()
  {
    entities.Data1099LocateRequest.Populated = false;

    return ReadEach("Read1099LocateRequest",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "requestSentDate1", local.NullDate.Date.GetValueOrDefault());
          
        db.SetNullableDate(
          command, "requestSentDate2",
          local.NnnDaysOldDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Data1099LocateRequest.CspNumber = db.GetString(reader, 0);
        entities.Data1099LocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.Data1099LocateRequest.RequestSentDate =
          db.GetNullableDate(reader, 2);
        entities.Data1099LocateRequest.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> Read1099LocateResponse()
  {
    System.Diagnostics.Debug.Assert(entities.Data1099LocateRequest.Populated);
    entities.Data1099LocateResponse.Populated = false;

    return ReadEach("Read1099LocateResponse",
      (db, command) =>
      {
        db.SetInt32(
          command, "lrqIdentifier", entities.Data1099LocateRequest.Identifier);
        db.SetString(
          command, "cspNumber", entities.Data1099LocateRequest.CspNumber);
      },
      (db, reader) =>
      {
        entities.Data1099LocateResponse.LrqIdentifier = db.GetInt32(reader, 0);
        entities.Data1099LocateResponse.CspNumber = db.GetString(reader, 1);
        entities.Data1099LocateResponse.Identifier = db.GetInt32(reader, 2);
        entities.Data1099LocateResponse.DateReceived =
          db.GetNullableDate(reader, 3);
        entities.Data1099LocateResponse.Populated = true;

        return true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LoclaOneYearOldDate.
    /// </summary>
    [JsonPropertyName("loclaOneYearOldDate")]
    public DateWorkArea LoclaOneYearOldDate
    {
      get => loclaOneYearOldDate ??= new();
      set => loclaOneYearOldDate = value;
    }

    /// <summary>
    /// A value of ResRead.
    /// </summary>
    [JsonPropertyName("resRead")]
    public Common ResRead
    {
      get => resRead ??= new();
      set => resRead = value;
    }

    /// <summary>
    /// A value of ReqDelCount.
    /// </summary>
    [JsonPropertyName("reqDelCount")]
    public Common ReqDelCount
    {
      get => reqDelCount ??= new();
      set => reqDelCount = value;
    }

    /// <summary>
    /// A value of ResDelCount.
    /// </summary>
    [JsonPropertyName("resDelCount")]
    public Common ResDelCount
    {
      get => resDelCount ??= new();
      set => resDelCount = value;
    }

    /// <summary>
    /// A value of ReqDel.
    /// </summary>
    [JsonPropertyName("reqDel")]
    public Common ReqDel
    {
      get => reqDel ??= new();
      set => reqDel = value;
    }

    /// <summary>
    /// A value of NnnTextDays.
    /// </summary>
    [JsonPropertyName("nnnTextDays")]
    public External NnnTextDays
    {
      get => nnnTextDays ??= new();
      set => nnnTextDays = value;
    }

    /// <summary>
    /// A value of Position.
    /// </summary>
    [JsonPropertyName("position")]
    public Common Position
    {
      get => position ??= new();
      set => position = value;
    }

    /// <summary>
    /// A value of NnnNumericDays.
    /// </summary>
    [JsonPropertyName("nnnNumericDays")]
    public Common NnnNumericDays
    {
      get => nnnNumericDays ??= new();
      set => nnnNumericDays = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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

    /// <summary>
    /// A value of Repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    public Common Repeat
    {
      get => repeat ??= new();
      set => repeat = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of NnnDaysOldDate.
    /// </summary>
    [JsonPropertyName("nnnDaysOldDate")]
    public DateWorkArea NnnDaysOldDate
    {
      get => nnnDaysOldDate ??= new();
      set => nnnDaysOldDate = value;
    }

    /// <summary>
    /// A value of ReqRead.
    /// </summary>
    [JsonPropertyName("reqRead")]
    public Common ReqRead
    {
      get => reqRead ??= new();
      set => reqRead = value;
    }

    /// <summary>
    /// A value of TempText.
    /// </summary>
    [JsonPropertyName("tempText")]
    public External TempText
    {
      get => tempText ??= new();
      set => tempText = value;
    }

    /// <summary>
    /// A value of Commits.
    /// </summary>
    [JsonPropertyName("commits")]
    public Common Commits
    {
      get => commits ??= new();
      set => commits = value;
    }

    private DateWorkArea loclaOneYearOldDate;
    private Common resRead;
    private Common reqDelCount;
    private Common resDelCount;
    private Common reqDel;
    private External nnnTextDays;
    private Common position;
    private Common nnnNumericDays;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External passArea;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common repeat;
    private DateWorkArea nullDate;
    private DateWorkArea nnnDaysOldDate;
    private Common reqRead;
    private External tempText;
    private Common commits;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Data1099LocateResponse.
    /// </summary>
    [JsonPropertyName("data1099LocateResponse")]
    public Data1099LocateResponse Data1099LocateResponse
    {
      get => data1099LocateResponse ??= new();
      set => data1099LocateResponse = value;
    }

    /// <summary>
    /// A value of Data1099LocateRequest.
    /// </summary>
    [JsonPropertyName("data1099LocateRequest")]
    public Data1099LocateRequest Data1099LocateRequest
    {
      get => data1099LocateRequest ??= new();
      set => data1099LocateRequest = value;
    }

    private Data1099LocateResponse data1099LocateResponse;
    private Data1099LocateRequest data1099LocateRequest;
  }
#endregion
}

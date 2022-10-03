// Program: SP_B707_HOUSEKEEPING, ID: 374481002, model: 746.
// Short name: SWE02616
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B707_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB707Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B707_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB707Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB707Housekeeping.
  /// </summary>
  public SpB707Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********
    // 10/31/2008      Linda/Arun      CQ#7505        Added Run mode 
    // functionality
    // 04/25/2011   AHockman       CQ26649         Changed coding to check for 
    // run set to C, removed the else that set it to R every time.
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB707";
    export.Current.Timestamp = Now();
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      export.ProgramProcessingInfo);

    // mjr---> If ppi process_date is null, set it to current date
    if (!Lt(local.Null1.Date, export.ProgramProcessingInfo.ProcessDate))
    {
      export.ProgramProcessingInfo.ProcessDate = Now().Date;
    }

    // --------------------------------------------------------------------
    // SET RUNTIME PARAMETERS TO DEFAULTS
    // --------------------------------------------------------------------
    export.DebugOn.Flag = "N";
    export.MaximumTriggers.Count = 5000;
    export.NewestReview.Date =
      AddMonths(export.ProgramProcessingInfo.ProcessDate, -35);
    local.BatchTimestampWorkArea.TextDate =
      NumberToString(DateToInt(export.NewestReview.Date), 8, 8);
    export.NewestReview.Timestamp =
      Timestamp(Substring(
        local.BatchTimestampWorkArea.TextDate,
      BatchTimestampWorkArea.TextDate_MaxLength, 1, 4) + "-" + Substring
      (local.BatchTimestampWorkArea.TextDate,
      BatchTimestampWorkArea.TextDate_MaxLength, 5, 2) + "-" + Substring
      (local.BatchTimestampWorkArea.TextDate,
      BatchTimestampWorkArea.TextDate_MaxLength, 7, 2) + "-00.00.00.000000");

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG=FULL");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag = "F";
      }
      else
      {
        local.Position.Count =
          Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

        if (local.Position.Count > 0)
        {
          export.DebugOn.Flag = "Y";
        }
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "MAX_TRIGGERS:");

      if (local.Position.Count > 0)
      {
        local.WorkArea.Text5 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 13, 5);

        if (Lt("00000", local.WorkArea.Text5) && !
          Lt("99999", local.WorkArea.Text5))
        {
          export.MaximumTriggers.Count =
            (int)StringToNumber(local.WorkArea.Text5);
        }
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DATE_NEWEST:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.BatchTimestampWorkArea.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 12, 2);
        local.BatchTimestampWorkArea.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 14, 2);
        local.BatchTimestampWorkArea.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 16, 4);

        // mjr---> Validate local views
        if (Lt(local.BatchTimestampWorkArea.TextDateYyyy, "0001") || Lt
          ("2099", local.BatchTimestampWorkArea.TextDateYyyy))
        {
          goto Test;
        }

        if (Lt(local.BatchTimestampWorkArea.TextDateMm, "01") || Lt
          ("12", local.BatchTimestampWorkArea.TextDateMm))
        {
          goto Test;
        }

        if (Lt(local.BatchTimestampWorkArea.TestDateDd, "01") || Lt
          ("31", local.BatchTimestampWorkArea.TestDateDd))
        {
          goto Test;
        }

        if (Lt("30", local.BatchTimestampWorkArea.TestDateDd) && (
          Equal(local.BatchTimestampWorkArea.TextDateMm, "04") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "06") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "09") || Equal
          (local.BatchTimestampWorkArea.TextDateMm, "11")))
        {
          goto Test;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.BatchTimestampWorkArea.TestDateDd) && Equal
          (local.BatchTimestampWorkArea.TextDateMm, "02"))
        {
          local.Calc.Count =
            (int)StringToNumber(local.BatchTimestampWorkArea.TextDateYyyy);
          local.Calc.TotalReal = (decimal)local.Calc.Count / 4;
          local.Calc.TotalInteger = local.Calc.Count / 4;

          if (local.Calc.TotalInteger != local.Calc.TotalReal)
          {
            goto Test;
          }

          local.Calc.TotalReal = (decimal)local.Calc.Count / 100;
          local.Calc.TotalInteger = local.Calc.Count / 100;

          if (local.Calc.TotalInteger == local.Calc.TotalReal)
          {
            local.Calc.TotalReal = (decimal)local.Calc.Count / 400;
            local.Calc.TotalInteger = local.Calc.Count / 400;

            if (local.Calc.TotalInteger != local.Calc.TotalReal)
            {
              goto Test;
            }
          }
        }

        // mjr---> Construct text date
        local.BatchTimestampWorkArea.TextDate =
          local.BatchTimestampWorkArea.TextDateYyyy + local
          .BatchTimestampWorkArea.TextDateMm + local
          .BatchTimestampWorkArea.TestDateDd;

        // mjr---> Construct ief date
        export.NewestReview.Date =
          IntToDate((int)StringToNumber(local.BatchTimestampWorkArea.TextDate));
          

        // mjr---> Construct ief timestamp
        export.NewestReview.Timestamp =
          Timestamp(local.BatchTimestampWorkArea.TextDateYyyy + "-" + local
          .BatchTimestampWorkArea.TextDateMm + "-" + local
          .BatchTimestampWorkArea.TestDateDd + "-00.00.00.000000");
      }

Test:

      // CQ7505 starts here
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "RUN_MODE:");

      if (local.Position.Count > 0)
      {
        local.WorkArea.Text1 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 1);

        // *** changed this from an if else to a case of statement.  Prior code 
        // was never setting the export run mode flag to C if the Parm had a C.
        // AHockman
        switch(AsChar(local.WorkArea.Text1))
        {
          case 'C':
            export.RunMode.Flag = "C";

            break;
          case 'R':
            export.RunMode.Flag = "R";

            break;
          default:
            export.RunMode.Flag = "R";

            break;
        }
      }

      // CQ7505 ends here
    }

    // --------------------------------------------------------------------
    // DETERMINE IF THIS IS A RESTART SITUATION
    // --------------------------------------------------------------------
    export.ProgramCheckpointRestart.ProgramName =
      export.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // --------------------------------------------------------------------
      // EXTRACT RESTART PARAMETERS FROM RESTART_INFO
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(export.ProgramCheckpointRestart.RestartInfo, "CASE:");

      if (local.Position.Count > 0)
      {
        export.Restart.Number =
          Substring(export.ProgramCheckpointRestart.RestartInfo,
          local.Position.Count + 5, 10);
      }
    }

    // -------------------------------------------------------
    // OPEN OUTPUT ERROR REPORT 99
    // -------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = export.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = export.ProgramProcessingInfo.Name;
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ------------------------------------------------------------
    // OPEN OUTPUT CONTROL REPORT 98
    // ------------------------------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO ERROR REPORT 99
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO CONTROL REPORT 98
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail = "R U N   T I M E   P A R A M E T E R S";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.DebugOn.Flag) == 'F')
    {
      local.EabReportSend.RptDetail = "DEBUG:  FULL";
    }
    else if (AsChar(export.DebugOn.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "DEBUG:  ON";
    }
    else
    {
      local.EabReportSend.RptDetail = "DEBUG:  OFF";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.WorkArea.Text15 = NumberToString(export.MaximumTriggers.Count, 15);
    local.Position.Count = Verify(local.WorkArea.Text15, "0");

    if (local.Position.Count > 0)
    {
      local.WorkArea.Text15 =
        Substring(local.WorkArea.Text15, local.Position.Count, 16 -
        local.Position.Count);
    }
    else
    {
      local.WorkArea.Text15 = "0";
    }

    local.EabReportSend.RptDetail = "MAXIMUM DOCUMENT TRIGGERS:  " + local
      .WorkArea.Text15;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabReportSend.RptDetail =
      "NEWEST REVIEW DATE (THAT WILL GET A NEW DOCUMENT):  " + local
      .BatchTimestampWorkArea.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // CQ7505 starts here
    if (AsChar(export.RunMode.Flag) == 'R')
    {
      local.EabReportSend.RptDetail = "RUN MODE : R(eview)";
    }
    else
    {
      local.EabReportSend.RptDetail = "RUN MODE : C(reate)";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // CQ7505 ends here
    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.EabReportSend.RptDetail = "RESTART:  YES";
    }
    else
    {
      local.EabReportSend.RptDetail = "RESTART:  NO";
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (AsChar(export.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.EabReportSend.RptDetail = "     Starting Case:  " + export
        .Restart.Number;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabReportSend.RptDetail = "Error writing to Control Report";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail = "Error writing to Control Report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
    export.ModificationReview.EventId = 8;
    export.ModificationReview.ReasonCode = "MODFNRVWDT";
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

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of ModificationReview.
    /// </summary>
    [JsonPropertyName("modificationReview")]
    public Infrastructure ModificationReview
    {
      get => modificationReview ??= new();
      set => modificationReview = value;
    }

    /// <summary>
    /// A value of MaximumTriggers.
    /// </summary>
    [JsonPropertyName("maximumTriggers")]
    public Common MaximumTriggers
    {
      get => maximumTriggers ??= new();
      set => maximumTriggers = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Case1 Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of NewestReview.
    /// </summary>
    [JsonPropertyName("newestReview")]
    public DateWorkArea NewestReview
    {
      get => newestReview ??= new();
      set => newestReview = value;
    }

    /// <summary>
    /// A value of RunMode.
    /// </summary>
    [JsonPropertyName("runMode")]
    public Common RunMode
    {
      get => runMode ??= new();
      set => runMode = value;
    }

    private Infrastructure modificationReview;
    private Common maximumTriggers;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Case1 restart;
    private Common debugOn;
    private DateWorkArea current;
    private DateWorkArea newestReview;
    private Common runMode;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public Common Start
    {
      get => start ??= new();
      set => start = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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

    /// <summary>
    /// A value of Calc.
    /// </summary>
    [JsonPropertyName("calc")]
    public Common Calc
    {
      get => calc ??= new();
      set => calc = value;
    }

    private WorkArea workArea;
    private Common length;
    private Common start;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common calc;
  }
#endregion
}

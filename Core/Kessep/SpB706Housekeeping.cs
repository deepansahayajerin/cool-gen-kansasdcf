// Program: SP_B706_HOUSEKEEPING, ID: 372988031, model: 746.
// Short name: SWE02506
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B706_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB706Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B706_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB706Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB706Housekeeping.
  /// </summary>
  public SpB706Housekeeping(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------
    // GET PROCESS DATE & OPTIONAL PARAMETERS
    // --------------------------------------------------------------
    local.ProgramProcessingInfo.Name = "SWEPB706";
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
    export.Debug.Flag = "N";
    export.Document.Name = "";
    export.DateStop.Timestamp =
      AddMicroseconds(new DateTime(2099, 12, 31, 23, 59, 59), 999999);
    local.DateStart.TextDate = "00010101";
    local.DateStop.TextDate = "20991231";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG");

      if (local.Position.Count > 0)
      {
        export.Debug.Flag = "Y";
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DOCUMENT:");

      if (local.Position.Count > 0)
      {
        export.Document.Name =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 8);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DATE_S:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.DateStart.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 7, 2);
        local.DateStart.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 2);
        local.DateStart.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 11, 4);

        // mjr---> Validate local views
        if (Lt(local.DateStart.TextDateYyyy, "0001") || Lt
          ("2099", local.DateStart.TextDateYyyy))
        {
          goto Test1;
        }

        if (Lt(local.DateStart.TextDateMm, "01") || Lt
          ("12", local.DateStart.TextDateMm))
        {
          goto Test1;
        }

        if (Lt(local.DateStart.TestDateDd, "01") || Lt
          ("31", local.DateStart.TestDateDd))
        {
          goto Test1;
        }

        if (Lt("30", local.DateStart.TestDateDd) && (
          Equal(local.DateStart.TextDateMm, "04") || Equal
          (local.DateStart.TextDateMm, "06") || Equal
          (local.DateStart.TextDateMm, "09") || Equal
          (local.DateStart.TextDateMm, "11")))
        {
          goto Test1;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.DateStart.TestDateDd) && Equal
          (local.DateStart.TextDateMm, "02"))
        {
          local.Calc.Count = (int)StringToNumber(local.DateStart.TextDateYyyy);
          local.Calc.TotalReal = (decimal)local.Calc.Count / 4;
          local.Calc.TotalInteger = local.Calc.Count / 4;

          if (local.Calc.TotalInteger != local.Calc.TotalReal)
          {
            goto Test1;
          }

          local.Calc.TotalReal = (decimal)local.Calc.Count / 100;
          local.Calc.TotalInteger = local.Calc.Count / 100;

          if (local.Calc.TotalInteger == local.Calc.TotalReal)
          {
            local.Calc.TotalReal = (decimal)local.Calc.Count / 400;
            local.Calc.TotalInteger = local.Calc.Count / 400;

            if (local.Calc.TotalInteger != local.Calc.TotalReal)
            {
              goto Test1;
            }
          }
        }

        // mjr---> Construct text date
        local.DateStart.TextDate = local.DateStart.TextDateYyyy + local
          .DateStart.TextDateMm + local.DateStart.TestDateDd;

        // mjr---> Construct ief timestamp
        export.DateStart.Timestamp =
          Timestamp(local.DateStart.TextDateYyyy + "-" + local
          .DateStart.TextDateMm + "-" + local.DateStart.TestDateDd);
      }

Test1:

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DATE_E:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.DateStop.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 7, 2);
        local.DateStop.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 2);
        local.DateStop.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 11, 4);

        // mjr---> Validate local views
        if (Lt(local.DateStop.TextDateYyyy, "0001") || Lt
          ("2099", local.DateStop.TextDateYyyy))
        {
          goto Test2;
        }

        if (Lt(local.DateStop.TextDateMm, "01") || Lt
          ("12", local.DateStop.TextDateMm))
        {
          goto Test2;
        }

        if (Lt(local.DateStop.TestDateDd, "01") || Lt
          ("31", local.DateStop.TestDateDd))
        {
          goto Test2;
        }

        if (Lt("30", local.DateStop.TestDateDd) && (
          Equal(local.DateStop.TextDateMm, "04") || Equal
          (local.DateStop.TextDateMm, "06") || Equal
          (local.DateStop.TextDateMm, "09") || Equal
          (local.DateStop.TextDateMm, "11")))
        {
          goto Test2;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.DateStop.TestDateDd) && Equal
          (local.DateStop.TextDateMm, "02"))
        {
          local.Calc.Count = (int)StringToNumber(local.DateStop.TextDateYyyy);
          local.Calc.TotalReal = (decimal)local.Calc.Count / 4;
          local.Calc.TotalInteger = local.Calc.Count / 4;

          if (local.Calc.TotalInteger != local.Calc.TotalReal)
          {
            goto Test2;
          }

          local.Calc.TotalReal = (decimal)local.Calc.Count / 100;
          local.Calc.TotalInteger = local.Calc.Count / 100;

          if (local.Calc.TotalInteger == local.Calc.TotalReal)
          {
            local.Calc.TotalReal = (decimal)local.Calc.Count / 400;
            local.Calc.TotalInteger = local.Calc.Count / 400;

            if (local.Calc.TotalInteger != local.Calc.TotalReal)
            {
              goto Test2;
            }
          }
        }

        // mjr---> Construct text date
        local.DateStop.TextDate = local.DateStop.TextDateYyyy + local
          .DateStop.TextDateMm + local.DateStop.TestDateDd;

        // mjr---> Construct ief timestamp
        export.DateStop.Timestamp =
          Timestamp(local.DateStop.TextDateYyyy + "-" + local
          .DateStop.TextDateMm + "-" + local.DateStop.TestDateDd + "-23.59.59.999999"
          );
      }

Test2:

      if (Lt(export.DateStop.Timestamp, export.DateStart.Timestamp))
      {
        local.DateWorkArea.Timestamp = export.DateStart.Timestamp;
        export.DateStart.Timestamp = export.DateStop.Timestamp;
        export.DateStop.Timestamp = local.DateWorkArea.Timestamp;
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
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "I N P U T   P A R A M E T E R S";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "-------------------------------";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DEBUG:  " + export.Debug.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DOCUMENT:   " + export.Document.Name;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DATE_START:     " + local
      .DateStart.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DATE_STOP:      " + local
      .DateStop.TextDate;
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

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
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
    /// A value of DateStart.
    /// </summary>
    [JsonPropertyName("dateStart")]
    public DateWorkArea DateStart
    {
      get => dateStart ??= new();
      set => dateStart = value;
    }

    /// <summary>
    /// A value of DateStop.
    /// </summary>
    [JsonPropertyName("dateStop")]
    public DateWorkArea DateStop
    {
      get => dateStop ??= new();
      set => dateStop = value;
    }

    private DateWorkArea current;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common debug;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea dateStart;
    private DateWorkArea dateStop;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of DateStart.
    /// </summary>
    [JsonPropertyName("dateStart")]
    public BatchTimestampWorkArea DateStart
    {
      get => dateStart ??= new();
      set => dateStart = value;
    }

    /// <summary>
    /// A value of DateStop.
    /// </summary>
    [JsonPropertyName("dateStop")]
    public BatchTimestampWorkArea DateStop
    {
      get => dateStop ??= new();
      set => dateStop = value;
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

    private DateWorkArea dateWorkArea;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private BatchTimestampWorkArea dateStart;
    private BatchTimestampWorkArea dateStop;
    private Common calc;
  }
#endregion
}

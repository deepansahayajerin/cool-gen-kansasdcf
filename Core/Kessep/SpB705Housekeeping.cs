// Program: SP_B705_HOUSEKEEPING, ID: 372448380, model: 746.
// Short name: SWE02327
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B705_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class SpB705Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B705_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB705Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB705Housekeeping.
  /// </summary>
  public SpB705Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEPB705";
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
    local.BatchTimestampWorkArea.TextDate = "00010101";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DEBUG:");

      if (local.Position.Count > 0)
      {
        export.DebugOn.Flag =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 1);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "START:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.BatchTimestampWorkArea.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 6, 2);
        local.BatchTimestampWorkArea.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 8, 2);
        local.BatchTimestampWorkArea.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 10, 4);

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
        export.CollectionStart.Date =
          IntToDate((int)StringToNumber(local.BatchTimestampWorkArea.TextDate));
          
      }
    }

Test:

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

    local.EabReportSend.RptDetail =
      "Collections created date must be greater or equal to " + local
      .BatchTimestampWorkArea.TextDate;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CollectionStart.
    /// </summary>
    [JsonPropertyName("collectionStart")]
    public DateWorkArea CollectionStart
    {
      get => collectionStart ??= new();
      set => collectionStart = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Common debugOn;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea collectionStart;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common calc;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public ProgramProcessingInfo Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private ProgramProcessingInfo zdel;
  }
#endregion
}

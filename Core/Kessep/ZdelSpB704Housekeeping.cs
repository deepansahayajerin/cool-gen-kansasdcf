// Program: ZDEL_SP_B704_HOUSEKEEPING, ID: 372985828, model: 746.
// Short name: ZDEL2501
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: ZDEL_SP_B704_HOUSEKEEPING.
/// </summary>
[Serializable]
public partial class ZdelSpB704Housekeeping: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the ZDEL_SP_B704_HOUSEKEEPING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new ZdelSpB704Housekeeping(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of ZdelSpB704Housekeeping.
  /// </summary>
  public ZdelSpB704Housekeeping(IContext context, Import import, Export export):
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
    local.ProgramProcessingInfo.Name = "SWEPB704";
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
    export.ErrorsOnly.Flag = "N";
    export.AdjustedInd.Flag = "N";
    export.ObligorStart.Number = "0000000000";
    export.ObligorStop.Number = "9999999999";
    export.CollectionDateStop.Date = new DateTime(2099, 12, 31);
    local.CollectionDateStart.TextDate = "00010101";
    local.CollectionDateStop.TextDate = "20991231";

    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // --------------------------------------------------------------------
      // EXTRACT RUNTIME PARAMETERS FROM PPI
      // --------------------------------------------------------------------
      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "OBLIGOR_S:");

      if (local.Position.Count > 0)
      {
        export.ObligorStart.Number =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 10, 10);
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "OBLIGOR_E:");

      if (local.Position.Count > 0)
      {
        export.ObligorStop.Number =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 10, 10);
      }

      if (Lt(export.ObligorStop.Number, export.ObligorStart.Number))
      {
        local.CsePerson.Number = export.ObligorStart.Number;
        export.ObligorStart.Number = export.ObligorStop.Number;
        export.ObligorStop.Number = local.CsePerson.Number;
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DATE_S:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.CollectionDateStart.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 7, 2);
        local.CollectionDateStart.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 2);
        local.CollectionDateStart.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 11, 4);

        // mjr---> Validate local views
        if (Lt(local.CollectionDateStart.TextDateYyyy, "0001") || Lt
          ("2099", local.CollectionDateStart.TextDateYyyy))
        {
          goto Test1;
        }

        if (Lt(local.CollectionDateStart.TextDateMm, "01") || Lt
          ("12", local.CollectionDateStart.TextDateMm))
        {
          goto Test1;
        }

        if (Lt(local.CollectionDateStart.TestDateDd, "01") || Lt
          ("31", local.CollectionDateStart.TestDateDd))
        {
          goto Test1;
        }

        if (Lt("30", local.CollectionDateStart.TestDateDd) && (
          Equal(local.CollectionDateStart.TextDateMm, "04") || Equal
          (local.CollectionDateStart.TextDateMm, "06") || Equal
          (local.CollectionDateStart.TextDateMm, "09") || Equal
          (local.CollectionDateStart.TextDateMm, "11")))
        {
          goto Test1;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.CollectionDateStart.TestDateDd) && Equal
          (local.CollectionDateStart.TextDateMm, "02"))
        {
          local.Calc.Count =
            (int)StringToNumber(local.CollectionDateStart.TextDateYyyy);
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
        local.CollectionDateStart.TextDate =
          local.CollectionDateStart.TextDateYyyy + local
          .CollectionDateStart.TextDateMm + local
          .CollectionDateStart.TestDateDd;

        // mjr---> Construct ief date
        export.CollectionDateStart.Date =
          IntToDate((int)StringToNumber(local.CollectionDateStart.TextDate));
      }

Test1:

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "DATE_E:");

      if (local.Position.Count > 0)
      {
        // mjr---> Retrieve parameter into local views
        local.CollectionDateStop.TextDateMm =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 7, 2);
        local.CollectionDateStop.TestDateDd =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 9, 2);
        local.CollectionDateStop.TextDateYyyy =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.Position.Count + 11, 4);

        // mjr---> Validate local views
        if (Lt(local.CollectionDateStop.TextDateYyyy, "0001") || Lt
          ("2099", local.CollectionDateStop.TextDateYyyy))
        {
          goto Test2;
        }

        if (Lt(local.CollectionDateStop.TextDateMm, "01") || Lt
          ("12", local.CollectionDateStop.TextDateMm))
        {
          goto Test2;
        }

        if (Lt(local.CollectionDateStop.TestDateDd, "01") || Lt
          ("31", local.CollectionDateStop.TestDateDd))
        {
          goto Test2;
        }

        if (Lt("30", local.CollectionDateStop.TestDateDd) && (
          Equal(local.CollectionDateStop.TextDateMm, "04") || Equal
          (local.CollectionDateStop.TextDateMm, "06") || Equal
          (local.CollectionDateStop.TextDateMm, "09") || Equal
          (local.CollectionDateStop.TextDateMm, "11")))
        {
          goto Test2;
        }

        // mjr--->  LEAP YEAR check
        if (Lt("28", local.CollectionDateStop.TestDateDd) && Equal
          (local.CollectionDateStop.TextDateMm, "02"))
        {
          local.Calc.Count =
            (int)StringToNumber(local.CollectionDateStop.TextDateYyyy);
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
        local.CollectionDateStop.TextDate =
          local.CollectionDateStop.TextDateYyyy + local
          .CollectionDateStop.TextDateMm + local.CollectionDateStop.TestDateDd;

        // mjr---> Construct ief date
        export.CollectionDateStop.Date =
          IntToDate((int)StringToNumber(local.CollectionDateStop.TextDate));
      }

Test2:

      if (Lt(export.CollectionDateStop.Date, export.CollectionDateStart.Date))
      {
        local.DateWorkArea.Date = export.CollectionDateStart.Date;
        export.CollectionDateStart.Date = export.CollectionDateStop.Date;
        export.CollectionDateStop.Date = local.DateWorkArea.Date;
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "ADJUSTED");

      if (local.Position.Count > 0)
      {
        export.AdjustedInd.Flag = "Y";
      }

      local.Position.Count =
        Find(local.ProgramProcessingInfo.ParameterList, "ERRORS");

      if (local.Position.Count > 0)
      {
        export.ErrorsOnly.Flag = "Y";
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

    // ------------------------------------------------------------
    // OPEN OUTPUT REPORT 01
    // ------------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

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

    local.EabReportSend.RptDetail = "OBLIGOR_START:  " + export
      .ObligorStart.Number;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "OBLIGOR_STOP:   " + export
      .ObligorStop.Number;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DATE_START:     " + local
      .CollectionDateStart.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "DATE_STOP:      " + local
      .CollectionDateStop.TextDate;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "ADJUSTED:       " + export
      .AdjustedInd.Flag;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "ERRORS ONLY:    " + export.ErrorsOnly.Flag;
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

      return;
    }

    // -----------------------------------------------------------
    // WRITE INITIAL LINES TO REPORT 01
    // -----------------------------------------------------------
    local.EabReportSend.RptDetail = "";
    UseCabBusinessReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
    }

    // -----------------------------------------------------------
    // GET LITERALS
    // -----------------------------------------------------------
  }

  private static void MoveEabReportSend1(EabReportSend source,
    EabReportSend target)
  {
    target.BlankLineAfterColHead = source.BlankLineAfterColHead;
    target.NumberOfColHeadings = source.NumberOfColHeadings;
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
    target.ColHeading1 = source.ColHeading1;
    target.ColHeading2 = source.ColHeading2;
    target.ColHeading3 = source.ColHeading3;
  }

  private static void MoveEabReportSend2(EabReportSend source,
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

  private void UseCabBusinessReport1()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend1(local.EabReportSend, useImport.NeededToOpen);

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabBusinessReport2()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend2(local.EabReportSend, useImport.NeededToOpen);

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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ErrorsOnly.
    /// </summary>
    [JsonPropertyName("errorsOnly")]
    public Common ErrorsOnly
    {
      get => errorsOnly ??= new();
      set => errorsOnly = value;
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
    /// A value of CollectionDateStart.
    /// </summary>
    [JsonPropertyName("collectionDateStart")]
    public DateWorkArea CollectionDateStart
    {
      get => collectionDateStart ??= new();
      set => collectionDateStart = value;
    }

    /// <summary>
    /// A value of CollectionDateStop.
    /// </summary>
    [JsonPropertyName("collectionDateStop")]
    public DateWorkArea CollectionDateStop
    {
      get => collectionDateStop ??= new();
      set => collectionDateStop = value;
    }

    /// <summary>
    /// A value of ObligorStop.
    /// </summary>
    [JsonPropertyName("obligorStop")]
    public CsePerson ObligorStop
    {
      get => obligorStop ??= new();
      set => obligorStop = value;
    }

    /// <summary>
    /// A value of ObligorStart.
    /// </summary>
    [JsonPropertyName("obligorStart")]
    public CsePerson ObligorStart
    {
      get => obligorStart ??= new();
      set => obligorStart = value;
    }

    /// <summary>
    /// A value of AdjustedInd.
    /// </summary>
    [JsonPropertyName("adjustedInd")]
    public Common AdjustedInd
    {
      get => adjustedInd ??= new();
      set => adjustedInd = value;
    }

    private ProgramCheckpointRestart programCheckpointRestart;
    private Common errorsOnly;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea collectionDateStart;
    private DateWorkArea collectionDateStop;
    private CsePerson obligorStop;
    private CsePerson obligorStart;
    private Common adjustedInd;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of CollectionDateStart.
    /// </summary>
    [JsonPropertyName("collectionDateStart")]
    public BatchTimestampWorkArea CollectionDateStart
    {
      get => collectionDateStart ??= new();
      set => collectionDateStart = value;
    }

    /// <summary>
    /// A value of CollectionDateStop.
    /// </summary>
    [JsonPropertyName("collectionDateStop")]
    public BatchTimestampWorkArea CollectionDateStop
    {
      get => collectionDateStop ??= new();
      set => collectionDateStop = value;
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
    private CsePerson csePerson;
    private WorkArea workArea;
    private Common position;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private BatchTimestampWorkArea collectionDateStart;
    private BatchTimestampWorkArea collectionDateStop;
    private Common calc;
  }
#endregion
}

// Program: CSENET_30_DAY_REPORT, ID: 372944440, model: 746.
// Short name: SWEI730B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CSENET_30_DAY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class Csenet30DayReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CSENET_30_DAY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Csenet30DayReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Csenet30DayReport.
  /// </summary>
  public Csenet30DayReport(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    // *
    // 
    // *
    // * C H A N G E   L O G
    // 
    // *
    // * ===================
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    // *
    // 
    // *
    // *   Date   Name      PR#  Reason
    // 
    // *
    // *   ----   ----      ---  ------
    // 
    // *
    // * Sept 99                 
    // Production
    // 
    // *
    // *
    // 
    // *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.PpiFound.Flag = "N";

    // ***
    // *** get each Program Processing Info for SWEIB730
    // ***
    if (ReadProgramProcessingInfo())
    {
      local.PpiFound.Flag = "Y";
    }

    // ***
    // *** OPEN the Error Report
    // ***
    export.EabFileHandling.Action = "OPEN";
    export.NeededToOpen.ProgramName = "SWEIB730";
    export.NeededToOpen.ProcessDate =
      entities.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    if (AsChar(local.PpiFound.Flag) == 'N')
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "Program Processing Info not found";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** OPEN the CSENET 30 DAY extract file
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayFileReader2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        "Error Opening the CSENET 30 Day Extract File";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** OPEN the CSENET 30 DAY report
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayReport2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "Error Opening the CSENET 30 Day Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    local.Eof.Flag = "N";

    while(AsChar(local.Eof.Flag) == 'N')
    {
      // ***
      // *** READ the CSENET 30 DAY extract file
      // ***
      export.ReportParms.Parm1 = "GR";
      export.ReportParms.Parm2 = "";
      UseEabCsenet30DayFileReader1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        if (Equal(local.ReportParms.Parm1, "EF"))
        {
          local.Eof.Flag = "Y";

          continue;
        }

        ExitState = "ERROR_READING_FILE_AB";

        // ***
        // *** WRITE to the Error Report
        // ***
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail =
          "Error Reading the CSENET 30 Day Extract File";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

        // ***
        // *** CLOSE the Error Report
        // ***
        export.EabFileHandling.Action = "CLOSE";
        UseCabErrorReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_CLOSING_FILE_AB";
        }

        return;
      }

      // ***
      // *** WRITE to the CSENET 30 DAY report
      // ***
      export.ReportParms.Parm1 = "GR";
      export.ReportParms.Parm2 = "";
      UseEabCsenet30DayReport1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        // ***
        // *** WRITE to the Error Report
        // ***
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail =
          "Error Writing to the CSENET 30 Day Report";
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

        // ***
        // *** CLOSE the Error Report
        // ***
        export.EabFileHandling.Action = "CLOSE";
        UseCabErrorReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_CLOSING_FILE_AB";
        }

        return;
      }
    }

    // ***
    // *** CLOSE the CSENET 30 DAY extract file
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayFileReader2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail =
        "Error Closing the CSENET 30 Day Extract File";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** CLOSE the CSENET 30 DAY report
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet30DayReport2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      export.EabFileHandling.Action = "WRITE";
      export.NeededToWrite.RptDetail = "Error Closing the CSENET 30 Day Report";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      export.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_CLOSING_FILE_AB";
      }

      return;
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    export.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveReportParms(ReportParms source, ReportParms target)
  {
    target.Parm1 = source.Parm1;
    target.Parm2 = source.Parm2;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(export.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCsenet30DayFileReader1()
  {
    var useImport = new EabCsenet30DayFileReader.Import();
    var useExport = new EabCsenet30DayFileReader.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);
    useExport.Csenet30DayExtract.Assign(export.Csenet30DayExtract);

    Call(EabCsenet30DayFileReader.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
    export.Csenet30DayExtract.Assign(useExport.Csenet30DayExtract);
  }

  private void UseEabCsenet30DayFileReader2()
  {
    var useImport = new EabCsenet30DayFileReader.Import();
    var useExport = new EabCsenet30DayFileReader.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet30DayFileReader.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCsenet30DayReport1()
  {
    var useImport = new EabCsenet30DayReport.Import();
    var useExport = new EabCsenet30DayReport.Export();

    useImport.Csenet30DayExtract.Assign(export.Csenet30DayExtract);
    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet30DayReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCsenet30DayReport2()
  {
    var useImport = new EabCsenet30DayReport.Import();
    var useExport = new EabCsenet30DayReport.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet30DayReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ProcessDate =
          db.GetNullableDate(reader, 2);
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
    /// A value of Csenet30DayExtract.
    /// </summary>
    [JsonPropertyName("csenet30DayExtract")]
    public Csenet30DayExtract2 Csenet30DayExtract
    {
      get => csenet30DayExtract ??= new();
      set => csenet30DayExtract = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private Csenet30DayExtract2 csenet30DayExtract;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Eof.
    /// </summary>
    [JsonPropertyName("eof")]
    public Common Eof
    {
      get => eof ??= new();
      set => eof = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
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

    private Common eof;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
    private Common ppiFound;
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

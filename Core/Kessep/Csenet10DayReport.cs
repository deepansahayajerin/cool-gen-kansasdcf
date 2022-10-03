// Program: CSENET_10_DAY_REPORT, ID: 372943169, model: 746.
// Short name: SWEI710B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CSENET_10_DAY_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class Csenet10DayReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CSENET_10_DAY_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new Csenet10DayReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of Csenet10DayReport.
  /// </summary>
  public Csenet10DayReport(IContext context, Import import, Export export):
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
    // *
    // 
    // *
    // *******************************************************************
    // *   Date   Name      PR#  Reason
    // 
    // *
    // *   ----   ----      ---  -------
    // 
    // *
    // * Sept 99                 
    // Production
    // 
    // *
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.PpiFound.Flag = "N";
    local.NeededToOpen.ProgramName = "SWEIB710";

    // ***
    // *** get each Program Processing Info for SWEIB710
    // ***
    if (ReadProgramProcessingInfo())
    {
      local.PpiFound.Flag = "Y";
      local.NeededToOpen.ProcessDate =
        entities.ProgramProcessingInfo.ProcessDate;
    }

    // ***
    // *** OPEN the Error Report
    // ***
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";

      return;
    }

    if (AsChar(local.PpiFound.Flag) == 'N')
    {
      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Program Processing Info not found.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        goto Test;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
      }

      return;
    }

Test:

    // ***
    // *** OPEN the sorted CSENET 10 DAY extract file
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";
    UseEabCsenetFileReader2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error opening the sorted CSENET extract file";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
      }

      return;
    }

    // ***
    // *** OPEN the CSENET 10 DAY report
    // ***
    export.ReportParms.Parm1 = "OF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet10DayReport2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ACO_RC_AB0001_ERROR_ON_OPEN_FILE";

      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error opening the CSENET report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
      }

      return;
    }

    local.Eof.Flag = "N";

    // ****** =============================== ******
    // ***
    // 
    // ***
    // **** ****         MAIN LOOP         **** ****
    // ***
    // 
    // ***
    // ****** =============================== ******
    while(AsChar(local.Eof.Flag) == 'N')
    {
      // ***
      // *** READ the sorted CSENET 10 DAY extract file
      // ***
      export.ReportParms.Parm1 = "GR";
      export.ReportParms.Parm2 = "";
      UseEabCsenetFileReader1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        if (Equal(local.ReportParms.Parm1, "EF"))
        {
          local.Eof.Flag = "Y";

          continue;
        }

        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error reading the sorted CSENET extract file";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

        // ***
        // *** CLOSE the Error Report
        // ***
        local.EabFileHandling.Action = "CLOSE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
        }

        return;
      }

      // ***
      // *** WRITE to the CSENET 10 DAY report
      // ***
      export.ReportParms.Parm1 = "GR";
      export.ReportParms.Parm2 = "";
      UseEabCsenet10DayReport1();

      if (!IsEmpty(local.ReportParms.Parm1))
      {
        // ***
        // *** WRITE to the Error Report
        // ***
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Error writing to the CSENET report";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          return;
        }

        // ***
        // *** CLOSE the Error Report
        // ***
        local.EabFileHandling.Action = "CLOSE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
        }

        return;
      }
    }

    // ***
    // *** CLOSE the sorted CSENET 10 DAY extract file
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabCsenetFileReader2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error closing the sorted CSENET extract file";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
      }

      return;
    }

    // ***
    // *** CLOSE the CSENET 10 DAY report
    // ***
    export.ReportParms.Parm1 = "CF";
    export.ReportParms.Parm2 = "";
    UseEabCsenet10DayReport2();

    if (!IsEmpty(local.ReportParms.Parm1))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      // ***
      // *** WRITE to the Error Report
      // ***
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Error closing the CSENET report";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ERROR_WRITING_TO_REPORT_AB";

        return;
      }

      // ***
      // *** CLOSE the Error Report
      // ***
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
      }

      return;
    }

    // ***
    // *** CLOSE the Error Report
    // ***
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_RE0000_ERR_OPNG_ERROR_RPT_A";
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCsenet10DayReport1()
  {
    var useImport = new EabCsenet10DayReport.Import();
    var useExport = new EabCsenet10DayReport.Export();

    useImport.Csenet10DayExtract.Assign(export.Csenet10DayExtract);
    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet10DayReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCsenet10DayReport2()
  {
    var useImport = new EabCsenet10DayReport.Import();
    var useExport = new EabCsenet10DayReport.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenet10DayReport.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
  }

  private void UseEabCsenetFileReader1()
  {
    var useImport = new EabCsenetFileReader.Import();
    var useExport = new EabCsenetFileReader.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);
    useExport.Csenet10DayExtract.Assign(export.Csenet10DayExtract);

    Call(EabCsenetFileReader.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.ReportParms);
    export.Csenet10DayExtract.Assign(useExport.Csenet10DayExtract);
  }

  private void UseEabCsenetFileReader2()
  {
    var useImport = new EabCsenetFileReader.Import();
    var useExport = new EabCsenetFileReader.Export();

    MoveReportParms(export.ReportParms, useImport.ReportParms);
    MoveReportParms(local.ReportParms, useExport.ReportParms);

    Call(EabCsenetFileReader.Execute, useImport, useExport);

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
    /// A value of Csenet10DayExtract.
    /// </summary>
    [JsonPropertyName("csenet10DayExtract")]
    public Csenet10DayExtract2 Csenet10DayExtract
    {
      get => csenet10DayExtract ??= new();
      set => csenet10DayExtract = value;
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

    private Csenet10DayExtract2 csenet10DayExtract;
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

    private Common eof;
    private ReportParms reportParms;
    private Common ppiFound;
    private EabReportSend neededToWrite;
    private EabReportSend neededToOpen;
    private EabFileHandling eabFileHandling;
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

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateCaseAssignment.
    /// </summary>
    [JsonPropertyName("interstateCaseAssignment")]
    public InterstateCaseAssignment InterstateCaseAssignment
    {
      get => interstateCaseAssignment ??= new();
      set => interstateCaseAssignment = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private InterstateCase interstateCase;
    private InterstateCaseAssignment interstateCaseAssignment;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
  }
#endregion
}

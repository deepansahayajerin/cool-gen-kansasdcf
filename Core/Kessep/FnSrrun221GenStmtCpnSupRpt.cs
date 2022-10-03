// Program: FN_SRRUN221_GEN_STMT_CPN_SUP_RPT, ID: 372800856, model: 746.
// Short name: SWEF221B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_SRRUN221_GEN_STMT_CPN_SUP_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnSrrun221GenStmtCpnSupRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_SRRUN221_GEN_STMT_CPN_SUP_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnSrrun221GenStmtCpnSupRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnSrrun221GenStmtCpnSupRpt.
  /// </summary>
  public FnSrrun221GenStmtCpnSupRpt(IContext context, Import import,
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
    // THIS IS THE REPORT GENERATE PROGRAM FOR THE OBLIGATION SUPPRESSION
    // REPORT BY WORKER (job SRRUN220).
    // This job uses 3 Report Composer EABs: a file generator (SWEFB220), a
    // file reader, and a report generator.  If you change any eab view, you 
    // must
    // make the same change in the other 2 eabs, regenerate the stubs (on the
    // workstation, in Cobol), and regenerate the eab source code in Report 
    // Composer for each eab.
    ExitState = "ACO_NN0000_ALL_OK";
    local.NeededToOpen.ProgramName = "SWEFB220";
    local.ProgramProcessingInfo.Name = "SWEFB220";
    local.Current.Date = Now().Date;
    local.CanamSend.Parm2 = "";

    // : Open Error File
    local.EabFileHandling.Action = "OPEN";
    local.NeededToOpen.ProcessDate = local.Current.Date;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseReadProgramProcessingInfo();

    // : Check exitstate from read program process cab.
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = local.ExitStateWorkArea.Message;
      local.EabFileHandling.Action = "WRITE";
      UseCabErrorReport3();
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Month.Month = Month(local.ProgramProcessingInfo.ProcessDate);

    switch(local.Month.Month)
    {
      case 1:
        local.ReportMonth.Text10 = "January";

        break;
      case 2:
        local.ReportMonth.Text10 = "February";

        break;
      case 3:
        local.ReportMonth.Text10 = "March";

        break;
      case 4:
        local.ReportMonth.Text10 = "April";

        break;
      case 5:
        local.ReportMonth.Text10 = "May";

        break;
      case 6:
        local.ReportMonth.Text10 = "June";

        break;
      case 7:
        local.ReportMonth.Text10 = "July";

        break;
      case 8:
        local.ReportMonth.Text10 = "August";

        break;
      case 9:
        local.ReportMonth.Text10 = "September";

        break;
      case 10:
        local.ReportMonth.Text10 = "October";

        break;
      case 11:
        local.ReportMonth.Text10 = "November";

        break;
      case 12:
        local.ReportMonth.Text10 = "December";

        break;
      default:
        local.ReportMonth.Text10 = "Invalid";

        break;
    }

    // : Open the CANAM input data file.
    local.CanamSend.Parm1 = "OF";
    UseFnEabProduceSrrun220Rpt2();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // Parm 2 contains the file status
      // : output error here
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem opening report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // : Open the CANAM input file
    local.CanamSend.Parm1 = "OF";
    UseFnEabReadSrrun220ReportFile2();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // Parm 2 contains the file status
      // : output error here
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem opening report input file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport4();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.CanamSend.Parm1 = "GR";
    local.CanamReturn.Parm1 = "";

    while(IsEmpty(local.CanamReturn.Parm1))
    {
      UseFnEabReadSrrun220ReportFile1();

      if (!IsEmpty(local.CanamReturn.Parm1))
      {
        if (Equal(local.CanamReturn.Parm1, "EF"))
        {
          // : End of File
          break;
        }

        // Parm 2 contains the file status
        // :output error here
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Problem reading report file.  File status is  " + local
          .CanamReturn.Parm2;
        UseCabErrorReport4();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      UseFnEabProduceSrrun220Rpt1();

      if (!IsEmpty(local.CanamReturn.Parm1))
      {
        // Parm 2 contains the file status
        // :output error here
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Problem writing report.  File status is  " + local
          .CanamReturn.Parm2;
        UseCabErrorReport4();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }
    }

    // : Close the CANAM output data file.
    local.CanamSend.Parm1 = "CF";
    UseFnEabReadSrrun220ReportFile2();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // : Error
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem closing report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport4();
    }

    // : Close the CANAM report file.
    local.CanamSend.Parm1 = "CF";
    UseFnEabProduceSrrun220Rpt3();

    if (!IsEmpty(local.CanamReturn.Parm1))
    {
      // : Error
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Problem closing report file.  File status is  " + local
        .CanamReturn.Parm2;
      UseCabErrorReport4();
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MoveStmtCouponSuppStatusHist(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.CreatedBy = source.CreatedBy;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport4()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseFnEabProduceSrrun220Rpt1()
  {
    var useImport = new FnEabProduceSrrun220Rpt.Import();
    var useExport = new FnEabProduceSrrun220Rpt.Export();

    useImport.ReportMonth.Text10 = local.ReportMonth.Text10;
    useImport.Office.Name = local.Office.Name;
    useImport.Supervisor.Assign(local.Supervisor);
    useImport.ServiceProvider.Assign(local.ServiceProvider);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
    useImport.SuppressType.Text10 = local.SuppressType.Text10;
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    MoveStmtCouponSuppStatusHist(local.StmtCouponSuppStatusHist,
      useImport.StmtCouponSuppStatusHist);
    useImport.SuppressReason.Text30 = local.SuppressReason.Text30;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabProduceSrrun220Rpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnEabProduceSrrun220Rpt2()
  {
    var useImport = new FnEabProduceSrrun220Rpt.Import();
    var useExport = new FnEabProduceSrrun220Rpt.Export();

    useImport.ReportMonth.Text10 = local.ReportMonth.Text10;
    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabProduceSrrun220Rpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnEabProduceSrrun220Rpt3()
  {
    var useImport = new FnEabProduceSrrun220Rpt.Import();
    var useExport = new FnEabProduceSrrun220Rpt.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabProduceSrrun220Rpt.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnEabReadSrrun220ReportFile1()
  {
    var useImport = new FnEabReadSrrun220ReportFile.Import();
    var useExport = new FnEabReadSrrun220ReportFile.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    useExport.Office.Name = local.Office.Name;
    useExport.Supervisor.Assign(local.Supervisor);
    useExport.ServiceProvider.Assign(local.ServiceProvider);
    MoveCsePersonsWorkSet(local.CsePersonsWorkSet, useExport.CsePersonsWorkSet);
    useExport.SuppressType.Text10 = local.SuppressType.Text10;
    useExport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;
    MoveStmtCouponSuppStatusHist(local.StmtCouponSuppStatusHist,
      useExport.StmtCouponSuppStatusHist);
    useExport.SuppressReason.Text30 = local.SuppressReason.Text30;
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabReadSrrun220ReportFile.Execute, useImport, useExport);

    local.Office.Name = useExport.Office.Name;
    local.Supervisor.Assign(useExport.Supervisor);
    local.ServiceProvider.Assign(useExport.ServiceProvider);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
    local.SuppressType.Text10 = useExport.SuppressType.Text10;
    local.LegalAction.StandardNumber = useExport.LegalAction.StandardNumber;
    MoveStmtCouponSuppStatusHist(useExport.StmtCouponSuppStatusHist,
      local.StmtCouponSuppStatusHist);
    local.SuppressReason.Text30 = useExport.SuppressReason.Text30;
    MoveReportParms(useExport.ReportParms, local.CanamReturn);
  }

  private void UseFnEabReadSrrun220ReportFile2()
  {
    var useImport = new FnEabReadSrrun220ReportFile.Import();
    var useExport = new FnEabReadSrrun220ReportFile.Export();

    MoveReportParms(local.CanamSend, useImport.ReportParms);
    MoveReportParms(local.CanamReturn, useExport.ReportParms);

    Call(FnEabReadSrrun220ReportFile.Execute, useImport, useExport);

    MoveReportParms(useExport.ReportParms, local.CanamReturn);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of LoopControl.
    /// </summary>
    [JsonPropertyName("loopControl")]
    public Common LoopControl
    {
      get => loopControl ??= new();
      set => loopControl = value;
    }

    /// <summary>
    /// A value of SuppressReason.
    /// </summary>
    [JsonPropertyName("suppressReason")]
    public TextWorkArea SuppressReason
    {
      get => suppressReason ??= new();
      set => suppressReason = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    /// <summary>
    /// A value of ReportMonth.
    /// </summary>
    [JsonPropertyName("reportMonth")]
    public TextWorkArea ReportMonth
    {
      get => reportMonth ??= new();
      set => reportMonth = value;
    }

    /// <summary>
    /// A value of SuppressType.
    /// </summary>
    [JsonPropertyName("suppressType")]
    public TextWorkArea SuppressType
    {
      get => suppressType ??= new();
      set => suppressType = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CurrentTimestamp.
    /// </summary>
    [JsonPropertyName("currentTimestamp")]
    public DateWorkArea CurrentTimestamp
    {
      get => currentTimestamp ??= new();
      set => currentTimestamp = value;
    }

    /// <summary>
    /// A value of CanamSend.
    /// </summary>
    [JsonPropertyName("canamSend")]
    public ReportParms CanamSend
    {
      get => canamSend ??= new();
      set => canamSend = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of CanamReturn.
    /// </summary>
    [JsonPropertyName("canamReturn")]
    public ReportParms CanamReturn
    {
      get => canamReturn ??= new();
      set => canamReturn = value;
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
    /// A value of Month.
    /// </summary>
    [JsonPropertyName("month")]
    public DateWorkArea Month
    {
      get => month ??= new();
      set => month = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    private Common loopControl;
    private TextWorkArea suppressReason;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Office office;
    private ServiceProvider serviceProvider;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private TextWorkArea reportMonth;
    private TextWorkArea suppressType;
    private ServiceProvider supervisor;
    private LegalAction legalAction;
    private EabReportSend neededToOpen;
    private DateWorkArea current;
    private DateWorkArea currentTimestamp;
    private ReportParms canamSend;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ReportParms canamReturn;
    private DateWorkArea null1;
    private DateWorkArea month;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private ServiceProvider serviceProvider;
  }
#endregion
}

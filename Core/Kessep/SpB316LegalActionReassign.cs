// Program: SP_B316_LEGAL_ACTION_REASSIGN, ID: 374563251, model: 746.
// Short name: SWEP316B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B316_LEGAL_ACTION_REASSIGN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB316LegalActionReassign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B316_LEGAL_ACTION_REASSIGN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB316LegalActionReassign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB316LegalActionReassign.
  /// </summary>
  public SpB316LegalActionReassign(IContext context, Import import,
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
    // *************************************************************************
    // This program reads an input data set containing a 'to' and 'from' pair of
    // service providers.
    // It passes those providers to SP_B316_REASSIGN_LEGAL_ACTIONS
    // which then reassigns the legal actions to the 'to' service provider.  The
    // previous
    // legal actions are determined by finding legal referrals currently 
    // assigned to
    // the 'from' service provider.
    // *************************************************************************
    // ------------------------------------------------------------------
    //                   M A I N T E N A N C E   L O G
    // Date		Developer	Request #	Description
    // 07/14/10	J Huss		CQ# 20646	Initial development.
    // ------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.InputCommon.Count = 0;
    local.Report.Action = "WRITE";
    local.InputEabFileHandling.Action = "READ";
    UseSpB316Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Read the first input record from the file
    UseSpB316EabReadInputFile();

    while(Equal(local.InputEabFileHandling.Status, "OK"))
    {
      ++local.InputCommon.Count;
      local.EabReportSend.RptDetail = "Processing input record number " + NumberToString
        (local.InputCommon.Count, 11, 5);
      UseCabControlReport();

      if (!Equal(local.Report.Status, "OK"))
      {
        local.Report.Status = "ERROR";

        break;
      }

      // Reassign legal actions to the 'to' service provider.
      UseSpB316ReassignLegalAction();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.Report.Status = "ERROR";

        break;
      }

      // Write totals to control report
      for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Total Monitored Activity reassignments: " + NumberToString
              (local.MonitoredActivity.Count, 6, 10);

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "Total Service Provider reassignments:   " + NumberToString
              (local.LegalActionAssignment.Count, 6, 10);

            break;
          default:
            break;
        }

        UseCabControlReport();

        if (!Equal(local.Report.Status, "OK"))
        {
          local.Report.Status = "ERROR";
          ExitState = "ERROR_WRITING_TO_REPORT_AB";

          goto AfterCycle;
        }
      }

      local.InputEabFileHandling.Action = "READ";

      // Read the next input record in the file
      UseSpB316EabReadInputFile();
    }

AfterCycle:

    local.EabReportSend.RptDetail = "";

    if (Equal(local.InputEabFileHandling.Status, "FILE ERR"))
    {
      local.EabReportSend.RptDetail = local.External.TextLine130;
    }
    else if (Equal(local.Report.Status, "ERROR"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
    }
    else if (Equal(local.InputEabFileHandling.Status, "EOF"))
    {
      // This is ok
    }
    else
    {
      // An unexpected result has occurred.
      local.EabReportSend.RptDetail =
        "An unknown file error has occurred, file status is: " + local
        .Report.Status;
    }

    UseSpB316CloseReports();
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Report.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Report.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseSpB316CloseReports()
  {
    var useImport = new SpB316CloseReports.Import();
    var useExport = new SpB316CloseReports.Export();

    useImport.Error.RptDetail = local.EabReportSend.RptDetail;

    Call(SpB316CloseReports.Execute, useImport, useExport);
  }

  private void UseSpB316EabReadInputFile()
  {
    var useImport = new SpB316EabReadInputFile.Import();
    var useExport = new SpB316EabReadInputFile.Export();

    useImport.EabFileHandling.Action = local.InputEabFileHandling.Action;
    useExport.ToOffice.SystemGeneratedId = local.ToOffice.SystemGeneratedId;
    useExport.FromOffice.SystemGeneratedId = local.FromOffice.SystemGeneratedId;
    useExport.ToServiceProvider.SystemGeneratedId =
      local.ToServiceProvider.SystemGeneratedId;
    useExport.FromServiceProvider.SystemGeneratedId =
      local.FromServiceProvider.SystemGeneratedId;
    MoveEabFileHandling(local.InputEabFileHandling, useExport.EabFileHandling);
    useExport.FromOfficeServiceProvider.RoleCode =
      local.FromOfficeServiceProvider.RoleCode;
    useExport.ResultDetail.TextLine130 = local.External.TextLine130;
    useExport.ToOfficeServiceProvider.RoleCode =
      local.ToOfficeServiceProvider.RoleCode;
    useExport.Tribunal.Identifier = local.Tribunal.Identifier;

    Call(SpB316EabReadInputFile.Execute, useImport, useExport);

    local.ToOffice.SystemGeneratedId = useExport.ToOffice.SystemGeneratedId;
    local.FromOffice.SystemGeneratedId = useExport.FromOffice.SystemGeneratedId;
    local.ToServiceProvider.SystemGeneratedId =
      useExport.ToServiceProvider.SystemGeneratedId;
    local.FromServiceProvider.SystemGeneratedId =
      useExport.FromServiceProvider.SystemGeneratedId;
    MoveEabFileHandling(useExport.EabFileHandling, local.InputEabFileHandling);
    local.FromOfficeServiceProvider.RoleCode =
      useExport.FromOfficeServiceProvider.RoleCode;
    local.External.TextLine130 = useExport.ResultDetail.TextLine130;
    local.ToOfficeServiceProvider.RoleCode =
      useExport.ToOfficeServiceProvider.RoleCode;
    local.Tribunal.Identifier = useExport.Tribunal.Identifier;
  }

  private void UseSpB316Housekeeping()
  {
    var useImport = new SpB316Housekeeping.Import();
    var useExport = new SpB316Housekeeping.Export();

    Call(SpB316Housekeeping.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseSpB316ReassignLegalAction()
  {
    var useImport = new SpB316ReassignLegalAction.Import();
    var useExport = new SpB316ReassignLegalAction.Export();

    useImport.FromOffice.SystemGeneratedId = local.FromOffice.SystemGeneratedId;
    useImport.ToOffice.SystemGeneratedId = local.ToOffice.SystemGeneratedId;
    useImport.FromOfficeServiceProvider.RoleCode =
      local.FromOfficeServiceProvider.RoleCode;
    useImport.ToOfficeServiceProvider.RoleCode =
      local.ToOfficeServiceProvider.RoleCode;
    useImport.ProgramCheckpointRestart.UpdateFrequencyCount =
      local.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.FromServiceProvider.SystemGeneratedId =
      local.FromServiceProvider.SystemGeneratedId;
    useImport.ToServiceProvider.SystemGeneratedId =
      local.ToServiceProvider.SystemGeneratedId;
    useImport.Tribunal.Identifier = local.Tribunal.Identifier;

    Call(SpB316ReassignLegalAction.Execute, useImport, useExport);

    local.LegalActionAssignment.Count = useExport.LegalActionAssignment.Count;
    local.MonitoredActivity.Count = useExport.MonitoredActivityChange.Count;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of FromOffice.
    /// </summary>
    [JsonPropertyName("fromOffice")]
    public Office FromOffice
    {
      get => fromOffice ??= new();
      set => fromOffice = value;
    }

    /// <summary>
    /// A value of FromOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromOfficeServiceProvider")]
    public OfficeServiceProvider FromOfficeServiceProvider
    {
      get => fromOfficeServiceProvider ??= new();
      set => fromOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of FromServiceProvider.
    /// </summary>
    [JsonPropertyName("fromServiceProvider")]
    public ServiceProvider FromServiceProvider
    {
      get => fromServiceProvider ??= new();
      set => fromServiceProvider = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of InputEabFileHandling.
    /// </summary>
    [JsonPropertyName("inputEabFileHandling")]
    public EabFileHandling InputEabFileHandling
    {
      get => inputEabFileHandling ??= new();
      set => inputEabFileHandling = value;
    }

    /// <summary>
    /// A value of InputCommon.
    /// </summary>
    [JsonPropertyName("inputCommon")]
    public Common InputCommon
    {
      get => inputCommon ??= new();
      set => inputCommon = value;
    }

    /// <summary>
    /// A value of LegalActionAssignment.
    /// </summary>
    [JsonPropertyName("legalActionAssignment")]
    public Common LegalActionAssignment
    {
      get => legalActionAssignment ??= new();
      set => legalActionAssignment = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public Common MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Report.
    /// </summary>
    [JsonPropertyName("report")]
    public EabFileHandling Report
    {
      get => report ??= new();
      set => report = value;
    }

    /// <summary>
    /// A value of ToOffice.
    /// </summary>
    [JsonPropertyName("toOffice")]
    public Office ToOffice
    {
      get => toOffice ??= new();
      set => toOffice = value;
    }

    /// <summary>
    /// A value of ToOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("toOfficeServiceProvider")]
    public OfficeServiceProvider ToOfficeServiceProvider
    {
      get => toOfficeServiceProvider ??= new();
      set => toOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ToServiceProvider.
    /// </summary>
    [JsonPropertyName("toServiceProvider")]
    public ServiceProvider ToServiceProvider
    {
      get => toServiceProvider ??= new();
      set => toServiceProvider = value;
    }

    private Tribunal tribunal;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private External external;
    private Office fromOffice;
    private OfficeServiceProvider fromOfficeServiceProvider;
    private ServiceProvider fromServiceProvider;
    private Common common;
    private EabFileHandling inputEabFileHandling;
    private Common inputCommon;
    private Common legalActionAssignment;
    private Common monitoredActivity;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling report;
    private Office toOffice;
    private OfficeServiceProvider toOfficeServiceProvider;
    private ServiceProvider toServiceProvider;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalReferral1.
    /// </summary>
    [JsonPropertyName("legalReferral1")]
    public OfficeServiceProvider LegalReferral1
    {
      get => legalReferral1 ??= new();
      set => legalReferral1 = value;
    }

    /// <summary>
    /// A value of LegalReferral2.
    /// </summary>
    [JsonPropertyName("legalReferral2")]
    public LegalReferral LegalReferral2
    {
      get => legalReferral2 ??= new();
      set => legalReferral2 = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of LegalAction1.
    /// </summary>
    [JsonPropertyName("legalAction1")]
    public LegalAction LegalAction1
    {
      get => legalAction1 ??= new();
      set => legalAction1 = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
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
    /// A value of LegalActionAssigment.
    /// </summary>
    [JsonPropertyName("legalActionAssigment")]
    public LegalActionAssigment LegalActionAssigment
    {
      get => legalActionAssigment ??= new();
      set => legalActionAssigment = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of LegalAction2.
    /// </summary>
    [JsonPropertyName("legalAction2")]
    public OfficeServiceProvider LegalAction2
    {
      get => legalAction2 ??= new();
      set => legalAction2 = value;
    }

    private OfficeServiceProvider legalReferral1;
    private LegalReferral legalReferral2;
    private CaseRole caseRole;
    private Case1 case1;
    private LegalAction legalAction1;
    private ServiceProvider serviceProvider;
    private Office office;
    private Infrastructure infrastructure;
    private LegalActionAssigment legalActionAssigment;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalReferralAssignment legalReferralAssignment;
    private OfficeServiceProvider legalAction2;
  }
#endregion
}

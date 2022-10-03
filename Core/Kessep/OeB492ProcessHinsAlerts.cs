// Program: OE_B492_PROCESS_HINS_ALERTS, ID: 372869030, model: 746.
// Short name: SWEE492B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B492_PROCESS_HINS_ALERTS.
/// </para>
/// <para>
/// This program processes a sequential file from KMMIS.  External SWEXEE25 is 
/// used to read this file.  This program determines whether or not the input
/// record is an add, or an update to existing coverage.  If it is an update, it
/// writes details of the update to a report, but does not perform the update
/// to the database.  If it is determined to be new coverage, the database
/// create IS processed.
/// The KMMIS update report SWEXEE26 (developed using report composer) is an 
/// output of this program, as well as control and error reports.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB492ProcessHinsAlerts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B492_PROCESS_HINS_ALERTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB492ProcessHinsAlerts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB492ProcessHinsAlerts.
  /// </summary>
  public OeB492ProcessHinsAlerts(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *              M A I N T E N A N C E   L O G                          *
    // ***********************************************************************
    // *   Date      	Name		WR/PR		Description           *
    // ***********************************************************************
    // * 12/09/2002	Ed Lyman	WR020311	Initial Coding
    // * 05/17/2003	E.Shirk		WR20311		Minor fixes.
    // ***********************************************************************
    // ***********************************************************************
    // *   Process Notifications from EDS sent via AE                        *
    // *       Report all notifications for CSE (Rpt01)                      *
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseOeB492Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Status = "EOF";
    }

    while(!Equal(local.EabFileHandling.Status, "EOF"))
    {
      local.EabFileHandling.Action = "READ";
      UseEabReadHinsAlerts();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          ++local.RecordsRead.Count;

          break;
        case "EOF":
          if (local.RecordsRead.Count == 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "The input file is empty." + "";
            UseCabErrorReport();
            ExitState = "OE0000_ERROR_READING_EXT_FILE";
          }

          goto AfterCycle;
        default:
          local.NeededToWrite.RptDetail = "Error reading EDS file: " + local
            .EabFileHandling.Status;
          UseCabErrorReport();
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          goto AfterCycle;
      }

      switch(TrimEnd(local.MessageType.ActionEntry))
      {
        case "16":
          // ****************************************************************
          // *  Health Insurance Terminated
          // 
          // *
          // ****************************************************************
          break;
        case "21":
          // ****************************************************************
          // *  Claim adjustment
          // 
          // *
          // ****************************************************************
          break;
        case "22":
          // ****************************************************************
          // *  Claim has been recovered
          // 
          // *
          // ****************************************************************
          break;
        default:
          ++local.RecordsSkippedInvalid.Count;

          continue;
      }

      UseOeB492GetBeneficiaryInfo();

      // ********************************************************************************
      // *  Bypass bene if not found on our system. 
      // ********************************************************************************
      if (AsChar(local.PersonFound.Flag) == 'N')
      {
        ++local.RecordsSkippedNotFound.Count;
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Person " + local
          .CsePersonsWorkSet.Number + " not known to CSE.";
        UseCabErrorReport();

        continue;
      }

      // ********************************************************************************
      // *  Bypass bene if not on any open cases.  
      // ******************************************************************************
      if (AsChar(local.OpenCaseFound.Flag) == 'N')
      {
        ++local.RecordsSkippedNotFound.Count;
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "Person " + local
          .CsePersonsWorkSet.Number + " not on any open CSE cases.";
        UseCabErrorReport();

        continue;
      }

      switch(TrimEnd(local.MessageType.ActionEntry))
      {
        case "16":
          // ****************************************************************
          // *  Health Insurance Terminated
          // 
          // *
          // ****************************************************************
          ++local.PoliciesTerminated.Count;
          local.Infrastructure.ReasonCode = "HEALTHINSENDED";
          local.Infrastructure.Detail = "Health Insurance Policy terminated.";
          local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Number + "           " +
            "   Policy Terminated";

          break;
        case "21":
          // ****************************************************************
          // *  Claim adjustment
          // 
          // *
          // ****************************************************************
          ++local.ClaimsAdjusted.Count;
          local.Infrastructure.ReasonCode = "MEDCLAIMPDADJ";
          local.Infrastructure.Detail =
            "Claim Adjusted. Check MMIS birth exp case " + local
            .BirthExpenseCaseNum.Text9 + " Orig ICN " + local
            .OriginalIcn.Text13;
          local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Number + "   " +
            local.BirthExpenseCaseNum.Text9 + "  Claim Adjusted     " + local
            .AdjustedIcn.Text13 + "  " + local.OriginalIcn.Text13;

          break;
        case "22":
          // ****************************************************************
          // *  Claim has been recovered
          // 
          // *
          // ****************************************************************
          ++local.ClaimsPaid.Count;
          local.Infrastructure.ReasonCode = "MEDCLAIMPDADJ";
          local.Infrastructure.Detail =
            "Claim Recovered. Check MMIS birth exp case " + local
            .BirthExpenseCaseNum.Text9 + " Orig ICN " + local
            .OriginalIcn.Text13;
          local.NeededToWrite.RptDetail = local.CsePersonsWorkSet.Number + "   " +
            local.BirthExpenseCaseNum.Text9 + "  Claim Recovered    " + local
            .OriginalIcn.Text13 + "                 " + NumberToString
            ((long)(local.AmountRecovered.AverageCurrency * 100), 7, 9);

          break;
        default:
          ++local.RecordsSkippedInvalid.Count;
          local.NeededToWrite.RptDetail = "Message type is invalid: " + local
            .MessageType.ActionEntry;
          UseCabErrorReport();

          continue;
      }

      // ****************************************************************
      // *  Write details to the 01 report.
      // ****************************************************************
      local.EabFileHandling.Action = "WRITE";
      UseCabBusinessReport01();

      // ****************************************************************
      // *  Build relevant event.
      // ****************************************************************
      UseOeB492SendAlert();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      // *****************************************************************
      // This program will not do a checkpoint restart.  Simply
      // reprocess all the records.  Any previously processed
      // records will result in no change.
      // *****************************************************************
      ++local.CommitCount.Count;

      if (local.CommitCount.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.CommitCount.Count = 0;
      }
    }

AfterCycle:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeB492Close();
    }
    else
    {
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail = "Abend: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ALL_OK";
      UseOeB492Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ReasonCode = source.ReasonCode;
    target.Detail = source.Detail;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadHinsAlerts()
  {
    var useImport = new EabReadHinsAlerts.Import();
    var useExport = new EabReadHinsAlerts.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.TerminationDate.Date = local.TerminationDate.Date;
    useExport.HealthInsuranceCoverage.InsurancePolicyNumber =
      local.HealthInsuranceCoverage.InsurancePolicyNumber;
    useExport.AmountRecovered.AverageCurrency =
      local.AmountRecovered.AverageCurrency;
    useExport.AdjustedIcn.Text13 = local.AdjustedIcn.Text13;
    useExport.OriginalIcn.Text13 = local.OriginalIcn.Text13;
    useExport.BirthExpenseCaseNum.Text9 = local.BirthExpenseCaseNum.Text9;
    useExport.DateSentToState.Date = local.DateSentToState.Date;
    useExport.CaseNumber.Text8 = local.CaseNumber.Text8;
    useExport.BeneficiaryId.Number = local.CsePersonsWorkSet.Number;
    useExport.MessageType.ActionEntry = local.MessageType.ActionEntry;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabReadHinsAlerts.Execute, useImport, useExport);

    local.TerminationDate.Date = useExport.TerminationDate.Date;
    local.HealthInsuranceCoverage.InsurancePolicyNumber =
      useExport.HealthInsuranceCoverage.InsurancePolicyNumber;
    local.AmountRecovered.AverageCurrency =
      useExport.AmountRecovered.AverageCurrency;
    local.AdjustedIcn.Text13 = useExport.AdjustedIcn.Text13;
    local.OriginalIcn.Text13 = useExport.OriginalIcn.Text13;
    local.BirthExpenseCaseNum.Text9 = useExport.BirthExpenseCaseNum.Text9;
    local.DateSentToState.Date = useExport.DateSentToState.Date;
    local.CaseNumber.Text8 = useExport.CaseNumber.Text8;
    local.CsePersonsWorkSet.Number = useExport.BeneficiaryId.Number;
    local.MessageType.ActionEntry = useExport.MessageType.ActionEntry;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseOeB492Close()
  {
    var useImport = new OeB492Close.Import();
    var useExport = new OeB492Close.Export();

    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.RecordsSkippedInvalid.Count = local.RecordsSkippedInvalid.Count;
    useImport.RecordsSkippedNotFound.Count = local.RecordsSkippedNotFound.Count;
    useImport.PoliciesTerminated.Count = local.PoliciesTerminated.Count;
    useImport.ClaimsAdjusted.Count = local.ClaimsAdjusted.Count;
    useImport.ClaimsPaid.Count = local.ClaimsPaid.Count;
    useImport.AlertsFailed.Count = local.AlertsFailed.Count;
    useImport.AlertsSent.Count = local.AlertsSent.Count;

    Call(OeB492Close.Execute, useImport, useExport);
  }

  private void UseOeB492GetBeneficiaryInfo()
  {
    var useImport = new OeB492GetBeneficiaryInfo.Import();
    var useExport = new OeB492GetBeneficiaryInfo.Export();

    useImport.Beneficiary.Number = local.CsePersonsWorkSet.Number;

    Call(OeB492GetBeneficiaryInfo.Execute, useImport, useExport);

    local.PersonFound.Flag = useExport.PersonFound.Flag;
    local.OpenCaseFound.Flag = useExport.OpenCaseFound.Flag;
    local.CsePersonsWorkSet.Number = useExport.PolicyHolder.Number;
  }

  private void UseOeB492Housekeeping()
  {
    var useImport = new OeB492Housekeeping.Import();
    var useExport = new OeB492Housekeeping.Export();

    Call(OeB492Housekeeping.Execute, useImport, useExport);

    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseOeB492SendAlert()
  {
    var useImport = new OeB492SendAlert.Import();
    var useExport = new OeB492SendAlert.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.AlertsFailed.Count = local.AlertsFailed.Count;
    useImport.AlertsSent.Count = local.AlertsSent.Count;

    Call(OeB492SendAlert.Execute, useImport, useExport);

    local.AlertsFailed.Count = useExport.AlertsFailed.Count;
    local.AlertsSent.Count = useExport.AlertsSent.Count;
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
    /// <summary>
    /// A value of Beneficiary.
    /// </summary>
    [JsonPropertyName("beneficiary")]
    public CsePersonsWorkSet Beneficiary
    {
      get => beneficiary ??= new();
      set => beneficiary = value;
    }

    private CsePersonsWorkSet beneficiary;
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
    /// A value of TerminationDate.
    /// </summary>
    [JsonPropertyName("terminationDate")]
    public DateWorkArea TerminationDate
    {
      get => terminationDate ??= new();
      set => terminationDate = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of AmountRecovered.
    /// </summary>
    [JsonPropertyName("amountRecovered")]
    public Common AmountRecovered
    {
      get => amountRecovered ??= new();
      set => amountRecovered = value;
    }

    /// <summary>
    /// A value of AdjustedIcn.
    /// </summary>
    [JsonPropertyName("adjustedIcn")]
    public WorkArea AdjustedIcn
    {
      get => adjustedIcn ??= new();
      set => adjustedIcn = value;
    }

    /// <summary>
    /// A value of OriginalIcn.
    /// </summary>
    [JsonPropertyName("originalIcn")]
    public WorkArea OriginalIcn
    {
      get => originalIcn ??= new();
      set => originalIcn = value;
    }

    /// <summary>
    /// A value of BirthExpenseCaseNum.
    /// </summary>
    [JsonPropertyName("birthExpenseCaseNum")]
    public WorkArea BirthExpenseCaseNum
    {
      get => birthExpenseCaseNum ??= new();
      set => birthExpenseCaseNum = value;
    }

    /// <summary>
    /// A value of DateSentToState.
    /// </summary>
    [JsonPropertyName("dateSentToState")]
    public DateWorkArea DateSentToState
    {
      get => dateSentToState ??= new();
      set => dateSentToState = value;
    }

    /// <summary>
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    public WorkArea CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
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
    /// A value of PersonFound.
    /// </summary>
    [JsonPropertyName("personFound")]
    public Common PersonFound
    {
      get => personFound ??= new();
      set => personFound = value;
    }

    /// <summary>
    /// A value of OpenCaseFound.
    /// </summary>
    [JsonPropertyName("openCaseFound")]
    public Common OpenCaseFound
    {
      get => openCaseFound ??= new();
      set => openCaseFound = value;
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
    /// A value of MessageType.
    /// </summary>
    [JsonPropertyName("messageType")]
    public Common MessageType
    {
      get => messageType ??= new();
      set => messageType = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
    }

    /// <summary>
    /// A value of CommitCount.
    /// </summary>
    [JsonPropertyName("commitCount")]
    public Common CommitCount
    {
      get => commitCount ??= new();
      set => commitCount = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of RecordsSkippedInvalid.
    /// </summary>
    [JsonPropertyName("recordsSkippedInvalid")]
    public Common RecordsSkippedInvalid
    {
      get => recordsSkippedInvalid ??= new();
      set => recordsSkippedInvalid = value;
    }

    /// <summary>
    /// A value of RecordsSkippedNotFound.
    /// </summary>
    [JsonPropertyName("recordsSkippedNotFound")]
    public Common RecordsSkippedNotFound
    {
      get => recordsSkippedNotFound ??= new();
      set => recordsSkippedNotFound = value;
    }

    /// <summary>
    /// A value of PoliciesTerminated.
    /// </summary>
    [JsonPropertyName("policiesTerminated")]
    public Common PoliciesTerminated
    {
      get => policiesTerminated ??= new();
      set => policiesTerminated = value;
    }

    /// <summary>
    /// A value of ClaimsAdjusted.
    /// </summary>
    [JsonPropertyName("claimsAdjusted")]
    public Common ClaimsAdjusted
    {
      get => claimsAdjusted ??= new();
      set => claimsAdjusted = value;
    }

    /// <summary>
    /// A value of ClaimsPaid.
    /// </summary>
    [JsonPropertyName("claimsPaid")]
    public Common ClaimsPaid
    {
      get => claimsPaid ??= new();
      set => claimsPaid = value;
    }

    /// <summary>
    /// A value of AlertsFailed.
    /// </summary>
    [JsonPropertyName("alertsFailed")]
    public Common AlertsFailed
    {
      get => alertsFailed ??= new();
      set => alertsFailed = value;
    }

    /// <summary>
    /// A value of AlertsSent.
    /// </summary>
    [JsonPropertyName("alertsSent")]
    public Common AlertsSent
    {
      get => alertsSent ??= new();
      set => alertsSent = value;
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

    private DateWorkArea terminationDate;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Common amountRecovered;
    private WorkArea adjustedIcn;
    private WorkArea originalIcn;
    private WorkArea birthExpenseCaseNum;
    private DateWorkArea dateSentToState;
    private WorkArea caseNumber;
    private Infrastructure infrastructure;
    private Common personFound;
    private Common openCaseFound;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common messageType;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private DateWorkArea max;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabReportSend neededToWrite;
    private Common commitCount;
    private Common recordsRead;
    private Common recordsSkippedInvalid;
    private Common recordsSkippedNotFound;
    private Common policiesTerminated;
    private Common claimsAdjusted;
    private Common claimsPaid;
    private Common alertsFailed;
    private Common alertsSent;
    private ExitStateWorkArea exitStateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}

// Program: FN_B721_OCSE34_CSE_EXTRACT, ID: 371199434, model: 746.
// Short name: SWEB721P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_OCSE34_CSE_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB721Ocse34CseExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_OCSE34_CSE_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721Ocse34CseExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721Ocse34CseExtract.
  /// </summary>
  public FnB721Ocse34CseExtract(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // 03/10/04  EShirk	PR198543	Added two more items to the misc file 
    // processing.
    // 06/24/04  CM Johnson	PR207423	Added new module call for fdso suppressed 
    // disbursements.
    // 01/06/09  GVandy	CQ486		Add an audit trail to determine why part 1 and 
    // part 2 of the
    // 					OCSE34 report do not balance.
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";

    // --------------------------------------------------------------------------------------------
    // -- General housekeeping and initializations.
    // --------------------------------------------------------------------------------------------
    UseFnB721BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!Equal(ExitState, "ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization CAB Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE disbursement credit errors.
    // --------------------------------------------------------------------------------------------
    if (!Lt("01", local.RestartLine.Text2))
    {
      UseFnB721DisbursementCredits();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error Extracting CSE Disbursement Credits...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE disbursement debit errors.
    // --------------------------------------------------------------------------------------------
    if (!Lt("02", local.RestartLine.Text2))
    {
      UseFnB721DisbursementDebits();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error Extracting CSE Disbursement Debits...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE warrant errors.
    // --------------------------------------------------------------------------------------------
    if (!Lt("03", local.RestartLine.Text2))
    {
      UseFnB721Warrants();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error Extracting CSE Warrants...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE payment errors.
    // --------------------------------------------------------------------------------------------
    if (!Lt("04", local.RestartLine.Text2))
    {
      UseFnB721Payments();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Error Extracting CSE Payments...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE interstate payment errors.
    // --------------------------------------------------------------------------------------------
    if (!Lt("05", local.RestartLine.Text2))
    {
      UseFnB721InterstatePayments();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error Extracting CSE Interstate Payments...." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE suspended cash receipt details.
    // --------------------------------------------------------------------------------------------
    if (!Lt("06", local.RestartLine.Text2))
    {
      UseFnB721CrdSuspense();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error Extracting CSE Suspended Cash Receipt Details..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Extract CSE suppressed disbursements.
    // --------------------------------------------------------------------------------------------
    if (!Lt("07", local.RestartLine.Text2))
    {
      UseFnB721SuppressedDisbursements();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail =
          "Error Extracting CSE Suppressed Disbursements..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // --------------------------------------------------------------------------------------------
    // -- Put the timestamp of the OCSE34 record in PPI record for SWEFB722 (KPC
    // Extract Validation).
    // -- This is needed so that SWEFB722 will update the correct OCSE34 record.
    // --------------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.ParameterList =
      NumberToString(local.Ocse34.Period, 10, 6);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + " " + NumberToString
      (Year(local.Ocse34.CreatedTimestamp), 12, 4);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + "-";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Month(local.Ocse34.CreatedTimestamp), 14, 2);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + "-";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Day(local.Ocse34.CreatedTimestamp), 14, 2);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + "-";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Hour(local.Ocse34.CreatedTimestamp), 14, 2);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + ".";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Minute(local.Ocse34.CreatedTimestamp), 14, 2);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + ".";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Second(local.Ocse34.CreatedTimestamp), 14, 2);
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + ".";
    local.ProgramProcessingInfo.ParameterList =
      TrimEnd(local.ProgramProcessingInfo.ParameterList) + NumberToString
      (Microsecond(local.Ocse34.CreatedTimestamp), 10, 6);

    if (ReadProgramProcessingInfo())
    {
      try
      {
        UpdateProgramProcessingInfo();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "PROGRAM_PROCESSING_INFO_NU_AB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "PROGRAM_PROCESSING_INFO_PV_AB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail =
        "Error Updating PPI Record for SWEFB722..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Take a final Checkpoint
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInd = "N";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Error Taking Final Checkpoint..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Control report.
    // -------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_CTL_RPT_FILE";

      // Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close the Error report.
    // -------------------------------------------------------------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveOcse157Verification(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

  private void UseFnB721BatchInitialization()
  {
    var useImport = new FnB721BatchInitialization.Import();
    var useExport = new FnB721BatchInitialization.Export();

    Call(FnB721BatchInitialization.Execute, useImport, useExport);

    local.RestartLine.Text2 = useExport.RestartLine.Text2;
    MoveOcse34(useExport.Ocse34, local.Ocse34);
    MoveDateWorkArea(useExport.ReportPeriodEndDate, local.ReportPeriodEndDate);
    MoveOcse157Verification(useExport.Ocse157Verification,
      local.Ocse157Verification);
    local.Audit.Flag = useExport.Audit.Flag;
  }

  private void UseFnB721CrdSuspense()
  {
    var useImport = new FnB721CrdSuspense.Import();
    var useExport = new FnB721CrdSuspense.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721CrdSuspense.Execute, useImport, useExport);
  }

  private void UseFnB721DisbursementCredits()
  {
    var useImport = new FnB721DisbursementCredits.Import();
    var useExport = new FnB721DisbursementCredits.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721DisbursementCredits.Execute, useImport, useExport);
  }

  private void UseFnB721DisbursementDebits()
  {
    var useImport = new FnB721DisbursementDebits.Import();
    var useExport = new FnB721DisbursementDebits.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721DisbursementDebits.Execute, useImport, useExport);
  }

  private void UseFnB721InterstatePayments()
  {
    var useImport = new FnB721InterstatePayments.Import();
    var useExport = new FnB721InterstatePayments.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721InterstatePayments.Execute, useImport, useExport);
  }

  private void UseFnB721Payments()
  {
    var useImport = new FnB721Payments.Import();
    var useExport = new FnB721Payments.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721Payments.Execute, useImport, useExport);
  }

  private void UseFnB721SuppressedDisbursements()
  {
    var useImport = new FnB721SuppressedDisbursements.Import();
    var useExport = new FnB721SuppressedDisbursements.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721SuppressedDisbursements.Execute, useImport, useExport);
  }

  private void UseFnB721Warrants()
  {
    var useImport = new FnB721Warrants.Import();
    var useExport = new FnB721Warrants.Export();

    useImport.Ocse34.CreatedTimestamp = local.Ocse34.CreatedTimestamp;
    MoveDateWorkArea(local.ReportPeriodEndDate, useImport.ReportingPeriodEndDate);
      
    MoveOcse157Verification(local.Ocse157Verification,
      useImport.Ocse157Verification);
    useImport.Audit.Flag = local.Audit.Flag;

    Call(FnB721Warrants.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
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
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateProgramProcessingInfo()
  {
    var parameterList = local.ProgramProcessingInfo.ParameterList ?? "";

    entities.ProgramProcessingInfo.Populated = false;
    Update("UpdateProgramProcessingInfo",
      (db, command) =>
      {
        db.SetNullableString(command, "parameterList", parameterList);
        db.SetString(command, "name", entities.ProgramProcessingInfo.Name);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      });

    entities.ProgramProcessingInfo.ParameterList = parameterList;
    entities.ProgramProcessingInfo.Populated = true;
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
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    /// <summary>
    /// A value of Audit.
    /// </summary>
    [JsonPropertyName("audit")]
    public Common Audit
    {
      get => audit ??= new();
      set => audit = value;
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
    /// A value of RestartLine.
    /// </summary>
    [JsonPropertyName("restartLine")]
    public TextWorkArea RestartLine
    {
      get => restartLine ??= new();
      set => restartLine = value;
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
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
    /// A value of ReportPeriodEndDate.
    /// </summary>
    [JsonPropertyName("reportPeriodEndDate")]
    public DateWorkArea ReportPeriodEndDate
    {
      get => reportPeriodEndDate ??= new();
      set => reportPeriodEndDate = value;
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

    private Ocse157Verification ocse157Verification;
    private Common audit;
    private ProgramProcessingInfo programProcessingInfo;
    private TextWorkArea restartLine;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Ocse34 ocse34;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea reportPeriodEndDate;
    private ProgramCheckpointRestart programCheckpointRestart;
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

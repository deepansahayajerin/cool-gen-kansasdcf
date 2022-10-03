// Program: FN_BATCH_SRRUN230_MMIS_REPORT, ID: 372814451, model: 746.
// Short name: SWEB230P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BATCH_SRRUN230_MMIS_REPORT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBatchSrrun230MmisReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BATCH_SRRUN230_MMIS_REPORT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBatchSrrun230MmisReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBatchSrrun230MmisReport.
  /// </summary>
  public FnBatchSrrun230MmisReport(IContext context, Import import,
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
    // ***  Initial code completed July, 1999 swsrmxk
    // ***
    // ---------------------------------------------------------------
    // Initialize Exit State and set program execution date.
    // ---------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.LpgmProcessing.Date = Now().Date;
    local.LprogramProcessingInfo.Name = global.UserId;

    // ---------------------------------------------------------------
    // Retrieve the run parameters for this program.
    // ---------------------------------------------------------------
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------------
    // Process Date represents the start date for the report.
    // Abort processing if Process Date is null.
    // ---------------------------------------------------------------
    if (Lt(local.Lnull.Date, local.LprogramProcessingInfo.ProcessDate))
    {
      local.LpgmRun.Date = local.LprogramProcessingInfo.ProcessDate;
    }
    else
    {
      ExitState = "INVALID_PROCESS_DATE_RB";

      return;
    }

    // ---------------------------------------------------------------
    // Record the start time of this program.
    // ---------------------------------------------------------------
    // ---------------------------------------------------------------
    // Open the Standard Batch Error Report File.
    // DDNAME = RPT99
    // ---------------------------------------------------------------
    local.Lerr.Action = "OPEN";
    local.LerrNeededToOpen.ProgramName = local.LprogramProcessingInfo.Name;
    local.LerrNeededToOpen.ProcessDate = local.LpgmProcessing.Date;
    UseCabErrorReport();

    if (Equal(local.Lerr.Status, "OK"))
    {
      // --> FALL THRU
    }
    else
    {
      // -->  Unable to open error file.
      //      Abort Program.
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------
    // Call action block to generate report.
    // ---------------------------------------------------------------
    local.LnfError.Subscript = 0;
    UseFnMmisRptExtract();

    // ---------------------------------------------------------------
    // Export GRP View
    // Returned subscript value greater than 0 indicates nonfatal errors which 
    // should be listed in the Error Report.
    // Export RC4 Error Message View
    // Export RC4 Error Message indicates a fatal error which corresponds to 
    // EXIT STATE other than ALL OK.
    // ---------------------------------------------------------------
    if (local.LnfError.Subscript > 0)
    {
      local.Lerr.Action = "WRITE";

      for(local.LgrpNfError.Index = 0; local.LgrpNfError.Index < local
        .LgrpNfError.Count; ++local.LgrpNfError.Index)
      {
        if (!local.LgrpNfError.CheckSize())
        {
          break;
        }

        local.LerrorNeededToWrite.RptDetail =
          local.LgrpNfError.Item.LgrpmbrNfErrorEabReportSend.RptDetail;
        UseCabErrorReport();

        if (Equal(local.Lerr.Status, "OK"))
        {
          // --> FALL THRU
        }
        else
        {
          // -->  File Error.
          //      Abort Program.
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      local.LgrpNfError.CheckIndex();
    }

    // ---------------------------------------------------------------
    // Any exit state returned other than 'all ok' should terminate
    // processing with ABORT.
    // ---------------------------------------------------------------
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.LerrorNeededToWrite.RptDetail = local.Lrc4ErrorMessage.RptDetail;
      local.Lerr.Action = "WRITE";
      UseCabErrorReport();

      if (Equal(local.Lerr.Status, "OK"))
      {
        // --> FALL THRU
      }
      else
      {
        // -->  File Error.
        //      Abort Program.
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // ---------------------------------------------------------------
    // Close the Standard Batch Error Report File.
    // DDNAME = RPT99
    // ---------------------------------------------------------------
    local.Lerr.Action = "CLOSE";
    UseCabErrorReport();

    if (Equal(local.Lerr.Status, "OK"))
    {
      // --> FALL THRU
    }
    else
    {
      // -->  File Error.
      //      Abort Program.
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ---------------------------------------------------------------
    // Record the program end time.
    // ---------------------------------------------------------------
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveNfError(FnMmisRptExtract.Export.NfErrorGroup source,
    Local.LgrpNfErrorGroup target)
  {
    target.LgrpmbrNfErrorCollection.SystemGeneratedIdentifier =
      source.GrpmbrNfCollection.SystemGeneratedIdentifier;
    target.LgrpmbrNfErrorEabReportSend.RptDetail =
      source.GrpmbrNfEabReportSend.RptDetail;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Lerr.Action;
    MoveEabReportSend(local.LerrNeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.LerrorNeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Lerr.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnMmisRptExtract()
  {
    var useImport = new FnMmisRptExtract.Import();
    var useExport = new FnMmisRptExtract.Export();

    useImport.RunMonthStart.Date = local.LpgmRun.Date;

    Call(FnMmisRptExtract.Execute, useImport, useExport);

    local.Lrc4ErrorMessage.RptDetail = useExport.RcErrorMessage.RptDetail;
    local.LnfError.Subscript = useExport.NfError1.Subscript;
    useExport.NfError.CopyTo(local.LgrpNfError, MoveNfError);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.LprogramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.LprogramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A LgrpNfErrorGroup group.</summary>
    [Serializable]
    public class LgrpNfErrorGroup
    {
      /// <summary>
      /// A value of LgrpmbrNfErrorCollection.
      /// </summary>
      [JsonPropertyName("lgrpmbrNfErrorCollection")]
      public Collection LgrpmbrNfErrorCollection
      {
        get => lgrpmbrNfErrorCollection ??= new();
        set => lgrpmbrNfErrorCollection = value;
      }

      /// <summary>
      /// A value of LgrpmbrNfErrorEabReportSend.
      /// </summary>
      [JsonPropertyName("lgrpmbrNfErrorEabReportSend")]
      public EabReportSend LgrpmbrNfErrorEabReportSend
      {
        get => lgrpmbrNfErrorEabReportSend ??= new();
        set => lgrpmbrNfErrorEabReportSend = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Collection lgrpmbrNfErrorCollection;
      private EabReportSend lgrpmbrNfErrorEabReportSend;
    }

    /// <summary>
    /// A value of Lerr.
    /// </summary>
    [JsonPropertyName("lerr")]
    public EabFileHandling Lerr
    {
      get => lerr ??= new();
      set => lerr = value;
    }

    /// <summary>
    /// A value of LerrNeededToOpen.
    /// </summary>
    [JsonPropertyName("lerrNeededToOpen")]
    public EabReportSend LerrNeededToOpen
    {
      get => lerrNeededToOpen ??= new();
      set => lerrNeededToOpen = value;
    }

    /// <summary>
    /// A value of LerrorNeededToWrite.
    /// </summary>
    [JsonPropertyName("lerrorNeededToWrite")]
    public EabReportSend LerrorNeededToWrite
    {
      get => lerrorNeededToWrite ??= new();
      set => lerrorNeededToWrite = value;
    }

    /// <summary>
    /// A value of Lnull.
    /// </summary>
    [JsonPropertyName("lnull")]
    public DateWorkArea Lnull
    {
      get => lnull ??= new();
      set => lnull = value;
    }

    /// <summary>
    /// A value of LpgmRun.
    /// </summary>
    [JsonPropertyName("lpgmRun")]
    public DateWorkArea LpgmRun
    {
      get => lpgmRun ??= new();
      set => lpgmRun = value;
    }

    /// <summary>
    /// A value of LpgmProcessing.
    /// </summary>
    [JsonPropertyName("lpgmProcessing")]
    public DateWorkArea LpgmProcessing
    {
      get => lpgmProcessing ??= new();
      set => lpgmProcessing = value;
    }

    /// <summary>
    /// A value of Lrc4ErrorMessage.
    /// </summary>
    [JsonPropertyName("lrc4ErrorMessage")]
    public EabReportSend Lrc4ErrorMessage
    {
      get => lrc4ErrorMessage ??= new();
      set => lrc4ErrorMessage = value;
    }

    /// <summary>
    /// A value of LprogramProcessingInfo.
    /// </summary>
    [JsonPropertyName("lprogramProcessingInfo")]
    public ProgramProcessingInfo LprogramProcessingInfo
    {
      get => lprogramProcessingInfo ??= new();
      set => lprogramProcessingInfo = value;
    }

    /// <summary>
    /// A value of LprogramRun.
    /// </summary>
    [JsonPropertyName("lprogramRun")]
    public ProgramRun LprogramRun
    {
      get => lprogramRun ??= new();
      set => lprogramRun = value;
    }

    /// <summary>
    /// A value of LnfError.
    /// </summary>
    [JsonPropertyName("lnfError")]
    public Common LnfError
    {
      get => lnfError ??= new();
      set => lnfError = value;
    }

    /// <summary>
    /// Gets a value of LgrpNfError.
    /// </summary>
    [JsonIgnore]
    public Array<LgrpNfErrorGroup> LgrpNfError => lgrpNfError ??= new(
      LgrpNfErrorGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LgrpNfError for json serialization.
    /// </summary>
    [JsonPropertyName("lgrpNfError")]
    [Computed]
    public IList<LgrpNfErrorGroup> LgrpNfError_Json
    {
      get => lgrpNfError;
      set => LgrpNfError.Assign(value);
    }

    private EabFileHandling lerr;
    private EabReportSend lerrNeededToOpen;
    private EabReportSend lerrorNeededToWrite;
    private DateWorkArea lnull;
    private DateWorkArea lpgmRun;
    private DateWorkArea lpgmProcessing;
    private EabReportSend lrc4ErrorMessage;
    private ProgramProcessingInfo lprogramProcessingInfo;
    private ProgramRun lprogramRun;
    private Common lnfError;
    private Array<LgrpNfErrorGroup> lgrpNfError;
  }
#endregion
}

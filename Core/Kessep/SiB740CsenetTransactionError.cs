// Program: SI_B740_CSENET_TRANSACTION_ERROR, ID: 372895041, model: 746.
// Short name: SWEI740B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B740_CSENET_TRANSACTION_ERROR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB740CsenetTransactionError: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B740_CSENET_TRANSACTION_ERROR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB740CsenetTransactionError(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB740CsenetTransactionError.
  /// </summary>
  public SiB740CsenetTransactionError(IContext context, Import import,
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
    //     Date   Name      PR#  Reason
    //     ----   ----      ---  ------
    //   Sept 99                  Production
    //   Oct  01  MCA       10502 Re-Wrote prad to Read the error file and 
    // update
    //                      the CSENet Transaction Envelope Status to "E"
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB740Houskeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    while(!Equal(export.EabFileHandling.Status, "EOF"))
    {
      // **********************************************************************
      // *** READ the CSENET Transaction Error file
      // **********************************************************************
      export.EabFileHandling.Action = "READ";
      UseEabCsenetErrorsFileReader();

      switch(TrimEnd(export.EabFileHandling.Status))
      {
        case "EOF":
          goto AfterCycle;
        case "OK":
          break;
        default:
          // **********************************************************************
          // *** WRITE to the Error Report
          // **********************************************************************
          export.EabFileHandling.Action = "WRITE";
          export.NeededToWrite.RptDetail =
            "Error Reading the CSENET Error File";
          UseCabErrorReport();

          goto AfterCycle;
      }

      ++local.CountErrorRecordsRead.Count;

      if (ReadCsenetTransactionEnvelop())
      {
        ++local.CountEnvelopesRead.Count;

        try
        {
          UpdateCsenetTransactionEnvelop();
          ++local.CountEnvelopesUpdated.Count;

          // **********************************************************************
          // Create alert for certain error codes. Currently the only one is 
          // E569.  We can set it up so it reads the event detail equal to the
          // error code(The error code in the file is the reason code on event
          // detail.) and if there is a alert dist rule, create the alert.  That
          // way, the users are in total control of what errors generate
          // alerts.
          // **********************************************************************
          local.Infrastructure.Function = export.Prad.FunctionalTypeCd;
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.ReferenceDate =
            entities.ProgramProcessingInfo.ProcessDate;
          local.Infrastructure.DenormDate =
            entities.ProgramProcessingInfo.ProcessDate;
          local.Infrastructure.CreatedBy = entities.ProgramProcessingInfo.Name;
          local.Infrastructure.LastUpdatedBy =
            entities.ProgramProcessingInfo.Name;
          local.Infrastructure.UserId = entities.ProgramProcessingInfo.Name;
          local.Infrastructure.CreatedTimestamp = Now();
          local.Infrastructure.DenormTimestamp = Now();
          local.Infrastructure.CsenetInOutCode = "O";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.EventType = "CSENET";
          local.Infrastructure.ReasonCode = export.Prad.ErrorCd;
          local.Infrastructure.CaseNumber = export.Prad.LocalCaseId;
          local.Infrastructure.Detail = export.Prad.ErrorMsg;

          if (IsEmpty(export.Prad.LocalCaseId))
          {
            local.Infrastructure.InitiatingStateCode = export.Prad.OtherFipsCd;
            local.Infrastructure.EventId = 17;
            local.Infrastructure.EventDetailName =
              "CSENET OS CASE ERROR FROM HOST";
          }
          else
          {
            local.Infrastructure.InitiatingStateCode = "KS";
            local.Infrastructure.EventId = 14;
            local.Infrastructure.EventDetailName =
              "CSENET KS CASE ERROR FROM HOST";
          }

          UseSpCabCreateInfrastructure();

          // **********************************************************************
          // This is commented out because if there was no event detail,  the 
          // users do not want an alert created for that error.
          // **********************************************************************
          ExitState = "ACO_NN0000_ALL_OK";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              // **********************************************************************
              // *** WRITE to the Error Report
              // **********************************************************************
              export.EabFileHandling.Action = "WRITE";
              export.NeededToWrite.RptDetail =
                "Update to Transaction Envelope not unique for serial number: " +
                NumberToString(export.Prad.TransactionId, 15);
              UseCabErrorReport();

              break;
            case ErrorCode.PermittedValueViolation:
              // **********************************************************************
              // *** WRITE to the Error Report
              // **********************************************************************
              export.EabFileHandling.Action = "WRITE";
              export.NeededToWrite.RptDetail =
                "Update to Transaction Envelope Permited value error for serial number: " +
                NumberToString(export.Prad.TransactionId, 15);
              UseCabErrorReport();

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
        // **********************************************************************
        // *** WRITE to the Error Report
        // **********************************************************************
        export.EabFileHandling.Action = "WRITE";
        export.NeededToWrite.RptDetail =
          "Transaction Envelope not found for serial number: " + NumberToString
          (export.Prad.TransactionId, 15);
        UseCabErrorReport();
      }
    }

AfterCycle:

    UseSiB740Close();
  }

  private static void MoveCsenetHostErrorFile(CsenetHostErrorFile source,
    CsenetHostErrorFile target)
  {
    target.TxnCounter = source.TxnCounter;
    target.LocalFipsCd = source.LocalFipsCd;
    target.OtherFipsCd = source.OtherFipsCd;
    target.LocalCaseId = source.LocalCaseId;
    target.OtherCaseId = source.OtherCaseId;
    target.ActionCd = source.ActionCd;
    target.FunctionalTypeCd = source.FunctionalTypeCd;
    target.ActionReasonCd = source.ActionReasonCd;
    target.TransactionDt = source.TransactionDt;
    target.ErrorCd = source.ErrorCd;
    target.ErrorMsg = source.ErrorMsg;
    target.TransactionId = source.TransactionId;
  }

  private static void MoveEabFileHandling(EabFileHandling source,
    EabFileHandling target)
  {
    target.Action = source.Action;
    target.Status = source.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = export.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabCsenetErrorsFileReader()
  {
    var useImport = new EabCsenetErrorsFileReader.Import();
    var useExport = new EabCsenetErrorsFileReader.Export();

    useImport.EabFileHandling.Action = export.EabFileHandling.Action;
    MoveCsenetHostErrorFile(export.Prad, useExport.CsenetHostErrorFile);
    useExport.EabFileHandling.Status = export.EabFileHandling.Status;

    Call(EabCsenetErrorsFileReader.Execute, useImport, useExport);

    export.Prad.Assign(useExport.CsenetHostErrorFile);
    export.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSiB740Close()
  {
    var useImport = new SiB740Close.Import();
    var useExport = new SiB740Close.Export();

    useImport.CountErrorRecordsRead.Count = local.CountErrorRecordsRead.Count;
    useImport.CountEnvelopesRead.Count = local.CountEnvelopesRead.Count;
    useImport.CountEnvelopesUpdated.Count = local.CountEnvelopesUpdated.Count;

    Call(SiB740Close.Execute, useImport, useExport);
  }

  private void UseSiB740Houskeeping()
  {
    var useImport = new SiB740Houskeeping.Import();
    var useExport = new SiB740Houskeeping.Export();

    Call(SiB740Houskeeping.Execute, useImport, useExport);

    MoveEabFileHandling(useExport.EabFileHandling, export.EabFileHandling);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(command, "ccaTransSerNum", export.Prad.TransactionId);
        db.SetDate(
          command, "ccaTransactionDt",
          export.Prad.TransactionDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ErrorCode =
          db.GetNullableString(reader, 5);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = "SWEIB740";
    var lastUpdatedTimestamp = Now();
    var processingStatusCode = "E";
    var errorCode1 = export.Prad.ErrorCd;

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetNullableString(command, "errorCode", errorCode1);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.CsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.CsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.ErrorCode = errorCode1;
    entities.CsenetTransactionEnvelop.Populated = true;
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
    /// A value of Prad.
    /// </summary>
    [JsonPropertyName("prad")]
    public CsenetHostErrorFile Prad
    {
      get => prad ??= new();
      set => prad = value;
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private CsenetHostErrorFile prad;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToWrite;
    private ReportParms reportParms;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CountEnvelopesRead.
    /// </summary>
    [JsonPropertyName("countEnvelopesRead")]
    public Common CountEnvelopesRead
    {
      get => countEnvelopesRead ??= new();
      set => countEnvelopesRead = value;
    }

    /// <summary>
    /// A value of CountEnvelopesUpdated.
    /// </summary>
    [JsonPropertyName("countEnvelopesUpdated")]
    public Common CountEnvelopesUpdated
    {
      get => countEnvelopesUpdated ??= new();
      set => countEnvelopesUpdated = value;
    }

    /// <summary>
    /// A value of CountErrorRecordsRead.
    /// </summary>
    [JsonPropertyName("countErrorRecordsRead")]
    public Common CountErrorRecordsRead
    {
      get => countErrorRecordsRead ??= new();
      set => countErrorRecordsRead = value;
    }

    /// <summary>
    /// A value of CsenetHostErrorFile.
    /// </summary>
    [JsonPropertyName("csenetHostErrorFile")]
    public CsenetHostErrorFile CsenetHostErrorFile
    {
      get => csenetHostErrorFile ??= new();
      set => csenetHostErrorFile = value;
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
    /// A value of Eof.
    /// </summary>
    [JsonPropertyName("eof")]
    public Common Eof
    {
      get => eof ??= new();
      set => eof = value;
    }

    private Infrastructure infrastructure;
    private Common countEnvelopesRead;
    private Common countEnvelopesUpdated;
    private Common countErrorRecordsRead;
    private CsenetHostErrorFile csenetHostErrorFile;
    private Common ppiFound;
    private EabFileHandling eabFileHandling;
    private ReportParms reportParms;
    private Common eof;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

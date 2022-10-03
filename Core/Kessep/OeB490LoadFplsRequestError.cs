// Program: OE_B490_LOAD_FPLS_REQUEST_ERROR, ID: 372366307, model: 746.
// Short name: SWEE490B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B490_LOAD_FPLS_REQUEST_ERROR.
/// </para>
/// <para>
/// RESP: OBLGEST	
/// This Batch procedure handles all the logic nedded to update FPLS_REQUEST 
/// with errors returned by the Federal Parent Locator Service. Input is via an
/// External procedure which is responsible for reading a flat file containing
/// the errors.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class OeB490LoadFplsRequestError: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B490_LOAD_FPLS_REQUEST_ERROR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB490LoadFplsRequestError(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB490LoadFplsRequestError.
  /// </summary>
  public OeB490LoadFplsRequestError(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //   DATE		Developer	Description
    // 07/13/96     	T.O.Redmond	Initial Creation
    // This Action Block contains all the logic required to process an error 
    // file to be received from FPLS(Federal Locator Person Service).
    // 1/2/98   Siraj Konkader
    // Modified calls to Create_Program_Error, Create_Program_Control_Total - 
    // removed persistent views beacuse of performance problems.
    // Also removed calls to assign_program_error_id and  
    // assign_program_control_total_id. The function of these cabs were to set
    // the identifiers of Program Error Id and Program Control Total.  However,
    // since both the above tables are used in conjunction with Program Run, the
    // identifiers will always start from 1 and not any "last used" value + 1.
    // ---------------------------------------------------------------
    // ************************************************
    // *7/16/96 Due to the expectation that only a very low volume of errors 
    // will be received, It was decided that this procedure should not contain
    // restart logic.
    // ************************************************
    // 7/16/96 FPLS sends a 10 byte field for transaction errors which contains 
    // up to 5 possible error codes. Kessep has decided that they only expect
    // error code 03 to appear, therefore only a single error code will be
    // updated from this file.
    // ************************************************
    // For restart this process can simply be rerun.
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // ***********************************************
    // *Initialize flags                             *
    // ***********************************************
    export.DateError.Flag = "Y";
    local.FileOpened.Flag = "N";
    local.NoOfFplsRecsRead.Count = 0;
    local.NoFplsUpdates.Count = 0;
    local.ProgramError.SystemGeneratedIdentifier = 0;

    // -----------------------------------------------------------
    // 4.10.100
    // Beginning of Change
    // Open Error Report DDNAME=RPT99 and Control Report DDNAME = RPT98 .
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = "SWEEB490";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error encountered opening control report.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -----------------------------------------------------------
    // 4.10.100
    // End of Change
    // -----------------------------------------------------------
    // ************************************************
    // *Find out if we are in a continuation cycle    *
    // *(recursive flow) or if this is the first time *
    // *into the program.                             *
    // ************************************************
    if (AsChar(import.BatchAttributes.ContinuationInd) == 'Y')
    {
      // ************************************************
      // *Since the Continuation_Ind is 'Y' this means  *
      // *that we are in a continuation cycle.          *
      // ************************************************
      // ************************************************
      // *Move imports to exports for the recursive flow*
      // ************************************************
      export.BatchAttributes.ContinuationInd =
        import.BatchAttributes.ContinuationInd;
      export.ProgramProcessingInfo.Assign(import.ProgramProcessingInfo);
      MoveProgramRun(import.ProgramRun, export.ProgramRun);

      // ************************************************
      // *Get latest commit frequencies numbers.        *
      // ************************************************
    }
    else
    {
      // ************************************************
      // *Since the Continuation_Ind is not 'Y' this    *
      // *means that this is the first time into this   *
      // *program.
      // 
      // *
      // ************************************************
      // ***********************************************
      // *Get the run parameters for this program.     *
      // ***********************************************
      local.ProgramProcessingInfo.Name = global.UserId;
      UseReadProgramProcessingInfo();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ************************************************
      // *Record the start time of this program.        *
      // ************************************************
      UseCreateProgramRun();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      local.FileOpened.Flag = "N";

      // ************************************************
      // * Call external to OPEN the driver file.       *
      // ************************************************
      local.PassArea.FileInstruction = "OPEN";
      UseOeEabReceiveFplsErrors();

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        ExitState = "FILE_POSITION_ERROR_WITH_RB";

        return;
      }

      local.FileOpened.Flag = "Y";
    }

    // ************************************************
    // *Read the program_run each time we come into   *
    // *this program so that we will have currency for*
    // *creating any error rows or control total rows.*
    // ************************************************
    if (ReadProgramRun())
    {
      // Get the next error number and control total number so that it can be 
      // incremented below.  Only want to read the database once to get the next
      // number, not every insert.
      // **** Above stmt incorrect. Deleted calls to cabs. See modification log
      // ... SAK 1/2/98
    }
    else
    {
      ExitState = "PROGRAM_RUN_NF_RB";

      return;
    }

    // ************************************************
    // *Process driver records until we need to do a  *
    // *commit or until we have reached the end of    *
    // *file.
    // 
    // *
    // ************************************************
    do
    {
      // ************************************************
      // *Call external to READ the driver file.        *
      // ************************************************
      local.PassArea.FileInstruction = "READ";
      UseOeEabReceiveFplsErrors();

      if (Equal(local.PassArea.TextReturnCode, "EF"))
      {
        continue;
      }

      if (!IsEmpty(local.PassArea.TextReturnCode))
      {
        ExitState = "FILE_READ_ERROR_WITH_RB";

        return;
      }

      // ************************************************
      // *At this point we will create a local view of  *
      // *FPLS taken from the Batch Import.             *
      // ************************************************
      ++local.NoOfFplsRecsRead.Count;
      local.FplsRequestFound.Flag = "N";

      if (ReadCsePerson())
      {
        if (ReadFplsLocateRequest())
        {
          // -------------------------------------------------------------
          // Beginning Of Change
          // 4.10.100
          // Removed 'IF' condition for Transaction Error and set all fields for
          // Transaction Error to what we get back from Fed.
          // -------------------------------------------------------------
          try
          {
            UpdateFplsLocateRequest();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
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
          local.FplsRequestFound.Flag = "N";

          // -----------------------------------------------------------
          // 4.10.100
          // Beginning of Change
          // Write error to error report.
          // -----------------------------------------------------------
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "FPLS Request is missing for ID:" + NumberToString
            (local.Batch.FplsLocateRequestIdentifier, 15) + " CSE Person:" + local
            .Batch.ApCsePersonNumber;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // -----------------------------------------------------------
          // 4.10.100
          // End of Change
          // -----------------------------------------------------------
        }
      }
      else
      {
        // -----------------------------------------------------------
        // 4.10.100
        // Beginning of Change
        // Write error to error report.
        // -----------------------------------------------------------
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "FPLS: CSE Person is missing for ID:" + NumberToString
          (local.Batch.FplsLocateRequestIdentifier, 15) + " CSE Person:" + local
          .Batch.ApCsePersonNumber;
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        // -----------------------------------------------------------
        // 4.10.100
        // End of Change
        // -----------------------------------------------------------
      }
    }
    while(!Equal(local.PassArea.TextReturnCode, "EF"));

    local.TotalReads.Count += local.NoOfFplsRecsRead.Count;
    local.NoOfFplsRecsRead.Count = 0;

    if (Equal(local.PassArea.TextReturnCode, "EF"))
    {
      // ************************************************
      // *The external hit the end of the driver file,  *
      // *closed the file and returned an EF (EOF)      *
      // *indicator.
      // 
      // *
      // ************************************************
      // -----------------------------------------------------------------
      // 4.10.100
      // Beginning Of Change
      // Write all totals to Control Report
      // -----------------------------------------------------------------
      local.EabReportSend.RptDetail =
        "FPLS Total  Records Read Count       :" + NumberToString
        (local.TotalReads.Count, 15);
      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while writting control report(number of request read).";
          
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ************************************************
      // *Record the program end time.                  *
      // ************************************************
      UseUpdateProgramRun();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ---------------------------------------------------------------
      // 4.10.100
      // Beginning Of Change
      // Close Error Report and Control Report files.
      // ---------------------------------------------------------------
      local.EabFileHandling.Action = "CLOSE";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error encountered while closing control report.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // ---------------------------------------------------------------
      // 4.10.100
      // End Of Change
      // ---------------------------------------------------------------
      ExitState = "ACO_NN0000_ALL_OK";

      return;
    }
    else
    {
      ExitState = "DO_A_RECURSIVE_FLOW";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
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
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveProgramRun(ProgramRun source, ProgramRun target)
  {
    target.StartTimestamp = source.StartTimestamp;
    target.EndTimestamp = source.EndTimestamp;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
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

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCreateProgramRun()
  {
    var useImport = new CreateProgramRun.Import();
    var useExport = new CreateProgramRun.Export();

    MoveProgramProcessingInfo(export.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.FromRestartInd = export.ProgramRun.FromRestartInd;

    Call(CreateProgramRun.Execute, useImport, useExport);

    export.ProgramRun.StartTimestamp = useExport.ProgramRun.StartTimestamp;
  }

  private void UseOeEabReceiveFplsErrors()
  {
    var useImport = new OeEabReceiveFplsErrors.Import();
    var useExport = new OeEabReceiveFplsErrors.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.ExternalFplsRequest.Assign(local.Batch);
    useExport.External.Assign(local.PassArea);

    Call(OeEabReceiveFplsErrors.Execute, useImport, useExport);

    local.Batch.Assign(useExport.ExternalFplsRequest);
    local.PassArea.Assign(useExport.External);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    export.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateProgramRun()
  {
    var useImport = new UpdateProgramRun.Import();
    var useExport = new UpdateProgramRun.Export();

    MoveProgramProcessingInfo(export.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.ProgramRun.StartTimestamp = export.ProgramRun.StartTimestamp;

    Call(UpdateProgramRun.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Batch.ApCsePersonNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);
      });
  }

  private bool ReadFplsLocateRequest()
  {
    entities.ExistingFplsLocateRequest.Populated = false;

    return Read("ReadFplsLocateRequest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(
          command, "fplsLocateRequestIdentifier",
          local.Batch.FplsLocateRequestIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingFplsLocateRequest.CspNumber = db.GetString(reader, 0);
        entities.ExistingFplsLocateRequest.Identifier = db.GetInt32(reader, 1);
        entities.ExistingFplsLocateRequest.TransactionStatus =
          db.GetNullableString(reader, 2);
        entities.ExistingFplsLocateRequest.TransactionError =
          db.GetNullableString(reader, 3);
        entities.ExistingFplsLocateRequest.LastUpdatedBy =
          db.GetString(reader, 4);
        entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ExistingFplsLocateRequest.Populated = true;
      });
  }

  private bool ReadProgramRun()
  {
    entities.ExistingProgramRun.Populated = false;

    return Read("ReadProgramRun",
      (db, command) =>
      {
        db.SetDateTime(
          command, "startTimestamp",
          export.ProgramRun.StartTimestamp.GetValueOrDefault());
        db.SetString(command, "ppiName", global.UserId);
        db.SetDateTime(
          command, "ppiCreatedTstamp",
          export.ProgramProcessingInfo.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingProgramRun.PpiCreatedTstamp =
          db.GetDateTime(reader, 0);
        entities.ExistingProgramRun.PpiName = db.GetString(reader, 1);
        entities.ExistingProgramRun.StartTimestamp = db.GetDateTime(reader, 2);
        entities.ExistingProgramRun.Populated = true;
      });
  }

  private void UpdateFplsLocateRequest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingFplsLocateRequest.Populated);

    var transactionError = local.Batch.TransactionError;
    var lastUpdatedBy = global.TranCode;
    var lastUpdatedTimestamp = Now();

    entities.ExistingFplsLocateRequest.Populated = false;
    Update("UpdateFplsLocateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "transactionError", transactionError);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(
          command, "cspNumber", entities.ExistingFplsLocateRequest.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingFplsLocateRequest.Identifier);
          
      });

    entities.ExistingFplsLocateRequest.TransactionError = transactionError;
    entities.ExistingFplsLocateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFplsLocateRequest.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFplsLocateRequest.Populated = true;
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
    /// A value of BatchAttributes.
    /// </summary>
    [JsonPropertyName("batchAttributes")]
    public BatchAttributes BatchAttributes
    {
      get => batchAttributes ??= new();
      set => batchAttributes = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private BatchAttributes batchAttributes;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DateError.
    /// </summary>
    [JsonPropertyName("dateError")]
    public Common DateError
    {
      get => dateError ??= new();
      set => dateError = value;
    }

    /// <summary>
    /// A value of BatchAttributes.
    /// </summary>
    [JsonPropertyName("batchAttributes")]
    public BatchAttributes BatchAttributes
    {
      get => batchAttributes ??= new();
      set => batchAttributes = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
    }

    private Common dateError;
    private BatchAttributes batchAttributes;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramRun programRun;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public ExternalFplsRequest Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of TotalReads.
    /// </summary>
    [JsonPropertyName("totalReads")]
    public Common TotalReads
    {
      get => totalReads ??= new();
      set => totalReads = value;
    }

    /// <summary>
    /// A value of NoOfFplsRecsRead.
    /// </summary>
    [JsonPropertyName("noOfFplsRecsRead")]
    public Common NoOfFplsRecsRead
    {
      get => noOfFplsRecsRead ??= new();
      set => noOfFplsRecsRead = value;
    }

    /// <summary>
    /// A value of NoFplsUpdates.
    /// </summary>
    [JsonPropertyName("noFplsUpdates")]
    public Common NoFplsUpdates
    {
      get => noFplsUpdates ??= new();
      set => noFplsUpdates = value;
    }

    /// <summary>
    /// A value of NumberOfReads.
    /// </summary>
    [JsonPropertyName("numberOfReads")]
    public Common NumberOfReads
    {
      get => numberOfReads ??= new();
      set => numberOfReads = value;
    }

    /// <summary>
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of FileOpened.
    /// </summary>
    [JsonPropertyName("fileOpened")]
    public Common FileOpened
    {
      get => fileOpened ??= new();
      set => fileOpened = value;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
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
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
    }

    /// <summary>
    /// A value of FplsRequestFound.
    /// </summary>
    [JsonPropertyName("fplsRequestFound")]
    public Common FplsRequestFound
    {
      get => fplsRequestFound ??= new();
      set => fplsRequestFound = value;
    }

    /// <summary>
    /// A value of NoOfCasesInput.
    /// </summary>
    [JsonPropertyName("noOfCasesInput")]
    public Common NoOfCasesInput
    {
      get => noOfCasesInput ??= new();
      set => noOfCasesInput = value;
    }

    private EabReportSend neededToWrite;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private ExternalFplsRequest batch;
    private Common totalReads;
    private Common noOfFplsRecsRead;
    private Common noFplsUpdates;
    private Common numberOfReads;
    private Common numberOfUpdates;
    private External passArea;
    private Common fileOpened;
    private ProgramError programError;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramControlTotal programControlTotal;
    private Common fplsRequestFound;
    private Common noOfCasesInput;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFplsLocateRequest.
    /// </summary>
    [JsonPropertyName("existingFplsLocateRequest")]
    public FplsLocateRequest ExistingFplsLocateRequest
    {
      get => existingFplsLocateRequest ??= new();
      set => existingFplsLocateRequest = value;
    }

    /// <summary>
    /// A value of ExistingProgramRun.
    /// </summary>
    [JsonPropertyName("existingProgramRun")]
    public ProgramRun ExistingProgramRun
    {
      get => existingProgramRun ??= new();
      set => existingProgramRun = value;
    }

    /// <summary>
    /// A value of ExistingProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("existingProgramProcessingInfo")]
    public ProgramProcessingInfo ExistingProgramProcessingInfo
    {
      get => existingProgramProcessingInfo ??= new();
      set => existingProgramProcessingInfo = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private FplsLocateRequest existingFplsLocateRequest;
    private ProgramRun existingProgramRun;
    private ProgramProcessingInfo existingProgramProcessingInfo;
    private CsePerson existingCsePerson;
  }
#endregion
}

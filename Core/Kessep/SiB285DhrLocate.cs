// Program: SI_B285_DHR_LOCATE, ID: 372426679, model: 746.
// Short name: SWEI285B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B285_DHR_LOCATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB285DhrLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B285_DHR_LOCATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB285DhrLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB285DhrLocate.
  /// </summary>
  public SiB285DhrLocate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Description
    // ------------------------------------------------------------
    // 01/13/1999	C Ott		Initial Dev.
    // 09/20/1999	C Ott		Problem # 74064.
    // Remove ABEND exit state when DHR input data set is empty or not found.
    // 10/12/1999	C Ott		Problem # 77196.
    // Reset flag to write record to error report.
    // 07/18/2001	M Ramirez	PR# xxxxxx
    // Rework to remove restart logic
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEIB285";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***************************************************
    // *Open the ERROR RPT.  DDNAME=RPT99.
    // ***************************************************
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = local.ProgramProcessingInfo.Name;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ***************************************************
    // *Open the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered opening the Control Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Get Processing Parameters
    // ***************************************************
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      if (Find(local.ProgramProcessingInfo.ParameterList, "DEBUG") > 0)
      {
        local.Debug.Flag = "Y";
      }
      else
      {
        local.Debug.Flag = "N";
      }
    }
    else
    {
      local.Debug.Flag = "N";
    }

    // ***************************************************
    // *Get Checkpoint restart information
    // ***************************************************
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered reading the Checkpoint Restart Information.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.External.FileInstruction = "OPEN";
    UseSiEabReceiveWageIncSource2();

    if (!IsEmpty(local.Return1.TextReturnCode))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered opening the DHR Locate file.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    local.EabFileHandling.Action = "WRITE";
    local.External.FileInstruction = "READ";

    do
    {
      UseSiEabReceiveWageIncSource1();

      if (AsChar(local.Debug.Flag) == 'Y')
      {
        local.Write.RptDetail = "DEBUG:  Read Record, SSN = " + local
          .SiWageIncomeSourceRec.PersonSsn;
        UseCabErrorReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (IsEmpty(local.Return1.TextReturnCode))
      {
        ++local.DhrInputFileRecsRead.Count;
        ++local.CheckpointReads.Count;

        // --------------------------------------------------------
        // Only process records that are Basic Wage type records
        // --------------------------------------------------------
        if (!Equal(local.SiWageIncomeSourceRec.RecordTypeIndicator, "BW"))
        {
          if (AsChar(local.Debug.Flag) == 'Y')
          {
            local.Write.RptDetail = "DEBUG:  Record type <> BW";
            UseCabErrorReport3();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          goto Test;
        }

        if (IsEmpty(local.SiWageIncomeSourceRec.PersonSsn) || Equal
          (local.SiWageIncomeSourceRec.PersonSsn, "000000000"))
        {
          local.Write.RptDetail = "SSN is not populated on record, SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn;
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto Test;
        }

        local.TxnFound.Flag = "";

        if (ReadCsenetTransactionEnvelopInterstateApIdentification3())
        {
          local.TxnFound.Flag = "Y";
          local.CsenetTransactionEnvelop.ProcessingStatusCode =
            entities.CsenetTransactionEnvelop.ProcessingStatusCode;
          local.InterstateCase.Assign(entities.InterstateCase);
        }

        if (IsEmpty(local.TxnFound.Flag))
        {
          if (ReadCsenetTransactionEnvelopInterstateApIdentification1())
          {
            local.TxnFound.Flag = "Y";
            local.CsenetTransactionEnvelop.ProcessingStatusCode =
              entities.CsenetTransactionEnvelop.ProcessingStatusCode;
            local.InterstateCase.Assign(entities.InterstateCase);
          }
        }

        if (IsEmpty(local.TxnFound.Flag))
        {
          if (ReadCsenetTransactionEnvelopInterstateApIdentification2())
          {
            local.TxnFound.Flag = "Y";
            local.CsenetTransactionEnvelop.ProcessingStatusCode =
              entities.CsenetTransactionEnvelop.ProcessingStatusCode;
            local.InterstateCase.Assign(entities.InterstateCase);
          }
        }

        if (IsEmpty(local.TxnFound.Flag))
        {
          ++local.TotalTxnAe.Count;
          local.Write.RptDetail = "SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn + ";  A response was already sent for SSN";
            
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto Test;
        }

        UseSiProcessQuickLocateRequests();

        if (AsChar(local.Debug.Flag) == 'Y' && local.New1.TransSerialNumber > 0)
        {
          local.Write.RptDetail =
            "DEBUG:  New transaction created; Serial # " + NumberToString
            (local.New1.TransSerialNumber, 4, 12) + ", " + NumberToString
            (DateToInt(local.New1.TransactionDate), 8, 8);
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.Write.RptDetail = "SSN = " + local
            .SiWageIncomeSourceRec.PersonSsn + ";  " + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          break;
        }

        if (ReadCsenetTransactionEnvelop())
        {
          try
          {
            UpdateCsenetTransactionEnvelop();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.Write.RptDetail =
                  "A database error occurred while creating CSENet data blocks for SSN " +
                  local.SiWageIncomeSourceRec.PersonSsn;
                UseCabErrorReport3();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                break;
              case ErrorCode.PermittedValueViolation:
                local.Write.RptDetail =
                  "A database error occurred while creating CSENet data blocks for SSN " +
                  local.SiWageIncomeSourceRec.PersonSsn;
                UseCabErrorReport3();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

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
          local.Write.RptDetail =
            "A database error occurred while creating CSENet data blocks for SSN " +
            local.SiWageIncomeSourceRec.PersonSsn;
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";

          break;
        }
      }
      else if (Equal(local.Return1.TextReturnCode, "EF"))
      {
        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.Write.RptDetail = "DEBUG:  End of file";
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
      else
      {
        local.Write.RptDetail =
          "Error encountered reading the DHR Locate file.";
        UseCabErrorReport3();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_4_BATCH";

        break;
      }

Test:

      if (local.CheckpointReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CheckpointUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.Write.RptDetail = "DEBUG:  Commit performed";
          UseCabErrorReport3();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.CheckpointUpdates.Count = 0;
        local.CheckpointReads.Count = 0;
      }
    }
    while(!Equal(local.Return1.TextReturnCode, "EF"));

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseExtToDoACommit();
    }

    // ***************************************************
    // *Close the DHR Locate Response File.
    // ***************************************************
    local.External.FileInstruction = "CLOSE";
    UseSiEabReceiveWageIncSource2();

    if (!IsEmpty(local.Return1.TextReturnCode))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the DHR Locate file.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Write the CONTROL REPORT
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "Records read from DHR Locate file                           " + NumberToString
      (local.DhrInputFileRecsRead.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing the Control Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Write.RptDetail =
      "Quick Locate Responses created from DHR Locate file         " + NumberToString
      (local.InterstateCaseCreates.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing the Control Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Write.RptDetail =
      "SSNs already processed                                      " + NumberToString
      (local.TotalTxnAe.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered writing the Control Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Close the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Control Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Close the ERROR RPT. DDNAME=RPT99.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Error Report.";
      UseCabErrorReport3();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

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

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private static void MoveSiWageIncomeSourceRec(SiWageIncomeSourceRec source,
    SiWageIncomeSourceRec target)
  {
    target.BwQtr = source.BwQtr;
    target.BwYr = source.BwYr;
    target.WageOrUiAmt = source.WageOrUiAmt;
    target.EmpId = source.EmpId;
    target.EmpName = source.EmpName;
    target.Street1 = source.Street1;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    Call(ExtToDoACommit.Execute, useImport, useExport);
  }

  private void UseReadPgmCheckpointRestart()
  {
    var useImport = new ReadPgmCheckpointRestart.Import();
    var useExport = new ReadPgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;

    Call(ReadPgmCheckpointRestart.Execute, useImport, useExport);

    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseSiEabReceiveWageIncSource1()
  {
    var useImport = new SiEabReceiveWageIncSource.Import();
    var useExport = new SiEabReceiveWageIncSource.Export();

    useImport.External.Assign(local.External);
    useExport.External.TextReturnCode = local.Return1.TextReturnCode;
    useExport.SiWageIncomeSourceRec.Assign(local.SiWageIncomeSourceRec);

    Call(SiEabReceiveWageIncSource.Execute, useImport, useExport);

    local.Return1.TextReturnCode = useExport.External.TextReturnCode;
    local.SiWageIncomeSourceRec.Assign(useExport.SiWageIncomeSourceRec);
  }

  private void UseSiEabReceiveWageIncSource2()
  {
    var useImport = new SiEabReceiveWageIncSource.Import();
    var useExport = new SiEabReceiveWageIncSource.Export();

    useImport.External.Assign(local.External);
    useExport.External.TextReturnCode = local.Return1.TextReturnCode;

    Call(SiEabReceiveWageIncSource.Execute, useImport, useExport);

    local.Return1.TextReturnCode = useExport.External.TextReturnCode;
  }

  private void UseSiProcessQuickLocateRequests()
  {
    var useImport = new SiProcessQuickLocateRequests.Import();
    var useExport = new SiProcessQuickLocateRequests.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);
    useImport.ExpInterstateCaseCreates.Count =
      local.InterstateCaseCreates.Count;
    useImport.ExpApIdentCreates.Count = local.ApIdentCreates.Count;
    useImport.ExpCheckpointUpdates.Count = local.CheckpointUpdates.Count;
    useImport.CsenetTransactionEnvelop.ProcessingStatusCode =
      local.CsenetTransactionEnvelop.ProcessingStatusCode;
    MoveSiWageIncomeSourceRec(local.SiWageIncomeSourceRec,
      useImport.SiWageIncomeSourceRec);
    useImport.BatchProcess.Date = local.Current.Date;

    Call(SiProcessQuickLocateRequests.Execute, useImport, useExport);

    local.InterstateCaseCreates.Count =
      useImport.ExpInterstateCaseCreates.Count;
    local.ApIdentCreates.Count = useImport.ExpApIdentCreates.Count;
    local.CheckpointUpdates.Count = useImport.ExpCheckpointUpdates.Count;
    MoveInterstateCase(useExport.New1, local.New1);
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", local.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          local.InterstateCase.TransactionDate.GetValueOrDefault());
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
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 6);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelopInterstateApIdentification1()
  {
    entities.CsenetTransactionEnvelop.Populated = false;
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadCsenetTransactionEnvelopInterstateApIdentification1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "altSsn1", local.SiWageIncomeSourceRec.PersonSsn);
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 6);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 8);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 9);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 9);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 14);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 15);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 16);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 17);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 22);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 23);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 24);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 25);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 26);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 27);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 28);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 29);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 30);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 31);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 32);
        entities.InterstateCase.ActionCode = db.GetString(reader, 33);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 34);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 35);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 38);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 39);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 40);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 41);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 42);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 43);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 44);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 45);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 46);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 47);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 48);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 49);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 50);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 51);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 52);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 53);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 54);
        entities.InterstateCase.CaseType = db.GetString(reader, 55);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 56);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 58);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 59);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 60);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 67);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 68);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 69);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 70);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 72);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 73);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 74);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 75);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 76);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 77);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 78);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 79);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 80);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 81);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 82);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 83);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 84);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 85);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 86);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 87);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 88);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 89);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 90);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 91);
        entities.CsenetTransactionEnvelop.Populated = true;
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelopInterstateApIdentification2()
  {
    entities.CsenetTransactionEnvelop.Populated = false;
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadCsenetTransactionEnvelopInterstateApIdentification2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "altSsn2", local.SiWageIncomeSourceRec.PersonSsn);
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 6);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 8);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 9);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 9);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 14);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 15);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 16);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 17);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 22);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 23);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 24);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 25);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 26);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 27);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 28);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 29);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 30);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 31);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 32);
        entities.InterstateCase.ActionCode = db.GetString(reader, 33);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 34);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 35);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 38);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 39);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 40);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 41);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 42);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 43);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 44);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 45);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 46);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 47);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 48);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 49);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 50);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 51);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 52);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 53);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 54);
        entities.InterstateCase.CaseType = db.GetString(reader, 55);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 56);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 58);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 59);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 60);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 67);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 68);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 69);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 70);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 72);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 73);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 74);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 75);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 76);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 77);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 78);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 79);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 80);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 81);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 82);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 83);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 84);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 85);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 86);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 87);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 88);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 89);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 90);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 91);
        entities.CsenetTransactionEnvelop.Populated = true;
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelopInterstateApIdentification3()
  {
    entities.CsenetTransactionEnvelop.Populated = false;
    entities.InterstateApIdentification.Populated = false;
    entities.InterstateCase.Populated = false;

    return Read("ReadCsenetTransactionEnvelopInterstateApIdentification3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ssn", local.SiWageIncomeSourceRec.PersonSsn);
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 6);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 8);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 8);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 9);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 9);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 14);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 15);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 16);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 17);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 22);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 23);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 24);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 25);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 26);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 27);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 28);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 29);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 30);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 31);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 32);
        entities.InterstateCase.ActionCode = db.GetString(reader, 33);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 34);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 35);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 38);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 39);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 40);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 41);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 42);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 43);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 44);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 45);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 46);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 47);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 48);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 49);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 50);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 51);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 52);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 53);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 54);
        entities.InterstateCase.CaseType = db.GetString(reader, 55);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 56);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 58);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 59);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 60);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 67);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 68);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 69);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 70);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 72);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 73);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 74);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 75);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 76);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 77);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 78);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 79);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 80);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 81);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 82);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 83);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 84);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 85);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 86);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 87);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 88);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 89);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 90);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 91);
        entities.CsenetTransactionEnvelop.Populated = true;
        entities.InterstateApIdentification.Populated = true;
        entities.InterstateCase.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var processingStatusCode = "P";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Return1.
    /// </summary>
    [JsonPropertyName("return1")]
    public External Return1
    {
      get => return1 ??= new();
      set => return1 = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCase New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
    }

    /// <summary>
    /// A value of SiWageIncomeSourceRec.
    /// </summary>
    [JsonPropertyName("siWageIncomeSourceRec")]
    public SiWageIncomeSourceRec SiWageIncomeSourceRec
    {
      get => siWageIncomeSourceRec ??= new();
      set => siWageIncomeSourceRec = value;
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
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of AbendError.
    /// </summary>
    [JsonPropertyName("abendError")]
    public Common AbendError
    {
      get => abendError ??= new();
      set => abendError = value;
    }

    /// <summary>
    /// A value of CreateApLocate.
    /// </summary>
    [JsonPropertyName("createApLocate")]
    public Common CreateApLocate
    {
      get => createApLocate ??= new();
      set => createApLocate = value;
    }

    /// <summary>
    /// A value of InterstateCaseCreates.
    /// </summary>
    [JsonPropertyName("interstateCaseCreates")]
    public Common InterstateCaseCreates
    {
      get => interstateCaseCreates ??= new();
      set => interstateCaseCreates = value;
    }

    /// <summary>
    /// A value of ApIdentCreates.
    /// </summary>
    [JsonPropertyName("apIdentCreates")]
    public Common ApIdentCreates
    {
      get => apIdentCreates ??= new();
      set => apIdentCreates = value;
    }

    /// <summary>
    /// A value of ZdelLocalRstrtOutputErrRec.
    /// </summary>
    [JsonPropertyName("zdelLocalRstrtOutputErrRec")]
    public Common ZdelLocalRstrtOutputErrRec
    {
      get => zdelLocalRstrtOutputErrRec ??= new();
      set => zdelLocalRstrtOutputErrRec = value;
    }

    /// <summary>
    /// A value of CheckpointUpdates.
    /// </summary>
    [JsonPropertyName("checkpointUpdates")]
    public Common CheckpointUpdates
    {
      get => checkpointUpdates ??= new();
      set => checkpointUpdates = value;
    }

    /// <summary>
    /// A value of CheckpointReads.
    /// </summary>
    [JsonPropertyName("checkpointReads")]
    public Common CheckpointReads
    {
      get => checkpointReads ??= new();
      set => checkpointReads = value;
    }

    /// <summary>
    /// A value of ZdelLocalRstrtInputFileRec.
    /// </summary>
    [JsonPropertyName("zdelLocalRstrtInputFileRec")]
    public Common ZdelLocalRstrtInputFileRec
    {
      get => zdelLocalRstrtInputFileRec ??= new();
      set => zdelLocalRstrtInputFileRec = value;
    }

    /// <summary>
    /// A value of NumberOfRecsWritten.
    /// </summary>
    [JsonPropertyName("numberOfRecsWritten")]
    public Common NumberOfRecsWritten
    {
      get => numberOfRecsWritten ??= new();
      set => numberOfRecsWritten = value;
    }

    /// <summary>
    /// A value of DhrInputFileRecsRead.
    /// </summary>
    [JsonPropertyName("dhrInputFileRecsRead")]
    public Common DhrInputFileRecsRead
    {
      get => dhrInputFileRecsRead ??= new();
      set => dhrInputFileRecsRead = value;
    }

    /// <summary>
    /// A value of TxnFound.
    /// </summary>
    [JsonPropertyName("txnFound")]
    public Common TxnFound
    {
      get => txnFound ??= new();
      set => txnFound = value;
    }

    /// <summary>
    /// A value of TotalTxnAe.
    /// </summary>
    [JsonPropertyName("totalTxnAe")]
    public Common TotalTxnAe
    {
      get => totalTxnAe ??= new();
      set => totalTxnAe = value;
    }

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private External return1;
    private InterstateCase new1;
    private ExitStateWorkArea exitStateWorkArea;
    private Common debug;
    private SiWageIncomeSourceRec siWageIncomeSourceRec;
    private External external;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend open;
    private EabReportSend write;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea current;
    private Common abendError;
    private Common createApLocate;
    private Common interstateCaseCreates;
    private Common apIdentCreates;
    private Common zdelLocalRstrtOutputErrRec;
    private Common checkpointUpdates;
    private Common checkpointReads;
    private Common zdelLocalRstrtInputFileRec;
    private Common numberOfRecsWritten;
    private Common dhrInputFileRecsRead;
    private Common txnFound;
    private Common totalTxnAe;
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
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
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

    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateApIdentification interstateApIdentification;
    private InterstateCase interstateCase;
  }
#endregion
}

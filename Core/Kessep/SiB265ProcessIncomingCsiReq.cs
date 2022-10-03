// Program: SI_B265_PROCESS_INCOMING_CSI_REQ, ID: 371085502, model: 746.
// Short name: SWEI265B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B265_PROCESS_INCOMING_CSI_REQ.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB265ProcessIncomingCsiReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B265_PROCESS_INCOMING_CSI_REQ program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB265ProcessIncomingCsiReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB265ProcessIncomingCsiReq.
  /// </summary>
  public SiB265ProcessIncomingCsiReq(IContext context, Import import,
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
    //                    M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------
    // 04/12/2001	M Ramirez	WR# 287		Initial Development
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB265Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    do
    {
      if (local.CkptRead.Count > 0)
      {
        local.TotRead.Count += local.CkptRead.Count;
        local.TotProcessed.Count += local.CkptProcessed.Count;
        local.TotErred.Count += local.CkptErred.Count;
        local.TotRollbacks.Count += local.CkptRollbacks.Count;
        local.TotFsinf.Count += local.CkptFsinf.Count;
        local.TotFuinfFv.Count += local.CkptFuinfFv.Count;
        local.TotFuinfInvalidCase.Count += local.CkptFuinfInvalidCase.Count;
        local.TotFuinfMissingCase.Count += local.CkptFuinfMissingCase.Count;
        local.TotTransaction.Count += local.CkptTransaction.Count;
        local.TotCaseDb.Count += local.CkptCaseDb.Count;
        local.TotApidDb.Count += local.CkptApidDb.Count;
        local.TotApLocateDb.Count += local.CkptApLocateDb.Count;
        local.TotParticipantDb.Count += local.CkptParticipantDb.Count;
        local.TotOrderDb.Count += local.CkptOrderDb.Count;
        local.TotMiscDb.Count += local.CkptMiscDb.Count;
        local.CkptRead.Count = 0;
        local.CkptProcessed.Count = 0;
        local.CkptErred.Count = 0;
        local.CkptRollbacks.Count = 0;
        local.CkptUpdate.Count = 0;
        local.CkptFsinf.Count = 0;
        local.CkptFuinfFv.Count = 0;
        local.CkptFuinfInvalidCase.Count = 0;
        local.CkptFuinfMissingCase.Count = 0;
        local.CkptTransaction.Count = 0;
        local.CkptCaseDb.Count = 0;
        local.CkptApidDb.Count = 0;
        local.CkptApLocateDb.Count = 0;
        local.CkptParticipantDb.Count = 0;
        local.CkptOrderDb.Count = 0;
        local.CkptMiscDb.Count = 0;
      }

      foreach(var item in ReadCsenetTransactionEnvelopInterstateCase())
      {
        ++local.CkptRead.Count;

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "DEBUG:  Read transaction, Txn = " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        UseSiProcessIncomingCsiRequest();

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";

          if (local.Created.TransSerialNumber > 0 && Lt
            (local.Null1.Date, local.Created.TransactionDate))
          {
            local.EabReportSend.RptDetail =
              "DEBUG:       Created outgoing transaction, Txn = " + NumberToString
              (local.Created.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
              (DateToInt(local.Created.TransactionDate), 8, 8);
          }
          else
          {
            local.EabReportSend.RptDetail =
              "DEBUG:       No outgoing transaction created.";
          }

          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (AsChar(local.WriteError.Flag) == 'Y')
        {
          ++local.CkptErred.Count;
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + ", Txn = " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "     EXITSTATE:  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          if (AsChar(local.AbendRollbackRequired.Flag) == 'A')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "     Error is with ABEND";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto AfterCycle;
          }

          if (AsChar(local.AbendRollbackRequired.Flag) == 'R')
          {
            ++local.CkptRollbacks.Count;
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "     Error is with ROLLBACK";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            UseEabRollbackSql();

            if (local.External.NumericReturnCode != 0)
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "     Unable to rollback database after error";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }

            // -----------------------------------------------
            // Mark record as erred
            // -----------------------------------------------
            try
            {
              UpdateCsenetTransactionEnvelop2();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "     ABEND:  Unable to update Transaction after Error,  Reason = Not Unique";
                    
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  goto AfterCycle;
                case ErrorCode.PermittedValueViolation:
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "     ABEND:  Unable to update Transaction after Error,  Reason = Permitted Value Violation";
                    
                  UseCabErrorReport();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                    return;
                  }

                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  goto AfterCycle;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            // -----------------------------------------------
            // Commit to keep the record marked as erred
            // -----------------------------------------------
            UseExtToDoACommit();

            if (local.External.NumericReturnCode != 0)
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "     Unable to Commit during Rollback";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            }

            // -----------------------------------------------
            // Rollback processing completed, rollback
            // transaction has been updated and committed.
            // Restart the READ EACH
            // -----------------------------------------------
            goto Next;
          }

          local.CsenetTransactionEnvelop.ProcessingStatusCode = "E";
        }
        else
        {
          ++local.CkptProcessed.Count;
          local.CsenetTransactionEnvelop.ProcessingStatusCode = "P";
        }

        // -----------------------------------------------
        // Restart records and Abend records don't reach
        // this point.
        // Mark record as processed(P) or erred(E)
        // -----------------------------------------------
        try
        {
          UpdateCsenetTransactionEnvelop1();

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "DEBUG:       Processed transaction";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "     ABEND:  Unable to update Transaction after processing,  Reason = Not Unique";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            case ErrorCode.PermittedValueViolation:
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "     ABEND:  Unable to update Transaction after processing,  Reason = Permitted Value Violation";
                
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              goto AfterCycle;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (local.CkptRead.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
          .CkptUpdate.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // -----------------------------------------------
          // Perform a periodic Commit
          // -----------------------------------------------
          break;
        }
      }

      if (local.CkptRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CkptUpdate.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered performing a periodic Commit";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "DEBUG:       Committed after transaction";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }

Next:
      ;
    }
    while(local.CkptRead.Count != 0);

AfterCycle:

    // -----------------------------------------------
    // Record any remaining counts on the Control Report
    //      (only happens during ABENDs)
    // -----------------------------------------------
    if (local.CkptRead.Count > 0)
    {
      local.TotRead.Count += local.CkptRead.Count;
      local.TotProcessed.Count += local.CkptProcessed.Count;
      local.TotErred.Count += local.CkptErred.Count;
      local.TotRollbacks.Count += local.CkptRollbacks.Count;
      local.TotFsinf.Count += local.CkptFsinf.Count;
      local.TotFuinfFv.Count += local.CkptFuinfFv.Count;
      local.TotFuinfInvalidCase.Count += local.CkptFuinfInvalidCase.Count;
      local.TotFuinfMissingCase.Count += local.CkptFuinfMissingCase.Count;
      local.TotTransaction.Count += local.CkptTransaction.Count;
      local.TotCaseDb.Count += local.CkptCaseDb.Count;
      local.TotApidDb.Count += local.CkptApidDb.Count;
      local.TotApLocateDb.Count += local.CkptApLocateDb.Count;
      local.TotParticipantDb.Count += local.CkptParticipantDb.Count;
      local.TotOrderDb.Count += local.CkptOrderDb.Count;
      local.TotMiscDb.Count += local.CkptMiscDb.Count;
      local.CkptRead.Count = 0;
      local.CkptProcessed.Count = 0;
      local.CkptErred.Count = 0;
      local.CkptRollbacks.Count = 0;
      local.CkptUpdate.Count = 0;
      local.CkptFsinf.Count = 0;
      local.CkptFuinfFv.Count = 0;
      local.CkptFuinfInvalidCase.Count = 0;
      local.CkptFuinfMissingCase.Count = 0;
      local.CkptTransaction.Count = 0;
      local.CkptCaseDb.Count = 0;
      local.CkptApidDb.Count = 0;
      local.CkptApLocateDb.Count = 0;
      local.CkptParticipantDb.Count = 0;
      local.CkptOrderDb.Count = 0;
      local.CkptMiscDb.Count = 0;
    }

    // -----------------------------------------------------------
    // Write Control Report and close all files
    // -----------------------------------------------------------
    UseSiB265WriteControlsAndClose();

    // -----------------------------------------------------------
    // Close ADABAS
    // -----------------------------------------------------------
    UseSiCloseAdabas();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

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

  private void UseEabRollbackSql()
  {
    var useImport = new EabRollbackSql.Import();
    var useExport = new EabRollbackSql.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB265Housekeeping()
  {
    var useImport = new SiB265Housekeeping.Import();
    var useExport = new SiB265Housekeeping.Export();

    Call(SiB265Housekeeping.Execute, useImport, useExport);

    local.Max.Date = useExport.Max.Date;
    MoveDateWorkArea(useExport.Current, local.CurrentDate);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.ProgramProcessingInfo.Name = useExport.ProgramProcessingInfo.Name;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseSiB265WriteControlsAndClose()
  {
    var useImport = new SiB265WriteControlsAndClose.Import();
    var useExport = new SiB265WriteControlsAndClose.Export();

    useImport.TotRead.Count = local.TotRead.Count;
    useImport.TotProcessed.Count = local.TotProcessed.Count;
    useImport.TotErred.Count = local.TotErred.Count;
    useImport.TotRollbacks.Count = local.TotRollbacks.Count;
    useImport.TotMiscDb.Count = local.TotMiscDb.Count;
    useImport.TotOrderDb.Count = local.TotOrderDb.Count;
    useImport.TotParticipantDb.Count = local.TotParticipantDb.Count;
    useImport.TotApLocateDb.Count = local.TotApLocateDb.Count;
    useImport.TotApidDb.Count = local.TotApidDb.Count;
    useImport.TotCaseDb.Count = local.TotCaseDb.Count;
    useImport.TotFsinf.Count = local.TotFsinf.Count;
    useImport.TotTransaction.Count = local.TotTransaction.Count;
    useImport.TotFuinfFv.Count = local.TotFuinfFv.Count;
    useImport.TotFuinfInvalidCase.Count = local.TotFuinfInvalidCase.Count;
    useImport.TotFuinfMissingCase.Count = local.TotFuinfMissingCase.Count;

    Call(SiB265WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiProcessIncomingCsiRequest()
  {
    var useImport = new SiProcessIncomingCsiRequest.Import();
    var useExport = new SiProcessIncomingCsiRequest.Export();

    useImport.InterstateCase.Assign(entities.InterstateCase);
    useImport.ExpMiscDb.Count = local.CkptMiscDb.Count;
    useImport.ExpOrderDb.Count = local.CkptOrderDb.Count;
    useImport.ExpParticipantDb.Count = local.CkptParticipantDb.Count;
    useImport.ExpApLocateDb.Count = local.CkptApLocateDb.Count;
    useImport.ExpApidDb.Count = local.CkptApidDb.Count;
    useImport.ExpCaseDb.Count = local.CkptCaseDb.Count;
    useImport.ExpFsinf.Count = local.CkptFsinf.Count;
    useImport.ExpTransaction.Count = local.CkptTransaction.Count;
    useImport.ExpFuinfFv.Count = local.CkptFuinfFv.Count;
    useImport.ExpFuinfInvalidCase.Count = local.CkptFuinfInvalidCase.Count;
    useImport.ExpFuinfMissingCase.Count = local.CkptFuinfMissingCase.Count;
    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.CurrentDate.Date;
    useImport.ExpCheckpointUpdate.Count = local.CkptUpdate.Count;

    Call(SiProcessIncomingCsiRequest.Execute, useImport, useExport);

    local.CkptMiscDb.Count = useImport.ExpMiscDb.Count;
    local.CkptOrderDb.Count = useImport.ExpOrderDb.Count;
    local.CkptParticipantDb.Count = useImport.ExpParticipantDb.Count;
    local.CkptApLocateDb.Count = useImport.ExpApLocateDb.Count;
    local.CkptApidDb.Count = useImport.ExpApidDb.Count;
    local.CkptCaseDb.Count = useImport.ExpCaseDb.Count;
    local.CkptFsinf.Count = useImport.ExpFsinf.Count;
    local.CkptTransaction.Count = useImport.ExpTransaction.Count;
    local.CkptFuinfFv.Count = useImport.ExpFuinfFv.Count;
    local.CkptFuinfInvalidCase.Count = useImport.ExpFuinfInvalidCase.Count;
    local.CkptFuinfMissingCase.Count = useImport.ExpFuinfMissingCase.Count;
    local.CkptUpdate.Count = useImport.ExpCheckpointUpdate.Count;
    MoveInterstateCase(useExport.Created, local.Created);
    local.AbendRollbackRequired.Flag = useExport.AbendRollbackRequired.Flag;
    local.WriteError.Flag = useExport.WriteError.Flag;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
  }

  private IEnumerable<bool> ReadCsenetTransactionEnvelopInterstateCase()
  {
    entities.InterstateCase.Populated = false;
    entities.CsenetTransactionEnvelop.Populated = false;

    return ReadEach("ReadCsenetTransactionEnvelopInterstateCase",
      null,
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
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 6);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 7);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 8);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 9);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 11);
        entities.InterstateCase.ActionCode = db.GetString(reader, 12);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 13);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 14);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 15);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 16);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 17);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 18);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 23);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 25);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 26);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 29);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 30);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 31);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 32);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.CaseType = db.GetString(reader, 34);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 35);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 41);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 46);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 48);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 52);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 53);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 54);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 56);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 58);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 59);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 60);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 63);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 66);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 67);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 68);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 69);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 70);
        entities.InterstateCase.Populated = true;
        entities.CsenetTransactionEnvelop.Populated = true;

        return true;
      });
  }

  private void UpdateCsenetTransactionEnvelop1()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.CurrentDate.Timestamp;
    var processingStatusCode =
      local.CsenetTransactionEnvelop.ProcessingStatusCode;

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop1",
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

  private void UpdateCsenetTransactionEnvelop2()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.CurrentDate.Timestamp;
    var processingStatusCode = "E";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop2",
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public InterstateCase Created
    {
      get => created ??= new();
      set => created = value;
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
    /// A value of TotRead.
    /// </summary>
    [JsonPropertyName("totRead")]
    public Common TotRead
    {
      get => totRead ??= new();
      set => totRead = value;
    }

    /// <summary>
    /// A value of CkptRead.
    /// </summary>
    [JsonPropertyName("ckptRead")]
    public Common CkptRead
    {
      get => ckptRead ??= new();
      set => ckptRead = value;
    }

    /// <summary>
    /// A value of TotProcessed.
    /// </summary>
    [JsonPropertyName("totProcessed")]
    public Common TotProcessed
    {
      get => totProcessed ??= new();
      set => totProcessed = value;
    }

    /// <summary>
    /// A value of CkptProcessed.
    /// </summary>
    [JsonPropertyName("ckptProcessed")]
    public Common CkptProcessed
    {
      get => ckptProcessed ??= new();
      set => ckptProcessed = value;
    }

    /// <summary>
    /// A value of TotErred.
    /// </summary>
    [JsonPropertyName("totErred")]
    public Common TotErred
    {
      get => totErred ??= new();
      set => totErred = value;
    }

    /// <summary>
    /// A value of CkptErred.
    /// </summary>
    [JsonPropertyName("ckptErred")]
    public Common CkptErred
    {
      get => ckptErred ??= new();
      set => ckptErred = value;
    }

    /// <summary>
    /// A value of TotRollbacks.
    /// </summary>
    [JsonPropertyName("totRollbacks")]
    public Common TotRollbacks
    {
      get => totRollbacks ??= new();
      set => totRollbacks = value;
    }

    /// <summary>
    /// A value of CkptRollbacks.
    /// </summary>
    [JsonPropertyName("ckptRollbacks")]
    public Common CkptRollbacks
    {
      get => ckptRollbacks ??= new();
      set => ckptRollbacks = value;
    }

    /// <summary>
    /// A value of TotMiscDb.
    /// </summary>
    [JsonPropertyName("totMiscDb")]
    public Common TotMiscDb
    {
      get => totMiscDb ??= new();
      set => totMiscDb = value;
    }

    /// <summary>
    /// A value of TotOrderDb.
    /// </summary>
    [JsonPropertyName("totOrderDb")]
    public Common TotOrderDb
    {
      get => totOrderDb ??= new();
      set => totOrderDb = value;
    }

    /// <summary>
    /// A value of TotParticipantDb.
    /// </summary>
    [JsonPropertyName("totParticipantDb")]
    public Common TotParticipantDb
    {
      get => totParticipantDb ??= new();
      set => totParticipantDb = value;
    }

    /// <summary>
    /// A value of TotApLocateDb.
    /// </summary>
    [JsonPropertyName("totApLocateDb")]
    public Common TotApLocateDb
    {
      get => totApLocateDb ??= new();
      set => totApLocateDb = value;
    }

    /// <summary>
    /// A value of TotApidDb.
    /// </summary>
    [JsonPropertyName("totApidDb")]
    public Common TotApidDb
    {
      get => totApidDb ??= new();
      set => totApidDb = value;
    }

    /// <summary>
    /// A value of TotCaseDb.
    /// </summary>
    [JsonPropertyName("totCaseDb")]
    public Common TotCaseDb
    {
      get => totCaseDb ??= new();
      set => totCaseDb = value;
    }

    /// <summary>
    /// A value of TotFsinf.
    /// </summary>
    [JsonPropertyName("totFsinf")]
    public Common TotFsinf
    {
      get => totFsinf ??= new();
      set => totFsinf = value;
    }

    /// <summary>
    /// A value of TotTransaction.
    /// </summary>
    [JsonPropertyName("totTransaction")]
    public Common TotTransaction
    {
      get => totTransaction ??= new();
      set => totTransaction = value;
    }

    /// <summary>
    /// A value of TotFuinfFv.
    /// </summary>
    [JsonPropertyName("totFuinfFv")]
    public Common TotFuinfFv
    {
      get => totFuinfFv ??= new();
      set => totFuinfFv = value;
    }

    /// <summary>
    /// A value of TotFuinfInvalidCase.
    /// </summary>
    [JsonPropertyName("totFuinfInvalidCase")]
    public Common TotFuinfInvalidCase
    {
      get => totFuinfInvalidCase ??= new();
      set => totFuinfInvalidCase = value;
    }

    /// <summary>
    /// A value of TotFuinfMissingCase.
    /// </summary>
    [JsonPropertyName("totFuinfMissingCase")]
    public Common TotFuinfMissingCase
    {
      get => totFuinfMissingCase ??= new();
      set => totFuinfMissingCase = value;
    }

    /// <summary>
    /// A value of CkptMiscDb.
    /// </summary>
    [JsonPropertyName("ckptMiscDb")]
    public Common CkptMiscDb
    {
      get => ckptMiscDb ??= new();
      set => ckptMiscDb = value;
    }

    /// <summary>
    /// A value of CkptOrderDb.
    /// </summary>
    [JsonPropertyName("ckptOrderDb")]
    public Common CkptOrderDb
    {
      get => ckptOrderDb ??= new();
      set => ckptOrderDb = value;
    }

    /// <summary>
    /// A value of CkptParticipantDb.
    /// </summary>
    [JsonPropertyName("ckptParticipantDb")]
    public Common CkptParticipantDb
    {
      get => ckptParticipantDb ??= new();
      set => ckptParticipantDb = value;
    }

    /// <summary>
    /// A value of CkptApLocateDb.
    /// </summary>
    [JsonPropertyName("ckptApLocateDb")]
    public Common CkptApLocateDb
    {
      get => ckptApLocateDb ??= new();
      set => ckptApLocateDb = value;
    }

    /// <summary>
    /// A value of CkptApidDb.
    /// </summary>
    [JsonPropertyName("ckptApidDb")]
    public Common CkptApidDb
    {
      get => ckptApidDb ??= new();
      set => ckptApidDb = value;
    }

    /// <summary>
    /// A value of CkptCaseDb.
    /// </summary>
    [JsonPropertyName("ckptCaseDb")]
    public Common CkptCaseDb
    {
      get => ckptCaseDb ??= new();
      set => ckptCaseDb = value;
    }

    /// <summary>
    /// A value of CkptFsinf.
    /// </summary>
    [JsonPropertyName("ckptFsinf")]
    public Common CkptFsinf
    {
      get => ckptFsinf ??= new();
      set => ckptFsinf = value;
    }

    /// <summary>
    /// A value of CkptTransaction.
    /// </summary>
    [JsonPropertyName("ckptTransaction")]
    public Common CkptTransaction
    {
      get => ckptTransaction ??= new();
      set => ckptTransaction = value;
    }

    /// <summary>
    /// A value of CkptFuinfFv.
    /// </summary>
    [JsonPropertyName("ckptFuinfFv")]
    public Common CkptFuinfFv
    {
      get => ckptFuinfFv ??= new();
      set => ckptFuinfFv = value;
    }

    /// <summary>
    /// A value of CkptFuinfInvalidCase.
    /// </summary>
    [JsonPropertyName("ckptFuinfInvalidCase")]
    public Common CkptFuinfInvalidCase
    {
      get => ckptFuinfInvalidCase ??= new();
      set => ckptFuinfInvalidCase = value;
    }

    /// <summary>
    /// A value of CkptFuinfMissingCase.
    /// </summary>
    [JsonPropertyName("ckptFuinfMissingCase")]
    public Common CkptFuinfMissingCase
    {
      get => ckptFuinfMissingCase ??= new();
      set => ckptFuinfMissingCase = value;
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
    /// A value of CurrentDate.
    /// </summary>
    [JsonPropertyName("currentDate")]
    public DateWorkArea CurrentDate
    {
      get => currentDate ??= new();
      set => currentDate = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

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
    /// A value of AbendRollbackRequired.
    /// </summary>
    [JsonPropertyName("abendRollbackRequired")]
    public Common AbendRollbackRequired
    {
      get => abendRollbackRequired ??= new();
      set => abendRollbackRequired = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
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
    /// A value of CkptUpdate.
    /// </summary>
    [JsonPropertyName("ckptUpdate")]
    public Common CkptUpdate
    {
      get => ckptUpdate ??= new();
      set => ckptUpdate = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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

    private DateWorkArea null1;
    private InterstateCase created;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totRead;
    private Common ckptRead;
    private Common totProcessed;
    private Common ckptProcessed;
    private Common totErred;
    private Common ckptErred;
    private Common totRollbacks;
    private Common ckptRollbacks;
    private Common totMiscDb;
    private Common totOrderDb;
    private Common totParticipantDb;
    private Common totApLocateDb;
    private Common totApidDb;
    private Common totCaseDb;
    private Common totFsinf;
    private Common totTransaction;
    private Common totFuinfFv;
    private Common totFuinfInvalidCase;
    private Common totFuinfMissingCase;
    private Common ckptMiscDb;
    private Common ckptOrderDb;
    private Common ckptParticipantDb;
    private Common ckptApLocateDb;
    private Common ckptApidDb;
    private Common ckptCaseDb;
    private Common ckptFsinf;
    private Common ckptTransaction;
    private Common ckptFuinfFv;
    private Common ckptFuinfInvalidCase;
    private Common ckptFuinfMissingCase;
    private DateWorkArea max;
    private DateWorkArea currentDate;
    private Common debugOn;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private Common abendRollbackRequired;
    private Common writeError;
    private External external;
    private Common ckptUpdate;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
  }
#endregion
}

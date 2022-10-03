// Program: SI_B292_RESET_ERRED_CSENET_TRANS, ID: 373440512, model: 746.
// Short name: SWEI292B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B292_RESET_ERRED_CSENET_TRANS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB292ResetErredCsenetTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B292_RESET_ERRED_CSENET_TRANS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB292ResetErredCsenetTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB292ResetErredCsenetTrans.
  /// </summary>
  public SiB292ResetErredCsenetTrans(IContext context, Import import,
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
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // ------------------------------------------------------------
    // 05/24/2002	M Ramirez	WR010502 Seg C	Init Dev
    // ------------------------------------------------------------
    // ------------------------------------------------------------
    // Processing Description
    // ------------------------------------------------------------
    // This batch re-processes csenet transactions that had erred
    // the first time they were sent.  The PPI record defines which
    // error codes will be re-processed in housekeeping.  The data
    // is retrieved and it is determined whether the transaction
    // can be sent again in reset_erred_csenet_tran.  The PrAD
    // updates the csenet_trans_envelop appropriately.  The control
    // report and external files are closed in write_controls_and_close
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB292Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.Batch.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";

    // ---------------------------------------------------
    // Process one error code at a time
    // ---------------------------------------------------
    local.ErrorCodes.Index = 0;

    for(var limit = local.ErrorCodes.Count; local.ErrorCodes.Index < limit; ++
      local.ErrorCodes.Index)
    {
      if (!local.ErrorCodes.CheckSize())
      {
        break;
      }

      foreach(var item in ReadCsenetTransactionEnvelopInterstateCase())
      {
        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "DEBUG:  Read Transaction; Txn No = " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        ++local.CheckpointReads.Count;
        ++local.ControlReads.Count;
        UseSiResetErredCsenetTran();

        // -----------------------------------------------------
        // Initialize control counts
        // -----------------------------------------------------
        if (!IsEmpty(local.Controls.FunctionalTypeCode))
        {
          if (local.Totals.Count > 0)
          {
            for(local.Totals.Index = 0; local.Totals.Index < local
              .Totals.Count; ++local.Totals.Index)
            {
              if (!local.Totals.CheckSize())
              {
                break;
              }

              if (Equal(local.Totals.Item.GlocalControls.FunctionalTypeCode,
                local.Controls.FunctionalTypeCode) && AsChar
                (local.Totals.Item.GlocalControls.ActionCode) == AsChar
                (local.Controls.ActionCode))
              {
                break;
              }
            }

            local.Totals.CheckIndex();

            if (!Equal(local.Totals.Item.GlocalControls.FunctionalTypeCode,
              local.Controls.FunctionalTypeCode) || AsChar
              (local.Totals.Item.GlocalControls.ActionCode) != AsChar
              (local.Controls.ActionCode))
            {
              local.Totals.Index = local.Totals.Count;
              local.Totals.CheckSize();

              MoveInterstateCase2(local.Controls,
                local.Totals.Update.GlocalControls);
            }
          }
          else
          {
            local.Totals.Index = 0;
            local.Totals.CheckSize();

            MoveInterstateCase2(local.Controls,
              local.Totals.Update.GlocalControls);
          }

          local.Totals.Update.GlocalReads.Count =
            local.Totals.Item.GlocalReads.Count + 1;

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "DEBUG:     Control category = " + local
              .Controls.FunctionalTypeCode + "-" + local.Controls.ActionCode + ", Reason = " +
              (local.Controls.ActionReasonCode ?? "");
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }

        if (AsChar(local.WriteError.Flag) != 'Y')
        {
          // -----------------------------------------------------
          // No error.  Update envelope so that it will be re-sent
          // -----------------------------------------------------
          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "DEBUG:     No errors, update CSENET_TRANS_ENVELOP to S";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.DebugOn.Flag) == 'Y')
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "DEBUG:     EXITSTATE:  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          local.CsenetTransactionEnvelop.ProcessingStatusCode = "S";
          local.CsenetTransactionEnvelop.ErrorCode = "";
          local.CsenetTransactionEnvelop.LastUpdatedBy =
            local.ProgramProcessingInfo.Name;
          local.CsenetTransactionEnvelop.LastUpdatedTimestamp =
            local.Current.Timestamp;
          UseSiUpdateCsenetTransEnvelop();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------------
            // Critical error.  Write error and ABEND
            // -----------------------------------------------------
            local.WriteError.Flag = "Y";
            local.TerminatingError.Flag = "A";
            local.Error.RptDetail =
              "Error occurred updating CSENET_TRANS_ENVELOP after successfully processing transaction";
              
          }
        }
        else if (IsEmpty(local.TerminatingError.Flag))
        {
          // -----------------------------------------------------
          // Non critical error.  Update envelope error code
          // -----------------------------------------------------
          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "DEBUG:     Non critical error, update CSENET_TRANS_ENVELOP to E";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (AsChar(local.DebugOn.Flag) == 'Y')
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "DEBUG:     EXITSTATE:  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }

          local.CsenetTransactionEnvelop.ProcessingStatusCode = "E";
          local.CsenetTransactionEnvelop.ErrorCode = "FAIL";
          local.CsenetTransactionEnvelop.LastUpdatedBy =
            local.ProgramProcessingInfo.Name;
          local.CsenetTransactionEnvelop.LastUpdatedTimestamp =
            local.Current.Timestamp;
          UseSiUpdateCsenetTransEnvelop();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -----------------------------------------------------
            // Critical error.  Write error and ABEND
            // -----------------------------------------------------
            local.WriteError.Flag = "Y";
            local.TerminatingError.Flag = "A";
            local.Error.RptDetail =
              "Error occurred updating CSENET_TRANS_ENVELOP after non critical error occurred processing transaction";
              
          }
        }
        else
        {
          // -----------------------------------------------------
          // Critical error returned from CAB.  Write error and ABEND
          // -----------------------------------------------------
          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "DEBUG:     Critical error, no update to CSENET_TRANS_ENVELOP";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }
        }

        if (AsChar(local.WriteError.Flag) == 'Y')
        {
          ++local.ControlErrors.Count;
          local.Totals.Update.GlocalErrors.Count =
            local.Totals.Item.GlocalErrors.Count + 1;

          // ------------------------------------------------------------
          // Write message with identifiers
          // -----------------------------------------------------------
          local.EabReportSend.RptDetail =
            "Error occurred processing transaction; Txn No = " + NumberToString
            (entities.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
            (DateToInt(entities.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // ------------------------------------------------------------
          // Now write error message
          // -----------------------------------------------------------
          local.EabReportSend.RptDetail = "     " + local.Error.RptDetail;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          // ------------------------------------------------------------
          // Extract exitstate message if there is one
          // -----------------------------------------------------------
          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
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

          if (!IsEmpty(local.TerminatingError.Flag))
          {
            local.EabReportSend.RptDetail = "     Error caused an ABEND";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            goto AfterCycle;
          }

          ++local.CheckpointUpdates.Count;
        }
        else
        {
          // -----------------------------------------------------
          // Everything went fine.  Update control counts
          // -----------------------------------------------------
          ++local.ControlProcessed.Count;
          local.Totals.Update.GlocalProcessed.Count =
            local.Totals.Item.GlocalProcessed.Count + 1;

          // -----------------------------------------
          // Sub 1 = Created AP ID datablocks
          // ------------------------------------------
          local.Totals.Item.SubGroupTotals.Index = 0;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.ApIdentificationInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 2 = Created AP Locate datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.ApLocateDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 3 = Created Participant datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.ParticipantDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 4 = Created Order datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.OrderDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 5 = Created Collection datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.CollectionDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 6 = Created Information datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Creates.InformationInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 7 = Updated Case datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.CaseDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 8 = Updated AP ID datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.ApIdentificationInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 9 = Updated AP Locate datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.ApLocateDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 10 = Updated Participant datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.ParticipantDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 11 = Updated Order datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.OrderDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 12 = Updated Collection datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.CollectionDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 13 = Updated Information datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Updates.InformationInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 14 = Deleted AP ID datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.ApIdentificationInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 15 = Deleted AP Locate datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.ApLocateDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 16 = Deleted Participant datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.ParticipantDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 17 = Deleted Order datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.OrderDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 18 = Deleted Collection datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.CollectionDataInd.GetValueOrDefault();

          // -----------------------------------------
          // Sub 19 = Deleted Information datablocks
          // ------------------------------------------
          ++local.Totals.Item.SubGroupTotals.Index;
          local.Totals.Item.SubGroupTotals.CheckSize();

          local.Totals.Update.SubGroupTotals.Update.GlocalSub.Count =
            local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count + local
            .Deletes.InformationInd.GetValueOrDefault();

          // ------------------------------------------------------
          // Set checkpoint updates to the max of all the create,
          // update, and delete counts returned from the CAB
          // ------------------------------------------------------
          for(local.Totals.Item.SubGroupTotals.Index = 0; local
            .Totals.Item.SubGroupTotals.Index < local
            .Totals.Item.SubGroupTotals.Count; ++
            local.Totals.Item.SubGroupTotals.Index)
          {
            if (!local.Totals.Item.SubGroupTotals.CheckSize())
            {
              break;
            }

            if (local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count > local
              .Temp.Count)
            {
              local.Temp.Count =
                local.Totals.Item.SubGroupTotals.Item.GlocalSub.Count;
            }
          }

          local.Totals.Item.SubGroupTotals.CheckIndex();
          local.CheckpointUpdates.Count += local.Temp.Count;
        }

        if (local.CheckpointReads.Count >= local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
          .CheckpointUpdates.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error encountered performing a Commit.";
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "DEBUG:     Commit performed";
            UseCabErrorReport();

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
    }

AfterCycle:

    local.ErrorCodes.CheckIndex();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (local.CheckpointUpdates.Count > 0)
      {
        UseExtToDoACommit();

        if (local.External.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error encountered performing a Commit.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "DEBUG:     Commit performed at end of job";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
      else if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "DEBUG:     Commit NOT performed at end of job because no updates occurred";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    UseSiB292WriteControlsAndClose();

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

  private static void MoveErrorCodes(SiB292Housekeeping.Export.
    ErrorCodesGroup source, Local.ErrorCodesGroup target)
  {
    target.G.ErrorCode = source.G.ErrorCode;
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSubGroupTotals(Local.SubGroupTotalsGroup source,
    SiB292WriteControlsAndClose.Import.SubGroupTotalsGroup target)
  {
    target.GimportSub.Count = source.GlocalSub.Count;
  }

  private static void MoveTotals(Local.TotalsGroup source,
    SiB292WriteControlsAndClose.Import.TotalsGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    source.SubGroupTotals.CopyTo(target.SubGroupTotals, MoveSubGroupTotals);
    MoveInterstateCase2(source.GlocalControls, target.GimportControls);
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB292Housekeeping()
  {
    var useImport = new SiB292Housekeeping.Import();
    var useExport = new SiB292Housekeeping.Export();

    Call(SiB292Housekeeping.Execute, useImport, useExport);

    local.DebugOn.Flag = useExport.DebugOn.Flag;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveDateWorkArea(useExport.Current, local.Current);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.Cutoff.Date = useExport.Cutoff.Date;
    useExport.ErrorCodes.CopyTo(local.ErrorCodes, MoveErrorCodes);
  }

  private void UseSiB292WriteControlsAndClose()
  {
    var useImport = new SiB292WriteControlsAndClose.Import();
    var useExport = new SiB292WriteControlsAndClose.Export();

    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Read.Count = local.ControlReads.Count;
    useImport.Processed.Count = local.ControlProcessed.Count;
    useImport.Erred.Count = local.ControlErrors.Count;
    local.Totals.CopyTo(useImport.Totals, MoveTotals);

    Call(SiB292WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSiResetErredCsenetTran()
  {
    var useImport = new SiResetErredCsenetTran.Import();
    var useExport = new SiResetErredCsenetTran.Export();

    MoveInterstateCase1(entities.InterstateCase, useImport.InterstateCase);
    useImport.Current.Date = local.Current.Date;
    useImport.Batch.Flag = local.Batch.Flag;

    Call(SiResetErredCsenetTran.Execute, useImport, useExport);

    local.WriteError.Flag = useExport.WriteError.Flag;
    local.Error.RptDetail = useExport.Error.RptDetail;
    local.TerminatingError.Flag = useExport.AbendRollbackRequired.Flag;
    local.Controls.Assign(useExport.Controls);
    local.Deletes.Assign(useExport.Deletes);
    local.Updates.Assign(useExport.Updates);
    local.Creates.Assign(useExport.Creates);
  }

  private void UseSiUpdateCsenetTransEnvelop()
  {
    var useImport = new SiUpdateCsenetTransEnvelop.Import();
    var useExport = new SiUpdateCsenetTransEnvelop.Export();

    useImport.CsenetTransactionEnvelop.Assign(local.CsenetTransactionEnvelop);
    MoveInterstateCase1(entities.InterstateCase, useImport.InterstateCase);

    Call(SiUpdateCsenetTransEnvelop.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCsenetTransactionEnvelopInterstateCase()
  {
    entities.InterstateCase.Populated = false;
    entities.CsenetTransactionEnvelop.Populated = false;

    return ReadEach("ReadCsenetTransactionEnvelopInterstateCase",
      (db, command) =>
      {
        db.SetNullableString(
          command, "errorCode", local.ErrorCodes.Item.G.ErrorCode ?? "");
        db.SetDate(
          command, "ccaTransactionDt", local.Cutoff.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 3);
        entities.CsenetTransactionEnvelop.ErrorCode =
          db.GetNullableString(reader, 4);
        entities.InterstateCase.Populated = true;
        entities.CsenetTransactionEnvelop.Populated = true;

        return true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of GlocalControls.
      /// </summary>
      [JsonPropertyName("glocalControls")]
      public InterstateCase GlocalControls
      {
        get => glocalControls ??= new();
        set => glocalControls = value;
      }

      /// <summary>
      /// A value of GlocalReads.
      /// </summary>
      [JsonPropertyName("glocalReads")]
      public Common GlocalReads
      {
        get => glocalReads ??= new();
        set => glocalReads = value;
      }

      /// <summary>
      /// A value of GlocalProcessed.
      /// </summary>
      [JsonPropertyName("glocalProcessed")]
      public Common GlocalProcessed
      {
        get => glocalProcessed ??= new();
        set => glocalProcessed = value;
      }

      /// <summary>
      /// A value of GlocalErrors.
      /// </summary>
      [JsonPropertyName("glocalErrors")]
      public Common GlocalErrors
      {
        get => glocalErrors ??= new();
        set => glocalErrors = value;
      }

      /// <summary>
      /// Gets a value of SubGroupTotals.
      /// </summary>
      [JsonIgnore]
      public Array<SubGroupTotalsGroup> SubGroupTotals =>
        subGroupTotals ??= new(SubGroupTotalsGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of SubGroupTotals for json serialization.
      /// </summary>
      [JsonPropertyName("subGroupTotals")]
      [Computed]
      public IList<SubGroupTotalsGroup> SubGroupTotals_Json
      {
        get => subGroupTotals;
        set => SubGroupTotals.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private InterstateCase glocalControls;
      private Common glocalReads;
      private Common glocalProcessed;
      private Common glocalErrors;
      private Array<SubGroupTotalsGroup> subGroupTotals;
    }

    /// <summary>A SubGroupTotalsGroup group.</summary>
    [Serializable]
    public class SubGroupTotalsGroup
    {
      /// <summary>
      /// A value of GlocalSub.
      /// </summary>
      [JsonPropertyName("glocalSub")]
      public Common GlocalSub
      {
        get => glocalSub ??= new();
        set => glocalSub = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common glocalSub;
    }

    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsenetTransactionEnvelop G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsenetTransactionEnvelop g;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// Gets a value of Totals.
    /// </summary>
    [JsonIgnore]
    public Array<TotalsGroup> Totals => totals ??= new(TotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Totals for json serialization.
    /// </summary>
    [JsonPropertyName("totals")]
    [Computed]
    public IList<TotalsGroup> Totals_Json
    {
      get => totals;
      set => Totals.Assign(value);
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public EabReportSend Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Creates.
    /// </summary>
    [JsonPropertyName("creates")]
    public InterstateCase Creates
    {
      get => creates ??= new();
      set => creates = value;
    }

    /// <summary>
    /// A value of Updates.
    /// </summary>
    [JsonPropertyName("updates")]
    public InterstateCase Updates
    {
      get => updates ??= new();
      set => updates = value;
    }

    /// <summary>
    /// A value of Deletes.
    /// </summary>
    [JsonPropertyName("deletes")]
    public InterstateCase Deletes
    {
      get => deletes ??= new();
      set => deletes = value;
    }

    /// <summary>
    /// A value of Controls.
    /// </summary>
    [JsonPropertyName("controls")]
    public InterstateCase Controls
    {
      get => controls ??= new();
      set => controls = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of Cutoff.
    /// </summary>
    [JsonPropertyName("cutoff")]
    public DateWorkArea Cutoff
    {
      get => cutoff ??= new();
      set => cutoff = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of ControlReads.
    /// </summary>
    [JsonPropertyName("controlReads")]
    public Common ControlReads
    {
      get => controlReads ??= new();
      set => controlReads = value;
    }

    /// <summary>
    /// A value of ControlProcessed.
    /// </summary>
    [JsonPropertyName("controlProcessed")]
    public Common ControlProcessed
    {
      get => controlProcessed ??= new();
      set => controlProcessed = value;
    }

    /// <summary>
    /// A value of ControlErrors.
    /// </summary>
    [JsonPropertyName("controlErrors")]
    public Common ControlErrors
    {
      get => controlErrors ??= new();
      set => controlErrors = value;
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
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of TerminatingError.
    /// </summary>
    [JsonPropertyName("terminatingError")]
    public Common TerminatingError
    {
      get => terminatingError ??= new();
      set => terminatingError = value;
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

    private Common temp;
    private ExitStateWorkArea exitStateWorkArea;
    private Array<TotalsGroup> totals;
    private EabReportSend error;
    private InterstateCase creates;
    private InterstateCase updates;
    private InterstateCase deletes;
    private InterstateCase controls;
    private Common batch;
    private DateWorkArea cutoff;
    private Array<ErrorCodesGroup> errorCodes;
    private DateWorkArea current;
    private Common debugOn;
    private Common controlReads;
    private Common controlProcessed;
    private Common controlErrors;
    private Common checkpointUpdates;
    private Common checkpointReads;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private Common terminatingError;
    private Common writeError;
    private External external;
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

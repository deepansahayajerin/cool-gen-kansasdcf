// Program: FN_B669_PROCESS_NEW_DEBTS, ID: 372277233, model: 746.
// Short name: SWEF669B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B669_PROCESS_NEW_DEBTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB669ProcessNewDebts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B669_PROCESS_NEW_DEBTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB669ProcessNewDebts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB669ProcessNewDebts.
  /// </summary>
  public FnB669ProcessNewDebts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ----------  ---------  ---------  
    // -------------------------------------
    // ***********************************************************************
    // ***  Possible reasons for a new debt:
    // 1.  The start date of the obligation is revised to an earlier date and 
    // retroactive debts are created.
    // 2.  Payment history is added and still some previous debts remain 
    // unprocessed.
    // 3.  An obligation with a retroactive start date is added.
    // ***   Possible scenarios for reversing collections from a given date 
    // forward. ***
    // 1.  All collections for one court order.
    // 2.  All collections for one obligation.
    // 3.  All collections for one obligor.
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramStart.Timestamp = Now();
    local.ReportNeeded.Flag = "Y";
    local.TypeOfChange.SelectChar = "D";
    UseFnB669Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE") || IsExitState
        ("PROGRAM_PROCESSING_INFO_NF"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport();
      }
    }
    else
    {
      local.Collection.CollectionAdjustmentReasonTxt =
        local.CollectionAdjustmentReason.Description ?? "";
      local.Collection.CollectionAdjustmentDt =
        local.ProgramProcessingInfo.ProcessDate;
      local.PreviousObligor.Number = "";

      if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
      {
        foreach(var item in ReadDebtDetailCsePersonDebt1())
        {
          if (Equal(entities.CsePerson.Number, local.PreviousObligor.Number))
          {
            // --- All the (subsequent) collections for the obligor have been 
            // reversed. But we still need to update DEBT as processed.
          }
          else
          {
            // ---------------------------------------------
            // Reverse all the distributed COLLECTIONs of this obligor that are:
            // 	- unadjusted
            // 	- created by batch distribution proc
            //            - collection date on/later than the new debt detail 
            // due date (if the collection was received after the due date).
            //                  OR
            //            - distributed debt detail due date on/ later than the 
            // new debt detail due date (if the collection was received before
            // the due date as a 'future' payment)
            // ---------------------------------------------
            // ***  Report the earliest new debt found ***
            local.EabFileHandling.Action = "WRITE";
            local.Date.Text10 =
              NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Day(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Year(entities.DebtDetail.DueDt), 12, 4);
            local.EabReportSend.RptDetail = "New Debt  " + "  " + entities
              .CsePerson.Number + "   " + local.Date.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();

            if (Equal(entities.DebtDetail.DueDt, local.Null1.Date))
            {
              continue;
            }

            // ***  Reverse collections beginning with the first of the month. *
            // **
            local.ProcessMonth.Count = Month(entities.DebtDetail.DueDt);
            local.ProcessYear.Count = Year(entities.DebtDetail.DueDt);
            local.CashReceiptDetail.CollectionDate =
              IntToDate((int)((long)local.ProcessYear.Count * 10000 + (
                long)local.ProcessMonth.Count * 100 + 1));
            local.PreviousObligor.Number = entities.CsePerson.Number;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
            UseFnCabReverseAllCshRcptDtls();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to read or update database.  Obligor number = " +
                (local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (IsExitState("FN0000_OBLIGOR_NF"))
              {
                ExitState = "ACO_NN0000_ALL_OK";
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "It is likely that the Joint and Several obligation has only one obligor.";
                  
                UseCabErrorReport();

                continue;
              }
              else if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
              {
                ExitState = "ACO_NN0000_ALL_OK";

                continue;
              }
              else
              {
                break;
              }
            }
          }

          UseFnMarkNewDbtProcessed();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              (local.CashReceiptDetail.ObligorPersonNumber ?? "");

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            break;
          }

          ++local.NewDebtsProcessed.Count;
          ++local.NoOfIncrementalUpdates.Count;

          if (local.NoOfIncrementalUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            // ***** Record the number of checkpoints and the last checkpoint 
            // time and
            // set the restart indicator to yes.
            // Also return the checkpoint frequency counts in case they
            // been changed since the last read.
            local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              entities.CsePerson.Number;
            UseUpdatePgmCheckpointRestart();
            local.EabFileHandling.Action = "WRITE";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabReportSend.RptDetail = "Error attempting to commit.";
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
              }
              else
              {
                local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                  + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                  (Now().Date.Day, 14, 2);
                local.Time.Text8 =
                  NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
                local.EabReportSend.RptDetail =
                  "Checkpoint Taken after person number: " + entities
                  .CsePerson.Number + " Date: " + local.Date.Text10 + " Time: " +
                  local.Time.Text8;
                UseCabControlReport();
                local.NoOfIncrementalUpdates.Count = 0;
              }
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Error attempting to take a checkpoint.";
              UseCabErrorReport();

              break;
            }
          }
        }
      }
      else
      {
        foreach(var item in ReadDebtDetailCsePersonDebt2())
        {
          if (Equal(entities.CsePerson.Number, local.PreviousObligor.Number))
          {
            // --- All the (subsequent) collections for the obligor have been 
            // reversed. But we still need to update DEBT as processed.
          }
          else
          {
            // ---------------------------------------------
            // Reverse all the distributed COLLECTIONs of this obligor that are:
            // 	- unadjusted
            // 	- created by batch distribution proc
            //            - collection date on/later than the new debt detail 
            // due date (if the collection was received after the due date).
            //                  OR
            //            - distributed debt detail due date on/ later than the 
            // new debt detail due date (if the collection was received before
            // the due date as a 'future' payment)
            // ---------------------------------------------
            // ***  Report the earliest new debt found ***
            local.EabFileHandling.Action = "WRITE";
            local.Date.Text10 =
              NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Day(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Year(entities.DebtDetail.DueDt), 12, 4);
            local.EabReportSend.RptDetail = "New Debt  " + "  " + entities
              .CsePerson.Number + "   " + local.Date.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();

            if (Equal(entities.DebtDetail.DueDt, local.Null1.Date))
            {
              continue;
            }

            // ***  Reverse collections beginning with the first of the month. *
            // **
            local.ProcessMonth.Count = Month(entities.DebtDetail.DueDt);
            local.ProcessYear.Count = Year(entities.DebtDetail.DueDt);
            local.CashReceiptDetail.CollectionDate =
              IntToDate((int)((long)local.ProcessYear.Count * 10000 + (
                long)local.ProcessMonth.Count * 100 + 1));
            local.PreviousObligor.Number = entities.CsePerson.Number;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
            UseFnCabReverseAllCshRcptDtls();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to read or update database.  Obligor number = " +
                (local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF"))
              {
                ExitState = "ACO_NN0000_ALL_OK";

                continue;
              }
              else
              {
                break;
              }
            }
          }

          UseFnMarkNewDbtProcessed();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              (local.CashReceiptDetail.ObligorPersonNumber ?? "");

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
            UseCabErrorReport();

            break;
          }

          ++local.NewDebtsProcessed.Count;
          ++local.NoOfIncrementalUpdates.Count;

          if (local.NoOfIncrementalUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            // ***** Record the number of checkpoints and the last checkpoint 
            // time and
            // set the restart indicator to yes.
            // Also return the checkpoint frequency counts in case they
            // been changed since the last read.
            local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo =
              entities.CsePerson.Number;
            UseUpdatePgmCheckpointRestart();
            local.EabFileHandling.Action = "WRITE";

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabReportSend.RptDetail = "Error attempting to commit.";
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                break;
              }
              else
              {
                local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                  + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                  (Now().Date.Day, 14, 2);
                local.Time.Text8 =
                  NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
                local.EabReportSend.RptDetail =
                  "Checkpoint Taken after person number: " + entities
                  .CsePerson.Number + " Date: " + local.Date.Text10 + " Time: " +
                  local.Time.Text8;
                UseCabControlReport();
                local.NoOfIncrementalUpdates.Count = 0;
              }
            }
            else
            {
              local.EabReportSend.RptDetail =
                "Error attempting to take a checkpoint.";
              UseCabErrorReport();

              break;
            }
          }
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB669Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB669Close();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.DebtType = source.DebtType;
    target.NewDebtProcessDate = source.NewDebtProcessDate;
  }

  private static void MoveProgramProcessingInfo1(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveProgramProcessingInfo2(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
    target.ParameterList = source.ParameterList;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB669Close()
  {
    var useImport = new FnB669Close.Import();
    var useExport = new FnB669Close.Export();

    useImport.ProgramCheckpointRestart.ProgramName =
      local.ProgramCheckpointRestart.ProgramName;
    useImport.NoCashRcptDtlReversed.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoCollectiosReversed.Count = local.NoCollectionsReversed.Count;
    useImport.NewDebtsProcessed.Count = local.NewDebtsProcessed.Count;

    Call(FnB669Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB669Housekeeping()
  {
    var useImport = new FnB669Housekeeping.Import();
    var useExport = new FnB669Housekeeping.Export();

    Call(FnB669Housekeeping.Execute, useImport, useExport);

    local.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.ReleasedStatus.SystemGeneratedIdentifier;
    local.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.SuspendedStatus.SystemGeneratedIdentifier;
    local.CollectionAdjustmentReason.Assign(useExport.RetroDebtAdjustment);
    local.RestartObligor.Number = useExport.Restart.Number;
    local.Max.Date = useExport.Max.Date;
    MoveProgramProcessingInfo2(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.DistributedStatus.SystemGeneratedIdentifier =
      useExport.DistributedStatus.SystemGeneratedIdentifier;
    local.RefundedStatus.SystemGeneratedIdentifier =
      useExport.Refunded.SystemGeneratedIdentifier;
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
  }

  private void UseFnCabReverseAllCshRcptDtls()
  {
    var useImport = new FnCabReverseAllCshRcptDtls.Import();
    var useExport = new FnCabReverseAllCshRcptDtls.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.InterfaceIndicator =
      local.CashReceiptSourceType.InterfaceIndicator;
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.ReleasedStatus.SystemGeneratedIdentifier =
      local.ReleasedStatus.SystemGeneratedIdentifier;
    useImport.DistributedStatus.SystemGeneratedIdentifier =
      local.DistributedStatus.SystemGeneratedIdentifier;
    useImport.RefundedStatus.SystemGeneratedIdentifier =
      local.RefundedStatus.SystemGeneratedIdentifier;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useImport.CashReceipt.SequentialNumber = local.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.Max.Date = local.Max.Date;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    MoveCollection(local.Collection, useImport.Collection);
    useImport.SuspendedStatus.SystemGeneratedIdentifier =
      local.SuspendedStatus.SystemGeneratedIdentifier;
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useExport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useExport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useExport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;

    Call(FnCabReverseAllCshRcptDtls.Execute, useImport, useExport);

    local.NoCollectionsReversed.Count = useImport.NoCollectionsReversed.Count;
    local.NoCashRecptDtlUpdated.Count = useImport.NoCashRecptDtlUpdated.Count;
    local.NoOfIncrementalUpdates.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCollectionsReversed.Count = useExport.NoCollectionsReversed.Count;
    local.NoCashRecptDtlUpdated.Count = useExport.NoCashRecptDtlUpdated.Count;
    local.NoOfIncrementalUpdates.Count = useExport.NoOfIncrementalUpdates.Count;
  }

  private void UseFnMarkNewDbtProcessed()
  {
    var useImport = new FnMarkNewDbtProcessed.Import();
    var useExport = new FnMarkNewDbtProcessed.Export();

    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Debt.Assign(entities.Debt);

    Call(FnMarkNewDbtProcessed.Execute, useImport, useExport);

    MoveObligationTransaction(useImport.Debt, entities.Debt);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadDebtDetailCsePersonDebt1()
  {
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadDebtDetailCsePersonDebt1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", local.PpiParameter.ObligorPersonNumber ?? "");
        db.SetNullableDate(
          command, "newDebtProcDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.Debt.Amount = db.GetDecimal(reader, 14);
        entities.Debt.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Debt.LastUpdatedTmst = db.GetNullableDateTime(reader, 16);
        entities.Debt.DebtType = db.GetString(reader, 17);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 18);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 19);
        entities.Debt.NewDebtProcessDate = db.GetNullableDate(reader, 20);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetailCsePersonDebt2()
  {
    entities.DebtDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.CsePerson.Populated = false;

    return ReadEach("ReadDebtDetailCsePersonDebt2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "newDebtProcDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.Debt.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.Debt.Type1 = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        entities.Debt.Amount = db.GetDecimal(reader, 14);
        entities.Debt.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.Debt.LastUpdatedTmst = db.GetNullableDateTime(reader, 16);
        entities.Debt.DebtType = db.GetString(reader, 17);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 18);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 19);
        entities.Debt.NewDebtProcessDate = db.GetNullableDate(reader, 20);
        entities.DebtDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("DebtType", entities.Debt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of BeginStmt.
    /// </summary>
    [JsonPropertyName("beginStmt")]
    public DateWorkArea BeginStmt
    {
      get => beginStmt ??= new();
      set => beginStmt = value;
    }

    private DateWorkArea beginStmt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
    }

    /// <summary>
    /// A value of DistributedStatus.
    /// </summary>
    [JsonPropertyName("distributedStatus")]
    public CashReceiptDetailStatus DistributedStatus
    {
      get => distributedStatus ??= new();
      set => distributedStatus = value;
    }

    /// <summary>
    /// A value of RefundedStatus.
    /// </summary>
    [JsonPropertyName("refundedStatus")]
    public CashReceiptDetailStatus RefundedStatus
    {
      get => refundedStatus ??= new();
      set => refundedStatus = value;
    }

    /// <summary>
    /// A value of NewDebtsProcessed.
    /// </summary>
    [JsonPropertyName("newDebtsProcessed")]
    public Common NewDebtsProcessed
    {
      get => newDebtsProcessed ??= new();
      set => newDebtsProcessed = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    /// <summary>
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
    }

    /// <summary>
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
    }

    /// <summary>
    /// A value of RestartObligor.
    /// </summary>
    [JsonPropertyName("restartObligor")]
    public CsePerson RestartObligor
    {
      get => restartObligor ??= new();
      set => restartObligor = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ProcessMonth.
    /// </summary>
    [JsonPropertyName("processMonth")]
    public Common ProcessMonth
    {
      get => processMonth ??= new();
      set => processMonth = value;
    }

    /// <summary>
    /// A value of ProcessYear.
    /// </summary>
    [JsonPropertyName("processYear")]
    public Common ProcessYear
    {
      get => processYear ??= new();
      set => processYear = value;
    }

    /// <summary>
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      ppiParameter = null;
      cashReceiptType = null;
      cashReceiptEvent = null;
      cashReceiptSourceType = null;
      typeOfChange = null;
      reportNeeded = null;
      releasedStatus = null;
      suspendedStatus = null;
      distributedStatus = null;
      refundedStatus = null;
      newDebtsProcessed = null;
      noCollectionsReversed = null;
      noCashRecptDtlUpdated = null;
      noOfIncrementalUpdates = null;
      cashReceipt = null;
      cashReceiptDetail = null;
      time = null;
      date = null;
      max = null;
      null1 = null;
      eabFileHandling = null;
      previousObligor = null;
      restartObligor = null;
      collectionAdjustmentReason = null;
      programProcessingInfo = null;
      programCheckpointRestart = null;
      collection = null;
      passArea = null;
      exitStateWorkArea = null;
      processMonth = null;
      processYear = null;
      programStart = null;
    }

    private CashReceiptDetail ppiParameter;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private Common typeOfChange;
    private Common reportNeeded;
    private CashReceiptDetailStatus releasedStatus;
    private CashReceiptDetailStatus suspendedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus refundedStatus;
    private Common newDebtsProcessed;
    private Common noCollectionsReversed;
    private Common noCashRecptDtlUpdated;
    private Common noOfIncrementalUpdates;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private TextWorkArea time;
    private TextWorkArea date;
    private EabReportSend eabReportSend;
    private DateWorkArea max;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private CsePerson previousObligor;
    private CsePerson restartObligor;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Collection collection;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private Common processMonth;
    private Common processYear;
    private DateWorkArea programStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAccount obligor;
    private Obligation obligation;
    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private CsePerson csePerson;
  }
#endregion
}

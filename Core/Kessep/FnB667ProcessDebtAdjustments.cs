// Program: FN_B667_PROCESS_DEBT_ADJUSTMENTS, ID: 372273621, model: 746.
// Short name: SWEF667B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B667_PROCESS_DEBT_ADJUSTMENTS.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This nightly batch procedure provides a means of removing all 
/// collections for a obligor that have previously been distributed to various
/// obligations back to the date of the retroactive cash receipt.  This will
/// allow the distribution policy to redistribute all collections, thus ensuring
/// that the distribution policy rules are followed correctly.
/// Note:  A retroactive collection is a collection whose CASH_RECEIPT_DETAIL 
/// COLLECTION_DATE is less than the current month.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB667ProcessDebtAdjustments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B667_PROCESS_DEBT_ADJUSTMENTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB667ProcessDebtAdjustments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB667ProcessDebtAdjustments.
  /// </summary>
  public FnB667ProcessDebtAdjustments(IContext context, Import import,
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
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 11/22/1999  Ed Lyman   PR# 81072  Use debt detail due date rather than
    //                                   
    // debt adjustment date as starting
    // point
    //                                   
    // for reversing collections.
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramStart.Timestamp = Now();
    local.ReportNeeded.Flag = "Y";
    local.TypeOfChange.SelectChar = "D";
    UseFnB667Housekeeping();

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

      // *** Add the entity obligation transaction relation to the read, so that
      // we can read the debt being adjusted.  This will allow us to sort by
      // debt due date, so that we can locate the earliest adjusted debt. ***
      if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
      {
        foreach(var item in ReadObligationTransactionDebtDetailCsePerson1())
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
            // ***  Report the earliest debt adjusted.  ***
            local.EabFileHandling.Action = "WRITE";
            local.Date.Text10 =
              NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Day(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Year(entities.DebtDetail.DueDt), 12, 4);
            local.EabReportSend.RptDetail = "Debt Adj  " + "  " + entities
              .CsePerson.Number + "   " + local.Date.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();

            if (Equal(entities.DebtDetail.DueDt, local.Null1.Date))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Debt Due Date is null.  Obligor number = " + (
                  local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();

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

          try
          {
            UpdateObligationTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OBLIG_TRANS_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.DebtAdjustmentsProcessed.Count;
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
      else
      {
        foreach(var item in ReadObligationTransactionDebtDetailCsePerson2())
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
            // ***  Report the earliest debt adjusted.  ***
            local.EabFileHandling.Action = "WRITE";
            local.Date.Text10 =
              NumberToString(Month(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Day(entities.DebtDetail.DueDt), 14, 2) + "-" + NumberToString
              (Year(entities.DebtDetail.DueDt), 12, 4);
            local.EabReportSend.RptDetail = "Debt Adj  " + "  " + entities
              .CsePerson.Number + "   " + local.Date.Text10 + " " + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();

            if (Equal(entities.DebtDetail.DueDt, local.Null1.Date))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Debt Due Date is null.  Obligor number = " + (
                  local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();

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

          try
          {
            UpdateObligationTransaction();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_OBLIG_TRANS_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_OBLIG_TRANS_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          ++local.DebtAdjustmentsProcessed.Count;
          ++local.NoOfIncrementalUpdates.Count;

          if (local.NoOfIncrementalUpdates.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
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
                  "Commit Performed after person number: " + entities
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
      UseFnB667Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB667Close();

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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB667Close()
  {
    var useImport = new FnB667Close.Import();
    var useExport = new FnB667Close.Export();

    useImport.DebtAdjustsProcessed.Count = local.DebtAdjustmentsProcessed.Count;
    useImport.NoCashRcptDtlReversed.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoCollectiosReversed.Count = local.NoCollectionsReversed.Count;

    Call(FnB667Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB667Housekeeping()
  {
    var useImport = new FnB667Housekeeping.Import();
    var useExport = new FnB667Housekeeping.Export();

    Call(FnB667Housekeeping.Execute, useImport, useExport);

    local.Max.Date = useExport.Max.Date;
    MoveProgramProcessingInfo2(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.CollectionAdjustmentReason.Assign(useExport.RetroDebtAdjustment);
    local.DistributedStatus.SystemGeneratedIdentifier =
      useExport.DistributedStatus.SystemGeneratedIdentifier;
    local.RefundedStatus.SystemGeneratedIdentifier =
      useExport.Refunded.SystemGeneratedIdentifier;
    local.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.SuspendedStatus.SystemGeneratedIdentifier;
    local.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.ReleasedStatus.SystemGeneratedIdentifier;
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
  }

  private void UseFnCabReverseAllCshRcptDtls()
  {
    var useImport = new FnCabReverseAllCshRcptDtls.Import();
    var useExport = new FnCabReverseAllCshRcptDtls.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.InterfaceIndicator =
      local.CashReceiptSourceType.InterfaceIndicator;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber = local.CashReceipt.SequentialNumber;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.Max.Date = local.Max.Date;
    MoveProgramProcessingInfo1(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.DistributedStatus.SystemGeneratedIdentifier =
      local.DistributedStatus.SystemGeneratedIdentifier;
    useImport.RefundedStatus.SystemGeneratedIdentifier =
      local.RefundedStatus.SystemGeneratedIdentifier;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.CollectionAdjustmentReason.SystemGeneratedIdentifier;
    useImport.ReleasedStatus.SystemGeneratedIdentifier =
      local.ReleasedStatus.SystemGeneratedIdentifier;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useImport.SuspendedStatus.SystemGeneratedIdentifier =
      local.SuspendedStatus.SystemGeneratedIdentifier;
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useExport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useExport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useExport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;

    Call(FnCabReverseAllCshRcptDtls.Execute, useImport, useExport);

    local.NoOfIncrementalUpdates.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCashRecptDtlUpdated.Count = useImport.NoCashRecptDtlUpdated.Count;
    local.NoCollectionsReversed.Count = useImport.NoCollectionsReversed.Count;
    local.NoOfIncrementalUpdates.Count = useExport.NoOfIncrementalUpdates.Count;
    local.NoCashRecptDtlUpdated.Count = useExport.NoCashRecptDtlUpdated.Count;
    local.NoCollectionsReversed.Count = useExport.NoCollectionsReversed.Count;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadObligationTransactionDebtDetailCsePerson1()
  {
    entities.DebtDetail.Populated = false;
    entities.CsePerson.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionDebtDetailCsePerson1",
      (db, command) =>
      {
        db.SetString(
          command, "cspPNumber", local.PpiParameter.ObligorPersonNumber ?? "");
        db.SetDate(
          command, "debtAdjProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
        entities.ObligationTransaction.DebtAdjustmentProcessDate =
          db.GetDate(reader, 10);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 16);
        entities.DebtDetail.DueDt = db.GetDate(reader, 17);
        entities.CsePerson.Number = db.GetString(reader, 18);
        entities.DebtDetail.Populated = true;
        entities.CsePerson.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransactionDebtDetailCsePerson2()
  {
    entities.DebtDetail.Populated = false;
    entities.CsePerson.Populated = false;
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransactionDebtDetailCsePerson2",
      (db, command) =>
      {
        db.SetDate(
          command, "debtAdjProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ObligationTransaction.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 7);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 9);
        entities.ObligationTransaction.DebtAdjustmentProcessDate =
          db.GetDate(reader, 10);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 11);
        entities.DebtDetail.CspNumber = db.GetString(reader, 12);
        entities.DebtDetail.CpaType = db.GetString(reader, 13);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 14);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 15);
        entities.DebtDetail.OtrType = db.GetString(reader, 16);
        entities.DebtDetail.DueDt = db.GetDate(reader, 17);
        entities.CsePerson.Number = db.GetString(reader, 18);
        entities.DebtDetail.Populated = true;
        entities.CsePerson.Populated = true;
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);

        return true;
      });
  }

  private void UpdateObligationTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = Now();
    var debtAdjustmentProcessDate = local.ProgramProcessingInfo.ProcessDate;

    entities.ObligationTransaction.Populated = false;
    Update("UpdateObligationTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetDate(command, "debtAdjProcDate", debtAdjustmentProcessDate);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetInt32(
          command, "obTrnId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.SetString(command, "obTrnTyp", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
      });

    entities.ObligationTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.ObligationTransaction.LastUpdatedTmst = lastUpdatedTmst;
    entities.ObligationTransaction.DebtAdjustmentProcessDate =
      debtAdjustmentProcessDate;
    entities.ObligationTransaction.Populated = true;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of PreviousObligor.
    /// </summary>
    [JsonPropertyName("previousObligor")]
    public CsePerson PreviousObligor
    {
      get => previousObligor ??= new();
      set => previousObligor = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
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
    /// A value of DebtAdjustmentsProcessed.
    /// </summary>
    [JsonPropertyName("debtAdjustmentsProcessed")]
    public Common DebtAdjustmentsProcessed
    {
      get => debtAdjustmentsProcessed ??= new();
      set => debtAdjustmentsProcessed = value;
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
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
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

    private CashReceiptDetail ppiParameter;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private Common reportNeeded;
    private Common typeOfChange;
    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus refundedStatus;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CashReceiptDetailStatus suspendedStatus;
    private CashReceiptDetailStatus releasedStatus;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private Collection collection;
    private CsePerson previousObligor;
    private TextWorkArea date;
    private DateWorkArea null1;
    private Common processMonth;
    private Common processYear;
    private CashReceiptDetail cashReceiptDetail;
    private Common noOfIncrementalUpdates;
    private Common noCashRecptDtlUpdated;
    private Common noCollectionsReversed;
    private Common debtAdjustmentsProcessed;
    private External passArea;
    private TextWorkArea time;
    private DateWorkArea programStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    private DebtDetail debtDetail;
    private ObligationTransaction debt;
    private ObligationTransactionRln obligationTransactionRln;
    private Obligation obligation;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}

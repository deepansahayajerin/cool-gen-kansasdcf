// Program: FN_B640_NOTIFY_AE_OF_INCOME, ID: 372656391, model: 746.
// Short name: SWEF640B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B640_NOTIFY_AE_OF_INCOME.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB640NotifyAeOfIncome: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B640_NOTIFY_AE_OF_INCOME program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB640NotifyAeOfIncome(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB640NotifyAeOfIncome.
  /// </summary>
  public FnB640NotifyAeOfIncome(IContext context, Import import, Export export):
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
    // ***** SWEFB640  Notify AE of Child Support Income
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *******************************************************************
    // DATE      DEVELOPER NAME          REQUEST #  DESCRIPTION
    // --------  ----------------------  ---------  ---------------
    // 11/08/96  Beth Burrell - SRS        Initial Development
    // 11/17/97  Evelyn Parker - DIR	    Problem Report #26863.  Documentation 
    // will be made throughout the procedure and actionblock to explain changes
    // made.
    // 12/9/97   Evelyn Parker - DIR	    Modified logic to avoid notifying AE of
    // REIP pymts.  Also rematched views to update_program_control_total and
    // create_program_error.
    // 3/3/1998  Evelyn Parker - DIR	Updated Checkpoint Restart logic for 
    // performance.  Changed logic to expect PPI Process Date to be Current Date
    // as opposed to First day of month.
    // 3/3/1998  Evelyn Parker - DIR	Problem Report #39450 -- Changed logic in 
    // Notify AE of Collections to use Process Date instead of Current Date.
    // 11/9/1998  Evelyn Parker	Changed logic to write control report and error 
    // report to SAR as opposed to creating records in program_control_total and
    // program_error tables.  Marked program_control_total and program_error
    // views as zdel.  Further qualified read on Collection to avoid reading
    // unnecessary records.
    // 11/18/1998  Evelyn Parker	Added logic to find Case even if Case Roles are
    // discontinued.  Added logic to bypass Collections where
    // Program_Applied_To is an Interstate Program.
    // 01/14/2000  Evelyn Parker	Due to change in Business Rules, changed logic 
    // to make this a monthly job and to report all Collections regardless of
    // being retro.
    // 10/6/2000 Kalpesh Doshi
    // - Remove checkpoint logic. Commit logic stays.
    // - Cater for new AE requirements.
    // *******************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();

    // ***** Get run parameters for this program *****
    UseFnB640Initialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***** Abort  *******
      return;
    }

    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.EabFileHandling.Action = "WRITE";

    // ******************************************************************
    // 1/14/2000 - Modified logic to get the last run date from the PPI Parm 
    // List.  This job is now monthly, and we will read records created since
    // the last run.
    // ******************************************************************
    local.Process.Date = local.ProgramProcessingInfo.ProcessDate;
    UseFnHardcodedCashReceipting();

    if (IsEmpty(local.AbendLoop.Flag))
    {
      // *** MAIN PROCESSING ***
      // ===============================================
      // SWSRKXD
      // Add clause - Collection.applied_to_code = 'C'
      // Add clause - Obligation_type = 1 or 2
      // ===============================================
      foreach(var item in ReadCollectionCsePersonCashReceiptDetailDebt())
      {
        ++local.CollectionsRead.Count;

        if (!ReadCsePerson())
        {
          ExitState = "CSE_PERSON_NF";
          local.EabReportSend.RptDetail =
            "Supported Person nf.  Obligor # - " + entities.KeyObligor.Number;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id - " + NumberToString
            (entities.Collection.SystemGeneratedIdentifier, 7, 9);
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto Test;
        }

        // ===============================================
        // 11/17/00
        // Skip collections for Supp Persons NOT on 'AF'
        // at the time collection was created.
        // ===============================================
        if (!ReadProgram())
        {
          if (AsChar(local.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Coll skipped since supp person is not AF.  Obligor # -" + entities
              .KeyObligor.Number;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id - " + NumberToString
              (entities.Collection.SystemGeneratedIdentifier, 7, 9);
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }

        // ===============================================
        // 11/15/00 - SWSRKXD
        // Skip collections that are a result of re-applying an adjusted
        // collection where adjusted collection was already notified to AE.
        // ===============================================
        if (ReadCollection())
        {
          if (AsChar(local.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Coll skipped since it was reapplied to Adj coll.  Obligor # -" +
              entities.KeyObligor.Number;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id - " + NumberToString
              (entities.Collection.SystemGeneratedIdentifier, 7, 9);
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }

        // ===============================================
        // 11/15/00 - SWSRKXD
        // Add code to skip collections that caused a Recapture.
        // ===============================================
        if (ReadPaymentRequest())
        {
          if (AsChar(local.DisplayInd.Flag) == 'Y')
          {
            local.EabReportSend.RptDetail =
              "Coll skipped since it was Recaptured.  Obligor # -" + entities
              .KeyObligor.Number;
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id - " + NumberToString
              (entities.Collection.SystemGeneratedIdentifier, 7, 9);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "; PR.Id - " + NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9);
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
            UseCabControlReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
        }

        // *****  Determine if a commit is required. *****
        if (local.CollUpdSinceLastCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() &&
          !Equal(local.LastProcessed.Number, entities.KeyObligor.Number) && AsChar
          (local.TestRunInd.Flag) != 'Y')
        {
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

            return;
          }

          local.CollUpdSinceLastCommit.Count = 0;
        }

        local.LastProcessed.Number = entities.KeyObligor.Number;

        // ---------------------------------------------------
        // PR 102420 - 11/14/00
        // Map process_date instead of current_date.  Supply Supp person.
        // ---------------------------------------------------
        UseFnNotifyAeOfCollections();

        // *****  ABEND Program on error *****
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          goto Test;
        }

        ++local.CollectionsUpdated.Count;
        ++local.InterfaceRecsCreated.Count;
        ++local.CollUpdSinceLastCommit.Count;

        if (AsChar(local.DisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail =
            "Coll reported to AE.  Obligor # -" + entities.KeyObligor.Number;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "; Supp Person # -" + entities
            .Supp.Number;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "; Coll Id - " + NumberToString
            (entities.Collection.SystemGeneratedIdentifier, 7, 9);
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
          UseCabControlReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        // ***---  end of Read Each
      }
    }

Test:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ControlTable.Identifier = "SWEFB640 LAST RUN DATE";
      local.ControlTable.LastUsedNumber = DateToInt(local.Process.Date);
      UseFnUpdateControlTable();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseFnB640CloseDown();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
    else
    {
      UseFnB640CloseDown();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (AsChar(local.TestRunInd.Flag) == 'Y')
    {
      ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.ProgramAppliedTo = source.ProgramAppliedTo;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AppliedToCode = source.AppliedToCode;
    target.CollectionDt = source.CollectionDt;
    target.AdjustedInd = source.AdjustedInd;
    target.ConcurrentInd = source.ConcurrentInd;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.AppliedToOrderTypeCode = source.AppliedToOrderTypeCode;
    target.AeNotifiedDate = source.AeNotifiedDate;
  }

  private static void MoveControlTable(ControlTable source, ControlTable target)
  {
    target.Identifier = source.Identifier;
    target.LastUsedNumber = source.LastUsedNumber;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB640CloseDown()
  {
    var useImport = new FnB640CloseDown.Import();
    var useExport = new FnB640CloseDown.Export();

    useImport.NbrOfRecordsCreated.Count = local.InterfaceRecsCreated.Count;
    useImport.NbrOfRecordsUpdated.Count = local.CollectionsUpdated.Count;
    useImport.NbrOfRecordsRead.Count = local.CollectionsRead.Count;

    Call(FnB640CloseDown.Execute, useImport, useExport);
  }

  private void UseFnB640Initialization()
  {
    var useImport = new FnB640Initialization.Import();
    var useExport = new FnB640Initialization.Export();

    Call(FnB640Initialization.Execute, useImport, useExport);

    local.TestRunInd.Flag = useExport.TestRunInd.Flag;
    local.DisplayInd.Flag = useExport.DisplayInd.Flag;
    local.EndObligor.Number = useExport.EndObligor.Number;
    local.StartObligor.Number = useExport.StartObligor.Number;
    local.LastRun.Date = useExport.LastRunDate.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedFcrtRec.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
  }

  private void UseFnNotifyAeOfCollections()
  {
    var useImport = new FnNotifyAeOfCollections.Import();
    var useExport = new FnNotifyAeOfCollections.Export();

    useImport.Supported.Number = entities.Supp.Number;
    useImport.Current.Date = local.Process.Date;
    useImport.P.Assign(entities.Collection);
    useImport.Obligor.Number = entities.KeyObligor.Number;

    Call(FnNotifyAeOfCollections.Execute, useImport, useExport);

    MoveCollection(useImport.P, entities.Collection);
    local.EabReportSend.RptDetail = useExport.EabReportSend.RptDetail;
  }

  private void UseFnUpdateControlTable()
  {
    var useImport = new FnUpdateControlTable.Import();
    var useExport = new FnUpdateControlTable.Export();

    MoveControlTable(local.ControlTable, useImport.ControlTable);

    Call(FnUpdateControlTable.Execute, useImport, useExport);
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjusted.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetNullableDate(
          command, "aeNotifiedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Adjusted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Adjusted.AppliedToCode = db.GetString(reader, 1);
        entities.Adjusted.CollectionDt = db.GetDate(reader, 2);
        entities.Adjusted.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Adjusted.ConcurrentInd = db.GetString(reader, 4);
        entities.Adjusted.CrtType = db.GetInt32(reader, 5);
        entities.Adjusted.CstId = db.GetInt32(reader, 6);
        entities.Adjusted.CrvId = db.GetInt32(reader, 7);
        entities.Adjusted.CrdId = db.GetInt32(reader, 8);
        entities.Adjusted.ObgId = db.GetInt32(reader, 9);
        entities.Adjusted.CspNumber = db.GetString(reader, 10);
        entities.Adjusted.CpaType = db.GetString(reader, 11);
        entities.Adjusted.OtrId = db.GetInt32(reader, 12);
        entities.Adjusted.OtrType = db.GetString(reader, 13);
        entities.Adjusted.OtyId = db.GetInt32(reader, 14);
        entities.Adjusted.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Adjusted.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Adjusted.LastUpdatedTmst = db.GetNullableDateTime(reader, 17);
        entities.Adjusted.Amount = db.GetDecimal(reader, 18);
        entities.Adjusted.ProgramAppliedTo = db.GetString(reader, 19);
        entities.Adjusted.AppliedToOrderTypeCode = db.GetString(reader, 20);
        entities.Adjusted.AeNotifiedDate = db.GetNullableDate(reader, 21);
        entities.Adjusted.Populated = true;
        CheckValid<Collection>("AppliedToCode", entities.Adjusted.AppliedToCode);
          
        CheckValid<Collection>("AdjustedInd", entities.Adjusted.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd", entities.Adjusted.ConcurrentInd);
          
        CheckValid<Collection>("CpaType", entities.Adjusted.CpaType);
        CheckValid<Collection>("OtrType", entities.Adjusted.OtrType);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Adjusted.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Adjusted.AppliedToOrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadCollectionCsePersonCashReceiptDetailDebt()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.Debt.Populated = false;
    entities.KeyObligor.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePersonCashReceiptDetailDebt",
      (db, command) =>
      {
        db.SetDate(command, "date", local.LastRun.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "aeNotifiedDt", local.Initialize.Date.GetValueOrDefault());
        db.SetInt32(
          command, "crtType1",
          local.HardcodedFcrtRec.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtType2",
          local.HardcodedFdirPmt.SystemGeneratedIdentifier);
        db.SetString(command, "number1", local.StartObligor.Number);
        db.SetString(command, "number2", local.EndObligor.Number);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.ConcurrentInd = db.GetString(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.KeyObligor.Number = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Debt.CspNumber = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Debt.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Debt.Type1 = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Debt.OtyType = db.GetInt32(reader, 14);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 15);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 16);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 17);
        entities.Collection.Amount = db.GetDecimal(reader, 18);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 19);
        entities.Collection.AppliedToOrderTypeCode = db.GetString(reader, 20);
        entities.Collection.AeNotifiedDate = db.GetNullableDate(reader, 21);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 22);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 23);
        entities.CashReceiptDetail.Populated = true;
        entities.Debt.Populated = true;
        entities.KeyObligor.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);
        CheckValid<Collection>("AppliedToOrderTypeCode",
          entities.Collection.AppliedToOrderTypeCode);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supp.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supp.Number = db.GetString(reader, 0);
        entities.Supp.Type1 = db.GetString(reader, 1);
        entities.Supp.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Supp.Type1);
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "colId", entities.Collection.SystemGeneratedIdentifier);
        db.SetNullableInt32(command, "otyId", entities.Collection.OtyId);
        db.SetNullableInt32(command, "obgId", entities.Collection.ObgId);
        db.SetNullableString(
          command, "cspNumberDisb", entities.Collection.CspNumber);
        db.
          SetNullableString(command, "cpaTypeDisb", entities.Collection.CpaType);
          
        db.SetNullableInt32(command, "otrId", entities.Collection.OtrId);
        db.
          SetNullableString(command, "otrTypeDisb", entities.Collection.OtrType);
          
        db.SetNullableInt32(command, "crtId", entities.Collection.CrtType);
        db.SetNullableInt32(command, "cstId", entities.Collection.CstId);
        db.SetNullableInt32(command, "crvId", entities.Collection.CrvId);
        db.SetNullableInt32(command, "crdId", entities.Collection.CrdId);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Type1 = db.GetString(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadProgram()
  {
    entities.Af.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          entities.Collection.CreatedTmst.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Supp.Number);
      },
      (db, reader) =>
      {
        entities.Af.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Af.Code = db.GetString(reader, 1);
        entities.Af.InterstateIndicator = db.GetString(reader, 2);
        entities.Af.Populated = true;
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
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// A value of CollUpdSinceLastCommit.
    /// </summary>
    [JsonPropertyName("collUpdSinceLastCommit")]
    public Common CollUpdSinceLastCommit
    {
      get => collUpdSinceLastCommit ??= new();
      set => collUpdSinceLastCommit = value;
    }

    /// <summary>
    /// A value of AbendLoop.
    /// </summary>
    [JsonPropertyName("abendLoop")]
    public Common AbendLoop
    {
      get => abendLoop ??= new();
      set => abendLoop = value;
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
    /// A value of TestRunInd.
    /// </summary>
    [JsonPropertyName("testRunInd")]
    public Common TestRunInd
    {
      get => testRunInd ??= new();
      set => testRunInd = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
    }

    /// <summary>
    /// A value of EndObligor.
    /// </summary>
    [JsonPropertyName("endObligor")]
    public CsePerson EndObligor
    {
      get => endObligor ??= new();
      set => endObligor = value;
    }

    /// <summary>
    /// A value of StartObligor.
    /// </summary>
    [JsonPropertyName("startObligor")]
    public CsePerson StartObligor
    {
      get => startObligor ??= new();
      set => startObligor = value;
    }

    /// <summary>
    /// A value of NewTmst.
    /// </summary>
    [JsonPropertyName("newTmst")]
    public ProgramProcessingInfo NewTmst
    {
      get => newTmst ??= new();
      set => newTmst = value;
    }

    /// <summary>
    /// A value of RestartLoop.
    /// </summary>
    [JsonPropertyName("restartLoop")]
    public Common RestartLoop
    {
      get => restartLoop ??= new();
      set => restartLoop = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
    }

    /// <summary>
    /// A value of RestartDueToError.
    /// </summary>
    [JsonPropertyName("restartDueToError")]
    public Common RestartDueToError
    {
      get => restartDueToError ??= new();
      set => restartDueToError = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public CsePerson Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of WorkTimestamp.
    /// </summary>
    [JsonPropertyName("workTimestamp")]
    public DateWorkArea WorkTimestamp
    {
      get => workTimestamp ??= new();
      set => workTimestamp = value;
    }

    /// <summary>
    /// A value of LastProcessed.
    /// </summary>
    [JsonPropertyName("lastProcessed")]
    public CsePerson LastProcessed
    {
      get => lastProcessed ??= new();
      set => lastProcessed = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of Subscript.
    /// </summary>
    [JsonPropertyName("subscript")]
    public Common Subscript
    {
      get => subscript ??= new();
      set => subscript = value;
    }

    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public DateWorkArea Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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
    /// A value of InterfaceRecsCreated.
    /// </summary>
    [JsonPropertyName("interfaceRecsCreated")]
    public Common InterfaceRecsCreated
    {
      get => interfaceRecsCreated ??= new();
      set => interfaceRecsCreated = value;
    }

    /// <summary>
    /// A value of InterfaceRecsCrtdInJob.
    /// </summary>
    [JsonPropertyName("interfaceRecsCrtdInJob")]
    public Common InterfaceRecsCrtdInJob
    {
      get => interfaceRecsCrtdInJob ??= new();
      set => interfaceRecsCrtdInJob = value;
    }

    /// <summary>
    /// A value of CollectionsUpdated.
    /// </summary>
    [JsonPropertyName("collectionsUpdated")]
    public Common CollectionsUpdated
    {
      get => collectionsUpdated ??= new();
      set => collectionsUpdated = value;
    }

    /// <summary>
    /// A value of CollectionsRead.
    /// </summary>
    [JsonPropertyName("collectionsRead")]
    public Common CollectionsRead
    {
      get => collectionsRead ??= new();
      set => collectionsRead = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public DateWorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of HardcodedFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedFdirPmt")]
    public CashReceiptType HardcodedFdirPmt
    {
      get => hardcodedFdirPmt ??= new();
      set => hardcodedFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedFcrtRec.
    /// </summary>
    [JsonPropertyName("hardcodedFcrtRec")]
    public CashReceiptType HardcodedFcrtRec
    {
      get => hardcodedFcrtRec ??= new();
      set => hardcodedFcrtRec = value;
    }

    /// <summary>
    /// A value of End.
    /// </summary>
    [JsonPropertyName("end")]
    public DateWorkArea End
    {
      get => end ??= new();
      set => end = value;
    }

    /// <summary>
    /// A value of LastRun.
    /// </summary>
    [JsonPropertyName("lastRun")]
    public DateWorkArea LastRun
    {
      get => lastRun ??= new();
      set => lastRun = value;
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
    /// A value of ControlCounts.
    /// </summary>
    [JsonPropertyName("controlCounts")]
    public ProgramCheckpointRestart ControlCounts
    {
      get => controlCounts ??= new();
      set => controlCounts = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      controlTable = null;
      collUpdSinceLastCommit = null;
      abendLoop = null;
      null1 = null;
      testRunInd = null;
      displayInd = null;
      endObligor = null;
      startObligor = null;
      newTmst = null;
      commit = null;
      error = null;
      workTimestamp = null;
      lastProcessed = null;
      restart = null;
      current = null;
      subscript = null;
      initialize = null;
      eabReportSend = null;
      eabFileHandling = null;
      interfaceRecsCreated = null;
      interfaceRecsCrtdInJob = null;
      collectionsUpdated = null;
      collectionsRead = null;
      common = null;
      text = null;
      process = null;
      hardcodedFdirPmt = null;
      hardcodedFcrtRec = null;
      end = null;
      lastRun = null;
      programProcessingInfo = null;
      programCheckpointRestart = null;
      controlCounts = null;
      exitStateWorkArea = null;
      passArea = null;
    }

    private ControlTable controlTable;
    private Common collUpdSinceLastCommit;
    private Common abendLoop;
    private DateWorkArea null1;
    private Common testRunInd;
    private Common displayInd;
    private CsePerson endObligor;
    private CsePerson startObligor;
    private ProgramProcessingInfo newTmst;
    private Common restartLoop;
    private Common commit;
    private Common restartDueToError;
    private CsePerson error;
    private DateWorkArea workTimestamp;
    private CsePerson lastProcessed;
    private CsePerson restart;
    private DateWorkArea current;
    private Common subscript;
    private DateWorkArea initialize;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common interfaceRecsCreated;
    private Common interfaceRecsCrtdInJob;
    private Common collectionsUpdated;
    private Common collectionsRead;
    private Common common;
    private DateWorkArea text;
    private DateWorkArea process;
    private CashReceiptType hardcodedFdirPmt;
    private CashReceiptType hardcodedFcrtRec;
    private DateWorkArea end;
    private DateWorkArea lastRun;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramCheckpointRestart controlCounts;
    private ExitStateWorkArea exitStateWorkArea;
    private External passArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supp.
    /// </summary>
    [JsonPropertyName("supp")]
    public CsePerson Supp
    {
      get => supp ??= new();
      set => supp = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonAccount Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
    }

    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
    }

    /// <summary>
    /// A value of Disbursement.
    /// </summary>
    [JsonPropertyName("disbursement")]
    public DisbursementTransaction Disbursement
    {
      get => disbursement ??= new();
      set => disbursement = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Adjusted.
    /// </summary>
    [JsonPropertyName("adjusted")]
    public Collection Adjusted
    {
      get => adjusted ??= new();
      set => adjusted = value;
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
    /// A value of Af.
    /// </summary>
    [JsonPropertyName("af")]
    public Program Af
    {
      get => af ??= new();
      set => af = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of KeyObligor.
    /// </summary>
    [JsonPropertyName("keyObligor")]
    public CsePerson KeyObligor
    {
      get => keyObligor ??= new();
      set => keyObligor = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private CsePerson supp;
    private CsePersonAccount supported;
    private PersonProgram personProgram;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbCollection;
    private DisbursementTransaction disbursement;
    private PaymentRequest paymentRequest;
    private Collection adjusted;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Program af;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson keyObligor;
    private Collection collection;
    private ProgramProcessingInfo programProcessingInfo;
  }
#endregion
}

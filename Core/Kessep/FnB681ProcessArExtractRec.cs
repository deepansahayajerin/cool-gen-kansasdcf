// Program: FN_B681_PROCESS_AR_EXTRACT_REC, ID: 374558194, model: 746.
// Short name: SWEF681B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B681_PROCESS_AR_EXTRACT_REC.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB681ProcessArExtractRec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B681_PROCESS_AR_EXTRACT_REC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB681ProcessArExtractRec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB681ProcessArExtractRec.
  /// </summary>
  public FnB681ProcessArExtractRec(IContext context, Import import,
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
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 05/14/2010  DDupree	PR12681	Initial Development.  New business rules for 
    // GDG file.
    // -------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------------------
    // --  This program creates the gdg records for EES using an extract file 
    // from a previous job step which has been sorted for each ar, and obligor.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Read.Action = "READ";
    local.Write.Action = "WRITE";
    local.Close.Action = "CLOSE";

    // -------------------------------------------------------------------------------------------------------------------------
    // --  General Housekeeping and Initializations.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB681BatchInitialization();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (!IsExitState("ACO_NN0000_ABEND_FOR_BATCH"))
      {
        // -- Extract the exit state message and write to the error report.
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "Initialization Cab Error..." + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport1();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }

      return;
    }

    // -- Convert Reporting Period Starting Date to text.
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      NumberToString(Month(local.ReportingPeriodStartingDateWorkArea.Date), 14,
      2) + "/";
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + NumberToString
      (Day(local.ReportingPeriodStartingDateWorkArea.Date), 14, 2);
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + "/";
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodStartingTextWorkArea.Text10) + NumberToString
      (Year(local.ReportingPeriodStartingDateWorkArea.Date), 12, 4);

    // -- Convert Reporting Period Ending Date to text.
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      NumberToString(Month(local.ReportingPeriodEndingDateWorkArea.Date), 14, 2) +
      "/";
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + NumberToString
      (Day(local.ReportingPeriodEndingDateWorkArea.Date), 14, 2);
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + "/";
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      TrimEnd(local.ReportingPeriodEndingTextWorkArea.Text10) + NumberToString
      (Year(local.ReportingPeriodEndingDateWorkArea.Date), 12, 4);
    UseFnHardcodedDebtDistribution();
    local.Local1.Index = -1;
    local.Local2.Index = -1;

    do
    {
      // -------------------------------------------------------------------------------------------------------------------------
      // --  Get a record from the sorted/summed extract file.
      // --
      // --  Note that the external can return more data than what we actually 
      // need.  Not all views need to be returned for what we're doing here.
      // -------------------------------------------------------------------------------------------------------------------------
      UseFnB681ProcessArExtractData1();

      if (!Equal(local.External.TextReturnCode, "00") && !
        Equal(local.External.TextReturnCode, "EF"))
      {
        // --  write to error file...
        local.EabReportSend.RptDetail =
          "(01) Error reading extract file...  Returned Status = " + local
          .External.TextReturnCode;
        UseCabErrorReport1();
        ExitState = "ERROR_READING_FILE_AB";

        return;
      }

      if (IsEmpty(local.ArCsePerson.Number))
      {
        break;
      }

      // -------------------------------------------------------------------------------------------------------------------------
      // -- If restarting then skip any ARs previously processed.
      // -------------------------------------------------------------------------------------------------------------------------
      if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y' && !
        IsEmpty(local.ArCsePerson.Number) && !
        Lt(local.Restart.Number, local.ArCsePerson.Number))
      {
        continue;
      }

      ++local.ReadCount.Count;

      if (!Equal(local.ArCsePerson.Number, local.PreviousAr.Number))
      {
        if (AsChar(local.Local718Only.Flag) == 'Y')
        {
          // -- Take out the last AP if it was a 718b only AP, let the rest of 
          // the record process.
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (Equal(local.PreviousAp.Number,
              local.Local1.Item.ObligorCsePerson.Number))
            {
              continue;
            }

            ++local.Local2.Index;
            local.Local2.CheckSize();

            // load 2nd table with records that do not have the AP with only 
            // 718b money
            local.Local2.Update.ArGrp2CsePerson.Number =
              local.Local1.Item.ArCsePerson.Number;
            local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
              local.Local1.Item.ObligorCsePersonsWorkSet.LastName;
            local.Local2.Update.ObligorGrp2CsePerson.Number =
              local.Local1.Item.ObligorCsePerson.Number;
            local.Local2.Update.Grp2Collection.Assign(
              local.Local1.Item.Collection);
            local.Local2.Update.RetainedGrp2.Amount =
              local.Local1.Item.Retained.Amount;
            local.Local2.Update.ForwardToFamilyGrp2.Amount =
              local.Local1.Item.ForwardedToFamily.Amount;
            local.Local2.Update.ObligorGrp2Obligor.Type1 =
              local.Local1.Item.ObligorObligor.Type1;
            local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
              local.Local1.Item.ObligationType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
              local.Local1.Item.Obligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.Local1.Item.ObligationTransaction,
              local.Local2.Update.Grp2ObligationTransaction);
            local.Local2.Update.Grp2CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.Local1.Item.CashReceiptSourceType.
                SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
              local.Local1.Item.CashReceiptEvent.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
              local.Local1.Item.CashReceiptType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
              local.Local1.Item.CashReceiptDetail.SequentialIdentifier;
            local.Local2.Update.ChPersonGrp2.Assign(local.Local1.Item.ChPerson);
            local.Local2.Update.ArGrp2CsePersonsWorkSet.Assign(
              local.Local1.Item.ArCsePersonsWorkSet);
            local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
              local.Local1.Item.Taf.SystemGeneratedIdentifier;
          }

          local.Local1.CheckIndex();

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            // clean group view
            local.Local1.Update.ArCsePerson.Number =
              local.ArClearCsePerson.Number;
            local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
              local.ObligorClearCsePersonsWorkSet.LastName;
            local.Local1.Update.ObligorCsePerson.Number =
              local.ObligorClearCsePerson.Number;
            local.Local1.Update.Collection.Assign(local.ClearCollection);
            local.Local1.Update.Retained.Amount = local.RetainedClear.Amount;
            local.Local1.Update.ForwardedToFamily.Amount =
              local.ForwardToFamilyClear.Amount;
            local.Local1.Update.ObligorObligor.Type1 = local.ClearObligor.Type1;
            local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
            local.Local1.Update.Obligation.SystemGeneratedIdentifier =
              local.ClearObligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.LocaleClear,
              local.Local1.Update.ObligationTransaction);
            local.Local1.Update.CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
              local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
              local.ClearCashReceiptType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
              local.ClearCashReceiptDetail.SequentialIdentifier;
            local.Local1.Update.ChPerson.Assign(local.ChPersonClear);
            local.Local1.Update.ArCsePersonsWorkSet.Assign(local.ChPersonClear);
            local.Local1.Update.Taf.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
          }

          local.Local1.CheckIndex();
          local.Local1.Index = -1;
          local.Local1.Count = 0;

          for(local.Local2.Index = 0; local.Local2.Index < local.Local2.Count; ++
            local.Local2.Index)
          {
            if (!local.Local2.CheckSize())
            {
              break;
            }

            // now reload group view for processing
            ++local.Local1.Index;
            local.Local1.CheckSize();

            local.Local1.Update.ArCsePerson.Number =
              local.Local2.Item.ArGrp2CsePerson.Number;
            local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
              local.Local2.Item.ObligorGrp2CsePersonsWorkSet.LastName;
            local.Local1.Update.ObligorCsePerson.Number =
              local.Local2.Item.ObligorGrp2CsePerson.Number;
            local.Local1.Update.Collection.Assign(
              local.Local2.Item.Grp2Collection);
            local.Local1.Update.Retained.Amount =
              local.Local2.Item.RetainedGrp2.Amount;
            local.Local1.Update.ForwardedToFamily.Amount =
              local.Local2.Item.ForwardToFamilyGrp2.Amount;
            local.Local1.Update.ObligorObligor.Type1 =
              local.Local2.Item.ObligorGrp2Obligor.Type1;
            local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2ObligationType.SystemGeneratedIdentifier;
            local.Local1.Update.Obligation.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2Obligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.Local2.Item.
              Grp2ObligationTransaction,
              local.Local1.Update.ObligationTransaction);
            local.Local1.Update.CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.Local2.Item.Grp2CashReceiptSourceType.
                SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2CashReceiptEvent.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2CashReceiptType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
              local.Local2.Item.Grp2CashReceiptDetail.SequentialIdentifier;
            local.Local1.Update.ChPerson.Assign(local.Local2.Item.ChPersonGrp2);
            local.Local1.Update.ArCsePersonsWorkSet.Assign(
              local.Local2.Item.ArGrp2CsePersonsWorkSet);
            local.Local1.Update.Taf.SystemGeneratedIdentifier =
              local.Local2.Item.TafGrp2.SystemGeneratedIdentifier;
          }

          local.Local2.CheckIndex();
          local.Local2.Index = 0;

          for(var limit = local.Local2.Index + 1; local.Local2.Index < limit; ++
            local.Local2.Index)
          {
            if (!local.Local2.CheckSize())
            {
              break;
            }

            // cleanup 2nd group view for next time
            local.Local2.Update.ArGrp2CsePerson.Number =
              local.ArClearCsePerson.Number;
            local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
              local.ObligorClearCsePersonsWorkSet.LastName;
            local.Local2.Update.ObligorGrp2CsePerson.Number =
              local.ObligorClearCsePerson.Number;
            local.Local2.Update.Grp2Collection.Assign(local.ClearCollection);
            local.Local2.Update.RetainedGrp2.Amount =
              local.RetainedClear.Amount;
            local.Local2.Update.ForwardToFamilyGrp2.Amount =
              local.ForwardToFamilyClear.Amount;
            local.Local2.Update.ObligorGrp2Obligor.Type1 =
              local.ClearObligor.Type1;
            local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
              local.ClearObligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.LocaleClear,
              local.Local2.Update.Grp2ObligationTransaction);
            local.Local2.Update.Grp2CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
              local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
              local.ClearCashReceiptType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
              local.ClearCashReceiptDetail.SequentialIdentifier;
            local.Local2.Update.ChPersonGrp2.Assign(local.ChPersonClear);
            local.Local2.Update.ArGrp2CsePersonsWorkSet.Assign(
              local.ChPersonClear);
            local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
          }

          local.Local2.CheckIndex();
          local.Local2.Index = -1;
          local.Local2.Count = 0;
          local.PreviousAp.Number = "";
        }

        // -------------------------------------------------------------------------------------------------------------------------
        // --  Our most recent read of the extract file produced a new AR/AP 
        // combination.  We need to write the AR statement for the
        // --  previous AR using all the collection information stored in the 
        // local group.
        // -------------------------------------------------------------------------------------------------------------------------
        // -- Check the AR record.
        UseFnB681DetermineArRecords();

        // -- An exit state is set in fn_b681_process_ar_extraxt_rec for errors 
        // that should cause an abend.
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -- Extract the exit state message and write to the error report.
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail =
            "FN_B681_PROCESS_AR_RECORD Error..." + local
            .ExitStateWorkArea.Message;
          UseCabErrorReport1();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        if (local.ReadCount.Count > local
          .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
        {
          // -------------------------------------------------------------------------------------------------------------------------
          // --  Checkpoint.
          // -------------------------------------------------------------------------------------------------------------------------
          local.ReadCount.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = local.PreviousAr.Number;
          UseUpdatePgmCheckpointRestart();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // -- Extract the exit state message and write to the error report.
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Checkpoint Error..." + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          UseExtToDoACommit();

          if (local.External.NumericReturnCode != 0)
          {
            local.EabReportSend.RptDetail =
              "(03) Error in External Commit Routine.  Return Code = " + NumberToString
              (local.External.NumericReturnCode, 14, 2);
            UseCabErrorReport1();

            // -- Set Abort exit state and escape...
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }

        local.PreviousAp.Number = local.ObligorCsePerson.Number;
        local.PreviousAr.Number = local.ArCsePerson.Number;

        // -- Reset the running obligation type count for the next AR.
        local.Non718BCollection.Count = 0;
        local.TafCollection.Count = 0;

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          local.Local1.Update.ArCsePerson.Number =
            local.ArClearCsePerson.Number;
          local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
            local.ObligorClearCsePersonsWorkSet.LastName;
          local.Local1.Update.ObligorCsePerson.Number =
            local.ObligorClearCsePerson.Number;
          local.Local1.Update.Collection.Assign(local.ClearCollection);
          local.Local1.Update.Retained.Amount = local.RetainedClear.Amount;
          local.Local1.Update.ForwardedToFamily.Amount =
            local.ForwardToFamilyClear.Amount;
          local.Local1.Update.ObligorObligor.Type1 = local.ClearObligor.Type1;
          local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
          local.Local1.Update.Obligation.SystemGeneratedIdentifier =
            local.ClearObligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.LocaleClear,
            local.Local1.Update.ObligationTransaction);
          local.Local1.Update.CashReceiptSourceType.SystemGeneratedIdentifier =
            local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
            local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
            local.ClearCashReceiptType.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
            local.ClearCashReceiptDetail.SequentialIdentifier;
          local.Local1.Update.ChPerson.Assign(local.ChPersonClear);
          local.Local1.Update.ArCsePersonsWorkSet.Assign(local.ChPersonClear);
          local.Local1.Update.Taf.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
        }

        local.Local1.CheckIndex();
        local.Local1.Index = -1;
        local.Local1.Count = 0;
      }

      if (AsChar(local.OkToSendRecord.Flag) != 'Y')
      {
        continue;
      }

      if (!Equal(local.ObligorCsePerson.Number, local.PreviousAp.Number))
      {
        if (AsChar(local.Local718Only.Flag) == 'Y')
        {
          // -- Take out the last AP if it was a 718b only AP, let the rest of 
          // the record process.
          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            if (Equal(local.PreviousAp.Number,
              local.Local1.Item.ObligorCsePerson.Number))
            {
              continue;
            }

            ++local.Local2.Index;
            local.Local2.CheckSize();

            // load the 2nd group view with only records that do not include the
            // AP with only 718b money
            local.Local2.Update.ArGrp2CsePerson.Number =
              local.Local1.Item.ArCsePerson.Number;
            local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
              local.Local1.Item.ObligorCsePersonsWorkSet.LastName;
            local.Local2.Update.ObligorGrp2CsePerson.Number =
              local.Local1.Item.ObligorCsePerson.Number;
            local.Local2.Update.Grp2Collection.Assign(
              local.Local1.Item.Collection);
            local.Local2.Update.RetainedGrp2.Amount =
              local.Local1.Item.Retained.Amount;
            local.Local2.Update.ForwardToFamilyGrp2.Amount =
              local.Local1.Item.ForwardedToFamily.Amount;
            local.Local2.Update.ObligorGrp2Obligor.Type1 =
              local.Local1.Item.ObligorObligor.Type1;
            local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
              local.Local1.Item.ObligationType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
              local.Local1.Item.Obligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.Local1.Item.ObligationTransaction,
              local.Local2.Update.Grp2ObligationTransaction);
            local.Local2.Update.Grp2CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.Local1.Item.CashReceiptSourceType.
                SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
              local.Local1.Item.CashReceiptEvent.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
              local.Local1.Item.CashReceiptType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
              local.Local1.Item.CashReceiptDetail.SequentialIdentifier;
            local.Local2.Update.ChPersonGrp2.Assign(local.Local1.Item.ChPerson);
            local.Local2.Update.ArGrp2CsePersonsWorkSet.Assign(
              local.Local1.Item.ArCsePersonsWorkSet);
            local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
              local.Local1.Item.Taf.SystemGeneratedIdentifier;
          }

          local.Local1.CheckIndex();

          for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
            local.Local1.Index)
          {
            if (!local.Local1.CheckSize())
            {
              break;
            }

            // clean orginal group view of all records
            local.Local1.Update.ArCsePerson.Number =
              local.ArClearCsePerson.Number;
            local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
              local.ObligorClearCsePersonsWorkSet.LastName;
            local.Local1.Update.ObligorCsePerson.Number =
              local.ObligorClearCsePerson.Number;
            local.Local1.Update.Collection.Assign(local.ClearCollection);
            local.Local1.Update.Retained.Amount = local.RetainedClear.Amount;
            local.Local1.Update.ForwardedToFamily.Amount =
              local.ForwardToFamilyClear.Amount;
            local.Local1.Update.ObligorObligor.Type1 = local.ClearObligor.Type1;
            local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
            local.Local1.Update.Obligation.SystemGeneratedIdentifier =
              local.ClearObligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.LocaleClear,
              local.Local1.Update.ObligationTransaction);
            local.Local1.Update.CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
              local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
              local.ClearCashReceiptType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
              local.ClearCashReceiptDetail.SequentialIdentifier;
            local.Local1.Update.ChPerson.Assign(local.ChPersonClear);
            local.Local1.Update.ArCsePersonsWorkSet.Assign(local.ChPersonClear);
            local.Local1.Update.Taf.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
          }

          local.Local1.CheckIndex();
          local.Local1.Index = -1;
          local.Local1.Count = 0;

          for(local.Local2.Index = 0; local.Local2.Index < local.Local2.Count; ++
            local.Local2.Index)
          {
            if (!local.Local2.CheckSize())
            {
              break;
            }

            ++local.Local1.Index;
            local.Local1.CheckSize();

            // reload orginal table without the AP that only had 718b money
            local.Local1.Update.ArCsePerson.Number =
              local.Local2.Item.ArGrp2CsePerson.Number;
            local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
              local.Local2.Item.ObligorGrp2CsePersonsWorkSet.LastName;
            local.Local1.Update.ObligorCsePerson.Number =
              local.Local2.Item.ObligorGrp2CsePerson.Number;
            local.Local1.Update.Collection.Assign(
              local.Local2.Item.Grp2Collection);
            local.Local1.Update.Retained.Amount =
              local.Local2.Item.RetainedGrp2.Amount;
            local.Local1.Update.ForwardedToFamily.Amount =
              local.Local2.Item.ForwardToFamilyGrp2.Amount;
            local.Local1.Update.ObligorObligor.Type1 =
              local.Local2.Item.ObligorGrp2Obligor.Type1;
            local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2ObligationType.SystemGeneratedIdentifier;
            local.Local1.Update.Obligation.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2Obligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.Local2.Item.
              Grp2ObligationTransaction,
              local.Local1.Update.ObligationTransaction);
            local.Local1.Update.CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.Local2.Item.Grp2CashReceiptSourceType.
                SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2CashReceiptEvent.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
              local.Local2.Item.Grp2CashReceiptType.SystemGeneratedIdentifier;
            local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
              local.Local2.Item.Grp2CashReceiptDetail.SequentialIdentifier;
            local.Local1.Update.ChPerson.Assign(local.Local2.Item.ChPersonGrp2);
            local.Local1.Update.ArCsePersonsWorkSet.Assign(
              local.Local2.Item.ArGrp2CsePersonsWorkSet);
            local.Local1.Update.Taf.SystemGeneratedIdentifier =
              local.Local2.Item.TafGrp2.SystemGeneratedIdentifier;
          }

          local.Local2.CheckIndex();
          local.Local2.Index = 0;

          for(var limit = local.Local2.Index + 1; local.Local2.Index < limit; ++
            local.Local2.Index)
          {
            if (!local.Local2.CheckSize())
            {
              break;
            }

            // clear 2nd group view and get ready for next time
            local.Local2.Update.ArGrp2CsePerson.Number =
              local.ArClearCsePerson.Number;
            local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
              local.ObligorClearCsePersonsWorkSet.LastName;
            local.Local2.Update.ObligorGrp2CsePerson.Number =
              local.ObligorClearCsePerson.Number;
            local.Local2.Update.Grp2Collection.Assign(local.ClearCollection);
            local.Local2.Update.RetainedGrp2.Amount =
              local.RetainedClear.Amount;
            local.Local2.Update.ForwardToFamilyGrp2.Amount =
              local.ForwardToFamilyClear.Amount;
            local.Local2.Update.ObligorGrp2Obligor.Type1 =
              local.ClearObligor.Type1;
            local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
              local.ClearObligation.SystemGeneratedIdentifier;
            MoveObligationTransaction(local.LocaleClear,
              local.Local2.Update.Grp2ObligationTransaction);
            local.Local2.Update.Grp2CashReceiptSourceType.
              SystemGeneratedIdentifier =
                local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
              local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
              local.ClearCashReceiptType.SystemGeneratedIdentifier;
            local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
              local.ClearCashReceiptDetail.SequentialIdentifier;
            local.Local2.Update.ChPersonGrp2.Assign(local.ChPersonClear);
            local.Local2.Update.ArGrp2CsePersonsWorkSet.Assign(
              local.ChPersonClear);
            local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
              local.ClearObligationType.SystemGeneratedIdentifier;
          }

          local.Local2.CheckIndex();
          local.Local2.Index = -1;
          local.Local2.Count = 0;
          local.PreviousAp.Number = "";
        }

        local.Local718Only.Flag = "";
        local.PreviousAp.Number = local.ObligorCsePerson.Number;
      }

      if (local.Local718B.SystemGeneratedIdentifier <= 0)
      {
        if (AsChar(local.Local718Only.Flag) == 'N')
        {
        }
        else
        {
          local.Local718Only.Flag = "Y";
        }
      }
      else
      {
        local.Local718Only.Flag = "N";
      }

      // -- Move extract file info to the group view.
      ++local.Local1.Index;
      local.Local1.CheckSize();

      local.Local1.Update.ArCsePerson.Number = local.ArCsePerson.Number;
      local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
        local.ObligorCsePersonsWorkSet.LastName;
      local.Local1.Update.ObligorCsePerson.Number =
        local.ObligorCsePerson.Number;
      local.Local1.Update.Collection.Assign(local.Collection);
      local.Local1.Update.Retained.Amount = local.Retained.Amount;
      local.Local1.Update.ForwardedToFamily.Amount =
        local.ForwardedToFamily.Amount;
      local.Local1.Update.ObligorObligor.Type1 = local.ObligorObligor.Type1;
      local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
        local.Local718B.SystemGeneratedIdentifier;
      local.Local1.Update.Obligation.SystemGeneratedIdentifier =
        local.Obligation.SystemGeneratedIdentifier;
      MoveObligationTransaction(local.ObligationTransaction,
        local.Local1.Update.ObligationTransaction);
      local.Local1.Update.CashReceiptSourceType.SystemGeneratedIdentifier =
        local.CashReceiptSourceType.SystemGeneratedIdentifier;
      local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
        local.CashReceiptEvent.SystemGeneratedIdentifier;
      local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
        local.CashReceiptType.SystemGeneratedIdentifier;
      local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
        local.CashReceiptDetail.SequentialIdentifier;
      local.Local1.Update.ChPerson.Assign(local.ChPerson);
      local.Local1.Update.ArCsePersonsWorkSet.Assign(local.ArCsePersonsWorkSet);
      local.Local1.Update.Taf.SystemGeneratedIdentifier =
        local.Taf.SystemGeneratedIdentifier;

      // -- Increment the running obligation type count to determine if any 
      // collections other than for 718B judgements were received during the
      // reporting period.
      local.Non718BCollection.Count += local.Local718B.
        SystemGeneratedIdentifier;
      local.TafCollection.Count += local.Taf.SystemGeneratedIdentifier;
    }
    while(Equal(local.External.TextReturnCode, "00"));

    if (AsChar(local.OkToSendRecord.Flag) == 'Y' || local
      .Non718BCollection.Count > 0)
    {
      if (AsChar(local.Local718Only.Flag) == 'Y')
      {
        // -- Take out the last AP if it was a 718b only AP, let the rest of the
        // record process.
        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          if (Equal(local.PreviousAp.Number,
            local.Local1.Item.ObligorCsePerson.Number))
          {
            continue;
          }

          ++local.Local2.Index;
          local.Local2.CheckSize();

          // load the 2nd group view with only records that do not include the 
          // AP with only 718b money
          local.Local2.Update.ArGrp2CsePerson.Number =
            local.Local1.Item.ArCsePerson.Number;
          local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
            local.Local1.Item.ObligorCsePersonsWorkSet.LastName;
          local.Local2.Update.ObligorGrp2CsePerson.Number =
            local.Local1.Item.ObligorCsePerson.Number;
          local.Local2.Update.Grp2Collection.
            Assign(local.Local1.Item.Collection);
          local.Local2.Update.RetainedGrp2.Amount =
            local.Local1.Item.Retained.Amount;
          local.Local2.Update.ForwardToFamilyGrp2.Amount =
            local.Local1.Item.ForwardedToFamily.Amount;
          local.Local2.Update.ObligorGrp2Obligor.Type1 =
            local.Local1.Item.ObligorObligor.Type1;
          local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
            local.Local1.Item.ObligationType.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
            local.Local1.Item.Obligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.Local1.Item.ObligationTransaction,
            local.Local2.Update.Grp2ObligationTransaction);
          local.Local2.Update.Grp2CashReceiptSourceType.
            SystemGeneratedIdentifier =
              local.Local1.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
            
          local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
            local.Local1.Item.CashReceiptEvent.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
            local.Local1.Item.CashReceiptType.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
            local.Local1.Item.CashReceiptDetail.SequentialIdentifier;
          local.Local2.Update.ChPersonGrp2.Assign(local.Local1.Item.ChPerson);
          local.Local2.Update.ArGrp2CsePersonsWorkSet.Assign(
            local.Local1.Item.ArCsePersonsWorkSet);
          local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
            local.Local1.Item.Taf.SystemGeneratedIdentifier;
        }

        local.Local1.CheckIndex();

        for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
          local.Local1.Index)
        {
          if (!local.Local1.CheckSize())
          {
            break;
          }

          // clean orginal group view of all records
          local.Local1.Update.ArCsePerson.Number =
            local.ArClearCsePerson.Number;
          local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
            local.ObligorClearCsePersonsWorkSet.LastName;
          local.Local1.Update.ObligorCsePerson.Number =
            local.ObligorClearCsePerson.Number;
          local.Local1.Update.Collection.Assign(local.ClearCollection);
          local.Local1.Update.Retained.Amount = local.RetainedClear.Amount;
          local.Local1.Update.ForwardedToFamily.Amount =
            local.ForwardToFamilyClear.Amount;
          local.Local1.Update.ObligorObligor.Type1 = local.ClearObligor.Type1;
          local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
          local.Local1.Update.Obligation.SystemGeneratedIdentifier =
            local.ClearObligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.LocaleClear,
            local.Local1.Update.ObligationTransaction);
          local.Local1.Update.CashReceiptSourceType.SystemGeneratedIdentifier =
            local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
            local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
            local.ClearCashReceiptType.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
            local.ClearCashReceiptDetail.SequentialIdentifier;
          local.Local1.Update.ChPerson.Assign(local.ChPersonClear);
          local.Local1.Update.ArCsePersonsWorkSet.Assign(local.ChPersonClear);
          local.Local1.Update.Taf.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
        }

        local.Local1.CheckIndex();
        local.Local1.Index = -1;
        local.Local1.Count = 0;

        for(local.Local2.Index = 0; local.Local2.Index < local.Local2.Count; ++
          local.Local2.Index)
        {
          if (!local.Local2.CheckSize())
          {
            break;
          }

          ++local.Local1.Index;
          local.Local1.CheckSize();

          // reload orginal table without the AP that only had 718b money
          local.Local1.Update.ArCsePerson.Number =
            local.Local2.Item.ArGrp2CsePerson.Number;
          local.Local1.Update.ObligorCsePersonsWorkSet.LastName =
            local.Local2.Item.ObligorGrp2CsePersonsWorkSet.LastName;
          local.Local1.Update.ObligorCsePerson.Number =
            local.Local2.Item.ObligorGrp2CsePerson.Number;
          local.Local1.Update.Collection.
            Assign(local.Local2.Item.Grp2Collection);
          local.Local1.Update.Retained.Amount =
            local.Local2.Item.RetainedGrp2.Amount;
          local.Local1.Update.ForwardedToFamily.Amount =
            local.Local2.Item.ForwardToFamilyGrp2.Amount;
          local.Local1.Update.ObligorObligor.Type1 =
            local.Local2.Item.ObligorGrp2Obligor.Type1;
          local.Local1.Update.ObligationType.SystemGeneratedIdentifier =
            local.Local2.Item.Grp2ObligationType.SystemGeneratedIdentifier;
          local.Local1.Update.Obligation.SystemGeneratedIdentifier =
            local.Local2.Item.Grp2Obligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.Local2.Item.Grp2ObligationTransaction,
            local.Local1.Update.ObligationTransaction);
          local.Local1.Update.CashReceiptSourceType.SystemGeneratedIdentifier =
            local.Local2.Item.Grp2CashReceiptSourceType.
              SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptEvent.SystemGeneratedIdentifier =
            local.Local2.Item.Grp2CashReceiptEvent.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptType.SystemGeneratedIdentifier =
            local.Local2.Item.Grp2CashReceiptType.SystemGeneratedIdentifier;
          local.Local1.Update.CashReceiptDetail.SequentialIdentifier =
            local.Local2.Item.Grp2CashReceiptDetail.SequentialIdentifier;
          local.Local1.Update.ChPerson.Assign(local.Local2.Item.ChPersonGrp2);
          local.Local1.Update.ArCsePersonsWorkSet.Assign(
            local.Local2.Item.ArGrp2CsePersonsWorkSet);
          local.Local1.Update.Taf.SystemGeneratedIdentifier =
            local.Local2.Item.TafGrp2.SystemGeneratedIdentifier;
        }

        local.Local2.CheckIndex();
        local.Local2.Index = 0;

        for(var limit = local.Local2.Index + 1; local.Local2.Index < limit; ++
          local.Local2.Index)
        {
          if (!local.Local2.CheckSize())
          {
            break;
          }

          // clear 2nd group view and get ready for next time
          local.Local2.Update.ArGrp2CsePerson.Number =
            local.ArClearCsePerson.Number;
          local.Local2.Update.ObligorGrp2CsePersonsWorkSet.LastName =
            local.ObligorClearCsePersonsWorkSet.LastName;
          local.Local2.Update.ObligorGrp2CsePerson.Number =
            local.ObligorClearCsePerson.Number;
          local.Local2.Update.Grp2Collection.Assign(local.ClearCollection);
          local.Local2.Update.RetainedGrp2.Amount = local.RetainedClear.Amount;
          local.Local2.Update.ForwardToFamilyGrp2.Amount =
            local.ForwardToFamilyClear.Amount;
          local.Local2.Update.ObligorGrp2Obligor.Type1 =
            local.ClearObligor.Type1;
          local.Local2.Update.Grp2ObligationType.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2Obligation.SystemGeneratedIdentifier =
            local.ClearObligation.SystemGeneratedIdentifier;
          MoveObligationTransaction(local.LocaleClear,
            local.Local2.Update.Grp2ObligationTransaction);
          local.Local2.Update.Grp2CashReceiptSourceType.
            SystemGeneratedIdentifier =
              local.ClearCashReceiptSourceType.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2CashReceiptEvent.SystemGeneratedIdentifier =
            local.ClearCashReceiptEvent.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2CashReceiptType.SystemGeneratedIdentifier =
            local.ClearCashReceiptType.SystemGeneratedIdentifier;
          local.Local2.Update.Grp2CashReceiptDetail.SequentialIdentifier =
            local.ClearCashReceiptDetail.SequentialIdentifier;
          local.Local2.Update.ChPersonGrp2.Assign(local.ChPersonClear);
          local.Local2.Update.ArGrp2CsePersonsWorkSet.
            Assign(local.ChPersonClear);
          local.Local2.Update.TafGrp2.SystemGeneratedIdentifier =
            local.ClearObligationType.SystemGeneratedIdentifier;
        }

        local.Local2.CheckIndex();
        local.Local2.Index = -1;
        local.Local2.Count = 0;
        local.PreviousAp.Number = "";
      }

      if (local.Non718BCollection.Count <= 0)
      {
        goto Test;
      }

      local.Local1.Index = 0;

      for(var limit = local.Local1.Count; local.Local1.Index < limit; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        UseFnB680ArExtractData2();

        if (!Equal(local.External.TextReturnCode, "OK"))
        {
          // --  write to error file...
          local.EabReportSend.RptDetail =
            "(01) Error writing collection info to extract file...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();
          ExitState = "ERROR_WRITING_TO_FILE_AB";

          goto Test;
        }
      }

      local.Local1.CheckIndex();
    }

Test:

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Take a final Checkpoint.
    // -------------------------------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdatePgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "Final Checkpoint Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseExtToDoACommit();

    if (local.External.NumericReturnCode != 0)
    {
      local.EabReportSend.RptDetail =
        "(04) Error in External Commit Routine.  Return Code = " + NumberToString
        (local.External.NumericReturnCode, 14, 2);
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Incoming Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB681ProcessArExtractData2();

    if (!Equal(local.External.TextReturnCode, "00"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Incoming Extract File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Outgoing Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    UseFnB680ArExtractData3();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Outgoing Extract File...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabControlReport();

    if (!Equal(local.Close.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .Close.Status;
      UseCabErrorReport1();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------------------------------------------
    UseCabErrorReport3();

    if (!Equal(local.Close.Status, "OK"))
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

  private static void MoveGroupToLocal1(FnB681DetermineArRecords.Export.
    GroupGroup source, Local.LocalGroup target)
  {
    target.ArCsePerson.Number = source.ArCsePerson.Number;
    target.ObligorCsePersonsWorkSet.LastName =
      source.ObligorCsePersonsWorkSet.LastName;
    target.ObligorCsePerson.Number = source.ObligorCsePerson.Number;
    target.Collection.Assign(source.Collection);
    target.Retained.Amount = source.Retained.Amount;
    target.ForwardedToFamily.Amount = source.ForwardedToFamily.Amount;
    target.ObligorObligor.Type1 = source.Obligor.Type1;
    target.ObligationType.SystemGeneratedIdentifier =
      source.ObligationType.SystemGeneratedIdentifier;
    target.Obligation.SystemGeneratedIdentifier =
      source.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(source.ObligationTransaction,
      target.ObligationTransaction);
    target.CashReceiptSourceType.SystemGeneratedIdentifier =
      source.CashReceiptSourceType.SystemGeneratedIdentifier;
    target.CashReceiptEvent.SystemGeneratedIdentifier =
      source.CashReceiptEvent.SystemGeneratedIdentifier;
    target.CashReceiptType.SystemGeneratedIdentifier =
      source.CashReceiptType.SystemGeneratedIdentifier;
    target.CashReceiptDetail.SequentialIdentifier =
      source.CashReceiptDetail.SequentialIdentifier;
    target.ChPerson.Assign(source.Ch);
    target.ArCsePersonsWorkSet.Assign(source.ArCsePersonsWorkSet);
    target.Taf.SystemGeneratedIdentifier = source.Taf.SystemGeneratedIdentifier;
  }

  private static void MoveLocal1ToGroup(Local.LocalGroup source,
    FnB681DetermineArRecords.Import.GroupGroup target)
  {
    target.ArCsePerson.Number = source.ArCsePerson.Number;
    target.ObligorCsePersonsWorkSet.LastName =
      source.ObligorCsePersonsWorkSet.LastName;
    target.ObligorCsePerson.Number = source.ObligorCsePerson.Number;
    target.Collection.Assign(source.Collection);
    target.Retained.Amount = source.Retained.Amount;
    target.ForwardedToFamily.Amount = source.ForwardedToFamily.Amount;
    target.Obligor.Type1 = source.ObligorObligor.Type1;
    target.ObligationType.SystemGeneratedIdentifier =
      source.ObligationType.SystemGeneratedIdentifier;
    target.Obligation.SystemGeneratedIdentifier =
      source.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(source.ObligationTransaction,
      target.ObligationTransaction);
    target.CashReceiptSourceType.SystemGeneratedIdentifier =
      source.CashReceiptSourceType.SystemGeneratedIdentifier;
    target.CashReceiptEvent.SystemGeneratedIdentifier =
      source.CashReceiptEvent.SystemGeneratedIdentifier;
    target.CashReceiptType.SystemGeneratedIdentifier =
      source.CashReceiptType.SystemGeneratedIdentifier;
    target.CashReceiptDetail.SequentialIdentifier =
      source.CashReceiptDetail.SequentialIdentifier;
    target.Ch.Assign(source.ChPerson);
    target.ArCsePersonsWorkSet.Assign(source.ArCsePersonsWorkSet);
    target.Taf.SystemGeneratedIdentifier = source.Taf.SystemGeneratedIdentifier;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveProgramCheckpointRestart1(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.RestartInd = source.RestartInd;
  }

  private static void MoveProgramCheckpointRestart2(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.Write.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Write.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.Close.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.Close.Status = useExport.EabFileHandling.Status;
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

  private void UseFnB680ArExtractData2()
  {
    var useImport = new FnB680ArExtractData1.Import();
    var useExport = new FnB680ArExtractData1.Export();

    useImport.EabFileHandling.Action = local.Write.Action;
    useImport.ArCsePerson.Number = local.Local1.Item.ArCsePerson.Number;
    useImport.ObligorCsePersonsWorkSet.LastName =
      local.Local1.Item.ObligorCsePersonsWorkSet.LastName;
    useImport.ObligorCsePerson.Number =
      local.Local1.Item.ObligorCsePerson.Number;
    useImport.Collection.Assign(local.Local1.Item.Collection);
    useImport.Retained.Amount = local.Local1.Item.Retained.Amount;
    useImport.ForwardedToFamily.Amount =
      local.Local1.Item.ForwardedToFamily.Amount;
    useImport.Obligor.Type1 = local.Local1.Item.ObligorObligor.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      local.Local1.Item.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      local.Local1.Item.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(local.Local1.Item.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.Local1.Item.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.Local1.Item.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.Local1.Item.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.Local1.Item.CashReceiptDetail.SequentialIdentifier;
    useImport.ChPerson.Assign(local.Local1.Item.ChPerson);
    useImport.ArCsePersonsWorkSet.Assign(local.Local1.Item.ArCsePersonsWorkSet);
    useImport.Taf.SystemGeneratedIdentifier =
      local.Local1.Item.Taf.SystemGeneratedIdentifier;
    useExport.External.Assign(local.External);

    Call(FnB680ArExtractData1.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnB680ArExtractData3()
  {
    var useImport = new FnB680ArExtractData1.Import();
    var useExport = new FnB680ArExtractData1.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.External.Assign(local.External);

    Call(FnB680ArExtractData1.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnB681BatchInitialization()
  {
    var useImport = new FnB681BatchInitialization.Import();
    var useExport = new FnB681BatchInitialization.Export();

    Call(FnB681BatchInitialization.Execute, useImport, useExport);

    local.CreateEvents.Flag = useExport.CreateEvents.Flag;
    local.ReportingPeriodEndingTextWorkArea.Text10 =
      useExport.ReportingPeriodEndingTextWorkArea.Text10;
    local.ReportingPeriodStartingTextWorkArea.Text10 =
      useExport.ReportingPeriodStartingTextWorkArea.Text10;
    local.ProgramProcessingInfo.ProcessDate =
      useExport.ProgramProcessingInfo.ProcessDate;
    MoveDateWorkArea(useExport.ReportingPeriodEndingDateWorkArea,
      local.ReportingPeriodEndingDateWorkArea);
    MoveDateWorkArea(useExport.ReportingPeriodStartingDateWorkArea,
      local.ReportingPeriodStartingDateWorkArea);
    MoveProgramCheckpointRestart1(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.Restart.Number = useExport.Restart.Number;
  }

  private void UseFnB681DetermineArRecords()
  {
    var useImport = new FnB681DetermineArRecords.Import();
    var useExport = new FnB681DetermineArRecords.Export();

    useImport.TafCollection.Count = local.TafCollection.Count;
    useImport.OkToSendRecord.Flag = local.OkToSendRecord.Flag;
    useImport.Ap.Number = local.ObligorCsePerson.Number;
    useImport.Non718BCollection.Count = local.Non718BCollection.Count;
    useImport.ReportingPeriodEndingDateWorkArea.Date =
      local.ReportingPeriodEndingDateWorkArea.Date;
    useImport.Ar.Number = local.ArCsePerson.Number;
    local.Local1.CopyTo(useImport.Group, MoveLocal1ToGroup);
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.ReportingPeriodStarting.Text10 =
      local.ReportingPeriodStartingTextWorkArea.Text10;
    useImport.ReportingPeriodEndingTextWorkArea.Text10 =
      local.ReportingPeriodEndingTextWorkArea.Text10;
    useImport.Voluntary.SystemGeneratedIdentifier =
      local.Voluntary.SystemGeneratedIdentifier;
    useImport.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      local.SpousalArrearsJudgement.SystemGeneratedIdentifier;
    useImport.Import718B.SystemGeneratedIdentifier =
      local.Local718B.SystemGeneratedIdentifier;
    useImport.Taf.SystemGeneratedIdentifier =
      local.Taf.SystemGeneratedIdentifier;
    useImport.SpousalSupport.SystemGeneratedIdentifier =
      local.SpousalSupport.SystemGeneratedIdentifier;
    useImport.Import718BType.SystemGeneratedIdentifier =
      local.Local718BType.SystemGeneratedIdentifier;

    Call(FnB681DetermineArRecords.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Local1, MoveGroupToLocal1);
    local.OkToSendRecord.Flag = useExport.OkToSendRecord.Flag;
  }

  private void UseFnB681ProcessArExtractData1()
  {
    var useImport = new FnB681ProcessArExtractData.Import();
    var useExport = new FnB681ProcessArExtractData.Export();

    useImport.EabFileHandling.Action = local.Read.Action;
    useExport.ArCsePerson.Number = local.ArCsePerson.Number;
    useExport.ObligorCsePersonsWorkSet.LastName =
      local.ObligorCsePersonsWorkSet.LastName;
    useExport.ObligorCsePerson.Number = local.ObligorCsePerson.Number;
    useExport.Collection.Assign(local.Collection);
    useExport.Retained.Amount = local.Retained.Amount;
    useExport.ForwardToFamily.Amount = local.ForwardedToFamily.Amount;
    useExport.Obligor.Type1 = local.ObligorObligor.Type1;
    useExport.ObligationType.SystemGeneratedIdentifier =
      local.Local718B.SystemGeneratedIdentifier;
    useExport.Obligation.SystemGeneratedIdentifier =
      local.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(local.ObligationTransaction,
      useExport.ObligationTransaction);
    useExport.CashReceiptSourceType.SystemGeneratedIdentifier =
      local.CashReceiptSourceType.SystemGeneratedIdentifier;
    useExport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useExport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useExport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;
    useExport.ChPerson.Assign(local.ChPerson);
    useExport.ArCsePersonsWorkSet.Assign(local.ArCsePersonsWorkSet);
    useExport.External.Assign(local.External);
    useExport.Taf.SystemGeneratedIdentifier =
      local.Taf.SystemGeneratedIdentifier;

    Call(FnB681ProcessArExtractData.Execute, useImport, useExport);

    local.ArCsePerson.Number = useExport.ArCsePerson.Number;
    local.ObligorCsePersonsWorkSet.LastName =
      useExport.ObligorCsePersonsWorkSet.LastName;
    local.ObligorCsePerson.Number = useExport.ObligorCsePerson.Number;
    local.Collection.Assign(useExport.Collection);
    local.Retained.Amount = useExport.Retained.Amount;
    local.ForwardedToFamily.Amount = useExport.ForwardToFamily.Amount;
    local.ObligorObligor.Type1 = useExport.Obligor.Type1;
    local.Local718B.SystemGeneratedIdentifier =
      useExport.ObligationType.SystemGeneratedIdentifier;
    local.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
    MoveObligationTransaction(useExport.ObligationTransaction,
      local.ObligationTransaction);
    local.CashReceiptSourceType.SystemGeneratedIdentifier =
      useExport.CashReceiptSourceType.SystemGeneratedIdentifier;
    local.CashReceiptEvent.SystemGeneratedIdentifier =
      useExport.CashReceiptEvent.SystemGeneratedIdentifier;
    local.CashReceiptType.SystemGeneratedIdentifier =
      useExport.CashReceiptType.SystemGeneratedIdentifier;
    local.CashReceiptDetail.SequentialIdentifier =
      useExport.CashReceiptDetail.SequentialIdentifier;
    local.ChPerson.Assign(useExport.ChPerson);
    local.ArCsePersonsWorkSet.Assign(useExport.ArCsePersonsWorkSet);
    local.External.Assign(useExport.External);
    local.Taf.SystemGeneratedIdentifier =
      useExport.Taf.SystemGeneratedIdentifier;
  }

  private void UseFnB681ProcessArExtractData2()
  {
    var useImport = new FnB681ProcessArExtractData.Import();
    var useExport = new FnB681ProcessArExtractData.Export();

    useImport.EabFileHandling.Action = local.Close.Action;
    useExport.External.Assign(local.External);

    Call(FnB681ProcessArExtractData.Execute, useImport, useExport);

    local.External.Assign(useExport.External);
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.Local718BType.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    local.Voluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.SpousalArrearsJudgement.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.SpousalSupport.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    MoveProgramCheckpointRestart2(local.ProgramCheckpointRestart,
      useImport.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
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
    /// <summary>A Local2Group group.</summary>
    [Serializable]
    public class Local2Group
    {
      /// <summary>
      /// A value of ArGrp2CsePerson.
      /// </summary>
      [JsonPropertyName("arGrp2CsePerson")]
      public CsePerson ArGrp2CsePerson
      {
        get => arGrp2CsePerson ??= new();
        set => arGrp2CsePerson = value;
      }

      /// <summary>
      /// A value of ObligorGrp2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("obligorGrp2CsePersonsWorkSet")]
      public CsePersonsWorkSet ObligorGrp2CsePersonsWorkSet
      {
        get => obligorGrp2CsePersonsWorkSet ??= new();
        set => obligorGrp2CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ObligorGrp2CsePerson.
      /// </summary>
      [JsonPropertyName("obligorGrp2CsePerson")]
      public CsePerson ObligorGrp2CsePerson
      {
        get => obligorGrp2CsePerson ??= new();
        set => obligorGrp2CsePerson = value;
      }

      /// <summary>
      /// A value of Grp2Collection.
      /// </summary>
      [JsonPropertyName("grp2Collection")]
      public Collection Grp2Collection
      {
        get => grp2Collection ??= new();
        set => grp2Collection = value;
      }

      /// <summary>
      /// A value of RetainedGrp2.
      /// </summary>
      [JsonPropertyName("retainedGrp2")]
      public Collection RetainedGrp2
      {
        get => retainedGrp2 ??= new();
        set => retainedGrp2 = value;
      }

      /// <summary>
      /// A value of ForwardToFamilyGrp2.
      /// </summary>
      [JsonPropertyName("forwardToFamilyGrp2")]
      public Collection ForwardToFamilyGrp2
      {
        get => forwardToFamilyGrp2 ??= new();
        set => forwardToFamilyGrp2 = value;
      }

      /// <summary>
      /// A value of ObligorGrp2Obligor.
      /// </summary>
      [JsonPropertyName("obligorGrp2Obligor")]
      public CsePersonAccount ObligorGrp2Obligor
      {
        get => obligorGrp2Obligor ??= new();
        set => obligorGrp2Obligor = value;
      }

      /// <summary>
      /// A value of Grp2ObligationType.
      /// </summary>
      [JsonPropertyName("grp2ObligationType")]
      public ObligationType Grp2ObligationType
      {
        get => grp2ObligationType ??= new();
        set => grp2ObligationType = value;
      }

      /// <summary>
      /// A value of Grp2Obligation.
      /// </summary>
      [JsonPropertyName("grp2Obligation")]
      public Obligation Grp2Obligation
      {
        get => grp2Obligation ??= new();
        set => grp2Obligation = value;
      }

      /// <summary>
      /// A value of Grp2ObligationTransaction.
      /// </summary>
      [JsonPropertyName("grp2ObligationTransaction")]
      public ObligationTransaction Grp2ObligationTransaction
      {
        get => grp2ObligationTransaction ??= new();
        set => grp2ObligationTransaction = value;
      }

      /// <summary>
      /// A value of Grp2CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("grp2CashReceiptSourceType")]
      public CashReceiptSourceType Grp2CashReceiptSourceType
      {
        get => grp2CashReceiptSourceType ??= new();
        set => grp2CashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of Grp2CashReceiptEvent.
      /// </summary>
      [JsonPropertyName("grp2CashReceiptEvent")]
      public CashReceiptEvent Grp2CashReceiptEvent
      {
        get => grp2CashReceiptEvent ??= new();
        set => grp2CashReceiptEvent = value;
      }

      /// <summary>
      /// A value of Grp2CashReceiptType.
      /// </summary>
      [JsonPropertyName("grp2CashReceiptType")]
      public CashReceiptType Grp2CashReceiptType
      {
        get => grp2CashReceiptType ??= new();
        set => grp2CashReceiptType = value;
      }

      /// <summary>
      /// A value of Grp2CashReceiptDetail.
      /// </summary>
      [JsonPropertyName("grp2CashReceiptDetail")]
      public CashReceiptDetail Grp2CashReceiptDetail
      {
        get => grp2CashReceiptDetail ??= new();
        set => grp2CashReceiptDetail = value;
      }

      /// <summary>
      /// A value of ChPersonGrp2.
      /// </summary>
      [JsonPropertyName("chPersonGrp2")]
      public CsePersonsWorkSet ChPersonGrp2
      {
        get => chPersonGrp2 ??= new();
        set => chPersonGrp2 = value;
      }

      /// <summary>
      /// A value of ArGrp2CsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("arGrp2CsePersonsWorkSet")]
      public CsePersonsWorkSet ArGrp2CsePersonsWorkSet
      {
        get => arGrp2CsePersonsWorkSet ??= new();
        set => arGrp2CsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of TafGrp2.
      /// </summary>
      [JsonPropertyName("tafGrp2")]
      public ObligationType TafGrp2
      {
        get => tafGrp2 ??= new();
        set => tafGrp2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson arGrp2CsePerson;
      private CsePersonsWorkSet obligorGrp2CsePersonsWorkSet;
      private CsePerson obligorGrp2CsePerson;
      private Collection grp2Collection;
      private Collection retainedGrp2;
      private Collection forwardToFamilyGrp2;
      private CsePersonAccount obligorGrp2Obligor;
      private ObligationType grp2ObligationType;
      private Obligation grp2Obligation;
      private ObligationTransaction grp2ObligationTransaction;
      private CashReceiptSourceType grp2CashReceiptSourceType;
      private CashReceiptEvent grp2CashReceiptEvent;
      private CashReceiptType grp2CashReceiptType;
      private CashReceiptDetail grp2CashReceiptDetail;
      private CsePersonsWorkSet chPersonGrp2;
      private CsePersonsWorkSet arGrp2CsePersonsWorkSet;
      private ObligationType tafGrp2;
    }

    /// <summary>A StatementCountGroup group.</summary>
    [Serializable]
    public class StatementCountGroup
    {
      /// <summary>
      /// A value of GlocalCount.
      /// </summary>
      [JsonPropertyName("glocalCount")]
      public Common GlocalCount
      {
        get => glocalCount ??= new();
        set => glocalCount = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common glocalCount;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of ArCsePerson.
      /// </summary>
      [JsonPropertyName("arCsePerson")]
      public CsePerson ArCsePerson
      {
        get => arCsePerson ??= new();
        set => arCsePerson = value;
      }

      /// <summary>
      /// A value of ObligorCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("obligorCsePersonsWorkSet")]
      public CsePersonsWorkSet ObligorCsePersonsWorkSet
      {
        get => obligorCsePersonsWorkSet ??= new();
        set => obligorCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ObligorCsePerson.
      /// </summary>
      [JsonPropertyName("obligorCsePerson")]
      public CsePerson ObligorCsePerson
      {
        get => obligorCsePerson ??= new();
        set => obligorCsePerson = value;
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
      /// A value of Retained.
      /// </summary>
      [JsonPropertyName("retained")]
      public Collection Retained
      {
        get => retained ??= new();
        set => retained = value;
      }

      /// <summary>
      /// A value of ForwardedToFamily.
      /// </summary>
      [JsonPropertyName("forwardedToFamily")]
      public Collection ForwardedToFamily
      {
        get => forwardedToFamily ??= new();
        set => forwardedToFamily = value;
      }

      /// <summary>
      /// A value of ObligorObligor.
      /// </summary>
      [JsonPropertyName("obligorObligor")]
      public CsePersonAccount ObligorObligor
      {
        get => obligorObligor ??= new();
        set => obligorObligor = value;
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
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
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
      /// A value of ChPerson.
      /// </summary>
      [JsonPropertyName("chPerson")]
      public CsePersonsWorkSet ChPerson
      {
        get => chPerson ??= new();
        set => chPerson = value;
      }

      /// <summary>
      /// A value of ArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("arCsePersonsWorkSet")]
      public CsePersonsWorkSet ArCsePersonsWorkSet
      {
        get => arCsePersonsWorkSet ??= new();
        set => arCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of Taf.
      /// </summary>
      [JsonPropertyName("taf")]
      public ObligationType Taf
      {
        get => taf ??= new();
        set => taf = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1000;

      private CsePerson arCsePerson;
      private CsePersonsWorkSet obligorCsePersonsWorkSet;
      private CsePerson obligorCsePerson;
      private Collection collection;
      private Collection retained;
      private Collection forwardedToFamily;
      private CsePersonAccount obligorObligor;
      private ObligationType obligationType;
      private Obligation obligation;
      private ObligationTransaction obligationTransaction;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceiptEvent cashReceiptEvent;
      private CashReceiptType cashReceiptType;
      private CashReceiptDetail cashReceiptDetail;
      private CsePersonsWorkSet chPerson;
      private CsePersonsWorkSet arCsePersonsWorkSet;
      private ObligationType taf;
    }

    /// <summary>
    /// Gets a value of Local2.
    /// </summary>
    [JsonIgnore]
    public Array<Local2Group> Local2 => local2 ??= new(Local2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Local2 for json serialization.
    /// </summary>
    [JsonPropertyName("local2")]
    [Computed]
    public IList<Local2Group> Local2_Json
    {
      get => local2;
      set => Local2.Assign(value);
    }

    /// <summary>
    /// A value of LastGoodRecordCount.
    /// </summary>
    [JsonPropertyName("lastGoodRecordCount")]
    public Common LastGoodRecordCount
    {
      get => lastGoodRecordCount ??= new();
      set => lastGoodRecordCount = value;
    }

    /// <summary>
    /// A value of Local718BType.
    /// </summary>
    [JsonPropertyName("local718BType")]
    public ObligationType Local718BType
    {
      get => local718BType ??= new();
      set => local718BType = value;
    }

    /// <summary>
    /// A value of Local718Only.
    /// </summary>
    [JsonPropertyName("local718Only")]
    public Common Local718Only
    {
      get => local718Only ??= new();
      set => local718Only = value;
    }

    /// <summary>
    /// A value of TafCollection.
    /// </summary>
    [JsonPropertyName("tafCollection")]
    public Common TafCollection
    {
      get => tafCollection ??= new();
      set => tafCollection = value;
    }

    /// <summary>
    /// A value of Taf.
    /// </summary>
    [JsonPropertyName("taf")]
    public ObligationType Taf
    {
      get => taf ??= new();
      set => taf = value;
    }

    /// <summary>
    /// A value of ArClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arClearCsePersonsWorkSet")]
    public CsePersonsWorkSet ArClearCsePersonsWorkSet
    {
      get => arClearCsePersonsWorkSet ??= new();
      set => arClearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChPersonClear.
    /// </summary>
    [JsonPropertyName("chPersonClear")]
    public CsePersonsWorkSet ChPersonClear
    {
      get => chPersonClear ??= new();
      set => chPersonClear = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("clearCashReceiptDetail")]
    public CashReceiptDetail ClearCashReceiptDetail
    {
      get => clearCashReceiptDetail ??= new();
      set => clearCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptType.
    /// </summary>
    [JsonPropertyName("clearCashReceiptType")]
    public CashReceiptType ClearCashReceiptType
    {
      get => clearCashReceiptType ??= new();
      set => clearCashReceiptType = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("clearCashReceiptEvent")]
    public CashReceiptEvent ClearCashReceiptEvent
    {
      get => clearCashReceiptEvent ??= new();
      set => clearCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of ClearCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("clearCashReceiptSourceType")]
    public CashReceiptSourceType ClearCashReceiptSourceType
    {
      get => clearCashReceiptSourceType ??= new();
      set => clearCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of LocaleClear.
    /// </summary>
    [JsonPropertyName("localeClear")]
    public ObligationTransaction LocaleClear
    {
      get => localeClear ??= new();
      set => localeClear = value;
    }

    /// <summary>
    /// A value of ClearObligation.
    /// </summary>
    [JsonPropertyName("clearObligation")]
    public Obligation ClearObligation
    {
      get => clearObligation ??= new();
      set => clearObligation = value;
    }

    /// <summary>
    /// A value of ClearObligor.
    /// </summary>
    [JsonPropertyName("clearObligor")]
    public CsePersonAccount ClearObligor
    {
      get => clearObligor ??= new();
      set => clearObligor = value;
    }

    /// <summary>
    /// A value of ObligorClearCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorClearCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorClearCsePersonsWorkSet
    {
      get => obligorClearCsePersonsWorkSet ??= new();
      set => obligorClearCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ClearObligationType.
    /// </summary>
    [JsonPropertyName("clearObligationType")]
    public ObligationType ClearObligationType
    {
      get => clearObligationType ??= new();
      set => clearObligationType = value;
    }

    /// <summary>
    /// A value of ForwardToFamilyClear.
    /// </summary>
    [JsonPropertyName("forwardToFamilyClear")]
    public Collection ForwardToFamilyClear
    {
      get => forwardToFamilyClear ??= new();
      set => forwardToFamilyClear = value;
    }

    /// <summary>
    /// A value of RetainedClear.
    /// </summary>
    [JsonPropertyName("retainedClear")]
    public Collection RetainedClear
    {
      get => retainedClear ??= new();
      set => retainedClear = value;
    }

    /// <summary>
    /// A value of ClearCollection.
    /// </summary>
    [JsonPropertyName("clearCollection")]
    public Collection ClearCollection
    {
      get => clearCollection ??= new();
      set => clearCollection = value;
    }

    /// <summary>
    /// A value of ObligorClearCsePerson.
    /// </summary>
    [JsonPropertyName("obligorClearCsePerson")]
    public CsePerson ObligorClearCsePerson
    {
      get => obligorClearCsePerson ??= new();
      set => obligorClearCsePerson = value;
    }

    /// <summary>
    /// A value of ArClearCsePerson.
    /// </summary>
    [JsonPropertyName("arClearCsePerson")]
    public CsePerson ArClearCsePerson
    {
      get => arClearCsePerson ??= new();
      set => arClearCsePerson = value;
    }

    /// <summary>
    /// A value of OkToSendRecord.
    /// </summary>
    [JsonPropertyName("okToSendRecord")]
    public Common OkToSendRecord
    {
      get => okToSendRecord ??= new();
      set => okToSendRecord = value;
    }

    /// <summary>
    /// A value of PreviousAp.
    /// </summary>
    [JsonPropertyName("previousAp")]
    public CsePerson PreviousAp
    {
      get => previousAp ??= new();
      set => previousAp = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChPerson.
    /// </summary>
    [JsonPropertyName("chPerson")]
    public CsePersonsWorkSet ChPerson
    {
      get => chPerson ??= new();
      set => chPerson = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ObligorObligor.
    /// </summary>
    [JsonPropertyName("obligorObligor")]
    public CsePersonAccount ObligorObligor
    {
      get => obligorObligor ??= new();
      set => obligorObligor = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Local718B.
    /// </summary>
    [JsonPropertyName("local718B")]
    public ObligationType Local718B
    {
      get => local718B ??= new();
      set => local718B = value;
    }

    /// <summary>
    /// A value of ForwardedToFamily.
    /// </summary>
    [JsonPropertyName("forwardedToFamily")]
    public Collection ForwardedToFamily
    {
      get => forwardedToFamily ??= new();
      set => forwardedToFamily = value;
    }

    /// <summary>
    /// A value of Retained.
    /// </summary>
    [JsonPropertyName("retained")]
    public Collection Retained
    {
      get => retained ??= new();
      set => retained = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of CreateEvents.
    /// </summary>
    [JsonPropertyName("createEvents")]
    public Common CreateEvents
    {
      get => createEvents ??= new();
      set => createEvents = value;
    }

    /// <summary>
    /// A value of ArStatementStatus.
    /// </summary>
    [JsonPropertyName("arStatementStatus")]
    public TextWorkArea ArStatementStatus
    {
      get => arStatementStatus ??= new();
      set => arStatementStatus = value;
    }

    /// <summary>
    /// Gets a value of StatementCount.
    /// </summary>
    [JsonIgnore]
    public Array<StatementCountGroup> StatementCount => statementCount ??= new(
      StatementCountGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of StatementCount for json serialization.
    /// </summary>
    [JsonPropertyName("statementCount")]
    [Computed]
    public IList<StatementCountGroup> StatementCount_Json
    {
      get => statementCount;
      set => StatementCount.Assign(value);
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
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabFileHandling Write
    {
      get => write ??= new();
      set => write = value;
    }

    /// <summary>
    /// A value of Close.
    /// </summary>
    [JsonPropertyName("close")]
    public EabFileHandling Close
    {
      get => close ??= new();
      set => close = value;
    }

    /// <summary>
    /// A value of Read.
    /// </summary>
    [JsonPropertyName("read")]
    public EabFileHandling Read
    {
      get => read ??= new();
      set => read = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingTextWorkArea")]
    public TextWorkArea ReportingPeriodStartingTextWorkArea
    {
      get => reportingPeriodStartingTextWorkArea ??= new();
      set => reportingPeriodStartingTextWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingTextWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingTextWorkArea")]
    public TextWorkArea ReportingPeriodEndingTextWorkArea
    {
      get => reportingPeriodEndingTextWorkArea ??= new();
      set => reportingPeriodEndingTextWorkArea = value;
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
    /// A value of NumOfStatementsErrored.
    /// </summary>
    [JsonPropertyName("numOfStatementsErrored")]
    public Common NumOfStatementsErrored
    {
      get => numOfStatementsErrored ??= new();
      set => numOfStatementsErrored = value;
    }

    /// <summary>
    /// A value of NumOfStatementsPrinted.
    /// </summary>
    [JsonPropertyName("numOfStatementsPrinted")]
    public Common NumOfStatementsPrinted
    {
      get => numOfStatementsPrinted ??= new();
      set => numOfStatementsPrinted = value;
    }

    /// <summary>
    /// A value of NumOfStatementsNoassob.
    /// </summary>
    [JsonPropertyName("numOfStatementsNoassob")]
    public Common NumOfStatementsNoassob
    {
      get => numOfStatementsNoassob ??= new();
      set => numOfStatementsNoassob = value;
    }

    /// <summary>
    /// A value of NumOfStatementsDeceased.
    /// </summary>
    [JsonPropertyName("numOfStatementsDeceased")]
    public Common NumOfStatementsDeceased
    {
      get => numOfStatementsDeceased ??= new();
      set => numOfStatementsDeceased = value;
    }

    /// <summary>
    /// A value of NumOfStatements718B.
    /// </summary>
    [JsonPropertyName("numOfStatements718B")]
    public Common NumOfStatements718B
    {
      get => numOfStatements718B ??= new();
      set => numOfStatements718B = value;
    }

    /// <summary>
    /// A value of Non718BCollection.
    /// </summary>
    [JsonPropertyName("non718BCollection")]
    public Common Non718BCollection
    {
      get => non718BCollection ??= new();
      set => non718BCollection = value;
    }

    /// <summary>
    /// A value of NumOfStatementsNoactadd.
    /// </summary>
    [JsonPropertyName("numOfStatementsNoactadd")]
    public Common NumOfStatementsNoactadd
    {
      get => numOfStatementsNoactadd ??= new();
      set => numOfStatementsNoactadd = value;
    }

    /// <summary>
    /// A value of PreviousAr.
    /// </summary>
    [JsonPropertyName("previousAr")]
    public CsePerson PreviousAr
    {
      get => previousAr ??= new();
      set => previousAr = value;
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of Counter.
    /// </summary>
    [JsonPropertyName("counter")]
    public Common Counter
    {
      get => counter ??= new();
      set => counter = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public CsePerson Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of ReadCount.
    /// </summary>
    [JsonPropertyName("readCount")]
    public Common ReadCount
    {
      get => readCount ??= new();
      set => readCount = value;
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
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodEndingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndingDateWorkArea")]
    public DateWorkArea ReportingPeriodEndingDateWorkArea
    {
      get => reportingPeriodEndingDateWorkArea ??= new();
      set => reportingPeriodEndingDateWorkArea = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartingDateWorkArea.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartingDateWorkArea")]
    public DateWorkArea ReportingPeriodStartingDateWorkArea
    {
      get => reportingPeriodStartingDateWorkArea ??= new();
      set => reportingPeriodStartingDateWorkArea = value;
    }

    /// <summary>
    /// A value of Voluntary.
    /// </summary>
    [JsonPropertyName("voluntary")]
    public ObligationType Voluntary
    {
      get => voluntary ??= new();
      set => voluntary = value;
    }

    /// <summary>
    /// A value of SpousalArrearsJudgement.
    /// </summary>
    [JsonPropertyName("spousalArrearsJudgement")]
    public ObligationType SpousalArrearsJudgement
    {
      get => spousalArrearsJudgement ??= new();
      set => spousalArrearsJudgement = value;
    }

    /// <summary>
    /// A value of SpousalSupport.
    /// </summary>
    [JsonPropertyName("spousalSupport")]
    public ObligationType SpousalSupport
    {
      get => spousalSupport ??= new();
      set => spousalSupport = value;
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

    private Array<Local2Group> local2;
    private Common lastGoodRecordCount;
    private ObligationType local718BType;
    private Common local718Only;
    private Common tafCollection;
    private ObligationType taf;
    private CsePersonsWorkSet arClearCsePersonsWorkSet;
    private CsePersonsWorkSet chPersonClear;
    private CashReceiptDetail clearCashReceiptDetail;
    private CashReceiptType clearCashReceiptType;
    private CashReceiptEvent clearCashReceiptEvent;
    private CashReceiptSourceType clearCashReceiptSourceType;
    private ObligationTransaction localeClear;
    private Obligation clearObligation;
    private CsePersonAccount clearObligor;
    private CsePersonsWorkSet obligorClearCsePersonsWorkSet;
    private ObligationType clearObligationType;
    private Collection forwardToFamilyClear;
    private Collection retainedClear;
    private Collection clearCollection;
    private CsePerson obligorClearCsePerson;
    private CsePerson arClearCsePerson;
    private Common okToSendRecord;
    private CsePerson previousAp;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet chPerson;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private CsePersonAccount obligorObligor;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private ObligationType local718B;
    private Collection forwardedToFamily;
    private Collection retained;
    private Collection collection;
    private CsePerson obligorCsePerson;
    private CsePerson arCsePerson;
    private Common createEvents;
    private TextWorkArea arStatementStatus;
    private Array<StatementCountGroup> statementCount;
    private EabReportSend eabReportSend;
    private EabFileHandling write;
    private EabFileHandling close;
    private EabFileHandling read;
    private TextWorkArea reportingPeriodStartingTextWorkArea;
    private TextWorkArea reportingPeriodEndingTextWorkArea;
    private ProgramProcessingInfo programProcessingInfo;
    private Common numOfStatementsErrored;
    private Common numOfStatementsPrinted;
    private Common numOfStatementsNoassob;
    private Common numOfStatementsDeceased;
    private Common numOfStatements718B;
    private Common non718BCollection;
    private Common numOfStatementsNoactadd;
    private CsePerson previousAr;
    private Array<LocalGroup> local1;
    private Common counter;
    private External external;
    private CsePerson restart;
    private Common readCount;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea reportingPeriodEndingDateWorkArea;
    private DateWorkArea reportingPeriodStartingDateWorkArea;
    private ObligationType voluntary;
    private ObligationType spousalArrearsJudgement;
    private ObligationType spousalSupport;
    private EabFileHandling eabFileHandling;
  }
#endregion
}

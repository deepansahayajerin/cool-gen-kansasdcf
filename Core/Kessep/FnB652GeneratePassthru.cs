// Program: FN_B652_GENERATE_PASSTHRU, ID: 372708110, model: 746.
// Short name: SWEF652B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B652_GENERATE_PASSTHRU.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB652GeneratePassthru: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_GENERATE_PASSTHRU program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652GeneratePassthru(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652GeneratePassthru.
  /// </summary>
  public FnB652GeneratePassthru(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	By	IDCR#	DEscription
    // 111797	govind		Initial code. This replaces the Passthru Credits (B650), 
    // Debits (B651) and Passthru Warrants (B655)
    // 120197	govind		Fixed to pick up only those disbursement debits with 
    // collection date <= process date.
    // 011698  R.B.Mohapatra	Incorporated new requirements to populate the 
    // interface income
    //                         notification table with the passthru amounts 
    // received by the AR
    // 011998	govind		Fixed to handle where the collection is a 'future' 
    // collection. i.e. Collection month < Debt Due month.
    // 			Fixed to select only ADC Current Child Support disbursements
    // 013098	govind		Fixed to select AF Current MS and AF Current MC 
    // disbursements also
    // 022698	govind		close adabas
    // 031098	govind		Skip Interstate disbursements.
    // 020899  N.Engoor        Fixed  to select ADC Voluntary Payment also.
    // 020899  N.Engoor        Removed old AB's dealing with error  and control 
    // reports and added new ones.
    // 050999 N.Engoor         Deleted the Hardcoded EFT CAB since it was not 
    // being used
    // 04192000 K.Doshi  PR#93468
    // Skip Non-cash payments when creating passthrus.
    // ---------------------------------------------
    // ---------------------------------------------
    // This batch procedure 'nets' the passthru disbursement debits and creates 
    // either a Warrant/EFT or a potential recovery based on whether the net
    // amount is positive or negative.
    // If any recapture is involved, it creates a disb debit of type recapture 
    // and creates a payment request of type 'RCP' for the same amount. It also
    // creates the cash receipt.
    // N.Engoor  - 04281999
    // If any recapture is involved, it creates the cash receipt detail. The 
    // distribution part of it will be taken care off in the Distribution batch.
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // --------------
    // Get the run parameters for this program.
    // --------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    local.CreateIntfRecFor.Date = local.ProgramProcessingInfo.ProcessDate;
    local.CreateIntfRecFor.YearMonth = UseCabGetYearMonthFromDate3();
    MoveDateWorkArea(local.CreateIntfRecFor, local.PrevMonthCreateIntrfc);

    if (Month(local.PrevMonthCreateIntrfc.Date) == 1)
    {
      local.PrevMonthCreateIntrfc.Month = 12;
      local.PrevMonthCreateIntrfc.Year =
        Year(local.ProgramProcessingInfo.ProcessDate) - 1;
    }
    else
    {
      local.PrevMonthCreateIntrfc.Month =
        Month(AddMonths(local.ProgramProcessingInfo.ProcessDate, -1));
      local.PrevMonthCreateIntrfc.Year =
        Year(local.ProgramProcessingInfo.ProcessDate);
    }

    local.PrevMonthCreateIntrfc.YearMonth =
      (int)StringToNumber(
        NumberToString(local.PrevMonthCreateIntrfc.Year, 12, 4) +
      NumberToString(local.PrevMonthCreateIntrfc.Month, 14, 2));

    // ----------------------------------------
    // Open Error report - 99.
    // ----------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------
    // Open Control  report - 98.
    // ----------------------------------------
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ***** Get the DB2 commit frequency counts.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.RestartObligee.Number =
        TrimEnd(local.ProgramCheckpointRestart.RestartInfo);
    }
    else
    {
      local.RestartObligee.Number = "";
    }

    // ----------------
    // Read the program_run each time we come into this program so that we will 
    // have currency for creating any error rows or control total rows.
    // ----------------
    UseFnHardcodedDisbursementInfo();
    local.DisbTranRead.Flag = "N";
    local.NetAdcCurCollecton.Amount = 0;
    local.ProgramCheckpointRestart.CheckpointCount = 0;

    // --------------
    // For "System Test"  all disbursement debits with a collection date (
    // greater than or equal to 1st July 1999 i.e. after the conversion process)
    // are only supposed to be taken into consideration.
    // This date will be updated to Sep 1st 1999 once the system goes into 
    // production so that no disb debits with a collection date less than Sep
    // 1st 1999 will not be read.
    // --------------
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      local.HardcodeAfterAug3199.CollectionDate =
        StringToDate(local.ProgramProcessingInfo.ParameterList);
    }

    local.TempProcessDate.Date = local.ProgramProcessingInfo.ProcessDate;

    if (Day(local.ProgramProcessingInfo.ProcessDate) == 1)
    {
      // --------------
      // Continue processing.
      // --------------
    }
    else
    {
      // --------------
      // Retrieve the start date of this month.
      // --------------
      UseCabGetYearMonthFromDate2();
      UseOeGetMonthStartAndEndDate();

      if (AsChar(local.InvalidMonth.Flag) == 'Y')
      {
        ExitState = "FN0000_INVALID_MONTH_YEAR";

        return;
      }
    }

    // --------------------------------------------------------
    // Select disbursement transactions that have not been
    // processed yet for passthrus. Process by the collection
    // month.
    // 04192000 K.Doshi  PR#93468
    // Skip Non-cash payments when creating passthrus.
    // --------------------------------------------------------
    foreach(var item in ReadCsePersonCsePersonAccountDisbursementTransaction())
    {
      // -----------
      // No Passthru to be created if the State of Kansas is the Obligee.
      // -----------
      if (Equal(entities.ObligeeCsePerson.Number, "000000017O"))
      {
        continue;
      }

      // -----------
      // Process only if the collection was applied to 'Current'.
      // -----------
      if (!ReadCollectionDisbursementTransactionDisbursementTransactionRln())
      {
        continue;
      }

      // -----------------------------
      // N.Engoor - 07/13/99
      // Any collection type which is a Direct Payment from AP, CRU or the Court
      // are also to be considered for creating Passthrus. Any other non-cash
      // collection is to be skipped.
      // If the amount is to be recovered how will it be done. (Still a query)?
      // No decision yet on how to process direct payments. Code has been 
      // commented out. At this time it is a future enhancement.
      // -----------------------------
      if (!ReadDebtDetail())
      {
        // --- Cannot happen
        continue;
      }

      if ((!Lt(entities.DebtDetail.DueDt, local.TempProcessDate.Date) || Lt
        (entities.DebtDetail.DueDt, local.HardcodeAfterAug3199.CollectionDate)) &&
        Lt(local.Initialized.ProcessDate, entities.DebtDetail.DueDt))
      {
        // ----------------------------------------------------------------------
        // Collection was applied to future month (next month). Skip this.
        // ----------------------------------------------------------------------
        continue;
      }

      ++local.NoOfDisbTransBefCommt.Count;
      local.Dummy.Flag = "Y";

      if (AsChar(local.Dummy.Flag) == 'Y')
      {
        // --- This IF statement is only an envelope to trap and process the 
        // errors.
        local.InputNoOfDisbTrans.Value =
          local.InputNoOfDisbTrans.Value.GetValueOrDefault() + 1;

        // ----------------------------------------------------------
        // ADC Disbursement month is set to debt detail due month instead of the
        // collection month because it could be a 'future' collection.
        // ----------------------------------------------------------
        if (Equal(entities.DebtDetail.DueDt, local.Initialized.ProcessDate))
        {
          // -----------------------------
          // This case would only occur for disbursement transactions that are 
          // adc voluntary payments. In that case the adc disb month date is
          // being set to the disb debit collection date.
          // -----------------------------
          if (Lt(entities.DisbursementTransaction.CollectionDate,
            local.HardcodeAfterAug3199.CollectionDate) || !
            Lt(entities.DisbursementTransaction.CollectionDate,
            local.TempProcessDate.Date))
          {
            // -----------------------------
            // If the collection date is less than Sep 1st, 1999 the record is 
            // to be skipped.
            // -----------------------------
            continue;
          }

          local.AdcDisbMonth.Date =
            entities.DisbursementTransaction.CollectionDate;
        }
        else
        {
          local.AdcDisbMonth.Date = entities.DebtDetail.DueDt;
        }

        // ----------------------------------------------------------
        // Right now the adc disbursement month is being set to the disb_debit 
        // collection_date.
        // ----------------------------------------------------------
        UseCabGetYearMonthFromDate1();

        if (AsChar(local.DisbTranRead.Flag) == 'N')
        {
          local.DisbTranRead.Flag = "Y";
          local.Previous.Number = entities.ObligeeCsePerson.Number;
          local.LastAdcDisbMonth.YearMonth = local.AdcDisbMonth.YearMonth;
          local.LastAdcDisbMonth.Date = local.AdcDisbMonth.Date;
        }

        // ------------------------
        // A new obligee is read. Finish processing for the previous Obligee.
        // ------------------------
        if (!Equal(entities.ObligeeCsePerson.Number, local.Previous.Number))
        {
          UseFnB652CreatePassthruCrNDr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          local.NetAdcCurCollecton.Amount = 0;

          if (!IsEmpty(local.ReleasedOrSuppressed.Flag))
          {
            local.PrevReleasedOrSuppress.Flag = local.ReleasedOrSuppressed.Flag;
          }

          if (AsChar(local.PrevReleasedOrSuppress.Flag) == 'R')
          {
            local.PrevReleasedOrSuppress.Flag = "";

            // ------------------------
            // RBM   11/16/98  If the Collection month and Year are same as that
            //           of the Processing month and year, Create an occurence 
            // of
            //           Income_Interface_Notification table.
            // ------------------------
            if (local.LastAdcDisbMonth.YearMonth == local
              .PrevMonthCreateIntrfc.YearMonth)
            {
              local.PrevMonthCreateIntrfc.Date = local.LastAdcDisbMonth.Date;
              UseFnCabPopulateIntfIncmTable();
              local.NoOfIncIntBefCommit.Count =
                (int)(local.NoOfIncIntBefCommit.Count + local
                .NoOfIncIntfRecCreated.Value.GetValueOrDefault());

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test1;
              }
            }

            // ---------------------------------------------------------------
            // Create Payment Request (WAR/EFT/RCV/RCP) for passthrus.
            // ---------------------------------------------------------------
            UseFnB652GenPassthruFObligee2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test1;
            }

            local.NoOfRcapCollsToCommit.Count += local.NoOfCollCreatedFRcap.
              Count;
            local.NoOfWarrToCommt.Count += local.NoOfWarrCreatdFOblgee.Count;
            local.NoOfRecoveriesToCommit.Count += local.NoOfRcvsCreatdFOblgee.
              Count;
            local.NoOfEftsToCommit.Count += local.NoOfEftCreatedFOblgee.Count;
          }

          local.OutputNoOfSuccObligees.Value =
            local.OutputNoOfSuccObligees.Value.GetValueOrDefault() + 1;
          local.InputNoOfObligees.Value =
            local.InputNoOfObligees.Value.GetValueOrDefault() + 1;

          // --------------
          // The commit frequency is being checked only if a unit of work has 
          // been completed. In this case a unit of work is an Obligee number.
          // --------------
          if (local.NoOfDisbTransBefCommt.Count >= local
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
          {
            // --------------
            // Record the number of checkpoints and the last checkpoint time and
            // set the restart indicator to yes.
            // Also return the checkpoint frequency counts in case they have 
            // been changed since the last read.
            // --------------
            local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
            local.ProgramCheckpointRestart.RestartInd = "Y";
            local.ProgramCheckpointRestart.RestartInfo = local.Previous.Number;
            UseUpdatePgmCheckpointRestart2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              // ok, continue processing
            }
            else
            {
              return;
            }

            // ***** Call an external that does a DB2 commit using a Cobol 
            // program.
            UseExtToDoACommit();

            if (local.PassArea.NumericReturnCode != 0)
            {
              ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

              return;
            }

            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Checkpoint No:" + NumberToString
              (local.ProgramCheckpointRestart.CheckpointCount.
                GetValueOrDefault(), 9, 7) + "  Time : " + NumberToString
              (TimeToInt(
                TimeOfDay(local.ProgramCheckpointRestart.LastCheckpointTimestamp))
              , 10, 6);
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + " CSE Person No : " + TrimEnd
              (local.ProgramCheckpointRestart.RestartInfo);
            UseCabControlReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.NoOfDisbTransBefCommt.Count = 0;
          }
        }
        else if (local.AdcDisbMonth.YearMonth != local
          .LastAdcDisbMonth.YearMonth)
        {
          UseFnB652CreatePassthruCrNDr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test1;
          }

          if (!IsEmpty(local.ReleasedOrSuppressed.Flag))
          {
            local.PrevReleasedOrSuppress.Flag = local.ReleasedOrSuppressed.Flag;
          }

          if (AsChar(local.PrevReleasedOrSuppress.Flag) == 'R')
          {
            // ------------------------
            // RBM   -  11/16/98  If the Collection month and Year are same as 
            // that of the Processing month and year and the passthrus created
            // for the person for the current month is not suppressed, Create an
            // occurence of           Income_Interface_Notification table.
            // ------------------------
            if (local.LastAdcDisbMonth.YearMonth == local
              .PrevMonthCreateIntrfc.YearMonth)
            {
              local.PrevMonthCreateIntrfc.Date = local.LastAdcDisbMonth.Date;
              UseFnCabPopulateIntfIncmTable();
              local.NoOfIncIntBefCommit.Count =
                (int)(local.NoOfIncIntBefCommit.Count + local
                .NoOfIncIntfRecCreated.Value.GetValueOrDefault());

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test1;
              }
            }
          }

          local.NetAdcCurCollecton.Amount = 0;
        }
        else
        {
          // --- The obligee-month is the same
        }

        local.Previous.Number = entities.ObligeeCsePerson.Number;
        local.LastAdcDisbMonth.YearMonth = local.AdcDisbMonth.YearMonth;
        local.LastAdcDisbMonth.Date = local.AdcDisbMonth.Date;
        local.NetAdcCurCollecton.Amount += entities.DisbursementTransaction.
          Amount;
        UseFnB652MarkDisbPassthruPrcsd();
      }

Test1:

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ---------------------------------------------
        // Processed one disbursement transaction
        // ---------------------------------------------
        continue;
      }

      // ---------------------------------------------
      // Process the program error encountered
      // ---------------------------------------------
      UseEabExtractExitStateMessage();

      // ----------------
      // Reset the exit state to all_ok to allow further processing.
      // ----------------
      ExitState = "ACO_NN0000_ALL_OK";
      UseEabRollbackSql();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ZD_ERROR_IN_EAB_ROLLBACK_SQL_AB";

        return;
      }

      // --- rollback the counters also
      // ----------------------------------
      // New CAB added to handle errors.
      // ----------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.Count.Count = 1;

      do
      {
        switch(local.Count.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;

            break;
          case 2:
            local.EabReportSend.RptDetail = "CSE Person Number:";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + entities
              .ObligeeCsePerson.Number;

            break;
          case 3:
            local.EabReportSend.RptDetail = "Disbursement Transaction Id : " + NumberToString
              (entities.DisbursementTransaction.SystemGeneratedIdentifier, 9, 7);
              

            break;
          case 4:
            local.EabReportSend.RptDetail = "Obligee Month :";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (local.MonthlyObligeeSummary.Month, 14, 2);

            break;
          case 5:
            local.EabReportSend.RptDetail = "Obligee Year :";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (local.MonthlyObligeeSummary.Year, 12, 4);

            break;
          default:
            break;
        }

        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.Count.Count;
      }
      while(local.Count.Count <= 5);

      local.EabFileHandling.Action = "CLOSE";
      local.EabReportSend.RptDetail = "";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      UseSiCloseAdabas();

      return;
    }

    // ---------
    // No more CSE People read.  Complete processing for the last obligee.
    // ---------
    local.Dummy.Flag = "Y";

    if (AsChar(local.Dummy.Flag) == 'Y')
    {
      if (AsChar(local.DisbTranRead.Flag) == 'N')
      {
        // ---------
        // If the above READ EACH did not fetch any records skip processing for 
        // the last obligee.
        // ---------
        goto Test2;
      }

      UseFnB652CreatePassthruCrNDr();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test2;
      }

      if (!IsEmpty(local.ReleasedOrSuppressed.Flag))
      {
        local.PrevReleasedOrSuppress.Flag = local.ReleasedOrSuppressed.Flag;
      }

      if (AsChar(local.PrevReleasedOrSuppress.Flag) == 'R')
      {
        // ---------------------------------
        // N.Engoor  -  11/16/98  If the Collection month and Year are current, 
        // create an occurence of the Income_Interface_Notification record. For
        // e.g if Passthrus is being run on Nov 1st, then an interface income
        // notification record would be created for the month of Oct for each
        // Obligee as long as there is a passthru for that month.
        // ---------------------------------
        if (local.LastAdcDisbMonth.YearMonth == local
          .PrevMonthCreateIntrfc.YearMonth)
        {
          local.PrevMonthCreateIntrfc.Date = local.LastAdcDisbMonth.Date;
          UseFnCabPopulateIntfIncmTable();
          local.NoOfIncIntBefCommit.Count =
            (int)(local.NoOfIncIntBefCommit.Count + local
            .NoOfIncIntfRecCreated.Value.GetValueOrDefault());

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test2;
          }
        }

        // -------------------------------------
        // Generate a payment request only if the person is not in a suppressed 
        // status.
        // -------------------------------------
        UseFnB652GenPassthruFObligee1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        local.NoOfWarrToCommt.Count += local.NoOfWarrCreatdFOblgee.Count;
        local.NoOfRecoveriesToCommit.Count += local.NoOfRcvsCreatdFOblgee.Count;
        local.NoOfRcapCollsToCommit.Count = local.NoOfCollCreatedFRcap.Count + local
          .NoOfRcapCollsToCommit.Count;
        local.NoOfEftsToCommit.Count += local.NoOfEftCreatedFOblgee.Count;
      }

      local.OutputNoOfSuccObligees.Value =
        local.OutputNoOfSuccObligees.Value.GetValueOrDefault() + 1;
      local.InputNoOfObligees.Value =
        local.InputNoOfObligees.Value.GetValueOrDefault() + 1;
    }

Test2:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ---------------------------------------------
      // Process the program error encountered
      // ---------------------------------------------
      UseEabExtractExitStateMessage();

      // --- Reset the exit state to all_ok to allow further processing
      ExitState = "ACO_NN0000_ALL_OK";
      UseEabRollbackSql();

      if (local.PassArea.NumericReturnCode != 0)
      {
        ExitState = "ZD_ERROR_IN_EAB_ROLLBACK_SQL_AB";

        return;
      }

      // --- rollback the counters also
      local.NoOfSuccOblgsToCommit.Count = 0;
      local.NoOfWarrToCommt.Count = 0;
      local.NoOfRecoveriesToCommit.Count = 0;
      local.NoOfRcapsToCommit.Count = 0;
      local.NoOfEftsToCommit.Count = 0;

      // ----------------------------------
      // New CAB added to handle errors.
      // ----------------------------------
      local.EabFileHandling.Action = "WRITE";
      local.Count.Count = 1;

      do
      {
        switch(local.Count.Count)
        {
          case 1:
            local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;

            break;
          case 2:
            local.EabReportSend.RptDetail = "CSE Person Number:";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + entities
              .ObligeeCsePerson.Number;

            break;
          case 3:
            local.EabReportSend.RptDetail = "Disbursement Transaction Id : " + NumberToString
              (entities.DisbursementTransaction.SystemGeneratedIdentifier, 9, 7);
              

            break;
          case 4:
            local.EabReportSend.RptDetail = "Obligee Month :";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (local.MonthlyObligeeSummary.Month, 14, 2);

            break;
          case 5:
            local.EabReportSend.RptDetail = "Obligee Year :";
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + NumberToString
              (local.MonthlyObligeeSummary.Year, 12, 4);

            break;
          default:
            break;
        }

        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ++local.Count.Count;
      }
      while(local.Count.Count <= 5);

      local.EabFileHandling.Action = "CLOSE";
      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // ---------------------------------------------------
      // Close the ADABAS files
      // ---------------------------------------------------
      UseSiCloseAdabas();

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    local.OutputNoOfEfts.Value =
      local.OutputNoOfEfts.Value.GetValueOrDefault() + local
      .NoOfEftsToCommit.Count;
    local.OutputNoOfWarrants.Value =
      local.OutputNoOfWarrants.Value.GetValueOrDefault() + local
      .NoOfWarrToCommt.Count;
    local.OutputNoOfPotRecov.Value =
      local.OutputNoOfPotRecov.Value.GetValueOrDefault() + local
      .NoOfRecoveriesToCommit.Count;
    local.OutputNoOfRecaptures.Value =
      local.OutputNoOfRecaptures.Value.GetValueOrDefault() + local
      .NoOfRcapCollsToCommit.Count;
    local.OutputNoOfSuccObligees.Value =
      local.OutputNoOfSuccObligees.Value.GetValueOrDefault() + local
      .NoOfSuccOblgsToCommit.Count;

    // ----------------------------------------
    // Write Control Totals - 98
    // ----------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.ProgramName = local.ProgramProcessingInfo.Name;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.Count.Count = 1;

    do
    {
      switch(local.Count.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "";

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "No: of Obligees read                    : " + NumberToString
            ((long)local.InputNoOfObligees.Value.GetValueOrDefault(), 2, 14);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "No: of Successful Obligees              : " + NumberToString
            ((long)local.OutputNoOfSuccObligees.Value.GetValueOrDefault(), 2, 14);
            

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "No: of Income Interface records created : " + NumberToString
            (local.NoOfIncIntBefCommit.Count, 2, 14);

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "No: of Warrants created                 : " + NumberToString
            ((long)local.OutputNoOfWarrants.Value.GetValueOrDefault(), 2, 14);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "No: of Potential Recoveries created     : " + NumberToString
            ((long)local.OutputNoOfPotRecov.Value.GetValueOrDefault(), 2, 14);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "No: of Recaptures created               : " + NumberToString
            ((long)local.OutputNoOfRecaptures.Value.GetValueOrDefault(), 2, 14);
            

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "No: of EFT's created                    : " + NumberToString
            ((long)local.OutputNoOfEfts.Value.GetValueOrDefault(), 2, 14);

          break;
        default:
          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ++local.Count.Count;
    }
    while(local.Count.Count <= 8);

    // ***** Call an external that does a DB2 commit using a Cobol program.
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    // ----------------------------------------
    // Close the Control report File - 98.
    // ----------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ----------------------------------------
    // Close the Error report File - 99.
    // ----------------------------------------
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // ---------------
    // Set restart indicator to 'N' since the program has successfully finished.
    // ---------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    UseUpdatePgmCheckpointRestart1();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ok, continue processing
    }
    else
    {
      return;
    }

    // ---------------------------------------------------
    // Close the ADABAS files
    // ---------------------------------------------------
    UseSiCloseAdabas();
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.YearMonth = source.YearMonth;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
    target.PassthruProcDate = source.PassthruProcDate;
    target.DisbursementDate = source.DisbursementDate;
    target.CashNonCashInd = source.CashNonCashInd;
    target.RecapturedInd = source.RecapturedInd;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.TraceNumber = source.TraceNumber;
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

  private void UseCabGetYearMonthFromDate1()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.AdcDisbMonth.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.AdcDisbMonth.YearMonth = useExport.DateWorkArea.YearMonth;
  }

  private void UseCabGetYearMonthFromDate2()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.TempProcessDate.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    local.TempProcessDate.YearMonth = useExport.DateWorkArea.YearMonth;
  }

  private int UseCabGetYearMonthFromDate3()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.CreateIntfRecFor.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
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

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(EabRollbackSql.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB652CreatePassthruCrNDr()
  {
    var useImport = new FnB652CreatePassthruCrNDr.Import();
    var useExport = new FnB652CreatePassthruCrNDr.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = local.Previous.Number;
    MoveDateWorkArea(local.LastAdcDisbMonth, useImport.Passthru);
    useImport.NetAdcCurrCollection.Amount = local.NetAdcCurCollecton.Amount;
    useImport.AfterJune301999.CollectionDate =
      local.HardcodeAfterAug3199.CollectionDate;

    Call(FnB652CreatePassthruCrNDr.Execute, useImport, useExport);

    local.ReleasedOrSuppressed.Flag = useExport.ReleasedOrSuppressed.Flag;
    local.MonthlyObligeeSummary.Assign(useExport.MonthlyObligeeSummary);
  }

  private void UseFnB652GenPassthruFObligee1()
  {
    var useImport = new FnB652GenPassthruFObligee.Import();
    var useExport = new FnB652GenPassthruFObligee.Export();

    useImport.Suppressed.SystemGeneratedIdentifier =
      local.HardcodeSuppressed.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = local.Previous.Number;
    MoveElectronicFundTransmission(local.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.EftCreated.Flag = local.EftCreated.Flag;

    Call(FnB652GenPassthruFObligee.Execute, useImport, useExport);

    local.Unsuppressed.Flag = useExport.UnsuppressedFlag.Flag;
    local.NoOfCollCreatedFRcap.Count = useExport.NoOfRcapForObligee.Count;
    local.NoOfRcvsCreatdFOblgee.Count = useExport.NoOfPotRecsCreated.Count;
    local.NoOfWarrCreatdFOblgee.Count = useExport.NoOfWarrantsCreated.Count;
    MoveElectronicFundTransmission(useExport.ElectronicFundTransmission,
      local.ElectronicFundTransmission);
    local.EftCreated.Flag = useExport.EftCreated.Flag;
    local.NoOfEftCreatedFOblgee.Count = useExport.NoOfEftsCreated.Count;
  }

  private void UseFnB652GenPassthruFObligee2()
  {
    var useImport = new FnB652GenPassthruFObligee.Import();
    var useExport = new FnB652GenPassthruFObligee.Export();

    useImport.Suppressed.SystemGeneratedIdentifier =
      local.HardcodeSuppressed.SystemGeneratedIdentifier;
    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Obligee.Number = local.Previous.Number;
    MoveElectronicFundTransmission(local.ElectronicFundTransmission,
      useImport.ElectronicFundTransmission);
    useImport.EftCreated.Flag = local.EftCreated.Flag;

    Call(FnB652GenPassthruFObligee.Execute, useImport, useExport);

    local.NoOfCollCreatedFRcap.Count = useExport.NoOfRcapForObligee.Count;
    local.NoOfRcvsCreatdFOblgee.Count = useExport.NoOfPotRecsCreated.Count;
    local.NoOfWarrCreatdFOblgee.Count = useExport.NoOfWarrantsCreated.Count;
    local.NoOfEftCreatedFOblgee.Count = useExport.NoOfEftsCreated.Count;
    MoveElectronicFundTransmission(useExport.ElectronicFundTransmission,
      local.ElectronicFundTransmission);
    local.EftCreated.Flag = useExport.EftCreated.Flag;
  }

  private void UseFnB652MarkDisbPassthruPrcsd()
  {
    var useImport = new FnB652MarkDisbPassthruPrcsd.Import();
    var useExport = new FnB652MarkDisbPassthruPrcsd.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Persistent.Assign(entities.DisbursementTransaction);

    Call(FnB652MarkDisbPassthruPrcsd.Execute, useImport, useExport);

    MoveDisbursementTransaction(useImport.Persistent,
      entities.DisbursementTransaction);
  }

  private void UseFnCabPopulateIntfIncmTable()
  {
    var useImport = new FnCabPopulateIntfIncmTable.Import();
    var useExport = new FnCabPopulateIntfIncmTable.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    useImport.Payee.Number = local.Previous.Number;
    useImport.CreateIntfRec.Date = local.PrevMonthCreateIntrfc.Date;

    Call(FnCabPopulateIntfIncmTable.Execute, useImport, useExport);

    local.NoOfIncIntfRecCreated.Value =
      useExport.ExNoOfIncmIntfRecsCreated.Value;
  }

  private void UseFnHardcodedDisbursementInfo()
  {
    var useImport = new FnHardcodedDisbursementInfo.Import();
    var useExport = new FnHardcodedDisbursementInfo.Export();

    Call(FnHardcodedDisbursementInfo.Execute, useImport, useExport);

    local.HardcodeSuppressed.SystemGeneratedIdentifier =
      useExport.Suppressed.SystemGeneratedIdentifier;
    local.HardcodeAfCurrentVol.SystemGeneratedIdentifier =
      useExport.AfVol.SystemGeneratedIdentifier;
    local.HardcodeAfCurrentMc.SystemGeneratedIdentifier =
      useExport.AfCmc.SystemGeneratedIdentifier;
    local.HardcodeAfCurrentMs.SystemGeneratedIdentifier =
      useExport.AfCms.SystemGeneratedIdentifier;
    local.HardcodeAfCurrentChSupp.SystemGeneratedIdentifier =
      useExport.AfCcs.SystemGeneratedIdentifier;
    local.HardcodePassthru.SystemGeneratedIdentifier =
      useExport.Pt.SystemGeneratedIdentifier;
    local.HardcodeDisbursement.Type1 =
      useExport.DisbursementDisbursementTransaction.Type1;
  }

  private void UseOeGetMonthStartAndEndDate()
  {
    var useImport = new OeGetMonthStartAndEndDate.Import();
    var useExport = new OeGetMonthStartAndEndDate.Export();

    useImport.DateWorkArea.YearMonth = local.TempProcessDate.YearMonth;

    Call(OeGetMonthStartAndEndDate.Execute, useImport, useExport);

    local.InvalidMonth.Flag = useExport.InvalidMonth.Flag;
    local.TempProcessDate.Date = useExport.MonthStartDate.Date;
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

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private bool ReadCollectionDisbursementTransactionDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementTransactionRln.Populated = false;
    entities.Credit.Populated = false;
    entities.Collection.Populated = false;

    return Read(
      "ReadCollectionDisbursementTransactionDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Credit.ColId = db.GetNullableInt32(reader, 0);
        entities.Collection.AppliedToCode = db.GetString(reader, 1);
        entities.Collection.CollectionDt = db.GetDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.CrtType = db.GetInt32(reader, 4);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 4);
        entities.Collection.CstId = db.GetInt32(reader, 5);
        entities.Credit.CstId = db.GetNullableInt32(reader, 5);
        entities.Collection.CrvId = db.GetInt32(reader, 6);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 6);
        entities.Collection.CrdId = db.GetInt32(reader, 7);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 7);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 8);
        entities.Collection.CspNumber = db.GetString(reader, 9);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 9);
        entities.Collection.CpaType = db.GetString(reader, 10);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 10);
        entities.Collection.OtrId = db.GetInt32(reader, 11);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 11);
        entities.Collection.OtrType = db.GetString(reader, 12);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 12);
        entities.Collection.OtyId = db.GetInt32(reader, 13);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 13);
        entities.Credit.CpaType = db.GetString(reader, 14);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 14);
        entities.Credit.CspNumber = db.GetString(reader, 15);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 15);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 16);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 16);
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 17);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 18);
        entities.DisbursementTransactionRln.CspNumber =
          db.GetString(reader, 19);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 20);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 21);
        entities.DisbursementTransactionRln.Populated = true;
        entities.Credit.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AppliedToCode",
          entities.Collection.AppliedToCode);
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
      });
  }

  private IEnumerable<bool>
    ReadCsePersonCsePersonAccountDisbursementTransaction()
  {
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadCsePersonCsePersonAccountDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "numb", local.RestartObligee.Number);
        db.SetNullableDate(
          command, "passthruProcDate",
          local.Initialized.ProcessDate.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeAfCurrentChSupp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeAfCurrentMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeAfCurrentMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeAfCurrentVol.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligeeCsePerson.Number = db.GetString(reader, 0);
        entities.ObligeeCsePerson.Type1 = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 2);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 2);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 3);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 5);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 6);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.LastUpdateTmst =
          db.GetNullableDateTime(reader, 9);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 10);
        entities.DisbursementTransaction.CashNonCashInd =
          db.GetString(reader, 11);
        entities.DisbursementTransaction.RecapturedInd =
          db.GetString(reader, 12);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 13);
        entities.DisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 14);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementTransaction.PassthruProcDate =
          db.GetNullableDate(reader, 16);
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ObligeeCsePerson.Type1);
        CheckValid<CsePersonAccount>("Type1",
          entities.ObligeeCsePersonAccount.Type1);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.DisbursementTransaction.CashNonCashInd);

        return true;
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otrGeneratedId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
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
    /// <summary>
    /// A value of PrevMonthCreateIntrfc.
    /// </summary>
    [JsonPropertyName("prevMonthCreateIntrfc")]
    public DateWorkArea PrevMonthCreateIntrfc
    {
      get => prevMonthCreateIntrfc ??= new();
      set => prevMonthCreateIntrfc = value;
    }

    /// <summary>
    /// A value of PrevReleasedOrSuppress.
    /// </summary>
    [JsonPropertyName("prevReleasedOrSuppress")]
    public Common PrevReleasedOrSuppress
    {
      get => prevReleasedOrSuppress ??= new();
      set => prevReleasedOrSuppress = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of EftCreated.
    /// </summary>
    [JsonPropertyName("eftCreated")]
    public Common EftCreated
    {
      get => eftCreated ??= new();
      set => eftCreated = value;
    }

    /// <summary>
    /// A value of NoOfEftsToCommit.
    /// </summary>
    [JsonPropertyName("noOfEftsToCommit")]
    public Common NoOfEftsToCommit
    {
      get => noOfEftsToCommit ??= new();
      set => noOfEftsToCommit = value;
    }

    /// <summary>
    /// A value of NoOfEftCreatedFOblgee.
    /// </summary>
    [JsonPropertyName("noOfEftCreatedFOblgee")]
    public Common NoOfEftCreatedFOblgee
    {
      get => noOfEftCreatedFOblgee ??= new();
      set => noOfEftCreatedFOblgee = value;
    }

    /// <summary>
    /// A value of OutputNoOfEfts.
    /// </summary>
    [JsonPropertyName("outputNoOfEfts")]
    public ProgramControlTotal OutputNoOfEfts
    {
      get => outputNoOfEfts ??= new();
      set => outputNoOfEfts = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of InvalidMonth.
    /// </summary>
    [JsonPropertyName("invalidMonth")]
    public Common InvalidMonth
    {
      get => invalidMonth ??= new();
      set => invalidMonth = value;
    }

    /// <summary>
    /// A value of TempProcessDate.
    /// </summary>
    [JsonPropertyName("tempProcessDate")]
    public DateWorkArea TempProcessDate
    {
      get => tempProcessDate ??= new();
      set => tempProcessDate = value;
    }

    /// <summary>
    /// A value of HardcodeAfterAug3199.
    /// </summary>
    [JsonPropertyName("hardcodeAfterAug3199")]
    public DisbursementTransaction HardcodeAfterAug3199
    {
      get => hardcodeAfterAug3199 ??= new();
      set => hardcodeAfterAug3199 = value;
    }

    /// <summary>
    /// A value of ReleasedOrSuppressed.
    /// </summary>
    [JsonPropertyName("releasedOrSuppressed")]
    public Common ReleasedOrSuppressed
    {
      get => releasedOrSuppressed ??= new();
      set => releasedOrSuppressed = value;
    }

    /// <summary>
    /// A value of HardcodeSuppressed.
    /// </summary>
    [JsonPropertyName("hardcodeSuppressed")]
    public DisbursementStatus HardcodeSuppressed
    {
      get => hardcodeSuppressed ??= new();
      set => hardcodeSuppressed = value;
    }

    /// <summary>
    /// A value of NoOfIncIntBefCommit.
    /// </summary>
    [JsonPropertyName("noOfIncIntBefCommit")]
    public Common NoOfIncIntBefCommit
    {
      get => noOfIncIntBefCommit ??= new();
      set => noOfIncIntBefCommit = value;
    }

    /// <summary>
    /// A value of Unsuppressed.
    /// </summary>
    [JsonPropertyName("unsuppressed")]
    public Common Unsuppressed
    {
      get => unsuppressed ??= new();
      set => unsuppressed = value;
    }

    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of CashNonCashInd.
    /// </summary>
    [JsonPropertyName("cashNonCashInd")]
    public DisbursementTransaction CashNonCashInd
    {
      get => cashNonCashInd ??= new();
      set => cashNonCashInd = value;
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
    /// A value of HardcodeAfCurrentVol.
    /// </summary>
    [JsonPropertyName("hardcodeAfCurrentVol")]
    public DisbursementType HardcodeAfCurrentVol
    {
      get => hardcodeAfCurrentVol ??= new();
      set => hardcodeAfCurrentVol = value;
    }

    /// <summary>
    /// A value of HardcodeAfCurrentMc.
    /// </summary>
    [JsonPropertyName("hardcodeAfCurrentMc")]
    public DisbursementType HardcodeAfCurrentMc
    {
      get => hardcodeAfCurrentMc ??= new();
      set => hardcodeAfCurrentMc = value;
    }

    /// <summary>
    /// A value of HardcodeAfCurrentMs.
    /// </summary>
    [JsonPropertyName("hardcodeAfCurrentMs")]
    public DisbursementType HardcodeAfCurrentMs
    {
      get => hardcodeAfCurrentMs ??= new();
      set => hardcodeAfCurrentMs = value;
    }

    /// <summary>
    /// A value of HardcodeAfCurrentChSupp.
    /// </summary>
    [JsonPropertyName("hardcodeAfCurrentChSupp")]
    public DisbursementType HardcodeAfCurrentChSupp
    {
      get => hardcodeAfCurrentChSupp ??= new();
      set => hardcodeAfCurrentChSupp = value;
    }

    /// <summary>
    /// A value of CreateIntfRecFor.
    /// </summary>
    [JsonPropertyName("createIntfRecFor")]
    public DateWorkArea CreateIntfRecFor
    {
      get => createIntfRecFor ??= new();
      set => createIntfRecFor = value;
    }

    /// <summary>
    /// A value of NoOfIncIntfRecCreated.
    /// </summary>
    [JsonPropertyName("noOfIncIntfRecCreated")]
    public ProgramControlTotal NoOfIncIntfRecCreated
    {
      get => noOfIncIntfRecCreated ??= new();
      set => noOfIncIntfRecCreated = value;
    }

    /// <summary>
    /// A value of NoOfWarrCreatdFOblgee.
    /// </summary>
    [JsonPropertyName("noOfWarrCreatdFOblgee")]
    public Common NoOfWarrCreatdFOblgee
    {
      get => noOfWarrCreatdFOblgee ??= new();
      set => noOfWarrCreatdFOblgee = value;
    }

    /// <summary>
    /// A value of NoOfRcvsCreatdFOblgee.
    /// </summary>
    [JsonPropertyName("noOfRcvsCreatdFOblgee")]
    public Common NoOfRcvsCreatdFOblgee
    {
      get => noOfRcvsCreatdFOblgee ??= new();
      set => noOfRcvsCreatdFOblgee = value;
    }

    /// <summary>
    /// A value of NetAdcCurCollecton.
    /// </summary>
    [JsonPropertyName("netAdcCurCollecton")]
    public DisbursementTransaction NetAdcCurCollecton
    {
      get => netAdcCurCollecton ??= new();
      set => netAdcCurCollecton = value;
    }

    /// <summary>
    /// A value of LastAdcDisbMonth.
    /// </summary>
    [JsonPropertyName("lastAdcDisbMonth")]
    public DateWorkArea LastAdcDisbMonth
    {
      get => lastAdcDisbMonth ??= new();
      set => lastAdcDisbMonth = value;
    }

    /// <summary>
    /// A value of AdcDisbMonth.
    /// </summary>
    [JsonPropertyName("adcDisbMonth")]
    public DateWorkArea AdcDisbMonth
    {
      get => adcDisbMonth ??= new();
      set => adcDisbMonth = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of DebugObligee.
    /// </summary>
    [JsonPropertyName("debugObligee")]
    public CsePerson DebugObligee
    {
      get => debugObligee ??= new();
      set => debugObligee = value;
    }

    /// <summary>
    /// A value of LeftPadZero.
    /// </summary>
    [JsonPropertyName("leftPadZero")]
    public TextWorkArea LeftPadZero
    {
      get => leftPadZero ??= new();
      set => leftPadZero = value;
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
    /// A value of RestartObligee.
    /// </summary>
    [JsonPropertyName("restartObligee")]
    public CsePerson RestartObligee
    {
      get => restartObligee ??= new();
      set => restartObligee = value;
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

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Req.
    /// </summary>
    [JsonPropertyName("req")]
    public ElectronicFundTransmission Req
    {
      get => req ??= new();
      set => req = value;
    }

    /// <summary>
    /// A value of Odfi.
    /// </summary>
    [JsonPropertyName("odfi")]
    public ElectronicFundTransmission Odfi
    {
      get => odfi ??= new();
      set => odfi = value;
    }

    /// <summary>
    /// A value of Outbound.
    /// </summary>
    [JsonPropertyName("outbound")]
    public ElectronicFundTransmission Outbound
    {
      get => outbound ??= new();
      set => outbound = value;
    }

    /// <summary>
    /// A value of HardcodeCrFee.
    /// </summary>
    [JsonPropertyName("hardcodeCrFee")]
    public DisbursementType HardcodeCrFee
    {
      get => hardcodeCrFee ??= new();
      set => hardcodeCrFee = value;
    }

    /// <summary>
    /// A value of HardcodeSupport.
    /// </summary>
    [JsonPropertyName("hardcodeSupport")]
    public PaymentRequest HardcodeSupport
    {
      get => hardcodeSupport ??= new();
      set => hardcodeSupport = value;
    }

    /// <summary>
    /// A value of HardcodeDesignatedPayee.
    /// </summary>
    [JsonPropertyName("hardcodeDesignatedPayee")]
    public CsePersonRlnRsn HardcodeDesignatedPayee
    {
      get => hardcodeDesignatedPayee ??= new();
      set => hardcodeDesignatedPayee = value;
    }

    /// <summary>
    /// A value of HardcodeRequested.
    /// </summary>
    [JsonPropertyName("hardcodeRequested")]
    public PaymentStatus HardcodeRequested
    {
      get => hardcodeRequested ??= new();
      set => hardcodeRequested = value;
    }

    /// <summary>
    /// A value of HardcodeProcessed.
    /// </summary>
    [JsonPropertyName("hardcodeProcessed")]
    public DisbursementStatus HardcodeProcessed
    {
      get => hardcodeProcessed ??= new();
      set => hardcodeProcessed = value;
    }

    /// <summary>
    /// A value of HardcodeWarrant.
    /// </summary>
    [JsonPropertyName("hardcodeWarrant")]
    public PaymentMethodType HardcodeWarrant
    {
      get => hardcodeWarrant ??= new();
      set => hardcodeWarrant = value;
    }

    /// <summary>
    /// A value of Eft.
    /// </summary>
    [JsonPropertyName("eft")]
    public PaymentMethodType Eft
    {
      get => eft ??= new();
      set => eft = value;
    }

    /// <summary>
    /// A value of HardcodePassthru.
    /// </summary>
    [JsonPropertyName("hardcodePassthru")]
    public DisbursementType HardcodePassthru
    {
      get => hardcodePassthru ??= new();
      set => hardcodePassthru = value;
    }

    /// <summary>
    /// A value of HardcodeReleased.
    /// </summary>
    [JsonPropertyName("hardcodeReleased")]
    public DisbursementStatus HardcodeReleased
    {
      get => hardcodeReleased ??= new();
      set => hardcodeReleased = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursement.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursement")]
    public DisbursementTransaction HardcodeDisbursement
    {
      get => hardcodeDisbursement ??= new();
      set => hardcodeDisbursement = value;
    }

    /// <summary>
    /// A value of InputNoOfObligees.
    /// </summary>
    [JsonPropertyName("inputNoOfObligees")]
    public ProgramControlTotal InputNoOfObligees
    {
      get => inputNoOfObligees ??= new();
      set => inputNoOfObligees = value;
    }

    /// <summary>
    /// A value of InputNoOfDisbTrans.
    /// </summary>
    [JsonPropertyName("inputNoOfDisbTrans")]
    public ProgramControlTotal InputNoOfDisbTrans
    {
      get => inputNoOfDisbTrans ??= new();
      set => inputNoOfDisbTrans = value;
    }

    /// <summary>
    /// A value of OutputNoOfWarrants.
    /// </summary>
    [JsonPropertyName("outputNoOfWarrants")]
    public ProgramControlTotal OutputNoOfWarrants
    {
      get => outputNoOfWarrants ??= new();
      set => outputNoOfWarrants = value;
    }

    /// <summary>
    /// A value of OutputNoOfPotRecov.
    /// </summary>
    [JsonPropertyName("outputNoOfPotRecov")]
    public ProgramControlTotal OutputNoOfPotRecov
    {
      get => outputNoOfPotRecov ??= new();
      set => outputNoOfPotRecov = value;
    }

    /// <summary>
    /// A value of OutputNoOfRecaptures.
    /// </summary>
    [JsonPropertyName("outputNoOfRecaptures")]
    public ProgramControlTotal OutputNoOfRecaptures
    {
      get => outputNoOfRecaptures ??= new();
      set => outputNoOfRecaptures = value;
    }

    /// <summary>
    /// A value of OutputNoOfProgErrors.
    /// </summary>
    [JsonPropertyName("outputNoOfProgErrors")]
    public ProgramControlTotal OutputNoOfProgErrors
    {
      get => outputNoOfProgErrors ??= new();
      set => outputNoOfProgErrors = value;
    }

    /// <summary>
    /// A value of OutputNoOfSuccObligees.
    /// </summary>
    [JsonPropertyName("outputNoOfSuccObligees")]
    public ProgramControlTotal OutputNoOfSuccObligees
    {
      get => outputNoOfSuccObligees ??= new();
      set => outputNoOfSuccObligees = value;
    }

    /// <summary>
    /// A value of OutputNoOfRcapColl.
    /// </summary>
    [JsonPropertyName("outputNoOfRcapColl")]
    public ProgramControlTotal OutputNoOfRcapColl
    {
      get => outputNoOfRcapColl ??= new();
      set => outputNoOfRcapColl = value;
    }

    /// <summary>
    /// A value of DisbTranRead.
    /// </summary>
    [JsonPropertyName("disbTranRead")]
    public Common DisbTranRead
    {
      get => disbTranRead ??= new();
      set => disbTranRead = value;
    }

    /// <summary>
    /// A value of ProgramControlCreated.
    /// </summary>
    [JsonPropertyName("programControlCreated")]
    public Common ProgramControlCreated
    {
      get => programControlCreated ??= new();
      set => programControlCreated = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DisbursementTransaction Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NoOfDisbTransBefCommt.
    /// </summary>
    [JsonPropertyName("noOfDisbTransBefCommt")]
    public Common NoOfDisbTransBefCommt
    {
      get => noOfDisbTransBefCommt ??= new();
      set => noOfDisbTransBefCommt = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of DisbReleasedStatusFound.
    /// </summary>
    [JsonPropertyName("disbReleasedStatusFound")]
    public Common DisbReleasedStatusFound
    {
      get => disbReleasedStatusFound ??= new();
      set => disbReleasedStatusFound = value;
    }

    /// <summary>
    /// A value of Recaptured.
    /// </summary>
    [JsonPropertyName("recaptured")]
    public PaymentRequest Recaptured
    {
      get => recaptured ??= new();
      set => recaptured = value;
    }

    /// <summary>
    /// A value of ObligeeNet.
    /// </summary>
    [JsonPropertyName("obligeeNet")]
    public PaymentRequest ObligeeNet
    {
      get => obligeeNet ??= new();
      set => obligeeNet = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of NoOfWarrToCommt.
    /// </summary>
    [JsonPropertyName("noOfWarrToCommt")]
    public Common NoOfWarrToCommt
    {
      get => noOfWarrToCommt ??= new();
      set => noOfWarrToCommt = value;
    }

    /// <summary>
    /// A value of NoOfRecoveriesToCommit.
    /// </summary>
    [JsonPropertyName("noOfRecoveriesToCommit")]
    public Common NoOfRecoveriesToCommit
    {
      get => noOfRecoveriesToCommit ??= new();
      set => noOfRecoveriesToCommit = value;
    }

    /// <summary>
    /// A value of NoOfCollCreatedFRcap.
    /// </summary>
    [JsonPropertyName("noOfCollCreatedFRcap")]
    public Common NoOfCollCreatedFRcap
    {
      get => noOfCollCreatedFRcap ??= new();
      set => noOfCollCreatedFRcap = value;
    }

    /// <summary>
    /// A value of NoOfRcapsToCommit.
    /// </summary>
    [JsonPropertyName("noOfRcapsToCommit")]
    public Common NoOfRcapsToCommit
    {
      get => noOfRcapsToCommit ??= new();
      set => noOfRcapsToCommit = value;
    }

    /// <summary>
    /// A value of NoOfRcapCollsToCommit.
    /// </summary>
    [JsonPropertyName("noOfRcapCollsToCommit")]
    public Common NoOfRcapCollsToCommit
    {
      get => noOfRcapCollsToCommit ??= new();
      set => noOfRcapCollsToCommit = value;
    }

    /// <summary>
    /// A value of NoOfSuccOblgsToCommit.
    /// </summary>
    [JsonPropertyName("noOfSuccOblgsToCommit")]
    public Common NoOfSuccOblgsToCommit
    {
      get => noOfSuccOblgsToCommit ??= new();
      set => noOfSuccOblgsToCommit = value;
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
    /// A value of NumberOfUpdates.
    /// </summary>
    [JsonPropertyName("numberOfUpdates")]
    public Common NumberOfUpdates
    {
      get => numberOfUpdates ??= new();
      set => numberOfUpdates = value;
    }

    /// <summary>
    /// A value of Initiailized.
    /// </summary>
    [JsonPropertyName("initiailized")]
    public PaymentRequest Initiailized
    {
      get => initiailized ??= new();
      set => initiailized = value;
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
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of AbortJob.
    /// </summary>
    [JsonPropertyName("abortJob")]
    public Common AbortJob
    {
      get => abortJob ??= new();
      set => abortJob = value;
    }

    private DateWorkArea prevMonthCreateIntrfc;
    private Common prevReleasedOrSuppress;
    private ElectronicFundTransmission electronicFundTransmission;
    private Common eftCreated;
    private Common noOfEftsToCommit;
    private Common noOfEftCreatedFOblgee;
    private ProgramControlTotal outputNoOfEfts;
    private Common count;
    private Common invalidMonth;
    private DateWorkArea tempProcessDate;
    private DisbursementTransaction hardcodeAfterAug3199;
    private Common releasedOrSuppressed;
    private DisbursementStatus hardcodeSuppressed;
    private Common noOfIncIntBefCommit;
    private Common unsuppressed;
    private MonthlyObligeeSummary monthlyObligeeSummary;
    private DisbursementTransaction cashNonCashInd;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private DisbursementType hardcodeAfCurrentVol;
    private DisbursementType hardcodeAfCurrentMc;
    private DisbursementType hardcodeAfCurrentMs;
    private DisbursementType hardcodeAfCurrentChSupp;
    private DateWorkArea createIntfRecFor;
    private ProgramControlTotal noOfIncIntfRecCreated;
    private Common noOfWarrCreatdFOblgee;
    private Common noOfRcvsCreatdFOblgee;
    private DisbursementTransaction netAdcCurCollecton;
    private DateWorkArea lastAdcDisbMonth;
    private DateWorkArea adcDisbMonth;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson debugObligee;
    private TextWorkArea leftPadZero;
    private ProgramCheckpointRestart programCheckpointRestart;
    private CsePerson restartObligee;
    private ProgramRun programRun;
    private DateWorkArea maximum;
    private ElectronicFundTransmission req;
    private ElectronicFundTransmission odfi;
    private ElectronicFundTransmission outbound;
    private DisbursementType hardcodeCrFee;
    private PaymentRequest hardcodeSupport;
    private CsePersonRlnRsn hardcodeDesignatedPayee;
    private PaymentStatus hardcodeRequested;
    private DisbursementStatus hardcodeProcessed;
    private PaymentMethodType hardcodeWarrant;
    private PaymentMethodType eft;
    private DisbursementType hardcodePassthru;
    private DisbursementStatus hardcodeReleased;
    private DisbursementTransaction hardcodeDisbursement;
    private ProgramControlTotal inputNoOfObligees;
    private ProgramControlTotal inputNoOfDisbTrans;
    private ProgramControlTotal outputNoOfWarrants;
    private ProgramControlTotal outputNoOfPotRecov;
    private ProgramControlTotal outputNoOfRecaptures;
    private ProgramControlTotal outputNoOfProgErrors;
    private ProgramControlTotal outputNoOfSuccObligees;
    private ProgramControlTotal outputNoOfRcapColl;
    private Common disbTranRead;
    private Common programControlCreated;
    private DisbursementTransaction initialized;
    private Common noOfDisbTransBefCommt;
    private Common dummy;
    private Common disbReleasedStatusFound;
    private PaymentRequest recaptured;
    private PaymentRequest obligeeNet;
    private CsePerson previous;
    private Common noOfWarrToCommt;
    private Common noOfRecoveriesToCommit;
    private Common noOfCollCreatedFRcap;
    private Common noOfRcapsToCommit;
    private Common noOfRcapCollsToCommit;
    private Common noOfSuccOblgsToCommit;
    private External passArea;
    private Common numberOfUpdates;
    private PaymentRequest initiailized;
    private ExitStateWorkArea exitStateWorkArea;
    private ProgramError programError;
    private Common abortJob;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of ProgramRun.
    /// </summary>
    [JsonPropertyName("programRun")]
    public ProgramRun ProgramRun
    {
      get => programRun ??= new();
      set => programRun = value;
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
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CollectionType collectionType;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private Collection collection;
    private ProgramRun programRun;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}

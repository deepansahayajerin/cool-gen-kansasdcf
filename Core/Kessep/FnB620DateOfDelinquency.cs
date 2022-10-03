// Program: FN_B620_DATE_OF_DELINQUENCY, ID: 370976491, model: 746.
// Short name: SWEF620B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B620_DATE_OF_DELINQUENCY.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB620DateOfDelinquency: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B620_DATE_OF_DELINQUENCY program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB620DateOfDelinquency(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB620DateOfDelinquency.
  /// </summary>
  public FnB620DateOfDelinquency(IContext context, Import import, Export export):
    
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
    //           M A I N T E N A N C E   L O G
    // Date	    Developer		Description
    // Feb 29, 2001 - PR#113774, Maureen Brown; change date used for delinquency
    // event.
    // Set it to 1 day after the oldest debt detail due date on the court order.
    // April 4, 2001 - PR#117508, Maureen Brown; The number of delinquent court 
    // orders total was wrong.  Fixed it and also made a change to improve
    // performance.
    // END of   M A I N T E N A N C E   L O G
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Obligation.DelinquentInd = "N";
    local.EventsRaised.Count = 0;

    // : Set hardcode values.
    local.HardcodeAccrualInstructions.DebtType = "A";
    local.HardcodeAccruing.Classification = "A";
    local.HardcodeCs.SystemGeneratedIdentifier = 1;
    local.HardcodeMs.SystemGeneratedIdentifier = 3;
    local.HardcodeMc.SystemGeneratedIdentifier = 19;
    local.HardcodeSp.SystemGeneratedIdentifier = 2;
    local.HardcodeWc.SystemGeneratedIdentifier = 21;
    local.PassToCab.Timestamp = Now();
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    UseFnB620Housekeeping();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      if (local.FileInError.ReportNumber != 99)
      {
        // : Write error message to error report if the error was not on open
        //   of the error display file
        local.EabFileHandling.Action = "WRITE";
        UseEabExtractExitStateMessage();
        local.NeededToWrite.RptDetail = local.ExitState.Message;
        UseCabErrorReport2();
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Process.Date = local.ProgramProcessingInfo.ProcessDate;
    UseCabFirstAndLastDateOfMonth();
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.EabFileHandling.Action = "WRITE";

    // *** Logic is as follows:
    // 1. Read each legal action sorting on standard number, and where the legal
    // action has
    //    at least one legal action detail associated to an accruing obligation 
    // type.
    // 2. If the standard number has changed, complete processing for the 
    // previous court
    //    order.  Call a cab to update ALL accruing obligations on the court 
    // order
    //    and create an event, if necessary.
    // 3. Determine whether or not the current court order has a delinquent 
    // obligation.
    //    If one is found, set the local obligation delinquent indicator to 'Y',
    // and loop
    //   through the legal actions until  the next break on legal action 
    // standard number.
    // *** READ IS WITH CURSOR HOLD ***
    foreach(var item in ReadLegalAction())
    {
      if (!Equal(entities.LegalAction.StandardNumber, local.Save.StandardNumber))
        
      {
        // : Check to see if this is the first L.A. read.
        if (IsEmpty(local.Save.StandardNumber))
        {
          MoveLegalAction(entities.LegalAction, local.Save);

          goto Test1;
        }

        ++local.CourtOrdersRead.Count;

        if (AsChar(local.Obligation.DelinquentInd) == 'Y')
        {
          ReadDebtDetail2();

          // : Set up date for infrastructure record.
          // : Feb 19, 2001, PR#113774, Maureen Brown
          if (!Equal(local.PassToCab.Date, local.Null1.Date))
          {
            local.PassToCab.Date = AddDays(local.PassToCab.Date, 1);
          }

          local.PassToCab.TextDate =
            NumberToString(DateToInt(local.PassToCab.Date), 8, 8);
          local.DateCcyy.Text4 = Substring(local.PassToCab.TextDate, 1, 4);
          local.DateMmdd.Text4 = Substring(local.PassToCab.TextDate, 5, 4);
          local.PassToCab.TextDate = local.DateMmdd.Text4 + local
            .DateCcyy.Text4;
        }
        else
        {
          local.PassToCab.TextDate = "";
        }

        UseFnB620UpdateObligations();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // Reset comparison fields and flags.
        MoveLegalAction(entities.LegalAction, local.Save);
        local.Obligation.DelinquentInd = "N";

        // *** Commit if it's time
        if (local.ProcessCountToCommit.Count >= local
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          UseExtToDoACommit();

          if (local.PassArea.NumericReturnCode != 0)
          {
            ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
            local.Abend.Flag = "Y";

            return;
          }

          local.ProcessCountToCommit.Count = 0;
        }
      }

Test1:

      // *** PROCESS THE CURRENT LEGAL ACTION ***
      // : Bypass processing if a delinquent obligation was already found.
      //   In this case, we are looping through until the legal action standard
      //   number changes.
      // M. Brown, April 4, 2001, pr# 117508 - Uncommented this - getting oldest
      // debt detail due date differently in order to improve performance.
      if (AsChar(local.Obligation.DelinquentInd) == 'Y')
      {
        continue;
      }

      // : Preliminary read on debt detail.  Don't need to process further if no
      //   debt detail in arrears.
      if (!ReadDebtDetail1())
      {
        // : Bypass next processing, which checks for delinquency, if no debt 
        // details for any accruing obligations related to the legal action have
        // a balance and a due date less than the beginning of the process
        // month.
        continue;
      }

      // *** read each obligation and check for delinquency.
      // *** As soon as one is found to be delinquent, escape read each.
      // : Feb 19, 2001, PR#113774, Maureen Brown; change date used for 
      // delinquency event.
      local.PassToCab.Date = local.MaxDiscontinue.Date;

      foreach(var item1 in ReadObligation())
      {
        local.TmpAmount.TotalCurrency = 0;
        local.ScreenDueAmounts.CurrentAmountDue = 0;
        local.ScreenOwedAmounts.ArrearsAmountOwed = 0;

        // *** DETERMINE AMOUNT DUE
        // : Read each set of accrual instructions.
        //   One for each supported person.
        foreach(var item2 in ReadDebtAccrualInstructions())
        {
          local.TmpAmount.TotalCurrency += entities.Debt.Amount;
        }

        if (local.TmpAmount.TotalCurrency != 0)
        {
          if (!ReadObligationPaymentSchedule())
          {
            goto Test2;
          }

          // : Must convert amount to a monthly amount.
          UseFnCalculateMonthlyAmountDue();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NN0000_ALL_OK";

            goto Test2;
          }

          local.ScreenDueAmounts.CurrentAmountDue =
            local.TmpAmount.TotalCurrency;
        }

Test2:

        // *** CALCULATE ANY ARREARS FOR THE OBLIGATION ***
        foreach(var item2 in ReadDebtDetail3())
        {
          // : Feb 19, 2001, PR#113774, Maureen Brown; change date used for 
          // delinquency event.
          if (Lt(entities.DebtDetail.DueDt, local.PassToCab.Date))
          {
            local.PassToCab.Date = entities.DebtDetail.DueDt;
          }

          local.ScreenOwedAmounts.ArrearsAmountOwed += entities.DebtDetail.
            BalanceDueAmt;
        }

        if (local.ScreenOwedAmounts.ArrearsAmountOwed == 0)
        {
          // : Go to next obligation
          continue;
        }

        // : Determine delinquency.
        if (local.ScreenOwedAmounts.ArrearsAmountOwed >= local
          .ScreenDueAmounts.CurrentAmountDue)
        {
          // : Obligation is delinquent - set flag .
          local.Obligation.DelinquentInd = "Y";
          ++local.DelinquentCourtOrders.Count;

          // M. Brown, April 4, 2001, pr# 117508 - Uncommented this.
          break;
        }

        // This is the obligation read each loop
      }

      // This is the Legal Action Read Each loop
    }

    // END OF PROCESSING
    // *** Process last legal action
    ++local.CourtOrdersRead.Count;

    if (AsChar(local.Obligation.DelinquentInd) == 'Y')
    {
      ReadDebtDetail2();

      // : Feb 19, 2001, PR#113774, Maureen Brown
      if (!Equal(local.PassToCab.Date, local.Null1.Date))
      {
        local.PassToCab.Date = AddDays(local.PassToCab.Date, 1);
      }

      local.PassToCab.TextDate =
        NumberToString(DateToInt(local.PassToCab.Date), 8, 8);
      local.DateCcyy.Text4 = Substring(local.PassToCab.TextDate, 1, 4);
      local.DateMmdd.Text4 = Substring(local.PassToCab.TextDate, 5, 4);
      local.PassToCab.TextDate = local.DateMmdd.Text4 + local.DateCcyy.Text4;
    }
    else
    {
      local.PassToCab.TextDate = "";
    }

    UseFnB620UpdateObligations();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *** End process last legal action
    local.NeededToWrite.RptDetail =
      "Number of Court Orders processed:       " + NumberToString
      (local.CourtOrdersRead.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Number of delinquent Court Orders:      " + NumberToString
      (local.DelinquentCourtOrders.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Number of events written:               " + NumberToString
      (local.EventsRaised.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Obligations set to N:                   " + NumberToString
      (local.ObsUpdatedYToN.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Obligations set to Y:                   " + NumberToString
      (local.ObsUpdatedNToY.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Obligations that stayed delinquent:     " + NumberToString
      (local.ObsStillY.Count, 15);
    UseCabControlReport2();
    local.NeededToWrite.RptDetail =
      "Obligations that stayed non-delinquent: " + NumberToString
      (local.ObsStillN.Count, 15);
    UseCabControlReport2();

    // *****************************************************************
    // * Close the ERROR RPT. DDNAME=RPT99.                             *
    // *****************************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.NeededToOpen, useImport.NeededToOpen);
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabFirstAndLastDateOfMonth()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.ProcessMonthBegin.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitState.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitState.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB620Housekeeping()
  {
    var useImport = new FnB620Housekeeping.Import();
    var useExport = new FnB620Housekeeping.Export();

    Call(FnB620Housekeeping.Execute, useImport, useExport);

    local.FileInError.ReportNumber = useExport.FileInError.ReportNumber;
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseFnB620UpdateObligations()
  {
    var useImport = new FnB620UpdateObligations.Import();
    var useExport = new FnB620UpdateObligations.Export();

    useImport.LegalAction.StandardNumber = local.Save.StandardNumber;
    useImport.Obligation.DelinquentInd = local.Obligation.DelinquentInd;
    useImport.ProcessCountToCommit.Count = local.ProcessCountToCommit.Count;
    useImport.ObsUpdatedYToN.Count = local.ObsUpdatedYToN.Count;
    useImport.ObsUpdatedNToY.Count = local.ObsUpdatedNToY.Count;
    useImport.ObsStillY.Count = local.ObsStillY.Count;
    useImport.ObsStillN.Count = local.ObsStillN.Count;
    useImport.EventsRaised.Count = local.EventsRaised.Count;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.DateWorkArea.Assign(local.PassToCab);

    Call(FnB620UpdateObligations.Execute, useImport, useExport);

    local.ProcessCountToCommit.Count = useExport.ProcessCountToCommit.Count;
    local.ObsUpdatedYToN.Count = useExport.ObsUpdatedYToN.Count;
    local.ObsUpdatedNToY.Count = useExport.ObsUpdatedNToY.Count;
    local.ObsStillY.Count = useExport.ObsStillY.Count;
    local.ObsStillN.Count = useExport.ObsStillN.Count;
    local.EventsRaised.Count = useExport.EventsRaised.Count;
  }

  private void UseFnCalculateMonthlyAmountDue()
  {
    var useImport = new FnCalculateMonthlyAmountDue.Import();
    var useExport = new FnCalculateMonthlyAmountDue.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.PeriodAmountDue.TotalCurrency = local.TmpAmount.TotalCurrency;
    useImport.Period.Date = local.Process.Date;

    Call(FnCalculateMonthlyAmountDue.Execute, useImport, useExport);

    local.TmpAmount.TotalCurrency = useExport.MonthlyDue.TotalCurrency;
  }

  private IEnumerable<bool> ReadDebtAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnlyObligation.Populated);
    entities.Debt.Populated = false;
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadDebtAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.KeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.KeyOnlyObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.KeyOnlyObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.KeyOnlyObligation.CpaType);
        db.SetNullableDate(
          command, "discontinueDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 8);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 9);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 10);
        entities.Debt.Populated = true;
        entities.AccrualInstructions.Populated = true;

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodeWc.SystemGeneratedIdentifier);
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
      });
  }

  private bool ReadDebtDetail2()
  {
    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodeWc.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "standardNo", local.Save.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        local.PassToCab.Date = db.GetDate(reader, 0);
      });
  }

  private IEnumerable<bool> ReadDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnlyObligation.Populated);
    entities.DebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail3",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.KeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.KeyOnlyObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.KeyOnlyObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.KeyOnlyObligation.CpaType);
        db.SetDate(
          command, "dueDt", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
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
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodeWc.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.KeyOnlyObligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(command, "lgaId", entities.LegalAction.Identifier);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.HardcodeCs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HardcodeMc.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.HardcodeMs.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HardcodeSp.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier5",
          local.HardcodeWc.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.KeyOnlyObligation.CpaType = db.GetString(reader, 0);
        entities.KeyOnlyObligation.CspNumber = db.GetString(reader, 1);
        entities.KeyOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.KeyOnlyObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.KeyOnlyObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.KeyOnlyObligation.Populated = true;

        return true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.KeyOnlyObligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.KeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.KeyOnlyObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "obgCspNumber", entities.KeyOnlyObligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.KeyOnlyObligation.CpaType);
        db.SetDate(command, "startDt", local.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

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
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public LegalAction Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of HardcodeAccrualInstructions.
    /// </summary>
    [JsonPropertyName("hardcodeAccrualInstructions")]
    public ObligationTransaction HardcodeAccrualInstructions
    {
      get => hardcodeAccrualInstructions ??= new();
      set => hardcodeAccrualInstructions = value;
    }

    /// <summary>
    /// A value of HardcodeAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeAccruing")]
    public ObligationType HardcodeAccruing
    {
      get => hardcodeAccruing ??= new();
      set => hardcodeAccruing = value;
    }

    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    /// <summary>
    /// A value of TmpAmount.
    /// </summary>
    [JsonPropertyName("tmpAmount")]
    public Common TmpAmount
    {
      get => tmpAmount ??= new();
      set => tmpAmount = value;
    }

    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
    }

    /// <summary>
    /// A value of ObsStillN.
    /// </summary>
    [JsonPropertyName("obsStillN")]
    public Common ObsStillN
    {
      get => obsStillN ??= new();
      set => obsStillN = value;
    }

    /// <summary>
    /// A value of EventsRaised.
    /// </summary>
    [JsonPropertyName("eventsRaised")]
    public Common EventsRaised
    {
      get => eventsRaised ??= new();
      set => eventsRaised = value;
    }

    /// <summary>
    /// A value of ObsStillY.
    /// </summary>
    [JsonPropertyName("obsStillY")]
    public Common ObsStillY
    {
      get => obsStillY ??= new();
      set => obsStillY = value;
    }

    /// <summary>
    /// A value of ObsUpdatedNToY.
    /// </summary>
    [JsonPropertyName("obsUpdatedNToY")]
    public Common ObsUpdatedNToY
    {
      get => obsUpdatedNToY ??= new();
      set => obsUpdatedNToY = value;
    }

    /// <summary>
    /// A value of ObsUpdatedYToN.
    /// </summary>
    [JsonPropertyName("obsUpdatedYToN")]
    public Common ObsUpdatedYToN
    {
      get => obsUpdatedYToN ??= new();
      set => obsUpdatedYToN = value;
    }

    /// <summary>
    /// A value of DelinquentCourtOrders.
    /// </summary>
    [JsonPropertyName("delinquentCourtOrders")]
    public Common DelinquentCourtOrders
    {
      get => delinquentCourtOrders ??= new();
      set => delinquentCourtOrders = value;
    }

    /// <summary>
    /// A value of CourtOrdersRead.
    /// </summary>
    [JsonPropertyName("courtOrdersRead")]
    public Common CourtOrdersRead
    {
      get => courtOrdersRead ??= new();
      set => courtOrdersRead = value;
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
    /// A value of ExitState.
    /// </summary>
    [JsonPropertyName("exitState")]
    public ExitStateWorkArea ExitState
    {
      get => exitState ??= new();
      set => exitState = value;
    }

    /// <summary>
    /// A value of FileInError.
    /// </summary>
    [JsonPropertyName("fileInError")]
    public EabReportSend FileInError
    {
      get => fileInError ??= new();
      set => fileInError = value;
    }

    /// <summary>
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of Abend.
    /// </summary>
    [JsonPropertyName("abend")]
    public Common Abend
    {
      get => abend ??= new();
      set => abend = value;
    }

    /// <summary>
    /// A value of ProcessCountToCommit.
    /// </summary>
    [JsonPropertyName("processCountToCommit")]
    public Common ProcessCountToCommit
    {
      get => processCountToCommit ??= new();
      set => processCountToCommit = value;
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
    /// A value of ProcessMonthBegin.
    /// </summary>
    [JsonPropertyName("processMonthBegin")]
    public DateWorkArea ProcessMonthBegin
    {
      get => processMonthBegin ??= new();
      set => processMonthBegin = value;
    }

    /// <summary>
    /// A value of ProcessMonthEnd.
    /// </summary>
    [JsonPropertyName("processMonthEnd")]
    public DateWorkArea ProcessMonthEnd
    {
      get => processMonthEnd ??= new();
      set => processMonthEnd = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

    /// <summary>
    /// A value of PassToCab.
    /// </summary>
    [JsonPropertyName("passToCab")]
    public DateWorkArea PassToCab
    {
      get => passToCab ??= new();
      set => passToCab = value;
    }

    /// <summary>
    /// A value of DateMmdd.
    /// </summary>
    [JsonPropertyName("dateMmdd")]
    public TextWorkArea DateMmdd
    {
      get => dateMmdd ??= new();
      set => dateMmdd = value;
    }

    /// <summary>
    /// A value of DateCcyy.
    /// </summary>
    [JsonPropertyName("dateCcyy")]
    public TextWorkArea DateCcyy
    {
      get => dateCcyy ??= new();
      set => dateCcyy = value;
    }

    /// <summary>
    /// A value of HardcodeCs.
    /// </summary>
    [JsonPropertyName("hardcodeCs")]
    public ObligationType HardcodeCs
    {
      get => hardcodeCs ??= new();
      set => hardcodeCs = value;
    }

    /// <summary>
    /// A value of HardcodeMs.
    /// </summary>
    [JsonPropertyName("hardcodeMs")]
    public ObligationType HardcodeMs
    {
      get => hardcodeMs ??= new();
      set => hardcodeMs = value;
    }

    /// <summary>
    /// A value of HardcodeMc.
    /// </summary>
    [JsonPropertyName("hardcodeMc")]
    public ObligationType HardcodeMc
    {
      get => hardcodeMc ??= new();
      set => hardcodeMc = value;
    }

    /// <summary>
    /// A value of HardcodeSp.
    /// </summary>
    [JsonPropertyName("hardcodeSp")]
    public ObligationType HardcodeSp
    {
      get => hardcodeSp ??= new();
      set => hardcodeSp = value;
    }

    /// <summary>
    /// A value of HardcodeWc.
    /// </summary>
    [JsonPropertyName("hardcodeWc")]
    public ObligationType HardcodeWc
    {
      get => hardcodeWc ??= new();
      set => hardcodeWc = value;
    }

    private Obligation obligation;
    private Infrastructure infrastructure;
    private LegalAction save;
    private ObligationTransaction hardcodeAccrualInstructions;
    private ObligationType hardcodeAccruing;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common tmpAmount;
    private ScreenDueAmounts screenDueAmounts;
    private Common obsStillN;
    private Common eventsRaised;
    private Common obsStillY;
    private Common obsUpdatedNToY;
    private Common obsUpdatedYToN;
    private Common delinquentCourtOrders;
    private Common courtOrdersRead;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private ExitStateWorkArea exitState;
    private EabReportSend fileInError;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common abend;
    private Common processCountToCommit;
    private External passArea;
    private DateWorkArea processMonthBegin;
    private DateWorkArea processMonthEnd;
    private DateWorkArea null1;
    private DateWorkArea process;
    private DateWorkArea maxDiscontinue;
    private DateWorkArea passToCab;
    private TextWorkArea dateMmdd;
    private TextWorkArea dateCcyy;
    private ObligationType hardcodeCs;
    private ObligationType hardcodeMs;
    private ObligationType hardcodeMc;
    private ObligationType hardcodeSp;
    private ObligationType hardcodeWc;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of KeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("keyOnlyObligation")]
    public Obligation KeyOnlyObligation
    {
      get => keyOnlyObligation ??= new();
      set => keyOnlyObligation = value;
    }

    /// <summary>
    /// A value of KeyOnlyObligationType.
    /// </summary>
    [JsonPropertyName("keyOnlyObligationType")]
    public ObligationType KeyOnlyObligationType
    {
      get => keyOnlyObligationType ??= new();
      set => keyOnlyObligationType = value;
    }

    /// <summary>
    /// A value of KeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("keyOnlyDebt")]
    public ObligationTransaction KeyOnlyDebt
    {
      get => keyOnlyDebt ??= new();
      set => keyOnlyDebt = value;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public LegalAction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of LaDetFinancial.
    /// </summary>
    [JsonPropertyName("laDetFinancial")]
    public LegalActionDetail LaDetFinancial
    {
      get => laDetFinancial ??= new();
      set => laDetFinancial = value;
    }

    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation keyOnlyObligation;
    private ObligationType keyOnlyObligationType;
    private ObligationTransaction keyOnlyDebt;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private AccrualInstructions accrualInstructions;
    private LegalAction legalAction;
    private LegalAction other;
    private LegalActionDetail laDetFinancial;
  }
#endregion
}

// Program: FN_BF11_MO_CR_FEE_CASE_NBR, ID: 371041154, model: 746.
// Short name: SWEFF11B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF11_MO_CR_FEE_CASE_NBR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBf11MoCrFeeCaseNbr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF11_MO_CR_FEE_CASE_NBR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf11MoCrFeeCaseNbr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf11MoCrFeeCaseNbr.
  /// </summary>
  public FnBf11MoCrFeeCaseNbr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // 2000-11-27  PR 108247  Fangman - New pgm for Monthly CR Fee tbl Fix run.
    // This program will delete the mo cr fee rows for each Obligee and
    // recreate them using the standard number instead of the court case number.
    // ***************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "WRITE";
    local.AbendCheckLoop.Flag = "Y";

    if (AsChar(local.AbendCheckLoop.Flag) == 'Y')
    {
      UseFnBf11Housekeeping();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // continue
      }
      else
      {
        goto Test;
      }

      local.FirstTimeThru.Flag = "Y";

      // Could check the person type to eliminate the organizations.
      foreach(var item in ReadDisbursementTransactionCsePersonLegalAction())
      {
        ++local.ProcessCountToCommit.Count;
        ++local.CountsAndAmounts.NbrOfCrFeeDisbRead.Count;
        local.CountsAndAmounts.AmtOfCrFeesRead.TotalCurrency += entities.
          CostRecoveryFee.Amount;
        local.CollectionDate.Date = entities.CostRecoveryFee.CollectionDate;
        local.CollectionYyMm.YearMonth = UseCabGetYearMonthFromDate();

        if (AsChar(local.FirstTimeThru.Flag) == 'Y')
        {
          local.FirstTimeThru.Flag = "N";
          local.HoldObligee.Number = entities.ObligeeCsePerson.Number;
          local.Hold.CourtOrderNumber = entities.LegalAction.StandardNumber ?? Spaces
            (20);
          local.Hold.YearMonth = local.CollectionYyMm.YearMonth;
          ++local.CountsAndAmounts.NbrOfArs.Count;

          // Total the monthly cr fees for the next obligee & then delete them.
          local.NumArCrFeesFromMoTbl.Number112 = 0;

          foreach(var item1 in ReadMonthlyCourtOrderFee())
          {
            if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "    From Mo tbl " + entities
                .ObligeeCsePerson.Number + "  " + entities
                .MonthlyCourtOrderFee.CourtOrderNumber + "  " + NumberToString
                (entities.MonthlyCourtOrderFee.YearMonth, 10, 6) + "  " + NumberToString
                ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 10, 6);
              UseCabErrorReport1();
            }

            local.NumArCrFeesFromMoTbl.Number112 += entities.
              MonthlyCourtOrderFee.Amount;
            ++local.CountsAndAmounts.NbrOfMoCrFeesDeleted.Count;
            local.CountsAndAmounts.AmtOfMoCrFeesDeleted.
              TotalCurrency += entities.MonthlyCourtOrderFee.Amount;

            if (AsChar(local.Test.TestRunInd.Flag) == 'N')
            {
              DeleteMonthlyCourtOrderFee();
            }
          }
        }
        else if (!Equal(entities.ObligeeCsePerson.Number,
          local.HoldObligee.Number))
        {
          // Create the monthly cost recovery fee for the previous person.
          if (AsChar(local.Test.TestRunInd.Flag) == 'N' && local.Hold.Amount > 0
            )
          {
            UseFnBf11CreateMoCrFee();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.CountsAndAmounts.NbrOfMoCrFeesCreated.Count;
            local.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency += local.
              Hold.Amount;
          }
          else
          {
            goto Test;
          }

          if (local.NumArCrFeesFromDisbTbl.Number112 != local
            .NumArCrFeesFromMoTbl.Number112)
          {
            // This could be an error.
            ++local.CountsAndAmounts.NbrOfErrors.Count;
            local.PrintMsg.Text22 = ".  ****  Warning  ****";
            UseCabFormat112AmtFieldTo1();
            UseCabFormat112AmtFieldTo2();
            local.EabReportSend.RptDetail = "AR " + local.HoldObligee.Number + "  Disb Tran Tbl Tot " +
              local.TxtArCrFeesFromDisbTbl.Text9 + "  Mo CR Fee Tbl Tot " + local
              .TxtArCrFeesFromMoTbl.Text9 + local.PrintMsg.Text22;
            UseCabErrorReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          if (local.ProcessCountToCommit.Count >= local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.ProcessCountToCommit.Count = 0;

            if (AsChar(local.Test.TestRunInd.Flag) == 'N')
            {
              UseExtToDoACommit();

              if (local.PassArea.NumericReturnCode != 0)
              {
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                return;
              }
            }
          }

          // Total the monthly cr fees for the next obligee & then delete them.
          local.NumArCrFeesFromMoTbl.Number112 = 0;

          foreach(var item1 in ReadMonthlyCourtOrderFee())
          {
            if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
            {
              local.EabReportSend.RptDetail = "    From Mo tbl " + entities
                .ObligeeCsePerson.Number + "  " + entities
                .MonthlyCourtOrderFee.CourtOrderNumber + "  " + NumberToString
                (entities.MonthlyCourtOrderFee.YearMonth, 10, 6) + "  " + NumberToString
                ((long)(entities.MonthlyCourtOrderFee.Amount * 100), 10, 6);
              UseCabErrorReport1();
            }

            local.NumArCrFeesFromMoTbl.Number112 += entities.
              MonthlyCourtOrderFee.Amount;
            ++local.CountsAndAmounts.NbrOfMoCrFeesDeleted.Count;
            local.CountsAndAmounts.AmtOfMoCrFeesDeleted.
              TotalCurrency += entities.MonthlyCourtOrderFee.Amount;

            if (AsChar(local.Test.TestRunInd.Flag) == 'N')
            {
              DeleteMonthlyCourtOrderFee();
            }
          }

          local.HoldObligee.Number = entities.ObligeeCsePerson.Number;
          local.Hold.CourtOrderNumber = entities.LegalAction.StandardNumber ?? Spaces
            (20);
          local.Hold.YearMonth = local.CollectionYyMm.YearMonth;
          local.Hold.Amount = 0;
          local.NumArCrFeesFromDisbTbl.Number112 = 0;
          ++local.CountsAndAmounts.NbrOfArs.Count;
        }
        else if (!Equal(entities.LegalAction.StandardNumber,
          local.Hold.CourtOrderNumber))
        {
          // Create the monthly cost recovery fee for the previous legal action.
          if (AsChar(local.Test.TestRunInd.Flag) == 'N' && local.Hold.Amount > 0
            )
          {
            UseFnBf11CreateMoCrFee();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.CountsAndAmounts.NbrOfMoCrFeesCreated.Count;
            local.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency += local.
              Hold.Amount;
          }
          else
          {
            goto Test;
          }

          local.Hold.CourtOrderNumber = entities.LegalAction.StandardNumber ?? Spaces
            (20);
          local.Hold.YearMonth = local.CollectionYyMm.YearMonth;
          local.Hold.Amount = 0;
        }
        else if (local.CollectionYyMm.YearMonth != local.Hold.YearMonth)
        {
          // Create the monthly cost recovery fee for the previous year month.
          if (AsChar(local.Test.TestRunInd.Flag) == 'N' && local.Hold.Amount > 0
            )
          {
            UseFnBf11CreateMoCrFee();
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.CountsAndAmounts.NbrOfMoCrFeesCreated.Count;
            local.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency += local.
              Hold.Amount;
          }
          else
          {
            goto Test;
          }

          local.Hold.YearMonth = local.CollectionYyMm.YearMonth;
          local.Hold.Amount = 0;
        }

        if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
        {
          local.EabReportSend.RptDetail = "    From Disb tbl " + entities
            .ObligeeCsePerson.Number + "  " + NumberToString
            ((long)entities.CostRecoveryFee.SystemGeneratedIdentifier * 1, 7, 9) +
            "  " + NumberToString
            ((long)(entities.CostRecoveryFee.Amount * 100), 10, 6);
          UseCabErrorReport1();
          local.EabReportSend.RptDetail = "      Coll ID " + NumberToString
            (entities.Collection.SystemGeneratedIdentifier, 7, 9) + "  Trib " +
            NumberToString(entities.Tribunal.Identifier, 7, 9) + "  LA S " + Substring
            (entities.LegalAction.StandardNumber,
            LegalAction.StandardNumber_MaxLength, 1, 14) + "  LA CN " + entities
            .LegalAction.CourtCaseNumber;
          UseCabErrorReport1();
          UseFnB651DetIfCrFeeNeeded();

          if (AsChar(local.CrFeeNeededInd.Flag) == 'N')
          {
            local.EabReportSend.RptDetail =
              "      CR Fee not taken due to being on assistance.";
            UseCabErrorReport1();
          }
        }

        local.Hold.Amount += entities.CostRecoveryFee.Amount;
        local.NumArCrFeesFromDisbTbl.Number112 += entities.CostRecoveryFee.
          Amount;
      }

      if (AsChar(local.FirstTimeThru.Flag) == 'N')
      {
        // Create the monthly cost recovery fee for the last person/legal action
        // /year month.
        if (AsChar(local.Test.TestRunInd.Flag) == 'N' && local.Hold.Amount > 0)
        {
          UseFnBf11CreateMoCrFee();
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ++local.CountsAndAmounts.NbrOfMoCrFeesCreated.Count;
          local.CountsAndAmounts.AmtOfMoCrFeesCreated.TotalCurrency += local.
            Hold.Amount;
        }
        else
        {
          goto Test;
        }

        // Check totals to ensure that the newly created mo cr fees = the old 
        // ones for the obligee just processed.
        if (local.NumArCrFeesFromDisbTbl.Number112 != local
          .NumArCrFeesFromMoTbl.Number112)
        {
          ++local.CountsAndAmounts.NbrOfErrors.Count;
          local.PrintMsg.Text22 = ".  ****  Warning  ****";
          UseCabFormat112AmtFieldTo1();
          UseCabFormat112AmtFieldTo2();
          local.EabReportSend.RptDetail = "AR " + local.HoldObligee.Number + "  Disb Tran Tbl Tot " +
            local.TxtArCrFeesFromDisbTbl.Text9 + "  Mo CR Fee Tbl Tot " + local
            .TxtArCrFeesFromMoTbl.Text9 + local.PrintMsg.Text22;
          UseCabErrorReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "AR " + entities
        .ObligeeCsePerson.Number + "  Disb ID " + NumberToString
        (entities.Credit.SystemGeneratedIdentifier, 7, 9) + "  ERROR:  " + local
        .ExitStateMessage.Message;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    UseFnBf11WriteTotals();
    local.EabFileHandling.Action = "CLOSE";
    local.EabReportSend.RptDetail = "";
    UseCabControlReport();
    UseCabErrorReport2();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Test.TestRunInd.Flag) == 'Y')
      {
        ExitState = "ACO_NN000_ROLLBACK_FOR_BATCH_TST";
      }
      else
      {
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";
        }
        else
        {
          ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
        }
      }
    }
    else
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
  }

  private static void MoveCountsAndAmounts1(FnBf11WriteTotals.Export.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfCrFeeDisbRead.Count = source.NbrOfCrFeeDisbRead.Count;
    target.NbrOfMoCrFeesDeleted.Count = source.NbrOfMoCrFeesDeleted.Count;
    target.NbrOfMoCrFeesCreated.Count = source.NbrOfMoCrFeesCreated.Count;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.AmtOfCrFeesRead.TotalCurrency =
      source.AmtOfCrFeeDisbRead.TotalCurrency;
    target.AmtOfMoCrFeesDeleted.TotalCurrency =
      source.AmtOfMoCrFeesDeleted.TotalCurrency;
    target.AmtOfMoCrFeesCreated.TotalCurrency =
      source.AmtOfMoCrFeesCreated.TotalCurrency;
  }

  private static void MoveCountsAndAmounts2(Local.CountsAndAmountsGroup source,
    FnBf11WriteTotals.Export.CountsAndAmountsGroup target)
  {
    target.NbrOfCrFeeDisbRead.Count = source.NbrOfCrFeeDisbRead.Count;
    target.NbrOfMoCrFeesDeleted.Count = source.NbrOfMoCrFeesDeleted.Count;
    target.NbrOfMoCrFeesCreated.Count = source.NbrOfMoCrFeesCreated.Count;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfErrors.Count = source.NbrOfErrors.Count;
    target.AmtOfCrFeeDisbRead.TotalCurrency =
      source.AmtOfCrFeesRead.TotalCurrency;
    target.AmtOfMoCrFeesDeleted.TotalCurrency =
      source.AmtOfMoCrFeesDeleted.TotalCurrency;
    target.AmtOfMoCrFeesCreated.TotalCurrency =
      source.AmtOfMoCrFeesCreated.TotalCurrency;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTest(FnBf11Housekeeping.Export.TestGroup source,
    Local.TestGroup target)
  {
    target.TestRunInd.Flag = source.TestRunInd.Flag;
    target.TestDisplayInd.Flag = source.TestDisplayInd.Flag;
    target.TestFirstObligee.Number = source.TestFirstObligee.Number;
    target.TestLastObligee.Number = source.TestLastObligee.Number;
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

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseCabFormat112AmtFieldTo1()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.NumArCrFeesFromDisbTbl.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.TxtArCrFeesFromDisbTbl.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private void UseCabFormat112AmtFieldTo2()
  {
    var useImport = new CabFormat112AmtFieldTo8.Import();
    var useExport = new CabFormat112AmtFieldTo8.Export();

    useImport.Import112AmountField.Number112 =
      local.NumArCrFeesFromMoTbl.Number112;

    Call(CabFormat112AmtFieldTo8.Execute, useImport, useExport);

    local.TxtArCrFeesFromMoTbl.Text9 = useExport.Formatted112AmtField.Text9;
  }

  private int UseCabGetYearMonthFromDate()
  {
    var useImport = new CabGetYearMonthFromDate.Import();
    var useExport = new CabGetYearMonthFromDate.Export();

    useImport.DateWorkArea.Date = local.CollectionDate.Date;

    Call(CabGetYearMonthFromDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.YearMonth;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateMessage.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateMessage.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB651DetIfCrFeeNeeded()
  {
    var useImport = new FnB651DetIfCrFeeNeeded.Import();
    var useExport = new FnB651DetIfCrFeeNeeded.Export();

    useImport.Ar.Number = entities.ObligeeCsePerson.Number;
    useImport.Collection.CollectionDt = entities.Collection.CollectionDt;
    useImport.TestDisplay.Flag = local.Test.TestDisplayInd.Flag;

    Call(FnB651DetIfCrFeeNeeded.Execute, useImport, useExport);

    local.CrFeeNeededInd.Flag = useExport.CrFeeNeededInd.Flag;
  }

  private void UseFnBf11CreateMoCrFee()
  {
    var useImport = new FnBf11CreateMoCrFee.Import();
    var useExport = new FnBf11CreateMoCrFee.Export();

    useImport.Obligee.Number = local.HoldObligee.Number;
    useImport.MonthlyCourtOrderFee.Assign(local.Hold);
    useImport.TestDisplayInd.Flag = local.Test.TestDisplayInd.Flag;

    Call(FnBf11CreateMoCrFee.Execute, useImport, useExport);
  }

  private void UseFnBf11Housekeeping()
  {
    var useImport = new FnBf11Housekeeping.Import();
    var useExport = new FnBf11Housekeeping.Export();

    Call(FnBf11Housekeeping.Execute, useImport, useExport);

    MoveTest(useExport.Test, local.Test);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseFnBf11WriteTotals()
  {
    var useImport = new FnBf11WriteTotals.Import();
    var useExport = new FnBf11WriteTotals.Export();

    MoveCountsAndAmounts2(local.CountsAndAmounts, useExport.CountsAndAmounts);

    Call(FnBf11WriteTotals.Execute, useImport, useExport);

    MoveCountsAndAmounts1(useExport.CountsAndAmounts, local.CountsAndAmounts);
  }

  private void DeleteMonthlyCourtOrderFee()
  {
    Update("DeleteMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.MonthlyCourtOrderFee.CpaType);
        db.SetString(
          command, "cspNumber", entities.MonthlyCourtOrderFee.CspNumber);
        db.SetString(
          command, "courtOrderNumber",
          entities.MonthlyCourtOrderFee.CourtOrderNumber);
        db.SetInt32(
          command, "yearMonth", entities.MonthlyCourtOrderFee.YearMonth);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionCsePersonLegalAction()
  {
    entities.ObligeeCsePerson.Populated = false;
    entities.ObligeeCsePersonAccount.Populated = false;
    entities.CostRecoveryFee.Populated = false;
    entities.LegalAction.Populated = false;
    entities.Tribunal.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePersonLegalAction",
      (db, command) =>
      {
        db.SetString(command, "number1", local.Test.TestFirstObligee.Number);
        db.SetString(command, "number2", local.Test.TestLastObligee.Number);
      },
      (db, reader) =>
      {
        entities.CostRecoveryFee.CpaType = db.GetString(reader, 0);
        entities.ObligeeCsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.CostRecoveryFee.CspNumber = db.GetString(reader, 1);
        entities.ObligeeCsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.CostRecoveryFee.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CostRecoveryFee.Type1 = db.GetString(reader, 3);
        entities.CostRecoveryFee.Amount = db.GetDecimal(reader, 4);
        entities.CostRecoveryFee.CollectionDate = db.GetNullableDate(reader, 5);
        entities.CostRecoveryFee.DbtGeneratedId =
          db.GetNullableInt32(reader, 6);
        entities.ObligeeCsePerson.Number = db.GetString(reader, 7);
        entities.ObligeeCsePerson.Type1 = db.GetString(reader, 8);
        entities.LegalAction.Identifier = db.GetInt32(reader, 9);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 10);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 11);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 12);
        entities.Tribunal.Identifier = db.GetInt32(reader, 12);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Collection.CollectionDt = db.GetDate(reader, 14);
        entities.Collection.CrtType = db.GetInt32(reader, 15);
        entities.Collection.CstId = db.GetInt32(reader, 16);
        entities.Collection.CrvId = db.GetInt32(reader, 17);
        entities.Collection.CrdId = db.GetInt32(reader, 18);
        entities.Collection.ObgId = db.GetInt32(reader, 19);
        entities.Collection.CspNumber = db.GetString(reader, 20);
        entities.Collection.CpaType = db.GetString(reader, 21);
        entities.Collection.OtrId = db.GetInt32(reader, 22);
        entities.Collection.OtrType = db.GetString(reader, 23);
        entities.Collection.OtyId = db.GetInt32(reader, 24);
        entities.ObligeeCsePerson.Populated = true;
        entities.ObligeeCsePersonAccount.Populated = true;
        entities.CostRecoveryFee.Populated = true;
        entities.LegalAction.Populated = true;
        entities.Tribunal.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMonthlyCourtOrderFee()
  {
    System.Diagnostics.Debug.Assert(entities.ObligeeCsePersonAccount.Populated);
    entities.MonthlyCourtOrderFee.Populated = false;

    return ReadEach("ReadMonthlyCourtOrderFee",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ObligeeCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ObligeeCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.MonthlyCourtOrderFee.CpaType = db.GetString(reader, 0);
        entities.MonthlyCourtOrderFee.CspNumber = db.GetString(reader, 1);
        entities.MonthlyCourtOrderFee.CourtOrderNumber =
          db.GetString(reader, 2);
        entities.MonthlyCourtOrderFee.YearMonth = db.GetInt32(reader, 3);
        entities.MonthlyCourtOrderFee.Amount = db.GetDecimal(reader, 4);
        entities.MonthlyCourtOrderFee.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.MonthlyCourtOrderFee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 6);
        entities.MonthlyCourtOrderFee.Populated = true;

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
    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfCrFeeDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfCrFeeDisbRead")]
      public Common NbrOfCrFeeDisbRead
      {
        get => nbrOfCrFeeDisbRead ??= new();
        set => nbrOfCrFeeDisbRead = value;
      }

      /// <summary>
      /// A value of NbrOfMoCrFeesDeleted.
      /// </summary>
      [JsonPropertyName("nbrOfMoCrFeesDeleted")]
      public Common NbrOfMoCrFeesDeleted
      {
        get => nbrOfMoCrFeesDeleted ??= new();
        set => nbrOfMoCrFeesDeleted = value;
      }

      /// <summary>
      /// A value of NbrOfMoCrFeesCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoCrFeesCreated")]
      public Common NbrOfMoCrFeesCreated
      {
        get => nbrOfMoCrFeesCreated ??= new();
        set => nbrOfMoCrFeesCreated = value;
      }

      /// <summary>
      /// A value of NbrOfArs.
      /// </summary>
      [JsonPropertyName("nbrOfArs")]
      public Common NbrOfArs
      {
        get => nbrOfArs ??= new();
        set => nbrOfArs = value;
      }

      /// <summary>
      /// A value of NbrOfErrors.
      /// </summary>
      [JsonPropertyName("nbrOfErrors")]
      public Common NbrOfErrors
      {
        get => nbrOfErrors ??= new();
        set => nbrOfErrors = value;
      }

      /// <summary>
      /// A value of AmtOfCrFeesRead.
      /// </summary>
      [JsonPropertyName("amtOfCrFeesRead")]
      public Common AmtOfCrFeesRead
      {
        get => amtOfCrFeesRead ??= new();
        set => amtOfCrFeesRead = value;
      }

      /// <summary>
      /// A value of AmtOfMoCrFeesDeleted.
      /// </summary>
      [JsonPropertyName("amtOfMoCrFeesDeleted")]
      public Common AmtOfMoCrFeesDeleted
      {
        get => amtOfMoCrFeesDeleted ??= new();
        set => amtOfMoCrFeesDeleted = value;
      }

      /// <summary>
      /// A value of AmtOfMoCrFeesCreated.
      /// </summary>
      [JsonPropertyName("amtOfMoCrFeesCreated")]
      public Common AmtOfMoCrFeesCreated
      {
        get => amtOfMoCrFeesCreated ??= new();
        set => amtOfMoCrFeesCreated = value;
      }

      private Common nbrOfCrFeeDisbRead;
      private Common nbrOfMoCrFeesDeleted;
      private Common nbrOfMoCrFeesCreated;
      private Common nbrOfArs;
      private Common nbrOfErrors;
      private Common amtOfCrFeesRead;
      private Common amtOfMoCrFeesDeleted;
      private Common amtOfMoCrFeesCreated;
    }

    /// <summary>A TestGroup group.</summary>
    [Serializable]
    public class TestGroup
    {
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
      /// A value of TestDisplayInd.
      /// </summary>
      [JsonPropertyName("testDisplayInd")]
      public Common TestDisplayInd
      {
        get => testDisplayInd ??= new();
        set => testDisplayInd = value;
      }

      /// <summary>
      /// A value of TestFirstObligee.
      /// </summary>
      [JsonPropertyName("testFirstObligee")]
      public CsePerson TestFirstObligee
      {
        get => testFirstObligee ??= new();
        set => testFirstObligee = value;
      }

      /// <summary>
      /// A value of TestLastObligee.
      /// </summary>
      [JsonPropertyName("testLastObligee")]
      public CsePerson TestLastObligee
      {
        get => testLastObligee ??= new();
        set => testLastObligee = value;
      }

      private Common testRunInd;
      private Common testDisplayInd;
      private CsePerson testFirstObligee;
      private CsePerson testLastObligee;
    }

    /// <summary>
    /// A value of CrFeeNeededInd.
    /// </summary>
    [JsonPropertyName("crFeeNeededInd")]
    public Common CrFeeNeededInd
    {
      get => crFeeNeededInd ??= new();
      set => crFeeNeededInd = value;
    }

    /// <summary>
    /// A value of NumArCrFeesFromDisbTbl.
    /// </summary>
    [JsonPropertyName("numArCrFeesFromDisbTbl")]
    public NumericWorkSet NumArCrFeesFromDisbTbl
    {
      get => numArCrFeesFromDisbTbl ??= new();
      set => numArCrFeesFromDisbTbl = value;
    }

    /// <summary>
    /// A value of TxtArCrFeesFromDisbTbl.
    /// </summary>
    [JsonPropertyName("txtArCrFeesFromDisbTbl")]
    public WorkArea TxtArCrFeesFromDisbTbl
    {
      get => txtArCrFeesFromDisbTbl ??= new();
      set => txtArCrFeesFromDisbTbl = value;
    }

    /// <summary>
    /// A value of NumArCrFeesFromMoTbl.
    /// </summary>
    [JsonPropertyName("numArCrFeesFromMoTbl")]
    public NumericWorkSet NumArCrFeesFromMoTbl
    {
      get => numArCrFeesFromMoTbl ??= new();
      set => numArCrFeesFromMoTbl = value;
    }

    /// <summary>
    /// A value of TxtArCrFeesFromMoTbl.
    /// </summary>
    [JsonPropertyName("txtArCrFeesFromMoTbl")]
    public WorkArea TxtArCrFeesFromMoTbl
    {
      get => txtArCrFeesFromMoTbl ??= new();
      set => txtArCrFeesFromMoTbl = value;
    }

    /// <summary>
    /// Gets a value of CountsAndAmounts.
    /// </summary>
    [JsonPropertyName("countsAndAmounts")]
    public CountsAndAmountsGroup CountsAndAmounts
    {
      get => countsAndAmounts ?? (countsAndAmounts = new());
      set => countsAndAmounts = value;
    }

    /// <summary>
    /// Gets a value of Test.
    /// </summary>
    [JsonPropertyName("test")]
    public TestGroup Test
    {
      get => test ?? (test = new());
      set => test = value;
    }

    /// <summary>
    /// A value of HoldObligee.
    /// </summary>
    [JsonPropertyName("holdObligee")]
    public CsePerson HoldObligee
    {
      get => holdObligee ??= new();
      set => holdObligee = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public MonthlyCourtOrderFee Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of CollectionDate.
    /// </summary>
    [JsonPropertyName("collectionDate")]
    public DateWorkArea CollectionDate
    {
      get => collectionDate ??= new();
      set => collectionDate = value;
    }

    /// <summary>
    /// A value of CollectionYyMm.
    /// </summary>
    [JsonPropertyName("collectionYyMm")]
    public DateWorkArea CollectionYyMm
    {
      get => collectionYyMm ??= new();
      set => collectionYyMm = value;
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
    /// A value of PrintMsg.
    /// </summary>
    [JsonPropertyName("printMsg")]
    public WorkArea PrintMsg
    {
      get => printMsg ??= new();
      set => printMsg = value;
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
    /// A value of FirstTimeThru.
    /// </summary>
    [JsonPropertyName("firstTimeThru")]
    public Common FirstTimeThru
    {
      get => firstTimeThru ??= new();
      set => firstTimeThru = value;
    }

    /// <summary>
    /// A value of AbendCheckLoop.
    /// </summary>
    [JsonPropertyName("abendCheckLoop")]
    public Common AbendCheckLoop
    {
      get => abendCheckLoop ??= new();
      set => abendCheckLoop = value;
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
    /// A value of ExitStateMessage.
    /// </summary>
    [JsonPropertyName("exitStateMessage")]
    public ExitStateWorkArea ExitStateMessage
    {
      get => exitStateMessage ??= new();
      set => exitStateMessage = value;
    }

    private Common crFeeNeededInd;
    private NumericWorkSet numArCrFeesFromDisbTbl;
    private WorkArea txtArCrFeesFromDisbTbl;
    private NumericWorkSet numArCrFeesFromMoTbl;
    private WorkArea txtArCrFeesFromMoTbl;
    private CountsAndAmountsGroup countsAndAmounts;
    private TestGroup test;
    private CsePerson holdObligee;
    private MonthlyCourtOrderFee hold;
    private DateWorkArea collectionDate;
    private DateWorkArea collectionYyMm;
    private Common processCountToCommit;
    private WorkArea printMsg;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common numberOfUpdates;
    private External passArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common firstTimeThru;
    private Common abendCheckLoop;
    private Common common;
    private ExitStateWorkArea exitStateMessage;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CostRecoveryFee.
    /// </summary>
    [JsonPropertyName("costRecoveryFee")]
    public DisbursementTransaction CostRecoveryFee
    {
      get => costRecoveryFee ??= new();
      set => costRecoveryFee = value;
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
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of MonthlyCourtOrderFee.
    /// </summary>
    [JsonPropertyName("monthlyCourtOrderFee")]
    public MonthlyCourtOrderFee MonthlyCourtOrderFee
    {
      get => monthlyCourtOrderFee ??= new();
      set => monthlyCourtOrderFee = value;
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

    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementTransaction costRecoveryFee;
    private DisbursementType disbursementType;
    private DisbursementTransactionRln disbursementTransactionRln;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private MonthlyCourtOrderFee monthlyCourtOrderFee;
    private DisbursementTransaction credit;
    private Collection collection;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}

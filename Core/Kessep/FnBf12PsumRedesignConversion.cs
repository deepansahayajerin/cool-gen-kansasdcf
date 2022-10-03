// Program: FN_BF12_PSUM_REDESIGN_CONVERSION, ID: 373336716, model: 746.
// Short name: SWEFF12B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BF12_PSUM_REDESIGN_CONVERSION.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBf12PsumRedesignConversion: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BF12_PSUM_REDESIGN_CONVERSION program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBf12PsumRedesignConversion(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBf12PsumRedesignConversion.
  /// </summary>
  public FnBf12PsumRedesignConversion(IContext context, Import import,
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
    // ***************************************************
    // 2001-02-26  WR 000235  Fangman - New pgm to recalculate the Monthly Payee
    // Summary totals for the PSUM redesign.
    // 2002-01-25  WR 000235  Fangman - Added changes to track  the recpature 
    // amounts and a payment by process date amount.
    // 2002-05-08  PR 146227  Fangman - Added code to skip the update of 
    // collection count and amount for recaptured passthrus.
    // 2002-05-14  PR 146227  Fangman - Last Update of Monthly Summary call did 
    // not have view matching on the export.  Matched view so that the last row
    // created would not be overlayed w/ zeros in the delete AB.
    // ***************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.CurrentTmst.Timestamp = Now();
    local.EabFileHandling.Action = "WRITE";
    local.AbendCheckLoop.Flag = "Y";
    local.FirstTimeThru.Flag = "Y";

    if (AsChar(local.AbendCheckLoop.Flag) == 'Y')
    {
      UseFnBf12Housekeeping();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test;
      }

      if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail =
          "MO/YR  #  Coll Amt      AF        NA     CR Fees     Suppr     Recap      P/T     X URA";
          
        UseCabControlReport();
      }

      foreach(var item in ReadCsePersonDisbursementDisbursementTypeObligee())
      {
        ++local.ProcessCountToCommit.Count;
        ++local.CountsAndAmounts.NbrOfDisbRead.Count;
        local.CountsAndAmounts.AmtOfDisbRead.TotalCurrency += entities.
          Disbursement.Amount;
        local.CollectionDate.Date = entities.Disbursement.CollectionDate;
        local.CollectionYyMm.YearMonth = UseCabGetYearMonthFromDate();

        if (!Equal(entities.Disbursement.ReferenceNumber,
          local.HoldForCount.ReferenceNumber))
        {
          local.HoldForCount.ReferenceNumber =
            entities.Disbursement.ReferenceNumber;
          ++local.CountsAndAmounts.NbrOfColl.Count;
        }

        if (AsChar(local.FirstTimeThru.Flag) == 'Y')
        {
          local.FirstTimeThru.Flag = "N";
          local.HoldObligee.Number = entities.Obligee1.Number;
          local.CountsAndAmounts.NbrOfArs.Count = 1;
          local.HoldCollection.YearMonth = local.CollectionYyMm.YearMonth;
          local.ForUpdate.Year = Year(entities.Disbursement.CollectionDate);
          local.ForUpdate.Month = Month(entities.Disbursement.CollectionDate);
          local.Hold.ReferenceNumber = entities.Disbursement.ReferenceNumber;
          local.HoldRefNbrCollAmt.TotalCurrency = 0;
          local.ForUpdate.NumberOfCollections = 1;
        }
        else if (!Equal(entities.Obligee1.Number, local.HoldObligee.Number))
        {
          if (local.HoldRefNbrCollAmt.TotalCurrency == 0)
          {
            local.ForUpdate.NumberOfCollections =
              local.ForUpdate.NumberOfCollections.GetValueOrDefault() - 1;
          }

          // Create the monthly summary for the previous person.
          UseFnBf12UpdateMoObligeeSum();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test;
          }

          if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
          {
            UseFnBf12WriteTotals();
          }

          UseFnBf12DeleteMoObligeeSum();

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

          MoveMonthlyObligeeSummary(local.InitializedMonthlyObligeeSummary,
            local.ForUpdate);
          local.HoldObligee.Number = entities.Obligee1.Number;
          ++local.CountsAndAmounts.NbrOfArs.Count;
          local.HoldCollection.YearMonth = local.CollectionYyMm.YearMonth;
          local.ForUpdate.Year = Year(entities.Disbursement.CollectionDate);
          local.ForUpdate.Month = Month(entities.Disbursement.CollectionDate);
          local.Hold.ReferenceNumber = entities.Disbursement.ReferenceNumber;
          local.HoldRefNbrCollAmt.TotalCurrency = 0;
          local.ForUpdate.NumberOfCollections = 1;
          UseFnBf12ClearTbl();
        }
        else if (local.CollectionYyMm.YearMonth != local
          .HoldCollection.YearMonth)
        {
          if (local.HoldRefNbrCollAmt.TotalCurrency == 0)
          {
            local.ForUpdate.NumberOfCollections =
              local.ForUpdate.NumberOfCollections.GetValueOrDefault() - 1;
          }

          // Create the monthly summary for the previous year month.
          UseFnBf12UpdateMoObligeeSum();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test;
          }

          if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
          {
            UseFnBf12WriteTotals();
          }

          MoveMonthlyObligeeSummary(local.InitializedMonthlyObligeeSummary,
            local.ForUpdate);
          local.HoldCollection.YearMonth = local.CollectionYyMm.YearMonth;
          local.ForUpdate.Year = Year(entities.Disbursement.CollectionDate);
          local.ForUpdate.Month = Month(entities.Disbursement.CollectionDate);
          local.Hold.ReferenceNumber = entities.Disbursement.ReferenceNumber;
          local.HoldRefNbrCollAmt.TotalCurrency = 0;
          local.ForUpdate.NumberOfCollections = 1;
        }
        else if (!Equal(entities.Disbursement.ReferenceNumber,
          local.Hold.ReferenceNumber))
        {
          if (local.HoldRefNbrCollAmt.TotalCurrency == 0)
          {
            local.ForUpdate.NumberOfCollections =
              local.ForUpdate.NumberOfCollections.GetValueOrDefault() - 1;
          }

          local.Hold.ReferenceNumber = entities.Disbursement.ReferenceNumber;
          local.HoldRefNbrCollAmt.TotalCurrency = 0;
          local.ForUpdate.NumberOfCollections =
            local.ForUpdate.NumberOfCollections.GetValueOrDefault() + 1;
        }

        local.IncrementCollections.Flag = "Y";

        // Check suppression status/Determine Disb type & update appropriate 
        // buckets.
        if (Equal(entities.Disbursement.ProcessDate,
          local.InitializedDateWorkArea.Date))
        {
          if (entities.DisbursementType.SystemGeneratedIdentifier == 71)
          {
            local.IncrementCollections.Flag = "N";
          }

          local.ForUpdate.DisbursementsSuppressed =
            local.ForUpdate.DisbursementsSuppressed.GetValueOrDefault() + entities
            .Disbursement.Amount;
        }
        else if (AsChar(entities.DisbursementType.RecaptureInd) == 'Y')
        {
          local.ForUpdate.RecapturedAmt =
            local.ForUpdate.RecapturedAmt.GetValueOrDefault() + entities
            .Disbursement.Amount;

          if (entities.DisbursementType.SystemGeneratedIdentifier == 72)
          {
            local.IncrementCollections.Flag = "N";
          }

          UseFnBf12SumByProcessDate();
        }
        else if (AsChar(entities.Disbursement.ExcessUraInd) == 'Y' || Equal
          (entities.DisbursementType.ProgramCode, "XA") || Equal
          (entities.DisbursementType.ProgramCode, "XC"))
        {
          local.ForUpdate.TotExcessUraAmt =
            local.ForUpdate.TotExcessUraAmt.GetValueOrDefault() + entities
            .Disbursement.Amount;
        }
        else if (entities.DisbursementType.SystemGeneratedIdentifier == 73)
        {
          local.ForUpdate.FeeAmount =
            local.ForUpdate.FeeAmount.GetValueOrDefault() + entities
            .Disbursement.Amount;
        }
        else if (entities.DisbursementType.SystemGeneratedIdentifier == 71)
        {
          local.IncrementCollections.Flag = "N";
          local.ForUpdate.PassthruAmount += entities.Disbursement.Amount;
        }
        else if (Equal(entities.DisbursementType.ProgramCode, "FC") || Equal
          (entities.DisbursementType.ProgramCode, "NF") || Equal
          (entities.DisbursementType.ProgramCode, "NC"))
        {
          // Skip these - they do not affect the AR monthly totals.
          local.IncrementCollections.Flag = "N";
        }
        else if (Equal(entities.DisbursementType.ProgramCode, "AF"))
        {
          local.ForUpdate.AdcReimbursedAmount =
            local.ForUpdate.AdcReimbursedAmount.GetValueOrDefault() + entities
            .Disbursement.Amount;
        }
        else if (Equal(entities.DisbursementType.ProgramCode, "NA"))
        {
          local.ForUpdate.CollectionsDisbursedToAr =
            local.ForUpdate.CollectionsDisbursedToAr.GetValueOrDefault() + entities
            .Disbursement.Amount;
        }
        else
        {
          // Unknown code - error
          local.EabReportSend.RptDetail =
            "Invalid Disbursement_Type Program_Code " + NumberToString
            (entities.DisbursementType.SystemGeneratedIdentifier, 15) + "  " + entities
            .DisbursementType.ProgramCode;
          UseCabErrorReport1();
          ExitState = "FN0000_PROGRAM_CODE_ERROR";

          goto Test;
        }

        if (AsChar(local.IncrementCollections.Flag) == 'Y')
        {
          local.ForUpdate.CollectionsAmount =
            local.ForUpdate.CollectionsAmount.GetValueOrDefault() + entities
            .Disbursement.Amount;
          local.HoldRefNbrCollAmt.TotalCurrency += entities.Disbursement.Amount;
        }

        if (AsChar(local.Test.TestDisplayInd.Flag) == 'Y')
        {
          if (entities.Disbursement.Amount < 0)
          {
            local.Sign.Text2 = " -";
          }
          else
          {
            local.Sign.Text2 = "";
          }

          local.UnformattedDate.Date = entities.Disbursement.CollectionDate;
          UseCabFormatDate1();
          local.UnformattedDate.Date = entities.Disbursement.ProcessDate;
          UseCabFormatDate2();
          local.EabReportSend.RptDetail = "AR " + Substring
            (entities.Obligee1.Number, CsePerson.Number_MaxLength, 4, 7) + " " +
            Substring(entities.Disbursement.ReferenceNumber, 2, 11) + " Disb ID " +
            NumberToString
            (entities.Disbursement.SystemGeneratedIdentifier, 7, 9) + local
            .Sign.Text2 + NumberToString((long)(entities.Disbursement.Amount * 100
            ), 10, 6) + " " + local.FormattedCollectionDate.Text10 + "  " + local
            .FormattedProcessDate.Text10 + " " + entities
            .DisbursementType.ProgramCode + "  " + entities
            .DisbursementType.Code + " " + entities.Disbursement.ExcessUraInd;
          UseCabControlReport();
          UseFnBf12PrintMoObligeeSum();
          local.EabReportSend.RptDetail = "";
          UseCabControlReport();
        }
      }

      if (AsChar(local.FirstTimeThru.Flag) == 'N')
      {
        if (local.HoldRefNbrCollAmt.TotalCurrency == 0)
        {
          local.ForUpdate.NumberOfCollections =
            local.ForUpdate.NumberOfCollections.GetValueOrDefault() - 1;
        }

        // Update the monthly summary for the last person/year month.
        UseFnBf12UpdateMoObligeeSum();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test;
        }

        UseFnBf12DeleteMoObligeeSum();
      }
    }

Test:

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      local.EabReportSend.RptDetail = "AR " + entities.Obligee1.Number + "  Disb ID " +
        NumberToString
        (entities.Disbursement.SystemGeneratedIdentifier, 7, 9) + "  ERROR:  " +
        local.ExitStateMessage.Message;
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    UseFnBf12WriteTotals();
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

  private static void MoveCountsAndAmounts1(FnBf12DeleteMoObligeeSum.Import.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveCountsAndAmounts2(FnBf12UpdateMoObligeeSum.Import.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveCountsAndAmounts3(FnBf12WriteTotals.Import.
    CountsAndAmountsGroup source, Local.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveCountsAndAmounts4(Local.CountsAndAmountsGroup source,
    FnBf12DeleteMoObligeeSum.Import.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveCountsAndAmounts5(Local.CountsAndAmountsGroup source,
    FnBf12UpdateMoObligeeSum.Import.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveCountsAndAmounts6(Local.CountsAndAmountsGroup source,
    FnBf12WriteTotals.Import.CountsAndAmountsGroup target)
  {
    target.NbrOfDisbRead.Count = source.NbrOfDisbRead.Count;
    target.AmtOfDisbRead.TotalCurrency = source.AmtOfDisbRead.TotalCurrency;
    target.NbrOfArs.Count = source.NbrOfArs.Count;
    target.NbrOfMoSumRowsUpdated.Count = source.NbrOfMoSumRowsUpdated.Count;
    target.NbrOfMoSumRowsCreated.Count = source.NbrOfMoSumRowsCreated.Count;
    target.NbrOfMoSumRowsDeleted.Count = source.NbrOfMoSumRowsDeleted.Count;
    target.NbrOfRowsNotMatching.Count = source.NbrOfRowsNotMatching.Count;
    target.NbrOfRowsWithNegNbr.Count = source.NbrOfRowsWithNegNbr.Count;
    target.NbrOfColl.Count = source.NbrOfColl.Count;
    target.AmtOfColl.TotalCurrency = source.AmtOfColl.TotalCurrency;
    target.AmtOfAf.TotalCurrency = source.AmtOfAf.TotalCurrency;
    target.AmtOfNa.TotalCurrency = source.AmtOfNa.TotalCurrency;
    target.AmtOfFees.TotalCurrency = source.AmtOfFees.TotalCurrency;
    target.AmtOfSuppr.TotalCurrency = source.AmtOfSuppr.TotalCurrency;
    target.AmtOfRecap.TotalCurrency = source.AmtOfRecap.TotalCurrency;
    target.AmtOfPt.TotalCurrency = source.AmtOfPt.TotalCurrency;
    target.AmtOfXUra.TotalCurrency = source.AmtOfXUra.TotalCurrency;
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.DisbursementsSuppressed = source.DisbursementsSuppressed;
    target.RecapturedAmt = source.RecapturedAmt;
    target.PassthruAmount = source.PassthruAmount;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
    target.CollectionsAmount = source.CollectionsAmount;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
    target.FeeAmount = source.FeeAmount;
    target.AdcReimbursedAmount = source.AdcReimbursedAmount;
    target.TotExcessUraAmt = source.TotExcessUraAmt;
    target.NumberOfCollections = source.NumberOfCollections;
  }

  private static void MoveProcessDtTbl1(FnBf12ClearTbl.Export.
    ProcessDtTblGroup source, Local.ProcessDtTblGroup target)
  {
    target.Common.Flag = source.MoSumTblUpdatedInd.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl2(FnBf12DeleteMoObligeeSum.Export.
    ProcessDtTblGroup source, Local.ProcessDtTblGroup target)
  {
    target.Common.Flag = source.MoSumTblUpdatedInd.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl3(FnBf12SumByProcessDate.Export.
    ProcessDtTblGroup source, Local.ProcessDtTblGroup target)
  {
    target.Common.Flag = source.MoSumTblUpdatedInd.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl4(FnBf12UpdateMoObligeeSum.Export.
    ProcessDtTblGroup source, Local.ProcessDtTblGroup target)
  {
    target.Common.Flag = source.MoSumTblUpdatedInd.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl5(Local.ProcessDtTblGroup source,
    FnBf12DeleteMoObligeeSum.Export.ProcessDtTblGroup target)
  {
    target.MoSumTblUpdatedInd.Flag = source.Common.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl6(Local.ProcessDtTblGroup source,
    FnBf12SumByProcessDate.Export.ProcessDtTblGroup target)
  {
    target.MoSumTblUpdatedInd.Flag = source.Common.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProcessDtTbl7(Local.ProcessDtTblGroup source,
    FnBf12UpdateMoObligeeSum.Export.ProcessDtTblGroup target)
  {
    target.MoSumTblUpdatedInd.Flag = source.Common.Flag;
    target.MonthlyObligeeSummary.Assign(source.MonthlyObligeeSummary);
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveTest1(FnBf12Housekeeping.Export.TestGroup source,
    Local.TestGroup target)
  {
    target.TestRunInd.Flag = source.TestRunInd.Flag;
    target.TestDisplayInd.Flag = source.TestDisplayInd.Flag;
    target.TestFirstObligee.Number = source.TestFirstObligee.Number;
    target.TestLastObligee.Number = source.TestLastObligee.Number;
  }

  private static void MoveTest2(Local.TestGroup source,
    FnBf12DeleteMoObligeeSum.Import.TestGroup target)
  {
    target.TestRunInd.Flag = source.TestRunInd.Flag;
    target.TestDisplayInd.Flag = source.TestDisplayInd.Flag;
    target.TestFirstObligee.Number = source.TestFirstObligee.Number;
    target.TestLastObligee.Number = source.TestLastObligee.Number;
  }

  private static void MoveTest3(Local.TestGroup source,
    FnBf12UpdateMoObligeeSum.Import.TestGroup target)
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

  private void UseCabFormatDate1()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.UnformattedDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedCollectionDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatDate2()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.UnformattedDate.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedProcessDate.Text10 = useExport.FormattedDate.Text10;
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

  private void UseFnBf12ClearTbl()
  {
    var useImport = new FnBf12ClearTbl.Import();
    var useExport = new FnBf12ClearTbl.Export();

    Call(FnBf12ClearTbl.Execute, useImport, useExport);

    useExport.ProcessDtTbl.CopyTo(local.ProcessDtTbl, MoveProcessDtTbl1);
  }

  private void UseFnBf12DeleteMoObligeeSum()
  {
    var useImport = new FnBf12DeleteMoObligeeSum.Import();
    var useExport = new FnBf12DeleteMoObligeeSum.Export();

    useImport.DateWorkArea.Timestamp = local.CurrentTmst.Timestamp;
    MoveCountsAndAmounts4(local.CountsAndAmounts, useImport.CountsAndAmounts);
    MoveTest2(local.Test, useImport.Test);
    useImport.Obligee.Number = local.HoldObligee.Number;
    local.ProcessDtTbl.CopyTo(useExport.ProcessDtTbl, MoveProcessDtTbl5);

    Call(FnBf12DeleteMoObligeeSum.Execute, useImport, useExport);

    MoveCountsAndAmounts1(useImport.CountsAndAmounts, local.CountsAndAmounts);
    useExport.ProcessDtTbl.CopyTo(local.ProcessDtTbl, MoveProcessDtTbl2);
  }

  private void UseFnBf12Housekeeping()
  {
    var useImport = new FnBf12Housekeeping.Import();
    var useExport = new FnBf12Housekeeping.Export();

    Call(FnBf12Housekeeping.Execute, useImport, useExport);

    MoveTest1(useExport.Test, local.Test);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.UpdateFrequencyCount =
      useExport.ProgramCheckpointRestart.UpdateFrequencyCount;
  }

  private void UseFnBf12PrintMoObligeeSum()
  {
    var useImport = new FnBf12PrintMoObligeeSum.Import();
    var useExport = new FnBf12PrintMoObligeeSum.Export();

    useImport.MonthlyObligeeSummary.Assign(local.ForUpdate);

    Call(FnBf12PrintMoObligeeSum.Execute, useImport, useExport);
  }

  private void UseFnBf12SumByProcessDate()
  {
    var useImport = new FnBf12SumByProcessDate.Import();
    var useExport = new FnBf12SumByProcessDate.Export();

    MoveDisbursementTransaction(entities.Disbursement, useImport.Disbursement);
    useImport.DisbursementType.Assign(entities.DisbursementType);
    useImport.TestDisplayInd.Flag = local.Test.TestDisplayInd.Flag;
    local.ProcessDtTbl.CopyTo(useExport.ProcessDtTbl, MoveProcessDtTbl6);

    Call(FnBf12SumByProcessDate.Execute, useImport, useExport);

    useExport.ProcessDtTbl.CopyTo(local.ProcessDtTbl, MoveProcessDtTbl3);
  }

  private void UseFnBf12UpdateMoObligeeSum()
  {
    var useImport = new FnBf12UpdateMoObligeeSum.Import();
    var useExport = new FnBf12UpdateMoObligeeSum.Export();

    useImport.MonthlyObligeeSummary.Assign(local.ForUpdate);
    useImport.DateWorkArea.Timestamp = local.CurrentTmst.Timestamp;
    MoveCountsAndAmounts5(local.CountsAndAmounts, useImport.CountsAndAmounts);
    MoveTest3(local.Test, useImport.Test);
    useImport.Obligee.Number = local.HoldObligee.Number;
    local.ProcessDtTbl.CopyTo(useExport.ProcessDtTbl, MoveProcessDtTbl7);

    Call(FnBf12UpdateMoObligeeSum.Execute, useImport, useExport);

    MoveCountsAndAmounts2(useImport.CountsAndAmounts, local.CountsAndAmounts);
    useExport.ProcessDtTbl.CopyTo(local.ProcessDtTbl, MoveProcessDtTbl4);
  }

  private void UseFnBf12WriteTotals()
  {
    var useImport = new FnBf12WriteTotals.Import();
    var useExport = new FnBf12WriteTotals.Export();

    MoveCountsAndAmounts6(local.CountsAndAmounts, useImport.CountsAndAmounts);

    Call(FnBf12WriteTotals.Execute, useImport, useExport);

    MoveCountsAndAmounts3(useImport.CountsAndAmounts, local.CountsAndAmounts);
  }

  private IEnumerable<bool> ReadCsePersonDisbursementDisbursementTypeObligee()
  {
    entities.Obligee1.Populated = false;
    entities.Obligee2.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadCsePersonDisbursementDisbursementTypeObligee",
      (db, command) =>
      {
        db.SetString(command, "number1", local.Test.TestFirstObligee.Number);
        db.SetString(command, "number2", local.Test.TestLastObligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee1.Number = db.GetString(reader, 0);
        entities.Obligee1.Type1 = db.GetString(reader, 1);
        entities.Disbursement.CpaType = db.GetString(reader, 2);
        entities.Obligee2.Type1 = db.GetString(reader, 2);
        entities.Disbursement.CspNumber = db.GetString(reader, 3);
        entities.Obligee2.CspNumber = db.GetString(reader, 3);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Disbursement.Type1 = db.GetString(reader, 5);
        entities.Disbursement.Amount = db.GetDecimal(reader, 6);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 7);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Disbursement.CollectionDate = db.GetNullableDate(reader, 9);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 10);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 10);
        entities.Disbursement.ReferenceNumber =
          db.GetNullableString(reader, 11);
        entities.Disbursement.ExcessUraInd = db.GetNullableString(reader, 12);
        entities.DisbursementType.Code = db.GetString(reader, 13);
        entities.DisbursementType.CurrentArrearsInd =
          db.GetNullableString(reader, 14);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 15);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 16);
        entities.Obligee1.Populated = true;
        entities.Obligee2.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbursementType.Populated = true;

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
    /// <summary>A ProcessDtTblGroup group.</summary>
    [Serializable]
    public class ProcessDtTblGroup
    {
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
      /// A value of MonthlyObligeeSummary.
      /// </summary>
      [JsonPropertyName("monthlyObligeeSummary")]
      public MonthlyObligeeSummary MonthlyObligeeSummary
      {
        get => monthlyObligeeSummary ??= new();
        set => monthlyObligeeSummary = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 360;

      private Common common;
      private MonthlyObligeeSummary monthlyObligeeSummary;
    }

    /// <summary>A CountsAndAmountsGroup group.</summary>
    [Serializable]
    public class CountsAndAmountsGroup
    {
      /// <summary>
      /// A value of NbrOfDisbRead.
      /// </summary>
      [JsonPropertyName("nbrOfDisbRead")]
      public Common NbrOfDisbRead
      {
        get => nbrOfDisbRead ??= new();
        set => nbrOfDisbRead = value;
      }

      /// <summary>
      /// A value of AmtOfDisbRead.
      /// </summary>
      [JsonPropertyName("amtOfDisbRead")]
      public Common AmtOfDisbRead
      {
        get => amtOfDisbRead ??= new();
        set => amtOfDisbRead = value;
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
      /// A value of NbrOfMoSumRowsUpdated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsUpdated")]
      public Common NbrOfMoSumRowsUpdated
      {
        get => nbrOfMoSumRowsUpdated ??= new();
        set => nbrOfMoSumRowsUpdated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsCreated.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsCreated")]
      public Common NbrOfMoSumRowsCreated
      {
        get => nbrOfMoSumRowsCreated ??= new();
        set => nbrOfMoSumRowsCreated = value;
      }

      /// <summary>
      /// A value of NbrOfMoSumRowsDeleted.
      /// </summary>
      [JsonPropertyName("nbrOfMoSumRowsDeleted")]
      public Common NbrOfMoSumRowsDeleted
      {
        get => nbrOfMoSumRowsDeleted ??= new();
        set => nbrOfMoSumRowsDeleted = value;
      }

      /// <summary>
      /// A value of NbrOfRowsNotMatching.
      /// </summary>
      [JsonPropertyName("nbrOfRowsNotMatching")]
      public Common NbrOfRowsNotMatching
      {
        get => nbrOfRowsNotMatching ??= new();
        set => nbrOfRowsNotMatching = value;
      }

      /// <summary>
      /// A value of NbrOfRowsWithNegNbr.
      /// </summary>
      [JsonPropertyName("nbrOfRowsWithNegNbr")]
      public Common NbrOfRowsWithNegNbr
      {
        get => nbrOfRowsWithNegNbr ??= new();
        set => nbrOfRowsWithNegNbr = value;
      }

      /// <summary>
      /// A value of NbrOfColl.
      /// </summary>
      [JsonPropertyName("nbrOfColl")]
      public Common NbrOfColl
      {
        get => nbrOfColl ??= new();
        set => nbrOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfColl.
      /// </summary>
      [JsonPropertyName("amtOfColl")]
      public Common AmtOfColl
      {
        get => amtOfColl ??= new();
        set => amtOfColl = value;
      }

      /// <summary>
      /// A value of AmtOfAf.
      /// </summary>
      [JsonPropertyName("amtOfAf")]
      public Common AmtOfAf
      {
        get => amtOfAf ??= new();
        set => amtOfAf = value;
      }

      /// <summary>
      /// A value of AmtOfNa.
      /// </summary>
      [JsonPropertyName("amtOfNa")]
      public Common AmtOfNa
      {
        get => amtOfNa ??= new();
        set => amtOfNa = value;
      }

      /// <summary>
      /// A value of AmtOfFees.
      /// </summary>
      [JsonPropertyName("amtOfFees")]
      public Common AmtOfFees
      {
        get => amtOfFees ??= new();
        set => amtOfFees = value;
      }

      /// <summary>
      /// A value of AmtOfSuppr.
      /// </summary>
      [JsonPropertyName("amtOfSuppr")]
      public Common AmtOfSuppr
      {
        get => amtOfSuppr ??= new();
        set => amtOfSuppr = value;
      }

      /// <summary>
      /// A value of AmtOfRecap.
      /// </summary>
      [JsonPropertyName("amtOfRecap")]
      public Common AmtOfRecap
      {
        get => amtOfRecap ??= new();
        set => amtOfRecap = value;
      }

      /// <summary>
      /// A value of AmtOfPt.
      /// </summary>
      [JsonPropertyName("amtOfPt")]
      public Common AmtOfPt
      {
        get => amtOfPt ??= new();
        set => amtOfPt = value;
      }

      /// <summary>
      /// A value of AmtOfXUra.
      /// </summary>
      [JsonPropertyName("amtOfXUra")]
      public Common AmtOfXUra
      {
        get => amtOfXUra ??= new();
        set => amtOfXUra = value;
      }

      private Common nbrOfDisbRead;
      private Common amtOfDisbRead;
      private Common nbrOfArs;
      private Common nbrOfMoSumRowsUpdated;
      private Common nbrOfMoSumRowsCreated;
      private Common nbrOfMoSumRowsDeleted;
      private Common nbrOfRowsNotMatching;
      private Common nbrOfRowsWithNegNbr;
      private Common nbrOfColl;
      private Common amtOfColl;
      private Common amtOfAf;
      private Common amtOfNa;
      private Common amtOfFees;
      private Common amtOfSuppr;
      private Common amtOfRecap;
      private Common amtOfPt;
      private Common amtOfXUra;
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
    /// A value of HoldForCount.
    /// </summary>
    [JsonPropertyName("holdForCount")]
    public DisbursementTransaction HoldForCount
    {
      get => holdForCount ??= new();
      set => holdForCount = value;
    }

    /// <summary>
    /// Gets a value of ProcessDtTbl.
    /// </summary>
    [JsonIgnore]
    public Array<ProcessDtTblGroup> ProcessDtTbl => processDtTbl ??= new(
      ProcessDtTblGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ProcessDtTbl for json serialization.
    /// </summary>
    [JsonPropertyName("processDtTbl")]
    [Computed]
    public IList<ProcessDtTblGroup> ProcessDtTbl_Json
    {
      get => processDtTbl;
      set => ProcessDtTbl.Assign(value);
    }

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public WorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    /// <summary>
    /// A value of FormattedCollectionDate.
    /// </summary>
    [JsonPropertyName("formattedCollectionDate")]
    public WorkArea FormattedCollectionDate
    {
      get => formattedCollectionDate ??= new();
      set => formattedCollectionDate = value;
    }

    /// <summary>
    /// A value of FormattedProcessDate.
    /// </summary>
    [JsonPropertyName("formattedProcessDate")]
    public WorkArea FormattedProcessDate
    {
      get => formattedProcessDate ??= new();
      set => formattedProcessDate = value;
    }

    /// <summary>
    /// A value of UnformattedDate.
    /// </summary>
    [JsonPropertyName("unformattedDate")]
    public DateWorkArea UnformattedDate
    {
      get => unformattedDate ??= new();
      set => unformattedDate = value;
    }

    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public MonthlyObligeeSummary ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of HoldCollection.
    /// </summary>
    [JsonPropertyName("holdCollection")]
    public DateWorkArea HoldCollection
    {
      get => holdCollection ??= new();
      set => holdCollection = value;
    }

    /// <summary>
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public DisbursementTransaction Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of HoldRefNbrCollAmt.
    /// </summary>
    [JsonPropertyName("holdRefNbrCollAmt")]
    public Common HoldRefNbrCollAmt
    {
      get => holdRefNbrCollAmt ??= new();
      set => holdRefNbrCollAmt = value;
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
    /// A value of CurrentTmst.
    /// </summary>
    [JsonPropertyName("currentTmst")]
    public DateWorkArea CurrentTmst
    {
      get => currentTmst ??= new();
      set => currentTmst = value;
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
    /// A value of IncrementCollections.
    /// </summary>
    [JsonPropertyName("incrementCollections")]
    public Common IncrementCollections
    {
      get => incrementCollections ??= new();
      set => incrementCollections = value;
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

    /// <summary>
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InitializedMonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("initializedMonthlyObligeeSummary")]
    public MonthlyObligeeSummary InitializedMonthlyObligeeSummary
    {
      get => initializedMonthlyObligeeSummary ??= new();
      set => initializedMonthlyObligeeSummary = value;
    }

    private DisbursementTransaction holdForCount;
    private Array<ProcessDtTblGroup> processDtTbl;
    private WorkArea sign;
    private WorkArea formattedCollectionDate;
    private WorkArea formattedProcessDate;
    private DateWorkArea unformattedDate;
    private MonthlyObligeeSummary forUpdate;
    private DateWorkArea holdCollection;
    private DisbursementTransaction hold;
    private Common holdRefNbrCollAmt;
    private NumericWorkSet numArCrFeesFromDisbTbl;
    private WorkArea txtArCrFeesFromDisbTbl;
    private NumericWorkSet numArCrFeesFromMoTbl;
    private WorkArea txtArCrFeesFromMoTbl;
    private DateWorkArea currentTmst;
    private CountsAndAmountsGroup countsAndAmounts;
    private TestGroup test;
    private CsePerson holdObligee;
    private DateWorkArea collectionDate;
    private DateWorkArea collectionYyMm;
    private Common incrementCollections;
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
    private DateWorkArea initializedDateWorkArea;
    private MonthlyObligeeSummary initializedMonthlyObligeeSummary;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private DisbursementTransaction disbursement;
    private DisbursementType disbursementType;
  }
#endregion
}

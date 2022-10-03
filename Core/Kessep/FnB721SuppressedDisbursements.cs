// Program: FN_B721_SUPPRESSED_DISBURSEMENTS, ID: 371197622, model: 746.
// Short name: SWE02061
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_SUPPRESSED_DISBURSEMENTS.
/// </summary>
[Serializable]
public partial class FnB721SuppressedDisbursements: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_SUPPRESSED_DISBURSEMENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721SuppressedDisbursements(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721SuppressedDisbursements.
  /// </summary>
  public FnB721SuppressedDisbursements(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -------------------------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // 02/23/05  CMJ				Aging logic
    // 06/14/04  CM Johnson	pr207423	Changes for ocse34 report
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // 					4) Include cost recovery fee in suppressed disbursement amount.
    // 					5) Don't exclude Duplicate suppressions. (Undo changes made for 
    // pr207423)
    // 					6) Skip adjusted collections if the disbursement adjustment has
    // 					   not yet processed.
    // 10/05/12  GVandy	CQ36580		Correct court order suppression logic.
    // -----------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------
    // -- Open CSE Suppressed Disbursement error file and CSE Suppressed FDSO 
    // Disbursement error file.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.FileNumber.Count = 7;

          break;
        case 2:
          local.FileNumber.Count = 8;

          break;
        default:
          break;
      }

      local.External.FileInstruction = "OPEN";
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Header Records to CSE Suppressed Disbursement error file.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.DateWorkArea.TextDate =
            NumberToString(DateToInt(import.ReportingPeriodEndDate.Date), 8, 8);
            
          local.External.TextLine80 = "00 REPORTING PERIOD END DATE = " + local
            .DateWorkArea.TextDate;
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 39) + "...SUPPRESSED DISBURSEMENT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 CSP NUMBER      AMOUNT    CREATED DATE  PROCESS DATE  REFERENCE NUMBER  LGL    AGE";
            

          break;
        default:
          break;
      }

      local.FileNumber.Count = 7;
      local.External.FileInstruction = "WRITE";
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Header Records to CSE Suppressed FDSO Disbursement error file.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.DateWorkArea.TextDate =
            NumberToString(DateToInt(import.ReportingPeriodEndDate.Date), 8, 8);
            
          local.External.TextLine80 = "00 REPORTING PERIOD END DATE = " + local
            .DateWorkArea.TextDate;
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 41) + "...SUPPRESSED FDSO DISBURSEMENT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00  CSP NUMBER      AMOUNT   CREATED DATE  PROCESS DATE  REFERENCE NUMBER";
            

          break;
        default:
          break;
      }

      local.FileNumber.Count = 8;
      local.External.FileInstruction = "WRITE";
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // cmj 02/23/2005 calculate the date for the aging process.
    local.Gt2DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -1);
    local.Gt30DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -29);
    local.Gt180DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -181);
    local.Gt365DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -364);
    local.Gt1095DayDate.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1094);
    local.Gt1825DayDate.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1824);

    // -------------------------------------------------------------------------------------------
    // -- Read each Suppressed Disbursement
    // -------------------------------------------------------------------------------------------
    // cmj 6/09/2004  pr 207423  exclude duplicate suppressions - which are a '
    // d' in the suppression reason of disbursement history
    // -- CQ486  Don't exclude duplicate suppressions.  This reverses the change
    // for pr207423.
    foreach(var item in ReadDisbursementTransactionCsePerson())
    {
      local.ForCreate.Comment = "";
      ReadDisbursementStatusHistory();
      local.DisbEffective.Date =
        entities.DisbursementStatusHistory.EffectiveDate;
      local.SuppLegalDisb.Flag = "N";

      if (ReadDisbursementTransactionRln())
      {
        // continue processing
      }
      else
      {
        continue;
      }

      if (ReadDisbursementTransaction2())
      {
        // continue processing
      }
      else
      {
        continue;
      }

      // -- CQ486  Skip if collection is adjusted and the disbursement 
      // adjustment has not yet processed.
      if (ReadCollection())
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
          Lt(import.ReportingPeriodEndDate.Date,
          entities.Collection.CollectionAdjustmentDt) && (
            Equal(entities.Collection.DisbursementAdjProcessDate,
          local.Null1.Date) || Lt
          (import.ReportingPeriodEndDate.Date,
          entities.Collection.DisbursementAdjProcessDate)))
        {
          // -- CQ486 The disbursement adjustment processing has not yet 
          // occurred, so there is no negative disbursement amount to offset the
          // positive disbursement amount.  Since the collection is adjusted
          // the collection amount will be included in Suspense, do not include
          // it in the suppressed disbursements.
          continue;
        }
      }
      else
      {
        continue;
      }

      // -- CQ486  Calculate total debit disbursement amount including cost 
      // recovery fee.
      local.CrfAmt.Amount = 0;
      local.TotalDebitPlusCrfAmt.Amount = 0;

      foreach(var item1 in ReadDisbursementTransaction3())
      {
        local.CrfAmt.Amount += entities.CostRecoveryFeeDisbursementTransaction.
          Amount;
      }

      // -- CQ486  More changes to Cost Recovery Fee calculation.
      if (local.CrfAmt.Amount > 0)
      {
        if (ReadDisbursementTransaction1())
        {
          // -- CQ486  If the disbursement credit split into multiple 
          // disbursement debits then we only
          //     include the cost recovery fee with one of the debits.  We'll 
          // include it on the debit with the
          //     smallest disbursement system generated id.
          local.CrfAmt.Amount = 0;
        }
      }

      local.TotalDebitPlusCrfAmt.Amount = entities.Debit.Amount + local
        .CrfAmt.Amount;

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // -- Check for FDSO and Legal suppressions.
        if (ReadCollectionType())
        {
          if (entities.CollectionType.SequentialIdentifier == 3)
          {
            // -- F type (i.e. FDSO) collections will be classified as FDSO 
            // suppressions if a F
            //    type collection suppression rule exists.
            if (ReadDisbSuppressionStatusHistory2())
            {
              local.SuppFdsoDisb.TotalCurrency += local.TotalDebitPlusCrfAmt.
                Amount;
              ++local.SuppFdsoDisb.Count;
              local.ForCreate.Comment = "B721 SUPPRESSED DISBURSEMENTS (FDSO)";
              local.FileNumber.Count = 8;
              local.DateWorkArea.TextDate =
                NumberToString(DateToInt(entities.Debit.ProcessDate), 8, 8);
              local.Created.TextDate =
                NumberToString(DateToInt(Date(entities.Debit.CreatedTimestamp)),
                8, 8);
              local.TextWorkArea.Text30 =
                NumberToString((long)(local.TotalDebitPlusCrfAmt.Amount * 100),
                5, 9) + "." + NumberToString
                ((long)(local.TotalDebitPlusCrfAmt.Amount * 100), 14, 2);
              local.External.TextLine80 = "O1  " + entities.CsePerson.Number;

              if (local.TotalDebitPlusCrfAmt.Amount < 0)
              {
                local.External.TextLine80 =
                  TrimEnd(local.External.TextLine80) + "  -";
              }

              local.External.TextLine80 =
                Substring(local.External.TextLine80,
                External.TextLine80_MaxLength, 1, 16) + local
                .TextWorkArea.Text30;
              local.External.TextLine80 =
                Substring(local.External.TextLine80,
                External.TextLine80_MaxLength, 1, 31) + local.Created.TextDate;
              local.External.TextLine80 =
                Substring(local.External.TextLine80,
                External.TextLine80_MaxLength, 1, 45) + local
                .DateWorkArea.TextDate;
              local.External.TextLine80 =
                Substring(local.External.TextLine80,
                External.TextLine80_MaxLength, 1, 59) + entities
                .Debit.ReferenceNumber;
              UseFnB721ExtWriteFile();

              if (!Equal(local.External.TextReturnCode, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              goto Test;
            }
            else
            {
              // -- Continue.
            }
          }
        }
        else
        {
          // -- Continue.
        }

        // -- Either the collection type is not 3 or the collection type is 3 
        // but no F type
        //    suppression existed.  Check for court order suppression.
        // -- Collections will be classified as court order suppressions if a 
        // court order
        //    suppression rule exists for this court order number.
        if (ReadDisbSuppressionStatusHistory1())
        {
          local.SuppLegalDisb.Flag = "Y";
          ++local.SuppLegalDisb.Count;
          local.SuppLegalDisb.TotalCurrency += local.TotalDebitPlusCrfAmt.
            Amount;
          local.ForCreate.Comment = "B721 SUPPRESSED DISBURSEMENTS (LEGAL)";
        }
        else
        {
          // -- Continue.
        }
      }

Test:

      // @@@ Old FDSO and Legal suppression code below....
      // @@@ Old FDSO and Legal suppression code above....
      local.CompareDate.Date = entities.DisbursementStatusHistory.EffectiveDate;
      local.CompareDateText.Text8 =
        NumberToString(DateToInt(local.CompareDate.Date), 8, 8);
      ++local.TotalSuppressedDisbmnt.Count;
      local.TotalSuppressedDisbmnt.TotalCurrency += local.TotalDebitPlusCrfAmt.
        Amount;
      local.DateWorkArea.TextDate =
        NumberToString(DateToInt(entities.Debit.ProcessDate), 8, 8);
      local.Created.TextDate =
        NumberToString(DateToInt(Date(entities.Debit.CreatedTimestamp)), 8, 8);
      local.TextWorkArea.Text30 =
        NumberToString((long)(local.TotalDebitPlusCrfAmt.Amount * 100), 5, 9) +
        "." + NumberToString
        ((long)(local.TotalDebitPlusCrfAmt.Amount * 100), 14, 2);

      // -------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <Payee Number> <Amount> <Created 
      // Date> <Process Date> <Payment Request ID> <Disbursement Status History
      // Effective Date>
      // -------------------------------------------------------------------------------------------
      local.FileNumber.Count = 7;
      local.External.TextLine80 = "01 " + entities.CsePerson.Number;

      if (local.TotalDebitPlusCrfAmt.Amount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "  -";
      }

      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        16) + local.TextWorkArea.Text30;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        31) + local.Created.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        45) + local.DateWorkArea.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        59) + entities.Debit.ReferenceNumber;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        75) + local.SuppLegalDisb.Flag;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        80) + local.CompareDateText.Text8;
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (AsChar(import.Audit.Flag) == 'Y')
      {
        // -- Log the amount to the audit file.
        local.ForCreate.FiscalYear =
          import.Ocse157Verification.FiscalYear.GetValueOrDefault();
        local.ForCreate.RunNumber =
          import.Ocse157Verification.RunNumber.GetValueOrDefault();
        local.ForCreate.LineNumber = "B13";

        if (IsEmpty(local.ForCreate.Comment))
        {
          local.ForCreate.Comment = "B721 SUPPRESSED DISBURSEMENTS (1)";
        }

        local.ForCreate.ObligorPersonNbr = entities.CsePerson.Number;
        local.ForCreate.CollectionDte = Date(entities.Debit.CreatedTimestamp);
        local.ForCreate.CollectionAmount = local.TotalDebitPlusCrfAmt.Amount;

        if (ReadCashReceiptDetailCashReceipt())
        {
          // *** Build CRD #
          local.ForCreate.CaseWorkerName =
            NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
          local.ForCreate.CaseWorkerName =
            TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        }
        else
        {
          local.ForCreate.CaseWorkerName = "";
        }

        UseFnCreateOcse157Verification();
      }

      // cmj 02/23/2005 compare date for aging calculate the aging amount
      if (Lt(local.CompareDate.Date, local.Gt1825DayDate.Date))
      {
        local.AgingGt1825.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt1825.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt1095DayDate.Date))
      {
        local.AgingGt1095.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt1095.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt365DayDate.Date))
      {
        local.AgingGt365.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt365.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt180DayDate.Date))
      {
        local.AgingGt180.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt180.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt30DayDate.Date))
      {
        local.AgingGt30.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt30.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt2DayDate.Date))
      {
        local.AgingGt2.TotalCurrency += local.TotalDebitPlusCrfAmt.Amount;
        ++local.AgingGt2.Count;

        continue;
      }

      local.AgingLastDaysBusiness.TotalCurrency += local.TotalDebitPlusCrfAmt.
        Amount;
      ++local.AgingLastDaysBusiness.Count;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // SUPPRESSED DISB AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.FileNumber.Count = 7;
          local.TextWorkArea.Text30 =
            NumberToString((long)(local.TotalSuppressedDisbmnt.TotalCurrency * 100
            ), 1, 13) + "." + NumberToString
            ((long)(local.TotalSuppressedDisbmnt.TotalCurrency * 100), 14, 2);
          local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
            (local.TotalSuppressedDisbmnt.Count, 6, 10);
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 35) + "Total Supp Disbmnt Amount = ";
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 63) + local.TextWorkArea.Text30;

          if (local.TotalSuppressedDisbmnt.TotalCurrency < 0)
          {
            local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-"
              ;
          }

          break;
        case 2:
          local.FileNumber.Count = 8;
          local.TextWorkArea.Text30 =
            NumberToString((long)(local.SuppFdsoDisb.TotalCurrency * 100), 1, 13)
            + "." + NumberToString
            ((long)(local.SuppFdsoDisb.TotalCurrency * 100), 14, 2);
          local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
            (local.SuppFdsoDisb.Count, 6, 10);
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 35) + "Total FDSO Supp Disb Amt = ";
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 63) + local.TextWorkArea.Text30;

          if (local.SuppFdsoDisb.TotalCurrency < 0)
          {
            local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-"
              ;
          }

          break;
        default:
          break;
      }

      local.External.FileInstruction = "WRITE";
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Close CSE Suppressed Disbursement error file and CSE Suppressed FDSO 
    // Disbursement error file.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 2; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.FileNumber.Count = 7;

          break;
        case 2:
          local.FileNumber.Count = 8;

          break;
        default:
          break;
      }

      local.External.FileInstruction = "CLOSE";
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

        return;
      }
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Total Suppressed Disbursement amount to the OCSE34 record.
    // -------------------------------------------------------------------------------------------
    if (ReadOcse34())
    {
      try
      {
        UpdateOcse34();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OCSE34_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OCSE34_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OCSE34_NF";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Checkpoint.
    // -------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "08 " + Substring
      (local.ProgramCheckpointRestart.RestartInfo, 250, 4, 247);
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write totals to the control report.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 12; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.TextWorkArea.Text30 =
            NumberToString((long)(local.TotalSuppressedDisbmnt.TotalCurrency * 100
            ), 1, 13) + "." + NumberToString
            ((long)(local.TotalSuppressedDisbmnt.TotalCurrency * 100), 14, 2);
          local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
            (local.TotalSuppressedDisbmnt.Count, 6, 10);
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 35) + "Total Supp Disbmnt Amount = ";
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 63) + local.TextWorkArea.Text30;

          if (local.TotalSuppressedDisbmnt.TotalCurrency < 0)
          {
            local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-"
              ;
          }

          local.EabReportSend.RptDetail = "SUPPRESSED DISBMNT...." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 76);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Suppressed Disbursements........Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuppressDisbLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuppressDisbLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuppressDisbGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuppressDisbGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuprsDisbGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuprsDisbGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuprsDisbGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuprsDisbGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuprsDisbGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuprsDisbGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuprsDisbGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuprsDisbGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuprsDisbGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuprsDisbGt1825, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "      FDSO Suppressions............." + NumberToString
            (local.SuppFdsoDisb.Count, 7, 9) + "......" + NumberToString
            ((long)local.SuppFdsoDisb.TotalCurrency, 7, 9);

          if (local.SuppFdsoDisb.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "      Legal Suppressions............" + NumberToString
            (local.SuppLegalDisb.Count, 7, 9) + "......" + NumberToString
            ((long)local.SuppLegalDisb.TotalCurrency, 7, 9);

          if (local.SuppLegalDisb.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 12:
          local.EabReportSend.RptDetail = "";

          break;
        case 13:
          break;
        case 14:
          break;
        default:
          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "SI0000_ERR_WRITING_CTL_RPT_FILE";

        return;
      }
    }
  }

  private static void MoveExternal(External source, External target)
  {
    target.FileInstruction = source.FileInstruction;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveOcse157Verification(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.CheckpointCount = source.CheckpointCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

  private void UseFnB721ExtWriteFile()
  {
    var useImport = new FnB721ExtWriteFile.Import();
    var useExport = new FnB721ExtWriteFile.Export();

    useImport.FileNumber.Count = local.FileNumber.Count;
    MoveExternal(local.External, useImport.External);
    useExport.External.TextReturnCode = local.External.TextReturnCode;

    Call(FnB721ExtWriteFile.Execute, useImport, useExport);

    local.External.TextReturnCode = useExport.External.TextReturnCode;
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification(local.ForCreate, useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
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

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetailCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
        db.SetInt32(
          command, "crvIdentifier", entities.Credit.CrvId.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier", entities.Credit.CstId.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier", entities.Credit.CrtId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 6);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.
          SetInt32(command, "collId", entities.Credit.ColId.GetValueOrDefault());
          
        db.
          SetInt32(command, "otyId", entities.Credit.OtyId.GetValueOrDefault());
          
        db.
          SetInt32(command, "obgId", entities.Credit.ObgId.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.Credit.CspNumberDisb ?? "");
        db.SetString(command, "cpaType", entities.Credit.CpaTypeDisb ?? "");
        db.
          SetInt32(command, "otrId", entities.Credit.OtrId.GetValueOrDefault());
          
        db.SetString(command, "otrType", entities.Credit.OtrTypeDisb ?? "");
        db.SetInt32(
          command, "crtType", entities.Credit.CrtId.GetValueOrDefault());
        db.
          SetInt32(command, "cstId", entities.Credit.CstId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crvId", entities.Credit.CrvId.GetValueOrDefault());
          
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 13);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 14);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 15);
        entities.Collection.CourtOrderAppliedTo =
          db.GetNullableString(reader, 16);
        entities.Collection.ArNumber = db.GetNullableString(reader, 17);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.
          SetInt32(command, "crdId", entities.Credit.CrdId.GetValueOrDefault());
          
        db.SetInt32(
          command, "crvIdentifier", entities.Credit.CrvId.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier", entities.Credit.CstId.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier", entities.Credit.CrtId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDisbSuppressionStatusHistory1()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.DisbEffective.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.Collection.CourtOrderAppliedTo ?? ""
          );
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory2()
  {
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          local.DisbEffective.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "cltSequentialId",
          entities.CollectionType.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.CltSequentialId =
          db.GetNullableInt32(reader, 3);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 6);
        entities.DisbSuppressionStatusHistory.LgaIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
      });
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.DisbursementStatusHistory.SuppressionReason =
          db.GetNullableString(reader, 8);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadDisbursementTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.Other.Populated = false;

    return Read("ReadDisbursementTransaction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.Credit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Credit.CpaType);
        db.SetString(command, "cspPNumber", entities.Credit.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.Debit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Other.CpaType = db.GetString(reader, 0);
        entities.Other.CspNumber = db.GetString(reader, 1);
        entities.Other.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Other.Type1 = db.GetString(reader, 3);
        entities.Other.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Other.RecapturedInd = db.GetString(reader, 5);
        entities.Other.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.Other.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Other.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Other.Type1);
      });
  }

  private bool ReadDisbursementTransaction2()
  {
    System.Diagnostics.Debug.Assert(
      entities.DisbursementTransactionRln.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransactionRln.DtrPGeneratedId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransactionRln.CpaPType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransactionRln.CspPNumber);
          
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Amount = db.GetDecimal(reader, 4);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Credit.OtyId = db.GetNullableInt32(reader, 7);
        entities.Credit.OtrTypeDisb = db.GetNullableString(reader, 8);
        entities.Credit.OtrId = db.GetNullableInt32(reader, 9);
        entities.Credit.CpaTypeDisb = db.GetNullableString(reader, 10);
        entities.Credit.CspNumberDisb = db.GetNullableString(reader, 11);
        entities.Credit.ObgId = db.GetNullableInt32(reader, 12);
        entities.Credit.CrdId = db.GetNullableInt32(reader, 13);
        entities.Credit.CrvId = db.GetNullableInt32(reader, 14);
        entities.Credit.CstId = db.GetNullableInt32(reader, 15);
        entities.Credit.CrtId = db.GetNullableInt32(reader, 16);
        entities.Credit.ColId = db.GetNullableInt32(reader, 17);
        entities.Credit.ReferenceNumber = db.GetNullableString(reader, 18);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.Credit.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.Credit.CpaTypeDisb);
      });
  }

  private IEnumerable<bool> ReadDisbursementTransaction3()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.CostRecoveryFeeDisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransaction3",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.Credit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Credit.CpaType);
        db.SetString(command, "cspPNumber", entities.Credit.CspNumber);
      },
      (db, reader) =>
      {
        entities.CostRecoveryFeeDisbursementTransaction.CpaType =
          db.GetString(reader, 0);
        entities.CostRecoveryFeeDisbursementTransaction.CspNumber =
          db.GetString(reader, 1);
        entities.CostRecoveryFeeDisbursementTransaction.
          SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.CostRecoveryFeeDisbursementTransaction.Type1 =
          db.GetString(reader, 3);
        entities.CostRecoveryFeeDisbursementTransaction.Amount =
          db.GetDecimal(reader, 4);
        entities.CostRecoveryFeeDisbursementTransaction.DbtGeneratedId =
          db.GetNullableInt32(reader, 5);
        entities.CostRecoveryFeeDisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.CostRecoveryFeeDisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.CostRecoveryFeeDisbursementTransaction.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementTransactionCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.Debit.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePerson",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debit.CpaType = db.GetString(reader, 0);
        entities.Debit.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.Debit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debit.Type1 = db.GetString(reader, 3);
        entities.Debit.Amount = db.GetDecimal(reader, 4);
        entities.Debit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Debit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Debit.RecapturedInd = db.GetString(reader, 7);
        entities.Debit.ReferenceNumber = db.GetNullableString(reader, 8);
        entities.CsePerson.Populated = true;
        entities.Debit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Debit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Debit.Type1);

        return true;
      });
  }

  private bool ReadDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Debit.Populated);
    entities.DisbursementTransactionRln.Populated = false;

    return Read("ReadDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId", entities.Debit.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debit.CpaType);
        db.SetString(command, "cspNumber", entities.Debit.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionRln.Description =
          db.GetNullableString(reader, 1);
        entities.DisbursementTransactionRln.CreatedBy = db.GetString(reader, 2);
        entities.DisbursementTransactionRln.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.DisbursementTransactionRln.DnrGeneratedId =
          db.GetInt32(reader, 4);
        entities.DisbursementTransactionRln.CspNumber = db.GetString(reader, 5);
        entities.DisbursementTransactionRln.CpaType = db.GetString(reader, 6);
        entities.DisbursementTransactionRln.DtrGeneratedId =
          db.GetInt32(reader, 7);
        entities.DisbursementTransactionRln.CspPNumber =
          db.GetString(reader, 8);
        entities.DisbursementTransactionRln.CpaPType = db.GetString(reader, 9);
        entities.DisbursementTransactionRln.DtrPGeneratedId =
          db.GetInt32(reader, 10);
        entities.DisbursementTransactionRln.Populated = true;
        CheckValid<DisbursementTransactionRln>("CpaType",
          entities.DisbursementTransactionRln.CpaType);
        CheckValid<DisbursementTransactionRln>("CpaPType",
          entities.DisbursementTransactionRln.CpaPType);
      });
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.Ocse34.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.Ocse34.CseDisbSuppressAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.FdsoDsbSuppAmt = db.GetNullableInt32(reader, 2);
        entities.Ocse34.SuppressDisbLda = db.GetNullableInt32(reader, 3);
        entities.Ocse34.SuppressDisbGt2 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.SuprsDisbGt30 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.SuprsDisbGt180 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.SuprsDisbGt365 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.SuprsDisbGt1095 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.SuprsDisbGt1825 = db.GetNullableInt32(reader, 9);
        entities.Ocse34.SuppDisbLegal = db.GetNullableInt32(reader, 10);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse34()
  {
    var cseDisbSuppressAmt = (int?)local.TotalSuppressedDisbmnt.TotalCurrency;
    var fdsoDsbSuppAmt = (int?)local.SuppFdsoDisb.TotalCurrency;
    var suppressDisbLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var suppressDisbGt2 = (int?)local.AgingGt2.TotalCurrency;
    var suprsDisbGt30 = (int?)local.AgingGt30.TotalCurrency;
    var suprsDisbGt180 = (int?)local.AgingGt180.TotalCurrency;
    var suprsDisbGt365 = (int?)local.AgingGt365.TotalCurrency;
    var suprsDisbGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var suprsDisbGt1825 = (int?)local.AgingGt1825.TotalCurrency;
    var suppDisbLegal = (int?)local.SuppLegalDisb.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cseDsbSuppAmt", cseDisbSuppressAmt);
        db.SetNullableInt32(command, "fdsoDsbSuppAmt", fdsoDsbSuppAmt);
        db.SetNullableInt32(command, "suppressDisbLda", suppressDisbLda);
        db.SetNullableInt32(command, "suppressDisbGt2", suppressDisbGt2);
        db.SetNullableInt32(command, "suprsDisbGt30", suprsDisbGt30);
        db.SetNullableInt32(command, "suprsDisbGt180", suprsDisbGt180);
        db.SetNullableInt32(command, "suprsDisbGt365", suprsDisbGt365);
        db.SetNullableInt32(command, "suprsDisbGt1095", suprsDisbGt1095);
        db.SetNullableInt32(command, "suprsDisbGt1825", suprsDisbGt1825);
        db.SetNullableInt32(command, "suppDisbLegal", suppDisbLegal);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CseDisbSuppressAmt = cseDisbSuppressAmt;
    entities.Ocse34.FdsoDsbSuppAmt = fdsoDsbSuppAmt;
    entities.Ocse34.SuppressDisbLda = suppressDisbLda;
    entities.Ocse34.SuppressDisbGt2 = suppressDisbGt2;
    entities.Ocse34.SuprsDisbGt30 = suprsDisbGt30;
    entities.Ocse34.SuprsDisbGt180 = suprsDisbGt180;
    entities.Ocse34.SuprsDisbGt365 = suprsDisbGt365;
    entities.Ocse34.SuprsDisbGt1095 = suprsDisbGt1095;
    entities.Ocse34.SuprsDisbGt1825 = suprsDisbGt1825;
    entities.Ocse34.SuppDisbLegal = suppDisbLegal;
    entities.Ocse34.Populated = true;
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
    /// <summary>
    /// A value of ReportingPeriodEndDate.
    /// </summary>
    [JsonPropertyName("reportingPeriodEndDate")]
    public DateWorkArea ReportingPeriodEndDate
    {
      get => reportingPeriodEndDate ??= new();
      set => reportingPeriodEndDate = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of Audit.
    /// </summary>
    [JsonPropertyName("audit")]
    public Common Audit
    {
      get => audit ??= new();
      set => audit = value;
    }

    /// <summary>
    /// A value of Ocse157Verification.
    /// </summary>
    [JsonPropertyName("ocse157Verification")]
    public Ocse157Verification Ocse157Verification
    {
      get => ocse157Verification ??= new();
      set => ocse157Verification = value;
    }

    private DateWorkArea reportingPeriodEndDate;
    private Ocse34 ocse34;
    private Common audit;
    private Ocse157Verification ocse157Verification;
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
    /// A value of CrfAmt.
    /// </summary>
    [JsonPropertyName("crfAmt")]
    public DisbursementTransaction CrfAmt
    {
      get => crfAmt ??= new();
      set => crfAmt = value;
    }

    /// <summary>
    /// A value of TotalDebitPlusCrfAmt.
    /// </summary>
    [JsonPropertyName("totalDebitPlusCrfAmt")]
    public DisbursementTransaction TotalDebitPlusCrfAmt
    {
      get => totalDebitPlusCrfAmt ??= new();
      set => totalDebitPlusCrfAmt = value;
    }

    /// <summary>
    /// A value of CompareDateText.
    /// </summary>
    [JsonPropertyName("compareDateText")]
    public TextWorkArea CompareDateText
    {
      get => compareDateText ??= new();
      set => compareDateText = value;
    }

    /// <summary>
    /// A value of CompareDate.
    /// </summary>
    [JsonPropertyName("compareDate")]
    public DateWorkArea CompareDate
    {
      get => compareDate ??= new();
      set => compareDate = value;
    }

    /// <summary>
    /// A value of SuppLegalDisb.
    /// </summary>
    [JsonPropertyName("suppLegalDisb")]
    public Common SuppLegalDisb
    {
      get => suppLegalDisb ??= new();
      set => suppLegalDisb = value;
    }

    /// <summary>
    /// A value of SuppFdsoDisb.
    /// </summary>
    [JsonPropertyName("suppFdsoDisb")]
    public Common SuppFdsoDisb
    {
      get => suppFdsoDisb ??= new();
      set => suppFdsoDisb = value;
    }

    /// <summary>
    /// A value of TotalSuppressedDisbmnt.
    /// </summary>
    [JsonPropertyName("totalSuppressedDisbmnt")]
    public Common TotalSuppressedDisbmnt
    {
      get => totalSuppressedDisbmnt ??= new();
      set => totalSuppressedDisbmnt = value;
    }

    /// <summary>
    /// A value of DisbEffective.
    /// </summary>
    [JsonPropertyName("disbEffective")]
    public DateWorkArea DisbEffective
    {
      get => disbEffective ??= new();
      set => disbEffective = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of AgingGt1825.
    /// </summary>
    [JsonPropertyName("agingGt1825")]
    public Common AgingGt1825
    {
      get => agingGt1825 ??= new();
      set => agingGt1825 = value;
    }

    /// <summary>
    /// A value of AgingGt1095.
    /// </summary>
    [JsonPropertyName("agingGt1095")]
    public Common AgingGt1095
    {
      get => agingGt1095 ??= new();
      set => agingGt1095 = value;
    }

    /// <summary>
    /// A value of AgingGt365.
    /// </summary>
    [JsonPropertyName("agingGt365")]
    public Common AgingGt365
    {
      get => agingGt365 ??= new();
      set => agingGt365 = value;
    }

    /// <summary>
    /// A value of AgingGt180.
    /// </summary>
    [JsonPropertyName("agingGt180")]
    public Common AgingGt180
    {
      get => agingGt180 ??= new();
      set => agingGt180 = value;
    }

    /// <summary>
    /// A value of AgingGt30.
    /// </summary>
    [JsonPropertyName("agingGt30")]
    public Common AgingGt30
    {
      get => agingGt30 ??= new();
      set => agingGt30 = value;
    }

    /// <summary>
    /// A value of AgingGt2.
    /// </summary>
    [JsonPropertyName("agingGt2")]
    public Common AgingGt2
    {
      get => agingGt2 ??= new();
      set => agingGt2 = value;
    }

    /// <summary>
    /// A value of AgingLastDaysBusiness.
    /// </summary>
    [JsonPropertyName("agingLastDaysBusiness")]
    public Common AgingLastDaysBusiness
    {
      get => agingLastDaysBusiness ??= new();
      set => agingLastDaysBusiness = value;
    }

    /// <summary>
    /// A value of Gt1825DayDate.
    /// </summary>
    [JsonPropertyName("gt1825DayDate")]
    public DateWorkArea Gt1825DayDate
    {
      get => gt1825DayDate ??= new();
      set => gt1825DayDate = value;
    }

    /// <summary>
    /// A value of Gt1095DayDate.
    /// </summary>
    [JsonPropertyName("gt1095DayDate")]
    public DateWorkArea Gt1095DayDate
    {
      get => gt1095DayDate ??= new();
      set => gt1095DayDate = value;
    }

    /// <summary>
    /// A value of Gt365DayDate.
    /// </summary>
    [JsonPropertyName("gt365DayDate")]
    public DateWorkArea Gt365DayDate
    {
      get => gt365DayDate ??= new();
      set => gt365DayDate = value;
    }

    /// <summary>
    /// A value of Gt180DayDate.
    /// </summary>
    [JsonPropertyName("gt180DayDate")]
    public DateWorkArea Gt180DayDate
    {
      get => gt180DayDate ??= new();
      set => gt180DayDate = value;
    }

    /// <summary>
    /// A value of Gt30DayDate.
    /// </summary>
    [JsonPropertyName("gt30DayDate")]
    public DateWorkArea Gt30DayDate
    {
      get => gt30DayDate ??= new();
      set => gt30DayDate = value;
    }

    /// <summary>
    /// A value of Gt2DayDate.
    /// </summary>
    [JsonPropertyName("gt2DayDate")]
    public DateWorkArea Gt2DayDate
    {
      get => gt2DayDate ??= new();
      set => gt2DayDate = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public DateWorkArea Created
    {
      get => created ??= new();
      set => created = value;
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
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Local20990101.
    /// </summary>
    [JsonPropertyName("local20990101")]
    public DateWorkArea Local20990101
    {
      get => local20990101 ??= new();
      set => local20990101 = value;
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    private DisbursementTransaction crfAmt;
    private DisbursementTransaction totalDebitPlusCrfAmt;
    private TextWorkArea compareDateText;
    private DateWorkArea compareDate;
    private Common suppLegalDisb;
    private Common suppFdsoDisb;
    private Common totalSuppressedDisbmnt;
    private DateWorkArea disbEffective;
    private Ocse34 ocse34;
    private Common agingGt1825;
    private Common agingGt1095;
    private Common agingGt365;
    private Common agingGt180;
    private Common agingGt30;
    private Common agingGt2;
    private Common agingLastDaysBusiness;
    private DateWorkArea gt1825DayDate;
    private DateWorkArea gt1095DayDate;
    private DateWorkArea gt365DayDate;
    private DateWorkArea gt180DayDate;
    private DateWorkArea gt30DayDate;
    private DateWorkArea gt2DayDate;
    private CsePerson prev;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DateWorkArea created;
    private DateWorkArea null1;
    private Common fileNumber;
    private External external;
    private DateWorkArea dateWorkArea;
    private TextWorkArea textWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DateWorkArea local20990101;
    private Common common;
    private Ocse157Verification forCreate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public DisbursementTransaction Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of CostRecoveryFeeDisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("costRecoveryFeeDisbursementTransactionRln")]
    public DisbursementTransactionRln CostRecoveryFeeDisbursementTransactionRln
    {
      get => costRecoveryFeeDisbursementTransactionRln ??= new();
      set => costRecoveryFeeDisbursementTransactionRln = value;
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
    /// A value of CostRecoveryFeeDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("costRecoveryFeeDisbursementTransaction")]
    public DisbursementTransaction CostRecoveryFeeDisbursementTransaction
    {
      get => costRecoveryFeeDisbursementTransaction ??= new();
      set => costRecoveryFeeDisbursementTransaction = value;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of Debit.
    /// </summary>
    [JsonPropertyName("debit")]
    public DisbursementTransaction Debit
    {
      get => debit ??= new();
      set => debit = value;
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

    private LegalAction legalAction;
    private PaymentRequest paymentRequest;
    private DisbursementTransaction other;
    private DisbursementTransactionRln costRecoveryFeeDisbursementTransactionRln;
      
    private DisbursementType disbursementType;
    private DisbursementTransaction costRecoveryFeeDisbursementTransaction;
    private CashReceipt cashReceipt;
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbursementTransaction credit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private CollectionType collectionType;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private Ocse34 ocse34;
    private DisbursementTransaction debit;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus disbursementStatus;
  }
#endregion
}

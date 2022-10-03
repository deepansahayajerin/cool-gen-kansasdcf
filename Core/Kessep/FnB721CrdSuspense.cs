// Program: FN_B721_CRD_SUSPENSE, ID: 371194168, model: 746.
// Short name: SWE02060
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_CRD_SUSPENSE.
/// </summary>
[Serializable]
public partial class FnB721CrdSuspense: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_CRD_SUSPENSE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721CrdSuspense(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721CrdSuspense.
  /// </summary>
  public FnB721CrdSuspense(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------------------
    //  Date	  Developer	Request #	Description
    // --------  ------------	----------	
    // -----------------------------------------------------------
    // 12/02/03  GVandy	WR040134	Initial Development
    // 02/24/05  CMJ				Aging logic
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of
    // 					   the OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // 					4) Only extract cash receipt details in suspense on the
    // 					   last day of the quarter.  Was previously extracting
    // 					   cash receipt details in suspense on the report run date.
    // 					5) Include if cash receipt detail collection date is within
    // 					   the quarter instead of cash receipt receipt date.
    // 					6) Suspense discontinue date must be greater than end of
    // 					   reporting period, not greater or equal to.
    // 					7) Code around FDSO adjustment issue which sets suspense
    // 					   CRD status history discontinue date to CRD collection
    // 					   date instead of current processing date.
    // 					8) Skip if Collection Type is not F,S,U,I,A,C,K,T,V,Y or Z.
    // 					9) Calculate amount in suspense at the end of the quarter.
    // 					10) Don't report an amount larger than the collection
    // 					    amount.
    // 02/07/12  GVandy	CQ31906		Calculate the amount of joint filer FDSO money 
    // in
    // 					Pended status with reason code "Research".  This money
    // 					will now be included in Part 2 Line 4 (Collections
    // 					from Tax Offsets being held for up to six months).
    // ---------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 6;

    // -------------------------------------------------------------------------------------------
    // -- Open CSE Cash Receipt Detail Suspense error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "OPEN";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Header Records.
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
            1, 39) + "...CASH RECEIPT DETAIL SUSPENSE ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 OBLIGOR #  CASH RCPT ID  CRD ID   COLL DT TYPE  AMOUNT     AGE";
            

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

    // cmj 02/24/2005 added calculations for dates for aging
    local.Gt2DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -1);
    local.Gt30DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -29);
    local.Gt180DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -181);
    local.Gt365DayDate.Date = AddDays(import.ReportingPeriodEndDate.Date, -364);
    local.Gt1095DayDate.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1094);
    local.Gt1825DayDate.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1824);

    // -------------------------------------------------------------------------------------------
    // -- Read each Suspended (or pended) Cash Receipt Detail.
    // -------------------------------------------------------------------------------------------
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.ReportingPeriodStartDat.Date =
      AddMonths(AddDays(import.ReportingPeriodEndDate.Date, 1), -3);

    foreach(var item in ReadCashReceiptDetailCashReceipt())
    {
      local.ForCreate.Comment = "";

      // -- CQ486  <<START>>  Code around FDSO adjustment problem...
      local.Previous.CreatedTimestamp = local.Null1.Timestamp;
      local.Suspense.Flag = "N";

      foreach(var item1 in ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
        
      {
        if (Equal(local.Previous.CreatedTimestamp, local.Null1.Timestamp))
        {
          local.Previous.CreatedTimestamp = entities.Other.CreatedTimestamp;
        }
        else if (!Equal(entities.Other.CreatedTimestamp,
          local.Previous.CreatedTimestamp))
        {
          // -- We've exhausted all the status records created for the most 
          // recent day this CRD was processed.
          break;
        }

        if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == 3 || entities
          .CashReceiptDetailStatus.SystemGeneratedIdentifier == 7)
        {
          local.Suspense.Flag = "Y";

          break;
        }
      }

      if (AsChar(local.Suspense.Flag) == 'N')
      {
        // -- Not in suspense at the end of the quarter.
        continue;
      }

      // -- CQ486  <<END>> Code around FDSO adjustment problem...
      // -- CQ486  Added for Issue #30
      // *** Skip if Coll Type is not F,S,U,I,A,C,K,T,V,Y or Z.
      if (ReadCollectionType())
      {
        if (entities.CollectionType.SequentialIdentifier == 1 || entities
          .CollectionType.SequentialIdentifier == 3 || entities
          .CollectionType.SequentialIdentifier == 4 || entities
          .CollectionType.SequentialIdentifier == 5 || entities
          .CollectionType.SequentialIdentifier == 6 || entities
          .CollectionType.SequentialIdentifier == 10 || entities
          .CollectionType.SequentialIdentifier == 15 || entities
          .CollectionType.SequentialIdentifier == 19 || entities
          .CollectionType.SequentialIdentifier == 23 || entities
          .CollectionType.SequentialIdentifier == 25 || entities
          .CollectionType.SequentialIdentifier == 26)
        {
        }
        else
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      // -- CQ486  Calculate the amount in suspense at the end of the quarter...
      local.UndistributedAmount.TotalCurrency =
        entities.CashReceiptDetail.CollectionAmount - entities
        .CashReceiptDetail.DistributedAmount.GetValueOrDefault() - entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault();

      foreach(var item1 in ReadReceiptRefund())
      {
        // -- Add back amounts refunded after the end of the quarter.
        local.UndistributedAmount.TotalCurrency += entities.ReceiptRefund.
          Amount;
      }

      foreach(var item1 in ReadCollection2())
      {
        // -- Add back amounts distributed after the end of the quarter.
        local.UndistributedAmount.TotalCurrency += entities.Collection.Amount;
      }

      foreach(var item1 in ReadCollection1())
      {
        // -- Subtract adjustment after the end of the quarter to collections 
        // created before the end of the quarter.
        local.UndistributedAmount.TotalCurrency -= entities.Collection.Amount;
      }

      // -- CQ486  (END) Calculate the amount in suspense at the end of the 
      // quarter...
      // -- CQ486  Added for Issue #29
      // -- If the amount we calculated as in suspense exceeds the cash receipt 
      // detail amount then only report the
      // -- cash receipt detail amount.  This can happen if following the end of
      // the reporting quarter the cash
      // -- receipt detail is BOTH refunded and adjusted.
      if (local.UndistributedAmount.TotalCurrency > entities
        .CashReceiptDetail.CollectionAmount)
      {
        local.UndistributedAmount.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount;
      }

      local.CrdStatus.Count = 0;

      foreach(var item1 in ReadCashReceiptDetailStatHistory())
      {
        ++local.CrdStatus.Count;

        switch(local.CrdStatus.Count)
        {
          case 1:
            // cmj 02/25/2005 note check for future reason and calculated the 
            // amount and count
            if (Equal(entities.CashReceiptDetailStatHistory.ReasonCodeId,
              "FUTURE"))
            {
              local.FutureSuspCredit.TotalCurrency += local.UndistributedAmount.
                TotalCurrency;
              ++local.FutureSuspCredit.Count;
              local.ForCreate.Comment = "B721 CRD SUSPENSE (FUTURE)";
            }
            else if (entities.CollectionType.SequentialIdentifier == 3 && AsChar
              (entities.CashReceiptDetail.JointReturnInd) == 'Y' && Equal
              (entities.CashReceiptDetailStatHistory.ReasonCodeId, "RESEARCH"))
            {
              // 02/07/12 GVandy  CQ31906  Calculate the amount of joint filer 
              // FDSO money in Pended status with reason code "Research".  This
              // money will now be included in Part 2 Line 4 (Collections from
              // Tax Offsets being held for up to six months).
              local.PendedJointFdso.TotalCurrency += local.UndistributedAmount.
                TotalCurrency;
              ++local.PendedJointFdso.Count;
              local.ForCreate.Comment = "B721 CRD SUSPENSE (FDSO RESEARCH)";
            }

            break;
          case 2:
            local.CompareDate.Date =
              AddDays(entities.CashReceiptDetailStatHistory.DiscontinueDate, 1);
              

            goto ReadEach;
          default:
            break;
        }
      }

ReadEach:

      // -- Increment running total amount and cash receipt detail count.
      ++local.TotalSuspendedCrd.Count;
      local.TotalSuspendedCrd.TotalCurrency += local.UndistributedAmount.
        TotalCurrency;

      // -- Convert the collection date to text
      local.DateWorkArea.TextDate =
        NumberToString(DateToInt(entities.CashReceiptDetail.CollectionDate), 8,
        8);

      // -- Convert the undistributed amount to text (Multiply * 100 then 
      // convert and insert a decimal point)
      local.TextWorkArea.Text30 =
        NumberToString((long)(local.UndistributedAmount.TotalCurrency * 100), 5,
        9) + "." + NumberToString
        ((long)(local.UndistributedAmount.TotalCurrency * 100), 14, 2);

      // -----------------------------------------------------------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <Obligor Number> <Cash Recipt Id> 
      // <Cash Receipt Detail Id> <Collection Date>  <Undistributed Amount>
      // -----------------------------------------------------------------------------------------------------------------------------------------------
      local.CompareTextDate.Text8 =
        NumberToString(DateToInt(local.CompareDate.Date), 8, 8);
      local.External.TextLine80 = "01 " + entities
        .CashReceiptDetail.ObligorPersonNumber;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        15) + NumberToString(entities.CashReceipt.SequentialNumber, 7, 9);
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        27) + NumberToString
        (entities.CashReceiptDetail.SequentialIdentifier, 7, 9);
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        37) + local.DateWorkArea.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        47) + entities.CollectionType.Code;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        50) + local.TextWorkArea.Text30;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        65) + local.CompareTextDate.Text8;

      if (entities.CashReceiptDetail.CollectionAmount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
      }

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
          local.ForCreate.Comment = "B721 CRD SUSPENSE (1)";
        }

        local.ForCreate.ObligorPersonNbr =
          entities.CashReceiptDetail.ObligorPersonNumber;
        local.ForCreate.CollectionDte =
          entities.CashReceiptDetail.CollectionDate;
        local.ForCreate.CollectionAmount =
          local.UndistributedAmount.TotalCurrency;

        // *** Build CRD #
        local.ForCreate.CaseWorkerName =
          NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
        local.ForCreate.CaseWorkerName =
          TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        UseFnCreateOcse157Verification();
      }

      if (Lt(local.CompareDate.Date, local.Gt1825DayDate.Date))
      {
        local.AgingGt1825.TotalCurrency += local.UndistributedAmount.
          TotalCurrency;
        ++local.AgingGt1825.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt1095DayDate.Date))
      {
        local.AgingGt1095.TotalCurrency += local.UndistributedAmount.
          TotalCurrency;
        ++local.AgingGt1095.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt365DayDate.Date))
      {
        local.AgingGt365.TotalCurrency += local.UndistributedAmount.
          TotalCurrency;
        ++local.AgingGt365.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt180DayDate.Date))
      {
        local.AgingGt180.TotalCurrency += local.UndistributedAmount.
          TotalCurrency;
        ++local.AgingGt180.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt30DayDate.Date))
      {
        local.AgingGt30.TotalCurrency += local.UndistributedAmount.
          TotalCurrency;
        ++local.AgingGt30.Count;

        continue;
      }

      if (Lt(local.CompareDate.Date, local.Gt2DayDate.Date))
      {
        local.AgingGt2.TotalCurrency += local.UndistributedAmount.TotalCurrency;
        ++local.AgingGt2.Count;

        continue;
      }

      local.AgingLastDaysBusiness.TotalCurrency += local.UndistributedAmount.
        TotalCurrency;
      ++local.AgingLastDaysBusiness.Count;
    }

    // -- Convert the total suspended amount to text (Multiply * 100 then 
    // convert and insert a decimal point)
    local.TextWorkArea.Text30 =
      NumberToString((long)(local.TotalSuspendedCrd.TotalCurrency * 100), 1, 13) +
      "." + NumberToString
      ((long)(local.TotalSuspendedCrd.TotalCurrency * 100), 14, 2);

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // CRD SUSPENSE AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
      (local.TotalSuspendedCrd.Count, 6, 10);
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 35) +
      "Total CRD Suspense Amount = ";
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 63) +
      local.TextWorkArea.Text30;

    if (local.TotalSuspendedCrd.TotalCurrency < 0)
    {
      local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
    }

    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Close CSE Cash Receipt Detail Suspense error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Total Cash Receipt Detail Suspense amount to the OCSE34 record.
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
    local.ProgramCheckpointRestart.RestartInfo = "07 " + Substring
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
          local.EabReportSend.RptDetail = "SUSPENDED CR DETAIL..." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 77);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Suspense Credits................Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrdLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrdLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrdGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrdGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrdGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrdGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrdGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrdGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrdGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrdGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.SuspendCrGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.SuspendCrGt1825, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 10:
          local.EabReportSend.RptDetail =
            "      Suspended for FUTURE reason..." + NumberToString
            (local.FutureSuspCredit.Count, 7, 9) + "......" + NumberToString
            ((long)local.FutureSuspCredit.TotalCurrency, 7, 9);

          if (local.FutureSuspCredit.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "      Joint Filer FDSO in RESEARCH.." + NumberToString
            (local.PendedJointFdso.Count, 7, 9) + "......" + NumberToString
            ((long)local.PendedJointFdso.TotalCurrency, 7, 9);

          if (local.PendedJointFdso.TotalCurrency < 0)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 12:
          local.EabReportSend.RptDetail = "";

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

  private IEnumerable<bool> ReadCashReceiptDetailCashReceipt()
  {
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetDate(
          command, "collectionDate",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate",
          local.ReportingPeriodStartDat.Date.GetValueOrDefault());
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
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 6);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 9);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 13);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 14);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;

    return ReadEach("ReadCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Other.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Other.CrdIdentifier = db.GetInt32(reader, 0);
        entities.Other.CrvIdentifier = db.GetInt32(reader, 1);
        entities.Other.CstIdentifier = db.GetInt32(reader, 2);
        entities.Other.CrtIdentifier = db.GetInt32(reader, 3);
        entities.Other.CdsIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.Other.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Other.DiscontinueDate = db.GetNullableDate(reader, 6);
        entities.Other.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDateTime(
          command, "createdTmst",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
        db.SetDate(
          command, "collAdjDt",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
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
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDateTime(
          command, "createdTmst",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.ConcurrentInd = db.GetString(reader, 2);
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
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 14);
        entities.Collection.Amount = db.GetDecimal(reader, 15);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("ConcurrentInd",
          entities.Collection.ConcurrentInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "collectionTypeId",
          entities.CashReceiptDetail.CltIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
        entities.Ocse34.CseCshRcptDtlSuspAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.SuspendCrdLda = db.GetNullableInt32(reader, 2);
        entities.Ocse34.SuspendCrdGt2 = db.GetNullableInt32(reader, 3);
        entities.Ocse34.SuspendCrdGt30 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.SuspendCrdGt180 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.SuspendCrdGt365 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.SuspendCrGt1095 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.SuspendCrGt1825 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.SuspCrdForFut = db.GetNullableInt32(reader, 9);
        entities.Ocse34.PendedJointFdso = db.GetNullableInt32(reader, 10);
        entities.Ocse34.Populated = true;
      });
  }

  private IEnumerable<bool> ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.ReceiptRefund.Populated = false;

    return ReadEach("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetNullableInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetNullableInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetNullableInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 1);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.Populated = true;

        return true;
      });
  }

  private void UpdateOcse34()
  {
    var cseCshRcptDtlSuspAmt = (int?)local.TotalSuspendedCrd.TotalCurrency;
    var suspendCrdLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var suspendCrdGt2 = (int?)local.AgingGt2.TotalCurrency;
    var suspendCrdGt30 = (int?)local.AgingGt30.TotalCurrency;
    var suspendCrdGt180 = (int?)local.AgingGt180.TotalCurrency;
    var suspendCrdGt365 = (int?)local.AgingGt365.TotalCurrency;
    var suspendCrGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var suspendCrGt1825 = (int?)local.AgingGt1825.TotalCurrency;
    var suspCrdForFut = (int?)local.FutureSuspCredit.TotalCurrency;
    var pendedJointFdso = (int?)local.PendedJointFdso.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cseCrdSuspAmt", cseCshRcptDtlSuspAmt);
        db.SetNullableInt32(command, "suspendCrdLda", suspendCrdLda);
        db.SetNullableInt32(command, "suspendCrdGt2", suspendCrdGt2);
        db.SetNullableInt32(command, "suspendCrdGt30", suspendCrdGt30);
        db.SetNullableInt32(command, "suspendCrdGt180", suspendCrdGt180);
        db.SetNullableInt32(command, "suspendCrdGt365", suspendCrdGt365);
        db.SetNullableInt32(command, "suspendCrGt1095", suspendCrGt1095);
        db.SetNullableInt32(command, "suspendCrGt1825", suspendCrGt1825);
        db.SetNullableInt32(command, "suspCrdForFut", suspCrdForFut);
        db.SetNullableInt32(command, "pendedJointFdso", pendedJointFdso);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CseCshRcptDtlSuspAmt = cseCshRcptDtlSuspAmt;
    entities.Ocse34.SuspendCrdLda = suspendCrdLda;
    entities.Ocse34.SuspendCrdGt2 = suspendCrdGt2;
    entities.Ocse34.SuspendCrdGt30 = suspendCrdGt30;
    entities.Ocse34.SuspendCrdGt180 = suspendCrdGt180;
    entities.Ocse34.SuspendCrdGt365 = suspendCrdGt365;
    entities.Ocse34.SuspendCrGt1095 = suspendCrGt1095;
    entities.Ocse34.SuspendCrGt1825 = suspendCrGt1825;
    entities.Ocse34.SuspCrdForFut = suspCrdForFut;
    entities.Ocse34.PendedJointFdso = pendedJointFdso;
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
    /// A value of PendedJointFdso.
    /// </summary>
    [JsonPropertyName("pendedJointFdso")]
    public Common PendedJointFdso
    {
      get => pendedJointFdso ??= new();
      set => pendedJointFdso = value;
    }

    /// <summary>
    /// A value of Suspense.
    /// </summary>
    [JsonPropertyName("suspense")]
    public Common Suspense
    {
      get => suspense ??= new();
      set => suspense = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CashReceiptDetailStatHistory Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of ReportingPeriodStartDat.
    /// </summary>
    [JsonPropertyName("reportingPeriodStartDat")]
    public DateWorkArea ReportingPeriodStartDat
    {
      get => reportingPeriodStartDat ??= new();
      set => reportingPeriodStartDat = value;
    }

    /// <summary>
    /// A value of CompareTextDate.
    /// </summary>
    [JsonPropertyName("compareTextDate")]
    public TextWorkArea CompareTextDate
    {
      get => compareTextDate ??= new();
      set => compareTextDate = value;
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
    /// A value of FutureSuspCredit.
    /// </summary>
    [JsonPropertyName("futureSuspCredit")]
    public Common FutureSuspCredit
    {
      get => futureSuspCredit ??= new();
      set => futureSuspCredit = value;
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
    /// A value of UndistributedAmount.
    /// </summary>
    [JsonPropertyName("undistributedAmount")]
    public Common UndistributedAmount
    {
      get => undistributedAmount ??= new();
      set => undistributedAmount = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of TotalSuspendedCrd.
    /// </summary>
    [JsonPropertyName("totalSuspendedCrd")]
    public Common TotalSuspendedCrd
    {
      get => totalSuspendedCrd ??= new();
      set => totalSuspendedCrd = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of CrdStatus.
    /// </summary>
    [JsonPropertyName("crdStatus")]
    public Common CrdStatus
    {
      get => crdStatus ??= new();
      set => crdStatus = value;
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

    private Common pendedJointFdso;
    private Common suspense;
    private DateWorkArea null1;
    private CashReceiptDetailStatHistory previous;
    private DateWorkArea reportingPeriodStartDat;
    private TextWorkArea compareTextDate;
    private DateWorkArea gt1825DayDate;
    private DateWorkArea gt1095DayDate;
    private DateWorkArea gt365DayDate;
    private DateWorkArea gt180DayDate;
    private DateWorkArea gt30DayDate;
    private DateWorkArea gt2DayDate;
    private Common agingGt1825;
    private Common agingGt1095;
    private Common agingGt365;
    private Common agingGt180;
    private Common agingGt30;
    private Common agingGt2;
    private Common agingLastDaysBusiness;
    private Common futureSuspCredit;
    private DateWorkArea compareDate;
    private Common undistributedAmount;
    private DateWorkArea maxDate;
    private Common fileNumber;
    private External external;
    private DateWorkArea dateWorkArea;
    private Common totalSuspendedCrd;
    private TextWorkArea textWorkArea;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common common;
    private Common crdStatus;
    private Ocse157Verification forCreate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public CashReceiptDetailStatHistory Other
    {
      get => other ??= new();
      set => other = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private CashReceiptDetailStatHistory other;
    private Collection collection;
    private ReceiptRefund receiptRefund;
    private CollectionType collectionType;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Ocse34 ocse34;
  }
#endregion
}

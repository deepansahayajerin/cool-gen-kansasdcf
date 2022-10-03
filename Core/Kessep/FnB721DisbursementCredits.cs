// Program: FN_B721_DISBURSEMENT_CREDITS, ID: 371194909, model: 746.
// Short name: SWE02055
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_DISBURSEMENT_CREDITS.
/// </summary>
[Serializable]
public partial class FnB721DisbursementCredits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_DISBURSEMENT_CREDITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721DisbursementCredits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721DisbursementCredits.
  /// </summary>
  public FnB721DisbursementCredits(IContext context, Import import,
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
    // 02/10/04  CMJ				Aging logic
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // 					4) Skip collections created and adjusted during the quarter.
    // -----------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 1;

    // -------------------------------------------------------------------------------------------
    // -- Open CSE Disbursement credit error interface file.
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
          local.Collection.TextDate =
            NumberToString(DateToInt(import.ReportingPeriodEndDate.Date), 8, 8);
            
          local.External.TextLine80 = "00 REPORTING PERIOD END DATE = " + local
            .Collection.TextDate;
          local.External.TextLine80 =
            Substring(local.External.TextLine80, External.TextLine80_MaxLength,
            1, 39) + "...DISBURSEMENT CREDIT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 OBLIGOR # OB#OB TYPE DUE DT COLL AMOUNT PGM COLL DT CR#     CRD# SUPPORTED  AGE DATE";
            

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

    // cmj 02/10/2004 calculate the dates for the aging process
    local.Local2Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1);
    local.Local30Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -29);
    local.Local180Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -181);
    local.Local365Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -364);
    local.Local1095Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1094);
    local.Local1825Date.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1824);

    // -------------------------------------------------------------------------------------------
    // -- Read each disbursement credit error.
    // -------------------------------------------------------------------------------------------
    foreach(var item in ReadCollectionCashReceiptDetailCashReceiptObligation())
    {
      // -- CQ486  Skip the collection if it was created and adjusted in the 
      // same quarter.
      if (AsChar(entities.Collection.AdjustedInd) == 'Y')
      {
        if (Year(entities.Collection.CollectionAdjustmentDt) == Year
          (entities.Collection.CreatedTmst))
        {
          local.CollectionMonth.Count = Month(entities.Collection.CreatedTmst);
          local.CollAdjustmentMonth.Count =
            Month(entities.Collection.CollectionAdjustmentDt);

          if (local.CollectionMonth.Count >= 1 && local
            .CollectionMonth.Count <= 3)
          {
            if (local.CollAdjustmentMonth.Count >= 1 && local
              .CollAdjustmentMonth.Count <= 3)
            {
              continue;
            }
          }
          else if (local.CollectionMonth.Count >= 4 && local
            .CollectionMonth.Count <= 6)
          {
            if (local.CollAdjustmentMonth.Count >= 4 && local
              .CollAdjustmentMonth.Count <= 6)
            {
              continue;
            }
          }
          else if (local.CollectionMonth.Count >= 7 && local
            .CollectionMonth.Count <= 9)
          {
            if (local.CollAdjustmentMonth.Count >= 7 && local
              .CollAdjustmentMonth.Count <= 9)
            {
              continue;
            }
          }
          else if (local.CollectionMonth.Count >= 10 && local
            .CollectionMonth.Count <= 12)
          {
            if (local.CollAdjustmentMonth.Count >= 10 && local
              .CollAdjustmentMonth.Count <= 12)
            {
              continue;
            }
          }
        }
      }

      if (!ReadObligationType())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      if (!ReadDebtDetail())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      if (!ReadCsePerson())
      {
        // -- This shouldn't happen....skip this collection if not found.
        continue;
      }

      // -- Increment running total amount and disbursement credit count.
      ++local.TotalDisbursementCredit.Count;
      local.TotalDisbursementCredit.TotalCurrency += entities.Collection.Amount;
      local.Collection.TextDate =
        NumberToString(DateToInt(entities.Collection.CollectionDt), 8, 8);

      // collection timestamp date to date because of aging with collection 
      // timestamp
      local.Age.TextDate =
        NumberToString(DateToInt(Date(entities.Collection.CreatedTmst)), 8, 8);

      // -- Convert the debt detail due date to text
      local.DebtDetailDueDate.TextDate =
        NumberToString(DateToInt(entities.DebtDetail.DueDt), 8, 8);

      // -- Convert the collection amount to text (Multiply * 100 then convert 
      // and insert a decimal point)
      local.TextWorkArea.Text30 =
        NumberToString((long)(entities.Collection.Amount * 100), 5, 9) + "." + NumberToString
        ((long)(entities.Collection.Amount * 100), 14, 2);

      // -- Increment running total amount and disbursement credit count.
      // -------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <Obligor #><Obligation #><Obligation
      // Type><Due Date><Collection Amount><Program Applied To><Collection
      // Date><Cash Receipt #><Cash Receipt Detail #><Supported Person #><age
      // date>
      // -------------------------------------------------------------------------------------------
      local.External.TextLine80 = "01 " + entities.Obligor1.Number;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        13) + NumberToString
        (entities.Obligation.SystemGeneratedIdentifier, 13, 3);
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        16) + entities.ObligationType.Code;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        23) + local.DebtDetailDueDate.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        31) + local.TextWorkArea.Text30;

      if (entities.Collection.Amount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
      }

      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        44) + entities.Collection.ProgramAppliedTo;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        47) + local.Collection.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        55) + NumberToString(entities.CashReceipt.SequentialNumber, 7, 9);
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        64) + NumberToString
        (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        68) + entities.Supported2.Number;

      // move date of the timestamp to audit report for reference   4/07/2005 
      // cmj
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        80) + local.Age.TextDate;
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (AsChar(import.Audit.Flag) == 'Y')
      {
        // -- Log the amount to the audit trail.
        local.ForCreate.FiscalYear =
          import.Ocse157Verification.FiscalYear.GetValueOrDefault();
        local.ForCreate.RunNumber =
          import.Ocse157Verification.RunNumber.GetValueOrDefault();
        local.ForCreate.LineNumber = "B13";
        local.ForCreate.Comment = "B721 DISBURSEMENT CREDITS (1)";
        local.ForCreate.ObligorPersonNbr = entities.Obligor1.Number;
        local.ForCreate.CollectionAmount = entities.Collection.Amount;
        local.ForCreate.CollectionDte = entities.Collection.CollectionDt;

        // *** Build CRD #
        local.ForCreate.CaseWorkerName =
          NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
        local.ForCreate.CaseWorkerName =
          TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        UseFnCreateOcse157Verification();
      }

      // cmj 2/10/2004  sets compare date from the collection created_tmst to do
      // aging
      local.CollectionCreated.Date = Date(entities.Collection.CreatedTmst);

      // cmj 2/10/2004  calculate the aging totals
      if (Lt(local.CollectionCreated.Date, local.Local1825Date.Date))
      {
        local.AgingGt1825.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt1825.Count;

        continue;
      }

      if (Lt(local.CollectionCreated.Date, local.Local1095Date.Date))
      {
        local.AgingGt1095.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt1095.Count;

        continue;
      }

      if (Lt(local.CollectionCreated.Date, local.Local365Date.Date))
      {
        local.AgingGt365.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt365.Count;

        continue;
      }

      if (Lt(local.CollectionCreated.Date, local.Local180Date.Date))
      {
        local.AgingGt180.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt180.Count;

        continue;
      }

      if (Lt(local.CollectionCreated.Date, local.Local30Date.Date))
      {
        local.AgingGt30.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt30.Count;

        continue;
      }

      if (Lt(local.CollectionCreated.Date, local.Local2Date.Date))
      {
        local.AgingGt2.TotalCurrency += entities.Collection.Amount;
        ++local.AgingGt2.Count;

        continue;
      }

      ++local.AgingLastDaysBusiness.Count;
      local.AgingLastDaysBusiness.TotalCurrency += entities.Collection.Amount;
    }

    // -- Convert the total disbursement credit amount to text (Multiply * 100 
    // then convert and insert a decimal point)
    local.TextWorkArea.Text30 =
      NumberToString((long)(local.TotalDisbursementCredit.TotalCurrency * 100),
      1, 13) + "." + NumberToString
      ((long)(local.TotalDisbursementCredit.TotalCurrency * 100), 14, 2);

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // DISB CREDIT AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
      (local.TotalDisbursementCredit.Count, 6, 10);
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 35) +
      "Total Disb Credit Amount = ";
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 63) +
      local.TextWorkArea.Text30;

    if (local.TotalDisbursementCredit.TotalCurrency < 0)
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
    // -- Close CSE Disbursement credit error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Total Disbursement credit amount to the OCSE34 record.
    // -------------------------------------------------------------------------------------------
    // cmj 02/11/2005  need to add the data here for the ocse34 new table 
    // attributes.
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
    local.ProgramCheckpointRestart.RestartInfo = "02 " + Substring
      (local.ProgramCheckpointRestart.RestartInfo, 250, 4, 247);
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write totals to the control report.
    // -------------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 10; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail = "DISBURSEMENT CREDITS.." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 76);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Disbursement Credit Totals......Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCreditLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCreditLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCreditGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCreditGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCreditGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCreditGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCreditGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCreditGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCreditGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCreditGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCrdGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCrdGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbCrdGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbCrdGt1825, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 10:
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

  private IEnumerable<bool>
    ReadCollectionCashReceiptDetailCashReceiptObligation()
  {
    entities.Obligor1.Populated = false;
    entities.Obligation.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCashReceiptDetailCashReceiptObligation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "disbDt1",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "disbDt2", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.DisbursementDt = db.GetNullableDate(reader, 2);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 3);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 4);
        entities.Collection.CrtType = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 5);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 5);
        entities.Collection.CstId = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 6);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 6);
        entities.Collection.CrvId = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 7);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 7);
        entities.Collection.CrdId = db.GetInt32(reader, 8);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 9);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 9);
        entities.Collection.CspNumber = db.GetString(reader, 10);
        entities.Obligation.CspNumber = db.GetString(reader, 10);
        entities.Obligor1.Number = db.GetString(reader, 10);
        entities.Obligor1.Number = db.GetString(reader, 10);
        entities.Collection.CpaType = db.GetString(reader, 11);
        entities.Obligation.CpaType = db.GetString(reader, 11);
        entities.Collection.OtrId = db.GetInt32(reader, 12);
        entities.Collection.OtrType = db.GetString(reader, 13);
        entities.Collection.OtyId = db.GetInt32(reader, 14);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 14);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 15);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 16);
        entities.Collection.Amount = db.GetDecimal(reader, 17);
        entities.Collection.DisbursementProcessingNeedInd =
          db.GetNullableString(reader, 18);
        entities.Collection.ProgramAppliedTo = db.GetString(reader, 19);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 20);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 21);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 22);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 23);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 24);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 25);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 26);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 27);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 28);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 29);
        entities.Obligor1.Populated = true;
        entities.Obligation.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
        CheckValid<Collection>("DisbursementProcessingNeedInd",
          entities.Collection.DisbursementProcessingNeedInd);
        CheckValid<Collection>("ProgramAppliedTo",
          entities.Collection.ProgramAppliedTo);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.Supported2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Collection.OtyId);
        db.SetString(command, "obTrnTyp", entities.Collection.OtrType);
        db.SetInt32(command, "obTrnId", entities.Collection.OtrId);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetInt32(command, "obgGeneratedId", entities.Collection.ObgId);
      },
      (db, reader) =>
      {
        entities.Supported2.Number = db.GetString(reader, 0);
        entities.Supported2.Populated = true;
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

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", entities.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
        entities.Ocse34.CseDisbCreditAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.DisbCreditLda = db.GetNullableInt32(reader, 2);
        entities.Ocse34.DisbCreditGt2 = db.GetNullableInt32(reader, 3);
        entities.Ocse34.DisbCreditGt30 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.DisbCreditGt180 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.DisbCreditGt365 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.DisbCrdGt1095 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.DisbCrdGt1825 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse34()
  {
    var cseDisbCreditAmt = (int?)local.TotalDisbursementCredit.TotalCurrency;
    var disbCreditLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var disbCreditGt2 = (int?)local.AgingGt2.TotalCurrency;
    var disbCreditGt30 = (int?)local.AgingGt30.TotalCurrency;
    var disbCreditGt180 = (int?)local.AgingGt180.TotalCurrency;
    var disbCreditGt365 = (int?)local.AgingGt365.TotalCurrency;
    var disbCrdGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var disbCrdGt1825 = (int?)local.AgingGt1825.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cseDsbCrdtAmt", cseDisbCreditAmt);
        db.SetNullableInt32(command, "disbCreditLda", disbCreditLda);
        db.SetNullableInt32(command, "disbCreditGt2", disbCreditGt2);
        db.SetNullableInt32(command, "disbCreditGt30", disbCreditGt30);
        db.SetNullableInt32(command, "disbCreditGt180", disbCreditGt180);
        db.SetNullableInt32(command, "disbCreditGt365", disbCreditGt365);
        db.SetNullableInt32(command, "disbCrdGt1095", disbCrdGt1095);
        db.SetNullableInt32(command, "disbCrdGt1825", disbCrdGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CseDisbCreditAmt = cseDisbCreditAmt;
    entities.Ocse34.DisbCreditLda = disbCreditLda;
    entities.Ocse34.DisbCreditGt2 = disbCreditGt2;
    entities.Ocse34.DisbCreditGt30 = disbCreditGt30;
    entities.Ocse34.DisbCreditGt180 = disbCreditGt180;
    entities.Ocse34.DisbCreditGt365 = disbCreditGt365;
    entities.Ocse34.DisbCrdGt1095 = disbCrdGt1095;
    entities.Ocse34.DisbCrdGt1825 = disbCrdGt1825;
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
    /// A value of CollAdjustmentMonth.
    /// </summary>
    [JsonPropertyName("collAdjustmentMonth")]
    public Common CollAdjustmentMonth
    {
      get => collAdjustmentMonth ??= new();
      set => collAdjustmentMonth = value;
    }

    /// <summary>
    /// A value of CollectionMonth.
    /// </summary>
    [JsonPropertyName("collectionMonth")]
    public Common CollectionMonth
    {
      get => collectionMonth ??= new();
      set => collectionMonth = value;
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
    /// A value of Age.
    /// </summary>
    [JsonPropertyName("age")]
    public DateWorkArea Age
    {
      get => age ??= new();
      set => age = value;
    }

    /// <summary>
    /// A value of Local1825Date.
    /// </summary>
    [JsonPropertyName("local1825Date")]
    public DateWorkArea Local1825Date
    {
      get => local1825Date ??= new();
      set => local1825Date = value;
    }

    /// <summary>
    /// A value of Local1095Date.
    /// </summary>
    [JsonPropertyName("local1095Date")]
    public DateWorkArea Local1095Date
    {
      get => local1095Date ??= new();
      set => local1095Date = value;
    }

    /// <summary>
    /// A value of Local365Date.
    /// </summary>
    [JsonPropertyName("local365Date")]
    public DateWorkArea Local365Date
    {
      get => local365Date ??= new();
      set => local365Date = value;
    }

    /// <summary>
    /// A value of Local180Date.
    /// </summary>
    [JsonPropertyName("local180Date")]
    public DateWorkArea Local180Date
    {
      get => local180Date ??= new();
      set => local180Date = value;
    }

    /// <summary>
    /// A value of Local30Date.
    /// </summary>
    [JsonPropertyName("local30Date")]
    public DateWorkArea Local30Date
    {
      get => local30Date ??= new();
      set => local30Date = value;
    }

    /// <summary>
    /// A value of Local2Date.
    /// </summary>
    [JsonPropertyName("local2Date")]
    public DateWorkArea Local2Date
    {
      get => local2Date ??= new();
      set => local2Date = value;
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
    /// A value of CollectionCreated.
    /// </summary>
    [JsonPropertyName("collectionCreated")]
    public DateWorkArea CollectionCreated
    {
      get => collectionCreated ??= new();
      set => collectionCreated = value;
    }

    /// <summary>
    /// A value of DebtDetailDueDate.
    /// </summary>
    [JsonPropertyName("debtDetailDueDate")]
    public DateWorkArea DebtDetailDueDate
    {
      get => debtDetailDueDate ??= new();
      set => debtDetailDueDate = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public DateWorkArea Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of TotalDisbursementCredit.
    /// </summary>
    [JsonPropertyName("totalDisbursementCredit")]
    public Common TotalDisbursementCredit
    {
      get => totalDisbursementCredit ??= new();
      set => totalDisbursementCredit = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

    private Common collAdjustmentMonth;
    private Common collectionMonth;
    private Common common;
    private DateWorkArea age;
    private DateWorkArea local1825Date;
    private DateWorkArea local1095Date;
    private DateWorkArea local365Date;
    private DateWorkArea local180Date;
    private DateWorkArea local30Date;
    private DateWorkArea local2Date;
    private Common agingGt1825;
    private Common agingGt1095;
    private Common agingGt365;
    private Common agingGt180;
    private Common agingGt30;
    private Common agingGt2;
    private Common agingLastDaysBusiness;
    private DateWorkArea collectionCreated;
    private DateWorkArea debtDetailDueDate;
    private Common fileNumber;
    private External external;
    private DateWorkArea collection;
    private Common totalDisbursementCredit;
    private TextWorkArea textWorkArea;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Ocse157Verification forCreate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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

    private CsePersonAccount supported1;
    private CsePerson supported2;
    private DebtDetail debtDetail;
    private ObligationType obligationType;
    private CsePerson obligor1;
    private CsePersonAccount obligor2;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private Ocse34 ocse34;
  }
#endregion
}

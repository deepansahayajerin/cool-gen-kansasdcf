// Program: FN_B721_PAYMENTS, ID: 371197014, model: 746.
// Short name: SWE02058
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_PAYMENTS.
/// </summary>
[Serializable]
public partial class FnB721Payments: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_PAYMENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721Payments(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721Payments.
  /// </summary>
  public FnB721Payments(IContext context, Import import, Export export):
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
    // 02/22/05  CMJ				Aging logic
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // -----------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 4;

    // -------------------------------------------------------------------------------------------
    // -- Open CSE Payment error interface file.
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
            1, 39) + "...PAYMENT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 PAYEE NUMBER    AMOUNT    PROCESS DATE  PAYMENT REQUEST ID   AGE DATE";
            

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

    // cmj 02/22/2005 compute the dates for aging calculations
    local.Local2Date.Date = AddDays(import.ReportingPeriodEndDate.Date, -1);
    local.Local30Date.Date = AddDays(import.ReportingPeriodEndDate.Date, -29);
    local.Local180Date.Date = AddDays(import.ReportingPeriodEndDate.Date, -181);
    local.Local365Date.Date = AddDays(import.ReportingPeriodEndDate.Date, -364);
    local.Local1095Date.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1094);
    local.Local1825Date.Date =
      AddDays(import.ReportingPeriodEndDate.Date, -1824);

    // -------------------------------------------------------------------------------------------
    // -- Read each payment error.
    // -------------------------------------------------------------------------------------------
    local.Local20990101.Date = new DateTime(2099, 1, 1);

    foreach(var item in ReadDisbursementTransactionDisbursementStatusHistory())
    {
      // -- Increment running total amount and payment count.
      ++local.TotalPayment.Count;
      local.TotalPayment.TotalCurrency += entities.DisbursementTransaction.
        Amount;

      // -- Convert the payment process date to text
      local.DateWorkArea.TextDate =
        NumberToString(DateToInt(entities.DisbursementTransaction.ProcessDate),
        8, 8);

      // convert the age date to later be added to audit file for reference 04/
      // 07/2005 cmj
      local.Age.TextDate =
        NumberToString(DateToInt(
          entities.DisbursementStatusHistory.EffectiveDate), 8, 8);

      // -- Convert the payment amount to text (Multiply * 100 then convert and 
      // insert a decimal point)
      local.TextWorkArea.Text30 =
        NumberToString((long)(entities.DisbursementTransaction.Amount * 100), 5,
        9) + "." + NumberToString
        ((long)(entities.DisbursementTransaction.Amount * 100), 14, 2);

      // -------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <Payee Number> <Amount> <Process 
      // Date> <Payment Request ID> <Disbursement Status History Effective Date>
      // -------------------------------------------------------------------------------------------
      local.External.TextLine80 = "01 " + entities
        .DisbursementTransaction.DesignatedPayee;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        16) + local.TextWorkArea.Text30;

      if (entities.DisbursementTransaction.Amount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
      }

      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        31) + local.DateWorkArea.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        45) + entities.DisbursementTransaction.ReferenceNumber;

      // added to audit file for aging date reference 4/07/2005 cmj
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        60) + local.Age.TextDate;
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
        local.ForCreate.Comment = "B721 PAYMENTS (1)";
        local.ForCreate.ObligorPersonNbr =
          entities.DisbursementTransaction.DesignatedPayee;
        local.ForCreate.CollectionDte =
          entities.DisbursementTransaction.ProcessDate;
        local.ForCreate.CollectionAmount =
          entities.DisbursementTransaction.Amount;

        if (ReadDisbCollection())
        {
          if (ReadCashReceiptCashReceiptDetail())
          {
            // *** Build CRD #
            local.ForCreate.CaseWorkerName =
              NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-"
              ;
            local.ForCreate.CaseWorkerName =
              TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
              (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
          }
          else
          {
            local.ForCreate.CaseWorkerName = "";
          }
        }
        else
        {
          local.ForCreate.CaseWorkerName = "";
        }

        UseFnCreateOcse157Verification();
      }

      // cmj 02/22/2005 set the compare date to the effective date
      local.DisbStatusEffectiveDate.Date =
        entities.DisbursementStatusHistory.EffectiveDate;

      // cmj 02/22/2005 calculate the aging monies and count the details
      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local1825Date.Date))
      {
        local.AgingGt1825.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt1825.Count;

        continue;
      }

      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local1095Date.Date))
      {
        local.AgingGt1095.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt1095.Count;

        continue;
      }

      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local365Date.Date))
      {
        local.AgingGt365.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt365.Count;

        continue;
      }

      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local180Date.Date))
      {
        local.AgingGt180.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt180.Count;

        continue;
      }

      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local30Date.Date))
      {
        local.AgingGt30.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt30.Count;

        continue;
      }

      if (Lt(local.DisbStatusEffectiveDate.Date, local.Local2Date.Date))
      {
        local.AgingGt2.TotalCurrency += entities.DisbursementTransaction.Amount;
        ++local.AgingGt2.Count;

        continue;
      }

      local.AgingLastDaysBusiness.TotalCurrency += entities.
        DisbursementTransaction.Amount;
      ++local.AgingLastDaysBusiness.Count;
    }

    // -- Convert the total payment amount to text (Multiply * 100 then convert 
    // and insert a decimal point)
    local.TextWorkArea.Text30 =
      NumberToString((long)(local.TotalPayment.TotalCurrency * 100), 1, 13) + "."
      + NumberToString((long)(local.TotalPayment.TotalCurrency * 100), 14, 2);

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // PAYMENT AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
      (local.TotalPayment.Count, 6, 10);
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 35) +
      "Total Payment Amount = ";
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 58) +
      local.TextWorkArea.Text30;

    if (local.TotalPayment.TotalCurrency < 0)
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
    // -- Close CSE payment error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Total Payment amount to the OCSE34 record.
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
    local.ProgramCheckpointRestart.RestartInfo = "05 " + Substring
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
          local.EabReportSend.RptDetail = "PAYMENTS.............." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 76);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Payments........................Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.PayErrorGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.PayErrorGt1825, 0))
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

  private bool ReadCashReceiptCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crvId", entities.DisbCollection.CrvId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "cstId", entities.DisbCollection.CstId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crtId", entities.DisbCollection.CrtId.GetValueOrDefault());
        db.SetNullableInt32(
          command, "crdId", entities.DisbCollection.CrdId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 7);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadDisbCollection()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbCollection.Populated = false;

    return Read("ReadDisbCollection",
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
        entities.DisbCollection.CpaType = db.GetString(reader, 0);
        entities.DisbCollection.CspNumber = db.GetString(reader, 1);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 3);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 4);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 5);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 6);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 7);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 8);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 9);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 10);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 11);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 12);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 13);
        entities.DisbCollection.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbCollection.CpaType);
        CheckValid<DisbursementTransaction>("OtrTypeDisb",
          entities.DisbCollection.OtrTypeDisb);
        CheckValid<DisbursementTransaction>("CpaTypeDisb",
          entities.DisbCollection.CpaTypeDisb);
      });
  }

  private IEnumerable<bool>
    ReadDisbursementTransactionDisbursementStatusHistory()
  {
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate", local.Local20990101.Date.GetValueOrDefault());
          
        db.SetDate(
          command, "effectiveDate",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CreatedBy = db.GetString(reader, 6);
        entities.DisbursementTransaction.InterstateInd =
          db.GetNullableString(reader, 7);
        entities.DisbursementTransaction.DesignatedPayee =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 10);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 12);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 13);
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);

        return true;
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
        entities.Ocse34.CsePaymentAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.PayErrorLda = db.GetNullableInt32(reader, 2);
        entities.Ocse34.PayErrorGt2 = db.GetNullableInt32(reader, 3);
        entities.Ocse34.PayErrorGt30 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.PayErrorGt180 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.PayErrorGt365 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.PayErrorGt1095 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.PayErrorGt1825 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse34()
  {
    var csePaymentAmt = (int?)local.TotalPayment.TotalCurrency;
    var payErrorLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var payErrorGt2 = (int?)local.AgingGt2.TotalCurrency;
    var payErrorGt30 = (int?)local.AgingGt30.TotalCurrency;
    var payErrorGt180 = (int?)local.AgingGt180.TotalCurrency;
    var payErrorGt365 = (int?)local.AgingGt365.TotalCurrency;
    var payErrorGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var payErrorGt1825 = (int?)local.AgingGt1825.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "csePymntAmt", csePaymentAmt);
        db.SetNullableInt32(command, "payErrorLda", payErrorLda);
        db.SetNullableInt32(command, "payErrorGt2", payErrorGt2);
        db.SetNullableInt32(command, "payErrorGt30", payErrorGt30);
        db.SetNullableInt32(command, "payErrorGt180", payErrorGt180);
        db.SetNullableInt32(command, "payErrorGt365", payErrorGt365);
        db.SetNullableInt32(command, "payErrorGt1095", payErrorGt1095);
        db.SetNullableInt32(command, "payErrorGt1825", payErrorGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CsePaymentAmt = csePaymentAmt;
    entities.Ocse34.PayErrorLda = payErrorLda;
    entities.Ocse34.PayErrorGt2 = payErrorGt2;
    entities.Ocse34.PayErrorGt30 = payErrorGt30;
    entities.Ocse34.PayErrorGt180 = payErrorGt180;
    entities.Ocse34.PayErrorGt365 = payErrorGt365;
    entities.Ocse34.PayErrorGt1095 = payErrorGt1095;
    entities.Ocse34.PayErrorGt1825 = payErrorGt1825;
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
    /// A value of Age.
    /// </summary>
    [JsonPropertyName("age")]
    public DateWorkArea Age
    {
      get => age ??= new();
      set => age = value;
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
    /// A value of AgingGt2.
    /// </summary>
    [JsonPropertyName("agingGt2")]
    public Common AgingGt2
    {
      get => agingGt2 ??= new();
      set => agingGt2 = value;
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
    /// A value of AgingGt180.
    /// </summary>
    [JsonPropertyName("agingGt180")]
    public Common AgingGt180
    {
      get => agingGt180 ??= new();
      set => agingGt180 = value;
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
    /// A value of AgingGt1095.
    /// </summary>
    [JsonPropertyName("agingGt1095")]
    public Common AgingGt1095
    {
      get => agingGt1095 ??= new();
      set => agingGt1095 = value;
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
    /// A value of Local2Date.
    /// </summary>
    [JsonPropertyName("local2Date")]
    public DateWorkArea Local2Date
    {
      get => local2Date ??= new();
      set => local2Date = value;
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
    /// A value of Local180Date.
    /// </summary>
    [JsonPropertyName("local180Date")]
    public DateWorkArea Local180Date
    {
      get => local180Date ??= new();
      set => local180Date = value;
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
    /// A value of Local1095Date.
    /// </summary>
    [JsonPropertyName("local1095Date")]
    public DateWorkArea Local1095Date
    {
      get => local1095Date ??= new();
      set => local1095Date = value;
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
    /// A value of DisbStatusEffectiveDate.
    /// </summary>
    [JsonPropertyName("disbStatusEffectiveDate")]
    public DateWorkArea DisbStatusEffectiveDate
    {
      get => disbStatusEffectiveDate ??= new();
      set => disbStatusEffectiveDate = value;
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
    /// A value of TotalPayment.
    /// </summary>
    [JsonPropertyName("totalPayment")]
    public Common TotalPayment
    {
      get => totalPayment ??= new();
      set => totalPayment = value;
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

    private DateWorkArea age;
    private Common agingLastDaysBusiness;
    private Common agingGt2;
    private Common agingGt30;
    private Common agingGt180;
    private Common agingGt365;
    private Common agingGt1095;
    private Common agingGt1825;
    private DateWorkArea local2Date;
    private DateWorkArea local30Date;
    private DateWorkArea local180Date;
    private DateWorkArea local365Date;
    private DateWorkArea local1095Date;
    private DateWorkArea local1825Date;
    private DateWorkArea disbStatusEffectiveDate;
    private DateWorkArea local20990101;
    private Common fileNumber;
    private External external;
    private DateWorkArea dateWorkArea;
    private Common totalPayment;
    private TextWorkArea textWorkArea;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
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

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbCollection;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private DisbursementStatus disbursementStatus;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private Ocse34 ocse34;
  }
#endregion
}

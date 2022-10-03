// Program: FN_B721_WARRANTS, ID: 371198256, model: 746.
// Short name: SWE02057
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_WARRANTS.
/// </summary>
[Serializable]
public partial class FnB721Warrants: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_WARRANTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721Warrants(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721Warrants.
  /// </summary>
  public FnB721Warrants(IContext context, Import import, Export export):
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
    // 02/15/05  CMJ    	WR040796	Aging logic
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // 					4) Include cost recovery fee in warrant amount.
    // 					5) Use print date instead of created timestamp to determine
    // 					   warrants included in the current quarter.
    // 					6) Exclude warrants for less than $1.00.  They are
    // 					   cancelled and not issued.
    // 					7) Do not include adjusted collections if the disbursement
    // 					   adjustment has not processed.
    // -----------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 3;

    // -------------------------------------------------------------------------------------------
    // -- Open CSE Warrant error interface file.
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
            1, 39) + "...WARRANT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 CSP NUMBER      AMOUNT     CREATED DATE  PAYMENT REQUEST ID";

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

    // cmj 02/15/2005 added the code to calculate the aging definitions.
    local.WarrGt2.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1);
    local.WarrGt30.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -29);
    local.WarrGt180.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -181);
    local.WarrGt365.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -364);
    local.WarrGt1095.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1094);
    local.WarrGt1825.Date =
      AddDays(Date(import.ReportingPeriodEndDate.Timestamp), -1824);

    // -------------------------------------------------------------------------------------------
    // -- Read each warrant error.
    // -------------------------------------------------------------------------------------------
    // CQ486  Correct where last days business processed after midnight and 
    // created a warrant with request date 09-30-2008.  Since the
    // created_timestamp was 10-01-2008... it was not picked up.  Use print_date
    // instead to determine if the warrant actually printed within the quarter.
    // CQ486  Removed check that payment request created timestamp was less than
    // the end of the quarter and
    //     added check that a payment status history records exists with an 
    // effective date less than the end of the quarter.
    // CQ486 Issue #28.  Include warrants for exactly $1.00.  Was previously on 
    // reading warrants greater than $1.00.
    foreach(var item in ReadPaymentRequest())
    {
      local.WarrPlusCrf.Amount = 0;

      // CQ486  Don't include adjusted collection amounts if the disbursement 
      // adjustment has not processed.  This money will be counted in suspense.
      foreach(var item1 in ReadDisbCollectionDisbursementCollection())
      {
        // CQ486  Skip if collection is adjusted and the disbursement adjustment
        // has not yet processed.
        if (AsChar(entities.Collection.AdjustedInd) == 'Y' && !
          Lt(import.ReportingPeriodEndDate.Date,
          entities.Collection.CollectionAdjustmentDt) && (
            Equal(entities.Collection.DisbursementAdjProcessDate,
          local.Null1.Date) || Lt
          (import.ReportingPeriodEndDate.Date,
          entities.Collection.DisbursementAdjProcessDate)))
        {
          // CQ486 The disbursement adjustment processing has not yet occurred, 
          // so there is no negative disbursement amount to offset the positive
          // disbursement amount.  Since the collection is adjusted the
          // collection amount will be included in Suspense.  Skip this
          // collection.
          continue;
        }

        local.DisbPlusCrf.Amount = entities.Disbursement.Amount;

        foreach(var item2 in ReadDisbursement())
        {
          local.DisbPlusCrf.Amount += entities.CostRecoveryFee.Amount;
        }

        local.WarrPlusCrf.Amount += local.DisbPlusCrf.Amount;

        if (AsChar(import.Audit.Flag) == 'Y')
        {
          // -- Log the amount to the audit file.
          local.ForCreate.FiscalYear =
            import.Ocse157Verification.FiscalYear.GetValueOrDefault();
          local.ForCreate.RunNumber =
            import.Ocse157Verification.RunNumber.GetValueOrDefault();
          local.ForCreate.LineNumber = "B13";
          local.ForCreate.Comment = "B721 WARRANTS (1)";
          local.ForCreate.ObligorPersonNbr =
            entities.PaymentRequest.CsePersonNumber;
          local.ForCreate.CollectionAmount = local.DisbPlusCrf.Amount;
          local.ForCreate.CollectionDte =
            Date(entities.PaymentRequest.CreatedTimestamp);
          local.ForCreate.CaseWorkerName =
            entities.Disbursement.ReferenceNumber;
          UseFnCreateOcse157Verification();
        }
      }

      // -- Increment running total amount and warrant count.
      ++local.TotalWarrant.Count;
      local.TotalWarrant.TotalCurrency += local.WarrPlusCrf.Amount;

      // -- Convert the warrant created date to text
      local.DateWorkArea.TextDate =
        NumberToString(
          DateToInt(Date(entities.PaymentRequest.CreatedTimestamp)), 8, 8);

      // -- Convert the warrant amount to text (Multiply * 100 then convert and 
      // insert a decimal point)
      local.TextWorkArea.Text30 =
        NumberToString((long)(local.WarrPlusCrf.Amount * 100), 5, 9) + "." + NumberToString
        ((long)(local.WarrPlusCrf.Amount * 100), 14, 2);

      // -------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <CSE Person Number> <Amount> 
      // <Created Date> <Payment Request ID>
      // -------------------------------------------------------------------------------------------
      local.External.TextLine80 = "01 " + entities
        .PaymentRequest.CsePersonNumber;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        16) + local.TextWorkArea.Text30;

      if (local.WarrPlusCrf.Amount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
      }

      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        32) + local.DateWorkArea.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        48) + NumberToString
        (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9);
      UseFnB721ExtWriteFile();

      if (!Equal(local.External.TextReturnCode, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      // cmj 02/19/2005 set compare date the to payment request timestamp
      local.PaymentRequestCreated.Date =
        Date(entities.PaymentRequest.CreatedTimestamp);

      // cmj 02/19/2005  compare the dates and set amount and count for each 
      // aging category
      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt1825.Date))
      {
        ++local.AgingGt1825.Count;
        local.AgingGt1825.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt1095.Date))
      {
        ++local.AgingGt1095.Count;
        local.AgingGt1095.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt365.Date))
      {
        ++local.AgingGt365.Count;
        local.AgingGt365.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt180.Date))
      {
        ++local.AgingGt180.Count;
        local.AgingGt180.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt30.Date))
      {
        ++local.AgingGt30.Count;
        local.AgingGt30.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      if (Lt(local.PaymentRequestCreated.Date, local.WarrGt2.Date))
      {
        ++local.AgingGt2.Count;
        local.AgingGt2.TotalCurrency += local.WarrPlusCrf.Amount;

        continue;
      }

      local.AgingLastDaysBusiness.TotalCurrency += local.WarrPlusCrf.Amount;
      ++local.AgingLastDaysBusiness.Count;
    }

    // -- Convert the total warrant amount to text (Multiply * 100 then convert 
    // and insert a decimal point)
    local.TextWorkArea.Text30 =
      NumberToString((long)(local.TotalWarrant.TotalCurrency * 100), 1, 13) + "."
      + NumberToString((long)(local.TotalWarrant.TotalCurrency * 100), 14, 2);

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // WARRANT AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
      (local.TotalWarrant.Count, 6, 10);
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 35) +
      "Total Warrant Amount = ";
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 58) +
      local.TextWorkArea.Text30;

    if (local.TotalWarrant.TotalCurrency < 0)
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
    // -- Close CSE Warrant error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // cmj 02/19/2005  set the ocse34 with data for aging
    // -------------------------------------------------------------------------------------------
    // -- Write Total Warrant amount to the OCSE34 record.
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
    local.ProgramCheckpointRestart.RestartInfo = "04 " + Substring
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
          local.EabReportSend.RptDetail = "WARRANTS.............." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 76);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Warrants........................Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.WarrErrorGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.WarrErrorGt1825, 0))
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

  private IEnumerable<bool> ReadDisbCollectionDisbursementCollection()
  {
    entities.Disbursement.Populated = false;
    entities.DisbCollection.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadDisbCollectionDisbursementCollection",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbCollection.CpaType = db.GetString(reader, 0);
        entities.DisbCollection.CspNumber = db.GetString(reader, 1);
        entities.DisbCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbCollection.OtyId = db.GetNullableInt32(reader, 3);
        entities.Collection.OtyId = db.GetInt32(reader, 3);
        entities.DisbCollection.OtrTypeDisb = db.GetNullableString(reader, 4);
        entities.Collection.OtrType = db.GetString(reader, 4);
        entities.DisbCollection.OtrId = db.GetNullableInt32(reader, 5);
        entities.Collection.OtrId = db.GetInt32(reader, 5);
        entities.DisbCollection.CpaTypeDisb = db.GetNullableString(reader, 6);
        entities.Collection.CpaType = db.GetString(reader, 6);
        entities.DisbCollection.CspNumberDisb = db.GetNullableString(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.DisbCollection.ObgId = db.GetNullableInt32(reader, 8);
        entities.Collection.ObgId = db.GetInt32(reader, 8);
        entities.DisbCollection.CrdId = db.GetNullableInt32(reader, 9);
        entities.Collection.CrdId = db.GetInt32(reader, 9);
        entities.DisbCollection.CrvId = db.GetNullableInt32(reader, 10);
        entities.Collection.CrvId = db.GetInt32(reader, 10);
        entities.DisbCollection.CstId = db.GetNullableInt32(reader, 11);
        entities.Collection.CstId = db.GetInt32(reader, 11);
        entities.DisbCollection.CrtId = db.GetNullableInt32(reader, 12);
        entities.Collection.CrtType = db.GetInt32(reader, 12);
        entities.DisbCollection.ColId = db.GetNullableInt32(reader, 13);
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 13);
        entities.Disbursement.CpaType = db.GetString(reader, 14);
        entities.Disbursement.CspNumber = db.GetString(reader, 15);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 16);
        entities.Disbursement.Amount = db.GetDecimal(reader, 17);
        entities.Disbursement.PrqGeneratedId = db.GetNullableInt32(reader, 18);
        entities.Disbursement.ReferenceNumber =
          db.GetNullableString(reader, 19);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 20);
        entities.Collection.DisbursementAdjProcessDate = db.GetDate(reader, 21);
        entities.Collection.CollectionAdjustmentDt = db.GetDate(reader, 22);
        entities.Disbursement.Populated = true;
        entities.DisbCollection.Populated = true;
        entities.Collection.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursement()
  {
    System.Diagnostics.Debug.Assert(entities.DisbCollection.Populated);
    entities.CostRecoveryFee.Populated = false;

    return ReadEach("ReadDisbursement",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrPGeneratedId",
          entities.DisbCollection.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.DisbCollection.CpaType);
        db.SetString(command, "cspPNumber", entities.DisbCollection.CspNumber);
      },
      (db, reader) =>
      {
        entities.CostRecoveryFee.CpaType = db.GetString(reader, 0);
        entities.CostRecoveryFee.CspNumber = db.GetString(reader, 1);
        entities.CostRecoveryFee.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CostRecoveryFee.Amount = db.GetDecimal(reader, 3);
        entities.CostRecoveryFee.DbtGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.CostRecoveryFee.Populated = true;

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
        entities.Ocse34.CseWarrantAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.WarrErrorLda = db.GetNullableInt32(reader, 2);
        entities.Ocse34.WarrErrorGt2 = db.GetNullableInt32(reader, 3);
        entities.Ocse34.WarrErrorGt30 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.WarrErrorGt180 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.WarrErrorGt365 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.WarrErrorGt1095 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.WarrErrorGt1825 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetDate(
          command, "processDate1",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
        db.
          SetDate(command, "processDate2", local.Null1.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.Classification = db.GetString(reader, 5);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 6);
        entities.PaymentRequest.Type1 = db.GetString(reader, 7);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.PaymentRequest.Populated = true;

        return true;
      });
  }

  private void UpdateOcse34()
  {
    var cseWarrantAmt = (int?)local.TotalWarrant.TotalCurrency;
    var warrErrorLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var warrErrorGt2 = (int?)local.AgingGt2.TotalCurrency;
    var warrErrorGt30 = (int?)local.AgingGt30.TotalCurrency;
    var warrErrorGt180 = (int?)local.AgingGt180.TotalCurrency;
    var warrErrorGt365 = (int?)local.AgingGt365.TotalCurrency;
    var warrErrorGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var warrErrorGt1825 = (int?)local.AgingGt1825.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cseWarrAmt", cseWarrantAmt);
        db.SetNullableInt32(command, "warrErrorLda", warrErrorLda);
        db.SetNullableInt32(command, "warrErrorGt2", warrErrorGt2);
        db.SetNullableInt32(command, "warrErrorGt30", warrErrorGt30);
        db.SetNullableInt32(command, "warrErrorGt180", warrErrorGt180);
        db.SetNullableInt32(command, "warrErrorGt365", warrErrorGt365);
        db.SetNullableInt32(command, "warrErrorGt1095", warrErrorGt1095);
        db.SetNullableInt32(command, "warrErrorGt1825", warrErrorGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CseWarrantAmt = cseWarrantAmt;
    entities.Ocse34.WarrErrorLda = warrErrorLda;
    entities.Ocse34.WarrErrorGt2 = warrErrorGt2;
    entities.Ocse34.WarrErrorGt30 = warrErrorGt30;
    entities.Ocse34.WarrErrorGt180 = warrErrorGt180;
    entities.Ocse34.WarrErrorGt365 = warrErrorGt365;
    entities.Ocse34.WarrErrorGt1095 = warrErrorGt1095;
    entities.Ocse34.WarrErrorGt1825 = warrErrorGt1825;
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
    /// A value of DisbPlusCrf.
    /// </summary>
    [JsonPropertyName("disbPlusCrf")]
    public PaymentRequest DisbPlusCrf
    {
      get => disbPlusCrf ??= new();
      set => disbPlusCrf = value;
    }

    /// <summary>
    /// A value of WarrPlusCrf.
    /// </summary>
    [JsonPropertyName("warrPlusCrf")]
    public PaymentRequest WarrPlusCrf
    {
      get => warrPlusCrf ??= new();
      set => warrPlusCrf = value;
    }

    /// <summary>
    /// A value of PaymentRequestCreated.
    /// </summary>
    [JsonPropertyName("paymentRequestCreated")]
    public DateWorkArea PaymentRequestCreated
    {
      get => paymentRequestCreated ??= new();
      set => paymentRequestCreated = value;
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
    /// A value of WarrGt1825.
    /// </summary>
    [JsonPropertyName("warrGt1825")]
    public DateWorkArea WarrGt1825
    {
      get => warrGt1825 ??= new();
      set => warrGt1825 = value;
    }

    /// <summary>
    /// A value of WarrGt1095.
    /// </summary>
    [JsonPropertyName("warrGt1095")]
    public DateWorkArea WarrGt1095
    {
      get => warrGt1095 ??= new();
      set => warrGt1095 = value;
    }

    /// <summary>
    /// A value of WarrGt365.
    /// </summary>
    [JsonPropertyName("warrGt365")]
    public DateWorkArea WarrGt365
    {
      get => warrGt365 ??= new();
      set => warrGt365 = value;
    }

    /// <summary>
    /// A value of WarrGt180.
    /// </summary>
    [JsonPropertyName("warrGt180")]
    public DateWorkArea WarrGt180
    {
      get => warrGt180 ??= new();
      set => warrGt180 = value;
    }

    /// <summary>
    /// A value of WarrGt30.
    /// </summary>
    [JsonPropertyName("warrGt30")]
    public DateWorkArea WarrGt30
    {
      get => warrGt30 ??= new();
      set => warrGt30 = value;
    }

    /// <summary>
    /// A value of WarrGt2.
    /// </summary>
    [JsonPropertyName("warrGt2")]
    public DateWorkArea WarrGt2
    {
      get => warrGt2 ??= new();
      set => warrGt2 = value;
    }

    /// <summary>
    /// A value of Warr1Da.
    /// </summary>
    [JsonPropertyName("warr1Da")]
    public DateWorkArea Warr1Da
    {
      get => warr1Da ??= new();
      set => warr1Da = value;
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
    /// A value of TotalWarrant.
    /// </summary>
    [JsonPropertyName("totalWarrant")]
    public Common TotalWarrant
    {
      get => totalWarrant ??= new();
      set => totalWarrant = value;
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

    private PaymentRequest disbPlusCrf;
    private PaymentRequest warrPlusCrf;
    private DateWorkArea paymentRequestCreated;
    private Common agingGt1825;
    private Common agingGt1095;
    private Common agingGt365;
    private Common agingGt180;
    private Common agingGt30;
    private Common agingGt2;
    private Common agingLastDaysBusiness;
    private DateWorkArea warrGt1825;
    private DateWorkArea warrGt1095;
    private DateWorkArea warrGt365;
    private DateWorkArea warrGt180;
    private DateWorkArea warrGt30;
    private DateWorkArea warrGt2;
    private DateWorkArea warr1Da;
    private Common fileNumber;
    private External external;
    private DateWorkArea dateWorkArea;
    private Common totalWarrant;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of CostRecoveryFee.
    /// </summary>
    [JsonPropertyName("costRecoveryFee")]
    public DisbursementTransaction CostRecoveryFee
    {
      get => costRecoveryFee ??= new();
      set => costRecoveryFee = value;
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
    /// A value of DisbCollection.
    /// </summary>
    [JsonPropertyName("disbCollection")]
    public DisbursementTransaction DisbCollection
    {
      get => disbCollection ??= new();
      set => disbCollection = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private PaymentStatusHistory paymentStatusHistory;
    private DisbursementType disbursementType;
    private DisbursementTransaction costRecoveryFee;
    private DisbursementTransaction disbursement;
    private DisbursementTransaction disbCollection;
    private DisbursementTransactionRln disbursementTransactionRln;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private PaymentRequest paymentRequest;
    private Ocse34 ocse34;
    private Collection collection;
  }
#endregion
}

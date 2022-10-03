// Program: FN_B721_DISBURSEMENT_DEBITS, ID: 371195796, model: 746.
// Short name: SWE02056
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B721_DISBURSEMENT_DEBITS.
/// </summary>
[Serializable]
public partial class FnB721DisbursementDebits: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B721_DISBURSEMENT_DEBITS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB721DisbursementDebits(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB721DisbursementDebits.
  /// </summary>
  public FnB721DisbursementDebits(IContext context, Import import, Export export)
    :
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
    // 02/11/05  CMJ				Aging logic
    // 01/06/09  GVandy	CQ486		1) Add an audit trail to determine why part 1 and
    // part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Reformatted aging report output
    // 					3) General cleanup of aging logic
    // -----------------------------------------------------------------------------------------------------
    local.FileNumber.Count = 2;

    // -------------------------------------------------------------------------------------------
    // -- Open CSE Disbursement debit error interface file.
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
            1, 39) + "...DISBURSEMENT DEBIT ERRORS...";

          break;
        case 2:
          local.External.TextLine80 =
            "00 CSP NUMBER  REFERENCE NUMBER  COLLECTION DATE      AMOUNT   AGED DATE";
            

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
    // -- Read each disbursement error.
    // -------------------------------------------------------------------------------------------
    // -- 1/2/04 Per Jennifer, changed disbursement date to process date.
    foreach(var item in ReadDisbursementTransactionCsePerson())
    {
      // -- Increment running total amount and disbursement transaction count.
      ++local.TotalDisbursement.Count;
      local.TotalDisbursement.TotalCurrency += entities.DisbursementTransaction.
        Amount;

      // -- Convert the disbursement transaction collection date to text
      local.DateWorkArea.TextDate =
        NumberToString(
          DateToInt(entities.DisbursementTransaction.CollectionDate), 8, 8);

      // -- Convert the disbursement transaction collection timestamp date  to 
      // text for aging
      local.Age.TextDate =
        NumberToString(DateToInt(
          Date(entities.DisbursementTransaction.CreatedTimestamp)), 8, 8);

      // -- Convert the disbursement transaction amount to text (Multiply * 100 
      // then convert and insert a decimal point)
      local.TextWorkArea.Text30 =
        NumberToString((long)(entities.DisbursementTransaction.Amount * 100), 5,
        9) + "." + NumberToString
        ((long)(entities.DisbursementTransaction.Amount * 100), 14, 2);

      // -------------------------------------------------------------------------------------------
      // -- Write Detail Record.  Format 01 <Reference Number> <Collection Date>
      // <Amount> <Age Date>
      // -------------------------------------------------------------------------------------------
      local.External.TextLine80 = "01 " + entities.CsePerson.Number;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        16) + entities.DisbursementTransaction.ReferenceNumber;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        36) + local.DateWorkArea.TextDate;
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        50) + local.TextWorkArea.Text30;

      if (entities.DisbursementTransaction.Amount < 0)
      {
        local.External.TextLine80 = TrimEnd(local.External.TextLine80) + "-";
      }

      // add age date to audit file  4/07/2005 cmj
      local.External.TextLine80 =
        Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1,
        67) + local.Age.TextDate;
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
        local.ForCreate.Comment = "B721 DISBURSEMENT DEBITS (1)";
        local.ForCreate.ObligorPersonNbr = entities.CsePerson.Number;
        local.ForCreate.CollectionDte =
          entities.DisbursementTransaction.CollectionDate;
        local.ForCreate.CollectionAmount =
          entities.DisbursementTransaction.Amount;
        local.ForCreate.CaseWorkerName =
          entities.DisbursementTransaction.ReferenceNumber;
        UseFnCreateOcse157Verification();
      }

      // cmj 2/11/2005 set the compare date for the created timestamp
      local.DisbTranCreated.Date =
        Date(entities.DisbursementTransaction.CreatedTimestamp);

      // cmj 02/11/2005  calculate the aging monies
      if (Lt(local.DisbTranCreated.Date, local.Local1825Date.Date))
      {
        local.AgingGt1825.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt1825.Count;

        continue;
      }

      if (Lt(local.DisbTranCreated.Date, local.Local1095Date.Date))
      {
        local.AgingGt1095.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt1095.Count;

        continue;
      }

      if (Lt(local.DisbTranCreated.Date, local.Local365Date.Date))
      {
        local.AgingGt365.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt365.Count;

        continue;
      }

      if (Lt(local.DisbTranCreated.Date, local.Local180Date.Date))
      {
        local.AgingGt180.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt180.Count;

        continue;
      }

      if (Lt(local.DisbTranCreated.Date, local.Local30Date.Date))
      {
        local.AgingGt30.TotalCurrency += entities.DisbursementTransaction.
          Amount;
        ++local.AgingGt30.Count;

        continue;
      }

      if (Lt(local.DisbTranCreated.Date, local.Local2Date.Date))
      {
        local.AgingGt2.TotalCurrency += entities.DisbursementTransaction.Amount;
        ++local.AgingGt2.Count;

        continue;
      }

      local.AgingLastDaysBusiness.TotalCurrency += entities.
        DisbursementTransaction.Amount;
      ++local.AgingLastDaysBusiness.Count;
    }

    // -- Convert the total disbursement transaction amount to text (Multiply * 
    // 100 then convert and insert a decimal point)
    local.TextWorkArea.Text30 =
      NumberToString((long)(local.TotalDisbursement.TotalCurrency * 100), 1, 13) +
      "." + NumberToString
      ((long)(local.TotalDisbursement.TotalCurrency * 100), 14, 2);

    // -------------------------------------------------------------------------------------------
    // -- Write Footer Record.  Format 99 TOTAL DETAIL COUNT = 9999999999 TOTAL 
    // DISB AMOUNT = 9999999999999.99
    // -------------------------------------------------------------------------------------------
    local.External.TextLine80 = "99 Total Detail Count = " + NumberToString
      (local.TotalDisbursement.Count, 6, 10);
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 35) +
      "Total Disb Debit Amount = ";
    local.External.TextLine80 =
      Substring(local.External.TextLine80, External.TextLine80_MaxLength, 1, 62) +
      local.TextWorkArea.Text30;

    if (local.TotalDisbursement.TotalCurrency < 0)
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
    // -- Close CSE Disbursement debit error interface file.
    // -------------------------------------------------------------------------------------------
    local.External.FileInstruction = "CLOSE";
    UseFnB721ExtWriteFile();

    if (!Equal(local.External.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // -------------------------------------------------------------------------------------------
    // -- Write Total Disbursement amount to the OCSE34 record.
    // -------------------------------------------------------------------------------------------
    // cmj 02/11/2005  add the new ocse34 attributes here for updating the 
    // ocse34 for the aging attributes
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
    local.ProgramCheckpointRestart.RestartInfo = "03 " + Substring
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
          local.EabReportSend.RptDetail = "DISBURSEMENT DEBITS..." + Substring
            (local.External.TextLine80, External.TextLine80_MaxLength, 4, 76);

          break;
        case 2:
          local.EabReportSend.RptDetail =
            "Aging Disbursement Debit Totals.......Count.........Amount..";

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "      Last Days Business............" + NumberToString
            (local.AgingLastDaysBusiness.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitLda.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitLda, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 4:
          local.EabReportSend.RptDetail =
            "      Greater than 2 Days..........." + NumberToString
            (local.AgingGt2.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt2.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt2, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 5:
          local.EabReportSend.RptDetail =
            "      Greater than 30 Days.........." + NumberToString
            (local.AgingGt30.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt30.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt30, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "      Greater than 180 Days........." + NumberToString
            (local.AgingGt180.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt180.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt180, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "      Greater than 365 Days........." + NumberToString
            (local.AgingGt365.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt365.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt365, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "      Greater than 1095 Days........" + NumberToString
            (local.AgingGt1095.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt1095.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt1095, 0))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "-";
          }

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "      Greater than 1825 Days........" + NumberToString
            (local.AgingGt1825.Count, 7, 9) + "......" + NumberToString
            (entities.Ocse34.DisbDebitGt1825.GetValueOrDefault(), 7, 9);

          if (Lt(entities.Ocse34.DisbDebitGt1825, 0))
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

    MoveExternal(local.External, useImport.External);
    useImport.FileNumber.Count = local.FileNumber.Count;
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

  private IEnumerable<bool> ReadDisbursementTransactionCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return ReadEach("ReadDisbursementTransactionCsePerson",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.ReportingPeriodEndDate.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate1",
          import.ReportingPeriodEndDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate2", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 0);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 3);
        entities.DisbursementTransaction.Amount = db.GetDecimal(reader, 4);
        entities.DisbursementTransaction.ProcessDate =
          db.GetNullableDate(reader, 5);
        entities.DisbursementTransaction.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTransaction.DisbursementDate =
          db.GetNullableDate(reader, 7);
        entities.DisbursementTransaction.CashNonCashInd =
          db.GetString(reader, 8);
        entities.DisbursementTransaction.CollectionDate =
          db.GetNullableDate(reader, 9);
        entities.DisbursementTransaction.ReferenceNumber =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
        CheckValid<DisbursementTransaction>("CashNonCashInd",
          entities.DisbursementTransaction.CashNonCashInd);

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
        entities.Ocse34.CseDisbDebitAmt = db.GetNullableInt32(reader, 1);
        entities.Ocse34.DisbDebitLda = db.GetNullableInt32(reader, 2);
        entities.Ocse34.DisbDebitGt2 = db.GetNullableInt32(reader, 3);
        entities.Ocse34.DisbDebitGt30 = db.GetNullableInt32(reader, 4);
        entities.Ocse34.DisbDebitGt180 = db.GetNullableInt32(reader, 5);
        entities.Ocse34.DisbDebitGt365 = db.GetNullableInt32(reader, 6);
        entities.Ocse34.DisbDebitGt1095 = db.GetNullableInt32(reader, 7);
        entities.Ocse34.DisbDebitGt1825 = db.GetNullableInt32(reader, 8);
        entities.Ocse34.Populated = true;
      });
  }

  private void UpdateOcse34()
  {
    var cseDisbDebitAmt = (int?)local.TotalDisbursement.TotalCurrency;
    var disbDebitLda = (int?)local.AgingLastDaysBusiness.TotalCurrency;
    var disbDebitGt2 = (int?)local.AgingGt2.TotalCurrency;
    var disbDebitGt30 = (int?)local.AgingGt30.TotalCurrency;
    var disbDebitGt180 = (int?)local.AgingGt180.TotalCurrency;
    var disbDebitGt365 = (int?)local.AgingGt365.TotalCurrency;
    var disbDebitGt1095 = (int?)local.AgingGt1095.TotalCurrency;
    var disbDebitGt1825 = (int?)local.AgingGt1825.TotalCurrency;

    entities.Ocse34.Populated = false;
    Update("UpdateOcse34",
      (db, command) =>
      {
        db.SetNullableInt32(command, "cseDsbDbtAmt", cseDisbDebitAmt);
        db.SetNullableInt32(command, "disbDebitLda", disbDebitLda);
        db.SetNullableInt32(command, "disbDebitGt2", disbDebitGt2);
        db.SetNullableInt32(command, "disbDebitGt30", disbDebitGt30);
        db.SetNullableInt32(command, "disbDebitGt180", disbDebitGt180);
        db.SetNullableInt32(command, "disbDebitGt365", disbDebitGt365);
        db.SetNullableInt32(command, "disbDebitGt1095", disbDebitGt1095);
        db.SetNullableInt32(command, "disbDebitGt1825", disbDebitGt1825);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Ocse34.CreatedTimestamp.GetValueOrDefault());
      });

    entities.Ocse34.CseDisbDebitAmt = cseDisbDebitAmt;
    entities.Ocse34.DisbDebitLda = disbDebitLda;
    entities.Ocse34.DisbDebitGt2 = disbDebitGt2;
    entities.Ocse34.DisbDebitGt30 = disbDebitGt30;
    entities.Ocse34.DisbDebitGt180 = disbDebitGt180;
    entities.Ocse34.DisbDebitGt365 = disbDebitGt365;
    entities.Ocse34.DisbDebitGt1095 = disbDebitGt1095;
    entities.Ocse34.DisbDebitGt1825 = disbDebitGt1825;
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
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

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

    private Ocse34 ocse34;
    private DateWorkArea reportingPeriodEndDate;
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
    /// A value of DisbTranCreated.
    /// </summary>
    [JsonPropertyName("disbTranCreated")]
    public DateWorkArea DisbTranCreated
    {
      get => disbTranCreated ??= new();
      set => disbTranCreated = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of TotalDisbursement.
    /// </summary>
    [JsonPropertyName("totalDisbursement")]
    public Common TotalDisbursement
    {
      get => totalDisbursement ??= new();
      set => totalDisbursement = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
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
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
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
    private DateWorkArea disbTranCreated;
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
    private TextWorkArea textWorkArea;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private Common totalDisbursement;
    private DateWorkArea null1;
    private DateWorkArea dateWorkArea;
    private External external;
    private Common fileNumber;
    private ProgramCheckpointRestart programCheckpointRestart;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private Collection collection;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private Ocse34 ocse34;
    private DisbursementTransaction disbursementTransaction;
  }
#endregion
}

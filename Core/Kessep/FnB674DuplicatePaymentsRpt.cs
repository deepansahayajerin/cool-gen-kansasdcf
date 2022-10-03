// Program: FN_B674_DUPLICATE_PAYMENTS_RPT, ID: 373002898, model: 746.
// Short name: SWEF674B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B674_DUPLICATE_PAYMENTS_RPT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB674DuplicatePaymentsRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B674_DUPLICATE_PAYMENTS_RPT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB674DuplicatePaymentsRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB674DuplicatePaymentsRpt.
  /// </summary>
  public FnB674DuplicatePaymentsRpt(IContext context, Import import,
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
    // *****************************************************************
    // Date      PR        Developer           Description
    // --------  --------  ------------------  
    // ------------------------------------------
    // 01/25/00  00084666  Mike Fangman        Create new report of duplicate 
    // payments.  This is done by adding up all of the money that went out for a
    // person/reference number and comparing the total to the cash receipt
    // detail amount.
    // 12/10/10  CQ22192   RMathews            Expanded amount fields from 5.2 
    // to 6.2
    // *****************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Initialized.Date = new DateTime(1, 1, 2);
    local.EabFileHandling.Action = "OPEN";

    // --------------  Get CSE person number range to process --------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "WRITE";

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.RptDetail =
        "Program processing info not found for " + local
        .ProgramProcessingInfo.Name;
      UseCabErrorReport3();

      return;
    }

    local.TraceInd.Flag =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 1);

    if (AsChar(local.TraceInd.Flag) == 'Y')
    {
      local.EabReportSend.RptDetail = "Trace ind " + local.TraceInd.Flag;
      UseCabControlReport1();
    }

    local.StartingCsePerson.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 3, 10);
    local.EndingCsePerson.Number =
      Substring(local.ProgramProcessingInfo.ParameterList, 14, 10);

    if (!Equal(local.StartingCsePerson.Number, "0000000000") || !
      Equal(local.EndingCsePerson.Number, "9999999999"))
    {
      local.EabReportSend.RptDetail = local.EabReportSend.ProgramName + ":  Starting CSE person # " +
        local.StartingCsePerson.Number + "  Ending CSE person # " + local
        .EndingCsePerson.Number;
      UseCabControlReport1();
    }

    local.StartingDisbDateTxt.Text10 =
      Substring(local.ProgramProcessingInfo.ParameterList, 25, 10);

    if (!IsEmpty(local.StartingDisbDateTxt.Text10))
    {
      local.StartingDisbursement.ProcessDate =
        StringToDate(local.StartingDisbDateTxt.Text10);
    }
    else
    {
      local.StartingDisbursement.ProcessDate =
        local.ProgramProcessingInfo.ProcessDate;
    }

    local.EndingDisbursement.ProcessDate =
      local.ProgramProcessingInfo.ProcessDate;
    local.EndingDisbDateTxt.Text8 =
      NumberToString(DateToInt(local.EndingDisbursement.ProcessDate), 8, 8);
    local.EabReportSend.RptDetail = "Starting date " + local
      .StartingDisbDateTxt.Text10 + " ending date " + Substring
      (local.EndingDisbDateTxt.Text8, TextWorkArea.Text8_MaxLength, 1, 4) + "-"
      + Substring
      (local.EndingDisbDateTxt.Text8, TextWorkArea.Text8_MaxLength, 5, 2) + "-"
      + Substring
      (local.EndingDisbDateTxt.Text8, TextWorkArea.Text8_MaxLength, 7, 2);
    UseCabControlReport1();
    local.EabReportSend.RptDetail = "";
    UseCabErrorReport2();

    // --------------  Print column headings --------------
    local.EabReportSend.RptDetail =
      "                           ---- Cash Receipt Detail ----     Tot CR     Tot AR";
      
    UseCabErrorReport2();
    local.EabReportSend.RptDetail =
      "                            Created    Coll                 Pot Rec    Pot Rec";
      
    UseCabErrorReport2();
    local.EabReportSend.RptDetail =
      " Obligee #   Reference #     Date      Date       Amount     Amount     Amount";
      
    UseCabErrorReport2();

    // Add read each (with distinct) to get the reference numbers of those 
    // disbursements created with a process date = the PPI process date.
    // Only report on disbursements for those reference numbers.
    foreach(var item in ReadDisbursementCsePerson())
    {
      if (Equal(entities.DriverObligee2.Number, local.DriverCsePerson.Number) &&
        Equal
        (entities.DriverDisbursement.ReferenceNumber,
        local.DriverDisbursement.ReferenceNumber))
      {
        continue;
      }

      local.DriverCsePerson.Number = entities.DriverObligee2.Number;
      local.DriverDisbursement.ReferenceNumber =
        entities.DriverDisbursement.ReferenceNumber;

      if (AsChar(local.TraceInd.Flag) == 'Y')
      {
        local.EabReportSend.RptDetail = "Driver Person # " + entities
          .DriverObligee2.Number + "  Reference # " + entities
          .DriverDisbursement.ReferenceNumber + " Process date " + NumberToString
          (DateToInt(entities.DriverDisbursement.ProcessDate), 15);
        UseCabErrorReport2();
      }

      foreach(var item1 in ReadDisbursementCsePersonDisbursementType())
      {
        local.Ref1.Text1 =
          Substring(entities.Disbursement.ReferenceNumber, 8, 1);

        if (AsChar(local.Ref1.Text1) != '-')
        {
          continue;
        }

        if (!Equal(entities.Disbursement.ReferenceNumber,
          local.HoldForCrRead.ReferenceNumber))
        {
          local.CashReceipt.SequentialNumber =
            (int)StringToNumber(Substring(
              entities.Disbursement.ReferenceNumber, 1, 7));
          local.CashReceiptDetail.SequentialIdentifier =
            (int)StringToNumber(Substring(
              entities.Disbursement.ReferenceNumber, 9, 4));

          if (ReadCashReceiptDetailCashReceiptCashReceiptType())
          {
            local.HoldForCrRead.ReferenceNumber =
              entities.Disbursement.ReferenceNumber;
          }
          else
          {
            local.EabReportSend.RptDetail = "CRD nf for disb id " + NumberToString
              (entities.Disbursement.SystemGeneratedIdentifier, 15);
            UseCabErrorReport2();

            continue;
          }
        }

        // Don't process non-cash collections.
        if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 7 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 8 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 11 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 13 || entities
          .CashReceiptType.SystemGeneratedIdentifier == 14)
        {
          continue;
        }

        if (AsChar(local.TraceInd.Flag) == 'Y')
        {
          UseFnSwefr001PrintDisb();
        }

        ++local.DisbRecsRead.Count;

        if (!Equal(entities.Obligee1.Number, local.HoldCsePerson.Number))
        {
          // Finish processing for last CR.
          if (local.TotDisbForPerRefNbr.TotalCurrency > local
            .HoldCashReceiptDetail.CollectionAmount)
          {
            local.PotRecForCr.TotalCurrency =
              local.TotDisbForPerRefNbr.TotalCurrency - local
              .HoldCashReceiptDetail.CollectionAmount;
            local.PotRecForAr.TotalCurrency += local.PotRecForCr.TotalCurrency;
            local.PotRecForRun.TotalCurrency += local.PotRecForCr.TotalCurrency;
            UseFnSwefr001DupPmtRptDtl();
            local.PotRecForCr.TotalCurrency = 0;
          }

          if (local.PotRecForAr.TotalCurrency > 0)
          {
            local.PotRcvTxt.Text10 =
              NumberToString((long)local.PotRecForAr.TotalCurrency, 10, 6) + "."
              + NumberToString
              ((long)(local.PotRecForAr.TotalCurrency * 100), 14, 2);
            local.EabReportSend.RptDetail =
              "                                                                     " +
              local.PotRcvTxt.Text10;
            UseCabErrorReport2();
            local.EabReportSend.RptDetail = "";
            UseCabErrorReport2();
            local.PotRecForAr.TotalCurrency = 0;
            ++local.ArsWithDupDisbPmts.Count;
          }

          // Start processing for current CR.
          if (entities.Disbursement.Amount <= 0)
          {
            local.TotPosForProcessDate.Amount = 0;
            local.TotDisbForPerRefNbr.TotalCurrency = 0;
          }
          else
          {
            local.TotPosForProcessDate.Amount = entities.Disbursement.Amount;
            local.TotDisbForPerRefNbr.TotalCurrency =
              entities.Disbursement.Amount;
          }

          local.HoldCsePerson.Number = entities.Obligee1.Number;
          local.HoldDisbursement.ReferenceNumber =
            entities.Disbursement.ReferenceNumber;
          local.HoldDisbursement.ProcessDate =
            entities.Disbursement.ProcessDate;
          local.HoldCashReceiptDetail.Assign(entities.CashReceiptDetail);

          continue;
        }
        else if (!Equal(entities.Disbursement.ReferenceNumber,
          local.HoldDisbursement.ReferenceNumber))
        {
          // Finish processing for last CR.
          if (local.TotDisbForPerRefNbr.TotalCurrency > local
            .HoldCashReceiptDetail.CollectionAmount)
          {
            local.PotRecForCr.TotalCurrency =
              local.TotDisbForPerRefNbr.TotalCurrency - local
              .HoldCashReceiptDetail.CollectionAmount;
            local.PotRecForAr.TotalCurrency += local.PotRecForCr.TotalCurrency;
            local.PotRecForRun.TotalCurrency += local.PotRecForCr.TotalCurrency;
            UseFnSwefr001DupPmtRptDtl();
            local.PotRecForCr.TotalCurrency = 0;
          }

          // Start processing for current CR.
          if (entities.Disbursement.Amount <= 0)
          {
            local.TotPosForProcessDate.Amount = 0;
            local.TotDisbForPerRefNbr.TotalCurrency = 0;
            local.HoldDisbursement.ProcessDate = local.Initialized.Date;
          }
          else
          {
            local.TotPosForProcessDate.Amount = entities.Disbursement.Amount;
            local.TotDisbForPerRefNbr.TotalCurrency =
              entities.Disbursement.Amount;
            local.HoldDisbursement.ProcessDate =
              entities.Disbursement.ProcessDate;
            local.HoldDisbursement.ReferenceNumber =
              entities.Disbursement.ReferenceNumber;
            local.HoldCashReceiptDetail.Assign(entities.CashReceiptDetail);
          }

          continue;
        }
        else if (!Equal(entities.Disbursement.ProcessDate,
          local.HoldDisbursement.ProcessDate))
        {
          if (entities.Disbursement.Amount <= 0)
          {
            local.TotPosForProcessDate.Amount = 0;
          }
          else
          {
            local.TotPosForProcessDate.Amount = entities.Disbursement.Amount;
            local.TotDisbForPerRefNbr.TotalCurrency += entities.Disbursement.
              Amount;
          }

          local.HoldDisbursement.ProcessDate =
            entities.Disbursement.ProcessDate;
        }
        else if (entities.Disbursement.Amount < 0)
        {
          if (-entities.Disbursement.Amount <= local
            .TotPosForProcessDate.Amount)
          {
            local.TotDisbForPerRefNbr.TotalCurrency += entities.Disbursement.
              Amount;
            local.TotPosForProcessDate.Amount += entities.Disbursement.Amount;
          }
          else
          {
            local.TotDisbForPerRefNbr.TotalCurrency += -
              local.TotPosForProcessDate.Amount;
            local.TotPosForProcessDate.Amount = 0;
          }
        }
        else
        {
          local.TotDisbForPerRefNbr.TotalCurrency += entities.Disbursement.
            Amount;
          local.TotPosForProcessDate.Amount += entities.Disbursement.Amount;
        }
      }
    }

    // Finish processing for last CR.
    if (local.TotDisbForPerRefNbr.TotalCurrency > local
      .HoldCashReceiptDetail.CollectionAmount)
    {
      local.PotRecForCr.TotalCurrency =
        local.TotDisbForPerRefNbr.TotalCurrency - local
        .HoldCashReceiptDetail.CollectionAmount;
      local.PotRecForAr.TotalCurrency += local.PotRecForCr.TotalCurrency;
      local.PotRecForRun.TotalCurrency += local.PotRecForCr.TotalCurrency;
      UseFnSwefr001DupPmtRptDtl();
    }

    if (local.PotRecForAr.TotalCurrency > 0)
    {
      local.PotRcvTxt.Text10 =
        NumberToString((long)local.PotRecForAr.TotalCurrency, 10, 6) + "." + NumberToString
        ((long)(local.PotRecForAr.TotalCurrency * 100), 14, 2);
      local.EabReportSend.RptDetail =
        "                                                                     " +
        local.PotRcvTxt.Text10;
      UseCabErrorReport2();
      ++local.ArsWithDupDisbPmts.Count;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total disbursement records read                  " + NumberToString
      (local.DisbRecsRead.Count, 6, 10);
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total CRDs with potential duplicates             " + NumberToString
      (local.CrdsWithDupPmt.Count, 6, 10);
    UseCabControlReport1();
    local.EabReportSend.RptDetail =
      "Total ARs with potential duplicate disbursements " + NumberToString
      (local.ArsWithDupDisbPmts.Count, 6, 10);
    UseCabControlReport1();
    local.TotDupPmtAmtTxt.Text10 =
      NumberToString((long)local.PotRecForRun.TotalCurrency, 9, 7) + "." + NumberToString
      ((long)(local.PotRecForRun.TotalCurrency * 100), 14, 2);
    local.EabReportSend.RptDetail =
      "Total potential recovery amount                  " + local
      .TotDupPmtAmtTxt.Text10;
    UseCabControlReport1();
  }

  private static void MoveDisbursementTransaction(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);
  }

  private void UseFnSwefr001DupPmtRptDtl()
  {
    var useImport = new FnSwefr001DupPmtRptDtl.Import();
    var useExport = new FnSwefr001DupPmtRptDtl.Export();

    useImport.PotRcvForCr.TotalCurrency = local.PotRecForCr.TotalCurrency;
    useImport.Obligee.Number = local.HoldCsePerson.Number;
    useImport.Disbursement.ReferenceNumber =
      local.HoldDisbursement.ReferenceNumber;
    useImport.CashReceiptDetail.Assign(local.HoldCashReceiptDetail);
    useExport.ImpExpCrdsWithDupPmt.Count = local.CrdsWithDupPmt.Count;

    Call(FnSwefr001DupPmtRptDtl.Execute, useImport, useExport);

    local.CrdsWithDupPmt.Count = useExport.ImpExpCrdsWithDupPmt.Count;
  }

  private void UseFnSwefr001PrintDisb()
  {
    var useImport = new FnSwefr001PrintDisb.Import();
    var useExport = new FnSwefr001PrintDisb.Export();

    useImport.Obligee.Number = entities.Obligee1.Number;
    MoveDisbursementTransaction(entities.Disbursement, useImport.Disbursement);
    useImport.DisbursementType.Code = entities.DisbursementType.Code;
    useImport.CashReceiptDetail.CollectionAmount =
      entities.CashReceiptDetail.CollectionAmount;
    useImport.TotPosForProcessDate.Amount = local.TotPosForProcessDate.Amount;
    useImport.TotDisbForPerRefNbr.TotalCurrency =
      local.TotDisbForPerRefNbr.TotalCurrency;

    Call(FnSwefr001PrintDisb.Execute, useImport, useExport);
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private bool ReadCashReceiptDetailCashReceiptCashReceiptType()
  {
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptDetailCashReceiptCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.
          SetInt32(command, "cashReceiptId", local.CashReceipt.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 6);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 7);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 8);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 9);
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private IEnumerable<bool> ReadDisbursementCsePerson()
  {
    entities.DriverObligee2.Populated = false;
    entities.DriverDisbursement.Populated = false;

    return ReadEach("ReadDisbursementCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "processDate1",
          local.StartingDisbursement.ProcessDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "processDate2",
          local.EndingDisbursement.ProcessDate.GetValueOrDefault());
        db.SetString(command, "number1", local.StartingCsePerson.Number);
        db.SetString(command, "number2", local.EndingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DriverDisbursement.CpaType = db.GetString(reader, 0);
        entities.DriverDisbursement.CspNumber = db.GetString(reader, 1);
        entities.DriverObligee2.Number = db.GetString(reader, 1);
        entities.DriverDisbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DriverDisbursement.ProcessDate = db.GetNullableDate(reader, 3);
        entities.DriverDisbursement.ReferenceNumber =
          db.GetNullableString(reader, 4);
        entities.DriverObligee2.Populated = true;
        entities.DriverDisbursement.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DriverDisbursement.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDisbursementCsePersonDisbursementType()
  {
    entities.Obligee1.Populated = false;
    entities.Disbursement.Populated = false;
    entities.DisbursementType.Populated = false;

    return ReadEach("ReadDisbursementCsePersonDisbursementType",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.DriverObligee2.Number);
        db.SetNullableString(
          command, "referenceNumber",
          entities.DriverDisbursement.ReferenceNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Disbursement.CpaType = db.GetString(reader, 0);
        entities.Disbursement.CspNumber = db.GetString(reader, 1);
        entities.Obligee1.Number = db.GetString(reader, 1);
        entities.Disbursement.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.Disbursement.Amount = db.GetDecimal(reader, 3);
        entities.Disbursement.ProcessDate = db.GetNullableDate(reader, 4);
        entities.Disbursement.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.Disbursement.DbtGeneratedId = db.GetNullableInt32(reader, 6);
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 6);
        entities.Disbursement.ReferenceNumber = db.GetNullableString(reader, 7);
        entities.DisbursementType.Code = db.GetString(reader, 8);
        entities.DisbursementType.ProgramCode = db.GetString(reader, 9);
        entities.Obligee1.Populated = true;
        entities.Disbursement.Populated = true;
        entities.DisbursementType.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType",
          entities.Disbursement.CpaType);

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
    /// <summary>
    /// A value of DriverDisbursement.
    /// </summary>
    [JsonPropertyName("driverDisbursement")]
    public DisbursementTransaction DriverDisbursement
    {
      get => driverDisbursement ??= new();
      set => driverDisbursement = value;
    }

    /// <summary>
    /// A value of DriverCsePerson.
    /// </summary>
    [JsonPropertyName("driverCsePerson")]
    public CsePerson DriverCsePerson
    {
      get => driverCsePerson ??= new();
      set => driverCsePerson = value;
    }

    /// <summary>
    /// A value of EndingDisbDateTxt.
    /// </summary>
    [JsonPropertyName("endingDisbDateTxt")]
    public TextWorkArea EndingDisbDateTxt
    {
      get => endingDisbDateTxt ??= new();
      set => endingDisbDateTxt = value;
    }

    /// <summary>
    /// A value of StartingDisbDateTxt.
    /// </summary>
    [JsonPropertyName("startingDisbDateTxt")]
    public TextWorkArea StartingDisbDateTxt
    {
      get => startingDisbDateTxt ??= new();
      set => startingDisbDateTxt = value;
    }

    /// <summary>
    /// A value of StartingDisbursement.
    /// </summary>
    [JsonPropertyName("startingDisbursement")]
    public DisbursementTransaction StartingDisbursement
    {
      get => startingDisbursement ??= new();
      set => startingDisbursement = value;
    }

    /// <summary>
    /// A value of EndingDisbursement.
    /// </summary>
    [JsonPropertyName("endingDisbursement")]
    public DisbursementTransaction EndingDisbursement
    {
      get => endingDisbursement ??= new();
      set => endingDisbursement = value;
    }

    /// <summary>
    /// A value of PotRcvTxt.
    /// </summary>
    [JsonPropertyName("potRcvTxt")]
    public TextWorkArea PotRcvTxt
    {
      get => potRcvTxt ??= new();
      set => potRcvTxt = value;
    }

    /// <summary>
    /// A value of HoldForCrRead.
    /// </summary>
    [JsonPropertyName("holdForCrRead")]
    public DisbursementTransaction HoldForCrRead
    {
      get => holdForCrRead ??= new();
      set => holdForCrRead = value;
    }

    /// <summary>
    /// A value of Ref1.
    /// </summary>
    [JsonPropertyName("ref1")]
    public WorkArea Ref1
    {
      get => ref1 ??= new();
      set => ref1 = value;
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
    /// A value of FeeInfoInd.
    /// </summary>
    [JsonPropertyName("feeInfoInd")]
    public Common FeeInfoInd
    {
      get => feeInfoInd ??= new();
      set => feeInfoInd = value;
    }

    /// <summary>
    /// A value of TraceInd.
    /// </summary>
    [JsonPropertyName("traceInd")]
    public Common TraceInd
    {
      get => traceInd ??= new();
      set => traceInd = value;
    }

    /// <summary>
    /// A value of PotRecForCr.
    /// </summary>
    [JsonPropertyName("potRecForCr")]
    public Common PotRecForCr
    {
      get => potRecForCr ??= new();
      set => potRecForCr = value;
    }

    /// <summary>
    /// A value of PotRecForAr.
    /// </summary>
    [JsonPropertyName("potRecForAr")]
    public Common PotRecForAr
    {
      get => potRecForAr ??= new();
      set => potRecForAr = value;
    }

    /// <summary>
    /// A value of PotRecForRun.
    /// </summary>
    [JsonPropertyName("potRecForRun")]
    public Common PotRecForRun
    {
      get => potRecForRun ??= new();
      set => potRecForRun = value;
    }

    /// <summary>
    /// A value of SkipCsePerson.
    /// </summary>
    [JsonPropertyName("skipCsePerson")]
    public CsePerson SkipCsePerson
    {
      get => skipCsePerson ??= new();
      set => skipCsePerson = value;
    }

    /// <summary>
    /// A value of SkipDisbursement.
    /// </summary>
    [JsonPropertyName("skipDisbursement")]
    public DisbursementTransaction SkipDisbursement
    {
      get => skipDisbursement ??= new();
      set => skipDisbursement = value;
    }

    /// <summary>
    /// A value of HoldCsePerson.
    /// </summary>
    [JsonPropertyName("holdCsePerson")]
    public CsePerson HoldCsePerson
    {
      get => holdCsePerson ??= new();
      set => holdCsePerson = value;
    }

    /// <summary>
    /// A value of HoldDisbursement.
    /// </summary>
    [JsonPropertyName("holdDisbursement")]
    public DisbursementTransaction HoldDisbursement
    {
      get => holdDisbursement ??= new();
      set => holdDisbursement = value;
    }

    /// <summary>
    /// A value of HoldCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("holdCashReceiptDetail")]
    public CashReceiptDetail HoldCashReceiptDetail
    {
      get => holdCashReceiptDetail ??= new();
      set => holdCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of StartingDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("startingDisbursementTransaction")]
    public DisbursementTransaction StartingDisbursementTransaction
    {
      get => startingDisbursementTransaction ??= new();
      set => startingDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of EndingDisbursementTransaction.
    /// </summary>
    [JsonPropertyName("endingDisbursementTransaction")]
    public DisbursementTransaction EndingDisbursementTransaction
    {
      get => endingDisbursementTransaction ??= new();
      set => endingDisbursementTransaction = value;
    }

    /// <summary>
    /// A value of MultipleArsForCrdInd.
    /// </summary>
    [JsonPropertyName("multipleArsForCrdInd")]
    public Common MultipleArsForCrdInd
    {
      get => multipleArsForCrdInd ??= new();
      set => multipleArsForCrdInd = value;
    }

    /// <summary>
    /// A value of DisbSign.
    /// </summary>
    [JsonPropertyName("disbSign")]
    public TextWorkArea DisbSign
    {
      get => disbSign ??= new();
      set => disbSign = value;
    }

    /// <summary>
    /// A value of TotPosForProcessDate.
    /// </summary>
    [JsonPropertyName("totPosForProcessDate")]
    public DisbursementTransaction TotPosForProcessDate
    {
      get => totPosForProcessDate ??= new();
      set => totPosForProcessDate = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of TotDisbForPerRefNbr.
    /// </summary>
    [JsonPropertyName("totDisbForPerRefNbr")]
    public Common TotDisbForPerRefNbr
    {
      get => totDisbForPerRefNbr ??= new();
      set => totDisbForPerRefNbr = value;
    }

    /// <summary>
    /// A value of DisbRecsRead.
    /// </summary>
    [JsonPropertyName("disbRecsRead")]
    public Common DisbRecsRead
    {
      get => disbRecsRead ??= new();
      set => disbRecsRead = value;
    }

    /// <summary>
    /// A value of CrdsWithDupPmt.
    /// </summary>
    [JsonPropertyName("crdsWithDupPmt")]
    public Common CrdsWithDupPmt
    {
      get => crdsWithDupPmt ??= new();
      set => crdsWithDupPmt = value;
    }

    /// <summary>
    /// A value of ArsWithDupDisbPmts.
    /// </summary>
    [JsonPropertyName("arsWithDupDisbPmts")]
    public Common ArsWithDupDisbPmts
    {
      get => arsWithDupDisbPmts ??= new();
      set => arsWithDupDisbPmts = value;
    }

    /// <summary>
    /// A value of ReferenceNbr.
    /// </summary>
    [JsonPropertyName("referenceNbr")]
    public WorkArea ReferenceNbr
    {
      get => referenceNbr ??= new();
      set => referenceNbr = value;
    }

    /// <summary>
    /// A value of CollId.
    /// </summary>
    [JsonPropertyName("collId")]
    public WorkArea CollId
    {
      get => collId ??= new();
      set => collId = value;
    }

    /// <summary>
    /// A value of DisbId.
    /// </summary>
    [JsonPropertyName("disbId")]
    public WorkArea DisbId
    {
      get => disbId ??= new();
      set => disbId = value;
    }

    /// <summary>
    /// A value of DisbAmt.
    /// </summary>
    [JsonPropertyName("disbAmt")]
    public TextWorkArea DisbAmt
    {
      get => disbAmt ??= new();
      set => disbAmt = value;
    }

    /// <summary>
    /// A value of DisbProcessDate.
    /// </summary>
    [JsonPropertyName("disbProcessDate")]
    public TextWorkArea DisbProcessDate
    {
      get => disbProcessDate ??= new();
      set => disbProcessDate = value;
    }

    /// <summary>
    /// A value of DisbCreated.
    /// </summary>
    [JsonPropertyName("disbCreated")]
    public TextWorkArea DisbCreated
    {
      get => disbCreated ??= new();
      set => disbCreated = value;
    }

    /// <summary>
    /// A value of ReceiptAmt.
    /// </summary>
    [JsonPropertyName("receiptAmt")]
    public TextWorkArea ReceiptAmt
    {
      get => receiptAmt ??= new();
      set => receiptAmt = value;
    }

    /// <summary>
    /// A value of CummulativeDisbAmt.
    /// </summary>
    [JsonPropertyName("cummulativeDisbAmt")]
    public TextWorkArea CummulativeDisbAmt
    {
      get => cummulativeDisbAmt ??= new();
      set => cummulativeDisbAmt = value;
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
    /// A value of StartingCsePerson.
    /// </summary>
    [JsonPropertyName("startingCsePerson")]
    public CsePerson StartingCsePerson
    {
      get => startingCsePerson ??= new();
      set => startingCsePerson = value;
    }

    /// <summary>
    /// A value of EndingCsePerson.
    /// </summary>
    [JsonPropertyName("endingCsePerson")]
    public CsePerson EndingCsePerson
    {
      get => endingCsePerson ??= new();
      set => endingCsePerson = value;
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
    /// A value of TotDupPmtAmtTxt.
    /// </summary>
    [JsonPropertyName("totDupPmtAmtTxt")]
    public TextWorkArea TotDupPmtAmtTxt
    {
      get => totDupPmtAmtTxt ??= new();
      set => totDupPmtAmtTxt = value;
    }

    private DisbursementTransaction driverDisbursement;
    private CsePerson driverCsePerson;
    private TextWorkArea endingDisbDateTxt;
    private TextWorkArea startingDisbDateTxt;
    private DisbursementTransaction startingDisbursement;
    private DisbursementTransaction endingDisbursement;
    private TextWorkArea potRcvTxt;
    private DisbursementTransaction holdForCrRead;
    private WorkArea ref1;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private Common feeInfoInd;
    private Common traceInd;
    private Common potRecForCr;
    private Common potRecForAr;
    private Common potRecForRun;
    private CsePerson skipCsePerson;
    private DisbursementTransaction skipDisbursement;
    private CsePerson holdCsePerson;
    private DisbursementTransaction holdDisbursement;
    private CashReceiptDetail holdCashReceiptDetail;
    private DisbursementTransaction startingDisbursementTransaction;
    private DisbursementTransaction endingDisbursementTransaction;
    private Common multipleArsForCrdInd;
    private TextWorkArea disbSign;
    private DisbursementTransaction totPosForProcessDate;
    private DateWorkArea initialized;
    private Common totDisbForPerRefNbr;
    private Common disbRecsRead;
    private Common crdsWithDupPmt;
    private Common arsWithDupDisbPmts;
    private WorkArea referenceNbr;
    private WorkArea collId;
    private WorkArea disbId;
    private TextWorkArea disbAmt;
    private TextWorkArea disbProcessDate;
    private TextWorkArea disbCreated;
    private TextWorkArea receiptAmt;
    private TextWorkArea cummulativeDisbAmt;
    private ProgramProcessingInfo programProcessingInfo;
    private CsePerson startingCsePerson;
    private CsePerson endingCsePerson;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private TextWorkArea totDupPmtAmtTxt;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DriverObligee1.
    /// </summary>
    [JsonPropertyName("driverObligee1")]
    public CsePersonAccount DriverObligee1
    {
      get => driverObligee1 ??= new();
      set => driverObligee1 = value;
    }

    /// <summary>
    /// A value of DriverObligee2.
    /// </summary>
    [JsonPropertyName("driverObligee2")]
    public CsePerson DriverObligee2
    {
      get => driverObligee2 ??= new();
      set => driverObligee2 = value;
    }

    /// <summary>
    /// A value of DriverDisbursement.
    /// </summary>
    [JsonPropertyName("driverDisbursement")]
    public DisbursementTransaction DriverDisbursement
    {
      get => driverDisbursement ??= new();
      set => driverDisbursement = value;
    }

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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private CsePersonAccount driverObligee1;
    private CsePerson driverObligee2;
    private DisbursementTransaction driverDisbursement;
    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private DisbursementTransaction disbursement;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction disbCollection;
    private DisbursementType disbursementType;
    private Collection collection;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}

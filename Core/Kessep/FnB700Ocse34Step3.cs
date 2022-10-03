// Program: FN_B700_OCSE34_STEP_3, ID: 373315419, model: 746.
// Short name: SWE02988
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_3.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step3: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step3(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step3.
  /// </summary>
  public FnB700Ocse34Step3(IContext context, Import import, Export export):
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
    // ??/??/??  ????????	????????	Initial Development
    // 12/03/07  GVandy	CQ295		Federally mandated changes to OCSE34 report.
    // 01/06/09  GVandy	CQ486		Enhance audit trail to determine why part 1 and 
    // part 2 of the
    // 					OCSE34 report do not balance.
    // 10/14/12  GVandy			Emergency fix to expand foreign group view size
    // -----------------------------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreate.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreate.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();
    local.Max.Date = new DateTime(2099, 12, 31);

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "03"))
    {
      // CMJ 02/18/05 FOR RESTARTING PURPOSES
      local.RestartPaymentRequest.SystemGeneratedIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 4, 9));
      UseFnB700BuildGvForRestart();
    }

    // This program is for advanced refunds and procan refunds.  advance refunds
    // are on the payment request table labeled as "ADV" classification.    THe
    // 'REF' refunds must be in a CANRESET and must relate back to a PROCAN
    // reason code.
    foreach(var item in ReadPaymentRequest())
    {
      if (Equal(entities.PaymentRequest.Classification, "ADV"))
      {
        // advance refunds due not have cash receipts.  Only need to fine 
        // receipt refunds and collection type
        if (ReadReceiptRefundCollectionType())
        {
          // skip some collection types  only want f,s   which is 3 and 4
          if (entities.CollectionType.SequentialIdentifier == 3 || entities
            .CollectionType.SequentialIdentifier == 4)
          {
          }
          else
          {
            continue;
          }

          // subtract monies to total-currency and continue only for advances
          local.Common.TotalCurrency = -entities.PaymentRequest.Amount;
          MoveOcse157Verification2(local.Null1, local.ForCreate);
          UseFnB700MaintainLine2Totals2();

          if (IsEmpty(local.ForCreate.LineNumber))
          {
            local.EabReportSend.RptDetail =
              "Unclassified collection - Step 3 (1), Amount = " + NumberToString
              ((long)(local.Common.TotalCurrency * 100), 15);

            if (local.Common.TotalCurrency < 0)
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "-";
            }

            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", Payment Request ID = " +
              NumberToString
              (entities.PaymentRequest.SystemGeneratedIdentifier, 7, 9);
            UseCabErrorReport();

            continue;
          }

          local.ForCreate.Comment = "B700 Step 3 (1)";
        }
        else
        {
          continue;

          // cash receipt source type will be blank
        }
      }
      else
      {
        // CHeck for Canreset (id = 28)  for refund  no collections involved 
        // with procan
        if (Equal(entities.PaymentRequest.Classification, "REF") && Equal
          (entities.PaymentRequest.Type1, "WAR"))
        {
          if (ReadPaymentStatusPaymentStatusHistory())
          {
            // continue processing
            local.PaymentRequest.CsePersonNumber =
              entities.PaymentRequest.CsePersonNumber;
          }
          else
          {
            continue;
          }

          if (ReadReceiptRefund())
          {
            // continue
          }
          else
          {
            continue;
          }

          if (ReadCashReceiptDetailCollectionType())
          {
            // continue processing
            // skip some collection types  only want f,s ,u,i,a,c,k,t,v,y z
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

          if (ReadCashReceiptCashReceiptType())
          {
            // continue processing,but skip non-cash Cash receipts types.
            if (entities.CashReceiptType.SystemGeneratedIdentifier == 2 || entities
              .CashReceiptType.SystemGeneratedIdentifier == 7 || entities
              .CashReceiptType.SystemGeneratedIdentifier == 8 || entities
              .CashReceiptType.SystemGeneratedIdentifier == 11 || entities
              .CashReceiptType.SystemGeneratedIdentifier == 13 || entities
              .CashReceiptType.SystemGeneratedIdentifier == 14)
            {
              continue;
            }
          }
          else
          {
            continue;
          }

          if (ReadCashReceiptEventCashReceiptSourceType())
          {
            // continue processing
          }
          else
          {
            continue;
          }

          if (ReadCashReceiptDetailStatusCashReceiptDetailStatHistory())
          {
            // CONTINUE PROCESSING
          }
          else
          {
            continue;
          }

          // should it be payment request amount or refund-amount
          local.Common.TotalCurrency = entities.PaymentRequest.Amount * 1;
          MoveOcse157Verification2(local.Null1, local.ForCreate);
          UseFnB700MaintainLine2Totals1();

          if (IsEmpty(local.ForCreate.LineNumber))
          {
            local.EabReportSend.RptDetail =
              "Unclassified collection - Step 3 (2), Amount = " + NumberToString
              ((long)(local.Common.TotalCurrency * 100), 15);

            if (local.Common.TotalCurrency < 0)
            {
              local.EabReportSend.RptDetail =
                TrimEnd(local.EabReportSend.RptDetail) + "-";
            }

            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + ", CRD ID = " + NumberToString
              (entities.CashReceipt.SequentialNumber, 9, 7) + "-" + NumberToString
              (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
            UseCabErrorReport();

            continue;
          }

          local.ForCreate.Comment = "B700 Step 3 (2)";
          local.ForCreate.CaseWorkerName =
            NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
          local.ForCreate.CaseWorkerName =
            TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        }
        else
        {
          continue;
        }
      }

      // set ocse157 for processing
      if (AsChar(import.DisplayInd.Flag) != 'A')
      {
        local.ForCreate.ObligorPersonNbr =
          entities.PaymentRequest.CsePersonNumber;
        local.ForCreate.CollectionDte = entities.PaymentRequest.PrintDate;
        local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
        UseFnCreateOcse157Verification();
      }

      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --------------------------------------------
        // Apply updates to Database from GV.
        // --------------------------------------------
        UseFnB700ApplyUpdates();
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "03";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.PaymentRequest.SystemGeneratedIdentifier, 9, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "03";
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // final commit
    UseFnB700ApplyUpdates();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "04" + "  ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "03";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB700MaintainLine2Totals.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(Import.GroupGroup source,
    FnB700ApplyUpdates.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup3(FnB700BuildGvForRestart.Export.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup4(FnB700MaintainLine2Totals.Import.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveOcse157Verification1(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.FiscalYear = source.FiscalYear;
    target.RunNumber = source.RunNumber;
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.CaseNumber = source.CaseNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObTranSgi = source.ObTranSgi;
    target.ObTranAmount = source.ObTranAmount;
    target.ObligationSgi = source.ObligationSgi;
    target.DebtAdjType = source.DebtAdjType;
    target.DebtDetailDueDt = source.DebtDetailDueDt;
    target.DebtDetailBalanceDue = source.DebtDetailBalanceDue;
    target.ObTypeSgi = source.ObTypeSgi;
    target.CollectionSgi = source.CollectionSgi;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollApplToCode = source.CollApplToCode;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.CaseAsinEffDte = source.CaseAsinEffDte;
    target.CaseAsinEndDte = source.CaseAsinEndDte;
    target.IntRequestIdent = source.IntRequestIdent;
    target.KansasCaseInd = source.KansasCaseInd;
    target.PersonProgCode = source.PersonProgCode;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse34(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOutgoingForeign(Import.OutgoingForeignGroup source,
    FnB700MaintainLine2Totals.Import.OutgoingForeignGroup target)
  {
    target.GimportOutgoingForeign.StandardNumber =
      source.GimportOutgoingForeign.StandardNumber;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB700ApplyUpdates()
  {
    var useImport = new FnB700ApplyUpdates.Import();
    var useExport = new FnB700ApplyUpdates.Export();

    useImport.Ocse34.Assign(import.Ocse34);
    import.Group.CopyTo(useImport.Group, MoveGroup2);

    Call(FnB700ApplyUpdates.Execute, useImport, useExport);
  }

  private void UseFnB700BuildGvForRestart()
  {
    var useImport = new FnB700BuildGvForRestart.Import();
    var useExport = new FnB700BuildGvForRestart.Export();

    MoveOcse34(import.Ocse34, useImport.Ocse34);

    Call(FnB700BuildGvForRestart.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup3);
  }

  private void UseFnB700MaintainLine2Totals1()
  {
    var useImport = new FnB700MaintainLine2Totals.Import();
    var useExport = new FnB700MaintainLine2Totals.Export();

    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    MoveCashReceiptSourceType(entities.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;
    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CashReceiptDetail.CourtOrderNumber;
    import.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign);

    Call(FnB700MaintainLine2Totals.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup4);
    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnB700MaintainLine2Totals2()
  {
    var useImport = new FnB700MaintainLine2Totals.Import();
    var useExport = new FnB700MaintainLine2Totals.Export();

    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    MoveCashReceiptSourceType(entities.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;

    Call(FnB700MaintainLine2Totals.Execute, useImport, useExport);

    useImport.Group.CopyTo(import.Group, MoveGroup4);
    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    local.ForCreate.LineNumber = useExport.Ocse157Verification.LineNumber;
  }

  private void UseFnCreateOcse157Verification()
  {
    var useImport = new FnCreateOcse157Verification.Import();
    var useExport = new FnCreateOcse157Verification.Export();

    MoveOcse157Verification1(local.ForCreate, useImport.Ocse157Verification);

    Call(FnCreateOcse157Verification.Execute, useImport, useExport);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    MoveOcse157Verification3(local.ForError, useImport.Ocse157Verification);

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptType.Populated = true;
        entities.CashReceipt.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.ReceiptRefund.Populated);
    entities.CollectionType.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCollectionType",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", local.PaymentRequest.CsePersonNumber ?? ""
          );
        db.SetInt32(
          command, "crdId",
          entities.ReceiptRefund.CrdIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crvIdentifier",
          entities.ReceiptRefund.CrvIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "cstIdentifier",
          entities.ReceiptRefund.CstIdentifier.GetValueOrDefault());
        db.SetInt32(
          command, "crtIdentifier",
          entities.ReceiptRefund.CrtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 8);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 13);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 13);
        entities.CollectionType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailStatusCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatHistory.Populated = false;
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatusCashReceiptDetailStatHistory",
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
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatHistory.CdsIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailStatHistory.CrdIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailStatHistory.CrvIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailStatHistory.CstIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CrtIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CashReceiptDetailStatHistory.ReasonCodeId =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.CashReceiptDetailStatHistory.Populated = true;
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentRequest()
  {
    entities.PaymentRequest.Populated = false;

    return ReadEach("ReadPaymentRequest",
      (db, command) =>
      {
        db.
          SetDate(command, "date1", import.ReportStart.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.ReportEnd.Date.GetValueOrDefault());
        db.SetInt32(
          command, "paymentRequestId",
          local.RestartPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.Classification = db.GetString(reader, 3);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.RctRTstamp = db.GetNullableDateTime(reader, 6);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 7);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private bool ReadPaymentStatusPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatusPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.
          SetDate(command, "date1", import.ReportStart.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.ReportEnd.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 3);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 4);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 5);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 6);
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadReceiptRefund()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PaymentRequest.RctRTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 1);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 6);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadReceiptRefundCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentRequest.Populated);
    entities.CollectionType.Populated = false;
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefundCollectionType",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.PaymentRequest.RctRTstamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 1);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CltIdentifier = db.GetNullableInt32(reader, 4);
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 4);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 6);
        entities.CollectionType.Populated = true;
        entities.ReceiptRefund.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>A OutgoingForeignGroup group.</summary>
    [Serializable]
    public class OutgoingForeignGroup
    {
      /// <summary>
      /// A value of GimportOutgoingForeign.
      /// </summary>
      [JsonPropertyName("gimportOutgoingForeign")]
      public LegalAction GimportOutgoingForeign
      {
        get => gimportOutgoingForeign ??= new();
        set => gimportOutgoingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction gimportOutgoingForeign;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public CashReceipt To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public CashReceipt From
    {
      get => from ??= new();
      set => from = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    /// <summary>
    /// A value of ReportStart.
    /// </summary>
    [JsonPropertyName("reportStart")]
    public DateWorkArea ReportStart
    {
      get => reportStart ??= new();
      set => reportStart = value;
    }

    /// <summary>
    /// A value of ReportEnd.
    /// </summary>
    [JsonPropertyName("reportEnd")]
    public DateWorkArea ReportEnd
    {
      get => reportEnd ??= new();
      set => reportEnd = value;
    }

    /// <summary>
    /// A value of DisplayInd.
    /// </summary>
    [JsonPropertyName("displayInd")]
    public Common DisplayInd
    {
      get => displayInd ??= new();
      set => displayInd = value;
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
    /// Gets a value of OutgoingForeign.
    /// </summary>
    [JsonIgnore]
    public Array<OutgoingForeignGroup> OutgoingForeign =>
      outgoingForeign ??= new(OutgoingForeignGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of OutgoingForeign for json serialization.
    /// </summary>
    [JsonPropertyName("outgoingForeign")]
    [Computed]
    public IList<OutgoingForeignGroup> OutgoingForeign_Json
    {
      get => outgoingForeign;
      set => OutgoingForeign.Assign(value);
    }

    private CashReceipt to;
    private CashReceipt from;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<GroupGroup> group;
    private DateWorkArea reportStart;
    private DateWorkArea reportEnd;
    private Common displayInd;
    private Ocse157Verification ocse157Verification;
    private Ocse34 ocse34;
    private Array<OutgoingForeignGroup> outgoingForeign;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroDivide.
    /// </summary>
    [JsonPropertyName("zeroDivide")]
    public Common ZeroDivide
    {
      get => zeroDivide ??= new();
      set => zeroDivide = value;
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
    /// A value of RestartPaymentStatus.
    /// </summary>
    [JsonPropertyName("restartPaymentStatus")]
    public PaymentStatus RestartPaymentStatus
    {
      get => restartPaymentStatus ??= new();
      set => restartPaymentStatus = value;
    }

    /// <summary>
    /// A value of RestartPaymentRequest.
    /// </summary>
    [JsonPropertyName("restartPaymentRequest")]
    public PaymentRequest RestartPaymentRequest
    {
      get => restartPaymentRequest ??= new();
      set => restartPaymentRequest = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of RestartCashReceipt.
    /// </summary>
    [JsonPropertyName("restartCashReceipt")]
    public CashReceipt RestartCashReceipt
    {
      get => restartCashReceipt ??= new();
      set => restartCashReceipt = value;
    }

    /// <summary>
    /// A value of RestartCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("restartCashReceiptDetail")]
    public CashReceiptDetail RestartCashReceiptDetail
    {
      get => restartCashReceiptDetail ??= new();
      set => restartCashReceiptDetail = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Ocse157Verification Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common zeroDivide;
    private PaymentRequest paymentRequest;
    private PaymentStatus restartPaymentStatus;
    private PaymentRequest restartPaymentRequest;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea max;
    private CashReceipt restartCashReceipt;
    private CashReceiptDetail restartCashReceiptDetail;
    private Common common;
    private Ocse157Verification null1;
    private Ocse157Verification forCreate;
    private Common commitCnt;
    private Ocse157Verification forError;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
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
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private PaymentRequest paymentRequest;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private CashReceiptType cashReceiptType;
    private CollectionType collectionType;
    private CashReceiptDetail cashReceiptDetail;
    private ReceiptRefund receiptRefund;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
  }
#endregion
}

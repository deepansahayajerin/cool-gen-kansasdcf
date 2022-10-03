// Program: FN_B700_OCSE34_STEP_2, ID: 373315424, model: 746.
// Short name: SWE02987
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_2.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step2.
  /// </summary>
  public FnB700Ocse34Step2(IContext context, Import import, Export export):
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
    // 01/06/09  GVandy	CQ486		1) Enhance audit trail to determine why part 1 
    // and part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Cash receipt detail amount was being reported multiple times;
    // 					   once for each status history record of adjusted or refunded.
    // 					3) Accomodate federal FDSO adjustments/refunds.
    // 					4) Ignore refunds where the warrant issued is in CANRESET status.
    // 					5) The maximum adjustment amount to be reported is the
    // 					   CRD collection amount minus refund amounts.
    // 					6) Performance changes
    // 					7) Checkpoint/Restart changes to accomodate 2 main READ EACHs.
    // 03/18/10  GVandy	CQ15723		Subtract IVD Non-IWO amount provided by the KPC
    // from line 2H.
    // 10/14/12  GVandy			Emergency fix to expand foreign group view size
    // -----------------------------------------------------------------------------------------------------
    // ************************************************************************************
    // *** Step 2 - Subtract adjustments and refunds that occurred during the 
    // reporting period.
    // ************************************************************************************
    local.ProgramCheckpointRestart.ProgramName =
      import.ProgramCheckpointRestart.ProgramName;
    local.ForCreate.FiscalYear =
      import.Ocse157Verification.FiscalYear.GetValueOrDefault();
    local.ForCreate.RunNumber =
      import.Ocse157Verification.RunNumber.GetValueOrDefault();

    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "02"))
    {
      // CQ486  Changes to checkpoint/restart to accomodate having 2 main READ 
      // EACHs in this cab.
      if (CharAt(import.ProgramCheckpointRestart.RestartInfo, 16) == '1')
      {
        // -- We're restarting in part 1 of this cab
        local.RestartingInPart2.Flag = "N";
      }
      else if (CharAt(import.ProgramCheckpointRestart.RestartInfo, 16) == '2')
      {
        // -- We're restarting in part 2 of this cab
        local.RestartingInPart2.Flag = "Y";
      }

      local.RestartCashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 3, 7));
      local.RestartCashReceiptDetail.SequentialIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 11, 4));
      UseFnB700BuildGvForRestart();
    }
    else
    {
      // -- We're not restarting.
      local.RestartingInPart2.Flag = "N";
    }

    if (AsChar(local.RestartingInPart2.Flag) == 'N')
    {
      // -- CQ486  New to read refunds for all CRDs (not just FDSO).  Then after
      // that we'll read CRD Adjustments
      //    and compensate for all the refunds issued against that CRD.
      foreach(var item in ReadCashReceiptDetailCashReceiptEventCashReceiptSourceType())
        
      {
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
          // *** Ok.
        }

        // -- CQ486  Insure that warrant that issued the refund is not in 
        // CANRESET status.
        if (ReadPaymentStatusHistory())
        {
          // -- The refund warrant is in CANRESET status.  (i.e. the warrant was
          // cancelled and the
          //    cash receipt detail was released for distribution)  Skip this 
          // refund.
          continue;
        }

        local.Common.TotalCurrency = 0;

        // *** Reduce the collection amount by any refunds active during the 
        // reporting period.
        if (entities.ReceiptRefund.Amount > entities
          .CashReceiptDetail.CollectionAmount)
        {
          // -- Do not report an advance amount that exceeds the cash receipt 
          // detail collection amount.
          local.Common.TotalCurrency =
            -entities.CashReceiptDetail.CollectionAmount;
        }
        else
        {
          local.Common.TotalCurrency = -entities.ReceiptRefund.Amount;
        }

        MoveOcse157Verification2(local.Null1, local.ForCreate);
        UseFnB700MaintainLine2Totals();

        if (IsEmpty(local.ForCreate.LineNumber))
        {
          local.EabReportSend.RptDetail =
            "Unclassified collection - Step 2, Amount = " + NumberToString
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

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreate.ObligorPersonNbr =
            entities.CashReceiptDetail.ObligorPersonNumber;
          local.ForCreate.CourtOrderNumber =
            entities.CashReceiptDetail.CourtOrderNumber;
          local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
          local.ForCreate.CollectionDte =
            entities.CashReceiptDetail.CollectionDate;

          // *** Adjusted date
          local.ForCreate.CollCreatedDte =
            Date(entities.ReceiptRefund.CreatedTimestamp);

          // *** Build CRD #
          local.ForCreate.CaseWorkerName =
            NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
          local.ForCreate.CaseWorkerName =
            TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
          local.ForCreate.Comment = "B700 Step 2 (1)";
          UseFnCreateOcse157Verification();
          ++local.CommitCnt.Count;
        }

        // --------------------------------------------
        // Check commit counts.
        // --------------------------------------------
        if (local.CommitCnt.Count >= import
          .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
        {
          // --------------------------------------------
          // Apply updates to Database from GV.
          // --------------------------------------------
          UseFnB700ApplyUpdates();
          local.CommitCnt.Count = 0;
          local.ProgramCheckpointRestart.RestartInd = "Y";
          local.ProgramCheckpointRestart.RestartInfo = "02";
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (entities.CashReceipt.SequentialNumber, 9, 7);
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);

          // CQ486  Changes to checkpoint/restart to accomodate having 2 main 
          // READ EACHs in this cab.
          local.ProgramCheckpointRestart.RestartInfo =
            TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " 1";
          UseUpdateCheckpointRstAndCommit();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            local.ForError.LineNumber = "02";
            UseOcse157WriteError();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.Abort.Flag = "Y";

              return;
            }
          }
        }
      }
    }

    // CQ486  Changes to checkpoint/restart to accomodate having 2 main READ 
    // EACHs in this cab.
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "02") && CharAt
      (import.ProgramCheckpointRestart.RestartInfo, 16) == '1')
    {
      // -- We restarted in step 1.  Re-initialize the restart views before 
      // processing part 2.
      local.RestartCashReceipt.SequentialNumber = 0;
      local.RestartCashReceiptDetail.SequentialIdentifier = 0;
    }

    // --CQ486  Report CRD adjustments (not to exceed the CRD collection amount 
    // minus previously issued refunds).
    // -- CQ486  Performance changes.
    foreach(var item in ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt())
    {
      if (entities.CashReceipt.SequentialNumber == local
        .PreviousCashReceipt.SequentialNumber && entities
        .CashReceiptDetail.SequentialIdentifier == local
        .PreviousCashReceiptDetail.SequentialIdentifier)
      {
        continue;
      }
      else
      {
        local.PreviousCashReceipt.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        local.PreviousCashReceiptDetail.SequentialIdentifier =
          entities.CashReceiptDetail.SequentialIdentifier;
      }

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
        // *** Ok.
      }

      local.Common.TotalCurrency = -entities.CashReceiptDetail.CollectionAmount;

      // *** Reduce the adjustment amount by any refunds processed through the 
      // end of the current reporting period.
      foreach(var item1 in ReadReceiptRefund())
      {
        local.Common.TotalCurrency += entities.ReceiptRefund.Amount;
      }

      if (local.Common.TotalCurrency >= 0)
      {
        // -- Refund amounts equaling or exceeding the CRD collection amount 
        // have already been reported.  Don't report anything for the
        // adjustment.
        continue;
      }

      MoveOcse157Verification2(local.Null1, local.ForCreate);
      UseFnB700MaintainLine2Totals();

      if (IsEmpty(local.ForCreate.LineNumber))
      {
        local.EabReportSend.RptDetail =
          "Unclassified collection - Step 2, Amount = " + NumberToString
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

      if (AsChar(import.DisplayInd.Flag) == 'Y')
      {
        local.ForCreate.ObligorPersonNbr =
          entities.CashReceiptDetail.ObligorPersonNumber;
        local.ForCreate.CourtOrderNumber =
          entities.CashReceiptDetail.CourtOrderNumber;
        local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
        local.ForCreate.CollectionDte =
          entities.CashReceiptDetail.CollectionDate;

        // *** Adjusted date
        local.ForCreate.CollCreatedDte =
          Date(entities.CashReceiptDetailStatHistory.CreatedTimestamp);

        // *** Build CRD #
        local.ForCreate.CaseWorkerName =
          NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
        local.ForCreate.CaseWorkerName =
          TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        local.ForCreate.Comment = "B700 Step 2 (2)";
        UseFnCreateOcse157Verification();
        ++local.CommitCnt.Count;
      }

      // --------------------------------------------
      // Check commit counts.
      // --------------------------------------------
      if (local.CommitCnt.Count >= import
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --------------------------------------------
        // Apply updates to Database from GV.
        // --------------------------------------------
        UseFnB700ApplyUpdates();
        local.CommitCnt.Count = 0;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = "02";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceipt.SequentialNumber, 9, 7);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);

        // CQ486  Changes to checkpoint/restart to accomodate having 2 main READ
        // EACHs in this cab.
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " 2";
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "02";
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }
    }

    // ***************************************************************************
    // **                    Compute line 2 final amounts.
    // **
    // ** At this point all cash receipts, adjustments and refunds have been 
    // processed
    // **  for the reporting period.  The  final step in line 2 calculations 
    // will be to
    // **  add the applicable KPC amounts to our computed amounts.
    // ***************************************************************************
    // *****   Set line 2E amount.
    import.Group.Index = 6;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import
      .Ocse34.KpcNon4DIwoCollAmt.GetValueOrDefault() + import
      .Ocse34.KpcUiNonIvdIwoAmt.GetValueOrDefault();

    // *****   Set line 2F amount.
    import.Group.Index = 7;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import
      .Ocse34.KpcIvdNonIwoCollAmt.GetValueOrDefault() + import
      .Ocse34.UiIvdNonIwoIntAmt.GetValueOrDefault();

    // *****   Set line 2H amount.
    import.Group.Index = 8;
    import.Group.CheckSize();

    // 03/18/10  GVandy  CQ15723  Since KAECSES cannot identify non-iwo payments
    // received from other states, the KPC provides
    // the IVD Non-IWO amount which we include in line 2F.  The processing is 
    // Step 1 and Step 2 pick up the same cash receipt
    // details which are ultimately misclassified and included in Line 2H, 
    // thereby double counting these payments.  Therefore,
    // we need to subtract the IVD Non-IWO amount provided by the KPC from line 
    // 2H to correct this problem.
    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import
      .Ocse34.KpcUiIvdNonIwoNonIntAmt.GetValueOrDefault() - import
      .Ocse34.KpcIvdNonIwoCollAmt.GetValueOrDefault();
    UseFnB700ApplyUpdates();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "03" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "02";
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.Column = source.Column;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CollCreatedDte = source.CollCreatedDte;
    target.CaseWorkerName = source.CaseWorkerName;
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

    import.Group.CopyTo(useImport.Group, MoveGroup2);
    useImport.Ocse34.Assign(import.Ocse34);

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

  private void UseFnB700MaintainLine2Totals()
  {
    var useImport = new FnB700MaintainLine2Totals.Import();
    var useExport = new FnB700MaintainLine2Totals.Export();

    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    MoveCashReceiptSourceType(entities.CashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CashReceiptDetail.CourtOrderNumber;
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    import.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign);
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

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptEventCashReceiptSourceType()
  {
    entities.ReceiptRefund.Populated = false;
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach(
      "ReadCashReceiptDetailCashReceiptEventCashReceiptSourceType",
      (db, command) =>
      {
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEnd.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "cashReceiptId", local.RestartCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crdId",
          local.RestartCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "sequentialNumber1", import.From.SequentialNumber);
        db.SetInt32(command, "sequentialNumber2", import.To.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 3);
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
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 15);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 16);
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 17);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 18);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 19);
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptSourceTypeCashReceipt",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTmst",
          import.ReportEnd.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp",
          import.ReportStart.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "cashReceiptId", local.RestartCashReceipt.SequentialNumber);
        db.SetInt32(
          command, "crdId",
          local.RestartCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "sequentialNumber1", import.From.SequentialNumber);
        db.SetInt32(command, "sequentialNumber2", import.To.SequentialNumber);
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
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 15);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 16);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;

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
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", import.ReportEnd.Date.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "rctRTstamp",
          entities.ReceiptRefund.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.Populated = true;
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
          import.ReportEnd.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.ReasonCode = db.GetString(reader, 1);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 3);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 6);
        entities.ReceiptRefund.Populated = true;

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
    /// A value of ReportEnd.
    /// </summary>
    [JsonPropertyName("reportEnd")]
    public DateWorkArea ReportEnd
    {
      get => reportEnd ??= new();
      set => reportEnd = value;
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
    private DateWorkArea reportEnd;
    private DateWorkArea reportStart;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<GroupGroup> group;
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
    /// A value of RestartingInPart2.
    /// </summary>
    [JsonPropertyName("restartingInPart2")]
    public Common RestartingInPart2
    {
      get => restartingInPart2 ??= new();
      set => restartingInPart2 = value;
    }

    /// <summary>
    /// A value of PreviousCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("previousCashReceiptDetail")]
    public CashReceiptDetail PreviousCashReceiptDetail
    {
      get => previousCashReceiptDetail ??= new();
      set => previousCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of PreviousCashReceipt.
    /// </summary>
    [JsonPropertyName("previousCashReceipt")]
    public CashReceipt PreviousCashReceipt
    {
      get => previousCashReceipt ??= new();
      set => previousCashReceipt = value;
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
    /// A value of RestartCashReceipt.
    /// </summary>
    [JsonPropertyName("restartCashReceipt")]
    public CashReceipt RestartCashReceipt
    {
      get => restartCashReceipt ??= new();
      set => restartCashReceipt = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
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
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public Ocse157Verification ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
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
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
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

    private Common restartingInPart2;
    private CashReceiptDetail previousCashReceiptDetail;
    private CashReceipt previousCashReceipt;
    private CashReceiptDetail restartCashReceiptDetail;
    private CashReceipt restartCashReceipt;
    private Common common;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forError;
    private DateWorkArea reportStart;
    private DateWorkArea reportEnd;
    private Ocse157Verification forCreate;
    private Ocse157Verification null1;
    private Common commitCnt;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest paymentRequest;
    private ReceiptRefund receiptRefund;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
  }
#endregion
}

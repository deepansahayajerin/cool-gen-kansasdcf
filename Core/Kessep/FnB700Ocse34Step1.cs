// Program: FN_B700_OCSE34_STEP_1, ID: 373315426, model: 746.
// Short name: SWE02986
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_1.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step1: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_1 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step1(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step1.
  /// </summary>
  public FnB700Ocse34Step1(IContext context, Import import, Export export):
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
    // 12/05/03  SWSRESS	WR040134	Federally mandated changes to OCSE34 report.
    // 12/03/07  GVandy	CQ295		Federally mandated changes to OCSE34 report.
    // 01/06/09  GVandy	CQ486		1) Enhance audit trail to determine why part 1 
    // and part 2 of the
    // 					   OCSE34 report do not balance.
    // 					2) Account for cash receipt details without status history
    // 					   records effective at the end of the quarter.
    // 					3) Don't exclude cash receipt details in adjusted status.
    // 					4) Subtract advance refunds.
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
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "01"))
    {
      local.RestartCashReceipt.SequentialNumber =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 3, 7));
      local.RestartCashReceiptDetail.SequentialIdentifier =
        (int)StringToNumber(Substring(
          import.ProgramCheckpointRestart.RestartInfo, 250, 11, 4));
      UseFnB700BuildGvForRestart();
    }

    // -- CQ486   Added adjustment_ind <> 'Y' and removed 'and that 
    // cash_receipt_detail_status system_generated_identifer is not equal to 2'
    // Need to include the CRD amount as a positive value even if the current 
    // status is adjusted.  There will be a corresponding negative value
    // included in step 2.  Those with adjustment_ind = 'y' are the court
    // adjustments that cause the status on the original CRD to change to
    // adjusted.
    foreach(var item in ReadCashReceiptCashReceiptDetailCashReceiptSourceType())
    {
      if (!ReadCollectionType())
      {
        // *** Ok.
      }

      // -- CQ486 Changes in the read each above will prevent ever having 
      // adjustment_ind = 'y'.  The if can be removed... leave the else logic...
      if (AsChar(entities.CashReceiptDetail.AdjustmentInd) == 'Y')
      {
        local.Common.TotalCurrency =
          -entities.CashReceiptDetail.CollectionAmount;
      }
      else
      {
        local.Common.TotalCurrency =
          entities.CashReceiptDetail.CollectionAmount;
      }

      MoveOcse157Verification3(local.Null1, local.ForCreate);
      UseFnB700MaintainLine2Totals();

      if (IsEmpty(local.ForCreate.LineNumber))
      {
        local.EabReportSend.RptDetail =
          "Unclassified collection - Step 1, Amount = " + NumberToString
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
        // -- CQ486  Changes in the read each above will prevent ever having 
        // adjustment_ind = 'y'.  The if can be removed... leave the else logic
        // ...
        if (AsChar(entities.CashReceiptDetail.AdjustmentInd) == 'Y')
        {
          // -- CQ486  Find the original cash receipt detail which is adjusted 
          // by this court adjustment.
          if (ReadCashReceiptDetailCashReceipt())
          {
            local.ForCreate.CollectionDte =
              entities.AdjustedCashReceiptDetail.CollectionDate;
            local.ForCreate.ObligorPersonNbr =
              entities.AdjustedCashReceiptDetail.ObligorPersonNumber;
            local.ForCreate.CourtOrderNumber =
              entities.AdjustedCashReceiptDetail.CourtOrderNumber;

            // *** Build CRD #
            local.ForCreate.CaseWorkerName =
              NumberToString(entities.AdjustedCashReceipt.SequentialNumber, 9, 7)
              + "-";
            local.ForCreate.CaseWorkerName =
              TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
              (entities.AdjustedCashReceiptDetail.SequentialIdentifier, 12, 4);
          }
          else
          {
            export.Abort.Flag = "Y";
            ExitState = "FN0000_ADJUSTED_CRD_NF_ABORT";

            return;
          }
        }
        else
        {
          local.ForCreate.CollectionDte =
            entities.CashReceiptDetail.CollectionDate;
          local.ForCreate.ObligorPersonNbr =
            entities.CashReceiptDetail.ObligorPersonNumber;
          local.ForCreate.CourtOrderNumber =
            entities.CashReceiptDetail.CourtOrderNumber;

          // *** Build CRD #
          local.ForCreate.CaseWorkerName =
            NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
          local.ForCreate.CaseWorkerName =
            TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        }

        local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
        local.ForCreate.Comment = "B700 Step 1 (1)";
        UseFnCreateOcse157Verification();
        ++local.CommitCnt.Count;
      }

      // -- CQ486  Subtract advance refunds.  Do NOT check created timestamps, 
      // we want to subtract the advance
      //     refund when the cash receipt detail is received even if the advance
      // refund was done in a prior quarter.
      foreach(var item1 in ReadReceiptRefund())
      {
        if (entities.ReceiptRefund.Amount > entities
          .CashReceiptDetail.CollectionAmount)
        {
          // -- Do not report an advance refund amount that exceeds the cash 
          // receipt detail collection amount.
          local.Common.TotalCurrency =
            -entities.CashReceiptDetail.CollectionAmount;
        }
        else
        {
          local.Common.TotalCurrency = -entities.ReceiptRefund.Amount;
        }

        MoveOcse157Verification3(local.Null1, local.ForCreate);
        UseFnB700MaintainLine2Totals();

        if (IsEmpty(local.ForCreate.LineNumber))
        {
          local.EabReportSend.RptDetail =
            "Unclassified collection - Step 1a, Amount = " + NumberToString
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

          goto ReadEach;
        }

        if (AsChar(import.DisplayInd.Flag) == 'Y')
        {
          local.ForCreate.CollectionDte =
            entities.CashReceiptDetail.CollectionDate;
          local.ForCreate.ObligorPersonNbr =
            entities.CashReceiptDetail.ObligorPersonNumber;
          local.ForCreate.CourtOrderNumber =
            entities.CashReceiptDetail.CourtOrderNumber;

          // *** Build CRD #
          local.ForCreate.CaseWorkerName =
            NumberToString(entities.CashReceipt.SequentialNumber, 9, 7) + "-";
          local.ForCreate.CaseWorkerName =
            TrimEnd(local.ForCreate.CaseWorkerName) + NumberToString
            (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
          local.ForCreate.CollectionAmount = local.Common.TotalCurrency;
          local.ForCreate.Comment = "B700 Step 1 (1a)";
          UseFnCreateOcse157Verification();
          ++local.CommitCnt.Count;
        }
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
        local.ProgramCheckpointRestart.RestartInfo = "01";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceipt.SequentialNumber, 9, 7);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + "-";
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + NumberToString
          (entities.CashReceiptDetail.SequentialIdentifier, 12, 4);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ForError.LineNumber = "01";
          UseOcse157WriteError();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.Abort.Flag = "Y";

            return;
          }
        }
      }

ReadEach:
      ;
    }

    import.Group.Index = 0;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Ocse34.PreviousUndistribAmount;

    import.Group.Index = 9;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Ocse34.AdjustmentsAmount.GetValueOrDefault();

    import.Group.Index = 37;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Ocse34.UndistributedAmount.GetValueOrDefault();

    import.Group.Index = 45;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      import.Ocse34.IncentivePaymentAmount.GetValueOrDefault();
    UseFnB700ApplyUpdates();
    local.ProgramCheckpointRestart.RestartInd = "Y";
    local.ProgramCheckpointRestart.RestartInfo = "02" + " ";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "01";
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
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
  }

  private static void MoveOcse157Verification2(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.SuppPersonNumber = source.SuppPersonNumber;
  }

  private static void MoveOcse157Verification3(Ocse157Verification source,
    Ocse157Verification target)
  {
    target.LineNumber = source.LineNumber;
    target.ObligorPersonNbr = source.ObligorPersonNbr;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDte = source.CollectionDte;
    target.CaseWorkerName = source.CaseWorkerName;
    target.Comment = source.Comment;
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
      source.GlocalOutgoingForeign.StandardNumber;
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

    MoveOcse34(import.Ocse34, useImport.Ocse34);
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

  private void UseFnB700MaintainLine2Totals()
  {
    var useImport = new FnB700MaintainLine2Totals.Import();
    var useExport = new FnB700MaintainLine2Totals.Export();

    useImport.CashReceiptDetail.CourtOrderNumber =
      entities.CashReceiptDetail.CourtOrderNumber;
    import.OutgoingForeign.
      CopyTo(useImport.OutgoingForeign, MoveOutgoingForeign);
    useImport.Common.TotalCurrency = local.Common.TotalCurrency;
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.CollectionType.SequentialIdentifier =
      entities.CollectionType.SequentialIdentifier;
    MoveCashReceiptSourceType(entities.CashReceiptSourceType,
      useImport.CashReceiptSourceType);

    Call(FnB700MaintainLine2Totals.Execute, useImport, useExport);

    local.Common.TotalCurrency = useImport.Common.TotalCurrency;
    useImport.Group.CopyTo(import.Group, MoveGroup4);
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

    MoveOcse157Verification2(local.ForError, useImport.Ocse157Verification);

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
    ReadCashReceiptCashReceiptDetailCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;
    entities.CashReceiptDetail.Populated = false;
    entities.CashReceipt.Populated = false;

    return ReadEach("ReadCashReceiptCashReceiptDetailCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", local.RestartCashReceipt.SequentialNumber);
        db.
          SetDate(command, "date1", import.ReportStart.Date.GetValueOrDefault());
          
        db.SetDate(command, "date2", import.ReportEnd.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp1",
          import.ReportStart.Timestamp.GetValueOrDefault());
        db.SetDateTime(
          command, "timestamp2",
          import.ReportEnd.Timestamp.GetValueOrDefault());
        db.SetInt32(
          command, "crdId",
          local.RestartCashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "sequentialNumber1", import.From.SequentialNumber);
        db.SetInt32(command, "sequentialNumber2", import.To.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 7);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 11);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 13);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 14);
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 15);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 16);
        entities.CashReceiptSourceType.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        entities.CashReceipt.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptDetailCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.AdjustedCashReceipt.Populated = false;
    entities.AdjustedCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetailCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdSIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvSIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstSIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtSIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.AdjustedCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.AdjustedCashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.AdjustedCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.AdjustedCashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.AdjustedCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.AdjustedCashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.AdjustedCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.AdjustedCashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.AdjustedCashReceiptDetail.CollectionDate =
          db.GetDate(reader, 5);
        entities.AdjustedCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 6);
        entities.AdjustedCashReceipt.SequentialNumber = db.GetInt32(reader, 7);
        entities.AdjustedCashReceipt.Populated = true;
        entities.AdjustedCashReceiptDetail.Populated = true;
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
      /// A value of GlocalOutgoingForeign.
      /// </summary>
      [JsonPropertyName("glocalOutgoingForeign")]
      public LegalAction GlocalOutgoingForeign
      {
        get => glocalOutgoingForeign ??= new();
        set => glocalOutgoingForeign = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private LegalAction glocalOutgoingForeign;
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
    private DateWorkArea reportStart;
    private DateWorkArea reportEnd;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private CashReceiptDetail restartCashReceiptDetail;
    private CashReceipt restartCashReceipt;
    private Common common;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Ocse157Verification forError;
    private DateWorkArea reportStart;
    private DateWorkArea reportEnd;
    private DateWorkArea max;
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
    /// A value of AdjustedCashReceipt.
    /// </summary>
    [JsonPropertyName("adjustedCashReceipt")]
    public CashReceipt AdjustedCashReceipt
    {
      get => adjustedCashReceipt ??= new();
      set => adjustedCashReceipt = value;
    }

    /// <summary>
    /// A value of AdjustedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("adjustedCashReceiptDetail")]
    public CashReceiptDetail AdjustedCashReceiptDetail
    {
      get => adjustedCashReceiptDetail ??= new();
      set => adjustedCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
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

    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
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

    private CashReceipt adjustedCashReceipt;
    private CashReceiptDetail adjustedCashReceiptDetail;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CollectionType collectionType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
    private ReceiptRefund receiptRefund;
  }
#endregion
}

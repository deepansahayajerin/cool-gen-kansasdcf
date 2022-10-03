// Program: FN_B642_PROCESS_COLLECTION_ADJ, ID: 372271843, model: 746.
// Short name: SWEF642B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B642_PROCESS_COLLECTION_ADJ.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure step reverses the subsequent collections due a collection 
/// being reversed.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB642ProcessCollectionAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B642_PROCESS_COLLECTION_ADJ program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB642ProcessCollectionAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB642ProcessCollectionAdj.
  /// </summary>
  public FnB642ProcessCollectionAdj(IContext context, Import import,
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
    // ***********************************************************************
    //   DATE      DEVELOPER  REQUEST #  DESCRIPTION
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/29/1998  Ed Lyman              Initial Development
    // ----------  ---------  ---------  
    // -------------------------------------
    // 12/02/1999  Ed Lyman   PR# 81189  Added program start timestamp.
    // ----------  ---------  ---------  
    // -------------------------------------
    // ***********************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ReportNeeded.Flag = "Y";
    local.ProgramStart.Timestamp = Now();
    UseFnB642Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE") || IsExitState
        ("PROGRAM_PROCESSING_INFO_NF"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
      else
      {
        // **********************************************************
        // WRITE TO ERROR REPORT 99
        // **********************************************************
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
        UseCabErrorReport();
      }
    }
    else
    {
      local.Collection.CollectionAdjustmentReasonTxt =
        local.RetroCollAdjustment.Description ?? "";

      // ***** Find the earliest unprocessed collection adjustment for an 
      // obligor. (An unprocessed collection adjustment has a null date in the
      // adj process date and the adjustment indicator is Y.)  This is a trigger
      // to reverse all subsequent collections for the obligor that were
      // automatically distributed (manually distributed collections are
      // excluded).  If there are subsequent unprocessed collection adjustments,
      // the cab will mark these as being processed these as well.  *****
      // *** We are determining the obligor by reading the debt, because the 
      // obligor number in the Cash Receipt Detail may have been changed. ***
      // *** No restart logic is needed.  Unprocessed collections are marked as 
      // processed whenever a commit is done.
      local.TypeOfChange.SelectChar = "C";

      if (!IsEmpty(local.PpiParameter.ObligorPersonNumber))
      {
        foreach(var item in ReadCollectionCsePerson1())
        {
          if (Equal(entities.CsePerson.Number,
            local.CashReceiptDetail.ObligorPersonNumber))
          {
            // *** There was an earlier unprocessed collection adjustment and 
            // all subsequent collections have already been reversed.
          }
          else
          {
            local.FormatDate.Text10 =
              NumberToString(Month(entities.Collection.CollectionDt), 14, 2) + "-"
              + NumberToString(Day(entities.Collection.CollectionDt), 14, 2) + "-"
              + NumberToString(Year(entities.Collection.CollectionDt), 12, 4);

            // **********************************************************
            // WRITE TO BUSINESS REPORT 01
            // **********************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "";
            UseCabBusinessReport01();
            local.EabReportSend.RptDetail = "COL ADJUST" + "  " + entities
              .CsePerson.Number + "   " + local.FormatDate.Text10 + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();
            local.Collection.CollectionAdjustmentDt =
              local.ProgramProcessingInfo.ProcessDate;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
            local.CashReceiptDetail.CollectionDate =
              entities.Collection.CollectionDt;

            // **********************************************************
            // REVERSE ANY SUBSEQUENT COLLECTIONS
            // **********************************************************
            UseFnCabReverseAllCshRcptDtls();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to read or update database.  Obligor number = " +
                (local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              break;
            }
          }

          try
          {
            UpdateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (local.NoOfIncrementalUpdates.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();
              local.NoOfIncrementalUpdates.Count = 0;

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure trying to commit a unit of work.  Obligor number = " +
                  (local.CashReceiptDetail.ObligorPersonNumber ?? "");

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                break;
              }
              else
              {
                local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                  + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                  (Now().Date.Day, 14, 2);
                local.Time.Text8 =
                  NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
                local.EabReportSend.RptDetail =
                  "Checkpoint Taken after person number: " + entities
                  .CsePerson.Number + " Date: " + local.Date.Text10 + " Time: " +
                  local.Time.Text8;
                UseCabControlReport();
                local.NoOfIncrementalUpdates.Count = 0;
              }
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              (local.CashReceiptDetail.ObligorPersonNumber ?? "");

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Exit state message:  " + local
              .ExitStateWorkArea.Message;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              break;
            }
          }
        }
      }
      else
      {
        foreach(var item in ReadCollectionCsePerson2())
        {
          if (Equal(entities.CsePerson.Number,
            local.CashReceiptDetail.ObligorPersonNumber))
          {
            // *** There was an earlier unprocessed collection adjustment and 
            // all subsequent collections have already been reversed.
          }
          else
          {
            local.FormatDate.Text10 =
              NumberToString(Month(entities.Collection.CollectionDt), 14, 2) + "-"
              + NumberToString(Day(entities.Collection.CollectionDt), 14, 2) + "-"
              + NumberToString(Year(entities.Collection.CollectionDt), 12, 4);

            // **********************************************************
            // WRITE TO BUSINESS REPORT 01
            // **********************************************************
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "";
            UseCabBusinessReport01();
            local.EabReportSend.RptDetail = "COL ADJUST" + "  " + entities
              .CsePerson.Number + "   " + local.FormatDate.Text10 + " " + " " +
              " " + " " + " " + " " + " " + " " + " " + " " + " " + " ";
            UseCabBusinessReport01();
            local.Collection.CollectionAdjustmentDt =
              local.ProgramProcessingInfo.ProcessDate;
            local.CashReceiptDetail.ObligorPersonNumber =
              entities.CsePerson.Number;
            local.CashReceiptDetail.CollectionDate =
              entities.Collection.CollectionDt;

            // **********************************************************
            // REVERSE ANY SUBSEQUENT COLLECTIONS
            // **********************************************************
            UseFnCabReverseAllCshRcptDtls();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Failure trying to read or update database.  Obligor number = " +
                (local.CashReceiptDetail.ObligorPersonNumber ?? "");

              // **********************************************************
              // WRITE TO ERROR REPORT 99
              // **********************************************************
              UseCabErrorReport();
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = local.ExitStateWorkArea.Message;
              UseCabErrorReport();

              break;
            }
          }

          try
          {
            UpdateCollection();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_COLLECTION_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_COLLECTION_PV";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            if (local.NoOfIncrementalUpdates.Count >= local
              .ProgramCheckpointRestart.UpdateFrequencyCount.
                GetValueOrDefault())
            {
              // ***** Call an external that does a DB2 commit using a Cobol 
              // program.
              UseExtToDoACommit();
              local.NoOfIncrementalUpdates.Count = 0;

              if (local.PassArea.NumericReturnCode != 0)
              {
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Failure trying to commit a unit of work.  Obligor number = " +
                  (local.CashReceiptDetail.ObligorPersonNumber ?? "");

                // **********************************************************
                // WRITE TO ERROR REPORT 99
                // **********************************************************
                UseCabErrorReport();
                ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

                break;
              }
              else
              {
                local.Date.Text10 = NumberToString(Now().Date.Year, 12, 4) + "-"
                  + NumberToString(Now().Date.Month, 14, 2) + "-" + NumberToString
                  (Now().Date.Day, 14, 2);
                local.Time.Text8 =
                  NumberToString(TimeToInt(TimeOfDay(Now())), 10, 6);
                local.EabReportSend.RptDetail =
                  "Checkpoint Taken after person number: " + entities
                  .CsePerson.Number + " Date: " + local.Date.Text10 + " Time: " +
                  local.Time.Text8;
                UseCabControlReport();
                local.NoOfIncrementalUpdates.Count = 0;
              }
            }
          }
          else
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Failure trying to read or update database.  Obligor number = " +
              (local.CashReceiptDetail.ObligorPersonNumber ?? "");

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();
            UseEabExtractExitStateMessage();
            local.EabReportSend.RptDetail = "Exit state message:  " + local
              .ExitStateWorkArea.Message;

            // **********************************************************
            // WRITE TO ERROR REPORT 99
            // **********************************************************
            UseCabErrorReport();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
            }
            else
            {
              break;
            }
          }
        }
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ALL_OK";
      UseFnB642Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      UseFnB642Close();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.CollectionAdjustmentDt = source.CollectionAdjustmentDt;
    target.CollectionAdjustmentReasonTxt = source.CollectionAdjustmentReasonTxt;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseFnB642Close()
  {
    var useImport = new FnB642Close.Import();
    var useExport = new FnB642Close.Export();

    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;

    Call(FnB642Close.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB642Housekeeping()
  {
    var useImport = new FnB642Housekeeping.Import();
    var useExport = new FnB642Housekeeping.Export();

    Call(FnB642Housekeeping.Execute, useImport, useExport);

    local.Max.Date = useExport.Max.Date;
    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    local.RetroCollAdjustment.Assign(useExport.SubsequentCollection);
    local.AdjustedStatus.SystemGeneratedIdentifier =
      useExport.AdjustedStatus.SystemGeneratedIdentifier;
    local.SuspendedStatus.SystemGeneratedIdentifier =
      useExport.SuspendedStatus.SystemGeneratedIdentifier;
    local.ReleasedStatus.SystemGeneratedIdentifier =
      useExport.ReleasedStatus.SystemGeneratedIdentifier;
    local.DistributedStatus.SystemGeneratedIdentifier =
      useExport.DistributedStatus.SystemGeneratedIdentifier;
    local.RefundedStatus.SystemGeneratedIdentifier =
      useExport.RefundedStatus.SystemGeneratedIdentifier;
    local.PpiParameter.ObligorPersonNumber =
      useExport.PpiParameter.ObligorPersonNumber;
  }

  private void UseFnCabReverseAllCshRcptDtls()
  {
    var useImport = new FnCabReverseAllCshRcptDtls.Import();
    var useExport = new FnCabReverseAllCshRcptDtls.Export();

    useImport.CashReceiptType.SystemGeneratedIdentifier =
      local.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      local.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.InterfaceIndicator =
      local.CashReceiptSourceType.InterfaceIndicator;
    MoveCollection(local.Collection, useImport.Collection);
    useImport.CashReceipt.SequentialNumber = local.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.Assign(local.CashReceiptDetail);
    useImport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useImport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useImport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;
    useImport.ReportNeeded.Flag = local.ReportNeeded.Flag;
    useImport.TypeOfChange.SelectChar = local.TypeOfChange.SelectChar;
    useImport.RefundedStatus.SystemGeneratedIdentifier =
      local.RefundedStatus.SystemGeneratedIdentifier;
    useImport.DistributedStatus.SystemGeneratedIdentifier =
      local.DistributedStatus.SystemGeneratedIdentifier;
    useImport.ReleasedStatus.SystemGeneratedIdentifier =
      local.ReleasedStatus.SystemGeneratedIdentifier;
    useImport.SuspendedStatus.SystemGeneratedIdentifier =
      local.SuspendedStatus.SystemGeneratedIdentifier;
    useImport.CollectionAdjustmentReason.SystemGeneratedIdentifier =
      local.RetroCollAdjustment.SystemGeneratedIdentifier;
    MoveProgramProcessingInfo(local.ProgramProcessingInfo,
      useImport.ProgramProcessingInfo);
    useImport.Max.Date = local.Max.Date;
    useImport.ProgramStart.Timestamp = local.ProgramStart.Timestamp;
    useExport.NoOfIncrementalUpdates.Count = local.NoOfIncrementalUpdates.Count;
    useExport.NoCollectionsReversed.Count = local.NoCollectionsReversed.Count;
    useExport.NoCashRecptDtlUpdated.Count = local.NoCashRecptDtlUpdated.Count;

    Call(FnCabReverseAllCshRcptDtls.Execute, useImport, useExport);

    local.NoOfIncrementalUpdates.Count = useImport.NoOfIncrementalUpdates.Count;
    local.NoCollectionsReversed.Count = useImport.NoCollectionsReversed.Count;
    local.NoCashRecptDtlUpdated.Count = useImport.NoCashRecptDtlUpdated.Count;
    local.NoOfIncrementalUpdates.Count = useExport.NoOfIncrementalUpdates.Count;
    local.NoCollectionsReversed.Count = useExport.NoCollectionsReversed.Count;
    local.NoCashRecptDtlUpdated.Count = useExport.NoCashRecptDtlUpdated.Count;
  }

  private IEnumerable<bool> ReadCollectionCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePerson1",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", local.PpiParameter.ObligorPersonNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.CsePerson.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollectionCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.Collection.Populated = false;

    return ReadEach("ReadCollectionCsePerson2",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.CollectionDt = db.GetDate(reader, 1);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 2);
        entities.Collection.CrtType = db.GetInt32(reader, 3);
        entities.Collection.CstId = db.GetInt32(reader, 4);
        entities.Collection.CrvId = db.GetInt32(reader, 5);
        entities.Collection.CrdId = db.GetInt32(reader, 6);
        entities.Collection.ObgId = db.GetInt32(reader, 7);
        entities.Collection.CspNumber = db.GetString(reader, 8);
        entities.CsePerson.Number = db.GetString(reader, 8);
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CollectionAdjProcessDate = db.GetDate(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.CsePerson.Populated = true;
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var collectionAdjProcessDate = local.ProgramProcessingInfo.ProcessDate;
    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTmst = Now();

    entities.Collection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetDate(command, "collAdjProcDate", collectionAdjProcessDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "collId", entities.Collection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", entities.Collection.CrtType);
        db.SetInt32(command, "cstId", entities.Collection.CstId);
        db.SetInt32(command, "crvId", entities.Collection.CrvId);
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "obgId", entities.Collection.ObgId);
        db.SetString(command, "cspNumber", entities.Collection.CspNumber);
        db.SetString(command, "cpaType", entities.Collection.CpaType);
        db.SetInt32(command, "otrId", entities.Collection.OtrId);
        db.SetString(command, "otrType", entities.Collection.OtrType);
        db.SetInt32(command, "otyId", entities.Collection.OtyId);
      });

    entities.Collection.CollectionAdjProcessDate = collectionAdjProcessDate;
    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.Populated = true;
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
    /// A value of PpiParameter.
    /// </summary>
    [JsonPropertyName("ppiParameter")]
    public CashReceiptDetail PpiParameter
    {
      get => ppiParameter ??= new();
      set => ppiParameter = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
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
    /// A value of NoOfIncrementalUpdates.
    /// </summary>
    [JsonPropertyName("noOfIncrementalUpdates")]
    public Common NoOfIncrementalUpdates
    {
      get => noOfIncrementalUpdates ??= new();
      set => noOfIncrementalUpdates = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
    }

    /// <summary>
    /// A value of NoCollectionsReversed.
    /// </summary>
    [JsonPropertyName("noCollectionsReversed")]
    public Common NoCollectionsReversed
    {
      get => noCollectionsReversed ??= new();
      set => noCollectionsReversed = value;
    }

    /// <summary>
    /// A value of NoCashRecptDtlUpdated.
    /// </summary>
    [JsonPropertyName("noCashRecptDtlUpdated")]
    public Common NoCashRecptDtlUpdated
    {
      get => noCashRecptDtlUpdated ??= new();
      set => noCashRecptDtlUpdated = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of TypeOfChange.
    /// </summary>
    [JsonPropertyName("typeOfChange")]
    public Common TypeOfChange
    {
      get => typeOfChange ??= new();
      set => typeOfChange = value;
    }

    /// <summary>
    /// A value of AdjustedStatus.
    /// </summary>
    [JsonPropertyName("adjustedStatus")]
    public CashReceiptDetailStatus AdjustedStatus
    {
      get => adjustedStatus ??= new();
      set => adjustedStatus = value;
    }

    /// <summary>
    /// A value of RefundedStatus.
    /// </summary>
    [JsonPropertyName("refundedStatus")]
    public CashReceiptDetailStatus RefundedStatus
    {
      get => refundedStatus ??= new();
      set => refundedStatus = value;
    }

    /// <summary>
    /// A value of DistributedStatus.
    /// </summary>
    [JsonPropertyName("distributedStatus")]
    public CashReceiptDetailStatus DistributedStatus
    {
      get => distributedStatus ??= new();
      set => distributedStatus = value;
    }

    /// <summary>
    /// A value of ReleasedStatus.
    /// </summary>
    [JsonPropertyName("releasedStatus")]
    public CashReceiptDetailStatus ReleasedStatus
    {
      get => releasedStatus ??= new();
      set => releasedStatus = value;
    }

    /// <summary>
    /// A value of SuspendedStatus.
    /// </summary>
    [JsonPropertyName("suspendedStatus")]
    public CashReceiptDetailStatus SuspendedStatus
    {
      get => suspendedStatus ??= new();
      set => suspendedStatus = value;
    }

    /// <summary>
    /// A value of RetroCollAdjustment.
    /// </summary>
    [JsonPropertyName("retroCollAdjustment")]
    public CollectionAdjustmentReason RetroCollAdjustment
    {
      get => retroCollAdjustment ??= new();
      set => retroCollAdjustment = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
    }

    /// <summary>
    /// A value of FormatDate.
    /// </summary>
    [JsonPropertyName("formatDate")]
    public TextWorkArea FormatDate
    {
      get => formatDate ??= new();
      set => formatDate = value;
    }

    /// <summary>
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public TextWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    /// <summary>
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public TextWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    /// <summary>
    /// A value of ProgramStart.
    /// </summary>
    [JsonPropertyName("programStart")]
    public DateWorkArea ProgramStart
    {
      get => programStart ??= new();
      set => programStart = value;
    }

    private CashReceiptDetail ppiParameter;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private Collection collection;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Common noOfIncrementalUpdates;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Common noCollectionsReversed;
    private Common noCashRecptDtlUpdated;
    private Common reportNeeded;
    private Common typeOfChange;
    private CashReceiptDetailStatus adjustedStatus;
    private CashReceiptDetailStatus refundedStatus;
    private CashReceiptDetailStatus distributedStatus;
    private CashReceiptDetailStatus releasedStatus;
    private CashReceiptDetailStatus suspendedStatus;
    private CollectionAdjustmentReason retroCollAdjustment;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private DateWorkArea max;
    private DateWorkArea null1;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External passArea;
    private ExitStateWorkArea exitStateWorkArea;
    private TextWorkArea formatDate;
    private TextWorkArea date;
    private TextWorkArea time;
    private DateWorkArea programStart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private CsePerson csePerson;
    private ObligationTransaction debt;
    private Obligation obligation;
    private CsePersonAccount obligor;
    private Collection collection;
  }
#endregion
}

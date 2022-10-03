// Program: FN_BFXM_FDSO_ADJUSTMENT_FIX, ID: 374577388, model: 746.
// Short name: FNBFXMFD
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXM_FDSO_ADJUSTMENT_FIX.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxmFdsoAdjustmentFix: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXM_FDSO_ADJUSTMENT_FIX program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxmFdsoAdjustmentFix(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxmFdsoAdjustmentFix.
  /// </summary>
  public FnBfxmFdsoAdjustmentFix(IContext context, Import import, Export export):
    
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
    // 06/28/10  R.Mathews	CQ14023		Correct FDSO adjustment discontinue dates
    // -----------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "OPEN";

    // --  Read PPI record.
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.ParmAction.Text8 =
      Substring(local.ProgramProcessingInfo.ParameterList, 1, 6);
    local.ParmWork.Text12 =
      Substring(local.ProgramProcessingInfo.ParameterList, 8, 12);

    if (IsEmpty(local.ParmWork.Text12))
    {
      local.ParmCrStart.SequentialNumber = (int)StringToNumber("0");
    }
    else
    {
      local.ParmCrStart.SequentialNumber =
        (int)StringToNumber(local.ParmWork.Text12);
    }

    local.ParmWork.Text12 =
      Substring(local.ProgramProcessingInfo.ParameterList, 21, 12);

    if (IsEmpty(local.ParmWork.Text12))
    {
      local.ParmCrEnd.SequentialNumber = (int)StringToNumber("999999999999");
    }
    else
    {
      local.ParmCrEnd.SequentialNumber =
        (int)StringToNumber(local.ParmWork.Text12);
    }

    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    local.EabReportSend.ProgramName = global.UserId;

    // -- Open error report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // -- Open control report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // --  Get commit frequency.
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";

    // -- Log the beginning and ending cash receipt numbers to the control 
    // report
    for(local.I.Count = 1; local.I.Count <= 4; ++local.I.Count)
    {
      switch(local.I.Count)
      {
        case 2:
          local.EabReportSend.RptDetail = "Starting Cash Receipt Detail   " + NumberToString
            (local.ParmCrStart.SequentialNumber, 15);

          break;
        case 3:
          local.EabReportSend.RptDetail = "Ending Cash Receipt Detail  " + NumberToString
            (local.ParmCrEnd.SequentialNumber, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.HoldCrdId.SequentialIdentifier = 0;
    local.HoldCrId.SequentialNumber = 0;
    local.ProcessCount.Count = 0;
    local.UpdateCount.Count = 0;
    local.TotalUpdateCount.Count = 0;

    // --- Read FDSO cash receipts for update.  Edit for refunded amt > 0 
    // removed 7/26/10.
    foreach(var item in ReadCashReceiptEventCashReceiptCashReceiptDetail())
    {
      // -- and desired cash_receipt_detail refunded_amount is greater than 0
      local.CrHistory.Index = -1;
      local.CrHistory.Count = 0;
      local.ChgFlag.Text1 = "N";
      ++local.ProcessCount.Count;

      // --- Read through associated status history records checking for gaps in
      // discontinue date
      foreach(var item1 in ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus())
        
      {
        ++local.CrHistory.Index;
        local.CrHistory.CheckSize();

        local.CrHistory.Update.Cr.SequentialNumber =
          entities.CashReceipt.SequentialNumber;
        local.CrHistory.Update.CrDtl.SequentialIdentifier =
          entities.CashReceiptDetail.SequentialIdentifier;
        local.CrHistory.Update.CrSts.Code =
          entities.CashReceiptDetailStatus.Code;
        local.CrHistory.Update.CrHist.CreatedBy =
          entities.CashReceiptDetailStatHistory.CreatedBy;
        local.CrHistory.Update.CrHist.CreatedTimestamp =
          entities.CashReceiptDetailStatHistory.CreatedTimestamp;
        local.CrHistory.Update.CrHist.DiscontinueDate =
          entities.CashReceiptDetailStatHistory.DiscontinueDate;
        local.CrHistory.Update.ChgDate.DiscontinueDate =
          new DateTime(1900, 1, 1);

        if (entities.CashReceipt.SequentialNumber != local
          .HoldCrId.SequentialNumber || entities
          .CashReceiptDetail.SequentialIdentifier != local
          .HoldCrdId.SequentialIdentifier)
        {
          local.HoldTimestamp.CreatedTimestamp =
            entities.CashReceiptDetailStatHistory.CreatedTimestamp;
          local.HoldCrId.SequentialNumber =
            entities.CashReceipt.SequentialNumber;
          local.HoldCrdId.SequentialIdentifier =
            entities.CashReceiptDetail.SequentialIdentifier;
          local.HoldCreatedBy.CreatedBy =
            entities.CashReceiptDetailStatHistory.CreatedBy;

          continue;
        }

        if (Equal(entities.CashReceiptDetailStatHistory.DiscontinueDate,
          new DateTime(2099, 12, 31)) || Equal
          (entities.CashReceiptDetailStatHistory.DiscontinueDate,
          Date(AddHours(local.HoldTimestamp.CreatedTimestamp, -7))))
        {
          local.HoldTimestamp.CreatedTimestamp =
            entities.CashReceiptDetailStatHistory.CreatedTimestamp;
          local.HoldCreatedBy.CreatedBy =
            entities.CashReceiptDetailStatHistory.CreatedBy;

          continue;
        }

        // Check that date difference was caused by FDSO adjustment in SWEFB612
        if (!Equal(local.HoldCreatedBy.CreatedBy, "SWEFB612"))
        {
          local.HoldTimestamp.CreatedTimestamp =
            entities.CashReceiptDetailStatHistory.CreatedTimestamp;
          local.HoldCreatedBy.CreatedBy =
            entities.CashReceiptDetailStatHistory.CreatedBy;

          continue;
        }

        local.CrHistory.Update.ChgDate.DiscontinueDate =
          Date(AddHours(local.HoldTimestamp.CreatedTimestamp, -7));
        local.HoldTimestamp.CreatedTimestamp =
          entities.CashReceiptDetailStatHistory.CreatedTimestamp;
        local.HoldCreatedBy.CreatedBy =
          entities.CashReceiptDetailStatHistory.CreatedBy;
        local.ChgFlag.Text1 = "Y";

        // ----------------------------------------------------------------------------------------
        // If Parm = Update, change the cash receipt detail
        // status history discontinue date to the proposed value
        // ----------------------------------------------------------------------------------------
        if (Equal(local.ParmAction.Text8, "UPDATE"))
        {
          try
          {
            UpdateCashReceiptDetailStatHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }

      if (AsChar(local.ChgFlag.Text1) == 'Y')
      {
        ++local.UpdateCount.Count;
        ++local.TotalUpdateCount.Count;

        // -------------------------------------------------------------------------------
        // Write report heading line for each cash receipt detail updated
        // -------------------------------------------------------------------------------
        for(local.I.Count = 1; local.I.Count <= 3; ++local.I.Count)
        {
          switch(local.I.Count)
          {
            case 2:
              local.EabReportSend.RptDetail =
                "CRD_ID        CODE  CREATE_BY  CREATE_TIMESTAMP            DISC_DATE   UPDT_DATE";
                

              break;
            case 3:
              local.EabReportSend.RptDetail =
                "------------  ----  ---------  --------------------------  ----------  ----------";
                

              break;
            default:
              local.EabReportSend.RptDetail = "";

              break;
          }

          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        for(local.CrHistory.Index = 0; local.CrHistory.Index < local
          .CrHistory.Count; ++local.CrHistory.Index)
        {
          if (!local.CrHistory.CheckSize())
          {
            break;
          }

          // -----------------------------------------------------------------------------
          // Format and write report detail line
          // -----------------------------------------------------------------------------
          local.EabReportSend.RptDetail = "";
          local.EabReportSend.RptDetail =
            NumberToString(local.CrHistory.Item.Cr.SequentialNumber, 9, 7) + "-"
            ;
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + NumberToString
            (local.CrHistory.Item.CrDtl.SequentialIdentifier, 12, 4);
          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  " + Substring
            (local.CrHistory.Item.CrSts.Code,
            CashReceiptDetailStatus.Code_MaxLength, 1, 4);

          if (Length(TrimEnd(local.EabReportSend.RptDetail)) == 17)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + Substring
              (local.CrHistory.Item.CrHist.CreatedBy,
              CashReceiptDetailStatHistory.CreatedBy_MaxLength, 1, 8);
          }
          else
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  " + Substring
              (local.CrHistory.Item.CrHist.CreatedBy,
              CashReceiptDetailStatHistory.CreatedBy_MaxLength, 1, 8);
          }

          // -----------------------------------------------------------------------------
          // Parse timestamp into text field in following lines
          // -----------------------------------------------------------------------------
          local.WorkTimestamp.Text30 = "";
          local.WorkTimestamp.Text30 =
            NumberToString(Year(local.CrHistory.Item.CrHist.CreatedTimestamp),
            12, 4) + "-";
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Month(local.CrHistory.Item.CrHist.CreatedTimestamp), 14, 2);
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + "-"
            ;
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Day(local.CrHistory.Item.CrHist.CreatedTimestamp), 14, 2);
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + "-"
            ;
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Hour(local.CrHistory.Item.CrHist.CreatedTimestamp), 14, 2);
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + "."
            ;
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Minute(local.CrHistory.Item.CrHist.CreatedTimestamp), 14, 2);
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + "."
            ;
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Second(local.CrHistory.Item.CrHist.CreatedTimestamp), 14, 2);
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + "."
            ;
          local.WorkTimestamp.Text30 = TrimEnd(local.WorkTimestamp.Text30) + NumberToString
            (Microsecond(local.CrHistory.Item.CrHist.CreatedTimestamp), 10, 6);

          // -----------------------------------------------------------------------------------
          if (Length(TrimEnd(local.EabReportSend.RptDetail)) == 27)
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "    " + Substring
              (local.WorkTimestamp.Text30, TextWorkArea.Text30_MaxLength, 1, 26);
              
          }
          else
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "   " + Substring
              (local.WorkTimestamp.Text30, TextWorkArea.Text30_MaxLength, 1, 26);
              
          }

          local.EabReportSend.RptDetail =
            TrimEnd(local.EabReportSend.RptDetail) + "  " + NumberToString
            (DateToInt(local.CrHistory.Item.CrHist.DiscontinueDate), 8, 4) + "-"
            + NumberToString
            (DateToInt(local.CrHistory.Item.CrHist.DiscontinueDate), 12, 2) + "-"
            + NumberToString
            (DateToInt(local.CrHistory.Item.CrHist.DiscontinueDate), 14, 2);

          if (!Equal(local.CrHistory.Item.ChgDate.DiscontinueDate,
            new DateTime(1900, 1, 1)))
          {
            local.EabReportSend.RptDetail =
              TrimEnd(local.EabReportSend.RptDetail) + "  " + NumberToString
              (DateToInt(local.CrHistory.Item.ChgDate.DiscontinueDate), 8, 4) +
              "-" + NumberToString
              (DateToInt(local.CrHistory.Item.ChgDate.DiscontinueDate), 12, 2) +
              "-" + NumberToString
              (DateToInt(local.CrHistory.Item.ChgDate.DiscontinueDate), 14, 2);
          }

          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.CrHistory.CheckIndex();

        // -------------------------------------------------------------------------------
        // Check if changes need to be committed
        // -------------------------------------------------------------------------------
        if (Equal(local.ParmAction.Text8, "UPDATE"))
        {
          if (local.UpdateCount.Count > local
            .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
          {
            local.UpdateCount.Count = 0;
            UseUpdateCheckpointRstAndCommit();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
      }
    }

    // -- Log the total counts to the control report
    for(local.I.Count = 1; local.I.Count <= 4; ++local.I.Count)
    {
      switch(local.I.Count)
      {
        case 3:
          local.EabReportSend.RptDetail = "Total Process Count = " + NumberToString
            (local.ProcessCount.Count, 15);

          break;
        case 4:
          local.EabReportSend.RptDetail = "Total Update Count  = " + NumberToString
            (local.TotalUpdateCount.Count, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    local.EabFileHandling.Action = "CLOSE";

    // -- Close control report
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";

      return;
    }

    // -- Close error report
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ERROR_CLOSING_FILE_AB";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
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

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool>
    ReadCashReceiptDetailStatHistoryCashReceiptDetailStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetailStatHistory.Populated = false;

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
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetailStatHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptDetailStatHistory.CreatedBy =
          db.GetString(reader, 6);
        entities.CashReceiptDetailStatHistory.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.CashReceiptDetailStatus.Code = db.GetString(reader, 8);
        entities.CashReceiptDetailStatus.Name = db.GetString(reader, 9);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetailStatHistory.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptEventCashReceiptCashReceiptDetail()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptEventCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "sequentialNumber1", local.ParmCrStart.SequentialNumber);
        db.SetInt32(
          command, "sequentialNumber2", local.ParmCrEnd.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 3);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 5);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private void UpdateCashReceiptDetailStatHistory()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptDetailStatHistory.Populated);

    var discontinueDate = local.CrHistory.Item.ChgDate.DiscontinueDate;

    entities.CashReceiptDetailStatHistory.Populated = false;
    Update("UpdateCashReceiptDetailStatHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetailStatHistory.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailStatHistory.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailStatHistory.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailStatHistory.CrtIdentifier);
        db.SetInt32(
          command, "cdsIdentifier",
          entities.CashReceiptDetailStatHistory.CdsIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptDetailStatHistory.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptDetailStatHistory.DiscontinueDate = discontinueDate;
    entities.CashReceiptDetailStatHistory.Populated = true;
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
    /// <summary>A CrHistoryGroup group.</summary>
    [Serializable]
    public class CrHistoryGroup
    {
      /// <summary>
      /// A value of ChgDate.
      /// </summary>
      [JsonPropertyName("chgDate")]
      public CashReceiptDetailStatHistory ChgDate
      {
        get => chgDate ??= new();
        set => chgDate = value;
      }

      /// <summary>
      /// A value of CrSts.
      /// </summary>
      [JsonPropertyName("crSts")]
      public CashReceiptDetailStatus CrSts
      {
        get => crSts ??= new();
        set => crSts = value;
      }

      /// <summary>
      /// A value of CrHist.
      /// </summary>
      [JsonPropertyName("crHist")]
      public CashReceiptDetailStatHistory CrHist
      {
        get => crHist ??= new();
        set => crHist = value;
      }

      /// <summary>
      /// A value of CrDtl.
      /// </summary>
      [JsonPropertyName("crDtl")]
      public CashReceiptDetail CrDtl
      {
        get => crDtl ??= new();
        set => crDtl = value;
      }

      /// <summary>
      /// A value of Cr.
      /// </summary>
      [JsonPropertyName("cr")]
      public CashReceipt Cr
      {
        get => cr ??= new();
        set => cr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1200;

      private CashReceiptDetailStatHistory chgDate;
      private CashReceiptDetailStatus crSts;
      private CashReceiptDetailStatHistory crHist;
      private CashReceiptDetail crDtl;
      private CashReceipt cr;
    }

    /// <summary>
    /// A value of HoldCreatedBy.
    /// </summary>
    [JsonPropertyName("holdCreatedBy")]
    public CashReceiptDetailStatHistory HoldCreatedBy
    {
      get => holdCreatedBy ??= new();
      set => holdCreatedBy = value;
    }

    /// <summary>
    /// A value of ParmWork.
    /// </summary>
    [JsonPropertyName("parmWork")]
    public TextWorkArea ParmWork
    {
      get => parmWork ??= new();
      set => parmWork = value;
    }

    /// <summary>
    /// A value of ParmCrStart.
    /// </summary>
    [JsonPropertyName("parmCrStart")]
    public CashReceipt ParmCrStart
    {
      get => parmCrStart ??= new();
      set => parmCrStart = value;
    }

    /// <summary>
    /// A value of ParmCrEnd.
    /// </summary>
    [JsonPropertyName("parmCrEnd")]
    public CashReceipt ParmCrEnd
    {
      get => parmCrEnd ??= new();
      set => parmCrEnd = value;
    }

    /// <summary>
    /// A value of ParmAction.
    /// </summary>
    [JsonPropertyName("parmAction")]
    public TextWorkArea ParmAction
    {
      get => parmAction ??= new();
      set => parmAction = value;
    }

    /// <summary>
    /// A value of HoldCrId.
    /// </summary>
    [JsonPropertyName("holdCrId")]
    public CashReceipt HoldCrId
    {
      get => holdCrId ??= new();
      set => holdCrId = value;
    }

    /// <summary>
    /// A value of WorkTimestamp.
    /// </summary>
    [JsonPropertyName("workTimestamp")]
    public TextWorkArea WorkTimestamp
    {
      get => workTimestamp ??= new();
      set => workTimestamp = value;
    }

    /// <summary>
    /// A value of TotalUpdateCount.
    /// </summary>
    [JsonPropertyName("totalUpdateCount")]
    public Common TotalUpdateCount
    {
      get => totalUpdateCount ??= new();
      set => totalUpdateCount = value;
    }

    /// <summary>
    /// A value of HoldTimestamp.
    /// </summary>
    [JsonPropertyName("holdTimestamp")]
    public CashReceiptDetailStatHistory HoldTimestamp
    {
      get => holdTimestamp ??= new();
      set => holdTimestamp = value;
    }

    /// <summary>
    /// A value of HoldCrdId.
    /// </summary>
    [JsonPropertyName("holdCrdId")]
    public CashReceiptDetail HoldCrdId
    {
      get => holdCrdId ??= new();
      set => holdCrdId = value;
    }

    /// <summary>
    /// A value of ChgFlag.
    /// </summary>
    [JsonPropertyName("chgFlag")]
    public TextWorkArea ChgFlag
    {
      get => chgFlag ??= new();
      set => chgFlag = value;
    }

    /// <summary>
    /// A value of ProcessCount.
    /// </summary>
    [JsonPropertyName("processCount")]
    public Common ProcessCount
    {
      get => processCount ??= new();
      set => processCount = value;
    }

    /// <summary>
    /// Gets a value of CrHistory.
    /// </summary>
    [JsonIgnore]
    public Array<CrHistoryGroup> CrHistory => crHistory ??= new(
      CrHistoryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CrHistory for json serialization.
    /// </summary>
    [JsonPropertyName("crHistory")]
    [Computed]
    public IList<CrHistoryGroup> CrHistory_Json
    {
      get => crHistory;
      set => CrHistory.Assign(value);
    }

    /// <summary>
    /// A value of UpdateCount.
    /// </summary>
    [JsonPropertyName("updateCount")]
    public Common UpdateCount
    {
      get => updateCount ??= new();
      set => updateCount = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
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

    private CashReceiptDetailStatHistory holdCreatedBy;
    private TextWorkArea parmWork;
    private CashReceipt parmCrStart;
    private CashReceipt parmCrEnd;
    private TextWorkArea parmAction;
    private CashReceipt holdCrId;
    private TextWorkArea workTimestamp;
    private Common totalUpdateCount;
    private CashReceiptDetailStatHistory holdTimestamp;
    private CashReceiptDetail holdCrdId;
    private TextWorkArea chgFlag;
    private Common processCount;
    private Array<CrHistoryGroup> crHistory;
    private Common updateCount;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common i;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}

// Program: FN_B711_PROCESS_KPC_TRIGGERS, ID: 374442413, model: 746.
// Short name: SWEF711B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B711_PROCESS_KPC_TRIGGERS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB711ProcessKpcTriggers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B711_PROCESS_KPC_TRIGGERS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB711ProcessKpcTriggers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB711ProcessKpcTriggers.
  /// </summary>
  public FnB711ProcessKpcTriggers(IContext context, Import import, Export export)
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
    // -----------------------------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------      --------------  --------------
    // 
    // -----------------------------------------------
    // 06/07/2000	M Ramirez	WR 172 Seg F	Initial Dev
    // 01/04/2000	PMcElderry	WR # 272	Added header record containing process 
    // date.
    // 						Added a trailer record containing total number
    // 						of rows, total number of notice records, and
    // 						total dollar amount of the details.
    // 06/18/2002	M Ramirez	PR147627	Added functionality to print a specified
    // 						time range to allow resending previous files
    // 06/01/2009	Joyce Harden	CQ7812		Close ADABAS
    // 02/13/2018	GVandy		CQ45790		The interface will now include refunds
    // 						for payments which originated at the KPC.
    // -----------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB711Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 9;

    // ------------
    // Beg WR # 272
    // ------------
    local.WorkArea.Text8 =
      NumberToString(DateToInt(local.ProgramProcessingInfo.ProcessDate), 8);
    local.HeaderRecord.Text9 = "H" + local.WorkArea.Text8;
    local.FnKpcCourtNotc.ObligorSsn = local.HeaderRecord.Text9;
    local.FnKpcCourtNotc.CollectionType = "";
    local.FnKpcCourtNotc.Amount = 0;
    local.FnKpcCourtNotc.LegalActionStandardNumber = "";
    local.FnKpcCourtNotc.DistributionDate = null;
    UseFnEabWriteKpcCourtNotcFile();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabReportSend.RptDetail =
        "ABEND:  Unable to write Header record to file";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_4_BATCH";

      return;
    }

    // ------------
    // End WR # 272
    // ------------
    foreach(var item in ReadTrigger())
    {
      ++local.TriggersRead.Count;
      ++local.ControlTotalRead.Count;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Trigger.Identifier, 15);
        UseEabConvertNumeric1();
        local.Trigger.Text9 =
          Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
        local.EabReportSend.RptDetail = "DEBUG: READ trigger; TRIGGER = " + local
          .Trigger.Text9;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      switch(TrimEnd(entities.Trigger.Type1))
      {
        case "KPC":
          local.LegalAction1.Identifier =
            entities.Trigger.DenormNumeric1.GetValueOrDefault();
          local.CashReceiptDetail.SequentialIdentifier =
            entities.Trigger.DenormNumeric2.GetValueOrDefault();
          local.CashReceiptEvent.SystemGeneratedIdentifier =
            entities.Trigger.DenormNumeric3.GetValueOrDefault();
          local.CsePerson.Number = entities.Trigger.DenormText1 ?? Spaces(10);
          local.Temp.TotalInteger =
            StringToNumber(entities.Trigger.DenormText2);
          local.CashReceiptSourceType.SystemGeneratedIdentifier =
            (int)local.Temp.TotalInteger;
          local.Temp.TotalInteger =
            StringToNumber(entities.Trigger.DenormText3);
          local.CashReceiptType.SystemGeneratedIdentifier =
            (int)local.Temp.TotalInteger;
          local.TriggerReference.Date = Date(entities.Trigger.UpdatedTimestamp);

          if (ReadCsePerson())
          {
            local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
            UseSiReadCsePersonBatch();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              if (IsExitState("ADABAS_UNAVAILABLE_RB"))
              {
                local.EabReportSend.RptDetail = "ABEND:  ADABAS Unavailable";
                UseCabErrorReport();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                  return;
                }

                ExitState = "ACO_NN0000_ABEND_4_BATCH";

                goto ReadEach;
              }

              ExitState = "ACO_NN0000_ALL_OK";
            }

            local.FnKpcCourtNotc.ObligorSsn = local.CsePersonsWorkSet.Ssn;
          }
          else
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  CSE_Person not found for TRIGGER;  TRIGGER = " + local
              .Trigger.Text9 + "; CSE_Person = " + local.CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          if (ReadLegalAction())
          {
            local.FnKpcCourtNotc.LegalActionStandardNumber =
              entities.LegalAction.StandardNumber ?? Spaces(20);
          }
          else
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(local.LegalAction1.Identifier, 15);
            UseEabConvertNumeric1();
            local.LegalAction2.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  Legal_Action not found for TRIGGER;  TRIGGER = " + local
              .Trigger.Text9 + "; Legal_Action = " + local.LegalAction2.Text9;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          if (ReadCashReceiptDetail())
          {
            local.FnKpcCourtNotc.DistributionDate =
              entities.CashReceiptDetail.CollectionDate;
          }
          else
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  Cash_Receipt_Detail not found for TRIGGER;  TRIGGER = " +
              local.Trigger.Text9;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          local.SendNotice.Flag = "N";
          local.Total.Amount = 0;
          local.CollectionType.Name = "";

          foreach(var item1 in ReadCollection())
          {
            if (Equal(entities.Collection.CourtNoticeProcessedDate,
              entities.Collection.CourtNoticeAdjProcessDate))
            {
              // mjr
              // ------------------------------------------------------------------
              // This collection was applied and adjusted in the same day.
              // The net of these actions is 0 so don't count this record in the
              // total
              // ---------------------------------------------------------------------
              continue;
            }

            local.SendNotice.Flag = "Y";

            if (AsChar(entities.Collection.AdjustedInd) == 'Y')
            {
              local.Total.Amount -= entities.Collection.Amount;
            }
            else
            {
              local.Total.Amount += entities.Collection.Amount;
            }

            if (IsEmpty(local.CollectionType.Name))
            {
              if (AsChar(entities.Collection.AdjustedInd) == 'Y')
              {
                if (ReadCollectionAdjustmentReason())
                {
                  if (Equal(entities.CollectionAdjustmentReason.Code, "FDSOADJ") ||
                    Equal
                    (entities.CollectionAdjustmentReason.Code, "IRS NEG") || Equal
                    (entities.CollectionAdjustmentReason.Code, "SDSOADJ"))
                  {
                    local.CollectionType.Name = "PAYMENT ADJUSTMENT";
                  }
                  else
                  {
                    local.CollectionType.Name =
                      entities.CollectionAdjustmentReason.Name;
                  }
                }
              }
              else if (ReadCollectionType())
              {
                if (Equal(entities.CollectionType.Code, "F") || Equal
                  (entities.CollectionType.Code, "K") || Equal
                  (entities.CollectionType.Code, "S") || Equal
                  (entities.CollectionType.Code, "T") || Equal
                  (entities.CollectionType.Code, "U") || Equal
                  (entities.CollectionType.Code, "Y") || Equal
                  (entities.CollectionType.Code, "Z") || Equal
                  (entities.CollectionType.Code, "8") || Equal
                  (entities.CollectionType.Code, "9"))
                {
                  local.CollectionType.Name = "PAYMENT";
                }
                else
                {
                  local.CollectionType.Name = entities.CollectionType.Name;
                }
              }
            }
          }

          if (AsChar(local.SendNotice.Flag) == 'N')
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  No collections found for TRIGGER;  TRIGGER = " + local
              .Trigger.Text9 + "; Obligor = " + local.CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          if (local.Total.Amount == 0)
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  Notice not sent for TRIGGER because collections balance;  TRIGGER = " +
              local.Trigger.Text9 + "; Obligor = " + local.CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          if (IsEmpty(local.CollectionType.Name))
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ERROR:  No collection type/adjustment reason found for TRIGGER because collections balance;  TRIGGER = " +
              local.Trigger.Text9 + "; Obligor = " + local.CsePerson.Number;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          local.FnKpcCourtNotc.CollectionType = local.CollectionType.Name;
          local.FnKpcCourtNotc.Amount = local.Total.Amount;
          local.FileTotal.Amount += local.FnKpcCourtNotc.Amount;
          UseFnEabWriteKpcCourtNotcFile();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ABEND: Writing to file; TRIGGER = " + local.Trigger.Text9;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_4_BATCH";

            goto ReadEach;
          }

          ++local.TriggersProcessed.Count;
          ++local.ControlTotalProcessed.Count;

          // mjr
          // -------------------------------------------------
          // End READ EACH
          // ----------------------------------------------------
          break;
        case "KPC REF":
          // --02/13/2018  GVandy  CQ45790  The interface will now include 
          // refunds for payments
          // which originated at the KPC.
          if (!ReadReceiptRefundCashReceiptDetail())
          {
            ++local.ControlTotalErred.Count;
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.BatchTimestampWorkArea.IefTimestamp =
              entities.Trigger.DenormTimestamp;
            local.BatchTimestampWorkArea.TextTimestamp = "";
            UseLeCabConvertTimestamp();
            local.EabReportSend.RptDetail =
              "ERROR:  Receipt refund not found for TRIGGER;  TRIGGER = " + local
              .Trigger.Text9 + "; Receipt Refund ID = " + local
              .BatchTimestampWorkArea.TextTimestamp;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";

            continue;
          }

          local.FnKpcCourtNotc.ObligorSsn =
            entities.CashReceiptDetail.ObligorSocialSecurityNumber ?? Spaces
            (9);
          local.FnKpcCourtNotc.LegalActionStandardNumber =
            entities.CashReceiptDetail.CourtOrderNumber ?? Spaces(20);
          local.FnKpcCourtNotc.DistributionDate =
            entities.ReceiptRefund.RequestDate;
          local.FnKpcCourtNotc.Amount = -entities.ReceiptRefund.Amount;
          local.FnKpcCourtNotc.CollectionType = "REFUND";
          local.FileTotal.Amount += local.FnKpcCourtNotc.Amount;
          UseFnEabWriteKpcCourtNotcFile();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            local.EabConvertNumeric.SendNonSuppressPos = 9;
            local.EabConvertNumeric.SendAmount =
              NumberToString(entities.Trigger.Identifier, 15);
            UseEabConvertNumeric1();
            local.Trigger.Text9 =
              Substring(local.EabConvertNumeric.ReturnNoCommasInNonDecimal, 7, 9);
              
            local.EabReportSend.RptDetail =
              "ABEND: Writing Refund to file; TRIGGER = " + local
              .Trigger.Text9;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            ExitState = "ACO_NN0000_ABEND_4_BATCH";

            goto ReadEach;
          }

          ++local.TriggersProcessed.Count;
          ++local.ControlTotalProcessed.Count;

          break;
        default:
          break;
      }
    }

ReadEach:

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // ---------------------------------------------------------
      // Update Checkpoint_Restart Last_Checkpoint_Timestamp.
      // ------------------------------------------------------------
      if (Equal(local.TimeLimit.Timestamp, local.Max.Timestamp))
      {
        // mjr
        // ---------------------------------------------------------
        // Only perform update if this is a current run, not one for
        // recreating a previous file
        // ------------------------------------------------------------
        local.ProgramCheckpointRestart.ProgramName =
          local.ProgramProcessingInfo.Name;
        local.ProgramCheckpointRestart.RestartInd = "N";
        local.ProgramCheckpointRestart.RestartInfo = "";
        local.ProgramCheckpointRestart.CheckpointCount = 1;
        UseUpdatePgmCheckpointRestart();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Error updating checkpoint_restart";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_4_BATCH";
        }
      }
    }

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseFnB711WriteControlsAndClose();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }

    // Close ADABAS per CQ 7812
    UseSiCloseAdabas();
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Ssn = source.Ssn;
  }

  private static void MoveEabConvertNumeric1(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.ReturnAmountNonDecimalSigned = source.ReturnAmountNonDecimalSigned;
    target.ReturnNoCommasInNonDecimal = source.ReturnNoCommasInNonDecimal;
  }

  private static void MoveEabConvertNumeric3(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric3(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    MoveEabConvertNumeric1(local.EabConvertNumeric, useExport.EabConvertNumeric);
      

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    MoveEabConvertNumeric1(useExport.EabConvertNumeric, local.EabConvertNumeric);
      
  }

  private void UseFnB711Housekeeping()
  {
    var useImport = new FnB711Housekeeping.Import();
    var useExport = new FnB711Housekeeping.Export();

    Call(FnB711Housekeeping.Execute, useImport, useExport);

    local.Max.Timestamp = useExport.Max.Timestamp;
    local.TimeLimit.Timestamp = useExport.TimeLimit.Timestamp;
    local.Parm.Assign(useExport.Trigger);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.ParmRefund.Assign(useExport.Refund);
  }

  private void UseFnB711WriteControlsAndClose()
  {
    var useImport = new FnB711WriteControlsAndClose.Import();
    var useExport = new FnB711WriteControlsAndClose.Export();

    useImport.FileTotal.Amount = local.FileTotal.Amount;
    useImport.Erred.Count = local.ControlTotalErred.Count;
    useImport.Read.Count = local.ControlTotalRead.Count;
    useImport.Processed.Count = local.ControlTotalProcessed.Count;

    Call(FnB711WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseFnEabWriteKpcCourtNotcFile()
  {
    var useImport = new FnEabWriteKpcCourtNotcFile.Import();
    var useExport = new FnEabWriteKpcCourtNotcFile.Export();

    useImport.FnKpcCourtNotc.Assign(local.FnKpcCourtNotc);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnEabWriteKpcCourtNotcFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          local.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          local.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          local.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 5);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadCollection()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetString(command, "cspNumber", local.CsePerson.Number);
        db.SetNullableDate(
          command, "crtNoticeProcDt",
          local.TriggerReference.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo",
          local.FnKpcCourtNotc.LegalActionStandardNumber);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.CarId = db.GetNullableInt32(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.Amount = db.GetDecimal(reader, 14);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 15);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 16);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 17);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCollectionAdjustmentReason()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionAdjustmentReason.Populated = false;

    return Read("ReadCollectionAdjustmentReason",
      (db, command) =>
      {
        db.SetInt32(
          command, "obTrnRlnRsnId",
          entities.Collection.CarId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionAdjustmentReason.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CollectionAdjustmentReason.Code = db.GetString(reader, 1);
        entities.CollectionAdjustmentReason.Name = db.GetString(reader, 2);
        entities.CollectionAdjustmentReason.Populated = true;
      });
  }

  private bool ReadCollectionType()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Name = db.GetString(reader, 2);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", local.LegalAction1.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadReceiptRefundCashReceiptDetail()
  {
    entities.ReceiptRefund.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadReceiptRefundCashReceiptDetail",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          entities.Trigger.DenormTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.Taxid = db.GetNullableString(reader, 1);
        entities.ReceiptRefund.Amount = db.GetDecimal(reader, 2);
        entities.ReceiptRefund.RequestDate = db.GetDate(reader, 3);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 4);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 4);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 5);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 5);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 6);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 6);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 7);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 7);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 9);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 10);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 11);
        entities.CashReceiptDetail.CltIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ReceiptRefund.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private IEnumerable<bool> ReadTrigger()
  {
    entities.Trigger.Populated = false;

    return ReadEach("ReadTrigger",
      (db, command) =>
      {
        db.SetString(command, "type1", local.Parm.Type1);
        db.SetString(command, "type2", local.ParmRefund.Type1);
        db.SetNullableString(command, "status", local.Parm.Status ?? "");
        db.SetNullableDateTime(
          command, "createdTimestamp1",
          local.Parm.CreatedTimestamp.GetValueOrDefault());
        db.SetNullableDateTime(
          command, "createdTimestamp2",
          local.TimeLimit.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Trigger.Identifier = db.GetInt32(reader, 0);
        entities.Trigger.Type1 = db.GetString(reader, 1);
        entities.Trigger.Action = db.GetNullableString(reader, 2);
        entities.Trigger.Status = db.GetNullableString(reader, 3);
        entities.Trigger.DenormNumeric1 = db.GetNullableInt32(reader, 4);
        entities.Trigger.DenormNumeric2 = db.GetNullableInt32(reader, 5);
        entities.Trigger.DenormNumeric3 = db.GetNullableInt32(reader, 6);
        entities.Trigger.DenormText1 = db.GetNullableString(reader, 7);
        entities.Trigger.DenormText2 = db.GetNullableString(reader, 8);
        entities.Trigger.DenormText3 = db.GetNullableString(reader, 9);
        entities.Trigger.CreatedTimestamp = db.GetNullableDateTime(reader, 10);
        entities.Trigger.UpdatedTimestamp = db.GetNullableDateTime(reader, 11);
        entities.Trigger.DenormTimestamp = db.GetNullableDateTime(reader, 12);
        entities.Trigger.Populated = true;

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
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of ParmRefund.
    /// </summary>
    [JsonPropertyName("parmRefund")]
    public Trigger ParmRefund
    {
      get => parmRefund ??= new();
      set => parmRefund = value;
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
    /// A value of TimeLimit.
    /// </summary>
    [JsonPropertyName("timeLimit")]
    public DateWorkArea TimeLimit
    {
      get => timeLimit ??= new();
      set => timeLimit = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of TriggerReference.
    /// </summary>
    [JsonPropertyName("triggerReference")]
    public DateWorkArea TriggerReference
    {
      get => triggerReference ??= new();
      set => triggerReference = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of FnKpcCourtNotc.
    /// </summary>
    [JsonPropertyName("fnKpcCourtNotc")]
    public FnKpcCourtNotc FnKpcCourtNotc
    {
      get => fnKpcCourtNotc ??= new();
      set => fnKpcCourtNotc = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public Common Temp
    {
      get => temp ??= new();
      set => temp = value;
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
    /// A value of LegalAction1.
    /// </summary>
    [JsonPropertyName("legalAction1")]
    public LegalAction LegalAction1
    {
      get => legalAction1 ??= new();
      set => legalAction1 = value;
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
    /// A value of TriggersErred.
    /// </summary>
    [JsonPropertyName("triggersErred")]
    public Common TriggersErred
    {
      get => triggersErred ??= new();
      set => triggersErred = value;
    }

    /// <summary>
    /// A value of ControlTotalErred.
    /// </summary>
    [JsonPropertyName("controlTotalErred")]
    public Common ControlTotalErred
    {
      get => controlTotalErred ??= new();
      set => controlTotalErred = value;
    }

    /// <summary>
    /// A value of ControlTotalRead.
    /// </summary>
    [JsonPropertyName("controlTotalRead")]
    public Common ControlTotalRead
    {
      get => controlTotalRead ??= new();
      set => controlTotalRead = value;
    }

    /// <summary>
    /// A value of ControlTotalProcessed.
    /// </summary>
    [JsonPropertyName("controlTotalProcessed")]
    public Common ControlTotalProcessed
    {
      get => controlTotalProcessed ??= new();
      set => controlTotalProcessed = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public WorkArea Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
    }

    /// <summary>
    /// A value of LegalAction2.
    /// </summary>
    [JsonPropertyName("legalAction2")]
    public WorkArea LegalAction2
    {
      get => legalAction2 ??= new();
      set => legalAction2 = value;
    }

    /// <summary>
    /// A value of Parm.
    /// </summary>
    [JsonPropertyName("parm")]
    public Trigger Parm
    {
      get => parm ??= new();
      set => parm = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
    }

    /// <summary>
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RowLock.
    /// </summary>
    [JsonPropertyName("rowLock")]
    public Common RowLock
    {
      get => rowLock ??= new();
      set => rowLock = value;
    }

    /// <summary>
    /// A value of TriggersRead.
    /// </summary>
    [JsonPropertyName("triggersRead")]
    public Common TriggersRead
    {
      get => triggersRead ??= new();
      set => triggersRead = value;
    }

    /// <summary>
    /// A value of TriggersProcessed.
    /// </summary>
    [JsonPropertyName("triggersProcessed")]
    public Common TriggersProcessed
    {
      get => triggersProcessed ??= new();
      set => triggersProcessed = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SendNotice.
    /// </summary>
    [JsonPropertyName("sendNotice")]
    public Common SendNotice
    {
      get => sendNotice ??= new();
      set => sendNotice = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Collection Total
    {
      get => total ??= new();
      set => total = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of FileTotal.
    /// </summary>
    [JsonPropertyName("fileTotal")]
    public Collection FileTotal
    {
      get => fileTotal ??= new();
      set => fileTotal = value;
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
    /// A value of HeaderRecord.
    /// </summary>
    [JsonPropertyName("headerRecord")]
    public WorkArea HeaderRecord
    {
      get => headerRecord ??= new();
      set => headerRecord = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Trigger parmRefund;
    private DateWorkArea max;
    private DateWorkArea timeLimit;
    private WorkArea workArea;
    private DateWorkArea triggerReference;
    private AbendData abendData;
    private FnKpcCourtNotc fnKpcCourtNotc;
    private Common temp;
    private CsePerson csePerson;
    private LegalAction legalAction1;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private Common triggersErred;
    private Common controlTotalErred;
    private Common controlTotalRead;
    private Common controlTotalProcessed;
    private WorkArea trigger;
    private WorkArea legalAction2;
    private Trigger parm;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private EabConvertNumeric2 eabConvertNumeric;
    private Common debugOn;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private Common rowLock;
    private Common triggersRead;
    private Common triggersProcessed;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common sendNotice;
    private Collection total;
    private Collection collection;
    private DateWorkArea null1;
    private CollectionType collectionType;
    private Collection fileTotal;
    private DateWorkArea dateWorkArea;
    private WorkArea headerRecord;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Trigger.
    /// </summary>
    [JsonPropertyName("trigger")]
    public Trigger Trigger
    {
      get => trigger ??= new();
      set => trigger = value;
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private ReceiptRefund receiptRefund;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private Collection collection;
    private CsePerson csePerson;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private LegalAction legalAction;
    private Trigger trigger;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private CollectionType collectionType;
  }
#endregion
}

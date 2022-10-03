// Program: LE_B588_INCOMING_EIWO_RESULTS, ID: 1902488195, model: 746.
// Short name: SWEL588P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B588_INCOMING_EIWO_RESULTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB588IncomingEiwoResults: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B588_INCOMING_EIWO_RESULTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB588IncomingEiwoResults(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB588IncomingEiwoResults.
  /// </summary>
  public LeB588IncomingEiwoResults(IContext context, Import import,
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
    // **************************************************************************
    // *                   Maintenance Log
    // 
    // *
    // **************************************************************************
    // *    DATE       NAME      REQUEST    DESCRIPTION                         
    // *
    // * ----------  ---------  ---------
    // 
    // -----------------------------------
    // *
    // * 07/10/2015  DDupree     CQ22212    Initial Coding.                     
    // *
    // *
    // 
    // *
    // * 03/09/2016  GVandy      CQ51340    Don't abend if errors are           
    // *
    // *
    // 
    // returned from the portal.
    // *
    // *
    // 
    // *
    // * 01/12/2018  GVandy      CQ60959    Increase local_batch_control_group  
    // *
    // *				     size from 100 to 500.               *
    // *
    // 
    // *
    // **************************************************************************
    // ***********************************************************************
    // First run through the file to make sure it is complete
    // ***********************************************************************
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProcessDate = Now().Date;
    local.EabReportSend.ProgramName = global.UserId;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabFileHandling.Action = "OPEN";
    UseLeB588ReadFile2();

    switch(TrimEnd(local.ReturnCode.TextReturnCode))
    {
      case "00":
        break;
      case "EF":
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "ERROR OPENING EIWO EMPLOYER INPUT FILE";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    local.BatchControl.Index = -1;
    local.BatchControl.Count = 0;
    local.FileControl.Index = -1;
    local.FileControl.Count = 0;
    local.DetailRecordFound.Flag = "";

    do
    {
      local.EabFileHandling.Action = "READ";

      // If the program gets any "**I" records instead of "**S" records it will 
      // fail the entire file.
      UseLeB588ReadFile1();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle1;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING EIWO EMPLOYER INPUT FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle1;
      }

      ++local.RecordsRead.Count;

      switch(TrimEnd(local.DocCode.Text3))
      {
        case "FHI":
          ++local.FileControl.Index;
          local.FileControl.CheckSize();

          local.FileControl.Update.FileControl1.Text22 =
            local.ControlNum.Text22;
          local.FileControl.Update.ErrorCode.Text18 = local.ErrFieldName.Text18;
          local.FileControl.Update.AbortFile.Text1 = "Y";
          local.CurrentFileControlErrd.Flag = "Y";
          local.DetailRecordFound.Flag = "Y";
          local.FileControl1.Text22 = local.ControlNum.Text22;

          break;
        case "BHI":
          local.DetailRecordFound.Flag = "Y";

          break;
        case "FTI":
          local.DetailRecordFound.Flag = "Y";

          break;
        case "BTI":
          local.DetailRecordFound.Flag = "Y";

          break;
        case "DTL":
          break;
        default:
          if (!Equal(local.FileControl1.Text22, local.ControlNum.Text22) && Equal
            (local.DocCode.Text3, "FHS"))
          {
            local.CurrentFileControlErrd.Flag = "";
            local.FileControl1.Text22 = local.ControlNum.Text22;

            ++local.FileControl.Index;
            local.FileControl.CheckSize();

            local.FileControl.Update.FileControl1.Text22 =
              local.ControlNum.Text22;
            local.FileControl.Update.ErrorCode.Text18 =
              local.ErrFieldName.Text18;
          }

          if (Equal(local.DocCode.Text3, "BHS") && IsEmpty
            (local.ErrFieldName.Text18))
          {
            ++local.BatchControl.Index;
            local.BatchControl.CheckSize();

            local.BatchControl.Update.BatchConrtol.Text22 =
              local.ControlNum.Text22;
          }

          continue;
      }
    }
    while(!Equal(local.ReturnCode.TextReturnCode, "EF"));

AfterCycle1:

    if (AsChar(local.DetailRecordFound.Flag) == 'Y')
    {
      local.FileControl.Index = 0;

      for(var limit = local.FileControl.Count; local.FileControl.Index < limit; ++
        local.FileControl.Index)
      {
        if (!local.FileControl.CheckSize())
        {
          break;
        }

        if (AsChar(local.FileControl.Item.AbortFile.Text1) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "RECIEVED AN **I, NO RECORDS IN FILE CONTROL PROCESSED FOR FILE CONTROL NUMBER:" +
            local.FileControl.Item.FileControl1.Text22;
          UseCabErrorReport1();

          if (!IsEmpty(local.FileControl.Item.ErrorCode.Text18))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail = "For file control number:" + local
              .FileControl.Item.FileControl1.Text22 + " Received the following errors: " +
              local.FileControl.Item.ErrorCode.Text18;
            UseCabErrorReport1();
          }
        }
        else
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "RECIEVED A FILE CONTROL THAT COULD NOT BE PROCESSED SO NO FILES COULD BE PROCESSED, FILE CONTROL NUMBER:" +
            local.FileControl.Item.FileControl1.Text22;
          UseCabErrorReport1();
        }
      }

      local.FileControl.CheckIndex();
      ExitState = "ACO_NE0000_FILE_REJECT_BY_PORTAL";

      return;
    }

    local.DtlRecords.Count = 0;
    local.EabFileHandling.Action = "CLOSE";
    UseLeB588ReadFile2();

    switch(TrimEnd(local.ReturnCode.TextReturnCode))
    {
      case "00":
        break;
      case "EF":
        break;
      default:
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail = "ERROR CLOSING EIWO PORTAL INPUT FILE";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = new DateTime(2099, 12, 31);
    UseLeB588Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.RecordsRead.Count = 0;
    local.FhsRecords.Count = 0;
    local.BhsRecords.Count = 0;
    local.FtsRecords.Count = 0;
    local.BhsRecords.Count = 0;
    local.DtlRecords.Count = 0;
    local.NoErrRecords.Count = 0;

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.Restart.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 1, 15));

      do
      {
        local.EabFileHandling.Action = "READ";
        UseLeB588ReadFile2();

        switch(TrimEnd(local.ReturnCode.TextReturnCode))
        {
          case "00":
            break;
          case "EF":
            break;
          default:
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "ERROR READING EIWO EMPLOYER INPUT FILE";
            UseCabErrorReport1();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
        }

        ++local.RecordsRead.Count;

        if (local.Restart.Count == local.RecordsRead.Count)
        {
          goto Test;
        }
      }
      while(!Equal(local.ReturnCode.TextReturnCode, "EF"));
    }

Test:

    // **********************************************************
    // Read each input record from the file.
    // **********************************************************
    do
    {
      local.EabFileHandling.Action = "READ";
      UseLeB588ReadFile1();

      switch(TrimEnd(local.ReturnCode.TextReturnCode))
      {
        case "00":
          break;
        case "EF":
          goto AfterCycle2;
        default:
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "ERROR READING EIWO EMPLOYER INPUT FILE";
          UseCabErrorReport1();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto AfterCycle2;
      }

      local.IwoAction.Assign(local.Clear);
      ++local.RecordsRead.Count;
      ++local.Commit.Count;

      switch(TrimEnd(local.DocCode.Text3))
      {
        case "FHS":
          local.FileControl1.Text22 = local.ControlNum.Text22;
          local.RecordFound.Flag = "";
          ++local.FhsRecords.Count;

          break;
        case "BHS":
          local.RecordFound.Flag = "";
          ++local.BhsRecords.Count;

          if (!IsEmpty(local.ErrFieldName.Text18))
          {
            local.AbortProgram.Flag = "Y";

            foreach(var item in ReadIwoAction2())
            {
              local.RecordFound.Flag = "Y";
              MoveIwoAction(entities.IwoAction, local.IwoAction);

              try
              {
                UpdateIwoAction1();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "IWO_ACTION_NU";

                    // get exitstate iwo action nu
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "IWO_ACTION_PV";

                    // get exitstate iwo action pv
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (ReadIwoTransactionLegalActionCsePerson())
              {
                local.IwoAction.Identifier = entities.IwoAction.Identifier;
                local.IwoAction.StatusCd = "E";
                local.IwoAction.StatusReasonCode =
                  Substring(local.ErrFieldName.Text18, 1, 3);
                UseLeUpdateIwoActionStatus();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabExtractExitStateMessage();
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error for IWO ACTION CONTROL # " + entities
                    .IwoAction.FileControlId + "  " + local
                    .ExitStateWorkArea.Message;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "IWO_TRANSACTION_NF";

                // get exitstate iwo action nu
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error for IWO ACTION CONTROL # " + entities
                  .IwoAction.FileControlId + "  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                goto Next;
              }
            }

            if (IsEmpty(local.RecordFound.Flag))
            {
              ExitState = "IWO_ACTION_NF";

              // get exitstate iwo action nu
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error for IWO ACTION CONTROL # " + entities
                .IwoAction.FileControlId + "  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            continue;
          }

          break;
        case "FTS":
          local.RecordFound.Flag = "";
          ++local.FtsRecords.Count;

          break;
        case "BTS":
          local.RecordFound.Flag = "";
          ++local.BtsRecords.Count;

          if (!IsEmpty(local.ErrFieldName.Text18))
          {
            local.AbortProgram.Flag = "Y";

            foreach(var item in ReadIwoAction2())
            {
              local.RecordFound.Flag = "Y";

              try
              {
                UpdateIwoAction2();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "IWO_ACTION_NU";

                    // get exitstate iwo action nu
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "IWO_ACTION_PV";

                    // get exitstate iwo action pv
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (ReadIwoTransactionLegalActionCsePerson())
              {
                local.IwoAction.Identifier = entities.IwoAction.Identifier;
                local.IwoAction.StatusCd = "E";
                local.IwoAction.StatusReasonCode =
                  Substring(local.ErrFieldName.Text18, 1, 3);
                UseLeUpdateIwoActionStatus();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabExtractExitStateMessage();
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error for IWO ACTION CONTROL # " + entities
                    .IwoAction.FileControlId + "  " + local
                    .ExitStateWorkArea.Message;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "IWO_TRANSACTION_NF";

                // get exitstate iwo action nu
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error for IWO ACTION CONTROL # " + entities
                  .IwoAction.FileControlId + "  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                goto Next;
              }
            }

            if (IsEmpty(local.RecordFound.Flag))
            {
              ExitState = "IWO_ACTION_NF";

              // get exitstate iwo action nu
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error for IWO ACTION CONTROL # " + entities
                .IwoAction.FileControlId + "  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            continue;
          }

          break;
        case "DTL":
          local.RecordFound.Flag = "";
          ++local.DtlRecords.Count;

          if (!IsEmpty(local.Local2NdErrorFieldName.Text32) || !
            IsEmpty(local.Local1StErrorFieldName.Text32) || AsChar
            (local.MultipleErrorInd.Text1) == 'T')
          {
            local.AbortProgram.Flag = "Y";
            local.DocTrackingNumb.Text12 =
              Substring(local.DocTrackingNumb.Text30, 19, 12);

            if (!IsEmpty(local.Local1StErrorFieldName.Text32))
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "For document tracking number: " + local
                .DocTrackingNumb.Text12 + " Received the following error: " + local
                .Local1StErrorFieldName.Text32;
              UseCabErrorReport1();
            }

            if (!IsEmpty(local.Local2NdErrorFieldName.Text32))
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "For document tracking number: " + local
                .DocTrackingNumb.Text12 + " Received the following error: " + local
                .Local2NdErrorFieldName.Text32;
              UseCabErrorReport1();
            }

            if (AsChar(local.MultipleErrorInd.Text1) == 'T')
            {
              local.EabFileHandling.Action = "WRITE";
              local.NeededToWrite.RptDetail =
                "For document tracking number: " + local
                .DocTrackingNumb.Text12 + "Multiple Error Indicator was True" +
                " ";
              UseCabErrorReport1();
            }

            foreach(var item in ReadIwoAction3())
            {
              local.RecordFound.Flag = "Y";
              MoveIwoAction(entities.IwoAction, local.IwoAction);

              try
              {
                UpdateIwoAction3();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "IWO_ACTION_NU";

                    // get exitstate iwo action nu
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "IWO_ACTION_PV";

                    // get exitstate iwo action pv
                    UseEabExtractExitStateMessage();
                    local.EabFileHandling.Action = "WRITE";
                    local.EabReportSend.RptDetail =
                      "Error for IWO ACTION CONTROL # " + entities
                      .IwoAction.FileControlId + "  " + local
                      .ExitStateWorkArea.Message;
                    UseCabErrorReport2();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    }

                    goto Next;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              if (ReadIwoTransactionLegalActionCsePerson())
              {
                local.IwoAction.Identifier = entities.IwoAction.Identifier;
                local.IwoAction.StatusCd = "E";
                local.IwoAction.StatusReasonCode =
                  Substring(local.ErrFieldName.Text18, 1, 3);
                UseLeUpdateIwoActionStatus();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  UseEabExtractExitStateMessage();
                  local.EabFileHandling.Action = "WRITE";
                  local.EabReportSend.RptDetail =
                    "Error for IWO ACTION CONTROL # " + entities
                    .IwoAction.FileControlId + "  " + local
                    .ExitStateWorkArea.Message;
                  UseCabErrorReport2();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  }
                }
              }
              else
              {
                ExitState = "IWO_TRANSACTION_NF";

                // get exitstate iwo action nu
                UseEabExtractExitStateMessage();
                local.EabFileHandling.Action = "WRITE";
                local.EabReportSend.RptDetail =
                  "Error for IWO ACTION CONTROL # " + entities
                  .IwoAction.FileControlId + "  " + local
                  .ExitStateWorkArea.Message;
                UseCabErrorReport2();

                if (!Equal(local.EabFileHandling.Status, "OK"))
                {
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                goto Next;
              }
            }

            if (IsEmpty(local.RecordFound.Flag))
            {
              ExitState = "IWO_ACTION_NF";

              // get exitstate iwo action nu
              UseEabExtractExitStateMessage();
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail =
                "Error for IWO ACTION CONTROL # " + entities
                .IwoAction.FileControlId + "  " + local
                .ExitStateWorkArea.Message;
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                return;
              }

              continue;
            }

            continue;
          }

          break;
        default:
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Received an invalid record type. Record type received was: " + local
            .DocCode.Text3;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          continue;
      }

      ++local.NoErrRecords.Count;

      // *************** Check to see if commit is needed ********************
      ++local.Commit.Count;

      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.Commit.Count = 0;
        local.EabFileHandling.Action = "WRITE";
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(local.RecordsRead.Count, 250);
        UseCabErrorReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

          break;
        }
      }

Next:
      ;
    }
    while(!Equal(local.ReturnCode.TextReturnCode, "EF"));

AfterCycle2:

    local.BatchControl.Index = 0;

    for(var limit = local.BatchControl.Count; local.BatchControl.Index < limit; ++
      local.BatchControl.Index)
    {
      if (!local.BatchControl.CheckSize())
      {
        break;
      }

      foreach(var item in ReadIwoAction1())
      {
        local.IwoAction.Assign(local.Clear);
        MoveIwoAction(entities.IwoAction, local.IwoAction);

        if (ReadIwoTransactionLegalActionCsePerson())
        {
          local.IwoAction.Identifier = entities.IwoAction.Identifier;
          local.IwoAction.StatusCd = "R";
          UseLeUpdateIwoActionStatus();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabExtractExitStateMessage();
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail =
              "Error for IWO ACTION CONTROL # " + entities
              .IwoAction.FileControlId + "  " + local
              .ExitStateWorkArea.Message;
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }
        else
        {
          ExitState = "IWO_TRANSACTION_NF";

          // get exitstate iwo action nu
          UseEabExtractExitStateMessage();
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error for IWO ACTION CONTROL # " + entities
            .IwoAction.FileControlId + "  " + local.ExitStateWorkArea.Message;
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }
    }

    local.BatchControl.CheckIndex();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseLeB588Close();
      local.ProgramCheckpointRestart.CheckpointCount = -1;
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.ProgramName =
        local.ProgramProcessingInfo.Name;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();
    }
    else
    {
      local.EabFileHandling.Action = "WRITE";
      UseEabExtractExitStateMessage();
      local.NeededToWrite.RptDetail = "Program abended because: " + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ALL_OK";
      UseLeB588Close();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // -- 03/09/2016 GVandy CQ51340 Don't abend if errors are returned from the 
    // portal.
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.Identifier = source.Identifier;
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
    target.DocumentTrackingIdentifier = source.DocumentTrackingIdentifier;
    target.FileControlId = source.FileControlId;
    target.BatchControlId = source.BatchControlId;
    target.ErrorRecordType = source.ErrorRecordType;
    target.ErrorField1 = source.ErrorField1;
    target.ErrorField2 = source.ErrorField2;
    target.MoreErrorsInd = source.MoreErrorsInd;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.ProgramName = source.ProgramName;
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

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

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

    useExport.External.NumericReturnCode = local.ReturnCode.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ReturnCode.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseLeB588Close()
  {
    var useImport = new LeB588Close.Import();
    var useExport = new LeB588Close.Export();

    useImport.NoErrRecords.Count = local.NoErrRecords.Count;
    useImport.DtlRecords.Count = local.DtlRecords.Count;
    useImport.BtiRecors.Count = local.BtsRecords.Count;
    useImport.RecordsRead.Count = local.RecordsRead.Count;
    useImport.FhiRecords.Count = local.FhsRecords.Count;
    useImport.BhiRecords.Count = local.BhsRecords.Count;
    useImport.FtiRecords.Count = local.FtsRecords.Count;

    Call(LeB588Close.Execute, useImport, useExport);
  }

  private void UseLeB588Housekeeping()
  {
    var useImport = new LeB588Housekeeping.Import();
    var useExport = new LeB588Housekeeping.Export();

    Call(LeB588Housekeeping.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
    local.Process.Date = useExport.Process.Date;
    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
  }

  private void UseLeB588ReadFile1()
  {
    var useImport = new LeB588ReadFile.Import();
    var useExport = new LeB588ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.ErrFieldName.Text18 = local.ErrFieldName.Text18;
    useExport.HeaderCreateTime.Time = local.HeaderCreateTime.Time;
    useExport.HeaderCreateDate.Date = local.HeaderCreateDate.Date;
    useExport.HeaderPrimaryEin.Text9 = local.HeaderPrimaryEin.Text9;
    useExport.HeaderEin.Text9 = local.HeaderEin.Text9;
    useExport.StFipsCode.Text5 = local.StFipsCode.Text5;
    useExport.ControlNum.Text22 = local.ControlNum.Text22;
    useExport.DocCode.Text3 = local.DocCode.Text3;
    useExport.DocActionCode.Text3 = local.DocActionCode.Text3;
    useExport.Employer.Ein = local.Employer.Ein;
    useExport.DocTrackingNumb.Text30 = local.DocTrackingNumb.Text30;
    useExport.RecDispStatsCd.Text2 = local.RecDispStatsCd.Text2;
    useExport.DisposionReasonCd.Text3 = local.DisposionReasonCd.Text3;
    useExport.Export1StErrorFieldName.Text32 =
      local.Local1StErrorFieldName.Text32;
    useExport.Export2NdErrorFieldName.Text32 =
      local.Local2NdErrorFieldName.Text32;
    useExport.MultipleErrorInd.Text1 = local.MultipleErrorInd.Text1;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB588ReadFile.Execute, useImport, useExport);

    local.ErrFieldName.Text18 = useExport.ErrFieldName.Text18;
    local.HeaderCreateTime.Time = useExport.HeaderCreateTime.Time;
    local.HeaderCreateDate.Date = useExport.HeaderCreateDate.Date;
    local.HeaderPrimaryEin.Text9 = useExport.HeaderPrimaryEin.Text9;
    local.HeaderEin.Text9 = useExport.HeaderEin.Text9;
    local.StFipsCode.Text5 = useExport.StFipsCode.Text5;
    local.ControlNum.Text22 = useExport.ControlNum.Text22;
    local.DocCode.Text3 = useExport.DocCode.Text3;
    local.DocActionCode.Text3 = useExport.DocActionCode.Text3;
    local.Employer.Ein = useExport.Employer.Ein;
    local.DocTrackingNumb.Text30 = useExport.DocTrackingNumb.Text30;
    local.RecDispStatsCd.Text2 = useExport.RecDispStatsCd.Text2;
    local.DisposionReasonCd.Text3 = useExport.DisposionReasonCd.Text3;
    local.Local1StErrorFieldName.Text32 =
      useExport.Export1StErrorFieldName.Text32;
    local.Local2NdErrorFieldName.Text32 =
      useExport.Export2NdErrorFieldName.Text32;
    local.MultipleErrorInd.Text1 = useExport.MultipleErrorInd.Text1;
    local.ReturnCode.Assign(useExport.External);
  }

  private void UseLeB588ReadFile2()
  {
    var useImport = new LeB588ReadFile.Import();
    var useExport = new LeB588ReadFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.External.Assign(local.ReturnCode);

    Call(LeB588ReadFile.Execute, useImport, useExport);

    local.ReturnCode.Assign(useExport.External);
  }

  private void UseLeUpdateIwoActionStatus()
  {
    var useImport = new LeUpdateIwoActionStatus.Import();
    var useExport = new LeUpdateIwoActionStatus.Export();

    useImport.IwoTransaction.Identifier = entities.IwoTransaction.Identifier;
    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.LegalAction.Identifier = entities.LegalAction.Identifier;
    useImport.IwoAction.Assign(local.IwoAction);

    Call(LeUpdateIwoActionStatus.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadIwoAction1()
  {
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoAction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "batchControlId",
          local.BatchControl.Item.BatchConrtol.Text22);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 3);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 4);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 8);
        entities.IwoAction.MoreErrorsInd = db.GetNullableString(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIwoAction2()
  {
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoAction2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "batchControlId", local.ControlNum.Text22);
          
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 3);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 4);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 8);
        entities.IwoAction.MoreErrorsInd = db.GetNullableString(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoAction.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIwoAction3()
  {
    entities.IwoAction.Populated = false;

    return ReadEach("ReadIwoAction3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "docTrackingId", local.DocTrackingNumb.Text12);
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 3);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 4);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 5);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 6);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 7);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 8);
        entities.IwoAction.MoreErrorsInd = db.GetNullableString(reader, 9);
        entities.IwoAction.CspNumber = db.GetString(reader, 10);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 11);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 12);
        entities.IwoAction.Populated = true;

        return true;
      });
  }

  private bool ReadIwoTransactionLegalActionCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);
    entities.IwoTransaction.Populated = false;
    entities.CsePerson.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadIwoTransactionLegalActionCsePerson",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "identifier", entities.IwoAction.IwtIdentifier);
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 1);
        entities.LegalAction.Identifier = db.GetInt32(reader, 1);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 2);
        entities.CsePerson.Number = db.GetString(reader, 2);
        entities.IwoTransaction.Populated = true;
        entities.CsePerson.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateIwoAction1()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var errorRecordType = "BHS";
    var errorField1 = local.ErrFieldName.Text18;

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction1",
      (db, command) =>
      {
        db.SetNullableString(command, "errorRecordType", errorRecordType);
        db.SetNullableString(command, "errorField1", errorField1);
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.ErrorRecordType = errorRecordType;
    entities.IwoAction.ErrorField1 = errorField1;
    entities.IwoAction.Populated = true;
  }

  private void UpdateIwoAction2()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var errorRecordType = "BTS";
    var errorField1 = local.ErrFieldName.Text18;

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction2",
      (db, command) =>
      {
        db.SetNullableString(command, "errorRecordType", errorRecordType);
        db.SetNullableString(command, "errorField1", errorField1);
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.ErrorRecordType = errorRecordType;
    entities.IwoAction.ErrorField1 = errorField1;
    entities.IwoAction.Populated = true;
  }

  private void UpdateIwoAction3()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var errorRecordType = "DTL";
    var errorField1 = local.Local1StErrorFieldName.Text32;
    var errorField2 = local.Local2NdErrorFieldName.Text32;
    var moreErrorsInd = local.MultipleErrorInd.Text1;

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction3",
      (db, command) =>
      {
        db.SetNullableString(command, "errorRecordType", errorRecordType);
        db.SetNullableString(command, "errorField1", errorField1);
        db.SetNullableString(command, "errorField2", errorField2);
        db.SetNullableString(command, "moreErrorsInd", moreErrorsInd);
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.ErrorRecordType = errorRecordType;
    entities.IwoAction.ErrorField1 = errorField1;
    entities.IwoAction.ErrorField2 = errorField2;
    entities.IwoAction.MoreErrorsInd = moreErrorsInd;
    entities.IwoAction.Populated = true;
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
    /// <summary>A BatchControlGroup group.</summary>
    [Serializable]
    public class BatchControlGroup
    {
      /// <summary>
      /// A value of BatchConrtol.
      /// </summary>
      [JsonPropertyName("batchConrtol")]
      public WorkArea BatchConrtol
      {
        get => batchConrtol ??= new();
        set => batchConrtol = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 500;

      private WorkArea batchConrtol;
    }

    /// <summary>A FileControlGroup group.</summary>
    [Serializable]
    public class FileControlGroup
    {
      /// <summary>
      /// A value of AbortFile.
      /// </summary>
      [JsonPropertyName("abortFile")]
      public WorkArea AbortFile
      {
        get => abortFile ??= new();
        set => abortFile = value;
      }

      /// <summary>
      /// A value of ErrorCode.
      /// </summary>
      [JsonPropertyName("errorCode")]
      public WorkArea ErrorCode
      {
        get => errorCode ??= new();
        set => errorCode = value;
      }

      /// <summary>
      /// A value of FileControl1.
      /// </summary>
      [JsonPropertyName("fileControl1")]
      public WorkArea FileControl1
      {
        get => fileControl1 ??= new();
        set => fileControl1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private WorkArea abortFile;
      private WorkArea errorCode;
      private WorkArea fileControl1;
    }

    /// <summary>
    /// Gets a value of BatchControl.
    /// </summary>
    [JsonIgnore]
    public Array<BatchControlGroup> BatchControl => batchControl ??= new(
      BatchControlGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of BatchControl for json serialization.
    /// </summary>
    [JsonPropertyName("batchControl")]
    [Computed]
    public IList<BatchControlGroup> BatchControl_Json
    {
      get => batchControl;
      set => BatchControl.Assign(value);
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public IwoAction Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// Gets a value of FileControl.
    /// </summary>
    [JsonIgnore]
    public Array<FileControlGroup> FileControl => fileControl ??= new(
      FileControlGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FileControl for json serialization.
    /// </summary>
    [JsonPropertyName("fileControl")]
    [Computed]
    public IList<FileControlGroup> FileControl_Json
    {
      get => fileControl;
      set => FileControl.Assign(value);
    }

    /// <summary>
    /// A value of CurrentFileControlErrd.
    /// </summary>
    [JsonPropertyName("currentFileControlErrd")]
    public Common CurrentFileControlErrd
    {
      get => currentFileControlErrd ??= new();
      set => currentFileControlErrd = value;
    }

    /// <summary>
    /// A value of FileControlOk.
    /// </summary>
    [JsonPropertyName("fileControlOk")]
    public WorkArea FileControlOk
    {
      get => fileControlOk ??= new();
      set => fileControlOk = value;
    }

    /// <summary>
    /// A value of AbortProgram.
    /// </summary>
    [JsonPropertyName("abortProgram")]
    public Common AbortProgram
    {
      get => abortProgram ??= new();
      set => abortProgram = value;
    }

    /// <summary>
    /// A value of DetailRecordFound.
    /// </summary>
    [JsonPropertyName("detailRecordFound")]
    public Common DetailRecordFound
    {
      get => detailRecordFound ??= new();
      set => detailRecordFound = value;
    }

    /// <summary>
    /// A value of FileControl1.
    /// </summary>
    [JsonPropertyName("fileControl1")]
    public WorkArea FileControl1
    {
      get => fileControl1 ??= new();
      set => fileControl1 = value;
    }

    /// <summary>
    /// A value of RecordFound.
    /// </summary>
    [JsonPropertyName("recordFound")]
    public Common RecordFound
    {
      get => recordFound ??= new();
      set => recordFound = value;
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
    /// A value of FileControlNumber.
    /// </summary>
    [JsonPropertyName("fileControlNumber")]
    public WorkArea FileControlNumber
    {
      get => fileControlNumber ??= new();
      set => fileControlNumber = value;
    }

    /// <summary>
    /// A value of NoErrRecords.
    /// </summary>
    [JsonPropertyName("noErrRecords")]
    public Common NoErrRecords
    {
      get => noErrRecords ??= new();
      set => noErrRecords = value;
    }

    /// <summary>
    /// A value of DtlRecords.
    /// </summary>
    [JsonPropertyName("dtlRecords")]
    public Common DtlRecords
    {
      get => dtlRecords ??= new();
      set => dtlRecords = value;
    }

    /// <summary>
    /// A value of BtsRecords.
    /// </summary>
    [JsonPropertyName("btsRecords")]
    public Common BtsRecords
    {
      get => btsRecords ??= new();
      set => btsRecords = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of MultipleErrorInd.
    /// </summary>
    [JsonPropertyName("multipleErrorInd")]
    public WorkArea MultipleErrorInd
    {
      get => multipleErrorInd ??= new();
      set => multipleErrorInd = value;
    }

    /// <summary>
    /// A value of Local2NdErrorFieldName.
    /// </summary>
    [JsonPropertyName("local2NdErrorFieldName")]
    public WorkArea Local2NdErrorFieldName
    {
      get => local2NdErrorFieldName ??= new();
      set => local2NdErrorFieldName = value;
    }

    /// <summary>
    /// A value of Local1StErrorFieldName.
    /// </summary>
    [JsonPropertyName("local1StErrorFieldName")]
    public WorkArea Local1StErrorFieldName
    {
      get => local1StErrorFieldName ??= new();
      set => local1StErrorFieldName = value;
    }

    /// <summary>
    /// A value of DisposionReasonCd.
    /// </summary>
    [JsonPropertyName("disposionReasonCd")]
    public WorkArea DisposionReasonCd
    {
      get => disposionReasonCd ??= new();
      set => disposionReasonCd = value;
    }

    /// <summary>
    /// A value of RecDispStatsCd.
    /// </summary>
    [JsonPropertyName("recDispStatsCd")]
    public WorkArea RecDispStatsCd
    {
      get => recDispStatsCd ??= new();
      set => recDispStatsCd = value;
    }

    /// <summary>
    /// A value of DocTrackingNumb.
    /// </summary>
    [JsonPropertyName("docTrackingNumb")]
    public TextWorkArea DocTrackingNumb
    {
      get => docTrackingNumb ??= new();
      set => docTrackingNumb = value;
    }

    /// <summary>
    /// A value of DocActionCode.
    /// </summary>
    [JsonPropertyName("docActionCode")]
    public WorkArea DocActionCode
    {
      get => docActionCode ??= new();
      set => docActionCode = value;
    }

    /// <summary>
    /// A value of ControlNum.
    /// </summary>
    [JsonPropertyName("controlNum")]
    public WorkArea ControlNum
    {
      get => controlNum ??= new();
      set => controlNum = value;
    }

    /// <summary>
    /// A value of StFipsCode.
    /// </summary>
    [JsonPropertyName("stFipsCode")]
    public WorkArea StFipsCode
    {
      get => stFipsCode ??= new();
      set => stFipsCode = value;
    }

    /// <summary>
    /// A value of HeaderEin.
    /// </summary>
    [JsonPropertyName("headerEin")]
    public WorkArea HeaderEin
    {
      get => headerEin ??= new();
      set => headerEin = value;
    }

    /// <summary>
    /// A value of HeaderPrimaryEin.
    /// </summary>
    [JsonPropertyName("headerPrimaryEin")]
    public WorkArea HeaderPrimaryEin
    {
      get => headerPrimaryEin ??= new();
      set => headerPrimaryEin = value;
    }

    /// <summary>
    /// A value of HeaderCreateDate.
    /// </summary>
    [JsonPropertyName("headerCreateDate")]
    public DateWorkArea HeaderCreateDate
    {
      get => headerCreateDate ??= new();
      set => headerCreateDate = value;
    }

    /// <summary>
    /// A value of HeaderCreateTime.
    /// </summary>
    [JsonPropertyName("headerCreateTime")]
    public DateWorkArea HeaderCreateTime
    {
      get => headerCreateTime ??= new();
      set => headerCreateTime = value;
    }

    /// <summary>
    /// A value of ErrFieldName.
    /// </summary>
    [JsonPropertyName("errFieldName")]
    public WorkArea ErrFieldName
    {
      get => errFieldName ??= new();
      set => errFieldName = value;
    }

    /// <summary>
    /// A value of DocCode.
    /// </summary>
    [JsonPropertyName("docCode")]
    public WorkArea DocCode
    {
      get => docCode ??= new();
      set => docCode = value;
    }

    /// <summary>
    /// A value of Change.
    /// </summary>
    [JsonPropertyName("change")]
    public Employer Change
    {
      get => change ??= new();
      set => change = value;
    }

    /// <summary>
    /// A value of FhsRecords.
    /// </summary>
    [JsonPropertyName("fhsRecords")]
    public Common FhsRecords
    {
      get => fhsRecords ??= new();
      set => fhsRecords = value;
    }

    /// <summary>
    /// A value of TimeDel.
    /// </summary>
    [JsonPropertyName("timeDel")]
    public TextWorkArea TimeDel
    {
      get => timeDel ??= new();
      set => timeDel = value;
    }

    /// <summary>
    /// A value of DateDel.
    /// </summary>
    [JsonPropertyName("dateDel")]
    public TextWorkArea DateDel
    {
      get => dateDel ??= new();
      set => dateDel = value;
    }

    /// <summary>
    /// A value of RecordsRead.
    /// </summary>
    [JsonPropertyName("recordsRead")]
    public Common RecordsRead
    {
      get => recordsRead ??= new();
      set => recordsRead = value;
    }

    /// <summary>
    /// A value of BhsRecords.
    /// </summary>
    [JsonPropertyName("bhsRecords")]
    public Common BhsRecords
    {
      get => bhsRecords ??= new();
      set => bhsRecords = value;
    }

    /// <summary>
    /// A value of FtsRecords.
    /// </summary>
    [JsonPropertyName("ftsRecords")]
    public Common FtsRecords
    {
      get => ftsRecords ??= new();
      set => ftsRecords = value;
    }

    /// <summary>
    /// A value of Commit.
    /// </summary>
    [JsonPropertyName("commit")]
    public Common Commit
    {
      get => commit ??= new();
      set => commit = value;
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
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public External ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public Common Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    private Array<BatchControlGroup> batchControl;
    private IwoAction clear;
    private Array<FileControlGroup> fileControl;
    private Common currentFileControlErrd;
    private WorkArea fileControlOk;
    private Common abortProgram;
    private Common detailRecordFound;
    private WorkArea fileControl1;
    private Common recordFound;
    private LegalAction legalAction;
    private WorkArea fileControlNumber;
    private Common noErrRecords;
    private Common dtlRecords;
    private Common btsRecords;
    private IwoAction iwoAction;
    private WorkArea multipleErrorInd;
    private WorkArea local2NdErrorFieldName;
    private WorkArea local1StErrorFieldName;
    private WorkArea disposionReasonCd;
    private WorkArea recDispStatsCd;
    private TextWorkArea docTrackingNumb;
    private WorkArea docActionCode;
    private WorkArea controlNum;
    private WorkArea stFipsCode;
    private WorkArea headerEin;
    private WorkArea headerPrimaryEin;
    private DateWorkArea headerCreateDate;
    private DateWorkArea headerCreateTime;
    private WorkArea errFieldName;
    private WorkArea docCode;
    private Employer change;
    private Common fhsRecords;
    private TextWorkArea timeDel;
    private TextWorkArea dateDel;
    private Common recordsRead;
    private Common bhsRecords;
    private Common ftsRecords;
    private Common commit;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External returnCode;
    private ExitStateWorkArea exitStateWorkArea;
    private DateWorkArea process;
    private DateWorkArea max;
    private DateWorkArea null1;
    private Employer employer;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private AbendData abendData;
    private EabReportSend eabReportSend;
    private ProgramProcessingInfo programProcessingInfo;
    private Common restart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    private IwoTransaction iwoTransaction;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private IwoAction iwoAction;
  }
#endregion
}

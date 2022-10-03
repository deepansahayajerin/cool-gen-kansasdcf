// Program: LE_B570_PROCESS_ASSIGNMNT_TIMING, ID: 372951077, model: 746.
// Short name: SWEL570B
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
/// A program: LE_B570_PROCESS_ASSIGNMNT_TIMING.
/// </para>
/// <para>
/// This procedure is designed to process Infrastructure records not processed 
/// by the Event Processor due to Assignment Timing. This procedure acts as a
/// clean up program that would run daily and change the Infrastructure record
/// status from 'E' to 'Q' for all the Event/Event deails resulting in non-
/// processing of records.
/// =============================================================
/// NOTE - For processing a specific number of records, at MPPI screen (or 
/// Program_Processing_Information entity), set the process date and prefix the
/// parameter list (numbers 2 and 3):
/// 1.  Time from which processing (from program processing date) will commence
/// 2.  Method or processing:  choose between ascending or decending
/// infrastructure created timestamp.
/// 3.  &quot;NUMBER OF RECORDS TO BE PROCESSED = XXX&quot; where &quot;XXX
/// &quot; represents the number of records desired to be processed.  If the
/// parameter list is left blank or if the verbage is not exact, ALL possible
/// records will be processed.
/// =============================================================
/// ADDITIONAL NOTES:
/// If the restart indicator is set to &quot;Y&quot;, it overides all other 
/// settings.  In addition, unless the number of records is equal to or less
/// than the value of &quot;NUMBER OF RECORDS TO BE PROCESSED&quot;, the restart
/// indicator will remain &quot;Y&quot;.
/// Checkpoint restart will be from a particular date.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB570ProcessAssignmntTiming: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B570_PROCESS_ASSIGNMNT_TIMING program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB570ProcessAssignmntTiming(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB570ProcessAssignmntTiming.
  /// </summary>
  public LeB570ProcessAssignmntTiming(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Date		By		Request #
    // Description
    // --------------------------------------------------------------
    // 10/99		Anand Katuri 	H00074517
    // Initial coding
    // 08/14/2000	SWSRPRM		PR # 101777
    // Added PPI Parm usage logic for a predetermined number of
    // records to be processed
    // 11/20/2000	SWSRPRM		PR # 107570
    // Added extra PPI parms - commence processing from the
    // Program Processing Info process_date, # of recs and choice
    // of how to process (ascending or descending).
    // Changed checkpoint restart to be from a particular date the
    // infrastructure record was created instead of a particular
    // record identifier number.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
      {
        local.ProcessMethod.Command = "D";
      }
      else
      {
        local.ProcessMethod.Command =
          Substring(local.ProgramProcessingInfo.ParameterList, 43, 1);
        local.ParameterTextLength.Count =
          Find(local.ProgramProcessingInfo.ParameterList, "=") + 2;
        local.ParameterStringLength.Count =
          Length(local.ProgramProcessingInfo.ParameterList);
        local.TextCount.AddrZip4 =
          Substring(local.ProgramProcessingInfo.ParameterList,
          local.ParameterTextLength.Count,
          Length(TrimEnd(local.ProgramProcessingInfo.ParameterList)) - local
          .ParameterTextLength.Count + 1);
        local.ParameterCount.Count =
          (int)StringToNumber(local.TextCount.AddrZip4);
      }

      local.ProgramCheckpointRestart.ProgramName = global.UserId;
      UseReadPgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.ProgramCheckpointRestart.CheckpointCount = 0;

        if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
        {
          local.Restart.Timestamp =
            Timestamp(TrimEnd(local.ProgramCheckpointRestart.RestartInfo));
        }
        else
        {
          local.DateCount.TotalInteger =
            DateToInt(local.ProgramProcessingInfo.ProcessDate);
          local.TempHoldForDates.TextDate =
            NumberToString(local.DateCount.TotalInteger, 8);
          local.Restart.Timestamp =
            Timestamp(Substring(
              local.TempHoldForDates.TextDate, OblgWork.TextDate_MaxLength, 1,
            4) + "-" + Substring
            (local.TempHoldForDates.TextDate, OblgWork.TextDate_MaxLength, 5, 2) +
            "-"
            +
            Substring(local.TempHoldForDates.TextDate,
            OblgWork.TextDate_MaxLength, 7, 2));
        }

        local.Update.Count = 0;
        local.Total.Count = 0;
        local.UpdateLoop.Count = 0;
        local.ReportEabReportSend.ProcessDate = local.Current.Date;
        local.ReportEabReportSend.ProgramName = "SWELB570";
        local.ReportEabFileHandling.Action = "OPEN";
        UseCabControlReport2();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          local.ReportEabReportSend.ProcessDate = local.Current.Date;
          local.ReportEabReportSend.ProgramName = "SWELB570";
          local.ReportEabFileHandling.Action = "OPEN";
          UseCabErrorReport2();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          }
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
      else
      {
        // -------------------
        // continue processing
        // -------------------
      }
    }
    else
    {
      // -------------------
      // continue processing
      // -------------------
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    global.Command = "PROCESS";

    while(Equal(global.Command, "PROCESS"))
    {
      if (Equal(local.ProcessMethod.Command, "A"))
      {
        foreach(var item in ReadInfrastructure1())
        {
          ++local.Total.Count;
          local.Infrastructure.Assign(entities.Infrastructure);
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.LastUpdatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
          UseSpCabUpdateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.Update.Count;
            ++local.UpdateLoop.Count;
          }
          else
          {
            local.ErrorFound.Flag = "Y";
            ++local.Error.Count;
          }

          if (local.UpdateLoop.Count >= local
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() ||
            AsChar(local.ErrorFound.Flag) == 'Y')
          {
            // ---------------------------------------------------------
            // Record the number of checkpoints and the last checkpoint
            // time and set the restart indicator to yes.
            // Also return the checkpoint frequency counts in case they
            // been changed since the last read.
            // ---------------------------------------------------------
            local.ProgramCheckpointRestart.RestartInfo =
              NumberToString(entities.Infrastructure.SystemGeneratedIdentifier,
              250);
            local.ProgramCheckpointRestart.LastCheckpointTimestamp =
              local.Current.Timestamp;
            local.ProgramCheckpointRestart.RestartInd = "Y";
            UseUpdatePgmCheckpointRestart2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.UpdateLoop.Count = 0;
            }
            else
            {
              local.ErrorFound.Flag = "Y";
            }
          }

          if (AsChar(local.ErrorFound.Flag) == 'Y')
          {
            // *** Extract the Exit State Message for Display
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();

            // * * * * * * * * * * * *
            // WRITE Error Report
            // * * * * * * * * * * * *
            local.ReportEabReportSend.RptDetail = "Exit state is " + local
              .ExitStateWorkArea.Message;
            local.ReportEabReportSend.ProgramName = "SWELB570";
            local.ReportEabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
              ++local.Error.Count;
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              goto Test;
            }

            // * * * * * * * * * * * * *
            // WRITE Error Report spacing
            // * * * * * * * * * * * * *
            local.ReportEabReportSend.RptDetail = "";
            local.ReportEabReportSend.ProgramName = "SWELB570";
            local.ReportEabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              goto Test;
            }
          }

          if (local.Total.Count == local.ParameterCount.Count)
          {
            goto Test;
          }
          else
          {
            // *** Reinitialize Variables
            local.ErrorFound.Flag = "";
            local.ProgramErrorCode.RptDetail = "";
            local.ReportEabReportSend.RptDetail = "";
            local.ExitStateWorkArea.Message = "";
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }
      else
      {
        foreach(var item in ReadInfrastructure2())
        {
          ++local.Total.Count;
          local.Infrastructure.Assign(entities.Infrastructure);
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.LastUpdatedBy = local.ProgramProcessingInfo.Name;
          local.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
          UseSpCabUpdateInfrastructure();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ++local.Update.Count;
            ++local.UpdateLoop.Count;
          }
          else
          {
            local.ErrorFound.Flag = "Y";
            ++local.Error.Count;
          }

          if (local.UpdateLoop.Count >= local
            .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() ||
            AsChar(local.ErrorFound.Flag) == 'Y')
          {
            local.ProgramCheckpointRestart.RestartInfo =
              NumberToString(entities.Infrastructure.SystemGeneratedIdentifier,
              250);
            local.ProgramCheckpointRestart.LastCheckpointTimestamp =
              local.Current.Timestamp;
            local.ProgramCheckpointRestart.RestartInd = "Y";
            UseUpdatePgmCheckpointRestart2();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.UpdateLoop.Count = 0;
            }
            else
            {
              local.ErrorFound.Flag = "Y";
            }
          }

          if (AsChar(local.ErrorFound.Flag) == 'Y')
          {
            // *** Extract the Exit State Message for Display
            local.ExitStateWorkArea.Message = UseEabExtractExitStateMessage();
            local.ReportEabReportSend.RptDetail = "Exit state is " + local
              .ExitStateWorkArea.Message;
            local.ReportEabReportSend.ProgramName = "SWELB570";
            local.ReportEabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
              ++local.Error.Count;
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              goto Test;
            }

            // * * * * * * * * * * * * *
            // WRITE Error Report spacing
            // * * * * * * * * * * * * *
            local.ReportEabReportSend.RptDetail = "";
            local.ReportEabReportSend.ProgramName = "SWELB570";
            local.ReportEabFileHandling.Action = "WRITE";
            UseCabErrorReport1();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              goto Test;
            }
          }

          if (local.Total.Count == local.ParameterCount.Count)
          {
            goto Test;
          }
          else
          {
            local.ErrorFound.Flag = "";
            local.ProgramErrorCode.RptDetail = "";
            local.ReportEabReportSend.RptDetail = "";
            local.ExitStateWorkArea.Message = "";
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }
      }

Test:

      global.Command = "FINISHED";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      local.ReportEabReportSend.RptDetail =
        "Total Number of Records Read     = " + NumberToString
        (local.Total.Count, 15);
      local.ReportEabReportSend.ProgramName = "SWELB570";
      local.ReportEabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (Equal(local.ReportEabFileHandling.Status, "OK"))
      {
        local.ReportEabReportSend.RptDetail = "";
        local.ReportEabReportSend.ProgramName = "SWELB570";
        local.ReportEabFileHandling.Action = "WRITE";
        UseCabControlReport1();

        if (Equal(local.ReportEabFileHandling.Status, "OK"))
        {
          local.ReportEabReportSend.RptDetail =
            "Total Number of Records Updated  = " + NumberToString
            (local.Update.Count, 15);
          local.ReportEabReportSend.ProgramName = "SWELB570";
          local.ReportEabFileHandling.Action = "WRITE";
          UseCabControlReport1();

          if (Equal(local.ReportEabFileHandling.Status, "OK"))
          {
            local.ReportEabReportSend.RptDetail = "";
            local.ReportEabReportSend.ProgramName = "SWELB570";
            local.ReportEabFileHandling.Action = "WRITE";
            UseCabControlReport1();

            if (Equal(local.ReportEabFileHandling.Status, "OK"))
            {
              local.ReportEabReportSend.RptDetail =
                "Total Number of Records in Error = " + NumberToString
                (local.Error.Count, 15);
              local.ReportEabReportSend.ProgramName = "SWELB570";
              local.ReportEabFileHandling.Action = "WRITE";
              UseCabControlReport1();

              if (Equal(local.ReportEabFileHandling.Status, "OK"))
              {
                local.ReportEabReportSend.ProgramName = "SWELB570";
                local.ReportEabFileHandling.Action = "CLOSE";
                UseCabControlReport2();

                if (Equal(local.ReportEabFileHandling.Status, "OK"))
                {
                  local.ReportEabReportSend.ProgramName = "SWELB570";
                  local.ReportEabFileHandling.Action = "CLOSE";
                  UseCabErrorReport2();

                  if (Equal(local.ReportEabFileHandling.Status, "OK"))
                  {
                    if (local.Total.Count != local.ParameterCount.Count)
                    {
                      local.ProgramCheckpointRestart.RestartInfo = "";
                      local.ProgramCheckpointRestart.RestartInd = "N";
                    }
                    else
                    {
                      local.ProgramCheckpointRestart.RestartInd = "Y";
                      local.Restart.Date =
                        Date(entities.Infrastructure.CreatedTimestamp);
                      local.DateCount.TotalInteger =
                        DateToInt(local.Restart.Date);
                      local.TempHoldForDates.TextDate =
                        NumberToString(local.DateCount.TotalInteger, 8);
                      local.ProgramCheckpointRestart.RestartInfo =
                        Substring(local.TempHoldForDates.TextDate,
                        OblgWork.TextDate_MaxLength, 1, 4) + "-" + Substring
                        (local.TempHoldForDates.TextDate,
                        OblgWork.TextDate_MaxLength, 5, 2) + "-" + Substring
                        (local.TempHoldForDates.TextDate,
                        OblgWork.TextDate_MaxLength, 7, 2);
                    }

                    UseUpdatePgmCheckpointRestart1();
                  }
                  else
                  {
                    ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                  }
                }
                else
                {
                  ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
                }
              }
              else
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
              }
            }
            else
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
            }
          }
          else
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
          }
        }
        else
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
        }
      }
      else
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";
      }
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
    target.ReadFrequencyCount = source.ReadFrequencyCount;
    target.CheckpointCount = source.CheckpointCount;
    target.LastCheckpointTimestamp = source.LastCheckpointTimestamp;
    target.RestartInd = source.RestartInd;
    target.RestartInfo = source.RestartInfo;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.ReportEabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    MoveEabReportSend(local.ReportEabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.ReportEabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.ReportEabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private string UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    return useExport.ExitStateWorkArea.Message;
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

  private void UseSpCabUpdateInfrastructure()
  {
    var useImport = new SpCabUpdateInfrastructure.Import();
    var useExport = new SpCabUpdateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabUpdateInfrastructure.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart1()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);
  }

  private void UseUpdatePgmCheckpointRestart2()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadInfrastructure1()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure1",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Restart.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInfrastructure2()
  {
    entities.Infrastructure.Populated = false;

    return ReadEach("ReadInfrastructure2",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          local.Restart.Timestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.SituationNumber = db.GetInt32(reader, 1);
        entities.Infrastructure.ProcessStatus = db.GetString(reader, 2);
        entities.Infrastructure.EventId = db.GetInt32(reader, 3);
        entities.Infrastructure.EventType = db.GetString(reader, 4);
        entities.Infrastructure.EventDetailName = db.GetString(reader, 5);
        entities.Infrastructure.ReasonCode = db.GetString(reader, 6);
        entities.Infrastructure.BusinessObjectCd = db.GetString(reader, 7);
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 8);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 9);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 10);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.Infrastructure.InitiatingStateCode = db.GetString(reader, 12);
        entities.Infrastructure.CsenetInOutCode = db.GetString(reader, 13);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 14);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 15);
        entities.Infrastructure.CaseUnitNumber =
          db.GetNullableInt32(reader, 16);
        entities.Infrastructure.UserId = db.GetString(reader, 17);
        entities.Infrastructure.CreatedBy = db.GetString(reader, 18);
        entities.Infrastructure.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Infrastructure.LastUpdatedBy =
          db.GetNullableString(reader, 20);
        entities.Infrastructure.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 21);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 22);
        entities.Infrastructure.Function = db.GetNullableString(reader, 23);
        entities.Infrastructure.CaseUnitState =
          db.GetNullableString(reader, 24);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 25);
        entities.Infrastructure.Populated = true;

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
    /// A value of TempHoldForDates.
    /// </summary>
    [JsonPropertyName("tempHoldForDates")]
    public OblgWork TempHoldForDates
    {
      get => tempHoldForDates ??= new();
      set => tempHoldForDates = value;
    }

    /// <summary>
    /// A value of DateCount.
    /// </summary>
    [JsonPropertyName("dateCount")]
    public Common DateCount
    {
      get => dateCount ??= new();
      set => dateCount = value;
    }

    /// <summary>
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public DateWorkArea Restart
    {
      get => restart ??= new();
      set => restart = value;
    }

    /// <summary>
    /// A value of ProcessMethod.
    /// </summary>
    [JsonPropertyName("processMethod")]
    public Common ProcessMethod
    {
      get => processMethod ??= new();
      set => processMethod = value;
    }

    /// <summary>
    /// A value of ParameterTextLength.
    /// </summary>
    [JsonPropertyName("parameterTextLength")]
    public Common ParameterTextLength
    {
      get => parameterTextLength ??= new();
      set => parameterTextLength = value;
    }

    /// <summary>
    /// A value of ParameterStringLength.
    /// </summary>
    [JsonPropertyName("parameterStringLength")]
    public Common ParameterStringLength
    {
      get => parameterStringLength ??= new();
      set => parameterStringLength = value;
    }

    /// <summary>
    /// A value of TextCount.
    /// </summary>
    [JsonPropertyName("textCount")]
    public OblgWork TextCount
    {
      get => textCount ??= new();
      set => textCount = value;
    }

    /// <summary>
    /// A value of ParameterCount.
    /// </summary>
    [JsonPropertyName("parameterCount")]
    public Common ParameterCount
    {
      get => parameterCount ??= new();
      set => parameterCount = value;
    }

    /// <summary>
    /// A value of UpdateLoop.
    /// </summary>
    [JsonPropertyName("updateLoop")]
    public Common UpdateLoop
    {
      get => updateLoop ??= new();
      set => updateLoop = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of ZdelCheckpointRestartKey.
    /// </summary>
    [JsonPropertyName("zdelCheckpointRestartKey")]
    public Infrastructure ZdelCheckpointRestartKey
    {
      get => zdelCheckpointRestartKey ??= new();
      set => zdelCheckpointRestartKey = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of ErrorFound.
    /// </summary>
    [JsonPropertyName("errorFound")]
    public Common ErrorFound
    {
      get => errorFound ??= new();
      set => errorFound = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    public OblgWork ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of Total.
    /// </summary>
    [JsonPropertyName("total")]
    public Common Total
    {
      get => total ??= new();
      set => total = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public Common Error
    {
      get => error ??= new();
      set => error = value;
    }

    /// <summary>
    /// A value of Written.
    /// </summary>
    [JsonPropertyName("written")]
    public Common Written
    {
      get => written ??= new();
      set => written = value;
    }

    /// <summary>
    /// A value of ReportEabReportSend.
    /// </summary>
    [JsonPropertyName("reportEabReportSend")]
    public EabReportSend ReportEabReportSend
    {
      get => reportEabReportSend ??= new();
      set => reportEabReportSend = value;
    }

    /// <summary>
    /// A value of ReportEabFileHandling.
    /// </summary>
    [JsonPropertyName("reportEabFileHandling")]
    public EabFileHandling ReportEabFileHandling
    {
      get => reportEabFileHandling ??= new();
      set => reportEabFileHandling = value;
    }

    /// <summary>
    /// A value of SdsoRecordCreated.
    /// </summary>
    [JsonPropertyName("sdsoRecordCreated")]
    public Common SdsoRecordCreated
    {
      get => sdsoRecordCreated ??= new();
      set => sdsoRecordCreated = value;
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
    /// A value of ProgramErrorCode.
    /// </summary>
    [JsonPropertyName("programErrorCode")]
    public EabReportSend ProgramErrorCode
    {
      get => programErrorCode ??= new();
      set => programErrorCode = value;
    }

    private OblgWork tempHoldForDates;
    private Common dateCount;
    private DateWorkArea restart;
    private Common processMethod;
    private Common parameterTextLength;
    private Common parameterStringLength;
    private OblgWork textCount;
    private Common parameterCount;
    private Common updateLoop;
    private Infrastructure infrastructure;
    private Infrastructure zdelCheckpointRestartKey;
    private DateWorkArea current;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Common errorFound;
    private OblgWork processDate;
    private Common update;
    private Common total;
    private Common error;
    private Common written;
    private EabReportSend reportEabReportSend;
    private EabFileHandling reportEabFileHandling;
    private Common sdsoRecordCreated;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend programErrorCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }
#endregion
}

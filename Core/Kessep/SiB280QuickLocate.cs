// Program: SI_B280_QUICK_LOCATE, ID: 372424713, model: 746.
// Short name: SWEI280B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B280_QUICK_LOCATE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB280QuickLocate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B280_QUICK_LOCATE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB280QuickLocate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB280QuickLocate.
  /// </summary>
  public SiB280QuickLocate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Description
    // 12/30/1998	C Ott		Initial Dev.
    // 5/17/1999	C Ott		Close Adabas files.
    // 9/20/1999	C Ott		Problem # 74064.
    // Remove ABEND exit state when DHR input data set is empty or not found.
    // 11/01/1999	C Ott		Problem # 79027.
    // In order to solve problem of duplicate records been written to the ouput 
    // file that is sent to DHR after a rollback error is encountered,
    // redesigned batch for a commit after each successful record.
    // 01/07/2000	C Ott		Problem # 84205.
    // Modified for subsequent requests from the same state for the same SSN.
    // 3/30/2001	E Lyman		PR # 115484
    // Fix Alias EAB and close Adabas files properly.
    // 07/17/2001	M Ramirez	PR# xxxxxx
    // Rework to remove writing to DHR file, which is now done in SWEIB281.  
    // This allows us to remove restart logic
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = "SWEIB280";
    UseReadProgramProcessingInfo();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ***************************************************
    // *Open the ERROR RPT.  DDNAME=RPT99.
    // ***************************************************
    local.EabFileHandling.Action = "OPEN";
    local.Open.ProgramName = local.ProgramProcessingInfo.Name;
    local.Open.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // ***************************************************
    // *Open the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered opening the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ***************************************************
    // *Get Processing Parameters
    // ***************************************************
    if (!IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      if (Find(local.ProgramProcessingInfo.ParameterList, "DEBUG") > 0)
      {
        local.Debug.Flag = "Y";
      }
      else
      {
        local.Debug.Flag = "N";
      }
    }
    else
    {
      local.Debug.Flag = "N";
    }

    // ***************************************************
    // *Get Checkpoint restart information
    // ***************************************************
    local.ProgramCheckpointRestart.ProgramName =
      local.ProgramProcessingInfo.Name;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered reading the Checkpoint Restart Information.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    local.Local30DaysAgo.Timestamp = Now().AddDays(-30);
    local.EabFileHandling.Action = "WRITE";

    foreach(var item in ReadCsenetTransactionEnvelopInterstateCase())
    {
      local.InterstateCase.Assign(entities.InterstateCase);

      if (AsChar(local.Debug.Flag) == 'Y')
      {
        local.Write.RptDetail = "DEBUG:  Read Interstate Case; Serial # = " + NumberToString
          (local.InterstateCase.TransSerialNumber, 4, 12) + ", Date = " + NumberToString
          (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      ++local.LcontrolTotalReads.Count;

      switch(AsChar(entities.CsenetTransactionEnvelop.ProcessingStatusCode))
      {
        case 'C':
          break;
        case 'D':
          // **************************************************************
          // Processing Status Code is set to "D" when Quick Locate
          // Request is sent to DHR for search.  If these requests were
          // made longer than one month in the past, send a response without
          // DHR's input.
          // ****************************************************************
          if (!Lt(entities.CsenetTransactionEnvelop.LastUpdatedTimestamp,
            local.Local30DaysAgo.Timestamp))
          {
            if (AsChar(local.Debug.Flag) == 'Y')
            {
              local.Write.RptDetail = "DEBUG:  DHR txn is not 30 days old yet";
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }

            continue;
          }

          break;
        default:
          if (AsChar(local.Debug.Flag) == 'Y')
          {
            local.Write.RptDetail = "DEBUG:  Processing status is not C nor D";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          continue;
      }

      UseSiProcessQuickLocateRequests();

      if (AsChar(local.Debug.Flag) == 'Y' && local.New1.TransSerialNumber > 0)
      {
        local.Write.RptDetail = "DEBUG:  New transaction created; Serial # " + NumberToString
          (local.New1.TransSerialNumber, 4, 12) + ", " + NumberToString
          (DateToInt(local.New1.TransactionDate), 8, 8);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (IsExitState("SI0000_LO1R_REQUIRES_1_MNTH_WAIT"))
        {
          ReadFips();
          local.Write.RptDetail =
            "Quick Locate Request was not processed.  Duplicate request within 30 days from " +
            entities.Fips.StateAbbreviation + " for SSN " + (
              local.InterstateApIdentification.Ssn ?? "");
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.CsenetTransactionEnvelop.ProcessingStatusCode = "P";
          ExitState = "ACO_NN0000_ALL_OK";
        }
        else
        {
          UseEabExtractExitStateMessage();
          local.Write.RptDetail = TrimEnd(local.ExitStateWorkArea.Message) + NumberToString
            (local.InterstateCase.TransSerialNumber, 4, 12) + ", " + NumberToString
            (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
      else
      {
        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.Write.RptDetail = "DEBUG:  Processed successfully";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        if (AsChar(local.SendToDhr.Flag) == 'Y')
        {
          local.CsenetTransactionEnvelop.ProcessingStatusCode = "D";
          ++local.LcontrolTotalForDhrLocate.Count;

          if (AsChar(local.Debug.Flag) == 'Y')
          {
            local.Write.RptDetail = "DEBUG:  Record written to DHR file";
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }
          }

          local.SendToDhr.Flag = "";
        }
        else
        {
          local.CsenetTransactionEnvelop.ProcessingStatusCode = "P";
        }
      }

      // -------------------------------------------------
      // Mark record as processed
      // -------------------------------------------------
      if (ReadCsenetTransactionEnvelop())
      {
        try
        {
          UpdateCsenetTransactionEnvelop();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.AbendError.Flag = "Y";

              break;
            case ErrorCode.PermittedValueViolation:
              local.AbendError.Flag = "Y";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        local.AbendError.Flag = "Y";
      }

      if (AsChar(local.AbendError.Flag) == 'Y')
      {
        local.Write.RptDetail =
          "Error updating CSENet Transaction Envelope for IS Case Serial # " + NumberToString
          (local.InterstateCase.TransSerialNumber, 4, 12) + ", " + NumberToString
          (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
        UseCabErrorReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      ++local.CheckpointReads.Count;

      if (local.CheckpointReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CheckpointUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();

        if (AsChar(local.Debug.Flag) == 'Y')
        {
          local.Write.RptDetail = "DEBUG:  Commit performed";
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.CheckpointUpdates.Count = 0;
        local.CheckpointReads.Count = 0;
      }
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseExtToDoACommit();
    }

    // **************************************************************
    // 3/30/01    E. Lyman   Replaced code with a CAB.
    // *************************************************************
    UseSiCloseAdabas();

    // ***************************************************
    // *Write to the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    local.EabFileHandling.Action = "WRITE";
    local.Write.RptDetail =
      "Number of Incoming Quick Locate Requests read      " + NumberToString
      (local.LcontrolTotalReads.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered writing to the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Write.RptDetail =
      "CSENet Interstate Case data blocks created         " + NumberToString
      (local.LcontrolTotCreateIsCases.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered writing to the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    local.Write.RptDetail =
      "Records written to DHR Interface file for Locate   " + NumberToString
      (local.LcontrolTotalForDhrLocate.Count, 15);
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail =
        "Error encountered writing to the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Close the CONTROL RPT. DDNAME=RPT98.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Control Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    // ***************************************************
    // *Close the ERROR RPT. DDNAME=RPT99.
    // ***************************************************
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***************************************************
      // *Write a line to the ERROR RPT.
      // ***************************************************
      local.EabFileHandling.Action = "WRITE";
      local.Write.RptDetail = "Error encountered closing the Error Report.";
      UseCabErrorReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
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

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.Open, useImport.NeededToOpen);

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.Write.RptDetail;

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

    Call(ExtToDoACommit.Execute, useImport, useExport);
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

  private void UseSiCloseAdabas()
  {
    var useImport = new SiCloseAdabas.Import();
    var useExport = new SiCloseAdabas.Export();

    Call(SiCloseAdabas.Execute, useImport, useExport);
  }

  private void UseSiProcessQuickLocateRequests()
  {
    var useImport = new SiProcessQuickLocateRequests.Import();
    var useExport = new SiProcessQuickLocateRequests.Export();

    useImport.InterstateCase.Assign(entities.InterstateCase);
    useImport.ExpRecordsForDhrLocates.Count =
      local.LcontrolTotalForDhrLocate.Count;
    useImport.ExpInterstateCaseCreates.Count =
      local.LcontrolTotCreateIsCases.Count;
    useImport.ExpCheckpointUpdates.Count = local.CheckpointUpdates.Count;
    useImport.CsenetTransactionEnvelop.ProcessingStatusCode =
      entities.CsenetTransactionEnvelop.ProcessingStatusCode;
    useImport.BatchProcess.Date = local.Current.Date;

    Call(SiProcessQuickLocateRequests.Execute, useImport, useExport);

    local.LcontrolTotalForDhrLocate.Count =
      useImport.ExpRecordsForDhrLocates.Count;
    local.LcontrolTotCreateIsCases.Count =
      useImport.ExpInterstateCaseCreates.Count;
    local.CheckpointUpdates.Count = useImport.ExpCheckpointUpdates.Count;
    local.InterstateApIdentification.Ssn =
      useExport.InterstateApIdentification.Ssn;
    local.SendToDhr.Flag = useExport.SendToDhr.Flag;
    MoveInterstateCase(useExport.New1, local.New1);
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", local.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          local.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsenetTransactionEnvelopInterstateCase()
  {
    entities.InterstateCase.Populated = false;
    entities.CsenetTransactionEnvelop.Populated = false;

    return ReadEach("ReadCsenetTransactionEnvelopInterstateCase",
      null,
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.LastUpdatedBy =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 4);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 5);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 6);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 7);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 8);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 9);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 11);
        entities.InterstateCase.ActionCode = db.GetString(reader, 12);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 13);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 14);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 15);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 16);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 17);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 18);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 19);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 20);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 23);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 25);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 26);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 27);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 29);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 30);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 31);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 32);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 33);
        entities.InterstateCase.CaseType = db.GetString(reader, 34);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 35);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 36);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 37);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 41);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 46);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 48);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 52);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 53);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 54);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 56);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 57);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 58);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 59);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 60);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 61);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 63);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 65);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 66);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 67);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 68);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 69);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 70);
        entities.InterstateCase.Populated = true;
        entities.CsenetTransactionEnvelop.Populated = true;

        return true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(command, "state", local.InterstateCase.OtherFipsState);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var processingStatusCode =
      local.CsenetTransactionEnvelop.ProcessingStatusCode;

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
        db.SetString(command, "processingStatus", processingStatusCode);
        db.SetDate(
          command, "ccaTransactionDt",
          entities.CsenetTransactionEnvelop.CcaTransactionDt.
            GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum",
          entities.CsenetTransactionEnvelop.CcaTransSerNum);
      });

    entities.CsenetTransactionEnvelop.LastUpdatedBy = lastUpdatedBy;
    entities.CsenetTransactionEnvelop.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.CsenetTransactionEnvelop.ProcessingStatusCode =
      processingStatusCode;
    entities.CsenetTransactionEnvelop.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public InterstateCase New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Debug.
    /// </summary>
    [JsonPropertyName("debug")]
    public Common Debug
    {
      get => debug ??= new();
      set => debug = value;
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
    /// A value of Local30DaysAgo.
    /// </summary>
    [JsonPropertyName("local30DaysAgo")]
    public DateWorkArea Local30DaysAgo
    {
      get => local30DaysAgo ??= new();
      set => local30DaysAgo = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of SendToDhr.
    /// </summary>
    [JsonPropertyName("sendToDhr")]
    public Common SendToDhr
    {
      get => sendToDhr ??= new();
      set => sendToDhr = value;
    }

    /// <summary>
    /// A value of AbendError.
    /// </summary>
    [JsonPropertyName("abendError")]
    public Common AbendError
    {
      get => abendError ??= new();
      set => abendError = value;
    }

    /// <summary>
    /// A value of LcontrolTotalForDhrLocate.
    /// </summary>
    [JsonPropertyName("lcontrolTotalForDhrLocate")]
    public Common LcontrolTotalForDhrLocate
    {
      get => lcontrolTotalForDhrLocate ??= new();
      set => lcontrolTotalForDhrLocate = value;
    }

    /// <summary>
    /// A value of LcontrolTotCreateIsCases.
    /// </summary>
    [JsonPropertyName("lcontrolTotCreateIsCases")]
    public Common LcontrolTotCreateIsCases
    {
      get => lcontrolTotCreateIsCases ??= new();
      set => lcontrolTotCreateIsCases = value;
    }

    /// <summary>
    /// A value of LcontrolTotalReads.
    /// </summary>
    [JsonPropertyName("lcontrolTotalReads")]
    public Common LcontrolTotalReads
    {
      get => lcontrolTotalReads ??= new();
      set => lcontrolTotalReads = value;
    }

    /// <summary>
    /// A value of CheckpointUpdates.
    /// </summary>
    [JsonPropertyName("checkpointUpdates")]
    public Common CheckpointUpdates
    {
      get => checkpointUpdates ??= new();
      set => checkpointUpdates = value;
    }

    /// <summary>
    /// A value of CheckpointReads.
    /// </summary>
    [JsonPropertyName("checkpointReads")]
    public Common CheckpointReads
    {
      get => checkpointReads ??= new();
      set => checkpointReads = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Open.
    /// </summary>
    [JsonPropertyName("open")]
    public EabReportSend Open
    {
      get => open ??= new();
      set => open = value;
    }

    /// <summary>
    /// A value of Write.
    /// </summary>
    [JsonPropertyName("write")]
    public EabReportSend Write
    {
      get => write ??= new();
      set => write = value;
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

    private InterstateCase new1;
    private ExitStateWorkArea exitStateWorkArea;
    private Common debug;
    private DateWorkArea current;
    private DateWorkArea local30DaysAgo;
    private InterstateApIdentification interstateApIdentification;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private Common sendToDhr;
    private Common abendError;
    private Common lcontrolTotalForDhrLocate;
    private Common lcontrolTotCreateIsCases;
    private Common lcontrolTotalReads;
    private Common checkpointUpdates;
    private Common checkpointReads;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend open;
    private EabReportSend write;
    private ProgramCheckpointRestart programCheckpointRestart;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    private Fips fips;
    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
  }
#endregion
}

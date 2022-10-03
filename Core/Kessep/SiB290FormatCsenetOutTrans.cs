// Program: SI_B290_FORMAT_CSENET_OUT_TRANS, ID: 372618069, model: 746.
// Short name: SWEI290B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B290_FORMAT_CSENET_OUT_TRANS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SiB290FormatCsenetOutTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B290_FORMAT_CSENET_OUT_TRANS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB290FormatCsenetOutTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB290FormatCsenetOutTrans.
  /// </summary>
  public SiB290FormatCsenetOutTrans(IContext context, Import import,
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
    // ------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // 02/12/1999	Carl Ott
    // Initial Dev.
    // 04/20/1999	M Ramirez
    // Made changes to PrAD to incorporate the use of new EAB.
    // 05/19/1999	M Ramirez
    // Added local_send_date_time interstate_case for change to Version 3.1
    // 07/24/1999	M Ramirez
    // Rework to use housekeeping and closing cabs
    // 04/10/2001	M Ramirez	114580 Seg C
    // Changed to only process 'S' status transactions
    // ------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiB290Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      local.External.FileInstruction = "OPENX";
    }
    else
    {
      local.External.FileInstruction = "OPENO";
    }

    // mjr---> Removed view matching (except local external)
    UseSiEabFormatCsenetOutTrans2();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered opening the CSENet Outgoing File.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // mjr
    // ----------------------------------------------------
    // 05/19/1999
    // Added following 2 statements for change to Version 3.1
    // Note:  I am using CURRENT_DATE (instead of PROCESS_DATE) because
    // we are also showing the actual time.
    // -----------------------------------------------------------------
    local.SendDateTime.SentDate = Now().Date;
    local.SendDateTime.SentTime = Time(Now());

    // mjr
    // -----------------------------------------------
    // 04/10/2001
    // PR# 114580 Segment C
    // Changed to only process 'S' status transactions.
    // Following READ EACH statement was changed
    // -----------------------------------------------------------
    foreach(var item in ReadCsenetTransactionEnvelopInterstateCase())
    {
      local.TerminatingError.Flag = "N";
      local.Restart.TransSerialNumber =
        entities.InterstateCase.TransSerialNumber;
      local.InterstateCase.Assign(entities.InterstateCase);
      local.WriteError.Flag = "";
      ++local.CheckpointReads.Count;
      ++local.ControlTotalReads.Count;
      UseSiFormatCsenetOutTrans();

      // mjr
      // ----------------------------------------------------
      // 05/19/1999
      // Added following 2 statements for change to Version 3.1
      // -----------------------------------------------------------------
      local.InterstateCase.SentDate = local.SendDateTime.SentDate;
      local.InterstateCase.SentTime =
        local.SendDateTime.SentTime.GetValueOrDefault();

      if (AsChar(local.WriteError.Flag) != 'Y')
      {
        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.WriteError.Flag = "N";
          local.External.FileInstruction = "WRITE";

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            local.InterstateCase.OtherFipsState = 92;
            local.InterstateCase.OtherFipsCounty = 0;
            local.InterstateCase.OtherFipsLocation = 0;
          }

          UseSiEabFormatCsenetOutTrans1();

          if (!IsEmpty(local.External.TextReturnCode))
          {
            local.EabFileHandling.Action = "WRITE";
            local.NeededToWrite.RptDetail =
              "Error encountered writing to the CSENet Outgoing File.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          ++local.ControlTransSent.Count;
          local.CsenetTransactionEnvelop.ProcessingStatusCode = "P";

          // ****************************************************************
          // Mark record as processed
          // ****************************************************************
          try
          {
            UpdateCsenetTransactionEnvelop1();

            try
            {
              UpdateInterstateCase();
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  local.TerminatingError.Flag = "Y";

                  break;
                case ErrorCode.PermittedValueViolation:
                  local.TerminatingError.Flag = "Y";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.TerminatingError.Flag = "Y";

                break;
              case ErrorCode.PermittedValueViolation:
                local.TerminatingError.Flag = "Y";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else if (IsExitState("CSENET_CASE_NF"))
        {
          local.TerminatingError.Flag = "Y";
        }
        else
        {
        }

        if (AsChar(local.TerminatingError.Flag) == 'Y')
        {
          local.WriteError.Flag = "Y";
          local.NeededToWrite.RptDetail =
            "Fatal database error terminated program.  Will restart at # " + NumberToString
            (local.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
            (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
        }
      }

      if (AsChar(local.WriteError.Flag) == 'Y')
      {
        ++local.ControlErrorRecords.Count;

        if (AsChar(local.TerminatingError.Flag) == 'Y')
        {
        }
        else
        {
          // ****************************************************************
          // The following READ statements are to collect Service Provider 
          // information for the error report.  If unsuccessful, it should not
          // cause processing of other outgoing CSENet transactions to stop.
          // ****************************************************************
          if (ReadServiceProvider())
          {
            if (ReadOfficeServiceProvider())
            {
              ReadOffice();
            }
          }

          local.NeededToWrite.RptDetail =
            TrimEnd(local.NeededToWrite.RptDetail) + "  " + Substring
            (entities.InterstateCase.KsCaseId,
            InterstateCase.KsCaseId_MaxLength, 1, 10) + "  " + NumberToString
            (entities.Office.SystemGeneratedId, 12, 4) + "    " + TrimEnd
            (entities.ServiceProvider.LastName) + ", " + entities
            .ServiceProvider.FirstName;
        }

        // ****************************************************************
        // Write to Error Report
        // ****************************************************************
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // mjr
        // -----------------------------------------------
        // 04/10/2001
        // PR# 114580 Segment C
        // Changed to only process 'S' status transactions.
        // Added update to 'E' status in the event of an error
        // -----------------------------------------------------------
        try
        {
          UpdateCsenetTransactionEnvelop2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              local.TerminatingError.Flag = "Y";
              local.NeededToWrite.RptDetail =
                "Fatal database error terminated program.  Will restart at # " +
                NumberToString
                (local.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
                (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              }

              break;
            case ErrorCode.PermittedValueViolation:
              local.TerminatingError.Flag = "Y";
              local.NeededToWrite.RptDetail =
                "Fatal database error terminated program.  Will restart at # " +
                NumberToString
                (local.InterstateCase.TransSerialNumber, 4, 12) + "   " + NumberToString
                (DateToInt(local.InterstateCase.TransactionDate), 8, 8);
              local.EabFileHandling.Action = "WRITE";
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                break;
              }

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (local.CheckpointReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .CheckpointUpdates.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault() || AsChar
        (local.TerminatingError.Flag) == 'Y')
      {
        local.RestartFileRec.Count += local.CsenetOutTransWritten.Count;
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          NumberToString(entities.InterstateCase.TransSerialNumber, 15) + NumberToString
          (local.RestartFileRec.Count, 15);
        local.Restart.TransSerialNumber =
          entities.InterstateCase.TransSerialNumber;
        ExitState = "ACO_NN0000_ALL_OK";
        UseUpdatePgmCheckpointRestart();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // ok, continue processing
        }
        else
        {
          // ***************************************************
          // *Write a line to the ERROR RPT.
          // ***************************************************
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered updating the Program Checkpoint Restart information.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        UseExtToDoACommit();

        if (local.ExtractPersonInfo.NumericReturnCode != 0)
        {
          local.EabFileHandling.Action = "WRITE";
          local.NeededToWrite.RptDetail =
            "Error encountered performing a Commit.";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            break;
          }
        }

        local.CheckpointUpdates.Count = 0;
        local.CheckpointReads.Count = 0;
        local.RestartFileRec.Count = 0;

        if (AsChar(local.TerminatingError.Flag) == 'Y')
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }
      }
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ProgramCheckpointRestart.RestartInd = "N";
      local.ProgramCheckpointRestart.CheckpointCount = 0;
      local.ProgramCheckpointRestart.RestartInfo = "";
      UseUpdatePgmCheckpointRestart();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // ok, continue processing
      }
      else
      {
        // ***************************************************
        // *Write a line to the ERROR RPT.
        // ***************************************************
        local.EabFileHandling.Action = "WRITE";
        local.NeededToWrite.RptDetail =
          "Error encountered updating the Program Checkpoint Restart information.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
        }
      }
    }

    local.External.FileInstruction = "CLOSEO";

    // mjr---> Removed view matching (except local external)
    UseSiEabFormatCsenetOutTrans2();

    if (!IsEmpty(local.External.TextReturnCode))
    {
      local.EabFileHandling.Action = "WRITE";
      local.NeededToWrite.RptDetail =
        "Error encountered closing the CSENet Outgoing File.";
      UseCabErrorReport();
    }

    UseSiB290WriteControlsAndClose();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveCollection1(Local.CollectionGroup source,
    SiEabFormatCsenetOutTrans.Import.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveCollection2(SiFormatCsenetOutTrans.Export.
    CollectionGroup source, Local.CollectionGroup target)
  {
    target.InterstateCollection.Assign(source.InterstateCollection);
  }

  private static void MoveExternal(External source, External target)
  {
    target.NumericReturnCode = source.NumericReturnCode;
    target.TextReturnCode = source.TextReturnCode;
  }

  private static void MoveOrder1(Local.OrderGroup source,
    SiEabFormatCsenetOutTrans.Import.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveOrder2(SiFormatCsenetOutTrans.Export.
    OrderGroup source, Local.OrderGroup target)
  {
    target.InterstateSupportOrder.Assign(source.InterstateSupportOrder);
  }

  private static void MoveParticipant1(Local.ParticipantGroup source,
    SiEabFormatCsenetOutTrans.Import.ParticipantGroup target)
  {
    target.InterstateParticipant.Assign(source.Croup);
  }

  private static void MoveParticipant2(SiFormatCsenetOutTrans.Export.
    ParticipantGroup source, Local.ParticipantGroup target)
  {
    target.Croup.Assign(source.InterstateParticipant);
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
    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSiB290Housekeeping()
  {
    var useImport = new SiB290Housekeeping.Import();
    var useExport = new SiB290Housekeeping.Export();

    Call(SiB290Housekeeping.Execute, useImport, useExport);

    local.Current.Timestamp = useExport.Current.Timestamp;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.RestartFileRec.Count = useExport.RestartFileRecord.Count;
    local.Restart.TransSerialNumber = useExport.Restart.TransSerialNumber;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseSiB290WriteControlsAndClose()
  {
    var useImport = new SiB290WriteControlsAndClose.Import();
    var useExport = new SiB290WriteControlsAndClose.Export();

    useImport.Created.Count = local.ControlTransSent.Count;
    useImport.Erred.Count = local.ControlErrorRecords.Count;
    useImport.Read.Count = local.ControlTotalReads.Count;

    Call(SiB290WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSiEabFormatCsenetOutTrans1()
  {
    var useImport = new SiEabFormatCsenetOutTrans.Import();
    var useExport = new SiEabFormatCsenetOutTrans.Export();

    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);
    local.Collection.CopyTo(useImport.Collection, MoveCollection1);
    local.Order.CopyTo(useImport.Order, MoveOrder1);
    local.Participant.CopyTo(useImport.Participant, MoveParticipant1);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);
    useImport.InterstateCase.Assign(local.InterstateCase);
    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SiEabFormatCsenetOutTrans.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseSiEabFormatCsenetOutTrans2()
  {
    var useImport = new SiEabFormatCsenetOutTrans.Import();
    var useExport = new SiEabFormatCsenetOutTrans.Export();

    useImport.External.FileInstruction = local.External.FileInstruction;
    MoveExternal(local.External, useExport.External);

    Call(SiEabFormatCsenetOutTrans.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.External);
  }

  private void UseSiFormatCsenetOutTrans()
  {
    var useImport = new SiFormatCsenetOutTrans.Import();
    var useExport = new SiFormatCsenetOutTrans.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiFormatCsenetOutTrans.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
    local.NeededToWrite.RptDetail = useExport.NeededToWrite.RptDetail;
    local.WriteError.Flag = useExport.WriteError.Flag;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
    useExport.Participant.CopyTo(local.Participant, MoveParticipant2);
    useExport.Order.CopyTo(local.Order, MoveOrder2);
    useExport.Collection.CopyTo(local.Collection, MoveCollection2);
    local.InterstateMiscellaneous.Assign(useExport.InterstateMiscellaneous);
  }

  private void UseUpdatePgmCheckpointRestart()
  {
    var useImport = new UpdatePgmCheckpointRestart.Import();
    var useExport = new UpdatePgmCheckpointRestart.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdatePgmCheckpointRestart.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private IEnumerable<bool> ReadCsenetTransactionEnvelopInterstateCase()
  {
    entities.InterstateCase.Populated = false;
    entities.CsenetTransactionEnvelop.Populated = false;

    return ReadEach("ReadCsenetTransactionEnvelopInterstateCase",
      (db, command) =>
      {
        db.SetInt64(command, "transSerialNbr", local.Restart.TransSerialNumber);
      },
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
        entities.CsenetTransactionEnvelop.CreatedBy = db.GetString(reader, 6);
        entities.CsenetTransactionEnvelop.CreatedTstamp =
          db.GetDateTime(reader, 7);
        entities.InterstateCase.LocalFipsState = db.GetInt32(reader, 8);
        entities.InterstateCase.LocalFipsCounty =
          db.GetNullableInt32(reader, 9);
        entities.InterstateCase.LocalFipsLocation =
          db.GetNullableInt32(reader, 10);
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 11);
        entities.InterstateCase.OtherFipsCounty =
          db.GetNullableInt32(reader, 12);
        entities.InterstateCase.OtherFipsLocation =
          db.GetNullableInt32(reader, 13);
        entities.InterstateCase.ActionCode = db.GetString(reader, 14);
        entities.InterstateCase.FunctionalTypeCode = db.GetString(reader, 15);
        entities.InterstateCase.KsCaseId = db.GetNullableString(reader, 16);
        entities.InterstateCase.InterstateCaseId =
          db.GetNullableString(reader, 17);
        entities.InterstateCase.ActionReasonCode =
          db.GetNullableString(reader, 18);
        entities.InterstateCase.ActionResolutionDate =
          db.GetNullableDate(reader, 19);
        entities.InterstateCase.AttachmentsInd = db.GetString(reader, 20);
        entities.InterstateCase.CaseDataInd = db.GetNullableInt32(reader, 21);
        entities.InterstateCase.ApIdentificationInd =
          db.GetNullableInt32(reader, 22);
        entities.InterstateCase.ApLocateDataInd =
          db.GetNullableInt32(reader, 23);
        entities.InterstateCase.ParticipantDataInd =
          db.GetNullableInt32(reader, 24);
        entities.InterstateCase.OrderDataInd = db.GetNullableInt32(reader, 25);
        entities.InterstateCase.CollectionDataInd =
          db.GetNullableInt32(reader, 26);
        entities.InterstateCase.InformationInd =
          db.GetNullableInt32(reader, 27);
        entities.InterstateCase.SentDate = db.GetNullableDate(reader, 28);
        entities.InterstateCase.SentTime = db.GetNullableTimeSpan(reader, 29);
        entities.InterstateCase.DueDate = db.GetNullableDate(reader, 30);
        entities.InterstateCase.OverdueInd = db.GetNullableInt32(reader, 31);
        entities.InterstateCase.DateReceived = db.GetNullableDate(reader, 32);
        entities.InterstateCase.TimeReceived =
          db.GetNullableTimeSpan(reader, 33);
        entities.InterstateCase.AttachmentsDueDate =
          db.GetNullableDate(reader, 34);
        entities.InterstateCase.InterstateFormsPrinted =
          db.GetNullableString(reader, 35);
        entities.InterstateCase.CaseType = db.GetString(reader, 36);
        entities.InterstateCase.CaseStatus = db.GetString(reader, 37);
        entities.InterstateCase.PaymentMailingAddressLine1 =
          db.GetNullableString(reader, 38);
        entities.InterstateCase.PaymentAddressLine2 =
          db.GetNullableString(reader, 39);
        entities.InterstateCase.PaymentCity = db.GetNullableString(reader, 40);
        entities.InterstateCase.PaymentState = db.GetNullableString(reader, 41);
        entities.InterstateCase.PaymentZipCode5 =
          db.GetNullableString(reader, 42);
        entities.InterstateCase.PaymentZipCode4 =
          db.GetNullableString(reader, 43);
        entities.InterstateCase.ContactNameLast =
          db.GetNullableString(reader, 44);
        entities.InterstateCase.ContactNameFirst =
          db.GetNullableString(reader, 45);
        entities.InterstateCase.ContactNameMiddle =
          db.GetNullableString(reader, 46);
        entities.InterstateCase.ContactNameSuffix =
          db.GetNullableString(reader, 47);
        entities.InterstateCase.ContactAddressLine1 = db.GetString(reader, 48);
        entities.InterstateCase.ContactAddressLine2 =
          db.GetNullableString(reader, 49);
        entities.InterstateCase.ContactCity = db.GetNullableString(reader, 50);
        entities.InterstateCase.ContactState = db.GetNullableString(reader, 51);
        entities.InterstateCase.ContactZipCode5 =
          db.GetNullableString(reader, 52);
        entities.InterstateCase.ContactZipCode4 =
          db.GetNullableString(reader, 53);
        entities.InterstateCase.ContactPhoneNum =
          db.GetNullableInt32(reader, 54);
        entities.InterstateCase.AssnDeactDt = db.GetNullableDate(reader, 55);
        entities.InterstateCase.AssnDeactInd = db.GetNullableString(reader, 56);
        entities.InterstateCase.LastDeferDt = db.GetNullableDate(reader, 57);
        entities.InterstateCase.Memo = db.GetNullableString(reader, 58);
        entities.InterstateCase.ContactPhoneExtension =
          db.GetNullableString(reader, 59);
        entities.InterstateCase.ContactFaxNumber =
          db.GetNullableInt32(reader, 60);
        entities.InterstateCase.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 61);
        entities.InterstateCase.ContactInternetAddress =
          db.GetNullableString(reader, 62);
        entities.InterstateCase.InitiatingDocketNumber =
          db.GetNullableString(reader, 63);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 64);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 65);
        entities.InterstateCase.NondisclosureFinding =
          db.GetNullableString(reader, 66);
        entities.InterstateCase.RespondingDocketNumber =
          db.GetNullableString(reader, 67);
        entities.InterstateCase.StateWithCej = db.GetNullableString(reader, 68);
        entities.InterstateCase.PaymentFipsCounty =
          db.GetNullableString(reader, 69);
        entities.InterstateCase.PaymentFipsState =
          db.GetNullableString(reader, 70);
        entities.InterstateCase.PaymentFipsLocation =
          db.GetNullableString(reader, 71);
        entities.InterstateCase.ContactAreaCode =
          db.GetNullableInt32(reader, 72);
        entities.InterstateCase.Populated = true;
        entities.CsenetTransactionEnvelop.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId", entities.OfficeServiceProvider.OffGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId", entities.CsenetTransactionEnvelop.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.LastName = db.GetString(reader, 2);
        entities.ServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ServiceProvider.MiddleInitial = db.GetString(reader, 4);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateCsenetTransactionEnvelop1()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var processingStatusCode =
      local.CsenetTransactionEnvelop.ProcessingStatusCode;

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop1",
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

  private void UpdateCsenetTransactionEnvelop2()
  {
    System.Diagnostics.Debug.
      Assert(entities.CsenetTransactionEnvelop.Populated);

    var lastUpdatedBy = local.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var processingStatusCode = "E";

    entities.CsenetTransactionEnvelop.Populated = false;
    Update("UpdateCsenetTransactionEnvelop2",
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

  private void UpdateInterstateCase()
  {
    var sentDate = local.InterstateCase.SentDate;
    var sentTime = local.InterstateCase.SentTime.GetValueOrDefault();

    entities.InterstateCase.Populated = false;
    Update("UpdateInterstateCase",
      (db, command) =>
      {
        db.SetNullableDate(command, "sentDate", sentDate);
        db.SetNullableTimeSpan(command, "sentTime", sentTime);
        db.SetInt64(
          command, "transSerialNbr", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "transactionDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    entities.InterstateCase.SentDate = sentDate;
    entities.InterstateCase.SentTime = sentTime;
    entities.InterstateCase.Populated = true;
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
    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of Croup.
      /// </summary>
      [JsonPropertyName("croup")]
      public InterstateParticipant Croup
      {
        get => croup ??= new();
        set => croup = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant croup;
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
    /// A value of SendDateTime.
    /// </summary>
    [JsonPropertyName("sendDateTime")]
    public InterstateCase SendDateTime
    {
      get => sendDateTime ??= new();
      set => sendDateTime = value;
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
    /// A value of ControlTransSent.
    /// </summary>
    [JsonPropertyName("controlTransSent")]
    public Common ControlTransSent
    {
      get => controlTransSent ??= new();
      set => controlTransSent = value;
    }

    /// <summary>
    /// A value of ControlErrorRecords.
    /// </summary>
    [JsonPropertyName("controlErrorRecords")]
    public Common ControlErrorRecords
    {
      get => controlErrorRecords ??= new();
      set => controlErrorRecords = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
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
    /// A value of CsenetOutTransWritten.
    /// </summary>
    [JsonPropertyName("csenetOutTransWritten")]
    public Common CsenetOutTransWritten
    {
      get => csenetOutTransWritten ??= new();
      set => csenetOutTransWritten = value;
    }

    /// <summary>
    /// A value of RestartFileRec.
    /// </summary>
    [JsonPropertyName("restartFileRec")]
    public Common RestartFileRec
    {
      get => restartFileRec ??= new();
      set => restartFileRec = value;
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
    /// A value of Restart.
    /// </summary>
    [JsonPropertyName("restart")]
    public InterstateCase Restart
    {
      get => restart ??= new();
      set => restart = value;
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
    /// A value of TerminatingError.
    /// </summary>
    [JsonPropertyName("terminatingError")]
    public Common TerminatingError
    {
      get => terminatingError ??= new();
      set => terminatingError = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of ControlTotalReads.
    /// </summary>
    [JsonPropertyName("controlTotalReads")]
    public Common ControlTotalReads
    {
      get => controlTotalReads ??= new();
      set => controlTotalReads = value;
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
    /// A value of NeededToOpen.
    /// </summary>
    [JsonPropertyName("neededToOpen")]
    public EabReportSend NeededToOpen
    {
      get => neededToOpen ??= new();
      set => neededToOpen = value;
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
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// A value of ExtractPersonInfo.
    /// </summary>
    [JsonPropertyName("extractPersonInfo")]
    public External ExtractPersonInfo
    {
      get => extractPersonInfo ??= new();
      set => extractPersonInfo = value;
    }

    private DateWorkArea current;
    private InterstateCase sendDateTime;
    private Common debugOn;
    private Common controlTransSent;
    private Common controlErrorRecords;
    private InterstateMiscellaneous interstateMiscellaneous;
    private Array<CollectionGroup> collection;
    private Array<OrderGroup> order;
    private Array<ParticipantGroup> participant;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private Common csenetOutTransWritten;
    private Common restartFileRec;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase restart;
    private InterstateCase interstateCase;
    private Common terminatingError;
    private Common writeError;
    private Common controlTotalReads;
    private External external;
    private Common checkpointUpdates;
    private Common checkpointReads;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend neededToOpen;
    private EabReportSend neededToWrite;
    private ProgramCheckpointRestart programCheckpointRestart;
    private External extractPersonInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
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

    private Office office;
    private Case1 case1;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private CaseAssignment caseAssignment;
    private InterstateCase interstateCase;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
  }
#endregion
}

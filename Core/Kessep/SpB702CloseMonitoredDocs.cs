// Program: SP_B702_CLOSE_MONITORED_DOCS, ID: 372447354, model: 746.
// Short name: SWEP702B
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
/// A program: SP_B702_CLOSE_MONITORED_DOCS.
/// </para>
/// <para>
/// Input : DB2
/// Output : DB2
/// External Commit.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB702CloseMonitoredDocs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B702_CLOSE_MONITORED_DOCS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB702CloseMonitoredDocs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB702CloseMonitoredDocs.
  /// </summary>
  public SpB702CloseMonitoredDocs(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // -------------------------------------------------------------------------
    // 03/09/1999	M Ramirez			Initial Development
    // 10/03/1999	M Ramirez	75695		Auto close monitored docs
    // 						for OVERPYMT
    // 10/13/1999	M Brown		73652		Changed overpayment logic so
    // 						that a monitored document is
    // 						closed if the most
    // 						recent overpayment history
    // 						record has an indicator of
    // 						anything except 'N'.
    // 11/03/1999	M Ramirez	78148		Second Send date should
    // 						close previous DMONs
    // 01/31/2000	M Ramirez	78148		another attempt to close
    // 						this PR
    // 05/16/2000	M Ramirez	88381		In the case of an archived document,
    // 						match based on the denormalized
    // 						arttributes on infrastructure
    // 05/16/2000	M Ramirez			Removed extra attributes from
    // 						infrastructure and included new denorm
    // 						infrastructure entity view.
    // 						Changed READs to select only
    // 06/28/2000	M Ramirez	88381		Reopened - changed to close a monitored
    // 						document if the end_date was entered
    // 						AFTER the send_date
    // 08/08/2000	M Ramirez	99884		Archived documents are now handled the
    // 						same as unarchived documents
    // 11/14/2000	M Ramirez	104694		Added output to error report
    // 						to help determine problem with
    // 						not closing monitored docs
    // 						that should be closed
    // -------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB702Housekeeping();
    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;
    local.Max.Date = new DateTime(2099, 12, 31);

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // --------------------------------------------------------
      // No message will be given in Error Report because program
      // failed before the Error Report was created.
      // -----------------------------------------------------------
      return;
    }

    // ----------------------------------------------------------------
    // Records are processed in groups based on commit frequency
    // obtained from entity Program Checkpoint Restart.
    // Commits are performed at the end of each group.
    // ----------------------------------------------------------------
    local.ProgramCheckpointRestart.CheckpointCount = 0;
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.MonitoredDocument.ClosureReasonCode = "R";
    local.MonitoredDocument.LastUpdatedBy = local.ProgramProcessingInfo.Name;
    local.MonitoredDocument.LastUpdatedTimestamp = local.Current.Timestamp;
    local.Field.Dependancy = " KEY";

    // --------------------------------------------------------------------
    // Read all monitored_documents which are not closed
    // --------------------------------------------------------------------
    foreach(var item in ReadMonitoredDocumentInfrastructure())
    {
      // ----------------------------------------------------------------
      // Checkpoint is done against number of reads.
      // ----------------------------------------------------------------
      ++local.CheckpointNumbOfReads.Count;
      ++local.LcontrolTotalRead.Count;

      if (ReadOutgoingDocumentDocument())
      {
        switch(TrimEnd(entities.Document.Name))
        {
          case "EMPVERIF":
            break;
          case "EMPVERCO":
            break;
          case "POSTMAST":
            break;
          case "OVERPYMT":
            break;
          default:
            // mjr---> Not eligible for automatic closing.
            continue;
        }

        if (!ReadInfrastructure())
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Infrastructure not found for outgoing_document.";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        // mjr
        // ----------------------------------------------------------
        // 08/08/2000
        // PR# 99884 - Archived documents are now handled the same as unarchived
        // documents
        // -----------------------------------------------------------------------
        UseSpPrintDataRetrievalKeys();

        if (!IsEmpty(local.ErrorDocumentField.ScreenPrompt))
        {
          local.ErrorType.Text15 = "SYSTEM ERROR:";
          local.ErrorFieldValue.Value =
            local.ErrorDocumentField.ScreenPrompt + (
              local.ErrorFieldValue.Value ?? "");

          goto Read;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.ErrorType.Text15 = "SYSTEM ERROR:";
          local.ErrorFieldValue.Value = local.ExitStateWorkArea.Message;

          goto Read;
        }
        else
        {
        }

        // mjr
        // ----------------------------------------------------------
        // This document is a candidate for automatic closing.
        // Determine if it meets the criteria.
        // -------------------------------------------------------------
        if (Equal(entities.Document.Name, "EMPVERCO") || Equal
          (entities.Document.Name, "EMPVERIF"))
        {
          if (IsEmpty(local.SpDocKey.KeyPerson))
          {
            local.ErrorType.Text15 = "DATA ERROR:";
            local.ErrorFieldValue.Value =
              "No Person number for outgoing_document.";

            goto Read;
          }

          if (!ReadIncomeSource())
          {
            if (ReadCsePerson())
            {
              local.ErrorType.Text15 = "DATA ERROR:";
              local.ErrorFieldValue.Value =
                "Income Source not found for outgoing_document.";

              goto Read;
            }
            else
            {
              local.ErrorType.Text15 = "DATA ERROR:";
              local.ErrorFieldValue.Value =
                "CSE_Person not found for outgoing_document.";

              goto Read;
            }
          }

          if (!Equal(entities.Denorm.ReferenceDate, entities.IncomeSource.SentDt))
            
          {
          }
          else if (!Lt(entities.IncomeSource.ReturnDt,
            AddDays(entities.IncomeSource.SentDt, 3)))
          {
          }
          else if (Lt(entities.IncomeSource.SentDt, entities.IncomeSource.EndDt) &&
            !Equal(entities.IncomeSource.EndDt, local.Max.Date))
          {
          }
          else
          {
            continue;
          }

          local.MonitoredDocument.ActualResponseDate =
            local.ProgramProcessingInfo.ProcessDate;
        }
        else if (Equal(entities.Document.Name, "POSTMAST"))
        {
          if (IsEmpty(local.SpDocKey.KeyPerson))
          {
            local.ErrorType.Text15 = "DATA ERROR:";
            local.ErrorFieldValue.Value =
              "No Person number for outgoing_document.";

            goto Read;
          }

          if (!ReadCsePersonAddress())
          {
            if (ReadCsePerson())
            {
              local.ErrorType.Text15 = "DATA ERROR:";
              local.ErrorFieldValue.Value =
                "CSE Person Address not found for outgoing_document.";

              goto Read;
            }
            else
            {
              local.ErrorType.Text15 = "DATA ERROR:";
              local.ErrorFieldValue.Value =
                "CSE_Person not found for outgoing_document.";

              goto Read;
            }
          }

          if (!Equal(entities.Denorm.ReferenceDate,
            entities.CsePersonAddress.SendDate))
          {
          }
          else if (!Lt(entities.CsePersonAddress.VerifiedDate,
            entities.CsePersonAddress.SendDate))
          {
          }
          else if (Lt(entities.CsePersonAddress.SendDate,
            entities.CsePersonAddress.EndDate) && !
            Equal(entities.CsePersonAddress.EndDate, local.Max.Date))
          {
          }
          else
          {
            continue;
          }

          local.MonitoredDocument.ActualResponseDate =
            local.ProgramProcessingInfo.ProcessDate;
        }
        else
        {
          // mjr---> OVERPYMT
          if (IsEmpty(entities.Denorm.CsePersonNumber))
          {
            local.ErrorType.Text15 = "DATA ERROR:";
            local.ErrorFieldValue.Value =
              "No Person number for outgoing_document.";

            goto Read;
          }

          // : Oct 13, 1999, pr#73652,  mbrown - Changed sort to descend by 
          // created timestamp.
          ReadOverpaymentHistory();

          // : Oct 13, 1999, pr#73652,  mbrown - Do not close the document if 
          // the most
          // recent value for the overpayment indicator is 'N'
          if (AsChar(entities.OverpaymentHistory.OverpaymentInd) == 'N')
          {
            continue;
          }

          if (IsEmpty(entities.OverpaymentHistory.OverpaymentInd))
          {
            continue;
          }

          local.MonitoredDocument.ActualResponseDate =
            local.ProgramProcessingInfo.ProcessDate;
        }

        // mjr
        // ----------------------------------------------------------
        // This document needs to be closed.
        // -------------------------------------------------------------
        UseSpCabUpdateMonitoredDocument();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.ErrorType.Text15 = "DATA ERROR:";
          local.ErrorFieldValue.Value = local.ExitStateWorkArea.Message;

          goto Read;
        }

        ++local.LcontrolTotalProcessed.Count;
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Outgoing_document not found for monitored_document.";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

Read:

      if (!IsEmpty(local.ErrorType.Text15))
      {
        ++local.LcontrolTotalErred.Count;

        if (Equal(local.ErrorType.Text15, "DATA ERROR:"))
        {
          local.EabConvertNumeric.SendAmount =
            NumberToString(entities.Denorm.SystemGeneratedIdentifier, 15);
          UseEabConvertNumeric1();
          local.EabReportSend.RptDetail =
            "DATA ERROR:    Infrastructure ID = " + TrimEnd
            (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  " + TrimEnd
            (local.ErrorFieldValue.Value) + " -- USERID:  " + entities
            .Denorm.UserId;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          if (AsChar(local.DebugOn.Flag) == 'Y')
          {
            if (Equal(entities.Document.Name, "OVERPYMT"))
            {
              local.EabReportSend.RptDetail =
                "DEBUG     :          CSE_PERSON = " + entities
                .Denorm.CsePersonNumber;
            }
            else
            {
              local.EabReportSend.RptDetail =
                "DEBUG     :          CSE_PERSON = " + local
                .SpDocKey.KeyPerson;
            }

            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            if (Equal(entities.Document.Name, "OVERPYMT"))
            {
            }
            else if (Equal(entities.Document.Name, "POSTMAST"))
            {
              if (Lt(local.Null1.Timestamp, local.SpDocKey.KeyPersonAddress))
              {
                local.BatchTimestampWorkArea.TextTimestamp = "populated";
              }
              else
              {
                local.BatchTimestampWorkArea.TextTimestamp = "blank";
              }

              local.EabReportSend.RptDetail =
                "DEBUG     :          ADDRESS    = " + local
                .BatchTimestampWorkArea.TextTimestamp;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
            else
            {
              if (Lt(local.Null1.Timestamp, local.SpDocKey.KeyIncomeSource))
              {
                local.BatchTimestampWorkArea.TextTimestamp = "populated";
              }
              else
              {
                local.BatchTimestampWorkArea.TextTimestamp = "blank";
              }

              local.EabReportSend.RptDetail =
                "DEBUG     :       INCOME_SOURCE = " + local
                .BatchTimestampWorkArea.TextTimestamp;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }
            }
          }
        }
        else
        {
          local.EabReportSend.RptDetail = local.ErrorType.Text15 + (
            local.ErrorFieldValue.Value ?? "");
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }
        }

        local.ErrorType.Text15 = "";
        local.ErrorFieldValue.Value = Spaces(FieldValue.Value_MaxLength);
        local.EabReportSend.RptDetail = "";
      }

      // -----------------------------------------------------------------------
      // Commit processing
      // -----------------------------------------------------------------------
      if (local.CheckpointNumbOfReads.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // ----------------------------------------------------------------
        // Restart info in this program is Case Number and Child
        // number.
        // ----------------------------------------------------------------
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Denorm.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.ProgramCheckpointRestart.RestartInfo = "INFRASTRUCT:" + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal);
        local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart2();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to update program_checkpoint_restart.";

          break;
        }

        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          local.EabReportSend.RptDetail =
            "SYSTEM ERROR:  Unable to commit database.";

          break;
        }

        // -------------------------------------------------------------
        // Reset checkpoint counter
        // -------------------------------------------------------------
        local.CheckpointNumbOfReads.Count = 0;

        // *****End of Control Total Processing
      }

      // *****End of READ EACH
    }

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      // -----------------------------------------------------------
      // Ending as an ABEND
      // -----------------------------------------------------------
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      if (local.CheckpointNumbOfReads.Count < local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -----------------------------------------------------------
        // ABEND occurred at a time other than committing; commit now.
        // -----------------------------------------------------------
        local.EabConvertNumeric.SendAmount =
          NumberToString(entities.Denorm.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.ProgramCheckpointRestart.RestartInfo = "INFRASTRUCT:" + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal);
        local.ProgramCheckpointRestart.LastCheckpointTimestamp = Now();
        local.ProgramCheckpointRestart.RestartInd = "Y";
        UseUpdatePgmCheckpointRestart2();
        UseExtToDoACommit();
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
      local.ProgramCheckpointRestart.RestartInfo = "";
      local.ProgramCheckpointRestart.RestartInd = "N";
      UseUpdatePgmCheckpointRestart1();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
      }
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB702WriteControlsAndClose();
  }

  private static void MoveDocument(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAp = source.KeyAp;
    target.KeyAr = source.KeyAr;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAddress = source.KeyPersonAddress;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      local.EabConvertNumeric.ReturnNoCommasInNonDecimal;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnNoCommasInNonDecimal =
      useExport.EabConvertNumeric.ReturnNoCommasInNonDecimal;
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

  private void UseSpB702Housekeeping()
  {
    var useImport = new SpB702Housekeeping.Import();
    var useExport = new SpB702Housekeeping.Export();

    Call(SpB702Housekeeping.Execute, useImport, useExport);

    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.CheckpointRestart.SystemGeneratedIdentifier =
      useExport.Restart.SystemGeneratedIdentifier;
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.Current.Timestamp = useExport.Current.Timestamp;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
  }

  private void UseSpB702WriteControlsAndClose()
  {
    var useImport = new SpB702WriteControlsAndClose.Import();
    var useExport = new SpB702WriteControlsAndClose.Export();

    useImport.DocsRead.Count = local.LcontrolTotalRead.Count;
    useImport.DocsProcessed.Count = local.LcontrolTotalProcessed.Count;
    useImport.DocsDataError.Count = local.LcontrolTotalErred.Count;

    Call(SpB702WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCabUpdateMonitoredDocument()
  {
    var useImport = new SpCabUpdateMonitoredDocument.Import();
    var useExport = new SpCabUpdateMonitoredDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Denorm.SystemGeneratedIdentifier;
    useImport.MonitoredDocument.Assign(local.MonitoredDocument);

    Call(SpCabUpdateMonitoredDocument.Execute, useImport, useExport);
  }

  private void UseSpPrintDataRetrievalKeys()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    MoveDocument(entities.Document, useImport.Document);
    MoveInfrastructure(entities.Denorm, useImport.Infrastructure);
    useImport.Field.Dependancy = local.Field.Dependancy;

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    MoveSpDocKey(useExport.SpDocKey, local.SpDocKey);
    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", local.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          local.SpDocKey.KeyPersonAddress.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 4);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          local.SpDocKey.KeyIncomeSource.GetValueOrDefault());
        db.SetString(command, "cspINumber", local.SpDocKey.KeyPerson);
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 1);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 2);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 5);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Denorm.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Denorm.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Denorm.DenormTimestamp = db.GetNullableDateTime(reader, 1);
        entities.Denorm.CsePersonNumber = db.GetNullableString(reader, 2);
        entities.Denorm.UserId = db.GetString(reader, 3);
        entities.Denorm.ReferenceDate = db.GetNullableDate(reader, 4);
        entities.Denorm.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonitoredDocumentInfrastructure()
  {
    entities.Infrastructure.Populated = false;
    entities.MonitoredDocument.Populated = false;

    return ReadEach("ReadMonitoredDocumentInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", local.CheckpointRestart.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.MonitoredDocument.ClosureReasonCode =
          db.GetNullableString(reader, 0);
        entities.MonitoredDocument.InfId = db.GetInt32(reader, 1);
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.Infrastructure.Populated = true;
        entities.MonitoredDocument.Populated = true;

        return true;
      });
  }

  private bool ReadOutgoingDocumentDocument()
  {
    System.Diagnostics.Debug.Assert(entities.MonitoredDocument.Populated);
    entities.Document.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocumentDocument",
      (db, command) =>
      {
        db.SetInt32(command, "infId", entities.MonitoredDocument.InfId);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 1);
        entities.Document.Name = db.GetString(reader, 1);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 2);
        entities.Document.EffectiveDate = db.GetDate(reader, 2);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 3);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 4);
        entities.Document.Populated = true;
        entities.OutgoingDocument.Populated = true;
        CheckValid<OutgoingDocument>("FieldValuesArchiveInd",
          entities.OutgoingDocument.FieldValuesArchiveInd);
      });
  }

  private bool ReadOverpaymentHistory()
  {
    entities.OverpaymentHistory.Populated = false;

    return Read("ReadOverpaymentHistory",
      (db, command) =>
      {
        db.
          SetString(command, "cspNumber", entities.Denorm.CsePersonNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.OverpaymentHistory.CpaType = db.GetString(reader, 0);
        entities.OverpaymentHistory.CspNumber = db.GetString(reader, 1);
        entities.OverpaymentHistory.EffectiveDt = db.GetDate(reader, 2);
        entities.OverpaymentHistory.OverpaymentInd = db.GetString(reader, 3);
        entities.OverpaymentHistory.CreatedTmst = db.GetDateTime(reader, 4);
        entities.OverpaymentHistory.Populated = true;
        CheckValid<OverpaymentHistory>("CpaType",
          entities.OverpaymentHistory.CpaType);
        CheckValid<OverpaymentHistory>("OverpaymentInd",
          entities.OverpaymentHistory.OverpaymentInd);
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
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
    /// A value of ErrorType.
    /// </summary>
    [JsonPropertyName("errorType")]
    public WorkArea ErrorType
    {
      get => errorType ??= new();
      set => errorType = value;
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
    /// A value of Field.
    /// </summary>
    [JsonPropertyName("field")]
    public Field Field
    {
      get => field ??= new();
      set => field = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    /// <summary>
    /// A value of CheckpointRestart.
    /// </summary>
    [JsonPropertyName("checkpointRestart")]
    public Infrastructure CheckpointRestart
    {
      get => checkpointRestart ??= new();
      set => checkpointRestart = value;
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
    /// A value of LcontrolTotalErred.
    /// </summary>
    [JsonPropertyName("lcontrolTotalErred")]
    public Common LcontrolTotalErred
    {
      get => lcontrolTotalErred ??= new();
      set => lcontrolTotalErred = value;
    }

    /// <summary>
    /// A value of LcontrolTotalProcessed.
    /// </summary>
    [JsonPropertyName("lcontrolTotalProcessed")]
    public Common LcontrolTotalProcessed
    {
      get => lcontrolTotalProcessed ??= new();
      set => lcontrolTotalProcessed = value;
    }

    /// <summary>
    /// A value of LcontrolTotalRead.
    /// </summary>
    [JsonPropertyName("lcontrolTotalRead")]
    public Common LcontrolTotalRead
    {
      get => lcontrolTotalRead ??= new();
      set => lcontrolTotalRead = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
    }

    /// <summary>
    /// A value of CheckpointNumbOfReads.
    /// </summary>
    [JsonPropertyName("checkpointNumbOfReads")]
    public Common CheckpointNumbOfReads
    {
      get => checkpointNumbOfReads ??= new();
      set => checkpointNumbOfReads = value;
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
    /// A value of ErrorDocumentField.
    /// </summary>
    [JsonPropertyName("errorDocumentField")]
    public DocumentField ErrorDocumentField
    {
      get => errorDocumentField ??= new();
      set => errorDocumentField = value;
    }

    /// <summary>
    /// A value of ErrorFieldValue.
    /// </summary>
    [JsonPropertyName("errorFieldValue")]
    public FieldValue ErrorFieldValue
    {
      get => errorFieldValue ??= new();
      set => errorFieldValue = value;
    }

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common debugOn;
    private DateWorkArea max;
    private WorkArea errorType;
    private CsePerson csePerson;
    private Field field;
    private SpDocKey spDocKey;
    private MonitoredDocument monitoredDocument;
    private Infrastructure checkpointRestart;
    private DateWorkArea current;
    private Common lcontrolTotalErred;
    private Common lcontrolTotalProcessed;
    private Common lcontrolTotalRead;
    private DateWorkArea null1;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointNumbOfReads;
    private ExitStateWorkArea exitStateWorkArea;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private EabConvertNumeric2 eabConvertNumeric;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of OverpaymentHistory.
    /// </summary>
    [JsonPropertyName("overpaymentHistory")]
    public OverpaymentHistory OverpaymentHistory
    {
      get => overpaymentHistory ??= new();
      set => overpaymentHistory = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public Document Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of Denorm.
    /// </summary>
    [JsonPropertyName("denorm")]
    public Infrastructure Denorm
    {
      get => denorm ??= new();
      set => denorm = value;
    }

    /// <summary>
    /// A value of MonitoredDocument.
    /// </summary>
    [JsonPropertyName("monitoredDocument")]
    public MonitoredDocument MonitoredDocument
    {
      get => monitoredDocument ??= new();
      set => monitoredDocument = value;
    }

    private Document document;
    private Infrastructure infrastructure;
    private CsePersonAccount csePersonAccount;
    private OverpaymentHistory overpaymentHistory;
    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private OutgoingDocument outgoingDocument;
    private Document zdel;
    private Infrastructure denorm;
    private MonitoredDocument monitoredDocument;
  }
#endregion
}

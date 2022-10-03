// Program: SP_B705_GEN_COURT_NOTICE, ID: 372448302, model: 746.
// Short name: SWEP705B
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
/// A program: SP_B705_GEN_COURT_NOTICE.
/// </para>
/// <para>
/// This skeleton uses:
/// A DB2 table to drive processing
/// An external to do DB2 commits
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB705GenCourtNotice: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B705_GEN_COURT_NOTICE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB705GenCourtNotice(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB705GenCourtNotice.
  /// </summary>
  public SpB705GenCourtNotice(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------
    // Date		Developer	Request #      Description
    // --------------------------------------------------------------------
    // 08/07/1996	Sid Chowdhary			Initial Dev
    // 06/23/1997	Siraj Konkader	IDCR 340	Complete rewrite. Design
    // 						changed. New requirements.
    // 11/04/1997	Siraj Konkader	PR 31404	Notices need to be sent
    // 						for each individual collection
    // 						(Cash Receipt Detail in model).
    // 03/10/1999	M Ramirez			Re-work to remove DB2 error
    // 						report
    // 03/10/1999	M Ramirez			Re-work to create document
    // 						trigger, instead of creating
    // 						document
    // 03/10/1999	M Ramirez			Re-work to use housekeeping
    // 						and close cabs.
    // 10/22/1999	M Ramirez	78145		Add check for CRD
    // 						Collections_fully_applied = Y
    // 11/23/1999	M Ramirez	81273		Collections which are adjusted
    // 						before they are processed
    // 						by this batch should not
    // 						calculate into the total amount
    // 						Also, added parameter collection
    // 						start date
    // 05/10/2000	M Ramirez	91623		Changed PrAD to look at a DISTINCT
    // 						Standard Number, rather than a
    // 						DISTINCT Legal_Action_Id
    // 05/11/2000	M Ramirez	91623		Changed ERROR Message and removed ABEND
    // 05/30/2000	M Ramirez	91623 B		Infrastructure attributes get reset on
    // 						error from sp_create_document_infrast.
    // 						Removed setting these attributes from
    // 						BEFORE the loop to INSIDE the loop
    // --------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB705Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // --------------------------------------------------------
      // No message will be given in Error Report because program
      // failed before the Error Report was created.
      // Message will be given via the exitstate
      // -----------------------------------------------------------
      return;
    }

    // ----------------------------------------------------------------
    // Records are processed in groups based on commit frequency
    // obtained from entity Program Checkpoint Restart.
    // Commits are performed at the end of each group.
    // ----------------------------------------------------------------
    local.CheckpointRead.Count = 0;
    local.Document.Name = "COURTNOT";
    local.SpDocKey.KeyPersonAccount = "R";
    local.EabFileHandling.Action = "WRITE";
    local.Collection.LastUpdatedBy = local.ProgramProcessingInfo.Name;
    local.Collection.LastUpdatedTmst = local.Current.Timestamp;

    // --------------------------------------------------------------------
    // Apart from a normal collection, there are two scenarios
    // that need to be considered:
    // Please note that this discussion is only for non court
    // collections.
    // 1. Joint and Several Obligors. This is when more than one
    // obligor is ordered to pay on the same obligation.
    //  Any payment is credited to all the obligors but the
    // court notice should be sent out only once and it can
    // contain any obligor number.
    // 2. Multi Payor: Each obligor is ordered to pay different
    // amounts on different obligations.
    //          Per Dixie there is no scenario where a single
    // collection is split up for multiple payors. Since we are
    // reporting per collection we would never have to worry
    // about which obligor made this payment. The entire coll
    // will be applied to the obligor making the payment.
    // --------------------------------------------------------------------
    // --------------------------------------------------------------------
    // The Read Each below is set to DISTINCT.
    // It will only pick up a distinct group of cash_receipt_detail, 
    // legal_action
    // and cse_person_acct that have some collection that requires a court 
    // notice
    // document printed for it.
    // --------------------------------------------------------------------
    // mjr
    // -------------------------------------------------------
    // 03/24/1999
    // Removed requirement that court_notice_processed_date = NULL in
    // following READEACH.
    // We also need to process adjustments, which have
    // court_notice_processed_date > NULL.
    // NOTE:  collection actual_date_court_notice_process is being used as
    // adjustment_court_notice_processed_date.
    // ---------------------------------------------------------------------
    // mjr
    // -------------------------------------------------------
    // 10/22/1999
    // Added check for CRD Collection_fully_applied = Y
    // ---------------------------------------------------------------------
    foreach(var item in ReadCashReceiptDetailCsePersonAccountLegalAction())
    {
      // --------------------------------------------------------------------
      // Checkpoint is done against number of reads. Do not change.
      // --------------------------------------------------------------------
      ++local.CheckpointRead.Count;
      ++local.LcontrolTotalRead.Count;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        if (ReadCsePerson())
        {
          local.EabReportSend.RptDetail = "DEBUG:  Obligor = " + entities
            .CsePerson.Number + ";  LA Standard No = " + entities
            .StandardNumber.StandardNumber;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.EabReportSend.RptDetail = "";
        }
      }

      // mjr
      // -------------------------------------------------------
      // 12/02/1999
      // Added check for collection start date
      // ---------------------------------------------------------------------
      if (Lt(local.Null1.Date, local.CollectionStart.Date))
      {
        if (Lt(entities.CashReceiptDetail.CollectionDate,
          local.CollectionStart.Date))
        {
          foreach(var item1 in ReadCollection2())
          {
            goto ReadEach1;
          }
        }
      }

      // mjr
      // -------------------------------------------
      // 03/24/1999
      // Determine if a document needs to be generated.
      // --------------------------------------------------------
      local.PrintDocument.Flag = "N";
      local.Total.Amount = 0;

      foreach(var item1 in ReadCollection1())
      {
        if (AsChar(entities.Collection.AdjustedInd) == 'Y')
        {
          if (Lt(local.Null1.Date, entities.Collection.CourtNoticeProcessedDate))
            
          {
            // mjr
            // --------------------------------------------------------------
            // A payment has been reported to the court, an adjustment has been
            // made to that payment, and no notice has been given to the court
            // about the adjustment.
            // -----------------------------------------------------------------
            local.Collection.CourtNoticeProcessedDate =
              entities.Collection.CourtNoticeProcessedDate;
            local.Collection.CourtNoticeAdjProcessDate =
              local.ProgramProcessingInfo.ProcessDate;
          }
          else
          {
            // mjr
            // --------------------------------------------------------------
            // A payment has not been reported to the court, and an adjustment
            // has been made to that payment.
            // -----------------------------------------------------------------
            // mjr
            // ---------------------------------------------------------
            // 11/23/1999
            // Collections which are adjusted before they are processed by this 
            // batch
            // should not calculate into the total amount
            // ----------------------------------------------------------------------
            try
            {
              UpdateCollection2();

              continue;
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

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabExtractExitStateMessage();
              local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
                .ExitStateWorkArea.Message;

              goto ReadEach2;
            }
          }
        }
        else if (Lt(local.Null1.Date,
          entities.Collection.CourtNoticeProcessedDate))
        {
          // mjr
          // --------------------------------------------------------------
          // A payment has been reported to the court, and no adjustment
          // has been made to that payment.
          // -----------------------------------------------------------------
          continue;
        }
        else
        {
          // mjr
          // --------------------------------------------------------------
          // A payment has not been reported to the court, and no adjustment
          // has been made to that payment.
          // -----------------------------------------------------------------
          local.Collection.CourtNoticeProcessedDate =
            local.ProgramProcessingInfo.ProcessDate;
          local.Collection.CourtNoticeAdjProcessDate = local.Null1.Date;
        }

        // mjr
        // --------------------------------------------------------------
        // It has been determined to send a document.  Update this collection
        // so it will be included in the document totals.
        // -----------------------------------------------------------------
        try
        {
          UpdateCollection1();
          local.PrintDocument.Flag = "Y";

          if (AsChar(entities.Collection.AdjustedInd) == 'Y')
          {
            local.Total.Amount -= entities.Collection.Amount;
          }
          else
          {
            local.Total.Amount += entities.Collection.Amount;
          }
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

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabExtractExitStateMessage();
          local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
            .ExitStateWorkArea.Message;

          goto ReadEach2;
        }
      }

      if (AsChar(local.PrintDocument.Flag) == 'N')
      {
        continue;
      }

      if (local.Total.Amount == 0)
      {
        // mjr
        // -----------------------------------------------------------------
        // Since collections balance, nothing needs to be reported to the court.
        // (To them, nothing has changed.)
        // --------------------------------------------------------------------
        continue;
      }

      // mjr
      // ----------------------------------------------------------------
      // Get keys for cash_receipt_detail to pass into 
      // sp_create_document_infrastruct
      // -------------------------------------------------------------------
      if (!ReadCashReceiptType())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_receipt_type missing for cash_receipt_detail.";

        break;
      }

      if (!ReadCashReceiptEvent())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_receipt_event missing for cash_receipt_detail.";

        break;
      }

      if (!ReadCashReceiptSourceType())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cash_receipt_source_type missing for cash_receipt_detail.";

        break;
      }

      // mjr
      // ----------------------------------------------------------------
      // Get keys for cse_person to pass into sp_create_document_infrastruct
      // -------------------------------------------------------------------
      if (!ReadCsePerson())
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Cse_person missing for cse_person_account.";

        break;
      }

      // mjr
      // ----------------------------------------------------------------
      // Get keys for legal_action to pass into sp_create_document_infrastruct
      // -------------------------------------------------------------------
      ReadLegalAction();

      if (entities.LegalAction.Identifier <= 0)
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Legal_Action missing for Standard Number.";

        break;
      }

      // mjr
      // --------------------------------------------------------------
      // Create document_trigger
      // -----------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier = 0;
      local.Infrastructure.ReferenceDate =
        local.ProgramProcessingInfo.ProcessDate;
      local.Infrastructure.CreatedBy = local.ProgramProcessingInfo.Name;
      local.Infrastructure.CreatedTimestamp = local.Current.Timestamp;
      local.SpDocKey.KeyPerson = entities.CsePerson.Number;
      local.SpDocKey.KeyLegalAction = entities.LegalAction.Identifier;
      local.SpDocKey.KeyCashRcptDetail =
        entities.CashReceiptDetail.SequentialIdentifier;
      local.SpDocKey.KeyCashRcptEvent =
        entities.CashReceiptEvent.SystemGeneratedIdentifier;
      local.SpDocKey.KeyCashRcptSource =
        entities.CashReceiptSourceType.SystemGeneratedIdentifier;
      local.SpDocKey.KeyCashRcptType =
        entities.CashReceiptType.SystemGeneratedIdentifier;
      UseSpCreateDocumentInfrastruct();

      // --------------------------------------------------------------------
      // For critical errors that need to abend the program, set
      // an abort exit state and escape.
      // For non-critical errors you may write an error record to
      // the program error entity type.
      // --------------------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ++local.LcontrolTotalErred.Count;
        UseEabExtractExitStateMessage();

        // mjr
        // ------------------------------------------------------
        // 05/11/2000
        // Changed ERROR Message and removed ABEND
        // -------------------------------------------------------------------
        local.EabReportSend.RptDetail = "DATA ERROR:  Obligor = " + entities
          .CsePerson.Number + "; LA Standard No = " + entities
          .StandardNumber.StandardNumber + "; ERROR = " + local
          .ExitStateWorkArea.Message;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";

        // mjr
        // -------------------------------------------------------
        // 12/01/1999
        // Added setting exitstate, since it was never reset
        // ---------------------------------------------------------------------
        ExitState = "ACO_NN0000_ALL_OK";
      }
      else
      {
        ++local.LcontrolTotalProcessed.Count;
      }

      // -----------------------------------------------------------------------
      // Commit processing
      // -----------------------------------------------------------------------
      if (local.CheckpointRead.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
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
        local.CheckpointRead.Count = 0;
      }

      // *****End of READ EACH
ReadEach1:
      ;
    }

ReadEach2:

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

      if (local.CheckpointRead.Count < local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault())
      {
        // -----------------------------------------------------------
        // ABEND occurred at a time other than committing; commit now.
        // -----------------------------------------------------------
        UseExtToDoACommit();
      }

      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";
    }
    else
    {
      // -----------------------------------------------------------
      // Successful end for this program
      // -----------------------------------------------------------
    }

    // -----------------------------------------------------------
    // Write control totals and close reports
    // -----------------------------------------------------------
    UseSpB705WriteControlsAndClose();
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
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
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
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

  private void UseSpB705Housekeeping()
  {
    var useImport = new SpB705Housekeeping.Import();
    var useExport = new SpB705Housekeeping.Export();

    Call(SpB705Housekeeping.Execute, useImport, useExport);

    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
    local.Current.Timestamp = useExport.Current.Timestamp;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.CollectionStart.Date = useExport.CollectionStart.Date;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
  }

  private void UseSpB705WriteControlsAndClose()
  {
    var useImport = new SpB705WriteControlsAndClose.Import();
    var useExport = new SpB705WriteControlsAndClose.Export();

    useImport.DocsRead.Count = local.LcontrolTotalRead.Count;
    useImport.DocsProcessed.Count = local.LcontrolTotalProcessed.Count;
    useImport.DocsDataError.Count = local.LcontrolTotalErred.Count;

    Call(SpB705WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCreateDocumentInfrastruct()
  {
    var useImport = new SpCreateDocumentInfrastruct.Import();
    var useExport = new SpCreateDocumentInfrastruct.Export();

    useImport.Document.Name = local.Document.Name;
    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpCreateDocumentInfrastruct.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private IEnumerable<bool> ReadCashReceiptDetailCsePersonAccountLegalAction()
  {
    entities.StandardNumber.Populated = false;
    entities.CsePersonAccount.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCsePersonAccountLegalAction",
      (db, command) =>
      {
        db.SetDate(
          command, "crtNtcAdjPrcDt", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", local.SpDocKey.KeyPersonAccount);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 5);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 6);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 7);
        entities.StandardNumber.Identifier = db.GetInt32(reader, 8);
        entities.StandardNumber.StandardNumber =
          db.GetNullableString(reader, 9);
        entities.StandardNumber.Populated = true;
        entities.CsePersonAccount.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);

        return true;
      });
  }

  private bool ReadCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(
          command, "crtNtcAdjPrcDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.StandardNumber.StandardNumber ?? "");
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
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 17);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 18);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Collection.Populated = false;

    return ReadEach("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(command, "crvId", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(command, "cstId", entities.CashReceiptDetail.CstIdentifier);
        db.
          SetInt32(command, "crtType", entities.CashReceiptDetail.CrtIdentifier);
          
        db.SetDate(
          command, "crtNtcAdjPrcDt", local.Null1.Date.GetValueOrDefault());
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
        entities.Collection.CpaType = db.GetString(reader, 9);
        entities.Collection.OtrId = db.GetInt32(reader, 10);
        entities.Collection.OtrType = db.GetString(reader, 11);
        entities.Collection.OtyId = db.GetInt32(reader, 12);
        entities.Collection.CreatedTmst = db.GetDateTime(reader, 13);
        entities.Collection.LastUpdatedBy = db.GetNullableString(reader, 14);
        entities.Collection.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 15);
        entities.Collection.Amount = db.GetDecimal(reader, 16);
        entities.Collection.CourtNoticeReqInd =
          db.GetNullableString(reader, 17);
        entities.Collection.CourtNoticeProcessedDate =
          db.GetNullableDate(reader, 18);
        entities.Collection.CourtNoticeAdjProcessDate = db.GetDate(reader, 19);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CsePersonAccount.CspNumber);
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
        db.SetNullableString(
          command, "standardNo", entities.StandardNumber.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 1);
        entities.LegalAction.Populated = true;
      });
  }

  private void UpdateCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = local.Collection.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.Collection.LastUpdatedTmst;
    var courtNoticeProcessedDate = local.Collection.CourtNoticeProcessedDate;
    var courtNoticeAdjProcessDate = local.Collection.CourtNoticeAdjProcessDate;

    entities.Collection.Populated = false;
    Update("UpdateCollection1",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "crtNoticeProcDt", courtNoticeProcessedDate);
          
        db.SetDate(command, "crtNtcAdjPrcDt", courtNoticeAdjProcessDate);
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

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CourtNoticeProcessedDate = courtNoticeProcessedDate;
    entities.Collection.CourtNoticeAdjProcessDate = courtNoticeAdjProcessDate;
    entities.Collection.Populated = true;
  }

  private void UpdateCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);

    var lastUpdatedBy = local.Collection.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.Collection.LastUpdatedTmst;
    var courtNoticeProcessedDate = local.ProgramProcessingInfo.ProcessDate;

    entities.Collection.Populated = false;
    Update("UpdateCollection2",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.
          SetNullableDate(command, "crtNoticeProcDt", courtNoticeProcessedDate);
          
        db.SetDate(command, "crtNtcAdjPrcDt", courtNoticeProcessedDate);
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

    entities.Collection.LastUpdatedBy = lastUpdatedBy;
    entities.Collection.LastUpdatedTmst = lastUpdatedTmst;
    entities.Collection.CourtNoticeProcessedDate = courtNoticeProcessedDate;
    entities.Collection.CourtNoticeAdjProcessDate = courtNoticeProcessedDate;
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of CollectionStart.
    /// </summary>
    [JsonPropertyName("collectionStart")]
    public DateWorkArea CollectionStart
    {
      get => collectionStart ??= new();
      set => collectionStart = value;
    }

    /// <summary>
    /// A value of PrintDocument.
    /// </summary>
    [JsonPropertyName("printDocument")]
    public Common PrintDocument
    {
      get => printDocument ??= new();
      set => printDocument = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of CheckpointRead.
    /// </summary>
    [JsonPropertyName("checkpointRead")]
    public Common CheckpointRead
    {
      get => checkpointRead ??= new();
      set => checkpointRead = value;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    /// <summary>
    /// A value of ProgramControlTotal.
    /// </summary>
    [JsonPropertyName("programControlTotal")]
    public ProgramControlTotal ProgramControlTotal
    {
      get => programControlTotal ??= new();
      set => programControlTotal = value;
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
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
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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

    private Common debugOn;
    private DateWorkArea collectionStart;
    private Common printDocument;
    private Collection collection;
    private DateWorkArea current;
    private Collection total;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea null1;
    private Common lcontrolTotalErred;
    private Common lcontrolTotalProcessed;
    private Common lcontrolTotalRead;
    private Document document;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private External passArea;
    private Common checkpointRead;
    private ProgramError programError;
    private ProgramControlTotal programControlTotal;
    private ExitStateWorkArea exitStateWorkArea;
    private EabFileHandling eabFileHandling;
    private Infrastructure infrastructure;
    private SpDocKey spDocKey;
    private EabReportSend eabReportSend;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea workArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StandardNumber.
    /// </summary>
    [JsonPropertyName("standardNumber")]
    public LegalAction StandardNumber
    {
      get => standardNumber ??= new();
      set => standardNumber = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    private LegalAction standardNumber;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CashReceiptDetail cashReceiptDetail;
    private Obligation obligation;
    private LegalAction legalAction;
    private Collection collection;
  }
#endregion
}

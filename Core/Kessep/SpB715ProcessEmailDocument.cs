// Program: SP_B715_PROCESS_EMAIL_DOCUMENT, ID: 371339399, model: 746.
// Short name: SWEP715B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B715_PROCESS_EMAIL_DOCUMENT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB715ProcessEmailDocument: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B715_PROCESS_EMAIL_DOCUMENT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB715ProcessEmailDocument(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB715ProcessEmailDocument.
  /// </summary>
  public SpB715ProcessEmailDocument(IContext context, Import import,
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
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 01/28/2008	M Ramirez	WR 276, 277	Initial Development.  Copied from 
    // SWEPB709
    // 02/01/2008	M Ramirez	WR 276, 277	Added continuous execution
    // 07/30/2008	J Huss		PR 984		Removed Exit State If statement to allow 
    // restart in nearly all cases.
    // 						Added read delay to make sure nothing else is still writing to a 
    // record.
    // 04/01/2009	J Huss		CQ 10264	Removed sorting from main Read Each to allow 
    // index to be utilized properly.
    // ---------------------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // Processing Description
    // ----------------------------------------------------------------------------
    // This process runs all day, sleeping at intervals, and stopping at the end
    // of the
    // day, defined by Program Processing Info parameters.
    // ----------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB715Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // mjr
      // --------------------------------------------------------
      // No message will be given in Error Report because program
      // failed before the Error Report was created.
      // Retain the message from the exitstate.
      // -----------------------------------------------------------
      return;
    }

    local.Current.Date = local.ProgramProcessingInfo.ProcessDate;

    // mjr
    // --------------------------------------------------------
    // Outgoing_document records with the print_successful_ind = 'M'
    // are triggers for this PrAD.
    // When a record is found for processing, check the reference_date
    // on infrastructure to verify that this record is ready to be
    // processed.  Then call sp_print_data_retrieval_main to retrieve
    // the data for the document.
    // -----------------------------------------------------------
    local.EabFileHandling.Action = "WRITE";
    local.EabConvertNumeric.SendNonSuppressPos = 1;
    local.Field.Dependancy = " KEY";
    local.RowLockDocument.Count = 2;
    local.RowLockFieldValue.Count = 2;

    // 07/30/2008	J Huss	Only going to process records that were last updated at
    // least 1 minute ago.
    local.Cutoff.LastUpdatdTstamp = Now().AddMinutes(-1);

    // 07/30/2008	J Huss	Added Timestamp criteria to read statement.
    // 04/01/2009	J Huss	Removed sorting from main Read Each to allow index to 
    // be utilized properly.
    // 			Moved timestamp criteria to stand alone check.
    foreach(var item in ReadOutgoingDocumentDocument())
    {
      if (Lt(local.Cutoff.LastUpdatdTstamp,
        entities.OutgoingDocument.LastUpdatdTstamp))
      {
        continue;
      }

      ExitState = "ACO_NN0000_ALL_OK";
      local.Document.Assign(local.NullDocument);
      local.Infrastructure.Assign(local.NullInfrastructure);

      // mjr---> 'P' indicates successful processing, ready for the email 
      // service
      local.OutgoingDocument.PrintSucessfulIndicator = "P";

      // mjr
      // --------------------------------------------------------
      // Find the current documents details and place them in Local Document and
      // Local Event Detail.
      // Only use Local Document and Local Event Detail in the rest of the PrAD
      // -----------------------------------------------------------
      for(local.DocumentDetails.Index = 0; local.DocumentDetails.Index < local
        .DocumentDetails.Count; ++local.DocumentDetails.Index)
      {
        if (!local.DocumentDetails.CheckSize())
        {
          break;
        }

        if (Equal(entities.Document.Name,
          local.DocumentDetails.Item.GlocalDocumentDetailDocument.Name) && Equal
          (entities.Document.EffectiveDate,
          local.DocumentDetails.Item.GlocalDocumentDetailDocument.
            EffectiveDate))
        {
          local.Document.Assign(
            local.DocumentDetails.Item.GlocalDocumentDetailDocument);
          local.EventDetail.ExceptionRoutine =
            local.DocumentDetails.Item.GlocalDocumentDetailEventDetail.
              ExceptionRoutine;

          break;
        }
      }

      local.DocumentDetails.CheckIndex();

      if (!Equal(entities.Document.Name, local.Document.Name) || !
        Equal(entities.Document.EffectiveDate, local.Document.EffectiveDate))
      {
        // mjr
        // --------------------------------------------------------
        // A document has an 'M' status but is not a MAIL document.
        // Update document as an error
        // -----------------------------------------------------------
        ++local.DocsSystemError.Count;

        if (ReadInfrastructure())
        {
          local.Infrastructure.SystemGeneratedIdentifier =
            entities.Infrastructure.SystemGeneratedIdentifier;
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Infrastructure not found for outgoing_document.";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        if (ReadDocument())
        {
          MoveDocument1(entities.Detail, local.Document);
        }
        else
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Document not found for outgoing_document.";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.WorkArea.Text50 =
          "SYSTEM ERROR:  Document is not an email document";
        local.EabReportSend.RptDetail = "";
        UseSpDocUpdateFailedBatchDoc();

        if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
        {
          // mjr-------> Trouble writing to Error Report:  quit immediately
          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr-------> Trouble updating DB2:  terminate
          break;
        }
        else
        {
          // mjr-------> Document error:  proceed to the next record
          continue;
        }
      }

      if (local.Totals.Count > 0)
      {
        for(local.Totals.Index = 0; local.Totals.Index < local.Totals.Count; ++
          local.Totals.Index)
        {
          if (!local.Totals.CheckSize())
          {
            break;
          }

          if (Equal(local.Document.Name, local.Totals.Item.GlocalTotals.Name) &&
            Equal
            (local.Document.VersionNumber,
            local.Totals.Item.GlocalTotals.VersionNumber))
          {
            break;
          }
        }

        local.Totals.CheckIndex();

        if (!Equal(local.Document.Name, local.Totals.Item.GlocalTotals.Name) ||
          !
          Equal(local.Document.VersionNumber,
          local.Totals.Item.GlocalTotals.VersionNumber))
        {
          local.Totals.Index = local.Totals.Count;
          local.Totals.CheckSize();

          MoveDocument3(local.Document, local.Totals.Update.GlocalTotals);
        }
      }
      else
      {
        local.Totals.Index = 0;
        local.Totals.CheckSize();

        MoveDocument3(local.Document, local.Totals.Update.GlocalTotals);
      }

      ++local.RowLockDocument.Count;
      ++local.DocsRead.Count;
      local.Totals.Update.GlocalTotalsRead.Count =
        local.Totals.Item.GlocalTotalsRead.Count + 1;

      // mjr
      // -------------------------------------------------------------
      // Document exceptions will not be processed
      // ----------------------------------------------------------------
      for(local.Exceptions.Index = 0; local.Exceptions.Index < local
        .Exceptions.Count; ++local.Exceptions.Index)
      {
        if (!local.Exceptions.CheckSize())
        {
          break;
        }

        if (Equal(local.Exceptions.Item.GlocalException.Name,
          local.Document.Name))
        {
          ++local.DocsException.Count;
          local.Totals.Update.GlocalTotalsException.Count =
            local.Totals.Item.GlocalTotalsException.Count + 1;

          goto ReadEach1;
        }
      }

      local.Exceptions.CheckIndex();

      if (ReadInfrastructure())
      {
        if (Lt(local.ProgramProcessingInfo.ProcessDate,
          entities.Infrastructure.ReferenceDate))
        {
          // mjr---> This document is not due to be processed yet.
          ++local.DocsFuture.Count;
          local.Totals.Update.GlocalTotalsFuture.Count =
            local.Totals.Item.GlocalTotalsFuture.Count + 1;

          continue;
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Infrastructure not found for outgoing_document.";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      UseSpPrintDataRetrievalMain();

      if (!IsEmpty(local.ErrorDocumentField.ScreenPrompt))
      {
        if (Equal(local.ErrorDocumentField.ScreenPrompt, "Resource Error"))
        {
          // mjr
          // -----------------------------------------------------------
          // Because a resource (normally ADABAS) is unavailable,
          // there is no need to continue the batch.
          // --------------------------------------------------------------
          local.EabReportSend.RptDetail =
            "ABEND:  Resource Unavailable  (usually ADABAS).";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          break;
        }

        local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
          .ErrorDocumentField.ScreenPrompt + (
            local.ErrorFieldValue.Value ?? "");
      }
      else if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabReportSend.RptDetail = "SYSTEM ERROR:  " + local
          .ExitStateWorkArea.Message;
      }

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        // mjr
        // -------------------------------------------------------
        // Document failed print due to system error:
        //   add 1 to SYSTEM ERROR count
        //   write to ERROR REPORT
        // ------------------------------------------------------------
        ++local.DocsSystemError.Count;
        local.Totals.Update.GlocalTotalsSystemError.Count =
          local.Totals.Item.GlocalTotalsSystemError.Count + 1;
        local.WorkArea.Text50 = local.EabReportSend.RptDetail;
        local.EabReportSend.RptDetail = "";
        UseSpDocUpdateFailedBatchDoc();

        if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
        {
          // mjr-------> Trouble writing to Error Report:  quit immediately
          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr-------> Trouble updating DB2:  terminate
          break;
        }
        else
        {
          // mjr-------> Document error:  proceed to the next record
          continue;
        }
      }

      // mjr
      // ------------------------------------------------------
      // Check for missing and mandatory fields.
      // ---------------------------------------------------------
      local.RequiredFieldMissing.Flag = "N";
      local.UserinputField.Flag = "N";

      foreach(var item1 in ReadDocumentField2())
      {
        if (ReadFieldValue())
        {
          local.FieldValue.Value = entities.FieldValue.Value;
        }
        else
        {
          local.FieldValue.Value = Spaces(FieldValue.Value_MaxLength);
        }

        if (!ReadField())
        {
          local.EabReportSend.RptDetail =
            "ABEND:  Field not found for document_field.";
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          goto ReadEach2;
        }

        if (IsEmpty(local.FieldValue.Value) && AsChar
          (entities.DocumentField.RequiredSwitch) != 'N')
        {
          local.RequiredFieldMissing.Flag = "Y";
        }

        if (Equal(entities.Field.SubroutineName, "USRINPUT"))
        {
          local.UserinputField.Flag = "Y";
        }
      }

      if (AsChar(local.UserinputField.Flag) == 'Y')
      {
        // mjr
        // -------------------------------------------------------
        // Document contains a User Input field:
        //   add 1 to WARNING count
        //   write to ERROR REPORT
        // ------------------------------------------------------------
        ++local.DocsWarning.Count;
        local.Totals.Update.GlocalTotalsWarning.Count =
          local.Totals.Item.GlocalTotalsWarning.Count + 1;
        local.EabReportSend.RptDetail =
          "SYSTEM WARNING:  Document contains one or more User Input fields which cannot be populated.        -- Document Name = " +
          local.Document.Name;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      if (AsChar(local.RequiredFieldMissing.Flag) == 'Y')
      {
        // mjr
        // -------------------------------------------------------
        // Document failed print due to data error:
        //   add 1 to DATA ERROR count
        //   write to ERROR REPORT
        // ------------------------------------------------------------
        ++local.DocsDataError.Count;
        local.Totals.Update.GlocalTotalsDataError.Count =
          local.Totals.Item.GlocalTotalsDataError.Count + 1;
        local.WorkArea.Text50 = "Missing mandatory field";
        local.EabReportSend.RptDetail = "";
        UseSpDocUpdateFailedBatchDoc();

        if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
        {
          // mjr-------> Trouble writing to Error Report:  quit immediately
          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr-------> Trouble updating DB2:  terminate
          break;
        }
        else
        {
          // mjr-------> Document error:  proceed to the next record
          continue;
        }
      }

      if (!IsEmpty(local.ErrorInd.Flag))
      {
        // mjr---->  Determine why the error ind is populated
        local.WorkArea.Text50 = TrimEnd(local.SpDocLiteral.IdDocument) + "RET"
          + local.ErrorInd.Flag;
        UseSpPrintDecodeReturnCode();

        if (!IsExitState("SP0000_DOWNLOAD_SUCCESSFUL"))
        {
          // mjr
          // -------------------------------------------------------
          // Document failed print due to data error:
          //   add 1 to DATA ERROR count
          //   write to ERROR REPORT
          // ------------------------------------------------------------
          ++local.DocsDataError.Count;
          local.Totals.Update.GlocalTotalsDataError.Count =
            local.Totals.Item.GlocalTotalsDataError.Count + 1;
          UseEabExtractExitStateMessage();
          local.WorkArea.Text50 = local.ExitStateWorkArea.Message;
          local.EabReportSend.RptDetail = "";
          UseSpDocUpdateFailedBatchDoc();

          if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
          {
            // mjr-------> Trouble writing to Error Report:  quit immediately
            return;
          }
          else if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // mjr-------> Trouble updating DB2:  terminate
            break;
          }
          else
          {
            // mjr-------> Document error:  proceed to the next record
            continue;
          }
        }
        else
        {
          // mjr
          // ---------------------------------------------------
          // 12/02/1999
          // Reset exitstate to All_OK after it is changed
          // ----------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      // mjr
      // -----------------------------------------------------------------
      // Check to make sure email address exists for the Service Provider.
      // --------------------------------------------------------------------
      if (IsEmpty(local.Infrastructure.UserId))
      {
        local.EabReportSend.RptDetail =
          "email:  Missing Infrastructure USER_ID";
      }
      else if (ReadServiceProvider())
      {
        if (IsEmpty(entities.ServiceProvider.EmailAddress))
        {
          local.EabReportSend.RptDetail =
            "email:  Service Provider email address is blank";
        }
      }
      else
      {
        local.EabReportSend.RptDetail =
          "email:  Inf USER_ID not matched in Service Provider";
      }

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        // mjr
        // -------------------------------------------------------
        // Document failed print due to data error:
        //   add 1 to DATA ERROR count
        //   write to ERROR REPORT
        // ------------------------------------------------------------
        ++local.DocsDataError.Count;
        local.Totals.Update.GlocalTotalsDataError.Count =
          local.Totals.Item.GlocalTotalsDataError.Count + 1;
        local.WorkArea.Text50 = local.EabReportSend.RptDetail;
        local.EabReportSend.RptDetail = "";
        UseSpDocUpdateFailedBatchDoc();

        if (IsExitState("OE0000_ERROR_WRITING_EXT_FILE"))
        {
          // mjr-------> Trouble writing to Error Report:  quit immediately
          return;
        }
        else if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // mjr-------> Trouble updating DB2:  terminate
          break;
        }
        else
        {
          // mjr-------> Document error:  proceed to the next record
          continue;
        }
      }

      // mjr
      // -----------------------------------------------------------------
      // Document printing is successful.
      // Update outgoing_document, create monitored document (if
      //     necessary), and update infrastructure record.
      // --------------------------------------------------------------------
      local.Infrastructure.SystemGeneratedIdentifier =
        local.Infrastructure.SystemGeneratedIdentifier;
      local.Infrastructure.LastUpdatedBy = local.ProgramProcessingInfo.Name;
      local.Infrastructure.LastUpdatedTimestamp = local.Current.Timestamp;
      UseSpDocUpdateSuccessfulPrint();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Could not update outgoing_document after a successful generation.";
          
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // mjr
      // -----------------------------------------------------------------
      // Reset Print Successful Indicator to 'P' to be ready for the email 
      // service
      // We could also change the SP_DOC_UPDATE_SUCCESSFUL_PRINT to handle this 
      // scenario.
      // --------------------------------------------------------------------
      if (Equal(entities.Document.Name, "ORDIWO2B") || Equal
        (entities.Document.Name, "ORDIWO2A"))
      {
        // turned off auto iwo emails. will go directly to 'G' status
        // (to be picked up by the batch document server) instead of 'P' status 
        // to be picked up by the email document server.
        // cq60707
        local.OutgoingDocument.PrintSucessfulIndicator = "G";

        if (Equal(entities.Document.Name, "ORDIWO2A"))
        {
          if (ReadDocumentField1())
          {
            if (ReadFieldValue())
            {
              if (Equal(entities.FieldValue.Value, "Y"))
              {
                // FOR EIWO EMPLOYERS IT WILL BE SET TO "D"  (ready for portal)
                // That is currently done by the email document server, and 
                // those documents will no longer go to the server.
                local.OutgoingDocument.PrintSucessfulIndicator = "D";
              }
            }
          }
        }
      }

      UseUpdateOutgoingDocument();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        local.EabReportSend.RptDetail =
          "ABEND:  Unable to update outgoing_document after successful print";
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        break;
      }

      // mjr
      // ---------------------------------------------------------
      // Alert for automatic documents
      // ------------------------------------------------------------
      if (Equal(local.EventDetail.ExceptionRoutine, "AUTODOC"))
      {
        // mjr
        // ---------------------------------------------------------
        // Alert for automatic Locate documents
        // ------------------------------------------------------------
        if (Equal(local.Document.BusinessObject, "LOC"))
        {
          UseSpPrintDataRetrievalKeys2();

          if (!IsEmpty(local.SpDocKey.KeyPerson))
          {
            local.CsePersonsWorkSet.Number = local.SpDocKey.KeyPerson;
          }
          else if (!IsEmpty(local.SpDocKey.KeyAp))
          {
            local.CsePersonsWorkSet.Number = local.SpDocKey.KeyAp;
          }
          else
          {
            goto Test;
          }

          foreach(var item1 in ReadCase())
          {
            local.OfficeServiceProviderAlert.RecipientUserId = "";

            // mjr
            // ------------------------------------------
            // 05/08/2001
            // Send an alert to each OSP assigned to each Open ENF
            // Legal Requests relaed to this case
            // -------------------------------------------------------
            foreach(var item2 in ReadLegalReferral())
            {
              if (Equal(entities.LegalReferral.ReferralReason1, "ENF"))
              {
              }
              else if (Equal(entities.LegalReferral.ReferralReason2, "ENF"))
              {
              }
              else if (Equal(entities.LegalReferral.ReferralReason3, "ENF"))
              {
              }
              else if (Equal(entities.LegalReferral.ReferralReason4, "ENF"))
              {
              }
              else if (Equal(entities.LegalReferral.ReferralReason5, "ENF"))
              {
              }
              else
              {
                continue;
              }

              foreach(var item3 in ReadLegalReferralAssignment())
              {
                if (ReadOfficeServiceProviderServiceProviderOffice2())
                {
                  local.ServiceProvider.SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                  local.Office.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                  local.OfficeServiceProvider.RoleCode =
                    entities.OfficeServiceProvider.RoleCode;
                  local.OfficeServiceProvider.EffectiveDate =
                    entities.OfficeServiceProvider.EffectiveDate;
                  MoveOfficeServiceProviderAlert(local.Automatic,
                    local.OfficeServiceProviderAlert);
                  local.OfficeServiceProviderAlert.RecipientUserId =
                    entities.ServiceProvider.UserId;
                  UseSpCabCreateOfcSrvPrvdAlert();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // mjr
                    // -------------------------------------------------------------
                    // Reset exitstate to All_OK after it is changed
                    // ----------------------------------------------------------------
                    ExitState = "ACO_NN0000_ALL_OK";
                    local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                      (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
                      TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
                      (local.Infrastructure.UserId);
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";
                  }
                }
              }
            }

            if (IsEmpty(local.OfficeServiceProviderAlert.RecipientUserId))
            {
              // mjr
              // ----------------------------------------------------
              // No Legal Requests were found for this Case.
              // Send the alert to the Case worker
              // -------------------------------------------------------
              foreach(var item2 in ReadCaseAssignment())
              {
                if (ReadOfficeServiceProviderServiceProviderOffice1())
                {
                  local.ServiceProvider.SystemGeneratedId =
                    entities.ServiceProvider.SystemGeneratedId;
                  local.Office.SystemGeneratedId =
                    entities.Office.SystemGeneratedId;
                  local.OfficeServiceProvider.RoleCode =
                    entities.OfficeServiceProvider.RoleCode;
                  local.OfficeServiceProvider.EffectiveDate =
                    entities.OfficeServiceProvider.EffectiveDate;
                  MoveOfficeServiceProviderAlert(local.Automatic,
                    local.OfficeServiceProviderAlert);
                  local.OfficeServiceProviderAlert.RecipientUserId =
                    entities.ServiceProvider.UserId;
                  UseSpCabCreateOfcSrvPrvdAlert();

                  if (!IsExitState("ACO_NN0000_ALL_OK"))
                  {
                    // mjr
                    // -------------------------------------------------------------
                    // Reset exitstate to All_OK after it is changed
                    // ----------------------------------------------------------------
                    ExitState = "ACO_NN0000_ALL_OK";
                    local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
                      (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
                      TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
                      (local.Infrastructure.UserId);
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.EabReportSend.RptDetail = "";
                  }
                }
              }
            }
          }
        }
        else
        {
          UseSpPrintDataRetrievalKeys1();

          if (!Equal(local.Document.Name, "ARCLOS60") && !
            Equal(local.Document.Name, "NOTCCLOS"))
          {
            local.DateWorkArea.Date = local.Current.Date;
          }
          else
          {
            local.DateWorkArea.Date = local.Infrastructure.ReferenceDate;
          }

          UseSpDocGetServiceProvider();
          local.ServiceProvider.SystemGeneratedId =
            local.OutDocRtrnAddr.ServProvSysGenId;
          local.Office.SystemGeneratedId = local.OutDocRtrnAddr.OfficeSysGenId;
          local.OfficeServiceProvider.RoleCode =
            local.OutDocRtrnAddr.OspRoleCode;
          local.OfficeServiceProvider.EffectiveDate =
            local.OutDocRtrnAddr.OspEffectiveDate;
          MoveOfficeServiceProviderAlert(local.Automatic,
            local.OfficeServiceProviderAlert);
          local.OfficeServiceProviderAlert.RecipientUserId =
            local.OutDocRtrnAddr.ServProvUserId;
          UseSpCabCreateOfcSrvPrvdAlert();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // mjr
            // -------------------------------------------------------------
            // Reset exitstate to All_OK after it is changed
            // ----------------------------------------------------------------
            ExitState = "ACO_NN0000_ALL_OK";
            local.EabReportSend.RptDetail =
              "     Unable to send Alert to Service Provider for the previous error.";
              
            local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
              (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":       Unable to send Alert to Service Provider      -- Document Name = " +
              TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
              (local.Infrastructure.UserId);
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.EabReportSend.RptDetail = "";
          }
        }
      }

Test:

      ++local.DocsProcessed.Count;
      local.Totals.Update.GlocalTotalsProcessed.Count =
        local.Totals.Item.GlocalTotalsProcessed.Count + 1;

      if (AsChar(local.DebugOn.Flag) == 'Y')
      {
        local.EabConvertNumeric.SendAmount =
          NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
        UseEabConvertNumeric1();
        local.EabReportSend.RptDetail = "Infrastructure ID = " + TrimEnd
          (local.EabConvertNumeric.ReturnNoCommasInNonDecimal) + ":  Successfully Printed      -- Document Name = " +
          TrimEnd(local.Document.Name) + ", USERID = " + TrimEnd
          (local.Infrastructure.UserId);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        local.EabReportSend.RptDetail = "";
      }

      // mjr
      // -----------------------------------------------------
      // Added check for commit frequency
      // --------------------------------------------------------
      if (local.RowLockDocument.Count >= local
        .ProgramCheckpointRestart.ReadFrequencyCount.GetValueOrDefault() || local
        .RowLockFieldValue.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        UseExtToDoACommit();
        local.RowLockDocument.Count = 0;
        local.RowLockFieldValue.Count = 0;
      }

      // mjr
      // -------------------------------------------------
      // End READ EACH outgoing_document
      // ----------------------------------------------------
ReadEach1:
      ;
    }

ReadEach2:

    if (!IsEmpty(local.EabReportSend.RptDetail))
    {
      local.EabConvertNumeric.SendAmount =
        NumberToString(local.Infrastructure.SystemGeneratedIdentifier, 15);
      UseEabConvertNumeric1();
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; Inf ID = " +
        local.EabConvertNumeric.ReturnNoCommasInNonDecimal;
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      // mjr
      // ---------------------------------------------------------
      // Commit here, so errors with closing files will not require the job to 
      // be re-run.
      // ------------------------------------------------------------
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseExtToDoACommit();
      }
    }

    // ---------------------------------------------------------------
    // WRITE CONTROL TOTALS AND CLOSE REPORTS
    // ---------------------------------------------------------------
    UseSpB715WriteControlsAndClose();

    // ---------------------------------------------------------------
    // CLOSE ADABAS IF AVAILABLE
    // ---------------------------------------------------------------
    local.CsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    // mjr
    // ------------------------------------------------------------
    // Sleep for specified time
    // ---------------------------------------------------------------
    // 07/30/2008	JHuss	Removed Exit State If statement to allow restart in 
    // nearly all cases.
    if (Time(Now()) < local.Stoptime.Time)
    {
      if (local.SleepLength.TotalInteger > 0)
      {
        local.SleepSeconds.Text10 =
          NumberToString(local.SleepLength.TotalInteger * 60, 6, 10);
        UseSpEabWait();
      }

      ExitState = "ACO_NN0000_RESTART_PROCEDURE";
    }
    else
    {
      ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDocument1(Document source, Document target)
  {
    target.Name = source.Name;
    target.RequiredResponseDays = source.RequiredResponseDays;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveDocument2(Document source, Document target)
  {
    target.Name = source.Name;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveDocument3(Document source, Document target)
  {
    target.Name = source.Name;
    target.VersionNumber = source.VersionNumber;
  }

  private static void MoveDocumentDetails(SpB715Housekeeping.Export.
    DocumentDetailsGroup source, Local.DocumentDetailsGroup target)
  {
    target.GlocalDocumentDetailDocument.Assign(
      source.GexportDocumentDetailDocument);
    target.GlocalDocumentDetailEventDetail.ExceptionRoutine =
      source.GexportDocumentDetailEventDetail.ExceptionRoutine;
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

  private static void MoveExceptions(SpB715Housekeeping.Export.
    ExceptionsGroup source, Local.ExceptionsGroup target)
  {
    target.GlocalException.Name = source.GexportException.Name;
  }

  private static void MoveField(Field source, Field target)
  {
    target.Dependancy = source.Dependancy;
    target.SubroutineName = source.SubroutineName;
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveInfrastructure3(Infrastructure source,
    Infrastructure target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReferenceDate = source.ReferenceDate;
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveOfficeServiceProviderAlert(
    OfficeServiceProviderAlert source, OfficeServiceProviderAlert target)
  {
    target.TypeCode = source.TypeCode;
    target.Message = source.Message;
    target.Description = source.Description;
    target.DistributionDate = source.DistributionDate;
    target.SituationIdentifier = source.SituationIdentifier;
    target.PrioritizationCode = source.PrioritizationCode;
    target.OptimizationInd = source.OptimizationInd;
    target.OptimizedFlag = source.OptimizedFlag;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private static void MoveProgramCheckpointRestart(
    ProgramCheckpointRestart source, ProgramCheckpointRestart target)
  {
    target.UpdateFrequencyCount = source.UpdateFrequencyCount;
    target.ReadFrequencyCount = source.ReadFrequencyCount;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveSpDocKey(SpDocKey source, SpDocKey target)
  {
    target.KeyAdminAction = source.KeyAdminAction;
    target.KeyAdminActionCert = source.KeyAdminActionCert;
    target.KeyAdminAppeal = source.KeyAdminAppeal;
    target.KeyAp = source.KeyAp;
    target.KeyAppointment = source.KeyAppointment;
    target.KeyAr = source.KeyAr;
    target.KeyBankruptcy = source.KeyBankruptcy;
    target.KeyCase = source.KeyCase;
    target.KeyCashRcptDetail = source.KeyCashRcptDetail;
    target.KeyCashRcptEvent = source.KeyCashRcptEvent;
    target.KeyCashRcptSource = source.KeyCashRcptSource;
    target.KeyCashRcptType = source.KeyCashRcptType;
    target.KeyChild = source.KeyChild;
    target.KeyContact = source.KeyContact;
    target.KeyGeneticTest = source.KeyGeneticTest;
    target.KeyHealthInsCoverage = source.KeyHealthInsCoverage;
    target.KeyIncarceration = source.KeyIncarceration;
    target.KeyIncomeSource = source.KeyIncomeSource;
    target.KeyInfoRequest = source.KeyInfoRequest;
    target.KeyInterstateRequest = source.KeyInterstateRequest;
    target.KeyLegalAction = source.KeyLegalAction;
    target.KeyLegalActionDetail = source.KeyLegalActionDetail;
    target.KeyLegalReferral = source.KeyLegalReferral;
    target.KeyMilitaryService = source.KeyMilitaryService;
    target.KeyObligation = source.KeyObligation;
    target.KeyObligationAdminAction = source.KeyObligationAdminAction;
    target.KeyObligationType = source.KeyObligationType;
    target.KeyPerson = source.KeyPerson;
    target.KeyPersonAccount = source.KeyPersonAccount;
    target.KeyPersonAddress = source.KeyPersonAddress;
    target.KeyRecaptureRule = source.KeyRecaptureRule;
    target.KeyResource = source.KeyResource;
    target.KeyTribunal = source.KeyTribunal;
    target.KeyWorkerComp = source.KeyWorkerComp;
    target.KeyWorksheet = source.KeyWorksheet;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdAdminActCert = source.IdAdminActCert;
    target.IdDocument = source.IdDocument;
    target.IdHealthInsCoverage = source.IdHealthInsCoverage;
    target.IdPrNumber = source.IdPrNumber;
  }

  private static void MoveTotals(Local.TotalsGroup source,
    SpB715WriteControlsAndClose.Import.DocumentTotalsGroup target)
  {
    MoveDocument3(source.GlocalTotals, target.G);
    target.GimportDataError.Count = source.GlocalTotalsDataError.Count;
    target.GimportException.Count = source.GlocalTotalsException.Count;
    target.GimportFuture.Count = source.GlocalTotalsFuture.Count;
    target.GimportProcessed.Count = source.GlocalTotalsProcessed.Count;
    target.GimportRead.Count = source.GlocalTotalsRead.Count;
    target.GimportSystemError.Count = source.GlocalTotalsSystemError.Count;
    target.GimportWarning.Count = source.GlocalTotalsWarning.Count;
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

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;
    useExport.AbendData.Type1 = local.AbendData.Type1;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Type1 = useExport.AbendData.Type1;
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.External.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.External.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpB715Housekeeping()
  {
    var useImport = new SpB715Housekeeping.Import();
    var useExport = new SpB715Housekeeping.Export();

    Call(SpB715Housekeeping.Execute, useImport, useExport);

    local.Automatic.Assign(useExport.Automatic);
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    MoveOfficeServiceProviderAlert(useExport.Monitored, local.Monitored);
    MoveProgramCheckpointRestart(useExport.ProgramCheckpointRestart,
      local.ProgramCheckpointRestart);
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.SleepLength.TotalInteger = useExport.SleepLength.TotalInteger;
    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
    local.Stoptime.Time = useExport.Stoptime.Time;
    MoveOfficeServiceProviderAlert(useExport.UnMonitored, local.UnMonitored);
    useExport.DocumentDetails.
      CopyTo(local.DocumentDetails, MoveDocumentDetails);
    useExport.Exceptions.CopyTo(local.Exceptions, MoveExceptions);
  }

  private void UseSpB715WriteControlsAndClose()
  {
    var useImport = new SpB715WriteControlsAndClose.Import();
    var useExport = new SpB715WriteControlsAndClose.Export();

    useImport.DocsDataError.Count = local.DocsDataError.Count;
    useImport.DocsException.Count = local.DocsException.Count;
    useImport.DocsProcessed.Count = local.DocsProcessed.Count;
    useImport.DocsRead.Count = local.DocsRead.Count;
    useImport.DocsWarning.Count = local.DocsWarning.Count;
    useImport.DocsSystemError.Count = local.DocsSystemError.Count;
    useImport.DocsUnprocessedFuture.Count = local.DocsFuture.Count;
    local.Totals.CopyTo(useImport.DocumentTotals, MoveTotals);

    Call(SpB715WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.Office.SystemGeneratedId = local.Office.SystemGeneratedId;
    MoveOfficeServiceProvider(local.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.OfficeServiceProviderAlert.
      Assign(local.OfficeServiceProviderAlert);
    useImport.ServiceProvider.SystemGeneratedId =
      local.ServiceProvider.SystemGeneratedId;

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpDocGetServiceProvider()
  {
    var useImport = new SpDocGetServiceProvider.Import();
    var useExport = new SpDocGetServiceProvider.Export();

    useImport.Current.Date = local.DateWorkArea.Date;
    useImport.Document.BusinessObject = local.Document.BusinessObject;
    MoveSpDocKey(local.SpDocKey, useImport.SpDocKey);

    Call(SpDocGetServiceProvider.Execute, useImport, useExport);

    local.OutDocRtrnAddr.Assign(useExport.OutDocRtrnAddr);
  }

  private void UseSpDocUpdateFailedBatchDoc()
  {
    var useImport = new SpDocUpdateFailedBatchDoc.Import();
    var useExport = new SpDocUpdateFailedBatchDoc.Export();

    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.Document.Assign(local.Document);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Message.Text50 = local.WorkArea.Text50;
    useImport.Monitored.Assign(local.Monitored);
    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;
    useImport.UnMonitored.Assign(local.UnMonitored);

    Call(SpDocUpdateFailedBatchDoc.Execute, useImport, useExport);
  }

  private void UseSpDocUpdateSuccessfulPrint()
  {
    var useImport = new SpDocUpdateSuccessfulPrint.Import();
    var useExport = new SpDocUpdateSuccessfulPrint.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);

    Call(SpDocUpdateSuccessfulPrint.Execute, useImport, useExport);
  }

  private void UseSpEabWait()
  {
    var useImport = new SpEabWait.Import();
    var useExport = new SpEabWait.Export();

    useImport.WaitSeconds.Text10 = local.SleepSeconds.Text10;

    Call(SpEabWait.Execute, useImport, useExport);
  }

  private void UseSpPrintDataRetrievalKeys1()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    useImport.Document.Assign(local.Document);
    MoveField(local.Field, useImport.Field);
    MoveInfrastructure3(local.Infrastructure, useImport.Infrastructure);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpPrintDataRetrievalKeys2()
  {
    var useImport = new SpPrintDataRetrievalKeys.Import();
    var useExport = new SpPrintDataRetrievalKeys.Export();

    useImport.Document.Assign(local.Document);
    MoveField(local.Field, useImport.Field);
    MoveInfrastructure3(local.Infrastructure, useImport.Infrastructure);

    Call(SpPrintDataRetrievalKeys.Execute, useImport, useExport);

    local.SpDocKey.Assign(useExport.SpDocKey);
  }

  private void UseSpPrintDataRetrievalMain()
  {
    var useImport = new SpPrintDataRetrievalMain.Import();
    var useExport = new SpPrintDataRetrievalMain.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      entities.Infrastructure.SystemGeneratedIdentifier;
    MoveDocument2(local.Document, useImport.Document);
    MoveProgramProcessingInfo(local.ProgramProcessingInfo, useImport.Batch);
    useImport.ExpImpRowLockFieldValue.Count = local.RowLockFieldValue.Count;

    Call(SpPrintDataRetrievalMain.Execute, useImport, useExport);

    local.RowLockFieldValue.Count = useImport.ExpImpRowLockFieldValue.Count;
    local.ErrorDocumentField.ScreenPrompt =
      useExport.ErrorDocumentField.ScreenPrompt;
    local.ErrorFieldValue.Value = useExport.ErrorFieldValue.Value;
    local.ErrorInd.Flag = useExport.ErrorInd.Flag;
    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSpPrintDecodeReturnCode()
  {
    var useImport = new SpPrintDecodeReturnCode.Import();
    var useExport = new SpPrintDecodeReturnCode.Export();

    useImport.WorkArea.Text50 = local.WorkArea.Text50;

    Call(SpPrintDecodeReturnCode.Execute, useImport, useExport);
  }

  private void UseUpdateOutgoingDocument()
  {
    var useImport = new UpdateOutgoingDocument.Import();
    var useExport = new UpdateOutgoingDocument.Export();

    useImport.Infrastructure.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OutgoingDocument.Assign(local.OutgoingDocument);

    Call(UpdateOutgoingDocument.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "statusDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseAssignment()
  {
    entities.CaseAssignment.Populated = false;

    return ReadEach("ReadCaseAssignment",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseAssignment.ReasonCode = db.GetString(reader, 0);
        entities.CaseAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.CaseAssignment.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.CaseAssignment.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.CaseAssignment.SpdId = db.GetInt32(reader, 4);
        entities.CaseAssignment.OffId = db.GetInt32(reader, 5);
        entities.CaseAssignment.OspCode = db.GetString(reader, 6);
        entities.CaseAssignment.OspDate = db.GetDate(reader, 7);
        entities.CaseAssignment.CasNo = db.GetString(reader, 8);
        entities.CaseAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadDocument()
  {
    entities.Detail.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetString(command, "name", entities.Document.Name);
        db.SetDate(
          command, "effectiveDate",
          entities.Document.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Detail.Name = db.GetString(reader, 0);
        entities.Detail.RequiredResponseDays = db.GetInt32(reader, 1);
        entities.Detail.EffectiveDate = db.GetDate(reader, 2);
        entities.Detail.Populated = true;
      });
  }

  private bool ReadDocumentField1()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField1",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;
      });
  }

  private IEnumerable<bool> ReadDocumentField2()
  {
    entities.DocumentField.Populated = false;

    return ReadEach("ReadDocumentField2",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.Document.EffectiveDate.GetValueOrDefault());
        db.SetString(command, "docName", entities.Document.Name);
      },
      (db, reader) =>
      {
        entities.DocumentField.RequiredSwitch = db.GetString(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;

        return true;
      });
  }

  private bool ReadField()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    entities.Field.Populated = false;

    return Read("ReadField",
      (db, command) =>
      {
        db.SetString(command, "name", entities.DocumentField.FldName);
      },
      (db, reader) =>
      {
        entities.Field.Name = db.GetString(reader, 0);
        entities.Field.Dependancy = db.GetString(reader, 1);
        entities.Field.SubroutineName = db.GetString(reader, 2);
        entities.Field.Populated = true;
      });
  }

  private bool ReadFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.FieldValue.Populated = false;

    return Read("ReadFieldValue",
      (db, command) =>
      {
        db.SetDate(
          command, "docEffectiveDte",
          entities.DocumentField.DocEffectiveDte.GetValueOrDefault());
        db.SetString(command, "docName", entities.DocumentField.DocName);
        db.SetString(command, "fldName", entities.DocumentField.FldName);
        db.SetInt32(command, "infIdentifier", entities.OutgoingDocument.InfId);
      },
      (db, reader) =>
      {
        entities.FieldValue.Value = db.GetNullableString(reader, 0);
        entities.FieldValue.FldName = db.GetString(reader, 1);
        entities.FieldValue.DocName = db.GetString(reader, 2);
        entities.FieldValue.DocEffectiveDte = db.GetDate(reader, 3);
        entities.FieldValue.InfIdentifier = db.GetInt32(reader, 4);
        entities.FieldValue.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.
          SetInt32(command, "systemGeneratedI", entities.OutgoingDocument.InfId);
          
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.ReferenceDate = db.GetNullableDate(reader, 1);
        entities.Infrastructure.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return ReadEach("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "statusDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.StatusDate = db.GetNullableDate(reader, 2);
        entities.LegalReferral.Status = db.GetNullableString(reader, 3);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 7);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 8);
        entities.LegalReferral.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalReferralAssignment()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferral.Populated);
    entities.LegalReferralAssignment.Populated = false;

    return ReadEach("ReadLegalReferralAssignment",
      (db, command) =>
      {
        db.SetInt32(command, "lgrId", entities.LegalReferral.Identifier);
        db.SetString(command, "casNo", entities.LegalReferral.CasNumber);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.LegalReferralAssignment.ReasonCode = db.GetString(reader, 0);
        entities.LegalReferralAssignment.EffectiveDate = db.GetDate(reader, 1);
        entities.LegalReferralAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.LegalReferralAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 3);
        entities.LegalReferralAssignment.SpdId = db.GetInt32(reader, 4);
        entities.LegalReferralAssignment.OffId = db.GetInt32(reader, 5);
        entities.LegalReferralAssignment.OspCode = db.GetString(reader, 6);
        entities.LegalReferralAssignment.OspDate = db.GetDate(reader, 7);
        entities.LegalReferralAssignment.CasNo = db.GetString(reader, 8);
        entities.LegalReferralAssignment.LgrId = db.GetInt32(reader, 9);
        entities.LegalReferralAssignment.Populated = true;

        return true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseAssignment.Populated);
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice1",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.CaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(command, "roleCode", entities.CaseAssignment.OspCode);
        db.SetInt32(command, "offGeneratedId", entities.CaseAssignment.OffId);
        db.SetInt32(command, "spdGeneratedId", entities.CaseAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 6);
        entities.Office.EffectiveDate = db.GetDate(reader, 7);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalReferralAssignment.Populated);
    entities.Office.Populated = false;
    entities.OfficeServiceProvider.Populated = false;
    entities.ServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          entities.LegalReferralAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.LegalReferralAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.LegalReferralAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.LegalReferralAssignment.SpdId);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 6);
        entities.Office.EffectiveDate = db.GetDate(reader, 7);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 8);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 9);
        entities.Office.Populated = true;
        entities.OfficeServiceProvider.Populated = true;
        entities.ServiceProvider.Populated = true;
      });
  }

  private IEnumerable<bool> ReadOutgoingDocumentDocument()
  {
    entities.Document.Populated = false;
    entities.OutgoingDocument.Populated = false;

    return ReadEach("ReadOutgoingDocumentDocument",
      null,
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.Document.Name = db.GetString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.Document.EffectiveDate = db.GetDate(reader, 3);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 4);
        entities.Document.Populated = true;
        entities.OutgoingDocument.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", local.Infrastructure.UserId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.EmailAddress = db.GetNullableString(reader, 2);
        entities.ServiceProvider.Populated = true;
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
    /// <summary>A DocumentDetailsGroup group.</summary>
    [Serializable]
    public class DocumentDetailsGroup
    {
      /// <summary>
      /// A value of GlocalDocumentDetailDocument.
      /// </summary>
      [JsonPropertyName("glocalDocumentDetailDocument")]
      public Document GlocalDocumentDetailDocument
      {
        get => glocalDocumentDetailDocument ??= new();
        set => glocalDocumentDetailDocument = value;
      }

      /// <summary>
      /// A value of GlocalDocumentDetailEventDetail.
      /// </summary>
      [JsonPropertyName("glocalDocumentDetailEventDetail")]
      public EventDetail GlocalDocumentDetailEventDetail
      {
        get => glocalDocumentDetailEventDetail ??= new();
        set => glocalDocumentDetailEventDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalDocumentDetailDocument;
      private EventDetail glocalDocumentDetailEventDetail;
    }

    /// <summary>A ExceptionsGroup group.</summary>
    [Serializable]
    public class ExceptionsGroup
    {
      /// <summary>
      /// A value of GlocalException.
      /// </summary>
      [JsonPropertyName("glocalException")]
      public Document GlocalException
      {
        get => glocalException ??= new();
        set => glocalException = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalException;
    }

    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of GlocalTotals.
      /// </summary>
      [JsonPropertyName("glocalTotals")]
      public Document GlocalTotals
      {
        get => glocalTotals ??= new();
        set => glocalTotals = value;
      }

      /// <summary>
      /// A value of GlocalTotalsDataError.
      /// </summary>
      [JsonPropertyName("glocalTotalsDataError")]
      public Common GlocalTotalsDataError
      {
        get => glocalTotalsDataError ??= new();
        set => glocalTotalsDataError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsException.
      /// </summary>
      [JsonPropertyName("glocalTotalsException")]
      public Common GlocalTotalsException
      {
        get => glocalTotalsException ??= new();
        set => glocalTotalsException = value;
      }

      /// <summary>
      /// A value of GlocalTotalsFuture.
      /// </summary>
      [JsonPropertyName("glocalTotalsFuture")]
      public Common GlocalTotalsFuture
      {
        get => glocalTotalsFuture ??= new();
        set => glocalTotalsFuture = value;
      }

      /// <summary>
      /// A value of GlocalTotalsProcessed.
      /// </summary>
      [JsonPropertyName("glocalTotalsProcessed")]
      public Common GlocalTotalsProcessed
      {
        get => glocalTotalsProcessed ??= new();
        set => glocalTotalsProcessed = value;
      }

      /// <summary>
      /// A value of GlocalTotalsRead.
      /// </summary>
      [JsonPropertyName("glocalTotalsRead")]
      public Common GlocalTotalsRead
      {
        get => glocalTotalsRead ??= new();
        set => glocalTotalsRead = value;
      }

      /// <summary>
      /// A value of GlocalTotalsSystemError.
      /// </summary>
      [JsonPropertyName("glocalTotalsSystemError")]
      public Common GlocalTotalsSystemError
      {
        get => glocalTotalsSystemError ??= new();
        set => glocalTotalsSystemError = value;
      }

      /// <summary>
      /// A value of GlocalTotalsWarning.
      /// </summary>
      [JsonPropertyName("glocalTotalsWarning")]
      public Common GlocalTotalsWarning
      {
        get => glocalTotalsWarning ??= new();
        set => glocalTotalsWarning = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Document glocalTotals;
      private Common glocalTotalsDataError;
      private Common glocalTotalsException;
      private Common glocalTotalsFuture;
      private Common glocalTotalsProcessed;
      private Common glocalTotalsRead;
      private Common glocalTotalsSystemError;
      private Common glocalTotalsWarning;
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
    /// A value of Automatic.
    /// </summary>
    [JsonPropertyName("automatic")]
    public OfficeServiceProviderAlert Automatic
    {
      get => automatic ??= new();
      set => automatic = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Cutoff.
    /// </summary>
    [JsonPropertyName("cutoff")]
    public OutgoingDocument Cutoff
    {
      get => cutoff ??= new();
      set => cutoff = value;
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
    /// A value of DebugOn.
    /// </summary>
    [JsonPropertyName("debugOn")]
    public Common DebugOn
    {
      get => debugOn ??= new();
      set => debugOn = value;
    }

    /// <summary>
    /// A value of DocsDataError.
    /// </summary>
    [JsonPropertyName("docsDataError")]
    public Common DocsDataError
    {
      get => docsDataError ??= new();
      set => docsDataError = value;
    }

    /// <summary>
    /// A value of DocsException.
    /// </summary>
    [JsonPropertyName("docsException")]
    public Common DocsException
    {
      get => docsException ??= new();
      set => docsException = value;
    }

    /// <summary>
    /// A value of DocsFuture.
    /// </summary>
    [JsonPropertyName("docsFuture")]
    public Common DocsFuture
    {
      get => docsFuture ??= new();
      set => docsFuture = value;
    }

    /// <summary>
    /// A value of DocsProcessed.
    /// </summary>
    [JsonPropertyName("docsProcessed")]
    public Common DocsProcessed
    {
      get => docsProcessed ??= new();
      set => docsProcessed = value;
    }

    /// <summary>
    /// A value of DocsRead.
    /// </summary>
    [JsonPropertyName("docsRead")]
    public Common DocsRead
    {
      get => docsRead ??= new();
      set => docsRead = value;
    }

    /// <summary>
    /// A value of DocsSystemError.
    /// </summary>
    [JsonPropertyName("docsSystemError")]
    public Common DocsSystemError
    {
      get => docsSystemError ??= new();
      set => docsSystemError = value;
    }

    /// <summary>
    /// A value of DocsWarning.
    /// </summary>
    [JsonPropertyName("docsWarning")]
    public Common DocsWarning
    {
      get => docsWarning ??= new();
      set => docsWarning = value;
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
    /// A value of EabConvertNumeric.
    /// </summary>
    [JsonPropertyName("eabConvertNumeric")]
    public EabConvertNumeric2 EabConvertNumeric
    {
      get => eabConvertNumeric ??= new();
      set => eabConvertNumeric = value;
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

    /// <summary>
    /// A value of ErrorInd.
    /// </summary>
    [JsonPropertyName("errorInd")]
    public Common ErrorInd
    {
      get => errorInd ??= new();
      set => errorInd = value;
    }

    /// <summary>
    /// A value of EventDetail.
    /// </summary>
    [JsonPropertyName("eventDetail")]
    public EventDetail EventDetail
    {
      get => eventDetail ??= new();
      set => eventDetail = value;
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of Monitored.
    /// </summary>
    [JsonPropertyName("monitored")]
    public OfficeServiceProviderAlert Monitored
    {
      get => monitored ??= new();
      set => monitored = value;
    }

    /// <summary>
    /// A value of NullDocument.
    /// </summary>
    [JsonPropertyName("nullDocument")]
    public Document NullDocument
    {
      get => nullDocument ??= new();
      set => nullDocument = value;
    }

    /// <summary>
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
    }

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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of OutDocRtrnAddr.
    /// </summary>
    [JsonPropertyName("outDocRtrnAddr")]
    public OutDocRtrnAddr OutDocRtrnAddr
    {
      get => outDocRtrnAddr ??= new();
      set => outDocRtrnAddr = value;
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
    /// A value of RequiredFieldMissing.
    /// </summary>
    [JsonPropertyName("requiredFieldMissing")]
    public Common RequiredFieldMissing
    {
      get => requiredFieldMissing ??= new();
      set => requiredFieldMissing = value;
    }

    /// <summary>
    /// A value of RowLockDocument.
    /// </summary>
    [JsonPropertyName("rowLockDocument")]
    public Common RowLockDocument
    {
      get => rowLockDocument ??= new();
      set => rowLockDocument = value;
    }

    /// <summary>
    /// A value of RowLockFieldValue.
    /// </summary>
    [JsonPropertyName("rowLockFieldValue")]
    public Common RowLockFieldValue
    {
      get => rowLockFieldValue ??= new();
      set => rowLockFieldValue = value;
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
    /// A value of SleepLength.
    /// </summary>
    [JsonPropertyName("sleepLength")]
    public Common SleepLength
    {
      get => sleepLength ??= new();
      set => sleepLength = value;
    }

    /// <summary>
    /// A value of SleepSeconds.
    /// </summary>
    [JsonPropertyName("sleepSeconds")]
    public TextWorkArea SleepSeconds
    {
      get => sleepSeconds ??= new();
      set => sleepSeconds = value;
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
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
    }

    /// <summary>
    /// A value of Stoptime.
    /// </summary>
    [JsonPropertyName("stoptime")]
    public DateWorkArea Stoptime
    {
      get => stoptime ??= new();
      set => stoptime = value;
    }

    /// <summary>
    /// A value of UnMonitored.
    /// </summary>
    [JsonPropertyName("unMonitored")]
    public OfficeServiceProviderAlert UnMonitored
    {
      get => unMonitored ??= new();
      set => unMonitored = value;
    }

    /// <summary>
    /// A value of UserinputField.
    /// </summary>
    [JsonPropertyName("userinputField")]
    public Common UserinputField
    {
      get => userinputField ??= new();
      set => userinputField = value;
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
    /// Gets a value of DocumentDetails.
    /// </summary>
    [JsonIgnore]
    public Array<DocumentDetailsGroup> DocumentDetails =>
      documentDetails ??= new(DocumentDetailsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DocumentDetails for json serialization.
    /// </summary>
    [JsonPropertyName("documentDetails")]
    [Computed]
    public IList<DocumentDetailsGroup> DocumentDetails_Json
    {
      get => documentDetails;
      set => DocumentDetails.Assign(value);
    }

    /// <summary>
    /// Gets a value of Exceptions.
    /// </summary>
    [JsonIgnore]
    public Array<ExceptionsGroup> Exceptions => exceptions ??= new(
      ExceptionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Exceptions for json serialization.
    /// </summary>
    [JsonPropertyName("exceptions")]
    [Computed]
    public IList<ExceptionsGroup> Exceptions_Json
    {
      get => exceptions;
      set => Exceptions.Assign(value);
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonIgnore]
    public Array<TotalsGroup> Totals => totals ??= new(TotalsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Totals for json serialization.
    /// </summary>
    [JsonPropertyName("totals")]
    [Computed]
    public IList<TotalsGroup> Totals_Json
    {
      get => totals;
      set => Totals.Assign(value);
    }

    private AbendData abendData;
    private OfficeServiceProviderAlert automatic;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private OutgoingDocument cutoff;
    private DateWorkArea dateWorkArea;
    private Common debugOn;
    private Common docsDataError;
    private Common docsException;
    private Common docsFuture;
    private Common docsProcessed;
    private Common docsRead;
    private Common docsSystemError;
    private Common docsWarning;
    private Document document;
    private EabConvertNumeric2 eabConvertNumeric;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private DocumentField errorDocumentField;
    private FieldValue errorFieldValue;
    private Common errorInd;
    private EventDetail eventDetail;
    private ExitStateWorkArea exitStateWorkArea;
    private External external;
    private Field field;
    private FieldValue fieldValue;
    private Infrastructure infrastructure;
    private OfficeServiceProviderAlert monitored;
    private Document nullDocument;
    private Infrastructure nullInfrastructure;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OutDocRtrnAddr outDocRtrnAddr;
    private OutgoingDocument outgoingDocument;
    private ProgramCheckpointRestart programCheckpointRestart;
    private ProgramProcessingInfo programProcessingInfo;
    private Common requiredFieldMissing;
    private Common rowLockDocument;
    private Common rowLockFieldValue;
    private ServiceProvider serviceProvider;
    private Common sleepLength;
    private TextWorkArea sleepSeconds;
    private SpDocKey spDocKey;
    private SpDocLiteral spDocLiteral;
    private DateWorkArea stoptime;
    private OfficeServiceProviderAlert unMonitored;
    private Common userinputField;
    private WorkArea workArea;
    private Array<DocumentDetailsGroup> documentDetails;
    private Array<ExceptionsGroup> exceptions;
    private Array<TotalsGroup> totals;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Detail.
    /// </summary>
    [JsonPropertyName("detail")]
    public Document Detail
    {
      get => detail ??= new();
      set => detail = value;
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
    /// A value of DocumentField.
    /// </summary>
    [JsonPropertyName("documentField")]
    public DocumentField DocumentField
    {
      get => documentField ??= new();
      set => documentField = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

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
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
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
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private Case1 case1;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private Document detail;
    private Document document;
    private DocumentField documentField;
    private Field field;
    private FieldValue fieldValue;
    private Infrastructure infrastructure;
    private LegalReferral legalReferral;
    private LegalReferralAssignment legalReferralAssignment;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private OutgoingDocument outgoingDocument;
    private ServiceProvider serviceProvider;
  }
#endregion
}

// Program: SP_B309_ARC_RETRIEV_FIELD_VALUES, ID: 372952419, model: 746.
// Short name: SWEP309B
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
/// A program: SP_B309_ARC_RETRIEV_FIELD_VALUES.
/// </para>
/// <para>
/// Retrieve FIELD_VALUE records from external file on request basis
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class SpB309ArcRetrievFieldValues: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B309_ARC_RETRIEV_FIELD_VALUES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB309ArcRetrievFieldValues(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB309ArcRetrievFieldValues.
  /// </summary>
  public SpB309ArcRetrievFieldValues(IContext context, Import import,
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
    // ------------------------------------------------------------------------
    // Retrieve FIELD_VALUE records from external file on request basis
    // ------------------------------------------------------------------------
    // DATE		Developer	Request		Description
    // ------------------------------------------------------------------------
    // 11/10/1999     	Srini Ganji			Initial Creation
    // 09/01/2000	M Ramirez	102424		Error occurs when ' KEY' field
    // 						already exists
    // 09/01/2000	M Ramirez			Added Housekeeping and Control
    // 						Total CABs
    // 09/01/2000	M Ramirez			Changed READ properties to
    // 						'Select Only' where appropriate
    // ------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseSpB309Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.TotalInfraRecsCreated.Count = 1;
    local.EabFileHandling.Action = "WRITE";

    // *****************************************************************
    // Read Each Retrieve_field_value_treigger table ascending by Archive date 
    // and Inf_id
    // *****************************************************************
    foreach(var item in ReadRetrieveFieldValueTrigger())
    {
      ++local.TotalTriggerRecsReads.Count;
      MoveRetrieveFieldValueTrigger(entities.RetrieveFieldValueTrigger,
        local.RetrieveFieldValueTrigger);
      local.OfficeServiceProviderAlert.Assign(local.Null1);
      local.Document.Name = "";
      local.Error.ActionEntry = "";
      local.Found.Flag = "N";

      // *****************************************************************
      // Used ( Program Processing info name != Spaces) to make While as 
      // Infinite Loop, to read multiple Field Value records for Outgoing
      // Document
      // *****************************************************************
      while(!IsEmpty(local.ProgramProcessingInfo.Name))
      {
        if (AsChar(local.EndOfFile.Flag) == 'Y')
        {
          local.Error.ActionEntry = "01";

          break;
        }

        // *****************************************************************
        // check for previously read record from External file
        // *****************************************************************
        if (local.Previous.InfId > 0)
        {
          if (Equal(local.Previous.ArchiveDate,
            entities.RetrieveFieldValueTrigger.ArchiveDate))
          {
            if (local.Previous.InfId == entities
              .RetrieveFieldValueTrigger.InfId)
            {
              // *****************************************************************
              // Record found - Create Field Value record
              // *****************************************************************
            }
            else if (local.Previous.InfId < entities
              .RetrieveFieldValueTrigger.InfId)
            {
              // *****************************************************************
              // Record not found - Error
              // *****************************************************************
              local.Error.ActionEntry = "01";

              break;
            }
            else
            {
              // *****************************************************************
              // Read next record from External File
              // *****************************************************************
              local.Previous.Assign(local.Blank);
            }
          }
          else if (Lt(local.Previous.ArchiveDate,
            entities.RetrieveFieldValueTrigger.ArchiveDate))
          {
            // *****************************************************************
            // Record not found - Error
            // *****************************************************************
            local.Error.ActionEntry = "01";

            break;
          }
          else
          {
            // *****************************************************************
            // Read next record from External File
            // *****************************************************************
            local.Previous.Assign(local.Blank);
          }
        }

        if (local.Previous.InfId == 0)
        {
          local.Previous.Assign(local.Blank);
          local.WsFieldValues.Assign(local.Blank);
          local.PassArea.FileInstruction = "READ";
          UseSpEabRetrieveFieldValues();

          if (Equal(local.PassArea.TextReturnCode, "EF"))
          {
            // *****************************************************************
            // End of Input File
            // *****************************************************************
            local.EndOfFile.Flag = "Y";

            if (AsChar(local.Found.Flag) == 'Y')
            {
              // *****************************************************************
              // Completed reading Field Value records for Read Trigger record, 
              // Update Outgoing Document.
              // *****************************************************************
              break;
            }
            else
            {
              // *****************************************************************
              // End of Input File, Record not found - Error
              // *****************************************************************
              local.Error.ActionEntry = "01";

              break;
            }
          }

          // *****************************************************************
          // Check for other errros
          // *****************************************************************
          if (!IsEmpty(local.PassArea.TextReturnCode))
          {
            local.EabReportSend.RptDetail = "Error Reading External File : " + NumberToString
              (local.PassArea.NumericReturnCode, 14, 2);
            local.EabFileHandling.Action = "WRITE";
            UseCabErrorReport();
            ExitState = "ERROR_READING_FILE_AB";

            return;
          }

          ++local.TotalRecsReadFrmArcFl.Count;

          if (Equal(local.WsFieldValues.ArchiveDate,
            entities.RetrieveFieldValueTrigger.ArchiveDate))
          {
            if (local.WsFieldValues.InfId == entities
              .RetrieveFieldValueTrigger.InfId)
            {
              // *****************************************************************
              // Record found - Create Field Value record
              // *****************************************************************
            }
            else if (local.WsFieldValues.InfId < entities
              .RetrieveFieldValueTrigger.InfId)
            {
              local.Previous.Assign(local.WsFieldValues);

              if (AsChar(local.Found.Flag) == 'Y')
              {
                // *****************************************************************
                // Completed reading Field Value reocrds for Read Trigger 
                // record, Update Outgoing Document.
                // *****************************************************************
                break;
              }
              else
              {
                // *****************************************************************
                // Record not found - Error
                // *****************************************************************
                local.Error.ActionEntry = "01";

                break;
              }
            }
            else
            {
              // *****************************************************************
              // Read next record from External file.
              // *****************************************************************
              continue;
            }
          }
          else if (Lt(local.WsFieldValues.ArchiveDate,
            entities.RetrieveFieldValueTrigger.ArchiveDate))
          {
            local.Previous.Assign(local.WsFieldValues);

            if (AsChar(local.Found.Flag) == 'Y')
            {
              // *****************************************************************
              // Completed reading Field Value reocrds for Read Trigger record, 
              // Update Outgoing Document.
              // *****************************************************************
              break;
            }
            else
            {
              // *****************************************************************
              // Record not found - Error
              // *****************************************************************
              local.Error.ActionEntry = "01";

              break;
            }
          }
          else
          {
            // *****************************************************************
            // Read next record from External file.
            // *****************************************************************
            continue;
          }
        }

        // *****************************************************************
        // On successful read, create Field_Value records and at end update 
        // Outgoing_Document Archive Flag to 'N'
        // *****************************************************************
        local.Document.Name = local.WsFieldValues.DocName;

        if (ReadOutgoingDocument1())
        {
          local.OutDoc.InfId = local.WsFieldValues.InfId;

          if (ReadDocumentField())
          {
            try
            {
              CreateFieldValue();
              ++local.TotalFieldValuesCreated.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  // mjr
                  // ----------------------------------------
                  // 09/01/2000
                  // PR# 102424 - Error occurs when ' KEY' field already exists.
                  // Write a warning instead of an error.
                  // -----------------------------------------------------
                  if (AsChar(local.DebugOn.Flag) == 'Y')
                  {
                    local.EabFileHandling.Action = "WRITE";
                    local.TextDate.TextDate =
                      NumberToString(DateToInt(
                        entities.RetrieveFieldValueTrigger.ArchiveDate), 8, 8);
                    local.Temp.RptDetail = local.TextDate.TextDate + "            " +
                      NumberToString
                      (entities.RetrieveFieldValueTrigger.InfId, 7, 9);
                    local.EabReportSend.RptDetail =
                      Substring(local.Temp.RptDetail,
                      EabReportSend.RptDetail_MaxLength, 1, 31) + "     " + "Field Value Already Exists";
                      
                    UseCabErrorReport();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                      return;
                    }

                    local.TextDate.TextDate = "";
                    local.Temp.RptDetail = "";
                    local.EabReportSend.RptDetail = "";
                  }

                  break;
                case ErrorCode.PermittedValueViolation:
                  local.Error.ActionEntry = "03";

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
            local.Error.ActionEntry = "04";

            break;
          }
        }
        else
        {
          local.Error.ActionEntry = "05";

          break;
        }

        local.Found.Flag = "Y";
        local.Previous.Assign(local.Blank);
      }

      if (IsEmpty(local.Error.ActionEntry))
      {
        // *****************************************************************
        // No Errors so far, completed reading Feild Values from External File, 
        // Update Outgoing Document
        // *****************************************************************
        if (ReadOutgoingDocument2())
        {
          try
          {
            UpdateOutgoingDocument();
            ++local.Commit.Count;
            ++local.TotalSucflRetrieveDocs.Count;
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                local.Error.ActionEntry = "06";

                break;
              case ErrorCode.PermittedValueViolation:
                local.Error.ActionEntry = "07";

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
          local.Error.ActionEntry = "05";
        }
      }
      else
      {
        ++local.TotalUnsuccRetrievDocs.Count;
      }

      // *****************************************************************
      // Create an alert for Service Provider after
      // successful/unsuccessful retrieval of Field Values.
      // *****************************************************************
      if (IsEmpty(local.Error.ActionEntry))
      {
        // *****************************************************************
        // Create successful alert for Service Provider
        // *****************************************************************
        local.OfficeServiceProviderAlert.Assign(local.Local416);
      }
      else
      {
        // *****************************************************************
        // Create un-successful alert for Service Provider
        // *****************************************************************
        local.OfficeServiceProviderAlert.Assign(local.Local417);
      }

      // *****************************************************************
      // Read Service Provider for Logon ID
      // *****************************************************************
      // mjr
      // ---------------------------------------------------------
      // 09/01/2000
      // Removed sending an Alert if the User that created the trigger is 
      // SWEPB706
      // ----------------------------------------------------------------------
      if (!Equal(local.RetrieveFieldValueTrigger.ServiceProviderLogonId,
        "SWEPB706"))
      {
        if (ReadServiceProvider())
        {
          // *****************************************************************
          // Read for Office Service Provider
          // *****************************************************************
          local.Office.SystemGeneratedId = 0;

          if (ReadOfficeServiceProviderOffice())
          {
            local.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
          }

          if (local.Office.SystemGeneratedId == 0)
          {
            // mjr
            // ---------------------------------------------------------
            // 09/01/2000
            // Removed setting Error Flag.  There may have already been an 
            // error,
            // and setting it now will override that message.
            // Added writing it to the Error Report
            // ----------------------------------------------------------------------
            local.TextDate.TextDate =
              NumberToString(DateToInt(
                entities.RetrieveFieldValueTrigger.ArchiveDate), 8, 8);
            local.Temp.RptDetail = local.TextDate.TextDate + "            " + NumberToString
              (entities.RetrieveFieldValueTrigger.InfId, 7, 9);
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Office service provider not found; User ID = " +
              local.RetrieveFieldValueTrigger.ServiceProviderLogonId;
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

              return;
            }

            local.TextDate.TextDate = "";
            local.Temp.RptDetail = "";
            local.EabReportSend.RptDetail = "";

            goto Test;
          }

          if (IsEmpty(local.Document.Name))
          {
            // *****************************************************************
            // Get Document name , to write on alert Description
            // *****************************************************************
            // mjr
            // ------------------------------------------
            // 09/01/2000
            // Removed Outgoing document from the READ since it is not used
            // -------------------------------------------------------
            if (ReadDocument())
            {
              local.Document.Name = entities.Document.Name;
            }
            else
            {
              local.Document.Name = "NOTFOUND";
            }
          }

          // *****************************************************************
          // Create only one alert for Service Provider in each run
          // *****************************************************************
          if (ReadOfficeServiceProviderAlert())
          {
            // *****************************************************************
            // Alert already created for this Service provider in this run.
            // So update Office Service Provider alert description with
            // retrieved document name if current document type different
            // from previously retrieved.
            // *****************************************************************
            if (Find(String(
              entities.OfficeServiceProviderAlert.Description,
              OfficeServiceProviderAlert.Description_MaxLength),
              TrimEnd(local.Document.Name)) <= 0)
            {
              local.OfficeServiceProviderAlert.Description =
                TrimEnd(entities.OfficeServiceProviderAlert.Description) + ","
                + TrimEnd(local.Document.Name);

              try
              {
                UpdateOfficeServiceProviderAlert();
                local.Document.Name = "";
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    break;
                  case ErrorCode.PermittedValueViolation:
                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
          else
          {
            // *****************************************************************
            // Create Office Service Provider alert
            // *****************************************************************
            local.OfficeServiceProviderAlert.RecipientUserId =
              local.RetrieveFieldValueTrigger.ServiceProviderLogonId;
            local.OfficeServiceProviderAlert.Description = local.Document.Name;
            UseSpCabCreateOfcSrvPrvdAlert();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              // mjr
              // ---------------------------------------------------------
              // 09/01/2000
              // Removed setting Error Flag.  There may have already been an 
              // error,
              // and setting it now will override that message.
              // Added writing it to the Error Report
              // ----------------------------------------------------------------------
              local.TextDate.TextDate =
                NumberToString(DateToInt(
                  entities.RetrieveFieldValueTrigger.ArchiveDate), 8, 8);
              local.Temp.RptDetail = local.TextDate.TextDate + "            " +
                NumberToString(entities.RetrieveFieldValueTrigger.InfId, 7, 9);
              local.EabReportSend.RptDetail =
                Substring(local.Temp.RptDetail,
                EabReportSend.RptDetail_MaxLength, 1, 31) + "     " + "Unable to create Service provider alert; User ID = " +
                local.RetrieveFieldValueTrigger.ServiceProviderLogonId;
              UseCabErrorReport();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

                return;
              }

              local.TextDate.TextDate = "";
              local.Temp.RptDetail = "";
              local.EabReportSend.RptDetail = "";
            }
            else
            {
              ++local.TotalAlertsCreated.Count;
            }
          }
        }
        else
        {
          // mjr
          // ---------------------------------------------------------
          // 09/01/2000
          // Removed setting Error Flag.  There may have already been an error,
          // and setting it now will override that message.
          // Added writing it to the Error Report
          // ----------------------------------------------------------------------
          local.TextDate.TextDate =
            NumberToString(DateToInt(
              entities.RetrieveFieldValueTrigger.ArchiveDate), 8, 8);
          local.Temp.RptDetail = local.TextDate.TextDate + "            " + NumberToString
            (entities.RetrieveFieldValueTrigger.InfId, 7, 9);
          local.EabReportSend.RptDetail =
            Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
            1, 31) + "     " + "Service provider not found; User ID = " + local
            .RetrieveFieldValueTrigger.ServiceProviderLogonId;
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

            return;
          }

          local.TextDate.TextDate = "";
          local.Temp.RptDetail = "";
          local.EabReportSend.RptDetail = "";
        }
      }

Test:

      local.TextDate.TextDate =
        NumberToString(
          DateToInt(entities.RetrieveFieldValueTrigger.ArchiveDate), 8, 8);
      local.Temp.RptDetail = local.TextDate.TextDate + "            " + NumberToString
        (entities.RetrieveFieldValueTrigger.InfId, 7, 9);

      if (!IsEmpty(local.Error.ActionEntry))
      {
        // *****************************************************************
        // Write if any Errors into Error Report file.
        // *****************************************************************
        switch(TrimEnd(local.Error.ActionEntry))
        {
          case "01":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Record not found on External File";

            break;
          case "03":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Field Value Permitted Value Violation";

            break;
          case "04":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Document Field not found";

            break;
          case "05":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Outgoing Document not found";

            break;
          case "06":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Outgoing Document not Unique";

            break;
          case "07":
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Outgoing Document Permitted Value Violation";

            break;
          default:
            local.EabReportSend.RptDetail =
              Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
              1, 31) + "     " + "Other Error";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        // ****************************************************************
        // Write Retrieval record as Successful into Report file
        // ****************************************************************
        // mjr
        // ---------------------------------------------
        // 09/01/2000
        // Removed writing successful triggers to Control Report.
        // Added writing them to Error Report if DEBUG = Y
        // ----------------------------------------------------------
        if (AsChar(local.DebugOn.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            Substring(local.Temp.RptDetail, EabReportSend.RptDetail_MaxLength,
            1, 35) + "     " + "Successful";
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      // *****************************************************************
      // Completed processing Field value Trigger record, delete it.
      // *****************************************************************
      DeleteRetrieveFieldValueTrigger();
      ++local.TotalTriggerRecsDel.Count;

      // *****************************************************************
      // Check for Commit count to Commit database
      // *****************************************************************
      if (local.Commit.Count >= local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // *****************************************************************
        // Commit database
        // *****************************************************************
        UseExtToDoACommit();

        if (local.PassArea.NumericReturnCode != 0)
        {
          ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

          return;
        }

        local.Commit.Count = 0;
      }
    }

    // *****************************************************************
    // Do final commit to database
    // *****************************************************************
    UseExtToDoACommit();

    if (local.PassArea.NumericReturnCode != 0)
    {
      ExitState = "ACO_RE0000_ERROR_EXT_DO_DB2_COM";

      return;
    }

    UseSpB309WriteControlsAndClose();
  }

  private static void MoveOfficeServiceProvider(OfficeServiceProvider source,
    OfficeServiceProvider target)
  {
    target.RoleCode = source.RoleCode;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MoveProgramProcessingInfo(ProgramProcessingInfo source,
    ProgramProcessingInfo target)
  {
    target.Name = source.Name;
    target.ProcessDate = source.ProcessDate;
  }

  private static void MoveRetrieveFieldValueTrigger(
    RetrieveFieldValueTrigger source, RetrieveFieldValueTrigger target)
  {
    target.InfId = source.InfId;
    target.ServiceProviderLogonId = source.ServiceProviderLogonId;
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

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.PassArea.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.PassArea.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void UseSpB309Housekeeping()
  {
    var useImport = new SpB309Housekeeping.Import();
    var useExport = new SpB309Housekeeping.Export();

    Call(SpB309Housekeeping.Execute, useImport, useExport);

    local.Local417.Assign(useExport.Export417);
    local.Local416.Assign(useExport.Export416);
    local.DebugOn.Flag = useExport.DebugOn.Flag;
    local.Current.Timestamp = useExport.Current.Timestamp;
    local.Infrastructure.SystemGeneratedIdentifier =
      useExport.Infrastructure.SystemGeneratedIdentifier;
    local.EndOfFile.Flag = useExport.EndOfFile.Flag;
    MoveProgramProcessingInfo(useExport.ProgramProcessingInfo,
      local.ProgramProcessingInfo);
    local.ProgramCheckpointRestart.Assign(useExport.ProgramCheckpointRestart);
  }

  private void UseSpB309WriteControlsAndClose()
  {
    var useImport = new SpB309WriteControlsAndClose.Import();
    var useExport = new SpB309WriteControlsAndClose.Export();

    useImport.DocsRetrieved.Count = local.TotalSucflRetrieveDocs.Count;
    useImport.DocsErrored.Count = local.TotalUnsuccRetrievDocs.Count;
    useImport.HistoryCreated.Count = local.TotalInfraRecsCreated.Count;
    useImport.AlertsCreated.Count = local.TotalAlertsCreated.Count;
    useImport.TriggersDeleted.Count = local.TotalTriggerRecsDel.Count;
    useImport.FieldValuesCreated.Count = local.TotalFieldValuesCreated.Count;
    useImport.ArchivedRecsRead.Count = local.TotalRecsReadFrmArcFl.Count;
    useImport.TriggersRead.Count = local.TotalTriggerRecsReads.Count;

    Call(SpB309WriteControlsAndClose.Execute, useImport, useExport);
  }

  private void UseSpCabCreateOfcSrvPrvdAlert()
  {
    var useImport = new SpCabCreateOfcSrvPrvdAlert.Import();
    var useExport = new SpCabCreateOfcSrvPrvdAlert.Export();

    useImport.ServiceProvider.SystemGeneratedId =
      entities.ServiceProvider.SystemGeneratedId;
    MoveOfficeServiceProvider(entities.OfficeServiceProvider,
      useImport.OfficeServiceProvider);
    useImport.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    useImport.Alerts.SystemGeneratedIdentifier =
      local.Infrastructure.SystemGeneratedIdentifier;
    useImport.OfficeServiceProviderAlert.
      Assign(local.OfficeServiceProviderAlert);

    Call(SpCabCreateOfcSrvPrvdAlert.Execute, useImport, useExport);
  }

  private void UseSpEabRetrieveFieldValues()
  {
    var useImport = new SpEabRetrieveFieldValues.Import();
    var useExport = new SpEabRetrieveFieldValues.Export();

    useImport.External.FileInstruction = local.PassArea.FileInstruction;
    useExport.WsFieldValues.Assign(local.WsFieldValues);
    useExport.External.Assign(local.PassArea);

    Call(SpEabRetrieveFieldValues.Execute, useImport, useExport);

    local.WsFieldValues.Assign(useExport.WsFieldValues);
    local.PassArea.Assign(useExport.External);
  }

  private void CreateFieldValue()
  {
    System.Diagnostics.Debug.Assert(entities.DocumentField.Populated);
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var createdBy = local.WsFieldValues.CreatedBy;
    var createdTimestamp = local.WsFieldValues.CreatedTimestamp;
    var lastUpdatedBy = local.WsFieldValues.LastUpdatedBy;
    var lastUpdatdTstamp = local.WsFieldValues.LastUpdatedTstamp;
    var value = local.WsFieldValues.Valu;
    var fldName = entities.DocumentField.FldName;
    var docName = entities.DocumentField.DocName;
    var docEffectiveDte = entities.DocumentField.DocEffectiveDte;
    var infIdentifier = entities.OutgoingDocument.InfId;

    entities.FieldValue.Populated = false;
    Update("CreateFieldValue",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetNullableString(command, "valu", value);
        db.SetString(command, "fldName", fldName);
        db.SetString(command, "docName", docName);
        db.SetDate(command, "docEffectiveDte", docEffectiveDte);
        db.SetInt32(command, "infIdentifier", infIdentifier);
      });

    entities.FieldValue.CreatedBy = createdBy;
    entities.FieldValue.CreatedTimestamp = createdTimestamp;
    entities.FieldValue.LastUpdatedBy = lastUpdatedBy;
    entities.FieldValue.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.FieldValue.Value = value;
    entities.FieldValue.FldName = fldName;
    entities.FieldValue.DocName = docName;
    entities.FieldValue.DocEffectiveDte = docEffectiveDte;
    entities.FieldValue.InfIdentifier = infIdentifier;
    entities.FieldValue.Populated = true;
  }

  private void DeleteRetrieveFieldValueTrigger()
  {
    Update("DeleteRetrieveFieldValueTrigger",
      (db, command) =>
      {
        db.SetDate(
          command, "archiveDate",
          entities.RetrieveFieldValueTrigger.ArchiveDate.GetValueOrDefault());
        db.SetInt32(command, "infId", entities.RetrieveFieldValueTrigger.InfId);
      });
  }

  private bool ReadDocument()
  {
    entities.Document.Populated = false;

    return Read("ReadDocument",
      (db, command) =>
      {
        db.SetInt32(command, "infId", local.RetrieveFieldValueTrigger.InfId);
      },
      (db, reader) =>
      {
        entities.Document.Name = db.GetString(reader, 0);
        entities.Document.EffectiveDate = db.GetDate(reader, 1);
        entities.Document.Populated = true;
      });
  }

  private bool ReadDocumentField()
  {
    entities.DocumentField.Populated = false;

    return Read("ReadDocumentField",
      (db, command) =>
      {
        db.SetString(command, "docName", local.WsFieldValues.DocName);
        db.SetDate(
          command, "docEffectiveDte",
          local.WsFieldValues.DocEffectiveDate.GetValueOrDefault());
        db.SetString(command, "fldName", local.WsFieldValues.FldName);
      },
      (db, reader) =>
      {
        entities.DocumentField.CreatedBy = db.GetString(reader, 0);
        entities.DocumentField.FldName = db.GetString(reader, 1);
        entities.DocumentField.DocName = db.GetString(reader, 2);
        entities.DocumentField.DocEffectiveDte = db.GetDate(reader, 3);
        entities.DocumentField.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderAlert()
  {
    entities.OfficeServiceProviderAlert.Populated = false;

    return Read("ReadOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetDate(
          command, "distributionDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(
          command, "situationIdentifi",
          local.OfficeServiceProviderAlert.SituationIdentifier);
        db.SetString(
          command, "userId",
          local.RetrieveFieldValueTrigger.ServiceProviderLogonId);
        db.SetString(command, "createdBy", local.ProgramProcessingInfo.Name);
      },
      (db, reader) =>
      {
        entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.OfficeServiceProviderAlert.TypeCode = db.GetString(reader, 1);
        entities.OfficeServiceProviderAlert.Message = db.GetString(reader, 2);
        entities.OfficeServiceProviderAlert.Description =
          db.GetNullableString(reader, 3);
        entities.OfficeServiceProviderAlert.DistributionDate =
          db.GetDate(reader, 4);
        entities.OfficeServiceProviderAlert.SituationIdentifier =
          db.GetString(reader, 5);
        entities.OfficeServiceProviderAlert.PrioritizationCode =
          db.GetNullableInt32(reader, 6);
        entities.OfficeServiceProviderAlert.OptimizationInd =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProviderAlert.OptimizedFlag =
          db.GetNullableString(reader, 8);
        entities.OfficeServiceProviderAlert.RecipientUserId =
          db.GetString(reader, 9);
        entities.OfficeServiceProviderAlert.CreatedBy =
          db.GetString(reader, 10);
        entities.OfficeServiceProviderAlert.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.OfficeServiceProviderAlert.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.OfficeServiceProviderAlert.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 13);
        entities.OfficeServiceProviderAlert.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderOffice()
  {
    entities.OfficeServiceProvider.Populated = false;
    entities.Office.Populated = false;

    return Read("ReadOfficeServiceProviderOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ServiceProvider.SystemGeneratedId);
        db.SetNullableDate(
          command, "discontinueDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.Populated = true;
        entities.Office.Populated = true;
      });
  }

  private bool ReadOutgoingDocument1()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument1",
      (db, command) =>
      {
        db.SetInt32(command, "infId", local.WsFieldValues.InfId);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private bool ReadOutgoingDocument2()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument2",
      (db, command) =>
      {
        db.SetInt32(command, "infId", local.OutDoc.InfId);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 0);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 1);
        entities.OutgoingDocument.DocName = db.GetNullableString(reader, 2);
        entities.OutgoingDocument.DocEffectiveDte =
          db.GetNullableDate(reader, 3);
        entities.OutgoingDocument.FieldValuesArchiveInd =
          db.GetNullableString(reader, 4);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 5);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private IEnumerable<bool> ReadRetrieveFieldValueTrigger()
  {
    entities.RetrieveFieldValueTrigger.Populated = false;

    return ReadEach("ReadRetrieveFieldValueTrigger",
      null,
      (db, reader) =>
      {
        entities.RetrieveFieldValueTrigger.ArchiveDate = db.GetDate(reader, 0);
        entities.RetrieveFieldValueTrigger.InfId = db.GetInt32(reader, 1);
        entities.RetrieveFieldValueTrigger.ServiceProviderLogonId =
          db.GetString(reader, 2);
        entities.RetrieveFieldValueTrigger.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ServiceProvider.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId",
          local.RetrieveFieldValueTrigger.ServiceProviderLogonId);
      },
      (db, reader) =>
      {
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ServiceProvider.UserId = db.GetString(reader, 1);
        entities.ServiceProvider.Populated = true;
      });
  }

  private void UpdateOfficeServiceProviderAlert()
  {
    var description = local.OfficeServiceProviderAlert.Description ?? "";

    entities.OfficeServiceProviderAlert.Populated = false;
    Update("UpdateOfficeServiceProviderAlert",
      (db, command) =>
      {
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.OfficeServiceProviderAlert.SystemGeneratedIdentifier);
      });

    entities.OfficeServiceProviderAlert.Description = description;
    entities.OfficeServiceProviderAlert.Populated = true;
  }

  private void UpdateOutgoingDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var fieldValuesArchiveInd = "N";

    CheckValid<OutgoingDocument>("FieldValuesArchiveInd", fieldValuesArchiveInd);
      
    entities.OutgoingDocument.Populated = false;
    Update("UpdateOutgoingDocument",
      (db, command) =>
      {
        db.SetNullableString(command, "fieldValArchInd", fieldValuesArchiveInd);
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      });

    entities.OutgoingDocument.FieldValuesArchiveInd = fieldValuesArchiveInd;
    entities.OutgoingDocument.Populated = true;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public OfficeServiceProviderAlert Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Local417.
    /// </summary>
    [JsonPropertyName("local417")]
    public OfficeServiceProviderAlert Local417
    {
      get => local417 ??= new();
      set => local417 = value;
    }

    /// <summary>
    /// A value of Local416.
    /// </summary>
    [JsonPropertyName("local416")]
    public OfficeServiceProviderAlert Local416
    {
      get => local416 ??= new();
      set => local416 = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of RetrieveFieldValueTrigger.
    /// </summary>
    [JsonPropertyName("retrieveFieldValueTrigger")]
    public RetrieveFieldValueTrigger RetrieveFieldValueTrigger
    {
      get => retrieveFieldValueTrigger ??= new();
      set => retrieveFieldValueTrigger = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OutDoc.
    /// </summary>
    [JsonPropertyName("outDoc")]
    public WsFieldValues OutDoc
    {
      get => outDoc ??= new();
      set => outDoc = value;
    }

    /// <summary>
    /// A value of WsFieldValues.
    /// </summary>
    [JsonPropertyName("wsFieldValues")]
    public WsFieldValues WsFieldValues
    {
      get => wsFieldValues ??= new();
      set => wsFieldValues = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public WsFieldValues Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
    }

    /// <summary>
    /// A value of Local416417.
    /// </summary>
    [JsonPropertyName("local416417")]
    public OfficeServiceProviderAlert Local416417
    {
      get => local416417 ??= new();
      set => local416417 = value;
    }

    /// <summary>
    /// A value of Repeat.
    /// </summary>
    [JsonPropertyName("repeat")]
    public Common Repeat
    {
      get => repeat ??= new();
      set => repeat = value;
    }

    /// <summary>
    /// A value of EndOfFile.
    /// </summary>
    [JsonPropertyName("endOfFile")]
    public Common EndOfFile
    {
      get => endOfFile ??= new();
      set => endOfFile = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public EabReportSend Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public DateWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
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
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public WsFieldValues Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of PassArea.
    /// </summary>
    [JsonPropertyName("passArea")]
    public External PassArea
    {
      get => passArea ??= new();
      set => passArea = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of TotalSucflRetrieveDocs.
    /// </summary>
    [JsonPropertyName("totalSucflRetrieveDocs")]
    public Common TotalSucflRetrieveDocs
    {
      get => totalSucflRetrieveDocs ??= new();
      set => totalSucflRetrieveDocs = value;
    }

    /// <summary>
    /// A value of TotalUnsuccRetrievDocs.
    /// </summary>
    [JsonPropertyName("totalUnsuccRetrievDocs")]
    public Common TotalUnsuccRetrievDocs
    {
      get => totalUnsuccRetrievDocs ??= new();
      set => totalUnsuccRetrievDocs = value;
    }

    /// <summary>
    /// A value of TotalInfraRecsCreated.
    /// </summary>
    [JsonPropertyName("totalInfraRecsCreated")]
    public Common TotalInfraRecsCreated
    {
      get => totalInfraRecsCreated ??= new();
      set => totalInfraRecsCreated = value;
    }

    /// <summary>
    /// A value of TotalAlertsCreated.
    /// </summary>
    [JsonPropertyName("totalAlertsCreated")]
    public Common TotalAlertsCreated
    {
      get => totalAlertsCreated ??= new();
      set => totalAlertsCreated = value;
    }

    /// <summary>
    /// A value of TotalTriggerRecsDel.
    /// </summary>
    [JsonPropertyName("totalTriggerRecsDel")]
    public Common TotalTriggerRecsDel
    {
      get => totalTriggerRecsDel ??= new();
      set => totalTriggerRecsDel = value;
    }

    /// <summary>
    /// A value of TotalFieldValuesCreated.
    /// </summary>
    [JsonPropertyName("totalFieldValuesCreated")]
    public Common TotalFieldValuesCreated
    {
      get => totalFieldValuesCreated ??= new();
      set => totalFieldValuesCreated = value;
    }

    /// <summary>
    /// A value of TotalRecsReadFrmArcFl.
    /// </summary>
    [JsonPropertyName("totalRecsReadFrmArcFl")]
    public Common TotalRecsReadFrmArcFl
    {
      get => totalRecsReadFrmArcFl ??= new();
      set => totalRecsReadFrmArcFl = value;
    }

    /// <summary>
    /// A value of TotalTriggerRecsReads.
    /// </summary>
    [JsonPropertyName("totalTriggerRecsReads")]
    public Common TotalTriggerRecsReads
    {
      get => totalTriggerRecsReads ??= new();
      set => totalTriggerRecsReads = value;
    }

    private OfficeServiceProviderAlert null1;
    private OfficeServiceProviderAlert local417;
    private OfficeServiceProviderAlert local416;
    private Common debugOn;
    private DateWorkArea current;
    private RetrieveFieldValueTrigger retrieveFieldValueTrigger;
    private Document document;
    private Office office;
    private WsFieldValues outDoc;
    private WsFieldValues wsFieldValues;
    private WsFieldValues previous;
    private Infrastructure infrastructure;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OfficeServiceProviderAlert local416417;
    private Common repeat;
    private Common endOfFile;
    private Common found;
    private Common error;
    private EabReportSend temp;
    private DateWorkArea textDate;
    private Common commit;
    private WsFieldValues blank;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private External passArea;
    private ProgramProcessingInfo programProcessingInfo;
    private ProgramCheckpointRestart programCheckpointRestart;
    private DateWorkArea nullDate;
    private Alert alert;
    private Common totalSucflRetrieveDocs;
    private Common totalUnsuccRetrievDocs;
    private Common totalInfraRecsCreated;
    private Common totalAlertsCreated;
    private Common totalTriggerRecsDel;
    private Common totalFieldValuesCreated;
    private Common totalRecsReadFrmArcFl;
    private Common totalTriggerRecsReads;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Event1.
    /// </summary>
    [JsonPropertyName("event1")]
    public Event1 Event1
    {
      get => event1 ??= new();
      set => event1 = value;
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
    /// A value of OfficeServiceProviderAlert.
    /// </summary>
    [JsonPropertyName("officeServiceProviderAlert")]
    public OfficeServiceProviderAlert OfficeServiceProviderAlert
    {
      get => officeServiceProviderAlert ??= new();
      set => officeServiceProviderAlert = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Alert.
    /// </summary>
    [JsonPropertyName("alert")]
    public Alert Alert
    {
      get => alert ??= new();
      set => alert = value;
    }

    /// <summary>
    /// A value of RetrieveFieldValueTrigger.
    /// </summary>
    [JsonPropertyName("retrieveFieldValueTrigger")]
    public RetrieveFieldValueTrigger RetrieveFieldValueTrigger
    {
      get => retrieveFieldValueTrigger ??= new();
      set => retrieveFieldValueTrigger = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
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
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
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
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    private EventDetail eventDetail;
    private Event1 event1;
    private ServiceProvider serviceProvider;
    private OfficeServiceProviderAlert officeServiceProviderAlert;
    private OfficeServiceProvider officeServiceProvider;
    private Office office;
    private Alert alert;
    private RetrieveFieldValueTrigger retrieveFieldValueTrigger;
    private Field field;
    private Infrastructure infrastructure;
    private DocumentField documentField;
    private Document document;
    private OutgoingDocument outgoingDocument;
    private FieldValue fieldValue;
  }
#endregion
}

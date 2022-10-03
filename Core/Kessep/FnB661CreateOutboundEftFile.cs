// Program: FN_B661_CREATE_OUTBOUND_EFT_FILE, ID: 372400323, model: 746.
// Short name: SWEF661B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B661_CREATE_OUTBOUND_EFT_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB661CreateOutboundEftFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B661_CREATE_OUTBOUND_EFT_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB661CreateOutboundEftFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB661CreateOutboundEftFile.
  /// </summary>
  public FnB661CreateOutboundEftFile(IContext context, Import import,
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
    // *************************************************
    // CHANGE LOG
    // AUTHOR        DATE        CHG REQ#   DESCRIPTION
    // H. Kennedy    6/5/97                 Source
    // H. Kennedy    6/5/97                 Current Date handled
    // M. Fangman    3/2/99                 Completely re-written
    // *************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseFnB661Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    local.OutputFileOpenInd.Flag = "N";
    local.EabFileHandling.Action = "WRITE";
    local.ElectronicFundTransmission.SequenceNumber = 0;
    local.ElectronicFundTransmission.FileCreationDate = Now().Date;
    local.ElectronicFundTransmission.FileCreationTime = Time(Now());
    local.PaymentStatusHistory.DiscontinueDate =
      UseCabSetMaximumDiscontinueDate();

    // *****
    // Process each outbound EFT in a RELEASED status.
    // *****
    foreach(var item in ReadElectronicFundTransmission())
    {
      local.EabReportSend.RptDetail = "";

      if (ReadPaymentRequest())
      {
        if (ReadPaymentStatusHistory())
        {
          try
          {
            UpdatePaymentRequest();

            try
            {
              UpdatePaymentStatusHistory();
              local.PaymentStatusHistory.SystemGeneratedIdentifier =
                entities.PaymentStatusHistory.SystemGeneratedIdentifier + 1;

              try
              {
                CreatePaymentStatusHistory();
              }
              catch(Exception e2)
              {
                switch(GetErrorCode(e2))
                {
                  case ErrorCode.AlreadyExists:
                    local.WorkArea.Text80 =
                      " Already Exists error creating \"DOA\" payment status history.";
                      

                    break;
                  case ErrorCode.PermittedValueViolation:
                    local.WorkArea.Text80 =
                      " Permitted Value violation creating \"DOA\" payment status history.";
                      

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
            catch(Exception e1)
            {
              switch(GetErrorCode(e1))
              {
                case ErrorCode.AlreadyExists:
                  local.WorkArea.Text80 =
                    " Not Unique error updating \"REQUESTED\" payment status history.";
                    

                  break;
                case ErrorCode.PermittedValueViolation:
                  local.WorkArea.Text80 =
                    " Permitted Value violation updating \"REQUESTED\" payment status history.";
                    

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
                local.WorkArea.Text80 =
                  " Not Unique error updating Payment Request process date.";

                break;
              case ErrorCode.PermittedValueViolation:
                local.WorkArea.Text80 =
                  " Permitted Value violation updating Payment Request process date.";
                  

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
          local.WorkArea.Text80 = " No current payment status history found.";
        }
      }
      else
      {
        local.WorkArea.Text80 = " No payment request associated to this EFT.";
      }

      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        UseCabTextnum1();

        if (entities.PaymentRequest.Populated)
        {
          UseCabTextnum2();
        }
        else
        {
          local.TextPaymentRequestId.Text9 = "";
        }

        local.EabReportSend.RptDetail = "EFT " + local
          .TextEftIdentifier.Text9 + "  Payment Request ID " + local
          .TextPaymentRequestId.Text9 + local.WorkArea.Text80;
        UseCabErrorReport();
        local.EabReportSend.RptDetail =
          "Abend error occurred.  Contact system support.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      ++local.NumberOfEftsRead.Count;

      if (AsChar(local.OutputFileOpenInd.Flag) == 'N')
      {
        // *****
        // Open outbound EFT file.
        // *****
        local.OutputFileOpenInd.Flag = "Y";
        local.EabFileHandling.Action = "OPEN";
        UseEabAccessOutboundEftFile2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // Write out the error line returned from the external.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 30) + local
            .EabFileHandling.Status;
          UseCabErrorReport();
          local.EabReportSend.RptDetail =
            "Abend error occurred.  Contact system support.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.EabFileHandling.Action = "WRITE";
      }

      // *****
      // Write out the EFT record (formatted for DOA)
      // *****
      UseEabAccessOutboundEftFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // *****
        // There was a file handling error with the outbound EFT transmission 
        // file.  Write out the error line returned from the external and then
        // write out the current EFT ID number read before abending.
        // *****
        local.Hold.Status = local.EabFileHandling.Status;
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();
        local.TextEftIdentifier.Text9 =
          NumberToString(entities.ElectronicFundTransmission.
            TransmissionIdentifier, 7, 9);
        local.EabReportSend.RptDetail = local.Hold.Status + " - EAB File Handling Status.  Current EFT record " +
          local.TextEftIdentifier.Text9;
        UseCabErrorReport();
        local.EabReportSend.RptDetail =
          "Abend error occurred.  Contact system support.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.ElectronicFundTransmission.SequenceNumber =
        local.ElectronicFundTransmission.SequenceNumber.GetValueOrDefault() + 1
        ;

      try
      {
        UpdateElectronicFundTransmission();
        ++local.NumberOfEftsProcessed.Count;
        local.AmountOfEftsProcessed.TotalCurrency += entities.
          ElectronicFundTransmission.TransmittalAmount;

        continue;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            local.WorkArea.Text80 =
              " Not Unique error updating the EFT Transmission status to \"DOA\".";
              

            break;
          case ErrorCode.PermittedValueViolation:
            local.WorkArea.Text80 =
              " Permitted Value error updating the EFT Transmission status to \"DOA\".";
              

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      UseCabTextnum1();
      UseCabTextnum2();
      local.EabReportSend.RptDetail = "EFT " + local.TextEftIdentifier.Text9 + "  Payment Request ID " +
        local.TextPaymentRequestId.Text9 + local.WorkArea.Text80;
      UseCabErrorReport();
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    try
    {
      CreateEftTransmissionFileInfo();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          local.EabReportSend.RptDetail =
            "Already Exists error creating the EFT Transmission File Info record.";
            
          UseCabErrorReport();
          local.EabReportSend.RptDetail =
            "Abend error occurred.  Contact system support.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.PermittedValueViolation:
          local.EabReportSend.RptDetail =
            "Permitted Value error creating the EFT Transmission File Info record.";
            
          UseCabErrorReport();
          local.EabReportSend.RptDetail =
            "Abend error occurred.  Contact system support.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // Write the totals to the control report.
    local.EabReportSend.RptDetail =
      "EFT Outbound Transmission File Info sent to DOA:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.DateWorkArea.Date = entities.EftTransmissionFileInfo.FileCreationDate;
    UseCabFormatDate();
    local.EabReportSend.RptDetail = "File creation date............" + local
      .FormattedDate.Text10;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.DateWorkArea.Time = entities.EftTransmissionFileInfo.FileCreationTime;
    UseCabFormatTime();
    local.EabReportSend.RptDetail = "File creation time............" + local
      .FormattedTime.Text8;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnum3();
    local.EabReportSend.RptDetail = "Total number of EFT records..." + local
      .WorkArea.Text9;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    UseCabTextnumAmount();
    local.EabReportSend.RptDetail = "Total amount of EFT records..." + local
      .WorkArea.Text11;
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "No errors occurred.";
    UseCabErrorReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // ***** Any error dealing with file handling is  "critical" and will 
      // result in an abend. *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private void UseCabControlReport()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseCabFormatDate()
  {
    var useImport = new CabFormatDate.Import();
    var useExport = new CabFormatDate.Export();

    useImport.Date.Date = local.DateWorkArea.Date;

    Call(CabFormatDate.Execute, useImport, useExport);

    local.FormattedDate.Text10 = useExport.FormattedDate.Text10;
  }

  private void UseCabFormatTime()
  {
    var useImport = new CabFormatTime.Import();
    var useExport = new CabFormatTime.Export();

    useImport.Time.Time = local.DateWorkArea.Time;

    Call(CabFormatTime.Execute, useImport, useExport);

    local.FormattedTime.Text8 = useExport.FormattedTime.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseCabTextnum1()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.ElectronicFundTransmission.TransmissionIdentifier =
      entities.ElectronicFundTransmission.TransmissionIdentifier;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TextEftIdentifier.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum2()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.PaymentRequest.SystemGeneratedIdentifier =
      entities.PaymentRequest.SystemGeneratedIdentifier;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.TextPaymentRequestId.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum3()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.NumberOfEftsProcessed.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnumAmount()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency = local.AmountOfEftsProcessed.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseEabAccessOutboundEftFile1()
  {
    var useImport = new EabAccessOutboundEftFile.Import();
    var useExport = new EabAccessOutboundEftFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.ElectronicFundTransmission.Assign(
      entities.ElectronicFundTransmission);
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.Error.RptDetail = local.EabReportSend.RptDetail;

    Call(EabAccessOutboundEftFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
  }

  private void UseEabAccessOutboundEftFile2()
  {
    var useImport = new EabAccessOutboundEftFile.Import();
    var useExport = new EabAccessOutboundEftFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.Error.RptDetail = local.EabReportSend.RptDetail;

    Call(EabAccessOutboundEftFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
  }

  private void UseFnB661Housekeeping()
  {
    var useImport = new FnB661Housekeeping.Import();
    var useExport = new FnB661Housekeeping.Export();

    useExport.PersistentDoa.Assign(entities.PaymentStatus);

    Call(FnB661Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    entities.PaymentStatus.SystemGeneratedIdentifier =
      useExport.PersistentDoa.SystemGeneratedIdentifier;
  }

  private void CreateEftTransmissionFileInfo()
  {
    var transmissionType = "O";
    var fileCreationDate = local.ElectronicFundTransmission.FileCreationDate;
    var fileCreationTime =
      local.ElectronicFundTransmission.FileCreationTime.GetValueOrDefault();
    var recordCount = local.NumberOfEftsProcessed.Count;
    var totalAmount = local.AmountOfEftsProcessed.TotalCurrency;

    CheckValid<EftTransmissionFileInfo>("TransmissionType", transmissionType);
    entities.EftTransmissionFileInfo.Populated = false;
    Update("CreateEftTransmissionFileInfo",
      (db, command) =>
      {
        db.SetString(command, "transmissionType", transmissionType);
        db.SetDate(command, "fileCreationDate", fileCreationDate);
        db.SetTimeSpan(command, "fileCreationTime", fileCreationTime);
        db.SetInt32(command, "recordCount", recordCount);
        db.SetDecimal(command, "totalAmount", totalAmount);
      });

    entities.EftTransmissionFileInfo.TransmissionType = transmissionType;
    entities.EftTransmissionFileInfo.FileCreationDate = fileCreationDate;
    entities.EftTransmissionFileInfo.FileCreationTime = fileCreationTime;
    entities.EftTransmissionFileInfo.RecordCount = recordCount;
    entities.EftTransmissionFileInfo.TotalAmount = totalAmount;
    entities.EftTransmissionFileInfo.Populated = true;
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistory.SystemGeneratedIdentifier;
    var effectiveDate = local.Process.Date;
    var discontinueDate = local.PaymentStatusHistory.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.PaymentStatusHistory.Populated = false;
    Update("CreatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "pstGeneratedId", pstGeneratedId);
        db.SetInt32(command, "prqGeneratedId", prqGeneratedId);
        db.SetInt32(command, "pymntStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", "");
      });

    entities.PaymentStatusHistory.PstGeneratedId = pstGeneratedId;
    entities.PaymentStatusHistory.PrqGeneratedId = prqGeneratedId;
    entities.PaymentStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PaymentStatusHistory.EffectiveDate = effectiveDate;
    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.CreatedBy = createdBy;
    entities.PaymentStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.PaymentStatusHistory.ReasonText = "";
    entities.PaymentStatusHistory.Populated = true;
  }

  private IEnumerable<bool> ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return ReadEach("ReadElectronicFundTransmission",
      null,
      (db, reader) =>
      {
        entities.ElectronicFundTransmission.CreatedBy = db.GetString(reader, 0);
        entities.ElectronicFundTransmission.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ElectronicFundTransmission.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.ElectronicFundTransmission.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 3);
        entities.ElectronicFundTransmission.PayDate =
          db.GetNullableDate(reader, 4);
        entities.ElectronicFundTransmission.TransmittalAmount =
          db.GetDecimal(reader, 5);
        entities.ElectronicFundTransmission.ApSsn = db.GetInt32(reader, 6);
        entities.ElectronicFundTransmission.MedicalSupportId =
          db.GetString(reader, 7);
        entities.ElectronicFundTransmission.ApName = db.GetString(reader, 8);
        entities.ElectronicFundTransmission.FipsCode =
          db.GetNullableInt32(reader, 9);
        entities.ElectronicFundTransmission.EmploymentTerminationId =
          db.GetNullableString(reader, 10);
        entities.ElectronicFundTransmission.SequenceNumber =
          db.GetNullableInt32(reader, 11);
        entities.ElectronicFundTransmission.ReceivingDfiIdentification =
          db.GetNullableInt32(reader, 12);
        entities.ElectronicFundTransmission.DfiAccountNumber =
          db.GetNullableString(reader, 13);
        entities.ElectronicFundTransmission.TransactionCode =
          db.GetString(reader, 14);
        entities.ElectronicFundTransmission.SettlementDate =
          db.GetNullableDate(reader, 15);
        entities.ElectronicFundTransmission.CaseId = db.GetString(reader, 16);
        entities.ElectronicFundTransmission.TransmissionStatusCode =
          db.GetString(reader, 17);
        entities.ElectronicFundTransmission.CompanyName =
          db.GetNullableString(reader, 18);
        entities.ElectronicFundTransmission.OriginatingDfiIdentification =
          db.GetNullableInt32(reader, 19);
        entities.ElectronicFundTransmission.ReceivingEntityName =
          db.GetNullableString(reader, 20);
        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 21);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 22);
        entities.ElectronicFundTransmission.TransmissionProcessDate =
          db.GetNullableDate(reader, 23);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 24);
        entities.ElectronicFundTransmission.FileCreationDate =
          db.GetNullableDate(reader, 25);
        entities.ElectronicFundTransmission.FileCreationTime =
          db.GetNullableTimeSpan(reader, 26);
        entities.ElectronicFundTransmission.CompanyIdentificationIcd =
          db.GetNullableString(reader, 27);
        entities.ElectronicFundTransmission.CompanyIdentificationNumber =
          db.GetNullableString(reader, 28);
        entities.ElectronicFundTransmission.CompanyDescriptiveDate =
          db.GetNullableDate(reader, 29);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 30);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 31);
        entities.ElectronicFundTransmission.TraceNumber =
          db.GetNullableInt64(reader, 32);
        entities.ElectronicFundTransmission.ApplicationIdentifier =
          db.GetNullableString(reader, 33);
        entities.ElectronicFundTransmission.CollectionAmount =
          db.GetNullableDecimal(reader, 34);
        entities.ElectronicFundTransmission.VendorNumber =
          db.GetNullableString(reader, 35);
        entities.ElectronicFundTransmission.CheckDigit =
          db.GetNullableInt32(reader, 36);
        entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
          db.GetNullableString(reader, 37);
        entities.ElectronicFundTransmission.CompanyEntryDescription =
          db.GetNullableString(reader, 38);
        entities.ElectronicFundTransmission.Populated = true;

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    System.Diagnostics.Debug.Assert(
      entities.ElectronicFundTransmission.Populated);
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          entities.ElectronicFundTransmission.PrqGeneratedId.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatusHistory()
  {
    entities.PaymentStatusHistory.Populated = false;

    return Read("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
        db.SetNullableDate(
          command, "discontinueDate",
          local.PaymentStatusHistory.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 7);
        entities.PaymentStatusHistory.Populated = true;
      });
  }

  private void UpdateElectronicFundTransmission()
  {
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var sequenceNumber =
      local.ElectronicFundTransmission.SequenceNumber.GetValueOrDefault();
    var transmissionStatusCode = "DOA";
    var transmissionProcessDate = local.Process.Date;
    var fileCreationDate = local.ElectronicFundTransmission.FileCreationDate;
    var fileCreationTime =
      local.ElectronicFundTransmission.FileCreationTime.GetValueOrDefault();

    entities.ElectronicFundTransmission.Populated = false;
    Update("UpdateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableInt32(command, "sequenceNumber", sequenceNumber);
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.
          SetNullableDate(command, "transProcessDate", transmissionProcessDate);
          
        db.SetNullableDate(command, "fileCreationDate", fileCreationDate);
        db.SetNullableTimeSpan(command, "fileCreationTime", fileCreationTime);
        db.SetString(
          command, "transmissionType",
          entities.ElectronicFundTransmission.TransmissionType);
        db.SetInt32(
          command, "transmissionId",
          entities.ElectronicFundTransmission.TransmissionIdentifier);
      });

    entities.ElectronicFundTransmission.LastUpdatedBy = lastUpdatedBy;
    entities.ElectronicFundTransmission.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ElectronicFundTransmission.SequenceNumber = sequenceNumber;
    entities.ElectronicFundTransmission.TransmissionStatusCode =
      transmissionStatusCode;
    entities.ElectronicFundTransmission.TransmissionProcessDate =
      transmissionProcessDate;
    entities.ElectronicFundTransmission.FileCreationDate = fileCreationDate;
    entities.ElectronicFundTransmission.FileCreationTime = fileCreationTime;
    entities.ElectronicFundTransmission.Populated = true;
  }

  private void UpdatePaymentRequest()
  {
    var processDate = local.Process.Date;

    entities.PaymentRequest.Populated = false;
    Update("UpdatePaymentRequest",
      (db, command) =>
      {
        db.SetDate(command, "processDate", processDate);
        db.SetInt32(
          command, "paymentRequestId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      });

    entities.PaymentRequest.ProcessDate = processDate;
    entities.PaymentRequest.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = local.Process.Date;

    entities.PaymentStatusHistory.Populated = false;
    Update("UpdatePaymentStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatusHistory.PstGeneratedId);
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentStatusHistory.PrqGeneratedId);
        db.SetInt32(
          command, "pymntStatHistId",
          entities.PaymentStatusHistory.SystemGeneratedIdentifier);
      });

    entities.PaymentStatusHistory.DiscontinueDate = discontinueDate;
    entities.PaymentStatusHistory.Populated = true;
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
    /// A value of OutputFileOpenInd.
    /// </summary>
    [JsonPropertyName("outputFileOpenInd")]
    public Common OutputFileOpenInd
    {
      get => outputFileOpenInd ??= new();
      set => outputFileOpenInd = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
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
    /// A value of NumberOfEftsRead.
    /// </summary>
    [JsonPropertyName("numberOfEftsRead")]
    public Common NumberOfEftsRead
    {
      get => numberOfEftsRead ??= new();
      set => numberOfEftsRead = value;
    }

    /// <summary>
    /// A value of NumberOfEftsProcessed.
    /// </summary>
    [JsonPropertyName("numberOfEftsProcessed")]
    public Common NumberOfEftsProcessed
    {
      get => numberOfEftsProcessed ??= new();
      set => numberOfEftsProcessed = value;
    }

    /// <summary>
    /// A value of AmountOfEftsProcessed.
    /// </summary>
    [JsonPropertyName("amountOfEftsProcessed")]
    public Common AmountOfEftsProcessed
    {
      get => amountOfEftsProcessed ??= new();
      set => amountOfEftsProcessed = value;
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
    /// A value of TextEftIdentifier.
    /// </summary>
    [JsonPropertyName("textEftIdentifier")]
    public WorkArea TextEftIdentifier
    {
      get => textEftIdentifier ??= new();
      set => textEftIdentifier = value;
    }

    /// <summary>
    /// A value of TextPaymentRequestId.
    /// </summary>
    [JsonPropertyName("textPaymentRequestId")]
    public WorkArea TextPaymentRequestId
    {
      get => textPaymentRequestId ??= new();
      set => textPaymentRequestId = value;
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
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public EabFileHandling Hold
    {
      get => hold ??= new();
      set => hold = value;
    }

    /// <summary>
    /// A value of EftRecsRead.
    /// </summary>
    [JsonPropertyName("eftRecsRead")]
    public Common EftRecsRead
    {
      get => eftRecsRead ??= new();
      set => eftRecsRead = value;
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
    /// A value of FormattedTime.
    /// </summary>
    [JsonPropertyName("formattedTime")]
    public TextWorkArea FormattedTime
    {
      get => formattedTime ??= new();
      set => formattedTime = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    private Common outputFileOpenInd;
    private PaymentStatusHistory paymentStatusHistory;
    private ElectronicFundTransmission electronicFundTransmission;
    private DateWorkArea process;
    private Common numberOfEftsRead;
    private Common numberOfEftsProcessed;
    private Common amountOfEftsProcessed;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private WorkArea textEftIdentifier;
    private WorkArea textPaymentRequestId;
    private WorkArea workArea;
    private EabFileHandling hold;
    private Common eftRecsRead;
    private DateWorkArea dateWorkArea;
    private TextWorkArea formattedTime;
    private WorkArea formattedDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public EftTransmissionFileInfo EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
    private EftTransmissionFileInfo eftTransmissionFileInfo;
  }
#endregion
}

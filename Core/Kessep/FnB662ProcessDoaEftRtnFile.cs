// Program: FN_B662_PROCESS_DOA_EFT_RTN_FILE, ID: 372401497, model: 746.
// Short name: SWEF662B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B662_PROCESS_DOA_EFT_RTN_FILE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB662ProcessDoaEftRtnFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B662_PROCESS_DOA_EFT_RTN_FILE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB662ProcessDoaEftRtnFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB662ProcessDoaEftRtnFile.
  /// </summary>
  public FnB662ProcessDoaEftRtnFile(IContext context, Import import,
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
    // M. Fangman  03/04/99                 Created this procedure and its 
    // called routines to receive and apply a "confirmation" file from D of A
    // for a EFT payment file that they sent to UMB.  We verify the dates &
    // totals then set the EFT tranmission records to a "SENT" status and update
    // the Effective Date (Settlement Date).
    // Fangman  9/9/99        Changed logic to accept a null or empty DofA file 
    // (return code 10).
    // *************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.ProgramProcessingInfo.Name = global.UserId;
    local.EabReportSend.ProgramName = global.UserId;
    UseReadProgramProcessingInfo();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    }
    else
    {
      // Check for an invalid return code coming back from the call to get the 
      // program processing info AFTER we have opened the Error Report file.
    }

    // *****
    // Open the Output Error Report 99.
    // *****
    local.EabFileHandling.Action = "OPEN";
    UseCabErrorReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // Now check for an invalid return code coming back from the call to get the
    // program processing info.
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Critical Error: Not Found reading program processing info for " + local
        .ProgramProcessingInfo.Name;
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Open the Output Control Report 98.
    // *****
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Read Payment Status of "SENT" to get currency to set the status of all of
    // the appropriate warrants to the Sent status.
    // *****
    if (!ReadPaymentStatus())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "\"SENT\" Payment Status was not found.";
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Open D of A EFT Return file and read the record.
    // *****
    UseEabAccessDoaEftReturnFile();
    local.EabFileHandling.Action = "WRITE";

    switch(TrimEnd(local.EabFileHandling.Status))
    {
      case "OK":
        // Continue
        break;
      case "10":
        // *****
        // The input file was empty which means DofA did not receive any EFT 
        // records to send so there are not EFT records to update.
        // *****
        local.EabReportSend.RptDetail =
          "DOA turn arround file is empty, there were no EFT records sent out.";
          
        UseCabControlReport1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

          return;
        }

        ExitState = "ACO_NI0000_PROCESSING_COMPLETE";

        return;
      default:
        // *****
        // There was a file handling error accessing the D of A Return File.  
        // Write out the error line returned from the external and then write
        // out an error message to the Error Report before abending.
        // *****
        local.Hold.Status = local.EabFileHandling.Status;
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          Substring(local.EabReportSend.RptDetail,
          EabReportSend.RptDetail_MaxLength, 1, 50) + local
          .EabFileHandling.Status;
        UseCabErrorReport1();
        local.EabReportSend.RptDetail = local.Hold.Status + "- EAB File Handling Status.";
          
        UseCabErrorReport1();
        local.EabReportSend.RptDetail =
          "Abend error occurred.  Contact system support.";
        UseCabErrorReport1();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
    }

    // *****
    // Write the D of A EFT Return File information to the Control Report 98.
    // *****
    local.EabReportSend.RptDetail = "DOA Outbound EFT Return File Information:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabTextnum5();
    local.EabReportSend.RptDetail =
      "Total number of EFT records sent by DOA to bank..." + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabTextnumAmount3();
    local.EabReportSend.RptDetail =
      "Total amount of EFTs sent by DOA to bank.........." + local
      .WorkArea.Text11;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.DateWorkArea.Date = local.DoaEftReturnRecord.DofASettlementDate.Date;
    UseCabFormatDate();
    local.EabReportSend.RptDetail =
      "DOA returned settlement date......................" + local
      .FormattedDate.Text10;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *****
    // Verify the count and amount against the EFT Transmission File Info table.
    // *****
    if (ReadEftTransmissionFileInfo())
    {
      local.EabReportSend.RptDetail =
        "EFT Outbound Transmission File Info sent to DOA:";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.DateWorkArea.Date =
        entities.EftTransmissionFileInfo.FileCreationDate;
      UseCabFormatDate();
      local.EabReportSend.RptDetail = "File creation date............" + local
        .FormattedDate.Text10;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.DateWorkArea.Time =
        entities.EftTransmissionFileInfo.FileCreationTime;
      UseCabFormatTime();
      local.EabReportSend.RptDetail = "File creation time............" + local
        .FormattedTime.Text8;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EftTransmissionFileInfo.Count =
        entities.EftTransmissionFileInfo.RecordCount;
      UseCabTextnum3();
      local.EabReportSend.RptDetail = "Total number of EFT records..." + local
        .WorkArea.Text9;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EftTransmissionFileInfo.TotalCurrency =
        entities.EftTransmissionFileInfo.TotalAmount;
      UseCabTextnumAmount1();
      local.EabReportSend.RptDetail = "Total Amount of EFT records..." + local
        .WorkArea.Text11;
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }

      local.EabReportSend.RptDetail = "";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }
    else
    {
      local.EabReportSend.RptDetail =
        "No matching Outbound EFT Transmission File Info record was not found.";
        
      UseCabErrorReport1();
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (entities.EftTransmissionFileInfo.RecordCount != local
      .DoaEftReturnRecord.TotalsFromDOfA.Count)
    {
      local.EabReportSend.RptDetail =
        "The EFT Transmission File Info count does not match the DOA EFT Return File count.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (entities.EftTransmissionFileInfo.TotalAmount != local
      .DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "The EFT Transmission File Info amount does not match the DOA EFT Return File amount.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (entities.EftTransmissionFileInfo.RecordCount != local
      .DoaEftReturnRecord.TotalsFromDOfA.Count || entities
      .EftTransmissionFileInfo.TotalAmount != local
      .DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    local.EftRecsReadUpdated.Count = 0;
    local.PaymentStatusHistory.DiscontinueDate =
      UseCabSetMaximumDiscontinueDate();

    // *****
    // Process inbound EFT transmission records for the file creation date.
    // *****
    foreach(var item in ReadElectronicFundTransmission())
    {
      ++local.EftRecsReadUpdated.Count;
      local.EftTableTotalAmount.TotalCurrency += entities.
        ElectronicFundTransmission.TransmittalAmount;

      try
      {
        UpdateElectronicFundTransmission();

        if (ReadPaymentRequest())
        {
          if (ReadPaymentStatusHistory())
          {
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
          else
          {
            local.WorkArea.Text80 = " No current payment status history found.";
          }
        }
        else
        {
          local.WorkArea.Text80 = " No payment request associated to this EFT.";
        }
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

      local.EabReportSend.RptDetail = "EFT " + local.TextEftIdentifier.Text9 + "  Payment Request ID " +
        local.TextPaymentRequestId.Text9 + local.WorkArea.Text80;
      UseCabErrorReport1();
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Write System Calculated totals to the Control Report 98.
    // *****
    local.EabReportSend.RptDetail = "System Calculated Matching Information:";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabTextnum4();
    local.EabReportSend.RptDetail =
      "Total number of outbound EFT Transmission records matched..." + local
      .WorkArea.Text9;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    UseCabTextnumAmount2();
    local.EabReportSend.RptDetail =
      "Total amount of outbound EFT transmission records matched..." + local
      .WorkArea.Text11;
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    // *****
    // Compare the System Calculated total records and total amount to D of A's 
    // total records and total amount.
    // *****
    if (local.EftRecsReadUpdated.Count != local
      .DoaEftReturnRecord.TotalsFromDOfA.Count)
    {
      local.EabReportSend.RptDetail =
        "The total system calculated inbound EFT transmission rec count did not match the total rec count on the trailer rec from the bank.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (local.EftTableTotalAmount.TotalCurrency != local
      .DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "The total system calculated inbound EFT transmission amount did not match the total amount on the trailer record from the bank.";
        
      UseCabErrorReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

        return;
      }
    }

    if (local.EftRecsReadUpdated.Count != local
      .DoaEftReturnRecord.TotalsFromDOfA.Count || local
      .EftTableTotalAmount.TotalCurrency != local
      .DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport1();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "No errors occurred.";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_WRITING_EXT_FILE";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

    useImport.Number.Count = local.EftTransmissionFileInfo.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum4()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.EftRecsReadUpdated.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnum5()
  {
    var useImport = new CabTextnum9.Import();
    var useExport = new CabTextnum9.Export();

    useImport.Number.Count = local.DoaEftReturnRecord.TotalsFromDOfA.Count;

    Call(CabTextnum9.Execute, useImport, useExport);

    local.WorkArea.Text9 = useExport.TextNumber.Text9;
  }

  private void UseCabTextnumAmount1()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.EftTransmissionFileInfo.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount2()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency = local.EftTableTotalAmount.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseCabTextnumAmount3()
  {
    var useImport = new CabTextnumAmount.Import();
    var useExport = new CabTextnumAmount.Export();

    useImport.Common.TotalCurrency =
      local.DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency;

    Call(CabTextnumAmount.Execute, useImport, useExport);

    local.WorkArea.Text11 = useExport.TextAmount.Text11;
  }

  private void UseEabAccessDoaEftReturnFile()
  {
    var useImport = new EabAccessDoaEftReturnFile.Import();
    var useExport = new EabAccessDoaEftReturnFile.Export();

    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.Error.RptDetail = local.EabReportSend.RptDetail;
    MoveCommon(local.DoaEftReturnRecord.TotalsFromDOfA,
      useExport.DoaEftReturnRecord.Totals);
    useExport.DoaEftReturnRecord.SettlementDate.Date =
      local.DoaEftReturnRecord.DofASettlementDate.Date;

    Call(EabAccessDoaEftReturnFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
    MoveCommon(useExport.DoaEftReturnRecord.Totals,
      local.DoaEftReturnRecord.TotalsFromDOfA);
    local.DoaEftReturnRecord.DofASettlementDate.Date =
      useExport.DoaEftReturnRecord.SettlementDate.Date;
  }

  private void UseReadProgramProcessingInfo()
  {
    var useImport = new ReadProgramProcessingInfo.Import();
    var useExport = new ReadProgramProcessingInfo.Export();

    useImport.ProgramProcessingInfo.Name = local.ProgramProcessingInfo.Name;

    Call(ReadProgramProcessingInfo.Execute, useImport, useExport);

    local.ProgramProcessingInfo.Assign(useExport.ProgramProcessingInfo);
  }

  private void CreatePaymentStatusHistory()
  {
    var pstGeneratedId = entities.PaymentStatus.SystemGeneratedIdentifier;
    var prqGeneratedId = entities.PaymentRequest.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.PaymentStatusHistory.SystemGeneratedIdentifier;
    var effectiveDate = local.ProgramProcessingInfo.ProcessDate;
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

  private bool ReadEftTransmissionFileInfo()
  {
    entities.EftTransmissionFileInfo.Populated = false;

    return Read("ReadEftTransmissionFileInfo",
      (db, command) =>
      {
        db.SetInt32(
          command, "recordCount",
          local.DoaEftReturnRecord.TotalsFromDOfA.Count);
        db.SetDecimal(
          command, "totalCurrency",
          local.DoaEftReturnRecord.TotalsFromDOfA.TotalCurrency);
      },
      (db, reader) =>
      {
        entities.EftTransmissionFileInfo.TransmissionType =
          db.GetString(reader, 0);
        entities.EftTransmissionFileInfo.FileCreationDate =
          db.GetDate(reader, 1);
        entities.EftTransmissionFileInfo.FileCreationTime =
          db.GetTimeSpan(reader, 2);
        entities.EftTransmissionFileInfo.RecordCount = db.GetInt32(reader, 3);
        entities.EftTransmissionFileInfo.TotalAmount = db.GetDecimal(reader, 4);
        entities.EftTransmissionFileInfo.Populated = true;
        CheckValid<EftTransmissionFileInfo>("TransmissionType",
          entities.EftTransmissionFileInfo.TransmissionType);
      });
  }

  private IEnumerable<bool> ReadElectronicFundTransmission()
  {
    entities.ElectronicFundTransmission.Populated = false;

    return ReadEach("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fileCreationDate",
          entities.EftTransmissionFileInfo.FileCreationDate.
            GetValueOrDefault());
        db.SetNullableTimeSpan(
          command, "fileCreationTime",
          entities.EftTransmissionFileInfo.FileCreationTime);
      },
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
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 1);
        entities.PaymentRequest.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Populated = true;
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
    var transmissionStatusCode = "SENT";
    var transmissionProcessDate = local.ProgramProcessingInfo.ProcessDate;
    var effectiveEntryDate = local.DoaEftReturnRecord.DofASettlementDate.Date;

    entities.ElectronicFundTransmission.Populated = false;
    Update("UpdateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.
          SetNullableDate(command, "transProcessDate", transmissionProcessDate);
          
        db.SetNullableDate(command, "effectiveEntryDt", effectiveEntryDate);
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
    entities.ElectronicFundTransmission.TransmissionStatusCode =
      transmissionStatusCode;
    entities.ElectronicFundTransmission.TransmissionProcessDate =
      transmissionProcessDate;
    entities.ElectronicFundTransmission.EffectiveEntryDate = effectiveEntryDate;
    entities.ElectronicFundTransmission.Populated = true;
  }

  private void UpdatePaymentStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);

    var discontinueDate = local.ProgramProcessingInfo.ProcessDate;

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
    /// <summary>A DoaEftReturnRecordGroup group.</summary>
    [Serializable]
    public class DoaEftReturnRecordGroup
    {
      /// <summary>
      /// A value of TotalsFromDOfA.
      /// </summary>
      [JsonPropertyName("totalsFromDOfA")]
      public Common TotalsFromDOfA
      {
        get => totalsFromDOfA ??= new();
        set => totalsFromDOfA = value;
      }

      /// <summary>
      /// A value of DofASettlementDate.
      /// </summary>
      [JsonPropertyName("dofASettlementDate")]
      public DateWorkArea DofASettlementDate
      {
        get => dofASettlementDate ??= new();
        set => dofASettlementDate = value;
      }

      private Common totalsFromDOfA;
      private DateWorkArea dofASettlementDate;
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
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
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
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
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
    /// A value of Hold.
    /// </summary>
    [JsonPropertyName("hold")]
    public EabFileHandling Hold
    {
      get => hold ??= new();
      set => hold = value;
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
    /// Gets a value of DoaEftReturnRecord.
    /// </summary>
    [JsonPropertyName("doaEftReturnRecord")]
    public DoaEftReturnRecordGroup DoaEftReturnRecord
    {
      get => doaEftReturnRecord ?? (doaEftReturnRecord = new());
      set => doaEftReturnRecord = value;
    }

    /// <summary>
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public Common EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    /// <summary>
    /// A value of EftRecsReadUpdated.
    /// </summary>
    [JsonPropertyName("eftRecsReadUpdated")]
    public Common EftRecsReadUpdated
    {
      get => eftRecsReadUpdated ??= new();
      set => eftRecsReadUpdated = value;
    }

    /// <summary>
    /// A value of EftTableTotalAmount.
    /// </summary>
    [JsonPropertyName("eftTableTotalAmount")]
    public Common EftTableTotalAmount
    {
      get => eftTableTotalAmount ??= new();
      set => eftTableTotalAmount = value;
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

    private DateWorkArea dateWorkArea;
    private WorkArea formattedDate;
    private TextWorkArea formattedTime;
    private PaymentStatusHistory paymentStatusHistory;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabFileHandling hold;
    private EabReportSend eabReportSend;
    private DoaEftReturnRecordGroup doaEftReturnRecord;
    private Common eftTransmissionFileInfo;
    private Common eftRecsReadUpdated;
    private Common eftTableTotalAmount;
    private WorkArea workArea;
    private WorkArea textEftIdentifier;
    private WorkArea textPaymentRequestId;
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

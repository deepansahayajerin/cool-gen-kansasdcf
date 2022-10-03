// Program: FN_B672_LOAD_INBOUND_EFT_TBL, ID: 372402590, model: 746.
// Short name: SWEF672B
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B672_LOAD_INBOUND_EFT_TBL.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB672LoadInboundEftTbl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B672_LOAD_INBOUND_EFT_TBL program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB672LoadInboundEftTbl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB672LoadInboundEftTbl.
  /// </summary>
  public FnB672LoadInboundEftTbl(IContext context, Import import, Export export):
    
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
    // DESCRIPTION
    // This process will read in an inbound EFT transmission file from the bank 
    // and write out each transaction to the EFT table.
    // There are three types of errors that may occur.
    // 1.  Catastrophic errors occur when we have a problem working with the 
    // flat files.  If we cannot access files to capture error information,
    // control totals or access the EFT file then we need to stop processing.
    // These types of errors require programmer intervention to resolve.
    // 2.  Critical errors occur when we could not write an EFT record to the 
    // table.  Processing of the remaining records continues after writing out
    // an error msg specifing the EFT record with the critical error.  Critical
    // errors require programmer intervention to resolve.
    // 3.  Pending errors occur when we could write the record to the table but 
    // one or more fields may not be correct.  Processing of the remaining
    // records continues after writing out an error msg specifing the EFT record
    // with the critical error.  Pending errors can be corrected online by a
    // user.
    // *************************************************
    // *************************************************
    // CHANGE LOG
    // AUTHOR        DATE        CHG REQ#   DESCRIPTION
    // M. Fangman   9/22/98                 Rewrote this procedure and its 
    // called routines to:
    // restructure, document, support interstate collections, avoid duplicate 
    // processing, support error resolution, support transactions after 12/31/
    // 1999, support capturing of error and control totals to a report.
    // Fangman 7/8/99  Changed code to use control table for next inbound eft 
    // number instead of reading for last used.
    // PR# H75935 - SWSRKXD 10/6/99
    // Ensure that an EFT file is not being resent with a different
    // date or time.  Compare record count and total amounts from
    // trailer record.
    // NB :- PPI parm is read to retrieve number of search days to
    // check for duplicate. Set PPI to 000 if duplicate search needs
    // to be bypassed!
    // Fangman    1/31/00  82289  Added an edit check on the company descriptive
    // date.
    // *************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // *****
    // House keeping will open the Error Rpt file and write the heading info, 
    // open the Control Rpt file and write heading info, open the inbound EFT
    // transmission file and return the control info (creation date, creation
    // time, total records in file, total amount of funds transferred), return
    // the next ID number for new EFT rows, returns the process date.
    // *****
    UseFnB672Housekeeping();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // *****
      // Any error dealing with file handling is a "catastrophic error" and will
      // result in an abend.
      // *****
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.Initialized.Date = new DateTime(1, 1, 1);

    if (ReadControlTable())
    {
      local.NextId.TransmissionIdentifier =
        entities.InboundEftNumber.LastUsedNumber + 1;
    }
    else
    {
      local.EabReportSend.RptDetail =
        "Critical Error:  Received a Not Found trying to read the control table to get the next Inbound EFT number.";
        
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // *****
    // Process inbound EFT transmission records until we have reached the end of
    // file.
    // *****
    do
    {
      local.ForCreateElectronicFundTransmission.TransmissionStatusCode =
        "RELEASED";

      // *****
      // Call external to read each EFT transmission record.
      // *****
      local.EabFileHandling.Action = "READ";
      UseEabAccessInboundEftFile();

      switch(TrimEnd(local.EabFileHandling.Status))
      {
        case "OK":
          // Continue
          break;
        case "EOF":
          // *****
          // An End Of File (EOF) is ok.  Jump out of the read loop.
          // *****
          goto AfterCycle;
        default:
          // *****
          // There was a file handling error with the inbound EFT transmission 
          // file.  Write out the error line returned from the external and then
          // write out an error record with the # of records read to the Error
          // Report before abending.
          // *****
          local.Hold.Status = local.EabFileHandling.Status;
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            Substring(local.EabReportSend.RptDetail,
            EabReportSend.RptDetail_MaxLength, 1, 46) + local
            .EabFileHandling.Status;
          UseCabErrorReport();

          if (Equal(local.EabFileHandling.Status, "OK"))
          {
            ++local.EftRecsRead.Count;
          }
          else if (Equal(local.EabFileHandling.Status, "NOT EOF"))
          {
            local.EftRecsRead.Count += 2;
          }

          local.EabReportSend.RptDetail = local.Hold.Status + " - EAB File Handling Status.  Records read  = " +
            NumberToString(local.EftRecsRead.Count, 15);
          UseCabErrorReport();

          // *****
          // Any error dealing with file handling is a "catastrophic error" and 
          // will result in an abend.
          // *****
          local.EabReportSend.RptDetail =
            "Abend error occurred.  Contact system support.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
      }

      ++local.EftRecsRead.Count;
      local.EftTableTotalAmount.TotalCurrency += local.EftDetailRecord.
        TransmittalAmount;

      // *****
      // If there was a non-critical error in the record read then an error msg 
      // will be in the rpt_detail field.
      // Types of errors may include invalid values in certain fields.
      // Records with errors are still written to the EFT table so that they can
      // be corrected online.
      // *****
      if (!IsEmpty(local.EabReportSend.RptDetail))
      {
        local.EabFileHandling.Action = "WRITE";
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****
          // Any error dealing with file handling is a "catastrophic error" and 
          // will result in an abend.
          // *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.EabReportSend.RptDetail =
          "Abend error occurred in external.  Contact system support.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      // *****
      // Before writing this EFT record to the table check to see if it has 
      // already been processed previously.
      // *****
      if (ReadElectronicFundTransmission())
      {
        // *****
        // A duplicate record was found.  This is an error.
        // Write a detail line to the Error Report and skip the processing of 
        // this record.
        // *****
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Sequence number " + NumberToString
          (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Critical Error: Duplicate record found on EFT table - may have been processed previsouly.";
          
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****
          // Any error dealing with file handling is a "catastrophic error" and 
          // will result in an abend.
          // *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.EabReportSend.RptDetail =
          "Abend error occurred.  Contact system support.";
        UseCabErrorReport();
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
      else
      {
        // *****
        // No duplicate record was found.  This is good - Continue.
        // *****
      }

      // *****
      // If the application ID is equal to 'RI', 'RT' or 'RO' then the two 
      // payment amounts must be different.  If the application ID is equal to '
      // CS', 'II', 'IT', 'IO' then the amounts must be the same. Write out an
      // error msg but do not pend the record.  When the cash receipt is created
      // in the next process it will be pended there.
      // *****
      if (!IsEmpty(local.EftDetailRecord.ApplicationIdentifier))
      {
        if (Equal(local.EftDetailRecord.ApplicationIdentifier, "RI") || Equal
          (local.EftDetailRecord.ApplicationIdentifier, "RT") || Equal
          (local.EftDetailRecord.ApplicationIdentifier, "RO"))
        {
          if (local.EftDetailRecord.TransmittalAmount == local
            .EftDetailRecord.CollectionAmount.GetValueOrDefault())
          {
            ++local.TotWarningsAndMsgs.Count;
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Sequence number " + NumberToString
              (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Warning: The amount fields should be different based upon the Ap ID of " +
              (local.EftDetailRecord.ApplicationIdentifier ?? "");
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****
              // Any error dealing with file handling is a "catastrophic error" 
              // and will result in an abend.
              // *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
          else
          {
            // Continue
          }
        }
        else if (Equal(local.EftDetailRecord.ApplicationIdentifier, "CS") || Equal
          (local.EftDetailRecord.ApplicationIdentifier, "II") || Equal
          (local.EftDetailRecord.ApplicationIdentifier, "IT") || Equal
          (local.EftDetailRecord.ApplicationIdentifier, "IO"))
        {
          if (local.EftDetailRecord.TransmittalAmount == local
            .EftDetailRecord.CollectionAmount.GetValueOrDefault())
          {
            // Continue
          }
          else
          {
            ++local.TotWarningsAndMsgs.Count;
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Sequence number " + NumberToString
              (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Warning: The amount fields should be the same based upon the Ap ID of " +
              (local.EftDetailRecord.ApplicationIdentifier ?? "");
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****
              // Any error dealing with file handling is a "catastrophic error" 
              // and will result in an abend.
              // *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }
          }
        }
        else if (!IsEmpty(local.EftDetailRecord.ApplicationIdentifier))
        {
          ++local.TotWarningsAndMsgs.Count;
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Sequence number " + NumberToString
            (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Warning: The application ID is invalid: " +
            (local.EftDetailRecord.ApplicationIdentifier ?? "");
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****
            // Any error dealing with file handling is a "catastrophic error" 
            // and will result in an abend.
            // *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }
        }
      }

      if (Equal(local.EftDetailRecord.CompanyDescriptiveDate,
        local.Initialized.Date))
      {
        ++local.TotWarningsAndMsgs.Count;
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Sequence number " + NumberToString
          (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Warning: The company descriptive date is invalid: " +
          NumberToString
          (DateToInt(local.EftDetailRecord.CompanyDescriptiveDate), 8, 8);
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****
          // Any error dealing with file handling is a "catastrophic error" and 
          // will result in an abend.
          // *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      if (local.EftDetailRecord.TransmittalAmount == 0)
      {
        // A transmittal amount of zero means this is a pre-note EFT.
        ++local.TotWarningsAndMsgs.Count;
        ++local.EftPreNotes.Count;
        local.ForCreateElectronicFundTransmission.TransmissionStatusCode =
          "TESTED";
        local.ForCreateElectronicFundTransmission.TransmissionProcessDate =
          local.Process.Date;
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Sequence number " + NumberToString
          (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Message: This EFT record is a pre-note for AP " +
          local.EftDetailRecord.ApName;
        UseCabErrorReport();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // *****
          // Any error dealing with file handling is a "catastrophic error" and 
          // will result in an abend.
          // *****
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }
      else
      {
        ++local.EftRecordsReleased.Count;
        local.ForCreateElectronicFundTransmission.TransmissionProcessDate =
          local.Null1.Date;
      }

      local.ForCreateDateWorkArea.Timestamp = Now();

      try
      {
        CreateElectronicFundTransmission();
        ++local.EftRecsWritten.Count;
        ++local.NextId.TransmissionIdentifier;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            // *****
            // There was an "already exists" error trying to write the inbound 
            // EFT transmission row.  Write out a record to the Error Report
            // before abending.
            // *****
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Sequence number " + NumberToString
              (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Critical Error: Already Exists error when writing to the database.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****
              // Any error dealing with file handling is a "catastrophic error" 
              // and will result in an abend.
              // *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            local.EabReportSend.RptDetail =
              "Abend error occurred.  Contact system support.";
            UseCabErrorReport();
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          case ErrorCode.PermittedValueViolation:
            // *****
            // There was a permitted value error trying to write the inbound EFT
            // transmission row.  Write out a record to the Error Report.
            // *****
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = "Sequence number " + NumberToString
              (local.EftDetailRecord.SequenceNumber.GetValueOrDefault(), 15) + " Critical Error: Permitted Value error when writing to the database.";
              
            UseCabErrorReport();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              // *****
              // Any error dealing with file handling is a "catastrophic error" 
              // and will result in an abend.
              // *****
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

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
    }
    while(!Equal(local.EabFileHandling.Status, "EOF"));

AfterCycle:

    // *****
    // The external hit the end of the driver file,
    // closed the file and returned an (EOF) indicator.
    // *****
    // **********************************************************
    // WRITE TOTALS TO CONTROL REPORT 98
    // **********************************************************
    local.EabFileHandling.Action = "WRITE";
    local.EabReportSend.RptDetail =
      "Total number of inbound EFT records in file trailer..." + NumberToString
      (local.EftTrailerRecord.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkArea.Text15 =
      NumberToString((long)(local.EftTrailerRecord.TotalCurrency * 100), 15);
    local.EabReportSend.RptDetail =
      "Total amount of inbound EFT records in file trailer..." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 5, 9) + "." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail = "System Calculated Information:";
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of inbound EFT records read.........." + NumberToString
      (local.EftRecsRead.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of EFT released records.............." + NumberToString
      (local.EftRecordsReleased.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of EFT pre-note records.............." + NumberToString
      (local.EftPreNotes.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of EFT rows written.................." + NumberToString
      (local.EftRecsWritten.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.WorkArea.Text15 =
      NumberToString((long)(local.EftTableTotalAmount.TotalCurrency * 100), 15);
      
    local.EabReportSend.RptDetail =
      "Total amount of inbound EFT transmissions........." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 7, 7) + "." + Substring
      (local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    local.EabReportSend.RptDetail =
      "Total number of warnings and messages............." + NumberToString
      (local.TotWarningsAndMsgs.Count, 15);
    UseCabControlReport();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // **********************************************************
    // Compare our total records and total amount to the banks total records and
    // total amount.
    // **********************************************************
    if (local.EftRecsRead.Count != local.EftTrailerRecord.Count)
    {
      local.EabReportSend.RptDetail =
        "The total system calculated inbound EFT transmission rec count did not match the total rec count on the trailer rec from the bank.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (local.EftTableTotalAmount.TotalCurrency != local
      .EftTrailerRecord.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "The total system calculated inbound EFT transmission amount did not match the total amount on the trailer record from the bank.";
        
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    if (local.EftRecsRead.Count != local.EftTrailerRecord.Count || local
      .EftTableTotalAmount.TotalCurrency != local
      .EftTrailerRecord.TotalCurrency)
    {
      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    if (local.EftRecsRead.Count == 0)
    {
      local.EabReportSend.RptDetail = "";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail = "No EFT payments received for this date.";
      UseCabControlReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // *****
    // PR# H75935 - SWSRKXD 10/6/99
    // Ensure that an EFT file is not being resent with a different
    // date or time.  Compare record count and total amounts from
    // trailer record.
    // *****
    // **********
    // Read PPI record to get number of search days. Only search
    // for a duplicate file within this range.
    // NB : - Set PPI to 000 if duplicate search needs to be bypassed!
    // **********
    if (!ReadProgramProcessingInfo())
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "PPI record not found for SWEFB672. Contact system support.";
      UseCabErrorReport();
      ExitState = "PROGRAM_PROCESSING_INFO_NF_AB";

      return;
    }

    local.NbrOfDays.Count =
      (int)StringToNumber(Substring(
        entities.ProgramProcessingInfo.ParameterList, 1, 3));

    if (ReadEftTransmissionFileInfo())
    {
      // *****
      // A duplicate file was found.  This is a critical error.
      // Write a detail line to the Error Report and Abort.
      // *****
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        " Critical Error: Duplicate file found on eft_transmission_file_info table. Previous file date =";
        
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (DateToInt(entities.EftTransmissionFileInfo.FileCreationDate), 8, 8);
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + "; Prev file time =";
        
      local.EabReportSend.RptDetail = TrimEnd(local.EabReportSend.RptDetail) + NumberToString
        (TimeToInt(entities.EftTransmissionFileInfo.FileCreationTime), 10, 6);
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // *****
        // Any error dealing with file handling is a "catastrophic error" and 
        // will result in an abend.
        // *****
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }

      local.EabReportSend.RptDetail =
        "Abend error occurred.  Contact system support.";
      UseCabErrorReport();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }
    else
    {
      // *****
      // This is good!
      // *****
    }

    // *****
    // Passed all validation!
    // *****
    try
    {
      CreateEftTransmissionFileInfo();

      // Continue
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          // *****
          // There was an "already exists" error trying to write the EFT 
          // transmission file info.  Write out a record to the Error Report
          // before abending.
          // *****
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Already Exists error writing EFT transmission file info to table.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****
            // Any error dealing with file handling is a "catastrophic error" 
            // and will result in an abend.
            // *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

          local.EabReportSend.RptDetail =
            "Abend error occurred.  Contact system support.";
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.PermittedValueViolation:
          // *****
          // There was a permitted value error trying to write the EFT 
          // transmission file info.  Write out a record to the Error Report.
          // *****
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Permitted Value error writing EFT transmission file info to table.";
            
          UseCabErrorReport();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            // *****
            // Any error dealing with file handling is a "catastrophic error" 
            // and will result in an abend.
            // *****
            ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

            return;
          }

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

    try
    {
      UpdateControlTable();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ++local.Totals.NbrOfUpdates.Count;
          local.EabReportSend.RptDetail =
            "Critical Error: Not Unique updating Control Table for " + entities
            .InboundEftNumber.Identifier;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.PermittedValueViolation:
          local.EabReportSend.RptDetail =
            "Critical Error: Permitted Value violation updating Control Table for " +
            entities.InboundEftNumber.Identifier;
          UseCabErrorReport();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (local.TotWarningsAndMsgs.Count == 0)
    {
      local.EabReportSend.RptDetail = "No errors found.";
      UseCabErrorReport();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Time = source.Time;
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.PayDate = source.PayDate;
    target.TransmittalAmount = source.TransmittalAmount;
    target.ApSsn = source.ApSsn;
    target.MedicalSupportId = source.MedicalSupportId;
    target.ApName = source.ApName;
    target.FipsCode = source.FipsCode;
    target.EmploymentTerminationId = source.EmploymentTerminationId;
    target.SequenceNumber = source.SequenceNumber;
    target.TransactionCode = source.TransactionCode;
    target.CaseId = source.CaseId;
    target.CompanyName = source.CompanyName;
    target.OriginatingDfiIdentification = source.OriginatingDfiIdentification;
    target.CompanyIdentificationIcd = source.CompanyIdentificationIcd;
    target.CompanyIdentificationNumber = source.CompanyIdentificationNumber;
    target.CompanyDescriptiveDate = source.CompanyDescriptiveDate;
    target.EffectiveEntryDate = source.EffectiveEntryDate;
    target.ReceivingCompanyName = source.ReceivingCompanyName;
    target.TraceNumber = source.TraceNumber;
    target.ApplicationIdentifier = source.ApplicationIdentifier;
    target.CollectionAmount = source.CollectionAmount;
    target.ReceivingDfiAccountNumber = source.ReceivingDfiAccountNumber;
    target.CompanyEntryDescription = source.CompanyEntryDescription;
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

  private void UseEabAccessInboundEftFile()
  {
    var useImport = new EabAccessInboundEftFile.Import();
    var useExport = new EabAccessInboundEftFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;
    useExport.Error.RptDetail = local.EabReportSend.RptDetail;
    useExport.EftDetailRecord.Assign(local.EftDetailRecord);
    MoveCommon(local.EftTrailerRecord, useExport.EftTrailerRecord);

    Call(EabAccessInboundEftFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
    local.EabReportSend.RptDetail = useExport.Error.RptDetail;
    MoveElectronicFundTransmission(useExport.EftDetailRecord,
      local.EftDetailRecord);
    MoveCommon(useExport.EftTrailerRecord, local.EftTrailerRecord);
  }

  private void UseFnB672Housekeeping()
  {
    var useImport = new FnB672Housekeeping.Import();
    var useExport = new FnB672Housekeeping.Export();

    Call(FnB672Housekeeping.Execute, useImport, useExport);

    local.Process.Date = useExport.Process.Date;
    MoveDateWorkArea(useExport.EftHeaderRecord, local.EftHeaderRecord);
  }

  private void CreateEftTransmissionFileInfo()
  {
    var transmissionType = "I";
    var fileCreationDate = local.EftHeaderRecord.Date;
    var fileCreationTime = local.EftHeaderRecord.Time;
    var recordCount = local.EftTrailerRecord.Count;
    var totalAmount = local.EftTrailerRecord.TotalCurrency;

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

  private void CreateElectronicFundTransmission()
  {
    var createdBy = global.UserId;
    var createdTimestamp = local.ForCreateDateWorkArea.Timestamp;
    var payDate = local.EftDetailRecord.PayDate;
    var transmittalAmount = local.EftDetailRecord.TransmittalAmount;
    var apSsn = local.EftDetailRecord.ApSsn;
    var medicalSupportId = local.EftDetailRecord.MedicalSupportId;
    var apName = local.EftDetailRecord.ApName;
    var fipsCode = local.EftDetailRecord.FipsCode.GetValueOrDefault();
    var employmentTerminationId =
      local.EftDetailRecord.EmploymentTerminationId ?? "";
    var sequenceNumber =
      local.EftDetailRecord.SequenceNumber.GetValueOrDefault();
    var transactionCode = local.EftDetailRecord.TransactionCode;
    var caseId = local.EftDetailRecord.CaseId;
    var transmissionStatusCode =
      local.ForCreateElectronicFundTransmission.TransmissionStatusCode;
    var companyName = local.EftDetailRecord.CompanyName ?? "";
    var originatingDfiIdentification =
      local.EftDetailRecord.OriginatingDfiIdentification.GetValueOrDefault();
    var transmissionType = "I";
    var transmissionIdentifier = local.NextId.TransmissionIdentifier;
    var transmissionProcessDate =
      local.ForCreateElectronicFundTransmission.TransmissionProcessDate;
    var fileCreationDate = local.EftHeaderRecord.Date;
    var fileCreationTime = local.EftHeaderRecord.Time;
    var companyIdentificationIcd =
      local.EftDetailRecord.CompanyIdentificationIcd ?? "";
    var companyIdentificationNumber =
      local.EftDetailRecord.CompanyIdentificationNumber ?? "";
    var companyDescriptiveDate = local.EftDetailRecord.CompanyDescriptiveDate;
    var effectiveEntryDate = local.EftDetailRecord.EffectiveEntryDate;
    var receivingCompanyName = local.EftDetailRecord.ReceivingCompanyName ?? "";
    var traceNumber = local.EftDetailRecord.TraceNumber.GetValueOrDefault();
    var applicationIdentifier = local.EftDetailRecord.ApplicationIdentifier ?? ""
      ;
    var collectionAmount =
      local.EftDetailRecord.CollectionAmount.GetValueOrDefault();
    var receivingDfiAccountNumber =
      local.EftDetailRecord.ReceivingDfiAccountNumber ?? "";
    var companyEntryDescription =
      local.EftDetailRecord.CompanyEntryDescription ?? "";

    entities.ElectronicFundTransmission.Populated = false;
    Update("CreateElectronicFundTransmission",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDate(command, "payDate", payDate);
        db.SetDecimal(command, "transmittalAmount", transmittalAmount);
        db.SetInt32(command, "apSsn", apSsn);
        db.SetString(command, "medicalSupportId", medicalSupportId);
        db.SetString(command, "apName", apName);
        db.SetNullableInt32(command, "fipsCode", fipsCode);
        db.SetNullableString(
          command, "employmentTermId", employmentTerminationId);
        db.SetNullableInt32(command, "zdelAdendaSqNum", 0);
        db.SetNullableInt32(command, "sequenceNumber", sequenceNumber);
        db.SetNullableInt32(command, "receivingDfiIden", 0);
        db.SetNullableString(command, "dfiAcctNumber", "");
        db.SetString(command, "transactionCode", transactionCode);
        db.SetNullableDate(command, "settlementDate", default(DateTime));
        db.SetString(command, "caseId", caseId);
        db.SetString(command, "transStatusCode", transmissionStatusCode);
        db.SetNullableString(command, "companyName", companyName);
        db.SetNullableInt32(
          command, "origDfiIdent", originatingDfiIdentification);
        db.SetNullableString(command, "recvEntityName", "");
        db.SetString(command, "transmissionType", transmissionType);
        db.SetInt32(command, "transmissionId", transmissionIdentifier);
        db.
          SetNullableDate(command, "transProcessDate", transmissionProcessDate);
          
        db.SetNullableDate(command, "fileCreationDate", fileCreationDate);
        db.SetNullableTimeSpan(command, "fileCreationTime", fileCreationTime);
        db.SetNullableString(
          command, "companyIdentIcd", companyIdentificationIcd);
        db.SetNullableString(
          command, "companyIdentNum", companyIdentificationNumber);
        db.SetNullableDate(command, "companyDescDate", companyDescriptiveDate);
        db.SetNullableDate(command, "effectiveEntryDt", effectiveEntryDate);
        db.SetNullableString(command, "recvCompanyName", receivingCompanyName);
        db.SetNullableInt64(command, "traceNumber", traceNumber);
        db.
          SetNullableString(command, "applicationIdent", applicationIdentifier);
          
        db.SetNullableDecimal(command, "collectionAmount", collectionAmount);
        db.SetNullableString(command, "vendorNumber", "");
        db.SetNullableInt32(command, "checkDigit", 0);
        db.SetNullableString(
          command, "recvDfiAcctNum", receivingDfiAccountNumber);
        db.SetNullableString(
          command, "companyEntryDesc", companyEntryDescription);
      });

    entities.ElectronicFundTransmission.CreatedBy = createdBy;
    entities.ElectronicFundTransmission.CreatedTimestamp = createdTimestamp;
    entities.ElectronicFundTransmission.LastUpdatedBy = createdBy;
    entities.ElectronicFundTransmission.LastUpdatedTimestamp = createdTimestamp;
    entities.ElectronicFundTransmission.PayDate = payDate;
    entities.ElectronicFundTransmission.TransmittalAmount = transmittalAmount;
    entities.ElectronicFundTransmission.ApSsn = apSsn;
    entities.ElectronicFundTransmission.MedicalSupportId = medicalSupportId;
    entities.ElectronicFundTransmission.ApName = apName;
    entities.ElectronicFundTransmission.FipsCode = fipsCode;
    entities.ElectronicFundTransmission.EmploymentTerminationId =
      employmentTerminationId;
    entities.ElectronicFundTransmission.SequenceNumber = sequenceNumber;
    entities.ElectronicFundTransmission.TransactionCode = transactionCode;
    entities.ElectronicFundTransmission.CaseId = caseId;
    entities.ElectronicFundTransmission.TransmissionStatusCode =
      transmissionStatusCode;
    entities.ElectronicFundTransmission.CompanyName = companyName;
    entities.ElectronicFundTransmission.OriginatingDfiIdentification =
      originatingDfiIdentification;
    entities.ElectronicFundTransmission.TransmissionType = transmissionType;
    entities.ElectronicFundTransmission.TransmissionIdentifier =
      transmissionIdentifier;
    entities.ElectronicFundTransmission.TransmissionProcessDate =
      transmissionProcessDate;
    entities.ElectronicFundTransmission.FileCreationDate = fileCreationDate;
    entities.ElectronicFundTransmission.FileCreationTime = fileCreationTime;
    entities.ElectronicFundTransmission.CompanyIdentificationIcd =
      companyIdentificationIcd;
    entities.ElectronicFundTransmission.CompanyIdentificationNumber =
      companyIdentificationNumber;
    entities.ElectronicFundTransmission.CompanyDescriptiveDate =
      companyDescriptiveDate;
    entities.ElectronicFundTransmission.EffectiveEntryDate = effectiveEntryDate;
    entities.ElectronicFundTransmission.ReceivingCompanyName =
      receivingCompanyName;
    entities.ElectronicFundTransmission.TraceNumber = traceNumber;
    entities.ElectronicFundTransmission.ApplicationIdentifier =
      applicationIdentifier;
    entities.ElectronicFundTransmission.CollectionAmount = collectionAmount;
    entities.ElectronicFundTransmission.ReceivingDfiAccountNumber =
      receivingDfiAccountNumber;
    entities.ElectronicFundTransmission.CompanyEntryDescription =
      companyEntryDescription;
    entities.ElectronicFundTransmission.Populated = true;
  }

  private bool ReadControlTable()
  {
    entities.InboundEftNumber.Populated = false;

    return Read("ReadControlTable",
      null,
      (db, reader) =>
      {
        entities.InboundEftNumber.Identifier = db.GetString(reader, 0);
        entities.InboundEftNumber.LastUsedNumber = db.GetInt32(reader, 1);
        entities.InboundEftNumber.Populated = true;
      });
  }

  private bool ReadEftTransmissionFileInfo()
  {
    entities.EftTransmissionFileInfo.Populated = false;

    return Read("ReadEftTransmissionFileInfo",
      (db, command) =>
      {
        db.SetInt32(command, "recordCount", local.EftTrailerRecord.Count);
        db.SetDecimal(
          command, "totalCurrency", local.EftTrailerRecord.TotalCurrency);
        db.SetInt32(command, "count", local.NbrOfDays.Count);
        db.SetDate(
          command, "date", local.EftHeaderRecord.Date.GetValueOrDefault());
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

  private bool ReadElectronicFundTransmission()
  {
    entities.ReadForDuplicates.Populated = false;

    return Read("ReadElectronicFundTransmission",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "fileCreationDate",
          local.EftHeaderRecord.Date.GetValueOrDefault());
        db.SetNullableTimeSpan(
          command, "fileCreationTime", local.EftHeaderRecord.Time);
        db.SetNullableInt32(
          command, "sequenceNumber",
          local.EftDetailRecord.SequenceNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReadForDuplicates.SequenceNumber =
          db.GetNullableInt32(reader, 0);
        entities.ReadForDuplicates.TransmissionType = db.GetString(reader, 1);
        entities.ReadForDuplicates.TransmissionIdentifier =
          db.GetInt32(reader, 2);
        entities.ReadForDuplicates.FileCreationDate =
          db.GetNullableDate(reader, 3);
        entities.ReadForDuplicates.FileCreationTime =
          db.GetNullableTimeSpan(reader, 4);
        entities.ReadForDuplicates.Populated = true;
      });
  }

  private bool ReadProgramProcessingInfo()
  {
    entities.ProgramProcessingInfo.Populated = false;

    return Read("ReadProgramProcessingInfo",
      null,
      (db, reader) =>
      {
        entities.ProgramProcessingInfo.Name = db.GetString(reader, 0);
        entities.ProgramProcessingInfo.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ProgramProcessingInfo.ParameterList =
          db.GetNullableString(reader, 2);
        entities.ProgramProcessingInfo.Populated = true;
      });
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = local.NextId.TransmissionIdentifier - 1;

    entities.InboundEftNumber.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.
          SetString(command, "cntlTblId", entities.InboundEftNumber.Identifier);
          
      });

    entities.InboundEftNumber.LastUsedNumber = lastUsedNumber;
    entities.InboundEftNumber.Populated = true;
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
    /// <summary>A TotalsGroup group.</summary>
    [Serializable]
    public class TotalsGroup
    {
      /// <summary>
      /// A value of NbrOfEftsRead.
      /// </summary>
      [JsonPropertyName("nbrOfEftsRead")]
      public Common NbrOfEftsRead
      {
        get => nbrOfEftsRead ??= new();
        set => nbrOfEftsRead = value;
      }

      /// <summary>
      /// A value of NbrOfEftsReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfEftsReceipted")]
      public Common NbrOfEftsReceipted
      {
        get => nbrOfEftsReceipted ??= new();
        set => nbrOfEftsReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfEftsPended.
      /// </summary>
      [JsonPropertyName("nbrOfEftsPended")]
      public Common NbrOfEftsPended
      {
        get => nbrOfEftsPended ??= new();
        set => nbrOfEftsPended = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecordsRead.
      /// </summary>
      [JsonPropertyName("amtOfEftRecordsRead")]
      public Common AmtOfEftRecordsRead
      {
        get => amtOfEftRecordsRead ??= new();
        set => amtOfEftRecordsRead = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecReceipted.
      /// </summary>
      [JsonPropertyName("amtOfEftRecReceipted")]
      public Common AmtOfEftRecReceipted
      {
        get => amtOfEftRecReceipted ??= new();
        set => amtOfEftRecReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfEftRecordsPended.
      /// </summary>
      [JsonPropertyName("amtOfEftRecordsPended")]
      public Common AmtOfEftRecordsPended
      {
        get => amtOfEftRecordsPended ??= new();
        set => amtOfEftRecordsPended = value;
      }

      /// <summary>
      /// A value of NbrOfCrReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfCrReceipted")]
      public Common NbrOfCrReceipted
      {
        get => nbrOfCrReceipted ??= new();
        set => nbrOfCrReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfNonCrReceipted.
      /// </summary>
      [JsonPropertyName("nbrOfNonCrReceipted")]
      public Common NbrOfNonCrReceipted
      {
        get => nbrOfNonCrReceipted ??= new();
        set => nbrOfNonCrReceipted = value;
      }

      /// <summary>
      /// A value of NbrOfIntCrMatched.
      /// </summary>
      [JsonPropertyName("nbrOfIntCrMatched")]
      public Common NbrOfIntCrMatched
      {
        get => nbrOfIntCrMatched ??= new();
        set => nbrOfIntCrMatched = value;
      }

      /// <summary>
      /// A value of NbrOfIntCrNotMatched.
      /// </summary>
      [JsonPropertyName("nbrOfIntCrNotMatched")]
      public Common NbrOfIntCrNotMatched
      {
        get => nbrOfIntCrNotMatched ??= new();
        set => nbrOfIntCrNotMatched = value;
      }

      /// <summary>
      /// A value of AmtOfCrReceipted.
      /// </summary>
      [JsonPropertyName("amtOfCrReceipted")]
      public Common AmtOfCrReceipted
      {
        get => amtOfCrReceipted ??= new();
        set => amtOfCrReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfNonCrReceipted.
      /// </summary>
      [JsonPropertyName("amtOfNonCrReceipted")]
      public Common AmtOfNonCrReceipted
      {
        get => amtOfNonCrReceipted ??= new();
        set => amtOfNonCrReceipted = value;
      }

      /// <summary>
      /// A value of AmtOfIntCrMatched.
      /// </summary>
      [JsonPropertyName("amtOfIntCrMatched")]
      public Common AmtOfIntCrMatched
      {
        get => amtOfIntCrMatched ??= new();
        set => amtOfIntCrMatched = value;
      }

      /// <summary>
      /// A value of AmtOfIntCrNotMatched.
      /// </summary>
      [JsonPropertyName("amtOfIntCrNotMatched")]
      public Common AmtOfIntCrNotMatched
      {
        get => amtOfIntCrNotMatched ??= new();
        set => amtOfIntCrNotMatched = value;
      }

      /// <summary>
      /// A value of NbrOfCrdRecorded.
      /// </summary>
      [JsonPropertyName("nbrOfCrdRecorded")]
      public Common NbrOfCrdRecorded
      {
        get => nbrOfCrdRecorded ??= new();
        set => nbrOfCrdRecorded = value;
      }

      /// <summary>
      /// A value of NbrOfCrdReleased.
      /// </summary>
      [JsonPropertyName("nbrOfCrdReleased")]
      public Common NbrOfCrdReleased
      {
        get => nbrOfCrdReleased ??= new();
        set => nbrOfCrdReleased = value;
      }

      /// <summary>
      /// A value of NbrOfCrdPended.
      /// </summary>
      [JsonPropertyName("nbrOfCrdPended")]
      public Common NbrOfCrdPended
      {
        get => nbrOfCrdPended ??= new();
        set => nbrOfCrdPended = value;
      }

      /// <summary>
      /// A value of NbrOfCrdSuspended.
      /// </summary>
      [JsonPropertyName("nbrOfCrdSuspended")]
      public Common NbrOfCrdSuspended
      {
        get => nbrOfCrdSuspended ??= new();
        set => nbrOfCrdSuspended = value;
      }

      /// <summary>
      /// A value of AmtOfCrdRecorded.
      /// </summary>
      [JsonPropertyName("amtOfCrdRecorded")]
      public Common AmtOfCrdRecorded
      {
        get => amtOfCrdRecorded ??= new();
        set => amtOfCrdRecorded = value;
      }

      /// <summary>
      /// A value of AmtOfCrdReleased.
      /// </summary>
      [JsonPropertyName("amtOfCrdReleased")]
      public Common AmtOfCrdReleased
      {
        get => amtOfCrdReleased ??= new();
        set => amtOfCrdReleased = value;
      }

      /// <summary>
      /// A value of AmtOfCrdPended.
      /// </summary>
      [JsonPropertyName("amtOfCrdPended")]
      public Common AmtOfCrdPended
      {
        get => amtOfCrdPended ??= new();
        set => amtOfCrdPended = value;
      }

      /// <summary>
      /// A value of AmtOfCrdSuspended.
      /// </summary>
      [JsonPropertyName("amtOfCrdSuspended")]
      public Common AmtOfCrdSuspended
      {
        get => amtOfCrdSuspended ??= new();
        set => amtOfCrdSuspended = value;
      }

      /// <summary>
      /// A value of NbrOfCrdFees.
      /// </summary>
      [JsonPropertyName("nbrOfCrdFees")]
      public Common NbrOfCrdFees
      {
        get => nbrOfCrdFees ??= new();
        set => nbrOfCrdFees = value;
      }

      /// <summary>
      /// A value of AmtOfCrdFees.
      /// </summary>
      [JsonPropertyName("amtOfCrdFees")]
      public Common AmtOfCrdFees
      {
        get => amtOfCrdFees ??= new();
        set => amtOfCrdFees = value;
      }

      /// <summary>
      /// A value of NbrOfReads.
      /// </summary>
      [JsonPropertyName("nbrOfReads")]
      public Common NbrOfReads
      {
        get => nbrOfReads ??= new();
        set => nbrOfReads = value;
      }

      /// <summary>
      /// A value of TotNbrOfReads.
      /// </summary>
      [JsonPropertyName("totNbrOfReads")]
      public Common TotNbrOfReads
      {
        get => totNbrOfReads ??= new();
        set => totNbrOfReads = value;
      }

      /// <summary>
      /// A value of NbrOfUpdates.
      /// </summary>
      [JsonPropertyName("nbrOfUpdates")]
      public Common NbrOfUpdates
      {
        get => nbrOfUpdates ??= new();
        set => nbrOfUpdates = value;
      }

      /// <summary>
      /// A value of NbrOfPendingErrors.
      /// </summary>
      [JsonPropertyName("nbrOfPendingErrors")]
      public Common NbrOfPendingErrors
      {
        get => nbrOfPendingErrors ??= new();
        set => nbrOfPendingErrors = value;
      }

      /// <summary>
      /// A value of NbrOfNonPendingErrors.
      /// </summary>
      [JsonPropertyName("nbrOfNonPendingErrors")]
      public Common NbrOfNonPendingErrors
      {
        get => nbrOfNonPendingErrors ??= new();
        set => nbrOfNonPendingErrors = value;
      }

      /// <summary>
      /// A value of TotNbrOfUpdates.
      /// </summary>
      [JsonPropertyName("totNbrOfUpdates")]
      public Common TotNbrOfUpdates
      {
        get => totNbrOfUpdates ??= new();
        set => totNbrOfUpdates = value;
      }

      /// <summary>
      /// A value of NbrOfCheckpoints.
      /// </summary>
      [JsonPropertyName("nbrOfCheckpoints")]
      public Common NbrOfCheckpoints
      {
        get => nbrOfCheckpoints ??= new();
        set => nbrOfCheckpoints = value;
      }

      private Common nbrOfEftsRead;
      private Common nbrOfEftsReceipted;
      private Common nbrOfEftsPended;
      private Common amtOfEftRecordsRead;
      private Common amtOfEftRecReceipted;
      private Common amtOfEftRecordsPended;
      private Common nbrOfCrReceipted;
      private Common nbrOfNonCrReceipted;
      private Common nbrOfIntCrMatched;
      private Common nbrOfIntCrNotMatched;
      private Common amtOfCrReceipted;
      private Common amtOfNonCrReceipted;
      private Common amtOfIntCrMatched;
      private Common amtOfIntCrNotMatched;
      private Common nbrOfCrdRecorded;
      private Common nbrOfCrdReleased;
      private Common nbrOfCrdPended;
      private Common nbrOfCrdSuspended;
      private Common amtOfCrdRecorded;
      private Common amtOfCrdReleased;
      private Common amtOfCrdPended;
      private Common amtOfCrdSuspended;
      private Common nbrOfCrdFees;
      private Common amtOfCrdFees;
      private Common nbrOfReads;
      private Common totNbrOfReads;
      private Common nbrOfUpdates;
      private Common nbrOfPendingErrors;
      private Common nbrOfNonPendingErrors;
      private Common totNbrOfUpdates;
      private Common nbrOfCheckpoints;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of NbrOfDays.
    /// </summary>
    [JsonPropertyName("nbrOfDays")]
    public Common NbrOfDays
    {
      get => nbrOfDays ??= new();
      set => nbrOfDays = value;
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
    /// A value of ForCreateElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("forCreateElectronicFundTransmission")]
    public ElectronicFundTransmission ForCreateElectronicFundTransmission
    {
      get => forCreateElectronicFundTransmission ??= new();
      set => forCreateElectronicFundTransmission = value;
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
    /// A value of EftHeaderRecord.
    /// </summary>
    [JsonPropertyName("eftHeaderRecord")]
    public DateWorkArea EftHeaderRecord
    {
      get => eftHeaderRecord ??= new();
      set => eftHeaderRecord = value;
    }

    /// <summary>
    /// A value of EftDetailRecord.
    /// </summary>
    [JsonPropertyName("eftDetailRecord")]
    public ElectronicFundTransmission EftDetailRecord
    {
      get => eftDetailRecord ??= new();
      set => eftDetailRecord = value;
    }

    /// <summary>
    /// A value of EftTrailerRecord.
    /// </summary>
    [JsonPropertyName("eftTrailerRecord")]
    public Common EftTrailerRecord
    {
      get => eftTrailerRecord ??= new();
      set => eftTrailerRecord = value;
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
    /// A value of EftRecsWritten.
    /// </summary>
    [JsonPropertyName("eftRecsWritten")]
    public Common EftRecsWritten
    {
      get => eftRecsWritten ??= new();
      set => eftRecsWritten = value;
    }

    /// <summary>
    /// A value of EftRecordsReleased.
    /// </summary>
    [JsonPropertyName("eftRecordsReleased")]
    public Common EftRecordsReleased
    {
      get => eftRecordsReleased ??= new();
      set => eftRecordsReleased = value;
    }

    /// <summary>
    /// A value of EftPreNotes.
    /// </summary>
    [JsonPropertyName("eftPreNotes")]
    public Common EftPreNotes
    {
      get => eftPreNotes ??= new();
      set => eftPreNotes = value;
    }

    /// <summary>
    /// A value of TotWarningsAndMsgs.
    /// </summary>
    [JsonPropertyName("totWarningsAndMsgs")]
    public Common TotWarningsAndMsgs
    {
      get => totWarningsAndMsgs ??= new();
      set => totWarningsAndMsgs = value;
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
    /// A value of NextId.
    /// </summary>
    [JsonPropertyName("nextId")]
    public ElectronicFundTransmission NextId
    {
      get => nextId ??= new();
      set => nextId = value;
    }

    /// <summary>
    /// A value of ForCreateDateWorkArea.
    /// </summary>
    [JsonPropertyName("forCreateDateWorkArea")]
    public DateWorkArea ForCreateDateWorkArea
    {
      get => forCreateDateWorkArea ??= new();
      set => forCreateDateWorkArea = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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
    /// A value of TraceIndicator.
    /// </summary>
    [JsonPropertyName("traceIndicator")]
    public Common TraceIndicator
    {
      get => traceIndicator ??= new();
      set => traceIndicator = value;
    }

    /// <summary>
    /// Gets a value of Totals.
    /// </summary>
    [JsonPropertyName("totals")]
    public TotalsGroup Totals
    {
      get => totals ?? (totals = new());
      set => totals = value;
    }

    private DateWorkArea initialized;
    private Common nbrOfDays;
    private EabFileHandling eabFileHandling;
    private EabFileHandling hold;
    private ElectronicFundTransmission forCreateElectronicFundTransmission;
    private EabReportSend eabReportSend;
    private DateWorkArea eftHeaderRecord;
    private ElectronicFundTransmission eftDetailRecord;
    private Common eftTrailerRecord;
    private Common eftRecsRead;
    private Common eftRecsWritten;
    private Common eftRecordsReleased;
    private Common eftPreNotes;
    private Common totWarningsAndMsgs;
    private Common eftTableTotalAmount;
    private ElectronicFundTransmission nextId;
    private DateWorkArea forCreateDateWorkArea;
    private DateWorkArea null1;
    private DateWorkArea process;
    private WorkArea workArea;
    private Common traceIndicator;
    private TotalsGroup totals;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ReadForDuplicates.
    /// </summary>
    [JsonPropertyName("readForDuplicates")]
    public ElectronicFundTransmission ReadForDuplicates
    {
      get => readForDuplicates ??= new();
      set => readForDuplicates = value;
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
    /// A value of EftTransmissionFileInfo.
    /// </summary>
    [JsonPropertyName("eftTransmissionFileInfo")]
    public EftTransmissionFileInfo EftTransmissionFileInfo
    {
      get => eftTransmissionFileInfo ??= new();
      set => eftTransmissionFileInfo = value;
    }

    /// <summary>
    /// A value of InboundEftNumber.
    /// </summary>
    [JsonPropertyName("inboundEftNumber")]
    public ControlTable InboundEftNumber
    {
      get => inboundEftNumber ??= new();
      set => inboundEftNumber = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private ElectronicFundTransmission readForDuplicates;
    private ElectronicFundTransmission electronicFundTransmission;
    private EftTransmissionFileInfo eftTransmissionFileInfo;
    private ControlTable inboundEftNumber;
  }
#endregion
}

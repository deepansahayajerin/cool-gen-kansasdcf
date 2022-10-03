// Program: FN_B794_NCP_PAYMENT_REMINDER, ID: 1902453725, model: 746.
// Short name: SWEF794B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B794_NCP_PAYMENT_REMINDER.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB794NcpPaymentReminder: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B794_NCP_PAYMENT_REMINDER program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB794NcpPaymentReminder(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB794NcpPaymentReminder.
  /// </summary>
  public FnB794NcpPaymentReminder(IContext context, Import import, Export export)
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
    // --------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------
    // 01/16/15  GVandy	CQ46307		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // In an effort to increase child support collections a postcard reminding 
    // NCPs that a child support payment is due will be sent to all NCPs who
    // have not made a payment in the preceding 45 days and have current support
    // or an arrears balance due.
    // The 4x6 postcards will be printed by Division of Printing using a tab 
    // delimited file containing the person number, name, and addresses of NCPs
    // supplied by ITS.
    // This batch job will be created to produce the tab delimited NCP file for 
    // the Division of Printing.  This batch job will be scheduled to run on the
    // first weekend of each month.  The number of calendar days in which a
    // payment has not been received will default to 45 but will be modifiable
    // via an MPPI parameter.
    // For an NCP to be included in the file all the criteria below must be 
    // satisfied:
    // 	1) No payments of any type have been received for the NCP within 45 
    // calendar
    // 	   days of the run date.
    // 	2) The NCP has a current support or arrears debt with a balance due,
    // 	   excluding future month current support debts.
    // 	3) The NCP has an active verified domestic address.
    // 	4) The NCP is not deceased.
    // 	5) At least one supported person on the debts is not deceased.
    // The file records will consist of the following data elements separated by
    // tab delimiters:
    // 	a) ncp person number
    // 	b) ncp first and last name
    // 	c) ncp street 1 and street 2 address
    // 	d) ncp city, state, zip code, and zip+4
    // 	
    // To document that the postcard was sent to the NCP, a HIST record will be 
    // created for each case where the NCP is the currently active AP.  Event 48
    // Event Detail 002 (Monthly NCP Payment Reminder Postcard) will be used
    // for the HIST entry.
    // --------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // -------------------------------------------------------------------------------------
    // --  Read the PPI Record.
    // -------------------------------------------------------------------------------------
    local.ProgramProcessingInfo.Name = global.UserId;
    UseReadProgramProcessingInfo();

    // ------------------------------------------------------------------------------
    // -- Read for restart info.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.ProgramName = global.UserId;
    UseReadPgmCheckpointRestart();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    local.EabReportSend.ProgramName = global.UserId;
    local.EabReportSend.ProcessDate = local.ProgramProcessingInfo.ProcessDate;
    UseCabErrorReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "FN0000_ERROR_OPENING_ERROR_RPT";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- This could have resulted from not finding the PPI record.
      // -- Extract the exit state message and write to the error report.
      UseEabExtractExitStateMessage();
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Initialization Error..." + local
        .ExitStateWorkArea.Message;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Open the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport3();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error opening the control report.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Determine if we're restarting.
    // ------------------------------------------------------------------------------
    if (AsChar(local.ProgramCheckpointRestart.RestartInd) == 'Y')
    {
      // -------------------------------------------------------------------------------------
      //  Checkpoint Info...
      // 	Position  Description
      // 	--------  
      // -----------------------------------------
      // 	001-010   Last NCP Number Processed
      // 	011-011   Blank
      // 	012-020   Total Number of NCPs Written to File
      // -------------------------------------------------------------------------------------
      local.RestartNcp.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.TotalNumbOfNcpsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9));

      // -- Log restart data to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 4; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Restarting at NCP person number " + local.RestartNcp.Number;

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "Number of NCPs previously written to file " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9);

            break;
          default:
            local.EabReportSend.RptDetail = "";

            break;
        }

        local.EabFileHandling.Action = "WRITE";
        UseCabControlReport2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          // -- Write to the error report.
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error Writing Restart Info to the Control Report...  Returned Status = " +
            local.EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
      }

      // -------------------------------------------------------------------------------------
      // --  Extend the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "EXTEND";
      UseFnB794WriteToNcpFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }
    else
    {
      local.RestartNcp.Number = "";
      local.TotalNumbOfNcpsWritten.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseFnB794WriteToNcpFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error opening output file.  Return status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // -- Determine the number of days during which a payment must not have been
    // collected.
    // -------------------------------------------------------------------------------------
    if (IsEmpty(local.ProgramProcessingInfo.ParameterList))
    {
      // -- Default is 45 days.
      local.NumberOfNonPaymentDays.Count = 45;
    }
    else
    {
      // -- Use value in first 3 characters of MPPI parameter.
      local.NumberOfNonPaymentDays.Count =
        (int)StringToNumber(Substring(
          local.ProgramProcessingInfo.ParameterList, 1, 3));
    }

    // -------------------------------------------------------------------------------------
    // -- Determine the cutoff date for when a payment must have been collected.
    // -------------------------------------------------------------------------------------
    local.PaymentCutoffDate.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(local.NumberOfNonPaymentDays.Count - 1));

    // -------------------------------------------------------------------------------------
    // -- Determine the first day of the month following the ppi processing 
    // date.
    // -- The NCP must have a debt due prior to this date.
    // -------------------------------------------------------------------------------------
    local.FirstOfNextMonth.Date =
      AddDays(local.ProgramProcessingInfo.ProcessDate,
      -(Day(local.ProgramProcessingInfo.ProcessDate) - 1));
    local.FirstOfNextMonth.Date = AddMonths(local.FirstOfNextMonth.Date, 1);

    // -------------------------------------------------------------------------------------
    // -- Log the number of days, payment cutoff date, and debt due date to the 
    // control report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of Days during which a Payment has not been received...." +
            NumberToString(local.NumberOfNonPaymentDays.Count, 9, 7);

          break;
        case 3:
          UseCabDate2TextWithHyphens1();
          local.EabReportSend.RptDetail =
            "NCP Payment Collection Date Cutoff............................." +
            local.TextDate.Text10;

          break;
        case 5:
          UseCabDate2TextWithHyphens2();
          local.EabReportSend.RptDetail =
            "NCP must have a Debt Due on or before.........................." +
            local.TextDate.Text10;

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(01) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    local.NumbOfNcpsSinceChckpnt.Count = 0;

    // -------------------------------------------------------------------------------------
    // -- Find each NCP with an existing balance on a debt due before the first 
    // of the next month.
    // -- This read each is set to retrieve DISTINCT NCP person numbers.
    // -------------------------------------------------------------------------------------
    foreach(var item in ReadCsePersonObligor())
    {
      // -- Determine if NCP has made a payment within the allotted time period.
      if (ReadCashReceiptDetail())
      {
        // -- NCP has made a payment.  Skip this NCP.
        continue;
      }
      else
      {
        // -- NCP has NOT made a payment.  Continue.
      }

      if (ReadCsePerson())
      {
        if (Lt(local.NullDateWorkArea.Date, entities.CsePerson.DateOfDeath))
        {
          // -- Skip if NCP is deceased.
          continue;
        }
      }

      if (ReadCsePersonAddress())
      {
        local.NcpCsePersonAddress.Assign(entities.NcpCsePersonAddress);

        if (AsChar(entities.NcpCsePersonAddress.LocationType) == 'F')
        {
          // -- Skip NCPs with foreign addresses.
          continue;
        }
      }

      if (!entities.NcpCsePersonAddress.Populated)
      {
        // -- Skip if NCP has no verified address
        continue;
      }

      // -- Get NCP name from adabase
      local.NcpCsePersonsWorkSet.Number = entities.NcpCsePerson.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
          .NcpCsePerson.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // --  Concatenate the person number <tab> name <tab> street 1 & 2 <tab> 
      // city state zip zip+4 and view match to the EAB.
      // -- In the concat statements below the <tab> character does not display.
      //    If you double click on one of the "" values you will see it is a 
      // <tab> character.
      //    You put the <tab> character into a character value by pressing CTRL 
      // TAB.
      local.NcpPostcard.RptDetail = local.NcpCsePersonsWorkSet.Number;
      local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + "\t";
      local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + TrimEnd
        (local.NcpCsePersonsWorkSet.FirstName) + " " + local
        .NcpCsePersonsWorkSet.LastName;
      local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + "\t";
      local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + TrimEnd
        (local.NcpCsePersonAddress.Street1) + " " + (
          local.NcpCsePersonAddress.Street2 ?? "");
      local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + "\t";

      if (IsEmpty(local.NcpCsePersonAddress.Zip4))
      {
        local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + TrimEnd
          (local.NcpCsePersonAddress.City) + ", " + (
            local.NcpCsePersonAddress.State ?? "") + " " + (
            local.NcpCsePersonAddress.ZipCode ?? "") + " " + (
            local.NcpCsePersonAddress.Zip4 ?? "");
      }
      else
      {
        local.NcpPostcard.RptDetail = TrimEnd(local.NcpPostcard.RptDetail) + TrimEnd
          (local.NcpCsePersonAddress.City) + ", " + (
            local.NcpCsePersonAddress.State ?? "") + " " + (
            local.NcpCsePersonAddress.ZipCode ?? "") + "-" + (
            local.NcpCsePersonAddress.Zip4 ?? "");
      }

      // -- Determine the record length being passed to the external.
      //    This is need in the external IF varying length records are to be 
      // produced.
      local.RecordLength.Count = Length(TrimEnd(local.NcpPostcard.RptDetail));
      local.EabFileHandling.Action = "WRITE";
      UseFnB794WriteToNcpFile1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error Writing NCP File...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_RC_AB0008_INVALID_RETURN_CD";

        return;
      }

      // -- Increment the NCP count.
      ++local.TotalNumbOfNcpsWritten.Count;
      ++local.NumbOfNcpsSinceChckpnt.Count;

      // -- Create a HIST record on each CASE indicating the postcard was sent.
      foreach(var item1 in ReadCase())
      {
        local.Infrastructure.EventId = 48;
        local.Infrastructure.ReasonCode = "REMINDPOSTCARD";
        local.Infrastructure.Detail = "NCP Payment Reminder Postcard Sent";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CsePersonNumber = entities.NcpCsePerson.Number;
        local.Infrastructure.CaseNumber = entities.Case1.Number;

        if (ReadCaseUnit())
        {
          local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        }
        else
        {
          continue;
        }

        local.Infrastructure.CsenetInOutCode = "";
        local.Infrastructure.InitiatingStateCode = "KS";
        local.Infrastructure.ReferenceDate =
          local.ProgramProcessingInfo.ProcessDate;
        local.Infrastructure.UserId = global.UserId;
        UseSpCabCreateInfrastructure();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }

      // -- File Commit and Checkpoint processing.
      if (local.NumbOfNcpsSinceChckpnt.Count > local
        .ProgramCheckpointRestart.UpdateFrequencyCount.GetValueOrDefault())
      {
        // --  COMMIT changes to the Output File.
        local.EabFileHandling.Action = "COMMIT";
        UseFnB794WriteToNcpFile2();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail =
            "Error on COMMIT for output file.  Return status = " + local
            .EabFileHandling.Status;
          UseCabErrorReport2();

          // -- Set Abort exit state and escape...
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        // -- Checkpoint.
        // -------------------------------------------------------------------------------------
        //  Checkpoint Info...
        // 	Position  Description
        // 	--------  
        // -----------------------------------------
        // 	001-010   Last NCP Number Processed
        // 	011-011   Blank
        // 	012-020   Total Number of NCPs Written to File
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo =
          entities.NcpCsePerson.Number;
        local.ProgramCheckpointRestart.RestartInfo =
          entities.NcpCsePerson.Number + " " + NumberToString
          (local.TotalNumbOfNcpsWritten.Count, 7, 9);
        UseUpdateCheckpointRstAndCommit();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = "Error taking final checkpoint.";
          UseCabErrorReport2();
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }

        local.NumbOfNcpsSinceChckpnt.Count = 0;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 3; ++local.Common.Count)
    {
      if (local.Common.Count == 1)
      {
        local.EabReportSend.RptDetail =
          "Number of NCPs Written to the NCP Output File.................." + NumberToString
          (local.TotalNumbOfNcpsWritten.Count, 9, 7);
      }
      else
      {
        local.EabReportSend.RptDetail = "";
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        // -- Write to the error report.
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "(02) Error Writing Control Report...  Returned Status = " + local
          .EabFileHandling.Status;
        UseCabErrorReport2();

        // -- Set Abort exit state and escape...
        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

        return;
      }
    }

    // -------------------------------------------------------------------------------------
    // --  Do a final Commit to the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "COMMIT";
    UseFnB794WriteToNcpFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error on final COMMIT for output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // ------------------------------------------------------------------------------
    // -- Take a final checkpoint.
    // ------------------------------------------------------------------------------
    local.ProgramCheckpointRestart.RestartInd = "N";
    local.ProgramCheckpointRestart.RestartInfo = "";
    UseUpdateCheckpointRstAndCommit();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail = "Error taking final checkpoint.";
      UseCabErrorReport2();
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Output File.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseFnB794WriteToNcpFile2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error closing output file.  Return status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Control Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      // -- Write to the error report.
      local.EabFileHandling.Action = "WRITE";
      local.EabReportSend.RptDetail =
        "Error Closing Control Report...  Returned Status = " + local
        .EabFileHandling.Status;
      UseCabErrorReport2();

      // -- Set Abort exit state and escape...
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close the Error Report.
    // -------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseCabErrorReport1();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "SI0000_ERR_CLOSING_ERR_RPT_FILE";

      return;
    }

    // -------------------------------------------------------------------------------------
    // --  Close Adabas.
    // ---------------------------------------------------------------------------
    local.NcpCsePersonsWorkSet.Number = "CLOSE";
    UseEabReadCsePersonBatch();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.PaymentCutoffDate.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.FirstOfNextMonth.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.TextDate.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabErrorReport1()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport2()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport3()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);

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

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnB794WriteToNcpFile1()
  {
    var useImport = new FnB794WriteToNcpFile.Import();
    var useExport = new FnB794WriteToNcpFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.EabReportSend.RptDetail = local.NcpPostcard.RptDetail;
    useImport.RecordLength.Count = local.RecordLength.Count;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB794WriteToNcpFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB794WriteToNcpFile2()
  {
    var useImport = new FnB794WriteToNcpFile.Import();
    var useExport = new FnB794WriteToNcpFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB794WriteToNcpFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
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

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.NcpCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.NcpCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseUpdateCheckpointRstAndCommit()
  {
    var useImport = new UpdateCheckpointRstAndCommit.Import();
    var useExport = new UpdateCheckpointRstAndCommit.Export();

    useImport.ProgramCheckpointRestart.Assign(local.ProgramCheckpointRestart);

    Call(UpdateCheckpointRstAndCommit.Execute, useImport, useExport);
  }

  private IEnumerable<bool> ReadCase()
  {
    entities.Case1.Populated = false;

    return ReadEach("ReadCase",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 1);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(
          command, "oblgorPrsnNbr", entities.NcpCsePerson.Number);
        db.SetDate(
          command, "collectionDate",
          local.PaymentCutoffDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 4);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.NcpCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.NcpCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.NcpCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          local.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NcpCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.NcpCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.NcpCsePersonAddress.Street1 = db.GetNullableString(reader, 2);
        entities.NcpCsePersonAddress.Street2 = db.GetNullableString(reader, 3);
        entities.NcpCsePersonAddress.City = db.GetNullableString(reader, 4);
        entities.NcpCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 5);
        entities.NcpCsePersonAddress.EndDate = db.GetNullableDate(reader, 6);
        entities.NcpCsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.NcpCsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.NcpCsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.NcpCsePersonAddress.LocationType = db.GetString(reader, 10);
        entities.NcpCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.NcpCsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.Obligor.Populated = false;
    entities.NcpCsePerson.Populated = false;

    return ReadEach("ReadCsePersonObligor",
      (db, command) =>
      {
        db.SetString(command, "numb", local.RestartNcp.Number);
        db.SetNullableDate(
          command, "dateOfDeath",
          local.NullDateWorkArea.Date.GetValueOrDefault());
        db.SetDate(
          command, "dueDt", local.FirstOfNextMonth.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NcpCsePerson.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Obligor.Populated = true;
        entities.NcpCsePerson.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

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
    /// A value of NumbOfNcpsSinceChckpnt.
    /// </summary>
    [JsonPropertyName("numbOfNcpsSinceChckpnt")]
    public Common NumbOfNcpsSinceChckpnt
    {
      get => numbOfNcpsSinceChckpnt ??= new();
      set => numbOfNcpsSinceChckpnt = value;
    }

    /// <summary>
    /// A value of RestartNcp.
    /// </summary>
    [JsonPropertyName("restartNcp")]
    public CsePerson RestartNcp
    {
      get => restartNcp ??= new();
      set => restartNcp = value;
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
    /// A value of NullInfrastructure.
    /// </summary>
    [JsonPropertyName("nullInfrastructure")]
    public Infrastructure NullInfrastructure
    {
      get => nullInfrastructure ??= new();
      set => nullInfrastructure = value;
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
    /// A value of RecordLength.
    /// </summary>
    [JsonPropertyName("recordLength")]
    public Common RecordLength
    {
      get => recordLength ??= new();
      set => recordLength = value;
    }

    /// <summary>
    /// A value of TextDate.
    /// </summary>
    [JsonPropertyName("textDate")]
    public TextWorkArea TextDate
    {
      get => textDate ??= new();
      set => textDate = value;
    }

    /// <summary>
    /// A value of NumberOfNonPaymentDays.
    /// </summary>
    [JsonPropertyName("numberOfNonPaymentDays")]
    public Common NumberOfNonPaymentDays
    {
      get => numberOfNonPaymentDays ??= new();
      set => numberOfNonPaymentDays = value;
    }

    /// <summary>
    /// A value of PaymentCutoffDate.
    /// </summary>
    [JsonPropertyName("paymentCutoffDate")]
    public DateWorkArea PaymentCutoffDate
    {
      get => paymentCutoffDate ??= new();
      set => paymentCutoffDate = value;
    }

    /// <summary>
    /// A value of NcpPostcard.
    /// </summary>
    [JsonPropertyName("ncpPostcard")]
    public EabReportSend NcpPostcard
    {
      get => ncpPostcard ??= new();
      set => ncpPostcard = value;
    }

    /// <summary>
    /// A value of NcpCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("ncpCsePersonsWorkSet")]
    public CsePersonsWorkSet NcpCsePersonsWorkSet
    {
      get => ncpCsePersonsWorkSet ??= new();
      set => ncpCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of FirstOfNextMonth.
    /// </summary>
    [JsonPropertyName("firstOfNextMonth")]
    public DateWorkArea FirstOfNextMonth
    {
      get => firstOfNextMonth ??= new();
      set => firstOfNextMonth = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
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
    /// A value of TotalNumbOfNcpsWritten.
    /// </summary>
    [JsonPropertyName("totalNumbOfNcpsWritten")]
    public Common TotalNumbOfNcpsWritten
    {
      get => totalNumbOfNcpsWritten ??= new();
      set => totalNumbOfNcpsWritten = value;
    }

    private Common numbOfNcpsSinceChckpnt;
    private CsePerson restartNcp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure nullInfrastructure;
    private Infrastructure infrastructure;
    private Common recordLength;
    private TextWorkArea textDate;
    private Common numberOfNonPaymentDays;
    private DateWorkArea paymentCutoffDate;
    private EabReportSend ncpPostcard;
    private CsePersonsWorkSet ncpCsePersonsWorkSet;
    private CsePersonAddress ncpCsePersonAddress;
    private DateWorkArea nullDateWorkArea;
    private DateWorkArea firstOfNextMonth;
    private Common common;
    private ProgramProcessingInfo programProcessingInfo;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private ExitStateWorkArea exitStateWorkArea;
    private Common totalNumbOfNcpsWritten;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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
    /// A value of NcpCsePersonAddress.
    /// </summary>
    [JsonPropertyName("ncpCsePersonAddress")]
    public CsePersonAddress NcpCsePersonAddress
    {
      get => ncpCsePersonAddress ??= new();
      set => ncpCsePersonAddress = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePersonAccount Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of NcpCsePerson.
    /// </summary>
    [JsonPropertyName("ncpCsePerson")]
    public CsePerson NcpCsePerson
    {
      get => ncpCsePerson ??= new();
      set => ncpCsePerson = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePerson Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private Case1 case1;
    private CashReceiptDetail cashReceiptDetail;
    private CsePersonAddress ncpCsePersonAddress;
    private CsePerson csePerson;
    private Obligation obligation;
    private CsePersonAccount supported1;
    private CsePersonAccount obligor;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson ncpCsePerson;
    private CsePerson supported2;
  }
#endregion
}

// Program: FN_B796_NCP_TEXT_MESSAGE, ID: 1902526472, model: 746.
// Short name: SWEF796B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B796_NCP_TEXT_MESSAGE.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnB796NcpTextMessage: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B796_NCP_TEXT_MESSAGE program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB796NcpTextMessage(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB796NcpTextMessage.
  /// </summary>
  public FnB796NcpTextMessage(IContext context, Import import, Export export):
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
    // 03/21/16  GVandy	CQ51336		Initial Development.
    // --------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------
    // Business Rules
    // In an effort to increase child support collections a text message 
    // reminding NCPs that a child support payment is due will be sent to all
    // NCPs who have not made a payment in the preceding 45 days and have
    // current support or an arrears balance due.
    // The text messages will be sent by outside vendor Contact Wireless using a
    // comma delimited file containing the NCPs cell phone number, person
    // number, name, and balance information supplied by ITS.
    // This program will be scheduled to run on the first weekend of each month 
    // and will produce the comma dlimited NCP file for Contact Wireless.  The
    // number of calendar days in which a payment has not been received will
    // default to 45 but will be modifiable via an MPPI parameter.
    // For an NCP to be included in the file all the criteria below must be 
    // satisfied:
    // 	1) No payments of any type have been received for the NCP within 45 
    // calendar
    // 	   days of the run date.
    // 	2) The NCP has a non secondary current support or arrears debt with a 
    // balance
    // 	   due, excluding future month current support debts.
    // 	3) The NCP is not deceased.
    // 	4) At least one supported person on the debts is not deceased.
    // 	5) The NCP has a 10 digit Cellphone number
    // 	6) The text message consent flag for the NCP is not "N"
    // 	7) The case is not an outgoing interstate case.
    // For each NCP, non-secondary current support and arrears debt balances 
    // will be calculated by court order.  To provide consistency, the OPAY
    // display action blocks will be used to calculate the balances per
    // obligation and then summarized by court order.  Separate records will be
    // written to the file for each NCP court order which will facilitate an
    // individual text message being sent containing the balance due for each
    // court order.
    // The file records will consist of the following data elements separated by
    // comma delimiters:
    // 	a) 10 digit cell phone number
    // 	b) ncp person number
    // 	c) tribunal county abbreviation for the court order plus court order 
    // number
    // 	d) name of the current month
    // 	e) current support balance due (excluding future current support)
    // 	f) name of month through which arrears are calculated
    // 	g) arrears balance due
    // To document that the text message was sent to the NCP, a HIST record will
    // be created for each case where the NCP is the currently active AP.
    // Event 48 Event Detail 003 (Monthly NCP Payment Reminder Text Msg) will be
    // used for the HIST entry.
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
      // 	012-020   Total Number of NCPs to Whom Text Messages Have been Sent
      // 	021-021   Blank
      // 	022-030   Total Number of Text Messages Written to File
      // -------------------------------------------------------------------------------------
      local.RestartNcp.Number =
        Substring(local.ProgramCheckpointRestart.RestartInfo, 1, 10);
      local.TotalNumbOfNcpsWritten.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9));
      local.TotalNumbOfTextMsgs.Count =
        (int)StringToNumber(Substring(
          local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9));

      // -- Log restart data to the control report.
      for(local.Common.Count = 1; local.Common.Count <= 6; ++local.Common.Count)
      {
        switch(local.Common.Count)
        {
          case 1:
            local.EabReportSend.RptDetail =
              "Restarting at NCP person number " + local.RestartNcp.Number;

            break;
          case 2:
            local.EabReportSend.RptDetail =
              "Number of NCPs for whom text messages were previously written to file " +
              Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 12, 9);

            break;
          case 4:
            local.EabReportSend.RptDetail =
              "Number of text messages previously written to file " + Substring
              (local.ProgramCheckpointRestart.RestartInfo, 250, 22, 9);

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
      UseFnB796WriteToTextMsgFile2();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail =
          "Error extending output file.  Return status = " + local
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
      local.TotalNumbOfTextMsgs.Count = 0;

      // -------------------------------------------------------------------------------------
      // --  Open the Output File.
      // -------------------------------------------------------------------------------------
      local.EabFileHandling.Action = "OPEN";
      UseFnB796WriteToTextMsgFile2();

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
    if (IsEmpty(Substring(local.ProgramProcessingInfo.ParameterList, 1, 3)))
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
    // -- Determine if excluded NCPs should be logged to the error report.
    // -------------------------------------------------------------------------------------
    if (CharAt(local.ProgramProcessingInfo.ParameterList, 4) == 'Y')
    {
      // -- Log excluded NCPs to the error report
      local.LogExclusions.Flag = "Y";
    }
    else
    {
      // -- Do Not log excluded NCPs to the error report
      local.LogExclusions.Flag = "N";
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
    // -- Determine Arrears Through Month and Current Month.
    // -------------------------------------------------------------------------------------
    switch(Month(local.ProgramProcessingInfo.ProcessDate))
    {
      case 1:
        local.CurrentSupportMonth.Text11 = "January";
        local.ArrearsThroughMonth.Text11 = "December";

        break;
      case 2:
        local.CurrentSupportMonth.Text11 = "February";
        local.ArrearsThroughMonth.Text11 = "January";

        break;
      case 3:
        local.CurrentSupportMonth.Text11 = "March";
        local.ArrearsThroughMonth.Text11 = "February";

        break;
      case 4:
        local.CurrentSupportMonth.Text11 = "April";
        local.ArrearsThroughMonth.Text11 = "March";

        break;
      case 5:
        local.CurrentSupportMonth.Text11 = "May";
        local.ArrearsThroughMonth.Text11 = "April";

        break;
      case 6:
        local.CurrentSupportMonth.Text11 = "June";
        local.ArrearsThroughMonth.Text11 = "May";

        break;
      case 7:
        local.CurrentSupportMonth.Text11 = "July";
        local.ArrearsThroughMonth.Text11 = "June";

        break;
      case 8:
        local.CurrentSupportMonth.Text11 = "August";
        local.ArrearsThroughMonth.Text11 = "July";

        break;
      case 9:
        local.CurrentSupportMonth.Text11 = "September";
        local.ArrearsThroughMonth.Text11 = "August";

        break;
      case 10:
        local.CurrentSupportMonth.Text11 = "October";
        local.ArrearsThroughMonth.Text11 = "September";

        break;
      case 11:
        local.CurrentSupportMonth.Text11 = "November";
        local.ArrearsThroughMonth.Text11 = "October";

        break;
      case 12:
        local.CurrentSupportMonth.Text11 = "December";
        local.ArrearsThroughMonth.Text11 = "November";

        break;
      default:
        break;
    }

    // -------------------------------------------------------------------------------------
    // -- Log the number of days, payment cutoff date, debt due date, current 
    // support month,
    // -- and arrears through month to the control report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 12; ++local.Common.Count)
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
        case 7:
          local.EabReportSend.RptDetail =
            "Current Support Month.........................................." +
            local.CurrentSupportMonth.Text11;

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "Arrears Through Month.........................................." +
            local.ArrearsThroughMonth.Text11;

          break;
        case 11:
          local.EabReportSend.RptDetail =
            "Log Excluded NCPs to the Error Report.........................." +
            local.LogExclusions.Flag;

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
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.Ncp.Number + " - Payment was made during the payment period";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

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
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.Ncp.Number + " - NCP is deceased";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          continue;
        }

        if (AsChar(entities.CsePerson.TextMessageIndicator) == 'N')
        {
          // -- Skip if NCP has refused text messaging.
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.Ncp.Number + " - NCP has refused text messaging";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          continue;
        }

        if (AsChar(entities.CsePerson.OtherPhoneType) != 'C' || Equal
          (entities.CsePerson.OtherAreaCode, 0) || Equal
          (entities.CsePerson.OtherNumber, 0))
        {
          // -- Skip if NCP has no cell phone number.
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.Ncp.Number + " - NCP has no cell phone number";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          continue;
        }
      }
      else
      {
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.Ncp.Number + " - Internal error reading NCP ";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        continue;
      }

      // -- Skip outgoing interstate cases.
      foreach(var item1 in ReadCase())
      {
        if (ReadInterstateRequest())
        {
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.Ncp.Number + " - Outgoing interstate case";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          goto ReadEach;
        }
      }

      // -- Get NCP name from adabase
      local.Ncp.Number = entities.Ncp.Number;
      UseSiReadCsePerson();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        local.EabFileHandling.Action = "WRITE";
        local.EabReportSend.RptDetail = "Error Reading ADABAS...NCP " + entities
          .Ncp.Number + " " + local.ExitStateWorkArea.Message;
        UseCabErrorReport2();

        if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

          return;
        }
        else
        {
          if (AsChar(local.LogExclusions.Flag) == 'Y')
          {
            local.EabFileHandling.Action = "WRITE";
            local.EabReportSend.RptDetail = entities.Ncp.Number + " - Error reading NCP in Adabas";
              
            UseCabErrorReport2();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

              return;
            }
          }

          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
      }

      // -- Initialize summary group
      for(local.Summary.Index = 0; local.Summary.Index < local.Summary.Count; ++
        local.Summary.Index)
      {
        if (!local.Summary.CheckSize())
        {
          break;
        }

        local.Summary.Update.SummaryArrearsOwed.TotalCurrency = 0;
        local.Summary.Update.SummaryCurrentOwed.TotalCurrency = 0;
        local.Summary.Update.SummaryTotalDue.TotalCurrency = 0;
        local.Summary.Update.SummaryAction.StandardNumber = "";
        local.Summary.Update.SummaryAction.CourtCaseNumber = "";
        local.Summary.Update.SummaryAction.Identifier = 0;
      }

      local.Summary.CheckIndex();
      local.Summary.Count = 0;

      // -- use OPAY cab to gather debt amounts by obligtion.
      UseFnDisplayObligationsByPayor();

      // -- Summarize amounts for each returned court order, excluding secondary
      // obligations.
      for(local.Local1.Index = 0; local.Local1.Index < local.Local1.Count; ++
        local.Local1.Index)
      {
        if (!local.Local1.CheckSize())
        {
          break;
        }

        if (AsChar(local.Local1.Item.DetailObligation.PrimarySecondaryCode) == 'S'
          )
        {
          // -- Do not include balances for secondary obligations.
          continue;
        }

        for(local.Summary.Index = 0; local.Summary.Index < local.Summary.Count; ++
          local.Summary.Index)
        {
          if (!local.Summary.CheckSize())
          {
            break;
          }

          if (Equal(local.Summary.Item.SummaryAction.StandardNumber,
            local.Local1.Item.DetailLegalAction.StandardNumber))
          {
            // -- Standard number already exists in the summary group.  Add to 
            // the totals.
            local.Summary.Update.SummaryCurrentOwed.TotalCurrency =
              local.Summary.Item.SummaryCurrentOwed.TotalCurrency + local
              .Local1.Item.DetailCurrentOwed.TotalCurrency;
            local.Summary.Update.SummaryArrearsOwed.TotalCurrency =
              local.Summary.Item.SummaryArrearsOwed.TotalCurrency + local
              .Local1.Item.DetailArrearsOwed.TotalCurrency;
            local.Summary.Update.SummaryTotalDue.TotalCurrency =
              local.Summary.Item.SummaryTotalDue.TotalCurrency + local
              .Local1.Item.DetailTotalDue.TotalCurrency;

            goto Next;
          }
        }

        local.Summary.CheckIndex();

        // -- Standard number does not already exist in the summary group.
        //    Add a new entry to the group for this standard number.
        local.Summary.Index = local.Summary.Count;
        local.Summary.CheckSize();

        local.Summary.Update.SummaryAction.Assign(
          local.Local1.Item.DetailLegalAction);
        local.Summary.Update.SummaryCurrentOwed.TotalCurrency =
          local.Local1.Item.DetailCurrentOwed.TotalCurrency;
        local.Summary.Update.SummaryArrearsOwed.TotalCurrency =
          local.Local1.Item.DetailArrearsOwed.TotalCurrency;
        local.Summary.Update.SummaryTotalDue.TotalCurrency =
          local.Local1.Item.DetailTotalDue.TotalCurrency;

Next:
        ;
      }

      local.Local1.CheckIndex();

      if (local.Summary.Count <= 0)
      {
        // -- There are no non-secondary debts due.
        if (AsChar(local.LogExclusions.Flag) == 'Y')
        {
          local.EabFileHandling.Action = "WRITE";
          local.EabReportSend.RptDetail = entities.Ncp.Number + " - NCP has only secondary debts";
            
          UseCabErrorReport2();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

            return;
          }
        }

        continue;
      }

      // -- Write the info for each court order to the file.
      local.Summary.Index = 0;

      for(var limit = local.Summary.Count; local.Summary.Index < limit; ++
        local.Summary.Index)
      {
        if (!local.Summary.CheckSize())
        {
          break;
        }

        // --  Concatenate the following for the text message record
        // Phone#,Person#,Tribunal County/Court Order #,Current Support Month,
        // Current Support Balance,Arrears Through Mohth,Arrears Balance,Total
        // Balance Due,NCP First Name,NCP Last Name.
        // -- General Formatting Note:  Any value with an embedded comma will 
        // get enclosed in double quotes.
        // -- NCP Phone number
        local.TextMessageFile.Text200 =
          NumberToString(entities.CsePerson.OtherAreaCode.GetValueOrDefault(),
          13, 3);
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + NumberToString
          (entities.CsePerson.OtherNumber.GetValueOrDefault(), 9, 7);
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- NCP Person number
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + entities.CsePerson.Number;
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        if (!ReadFipsLegalAction2())
        {
          if (!ReadFipsLegalAction1())
          {
            if (AsChar(local.LogExclusions.Flag) == 'Y')
            {
              local.EabFileHandling.Action = "WRITE";
              local.EabReportSend.RptDetail = entities.Ncp.Number + " - Legal Action not found for debt.  Standard number " +
                (local.Summary.Item.SummaryAction.StandardNumber ?? "");
              UseCabErrorReport2();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_ERROR_RPT";

                return;
              }
            }

            continue;
          }
        }

        // -- Tribunal County
        if (Equal(entities.Fips.StateAbbreviation, "KS"))
        {
          local.County.Text16 = entities.Fips.CountyAbbreviation ?? Spaces(16);
        }
        else
        {
          local.County.Text16 = "Non-Kansas Order";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + local.County.Text16;
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Legal Action Standard Number
        if (Find(entities.LegalAction.CourtCaseNumber, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + entities
            .LegalAction.StandardNumber;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + entities
            .LegalAction.StandardNumber;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Current Support Month
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + local
          .CurrentSupportMonth.Text11;
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Current Support Due
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.Summary.Item.SummaryCurrentOwed.
            TotalCurrency * 100), 15);
        UseEabConvertNumeric1();
        local.WorkArea.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));

        if (Find(local.EabConvertNumeric.ReturnCurrencySigned, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Arrears Through Month
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + local
          .ArrearsThroughMonth.Text11;
        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Arrears Balance
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.Summary.Item.SummaryArrearsOwed.
            TotalCurrency * 100), 15);
        UseEabConvertNumeric1();
        local.WorkArea.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));

        if (Find(local.EabConvertNumeric.ReturnCurrencySigned, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- Total Balance Due
        local.EabConvertNumeric.SendNonSuppressPos = 3;
        local.EabConvertNumeric.SendAmount =
          NumberToString((long)(local.Summary.Item.SummaryTotalDue.
            TotalCurrency * 100), 15);
        UseEabConvertNumeric1();
        local.WorkArea.Text17 = "$" + Substring
          (local.EabConvertNumeric.ReturnCurrencySigned,
          EabConvertNumeric2.ReturnCurrencySigned_MaxLength,
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "), 22 -
          Verify(local.EabConvertNumeric.ReturnCurrencySigned, " "));

        if (Find(local.EabConvertNumeric.ReturnCurrencySigned, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.WorkArea.Text17;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- NCP First Name
        if (Find(local.Ncp.FirstName, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.Ncp.FirstName;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.Ncp.FirstName;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        local.TextMessageFile.Text200 =
          TrimEnd(local.TextMessageFile.Text200) + ",";

        // -- NCP Last Name
        if (Find(local.Ncp.LastName, ",") == 0)
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.Ncp.LastName;
        }
        else
        {
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + local.Ncp.LastName;
          local.TextMessageFile.Text200 =
            TrimEnd(local.TextMessageFile.Text200) + "\"";
        }

        // -- Determine the record length being passed to the external.
        //    This is need in the external IF varying length records are to be 
        // produced.
        local.RecordLength.Count =
          Length(TrimEnd(local.TextMessageFile.Text200));
        local.EabFileHandling.Action = "WRITE";
        UseFnB796WriteToTextMsgFile1();

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

        // -- Increment the text message count.
        ++local.TotalNumbOfTextMsgs.Count;
      }

      local.Summary.CheckIndex();

      // -- Increment the NCP count.
      ++local.TotalNumbOfNcpsWritten.Count;
      ++local.NumbOfNcpsSinceChckpnt.Count;

      // -- Create a HIST record on each CASE indicating the postcard was sent.
      foreach(var item1 in ReadCase())
      {
        local.Infrastructure.EventId = 48;
        local.Infrastructure.ReasonCode = "REMINDTEXTMSG";
        local.Infrastructure.Detail = "NCP Payment Reminder Text Message Sent";
        local.Infrastructure.ProcessStatus = "Q";
        local.Infrastructure.BusinessObjectCd = "CAS";
        local.Infrastructure.CsePersonNumber = entities.Ncp.Number;
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
        UseFnB796WriteToTextMsgFile2();

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
        // 	012-020   Total Number of NCPs to Whom Text Messages Have been Sent
        // 	021-021   Blank
        // 	022-030   Total Number of Text Messages Written to File
        // -------------------------------------------------------------------------------------
        local.ProgramCheckpointRestart.RestartInd = "Y";
        local.ProgramCheckpointRestart.RestartInfo = entities.Ncp.Number;
        local.ProgramCheckpointRestart.RestartInfo = entities.Ncp.Number + " " +
          NumberToString(local.TotalNumbOfNcpsWritten.Count, 7, 9);
        local.ProgramCheckpointRestart.RestartInfo =
          TrimEnd(local.ProgramCheckpointRestart.RestartInfo) + " " + NumberToString
          (local.TotalNumbOfTextMsgs.Count, 7, 9);
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

ReadEach:
      ;
    }

    // -------------------------------------------------------------------------------------
    // --  Write Totals to the Control Report.
    // -------------------------------------------------------------------------------------
    for(local.Common.Count = 1; local.Common.Count <= 5; ++local.Common.Count)
    {
      switch(local.Common.Count)
      {
        case 1:
          local.EabReportSend.RptDetail =
            "Number of NCPs to Whom Text Messages Sent..........................." +
            NumberToString(local.TotalNumbOfNcpsWritten.Count, 9, 7);

          break;
        case 3:
          local.EabReportSend.RptDetail =
            "Number of Text Messages Written to the Output File.................." +
            NumberToString(local.TotalNumbOfTextMsgs.Count, 9, 7);

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
    UseFnB796WriteToTextMsgFile2();

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
    UseFnB796WriteToTextMsgFile2();

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
    local.Ncp.Number = "CLOSE";
    UseEabReadCsePersonBatch();
    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveEabConvertNumeric2(EabConvertNumeric2 source,
    EabConvertNumeric2 target)
  {
    target.SendAmount = source.SendAmount;
    target.SendNonSuppressPos = source.SendNonSuppressPos;
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExport1ToLocal1(FnDisplayObligationsByPayor.Export.
    ExportGroup source, Local.LocalGroup target)
  {
    target.DetailMonthlyDue.TotalCurrency =
      source.DetailMonthlyDue.TotalCurrency;
    target.DetailDark.Flag = source.DetailDark.Flag;
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailLegalAction.Assign(source.DetailLegalAction);
    target.DetailObligationType.Assign(source.DetailObligationType);
    target.DetailAcNonAc.SelectChar = source.DetailAcNonAc.SelectChar;
    target.DetailDebtDetailStatusHistory.Code =
      source.DetailDebtDetailStatusHistory.Code;
    target.GlocalPriSecAndIntrstInd.State =
      source.GexportPriSecAndIntrstInd.State;
    MoveObligationTransaction(source.DetailObligationTransaction,
      target.DetailObligationTransaction);
    target.DetailServiceProvider.UserId = source.DetailServiceProvider.UserId;
    target.DetailMultipleSp.SelectChar = source.DetailMultipleSp.SelectChar;
    target.DetailCurrentOwed.TotalCurrency =
      source.DetailCurrentOwed.TotalCurrency;
    MoveObligationPaymentSchedule(source.DetailObligationPaymentSchedule,
      target.DetailObligationPaymentSchedule);
    target.DetailArrearsOwed.TotalCurrency =
      source.DetailArrearsOwed.TotalCurrency;
    target.DetailIntrestDue.TotalCurrency =
      source.DetailIntrestDue.TotalCurrency;
    target.DetailTotalDue.TotalCurrency = source.DetailTotalDue.TotalCurrency;
    MoveObligation(source.DetailObligation, target.DetailObligation);
    target.DetailConcatInds.Text8 = source.DetailConcatInds.Text8;
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
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

  private void UseEabConvertNumeric1()
  {
    var useImport = new EabConvertNumeric1.Import();
    var useExport = new EabConvertNumeric1.Export();

    MoveEabConvertNumeric2(local.EabConvertNumeric, useImport.EabConvertNumeric);
      
    useExport.EabConvertNumeric.ReturnCurrencySigned =
      local.EabConvertNumeric.ReturnCurrencySigned;

    Call(EabConvertNumeric1.Execute, useImport, useExport);

    local.EabConvertNumeric.ReturnCurrencySigned =
      useExport.EabConvertNumeric.ReturnCurrencySigned;
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

    useImport.CsePersonsWorkSet.Number = local.Ncp.Number;

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);
  }

  private void UseFnB796WriteToTextMsgFile1()
  {
    var useImport = new FnB796WriteToTextMsgFile.Import();
    var useExport = new FnB796WriteToTextMsgFile.Export();

    useImport.WorkArea.Text200 = local.TextMessageFile.Text200;
    useImport.RecordLength.Count = local.RecordLength.Count;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB796WriteToTextMsgFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB796WriteToTextMsgFile2()
  {
    var useImport = new FnB796WriteToTextMsgFile.Import();
    var useExport = new FnB796WriteToTextMsgFile.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(FnB796WriteToTextMsgFile.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnDisplayObligationsByPayor()
  {
    var useImport = new FnDisplayObligationsByPayor.Import();
    var useExport = new FnDisplayObligationsByPayor.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = local.Ncp.Number;

    Call(FnDisplayObligationsByPayor.Execute, useImport, useExport);

    useExport.Export1.CopyTo(local.Local1, MoveExport1ToLocal1);
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

    useImport.CsePersonsWorkSet.Number = local.Ncp.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Ncp.Assign(useExport.CsePersonsWorkSet);
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
        db.SetString(command, "cspNumber", entities.Ncp.Number);
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
        db.SetNullableString(command, "oblgorPrsnNbr", entities.Ncp.Number);
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
        db.SetString(command, "numb", entities.Ncp.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 3);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 4);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 5);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 6);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonObligor()
  {
    entities.Obligor.Populated = false;
    entities.Ncp.Populated = false;

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
        entities.Ncp.Number = db.GetString(reader, 0);
        entities.Obligor.CspNumber = db.GetString(reader, 1);
        entities.Obligor.Type1 = db.GetString(reader, 2);
        entities.Obligor.Populated = true;
        entities.Ncp.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);

        return true;
      });
  }

  private bool ReadFipsLegalAction1()
  {
    entities.Fips.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadFipsLegalAction1",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          local.Summary.Item.SummaryAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.Fips.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadFipsLegalAction2()
  {
    entities.Fips.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadFipsLegalAction2",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          local.Summary.Item.SummaryAction.Identifier);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 4);
        entities.LegalAction.Identifier = db.GetInt32(reader, 5);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 8);
        entities.Fips.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 3);
        entities.InterstateRequest.Populated = true;
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
    /// <summary>A SummaryGroup group.</summary>
    [Serializable]
    public class SummaryGroup
    {
      /// <summary>
      /// A value of SummaryTotalDue.
      /// </summary>
      [JsonPropertyName("summaryTotalDue")]
      public Common SummaryTotalDue
      {
        get => summaryTotalDue ??= new();
        set => summaryTotalDue = value;
      }

      /// <summary>
      /// A value of SummaryArrearsOwed.
      /// </summary>
      [JsonPropertyName("summaryArrearsOwed")]
      public Common SummaryArrearsOwed
      {
        get => summaryArrearsOwed ??= new();
        set => summaryArrearsOwed = value;
      }

      /// <summary>
      /// A value of SummaryCurrentOwed.
      /// </summary>
      [JsonPropertyName("summaryCurrentOwed")]
      public Common SummaryCurrentOwed
      {
        get => summaryCurrentOwed ??= new();
        set => summaryCurrentOwed = value;
      }

      /// <summary>
      /// A value of SummaryAction.
      /// </summary>
      [JsonPropertyName("summaryAction")]
      public LegalAction SummaryAction
      {
        get => summaryAction ??= new();
        set => summaryAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common summaryTotalDue;
      private Common summaryArrearsOwed;
      private Common summaryCurrentOwed;
      private LegalAction summaryAction;
    }

    /// <summary>A LocalGroup group.</summary>
    [Serializable]
    public class LocalGroup
    {
      /// <summary>
      /// A value of DetailMonthlyDue.
      /// </summary>
      [JsonPropertyName("detailMonthlyDue")]
      public Common DetailMonthlyDue
      {
        get => detailMonthlyDue ??= new();
        set => detailMonthlyDue = value;
      }

      /// <summary>
      /// A value of DetailDark.
      /// </summary>
      [JsonPropertyName("detailDark")]
      public Common DetailDark
      {
        get => detailDark ??= new();
        set => detailDark = value;
      }

      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailLegalAction.
      /// </summary>
      [JsonPropertyName("detailLegalAction")]
      public LegalAction DetailLegalAction
      {
        get => detailLegalAction ??= new();
        set => detailLegalAction = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailAcNonAc.
      /// </summary>
      [JsonPropertyName("detailAcNonAc")]
      public Common DetailAcNonAc
      {
        get => detailAcNonAc ??= new();
        set => detailAcNonAc = value;
      }

      /// <summary>
      /// A value of DetailDebtDetailStatusHistory.
      /// </summary>
      [JsonPropertyName("detailDebtDetailStatusHistory")]
      public DebtDetailStatusHistory DetailDebtDetailStatusHistory
      {
        get => detailDebtDetailStatusHistory ??= new();
        set => detailDebtDetailStatusHistory = value;
      }

      /// <summary>
      /// A value of GlocalPriSecAndIntrstInd.
      /// </summary>
      [JsonPropertyName("glocalPriSecAndIntrstInd")]
      public Common GlocalPriSecAndIntrstInd
      {
        get => glocalPriSecAndIntrstInd ??= new();
        set => glocalPriSecAndIntrstInd = value;
      }

      /// <summary>
      /// A value of DetailObligationTransaction.
      /// </summary>
      [JsonPropertyName("detailObligationTransaction")]
      public ObligationTransaction DetailObligationTransaction
      {
        get => detailObligationTransaction ??= new();
        set => detailObligationTransaction = value;
      }

      /// <summary>
      /// A value of DetailServiceProvider.
      /// </summary>
      [JsonPropertyName("detailServiceProvider")]
      public ServiceProvider DetailServiceProvider
      {
        get => detailServiceProvider ??= new();
        set => detailServiceProvider = value;
      }

      /// <summary>
      /// A value of DetailMultipleSp.
      /// </summary>
      [JsonPropertyName("detailMultipleSp")]
      public Common DetailMultipleSp
      {
        get => detailMultipleSp ??= new();
        set => detailMultipleSp = value;
      }

      /// <summary>
      /// A value of DetailCurrentOwed.
      /// </summary>
      [JsonPropertyName("detailCurrentOwed")]
      public Common DetailCurrentOwed
      {
        get => detailCurrentOwed ??= new();
        set => detailCurrentOwed = value;
      }

      /// <summary>
      /// A value of DetailObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("detailObligationPaymentSchedule")]
      public ObligationPaymentSchedule DetailObligationPaymentSchedule
      {
        get => detailObligationPaymentSchedule ??= new();
        set => detailObligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of DetailArrearsOwed.
      /// </summary>
      [JsonPropertyName("detailArrearsOwed")]
      public Common DetailArrearsOwed
      {
        get => detailArrearsOwed ??= new();
        set => detailArrearsOwed = value;
      }

      /// <summary>
      /// A value of DetailIntrestDue.
      /// </summary>
      [JsonPropertyName("detailIntrestDue")]
      public Common DetailIntrestDue
      {
        get => detailIntrestDue ??= new();
        set => detailIntrestDue = value;
      }

      /// <summary>
      /// A value of DetailTotalDue.
      /// </summary>
      [JsonPropertyName("detailTotalDue")]
      public Common DetailTotalDue
      {
        get => detailTotalDue ??= new();
        set => detailTotalDue = value;
      }

      /// <summary>
      /// A value of DetailObligation.
      /// </summary>
      [JsonPropertyName("detailObligation")]
      public Obligation DetailObligation
      {
        get => detailObligation ??= new();
        set => detailObligation = value;
      }

      /// <summary>
      /// A value of DetailConcatInds.
      /// </summary>
      [JsonPropertyName("detailConcatInds")]
      public TextWorkArea DetailConcatInds
      {
        get => detailConcatInds ??= new();
        set => detailConcatInds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private Common detailMonthlyDue;
      private Common detailDark;
      private Common detailCommon;
      private LegalAction detailLegalAction;
      private ObligationType detailObligationType;
      private Common detailAcNonAc;
      private DebtDetailStatusHistory detailDebtDetailStatusHistory;
      private Common glocalPriSecAndIntrstInd;
      private ObligationTransaction detailObligationTransaction;
      private ServiceProvider detailServiceProvider;
      private Common detailMultipleSp;
      private Common detailCurrentOwed;
      private ObligationPaymentSchedule detailObligationPaymentSchedule;
      private Common detailArrearsOwed;
      private Common detailIntrestDue;
      private Common detailTotalDue;
      private Obligation detailObligation;
      private TextWorkArea detailConcatInds;
    }

    /// <summary>
    /// A value of LogExclusions.
    /// </summary>
    [JsonPropertyName("logExclusions")]
    public Common LogExclusions
    {
      get => logExclusions ??= new();
      set => logExclusions = value;
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
    /// A value of TextMessageFile.
    /// </summary>
    [JsonPropertyName("textMessageFile")]
    public WorkArea TextMessageFile
    {
      get => textMessageFile ??= new();
      set => textMessageFile = value;
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
    /// A value of County.
    /// </summary>
    [JsonPropertyName("county")]
    public WorkArea County
    {
      get => county ??= new();
      set => county = value;
    }

    /// <summary>
    /// Gets a value of Summary.
    /// </summary>
    [JsonIgnore]
    public Array<SummaryGroup> Summary => summary ??= new(
      SummaryGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Summary for json serialization.
    /// </summary>
    [JsonPropertyName("summary")]
    [Computed]
    public IList<SummaryGroup> Summary_Json
    {
      get => summary;
      set => Summary.Assign(value);
    }

    /// <summary>
    /// Gets a value of Local1.
    /// </summary>
    [JsonIgnore]
    public Array<LocalGroup> Local1 => local1 ??= new(LocalGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Local1 for json serialization.
    /// </summary>
    [JsonPropertyName("local1")]
    [Computed]
    public IList<LocalGroup> Local1_Json
    {
      get => local1;
      set => Local1.Assign(value);
    }

    /// <summary>
    /// A value of TotalNumbOfTextMsgs.
    /// </summary>
    [JsonPropertyName("totalNumbOfTextMsgs")]
    public Common TotalNumbOfTextMsgs
    {
      get => totalNumbOfTextMsgs ??= new();
      set => totalNumbOfTextMsgs = value;
    }

    /// <summary>
    /// A value of CurrentSupportMonth.
    /// </summary>
    [JsonPropertyName("currentSupportMonth")]
    public WorkArea CurrentSupportMonth
    {
      get => currentSupportMonth ??= new();
      set => currentSupportMonth = value;
    }

    /// <summary>
    /// A value of ArrearsThroughMonth.
    /// </summary>
    [JsonPropertyName("arrearsThroughMonth")]
    public WorkArea ArrearsThroughMonth
    {
      get => arrearsThroughMonth ??= new();
      set => arrearsThroughMonth = value;
    }

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
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePersonsWorkSet Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
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

    private Common logExclusions;
    private WorkArea workArea;
    private WorkArea textMessageFile;
    private EabConvertNumeric2 eabConvertNumeric;
    private WorkArea county;
    private Array<SummaryGroup> summary;
    private Array<LocalGroup> local1;
    private Common totalNumbOfTextMsgs;
    private WorkArea currentSupportMonth;
    private WorkArea arrearsThroughMonth;
    private Common numbOfNcpsSinceChckpnt;
    private CsePerson restartNcp;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Infrastructure nullInfrastructure;
    private Infrastructure infrastructure;
    private Common recordLength;
    private TextWorkArea textDate;
    private Common numberOfNonPaymentDays;
    private DateWorkArea paymentCutoffDate;
    private CsePersonsWorkSet ncp;
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
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
    }

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
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

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
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    public CsePerson Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
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

    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private CaseRole caseRole;
    private Case1 case1;
    private CashReceiptDetail cashReceiptDetail;
    private CsePerson csePerson;
    private Obligation obligation;
    private CsePersonAccount supported1;
    private CsePersonAccount obligor;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
    private CsePerson ncp;
    private CsePerson supported2;
  }
#endregion
}
